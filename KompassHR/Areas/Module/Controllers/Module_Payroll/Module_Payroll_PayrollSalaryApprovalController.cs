using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Module.Models.Module_Payroll;
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

namespace KompassHR.Areas.Module.Controllers.Module_Payroll
{
    public class Module_Payroll_PayrollSalaryApprovalController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
    
        // GET: Module/ Module_Payroll_PayrollSalaryApproval
        #region  Module_Payroll_PayrollSalaryApproval
        public ActionResult  Module_Payroll_PayrollSalaryApproval(SalaryMonthlyAttendance SalaryMonthlyAttendance)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 746;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Approve/Reject";

                var results = DapperORM.DynamicQueryMultiple(
                "SELECT DISTINCT CompanyId AS Id, CompanyName AS Name FROM Mas_CompanyProfile INNER JOIN Payroll_MakerChecker ON Payroll_MakerChecker.MakerCheckerCmpId = Mas_CompanyProfile.CompanyId WHERE Payroll_MakerChecker.Deactivate = 0 AND Mas_CompanyProfile.Deactivate = 0 " +
                "AND Payroll_MakerChecker.PayrollCheckerEmpId = '" + Session["EmployeeId"] + "'");

                var Companylist = results[0].Select(x => new AllDropDownBind{Id = Convert.ToDouble(x.Id),Name = (string)x.Name}).ToList();
                ViewBag.GetCompanyName = Companylist;
                var CompanyId = Companylist.FirstOrDefault()?.Id;
                if(CompanyId == null)
                {
                    CompanyId = 0;
                }

                var branch = DapperORM.DynamicQueryMultiple("SELECT DISTINCT BranchId AS Id, BranchName AS Name FROM Mas_Branch " +
                 "INNER JOIN Payroll_MakerChecker ON Payroll_MakerChecker.MakerCheckerBranchId = Mas_Branch.BranchId " +
                 "WHERE Payroll_MakerChecker.Deactivate = 0 " +
                 "AND Mas_Branch.Deactivate = 0 " +
                 "AND Payroll_MakerChecker.PayrollCheckerEmpId = '" + Session["EmployeeId"] + "' " +
                 "AND Payroll_MakerChecker.MakerCheckerCmpId = '" + CompanyId + "'");
                ViewBag.GetBranchName = branch[0].Select(x => new AllDropDownBind{Id = Convert.ToDouble(x.Id),Name = (string)x.Name}).ToList();

                var result1 = DapperORM.DynamicQueryMultiple("Select ProcessCategoryId as Id,ProcessCategoryName as Name from Payroll_ProcessCategory where Deactivate = 0 order by isdefault desc");
                var SalaryProcessList = result1[0].Select(x => new AllDropDownBind { Id = Convert.ToDouble(x.Id), Name = (string)x.Name }).ToList();
                ViewBag.SalaryProcess = SalaryProcessList;

