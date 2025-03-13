using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class HiringStageParameter
    {
        public int StageId { get; set; }
        public int ParameterId { get; set; }
        [ForeignKey(nameof(StageId))]
        public HiringStage HiringStage { get; set; }
        [ForeignKey(nameof(ParameterId))]
        public HiringParameter HiringParameter { get; set; }
    }
}
