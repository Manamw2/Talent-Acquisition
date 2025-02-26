using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HrBackOffice.Models
{
    public class HRUserViewModel
    {
        public string? Id { get; set; }
        public string? UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Display Name is required")]
        [Display(Name = "Display Name")]
        [StringLength(100, ErrorMessage = "Display Name cannot exceed 100 characters")]
        public string DisplayName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters long", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        public List<SelectListItem> Roles { get; set; } = new();
        [Display(Name = "Role")]
        [Required(ErrorMessage = "Please select a role")]
        public string? SelectedRole { get; set; }
    }

}
