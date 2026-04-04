using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Mobile.Controllers
{
    public class MobileDashboardController : Controller
    {
        // GET: Mobile/MobileDashboard
        public ActionResult MobileDashboard()
        {
            Session["EmployeeId"] = 5068;
            return View();
        }

        public ActionResult ShowMoreDashboard()
        {
            Session["EmployeeId"] = 5068;
            return View();
        }
    }
}