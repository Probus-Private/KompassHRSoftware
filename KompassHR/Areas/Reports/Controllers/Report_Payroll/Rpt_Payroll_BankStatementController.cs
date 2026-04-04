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
namespace KompassHR.Areas.Reports.Controllers.Report_Payroll
{
    public class Rpt_Payroll_BankStatementController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Reports/Rpt_Payroll_BankStatement/489
        public ActionResult Rpt_Payroll_BankStatement(MonthWiseFilter MonthWiseFilter)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 489;
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
                DynamicParameters SalaryMode = new DynamicParameters();
                SalaryMode.Add("@query", "Select BankStatementId as Id,Type As Name  from Payroll_BankStatement where Deactivate=0 and CompanyId=" + CMPID + "");
                ViewBag.SalaryMode = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", SalaryMode).ToList();

                var results = DapperORM.DynamicQueryMultiple($@"Select CompanyBankId As Id , BankName as Name from Payroll_CompanyBank where Deactivate=0
                                 AND CompanyBankCmpID = (SELECT CmpId FROM Mas_Employee WHERE EmployeeId = {Session["EmployeeId"]} AND Deactivate = 0)
                                 AND CompanyBankBUId = (SELECT EmployeeBranchId FROM Mas_Employee WHERE EmployeeId = {Session["EmployeeId"]} AND Deactivate = 0)                                                    
                                 Order By IsDefault Desc;
                                 Select ProcessCategoryId as Id,ProcessCategoryName as Name from Payroll_ProcessCategory where Deactivate=0 order by isdefault desc;");
                ViewBag.CompanyBankName = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.SalaryProcess = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                //ViewBag.CompanyBankName = results.Read<AllDropDownClass>().ToList();

                return View(MonthWiseFilter);
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
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "Select BankStatementId as Id,Type As Name  from Payroll_BankStatement where Deactivate=0 and CompanyId=" + CmpId + "");
                var Type = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param).ToList();

                return Json(new { Branch = Branch, Type = Type }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region DownloadExcelFile
        //public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, DateTime Month, string SalaryMode, string SalaryBankName)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { Area = "" });
        //        }
        //        var workbook = new XLWorkbook();
        //        var worksheet = workbook.Worksheets.Add("Bank Statement");
        //        worksheet.Range(1, 1, 1, 10).Merge();
        //        worksheet.SheetView.FreezeRows(2); // Freeze first two rows

        //        // Setup data
        //        DynamicParameters paramList = new DynamicParameters();
        //        paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
        //        paramList.Add("@p_MonthYear", Month);
        //        paramList.Add("@p_CompanyId", CmpId);
        //        paramList.Add("@p_BranchId", BranchId);
        //        paramList.Add("@p_SalaryMode", SalaryMode);
        //        //paramList.Add("@p_SalaryBankName", SalaryBankName);

        //        var reportData = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Payroll_BankStatement", paramList).ToList();
        //        if (reportData.Count == 0)
        //        {
        //            return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
        //        }

        //        // Convert to DataTable
        //        DapperORM dprObj = new DapperORM();
        //        DataTable dt = dprObj.ConvertToDataTable(reportData);

        //        // Insert data starting from row 2 (row 1 is for title)
        //        worksheet.Cell(2, 1).InsertTable(dt, false);

        //        // Report title in merged cell (Row 1)
        //        worksheet.Cell(1, 1).Value = $"Bank Statement Sheet Report - ({Month:dd/MMM/yyyy})";
        //        worksheet.Cell(1, 1).Style.Font.Bold = true;
        //        worksheet.Cell(1, 1).Style.Font.FontSize = 12;
        //        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

        //        // Header style (Row 2 only)
        //        var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
        //        headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
        //        headerRange.Style.Font.FontSize = 10;
        //        headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
        //        headerRange.Style.Font.Bold = true;

        //        // Data range (from Row 3 to N)
        //        var dataStartRow = 3;
        //        var dataEndRow = dataStartRow + dt.Rows.Count - 1;
        //        var dataRange = worksheet.Range(dataStartRow, 1, dataEndRow, dt.Columns.Count);
        //        dataRange.Style.Fill.BackgroundColor = XLColor.White;
        //        dataRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        //        dataRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        //        dataRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        //        dataRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
        //        dataRange.Style.Font.FontSize = 10;
        //        dataRange.Style.Font.FontColor = XLColor.Black;

        //        // ❌ Removed lastRow styling completely to avoid extra color

        //        // Adjust column widths
        //        worksheet.Columns().AdjustToContents();

        //        // Return Excel file
        //        using (var stream = new MemoryStream())
        //        {
        //            workbook.SaveAs(stream);
        //            stream.Position = 0;
        //            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report.xlsx");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
        //}

        //public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, DateTime Month, string SalaryMode, string SalaryBankName)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { Area = "" });
        //        }

        //        var workbook = new XLWorkbook();

        //        DynamicParameters paramList = new DynamicParameters();
        //        paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
        //        paramList.Add("@p_MonthYear", Month);
        //        paramList.Add("@p_CompanyId", CmpId);
        //        paramList.Add("@p_BranchId", BranchId);
        //        paramList.Add("@p_SalaryMode", SalaryMode);

        //        using (var connection = new SqlConnection(DapperORM.connectionString))
        //        {
        //            connection.Open();

        //            using (var multi = connection.QueryMultiple("sp_Rpt_Payroll_BankStatement", paramList, commandType: CommandType.StoredProcedure))
        //            {
        //                int sheetIndex = 1;
        //                bool hasData = false;

        //                // Predefined order for 'All'
        //                List<string> sheetOrder = new List<string> { "Bank-Salary", "Bank-NEFT", "Cheque", "Cash" };

        //                while (!multi.IsConsumed)
        //                {
        //                    var resultSet = multi.Read().ToList();
        //                    if (resultSet.Count == 0) continue;
        //                    hasData = true;

        //                    DataTable dt = new DapperORM().ConvertToDataTable(resultSet);

        //                    string sheetName;
        //                    if (SalaryMode.Equals("All", StringComparison.OrdinalIgnoreCase))
        //                    {
        //                        // Use predefined names in order
        //                        sheetName = sheetIndex <= sheetOrder.Count ? sheetOrder[sheetIndex - 1] : $"Sheet{sheetIndex}";
        //                    }
        //                    else
        //                    {
        //                        sheetName = SalaryMode;
        //                    }

        //                    var worksheet = workbook.Worksheets.Add(sheetName);
        //                    worksheet.Range(1, 1, 1, dt.Columns.Count).Merge();
        //                    worksheet.SheetView.FreezeRows(2);

        //                    // Report title
        //                    worksheet.Cell(1, 1).Value = $"Bank Statement Sheet Report - ({Month:dd/MMM/yyyy})";
        //                    worksheet.Cell(1, 1).Style.Font.Bold = true;
        //                    worksheet.Cell(1, 1).Style.Font.FontSize = 12;
        //                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

        //                    // Insert table starting at Row 2
        //                    worksheet.Cell(2, 1).InsertTable(dt, false);

        //                    // Header style
        //                    var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
        //                    headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
        //                    headerRange.Style.Font.FontSize = 10;
        //                    headerRange.Style.Font.FontColor = XLColor.Black;
        //                    headerRange.Style.Font.Bold = true;
        //                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        //                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        //                    // Data range style (with borders so gridlines are always visible)
        //                    var dataStartRow = 3;
        //                    var dataEndRow = dataStartRow + dt.Rows.Count - 1;
        //                    var dataRange = worksheet.Range(dataStartRow, 1, dataEndRow, dt.Columns.Count);
        //                    dataRange.Style.Fill.BackgroundColor = XLColor.White; // optional
        //                    dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        //                    dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        //                    dataRange.Style.Font.FontSize = 10;
        //                    dataRange.Style.Font.FontColor = XLColor.Black;

        //                    worksheet.Columns().AdjustToContents();

        //                    sheetIndex++;
        //                }

        //                if (!hasData)
        //                {
        //                    return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
        //                }
        //            }
        //        }

        //        using (var stream = new MemoryStream())
        //        {
        //            workbook.SaveAs(stream);
        //            stream.Position = 0;
        //            return File(stream.ToArray(),
        //                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //                "Report.xlsx");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
        //}


        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId,string BranchName, DateTime Month, string SalaryMode, string SalaryBankName, int? SalaryProcessCategory)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var workbook = new XLWorkbook();

                // Salary modes to loop through
                List<string> salaryModesToProcess = new List<string>();

                if (SalaryMode.Equals("All", StringComparison.OrdinalIgnoreCase))
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "SELECT BankStatementId AS Id, Type AS Name FROM Payroll_BankStatement WHERE Deactivate = 0 AND CompanyId = " + CmpId + "");
                    var sheetOrder = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param).ToList();

                    // Take only Name (SalaryMode text)
                    salaryModesToProcess = sheetOrder.Select(x => x.Name).ToList();
                }
                else
                {
                    salaryModesToProcess.Add(SalaryMode);
                }

                bool hasData = false;

                using (var connection = new SqlConnection(DapperORM.connectionString))
                {
                    connection.Open();

                    foreach (var mode in salaryModesToProcess)
                    {
                        DynamicParameters paramList = new DynamicParameters();
                        paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                        paramList.Add("@p_MonthYear", Month);
                        paramList.Add("@p_CompanyId", CmpId);
                        paramList.Add("@p_BranchId", BranchId);
                        paramList.Add("@p_SalaryMode", mode);
                        paramList.Add("@p_SalaryProcessCategory", SalaryProcessCategory);
                        using (var multi = connection.QueryMultiple("sp_Rpt_Payroll_BankStatement", paramList, commandType: CommandType.StoredProcedure))
                        {
                            while (!multi.IsConsumed)
                            {
                                var resultSet = multi.Read().ToList();
                                if (resultSet.Count == 0) continue;
                                hasData = true;

                                DataTable dt = new DapperORM().ConvertToDataTable(resultSet);

                                // Use current SalaryMode as sheet name
                                string sheetName = mode;

                                var worksheet = workbook.Worksheets.Add(sheetName);
                                worksheet.Range(1, 1, 1, dt.Columns.Count).Merge();
                                worksheet.SheetView.FreezeRows(2);

                                // Report title
                                worksheet.Cell(1, 1).Value = $"Bank Statement Report - {sheetName} ({Month:dd/MMM/yyyy} - ({BranchName}))";
                                worksheet.Cell(1, 1).Style.Font.Bold = true;
                                worksheet.Cell(1, 1).Style.Font.FontSize = 12;
                                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                                // Insert table
                                worksheet.Cell(2, 1).InsertTable(dt, false);

                                // Header styling
                                var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
                                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                                headerRange.Style.Font.FontSize = 10;
                                headerRange.Style.Font.FontColor = XLColor.Black;
                                headerRange.Style.Font.Bold = true;
                                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                                // Data styling
                                var dataStartRow = 3;
                                var dataEndRow = dataStartRow + dt.Rows.Count - 1;
                                var dataRange = worksheet.Range(dataStartRow, 1, dataEndRow, dt.Columns.Count);
                                dataRange.Style.Fill.BackgroundColor = XLColor.White;
                                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                                dataRange.Style.Font.FontSize = 10;
                                dataRange.Style.Font.FontColor = XLColor.Black;
                                worksheet.Columns().AdjustToContents();
                            }
                        }
                    }
                }
                if (!hasData)
                {
                    return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                }

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
