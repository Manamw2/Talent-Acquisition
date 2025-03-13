using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class HiringStageOutcome
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StageId { get; set; }
        [ForeignKey(nameof(StageId))]
        public HiringStage HiringStage { get; set; }
    }
}
