using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Digitalization.Models.AdminModel
{
    public class EmergencyContactModel
    {
        public int StaffNumber { get; set; }

        public string ContactName { get; set;}

        public string Relation { get; set;}

        public string MobilePhoneNumber { get; set;}

        public string LandlineNumber { get; set; }

        public string EmailId { get; set; }

        public int EmergencyID { get; set; }
    }
}
