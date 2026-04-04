using Dapper;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_TimeOffice
{
    public class ESS_TimeOffice_Pending_AttenRegularizationController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_TimeOffice_Pending_AttenRegularization
        #region Pending_AttenRegularization
        public ActionResult ESS_TimeOffice_Pending_AttenRegularization(DailyAttendanceReportFilter Daily_AttendanceReportFilter)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@query", "select CompanyId as Id ,CompanyName as Name from Mas_CompanyProfile where Deactivate = 0");
                var CompanyName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramList).ToList();
                ViewBag.GetCompanyName = CompanyName;

                var CMPID = CompanyName[0].Id;
                DynamicParameters paramBranch1 = new DynamicParameters();
                paramBranch1.Add("@p_employeeid", Session["EmployeeId"]);
                paramBranch1.Add("@p_CmpId", CMPID);
                var BranchName1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch1).ToList();
                ViewBag.GetBranchName = BranchName1;

                if (Daily_AttendanceReportFilter.BranchId != null)
                {
                    param = new DynamicParameters();
                    param.Add("@p_BranchID", Daily_AttendanceReportFilter.BranchId);
                    param.Add("@p_FromDate", Daily_AttendanceReportFilter.FromDate);
                    param.Add("@p_ToDate", Daily_AttendanceReportFilter.ToDate);
                    var data = DapperORM.ExecuteSP<dynamic>("sp_PendingAttendance", param).ToList();
                    ViewBag.PendingAttenRegularization = data;

                    DynamicParameters paramBranch = new DynamicParameters();
                    paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBranch.Add("@p_CmpId", Daily_AttendanceReportFilter.CmpId);
                    var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                    ViewBag.GetBranchName = BranchName;
                }
                else
                {
                    ViewBag.PendingAttenRegularization = "";
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

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region PendingEmployeeDetails
        public ActionResult PendingEmployeeDetails(DateTime AttendanceDate, int? BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                //var FromDateGet = Daily_AttendanceReportFilter.FromDate;
                var AttenFromDate = AttendanceDate.ToString("yyyy-MM-dd");
                param.Add("@p_date", AttenFromDate);
                param.Add("@p_BranchID", BranchId);
                var GetPendingAttedanceList = DapperORM.ExecuteSP<dynamic>("sp_PendingAttedanceList", param).ToList();
                ViewBag.PendingAttedanceList = GetPendingAttedanceList;
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