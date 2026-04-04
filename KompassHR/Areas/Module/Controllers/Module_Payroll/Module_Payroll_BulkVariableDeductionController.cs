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
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Payroll
{
    public class Module_Payroll_BulkVariableDeductionController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();

        DataTable dt_ValidationCheck = new DataTable();
        DataTable dt_BranchValidationCheck =new DataTable();
        DataTable dt_OtherDeduction = new DataTable();

        DataTable dt_Head_Variable = new DataTable();
        
        #region Module_Payroll_BulkVariableDeduction MAin View
        //[HttpPost]
        public ActionResult Module_Payroll_BulkVariableDeduction()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 853;
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
        public ActionResult Module_Payroll_BulkVariableDeduction(Payroll_OtherDeduction OBJOtherDeduction, HttpPostedFileBase AttachFile)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 853;
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
                                
                                int row = 3;
                                if (xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString() == "")
                                {

                                    TempData["Message"] = "Fill in missing information in the first column.";
                                    TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                    TempData["Icon"] = "error";
                                    return RedirectToAction("Module_Payroll_BulkVariableDeduction", "Module_Payroll_BulkVariableDeduction");
                                }
                                while (xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString() != "")
                                {
                                    Other_Deduction_Excel OtherDeduction = new Other_Deduction_Excel();
                                    OtherDeduction.EmployeeNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString();
                                    OtherDeduction.EmployeeName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 2).GetString();
                                    //OtherDeduction.CompanyName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 3).GetString();
                                    OtherDeduction.BranchName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 3).GetString();

                                    OtherDeduction.CategoryName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 4).GetString();
                                    OtherDeduction.SubCategoryName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 5).GetString();
                                    OtherDeduction.Amount = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 6).GetString();
                                    OtherDeduction.Remark = xlWorkwook.Worksheets.Worksheet(1).Cell(row,7).GetString();

                                    excelDataList.Add(OtherDeduction);
                                    row++;
                                }
                                ViewBag.GetOtherDeductionEmployeeLoad = excelDataList;

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

                            DynamicParameters paramEMP = new DynamicParameters();
                            paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId='" + OBJOtherDeduction.VariableDeductionBranchId + "'and ContractorId=1 order by Name");
                            var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                            ViewBag.GetEmployeeList = GetEmployeeName;
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

                    var DeductionMonthYear = OBJOtherDeduction.MonthYear.ToString("yyyy-MM-dd");
                    if (OBJOtherDeduction.VariableDeductionEmployeeId == 0)
                    {
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
        {
            try
            {
                dt_ValidationCheck = objcon.GetDataTable("Select Mas_Employee.EmployeeName,Mas_Employee.EmployeeNo,Mas_Employee.EmployeeCardNo,Mas_Employee_Personal.AadhaarNo  ,Convert (int,Mas_Employee.EmployeeId) As EmployeeId,Mas_Branch.BranchName,Convert (int,Mas_Employee.EmployeeBranchId) AS  EmployeeBranchId from Mas_Employee inner join Mas_Employee_Personal on Mas_Employee.employeeid=Mas_Employee_Personal.personalEmployeeid inner join Mas_Branch on Mas_Employee.EmployeeBranchId=Mas_Branch.BranchId where Mas_Employee.Deactivate=0 and Mas_Employee_Personal.Deactivate=0 and Mas_Employee.CmpID=" + CmpId + "");
                dt_BranchValidationCheck = objcon.GetDataTable("select BranchId,BranchName from Mas_Branch where Deactivate=0 and CmpId=" + CmpId + "");
                dt_Head_Variable = objcon.GetDataTable("exec sp_GetVariableHead_Deduction @p_CmpId=" + CmpId + "");
                dt_OtherDeduction = objcon.GetDataTable("Select VariableName as Id,DeductionCategory as Name from Payroll_OtherDeduction where  Deactivate=0 and CmpId=" + CmpId + "");
                StringBuilder strBuilder = new StringBuilder();

                if (ObjDeduction != null)
                {

                    string str_Deduction = "";
                    string str_DeductionName = "";
                    string str_Branch = "";

                    foreach (var Data in ObjDeduction)
                    {
                        List_Employee_Id = 0;
                        Employee_Id = 0;
                        //GetEmployeeId_Id(Data.EmployeeNo.ToString(), out Employee_Id, out Branch_Id, out BranchName);
                        GetEmployeeId_Id(Data.EmployeeNo, out Employee_Id);

                        GetBranchId_FromExcel(Data.BranchName, out Branch_Id);
                        List_Employee_Id = Employee_Id;
                        List_Branch_Id = Branch_Id;
                        
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
                        str_Branch = str_Branch + "'" + List_Branch_Id + "',";
                    }

                    if (str_Deduction != "")
                    {
                        str_Deduction = str_Deduction.Substring(0, str_Deduction.Length - 1);
                    }
                    if (str_DeductionName != "")
                    {
                        str_DeductionName = str_DeductionName.Substring(0, str_DeductionName.Length - 1);
                    }

                    if (str_Branch != "")
                    {
                        str_Branch = str_Branch.Substring(0, str_Branch.Length - 1);
                    }

                    //var ListEmployeeNo = DapperORM.DynamicQueryList("SELECT CompanyName AS Company, BranchName AS BU, EmployeeNo,            EmployeeName AS Employee, DepartmentName      FROM Mas_Employee    JOIN Mas_CompanyProfile ON Mas_Employee.CmpID = Mas_CompanyProfile.CompanyId    JOIN Mas_Branch ON Mas_Employee.EmployeeBranchId = Mas_Branch.BranchId    JOIN Mas_Department ON Mas_Employee.EmployeeDepartmentID = Mas_Department.DepartmentId    JOIN Payroll_VariableDeduction ON Mas_Employee.EmployeeId = Payroll_VariableDeduction.VariableDeductionEmployeeId    WHERE Mas_Employee.Deactivate = 0       AND Mas_Branch.Deactivate = 0       AND Mas_Department.Deactivate = 0       AND Payroll_VariableDeduction.Deactivate = 0       AND Payroll_VariableDeduction.VariableDeductionEmployeeId IN (" + str_Deduction + ")    AND Payroll_VariableDeduction.OtherDeductionName IN (" + str_DeductionName + ") AND MONTH(Payroll_VariableDeduction.MonthYear) = " + MonthYear.Month + " AND YEAR(Payroll_VariableDeduction.MonthYear) = " + MonthYear.Year + "").ToList();
                    var ListEmployeeNo = DapperORM.DynamicQueryList("SELECT  C.CompanyName AS Company, B.BranchName AS BU, E.EmployeeNo, E.EmployeeName AS Employee, D.DepartmentName FROM Payroll_VariableDeduction P JOIN Mas_Employee E ON P.VariableDeductionEmployeeId = E.EmployeeId JOIN Mas_CompanyProfile C ON P.VariableDeductionCmpId = C.CompanyId JOIN Mas_Branch B ON P.VariableDeductionBranchId = B.BranchId JOIN Mas_Department D  ON E.EmployeeDepartmentID = D.DepartmentId WHERE P.Deactivate = 0 AND E.Deactivate = 0 AND B.Deactivate = 0  AND D.Deactivate = 0  AND P.VariableDeductionEmployeeId IN(" + str_Deduction + ") AND  P.VariableDeductionBranchId IN ("+ str_Branch + ") AND P.OtherDeductionName IN(" + str_DeductionName + ") AND MONTH(P.MonthYear) = " + MonthYear.Month + " AND YEAR(P.MonthYear) = " + MonthYear.Year + "").ToList();

  
                    if (ListEmployeeNo.Count != 0)
                    {
                        ViewBag.GetListEmployeeNo = ListEmployeeNo;
                        return Json(new { ViewBag.GetListEmployeeNo }, JsonRequestBehavior.AllowGet);
                    }


                    foreach (var Data in ObjDeduction)
                    {

                        List_Employee_Id = 0;
                        Employee_Id = 0;
                        //GetEmployeeId_Id(Data.EmployeeNo.ToString(), out Employee_Id, out Branch_Id, out BranchName);
                        GetEmployeeId_Id(Data.EmployeeNo, out Employee_Id);
                        GetBranchId_FromExcel(Data.BranchName, out Branch_Id);

                        List_Employee_Id = Employee_Id;
                        List_Branch_Id = Branch_Id;


                        if (Employee_Id == 0)
                        {
                            var Message = "The employee does not belong to this company (SrNo). " + Data.EmployeeNo;
                            var Icon = "error";
                            var rowno = Data.EmployeeNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        if (List_Branch_Id == 0 || Branch_Id == 0)
                        {

                            var Message = "Invalid Branch in Excel for Employee : " + Data.EmployeeNo;
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

        #region GetEmployeeId_Id
        //void GetEmployeeId_Id(string EmployeeNo, out int Employee_Id, out int Branch_Id, out string BranchName)
        void GetEmployeeId_Id(string EmployeeNo, out int Employee_Id)
        {
            Employee_Id = 0;
       
            try
            {
                //var matchingRow = dt_ValidationCheck.AsEnumerable()
                // .FirstOrDefault(row =>
                //string.Equals(
                //    (row.Field<string>("EmployeeNo") ?? string.Empty).Replace(" ", "").Trim(),
                //    (EmployeeNo ?? string.Empty).Replace(" ", "").Trim(),
                //    StringComparison.OrdinalIgnoreCase));

                string excelNumber = (EmployeeNo ?? "")
                                    .Replace("\u00A0", "")
                                    .Replace("\t", "")
                                    .Replace("\n", "")
                                    .Replace(" ", "")
                                    .Trim()
                                   .TrimStart('0');

                var matchingRow = dt_ValidationCheck.AsEnumerable()
                     .FirstOrDefault(row =>
                     {
                         string dbNumber = (row.Field<string>("EmployeeNo") ?? "")
                             .Replace("\u00A0", "")
                             .Replace("\t", "")
                             .Replace("\n", "")
                             .Replace(" ", "")
                             .Trim()
                             .TrimStart('0');


                         return string.Equals(dbNumber, excelNumber,
                          StringComparison.OrdinalIgnoreCase);
                     });

                if (matchingRow != null)
                {
                    Employee_Id = matchingRow.Field<int?>("EmployeeId") ?? 0;
               
                }
                else
                {
                    Employee_Id = 0;
             
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Employee_Id = 0;
                Branch_Id = 0;
                BranchName = "";
            }
        }

        void GetBranchId_FromExcel(string branchName, out int Branch_Id)
        {
            Branch_Id = 0;
            try
            {
                if (string.IsNullOrEmpty(branchName))
                {
                    Branch_Id = 0;
                    return;
                }

                // Excel value as-is
                string excelBranch = branchName;

                foreach (DataRow row in dt_BranchValidationCheck.Rows)
                {
                    string dbBranch = row["BranchName"].ToString();

                    // EXACT MATCH only
                    if (string.Equals(excelBranch, dbBranch, StringComparison.OrdinalIgnoreCase))
                    {
                        Branch_Id = Convert.ToInt32(row["BranchId"]);
                        return;
                    }
                }

                Branch_Id = 0; // not found
            }
            catch
            {
                Branch_Id = 0;
            }
        }


        //void GetBranchId_FromExcel(string branchName, out int Branch_Id)
        //{
        //    Branch_Id = 0;

        //    try
        //    {


        //        //    var row = dt_BranchValidationCheck.AsEnumerable()
        //        //        .FirstOrDefault(r =>
        //        //            string.Equals(
        //        //                (r.Field<string>("BranchName") ?? "").Trim(),
        //        //                (branchName ?? "").Trim(),
        //        //                StringComparison.OrdinalIgnoreCase));

        //        //    if (row != null)
        //        //        // Branch_Id = row.Field<int?>("EmployeeBranchId") ?? 0;
        //        //        Branch_Id = row.Field<int?>("BranchId") ?? 0;
        //        //}


        //        string excelBranch = (branchName ?? "")
        //        .Replace("\u00A0", "")
        //        .Replace("\t", "")
        //        .Replace("\n", "")
        //        .Replace(" ", "")
        //        .Trim()
        //        .ToLower();

        //        var row = dt_BranchValidationCheck.AsEnumerable()
        //            .FirstOrDefault(r =>
        //            {
        //                string dbBranch = (r.Field<string>("BranchName") ?? "")
        //                    .Replace("\u00A0", "")
        //                    .Replace("\t", "")
        //                    .Replace("\n", "")
        //                    .Replace(" ", "")
        //                    .Trim()
        //                    .ToLower();

        //                return dbBranch == excelBranch;
        //            });

        //        if (row != null)
        //            Branch_Id = row.Field<int?>("BranchId") ?? 0;

        //    }
        //    catch
        //    {
        //        Branch_Id = 0;
        //    }
        //}


        void GetHead_Variable_Id(string CategoryName, out string CategoryName_Id)
        {
           
            try
            {
                
                var matchingRow = dt_Head_Variable.AsEnumerable()
                    .FirstOrDefault(row =>
                        string.Equals(row.Field<string>("Name"), CategoryName, StringComparison.OrdinalIgnoreCase));

                if (matchingRow != null)
                {
                  //  CategoryName_Id = (matchingRow["Id"] + "").Trim();
                    CategoryName_Id = matchingRow.Field<string>("Id");
                }
                else
                {
                    CategoryName_Id = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                CategoryName_Id = string.Empty;
            }
        }

        void GetHead_Variable_Name(string CategoryName, out string CategoryName_Id)
        {
            try
            {
                var matchingRow = dt_Head_Variable.AsEnumerable()
                    .FirstOrDefault(row =>
                        string.Equals(row.Field<string>("Id"), CategoryName, StringComparison.OrdinalIgnoreCase));

                if (matchingRow != null)
                {
                    CategoryName_Id = matchingRow.Field<string>("Name");
                }
                else
                {
                    CategoryName_Id = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                CategoryName_Id = string.Empty;
            }
        }


        void GetOtherEarning_Id(string subCategoryName, string listCategoryNameId, out string subCategoryNameId)
        {
            try
            {
                var matchingRow = dt_OtherDeduction.AsEnumerable()
                    .FirstOrDefault(row =>
                        string.Equals(row.Field<string>("Name"), subCategoryName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(row.Field<string>("Id"), listCategoryNameId, StringComparison.OrdinalIgnoreCase));

                if (matchingRow != null)
                {
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