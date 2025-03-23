using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class HiringStageVM
    {
        public HiringStage HiringStage { get; set; }
        public string AcceptedApplicationStatus { get; set; }
        public string RejectedApplicationStatus { get; set; }
        public string AcceptedNotification { get; set; }
        public string RejectedNotification { get; set; }
        public List<DepartmentVm> Departments { get; set; } = new List<DepartmentVm>();
        public List<int> ParameterIds { get; set; } = new List<int>();

        public IEnumerable<SelectListItem> ParametersList { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> DepartmentsList { get; set; } = Enumerable.Empty<SelectListItem>();
    }
    public class DepartmentVm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int NeededEmployees { get; set; }
    }
}
