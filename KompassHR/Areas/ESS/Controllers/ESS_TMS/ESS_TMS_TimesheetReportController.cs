using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TMS;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_TMS
{
    public class ESS_TMS_TimesheetReportController : Controller
    {
        #region Main View
        [HttpGet]
       // GET: ESS/ESS_TMS_TimesheetReport
        public ActionResult ESS_TMS_TimesheetReport(TimeSheet_Report TimeSheet_Report)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 899;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdate = "Add";

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetCompanyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.CompanyName = GetCompanyName;
                var CmpId = GetCompanyName[0].Id;

                DynamicParameters paramClient = new DynamicParameters();
                paramClient.Add("@Query", "Select ClientID AS Id,ClientName As [Name] from TMS_Client Where Deactivate=0  And IsActive=1 Order by ClientName");
                var ClientName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramClient).ToList();

                ViewBag.ClientName = ClientName;

                DynamicParameters paramEmp = new DynamicParameters();
                paramEmp.Add("Query", "Select EmployeeId as Id,EmployeeName as Name from Mas_Employee where Deactivate=0 and EmployeeLeft=0 order By Name");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmp).ToList();

                ViewBag.EmployeeName = EmployeeName;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ViewData
        public ActionResult ViewData(int CmpId,int ClientId,int EmployeeId,DateTime MonthYear)
        {
          try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_CmpId", CmpId);
                param.Add("@p_ClientId", ClientId);
                param.Add("@p_EmployeeId", EmployeeId);
                param.Add("@p_MonthYear", MonthYear);
                var GetTimesheetData = DapperORM.ReturnList<dynamic>("sp_Rpt_Reports_TemplateReport", param).ToList();
                ViewBag.Getdata = GetTimesheetData;

                return View();

            }
            catch(Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Download Excel File
        [HttpPost]
        public ActionResult DownloadExcelFile(int? CmpId, int? ClientId, int? EmployeeId, DateTime MonthYear)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                    return RedirectToAction("Login", "Login");

                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("TimeSheetReport");
              
                    int currentRow = 1;

                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_CmpId", CmpId);
                    param.Add("@p_ClientId", ClientId);
                    param.Add("@p_EmployeeId", EmployeeId);
                    param.Add("@p_MonthYear", MonthYear);

                    //var data = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Reports_TemplateReportDownload", param).ToList();
                    var data = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Reports_TemplateReport", param).ToList();

                    if (data.Count == 0)
                        return Content("No data found for selected month.");

                    var header = data.First();

                    ws.Range(currentRow, 1, currentRow, 7).Merge();
                    ws.Cell(currentRow, 1).Value = "TimeSheet Report - (" + MonthYear.ToString("MMM-yyyy") + ")";
                    ws.Cell(currentRow, 1).Style.Font.Bold = true;
                    ws.Cell(currentRow, 1).Style.Font.FontSize = 16;
                    ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    currentRow += 2;

                    ws.Cell(currentRow, 2).Value = "Employee No";
                    ws.Cell(currentRow, 3).Value = header.EmployeeNo;
                    ws.Cell(currentRow, 5).Value = "Employee Name";
                    ws.Cell(currentRow, 6).Value = header.EmployeeName;
                    currentRow++;

                    ws.Cell(currentRow, 2).Value = "Billing Month";
                    ws.Cell(currentRow,3).Value = MonthYear.ToString("MMM-yyyy");
                    ws.Cell(currentRow, 5).Value = "Customer Location";
                    ws.Cell(currentRow, 6).Value = header.BranchName;

                    ws.Range(currentRow - 1, 1, currentRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(currentRow - 1, 1, currentRow, 7).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                

                    currentRow += 2;

                    ws.Range(currentRow, 1, currentRow, 7).Merge();
                    ws.Cell(currentRow, 1).Value = "Timesheet Information";
                    ws.Cell(currentRow, 1).Style.Font.Bold = true;
                    ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    currentRow++;

                    ws.Cell(currentRow, 1).Value = "Sr.No";
                    ws.Cell(currentRow, 2).Value = "Dates";
                    ws.Cell(currentRow, 3).Value = "Task Title";
                 
                    ws.Cell(currentRow, 4).Value = "In Out Status";
                    ws.Cell(currentRow, 5).Value = "In Time";
                    ws.Cell(currentRow, 6).Value = "Out Time";
                    ws.Cell(currentRow, 7).Value = "Duration";

                    var headerRange = ws.Range(currentRow, 1, currentRow, 7);

                    headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;


                    currentRow++;

                    int srNo = 1;

                    foreach (var item in data.Where(x => x.Dates != null && x.Dates != ""))
                    {
                        ws.Cell(currentRow, 1).Value = srNo++;
                        ws.Cell(currentRow, 2).Value = item.Dates;
                        ws.Cell(currentRow, 3).Value = item.TaskTitle;
                      
                        ws.Cell(currentRow, 4).Value = item.InOutStatus;
                        //ws.Cell(currentRow, 5).Value = item.InTime;
                        //ws.Cell(currentRow, 6).Value = item.OutTime;
                        ws.Cell(currentRow, 5).Value = "'" + item.InTime;   
                        ws.Cell(currentRow, 6).Value = "'" + item.OutTime;
                        ws.Cell(currentRow, 7).Value = "'" + item.Duration;
                       // ws.Cell(currentRow, 7).Value = item.Duration;

                        currentRow++;
                    }


                    ws.Range(currentRow - srNo + 1, 1, currentRow - 1, 7)
                      .Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(currentRow - srNo + 1, 1, currentRow - 1, 7)
                      .Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // ================= TOTAL DURATION ROW ===================
                    var totalRange = ws.Range(currentRow, 1, currentRow, 7);
                    totalRange.Merge();
                    totalRange.Value = "Total Duration: " + header.TotalDuration;

                    // Styling
                    totalRange.Style.Font.Bold = true;
                    totalRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    totalRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    totalRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);

                    // Borders (correct way for merged cells)
                    totalRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    totalRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    totalRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    totalRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    totalRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    currentRow += 2;



                    //var totalRange = ws.Range(currentRow - srNo + 1, 1, currentRow - 1, 7);
                    //totalRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //totalRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;


                    //ws.Range(currentRow - srNo + 1, 1, currentRow - 1, 7)
                    //  .Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //ws.Range(currentRow - srNo + 1, 1, currentRow - 1, 7)
                    //  .Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                 

                    ws.Cell(currentRow, 3).Value = "Sr.No";
                    ws.Cell(currentRow, 4).Value = "Day Classification";
                    ws.Cell(currentRow, 5).Value = "Days";

                    ws.Range(currentRow, 3, currentRow, 5).Style.Fill.BackgroundColor = XLColor.LightGray;
                    ws.Range(currentRow,3, currentRow, 5).Style.Font.Bold = true;
                    currentRow++;

                    ws.Cell(currentRow, 3).Value = 1;
                    ws.Cell(currentRow, 4).Value = "Holiday";
                    ws.Cell(currentRow, 5).Value = header.TotalPH;
                    currentRow++;

                    ws.Cell(currentRow, 3).Value = 2;
                    ws.Cell(currentRow, 4).Value = "Week Off";
                    ws.Cell(currentRow, 5).Value = header.TotalWO;
                    currentRow++;

                    ws.Cell(currentRow, 3).Value = 3;
                    ws.Cell(currentRow, 4).Value = "Total Present";
                    ws.Cell(currentRow, 5).Value = header.TotalPresent;
                    currentRow++;

                    ws.Cell(currentRow, 3).Value = 4;
                    ws.Cell(currentRow, 4).Value = "Total Absent";
                    ws.Cell(currentRow, 5).Value = header.TotalAbsent;

                    ws.Range(currentRow - 5, 3, currentRow, 5)
                      .Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Range(currentRow - 5, 3, currentRow, 5)
                      .Style.Border.InsideBorder = XLBorderStyleValues.Thin;



                    currentRow += 3; // space after previous table

                    // Header Row
                    ws.Cell(currentRow, 3).Value = "Approving Authority Name";
                    ws.Cell(currentRow, 4).Value = "Signature";
                    ws.Cell(currentRow, 5).Value = "Date";

                    var signHeader = ws.Range(currentRow, 3, currentRow, 5);

                    signHeader.Style.Fill.BackgroundColor = XLColor.LightGray;
                    signHeader.Style.Font.Bold = true;
                    signHeader.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    signHeader.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    signHeader.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    signHeader.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    currentRow++;

                    // Data Row
                    ws.Cell(currentRow, 3).Value = header.ResponsiblePerson;
                    ws.Cell(currentRow, 4).Value = ""; // Signature space
                    ws.Cell(currentRow, 5).Value = DateTime.Now.ToString("dd/MM/yyyy");

                    var signData = ws.Range(currentRow, 3, currentRow, 5);
                    signData.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    signData.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    signData.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Columns().AdjustToContents();
                  //  ws.Column(3).Width = 20;



                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;

                        return File(
                            stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "TimeSheetReport.xlsx"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                return Content("ERROR: " + ex.Message);
            }
        }

        #endregion


        //#region DownloadExcelFile
        //[HttpPost]
        //public ActionResult DownloadExcelFile(int? CmpId, int? ClientId, int? EmployeeId, DateTime MonthYear)
        //{
        //    try
        //    {
        //        // Check Session
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login");
        //        }

        //        var workbook = new XLWorkbook();
        //        var worksheet = workbook.Worksheets.Add("TimeSheetReport");
        //        worksheet.SheetView.FreezeRows(2);

        //        DataTable dt = new DataTable();

        //        DynamicParameters param = new DynamicParameters();
        //        param.Add("@p_CmpId", CmpId);
        //        param.Add("@p_ClientId", ClientId);
        //        param.Add("@p_EmployeeId", EmployeeId);
        //        param.Add("@p_MonthYear", MonthYear);

        //        // Get SP Data
        //        var GetData = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Reports_TemplateReportDownload", param).ToList();

        //        if (GetData.Count == 0)
        //        {
        //            return Content("No data found for selected month.");
        //        }

        //        // Convert List to DataTable
        //        DapperORM dprObj = new DapperORM();
        //        dt = dprObj.ConvertToDataTable(GetData);

        //        // Dynamic merge based on column count
        //        worksheet.Range(1, 1, 1, dt.Columns.Count).Merge();

        //        // Insert DataTable
        //        worksheet.Cell(2, 1).InsertTable(dt, false);

        //        // Styling
        //        var usedRange = worksheet.RangeUsed();
        //        usedRange.Style.Fill.BackgroundColor = XLColor.White;
        //        usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        //        usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        //        usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        //        usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
        //        usedRange.Style.Font.FontSize = 10;

        //        // Header Title
        //        worksheet.Cell(1, 1).Value =
        //            "Employee Wise TMS Report - (" + MonthYear.ToString("MMM-yyyy") + ")";

        //        worksheet.Cell(1, 1).Style.Font.Bold = true;
        //        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

        //        // Header Row Color (Row 2)
        //        var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
        //        headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
        //        headerRange.Style.Font.Bold = true;
        //        headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);

        //        worksheet.Columns().AdjustToContents();

        //        // Export Excel
        //        using (var stream = new MemoryStream())
        //        {
        //            workbook.SaveAs(stream);
        //            stream.Position = 0;
        //            return File(
        //                stream.ToArray(),
        //                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //                "TimeSheetReport.xlsx"
        //            );
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // TEMPORARY — shows exact error
        //        return Content("ERROR: " + ex.Message);
        //    }
        //}

        //#endregion



    }
}