using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Digitalization.Models.AdminModel
{
    public class EmployeeListModel
    {
        public int StaffNumber { get; set; }

        public string Name { get; set; }

        public string Designation { get; set; }
        
        public string InternalPhoneNumber { get; set; }

        public int GroupID { get; set; }

        public string GroupName { get; set; }

        public int CenterID { get; set; }

        public string CenterName { get; set; }
    }
}
