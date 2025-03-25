using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public string UserName { get; set; }
        public int TotalBatches { get; set; }
        public int TotalJobs { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalTasks { get; set; }
    }
}
