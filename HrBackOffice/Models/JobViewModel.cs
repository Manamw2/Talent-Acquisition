using Models;

namespace HrBackOffice.Models
{
    public class JobViewModel
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string BatchName { get; set; } // Name of the associated batch
    }
}
