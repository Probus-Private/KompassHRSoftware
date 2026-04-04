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
using System.Web.Hosting;
using System.Web.Mvc;


namespace KompassHR.Areas.Reports.Controllers.Report_Claim
{
    public class Reports_Claim_GeneralReportController : Controller
    {
        // GET: Reports/Reports_Claim_GeneralReport
        public ActionResult Reports_Claim_GeneralReport()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 392;
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
                ViewBag.BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                var results = DapperORM.DynamicQueryMultiple(@"select DepartmentId as Id, DepartmentName as Name from Mas_Department where Deactivate=0 order by DepartmentName; 
                                                  select DesignationId as Id,DesignationName as Name from Mas_Designation where Deactivate=0 order by DesignationName
                                                  select GradeId as Id,GradeName as Name from Mas_Grade where Deactivate=0 order by Name");
                ViewBag.GradeName = results[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


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
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, int? DepartmentId, int? DesignationId, DateTime Month)
        {
            try
            {
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("GeneralClaim Report");
                worksheet.Range(1, 1, 1, 21).Merge();
                worksheet.SheetView.FreezeRows(2); // Freeze the row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();

                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_CompanyId", CmpId);
                paramList.Add("@p_BranchId", BranchId);
                paramList.Add("@p_Month", Month);
                paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                data = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Claim_GeneralClaim", paramList).ToList();
                if (data.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }

                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(data);
                //dt.Rows.RemoveAt(0);
                worksheet.Cell(2, 1).InsertTable(dt, false);

                // Handle all *_Link columns dynamically
                foreach (DataColumn col in dt.Columns)
                {
                    if (!col.ColumnName.EndsWith("_Link"))
                        continue;

                    int colIndex = col.Ordinal + 1;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var cell = worksheet.Cell(i + 3, colIndex);
                        string link = cell.GetString();

                        if (string.IsNullOrWhiteSpace(link))
                        {
                            cell.Value = "";
                            continue;
                        }

                        if (link.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                            link.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                        {
                            cell.Hyperlink = new XLHyperlink(link);
                            cell.Value = "Open";
                            cell.Style.Font.FontColor = XLColor.Blue;
                            cell.Style.Font.Underline = XLFontUnderlineValues.Single;
                            continue;
                        }
                        string fullPath = HostingEnvironment.MapPath(link);
                        if (System.IO.File.Exists(fullPath))
                        {
                            cell.Hyperlink = new XLHyperlink(link);
                            cell.Value = "Open";
                            cell.Style.Font.FontColor = XLColor.Blue;
                            cell.Style.Font.Underline = XLFontUnderlineValues.Single;
                        }
                        else
                        {
                            cell.Value = "";
                        }
                    }
                }
                        //        string fullPath = link;
                        //        if (!System.IO.Path.IsPathRooted(fullPath))
                        //        {
                        //           fullPath = System.Web.Hosting.HostingEnvironment.MapPath(link);
                        //        }
                        //        if (System.IO.File.Exists(fullPath))
                        //        {
                        //            cell.Hyperlink = new XLHyperlink(link);
                        //            cell.Value = "Open";
                        //            cell.Style.Font.FontColor = XLColor.Blue;
                        //            cell.Style.Font.Underline = XLFontUnderlineValues.Single;
                        //        }
                        //        else
                        //        {
                        //            cell.Value = "";
                        //        }
                        //    }
                        //}


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
                worksheet.Cell(1, 1).Value = "General Claim Report - (" + Month.ToString("MMM/yyyy") + ")";
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(1, 1).Style.Font.Bold = true;

                // Set the header row background color to grey and font color to black
                var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                headerRange.Style.Font.FontSize = 10;
                headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
                headerRange.Style.Font.Bold = true;

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0; // Reset the stream position to the beginning

                    // Return the file to the client
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TravelClaimReport.xlsx");
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