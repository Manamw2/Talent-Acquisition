using System.ComponentModel.DataAnnotations;

namespace HrBackOffice.Models
{
    public class ProfileViewModel
    {
        public string Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string DisplayName { get; set; }

        public string? ImageUrl { get; set; }
        public string? Role { get; set; }
        public IFormFile? ImageFile { get; set; }

    }
}
