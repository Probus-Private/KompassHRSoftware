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
    public class Setting_TimeOffice_UsbSettingController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_TimeOffice_UsbSetting

        #region Setting_TimeOffice_USBSetting
        public ActionResult Setting_TimeOffice_UsbSetting(string USBId_Encrypted)
        {
            try
            {
                ViewBag.AddUpdateTitle = "Add";
                Atten_LogColumnSettings LogColumnSettings = new Atten_LogColumnSettings();
                param = new DynamicParameters();
                if (USBId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_USBId_Encrypted", USBId_Encrypted);
                    LogColumnSettings = DapperORM.ReturnList<Atten_LogColumnSettings>("sp_List_Atten_LogColumnSettings", param).FirstOrDefault();
                }
                return View(LogColumnSettings);
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
        public ActionResult Is_USBNameExists(string USBName, string USBId_Encrypted)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "IsValidation");
                param.Add("@p_USBId_Encrypted", USBId_Encrypted);
                param.Add("@p_USBName", USBName);
                param.Add("@p_MachineName", Dns.GetHostName());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                var result = DapperORM.ExecuteReturn("sp_SUD_Atten_LogColumnSettings", param);
                var message = param.Get<string>("@p_msg");
                var icon = param.Get<string>("@p_Icon");

                if (!string.IsNullOrEmpty(message))
                {
                    return Json(new { message, icon }, JsonRequestBehavior.AllowGet);
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
        public ActionResult SaveUpdate(Atten_LogColumnSettings LogColumnSettings)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(LogColumnSettings.USBId_Encrypted) ? "Save" : "Update");
                param.Add("@p_USBName", LogColumnSettings.USBName);
                param.Add("@p_UserId", LogColumnSettings.UserId);
                param.Add("@p_LogDate", LogColumnSettings.LogDate);
                param.Add("@p_DeviceId", LogColumnSettings.DeviceId);
                param.Add("@p_Direction", LogColumnSettings.Direction);

                param.Add("@p_DirectionIn", LogColumnSettings.DirectionIn);
                param.Add("@p_DirectionOut", LogColumnSettings.DirectionOut);

                param.Add("@p_UseBy", LogColumnSettings.UseBy);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_USBId_Encrypted", LogColumnSettings.USBId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var data = DapperORM.ExecuteReturn("sp_SUD_Atten_LogColumnSettings", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_TimeOffice_UsbSetting", "Setting_TimeOffice_UsbSetting");
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
                param.Add("@p_USBId_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Atten_LogColumnSettings", param);
                ViewBag.Get_LogColumnSetting = data;
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
        public ActionResult Delete(string USBId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_USBId_Encrypted", USBId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_LogColumnSettings", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_TimeOffice_UsbSetting", new { Area = "Setting" });
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