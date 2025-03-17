using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class HiringTasksViewModel
    {
        public List<EmployeeTask> Tasks { get; set; } = new List<EmployeeTask>();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalTasks { get; set; }
        public int TotalPages { get; set; }
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public string? Department { get; set; }
        public string? Batch { get; set; }

        // Filter options for dropdowns
        public List<string> Statuses { get; set; } = new List<string>();
        public List<Department> Departments { get; set; } = new List<Department>();
        public List<Batch> Batches { get; set; } = new List<Batch>();
    }
}
