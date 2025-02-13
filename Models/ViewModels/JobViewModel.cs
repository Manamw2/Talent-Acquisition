using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class JobViewModel
    {
        public int JobId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public JobType JobType { get; set; }  // Added JobType enum

        [Required]
        [Display(Name = "Batch")]
        public int BatchId { get; set; }

        [Required]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;

        public List<SelectListItem> Batches { get; set; } = new();
        public List<SelectListItem> Departments { get; set; } = new();

        public List<SelectListItem> JobTypes => Enum.GetValues(typeof(JobType))
            .Cast<JobType>()
            .Select(jt => new SelectListItem
            {
                Value = ((int)jt).ToString(),
                Text = jt.ToString()
            }).ToList();
        public int ApplicationCount { get; set; }
        public int MyProperty { get; set; }
        public DateTime? EndDate { get; set; }
        public List<JobApplicationViewModel> JobApplications { get; set; } = new List<JobApplicationViewModel>();

    }
}
