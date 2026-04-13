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
    public class Setting_TimeOffice_RegulizationSettingController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region Setting_TimeOffice_RegulizationSetting Main View 
        [HttpGet]
        // GET: Setting/Setting_TimeOffice_RegulizationSetting
        public ActionResult Setting_TimeOffice_RegulizationSetting(string RegulizationSettingId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 864;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";


                Atten_RegulizationSetting RegulizationSetting = new Atten_RegulizationSetting();
                if (RegulizationSettingId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_RegulizationSettingId_Encrypted", RegulizationSettingId_Encrypted);
                    RegulizationSetting = DapperORM.ReturnList<Atten_RegulizationSetting>("sp_List_RegulizationSetting", param).FirstOrDefault();

                    if (RegulizationSetting.FromTime != null)
                    {
                        TempData["FromTime"] = RegulizationSetting.FromTime.Value.ToString(@"hh\:mm");
                        // TimeSpan FromTime = RegulizationSetting.FromTime;
                        //  TempData["FromTime"] = FromTime.ToString(@"hh\:mm");
                    }
                    if (RegulizationSetting.ToTime != null)
                    {
                        TempData["ToTime"] = RegulizationSetting.ToTime.Value.ToString(@"hh\:mm");

                        //  TimeSpan ToTime = RegulizationSetting.ToTime;
                        //  TempData["ToTime"] = ToTime.ToString(@"hh\:mm");
                    }

                }
                return View(RegulizationSetting);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsRegulizationExists
        public ActionResult IsRegulizationExists(string RegulizationSettingId_Encrypted,string Type,TimeSpan FromTime,TimeSpan ToTime)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_RegulizationSettingId_Encrypted", RegulizationSettingId_Encrypted);
                    param.Add("@p_Type", Type);
                    param.Add("@p_FromTime", FromTime);
                    param.Add("@p_ToTime", ToTime);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_RegulizationSetting", param);
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
        public ActionResult SaveUpdate(Atten_RegulizationSetting RegulizationSetting)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                param.Add("@p_process", string.IsNullOrEmpty(RegulizationSetting.RegulizationSettingId_Encrypted) ? "Save" : "Update");
                param.Add("@p_RegulizationSettingId_Encrypted", RegulizationSetting.RegulizationSettingId_Encrypted);
                param.Add("@p_FromTime", RegulizationSetting.FromTime);
                param.Add("@p_ToTime", RegulizationSetting.ToTime);
                param.Add("@p_Type", RegulizationSetting.Type);
                
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_RegulizationSetting", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
               
                return RedirectToAction("Setting_TimeOffice_RegulizationSetting", "Setting_TimeOffice_RegulizationSetting");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 864;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_RegulizationSettingId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_RegulizationSetting", param).ToList();
                ViewBag.GetReminderList = data;
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
        public ActionResult Delete(string RegulizationSettingId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_RegulizationSettingId_Encrypted", RegulizationSettingId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_RegulizationSetting", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_TimeOffice_RegulizationSetting");
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