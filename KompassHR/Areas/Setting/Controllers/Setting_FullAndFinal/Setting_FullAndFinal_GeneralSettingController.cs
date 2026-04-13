using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_FullAndFinal;
using KompassHR.Models;
using Dapper;
using System.Net;
using System.Data;
using System.Data.SqlClient;

namespace KompassHR.Areas.Setting.Controllers.Setting_FullAndFinal
{
    public class Setting_FullAndFinal_GeneralSettingController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: FNF/GeneralSetting
        #region GeneralSetting Main View
        [HttpGet]
        public ActionResult Setting_FullAndFinal_GeneralSetting(int? Count, string FNFSettingId_Encrypted)
        {
            ViewBag.AddUpdateTitle = "Add";
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 72;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Update";
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_FNFSettingId_Encrypted", "List");
                var data = DapperORM.ReturnList<FNF_GeneralSetting>("sp_List_FNF_GeneralSetting", param1).ToList();
                ViewBag.data = data;
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
        [HttpPost]
        public ActionResult SaveUpdate(FNF_GeneralSetting fnf_setting)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(fnf_setting.FNFSettingId_Encrypted) ? "Save" : "Update");
                param.Add("@p_FNFSettingId", fnf_setting.FNFSettingId);
                param.Add("@p_FNFSettingId_Encrypted", fnf_setting.FNFSettingId_Encrypted);
                param.Add("@p_BackDateEntryDays", fnf_setting.BackDateEntryDays);
                param.Add("@p_FutureDateEntryDays", fnf_setting.FutureDateEntryDays);
                param.Add("@p_IsImmediatelyShow", fnf_setting.IsImmediatelyShow);
                param.Add("@p_IsNoticePeriodPurchaseShow", fnf_setting.IsNoticePeriodPurchaseShow);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_FNF_Setting", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_FullAndFinal_GeneralSetting", "Setting_FullAndFinal_GeneralSetting");
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