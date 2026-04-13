using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Recruitment;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Recruitment
{
    public class ESS_Recruitment_AttritionReportsController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_Recruitment_AttritionReports
        #region AttritionReports Main VIew
        public ActionResult ESS_Recruitment_AttritionReports(AttritionReport Report)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 257;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@query", "select CompanyId AS Id,CompanyName As Name from Mas_CompanyProfile where Deactivate=0 order by CompanyName");
                var GetComapnyProfile = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.ComapnyProfile = GetComapnyProfile;
                                
                if (Report.CmpId != null)
                {
                    if(Report.BranchId != null)
                    {
                        DynamicParameters paramList = new DynamicParameters();
                        paramList.Add("@p_BranchId", Report.BranchId);
                        paramList.Add("@p_Year", Report.Year);
                        paramList.Add("@p_CompanyID", Report.CmpId);
                        ViewBag.ManpowerAttritionList = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Recruitment_ManpowerAttrition1", paramList).ToList();
                    }
                    //else
                    //{
                    //    DynamicParameters paramList = new DynamicParameters();
                    //    paramList.Add("@p_BranchId", "");
                    //    paramList.Add("@p_Year", Report.Year);
                    //    ViewBag.ManpowerAttritionList = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Recruitment_ManpowerAttrition1", paramList).ToList();
                    //    ViewBag.GetMasBranch = "";
                    //}
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_employeeid", Session["EmployeeId"]);
                    param.Add("@p_CmpId", Report.CmpId);
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                    ViewBag.GetMasBranch = data;

                }
                else
                {
                    ViewBag.ManpowerAttritionList = "";
                    ViewBag.GetMasBranch = "";
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


        [HttpGet]
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, int? Year)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("AttritionReport");
                worksheet.Range(1, 1, 1, 7).Merge(); // Merge 7 columns to match DataTable
                worksheet.SheetView.FreezeRows(2); // Freeze the header row
                DataTable dt = new DataTable();

                // Define DataTable columns to match query and view
                dt.Columns.Add("CompanyName", typeof(string));
                dt.Columns.Add("BranchName", typeof(string));
                dt.Columns.Add("MonthYear", typeof(string));
                dt.Columns.Add("NewJoinee", typeof(int));
                dt.Columns.Add("Left Employee", typeof(int));
                dt.Columns.Add("Start Month Active Employee", typeof(int));
                dt.Columns.Add("End Month Active Employee", typeof(int));
                dt.Columns.Add("Attrition(%)", typeof(float));

                // Fetch data using Dapper
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_CompanyID", CmpId);
                paramList.Add("@p_BranchID", BranchId);
                paramList.Add("@p_Year", Year);

                var attritionData = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Recruitment_ManpowerAttrition1", paramList).ToList();

                if (attritionData.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }

                // Map data to DataTable
                foreach (var item in attritionData)
                {
                    var dict = (IDictionary<string, object>)item;
                    var row = dt.NewRow();
                    row["CompanyName"] = dict["CompanyName"]?.ToString();
                    row["BranchName"] = dict["BranchName"]?.ToString();
                    row["MonthYear"] = $"{dict["MonthName"]} {dict["Year"]}";
                    row["NewJoinee"] = Convert.ToInt32(dict["NewJoineeCount"] ?? 0);
                    row["Left Employee"] = Convert.ToInt32(dict["EmployeeLeftCount"] ?? 0);
                    // Calculate Opening: TotalEmployeeOfThisMonth - NewJoineeCount + EmployeeLeftCount
                    row["Start Month Active Employee"] = Convert.ToInt32(dict["ActiveEmployee"] ?? 0);
                    row["End Month Active Employee"] = Convert.ToInt32(dict["TotalEmployeeOfThisMonth"] ?? 0);
                    row["Attrition(%)"] = Math.Round(Convert.ToDouble(dict["EmployeeAttritionPercentage"] ?? 0), 2);
                    dt.Rows.Add(row);
                }

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
                worksheet.Cell(1, 1).Value = $"Attrition Report - {Year}";
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
                    stream.Position = 0; // Reset the stream position to the beginning
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"AttritionReport_{Year}.xlsx");
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

    }
}