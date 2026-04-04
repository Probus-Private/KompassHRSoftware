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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace KompassHR.Areas.Reports.Controllers.Reports_Attendance
{
    public class Rpt_Attendance_WorkDurationController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        // GET: Reports/Rpt_Attendance_WorkDuration
        #region Main View 
        public ActionResult Rpt_Attendance_WorkDuration(DailyAttendanceReportFilter MasReport)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 365;
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

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetUnit
        [HttpGet]
        public ActionResult GetUnit(int? CmpId, int? UnitBranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (UnitBranchId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpId + " and EmployeeBranchId= " + UnitBranchId + "and Mas_Employee.EmployeeLeft=0");
                    var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                    DynamicParameters paramLine = new DynamicParameters();
                    paramLine.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + CmpId + " and Mas_LineMaster.BranchId=" + UnitBranchId + " ");
                    var List_LineMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();

                    return Json(new { EmployeeName = EmployeeName, List_LineMaster = List_LineMaster }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
                //DynamicParameters paramLine = new DynamicParameters();
                //paramLine.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + CmpId + " and Mas_LineMaster.BranchId=" + UnitBranchId + " order by Name");
                //var List_LineMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();
                //return Json(List_LineMaster, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetSubDepartment
        [HttpGet]
        public ActionResult GetSubDepartment(int? DepartmentId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (DepartmentId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "Select SubDepartmentId As Id,SubDepartmentName as Name from Mas_SubDepartment where Mas_SubDepartment.Deactivate=0 and Mas_SubDepartment.DepartmentId=" + DepartmentId + "");
                    var Data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    return Json(Data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int CmpId, int? BranchId, DateTime FromDate, int? ContractorId)
        {
            try
            {
                var employeeId = Convert.ToInt32(Session["EmployeeId"]);
                DynamicParameters Branch = new DynamicParameters();
                //Branch.Add("@p_employeeid", Session["EmployeeId"]);
                Branch.Add("@p_employeeid", employeeId);
                Branch.Add("@p_CmpId", CmpId);
                var allBranches = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", Branch).ToList();
                // Filter branch list if a specific BranchId is provided
                var selectedBranches = BranchId != null
                    ? allBranches.Where(b => b.Id == BranchId.Value).ToList()
                    : allBranches;

                var workbook = new XLWorkbook();
                foreach (var branch in selectedBranches)
                {
                    string branchName = Regex.Replace(branch.Name, @"[\\/\?\*\[\]]", "-");
                    double branchId = branch.Id;
                    DataTable dt = new DataTable();
                    List<dynamic> data = new List<dynamic>();
                    param.Add("@p_MonthYear", FromDate);
                    param.Add("@p_BranchId", branchId);
                    param.Add("@p_ContractorId", ContractorId?.ToString() ?? "");
                    data = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Atten_MonthlyWorkDuration", param).ToList();
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(data);

                    // Create worksheet regardless of data presence
                    var worksheet = workbook.Worksheets.Add(branchName);
                    worksheet.Range(1, 1, 1, 41).Merge();
                    worksheet.SheetView.FreezeRows(2); // Freeze the row

                    // Set the header row
                    worksheet.Cell(1, 1).Value = "Work Duration Wise Attendance Report - (" + FromDate.ToString("MMM/yyyy") + ") - " + branchName;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(1, 1).Style.Font.Bold = true;

                    // Only remove header row if dt has rows
                    //if (dt.Rows.Count > 0)
                    //{
                    //    dt.Rows.RemoveAt(0);
                    //}
                    if (data.Count == 0)
                    {
                        if (dt.Rows.Count == 0)
                        {
                            continue;
                        }
                        byte[] emptyFileContents = new byte[0];
                        return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                    }

                    worksheet.Cell(2, 1).InsertTable(dt, false); // Insert data starting from row 2
                    int totalRows = worksheet.RowsUsed().Count();
                    var colors = new[] { XLColor.FromArgb(205, 222, 172), XLColor.FromArgb(205, 222, 172) };
                    var FontColors = new[] { XLColor.Black, XLColor.Black };
                    int colorIndex = 0;

                    int ao = 3; // Start index for formatting
                    int ab = 0;

                    for (int m = 0; m < totalRows / 12; m++)
                    {
                        ab = ao + 10;
                        var groupRange = worksheet.Range(ao - 1, 1, ao - 1, worksheet.LastColumnUsed().ColumnNumber());
                        groupRange.Style.Fill.BackgroundColor = colors[colorIndex % colors.Length];
                        groupRange.Style.Font.FontColor = FontColors[colorIndex % FontColors.Length];
                        colorIndex++;
                        ao = ao + 12;
                    }

                    worksheet.Columns().AdjustToContents();
                    var usedRange = worksheet.RangeUsed();
                    var cellsToStyle = usedRange.Cells().Where(cell => cell.Value.ToString() == "AB");

                    foreach (var cell in cellsToStyle)
                    {
                        cell.Style.Font.FontColor = XLColor.Red;
                    }
                    usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Font.FontName = "Calibri"; // Change the font name
                    usedRange.Style.Font.FontSize = 10;
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0; // Reset the stream position to the beginning
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

        #region For Testing Not In Use
        //public ActionResult StartExcelGeneration(int CmpId, int? BranchId, DateTime FromDate, int? ContractorId)
        //{
        //    try
        //    {
        //        string connStr = Session["MyNewConnectionString"]?.ToString();
        //        // Generate unique file name
        //        string fileId = Guid.NewGuid().ToString();
        //        string filePath = Server.MapPath($"~/assets/TempReport/Report_{fileId}.xlsx");

        //        // Run the long SP + Excel generation in background thread
        //        Task.Run(() =>
        //        {
        //            GenerateExcelReport(filePath, CmpId, BranchId, FromDate, ContractorId, connStr); 
        //        });

        //        // Return the file ID immediately so client can later check/download
        //        return Json(new { success = true, fileId = fileId }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        //// Background worker method
        //private void GenerateExcelReport(string filePath, int CmpId, int? BranchId, DateTime FromDate, int? ContractorId, string connStr)
        //{
        //    using (var conn = new SqlConnection(connStr))
        //    {
        //        var employeeId = Convert.ToInt32(Session["EmployeeId"]);
        //        var workbook = new XLWorkbook();
        //        DynamicParameters Branch1 = new DynamicParameters();
        //        Branch1.Add("@p_employeeid", employeeId);
        //        Branch1.Add("@p_CmpId", CmpId);

        //        var allBranches = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", Branch1).ToList();
        //        var selectedBranches = BranchId != null
        //            ? allBranches.Where(b => b.Id == BranchId.Value).ToList()
        //            : allBranches;

        //        foreach (var branch in selectedBranches)
        //        {
        //            string branchName = Regex.Replace(branch.Name, @"[\\/\?\*\[\]]", "-");
        //            double branchId = branch.Id;

        //            DynamicParameters param = new DynamicParameters();
        //            param.Add("@p_MonthYear", FromDate);
        //            param.Add("@p_BranchId", branchId);
        //            param.Add("@p_ContractorId", ContractorId?.ToString() ?? "");

        //            var data = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Atten_MonthlyWorkDuration", param).ToList();
        //            DataTable dt = new DapperORM().ConvertToDataTable(data);

        //            if (data.Count == 0)
        //                continue;

        //            var ws = workbook.Worksheets.Add(branchName);
        //            ws.Cell(1, 1).InsertTable(dt);
        //            ws.Columns().AdjustToContents();
        //        }

        //        workbook.SaveAs(filePath);
        //    }
        //}

        //public ActionResult DownloadGeneratedExcel(string fileId)
        //{
        //    string filePath = Server.MapPath($"~/assets/TempReport/Report_{fileId}.xlsx");
        //    if (!System.IO.File.Exists(filePath))
        //        return Json(new { success = false, message = "Report not ready yet" }, JsonRequestBehavior.AllowGet);

        //    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
        //    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "WorkDuration.xlsx");
        //}
        #endregion

        //public ActionResult StartExcelGeneration(int CmpId, int? BranchId, DateTime FromDate, int? ContractorId)
        //{
        //    try
        //    {
        //        string downloadId = Guid.NewGuid().ToString();
        //        string filePath = Server.MapPath($"~/TempReport/Report_{downloadId}.xlsx");

        //        // Start background thread
        //        Thread backgroundThread = new Thread(() =>
        //        {
        //            try
        //            {
        //                GenerateExcelFile(CmpId, BranchId, FromDate, ContractorId, filePath);
        //            }
        //            catch (Exception ex)
        //            {
        //                // Optionally log error
        //                System.IO.File.WriteAllText(Server.MapPath($"~/TempReport/Error_{downloadId}.txt"), ex.ToString());
        //            }
        //        });

        //        backgroundThread.IsBackground = true;
        //        backgroundThread.Start();

        //        // Return the download ID so JS can check status
        //        return Json(new { success = true, downloadId = downloadId }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        //private void GenerateExcelFile(int CmpId, int? BranchId, DateTime FromDate, int? ContractorId, string filePath)
        //{
        //    DynamicParameters Branch = new DynamicParameters();
        //    Branch.Add("@p_employeeid",Session["EmployeeId"]);
        //    Branch.Add("@p_CmpId", CmpId);
        //    var allBranches = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", Branch).ToList();

        //    var selectedBranches = BranchId != null
        //        ? allBranches.Where(b => b.Id == BranchId.Value).ToList()
        //        : allBranches;

        //    var workbook = new XLWorkbook();

        //    foreach (var branch in selectedBranches)
        //    {
        //        string branchName = Regex.Replace(branch.Name, @"[\\/\?\*\[\]]", "-");
        //        double branchIdVal = branch.Id;
        //        DataTable dt = new DataTable();
        //        List<dynamic> data = new List<dynamic>();
        //        DynamicParameters param = new DynamicParameters();
        //        param.Add("@p_MonthYear", FromDate);
        //        param.Add("@p_BranchId", branchIdVal);
        //        param.Add("@p_ContractorId", ContractorId?.ToString() ?? "");
        //        data = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Atten_MonthlyWorkDuration", param).ToList();
        //        DapperORM dprObj = new DapperORM();
        //        dt = dprObj.ConvertToDataTable(data);

        //        var worksheet = workbook.Worksheets.Add(branchName);
        //        worksheet.Range(1, 1, 1, 41).Merge();
        //        worksheet.SheetView.FreezeRows(2);
        //        worksheet.Cell(1, 1).Value = "Work Duration Wise Attendance Report - (" + FromDate.ToString("MMM/yyyy") + ") - " + branchName;
        //        worksheet.Cell(1, 1).Style.Font.Bold = true;
        //        worksheet.Cell(2, 1).InsertTable(dt, false);
        //        worksheet.Columns().AdjustToContents();
        //    }

        //    workbook.SaveAs(filePath);
        //}

        //public ActionResult CheckExcelStatus(string downloadId)
        //{
        //    string filePath = Server.MapPath($"~/TempReport/Report_{downloadId}.xlsx");
        //    bool exists = System.IO.File.Exists(filePath);
        //    return Json(new { ready = exists }, JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult DownloadExcel(string downloadId)
        //{
        //    string filePath = Server.MapPath($"~/TempReport/Report_{downloadId}.xlsx");

        //    if (System.IO.File.Exists(filePath))
        //    {
        //        byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
        //        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"WorkDuration_{downloadId}.xlsx");
        //    }
        //    else
        //    {
        //        return HttpNotFound("File not ready yet");
        //    }
        //}

    }
}