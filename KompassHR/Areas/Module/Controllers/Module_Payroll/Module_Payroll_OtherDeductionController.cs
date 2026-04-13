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
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Payroll
{
    public class Module_Payroll_OtherDeductionController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();

        DataTable dt_ValidationCheck = new DataTable();
        DataTable dt_OtherDeduction = new DataTable();
        DataTable dt_Head_Variable = new DataTable();
        // GET: Module/Module_Payroll_OtherDeduction
        #region OtherDeduction MAin View
        //[HttpPost]
        public ActionResult Module_Payroll_OtherDeduction()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 161;
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
                ViewBag.GetHeadVariable = "";
                ViewBag.GetOtherDeductionName = "";
                ViewBag.GetOtherDeductionEmployeeLoad = "";
                ViewBag.GetBranchName = "";
                ViewBag.GetEmployeeList = "";
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region OtherDeduction MAin View
        [HttpPost]
        public ActionResult Module_Payroll_OtherDeduction(Payroll_OtherDeduction OBJOtherDeduction, HttpPostedFileBase AttachFile)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 161;
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
                ViewBag.GetHeadVariable = "";
                ViewBag.GetOtherDeductionName = "";
                ViewBag.GetOtherDeductionEmployeeLoad = "";
                ViewBag.GetBranchName = "";
                ViewBag.GetEmployeeList = "";

                var Query = "";
                if (AttachFile != null)
                {
                    if (AttachFile.ContentLength > 0)
                    {
                        List<Other_Deduction_Excel> excelDataList = new List<Other_Deduction_Excel>();
                        using (var stream = new MemoryStream())
                        {
                            // Load the uploaded file into the XLWorkbook
                            using (var attachStream = AttachFile.InputStream)
                            {
                                // Reset stream position if necessary
                                attachStream.Position = 0;

                                XLWorkbook xlWorkwook = new XLWorkbook(attachStream);

                                // Do any operations on workbook if necessary
                                xlWorkwook.SaveAs(stream);  // Save the workbook to memory stream
                                int row1 = 3;
                                var employeeNumbers = new HashSet<string>(); // To track unique Employee numbers
                                var duplicateEmployeeNumbers = new List<string>(); // To store duplicates

                                while (xlWorkwook.Worksheets.Worksheet(1).Cell(row1, 1).GetString() != "")
                                {
                                    string employeeNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row1, 1).GetString();

                                    if (!string.IsNullOrWhiteSpace(employeeNo))
                                    {
                                        if (!employeeNumbers.Add(employeeNo)) // EmployeeNo already exists
                                        {
                                            duplicateEmployeeNumbers.Add(employeeNo);
                                        }
                                    }
                                    row1++;
                                }



                                // Check for duplicates and handle them
                                //if (duplicateEmployeeNumbers.Any())
                                //{
                                //    TempData["Message"] = $"Duplicate Employee Numbers found: {string.Join(", ", duplicateEmployeeNumbers.Distinct())}";
                                //    TempData["Title"] = "Duplicate Employee Numbers Detected";
                                //    TempData["Icon"] = "error";
                                //    return RedirectToAction("Module_Payroll_OtherDeduction", "Module_Payroll_OtherDeduction");
                                //}
                                int row = 3;
                                if (xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString() == "")
                                {

                                    TempData["Message"] = "Fill in missing information in the first column.";
                                    TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                    TempData["Icon"] = "error";
                                    return RedirectToAction("Module_Payroll_OtherDeduction", "Module_Payroll_OtherDeduction");
                                }
                                while (xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString() != "")
                                {
                                    Other_Deduction_Excel OtherDeduction = new Other_Deduction_Excel();
                                    OtherDeduction.EmployeeNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString();
                                    OtherDeduction.EmployeeName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 2).GetString();
                                    OtherDeduction.CategoryName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 3).GetString();
                                    OtherDeduction.SubCategoryName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 4).GetString();
                                    OtherDeduction.Amount = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 5).GetString();
                                    OtherDeduction.Remark = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 6).GetString();

                                    excelDataList.Add(OtherDeduction);
                                    row++;
                                }
                                ViewBag.GetOtherDeductionEmployeeLoad = excelDataList;
                                //ViewBag.GetOtherDeductionEmployeeLoad = null;

                            }
                        }
                        if (OBJOtherDeduction.VariableDeductionCmpId != null)
                        {
                            DynamicParameters param = new DynamicParameters();
                            param.Add("@p_employeeid", Session["EmployeeId"]);
                            param.Add("@p_CmpId", OBJOtherDeduction.VariableDeductionCmpId);
                            var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                            ViewBag.GetBranchName = data;

                            DynamicParameters paramName = new DynamicParameters();
                            paramName.Add("@p_CmpId", OBJOtherDeduction.VariableDeductionCmpId);
                            var GetVeriableHead = DapperORM.ReturnList<PayrollDropDownBind>("sp_GetVariableHead_Deduction", paramName).ToList();
                            ViewBag.GetHeadVariable = GetVeriableHead;

                            DynamicParameters paramCat = new DynamicParameters();
                            paramCat.Add("@query", "Select DeductionCategory as Id,DeductionCategory as Name from Payroll_OtherDeduction where lower(VariableName)=lower('" + OBJOtherDeduction.VariableHead + "') and Deactivate=0 and CmpId=" + OBJOtherDeduction.VariableDeductionCmpId + "");
                            var GetVariableName = DapperORM.ReturnList<PayrollDropDownBind>("sp_QueryExcution", paramCat).ToList();
                            ViewBag.GetOtherDeductionName = GetVariableName;
                            if (OBJOtherDeduction.VariableDeductionBranchId != null)
                            {
                                DynamicParameters paramEMP = new DynamicParameters();
                                paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId='" + OBJOtherDeduction.VariableDeductionBranchId + "'and ContractorId=1 order by Name");
                                var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                                ViewBag.GetEmployeeList = GetEmployeeName;
                            }
                            else
                            {
                                DynamicParameters paramEMP = new DynamicParameters();
                                paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpId='" + OBJOtherDeduction.VariableDeductionCmpId + "' and ContractorId=1 order by Name");
                                var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                                ViewBag.GetEmployeeList = GetEmployeeName;
                            }
                        }
                    }

                }
                else if (OBJOtherDeduction.VariableDeductionCmpId != 0)
                {
                    if (OBJOtherDeduction.VariableDeductionBranchId != null)
                    {
                        DynamicParameters param = new DynamicParameters();
                        param.Add("@p_employeeid", Session["EmployeeId"]);
                        param.Add("@p_CmpId", OBJOtherDeduction.VariableDeductionCmpId);
                        var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                        ViewBag.GetBranchName = data;

                        DynamicParameters paramName = new DynamicParameters();
                        paramName.Add("@p_CmpId", OBJOtherDeduction.VariableDeductionCmpId);
                        var GetVeriableHead = DapperORM.ReturnList<PayrollDropDownBind>("sp_GetVariableHead_Deduction", paramName).ToList();
                        ViewBag.GetHeadVariable = GetVeriableHead;

                        DynamicParameters paramCat = new DynamicParameters();
                        paramCat.Add("@query", "Select DeductionCategory as Id,DeductionCategory as Name from Payroll_OtherDeduction where lower(VariableName)=lower('" + OBJOtherDeduction.VariableHead + "') and Deactivate=0 and CmpId=" + OBJOtherDeduction.VariableDeductionCmpId + "");
                        var GetVariableName = DapperORM.ReturnList<PayrollDropDownBind>("sp_QueryExcution", paramCat).ToList();
                        ViewBag.GetOtherDeductionName = GetVariableName;


                        Query = "and MAS_EMPLOYEE.EmployeeBranchId=" + OBJOtherDeduction.VariableDeductionBranchId + "";

                        DynamicParameters paramEMP = new DynamicParameters();
                        paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId=" + OBJOtherDeduction.VariableDeductionBranchId + " and ContractorId=1 order by Name");
                        var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                        ViewBag.GetEmployeeList = GetEmployeeName;
                    }
                    else
                    {
                        Query = " and MAs_employee.employeeBranchId in (select mas_branch.BranchID as Id from UserBranchMapping,mas_branch where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + OBJOtherDeduction.VariableDeductionCmpId + " and UserBranchMapping.IsActive = 1)";


                        DynamicParameters paramName = new DynamicParameters();
                        paramName.Add("@p_CmpId", OBJOtherDeduction.VariableDeductionCmpId);
                        var GetVeriableHead = DapperORM.ReturnList<PayrollDropDownBind>("sp_GetVariableHead_Deduction", paramName).ToList();
                        ViewBag.GetHeadVariable = GetVeriableHead;

                        DynamicParameters paramCat = new DynamicParameters();
                        paramCat.Add("@query", "Select DeductionCategory as Id,DeductionCategory as Name from Payroll_OtherDeduction where lower(VariableName)=lower('" + OBJOtherDeduction.VariableHead + "') and Deactivate=0 and CmpId=" + OBJOtherDeduction.VariableDeductionCmpId + "");
                        var GetVariableName = DapperORM.ReturnList<PayrollDropDownBind>("sp_QueryExcution", paramCat).ToList();
                        ViewBag.GetOtherDeductionName = GetVariableName;

                        DynamicParameters paramEMP = new DynamicParameters();
                        paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId  in (select mas_branch.BranchID as Id from UserBranchMapping,mas_branch where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + OBJOtherDeduction.VariableDeductionCmpId + " and UserBranchMapping.IsActive = 1) and ContractorId=1 order by Name");
                        var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                        ViewBag.GetEmployeeList = GetEmployeeName;

                    }

                    //var GetMonthYear = OBJOtherDeduction.OtherEarningMonthYear.ToString("yyyy-MM-dd");
                    var DeductionMonthYear = OBJOtherDeduction.MonthYear.ToString("yyyy-MM-dd");
                    if (OBJOtherDeduction.VariableDeductionEmployeeId == 0)
                    {
                        //Query = Query + "  and MAS_EMPLOYEE.ContractorID=1  and MAS_EMPLOYEE.EMPLOYEEID NOT IN (SELECT VariableDeductionEmployeeId FROM Payroll_VariableDeduction where Payroll_VariableDeduction.deactivate=0 and lower(Payroll_VariableDeduction.OtherDeductionName)=lower('" + OBJOtherDeduction.VariableHead + "')  AND MonthYear=FORMAT(convert(datetime," + OBJOtherDeduction.MonthYear.ToString("yyyy-MM-dd") + "),''))";
                        Query = Query + " and EmployeeLeft=0 and MAS_EMPLOYEE.ContractorID=1 ";
                    }
                    else
                    {
                        Query = Query + " and EmployeeLeft=0 and ContractorID=1 and MAS_EMPLOYEE.EMPLOYEEID =" + OBJOtherDeduction.VariableDeductionEmployeeId + "";
                    }
                    dt_Head_Variable = objcon.GetDataTable("exec sp_GetVariableHead_Deduction @p_CmpId=" + OBJOtherDeduction.VariableDeductionCmpId + "");
                    GetHead_Variable_Name(OBJOtherDeduction.VariableHead.ToString(), out CategoryName_Id);
                    List_CategoryName_Id = CategoryName_Id;

                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@P_Qry", Query);
                    paramList.Add("@p_VariableHead", List_CategoryName_Id);
                    paramList.Add("@p_SubVariableHead", OBJOtherDeduction.OtherDeductionName);
                    var OtherEarningEmployeeLoad = DapperORM.ExecuteSP<dynamic>("sp_GetVariableDeductionEmployee", paramList).ToList();
                    ViewBag.GetOtherDeductionEmployeeLoad = OtherEarningEmployeeLoad;
                }
                else
                {
                    ViewBag.GetOtherDeductionEmployeeLoad = "";
                    ViewBag.GetBranchName = "";
                    ViewBag.GetEmployeeList = "";
                }
                return View(OBJOtherDeduction);
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
                var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                DynamicParameters paramName = new DynamicParameters();
                paramName.Add("@p_CmpId", CmpId);
                var GetVeriableHead = DapperORM.ReturnList<PayrollDropDownBind>("sp_GetVariableHead_Deduction", paramName).ToList();
                return Json(new { BranchName, GetVeriableHead }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetEmployeeName
        [HttpGet]
        public ActionResult GetEmployeeName(int? BranchId, int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });

                }
                if (BranchId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeeLeft=0 and  EmployeeBranchId=" + BranchId + "  and ContractorId=1 order by Name");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    ViewBag.EmployeeList = data;
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeeLeft=0 and  EmployeeBranchId in     (select mas_branch.BranchID as Id from UserBranchMapping,mas_branch where UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + CmpId + " and UserBranchMapping.IsActive = 1)  and ContractorId=1 order by Name");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    ViewBag.EmployeeList = data;
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

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
        string List_CategoryName_Id = "";
        string CategoryName_Id = "";
        string List_SubCategoryName_Id = "";
        string SubCategoryName_Id = "";
        string BranchName = "";
        int List_Branch_Id = 0;
        int Branch_Id = 0;
        [HttpPost]
        public ActionResult SaveUpdate(List<Other_Deduction_Excel> ObjDeduction, int CmpId, DateTime MonthYear)
        //public ActionResult SaveUpdate(List<Other_Deduction_Excel> ObjDeduction, int CmpId, int BranchID, DateTime MonthYear, string VariableHead, string OtherDeductionName)
        {
            try
            {
                dt_ValidationCheck = objcon.GetDataTable("Select Mas_Employee.EmployeeName,Mas_Employee.EmployeeNo,Mas_Employee.EmployeeCardNo,Mas_Employee_Personal.AadhaarNo  ,Convert (int,Mas_Employee.EmployeeId) As EmployeeId,Mas_Branch.BranchName,Convert (int,Mas_Employee.EmployeeBranchId) AS  EmployeeBranchId from Mas_Employee inner join Mas_Employee_Personal on Mas_Employee.employeeid=Mas_Employee_Personal.personalEmployeeid inner join Mas_Branch on Mas_Employee.EmployeeBranchId=Mas_Branch.BranchId where Mas_Employee.Deactivate=0 and Mas_Employee_Personal.Deactivate=0 and Mas_Employee.CmpID=" + CmpId + "");
                dt_Head_Variable = objcon.GetDataTable("exec sp_GetVariableHead_Deduction @p_CmpId=" + CmpId + "");
                dt_OtherDeduction = objcon.GetDataTable("Select VariableName as Id,DeductionCategory as Name from Payroll_OtherDeduction where  Deactivate=0 and CmpId=" + CmpId + "");
                StringBuilder strBuilder = new StringBuilder();

                //var PayrollCount = DapperORM.DynamicQuerySingle("Select Count(PayrollLockId) as LockCount from Payroll_LOck where Deactivate=0 and PayrollLockBranchID=" + BranchID + " and Month(PayrollLockMonthYear)='" + MonthYear.ToString("MM") + "'  and Year(PayrollLockMonthYear) ='" + MonthYear.ToString("yyyy") + "' and Status=1");
                //if (PayrollCount.LockCount != 0)
                //{
                //    TempData["Message"] = "The record can't be saved because the payroll for this month ('" + MonthYear.ToString("MM-yyyy") + "') is locked.";
                //    TempData["Icon"] = "error";
                //    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                //}
                if (ObjDeduction != null)
                {

                    string str_Deduction = "";
                    string str_DeductionName = "";
                    foreach (var Data in ObjDeduction)
                    {
                        List_Employee_Id = 0;
                        Employee_Id = 0;
                        GetEmployeeId_Id(Data.EmployeeNo.ToString(), out Employee_Id, out Branch_Id, out BranchName);
                        List_Employee_Id = Employee_Id;
                        List_Branch_Id = Branch_Id;

                        //  var lockCheck = DapperORM.DynamicQueryList(
                        //    "SELECT COUNT(PayrollLockId) AS LockCount FROM Payroll_LOck WHERE Deactivate = 0 AND CmpId = " + CmpId +
                        //    " AND PayrollLockBranchID = " + List_Branch_Id +
                        //    " AND MONTH(PayrollLockMonthYear) = " + MonthYear.Month +
                        //    " AND YEAR(PayrollLockMonthYear) = " + MonthYear.Year +
                        //    " AND Status = 1"
                        //).FirstOrDefault();

                        //  if (lockCheck.LockCount != 0)
                        //  {
                        //      TempData["Message"] = "Payroll Lock for '" + MonthYear.ToString("MMM/yyyy") + " ' this Month and Company";
                        //      TempData["Icon"] = "error";
                        //      return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                        //  }

                        

                        if (Employee_Id == 0)
                        {
                            var Message = "The employee does not belong to this company (SrNo). " + Data.EmployeeNo;
                            var Icon = "error";
                            var rowno = Data.EmployeeNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        var PayrollCount = DapperORM.DynamicQuerySingle("Select Count(PayrollLockId) as LockCount from Payroll_LOck where Deactivate=0 and PayrollLockBranchID=" + List_Branch_Id + " and Month(PayrollLockMonthYear)='" + MonthYear.ToString("MM") + "'  and Year(PayrollLockMonthYear) ='" + MonthYear.ToString("yyyy") + "' and Status=1");
                        if (PayrollCount.LockCount != 0)
                        {
                            TempData["Message"] = "The record for employee " + Data.EmployeeNo + " cannot be saved because the payroll for '" + MonthYear.ToString("MMM-yyyy") + "' is locked for the " + BranchName + ". ";
                            TempData["Icon"] = "error";
                            return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                        }

                        GetHead_Variable_Id(Data.CategoryName.ToString(), out CategoryName_Id);
                        List_CategoryName_Id = CategoryName_Id;
                        if (List_CategoryName_Id == "")
                        {
                            var Message = "The category is not linked to the company of employee " + Data.EmployeeNo;
                            var Icon = "error";
                            var rowno = Data.EmployeeNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetOtherEarning_Id(Data.SubCategoryName.ToString(), List_CategoryName_Id, out SubCategoryName_Id);
                        List_SubCategoryName_Id = SubCategoryName_Id;
                        if (List_SubCategoryName_Id == "")
                        {
                            var Message = "Sub-Category Name is does not belong to this Category" + Data.EmployeeNo;
                            var Icon = "error";
                            var rowno = Data.EmployeeNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        str_Deduction = str_Deduction + "'" + List_Employee_Id + "',";
                        str_DeductionName = str_DeductionName + "'" + List_SubCategoryName_Id + "',";
                    }

                    // Ensure str_Deduction and str_DeductionName are correctly formatted as comma-separated values (numeric)
                    if (str_Deduction != "")
                    {
                        str_Deduction = str_Deduction.Substring(0, str_Deduction.Length - 1);
                    }
                    if (str_DeductionName != "")
                    {
                        str_DeductionName = str_DeductionName.Substring(0, str_DeductionName.Length - 1);
                    }

                    // Split the comma-separated strings into arrays



                    var ListEmployeeNo = DapperORM.DynamicQueryList("SELECT CompanyName AS Company, BranchName AS BU, EmployeeNo,            EmployeeName AS Employee, DepartmentName      FROM Mas_Employee    JOIN Mas_CompanyProfile ON Mas_Employee.CmpID = Mas_CompanyProfile.CompanyId    JOIN Mas_Branch ON Mas_Employee.EmployeeBranchId = Mas_Branch.BranchId    JOIN Mas_Department ON Mas_Employee.EmployeeDepartmentID = Mas_Department.DepartmentId    JOIN Payroll_VariableDeduction ON Mas_Employee.EmployeeId = Payroll_VariableDeduction.VariableDeductionEmployeeId    WHERE Mas_Employee.Deactivate = 0       AND Mas_Branch.Deactivate = 0       AND Mas_Department.Deactivate = 0       AND Payroll_VariableDeduction.Deactivate = 0       AND Payroll_VariableDeduction.VariableDeductionEmployeeId IN (" + str_Deduction + ")    AND Payroll_VariableDeduction.OtherDeductionName IN (" + str_DeductionName + ") AND MONTH(Payroll_VariableDeduction.MonthYear) = " + MonthYear.Month + " AND YEAR(Payroll_VariableDeduction.MonthYear) = " + MonthYear.Year + "").ToList();


                    if (ListEmployeeNo.Count != 0)
                    {
                        ViewBag.GetListEmployeeNo = ListEmployeeNo;
                        return Json(new { ViewBag.GetListEmployeeNo }, JsonRequestBehavior.AllowGet);
                    }


                    foreach (var Data in ObjDeduction)
                    {

                        List_Employee_Id = 0;
                        Employee_Id = 0;
                        GetEmployeeId_Id(Data.EmployeeNo.ToString(), out Employee_Id, out Branch_Id, out BranchName);
                        List_Employee_Id = Employee_Id;
                        List_Branch_Id = Branch_Id;
                        if (Employee_Id == 0)
                        {
                            var Message = "The employee does not belong to this company (SrNo). " + Data.EmployeeNo;
                            var Icon = "error";
                            var rowno = Data.EmployeeNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        var PayrollCount = DapperORM.DynamicQueryList("Select Count(PayrollLockId) as LockCount from Payroll_LOck where Deactivate=0 and PayrollLockBranchID=" + List_Branch_Id + " and Month(PayrollLockMonthYear)='" + MonthYear.ToString("MM") + "'  and Year(PayrollLockMonthYear) ='" + MonthYear.ToString("yyyy") + "' and Status=1").FirstOrDefault();
                        if (PayrollCount.LockCount != 0)
                        {
                            TempData["Message"] = "The record for employee " + Data.EmployeeNo + " cannot be saved because the payroll for '" + MonthYear.ToString("MMM-yyyy") + "' is locked for the " + BranchName + ". ";
                            TempData["Icon"] = "error";
                            return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                        }

                        GetHead_Variable_Id(Data.CategoryName.ToString(), out CategoryName_Id);
                        List_CategoryName_Id = CategoryName_Id;
                        if (List_CategoryName_Id == "")
                        {
                            var Message = "The category is not linked to the company of employee " + Data.EmployeeNo;
                            var Icon = "error";
                            var rowno = Data.EmployeeNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetOtherEarning_Id(Data.SubCategoryName.ToString(), List_CategoryName_Id, out SubCategoryName_Id);
                        List_SubCategoryName_Id = SubCategoryName_Id;
                        if (List_SubCategoryName_Id == "")
                        {
                            var Message = "Sub-Category Name is does not belong to this Category" + Data.EmployeeNo;
                            var Icon = "error";
                            var rowno = Data.EmployeeNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        string StringDeduction = " Insert Into Payroll_VariableDeduction(" +
                                              "  Deactivate  " +
                                              " , CreatedBy  " +
                                              " , CreatedDate  " +
                                              " , MachineName  " +
                                              " , VariableDeductionCmpId  " +
                                              " , VariableDeductionBranchId  " +
                                              " , MonthYear  " +
                                              " , VariableHead  " +
                                               " , VariableDeductionEmployeeId  " +
                                              " , Amount  " +
                                              " , SalaryLock  " +
                                              " , Remarks  " +
                                              " , OtherDeductionName " + ")  values (" +
                                               "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                               "'" + CmpId + "'," +
                                               "'" + List_Branch_Id + "'," +
                                               "'" + MonthYear.ToString("yyyy-MM-dd") + "'," +
                                               "'" + List_CategoryName_Id + "'," +
                                               "'" + List_Employee_Id + "'," +
                                               "'" + Data.Amount + "'," +
                                               "'0'," +
                                               "'" + Data.Remark + "'," +
                                               "'" + List_SubCategoryName_Id + "')" +
                                                " " +
                                               " " +
                                                " ";
                        strBuilder.Append(StringDeduction);

                    }
                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record save successfully";
                        TempData["Icon"] = "success";
                    }
                    if (abc != "")
                    {
                        DapperORM.DynamicQuerySingle("Insert Into Tool_ErrorLog ( " +
                                                                            "   Error_Desc " +
                                                                            " , Error_FormName " +
                                                                            " , Error_MachinceName " +
                                                                            " , Error_Date " +
                                                                            " , Error_UserID " +
                                                                            " , Error_UserName " + ") values (" +
                                                                            "'" + strBuilder + "'," +
                                                                            "'BuklInsert'," +
                                                                            "'" + Dns.GetHostName().ToString() + "'," +
                                                                            "GetDate()," +
                                                                            "'" + Session["EmployeeId"] + "'," +
                                                                            "'" + Session["EmployeeName"] + "'");
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
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
        public ActionResult GetList(Other_Deduction_List OtherDeduction_List)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 161;
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
                ViewBag.GetHeadVariable = "";

                if (OtherDeduction_List.OtherDeductionMonthYear != DateTime.MinValue)
                {

                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_employeeid", Session["EmployeeId"]);
                    param.Add("@p_CmpId", OtherDeduction_List.CmpId);
                    var data1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                    ViewBag.GetBranchName = data1;

                    DynamicParameters paramName = new DynamicParameters();
                    paramName.Add("@p_CmpId", OtherDeduction_List.CmpId);
                    var GetVeriableHead = DapperORM.ReturnList<PayrollDropDownBind>("sp_GetVariableHead_Deduction", paramName).ToList();
                    ViewBag.GetHeadVariable = GetVeriableHead;

                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_Origin", "List");
                    paramList.Add("@p_BranchId", OtherDeduction_List.BranchId);
                    paramList.Add("@p_VariableHead", OtherDeduction_List.VariableHead);
                    paramList.Add("@p_MonthYear", OtherDeduction_List.OtherDeductionMonthYear.ToString("yyyy-MM-dd"));
                    var data = DapperORM.DynamicList("sp_List_Payroll_VariableDeduction", paramList);
                    ViewBag.OtherDeduction = data;
                }
                else
                {
                    ViewBag.OtherDeduction = "";
                }

                return View(OtherDeduction_List);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete Event
        public ActionResult Delete(List<Payroll_OtherDeduction> ObjDelete)
        {
            try
            {
                StringBuilder strBuilder = new StringBuilder();
                if (ObjDelete != null)
                {
                    foreach (var Data in ObjDelete)
                    {
                        string StringMonthlyAttendance = " Update  Payroll_VariableDeduction set Deactivate=1,ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=GETDATE() where  VariableDeductionId=" + Data.VariableDeductionId + " and SalaryLock='0'    ";
                        strBuilder.Append(StringMonthlyAttendance);

                    }
                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record delete successfully";
                        TempData["Icon"] = "success";
                        //  TempData["Month"] =Month;
                    }
                    if (abc != "")
                    {
                        DapperORM.DynamicQuerySingle("Insert Into Tool_ErrorLog ( " +
                                                                            "   Error_Desc " +
                                                                            " , Error_FormName " +
                                                                            " , Error_MachinceName " +
                                                                            " , Error_Date " +
                                                                            " , Error_UserID " +
                                                                            " , Error_UserName " + ") values (" +
                                                                            "'" + strBuilder + "'," +
                                                                            "'VariableDeduction'," +
                                                                            "'" + Dns.GetHostName().ToString() + "'," +
                                                                            "GetDate()," +
                                                                            "'" + Session["EmployeeId"] + "'," +
                                                                            "'" + Session["EmployeeName"] + "'");
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"], Month = TempData["Month"] }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);

                //param.Add("@p_process", "Delete");
                //param.Add("@p_OtherDeductionID_Encrypted", OtherDeductionID_Encrypted);
                //param.Add("@p_MachineName", Dns.GetHostName().ToString());
                //param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                //param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                //param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                //var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_OtherDeduction", param);
                //TempData["Message"] = param.Get<string>("@p_msg");
                //TempData["Icon"] = param.Get<string>("@p_Icon");
                //return RedirectToAction("GetList", "Module_Payroll_OtherDeduction", new { Area = "Module" });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetOtherDeductionName
        [HttpGet]
        public ActionResult GetOtherDeductionName(string VariableName, int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();

                param.Add("@query", "Select DeductionCategory as Id,DeductionCategory as Name from Payroll_OtherDeduction where lower(VariableName)=lower('" + VariableName + "') and Deactivate=0 and CmpId=" + CmpId + "");
                var GetVariableName = DapperORM.ReturnList<PayrollDropDownBind>("sp_QueryExcution", param).ToList();

                return Json(GetVariableName, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetEmployeeId_Id
        void GetEmployeeId_Id(string EmployeeNo, out int Employee_Id, out int Branch_Id, out string BranchName)
        {
            Employee_Id = 0;
            Branch_Id = 0;

            try
            {
                // Check if EmployeeNo exists
                var matchingRow = dt_ValidationCheck.AsEnumerable()
                 .FirstOrDefault(row =>
                string.Equals(
                    (row.Field<string>("EmployeeNo") ?? string.Empty).Replace(" ", "").Trim(),
                    (EmployeeNo ?? string.Empty).Replace(" ", "").Trim(),
                    StringComparison.OrdinalIgnoreCase));

                if (matchingRow != null)
                {
                    Employee_Id = matchingRow.Field<int?>("EmployeeId") ?? 0;
                    Branch_Id = matchingRow.Field<int?>("EmployeeBranchId") ?? 0;
                    BranchName = matchingRow.Field<string>("BranchName") ?? string.Empty;
                }
                else
                {
                    Employee_Id = 0;
                    Branch_Id = 0;
                    BranchName = string.Empty;
                }

                //if (matchingRow != null)
                //{
                //    Employee_Id = matchingRow.Field<int>("EmployeeId");
                //    Branch_Id = matchingRow.Field<int>("EmployeeBranchId");
                //    BranchName = matchingRow.Field<string>("BranchName");
                //}
                //else
                //{
                //    Employee_Id = 0;
                //    Branch_Id = 0;
                //    BranchName = "";
                //}
            }
            catch (Exception ex)
            {
                // Log exception (if necessary)
                Console.WriteLine($"Error: {ex.Message}");
                Employee_Id = 0;
                Branch_Id = 0;
                BranchName = "";
            }
        }


        void GetHead_Variable_Id(string CategoryName, out string CategoryName_Id)
        {
            try
            {
                // Check if the CategoryName exists in the dt_Head_Variable
                var matchingRow = dt_Head_Variable.AsEnumerable()
                    .FirstOrDefault(row =>
                        string.Equals(row.Field<string>("Name"), CategoryName, StringComparison.OrdinalIgnoreCase));

                if (matchingRow != null)
                {
                    // If found, assign the Id as string to CategoryName_Id
                    CategoryName_Id = matchingRow.Field<string>("Id");
                }
                else
                {
                    // If not found, assign an empty string
                    CategoryName_Id = string.Empty;
                }
            }
            catch (Exception ex)
            {
                // Log exception (if necessary) and set default value
                Console.WriteLine($"Error: {ex.Message}");
                CategoryName_Id = string.Empty;
            }
        }

        void GetHead_Variable_Name(string CategoryName, out string CategoryName_Id)
        {
            try
            {
                // Check if the CategoryName exists in the dt_Head_Variable
                var matchingRow = dt_Head_Variable.AsEnumerable()
                    .FirstOrDefault(row =>
                        string.Equals(row.Field<string>("Id"), CategoryName, StringComparison.OrdinalIgnoreCase));

                if (matchingRow != null)
                {
                    // If found, assign the Id as string to CategoryName_Id
                    CategoryName_Id = matchingRow.Field<string>("Name");
                }
                else
                {
                    // If not found, assign an empty string
                    CategoryName_Id = string.Empty;
                }
            }
            catch (Exception ex)
            {
                // Log exception (if necessary) and set default value
                Console.WriteLine($"Error: {ex.Message}");
                CategoryName_Id = string.Empty;
            }
        }


        void GetOtherEarning_Id(string subCategoryName, string listCategoryNameId, out string subCategoryNameId)
        {
            try
            {
                // Check if the SubCategory and List_Category ID match any row in dt_OtherDeduction
                var matchingRow = dt_OtherDeduction.AsEnumerable()
                    .FirstOrDefault(row =>
                        string.Equals(row.Field<string>("Name"), subCategoryName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(row.Field<string>("Id"), listCategoryNameId, StringComparison.OrdinalIgnoreCase));

                if (matchingRow != null)
                {
                    // Assign the Id (or any other field you need to return)
                    subCategoryNameId = matchingRow.Field<string>("Name");
                }
                else
                {
                    subCategoryNameId = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                subCategoryNameId = string.Empty;
            }
        }
        #endregion
    }
}