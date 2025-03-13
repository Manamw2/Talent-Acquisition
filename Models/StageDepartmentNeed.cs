using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class StageDepartmentNeed
    {
        public int EmployeesNeeded { get; set; }
        public int DepartmentId { get; set; }
        public int StageId { get; set; }
        [ForeignKey(nameof(DepartmentId))]
        public Department Department { get; set; }
        [ForeignKey(nameof(StageId))]
        public HiringStage HiringStage { get; set; }
    }
}
