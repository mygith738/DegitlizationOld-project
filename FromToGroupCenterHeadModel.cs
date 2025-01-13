using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Digitalization.Models.UserModel
{
    public class FromToGroupCenterHeadModel
    {
        public int FromGroupHeadStaffNumber { get; set; }

        public string FromGroupHeadName { get; set; }

        public int FromCenterHeadStaffNumber { get; set; }

        public string FromCenterHeadName { get; set; }

        public int ToGroupHeadStaffNumber { get; set; }

        public string ToGroupHeadName { get; set; }

        public int ToCenterHeadStaffNumber { get; set; }

        public string ToCenterHeadName { get; set; }
    }
}
