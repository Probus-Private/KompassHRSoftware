using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Payroll;
using System.Data;
using System.Net;
using System.Data.SqlClient;
using KompassHR.Areas.Setting.Models.Setting_PolicyAndLibrary;

namespace KompassHR.Areas.Setting.Controllers.Setting_PolicyAndLibrary
{
    public class Setting_PolicyAndLibrary_PlantHRController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_PolicyAndLibrary_PlantHR
        public ActionResult Setting_PolicyAndLibrary_PlantHR(string PlantHRId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 514;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var GetEmployee = new BulkAccessClass().AllEmployeeName();
                ViewBag.AllEmployeeName = GetEmployee;

                ViewBag.AddUpdateTitle = "Add";
                Mas_PlantHR Mas_PlantHR = new Mas_PlantHR();

                if (PlantHRId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_PlantHRId_Encrypted", PlantHRId_Encrypted);
                    Mas_PlantHR = DapperORM.ReturnList<Mas_PlantHR>("sp_List_PlantHR", param).FirstOrDefault();
                }
                return View(Mas_PlantHR);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region IsValidation
        [HttpGet]
        public ActionResult IsPlantHRExists(int EmployeeID, string PlantHRId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_PlantHREmployeeID", EmployeeID);
                param.Add("@p_PlantHRId_Encrypted", PlantHRId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_PlantHR", param);
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
        public ActionResult SaveUpdate(Mas_PlantHR PlantHR)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(PlantHR.PlantHRId_Encrypted) ? "Save" : "Update");
                param.Add("@P_PlantHRId_Encrypted", PlantHR.PlantHRId_Encrypted);
                param.Add("@p_PlantHREmployeeId", PlantHR.PlantHREmployeeID);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_PlantHR", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_PolicyAndLibrary_PlantHR", "Setting_PolicyAndLibrary_PlantHR");

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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 514;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_PlantHRId_Encrypted", "List");
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ReturnList<Mas_PlantHR>("sp_List_PlantHR", param).ToList();
                ViewBag.GetPlantHRList = data;
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
        public ActionResult Delete(string PlantHRId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_PlantHRId_Encrypted", PlantHRId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_PlantHR", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_PolicyAndLibrary_PlantHR", new { Area = "Setting" });
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