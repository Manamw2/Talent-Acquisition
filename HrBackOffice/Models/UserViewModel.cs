using Models;
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
        public string? EducationLevel { get; set; }
        public string? EnglishLevel { get; set; }
        public string? Faculty { get; set; }
        public string? MethodOfContact { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string? CvUrl { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "The password must be at least {2} characters long.", MinimumLength = 6)]
        public string Password { get; set; } = "Temp@1234"; // Default password

        public IEnumerable<string>? Roles { get; set; } = new List<string> { "Applicant" };

        public List<ApplicantExperience> ApplicantExperiences { get; set; } = new();
        public List<ApplicantSkill> ApplicantSkills { get; set; } = new();
        public List<ApplicantProject> ApplicantProjects { get; set; } = new();
    }

}
