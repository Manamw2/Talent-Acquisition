using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public enum OutcomeType
    {
        PassFail,
        Set,
        Eveluation
    }

    public class HiringStage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public OutcomeType OutcomeType { get; set; }
        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? AppUserId { get; set; }
        [ForeignKey(nameof(AppUserId))]
        public AppUser? CreatedBy { get; set; }
        public List<HiringTemplateStage> HiringTemplateStages { get; set; } = new List<HiringTemplateStage>();
        public List<HiringStageOutcome> HiringStageOutcomes { get; set; } = new List<HiringStageOutcome>();
        public List<HiringStageParameter> HiringStageParameters { get; set; } = new List<HiringStageParameter>();
        public List<StageDepartmentNeed> StageDepartmentNeeds { get; set; } = new List<StageDepartmentNeed>();
        public List<HiringTask> Tasks { get; set; } = new List<HiringTask>();
    }
}
