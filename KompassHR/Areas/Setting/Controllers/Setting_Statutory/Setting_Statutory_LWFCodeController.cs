using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_Statutory;
using KompassHR.Models;
using System.Data;
using System.Net;
using System.Data.SqlClient;
using KompassHR.Areas.Setting.Models.Setting_PolicyAndLibrary;

namespace KompassHR.Areas.Setting.Controllers.Setting_Statutory
{
    public class Setting_Statutory_LWFCodeController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: StatutorySetting/StatutoryLWFCode
        #region LWFCode main View
        [HttpGet]
        public ActionResult Setting_Statutory_LWFCode(string LWFCodeId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 98;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Payroll_LWFCode payroll_lwfcode = new Payroll_LWFCode();
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetCompanyname = GetComapnyName;

                DynamicParameters paramstatesName = new DynamicParameters();
                paramstatesName.Add("@query", "Select StateId as Id,StateName As Name from  Mas_States Where Deactivate=0 and LWFApplicable=1");
                var listmas_states = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramstatesName).ToList();
                ViewBag.GetstatesName = listmas_states;

                ViewBag.AddUpdateTitle = "Add";
                if (LWFCodeId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_LWFCodeId_Encrypted", LWFCodeId_Encrypted);
                    payroll_lwfcode = DapperORM.ReturnList<Payroll_LWFCode>("sp_List_Payroll_LWFCode", param).FirstOrDefault();
                }
                return View(payroll_lwfcode);
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
        public ActionResult IsStatutoryPTCodeExists(double CmpID, string LWFCode, string LWFCodeId_Encrypted,string LWFRemark)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "IsValidation");
                param.Add("@p_LWFCodeId_Encrypted", LWFCodeId_Encrypted);
                param.Add("@p_CmpID", CmpID);
                param.Add("@p_LWFRemark", LWFRemark);
              //  param.Add("@p_LWFCodeBranchId", LWFCodeBranchId);
                param.Add("@p_LWFCode", LWFCode);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_LWFCode", param);
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
        public ActionResult SaveUpdate(Payroll_LWFCode PayrollLWFCode)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(PayrollLWFCode.LWFCodeId_Encrypted) ? "Save" : "Update");
                param.Add("@p_LWFCodeId", PayrollLWFCode.LWFCodeId);
                param.Add("@p_LWFCodeId_Encrypted", PayrollLWFCode.LWFCodeId_Encrypted);
                param.Add("@p_CmpID", PayrollLWFCode.CmpID);
           //     param.Add("@p_LWFCodeBranchId", PayrollLWFCode.LWFCodeBranchId);
                param.Add("@p_LWFCode", PayrollLWFCode.LWFCode);
                param.Add("@p_LWFRemark", PayrollLWFCode.LWFRemark);
                param.Add("@p_LWFStateCode", PayrollLWFCode.LWFStateCode);
                param.Add("@p_ResponsiblePerson", PayrollLWFCode.ResponsiblePerson);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_LWFCode", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Statutory_LWFCode", "Setting_Statutory_LWFCode");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 98;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_LWFCodeId_Encrypted", "List");
                var data = DapperORM.ReturnList<dynamic>("sp_List_Payroll_LWFCode", param).ToList();
                ViewBag.GetStatutoryLWFCodeList = data;
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
        public ActionResult Delete(string LWFCodeId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_LWFCodeId_Encrypted", LWFCodeId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_LWFCode", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Statutory_LWFCode");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
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