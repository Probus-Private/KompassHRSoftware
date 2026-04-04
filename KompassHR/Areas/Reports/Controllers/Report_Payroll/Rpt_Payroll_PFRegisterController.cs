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
    public class Rpt_Payroll_PFRegisterController : Controller
    {
        // GET: Reports/Rpt_Payroll_PFRegister
        #region Main View
        public ActionResult Rpt_Payroll_PFRegister()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 457;
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

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, DateTime Month)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("PFRegisterReport");
                worksheet.Range(1, 1, 1, 10).Merge();
                worksheet.SheetView.FreezeRows(2); // Freeze the first row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();


                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                paramList.Add("@p_MonthYear", Month);
                paramList.Add("@p_CompanyId", CmpId);
                paramList.Add("@p_BranchId", BranchId);
                var GetData = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Payroll_PF", paramList).ToList();
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
                worksheet.Cell(1, 1).Value = "Payroll PF Register Report - (" + Month.ToString("dd/MMM/yyyy") + ")";
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
        public ActionResult Print(int? CmpId, int? BranchId, DateTime Month)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                // Get company name
                string companyName = "";
                if (CmpId.HasValue)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "SELECT CompanyName FROM Mas_CompanyProfile WHERE CompanyId = " + CmpId);

                    var result = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).SingleOrDefault();
                    if (result != null && result.CompanyName != null)
                    {
                        companyName = result.CompanyName.ToString();
                    }
                }

                // Get payroll PF data
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_MonthYear", Month);
                paramList.Add("@p_CompanyId", CmpId);
                paramList.Add("@p_BranchId", BranchId);
                paramList.Add("@p_EmployeeId", Session["EmployeeId"]);

                paramList.Add("@p_TotalEmployee", dbType: DbType.Int32, direction: ParameterDirection.Output);
                paramList.Add("@p_ExcludedEmployee", dbType: DbType.Int32, direction: ParameterDirection.Output);
                paramList.Add("@p_GrossExcludedEmployee", dbType: DbType.Int32, direction: ParameterDirection.Output);

                var GetData = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Payroll_PF", paramList).ToList();

                var TotalEmployee = paramList.Get<int>("@p_TotalEmployee");
                var ExcludedEmployee = paramList.Get<int>("@p_ExcludedEmployee");
                var GrossExcludedEmployee = paramList.Get<int>("@p_GrossExcludedEmployee");



                if (GetData.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }

                var firstRow = (IDictionary<string, object>)GetData.First();
                var html = new System.Text.StringBuilder();

                // ====== HTML HEADER & STYLES ======
                html.Append("<!DOCTYPE html><html><head>");
                html.Append($"<title>Payroll PF Register Report - ({Month:MMM yyyy})</title>");
                html.Append(@"<style>
            body { font-family: Arial, sans-serif; margin: 20px; }
            h2,h3 { margin: 5px 0; }
            table { border-collapse: collapse; width: 100%; margin-top: 15px; }
            th, td { border: 1px solid #555; padding: 6px 8px; font-size: 12px; }
            th { background: #f0f0f0; text-align: center; }
            tr:nth-child(even) { background: #fafafa; }
            .summary-table { width: 70%; margin: 25px auto; border: 1px solid #444; }
            .summary-table td { border: 1px solid #444; padding: 6px; font-size: 13px; }
            .summary-title { background: #cddcac; font-weight: bold; text-align: center; }
            .right { text-align: right; }
        </style>");
                html.Append("</head><body>");

                // ====== HEADER ======
                if (!string.IsNullOrWhiteSpace(companyName))
                {
                    html.Append($"<h2 style='text-align:center;'>{companyName}</h2>");
                }
                html.Append($"<h3 style='text-align:center;'>Payroll PF Register Report - ({Month:MMM yyyy})</h3>");

                // ====== MAIN DATA TABLE ======
                html.Append("<table>");
                html.Append("<thead><tr>");
                foreach (var col in firstRow.Keys)
                {
                    if (!string.Equals(col, "CompanyName", StringComparison.OrdinalIgnoreCase))
                    {
                        html.Append($"<th>{col}</th>");
                    }
                }
                html.Append("</tr></thead>");

                html.Append("<tbody>");
                foreach (var row in GetData)
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

                // ====== SUMMARY TABLE ======
                var lastRow = (IDictionary<string, object>)GetData.Last();

                decimal acc01 = lastRow.ContainsKey("EPF Share Remitted") ? Convert.ToDecimal(lastRow["EPF Share Remitted"]) : 0;
                decimal epfWages = lastRow.ContainsKey("EPF Wages") ? Convert.ToDecimal(lastRow["EPF Wages"]) : 0;
                decimal acc02 = epfWages * 0.005m;
                decimal acc10 = lastRow.ContainsKey("EPS Contribution Remitted") ? Convert.ToDecimal(lastRow["EPS Contribution Remitted"]) : 0;
                decimal edliWages21 = lastRow.ContainsKey("EDLI Wages") ? Convert.ToDecimal(lastRow["EDLI Wages"]) : 0;
                decimal acc21 = edliWages21 * 0.005m;
                decimal edliWages22 = lastRow.ContainsKey("EDLI Wages") ? Convert.ToDecimal(lastRow["EDLI Wages"]) : 0;
                decimal acc22 = edliWages22 * 0.00m;
                decimal edliWages = lastRow.ContainsKey("EDLI Wages") ? Convert.ToDecimal(lastRow["EDLI Wages"]) : 0;
                decimal pensionWages = lastRow.ContainsKey("EPS Wages") ? Convert.ToDecimal(lastRow["EPS Wages"]) : 0;
                decimal total = (acc01 + acc02 + acc10 + edliWages21 + edliWages22);

                html.Append("<table class='summary-table'>");
                html.Append("<tr><td colspan='2' class='summary-title'>Summary</td></tr>");
                html.Append($"<tr><td>Account No:01 </td><td class='right'>{acc01:N2}</td></tr>");
                html.Append($"<tr><td>Account No:02 </td><td class='right'>{acc02:N2}</td></tr>");
                html.Append($"<tr><td>Account No:10 </td><td class='right'>{acc10:N2}</td></tr>");
                html.Append($"<tr><td>Account No:21 </td><td class='right'>{acc21:N2}</td></tr>");
                html.Append($"<tr><td>Account No:22 </td><td class='right'>{acc22:N2}</td></tr>");
                html.Append($"<tr><td><b>TOTAL</b></td><td class='right'><b>{total:N2}</b></td></tr>");

                html.Append($"<tr><td>EDLI Wages</td><td class='right'>{edliWages:N2}</td></tr>");
                html.Append($"<tr><td>Pension Wages</td><td class='right'>{pensionWages:N2}</td></tr>");

                html.Append($"<tr><td><b>Total No. of Employees in the Month</b></td><td class='right'><b>{TotalEmployee:N2}</b></td></tr>");
                html.Append($"<tr><td><b>No. of Excluded Employees</b></td><td class='right'><b>{ExcludedEmployee:N2}</b></td></tr>");
                html.Append($"<tr><td><b>Gross Wages of Excluded Employees</b></td><td class='right'><b>{GrossExcludedEmployee:N2}</b></td></tr>");

                html.Append("</table>");

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