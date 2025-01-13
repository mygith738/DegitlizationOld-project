using System.Collections.Generic;

namespace Digitalization.Models
{
    public class HolidaysModel
    {
        public int SlNo { get; set; }

        public string Date { get; set; }

        public string Day { get; set; }

        public string Occassion { get; set; }

        public string TypeOfHoliday { get; set; }

        public string Year { get; set; }

        public List<HolidaysModel> HolidayList { get; set; }
    }
}
