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
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Recruitment
{
    public class ESS_Recruitment_CandidateDatabaseReportsController : Controller
    {
        // GET: ESS/ESS_Recruitment_CandidateDatabaseReports
        #region ESS_Recruitment_CandidateDatabaseReports
        public ActionResult ESS_Recruitment_CandidateDatabaseReports(DailyAttendanceReportFilter MasReport)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 295;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                //DynamicParameters param = new DynamicParameters();
                //param.Add("@p_FromDate", candidatedatabase.FromDate);
                //param.Add("@p_ToDate", candidatedatabase.ToDate);
                //var GetCandidateData = DapperORM.ReturnList<dynamic>("sp_Rpt_Recruitment_CandidateDatabase", param).ToList();
                //ViewBag.GetCandidateReport = GetCandidateData;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(DateTime FromDate, DateTime ToDate)
        {
            try
            {
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("OfferedLetterDataReport");
                worksheet.Range(1, 1, 1, 21).Merge();
                worksheet.SheetView.FreezeRows(2); // Freeze the row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();

                //var Query = "";
                //if (CmpId != null)
                //{
                //    Query = " and Mas_Employee.CmpID=" + CmpId + "";
                //    if (BranchId != null)
                //    {
                //        Query = Query + " and Mas_Branch.BranchId=" + BranchId + "";
                //    }
                //    else
                //    {
                //        Query = Query + " and mas_branch.BranchId in (select UserBranchMapping.BranchID as Id from UserBranchMapping where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + "  and UserBranchMapping.CmpID = " + CmpId + " and UserBranchMapping.IsActive = 1 union select EmployeeBranchId as Id  from Mas_Employee where  EmployeeId= " + Session["EmployeeId"] + "  and Mas_Employee.CmpID= " + CmpId + " )";
                //    }
                //    if (DepartmentId != null)
                //    {
                //        Query = Query + " and Mas_Department.DepartmentId=" + DepartmentId + "";
                //    }
                //    if (DesignationId != null)
                //    {
                //        Query = Query + " and Mas_Designation.DesignationId=" + DesignationId + "";
                //    }
                //    var GetFromdate = FromDate.ToString("yyyy-MM-dd");
                //    var GetTodate = ToDate.ToString("yyyy-MM-dd");
                //    if (DesignationId != null)
                //    {
                //        Query = Query + " and Mas_Designation.DesignationId=" + DesignationId + "";
                //    }
                //    if (FromDate != null)
                //    {
                //        Query = Query + " and CONVERT(date, Recruitment_OfferLetterGeneration.CreatedDate) BETWEEN  '" + GetFromdate + "' AND '" + GetTodate + "'";
                //    }
                //    //if (SubDepartmentId != null)
                //    //{
                //    //    Query = Query + " and Mas_SubDepartment.SubDepartmentId=" + SubDepartmentId + "";
                //    //}

                //    //if (GradeId != null)
                //    //{
                //    //    Query = Query + " and Mas_Grade.GradeId=" + GradeId + "";
                //    //}
                //    ////if (LineId != null)
                //    ////{
                //    ////    Query = Query + " and Mas_LineMaster.LineId=" + LineId + "";
                //    ////}
                //    //if (EmployeeID != null)
                //    //{
                //    //    Query = Query + " and Recruitment_Interview.InterviewResourceEmployeeId=" + EmployeeID + "";
                //    //}
                //    //if (SubUnitId != null)
                //    //{
                //    //    Query = Query + " and Mas_Unit.UnitId=" + SubUnitId + "";
                //    //}
                //}
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_FromDate", FromDate.ToString("yyyy-MM-dd"));
                paramList.Add("@p_ToDate", ToDate.ToString("yyyy-MM-dd"));
                //paramList.Add("@P_Qry", Query);
                data = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Recruitment_CandidateDatabase", paramList).ToList();
                if (data.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }

                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(data);
                //dt.Rows.RemoveAt(0);
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
                worksheet.Cell(1, 1).Value = "Offered Letter Data Report - (" + FromDate.ToString("dd/MMM/yyyy") + " - " + ToDate.ToString("dd/MMM/yyyy") + ")";
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
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DailyAttendance.xlsx");
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