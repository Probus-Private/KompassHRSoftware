using Dapper;
using KompassHR.Models;
using KompassHR.Areas.Setting.Models.Setting_Prime;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Prime
{
    public class Setting_Prime_CompanyProfileController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        [HttpGet]

        #region CompanyProfile Main View 
        public ActionResult Setting_Prime_CompanyProfile(string CompanyId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 2;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Mas_CompanyProfile mas_companyProfile = new Mas_CompanyProfile();
                ViewBag.AddUpdateTitle = "Add";
                if (CompanyId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@P_CompanyId_Encrypted", CompanyId_Encrypted);
                    mas_companyProfile = DapperORM.ReturnList<Mas_CompanyProfile>("sp_List_Mas_CompanyProfile", param).FirstOrDefault();
                }
                TempData["EstablishedDate"] = mas_companyProfile.EstablishedDate;
                return View(mas_companyProfile);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsCompanyProfileExists
        public ActionResult IsCompanyProfileExists(string ComapnyName, string ShortName, string CompanyIdEncrypted)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_CompanyName", ComapnyName);
                    param.Add("@p_ShortName", ShortName);
                    param.Add("@P_CompanyId_Encrypted", CompanyIdEncrypted);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_CompanyProfile", param);
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
        public ActionResult SaveUpdate(Mas_CompanyProfile CompanyProfile, HttpPostedFileBase files)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
              //  var Logo = Request.Files["Logo"];
               // var Stamp = Request.Files["Stamp"];
                param.Add("@p_process", string.IsNullOrEmpty(CompanyProfile.CompanyId_Encrypted) ? "Save" : "Update");
                param.Add("@P_CompanyId_Encrypted", CompanyProfile.CompanyId_Encrypted);
                param.Add("@p_CompanyId", CompanyProfile.CompanyId);
                param.Add("@p_CompanyName", CompanyProfile.CompanyName);
                param.Add("@p_ShortName", CompanyProfile.ShortName);
                param.Add("@p_Address", CompanyProfile.Address);
                param.Add("@p_PhoneNo", CompanyProfile.PhoneNo);
                param.Add("@p_EmailId", CompanyProfile.EmailId);
                param.Add("@p_Website", CompanyProfile.WebSite);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_ManagerName", CompanyProfile.ManagerName);
                param.Add("@p_IndustryType", CompanyProfile.IndustryType);
                param.Add("@p_PAN", CompanyProfile.PAN);
                param.Add("@p_GST", CompanyProfile.GST);
                param.Add("@p_CIN", CompanyProfile.CIN);
                param.Add("@p_EstablishedDate", CompanyProfile.EstablishedDate);
                param.Add("@p_NotificationDays", CompanyProfile.NotificationDays);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_IsActive", true);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_CompanyProfile", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Prime_CompanyProfile", "Setting_Prime_CompanyProfile");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 2;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_CompanyId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Mas_CompanyProfile", param).ToList();
                ViewBag.GetCompanyProfiletList = data;
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
        public ActionResult Delete(double? CompanyId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_CompanyId", CompanyId);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_CompanyProfile", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("GetList", "Setting_Prime_CompanyProfile");
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