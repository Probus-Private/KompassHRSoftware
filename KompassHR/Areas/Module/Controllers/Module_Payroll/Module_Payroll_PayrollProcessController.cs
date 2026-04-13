using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Module.Models.Module_Payroll;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using System.Linq;

namespace KompassHR.Areas.Module.Controllers.Module_Payroll
{
    public class Module_Payroll_PayrollProcessController : Controller
    {
        // GET: Module/Module_Payroll_PayrollProcess
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        #region Module_Payroll_PayrollProcess
        public ActionResult Module_Payroll_PayrollProcess()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 282;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Session["MonthYear"] = null;
                //DynamicParameters paramCompany = new DynamicParameters();
                //paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                //var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                //ViewBag.GetCompanyName = GetComapnyName;

                var resultcmp = DapperORM.DynamicQueryMultiple(
                 "SELECT DISTINCT CompanyId AS Id, CompanyName AS Name FROM Mas_CompanyProfile INNER JOIN Payroll_MakerChecker ON Payroll_MakerChecker.MakerCheckerCmpId = Mas_CompanyProfile.CompanyId WHERE Payroll_MakerChecker.Deactivate = 0 AND Mas_CompanyProfile.Deactivate = 0 " +
                 "AND Payroll_MakerChecker.PayrollMakerEmpId = '" + Session["EmployeeId"] + "'");

                var Companylist = resultcmp[0].Select(x => new AllDropDownBind { Id = Convert.ToDouble(x.Id), Name = (string)x.Name }).ToList();
                ViewBag.GetCompanyName = Companylist;
                var CompanyId = Companylist.FirstOrDefault()?.Id;

                ViewBag.GetBranchName = "";


                var results = DapperORM.DynamicQueryMultiple("Select ProcessCategoryId as Id,ProcessCategoryName as Name from Payroll_ProcessCategory where Deactivate = 0 order by isdefault desc");
                var SalaryProcessList = results[0].Select(x => new AllDropDownBind { Id = Convert.ToDouble(x.Id), Name = (string)x.Name }).ToList();
                ViewBag.SalaryProcess = SalaryProcessList;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
        [HttpGet]
        public ActionResult GetList(MonthlyAttendance OBJList)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 282;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ////// Extract month and year from OBJList.Month
                //int month = OBJList.Month.Month;
                //int year = OBJList.Month.Year;

                //DynamicParameters EmpInfo = new DynamicParameters();
                //EmpInfo.Add("@query", @" SELECT COUNT(*) AS ApprovedCount FROM payroll_Salary
                //LEFT JOIN Mas_Branch ON Mas_Branch.BranchId = payroll_Salary.SalaryBranchId
                //LEFT JOIN Payroll_ProcessCategory ON Payroll_ProcessCategory.ProcessCategoryId = payroll_Salary.SalaryProcessCategoryId
                //WHERE MONTH(payroll_Salary.SalaryMonthYear) = " + OBJList.Month.Month + @"
                //  AND YEAR(payroll_Salary.SalaryMonthYear) = " + OBJList.Month.Year + @"
                //  AND payroll_Salary.Deactivate = 0 AND payroll_Salary.SalaryCheckerStatus = 'Approved'");
                //var approvedCountResult = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", EmpInfo).FirstOrDefault();
                //TempData["ApprovedCount"] = approvedCountResult.ApprovedCount;

                //DynamicParameters paramReject = new DynamicParameters();
                //paramReject.Add("@query", @" SELECT COUNT(*) AS ApprovedCount FROM payroll_Salary
                //LEFT JOIN Mas_Branch ON Mas_Branch.BranchId = payroll_Salary.SalaryBranchId
                //LEFT JOIN Payroll_ProcessCategory ON Payroll_ProcessCategory.ProcessCategoryId = payroll_Salary.SalaryProcessCategoryId
                //WHERE MONTH(payroll_Salary.SalaryMonthYear) = " + OBJList.Month.Month + @"
                //  AND YEAR(payroll_Salary.SalaryMonthYear) = " + OBJList.Month.Year + @"
                //  AND payroll_Salary.Deactivate = 0 AND payroll_Salary.SalaryCheckerStatus = 'Rejected'");
                //var RejectCountResult = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", EmpInfo).FirstOrDefault();
                //TempData["RejectCount"] = RejectCountResult.RejectCount;

