using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using Models.ViewModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HrBackOffice.Models
{
    public class UserViewModel
    {
        public string? Id { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string EnglishProficiencyLevel { get; set; } = string.Empty;

        public string? EducationLevel { get; set; }
        public string? EnglishLevel { get; set; }
        public string? Faculty { get; set; }
        public string? MethodOfContact { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string? CvUrl { get; set; }
        [Display(Name = "CV File")]
        public IFormFile CvFile { get; set; } // Add this property for file upload

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "The password must be at least {2} characters long.", MinimumLength = 6)]
        public string Password { get; set; } = "Temp@1234"; // Default password

        public IEnumerable<string>? Roles { get; set; } = new List<string> { "Applicant" };

        public List<AppExperienceViewModel> ApplicantExperiences { get; set; } = new();
        public List<AppSkillViewModel> ApplicantSkills { get; set; } = new();
        public List<AppProjectViewModel> ApplicantProjects { get; set; } = new();
    }

}
