using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Digitalization.Models.AuthorityModel
{
    public class VisitorPassModel
    {
        public string PassID { get; set; }

        public int SenderStaffNumber { get; set; }

        public string SenderFirstName { get; set; }

        public string SenderDesignation { get; set; }

        public int SenderGroupID { get; set; }

        public string SenderGroupName { get; set; }

        public int SenderCenterID { get; set; }

        public string SenderCenterName { get; set; }

        public string VisitorName { get; set; }

        public string VisitorInstitution { get; set; }

        public string VisitorAddress { get; set; }

        public int VisitorIDCardNumber { get; set; }

        public string VisitorDesignation { get; set; }

        public string VisitorMobileNumber { get; set; }

        public string OldStatus { get; set; }

        public string NewStatus { get; set; }

        public string OldRemarks { get; set; }

        public string NewRemarks { get; set; }

        public int ActionTakenByStaffNumber { get; set; }

        public string ActionTakenByName { get; set; }

        public string ActionTakenByDesignation { get; set; }

        public DateTime DateTime { get; set; }

        public DateTime RaisedDateTime { get; set; }

        public bool IsGroupSelected { get; set; }

        public int EmployeeCenterID { get; set; }

        public int EmployeeGroupID { get; set; }

        public string EmployeeCenterName { get; set; }

        public string EmployeeGroupName { get; set; }

        public int EmployeeStaffNumber { get; set; }

        public string EmployeeName { get; set; }

        public string EmployeeDesignation { get; set; }

        public string VisitorImageLocation { get; set; }

        public string IsSigned { get; set; }

        public string SignatureFileLocation { get; set; }

        public string AlternateSignatureFileLocation { get; set; }

        public string IsCarryingLaptop { get; set; }

        public string IsCarryingPendrive { get; set; }

        public string IsCarryingOther { get; set; }

        public string OtherCarryingItem { get; set; }

        public string PurposeOfVisit { get; set; }

        public string IsIDCardReturned { get; set; }

        public List<GroupModel> Groups { get; set; }

        public List<CenterModel> Centers { get; set; }

        public List<ReportingOfficerListModel> EmployeeListModel { get; set; }

        public List<VisitorPassVisitorsModel> VisitorsModel { get; set; }
    }
}
