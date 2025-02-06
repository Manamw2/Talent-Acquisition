using Models;

namespace HrBackOffice.Models
{
    public class JobViewModel
    {
        public int JobId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public JobType JobType { get; set; }
        public string BatchName { get; set; }
        public DateTime EndDate { get; set; }
        public string DepartmentName { get; set; }
        public int ApplicationCount { get; set; }
    }
}
