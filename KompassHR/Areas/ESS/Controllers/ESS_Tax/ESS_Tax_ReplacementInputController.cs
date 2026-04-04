using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Tax;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Tax
{
    public class ESS_Tax_ReplacementInputController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_Tax_ReplacementInput
        #region ESS_Tax_ReplacementInput
        public ActionResult ESS_Tax_ReplacementInput(string MonthlyTaxId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 548;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "Select TaxFyearId as Id,TaxYear As Name  from IncomeTax_Fyear where Deactivate=0 and [IsActive]=1");
                var GetTaxFyear = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param).ToList();
                ViewBag.GetTaxFyear = GetTaxFyear;

                param = new DynamicParameters();
                param.Add("@query", "Select EmployeeId as Id,Concat(EmployeeName ,'-',EmployeeNo) As Name  from Mas_Employee where Deactivate=0 and ContractorID=1");
                var GetEmployeeList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param).ToList();
                ViewBag.GetEmployeeList = GetEmployeeList;
                IncomeTax_MonthlyTax Incometax_MonthlyTax = new IncomeTax_MonthlyTax();
                if (MonthlyTaxId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_MonthlyTaxId_Encrypted", MonthlyTaxId_Encrypted);
                    Incometax_MonthlyTax = DapperORM.ReturnList<IncomeTax_MonthlyTax>("sp_List_IncomeTax_MonthlyTax", param).FirstOrDefault();
                    TempData["MonthlyTaxMonthYear"] = Incometax_MonthlyTax.MonthlyTaxMonthYear.ToString("yyyy-MM"); ;
                }
                return View(Incometax_MonthlyTax);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion ESS_Tax_ReplacementInput

        #region IsMonthlyTaxExists
        [HttpGet]
        public ActionResult IsMonthlyTaxExists(string MonthlyTaxFyearId, string MonthlyTaxId_Encrypted,DateTime MonthlyTaxMonthYear,string MonthlyTaxEmployeeId)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_MonthlyTaxFyearId", MonthlyTaxFyearId);
                    param.Add("@p_MonthlyTaxId_Encrypted", MonthlyTaxId_Encrypted);
                    param.Add("@p_MonthlyTaxMonthYear", MonthlyTaxMonthYear);
                    param.Add("@p_MonthlyTaxEmployeeId", MonthlyTaxEmployeeId);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_MonthlyTax", param);
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
        public ActionResult SaveUpdate(IncomeTax_MonthlyTax ReplacementInput)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(ReplacementInput.MonthlyTaxId_Encrypted) ? "Save" : "Update");
                param.Add("@p_MonthlyTaxId_Encrypted", ReplacementInput.MonthlyTaxId_Encrypted);
                param.Add("@p_MonthlyTaxFyearId", ReplacementInput.MonthlyTaxFyearId);
                param.Add("@p_MonthlyTaxEmployeeId", ReplacementInput.MonthlyTaxEmployeeId);
                param.Add("@p_MonthlyTaxMonthYear", ReplacementInput.MonthlyTaxMonthYear);
                param.Add("@p_MonthlyTaxGross", ReplacementInput.MonthlyTaxGross);
                param.Add("@p_MonthlyTaxAmount", ReplacementInput.MonthlyTaxAmount);
                param.Add("@p_MonthlyTaxBaseTax", ReplacementInput.MonthlyTaxBaseTax);

                param.Add("@p_MonthlyTaxSurcharge", ReplacementInput.MonthlyTaxSurcharge);
                param.Add("@p_MonthlyTaxEducation", ReplacementInput.MonthlyTaxEducation);
                param.Add("@p_MonthlyTaxRemark", ReplacementInput.MonthlyTaxRemark);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter

                var Result = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_MonthlyTax", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                return RedirectToAction("ESS_Tax_ReplacementInput", "ESS_Tax_ReplacementInput");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion SaveUpdate

        #region GetList
        public ActionResult GetList(DateTime? MonthlyTaxMonthYear)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 548;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_MonthlyTaxId_Encrypted", "List");
                param.Add("@p_MonthYear", MonthlyTaxMonthYear);
                var data = DapperORM.DynamicList("sp_List_IncomeTax_MonthlyTax", param);
                ViewBag.GetReplacementInputList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion GetList
    }
}