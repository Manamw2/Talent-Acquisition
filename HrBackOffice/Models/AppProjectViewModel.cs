using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace HrBackOffice.Models
{
    public class AppProjectViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Project name is required.")]
        [DisplayName("Project Name")]
        [StringLength(100, ErrorMessage = "Project name cannot exceed 100 characters.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Project description is required.")]
        [DisplayName("Project Description")]
        [StringLength(500, ErrorMessage = "Project description cannot exceed 500 characters.")]
        public string Description { get; set; }
    }
}
