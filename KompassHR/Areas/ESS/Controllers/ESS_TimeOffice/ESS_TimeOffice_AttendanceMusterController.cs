using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
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

namespace KompassHR.Areas.ESS.Controllers.ESS_TimeOffice
{
    public class ESS_TimeOffice_AttendanceMusterController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_TimeOffice_AttendanceMuster
        #region Main Page 
        public ActionResult ESS_TimeOffice_AttendanceMuster(AttendanceMuster AttendanceMuster)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 893;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetCompanyName = GetComapnyName;
                var CmpId = GetComapnyName[0].Id;

                ViewBag.GetDays = "";
                ViewBag.AttendanceMuster = "";
                ViewBag.MonthYear = "";
                ViewBag.GetBranchName = "";
                ViewBag.EmployeeName = "";

                if (AttendanceMuster.CmpId > 0)
                {

                    int daysInMonth = DateTime.DaysInMonth(AttendanceMuster.Month.Year, AttendanceMuster.Month.Month);
                    ViewBag.GetDays = Enumerable.Range(1, daysInMonth).ToList();
                    ViewBag.MonthYear = AttendanceMuster.Month;
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_CmpId", AttendanceMuster.CmpId);
                    paramList.Add("@p_BranchId", AttendanceMuster.BranchId);
                    paramList.Add("@p_EmployeeId", AttendanceMuster.EmployeeId);
                    paramList.Add("@p_MonthYear", AttendanceMuster.Month);
                    var data = DapperORM.ExecuteSP<dynamic>("sp_Atten_List_AttendanceMuster", paramList).ToList();
                    ViewBag.AttendanceMuster = data;

                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_employeeid", Session["EmployeeId"]);
                    param1.Add("@p_CmpId", AttendanceMuster.CmpId);
                    ViewBag.GetBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param1).ToList();

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeeLeft=0 and  CmpID=" + AttendanceMuster.CmpId + " and EmployeeBranchId=" + AttendanceMuster.BranchId + "  order by Name");
                    ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();

                }
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
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                DynamicParameters paramemp = new DynamicParameters();
                paramemp.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeeLeft=0 and  CmpID=" + CmpId + " order by Name");
                var Employee = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramemp).ToList();

                return Json(new { Branch= Branch, Employee= Employee }, JsonRequestBehavior.AllowGet);
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
        public ActionResult GetEmployeeName(int CmpId, int? BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                List<AllDropDownBind> data = new List<AllDropDownBind>();
                if (CmpId > 0)
                {
                    if (BranchId > 0)
                    {
                        DynamicParameters paramemployee = new DynamicParameters();
                        paramemployee.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeeLeft=0 and CmpID=" + CmpId + " and EmployeeBranchId=" + BranchId + " order by Name");
                        data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramemployee).ToList();
                    }
                    else
                    {
                        DynamicParameters param = new DynamicParameters();
                        param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeeLeft=0 and  CmpID=" + CmpId + " order by Name");
                        data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    }
                }
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

        public ActionResult Save(int EmployeeId, DateTime date, string Status)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 893;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_process", "Save");
                param.Add("@p_EmployeeId", EmployeeId);
                param.Add("@p_Date", date);
                param.Add("@p_Status", Status);
                param.Add("@p_CreatedupdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Atten_AttendanceMuster", param);
                var Message1 = param.Get<string>("@p_msg");
                var Icon1 = param.Get<string>("@p_Icon");
                return Json(new { Message1, Icon1 }, JsonRequestBehavior.AllowGet);
                //return RedirectToAction("ESS_TimeOffice_AttendanceMuster", "ESS_TimeOffice_AttendanceMuster");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, DateTime Month, int? EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("AttendanceMuster");
                worksheet.Range(1, 1, 1, 10).Merge();
                worksheet.SheetView.FreezeRows(2);
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();

                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_CmpId", CmpId);
                paramList.Add("@p_BranchId", BranchId);
                paramList.Add("@p_EmployeeId", EmployeeId);
                paramList.Add("@p_MonthYear", Month);
                var GetData = DapperORM.ExecuteSP<dynamic>("sp_Atten_List_AttendanceMuster", paramList).ToList();
                if (GetData.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }
                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(GetData);

                //for (int i = dt.Columns.Count - 1; i >= 0; i--)
                //{
                //    if (dt.Columns[i].ColumnName.Contains("_isAdjustFor"))
                //    {
                //        dt.Columns.RemoveAt(i);
                //    }
                //}

                dt = dprObj.ConvertToDataTable(GetData);

                // Insert table first
                worksheet.Cell(2, 1).InsertTable(dt, false);

                // Loop through columns to find _isAdjustFor columns
                for (int col = dt.Columns.Count - 1; col >= 0; col--)
                {
                    string columnName = dt.Columns[col].ColumnName;

                    if (columnName.Contains("_isAdjustFor"))
                    {
                        // Get actual date column name
                        string actualDateColumn = columnName.Replace("_isAdjustFor", "");

                        int adjustColIndex = col + 1; // Excel index
                        int dateColIndex = dt.Columns.IndexOf(actualDateColumn) + 1;

                        // Hide adjust column
                        worksheet.Column(adjustColIndex).Hide();

                        if (dateColIndex > 0)
                        {
                            // Loop rows
                            for (int row = 0; row < dt.Rows.Count; row++)
                            {
                                var adjustValue = dt.Rows[row][col];

                                if (adjustValue != DBNull.Value && Convert.ToInt32(adjustValue) == 1)
                                {
                                    // Excel row starts from 2 (header row at 2)
                                    int excelRow = row + 3;

                                    worksheet.Cell(excelRow, dateColIndex)
                                             .Style.Fill.BackgroundColor = XLColor.LightPink;

                                    worksheet.Cell(excelRow, dateColIndex)
                                             .Style.Font.Bold = true;
                                }
                            }
                        }
                    }
                }

                worksheet.Cell(2, 1).InsertTable(dt, false);
                int totalRows = worksheet.RowsUsed().Count();

                var lastRow = worksheet.Row(totalRows + 1);
                lastRow.Style.Font.Bold = true;

                // lastRow.Style.Font.FontColor = XLColor.Black;
                // lastRow.Style.Font.FontSize = 10;

                var usedRange = worksheet.RangeUsed();
                usedRange.Style.Fill.BackgroundColor = XLColor.White;
                usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Font.FontSize = 10;
                usedRange.Style.Font.FontColor = XLColor.Black;

                // Set the header row name
                worksheet.Cell(1, 1).Value = "Attendance Muster - (" + Month.ToString("MMM/yyyy") + ")";
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Columns().AdjustToContents();

                var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                headerRange.Style.Font.FontSize = 10;
                headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
                headerRange.Style.Font.Bold = true;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;

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