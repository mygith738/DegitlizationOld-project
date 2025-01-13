using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Digitalization.Models.AdminModel
{
    public class FamilyDeclarationModel
    {
        public int StaffNumber { get; set; }

        public string RelativeName { get; set; }

        public string Relation { get; set; }

        public DateTime DateOfBirth { get; set; }

        public int DependentID { get; set; }

        public int IsDependent { get; set; }
    }
}
