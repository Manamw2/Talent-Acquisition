using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HrBackOffice.Models
{
    public class UserViewModel
    {
        public string? Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public IEnumerable<string>? Roles{ get; set; }
    }
}
