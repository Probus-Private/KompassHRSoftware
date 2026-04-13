using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_Onboarding
{
    public class Reports_Onboarding_Record_DetailController : Controller
    {
     
        #region Main View
        // GET: Reports/Reports_Onboarding_Record_Detail
        public ActionResult Reports_Onboarding_Record_Detail()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 778;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
               // param.Add("@p_EmployeeId", Session["EmployeeId"]);
                //var GetEmployeeParams = new DynamicParameters();
                var GetData = DapperORM.ExecuteSP<dynamic>("sp_List_GetRecordDetails").ToList();
                ViewBag.GetDetailsList = GetData;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
           
    }
}