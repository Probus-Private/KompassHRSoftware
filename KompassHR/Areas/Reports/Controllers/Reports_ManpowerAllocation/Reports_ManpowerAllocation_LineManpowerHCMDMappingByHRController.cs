using ClosedXML.Excel;
using Dapper;
using DocumentFormat.OpenXml.Drawing.Charts;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;       

namespace KompassHR.Areas.Reports.Controllers.Reports_ManpowerAllocation
{
    public class Reports_ManpowerAllocation_LineManpowerHCMDMappingByHRController : Controller
    {
        // GET: Reports/Reports_ManpowerAllocation_LineManpowerHCMDMappingByHR
        #region MainView
        public ActionResult Reports_ManpowerAllocation_LineManpowerHCMDMappingByHR()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                // Check access permissions
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 780;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetCompanyName = GetComapnyName;

                ViewBag.GetBranchName = "";

                


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
                DynamicParameters paramBU = new DynamicParameters();
                paramBU.Add("@p_employeeid", Session["EmployeeId"]);
                paramBU.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBU).ToList();
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
        public ActionResult DownloadExcelFile(double? CmpId, double? BranchId, DateTime Date)
        {
            try
            {
                //if (Session["EmployeeId"] == null)
                //{
                //    return RedirectToAction("Login", "Login", new { Area = "" });
                //}

                // Initialize Dapper parameters
                DynamicParameters paramHC = new DynamicParameters();
                paramHC.Add("@p_CmpId", CmpId);
                paramHC.Add("@p_BranchId", BranchId);
                paramHC.Add("@p_Date", Date);

                var data = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Manpower_HC_MDMappingByHR", paramHC).ToList();

                //if (!)
                //{
                //    TempData["Message"] = "No data available to export.";
                //    TempData["Icon"] = "error";
                //    //return RedirectToAction("Reports_ManpowerAllocation_LineManpowerHCMDMappingByHR");
                //    return Json(new { success = false, message = "No data available to export." }, JsonRequestBehavior.AllowGet);
                //}

                if (data.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }


                // if (data.Any())
                // {
                using (var workbook = new XLWorkbook())
                    {
                        var ws = workbook.Worksheets.Add("HC_MDMapping");

                        // Convert first record to dictionary
                        var firstRow = (IDictionary<string, object>)data.First();

                        string companyName = firstRow["CompanyName"]?.ToString();
                        string branchName = firstRow["BranchName"]?.ToString();
                        string reportDate = Convert.ToDateTime(Date).ToString("dd-MMM-yyyy");

                        int currentRow = 1;

                        // === Header Rows ===
                        ws.Cell(currentRow, 1).Value = $"Company: {companyName}";
                        ws.Cell(currentRow, 1).Style.Font.Bold = true;
                        currentRow++;

                        ws.Cell(currentRow, 1).Value = $"Branch: {branchName}";
                        ws.Cell(currentRow, 1).Style.Font.Bold = true;
                        currentRow++;

                        ws.Cell(currentRow, 1).Value = $"Date: {reportDate}";
                        ws.Cell(currentRow, 1).Style.Font.Bold = true;
                        currentRow += 2;

                        // === Main Title ===
                        ws.Cell(currentRow, 1).Value = "Line Manpower Category HC/MD Mapping";
                        ws.Range(currentRow, 1, currentRow, 3).Merge();
                        ws.Cell(currentRow, 1).Style.Font.Bold = true;
                        ws.Cell(currentRow, 1).Style.Font.FontSize = 13;
                        ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        currentRow += 2;

                        // Group data by line name
                        var groupedData = data.GroupBy(d => ((IDictionary<string, object>)d)["LineName"]);

                        foreach (var lineGroup in groupedData)
                        {
                            var lineName = lineGroup.Key.ToString();

                            // === Line Header ===
                            ws.Cell(currentRow, 1).Value = $"Line: {lineName}";
                            ws.Range(currentRow, 1, currentRow, 3).Merge();
                            ws.Cell(currentRow, 1).Style.Font.Bold = true;
                            ws.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                            currentRow++;

                            // === Table Header ===
                            ws.Cell(currentRow, 1).Value = "Category";
                            ws.Cell(currentRow, 2).Value = "Head Count";
                            ws.Cell(currentRow, 3).Value = "Month Days";
                            ws.Range(currentRow, 1, currentRow, 3).Style.Font.Bold = true;
                            ws.Range(currentRow, 1, currentRow, 3).Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                            currentRow++;

                            // === Data Rows ===
                            foreach (var item in lineGroup)
                            {
                                var dict = (IDictionary<string, object>)item;
                                ws.Cell(currentRow, 1).Value = dict["KPISubCategoryFName"];
                                ws.Cell(currentRow, 2).Value = dict["HeadCount"];
                                ws.Cell(currentRow, 3).Value = dict["MonthDays"];
                                currentRow++;
                            }

                            currentRow++; // extra blank line between lines
                        }

                        // === Styling ===
                        ws.Columns().AdjustToContents();
                        ws.RangeUsed().Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                        ws.RangeUsed().Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
                        ws.SheetView.FreezeRows(7); // optional

                        // === Save and Return ===
                        using (var stream = new MemoryStream())
                        {
                            workbook.SaveAs(stream);
                            stream.Position = 0;
                            return File(stream.ToArray(),"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",$"HC_MDMapping_{Date:ddMMyyyy}.xlsx");
                        }
                    }
              //  }
              //  return RedirectToAction("Reports_ManpowerAllocation_LineManpowerHCMDMappingByHR");
               // return Json(false, JsonRequestBehavior.AllowGet);
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