using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Leave;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Data;

namespace KompassHR.Areas.ESS.Controllers.ESS_Leave
{
    public class ESS_Leave_CoffMonthlyCreditController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_Leave_CoffMonthlyCredit
        #region Main Form
        public ActionResult ESS_Leave_CoffMonthlyCredit(CoffMonthlyCredit obj)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            // CHECK IF USER HAS ACCESS OR NOT
            int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 775;
            bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
            if (!CheckAccess)
            {
                Session["AccessCheck"] = "False";
                return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
            }

            DynamicParameters paramCompany = new DynamicParameters();
            paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
            var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
            ViewBag.ComapnyName = GetComapnyName;
            var GetCmpId = GetComapnyName[0]?.Id;

            DynamicParameters paramBranch = new DynamicParameters();
            paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
            paramBranch.Add("@p_CmpId", GetCmpId);
            var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
            ViewBag.BranchName = BranchName;

            if (obj.BranchId!=0)
            {
                int Month = 0;
                int Year = 0;
                if (obj.MonthYear.HasValue)   // ✅ check null
                {
                     Month = obj.MonthYear.Value.Month;  // ✅ safe
                     Year = obj.MonthYear.Value.Year;    // ✅ safe
                }
                DynamicParameters paramBranch1 = new DynamicParameters();
                paramBranch1.Add("@p_employeeid", Session["EmployeeId"]);
                paramBranch1.Add("@p_CmpId", obj.CmpId);
                var BranchNames = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch1).ToList();
                ViewBag.BranchName = BranchNames;

                DynamicParameters EmpDetails = new DynamicParameters();
                EmpDetails.Add("@p_CmpId", obj.CmpId);
                EmpDetails.Add("@p_BranchId", obj.BranchId);
                EmpDetails.Add("@p_Month", Month);
                EmpDetails.Add("@p_Year", Year);
                var GetEmpDetails = DapperORM.ExecuteSP<dynamic>("sp_Coff_List_MonthlyCredit_Show", EmpDetails).ToList();
                ViewBag.GetEmployeeList = GetEmpDetails;
            }

            return View(obj);
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
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                return Json(new { data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Save Update
        public ActionResult SaveUpadte(double companyId, double BranchId, int Month, int Year, List<CoffMonthlyCreditList> CoffMonthlyCreditList)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 775;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                var AttenLockCount = DapperORM.DynamicQuerySingle("Select Count(AtdLockIDBranchId) as LockCount from Atten_Lock where Deactivate=0  and Month(AtdLockIDMonth)='" + Month + "'  and Year(AtdLockIDMonth) ='" + Year + "'  and AtdLockIDBranchId=" + BranchId + " and AtdLock=1");
                if (AttenLockCount.LockCount <= 0)
                {
                    TempData["Message"] = "The record can't be saved because the attendance for this month ('" + Month + '/'+ Year + "') is not locked.";
                    TempData["Icon"] = "error";
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }

                for (var i = 0; i < CoffMonthlyCreditList.Count; i++)
                {
                    var item = CoffMonthlyCreditList[i];

                    param.Add("@p_CmpId", companyId);
                    param.Add("@p_BranchId", BranchId);
                    param.Add("@p_EmployeeId", CoffMonthlyCreditList[i].EmployeeId);
                    param.Add("@p_Month", Month);
                    param.Add("@p_Year", Year);
                    param.Add("@p_ApprovedCoff", CoffMonthlyCreditList[i].ApprovedCoff);
                    param.Add("@p_ApproveRejectBy", Session["EmployeeId"]);
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_Coff_Save_MonthlyCredit", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                }
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList Main View 
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 775;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "ESS" });
                }
                DynamicParameters param = new DynamicParameters();
                var data = DapperORM.ExecuteSP<dynamic>("sp_Coff_List_Leave_MonthlyCredit", param).ToList();
                ViewBag.GetMonthlyCreditList = data;

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