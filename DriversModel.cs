using System;
using System.Collections.Generic;

namespace Digitalization.Models.CommonModel
{
    public class DriversModel
    {
        public string EmployeeType { get; set; }

        public int StaffNumber { get; set; }

        public string Name { get; set; }

        public string TempName { get; set; }

        public string Designation { get; set; }

        public int GroupID { get; set; }

        public int CenterID { get; set; }

        public string AssignedShift { get; set; }

        public string AlternateFromDate { get; set; }

        public string AlternateToDate { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public string ShiftType { get; set; }

        public string WorkingHours { get; set; }

        public TimeSpan WorkingFrom { get; set; }

        public TimeSpan WorkingTo { get; set; }

        public string AlternateWorkingFrom { get; set; }

        public string AlternateWorkingTo { get; set; }

        public string LunchHours { get; set; }

        public DateTime WeeklyOff { get; set; }

        public string AlternateWeeklyOff { get; set; }

        public List<DriversModel> DriversShift { get; set; }
    }
}
