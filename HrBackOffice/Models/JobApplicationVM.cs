namespace HrBackOffice.Models
{
    public class JobApplicationVM
    {
        public int ApplicationId { get; set; }
        public string UserId { get; set; }
        public string ApplicantName { get; set; }
        public string ApplicantEmail { get; set; }
        public DateTime AppliedDate { get; set; }
        public string Status { get; set; }
    }
}
