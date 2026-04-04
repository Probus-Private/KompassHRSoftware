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

namespace KompassHR.Areas.Reports.Controllers.Report_Payroll
{
    public class Rpt_Payroll_BonusLTAReportController : Controller
    {
        // GET: Reports/Rpt_Payroll_BonusLTAReport
        public ActionResult Rpt_Payroll_BonusLTAReport(MonthWiseFilter MonthWiseFilter)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 919;
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

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, DateTime FromMonthYear, DateTime ToMonthYear, string Head)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                #region Bonus
                if (Head == "Bonus")
                {
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FromMonth", FromMonthYear);
                    paramList.Add("@p_ToMonth", ToMonthYear);

                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_BonusRegister_Format", paramList).ToList();

                    if (GetData.Count == 0) return Content("No data found");

                    DapperORM dprObj = new DapperORM();
                    DataTable dt = dprObj.ConvertToDataTable(GetData);

                    using (var workbook = new XLWorkbook())
                    {
                        var ws = workbook.Worksheets.Add("BonusReport");

                        /* TITLE */
                        ws.Cell(1, 1).Value = $"Bonus Report ({FromMonthYear:dd-MMM-yyyy} to {ToMonthYear:dd-MMM-yyyy})";
                        ws.Range(1, 1, 1, dt.Columns.Count).Merge();
                        ws.Cell(1, 1).Style.Font.Bold = true;
                        ws.Cell(1, 1).Style.Font.FontSize = 14;
                        ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;


                        /* STATIC COLUMNS */
                        string[] staticCols = {"S.No", "Status", "Employee No", "Employee Name", "Department", "Designation", "Joining Date", "DOL"};

                        for (int i = 0; i < staticCols.Length; i++)
                        {
                            ws.Range(3, i + 1, 5, i + 1).Merge();
                            ws.Cell(3, i + 1).Value = staticCols[i];
                            ws.Cell(3, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Cell(3, i + 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            ws.Cell(3, i + 1).Style.Font.Bold = true;
                            ws.Cell(3, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                        }

                        int col = staticCols.Length + 1;

                        /* MONTH GROUP HEADER */
                        var monthGroups = dt.Columns.Cast<DataColumn>()
                          .Where(c => c.ColumnName.Contains("_Bonus"))
                          .GroupBy(c =>
                          {
                              var raw = c.ColumnName.Replace("_Bonus", "").Trim();
                              DateTime dtValue;

                              // ✅ Correct parsing for Jan-25
                              if (DateTime.TryParseExact(raw, "MMM-yy",
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    System.Globalization.DateTimeStyles.None, out dtValue))
                              {
                                  return new
                                  {
                                      Date = new DateTime(dtValue.Year, dtValue.Month, 1), // normalize
                                      Key = dtValue.ToString("MMM-yy") // Jan-25
                                  };
                              }

                              return new { Date = DateTime.MinValue, Key = raw };
                          });

                        foreach (var month in monthGroups)
                        {
                            int span = 2;

                            ws.Range(3, col, 3, col + span - 1).Merge();
                            ws.Cell(3, col).Style.NumberFormat.Format = "@";   // TEXT
                            ws.Cell(3, col).Value = month.Key.Key;
                            ws.Cell(3, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Cell(3, col).Style.Font.Bold = true;

                            int year = month.Key.Date.Year;
                            int monthNumber = month.Key.Date.Month;

                            int days = DateTime.DaysInMonth(year, monthNumber);
                            //  ws.Range(4, col, 4, col + span - 1).Merge();
                            ws.Cell(4, col).Value = "Monthly Salary Days ";
                            ws.Cell(4, col + 1).Value = days;
                            ws.Cell(4, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Cell(4, col).Style.Font.Bold = true;

                            /* Sub Headers */
                            ws.Cell(5, col).Value = "Actual Payable Days";
                            ws.Cell(5, col + 1).Value = "Eligible Bonus";

                            ws.Range(5, col, 5, col + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Range(5, col, 5, col + 2).Style.Fill.BackgroundColor = XLColor.Yellow;
                            ws.Range(5, col, 5, col + 2).Style.Font.Bold = true;

                            col += span;
                        }

                        /* TOTAL COLUMNS */
                        ws.Range(3, col, 3, col + 1).Merge();
                        ws.Cell(3, col).Value = "Total";
                        ws.Cell(3, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(3, col).Style.Font.Bold = true;

                        /* Sub Headers */
                        ws.Cell(5, col).Value = "Total Payable Days";
                        ws.Cell(5, col + 1).Value = "Total Bonus Amount";

                        ws.Range(5, col, 5, col + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                      //  ws.Range(5, col, 5, col + 2).Style.Fill.BackgroundColor = XLColor.LightGreen;
                        ws.Range(5, col, 5, col + 2).Style.Font.Bold = true;

                        /* INSERT DATA */
                        ws.Cell(6, 1).InsertData(dt.AsEnumerable());

                        /* STYLE */
                        var usedRange = ws.RangeUsed();
                        usedRange.Style.Fill.BackgroundColor = XLColor.White;
                        usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        usedRange.Style.Font.FontSize = 10;
                        usedRange.Style.Font.FontColor = XLColor.Black;
                        //  usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        //  usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Dotted;

                        ws.Columns().AdjustToContents();

                        ws.SheetView.FreezeRows(5);

                        using (var stream = new MemoryStream())
                        {
                            workbook.SaveAs(stream);
                            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report.xlsx");
                        }
                    }
                }
                #endregion

                #region LTA
                else if (Head == "LTA")
                {
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FromMonth", FromMonthYear);
                    paramList.Add("@p_ToMonth", ToMonthYear);

                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_LTARegister_Format", paramList).ToList();

                    if (GetData.Count == 0) return Content("No data found");

                    DapperORM dprObj = new DapperORM();
                    DataTable dt = dprObj.ConvertToDataTable(GetData);

                    using (var workbook = new XLWorkbook())
                    {
                        var ws = workbook.Worksheets.Add("LTAReport");

                        /* TITLE */
                        ws.Cell(1, 1).Value = $"LTA Report ({FromMonthYear:dd-MMM-yyyy} to {ToMonthYear:dd-MMM-yyyy})";
                        ws.Range(1, 1, 1, dt.Columns.Count).Merge();
                        ws.Cell(1, 1).Style.Font.Bold = true;
                        ws.Cell(1, 1).Style.Font.FontSize = 14;
                        ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;


                        /* STATIC COLUMNS */
                        string[] staticCols = { "S.No", "Status", "Employee No", "Employee Name", "Department", "Designation", "Joining Date", "DOL" };

                        for (int i = 0; i < staticCols.Length; i++)
                        {
                            ws.Range(3, i + 1, 5, i + 1).Merge();
                            ws.Cell(3, i + 1).Value = staticCols[i];
                            ws.Cell(3, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Cell(3, i + 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            ws.Cell(3, i + 1).Style.Font.Bold = true;
                            ws.Cell(3, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                        }

                        int col = staticCols.Length + 1;

                        var monthGroups = dt.Columns.Cast<DataColumn>()
                          .Where(c => c.ColumnName.Contains("_LTA"))
                          .GroupBy(c =>
                          {
                              var raw = c.ColumnName.Replace("_LTA", "").Trim();
                              DateTime dtValue;

                            // ✅ Correct parsing for Jan-25
                            if (DateTime.TryParseExact(raw, "MMM-yy",
                                  System.Globalization.CultureInfo.InvariantCulture,
                                  System.Globalization.DateTimeStyles.None, out dtValue))
                              {
                                  return new
                                  {
                                      Date = new DateTime(dtValue.Year, dtValue.Month, 1), // normalize
                                    Key = dtValue.ToString("MMM-yy") // Jan-25
                                };
                              }

                              return new { Date = DateTime.MinValue, Key = raw };
                          });

                        foreach (var month in monthGroups)
                        {
                            int span = 2;
                          
                            ws.Range(3, col, 3, col + span - 1).Merge();
                            ws.Cell(3, col).Style.NumberFormat.Format = "@";   // TEXT
                           // ws.Cell(3, col).DataType = XLDataType.Text;
                            ws.Cell(3, col).Value = month.Key.Key;
                            ws.Cell(3, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Cell(3, col).Style.Font.Bold = true;

                            int year = month.Key.Date.Year;
                            int monthNumber = month.Key.Date.Month;

                            int days = DateTime.DaysInMonth(year, monthNumber);
                            //  ws.Range(4, col, 4, col + span - 1).Merge();
                            ws.Cell(4, col).Value = "Monthly Salary Days ";
                            ws.Cell(4, col + 1).Value = days;
                            ws.Cell(4, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Cell(4, col).Style.Font.Bold = true;

                            /* Sub Headers */
                            ws.Cell(5, col).Value = "Actual Payable Days";
                            ws.Cell(5, col + 1).Value = "Eligible LTA";

                            ws.Range(5, col, 5, col + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Range(5, col, 5, col + 2).Style.Fill.BackgroundColor = XLColor.Yellow;
                            ws.Range(5, col, 5, col + 2).Style.Font.Bold = true;

                            col += span;
                        }

                        /* TOTAL COLUMNS */
                        ws.Range(3, col, 3, col + 1).Merge();
                        ws.Cell(3, col).Value = "Total";
                        ws.Cell(3, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(3, col).Style.Font.Bold = true;

                        /* Sub Headers */
                        ws.Cell(5, col).Value = "Total Payable Days";
                        ws.Cell(5, col + 1).Value = "Total LTA Amount";

                        ws.Range(5, col, 5, col + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                     //   ws.Range(5, col, 5, col + 2).Style.Fill.BackgroundColor = XLColor.LightGreen;
                        ws.Range(5, col, 5, col + 2).Style.Font.Bold = true;

                        /* INSERT DATA */
                        ws.Cell(6, 1).InsertData(dt.AsEnumerable());

                        /* STYLE */
                        var usedRange = ws.RangeUsed();
                        usedRange.Style.Fill.BackgroundColor = XLColor.White;
                        usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        usedRange.Style.Font.FontSize = 10;
                        usedRange.Style.Font.FontColor = XLColor.Black;
                        //  usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        //  usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Dotted;

                        ws.Columns().AdjustToContents();

                        ws.SheetView.FreezeRows(5);

                        using (var stream = new MemoryStream())
                        {
                            workbook.SaveAs(stream);
                            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report.xlsx");
                        }
                    }
                   
                }
                #endregion

                return Content("Invalid Head");
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