using System;
using System.Collections.Generic;
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
        public string UserPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string University { get; set; } = string.Empty;
        public string Faculty { get; set; } = string.Empty;
        public string EducationLevel { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string EnglishProficiencyLevel { get; set; } = string.Empty;
        public string MethodOfContact { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
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

            var Uni = new List<string>
            {   "Cairo University",
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
                "Sinai University"

            };

            var Fac = new List<string>
            {   "Faculty of Engineering",
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
                "Faculty of Islamic Studies"
            };
            var edus = new List<string> { "Undergraduate", "Graduate" };
            var engs = new List<string> { "Beginner", "Intermediate", "Advanced", "Fluent" };
            var meths = new List<string> { "Email", "Phone" };

            Universities = Uni.Select(u => new SelectListItem { Text = u, Value = u });
            Faculties = Fac.Select(u => new SelectListItem { Text = u, Value = u });
            EducationLevels = edus.Select(u => new SelectListItem { Text = u, Value = u });
            EnglishProficiencyLevels = engs.Select(u => new SelectListItem { Text = u, Value = u });
            MethodOfContactOptions = meths.Select(u => new SelectListItem { Text = u, Value = u });
        }
        public IEnumerable<SelectListItem> Universities { get; set; }
        public IEnumerable<SelectListItem> Faculties { get; set; }
        public IEnumerable<SelectListItem> EducationLevels { get; set; }
        public IEnumerable<SelectListItem> EnglishProficiencyLevels { get; set; }
        public IEnumerable<SelectListItem> MethodOfContactOptions { get; set; }
    }
}