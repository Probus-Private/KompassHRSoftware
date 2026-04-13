using ClosedXML.Excel;
using Dapper;
using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.IO;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;

namespace KompassHR.Areas.Reports.Controllers.Report_Payroll
{
    public class Rpt_Payroll_YearlyWiseLoanReportController : Controller
    {
        #region Main View
        // GET: Reports/Rpt_Payroll_YearlyWiseLoanReport
        public ActionResult Rpt_Payroll_YearlyWiseLoanReport(YearlyWiseLoanReportFilter YearlyWiseLoanReportFilter)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 888;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                YearlyWiseLoanReportFilter YearlyWiseLoanReportFilter1 = new YearlyWiseLoanReportFilter();

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetCompanyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.CompanyName = GetCompanyName;
                var CmpId = GetCompanyName[0].Id;


                if (YearlyWiseLoanReportFilter1.CmpId != 0)
                {
                    DynamicParameters paramBranch = new DynamicParameters();
                    paramBranch.Add("@query", "select Distinct BranchId as Id,BranchName as Name from Mas_Branch where Deactivate=0 AND CmpId='" + YearlyWiseLoanReportFilter1.CmpId + "';");
                    var branchList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranch).ToList();
                    ViewBag.BranchName = branchList;

                    DynamicParameters paramYear = new DynamicParameters();
                    paramYear.Add("@query", "select Fid As id,AssessmentYear as Name  from  Tool_FinancialYear where CompanyId='" + YearlyWiseLoanReportFilter1.CmpId + "';");
                    var YearList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramYear).ToList();
                    ViewBag.YearName = YearList;
                }
                else
                {
                    ViewBag.BranchName = new List<AllDropDownBind>();
                    ViewBag.YearName = new List<AllDropDownBind>();
                }
                //if (YearlyWiseLoanReportFilter1.CmpId != 0)
                //{
                //    DynamicParameters paramYear = new DynamicParameters();
                //    paramYear.Add("@query", "select Fid As id,AssessmentYear as Name  from  Tool_FinancialYear where CompanyId='" + YearlyWiseLoanReportFilter1.CmpId + "';");
                //    var YearList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramYear).ToList();
                //    ViewBag.YearName = YearList;
                //}
                //else
                //{
                //    ViewBag.YearName = new List<AllDropDownBind>();
                //}

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
                param.Add("@query", "Select BranchId as Id,BranchName as Name from Mas_Branch where Deactivate=0 AND CmpId='" + CmpId + "' Order By Name");
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.BranchName = Branch;

                DynamicParameters paramYear = new DynamicParameters();
                paramYear.Add("@query", "select Fid As Id,AssessmentYear as Name from  Tool_FinancialYear where  CompanyId='" + CmpId + "';");
                var YearList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramYear).ToList();
                ViewBag.YearName = YearList;

                var data = new{Branch = Branch,YearList = YearList};

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
        public ActionResult DownloadExcelFile(int CmpId,int BranchId, int? FromDate)
        {
            try
            {
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("YearlyAttendanceDetails");
                worksheet.Range(1, 1, 1, 26).Merge();
                worksheet.SheetView.FreezeRows(2); 
                System.Data.DataTable dt = new System.Data.DataTable();
                List<dynamic> data = new List<dynamic>();
  
                DynamicParameters paramList = new DynamicParameters();
               paramList.Add("@P_CmpId", CmpId);
                paramList.Add("@P_FromDate", FromDate);
                paramList.Add("@P_BranchId", BranchId);
                //  paramList.Add("@P_ToDate", ToDate);

                data = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Payroll_YearlyWiseLoanDetails", paramList).ToList();
                if (data.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }
                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(data);
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

                worksheet.Cell(1, 1).Value = "Yearly Wise Loan Details Report";

                //worksheet.Cell(1, 1).Value = "Yearly Wise Loan Details Report - (" + FromDate.ToString("MMM/yyyy") + ") - (" + ToDate.ToString("MMM/yyyy") + ")";
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