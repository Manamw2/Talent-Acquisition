using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class JobRecommend
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // Foreign Key to AppUser

        [ForeignKey("UserId")]
        public AppUser AppUser { get; set; } // Navigation Property
        [Required]
        public int JobId { get; set; }

        [ForeignKey("JobId")]
        public Job Job { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;


    }
}
