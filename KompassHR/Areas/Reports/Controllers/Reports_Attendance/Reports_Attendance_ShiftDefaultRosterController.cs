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
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_Attendance
{
    public class Reports_Attendance_ShiftDefaultRosterController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Reports/Reports_Attendance_ShiftDefaultRoster
        #region ShiftDefaultRoster
        public ActionResult Reports_Attendance_ShiftDefaultRoster(DailyAttendanceReportFilter MasReport)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 316;
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
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, DateTime FromDate,int? ContractorId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters Branch = new DynamicParameters();
                Branch.Add("@p_employeeid", Session["EmployeeId"]);
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
                    data = DapperORM.ExecuteSP<dynamic>("SP_Rpt_Atten_ShiftDefaultRoster", param).ToList();

                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(data);

                    // Create worksheet regardless of data presence
                    var worksheet = workbook.Worksheets.Add(branchName);
                    worksheet.Range(1, 1, 1, 60).Merge();
                    worksheet.SheetView.FreezeRows(2); // Freeze the row

                    // Set the header row
                    worksheet.Cell(1, 1).Value = "Shift Default Roster Report  - (" + FromDate.ToString("MMM/yyyy") + ") - " + branchName;
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
                  
                    worksheet.Cell(2, 1).InsertTable(dt, false);
                    int totalRows = worksheet.RowsUsed().Count();
                    var usedRange = worksheet.RangeUsed();
                    usedRange.Style.Fill.BackgroundColor = XLColor.White;
                    usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Font.FontSize = 10;
                    usedRange.Style.Font.FontColor = XLColor.Black;

                    var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
                    headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                    headerRange.Style.Font.FontSize = 10;
                    headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
                    headerRange.Style.Font.Bold = true;
                    //var range = worksheet.Range("V2:AZ" + worksheet.LastRowUsed().RowNumber());
                    var blankCells = usedRange.Cells().Where(cell => string.IsNullOrWhiteSpace(cell.GetString()));

                    worksheet.Columns().AdjustToContents(); // This code for all clomns
                                                            //worksheet.Column(3).AdjustToContents(); // This code for perticular columns

                    // Replace each blank cell with a dash
                    foreach (var cell in blankCells)
                    {
                        cell.Value = "-";
                    }
                }
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