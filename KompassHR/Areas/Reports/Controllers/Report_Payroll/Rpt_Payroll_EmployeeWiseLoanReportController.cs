using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Report_Payroll
{
    public class Rpt_Payroll_EmployeeWiseLoanReportController : Controller
    {
        // GET: Reports/Rpt_Payroll_BranchUnitWiseSummary
        #region Main view

        public ActionResult Rpt_Payroll_EmployeeWiseLoanReport(EmployeeWiseLoanReportFilter EmployeeWiseFilter)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 889;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                EmployeeWiseLoanReportFilter EmployeeWiseFilter1 = new EmployeeWiseLoanReportFilter();

                //GET COMPANY NAME
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                var CMPID = GetComapnyName[0].Id;
                
                
                //DynamicParameters paramEmpName = new DynamicParameters();
                //paramEmpName.Add("@query", "select distinct Payroll_Loan.LoanEmployeeID as Id, Concat(Mas_Employee.EmployeeName ,' - ', Mas_Employee.EmployeeNo) as Name from Payroll_Loan inner join Mas_Employee on Mas_Employee.EmployeeId = Payroll_Loan.LoanEmployeeID where Payroll_Loan.Deactivate = 0 and Payroll_Loan.CmpID = '" + CMPID +"' Order by Name");
                //ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();

                if (EmployeeWiseFilter1.CmpId != 0)
                {
                    DynamicParameters paramBranch = new DynamicParameters();
                    paramBranch.Add("@query", "select Distinct BranchId as Id,BranchName as Name from Mas_Branch where Deactivate=0 AND CmpId='" + EmployeeWiseFilter1.CmpId + "';");
                    var branchList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranch).ToList();
                    ViewBag.BranchName = branchList;

                    DynamicParameters paramYear = new DynamicParameters();
                    paramYear.Add("@query", "select Fid As id,AssessmentYear as Name  from  Tool_FinancialYear where CompanyId='" + EmployeeWiseFilter1.CmpId + "';");
                    var YearList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramYear).ToList();
                    ViewBag.YearName = YearList;
                }
                else
                {
                    ViewBag.BranchName = new List<AllDropDownBind>();
                    ViewBag.YearName = new List<AllDropDownBind>();
                }

                if (EmployeeWiseFilter1.CmpId != 0)
                {
                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select distinct Payroll_Loan.LoanEmployeeID as Id, Concat(Mas_Employee.EmployeeName ,' - ', Mas_Employee.EmployeeNo) as Name from Payroll_Loan inner join Mas_Employee on Mas_Employee.EmployeeId = Payroll_Loan.LoanEmployeeID where Payroll_Loan.Deactivate = 0 and Payroll_Loan.CmpID = '" + CMPID + "' Order by Name");
                ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                }
                else
                {
                    ViewBag.EmployeeName = new List<AllDropDownBind>();
                }

                return View();
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
                param.Add("@query", "Select BranchId as Id,BranchName as Name from Mas_Branch where Deactivate=0 AND CmpId='" + CmpId + "' Order By Name");
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.BranchName = Branch;

                DynamicParameters paramYear = new DynamicParameters();
                paramYear.Add("@query", "select Fid As Id,AssessmentYear as Name from  Tool_FinancialYear where  CompanyId='" + CmpId + "';");
                var YearList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramYear).ToList();
                ViewBag.YearName = YearList;
                
                DynamicParameters paramEmployee = new DynamicParameters();
                paramEmployee.Add("@query", "select distinct  Payroll_Loan.LoanEmployeeID as Id, Concat(Mas_Employee.EmployeeName ,' - ', Mas_Employee.EmployeeNo) as Name from Payroll_Loan inner join Mas_Employee on Mas_Employee.EmployeeId = Payroll_Loan.LoanEmployeeID where Payroll_Loan.Deactivate = 0 and Payroll_Loan.CmpID = '" + CmpId + "' Order by Name");
                var Employee = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmployee).ToList();
                ViewBag.EmployeeName = Employee;

                var data = new { Branch = Branch, YearList = YearList, Employee = Employee };

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region Get GetEmployee 
        [HttpGet]
        public ActionResult GetEmployee(int CmpId, int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select distinct  Payroll_Loan.LoanEmployeeID as Id, Concat(Mas_Employee.EmployeeName ,' - ', Mas_Employee.EmployeeNo) as Name from Payroll_Loan inner join Mas_Employee on Mas_Employee.EmployeeId = Payroll_Loan.LoanEmployeeID  Left join Mas_Branch on Mas_Branch.BranchId = Mas_Employee.EmployeeBranchId  where Payroll_Loan.Deactivate = 0 and Payroll_Loan.CmpID = '" + CmpId + "'  AND Mas_Branch.BranchId='" + BranchId + "' Order by Name");
                var Employee = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                var data = new {Employee = Employee };
                return Json(data, JsonRequestBehavior.AllowGet);
               // return Json(new { Employee = Employee }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ViewData
        [HttpGet]
        public ActionResult ViewData(int CmpId,int BranchId,int EmployeeId,int FromYear)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@P_CmpId", CmpId);
                param.Add("@P_BranchId", BranchId);
                param.Add("@P_FromDate", FromYear);
                param.Add("@P_EmployeeId", EmployeeId);
                var Data = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Payroll_EmployeeWiseLoanDetails", param).ToList();
                ViewBag.EmployeeData = Data;
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