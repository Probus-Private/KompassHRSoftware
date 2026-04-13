using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Module.Models.Module_Payroll;
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

namespace KompassHR.Areas.Module.Controllers.Module_Payroll
{
    public class Module_Payroll_PayrollBulkHoldReleaseController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        DataTable dt_ValidationCheck = new DataTable();
        DataTable dt_PayrollHoldCheck = new DataTable();
        // GET: Module/Module_Payroll_PayrollBulkHoldRelease
        #region Module_Payroll_PayrollBulkHoldRelease
        public ActionResult Module_Payroll_PayrollBulkHoldRelease()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 494;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Payroll_SalaryHold _SalaryHold = new Payroll_SalaryHold();
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                ViewBag.GetCompanyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetBranchName = "";
                ViewBag.GetPayrollHold = "";
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region Module_Payroll_PayrollBulkHoldRelease MAin View
        [HttpPost]
        public ActionResult Module_Payroll_PayrollBulkHoldRelease(Payroll_SalaryHold_Bulk _SalaryHold_Bulk, HttpPostedFileBase AttachFile)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 494;
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
                ViewBag.GetBranchName = "";

                DynamicParameters paramBu = new DynamicParameters();
                paramBu.Add("@p_employeeid", Session["EmployeeId"]);
                paramBu.Add("@p_CmpId", _SalaryHold_Bulk.CmpId);
                ViewBag.GetBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBu).ToList();
                if (AttachFile != null)
                {
                    if (AttachFile.ContentLength > 0)
                    {
                        List<Payroll_SalaryHold_Bulk_excel> excelDataList = new List<Payroll_SalaryHold_Bulk_excel>();
                        using (var stream = new MemoryStream())
                        {
                            using (var attachStream = AttachFile.InputStream)
                            {
                                attachStream.Position = 0;
                                XLWorkbook xlWorkwook = new XLWorkbook(attachStream);
                                xlWorkwook.SaveAs(stream);

                                int row1 = 3;
                                var employeeNumbers = new HashSet<string>();
                                var duplicateEmployeeNumbers = new List<string>();

                                while (xlWorkwook.Worksheets.Worksheet(1).Cell(row1, 1).GetString() != "")
                                {
                                    string employeeNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row1, 1).GetString();
                                    if (!string.IsNullOrWhiteSpace(employeeNo))
                                    {
                                        if (!employeeNumbers.Add(employeeNo))
                                        {
                                            duplicateEmployeeNumbers.Add(employeeNo);
                                        }
                                    }
                                    row1++;
                                }

                                if (duplicateEmployeeNumbers.Any())
                                {
                                    TempData["Message"] = $"Duplicate Employee Numbers found: {string.Join(", ", duplicateEmployeeNumbers.Distinct())}";
                                    TempData["Title"] = "Duplicate Employee Numbers Detected";
                                    TempData["Icon"] = "error";
                                    return RedirectToAction("Module_Payroll_PayrollBulkHoldRelease", "Module_Payroll_PayrollBulkHoldRelease");
                                }

                                int row = 3;
                                if (xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString() == "")
                                {
                                    TempData["Message"] = "Fill in missing information in the first column.";
                                    TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                    TempData["Icon"] = "error";
                                    return RedirectToAction("Module_Payroll_PayrollBulkHoldRelease", "Module_Payroll_PayrollBulkHoldRelease");
                                }

                                while (xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString() != "")
                                {
                                    Payroll_SalaryHold_Bulk_excel _excel = new Payroll_SalaryHold_Bulk_excel();
                                    _excel.EmployeeNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString();
                                    _excel.EmployeeName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 2).GetString();
                                    _excel.Remark = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 3).GetString();
                                    _excel.SalaryBranchId = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 4).GetString(); // Get Branch ID
                                    int BranchId = GetBranchId_Id(_excel.EmployeeNo);
                                    _excel.SalaryBranchId = BranchId.ToString();
                                    excelDataList.Add(_excel);
                                    row++;
                                }
                                ViewBag.GetPayrollHold = excelDataList;
                            }
                        }
                    }
                }
                else if (_SalaryHold_Bulk.CmpId != 0)
                {
                    DynamicParameters paramEMP = new DynamicParameters();
                    paramEMP.Add("@p_CmpId", _SalaryHold_Bulk.CmpId);
                    paramEMP.Add("@p_BranchId", _SalaryHold_Bulk.BranchId);
                    paramEMP.Add("@p_SalaryMonth", _SalaryHold_Bulk.Payroll_SalaryMonth);
                    paramEMP.Add("@p_Type", _SalaryHold_Bulk.Type);
                    ViewBag.GetPayrollHold = DapperORM.ExecuteSP<dynamic>("sp_Get_PayrollHoldRelease", paramEMP).ToList();
                }
                else
                {
                    ViewBag.GetPayrollHold = "";
                    ViewBag.GetBranchName = "";
                }
                return View(_SalaryHold_Bulk);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region SaveUpdate
        int List_Employee_Id = 0;
        int Employee_Id = 0;
        string PayrollHoldId_Encrypted = "";
        string List_PayrollHoldId_Encrypted = "";
        [HttpPost]
        public ActionResult SaveUpdate(List<Other_Deduction_Excel> ObjHoldRelease, int CmpId, int BranchId, DateTime MonthYear, string Type)
        {
            try
            {
                var PayrollCount = DapperORM.DynamicQuerySingle("Select Count(PayrollLockId) as LockCount from Payroll_LOck where Deactivate=0 and PayrollLockBranchID=" + BranchId + " and Month(PayrollLockMonthYear)='" + MonthYear.ToString("MM") + "'  and Year(PayrollLockMonthYear) ='" + MonthYear.ToString("yyyy") + "' and Status=1");
                if (PayrollCount.LockCount != 0)
                {
                    TempData["Message"] = "The record can't be saved because the payroll for this month ('" + MonthYear.ToString("MMM-yyyy") + "') is locked.";
                    TempData["Icon"] = "error";
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }

                dt_ValidationCheck = objcon.GetDataTable("Select Mas_Employee.EmployeeName,Mas_Employee.EmployeeNo,Mas_Employee.EmployeeCardNo,Mas_Employee_Personal.AadhaarNo  ,Convert (int,Mas_Employee.EmployeeId) As EmployeeId,Mas_CompanyProfile.CompanyName,Convert (int,Mas_Employee.CmpID) AS  EmployeeCmpID,Mas_Branch.BranchName,Convert (int,Mas_Employee.EmployeeBranchId) AS  EmployeeBranchId from Mas_Employee inner join Mas_Employee_Personal on Mas_Employee.employeeid = Mas_Employee_Personal.personalEmployeeid inner join Mas_CompanyProfile on Mas_Employee.CmpID = Mas_CompanyProfile.CompanyId inner join Mas_Branch on Mas_Employee.EmployeeBranchId = Mas_Branch.BranchId where Mas_Employee.Deactivate = 0 and Mas_Employee_Personal.Deactivate = 0");
                dt_PayrollHoldCheck = objcon.GetDataTable("Select Convert (int,Payroll_SalaryHold.Payroll_SalaryEmployeeId) As EmployeeId, Convert (int,Payroll_SalaryHold.PayrollHoldId) As PayrollHoldId,PayrollHoldId_Encrypted,Payroll_SalaryMonth  from Payroll_SalaryHold where Deactivate=0 and Payroll_SalaryHold.IsRelease=0 and   (month (Payroll_SalaryMonth)=month('" + Convert.ToDateTime(MonthYear).ToString("yyyy-MM-dd") + "') and Year (Payroll_SalaryMonth)=year('" + Convert.ToDateTime(MonthYear).ToString("yyyy-MM-dd") + "'))");

                //StringBuilder strBuilder = new StringBuilder();
                //int i = 0;
                if (Type == "Hold")
                {
                    if (ObjHoldRelease != null)
                    {
                        foreach (var Data in ObjHoldRelease)
                        {

                            List_Employee_Id = 0;
                            Employee_Id = 0;


                            GetEmployeeId_Id(Data.EmployeeNo.ToString(), CmpId, BranchId, out Employee_Id);
                            List_Employee_Id = Employee_Id;
                            if (Employee_Id == 0)
                            {
                                var Message = "The employee does not belong to this business unit of Employee No - " + Data.EmployeeNo;
                                var Icon = "error";
                                var rowno = Data.EmployeeNo;
                                return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                            }
                            GetEncrypted_Id(List_Employee_Id, MonthYear, out PayrollHoldId_Encrypted);
                            List_PayrollHoldId_Encrypted = PayrollHoldId_Encrypted;
                            if (List_PayrollHoldId_Encrypted != "")
                            {
                                var Message = $"The employee already exists in payroll hold/release of Employee No. - {Data.EmployeeNo}";
                                var Icon = "error";
                                var rowno = Data.EmployeeNo;
                                return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                            }
                        
                            // Check Employee Salary process done or not
                            DynamicParameters paramEMP = new DynamicParameters();
                            paramEMP.Add("@p_CmpId", CmpId);
                            paramEMP.Add("@p_BranchId", BranchId);
                            paramEMP.Add("@p_SalaryMonth", MonthYear);
                            paramEMP.Add("@p_Type", Type);
                            var GetPayrollHoldRealeaselist = DapperORM.ExecuteSP<dynamic>("sp_Get_PayrollHoldRelease", paramEMP).ToList();

                            var IsEmployeeProcessed = GetPayrollHoldRealeaselist
                                    .Any(x => x.EmployeeNo.ToString() == Data.EmployeeNo.ToString());

                            if (!IsEmployeeProcessed)
                            {
                                var Message = $"Payroll salary not processed for this employee for month {MonthYear:MMM-yyyy} - Employee No: {Data.EmployeeNo}";
                                var Icon = "error";
                                var rowno = Data.EmployeeNo;

                                return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                            }

                            param.Add("@p_process", "Save");

                            if (BranchId == 0)
                            {
                                param.Add("@p_BranchId", Data.SalaryBranchId);
                            }
                            else
                            {
                                param.Add("@p_BranchId", BranchId);
                            }

                            param.Add("@p_CmpId", CmpId);
                            param.Add("@p_Payroll_SalaryEmployeeId", List_Employee_Id);
                            param.Add("@p_Payroll_SalaryMonth", MonthYear.ToString("yyyy-MM-dd"));
                            //param.Add("@p_Payroll_SalaryAmount", OBJHold.Payroll_SalaryAmount);
                            param.Add("@p_HoldRemark", Data.Remark);
                            param.Add("@p_HoldEmployeeId", Session["EmployeeId"]);
                            param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                            param.Add("@p_MachineName", Dns.GetHostName().ToString());
                            param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                            param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                            var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_SalaryHold", param);
                            TempData["Message"] = param.Get<string>("@p_msg");
                            TempData["Icon"] = param.Get<string>("@p_Icon");

                            //if (i == 0)
                            //{
                            //    strBuilder.Append("DECLARE @IdentityValue_HoldId AS TABLE(ID INT); ");
                            //}
                            //List_Employee_Id = 0;
                            //Employee_Id = 0;


                            //GetEmployeeId_Id(Data.EmployeeNo.ToString(), BranchId, out Employee_Id);
                            //List_Employee_Id = Employee_Id;
                            //if (Employee_Id == 0)
                            //{
                            //    var Message = "The employee does not belong to this business unit (SrNo). " + Data.EmployeeNo;
                            //    var Icon = "error";
                            //    var rowno = Data.EmployeeNo;
                            //    return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                            //}


                            //string StringDeduction = " Insert Into Payroll_SalaryHold(" +
                            //                      "  Deactivate " +
                            //                      " ,CreatedBy " +
                            //                      " ,CreatedDate " +
                            //                      " ,MachineName " +
                            //                      " ,CmpId " +
                            //                      " ,BranchId " +
                            //                       " ,Payroll_SalaryId " +
                            //                      " ,Payroll_SalaryEmployeeId " +
                            //                      " ,Payroll_SalaryMonth " +
                            //                      " ,Payroll_SalaryAmount " +
                            //                      " ,IsHold " +
                            //                      " ,HoldRemark " +
                            //                      " ,IsHold " +
                            //                      " ,HoldEmployeeId " + ") OUTPUT Inserted.PayrollHoldId INTO @IdentityValue_HoldId  values (" +
                            //                      "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + " '" + Dns.GetHostName().ToString() + "'," + CmpId + "'," +
                            //                      "'" + BranchId + "'," +
                            //                      "'" + Payroll_SalaryId + "'," +
                            //                      "'" + List_Employee_Id + "'," +
                            //                      "'" + Convert.ToDateTime(MonthYear).ToString("yyyy-MM-dd") + "'," +
                            //                      "'" + Payroll_SalaryAmount + "'," +
                            //                      "'" + Type + "'," +
                            //                      "Getdate()," +
                            //                      "'" + Data.Remark + "'," +
                            //                      "'" + Session["EmployeeName"] + "'" + ")" +
                            //                      " " +
                            //                      " " +
                            //                      " " +
                            //                      " Update Payroll_SalaryHold Set PayrollHoldId_Encrypted = (master.dbo.fn_varbintohexstr(HashBytes('SHA2_256 ', convert(nvarchar(70), (SELECT ID FROM @IdentityValue_HoldId)))))  where PayrollHoldId=(SELECT ID FROM @IdentityValue_HoldId)" +
                            //                      " " +
                            //                      " " +
                            //                      " " +
                            //                        " ";
                            //strBuilder.Append(StringDeduction);
                            //strBuilder.Append(" DELETE FROM @IdentityValue_EmployeeID     ;");


                            //i = i + 1;

                        }
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    if (ObjHoldRelease != null)
                    {
                        foreach (var Data in ObjHoldRelease)
                        {

                            List_Employee_Id = 0;
                            Employee_Id = 0;
                            PayrollHoldId_Encrypted = "";
                            List_PayrollHoldId_Encrypted = "";


                            GetEmployeeId_Id(Data.EmployeeNo.ToString(), CmpId, BranchId, out Employee_Id);
                            List_Employee_Id = Employee_Id;
                            if (Employee_Id == 0)
                            {
                                var Message = "The employee does not belong to this business unit of Employee No - " + Data.EmployeeNo;
                                var Icon = "error";
                                var rowno = Data.EmployeeNo;
                                return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                            }

                            GetEncrypted_Id(List_Employee_Id, MonthYear, out PayrollHoldId_Encrypted);
                            List_PayrollHoldId_Encrypted = PayrollHoldId_Encrypted;
                            if (string.IsNullOrEmpty(PayrollHoldId_Encrypted))
                            {
                                var Message = $"The payroll process is not on hold for Employee No - {Data.EmployeeNo}";
                                var Icon = "error";
                                var rowno = Data.EmployeeNo;
                                return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                            }


                            //Check Imported Employee present in payroll salary or not
                         //   GetPayrollSalary_Employee(List_Employee_Id, MonthYear, out PayrollHoldId_Encrypted);

                            // Proceed with other logic if PayrollHoldId_Encrypted is found


                            param.Add("@p_process", "Update");
                            param.Add("@p_PayrollHoldId_Encrypted", List_PayrollHoldId_Encrypted);
                            param.Add("@p_Payroll_SalaryEmployeeId", List_Employee_Id);
                            param.Add("@p_ReleaseRemark", Data.Remark);
                            param.Add("@p_ReleaseEmployeeId", Session["EmployeeId"]);
                            param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                            param.Add("@p_MachineName", Dns.GetHostName().ToString());
                            param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                            param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                            var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_SalaryHold", param);
                            TempData["Message"] = param.Get<string>("@p_msg");
                            TempData["Icon"] = param.Get<string>("@p_Icon");

                            //if (i == 0)
                            //{
                            //    strBuilder.Append("DECLARE @IdentityValue_HoldId AS TABLE(ID INT); ");
                            //}
                            //List_Employee_Id = 0;
                            //Employee_Id = 0;


                            //GetEmployeeId_Id(Data.EmployeeNo.ToString(), BranchId, out Employee_Id);
                            //List_Employee_Id = Employee_Id;
                            //if (Employee_Id == 0)
                            //{
                            //    var Message = "The employee does not belong to this business unit (SrNo). " + Data.EmployeeNo;
                            //    var Icon = "error";
                            //    var rowno = Data.EmployeeNo;
                            //    return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                            //}


                            //string StringDeduction = " Insert Into Payroll_SalaryHold(" +
                            //                      "  Deactivate " +
                            //                      " ,CreatedBy " +
                            //                      " ,CreatedDate " +
                            //                      " ,MachineName " +
                            //                      " ,CmpId " +
                            //                      " ,BranchId " +
                            //                       " ,Payroll_SalaryId " +
                            //                      " ,Payroll_SalaryEmployeeId " +
                            //                      " ,Payroll_SalaryMonth " +
                            //                      " ,Payroll_SalaryAmount " +
                            //                      " ,IsHold " +
                            //                      " ,HoldRemark " +
                            //                      " ,IsHold " +
                            //                      " ,HoldEmployeeId " + ") OUTPUT Inserted.PayrollHoldId INTO @IdentityValue_HoldId  values (" +
                            //                      "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + " '" + Dns.GetHostName().ToString() + "'," + CmpId + "'," +
                            //                      "'" + BranchId + "'," +
                            //                      "'" + Payroll_SalaryId + "'," +
                            //                      "'" + List_Employee_Id + "'," +
                            //                      "'" + Convert.ToDateTime(MonthYear).ToString("yyyy-MM-dd") + "'," +
                            //                      "'" + Payroll_SalaryAmount + "'," +
                            //                      "'" + Type + "'," +
                            //                      "Getdate()," +
                            //                      "'" + Data.Remark + "'," +
                            //                      "'" + Session["EmployeeName"] + "'" + ")" +
                            //                      " " +
                            //                      " " +
                            //                      " " +
                            //                      " Update Payroll_SalaryHold Set PayrollHoldId_Encrypted = (master.dbo.fn_varbintohexstr(HashBytes('SHA2_256 ', convert(nvarchar(70), (SELECT ID FROM @IdentityValue_HoldId)))))  where PayrollHoldId=(SELECT ID FROM @IdentityValue_HoldId)" +
                            //                      " " +
                            //                      " " +
                            //                      " " +
                            //                        " ";
                            //strBuilder.Append(StringDeduction);
                            //strBuilder.Append(" DELETE FROM @IdentityValue_EmployeeID     ;");


                            //i = i + 1;

                        }
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
        public ActionResult GetList(ActualLogReportFilter ObjActualLogReportFilter)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 492;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                if (ObjActualLogReportFilter.FromDate != null)
                {
                    param.Add("@p_PayrollHoldId_Encrypted", "List");
                    param.Add("@p_EmployeeId", Session["EmployeeId"]);
                    param.Add("@p_HoldMonth", ObjActualLogReportFilter.FromDate);
                    var data = DapperORM.ExecuteSP<dynamic>("sp_List_Payroll_SalaryHold", param).ToList();
                    ViewBag._SalaryHold = data;
                    var Getmonth = ObjActualLogReportFilter.FromDate?.ToString("yyyy-MM");
                    TempData["MonthYear"] = Getmonth;

                }
                else
                {
                    TempData["MonthYear"] = "";
                    ViewBag._SalaryHold = "";
                }


                return View(ObjActualLogReportFilter);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetEmployeeId_Id
        void GetEmployeeId_Id(string EmployeeNo, int EmployeeCmpID, int EmployeeBranchId, out int Employee_Id)
        {
            try
            {
                // Find the row where EmployeeNo matches but belongs to a different branch
                var conflictingRow = dt_ValidationCheck.AsEnumerable()
                    .FirstOrDefault(row =>
                        string.Equals(row.Field<string>("EmployeeNo"), EmployeeNo, StringComparison.OrdinalIgnoreCase) &&
                        row.Field<int>("EmployeeCmpID") != EmployeeCmpID);

                if (conflictingRow != null)
                {
                    // EmployeeNo exists but belongs to a different branch
                    Employee_Id = 0;
                }
                else
                {
                    // Check if EmployeeNo exists in the specified branch
                    var matchingRow = dt_ValidationCheck.AsEnumerable()
                        .FirstOrDefault(row =>
                            string.Equals(row.Field<string>("EmployeeNo"), EmployeeNo, StringComparison.OrdinalIgnoreCase) &&
                            row.Field<int>("EmployeeCmpID") == EmployeeCmpID);

                    if (matchingRow != null)
                    {
                        // EmployeeNo belongs to the specified branch, return EmployeeId
                        Employee_Id = matchingRow.Field<int>("EmployeeId");
                    }
                    else
                    {
                        // EmployeeNo does not exist at all
                        Employee_Id = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception (if necessary) and set default value
                Console.WriteLine($"Error: {ex.Message}");
                Employee_Id = 0;
            }
        }



        void GetEncrypted_Id(int EmployeeId, DateTime MonthYear, out string PayrollHoldId_Encrypted)
        {
            PayrollHoldId_Encrypted = string.Empty; // Default value in case no match is found

            try
            {
                if (dt_PayrollHoldCheck != null && dt_PayrollHoldCheck.Rows.Count > 0)
                {
                    var matchingRow = dt_PayrollHoldCheck.AsEnumerable()
                        .FirstOrDefault(row =>
                            row.Field<DateTime>("Payroll_SalaryMonth").Month == MonthYear.Month &&
                            row.Field<DateTime>("Payroll_SalaryMonth").Year == MonthYear.Year &&
                            row.Field<int>("EmployeeId") == EmployeeId);

                    if (matchingRow != null)
                    {
                        PayrollHoldId_Encrypted = matchingRow.Field<string>("PayrollHoldId_Encrypted");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetEncrypted_Id: {ex.Message}");
                PayrollHoldId_Encrypted = string.Empty;
            }
        }
        #endregion

        private int GetBranchId_Id(string employeeNo)
        {
            var GetEmployeeBranchId = "select EmployeeBranchId from Mas_Employee where Deactivate=0 and Employeeno='" + employeeNo + "'";
            var EmployeeBranchId = DapperORM.DynamicQuerySingle(GetEmployeeBranchId);
            int BranchId = Convert.ToInt32(EmployeeBranchId.EmployeeBranchId);
            if (BranchId != 0)
            {
                return BranchId;
            }
            else
            {
                return 0;
            }
        }

        #region Delete
        public ActionResult Delete(string PayrollHoldId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_PayrollHoldId_Encrypted", PayrollHoldId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_SalaryHold", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Module_Payroll_PayrollBulkHoldRelease");
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