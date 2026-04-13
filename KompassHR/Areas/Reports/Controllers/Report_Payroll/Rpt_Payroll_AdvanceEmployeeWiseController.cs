using ClosedXML.Excel;
using Dapper;
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

namespace KompassHR.Areas.Reports.Controllers.Report_Payroll
{
    public class Rpt_Payroll_AdvanceEmployeeWiseController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: Reports/Rpt_Payroll_AdvanceEmployeeWise
        #region Rpt_Payroll_AdvanceEmployeeWise
        public ActionResult Rpt_Payroll_AdvanceEmployeeWise()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 624;
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
                ViewBag.BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "Select distinct Mas_Employee.EmployeeId as Id,concat(Mas_Employee.EmployeeName,' - ',Mas_Employee.EmployeeNo) as Name from Mas_Employee left join Payroll_advance on  Payroll_advance.AdvanceEmployeeID=Mas_Employee.EmployeeId where  Mas_Employee.Deactivate=0 and Payroll_Advance.Deactivate=0 and Payroll_Advance.BranchId in (Select BranchID from UserBranchMapping where UserBranchMapping.CmpID="+ CmpId + " and UserBranchMapping.IsActive=1 and UserBranchMapping.EmployeeID="+Session["EmployeeId"]+") order by Name");
                ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");

            }
        }
        #endregion


        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId,int? EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Advance employee wise");
                worksheet.SheetView.FreezeRows(1); // Freeze top header row

                DynamicParameters paramList = new DynamicParameters();
                string Query = "";

                if (BranchId != null)
                {
                    Query += " and Payroll_Advance.BranchId=" + BranchId;
                }
                else
                {
                    Query += " and Payroll_Advance.BranchId in (select BranchID as Id from UserBranchMapping where UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and UserBranchMapping.CmpID = " + CmpId + " and UserBranchMapping.IsActive = 1)";
                }
                if (EmployeeId != null)
                {
                    Query += " and Payroll_Advance.AdvanceEmployeeID=" + EmployeeId;
                }

                paramList.Add("@P_Qry", Query);
                var reportData = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Payroll_AdvanceEmployeeWise", paramList).ToList();

                if (reportData.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }

                // Convert to DataTable for reliable structure
                DapperORM dprObj = new DapperORM();
                DataTable dtAll = dprObj.ConvertToDataTable(reportData);

                // Group DataTable rows by EmployeeNo
                var groupedData = dtAll.AsEnumerable()
                    .Where(row => !string.IsNullOrWhiteSpace(row.Field<string>("EmployeeNo")))
                    .GroupBy(row => row.Field<string>("EmployeeNo"))
                    .ToList();

                int currentRow = 1;

                // Report Title
                worksheet.Cell(currentRow, 1).Value = "Advance Employee Wise Report - (" + DateTime.Now.ToString("dd/MMM/yyyy") + ")";
                worksheet.Range(currentRow, 1, currentRow, dtAll.Columns.Count).Merge().Style
                    .Font.SetBold()
                    .Font.SetFontSize(12)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                currentRow++;

                foreach (var employeeGroup in groupedData)
                {
                    // Header Row
                    for (int i = 0; i < dtAll.Columns.Count; i++)
                    {
                        worksheet.Cell(currentRow, i + 1).Value = dtAll.Columns[i].ColumnName;
                        worksheet.Cell(currentRow, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                        worksheet.Cell(currentRow, i + 1).Style.Font.FontSize = 10;
                        worksheet.Cell(currentRow, i + 1).Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
                        worksheet.Cell(currentRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(currentRow, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    currentRow++;

                    // Data Rows with EmployeeNo only once
                    bool isFirstRow = true;
                    foreach (var row in employeeGroup)
                    {
                        for (int i = 0; i < dtAll.Columns.Count; i++)
                        {
                            string columnName = dtAll.Columns[i].ColumnName;
                            object value = row[columnName];

                            // Show EmployeeNo only for the first row in group
                            if (columnName == "EmployeeNo" && !isFirstRow)
                            {
                                value = "";
                            }

                            worksheet.Cell(currentRow, i + 1).Value = value;
                            worksheet.Cell(currentRow, i + 1).Style.Fill.BackgroundColor = XLColor.White;
                            worksheet.Cell(currentRow, i + 1).Style.Font.FontSize = 10;
                            worksheet.Cell(currentRow, i + 1).Style.Font.FontColor = XLColor.Black;
                            worksheet.Cell(currentRow, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }
                        isFirstRow = false;
                        currentRow++;
                    }

                    currentRow++; // Optional space between groups
                }

                worksheet.Columns().AdjustToContents();

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
                if (BranchId != null)
                {
                    DynamicParameters paramEmpName = new DynamicParameters();
                    paramEmpName.Add("@query", "Select distinct Mas_Employee.EmployeeId as Id,concat(Mas_Employee.EmployeeName,' - ',Mas_Employee.EmployeeNo) as Name from Mas_Employee left join Payroll_advance on  Payroll_advance.AdvanceEmployeeID=Mas_Employee.EmployeeId where  Mas_Employee.Deactivate=0 and Payroll_Advance.Deactivate=0 and Payroll_Advance.BranchId="+ BranchId + " order by Name");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    DynamicParameters paramEmpName = new DynamicParameters();
                    paramEmpName.Add("@query", "Select distinct Mas_Employee.EmployeeId as Id,concat(Mas_Employee.EmployeeName,' - ',Mas_Employee.EmployeeNo) as Name from Mas_Employee left join Payroll_advance on  Payroll_advance.AdvanceEmployeeID=Mas_Employee.EmployeeId where  Mas_Employee.Deactivate=0 and Payroll_Advance.Deactivate=0 and Payroll_Advance.BranchId in (Select BranchID from UserBranchMapping where UserBranchMapping.CmpID=" + CmpId + " and UserBranchMapping.IsActive=1 and UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + ") order by Name");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
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
    }
}