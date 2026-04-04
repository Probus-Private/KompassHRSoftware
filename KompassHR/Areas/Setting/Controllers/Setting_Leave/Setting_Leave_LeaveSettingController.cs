using Dapper;
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

namespace KompassHR.Areas.Setting.Controllers.Setting_Leave
{
    public class Setting_Leave_LeaveSettingController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: LeaveSetting/LeaveSetting
        #region LeaveSetting Main view
        [HttpGet]
        public ActionResult Setting_Leave_LeaveSetting(string LeaveSettingId_Encrypted,int? CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 48;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Leave_Setting Leave_Setting = new Leave_Setting();

                var GetIsMonthlyCreadit = DapperORM.DynamicQuerySingle("Select IsMonthlyCreadit from Tool_CommonTable");
                TempData["CheckIsMonthlyCreadit"] = GetIsMonthlyCreadit?.IsMonthlyCreadit;


                if (CmpId!=null)
                {
                    DynamicParameters paramLeave = new DynamicParameters();
                    paramLeave.Add("@query", "select LeaveGroupId as Id, LeaveGroupName As Name from Leave_Group where Deactivate = 0 and CmpId=" + CmpId + "  order by Name");
                    var Leave_Groups = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLeave).ToList();                   
                    ViewBag.LeaveGroups = Leave_Groups;
                }
                else
                {
                    ViewBag.LeaveGroups = "";
                }
               

                param.Add("@query", "select LeaveTypeId,LeaveTypeName  from Leave_Type where Deactivate=0 and IsActive =1");
                var LeaveType = DapperORM.ReturnList<leave_type>("sp_QueryExcution", param).ToList();
                ViewBag.LeaveType = LeaveType;

                param.Add("@query", "Select CompanyId As Id,CompanyName As Name from Mas_CompanyProfile Where Deactivate=0");
                var listMas_CompanyProfile = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetCompanyname = listMas_CompanyProfile;
                if (LeaveSettingId_Encrypted != null)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    paramList.Add("@P_LeaveSettingID_Encrypted", LeaveSettingId_Encrypted);
                    Leave_Setting = DapperORM.ReturnList<Leave_Setting>("sp_List_Leave_LeaveSetting", paramList).FirstOrDefault();
                }
                ViewBag.LeaveSetting = Leave_Setting;
                return View(Leave_Setting);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsLeaveSettingExists
        [HttpGet]
        public ActionResult IsLeaveSettingExists(double LeaveGroup, double LeaveTypeName, string LeaveSettingId_Encrypted)
        {
            try
            {
                    if (Session["EmployeeId"] == null)
                    {
                        return RedirectToAction("Login", "Login", new { Area = "" });
                    }
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_LeaveSettingId_Encrypted", LeaveSettingId_Encrypted);
                    param.Add("@p_LeaveSettingLeaveGroupId ", LeaveGroup);
                    param.Add("@p_LeaveSettingLeaveTypeId", LeaveTypeName);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Leave_Setting", param);
                    var Message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    if (Message != "")
                    {
                        return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }              
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Leave_Setting Leavesetting)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(Leavesetting.LeaveSettingId_Encrypted) ? "Save" : "Update");
                param.Add("@p_LeaveSettingId", Leavesetting.LeaveSettingId);
                param.Add("@P_LeaveSettingID_Encrypted", Leavesetting.LeaveSettingId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_LeaveSettingLeaveGroupId", Leavesetting.LeaveSettingLeaveGroupId);
                param.Add("@p_LeaveSettingLeaveTypeId", Leavesetting.LeaveSettingLeaveTypeId);
                param.Add("@p_YearlyLeave", Leavesetting.YearlyLeave);
                param.Add("@p_IsMonthlyLimit", Leavesetting.IsMonthlyLimit);
                param.Add("@p_CarryforwardBalanceOrMax", Leavesetting.CarryforwardBalanceOrMax);
                param.Add("@p_IsAfterConfirmation", Leavesetting.IsAfterConfirmation);
                param.Add("@p_CarryforwardMaxLeave", Leavesetting.CarryforwardMaxLeave);
                param.Add("@p_MonthlyMaxLeave", Leavesetting.MonthlyMaxLeave);
                param.Add("@p_IsCarryforward", Leavesetting.IsCarryforward);
                param.Add("@p_CmpId", Leavesetting.CmpId);
                param.Add("@p_Color", "red");
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_IsEncashment", Leavesetting.IsEncashment);
                param.Add("@p_EncashmentdBalanceOrMax", Leavesetting.EncashmentdBalanceOrMax);
                param.Add("@p_EncashmentMaxLeave", Leavesetting.EncashmentMaxLeave);
                param.Add("@p_IsDocRequired", Leavesetting.IsDocRequired);
                param.Add("@p_IsSandwich", Leavesetting.IsSandwich);
                param.Add("@p_IsAfterDays", Leavesetting.IsAfterDays);
                param.Add("@p_IsAutoApproval", Leavesetting.IsAutoApproval);
                param.Add("@p_IsAutoApprovalAfterDays", Leavesetting.IsAutoApprovalAfterDays);
                param.Add("@p_IsActivate", Leavesetting.IsActivate);
                param.Add("@p_BackDateDays", Leavesetting.BackDateDays);
                param.Add("@p_FutureDateDays", Leavesetting.FutureDateDays);
                param.Add("@p_IsHalfDayAllow", Leavesetting.IsHalfDayAllow);
                param.Add("@p_MinLeave", Leavesetting.MinLeave);
                param.Add("@p_MaxLeave", Leavesetting.MaxLeave);
                param.Add("@p_IsMonthlyCreadit", Leavesetting.IsMonthlyCreadit);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Leave_Setting", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Leave_LeaveSetting", "Setting_Leave_LeaveSetting");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 48;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_LeaveSettingID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Leave_LeaveSetting", param);
                ViewBag.LeaveSetting = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(string LeaveSettingId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@P_LeaveSettingID_Encrypted", LeaveSettingId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.DynamicList("sp_SUD_Leave_Setting", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Leave_LeaveSetting");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetLeaveGroup
        [HttpGet]
        public ActionResult GetLeaveGroup(int CmpId)
        {
            try
            {
                DynamicParameters paramLeave = new DynamicParameters();
                paramLeave.Add("@query", "select LeaveGroupId as Id, LeaveGroupName As Name from Leave_Group where Deactivate = 0 and CmpId="+ CmpId + "  order by Name");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLeave).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
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