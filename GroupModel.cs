using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Digitalization.Models.AdminModel
{
    public class GroupModel
    {
        public string GroupId { get; set; }

        public string GroupName { get; set; }

        public string GHStaffNumber { get; set; }

        public string GroupHeadName { get; set; }

        public string SGHStaffNumber { get; set; }

        public string SGroupHeadName { get; set; }

        public int CenterID { get; set; }

        public string CenterName { get; set; }

        public List<CenterModel> Centers { get; set; }
    }
}
