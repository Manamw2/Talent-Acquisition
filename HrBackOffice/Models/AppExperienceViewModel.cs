using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HrBackOffice.Models
{
    public class AppExperienceViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Company name is required.")]
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters.")]
        public string Company { get; set; }
        [Required(ErrorMessage = "Position name is required.")]
        [StringLength(100, ErrorMessage = "Position name cannot exceed 100 characters.")]
        public string Position { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [Required(ErrorMessage = "Experience Description is required.")]
        [DisplayName("Experience Description")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }
    }
}
