using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Preboarding;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;


namespace KompassHR.Areas.Setting.Controllers.Setting_Preboarding
{
    public class Setting_Preboarding_GeneralSettingController : Controller
    {
        // GET: Setting/Setting_Preboarding_GeneralSetting
        DynamicParameters param = new DynamicParameters();
        #region GeneralSetting Main View
        public ActionResult Setting_Preboarding_GeneralSetting(string PreGeneralSettingId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 41;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Preboarding_GeneralSetting Setting_Preboarding_GeneralSetting = new Preboarding_GeneralSetting();
                Setting_Preboarding_GeneralSetting = DapperORM.ReturnList<Preboarding_GeneralSetting>("sp_List_Preboading_GeneralSetting", param).FirstOrDefault();
                if(Setting_Preboarding_GeneralSetting!=null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                }
                return View(Setting_Preboarding_GeneralSetting);
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
        public ActionResult SaveUpdate(Preboarding_GeneralSetting ObjPreboadring_GeneralSetting)
        {
            try
            {
                param.Add("@p_process", "Save");
                param.Add("@p_PreGeneralSettingId", ObjPreboadring_GeneralSetting.PreGeneralSettingId);
                param.Add("@p_PreGeneralSettingId_Encrypted", ObjPreboadring_GeneralSetting.PreGeneralSettingId_Encrypted);
                param.Add("@p_AgeLimitYear", ObjPreboadring_GeneralSetting.AgeLimitYear);
                param.Add("@p_PreboardingLinkExpiryDays", ObjPreboadring_GeneralSetting.PreboardingLinkExpiryDays);
                param.Add("@p_IncludingWorkingDay", ObjPreboadring_GeneralSetting.IncludingWorkingDay);
                param.Add("@p_PreboardingLink", ObjPreboadring_GeneralSetting.PreboardingLink);
                param.Add("@p_ReporingToHrAlertDays_Reporting", ObjPreboadring_GeneralSetting.ReporingToHrAlertDays_Reporting);
                param.Add("@p_ReportingToHrAlertDays_CandidateNotLogin", ObjPreboadring_GeneralSetting.ReportingToHrAlertDays_CandidateNotLogin);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Preaboarding_GeneralSetting", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Preboarding_GeneralSetting", "Setting_Preboarding_GeneralSetting");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 41;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_PreGeneralSettingId_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Preboadring_GeneralSetting", param);
                ViewBag.GetGenralSetting = data;
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