using RiskAdjustment.Data.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;



namespace RiskAdjustment.Data.Models.RefTables
{
    public class User
    {
        public int Id { get; set; }
        public string Initials { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string DisplayName
        {
            get
            {
                return $"{LastName}, {FirstName}";
            }
        }
        public string Email { get; set; }
        public string ActiveDate { get; set; }
        public string InactiveDate { get; set; }
        public string Note { get; set; }
        public string AthenaId { get; set; }
        public string Contract { get; set; }
        public string Role { get; set; }

    }
}
