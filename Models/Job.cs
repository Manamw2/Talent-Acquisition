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
        Internship
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
        [Column(TypeName = "NVARCHAR(MAX)")]
        public string Description { get; set; }

        [Required]
        public JobType JobType { get; set; }

        // Foreign Key for Batch (One Job belongs to One Batch)
        public int? BatchId { get; set; }

        [ForeignKey("BatchId")]
        public Batch Batch { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }
        public int? TemplateId { get; set; }
        [ForeignKey(nameof(TemplateId))]
        public HiringTemplate? HiringTemplate { get; set; }
    }

}
