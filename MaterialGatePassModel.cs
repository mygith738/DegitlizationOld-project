using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Digitalization.Models.AuthorityModel
{
    public class MaterialGatePassModel
    {
        public string GatePassID { get; set; }

        public int PreparedByStaffNumber { get; set; }

        public string PreparedByFirstName { get; set; }

        public string PreparedByDesignation { get; set; }

        public string PreparedByGroupName { get; set; }

        public string PreparedByCenterName { get; set; }

        public string Address { get; set; }

        public string Purpose { get; set; }

        public int ReturnType { get; set; }

        public string OldRemarks { get; set; }

        public string NewRemarks { get; set; }

        public string OldStatus { get; set; }

        public string NewStatus { get; set; }

        public int ActionTakenByStaffNumber { get; set; }

        public string ActionTakenByName { get; set; }

        public string ActionTakenByDesignation { get; set; }

        public DateTime PreparedDateTime { get; set; }

        public DateTime DateTime { get; set; }

        public int ReportingOfficer { get; set; }

        public string ReportingOfficerName { get; set; }

        public string ReportingOfficerDesignation { get; set; }

        public string TakenByName { get; set; }

        public string TakenByPhoneNumber { get; set; }

        public string TakenByCompanyName { get; set; }

        public string AlternateTakenByName { get; set; }

        public string AlternateTakenByPhoneNumber { get; set; }

        public string AlternateTakenByCompanyName { get; set; }

        public string SignatureFileLocation { get; set; }

        public int AuthorityStaffNumber { get; set; }

        public string AuthorityName { get; set; }

        public string AuthorityDesignation { get; set; }

        public int SecurityStaffNumber { get; set; }

        public string SecurityName { get; set; }

        public string SecurityDesignation { get; set; }

        public string IsSigned { get; set; }

        public bool IsAttachmentPresent { get; set; }

        public string AttachmentLocation { get; set; }

        public string InAttachmentLocation { get; set; }

        public string AlteredInAttachementLocation { get; set; }

        public List<MaterialGatePassItemsModel> Items { get; set; }

        public List<ReportingOfficerListModel> ReportingOfficerListModel { get; set; }
    }
}
