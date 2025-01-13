using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Digitalization.Models.AdminModel
{
    public class SearchModel
    {
        public int StaffNumber { get; set; }

        public int CenterID { get; set; }

        public int GroupID { get; set; }

        public List<GroupModel> Groups { get; set; }

        public List<CenterModel> Centers { get; set; }
    }
}
