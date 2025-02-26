using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class AppUser : IdentityUser
    {
        public string? DisplayName { get; set; }
        public string? University { get; set; }
        public string? Faculty { get; set; }
        public string? EducationLevel { get; set; }
        public string? EnglishLevel { get; set; }
        public string? MethodOfContact { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string? CvUrl { get; set; }
        public string? ImageUrl { get; set; }
        public List<ApplicantExperience> ApplicantExperiences { get; set; } = new List<ApplicantExperience>();
        public List<ApplicantSkill> ApplicantSkills { get; set; } = new List<ApplicantSkill>();
        public List<ApplicantProject> ApplicantProjects { get; set; } = new List<ApplicantProject>();
    }
}
