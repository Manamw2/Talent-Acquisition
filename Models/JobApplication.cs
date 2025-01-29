using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class JobApplication
    {
        [Required]
        public string AppUserId { get; set; } = string.Empty;  // Default value for non-null
        [Required]
        public int JobId { get; set; }

        [ForeignKey(nameof(AppUserId))]  // Defines the foreign key relationship
        public AppUser AppUser { get; set; } = new AppUser();  // Ensures default object initialization

        [ForeignKey(nameof(JobId))]  // Defines the foreign key relationship
        public Job Job { get; set; } = new Job();  // Ensures default object initialization
    }
}
