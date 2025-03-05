using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class BulkCvsJobsHistory
    {
        [Key]
        public string JobId { get; set; }
        public int TotalFiles { get; set; }
        public int SuccessfulFiles { get; set; }
        public int FailedFiles { get; set; }
        public int CvExists { get; set; }
        public int EmailExists { get; set; }
        public int EmailNotValid { get; set; }
        public int ServerErrors { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsRunning { get; set; }
        public string StartedBy { get; set; }
    }
}
