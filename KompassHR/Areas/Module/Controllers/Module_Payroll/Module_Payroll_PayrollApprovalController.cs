using Dapper;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Payroll
{
    public class Module_Payroll_PayrollApprovalController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();

        // GET: Module/Module_Payroll_PayrollApproval
        #region Main View 
        public ActionResult Module_Payroll_PayrollApproval(PayrollApproval OBJPayrollApproval)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 475;
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


                if (OBJPayrollApproval.Month != null)
                {
                    var GetBranch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(OBJPayrollApproval.CmpId), Convert.ToInt32(Session["EmployeeId"]));
                    ViewBag.BranchName = GetBranch;
                    if (OBJPayrollApproval.BranchId != 0)
                    {
                        var Getmonth = OBJPayrollApproval.Month?.ToString("yyyy-MM-dd");
                        DynamicParameters paramCompany = new DynamicParameters();
                        paramCompany.Add("@query", $@"Select SalaryID,ShowInESS,concat(Mas_Employee.EmployeeName ,' - ',Mas_Employee.EmployeeNo) as EmployeeName,SalaryDepartment,SalaryDesignation, SalaryGrade from Payroll_Salary
                                                Inner join Mas_Employee On Mas_Employee.EmployeeId= Payroll_Salary.SalaryEmployeeId where Payroll_Salary.Deactivate=0 And Payroll_Salary.SalaryCmpId={OBJPayrollApproval.CmpId} And Payroll_Salary.SalaryBranchId={OBJPayrollApproval.BranchId} And ShowInESS='{OBJPayrollApproval.Type}' and (month(Payroll_Salary.SalaryMonthYear)=month('" + Getmonth + "') and year(Payroll_Salary.SalaryMonthYear)=year('" + Getmonth + "'))");
                        var LoadEmployee = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramCompany).ToList();
                        ViewBag.GetLoadEmployee = LoadEmployee;
                    }
                    else
                    {
                        var Getmonth = OBJPayrollApproval.Month?.ToString("yyyy-MM-dd");
                        DynamicParameters paramCompany = new DynamicParameters();
                        paramCompany.Add("@query", $@"Select SalaryID,ShowInESS,concat(Mas_Employee.EmployeeName ,' - ',Mas_Employee.EmployeeNo) as EmployeeName,SalaryDepartment,SalaryDesignation, SalaryGrade from Payroll_Salary
                                                Inner join Mas_Employee On Mas_Employee.EmployeeId= Payroll_Salary.SalaryEmployeeId where Payroll_Salary.Deactivate=0  And Payroll_Salary.SalaryCmpId ='{OBJPayrollApproval.CmpId}' and Payroll_Salary.SalaryBranchId in  (Select BranchID from UserBranchMapping where EmployeeID={Session["EmployeeId"]} and IsActive=1) And ShowInESS='{OBJPayrollApproval.Type}'  and (month(Payroll_Salary.SalaryMonthYear)=month('" + Getmonth + "') and year(Payroll_Salary.SalaryMonthYear)=year('" + Getmonth + "'))");
                        var LoadEmployee = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramCompany).ToList();
                        ViewBag.GetLoadEmployee = LoadEmployee;
                    }


                    //PayrollApproval obj = new PayrollApproval
                    //{
                    //    CmpId = CmpId ?? 1,
                    //    BranchId = BranchId ?? 1,
                    //    Month = Month ?? null,
                    //    Type = Type ?? "0"
                    //};
                    return View(OBJPayrollApproval);
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

        public class clsApproval
        {
            public double SalaryID { get; set; }
            public bool IsChecked { get; set; }
        }
        #region Approval
        public ActionResult Approval(List<clsApproval> ObjApproval, string Type)
        {
            try
            {
                StringBuilder strBuilder = new StringBuilder();
                if (ObjApproval != null)
                {
                    foreach (var Data in ObjApproval)
                    {
                        if (Type == "Pending to Approve")
                        {
                            string StringApproval = "UPDATE Payroll_Salary SET ShowInESS = 1, ShowInESS_ApproverName = '" + Session["EmployeeName"] + "', ShowInESSDate = GETDATE() WHERE SalaryID = " + Data.SalaryID;
                            strBuilder.Append(StringApproval);
                        }
                        else if (Type == "Approved to Pending")
                        {
                            string StringApproval = "UPDATE Payroll_Salary SET ShowInESS = 0, ShowInESS_ApproverName = '" + Session["EmployeeName"] + "', ShowInESSDate = GETDATE() WHERE SalaryID = " + Data.SalaryID;
                            strBuilder.Append(StringApproval);
                        }
                    }
                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {
                        TempData["Message"] = "Record save successfully";
                        TempData["Icon"] = "success";
                    }

                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
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