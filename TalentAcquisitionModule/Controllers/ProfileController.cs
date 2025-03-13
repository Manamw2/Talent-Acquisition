using DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Models;
using Models.ViewModels;
using System.Security.Claims;
using System.Text.Json;
using TalentAcquisitionModule.Services;

namespace TalentAcquisitionModule.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IMemoryCache _memoryCache;
        private readonly FileStorageService _fileStorage;

        public ProfileController(FileStorageService fileStorage, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ApplicationDbContext context, IEmailSender emailSender, IMemoryCache memoryCache)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _memoryCache = memoryCache;
            _emailSender = emailSender;
            _fileStorage = fileStorage;
        }

        [Authorize]
        public async Task<IActionResult> UserInfo()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            AppUser appUser = await _userManager.Users
                .Include(u => u.ApplicantExperiences)
                .Include(u => u.ApplicantSkills)
                .Include(u => u.ApplicantProjects)
                .FirstOrDefaultAsync(u => u.Id == userId);
            ProfileInfoVM profileInfoVM = new ProfileInfoVM
            {
                Email = appUser.Email,
                Name = appUser.DisplayName,
                University = appUser.University,
                Faculty = appUser.Faculty,
                EducationLevel = appUser.EducationLevel,
                EnglishProficiencyLevel = appUser.EnglishLevel,
                Phone = appUser.PhoneNumber,
                DateOfBirth = appUser.BirthDate,
                CvUrl = appUser.CvUrl,
                MethodOfContact = appUser.MethodOfContact,
                Skills = appUser.ApplicantSkills.Select(skill => new SkillViewModel
                {
                    Id = skill.Id,
                    Name = skill.Name,
                    Level = skill.Level
                }).ToList(),
                Experiences = appUser.ApplicantExperiences.Select(experience => new ExperienceViewModel
                {
                    Id = experience.Id,
                    Company = experience.Company,
                    Position = experience.Position,
                    StartDate = experience.StartDate,
                    EndDate = experience.EndDate,
                    Description = experience.Description
                }).ToList(),
                Projects = appUser.ApplicantProjects.Select(project => new ProjectViewModel
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description
                }).ToList()
            };

            ViewBag.Universities = GetUniversities();
            ViewBag.Faculties = GetFaculties();
            return View(profileInfoVM);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UserInfo(ProfileInfoVM profileInfoVM)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Universities = GetUniversities();
                ViewBag.Faculties = GetFaculties();
                return View(profileInfoVM);
            }
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            AppUser appUser = await _userManager.Users
                .Include(u => u.ApplicantExperiences)
                .Include(u => u.ApplicantSkills)
                .Include(u => u.ApplicantProjects)
                .FirstOrDefaultAsync(u => u.Id == userId);

            //track email change
            var oldEmail = appUser.Email;

            appUser.DisplayName = profileInfoVM.Name;
            appUser.Email = profileInfoVM.Email;
            appUser.University = profileInfoVM.University;
            appUser.Faculty = profileInfoVM.Faculty;
            appUser.BirthDate = profileInfoVM.DateOfBirth;
            appUser.PhoneNumber = profileInfoVM.Phone;
            appUser.EducationLevel = profileInfoVM.EducationLevel;
            appUser.EnglishLevel = profileInfoVM.EnglishProficiencyLevel;
            appUser.MethodOfContact = profileInfoVM.MethodOfContact;

            

            if (profileInfoVM.CvFile != null)
            {
                // Delete existing CV file if it exists
                if (!string.IsNullOrEmpty(appUser.CvUrl) && System.IO.File.Exists(appUser.CvUrl))
                {
                    System.IO.File.Delete(appUser.CvUrl);
                }

                var fileName = Path.GetRandomFileName() + Path.GetExtension(profileInfoVM.CvFile.FileName); // Safer filename
                string sharedFolderPath = _fileStorage.GetSharedCsvFolderPath();
                string filePath = Path.Combine(sharedFolderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileInfoVM.CvFile.CopyToAsync(stream);
                }
                appUser.CvUrl = filePath;
            }

            appUser.ApplicantExperiences = profileInfoVM.Experiences.Select(exp => new ApplicantExperience
            {
                AppUserId = appUser.Id,
                Company = exp.Company,
                Position = exp.Position,
                StartDate = exp.StartDate,
                EndDate = exp.EndDate,
                Description = exp.Description
            }).ToList();

            appUser.ApplicantSkills = profileInfoVM.Skills.Select(skill => new ApplicantSkill
            {
                AppUserId = appUser.Id,
                Name = skill.Name,
                Level = skill.Level
            }).ToList();

            appUser.ApplicantProjects = profileInfoVM.Projects.Select(project => new ApplicantProject
            {
                AppUserId = appUser.Id,
                Name = project.Name,
                Description = project.Description
            }).ToList();

            if (oldEmail != profileInfoVM.Email)
            {
                appUser.UserName = profileInfoVM.Email;
                appUser.EmailConfirmed = false;
                // Single database update
                await _userManager.UpdateAsync(appUser);

                //send mail
                EmailConfirmationService emailService = new EmailConfirmationService(_memoryCache);
                var code = emailService.GenerateRandomCode(); // Generate a random 6-digit code
                emailService.StoreConfirmationCode(userId, code); // Store the code in memory cache

                await _emailSender.SendEmailAsync(profileInfoVM.Email, "Confirm your email",
                    $"Your confirmation code is: <strong>{code}</strong>. Please enter this code on the confirmation page to verify your account.");

                return RedirectToPage("/Account/RegisterConfirmation", new { area = "Identity", email = profileInfoVM.Email });
            }
            // Single database update
            await _userManager.UpdateAsync(appUser);
            ViewBag.Universities = GetUniversities();
            ViewBag.Faculties = GetFaculties();
            //view model should be modified to include cvfile
            return View(profileInfoVM);
        }

        [Authorize]
        public async Task<IActionResult> UserApplications()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var jobApplicaions = await _context.JobApplications.Where(u => u.UserId == userId).Include(u => u.Job).ThenInclude(u => u.Department).ToListAsync();
            List<ApplicationVM> ApplicationVMs = jobApplicaions.Select(x => new ApplicationVM
            {
                Id = x.ApplicationId,
                JobTitle = x.Job.Title,
                JobDescription = x.Job.Description,
                DepartmentName = x.Job.Department.Name,
                JobType = x.Job.JobType.ToString(),
                Status = x.Status,
                ApplicationDate = x.AppliedDate
            }).ToList();
            return View(ApplicationVMs);
        }


        [Authorize]
        public IActionResult UserSettings()
        {
            return View();
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UserSettings(ChangePasswordViewModel changePasswordViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(changePasswordViewModel);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, changePasswordViewModel.CurrentPassword, changePasswordViewModel.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(changePasswordViewModel);
            }

            await _signInManager.RefreshSignInAsync(user);

            return View(changePasswordViewModel);
        }

        [HttpGet]
        public IActionResult ConfirmResumeInfo()
        {
            if (TempData["ProfileInfo"] is string profileInfoJson)
            {
                var profileInfoVM = JsonSerializer.Deserialize<ProfileInfoVM>(profileInfoJson);
                ViewBag.Universities = GetUniversities();
                ViewBag.Faculties = GetFaculties();
                return View(profileInfoVM);
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmResumeInfo(ProfileInfoVM profileInfoVM)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Universities = GetUniversities();
                ViewBag.Faculties = GetFaculties();
                return View(profileInfoVM);
            }

            var appUser = new AppUser();

            appUser.DisplayName = profileInfoVM.Name;
            appUser.Email = profileInfoVM.Email;
            appUser.University = profileInfoVM.University;
            appUser.Faculty = profileInfoVM.Faculty;
            appUser.BirthDate = profileInfoVM.DateOfBirth;
            appUser.PhoneNumber = profileInfoVM.Phone;
            appUser.EducationLevel = profileInfoVM.EducationLevel;
            appUser.EnglishLevel = profileInfoVM.EnglishProficiencyLevel;
            appUser.MethodOfContact = profileInfoVM.MethodOfContact;

            appUser.CvUrl = profileInfoVM.CvUrl;

            appUser.ApplicantExperiences = profileInfoVM.Experiences.Select(exp => new ApplicantExperience
            {
                AppUserId = appUser.Id,
                Company = exp.Company,
                Position = exp.Position,
                StartDate = exp.StartDate,
                EndDate = exp.EndDate,
                Description = exp.Description
            }).ToList();

            appUser.ApplicantSkills = profileInfoVM.Skills.Select(skill => new ApplicantSkill
            {
                AppUserId = appUser.Id,
                Name = skill.Name,
                Level = skill.Level
            }).ToList();

            appUser.ApplicantProjects = profileInfoVM.Projects.Select(project => new ApplicantProject
            {
                AppUserId = appUser.Id,
                Name = project.Name,
                Description = project.Description
            }).ToList();


            appUser.UserName = profileInfoVM.Email;
            appUser.EmailConfirmed = false;
            // Single database update
            var result = await _userManager.CreateAsync(appUser, profileInfoVM.UserPassword);
            if (result.Succeeded)
            {
                result = await _userManager.AddToRoleAsync(appUser, "applicant");

                var userId = await _userManager.GetUserIdAsync(appUser);
                //send mail
                EmailConfirmationService emailService = new EmailConfirmationService(_memoryCache);
                var code = emailService.GenerateRandomCode(); // Generate a random 6-digit code
                emailService.StoreConfirmationCode(userId, code); // Store the code in memory cache

                await _emailSender.SendEmailAsync(profileInfoVM.Email, "Confirm your email",
                    $"Your confirmation code is: <strong>{code}</strong>. Please enter this code on the confirmation page to verify your account.");

                return RedirectToPage("/Account/RegisterConfirmation", new { area = "Identity", email = profileInfoVM.Email, isFromRegisterPage = false });
            }
            else
            {
                return View(profileInfoVM);
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult DownloadCV(string filePath, string fileName)
        {
            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found");

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", $"{fileName}");
        }


        private List<SelectListItem> GetUniversities()
        {
            return new List<string>
            {
                "Cairo University",
                "Ain Shams University",
                "Alexandria University",
                "Helwan University",
                "Mansoura University",
                "Zagazig University",
                "Assiut University",
                "Tanta University",
                "Benha University",
                "Suez Canal University",
                "Minia University",
                "South Valley University",
                "Fayoum University",
                "Beni-Suef University",
                "Sohag University",
                "Kafr El Sheikh University",
                "Damietta University",
                "Port Said University",
                "Menoufia University",
                "Al-Azhar University",
                "The British University in Egypt (BUE)",
                "The American University in Cairo (AUC)",
                "German University in Cairo (GUC)",
                "Misr University for Science and Technology (MUST)",
                "Future University in Egypt (FUE)",
                "October 6 University",
                "Modern Sciences and Arts University (MSA)",
                "Nahda University",
                "Sinai University",
                "Other"
            }.Select(u => new SelectListItem { Text = u, Value = u }).ToList();
        }

        private List<SelectListItem> GetFaculties()
        {
            return new List<string>
            {
                "Faculty of Engineering",
                "Faculty of Medicine",
                "Faculty of Pharmacy",
                "Faculty of Science",
                "Faculty of Commerce",
                "Faculty of Law",
                "Faculty of Arts",
                "Faculty of Education",
                "Faculty of Agriculture",
                "Faculty of Dentistry",
                "Faculty of Computer and Artificial Intelligence",
                "Faculty of Veterinary Medicine",
                "Faculty of Nursing",
                "Faculty of Physical Therapy",
                "Faculty of Tourism and Hotels",
                "Faculty of Mass Communication",
                "Faculty of Fine Arts",
                "Faculty of Applied Arts",
                "Faculty of Al-Alsun (Languages)",
                "Faculty of Islamic Studies",
                "Other"
            }.Select(f => new SelectListItem { Text = f, Value = f }).ToList();
        }
    }
}