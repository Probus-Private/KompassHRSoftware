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
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Report_Payroll
{
    public class Rpt_Payroll_PFECRController : Controller
    {
        // GET: Reports/Rpt_Payroll_PFECR
        #region Main View
        public ActionResult Rpt_Payroll_PFECR()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 458;
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

                DynamicParameters paramPFCode = new DynamicParameters();
                paramPFCode.Add("@query", $@"Select PFCodeId as Id ,PFCode As Name  from Payroll_PFCode Where Deactivate=0 And CmpId={CMPID}");
                var PFCode = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", paramPFCode).ToList();
                ViewBag.PFCode = PFCode;

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

                DynamicParameters paramPFCode = new DynamicParameters();
                paramPFCode.Add("@query", $@"Select PFCodeId as Id ,PFCode As Name  from Payroll_PFCode Where Deactivate=0 And CmpId={CmpId}");
                var PFCode = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", paramPFCode).ToList();
                
                return Json(new { Branch = Branch , PFCode = PFCode }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region DownloadExcelFile

        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, DateTime Month, int? PFCode, string FileType)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (FileType == "Excel")
                {
                    var workbook = new XLWorkbook();
                    var worksheet = workbook.Worksheets.Add("PFECRReport");
                    worksheet.Range(1, 1, 1, 10).Merge();
                    worksheet.SheetView.FreezeRows(2); // Freeze the first row
                    DataTable dt = new DataTable();
                    List<dynamic> data = new List<dynamic>();
                    DynamicParameters paramExcel = new DynamicParameters();
                    paramExcel.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramExcel.Add("@p_MonthYear", Month);
                    paramExcel.Add("@p_CompanyId", CmpId);
                    paramExcel.Add("@p_BranchId", BranchId);
                    paramExcel.Add("@p_PFCode", PFCode);
                    var GetExcelData = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Payroll_ECR_Excel", paramExcel).ToList();
                    if (GetExcelData.Count == 0)
                    {
                        byte[] emptyFileContents = new byte[0];
                        return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetExcelData);
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
                    worksheet.Cell(1, 1).Value = "Payroll ECR Report - (" + Month.ToString("dd/MMM/yyyy") + ")";
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
                if (FileType == "Text")
                {
                    DynamicParameters paramText = new DynamicParameters();
                    paramText.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramText.Add("@p_MonthYear", Month);
                    paramText.Add("@p_CompanyId", CmpId);
                    paramText.Add("@p_BranchId", BranchId);
                    paramText.Add("@p_PFCode", PFCode);
                    var GetTextData = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Payroll_ECR_Text", paramText).ToList();
                    // Convert the data to a string
                    StringBuilder fileContent = new StringBuilder();
                    foreach (var item in GetTextData)
                    {
                        // Assuming each row has dynamic properties, convert them to string
                        foreach (var property in ((IDictionary<string, object>)item))
                        {
                            fileContent.Append($"{property.Value}\t"); // Tab-separated or adjust format
                        }
                        fileContent.AppendLine(); // Add a new line at the end of each row
                    }

                    // Prepare the text file content as bytes
                    var fileBytes = Encoding.UTF8.GetBytes(fileContent.ToString());
                    var fileName = "PayrollECRReport.txt";

                    // Return the file for download
                    return File(fileBytes, "text/plain", fileName);
                }
                return new HttpStatusCodeResult(400, "Invalid File Type");
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