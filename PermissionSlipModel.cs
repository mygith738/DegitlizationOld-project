using System;
using System.Collections.Generic;

namespace Digitalization.Models.AuthorityModel
{
    public class PermissionSlipModel
    {
        public string PermissionID { get; set; }

        public int StaffNumber { get; set; }

        public string StaffName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string StaffDesignation { get; set; }

        public int GroupID { get; set; }

        public int CenterID { get; set; }

        public string GroupName { get; set; }

        public string CenterName { get; set; }

        public string PermissionType { get; set; }

        public string AlternateDate { get; set; }

        public DateTime Date { get; set; }

        public DateTime FromTime{ get; set; }

        public string AlternateFromTime{ get; set; }

        public DateTime ToTime { get; set; }

        public string AlternateToTime { get; set; }

        public string TotalDuration { get; set; }

        public string Reason { get; set; }

        public string OldRemarks { get; set; }

        public string NewRemarks { get; set; }

        public string OldStatus { get; set; }

        public string NewStatus { get; set; }

        public DateTime ExitTime { get; set; }

        public string AlternateExitTime { get; set; }

        public int ReportingOfficer { get; set; }

        public string ReportingOfficerName { get; set; }

        public string ReportingOfficerDesignation { get; set; }

        public DateTime PermissionRaisedDateTime { get; set; }

        public int ActionTakenByStaffNumber { get; set; }

        public string ActionTakenByName { get; set; }

        public string ActionTakenByDesignation { get; set; }

        public string DateTime { get; set; }

        public int PermissionCount { get ; set; }

        public int TotalMinutes { get ; set; }

        public int RemainingPermissionCount { get ; set; }

        public int RecieverStaffNumber { get ; set; }

        public string RecieverName { get; set; }

        public string RecieverDesignation { get; set; }

        public List<ReportingOfficerListModel> ReportingOfficerListModel { get; set; }
    }
}
