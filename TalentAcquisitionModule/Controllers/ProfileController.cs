﻿using DataAccess.Data;
using DataAccess.Migrations;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Shared;
using Models;
using Models.ViewModels;
using System.Security.Claims;
using TalentAcquisitionModule.Services;

namespace TalentAcquisitionModule.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IMemoryCache _memoryCache;
        public ProfileController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ApplicationDbContext context, IEmailSender emailSender, IMemoryCache memoryCache)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _memoryCache = memoryCache;
            _emailSender = emailSender;
        }
        public async Task<IActionResult> Index()
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
            
            ProfilePageViewModel profilePageViewModel = new ProfilePageViewModel();
            var jobApplicaions =  await _context.JobApplications.Where(u => u.UserId == userId).Include(u => u.Job).ThenInclude(u => u.Department).ToListAsync();
            profilePageViewModel.ApplicationVMs = jobApplicaions.Select(x => new ApplicationVM
            {
                Id = x.ApplicationId,
                JobTitle = x.Job.Title,
                JobDescription = x.Job.Description,
                DepartmentName = x.Job.Department.Name,
                JobType = x.Job.JobType.ToString(),
                Status = x.Status,
                ApplicationDate = x.AppliedDate
            }).ToList();
            profilePageViewModel.ProfileInfoVM = profileInfoVM;
            return View(profilePageViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(ProfileInfoVM profileInfoVM)
        {
            if (!ModelState.IsValid)
            {
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
            appUser.Faculty = profileInfoVM.Faculty;
            appUser.BirthDate = profileInfoVM.DateOfBirth;
            appUser.PhoneNumber = profileInfoVM.Phone;
            appUser.EducationLevel = profileInfoVM.EducationLevel;
            appUser.EnglishLevel = profileInfoVM.EnglishProficiencyLevel;
            appUser.MethodOfContact = profileInfoVM.MethodOfContact;

            // Delete existing CV file if it exists
            if (profileInfoVM.CvFile != null)
            {
                var existingFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", appUser.CvUrl.TrimStart('/'));
                System.IO.File.Delete(existingFilePath);
                // Save new CV file
                var fileName = Path.GetRandomFileName() + Path.GetExtension(profileInfoVM.CvFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/cvs", fileName);

                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileInfoVM.CvFile.CopyToAsync(stream);
                }

                appUser.CvUrl = $"/cvs/{fileName}";
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
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel changePasswordViewModel)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
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
                return RedirectToAction("Index");
            }

            await _signInManager.RefreshSignInAsync(user);

            return RedirectToAction("Index");
        }
    }
}
