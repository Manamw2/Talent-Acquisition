using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class EmployeeTask
    {
        public int Id { get; set; }  // Primary key
        public int EmployeeId { get; set; }
        [ForeignKey(nameof(EmployeeId))]
        public Employee Employee { get; set; }
        public int TaskId { get; set; }
        [ForeignKey(nameof(TaskId))]
        public HiringTask HiringTask { get; set; }

        public string Status { get; set; } = "New";  // Default status
        public DateTime AssignedDate { get; set; } = DateTime.Now;
    }
}
