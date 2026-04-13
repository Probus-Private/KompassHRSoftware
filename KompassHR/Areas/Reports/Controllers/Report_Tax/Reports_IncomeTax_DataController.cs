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

namespace KompassHR.Areas.Reports.Controllers.Report_Tax
{
    public class Reports_IncomeTax_DataController : Controller
    {
        #region Main View 
        // GET: Reports/Reports_IncomeTax_Data
        public ActionResult Reports_IncomeTax_Data()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 914;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                //GET COMPANY NAME
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                var CMPID = GetComapnyName[0].Id;


                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "Select TaxFyearId as Id,TaxYear As Name  from IncomeTax_Fyear where Deactivate=0 and [IsActive]=1 Order By Name");
                var FinantialYear = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.FinantialYear = FinantialYear;

                return View();

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion



        //#region Get Finaantial Year 
        //[HttpGet]
        //public ActionResult GetFinantialYear(int CmpId)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { area = "" });
        //        }
        //        DynamicParameters param = new DynamicParameters();
        //        param.Add("@query", "Select TaxFyearId as Id,TaxYear As Name  from IncomeTax_Fyear where Deactivate=0 and [IsActive]=1 Order By Name");
        //        var FinantialYear = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

        //    return Json(new { FinantialYear = FinantialYear }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
        //}
        //#endregion


        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? CmpId, int? FinantialYear, string IncomeTaxType)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("WeekOffAdjustmentReport");

                worksheet.Range(1, 1, 1, 10).Merge();
                worksheet.SheetView.FreezeRows(2);
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();

                DynamicParameters paramList = new DynamicParameters();
              //  paramList.Add("@p_WeekOffType", IncomeTaxType);
                paramList.Add("@p_CmpId", CmpId);
                paramList.Add("@FyearId", FinantialYear);

                List<dynamic> GetData = new List<dynamic>();

                //if (IncomeTaxType == "Bonus")
                //{
                //     GetData = DapperORM.ExecuteSP<dynamic>("sp_Tax_Rpt_Bonus", paramList).ToList();

                //}

                //if (IncomeTaxType == "Leave")
                //{
                //    GetData = DapperORM.ExecuteSP<dynamic>("sp_Tax_Rpt_LeaveEncashment", paramList).ToList();

                //}

                //if (IncomeTaxType == "Perquisite")
                //{
                //    GetData = DapperORM.ExecuteSP<dynamic>("sp_Tax_Rpt_Perquisite", paramList).ToList();

                //}
                //if (IncomeTaxType == "Salary")
                //{
                //    GetData = DapperORM.ExecuteSP<dynamic>("sp_Tax_Rpt_Salary", paramList).ToList();

                //}

                //if (IncomeTaxType == "OtherThanSalary")
                //{
                //    GetData = DapperORM.ExecuteSP<dynamic>("sp_Tax_Rpt_OtherThanSalary", paramList).ToList();

                //}

                //if (IncomeTaxType == "InvestmentDeclaration")
                //{
                //    GetData = DapperORM.ExecuteSP<dynamic>("sp_Tax_Rpt_InvestmentDeclaration", paramList).ToList();

                //}

                //if (IncomeTaxType == "FNF")
                //{
                //    GetData = DapperORM.ExecuteSP<dynamic>("sp_Tax_Rpt_FNF", paramList).ToList();

                //}

                string spName = DapperORM.ExecuteSP<string>( "sp_Get_IncomeTax_SPName", new { DisplayName = IncomeTaxType }).FirstOrDefault();

                if (string.IsNullOrEmpty(spName))
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "SPNotFound.txt");
                }

                GetData = DapperORM.ExecuteSP<dynamic>(spName, paramList).ToList();

                if (GetData.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }
                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(GetData);
                worksheet.Cell(2, 1).InsertTable(dt, false);
                int totalRows = worksheet.RowsUsed().Count();

                var lastRow = worksheet.Row(totalRows + 1);
                lastRow.Style.Font.Bold = true;

                lastRow.Style.Font.FontColor = XLColor.Black;
                lastRow.Style.Font.FontSize = 10;
                var usedRange = worksheet.RangeUsed();
                usedRange.Style.Fill.BackgroundColor = XLColor.White;
                usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Font.FontSize = 10;
                usedRange.Style.Font.FontColor = XLColor.Black;
                worksheet.Cell(1, 1).Value = "Income Tax Data Report";

                //worksheet.Cell(1, 1).Value = "Late Mark AdjustmentReport Report - (" + Month.ToString("dd/MMM/yyyy") + ")";
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