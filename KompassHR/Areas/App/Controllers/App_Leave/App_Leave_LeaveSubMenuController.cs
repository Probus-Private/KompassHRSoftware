using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.App.Controllers.App_Leave
{
    public class App_Leave_LeaveSubMenuController : Controller
    {
        // GET: App/App_Leave_LeaveSubMenu
        public ActionResult App_Leave_LeaveSubMenu()
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