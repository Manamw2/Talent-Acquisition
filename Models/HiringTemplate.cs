using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class HiringTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public string AppUserId { get; set; }
        [ForeignKey(nameof(AppUserId))]
        public AppUser CreatedBy { get; set; }
        public List<HiringTemplateStage> HiringTemplateStages { get; set; } = new List<HiringTemplateStage>();
        public List<Department> Departments { get; set; } = new List<Department>();
        public List<Job> Jobs { get; set; } = new List<Job>();
        public List<Batch> Batches { get; set; } = new List<Batch>();
    }
}
