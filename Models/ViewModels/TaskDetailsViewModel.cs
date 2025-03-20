using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class TaskDetailsViewModel
    {
        public EmployeeTask EmployeeTask { get; set; }
        public List<JobApplication> Applicants { get; set; }
        public string Comments { get; set; }
        public List<string> AvailableStatuses { get; set; } = new List<string> {
        "Pending", "Shortlisted", "Interview", "Rejected", "Hired", "Withdrawn"
    };
    }
}
