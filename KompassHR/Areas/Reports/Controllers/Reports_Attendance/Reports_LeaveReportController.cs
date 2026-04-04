using Dapper;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_Attendance
{

    public class Reports_LeaveReportController : Controller
    {
        #region Main View
        // GET: Reports/Reports_LeaveReport
        public ActionResult Reports_LeaveReport(int? LeaveId, int? YearId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 899;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                LeaveReportFilter LeaveReportFilter = new LeaveReportFilter();
                var EmployeeId = Session["EmployeeId"];

                DynamicParameters paramYear = new DynamicParameters();
                //paramYear.Add("@query", "select Distinct Fid as Id,AssessmentYear as Name from Tool_FinancialYear;");
                paramYear.Add("@query", "select LeaveYearID as Id, cast(year(FromDate) as nvarchar(4))+'-'+cast(YEAR(ToDate) as nvarchar(4)) as Name from[dbo].[Leave_Year] where Deactivate = 0  and IsActivate=1  order by IsDefault desc,FromDate desc");
                var YearList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramYear).ToList();
                ViewBag.YearName = YearList;

                ViewBag.Getdata = null;
                ViewBag.IsSearch = false;

                DynamicParameters paramLeave = new DynamicParameters();
                paramLeave.Add("@query", "select Distinct LeaveTypeId as Id,LeaveTypeShortName as Name from Leave_Type where Deactivate=0 Order by Name;");
                var LeaveList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLeave).ToList();
                ViewBag.LeaveName = LeaveList;

                if (YearId != null && LeaveId != null)
                {
                    ViewBag.IsSearch = true;

                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", EmployeeId);
                    paramList.Add("@p_YearId", YearId);
                    paramList.Add("@p_LeaveId", LeaveId);

                    var data = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Reports_LeaveReport", paramList).ToList();
                    ViewBag.Getdata = data;
                }
              
                    return View(LeaveReportFilter);
              
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