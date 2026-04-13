using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_Attendance
{
    public class Reports_LoanReportController : Controller
    {
        #region Main View
        // GET: Reports/Reports_LoanReport
        public ActionResult Reports_LoanReport()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 892;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                 var EmployeeId = Convert.ToInt32(Session["EmployeeId"]);
               // var EmployeeId = 4888;
                param.Add("@p_EmployeeId", EmployeeId);
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Reports_LoanReport", param).ToList();
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


        #region ViewLoanDetails
        public ActionResult ViewLoanDetails(int LoanID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_LoanID", LoanID);
                var data = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Reports_ViewLoanDetails", param).ToList();
                ViewBag.EmployeeData = data;
                return View();
            }
            catch(Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
    }
}