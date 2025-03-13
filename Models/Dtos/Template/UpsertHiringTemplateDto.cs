using Models.Dtos.Stage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Dtos.Template
{
    public class UpsertHiringTemplateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public List<int> TemplateStages { get; set; } = new List<int>();
    }
}
