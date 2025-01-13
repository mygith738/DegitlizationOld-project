using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Digitalization.Models.UserModel
{
    public class LeaveModel
    {
        public string LeaveID { get; set; }

        public DateTime LeaveRaisedDateTime { get; set; }

        public int StaffNumber { get; set; }

        public string EmployeeType { get; set; }

        public string StaffFirstName { get; set; }

        public string StaffDesignation { get; set; }

        public string GroupName { get; set; }

        public string CenterName { get; set; }

        public int CenterID { get; set; }

        public int GroupID { get; set; }

        public string AddresOnLeave { get; set; }

        public string Department { get; set; }

        public string TypeOfLeave { get; set; }

        public string LeaveFrom { get; set; }

        public string FromHalf { get; set; }

        public DateTime AlternateLeaveFrom { get; set; }

        public string LeaveTo { get; set; }

        public string ToHalf { get; set; }

        public double TotalLeave { get; set; }

        public string Reason { get; set; }

        public string OldRemarks { get; set; }

        public string NewRemarks { get; set; }

        public string OldStatus { get; set; }

        public string NewStatus { get; set; }

        public int ActionTakenByStaffNumber { get; set; }

        public string ActionTakenByName { get; set; }

        public string ActionTakenByDesignation { get; set; }

        public int ReportingOfficer { get; set; }

        public string ReportingOfficerName { get; set; }

        public string ReportingOfficerDesignation { get; set; }

        public DateTime DateTime { get; set; }

        public string EntryLevel { get; set; }

        public string AttchmentLocation { get; set; }

        public string AlteredAttchmentLocation { get; set; }

        public int RecieverStaffNumber { get; set; }

        public string RecieverName { get; set; }

        public string RecieverDesignation { get; set; }

        public string OverTimeID { get; set; }

        public List<ReportingOfficerListModel> ReportingOfficerListModel { get; set; }

        public List<EmployeeListModel> EmployeeList { get; set; }

        public List<OverTimeModel> CompensatoryOffModel { get; set; }
    }
}
