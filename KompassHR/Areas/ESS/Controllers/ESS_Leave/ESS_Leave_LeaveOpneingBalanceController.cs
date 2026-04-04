using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Leave;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Setting.Models.Setting_Leave;
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
    public class ESS_Leave_LeaveOpneingBalanceController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_Leave_LeaveOpneingBalance
        #region Main View Page
        public ActionResult ESS_Leave_LeaveOpneingBalance(Leave_OpeningBalance LeaveOpeningBalance)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 148;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Leave_OpeningBalance LeaveOpeningBalances = new Leave_OpeningBalance();

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.ComapnyName = GetComapnyName;

                if (LeaveOpeningBalance.CmpID != 0)
                {
                    param = new DynamicParameters();
                    param.Add("@p_Process", "List");
                    param.Add("@p_CmpID", LeaveOpeningBalance.CmpID);
                    param.Add("@p_EmployeeBranchId", LeaveOpeningBalance.LeaveOpeningBalanceBranchID);
                    param.Add("@p_LeaveSettingId", LeaveOpeningBalance.LeaveOpeningBalanceLeaveTypeId);
                    param.Add("@p_LeaveYearId", LeaveOpeningBalance.LeaveOpeningBalanceLeaveYearId);
                    var GetLeaveOpneingBalanceEmployee = DapperORM.ExecuteSP<dynamic>("sp_List_GetLeaveOpneingBalance", param).ToList();
                    ViewBag.LeaveOpneingBalanceEmployee = GetLeaveOpneingBalanceEmployee;

                    DynamicParameters paramBranch = new DynamicParameters();
                    paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBranch.Add("@p_CmpId", LeaveOpeningBalance.CmpID);
                    var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                    ViewBag.BranchName = BranchName;

                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@query", "select LeaveYearID as Id, cast(year(FromDate) as nvarchar(4))+'-'+cast(YEAR(ToDate) as nvarchar(4)) as Name from[dbo].[Leave_Year] where Deactivate = 0 and IsActivate=1  and CmpId=" + LeaveOpeningBalance.CmpID + " order by IsDefault desc,FromDate desc");
                    var LeaveYear = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();
                    ViewBag.GetLeaveYear = LeaveYear;

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@query", "select Leave_Setting.LeaveSettingId as Id ,concat(Leave_Group.LeaveGroupName, ' - ', Leave_Type.LeaveTypeShortName) as Name from Leave_Setting, Leave_Group, Leave_Type where Leave_Setting.Deactivate = 0 and Leave_Group.Deactivate = 0 and Leave_Type.Deactivate = 0 and Leave_Setting.LeaveSettingLeaveGroupId = Leave_Group.LeaveGroupId and Leave_Type.LeaveTypeId = Leave_Setting.LeaveSettingLeaveTypeId and Leave_Setting.CmpId=" + LeaveOpeningBalance.CmpID + "");
                    var LeaveType = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();
                    ViewBag.LeaveTypeName = LeaveType;

                }
                else
                {
                    ViewBag.LeaveOpneingBalanceEmployee = "";
                    ViewBag.BranchName = "";
                    ViewBag.LeaveTypeName = "";
                    ViewBag.GetLeaveYear = "";
                }
                return View(LeaveOpeningBalances);
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
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "select LeaveYearID as Id, cast(year(FromDate) as nvarchar(4))+'-'+cast(YEAR(ToDate) as nvarchar(4)) as Name from[dbo].[Leave_Year] where Deactivate = 0  and CmpId=" + CmpId + " order by IsDefault desc,FromDate desc");
                var LeaveYear = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@query", "select Leave_Setting.LeaveSettingId as Id ,concat(Leave_Group.LeaveGroupName, ' - ', Leave_Type.LeaveTypeShortName) as Name from Leave_Setting, Leave_Group, Leave_Type where Leave_Setting.Deactivate = 0 and Leave_Group.Deactivate = 0 and Leave_Type.Deactivate = 0 and Leave_Setting.LeaveSettingLeaveGroupId = Leave_Group.LeaveGroupId and Leave_Type.LeaveTypeId = Leave_Setting.LeaveSettingLeaveTypeId and Leave_Setting.CmpId=" + CmpId + "");
                var LeaveType = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();
                return Json(new { data = data, LeaveYear = LeaveYear, LeaveType = LeaveType }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        //#region Get Employee Details on Show Button
        //public ActionResult GetEmployeeDetiles(string CompanyId ,string BranchId, int LeaveSettingId)
        //{
        //    try
        //    {
        //        param = new DynamicParameters();             
        //        param.Add("@p_CmpID", CompanyId);
        //        param.Add("@p_EmployeeBranchId", BranchId);
        //        param.Add("@p_LeaveSettingId", LeaveSettingId);
        //        var data = DapperORM.ReturnList<dynamic>("sp_List_GetLeaveOpneingBalance", param).ToList();
        //        //var data = DapperORM.ReturnList<dynamic>("sp_List_LeaveOpneingBalance", param).ToList();
        //        return Json(data, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return RedirectToAction(ex.Message.ToString(), "ESS_Leave_LeaveOpneingBalance");
        //    }


        //}
        //#endregion

        #region Save Update
        public ActionResult SaveUpadte(double companyId, double BranchId, double LeaveSettingId, double LeaveYearId, List<Leave_OpeningBalance> LeaveOpeningBalance)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 148;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                for (var i = 0; i < LeaveOpeningBalance.Count; i++)
                {
                    param.Add("@p_process", "Save");
                    param.Add("@p_CmpID", companyId);
                    param.Add("@p_LeaveOpeningBalanceBranchID", BranchId);
                    param.Add("@p_LeaveOpeningBalanceSettingId", LeaveSettingId);
                    param.Add("@p_LeaveOpeningBalanceLeaveYearId", LeaveYearId);
                    param.Add("@p_LeaveOpeningBalanceId_Encrypted", LeaveOpeningBalance[i].LeaveOpeningBalanceId_Encrypted);
                    param.Add("@p_NoOfLeave", LeaveOpeningBalance[i].NoOfLeave);
                    param.Add("@p_LeaveOpeningBalanceEmployeeId", LeaveOpeningBalance[i].LeaveOpeningBalanceEmployeeId);
                    param.Add("@p_OpeningAdjCarryCredit_Remark", "Opening Balance");
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_SUD_LeaveOpneingBalance", param);
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

        #region GetList
        public ActionResult GetList(Leave_OpeningBalance LeaveOpeningBalance)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 148;
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

                ViewBag.BranchName = "";
                ViewBag.LeaveTypeName = "";
                ViewBag.GetLeaveYear = "";
                ViewBag.OpneingBalancelist = "";
                if (LeaveOpeningBalance.CmpID != 0)
                {
                    DynamicParameters paramBU = new DynamicParameters();
                    paramBU.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBU.Add("@p_CmpId", LeaveOpeningBalance.CmpID);
                    ViewBag.BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBU).ToList();

                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@query", "select LeaveYearID as Id, cast(year(FromDate) as nvarchar(4))+'-'+cast(YEAR(ToDate) as nvarchar(4)) as Name from[dbo].[Leave_Year] where Deactivate = 0  and CmpId=" + LeaveOpeningBalance.CmpID + " order by IsDefault desc,FromDate desc");
                    ViewBag.GetLeaveYear = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@query", "select Leave_Setting.LeaveSettingId as Id ,concat(Leave_Group.LeaveGroupName, ' - ', Leave_Type.LeaveTypeShortName) as Name from Leave_Setting, Leave_Group, Leave_Type where Leave_Setting.Deactivate = 0 and Leave_Group.Deactivate = 0 and Leave_Type.Deactivate = 0 and Leave_Setting.LeaveSettingLeaveGroupId = Leave_Group.LeaveGroupId and Leave_Type.LeaveTypeId = Leave_Setting.LeaveSettingLeaveTypeId and Leave_Setting.CmpId=" + LeaveOpeningBalance.CmpID + "");
                    ViewBag.LeaveTypeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();

                    DynamicParameters param3 = new DynamicParameters();
                    param3.Add("@p_LeaveOpeningBalanceId_Encrypted", "List");
                    param3.Add("@p_BranchId", LeaveOpeningBalance.LeaveOpeningBalanceBranchID);
                    param3.Add("@p_LeaveYearId", LeaveOpeningBalance.LeaveOpeningBalanceLeaveYearId);
                    param3.Add("@p_LeaveTypeId", LeaveOpeningBalance.LeaveOpeningBalanceLeaveTypeId);
                    var OpneingBalancelist = DapperORM.DynamicList("sp_List_LeaveOpneingBalance", param3);
                    ViewBag.OpneingBalancelist = OpneingBalancelist;
                }
                return View(LeaveOpeningBalance);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        [HttpGet]
        public ActionResult Delete(string LeaveOpeningBalanceId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 148;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_LeaveOpeningBalanceId_Encrypted", LeaveOpeningBalanceId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_LeaveOpneingBalance", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_Leave_LeaveOpneingBalance");
                //return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete All Record
        [HttpGet]
        public ActionResult DeleteAllRecored(string EncrptedIds)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 148;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                string[] splitdEncrptedId = EncrptedIds.Split(',');
                for (int i = 0; i < splitdEncrptedId.Count(); i++)
                {
                    param.Add("@p_process", "Delete");
                    param.Add("@p_LeaveOpeningBalanceId_Encrypted", splitdEncrptedId[i]);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_LeaveOpneingBalance", param);
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

    }
}