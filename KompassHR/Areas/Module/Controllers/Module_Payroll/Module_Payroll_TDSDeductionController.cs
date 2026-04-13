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
    public class Module_Payroll_TDSDeductionController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        DataTable dt_ValidationCheck = new DataTable();
        DataTable dt_BU = new DataTable();

        // GET: Module/Module_Payroll_TDSDeduction
        #region Module_Payroll_TDSDeduction
        public ActionResult Module_Payroll_TDSDeduction()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 470;
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
                var CmpId = GetComapnyName[0].Id;

                //GET BRANCH NAME
                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CmpId), Convert.ToInt32(Session["EmployeeId"]));
                ViewBag.GetBranchName = Branch;
                var BranchId = Branch[0].Id;

                DynamicParameters param7 = new DynamicParameters();
                param7.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeeLeft=0 and  CmpID=" + CmpId + "  and ContractorId=1 order by Name");
                var data7 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param7).ToList();
                ViewBag.GetEmployeeList = data7;

                ViewBag.GetTDSEmployeeLoad = "";


                //var Query = "";
                //if (OBjTDS.TDSDeductionCmpId != 0)
                //{
                //    DynamicParameters param = new DynamicParameters();
                //    param.Add("@p_employeeid", Session["EmployeeId"]);
                //    param.Add("@p_CmpId", OBjTDS.TDSDeductionCmpId);
                //    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                //    ViewBag.GetBranchName = data;

                //    if (OBjTDS.TDSBranchId != 0)
                //    {
                //        Query = "and MAS_EMPLOYEE.EmployeeBranchId=" + OBjTDS.TDSBranchId + "";

                //        DynamicParameters paramEMP = new DynamicParameters();
                //        paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId='" + OBjTDS.TDSBranchId + "'and Mas_Employee.EmployeeLeft=0 and ContractorId=1 order by Name");
                //        var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                //        ViewBag.GetEmployeeList = GetEmployeeName;
                //    }
                //    else
                //    {
                //        Query = " and MAs_employee.employeeBranchId in (select mas_branch.BranchID as Id from UserBranchMapping,mas_branch where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + OBjTDS.TDSDeductionCmpId + " and UserBranchMapping.IsActive = 1)";
                //    }

                //    //var GetMonthYear = OBJOtherDeduction.OtherEarningMonthYear.ToString("yyyy-MM-dd");
                //    var DeductionMonthYear = OBjTDS.MonthYear.ToString("yyyy-MM-dd");
                //    if (OBjTDS.TDSEmployeeId == 0)
                //    {
                //        //Query = Query + "  and MAS_EMPLOYEE.ContractorID=1  and MAS_EMPLOYEE.EMPLOYEEID NOT IN (SELECT VariableDeductionEmployeeId FROM Payroll_VariableDeduction where Payroll_VariableDeduction.deactivate=0 and lower(Payroll_VariableDeduction.OtherDeductionName)=lower('" + OBJOtherDeduction.VariableHead + "')  AND MonthYear=FORMAT(convert(datetime," + OBJOtherDeduction.MonthYear.ToString("yyyy-MM-dd") + "),''))";
                //        Query = Query + "  and MAS_EMPLOYEE.ContractorID=1 ";
                //    }
                //    else
                //    {
                //        Query = Query + "  and ContractorID=1 and MAS_EMPLOYEE.EMPLOYEEID =" + OBjTDS.TDSEmployeeId + "";
                //    }

                //    DynamicParameters paramList = new DynamicParameters();
                //    paramList.Add("@P_Qry", Query);
                //    var OtherEarningEmployeeLoad = DapperORM.ReturnList<dynamic>("sp_GetTDSDeductionEmployee", paramList).ToList();
                //    ViewBag.GetOtherDeductionEmployeeLoad = OtherEarningEmployeeLoad;
                //}
                //else
                //{
                //    ViewBag.GetOtherDeductionEmployeeLoad = "";
                //    ViewBag.GetBranchName = "";
                //    ViewBag.GetEmployeeList = "";
                //}
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Module_Payroll_TDSDeduction Main VIew
        [HttpPost]
        public ActionResult Module_Payroll_TDSDeduction(Payroll_TDSDeduction OBjTDS, HttpPostedFileBase AttachFile)
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
                var CmpId = GetComapnyName[0].Id;

                //GET BRANCH NAME
                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CmpId), Convert.ToInt32(Session["EmployeeId"]));
                ViewBag.GetBranchName = Branch;

                DynamicParameters param7 = new DynamicParameters();
                param7.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeeLeft=0 and  CmpID=" + CmpId + "  and ContractorId=1 order by Name");
                var data7 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param7).ToList();
                ViewBag.GetEmployeeList = data7;

                ViewBag.GetOtherEarningName = "";
                ViewBag.GetTDSEmployeeLoad = "";

                var Query = "";
                if (AttachFile != null)
                {
                    if (AttachFile.ContentLength > 0)
                    {
                        List<TDSDeduction_Excel> excelDataList = new List<TDSDeduction_Excel>();
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
                                if (duplicateEmployeeNumbers.Any())
                                {
                                    TempData["Message"] = $"Duplicate Employee Numbers found: {string.Join(", ", duplicateEmployeeNumbers.Distinct())}";
                                    TempData["Title"] = "Duplicate Employee Numbers Detected";
                                    TempData["Icon"] = "error";
                                    return RedirectToAction("Module_Payroll_TDSDeduction", "Module_Payroll_TDSDeduction");
                                }
                                int row = 3;
                                if (xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString() == "")
                                {

                                    TempData["Message"] = "Fill in missing information in the first column.";
                                    TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                    TempData["Icon"] = "error";
                                    return RedirectToAction("Module_Payroll_TDSDeduction", "Module_Payroll_TDSDeduction");
                                }
                                while (xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString() != "")
                                {
                                    TDSDeduction_Excel TDSDeduction = new TDSDeduction_Excel();
                                    TDSDeduction.EmployeeNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString();
                                    TDSDeduction.EmployeeName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 2).GetString();
                                    TDSDeduction.Amount = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 3).GetString();
                                    TDSDeduction.Remark = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 4).GetString();
                                    int BranchId = GetBranchId_Id(TDSDeduction.EmployeeNo);
                                    TDSDeduction.EmployeeBranchId = BranchId;
                                    excelDataList.Add(TDSDeduction);
                                    row++;
                                }
                                ViewBag.GetTDSEmployeeLoad = excelDataList;
                                //ViewBag.GetOtherDeductionEmployeeLoad = null;

                            }
                        }
                        if ((OBjTDS.TDSDeductionCmpId != 0) && (OBjTDS.TDSDeductionCmpId != null))
                        {
                            DynamicParameters param = new DynamicParameters();
                            param.Add("@p_employeeid", Session["EmployeeId"]);
                            param.Add("@p_CmpId", OBjTDS.TDSDeductionCmpId);
                            var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                            ViewBag.GetBranchName = data;
                            if (OBjTDS.TDSBranchId != 0)
                            {
                                DynamicParameters paramEmployee = new DynamicParameters();
                                paramEmployee.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeeLeft=0 and  CmpID=" + CmpId + "  and ContractorId=1 order by Name");
                                var EmployeeList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmployee).ToList();
                                ViewBag.GetEmployeeList = EmployeeList;
                            }
                            else
                            {
                                DynamicParameters paramEmployee = new DynamicParameters();
                                paramEmployee.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeeLeft=0 and  EmployeeBranchId=" + OBjTDS.TDSBranchId + "  and ContractorId=1 order by Name");
                                var EmployeeList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmployee).ToList();
                                ViewBag.GetEmployeeList = EmployeeList;
                            }

                        }
                    }

                }
                else if (OBjTDS.TDSDeductionCmpId != null)
                {

                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_employeeid", Session["EmployeeId"]);
                    param.Add("@p_CmpId", OBjTDS.TDSDeductionCmpId);
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                    ViewBag.GetBranchName = data;

                    if (OBjTDS.TDSBranchId != 0)
                    {
                        Query = "and MAS_EMPLOYEE.EmployeeBranchId=" + OBjTDS.TDSBranchId + "";

                        DynamicParameters paramEMP = new DynamicParameters();
                        paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeeLeft=0 and EmployeeBranchId='" + OBjTDS.TDSBranchId + "'and ContractorId=1 order by Name");
                        var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                        ViewBag.GetEmployeeList = GetEmployeeName;
                    }
                    else
                    {
                        Query = " and MAs_employee.employeeBranchId in (select mas_branch.BranchID as Id from UserBranchMapping,mas_branch where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + OBjTDS.TDSDeductionCmpId + " and UserBranchMapping.IsActive = 1)";
                    }

                    //var GetMonthYear = OBJOtherDeduction.OtherEarningMonthYear.ToString("yyyy-MM-dd");
                    var DeductionMonthYear = OBjTDS.MonthYear.ToString("yyyy-MM-dd");
                    if (OBjTDS.TDSEmployeeId == 0)
                    {
                        //Query = Query + "  and MAS_EMPLOYEE.ContractorID=1  and MAS_EMPLOYEE.EMPLOYEEID NOT IN (SELECT VariableDeductionEmployeeId FROM Payroll_VariableDeduction where Payroll_VariableDeduction.deactivate=0 and lower(Payroll_VariableDeduction.OtherDeductionName)=lower('" + OBJOtherDeduction.VariableHead + "')  AND MonthYear=FORMAT(convert(datetime," + OBJOtherDeduction.MonthYear.ToString("yyyy-MM-dd") + "),''))";
                        Query = Query + "  and MAS_EMPLOYEE.ContractorID=1 ";
                    }
                    else
                    {
                        Query = Query + "  and ContractorID=1 and MAS_EMPLOYEE.EMPLOYEEID =" + OBjTDS.TDSEmployeeId + "";
                    }

                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@P_Qry", Query);
                    var OtherEarningEmployeeLoad = DapperORM.ReturnList<dynamic>("sp_GetTDSDeductionEmployee", paramList).ToList();
                    ViewBag.GetTDSEmployeeLoad = OtherEarningEmployeeLoad;
                }
                else
                {
                    ViewBag.GetTDSEmployeeLoad = "";
                    ViewBag.GetBranchName = "";
                    ViewBag.GetEmployeeList = "";
                }
                return View(OBjTDS);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetList

        public ActionResult GetList(Payroll_TDSDeduction _TDSDeduction)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 470;
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
                var CmpId = GetComapnyName[0].Id;

                //GET BRANCH NAME
                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CmpId), Convert.ToInt32(Session["EmployeeId"]));
                ViewBag.GetBranchName = Branch;
                if (_TDSDeduction.MonthYear != DateTime.MinValue)
                {

                    //DynamicParameters param = new DynamicParameters();
                    //param.Add("@p_employeeid", Session["EmployeeId"]);
                    //param.Add("@p_CmpId", _TDSDeduction.TDSDeductionCmpId);
                    //var data1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                    //ViewBag.GetBranchName = data1;


                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_Origin", "List");
                    paramList.Add("@p_CmpId", _TDSDeduction.TDSDeductionCmpId);
                    paramList.Add("@p_BranchId", _TDSDeduction.TDSBranchId);
                    paramList.Add("@p_MonthYear", _TDSDeduction.MonthYear.ToString("yyyy-MM-dd"));
                    var data = DapperORM.DynamicList("sp_List_Payroll_TDSDeduction", paramList);
                    ViewBag.TDSDeduction = data;
                }
                else
                {
                    ViewBag.TDSDeduction = "";
                }

                return View(_TDSDeduction);
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
        int List_Employee_Id = 0;
        int Employee_Id = 0;
        int List_BUId = 0;
        int Branch_Id = 0;

        [HttpPost]
        public ActionResult SaveUpdate(List<TDSDeduction_Excel> ObjDeduction, int CmpId, int? BranchID, DateTime MonthYear)
        {
            try
            {
                // dt_ValidationCheck = objcon.GetDataTable("Select Mas_Employee.EmployeeName,Mas_Employee.EmployeeNo,Mas_Employee.EmployeeCardNo,Mas_Employee_Personal.AadhaarNo  ,Convert (int,Mas_Employee.EmployeeId) As EmployeeId,Mas_Branch.BranchName,Convert (int,Mas_Employee.EmployeeBranchId) AS  EmployeeBranchId,Convert (int,Mas_Employee.CmpID) As CmpID from Mas_Employee left join Mas_Employee_Personal on Mas_Employee.employeeid=Mas_Employee_Personal.personalEmployeeid inner join Mas_Branch on Mas_Employee.EmployeeBranchId=Mas_Branch.BranchId where Mas_Employee.Deactivate=0  and Mas_Employee.employeeLeft=0 and Mas_Employee.ContractorId=1 and Mas_Employee.EmployeeId <> 1 order by Mas_Employee.EmployeeNo");
                //dt_ValidationCheck = objcon.GetDataTable("Select Mas_Employee.EmployeeName,Mas_Employee.EmployeeNo,Mas_Employee.EmployeeCardNo,Mas_Employee_Personal.AadhaarNo,Convert(int, Mas_Employee.EmployeeId) As EmployeeId, Mas_Branch.BranchName, Convert(int, Mas_Employee.EmployeeBranchId) AS  EmployeeBranchId, Convert(int, Mas_Employee.CmpID)  As CmpID from Mas_Employee  left join Mas_Employee_Personal on Mas_Employee.employeeid = Mas_Employee_Personal.personalEmployeeid  inner join Mas_Branch on Mas_Employee.EmployeeBranchId = Mas_Branch.BranchId  where Mas_Employee.Deactivate = 0  and Mas_Employee.employeeLeft = 0 and Mas_Employee.ContractorId = 1 order by mas_employee.EmployeeNo");

                StringBuilder strBuilder = new StringBuilder();
                if (ObjDeduction != null)
                {
                    foreach (var Data in ObjDeduction)
                    {
                        List_Employee_Id = 0;
                        List_BUId = 0;
                        Employee_Id = 0;
                        GetEmployeeId_Id(Data.EmployeeNo.ToString(), CmpId, Convert.ToInt32(BranchID), out Employee_Id);
                        List_Employee_Id = Employee_Id;
                        if (BranchID == null)
                        {
                            List_BUId = Convert.ToInt32(Data.EmployeeBranchId);
                            var PayrollCount = DapperORM.DynamicQuerySingle("Select Count(PayrollLockId) as LockCount from Payroll_LOck where Deactivate=0 and PayrollLockBranchID=" + Data.EmployeeBranchId + " and Month(PayrollLockMonthYear)='" + MonthYear.ToString("MM") + "'  and Year(PayrollLockMonthYear) ='" + MonthYear.ToString("yyyy") + "' and Status=1");
                            if (PayrollCount.LockCount != 0)
                            {
                                TempData["Message"] = "The record can't be saved because the payroll for this month ('" + MonthYear.ToString("MMM-yyyy") + "') is locked.";
                                TempData["Icon"] = "error";
                                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            List_BUId = Convert.ToInt32(BranchID);
                            var PayrollCount = DapperORM.DynamicQuerySingle("Select Count(PayrollLockId) as LockCount from Payroll_LOck where Deactivate=0 and PayrollLockBranchID=" + BranchID + " and Month(PayrollLockMonthYear)='" + MonthYear.ToString("MM") + "'  and Year(PayrollLockMonthYear) ='" + MonthYear.ToString("yyyy") + "' and Status=1");
                            if (PayrollCount.LockCount != 0)
                            {
                                TempData["Message"] = "The record can't be saved because the payroll for this month ('" + MonthYear.ToString("MMM-yyyy") + "') is locked.";
                                TempData["Icon"] = "error";
                                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                            }
                        }

                        if (Employee_Id == 0)
                        {
                            var Message = "The employee does not belong to this business unit (EmployeeNo). " + Data.EmployeeNo;
                            var Icon = "error";
                            var rowno = Data.EmployeeNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        string StringTDSDeduction = " Insert Into Payroll_TDSDeduction(" +
                                              "  Deactivate  " +
                                              " , CreatedBy  " +
                                              " , CreatedDate  " +
                                              " , MachineName  " +
                                              " , TDSDeductionCmpId  " +
                                              " , TDSBranchId  " +
                                              " , MonthYear  " +
                                               " , TDSEmployeeId  " +
                                              " , Amount  " +
                                              " , SalaryLock  " +
                                              " , Remarks   " + ")  values (" +
                                               "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                               "'" + CmpId + "'," +
                                               "'" + List_BUId + "'," +
                                               "'" + MonthYear.ToString("yyyy-MM-dd") + "'," +
                                               "'" + List_Employee_Id + "'," +
                                               "'" + Data.Amount + "'," +
                                               "'0'," +
                                               "'" + Data.Remark + "')" +
                                                " " +
                                               " " +
                                                " ";
                        strBuilder.Append(StringTDSDeduction);

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
                                                                            "'TDSDeduction'," +
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

        #region GetEmployeeName
        [HttpGet]
        public ActionResult GetEmployeeName(int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });

                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeeLeft=0 and  EmployeeBranchId=" + BranchId + "  and ContractorId=1 order by Name");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.EmployeeList = data;
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region Delete Event
        public ActionResult Delete(List<Payroll_TDSDeduction> ObjDelete)
        {
            try
            {
                StringBuilder strBuilder = new StringBuilder();
                if (ObjDelete != null)
                {
                    foreach (var Data in ObjDelete)
                    {
                        string StringTDSDeduction = " Update  Payroll_TDSDeduction set Deactivate=1,ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=GETDATE() where  TDSDeductionId=" + Data.TDSDeductionId + " and SalaryLock='0'    ";
                        strBuilder.Append(StringTDSDeduction);

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
                                                                            "'TDSDeduction'," +
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

        #region GetEmployeeId_Id
        void GetEmployeeId_Id(string EmployeeNo, int EmployeeCmpID, int EmployeeBranchId, out int Employee_Id)
        {
            Employee_Id = 0; // default value

            try
            {
                var EmployeeIdd = DapperORM.DynamicQuerySingle("Select Convert(int, EmployeeId) as EmployeeId  from Mas_Employee where EmployeeNo='" + EmployeeNo + "' ;");
                if (EmployeeIdd != null)
                {
                    Employee_Id = EmployeeIdd.EmployeeId;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Employee_Id = 0;
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

    }
}