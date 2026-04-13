using Dapper;
using KompassHR.Areas.ESS.Models.ESS_LoanAndAdvance;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_LoanAndAdvance
{
    public class ESS_Advance_InstallmentChangeController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_Advance_InstallmentChange
        #region ESS_Advance_InstallmentChange
        public ActionResult ESS_Advance_InstallmentChange(Payroll_AdvanceInstallment OBJStop)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 660;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Payroll_AdvanceInstallment AdvanceInstallment = new Payroll_AdvanceInstallment();

                DynamicParameters param = new DynamicParameters();
                ViewBag.GetEmployee = DapperORM.ReturnList<AllDropDownBind>("sp_PayrollAdvanceInstallationEmployeeDropdown", param).ToList();

                ViewBag.GetEmployeeAdvanceList = "";
                if (OBJStop.AdvanceInstallmentID_Encrypted != null)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_AdvanceInstallmentID_Encrypted", OBJStop.AdvanceInstallmentID_Encrypted);
                    AdvanceInstallment = DapperORM.ReturnList<Payroll_AdvanceInstallment>("sp_List_Payroll_AdvanceInstallment", param1).FirstOrDefault();

                    DynamicParameters paramAdvance = new DynamicParameters();
                    paramAdvance.Add("@p_EmployeeId", AdvanceInstallment.AdvanceInstallmentEmployeeId);
                    ViewBag.GetEmployeeAdvanceList = DapperORM.ReturnList<AllDropDownBind>("sp_PayrollAdvanceDropdown", paramAdvance).ToList();

                    TempData["FromMonthYear"] = AdvanceInstallment.FromMonthYear.ToString("yyyy-MM");
                    TempData["ToMonthYear"] = AdvanceInstallment.ToMonthYear.ToString("yyyy-MM");
                }
                return View(AdvanceInstallment);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetEmployeeAdvance
        [HttpGet]
        public ActionResult GetEmployeeAdvance(int EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramAdvance1 = new DynamicParameters();
                paramAdvance1.Add("@p_EmployeeId", EmployeeId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_PayrollAdvanceDropdown", paramAdvance1).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 660;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_AdvanceInstallmentID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Payroll_AdvanceInstallment", param);
                ViewBag.GetAdvanceInstallmentChangeList = data;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_Advance_InstallmentChange");
            }
        }
        #endregion

        #region Ispayroll_AdvanceInstallmentExists
        public ActionResult Ispayroll_AdvanceInstallmentExists(string AdvanceInstallmentID_Encrypted, string AdvanceInstallmentEmployeeId, string AdvanceId, DateTime FromMonthYear, DateTime ToMonthYear,string AdvanceInstallmentAmount)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_AdvanceInstallmentID_Encrypted",AdvanceInstallmentID_Encrypted);
                    param.Add("@p_AdvanceInstallmentEmployeeId",AdvanceInstallmentEmployeeId);
                    param.Add("@p_AdvanceId", AdvanceId);
                    param.Add("@p_FromMonthYear",FromMonthYear);
                    param.Add("@p_ToMonthYear",ToMonthYear);
                    param.Add("@p_AdvanceInstallmentAmount",AdvanceInstallmentAmount);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_AdvanceInstallmentChange", param);
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
        public ActionResult SaveUpdate(Payroll_AdvanceInstallment payroll_AdvanceInstallment)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                param.Add("@p_process", string.IsNullOrEmpty(payroll_AdvanceInstallment.AdvanceInstallmentID_Encrypted) ? "Save" : "Update");
                param.Add("@p_AdvanceInstallmentID", payroll_AdvanceInstallment.AdvanceInstallmentID);
                param.Add("@p_AdvanceInstallmentID_Encrypted", payroll_AdvanceInstallment.AdvanceInstallmentID_Encrypted);
                param.Add("@p_AdvanceInstallmentEmployeeId", payroll_AdvanceInstallment.AdvanceInstallmentEmployeeId);
                param.Add("@p_AdvanceId", payroll_AdvanceInstallment.AdvanceId);
                param.Add("@p_FromMonthYear", payroll_AdvanceInstallment.FromMonthYear);
                param.Add("@p_ToMonthYear", payroll_AdvanceInstallment.ToMonthYear);
                param.Add("@p_AdvanceInstallmentAmount", payroll_AdvanceInstallment.AdvanceInstallmentAmount);
                param.Add("@p_Remark", payroll_AdvanceInstallment.Remark);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_AdvanceInstallmentChange", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_Advance_InstallmentChange", "ESS_Advance_InstallmentChange");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(int AdvanceInstallmentID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_AdvanceInstallmentID", AdvanceInstallmentID);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_AdvanceInstallmentChange", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_Advance_InstallmentChange");
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