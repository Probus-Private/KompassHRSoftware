using Dapper;
using KompassHR.Areas.Setting.Models.Setting_TimeOffice;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_TimeOffice
{
    public class Setting_TimeOffice_GeneralSettingController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_TimeOffice_GeneralSetting

        #region GeneralSetting Main View
        [HttpGet]
        public ActionResult Setting_TimeOffice_GeneralSetting(string AttenGeneralId_Encrypted,int? CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 58;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Atten_GeneralSetting GeneralSettingOutDoor = new Atten_GeneralSetting();
                ViewBag.AddUpdateTitle = "Add";
                param.Add("@query", "Select CompanyId As Id,CompanyName As Name from Mas_CompanyProfile Where Deactivate = 0 order by Name");
                var listMas_CompanyProfile = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetCompanyname = listMas_CompanyProfile;
                if(AttenGeneralId_Encrypted!=null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_AttenGeneralId_Encrypted", AttenGeneralId_Encrypted);
                    GeneralSettingOutDoor = DapperORM.ReturnList<Atten_GeneralSetting>("sp_List_Atten_GeneralSetting", param).FirstOrDefault();

                    DynamicParameters paramBranch = new DynamicParameters();
                    paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBranch.Add("@p_CmpId", CmpId);
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                    ViewBag.BranchName = data;
                }
                else
                {
                    ViewBag.BranchName = "";
                }
               
                return View(GeneralSettingOutDoor);
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
        public ActionResult IsGeneralSettingExists(double CmpId,string AttenGeneralId_Encrypted,int OutDoorCompanyBranchId, bool IsDefault,string OutDoorCompanySettingName)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_AttenGeneralId_Encrypted", AttenGeneralId_Encrypted);
                param.Add("@p_CmpId", CmpId);
                param.Add("@p_OutDoorCompanyBranchId", OutDoorCompanyBranchId);
                param.Add("@p_IsDefault", IsDefault);
                param.Add("@p_OutDoorCompanySettingName", OutDoorCompanySettingName);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Atten_GeneralSetting", param);
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 58;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_AttenGeneralId_Encrypted", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var data = DapperORM.ReturnList<dynamic>("sp_List_Atten_GeneralSetting", param).ToList();
                ViewBag.AttenGeneralList = data;
                return View();
                //var data = DapperORM.DynamicQuerySingle("Select  OutDoorCompanyShortPeriod, OutDoorCompanyIstHalfDuration, OutDoorCompanyIIndHalfDuration,OutDoorCompanyFullDayDuration,  BackDatedDays, AllowTeamRequest ,IsDefault,AttenGeneralId_Encrypted from Atten_GeneralSetting where Deactivate=0 and  cmpId=" + CmpId + " and OutDoorCompanyBranchId="+ OutDoorCompanyBranchId + "");
                //return Json(data, JsonRequestBehavior.AllowGet);
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
        public ActionResult SaveUpdate(Atten_GeneralSetting ObjGeneralSetting)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(ObjGeneralSetting.AttenGeneralId_Encrypted) ? "Save" : "Update");
                param.Add("@p_AttenGeneralId", ObjGeneralSetting.AttenGeneralId);
                param.Add("@p_AttenGeneralId_Encrypted", ObjGeneralSetting.AttenGeneralId_Encrypted);
                param.Add("@p_CmpId", ObjGeneralSetting.CmpId);
                param.Add("@p_OutDoorCompanySettingName", ObjGeneralSetting.OutDoorCompanySettingName);
                param.Add("@p_IsDefault", ObjGeneralSetting.IsDefault);
                param.Add("@p_OutDoorCompanyBranchId", ObjGeneralSetting.OutDoorCompanyBranchId);
                param.Add("@p_OutDoorCompanyShortPeriod", ObjGeneralSetting.OutDoorCompanyShortPeriod);
                param.Add("@p_OutDoorCompanyIstHalfDuration", ObjGeneralSetting.OutDoorCompanyIstHalfDuration);
                param.Add("@p_OutDoorCompanyIIndHalfDuration", ObjGeneralSetting.OutDoorCompanyIIndHalfDuration);
                param.Add("@p_OutDoorCompanyFullDayDuration", ObjGeneralSetting.OutDoorCompanyFullDayDuration);
                param.Add("@p_AllowTeamRequest", ObjGeneralSetting.AllowTeamRequest);
                param.Add("@p_IsBackDate", ObjGeneralSetting.IsBackDate);
                param.Add("@p_BackDatedDays", ObjGeneralSetting.BackDatedDays);
                param.Add("@p_FutureDatedDays", ObjGeneralSetting.FutureDatedDays);
                param.Add("@p_AutoApprovalDays", ObjGeneralSetting.AutoApprovalDays);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);              
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Atten_GeneralSetting", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return RedirectToAction("Setting_TimeOffice_GeneralSetting", "Setting_TimeOffice_GeneralSetting");
                
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

        #region Delete
        public ActionResult Delete(string AttenGeneralId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_AttenGeneralId_Encrypted", AttenGeneralId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_GeneralSetting", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_TimeOffice_GeneralSetting");
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