using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Digitalization.Models.UserModel
{
    public class ResetPasswordModel
    {
        public int StaffNumber { get; set; }

        public string SystemPassword { get; set; }

        public string NewPassword { get; set; }

        public string ConfirmPassword { get; set; }
    }
}
