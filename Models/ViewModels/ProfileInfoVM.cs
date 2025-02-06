using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Models.ViewModels
{
    public class ProfileInfoVM
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Faculty { get; set; } = string.Empty;
        public string EducationLevel { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string EnglishProficiencyLevel { get; set; } = string.Empty;
        public string MethodOfContact { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public string? CvUrl { get; set; }
        public IFormFile? CvFile { get; set; }

        // Changed to property with initialization
        public List<ExperienceViewModel> Experiences { get; set; } = new List<ExperienceViewModel>();
        public List<SkillViewModel> Skills { get; set; } = new List<SkillViewModel>();
        public List<ProjectViewModel> Projects { get; set; } = new List<ProjectViewModel>();

        public MyInput Input { get; set; } = new MyInput();
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

    public class MyInput
    {
        public MyInput()
        {
            // Use standard List initialization
            var edus = new List<string> { "Undergraduate", "Graduate" };
            var engs = new List<string> { "Beginner", "Intermediate", "Advanced", "Fluent" };
            var meths = new List<string> { "Email", "Phone" };

            EducationLevels = edus.Select(u => new SelectListItem { Text = u, Value = u });
            EnglishProficiencyLevels = engs.Select(u => new SelectListItem { Text = u, Value = u });
            MethodOfContactOptions = meths.Select(u => new SelectListItem { Text = u, Value = u });
        }

        public IEnumerable<SelectListItem> EducationLevels { get; set; }
        public IEnumerable<SelectListItem> EnglishProficiencyLevels { get; set; }
        public IEnumerable<SelectListItem> MethodOfContactOptions { get; set; }
    }
}