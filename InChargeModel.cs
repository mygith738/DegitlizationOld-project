using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Digitalization.Models.AdminModel
{
    public class InchargeModel
    {
        public string InchargeID { get; set; }

        public int GroupID { get; set; }

        public string GroupName { get; set; }

        public int InchargeStaffNumber { get; set; }

        public string InchargeName { get; set; }

        public List<GroupModel> Groups { get; set; }
    }
}
