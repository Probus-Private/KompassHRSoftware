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
    public class Rpt_Payroll_YearWise_PTRegisterController : Controller
    {
        // GET: Reports/Rpt_Payroll_YearWise_PTRegister
        #region Main View
        public ActionResult Rpt_Payroll_YearWise_PTRegister()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 903;
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
                var worksheet = workbook.Worksheets.Add("YearWisePT");
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
                var GetData = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Payroll_YearWisePT", paramList).ToList();
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
                worksheet.Cell(1, 1).Value = "Year Wise PT Report - (" + FromMonthYear.ToString("dd/MMM/yyyy") + " to " + ToMonthYear.ToString("dd/MMM/yyyy") + ")";
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
                    companyName = result.CompanyName?.ToString();
                }
              
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                paramList.Add("@p_CompanyId", CmpId);
                paramList.Add("@p_BranchId", BranchId);
                paramList.Add("@p_FromMonthYear", FromMonthYear);
                paramList.Add("@p_ToMonthYear", ToMonthYear);
                var detailData = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Payroll_YearWisePT", paramList).ToList();
                if (detailData.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }

                // ----------------- SECOND SP (Summary Report) -----------------
                DynamicParameters paramSummary = new DynamicParameters();
                paramSummary.Add("@p_EmployeeId", Session["EmployeeId"]);
                paramSummary.Add("@p_CompanyId", CmpId);
                paramSummary.Add("@p_BranchId", BranchId);
                paramSummary.Add("@p_FromMonthYear", FromMonthYear);
                paramSummary.Add("@p_ToMonthYear", ToMonthYear);
                var summaryData = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Payroll_YearWisePTSummary", paramSummary).ToList();

                // ----------------- HTML GENERATION -----------------
                var firstRow = (IDictionary<string, object>)detailData.First();
                var html = new System.Text.StringBuilder();

                html.Append("<!DOCTYPE html><html><head>");
                html.Append($"<title>Year Wise Payroll PT Register Report - ({FromMonthYear:MMM yyyy} - {ToMonthYear:MMM yyyy})</title>");
                html.Append("<style>");
                html.Append("body { font-family: Arial, sans-serif; margin: 20px; font-size:12px; }");
                html.Append("table { border-collapse: collapse; width: 100%; margin-bottom: 20px; }");
                html.Append("th, td { border: 1px solid #ddd; padding: 5px; text-align: left; }");
                html.Append("th { background-color: #f2f2f2; }");
                html.Append("</style>");
                html.Append("</head><body>");

                if (!string.IsNullOrWhiteSpace(companyName))
                {
                    html.Append($"<h2 style='text-align:center;margin-bottom:5px;'>{companyName}</h2>");
                }

                html.Append($"<h3 style='text-align:left;margin-bottom:10px;'>Year Wise Payroll PT Register Report - ({FromMonthYear:MMM yyyy} - {ToMonthYear:MMM yyyy})</h3>");

                // ----------------- FIRST TABLE -----------------
                html.Append("<table>");
                html.Append("<thead><tr style='background-color:#cddcac;font-weight:bold;'>");
                foreach (var col in firstRow.Keys)
                {
                    if (!string.Equals(col, "CompanyName", StringComparison.OrdinalIgnoreCase))
                    {
                        html.Append($"<th>{col}</th>");
                    }
                }
                html.Append("</tr></thead><tbody>");

                foreach (var row in detailData)
                {
                    html.Append("<tr>");
                    foreach (var kv in (IDictionary<string, object>)row)
                    {
                        if (!string.Equals(kv.Key, "CompanyName", StringComparison.OrdinalIgnoreCase))
                        {
                            html.Append($"<td>{kv.Value}</td>");
                        }
                    }
                    html.Append("</tr>");
                }
                html.Append("</tbody></table>");

                // ----------------- SECOND SUMMARY TABLE -----------------
                // ----------------- SECOND SUMMARY TABLE -----------------
                // ----------------- SECOND SUMMARY TABLE -----------------
                if (summaryData.Count > 0)
                {
                    html.Append("<h3 style='margin-top:20px;text-align:center;'>PT Summary by Gender</h3>");
                    html.Append("<table style='border:1px solid #444;border-collapse:collapse;width:70%;margin:auto;font-size:13px;'>");
                    html.Append("<thead>");
                    html.Append("<tr style='background-color:#f0f0f0;font-weight:bold;text-align:center;'>");
                    html.Append("<th style='border:1px solid #444;padding:8px;'>Gender</th>");
                    html.Append("<th style='border:1px solid #444;padding:8px;'>PT Amount</th>");
                    html.Append("<th style='border:1px solid #444;padding:8px;'>Employee Count</th>");
                    html.Append("<th style='border:1px solid #444;padding:8px;'>Total</th>");
                    html.Append("</tr></thead><tbody>");

                    string lastGender = null;

                    foreach (var row in summaryData)
                    {
                        var dict = (IDictionary<string, object>)row;
                        string gender = dict["Gender"]?.ToString();

                        bool isTotal = string.Equals(gender, "Total", StringComparison.OrdinalIgnoreCase);

                        // Open row with bold + border if it's Total
                        if (isTotal)
                            html.Append("<tr style='font-weight:bold;border-top:2px solid black;border-bottom:2px solid black;'>");
                        else
                            html.Append("<tr>");

                        // Gender column
                        if (!isTotal)
                        {
                            if (gender != lastGender)
                            {
                                html.Append($"<td style='border:1px solid #444;padding:6px;text-align:center;'>{gender}</td>");
                                lastGender = gender;
                            }
                            else
                            {
                                html.Append("<td style='border:1px solid #444;padding:6px;'></td>");
                            }
                        }
                        else
                        {
                            // Always show "Total"
                            html.Append($"<td style='border:1px solid #444;padding:6px;text-align:center;'>{gender}</td>");
                        }

                        // Other columns
                        var ptAmount = dict.ContainsKey("PTAmount") ? dict["PTAmount"]?.ToString() : "";
                        var empCount = dict.ContainsKey("EmployeeCount") ? dict["EmployeeCount"]?.ToString() : "";
                        var totalAmt = dict.ContainsKey("TotalAmount") ? dict["TotalAmount"]?.ToString() : "";

                        html.Append($"<td style='border:1px solid #444;padding:6px;text-align:right;'>{ptAmount}</td>");
                        html.Append($"<td style='border:1px solid #444;padding:6px;text-align:right;'>{empCount}</td>");
                        html.Append($"<td style='border:1px solid #444;padding:6px;text-align:right;'>{totalAmt}</td>");

                        //html.Append($"<td style='border:1px solid #444;padding:6px;text-align:right;'>{dict["PTAmount"]}</td>");
                        //html.Append($"<td style='border:1px solid #444;padding:6px;text-align:right;'>{dict["EmployeeCount"]}</td>");
                        //html.Append($"<td style='border:1px solid #444;padding:6px;text-align:right;'>{dict["TotalAmount"]}</td>");

                        html.Append("</tr>");
                    }
                    html.Append("</tbody></table>");
                }
                html.Append("</body></html>");
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