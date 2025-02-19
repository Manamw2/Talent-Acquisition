using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Models.ViewModels
{
    public class ProfileInfoVM
    {
        public string Email { get; set; } = string.Empty;
        [DisplayName("User Password")]
        public string UserPassword { get; set; } = string.Empty;
        [DisplayName("User Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string University { get; set; } = string.Empty;
        public string Faculty { get; set; } = string.Empty;
        [DisplayName("Education Level")]
        public string EducationLevel { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        [DisplayName("English Proficiency Level")]
        public string EnglishProficiencyLevel { get; set; } = string.Empty;
        [DisplayName("Method Of Contact")]
        public string MethodOfContact { get; set; } = string.Empty;
        [DisplayName("Date of Birth")]
        public DateOnly? DateOfBirth { get; set; }
        [DisplayName("CV")]
        public string? CvUrl { get; set; }
        //[JsonIgnore] // Prevent serialization of IFormFile
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
        [DisplayName("Experience Description")]
        public string Description { get; set; }
    }

    public class SkillViewModel
    {
        public int Id { get; set; }
        [DisplayName("Skill Name")]
        public string Name { get; set; }
        [DisplayName("Skill Level")]
        public string Level { get; set; }
    }

    public class ProjectViewModel
    {
        public int Id { get; set; }
        [DisplayName("Project Name")]
        public string Name { get; set; }
        [DisplayName("Project Description")]
        public string Description { get; set; }
    }

    public class MyInput
    {
        public MyInput()
        {

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