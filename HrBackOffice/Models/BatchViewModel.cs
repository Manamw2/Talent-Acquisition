using HrBackOffice.Helper;
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
        public string BatchName { get; set; }

        [Required(ErrorMessage = "Start Date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DateGreaterThan("StartDate", ErrorMessage = "End Date must be greater than or equal to Start Date")]
        public DateTime EndDate { get; set; }

        public int? JobId { get; set; }

        [Display(Name = "Job")]
        public string JobTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Target Number is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Target Number must be greater than 0")]
        [Display(Name = "Target Number")]
        public int TargetNumber { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public BatchStatus Status { get; set; } = BatchStatus.New;

        [Display(Name = "Status")]
        public string StatusName => Status.ToString();

        public List<JobApplicationVM> JobApplications { get; set; } = new List<JobApplicationVM>();
    }

   
}
