using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Onboarding;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Onboarding
{
    public class Setting_Onboarding_PinCodeController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: Onboarding/PinCode
        #region Setting_Onboarding_PinCode
        [HttpGet]
        public ActionResult Setting_Onboarding_PinCode(string PinId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                 // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 32;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Mas_PinCode mas_PinCode = new Mas_PinCode();
                ViewBag.AddUpdateTitle = "Add";
                
                if (PinId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_PinId_Encrypted", PinId_Encrypted);
                    mas_PinCode = DapperORM.ReturnList<Mas_PinCode>("sp_List_Mas_PinCode", param).FirstOrDefault();
                }
                return View(mas_PinCode);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region IsPinCodeExists
        public ActionResult IsPinCodeExists(string PinCode, string PinIdEncrypted, string StateName, string DistrictName, string TalukaName, string OfficeName)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {

                    param.Add("@p_process", "IsValidation");
                    param.Add("@P_PinId_Encrypted", PinIdEncrypted);
                    param.Add("@p_PinCode", PinCode);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_PinCode", param);
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
        public ActionResult SaveUpdate(Mas_PinCode MasPinCode)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(MasPinCode.PinId_Encrypted) ? "Save" : "Update");
                param.Add("@p_PinId", MasPinCode.PinId);
                param.Add("@P_PinId_Encrypted", MasPinCode.PinId_Encrypted);
                param.Add("@p_PinCode", MasPinCode.PinCode);
                param.Add("@p_StateName", MasPinCode.StateName);
                param.Add("@p_DistrictName", MasPinCode.DistrictName);
                param.Add("@p_TalukaName", MasPinCode.TalukaName);
                param.Add("@p_OfficeName", MasPinCode.OfficeName);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_PinCode", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Onboarding_PinCode", "Setting_Onboarding_PinCode");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
        
        public ActionResult GetList(Mas_PinCode district)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
               
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 32;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                if (district.DistrictName!=null)
                {
                    param.Add("@p_PinId_Encrypted", "List");
                    param.Add("@p_DistrictName", district.DistrictName);
                    param.Add("@p_SearchType", district.SearchType);
                    ViewBag.EmployeeType = DapperORM.DynamicList("sp_List_Mas_PinCode", param);
                }
                else
                {
                    ViewBag.EmployeeType = "";
                }
               
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetPincode 
        public ActionResult GetListPincode(string DistrictName)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                
                param.Add("@p_PinId_Encrypted", "List");
                param.Add("@p_DistrictName", DistrictName);
               var data = DapperORM.DynamicList("sp_List_Mas_PinCode", param);
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
        public ActionResult Delete(string PinId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_PinId_Encrypted", PinId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_PinCode", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Onboarding_PinCode");
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