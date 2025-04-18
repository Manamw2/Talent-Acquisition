﻿using System.Collections.Generic;

namespace HrBackOffice.Models
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public List<RoleViewModel> Roles { get; set; } = new List<RoleViewModel>();
        public string SelectedRoleId { get; set; } 

    }
}
