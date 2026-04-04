using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Module.Models.Module_Payroll;
using KompassHR.Areas.Setting.Models.Setting_Leave;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Payroll
{
    public class Module_Payroll_PayrollLockController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Module/Module_Payroll_PayrollLock
        #region PayrollLock MAin View
        [HttpGet]
        public ActionResult Module_Payroll_PayrollLock(string PayrollLockId_Encrypted, int? CmpId)
        {
            try
            {
                ViewBag.AddUpdateTitle = "Add";
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 166;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                //Payroll_Lock PayrollLock = new Payroll_Lock();
                //DynamicParameters paramCompany = new DynamicParameters();
                //paramCompany.Add("@query", "Select  CompanyId As Id, CompanyName As Name from Mas_CompanyProfile where Deactivate=0 order by Name");
                //var listCompanyName = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", paramCompany).ToList();
                //ViewBag.GetCompanyName = listCompanyName;

                //GET COMPANY NAME
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                var CMPID = GetComapnyName[0].Id;
                //GET BRANCH NAME
                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CMPID), Convert.ToInt32(Session["EmployeeId"]));
                ViewBag.BranchName = Branch;


                Payroll_Lock PayrollLock = new Payroll_Lock();
                if (PayrollLockId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_PayrollLockId_Encrypted", PayrollLockId_Encrypted);
                    PayrollLock = DapperORM.ReturnList<Payroll_Lock>("sp_List_Payroll_Lock", param).FirstOrDefault();
                    TempData["PayrollLockMonthYear"] = PayrollLock.PayrollLockMonthYear.ToString("yyyy-MM");
                }

                //if (CmpId != null)
                //{
                //    DynamicParameters paramName = new DynamicParameters();
                //    paramName.Add("@query", "Select  BranchId as Id, BranchName as Name from Mas_Branch where Deactivate=0 and CmpId= '" + CmpId + "'");
                //    var GetBranch = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramName).ToList();
                //    ViewBag.GetBranchList = GetBranch;
                //}
                //else
                //{
                //    ViewBag.GetBranchList = "";
                //}
                return View(PayrollLock);

            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "Module_Payroll_PayrollLock ");
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

        //#region GetBusinessUnit
        //[HttpGet]
        //public ActionResult GetBusinessUnit(int CmpId)
        //{
        //    DynamicParameters param = new DynamicParameters();
        //    param.Add("@query", "Select  BranchId, BranchName from Mas_Branch where Deactivate=0 and CmpId= '" + CmpId + "'");
        //    var data = DapperORM.ReturnList<Mas_Branch>("sp_QueryExcution", param).ToList();
        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}
        //#endregion

        #region ISValidation
        [HttpGet]
        public JsonResult IsPayrollLockExists(bool Status, string PayrollLockMonthYear, string PayrollLockBranchID, string PayrollLockId_Encrypted)
        {
            try
            {

                param.Add("@p_process", "IsValidation");
                param.Add("@@p_PayrollLockBranchID", PayrollLockBranchID);
                param.Add("@p_Status", Status);
                param.Add("@p_PayrollLockMonthYear", DateTime.Parse(PayrollLockMonthYear));
                param.Add("@p_PayrollLockId_Encrypted", PayrollLockId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_Lock", param);
                var Message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                if (Message != "")
                {
                    return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception Ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Payroll_Lock Payroll)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(Payroll.PayrollLockId_Encrypted) ? "Save" : "Update");
                param.Add("@p_PayrollLockId", Payroll.PayrollLockId);
                param.Add("@p_PayrollLockId_Encrypted", Payroll.PayrollLockId_Encrypted);
                param.Add("@p_CmpId", Payroll.CmpId);
                param.Add("@p_PayrollLockBranchID", Payroll.PayrollLockBranchID);
                param.Add("@p_PayrollLockMonthYear", Payroll.PayrollLockMonthYear);
                param.Add("@p_Status", Payroll.Status);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_Status", Payroll.Status);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_Lock", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Payroll_PayrollLock", "Module_Payroll_PayrollLock");
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "Module_Payroll_PayrollLock");
            }
        }
        #endregion

        #region GetList View
        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 166;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_PayrollLockId_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Payroll_Lock", param);
                ViewBag.GetPayrollLockList = data;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "Module_Payroll_PayrollLock");
            }
        }

        #endregion

        #region Delete
        [HttpGet]
        public ActionResult Delete(string PayrollLockId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_PayrollLockId_Encrypted", PayrollLockId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_Lock", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Module_Payroll_PayrollLock");
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "Module_Payroll_PayrollLock");
            }
        }
        #endregion

        #region GetPayrollSalarydetails
        public ActionResult GetPayrollSalarydetails(int? CmpId, int? PayrollLockBranchID, DateTime PayrollLockMonthYear)
        {
            try
            {
                param.Add("@p_PayrollLockId_Encrypted", "PayrollSalarydetails");
                param.Add("@p_CmpId", CmpId);
                param.Add("@p_BranchId", PayrollLockBranchID);
                param.Add("@p_MonthYear", PayrollLockMonthYear);

                var data = DapperORM.ReturnList<dynamic>("sp_List_Payroll_Lock", param).ToList();
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
        public ActionResult DownloadExcelFile(int? CmpId, int PayrollLockBranchID, DateTime PayrollLockMonthYear)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Salary Process Report");
                worksheet.Range(1, 1, 1, 9).Merge();  //Merge First Row with 60 Clomn
                worksheet.SheetView.FreezeRows(2); // Freeze the row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();

                param.Add("@p_PayrollLockId_Encrypted", "PayrollSalarydetails");
                param.Add("@p_CmpId", CmpId);
                param.Add("@p_BranchId", PayrollLockBranchID);
                param.Add("@p_MonthYear", PayrollLockMonthYear);

                data = DapperORM.ExecuteSP<dynamic>("sp_List_Payroll_Lock", param).ToList();
                if (data.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }
                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(data);
                worksheet.Cell(2, 1).InsertTable(dt, false);
                int totalRows = worksheet.RowsUsed().Count();

                var lastRow = worksheet.Row(totalRows + 1);
                lastRow.Style.Font.Bold = true;
                // Additional styling if required
                lastRow.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
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
              //  var SetMonth = DateTime.Parse(MonthYear).ToString("MMM-yyyy");
                worksheet.Cell(1, 1).Value = "Payroll Salary Details ";
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
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
    }
}