using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Digitalization.Models.AuthorityModel
{
    public class IndentModel
    {
        public string IndentID { get; set; }

        public DateTime IndentRaisedDateTime { get; set; }

        public int SenderStaffNumber { get; set; }

        public string SenderFirstName { get; set; }

        public string SenderDesignation { get; set; }

        public int RecieverStaffNumber { get; set; }

        public string RecieverName { get; set; }

        public string RecieverDesignation { get; set; }

        public string ProjectNumber { get; set; }

        public string ProjectName { get; set; }

        public int FromGroupID { get; set; }

        public string FromGroupName { get; set; }

        public int FromCenterID { get; set; }

        public string FromCenterName { get; set; }

        public int ToGroupID { get; set; }

        public string ToGroupName { get; set; }

        public int ToCenterID { get; set; }

        public string ToCenterName { get; set; }

        public string Requirement { get; set; }

        public bool IsAttachementPresent { get; set; }

        public string AttachementLocation { get; set; }

        public string AlteredAttachementLocation { get; set; }

        public string OldIndentStatus { get; set; }

        public DateTime DateTime { get; set; }

        public string OldRemarks { get; set; }

        public DateTime OldTargetDate { get; set; }

        public DateTime NewTargetDate { get; set; }

        public int ActionTakenByStaffNumber { get; set; }

        public string ActionTakenByName { get; set; }

        public string ActionTakenByDesignation { get; set; }

        public int ReportingOfficer { get; set; }

        public string ReportingOfficerName { get; set; }

        public string ReportingOfficerDesignation { get; set; }

        public string Subject { get; set; }
    }
}
