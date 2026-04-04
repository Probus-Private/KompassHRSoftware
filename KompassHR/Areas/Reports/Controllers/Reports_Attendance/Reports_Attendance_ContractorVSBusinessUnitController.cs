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

namespace KompassHR.Areas.Reports.Controllers.Reports_Attendance
{
    public class Reports_Attendance_ContractorVSBusinessUnitController : Controller
    {

        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: /Reports/Reports_Attendance_ContractorVSBusinessUnit/Reports_Attendance_ContractorVSBusinessUnit
        public ActionResult Reports_Attendance_ContractorVSBusinessUnit(HeadCountSummary HeadCount)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 397;
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
                ViewBag.BusinessUnit = "";

                DynamicParameters paramGrade = new DynamicParameters();
                paramGrade.Add("@query", "select GradeId as Id,GradeName as Name from Mas_Grade where Deactivate=0 order by Name");
                ViewBag.GradeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramGrade).ToList();

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        //#region GetBusinessUnit
        //[HttpGet]
        //public ActionResult GetBusinessUnit(int CmpId)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { area = "" });
        //        }

        //        DynamicParameters param = new DynamicParameters();
        //        param.Add("@p_employeeid", Session["EmployeeId"]);
        //        param.Add("@p_CmpId", CmpId);
        //        var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
        //        return Json(data, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }

        //}
        //#endregion

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? CmpId,  DateTime FromTime, string[] GradeId, DateTime ToTime, DateTime txtDate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("ContractorVSBusinessUnit");
                worksheet.Range(1, 1, 1, 10).Merge();
                worksheet.SheetView.FreezeRows(2); // Freeze the row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();

                var Query = "";
                var GetDate = txtDate.ToString("yyyy-MM-dd");
                var FromTime1 = FromTime.ToString("HH:mm");
                var ToTime1 = ToTime.ToString("HH:mm");
                //if (CmpId != null)
                //{
                //    param = new DynamicParameters();
                //    if (BranchId != null)
                //    {
                //        Query = "" + BranchId + " and ";
                //    }
                //}
                var AllGradId = GradeId[0];
                Query = " and Mas_Employee.EmployeeGradeId In (" + AllGradId + ") ";
                param.Add("@p_qry", Query);
                param.Add("@p_Employeeid", Session["EmployeeId"]);
                param.Add("@p_Branchid", "");
                param.Add("@p_CmpId", CmpId);
                param.Add("@p_Date", GetDate);
                param.Add("@p_FromTime", FromTime1);
                param.Add("@p_ToTime", ToTime1);
                //if (BranchId == null)
                //{
                //    param.Add("@p_Branchid", "");
                //}
                //else
                //{
                //    param.Add("@p_Branchid", BranchId);
                //}

                var ContractorVSBusinessUnit = DapperORM.ReturnList<dynamic>("sp_Rpt_Atten_ContractorVSBusinessUnit", param).ToList();

                if (ContractorVSBusinessUnit.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }
                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(ContractorVSBusinessUnit);
                worksheet.Cell(2, 1).InsertTable(dt, false);
                int totalRows = worksheet.RowsUsed().Count();



                // Insert a new column at the end (or choose the column index where you want the new column)
                int newColumnIndex = dt.Columns.Count + 1;
                worksheet.Column(newColumnIndex).InsertColumnsAfter(1);
                worksheet.Cell(2, newColumnIndex).Value = "Total";
                // Sum values from specific columns (e.g., columns 2 to 4)
                for (int row = 3; row <= totalRows + 1; row++)
                {
                    double sum = 0;
                    // Loop through specific columns to sum their values (e.g., columns 2 to 4)
                    for (int col = 3; col <= 6; col++) // Specify the columns you want to sum (change 2 to 4 to your target columns)
                    {
                        var cell = worksheet.Cell(row, col);
                        double value;

                        if (double.TryParse(cell.GetValue<string>(), out value))
                        {
                            sum += value;
                        }
                    }
                    // Insert the sum in the newly added column
                    var sumCell = worksheet.Cell(row, newColumnIndex);
                    sumCell.Value = sum;
                }

                // Convert the column values to numbers and calculate the sum
                for (int col = dt.Columns.Count; col >= 2; col--) // Loop backwards to avoid index issues when deleting columns
                {
                    double sum = 0;
                    for (int row = 2; row <= totalRows + 1; row++)
                    {
                        var cell = worksheet.Cell(row, col);
                        double value;
                        if (double.TryParse(cell.GetValue<string>(), out value))
                        {
                            cell.Value = value;
                            sum += value;
                        }
                    }

                    // If the sum is 0, delete the entire column
                    if (sum == 0)
                    {
                        worksheet.Column(col).Delete();
                    }
                    else
                    {
                        // Insert the total value at the end of the column if the sum is non-zero
                        var totalCell = worksheet.Cell(totalRows + 2, col);
                        totalCell.Value = sum;
                        totalCell.Style.Font.Bold = true;
                        totalCell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        totalCell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        totalCell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        totalCell.Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    }
                }
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
                worksheet.Cell(1, 1).Value = "Contractor VS Business Unit - (" + txtDate.ToString("dd/MMM/yyyy") + ")";
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Columns().AdjustToContents(); // This code for all columns

                var usedColumnsCount = worksheet.ColumnsUsed().Count();

                // Apply header styling only to the present columns
                var headerRange = worksheet.Range(2, 1, 2, usedColumnsCount); // Apply styles to only the columns that are still in use
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                headerRange.Style.Font.FontSize = 10;
                headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
                headerRange.Style.Font.Bold = true;

                // Merge the first two columns in the total row and add "Total" label
                //var totalLabelRange = worksheet.Range(totalRows + 2, 1, totalRows + 2, 2);
                var totalLabelCell = worksheet.Cell(totalRows + 2, 1); // Use a single cell instead of a range
                totalLabelCell.Value = "Total";
                totalLabelCell.Style.Font.Bold = true;
                totalLabelCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                totalLabelCell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                totalLabelCell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                totalLabelCell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                totalLabelCell.Style.Border.RightBorder = XLBorderStyleValues.Thin;

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