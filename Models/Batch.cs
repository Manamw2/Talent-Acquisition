using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Batch
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment
        public int BatchId { get; set; }

        [Required]
        [StringLength(50)]
        public string BatchName { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now;

        // New properties
        [Required(ErrorMessage = "Target number is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Target number must be greater than 0")]
        public int TargetNumber { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public BatchStatus Status { get; set; } = BatchStatus.New;

        // One Batch has one Job (one-to-one relationship)
        public int? JobId { get; set; }

        [ForeignKey("JobId")]
        [DeleteBehavior(DeleteBehavior.NoAction)] // Add this to ensure consistency
        public Job? Job { get; set; }

        public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
    }

    // Add this enum for batch status
    public enum BatchStatus
    {
        New,
        Active,
        OnHold,
        Completed,
        Cancelled
    }
}
