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

namespace KompassHR.Areas.Reports.Controllers.Reports_Onboarding
{
    public class Reports_Onboarding_AttributionVsOnboardingReportController : Controller
    {
        // GET: Reports/Reports_Onboarding_AttributionVsOnboardingReport
        public ActionResult Reports_Onboarding_AttributionVsOnboardingReport(MonthWiseFilter MonthWiseFilter)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 702;
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

                return View();
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

        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, DateTime FromMonthYear, DateTime ToMonthYear)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("AttritionVsOnboardingReport");
                worksheet.Range(1, 1, 1, 7).Merge();
                worksheet.SheetView.FreezeRows(2); // Freeze the first row
                DataTable dt = new System.Data.DataTable();
                List<dynamic> data = new List<dynamic>();


                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                paramList.Add("@p_CompanyId", CmpId);
                paramList.Add("@p_BranchId", BranchId);
                paramList.Add("@p_FromMonthYear", FromMonthYear);
                paramList.Add("@p_ToMonthYear", ToMonthYear);
                var GetDailyAttendanceReport = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Onboarding_AttritionVsOnboardingReport", paramList).ToList();
                if (GetDailyAttendanceReport.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }
                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(GetDailyAttendanceReport);
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
                worksheet.Cell(1, 1).Value = "Attrition Vs Onboarding Report - (" + FromMonthYear.ToString("dd/MMM/yyyy") + " to " + ToMonthYear.ToString("dd/MMM/yyyy") + ")";
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
                ex.Message.ToString();
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

    }
}