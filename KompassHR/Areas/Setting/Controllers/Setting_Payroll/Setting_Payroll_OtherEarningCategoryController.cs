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
    public class Setting_Payroll_OtherEarningCategoryController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_Payroll_OtherEarningCategory
        #region Setting_Payroll_OtherEarningCategory
        public ActionResult Setting_Payroll_OtherEarningCategory(string OtherEarningId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 467;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Payroll_OtherEarning OBJOtherEarning = new Payroll_OtherEarning();

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetComapnyName = GetComapnyName;
                ViewBag.GetVariableName = "";

                if (OtherEarningId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_OtherEarningId_Encrypted", OtherEarningId_Encrypted);
                    OBJOtherEarning = DapperORM.ReturnList<Payroll_OtherEarning>("sp_List_Payroll_OtherEarning", paramList).FirstOrDefault();

                    DynamicParameters paramName = new DynamicParameters();
                    paramName.Add("@p_CmpId", OBJOtherEarning.CmpId);
                    ViewBag.GetVariableName = DapperORM.ReturnList<PayrollDropDownBind>("sp_GetVariableHead_Earning", paramName).ToList();
                }
                return View(OBJOtherEarning);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsOtherEarningCategoryExists
        [HttpGet]
        public ActionResult IsOtherEarningCategoryExists(int CmpId, string VariableName, string EarningCategory, string OtherEarningId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_CmpId", CmpId);
                param.Add("@p_VariableName", VariableName);
                param.Add("@p_EarningCategory", EarningCategory);
                param.Add("@p_OtherEarningId_Encrypted", OtherEarningId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_OtherEarning", param);
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
        public ActionResult SaveUpdate(Payroll_OtherEarning _OtherEarning)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(_OtherEarning.OtherEarningId_Encrypted) ? "Save" : "Update");
                param.Add("@p_OtherEarningId_Encrypted", _OtherEarning.OtherEarningId_Encrypted);
                param.Add("@p_CmpId", _OtherEarning.CmpId);
                param.Add("@p_VariableName", _OtherEarning.VariableName);
                param.Add("@p_EarningCategory", _OtherEarning.EarningCategory);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_OtherEarning", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Payroll_OtherEarningCategory", "Setting_Payroll_OtherEarningCategory");
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
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 467;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_OtherEarningId_Encrypted", "List");
                var result = DapperORM.ReturnList<dynamic>("sp_List_Payroll_OtherEarning", param).ToList();
                ViewBag.Get_OtherEarning = result;
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
        public ActionResult Delete(string OtherEarningId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_OtherEarningId_Encrypted", OtherEarningId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_OtherEarning", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Payroll_OtherEarningCategory");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetVariableName
        [HttpGet]
        public ActionResult GetVariableName(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramName = new DynamicParameters();
                paramName.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<PayrollDropDownBind>("sp_GetVariableHead_Earning", paramName).ToList();
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