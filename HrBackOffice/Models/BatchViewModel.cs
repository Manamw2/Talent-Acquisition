using Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace HrBackOffice.Models
{
    public class BatchViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Batch Name is required")]
        [StringLength(100, ErrorMessage = "Batch Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Start Date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        // Navigation property for Jobs
        public ICollection<Job> Jobs { get; set; } = new List<Job>();

        public List<JobApplicationVM> JobApplications { get; set; } = new List<JobApplicationVM>();

    }
}
