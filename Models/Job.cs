using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Models
{
    public enum JobType
    {
        FullTime,
        PartTime,
        Freelance
    }
    public class Job
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment
        public int JobId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public JobType JobType { get; set; } // Add JobType

        // Foreign Key for Batch (One Job belongs to One Batch)
        public int BatchId { get; set; }

        [ForeignKey("BatchId")]
        public Batch Batch { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }

        // One job can have many applicants (Applications)
        public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    }

}
