using System;
using System.Collections.Generic;

namespace Digitalization.Models.UserModel
{
    public class ContingentBillModel
    {
        public string ContingentBillID { get; set; }

        public string Type { get; set; }

        public string ProjectName { get; set; }

        public string AccountHead { get; set; }

        public string NetPaymentRs { get; set; }

        public string NetPaymentinrs { get; set; }

        public string NetPaymentinps { get; set; }

        public string PayRupees { get; set; }

        public string AccountProjectName { get; set; }

        public string BudgetHead { get; set; }

        public string AdvanceDrawn { get; set; }

        public string AmountSpent { get; set; }

        public string NetAmountRefundable { get; set; }

        public string RTGSOnDated { get; set; }

        public string AmountingRs { get; set; }

        public string AmountingPs { get; set; }

        public string Amount { get; set; }

        public string OldStatus { get; set; }

        public string NewStatus { get; set; }

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

        public string OldRemarks { get; set; }

        public string NewRemarks { get; set; }

        public DateTime PreparedDateTime { get; set; }

        public string DateTime { get; set; }

        public List<ContingentBillItemsModel> Items { get; set; }

        public List<ReportingOfficerListModel> ReportingOfficerListModel { get; set; }
    }
}
