using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_Attendance
{
    public class Rpt_Attendance_CheckInOutReportController : Controller
    {
        // GET: Reports/Rpt_Daily_Attendance
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();

        #region  // GET: Reports/Rpt_Attendance_CheckInOutReport
        public ActionResult Rpt_Attendance_CheckInOutReport(DailyAttendanceReportFilter DailyAttendanceReportFilter)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 744;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                //GET COMPANY NAME
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                var CmpId = GetComapnyName[0].Id;

                //GET BRANCH NAME
                //var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CmpId), Convert.ToInt32(Session["EmployeeId"]));
                //ViewBag.BranchName = Branch;
                //var BranchId = Branch[0].Id;
                var paramIsAddress = new DynamicParameters();
                paramIsAddress.Add("@query", @"SELECT TOP 1 LocationApiAddressApplicable FROM Tool_CommonTable");
                var Result = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramIsAddress).FirstOrDefault();
                var LocationApiAddressApplicable = Result?.LocationApiAddressApplicable ?? 0;
                if (LocationApiAddressApplicable == true)
                {
                    ViewBag.LocationAddressApplicable = true;
                }
                else
                {
                    ViewBag.LocationAddressApplicable = false;
                }
                if (DailyAttendanceReportFilter.CmpId != null)
                {
                    DynamicParameters paramBranch = new DynamicParameters();
                    paramBranch.Add("@query", "select Distinct BranchId as Id,BranchName as Name from Mas_Branch where Deactivate=0 AND CmpId='" + DailyAttendanceReportFilter.CmpId + "';");
                    var branchList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranch).ToList();
                    ViewBag.BranchName = branchList;
                    
                }
                else
                {
                    ViewBag.BranchName = new List<AllDropDownBind>();
                }
                
                if (DailyAttendanceReportFilter.CmpId != null && DailyAttendanceReportFilter.BranchId != null)
                {

                    DynamicParameters paramEmp = new DynamicParameters();
                    paramEmp.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID ='" + DailyAttendanceReportFilter.CmpId + "' and EmployeeId<>1 and EmployeeBranchId=" + DailyAttendanceReportFilter.BranchId + " and EmployeeLeft=0 order by Name");
                    var EmployeeList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmp).ToList();
                    ViewBag.EmployeeName = EmployeeList;
                }
                if (DailyAttendanceReportFilter.CmpId != null)
                {
                    DynamicParameters paramEmp = new DynamicParameters();
                    paramEmp.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID ='" + DailyAttendanceReportFilter.CmpId + "' and EmployeeId<>1 and EmployeeLeft=0 order by Name");
                    var EmployeeList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmp).ToList();
                    ViewBag.EmployeeName = EmployeeList;

                }
                else
                {
                    ViewBag.EmployeeName = new List<AllDropDownBind>();
                }

                if (DailyAttendanceReportFilter.FromDate != DateTime.MinValue && DailyAttendanceReportFilter.ToDate != DateTime.MinValue)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", DailyAttendanceReportFilter.EmployeeID);
                    paramList.Add("@p_FromDate", DailyAttendanceReportFilter.FromDate);
                    paramList.Add("@p_ToDate", DailyAttendanceReportFilter.ToDate);

                    paramList.Add("@p_CompanyId", DailyAttendanceReportFilter.CmpId);
                    paramList.Add("@p_BranchId", DailyAttendanceReportFilter.BranchId);

                    var data = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Atten_CheckInOut", paramList).ToList();
                    ViewBag.Getdata = data;

                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Atten_CheckInOut", paramList).ToList();
                    ViewBag.GetCheckInOutList = GetData;
                    if (GetData.Count == 0)
                    {
                        TempData["Message"] = "Record Not Found";

                        TempData["Icon"] = "error";
                    }

                    if (GetData.Count > 0)
                    {
                        ViewBag.GetCheckInOutList = GetData;

                    }
                    else
                    {
                        TempData["Message"] = "Record Not Found";
                        TempData["Icon"] = "error";
                    }

                }
                else
                {
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromDate", null);
                    paramList.Add("@p_ToDate", null);
                    paramList.Add("@p_CompanyId", DailyAttendanceReportFilter.CmpId);
                    paramList.Add("@p_BranchId", DailyAttendanceReportFilter.BranchId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Atten_CheckInOut", paramList).ToList();
                    ViewBag.GetCheckInOutList = GetData;
                }
                return View(DailyAttendanceReportFilter);
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
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select Distinct BranchId as Id,BranchName as Name from Mas_Branch where Deactivate=0 AND CmpId='" + CmpId + "';");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);


                //var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CmpId), Convert.ToInt32(Session["EmployeeId"]));
                //return Json(new { Branch = Branch }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetEmployeeName
        [HttpGet]
        public ActionResult GetEmployeeNameDateWise(int ? CmpId, int ? BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                if (BranchId == 0 || BranchId == null )
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID ='" + CmpId + "' and EmployeeId<>1  and EmployeeLeft=0 order by Name");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID ='" + CmpId + "' and EmployeeId<>1 and EmployeeBranchId=" + BranchId + " and EmployeeLeft=0 order by Name");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ViewSelfie
        public ActionResult ViewSelfie(string filePath, string Id)
        {
            if (string.IsNullOrWhiteSpace(filePath) || filePath.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
            {
                return HttpNotFound("Invalid file name.");
            }

            string directoryPath = "";
            var GetPath = DapperORM.DynamicQuerySingle("select * from Tool_Documnet_DirectoryPath where DocOrigin='Checkinout'");
            directoryPath = GetPath.DocInitialPath;
            string fullPath = Path.Combine(directoryPath, Id,filePath);

            if (!System.IO.File.Exists(fullPath))
            {
                return HttpNotFound("File not found.");
            }

            string contentType = MimeMapping.GetMimeMapping(fullPath);
            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(fileBytes, contentType);
        }
        #endregion

        #region View Photo
        public ActionResult ViewPhoto(string filePath, string Id)
        {
            if (string.IsNullOrWhiteSpace(filePath) || filePath.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
            {
                return HttpNotFound("Invalid file name.");
            }

            string directoryPath = "";
            var GetPath = DapperORM.DynamicQuerySingle("select * from Tool_Documnet_DirectoryPath where DocOrigin='Onboarding'");
            directoryPath = GetPath.DocInitialPath;
            string fullPath = Path.Combine(directoryPath, Id,"Photo", filePath);

            if (!System.IO.File.Exists(fullPath))
            {
                return HttpNotFound("File not found.");
            }

            string contentType = MimeMapping.GetMimeMapping(fullPath);
            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(fileBytes, contentType);
        }
        #endregion

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, DateTime FromDate, DateTime ToDate, int? EmployeeID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("CheckInOut");
                worksheet.Range(1, 1, 1, 10).Merge();
                worksheet.SheetView.FreezeRows(2); // Freeze the first row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();


                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_CompanyId", CmpId);
                paramList.Add("@p_BranchId", BranchId);
                paramList.Add("@p_FromDate", FromDate);
                paramList.Add("@p_ToDate", ToDate);
                paramList.Add("@p_EmployeeId", EmployeeID);

                var GetData = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Atten_CheckInOut_Excel", paramList).ToList();

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
                worksheet.Cell(1, 1).Value = "Check InOut Report - (" + FromDate.ToString("dd/MMM/yyyy") + " to " + ToDate.ToString("dd/MMM/yyyy") + ")";
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