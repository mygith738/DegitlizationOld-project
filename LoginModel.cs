using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Digitalization.Models.UserModel
{
    public class LoginModel
    {
        public int StaffNumber { get; set; }

        public string Name { get; set; }

        public string ShortName { get; set; }

        public string Password { get; set; }

        public string RoleName { get; set; }

        public Boolean IsDirector { get; set; }

        public string EmployeeGroup { get; set; }

        public string EmployeeType { get; set; }

        public string Gender { get; set; }

        public string MaritalStatus { get; set; }

        public string Designation { get; set; }

        public string PhysicallyHandicapped { get; set; }
    }
}
