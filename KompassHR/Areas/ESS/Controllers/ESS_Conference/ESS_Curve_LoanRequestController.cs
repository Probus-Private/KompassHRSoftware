using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Conference
{
    public class ESS_Curve_LoanRequestController : Controller
    {
        // GET: ESS/ESS_Curve_LoanRequest
        public ActionResult ESS_Curve_LoanRequest()
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            // CHECK IF USER HAS ACCESS OR NOT
            int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 676;
            bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
            if (!CheckAccess)
            {
                Session["AccessCheck"] = "False";
                return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
            }

            var GetDocNo = "Select isnull(Max(DocNo),0)+1 As DocNo from Payroll_Loan";
            var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
            ViewBag.DocNo = DocNo;
            return View();
        }
    }
}