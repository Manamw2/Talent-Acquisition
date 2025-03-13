using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Dtos.Stage;

namespace Models
{
    public class Department
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment
        public int DepartmentId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        // One Department can have multiple Jobs
        public ICollection<Job> Jobs { get; set; } = new List<Job>();
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public List<StageDepartmentNeed> departmentNeedDtos { get; set; } = new List<StageDepartmentNeed>();
        public int? TemplateId { get; set; }
        [ForeignKey(nameof(TemplateId))]
        public HiringTemplate? HiringTemplate { get; set; }
    }
}
