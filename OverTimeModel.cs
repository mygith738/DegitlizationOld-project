
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Digitalization.Models.UserModel
{
    public class OverTimeModel
    {
        public string OverTimeID { get; set; }

        public string CashClaimID { get; set; }

        public int StaffNumber { get; set; }

        public string StaffName { get; set; }

        public string Designation { get; set; }

        public int GroupID { get; set; }

        public int CenterID { get; set; }

        public string GroupName { get; set; }

        public string CenterName { get; set; }

        public int ReportingOfficer { get; set; }

        public string ReportingOfficerName { get; set; }

        public string ReportingOfficerDesignation { get; set; }

        public string ActionTaken { get; set; }

        public int ActionTakenByStaffNumber { get; set; }

        public string ActionTakenByName { get; set; }

        public string ActionTakenByDesignation { get; set; }

        public DateTime ApplyingOn { get; set; }

        public DateTime OverTimeRaisedDateTime { get; set; }

        public DateTime CashClaimRaisedDateTime { get; set; }

        public string AlteredApplyingOnDate { get; set; }

        public DateTime FromTime { get; set; }

        public DateTime ToTime { get; set; }

        public string DateTime { get; set; }

        public string TotalTime { get; set; }

        public string AlternateFromTime { get; set; }

        public string AlternateToTime { get; set; }

        public string OldRemarks { get; set; }

        public string NewRemarks { get; set; }

        public string OldStatus { get; set; }

        public string NewStatus { get; set; }

        public string TempVariable { get; set; }

        public List<ReportingOfficerListModel> ReportingOfficerListModel { get; set; }

    }
}
