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
    public class Setting_TimeOffice_PersonalGatepassController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: Setting/Setting_TimeOffice_PersonalGatepass

        #region PersonalGatepass Main View
        [HttpGet]
        public ActionResult Setting_TimeOffice_PersonalGatepass(string PersonalGatepassSettingId_Encrypted, int? CompanyId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 57;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Atten_PersonalGatepassSetting Gatepass_setting = new Atten_PersonalGatepassSetting();              
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
                if (PersonalGatepassSettingId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_PersonalGatepassSettingId_Encrypted", PersonalGatepassSettingId_Encrypted);
                    Gatepass_setting = DapperORM.ReturnList<Atten_PersonalGatepassSetting>("sp_List_Atten_PersonalGatepassSetting", param).FirstOrDefault();
                }
                return View(Gatepass_setting);
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
        public ActionResult IsPersonalGatePassExists(string PersonalGatepassSettingName, string PersonalGatepassSettingId_Encrypted, double BusinessUnit, double CmpId)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_PersonalGatepassSettingId_Encrypted", PersonalGatepassSettingId_Encrypted);
                param.Add("@p_CmpID", CmpId);
                param.Add("@p_PersonalGatepassSettingName", PersonalGatepassSettingName);
                param.Add("@p_PersonalGatepassSettingBranchId", BusinessUnit);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Atten_PersonalGatepassSetting", param);
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
        public ActionResult SaveUpdate(Atten_PersonalGatepassSetting PersonalGatePass)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(PersonalGatePass.PersonalGatepassSettingId_Encrypted) ? "Save" : "Update");
                param.Add("@p_PersonalGatepassSettingId", PersonalGatePass.PersonalGatepassSettingId);
                param.Add("@p_PersonalGatepassSettingId_Encrypted", PersonalGatePass.PersonalGatepassSettingId_Encrypted);
                param.Add("@p_PersonalGatepassSettingBranchId", PersonalGatePass.PersonalGatepassSettingBranchId);
                param.Add("@p_PersonalGatepassSettingName", PersonalGatePass.PersonalGatepassSettingName);
                param.Add("@p_MonthlyLimit", PersonalGatePass.MonthlyLimit);
                param.Add("@p_BackDateDays", PersonalGatePass.BackDateDays);
                param.Add("@p_Minute", PersonalGatePass.Minute);
                param.Add("@p_IsMultipleReqAllowInDay", PersonalGatePass.IsMultipleReqAllowInDay);
                param.Add("@p_IsDefault", PersonalGatePass.IsDefault);
                param.Add("@p_AutoApprovalDays", PersonalGatePass.AutoApprovalDays);
                param.Add("@p_TotalDurationInMonth", PersonalGatePass.TotalDurationInMonth);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_CmpID", PersonalGatePass.CmpId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Atten_PersonalGatepassSetting", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return RedirectToAction("Setting_TimeOffice_PersonalGatepass", "Setting_TimeOffice_PersonalGatepass");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 57;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_PersonalGatepassSettingId_Encrypted", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var data = DapperORM.DynamicList("sp_List_Atten_PersonalGatepassSetting", param);
                ViewBag.GetPersonalGatepassList = data;
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
        public ActionResult Delete(string PersonalGatepassSettingId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_PersonalGatepassSettingId_Encrypted", PersonalGatepassSettingId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_PersonalGatepassSetting", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_TimeOffice_PersonalGatepass");

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