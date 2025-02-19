using System.ComponentModel.DataAnnotations;

namespace HrBackOffice.Models
{
    public class AssignJobViewModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public int JobId { get; set; }

        [Required]
        public string Reason { get; set; }

        public string? ReferralName { get; set; }
    }
}
