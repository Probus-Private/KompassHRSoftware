using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Leave;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Leave
{
    public class ESS_Leave_MonthlyCreditController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_Leave_MonthlyCredit
        #region Main View
        public ActionResult ESS_Leave_MonthlyCredit()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 709;
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
                double? companyId = GetComapnyName.FirstOrDefault()?.Id; // assuming Id is int

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@query", $@"Select Leave_Setting.LeaveSettingId as Id ,concat(Leave_Group.LeaveGroupName, ' - ', Leave_Type.LeaveTypeShortName) as Name from Leave_Setting, Leave_Group, Leave_Type where Leave_Setting.Deactivate = 0 and Leave_Group.Deactivate = 0 and Leave_Type.Deactivate = 0 and Leave_Setting.LeaveSettingLeaveGroupId = Leave_Group.LeaveGroupId and Leave_Type.LeaveTypeId = Leave_Setting.LeaveSettingLeaveTypeId and Leave_Setting.CmpId={companyId} and IsMonthlyCreadit=1");
                var LeaveType = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();
                ViewBag.LeaveTypeName = LeaveType;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
        public ActionResult GetList(LeaveMonthlyCredit Obj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 709;
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
                double? companyId = GetComapnyName.FirstOrDefault()?.Id; // assuming Id is int

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@query", $@"Select Leave_Setting.LeaveSettingId as Id ,concat(Leave_Group.LeaveGroupName, ' - ', Leave_Type.LeaveTypeShortName) as Name from Leave_Setting, Leave_Group, Leave_Type where Leave_Setting.Deactivate = 0 and Leave_Group.Deactivate = 0 and Leave_Type.Deactivate = 0 and Leave_Setting.LeaveSettingLeaveGroupId = Leave_Group.LeaveGroupId and Leave_Type.LeaveTypeId = Leave_Setting.LeaveSettingLeaveTypeId and Leave_Setting.CmpId={companyId} and IsMonthlyCreadit=1");
                var LeaveType = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();
                ViewBag.LeaveTypeName = LeaveType;



                DateTime monthYear = Obj.MonthYear ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                DynamicParameters param3 = new DynamicParameters();
                param3.Add("@p_Process", "List");
                param3.Add("@p_MonthYear", monthYear);
                param3.Add("@p_CmpId", Obj.CmpId);
                var OpneingBalancelist = DapperORM.DynamicList("sp_List_Leave_MonthlyCredit", param3);
                ViewBag.OpneingBalancelist = OpneingBalancelist;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region SaveUpdate
        public ActionResult SaveUpdate(LeaveMonthlyCredit Obj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 709;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "Save");
                param.Add("@p_CmpID", Obj.CmpId);
                param.Add("@p_LeaveSettingId", Obj.LeaveSettingId);
                param.Add("@p_MonthYear", Obj.MonthYear);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@P_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@P_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Leave_MonthlyCredit", param);
                TempData["Message"] = param.Get<string>("@P_msg");
                TempData["Icon"] = param.Get<string>("@P_Icon");

                return RedirectToAction("ESS_Leave_MonthlyCredit", "ESS_Leave_MonthlyCredit");
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
        public ActionResult GetLeaveType(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@query", $@"Select Leave_Setting.LeaveSettingId as Id ,concat(Leave_Group.LeaveGroupName, ' - ', Leave_Type.LeaveTypeShortName) as Name from Leave_Setting, Leave_Group, Leave_Type where Leave_Setting.Deactivate = 0 and Leave_Group.Deactivate = 0 and Leave_Type.Deactivate = 0 and Leave_Setting.LeaveSettingLeaveGroupId = Leave_Group.LeaveGroupId and Leave_Type.LeaveTypeId = Leave_Setting.LeaveSettingLeaveTypeId and Leave_Setting.CmpId={CmpId} and IsMonthlyCreadit=1 "); //and IsMonthlyCreadit=1
                var LeaveType = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();
                return Json(new { LeaveType = LeaveType }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete  
        public ActionResult Delete(int LeaveOpeningBalanceId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 709;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_LeaveOpeningBalanceId", LeaveOpeningBalanceId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Leave_MonthlyCredit", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_Leave_MonthlyCredit");
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