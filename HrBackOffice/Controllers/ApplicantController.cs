using AutoMapper;
using DataAccess.Data;
using DataAccess.Repository.IRepository;
using HrBackOffice.Helper.ApplicantService;
using HrBackOffice.Helper.EmailSetting;
using HrBackOffice.Models;
using Microsoft.AspNetCore.Identity;
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
        private readonly IApplicantService _AppSevice;
        private readonly IConfiguration _configuration;
        private readonly FileStorageService _fileStorage;
        public ApplicantController(FileStorageService fileStorage,IConfiguration configuration,IApplicantService applicantService,HttpClient httpClient,IEmailSend emailSender,IUnitOfWork unitOfWork, IMapper mapper, UserManager<AppUser> userManager)
        {
            _emailSender = emailSender;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpClient = httpClient;
            _AppSevice= applicantService;
            _configuration = configuration;
            _fileStorage = fileStorage;
        }
      
        public async Task<IActionResult> Index(int page =1, string searchQuery = null)
        {
            int pageSize = 5;
            var applicants = new List<UserViewModel>();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                using (var client = new HttpClient())
                {
                    var link = _configuration["Search"];
                    var response = await client.GetAsync($"{link}query={Uri.EscapeDataString(searchQuery)}&max_results=5&exact_thresh=0.9&nonexact_thresh=0.5");

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
                                    DisplayName = user.DisplayName,
                                    Email = user.Email,
                                    EducationLevel = user.EducationLevel,
                                    EnglishProficiencyLevel = user.EnglishLevel,
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
                            DisplayName = user.DisplayName,
                            Email = user.Email,
                            EducationLevel = user.EducationLevel,
                            EnglishProficiencyLevel = user.EnglishLevel,
                            Roles = (await _userManager.GetRolesAsync(user)).ToList()
                        });
                    }
                }
            }

           // var pagedApplicant = applicants.ToPagedList(pageNumber, pageSize);
           // return View(pagedApplicant);
            var paginatedJobs = applicants.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Calculate total pages
            var totalJobs = applicants.Count();
            var totalPages = (int)Math.Ceiling(totalJobs / (double)pageSize);

            // Pass data to the view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            return View(paginatedJobs);
        }
        

        public IActionResult AddApplicant()
        {
            var model = new UserViewModel
            {
                ApplicantExperiences = new List<AppExperienceViewModel>(), // Ensure list is not null
                ApplicantSkills = new List<AppSkillViewModel>(),
                ApplicantProjects = new List<AppProjectViewModel>()
            };
            _AppSevice.PopulateDropdownLists(model);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> AddApplicant(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                Console.WriteLine(string.Join("\n", errors)); // Check logs for the actual errors
                _AppSevice.PopulateDropdownLists(model);
                return View(model);
            }
            if (model.CvFile != null && model.CvFile.Length > 0)
            {
                try
                {
                    var fileName = Path.GetRandomFileName() + Path.GetExtension(model.CvFile.FileName); // Safer filename
                    string sharedFolderPath = _fileStorage.GetSharedCsvFolderPath();
                    string filePath = Path.Combine(sharedFolderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.CvFile.CopyToAsync(stream);
                    }
                    model.CvUrl = filePath;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"CV upload failed: {ex.Message}");
                  
                }
            }
            var university = model.University == "Other" ? Request.Form["University"].ToString() : model.University;
            var faculty = model.Faculty == "Other" ? Request.Form["Faculty"].ToString() : model.Faculty;
            var username = model.Email.Split('@')[0];
            var user = new AppUser
            {
                UserName = username,
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

            var careerBaseUrl = _configuration["CareerBaseUrl"]; // Read from appsettings.json


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
            _AppSevice.PopulateDropdownLists(model);

            // Call API to extract data
            var extractedData = await _AppSevice.ExtractDataFromCv(model.CvFile);

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

        [HttpPost]
        public async Task<IActionResult> DeleteApplicant(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid applicant ID.");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("Applicant not found.");
            }

            // Delete related data
            var experiences = await _unitOfWork.AppExpRepository.GetAllAsync(e => e.AppUserId == id);
            var skills = await _unitOfWork.AppSkillRepository.GetAllAsync(s => s.AppUserId == id);
            var projects = await _unitOfWork.AppProjRepository.GetAllAsync(p => p.AppUserId == id);

            _unitOfWork.AppExpRepository.RemoveRange(experiences);
            _unitOfWork.AppSkillRepository.RemoveRange(skills);
            _unitOfWork.AppProjRepository.RemoveRange(projects);

            await _unitOfWork.SaveAsync();

            // Remove user from database
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest("Error deleting the applicant.");
            }

            return Ok(new { success = true, message = "Applicant deleted successfully!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableJobs(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID is required");
            // First, get all job applications for this user
            var applications = await _unitOfWork.JobApplicationRepository
                .GetAllAsync(ja => ja.UserId == userId);
            // Then extract the JobIds
            var appliedJobIds = applications.Select(ja => ja.JobId);
            // Now get jobs that are not in the applied list
            var availableJobs = await _unitOfWork.JobRepository
                .GetAllAsync(filter: 
                job => job.Batch != null && job.Batch.EndDate > DateTime.UtcNow &&!appliedJobIds.Contains(job.JobId));
            return Json(availableJobs.Select(j => new { id = j.JobId, title = j.Title }));
        }
        // POST: Assign job to applicant
        [HttpPost]
        public async Task<IActionResult> AssignJob([FromBody] AssignJobViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var applicant = await _userManager.FindByIdAsync(model.UserId);
            if (applicant == null)
                return NotFound("Applicant not found");
            var job = await _unitOfWork.JobRepository.GetFirstOrDefaultAsync(j => j.JobId == model.JobId);
            if (job == null)
                return NotFound("Job not found");
            // Check for duplicate application
            var existingApplication = await _unitOfWork.JobApplicationRepository.GetFirstOrDefaultAsync(
                a => a.UserId == model.UserId && a.JobId == model.JobId);
            if (existingApplication != null)
                return BadRequest("This applicant has already applied for this job");
            // Create new job application
            var jobApplication = new JobApplication
            {
                UserId = model.UserId,
                JobId = model.JobId,
                AppliedDate = DateTime.UtcNow,
                Status = "HR Added",
                Source = model.Reason,
                SourceDetails = model.Reason == "Internal referral" ? $"Referred by: {model.ReferralName}" : null,
                AddedBy = User.Identity.Name
            };
            await _unitOfWork.JobApplicationRepository.AddAsync(jobApplication);
            await _unitOfWork.SaveAsync();
            return Ok(new { message = "Job application successfully added" });
        }

        [HttpPost]
        public async Task<IActionResult> RecommendJobToApplicant([FromBody] AssignJobViewModel model)
        {
            if (string.IsNullOrEmpty(model.UserId) || model.JobId == null)
                return BadRequest("Invalid data");

            var job = await _unitOfWork.JobRepository.GetFirstOrDefaultAsync(j => j.JobId == model.JobId);
            if (job == null)
                return NotFound("Job not found");

            var applicant = await _userManager.FindByIdAsync(model.UserId);
            if (applicant == null)
                return NotFound("applicant not found");

            var existingRecommendation = await _unitOfWork.JobRecommendRepository.GetFirstOrDefaultAsync(
                a => a.UserId == model.UserId && a.JobId == model.JobId);
            if (existingRecommendation != null)
                return BadRequest("This applicant has already Recommended for this job");
            var JobRecommendation = new JobRecommend
            {
                UserId = model.UserId,
                JobId = model.JobId,
                Date = DateTime.UtcNow,
                
            };
            await _unitOfWork.JobRecommendRepository.AddAsync(JobRecommendation);
            await _unitOfWork.SaveAsync();
            // Construct job application link
            string applyLink = _configuration["JobsLink"];

            // Send Email
            // Simple HTML email message
            await _emailSender.SendEmailAsync(
                applicant.Email,
                "Job Recommendation",
                $@"<div style='font-family: Arial, sans-serif; line-height: 1.6; padding: 20px;'>
        <p>Dear {applicant.DisplayName},</p>

        <p>Here is the most suitable job based on your profile.</p>

        <p>If you feel that is not relevant, please update your preferences to send you better jobs in my next email.</p>

        <div style='margin: 20px 0;'>
            <h2 style='color: #2c5282; margin: 0;'>{job.Title}</h2>
            <div style='color: #666; margin: 5px 0;'>{job.JobType}</div>
        </div>

        <div style='margin: 20px 0; white-space: pre-line;'>
            {job.Description}
        </div>

        <p>If you're interested, click below to apply:</p>

        <p style='margin: 20px 0;'>
            <a href='{applyLink}' style='background-color: #4299e1; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                Apply Now
            </a>
        </p>

        <p style='margin-top: 30px; border-top: 1px solid #eee; padding-top: 20px;'>
            Best of luck with your job hunt,<br>
            Soft-trend HR Team
        </p>
    </div>");

            return Ok("Job recommendation sent successfully.");
        }


    }
}
