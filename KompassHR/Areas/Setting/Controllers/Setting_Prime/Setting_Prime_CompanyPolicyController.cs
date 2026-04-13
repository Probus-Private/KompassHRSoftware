using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Prime
{
    public class Setting_Prime_CompanyPolicyController : Controller
    {
        // GET: Setting/Setting_Prime_CompanyPolicy
        public ActionResult Setting_Prime_CompanyPolicy()
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            return View();
        }
    }
}