using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using Models.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HrBackOffice.Models
{
    

    public class UserViewModel
    {
        public string? Id { get; set; }
        public string? UserName { get; set; } = string.Empty;
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
        [RegularExpression(@"^[0-9]{11}$", ErrorMessage = "Phone number must be exactly 11 digits")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        public string? EducationLevel { get; set; }
        public string? Faculty { get; set; }
        public string? University { get; set; }
        public string? MethodOfContact { get; set; }

        public string EnglishProficiencyLevel { get; set; } = string.Empty; // Removed duplicate EnglishLevel

        public DateOnly? BirthDate { get; set; } // Changed from DateOnly? to DateTime?

        public string? CvUrl { get; set; }

        [Display(Name = "CV File")]
        public IFormFile? CvFile { get; set; } // For file upload

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "The password must be at least {2} characters long.", MinimumLength = 6)]
        public string Password { get; set; } = "Temp@1234"; // Default password

        public IEnumerable<string> Roles { get; set; } = new List<string> { "Applicant" };

        public List<AppExperienceViewModel> ApplicantExperiences { get; set; } = new();
        public List<AppSkillViewModel> ApplicantSkills { get; set; } = new();
        public List<AppProjectViewModel> ApplicantProjects { get; set; } = new();

        // Dropdown lists with default initialization
        public List<SelectListItem> EducationLevels { get; set; } = new();
        public List<SelectListItem> EnglishProficiencyLevels { get; set; } = new();
        public List<SelectListItem> Universities { get; set; } = new();
        public List<SelectListItem> Faculties { get; set; } = new();
    }


}
