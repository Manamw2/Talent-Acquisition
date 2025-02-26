using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Models.ViewModels
{
    public class ProfileInfoVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string UserPassword { get; set; } = string.Empty;

        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        [RegularExpression(@"^[a-zA-Z\u0600-\u06FF\s-']+$", ErrorMessage = "Name can only contain English letters, Arabic letters, spaces, hyphens, and apostrophes.")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "University name cannot exceed 100 characters.")]
        public string University { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "Faculty name cannot exceed 100 characters.")]
        public string Faculty { get; set; } = string.Empty;

        [Required]
        [DisplayName("Education Level")]
        public string EducationLevel { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [DisplayName("English Proficiency Level")]
        public string EnglishProficiencyLevel { get; set; } = string.Empty;

        [Required]
        [DisplayName("Method Of Contact")]
        public string MethodOfContact { get; set; } = string.Empty;

        [DisplayName("Date of Birth")]
        [DataType(DataType.Date)]
        public DateOnly? DateOfBirth { get; set; }

        [DisplayName("CV")]
        public string? CvUrl { get; set; }

        [DisplayName("CV File")]
        [DataType(DataType.Upload)]
        public IFormFile? CvFile { get; set; }

        public List<ExperienceViewModel> Experiences { get; set; } = new List<ExperienceViewModel>();
        public List<SkillViewModel> Skills { get; set; } = new List<SkillViewModel>();
        public List<ProjectViewModel> Projects { get; set; } = new List<ProjectViewModel>();

        public MyInput Input { get; set; } = new MyInput();
    }

    public class ExperienceViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters.")]
        public string Company { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "Position name cannot exceed 100 characters.")]
        public string Position { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Required]
        [DisplayName("Experience Description")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;
    }

    public class SkillViewModel
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Skill Name")]
        [StringLength(100, ErrorMessage = "Skill name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [DisplayName("Skill Level")]
        public string Level { get; set; } = string.Empty;
    }

    public class ProjectViewModel
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Project Name")]
        [StringLength(100, ErrorMessage = "Project name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [DisplayName("Project Description")]
        [StringLength(500, ErrorMessage = "Project description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;
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