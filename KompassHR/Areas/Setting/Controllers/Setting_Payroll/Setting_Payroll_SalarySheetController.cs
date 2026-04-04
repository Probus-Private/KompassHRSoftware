using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Payroll;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Payroll
{
    public class Setting_Payroll_SalarySheetController : Controller
    {

        #region Main View 
        // GET: Setting/Setting_Payroll_SalarySheet
        public ActionResult Setting_Payroll_SalarySheet(string SalarySheet_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 748;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Payroll_SalarySheet  SalarySheet = new Payroll_SalarySheet();
                ViewBag.AddUpdateTitle = "Add";

                Payroll_SalarySheet Salary_Sheet = new Payroll_SalarySheet();
                DynamicParameters param = new DynamicParameters();

                if (SalarySheet_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_SalarySheet_Encrypted", SalarySheet_Encrypted);
                    SalarySheet = DapperORM.ReturnList<Payroll_SalarySheet>("sp_List_Payroll_SalarySheet", param).FirstOrDefault();
                }
                return View(SalarySheet);
              
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region IsSalarySheetExists
        public ActionResult IsSalarySheetExists(string SalarySheetName, string SalarySheet_Encrypted)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@p_process", "IsValidation");
                param.Add("@p_SalarySheetName", SalarySheetName);
                param.Add("@p_SalarySheet_Encrypted", SalarySheet_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_SalarySheet", param);
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
        public ActionResult SaveUpdate(Payroll_SalarySheet  Salary_Sheet)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var param = new DynamicParameters();

                //param.Add("@p_process", "IsValidation");

                param.Add("@p_process", string.IsNullOrEmpty(Salary_Sheet.SalarySheet_Encrypted) ? "Save" : "Update");
                 param.Add("@p_SalarySheetId", Salary_Sheet.SalarySheetId);
                param.Add("@p_SalarySheet_Encrypted", Salary_Sheet.SalarySheet_Encrypted);
                param.Add("@p_SalarySheetName", Salary_Sheet.SalarySheetName);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_IsDefault", Salary_Sheet.IsDefault);
                
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_SalarySheet", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("GetList", "Setting_Payroll_SalarySheet");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 748;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var param = new DynamicParameters();
                param.Add("@p_SalarySheet_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Payroll_SalarySheet", param);
                ViewBag.GetSalarySheetList = data;
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
        public ActionResult Delete(string SalarySheet_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var param = new DynamicParameters();
                param.Add("@p_process", "Delete");
                param.Add("@p_SalarySheet_Encrypted", SalarySheet_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_SalarySheet", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Payroll_SalarySheet");
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