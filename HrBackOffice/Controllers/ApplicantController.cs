using AutoMapper;
using DataAccess.Migrations;
using DataAccess.Repository.IRepository;
using HrBackOffice.Helper.EmailSetting;
using HrBackOffice.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Mappers;
using Models.ViewModels;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using X.PagedList.Extensions;

namespace HrBackOffice.Controllers
{
    public class ApplicantController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSend _emailSender;
        private readonly HttpClient _httpClient;

        public ApplicantController(HttpClient httpClient,IEmailSend emailSender,IUnitOfWork unitOfWork, IMapper mapper, UserManager<AppUser> userManager)
        {
            _emailSender = emailSender;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpClient = httpClient;
        }
      

        public async Task<IActionResult> Index(int? page, string searchQuery = null)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;
            var applicants = new List<UserViewModel>();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync($"http://localhost:8000/search?query={Uri.EscapeDataString(searchQuery)}&max_results=5&exact_thresh=0.9&nonexact_thresh=0.5");

                    if (response.IsSuccessStatusCode)
                    {
                        var searchResult = await response.Content.ReadFromJsonAsync<SearchResult>();
                        var matchedUserIds = searchResult.Results.Select(r => r.Id).ToList();

                        // Get only the users that match the IDs from the search results
                        foreach (var userId in matchedUserIds)
                        {
                            var user = await _userManager.FindByIdAsync(userId);
                            if (user != null && await _userManager.IsInRoleAsync(user, "Applicant"))
                            {
                                applicants.Add(new UserViewModel
                                {
                                    Id = user.Id,
                                    UserName = user.UserName,
                                    Email = user.Email,
                                    EducationLevel = user.EducationLevel,
                                    EnglishLevel = user.EnglishLevel,
                                    Roles = (await _userManager.GetRolesAsync(user)).ToList()
                                });
                            }
                        }
                    }
                }
            }
            else
            {
                // Default behavior when no search query
                var users = await _userManager.Users.ToListAsync();
                foreach (var user in users)
                {
                    if (await _userManager.IsInRoleAsync(user, "Applicant"))
                    {
                        applicants.Add(new UserViewModel
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            Email = user.Email,
                            EducationLevel = user.EducationLevel,
                            EnglishLevel = user.EnglishLevel,
                            Roles = (await _userManager.GetRolesAsync(user)).ToList()
                        });
                    }
                }
            }

            var pagedApplicant = applicants.ToPagedList(pageNumber, pageSize);
            return View(pagedApplicant);
        }
        public async Task<UserViewModel> ExtractDataFromCv(IFormFile cvFile)
        {
            var content = new MultipartFormDataContent();
            // Read the file content from cvFile into a MemoryStream
            using (var memoryStream = new MemoryStream())
            {
                await cvFile.CopyToAsync(memoryStream); // Copy the file content to the MemoryStream
                memoryStream.Position = 0; // Reset the stream position to the beginning

                // Create ByteArrayContent from the MemoryStream
                var fileContentStream = new ByteArrayContent(memoryStream.ToArray());

                // Set the Content-Disposition header
                fileContentStream.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                {
                    Name = "file", // Name of the form field
                    FileName = cvFile.FileName // Name of the file
                };

                // Set the Content-Type header based on the file type
                fileContentStream.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(cvFile.ContentType);

                // Add the file content to the MultipartFormDataContent
                content.Add(fileContentStream);

                // Make the API call
                var response = await _httpClient.PostAsync("http://127.0.0.1:8000/parse-resume", content);
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();
                ResumeViewModel resumeViewModel = JsonSerializer.Deserialize<ResumeViewModel>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                var userViewModel = resumeViewModel.HRResumeToProfileInfo();
                return userViewModel;
            }
        }

        public IActionResult AddApplicant()
        {
            var model = new UserViewModel
            {
                ApplicantExperiences = new List<AppExperienceViewModel>(), // Ensure list is not null
                ApplicantSkills = new List<AppSkillViewModel>(),
                ApplicantProjects = new List<AppProjectViewModel>()
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> AddApplicant(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var user = new AppUser
            {
                UserName = model.UserName,
                DisplayName = model.DisplayName,
                Email = model.Email,
                PhoneNumber = model.Phone,
                EnglishLevel = model.EnglishProficiencyLevel,
                EducationLevel = model.EducationLevel,
                Faculty = model.Faculty,
                MethodOfContact = model.MethodOfContact,
                BirthDate = model.BirthDate,
                CvUrl = model.CvUrl
            };
            
            var result = await _userManager.CreateAsync(user, "Temp@1234"); // Default password
            if (!result.Succeeded)
                return BadRequest(result.Errors);
            foreach (var experience in model.ApplicantExperiences)
            {
                var newExperience = new ApplicantExperience
                {
                    Company = experience.Company,
                    Position = experience.Position,
                    StartDate = experience.StartDate,
                    EndDate = experience.EndDate,
                    Description = experience.Description,
                    AppUserId = user.Id
                };
                await _unitOfWork.AppExpRepository.AddAsync(newExperience);
            }

            // Add Skills
            foreach (var skill in model.ApplicantSkills)
            {
                var newSkill = new ApplicantSkill
                {
                    Name = skill.Name,
                    Level = skill.Level,
                    AppUserId = user.Id
                };
                await _unitOfWork.AppSkillRepository.AddAsync(newSkill);
            }

            // Add Projects
            foreach (var project in model.ApplicantProjects)
            {
                var newProject = new ApplicantProject
                {
                    Name = project.Name,
                    Description = project.Description,
                    AppUserId = user.Id
                };
                await _unitOfWork.AppProjRepository.AddAsync(newProject);
            }

            await _unitOfWork.SaveAsync();
            await _userManager.AddToRoleAsync(user, "Applicant");


            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            
            string careerBaseUrl = "https://localhost:7142";
            

            var resetLink = $"{careerBaseUrl}/Identity/Account/ResetPassword?code={Uri.EscapeDataString(code)}";

            string emailMessage = $"Welcome {model.DisplayName}," +
                $" <p>Your account has been created on our careers platform.</p>" +
                $"    </ul> <br/> Click <a href='{HtmlEncoder.Default.Encode(resetLink)}'>{HtmlEncoder.Default.Encode(resetLink)}</a> to reset your password.";
      
            await _emailSender.SendEmailAsync(model.Email, "Welcome to Our Career Portal", emailMessage);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ExtractDataFromCv(UserViewModel model)
        {
            if (model.CvFile == null || model.CvFile.Length == 0)
            {
                ModelState.AddModelError("CvFile", "Please upload a valid CV.");
                return View("AddApplicant", model);
            }

            // Call API to extract data
            var extractedData = await ExtractDataFromCv(model.CvFile);

            // Populate model with extracted data
            model.UserName = extractedData.UserName;
            model.DisplayName = extractedData.DisplayName;
            model.Email = extractedData.Email;
            model.Phone = extractedData.Phone;
            model.EducationLevel = extractedData.EducationLevel;
            model.EnglishProficiencyLevel = extractedData.EnglishProficiencyLevel;
            model.Faculty = extractedData.Faculty;
            model.BirthDate = extractedData.BirthDate;
            model.MethodOfContact = extractedData.MethodOfContact;
            model.ApplicantExperiences = extractedData.ApplicantExperiences;
            model.ApplicantSkills = extractedData.ApplicantSkills;
            model.ApplicantProjects = extractedData.ApplicantProjects;

            return View("AddApplicant", model);
        }


    }
}
