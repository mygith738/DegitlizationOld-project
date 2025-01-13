using Digitalization.Models;
using Digitalization.Models.UserModel;
using Digitalization.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UAParser;

namespace Digitalization.Controllers
{
    public class UserController : Controller
    {
        private readonly IWebHostEnvironment _hostingenvironment;

        public UserController(IWebHostEnvironment hostingenvironment)
        {
            _hostingenvironment = hostingenvironment;
        }

        // Common tasks start
        // ***************************************************************************************************//

        public IActionResult Index()
        {
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") != 0)
                {
                    return RedirectToAction("Home", "User");
                } 
            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                CommonHelper.LogError(error, 0000, IPAddress);
                Console.WriteLine(error.Message);
            }
            //return View();
            return Redirect("~/index.html");
        }
        
        // view break

        [HttpGet]
        public IActionResult Login()
        {
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") != 0)
                {
                    return RedirectToAction("Home", "User");
                }
                if (TempData["SuccessMessage"] != null)
                {
                    ViewBag.SuccessMessage = TempData["SuccessMessage"];
                    TempData["SuccessMessage"] = null;
                }
            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                CommonHelper.LogError(error, 0000, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (model.StaffNumber == 0)
                {
                    ViewBag.StaffNumberError = "Please enter staffnumber";
                    return View();
                }
                if (model.Password == "" || model.Password == null)
                {
                    ViewBag.PasswordError = "Please enter password";
                    return View();
                } 
                
                LoginModel loginmodel = new LoginModel();

                // check if user logging in for first name
                bool IsFirstLogin = UserHelper.CheckFirstLogin(model.StaffNumber, IPAddress);

                bool IsUserExists = CommonHelper.CheckUserExists(model.StaffNumber, IPAddress);

                // check user exist or not
                if(!(IsUserExists))
                {
                    // if login failed move to Login page
                    //Console.WriteLine("Invalid Login attempt");
                    ViewBag.ExceptionError = "Invalid login attempt";
                    return View();
                }

                // if first time check password from PasswordRecovery
                else if (IsFirstLogin) 
                { 
                    // redirect to validate for first login
                    bool FirstLoginResult = UserHelper.ValidateFirstLogin(model, IPAddress);
                    if (FirstLoginResult)
                    {
                        // if login successful redirect to Reset Password
                        HttpContext.Session.SetInt32("StaffNumber", model.StaffNumber);
                        return RedirectToAction("ResetPassword", "User");
                    }

                    // if login failed move to Login page
                    //Console.WriteLine("Invalid Login attempt");
                    ViewBag.ExceptionError = "Invalid login attempt";
                    return View();
                }
                // else check password from Login
                else
                {
                    loginmodel = UserHelper.ValidateNormalLogin(model, IPAddress);
                }

                if (loginmodel.StaffNumber != 0)
                {
                    //Console.WriteLine("Login successful");
                    ViewBag.Message = "Login successful";

                    // creating sessions
                    HttpContext.Session.SetInt32("StaffNumber", loginmodel.StaffNumber);
                    HttpContext.Session.SetString("Name", loginmodel.Name);
                    HttpContext.Session.SetString("Role", loginmodel.RoleName);
                    if(loginmodel.IsDirector)
                    {
                        HttpContext.Session.SetString("IsDirector", "True");
                    }
                    HttpContext.Session.SetString("ShortName", loginmodel.ShortName);
                    HttpContext.Session.SetString("EmployeeGroup", loginmodel.EmployeeGroup);
                    if(loginmodel.EmployeeType == "Regular Employee")
                    {
                        HttpContext.Session.SetString("EmployeeType", "Regular");
                    }
                    else
                    {
                        HttpContext.Session.SetString("EmployeeType", "Contract");
                    }
                    HttpContext.Session.SetString("Gender", loginmodel.Gender);
                    HttpContext.Session.SetString("MaritalStatus", loginmodel.MaritalStatus);
                    HttpContext.Session.SetString("Designation", loginmodel.Designation);
                    HttpContext.Session.SetString("PhysicallyHandicapped", loginmodel.PhysicallyHandicapped);
                    //Console.WriteLine(loginmodel.RoleName);
                    return RedirectToAction("Home", "User");
                }
                else
                {
                    //Console.WriteLine("Invalid Login attempt");
                    ViewBag.ExceptionError = "Invalid login attempt";
                    return View();
                }
            }
            catch (Exception error)
            {
                CommonHelper.LogError(error, model.StaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            ViewBag.ExceptionError = "An error occured. Please contact system administrator";
            return View();

        }

        // view break

        public IActionResult Home()
        {
            List<AttendanceModel> attendances = new List<AttendanceModel>();
            
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int Year = DateTime.Now.Year;
                string Status = "Null";

                // to get outbox IDR Indent Request count
                List <IndentModel> outboxindentrequest = new List<IndentModel>();
                outboxindentrequest = UserHelper.GetOutboxIDRRequestList(SessionStaffNumber, Year, 0, Status, IPAddress);
                ViewBag.MyIDRIndentRequests = outboxindentrequest.Count();

                // to get Pending IDR Indent Request count
                List<IndentModel> pendingrequestindentmodel = new List<IndentModel>();
                pendingrequestindentmodel = UserHelper.GetPendingRequestListWithoutFilter(SessionStaffNumber, IPAddress);
                ViewBag.PendingIDRIndentRequests = pendingrequestindentmodel.Count();

                // to get outbox MGP Request count
                List<MaterialGatePassModel> outboxmgprequest = new List<MaterialGatePassModel>();
                outboxmgprequest = UserHelper.GetOutboxMGPRequestList(SessionStaffNumber, Year, 0, Status, IPAddress);
                ViewBag.OutboxMGPRequests = outboxmgprequest.Count();

                // to get Pending MGP Request count
                List<MaterialGatePassModel> pendingrequestmgpmodel = new List<MaterialGatePassModel>();
                if (HttpContext.Session.GetString("Role") == "Authority" || HttpContext.Session.GetString("Role") == "AdminAuthority")
                {
                    pendingrequestmgpmodel = UserHelper.GetPendingMaterialGatePassRequestListAuthorityWithoutFilter(SessionStaffNumber, IPAddress);
                }
                else
                {
                    pendingrequestmgpmodel = UserHelper.GetPendingMaterialGatePassRequestListUserWithoutFilter(SessionStaffNumber, IPAddress);
                }
                ViewBag.PendingMaterialGatePassRequests = pendingrequestmgpmodel.Count();

                // to get outbox Visitor Pass Request count
                List<VisitorPassModel> outboxvisitorrequest = new List<VisitorPassModel>();
                outboxvisitorrequest = UserHelper.GetOutboxVisitorRequestListWithoutFilter(SessionStaffNumber, IPAddress);
                ViewBag.OutboxVisitorPassRequests = outboxvisitorrequest.Count();

                // to get Pending Visitor Pass Request count
                List<VisitorPassModel> pendingvisitorpassrequestmodel = new List<VisitorPassModel>();
                pendingvisitorpassrequestmodel = UserHelper.GetPendingVisitorPassRequestListWithoutFilter(SessionStaffNumber, IPAddress);
                ViewBag.PendingVisitorPassRequests = pendingvisitorpassrequestmodel.Count();
                
                // to get outbox Vehicle Indent Request count
                List<VehicleIndentModel> outboxvehiclerequest = new List<VehicleIndentModel>();
                outboxvehiclerequest = UserHelper.GetOutBoxVehicleRequestListWithoutFilter(SessionStaffNumber, IPAddress);
                ViewBag.OutboxVehicleIndentRequests = outboxvehiclerequest.Count();

                // to get Pending Vehicle Indent Request count
                List<VehicleIndentModel> pendingvehiclerequest = new List<VehicleIndentModel>();
                pendingvehiclerequest = UserHelper.GetPendingVehicleRequestListWithoutFilter(SessionStaffNumber, IPAddress);
                ViewBag.PendingVehicleIndentRequests = pendingvehiclerequest.Count();
                               

                foreach (var pendingindent in pendingrequestindentmodel)
                {
                    DateTime dateTime = DateTime.Now;
                    if(pendingindent.OldTargetDate <= dateTime)
                    {
                        ViewBag.WarningIndent = "! Please check IDR indents that are pending with you and take necessary action. Pending!!!!";
                    }
                }

                //foreach(var leaverequest in requestleavemodel)
                //{
                //    DateTime dateTime = DateTime.Now;
                //    if(leaverequest.AlternateLeaveFrom <= dateTime)
                //    {
                //        ViewBag.WarningLeave = "! Please check leave requests that are to be approved/sactioned. Pending!!!!";
                //    }
                //}

                //foreach(var permissionrequest in pendingpermissionmodel)
                //{
                //    DateTime dateTime = DateTime.Now;
                //    if(permissionrequest.Date <= dateTime)
                //    {
                //        ViewBag.WarningPermission = "! Please check permission requests that are to be approved/sactioned. Pending!!!!";
                //    }
                //}

                AttendanceModel model = new AttendanceModel();

                int StaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                string Date = DateTime.Now.Day.ToString();
                string Month = DateTime.Now.Month.ToString();

                ViewBag.LeaveInserted = TempData["LeaveInserted"];
                ViewBag.PermissionInserted = TempData["PermissionInserted"];
                TempData["LeaveInserted"] = "";
                TempData["PermissionInserted"] = "";

                if ((Date == "1" && (Month == "7" || Month == "1")) && ViewBag.LeaveInserted != "True")
                {
                    ViewBag.CommencementMonth = "Yes";
                }
                else
                {
                    ViewBag.CommencementMonth = "No";
                }

                if(Date=="1" && ViewBag.PermissionInserted != "True")
                {
                    ViewBag.BeginningMonth = "Yes";
                }
                else
                {
                    ViewBag.BeginningMonth = "No";
                }
            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                int StaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, StaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(attendances);
        }

        // view break

        public IActionResult Logout()
        {
            try
            {
                if (TempData["SuccessMessage"] != null)
                {
                    TempData["SuccessMessage"] = TempData["SuccessMessage"];
                }
                // Clearing sessions
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
                int StaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, StaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return RedirectToAction("Login", "User");
        }

        // view break

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(int StaffNumber)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                // check if user exist
                bool UserExist = CommonHelper.CheckUserExists(StaffNumber, IPAddress);

                // show error message if user does not exist
                if (!UserExist)
                {
                    ViewBag.ErrorMessage = "Please enter existing staff number";
                    return View();
                }

                // user exists store system generated password
                bool IsPasswordSaved = UserHelper.StoreGeneratedSystemPassword(StaffNumber, BrowserName, IPAddress);

                // if password saved move to ResetPassword
                if (IsPasswordSaved)
                {
                    HttpContext.Session.SetInt32("StaffNumber", StaffNumber);
                    return RedirectToAction("ResetPassword");
                }
            }
            catch (Exception error)
            {
                CommonHelper.LogError(error, StaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            // if any error go back to view
            ViewBag.ExceptionError = "An error occured. Please contact system administrator";
            return View();
        }

        // view break

        [HttpGet]
        public IActionResult ResetPassword()
        {
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.StaffNumber = HttpContext.Session.GetInt32("StaffNumber");
                HttpContext.Session.SetInt32("StaffNumber", 0);
            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                int StaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, StaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordModel model)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (model.SystemPassword == "" || model.SystemPassword == null)
                {
                    ViewBag.ErrorMessage = "Please enter password recieved through email";
                    return View();
                }
                if (model.NewPassword == "" || model.NewPassword == null)
                {
                    ViewBag.NewPasswordError = "Please enter new password";
                    return View();
                }
                if (model.ConfirmPassword == "" || model.ConfirmPassword == null)
                {
                    ViewBag.ConfirmPasswordError = "Please enter password again";
                    return View();
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                // get the system generated password that has sent to user
                string[] SystemPasswordAndSalt = UserHelper.GetRecoveryPassword(model.StaffNumber, IPAddress);
                string HashedStoredPassword = SystemPasswordAndSalt[0];
                string SALT = SystemPasswordAndSalt[1];

                // hash user entered system Password
                var hmac1 = CommonHelper.ComputeHash(Encoding.UTF8.GetBytes(model.SystemPassword), Convert.FromBase64String(SALT));
                string HashedEnteredPassword = Convert.ToBase64String(hmac1);

                // check if entered system password is same as in database
                if (HashedStoredPassword != HashedEnteredPassword)
                {
                    ViewBag.ErrorMessage = "Please enter valid password";
                    return View();
                }

                // check if Password is reset
                bool IsUpdated = UserHelper.ResetPassword(model, BrowserName, IPAddress);

                if (IsUpdated)
                {
                    string[] EmailAndName = CommonHelper.GetEmailAndName(model.StaffNumber, IPAddress);
                    string EmailID = EmailAndName[0];
                    string FirstName = EmailAndName[1];
                    string Message = @"Dear <b>" + FirstName + "</b>,<br/><br/>Your password has been reset successfully." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                    string Subject = "ebandhu - Reset Password";
                    // send email that password is changed
                    List<string> EmailList = new List<string>();
                    EmailList.Add(EmailID);
                    CommonHelper.SendEmail(EmailList, Subject, Message, model.StaffNumber, IPAddress);

                    //HttpContext.Session.SetString("SuccessMessage", "Password Changed Sucessfully");
                    TempData["SuccessMessage"] = "Password changed sucessfully";
                    // if password is reset Logout the user
                    return RedirectToAction("Logout", "User");
                }
            }
            catch (Exception error)
            {
                CommonHelper.LogError(error, model.StaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            // go back to view if any error happens
            ViewBag.ExceptionError = "An error occured. Please contact system administrator";
            return View();
        }

        // view break

        [HttpGet]
        public IActionResult ChangePassword()
        {
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                int StaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, StaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordModel model)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (model.OldPassword == "" || model.OldPassword == null)
                {
                    ViewBag.ErrorMessage = "Please enter old password";
                    return View();
                }
                if (model.NewPassword == "" || model.NewPassword == null)
                {
                    ViewBag.NewPasswordError = "Please enter new password";
                    return View();
                }
                if (model.ConfirmPassword == "" || model.ConfirmPassword == null)
                {
                    ViewBag.ConfirmPasswordError = "Please enter password again";
                    return View();
                }

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                // getting staff number from session
                model.StaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;
                // get old password
                string[] OldPasswordAndSalt = UserHelper.GetLoginPassword(model.StaffNumber, IPAddress);
                string OldHashedPassword = OldPasswordAndSalt[0];
                string SALT = OldPasswordAndSalt[1];

                // hash user entered system Password
                var hmac1 = CommonHelper.ComputeHash(Encoding.UTF8.GetBytes(model.OldPassword), Convert.FromBase64String(SALT));
                string HashedEnteredPassword = Convert.ToBase64String(hmac1);

                // check if entered password is correct
                if (OldHashedPassword != HashedEnteredPassword)
                {
                    ViewBag.ErrorMessage = "Please enter valid password";
                    return View();
                }

                // check if password is updated
                bool IsUpdated = UserHelper.ChangePassword(model, BrowserName, IPAddress);
                if (IsUpdated)
                {
                    string[] EmailAndName = CommonHelper.GetEmailAndName(model.StaffNumber, IPAddress);
                    string EmailID = EmailAndName[0];
                    string FirstName = EmailAndName[1];
                    string Message = @"Dear <b>" + FirstName + "</b>,<br/><br/>You have successfully changed your Password." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                    string Subject = "ebandhu - Change Password";
                    // send email that password is changed
                    List<string> EmailList = new List<string>();
                    EmailList.Add(EmailID);
                    CommonHelper.SendEmail(EmailList, Subject, Message, model.StaffNumber, IPAddress);

                    //HttpContext.Session.SetString("SuccessMessage", "Password Changed Sucessfully");
                    TempData["SuccessMessage"] = "Password changed sucessfully";
                    // if password changed Logout the user
                    return RedirectToAction("Logout", "User");
                }
            }
            catch (Exception error)
            {
                // getting staff number from session
                model.StaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, model.StaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            // go back to view if any error happens
            ViewBag.ExceptionError = "An error occured. Please contact system administrator";
            return View();
        }

        // view break

        public IActionResult AccessDenied()
        {
            return View();
        }

        // view break

        public IActionResult Profile()
        {
            // Profile Model
            ProfileModel profilemodel = new ProfileModel();
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int StaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                // assigning the staffnumber to viewbag for getting the image
                ViewBag.Image = StaffNumber;

                // Redirecting to Login Page if not logged in
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                if (SessionStaffNumber == 0)
                {
                    return RedirectToAction("Login", "User");
                }

                // checking if authentication is successful or not
                string IsAuthenticated = HttpContext.Session.GetString("IsProfileAuthenticated");

                //if (IsAuthenticated != "True")
                //{
                //    // if not authenticated redirect for authentication
                //    return RedirectToAction("ProfileAuthentication");
                //}

                profilemodel.Employee = UserHelper.GetEmployeeData(StaffNumber, IPAddress);
                if (profilemodel.Employee.IsPhotoAvailable)
                {
                    ViewBag.IsPhotoAvailable = true;
                    var configuration = CommonHelper.GetConfig();
                    var loglocation = configuration["FileLocations:EmployeeImageLocationForDownload"];

                    ViewBag.UserImagePath = profilemodel.Employee.UserImagePath.Replace(loglocation, "../StaffPhotoUploads/");
                }
                profilemodel.Qualifications = UserHelper.GetQualificationData(StaffNumber, IPAddress);
                profilemodel.EmergencyContact = UserHelper.GetEmergencyContactData(StaffNumber, IPAddress);
                profilemodel.FamilyDeclaration = UserHelper.GetFamilyDeclarationData(StaffNumber, IPAddress);
                profilemodel.MedicalDependents = UserHelper.GetMedicalDependentsData(StaffNumber, IPAddress);
                profilemodel.Gratuity = UserHelper.GetGratuityData(StaffNumber, IPAddress);

            }
            catch (Exception error)
            {
                int StaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error occured. Please contact system administrator";
                CommonHelper.LogError(error, StaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(profilemodel);
        }

        // view break

        [HttpGet]
        public IActionResult AttendanceData(DateTime FromDate, DateTime ToDate)
        {
            if (ToDate > DateTime.Now)
            {
                ToDate = DateTime.Now;
            }

            List<AttendanceModel> attendances = new List<AttendanceModel>();

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                var configuration = CommonHelper.GetConfig();
                var connectionString = configuration["ConnectionStrings:Attendance"];

                bool isserverrunning = CommonHelper.IsAttendanceServerRunning(connectionString);

                if (!(isserverrunning))
                {
                    return RedirectToAction("ServerError", "User");
                }

                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int Year = DateTime.Now.Year;

                AttendanceModel model = new AttendanceModel();

                int StaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

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

                    List<HolidaysModel> closedholidays = UserHelper.GetHolidays(IPAddress, SessionStaffNumber, "CH", DateTime.Now.Year);
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
                            shifttimings = UserHelper.GetDriverShiftBasedAttendance(SessionStaffNumber, IPAddress, attedance.AlternateDate);
                        }
                        else if (HttpContext.Session.GetString("Role") == "Security")
                        {
                            shifttimings = UserHelper.GetSecurityShiftBasedAttendance(SessionStaffNumber, IPAddress, attedance.AlternateDate);
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
                int StaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, StaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(attendances);
        }

        // view break

        public IActionResult AttendanceActions()
        {
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                CommonHelper.LogError(error, 0000, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        // view break

        [HttpGet]
        public IActionResult Holidays()
        {
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<HolidaysModel> holidays = new List<HolidaysModel>();

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                ViewBag.IsFiltered = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View();
        }

        [HttpPost]
        public IActionResult Holidays(int Year, string Type)
        {
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<HolidaysModel> holidays = new List<HolidaysModel>();

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                holidays = UserHelper.GetHolidays(IPAddress, SessionStaffNumber, Type, Year);

                ViewBag.IsFiltered = true;
                ViewBag.TypeOfHoliday = Type;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(holidays);
        }

        // view break

        public IActionResult ServerError()
        {
            return View();
        }

        [HttpGet]
        public IActionResult OutboxMGPRequests()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult OutboxMGPRequests(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            List<MaterialGatePassModel> gatepassmodel = new List<MaterialGatePassModel>();
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetString("Role") == null)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                gatepassmodel = UserHelper.GetOutboxMGPRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(gatepassmodel);
        }

        public IActionResult OutboxMGPRequestsWithoutFilter()
        {
            ViewBag.Search = true;

            List<MaterialGatePassModel> gatepassmodel = new List<MaterialGatePassModel>();
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetString("Role") == null)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                gatepassmodel = UserHelper.GetOutboxMGPRequestListWithoutFilter(SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(gatepassmodel);
        }

        // view break

        public IActionResult ViewOutboxMGPRequest(string GatePassID)
        {
            List<MaterialGatePassModel> materialgatepass = new List<MaterialGatePassModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                materialgatepass = UserHelper.GetMaterialGatePassData(SessionStaffNumber, GatePassID, IPAddress);

                foreach (var gatepass in materialgatepass)
                {
                    if (gatepass.IsAttachmentPresent == false)
                    {
                        gatepass.Items = UserHelper.GetMaterialGatePassItems(SessionStaffNumber, GatePassID, IPAddress);
                    }
                    else
                    {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:MaterialGatePassFileLocationForDownload"];

                        ViewBag.AlteredAttachementLocation = gatepass.AttachmentLocation.Replace(loglocation, "../GatePassAttachment/");
                        if (gatepass.OldStatus == "PARTIAL IN" || gatepass.OldStatus == "OUT")
                        {
                            ViewBag.AlteredInAttachementLocation = gatepass.InAttachmentLocation.Replace(loglocation, "../GatePassAttachment/");
                        }
                    }

                    if (gatepass.SignatureFileLocation != null &&
                        (gatepass.OldStatus == "OUT" || gatepass.OldStatus == "PARTIAL IN" || gatepass.OldStatus == "IN"))
                    {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:MaterialGatePassSignatureLocationForDownload"];

                        ViewBag.SignatureFileLocation = gatepass.SignatureFileLocation.Replace(loglocation, "../GatePassSignature/");
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(materialgatepass);
        }

        // view break

        // Common tasks end

        // *************************************************************************************************** //
        // Indent start

        public IActionResult IDRIndent()
        {
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                CommonHelper.LogError(error, 0000, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        // view break

        [HttpGet]
        public IActionResult RaiseIndent()
        {
            IndentModel indentmodel = new IndentModel();

            ViewBag.IsCenterSelected = false;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                EmployeeListModel employee = new EmployeeListModel();
                employee = UserHelper.GetUserDataFromStaffNumber(SessionStaffNumber, IPAddress);

                indentmodel.SenderStaffNumber = employee.StaffNumber;
                indentmodel.SenderFirstName = employee.Name;
                indentmodel.FromGroupID = employee.GroupID;
                indentmodel.FromGroupName = employee.GroupName;
                indentmodel.FromCenterID = employee.CenterID;
                indentmodel.FromCenterName = employee.CenterName;
                indentmodel.NewTargetDate = DateTime.Today;
                List<CenterModel> centers = UserHelper.GetCenterData(SessionStaffNumber, IPAddress);
                indentmodel.Centers = centers;

                indentmodel.ReportingOfficerListModel = UserHelper.GetEmployeeListForReportingOfficer(SessionStaffNumber, IPAddress, indentmodel.FromGroupID);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
                ViewBag.ExceptionError = "An error has occured. Please contact System Administrator.";
            }

            return View(indentmodel);
        }

        [HttpPost]
        public IActionResult RaiseIndent(IndentModel model, IFormFile Attachment)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                if (model.ToCenterID == -1)
                {
                    EmployeeListModel employee = new EmployeeListModel();
                    employee = UserHelper.GetUserDataFromStaffNumber(SessionStaffNumber, IPAddress);

                    model.SenderStaffNumber = employee.StaffNumber;
                    model.SenderFirstName = employee.Name;
                    model.FromGroupID = employee.GroupID;
                    model.FromGroupName = employee.GroupName;
                    model.FromCenterID = employee.CenterID;
                    model.FromCenterName = employee.CenterName;
                    model.NewTargetDate = DateTime.Today;
                    model.Centers = UserHelper.GetCenterData(SessionStaffNumber, IPAddress);
                    model.ReportingOfficerListModel = UserHelper.GetEmployeeListForReportingOfficer(SessionStaffNumber, IPAddress, model.FromGroupID);
                    ViewBag.CenterError = "Please select center";
                    ViewBag.IsCenterSelected = false;
                    return View(model);
                }
                else if (model.ToGroupID == -1)
                {
                    int CenterID = model.ToCenterID;
                    EmployeeListModel employee = new EmployeeListModel();
                    employee = UserHelper.GetUserDataFromStaffNumber(SessionStaffNumber, IPAddress);

                    model.SenderStaffNumber = employee.StaffNumber;
                    model.SenderFirstName = employee.Name;
                    model.FromGroupID = employee.GroupID;
                    model.FromGroupName = employee.GroupName;
                    model.FromCenterID = employee.CenterID;
                    model.FromCenterName = employee.CenterName;
                    model.NewTargetDate = DateTime.Today;
                    model.Centers = UserHelper.GetCenterData(SessionStaffNumber, IPAddress);
                    model.Groups = UserHelper.GetGroupDataFromCenter(CenterID, SessionStaffNumber, IPAddress);
                    model.ReportingOfficerListModel = UserHelper.GetEmployeeListForReportingOfficer(SessionStaffNumber, IPAddress, model.FromGroupID);

                    ViewBag.IsCenterSelected = true;

                    return View(model);
                }
                else
                {
                    // to get browser name
                    var userAgent = HttpContext.Request.Headers["User-Agent"];
                    var uaParser = Parser.GetDefault();
                    ClientInfo c = uaParser.Parse(userAgent);
                    string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                    // to get IPAddress
                    string IpAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                    // to get the count from database table
                    int IndentCount = UserHelper.GetTodaysIndentsCount(model.SenderStaffNumber, IpAddress);

                    IndentCount = IndentCount + 1;

                    //model.IndentID = "IDR" + DateTime.Now.ToString("ddMMyy") + model.FromCenterID + model.ToCenterID + IndentCount;

                    model.IndentID = "IDR" + DateTime.Now.ToString("ddMMyy") + IndentCount;

                    if (Attachment == null)
                    {
                        model.IsAttachementPresent = false;
                        model.AttachementLocation = "";
                    }
                    else
                    {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:IndentFileLocation"];
                        string folderName = loglocation;
                        folderName = folderName.Replace("Year", DateTime.Now.Year.ToString());
                        folderName = folderName.Replace("Month", DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture));
                        folderName = folderName.Replace("IndentID", model.IndentID);
                        //string uploadPath = Server.MapPath(folderName);
                        string webRootPath = _hostingenvironment.ContentRootPath;
                        string newPath = Path.Combine(webRootPath, folderName);
                        if (!Directory.Exists(folderName))
                        {
                            Directory.CreateDirectory(folderName);
                        }
                        string TemporaryFileName = Attachment.FileName;
                        string Extension = TemporaryFileName.Split('.').Last();
                        string FileName = CommonHelper.GeneratePassword(10) + "." + Extension;
                        model.AttachementLocation = Path.Combine(folderName, FileName);
                        string envpath = folderName + "\\" + FileName;

                        using (var stream = new FileStream(model.AttachementLocation, FileMode.Create))
                        {
                            Attachment.CopyTo(stream);
                        }

                        model.IsAttachementPresent = true;
                    }

                    bool IsInserted = UserHelper.RaiseIndent(model, BrowserName, IPAddress);

                    if (IsInserted)
                    {
                        TempData["SuccessMessage"] = "Indent " + model.IndentID + " raised successfully";

                        // indentor
                        string[] IndentorEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                        string IndentorEmailID = IndentorEmailAndName[0];
                        // reporting officer
                        string[] ReportingOfficerEmailAndName = CommonHelper.GetEmailAndName(model.ReportingOfficer, IPAddress);
                        string ReportingOfficerEmailID = ReportingOfficerEmailAndName[0];

                        string Message = @"Dear Sir/Madam,<br/><br/>Inter Departmental Requisition has" +
                                            " been raised successfully and Indent ID is <b>" + model.IndentID + "</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                        string Subject = "ebandhu - Inter Departmental Requisition";
                        List<string> EmailList = new List<string>();
                        EmailList.Add(IndentorEmailID);
                        EmailList.Add(ReportingOfficerEmailID);

                        Console.WriteLine("RAISED");
                        foreach (var Email in EmailList)
                        {
                            Console.WriteLine(Email);
                        }
                        Console.WriteLine("---");

                        CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                        return RedirectToAction("RaiseIndentResult", "User");
                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                        return RedirectToAction("RaiseIndentResult", "User");
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
                ViewBag.ExceptionError = "An error has occured. Please contact system administrator.";
            }

            return View(model);
        }

        public IActionResult RaiseIndentResult()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            return View();
        }

        // view break

        [HttpGet]
        public IActionResult MyRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult MyRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            List<IndentModel> indentmodel = new List<IndentModel>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetMyRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(indentmodel);
        }

        public IActionResult MyRequestWithoutFilter()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            TempData["Search"] = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            List<IndentModel> indentmodel = new List<IndentModel>();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetMyRequestListWithoutFilter(SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(indentmodel);
        }

        // view break

        [HttpGet]
        public IActionResult ViewMyIndentRequest(string IndentID)
        {
            List<IndentModel> indentmodel = new List<IndentModel>();
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetIndentData(SessionStaffNumber, IndentID, IPAddress);

                foreach (var indent in indentmodel)
                {
                    if (indent.IsAttachementPresent == true && indent.AttachementLocation != null)
                    {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:IndentFileLocationForDownload"];

                        indent.AlteredAttachementLocation = indent.AttachementLocation.Replace(loglocation, "../IndentUploads/");
                    }
                }

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(indentmodel);
        }

        [HttpPost]
        public IActionResult ViewMyIndentRequest(List<IndentModel> model, IFormFile NewAttachment)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                foreach (var indent in model)
                {
                    if (NewAttachment != null)
                    {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:IndentFileLocation"];
                        string folderName = loglocation;
                        folderName = folderName.Replace("Year", DateTime.Now.Year.ToString());
                        folderName = folderName.Replace("Month", DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture));
                        folderName = folderName.Replace("IndentID", indent.IndentID);
                        //string uploadPath = Server.MapPath(folderName);
                        string webRootPath = _hostingenvironment.ContentRootPath;
                        string newPath = Path.Combine(webRootPath, folderName);
                        if (!Directory.Exists(folderName))
                        {
                            Directory.CreateDirectory(folderName);
                        }
                        string TemporaryFileName = NewAttachment.FileName;
                        string Extension = TemporaryFileName.Split('.').Last();
                        string FileName = CommonHelper.GeneratePassword(10) + "." + Extension;
                        indent.AttachementLocation = Path.Combine(folderName, FileName);
                        string envpath = folderName + "\\" + FileName;

                        using (var stream = new FileStream(indent.AttachementLocation, FileMode.Create))
                        {
                            NewAttachment.CopyTo(stream);
                        }

                        indent.IsAttachementPresent = true;
                    }
                    else
                    {
                        indent.AttachementLocation = "";
                        indent.IsAttachementPresent = false;
                    }

                    bool IsUpdated = UserHelper.UpdateIndentData(SessionStaffNumber, indent, BrowserName, IPAddress);

                    if (IsUpdated)
                    {
                        if (indent.NewIndentStatus == "CANCELLED")
                        {
                            // send email
                            string[] EmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string EmailID = EmailAndName[0];
                            string FirstName = EmailAndName[1];
                            string Message = @"Dear <b>" + FirstName + ",</b><br/><br/>Your request for Inter Departmental Requisition" +
                                            " with Indent ID <b>" + indent.IndentID + "</b> has been <b>CANCELLED</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Inter Departmental Requisition";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(EmailID);

                            Console.WriteLine(indent.NewIndentStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "Indent request has been cancelled";
                        }
                        else if (indent.NewIndentStatus == "ACCEPTED & CLOSED")
                        {
                            EmployeeListModel CompletedEmployee = UserHelper.GetIndentDataWithSpecificStatus(SessionStaffNumber, indent.IndentID, "COMPLETED", IPAddress);

                            // send email to indentor
                            string[] IndentorEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string IndentorEmailID = IndentorEmailAndName[0];

                            // send email to assigned user
                            string[] AssignedUserEmailAndName = CommonHelper.GetEmailAndName(CompletedEmployee.StaffNumber, IPAddress);
                            string AssignedUserEmailID = AssignedUserEmailAndName[0];

                            string Message = @"Dear Sir/Madam,<br/><br/>Your request for Inter Departmental Requisition" +
                                            " with Indent ID <b>" + indent.IndentID + "</b> has been <b>ACCEPTED & CLOSED</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Inter Departmental Requisition";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(IndentorEmailID);
                            EmailList.Add(AssignedUserEmailID);

                            Console.WriteLine(indent.NewIndentStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "Indent request has been accepted";
                        }
                        else if (indent.NewIndentStatus == "REOPEN")
                        {
                            EmployeeListModel CompletedEmployee = UserHelper.GetIndentDataWithSpecificStatus(SessionStaffNumber, indent.IndentID, "COMPLETED", IPAddress);

                            // send email to indentor
                            string[] IndentorEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string IndentorEmailID = IndentorEmailAndName[0];

                            // send email to assigned user
                            string[] AssignedUserEmailAndName = CommonHelper.GetEmailAndName(indent.RecieverStaffNumber, IPAddress);
                            string AssignedUserEmailID = AssignedUserEmailAndName[0];

                            string Message = @"Dear Sir/Madam,<br/><br/>Your request for Inter Departmental Requisition" +
                                            " with Indent ID <b>" + indent.IndentID + "</b> has been <b>REOPENED</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Inter Departmental Requisition";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(IndentorEmailID);
                            EmailList.Add(AssignedUserEmailID);

                            Console.WriteLine(indent.NewIndentStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "Indent request has been reopened";
                        }
                        else
                        {
                            TempData["SuccessMessage"] = "Update Successful";
                        }


                        return RedirectToAction("MyRequestWithoutFilter", "User");
                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error occured. Please contact system administrator.";
                        return RedirectToAction("MyRequestWithoutFilter", "User");
                    }
                }

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        // view break

        [HttpGet]
        public IActionResult PendingRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult PendingRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            List<IndentModel> indentmodel = new List<IndentModel>();
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetString("Role") == null)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetPendingRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(indentmodel);
        }

        public IActionResult PendingRequestWithoutFilter()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            ViewBag.Search = true;

            List<IndentModel> indentmodel = new List<IndentModel>();
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetString("Role") == null)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetPendingRequestListWithoutFilter(SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(indentmodel);
        }
        // view break

        [HttpGet]
        public IActionResult ViewPendingIndentRequest(string IndentID)
        {
            List<IndentModel> indentmodel = new List<IndentModel>();
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            // to get browser name
            var userAgent = HttpContext.Request.Headers["User-Agent"];
            var uaParser = Parser.GetDefault();
            ClientInfo c = uaParser.Parse(userAgent);
            string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetIndentData(SessionStaffNumber, IndentID, IPAddress);


                foreach (var indent in indentmodel)
                {
                    string StaffEmployeeGroup = HttpContext.Session.GetString("EmployeeGroup");
                    indent.EmployeeList = UserHelper.GetEmployeeListForIndentAssign(SessionStaffNumber, IPAddress, StaffEmployeeGroup, indent.ToGroupID);

                    if (indent.OldIndentStatus == "ASSIGNED")
                    {
                        indent.NewIndentStatus = "WORK IN PROGRESS";
                        indent.NewRemarks = "Auto update";

                        bool IsUpdated = UserHelper.UpdateIndentData(SessionStaffNumber, indent, BrowserName, IPAddress);

                        indent.NewIndentStatus = "";
                        indent.NewRemarks = "";
                    }

                    break;
                }

                foreach (var indent in indentmodel)
                {
                    if (indent.IsAttachementPresent == true && indent.AttachementLocation != null)
                    {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:IndentFileLocationForDownload"];

                        indent.AlteredAttachementLocation = indent.AttachementLocation.Replace(loglocation, "../IndentUploads/");
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(indentmodel);
        }

        [HttpPost]
        public IActionResult ViewPendingIndentRequest(List<IndentModel> model, IFormFile NewAttachment)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                foreach (var indent in model)
                {
                    if (NewAttachment != null)
                    {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:IndentFileLocation"];
                        string folderName = loglocation;
                        folderName = folderName.Replace("Year", DateTime.Now.Year.ToString());
                        folderName = folderName.Replace("Month", DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture));
                        folderName = folderName.Replace("IndentID", indent.IndentID);
                        //string uploadPath = Server.MapPath(folderName);
                        string webRootPath = _hostingenvironment.ContentRootPath;
                        string newPath = Path.Combine(webRootPath, folderName);
                        if (!Directory.Exists(folderName))
                        {
                            Directory.CreateDirectory(folderName);
                        }
                        string TemporaryFileName = NewAttachment.FileName;
                        string Extension = TemporaryFileName.Split('.').Last();
                        string FileName = CommonHelper.GeneratePassword(10) + "." + Extension;
                        indent.AttachementLocation = Path.Combine(folderName, FileName);
                        string envpath = folderName + "\\" + FileName;

                        using (var stream = new FileStream(indent.AttachementLocation, FileMode.Create))
                        {
                            NewAttachment.CopyTo(stream);
                        }

                        indent.IsAttachementPresent = true;
                    }
                    else
                    {
                        indent.AttachementLocation = "";
                        indent.IsAttachementPresent = false;
                    }

                    bool IsUpdated = UserHelper.UpdateIndentData(SessionStaffNumber, indent, BrowserName, IPAddress);

                    if (IsUpdated)
                    {
                        if (indent.NewIndentStatus == "FORWARDED")
                        {
                            FromToGroupCenterHeadModel fromtogroupcenter = UserHelper.GetGroupCenterHeadOfFromAndTo(SessionStaffNumber, indent.IndentID, IPAddress);

                            // send email to indentor
                            string[] IndentorEmailAndName = CommonHelper.GetEmailAndName(indent.SenderStaffNumber, IPAddress);
                            string IndentorEmailID = IndentorEmailAndName[0];

                            // send email to reporting officer
                            string[] ReportingOfficerEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string ReportingOfficerEmailID = ReportingOfficerEmailAndName[0];

                            // send email to from Group Head
                            string[] FromGroupHeadEmailAndName = CommonHelper.GetEmailAndName(fromtogroupcenter.FromGroupHeadStaffNumber, IPAddress);
                            string FromGroupHeadEmailID = FromGroupHeadEmailAndName[0];

                            // send email to from Center Head
                            string[] FromCenterHeadEmailAndName = CommonHelper.GetEmailAndName(fromtogroupcenter.FromCenterHeadStaffNumber, IPAddress);
                            string FromCenterHeadEmailID = FromCenterHeadEmailAndName[0];

                            string Message = @"Dear Sir/Madam,<br/><br/>Inter Departmental Requisition" +
                                            " with Indent ID <b>" + indent.IndentID + "</b> has been <b>FORWARDED</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Inter Departmental Requisition";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(IndentorEmailID);
                            EmailList.Add(ReportingOfficerEmailID);
                            EmailList.Add(FromGroupHeadEmailID);
                            EmailList.Add(FromCenterHeadEmailID);
                            
                            Console.WriteLine(indent.NewIndentStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "Indent request has been forwarded";
                        }
                        else if (indent.NewIndentStatus == "APPROVED")
                        {
                            FromToGroupCenterHeadModel fromtogroupcenter = UserHelper.GetGroupCenterHeadOfFromAndTo(SessionStaffNumber, indent.IndentID, IPAddress);

                            // send email to indentor
                            string[] IndentorEmailAndName = CommonHelper.GetEmailAndName(indent.SenderStaffNumber, IPAddress);
                            string IndentorEmailID = IndentorEmailAndName[0];

                            // send email to from Group Head
                            //string[] FromGroupHeadEmailAndName = CommonHelper.GetEmailAndName(fromtogroupcenter.FromGroupHeadStaffNumber, IPAddress);
                            //string FromGroupHeadEmailID = FromGroupHeadEmailAndName[0];

                            //// send email to from Center Head
                            //string[] FromCenterHeadEmailAndName = CommonHelper.GetEmailAndName(fromtogroupcenter.FromCenterHeadStaffNumber, IPAddress);
                            //string FromCenterHeadEmailID = FromCenterHeadEmailAndName[0];

                            string[] ActionTakenEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string ActionTakenEmailID = ActionTakenEmailAndName[0];

                            // send email to to Group Head
                            string[] ToGroupHeadEmailAndName = CommonHelper.GetEmailAndName(fromtogroupcenter.ToGroupHeadStaffNumber, IPAddress);
                            string ToGroupHeadEmailID = ToGroupHeadEmailAndName[0];

                            // send email to to Center Head
                            string[] ToCenterHeadEmailAndName = CommonHelper.GetEmailAndName(fromtogroupcenter.ToCenterHeadStaffNumber, IPAddress);
                            string ToCenterHeadEmailID = ToCenterHeadEmailAndName[0];

                            string Message = @"Dear Sir/Madam,<br/><br/>Inter Departmental Requisition" +
                                            " with Indent ID <b>" + indent.IndentID + "</b> has been <b>APPROVED</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Inter Departmental Requisition";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(IndentorEmailID);
                            //EmailList.Add(FromGroupHeadEmailID);
                            //EmailList.Add(FromCenterHeadEmailID);
                            EmailList.Add(ActionTakenEmailID);
                            EmailList.Add(ToGroupHeadEmailID);
                            EmailList.Add(ToCenterHeadEmailID);

                            Console.WriteLine(indent.NewIndentStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "Indent request has been approved";
                        }
                        else if (indent.NewIndentStatus == "ASSIGNED")
                        {
                            FromToGroupCenterHeadModel fromtogroupcenter = UserHelper.GetGroupCenterHeadOfFromAndTo(SessionStaffNumber, indent.IndentID, IPAddress);

                            // send email to indentor
                            string[] IndentorEmailAndName = CommonHelper.GetEmailAndName(indent.SenderStaffNumber, IPAddress);
                            string IndentorEmailID = IndentorEmailAndName[0];

                            // send email to from Group Head
                            string[] AssignedUserEmailAndName = CommonHelper.GetEmailAndName(indent.RecieverStaffNumber, IPAddress);
                            string AssignedUserEmailID = AssignedUserEmailAndName[0];
                            string AssignedUserFullName = AssignedUserEmailAndName[1];

                            // send email to to Group Head
                            //string[] ToGroupHeadEmailAndName = CommonHelper.GetEmailAndName(fromtogroupcenter.ToGroupHeadStaffNumber, IPAddress);
                            //string ToGroupHeadEmailID = ToGroupHeadEmailAndName[0];

                            //// send email to to Center Head
                            //string[] ToCenterHeadEmailAndName = CommonHelper.GetEmailAndName(fromtogroupcenter.ToCenterHeadStaffNumber, IPAddress);
                            //string ToCenterHeadEmailID = ToCenterHeadEmailAndName[0];

                            string[] ActionTakenEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string ActionTakenEmailID = ActionTakenEmailAndName[0];

                            string Message = @"Dear Sir/Madam,<br/><br/>Inter Departmental Requisition" +
                                            " with Indent ID <b>" + indent.IndentID + "</b> has been <b>ASSIGNED</b> to " +
                                            AssignedUserFullName + "." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Inter Departmental Requisition";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(IndentorEmailID);
                            EmailList.Add(AssignedUserEmailID);
                            EmailList.Add(ActionTakenEmailID);

                            Console.WriteLine(indent.NewIndentStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "Indent request has been assigned to " + AssignedUserFullName;
                        }
                        else if (indent.NewIndentStatus == "COMPLETED")
                        {
                            FromToGroupCenterHeadModel fromtogroupcenter = UserHelper.GetGroupCenterHeadOfFromAndTo(SessionStaffNumber, indent.IndentID, IPAddress);

                            // send email to indentor
                            string[] IndentorEmailAndName = CommonHelper.GetEmailAndName(indent.SenderStaffNumber, IPAddress);
                            string IndentorEmailID = IndentorEmailAndName[0];

                            // send email to completed employee
                            string[] EmployeeEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string EmployeeEmailID = EmployeeEmailAndName[0];

                            // send email to to Group Head
                            //string[] ToGroupHeadEmailAndName = CommonHelper.GetEmailAndName(fromtogroupcenter.ToGroupHeadStaffNumber, IPAddress);
                            //string ToGroupHeadEmailID = ToGroupHeadEmailAndName[0];
                            //// send email to to Center Head
                            //string[] ToCenterHeadEmailAndName = CommonHelper.GetEmailAndName(fromtogroupcenter.ToCenterHeadStaffNumber, IPAddress);
                            //string ToCenterHeadEmailID = ToCenterHeadEmailAndName[0];

                            string Message = @"Dear Sir/Madam,<br/><br/>Inter Departmental Requisition" +
                                            " with Indent ID <b>" + indent.IndentID + "</b> has been <b>COMPLETED</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Inter Departmental Requisition";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(IndentorEmailID);
                            EmailList.Add(EmployeeEmailID);

                            Console.WriteLine(indent.NewIndentStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "Indent request has been completed";
                        }
                        else if (indent.NewIndentStatus == "REJECTED")
                        {
                            // send email to indentor
                            string[] IndentorEmailAndName = CommonHelper.GetEmailAndName(indent.SenderStaffNumber, IPAddress);
                            string IndentorEmailID = IndentorEmailAndName[0];

                            // send email to completed employee
                            string[] EmployeeEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string EmployeeEmailID = EmployeeEmailAndName[0];

                            string Message = @"Dear Sir/Madam,<br/><br/>Inter Departmental Requisition" +
                                            " with Indent ID <b>" + indent.IndentID + "</b> has been <b>REJECTED</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Inter Departmental Requisition";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(IndentorEmailID);
                            EmailList.Add(EmployeeEmailID);

                            Console.WriteLine(indent.NewIndentStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "Indent request has been rejected";
                        }
                        else
                        {
                            TempData["SuccessMessage"] = "Update successful";
                        }

                        return RedirectToAction("PendingRequestWithoutFilter", "User");
                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error occured. Please contact system administrator";
                        return RedirectToAction("PendingRequestWithoutFilter", "User");
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        // view break

        [HttpGet]
        public IActionResult AssignedRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult AssignedRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            List<IndentModel> indentmodel = new List<IndentModel>();
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetString("Role") == null)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetAssignedRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(indentmodel);
        }

        public IActionResult AssignedRequestWithoutFilter()
        {
            ViewBag.Search = true;

            List<IndentModel> indentmodel = new List<IndentModel>();
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetString("Role") == null)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetAssignedRequestListWithoutFilter(SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(indentmodel);
        }

        // view break

        public IActionResult ViewAssignedIndentRequest(string IndentID)
        {
            List<IndentModel> indentmodel = new List<IndentModel>();
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetIndentData(SessionStaffNumber, IndentID, IPAddress);

                foreach (var indent in indentmodel)
                {
                    if (indent.IsAttachementPresent == true && indent.AttachementLocation != null)
                    {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:IndentFileLocationForDownload"];

                        indent.AlteredAttachementLocation = indent.AttachementLocation.Replace(loglocation, "../IndentUploads/");
                    }
                }

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(indentmodel);
        }

        // view break

        [HttpGet]
        public IActionResult OutboxIDRRequests()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult OutboxIDRRequests(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            List<IndentModel> indentmodel = new List<IndentModel>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetOutboxIDRRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);

                foreach (var indent in indentmodel)
                {
                    var tempindents = UserHelper.GetIndentData(SessionStaffNumber, indent.IndentID, IPAddress);
                    foreach (var tempindent in tempindents)
                    {
                        indent.OldIndentStatus = tempindent.OldIndentStatus;
                        break;
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(indentmodel);
        }

        public IActionResult OutboxIDRRequestsWithoutFilter()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            TempData["Search"] = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            List<IndentModel> indentmodel = new List<IndentModel>();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetOutboxIDRRequestListWithoutFilter(SessionStaffNumber, IPAddress);


            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(indentmodel);
        }

        // view break

        public IActionResult ViewOutboxIDRRequest(string IndentID)
        {
            List<IndentModel> indentmodel = new List<IndentModel>();
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetIndentData(SessionStaffNumber, IndentID, IPAddress);

                foreach (var indent in indentmodel)
                {
                    if (indent.IsAttachementPresent == true && indent.AttachementLocation != null)
                    {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:IndentFileLocationForDownload"];

                        indent.AlteredAttachementLocation = indent.AttachementLocation.Replace(loglocation, "../IndentUploads/");
                    }
                }

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(indentmodel);
        }

        // view break

        // Indent end
        // *************************************************************************************************** //


        // view break


        // *************************************************************************************************** //
        // Vehicle Indent start

        public IActionResult VehicleIndent()
        {
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                CommonHelper.LogError(error, 0000, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        // view break

        [HttpGet]
        public IActionResult RaiseVehicleIndent()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            VehicleIndentModel vehicleindentmodel = new VehicleIndentModel();

            ViewBag.IsCenterSelected = false;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                EmployeeListModel employee = UserHelper.GetUserDataFromStaffNumber(SessionStaffNumber, IPAddress);

                vehicleindentmodel.SenderStaffNumber = employee.StaffNumber;
                vehicleindentmodel.SenderFirstName = employee.Name;
                vehicleindentmodel.SenderDesignation = employee.Designation;
                vehicleindentmodel.SenderGroupName = employee.GroupName;
                vehicleindentmodel.SenderGroupID = employee.GroupID;
                vehicleindentmodel.SenderCenterName = employee.CenterName;
                vehicleindentmodel.SenderCenterID = employee.CenterID;
                vehicleindentmodel.IndentRequiredDate = DateTime.Today;
                vehicleindentmodel.IndentRequiredTime = DateTime.Now.TimeOfDay;

                vehicleindentmodel.ReportingOfficerListModel = UserHelper.GetEmployeeListForReportingOfficer(SessionStaffNumber, IPAddress, vehicleindentmodel.SenderGroupID);

                vehicleindentmodel.GroupEmployees = UserHelper.GetEmployeeDataListFromGroupCenter(vehicleindentmodel.SenderGroupID, vehicleindentmodel.SenderCenterID, SessionStaffNumber, IPAddress);

                vehicleindentmodel.Employees = UserHelper.GetAllEmployees(SessionStaffNumber, IPAddress);

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
                ViewBag.ExceptionError = "An error has occured. Please contact system administrator.";
            }

            return View(vehicleindentmodel);
        }

        [HttpPost]
        public IActionResult RaiseVehicleIndent(VehicleIndentModel model)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                string currenttime=DateTime.Now.TimeOfDay.ToString();

                if ((TimeSpan.Parse(currenttime) > model.IndentRequiredTime) && model.IndentRequiredDate.ToString("dd/MM/yyyy") == DateTime.Now.ToString("dd/MM/yyyy"))
                {
                    VehicleIndentModel vehicleindentmodel = new VehicleIndentModel();

                    int StaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                    EmployeeListModel employee = UserHelper.GetUserDataFromStaffNumber(StaffNumber, IPAddress);

                    vehicleindentmodel.SenderStaffNumber = employee.StaffNumber;
                    vehicleindentmodel.SenderFirstName = employee.Name;
                    vehicleindentmodel.SenderDesignation = employee.Designation;
                    vehicleindentmodel.SenderGroupName = employee.GroupName;
                    vehicleindentmodel.SenderGroupID = employee.GroupID;
                    vehicleindentmodel.SenderCenterName = employee.CenterName;
                    vehicleindentmodel.SenderCenterID = employee.CenterID;
                    vehicleindentmodel.IndentRequiredDate = DateTime.Today;
                    vehicleindentmodel.IndentRequiredTime = DateTime.Now.TimeOfDay;

                    vehicleindentmodel.ReportingOfficerListModel = UserHelper.GetEmployeeListForReportingOfficer(StaffNumber, IPAddress, vehicleindentmodel.SenderGroupID);

                    vehicleindentmodel.Employees = UserHelper.GetAllEmployees(StaffNumber, IPAddress);

                    ViewBag.TimeError = "Please select valid time";
                    return View(vehicleindentmodel);
                }

                // to get the count from database table
                int IndentCount = UserHelper.GetTodaysVehicleIndentsCount(model.SenderStaffNumber, IPAddress);

                IndentCount = IndentCount + 1;

                model.IndentID = "VRF" + DateTime.Now.ToString("ddMMyy") + IndentCount;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                bool IsInserted = UserHelper.RaiseVehicleIndent(model, BrowserName, IPAddress);

                if (IsInserted)
                {
                    TempData["SuccessMessage"] = "Vehicle indent " + model.IndentID + " raised successfully";

                    string[] IndentorEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                    string IndentorEmailID = IndentorEmailAndName[0];
                    // reporting officer
                    string[] ReportingOfficerEmailAndName = CommonHelper.GetEmailAndName(model.ReportingOfficerStaffNumber, IPAddress);
                    string ReportingOfficerEmailID = ReportingOfficerEmailAndName[0];

                    string Message = @"Dear Sir/Madam,<br/><br/>Vehicle Requisition Form has" +
                                        " been raised successfully and Vehicle Indent ID is <b>" + model.IndentID + "</b>." +
                                        "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                    string Subject = "ebandhu - Vehicle Requisition Form";
                    List<string> EmailList = new List<string>();
                    EmailList.Add(IndentorEmailID);
                    EmailList.Add(ReportingOfficerEmailID);

                    Console.WriteLine("RAISED");
                    foreach (var Email in EmailList)
                    {
                        Console.WriteLine(Email);
                    }
                    Console.WriteLine("---");

                    CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                    return RedirectToAction("RaiseVehicleIndentResult", "User");
                }
                else
                {
                    TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                    return RedirectToAction("RaiseVehicleIndentResult", "User");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
                ViewBag.ExceptionError = "An error has occured. Please contact system administrator.";
            }

            return View();
        }
        
        // view break

        public IActionResult RaiseVehicleIndentResult()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            return View();
        }

        // view break

        public IActionResult MyVehicleRequestWithoutFilter()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<VehicleIndentModel> indentmodel = new List<VehicleIndentModel>();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetMyVehicleRequestListWithoutFilter(SessionStaffNumber, IPAddress);

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(indentmodel);
        }

        [HttpGet]
        public IActionResult MyVehicleRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult MyVehicleRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<VehicleIndentModel> indentmodel = new List<VehicleIndentModel>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetMyVehicleRequestList(SessionStaffNumber, Year, Month, IPAddress, Status);

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(indentmodel);
        }

        // view break

        [HttpGet]
        public IActionResult ViewMyVehicleIndentRequest(string IndentID)
        {
            List<VehicleIndentModel> indentmodel = new List<VehicleIndentModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetVehicleIndentData(SessionStaffNumber, IndentID, IPAddress);

                foreach (var indent in indentmodel)
                {
                    indent.IndentInvolvedPersons = UserHelper.GetVehicleIndentPersons(SessionStaffNumber, IndentID, IPAddress);
                    indent.AlteredIndentRequiredTime = Convert.ToDateTime(indent.AlteredIndentRequiredTime).ToString("hh:mm:ss tt");
                    indent.AlteredIndentRequiredDate = Convert.ToDateTime(indent.AlteredIndentRequiredDate).ToString("dd/MM/yyyy");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(indentmodel);
        }

        [HttpPost]
        public IActionResult ViewMyVehicleIndentRequest(List<VehicleIndentModel> model)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                foreach (var indent in model)
                {
                    bool IsUpdated = UserHelper.UpdateVehicleIndentData(SessionStaffNumber, indent, BrowserName, IPAddress);
                    if (IsUpdated)
                    {
                        if (indent.NewIndentStatus == "CANCELLED")
                        {
                            // send email
                            string[] EmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string EmailID = EmailAndName[0];
                            string FirstName = EmailAndName[1];
                            string Message = @"Dear Sir/Madam,<br/><br/>Vehicle Requisition Form" +
                                            " with Vehicle Indent ID <b>" + indent.IndentID + "</b> has been <b>CANCELLED</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Vehicle Requisition Form";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(EmailID);

                            Console.WriteLine(indent.NewIndentStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "Vehicle request has been CANCELLED";
                        }

                        return RedirectToAction("MyVehicleRequestWithoutFilter", "User");
                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                        return RedirectToAction("MyVehicleRequestWithoutFilter", "User");
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
            return RedirectToAction("MyVehicleRequest", "User");
        }

        // view break

        public IActionResult PendingVehicleRequestWithoutFilter()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<VehicleIndentModel> indentmodel = new List<VehicleIndentModel>();

            try
            {
                int Year = DateTime.Now.Year;
                string Status = "";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetPendingVehicleRequestListWithoutFilter(SessionStaffNumber, IPAddress);

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(indentmodel);
        }

        [HttpGet]
        public IActionResult PendingVehicleRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult PendingVehicleRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<VehicleIndentModel> indentmodel = new List<VehicleIndentModel>();

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetPendingVehicleRequestList(SessionStaffNumber, Year, Month, IPAddress, Status);

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(indentmodel);
        }

        // view break

        [HttpGet]
        public IActionResult ViewPendingVehicleIndentRequest(string IndentID)
        {
            List<VehicleIndentModel> indentmodel = new List<VehicleIndentModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetVehicleIndentData(SessionStaffNumber, IndentID, IPAddress);

                foreach (var indent in indentmodel)
                {
                    string StaffEmployeeGroup = HttpContext.Session.GetString("StaffNumber");
                    indent.IndentInvolvedPersons = UserHelper.GetVehicleIndentPersons(SessionStaffNumber, IndentID, IPAddress);

                    indent.EmployeeList = AdminHelper.GetDriversData(IPAddress, SessionStaffNumber);

                    indent.AlteredIndentRequiredTime = Convert.ToDateTime(indent.AlteredIndentRequiredTime).ToString("hh:mm:ss tt");
                    indent.AlteredIndentRequiredDate = Convert.ToDateTime(indent.AlteredIndentRequiredDate).ToString("dd/MM/yyyy");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(indentmodel);
        }

        [HttpPost]
        public IActionResult ViewPendingVehicleIndentRequest(List<VehicleIndentModel> model)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                foreach (var indent in model)
                {
                    bool IsUpdated = UserHelper.UpdateVehicleIndentData(SessionStaffNumber, indent, BrowserName, IPAddress);
                    if (IsUpdated)
                    {
                        if (indent.NewIndentStatus == "FORWARDED")
                        {
                            FromToGroupCenterHeadModel fromtogroupcenter = UserHelper.GetGroupCenterHeadOfFromVehicleIndent(SessionStaffNumber, indent.IndentID, IPAddress);

                            // send email
                            string[] EmailAndName = CommonHelper.GetEmailAndName(indent.SenderStaffNumber, IPAddress);
                            string EmailID = EmailAndName[0];

                            string[] ReportingOfficerEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string ReportingOfficerEmailID = ReportingOfficerEmailAndName[0];

                            // send email to from Group Head
                            string[] FromGroupHeadEmailAndName = CommonHelper.GetEmailAndName(fromtogroupcenter.FromGroupHeadStaffNumber, IPAddress);
                            string FromGroupHeadEmailID = FromGroupHeadEmailAndName[0];

                            // send email to from Center Head
                            string[] FromCenterHeadEmailAndName = CommonHelper.GetEmailAndName(fromtogroupcenter.FromCenterHeadStaffNumber, IPAddress);
                            string FromCenterHeadEmailID = FromCenterHeadEmailAndName[0];

                            string Message = @"Dear Sir/Madam,<br/><br/>Vehicle Requisition Form" +
                                            " with Vehicle Indent ID <b>" + indent.IndentID + "</b> has been <b>FORWARDED</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Vehicle Requisition Form";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(EmailID);
                            EmailList.Add(ReportingOfficerEmailID);
                            EmailList.Add(FromGroupHeadEmailID);
                            EmailList.Add(FromCenterHeadEmailID);

                            Console.WriteLine(indent.NewIndentStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "Vehicle request has been FORWARDED";
                        }
                        else if (indent.NewIndentStatus == "APPROVED")
                        {
                            InchargeModel incharge = UserHelper.GetInchargeOfSpecificDepartment(SessionStaffNumber, IPAddress, "Transportation");

                            // send email
                            string[] EmailAndName = CommonHelper.GetEmailAndName(indent.SenderStaffNumber, IPAddress);
                            string EmailID = EmailAndName[0];

                            string[] ActionTakenEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string ActionTakenEmailID = ActionTakenEmailAndName[0];

                            string[] InchargeEmailAndName = CommonHelper.GetEmailAndName(incharge.InchargeStaffNumber, IPAddress);
                            string InchargeEmailID = InchargeEmailAndName[0];

                            string Message = @"Dear Sir/Madam,<br/><br/>Vehicle Requisition Form" +
                                            " with Vehicle Indent ID <b>" + indent.IndentID + "</b> has been <b>APPROVED</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Vehicle Requisition Form";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(EmailID);
                            EmailList.Add(ActionTakenEmailID);
                            EmailList.Add(InchargeEmailID);

                            Console.WriteLine(indent.NewIndentStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "Vehicle request has been APPROVED";
                        }
                        else if (indent.NewIndentStatus == "ASSIGNED")
                        {
                            // send email
                            string[] EmailAndName = CommonHelper.GetEmailAndName(indent.SenderStaffNumber, IPAddress);
                            string EmailID = EmailAndName[0];

                            string[] ActionTakenEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string ActionTakenEmailID = ActionTakenEmailAndName[0];

                            string[] DriverEmailAndName = CommonHelper.GetEmailAndName(indent.RecieverStaffNumber, IPAddress);
                            string DriverEmailID = DriverEmailAndName[0];

                            string Message = @"Dear Sir/Madam,<br/><br/>Vehicle Requisition Form" +
                                            " with Vehicle Indent ID <b>" + indent.IndentID + "</b> has been <b>ASSIGNED</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Vehicle Requisition Form";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(EmailID);
                            EmailList.Add(ActionTakenEmailID);
                            EmailList.Add(DriverEmailID);

                            Console.WriteLine(indent.NewIndentStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "Vehicle request has been ASSIGNED";
                        }
                        else if (indent.NewIndentStatus == "COMPLETED")
                        {
                            // send email
                            string[] EmailAndName = CommonHelper.GetEmailAndName(indent.SenderStaffNumber, IPAddress);
                            string EmailID = EmailAndName[0];

                            string[] ActionTakenEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string ActionTakenEmailID = ActionTakenEmailAndName[0];

                            string Message = @"Dear Sir/Madam,<br/><br/>Vehicle Requisition Form" +
                                            " with Vehicle Indent ID <b>" + indent.IndentID + "</b> has been <b>COMPLETED</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Vehicle Requisition Form";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(EmailID);
                            EmailList.Add(ActionTakenEmailID);

                            Console.WriteLine(indent.NewIndentStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "Vehicle request has been COMPLETED";
                        }
                        else if (indent.NewIndentStatus == "REJECTED")
                        {
                            // send email
                            string[] EmailAndName = CommonHelper.GetEmailAndName(indent.SenderStaffNumber, IPAddress);
                            string EmailID = EmailAndName[0];

                            string[] ActionTakenEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string ActionTakenEmailID = ActionTakenEmailAndName[0];

                            string Message = @"Dear Sir/Madam,<br/><br/>Vehicle Requisition Form" +
                                            " with Vehicle Indent ID <b>" + indent.IndentID + "</b> has been <b>REJECTED</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Vehicle Requisition Form";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(EmailID);
                            EmailList.Add(ActionTakenEmailID);

                            Console.WriteLine(indent.NewIndentStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "Vehicle request has been REJECTED";
                        }

                        return RedirectToAction("PendingVehicleRequestWithoutFilter", "User");
                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                        return RedirectToAction("PendingVehicleRequestWithoutFilter", "User");
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
            return RedirectToAction("PendingVehicleRequestWithoutFilter", "User");
        }

        // view break

        [HttpGet]
        public IActionResult AssignedVehicleRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult AssignedVehicleRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            List<VehicleIndentModel> vehicleindent = new List<VehicleIndentModel>();
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetString("Role") == null)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                vehicleindent = UserHelper.GetAssignedVehicleRequestList(SessionStaffNumber, Year, Month, IPAddress, Status);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(vehicleindent);
        }

        public IActionResult AssignedVehicleRequestWithoutFilter()
        {
            ViewBag.Search = true;

            List<VehicleIndentModel> vehicleindent = new List<VehicleIndentModel>();
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetString("Role") == null)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                vehicleindent = UserHelper.GetAssignedVehicleRequestListWithoutFilter(SessionStaffNumber, IPAddress);
                var tempvehicleindent = UserHelper.GetAssignedVehicleRequestListWithoutFilter(SessionStaffNumber, IPAddress);
                

                // some error
                // last row is not deleting
                for(int j=0; j < tempvehicleindent.Count(); j++)
                {
                    var indentdata = UserHelper.GetVehicleIndentData(SessionStaffNumber, tempvehicleindent[j].IndentID, IPAddress);

                    List<int> temp = new List<int>();

                    foreach (var tempindent in indentdata)
                    {
                        temp.Add(tempindent.ActionTakenByStaffNumber);
                    }

                    if(!(temp.Contains(SessionStaffNumber)))
                    {
                        vehicleindent.RemoveAt(j);
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(vehicleindent);
        }

        // view break

        public IActionResult ViewAssignedVehicleIndentRequest(string IndentID)
        {
            List<VehicleIndentModel> indentmodel = new List<VehicleIndentModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetVehicleIndentData(SessionStaffNumber, IndentID, IPAddress);

                foreach (var indent in indentmodel)
                {
                    indent.IndentInvolvedPersons = UserHelper.GetVehicleIndentPersons(SessionStaffNumber, IndentID, IPAddress);
                    indent.AlteredIndentRequiredTime = Convert.ToDateTime(indent.AlteredIndentRequiredTime).ToString("hh:mm:ss tt");
                    indent.AlteredIndentRequiredDate = Convert.ToDateTime(indent.AlteredIndentRequiredDate).ToString("dd/MM/yyyy");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(indentmodel);
        }

        // view break

        [HttpGet]
        public IActionResult OutBoxVehicleRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult OutBoxVehicleRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<VehicleIndentModel> indentmodel = new List<VehicleIndentModel>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetOutBoxVehicleRequestList(SessionStaffNumber, Year, Month, IPAddress, Status);

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(indentmodel);
        }

        public IActionResult OutBoxVehicleRequestWithoutFilter()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<VehicleIndentModel> indentmodel = new List<VehicleIndentModel>();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetOutBoxVehicleRequestListWithoutFilter(SessionStaffNumber, IPAddress);

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(indentmodel);
        }

        // view break

        public IActionResult ViewOutBoxVehicleIndentRequest(string IndentID)
        {
            List<VehicleIndentModel> indentmodel = new List<VehicleIndentModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetVehicleIndentData(SessionStaffNumber, IndentID, IPAddress);

                foreach (var indent in indentmodel)
                {
                    indent.IndentInvolvedPersons = UserHelper.GetVehicleIndentPersons(SessionStaffNumber, IndentID, IPAddress);
                    indent.AlteredIndentRequiredTime = Convert.ToDateTime(indent.AlteredIndentRequiredTime).ToString("hh:mm:ss tt");
                    indent.AlteredIndentRequiredDate = Convert.ToDateTime(indent.AlteredIndentRequiredDate).ToString("dd/MM/yyyy");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(indentmodel);
        }

        // Vehicle Indent end
        // *************************************************************************************************** //

        // view break

        // *************************************************************************************************** //
        // Material Gate Pass start

        public IActionResult MaterialGatePass()
        {
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                CommonHelper.LogError(error, 0000, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        // view break

        [HttpGet]
        public IActionResult PrepareMaterialGatePass()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            MaterialGatePassModel gatepass = new MaterialGatePassModel();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                EmployeeListModel employee = UserHelper.GetUserDataFromStaffNumber(SessionStaffNumber, IPAddress);

                gatepass.PreparedByStaffNumber = employee.StaffNumber;
                gatepass.PreparedByFirstName = employee.Name;
                gatepass.PreparedByDesignation = employee.Designation;
                gatepass.PreparedByGroupName = employee.GroupName;
                gatepass.PreparedByCenterName = employee.CenterName;
                gatepass.ReturnType = -1;

                gatepass.ReportingOfficerListModel = UserHelper.GetEmployeeListForMaterialGatePassCheck(SessionStaffNumber, IPAddress, employee.GroupID);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
                ViewBag.ExceptionError = "An error has occured. Please contact system administrator.";
            }

            return View(gatepass);
        }

        [HttpPost]
        public IActionResult PrepareMaterialGatePass(MaterialGatePassModel model, IFormFile Attachment)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                // to get the count from database table
                int GatePassCount = UserHelper.GetTodaysMaterialGatePassCount(model.PreparedByStaffNumber, IPAddress);

                GatePassCount = GatePassCount + 1;

                model.GatePassID = "MGP" + DateTime.Now.ToString("ddMMyy") + GatePassCount;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                if(Attachment != null)
                {
                    var configuration = CommonHelper.GetConfig();
                    var loglocation = configuration["FileLocations:MaterialGatePassFileLocation"];
                    string folderName = loglocation;
                    folderName = folderName.Replace("Year", DateTime.Now.Year.ToString());
                    folderName = folderName.Replace("Month", DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture));
                    folderName = folderName.Replace("GatePassID", model.GatePassID);
                    //string uploadPath = Server.MapPath(folderName);
                    string webRootPath = _hostingenvironment.ContentRootPath;
                    string newPath = Path.Combine(webRootPath, folderName);
                    if (!Directory.Exists(folderName))
                    {
                        Directory.CreateDirectory(folderName);
                    }
                    string TemporaryFileName = Attachment.FileName;
                    string Extension = TemporaryFileName.Split('.').Last();
                    string FileName = CommonHelper.GeneratePassword(10) + "." + Extension;
                    model.AttachmentLocation = Path.Combine(folderName, FileName);
                    string envpath = folderName + "\\" + FileName;

                    using (var stream = new FileStream(model.AttachmentLocation, FileMode.Create))
                    {
                        Attachment.CopyTo(stream);
                    }

                    model.IsAttachmentPresent = true;
                }
                else
                {
                    model.AttachmentLocation = "";

                    model.IsAttachmentPresent = false;
                }

                bool IsInserted = UserHelper.PrepareMatrialGatePass(model, BrowserName, IPAddress);

                if (IsInserted)
                {
                    TempData["SuccessMessage"] = "Material gate pass " + model.GatePassID + " prepared successfully";

                    // send email to user
                    string[] EmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                    string EmailID = EmailAndName[0];

                    string[] ReportingOfficerEmailAndName = CommonHelper.GetEmailAndName(model.ReportingOfficer, IPAddress);
                    string ReportingOfficerEmailID = ReportingOfficerEmailAndName[0];

                    string Message = @"Dear Sir/Madam,<br/><br/>Material Gate Pass has" +
                                        " been prepared successfully and Gate Pass ID is <b>" + model.GatePassID + "</b>." +
                                        "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                    string Subject = "ebandhu - Material Gate Pass";
                    List<string> EmailList = new List<string>();
                    EmailList.Add(EmailID);
                    EmailList.Add(ReportingOfficerEmailID);

                    Console.WriteLine("PREPARED");
                    foreach (var Email in EmailList)
                    {
                        Console.WriteLine(Email);
                    }
                    Console.WriteLine("---");

                    CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                    return RedirectToAction("PrepareMaterialGatePassResult", "User");
                }
                else
                {
                    TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                    return RedirectToAction("PrepareMaterialGatePassResult", "User");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
                ViewBag.ExceptionError = "An error has occured. Please contact system administrator.";
            }

            return View();
        }

        public IActionResult PrepareMaterialGatePassResult()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            return View();
        }

        // view break

        [HttpGet]
        public IActionResult MyMaterialGatePassRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult MyMaterialGatePassRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<MaterialGatePassModel> gatepass = new List<MaterialGatePassModel>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                gatepass = UserHelper.GetMyMaterialGatePassRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(gatepass);
        }

        public IActionResult MyMaterialGatePassRequestWithoutFilter()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<MaterialGatePassModel> gatepass = new List<MaterialGatePassModel>();
            try
            {
                int Year=DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                gatepass = UserHelper.GetMyMaterialGatePassRequestListWithoutFilter(SessionStaffNumber, IPAddress);

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(gatepass);
        }

        // view break

        [HttpGet]
        public IActionResult ViewMyMaterialGatePassRequest(string GatePassID)
        {
            List<MaterialGatePassModel> materialgatepass = new List<MaterialGatePassModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                materialgatepass = UserHelper.GetMaterialGatePassData(SessionStaffNumber, GatePassID, IPAddress);

                foreach (var gatepass in materialgatepass)
                {
                    if (gatepass.IsAttachmentPresent == false)
                    {
                        gatepass.Items = UserHelper.GetMaterialGatePassItems(SessionStaffNumber, GatePassID, IPAddress);
                    }
                    else
                    {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:MaterialGatePassFileLocationForDownload"];

                        ViewBag.AlteredAttachementLocation = gatepass.AttachmentLocation.Replace(loglocation, "../GatePassAttachment/");
                        if (gatepass.OldStatus == "PARTIAL IN" || gatepass.OldStatus == "IN")
                        {
                            gatepass.AlteredInAttachementLocation = gatepass.InAttachmentLocation.Replace(loglocation, "../GatePassAttachment/");
                        }
                    }

                    if (gatepass.SignatureFileLocation != null &&
                        (gatepass.OldStatus == "OUT" || gatepass.OldStatus == "PARTIAL IN" || gatepass.OldStatus == "IN"))
                    {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:MaterialGatePassSignatureLocationForDownload"];

                        ViewBag.SignatureFileLocation = gatepass.SignatureFileLocation.Replace(loglocation, "../GatePassSignature/");
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(materialgatepass);
        }

        [HttpPost]
        public IActionResult ViewMyMaterialGatePassRequest(List<MaterialGatePassModel> model)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                foreach (var gatepass in model)
                {                    
                    gatepass.SignatureFileLocation = "";
                    bool IsUpdated = UserHelper.UpdateMaterialGatePassData(SessionStaffNumber, gatepass, BrowserName, IPAddress);
                    if (IsUpdated)
                    {
                        if (gatepass.NewStatus == "CANCELLED")
                        {
                            // send email
                            string[] EmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string EmailID = EmailAndName[0];
                            string FirstName = EmailAndName[1];
                            string Message = @"Dear Sir/Madam,<br/><br/>Material Gate Pass request" +
                                            " with Gate Pass ID <b>" + gatepass.GatePassID + "</b> has been <b>CANCELLED</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Material Gate Pass";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(EmailID);

                            Console.WriteLine(gatepass.NewStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "GatePass request has been CANCELLED";
                        }

                        return RedirectToAction("MyMaterialGatePassRequestWithoutFilter", "User");
                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                        return RedirectToAction("MyMaterialGatePassRequest", "User");
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
            return RedirectToAction("MyMaterialGatePassRequest", "User");
        }

        // view break

        [HttpGet]
        public IActionResult PendingMaterialGatePassRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult PendingMaterialGatePassRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<MaterialGatePassModel> gatepassmodel = new List<MaterialGatePassModel>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                if (HttpContext.Session.GetString("Role") == "Authority" || HttpContext.Session.GetString("Role") == "AdminAuthority")
                {
                    gatepassmodel = UserHelper.GetPendingMaterialGatePassRequestListAuthority(SessionStaffNumber, Year, Month, Status, IPAddress);
                }
                else
                {
                    gatepassmodel = UserHelper.GetPendingMaterialGatePassRequestListUser(SessionStaffNumber, Year, Month, Status, IPAddress);
                }

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(gatepassmodel);
        }

        public IActionResult PendingMaterialGatePassRequestWithoutFilter()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<MaterialGatePassModel> gatepassmodel = new List<MaterialGatePassModel>();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";
                 
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                if (HttpContext.Session.GetString("Role") == "Authority" || HttpContext.Session.GetString("Role") == "AdminAuthority")
                {
                    gatepassmodel = UserHelper.GetPendingMaterialGatePassRequestListAuthorityWithoutFilter(SessionStaffNumber, IPAddress);
                }
                else
                {
                    gatepassmodel = UserHelper.GetPendingMaterialGatePassRequestListUserWithoutFilter(SessionStaffNumber, IPAddress);
                }

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(gatepassmodel);
        }

        // view break

        [HttpGet]
        public IActionResult ViewPendingMaterialGatePassRequest(string GatePassID)
        {
            List<MaterialGatePassModel> gatepassmodel = new List<MaterialGatePassModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                gatepassmodel = UserHelper.GetMaterialGatePassData(SessionStaffNumber, GatePassID, IPAddress);

                int i = 1;

                foreach (var gatepass in gatepassmodel)
                {
                    if (gatepass.IsAttachmentPresent == false)
                    {
                        gatepass.Items = UserHelper.GetMaterialGatePassItems(SessionStaffNumber, GatePassID, IPAddress);
                    }
                    else
                    {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:MaterialGatePassFileLocationForDownload"];

                        ViewBag.AlteredAttachementLocation = gatepass.AttachmentLocation.Replace(loglocation, "../GatePassAttachment/");
                        if (gatepass.OldStatus == "PARTIAL IN" || gatepass.OldStatus == "IN")
                        {
                            gatepass.AlteredInAttachementLocation = gatepass.InAttachmentLocation.Replace(loglocation, "../GatePassAttachment/");
                        }
                    }


                    if (gatepass.SignatureFileLocation != null &&
                        (gatepass.OldStatus == "OUT" || gatepass.OldStatus == "PARTIAL IN" || gatepass.OldStatus == "IN"))
                    {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:MaterialGatePassSignatureLocationForDownload"];

                        ViewBag.SignatureFileLocation = gatepass.SignatureFileLocation.Replace(loglocation, "../GatePassSignature/");
                    }
                    i++;
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(gatepassmodel);
        }

        [HttpPost]
        public IActionResult ViewPendingMaterialGatePassRequest(List<MaterialGatePassModel> model, IFormFile Signature, IFormFile Attachment)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                foreach (var gatepass in model)
                {
                    if (gatepass.NewStatus == "OUT")
                    {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:MaterialGatePassSignatureLocation"];

                        string folderName = loglocation;
                        folderName = folderName.Replace("Year", DateTime.Now.Year.ToString());
                        folderName = folderName.Replace("Month", DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture));
                        folderName = folderName.Replace("GatePassID", gatepass.GatePassID);

                        if (!Directory.Exists(folderName))
                        {
                            Directory.CreateDirectory(folderName);
                        }

                        string Year = DateTime.Now.Year.ToString();
                        string Month = DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture);

                        //string sourceFile = System.IO.Path.Combine("G:\\Mokshith\\SignatureImages\\" + Year + "\\" + Month + "\\" + gatepass.GatePassID, gatepass.GatePassID + ".png");
                        string sourceFile = System.IO.Path.Combine("F:\\Mokshith\\SignatureImages\\" + Year + "\\" + Month + "\\" + gatepass.GatePassID, gatepass.GatePassID + ".png");
                        string destFile = System.IO.Path.Combine(folderName, gatepass.GatePassID + ".png");

                        System.IO.File.Copy(sourceFile, destFile, true);

                        gatepass.SignatureFileLocation = destFile;
                    }
                    else
                    {
                        gatepass.SignatureFileLocation = "";
                    }

                    if (Attachment != null && (gatepass.NewStatus == "PARTIAL IN" || gatepass.NewStatus == "IN"))
                    {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:MaterialGatePassFileLocation"];
                        string folderName = loglocation;
                        folderName = folderName.Replace("Year", DateTime.Now.Year.ToString());
                        folderName = folderName.Replace("Month", DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture));
                        folderName = folderName.Replace("GatePassID", gatepass.GatePassID);
                        //string uploadPath = Server.MapPath(folderName);
                        string webRootPath = _hostingenvironment.ContentRootPath;
                        string newPath = Path.Combine(webRootPath, folderName);
                        if (!Directory.Exists(folderName))
                        {
                            Directory.CreateDirectory(folderName);
                        }
                        string TemporaryFileName = Attachment.FileName;
                        string Extension = TemporaryFileName.Split('.').Last();
                        string FileName = CommonHelper.GeneratePassword(10) + "." + Extension;
                        gatepass.InAttachmentLocation = Path.Combine(folderName, FileName);
                        string envpath = folderName + "\\" + FileName;

                        using (var stream = new FileStream(gatepass.InAttachmentLocation, FileMode.Create))
                        {
                            Attachment.CopyTo(stream);
                        }

                        gatepass.IsAttachmentPresent = true;
                    }
                    else
                    {
                        gatepass.InAttachmentLocation = "";

                        gatepass.IsAttachmentPresent = false;
                    }


                    bool IsUpdated = UserHelper.UpdateMaterialGatePassData(SessionStaffNumber, gatepass, BrowserName, IPAddress);

                    if (IsUpdated)
                    {
                        if (gatepass.NewStatus == "CHECKED")
                        {
                            FromToGroupCenterHeadModel fromtogroupcenter = UserHelper.GetGroupCenterHeadOfFromGatePass(SessionStaffNumber, gatepass.GatePassID, IPAddress);

                            // send email
                            string[] EmailAndName = CommonHelper.GetEmailAndName(gatepass.PreparedByStaffNumber, IPAddress);
                            string EmailID = EmailAndName[0];

                            string[] ReportingOfficerEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string ReportingOfficerEmailID = ReportingOfficerEmailAndName[0];

                            // send email to from Group Head
                            string[] FromGroupHeadEmailAndName = CommonHelper.GetEmailAndName(fromtogroupcenter.FromGroupHeadStaffNumber, IPAddress);
                            string FromGroupHeadEmailID = FromGroupHeadEmailAndName[0];

                            // send email to from Center Head
                            string[] FromCenterHeadEmailAndName = CommonHelper.GetEmailAndName(fromtogroupcenter.FromCenterHeadStaffNumber, IPAddress);
                            string FromCenterHeadEmailID = FromCenterHeadEmailAndName[0];

                            string Message = @"Dear Sir/Madam,<br/><br/>Your Material Gate Pass request" +
                                            " with Gate Pass ID <b>" + gatepass.GatePassID + "</b> has been <b>CHECKED</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Material Gate Pass";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(EmailID);
                            EmailList.Add(ReportingOfficerEmailID);
                            EmailList.Add(FromGroupHeadEmailID);
                            EmailList.Add(FromCenterHeadEmailID);

                            Console.WriteLine(gatepass.NewStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "GatePass request has been CHECKED";
                        }
                        else if (gatepass.NewStatus == "PERMITTED")
                        {
                            // send email
                            string[] EmailAndName = CommonHelper.GetEmailAndName(gatepass.PreparedByStaffNumber, IPAddress);
                            string EmailID = EmailAndName[0];

                            string[] ActionTakenEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string ActionTakenEmailID = ActionTakenEmailAndName[0];

                            string Message = @"Dear Sir/Madam,<br/><br/>Your Material Gate Pass request" +
                                            " with Gate Pass ID <b>" + gatepass.GatePassID + "</b> has been <b>PERMITTED</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Material Gate Pass";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(EmailID);
                            EmailList.Add(ActionTakenEmailID);

                            Console.WriteLine(gatepass.NewStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "GatePass request has been PERMITTED";
                        }
                        else if (gatepass.NewStatus == "OUT")
                        {
                            // send email
                            string[] EmailAndName = CommonHelper.GetEmailAndName(gatepass.PreparedByStaffNumber, IPAddress);
                            string EmailID = EmailAndName[0];

                            string[] ActionTakenEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string ActionTakenEmailID = ActionTakenEmailAndName[0];

                            string Message = @"Dear Sir/Madam,<br/><br/>Your Material Gate Pass request" +
                                            " with Gate Pass ID <b>" + gatepass.GatePassID + "</b> has taken <b>OUT</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Material Gate Pass";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(EmailID);
                            EmailList.Add(ActionTakenEmailID);

                            Console.WriteLine(gatepass.NewStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "GatePass request has been taken OUT";
                        }
                        else if (gatepass.NewStatus == "PARTIAL IN")
                        {
                            // send email
                            string[] EmailAndName = CommonHelper.GetEmailAndName(gatepass.PreparedByStaffNumber, IPAddress);
                            string EmailID = EmailAndName[0];

                            string[] ActionTakenEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string ActionTakenEmailID = ActionTakenEmailAndName[0];

                            string Message = @"Dear Sir/Madam,<br/><br/>Your Material Gate Pass request" +
                                            " with Gate Pass ID <b>" + gatepass.GatePassID + "</b> has set to <b>PARTIAL IN</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Material Gate Pass";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(EmailID);
                            EmailList.Add(ActionTakenEmailID);

                            Console.WriteLine(gatepass.NewStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "GatePass request has been set to PARTIAL IN";
                        }
                        else if (gatepass.NewStatus == "IN")
                        {
                            // send email
                            string[] EmailAndName = CommonHelper.GetEmailAndName(gatepass.PreparedByStaffNumber, IPAddress);
                            string EmailID = EmailAndName[0];

                            string[] ActionTakenEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string ActionTakenEmailID = ActionTakenEmailAndName[0];

                            string Message = @"Dear Sir/Madam,<br/><br/>Your Material Gate Pass request" +
                                            " with Gate Pass ID <b>" + gatepass.GatePassID + "</b> has set to <b>IN</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Material Gate Pass";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(EmailID);
                            EmailList.Add(ActionTakenEmailID);

                            Console.WriteLine(gatepass.NewStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "GatePass request has been set to IN";
                        }
                        else if (gatepass.NewStatus == "REJECTED")
                        {
                            // send email
                            string[] EmailAndName = CommonHelper.GetEmailAndName(gatepass.PreparedByStaffNumber, IPAddress);
                            string EmailID = EmailAndName[0];

                            string[] ActionTakenEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string ActionTakenEmailID = ActionTakenEmailAndName[0];

                            string Message = @"Dear Sir/Madam,<br/><br/>Your Material Gate Pass request" +
                                            " with Gate Pass ID <b>" + gatepass.GatePassID + "</b> has been <b>REJECTED</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Material Gate Pass";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(EmailID);
                            EmailList.Add(ActionTakenEmailID);

                            Console.WriteLine(gatepass.NewStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "GatePass request has been REJECTED";
                        }

                        return RedirectToAction("PendingMaterialGatePassRequestWithoutFilter", "User");

                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                        return RedirectToAction("PendingMaterialGatePassRequest", "User");
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
            return RedirectToAction("PendingMaterialGatePassRequest", "User");
        }

        // view break

        [HttpGet]
        public IActionResult AssignedMaterialGatePassRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult AssignedMaterialGatePassRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            List<MaterialGatePassModel> gatepassmodel = new List<MaterialGatePassModel>();
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetString("Role") == null)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                gatepassmodel = UserHelper.GetAssignedMaterialGatePassRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(gatepassmodel);
        }

        public IActionResult AssignedMaterialGatePassRequestWithoutFilter()
        {
            ViewBag.Search = true;

            List<MaterialGatePassModel> gatepassmodel = new List<MaterialGatePassModel>();
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetString("Role") == null)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                gatepassmodel = UserHelper.GetAssignedMaterialGatePassRequestListWitoutFilter(SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(gatepassmodel);
        }

        // view break

        public IActionResult ViewAssignedMaterialGatePassRequest(string GatePassID)
        {
            List<MaterialGatePassModel> materialgatepass = new List<MaterialGatePassModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                materialgatepass = UserHelper.GetMaterialGatePassData(SessionStaffNumber, GatePassID, IPAddress);

                foreach (var gatepass in materialgatepass)
                {
                    if (gatepass.IsAttachmentPresent == false)
                    {
                        gatepass.Items = UserHelper.GetMaterialGatePassItems(SessionStaffNumber, GatePassID, IPAddress);
                    }
                    else
                    {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:MaterialGatePassFileLocationForDownload"];

                        ViewBag.AlteredAttachementLocation = gatepass.AttachmentLocation.Replace(loglocation, "../GatePassAttachment/");
                        if (gatepass.OldStatus == "PARTIAL IN" || gatepass.OldStatus == "IN")
                        {
                            gatepass.AlteredInAttachementLocation = gatepass.InAttachmentLocation.Replace(loglocation, "../GatePassAttachment/");
                        }
                    }


                    if (gatepass.SignatureFileLocation != null &&
                        (gatepass.OldStatus == "OUT" || gatepass.OldStatus == "PARTIAL IN" || gatepass.OldStatus == "IN"))
                    {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:MaterialGatePassSignatureLocationForDownload"];

                        ViewBag.SignatureFileLocation = gatepass.SignatureFileLocation.Replace(loglocation, "../GatePassSignature/");
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(materialgatepass);
        }

        // view break

        public IActionResult RedirectToSecurity()
        {
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            //if(IPAddress == "172.18.21.7")
            if(IPAddress == "172.18.7.40")
            //if(IPAddress == "172.18.100.81")
            //if(IPAddress == "172.18.2.205")
            {
                //return Redirect("http://172.18.21.7:8085/signature");
                return Redirect("http://172.18.7.40:8085/signature");
                //return Redirect("http://172.18.100.81:8085/signature");
                //return Redirect("http://172.18.2.205:8085/signature");
            }
            else
            {
                return RedirectToAction("AccessDenied","User");
            }

        }

        // view break

        public IActionResult PrintMaterialGatePass(string GatePassID)
        {
            List<MaterialGatePassModel> gatepassmodel = new List<MaterialGatePassModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                gatepassmodel = UserHelper.GetMaterialGatePassData(SessionStaffNumber, GatePassID, IPAddress);


                foreach (var gatepass in gatepassmodel)
                {
                    if (gatepass.IsAttachmentPresent == false)
                    {
                        gatepass.Items = UserHelper.GetMaterialGatePassItems(SessionStaffNumber, GatePassID, IPAddress);
                    }
                    else
                    {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:MaterialGatePassFileLocationForDownload"];

                        ViewBag.AlteredAttachementLocation = gatepass.AttachmentLocation.Replace(loglocation, "../GatePassAttachment/");
                    }

                    if (gatepass.SignatureFileLocation != null)
                    {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:MaterialGatePassSignatureLocationForDownload"];

                        ViewBag.SignatureFileLocation = gatepass.SignatureFileLocation.Replace(loglocation, "../GatePassSignature/");
                    }
                    break;
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(gatepassmodel);
        }

        // Material Gate Pass end
        // *************************************************************************************************** //   

        // view break

        // *************************************************************************************************** //
        // Leave start

        public IActionResult Leave()
        {
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                CommonHelper.LogError(error, 0000, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        // view break

        [HttpGet]
        public IActionResult ApplyLeave()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            ViewBag.FromDateError = TempData["FromDateError"];
            TempData["FromDateError"] = "";

            ViewBag.ToDateError = TempData["ToDateError"];
            TempData["ToDateError"] = "";

            LeaveModel leavemodel = new LeaveModel();

            ViewBag.IsCenterSelected = false;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                EmployeeListModel employee = UserHelper.GetUserDataFromStaffNumber(SessionStaffNumber, IPAddress);
                leavemodel.StaffNumber = employee.StaffNumber;
                leavemodel.StaffFirstName = employee.Name;
                leavemodel.StaffDesignation = employee.Designation;
                leavemodel.GroupID = employee.GroupID;
                leavemodel.CenterID = employee.CenterID;
                leavemodel.GroupName = employee.GroupName;
                leavemodel.CenterName = employee.CenterName;
                leavemodel.EntryLevel = employee.EntryLevel;

                DateTime Today = DateTime.Now;

                int DateDifference = Convert.ToInt32((Today - employee.DateOfJoin).TotalDays);

                ViewBag.JoingDateDifference = DateDifference;

                leavemodel.ReportingOfficerListModel = UserHelper.GetEmployeeListForReportingOfficer(SessionStaffNumber, IPAddress, leavemodel.GroupID);

                ViewBag.EarnedLeaveCount = UserHelper.GetRemainingEarnedLeaveCount(leavemodel.StaffNumber, IPAddress);
                ViewBag.CasualLeaveCount = UserHelper.GetRemainingCasualLeaveCount(leavemodel.StaffNumber, IPAddress);
                ViewBag.HalfPayLeaveCount = UserHelper.GetRemainingHalfPayLeaveCount(leavemodel.StaffNumber, IPAddress);
                ViewBag.RestrictedLeaveCount = UserHelper.GetRemainingRestrictedLeaveCount(leavemodel.StaffNumber, IPAddress);

                double TempCompensatoryOffCount = UserHelper.GetRemainingCompensatoryOffCount(leavemodel.StaffNumber, IPAddress);
                ViewBag.CompensatoryOffCount = (TempCompensatoryOffCount / 60) / 8;

                //leavemodel.CompensatoryOffModel = UserHelper.GetPendingOverTimeData(leavemodel.StaffNumber, IPAddress);

                //foreach(var overtime in leavemodel.CompensatoryOffModel)
                //{
                //    overtime.TempVariable = "Over Time done on " + overtime.AlteredApplyingOnDate + " with total duration of " + overtime.TotalTime + " minutes";
                //}
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
                ViewBag.ExceptionError = "An error has occured. Please contact system administrator.";
            }

            return View(leavemodel);
        }

        [HttpPost]
        public IActionResult ApplyLeave(LeaveModel model, IFormFile Attachment)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                // to get IPAddress
                string IpAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

                if(model.TypeOfLeave == "RH")
                {
                    var rhholidays = UserHelper.GetHolidays(IPAddress, SessionStaffNumber, "RH", DateTime.Now.Year);

                    bool fromflag = false;
                    bool toflag = false;

                    foreach (var rhholiday in rhholidays)
                    {
                        string alternateleavefrom = Convert.ToDateTime(model.LeaveFrom).ToString("dd/MM/yyyy");
                        string alternateleaveto = Convert.ToDateTime(model.LeaveTo).ToString("dd/MM/yyyy");

                        if (rhholiday.Date == alternateleavefrom)
                        {
                            fromflag = true;
                        }

                        if (rhholiday.Date == alternateleaveto)
                        {
                            toflag = true;
                        }
                    }

                    if (fromflag == false)
                    {
                        TempData["FromDateError"] = "Please select a restricted holiday";
                        return RedirectToAction("ApplyLeave", "User");
                    }
                    else if (toflag == false)
                    {
                        TempData["ToDateError"] = "Please select a restricted holiday";
                        return RedirectToAction("ApplyLeave", "User");
                    }
                }

                if(model.TypeOfLeave == "CO")
                {
                    int totalovertimeduration = 0;

                    totalovertimeduration = UserHelper.GetCompensatoryOffDataDuration(model.StaffNumber, IPAddress);

                    int TempTotalLeave = Convert.ToInt32((model.TotalLeave * 8) * 60);

                    if(TempTotalLeave > totalovertimeduration)
                    {
                        LeaveModel leavemodel = new LeaveModel();

                        EmployeeListModel employee = UserHelper.GetUserDataFromStaffNumber(SessionStaffNumber, IPAddress);
                        leavemodel.StaffNumber = employee.StaffNumber;
                        leavemodel.StaffFirstName = employee.Name;
                        leavemodel.StaffDesignation = employee.Designation;
                        leavemodel.GroupID = employee.GroupID;
                        leavemodel.CenterID = employee.CenterID;
                        leavemodel.GroupName = employee.GroupName;
                        leavemodel.CenterName = employee.CenterName;
                        leavemodel.EntryLevel = employee.EntryLevel;

                        DateTime Today = DateTime.Now;

                        int DateDifference = Convert.ToInt32((Today - employee.DateOfJoin).TotalDays);

                        ViewBag.JoingDateDifference = DateDifference;

                        leavemodel.ReportingOfficerListModel = UserHelper.GetEmployeeListForReportingOfficer(SessionStaffNumber, IPAddress, leavemodel.GroupID);

                        ViewBag.EarnedLeaveCount = UserHelper.GetRemainingEarnedLeaveCount(leavemodel.StaffNumber, IPAddress);
                        ViewBag.CasualLeaveCount = UserHelper.GetRemainingCasualLeaveCount(leavemodel.StaffNumber, IPAddress);
                        ViewBag.HalfPayLeaveCount = UserHelper.GetRemainingHalfPayLeaveCount(leavemodel.StaffNumber, IPAddress);
                        ViewBag.RestrictedLeaveCount = UserHelper.GetRemainingRestrictedLeaveCount(leavemodel.StaffNumber, IPAddress);

                        ViewBag.TotalTimeError = "You don't have enough Compensatory Off balance";

                        return View(leavemodel);
                    }
                }

                // to get the count from database table
                int LeaveCount = UserHelper.GetTodaysLeaveCount(model.StaffNumber, IpAddress);

                LeaveCount++;

                model.LeaveID = "LEAVE" + DateTime.Now.ToString("ddMMyy") + LeaveCount;

                if(Attachment != null)
                {
                    var configuration = CommonHelper.GetConfig();
                    var loglocation = configuration["FileLocations:LeaveAttachmentFileLocation"];
                    string folderName = loglocation;
                    folderName = folderName.Replace("Year", DateTime.Now.Year.ToString());
                    folderName = folderName.Replace("Month", DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture));
                    folderName = folderName.Replace("LeaveID", model.LeaveID);
                    //string uploadPath = Server.MapPath(folderName);
                    string webRootPath = _hostingenvironment.ContentRootPath;
                    string newPath = Path.Combine(webRootPath, folderName);
                    if (!Directory.Exists(folderName))
                    {
                        Directory.CreateDirectory(folderName);
                    }
                    string TemporaryFileName = Attachment.FileName;
                    string Extension = TemporaryFileName.Split('.').Last();
                    string FileName = CommonHelper.GeneratePassword(10) + "." + Extension;
                    model.AttchmentLocation = Path.Combine(folderName, FileName);
                    string envpath = folderName + "\\" + FileName;

                    using (var stream = new FileStream(model.AttchmentLocation, FileMode.Create))
                    {
                        Attachment.CopyTo(stream);
                    }
                }
                else
                {
                    model.AttchmentLocation = " ";
                }

                bool IsInserted = UserHelper.ApplyLeave(model, BrowserName, IPAddress);

                if (IsInserted)
                {
                    TempData["SuccessMessage"] = "Leave with ID " + model.LeaveID + " applied successfully";

                    // send email to user so that Indent is raised
                    string[] IndentorEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                    string IndentorEmailID = IndentorEmailAndName[0];
                    // reporting officer
                    string[] ReportingOfficerEmailAndName = CommonHelper.GetEmailAndName(model.ReportingOfficer, IPAddress);
                    string ReportingOfficerEmailID = ReportingOfficerEmailAndName[0];

                    string Message = @"Dear Sir/Madam,<br/><br/>Leave has" +
                                        " been applied successfully and Leave ID is <b>" + model.LeaveID + "</b>." +
                                        "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                    string Subject = "ebandhu - Leave";
                    List<string> EmailList = new List<string>();
                    EmailList.Add(IndentorEmailID);
                    EmailList.Add(ReportingOfficerEmailID);

                    Console.WriteLine("RAISED");
                    foreach (var Email in EmailList)
                    {
                        Console.WriteLine(Email);
                    }
                    Console.WriteLine("---");

                    CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                    return RedirectToAction("ApplyLeave", "User");
                }
                else
                {
                    TempData["ExceptionError"] = "An error has occured. Please contact System Administrator.";
                    return RedirectToAction("ApplyLeave", "User");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
                ViewBag.ExceptionError = "An error has occured. Please contact system administrator.";
            }

            return View(model);
        }

        // view break

        [HttpGet]
        public IActionResult MyLeaveRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult MyLeaveRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            List<LeaveModel> leavemodel = new List<LeaveModel>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                leavemodel = UserHelper.GetMyLeaveRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(leavemodel);
        }

        public IActionResult MyLeaveRequestWithoutFilter()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            List<LeaveModel> leavemodel = new List<LeaveModel>();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                leavemodel = UserHelper.GetMyLeaveRequestListWithoutFilter(SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(leavemodel);
        }

        // view break

        [HttpGet]
        public IActionResult ViewMyLeaveRequest(string LeaveID)
        {
            List<LeaveModel> leavemodel = new List<LeaveModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                leavemodel = UserHelper.GetLeaveData(SessionStaffNumber, LeaveID, IPAddress);

                foreach (var leave in leavemodel)
                {
                    if (leave.AttchmentLocation != " ")
                    {
                        ViewBag.IsAttachementPresent = true;

                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:LeaveAttachmentFileLocationForDownload"];

                        leave.AlteredAttchmentLocation = leave.AttchmentLocation.Replace(loglocation, "../LeaveAttachmentUploads/");
                    }
                    else
                    {
                        ViewBag.IsAttachementPresent = false;
                    }
                }

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(leavemodel);
        }

        [HttpPost]
        public IActionResult ViewMyLeaveRequest(List<LeaveModel> model)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                foreach (var leave in model)
                {
                    bool IsUpdated = UserHelper.UpdateLeaveData(SessionStaffNumber, leave, BrowserName, IPAddress);
                    if (IsUpdated)
                    {
                        if (leave.NewStatus == "CANCELLED")
                        {
                            // send email
                            string[] EmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string EmailID = EmailAndName[0];
                            string FirstName = EmailAndName[1];
                            string Message = @"Dear <b> Dear Sir/Madam,</b><br/><br/>Leave" +
                                            " with Leave ID <b>" + leave.LeaveID + "</b> has been <b>CANCELLED</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Leave";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(EmailID);

                            Console.WriteLine(leave.NewStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "Leave has been CANCELLED";
                        }

                        return RedirectToAction("MyLeaveRequestWithoutFilter", "User");
                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                        return RedirectToAction("MyLeaveRequestWithoutFilter", "User");
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
            return RedirectToAction("MyLeaveRequestWithoutFilter", "User");
        }

        // view break

        [HttpGet]
        public IActionResult PendingLeaveRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult PendingLeaveRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            List<LeaveModel> leavemodel = new List<LeaveModel>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                leavemodel = UserHelper.GetPendingLeaveRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(leavemodel);
        }

        public IActionResult PendingLeaveRequestWithoutFilter() 
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            List<LeaveModel> leavemodel = new List<LeaveModel>();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                leavemodel = UserHelper.GetPendingLeaveRequestListWithoutFilter(SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(leavemodel);
        }

        // view break

        [HttpGet]
        public IActionResult ViewPendingLeaveRequest(string LeaveID)
        {
            List<LeaveModel> leavemodel = new List<LeaveModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                leavemodel = UserHelper.GetLeaveData(SessionStaffNumber, LeaveID, IPAddress);

                bool flag = true;

                foreach (var leave in leavemodel)
                {
                    if (leave.AttchmentLocation != " ")
                    {
                        ViewBag.IsAttachementPresent = true;

                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:LeaveAttachmentFileLocationForDownload"];

                        leave.AlteredAttchmentLocation = leave.AttchmentLocation.Replace(loglocation, "../LeaveAttachmentUploads/");
                    }
                    else
                    {
                        ViewBag.IsAttachementPresent = false;
                    }

                    if(flag)
                    {
                        ViewBag.EarnedLeaveCount = UserHelper.GetRemainingEarnedLeaveCount(leave.StaffNumber, IPAddress);
                        ViewBag.CasualLeaveCount = UserHelper.GetRemainingCasualLeaveCount(leave.StaffNumber, IPAddress);
                        ViewBag.HalfPayLeaveCount = UserHelper.GetRemainingHalfPayLeaveCount(leave.StaffNumber, IPAddress);
                        ViewBag.RestrictedLeaveCount = UserHelper.GetRemainingRestrictedLeaveCount(leave.StaffNumber, IPAddress);

                        ViewBag.EmployeeType = leave.EmployeeType;
                    }

                    string AdminGroupID = CommonHelper.GetGroupID(SessionStaffNumber, "Administration", IPAddress);

                    string StaffEmployeeGroup = HttpContext.Session.GetString("EmployeeGroup");
                    leave.EmployeeList = UserHelper.GetEmployeeListForIndentAssign(SessionStaffNumber, IPAddress, StaffEmployeeGroup, Convert.ToInt32(AdminGroupID));

                    flag = false;
                }

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(leavemodel);
        }

        [HttpPost]
        public IActionResult ViewPendingLeaveRequest(List<LeaveModel> model)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                foreach (var leave in model)
                {
                    bool IsUpdated = UserHelper.UpdateLeaveData(SessionStaffNumber, leave, BrowserName, IPAddress);
                    if (IsUpdated)
                    {
                        if (leave.NewStatus == "FORWARDED")
                        {
                            FromToGroupCenterHeadModel fromtogroupcenter = UserHelper.GetGroupCenterHeadOfFromLeave(SessionStaffNumber, leave.LeaveID, IPAddress);

                            // send email
                            string[] EmailAndName = CommonHelper.GetEmailAndName(leave.StaffNumber, IPAddress);
                            string EmailID = EmailAndName[0];

                            string[] ReportingOfficerEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string ReportingOfficerEmailID = ReportingOfficerEmailAndName[0];

                            // send email to from Group Head
                            string[] FromGroupHeadEmailAndName = CommonHelper.GetEmailAndName(fromtogroupcenter.FromGroupHeadStaffNumber, IPAddress);
                            string FromGroupHeadEmailID = FromGroupHeadEmailAndName[0];

                            // send email to from Center Head
                            string[] FromCenterHeadEmailAndName = CommonHelper.GetEmailAndName(fromtogroupcenter.FromCenterHeadStaffNumber, IPAddress);
                            string FromCenterHeadEmailID = FromCenterHeadEmailAndName[0];

                            string Message = @"Dear <b> Dear Sir/Madam,</b><br/><br/>Leave" +
                                            " with Leave ID <b>" + leave.LeaveID + "</b> has been <b>FORWARDED</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Leave";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(EmailID);
                            EmailList.Add(ReportingOfficerEmailID);
                            EmailList.Add(FromGroupHeadEmailID);
                            EmailList.Add(FromCenterHeadEmailID);

                            Console.WriteLine(leave.NewStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "Leave has been FORWARDED";
                        }
                        else if (leave.NewStatus == "APPROVED")
                        {
                            // send email
                            string[] EmailAndName = CommonHelper.GetEmailAndName(leave.StaffNumber, IPAddress);
                            string EmailID = EmailAndName[0];

                            string[] ActionTakenEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string ActionTakenEmailID = ActionTakenEmailAndName[0];

                            string Message = @"Dear <b> Dear Sir/Madam,</b><br/><br/>Leave" +
                                            " with Leave ID <b>" + leave.LeaveID + "</b> has been <b>APPROVED</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Leave";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(EmailID);

                            Console.WriteLine(leave.NewStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "Leave has been APPROVED";
                        }

                        return RedirectToAction("PendingLeaveRequestWithoutFilter", "User");
                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                        return RedirectToAction("PendingLeaveRequestWithoutFilter", "User");
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
            return RedirectToAction("PendingLeaveRequestWithoutFilter", "User");
        }

        // Leave end
        // *************************************************************************************************** //

        // view break

        // *************************************************************************************************** //
        // Permission start

        public IActionResult PermissionSlip()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        // view break

        [HttpGet]
        public IActionResult ApplyPermissionSlip()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            PermissionSlipModel permission=new PermissionSlipModel();
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                EmployeeListModel employee = UserHelper.GetUserDataFromStaffNumber(SessionStaffNumber, IPAddress);
                permission.StaffNumber = employee.StaffNumber;
                permission.StaffName = employee.Name;
                permission.StaffDesignation = employee.Designation;
                permission.GroupID = employee.GroupID;
                permission.CenterID = employee.CenterID;
                permission.GroupName = employee.GroupName;
                permission.CenterName = employee.CenterName;

                permission.ReportingOfficerListModel = UserHelper.GetEmployeeListForReportingOfficer(SessionStaffNumber, IPAddress, permission.GroupID);

                var PermissionCount = UserHelper.GetPermissionCount(SessionStaffNumber, IPAddress);

                permission.PermissionCount = PermissionCount.PermissionCount;
                permission.SpecialPermissionCount = PermissionCount.SpecialPermissionCount;

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(permission);
        }

        [HttpPost]
        public IActionResult ApplyPermissionSlip(PermissionSlipModel model)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                // to get IPAddress
                string IpAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

                // to get the count from database table
                int PermissionCount = UserHelper.GetTodaysPermissionCount(model.StaffNumber, IpAddress);

                PermissionCount++;

                model.PermissionID = "PRM" + DateTime.Now.ToString("ddMMyy") + PermissionCount;

                bool IsInserted = UserHelper.ApplyPermission(model, BrowserName, IPAddress);

                if (IsInserted)
                {
                    TempData["SuccessMessage"] = "Permission " + model.PermissionID + " applied successfully";

                    // send email to user so that Indent is raised
                    string[] EmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                    string EmailID = EmailAndName[0];
                    string Message = @"Dear Sir/Madam,<br/><br/>Permission has" +
                                        " been applied successfully and Permission ID is <b>" + model.PermissionID + "</b>." +
                                        "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                    string Subject = "ebandhu - Permission";
                    List<string> EmailList = new List<string>();
                    EmailList.Add(EmailID);
                    CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                    return RedirectToAction("ApplyPermissionSlip", "User");
                }
                else
                {
                    TempData["ExceptionError"] = "An error has occured. Please contact System Administrator.";
                    return RedirectToAction("ApplyPermissionSlip", "User");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
                ViewBag.ExceptionError = "An error has occured. Please contact system administrator.";
            }

            return View(model);
        }

        // view break

        [HttpGet]
        public IActionResult MyPermissionRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult MyPermissionRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<PermissionSlipModel> permissionmodel = new List<PermissionSlipModel>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                permissionmodel = UserHelper.GetMyPermissionRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);

                foreach (var permission in permissionmodel)
                {
                    permission.AlternateFromTime = Convert.ToDateTime(permission.AlternateFromTime).ToString("hh:mm:ss tt");
                    permission.AlternateToTime = Convert.ToDateTime(permission.AlternateToTime).ToString("hh:mm:ss tt");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(permissionmodel);
        }

        public IActionResult MyPermissionRequestWithoutFilter()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<PermissionSlipModel> permissionmodel = new List<PermissionSlipModel>();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                permissionmodel = UserHelper.GetMyPermissionRequestListWithoutFilter(SessionStaffNumber, IPAddress);

                foreach (var permission in permissionmodel)
                {
                    permission.AlternateFromTime = Convert.ToDateTime(permission.AlternateFromTime).ToString("hh:mm:ss tt");
                    permission.AlternateToTime = Convert.ToDateTime(permission.AlternateToTime).ToString("hh:mm:ss tt");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(permissionmodel);
        }

        // view break

        [HttpGet]
        public IActionResult ViewMyPermissionRequest(string PermissionID)
        {
            List<PermissionSlipModel> permissionmodel = new List<PermissionSlipModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                permissionmodel = UserHelper.GetPermissionData(SessionStaffNumber, PermissionID, IPAddress);

                foreach (var permission in permissionmodel)
                {
                    permission.AlternateFromTime = Convert.ToDateTime(permission.AlternateFromTime).ToString("hh:mm:ss tt");
                    permission.AlternateToTime = Convert.ToDateTime(permission.AlternateToTime).ToString("hh:mm:ss tt");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(permissionmodel);
        }

        [HttpPost]
        public IActionResult ViewMyPermissionRequest(List<PermissionSlipModel> model)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                foreach (var permission in model)
                {
                    bool IsUpdated = UserHelper.UpdatePermissionData(SessionStaffNumber, permission, BrowserName, IPAddress);
                    if (IsUpdated)
                    {
                        TempData["SuccessMessage"] = "Update successful";
                        return RedirectToAction("MyPermissionRequestWithoutFilter", "User");
                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                        return RedirectToAction("MyPermissionRequestWithoutFilter", "User");
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
            return RedirectToAction("MyPermissionRequestWithoutFilter", "User");
        }

        // view break

        [HttpGet]
        public IActionResult PendingPermissionRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult PendingPermissionRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<PermissionSlipModel> permissionmodel = new List<PermissionSlipModel>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                permissionmodel = UserHelper.GetPendingPermissionRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);

                foreach (var permission in permissionmodel)
                {
                    permission.AlternateFromTime = Convert.ToDateTime(permission.AlternateFromTime).ToString("hh:mm:ss tt");
                    permission.AlternateToTime = Convert.ToDateTime(permission.AlternateToTime).ToString("hh:mm:ss tt");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(permissionmodel);
        }

        public IActionResult PendingPermissionRequestWithoutFilter()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<PermissionSlipModel> permissionmodel = new List<PermissionSlipModel>();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                permissionmodel = UserHelper.GetPendingPermissionRequestListWithoutFilter(SessionStaffNumber, IPAddress);

                foreach (var permission in permissionmodel)
                {
                    permission.AlternateFromTime = Convert.ToDateTime(permission.AlternateFromTime).ToString("hh:mm:ss tt");
                    permission.AlternateToTime = Convert.ToDateTime(permission.AlternateToTime).ToString("hh:mm:ss tt");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(permissionmodel);
        }

        // view break

        [HttpGet]
        public IActionResult ViewPendingPermissionRequest(string PermissionID)
        {
            List<PermissionSlipModel> permissionmodel = new List<PermissionSlipModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                permissionmodel = UserHelper.GetPermissionData(SessionStaffNumber, PermissionID, IPAddress);

                foreach (var permission in permissionmodel)
                {
                    string AdminGroupID = CommonHelper.GetGroupID(SessionStaffNumber, "Administration", IPAddress);

                    string StaffEmployeeGroup = HttpContext.Session.GetString("EmployeeGroup");
                    permission.EmployeeList = UserHelper.GetEmployeeListForIndentAssign(SessionStaffNumber, IPAddress, StaffEmployeeGroup, Convert.ToInt32(AdminGroupID));

                    permission.AlternateFromTime = Convert.ToDateTime(permission.AlternateFromTime).ToString("hh:mm:ss tt");
                    permission.AlternateToTime = Convert.ToDateTime(permission.AlternateToTime).ToString("hh:mm:ss tt");

                    break;
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(permissionmodel);
        }

        [HttpPost]
        public IActionResult ViewPendingPermissionRequest(List<PermissionSlipModel> model)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                foreach (var permission in model)
                {
                    bool IsUpdated = UserHelper.UpdatePermissionData(SessionStaffNumber, permission, BrowserName, IPAddress);
                    if (IsUpdated)
                    {
                        TempData["SuccessMessage"] = "Update successful";
                        return RedirectToAction("PendingPermissionRequestWithoutFilter", "User");
                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                        return RedirectToAction("PendingPermissionRequestWithoutFilter", "User");
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
            return RedirectToAction("PendingPermissionRequestWithoutFilter", "User");
        }

        // view break

        [HttpGet]
        public IActionResult AssignedPermissionRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult AssignedPermissionRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<PermissionSlipModel> permissionmodel = new List<PermissionSlipModel>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                permissionmodel = UserHelper.GetAssignedPermissionRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(permissionmodel);
        }

        public IActionResult AssignedPermissionRequestWithoutFilter()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<PermissionSlipModel> permissionmodel = new List<PermissionSlipModel>();
            try
            {

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                permissionmodel = UserHelper.GetAssignedPermissionRequestListWithoutFilter(SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(permissionmodel);
        }

        // view break

        [HttpGet]
        public IActionResult ViewAssignedPermissionRequest(string PermissionID)
        {
            List<PermissionSlipModel> permissionmodel = new List<PermissionSlipModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                permissionmodel = UserHelper.GetPermissionData(SessionStaffNumber, PermissionID, IPAddress);

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(permissionmodel);
        }

        // view break

        [HttpGet]
        public IActionResult OutboxPermissionRequests()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult OutboxPermissionRequests(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            List<PermissionSlipModel> permissionrequests = new List<PermissionSlipModel>();
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetString("Role") == null)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                permissionrequests = UserHelper.GetOutboxPermissionRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(permissionrequests);
        }

        public IActionResult OutboxPermissionRequestsWithoutFilter()
        {
            ViewBag.Search = true;

            List<PermissionSlipModel> permissionrequests = new List<PermissionSlipModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetString("Role") == null)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                permissionrequests = UserHelper.GetOutboxPermissionRequestListWithoutFilter(SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(permissionrequests);
        }

        // view break

        public IActionResult ViewOutboxPermissionRequest(string PermissionID)
        {
            List<PermissionSlipModel> permissionmodel = new List<PermissionSlipModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                permissionmodel = UserHelper.GetPermissionData(SessionStaffNumber, PermissionID, IPAddress);

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(permissionmodel);
        }

        // Permission end
        // *************************************************************************************************** //

        // view break

        // *************************************************************************************************** //
        // Purchase Indent Upto 25k start

        public IActionResult PurchaseIndentUpto25k()
        {
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        // view break

        [HttpGet]
        public IActionResult RaisePurchaseIndentUpto25k()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            PurchaseIndentModelUpto25Model purchaseindent = new PurchaseIndentModelUpto25Model();

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                EmployeeListModel employee = UserHelper.GetUserDataFromStaffNumber(SessionStaffNumber, IPAddress);

                purchaseindent.IndentorStaffNumber = employee.StaffNumber;
                purchaseindent.IndentorName = employee.Name;
                purchaseindent.IndentorDesignation = employee.Designation;
                purchaseindent.IndentorGroupID = employee.GroupID;
                purchaseindent.IndentorGroupName = employee.GroupName;
                purchaseindent.IndentorCenterID = employee.CenterID;
                purchaseindent.IndentorCenterName = employee.CenterName;
                purchaseindent.MaterialInvoiceDate = DateTime.Today;
                purchaseindent.MaterialReceivedDate = DateTime.Today;
                purchaseindent.PurchaseVoucherDate = DateTime.Today;
                purchaseindent.PurchaseVoucherInvoiceDate = DateTime.Today;

                purchaseindent.ReportingOfficerModel = UserHelper.GetEmployeeListForReportingOfficer(SessionStaffNumber, IPAddress, purchaseindent.IndentorGroupID);

                purchaseindent.ProjectLeaderModel = UserHelper.GetEmployeeListForProjectLeader(SessionStaffNumber, IPAddress,purchaseindent.IndentorGroupID);

               
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(purchaseindent);
        }

        [HttpPost]
        public IActionResult RaisePurchaseIndentUpto25k(PurchaseIndentModelUpto25Model model, IFormFile MaterialAttachment)
        {
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;            

                // to get the count from database table
                int IndentCount = UserHelper.GetTodaysPurchaseIndentsUpto25kCount(model.IndentorStaffNumber, IPAddress);

                IndentCount = IndentCount + 1;

                //model.IndentID = "IDR" + DateTime.Now.ToString("ddMMyy") + model.FromCenterID + model.ToCenterID + IndentCount;

                model.PurchaseIndentID = "PRUT25K" + DateTime.Now.ToString("ddMMyy") + IndentCount;

                // code for attachment save start
                var configuration = CommonHelper.GetConfig();

                var loglocation = configuration["FileLocations:PurhaseIndentUpto25kAttachmentFileLocation"];
                string folderName = loglocation;
                folderName = folderName.Replace("Year", DateTime.Now.Year.ToString());
                folderName = folderName.Replace("Month", DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture));
                folderName = folderName.Replace("PurchaseIndentID", model.PurchaseIndentID);
                //string uploadPath = Server.MapPath(folderName);            
                string webRootPath = _hostingenvironment.ContentRootPath;
                string newPath = Path.Combine(webRootPath, folderName);

                if (!Directory.Exists(folderName))
                {
                    Directory.CreateDirectory(folderName);
                }

                if (MaterialAttachment != null)
                {
                    string TemporaryFileName = MaterialAttachment.FileName;
                    string Extension = TemporaryFileName.Split('.').Last();
                    string FileName = CommonHelper.GeneratePassword(10) + "." + Extension;
                    model.TSSAttachmentLocation = Path.Combine(folderName, FileName);
                    string envpath = folderName + "\\" + FileName;

                    using (var stream = new FileStream(model.TSSAttachmentLocation, FileMode.Create))
                    {
                        MaterialAttachment.CopyTo(stream);   
                    }
                }
                else
                {
                    model.TSSAttachmentLocation = "";
                }
                
                bool IsUpdated = UserHelper.RaisePurchaseIndentUpto25k(model, BrowserName, IPAddress);

                if (IsUpdated)
                //if (true)
                {
                    TempData["SuccessMessage"] = "Purhase Indent " + model.PurchaseIndentID + " raised successfully";

                    // send email
                    //string[] EmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                    //string EmailID = EmailAndName[0];
                    //string FirstName = EmailAndName[1];
                    //string Message = @"Dear <b>" + FirstName + ",</b><br/>Your request for Inter Departmental Requisition has" +
                    //                    " been raised successfully and your Indent ID is <b>" + model.IndentID + "</b>.";
                    //string Subject = "Inter Departmental Requisition";
                    //CommonHelper.SendEmail(EmailID, Subject, Message, SessionStaffNumber, IPAddress);

                    return RedirectToAction("RaisePurchaseIndentUpto25k", "User");
                }
                else
                {
                    TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                    return RedirectToAction("RaisePurchaseIndentUpto25k", "User");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View();
        }

        // view break

        [HttpGet]
        public IActionResult MyPurchaseIndentUpto25kRequest()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            //TempData["Search"] = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            List<PurchaseIndentModelUpto25Model> indentmodel = new List<PurchaseIndentModelUpto25Model>();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                ViewBag.Search = false;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetMyPurchaseIndentUpto25kRequestList(SessionStaffNumber, Year, 0, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(indentmodel);
        }

        [HttpPost]
        public IActionResult MyPurchaseIndentUpto25kRequest(int Year, int Month, string Status)
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            //TempData["Search"] = true;

            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            List<PurchaseIndentModelUpto25Model> indentmodel = new List<PurchaseIndentModelUpto25Model>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetMyPurchaseIndentUpto25kRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(indentmodel);
        }

        public IActionResult MyPurchaseIndentUpto25kRequestWithoutFilter()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            TempData["Search"] = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            List<PurchaseIndentModelUpto25Model> indentmodel = new List<PurchaseIndentModelUpto25Model>();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetMyPurchaseIndentUpto25kRequestList(SessionStaffNumber, Year, 0, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(indentmodel);
        }

        // view break

        [HttpGet]
        public IActionResult ViewMyPurchaseIndentUpto25k(string PurchaseIndentID)
        {
            List<PurchaseIndentModelUpto25Model> model = new List<PurchaseIndentModelUpto25Model>();

            try
            {
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = UserHelper.GetPurchaseIndentDataUpto25k(SessionStaffNumber, PurchaseIndentID, IPAddress);

                foreach (var indent in model)
                {
                    if (indent.IsAttachmentPresent == true && indent.TSSAttachmentLocation != null)
                    {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:PurhaseIndentUpto25kAttachmentFileLocationForDownload"];

                        indent.AlteredTSSAttachmentLocation = indent.TSSAttachmentLocation.Replace(loglocation, "../PurchaseIndentUploads/");
                    }
                }
            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                CommonHelper.LogError(error, 0000, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult ViewMyPurchaseIndentUpto25k(List<PurchaseIndentModelUpto25Model> model)
        {
            try
            {
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                foreach (var indent in model)
                {
                    bool IsUpdate = UserHelper.UpdatePurchaseIndentUpto25k(indent, BrowserName, IPAddress, SessionStaffNumber);

                    if (IsUpdate)
                    {
                        TempData["SuccessMessage"] = "Update successful";
                        return RedirectToAction("MyPurchaseIndentUpto25kRequestWithoutFilter", "User");
                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                        return RedirectToAction("MyPurchaseIndentUpto25kRequestWithoutFilter", "User");
                    }
                }
            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                CommonHelper.LogError(error, 0000, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(model);
        }

        // view break

        [HttpGet]
        public IActionResult PendingPurchaseIndentUpto25kRequest()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            //TempData["Search"] = true;

            ViewBag.Search = false;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            List<PurchaseIndentModelUpto25Model> indentmodel = new List<PurchaseIndentModelUpto25Model>();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetPendingPurchaseIndentUpto25kRequestList(SessionStaffNumber, Year, 0, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(indentmodel);
        }

        [HttpPost]
        public IActionResult PendingPurchaseIndentUpto25kRequest(int Year, int Month, string Status)
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            TempData["Search"] = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            List<PurchaseIndentModelUpto25Model> indentmodel = new List<PurchaseIndentModelUpto25Model>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                ViewBag.Search = true;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetPendingPurchaseIndentUpto25kRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(indentmodel);
        }

        public IActionResult PendingPurchaseIndentUpto25kRequestWithoutFilter()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            TempData["Search"] = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            List<PurchaseIndentModelUpto25Model> indentmodel = new List<PurchaseIndentModelUpto25Model>();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = UserHelper.GetPendingPurchaseIndentUpto25kRequestList(SessionStaffNumber, Year, 0, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(indentmodel);
        }

        // view break

        [HttpGet]
        public IActionResult ViewPendingPurchaseIndentUpto25k(string PurchaseIndentID)
        {
            List<PurchaseIndentModelUpto25Model> model = new List<PurchaseIndentModelUpto25Model>();

            try
            {
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = UserHelper.GetPurchaseIndentDataUpto25k(SessionStaffNumber, PurchaseIndentID, IPAddress);

                foreach (var indent in model)
                {
                    if (indent.IsAttachmentPresent == true && indent.TSSAttachmentLocation != null)
                    {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:PurhaseIndentUpto25kAttachmentFileLocationForDownload"];
                        indent.AlteredTSSAttachmentLocation = indent.TSSAttachmentLocation.Replace(loglocation, "../PurchaseIndentUploads/");
                    }
                    indent.AlteredMaterialInvoiceDate = Convert.ToDateTime(indent.MaterialInvoiceDate).ToString("dd/MM/yyyy");
                    indent.AlteredMaterialReceivedDate = Convert.ToDateTime(indent.MaterialReceivedDate).ToString("dd/MM/yyyy");
                    indent.AlteredPurchaseVoucherDate = Convert.ToDateTime(indent.PurchaseVoucherDate).ToString("dd/MM/yyyy");
                    indent.AlteredPurchaseVoucherInvoiceDate = Convert.ToDateTime(indent.PurchaseVoucherInvoiceDate).ToString("dd/MM/yyyy");

                    indent.EmployeeList = UserHelper.GetEmployeeListForPurchaseIndent(SessionStaffNumber, IPAddress, indent.IndentorGroupID);

                }
            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                CommonHelper.LogError(error, 0000, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult ViewPendingPurchaseIndentUpto25k(List<PurchaseIndentModelUpto25Model> model, IFormFile ItemBillAttachment, IFormFile NoStockCertificateAttachment, IFormFile POCertificateAttachment)
        {
            try
            {
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

 
                foreach (var indent in model)
                {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:PurhaseIndentUpto25kAttachmentFileLocation"];
                        string folderName = loglocation;
                        folderName = folderName.Replace("Year", DateTime.Now.Year.ToString());
                        folderName = folderName.Replace("Month", DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture));
                        folderName = folderName.Replace("PurchaseIndentID", indent.PurchaseIndentID);
                        //string uploadPath = Server.MapPath(folderName);
                        string webRootPath = _hostingenvironment.ContentRootPath;
                        string newPath = Path.Combine(webRootPath, folderName);

                        if (!Directory.Exists(folderName))
                        {
                            Directory.CreateDirectory(folderName);
                        }

                        if(NoStockCertificateAttachment!=null)
                        {
                        string TemporaryFileName = NoStockCertificateAttachment.FileName;
                        string Extension = TemporaryFileName.Split('.').Last();
                        string FileName = CommonHelper.GeneratePassword(10) + "." + Extension;
                        indent.NoStockCertificateAttachment = Path.Combine(folderName, FileName);
                        string envpath = folderName + "\\" + FileName;

                        using (var stream = new FileStream(indent.NoStockCertificateAttachment, FileMode.Create))
                        {
                            NoStockCertificateAttachment.CopyTo(stream);
                        }

                        }
                        else
                        {
                        indent.NoStockCertificateAttachment = "";
                        }

                        if (ItemBillAttachment != null)
                        {
                        string TemporaryFileName = ItemBillAttachment.FileName;
                        string Extension = TemporaryFileName.Split('.').Last();
                        string FileName = CommonHelper.GeneratePassword(10) + "." + Extension;
                        indent.ItemBillAttachment = Path.Combine(folderName, FileName);
                        string envpath = folderName + "\\" + FileName;

                        using (var stream = new FileStream(indent.ItemBillAttachment, FileMode.Create))
                        {
                            ItemBillAttachment.CopyTo(stream);
                        }

                        }
                        else
                        {
                        indent.ItemBillAttachment = "";
                        }

                        if (POCertificateAttachment != null)
                        {
                        string TemporaryFileName = POCertificateAttachment.FileName;
                        string Extension = TemporaryFileName.Split('.').Last();
                        string FileName = CommonHelper.GeneratePassword(10) + "." + Extension;
                        indent.POCertificateAttachment = Path.Combine(folderName, FileName);
                        string envpath = folderName + "\\" + FileName;

                        using (var stream = new FileStream(indent.POCertificateAttachment, FileMode.Create))
                        {
                            POCertificateAttachment.CopyTo(stream);
                        }

                        }
                        else
                        {
                        indent.POCertificateAttachment = "";
                        }

                    if (indent.MaterialInvoiceDate == DateTime.MinValue)
                    {
                        indent.AlteredMaterialInvoiceDate = Convert.ToString(indent.AlteredMaterialInvoiceDate);
                    }
                    else
                    {
                        indent.AlteredMaterialInvoiceDate = Convert.ToString(indent.MaterialInvoiceDate);
                    }

                    if (indent.MaterialReceivedDate == DateTime.MinValue)
                    {
                        indent.AlteredMaterialReceivedDate = Convert.ToString(indent.AlteredMaterialReceivedDate);
                    }
                    else
                    {
                        indent.AlteredMaterialReceivedDate = Convert.ToString(indent.MaterialReceivedDate);
                    }

                    if (indent.PurchaseVoucherDate == DateTime.MinValue)
                    {
                        indent.AlteredPurchaseVoucherDate = Convert.ToString(indent.AlteredPurchaseVoucherDate);
                    }
                    else
                    {
                        indent.AlteredPurchaseVoucherDate = Convert.ToString(indent.PurchaseVoucherDate);
                    }

                    if (indent.PurchaseVoucherInvoiceDate == DateTime.MinValue)
                    {
                        indent.AlteredPurchaseVoucherInvoiceDate = Convert.ToString(indent.AlteredPurchaseVoucherInvoiceDate);
                    }
                    else
                    {
                        indent.AlteredPurchaseVoucherInvoiceDate = Convert.ToString(indent.PurchaseVoucherInvoiceDate);
                    }

                    bool IsUpdate = UserHelper.UpdatePurchaseIndentUpto25k(indent, BrowserName, IPAddress, SessionStaffNumber);

                    if (IsUpdate)
                    {
                        TempData["SuccessMessage"] = "Update successful";
                        return RedirectToAction("PendingPurchaseIndentUpto25kRequestWithoutFilter", "User");
                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                        return RedirectToAction("PendingPurchaseIndentUpto25kRequestWithoutFilter", "User");
                    }
                }
            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                CommonHelper.LogError(error, 0000, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(model);
        }

        // Purchase Indent Upto 25k end
        // *************************************************************************************************** //


        // *************************************************************************************************** //
        // Purchase Indent More than 25k start

        public IActionResult PurchaseIndentMorethan25k()
        {
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        // view break

        [HttpGet]
        public IActionResult RaisePurchaseIndentMorethan25k()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            PurchaseIndentMorethan25kModel purchaseindent = new PurchaseIndentMorethan25kModel();

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                EmployeeListModel employee = UserHelper.GetUserDataFromStaffNumber(SessionStaffNumber, IPAddress);

                purchaseindent.IndentorStaffNumber = employee.StaffNumber;
                purchaseindent.IndentorName = employee.Name;
                purchaseindent.IndentorDesignation = employee.Designation;
                purchaseindent.IndentorGroupID = employee.GroupID;
                purchaseindent.IndentorGroupName = employee.GroupName;
                purchaseindent.IndentorCenterID = employee.CenterID;
                purchaseindent.IndentorCenterName = employee.CenterName;

                purchaseindent.ReportingOfficerModel = UserHelper.GetEmployeeListForReportingOfficer(SessionStaffNumber, IPAddress, purchaseindent.IndentorGroupID);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(purchaseindent);
        }

        [HttpPost]
        public IActionResult RaisePurchaseIndentMorethan25k(PurchaseIndentMorethan25kModel model, IFormFile AttachmentFile, IFormFile AttachmentAbove15L, IFormFile ProprietaryArticleCertificateEnclosedAttachment, IFormFile CGCApprovalAttachment)
        {
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                // to get the count from database table
                int IndentCount = UserHelper.GetTodaysPurchaseIndentsMorethan25kCount(model.IndentorStaffNumber, IPAddress);

                IndentCount = IndentCount + 1;

                //model.IndentID = "IDR" + DateTime.Now.ToString("ddMMyy") + model.FromCenterID + model.ToCenterID + IndentCount;

                model.PurchaseIndentID = "PRMT25K" + DateTime.Now.ToString("ddMMyy") + IndentCount;

                // code for attachment save start
                var configuration = CommonHelper.GetConfig();

                var loglocation = configuration["FileLocations:PurhaseIndentMorethan25kAttachmentFileLocation"];
                string folderName = loglocation;
                folderName = folderName.Replace("Year", DateTime.Now.Year.ToString());
                folderName = folderName.Replace("Month", DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture));
                folderName = folderName.Replace("PurchaseIndentID", model.PurchaseIndentID);
                //string uploadPath = Server.MapPath(folderName);
                string webRootPath = _hostingenvironment.ContentRootPath;
                string newPath = Path.Combine(webRootPath, folderName);

                if (!Directory.Exists(folderName))
                {
                    Directory.CreateDirectory(folderName);
                }

                if (AttachmentFile != null)
                {
                    string TemporaryFileName = AttachmentFile.FileName;
                    string Extension = TemporaryFileName.Split('.').Last();
                    string FileName = CommonHelper.GeneratePassword(10) + "." + Extension;
                    model.TechnicalSpecificationAttachmentLocation = Path.Combine(folderName, FileName);
                    string envpath = folderName + "\\" + FileName;

                    using (var stream = new FileStream(model.TechnicalSpecificationAttachmentLocation, FileMode.Create))
                    {
                        AttachmentFile.CopyTo(stream);
                    }
                }
                else
                {
                    model.TechnicalSpecificationAttachmentLocation = "";
                }

                if (AttachmentAbove15L != null)
                {
                    string TemporaryFileName = AttachmentAbove15L.FileName;
                    string Extension = TemporaryFileName.Split('.').Last();
                    string FileName = CommonHelper.GeneratePassword(10) + "." + Extension;
                    model.AttachmentAbove15L = Path.Combine(folderName, FileName);
                    string envpath = folderName + "\\" + FileName;

                    using (var stream = new FileStream(model.AttachmentAbove15L, FileMode.Create))
                    {
                        AttachmentAbove15L.CopyTo(stream);
                    }
                }
                else
                {
                    model.AttachmentAbove15L = "";
                }

                if (ProprietaryArticleCertificateEnclosedAttachment != null)
                {
                    string TemporaryFileName = ProprietaryArticleCertificateEnclosedAttachment.FileName;
                    string Extension = TemporaryFileName.Split('.').Last();
                    string FileName = CommonHelper.GeneratePassword(10) + "." + Extension;
                    model.ProprietaryArticleAttachmentLocation = Path.Combine(folderName, FileName);
                    string envpath = folderName + "\\" + FileName;

                    using (var stream = new FileStream(model.ProprietaryArticleAttachmentLocation, FileMode.Create))
                    {
                        ProprietaryArticleCertificateEnclosedAttachment.CopyTo(stream);
                    }
                }
                else
                {
                    model.ProprietaryArticleAttachmentLocation = "";
                }

                if (CGCApprovalAttachment != null)
                {
                    string TemporaryFileName = CGCApprovalAttachment.FileName;
                    string Extension = TemporaryFileName.Split('.').Last();
                    string FileName = CommonHelper.GeneratePassword(10) + "." + Extension;
                    model.CGCApprovalAttachmentLocation = Path.Combine(folderName, FileName);
                    string envpath = folderName + "\\" + FileName;

                    using (var stream = new FileStream(model.CGCApprovalAttachmentLocation, FileMode.Create))
                    {
                        CGCApprovalAttachment.CopyTo(stream);
                    }
                }
                else
                {
                    model.CGCApprovalAttachmentLocation = "";
                }


                bool IsUpdated = UserHelper.RaisePurchaseIndentMorethan25k(model, BrowserName, IPAddress);

                if (IsUpdated)
                {
                    TempData["SuccessMessage"] = "Purhase Indent " + model.PurchaseIndentID + " raised successfully";

                    // send email
                    //string[] EmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                    //string EmailID = EmailAndName[0];
                    //string FirstName = EmailAndName[1];
                    //string Message = @"Dear <b>" + FirstName + ",</b><br/>Your request for Inter Departmental Requisition has" +
                    //                    " been raised successfully and your Indent ID is <b>" + model.IndentID + "</b>.";
                    //string Subject = "Inter Departmental Requisition";
                    //CommonHelper.SendEmail(EmailID, Subject, Message, SessionStaffNumber, IPAddress);

                    return RedirectToAction("RaisePurchaseIndentMorethan25k", "User");
                }
                else
                {
                    TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                    return RedirectToAction("RaisePurchaseIndentMorethan25k", "User");
                }
            }

            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View();
        }

        // Purchase Indent More than 25k end
        // *************************************************************************************************** //

        // view break

        // *************************************************************************************************** //
        // Visitor Pass start

        public IActionResult VisitorPass()
        {
            Console.Write(HttpContext.Session.GetString("Role"));
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        // view break

        [HttpGet]
        public IActionResult RaiseVisitorPass()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            VisitorPassModel visitorpass = new VisitorPassModel();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                // check if logged in user is security supervisor or not
                //if (HttpContext.Session.GetString("Role") != "Security")
                //{
                //    return RedirectToAction("AccessDenied", "User");
                //}

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                EmployeeListModel employee = UserHelper.GetUserDataFromStaffNumber(SessionStaffNumber, IPAddress);

                visitorpass.SenderStaffNumber = employee.StaffNumber;
                visitorpass.SenderFirstName = employee.Name;
                visitorpass.SenderDesignation = employee.Designation;
                visitorpass.SenderGroupName = employee.GroupName;
                visitorpass.SenderCenterName = employee.CenterName;

                visitorpass.IsCarryingLaptop = "No";
                visitorpass.IsCarryingPendrive = "No";
                visitorpass.IsCarryingOther = "No";
                visitorpass.OtherCarryingItem = "Null";

                visitorpass.Centers = UserHelper.GetCenterData(SessionStaffNumber, IPAddress);

                visitorpass.VisitedVisitors = UserHelper.GetPreviouslyVisitedVistors(SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
                ViewBag.ExceptionError = "An error has occured. Please contact system administrator.";
            }
            
            //if(IPAddress == "172.18.21.3")
            if(true)
            {
                return View(visitorpass);
            }
            else
            {
                return RedirectToAction("AccessDenied", "User");
            }

            //Console.WriteLine(ViewBag.ImageCaptured);
            //return View(visitorpass);
        }

        [HttpPost]
        public IActionResult RaiseVisitorPass(VisitorPassModel model, IFormFile webcam)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                if(model.EmployeeCenterID == -1 )
                {
                    VisitorPassModel visitorpass = new VisitorPassModel();

                    EmployeeListModel employee = UserHelper.GetUserDataFromStaffNumber(SessionStaffNumber, IPAddress);

                    visitorpass.SenderStaffNumber = employee.StaffNumber;
                    visitorpass.SenderFirstName = employee.Name;
                    visitorpass.SenderDesignation = employee.Designation;
                    visitorpass.SenderGroupName = employee.GroupName;
                    visitorpass.SenderCenterName = employee.CenterName;

                    visitorpass.Centers = UserHelper.GetCenterData(SessionStaffNumber, IPAddress);

                    visitorpass.VisitedVisitors = UserHelper.GetPreviouslyVisitedVistors(SessionStaffNumber, IPAddress);

                    return View(visitorpass);
                }
                else if(model.EmployeeGroupID == -1)
                {
                    VisitorPassModel visitorpass = new VisitorPassModel();

                    EmployeeListModel employee = UserHelper.GetUserDataFromStaffNumber(SessionStaffNumber, IPAddress);

                    visitorpass.SenderStaffNumber = employee.StaffNumber;
                    visitorpass.SenderFirstName = employee.Name;
                    visitorpass.SenderDesignation = employee.Designation;
                    visitorpass.SenderGroupName = employee.GroupName;
                    visitorpass.SenderCenterName = employee.CenterName;

                    visitorpass.Centers = UserHelper.GetCenterData(SessionStaffNumber, IPAddress);

                    visitorpass.Groups = UserHelper.GetGroupDataFromCenter(model.EmployeeCenterID, SessionStaffNumber, IPAddress);

                    visitorpass.EmployeeListModel = UserHelper.GetEmployeeListForReportingOfficer(SessionStaffNumber, IPAddress, model.EmployeeGroupID);

                    visitorpass.VisitedVisitors = UserHelper.GetPreviouslyVisitedVistors(SessionStaffNumber, IPAddress);

                    return View(visitorpass);
                }
                else if(model.EmployeeStaffNumber == -1)
                {
                    VisitorPassModel visitorpass = new VisitorPassModel();

                    EmployeeListModel employee = UserHelper.GetUserDataFromStaffNumber(SessionStaffNumber, IPAddress);

                    visitorpass.SenderStaffNumber = employee.StaffNumber;
                    visitorpass.SenderFirstName = employee.Name;
                    visitorpass.SenderDesignation = employee.Designation;
                    visitorpass.SenderGroupName = employee.GroupName;
                    visitorpass.SenderCenterName = employee.CenterName;

                    visitorpass.Centers = UserHelper.GetCenterData(SessionStaffNumber, IPAddress);

                    visitorpass.Groups = UserHelper.GetGroupDataFromCenter(model.EmployeeCenterID, SessionStaffNumber, IPAddress);

                    visitorpass.EmployeeListModel = UserHelper.GetEmployeeListForReportingOfficer(SessionStaffNumber, IPAddress, model.EmployeeGroupID);

                    visitorpass.VisitedVisitors = UserHelper.GetPreviouslyVisitedVistors(SessionStaffNumber, IPAddress);

                    return View(visitorpass);
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                // to get the count from database table
                int PassCount = UserHelper.GetTodaysVisitorPassCount(model.SenderStaffNumber, IPAddress);

                PassCount = PassCount + 1;

                model.PassID = "VP" + DateTime.Now.ToString("ddMMyy") + PassCount;

                //To copy all the files in one directory to another directory.
                // Get the files in the source folder. (To recursively iterate through
                // all subfolders under the current directory, see
                // "How to: Iterate Through a Directory Tree.")
                // Note: Check for target path was performed previously
                //       in this code example.

                int i = 0;

                foreach(var visitor in model.VisitorsModel)
                {
                    if (visitor.VisitorName != null && visitor.VisitorDesignation != null &&
                        visitor.VisitorInstitution!= null)
                    {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:VisitorPassFileLocation"];
                        string folderName = loglocation;
                        folderName = folderName.Replace("Year", DateTime.Now.Year.ToString());
                        folderName = folderName.Replace("Month", DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture));
                        folderName = folderName.Replace("SlNo", i.ToString());
                        folderName = folderName.Replace("VisitorPassID", model.PassID);
                        //string uploadPath = Server.MapPath(folderName);
                        string webRootPath = _hostingenvironment.ContentRootPath;
                        string newPath = Path.Combine(webRootPath, folderName);
                        if (!Directory.Exists(folderName))
                        {
                            Directory.CreateDirectory(folderName);

                            string signaturefolder = loglocation;
                            signaturefolder = signaturefolder.Replace("Year", DateTime.Now.Year.ToString());
                            signaturefolder = signaturefolder.Replace("Month", DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture));
                            signaturefolder = signaturefolder.Replace("SlNo", "Signature");
                            signaturefolder = signaturefolder.Replace("VisitorPassID", model.PassID);

                            Directory.CreateDirectory(signaturefolder);

                            // to create folder for shared file
                            var temploglocation = configuration["FileLocations:VisitorPassSignatureLocationTemporary"];
                            string tempfolder = temploglocation;
                            tempfolder = tempfolder.Replace("Year", DateTime.Now.Year.ToString());
                            tempfolder = tempfolder.Replace("Month", DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture));
                            tempfolder = tempfolder.Replace("VisitorPassID", model.PassID);

                            Directory.CreateDirectory(tempfolder);
                        } 

                        string tempfolderName = configuration["FileLocations:VisitorPassTempFileLocation"];
                        tempfolderName = tempfolderName.Replace("Year", DateTime.Now.Year.ToString());
                        tempfolderName = tempfolderName.Replace("Month", DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture));
                        tempfolderName = tempfolderName.Replace("SlNo", i.ToString());
                        //var sourcefilepath = Path.Combine(tempfolderName, "Photos");
                        if (System.IO.Directory.Exists(tempfolderName))
                        {
                            string[] imagefiles = System.IO.Directory.GetFiles(tempfolderName);

                            System.IO.DirectoryInfo di = new DirectoryInfo(tempfolderName);

                            foreach (FileInfo imagefile in di.GetFiles())
                            {
                                if (!(imagefile.Extension == ".png"))
                                {
                                    TempData["ExceptionError"] = "Please capture visitor photo";
                                    return RedirectToAction("RaiseVisitorPass", "User");
                                }
                            }

                            // Copy the files and overwrite destination files if they already exist.
                            foreach (string s in imagefiles)
                            {
                                // Use static Path methods to extract only the file name from the path.
                                string sourcefileName = System.IO.Path.GetFileName(s);
                                string destFile = System.IO.Path.Combine(folderName, sourcefileName);
                                System.IO.File.Copy(s, destFile, true);
                                visitor.VisitorImageLocation = destFile;
                            }

                            foreach (FileInfo imagefile in di.GetFiles())
                            {
                                if (imagefile.Extension == ".png")
                                {
                                    imagefile.Delete();
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Source path does not exist!");
                        }
                    }

                    i++;
                }

                foreach(var visitor in model.VisitorsModel)
                {
                    if(visitor.VisitorEmailID == null)
                    {
                        visitor.VisitorEmailID = "";
                    }

                    if(visitor.Nationality == "Other" && visitor.OtherNationality!="")
                    {
                        visitor.Nationality = visitor.OtherNationality;
                    }
                    else if(visitor.Nationality == "Indian")
                    {
                        visitor.Nationality = "Indian";
                    }
                    else
                    {
                        visitor.Nationality = "Other";
                    }
                }

                if (model.NewRemarks == null)
                {
                    model.NewRemarks = "";
                }

                bool IsInserted = UserHelper.RaiseVisitorPass(model, BrowserName, IPAddress);

                if (IsInserted)
                {
                    TempData["AlertMessage"] = "Visitor Pass is created. Signature from Visitor 1 has to be taken.";
                    return RedirectToAction("ViewPendingVisitorPassRequest", "User", new { VisitorPassID = model.PassID });
                }
                else
                {
                    TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                    return RedirectToAction("RaiseVisitorPass", "User");
                }
            }
            catch (Exception error)
            {
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
                ViewBag.ExceptionError = "An error has occured. Please contact system administrator.";
            }

            return View();
        }

        public IActionResult RaiseVisitorPassResult()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            return View();
        }

        // view break

        [HttpGet]
        public IActionResult MyVisitorPassRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                // check if logged in user is security supervisor or not
                if (HttpContext.Session.GetString("Role") != "Security")
                {
                    return RedirectToAction("AccessDenied", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult MyVisitorPassRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            List<VisitorPassModel> model = new List<VisitorPassModel>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = UserHelper.GetMyVisitorPassRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(model);
        }

        public IActionResult MyVisitorPassRequestWithoutFilter()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            TempData["Search"] = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            List<VisitorPassModel> model = new List<VisitorPassModel>();

            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                // check if logged in user is security supervisor or not
                if (HttpContext.Session.GetString("Role") != "Security")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = UserHelper.GetMyVisitorPassRequestListWithoutFilter(SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(model);
        }

        // view break

        [HttpGet]
        public IActionResult ViewMyVisitorPassRequest(string VisitorPassID)
        {
            List<VisitorPassModel> model = new List<VisitorPassModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = UserHelper.GetVisitorPassData(SessionStaffNumber, VisitorPassID, IPAddress);

                int i = 1;

                foreach (var visitors in model)
                {
                    visitors.VisitorsModel = UserHelper.GetVisitorPassVisitorsData(SessionStaffNumber, VisitorPassID, IPAddress);

                    var configuration = CommonHelper.GetConfig();
                    var loglocation = configuration["FileLocations:VisitorPassFileLocationForDownload"];

                    foreach (var visitor in visitors.VisitorsModel)
                    {
                        if (visitor.VisitorImageLocation != null)
                        {
                            visitor.AlternateVisitorImageLocation = visitor.VisitorImageLocation.Replace(loglocation, "../VisitorImages/");
                        }
                    }

                    if (visitors.SignatureFileLocation != null && i == 1)
                    {
                        visitors.AlternateSignatureFileLocation = visitors.SignatureFileLocation.Replace(loglocation, "../VisitorImages/");
                        ViewBag.SignatureFileLocation = visitors.AlternateSignatureFileLocation;
                    }

                    i++;
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult ViewMyVisitorPassRequest(List<VisitorPassModel> model)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                foreach (var visitorpass in model)
                {
                    bool IsUpdated = UserHelper.UpdateVisitorPassData(SessionStaffNumber, visitorpass, BrowserName, IPAddress);

                    if (IsUpdated)
                    {
                        if (visitorpass.NewStatus == "CANCELLED")
                        {
                            // send email
                            string[] EmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string EmailID = EmailAndName[0];

                            string Message = @"Dear <b> Dear Sir/Madam,</b><br/><br/>Visitor Pass request" +
                                            " with Visitor Pass ID <b>" + visitorpass.PassID + "</b> has been <b>CANCELLED</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Visitor Pass";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(EmailID);

                            Console.WriteLine(visitorpass.NewStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "Visitor pass has been CANCELLED";
                        }

                        return RedirectToAction("MyVisitorPassRequestWithoutFilter", "User");
                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                        return RedirectToAction("MyVisitorPassRequestWithoutFilter", "User");
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
            return RedirectToAction("MyVisitorPassRequestWithoutFilter", "User");
        }

        // view break

        [HttpGet]
        public IActionResult PendingVisitorPassRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult PendingVisitorPassRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            List<VisitorPassModel> model = new List<VisitorPassModel>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = UserHelper.GetPendingVisitorPassRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);
                
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(model);
        }

        public IActionResult PendingVisitorPassRequestWithoutFilter()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            TempData["Search"] = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            List<VisitorPassModel> model = new List<VisitorPassModel>();

            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = UserHelper.GetPendingVisitorPassRequestListWithoutFilter(SessionStaffNumber, IPAddress);

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(model);
        }
        
        // view break

        [HttpGet]
        public IActionResult ViewPendingVisitorPassRequest(string VisitorPassID)
        {
            ViewBag.SuccessMessage = TempData["AlertMessage"];
            TempData["AlertMessage"] = "";

            List<VisitorPassModel> model = new List<VisitorPassModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = UserHelper.GetVisitorPassData(SessionStaffNumber, VisitorPassID, IPAddress);

                int i = 1;

                foreach (var visitors in model)
                {
                    visitors.VisitorsModel = UserHelper.GetVisitorPassVisitorsData(SessionStaffNumber, VisitorPassID, IPAddress);

                    if (visitors.OldStatus == "PENDING FOR SIGNATURE" && i == 1)
                    {
                        visitors.IsSigned = SignatureHelper.CheckIfVisitorPassSigned(visitors.PassID, IPAddress);
                    }
                    else
                    {
                        visitors.IsSigned = "Exists";
                    }

                    var configuration = CommonHelper.GetConfig();
                    var loglocation = configuration["FileLocations:VisitorPassFileLocationForDownload"];

                    foreach (var visitor in visitors.VisitorsModel)
                    {
                        if (visitor.VisitorImageLocation != null)
                        {
                            visitor.AlternateVisitorImageLocation = visitor.VisitorImageLocation.Replace(loglocation, "../VisitorImages/");
                        }
                    }

                    if (visitors.SignatureFileLocation != null && i == 1)
                    {
                        visitors.AlternateSignatureFileLocation = visitors.SignatureFileLocation.Replace(loglocation, "../VisitorImages/");
                        ViewBag.SignatureFileLocation = visitors.AlternateSignatureFileLocation;
                    }

                    i++;
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult ViewPendingVisitorPassRequest(List<VisitorPassModel> model)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                foreach (var visitorpass in model)
                {
                    bool IsUpdated = UserHelper.UpdateVisitorPassData(SessionStaffNumber, visitorpass, BrowserName, IPAddress);

                    if (IsUpdated)
                    {
                        if (visitorpass.NewStatus == "VLI")
                        {
                            // send email
                            string[] EmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string EmailID = EmailAndName[0];

                            string[] EmployeeEmailAndName = CommonHelper.GetEmailAndName(visitorpass.EmployeeStaffNumber, IPAddress);
                            string EmployeeEmailID = EmployeeEmailAndName[0];

                            string Message = @"Dear Sir/Madam,<br/><br/>Visitor Pass request" +
                                            " with Visitor Pass ID <b>" + visitorpass.PassID + "</b> visitors has <b>LEFT THE INSTITUTE</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Visitor Pass";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(EmailID);
                            EmailList.Add(EmployeeEmailID);

                            Console.WriteLine(visitorpass.NewStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "Visitor has LEFT THE INSTITUTE";
                        }
                        else if (visitorpass.NewStatus == "COMPLETED")
                        {
                            // send email
                            string[] EmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string EmailID = EmailAndName[0];

                            string Message = @"Dear Sir/Madam,<br/><br/>Visitor Pass request" +
                                            " with Visitor Pass ID <b>" + visitorpass.PassID + "</b> has been <b>COMPLETED</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Visitor Pass";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(EmailID);

                            Console.WriteLine(visitorpass.NewStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "Visitor pass has been COMPLETED";
                        }
                        else if (visitorpass.NewStatus == "REJECTED")
                        {
                            // send email
                            string[] EmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                            string EmailID = EmailAndName[0];

                            string Message = @"Dear Sir/Madam,<br/><br/>Visitor Pass request" +
                                            " with Visitor Pass ID <b>" + visitorpass.PassID + "</b> has been <b>REJECTED</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                            string Subject = "ebandhu - Visitor Pass";
                            List<string> EmailList = new List<string>();
                            EmailList.Add(EmailID);

                            Console.WriteLine(visitorpass.NewStatus);
                            foreach (var Email in EmailList)
                            {
                                Console.WriteLine(Email);
                            }
                            Console.WriteLine("---");

                            CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                            TempData["SuccessMessage"] = "Visitor pass has been REJECTED";
                        }
                        return RedirectToAction("PendingVisitorPassRequestWithoutFilter", "User");
                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                        return RedirectToAction("PendingVisitorPassRequestWithoutFilter", "User");
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
            return RedirectToAction("PendingVisitorPassRequestWithoutFilter", "User");
        }

        // view break

        [HttpGet]
        public IActionResult OutboxVisitorPassRequests()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult OutboxVisitorPassRequests(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            List<VisitorPassModel> model = new List<VisitorPassModel>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = UserHelper.GetOutBoxVisitorPassRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(model);
        }

        public IActionResult OutboxVisitorPassRequestsWithoutFilter()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            TempData["Search"] = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            List<VisitorPassModel> model = new List<VisitorPassModel>();

            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = UserHelper.GetOutBoxVisitorPassRequestListWithoutFilter(SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(model);
        }

        // view break

        public IActionResult ViewOutBoxVisitorPassRequest(string VisitorPassID)
        {
            ViewBag.SuccessMessage = TempData["AlertMessage"];
            TempData["AlertMessage"] = "";

            List<VisitorPassModel> model = new List<VisitorPassModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = UserHelper.GetVisitorPassData(SessionStaffNumber, VisitorPassID, IPAddress);

                int i = 1;

                foreach (var visitors in model)
                {
                    visitors.VisitorsModel = UserHelper.GetVisitorPassVisitorsData(SessionStaffNumber, VisitorPassID, IPAddress);

                    if (visitors.OldStatus == "PENDING FOR SIGNATURE" && i == 1)
                    {
                        visitors.IsSigned = SignatureHelper.CheckIfVisitorPassSigned(visitors.PassID, IPAddress);
                    }
                    else
                    {
                        visitors.IsSigned = "Exists";
                    }

                    var configuration = CommonHelper.GetConfig();
                    var loglocation = configuration["FileLocations:VisitorPassFileLocationForDownload"];

                    foreach (var visitor in visitors.VisitorsModel)
                    {
                        if (visitor.VisitorImageLocation != null)
                        {
                            visitor.AlternateVisitorImageLocation = visitor.VisitorImageLocation.Replace(loglocation, "../VisitorImages/");
                        }
                    }

                    if (visitors.SignatureFileLocation != null && i == 1)
                    {
                        visitors.AlternateSignatureFileLocation = visitors.SignatureFileLocation.Replace(loglocation, "../VisitorImages/");
                        ViewBag.SignatureFileLocation = visitors.AlternateSignatureFileLocation;
                    }

                    i++;
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(model);
        }

        // view break

        [HttpGet]
        public IActionResult AssignedVisitorPassRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult AssignedVisitorPassRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            List<VisitorPassModel> model = new List<VisitorPassModel>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = UserHelper.GetAssignedVisitorPassRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(model);
        }

        public IActionResult AssignedVisitorPassRequestWithoutFilter()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            TempData["Search"] = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            List<VisitorPassModel> model = new List<VisitorPassModel>();

            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = UserHelper.GetAssignedVisitorPassRequestListWithoutFilter(SessionStaffNumber, IPAddress);

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(model);
        }

        // view break

        public IActionResult ViewAssignedVisitorPassRequest(string VisitorPassID)
        {
            ViewBag.SuccessMessage = TempData["AlertMessage"];
            TempData["AlertMessage"] = "";

            List<VisitorPassModel> model = new List<VisitorPassModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = UserHelper.GetVisitorPassData(SessionStaffNumber, VisitorPassID, IPAddress);

                int i = 1;

                foreach (var visitors in model)
                {
                    visitors.VisitorsModel = UserHelper.GetVisitorPassVisitorsData(SessionStaffNumber, VisitorPassID, IPAddress);

                    if (visitors.OldStatus == "PENDING FOR SIGNATURE" && i == 1)
                    {
                        visitors.IsSigned = SignatureHelper.CheckIfVisitorPassSigned(visitors.PassID, IPAddress);
                    }
                    else
                    {
                        visitors.IsSigned = "Exists";
                    }

                    var configuration = CommonHelper.GetConfig();
                    var loglocation = configuration["FileLocations:VisitorPassFileLocationForDownload"];

                    foreach (var visitor in visitors.VisitorsModel)
                    {
                        if (visitor.VisitorImageLocation != null)
                        {
                            visitor.AlternateVisitorImageLocation = visitor.VisitorImageLocation.Replace(loglocation, "../VisitorImages/");
                        }
                    }

                    if (visitors.SignatureFileLocation != null && i == 1)
                    {
                        visitors.AlternateSignatureFileLocation = visitors.SignatureFileLocation.Replace(loglocation, "../VisitorImages/");
                        ViewBag.SignatureFileLocation = visitors.AlternateSignatureFileLocation;
                    }

                    i++;
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(model);
        }

        // view break

        public IActionResult UpdateVisitorPassSignature(string VisitorPassID)
        {
            List<VisitorPassModel> model = new List<VisitorPassModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                model = UserHelper.GetVisitorPassData(SessionStaffNumber, VisitorPassID, IPAddress);

                var configuration = CommonHelper.GetConfig();
                var loglocation = configuration["FileLocations:VisitorPassSignatureFileLocation"];

                string folderName = loglocation;
                folderName = folderName.Replace("Year", DateTime.Now.Year.ToString());
                folderName = folderName.Replace("Month", DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture));
                folderName = folderName.Replace("VisitorPassID", VisitorPassID);

                if (!Directory.Exists(folderName))
                {
                    Directory.CreateDirectory(folderName);
                }

                string Year = DateTime.Now.Year.ToString();
                string Month = DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture);

                string sourceFile = System.IO.Path.Combine("G:\\Mokshith\\SignatureImages\\" + Year + "\\" + Month + "\\" + VisitorPassID, VisitorPassID + ".png");
                string destFile = System.IO.Path.Combine(folderName, VisitorPassID + ".png");

                System.IO.File.Copy(sourceFile, destFile, true);

                string FileLocation = destFile;

                foreach (var pass in model)
                {
                    pass.SignatureFileLocation = FileLocation;
                    pass.NewRemarks = "Signature image captured by Security";
                    pass.NewStatus = "ISSUED";

                    bool IsUpdated = UserHelper.UpdateVisitorPassData(SessionStaffNumber, pass, BrowserName, IPAddress);

                    if (IsUpdated)
                    {
                        TempData["SuccessMessage"] = "Update successful";
                        return RedirectToAction("PendingVisitorPassRequestWithoutFilter", "User");
                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                        return RedirectToAction("PendingVisitorPassRequestWithoutFilter", "User");
                    }
                }

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return RedirectToAction("PendingVisitorPassRequestWithoutFilter", "User");
        }

        // Visitor Pass end
        // *************************************************************************************************** //

        // view break

       

        // view break

        // *************************************************************************************************** //
        // OverTime start

        public IActionResult OverTime()
        {
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        // view break

        [HttpGet]
        public IActionResult RaiseOverTime()
        {
            OverTimeModel overtimemodel = new OverTimeModel();

            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                EmployeeListModel employee = UserHelper.GetUserDataFromStaffNumber(SessionStaffNumber, IPAddress);

                overtimemodel.StaffNumber = employee.StaffNumber;
                overtimemodel.StaffName = employee.Name;
                overtimemodel.GroupName = employee.GroupName;
                overtimemodel.CenterName = employee.CenterName;
                overtimemodel.GroupID = employee.GroupID;
                overtimemodel.CenterID = employee.CenterID;
                overtimemodel.Designation = employee.Designation;

                overtimemodel.ReportingOfficerListModel = UserHelper.GetEmployeeListForReportingOfficer(SessionStaffNumber, IPAddress, overtimemodel.GroupID);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(overtimemodel);
        }

        [HttpPost]
        public IActionResult RaiseOverTime(OverTimeModel model)
        {
            OverTimeModel overtimemodel = new OverTimeModel();

            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                int otcount = UserHelper.GetTodaysOTCount(SessionStaffNumber, IPAddress);

                otcount = otcount + 1;

                model.OverTimeID = "OT" + DateTime.Now.ToString("ddMMyy") + otcount;

                bool isupdated = UserHelper.RaiseOverTime(model, IPAddress, BrowserName);

                if (isupdated)
                {
                    TempData["SuccessMessage"] = "OverTime with ID " + model.OverTimeID + " Raised Successfully ";
                    return RedirectToAction("RaiseOverTime", "User");
                }
                else
                {
                    TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                    return RedirectToAction("RaiseOverTime", "User");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(overtimemodel);
        }

        // view break

        [HttpGet]
        public IActionResult MyOverTimeRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult MyOverTimeRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<OverTimeModel> overtimemodel = new List<OverTimeModel>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                overtimemodel = UserHelper.GetMyOverTimeRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);

                foreach (var overtime in overtimemodel)
                {
                    overtime.FromTime = Convert.ToDateTime(overtime.AlternateFromTime);
                    overtime.ToTime = Convert.ToDateTime(overtime.AlternateToTime);

                    overtime.AlternateFromTime = Convert.ToDateTime(overtime.AlternateFromTime).ToString("hh:mm:ss tt");
                    overtime.AlternateToTime = Convert.ToDateTime(overtime.AlternateToTime).ToString("hh:mm:ss tt");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(overtimemodel);
        }

        public IActionResult MyOverTimeRequestWithoutFilter()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<OverTimeModel> overtimemodel = new List<OverTimeModel>();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                overtimemodel = UserHelper.GetMyOverTimeRequestListWithoutFilter(SessionStaffNumber, IPAddress);

                foreach (var overtime in overtimemodel)
                {
                    overtime.AlternateFromTime = Convert.ToDateTime(overtime.AlternateFromTime).ToString("hh:mm:ss tt");
                    overtime.AlternateToTime = Convert.ToDateTime(overtime.AlternateToTime).ToString("hh:mm:ss tt");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(overtimemodel);
        }

        // view break

        [HttpGet]
        public IActionResult ViewMyOverTimeRequest(string OverTimeID)
        {
            List<OverTimeModel> model = new List<OverTimeModel>();

            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = UserHelper.GetOverTimeData(SessionStaffNumber, OverTimeID, IPAddress);

                foreach (var overtime in model)
                {
                    overtime.FromTime = Convert.ToDateTime(overtime.AlternateFromTime);
                    overtime.ToTime = Convert.ToDateTime(overtime.AlternateToTime);

                    overtime.AlternateFromTime = Convert.ToDateTime(overtime.AlternateFromTime).ToString("hh:mm:ss tt");
                    overtime.AlternateToTime = Convert.ToDateTime(overtime.AlternateToTime).ToString("hh:mm:ss tt");
                }

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult ViewMyOverTimeRequest(List<OverTimeModel> model)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                foreach (var overtime in model)
                {
                    bool IsUpdated = UserHelper.UpdateOverTimeData(SessionStaffNumber, overtime, BrowserName, IPAddress);
                    if (IsUpdated)
                    {
                        TempData["SuccessMessage"] = "Update successful";
                        return RedirectToAction("MyOverTimeRequestWithoutFilter", "User");
                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                        return RedirectToAction("MyOverTimeRequestWithoutFilter", "User");
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
            return RedirectToAction("MyOverTimeRequestWithoutFilter", "User");
        }

        // view break

        [HttpGet]
        public IActionResult PendingOverTimeRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult PendingOverTimeRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<OverTimeModel> overtimemodel = new List<OverTimeModel>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                overtimemodel = UserHelper.GetPendingOverTimeRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);


                foreach (var overtime in overtimemodel)
                {
                    overtime.FromTime = Convert.ToDateTime(overtime.AlternateFromTime);
                    overtime.ToTime = Convert.ToDateTime(overtime.AlternateToTime);

                    overtime.AlternateFromTime = Convert.ToDateTime(overtime.AlternateFromTime).ToString("hh:mm:ss tt");
                    overtime.AlternateToTime = Convert.ToDateTime(overtime.AlternateToTime).ToString("hh:mm:ss tt");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(overtimemodel);
        }

        // view break

        public IActionResult PendingOverTimeRequestWithoutFilter()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<OverTimeModel> overtimemodel = new List<OverTimeModel>();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                overtimemodel = UserHelper.GetPendingOverTimeRequestList(SessionStaffNumber, Year, 0, Status, IPAddress);

                foreach (var overtime in overtimemodel)
                {
                    overtime.FromTime = Convert.ToDateTime(overtime.AlternateFromTime);
                    overtime.ToTime = Convert.ToDateTime(overtime.AlternateToTime);

                    overtime.AlternateFromTime = Convert.ToDateTime(overtime.AlternateFromTime).ToString("hh:mm:ss tt");
                    overtime.AlternateToTime = Convert.ToDateTime(overtime.AlternateToTime).ToString("hh:mm:ss tt");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(overtimemodel);
        }

        [HttpGet]
        public IActionResult ViewPendingOverTimeRequest(string OverTimeID)
        {
            List<OverTimeModel> overtimemodel = new List<OverTimeModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                overtimemodel = UserHelper.GetOverTimeData(SessionStaffNumber, OverTimeID, IPAddress);

                foreach (var overtime in overtimemodel)
                {
                    overtime.FromTime = Convert.ToDateTime(overtime.AlternateFromTime);
                    overtime.ToTime = Convert.ToDateTime(overtime.AlternateToTime);

                    overtime.AlternateFromTime = Convert.ToDateTime(overtime.AlternateFromTime).ToString("hh:mm:ss tt");
                    overtime.AlternateToTime = Convert.ToDateTime(overtime.AlternateToTime).ToString("hh:mm:ss tt");
                }

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(overtimemodel);
        }

        // view break

        [HttpPost]
        public IActionResult ViewPendingOverTimeRequest(List<OverTimeModel> model)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                foreach (var overtime in model)
                {
                    bool IsUpdated = UserHelper.UpdateOverTimeData(SessionStaffNumber, overtime, BrowserName, IPAddress);
                    if (IsUpdated)
                    {
                        TempData["SuccessMessage"] = "Update successful";
                        return RedirectToAction("PendingOverTimeRequestWithoutFilter", "User");
                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                        return RedirectToAction("PendingOverTimeRequestWithoutFilter", "User");
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
            return RedirectToAction("PendingOverTimeRequestWithoutFilter", "User");
        }

        // OverTime end
        // *************************************************************************************************** //

        // view break

        // *************************************************************************************************** //
        // OverTime CashClaim start

        public IActionResult OverTimeCashClaim()
        {
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                CommonHelper.LogError(error, 0000, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View();
        }
        
        // view break

        public IActionResult ClaimCash()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<OverTimeModel> overtimemodel = new List<OverTimeModel>();
            try
            {

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                overtimemodel = UserHelper.GetPendingOverTimeData(SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(overtimemodel);
        }

        // view break

        public IActionResult InsertClaimCash(string OverTimeID)
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                OverTimeModel model = UserHelper.GetClaimCashRequestData(SessionStaffNumber, IPAddress, OverTimeID);

                int CashClaimCount = UserHelper.GetTodaysCashClaimCount(model.StaffNumber, IPAddress);

                CashClaimCount++;

                model.CashClaimID = "CC" + DateTime.Now.ToString("ddMMyy") + CashClaimCount;

                bool IsUpdated = UserHelper.InsertCashClaimData(SessionStaffNumber, model, BrowserName, IPAddress);


                if (IsUpdated)
                {
                    TempData["SuccessMessage"] = "Cash Claim with ID " + model.CashClaimID + " applied successfully";
                    return RedirectToAction("ClaimCash", "User");
                }
                else
                {
                    TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                    return RedirectToAction("ClaimCash", "User");
                }

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View();
        }

        // view break

        [HttpGet]
        public IActionResult MyCashClaimRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult MyCashClaimRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<OverTimeModel> cashclaimmodel = new List<OverTimeModel>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                cashclaimmodel = UserHelper.GetMyClaimCashRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(cashclaimmodel);
        }

        public IActionResult MyCashClaimRequestWithoutFilter()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<OverTimeModel> cashclaimmodel = new List<OverTimeModel>();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                cashclaimmodel = UserHelper.GetMyClaimCashRequestListWithoutFilter(SessionStaffNumber, IPAddress);

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(cashclaimmodel);
        }

        // view break

        [HttpGet]
        public IActionResult ViewMyCashClaimRequest(string CashClaimID)
        {
            List<OverTimeModel> cashclaim = new List<OverTimeModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                cashclaim = UserHelper.GetCashClaimData(SessionStaffNumber, CashClaimID, IPAddress);

                foreach (var overtime in cashclaim)
                {
                    overtime.FromTime = Convert.ToDateTime(overtime.AlternateFromTime);
                    overtime.ToTime = Convert.ToDateTime(overtime.AlternateToTime);

                    overtime.AlternateFromTime = Convert.ToDateTime(overtime.AlternateFromTime).ToString("hh:mm:ss tt");
                    overtime.AlternateToTime = Convert.ToDateTime(overtime.AlternateToTime).ToString("hh:mm:ss tt");
                }

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(cashclaim);
        }

        [HttpPost]
        public IActionResult ViewMyCashClaimRequest(List<OverTimeModel> model)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                foreach (var overtime in model)
                {
                    bool IsUpdated = UserHelper.UpdateCashClaimData(SessionStaffNumber, overtime, BrowserName, IPAddress);
                    if (IsUpdated)
                    {
                        TempData["SuccessMessage"] = "Update successful";
                        return RedirectToAction("MyCashClaimRequestWithoutFilter", "User");
                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                        return RedirectToAction("MyCashClaimRequestWithoutFilter", "User");
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
            return RedirectToAction("MyCashClaimRequestWithoutFilter", "User");
        }

        // view break

        [HttpGet]
        public IActionResult PendingCashClaimRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult PendingCashClaimRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<OverTimeModel> cashclaimmodel = new List<OverTimeModel>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                cashclaimmodel = UserHelper.GetMyClaimCashRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(cashclaimmodel);
        }

        public IActionResult PendingCashClaimRequestWithoutFilter()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<OverTimeModel> cashclaimmodel = new List<OverTimeModel>();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                cashclaimmodel = UserHelper.GetPendingClaimCashRequestList(SessionStaffNumber, Year, 0, Status, IPAddress);

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(cashclaimmodel);
        }

        // view break

        [HttpGet]
        public IActionResult ViewPendingCashClaimRequest(string CashClaimID)
        {
            List<OverTimeModel> cashclaim = new List<OverTimeModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                cashclaim = UserHelper.GetCashClaimData(SessionStaffNumber, CashClaimID, IPAddress);

                foreach (var overtime in cashclaim)
                {
                    overtime.FromTime = Convert.ToDateTime(overtime.AlternateFromTime);
                    overtime.ToTime = Convert.ToDateTime(overtime.AlternateToTime);

                    overtime.AlternateFromTime = Convert.ToDateTime(overtime.AlternateFromTime).ToString("hh:mm:ss tt");
                    overtime.AlternateToTime = Convert.ToDateTime(overtime.AlternateToTime).ToString("hh:mm:ss tt");
                }

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(cashclaim);
        }

        [HttpPost]
        public IActionResult ViewPendingCashClaimRequest(List<OverTimeModel> model)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                foreach (var overtime in model)
                {
                    bool IsUpdated = UserHelper.UpdateCashClaimData(SessionStaffNumber, overtime, BrowserName, IPAddress);
                    if (IsUpdated)
                    {
                        TempData["SuccessMessage"] = "Update successful";
                        return RedirectToAction("PendingCashClaimRequestWithoutFilter", "User");
                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                        return RedirectToAction("PendingCashClaimRequestWithoutFilter", "User");
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
            return RedirectToAction("PendingCashClaimRequestWithoutFilter", "User");
        }

        // OverTime CashClaim end
        // *************************************************************************************************** //

        // view break

        // *************************************************************************************************** //
        // Contingent Bill start

        public IActionResult ContigentBill()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            try
            {

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View();
        }

        // view break

        [HttpGet]
        public IActionResult RaiseContigentBill()
        {
            ContingentBillModel model = new ContingentBillModel();

            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                EmployeeListModel employee = UserHelper.GetUserDataFromStaffNumber(SessionStaffNumber, IPAddress);

                model.StaffNumber = employee.StaffNumber;
                model.StaffName = employee.Name;
                model.GroupName = employee.GroupName;
                model.CenterName = employee.CenterName;
                model.GroupID = employee.GroupID;
                model.CenterID = employee.CenterID;
                model.Designation = employee.Designation;

                model.ReportingOfficerListModel = UserHelper.GetEmployeeListForReportingOfficer(SessionStaffNumber, IPAddress, model.GroupID);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult RaiseContigentBill(ContingentBillModel model)
        {
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                //if(model.NetPaymentRs == null)
                //{
                //    model.NetPaymentRs = "";
                //}
                if(model.PayRupees == null)
                {
                    model.PayRupees = "";
                }
                if(model.AccountProjectName == null)
                {
                    model.AccountProjectName = "";
                }
                if(model.BudgetHead == null)
                {
                    model.BudgetHead = "";
                }
                if(model.AdvanceDrawn == null)
                {
                    model.AdvanceDrawn = "";
                }
                if(model.AmountSpent == null)
                {
                    model.AmountSpent = "";
                }
                if(model.NetAmountRefundable == null)
                {
                    model.NetAmountRefundable = "";
                }
                if(true)
                {
                    model.RTGSOnDated = DateTime.Now.ToString();
                }
                if (model.AmountingRs == null)
                {
                    model.AmountingRs = "";
                }

                int count = UserHelper.GetTodaysContingentBillCount(SessionStaffNumber, IPAddress);

                count++;

                model.ContingentBillID = "CGT" + DateTime.Now.ToString("ddMMyy") + count;

                bool isupdated = UserHelper.RaiseContingentBill(model, IPAddress, BrowserName);
                //bool isupdated = true;

                if (isupdated)
                {
                    TempData["SuccessMessage"] = "Contingent Bill with ID " + model.ContingentBillID + " Raised Successfully ";
                    return RedirectToAction("RaiseContigentBill", "User");
                }
                else
                {
                    TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                    return RedirectToAction("RaiseContigentBill", "User");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(model);
        }

        // view break

        [HttpGet]
        public IActionResult MyContingentBillRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult MyContingentBillRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<ContingentBillModel> model = new List<ContingentBillModel>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = UserHelper.GetMyContingentBillRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(model);
        }

        public IActionResult MyContingentBillRequestWithoutFilter()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<ContingentBillModel> model = new List<ContingentBillModel>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = UserHelper.GetMyContingentBillRequestListWithoutFilter(SessionStaffNumber, IPAddress);

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(model);
        }

        // view break

        [HttpGet]
        public IActionResult PendingContingentBillRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }
                ViewBag.Search = false;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View();
        }

        [HttpPost]
        public IActionResult PendingContingentBillRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<ContingentBillModel> model = new List<ContingentBillModel>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = UserHelper.GetPendingContingentBillRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(model);
        }

        public IActionResult PendingContingentBillRequestWithoutFilter()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<ContingentBillModel> model = new List<ContingentBillModel>();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = UserHelper.GetPendingContingentBillRequestListWithoutFilter(SessionStaffNumber, IPAddress);

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(model);
        }

        // view break

        [HttpGet]
        public IActionResult ViewMyContingentBillRequest(string ContingentBillID)
        {
            List<ContingentBillModel> model = new List<ContingentBillModel>();

            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = UserHelper.GetContingentBillData(SessionStaffNumber, ContingentBillID, IPAddress);

                foreach (var bill in model)
                {
                    bill.Items = UserHelper.GetContingentBillItemsData(SessionStaffNumber, ContingentBillID, IPAddress);
                }

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult ViewMyContingentBillRequest(List<ContingentBillModel> model)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                foreach (var CGTbill in model)
                {
                    if (CGTbill.PayRupees == null)
                    {
                        CGTbill.PayRupees = "";
                    }
                    if (CGTbill.AccountProjectName == null)
                    {
                        CGTbill.AccountProjectName = "";
                    }
                    if (CGTbill.BudgetHead == null)
                    {
                        CGTbill.BudgetHead = "";
                    }
                    if (CGTbill.AdvanceDrawn == null)
                    {
                        CGTbill.AdvanceDrawn = "";
                    }
                    if (CGTbill.AmountSpent == null)
                    {
                        CGTbill.AmountSpent = "";
                    }
                    if (CGTbill.NetAmountRefundable == null)
                    {
                        CGTbill.NetAmountRefundable = "";
                    }
                    if (true)
                    {
                        CGTbill.RTGSOnDated = DateTime.Now.ToString();
                    }
                    if (CGTbill.AmountingRs == null)
                    {
                        CGTbill.AmountingRs = "";
                    }

                    bool IsUpdated = UserHelper.UpdateContingentBillData(SessionStaffNumber, CGTbill, BrowserName, IPAddress);
                    if (IsUpdated)
                    {
                        TempData["SuccessMessage"] = "Update successful";
                        return RedirectToAction("MyContingentBillRequestWithoutFilter", "User");
                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                        return RedirectToAction("MyContingentBillRequestWithoutFilter", "User");
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
            return RedirectToAction("MyContingentBillRequestWithoutFilter", "User");
        }

        // view break

        [HttpGet]
        public IActionResult ViewPendingContingentBillRequest(string ContingentBillID)
        {
            List<ContingentBillModel> model = new List<ContingentBillModel>();

            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = UserHelper.GetContingentBillData(SessionStaffNumber, ContingentBillID, IPAddress);

                foreach (var bill in model)
                {
                    bill.Items = UserHelper.GetContingentBillItemsData(SessionStaffNumber, ContingentBillID, IPAddress);
                }

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult ViewPendingContingentBillRequest(List<ContingentBillModel> model)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                foreach (var CGTbill in model)
                {
                    if (CGTbill.PayRupees == null)
                    {
                        CGTbill.PayRupees = "";
                    }
                    if (CGTbill.AccountProjectName == null)
                    {
                        CGTbill.AccountProjectName = "";
                    }
                    if (CGTbill.BudgetHead == null)
                    {
                        CGTbill.BudgetHead = "";
                    }
                    if (CGTbill.AdvanceDrawn == null)
                    {
                        CGTbill.AdvanceDrawn = "";
                    }
                    if (CGTbill.AmountSpent == null)
                    {
                        CGTbill.AmountSpent = "";
                    }
                    if (CGTbill.NetAmountRefundable == null)
                    {
                        CGTbill.NetAmountRefundable = "";
                    }
                    if (true)
                    {
                        CGTbill.RTGSOnDated = DateTime.Now.ToString();
                    }
                    if (CGTbill.AmountingRs == null)
                    {
                        CGTbill.AmountingRs = "";
                    }

                    bool IsUpdated = UserHelper.UpdateContingentBillData(SessionStaffNumber, CGTbill, BrowserName, IPAddress);
                    if (IsUpdated)
                    {
                        TempData["SuccessMessage"] = "Update successful";
                        return RedirectToAction("PendingContingentBillRequestWithoutFilter", "User");
                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                        return RedirectToAction("PendingContingentBillRequestWithoutFilter", "User");
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
            return RedirectToAction("PendingContingentBillRequestWithoutFilter", "User");
        }

        // view break

        [HttpGet]
        public IActionResult RaiseTempIndent()
        {
            TempModel tempmodel = new TempModel();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                EmployeeListModel employee = new EmployeeListModel();
                employee = UserHelper.GetUserDataFromStaffNumber(SessionStaffNumber, IPAddress);

                tempmodel.Name = Convert.ToString(employee.StaffNumber);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
                ViewBag.ExceptionError = "An error has occured. Please contact System Administrator.";
            }

            return View();
        }

        [HttpPost]
        public IActionResult RaiseTempIndent(IndentModel model, IFormFile Attachment)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                if (model.ToCenterID == -1)
                {
                    EmployeeListModel employee = new EmployeeListModel();
                    employee = UserHelper.GetUserDataFromStaffNumber(SessionStaffNumber, IPAddress);

                    model.SenderStaffNumber = employee.StaffNumber;
                    model.SenderFirstName = employee.Name;
                    model.FromGroupID = employee.GroupID;
                    model.FromGroupName = employee.GroupName;
                    model.FromCenterID = employee.CenterID;
                    model.FromCenterName = employee.CenterName;
                    model.NewTargetDate = DateTime.Today;
                    model.Centers = UserHelper.GetCenterData(SessionStaffNumber, IPAddress);
                    model.ReportingOfficerListModel = UserHelper.GetEmployeeListForReportingOfficer(SessionStaffNumber, IPAddress, model.FromGroupID);
                    ViewBag.CenterError = "Please select center";
                    ViewBag.IsCenterSelected = false;
                    return View(model);
                }
                else if (model.ToGroupID == -1)
                {
                    int CenterID = model.ToCenterID;
                    EmployeeListModel employee = new EmployeeListModel();
                    employee = UserHelper.GetUserDataFromStaffNumber(SessionStaffNumber, IPAddress);

                    model.SenderStaffNumber = employee.StaffNumber;
                    model.SenderFirstName = employee.Name;
                    model.FromGroupID = employee.GroupID;
                    model.FromGroupName = employee.GroupName;
                    model.FromCenterID = employee.CenterID;
                    model.FromCenterName = employee.CenterName;
                    model.NewTargetDate = DateTime.Today;
                    model.Centers = UserHelper.GetCenterData(SessionStaffNumber, IPAddress);
                    model.Groups = UserHelper.GetGroupDataFromCenter(CenterID, SessionStaffNumber, IPAddress);
                    model.ReportingOfficerListModel = UserHelper.GetEmployeeListForReportingOfficer(SessionStaffNumber, IPAddress, model.FromGroupID);

                    ViewBag.IsCenterSelected = true;

                    return View(model);
                }
                else
                {
                    // to get browser name
                    var userAgent = HttpContext.Request.Headers["User-Agent"];
                    var uaParser = Parser.GetDefault();
                    ClientInfo c = uaParser.Parse(userAgent);
                    string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                    // to get IPAddress
                    string IpAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                    // to get the count from database table
                    int IndentCount = UserHelper.GetTodaysIndentsCount(model.SenderStaffNumber, IpAddress);

                    IndentCount = IndentCount + 1;

                    //model.IndentID = "IDR" + DateTime.Now.ToString("ddMMyy") + model.FromCenterID + model.ToCenterID + IndentCount;

                    model.IndentID = "IDR" + DateTime.Now.ToString("ddMMyy") + IndentCount;

                    if (Attachment == null)
                    {
                        model.IsAttachementPresent = false;
                        model.AttachementLocation = "";
                    }
                    else
                    {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:IndentFileLocation"];
                        string folderName = loglocation;
                        folderName = folderName.Replace("Year", DateTime.Now.Year.ToString());
                        folderName = folderName.Replace("Month", DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture));
                        folderName = folderName.Replace("IndentID", model.IndentID);
                        //string uploadPath = Server.MapPath(folderName);
                        string webRootPath = _hostingenvironment.ContentRootPath;
                        string newPath = Path.Combine(webRootPath, folderName);
                        if (!Directory.Exists(folderName))
                        {
                            Directory.CreateDirectory(folderName);
                        }
                        string TemporaryFileName = Attachment.FileName;
                        string Extension = TemporaryFileName.Split('.').Last();
                        string FileName = CommonHelper.GeneratePassword(10) + "." + Extension;
                        model.AttachementLocation = Path.Combine(folderName, FileName);
                        string envpath = folderName + "\\" + FileName;

                        using (var stream = new FileStream(model.AttachementLocation, FileMode.Create))
                        {
                            Attachment.CopyTo(stream);
                        }

                        model.IsAttachementPresent = true;
                    }

                    bool IsInserted = UserHelper.RaiseIndent(model, BrowserName, IPAddress);

                    if (IsInserted)
                    {
                        TempData["SuccessMessage"] = "Indent " + model.IndentID + " raised successfully";

                        // indentor
                        string[] IndentorEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber, IPAddress);
                        string IndentorEmailID = IndentorEmailAndName[0];
                        // reporting officer
                        string[] ReportingOfficerEmailAndName = CommonHelper.GetEmailAndName(model.ReportingOfficer, IPAddress);
                        string ReportingOfficerEmailID = ReportingOfficerEmailAndName[0];

                        string Message = @"Dear Sir/Madam,<br/><br/>Inter Departmental Requisition has" +
                                            " been raised successfully and Indent ID is <b>" + model.IndentID + "</b>." +
                                            "<br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                        string Subject = "ebandhu - Inter Departmental Requisition";
                        List<string> EmailList = new List<string>();
                        EmailList.Add(IndentorEmailID);
                        EmailList.Add(ReportingOfficerEmailID);

                        Console.WriteLine("RAISED");
                        foreach (var Email in EmailList)
                        {
                            Console.WriteLine(Email);
                        }
                        Console.WriteLine("---");

                        CommonHelper.SendEmail(EmailList, Subject, Message, SessionStaffNumber, IPAddress);

                        return RedirectToAction("RaiseIndentResult", "User");
                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                        return RedirectToAction("RaiseIndentResult", "User");
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
                ViewBag.ExceptionError = "An error has occured. Please contact system administrator.";
            }

            return View(model);
        }
        // contigentBIll.......
        public IActionResult PrepareContigentBill()
        {
            //ViewBag.SuccessMessage = TempData["SuccessMessage"];
            //TempData["SuccessMessage"] = "";

            //ViewBag.ExceptionError = TempData["ExceptionError"];
            //TempData["ExceptionError"] = "";

            //ViewBag.Search = true;

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            try
            {

                if (HttpContext.Session.GetInt32("StaffNumber") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetInt32("StaffNumber") == 0)
                {
                    return RedirectToAction("Logout", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View();
        }


    }
}

