using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class HiringTemplateStage
    {
        public int Id { get; set; }
        public int Occurrence { get; set; }
        public int TemplateId { get; set; }
        public int StageId { get; set; }
        [ForeignKey(nameof(TemplateId))]
        public HiringTemplate HiringTemplate { get; set; }
        [ForeignKey(nameof(StageId))]
        public HiringStage HiringStage { get; set; }
    }
}
