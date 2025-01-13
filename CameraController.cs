using Digitalization.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Digitalization.Controllers
{
    public class CameraController : Controller
    {
        private readonly IWebHostEnvironment _hostingenvironment;

        public CameraController(IWebHostEnvironment hostingenvironment)
        {
            _hostingenvironment = hostingenvironment;
        }

        public IActionResult Capture(IFormFile webcam, int value)
        { 
            try
            {
                var configuration = CommonHelper.GetConfig();
                var loglocation = configuration["FileLocations:VisitorPassTempFileLocation"];
                string folderName = loglocation;
                folderName = folderName.Replace("Year", DateTime.Now.Year.ToString());
                folderName = folderName.Replace("Month", DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture));
                folderName = folderName.Replace("SlNo", value.ToString());
                //string uploadPath = Server.MapPath(folderName);
                string webRootPath = _hostingenvironment.ContentRootPath;
                string newPath = Path.Combine(webRootPath, folderName);
                if (!Directory.Exists(folderName))
                {
                    Directory.CreateDirectory(folderName);
                }
                string TemporaryFileName = webcam.FileName;
                string Extension = TemporaryFileName.Split('.').Last();
                string FileName = value + "." + Extension;
                string FinalPath = Path.Combine(folderName, FileName);
                string envpath = folderName + "\\" + FileName;

                using (var stream = new FileStream(FinalPath, FileMode.Create))
                {
                    webcam.CopyTo(stream);
                }

                ViewBag.FinalPath = FinalPath;
            }
            catch (Exception error)
            {
                string IPAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();

                int SessionStaffNumber = (int)HttpContext.Session.GetInt32("StaffNumber");

                CommonHelper.LogError(error, SessionStaffNumber, IPAddress);
                Console.WriteLine(error.Message);
                ViewBag.ExceptionError = "An error has occured. Please contact system administrator.";
            }
            // end
            //TempData["CameraCaptured"] = "TRUE";
            return RedirectToAction("RaiseVisitorPass", "User");
            //return View();
        }
    }
}
