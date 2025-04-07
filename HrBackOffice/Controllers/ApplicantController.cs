using AutoMapper;
using DataAccess.Data;
using DataAccess.Repository.IRepository;
using HrBackOffice.Helper.ApplicantService;
using HrBackOffice.Helper.EmailSetting;
using HrBackOffice.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Text;
using System.Text.Encodings.Web;
using X.PagedList.Extensions;

namespace HrBackOffice.Controllers
{
    public class ApplicantController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSend _emailSender;
        private readonly HttpClient _httpClient;
        private readonly IApplicantService _AppSevice;
        private readonly IConfiguration _configuration;
        private readonly FileStorageService _fileStorage;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ApplicantController> _logger;

        public ApplicantController(FileStorageService fileStorage,
            IConfiguration configuration,
            IApplicantService applicantService,
            HttpClient httpClient,IEmailSend emailSender,
            IUnitOfWork unitOfWork, IMapper mapper,
            ApplicationDbContext context,
            UserManager<AppUser> userManager, ILogger<ApplicantController> logger, RoleManager<IdentityRole> roleManager)
        {
            _emailSender = emailSender;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpClient = httpClient;
            _AppSevice = applicantService;
            _configuration = configuration;
            _fileStorage = fileStorage;
            _logger = logger;
            _context = context;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(int page = 1, string searchQuery = null)
        {
            try
            {
                int pageSize = 5;
                var totalApplicants = 0;
                var paginatedApplicants = new List<UserViewModel>();

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            var link = _configuration["Search"];
                            if (string.IsNullOrEmpty(link))
                            {
                                throw new InvalidOperationException("Search API URL is not configured");
                            }

                            var response = await client.GetAsync($"{link}query={Uri.EscapeDataString(searchQuery)}&max_results=20&exact_thresh=0.9&nonexact_thresh=0.5");

                            if (response.IsSuccessStatusCode)
                            {
                                var searchResult = await response.Content.ReadFromJsonAsync<SearchResult>();
                                if (searchResult?.Results == null)
                                {
                                    throw new InvalidOperationException("Invalid search result format");
                                }

                                var matchedUserIds = searchResult.Results.Select(r => r.Id).ToList();
                                totalApplicants = matchedUserIds.Count;

                                // Only get the IDs needed for the current page
                                var pageUserIds = matchedUserIds
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToList();

                                foreach (var userId in pageUserIds)
                                {
                                    try
                                    {
                                        var user = await _userManager.FindByIdAsync(userId);
                                        if (user != null && await _userManager.IsInRoleAsync(user, "Applicant"))
                                        {
                                            paginatedApplicants.Add(new UserViewModel
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
                                    catch (Exception ex) when (ex is ArgumentNullException || ex is InvalidOperationException)
                                    {
                                        _logger.LogError($"Error processing user {userId}: {ex.Message}");
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                throw new HttpRequestException($"Search API returned status code: {response.StatusCode}");
                            }
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        _logger.LogError($"Error calling search API: {ex.Message}");
                        // Fallback to database pagination
                        (paginatedApplicants, totalApplicants) = await LoadPaginatedApplicants(page, pageSize);
                    }
                }
                else
                {
                    // Use database pagination
                    (paginatedApplicants, totalApplicants) = await LoadPaginatedApplicants(page, pageSize);
                }

                var totalPages = (int)Math.Ceiling(totalApplicants / (double)pageSize);

                ViewBag.SearchQuery = searchQuery;
                ViewBag.TotalItems = totalApplicants;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                return View(paginatedApplicants);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unhandled error in Index action: {ex.Message}");
                return View("Error");
            }
        }

        // Optimized helper method to load only the applicants for current page
        private async Task<(List<UserViewModel>, int)> LoadPaginatedApplicants(int page, int pageSize)
        {
            try
            {
                // Get applicant users from database with pagination
                var applicantRole = await _roleManager.FindByNameAsync("Applicant");
                if (applicantRole == null)
                {
                    return (new List<UserViewModel>(), 0);
                }

                // Get total count for pagination
                var totalCount = await _context.UserRoles
                    .Where(ur => ur.RoleId == applicantRole.Id)
                    .CountAsync();

                // Get just the users for the current page
                var userIdsForCurrentPage = await _context.UserRoles
                    .Where(ur => ur.RoleId == applicantRole.Id)
                    .Select(ur => ur.UserId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var paginatedUsers = new List<UserViewModel>();
                foreach (var userId in userIdsForCurrentPage)
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        paginatedUsers.Add(new UserViewModel
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

                return (paginatedUsers, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading paginated applicants: {ex.Message}`");
                throw;
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
                CvUrl = model.CvUrl,
                EmailConfirmed = true


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
        #region GetAvailableJobs
//[HttpGet]
//        public async Task<IActionResult> GetAvailableJobs(string userId)
//        {
//            if (string.IsNullOrEmpty(userId))
//                return BadRequest("User ID is required");
//            // First, get all job applications for this user
//            var applications = await _unitOfWork.JobApplicationRepository
//                .GetAllAsync(ja => ja.UserId == userId);
//            // Then extract the JobIds
//            var appliedJobIds = applications.Select(ja => ja.JobId);
//            // Now get jobs that are not in the applied list
//            var availableJobs = await _unitOfWork.JobRepository
//                .GetAllAsync(filter: 
//                job => job.Batch != null && job.Batch.EndDate > DateTime.UtcNow &&!appliedJobIds.Contains(job.JobId));
//            return Json(availableJobs.Select(j => new { id = j.JobId, title = j.Title }));
//        }
        #endregion
        [HttpGet]
        public async Task<IActionResult> GetAvailableJobs(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID is required");

            // Get all job applications for the user
            var applications = await _unitOfWork.JobApplicationRepository
                .GetAllAsync(ja => ja.UserId == userId, includeProperties: "Batch");

            // Extract job IDs from the related batches
            var appliedJobIds = applications
                .Where(ja => ja.Batch?.JobId != null)
                .Select(ja => ja.Batch.JobId.Value)
                .Distinct()
                .ToList();

            // Get jobs with valid batches that the user has not applied to
            var availableJobs = await _unitOfWork.JobRepository.GetAllAsync(
                filter: job => job.Batch != null &&
                               job.Batch.EndDate > DateTime.UtcNow &&
                               !appliedJobIds.Contains(job.JobId),
                includeProperties: "Batch"
            );

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

            var job = await _unitOfWork.JobRepository
                .GetFirstOrDefaultAsync(j => j.JobId == model.JobId, includeProperties: "Batch");
            if (job == null)
                return NotFound("Job not found");

            // Get the latest or active batch for this job
            var batch = await _unitOfWork.BatchRepository
                .GetFirstOrDefaultAsync(b => b.JobId == model.JobId && b.EndDate > DateTime.UtcNow);

            if (batch == null)
                return BadRequest("No available batch for this job");

            // Check for duplicate application based on BatchId
            var existingApplication = await _unitOfWork.JobApplicationRepository.GetFirstOrDefaultAsync(
                a => a.UserId == model.UserId && a.BatchId == batch.BatchId);
            if (existingApplication != null)
                return BadRequest("This applicant has already applied to this job's batch");

            // Create new job application (linked to batch)
            var jobApplication = new JobApplication
            {
                UserId = model.UserId,
                BatchId = batch.BatchId,
                AppliedDate = DateTime.UtcNow,
                Status = "HR Added",
                Source = model.Reason,
                SourceDetails = model.Reason == "Internal referral" ? $"Referred by: {model.ReferralName}" : null,
                AddedBy = User.Identity?.Name
            };

            await _unitOfWork.JobApplicationRepository.AddAsync(jobApplication);
            await _unitOfWork.SaveAsync();

            return Ok(new { message = "Job application successfully added" });
        }


        [HttpPost]
        public async Task<IActionResult> RecommendJobToApplicant([FromBody] AssignJobViewModel model)
        {
            if (string.IsNullOrEmpty(model.UserId) || model.JobId == 0)
                return BadRequest("Invalid data");

            var job = await _unitOfWork.JobRepository
                .GetFirstOrDefaultAsync(j => j.JobId == model.JobId, includeProperties: "Batch");

            if (job == null)
                return NotFound("Job not found");

            // Optional: Check for an active batch for this job
            var hasActiveBatch = job.Batch != null && job.Batch.EndDate > DateTime.UtcNow;
            if (!hasActiveBatch)
                return BadRequest("This job has no active batch currently available");

            var applicant = await _userManager.FindByIdAsync(model.UserId);
            if (applicant == null)
                return NotFound("Applicant not found");

            var existingRecommendation = await _unitOfWork.JobRecommendRepository.GetFirstOrDefaultAsync(
                r => r.UserId == model.UserId && r.JobId == model.JobId);
            if (existingRecommendation != null)
                return BadRequest("This applicant has already been recommended for this job");

            var jobRecommendation = new JobRecommend
            {
                UserId = model.UserId,
                JobId = model.JobId,
                Date = DateTime.UtcNow
            };

            await _unitOfWork.JobRecommendRepository.AddAsync(jobRecommendation);
            await _unitOfWork.SaveAsync();

            // Send email
            string applyLink = _configuration["JobsLink"]; // You may customize this with query params if needed

            await _emailSender.SendEmailAsync(
                applicant.Email,
                "Job Recommendation",
                $@"<div style='font-family: Arial, sans-serif; line-height: 1.6; padding: 20px;'>
    <p>Dear {applicant.DisplayName},</p>

    <p>Here is the most suitable job based on your profile.</p>

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

        [HttpGet]
        public async Task<IActionResult> GetApplicationCount(string id)
        {
            var count = await _context.JobApplications
                .Where(ja => ja.UserId == id)
                .CountAsync();
            return Json(count);
        }

        [HttpGet]
        public async Task<IActionResult> GetApplicationDetails(string id)
        {
            var applications = await _context.JobApplications
                .Where(ja => ja.UserId == id)
                .Include(ja => ja.Batch)
                .Select(ja => new
                {
                    jobTitle = ja.Batch.Job.Title,
                    applicationDate = ja.AppliedDate,
                    status = ja.Status
                })
                .ToListAsync();
            return Json(applications);
        }
    }
}
