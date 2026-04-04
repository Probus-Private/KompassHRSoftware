using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Report_Payroll
{
    public class Rpt_Payroll_PaidBonusController : Controller
    {
        #region Main View
        // GET: Reports/Rpt_Payroll_PaidBonus
        public ActionResult Rpt_Payroll_PaidBonus(PaidBonus PaidBonus)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 852;
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
                //GET BRANCH NAME
                if (PaidBonus.CmpId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select BranchId As Id,BranchName As Name From Mas_Branch where Deactivate=0 And CmpId='" + PaidBonus.CmpId + "' order by Name");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    ViewBag.BranchName = data;
                }
                else
                {
                    ViewBag.BranchName = "";
                }

                DynamicParameters fy = new DynamicParameters();
                fy.Add("@query", "SELECT BonusYearId As Id, CONCAT(YEAR(FromYear), '-', YEAR(ToYear)) AS Name FROM Payroll_BonusYear");
                //  fy.Add("@query", "select Fid as Id, TDSYear as Name from Tool_FinancialYear");
                ViewBag.FinantialYear = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", fy).ToList();

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region Get Branch Name
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
                param.Add("@query", "select BranchId As Id,BranchName As Name From Mas_Branch where Deactivate=0 And CmpId='" + CmpId + "' order by Name");
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                // return Json(data, JsonRequestBehavior.AllowGet);
                return Json(new { Branch = Branch }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region Download Excel
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, int? FinantialYear)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return Json(new { success = false, message = "Session expired" }, JsonRequestBehavior.AllowGet);
                }

                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                paramList.Add("@p_CompanyId", CmpId);
                paramList.Add("@p_BranchId", BranchId);
                paramList.Add("@p_FinantialYear", FinantialYear);

                var GetDailyAttendanceReport =
                    DapperORM.ExecuteSP<dynamic>("sp_Rpt_Payroll_PayrollPaidBonus", paramList).ToList();

                if (GetDailyAttendanceReport.Count == 0)
                {
                    return Json(new { success = false, message = "Record Not Found" }, JsonRequestBehavior.AllowGet);
                }

                DapperORM dprObj = new DapperORM();
                DataTable dt = dprObj.ConvertToDataTable(GetDailyAttendanceReport);
                int totalColumns = dt.Columns.Count;

                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("PaidBonus");

                worksheet.SheetView.FreezeRows(3);
                worksheet.Range(1, 1, 1, totalColumns).Merge();
                worksheet.Cell(1, 1).Value = "Paid Bonus Report";
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(1, 1).Style.Font.Bold = true;

                worksheet.Cell(2, 1).InsertTable(dt, false);

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
                headerRange.Style.Font.FontColor = XLColor.Black;
                headerRange.Style.Font.Bold = true;

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    string fileBase64 = Convert.ToBase64String(stream.ToArray());

                    return Json(new
                    {
                        success = true,
                        fileName = "PaidBonusReport.xlsx",
                        fileContents = fileBase64
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        

    }
}