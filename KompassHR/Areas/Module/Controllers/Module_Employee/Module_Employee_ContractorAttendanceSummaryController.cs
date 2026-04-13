using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Reports.Models;
using System.Data.SqlClient;
using ClosedXML.Excel;
using System.IO;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_ContractorAttendanceSummaryController : Controller
    {

        // GET: Module/Module_Payroll_MonthlyAttendance
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();

        #region Main View
        // GET: Module/Module_Employee_ContractorAttendanceSummary
        public ActionResult Module_Employee_ContractorAttendanceSummary(int? CmpId, int? BranchId, int? ContractorId, DateTime? InvoiceMonth)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 928;
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

                var CmpID = GetComapnyName[0].Id;
                ViewBag.BranchName = "";
                ViewBag.ContractorName = "";

                if (InvoiceMonth != null && CmpId != null && BranchId != null)
                {
                    var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CmpId), Convert.ToInt32(Session["EmployeeId"]));
                    ViewBag.BranchName = Branch;

                    DynamicParameters paramContractorName = new DynamicParameters();
                    paramContractorName.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + BranchId + "");
                    ViewBag.ContractorName = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", paramContractorName).ToList();

                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_MonthYear", InvoiceMonth);
                    param1.Add("@p_CmpId", CmpId);
                    param1.Add("@p_BranchId", BranchId);
                    param1.Add("@p_ContractorId", ContractorId);
                    var data = DapperORM.ExecuteSP<dynamic>("SP_Invoice_Conctrator_Attendance_Show", param1).ToList();

                    int hideCount = 0;
                    if (data != null && data.Count > 0)
                    {
                        var hideValue = ((IDictionary<string, object>)data[0])["Hide"];

                        if (hideValue != null && hideValue is int)
                        {
                            hideCount = (int)hideValue;
                        }
                        else if (hideValue != null)
                        {
                            int.TryParse(hideValue.ToString(), out hideCount);
                        }
                        ViewBag.HideCount = hideCount;
                        ViewBag.AttendanceSummary = data;
                    }
                    else
                    {
                        ViewBag.AttendanceSummary = new List<dynamic>();
                        TempData["Message"] = "Record Not Found";
                        TempData["Icon"] = "error";
                        return View();
                    }
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



        #region SaveUpdate
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveUpdate(SaveTypeContractorAttendance_summaryViewModel model, int CmpId, int? BranchId, int? ContractorId, DateTime? InvoiceMonth)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DataTable tbl_TypeContractorAttendance_summary = new DataTable();
                tbl_TypeContractorAttendance_summary.Columns.Add("ContractorMonthlyCmpId", typeof(double));
                tbl_TypeContractorAttendance_summary.Columns.Add("ContractorMonthlyBranchId", typeof(double));
                tbl_TypeContractorAttendance_summary.Columns.Add("ContractorMonthlyContractorId", typeof(double));
                tbl_TypeContractorAttendance_summary.Columns.Add("ContractorMonthlyEmployeeId", typeof(double));
                tbl_TypeContractorAttendance_summary.Columns.Add("ContractorMonthlyDepartmentId", typeof(double));
                tbl_TypeContractorAttendance_summary.Columns.Add("ContractorMonthlyDesignationId", typeof(double));
                tbl_TypeContractorAttendance_summary.Columns.Add("EmployeeName", typeof(string));
                tbl_TypeContractorAttendance_summary.Columns.Add("EmployeeNo", typeof(string));
                tbl_TypeContractorAttendance_summary.Columns.Add("Department", typeof(string));
                tbl_TypeContractorAttendance_summary.Columns.Add("Designation", typeof(string));
                tbl_TypeContractorAttendance_summary.Columns.Add("Contractor", typeof(string));
                tbl_TypeContractorAttendance_summary.Columns.Add("SubUnit", typeof(string));
                tbl_TypeContractorAttendance_summary.Columns.Add("ManpowerCategory", typeof(string));
                tbl_TypeContractorAttendance_summary.Columns.Add("DailyMonthly", typeof(string));
                tbl_TypeContractorAttendance_summary.Columns.Add("ShortHrApplicable", typeof(string));
                tbl_TypeContractorAttendance_summary.Columns.Add("ShortHrApplicableMin", typeof(string));
                tbl_TypeContractorAttendance_summary.Columns.Add("AB", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("PP", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("WO", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("PH", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("CO", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("CL", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("SL", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("EL", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("Other1", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("Other2", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("Other3", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("PayableDays", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("OThrs", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("LOPHrs", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("InvoiceProcess", typeof(bool));
                tbl_TypeContractorAttendance_summary.Columns.Add("LateCount", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("LateHrs", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("EarlyCount", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("EarlyHrs", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("InOut_ShortHrs", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("LOPDays", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("Rate_PerDay", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("Rate_Canteen", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("Rate_AttendanceBonus", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("Rate_TransportAllow", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("Rate_Other1", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("Rate_Other2", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("TotalRate_PerDay", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("TotalRate_Canteen", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("TotalRate_AttendanceBonus", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("TotalRate_TransportAllow", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("TotalRate_Other1", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("TotalRate_Other2", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("TotalOTAmount", typeof(float));
                tbl_TypeContractorAttendance_summary.Columns.Add("TotalAmount", typeof(float));
                foreach (var item in model.ObjContractorAttendanceSummary)
                {
                    DataRow dr = tbl_TypeContractorAttendance_summary.NewRow();
                    dr["ContractorMonthlyCmpId"] = Convert.ToDouble(item.ContractorMonthlyCmpId ?? "0");
                    dr["ContractorMonthlyBranchId"] = Convert.ToDouble(item.ContractorMonthlyBranchId ?? "0");
                    dr["ContractorMonthlyContractorId"] = Convert.ToDouble(item.ContractorMonthlyContractorId ?? "0");
                    dr["ContractorMonthlyEmployeeId"] = Convert.ToDouble(item.ContractorMonthlyEmployeeId ?? "0");
                    dr["ContractorMonthlyDepartmentId"] = Convert.ToDouble(item.ContractorMonthlyDepartmentId ?? "0");
                    dr["ContractorMonthlyDesignationId"] = Convert.ToDouble(item.ContractorMonthlyDesignationId ?? "0");
                    dr["EmployeeName"] = item.EmployeeName ?? "";
                    dr["EmployeeNo"] = item.EmployeeNo ?? "";
                    dr["Department"] = item.Department ?? "";
                    dr["Designation"] = item.Designation ?? "";
                    dr["Contractor"] = item.Contractor ?? "";
                    dr["SubUnit"] = item.SubUnit ?? "";
                    dr["ManpowerCategory"] = item.ManpowerCategory ?? "";
                    dr["DailyMonthly"] = item.DailyMonthly ?? "";
                    dr["ShortHrApplicable"] = Convert.ToDouble(item.ShortHrApplicable ?? "0");
                    dr["ShortHrApplicableMin"] = Convert.ToDouble(item.ShortHrApplicableMin ?? "0");
                    dr["AB"] = float.Parse(item.AB ?? "0");
                    dr["PP"] = float.Parse(item.PP ?? "0");
                    dr["WO"] = float.Parse(item.WO ?? "0");
                    dr["PH"] = float.Parse(item.PH ?? "0");
                    dr["CO"] = float.Parse(item.CO ?? "0");
                    dr["CL"] = float.Parse(item.CL ?? "0");
                    dr["SL"] = float.Parse(item.SL ?? "0");
                    dr["EL"] = float.Parse(item.EL ?? "0");
                    dr["Other1"] = float.Parse(item.Other1 ?? "0");
                    dr["Other2"] = float.Parse(item.Other2 ?? "0");
                    dr["Other3"] = float.Parse(item.Other3 ?? "0");
                    dr["PayableDays"] = float.Parse(item.PayableDays ?? "0");
                    dr["OThrs"] = float.Parse(item.OThrs ?? "0");
                    dr["LOPHrs"] = float.Parse(item.LOPHrs ?? "0");
                    dr["InvoiceProcess"] = item.InvoiceProcess == "1" || item.InvoiceProcess?.ToLower() == "true";
                    dr["LateCount"] = float.Parse(item.LateCount ?? "0");
                    dr["LateHrs"] = float.Parse(item.LateHrs ?? "0");
                    dr["EarlyCount"] = float.Parse(item.EarlyCount ?? "0");
                    dr["EarlyHrs"] = float.Parse(item.EarlyHrs ?? "0");
                    dr["InOut_ShortHrs"] = float.Parse(item.InOut_ShortHrs ?? "0");
                    dr["LOPDays"] = float.Parse(item.LOPDays ?? "0");
                    dr["Rate_PerDay"] = float.Parse(item.Rate_PerDay ?? "0");
                    dr["Rate_Canteen"] = float.Parse(item.Rate_Canteen ?? "0");
                    dr["Rate_AttendanceBonus"] = float.Parse(item.Rate_AttendanceBonus ?? "0");
                    dr["Rate_TransportAllow"] = float.Parse(item.Rate_TransportAllow ?? "0");
                    dr["Rate_Other1"] = float.Parse(item.Rate_Other1 ?? "0");
                    dr["Rate_Other2"] = float.Parse(item.Rate_Other2 ?? "0");
                    dr["TotalRate_PerDay"] = float.Parse(item.TotalRate_PerDay ?? "0");
                    dr["TotalRate_Canteen"] = float.Parse(item.TotalRate_Canteen ?? "0");
                    dr["TotalRate_AttendanceBonus"] = float.Parse(item.TotalRate_AttendanceBonus ?? "0");
                    dr["TotalRate_TransportAllow"] = float.Parse(item.TotalRate_TransportAllow ?? "0");
                    dr["TotalRate_Other1"] = float.Parse(item.TotalRate_Other1 ?? "0");
                    dr["TotalRate_Other2"] = float.Parse(item.TotalRate_Other2 ?? "0");
                    dr["TotalOTAmount"] = float.Parse(item.TotalOTAmount ?? "0");
                    dr["TotalAmount"] = float.Parse(item.TotalAmount ?? "0");
                    tbl_TypeContractorAttendance_summary.Rows.Add(dr);
                }

                DynamicParameters ARparam = new DynamicParameters();
                ARparam.Add("@tbl_TypeContractorAttendance_summary", tbl_TypeContractorAttendance_summary.AsTableValuedParameter("tbl_TypeContractorAttendance_summary"));
                ARparam.Add("@p_MonthYear", InvoiceMonth);
                ARparam.Add("@p_CreatedupdateBy", Session["EmployeeName"]);
                ARparam.Add("@p_MachineName", Dns.GetHostName().ToString());
                ARparam.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                ARparam.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter  
                var GetData = DapperORM.ExecuteSP<dynamic>("sp_SUD_ContractorAttendance_Summary", ARparam).ToList();
                TempData["Message"] = ARparam.Get<string>("@p_msg");
                TempData["Icon"] = ARparam.Get<string>("@p_Icon");
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"], GetData }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Icon = "error" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region GetContractor
        [HttpGet]
        public ActionResult GetContractor(int BranchId, int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters paramContractorName = new DynamicParameters();
                paramContractorName.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + BranchId + "");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", paramContractorName).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
        [HttpGet]
        public ActionResult GetList(Contractor_AttendanceSummary OBJList)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 928;
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

                if ((OBJList.InvoiceMonth != DateTime.MinValue) && OBJList.CmpId > 0)
                {
                    var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(OBJList.CmpId), Convert.ToInt32(Session["EmployeeId"]));
                    ViewBag.BranchName = Branch;

                    param.Add("@p_Origin", "List");
                    param.Add("@p_CmpId", OBJList.CmpId);
                    param.Add("@p_BranchId", OBJList.BranchId);
                    param.Add("@p_MonthYear", OBJList.InvoiceMonth);
                    var data = DapperORM.ReturnList<dynamic>("sp_List_ContractorAttendanceSummary", param).ToList();
                    if (data.Count == 0)
                    {
                        TempData["Message"] = "Record Not Found";
                        TempData["Icon"] = "error";
                        ViewBag.GetContractorAttendance = "";
                        return View();
                    }
                    else
                    {
                        ViewBag.GetContractorAttendance = data;
                    }
                }
                else
                {
                    ViewBag.BranchName = "";
                    ViewBag.GetContractorAttendance = "";
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

        #region GetContactorAttendanceSummary
        public ActionResult GetContactorAttendanceSummary(int? ContractorMonthlyCmpId, int? ContractorMonthlyBranchId, int? ContractorMonthlyContractorId, DateTime? ContractorMonthlyMonthYear)
        {
            try
            {
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@P_Origin", "ContractorAttendance");
                param1.Add("@p_MonthYear", ContractorMonthlyMonthYear);
                param1.Add("@p_CmpId", ContractorMonthlyCmpId);
                param1.Add("@p_BranchId", ContractorMonthlyBranchId);
                param1.Add("@p_ContractorId", ContractorMonthlyContractorId);
                var data = DapperORM.ReturnList<dynamic>("sp_List_ContractorAttendanceSummary", param1).ToList();
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
        public ActionResult DownloadExcelFile(int? ContractorMonthlyCmpId, int? ContractorMonthlyBranchId, int? ContractorMonthlyContractorId, DateTime? ContractorMonthlyMonthYear)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("ContractorAttendance");
                worksheet.Range(1, 1, 1, 23).Merge();  //Merge First Row with 60 Clomn
                worksheet.SheetView.FreezeRows(2); // Freeze the row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@P_Origin", "Export");
                param1.Add("@p_MonthYear", ContractorMonthlyMonthYear);
                param1.Add("@p_CmpId", ContractorMonthlyCmpId);
                param1.Add("@p_BranchId", ContractorMonthlyBranchId);
                param1.Add("@p_ContractorId", ContractorMonthlyContractorId);
                data = DapperORM.ExecuteSP<dynamic>("sp_List_ContractorAttendanceSummary", param1).ToList();
                if (data.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }
                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(data);
                worksheet.Cell(2, 1).InsertTable(dt, false);

                var usedRange = worksheet.RangeUsed();
                usedRange.Style.Fill.BackgroundColor = XLColor.White;
                usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Font.FontSize = 10;
                usedRange.Style.Font.FontColor = XLColor.Black;

                // Set the header row name
                worksheet.Cell(1, 1).Value = "Contractor Attendance "; // Replace "Excel Name" with the actual name
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

        #region DeleteContractorAttendance

        public ActionResult DeleteContractorAttendance(DeleteContractorAttendanceViewModel model, DateTime Month)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DataTable tbl_Type_DeleteContractorAttendance_Delete = new DataTable();
                tbl_Type_DeleteContractorAttendance_Delete.Columns.Add("ContractorMonthlyId", typeof(double));
                tbl_Type_DeleteContractorAttendance_Delete.Columns.Add("ContractorMonthlyEmployeeId", typeof(double));
                tbl_Type_DeleteContractorAttendance_Delete.Columns.Add("ContractorMonthlyMonthYear", typeof(DateTime));

                foreach (var item in model.ObjContractorAttendance)
                {
                    DataRow dr = tbl_Type_DeleteContractorAttendance_Delete.NewRow();
                    dr["ContractorMonthlyId"] = Convert.ToDouble(item.ContractorMonthlyId ?? "0");
                    dr["ContractorMonthlyEmployeeId"] = Convert.ToDouble(item.ContractorMonthlyEmployeeId ?? "0");
                    dr["ContractorMonthlyMonthYear"] = Convert.ToDateTime(item.ContractorMonthlyMonthYear);
                    tbl_Type_DeleteContractorAttendance_Delete.Rows.Add(dr);
                }

                DynamicParameters ARparam = new DynamicParameters();
                ARparam.Add("@p_ContractorMonthlyMonthYear", Month);
                ARparam.Add("@tbl_Type_DeleteContractorAttendance_Delete", tbl_Type_DeleteContractorAttendance_Delete.AsTableValuedParameter("tbl_Type_DeleteContractorAttendance_Delete"));
                ARparam.Add("@p_CreatedupdateBy", Session["EmployeeName"]);
                ARparam.Add("@p_MachineName", Dns.GetHostName().ToString());
                ARparam.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                ARparam.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter  
                var GetData = DapperORM.ExecuteSP<dynamic>("sp_SUD_ContractorAttendance_Summary_Delete", ARparam).ToList();
                TempData["Message"] = ARparam.Get<string>("@p_msg");
                TempData["Icon"] = ARparam.Get<string>("@p_Icon");
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"], Month = Month }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Icon = "error" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion



        #region DownloadSummaryExcelFile
        public ActionResult DownloadSummaryExcelFile(int? CmpId, int? BranchId, int? ContractorId, DateTime InvoiceMonth)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("AttendanceSummary");

                // -------------------------
                // ✅ FETCH COMPANY NAME
                // -------------------------
                DynamicParameters paramComp = new DynamicParameters();
                paramComp.Add("@Query", "SELECT CompanyName FROM Mas_CompanyProfile WHERE CompanyId = '" + CmpId + "'");
                var compResult = DapperORM.ExecuteSP<dynamic>("Sp_QueryExcution", paramComp).FirstOrDefault();
                string companyName = compResult?.CompanyName ?? "";

                // -------------------------
                // ✅ FETCH BRANCH NAME
                // -------------------------
                DynamicParameters paramBranch = new DynamicParameters();
                paramBranch.Add("@Query", "SELECT BranchName FROM Mas_Branch WHERE BranchId = '" + BranchId + "'");
                var branchResult = DapperORM.ExecuteSP<dynamic>("Sp_QueryExcution", paramBranch).FirstOrDefault();
                string branchName = branchResult?.BranchName ?? "";

                // -------------------------
                // ✅ GET DATA
                // -------------------------
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_ContractorId", ContractorId);
                paramList.Add("@p_MonthYear", InvoiceMonth);
                paramList.Add("@p_CmpId", CmpId);
                paramList.Add("@p_BranchId", BranchId);

                var GetData = DapperORM.ExecuteSP<dynamic>("SP_Invoice_Conctrator_Attendance_Show", paramList).ToList();

                if (GetData.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }

                DapperORM dprObj = new DapperORM();
                DataTable dt = dprObj.ConvertToDataTable(GetData);

                int totalColumns = dt.Columns.Count;

                // -------------------------
                // ✅ HEADER ROW 1
                // -------------------------
                worksheet.Cell(1, 1).Value = companyName + " - " + branchName;
                worksheet.Range(1, 1, 1, totalColumns).Merge();
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                // -------------------------
                // ✅ HEADER ROW 2
                // -------------------------
                worksheet.Cell(2, 1).Value = "Attendance Summary - (" + InvoiceMonth.ToString("MMM/yyyy") + ")";
                worksheet.Range(2, 1, 2, totalColumns).Merge();
                worksheet.Cell(2, 1).Style.Font.Bold = true;
                worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                // -------------------------
                // ❄ FREEZE FIRST 2 ROWS
                // -------------------------
                worksheet.SheetView.FreezeRows(2);

                // -------------------------
                // ✅ INSERT TABLE FROM ROW 3
                // -------------------------
                worksheet.Cell(3, 1).InsertTable(dt, false);
                worksheet.Columns(1, 18).Hide();
                // -------------------------
                // HEADER (ROW 3)
                // -------------------------
                var headerRange = worksheet.Range(3, 1, 3, totalColumns);
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontColor = XLColor.Black;
                headerRange.Style.Font.FontSize = 10;

                // -------------------------
                // Apply standard formatting
                // -------------------------
                var usedRange = worksheet.RangeUsed();
                usedRange.Style.Font.FontSize = 10;
                usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "Report.xlsx");
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        //#region DownloadSummaryExcelFile
        //public ActionResult DownloadSummaryExcelFile(int? CmpId, int? BranchId, int? ContractorId, DateTime InvoiceMonth)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { Area = "" });
        //        }
        //        var workbook = new XLWorkbook();
        //        var worksheet = workbook.Worksheets.Add("AttendanceSummary");

        //        worksheet.Range(1, 1, 1, 10).Merge();
        //        worksheet.SheetView.FreezeRows(2);
        //        DataTable dt = new DataTable();
        //        List<dynamic> data = new List<dynamic>();

        //        DynamicParameters paramList = new DynamicParameters();
        //        paramList.Add("@p_ContractorId", ContractorId);
        //        paramList.Add("@p_MonthYear", InvoiceMonth);
        //        paramList.Add("@p_CmpId", CmpId);
        //        paramList.Add("@p_BranchId", BranchId);
        //        var GetData = DapperORM.ExecuteSP<dynamic>("SP_Invoice_Conctrator_Attendance_Show", paramList).ToList();
        //        if (GetData.Count == 0)
        //        {
        //            byte[] emptyFileContents = new byte[0];
        //            return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
        //        }
        //        DapperORM dprObj = new DapperORM();
        //        dt = dprObj.ConvertToDataTable(GetData);
        //        worksheet.Cell(2, 1).InsertTable(dt, false);

        //        worksheet.Columns(1, 18).Hide();

        //        int totalRows = worksheet.RowsUsed().Count();

        //        var lastRow = worksheet.Row(totalRows + 1);
        //        lastRow.Style.Font.Bold = false;

        //        lastRow.Style.Font.FontColor = XLColor.Black;
        //        lastRow.Style.Font.FontSize = 10;
        //        var usedRange = worksheet.RangeUsed();
        //        usedRange.Style.Fill.BackgroundColor = XLColor.White;
        //        usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        //        usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        //        usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        //        usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
        //        usedRange.Style.Font.FontSize = 10;
        //        usedRange.Style.Font.FontColor = XLColor.Black;
        //        worksheet.Cell(1, 1).Value = "Attendance Summary";

        //        //worksheet.Cell(1, 1).Value = "Late Mark AdjustmentReport Report - (" + Month.ToString("dd/MMM/yyyy") + ")";
        //        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        //        worksheet.Cell(1, 1).Style.Font.Bold = true;
        //        worksheet.Columns().AdjustToContents();
        //        var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
        //        headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
        //        headerRange.Style.Font.FontSize = 10;
        //        headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
        //        headerRange.Style.Font.Bold = true;

        //        using (var stream = new MemoryStream())
        //        {
        //            workbook.SaveAs(stream);
        //            stream.Position = 0;

        //            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report.xlsx");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
        //}
        //#endregion




    }
}