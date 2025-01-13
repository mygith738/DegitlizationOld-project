using System;
using System.Collections.Generic;

namespace Digitalization.Models.AuthorityModel
{
    public class VisitorPassVisitorsModel
    {
        public string PassID { get; set; }

        public string VisitorName { get; set; }

        public string VisitorInstitution { get; set; }

        public string VisitorAddress { get; set; }

        public int VisitorIDCardNumber { get; set; }

        public string VisitorDesignation { get; set; }

        public string VisitorMobileNumber { get; set; }

        public int ActionTakenByStaffNumber { get; set; }

        public DateTime DateTime { get; set; }

        public string VisitorImageLocation { get; set; }

        public string AlternateVisitorImageLocation { get; set; }

        public string VisitorSignatureLocation { get; set; }

        public string VisitorGender { get; set; }

        public int VisitorAge { get; set; }

        public string VisitorEmailID { get; set; }

        public string Nationality { get; set; }
    }
}
