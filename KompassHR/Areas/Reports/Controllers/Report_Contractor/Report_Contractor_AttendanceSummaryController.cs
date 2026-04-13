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

namespace KompassHR.Areas.Reports.Controllers.Report_Contractor
{
    public class Report_Contractor_AttendanceSummaryController : Controller
    {
        #region Main View
        // GET: Reports/Report_Contractor_AttendanceSummary
        public ActionResult Report_Contractor_AttendanceSummary()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 933;
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
                ViewBag.ContractorName = "";

                //DynamicParameters param = new DynamicParameters();
                //param.Add("@p_employeeid", Session["EmployeeId"]);
                //param.Add("@p_CmpId", CmpId);
                //ViewBag.BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                //if (InvoiceMonth != null && CmpId != null && BranchId != null && ContractorId != null)
                //{
                //    DynamicParameters param1 = new DynamicParameters();
                //    param1.Add("@p_MonthYear", InvoiceMonth);
                //    param1.Add("@p_CmpId", CmpId);
                //    param1.Add("@p_BranchId", BranchId);
                //    param1.Add("@p_ContractorId", ContractorId);
                //    var data = DapperORM.ExecuteSP<dynamic>("SP_Invoice_Conctrator_Invoice_Show", param1).ToList();

                //    int hideCount = 0;
                //    if (data.Count > 0)
                //    {
                //        var hideValue = ((IDictionary<string, object>)data[0])["Hide"];

                //        if (hideValue != null && hideValue is int)
                //        {
                //            hideCount = (int)hideValue;
                //        }
                //        else if (hideValue != null)
                //        {
                //            int.TryParse(hideValue.ToString(), out hideCount);
                //        }
                //    }
                //    ViewBag.HideCount = hideCount;
                //    ViewBag.InvoiceData = data;


                //    DynamicParameters param = new DynamicParameters();
                //    param.Add("@p_employeeid", Session["EmployeeId"]);
                //    param.Add("@p_CmpId", CmpId);
                //    ViewBag.BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                //    DynamicParameters Contractparam = new DynamicParameters();
                //    Contractparam.Add("@Query", "select  DISTINCT Contractor_Master.ContractorId as Id,Contractor_Master.ContractorName As Name from Contractor_Master Inner Join Mas_ContractorMapping on  Mas_ContractorMapping.ContractorID = Contractor_Master.ContractorId where Contractor_Master.Deactivate = 0 and Mas_ContractorMapping.CmpID = '" + CmpId + "' and Mas_ContractorMapping.BranchId = '" + BranchId + "'");
                //    ViewBag.ContractorName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", Contractparam).ToList();

                //}
                //else
                //{
                //    ViewBag.InvoiceData = new List<dynamic>();
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

        #region GetContractorName
        [HttpGet]
        public ActionResult GetContractorName(int CmpId, int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                //DynamicParameters param = new DynamicParameters();
                //param.Add("@Query", "select  DISTINCT Contractor_Master.ContractorId as Id,Contractor_Master.ContractorName As Name from Contractor_Master Inner Join Mas_ContractorMapping on  Mas_ContractorMapping.ContractorID = Contractor_Master.ContractorId where Contractor_Master.Deactivate = 0 and Mas_ContractorMapping.CmpID = '" + CmpId + "' and Mas_ContractorMapping.BranchId = '" + BranchId + "'");
                //var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                //return Json(data, JsonRequestBehavior.AllowGet);

                DynamicParameters paramContractorName = new DynamicParameters();
                paramContractorName.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + BranchId + "");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", paramContractorName).ToList();
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
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, int? ContractorId, DateTime InvoiceMonth)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("AttendanceSummaryReport");

                worksheet.Range(1, 1, 1, 10).Merge();
                worksheet.SheetView.FreezeRows(2);
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();

                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_ContractorId", ContractorId);
                paramList.Add("@p_InvoiceMonth", InvoiceMonth);
                paramList.Add("@p_CompanyId", CmpId);
                paramList.Add("@p_BranchId", BranchId);
                var GetData = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Contractor_AttendanceSummaryReport", paramList).ToList();
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
                lastRow.Style.Font.Bold = false;

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
                worksheet.Cell(1, 1).Value = "Attendance Summary Report";

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