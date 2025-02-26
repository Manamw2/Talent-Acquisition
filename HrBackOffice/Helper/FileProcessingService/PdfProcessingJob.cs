using HrBackOffice.Helper.ApplicantService;
using HrBackOffice.Hubs;
using HrBackOffice.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Models;
using Models.ViewModels;
using System.Globalization;
using System.IO;
using System.Text.Encodings.Web;
using System.Text;
using System.Security.Cryptography;
using HrBackOffice.Helper.EmailSetting;
using static System.Net.WebRequestMethods;
using DataAccess.Data;


namespace HrBackOffice.Helper.FileProcessingService
{
    public class PdfProcessingJob
    {
        private readonly IHubContext<ProcessingHub> _hubContext;
        private static readonly ProcessingStatus _status = new();
        private readonly IApplicantService _applicantService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailSend _emailSender;
        private readonly FileStorageService _fileStorage;
        private static readonly object _statusLock = new(); // Lock object for thread safety

        public PdfProcessingJob(IHubContext<ProcessingHub> hubContext, IApplicantService applicantService, UserManager<AppUser> userManager, IConfiguration configuration, IEmailSend emailSender, FileStorageService fileStorage)
        {
            _hubContext = hubContext;
            _applicantService = applicantService;
            _userManager = userManager;
            _configuration = configuration;
            _emailSender = emailSender;
            _fileStorage = fileStorage;
        }

        public async Task ProcessFiles(List<string> files, string jobId)
        {
            lock (_statusLock) // Ensure thread-safe updates
            {
                _status.TotalFiles = files.Count;
                _status.ProcessedFiles = 0;
                _status.SuccessfulFiles = 0;
                _status.FailedFiles = 0;
                _status.ProcessingErrors.Clear();
            }
            await UpdateStatus();

            foreach (var file in files)
            {
                try
                {
                    await ProcessSingleFile(file);
                    _status.SuccessfulFiles++;
                }
                catch (Exception ex)
                {
                    _status.FailedFiles++;
                    _status.ProcessingErrors.Add($"Error processing {file}: {ex.Message}");
                }
                finally
                {
                    _status.ProcessedFiles++;
                    await UpdateStatus();
                }
            }
        }

        private async Task ProcessSingleFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            string fileHash;

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    using (var md5 = MD5.Create())
                    {
                        byte[] hash = await md5.ComputeHashAsync(stream);
                        fileHash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
                } // The FileStream is disposed here

                if (await IsCvExists(fileHash))
                {
                    // Delete the duplicate file from upload folder
                    throw new ArgumentException("This CV already exists in our system.");
                }

                // Reopen the file for further processing
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    IFormFile file = new FormFile(stream, 0, stream.Length, "name", fileName)
                    {
                        Headers = new HeaderDictionary(),
                        ContentType = "application/pdf" // Set the appropriate content type
                    };

                    UserViewModel model = await _applicantService.ExtractDataFromCv(file);

                    if (!IsValidEmail(model.Email))
                    {
                        throw new ArgumentException("The provided email is not in a valid format.");
                    }

                    var appUser = new AppUser();

                    appUser.DisplayName = model.DisplayName;
                    appUser.Email = model.Email;
                    appUser.University = "Others";
                    appUser.Faculty = "Others";
                    appUser.BirthDate = new DateOnly(year: 2000, month: 1, day: 1);
                    appUser.PhoneNumber = model.Phone ?? "";
                    appUser.EducationLevel = model.EducationLevel ?? "Beginner";
                    appUser.EnglishLevel = model.EnglishProficiencyLevel ?? "Beginner";
                    appUser.MethodOfContact = model.MethodOfContact ?? "Email";

                    appUser.ApplicantExperiences = model.ApplicantExperiences.Select(exp => new ApplicantExperience
                    {
                        AppUserId = appUser.Id,
                        Company = exp.Company ?? "",
                        Position = exp.Position ?? "",
                        StartDate = exp.StartDate,
                        EndDate = exp.EndDate,
                        Description = exp.Description ?? ""
                    }).ToList();

                    appUser.ApplicantSkills = model.ApplicantSkills.Select(skill => new ApplicantSkill
                    {
                        AppUserId = appUser.Id,
                        Name = skill.Name ?? "",
                        Level = skill.Level ?? ""
                    }).ToList();

                    appUser.ApplicantProjects = model.ApplicantProjects.Select(project => new ApplicantProject
                    {
                        AppUserId = appUser.Id,
                        Name = project.Name ?? "",
                        Description = project.Description ?? ""
                    }).ToList();

                    appUser.UserName = model.Email;
                    appUser.Email = model.Email;
                    appUser.EmailConfirmed = false;
                    // Single database update
                    var result = await _userManager.CreateAsync(appUser, GenerateRandomPassword(8));
                    if (result.Succeeded)
                    {
                        //add to shared cvs
                        var name = Path.GetRandomFileName() + Path.GetExtension(file.FileName); // Safer filename
                        string sharedFolderPath = _fileStorage.GetSharedCsvFolderPath();
                        string path = Path.Combine(sharedFolderPath, name);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        appUser.CvUrl = path;

                        result = await _userManager.AddToRoleAsync(appUser, "applicant");

                        var code = await _userManager.GeneratePasswordResetTokenAsync(appUser);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                        var careerBaseUrl = _configuration["CareerBaseUrl"]; // Read from appsettings.json

                        var resetLink = $"{careerBaseUrl}/Identity/Account/ResetPassword?code={Uri.EscapeDataString(code)}";

                        string emailMessage = $"Welcome {model.DisplayName}," +
                            $" <p>Your account has been created on our careers platform.</p>" +
                            $"    </ul> <br/> Click <a href='{HtmlEncoder.Default.Encode(resetLink)}'>{HtmlEncoder.Default.Encode(resetLink)}</a> to reset your password.";

                        //await _emailSender.SendEmailAsync(model.Email, "Welcome to Our Career Portal", emailMessage);
                    }
                    else
                    {
                        throw new ApplicationException("User creation failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                } // The FileStream is disposed here
            }
            catch (Exception ex)
            {
                // Delete the file in case of any exception
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                throw; // Re-throw the exception to propagate it further
            }

            // Delete the file after successful processing
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        private async Task UpdateStatus()
        {
            await _hubContext.Clients.All.SendAsync("ReceiveProcessingUpdate", _status);
        }

        private static string GenerateRandomPassword(int length)
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()-_=+";
            StringBuilder password = new StringBuilder();
            byte[] randomBytes = new byte[length];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            foreach (byte b in randomBytes)
            {
                password.Append(validChars[b % validChars.Length]);
            }

            return password.ToString();
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> IsCvExists(string fileHash)
        {
            // Check if file with same hash exists in shared folder
            string sharedFolderPath = _fileStorage.GetSharedCsvFolderPath();
            foreach (string existingFile in Directory.GetFiles(sharedFolderPath))
            {
                using var stream = new FileStream(existingFile, FileMode.Open);
                using (var md5 = MD5.Create())
                {
                    byte[] hash = await md5.ComputeHashAsync(stream);
                    string existingHash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

                    if (existingHash == fileHash)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
