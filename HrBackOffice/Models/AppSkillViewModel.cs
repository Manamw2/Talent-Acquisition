using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace HrBackOffice.Models
{
    public class AppSkillViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Skill name is required.")]
        [DisplayName("Skill Name")]
        [StringLength(100, ErrorMessage = "Skill name cannot exceed 100 characters.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Skill name is required.")]
        [DisplayName("Skill Level")]
        public string Level { get; set; }
    }
}
