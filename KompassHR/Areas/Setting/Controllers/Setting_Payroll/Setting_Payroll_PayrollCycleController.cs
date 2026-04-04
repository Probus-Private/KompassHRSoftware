using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Payroll;
using KompassHR.Areas.Setting.Models.Setting_PolicyAndLibrary;
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
    public class Setting_Payroll_PayrollCycleController : Controller
    {
        // GET: Setting/Setting_Payroll_PayrollCycle
        public ActionResult Setting_Payroll_PayrollCycle(string PayrollCycleId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" }); 
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 285;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString)) 
                {
                    ViewBag.AddUpdateTitle = "Add";

                    DynamicParameters paramCompany = new DynamicParameters();
                    paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                    var GetCompanyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                    ViewBag.getCompanyName = GetCompanyName;

                 
                   Payroll_PayrollCycle PayrollCycle = new Payroll_PayrollCycle();
                    if(PayrollCycleId_Encrypted!=null)
                    {
                        DynamicParameters param1 = new DynamicParameters();
                        ViewBag.AddUpdateTitle = "Update";
                        param1.Add("@p_PayrollCycleId_Encrypted", "List");
                        param1.Add("@p_PayrollCycleId_Encrypted", PayrollCycleId_Encrypted);
                        PayrollCycle = DapperORM.ReturnList<Payroll_PayrollCycle>("sp_List_Payroll_PayrollCycle", param1).FirstOrDefault();
                        TempData["IsPeriodBasedDays"] = PayrollCycle.IsPeriodBasedDays;                              
                    }
                    return View(PayrollCycle);
                }
             
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        [HttpPost]
        public ActionResult SaveUpdate(Payroll_PayrollCycle payrollcycle)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", string.IsNullOrEmpty(payrollcycle.PayrollCycleId_Encrypted) ? "Save" : "Update");
                param.Add("@p_PayrollCycleId_Encrypted", payrollcycle.PayrollCycleId_Encrypted);
                param.Add("@p_PayrollCycleId", payrollcycle.PayrollCycleId);
                param.Add("@p_CmpID",payrollcycle.CmpID);
                param.Add("@p_PayrollCycleBranchId", Session["BranchId"]);
                if(payrollcycle.PayrollCycleType== "IsMonthDays")
                {
                    param.Add("@p_IsMonthDays",1);
                }else
                {
                    param.Add("@p_IsMonthDays", 0);
                }
                if (payrollcycle.PayrollCycleType == "IsPeriodBasedDays")
                {
                    param.Add("@p_IsPeriodBasedDays",1);
                }
                else
                {
                    param.Add("@p_IsPeriodBasedDays", 0);
                }           
                param.Add("@p_PeriodBasedFromDay", payrollcycle.PeriodBasedFromDay);
                param.Add("@p_PeriodBasedToDay", payrollcycle.PeriodBasedToDay);
                param.Add("@p_PayrollCycleFixedDays", payrollcycle.PayrollCycleFixedDays);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_PayrollCycle", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");

                return RedirectToAction("Setting_Payroll_PayrollCycle", "Setting_Payroll_PayrollCycle");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        public ActionResult GetList()
        {
            try
            {
                if(Session["EmployeeId"]==null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 285;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "List");
                var result = DapperORM.ReturnList<dynamic>("sp_List_Payroll_PayrollCycle", param).ToList();
                ViewBag.GetPayrollCycleList = result;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        public ActionResult Delete(string PayrollCycleId_Encrypted)
        {
            try
            {
                if(Session["EmployeeId"]==null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Delete");
                param.Add("@p_PayrollCycleId_Encrypted", PayrollCycleId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = DapperORM.ExecuteReturn("sp_SUD_Payroll_PayrollCycle", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                return RedirectToAction("Delete", "Setting_Payroll_PayrollCycle");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
    }
}