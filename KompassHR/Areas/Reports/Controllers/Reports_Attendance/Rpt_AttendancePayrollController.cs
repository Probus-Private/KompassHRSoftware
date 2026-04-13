using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using KompassHR.Areas.Module.Models.Module_Employee;
using System.IO;

namespace KompassHR.Areas.Reports.Controllers.Reports_Attendance
{
    public class Rpt_AttendancePayrollController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Reports/Rpt_AttendancePayroll
        #region Rpt_AttendancePayroll
        public ActionResult Rpt_AttendancePayroll(DailyAttendanceReportFilter MasReport)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 355;
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
                var GetCmpId = GetComapnyName[0].Id;

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", GetCmpId);
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                ViewBag.BranchName = Branch;
                var GetBranchId = Branch[0].Id;

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + GetCmpId + " and EmployeeBranchId= " + GetBranchId + "and Mas_Employee.EmployeeLeft=0");
                ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();

                DynamicParameters paramLine = new DynamicParameters();
                paramLine.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + GetCmpId + " and Mas_LineMaster.BranchId=" + GetBranchId + " ");
                ViewBag.LineName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();

                DynamicParameters paramUnit = new DynamicParameters();
                paramLine.Add("@query", "select UnitId as Id,UnitName as Name from Mas_Unit where Deactivate=0 and CmpId=" + GetCmpId + " and UnitBranchId=" + GetBranchId + " ");
                ViewBag.SubUnit = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();


                var results = DapperORM.DynamicQueryMultiple(@"select DepartmentId as Id, DepartmentName as Name from Mas_Department where Deactivate=0 order by DepartmentName; 
                                                  select DesignationId as Id,DesignationName as Name from Mas_Designation where Deactivate=0 order by DesignationName
                                                  select GradeId as Id,GradeName as Name from Mas_Grade where Deactivate=0 order by Name");

                var EmpID = Session["EmployeeId"];

                //ViewBag.CompanyName = results.Read<AllDropDownClass>().ToList();
                ViewBag.DepartmentName = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.DesignationName = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GradeName = results[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.SubDepartmentName = "";

                //ViewBag.AttendancePayroll = "";
                //ViewBag.BranchName = "";
                //ViewBag.LineName = "";
                //ViewBag.EmployeeName = "";
                //ViewBag.SubUnit = "";


                //var Query = "";
                //if (MasReport.CmpId != null)
                //{
                //    Query = "and mas_employee.CmpID=" + MasReport.CmpId + "";

                //    if (MasReport.BranchId != null)
                //    {
                //        Query = Query + " and Mas_Branch.BranchId=" + MasReport.BranchId + "";
                //        DynamicParameters paramLine = new DynamicParameters();
                //        paramLine.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + MasReport.CmpId + " and Mas_LineMaster.BranchId=" + MasReport.BranchId + " ");
                //        var List_LineMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();
                //        ViewBag.LineName = List_LineMaster;

                //        DynamicParameters paramEMP = new DynamicParameters();
                //        paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + MasReport.CmpId + " and EmployeeBranchId= " + MasReport.BranchId + "and Mas_Employee.EmployeeLeft=0");
                //        var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                //        ViewBag.EmployeeName = GetEmployeeName;

                //        DynamicParameters paramSubUnit = new DynamicParameters();
                //        paramSubUnit.Add("@query", "select UnitId as Id,UnitName as Name from Mas_Unit where Deactivate=0 and CmpId=" + MasReport.CmpId + " and  UnitBranchId=" + MasReport.BranchId + " ");
                //        var GetSubUnit = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramSubUnit).ToList();
                //        ViewBag.SubUnit = GetSubUnit;

                //        if (MasReport.SubUnitId != null)
                //        {
                //            Query = Query + " and Mas_Unit.UnitId=" + MasReport.SubUnitId + "";
                //        }

                //    }
                //    else
                //    {
                //        //Query = " where BranchId in (select mas_branch.BranchID as Id from UserBranchMapping,mas_branch where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + MasReport.CmpId + " and UserBranchMapping.IsActive = 1)";
                //        ViewBag.LineName = "";
                //        ViewBag.EmployeeName = "";
                //        ViewBag.SubUnit = "";
                //    }
                //    if (MasReport.SubUnitId == null && MasReport.BranchId != null)
                //    {
                //        DynamicParameters paramSubUnit = new DynamicParameters();
                //        paramSubUnit.Add("@query", "select UnitId as Id,UnitName as Name from Mas_Unit where Deactivate=0 and CmpId=" + MasReport.CmpId + " and  UnitBranchId=" + MasReport.BranchId + " ");
                //        var GetSubUnit = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramSubUnit).ToList();
                //        ViewBag.SubUnit = GetSubUnit;
                //    }
                //    if (MasReport.DepartmentId != null)
                //    {
                //        Query = Query + " and  DepartmentId=" + MasReport.DepartmentId + "";

                //        DynamicParameters paramSub = new DynamicParameters();
                //        paramSub.Add("@query", "Select SubDepartmentId As Id,SubDepartmentName as Name from Mas_SubDepartment where Mas_SubDepartment.Deactivate=0 and Mas_SubDepartment.DepartmentId=" + MasReport.DepartmentId + "");
                //        var DataDept = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramSub).ToList();
                //        ViewBag.SubDepartmentName = DataDept;
                //    }
                //    else
                //    {
                //        ViewBag.SubDepartmentName = "";
                //    }
                //    //if (MasReport.SubDepartmentId != null)
                //    //{
                //    //    Query = Query + " and SubDepartementId=" + MasReport.SubDepartmentId + "";
                //    //}
                //    if (MasReport.DesignationId != null)
                //    {
                //        Query = Query + " and DesignationId=" + MasReport.DesignationId + "";
                //    }
                //    if (MasReport.GradeId != null)
                //    {
                //        Query = Query + " and GradeId=" + MasReport.GradeId + "";
                //    }
                //    if (MasReport.LineId != null)
                //    {
                //        Query = Query + " and LineId=" + MasReport.LineId + "";
                //    }
                //    if (MasReport.EmployeeID != null)
                //    {
                //        Query = Query + " and EmployeeID=" + MasReport.EmployeeID + "";
                //    }
                //    //var GetFromDate = MasReport.FromDate.ToString("yyyy-MM-dd");
                //    //var GetToDate = MasReport.ToDate.ToString("yyyy-MM-dd");
                //    //if (MasReport.FromDate != null && MasReport.ToDate != null)
                //    //{
                //    //    Query = Query + " and CONVERT(date, Atten_InOut.InOutDate) BETWEEN  '" + GetFromDate + "' AND '" + GetToDate + "'and JoiningDate<='" + GetFromDate + "' and (LeavingDate>='" + GetFromDate + "'or LeavingDate is null) ";
                //    //}
                //    DynamicParameters paramList = new DynamicParameters();
                //    var month = MasReport.FromDate.ToString("MM");
                //    var year = MasReport.FromDate.ToString("yyyy");
                //    paramList.Add("@p_BranchId", MasReport.BranchId);
                //    paramList.Add("@P_Month", month);
                //    paramList.Add("@p_Year", year);
                //    paramList.Add("@p_qry", Query);
                //    var GetAttendancePayroll = DapperORM.ReturnList<dynamic>("sp_Rpt_Atten_MonthlyAttendanceForPayroll", paramList).ToList();
                //    ViewBag.AttendancePayroll = GetAttendancePayroll;

                //    if (GetAttendancePayroll.Count == 0 || GetAttendancePayroll == null)
                //    {
                //        TempData["Message"] = "Records Not Found !";
                //        TempData["Icon"] = "error";
                //    }

                //    DynamicParameters paramBranchList = new DynamicParameters();
                //    paramBranchList.Add("@p_employeeid", Session["EmployeeId"]);
                //    paramBranchList.Add("@p_CmpId", MasReport.CmpId);
                //    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranchList).ToList();
                //    ViewBag.BranchName = data;
                //    TempData["ShowData"] = true;
                //}
                //else
                //{
                //    ViewBag.AttendancePayroll = "";
                //    ViewBag.BranchName = "";
                //    ViewBag.SubDepartmentName = "";
                //    ViewBag.LineName = "";
                //    ViewBag.EmployeeName = "";
                //    ViewBag.SubUnit = "";
                //}


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
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                var GetBranchId = Branch[0].Id;

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpId + " and EmployeeBranchId= " + GetBranchId + "and Mas_Employee.EmployeeLeft=0");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();

                DynamicParameters paramLine = new DynamicParameters();
                paramLine.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + CmpId + " and Mas_LineMaster.BranchId=" + GetBranchId + " ");
                var List_LineMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();

                DynamicParameters paramUnit = new DynamicParameters();
                paramLine.Add("@query", "select UnitId as Id,UnitName as Name from Mas_Unit where Deactivate=0 and CmpId=" + CmpId + " and UnitBranchId=" + GetBranchId + " ");
                var List_SubUnit = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();



                return Json(new { EmployeeName = EmployeeName, List_LineMaster = List_LineMaster, Branch = Branch , Unit= List_SubUnit }, JsonRequestBehavior.AllowGet);
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

                    DynamicParameters paramUnit = new DynamicParameters();
                    paramLine.Add("@query", "select UnitId as Id,UnitName as Name from Mas_Unit where Deactivate=0 and CmpId=" + CmpId + " and UnitBranchId=" + UnitBranchId + " ");
                    var List_SubUnit = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();

                    return Json(new { EmployeeName = EmployeeName, List_LineMaster = List_LineMaster, List_SubUnit = List_SubUnit }, JsonRequestBehavior.AllowGet);
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
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, DateTime FromDate, int? SubUnitId, int? DepartmentId, int? SubDepartmentId, int? DesignationId, int? LineId, int? EmployeeID, int? GradeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("AttendancePayrollReport");
                worksheet.Range(1, 1, 1, 25).Merge();
                worksheet.SheetView.FreezeRows(2); // Freeze the row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();

                var Query = "";
                if (CmpId != null)
                {
                    Query = "and mas_employee.CmpID=" + CmpId + "";

                    if (BranchId != null)
                    {
                        Query = Query + " and Atten_InOut.InOutBranchId=" + BranchId + "";
                    }
                    else
                    {
                        Query = " where Atten_InOut.InOutBranchId in (select Mas_Branch.BranchID as Id from UserBranchMapping,Mas_Branch where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and Mas_Branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and Mas_Branch.CmpId = " + CmpId + " and UserBranchMapping.IsActive = 1)";
                    }
                    if (SubUnitId != null)
                    {
                        Query = Query + " and Mas_Unit.UnitId=" + SubUnitId + "";
                    }

                    if (DepartmentId != null)
                    {
                        Query = Query + " and  Mas_Department.DepartmentId=" + DepartmentId + "";
                    }

                    if (SubDepartmentId != null)
                    {
                        Query = Query + " and Mas_SubDepartment.SubDepartmentId=" + SubDepartmentId + "";
                    }
                    if (DesignationId != null)
                    {
                        Query = Query + " and Mas_Designation.DesignationId=" + DesignationId + "";
                    }
                    if (GradeId != null)
                    {
                        Query = Query + " and Mas_Grade.GradeId=" + GradeId + "";
                    }
                    if (LineId != null)
                    {
                        Query = Query + " and Mas_LineMaster.LineId=" + LineId + "";
                    }
                    if (EmployeeID != null)
                    {
                        Query = Query + " and Mas_employee.EmployeeID=" + EmployeeID + "";
                    }
                }

                DynamicParameters paramList = new DynamicParameters();
                var month = FromDate.ToString("MM");
                var year = FromDate.ToString("yyyy");
                paramList.Add("@p_BranchId", BranchId);
                paramList.Add("@P_Month", month);
                paramList.Add("@p_Year", year);
                paramList.Add("@p_qry", Query);
                data = DapperORM.ReturnList<dynamic>("sp_Rpt_Atten_MonthlyAttendanceForPayroll", paramList).ToList();

                if (data.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }

                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(data);

                //dt.Rows.RemoveAt(0);
                worksheet.Cell(2, 1).InsertTable(dt, false);

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
                worksheet.Cell(1, 1).Value = "Attendance Payroll Report - (" + FromDate.ToString("MMM/yyyy") + ")";
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(1, 1).Style.Font.Bold = true;

                // Set the header row background color to grey and font color to black
                var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                headerRange.Style.Font.FontSize = 10;
                headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
                headerRange.Style.Font.Bold = true;
                worksheet.Columns().AdjustToContents();

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