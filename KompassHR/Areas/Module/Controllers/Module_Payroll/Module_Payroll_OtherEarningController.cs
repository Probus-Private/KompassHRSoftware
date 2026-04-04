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
    public class Module_Payroll_OtherEarningController : Controller
    {
        // GET: Module/Module_Payroll_OtherEarning

        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        //  BulkAccessClass obj = new BulkAccessClass();
        clsCommonFunction objcon = new clsCommonFunction();
        DataTable dt_ValidationCheck = new DataTable();
        DataTable dt_OtherEarning = new DataTable();
        DataTable dt_Head_Variable = new DataTable();
        #region OtherEarning Main VIew

        public ActionResult Module_Payroll_OtherEarning()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 160;
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
                ViewBag.GetOtherEarningName = "";
                ViewBag.GetBranchName = "";
                ViewBag.GetEmployeeList = "";
                ViewBag.GetOtherEarningEmployeeLoad = "";
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region OtherEarning Main VIew
        [HttpPost]
        public ActionResult Module_Payroll_OtherEarning(Payroll_OtherEarning OBJOtherEarning, HttpPostedFileBase AttachFile)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 160;
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
                ViewBag.GetOtherEarningName = "";
                ViewBag.GetEmployeeList = "";

                var Query = "";
                if (AttachFile != null)
                {
                    if (AttachFile.ContentLength > 0)
                    {
                        List<Other_Earning_Excel> excelDataList = new List<Other_Earning_Excel>();
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

                                //// Check for duplicates and handle them
                                //if (duplicateEmployeeNumbers.Any())
                                //{
                                //    TempData["Message"] = $"Duplicate Employee Numbers found: {string.Join(", ", duplicateEmployeeNumbers.Distinct())}";
                                //    TempData["Title"] = "Duplicate Employee Numbers Detected";
                                //    TempData["Icon"] = "error";
                                //    return RedirectToAction("Module_Payroll_OtherEarning", "Module_Payroll_OtherEarning");
                                //}
                                int row = 3;
                                if (xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString() == "")
                                {

                                    TempData["Message"] = "Fill in missing information in the first column.";
                                    TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                    TempData["Icon"] = "error";
                                    return RedirectToAction("Module_Payroll_OtherEarning", "Module_Payroll_OtherEarning");
                                }
                                while (xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString() != "")
                                {
                                    Other_Earning_Excel Other_Earning = new Other_Earning_Excel();
                                    Other_Earning.EmployeeNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString();
                                    Other_Earning.EmployeeName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 2).GetString();
                                    Other_Earning.CategoryName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 3).GetString();
                                    Other_Earning.SubCategoryName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 4).GetString();
                                    Other_Earning.Amount = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 5).GetString();
                                    Other_Earning.Remark = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 6).GetString();

                                    excelDataList.Add(Other_Earning);
                                    row++;
                                }
                                ViewBag.GetOtherEarningEmployeeLoad = excelDataList;
                                //ViewBag.GetOtherDeductionEmployeeLoad = null;

                            }
                        }
                        if (OBJOtherEarning.VariableEarningCmpId != null)
                        {
                            DynamicParameters param = new DynamicParameters();
                            param.Add("@p_employeeid", Session["EmployeeId"]);
                            param.Add("@p_CmpId", OBJOtherEarning.VariableEarningCmpId);
                            var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                            ViewBag.GetBranchName = data;

                            DynamicParameters paramCat = new DynamicParameters();
                            paramCat.Add("@query", "Select EarningCategory as Id,EarningCategory as Name from Payroll_OtherEarning where lower(VariableName)=lower('" + OBJOtherEarning.VariableHead + "') and Deactivate=0 and CmpId=" + OBJOtherEarning.VariableEarningCmpId + "");
                            var GetVariableName = DapperORM.ReturnList<PayrollDropDownBind>("sp_QueryExcution", paramCat).ToList();
                            ViewBag.GetOtherEarningName = GetVariableName;

                            DynamicParameters paramName = new DynamicParameters();
                            paramName.Add("@p_CmpId", OBJOtherEarning.VariableEarningCmpId);
                            var GetVeriableHead = DapperORM.ReturnList<PayrollDropDownBind>("sp_GetVariableHead_Earning", paramName).ToList();
                            ViewBag.GetHeadVariable = GetVeriableHead;

                            if (OBJOtherEarning.VariableEarningBranchId != null)
                            {
                                DynamicParameters paramEMP = new DynamicParameters();
                                paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId='" + OBJOtherEarning.VariableEarningBranchId + "' and ContractorId=1 order by Name");
                                var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                                ViewBag.GetEmployeeList = GetEmployeeName;
                            }

                        }
                    }

                }
                else if (OBJOtherEarning.VariableEarningCmpId != 0)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_employeeid", Session["EmployeeId"]);
                    param.Add("@p_CmpId", OBJOtherEarning.VariableEarningCmpId);
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                    ViewBag.GetBranchName = data;

                    DynamicParameters paramCat = new DynamicParameters();

                    paramCat.Add("@query", "Select EarningCategory as Id,EarningCategory as Name from Payroll_OtherEarning where lower(VariableName)=lower('" + OBJOtherEarning.VariableHead + "') and Deactivate=0 and CmpId=" + OBJOtherEarning.VariableEarningCmpId + "");
                    var GetVariableName = DapperORM.ReturnList<PayrollDropDownBind>("sp_QueryExcution", paramCat).ToList();
                    ViewBag.GetOtherEarningName = GetVariableName;

                    DynamicParameters paramName = new DynamicParameters();
                    paramName.Add("@p_CmpId", OBJOtherEarning.VariableEarningCmpId);
                    var GetVeriableHead = DapperORM.ReturnList<PayrollDropDownBind>("sp_GetVariableHead_Earning", paramName).ToList();
                    ViewBag.GetHeadVariable = GetVeriableHead;

                    if (OBJOtherEarning.VariableEarningBranchId != null)
                    {
                        Query = " and MAS_EMPLOYEE.EmployeeBranchId= " + OBJOtherEarning.VariableEarningBranchId + "";

                        DynamicParameters paramEMP = new DynamicParameters();
                        paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId='" + OBJOtherEarning.VariableEarningBranchId + "' order by Name");
                        var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                        ViewBag.GetEmployeeList = GetEmployeeName;
                    }
                    else
                    {
                        Query = "and MAs_employee.employeeBranchId in (select mas_branch.BranchID as Id from UserBranchMapping,mas_branch where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + OBJOtherEarning.VariableEarningCmpId + " and UserBranchMapping.IsActive = 1)";

                        DynamicParameters paramCa1t = new DynamicParameters();

                        paramCa1t.Add("@query", "Select EarningCategory as Id,EarningCategory as Name from Payroll_OtherEarning where lower(VariableName)=lower('" + OBJOtherEarning.VariableHead + "') and Deactivate=0 and CmpId=" + OBJOtherEarning.VariableEarningCmpId + "");
                        var GetVariableName1 = DapperORM.ReturnList<PayrollDropDownBind>("sp_QueryExcution", paramCa1t).ToList();
                        ViewBag.GetOtherEarningName = GetVariableName1;

                        DynamicParameters paramName1 = new DynamicParameters();
                        paramName1.Add("@p_CmpId", OBJOtherEarning.VariableEarningCmpId);
                        var GetVeriableHead1 = DapperORM.ReturnList<PayrollDropDownBind>("sp_GetVariableHead_Earning", paramName1).ToList();
                        ViewBag.GetHeadVariable = GetVeriableHead1;
                    }

                    if (OBJOtherEarning.VariableEarningEmployeeId == 0)
                    {
                        Query = Query + " and EmployeeLeft=0   and MAS_EMPLOYEE.ContractorID=1 ";
                    }
                    else
                    {
                        Query = Query + " and EmployeeLeft=0   and ContractorID=1 and MAS_EMPLOYEE.EMPLOYEEID =" + OBJOtherEarning.VariableEarningEmployeeId + "";
                    }

                    dt_Head_Variable = objcon.GetDataTable("exec sp_GetVariableHead_Earning @p_CmpId=" + OBJOtherEarning.VariableEarningCmpId + "");
                    GetHead_Variable_Name(OBJOtherEarning.VariableHead.ToString(), out CategoryName_Id);
                    List_CategoryName_Id = CategoryName_Id;

                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@P_Qry", Query);
                    paramList.Add("@p_VariableHead", List_CategoryName_Id);
                    paramList.Add("@p_SubVariableHead", OBJOtherEarning.OtherEarningName);
                    var OtherEarningEmployeeLoad = DapperORM.ReturnList<dynamic>("sp_GetVariableDeductionEmployee", paramList).ToList();
                    ViewBag.GetOtherEarningEmployeeLoad = OtherEarningEmployeeLoad;
                }
                else
                {
                    ViewBag.GetOtherEarningEmployeeLoad = "";
                    ViewBag.GetBranchName = "";
                    ViewBag.GetEmployeeList = "";
                }
                return View(OBJOtherEarning);
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
                var GetVeriableHead = DapperORM.ReturnList<PayrollDropDownBind>("sp_GetVariableHead_Earning", paramName).ToList();
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
        public ActionResult GetEmployeeName(int? BranchId, int CmpID)
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
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeeLeft=0 and  EmployeeBranchId=" + BranchId + " and ContractorId=1 order by Name");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    ViewBag.EmployeeList = data;
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeeLeft=0 and  EmployeeBranchId in     (select mas_branch.BranchID as Id from UserBranchMapping,mas_branch where UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + CmpID + " and UserBranchMapping.IsActive = 1)  and ContractorId=1 order by Name");
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
        // public ActionResult SaveUpdate(List<Other_Earning_Excel> ObjEarning, int CmpId, int BranchID, DateTime MonthYear, string VariableHead, string OtherEarningName)
        public ActionResult SaveUpdate(List<Other_Earning_Excel> ObjEarning, int CmpId, DateTime MonthYear)
        {
            try
            {
                dt_ValidationCheck = objcon.GetDataTable("Select Mas_Employee.EmployeeName,Mas_Employee.EmployeeNo,Mas_Employee.EmployeeCardNo,Mas_Employee_Personal.AadhaarNo  ,Convert (int,Mas_Employee.EmployeeId) As EmployeeId,Mas_Branch.BranchName,Convert (int,Mas_Employee.EmployeeBranchId) AS  EmployeeBranchId from Mas_Employee inner join Mas_Employee_Personal on Mas_Employee.employeeid=Mas_Employee_Personal.personalEmployeeid inner join Mas_Branch on Mas_Employee.EmployeeBranchId=Mas_Branch.BranchId where Mas_Employee.Deactivate=0 and Mas_Employee_Personal.Deactivate=0 and Mas_Employee.CmpID=" + CmpId + "");
                dt_Head_Variable = objcon.GetDataTable("exec sp_GetVariableHead_Earning @p_CmpId=" + CmpId + "");
                dt_OtherEarning = objcon.GetDataTable("Select VariableName as Id,EarningCategory as Name from Payroll_OtherEarning where  Deactivate=0 and CmpId=" + CmpId + "");
                StringBuilder strBuilder = new StringBuilder();
                //var PayrollCount = DapperORM.DynamicQuerySingle("Select Count(PayrollLockId) as LockCount from Payroll_LOck where Deactivate=0 and PayrollLockBranchID=" + BranchID + " and Month(PayrollLockMonthYear)='" + MonthYear.ToString("MM") + "'  and Year(PayrollLockMonthYear) ='" + MonthYear.ToString("yyyy") + "' and Status=1");
                //if (PayrollCount.LockCount != 0)
                //{
                //    TempData["Message"] = "The record can't be saved because the payroll for this month ('" + MonthYear.ToString("MM-yyyy") + "') is locked.";
                //    TempData["Icon"] = "error";
                //    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                //}

                if (ObjEarning != null)
                {
                    string str_Earning = "";
                    string str_EarningName = "";
                    foreach (var Data in ObjEarning)
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
                            var Message = "Category Name is does not belong to this company" + Data.EmployeeNo;
                            var Icon = "error";
                            var rowno = Data.EmployeeNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetOtherEarning_Id(Data.SubCategoryName.ToString(), out SubCategoryName_Id);
                        List_SubCategoryName_Id = SubCategoryName_Id;
                        if (List_SubCategoryName_Id == "")
                        {
                            var Message = "Sub-Category Name is does not belong to this company" + Data.EmployeeNo;
                            var Icon = "error";
                            var rowno = Data.EmployeeNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        str_Earning = str_Earning + "" + List_Employee_Id + ",";
                        str_EarningName = str_EarningName + "'" + List_SubCategoryName_Id + "',";
                    }

                    if (str_Earning != "")
                    {
                        str_Earning = str_Earning.Substring(0, str_Earning.Length - 1);
                    }

                    if (str_EarningName != "")
                    {
                        str_EarningName = str_EarningName.Substring(0, str_EarningName.Length - 1);
                    }

                    //var ListEmployeeNo = DapperORM.DynamicQuerySingle("Select CompanyName as Company,BranchName as BU,EmployeeNo, EmployeeName as Employee,DepartmentName from Mas_Employee,Mas_CompanyProfile,Mas_Branch,Mas_Department ,Payroll_VariableEarning where  Mas_Employee.Deactivate=0  and Mas_CompanyProfile.Deactivate=0 and Mas_Branch.Deactivate=0 and  Mas_Department.Deactivate=0 and Payroll_VariableEarning.Deactivate=0  and Mas_Employee.EmployeeId=Payroll_VariableEarning.VariableEarningEmployeeId and  Mas_Employee.CmpID=Mas_CompanyProfile.CompanyId and Mas_Employee.EmployeeBranchId=BranchId  and Mas_Employee.EmployeeDepartmentID=DepartmentId and  Payroll_VariableEarning.VariableEarningEmployeeId in (" + str_Earning + ")  and Payroll_VariableEarning.OtherEarningName='" + OtherEarningName + "' and month( Payroll_VariableEarning.MonthYear)='" + MonthYear.ToString("MM") + "' and Year( Payroll_VariableEarning.MonthYear)='" + MonthYear.ToString("yyyy") + "'").ToList();
                    // Assuming List_Employee_Id is a list of Employee IDs
                    // string str_Earning = string.Join(",", List_Employee_Id);

                    // Build the SQL query using parameterized values to prevent SQL injection
                    var ListEmployeeNo = DapperORM.DynamicQueryList("SELECT  CompanyName AS Company, BranchName AS BU, EmployeeNo, EmployeeName AS Employee, DepartmentName  FROM  Mas_Employee JOIN Mas_CompanyProfile ON Mas_Employee.CmpID = Mas_CompanyProfile.CompanyId JOIN Mas_Branch ON Mas_Employee.EmployeeBranchId = Mas_Branch.BranchId   JOIN Mas_Department ON Mas_Employee.EmployeeDepartmentID = Mas_Department.DepartmentId JOIN Payroll_VariableEarning ON Mas_Employee.EmployeeId = Payroll_VariableEarning.VariableEarningEmployeeId  WHERE  Mas_Employee.Deactivate = 0 AND  Mas_Branch.Deactivate = 0 AND  Mas_Department.Deactivate = 0 AND Payroll_VariableEarning.Deactivate = 0 AND   Payroll_VariableEarning.VariableEarningEmployeeId IN (" + str_Earning + ") AND   Payroll_VariableEarning.OtherEarningName in (" + str_EarningName + ") AND  MONTH(Payroll_VariableEarning.MonthYear) = " + MonthYear.Month + " AND   YEAR(Payroll_VariableEarning.MonthYear) = " + MonthYear.Year + "").ToList();
                    if (ListEmployeeNo.Count != 0)
                    {
                        ViewBag.GetListEmployeeNo = ListEmployeeNo;
                        return Json(new { ViewBag.GetListEmployeeNo }, JsonRequestBehavior.AllowGet);
                    }

                    foreach (var Data in ObjEarning)
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
                            var Message = "Category Name is does not belong to this company" + Data.EmployeeNo;
                            var Icon = "error";
                            var rowno = Data.EmployeeNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetOtherEarning_Id(Data.SubCategoryName.ToString(), out SubCategoryName_Id);
                        List_SubCategoryName_Id = SubCategoryName_Id;
                        if (List_SubCategoryName_Id == "")
                        {
                            var Message = "Sub-Category Name is does not belong to this company" + Data.EmployeeNo;
                            var Icon = "error";
                            var rowno = Data.EmployeeNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }


                        string StringEarning = " Insert Into Payroll_VariableEarning(" +
                                              "  Deactivate  " +
                                              " , CreatedBy  " +
                                              " , CreatedDate  " +
                                              " , MachineName  " +
                                              " , VariableEarningCmpId  " +
                                              " , VariableEarningBranchId  " +
                                              " , MonthYear  " +
                                              " , VariableHead  " +
                                               " , VariableEarningEmployeeId  " +
                                              " , Amount  " +
                                              " , SalaryLock  " +
                                              " , Remarks  " +
                                              " , OtherEarningName " + ")  values (" +
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
                        strBuilder.Append(StringEarning);

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

        public ActionResult GetList(OtherEarningList OBJOtherEarningList)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 160;
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

                if (OBJOtherEarningList.OtherEarningMonthYear != DateTime.MinValue)
                {

                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_employeeid", Session["EmployeeId"]);
                    param.Add("@p_CmpId", OBJOtherEarningList.CmpID);
                    var data1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                    ViewBag.GetBranchName = data1;

                    DynamicParameters paramName = new DynamicParameters();
                    paramName.Add("@p_CmpId", OBJOtherEarningList.CmpID);
                    var GetVeriableHead = DapperORM.ReturnList<PayrollDropDownBind>("sp_GetVariableHead_Earning", paramName).ToList();
                    ViewBag.GetHeadVariable = GetVeriableHead;

                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_Origin", "List");
                    paramList.Add("@p_BranchId", OBJOtherEarningList.BranchId);
                    paramList.Add("@p_VariableHead", OBJOtherEarningList.EarningName);
                    paramList.Add("@p_MonthYear", OBJOtherEarningList.OtherEarningMonthYear.ToString("yyyy-MM-dd"));
                    var data = DapperORM.DynamicList("sp_List_Payroll_VariableEarning", paramList);
                    ViewBag.OtherEarning = data;
                }
                else
                {
                    ViewBag.OtherEarning = "";
                }

                return View(OBJOtherEarningList);
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
                        string StringMonthlyAttendance = " Update  Payroll_VariableEarning set Deactivate=1,ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=GETDATE() where  VariableEarningId=" + Data.VariableDeductionId + " and SalaryLock='0'    ";
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
                                                                            "'BuklInsert'," +
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

        #region GetOtherEarningName
        [HttpGet]
        public ActionResult GetOtherEarningName(string VariableName, int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();

                param.Add("@query", "Select EarningCategory as Id,EarningCategory as Name from Payroll_OtherEarning where lower(VariableName)=lower('" + VariableName + "') and Deactivate=0 and CmpId=" + CmpId + "");
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
                    string.Equals(row.Field<string>("EmployeeNo")?.Trim(), EmployeeNo.Trim(), StringComparison.OrdinalIgnoreCase));

                        //string.Equals(row.Field<string>("EmployeeNo"), EmployeeNo, StringComparison.OrdinalIgnoreCase));

                if (matchingRow != null)
                {
                    Employee_Id = matchingRow.Field<int>("EmployeeId");
                    Branch_Id = matchingRow.Field<int>("EmployeeBranchId");
                    BranchName = matchingRow.Field<string>("BranchName");
                }
                else
                {
                    Employee_Id = 0;
                    Branch_Id = 0;
                    BranchName = "";
                }
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


        void GetOtherEarning_Id(string SubCategoryName, out string SubCategoryName_Id)
        {
            try
            {
                // Check if the CategoryName exists in the dt_Head_Variable
                var matchingRow = dt_OtherEarning.AsEnumerable()
                    .FirstOrDefault(row =>
                        string.Equals(row.Field<string>("Name"), SubCategoryName, StringComparison.OrdinalIgnoreCase));

                if (matchingRow != null)
                {
                    // If found, assign the Id as string to CategoryName_Id
                    SubCategoryName_Id = matchingRow.Field<string>("Name");
                }
                else
                {
                    // If not found, assign an empty string
                    SubCategoryName_Id = string.Empty;
                }
            }
            catch (Exception ex)
            {
                // Log exception (if necessary) and set default value
                Console.WriteLine($"Error: {ex.Message}");
                SubCategoryName_Id = string.Empty;
            }
        }
        #endregion

    }
}