using Dapper;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Report_Payroll
{
    public class Rpt_Payroll_EmployeeWiseSalarySlipController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Reports/Rpt_Payroll_EmployeeWiseSalarySlip
        #region Rpt_Payroll_EmployeeWiseSalarySlip
        public ActionResult Rpt_Payroll_EmployeeWiseSalarySlip(EmployeeWiseSalarySlip objEmployeeWiseSalarySlip)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 580;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                if (objEmployeeWiseSalarySlip.FromDate != null)
                {
                    SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
                    var GetEmpoyee = "Select * from View_Onboarding_EmployeeList Where EmployeeNo = '" + objEmployeeWiseSalarySlip.EmployeeNo + "' and Deactivate=0";
                    var GetEmpoyeeAll = DapperORM.DynamicQuerySingle(GetEmpoyee);
                    decimal CmpId = GetEmpoyeeAll.CompanyId;
                    if (GetEmpoyeeAll != null)
                    {
                        ViewBag.GetUS80CEmployee = GetEmpoyeeAll;
                    }

                    //var Getmonth = Month?.ToString("yyyy-MM-dd");
                    DynamicParameters paramCompany = new DynamicParameters();
                    paramCompany.Add("@query", @"Select SalaryID,SalaryEmployeeName,SalaryEmployeeNo,SalaryDepartment,SalaryDesignation,SalaryNetPay,  replace( CONVERT(varchar(12), SalaryMonthYear, 106),' ','/') AS SalaryMonthYear,SalaryTotalGross,Mas_Branch.BranchName as BusinessUnit,SalaryTotalDeduction From Payroll_Salary,Mas_Branch Where  Mas_Branch.BranchId=Payroll_Salary.SalaryBranchId and Payroll_Salary.Deactivate=0 and  Payroll_Salary.SalaryEmployeeNo ='" + objEmployeeWiseSalarySlip.EmployeeNo + "'  AND SalaryMonthYear BETWEEN '" + objEmployeeWiseSalarySlip.FromDate?.ToString("yyyy-MM-dd") + "' and '" + objEmployeeWiseSalarySlip.ToDate?.ToString("yyyy-MM-dd") + "'  order by SalaryMonthYear");
                    var PayslipList = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramCompany).ToList();
                    ViewBag.GetPaySlipList = PayslipList;
                    if (PayslipList.Count == 0)
                    {
                        TempData["Message"] = "Record Not Found";
                        TempData["Icon"] = "info";
                    }

                    //SET COMPANY LOGO COMPANY WISE
                    var path = DapperORM.DynamicQuerySingle("Select Logo from Mas_CompanyProfile Where CompanyId = " + CmpId + "");
                    var SecondPath = path.Logo;
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='CompanyLogo'");
                    var FisrtPath = GetDocPath.DocInitialPath + CmpId + "\\";
                    string GetBase64 = null;
                    string fullPath = "";
                    fullPath = FisrtPath + SecondPath;

                    //string Extention = System.IO.Path.GetExtension(fullPath);
                    if (path.Logo != null)
                    {
                        try
                        {
                            string directoryPath = Path.GetDirectoryName(fullPath);
                            if (!Directory.Exists(directoryPath))
                            {
                                Session["PaySlipCompanyLogo"] = "";
                            }
                            else
                            {
                                using (Image image = Image.FromFile(fullPath))
                                {
                                    using (MemoryStream m = new MemoryStream())
                                    {
                                        image.Save(m, image.RawFormat);
                                        byte[] imageBytes = m.ToArray();

                                        // Convert byte[] to Base64 String
                                        string base64String = Convert.ToBase64String(imageBytes);
                                        Session.Remove("PaySlipCompanyLogo");
                                        Session["PaySlipCompanyLogo"] = "data:image; base64," + base64String;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message != null)
                            {
                                Session["CompanyLogo"] = "";
                            }
                        }
                    }

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

        #region Pay Slip Details Employee Wise 
        [HttpGet]
        public ActionResult GetPaySlipDetails(int SalaryID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters PersonalInfo = new DynamicParameters();
                PersonalInfo.Add("@p_SalaryID", SalaryID);
                var GetPersonal = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Payroll_Payslip_Personal", PersonalInfo).ToList();


                DynamicParameters DeductionInfo = new DynamicParameters();
                DeductionInfo.Add("@p_SalaryID", SalaryID);
                var GetDeduction = DapperORM.ReturnList<Payroll_Deduction_Info>("sp_Rpt_Payroll_PaySlip_Deduction", DeductionInfo).ToList();

                DynamicParameters AttendanceInfo = new DynamicParameters();
                AttendanceInfo.Add("@p_SalaryID", SalaryID);
                var GetAttendance = DapperORM.ReturnList<Payroll_Atten_Info>("sp_Rpt_Payroll_PaySlip_Attendance", AttendanceInfo).ToList();


                DynamicParameters EarningInfo = new DynamicParameters();
                EarningInfo.Add("@p_SalaryID", SalaryID);
                var GetEarning = DapperORM.ReturnList<Payroll_Earning_Info>("sp_Rpt_Payroll_PaySlip_Earnings", EarningInfo).ToList();


                return Json(new { GetPersonal = GetPersonal, GetEarning = GetEarning, GetDeduction = GetDeduction, GetAttendance = GetAttendance }, JsonRequestBehavior.AllowGet);
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