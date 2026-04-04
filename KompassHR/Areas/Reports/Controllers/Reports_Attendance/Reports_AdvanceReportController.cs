using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_Attendance
{
    public class Reports_AdvanceReportController : Controller
    {
        #region Main View
        // GET: Reports/Reports_AdvanceReport
        public ActionResult Reports_AdvanceReport()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 894;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                var EmployeeId = Convert.ToInt32(Session["EmployeeId"]);
                // var EmployeeId = 5068;
                param.Add("@p_EmployeeId", EmployeeId);
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Reports_AdvanceReport", param).ToList();
                ViewBag.ListDetails = data;
                return View();
            }
            catch(Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
        
        #region  View Advance Details
        public ActionResult ViewAdvanceDetails(int AdvanceID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_AdvanceID", AdvanceID);
                var data = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Reports_ViewAdvanceDetails", param).ToList();
                ViewBag.EmployeeData = data;
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