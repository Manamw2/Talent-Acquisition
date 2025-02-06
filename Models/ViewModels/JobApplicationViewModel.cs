using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class JobApplicationViewModel
    {
        public int ApplicationId { get; set; }
        public string UserId { get; set; }
        public string ApplicantName { get; set; }
        public string ApplicantEmail { get; set; }
        public DateTime AppliedDate { get; set; }
        public string Status { get; set; }
    }
}
