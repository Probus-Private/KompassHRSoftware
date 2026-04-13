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
    public class Setting_TimeOffice_LogSettingController : Controller
    {
        // GET: Setting/Setting_TimeOffice_LogSetting
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        #region Setting_TimeOffice_LogSetting
        public ActionResult Setting_TimeOffice_LogSetting(string AttenLogSettingId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Atten_LogDownloadSetting Log_Setting = new Atten_LogDownloadSetting();
                param = new DynamicParameters();
                if (AttenLogSettingId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_AttenLogSettingId_Encrypted", AttenLogSettingId_Encrypted);
                    Log_Setting = DapperORM.ReturnList<Atten_LogDownloadSetting>("sp_List_Atten_LogDownloadSetting", param).FirstOrDefault();
                }
                return View(Log_Setting);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region TestConnection
        [HttpPost]
        public ActionResult TestConnection(string ServerName, string UserName, string Password, string DatabaseName)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                bool isValid = DatabaseHelper.VerifyDatabaseCredentials(ServerName, DatabaseName, UserName, Password);

                var Message = isValid ? "Connection successful!" : "Invalid credentials or unable to connect.";
                var Icon = isValid ? "success" : "error";
                return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
            //if (ModelState.IsValid)
            //{

            //}
        }
        #endregion

        #region IsValidation
        [HttpGet]
        public ActionResult Is_LogSettingExists(string LogSettingName, string ServerName, string UserName, string Password, string DatabaseName, string AttenLogSettingId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_AttenLogSettingId_Encrypted", AttenLogSettingId_Encrypted);
                param.Add("@p_LogSettingName", LogSettingName);
                param.Add("@p_ServerName", ServerName);
                param.Add("@p_UserName", UserName);
                param.Add("@p_Password", Password);
                param.Add("@p_DatabaseName", DatabaseName);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_LogDownloadSetting", param);
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
        public ActionResult SaveUpdate(Atten_LogDownloadSetting _LogSetting)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(_LogSetting.AttenLogSettingId_Encrypted) ? "Save" : "Update");
                param.Add("@p_LogSettingName", _LogSetting.LogSettingName);
                param.Add("@p_ServerName", _LogSetting.ServerName);
                param.Add("@p_UserName", _LogSetting.UserName);
                param.Add("@p_Password", _LogSetting.Password);
                param.Add("@p_DatabaseName", _LogSetting.DatabaseName);
                param.Add("@p_TableName", _LogSetting.TableName);
                param.Add("@p_DeviceId", _LogSetting.DeviceId);
                param.Add("@p_UserId", _LogSetting.UserId);
                param.Add("@p_DownloadDate", _LogSetting.DownloadDate);
                param.Add("@p_LogDate", _LogSetting.LogDate);
                param.Add("@p_Direction", _LogSetting.Direction);
                param.Add("@p_IsMonthWise", _LogSetting.IsMonthWise);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Atten_LogDownloadSetting", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_TimeOffice_LogSetting", "Setting_TimeOffice_LogSetting");
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
                param.Add("@p_AttenLogSettingId_Encrypted", "List");
                // param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var data = DapperORM.DynamicList("sp_List_Atten_LogDownloadSetting", param);
                ViewBag.Get_LogDownloadSetting = data;
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
        public ActionResult Delete(string AttenLogSettingId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@P_AttenLogSettingId_Encrypted", AttenLogSettingId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_LogDownloadSetting", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_TimeOffice_LogSetting");
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