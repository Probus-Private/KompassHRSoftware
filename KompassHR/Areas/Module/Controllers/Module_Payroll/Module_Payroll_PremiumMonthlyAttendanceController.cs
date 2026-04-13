using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Module.Models.Module_Payroll;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Payroll
{
    public class Module_Payroll_PremiumMonthlyAttendanceController : Controller
    {
        // GET: Module/Module_Payroll_PremiumMonthlyAttendance
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        #region Module_Payroll_PremiumMonthlyAttendance

        public ActionResult Module_Payroll_PremiumMonthlyAttendance(MonthlyAttendance obj)
         {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                Session["MonthYear"] = null;
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 798;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetCompanyName = GetComapnyName;

                var results = DapperORM.DynamicQueryMultiple("Select ProcessCategoryId as Id,ProcessCategoryName as Name from Payroll_ProcessCategory where Deactivate = 0 order by isdefault desc");
                var SalaryProcessList = results[0].Select(x => new AllDropDownBind { Id = Convert.ToDouble(x.Id), Name = (string)x.Name }).ToList();

                ViewBag.SalaryProcess = SalaryProcessList;

                ViewBag.GetBranchName = "";

                if (obj.CmpId != 0)
                {
                    DynamicParameters paramList = new DynamicParameters();

                    paramList.Add("@p_MonthYear", obj.Month);
                    paramList.Add("@p_CmpId", obj.CmpId);
                    paramList.Add("@p_ProcessCategoryId", obj.ProcessCategoryId);
                    if (obj.BranchId != 0)
                    {
                        paramList.Add("@p_BranchId", obj.BranchId);
                        paramList.Add("@p_AllBranchId", "No");
                    }
                    else
                    {
                        paramList.Add("@p_BranchId", obj.BranchId);
                        paramList.Add("@p_AllBranchId", "All");
                    }
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    var data = DapperORM.ExecuteSP<dynamic>("sp_Get_Atten_MonthlyAttendance", paramList).ToList();
                    ViewBag.MonthlyAttendance = data;

                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_employeeid", Session["EmployeeId"]);
                    param.Add("@p_CmpId", obj.CmpId);

                    ViewBag.GetBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                    if (data.Count == 0)
                    {
                        TempData["Message"] = "Record not found";
                        TempData["Icon"] = "error";
                    }
                }
                else
                {
                    ViewBag.MonthlyAttendance = "";
                }
                return View(obj);
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

        #region GetList
        [HttpGet]
        public ActionResult GetList(MonthlyAttendance OBJList)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 798;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                if (OBJList.Month == DateTime.MinValue)
                {
                    if (Session["MonthYear"] != null)
                    {
                        var a = TempData["MonthYear"];
                        param.Add("@p_Origin", "List");
                        param.Add("@p_EmployeeId", Session["EmployeeId"]);
                        //  param.Add("@p_CmpId", Session["CompanyId"]);
                        param.Add("@p_MonthYear", Session["MonthYear"]);
                        var data = DapperORM.ReturnList<ListMonthlyAttendance>("sp_List_Atten_MonthlyAttendance", param).ToList();
                        ViewBag.GetMonthlyAttendance = data;
                        if (data.Count != 0)
                        {
                            TempData["MonthYear"] = data[0].AttenMonthlyMonthYear.ToString("yyyy-MM");
                        }

                        // TempData["MonthYear"] = data[0].AttenMonthlyMonthYear.ToString("yyyy-MM");
                    }
                    else
                    {
                        ViewBag.GetMonthlyAttendance = "";
                        TempData["MonthYear"] = null;
                    }
                }
                else
                {
                    param.Add("@p_Origin", "List");
                    param.Add("@p_EmployeeId", Session["EmployeeId"]);
                    //param.Add("@p_CmpId", Session["CompanyId"]);
                    param.Add("@p_MonthYear", OBJList.Month);
                    var data = DapperORM.ReturnList<ListMonthlyAttendance>("sp_List_Atten_MonthlyAttendance", param).ToList();
                    ViewBag.GetMonthlyAttendance = data;
                    if (data.Count != 0)
                    {
                        TempData["MonthYear"] = data[0].AttenMonthlyMonthYear.ToString("yyyy-MM");
                    }

                }

                //e
                //if (OBJList.Month != DateTime.MinValue)
                //{

                //}
                //else
                //{
                //    ViewBag.GetMonthlyAttendance = "";
                //}
                return View(OBJList);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region Module_Payroll_PremiumMonthlyAttendance bulk Main View
        [HttpPost]
        public ActionResult Module_Payroll_PremiumMonthlyAttendance(HttpPostedFileBase AttachFile, MonthlyAttendance obj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                Session["MonthYear"] = null;

                // Check user access
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 798;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                // Populate company dropdown
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetCompanyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetCompanyName = GetCompanyName;
                ViewBag.GetBranchName = "";


                var results = DapperORM.DynamicQueryMultiple("Select ProcessCategoryId as Id,ProcessCategoryName as Name from Payroll_ProcessCategory where Deactivate = 0 order by isdefault desc");
                var SalaryProcessList = results[0].Select(x => new AllDropDownBind { Id = Convert.ToDouble(x.Id), Name = (string)x.Name }).ToList();

                ViewBag.SalaryProcess = SalaryProcessList;
                if (AttachFile != null && AttachFile.ContentLength > 0)
                {
                    try
                    {
                        string directoryPath = Server.MapPath("~/assets/MonthlyAttendance");
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }
                        string fileName = Path.GetFileName(AttachFile.FileName);
                        string filePath = Path.Combine(directoryPath, fileName);
                        AttachFile.SaveAs(filePath);

                        // Read Excel data
                        using (XLWorkbook xlWorkbook = new XLWorkbook(filePath))
                        {
                            var worksheet = xlWorkbook.Worksheets.Worksheet(1);
                            int row = 3;

                            //if (string.IsNullOrWhiteSpace(worksheet.Cell(row, 1).GetString()))
                            //{
                            //    TempData["Message"] = "Fill in missing information in the first column.";
                            //    TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                            //    TempData["Icon"] = "error";
                            //    System.IO.File.Delete(filePath);
                            //    return RedirectToAction("Module_Payroll_PremiumMonthlyAttendance", "Module_Payroll_PremiumMonthlyAttendance");
                            //}

                            DynamicParameters paramList = new DynamicParameters();
                            paramList.Add("@p_MonthYear", obj.Month);
                            paramList.Add("@p_CmpId", obj.CmpId);
                            paramList.Add("@p_ProcessCategoryId", obj.ProcessCategoryId);

                            if (obj.BranchId != 0)
                            {
                                paramList.Add("@p_BranchId", obj.BranchId);
                                paramList.Add("@p_AllBranchId", "No");
                            }
                            else
                            {
                                paramList.Add("@p_BranchId", obj.BranchId);
                                paramList.Add("@p_AllBranchId", "All");
                            }

                            paramList.Add("@p_EmployeeId", Session["EmployeeId"]);

                            var data = DapperORM.ExecuteSP<dynamic>("sp_Get_Atten_MonthlyAttendance", paramList).ToList();

                            if (data == null || !data.Any())
                            {
                                TempData["Message"] = "No template data found from database.";
                                TempData["Title"] = "Template Error";
                                TempData["Icon"] = "error";
                                System.IO.File.Delete(filePath);
                                return RedirectToAction("Module_Payroll_PremiumMonthlyAttendance", "Module_Payroll_PremiumMonthlyAttendance");
                            }

                            var firstRowData = data.First();
                            IDictionary<string, object> dict;

                            if (firstRowData is IDictionary<string, object>)
                            {
                                dict = (IDictionary<string, object>)firstRowData;
                            }
                            else
                            {
                                dict = new Dictionary<string, object>();
                                var properties = firstRowData.GetType().GetProperties();
                                foreach (var property in properties)
                                {
                                    dict[property.Name] = property.GetValue(firstRowData);
                                }
                            }

                            var columnNames = dict.Keys.ToList();

                            var excelDataList = new List<IDictionary<string, object>>();
                            var headerRow = 2;
                            var excelHeaders = new List<string>();
                            int colIndex = 1;

                            while (!string.IsNullOrWhiteSpace(worksheet.Cell(headerRow, colIndex).GetString()))
                            {
                                excelHeaders.Add(worksheet.Cell(headerRow, colIndex).GetString().Trim());
                                colIndex++;
                            }

                            var BranchName = DapperORM.DynamicQueryMultiple("SELECT BranchId as Id, lower(BranchName) as Name FROM Mas_Branch WHERE Deactivate = 0");
                            var branchList = BranchName[0].Select(x => new AllDropDownBind { Id = Convert.ToDouble(x.Id), Name = (string)x.Name }).ToList();

                            var Employee = DapperORM.DynamicQueryMultiple("select employeeid as Id,lower(EmployeeNo) as Name  from mas_employee where deactivate = 0 and Employeeleft = 0 order by Name");
                            var EmployeeList = Employee[0].Select(x => new AllDropDownBind{Id = Convert.ToDouble(x.Id), Name = (string)x.Name }).ToList();

                            var branchMapping = new Dictionary<string, int>();
                            foreach (var item in branchList) // Use branchList instead of BranchName
                            {
                                string name = item.Name?.Trim() ?? "";
                                if (!string.IsNullOrEmpty(name))
                                {
                                    int id = (int)item.Id; // Convert double to int
                                    if (!branchMapping.ContainsKey(name))
                                    {
                                        branchMapping.Add(name, id);
                                    }
                                }
                            }

                            var EmployeeMapping = new Dictionary<string, int>();
                            foreach (var item in EmployeeList) // Use branchList instead of BranchName
                            {
                                string name = item.Name?.Trim() ?? "";
                                if (!string.IsNullOrEmpty(name))
                                {
                                    int id = (int)item.Id; // Convert double to int
                                    if (!EmployeeMapping.ContainsKey(name))
                                    {
                                        EmployeeMapping.Add(name, id);
                                    }
                                }
                            }
                            int employeeNameColumnIndex = excelHeaders.IndexOf("EmployeeName") + 1;
                            // Process Excel rows
                            while (!string.IsNullOrWhiteSpace(worksheet.Cell(row, employeeNameColumnIndex).GetString()))

                            {
                                var dataRow = new Dictionary<string, object>();

                                foreach (var colName in columnNames)
                                {
                                    int excelIndex = excelHeaders.IndexOf(colName) + 1;
                                    if (excelIndex > 0)
                                    {
                                        dataRow[colName] = worksheet.Cell(row, excelIndex).GetString();
                                       // dataRow["HideColumns"] = 21;

                                    }
                                    else
                                    {
                                        dataRow[colName] = null;
                                    }
                                }

                             
                                    int branchId = 0;
                                    if (dataRow.ContainsKey("BusinessUnit") && dataRow["BusinessUnit"] != null)
                                    {
                                        string businessUnit = dataRow["BusinessUnit"].ToString().Trim().ToLower();
                                        if (branchMapping.TryGetValue(businessUnit, out branchId))
                                        {
                                            dataRow["AttenMonthlyBranchId"] = branchId;
                                        }
                                    }
                           

                              
                                    int EmployeeId = 0;
                                    if (dataRow.ContainsKey("EmployeeNo") && dataRow["EmployeeNo"] != null)
                                    {
                                        string EmployeeNo = dataRow["EmployeeNo"].ToString().Trim().ToLower();
                                        if (EmployeeMapping.TryGetValue(EmployeeNo, out EmployeeId))
                                        {
                                            dataRow["AttenMonthlyEmployeeId"] = EmployeeId;
                                        }
                                    }
                               
                                excelDataList.Add(dataRow);
                                row++;
                            }

                            // Delete the file and assign to ViewBag
                            System.IO.File.Delete(filePath);
                            ViewBag.count = 1;
                            ViewBag.MonthlyAttendance = excelDataList;
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["Message"] = $"Error processing file: {ex.Message}";
                        TempData["Title"] = "Processing Error";
                        TempData["Icon"] = "error";
                        return RedirectToAction("Module_Payroll_PremiumMonthlyAttendance", "Module_Payroll_PremiumMonthlyAttendance");
                    }
                }
                else
                {
                    if (obj.CmpId != 0)
                    {
                        DynamicParameters paramList = new DynamicParameters();
                        paramList.Add("@p_MonthYear", obj.Month);
                        paramList.Add("@p_CmpId", obj.CmpId);
                        paramList.Add("@p_ProcessCategoryId", obj.ProcessCategoryId);
                        if (obj.BranchId != 0)
                        {
                            paramList.Add("@p_BranchId", obj.BranchId);
                            paramList.Add("@p_AllBranchId", "No");
                        }
                        else
                        {
                            paramList.Add("@p_BranchId", obj.BranchId);
                            paramList.Add("@p_AllBranchId", "All");
                        }
                        paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                        var data = DapperORM.ExecuteSP<dynamic>("sp_Get_Atten_MonthlyAttendance", paramList).ToList();
                        ViewBag.MonthlyAttendance = data;

                        // Populate branch dropdown
                        DynamicParameters param = new DynamicParameters();
                        param.Add("@p_employeeid", Session["EmployeeId"]);
                        param.Add("@p_CmpId", obj.CmpId);
                        ViewBag.GetBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                        if (data.Count == 0)
                        {
                            TempData["Message"] = "Record not found";
                            TempData["Icon"] = "error";
                        }
                    }
                    else
                    {
                        ViewBag.MonthlyAttendance = new List<dynamic>(); // Empty list for consistency
                    }
                }

                return View(obj);
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
        public ActionResult SaveUpdate(SaveMonthlyAttendanceViewModel model, int CmpId, DateTime Month, int? BranchId, string ProcessCategoryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var PayrollCount = DapperORM.DynamicQuerySingle($@" SELECT COUNT(PayrollLockId) AS LockCount 
                                              FROM Payroll_Lock WHERE Deactivate = 0  
                                              AND ({(BranchId == null ? "1=1" : $"PayrollLockBranchID = {BranchId}")})
                                              AND MONTH(PayrollLockMonthYear) = {Month.ToString("MM")} 
                                              AND YEAR(PayrollLockMonthYear)  = {Month.ToString("yyyy")} 
                                              AND Status = 1");
                if (PayrollCount.LockCount != 0)
                {
                    TempData["Message"] = "The record can't be saved because the payroll for this month ('" + Month.ToString("MMM-yyyy") + "') is locked.";
                    TempData["Icon"] = "error";
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }

                DataTable tbl_TypeAtten_MonthlyAttendance = new DataTable();
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyBranchId", typeof(double));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyEmployeeId", typeof(double));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyIsSalaryFullPay", typeof(bool));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyDailyMonthly", typeof(string));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyMonthDays", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyAB", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyPP", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyWO", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyPH", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyCO", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyCL", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlySL", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyEL", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyPL", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyML", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyESIC", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyLWP", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyLL", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyPayableDays", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyOThrs", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyLOPHrs", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyLateCount", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyLateHrs", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyEarlyCount", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyEarlyHrs", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyTotalShortHrs", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyAtd1", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyAtd2", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyAtd3", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthly_LeaveDeduction", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthly_Remark", typeof(string));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyEmployeeLeft", typeof(bool));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyActualPayableDays", typeof(float));
                tbl_TypeAtten_MonthlyAttendance.Columns.Add("AttenMonthlyAttenAdjusted", typeof(bool));

                // First, group by employee to check for duplicates and validate payable days
                //var employeeGroups = model.ObjMonthlyAttendance.GroupBy(x => x.AttenMonthlyEmployeeId).Where(g => g.Count() > 1);
                //foreach (var duplicateGroup in employeeGroups)
                //{
                //    var employeeId = duplicateGroup.Key;
                //    var totalPayableDays = duplicateGroup.Sum(x => float.Parse(x.PayableDays ?? "0"));
                //    var monthDays = float.Parse(duplicateGroup.First().MonthDays ?? "0");

                //    if (totalPayableDays > monthDays)
                //    {
                //        var firstRecord = duplicateGroup.First();
                //        string employeeName = firstRecord.EmployeeName;
                //        TempData["Message"] = $"Payable days exceeds Month days ({monthDays}) for {employeeName} ";
                //        TempData["Title"] = "Validation Error";
                //        TempData["Icon"] = "error";
                //        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                //    }
                //}

                foreach (var item in model.ObjMonthlyAttendance)
                {
                    DataRow dr = tbl_TypeAtten_MonthlyAttendance.NewRow();

                    dr["AttenMonthlyBranchId"] = Convert.ToDouble(item.AttenMonthlyBranchId ?? "0");
                    dr["AttenMonthlyEmployeeId"] = Convert.ToDouble(item.AttenMonthlyEmployeeId ?? "0");
                    dr["AttenMonthlyIsSalaryFullPay"] = item.AttenMonthlyIsSalaryFullPay == "1" || item.AttenMonthlyIsSalaryFullPay?.ToLower() == "true";
                    dr["AttenMonthlyDailyMonthly"] = item.DailyMonthly ?? "";
                    dr["AttenMonthlyMonthDays"] = float.Parse(item.MonthDays ?? "0");
                    dr["AttenMonthlyAB"] = float.Parse(item.AB ?? "0");
                    dr["AttenMonthlyPP"] = float.Parse(item.PP ?? "0");
                    dr["AttenMonthlyWO"] = float.Parse(item.WO ?? "0");
                    dr["AttenMonthlyPH"] = float.Parse(item.PH ?? "0");
                    dr["AttenMonthlyCO"] = float.Parse(item.CO ?? "0");
                    dr["AttenMonthlyCL"] = float.Parse(item.CL ?? "0");
                    dr["AttenMonthlySL"] = float.Parse(item.SL ?? "0");
                    dr["AttenMonthlyEL"] = float.Parse(item.EL ?? "0");
                    dr["AttenMonthlyPL"] = float.Parse(item.PL ?? "0");
                    dr["AttenMonthlyML"] = float.Parse(item.ML ?? "0");
                    dr["AttenMonthlyESIC"] = float.Parse(item.ESIC ?? "0");
                    dr["AttenMonthlyLWP"] = float.Parse(item.LWP ?? "0");
                    dr["AttenMonthlyLL"] = float.Parse(item.OtherLeave ?? "0");
                    dr["AttenMonthlyPayableDays"] = float.Parse(item.PayableDays ?? "0");
                    dr["AttenMonthlyOThrs"] = float.Parse(item.OTHrs ?? "0");
                    dr["AttenMonthlyLOPHrs"] = float.Parse(item.LOPDays ?? "0");
                    dr["AttenMonthlyLateCount"] = float.Parse(item.LateComingCount ?? "0");
                    dr["AttenMonthlyLateHrs"] = float.Parse(item.LateHrs ?? "0");
                    dr["AttenMonthlyEarlyCount"] = float.Parse(item.EarlyGoingCount ?? "0");
                    dr["AttenMonthlyEarlyHrs"] = float.Parse(item.EarlyHrs ?? "0");
                    dr["AttenMonthlyTotalShortHrs"] = float.Parse(item.TotalShortHrs ?? "0");
                    dr["AttenMonthlyAtd1"] = float.Parse(item.Atd1 ?? "0");
                    dr["AttenMonthlyAtd2"] = float.Parse(item.Atd2 ?? "0");
                    dr["AttenMonthlyAtd3"] = float.Parse(item.Atd3 ?? "0");
                    dr["AttenMonthly_LeaveDeduction"] = float.Parse(item.DaysForLeaveDeduction ?? "0");
                    dr["AttenMonthly_Remark"] = item.Remark ?? "";
                    dr["AttenMonthlyEmployeeLeft"] = item.AttenMonthlyEmployeeLeft == "1" || item.AttenMonthlyEmployeeLeft?.ToLower() == "true";
                    dr["AttenMonthlyActualPayableDays"] = float.Parse(item.AttenMonthlyActualPayableDays ?? "0");
                    dr["AttenMonthlyAttenAdjusted"] = false;

                    tbl_TypeAtten_MonthlyAttendance.Rows.Add(dr);
                }

                DynamicParameters ARparam = new DynamicParameters();
                ARparam.Add("@tbl_TypeAtten_MonthlyAttendance", tbl_TypeAtten_MonthlyAttendance.AsTableValuedParameter("tbl_TypeAtten_MonthlyAttendance"));
                ARparam.Add("@p_CmpID", CmpId);
                ARparam.Add("@p_MonthYear", Month);
                ARparam.Add("@p_ProcessCategoryId", ProcessCategoryId);
                ARparam.Add("@p_CreatedupdateBy", Session["EmployeeName"]);
                ARparam.Add("@p_MachineName", Dns.GetHostName().ToString());
                ARparam.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                ARparam.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter  
                var GetData = DapperORM.ExecuteSP<dynamic>("sp_SUD_MonthlyAttendance_Summary", ARparam).ToList();
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

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, string MonthYear, int? Atten_ProcessCategoryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("MonthlyAttendance");
                worksheet.Range(1, 1, 1, 23).Merge();  //Merge First Row with 60 Clomn
                worksheet.SheetView.FreezeRows(2); // Freeze the row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();
                param.Add("@p_Origin", "Export");
                param.Add("@p_MonthYear", DateTime.Parse(MonthYear));
                param.Add("@p_BranchId", BranchId);
                param.Add("@p_Atten_ProcessCategoryId", Atten_ProcessCategoryId);
                data = DapperORM.ExecuteSP<dynamic>("sp_List_Atten_MonthlyAttendance", param).ToList();
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
                worksheet.Cell(1, 1).Value = "Monthly Attendance "; // Replace "Excel Name" with the actual name
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

        #region GetMontlyAttendance
        public ActionResult GetMontlyAttendance(int? AttenMonthlyBranchId, string AttenMonthlyMonthYear, int? Atten_ProcessCategoryId)
        {
            try
            {  //PinCode
                Session["AttenMonthlyMonthYear"] = AttenMonthlyMonthYear;
                param.Add("@p_Origin", "MonthlyAttendance");
                param.Add("@p_BranchId", AttenMonthlyBranchId);
                param.Add("@p_MonthYear", DateTime.Parse(AttenMonthlyMonthYear));
                param.Add("@p_Atten_ProcessCategoryId", Atten_ProcessCategoryId);

                var data = DapperORM.ReturnList<dynamic>("sp_List_Atten_MonthlyAttendance", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region DeleteMontlyAttendance

        public ActionResult DeleteMontlyAttendance(DeleteMonthlyAttendanceViewModel model, DateTime Month)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DataTable tbl_TypeAtten_MonthlyAttendance_Delete = new DataTable();
                tbl_TypeAtten_MonthlyAttendance_Delete.Columns.Add("AttenMonthlyId", typeof(double));
                tbl_TypeAtten_MonthlyAttendance_Delete.Columns.Add("AttenMonthlyEmployeeId", typeof(double));
                tbl_TypeAtten_MonthlyAttendance_Delete.Columns.Add("AttenMonthlyMonthYear", typeof(DateTime));

                foreach (var item in model.ObjMonthlyAttendance)
                {
                    DataRow dr = tbl_TypeAtten_MonthlyAttendance_Delete.NewRow();
                    dr["AttenMonthlyId"] = Convert.ToDouble(item.AttenMonthlyId ?? "0");
                    dr["AttenMonthlyEmployeeId"] = Convert.ToDouble(item.AttenMonthlyEmployeeId ?? "0");
                    dr["AttenMonthlyMonthYear"] = Convert.ToDateTime(item.AttenMonthlyMonthYear);
                    tbl_TypeAtten_MonthlyAttendance_Delete.Rows.Add(dr);
                }

                DynamicParameters ARparam = new DynamicParameters();
                ARparam.Add("@p_AttenMonthlyMonthYear", Month);
                ARparam.Add("@tbl_TypeAtten_MonthlyAttendance_Delete", tbl_TypeAtten_MonthlyAttendance_Delete.AsTableValuedParameter("tbl_TypeAtten_MonthlyAttendance_Delete"));
                ARparam.Add("@p_CreatedupdateBy", Session["EmployeeName"]);
                ARparam.Add("@p_MachineName", Dns.GetHostName().ToString());
                ARparam.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                ARparam.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter  
                var GetData = DapperORM.ExecuteSP<dynamic>("sp_SUD_MonthlyAttendance_Summary_Delete", ARparam).ToList();
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

        #region DownloadExcelFile
        public ActionResult DownloadMonthlyAttendnceExcelFile(int? CmpId, int? BranchId, DateTime MonthYear, int? ProcessCategoryId)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                // Initialize Dapper parameters
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_MonthYear", MonthYear);
                param.Add("@p_CmpId", CmpId ?? 0);
                param.Add("@p_BranchId", BranchId ?? 0);
                param.Add("@p_AllBranchId", BranchId.HasValue && BranchId != 0 ? "No" : "All");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_ProcessCategoryId", ProcessCategoryId);
                // Fetch data using Dapper
                var data = DapperORM.ExecuteSP<dynamic>("sp_Get_Atten_MonthlyAttendance", param)
                    .Select(item => (IDictionary<string, object>)item)
                    .ToList();

                if (!data.Any())
                {
                    TempData["Message"] = "No data available to export.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("Module_Payroll_PremiumMonthlyAttendance");
                }

                // Create Excel workbook
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("MonthlyAttendance");

                    // Get all column names
                    var columnNames = data.First().Keys.ToList();
                    int totalColumns = columnNames.Count;

                    // Check if there are enough columns
                    if (totalColumns < 22)
                    {
                        TempData["Message"] = $"Data has only {totalColumns} columns; cannot show columns from 22 onward.";
                        TempData["Icon"] = "warning";
                        return RedirectToAction("Module_Payroll_PremiumMonthlyAttendance");
                    }

                    // Merge first row for title (across all columns)
                    worksheet.Range(1, 1, 1, totalColumns).Merge();
                    worksheet.Cell(1, 1).Value = "Monthly Attendance";
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Font.FontSize = 12;

                    // Freeze the second row (headers)
                    worksheet.SheetView.FreezeRows(2);

                    // Write headers
                    for (int col = 0; col < totalColumns; col++)
                    {
                        worksheet.Cell(2, col + 1).Value = columnNames[col];
                    }

                    // Write data rows
                    for (int row = 0; row < data.Count; row++)
                    {
                        var dataRow = data[row];
                        for (int col = 0; col < totalColumns; col++)
                        {
                            var value = dataRow.ContainsKey(columnNames[col]) ? dataRow[columnNames[col]]?.ToString() : "";
                            worksheet.Cell(row + 3, col + 1).Value = value;
                        }
                    }

                    int hideColumns = 0;
                    if (data.Any() && data[0].ContainsKey("HideColumns"))
                    {
                        hideColumns = Convert.ToInt32(data[0]["HideColumns"]);
                    }

                    // Hide first 21 columns (A to U)
                    for (int col = 1; col <= hideColumns; col++)
                    {
                        worksheet.Column(col).Hide();
                    }

                    // Ensure columns from 22 onward are visible
                    for (int col = hideColumns + 1; col <= totalColumns; col++)
                    {
                        worksheet.Column(col).Unhide();
                    }

                    // Style the used range
                    var usedRange = worksheet.RangeUsed();
                    usedRange.Style.Fill.BackgroundColor = XLColor.White;
                    usedRange.Style.Border.SetTopBorder(XLBorderStyleValues.Thin);
                    usedRange.Style.Border.SetBottomBorder(XLBorderStyleValues.Thin);
                    usedRange.Style.Border.SetLeftBorder(XLBorderStyleValues.Thin);
                    usedRange.Style.Border.SetRightBorder(XLBorderStyleValues.Thin);
                    usedRange.Style.Font.FontSize = 10;
                    usedRange.Style.Font.FontColor = XLColor.Black;

                    // Style header row
                    var headerRange = worksheet.Range(2, 1, 2, totalColumns);
                    headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                    headerRange.Style.Font.FontSize = 10;
                    headerRange.Style.Font.FontColor = XLColor.Black;
                    headerRange.Style.Font.Bold = true;

                    // Auto-fit visible columns (22 onward)
                    for (int col = 22; col <= totalColumns; col++)
                    {
                        worksheet.Column(col).AdjustToContents();
                    }

                    // Save to memory stream
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MonthlyAttendance.xlsx");
                    }
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