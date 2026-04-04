using ClosedXML.Excel;
using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_Leave
{
    public class Reports_Leave_LeaveMonthlyCreditReportController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionStrings);
        DynamicParameters param = new DynamicParameters();
        // GET: Reports/Reports_Leave_LeaveMonthlyCreditReport
        #region Reports_Leave_LeaveMonthlyCreditReport

        public ActionResult Reports_Leave_LeaveMonthlyCreditReport()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 831;
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
                var CmpId = GetComapnyName[0].Id;

                DynamicParameters param = new DynamicParameters();
               // param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                ViewBag.GetMasBranch = "";
                ViewBag.GetEmployeeList = "";
                //DynamicParameters paramEmpName = new DynamicParameters();
                //paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpId + " and EmployeeBranchId=" + BUID + " and Mas_Employee.EmployeeLeft=0");
                //ViewBag.GetEmployeeList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();

                DynamicParameters paramLeaveType = new DynamicParameters();
                paramLeaveType.Add("@query", "select LeaveTypeId as Id, LeaveTypeShortName as Name  from Leave_Type where IsActive=1");
                var LeaveType= DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLeaveType).ToList();
                ViewBag.LeaveType = LeaveType;

                param = new DynamicParameters();
                param.Add("@query", "select LeaveYearID as Id, cast(year(FromDate) as nvarchar(4))+'-'+cast(YEAR(ToDate) as nvarchar(4)) as Name from[dbo].[Leave_Year] where Deactivate = 0  and IsActivate=1  and CmpId='" + CmpId + "' order by IsDefault desc,FromDate desc");
                var LeaveYearGet = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetLeaveYear = LeaveYearGet;

                ViewBag.GetLeaveSummaryList = "";
                return View();
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
                // param.Add("@p_employeeid", Session["EmployeeId"]);
                //param.Add("@p_CmpId", CmpId);
                //var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                DynamicParameters paramBranchName = new DynamicParameters();
                paramBranchName.Add("@query", "select BranchId as Id, BranchName as Name  from Mas_Branch where Deactivate = 0 and CmpId ='" + CmpId + "'  order by Name");
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranchName).ToList();
                ViewBag.GetMasBranch = Branch;

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName, ' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate = 0 and EmployeeLeft=0 AND CmpID='" + CmpId + "'  order by Name");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                ViewBag.GetEmployeeList = EmployeeName;
                
                return Json(new { Branch = Branch, EmployeeName = EmployeeName }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetEmployee
        [HttpGet]
        public ActionResult GetEmployee(int? CmpId, int? BranchId)
        {
            try
            {

                DynamicParameters param = new DynamicParameters();
                if (BranchId != null)
                {
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName, ' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate = 0 and EmployeeLeft=0 AND CmpID='" + CmpId + "' AND EmployeeBranchId='" + BranchId + "' order by Name");
                }
                else
                {
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName, ' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate = 0 and EmployeeLeft=0 AND CmpID='" + CmpId + "' order by Name");

                }
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetEmployeeList = data;
                
                return Json(data, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, int? EmployeeId, int? LeaveYear,int? LeaveTypeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("LeaveSummaryReports");
                worksheet.Range(1, 1, 1, 20).Merge();
                worksheet.SheetView.FreezeRows(2); // Freeze the first row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();

                if (CmpId != 0)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    if (EmployeeId == null)
                    {
                        paramList.Add("@P_EmployeeIds", "All");
                    }
                    else
                    {
                        paramList.Add("@P_EmployeeIds", EmployeeId);
                    }
                    paramList.Add("@p_Cmpid", CmpId);
                    paramList.Add("@p_BranchID", BranchId);
                    paramList.Add("@p_LeaveYearID", LeaveYear);
                    paramList.Add("@p_LeaveTypeId", LeaveTypeId);
                    //DynamicParameters paramList = new DynamicParameters();
                    //paramList.Add("@P_Qry", Query);
                    var LeftEmployeeReportList = DapperORM.ReturnList<dynamic>("sp_Rpt_Leave_LeaveMonthlyCredit", paramList).ToList();

                    if (LeftEmployeeReportList.Count == 0)
                    {
                        byte[] emptyFileContents = new byte[0];
                        return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(LeftEmployeeReportList);
                    worksheet.Cell(2, 1).InsertTable(dt, false);
                }
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
                var FromDate = DateTime.Now.Date;
                worksheet.Cell(1, 1).Value = "Leave Monthly Credit Report - (" + FromDate.ToString("dd/MMM/yyyy") + ")";
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Columns().AdjustToContents(); // This code for all clomns

                var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                headerRange.Style.Font.FontSize = 10;
                headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
                headerRange.Style.Font.Bold = true;

                // Save the workbook
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