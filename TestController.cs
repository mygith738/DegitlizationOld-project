using Digitalization.Models;
using Digitalization.Models.UserModel;
using Digitalization.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using UAParser;

namespace Digitalization.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Attendance()
        {
            return View();
        }
        
        [HttpGet]
        public IActionResult AttendanceData(DateTime FromDate, DateTime ToDate, int StaffNumber)
        {
            if (ToDate > DateTime.Now)
            {
                ToDate = DateTime.Now;
            }

            List<AttendanceModel> attendances = new List<AttendanceModel>();

            try
            {
                var configuration = CommonHelper.GetConfig();
                var connectionString = configuration["ConnectionStrings:Attendance"];

                bool isserverrunning = CommonHelper.IsAttendanceServerRunning(connectionString);

                if (!(isserverrunning))
                {
                    return RedirectToAction("ServerError", "User");
                }

                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

                //int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int Year = DateTime.Now.Year;

                AttendanceModel model = new AttendanceModel();

                //int StaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                int Dates;

                if ((FromDate.ToString("dd/MM/yyyy") == "01-01-0001" && ToDate.ToString("dd/MM/yyyy") == "01-01-0001") ||
                    (FromDate.ToString("dd/MM/yyyy") == "01-01-2001" && ToDate.ToString("dd/MM/yyyy") == "01-01-2001") ||
                    (FromDate.ToString("dd/MM/yyyy") == "1-1-2001" && ToDate.ToString("dd/MM/yyyy") == "1-1-2001") ||
                    (FromDate.ToString("dd/MM/yyyy") == "1-1-0001" && ToDate.ToString("dd/MM/yyyy") == "1-1-0001") ||
                    (FromDate.ToString("dd/MM/yyyy") == "01/01/0001" && ToDate.ToString("dd/MM/yyyy") == "01/01/0001"))
                {
                    Dates = 7 * (-1);
                    ToDate = DateTime.Now.AddDays(-1);

                    ViewBag.FromDate = DateTime.Now.Date.AddDays(Dates - 1);
                    ViewBag.ToDate = DateTime.Now.AddDays(-1);
                }
                else
                {
                    Dates = Convert.ToInt32((ToDate - FromDate).TotalDays);
                    Dates = Dates * -1;

                    ViewBag.FromDate = FromDate;
                    ViewBag.ToDate = ToDate;
                }

                for (int i = Dates; i <= 0; i++)
                {
                    model.FromDate = ToDate.AddDays(i).Day;
                    model.FromMonth = ToDate.AddDays(i).Month;
                    model.FromYear = ToDate.AddDays(i).Year;

                    string AlternateFromDate;
                    string AlternateFromMonth;

                    if ((Math.Floor(Math.Log10(model.FromDate) + 1)) == 1)
                    {
                        AlternateFromDate = "0" + model.FromDate.ToString();
                    }
                    else
                    {
                        AlternateFromDate = model.FromDate.ToString();
                    }

                    if ((Math.Floor(Math.Log10(model.FromMonth) + 1)) == 1)
                    {
                        AlternateFromMonth = "0" + model.FromMonth.ToString();
                    }
                    else
                    {
                        AlternateFromMonth = model.FromMonth.ToString();
                    }

                    try
                    {
                        model.AlternateDate = DateTime.Parse(AlternateFromDate + "-" + AlternateFromMonth + "-" + model.FromYear);
                    }
                    catch (Exception ex)
                    {
                        model.AlternateDate = DateTime.Parse(AlternateFromMonth + "-" + AlternateFromDate + "-" + model.FromYear);
                    }

                    model.Date = model.AlternateDate.ToString("dd/MM/yyyy");


                    AttendanceModel results = UserHelper.GetAttendanceData(model, StaffNumber, BrowserName, IPAddress);

                    attendances.Add(results);
                }

                foreach (var attedance in attendances)
                {
                    attedance.Day = attedance.AlternateDate.DayOfWeek.ToString();

                    List<HolidaysModel> closedholidays = UserHelper.GetHolidays(IPAddress, StaffNumber, "CH", DateTime.Now.Year);
                    //List<HolidaysModel> restrictedholidays = CommonHelper.GetRestrictedHolidays(IPAddress, SessionStaffNumber);

                    foreach (var closedholiday in closedholidays)
                    {
                        if (attedance.Date == closedholiday.Date)
                        {
                            attedance.ActionTaken = "CLOSED HOLIDAY";
                        }
                    }

                    if (attedance.PunchIn.Year.ToString() != "1")
                    {
                        attedance.Duration = Convert.ToInt32(attedance.PunchOut.Subtract(attedance.PunchIn).TotalMinutes);

                        attedance.PunchInTime = attedance.PunchIn.ToString("hh:mm tt");
                        attedance.PunchOutTime = attedance.PunchOut.ToString("hh:mm tt");

                        attedance.AlternatePunchInTime = attedance.PunchIn.TimeOfDay;
                        attedance.AlternatePunchOutTime = attedance.PunchOut.TimeOfDay;

                        TimeSpan eightfourty = TimeSpan.Parse("08:40:59");
                        TimeSpan fivethirty = TimeSpan.Parse("17:30:59");
                        TimeSpan five = TimeSpan.Parse("17:00:59");
                        TimeSpan nine = TimeSpan.Parse("09:00:59");

                        AttendanceModel shifttimings = new AttendanceModel();

                        if (HttpContext.Session.GetString("Role") == "Driver")
                        {
                            shifttimings = UserHelper.GetDriverShiftBasedAttendance(StaffNumber, IPAddress, attedance.AlternateDate);
                        }
                        else if (HttpContext.Session.GetString("Role") == "Security")
                        {
                            shifttimings = UserHelper.GetSecurityShiftBasedAttendance(StaffNumber, IPAddress, attedance.AlternateDate);
                        }

                        attedance.AlternateShiftFrom = shifttimings.AlternateShiftFrom;
                        attedance.AlternateShiftTo = shifttimings.AlternateShiftTo;
                        attedance.WeeklyOff = shifttimings.WeeklyOff;
                        attedance.Shift = shifttimings.Shift;
                        ViewBag.Shift = shifttimings.Shift;
                    }
                    else
                    {
                        attedance.PunchInTime = "-";
                        attedance.PunchOutTime = "-";
                    }
                }
            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                //int StaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, StaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(attendances);
        }
    }
}
