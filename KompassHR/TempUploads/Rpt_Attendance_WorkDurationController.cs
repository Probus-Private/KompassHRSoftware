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

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.CompanyName = GetComapnyName;

                var results = sqlcon.QueryMultiple(@"select DepartmentId as Id, DepartmentName as Name from Mas_Department where Deactivate=0 order by DepartmentName; 
                                                  select DesignationId as Id,DesignationName as Name from Mas_Designation where Deactivate=0 order by DesignationName
                                                  select GradeId as Id,GradeName as Name from Mas_Grade where Deactivate=0 order by Name");

                ViewBag.DepartmentName = results.Read<AllDropDownClass>().ToList();
                ViewBag.DesignationName = results.Read<AllDropDownClass>().ToList();
                ViewBag.GradeName = results.Read<AllDropDownClass>().ToList();

                var Query = "";
                if (MasReport.CmpId != null)
                {
                    if (MasReport.BranchId != null)
                    {
                        Query = "and Mas_Branch.BranchId=" + MasReport.BranchId + "";
                        DynamicParameters paramLine = new DynamicParameters();
                        paramLine.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + MasReport.CmpId + " and Mas_LineMaster.BranchId=" + MasReport.BranchId + " ");
                        var List_LineMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();
                        ViewBag.LineName = List_LineMaster;

                        DynamicParameters paramEMP = new DynamicParameters();
                        paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + MasReport.CmpId + " and EmployeeBranchId= " + MasReport.BranchId + "and Mas_Employee.EmployeeLeft=0");
                        var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                        ViewBag.EmployeeName = GetEmployeeName;
                    }
                    else
                    {
                        Query = "and mas_branch.BranchId in (select mas_branch.BranchID as Id from UserBranchMapping,mas_branch where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + MasReport.CmpId + " and UserBranchMapping.IsActive = 1)";
                        ViewBag.LineName = "";
                        ViewBag.EmployeeName = "";
                    }

                    if (MasReport.DepartmentId != null)
                    {
                        Query = Query + " and Mas_Department.DepartmentId=" + MasReport.DepartmentId + "";

                        DynamicParameters paramSub = new DynamicParameters();
                        paramSub.Add("@query", "Select SubDepartmentId As Id,SubDepartmentName as Name from Mas_SubDepartment where Mas_SubDepartment.Deactivate=0 and Mas_SubDepartment.DepartmentId=" + MasReport.DepartmentId + "");
                        var DataDept = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramSub).ToList();
                        ViewBag.SubDepartmentName = DataDept;
                    }
                    else
                    {
                        ViewBag.SubDepartmentName = "";
                    }

                    if (MasReport.SubDepartmentId != null)
                    {
                        Query = Query + " and Mas_SubDepartment.SubDepartmentId=" + MasReport.SubDepartmentId + "";
                    }

                    if (MasReport.DesignationId != null)
                    {
                        Query = Query + " and Mas_Designation.DesignationId=" + MasReport.DesignationId + "";
                    }

                    if (MasReport.GradeId != null)
                    {
                        Query = Query + " and Mas_Grade.GradeId=" + MasReport.GradeId + "";
                    }

                    if (MasReport.LineId != null)
                    {
                        Query = Query + " and Mas_LineMaster.LineId=" + MasReport.LineId + "";
                    }
                    if (MasReport.EmployeeID != null)
                    {
                        Query = Query + " and Mas_Employee.EmployeeID=" + MasReport.EmployeeID + "";
                    }

                    var GetFromDate = MasReport.FromDate.ToString("yyyy-MM-dd");
                    var GetToDate = MasReport.ToDate.ToString("yyyy-MM-dd");
                    if (MasReport.FromDate != null)
                    {
                        Query = Query + "and InOutDate between "+ GetFromDate + " and "+ GetToDate + "";
                        //Query = Query + " and convert (date,Atten_InOut.InOutDate) >=convert(date,'" + GetFromDate + "') and  convert(date,Atten_InOut.InOutDate) <=convert(date,'" + GetToDate + "')";
                    }

                    //THIS IS MAIN ALL FILTER SP
                    //DynamicParameters paramList = new DynamicParameters();
                    //paramList.Add("@p_Qry", Query);
                    //var GetDailyAttendanceReport = DapperORM.ReturnList<dynamic>("SP_AttendanceRegularization_Report_FromDateToDate", paramList).ToList();
                    //ViewBag.DailyAttendanceReport = GetDailyAttendanceReport;
                    //if (GetDailyAttendanceReport.Count == 0 || GetDailyAttendanceReport == null)
                    //{
                    //    TempData["Message"] = "Records Not Found !";
                    //    TempData["Icon"] = "error";
                    //}

                    var workbook = new XLWorkbook();
                    var worksheet = workbook.Worksheets.Add("Sheet1");

                    DataTable dt = new DataTable();
                    List<dynamic> data = new List<dynamic>();
                    param.Add("@Fromdate", GetFromDate);
                    param.Add("@Todate", GetToDate);
                    param.Add("@p_Branhid", Session["BranchId"]);
                   // param.Add("@p_Qry", Query);
                    data = DapperORM.ExecuteSP<dynamic>("Rpt_Monthlyworkduration", param).ToList();
                    dt = ConvertToDataTable(data);
                    dt.Rows.RemoveAt(0);
                    worksheet.Cell(1, 1).InsertTable(dt);

                    int totalRows = worksheet.RowsUsed().Count();

                    var colors = new[] { XLColor.LightGray, XLColor.LightGray };
                    int colorIndex = 0;

                    int Loop = totalRows;
                    Loop = Loop / 12;
                    int ao = 2;
                    int ab = 0;

                    for (int m = 0; m < Loop; m++)
                    {
                        ab = ao + 10;
                        worksheet.Range(ao, 1, ab, 1).Merge();
                        worksheet.Range(ao, 2, ab, 2).Merge();

                        var groupRange = worksheet.Range(ao - 1, 1, ao - 1, worksheet.LastColumnUsed().ColumnNumber());
                        groupRange.Style.Fill.BackgroundColor = colors[colorIndex % colors.Length];
                        colorIndex++;
                        ao = ao + 12;
                    }

                    var column = worksheet.Column(2);
                    column.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    column.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    column.AdjustToContents();

                    var usedRange = worksheet.RangeUsed();
                    usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Font.FontName = "Calibri"; // Change the font name
                    usedRange.Style.Font.FontSize = 10;


                    // Save the workbook
                    // workbook.SaveAs("D:\\TestMyExcel.xlsx");
                    //using (XLWorkbook wb = new XLWorkbook())
                    //{
                    //    using (MemoryStream stream = new MemoryStream())
                    //    {
                    //        wb.SaveAs(stream);
                    //        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MyNewGrid.xlsx");
                    //    }
                    //}

                   
                    //TempData["Title"] = "D:\\TestMyExcel.xlsx";
                    TempData["Message"] = "File Download Successfull";
                    TempData["Icon"] = "success";
                    return View();
                    return RedirectToAction("GetList", "ESS_ManpowerAllocation_ManpowerRates", new { Area = "ESS" });

                    //DynamicParameters paramBranchList = new DynamicParameters();
                    //paramBranchList.Add("@p_employeeid", Session["EmployeeId"]);
                    //paramBranchList.Add("@p_CmpId", MasReport.CmpId);
                    //var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranchList).ToList();
                    //ViewBag.BranchName = data;
                    //TempData["ShowData"] = true;
                }
                else
                {
                    ViewBag.DailyAttendanceReport = "";
                    ViewBag.BranchName = "";
                    ViewBag.SubDepartmentName = "";
                    ViewBag.LineName = "";
                    ViewBag.EmployeeName = "";
                }
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

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpId + " and Mas_Employee.EmployeeLeft=0");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                //ViewBag.EmployeeName = EmployeeName;

                DynamicParameters paramLine = new DynamicParameters();
                paramLine.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + CmpId + " ");
                var List_LineMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();
                //ViewBag.LineName = GetList_LineMaster;

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                return Json(new { EmployeeName = EmployeeName, List_LineMaster = List_LineMaster, Branch = Branch }, JsonRequestBehavior.AllowGet);
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

        //#region Export To Excel
        //public ActionResult DynamicExportToExcel()
        //{
        //    // Create a new workbook and worksheet
        //    var workbook = new XLWorkbook();
        //    var worksheet = workbook.Worksheets.Add("Sheet1");

        //    // Sample data for demonstration
        //    //for (int i = 1; i <= 500; i++)
        //    //{
        //    //    worksheet.Cell(i, 1).Value = $"Row {i}";
        //    //    worksheet.Cell(i, 2).Value = $"Row {i}";
        //    //    worksheet.Cell(i, 3).Value = $"Data {i}";
        //    //}

        //    // Merge every 7 rows in the first column
        //    DataTable dt = new DataTable();
        //    List<dynamic> data = new List<dynamic>();
        //    param.Add("@Fromdate", "2024-04-01");
        //    param.Add("@Todate", "2024-04-30");
        //    param.Add("@p_Branhid", Session["BranchId"]);
        //    data = DapperORM.ExecuteSP<dynamic>("Rpt_Monthlyworkduration", param).ToList();
        //    dt = ConvertToDataTable(data);
        //    dt.Rows.RemoveAt(0);
        //    worksheet.Cell(1, 1).InsertTable(dt);

        //    int totalRows = worksheet.RowsUsed().Count();

        //    var colors = new[] { XLColor.LightGray, XLColor.LightGray };
        //    int colorIndex = 0;

        //    int Loop = totalRows;
        //    Loop = Loop / 12;
        //    int ao = 2;
        //    int ab = 0;

        //    for (int m = 0; m < Loop; m++)
        //    {
        //        ab = ao + 10;
        //        worksheet.Range(ao, 1, ab, 1).Merge();
        //        worksheet.Range(ao, 2, ab, 2).Merge();

        //        var groupRange = worksheet.Range(ao - 1, 1, ao - 1, worksheet.LastColumnUsed().ColumnNumber());
        //        groupRange.Style.Fill.BackgroundColor = colors[colorIndex % colors.Length];
        //        colorIndex++;
        //        ao = ao + 12;
        //    }

        //    //var firstRowValuesForPaste = worksheet.FirstRowUsed();
        //    //var firstRowRange = worksheet.Range(firstRowValuesForPaste.FirstCellUsed(), firstRowValuesForPaste.LastCellUsed());
        //    //int o = 0;
        //    //var groups = Enumerable.Range(2, totalRows - 1)
        //    //                    .Select((value, index) => new { value, index })
        //    //                    .GroupBy(x => x.index / 12)
        //    //                    .Select(g => g.Select(x => x.value));


        //    //foreach (var group in groups)
        //    //{
        //    //    var firstRow = group.First();
        //    //    var lastRow = group.Last();
        //    //    worksheet.Range(firstRow, 1, lastRow, 1).Merge();
        //    //    worksheet.Range(firstRow, 2, lastRow, 2).Merge();
        //    //}
        //    var column = worksheet.Column(2);
        //    column.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    column.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        //    column.AdjustToContents();

        //    var usedRange = worksheet.RangeUsed();
        //    usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        //    usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        //    usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        //    usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
        //    usedRange.Style.Font.FontName = "Calibri"; // Change the font name
        //    usedRange.Style.Font.FontSize = 10;

        //    // Save the workbook
        //    workbook.SaveAs("D:\\TestMyExcel.xlsx");
        //    TempData["Title"] = "D:\\TestMyExcel.xlsx";
        //    TempData["Message"] = "File Download Successfull";
        //    TempData["Icon"] = "success";
        //    return RedirectToAction("GetList", "ESS_ManpowerAllocation_ManpowerRates", new { Area = "ESS" });
        //}
        //#endregion

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile()
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sheet1");

            DataTable dt = new DataTable();
            List<dynamic> data = new List<dynamic>();
            param.Add("@Fromdate", GetFromDate);
            param.Add("@Todate", GetToDate);
            param.Add("@p_Branhid", Session["BranchId"]);
            // param.Add("@p_Qry", Query);
            data = DapperORM.ExecuteSP<dynamic>("Rpt_Monthlyworkduration", param).ToList();
            dt = ConvertToDataTable(data);
            dt.Rows.RemoveAt(0);
            worksheet.Cell(1, 1).InsertTable(dt);

            int totalRows = worksheet.RowsUsed().Count();

            var colors = new[] { XLColor.LightGray, XLColor.LightGray };
            int colorIndex = 0;

            int Loop = totalRows;
            Loop = Loop / 12;
            int ao = 2;
            int ab = 0;

            for (int m = 0; m < Loop; m++)
            {
                ab = ao + 10;
                worksheet.Range(ao, 1, ab, 1).Merge();
                worksheet.Range(ao, 2, ab, 2).Merge();

                var groupRange = worksheet.Range(ao - 1, 1, ao - 1, worksheet.LastColumnUsed().ColumnNumber());
                groupRange.Style.Fill.BackgroundColor = colors[colorIndex % colors.Length];
                colorIndex++;
                ao = ao + 12;
            }

            var column = worksheet.Column(2);
            column.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            column.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            column.AdjustToContents();

            var usedRange = worksheet.RangeUsed();
            usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Font.FontName = "Calibri"; // Change the font name
            usedRange.Style.Font.FontSize = 10;


            // Save the workbook
            string SavefilePath = Server.MapPath("~/assets/BulkUpload/WorkDuration.xlsx");
            workbook.SaveAs(SavefilePath);
            
            // Path to the file you want to download
            string filePath = Server.MapPath("~/assets/BulkUpload/WorkDuration.xlsx");
            // Check if the file exists
            if (!System.IO.File.Exists(filePath))
            {
                return HttpNotFound();
            }
            // Get the file content
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            // Set the content type
            string contentType = MimeMapping.GetMimeMapping(filePath);
            // Return the file as a byte array
            return File(fileBytes, contentType, "WorkDuration.xlsx");
        }
        #endregion

        private static DataTable ConvertToDataTable(IEnumerable<dynamic> data)
        {
            var dataTable = new DataTable();
            var firstRecord = data.FirstOrDefault() as IDictionary<string, object>;

            if (firstRecord != null)
            {
                foreach (var kvp in firstRecord)
                {
                    dataTable.Columns.Add(kvp.Key, kvp.Value == null ? typeof(object) : kvp.Value.GetType());
                }

                foreach (var record in data)
                {
                    var dataRow = dataTable.NewRow();
                    foreach (var kvp in record)
                    {
                        dataRow[kvp.Key] = kvp.Value;
                    }
                    dataTable.Rows.Add(dataRow);
                }
            }

            return dataTable;
        }
    }
}