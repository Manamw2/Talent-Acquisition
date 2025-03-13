using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Dtos.Stage
{
    public class UpsertHiringStageDto
    {
        [Required]
        [MaxLength(100)]
        public string StageName { get; set; } = string.Empty;
        [Required]
        public OutcomeType OutcomeType { get; set; }
        public List<string> OutcomeSet { get; set; } = new List<string>();
        public List<int> ParametersIds { get; set; } = new List<int>();
        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }
        public List<DepartmentNeedDto> departmentNeedDtos { get; set; } = new List<DepartmentNeedDto>();
    }

    public class DepartmentNeedDto
    {
        [Required]
        public int DepartmentId { get; set; }
        [Required]
        public int DepartmentNeed {  get; set; }
    }
}
