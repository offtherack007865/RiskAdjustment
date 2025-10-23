using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskAdjustment.Data.Models.RefTables
{
    public class UserRoles
    {
        public List<string> TheValues { get; set; }

        public List<string> Values
        {
            get
            {
                return new List<string>()
                {
                    "Admin",
                    "Manager",
                    "Coding Support Specialist"
                };
            }
        }


    }


    //public enum UserRole
    //{
    //    [Description("Admin")]
    //    Admin = 1,
    //    [Description("Manager")]
    //    Manager = 2,
    //    [Description("Coding Support Specialist")]
    //    CodingSupportSpecialist = 3,
    //    [Description("PVP Specialist")]
    //    PvpSupportSpecialist = 4,
        
    //}
}
