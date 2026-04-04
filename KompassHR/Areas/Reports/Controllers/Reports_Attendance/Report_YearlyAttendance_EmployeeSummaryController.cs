using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
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

namespace KompassHR.Areas.Reports.Controllers.Reports_Attendance
{
    public class Report_YearlyAttendance_EmployeeSummaryController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        // GET: Reports/Report_YearlyAttendance_EmployeeSummary
        #region Report_YearlyAttendance_EmployeeSummary
        public ActionResult Report_YearlyAttendance_EmployeeSummary(DailyAttendanceReportFilter MasReport)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 481;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.CompanyName = GetComapnyName;
                var CMPID = GetComapnyName[0].Id;

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CMPID);
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                ViewBag.BranchName = Branch;

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_qry", " and Mas_ContractorMapping.BranchID =" + Branch[0].Id + "");
                var GetContractorDropdown = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param2).ToList();
                ViewBag.GetContractor = GetContractorDropdown;
                return View(MasReport);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int BranchId, DateTime FromDate, int? ContractorId,DateTime ToDate)
        {
            try
            {
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("YearlyAttendanceSummary");
                worksheet.Range(1, 1, 1, 25).Merge();
                worksheet.SheetView.FreezeRows(2); // Freeze the row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();
                var Query = "";
                if (ContractorId != null)
                {
                    Query = Query + " and Atten_InOut.InOutBranchId=" + BranchId + " and Atten_InOut.InOut_EmpContractorId=" + ContractorId + " and  YEAR(Atten_InOut.InOutMonthYear) BETWEEN " + FromDate.ToString("yyyy") + " AND " + ToDate.ToString("yyyy") + " ";
                }
                else
                {
                    Query = Query + " and Atten_InOut.InOutBranchId=" + BranchId + " and   YEAR(Atten_InOut.InOutMonthYear) BETWEEN " + FromDate.ToString("yyyy") + " AND " + ToDate.ToString("yyyy") + " ";

                    //Query = Query + " and Atten_InOut.InOutBranchId=" + BranchId + "  and  Atten_InOut.InOutMonthYear BETWEEN " + FromDate.ToString("yyyy-MM-dd") + " AND " + ToDate.ToString("yyyy-MM-dd") + " ";
                }
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@P_Qry", Query);
                data = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Atten_YearlyEmployeeSummary", paramList).ToList();
                if (data.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }
                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(data);
                worksheet.Cell(2, 1).InsertTable(dt, false);

                int totalRows = worksheet.RowsUsed().Count();

                // Set the background color to white and apply borders
                var usedRange = worksheet.RangeUsed();
                usedRange.Style.Fill.BackgroundColor = XLColor.White;
                usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Font.FontSize = 10;
                usedRange.Style.Font.FontColor = XLColor.Black;

                // Set the header row name
                worksheet.Cell(1, 1).Value = "Yearly Attendance Summary Report - (" + FromDate.ToString("MMM/yyyy") + ") - (" + ToDate.ToString("MMM/yyyy") + ")";
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Columns().AdjustToContents(); // This code for all clomns

                var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                headerRange.Style.Font.FontSize = 10;
                headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
                headerRange.Style.Font.Bold = true;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0; // Reset the stream position to the beginning

                    // Return the file to the client
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