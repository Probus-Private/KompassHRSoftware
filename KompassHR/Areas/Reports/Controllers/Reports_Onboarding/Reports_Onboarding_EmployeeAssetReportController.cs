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

namespace KompassHR.Areas.Reports.Controllers.Reports_Onboarding
{
    public class Reports_Onboarding_EmployeeAssetReportController : Controller
    {
        // GET: Reports/Reports_Onboarding_EmployeeAssetReport
        #region Main View
        public ActionResult Reports_Onboarding_EmployeeAssetReport()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 753;
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
                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CMPID), Convert.ToInt32(Session["EmployeeId"]));
                ViewBag.BranchName = Branch;


                DynamicParameters paramAssetName = new DynamicParameters();
                paramAssetName.Add("@query", "SELECT DISTINCT Mas_Asset.AssetId AS[Id], Mas_Asset.AssetName AS [Name] FROM Mas_Employee INNER JOIN Mas_Employee_Asset MEA ON Mas_Employee.EmployeeId = MEA.AssetEmployeeId INNER JOIN Mas_Asset  ON MEA.AssetName = Mas_Asset.AssetId INNER JOIN Mas_Branch  ON Mas_Employee.EmployeeBranchId = Mas_Branch.BranchId INNER JOIN Mas_CompanyProfile ON Mas_Employee.CmpId = Mas_CompanyProfile.CompanyId");

               // paramAssetName.Add("@query", "SELECT DISTINCT Mas_Asset.AssetId AS[Id], Mas_Asset.AssetName AS [Name] FROM Mas_Employee INNER JOIN Mas_Employee_Asset MEA ON Mas_Employee.EmployeeId = MEA.AssetEmployeeId INNER JOIN Mas_Asset  ON MEA.AssetName = Mas_Asset.AssetId INNER JOIN Mas_Branch  ON Mas_Employee.EmployeeBranchId = Mas_Branch.BranchId INNER JOIN Mas_CompanyProfile ON Mas_Employee.CmpId = Mas_CompanyProfile.CompanyId WHERE Mas_Employee.Deactivate = 0 AND Mas_CompanyProfile.CompanyId = " + CmpId + " AND Mas_Employee.EmployeeBranchId = " + BranchId + "");
                ViewBag.AssetName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramAssetName).ToList();
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
        public ActionResult GetMonthlyBusinessUnit(int CmpId)
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


        #region GetAssetName
        [HttpGet]
        public ActionResult GetAssetNameDateWise(int CmpId, int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();

                param.Add("@query", "SELECT DISTINCT Mas_Asset.AssetId AS[Id], Mas_Asset.AssetName AS [Name] FROM Mas_Employee INNER JOIN Mas_Employee_Asset MEA ON Mas_Employee.EmployeeId = MEA.AssetEmployeeId INNER JOIN Mas_Asset  ON MEA.AssetName = Mas_Asset.AssetId INNER JOIN Mas_Branch  ON Mas_Employee.EmployeeBranchId = Mas_Branch.BranchId INNER JOIN Mas_CompanyProfile ON Mas_Employee.CmpId = Mas_CompanyProfile.CompanyId WHERE Mas_Employee.Deactivate = 0 AND Mas_CompanyProfile.CompanyId = '" + CmpId+"'AND Mas_Employee.EmployeeBranchId = '"+ BranchId + "'");
                
               var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
               // return Json(new { data = data }, JsonRequestBehavior.AllowGet);
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
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, DateTime FromDate, DateTime ToDate,int? AssetId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("EmployeeAssetReport");
                worksheet.Range(1, 1, 1, 10).Merge();
                worksheet.SheetView.FreezeRows(2); 
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();


                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                paramList.Add("@p_FromDate", FromDate);
                paramList.Add("@p_ToDate", ToDate);
                paramList.Add("@p_CompanyId", CmpId);
                paramList.Add("@p_BranchId", BranchId);
                paramList.Add("@p_AssetId", AssetId);
           
                var GetData = DapperORM.ExecuteSP<dynamic>("sp_Onboarding_EmployeeAsset", paramList).ToList();
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
                worksheet.Cell(1, 1).Value = "Employee Asset Report - (" + FromDate.ToString("dd/MMM/yyyy") + " to " + ToDate.ToString("dd/MMM/yyyy") + ")";
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