                ViewBag.PayrollSalaryRequest = null;
                if (SalaryMonthlyAttendance.SalaryMonthYear != DateTime.MinValue)
                {
                    param.Add("@p_CheckerEmployeeId", Session["EmployeeId"]);
                    param.Add("@p_PayrollMonthYear", SalaryMonthlyAttendance.SalaryMonthYear);
                    param.Add("@p_CmpId", SalaryMonthlyAttendance.SalaryCmpId);
                    param.Add("@p_BranchId", SalaryMonthlyAttendance.SalaryBranchId);
                    param.Add("@p_ProcessCategoryId", SalaryMonthlyAttendance.SalaryProcessCategoryId);
                    var data = DapperORM.ReturnList<dynamic>("sp_Payroll_SalarySheet_List_ForApproval_Checker", param).ToList();
                    ViewBag.PayrollSalaryRequest = data;
                }
                return View(SalaryMonthlyAttendance);
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

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "SELECT DISTINCT BranchId AS Id, BranchName AS Name FROM Mas_Branch " +
                 "INNER JOIN Payroll_MakerChecker ON Payroll_MakerChecker.MakerCheckerBranchId = Mas_Branch.BranchId " +
                 "WHERE Payroll_MakerChecker.Deactivate = 0 " +
                 "AND Mas_Branch.Deactivate = 0 " +
                 "AND Payroll_MakerChecker.PayrollCheckerEmpId = '" + Session["EmployeeId"] + "' " +
                 "AND Payroll_MakerChecker.MakerCheckerCmpId = '" + CmpId + "'");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region  Multuple Multiple Approve Reject Request function
        [HttpPost]
        public ActionResult MultipleApproveRejectRequest(ApproveRejectPayrollSalaryRequestViewModel model,  DateTime Month,int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 745;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

               
                DataTable tbl_TypePayroll_Checker_ApprovedReject = new DataTable();
                tbl_TypePayroll_Checker_ApprovedReject.Columns.Add("SalaryID", typeof(double));
                tbl_TypePayroll_Checker_ApprovedReject.Columns.Add("SalaryEmployeeId", typeof(double));
                tbl_TypePayroll_Checker_ApprovedReject.Columns.Add("SalaryMonthYear", typeof(DateTime));
                tbl_TypePayroll_Checker_ApprovedReject.Columns.Add("Remark", typeof(string));
                tbl_TypePayroll_Checker_ApprovedReject.Columns.Add("Status", typeof(string));
                tbl_TypePayroll_Checker_ApprovedReject.Columns.Add("SalaryCmpId", typeof(double));
                tbl_TypePayroll_Checker_ApprovedReject.Columns.Add("SalaryBranchId", typeof(double));
              

                foreach (var item in model.ObjPayrollsalary)
                {
                    var PayrollCount = DapperORM.DynamicQuerySingle("Select Count(PayrollLockId) as LockCount from Payroll_LOck where Deactivate=0 and PayrollLockBranchID=" + item.SalaryBranchId + " and Month(PayrollLockMonthYear)='" + Month.ToString("MM") + "'  and Year(PayrollLockMonthYear) ='" + Month.ToString("yyyy") + "' and Status=1");
                    if (PayrollCount.LockCount != 0)
                    {
                        TempData["Message"] = "The record can't be saved because the payroll for this month ('" + Month.ToString("MMM-yyyy") + "') is locked.";
                        TempData["Icon"] = "error";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }

                    DataRow dr = tbl_TypePayroll_Checker_ApprovedReject.NewRow();
                    dr["SalaryID"] = Convert.ToDouble(item.SalaryID);
                    dr["SalaryEmployeeId"] = Convert.ToDouble(item.SalaryEmployeeId);
                    dr["SalaryMonthYear"] = Convert.ToDateTime(item.SalaryMonthYear);
                    dr["Remark"] = Convert.ToString(item.Remark ?? "");
                    dr["Status"] = Convert.ToString(item.Status ?? "");
                    dr["SalaryCmpId"] = Convert.ToDouble(item.SalaryCmpId);
                    dr["SalaryBranchId"] = Convert.ToDouble(item.SalaryBranchId);
                   

                    tbl_TypePayroll_Checker_ApprovedReject.Rows.Add(dr);
                }

                DynamicParameters ARparam = new DynamicParameters();
                //ARparam.Add("@p_AttenMonthlyMonthYear", Month);
                ARparam.Add("@p_PayrollMonthYear", Month);
                ARparam.Add("@p_PayrollCheckerEmployeeId", Session["EmployeeId"]);
                ARparam.Add("@p_CmpID", CmpId);
                ARparam.Add("@tbl_TypePayroll_Checker_ApprovedReject", tbl_TypePayroll_Checker_ApprovedReject.AsTableValuedParameter("tbl_TypePayroll_Checker_ApprovedReject"));
                ARparam.Add("@p_CreatedupdateBy", Session["EmployeeName"]);
                ARparam.Add("@p_MachineName", Dns.GetHostName().ToString());
                ARparam.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                ARparam.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter  
                var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Checker_ApprovedReject", ARparam).ToList();
                TempData["Message"] = ARparam.Get<string>("@p_msg");
                TempData["Icon"] = ARparam.Get<string>("@p_Icon");
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"], Month = Month }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region DownloadExcelFile
        public ActionResult DownloadMonthlyAttendnceExcelFile(int? CmpId, int? BranchId, DateTime MonthYear, int? ProcessCategoryId)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("PayrollSalaryApproval");
                worksheet.Range(1, 1, 1, 10).Merge();
                worksheet.SheetView.FreezeRows(2); // Freeze the first row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();

                // Initialize Dapper parameters
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_MonthYear", MonthYear);
                param.Add("@p_BranchId", BranchId);
                param.Add("@p_CompanyId", CmpId);
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_ProcessCategoryId", ProcessCategoryId);
                var GetData = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Payroll_WageSheet", param).ToList();
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
                worksheet.Cell(1, 1).Value = "Payroll Salary Approval Sheet - (" + MonthYear.ToString("dd/MMM/yyyy") + ")";
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Columns().AdjustToContents(); // This code for all clomns
               // worksheet.Columns(1, 5).Hide();

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

        #region GetApproveRejectList
        public ActionResult GetApproveRejectList(int? SalaryBranchId, string SalaryMonthYear, int? SalaryProcessCategoryId)
        {
            try
            {  //PinCode
                Session["SalaryMonthYear"] = SalaryMonthYear;
                param.Add("@p_Origin", "ApproveRejectList");
                param.Add("@p_BranchId", SalaryBranchId);
                param.Add("@p_MonthYear", DateTime.Parse(SalaryMonthYear));
                //param.Add("@p_status", status);
                param.Add("@p_SalaryProcessCategoryId", SalaryProcessCategoryId);
                var data = DapperORM.ReturnList<dynamic>("sp_List_Payroll_Salary", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
        #region UpdatePayrollApproval
        public ActionResult UpdatePayrollApproval(DeleteSalaryPayrollProcessViewModel model,DateTime SalaryMonthYear, string SalaryCheckerStatus)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                StringBuilder strBuilder = new StringBuilder();
                string Query = "";

                foreach (var item in model.ObjPayrollProcess)
                {
                    Query = "UPDATE dbo.payroll_Salary SET " +
                            "SalaryCheckerStatus = '" + SalaryCheckerStatus + "', " +
                            "SalaryCheckerApprovedDate = NULL, " +
                            "SalaryCheckerApprovedBy_EmployeeId = NULL, " +
                            "SalaryCheckerRemark = NULL" +
                            " WHERE SalaryMonthYear = '" + SalaryMonthYear + "' " +
                            "AND SalaryEmployeeId = '" + item.SalaryEmployeeId + "' " +
                            "AND SalaryID = '" + item.SalaryID + "'; ";

                    strBuilder.Append(Query);
                }

                string abc = "";
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    TempData["Message"] = "Record save successfully";
                    TempData["Icon"] = "success";
                }

                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Icon = "error" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

    }
}