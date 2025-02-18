namespace HrBackOffice.Models
{
    public class ApplicantViewModel
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int AppliedJobsCount { get; set; }
        public string LatestApplication { get; set; }
    }
}
