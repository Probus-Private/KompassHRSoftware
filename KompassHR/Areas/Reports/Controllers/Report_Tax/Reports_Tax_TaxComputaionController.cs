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
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Report_Tax
{
    public class Reports_Tax_TaxComputaionController : Controller
    {
        GetMenuList ClsGetMenuList = new GetMenuList();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        // GET: Reports/Reports_Tax_TaxComputaion

        #region Reports_Tax_TaxComputaion
        public ActionResult Reports_Tax_TaxComputaion(TaxFilters Report)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 572;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select CompanyId AS Id,CompanyName As Name from Mas_CompanyProfile where Deactivate=0 order by CompanyName");
                var GetComapnyProfile = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.ComapnyProfile = GetComapnyProfile;


                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "Select TaxFyearId as Id,TaxYear As Name  from IncomeTax_Fyear where Deactivate=0 and [IsActive]=1");
                var GetTaxFyear = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param1).ToList();
                ViewBag.GetTaxFyear = GetTaxFyear;
                //if (Report.CmpId != null)
                //{
                //    if (Report.BranchId != null)
                //    {
                //        DynamicParameters paramList = new DynamicParameters();
                //        paramList.Add("@p_BranchId", Report.BranchId);
                //        paramList.Add("@p_Year", Report.FinancialYear);
                //        ViewBag.TaxComputationList = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Recruitment_ManpowerAttrition", paramList).ToList();
                //    }
                //    else
                //    {
                //        DynamicParameters paramList = new DynamicParameters();
                //        paramList.Add("@p_BranchId", "");
                //        paramList.Add("@p_Year", Report.FinancialYear);
                //        ViewBag.TaxComputationList = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Recruitment_ManpowerAttrition", paramList).ToList();
                //        ViewBag.GetMasBranch = "";
                //    }
                //    DynamicParameters param2 = new DynamicParameters();
                //    param2.Add("@p_employeeid", Session["EmployeeId"]);
                //    param2.Add("@p_CmpId", Report.CmpId);
                //    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param2).ToList();
                //    ViewBag.GetMasBranch = data;
                //}
                //else
                //{
                //    ViewBag.ManpowerAttritionList = "";
                //    ViewBag.GetMasBranch = "";
                //}


                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion Reports_Tax_TaxComputaion


        #region DownloadExcel

        public ActionResult DownloadExcelFile(int FYearID,string TType,string Fyear)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add(TType);
                worksheet.Range(1, 1, 1, 35).Merge();
                worksheet.SheetView.FreezeRows(2); // Freeze the first row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();


                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_FYearID", FYearID);
                paramList.Add("@P_Type", TType);
                var GetTaxComputationReport = DapperORM.ExecuteSP<dynamic>("sp_Tax_Rpt_TaxComputation", paramList).ToList();
                if (GetTaxComputationReport.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }
                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(GetTaxComputationReport);
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
                worksheet.Cell(1, 1).Value = "Tax Computation Report  - (" + Fyear + ") - "+ TType + "";
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Columns().AdjustToContents(); // This code for all clomns

                var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
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
                ex.Message.ToString();
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion DownloadExcel

    }
}