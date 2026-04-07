using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers
{
    public class Setting_ProfileController : Controller
    {
        // GET: Setting/Setting_Profile
        public ActionResult Setting_Profile()
        {
            return View();
        }
    }
    public class Setting_SecurityController : Controller
    {
        // GET: Setting/Setting_Security
        public ActionResult Setting_Security()
        {
            return View();
        }
    }
}