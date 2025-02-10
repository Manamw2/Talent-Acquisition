﻿using System.Collections.Generic;
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
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "The password must be at least {2} characters long.", MinimumLength = 6)]
        public string Password { get; set; }
        public IEnumerable<string>? Roles{ get; set; }
    }
}
