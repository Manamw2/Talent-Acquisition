using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Models;
using Models.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TalentAcquisitionModule.Areas.Identity.Pages.Account
{
    public class ProfileCompletionModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;

        public ProfileCompletionModel(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            Experiences = new List<ExperienceViewModel>();
            Skills = new List<SkillViewModel>();
            Projects = new List<ProjectViewModel>();
        }

        [BindProperty]
        public List<ExperienceViewModel> Experiences { get; set; }

        [BindProperty]
        public List<SkillViewModel> Skills { get; set; }

        [BindProperty]
        public List<ProjectViewModel> Projects { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var appUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (appUser != null) {
                // Create all skills at once
                var newSkills = Skills.Select(skill => new ApplicantSkill
                {
                    AppUserId = appUser.Id,
                    Name = skill.Name,
                    Level = skill.Level
                }).ToList();

                // Add all skills in one go
                appUser.ApplicantSkills.AddRange(newSkills);

                var newExperiences = Experiences.Select(experience => new ApplicantExperience
                {
                    AppUserId = appUser.Id,
                    Company = experience.Company,
                    Position = experience.Position,
                    StartDate = experience.StartDate,
                    EndDate = experience.EndDate,
                    Description = experience.Description
                }).ToList();
                appUser.ApplicantExperiences.AddRange(newExperiences);

                var newProjects = Projects.Select(project => new ApplicantProject
                {
                    AppUserId= appUser.Id,
                    Name = project.Name,
                    Description = project.Description
                });
                appUser.ApplicantProjects.AddRange(newProjects);

                // Single database update
                await _userManager.UpdateAsync(appUser);

            }

            return RedirectToPage("Profile");
        }
    }
    public class ExperienceViewModel
    {
        public int Id { get; set; }
        public string Company { get; set; }
        public string Position { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Description { get; set; }
    }

    public class SkillViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Level { get; set; }
    }

    public class ProjectViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
