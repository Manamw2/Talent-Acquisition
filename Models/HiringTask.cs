using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class HiringTask
    {
        public int Id { get; set; }
        public int HiringStageId { get; set; }
        [ForeignKey(nameof(HiringStageId))]
        public HiringStage HiringStage { get; set; }
        public int BatchId { get; set; }
        [ForeignKey(nameof(BatchId))]
        public Batch Batch { get; set; }
        List<EmployeeTask> EmployeeTasks { get; set; } = new List<EmployeeTask>();
    }
}
