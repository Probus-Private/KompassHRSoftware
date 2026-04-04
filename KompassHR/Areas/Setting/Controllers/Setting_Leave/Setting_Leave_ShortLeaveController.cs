using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_Leave;
using KompassHR.Areas.Setting.Models.Setting_AccountAndFinance;
using Dapper;
using System.Data;
using KompassHR.Models;
using System.Net;
using System.Data.SqlClient;

namespace KompassHR.Areas.Setting.Controllers.Setting_Leave
{
    public class Setting_Leave_ShortLeaveController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region ShortLeave MAin View
        [HttpGet]
        public ActionResult Setting_Leave_ShortLeave(string ShortLeaveSettingID_Encrypted,int? CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 50;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Leave_ShortLeaveSetting leave_ShortLeaveSetting = new Leave_ShortLeaveSetting();

                param.Add("@query", "Select CompanyId As Id,CompanyName As Name from Mas_CompanyProfile Where Deactivate=0");
                var listMas_CompanyProfile = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetCompanyname = listMas_CompanyProfile;

               

                if (ShortLeaveSettingID_Encrypted != null)
                {
                  
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@P_ShortLeaveSettingID_Encrypted", ShortLeaveSettingID_Encrypted);
                    leave_ShortLeaveSetting = DapperORM.ReturnList<Leave_ShortLeaveSetting>("sp_List_Leave_ShortLeaveSetting", param).FirstOrDefault();

                    DynamicParameters paramBranch = new DynamicParameters();
                    paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBranch.Add("@p_CmpId", CmpId);
                    ViewBag.GetBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                    
                }
                else
                {
                    ViewBag.GetBranchName = "";
                }
                    return View(leave_ShortLeaveSetting);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsValidation
        [HttpGet]
        public ActionResult IsLeaveSettingExists(string ShortLeaveSettingName, string ShortLeaveSettingID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "IsValidation");
                param.Add("@P_ShortLeaveSettingID_Encrypted", ShortLeaveSettingID_Encrypted);                           
                param.Add("@P_ShortLeaveSettingName", ShortLeaveSettingName);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());                              
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Leave_ShortLeaveSetting", param);
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
        public ActionResult SaveUpdate(Leave_ShortLeaveSetting ShortLeaveSetting)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(ShortLeaveSetting.ShortLeaveSettingID_Encrypted) ? "Save" : "Update");
                param.Add("@P_ShortLeaveSettingID", ShortLeaveSetting.ShortLeaveSettingID);
                param.Add("@P_ShortLeaveSettingID_Encrypted", ShortLeaveSetting.ShortLeaveSettingID_Encrypted);
                param.Add("@P_CmpID", ShortLeaveSetting.CmpID);
                param.Add("@P_ShortLeaveSettingBranchId", ShortLeaveSetting.ShortLeaveSettingBranchId);
                param.Add("@P_ShortLeaveSettingName", ShortLeaveSetting.ShortLeaveSettingName);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@P_MonthlyLimit", ShortLeaveSetting.MonthlyLimit);
                param.Add("@P_Minute", ShortLeaveSetting.Minute);
                param.Add("@p_Direction", ShortLeaveSetting.Direction);
                param.Add("@P_BackDateDays", ShortLeaveSetting.BackDateDays);
                param.Add("@p_AutoApprovalDays", ShortLeaveSetting.AutoApprovalDays);
                param.Add("@P_IsMultipleReqAllowInDay", ShortLeaveSetting.IsMultipleReqAllowInDay);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Leave_ShortLeaveSetting", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Leave_ShortLeave", "Setting_Leave_ShortLeave");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region GetList View
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 50;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_ShortLeaveSettingID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Leave_ShortLeaveSetting", param);
                ViewBag.GetShortLeaveSetting = data;
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
        [HttpGet]
        public ActionResult Delete(string ShortLeaveSettingID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@P_ShortLeaveSettingID_Encrypted", ShortLeaveSettingID_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_Leave_ShortLeaveSetting", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Leave_ShortLeave");
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

    }
}