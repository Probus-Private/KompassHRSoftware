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
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_Onboarding
{
    public class Reports_Onboarding_EmployeeAccessPolicyController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Reports/Reports_Onboarding_EmployeeAccessPolicy
        #region Reports_Onboarding_EmployeeAccessPolicy
        public ActionResult Reports_Onboarding_EmployeeAccessPolicy(DailyAttendanceReportFilter MasReport)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 387;
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
                return View(MasReport);
                //if (MasReport.BranchId != null)
                //{
                //    DynamicParameters paramRPT = new DynamicParameters();
                //    paramRPT.Add("@P_BranchId", MasReport.BranchId);
                //    ViewBag.EmployeeAccessPolicy = DapperORM.ReturnList<AllDropDownBind>("sp_Rpt_Onboarding_EmployeeAccessPolicy", paramRPT).ToList();
                //}
                //else
                //{
                //    ViewBag.EmployeeAccessPolicy = "";
                //}
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
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId/*, DateTime FromDate, int? SubUnitId, int? DepartmentId, int? SubDepartmentId, int? DesignationId, int? LineId, int? EmployeeID, int? GradeId, DateTime ToDate*/)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("EmployeeAccessPolicy");
                worksheet.Range(1, 1, 1, 23).Merge();
                worksheet.SheetView.FreezeRows(2); // Freeze the row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();

                var Query = "";
                if (CmpId != null)
                {
                    Query = " and Mas_Employee.CmpID=" + CmpId + "";
                    if (BranchId != null)
                    {
                        Query = Query + " and Mas_Employee.EmployeeBranchId=" + BranchId + "";
                    }
                    else
                    {
                        Query = " and Mas_Employee.EmployeeBranchId in (select Mas_Branch.BranchID as Id from UserBranchMapping,Mas_Branch where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + CmpId + " and UserBranchMapping.IsActive = 1)";
                    }

                    //if (DepartmentId != null)
                    //{
                    //    Query = Query + " and Mas_Department.DepartmentId=" + DepartmentId + "";
                    //}

                    //if (DesignationId != null)
                    //{
                    //    Query = Query + " and Mas_Designation.DesignationId=" + DesignationId + "";
                    //}

                    //if (GradeId != null)
                    //{
                    //    Query = Query + " and Mas_Grade.GradeId=" + GradeId + "";
                    //}

                    //if (LineId != null)
                    //{
                    //    Query = Query + " and Mas_LineMaster.LineId=" + LineId + "";
                    //}
                    //if (EmployeeID != null)
                    //{
                    //    Query = Query + " and Mas_Employee.EmployeeID=" + EmployeeID + "";
                    //}

                }
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@P_Qry", Query);
                var GetLastDayReport = DapperORM.ReturnList<dynamic>("sp_Rpt_Onboarding_EmployeeAccessPolicy", paramList).ToList();
                if (GetLastDayReport.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }
                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(GetLastDayReport);
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
                var FromDate = DateTime.Now.Date;
                worksheet.Cell(1, 1).Value = "Employee Access Policy Report";
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Columns().AdjustToContents(); // This code for all clomns
                                                        // Set the header row background color to grey and font color to black
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