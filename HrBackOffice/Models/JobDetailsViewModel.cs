using Models;

namespace HrBackOffice.Models
{
    public class JobDetailsViewModel
    {
        public Job Job { get; set; }
        public List<JobApplication> Applicants { get; set; }
    }
}
