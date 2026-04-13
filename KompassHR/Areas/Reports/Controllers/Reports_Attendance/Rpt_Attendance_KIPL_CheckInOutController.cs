using ClosedXML.Excel;
using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_Attendance
{
    public class Rpt_Attendance_KIPL_CheckInOutController : Controller
    {
        #region Main View
        // GET: Reports/Rpt_Attendance_KIPL_CheckInOut
        public ActionResult Rpt_Attendance_KIPL_CheckInOut()
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 937;
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

                var CmpID = GetComapnyName[0].Id;
                ViewBag.BranchName = "";
                ViewBag.EmployeeName = "";

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

        #region GetEmployeeName
        [HttpGet]
        public ActionResult GetEmployeeName(int CmpId, int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters paramEmployeeName = new DynamicParameters();
                //paramContractorName.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + BranchId + "");
                //var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", paramContractorName).ToList();
                paramEmployeeName.Add("@Query", "select EmployeeId as Id , EmployeeNo + ' - ' + EmployeeName AS Name from Mas_Employee Where Deactivate=0 and EmployeeLeft=0 And CmpID='" + CmpId + "' and EmployeeBranchId='" + BranchId + "'");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmployeeName);
                return Json(data, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion



        #region Download
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, int? EmployeeId, DateTime FromDate, DateTime ToDate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var workbook = new XLWorkbook();
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EmployeeId", EmployeeId);
                paramList.Add("@p_FromDate", FromDate);
                paramList.Add("@p_ToDate", ToDate);
                paramList.Add("@p_CompanyId", CmpId);
                paramList.Add("@p_BranchId", BranchId);

                var GetData = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Atten_CheckInOut_Excel_KIPL", paramList).ToList();

                if (GetData.Count == 0)
                {
                    return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                }

                // ------------------------------------------------------
                // FIXED GROUPING: Ensure EmployeeId is an INT Always
                // ------------------------------------------------------
                var grouped = GetData.GroupBy(x =>
                {
                    var dict = (IDictionary<string, object>)x;

                    if (dict.ContainsKey("EmployeeId") && dict["EmployeeId"] != null)
                        return Convert.ToInt32(dict["EmployeeId"].ToString().Trim());

                    return 0;
                });

                // ------------------------------------------------------
                // FILTER FOR SPECIFIC EMPLOYEE IF EmployeeId != 0
                // ------------------------------------------------------
                if (EmployeeId != null && EmployeeId.Value != 0)
                {
                    grouped = grouped.Where(g => Convert.ToInt32(g.Key) == EmployeeId.Value);
                }

                // ------------------------------------------------------
                // CREATE SHEET FOR EACH EMPLOYEE
                // ------------------------------------------------------
                foreach (var empGroup in grouped)
                {
                    var firstRow = (IDictionary<string, object>)empGroup.First();

                    string empName = firstRow.ContainsKey("EmployeeName")
                                       ? firstRow["EmployeeName"]?.ToString()
                                       : "Employee_" + empGroup.Key;

                    if (string.IsNullOrWhiteSpace(empName))
                        empName = "Employee_" + empGroup.Key;

                    // Clean invalid filename chars
                    foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                        empName = empName.Replace(c.ToString(), "");

                    if (empName.Length > 25)
                        empName = empName.Substring(0, 25);

                    var worksheet = workbook.Worksheets.Add(empName);

                    worksheet.Range(1, 1, 1, 20).Merge();
                    worksheet.SheetView.FreezeRows(2);

                    DapperORM dprObj = new DapperORM();
                    DataTable dt = dprObj.ConvertToDataTable(empGroup.ToList());

                    worksheet.Cell(2, 1).InsertTable(dt, false);

                    // HIDE EmployeeId column
                    // ------------------------------------------------------
                    int empIdColIndex = -1;

                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        if (dt.Columns[i].ColumnName.Equals("EmployeeId", StringComparison.OrdinalIgnoreCase))
                        {
                            empIdColIndex = i + 1; 
                            break;
                        }
                    }

                    if (empIdColIndex != -1)
                    {
                        worksheet.Column(empIdColIndex).Hide();
                    }



                    var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
                    headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                    headerRange.Style.Font.Bold = true;

                    worksheet.Cell(1, 1).Value = "KIPL Check In Out Report";
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(1, 1).Style.Font.Bold = true;

                    var usedRange = worksheet.RangeUsed();
                    usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    worksheet.Columns().AdjustToContents();
                }

                // ------------------------------------------------------
                // RETURN FINAL EXCEL FILE
                // ------------------------------------------------------
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "KIPLCheckInOut_MultiSheet.xlsx"
                    );
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        //#region DownloadExcelFile
        //public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, int? EmployeeId, DateTime FromDate,DateTime ToDate)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { Area = "" });
        //        }
        //        var workbook = new XLWorkbook();
        //        var worksheet = workbook.Worksheets.Add("KIPLCheckInOutReport");

        //        worksheet.Range(1, 1, 1, 10).Merge();
        //        worksheet.SheetView.FreezeRows(2);
        //        DataTable dt = new DataTable();
        //        List<dynamic> data = new List<dynamic>();

        //        DynamicParameters paramList = new DynamicParameters();
        //        paramList.Add("@p_EmployeeId", EmployeeId);
        //        paramList.Add("@p_FromDate", FromDate);
        //        paramList.Add("@p_ToDate", ToDate);
        //        paramList.Add("@p_CompanyId", CmpId);
        //        paramList.Add("@p_BranchId", BranchId);
        //        var GetData = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Atten_CheckInOut_Excel_KIPL", paramList).ToList();
        //        if (GetData.Count == 0)
        //        {
        //            byte[] emptyFileContents = new byte[0];
        //            return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
        //        }
        //        DapperORM dprObj = new DapperORM();
        //        dt = dprObj.ConvertToDataTable(GetData);
        //        worksheet.Cell(2, 1).InsertTable(dt, false);
        //        int totalRows = worksheet.RowsUsed().Count();

        //        var lastRow = worksheet.Row(totalRows + 1);
        //        lastRow.Style.Font.Bold = false;

        //        lastRow.Style.Font.FontColor = XLColor.Black;
        //        lastRow.Style.Font.FontSize = 10;
        //        var usedRange = worksheet.RangeUsed();
        //        usedRange.Style.Fill.BackgroundColor = XLColor.White;
        //        usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        //        usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        //        usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        //        usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
        //        usedRange.Style.Font.FontSize = 10;
        //        usedRange.Style.Font.FontColor = XLColor.Black;
        //        worksheet.Cell(1, 1).Value = "KIPL Check In Out Report";

        //        //worksheet.Cell(1, 1).Value = "Late Mark AdjustmentReport Report - (" + Month.ToString("dd/MMM/yyyy") + ")";
        //        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        //        worksheet.Cell(1, 1).Style.Font.Bold = true;
        //        worksheet.Columns().AdjustToContents();
        //        var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
        //        headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
        //        headerRange.Style.Font.FontSize = 10;
        //        headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
        //        headerRange.Style.Font.Bold = true;

        //        using (var stream = new MemoryStream())
        //        {
        //            workbook.SaveAs(stream);
        //            stream.Position = 0;

        //            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report.xlsx");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
        //}
        //#endregion

    }
}