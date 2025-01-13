using System;
using System.Collections.Generic;

namespace Digitalization.Models.AuthorityModel
{
    public class PurchaseIndentModelUpto25Model
    {
        public int IndentorStaffNumber { get; set; }

        public string IndentorName { get; set; }

        public string IndentorDesignation { get; set; }

        public string IndentorGroupName { get; set; }

        public int IndentorGroupID { get; set; }

        public string IndentorCenterName { get; set; }

        public int IndentorCenterID { get; set; }

        public string PurchaseIndentID { get; set; }

        public DateTime PreparedDateTime { get; set; }

        public int ReportingOfficerStaffNumber { get; set; }
        public int ProjectLeaderStaffNumber { get; set; }

        public string ReportingOfficerName { get; set; }

        public string ReportingOfficerDesignation { get; set; }

        public string MaterialRequired { get; set; }

        public string MaterialQuantity { get; set; }

        public string TSSAttachmentLocation { get; set; }

        public string AlteredTSSAttachmentLocation { get; set; }

        public string ProjectNumber { get; set; }

        public string BudgetHead { get; set; }

        public string EstimatedCost { get; set; }

        public string ProcessingDepartment { get; set; }

        public string PossibleSource { get; set; }

        public string ProjectType { get; set; }

        public string NoStockCertificateAttachment { get; set; }

        public string UtilizingDepartment { get; set; }

        public string SanctionedRupeesInNumber { get; set; }

        public string SanctionedRupeesInText { get; set; }

        public string MaterialInvoiceNumber { get; set; }

        public DateTime MaterialInvoiceDate { get; set; }

        public string MaterialDescription { get; set; }

        public string CertifiedPrefix { get; set; }

        public string CertifiedName { get; set; }

        public DateTime MaterialReceivedDate { get; set; }

        public string RegisterReferenceNumber { get; set; }

        public string PurchaseVoucherNumber { get; set; }

        public DateTime PurchaseVoucherDate { get; set; }

        public string PurchaseVoucherInvoiceNumber { get; set; }

        public DateTime PurchaseVoucherInvoiceDate { get; set; }

        public string PurchaseAmount { get; set; }

        public string ReferenceTDS { get; set; }

        public string ActualAmount { get; set; }

        public string TransferredTo { get; set; }

        public string ChequeRefNo { get; set; }

        public string SAOSignature { get; set; }

        public string OldStatus { get; set; }

        public string OldRemarks { get; set; }

        public string NewStatus { get; set; }

        public string NewRemarks { get; set; }

        public bool IsAttachmentPresent { get; set; }

        public int ActionTakenByStaffNumber { get; set; }

        public string ActionTakenByName { get; set; }

        public string ActionTakenByDesignation { get; set; }

        public DateTime DateTime { get; set; }

        public List<ReportingOfficerListModel> ReportingOfficerModel { get; set; }

        public List<ProjectLeaderListModel> ProjectLeaderModel { get; set; }

        public string PRNumber { get; set; }
    }
}