                if (OBJList.Month == DateTime.MinValue)
                {
                    if (Session["MonthYear"] != null)
                    {
                        param.Add("@p_Origin", "List");
                        param.Add("@p_EmployeeId", Session["EmployeeId"]);
                        //  param.Add("@p_CmpId", Session["CompanyId"]);
                        param.Add("@p_MonthYear", Session["MonthYear"]);
                        var data = DapperORM.ReturnList<SalaryMonthlyAttendance>("sp_List_Payroll_Salary", param).ToList();
                        ViewBag.GetMonthlyAttendance = data;
                        if (data.Count != 0)
                        {
                            TempData["MonthYear"] = data[0].SalaryMonthYear.ToString("yyyy-MM");
                        }

                        // TempData["MonthYear"] = data[0].AttenMonthlyMonthYear.ToString("yyyy-MM");
                    }
                    else
                    {
                        ViewBag.GetMonthlyAttendance = "";
                        TempData["MonthYear"] = null;
                    }
                }
                else
                {
                    param.Add("@p_Origin", "List");
                    param.Add("@p_EmployeeId", Session["EmployeeId"]);
                    //param.Add("@p_CmpId", Session["CompanyId"]);
                    param.Add("@p_MonthYear", OBJList.Month);
                    var data = DapperORM.ReturnList<SalaryMonthlyAttendance>("sp_List_Payroll_Salary", param).ToList();
                    ViewBag.GetMonthlyAttendance = data;
                    if (data.Count != 0)
                    {
                        TempData["MonthYear"] = data[0].SalaryMonthYear.ToString("yyyy-MM");
                    }

                }
                return View(OBJList);
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
                //DynamicParameters param = new DynamicParameters();
                //param.Add("@p_employeeid", Session["EmployeeId"]);
                //param.Add("@p_CmpId", CmpId);
                //var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                //return Json(data, JsonRequestBehavior.AllowGet);

                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "SELECT DISTINCT BranchId AS Id, BranchName AS Name FROM Mas_Branch " +
                 "INNER JOIN Payroll_MakerChecker ON Payroll_MakerChecker.MakerCheckerBranchId = Mas_Branch.BranchId " +
                 "WHERE Payroll_MakerChecker.Deactivate = 0 " +
                 "AND Mas_Branch.Deactivate = 0 " +
                 "AND Payroll_MakerChecker.PayrollMakerEmpId = '" + Session["EmployeeId"] + "' " +
                 "AND Payroll_MakerChecker.MakerCheckerCmpId = '" + CmpId + "'");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();
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
        public ActionResult SaveUpdate(int? CmpId, int? BranchId, DateTime Month, int? ProcessCategoryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var PayrollCount = DapperORM.DynamicQuerySingle("Select Count(PayrollLockId) as LockCount from Payroll_LOck where Deactivate=0 and PayrollLockBranchID=" + BranchId + " and Month(PayrollLockMonthYear)='" + Month.ToString("MM") + "'  and Year(PayrollLockMonthYear) ='" + Month.ToString("yyyy") + "' and Status=1");
                if (PayrollCount.LockCount != 0)
                {
                    TempData["Message"] = "The record can't be saved because the payroll for this month ('" + Month.ToString("MMM-yyyy") + "') is locked.";
                    TempData["Icon"] = "error";
                    var IsValid = "1";
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"], Valid= IsValid }, JsonRequestBehavior.AllowGet);
                }

                param.Add("@p_process", "IsValidation");
                param.Add("@p_BranchId", BranchId);
                param.Add("@p_Cmpid", CmpId);
                param.Add("@p_MonthYear", Month);
                param.Add("@p_ProcessCategoryId", ProcessCategoryId);
                param.Add("@p_SalaryMakerApprovedBy_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Valid", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Salary_Process", param);
                var Message = param.Get<string>("@p_msg");
                var Valid = param.Get<string>("@p_Valid");
                var Icon = param.Get<string>("@p_Icon");

                if (Icon == "error" || Message != "")
                {
                    return Json(new { Message, Icon, Valid, Result }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_BranchId", BranchId);
                    param.Add("@p_Cmpid", CmpId);
                    param.Add("@p_MonthYear", Month);
                    param.Add("@p_ProcessCategoryId", ProcessCategoryId);
                    param.Add("@p_SalaryMakerApprovedBy_EmployeeId", Session["EmployeeId"]);
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    // param.Add("@p_Valid", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result1 = DapperORM.ExecuteReturn("sp_Payroll_Salary_Process", param);
                    var Message1 = param.Get<string>("@p_msg");
                    var Icon1 = param.Get<string>("@p_Icon");

                    return Json(new { Message1, Icon1 }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Message1 = ex.Message, Icon1 = "error" }, JsonRequestBehavior.AllowGet);
                //Session["GetErrorMessage"] = ex.Message;
                //return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? BranchId, string MonthYear, int? SalaryCmpId,int? ProcessCategoryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Salary Process Report");
                worksheet.Range(1, 1, 1, 23).Merge();  //Merge First Row with 60 Clomn
                worksheet.SheetView.FreezeRows(2); // Freeze the row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();

                param.Add("@p_MonthYear", DateTime.Parse(MonthYear));
                param.Add("@p_BranchId", BranchId);
                param.Add("@p_CompanyId", SalaryCmpId);
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_ProcessCategoryId", ProcessCategoryId);

                data = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Payroll_WageSheet", param).ToList();
                if (data.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }
                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(data);
                worksheet.Cell(2, 1).InsertTable(dt, false);
                int totalRows = worksheet.RowsUsed().Count();

                var lastRow = worksheet.Row(totalRows + 1);
                lastRow.Style.Font.Bold = true;
                // Additional styling if required
                lastRow.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
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
                var SetMonth = DateTime.Parse(MonthYear).ToString("MMM-yyyy");
                worksheet.Cell(1, 1).Value = "Payroll Wage Sheet Report Details - (" + SetMonth + ")";
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

        #region GetSalary
        public ActionResult GetSalary(int? SalaryBranchId, string SalaryMonthYear, string status, int? SalaryProcessCategoryId)
        {
            try
            {  //PinCode
                Session["SalaryMonthYear"] = SalaryMonthYear;
                param.Add("@p_Origin", "PayrollProcess");
                param.Add("@p_BranchId", SalaryBranchId);
                param.Add("@p_MonthYear", DateTime.Parse(SalaryMonthYear));
                param.Add("@p_status", status);
                param.Add("@p_SalaryProcessCategoryId", SalaryProcessCategoryId);
                var data = DapperORM.ReturnList<dynamic>("sp_List_Payroll_Salary", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion  

        #region DeleteMontlyAttendance

        public ActionResult DeleteSalaryProcessed(DeleteSalaryPayrollProcessViewModel model, DateTime Month)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DataTable tbl_TypePayroll_Salary_Process_Delete = new DataTable();
                tbl_TypePayroll_Salary_Process_Delete.Columns.Add("SalaryID", typeof(double));
                tbl_TypePayroll_Salary_Process_Delete.Columns.Add("SalaryEmployeeId", typeof(double));
                tbl_TypePayroll_Salary_Process_Delete.Columns.Add("SalaryMonthYear", typeof(DateTime));

                foreach (var item in model.ObjPayrollProcess)
                {
                    DataRow dr = tbl_TypePayroll_Salary_Process_Delete.NewRow();
                    dr["SalaryID"] = Convert.ToDouble(item.SalaryID);
                    dr["SalaryEmployeeId"] = Convert.ToDouble(item.SalaryEmployeeId);
                    dr["SalaryMonthYear"] = Convert.ToDateTime(item.SalaryMonthYear).ToString("dd/MM/yyyy");
                    tbl_TypePayroll_Salary_Process_Delete.Rows.Add(dr);
                }

                DynamicParameters ARparam = new DynamicParameters();
                ARparam.Add("@p_PayrollMonthYear", Month);
                ARparam.Add("@tbl_TypePayroll_Salary_Process_Delete", tbl_TypePayroll_Salary_Process_Delete.AsTableValuedParameter("tbl_TypePayroll_Salary_Process_Delete"));
                ARparam.Add("@p_CreatedupdateBy", Session["EmployeeName"]);
                ARparam.Add("@p_MachineName", Dns.GetHostName().ToString());
                ARparam.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                ARparam.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter  
                var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Salary_Process_Delete", ARparam).ToList();
                TempData["Message"] = ARparam.Get<string>("@p_msg");
                TempData["Icon"] = ARparam.Get<string>("@p_Icon");
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"], Month = Month }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Icon = "error" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        #region GetWagesSheetInvoice
        [HttpGet]
        public ActionResult GetWagesSheetInvoice(int? BranchId, string MonthYear, int? SalaryCmpId, int? SalarySheetId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 282;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_MonthYear", DateTime.Parse(MonthYear));
                param.Add("@p_BranchId", BranchId);
                param.Add("@p_CompanyId", SalaryCmpId);
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_SalarySheetId", SalarySheetId);
                var Getdata = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Payroll_WageSheet_Invoice", param).ToList();
                var GetCompanyName = Getdata?.FirstOrDefault();
                TempData["CompanyName"] = GetCompanyName?.CompanyName ?? "";

                var GetSalaryMonthYear = Getdata?.FirstOrDefault();
                TempData["MonthYear"] = GetSalaryMonthYear?.SalaryMonthYear?.ToString("MMM/yyyy") ?? "";

                //TempData["CompanyName"] = Getdata[0].CompanyName;
                //TempData["MonthYear"] = Getdata[0].SalaryMonthYear.ToString("MMM/yyyy");
                ViewBag.Payroll_WageSheet = Getdata;


                var GetMakerChecker = DapperORM.DynamicQueryList($@"SELECT E.EmployeeName, 'Maker' AS Role
                                                            FROM Mas_Employee E
                                                            INNER JOIN Payroll_MakerChecker PMC ON E.EmployeeId = PMC.PayrollMakerEmpId
                                                            WHERE PMC.Deactivate = 0
                                                              AND PMC.MakerCheckerCmpId = {SalaryCmpId}
                                                              AND PMC.MakerCheckerBranchId = {BranchId}
                                                              AND(PMC.PayrollCheckerEmpId = {Session["EmployeeId"]} OR PMC.PayrollMakerEmpId = {Session["EmployeeId"]})
                                                            UNION ALL
                                                            SELECT E.EmployeeName, 'Checker' AS Role
                                                            FROM Mas_Employee E
                                                            INNER JOIN Payroll_MakerChecker PMC ON E.EmployeeId = PMC.PayrollCheckerEmpId
                                                            WHERE PMC.Deactivate = 0
                                                              AND PMC.MakerCheckerCmpId = {SalaryCmpId}
                                                              AND PMC.MakerCheckerBranchId = {BranchId}
                                                              AND(PMC.PayrollCheckerEmpId = {Session["EmployeeId"]} OR PMC.PayrollMakerEmpId = {Session["EmployeeId"]})").ToList(); 
                if (GetMakerChecker != null)
                {
                    var maker = GetMakerChecker.FirstOrDefault(x => x.Role == "Maker");
                    var checker = GetMakerChecker.FirstOrDefault(x => x.Role == "Checker");
                    TempData["MakerName"] = maker?.EmployeeName ?? "";
                    TempData["CheckerName"] = checker?.EmployeeName ?? "";
                }

                ViewBag.GetMapHeadName = DapperORM.DynamicQuerySingle($@"Select * from Payroll_Head_CTC where Deactivate=0 and PayrollMap_CompanyId={SalaryCmpId}");
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ExportToExcel
        public ActionResult ExportToExcel()
        {
            // Retrieve the data from ViewBag (replace with your actual data source if different)
            var data = ViewBag.EmployeeWiseReport; // Replace with actual data source

            // Initialize ExcelPackage
            using (ExcelPackage package = new ExcelPackage())
            {
                // Add a worksheet to the workbook
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Salary Statement");

                // Add headers
                worksheet.Cells[1, 1].Value = "SM Auto Engineering";
                worksheet.Cells[2, 1].Value = "Employee Wise Salary Statement";
                worksheet.Cells[3, 1].Value = "For the Period Oct/2024";

                // Merge cells for each title row
                worksheet.Cells[1, 1, 1, 27].Merge = true; // Row 1
                worksheet.Cells[2, 1, 2, 27].Merge = true; // Row 2
                worksheet.Cells[3, 1, 3, 27].Merge = true; // Row 3

                // Apply styles
                worksheet.Cells[1, 1, 3, 27].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Center align all merged rows
                worksheet.Cells[1, 1, 3, 27].Style.Font.Bold = true; // Make text bold

                // Add column headers (based on your table's structure)
                var headers = new[]
                { new[]
                    {
                        "SR.No", "Employee Name", "WD", "WO", "EL", "Ph Days", "Basic", "D.A", "HRA",
                        "Training", "Wash Allow", "Punch Allow", "P&A", "Misc Allow", "Perf Allow",
                        "Prod Inc", "Tot Earn", "P.T", "ESI", "EPF", "Sal.Adv", "IT", "LIC", "Medi",
                        "Tot.Dedu", "Net Pay", "Employee Signature"
                    },
                    new[]
                    {
                        "", "Employee No", "WD", "WO", "EL", "Ph Days", "Basic", "D.A", "HRA",
                        "Training", "Wash Allow", "Punch Allow", "P&A", "Misc Allow", "Perf Allow",
                        "Prod Inc", "Tot Earn", "P.T", "ESI", "EPF", "Sal.Adv", "IT", "LIC", "Medi",
                        "Tot.Dedu", "Net Pay", "Employee Signature"
                    }
                };

                // Iterate through each array in the headers array
                for (int rows = 0; rows < headers.Length; rows++)
                {
                    var currentHeaders = headers[rows];
                    for (int col = 0; col < currentHeaders.Length; col++)
                    {
                        worksheet.Cells[4 + rows, col + 1].Value = currentHeaders[col]; // Adjust row index
                        worksheet.Cells[4 + rows, col + 1].Style.Font.Bold = true;
                        worksheet.Cells[4 + rows, col + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        worksheet.Cells[4 + rows, col + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                }

                // Add data rows dynamically
                int row = 5; // Start from row 5 (after headers)
                int srNo = 1;

                //foreach (var item in data)
                //{
                //    worksheet.Cells[row, 1].Value = srNo++;
                //    worksheet.Cells[row, 2].Value = item.EmployeeName; // Replace with actual property names
                //    worksheet.Cells[row, 3].Value = item.WD;
                //    worksheet.Cells[row, 4].Value = item.WO;
                //    worksheet.Cells[row, 5].Value = item.EL;
                //    worksheet.Cells[row, 6].Value = item.PhDays;
                //    worksheet.Cells[row, 7].Value = item.Basic;
                //    worksheet.Cells[row, 8].Value = item.DA;
                //    worksheet.Cells[row, 9].Value = item.HRA;
                //    // Continue for all properties...
                //    row++;
                //}

                // Auto-fit columns for better readability
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Export to a memory stream
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                // Return the Excel file as a download
                string fileName = "SalaryStatement.xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(stream, contentType, fileName);
            }
        }
        #endregion


    }
}