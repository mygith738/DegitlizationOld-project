using Digitalization.Models.AuthorityModel;
using Digitalization.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UAParser;
 

namespace Digitalization.Controllers
{
    public class AuthorityController : Controller
    {

        private readonly IWebHostEnvironment _hostingenvironment;

        public AuthorityController(IWebHostEnvironment hostingenvironment)
        {
            _hostingenvironment = hostingenvironment;
        }
        // index start
        public IActionResult Index()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                // check if user logged in or not
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
                // check if logged in user is authority or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
                {
                    return RedirectToAction("AccessDenied", "User");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "Something went wrong";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return RedirectToAction("Index", "User");
        }
        // index end

        // view break

        // Employee data related start
        // *************************************************************************************************//

        // Employee Request Start

        [HttpGet]
        public IActionResult EmployeeRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            try
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
                TempData["SuccessMessage"] = "";

                // error message
                ViewBag.ExceptionError = TempData["ExceptionError"];
                TempData["ExceptionError"] = "";

                ViewBag.IsFiltered = false;

                // check if user logged in or not
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
                // check if logged in user is authority or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "Something went wrong";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View();
        }

        [HttpPost]
        public IActionResult EmployeeRequest(int Year, int Month, char Status)
        {
            // Profile Model
            List<EmployeeModel> employeemodel = new List<EmployeeModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
                TempData["SuccessMessage"] = "";

                // error message
                ViewBag.ExceptionError = TempData["ExceptionError"];
                TempData["ExceptionError"] = "";

                ViewBag.IsFiltered = true;

                // check if user logged in or not
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
                // check if logged in user is authority or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                employeemodel = AuthorityHelper.GetEmployeeDataList(SessionStaffNumber, Year, Month, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "Something went wrong";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(employeemodel);
        }

        public IActionResult EmployeeRequestWithoutFilter()
        {
            // Profile Model
            List<EmployeeModel> employeemodel = new List<EmployeeModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
                TempData["SuccessMessage"] = "";

                // error message
                ViewBag.ExceptionError = TempData["ExceptionError"];
                TempData["ExceptionError"] = "";

                ViewBag.IsFiltered = true;

                // check if user logged in or not
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
                // check if logged in user is authority or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                employeemodel = AuthorityHelper.GetEmployeeDataList(SessionStaffNumber, 2022, 0, 'P', IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "Something went wrong";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(employeemodel);
        }
        // Employee Request end

        // view break

        // Employee Request data Start

        [HttpGet]
        public IActionResult EmployeeRequestData(int StaffNumber)
        {
            var employeemodel = new List<EmployeeModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                ViewBag.StaffNumber = StaffNumber;
                // check if user logged in or not
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                var employee = new EmployeeModel();
                employee = AuthorityHelper.GetEmployeeData(StaffNumber, SessionStaffNumber, IPAddress);
                employee.QualificationModel = AuthorityHelper.GetQualificationData(StaffNumber, SessionStaffNumber, IPAddress);
                employee.EmergencyContactModel = AuthorityHelper.GetEmergencyContactData(StaffNumber, SessionStaffNumber, IPAddress);
                employee.FamilyDeclarationModel = AuthorityHelper.GetFamilyDeclarationData(StaffNumber, SessionStaffNumber, IPAddress);
                employee.MedicalDependentsModel = AuthorityHelper.GetMedicalDependentsData(StaffNumber, SessionStaffNumber, IPAddress);
                employee.GratuityModel = AuthorityHelper.GetGratuityData(StaffNumber, SessionStaffNumber, IPAddress);
                employeemodel.Add(employee);

                var configuration = CommonHelper.GetConfig();
                var loglocation = configuration["FileLocations:EmployeeImageLocationForDownload"];
                string ImagePath = employee.UserImagePath;

                if(ImagePath != null)
                {
                    ViewBag.UserImagePath = ImagePath.Replace(loglocation, "../StaffPhotoUploads/");
                }
                else
                {
                    ViewBag.UserImagePath = ViewBag.UserImagePath;
                }

                ViewBag.IsPhotoAvailable = true;
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "Something went wrong";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(employeemodel);
        }

        [HttpPost]
        public IActionResult EmployeeRequestData(List<EmployeeModel> model)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                // check if user logged in or not
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                bool UserExists = false;

