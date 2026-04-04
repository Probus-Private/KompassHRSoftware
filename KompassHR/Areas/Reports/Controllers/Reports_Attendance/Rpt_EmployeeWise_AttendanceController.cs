using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Data;
using System.IO;
using ClosedXML.Excel;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_Attendance
{
    public class Rpt_EmployeeWise_AttendanceController : Controller
    {
        // GET: Reports/Rpt_EmployeeWise_Attendance
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        #region Main View
        public ActionResult Rpt_EmployeeWise_Attendance(MonthWiseFilter Obj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 266;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param1 = new DynamicParameters();
                ViewBag.AddUpdateTitle = "Add";

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.CompanyName = GetComapnyName;

                var CmpId = GetComapnyName[0].Id;
                DynamicParameters param6 = new DynamicParameters();
                param6.Add("@p_employeeid", Session["EmployeeId"]);
                param6.Add("@p_CmpId", CmpId);
                var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param6).ToList();
                ViewBag.BranchName = BranchName;
                var BUID = BranchName[0].Id;
                var GetDateMonth = DateTime.Now.Date.ToString("MM");
                var GetDateYear = DateTime.Now.Date.ToString("yyyy");
                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and month( InOutMonthYear)=" + GetDateMonth + " and Year(InOutMonthYear)=" + GetDateYear + " and InOutBranchId=" + BUID + " ) order by Name");
                ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();

                if (Obj.Month != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_Date", Obj.Month);
                    param.Add("@p_BranchId", Obj.BranchId);
                    param.Add("@p_EmployeeId", Obj.EmployeeId);
                    var EmployeeWiseReport = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Atten_EmployeeWise", param).ToList();
                    ViewBag.EmployeeWiseReport = EmployeeWiseReport;

                    DynamicParameters param4 = new DynamicParameters();
                    param4.Add("@p_Date", Obj.Month);
                    param4.Add("@p_BranchId", Obj.BranchId);
                    param4.Add("@p_EmployeeId", Obj.EmployeeId);
                    var EmployeeWiseSummeryReport = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Atten_EmployeeWiseSummery", param4).ToList();
                    ViewBag.EmployeeWiseSummeryReport = EmployeeWiseSummeryReport;

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@p_employeeid", Session["EmployeeId"]);
                    param2.Add("@p_CmpId", Obj.CmpId);
                    ViewBag.BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param2).ToList();
                    var GetMonth = "";
                    var GetMonthYear = "";
                    if (Obj.Month.HasValue)
                    {
                        GetMonth = Obj.Month.Value.ToString("MM");
                        GetMonthYear = Obj.Month.Value.ToString("yyyy");
                    }

                    DynamicParameters param3 = new DynamicParameters();
                    param3.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and month( InOutMonthYear)=" + GetMonth + " and Year(InOutMonthYear)=" + GetMonthYear + " and InOutBranchId=" + Obj.BranchId + " ) order by Name");
                    var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param3).ToList();
                    ViewBag.EmployeeName = EmployeeName;

                    DynamicParameters EmployeeDetails = new DynamicParameters();
                    EmployeeDetails.Add("@query", @"Select Mas_Employee.EmployeeName, DepartmentName, DesignationName, EmployeeNo from Mas_Employee
                                                    join Mas_Department on Mas_Department.DepartmentId = Mas_Employee.EmployeeDepartmentID
                                                    join Mas_Designation on Mas_Designation.DesignationId = Mas_Employee.EmployeeDesignationID
                                                    Where Mas_Employee.EmployeeId =" + Obj.EmployeeId + " and Mas_Employee.Deactivate = 0");
                    var datas = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", EmployeeDetails).ToList();
                    ViewBag.GetEmployeeDetails = datas;
                }
                else
                {
                    ViewBag.EmployeeWiseReport = "";
                    ViewBag.EmployeeWiseSummeryReport = "";
                    ViewBag.GetEmployeeDetails = "";
                }
                return View(Obj);
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
        public ActionResult GetBusinessUnit(int CmpId, DateTime GetMonthYear)
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
                var BranchIDD = Branch[0].Id;
                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and month( InOutMonthYear)=" + GetMonthYear.ToString("MM") + " and Year(InOutMonthYear)=" + GetMonthYear.ToString("yyyy") + " and InOutBranchId=" + BranchIDD + " ) order by Name");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                //ViewBag.EmployeeName = EmployeeName;


                return Json(new { EmployeeName = EmployeeName, Branch = Branch }, JsonRequestBehavior.AllowGet);
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
        public ActionResult GetEmployeeName(int CmpId, int BranchId, DateTime GetMonth)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and month( InOutMonthYear)=" + GetMonth.ToString("MM") + " and Year(InOutMonthYear)=" + GetMonth.ToString("yyyy") + " and InOutBranchId=" + BranchId + " ) order by Name");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                //DynamicParameters param = new DynamicParameters();
                //param.Add("@query", @"select EmployeeId as Id ,EmployeeName as Name from Mas_Employee  where Deactivate=0 and EmployeeBranchId=" + BranchId + "");
                //var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
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
        public ActionResult GetEmployeeNameDateWise(int BranchId, DateTime GetMonth)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and month( InOutMonthYear)=" + GetMonth.ToString("MM") + " and Year(InOutMonthYear)=" + GetMonth.ToString("yyyy") + " and InOutBranchId=" + BranchId + " ) order by Name");
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

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, DateTime GetMonth, int? EmployeeId)
        {
            try
            {

                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                Rpt_Atten_EmployeeWiseSummery EmpSummary = new Rpt_Atten_EmployeeWiseSummery();

                DynamicParameters param4 = new DynamicParameters();
                param4.Add("@p_Date", GetMonth);
                param4.Add("@p_BranchId", BranchId);
                param4.Add("@p_EmployeeId", EmployeeId);
                EmpSummary = DapperORM.ReturnList<Rpt_Atten_EmployeeWiseSummery>("sp_Rpt_Atten_EmployeeWiseSummery_Excel", param4).FirstOrDefault();
                ViewBag.EmployeeWiseSummeryReport = EmpSummary;

                var absent = EmpSummary.AbsentDays;

                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("EmployeeWiseSummary");
                worksheet.Range(1, 1, 1, 35).Merge();
                worksheet.SheetView.FreezeRows(9); // Freeze the first row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();

                //DynamicParameters param = new DynamicParameters();
                //param.Add("@p_Date", GetMonth);
                //param.Add("@p_BranchId", BranchId);
                //param.Add("@p_EmployeeId", EmployeeId);
                //var EmployeeWiseReport = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Atten_EmployeeWise", param).ToList();
                //ViewBag.EmployeeWiseReport = EmployeeWiseReport;

                DynamicParameters paramList = new DynamicParameters();

                paramList.Add("@p_EmployeeId", EmployeeId);
                paramList.Add("@p_BranchId", BranchId);
                paramList.Add("@p_Date", GetMonth.ToString("yyyy-MM-dd"));
                //paramList.Add("@p_CompanyId", CmpId);
                //paramList.Add("@p_BranchId", BranchId);
                var GetDailyAttendanceReport = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Atten_EmployeeWise", paramList).ToList();
                if (GetDailyAttendanceReport.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }

                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(GetDailyAttendanceReport);
                worksheet.Cell(9, 1).InsertTable(dt, false);
                int totalRows = worksheet.RowsUsed().Count();



                /////////////////////

                worksheet.Cell(1, 1).Value = "Probus Software Pvt Ltd";
                worksheet.Cell(1, 1).Style.Font.SetBold(true);
                worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                worksheet.Range("A1:R1").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                worksheet.Range("A1:B1").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("A1:B1").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("A1:B1").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("A1:B1").Style.Border.RightBorder = XLBorderStyleValues.Medium;



                worksheet.Range("A2:B2").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("A2:B2").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("A2:B2").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("A2:B2").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("A2:B2").Style.Border.RightBorder = XLBorderStyleValues.Medium;



                worksheet.Range("A3:B3").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("A3:B3").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("A3:B3").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("A3:B3").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("A3:B3").Style.Border.RightBorder = XLBorderStyleValues.Medium;



                worksheet.Range("A4:B4").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("A4:B4").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("A4:B4").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("A4:B4").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("A4:B4").Style.Border.RightBorder = XLBorderStyleValues.Medium;



                worksheet.Range("A5:B5").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("A5:B5").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("A5:B5").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("A5:B5").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("A5:B5").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Range("A6:B6").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("A6:B6").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("A6:B6").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("A6:B6").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("A6:B6").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Range("A7:B7").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("A7:B7").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("A7:B7").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("A7:B7").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("A7:B7").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Range("E2:F2").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("E2:F2").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("E2:F2").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("E2:F2").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("E2:F2").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Range("E3:F3").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("E3:F3").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("E3:F3").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("E3:F3").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("E3:F3").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Range("E4:F4").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("E4:F4").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("E4:F4").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("E4:F4").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("E4:F4").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Range("E5:F5").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("E5:F5").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("E5:F5").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("E5:F5").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("E5:F5").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Range("E6:F6").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("E6:F6").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("E6:F6").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("E6:F6").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("E6:F6").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Range("E7:F7").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("E7:F7").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("E7:F7").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("E7:F7").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("E7:F7").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Range("I2:J2").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("I2:J2").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("I2:J2").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("I2:J2").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("I2:J2").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Range("I3:J3").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("I3:J3").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("I3:J3").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("I3:J3").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("I3:J3").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Range("I4:J4").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("I4:J4").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("I4:J4").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("I4:J4").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("I4:J4").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Range("I5:J5").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("I5:J5").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("I5:J5").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("I5:J5").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("I5:J5").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Range("I6:J6").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("I6:J6").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("I6:J6").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("I6:J6").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("I6:J6").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Range("I7:J7").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("I7:J7").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("I7:J7").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("I7:J7").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("I7:J7").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Range("I8:J8").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("I8:J8").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("I8:J8").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("I8:J8").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("I8:J8").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Range("M2:N2").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("M2:N2").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("M2:N2").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("M2:N2").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("M2:N2").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Range("M3:N3").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("M3:N3").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("M3:N3").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("M3:N3").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("M3:N3").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Range("M4:N4").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("M4:N4").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("M4:N4").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("M4:N4").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("M4:N4").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Range("M5:N5").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("M5:N5").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("M5:N5").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("M5:N5").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("M5:N5").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Range("M6:N6").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("M6:N6").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("M6:N6").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("M6:N6").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("M6:N6").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Range("M7:N7").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("M7:N7").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("M7:N7").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("M7:N7").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("M7:N7").Style.Border.RightBorder = XLBorderStyleValues.Medium;
                //worksheet.Range("I9:J9").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                //worksheet.Range("I9:J9").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                //worksheet.Range("I9:J9").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                //worksheet.Range("I9:J9").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                //worksheet.Range("I9:J9").Style.Border.RightBorder = XLBorderStyleValues.Medium;


                //usedRange.Style.Fill.BackgroundColor = XLColor.White;
                //usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                //usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                //usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                //usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                //usedRange.Style.Font.FontSize = 10;
                //usedRange.Style.Font.FontColor = XLColor.Black;




                // Employee Details

                worksheet.Range("C2:D2").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("C2:D2").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("C2:D2").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("C2:D2").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("C2:D2").Style.Border.RightBorder = XLBorderStyleValues.Medium;


                worksheet.Cell(2, 1).Value = "Employee Name";
                worksheet.Cell(2, 3).Value = EmpSummary.EmployeeName;


                worksheet.Range("C3:D3").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("C3:D3").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("C3:D3").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("C3:D3").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("C3:D3").Style.Border.RightBorder = XLBorderStyleValues.Medium;


                worksheet.Cell(3, 1).Value = "DepartmentName";
                worksheet.Cell(3, 3).Value = EmpSummary.DepartmentName;

                worksheet.Range("C4:D4").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("C4:D4").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("C4:D4").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("C4:D4").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("C4:D4").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Cell(4, 1).Value = "PaidHolidays";
                worksheet.Cell(4, 3).Value = EmpSummary.PaidHolidays;


                worksheet.Range("C5:D5").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("C5:D5").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("C5:D5").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("C5:D5").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("C5:D5").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Cell(5, 1).Value = "AbsentDays";
                worksheet.Cell(5, 3).Value = EmpSummary.AbsentDays;


                worksheet.Range("C6:D6").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("C6:D6").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("C6:D6").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("C6:D6").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("C6:D6").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Cell(6, 1).Value = "Overtime";
                worksheet.Cell(6, 3).Value = EmpSummary.Overtime;

                worksheet.Range("C7:D7").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("C7:D7").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("C7:D7").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("C7:D7").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("C7:D7").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Cell(7, 1).Value = "CO";
                worksheet.Cell(7, 3).Value = EmpSummary.CO;


                worksheet.Range("G2:H2").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("G2:H2").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("G2:H2").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("G2:H2").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("G2:H2").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Cell(2, 5).Value = "Employee No";
                worksheet.Cell(2, 7).Style.NumberFormat.Format = "@";
                worksheet.Cell(2, 7).Value = EmpSummary.EmployeeNo;

                worksheet.Range("G3:H3").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("G3:H3").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("G3:H3").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("G3:H3").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("G3:H3").Style.Border.RightBorder = XLBorderStyleValues.Medium;
                
                worksheet.Cell(3, 5).Value = "Earned Leave";
                worksheet.Cell(3, 7).Value = EmpSummary.EarnedLeave;


                worksheet.Range("G4:H4").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("G4:H4").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("G4:H4").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("G4:H4").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("G4:H4").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Cell(4, 5).Value = "Maternity Leave";
                worksheet.Cell(4, 7).Value = EmpSummary.MaternityLeave;

                worksheet.Range("G5:H5").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("G5:H5").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("G5:H5").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("G5:H5").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("G5:H5").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Cell(5, 5).Value = "ESIC Leave";
                worksheet.Cell(5, 7).Value = EmpSummary.ESICLeave;

                worksheet.Range("G6:H6").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("G6:H6").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("G6:H6").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("G6:H6").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("G6:H6").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Cell(6, 5).Value = "LWP Leave";
                worksheet.Cell(6, 7).Value = EmpSummary.LWPLeave;

                worksheet.Range("G7:H7").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("G7:H7").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("G7:H7").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("G7:H7").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("G7:H7").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Cell(7, 5).Value = "Physical Worked Hours";
                worksheet.Cell(7, 7).Value = EmpSummary.PhysicalWorkedHours;

                //worksheet.Range("G8:H8").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                //worksheet.Range("G8:H8").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                //worksheet.Range("G8:H8").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                //worksheet.Range("G8:H8").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                //worksheet.Range("G8:H8").Style.Border.RightBorder = XLBorderStyleValues.Medium;

               

                //worksheet.Range("G9:H9").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                //worksheet.Range("G9:H9").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                //worksheet.Range("G9:H9").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                //worksheet.Range("G9:H9").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                //worksheet.Range("G9:H9").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Cell(8, 5).Value = "Worked on Woff & PH in days Value";
                worksheet.Cell(8, 7).Value = EmpSummary.WorkedonWoff;

                worksheet.Range("K2:L2").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("K2:L2").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("K2:L2").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("K2:L2").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("K2:L2").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Cell(2, 9).Value = "BusinessUnit";
                worksheet.Cell(2, 11).Value = EmpSummary.BusinessUnit;


                worksheet.Range("K3:L3").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("K3:L3").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("K3:L3").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("K3:L3").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("K3:L3").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Cell(3, 9).Value = "Payable Days";
                worksheet.Cell(3, 11).Value = EmpSummary.PayableDays;

                worksheet.Range("K4:L4").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("K4:L4").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("K4:L4").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("K4:L4").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("K4:L4").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Cell(4, 9).Value = "Month Days";
                worksheet.Cell(4, 11).Value = DateTime.DaysInMonth(GetMonth.Year, GetMonth.Month);

                worksheet.Range("K5:L5").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("K5:L5").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("K5:L5").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("K5:L5").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("K5:L5").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Cell(5, 9).Value = "Present Days";
                worksheet.Cell(5, 11).Value = EmpSummary.PresentDays;

                worksheet.Range("K6:L6").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("K6:L6").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("K6:L6").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("K6:L6").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("K6:L6").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Cell(6, 9).Value = "Casual Leave";
                worksheet.Cell(6, 11).Value = EmpSummary.CasualLeave;

                worksheet.Range("K7:L7").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("K7:L7").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("K7:L7").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("K7:L7").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("K7:L7").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Cell(7, 9).Value = "Week Off Days";
                worksheet.Cell(7, 11).Value = EmpSummary.WeekOffDays;

                worksheet.Range("O2:R2").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("O2:R2").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("O2:R2").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("O2:R2").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("O2:R2").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Cell(2, 13).Value = "Designation Name";
                worksheet.Cell(2, 15).Value = EmpSummary.DesginationName;

                worksheet.Range("O3:R3").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("O3:R3").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("O3:R3").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("O3:R3").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("O3:R3").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Cell(3, 13).Value = "LOP Hrs";
                worksheet.Cell(3, 15).Value = EmpSummary.LOPHrs;

                worksheet.Range("O4:R4").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("O4:R4").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("O4:R4").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("O4:R4").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("O4:R4").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Cell(4, 13).Value = "Sick Leave";
                worksheet.Cell(4, 15).Value = EmpSummary.SickLeave;

                worksheet.Range("O5:R5").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("O5:R5").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("O5:R5").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("O5:R5").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("O5:R5").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Range("O6:R6").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("O6:R6").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("O6:R6").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("O6:R6").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("O6:R6").Style.Border.RightBorder = XLBorderStyleValues.Medium;

                worksheet.Range("O7:R7").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("O7:R7").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("O7:R7").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("O7:R7").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("O7:R7").Style.Border.RightBorder = XLBorderStyleValues.Medium;
                // Formatting
                worksheet.Range("A2:A7").Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                worksheet.Range("A2:A7").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("A2:A7").Style.Font.Bold = true;
                worksheet.Range("B2:B7").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

                worksheet.Range("E2:E7").Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                worksheet.Range("E2:E7").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("E2:E7").Style.Font.Bold = true;
                worksheet.Range("F2:F7").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

                worksheet.Range("I2:I7").Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                worksheet.Range("I2:I7").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("I2:I7").Style.Font.Bold = true;
                worksheet.Range("J2:J7").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

                worksheet.Range("M2:M7").Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                worksheet.Range("M2:M7").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Range("M2:M7").Style.Font.Bold = true;
                worksheet.Range("N2:N7").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                // Employee Wise Summary Section



                worksheet.Range("A8:R8").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                worksheet.Range("A8:R8").Style.Border.TopBorder = XLBorderStyleValues.Medium;
                worksheet.Range("A8:R8").Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                worksheet.Range("A8:R8").Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                worksheet.Range("A8:R8").Style.Border.RightBorder = XLBorderStyleValues.Medium;
                worksheet.Cell(8, 1).Value = "Employee Wise Attendnce Summary";
                worksheet.Cell(8, 1).Style.Font.SetBold(true);




                /////////////////////


                //Set the background color to white and apply borders
                int startRow = 9;
                int startColumn = 1;
                int endRow = startRow + dt.Rows.Count;  // Adjust to include all rows starting from the 9th row
                int endColumn = dt.Columns.Count;
                var usedRange = worksheet.Range(startRow, startColumn, endRow, endColumn);
                usedRange.Style.Fill.BackgroundColor = XLColor.White;
                usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Font.FontSize = 10;
                usedRange.Style.Font.FontColor = XLColor.Black;

                //Set the header row name
                worksheet.Cell(1, 1).Value = "Employee Wise Sumarry - (" + GetMonth.ToString("MMM/yyyy") + ")";
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Columns().AdjustToContents(); // This code for all clomns

                var headerRange = worksheet.Range(9, 1, 9, dt.Columns.Count);
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