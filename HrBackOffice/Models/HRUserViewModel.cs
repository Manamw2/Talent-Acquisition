using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HrBackOffice.Models
{
    public class HRUserViewModel
    {
        public string? Id { get; set; }
        public string? UserName { get; set; } = string.Empty;

        [Display(Name = "Display Name")]
        public string DisplayName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "The password must be at least {2} characters long.", MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        public List<SelectListItem> Roles { get; set; } = new();
        public string? SelectedRole { get; set; }
    }

}
