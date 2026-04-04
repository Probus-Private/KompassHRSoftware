using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.App.Controllers.App_Attendance
{
    public class App_Attendance_AttendanceSubMenuController : Controller
    {
        // GET: App/App_Attendance_AttendanceSubMenu
        public ActionResult App_Attendance_AttendanceSubMenu()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
    }
}