using Models;
using System.ComponentModel.DataAnnotations;

namespace HrBackOffice.Models
{
    public class BatchViewModel
    {
        public int? Id { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
        // Navigation property for Jobs
        public ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}
