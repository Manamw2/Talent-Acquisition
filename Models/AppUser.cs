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
        public string? Faculty { get; set; }
        public string? EducationLevel { get; set; }
        public string? EnglishLevel { get; set; }
        public string? MethodOfContact { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string? CvUrl { get; set; }
    }
}
