using ClosedXML.Excel;
using Dapper;
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
    public class Rpt_Payroll_YearWise_LWFRegisterController : Controller
    {
        // GET: Reports/Rpt_Payroll_YearWise_LWFRegister
        #region Main View
        public ActionResult Rpt_Payroll_YearWise_LWFRegister()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 904;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                // GET COMPANY NAME
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
        #endregion

        #region Get Branch Name
        [HttpGet]
        public ActionResult GetBusinessUnit(int CmpId)
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
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, DateTime FromMonthYear, DateTime ToMonthYear)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("YearWiseLWF");
                worksheet.Range(1, 1, 1, 10).Merge();
                worksheet.SheetView.FreezeRows(2); // Freeze the first row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();

                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                paramList.Add("@p_CompanyId", CmpId);
                paramList.Add("@p_BranchId", BranchId);
                paramList.Add("@p_FromMonthYear", FromMonthYear);
                paramList.Add("@p_ToMonthYear", ToMonthYear);
                var GetData = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Payroll_YearWiseLWF", paramList).ToList();
                if (GetData.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }

                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(GetData);
                worksheet.Cell(2, 1).InsertTable(dt, false);
                int totalRows = worksheet.RowsUsed().Count();

                var lastRow = worksheet.Row(totalRows + 1);
                lastRow.Style.Font.Bold = true;
                // Additional styling if required
                // lastRow.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                lastRow.Style.Font.FontColor = XLColor.Black;
                lastRow.Style.Font.FontSize = 10;

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
                worksheet.Cell(1, 1).Value = "Year Wise LWF Register Report - (" + FromMonthYear.ToString("dd/MMM/yyyy") + " to " + ToMonthYear.ToString("dd/MMM/yyyy") + ")";
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

        #region Print
        public ActionResult Print(int? CmpId, int? BranchId, DateTime FromMonthYear, DateTime ToMonthYear)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                string companyName = "";
                if (CmpId.HasValue)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "SELECT CompanyName FROM Mas_CompanyProfile WHERE CompanyId = " + CmpId);

                    var result = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).SingleOrDefault();
                    companyName = result.CompanyName.ToString();
                }

                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                paramList.Add("@p_CompanyId", CmpId);
                paramList.Add("@p_BranchId", BranchId);
                paramList.Add("@p_FromMonthYear", FromMonthYear);
                paramList.Add("@p_ToMonthYear", ToMonthYear);
                var GetData = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Payroll_YearWiseLWFSummary", paramList).ToList();
                if (GetData.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }

                var firstRow = (IDictionary<string, object>)GetData.First();
                var html = new System.Text.StringBuilder();

                // Add a proper HTML document structure with <title>
                html.Append("<!DOCTYPE html><html><head>");
                html.Append($"<title>Year Wise Payroll LWF Register Report - ({FromMonthYear:MMM yyyy} - {ToMonthYear:MMM yyyy})</title>");
                html.Append("</head><body>");

                // Company Name (Centered & bold)
                if (!string.IsNullOrWhiteSpace(companyName))
                {
                    html.Append($"<h2 style='text-align:center;margin-bottom:5px;'>{companyName}</h2>");
                }

                // Report heading
                html.Append($"<h3 style='text-align:left;margin-bottom:10px;'>Year Wise Payroll LWF Register Report - ({FromMonthYear:MMM yyyy} - {ToMonthYear:MMM yyyy})</h3>");
                html.Append("<table border='1' style='border-collapse:collapse;width:100%;font-size:12px;'>");

                // Table header (skip CompanyName)
                html.Append("<thead><tr style='background-color:#cddcac;font-weight:bold;'>");
                foreach (var col in firstRow.Keys)
                {
                    if (!string.Equals(col, "CompanyName", StringComparison.OrdinalIgnoreCase))
                    {
                        html.Append($"<th style='padding:5px;'>{col}</th>");
                    }
                }
                html.Append("</tr></thead>");

                // Table rows (skip CompanyName)
                html.Append("<tbody>");
                foreach (var row in GetData)
                {
                    html.Append("<tr>");
                    foreach (var kv in (IDictionary<string, object>)row)
                    {
                        if (!string.Equals(kv.Key, "CompanyName", StringComparison.OrdinalIgnoreCase))
                        {
                            html.Append($"<td style='padding:5px;'>{kv.Value}</td>");
                        }
                    }
                    html.Append("</tr>");
                }
                html.Append("</tbody></table>");
                // After your first table is closed

                // ----------------- SECOND SUMMARY TABLE (Fixed Columns) -----------------

                DynamicParameters paramSummary = new DynamicParameters();
                paramSummary.Add("@p_EmployeeId", Session["EmployeeId"]);
                paramSummary.Add("@p_FromMonthYear", FromMonthYear);
                paramSummary.Add("@p_ToMonthYear", ToMonthYear);
                paramSummary.Add("@p_CompanyId", CmpId);
                paramSummary.Add("@p_BranchId", BranchId);

                // NOTE: make sure this SP name matches your DB
                var summaryData = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Payroll_YearWiseLWFSummary", paramSummary)?.ToList()
                                  ?? new List<dynamic>();
                // ----------------- SECOND SUMMARY TABLE (Total Row) -----------------
                if (summaryData.Count > 0)
                {
                    var d = (IDictionary<string, object>)summaryData.First();

                    var totalEmpCount = d.ContainsKey("TotalEmployeeCount") ? d["TotalEmployeeCount"] : null;
                    var totalGross = d.ContainsKey("TotalGross") ? d["TotalGross"] : null;
                    var lwfWages = d.ContainsKey("LWFWages") ? d["LWFWages"] : null;
                    var empContribution = d.ContainsKey("EmployeeContribution") ? d["EmployeeContribution"] : null;
                    var employeerContribution = d.ContainsKey("EmployerContribution") ? d["EmployerContribution"] : null;

                    html.Append("<h3 style='margin-top:20px;text-align:center;'>LWF Summary</h3>");
                    html.Append("<table style='border:1px solid #444;border-collapse:collapse;width:70%;margin:auto;font-size:13px;'>");
                    html.Append("<thead>");
                    html.Append("<tr style='background-color:#f0f0f0;font-weight:bold;text-align:center;'>");
                    html.Append("<th style='border:1px solid #444;padding:8px;'> </th>"); // Empty first cell (for "Total" row label)
                    html.Append("<th style='border:1px solid #444;padding:8px;'>Total Employee Count</th>");
                    html.Append("<th style='border:1px solid #444;padding:8px;'>Total Gross</th>");
                    html.Append("<th style='border:1px solid #444;padding:8px;'>LWF Wages</th>");
                    html.Append("<th style='border:1px solid #444;padding:8px;'>LWF Employee Contribution</th>");
                    html.Append("<th style='border:1px solid #444;padding:8px;'>LWF Employer Contribution</th>");

                    html.Append("</tr></thead><tbody>");

                    html.Append("<tr style='font-weight:bold;'>");
                    html.Append("<td style='border:1px solid #444;padding:6px;text-align:center;'>Total</td>");
                    html.Append($"<td style='border:1px solid #444;padding:6px;text-align:right;'>{totalEmpCount}</td>");
                    html.Append($"<td style='border:1px solid #444;padding:6px;text-align:right;'>{totalGross}</td>");
                    html.Append($"<td style='border:1px solid #444;padding:6px;text-align:right;'>{lwfWages}</td>");
                    html.Append($"<td style='border:1px solid #444;padding:6px;text-align:right;'>{empContribution}</td>");
                    html.Append($"<td style='border:1px solid #444;padding:6px;text-align:right;'>{employeerContribution}</td>");

                    html.Append("</tr>");

                    html.Append("</tbody></table>");
                }


                html.Append("</body></html>");

                // Return as HTML so it opens in browser for printing
                return Content(html.ToString(), "text/html");
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