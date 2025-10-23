using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskAdjustment.Data.Models.RefTables;

namespace RiskAdjustment.Data.ViewModels
{
    public class UserViewModel
    {
        public List<User> UserList { get; set; }
        public UserRoles UserRoles { get; set; }
        public User SelectedUser { get; set; }
    }
}
