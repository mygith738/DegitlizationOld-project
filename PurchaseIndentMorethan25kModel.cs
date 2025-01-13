using System;
using System.Collections.Generic;

namespace Digitalization.Models.UserModel
{
    public class PurchaseIndentMorethan25kModel
    {
        public int IndentorStaffNumber { get; set; }

        public string IndentorName { get; set; }

        public string IndentorDesignation { get; set; }

        public string IndentorGroupName { get; set; }

        public int IndentorGroupID { get; set; }

        public string IndentorCenterName { get; set; }

        public int IndentorCenterID { get; set; }

        public string PurchaseIndentID { get; set; }

        public string PreparedDateTime { get; set; }

        public int ReportingOfficerStaffNumber { get; set; }

        public string ReportingOfficerName { get; set; }

        public string ReportingOfficerDesignation { get; set; }

        public string Department { get; set; }

        public string ProjectHead { get; set; }

        public string TechnicalSpecificationAttachmentLocation { get; set; }

        public string AttachmentAbove15L { get; set; }

        public string IsProprietaryArticleEnclosed { get; set; }

        public string ProprietaryArticleAttachmentLocation { get; set; }

        public string IsCGCApprovalAttached { get; set; }

        public string CGCApprovalAttachmentLocation { get; set; }

        public string RequirementType { get; set; }

        public string IsRequirementImported { get; set; }

        public string DeliveryRequiredTime { get; set; }

        public string BudgetProvision { get; set; }

        public string FundAvailability { get; set; }

        public string BudgetHead { get; set; }

        public string OperationalPeriodRequiredForEquipment { get; set; }

        public string AMC { get; set; }

        public string Training { get; set; }

        public string TrainingLocation { get; set; }

        public string TrainingTime { get; set; }

        public string AcceptanceCriteria { get; set; }

        public string IsInstallationSiteUtilitiesReady { get; set; }

        public DateTime ApproximateDateForInstallationUtilities { get; set; }

        public string ItemAvailabilityInStores { get; set; }

        public string AvailableVendors { get; set; }

        public string ProjectType { get; set; }

        public string ProcessingDepatiment { get; set; }

        public int ProjectLeader { get; set;}
     

        public List<PurchaseIndentItemsModel> PurchaseIndentItems { get; set; }

        public List<ReportingOfficerListModel> ReportingOfficerModel { get; set; }
    }
}
