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
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_Leave
{
    public class Reports_Leave_LeaveSummaryController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionStrings);
        DynamicParameters param = new DynamicParameters();
        // GET: Reports/Reports_Leave_LeaveSummary
        #region Reports_Leave_LeaveSummary
        public ActionResult Reports_Leave_LeaveSummary(RptLeaveSummery MasReport)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 305;
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
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var GetMasBranch = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                ViewBag.GetMasBranch = GetMasBranch;
                var BUID = GetMasBranch[0].Id;

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpId + " and EmployeeBranchId=" + BUID + " and Mas_Employee.EmployeeLeft=0");
                ViewBag.GetEmployeeList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();

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

        #region GetUnit
        [HttpGet]
        public ActionResult GetUnit(int? CmpId, int? BranchId)
        {
            try
            {

                DynamicParameters param = new DynamicParameters();
                // param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and year( Atten_InOut.InOutDate )='" + GetYear + "' and EmployeeId<>1 and InOutBranchId=" + BranchId + " ) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid=" + BranchId + " and( year (mas_employee.JoiningDate)<='" + GetYear + "') and ( year(mas_employee.LeavingDate)='" + GetYear + "' or mas_employee.LeavingDate is null) order by Name");
                param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName, ' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate = 0 and employeebranchid = " + BranchId + " and EmployeeLeft = 0 order by Name");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);

                //else
                //{
                //    var a = "2024";
                //    DynamicParameters param = new DynamicParameters();
                //    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and year( Atten_InOut.InOutDate )='" + a + "' and EmployeeId<>1 and InOutBranchId=" + BranchId + " ) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid=" + BranchId + " and( year (mas_employee.JoiningDate)<='" + a + "') and ( year(mas_employee.LeavingDate)='" + a + "' or mas_employee.LeavingDate is null) order by Name");
                //    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                //    return Json(data, JsonRequestBehavior.AllowGet);
                //}
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

        #region Monthly Attendance DropDown
        [HttpGet]
        public ActionResult GetMonthlyEmployeeNameDateWise(int BranchId, string GetYear)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and year( Atten_InOut.InOutDate )='" + GetYear + "' and EmployeeId<>1 and InOutBranchId=" + BranchId + " ) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid=" + BranchId + " and( year (mas_employee.JoiningDate)<='" + GetYear + "') and ( year(mas_employee.LeavingDate)='" + GetYear + "' or mas_employee.LeavingDate is null) order by Name");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


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
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                DynamicParameters paramEmpName = new DynamicParameters();
                // param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and year( Atten_InOut.InOutDate )='" + GetFromDate + "' and EmployeeId<>1 and InOutBranchId=" + Branch[0].Id + " ) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid=" + Branch[0].Id + " and( year (mas_employee.JoiningDate)<='" + GetFromDate + "') and ( year(mas_employee.LeavingDate)='" + GetFromDate + "' or mas_employee.LeavingDate is null) order by Name");
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName, ' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate = 0 and employeebranchid = " + Branch[0].Id + " and EmployeeLeft = 0 order by Name");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();

                DynamicParameters paramYear = new DynamicParameters();
                paramYear.Add("@query", "select LeaveYearID as Id, cast(year(FromDate) as nvarchar(4))+'-'+cast(YEAR(ToDate) as nvarchar(4)) as Name from[dbo].[Leave_Year] where Deactivate = 0  and IsActivate=1  and CmpId='" + CmpId + "' order by IsDefault desc,FromDate desc");
                var LeaveYearGet = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramYear).ToList();

                return Json(new { Branch = Branch, EmployeeName = EmployeeName , LeaveYearGet = LeaveYearGet }, JsonRequestBehavior.AllowGet);

                //else
                //{
                //    DynamicParameters param = new DynamicParameters();
                //    param.Add("@p_employeeid", Session["EmployeeId"]);
                //    param.Add("@p_CmpId", CmpId);
                //    var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                //    DynamicParameters paramEmpName = new DynamicParameters();
                //    paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and year( Atten_InOut.InOutDate )='" + DateTime.Now.ToString("yyyy") + "' and EmployeeId<>1 and InOutBranchId=" + Branch[0].Id + " ) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid=" + Branch[0].Id + " and( year (mas_employee.JoiningDate)<='" + DateTime.Now.ToString("yyyy") + "') and ( year(mas_employee.LeavingDate)='" + DateTime.Now.ToString("yyyy") + "' or mas_employee.LeavingDate is null) order by Name");
                //    var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();

                //    return Json(new { Branch = Branch, EmployeeName = EmployeeName }, JsonRequestBehavior.AllowGet);
                //}

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, int? EmployeeId, int? LeaveYear/*, DateTime FromDate, int? SubUnitId, int? DepartmentId, int? SubDepartmentId, int? DesignationId, int? LineId, int? GradeId, DateTime ToDate*/)
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
                    //DynamicParameters paramList = new DynamicParameters();
                    //paramList.Add("@P_Qry", Query);
                    var LeftEmployeeReportList = DapperORM.ReturnList<dynamic>("sp_Rpt_Leave_LeaveSummery", paramList).ToList();

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
                worksheet.Cell(1, 1).Value = "Leave Summary Report - (" + FromDate.ToString("dd/MMM/yyyy") + ")";
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