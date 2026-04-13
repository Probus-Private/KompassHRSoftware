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
    public class ESS_Tax_PerquisiteAmountController : Controller
    {
        // GET: ESS/ESS_Tax_PerquisiteAmount
        DynamicParameters param = new DynamicParameters();


        #region ESS_Tax_PerquisiteAmount
        public ActionResult ESS_Tax_PerquisiteAmount(string PerquisiteAmountId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 527;
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
                param.Add("@query", "Select EmployeeId as Id,EmployeeName As Name  from Mas_Employee where Deactivate=0 and ContractorID=1");
                var GetEmployeeList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param).ToList();
                ViewBag.GetEmployeeList = GetEmployeeList;

                 param = new DynamicParameters();
                param.Add("@query", "Select PerquisiteListId as Id,PerquisiteName As Name  from IncomeTax_Perquisite where Deactivate=0");
                var GetPerquisiteList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param).ToList();
                ViewBag.GetPerquisiteList = GetPerquisiteList;
                IncomeTax_PerquisiteAmount Incometax_prequisiteAmount = new IncomeTax_PerquisiteAmount();
                if (PerquisiteAmountId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_PerquisiteAmountId_Encrypted", PerquisiteAmountId_Encrypted);
                    Incometax_prequisiteAmount = DapperORM.ReturnList<IncomeTax_PerquisiteAmount>("sp_List_IncomeTax_PerquisiteAmount", param).FirstOrDefault();
                    TempData["MonthYear"] = Incometax_prequisiteAmount.MonthYear.ToString("yyyy-MM"); ;
                }
                return View(Incometax_prequisiteAmount);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion ESS_Tax_PerquisiteAmount

        #region IsPerquisiteExist
        [HttpGet]
        public ActionResult IsPerquisiteExist(string PerquisiteAmountId_Encrypted, string PerquisiteFyearId,string PerquisiteEmployeeId,string PerquisiteListId)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_PerquisiteAmountId_Encrypted", PerquisiteAmountId_Encrypted);
                    param.Add("@p_PerquisiteFyearId", PerquisiteFyearId);
                    param.Add("@p_PerquisiteEmployeeId", PerquisiteEmployeeId);
                    param.Add("@p_PerquisiteListId", PerquisiteListId);

                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_PerquisiteAmount", param);
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
        public ActionResult SaveUpdate(IncomeTax_PerquisiteAmount PerquisiteAmount)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(PerquisiteAmount.PerquisiteAmountId_Encrypted) ? "Save" : "Update");
                param.Add("@p_PerquisiteAmountId_Encrypted", PerquisiteAmount.PerquisiteAmountId_Encrypted);
                param.Add("@p_PerquisiteFyearId", PerquisiteAmount.PerquisiteFyearId);
                param.Add("@p_PerquisiteEmployeeId", PerquisiteAmount.PerquisiteEmployeeId);
                param.Add("@p_PerquisiteListId", PerquisiteAmount.PerquisiteListId);
                param.Add("@p_TotalAmount", PerquisiteAmount.TotalAmount);
                param.Add("@p_TaxableAmount", PerquisiteAmount.TaxableAmount);
                param.Add("@p_MonthYear", PerquisiteAmount.MonthYear);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_PerquisiteAmount", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                return RedirectToAction("ESS_Tax_PerquisiteAmount", "ESS_Tax_PerquisiteAmount");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion SaveUpdate

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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 527;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_PerquisiteAmountId_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_IncomeTax_PerquisiteAmount", param);
                ViewBag.GetPerquisiteAmountList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion GetList

        #region Delete
        public ActionResult Delete(string PerquisiteAmountId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_PerquisiteAmountId_Encrypted", PerquisiteAmountId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_PerquisiteAmount", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_Tax_PerquisiteAmount");
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