                foreach (var employee in model)
                {
                    if (employee.EmployeeCategory == "Contract Employee")
                    {
                        employee.OfficialEmailId = "";
                        employee.MaritalStatus = "";
                        employee.EmployeeGroup = "";
                        employee.EntryLevel = "";
                        employee.CurrentBasicPay = 0;
                        employee.SpouseInGovernment = "";
                    }
                    if (employee.PAN == null)
                    {
                        employee.PAN = "";
                    }
                    if (employee.CasteCategory == null)
                    {
                        employee.CasteCategory = "";
                    }
                    if (employee.AadharNumber == null)
                    {
                        employee.AadharNumber = "";
                    }
                    if (employee.PersonalEmailId == null)
                    {
                        employee.PersonalEmailId = "";
                    }
                    if (employee.AccountNumber == "0")
                    {
                        employee.AccountNumber = "";
                    }
                    if (employee.Remarks == null)
                    {
                        employee.Remarks = " ";
                    }

                    UserExists = CommonHelper.CheckUserExists(employee.StaffNumber, IPAddress);
                }

                bool IsUpdated = AuthorityHelper.UpdateEmployeeData(model, BrowserName, SessionStaffNumber, IPAddress);

                if (IsUpdated)
                {
                    foreach (var employee in model)
                    {
                        if ((employee.Status == 'A') && (UserExists == false))
                        {
                            //system generated password
                            string SystemPassword = CommonHelper.GeneratePassword(8);

                            //get Hashed Password
                            string[] Hash = CommonHelper.ConvertTextToHash(SystemPassword);
                            string HashedPassword = Hash[0];
                            string SALT = Hash[1];

                            bool IsInserted = AuthorityHelper.StoreSystemGeneratedPassword(HashedPassword, SALT, SessionStaffNumber, employee.StaffNumber, IPAddress, BrowserName);

                            if (IsInserted)
                            {
                                string Name=employee.FirstName + " " + employee.LastName;
                                string Message = "Dear " + Name + ",<br/>Welcome to ebandhu platform. This is a system generated email and has the temporary password to access the platform for the first time. Kindly change your password after login.<br/> Password must have atleast 8 characters, an uppercase and a lower case alphabet, a numeric character and a special character.<br/>Your temporary password is <b> " + SystemPassword + "</b> <br/> Please use the following URL to login to e-bandhu http://172.18.91.93:8085/ebandhu/User <br/> The portal is only available as intranet. <br/><br/> Thanks and Regards, <br/>CMTI Digitalization Team";
                                string Subject = "Registration on e-bandhu platform";

                                //send email
                                List<string> EmailList = new List<string>();
                                if(employee.EmployeeCategory == "Contract Employee")
                                {
                                    EmailList.Add(employee.PersonalEmailId);
                                }
                                else
                                {
                                    EmailList.Add(employee.OfficialEmailId);
                                }

                                bool EmailResult=CommonHelper.SendEmail(EmailList, Subject, Message, 5485, IPAddress);

                                if (EmailResult)
                                {
                                    // redirect to Registration page with Success Message
                                    TempData["SuccessMessage"] = "Update successful";
                                    return RedirectToAction("EmployeeRequestWithoutFilter", "Authority");
                                }
                                else
                                {
                                    TempData["ExceptionError"] = "An error has occured. Please contact system administrator";
                                    return RedirectToAction("EmployeeRequestWithoutFilter", "Authority");
                                }
                                //TempData["SuccessMessage"] = "Update Successful";
                                //return RedirectToAction("EmployeeRequest", "Authority");
                            }
                            else
                            {
                                TempData["SuccessMessage"] = "Update successful";
                                return RedirectToAction("EmployeeRequestWithoutFilter", "Authority");
                            }
                        }
                    }

                    //TempData["SuccessMessage"] = "Update successful";
                    //return RedirectToAction("EmployeeRequestWithoutFilter", "Authority");
                }
                else
                {
                    TempData["ExceptionError"] = "An error has occured. Please contact system administrator";
                    return RedirectToAction("EmployeeRequestWithoutFilter", "Authority");
                }

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occured. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return RedirectToAction("EmployeeRequestWithoutFilter", "Authority");
        }

        // Employee request data end

        // employee search start

        [HttpGet]
        public IActionResult EmployeeSearch()
        {
            SearchModel searchmodel = new SearchModel();
            try
            {
                // check if user logged in or not
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                // get staffnumber from session
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                searchmodel.Groups = AuthorityHelper.GetGroupData(SessionStaffNumber, IPAddress);
                searchmodel.Centers = AuthorityHelper.GetCenterData(SessionStaffNumber, IPAddress);
                // say that center is not selected
                ViewBag.IsCenterSelected = false;
            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
                searchmodel.Groups = AuthorityHelper.GetGroupData(SessionStaffNumber, IPAddress);
                searchmodel.Centers = AuthorityHelper.GetCenterData(SessionStaffNumber, IPAddress);
            }
            return View(searchmodel);
        }

        [HttpPost]
        public IActionResult EmployeeSearch(SearchModel model)
        {
            SearchModel searchmodel = new SearchModel();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                // check if user logged in or not
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                // get staffnumber from session
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // check which data is entered
                if (model.StaffNumber != 0)
                {
                    // redirect to search if Staff Number is not 0
                    return RedirectToAction("EmployeeSearchData", "Authority", model);
                }
                else if (model.CenterID == -1)
                {
                    // if center is not selected show error that center is not selected
                    ViewBag.CenterError = "Please select center";
                    ViewBag.IsCenterSelected = false;
                }
                else if (model.CenterID != -1 && model.GroupID == -1)
                {
                    // if center is selected and group is not selected return to view
                    searchmodel.CenterID = model.CenterID;
                    searchmodel.Centers = AuthorityHelper.GetCenterData(SessionStaffNumber, IPAddress);
                    searchmodel.Groups = AuthorityHelper.GetGroupDataFromCenter(model.CenterID, SessionStaffNumber, IPAddress);
                    ViewBag.IsCenterSelected = true;
                    ViewBag.GroupError = "Please select group";
                    return View(searchmodel);
                }
                else
                {
                    // if eveything is fine move to search
                    return RedirectToAction("EmployeeSearchData", "Authority", model);
                }

                searchmodel.Groups = AuthorityHelper.GetGroupData(SessionStaffNumber, IPAddress);
                searchmodel.Centers = AuthorityHelper.GetCenterData(SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
                searchmodel.Groups = AuthorityHelper.GetGroupData(SessionStaffNumber, IPAddress);
                searchmodel.Centers = AuthorityHelper.GetCenterData(SessionStaffNumber, IPAddress);
                ViewBag.IsCenterSelected = false;
                return View(searchmodel);
            }

            return View(searchmodel);
        }

        // employee search end

        // view break

        // search data start

        public IActionResult EmployeeSearchData(SearchModel model)
        {
            List<EmployeeModel> employeemodel = new List<EmployeeModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                // check if user logged in or not
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
                {
                    return RedirectToAction("AccessDenied", "User");
                }
                // get staffnumber from session
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                if (model.StaffNumber != 0 && model.CenterID == -1 && model.GroupID == -1)
                {
                    // if Staffnumber is entered search based on staff number
                    employeemodel = AuthorityHelper.GetEmployeeDataListFromStaffNumber(model.StaffNumber, SessionStaffNumber, IPAddress);
                }
                else if (model.StaffNumber == 0 && model.CenterID != -1 && model.GroupID != -1)
                {
                    // if center and group is selected search based on center and group
                    employeemodel = AuthorityHelper.GetEmployeeDataListFromGroupCenter(model.GroupID, model.CenterID, SessionStaffNumber, IPAddress);
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(employeemodel);
        }

        // search data end

        // view break

        // search details start

        public IActionResult EmployeeSearchDetails(int StaffNumber)
        {
            var employeemodel = new List<EmployeeModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                ViewBag.StaffNumber = StaffNumber;
                // check if user logged in or not
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                var employee = new EmployeeModel();
                employee = AuthorityHelper.GetEmployeeDataFromSearch(StaffNumber, SessionStaffNumber, IPAddress);
                // check IsPhotoAvailable field is true or false and create a viewbag to toggle to show photo or not
                if (employee.IsPhotoAvailable)
                {
                    var configuration = CommonHelper.GetConfig();
                    var loglocation = configuration["FileLocations:EmployeeImageLocationForDownload"];

                    ViewBag.UserImagePath = employee.UserImagePath.Replace(loglocation, "../StaffPhotoUploads/");

                    ViewBag.IsPhotoAvailable = true;
                }
                else
                {
                    ViewBag.IsPhotoAvailable = false;
                }
                // Group, Center and Designation data is not necessary bcz search details or not editable

                //employee.Groups = AdminHelper.GetGroupData(SessionStaffNumber);
                //employee.Centers = AdminHelper.GetCenterData(SessionStaffNumber);
                //employee.Designations = AdminHelper.GetDesignationData(SessionStaffNumber);
                employee.QualificationModel = AuthorityHelper.GetQualificationDataForSearch(StaffNumber, SessionStaffNumber, IPAddress);
                employee.EmergencyContactModel = AuthorityHelper.GetEmergencyContactDataForSearch(StaffNumber, SessionStaffNumber, IPAddress);
                employee.FamilyDeclarationModel = AuthorityHelper.GetFamilyDeclarationDataForSearch(StaffNumber, SessionStaffNumber, IPAddress);
                employee.MedicalDependentsModel = AuthorityHelper.GetMedicalDependentsDataForSearch(StaffNumber, SessionStaffNumber, IPAddress);
                employee.GratuityModel = AuthorityHelper.GetGratuityDataForSearch(StaffNumber, SessionStaffNumber, IPAddress);
                employeemodel.Add(employee);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }


            return View(employeemodel);
        }

        // search details end

        // *************************************************************************************************//
        // Employee data related end

        // view break

        // IDR indent related start
        // *************************************************************************************************//

        // My group IDR indent request list start
        [HttpGet]
        public IActionResult MyGroupRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                // check if user logged in or not
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
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
        public IActionResult MyGroupRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            List<IndentModel> indentmodel = new List<IndentModel>();
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                // check if user logged in or not
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = AuthorityHelper.GetMyGroupRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(indentmodel);
        }

        public IActionResult MyGroupRequestWithoutFilter()
        {
            ViewBag.Search = true;

            List<IndentModel> indentmodel = new List<IndentModel>();
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                // check if user logged in or not
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = AuthorityHelper.GetMyGroupRequestListWithoutFilter(SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(indentmodel);
        }
        // My group IDR indent request list end

        // view break

        // View My group IDR indent request list request start
        public IActionResult ViewMyGroupIndentRequest(string IndentID)
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
                else if (HttpContext.Session.GetString("Role") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                indentmodel = AuthorityHelper.GetIndentData(SessionStaffNumber, IndentID, IPAddress);

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

        // View My group IDR indent request list request end

        // *************************************************************************************************//
        // IDR indent related end

        // view break

        // Material Gate Pass related start
        // *************************************************************************************************//

        // my group gatepass request list start
        [HttpGet]
        public IActionResult MyGroupMaterialGatePassRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                // check if user logged in or not
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
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
        public IActionResult MyGroupMaterialGatePassRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            List<MaterialGatePassModel> materialgatepass = new List<MaterialGatePassModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                // check if user logged in or not
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                materialgatepass = AuthorityHelper.GetMyGroupMaterialGatePassRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(materialgatepass);
        }

        public IActionResult MyGroupMaterialGatePassRequestWithoutFilter()
        {
            ViewBag.Search = true;

            List<MaterialGatePassModel> materialgatepass = new List<MaterialGatePassModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                // check if user logged in or not
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                materialgatepass = AuthorityHelper.GetMyGroupMaterialGatePassRequestListWithoutFilter(SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(materialgatepass);
        }
        //  my group gatepass request list end

        // view break

        // view my group gatepass request start
        public IActionResult ViewMyGroupMaterialGatePassRequest(string GatePassID)
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

                materialgatepass = AuthorityHelper.GetMaterialGatePassData(SessionStaffNumber, GatePassID, IPAddress);

                foreach (var gatepass in materialgatepass)
                {
                    if (gatepass.IsAttachmentPresent == false)
                    {
                        gatepass.Items = AuthorityHelper.GetMaterialGatePassItems(SessionStaffNumber, GatePassID, IPAddress);
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

        // view my group gatepass request end

        // *************************************************************************************************//
        // Material Gate Pass related end

        // view break

        // Permission request related start
        // *************************************************************************************************//

        // my group permission request list start

        [HttpGet]
        public IActionResult MyGroupPermissionRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                // check if user logged in or not
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
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
        public IActionResult MyGroupPermissionRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            List<PermissionSlipModel> permissionrequests = new List<PermissionSlipModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                // check if user logged in or not
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                permissionrequests = AuthorityHelper.GetMyGroupPermissionRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(permissionrequests);
        }

        public IActionResult MyGroupPermissionRequestWithoutFilter()
        {
            ViewBag.Search = true;

            List<PermissionSlipModel> permissionrequests = new List<PermissionSlipModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                // check if user logged in or not
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                permissionrequests = AuthorityHelper.GetMyGroupPermissionRequestList(SessionStaffNumber, Year, 0, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(permissionrequests);
        }

        // my group permission request list end

        // view break

        // view my group permission request start
        public IActionResult ViewMyGroupPermissionRequest(string PermissionID)
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

                permissionmodel = AuthorityHelper.GetPermissionData(SessionStaffNumber, PermissionID, IPAddress);

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(permissionmodel);
        }

        // view my group permission request end

        // *************************************************************************************************//
        // Permission request related end

        // view break

        // Visitor Pass related start
        // *************************************************************************************************//

        // my group visitor pass request start

        [HttpGet]
        public IActionResult MyGroupVisitorPassRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                // check if user logged in or not
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
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
        public IActionResult MyGroupVisitorPassRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            List<VisitorPassModel> model = new List<VisitorPassModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                // check if user logged in or not
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = AuthorityHelper.GetPendingVisitorPassRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(model);
        }

        public IActionResult MyGroupVisitorPassRequestWithoutFilter()
        {
            ViewBag.Search = true;

            List<VisitorPassModel> model = new List<VisitorPassModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                // check if user logged in or not
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = AuthorityHelper.GetPendingVisitorPassRequestList(SessionStaffNumber, Year, 0, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(model);
        }

        // my group visitor pass request end

        // view break

        // view my group visitor pass request start
        public IActionResult ViewMyGroupVisitorPassRequest(string VisitorPassID)
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

                model = AuthorityHelper.GetVisitorPassData(SessionStaffNumber, VisitorPassID, IPAddress);

                int i = 1;

                foreach (var visitors in model)
                {
                    visitors.VisitorsModel = AuthorityHelper.GetVisitorPassVisitorsData(SessionStaffNumber, VisitorPassID, IPAddress);

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

        // view my group visitor pass request end

        // *************************************************************************************************//
        // Visitor Pass related end

        // view break

        // Vehicle Indent related start
        // *************************************************************************************************//

        // my group vehicle indent request start

        [HttpGet]
        public IActionResult MyGroupVehicleIndentRequest()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                // check if user logged in or not
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
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
        public IActionResult MyGroupVehicleIndentRequest(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            List<VehicleIndentModel> model = new List<VehicleIndentModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                // check if user logged in or not
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = AuthorityHelper.GetMyGroupVehicleRequestList(SessionStaffNumber, Year, Month, IPAddress, Status);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(model);
        }

        public IActionResult MyGroupVehicleIndentRequestWithoutFilter()
        {
            ViewBag.Search = true;

            List<VehicleIndentModel> model = new List<VehicleIndentModel>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                // check if user logged in or not
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = AuthorityHelper.GetMyGroupVehicleRequestListWithoutFilter(SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(model);
        }

        // my group vehicle indent request end

        // view break

        // view my group vehicle indent request start

        public IActionResult ViewMyGroupVehicleIndentRequest(string IndentID)
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

                indentmodel = AuthorityHelper.GetVehicleIndentData(SessionStaffNumber, IndentID, IPAddress);

                foreach (var indent in indentmodel)
                {
                    indent.IndentInvolvedPersons = AuthorityHelper.GetVehicleIndentPersons(SessionStaffNumber, IndentID, IPAddress);
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

        // view my group vehicle indent request end

        // *************************************************************************************************//
        // Vehicle Indent related end





        //----------------------PurchaseIndentpto25k-----------------------

        [HttpGet]
        public IActionResult MyGroupPurchaseIndentUpto25k()
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                // check if user logged in or not
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
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
        public IActionResult MyGroupPurchaseIndentUpto25k(int Year, int Month, string Status)
        {
            ViewBag.Search = true;

            List<PurchaseIndentModelUpto25Model> purchase = new List<PurchaseIndentModelUpto25Model>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                // check if user logged in or not
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                purchase = AuthorityHelper.GetMyGroupPuchaseRequest(SessionStaffNumber, Year, Month, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(purchase);
        }

        public IActionResult MyGroupPurchaseIndentUpto25kRequestWithoutFilter()
        {
            ViewBag.Search = true;

            List<PurchaseIndentModelUpto25Model> purchase = new List<PurchaseIndentModelUpto25Model>();

            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                int Year = DateTime.Now.Year;
                string Status = "Null";

                // check if user logged in or not
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Authority" && HttpContext.Session.GetString("Role") != "AdminAuthority")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                purchase = AuthorityHelper.GetMyGroupPuchaseRequest(SessionStaffNumber, Year, 0, Status, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(purchase);
        }

    }
}