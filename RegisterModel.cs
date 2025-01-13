using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Digitalization.Models.AdminModel
{
    public class RegisterModel
    {
        public int StaffNumber { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public int GroupId { get; set; }

        public int CenterId { get; set; }

        public string EmployeeGroup { get; set; }

        public string DateOfJoining { get; set; }

        public string Designation { get; set; }

        public string TechNonTech { get; set; }

        public string DateOfBirth { get; set; }

        public string DateOfRetirement { get; set; }

        public string CasteCategory { get; set; }

        public string Gender { get; set; }

        public string CurrentBasicPay { get; set; }

        public string PayMatrix { get; set; }

        public string EntryLevel { get; set; }

        public string MobilePhoneNumber { get; set; }

        public string PersonalEmailId { get; set; }

        public string OfficialEmailId { get; set; }

        public string PresentAddress { get; set; }

        public string PermanentAddress { get; set; }

        public string PAN { get; set; }

        public string AadharNumber { get; set; }

        public string AccountNumber { get; set; }

        public string BloodGroup { get; set; }

        public string SpouseInGovernment { get; set; }

        public int EmployeeCategory { get; set; }

        public char MaritalStatus { get; set; }

        public string Remarks { get; set; }

        public string UserImagePath { get; set; }

        public int IsHRAApplicable { get; set; }

        public int IsExServiceMen { get; set; }

        public int IsMinorities { get; set; }

        public string Minorities { get; set; }

        public int IsPhysicallyHandicapped { get; set; }

        public string PhysicallyHandicapped { get; set; }

        public string EmergncyContactName { get; set; }

        public string EmergncyRelation { get; set; }

        public string EmergncyMobilePhoneNumber { get; set; }

        public string EmergncyLandlineNumber { get; set; }

        public string EmergncyEmailId { get; set; }

        public string ReportingOfficer { get; set; }

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
    }
}
