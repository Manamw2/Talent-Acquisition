using Microsoft.AspNetCore.Mvc.Rendering;
using Models.Dtos.Stage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class HiringTemplateVM
    {
        public HiringTemplate HiringTemplate { get; set; }

        // List of all available hiring stages to choose from
        public List<HiringStageDto> AvailableStages { get; set; } = new List<HiringStageDto>();
        public List<TemplateStageVM> TemplateStages { get; set; } = new List<TemplateStageVM>();
    }
    public class TemplateStageVM()
    {
        public int StageId { get; set; }
        public int Occurance { get; set; }
    }
}
