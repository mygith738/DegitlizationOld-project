using System;
using System.Collections.Generic;

namespace Digitalization.Models.UserModel
{
    public class AttendanceModel
    {
        public int FromDate { get; set; }

        public string FromDay { get; set; }

        public int FromMonth { get; set; }

        public int FromYear { get; set; }

        public int ToDate { get; set; }

        public string ToDay { get; set; }

        public int ToMonth { get; set; }

        public int ToYear { get; set; }

        public string Date { get; set; }

        public DateTime AlternateDate { get; set; }

        public string Day { get; set; }

        public DateTime PunchIn { get; set; }

        public string PunchInTime { get; set; }

        public TimeSpan AlternatePunchInTime { get; set; }

        public DateTime PunchOut { get; set; }

        public string PunchOutTime { get; set; }

        public TimeSpan AlternatePunchOutTime { get; set; }

        public int Duration { get; set; } 

        public string IfNotPunched { get; set; }

        public string ActionTaken { get; set; }

        public DateTime ShiftFromDate { get; set; }

        public DateTime ShiftToDate { get; set; }

        public string Shift { get; set; }

        public DateTime ShiftFrom { get; set; }

        public DateTime ShiftTo { get; set; }

        public string AlternateShiftFrom { get; set; }

        public string AlternateShiftTo { get; set; }

        public DateTime WeeklyOff { get; set; }

        public string AlternateWeeklyOff { get; set; }

        public string Direction { get; set; }
    }
}
