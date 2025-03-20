using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Dtos.Stage
{
    public class HiringStageDto
    {
        public int Id { get; set; }
        public string StageName { get; set; } = string.Empty;
        public OutcomeType OutcomeType { get; set; }
        public List<string> OutcomeSet { get; set; } = new List<string>();
        public List<HiringParameter> Parameters { get; set; } = new List<HiringParameter>();
        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public List<DepartmentNeedDto> departmentNeedDtos { get; set; } = new List<DepartmentNeedDto>();
    }
}
