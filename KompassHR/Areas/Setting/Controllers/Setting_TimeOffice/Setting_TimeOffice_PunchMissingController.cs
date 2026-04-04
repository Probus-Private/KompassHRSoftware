using Dapper;
using KompassHR.Areas.Setting.Models.Setting_TimeOffice;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_TimeOffice
{
    public class Setting_TimeOffice_PunchMissingController : Controller
    {
        DynamicParameters param = new DynamicParameters();

        #region PunchMissing Main View
        [HttpGet]
        public ActionResult Setting_TimeOffice_PunchMissing(string PunchMissingSettingId_Encrypted, int? CompanyId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 56;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();

                Atten_PunchMissingSetting punch_setting = new Atten_PunchMissingSetting();
                param.Add("@query", "Select  CompanyId As Id, CompanyName As Name from Mas_CompanyProfile where Deactivate = 0 order by Name");
                var GetCompanyName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.CompanyName = GetCompanyName;
                ViewBag.AddUpdateTitle = "Add";

                if (CompanyId != null)
                {
                    DynamicParameters ParamBranch = new DynamicParameters();
                    ParamBranch.Add("@p_employeeid", Session["EmployeeId"]);
                    ParamBranch.Add("@p_CmpId", CompanyId);
                    ViewBag.Location = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", ParamBranch).ToList();
                }
                else
                {
                    ViewBag.Location = "";
                }

                param = new DynamicParameters();
                if (PunchMissingSettingId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_PunchMissingSetting_Encrypted", PunchMissingSettingId_Encrypted);
                    punch_setting = DapperORM.ReturnList<Atten_PunchMissingSetting>("sp_List_Atten_PunchMissingSetting", param).FirstOrDefault();
                    TempData["MonthlyLimitType"] = punch_setting.MonthlyLimitType;
                }
                else
                {
                    TempData["MonthlyLimitType"] = "";
                }
                return View(punch_setting);
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

        #region IsValidation
        [HttpGet]
        public ActionResult IsPunchMissingExists(string PunchMissingSettingName, string PunchMissingSettingId_Encrypted, double BusinessUnit, double CmpId)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@P_PunchMissingSettingId_Encrypted", PunchMissingSettingId_Encrypted);
                param.Add("@P_CmpID", CmpId);
                param.Add("@p_PunchMissingSettingName", PunchMissingSettingName);
                param.Add("@P_PunchMissingSettingBranchId", BusinessUnit);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Atten_PunchMissingSetting", param);
                var Message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = Message;
                TempData["Icon"] = Icon;
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
        public ActionResult SaveUpdate(Atten_PunchMissingSetting punch)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(punch.PunchMissingSettingId_Encrypted) ? "Save" : "Update");
                param.Add("@P_PunchMissingSettingID", punch.PunchMissingSettingId);
                param.Add("@P_PunchMissingSettingId_Encrypted", punch.PunchMissingSettingId_Encrypted);
                param.Add("@p_PunchMissingSettingName", punch.PunchMissingSettingName);
                param.Add("@P_MonthlyRequestLimit", punch.MonthlyRequestLimit);
                param.Add("@P_MonthlyLimitType", punch.MonthlyLimitType);
                param.Add("@P_MonthlyLimitForIn", punch.MonthlyLimitForIn);
                param.Add("@P_MonthlyLimitForOut", punch.MonthlyLimitForOut);
                param.Add("@P_BackDateDays", punch.BackDateDays);
                param.Add("@P_PunchMissingSettingBranchId", punch.PunchMissingSettingBranchId);
                param.Add("@P_IsDefault", punch.IsDefault);
                param.Add("@P_AllowForAfterShiftEndTime", punch.AllowForAfterShiftEndTime);
                param.Add("@P_AllowForBeforeShiftStartTime", punch.AllowForBeforeShiftStartTime);
                param.Add("@P_InOutApplySameDay", punch.InOutApplySameDay);
                param.Add("@p_AutoApprovalDays", punch.AutoApprovalDays);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_CmpID", punch.CmpId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Atten_PunchMissingSetting", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return RedirectToAction("Setting_TimeOffice_PunchMissing", "Setting_TimeOffice_PunchMissing");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 56;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_PunchMissingSetting_Encrypted", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var data = DapperORM.DynamicList("sp_List_Atten_PunchMissingSetting", param);
                ViewBag.GetPunchMissingGroupList = data;
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
        public ActionResult Delete(string PunchMissingSettingId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@P_PunchMissingSettingId_Encrypted", PunchMissingSettingId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_PunchMissingSetting", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_TimeOffice_PunchMissing");
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