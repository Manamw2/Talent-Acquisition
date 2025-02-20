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

        public DateTime StartDate { get; set; } = DateTime.Now; // Add StartDate
        
        public DateTime EndDate { get; set; } = DateTime.Now; // Add EndDate

        // One Batch can have only one Job
        public Job? Job { get; set; } 
    }
}
