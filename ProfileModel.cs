using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Digitalization.Models.UserModel
{
    public class ProfileModel
    {
        public EmployeeModel Employee { get; set; }

        public List<EmergencyContactModel> EmergencyContact { get; set; }

        public List<FamilyDeclarationModel> FamilyDeclaration { get; set; }

        public List<MedicalDependentsModel> MedicalDependents { get; set; }

        public List<QualificationModel> Qualifications { get; set; }

        public List<GratuityModel> Gratuity { get; set; }
    }
}
