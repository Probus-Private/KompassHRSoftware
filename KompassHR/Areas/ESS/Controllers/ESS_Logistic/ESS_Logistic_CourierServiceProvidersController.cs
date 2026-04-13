using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Logistic;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
namespace KompassHR.Areas.ESS.Controllers.ESS_Logistic
{
    public class ESS_Logistic_CourierServiceProvidersController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region CourierServiceProvider Main View 
        [HttpGet]
        public ActionResult ESS_Logistic_CourierServiceProviders(string CourierServiceProviderID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 768;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                
                Logistic_CourierServiceProvider CourierServiceProvider = new Logistic_CourierServiceProvider();
                if (CourierServiceProviderID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_CourierServiceProviderID_Encrypted", CourierServiceProviderID_Encrypted);
                    CourierServiceProvider = DapperORM.ReturnList<Logistic_CourierServiceProvider>("sp_LogiTrack_List_CourierServiceProvider", param).FirstOrDefault();
                }
                return View(CourierServiceProvider);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList 
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 768;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_CourierServiceProviderID_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_LogiTrack_List_CourierServiceProvider", param).ToList();
                ViewBag.GetCourierServiceProviderList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsCourierServiceProviderExists
        public ActionResult IsCourierServiceProviderExists(string CourierServiceProvider, string CourierServiceProviderID_Encrypted,string MobileNo,string WhatappsNo,string Email,string CourierType,string Destination)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_CourierServiceProvider", CourierServiceProvider);
                    param.Add("@p_CourierServiceProviderID_Encrypted", CourierServiceProviderID_Encrypted);
                    param.Add("@p_MobileNo", MobileNo);
                    param.Add("@p_WhatappsNo", WhatappsNo);
                    param.Add("@p_EmailId", Email);
                    param.Add("@p_CourierType", CourierType);
                    param.Add("@p_Destination", Destination);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_LogiTrack_SUD_CourierServiceProvider", param);
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
        public ActionResult SaveUpdate(Logistic_CourierServiceProvider Logistic_CourierServiceProvider)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                param.Add("@p_process", string.IsNullOrEmpty(Logistic_CourierServiceProvider.CourierServiceProviderID_Encrypted) ? "Save" : "Update");
                param.Add("@p_CourierServiceProviderID", Logistic_CourierServiceProvider.CourierServiceProviderID);
                param.Add("@p_CourierServiceProviderID_Encrypted", Logistic_CourierServiceProvider.CourierServiceProviderID_Encrypted);
                param.Add("@p_CourierServiceProvider", Logistic_CourierServiceProvider.CourierServiceProviderName);
                param.Add("@p_MobileNo", Logistic_CourierServiceProvider.MobileNo);
                param.Add("@p_WhatappsNo", Logistic_CourierServiceProvider.WhatappsNo);
                param.Add("@p_EmailId", Logistic_CourierServiceProvider.EmailId);
                param.Add("@p_Address", Logistic_CourierServiceProvider.Address);
                param.Add("@p_CourierType", Logistic_CourierServiceProvider.CourierType);
                param.Add("@p_Destination", Logistic_CourierServiceProvider.Destination);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_LogiTrack_SUD_CourierServiceProvider", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_Logistic_CourierServiceProviders", "ESS_Logistic_CourierServiceProviders");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(int? CourierServiceProviderID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_CourierServiceProviderID", CourierServiceProviderID);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_LogiTrack_SUD_CourierServiceProvider", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_Logistic_CourierServiceProviders");
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