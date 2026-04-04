using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers
{
    public class Module_Employee_AssetController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Module/Module_Employee_Asset

        #region MainView
        public ActionResult Module_Employee_Asset()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 509;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@query", "select AssetId as Id,AssetName  as [Name] from Mas_Asset where Deactivate=0");
                var AssetName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetAssetName = AssetName;

                param = new DynamicParameters();
                param.Add("@p_AssetEmployeeId", Session["OnboardEmployeeId"]);
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Mas_Employee_Asset", param).ToList();
                ViewBag.GetAssetList = data;

                return View();

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetAssetRemark
        public ActionResult GetAssetRemark(int AssetId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@query", "select remark from Setting_Asset where AssetId=" + AssetId + " and Deactivate=0");
                var RemarkName = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).FirstOrDefault();

                var remark = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).FirstOrDefault();
                return Json(new { success = true, remark = remark?.remark ?? "" }, JsonRequestBehavior.AllowGet);
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
        public ActionResult IsAssetExists(string AssetId_Encrypted, string AssetName)
        {
            try
            {

                param.Add("@p_process", "IsValidation");
                param.Add("@p_AssetId_Encrypted", AssetId_Encrypted);
                param.Add("@p_AssetName", AssetName);

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_Asset", param);
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
        public ActionResult SaveUpdate(Mas_Employee_Asset EmployeeAsset)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var OnboardEmployeeId = Session["OnboardEmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(EmployeeAsset.AssetId_Encrypted) ? "Save" : "Update");
                param.Add("@p_AssetId", EmployeeAsset.AssetId);
                param.Add("@p_AssetId_Encrypted", EmployeeAsset.AssetId_Encrypted);
                param.Add("@p_AssetEmployeeId", OnboardEmployeeId);
                param.Add("@p_AssetEmployeeId", OnboardEmployeeId);
                param.Add("@p_AssetName", EmployeeAsset.AssetName);
                param.Add("@p_Remark", EmployeeAsset.Remark);
                param.Add("@p_RequiredAlert", EmployeeAsset.RequiredAlert);
                param.Add("@p_RequiredAlertDate", EmployeeAsset.RequiredAlertDate);
                param.Add("@p_AssetRemark", EmployeeAsset.AssetRemark);
                param.Add("@p_Date", EmployeeAsset.Date);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_Asset]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_Asset", "Module_Employee_Asset");
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
        public ActionResult Delete(string AssetId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_AssetId_Encrypted", AssetId_Encrypted);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parame
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_Asset", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_Asset", "Module_Employee_Asset");
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