using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Digitalization.Models.AdminModel
{
    public class EmployeeModel
    {
        public int StaffNumber { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string GroupName { get; set; }

        public int GroupID { get; set; }

        public string CenterName { get; set; }

        public int CenterID { get; set; }

        public DateTime DateOfJoining { get; set; }

        public string Designation { get; set; }

        public string TechNonTech { get; set; }

        public DateTime DateOfBirth { get; set; }

        public DateTime DateOfRetirement { get; set; }

        public string CasteCategory { get; set; }

        public string Gender { get; set; }

        public string EmployeeGroup { get; set; }

        public int CurrentBasicPay { get; set; }

        public int PayMatrix { get; set; }

        public string EntryLevel { get; set; }

        public string MobilePhoneNumber { get; set; }

        public string PersonalEmailId { get; set; }

        public string OfficialEmailId { get; set; }

        public string PresentAddress { get; set; }

        public string PermanentAddress { get; set; }

        public string PAN { get; set; }

        public string AccountNumber { get; set; }

        public string AadharNumber { get; set; }

        public string BloodGroup { get; set; }

        public string SpouseInGovernment { get; set; }

        public char Status { get; set; }

        public string UserRemarks { get; set; }

        public string Remarks { get; set; }

        public string EmployeeCategory { get; set; }

        public string MaritalStatus { get; set; }

        public bool IsPhotoAvailable { get; set; }

        public string UserImagePath { get; set; }

        public int IsHRAApplicable { get; set; }

        public int IsExServiceMen { get; set; }

        public int IsMinorities { get; set; }

        public string Minorities { get; set; }

        public int MinorityID { get; set; }

        public int IsPhysicallyHandicapped { get; set; }

        public int PhysicallyHandicappedID { get; set; }

        public string PhysicallyHandicapped { get; set; }

        public int ReportingOfficer { get; set; }

        public string ReportingOfficerName { get; set; }

        public List<GroupModel> Groups { get; set; }

        public List<CenterModel> Centers { get; set; }

        public List<DesignationModel> Designations { get; set; }

        public List<EmergencyContactModel> EmergencyContactModel { get; set; }

        public List<FamilyDeclarationModel> FamilyDeclarationModel { get; set; }

        public List<MedicalDependentsModel> MedicalDependentsModel { get; set; }

        public List<GratuityModel> GratuityModel { get; set; }

        public List<QualificationModel> QualificationModel { get; set; }

        public List<MinorityModel> MinorityModel { get; set; }

        public List<PhysicallyHandicappedModel> PhysicallyHandicappedModel { get; set; }

        public List<EmployeeListModel> EmployeeListModel { get; set; }

        public string WorkingStatus { get; set; }

        public string RetiredDate { get; set; }
    }
}
