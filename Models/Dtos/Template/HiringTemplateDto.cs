using Models.Dtos.Stage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Dtos.Template
{
    public class HiringTemplateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public string AddedByName { get; set; }
        public List<TemplateStageDto> Stages { get; set; } = new List<TemplateStageDto>();
    }

    public class TemplateStageDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
