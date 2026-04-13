using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Module.Models.Module_Payroll;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Payroll
{
    public class Module_Payroll_YearlyBonusPaidController : Controller
    {
        #region Module_Payroll_YearlyBonusPaid Main View
        // GET: Module/Module_Payroll_YearlyBonusPaid
        [HttpGet]
        public ActionResult Module_Payroll_YearlyBonusPaid(int ? CmpId,int ? BranchId,int ? FinantialYear, string Month)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                // Check access permissions
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 787;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                var CMPID = GetComapnyName[0].Id;

                DynamicParameters param = new DynamicParameters();
                //param.Add("@query", "SELECT BonusYearId As Id, CONCAT(YEAR(FromYear), '-', YEAR(ToYear)) AS Name FROM Payroll_BonusYear");
                param.Add("@query", "Select TaxFyearId as Id,TaxYear As Name  from IncomeTax_Fyear where Deactivate=0 and [IsActive]=1 Order By Name");
                var MonthYear1 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetMonthYear = MonthYear1;
                
                if (CmpId != null)
                {
                    DynamicParameters paramBranch = new DynamicParameters();
                    paramBranch.Add("@query", "select Distinct BranchId as Id,BranchName as Name from Mas_Branch where Deactivate=0 AND CmpId='" + CmpId + "';");
                    var branchList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranch).ToList();
                    ViewBag.GetBranchName = branchList;
                }
                else
                {
                    ViewBag.GetBranchName = new List<AllDropDownBind>();
                }
                
                //ViewBag.GetBranchName = "";
               // YearlyBonusPaid Employee = new YearlyBonusPaid();

                if (CmpId != null && BranchId!=null && FinantialYear != null)
                {
                    DynamicParameters paramEmp = new DynamicParameters();
                   paramEmp.Add("@p_FinantialYear", FinantialYear);
                    paramEmp.Add("@p_CmpId", CmpId);
                    paramEmp.Add("@p_BranchId", BranchId);
                    paramEmp.Add("@p_Month", Month);
                    var data = DapperORM.ReturnList<PayrollBonus>("sp_List_YearlyBonusEmployee", paramEmp).ToList();
                    //var data = DapperORM.DynamicList("sp_List_YearlyBonusEmployee", paramEmp);
                    ViewBag.GetEmployeeList = data;
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
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select Distinct BranchId as Id,BranchName as Name from Mas_Branch where Deactivate=0 AND CmpId='" + CmpId + "';");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
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
        public ActionResult SaveUpdate(int?CmpId, int ? BranchId,int? FinantialYear,string PaidMonth,List<EmployeeBonusData> EmployeeData)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                string Message = "";
                string Icon = "";

                foreach (var employee in EmployeeData)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_process", string.IsNullOrEmpty(employee.BonusId_Encrypted) ? "Save" : "Update");
                    param.Add("@p_BonusId_Encrypted", employee.BonusId_Encrypted); 
                    param.Add("@p_CmpId", CmpId);
                    param.Add("@p_BranchId", BranchId);
                    param.Add("@p_BonusEmpId", employee.EmployeeId);
                    param.Add("@p_FinantialYear", FinantialYear);
                    param.Add("@p_Amount", employee.Amount);
                    param.Add("@p_Remark", employee.Remark);
                    param.Add("@p_PaidRemark", employee.PaidRemark);
                   // param.Add("@p_PaidRemark", employee.PaidRemark); 
                    param.Add("@p_PaidMonth", PaidMonth); 
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_PayrollBonus", param);
                    Message = param.Get<string>("@p_msg");
                    Icon = param.Get<string>("@p_Icon");
                    
                }
                return Json(new{success = true,message = Message,icon = Icon});
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
        
        #region download 
        [HttpPost]
        public ActionResult DownloadExcelFile(List<PayrollBonus> TableData)
        {
           
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("YearlyBonus");
            worksheet.Cell(1, 1).Value = "Employee No";
            worksheet.Cell(1, 2).Value = "Employee Name";
            worksheet.Cell(1, 3).Value = "Amount";
            worksheet.Cell(1, 4).Value = "Paid Remark";
      
            for (int i = 0; i < TableData.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = TableData[i].EmployeeNo;
                worksheet.Cell(i + 2, 2).Value = TableData[i].EmployeeName;
                worksheet.Cell(i + 2, 3).Value = TableData[i].Amount;
                worksheet.Cell(i + 2, 4).Value = TableData[i].PaidRemark;
            }

            worksheet.Columns().AdjustToContents();
            var usedRange = worksheet.RangeUsed();
            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Font.FontName = "Calibri";
            usedRange.Style.Font.FontSize = 10;

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return File(
                    stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "YearlyBonus.xlsx"
                );
            }
        }
        #endregion

 

    }
}