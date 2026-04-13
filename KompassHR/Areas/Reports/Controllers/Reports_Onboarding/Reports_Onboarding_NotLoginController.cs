using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_Onboarding
{
    public class Reports_Onboarding_NotLoginController : Controller
    {
        #region Main View 
        // GET: Reports/Reports_Onboarding_NotLogin
        public ActionResult Reports_Onboarding_NotLogin(int?day)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 785;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.GetLoginList = null;

                if (day != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_day", day);
                    //var GetData = DapperORM.ExecuteSP<dynamic>("sp_List_GetLoginEmployee",param).ToList();
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_List_GetNotLoginEmployee", param).ToList();
                    ViewBag.GetLoginList = GetData;
                }
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