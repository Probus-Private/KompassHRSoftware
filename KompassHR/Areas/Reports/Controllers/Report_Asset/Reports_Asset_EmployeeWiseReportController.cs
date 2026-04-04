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

namespace KompassHR.Areas.Reports.Controllers.Report_Asset
{
    public class Reports_Asset_EmployeeWiseReportController : Controller
    {
        #region Main View
        // GET: Reports/Reports_Asset_EmployeeWiseReport
        public ActionResult Reports_Asset_EmployeeWiseReport(EmployeeWiseReportFilter EmployeeReport)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 843;
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


                if (EmployeeReport.CmpId != 0)
                {
                    DynamicParameters paramBranch = new DynamicParameters();
                    paramBranch.Add("@query", "select Distinct BranchId as Id,BranchName as Name from Mas_Branch where Deactivate=0 AND CmpId='" + EmployeeReport.CmpId + "';");
                    var branchList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranch).ToList();
                    ViewBag.BranchName = branchList;

                }
                else
                {
                    ViewBag.BranchName = new List<AllDropDownBind>();
                }

                if (EmployeeReport.CmpId != 0 && EmployeeReport.BranchId != null)
                {

                    DynamicParameters paramEmpName = new DynamicParameters();
                    paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName, ' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate = 0 and EmployeeLeft=0 AND CmpID='" + EmployeeReport.CmpId + "'  order by Name");

                    // paramAsset.Add("@query", "SELECT DISTINCT Mas_Asset.AssetId AS[Id], Mas_Asset.AssetName AS [Name] FROM Mas_Employee INNER JOIN Mas_Employee_Asset MEA ON Mas_Employee.EmployeeId = MEA.AssetEmployeeId INNER JOIN Mas_Asset  ON MEA.AssetName = Mas_Asset.AssetId INNER JOIN Mas_Branch  ON Mas_Employee.EmployeeBranchId = Mas_Branch.BranchId INNER JOIN Mas_CompanyProfile ON Mas_Employee.CmpId = Mas_CompanyProfile.CompanyId WHERE Mas_Employee.Deactivate = 0 AND Mas_CompanyProfile.CompanyId = '" + EmployeeReport.CmpId + "'AND Mas_Employee.EmployeeBranchId = '" + EmployeeReport.BranchId + "' order By Name");
                    var EmployeeList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                    ViewBag.AssetName = EmployeeList;
                }

                else
                {
                    ViewBag.EmployeeName = new List<AllDropDownBind>();
                }

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
                //  var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CmpId), Convert.ToInt32(Session["EmployeeId"]));

                DynamicParameters paramBranchName = new DynamicParameters();
                paramBranchName.Add("@query", "select BranchId as Id, BranchName as Name  from Mas_Branch where Deactivate = 0 and CmpId ='" + CmpId + "'  order by Name");
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranchName).ToList();
                ViewBag.BranchName = Branch;

                DynamicParameters paramEmpName = new DynamicParameters();
                // paramAsset.Add("@query", "SELECT DISTINCT Mas_Asset.AssetId AS[Id], Mas_Asset.AssetName AS [Name] FROM Mas_Employee INNER JOIN Mas_Employee_Asset MEA ON Mas_Employee.EmployeeId = MEA.AssetEmployeeId INNER JOIN Mas_Asset  ON MEA.AssetName = Mas_Asset.AssetId INNER JOIN Mas_CompanyProfile ON Mas_Employee.CmpId = Mas_CompanyProfile.CompanyId WHERE Mas_Employee.Deactivate = 0 AND Mas_CompanyProfile.CompanyId = '" + CmpId + "'order By Name");
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName, ' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate = 0 and EmployeeLeft=0 AND CmpID='" + CmpId + "'  order by Name");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
          

                return Json(new { Branch = Branch, EmployeeName = EmployeeName }, JsonRequestBehavior.AllowGet);
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
                // DynamicParameters param = new DynamicParameters();

                if (BranchId == 0 || BranchId == null)
                {
                    DynamicParameters paramEmpName = new DynamicParameters();
                    paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName, ' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate = 0 and EmployeeLeft=0 AND CmpID='" + CmpId + "'  order by Name");
                    // paramAsset.Add("@query", "SELECT DISTINCT Mas_Asset.AssetId AS[Id], Mas_Asset.AssetName AS [Name] FROM Mas_Employee INNER JOIN Mas_Employee_Asset MEA ON Mas_Employee.EmployeeId = MEA.AssetEmployeeId INNER JOIN Mas_Asset  ON MEA.AssetName = Mas_Asset.AssetId INNER JOIN Mas_CompanyProfile ON Mas_Employee.CmpId = Mas_CompanyProfile.CompanyId WHERE Mas_Employee.Deactivate = 0 AND Mas_CompanyProfile.CompanyId = '" + CmpId + "'order By Name");
                    var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                    return Json(EmployeeName, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    DynamicParameters paramEmpName = new DynamicParameters();
                    paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName, ' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate = 0 and EmployeeLeft=0 AND CmpID='" + CmpId + "' AND EmployeeBranchId='"+BranchId+"'  order by Name");
                    //paramAsset.Add("@query", "SELECT DISTINCT Mas_Asset.AssetId AS[Id], Mas_Asset.AssetName AS [Name] FROM Mas_Employee INNER JOIN Mas_Employee_Asset MEA ON Mas_Employee.EmployeeId = MEA.AssetEmployeeId INNER JOIN Mas_Asset  ON MEA.AssetName = Mas_Asset.AssetId INNER JOIN Mas_Branch  ON Mas_Employee.EmployeeBranchId = Mas_Branch.BranchId INNER JOIN Mas_CompanyProfile ON Mas_Employee.CmpId = Mas_CompanyProfile.CompanyId WHERE Mas_Employee.Deactivate = 0 AND Mas_CompanyProfile.CompanyId = '" + CmpId + "'AND Mas_Employee.EmployeeBranchId = '" + BranchId + "' order By Name");
                    var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                    return Json(EmployeeName, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Download Excel File
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, int? EmployeeId, DateTime? Month = null)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Employee Wise Report");

                worksheet.Range(1, 1, 1, 10).Merge();
                worksheet.SheetView.FreezeRows(2);

                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();

                DynamicParameters paramList = new DynamicParameters();
              //  paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                paramList.Add("@p_MonthYear", Month.HasValue ? (object)Month.Value : null);
                paramList.Add("@p_CompanyId", CmpId);
                paramList.Add("@p_BranchId", BranchId);
                paramList.Add("@p_EmployeeId", EmployeeId);

                var GetData = DapperORM.ExecuteSP<dynamic>("sp_Asset_EmployeeWiseReport", paramList).ToList();

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
               // lastRow.Style.Font.FontColor = XLColor.Black;
                lastRow.Style.Font.FontSize = 10;

                var usedRange = worksheet.RangeUsed();
                usedRange.Style.Fill.BackgroundColor = XLColor.White;
                usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Font.FontSize = 10;
                usedRange.Style.Font.FontColor = XLColor.Black;

                worksheet.Cell(1, 1).Value = Month.HasValue ? $"Employee Wise Report - ({Month.Value:dd/MMM/yyyy})" : "Asset Wise Report";
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

                    string fileName = Month.HasValue ? $"Report({Month.Value:ddMMMyyyy}).xlsx" : "Report.xlsx";
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
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