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
    public class Setting_Payroll_OtherDeductionCategoryController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_Payroll_OtherDeductionCategory
        #region Setting_Payroll_OtherDeductionCategory
        public ActionResult Setting_Payroll_OtherDeductionCategory(string OtherDeductionId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 465;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Payroll_OtherDeduction OBJOtherDeduction = new Payroll_OtherDeduction();

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetComapnyName = GetComapnyName;
                ViewBag.GetVariableName = "";

                if (OtherDeductionId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_OtherDeductionID_Encrypted", OtherDeductionId_Encrypted);
                    OBJOtherDeduction = DapperORM.ReturnList<Payroll_OtherDeduction>("sp_List_Payroll_OtherDeduction", paramList).FirstOrDefault();

                    DynamicParameters paramName = new DynamicParameters();
                    paramName.Add("@p_CmpId", OBJOtherDeduction.CmpId);
                    ViewBag.GetVariableName = DapperORM.ReturnList<PayrollDropDownBind>("sp_GetVariableHead_Deduction", paramName).ToList();
                }
                return View(OBJOtherDeduction);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region IsOtherDeductionCategoryExists
        [HttpGet]
        public ActionResult IsOtherDeductionCategoryExists(int CmpId, string VariableName, string DeductionCategory, string OtherDeductionId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_CmpId", CmpId);
                param.Add("@p_VariableName", VariableName);
                param.Add("@p_DeductionCategory", DeductionCategory);
                param.Add("@p_OtherDeductionId_Encrypted", OtherDeductionId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_OtherDeduction", param);
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
        public ActionResult SaveUpdate(Payroll_OtherDeduction _OtherDeduction)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(_OtherDeduction.OtherDeductionId_Encrypted) ? "Save" : "Update");
                param.Add("@p_OtherDeductionId_Encrypted", _OtherDeduction.OtherDeductionId_Encrypted);
                param.Add("@p_CmpId", _OtherDeduction.CmpId);
                param.Add("@p_VariableName", _OtherDeduction.VariableName);
                param.Add("@p_DeductionCategory", _OtherDeduction.DeductionCategory);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_OtherDeduction", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Payroll_OtherDeductionCategory", "Setting_Payroll_OtherDeductionCategory");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 465;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_OtherDeductionID_Encrypted", "List");
                var result = DapperORM.ReturnList<dynamic>("sp_List_Payroll_OtherDeduction", param).ToList();
                ViewBag.Get_OtherDeduction = result;
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
        public ActionResult Delete(string OtherDeductionID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_OtherDeductionID_Encrypted", OtherDeductionID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_OtherDeduction", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Payroll_OtherDeductionCategory");
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
                var data = DapperORM.ReturnList<PayrollDropDownBind>("sp_GetVariableHead_Deduction", paramName).ToList();
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