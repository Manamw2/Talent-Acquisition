using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Models.ViewModels
{
    public class ProfilePageViewModel
    {
        public ProfileInfoVM ProfileInfoVM { get; set; }
        public ChangePasswordViewModel ChangePasswordViewModel { get; set; }
        public List<ApplicationVM> ApplicationVMs { get; set; } = new List<ApplicationVM>();
    }
}
