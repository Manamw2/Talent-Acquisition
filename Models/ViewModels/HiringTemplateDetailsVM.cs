using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class HiringTemplateDetailsVM
    {
        public HiringTemplate HiringTemplate { get; set; }
        public List<TemplateStageDetailsVM> TemplateStages { get; set; } = new List<TemplateStageDetailsVM>();
    }

    public class TemplateStageDetailsVM
    {
        public int StageId { get; set; }
        public string StageName { get; set; }
        public int Occurance { get; set; }
        public OutcomeType OutcomeType { get; set; }
        public string OutcomeTypeName { get; set; }
        public List<string> Parameters { get; set; } = new List<string>();
    }
}
