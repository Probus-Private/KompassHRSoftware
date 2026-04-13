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

namespace KompassHR.Areas.Reports.Controllers.Reports_ManpowerAllocation
{
    public class Reports_ManpowerAllocation_ContractorWiseRateController : Controller
    {
        #region Reports_ManpowerAllocation_ContractorWiseRate Main View 
        // GET: ESS/Reports_ManpowerAllocation_ContractorWiseRate
        public ActionResult Reports_ManpowerAllocation_ContractorWiseRate(Manpower_ContractorWiseRateRpt obj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 784;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                var CMPID = GetComapnyName[0].Id;

                ViewBag.BUName = new List<AllDropDownBind>();
                ViewBag.ContractorName = new List<AllDropDownBind>();

                if (obj.CmpId > 0)
                {

                    DynamicParameters paramBUName = new DynamicParameters();
                    paramBUName.Add("@query", "select Distinct BranchId as Id,BranchName as Name from Mas_Branch where CmpId='" + obj.CmpId + "';");
                    ViewBag.BUName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBUName).ToList();

                    if (obj.BranchId > 0)
                    {
                        DynamicParameters paramContractorName = new DynamicParameters();
                        paramContractorName.Add("@query", "select Contractor_Master.ContractorId as Id,Contractor_Master.ContractorName as Name from Contractor_Master " +
                        "Inner Join Mas_ContractorMapping on Mas_ContractorMapping.ContractorID = Contractor_Master.ContractorId and Mas_ContractorMapping.IsActive = 1 " +
                       "where Contractor_Master.Deactivate = 0 and Mas_ContractorMapping.CmpID='" + obj.CmpId + "' AND Mas_ContractorMapping.BranchID='" + obj.BranchId + "' ORDER BY Name;");
                        ViewBag.ContractorName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramContractorName).ToList();
                    }
                    else
                    {
                        ViewBag.ContractorName = "";
                    }
                }
                return View(obj);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetContractor
        [HttpGet]
        public ActionResult GetContractor(int BranchId, int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select Contractor_Master.ContractorId as Id,Contractor_Master.ContractorName as Name from Contractor_Master " +
                      "Inner Join Mas_ContractorMapping on Mas_ContractorMapping.ContractorID = Contractor_Master.ContractorId and Mas_ContractorMapping.IsActive = 1 " +
                     "where Contractor_Master.Deactivate = 0 and Mas_ContractorMapping.CmpID='" + CmpId + "' AND Mas_ContractorMapping.BranchID='" + BranchId + "' ORDER BY Name;");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);

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
                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CmpId), Convert.ToInt32(Session["EmployeeId"]));
                return Json(new { Branch = Branch }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId,int ContractorId, DateTime FromMonth, DateTime ToMonth)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("ContracterWiseRateSheet");
                worksheet.Range(1, 1, 1, 10).Merge();
                worksheet.SheetView.FreezeRows(2); // Freeze the first row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();


                DynamicParameters paramHC = new DynamicParameters();
                paramHC.Add("@p_CmpId", CmpId);
                paramHC.Add("@p_BranchId", BranchId);
                paramHC.Add("@p_ContractorId",ContractorId);
                paramHC.Add("@p_FromMonth", FromMonth);
                paramHC.Add("@p_ToMonth", ToMonth);
                var GetData = DapperORM.ExecuteSP<dynamic>("sp_Manpower_Rpt_ContracterWiseRate", paramHC).ToList();

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
                // Additional styling if required
                // lastRow.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                lastRow.Style.Font.FontColor = XLColor.Black;
                lastRow.Style.Font.FontSize = 10;

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
                worksheet.Cell(1, 1).Value = "Contractor Wise Rate Report - (" + FromMonth.ToString("dd/MMM/yyyy") + " to " + ToMonth.ToString("dd/MMM/yyyy") + ") ";
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
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
    }
}