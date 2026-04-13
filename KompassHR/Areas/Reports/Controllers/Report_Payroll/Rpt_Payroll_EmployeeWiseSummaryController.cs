using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Report_Payroll
{
    public class Rpt_Payroll_EmployeeWiseSummaryController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: Reports/Rpt_Payroll_EmployeeWiseSummary
        #region EmployeeWiseSummary
        public ActionResult Rpt_Payroll_EmployeeWiseSummary(EmployeeWiseReportFilter EmployeeWiseReportFilter)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 469;
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

                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CMPID), Convert.ToInt32(Session["EmployeeId"]));
                ViewBag.BranchName = Branch;

                //GET EMPLOYEE NAME
                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId=" + Branch[0].Id + " and EmployeeLeft=0");
                //paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and Mas_Employee.EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and Atten_InOut.InOutBranchId in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CMPID + ") and month( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("MM") + "' and year( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("yyyy") + "' and EmployeeId<>1) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CMPID + ") and( month(mas_employee.JoiningDate)<='" + DateTime.Now.Date.ToString("MM") + "' and year (mas_employee.JoiningDate)<='" + DateTime.Now.Date.ToString("yyyy") + "') and (month(LeavingDate)='" + DateTime.Now.Date.ToString("MM") + "' and year(mas_employee.LeavingDate)='" + DateTime.Now.Date.ToString("yyyy") + "' or mas_employee.LeavingDate is null) order by Name");
                ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetMonthlyBusinessUnit
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

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, DateTime FromMonth, DateTime ToMonth, int? EmployeeId)
        {
            try
            {

                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("PayrollEmployeeWiseSummary");
                worksheet.Range(1, 1, 1, 35).Merge();
                worksheet.SheetView.FreezeRows(9); // Freeze the header rows

                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();

                #region GetEmployee
                var GetEmployee = "Select * from View_IncomeTax_Employee Where EmployeeID = " + EmployeeId;
                var Employee = DapperORM.DynamicQuerySingle(GetEmployee);
                #endregion

                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EmployeeId", EmployeeId);
                paramList.Add("@p_FromMonthYear", FromMonth);
                paramList.Add("@p_ToMonthYear", ToMonth);
                var GetDailyAttendanceReport = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Payroll_EmployeeWiseSummary", paramList).ToList();

                if (GetDailyAttendanceReport.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }

                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(GetDailyAttendanceReport);
                worksheet.Cell(9, 1).InsertTable(dt, false);

                // Header section
                worksheet.Cell(1, 1).Value = Employee.CompanyName;
                worksheet.Cell(1, 1).Style.Font.SetBold(true);
                worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                worksheet.Range("A1:M1").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                worksheet.Range("A1:M1").Style.Border.OutsideBorder = XLBorderStyleValues.Medium;

                for (int i = 2; i <= 7; i++)
                {
                    worksheet.Range($"A{i}:B{i}").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                    worksheet.Range($"A{i}:B{i}").Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                }

                worksheet.Range("A2:A7").Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                worksheet.Range("A2:A7").Style.Font.Bold = true;
                worksheet.Range("A2:A7").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("B2:B7").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

                worksheet.Cell(2, 1).Value = "Employee Name";
                worksheet.Cell(2, 3).Value = Employee.EmployeeName;

                worksheet.Cell(3, 1).Value = "Employee No";
                worksheet.Cell(3, 3).SetValue<string>(Employee.EmployeeNo.ToString());


                worksheet.Cell(4, 1).Value = "Business Unit";
                worksheet.Cell(4, 3).Value = Employee.BranchName;

                worksheet.Cell(5, 1).Value = "Department";
                worksheet.Cell(5, 3).Value = Employee.DepartmentName;

                worksheet.Cell(6, 1).Value = "Designation";
                worksheet.Cell(6, 3).Value = Employee.DesignationName;

                worksheet.Cell(7, 1).Value = "Date OF Joining";
                worksheet.Cell(7, 3).Value = Employee.JoiningDate.ToString("dd/MMM/yyyy");

                // Right Section (C2:M7)
                for (int i = 2; i <= 7; i++)
                {
                    worksheet.Range($"C{i}:M{i}").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                    worksheet.Range($"C{i}:M{i}").Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                }

                // Section Header
                worksheet.Range("A8:M8").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                worksheet.Range("A8:M8").Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                worksheet.Cell(8, 1).Value = "Payroll Employee Wise Summary";
                worksheet.Cell(8, 1).Style.Font.SetBold(true);

                // Set background and borders for data
                int headerRow = 9;
                int dataRowCount = dt.Rows.Count;
                int totalRowCount = headerRow + dataRowCount; // including header row

                var usedRange = worksheet.Range(headerRow, 1, totalRowCount, dt.Columns.Count);
                usedRange.Style.Fill.BackgroundColor = XLColor.White;
                usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Font.FontSize = 10;
                usedRange.Style.Font.FontColor = XLColor.Black;

                // Header Row Style
                var headerRange = worksheet.Range(headerRow, 1, headerRow, dt.Columns.Count);
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                headerRange.Style.Font.FontSize = 10;
                headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
                headerRange.Style.Font.Bold = true;

                // Optional: Auto-fit columns
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report.xlsx");
                }

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