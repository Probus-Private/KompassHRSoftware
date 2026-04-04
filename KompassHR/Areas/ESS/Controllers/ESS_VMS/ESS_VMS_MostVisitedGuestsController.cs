using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Models;
using KompassHR.Areas.ESS.Models.ESS_VMS;
using KompassHR.Areas.Setting.Models.Setting_Leave;
using ClosedXML.Excel;
using System.Data;
using System.IO;

namespace KompassHR.Areas.ESS.Controllers.ESS_VMS
{
    public class ESS_VMS_MostVisitedGuestsController : Controller
    {
        // GET: ESS/ESS_VMS_MostVisitedGuests
        #region ESS_VMS_MostVisitedGuests
        public ActionResult ESS_VMS_MostVisitedGuests(Visitor_Report VisitorReport)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 252;
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
                ViewBag.GetBusinessUnit = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();


                //if (VisitorReport.CmpID != null)
                //{
                //    DynamicParameters paramMostVisitor = new DynamicParameters();
                //    paramMostVisitor.Add("@p_FromDate", VisitorReport.FromDate);
                //    paramMostVisitor.Add("@p_Todate", VisitorReport.ToDate);
                //    paramMostVisitor.Add("@p_branchid", VisitorReport.BranchId);
                //    var getMostVisitedGuestReports = DapperORM.ExecuteSP<dynamic>("sp_RPTMostVisitedGuest", paramMostVisitor).ToList();
                //    ViewBag.getMostVisitedGuest = getMostVisitedGuestReports;

                //    DynamicParameters paramBranch = new DynamicParameters();
                //    paramBranch.Add("@query", "Select  BranchId as Id, BranchName As Name from Mas_Branch where Deactivate=0 and CmpId= '" + VisitorReport.CmpID + "'");
                //    var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranch).ToList();
                //    ViewBag.GetBranchName = BranchName;
                //    if (getMostVisitedGuestReports.Count == 0)
                //    {
                //        TempData["Message"] = "Record Not Found";
                //        TempData["Icon"] = "error";
                //    }

                //}
                //else
                //{
                //    ViewBag.getMostVisitedGuest = "";
                //    ViewBag.GetBranchName = "";
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


        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, DateTime FromDate, DateTime ToDate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("MostVisitedGuests");
                worksheet.Range(1, 1, 1, 35).Merge();
                worksheet.SheetView.FreezeRows(2); // Freeze the first row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();
                var GetFromDate = FromDate.ToString("yyyy-MM-dd");
                var GetToDate = ToDate.ToString("yyyy-MM-dd");
                //var Query = "";
                //if (CmpId != null)
                //{
                //    Query = " and Mas_Employee.CmpId=" + CmpId + "";
                //    if (BranchId != null)
                //    {
                //        Query = Query + " AND Mas_Employee.EmployeeBranchId = " + BranchId + "";
                //    }
                //    else
                //    {
                //        Query = Query + " and Mas_Employee.EmployeeBranchId in (select BranchID as Id from UserBranchMapping where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and UserBranchMapping.CmpID = " + CmpId + " and UserBranchMapping.IsActive = 1)";
                //    }
                //    Query = Query + " and convert(date, Visitor_Tra_Master.InDateTime, 106) between '" + GetFromDate + "' and '" + GetToDate + "'";

                //}

                DynamicParameters paramMostVisitor = new DynamicParameters();
                paramMostVisitor.Add("@p_FromDate", GetFromDate);
                paramMostVisitor.Add("@p_Todate", ToDate);
                paramMostVisitor.Add("@p_branchid", BranchId);
                var getDateWiseReports = DapperORM.ExecuteSP<dynamic>("sp_RPTMostVisitedGuest", paramMostVisitor).ToList();
                if (getDateWiseReports.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }
                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(getDateWiseReports);
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
                worksheet.Cell(1, 1).Value = "Most Visited Guests - (" + FromDate.ToString("dd/MMM/yyyy") + " - " + ToDate.ToString("dd/MMM/yyyy") + ")";
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Columns().AdjustToContents(); // This code for all clomns

                var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                headerRange.Style.Font.FontSize = 10;
                headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
                headerRange.Style.Font.Bold = true;

                // Save the workbook
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