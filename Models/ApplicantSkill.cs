using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class ApplicantSkill
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Level { get; set; }
        public string AppUserId { get; set; }
        [ForeignKey(nameof(AppUserId))]
        public AppUser? AppUser { get; set; }
    }
}
