using Dapper;
using KompassHR.Areas.Module.Models.Module_Payroll;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Report_Payroll
{
    public class Rpt_Payroll_ForPrintController : Controller
    {
        // GET: Reports/Rpt_Payroll_ForPrint
        DynamicParameters param = new DynamicParameters();
        public ActionResult Rpt_Payroll_ForPrint(MonthlyAttendance OBJList)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 282;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                //GET COMPANY NAME
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                var CMPID = GetComapnyName[0].Id;
                //GET BRANCH NAME
                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CMPID), Convert.ToInt32(Session["EmployeeId"]));
                ViewBag.BranchName = Branch;

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "Select SalarySheetId as Id,SalarySheetName as Name from Payroll_SalarySheet where Deactivate=0 order by isdefault desc");
                ViewBag.SalarySheet = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();

                if (OBJList.Month == DateTime.MinValue)
                {
                    if (Session["MonthYear"] != null)
                    {
                        param.Add("@p_Origin", "List");
                        param.Add("@p_EmployeeId", Session["EmployeeId"]);
                        //  param.Add("@p_CmpId", Session["CompanyId"]);
                        param.Add("@p_MonthYear", Session["MonthYear"]);
                        var data = DapperORM.ReturnList<SalaryMonthlyAttendance>("sp_List_Payroll_Salary", param).ToList();
                        ViewBag.GetMonthlyAttendance = data;
                        if (data.Count != 0)
                        {
                            TempData["MonthYear"] = data[0].SalaryMonthYear.ToString("yyyy-MM");
                        }
                        // TempData["MonthYear"] = data[0].AttenMonthlyMonthYear.ToString("yyyy-MM");
                    }
                    else
                    {
                        ViewBag.GetMonthlyAttendance = "";
                        TempData["MonthYear"] = null;
                    }
                }
                else
                {
                    param.Add("@p_Origin", "List");
                    param.Add("@p_EmployeeId", Session["EmployeeId"]);
                    //param.Add("@p_CmpId", Session["CompanyId"]);
                    param.Add("@p_MonthYear", OBJList.Month);
                    var data = DapperORM.ReturnList<SalaryMonthlyAttendance>("sp_List_Payroll_Salary", param).ToList();
                    ViewBag.GetMonthlyAttendance = data;
                    if (data.Count != 0)
                    {
                        TempData["MonthYear"] = data[0].SalaryMonthYear.ToString("yyyy-MM");
                    }

                }

                return View(OBJList);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region Get Branch Name
        [HttpGet]
        public ActionResult GetMonthlyBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CmpId), Convert.ToInt32(Session["EmployeeId"]));
                return Json(new { Branch = Branch }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetWagesSheetInvoice
        [HttpGet]
        public ActionResult GetWagesSheetInvoice(int? BranchId, string MonthYear, int? SalaryCmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 282;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_MonthYear", DateTime.Parse(MonthYear));
                param.Add("@p_BranchId", BranchId);
                param.Add("@p_CompanyId", SalaryCmpId);
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var Getdata = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Payroll_WageSheet_Invoice", param).ToList();
                var GetCompanyName = Getdata?.FirstOrDefault();
                TempData["CompanyName"] = GetCompanyName?.CompanyName ?? "";

                var GetSalaryMonthYear = Getdata?.FirstOrDefault();
                TempData["MonthYear"] = GetSalaryMonthYear?.SalaryMonthYear?.ToString("MMM/yyyy") ?? "";

                //TempData["CompanyName"] = Getdata[0].CompanyName;
                //TempData["MonthYear"] = Getdata[0].SalaryMonthYear.ToString("MMM/yyyy");
                ViewBag.Payroll_WageSheet = Getdata;
                return View();
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