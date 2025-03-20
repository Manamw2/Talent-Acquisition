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
        public List<JobApplication> Applicants { get; set; } = new List<JobApplication>();
    }
}
