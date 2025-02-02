using Models;

namespace HrBackOffice.Models
{
    public class BatchViewModel
    {
        public int? Id { get; set; }
        public string Name { get; set; }

        // Navigation property for Jobs
        public ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}
