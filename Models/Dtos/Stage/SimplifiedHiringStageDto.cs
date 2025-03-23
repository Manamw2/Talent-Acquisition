using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Dtos.Stage
{
    public class SimplifiedHiringStageDto
    {
        public int Id { get; set; }
        public string StageName { get; set; } = string.Empty;
        public OutcomeType OutcomeType { get; set; }
        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
    }
}
