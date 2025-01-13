using Digitalization.Models;
using Digitalization.Models.UserModel;
using Digitalization.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UAParser;

namespace Digitalization.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _hostingenvironment;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment hostingenvironment)
        {
            _logger = logger;
            _hostingenvironment = hostingenvironment;
            CommonHelper.Initialize(_hostingenvironment);
        }

        public IActionResult Test()
        {
            ViewBag.CurrentTime = DateTime.Now.TimeOfDay;
            ViewBag.CurrentTimeTemp = DateTime.Now.ToString("h:mm tt");

            //ViewBag.IPAddress = CommonHelper.GetIPAddress();
            ViewBag.IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.HostName = CommonHelper.GetHostName(HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString(), 0000);
            string MACAddress = CommonHelper.GetMacAddress(HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString()).ToUpper();
            ViewBag.MACAddress = MACAddress;
            //var configuration = CommonHelper.GetConfig();
            //var log = configuration["FileLocations:SignatureApplication"];
            //log = log.Replace("G:\\Mokshith\\SignatureApplication\\", "../SignatureApplication/");
            //ViewBag.SignatureApplication = log;
            //Console.WriteLine(ViewBag.SignatureApplication);
            ViewBag.Date=DateTime.Now;
            try
            {
                try
                {
                    int a = 0;
                    int b = 1;
                    int c = b / a;
                }
                catch (Exception e)
                {
                    CommonHelper.LogError(e, 0000, HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString());
                }
                ViewBag.IsErrorLogged = "True";
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                ViewBag.IsErrorLogged = "False";
            }

            return View();
        }
        
        // to create login for all employees
        public IActionResult SendPassword()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            // to get browser name
            var userAgent = HttpContext.Request.Headers["User-Agent"];
            var uaParser = Parser.GetDefault();
            ClientInfo c = uaParser.Parse(userAgent);
            string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

            List<SendPasswordModel> model = new List<SendPasswordModel>();

            //model = TestHelper.CreateAndSendPassword(IPAddress, BrowserName);

            return View(model);
        }

        public IActionResult Index()
        {
            try
            {
                // to insert leavecount
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

                //AttendanceModel shifttimings = UserHelper.GetShiftBasedAttendance(1935, IPAddress, DateTime.Now.Date);

                string Date = DateTime.Now.Day.ToString();
                string Month = DateTime.Now.Month.ToString();

                //if (Date == "1" && (Month == "7" || Month == "1"))
                //if (true)
                //{
                //    // function to insert leave half yearly
                //    string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

                //    // to get browser name
                //    var userAgent = HttpContext.Request.Headers["User-Agent"];
                //    var uaParser = Parser.GetDefault();
                //    ClientInfo c = uaParser.Parse(userAgent);
                //    string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                //    //AdminHelper.InsertLeave(IPAddress, 0000, BrowserName);
                //    AdminHelper.InsertPermissionMonthly(IPAddress, 0000, BrowserName);
                //}

                // Clearing sessions
                HttpContext.Session.SetInt32("StaffNumber", 0);
                HttpContext.Session.SetString("Role", "");
                HttpContext.Session.SetString("IsDirector", "");
                HttpContext.Session.SetString("IsProfileAuthenticated", "");
                HttpContext.Session.SetString("OTPSent", "");
                HttpContext.Session.SetInt32("OTP", 0);
                HttpContext.Session.SetString("ShortName", "");
                HttpContext.Session.SetString("Name", "");
                HttpContext.Session.SetString("EmployeeGroup", "");
                HttpContext.Session.SetString("Gender", "");

                HttpContext.Session.SetString("IsProfileAuthenticated", "");
                HttpContext.Session.SetString("OTPSent", "");
                HttpContext.Session.SetInt32("OTP", 0);
                HttpContext.Session.SetString("Name", "");
                HttpContext.Session.SetInt32("StaffNumber", 0);
                HttpContext.Session.SetString("Name", "");
                HttpContext.Session.SetString("Role", "");
                HttpContext.Session.SetString("IsDirector", "");
                HttpContext.Session.SetString("ShortName", "");
                HttpContext.Session.SetString("EmployeeGroup", "");
                HttpContext.Session.SetString("EmployeeType", "");
                HttpContext.Session.SetString("Gender", "");
                HttpContext.Session.SetString("MaritalStatus", "");
                HttpContext.Session.SetString("Designation", "");
                HttpContext.Session.SetString("PhysicallyHandicapped", "");
            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                ViewBag.ExceptionError = "Something went wrong";
                CommonHelper.LogError(error, 0000, IPAddress);
                Console.WriteLine(error.Message);
            }

            // redirecting to user index
            return Redirect("~/User/Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}