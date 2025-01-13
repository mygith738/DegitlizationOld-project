using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Digitalization.Models;
using Digitalization.Models.AdminModel;
using Digitalization.Models.CommonModel;
using Digitalization.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Security.Application;
using UAParser;

namespace Digitalization.Controllers
{
    public class AdminController : Controller
    {
        private readonly IWebHostEnvironment _hostingenvironment;

        public AdminController(IWebHostEnvironment hostingenvironment)
        {
            _hostingenvironment = hostingenvironment;
        }

        // Common tasks start
        // *************************************************************************************************//

        // index start
        public IActionResult Index()
        {
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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }
            }
            catch (Exception error)
            {
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                CommonHelper.LogError(error, 0000, IPAddress);
                Console.WriteLine(error.Message);
            }
            return RedirectToAction("Index", "User");
        }
        // index end

        // *************************************************************************************************//
        // Common tasks end

        // view break

        // Employee data related start
        // *************************************************************************************************//

        // Register start
        [HttpGet]
        public IActionResult Registration()
        {
            RegisterModel model = new RegisterModel();
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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                // show success message is there is any
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
                TempData["SuccessMessage"] = "";

                // Error Message
                ViewBag.ExceptionError = TempData["ExceptionError"];
                TempData["ExceptionError"] = "";

                // getting staff number from session
                int StaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

                model.Groups = AdminHelper.GetGroupData(StaffNumber, IPAddress);
                model.Centers = AdminHelper.GetCenterData(StaffNumber, IPAddress);
                model.Designations = AdminHelper.GetDesignationData(StaffNumber, IPAddress);
                model.MinorityModel = AdminHelper.GetMinorityData(StaffNumber, IPAddress);
                model.PhysicallyHandicappedModel = AdminHelper.GetPhysicallyHandicappedData(StaffNumber, IPAddress);

                ViewBag.IsCenterSelected = false;
            }
            catch (Exception error)
            {
                // getting staff number from session
                int StaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                CommonHelper.LogError(error, StaffNumber, IPAddress);
                Console.WriteLine(error.Message);
                model.Groups = AdminHelper.GetGroupData(StaffNumber, IPAddress);
                model.Centers = AdminHelper.GetCenterData(StaffNumber, IPAddress);
                model.Designations = AdminHelper.GetDesignationData(StaffNumber, IPAddress);
                model.MinorityModel = AdminHelper.GetMinorityData(StaffNumber, IPAddress);
                model.PhysicallyHandicappedModel = AdminHelper.GetPhysicallyHandicappedData(StaffNumber, IPAddress);
            }


            // Because in dropdown first value is -1
            model.EmployeeCategory = -1;
            model.IsMinorities = -1;
            model.IsPhysicallyHandicapped = -1;
            model.IsExServiceMen = -1;

            return View(model);
        }

        [HttpPost]
        public IActionResult Registration(RegisterModel model, IFormFile UserImage)
        {
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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                if (model.CenterId == 0)
                {
                    // getting staff number from session
                    int StaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                    // to get IPAddress
                    string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

                    model.Groups = AdminHelper.GetGroupData(StaffNumber, IPAddress);
                    model.Centers = AdminHelper.GetCenterData(StaffNumber, IPAddress);
                    model.Designations = AdminHelper.GetDesignationData(StaffNumber, IPAddress);
                    model.MinorityModel = AdminHelper.GetMinorityData(StaffNumber, IPAddress);
                    model.PhysicallyHandicappedModel = AdminHelper.GetPhysicallyHandicappedData(StaffNumber, IPAddress);

                    ViewBag.IsCenterSelected = false;

                    model.EmployeeCategory = -1;
                    model.IsMinorities = -1;
                    model.IsPhysicallyHandicapped = -1;
                    model.IsExServiceMen = -1;

                    ViewBag.CenterError = "Please select center";

                    return View(model);
                }

                else if (model.CenterId != 0 && model.CenterId != -1 && model.GroupId != 0 && model.GroupId != -1)
                {
                    var configuration = CommonHelper.GetConfig();
                    var loglocation = configuration["FileLocations:EmployeeImageLocation"];
                    string folderName = loglocation;
                    folderName = folderName.Replace("StaffNumber", model.StaffNumber.ToString());
                    //string uploadPath = Server.MapPath(folderName);
                    string webRootPath = _hostingenvironment.ContentRootPath;
                    string newPath = Path.Combine(webRootPath, folderName);
                    if (!Directory.Exists(folderName))
                    {
                        Directory.CreateDirectory(folderName);
                    }
                    string TemporaryFileName = UserImage.FileName;
                    string Extension = TemporaryFileName.Split('.').Last();
                    string FileName = CommonHelper.GeneratePassword(10) + "." + Extension;
                    model.UserImagePath = Path.Combine(folderName, FileName);
                    string envpath = folderName + "\\" + FileName;

                    using (var stream = new FileStream(model.UserImagePath, FileMode.Create))
                    {
                        UserImage.CopyTo(stream);
                    }

                    // applying AntiXSS to vomit any script code 
                    model.PresentAddress = Sanitizer.GetSafeHtmlFragment(model.PresentAddress);
                    model.PermanentAddress = Sanitizer.GetSafeHtmlFragment(model.PermanentAddress);
                    model.Remarks = Sanitizer.GetSafeHtmlFragment(model.Remarks);

                    // assign the empty and 0 value to Contract Employee data
                    if (model.EmployeeCategory == 0)
                    {
                        model.OfficialEmailId = "";
                        model.EmployeeGroup = "";
                        model.EntryLevel = "";
                        model.CurrentBasicPay = "";
                        model.SpouseInGovernment = "";
                    }
                    else
                    {
                        model.IsHRAApplicable = 0;
                    }
                    
                    if(model.AccountNumber == null)
                    {
                        model.AccountNumber = "";
                    }

                    // get staffnumber from session
                    int StaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                    // to get browser name
                    var userAgent = HttpContext.Request.Headers["User-Agent"];
                    var uaParser = Parser.GetDefault();
                    ClientInfo c = uaParser.Parse(userAgent);
                    string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                    // to get IPAddress
                    string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                    // check if staff number exists
                    bool IsUserExists = CommonHelper.CheckUserExists(model.StaffNumber, IPAddress);
                    if (IsUserExists)
                    {
                        ViewBag.ErrorMessage = "Staff number already exists";
                        // getting staff number from session
                        //int StaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                        // to get IPAddress
                        //string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

                        model.Groups = AdminHelper.GetGroupData(StaffNumber, IPAddress);
                        model.Centers = AdminHelper.GetCenterData(StaffNumber, IPAddress);
                        model.Designations = AdminHelper.GetDesignationData(StaffNumber, IPAddress);
                        model.MinorityModel = AdminHelper.GetMinorityData(StaffNumber, IPAddress);
                        model.PhysicallyHandicappedModel = AdminHelper.GetPhysicallyHandicappedData(StaffNumber, IPAddress);

                        ViewBag.IsCenterSelected = false;

                        model.EmployeeCategory = -1;
                        model.IsMinorities = -1;
                        model.IsPhysicallyHandicapped = -1;
                        model.IsExServiceMen = -1;

                        return View(model);
                    }

                    // check if registration successfull or not
                    bool IsRegistered = AdminHelper.RegisterUser(model, BrowserName, StaffNumber, IPAddress);

                    if (IsRegistered)
                    {
                        // redirect to Registration page with Success Message
                        TempData["SuccessMessage"] = "Registration of " + model.StaffNumber + " successful";
                        return RedirectToAction("Registration", "Admin");
                    }

                    TempData["ExceptionError"] = "An error has occurred. Please contact system administrator.";
                    return RedirectToAction("Registration", "Admin");
                }
                else
                {
                    // to get IPAddress
                    string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                    int CenterID = model.CenterId;
                    int StaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                    model.Groups = AdminHelper.GetGroupDataFromCenter(model.CenterId, StaffNumber, IPAddress);
                    model.Centers = AdminHelper.GetCenterData(StaffNumber, IPAddress);
                    model.Designations = AdminHelper.GetDesignationData(StaffNumber, IPAddress);
                    model.MinorityModel = AdminHelper.GetMinorityData(StaffNumber, IPAddress);
                    model.PhysicallyHandicappedModel = AdminHelper.GetPhysicallyHandicappedData(StaffNumber, IPAddress);
                    model.CenterId = CenterID;
                    ViewBag.IsCenterSelected = true;
                }
            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                int StaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, StaffNumber, IPAddress);
                Console.WriteLine(error.Message);
                model.Groups = AdminHelper.GetGroupData(StaffNumber, IPAddress);
                model.Centers = AdminHelper.GetCenterData(StaffNumber, IPAddress);
                model.Designations = AdminHelper.GetDesignationData(StaffNumber, IPAddress);
            }
            return View(model);
        }
        // Registration end

        // view break

        // Employee Request Start
        [HttpGet]
        public IActionResult EmployeeRequest()
        {
            try
            {
                // success message
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                // get StaffNumber from Session
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
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

            try
            {
                // success message
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                // get StaffNumber from Session
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                // get employee request list
                employeemodel = AdminHelper.GetEmployeeRequestList(SessionStaffNumber, Year, Month, Status, IPAddress);
                foreach (var employee in employeemodel)
                {
                    // replacing empty field with -
                    if (employee.LastName == "")
                    {
                        employee.LastName = "-";
                    }
                }

                //ViewBag.IsFiltered = true;
            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(employeemodel);
        }

        public IActionResult EmployeeRequestWithoutFilter()
        {
            // Profile Model
            List<EmployeeModel> employeemodel = new List<EmployeeModel>();

            try
            {
                // success message
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
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                // get StaffNumber from Session
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                // get employee request list
                employeemodel = AdminHelper.GetEmployeeRequestList(SessionStaffNumber, 2022, 0, 'S', IPAddress);
                foreach (var employee in employeemodel)
                {
                    // replacing empty field with -
                    if (employee.LastName == "")
                    {
                        employee.LastName = "-";
                    }
                }

                //ViewBag.IsFiltered = true;
            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(employeemodel);
        }
        // Employee Request end

        // view break

        // Employee Request Data Start
        [HttpGet]
        public IActionResult EmployeeRequestData(int StaffNumber)
        {
            var employeemodel = new List<EmployeeModel>();

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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                // get Staff Number from Session
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                var employee = new EmployeeModel();
                employee = AdminHelper.GetEmployeeData(StaffNumber, SessionStaffNumber, IPAddress);
                employee.Status = 'N';

                // check IsPhotoAvailable field is true or false and create a viewbag to toggle to show photo or not
                if (employee.IsPhotoAvailable)
                {
                    ViewBag.IsPhotoAvailable = true;
                    var configuration = CommonHelper.GetConfig();
                    var loglocation = configuration["FileLocations:EmployeeImageLocationForDownload"];

                    ViewBag.UserImagePath = employee.UserImagePath.Replace(loglocation, "../StaffPhotoUploads/");
                }
                else
                {
                    ViewBag.IsPhotoAvailable = false;
                }

                employee.Groups = AdminHelper.GetGroupData(SessionStaffNumber, IPAddress);
                employee.Centers = AdminHelper.GetCenterData(SessionStaffNumber, IPAddress);
                employee.Designations = AdminHelper.GetDesignationData(SessionStaffNumber, IPAddress);
                employee.EmergencyContactModel = AdminHelper.GetEmergencyContactData(StaffNumber, SessionStaffNumber, IPAddress);
                ViewBag.EmergencyCount = employee.EmergencyContactModel.Count();
                employee.MinorityModel = AdminHelper.GetMinorityData(SessionStaffNumber, IPAddress);
                employee.PhysicallyHandicappedModel = AdminHelper.GetPhysicallyHandicappedData(SessionStaffNumber, IPAddress);
                employee.FamilyDeclarationModel = AdminHelper.GetFamilyDeclarationData(StaffNumber, SessionStaffNumber, IPAddress);
                ViewBag.FamilyValueCount = employee.FamilyDeclarationModel.Count();
                employee.MedicalDependentsModel = AdminHelper.GetMedicalDependentsData(StaffNumber, SessionStaffNumber, IPAddress);
                ViewBag.MedicalValueCount = employee.MedicalDependentsModel.Count();
                employee.GratuityModel = AdminHelper.GetGratuityData(StaffNumber, SessionStaffNumber, IPAddress);
                ViewBag.GratuityValueCount = employee.GratuityModel.Count();
                employee.QualificationModel = AdminHelper.GetQualificationData(StaffNumber, SessionStaffNumber, IPAddress);
                ViewBag.QualificationCount = employee.QualificationModel.Count();
                for (var i = 1; i <= 10; i++)
                {
                    if (i > employee.FamilyDeclarationModel.Count)
                    {
                        var familydeclaration = new FamilyDeclarationModel();
                        //familydeclaration.RequestID = 0;
                        familydeclaration.StaffNumber = 0;
                        //familydeclaration.StaffName = "";
                        familydeclaration.RelativeName = "";
                        familydeclaration.Relation = "";
                        familydeclaration.DateOfBirth = Convert.ToDateTime("01-01-2001");
                        //familydeclaration.Status = 'S';
                        //familydeclaration.UserRemarks = "";
                        familydeclaration.DependentID = i;
                        employee.FamilyDeclarationModel.Add(familydeclaration);
                    }
                }
                for (var i = 1; i <= 10; i++)
                {
                    if (i > employee.MedicalDependentsModel.Count)
                    {
                        var medicaldependents = new MedicalDependentsModel();
                        //medicaldependents.RequestID = 0;
                        medicaldependents.StaffNumber = 0;
                        //medicaldependents.StaffName = "";
                        medicaldependents.DependentName = "";
                        medicaldependents.Relation = "";
                        medicaldependents.DateOfBirth = Convert.ToDateTime("01-01-2001");
                        //medicaldependents.Status = 'S';
                        //medicaldependents.UserRemarks = "";
                        medicaldependents.DependentID = i;
                        employee.MedicalDependentsModel.Add(medicaldependents);
                    }
                }
                for (var i = 1; i <= 10; i++)
                {
                    if (i > employee.GratuityModel.Count)
                    {
                        var gratuity = new GratuityModel();
                        gratuity.StaffNumber = 0;
                        gratuity.NomineeName = "";
                        gratuity.Percentage = 0;
                        gratuity.NomineeID = i;
                        employee.GratuityModel.Add(gratuity);
                    }
                }
                for (var i = 1; i <= 10; i++)
                {
                    if (i > employee.QualificationModel.Count)
                    {
                        var qualification = new QualificationModel();
                        qualification.Qualification1 = "";
                        qualification.Qualification2 = "";
                        qualification.Specialization = "";
                        qualification.QualificationID = i;
                        employee.QualificationModel.Add(qualification);
                    }
                }
                for (var i = 1; i <= 10; i++)
                {
                    if (i > employee.EmergencyContactModel.Count)
                    {
                        var emergency = new EmergencyContactModel();
                        emergency.ContactName = "";
                        emergency.LandlineNumber = "";
                        emergency.MobilePhoneNumber = "";
                        emergency.EmailId = "";
                        emergency.Relation = "";
                        emergency.EmergencyID = i;
                        employee.EmergencyContactModel.Add(emergency);
                    }
                }
                employeemodel.Add(employee);
            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(employeemodel);
        }

        [HttpPost]
        public IActionResult EmployeeRequestData(List<EmployeeModel> model, IFormFile userimage)
        {
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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                // get StaffNumber from Session
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                foreach (var employee in model)
                {
                    // apply antixss to vomit any script data
                    employee.PresentAddress = Sanitizer.GetSafeHtmlFragment(employee.PresentAddress);
                    employee.PermanentAddress = Sanitizer.GetSafeHtmlFragment(employee.PermanentAddress);
                    employee.Remarks = Sanitizer.GetSafeHtmlFragment(employee.Remarks);

                    if (employee.EmployeeCategory == "Contract Employee")
                    {
                        employee.OfficialEmailId = "";
                        employee.MaritalStatus = "";
                        employee.EmployeeGroup = "";
                        employee.EntryLevel = "";
                        employee.CurrentBasicPay = 0;
                        employee.SpouseInGovernment = "";
                    }
                    if(employee.PAN == null)
                    {
                        employee.PAN = "";
                    }
                    if(employee.CasteCategory == "Null")
                    {
                        employee.CasteCategory = "";
                    }
                    if(employee.AadharNumber == null)
                    {
                        employee.AadharNumber = "";
                    }
                    if(employee.PersonalEmailId == null)
                    {
                        employee.PersonalEmailId = "";
                    }
                    if(employee.AccountNumber == "0")
                    {
                        employee.AccountNumber = "";
                    }
                    
                    foreach(var qualification in employee.QualificationModel)
                    {
                        if(qualification.Qualification1 == "Other")
                        {
                            qualification.Qualification2 = qualification.Qualification2Other;
                        }
                    }
                }

                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                // update data
                bool IsUpdated = AdminHelper.UpdateEmployeeData(model, BrowserName, SessionStaffNumber, IPAddress);

                // if updated send email
                if (IsUpdated)
                {
                    //foreach (var employee in model)
                    //{
                    //    // send approved email
                    //    if (employee.Status == 'P')
                    //    {
                    //        // send email to Admin that he has approved the request
                    //        string[] AdminEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber);
                    //        string AdminEmailID = AdminEmailAndName[0];
                    //        string AdminFirstName = AdminEmailAndName[1];
                    //        string AdminMessage = "Dear " + AdminFirstName + ",<br/>You have checked the request of <b>" + employee.StaffNumber + "</b> with <b>" + employee.Remarks + "</b> remarks.";
                    //        string AdminSubject = "Request";
                    //        CommonHelper.SendEmail(AdminEmailID, AdminSubject, AdminMessage, employee.StaffNumber);
                    //    }
                    //    // send declined email
                    //    else if (employee.Status == 'D')
                    //    {
                    //        // send email to Employee that request has been declined
                    //        string[] UserEmailAndName = CommonHelper.GetEmailAndName(employee.StaffNumber);
                    //        string UserEmailID = UserEmailAndName[0];
                    //        string UserFirstName = UserEmailAndName[1];
                    //        string UserMessage = "Dear " + UserFirstName + ",<br/>Your request has declined with <b>" + employee.Remarks + "</b> remarks.";
                    //        string UserSubject = "Request";
                    //        CommonHelper.SendEmail(UserEmailID, UserSubject, UserMessage, employee.StaffNumber);

                    //        // send email to Admin that he has decline the request
                    //        string[] AdminEmailAndName = CommonHelper.GetEmailAndName(SessionStaffNumber);
                    //        string AdminEmailID = AdminEmailAndName[0];
                    //        string AdminFirstName = AdminEmailAndName[1];
                    //        string AdminMessage = "Dear " + AdminFirstName + ",<br/>You have declined the request of <b>" + employee.StaffNumber + "</b> with <b>" + employee.Remarks + "</b> remarks.";
                    //        string AdminSubject = "Request";
                    //        CommonHelper.SendEmail(AdminEmailID, AdminSubject, AdminMessage, employee.StaffNumber);
                    //    }
                    //}

                    TempData["SuccessMessage"] = "Update successful";
                    return RedirectToAction("EmployeeRequest", "Admin");
                }
                else
                {
                    TempData["ExceptionError"] = "An error has occurred. Please contact system administrator";
                    return RedirectToAction("EmployeeRequest", "Admin");
                }
            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return RedirectToAction("EmployeeRequest", "Admin");
        }

        // Employee Request Data end

        // view break

        // Employee Search start
        [HttpGet]
        public IActionResult EmployeeSearch()
        {
            // success message
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            // error message
            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                // get staffnumber from session
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                searchmodel.Groups = AdminHelper.GetGroupData(SessionStaffNumber, IPAddress);
                searchmodel.Centers = AdminHelper.GetCenterData(SessionStaffNumber, IPAddress);
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
                searchmodel.Groups = AdminHelper.GetGroupData(SessionStaffNumber, IPAddress);
                searchmodel.Centers = AdminHelper.GetCenterData(SessionStaffNumber, IPAddress);
            }
            return View(searchmodel);
        }

        [HttpPost]
        public IActionResult EmployeeSearch(SearchModel model)
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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                // get staffnumber from session
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

                // check which data is entered
                if (model.StaffNumber != 0)
                {
                    // redirect to search if Staff Number is not 0
                    return RedirectToAction("EmployeeSearchData", "Admin", model);
                }
                else if (model.CenterID == -1)
                {
                    // if center is not selected show error that center is not selected
                    ViewBag.CenterError = "Please Select Center";
                    ViewBag.IsCenterSelected = false;
                }
                else if (model.CenterID != -1 && model.GroupID == -1)
                {
                    // if center is selected and group is not selected return to view
                    searchmodel.CenterID = model.CenterID;
                    searchmodel.Centers = AdminHelper.GetCenterData(SessionStaffNumber, IPAddress);
                    searchmodel.Groups = AdminHelper.GetGroupDataFromCenter(model.CenterID, SessionStaffNumber, IPAddress);
                    ViewBag.IsCenterSelected = true;
                    ViewBag.GroupError = "Please Select Group";
                    return View(searchmodel);
                }
                else
                {
                    // if eveything is fine move to search
                    return RedirectToAction("EmployeeSearchData", "Admin", model);
                }

                searchmodel.Groups = AdminHelper.GetGroupData(SessionStaffNumber, IPAddress);
                searchmodel.Centers = AdminHelper.GetCenterData(SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
                searchmodel.Groups = AdminHelper.GetGroupData(SessionStaffNumber, IPAddress);
                searchmodel.Centers = AdminHelper.GetCenterData(SessionStaffNumber, IPAddress);
                ViewBag.IsCenterSelected = false;
                return View(searchmodel);
            }

            return View(searchmodel);
        }

        // employee search end 

        // view break

        // employee search data start

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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }
                // get staffnumber from session
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                if (model.StaffNumber != 0 && model.CenterID == -1 && model.GroupID == -1)
                {
                    // if Staffnumber is entered search based on staff number
                    employeemodel = AdminHelper.GetEmployeeDataListFromStaffNumber(model.StaffNumber, SessionStaffNumber, IPAddress);
                }
                else if (model.StaffNumber == 0 && model.CenterID != -1 && model.GroupID != -1)
                {
                    // if center and group is selected search based on center and group
                    employeemodel = AdminHelper.GetEmployeeDataListFromGroupCenter(model.GroupID, model.CenterID, SessionStaffNumber, IPAddress);
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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                var employee = new EmployeeModel();
                employee = AdminHelper.GetEmployeeDataFromSearch(StaffNumber, SessionStaffNumber, IPAddress);
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
                // Group, Center and Designation data is not necessary bcz search details are not editable

                //employee.Groups = AdminHelper.GetGroupData(SessionStaffNumber);
                //employee.Centers = AdminHelper.GetCenterData(SessionStaffNumber);
                //employee.Designations = AdminHelper.GetDesignationData(SessionStaffNumber);
                employee.QualificationModel = AdminHelper.GetQualificationDataForSearch(StaffNumber, SessionStaffNumber, IPAddress);
                employee.EmergencyContactModel = AdminHelper.GetEmergencyContactDataForSearch(StaffNumber, SessionStaffNumber, IPAddress);
                employee.FamilyDeclarationModel = AdminHelper.GetFamilyDeclarationDataForSearch(StaffNumber, SessionStaffNumber, IPAddress);
                employee.MedicalDependentsModel = AdminHelper.GetMedicalDependentsDataForSearch(StaffNumber, SessionStaffNumber, IPAddress);
                employee.GratuityModel = AdminHelper.GetGratuityDataForSearch(StaffNumber, SessionStaffNumber, IPAddress);
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

        // employee search data end

        // view break

        // edit employee start

        [HttpGet]
        public IActionResult EditEmployeeData(int StaffNumber)
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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                var employee = new EmployeeModel();
                employee = AdminHelper.GetEmployeeDataFromSearch(StaffNumber, SessionStaffNumber, IPAddress);

                employee.Gender = employee.Gender switch
                {
                    "Male" => "M",
                    "Female" => "F",
                    "Other" => "O",
                    _ => "U",
                };

                employee.MaritalStatus = employee.MaritalStatus switch
                {
                    "Married" => "M",
                    "Unmarried" => "U",
                    "Widow" => "W",
                    "Divorcee" => "D",
                    "Widower" => "R",
                    _ => "U",
                };

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

                employee.Groups = AdminHelper.GetGroupDataFromCenter(employee.CenterID, SessionStaffNumber, IPAddress);
                employee.Centers = AdminHelper.GetCenterData(SessionStaffNumber, IPAddress);
                employee.Designations = AdminHelper.GetDesignationData(SessionStaffNumber, IPAddress);
                employee.QualificationModel = AdminHelper.GetQualificationDataForSearch(StaffNumber, SessionStaffNumber, IPAddress);
                ViewBag.QualificationCount = employee.QualificationModel.Count();
                employee.EmergencyContactModel = AdminHelper.GetEmergencyContactDataForSearch(StaffNumber, SessionStaffNumber, IPAddress);
                ViewBag.EmergencyCount = employee.EmergencyContactModel.Count();
                employee.FamilyDeclarationModel = AdminHelper.GetFamilyDeclarationDataForSearch(StaffNumber, SessionStaffNumber, IPAddress);
                ViewBag.FamilyValueCount = employee.FamilyDeclarationModel.Count();
                employee.MedicalDependentsModel = AdminHelper.GetMedicalDependentsDataForSearch(StaffNumber, SessionStaffNumber, IPAddress);
                ViewBag.MedicalValueCount = employee.MedicalDependentsModel.Count();
                employee.GratuityModel = AdminHelper.GetGratuityDataForSearch(StaffNumber, SessionStaffNumber, IPAddress);
                ViewBag.GratuityValueCount = employee.GratuityModel.Count();
                employee.MinorityModel = AdminHelper.GetMinorityData(SessionStaffNumber, IPAddress);
                employee.PhysicallyHandicappedModel = AdminHelper.GetPhysicallyHandicappedData(SessionStaffNumber, IPAddress);
                for (var i = 1; i <= 10; i++)
                {
                    if (i > employee.FamilyDeclarationModel.Count)
                    {
                        var familydeclaration = new FamilyDeclarationModel();
                        //familydeclaration.RequestID = 0;
                        familydeclaration.StaffNumber = 0;
                        //familydeclaration.StaffName = "";
                        familydeclaration.RelativeName = "";
                        familydeclaration.Relation = "";
                        familydeclaration.DateOfBirth = Convert.ToDateTime("01-01-2001");
                        //familydeclaration.Status = 'S';
                        //familydeclaration.UserRemarks = "";
                        familydeclaration.DependentID = i;
                        employee.FamilyDeclarationModel.Add(familydeclaration);
                    }
                }
                for (var i = 1; i <= 10; i++)
                {
                    if (i > employee.MedicalDependentsModel.Count)
                    {
                        var medicaldependents = new MedicalDependentsModel();
                        //medicaldependents.RequestID = 0;
                        medicaldependents.StaffNumber = 0;
                        //medicaldependents.StaffName = "";
                        medicaldependents.DependentName = "";
                        medicaldependents.Relation = "";
                        medicaldependents.DateOfBirth = Convert.ToDateTime("01-01-2001");
                        //medicaldependents.Status = 'S';
                        //medicaldependents.UserRemarks = "";
                        medicaldependents.DependentID = i;
                        employee.MedicalDependentsModel.Add(medicaldependents);
                    }
                }
                for (var i = 1; i <= 10; i++)
                {
                    if (i > employee.GratuityModel.Count)
                    {
                        var gratuity = new GratuityModel();
                        gratuity.StaffNumber = 0;
                        gratuity.NomineeName = "";
                        gratuity.Percentage = 0;
                        gratuity.NomineeID = i;
                        employee.GratuityModel.Add(gratuity);
                    }
                }
                for (var i = 1; i <= 10; i++)
                {
                    if (i > employee.QualificationModel.Count)
                    {
                        var qualification = new QualificationModel();
                        qualification.Qualification1 = "";
                        qualification.Qualification2 = "";
                        qualification.Specialization = "";
                        qualification.QualificationID = i;
                        employee.QualificationModel.Add(qualification);
                    }
                }
                for (var i = 1; i <= 10; i++)
                {
                    if (i > employee.EmergencyContactModel.Count)
                    {
                        var emergency = new EmergencyContactModel();
                        emergency.ContactName = "";
                        emergency.LandlineNumber = "";
                        emergency.MobilePhoneNumber = "";
                        emergency.EmailId = "";
                        emergency.Relation = "";
                        emergency.EmergencyID = i;
                        employee.EmergencyContactModel.Add(emergency);
                    }
                }
                employeemodel.Add(employee);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            ViewBag.IsCenterSelected = true;
            return View(employeemodel);
        }

        [HttpPost]
        public IActionResult EditEmployeeData(List<EmployeeModel> model, IFormFile userimage)
        {
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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                // get StaffNumber from Session
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                bool IsGroupSelected = false;

                foreach (var employee in model)
                {
                    if (employee.GroupName != "Null")
                    {
                        employee.GroupName = employee.GroupID.ToString();
                        employee.CenterName = employee.CenterID.ToString();

                        if (userimage != null)
                        {
                            var configuration = CommonHelper.GetConfig();
                            var loglocation = configuration["FileLocations:EmployeeImageLocation"];
                            string folderName = loglocation;
                            folderName = folderName.Replace("StaffNumber", employee.StaffNumber.ToString());
                            //string uploadPath = Server.MapPath(folderName);
                            string webRootPath = _hostingenvironment.ContentRootPath;
                            string newPath = Path.Combine(webRootPath, folderName);
                            if (!Directory.Exists(folderName))
                            {
                                Directory.CreateDirectory(folderName);
                            }
                            string TemporaryFileName = userimage.FileName;
                            string Extension = TemporaryFileName.Split('.').Last();
                            string FileName = CommonHelper.GeneratePassword(10) + "." + Extension;
                            employee.UserImagePath = Path.Combine(folderName, FileName);
                            string envpath = folderName + "\\" + FileName;

                            using (var stream = new FileStream(employee.UserImagePath, FileMode.Create))
                            {
                                userimage.CopyTo(stream);
                            }
                        }

                        // apply antixss to vomit any script data
                        employee.PresentAddress = Sanitizer.GetSafeHtmlFragment(employee.PresentAddress);
                        employee.PermanentAddress = Sanitizer.GetSafeHtmlFragment(employee.PermanentAddress);
                        employee.Remarks = Sanitizer.GetSafeHtmlFragment(employee.Remarks);

                        if (employee.EmployeeCategory == "Contract Employee")
                        {
                            employee.OfficialEmailId = "";
                            employee.EmployeeGroup = "";
                            employee.EntryLevel = "";
                            employee.CurrentBasicPay = 0;
                            employee.SpouseInGovernment = "";
                        }
                        if(employee.AccountNumber == null)
                        {
                            employee.AccountNumber = "";
                        }
                        if(employee.AadharNumber == null)
                        {
                            employee.AadharNumber = "";
                        }
                        if(employee.PAN == null)
                        {
                            employee.PAN = "";
                        }

                        IsGroupSelected = true;
                    }
                }

                if (IsGroupSelected)
                {
                    // update data
                    bool IsUpdated = AdminHelper.UpdateEmployeeData(model, BrowserName, SessionStaffNumber, IPAddress);

                    // if updated send email
                    if (IsUpdated)
                    {
                        TempData["SuccessMessage"] = "Update successful";
                        return RedirectToAction("EmployeeSearch", "Admin");
                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error has occurred. Please contact system administrator";
                        return RedirectToAction("EmployeeSearch", "Admin");
                    }
                }
                else
                {
                    return View(model);
                }

            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return RedirectToAction("EmployeeRequest", "Admin");
        }

        [HttpGet]
        public IActionResult EditCenterGroup(int StaffNumber)
        {
            EmployeeModel employeemodel = new EmployeeModel();

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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                employeemodel = AdminHelper.GetEmployeeDataFromSearch(StaffNumber, SessionStaffNumber, IPAddress);

                // check IsPhotoAvailable field is true or false and create a viewbag to toggle to show photo or not
                if (employeemodel.IsPhotoAvailable)
                {
                    var configuration = CommonHelper.GetConfig();
                    var loglocation = configuration["FileLocations:EmployeeImageLocationForDownload"];

                    ViewBag.UserImagePath = employeemodel.UserImagePath.Replace(loglocation, "../StaffPhotoUploads/");

                    ViewBag.IsPhotoAvailable = true;
                }
                else
                {
                    ViewBag.IsPhotoAvailable = false;
                }

                employeemodel.Groups = AdminHelper.GetGroupDataFromCenter(employeemodel.CenterID, SessionStaffNumber, IPAddress);
                employeemodel.Centers = AdminHelper.GetCenterData(SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
            }

            return View(employeemodel);
        }

        [HttpPost]
        public IActionResult EditCenterGroup(EmployeeModel model)
        {
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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                // get StaffNumber from Session
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                if(model.GroupID == -1)
                {
                    int CenterID = model.CenterID;

                    model = AdminHelper.GetEmployeeDataFromSearch(model.StaffNumber, SessionStaffNumber, IPAddress);

                    // check IsPhotoAvailable field is true or false and create a viewbag to toggle to show photo or not
                    if (model.IsPhotoAvailable)
                    {
                        var configuration = CommonHelper.GetConfig();
                        var loglocation = configuration["FileLocations:EmployeeImageLocationForDownload"];

                        ViewBag.UserImagePath = model.UserImagePath.Replace(loglocation, "../StaffPhotoUploads/");

                        ViewBag.IsPhotoAvailable = true;
                    }
                    else
                    {
                        ViewBag.IsPhotoAvailable = false;
                    }

                    model.Groups = AdminHelper.GetGroupDataFromCenter(CenterID, SessionStaffNumber, IPAddress);
                    model.Centers = AdminHelper.GetCenterData(SessionStaffNumber, IPAddress);
                }
                else
                {
                    model.Remarks = "";

                    bool result = AdminHelper.UpdateCenterGroupData(model,BrowserName,SessionStaffNumber,IPAddress);

                    if (result)
                    {
                        TempData["SuccessMessage"] = "Update successful";
                        return RedirectToAction("EmployeeSearch", "Admin");
                    }
                    else
                    {
                        TempData["ExceptionError"] = "An error has occurred. Please contact system administrator";
                        return RedirectToAction("EmployeeSearch", "Admin");
                    }
                }

                //foreach (var employee in model)
                //{
                //    if (employee.GroupName != "Null")
                //    {
                //        // apply antixss to vomit any script data
                //        employee.PresentAddress = Sanitizer.GetSafeHtmlFragment(employee.PresentAddress);
                //        employee.PermanentAddress = Sanitizer.GetSafeHtmlFragment(employee.PermanentAddress);
                //        employee.Remarks = Sanitizer.GetSafeHtmlFragment(employee.Remarks);

                //        if (employee.EmployeeCategory == "Contract Employee")
                //        {
                //            employee.OfficialEmailId = "";
                //            employee.EmployeeGroup = "";
                //            employee.EntryLevel = "";
                //            employee.CurrentBasicPay = 0;
                //            employee.SpouseInGovernment = "";
                //        }
                //        if (employee.AccountNumber == null)
                //        {
                //            employee.AccountNumber = "";
                //        }
                //        if (employee.AadharNumber == null)
                //        {
                //            employee.AadharNumber = "";
                //        }
                //        if (employee.PAN == null)
                //        {
                //            employee.PAN = "";
                //        }

                //        IsGroupSelected = true;
                //    }
                //}

                //if (IsGroupSelected)
                //{
                //    // update data
                //    bool IsUpdated = AdminHelper.UpdateEmployeeData(model, BrowserName, SessionStaffNumber, IPAddress);

                //    // if updated send email
                //    if (IsUpdated)
                //    {
                //        TempData["SuccessMessage"] = "Update successful";
                //        return RedirectToAction("EmployeeSearch", "Admin");
                //    }
                //    else
                //    {
                //        TempData["ExceptionError"] = "An error has occurred. Please contact system administrator";
                //        return RedirectToAction("EmployeeSearch", "Admin");
                //    }
                //}
                //else
                //{
                //    return View(model);
                //}

            }
            catch (Exception error)
            {
                // to get IPAddress
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(model);
        }

        // edit employee end

        // view break

        // *************************************************************************************************//
        // Employee data related end


        // Manage Center and Group start
        // *************************************************************************************************//

        // view centers start

        public IActionResult ViewCenters()
        {
            // show success message is there is any
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            // Error Message
            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            var centermodel = new List<CenterModel>();

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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                centermodel = AdminHelper.GetCenterData(SessionStaffNumber, IPAddress);

                foreach (var center in centermodel)
                {
                    if (center.CHBackupStaffNumber == "")
                    {
                        center.CHBackupStaffNumber = "-";
                    }
                    if (center.CHBackupName == "")
                    {
                        center.CHBackupName = "-";
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(centermodel);
        }

        // view centers end

        // view break

        // edit center start

        [HttpGet]
        public IActionResult EditCenter(int CenterID)
        {

            // show success message is there is any
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            // Error Message
            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            CenterModel center = new CenterModel();

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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                 
                center = AdminHelper.GetCenterDataForEdit(CenterID, SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(center);
        }

        [HttpPost]
        public IActionResult EditCenter(CenterModel model)
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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                bool IsUpdated = AdminHelper.UpdateCenterData(model, SessionStaffNumber, IPAddress, BrowserName);

                if (IsUpdated)
                {
                    TempData["SuccessMessage"] = "Update successful";
                    return RedirectToAction("ViewCenters", "Admin");
                }
                else
                {
                    TempData["ExceptionError"] = "An error has occurred. Please contact system administrator.";
                    return RedirectToAction("ViewCenters", "Admin");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(model);
        }

        // edit center end

        // view break

        // add center start

        [HttpGet]
        public IActionResult AddCenter()
        {

            CenterModel center = new CenterModel();

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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View();
        }

        [HttpPost]
        public IActionResult AddCenter(CenterModel model)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (model.CenterName == "")
                {
                    ViewBag.CenterNameError = "Please Enter Center name";
                    return View();
                }
                if (model.CHStaffNumber == "")
                {
                    ViewBag.CHStaffNumberError = "Please Enter CH Staffnumber";
                    return View();
                }
                if (model.CHBackupStaffNumber == "")
                {
                    ViewBag.CHBackupStaffNumberError = "Please CH Backup Staffnumber";
                    return View();
                }

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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int CenterCount = AdminHelper.GetCentersCount(SessionStaffNumber, IPAddress);

                model.CenterId = CenterCount + 1;
                bool IsUpdated = AdminHelper.UpdateCenterData(model, SessionStaffNumber, IPAddress, BrowserName);

                if (IsUpdated)
                {
                    TempData["SuccessMessage"] = "New Center " + model.CenterName + " Added";
                    return RedirectToAction("ViewCenters", "Admin");
                }
                else
                {
                    TempData["ExceptionError"] = "An error has occurred. Please contact system administrator.";
                    return RedirectToAction("ViewCenters", "Admin");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(model);
        }

        // add center end

        // view break

        // delete center start

        [HttpGet]
        public IActionResult DeleteCenter(int CenterID)
        {

            // show success message is there is any
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            // Error Message
            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            CenterModel center = new CenterModel();

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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                center = AdminHelper.GetCenterDataForEdit(CenterID, SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(center);
        }

        [HttpPost]
        public IActionResult DeleteCenter(CenterModel model)
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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                bool IsUpdated = AdminHelper.DeleteCenterData(model, SessionStaffNumber, IPAddress, BrowserName);

                if (IsUpdated)
                {
                    TempData["SuccessMessage"] = "Delete of center " + model.CenterName + " successful";
                    return RedirectToAction("ViewCenters", "Admin");
                }
                else
                {
                    TempData["ExceptionError"] = "An error has occurred. Please contact system administrator.";
                    return RedirectToAction("ViewCenters", "Admin");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(model);
        }

        // delete center end

        // view break

        // view groups start

        public IActionResult ViewGroups()
        {
            // show success message is there is any
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            // Error Message
            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            var groupmodel = new List<GroupModel>();

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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                groupmodel = AdminHelper.GetGroupData(SessionStaffNumber, IPAddress);

                foreach (var center in groupmodel)
                {
                    if (center.SGHStaffNumber == "")
                    {
                        center.SGHStaffNumber = "-";
                    }
                    if (center.SGroupHeadName == "")
                    {
                        center.SGroupHeadName = "-";
                    }
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return View(groupmodel);
        }

        // view groups end

        // view break

        // edit group start

        [HttpGet]
        public IActionResult EditGroup(int GroupID)
        {

            // show success message is there is any
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            // Error Message
            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            GroupModel group = new GroupModel();

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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                group = AdminHelper.GetGroupDataForEdit(GroupID, SessionStaffNumber, IPAddress);
                group.Centers = AdminHelper.GetCenterData(SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(group);
        }

        [HttpPost]
        public IActionResult EditGroup(GroupModel model)
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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                bool IsUpdated = AdminHelper.UpdateGroupData(model, SessionStaffNumber, IPAddress, BrowserName);

                if (IsUpdated)
                {
                    TempData["SuccessMessage"] = "Update successful";
                    return RedirectToAction("ViewGroups", "Admin");
                }
                else
                {
                    TempData["ExceptionError"] = "An error has occurred. Please contact system administrator.";
                    return RedirectToAction("ViewGroups", "Admin");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(model);
        }

        // edit group end

        // view break

        // add group start

        [HttpGet]
        public IActionResult AddGroup()
        {

            // show success message is there is any
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            // Error Message
            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            GroupModel group = new GroupModel();

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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                group.Centers = AdminHelper.GetCenterData(SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(group);
        }

        [HttpPost]
        public IActionResult AddGroup(GroupModel model)
        {
            // to get IPAddress
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            try
            {
                if (model.CenterName == "Null" || model.CenterName == "")
                {
                    ViewBag.CenterNameError = "Please select center";
                    return View();
                }
                if (model.GHStaffNumber == "")
                {
                    ViewBag.GHStaffNumberError = "Please enter GH staffnumber";
                    return View();
                }
                if (model.SGHStaffNumber == "")
                {
                    ViewBag.SGHStaffNumberError = "Please enter secondary GH staffnumber";
                    return View();
                }

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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int GroupCount = AdminHelper.GetGroupsCount(SessionStaffNumber, IPAddress);

                model.GroupId = Convert.ToString(GroupCount + 1);

                bool IsUpdated = AdminHelper.UpdateGroupData(model, SessionStaffNumber, IPAddress, BrowserName);

                if (IsUpdated)
                {
                    TempData["SuccessMessage"] = "Insertion of group " + model.GroupName + " successful";
                    return RedirectToAction("ViewGroups", "Admin");
                }
                else
                {
                    TempData["ExceptionError"] = "An error has occurred. Please contact system administrator.";
                    return RedirectToAction("ViewGroups", "Admin");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(model);
        }

        // add group end

        // view break

        // delete group start

        [HttpGet]
        public IActionResult DeleteGroup(int GroupID)
        {

            // show success message is there is any
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            // Error Message
            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            GroupModel group = new GroupModel();

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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                group = AdminHelper.GetGroupDataForEdit(GroupID, SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(group);
        }

        [HttpPost]
        public IActionResult DeleteGroup(GroupModel model)
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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                bool IsUpdated = AdminHelper.DeleteGroupData(model, SessionStaffNumber, IPAddress, BrowserName);

                if (IsUpdated)
                {
                    TempData["SuccessMessage"] = "Delete of group " + model.GroupName + " successful";
                    return RedirectToAction("ViewGroups", "Admin");
                }
                else
                {
                    TempData["ExceptionError"] = "An error has occurred. Please contact system administrator.";
                    return RedirectToAction("ViewGroups", "Admin");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(model);
        }

        // delete group end

        // view break

        // *************************************************************************************************//
        // Manage Center and Group end


        // Manage Leave & Permission updates start
        // *************************************************************************************************//

        // half yearly update start

        public IActionResult HalfYearlyUpdate()
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
                else if (HttpContext.Session.GetString("Role") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                string Date = DateTime.Now.Day.ToString();
                string Month = DateTime.Now.Month.ToString();

                if (Date == "1" && (Month == "7" || Month == "1"))
                {
                    // to get browser name
                    var userAgent = HttpContext.Request.Headers["User-Agent"];
                    var uaParser = Parser.GetDefault();
                    ClientInfo c = uaParser.Parse(userAgent);
                    string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                    bool flag = AdminHelper.InsertLeave(IPAddress, SessionStaffNumber, BrowserName);

                    if(flag)
                    {
                        TempData["LeaveInserted"] = "True";
                    }
                    else
                    {
                        TempData["LeaveInserted"] = "False";
                    }
                    
                }

                //if (true)
                //{
                //    // to get browser name
                //    var userAgent = HttpContext.Request.Headers["User-Agent"];
                //    var uaParser = Parser.GetDefault();
                //    ClientInfo c = uaParser.Parse(userAgent);
                //    string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                //    bool flag = AdminHelper.InsertLeaveTemp(IPAddress, SessionStaffNumber, BrowserName);

                //    if (flag)
                //    {
                //        TempData["LeaveInserted"] = "True";
                //    }
                //    else
                //    {
                //        TempData["LeaveInserted"] = "False";
                //    }

                //}
            }

            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return RedirectToAction("Home","User");
        }

        // half yearly update end

        // view break
        
        // insert permission monthly start

        public IActionResult InsertPermissionMonthly()
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
                else if (HttpContext.Session.GetString("Role") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                string Date = DateTime.Now.Day.ToString();
                string Month = DateTime.Now.Month.ToString();

                //if (Date == "1")
                if (true)
                {
                    // to get browser name
                    var userAgent = HttpContext.Request.Headers["User-Agent"];
                    var uaParser = Parser.GetDefault();
                    ClientInfo c = uaParser.Parse(userAgent);
                    string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                    bool flag = AdminHelper.InsertPermissionMonthly(IPAddress, SessionStaffNumber, BrowserName);

                    if (flag)
                    {
                        TempData["PermissionInserted"] = "True";
                    }
                    else
                    {
                        TempData["PermissionInserted"] = "False";
                    }
                }
            }

            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }
            return RedirectToAction("Home", "User");
        }

        // insert permission monthly end

        // view break

        // *************************************************************************************************//
        // Manage Leave & Permission updates end


        // Manage Shift Assigns start
        // *************************************************************************************************//

        // driver shift assign start

        [HttpGet]
        public IActionResult DriversShiftAssign()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List <DriversModel> model = new List <DriversModel> ();

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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = AdminHelper.GetDriversData(IPAddress, SessionStaffNumber);

                foreach(var driver in model)
                {
                    driver.DriversShift = AdminHelper.GetDriversShiftData(IPAddress, SessionStaffNumber);
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
        public IActionResult DriversShiftAssign(List<DriversModel> model)
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
                else if (HttpContext.Session.GetString("Role") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string Browser = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                bool IsInserted = AdminHelper.AssignDriversShift(model, SessionStaffNumber, IPAddress, Browser);

                if (IsInserted)
                {
                    TempData["SuccessMessage"] = "Shift assigned successfully";
                    return RedirectToAction("DriversShiftAssign", "Admin");
                }
                else
                {
                    TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                    return RedirectToAction("DriversShiftAssign", "Admin");
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

        // driver shift assign end

        // view break

        // driver shift start

        [HttpGet]
        public IActionResult DriversShift()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<DriversModel> model = new List<DriversModel>();

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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = AdminHelper.GetDriversShiftData(IPAddress, SessionStaffNumber);
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
        public IActionResult DriversShift(List<DriversModel> model)
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
                else if (HttpContext.Session.GetString("Role") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string Browser = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                bool IsInserted = AdminHelper.UpdateDriversShift(model, SessionStaffNumber, IPAddress, Browser);

                if (IsInserted)
                {
                    TempData["SuccessMessage"] = "Update successful";
                    return RedirectToAction("DriversShift", "Admin");
                }
                else
                {
                    TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                    return RedirectToAction("DriversShift", "Admin");
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

        // driver shift end

        // view break

        // print driver shift start

        public IActionResult PrintDriversShiftDetails()
        {
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<DriversModel> model = new List<DriversModel>();

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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = AdminHelper.GetDriversShifts(IPAddress, SessionStaffNumber);

                foreach (var driver in model)
                {
                    driver.DriversShift = AdminHelper.GetDriversShiftData(IPAddress, SessionStaffNumber);
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

        // print driver shift end

        // view break

        // security shift assign start
        [HttpGet]
        public IActionResult SecurityShiftAssign()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<SecurityModel> model = new List<SecurityModel>();

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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                //model = AdminHelper.GetSecurityData(IPAddress, SessionStaffNumber);

                for(int i=0;i<7;i++)
                {
                    var data = new SecurityModel();
                    model.Add(data);
                }
                foreach (var security in model)
                {
                    security.SecurityShift = AdminHelper.GetSecurityShiftData(IPAddress, SessionStaffNumber);
                    security.SecurityEmployees = AdminHelper.GetSecurityData(IPAddress, SessionStaffNumber);
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
        public IActionResult SecurityShiftAssign(List<SecurityModel> model)
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
                else if (HttpContext.Session.GetString("Role") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                // check if logged in user is admin or not
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string Browser = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                bool IsInserted = AdminHelper.AssignSecurityShift(model, SessionStaffNumber, IPAddress, Browser);

                if (IsInserted)
                {
                    TempData["SuccessMessage"] = "Shift assigned successfully";
                    return RedirectToAction("SecurityShiftAssign", "Admin");
                }
                else
                {
                    TempData["ExceptionError"] = "An error has occured. Please contact system administrator.";
                    return RedirectToAction("SecurityShiftAssign", "Admin");
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

        // security shift assign end

        // view break

        // print security shift start

        public IActionResult PrintSecurityShiftDetails()
        {
            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<SecurityModel> model = new List<SecurityModel>();

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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = AdminHelper.GetSecurityShifts(IPAddress, SessionStaffNumber);

                foreach (var security in model)
                {
                    security.SecurityShift = AdminHelper.GetSecurityShiftData(IPAddress, SessionStaffNumber);
                    security.SecurityEmployees = AdminHelper.GetSecurityData(IPAddress, SessionStaffNumber);
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

        // print security shift end

        // *********************************************************************************
        // temporary
        public IActionResult SecurityShiftAssignCopy()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            List<SecurityModel> model = new List<SecurityModel>();

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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = AdminHelper.GetSecurityData(IPAddress, SessionStaffNumber);

                foreach (var driver in model)
                {
                    driver.SecurityShift = AdminHelper.GetSecurityShiftData(IPAddress, SessionStaffNumber);
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

        // temporary
        // *********************************************************************************

        // *************************************************************************************************//
        // Manage Shift Assigns end


        // Manage Holidays start
        // *************************************************************************************************//
        
        // Add holidays start

        [HttpGet]
        public IActionResult AddHolidays()
        {

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

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

        [HttpPost]
        public IActionResult AddHolidays(HolidaysModel model)
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
                else if (HttpContext.Session.GetString("Role") == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("Logout", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                bool IsUpdated = AdminHelper.UpdateHolidayData(model, SessionStaffNumber, IPAddress);

                if (IsUpdated)
                {
                    TempData["SuccessMessage"] = "Holiday List Updated";
                    return RedirectToAction("AddHolidays", "Admin");
                }
                else
                {
                    TempData["ExceptionError"] = "An error has occurred. Please contact system administrator.";
                    return RedirectToAction("AddHolidays", "Admin");
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

        // Add holidays ends

        // *************************************************************************************************//
        // Manage Holidays end

        // View break

        // Manage Incharge start
        // *************************************************************************************************//

        // View incharge start

        public IActionResult ViewInchargeData()
        {

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

            var model = new List<InchargeModel>(); 

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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                model = AdminHelper.GetInchargeData(IPAddress, SessionStaffNumber);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(model);
        }

        // View incharge end

        // view break

        // Add incharge start

        [HttpGet]
        public IActionResult AddIncharge()
        {
            var groupmodel = new InchargeModel();

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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                groupmodel.Groups = AdminHelper.GetGroupData(SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(groupmodel);
        }

        [HttpPost]
        public IActionResult AddIncharge(InchargeModel model)
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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                int InchargeCount = AdminHelper.GetInchargeCount(SessionStaffNumber, IPAddress);

                model.InchargeID = "INC" + DateTime.Now.ToString("ddMMyy") + InchargeCount;

                bool IsUpdated = AdminHelper.UpdateInchargeData(model, SessionStaffNumber, IPAddress, BrowserName);

                if (IsUpdated)
                {
                    TempData["SuccessMessage"] = "Update successful";
                    return RedirectToAction("ViewInchargeData", "Admin");
                }
                else
                {
                    TempData["ExceptionError"] = "An error has occurred. Please contact system administrator.";
                    return RedirectToAction("ViewInchargeData", "Admin");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(model);
        }

        // Add incharge end

        // view break

        // Edit incharge start

        [HttpGet]
        public IActionResult EditIncharge(string InchargeID)
        {

            // show success message is there is any
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            TempData["SuccessMessage"] = "";

            // Error Message
            ViewBag.ExceptionError = TempData["ExceptionError"];
            TempData["ExceptionError"] = "";

            InchargeModel incharge = new InchargeModel();

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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                incharge = AdminHelper.GetInchargeDataForEdit(InchargeID, SessionStaffNumber, IPAddress);
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(incharge);
        }

        [HttpPost]
        public IActionResult EditIncharge(InchargeModel model)
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
                else if (HttpContext.Session.GetString("Role") != "Admin")
                {
                    return RedirectToAction("AccessDenied", "User");
                }

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                // to get browser name
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var uaParser = Parser.GetDefault();
                ClientInfo c = uaParser.Parse(userAgent);
                string BrowserName = c.UA.Family + " " + c.UA.Major + "." + c.UA.Minor;

                bool IsUpdated = AdminHelper.UpdateInchargeData(model, SessionStaffNumber, IPAddress, BrowserName);

                if (IsUpdated)
                {
                    TempData["SuccessMessage"] = "Update successful";
                    return RedirectToAction("ViewInchargeData", "Admin");
                }
                else
                {
                    TempData["ExceptionError"] = "An error has occurred. Please contact system administrator.";
                    return RedirectToAction("ViewInchargeData", "Admin");
                }
            }
            catch (Exception error)
            {
                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");
                ViewBag.ExceptionError = "An error has occurred. Please contact system administrator";
                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
            }

            return View(model);
        }

        // Edit incharge end

        // *************************************************************************************************//
        // Manage incharge end

        // view break
    }
}
