using Digitalization.Models.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Digitalization.Models.AuthorityModel
{
    public class VehicleIndentModel
    {
        public string IndentID { get; set; }

        public DateTime IndentRaisedDateTime { get; set; }

        public int SenderStaffNumber { get; set; }

        public string SenderFirstName { get; set; }

        public string SenderDesignation { get; set; }

        public int RecieverStaffNumber { get; set; }

        public string RecieverName { get; set; }

        public string RecieverDesignation { get; set; }

        public int SenderGroupID { get; set; }

        public string SenderGroupName { get; set; }

        public int SenderCenterID { get; set; }

        public string SenderCenterName { get; set; }

        public string OldIndentStatus { get; set; }

        public string NewIndentStatus { get; set; }

        public DateTime DateTime { get; set; }

        public string OldRemarks { get; set; }

        public string NewRemarks { get; set; }

        public int ActionTakenByStaffNumber { get; set; }

        public string ActionTakenByName { get; set; }

        public string ActionTakenByDesignation { get; set; }

        public DateTime IndentRequiredDate { get; set; }

        public string AlteredIndentRequiredDate { get; set; }

        public TimeSpan IndentRequiredTime { get; set; }

        public string AlteredIndentRequiredTime { get; set; }

        public string VehicleFor { get; set; }

        public string Organization { get; set; }

        public int Distance { get; set; }

        public int DurationOfWork { get; set; }

        public int ReportingOfficerStaffNumber { get; set; }

        public string ReportingOfficerName { get; set; }

        public string ReportingOfficerDesignation { get; set; }

        public string VehicleNumber { get; set; }

        public int DriverStaffNumber { get; set; }

        public TimeSpan ReturnTiming { get; set; }

        public string AlternateReturnTiming { get; set; }

        public List<DriversModel> EmployeeList { get; set; }

        public List<EmployeeListModel> IndentInvolvedPersons { get; set; }

        public List<EmployeeListModel> GroupEmployees { get; set; }

        public List<ReportingOfficerListModel> ReportingOfficerListModel { get; set; }
    }
}
