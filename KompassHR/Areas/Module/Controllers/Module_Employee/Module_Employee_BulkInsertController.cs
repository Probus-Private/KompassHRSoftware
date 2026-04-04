using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClosedXML.Excel;
using System.IO;
//using OfficeOpenXml;//EPPLUS
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data;
using KompassHR.Areas.Module.Models.Module_Employee;
using Newtonsoft.Json;
using System.Net;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_BulkInsertController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: /Module/Module_Employee_BulkInsert/Module_Employee_BulkInsert

        #region Module_Employee_BulkInsert main View

        DataTable dt_company = new DataTable();
        DataTable dt_BU = new DataTable();
        DataTable dt_Contractor = new DataTable();
        DataTable dt_Qualification = new DataTable();
        DataTable dt_Department = new DataTable();
        DataTable dt_SubDepartment = new DataTable();
        DataTable dt_Designation = new DataTable();
        DataTable dt_Grade = new DataTable();
        DataTable dt_Line = new DataTable();
        DataTable dt_SubUnit = new DataTable();
        DataTable dt_ShiftGroup = new DataTable();
        DataTable dt_ShiftRule = new DataTable();
        DataTable dt_AssessmentLevel = new DataTable();
        DataTable dt_CriticalStage = new DataTable();
        DataTable dt_ManpowerCategory = new DataTable();
        DataTable dt_RelationWithNominee = new DataTable();
        DataTable dt_companyName = new DataTable();

        public ActionResult Module_Employee_BulkInsert()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 297;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.ComapnyProfile = GetComapnyName;

                var results = DapperORM.DynamicQueryMultiple(@"select DepartmentId as Id, DepartmentName as Name from Mas_Department where Deactivate=0 order by DepartmentName;
                                                         select DesignationId as Id,DesignationName as Name from Mas_Designation where Deactivate = 0 and ApplicableForWorker = 1 order by DesignationName; 
                                                         select GradeId as Id,GradeName as Name from Mas_Grade where Deactivate=0 order by Name;
                                                         Select EmployeeLevelId as Id ,EmployeeLevel As Name from Mas_EmployeeLevel where Deactivate=0 order by Name ;
                                                         Select QualificationPFId as Id ,QualificationPFName As Name from Mas_Qualification_PF where Deactivate=0 order by Name;
                                                         Select CriticalStageId As Id,CriticalStageName as Name from Mas_CriticalStageCategory where Mas_CriticalStageCategory.Deactivate=0  order by CriticalStageId;
                                                         Select KPISubCategoryId as Id,KPISubCategoryFName as Name  from KPI_SubCategory  where KPI_SubCategory.Deactivate=0 order by Name");
                ViewBag.DepartmentList = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.DesignationList = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GradeList = results[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
              //  ViewBag.ContractorNameList = results[3].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.EmployeeLevel = results[3].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.QualificationName = results[4].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.CriticalStageName = results[5].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.EmployeeAllocationCategory = results[6].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                ViewBag.SubDepartmentList = "";
               //  ViewBag.DesignationList = "";
                ViewBag.ContractorNameList = "";
                return View();
                //select DesignationId as Id,DesignationName as Name from Mas_Designation where Deactivate = 0 and ApplicableForWorker = 1 order by DesignationName;
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
                    return RedirectToAction("Login", "Login", new { Area = "" });
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

        #region GetUnit
        //Get Unit ,Shift Rule ,Shift Group Name
        public ActionResult GetUnit(int BusinessUnit)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramShiftRule = new DynamicParameters();
                paramShiftRule.Add("@query", "select ShiftRuleId as Id ,ShiftRuleName as Name from Atten_ShiftRule where Deactivate=0 and ShiftRuleBranchId='" + BusinessUnit + "'");
                var ShiftRuleName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramShiftRule).ToList();

                DynamicParameters paramShiftGroup = new DynamicParameters();
                paramShiftGroup.Add("@query", "select ShiftGroupId as Id,ShiftGroupFName as Name from Atten_ShiftGroups where Deactivate=0 and ShiftGroupBranchId='" + BusinessUnit + "'");
                var ShiftGroupName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramShiftGroup).ToList();

                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "Select  UnitID as Id, UnitName As Name from Mas_Unit where Deactivate=0 and UnitBranchId= '" + BusinessUnit + "' ");
                var UnitName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                DynamicParameters paramLine = new DynamicParameters();
                paramLine.Add("@query", "select LineId as Id ,LineName as Name from Mas_LineMaster where Deactivate=0 and BranchId='" + BusinessUnit + "' ");
                var LineName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + BusinessUnit + " and Mas_ContractorMapping.ContractorID<>1");
                var GetContractorDropdown = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param2).ToList();

                //DynamicParameters paramCategory = new DynamicParameters();
                //paramCategory.Add("@query", "Select KPISubCategoryId as Id,KPISubCategoryFName as Name  from KPI_SubCategory  where KPI_SubCategory.Deactivate=0 order by Name");
                //var EmployeeAllocationCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramCategory).ToList();




                return Json(new { ShiftGroupName = ShiftGroupName, ShiftRuleName = ShiftRuleName, UnitName = UnitName, LineName = LineName, GetContractorDropdown = GetContractorDropdown }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetSubDepartment
        //Get SubDepartment
        public ActionResult GetSubDepartment(int DepartmentId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select SubDepartmentId as Id,SubDepartmentName as Name from Mas_SubDepartment where Deactivate=0 and DepartmentId='" + DepartmentId + "' ");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ImportExcelFile
        [HttpPost]
        public ActionResult Module_Employee_BulkInsert(HttpPostedFileBase AttachFile)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@query", "select CompanyId as Id, CompanyName as Name from Mas_CompanyProfile  where Deactivate = 0  and  CompanyId in ( Select distinct CmpID from UserBranchMapping where IsActive=1 and EmployeeID=" + Session["EmployeeId"] + ") order by CompanyName ;");
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramCompany).ToList();
                ViewBag.ComapnyProfile = GetComapnyName;

                var results = DapperORM.DynamicQueryMultiple(@"select DepartmentId as Id, DepartmentName as Name from Mas_Department where Deactivate=0 order by DepartmentName;
                                                         select DesignationId as Id,DesignationName as Name from Mas_Designation where Deactivate = 0 and ApplicableForWorker = 1 order by DesignationName; 
                                                         select GradeId as Id,GradeName as Name from Mas_Grade where Deactivate=0 order by Name;
                                                         Select EmployeeLevelId as Id ,EmployeeLevel As Name from Mas_EmployeeLevel where Deactivate=0 order by Name ;
                                                         Select QualificationPFId as Id ,QualificationPFName As Name from Mas_Qualification_PF where Deactivate=0 order by Name;
                                                         Select CriticalStageId As Id,CriticalStageName as Name from Mas_CriticalStageCategory where Mas_CriticalStageCategory.Deactivate=0  order by CriticalStageId;
                                                         Select KPISubCategoryId as Id,KPISubCategoryFName as Name  from KPI_SubCategory  where KPI_SubCategory.Deactivate=0 order by Name");
                ViewBag.DepartmentList = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.DesignationList = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GradeList = results[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.EmployeeLevel = results[3].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.QualificationName = results[4].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.CriticalStageName = results[5].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.EmployeeAllocationCategory = results[6].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                ViewBag.SubDepartmentList = "";
                ViewBag.ContractorNameList = "";
              //  ViewBag.EmployeeAllocationCategory = "";

                List<Module_Employee_BulkInsert> excelDataList = new List<Module_Employee_BulkInsert>();
                if (AttachFile.ContentLength > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        using (var attachStream = AttachFile.InputStream)
                        {
                            attachStream.Position = 0;

                            XLWorkbook xlWorkwook = new XLWorkbook(attachStream);

                            xlWorkwook.SaveAs(stream);  // Save the workbook to memory stream

                            int row = 3; // Starting row
                            var employeeNoSet = new HashSet<string>();
                            var employeeCardNoSet = new HashSet<string>();
                            var aadhaarNoSet = new HashSet<string>();
                            List<string> duplicateRows = new List<string>(); // To store details of duplicate rows

                            // Check if the first column is empty
                            if (xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString() == "")
                            {
                                TempData["Message"] = "Fill in missing information in the first column.";
                                TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                TempData["Icon"] = "error";
                                return RedirectToAction("Module_Employee_BulkInsert", "Module_Employee_BulkInsert");
                            }

                            //List<Module_Employee_BulkInsert> excelDataList = new List<Module_Employee_BulkInsert>();

                            while (!xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).IsEmpty())
                            {
                                string srNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString(); // Get SrNo
                                string employeeNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 7).GetString();
                                string employeeCardNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 8).GetString();
                                string aadhaarNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 19).GetString();

                                // Check for duplicate Aadhaar numbers
                                if (!string.IsNullOrWhiteSpace(aadhaarNo) && !aadhaarNoSet.Add(aadhaarNo))
                                {
                                    duplicateRows.Add($"Sr.No {srNo}: Duplicate Aadhaar Number ({aadhaarNo})");
                                }

                                // Check for duplicate Employee Card numbers
                                if (!string.IsNullOrWhiteSpace(employeeCardNo) && !employeeCardNoSet.Add(employeeCardNo))
                                {
                                    duplicateRows.Add($"Sr.No {srNo}: Duplicate Employee Card No ({employeeCardNo})");
                                }

                                // Check for duplicate Employee numbers
                                if (!string.IsNullOrWhiteSpace(employeeNo) && !employeeNoSet.Add(employeeNo))
                                {
                                    duplicateRows.Add($"Sr.No {srNo}: Duplicate Employee No ({employeeNo})");
                                }

                                // If any duplicates are found, stop processing and notify the user
                                if (duplicateRows.Any())
                                {
                                    TempData["Message"] = $"Duplicate entries found:\n{string.Join("\n", duplicateRows)}";
                                    TempData["Title"] = "Duplicate Entries Detected";
                                    TempData["Icon"] = "error";
                                    return RedirectToAction("Module_Employee_BulkInsert", "Module_Employee_BulkInsert");
                                }

                                // Create and populate the BulkInsert object
                                Module_Employee_BulkInsert BulkInsert = new Module_Employee_BulkInsert
                                {
                                    SrNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString(),
                                    CmpID = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 2).GetString(),
                                    EmployeeBranchId = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 3).GetString(),
                                    ContractorID = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 4).GetString(),
                                    Salutation = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 5).GetString(),
                                    EmployeeName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 6).GetString(),
                                    EmployeeNo = employeeNo,
                                    EmployeeCardNo = employeeCardNo,
                                    EmployeeDepartmentID = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 9).GetString(),
                                    EmployeeSubDepartmentName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 10).GetString(),
                                    EmployeeAllocationCategoryId = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 11).GetString(),
                                    EmployeeDesignationID = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 12).GetString(),
                                    EmployeeGradeID = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 13).GetString(),
                                    IsCriticalStageApplicable = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 14).GetString(),
                                    EmployeeCriticalStageID = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 15).GetString(),
                                    EmployeeLineID = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 16).GetString(),
                                    EmployeeLevelID = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 17).GetString(),
                                    EmployeeUnitID = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 18).GetString(),
                                    AadhaarNo = aadhaarNo,
                                    NameAsPerAadhaar = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 20).GetString(),
                                    PrimaryMobile = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 21).GetString(),
                                    Gender = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 22).GetString(),
                                    BirthdayDate = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 23).IsEmpty()
                                        ? DateTime.Now.ToString("yyyy-MM-dd")
                                        : xlWorkwook.Worksheets.Worksheet(1).Cell(row, 23).GetDateTime().ToString("yyyy-MM-dd"),
                                    MaritalStatus = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 24).GetString(),
                                    EmployeeQualificationID = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 25).GetString(),
                                    JoiningDate = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 26).IsEmpty()
                                        ? DateTime.Now.ToString("yyyy-MM-dd")
                                        : xlWorkwook.Worksheets.Worksheet(1).Cell(row, 26).GetDateTime().ToString("yyyy-MM-dd"),
                                    EM_Atten_WOFF1 = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 27).GetString(),
                                    EM_Atten_ShiftGroupId = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 28).GetString(),
                                    EM_Atten_ShiftRuleId = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 29).GetString(),
                                    PF_NO = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 30).GetString(),
                                    PF_FSType = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 31).GetString(),
                                    PF_FS_Name = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 32).GetString(),
                                    PF_UAN = string.IsNullOrEmpty(xlWorkwook.Worksheets.Worksheet(1).Cell(row, 33).GetString())
                                        ? "Pending"
                                        : xlWorkwook.Worksheets.Worksheet(1).Cell(row, 33).GetString(),
                                    ESIC_NO = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 34).GetString(),
                                    PF_Reletion1 = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 35).GetString(),
                                    PF_Nominee1 = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 36).GetString(),
                                    PresentPin = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 37).GetString(),
                                    PresentState = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 38).GetString(),
                                    PresentDistrict = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 39).GetString(),
                                    PresentTaluka = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 40).GetString(),
                                    PresentPO = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 41).GetString(),
                                    PresentCity = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 42).GetString(),
                                    PresentPostelAddress = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 43).GetString()
                                };
                                excelDataList.Add(BulkInsert);
                                row++;
                            }
                        }
                    }

                    ViewBag.count = 1;
                    ViewBag.GetExceldata = excelDataList;

                    DynamicParameters paramCompany1 = new DynamicParameters();
                    paramCompany1.Add("@query", "select CompanyId as Id, CompanyName as Name from Mas_CompanyProfile  where Deactivate = 0  and  CompanyId in ( Select distinct CmpID from UserBranchMapping where IsActive=1 and EmployeeID=" + Session["EmployeeId"] + ") order by CompanyName ;");
                    var GetComapnyName1 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramCompany1).ToList();
                    ViewBag.ComapnyProfile = GetComapnyName1;

                    var results1 = DapperORM.DynamicQueryMultiple(@"select DepartmentId as Id, DepartmentName as Name from Mas_Department where Deactivate=0 order by DepartmentName; 
                                                  select DesignationId as Id,DesignationName as Name from Mas_Designation where Deactivate=0 order by DesignationName;
                                                  select GradeId as Id,GradeName as Name from Mas_Grade where Deactivate=0 order by Name;
                                                  select ContractorId as Id,ContractorName as Name from Contractor_Master where Deactivate=0 order by ContractorName; 
                                                   Select EmployeeLevelId as Id ,EmployeeLevel As Name from Mas_EmployeeLevel where Deactivate=0 order by Name ;
                                                  Select QualificationPFId as Id ,QualificationPFName As Name from Mas_Qualification_PF where Deactivate=0 order by Name;
                                                  Select CriticalStageId As Id,CriticalStageName as Name from Mas_CriticalStageCategory where Mas_CriticalStageCategory.Deactivate=0  order by CriticalStageId;");
                    ViewBag.DepartmentList = results1[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                    ViewBag.DesignationList = results1[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                    ViewBag.GradeList = results1[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                    ViewBag.ContractorNameList = results1[3].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                    ViewBag.EmployeeLevel = results1[4].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                    ViewBag.QualificationName = results1[5].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                    ViewBag.CriticalStageName = results1[6].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                    ViewBag.SubDepartmentList = "";
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

        #region SaveUpdate
        int List_CompanyID = 0;
        int List_BUId = 0;
        int List_Contractor_Id = 0;
        int List_Qualification_Id = 0;
        int List_Department_Id = 0;
        int List_SubDepatment_Id = 0;
        int List_Designation_Id = 0;
        int List_Grade_Id = 0;
        int List_Line_Id = 0;
        int List_Unit_Id = 0;
        int List_ShiftGroup_Id = 0;
        int List_ShiftRule_Id = 0;
        int List_AssessmentLevel_Id = 0;
        int List_CriticalStage_Id = 0;
        int List_ManpowerCategory_Id = 0;
        int List_RelationWithNominee_Id = 0;

        int CompanyID_Id = 0;
        int Branch_Id = 0;
        int Contractor_Id = 0;
        int Qualification_Id = 0;
        int Department_Id = 0;
        int SubDepatment_Id = 0;
        int Designation_Id = 0;
        int Grade_Id = 0;
        int Line_Id = 0;
        int Unit_Id = 0;
        int ShiftGroup_Id = 0;
        int ShiftRule_Id = 0;
        int AssessmentLevel_Id = 0;
        int CriticalStage_Id = 0;
        int ManpowerCategory_Id = 0;
        bool GetIsCriticalStageApplicable = false;
        int RelationWithNominee_Id = 0;

        public JsonResult SaveUpdate(List<Module_Employee_BulkInsert> tbldata)
        {
            try
            {
                //if (Session["EmployeeId"] == null)
                //{
                //    return RedirectToAction("Login", "Login", new { Area = "" });
                //}

                string AdharNopattern = @"^\d{12}$";
                string MobileNopattern = @"^\d{10}$";
                string EmployeeNoNopattern = @"^[a-zA-Z0-9]{1,10}$";

                dt_companyName = objcon.GetDataTable("select CompanyId as Id, CompanyName as Name from Mas_CompanyProfile  where Deactivate = 0");
                dt_company = objcon.GetDataTable("select CompanyId as Id, CompanyName as Name from Mas_CompanyProfile  where Deactivate = 0  and  CompanyId in ( Select distinct CmpID from UserBranchMapping where IsActive=1 and EmployeeID=" + Session["EmployeeId"] + ") order by CompanyName ;");
                // dt_BU = objcon.GetDataTable("select BranchId ,BranchName ,Convert (int,CmpId) as CmpId  from Mas_Branch where Deactivate=0");
                dt_BU = objcon.GetDataTable("Select UserBranchMapping.BranchId , Mas_Branch.BranchName,Convert (int,UserBranchMapping.CmpID) as CmpId from UserBranchMapping,Mas_Branch where UserBranchMapping.IsActive=1 and Mas_Branch.deactivate=0 and UserBranchMapping.BranchID=Mas_Branch.branchid and UserBranchMapping.EmployeeID='" + Session["EmployeeId"] + "'");

                dt_ManpowerCategory = objcon.GetDataTable("Select KPISubCategoryId as Id,KPISubCategoryFName as Name from KPI_SubCategory  where KPI_SubCategory.Deactivate=0 ");
                //dt_Contractor = objcon.GetDataTable("select ContractorId as Id,ContractorName as Name from Contractor_Master where Deactivate=0 order by ContractorName");
                dt_Contractor = objcon.GetDataTable("Select Mas_ContractorMapping.ContractorID as Id,Contractor_Master.ContractorName as Name,Convert (int,Mas_ContractorMapping.BranchID) as BranchID,Convert (int,Mas_ContractorMapping.CmpID) as CmpID  from Mas_ContractorMapping,Contractor_Master where Mas_ContractorMapping.IsActive=1 and Mas_ContractorMapping.ContractorID=Contractor_Master.ContractorId and Contractor_Master.Deactivate=0 and Mas_ContractorMapping.ContractorID<>1 order by ContractorName");
                dt_Qualification = objcon.GetDataTable("Select QualificationPFId as Id ,QualificationPFName As Name from Mas_Qualification_PF where Deactivate=0 order by Name");
                dt_Department = objcon.GetDataTable("select DepartmentId as Id, DepartmentName as Name from Mas_Department where Deactivate=0");
                dt_SubDepartment = objcon.GetDataTable("select SubDepartmentId ,SubDepartmentName ,Convert (int,DepartmentId) As DepartmentId from Mas_SubDepartment where Deactivate=0");
                //dt_Designation = objcon.GetDataTable("Select DesignationId as Id,DesignationName as Name,Convert (int,KPI_SubCategory.KPISubCategoryId) as KPISubCategoryId from Mas_Designation,KPI_CategoryDepartment,KPI_SubCategory where Mas_Designation.Deactivate=0 and KPI_SubCategory.Deactivate=0 and KPI_SubCategory.KPISubCategoryId=KPI_CategoryDepartment.KPI_Category_CategoryId and Mas_Designation.DesignationId=KPI_CategoryDepartment.KPI_Category_DesignationId");
                dt_Designation = objcon.GetDataTable("select DesignationId as Id,DesignationName as Name from Mas_Designation where Deactivate=0 and ApplicableForWorker=1");
                dt_Grade = objcon.GetDataTable("select GradeId as Id,GradeName as Name from Mas_Grade where Deactivate=0 ");
                dt_Line = objcon.GetDataTable("select LineId ,LineName ,Convert (int,BranchId) as BranchId,Convert (int,CmpId) as CmpId from Mas_LineMaster where Deactivate=0 ");
                dt_SubUnit = objcon.GetDataTable("select UnitId,UnitName,Convert (int,CmpId) as CmpId,Convert (int,UnitBranchId) as UnitBranchId from Mas_Unit where Deactivate=0 ");
                dt_ShiftGroup = objcon.GetDataTable("select ShiftGroupId,ShiftGroupFName,Convert (int,CmpId) as CmpId, Convert (int,ShiftGroupBranchId) As ShiftGroupBranchId from Atten_ShiftGroups where Deactivate=0");
                dt_ShiftRule = objcon.GetDataTable("select ShiftRuleId,ShiftRuleName,Convert (int,CmpId) As CmpId,Convert (int,ShiftRuleBranchId) as ShiftRuleBranchId from Atten_ShiftRule where Deactivate=0 ");
                dt_AssessmentLevel = objcon.GetDataTable("Select EmployeeLevelId as Id ,EmployeeLevel As Name from Mas_EmployeeLevel where Deactivate=0");
                dt_CriticalStage = objcon.GetDataTable(" Select CriticalStageId As Id,CriticalStageName as Name from Mas_CriticalStageCategory where Mas_CriticalStageCategory.Deactivate=0");
                dt_RelationWithNominee = objcon.GetDataTable("Select RelationId as Id,RelationName as Name from Mas_relation where Deactivate=0");

                StringBuilder strBuilder = new StringBuilder();
                int i = 0;
                string str_aadhar = "";
                string str_EmployeeNo = "";
                string str_EmployeeCardNo = "";
                foreach (var Data in tbldata)
                {
                    str_aadhar = str_aadhar + "'" + Data.AadhaarNo.ToString().Replace("'", "") + "',";
                    str_EmployeeNo = str_EmployeeNo + "'" + Data.EmployeeNo + "',";
                    str_EmployeeCardNo += "'" + Data.EmployeeCardNo.PadLeft(10, '0') + "',";

                    //  string a = str_EmployeeCardNo.ToString("D10");
                }

                if (str_aadhar != "")
                {
                    str_aadhar = str_aadhar.Substring(0, str_aadhar.Length - 1);
                }

                if (str_EmployeeNo != "")
                {
                    str_EmployeeNo = str_EmployeeNo.Substring(0, str_EmployeeNo.Length - 1);
                }

                if (str_EmployeeCardNo != "")
                {
                    str_EmployeeCardNo = str_EmployeeCardNo.Substring(0, str_EmployeeCardNo.Length - 1);


                }

                var ListEmployeeNo = DapperORM.DynamicQueryList("Select CompanyName as Company,BranchName as BU, EmployeeName as Employee,DepartmentName,EmployeeNo,EmployeeCardNo from Mas_Employee,Mas_CompanyProfile,Mas_Branch,Mas_Department  where  Mas_Employee.Deactivate=0  and Mas_CompanyProfile.Deactivate=0 and Mas_Branch.Deactivate=0 and  Mas_Department.Deactivate=0 and Mas_Employee.CmpID=Mas_CompanyProfile.CompanyId and Mas_Employee.EmployeeBranchId=BranchId and Mas_Employee.EmployeeDepartmentID=DepartmentId and EmployeeNo in (" + str_EmployeeNo + ")").ToList();
                if (ListEmployeeNo.Count != 0)
                {
                    ViewBag.GetListEmployeeNo = ListEmployeeNo;
                    return Json(new { ViewBag.GetListEmployeeNo }, JsonRequestBehavior.AllowGet);
                }
                var ListEmployeeCardNo = DapperORM.DynamicQueryList("Select CompanyName as Company,BranchName as BU, EmployeeName as Employee,DepartmentName,EmployeeNo,EmployeeCardNo from Mas_Employee,Mas_CompanyProfile,Mas_Branch,Mas_Department  where  Mas_Employee.Deactivate=0  and Mas_CompanyProfile.Deactivate=0 and Mas_Branch.Deactivate=0 and  Mas_Department.Deactivate=0 and Mas_Employee.CmpID=Mas_CompanyProfile.CompanyId and Mas_Employee.EmployeeBranchId=BranchId and Mas_Employee.EmployeeDepartmentID=DepartmentId and EmployeeCardNo in (" + str_EmployeeCardNo + ")").ToList();
                if (ListEmployeeCardNo.Count != 0)
                {
                    ViewBag.GetListEmployeeCardNo = ListEmployeeCardNo;
                    return Json(new { ViewBag.GetListEmployeeCardNo }, JsonRequestBehavior.AllowGet);
                }
                try
                {
                    var ListEmployeeAadharNo = DapperORM.DynamicQueryList("Select CompanyName as Company,BranchName as BU, EmployeeName as Employee,DepartmentName,EmployeeNo,EmployeeCardNo, Mas_Employee_Personal.AadhaarNo as  AadhaarNo from Mas_Employee,Mas_CompanyProfile,Mas_Branch,Mas_Department,Mas_Employee_Personal  where Mas_Employee.Employeeleft=0 and   Mas_Employee.Deactivate=0 and  Mas_Employee_Personal.Deactivate=0  and Mas_CompanyProfile.Deactivate=0 and Mas_Branch.Deactivate=0 and  Mas_Department.Deactivate=0 and Mas_Employee.CmpID=Mas_CompanyProfile.CompanyId and Mas_Employee.EmployeeBranchId=BranchId and Mas_Employee.EmployeeDepartmentID=DepartmentId and Mas_Employee.employeeId=Mas_Employee_Personal.PersonalEmployeeId and Mas_Employee_Personal.AadhaarNo in (" + str_aadhar + ")").ToList();
                    if (ListEmployeeAadharNo.Count != 0)
                    {
                        ViewBag.GetListEmployeeAadharNo = ListEmployeeAadharNo;
                        return Json(new { ViewBag.GetListEmployeeAadharNo }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    var Message = "Invalid Aadhaar No";
                    var Icon = "error";
                    var rowno = "";
                    return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                }


                foreach (var Data in tbldata)
                {
                    List_Contractor_Id = 0;
                    List_CompanyID = 0;
                    List_BUId = 0;
                    List_Qualification_Id = 0;
                    List_Department_Id = 0;
                    List_SubDepatment_Id = 0;
                    List_Designation_Id = 0;
                    List_Grade_Id = 0;
                    List_Line_Id = 0;
                    List_Unit_Id = 0;
                    List_ShiftGroup_Id = 0;
                    List_ShiftRule_Id = 0;
                    List_AssessmentLevel_Id = 0;
                    List_CriticalStage_Id = 0;
                    List_RelationWithNominee_Id = 0;


                    CompanyID_Id = 0;
                    Branch_Id = 0;
                    Contractor_Id = 0;
                    Qualification_Id = 0;
                    Department_Id = 0;
                    SubDepatment_Id = 0;
                    Designation_Id = 0;
                    Grade_Id = 0;
                    Line_Id = 0;
                    Unit_Id = 0;
                    ShiftGroup_Id = 0;
                    ShiftRule_Id = 0;
                    AssessmentLevel_Id = 0;
                    CriticalStage_Id = 0;
                    ManpowerCategory_Id = 0;
                    GetIsCriticalStageApplicable = false;
                    RelationWithNominee_Id = 0;
                    //GetEM_Atten_OT_Applicable = false;


                    if (Data.EmployeeNo == null)
                    {
                        var Message = "Enter Employee no in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }
                    if (Data.EmployeeNo != null)
                    {
                        if (!Regex.IsMatch(Data.EmployeeNo, EmployeeNoNopattern))
                        {
                            var Message = "Enter a valid employee no, and make sure the employee no should not more than 10 digits in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                    }

                    ////EmployeeNo   check  end ----------------------------------------------------------------------------------------

                    if (Data.EmployeeCardNo == null)
                    {
                        var Message = "Enter Employee card no in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }
                    if (Data.EmployeeCardNo != null)
                    {
                        if (!Regex.IsMatch(Data.EmployeeCardNo, EmployeeNoNopattern))
                        {
                            var Message = "Employee card no should not more than 10 digit in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    ////EmployeeCardNo   check  end ----------------------------------------------------------------------------------------
                    if (Data.AadhaarNo == null)
                    {
                        var Message = "Enter Aadhaar no  in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }
                    if (!Regex.IsMatch(Data.AadhaarNo, AdharNopattern))
                    {
                        var Message = "Enter Valid AadhaarNo. in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }

                    if (Data.PF_UAN == null)
                    {
                        var Message = "Enter UAN no in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }

                    if (Data.PF_UAN != "Pending")
                    {
                        if (!Regex.IsMatch(Data.PF_UAN, AdharNopattern))
                        {
                            var Message = "Enter Valid UAN No. in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    ////AadhaarNo   check  end ----------------------------------------------------------------------------------------




                    //CmpID check   ----------------------------------------------------------------------------------------
                    if (Data.CmpID == null)
                    {
                        var Message = "Enter Company Name in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }

                    GetCompany_Id_Name(Data.CmpID.ToString(), out CompanyID_Id);
                    // List_CompanyID = CompanyID_Id;
                    if (CompanyID_Id == 0)
                    {
                        var Message = "Company Name not valid in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }


                    GetCompany_Id(Data.CmpID.ToString(), out CompanyID_Id);
                    List_CompanyID = CompanyID_Id;
                    if (List_CompanyID == 0)
                    {
                        var Message = "Company name is not mapped to you in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }
                    ////CmpID check  end ----------------------------------------------------------------------------------------

                    if (Data.EmployeeBranchId == null)
                    {
                        var Message = "Enter Business unit in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }

                    GetBranch_Id_Name(Data.EmployeeBranchId.ToString(), out Branch_Id);
                    //   List_BUId = Branch_Id;
                    if (Branch_Id == 0)
                    {
                        var Message = "Business unit not valid in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }

                    GetBranch_Id(Data.EmployeeBranchId.ToString(), List_CompanyID, out Branch_Id);
                    List_BUId = Branch_Id;
                    if (List_BUId == 0)
                    {
                        var Message = "Business unit is not mapped to you in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }
                    ////Business unit  check  end ----------------------------------------------------------------------------------------

                    if (Data.ContractorID == null)
                    {
                        var Message = "Enter Contractor name in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }

                    GetContractor_Id_Name(Data.ContractorID.ToString(), out Contractor_Id);
                    //List_Contractor_Id = Contractor_Id;
                    if (Contractor_Id == 0)
                    {
                        var Message = "Contractor name not valid in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }

                    GetContractor_Id(Data.ContractorID.ToString(), out Contractor_Id, List_CompanyID, List_BUId);
                    List_Contractor_Id = Contractor_Id;
                    if (List_Contractor_Id == 0)
                    {
                        var Message = "Contractor name is not mapped to the specified business unit in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }
                    ////Contractor name  check  end ----------------------------------------------------------------------------------------

                    if (Data.Salutation == null)
                    {
                        var Message = "Enter salutation in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }
                    else if (Data.Salutation == "Mr." || Data.Salutation == "Ms." || Data.Salutation == "Mrs.")
                    {

                    }
                    else
                    {
                        var Message = "Please Enter proper salutation(Mr./Ms./Mrs.) in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }

                    ////Salutation name  check  end ----------------------------------------------------------------------------------------

                    if (Data.EmployeeName == null)
                    {
                        var Message = "Enter employee name in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }

                    ////EmployeeName name  check  end ----------------------------------------------------------------------------------------



                    if (Data.PrimaryMobile != null)
                    {
                        if (!Regex.IsMatch(Data.PrimaryMobile, MobileNopattern))
                        {
                            var Message = "Enter valid Mobile no in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    ////PrimaryMobile   check  end ----------------------------------------------------------------------------------------

                    if (Data.Gender == null)
                    {
                        var Message = "Enter gender in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }
                    else if (Data.Gender == "Male" || Data.Gender == "Female")
                    {

                    }
                    else
                    {
                        var Message = "Please enter proper gender in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }
                    ////Gender   check  end ----------------------------------------------------------------------------------------

                    if (Data.BirthdayDate == null)
                    {
                        var Message = "Enter birthday date in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }



                    ////BirthdayDate   check  end ----------------------------------------------------------------------------------------
                    if (Data.JoiningDate == null)
                    {
                        var Message = "Enter joining date in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }

                    ////JoiningDate   check  end ----------------------------------------------------------------------------------------


                    //if (Data.MaritalStatus == null)
                    //{
                    //    var Message = "Enter marital status in SrNo " + Data.SrNo;
                    //    var Icon = "error";
                    //    var rowno = Data.SrNo;
                    //    return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    //}
                    //else if (Data.MaritalStatus == "Unmarried" || Data.MaritalStatus == "Married" || Data.MaritalStatus == "Widow")
                    //{

                    //}
                    //else
                    //{
                    //    var Message = "Please enter proper marital status in SrNo " + Data.SrNo;
                    //    var Icon = "error";
                    //    var rowno = Data.SrNo;
                    //    return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    //}
                    ////Gender   check  end ----------------------------------------------------------------------------------------

                    //if (Data.EM_Atten_WOFF1 == null)
                    //{
                    //    var Message = "Enter weekly off in SrNo " + Data.SrNo;
                    //    var Icon = "error";
                    //    var rowno = Data.SrNo;
                    //    return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    //}
                    //else if (Data.EM_Atten_WOFF1 == "Sunday" || Data.EM_Atten_WOFF1 == "Monday" || Data.EM_Atten_WOFF1 == "Tuesday" || Data.EM_Atten_WOFF1 == "Wednesday" || Data.EM_Atten_WOFF1 == "Thursday" || Data.EM_Atten_WOFF1 == "Friday" || Data.EM_Atten_WOFF1 == "Saturday" || Data.EM_Atten_WOFF1 == "NA")
                    //{

                    //}
                    //else
                    //{
                    //    var Message = "Please enter proper weekly off in SrNo " + Data.SrNo;
                    //    var Icon = "error";
                    //    var rowno = Data.SrNo;
                    //    return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    //}


                    //if (Data.EmployeeQualificationID == null)
                    //{
                    //    var Message = "Enter Qualification in SrNo " + Data.SrNo;
                    //    var Icon = "error";
                    //    var rowno = Data.SrNo;
                    //    return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    //}
                    //if (Data.EmployeeQualificationID != null)
                    //{
                    //    GetQualification_Id(Data.EmployeeQualificationID.ToString(), out Qualification_Id);
                    //    List_Qualification_Id = Qualification_Id;
                    //    if (List_Qualification_Id == 0)
                    //    {
                    //        var Message = "Qualification not valid in SrNo " + Data.SrNo;
                    //        var Icon = "error";
                    //        var rowno = Data.SrNo;
                    //        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    //    }
                    //}

                    ////Employee Qualification  check  end ----------------------------------------------------------------------------------------

                    if (Data.EmployeeDepartmentID == null)
                    {
                        var Message = "Enter department name in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }

                    GetDepartment_Id(Data.EmployeeDepartmentID.ToString(), out Department_Id);
                    List_Department_Id = Department_Id;
                    if (List_Department_Id == 0)
                    {
                        var Message = "Department name not valid in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }
                    ////Employee Department  check  end ----------------------------------------------------------------------------------------

                    if (Data.EmployeeSubDepartmentName == null)
                    {
                        var Message = "Enter sub department name in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }

                    GetSubDepatment_Id(Data.EmployeeSubDepartmentName.ToString(), List_Department_Id, out SubDepatment_Id);
                    List_SubDepatment_Id = SubDepatment_Id;
                    if (List_SubDepatment_Id == 0)
                    {
                        var Message = "Sub Department name not valid in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }
                    ////Employee sub  Department  check  end ----------------------------------------------------------------------------------------


                    if (Data.EmployeeAllocationCategoryId == null)
                    {
                        var Message = "Enter manpower category name in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }

                    GetManpowerCategory_Id(Data.EmployeeAllocationCategoryId.ToString(), List_CompanyID, List_BUId, out ManpowerCategory_Id);
                    List_ManpowerCategory_Id = ManpowerCategory_Id;
                    if (List_ManpowerCategory_Id == 0)
                    {
                        var Message = "Manpower category not valid in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }
                    ////Employee Man power  check  end ----------------------------------------------------------------------------------------

                    if (Data.EmployeeDesignationID == null)
                    {
                        var Message = "Enter designation name in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }

                    GetDesignation_Id(Data.EmployeeDesignationID.ToString(), out Designation_Id);
                    List_Designation_Id = Designation_Id;
                    if (List_Designation_Id == 0)
                    {
                        var Message = "Designation name not valid in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }
                    ////Employee Desigantion  check  end ----------------------------------------------------------------------------------------

                    if (Data.EmployeeGradeID == null)
                    {
                        var Message = "Enter grade name in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }

                    GetGrade_Id(Data.EmployeeGradeID.ToString(), out Grade_Id);
                    List_Grade_Id = Grade_Id;
                    if (List_Grade_Id == 0)
                    {
                        var Message = "Grade name not valid in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }
                    //ask comment start
                    //Employee Grade  check  end ----------------------------------------------------------------------------------------

                    if (Data.EmployeeLineID == null)
                    {
                        var Message = "Enter line name in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }

                    GetLine_Id(Data.EmployeeLineID.ToString(), List_CompanyID, List_BUId, out Line_Id);
                    List_Line_Id = Line_Id;
                    if (List_Grade_Id == 0)
                    {
                        var Message = "Line name not valid in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }
                    //Employee Line  check  end ----------------------------------------------------------------------------------------

                    if (Data.EmployeeUnitID == null)
                    {
                        var Message = "Enter sub unit in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }

                    GetUnit_Id(Data.EmployeeUnitID.ToString(), List_CompanyID, List_BUId, out Unit_Id);
                    List_Unit_Id = Unit_Id;
                    if (List_Unit_Id == 0)
                    {
                        var Message = "Sub unit not valid in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }
                    //Employee Sub Unit check  end ----------------------------------------------------------------------------------------

                    if (Data.EM_Atten_ShiftGroupId == null)
                    {
                        var Message = "Enter shift group in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }

                    GetShiftGroup_Id(Data.EM_Atten_ShiftGroupId.ToString(), List_CompanyID, List_BUId, out ShiftGroup_Id);
                    List_ShiftGroup_Id = ShiftGroup_Id;
                    if (List_ShiftGroup_Id == 0)
                    {
                        var Message = "Shift group not valid in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }
                    //Employee Shift Group check  end ----------------------------------------------------------------------------------------

                    if (Data.EM_Atten_ShiftRuleId == null)
                    {
                        var Message = "Enter shift rule in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }

                    GetShiftRule_Id(Data.EM_Atten_ShiftRuleId.ToString(), List_CompanyID, List_BUId, out ShiftRule_Id);
                    List_ShiftRule_Id = ShiftRule_Id;
                    if (List_ShiftRule_Id == 0)
                    {
                        var Message = "Shift rule not valid in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }
                    //Employee Shift Rule check  end ----------------------------------------------------------------------------------------

                    if (Data.EmployeeLevelID == null)
                    {
                        var Message = "Assessment Level in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }

                    GetAssessmentLevel_Id(Data.EmployeeLevelID.ToString(), out AssessmentLevel_Id);
                    List_AssessmentLevel_Id = AssessmentLevel_Id;
                    if (List_AssessmentLevel_Id == 0)
                    {
                        var Message = "Assessment Level not valid in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }
                    //Employee GetAssessmentLevel_Id check  end ----------------------------------------------------------------------------------------
                    //ask comment end
                    if (Data.IsCriticalStageApplicable == "Yes")
                    {
                        GetIsCriticalStageApplicable = true;
                        if (Data.EmployeeCriticalStageID == null)
                        {
                            var Message = "Critical stage in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetCriticalStage_Id(Data.EmployeeCriticalStageID.ToString(), out CriticalStage_Id);
                        List_CriticalStage_Id = CriticalStage_Id;
                        if (List_CriticalStage_Id == 0)
                        {
                            var Message = "Critical stage not valid in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        GetIsCriticalStageApplicable = false;
                    }

                    if (Data.JoiningDate != null)
                    {
                        try
                        {
                            var GetDate = Convert.ToDateTime(Data.JoiningDate).ToString("yyyy-MM-dd");
                        }
                        catch (Exception ex)
                        {
                            var Message = "Incorrect Joining date in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }


                    }

                    if (Data.BirthdayDate != null)
                    {
                        try
                        {
                            var GetDate = Convert.ToDateTime(Data.BirthdayDate).ToString("yyyy-MM-dd");
                        }
                        catch (Exception ex)
                        {
                            var Message = "Incorrect Birthdate in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }


                    }
                    ////Employee Critical stage check  end ----------------------------------------------------------------------------------------
                    if (Data.PF_Reletion1 != null)
                    {
                        GetReationNonimee_Id(Data.PF_Reletion1.ToString(), out RelationWithNominee_Id);
                        List_RelationWithNominee_Id = RelationWithNominee_Id;
                        if (List_RelationWithNominee_Id == 0)
                        {
                            var Message = "Relation/Nonimee  not valid in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                    }

                    ////Employee Relation  end ----------------------------------------------------------------------------------------

                    //if (Data.PF_FSType == null)
                    //{
                    //    var Message = "Enter Father/Mother/Spouse relation in SrNo " + Data.SrNo;
                    //    var Icon = "error";
                    //    var rowno = Data.SrNo;
                    //    return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    //}
                    //if (Data.PF_FS_Name == null)
                    //{
                    //    var Message = "Enter Father/Mother/Spouse Name in SrNo " + Data.SrNo;
                    //    var Icon = "error";
                    //    var rowno = Data.SrNo;
                    //    return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    //}



                    //if (Data.EM_Atten_OT_Applicable == "Yes")
                    //{
                    //    GetEM_Atten_OT_Applicable = true;
                    //}
                    //else if (Data.EM_Atten_OT_Applicable == "No")
                    //{
                    //    GetEM_Atten_OT_Applicable = false;
                    //}
                    //else if (Data.EM_Atten_OT_Applicable != "Yes" || Data.EM_Atten_OT_Applicable != "No")
                    //{
                    //    var Message = "OT Applicable should be in Yes/No in SrNo" + Data.SrNo;
                    //    var Icon = "error";
                    //    var rowno = Data.SrNo;
                    //    return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);

                    //}

                    //////Employee EM_Atten_OT_Applicable check  end ----------------------------------------------------------------------------------------
                    //try
                    //{
                    //    if (Convert.ToInt32(Data.EM_Atten_OTMultiplyBy) > 9 || Convert.ToInt32(Data.EM_Atten_OTMultiplyBy) < 1)
                    //    {

                    //        var Message = "OT Multiply By should be in (1-9) in SrNo" + Data.SrNo;
                    //        var Icon = "error";
                    //        var rowno = Data.SrNo;
                    //        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    //    }
                    //}
                    //catch 
                    //{
                    //    var Message = "OT Multiply By should not allowed in decimal in SrNo" + Data.SrNo;
                    //    var Icon = "error";
                    //    var rowno = Data.SrNo;
                    //    return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    //}



                    ////Employee EM_Atten_OTMultiplyBy check  end ----------------------------------------------------------------------------------------

                    if (i == 0)
                    {
                        strBuilder.Append("DECLARE @IdentityValue_EmployeeID AS TABLE(ID INT); ");

                        strBuilder.Append("DECLARE @IdentityValue_PersonalEmployeeID AS TABLE(ID INT); ");

                        strBuilder.Append("DECLARE @IdentityValue_AttendanceEmployeeID AS TABLE(ID INT); ");

                        strBuilder.Append("DECLARE @IdentityValue_AddressEmployeeID AS TABLE(ID INT); ");

                        strBuilder.Append("DECLARE @IdentityValue_StatutoryEmployeeID AS TABLE(ID INT); ");

                        strBuilder.Append("DECLARE @EmployeeId  int ");
                    }


                    string Employee_Admin = "  if(exists(select AadhaarNo from Mas_Employee_Personal,Mas_Employee where EmployeeId=PersonalEmployeeId and  AadhaarNo= '" + Data.AadhaarNo + "' and Mas_Employee_Personal.Deactivate=0  and Mas_Employee.Deactivate=0 and EmployeeLeft=0))" +
                                              " Begin  " +
                                                "   Set @EmployeeId=(select EmployeeId from Mas_Employee_Personal,Mas_Employee where EmployeeId=PersonalEmployeeId and  AadhaarNo='" + Data.AadhaarNo + "' and Mas_Employee_Personal.Deactivate=0  and Mas_Employee.Deactivate=0 and EmployeeLeft=0)  " +
                                                "   Insert into Log_Mas_Employee  (EmployeeId, EmployeeId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, CmpID, PreboardingFid, EmployeeBranchId, EmployeeOrigin, EmployeeSeries, EmployeeNo, EmployeeCardNo, LocalExpat, IsNRI, Salutation, EmployeeName, EmployeeLevelID, EmployeeWageID, EmployeeDepartmentID, EmployeeSubDepartmentName, EmployeeDesignationID, EmployeeGradeID, EmployeeGroupID, EmployeeTypeID, EmployeeCostCenterID, EmployeeZoneID, EmployeeUnitID, ContractorID, IsCriticalStageApplicable, EmployeeCriticalStageID, EmployeeLineID, JoiningDate, IsJoiningSpecial, JoiningStatus, TraineeDueDate, ProbationDueDate, ConfirmationDate, IsConfirmation, ConfirmationBy, CompanyMobileNo, CompanyMailID, ReportingHR, ReportingManager1, ReportingManager2, ReportingAccount, NoOfBranchTransfer, IsReasigned, ResignationDate, NoticePeriodDays, IsExit, ExitDate, EmployeeLeft, LeavingDate, LeavingReason, LeavingReasonPF, LeavingReasonESI, EM_PayrollLastDate, VMSApplicable, IsVMSMultipleLocation, LeftEmployeeApprovedBy,EM_Atten_DailyMonthly)    " +
                                                "   Select EmployeeId, EmployeeId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, CmpID, PreboardingFid, EmployeeBranchId, EmployeeOrigin, EmployeeSeries, EmployeeNo, EmployeeCardNo, LocalExpat, IsNRI, Salutation, EmployeeName, EmployeeLevelID, EmployeeWageID, EmployeeDepartmentID, EmployeeSubDepartmentName, EmployeeDesignationID, EmployeeGradeID, EmployeeGroupID, EmployeeTypeID, EmployeeCostCenterID, EmployeeZoneID, EmployeeUnitID, ContractorID, IsCriticalStageApplicable, EmployeeCriticalStageID, EmployeeLineID, JoiningDate, IsJoiningSpecial, JoiningStatus, TraineeDueDate, ProbationDueDate, ConfirmationDate, IsConfirmation, ConfirmationBy, CompanyMobileNo, CompanyMailID, ReportingHR, ReportingManager1, ReportingManager2, ReportingAccount, NoOfBranchTransfer, IsReasigned, ResignationDate, NoticePeriodDays, IsExit, ExitDate, EmployeeLeft, LeavingDate, LeavingReason, LeavingReasonPF, LeavingReasonESI, EM_PayrollLastDate, VMSApplicable, IsVMSMultipleLocation, LeftEmployeeApprovedBy,EM_Atten_DailyMonthly from Mas_Employee where Mas_Employee.Deactivate = 0 and Mas_Employee.EmployeeId = @EmployeeId    " +
                                                "   Insert into Log_Mas_Employee_Attendance(AttendanceId, AttendanceId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, AttendanceEmployeeId, EmployeeCardNo, EM_Atten_OT_Applicable, EM_Atten_OTMultiplyBy, EM_Atten_PerDayShiftHrs, EM_Atten_CoffApplicable, EM_Atten_CoffSettingId, EM_Atten_ShiftGroupId, EM_Atten_ShiftRuleId, EM_Atten_WOFF1, EM_Atten_WOFF2, EM_Atten_WOFF2_ForCheck, EM_Atten_WOFF_Check1, EM_Atten_WOFF_Check2, EM_Atten_WOFF_Check3, EM_Atten_WOFF_Check4, EM_Atten_WOFF_Check5, EM_Atten_LateMarkSettingApplicable, EM_Atten_LateMarkSettingId, EM_Atten_IsSalaryFullPay, EM_Atten_LeaveGroupId, EM_Atten_DefaultAttenShow, EM_Atten_SinglePunch_Present, EM_Atten_RotationalWeekOff, EM_Atten_Regularization_Required, EM_Atten_ShortLeaveApplicable, EM_Atten_ShortLeaveSettingId, EM_Atten_PersonalGatepassApplicable, EM_Atten_Atten_PersonalGatepassSettingId, EM_Atten_AttendanceLastDate, EM_Atten_PunchMissingApplicable, EM_Atten_Atten_PunchMissingSettingId, EM_Atten_flexibleShiftApplicable, EM_Atten_Atten_OutDoorCompanySettingId, EM_Atten_OutDoorCompanyApplicable, EM_Atten_WOPH_CoffApplicable, PHApplicable)    " +
                                                "   Select AttendanceId, AttendanceId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, AttendanceEmployeeId, EmployeeCardNo, EM_Atten_OT_Applicable, EM_Atten_OTMultiplyBy, EM_Atten_PerDayShiftHrs, EM_Atten_CoffApplicable, EM_Atten_CoffSettingId, EM_Atten_ShiftGroupId, EM_Atten_ShiftRuleId, EM_Atten_WOFF1, EM_Atten_WOFF2, EM_Atten_WOFF2_ForCheck, EM_Atten_WOFF_Check1, EM_Atten_WOFF_Check2, EM_Atten_WOFF_Check3, EM_Atten_WOFF_Check4, EM_Atten_WOFF_Check5, EM_Atten_LateMarkSettingApplicable, EM_Atten_LateMarkSettingId, EM_Atten_IsSalaryFullPay, EM_Atten_LeaveGroupId, EM_Atten_DefaultAttenShow, EM_Atten_SinglePunch_Present, EM_Atten_RotationalWeekOff, EM_Atten_Regularization_Required, EM_Atten_ShortLeaveApplicable, EM_Atten_ShortLeaveSettingId, EM_Atten_PersonalGatepassApplicable, EM_Atten_Atten_PersonalGatepassSettingId, EM_Atten_AttendanceLastDate, EM_Atten_PunchMissingApplicable, EM_Atten_Atten_PunchMissingSettingId, EM_Atten_flexibleShiftApplicable, EM_Atten_Atten_OutDoorCompanySettingId, EM_Atten_OutDoorCompanyApplicable, EM_Atten_WOPH_CoffApplicable, PHApplicable from Mas_Employee_Attendance where Mas_Employee_Attendance.Deactivate = 0 and Mas_Employee_Attendance.AttendanceEmployeeId = @EmployeeId    " +
                                                "   Insert into Log_Mas_Employee_Personal(PersonalId, PersonalId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, PersonalEmployeeId, AadhaarNo, NameAsPerAadhaar, AadhaarNoMobileNoLink, AadhaarNoMobileNo, PAN, NameAsPerPan, PANAadhaarLink, PrimaryMobile, SecondaryMobile, WhatsAppNo, PersonalEmailId, BirthdayDate, AgeOfJoining, BirthdayPlace, BirthdayProofOfDocumentID, BirthdayProofOfCertificateNo, IsDOBSpecial, EmployeeQualificationID, QualificationRemark, Gender, BloodGroup, MaritalStatus, AnniversaryDate, Ifyouwantdonotdisclosemygenderthentick, PhysicallyDisabled, PhysicallyDisableType, PhysicallyDisableRemark, IdentificationMark, DrivingLicenceNo, DrivingLicenceExpiryDate, PassportNo, PassportExpiryDate, EmployeeReligionID, Ifyouwantdonotdisclosemyreligioncastthentick, EmployeeCasteID, EmployeeSpecificDegree, EmployeeBirthProofEducation, EmployeeSubCategory)    " +
                                                "   Select PersonalId, PersonalId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, PersonalEmployeeId, AadhaarNo, NameAsPerAadhaar, AadhaarNoMobileNoLink, AadhaarNoMobileNo, PAN, NameAsPerPan, PANAadhaarLink, PrimaryMobile, SecondaryMobile, WhatsAppNo, PersonalEmailId, BirthdayDate, AgeOfJoining, BirthdayPlace, BirthdayProofOfDocumentID, BirthdayProofOfCertificateNo, IsDOBSpecial, EmployeeQualificationID, QualificationRemark, Gender, BloodGroup, MaritalStatus, AnniversaryDate, Ifyouwantdonotdisclosemygenderthentick, PhysicallyDisabled, PhysicallyDisableType, PhysicallyDisableRemark, IdentificationMark, DrivingLicenceNo, DrivingLicenceExpiryDate, PassportNo, PassportExpiryDate, EmployeeReligionID, Ifyouwantdonotdisclosemyreligioncastthentick, EmployeeCasteID, EmployeeSpecificDegree, EmployeeBirthProofEducation, EmployeeSubCategory from Mas_Employee_Personal where Mas_Employee_Personal.Deactivate = 0 and Mas_Employee_Personal.PersonalEmployeeId = @EmployeeId    " +

                                              " Update Mas_Employee Set EmployeeLeft=1 ,LeavingDate=GETDATE(),LeftEmployeeApprovedBy='" + Session["EmployeeName"] + "' where EmployeeId= @EmployeeId  " +
                                              " End  " +
                                              " " +
                                              " " +
                                              "  Insert Into mas_Employee ( " +
                                              "  Deactivate  " +
                                              " ,UseBy  " +
                                              " ,CreatedBy  " +
                                              " ,CreatedDate  " +
                                              " ,MachineName  " +
                                              " ,CmpID  " +
                                              " ,EmployeeBranchId  " +
                                              " ,EmployeeNo  " +
                                              " ,EmployeeCardNo  " +
                                              " ,Salutation  " +
                                              " ,EmployeeName  " +
                                              " ,ContractorID  " +
                                              " ,EmployeeDepartmentID  " +
                                              " ,EmployeeSubDepartmentName " +
                                              " ,EmployeeDesignationID  " +
                                              " ,EmployeeGradeID  " +
                                              " ,EmployeeUnitID  " +
                                              " ,EmployeeLevelID  " +
                                              " ,ConfirmationDate  " +
                                              " ,ProbationDueDate  " +
                                              " ,TraineeDueDate  " +
                                              " ,IsCriticalStageApplicable  " +
                                              " ,EmployeeCriticalStageID  " +
                                              " ,EmployeeLineID  " +
                                              " ,EmployeeOrigin  " +
                                              " ,EM_Atten_DailyMonthly " +
                                              " ,EmployeeAllocationCategoryId  " +
                                              " ,JoiningDate " +
                                              " ) OUTPUT Inserted.EmployeeId INTO @IdentityValue_EmployeeID values" +
                                              "('0','0','" + Session["EmployeeName"] + "',GetDate(),'" + Dns.GetHostName().ToString() + "','" + List_CompanyID + "'," +
                                              "'" + List_BUId + "'," +
                                              "LTRIM(RTRIM(replace('" + Data.EmployeeNo + "', ' ', '')))," +
                                              "RIGHT('000000000' + LTRIM(RTRIM(replace('" + Data.EmployeeCardNo + "', ' ', ''))), 10)," +
                                              "'" + Data.Salutation + "'," +
                                              "'" + Data.EmployeeName + "'," +
                                              "'" + List_Contractor_Id + "'," +
                                              "'" + List_Department_Id + "'," +
                                              "'" + List_SubDepatment_Id + "'," +
                                              "'" + List_Designation_Id + "'," +
                                              "'" + List_Grade_Id + "'," +
                                              "'" + List_Unit_Id + "'," +
                                              "'" + List_AssessmentLevel_Id + "'," +
                                              "'" + Convert.ToDateTime(Data.JoiningDate).ToString("yyyy-MM-dd") + "'," +
                                              "'" + Convert.ToDateTime(Data.JoiningDate).ToString("yyyy-MM-dd") + "'," +
                                              "'" + Convert.ToDateTime(Data.JoiningDate).ToString("yyyy-MM-dd") + "'," +
                                              "'" + GetIsCriticalStageApplicable + "'," +
                                              "'" + List_CriticalStage_Id + "'," +
                                              "'" + List_Line_Id + "'," +
                                              "'Bulk'," +
                                              "'Daily'," +
                                              "'" + List_ManpowerCategory_Id + "'," +
                                              "'" + Convert.ToDateTime(Data.JoiningDate).ToString("yyyy-MM-dd") + "'" + ")  " +
                                              " " +
                                              " " +
                                              " " +
                                              " Update Mas_Employee Set EmployeeId_Encrypted = (master.dbo.fn_varbintohexstr(HashBytes('SHA2_256 ', convert(nvarchar(70), (SELECT ID FROM @IdentityValue_EmployeeID)))))  where EmployeeId=(SELECT ID FROM @IdentityValue_EmployeeID) " +
                                              " " +
                                              " " +
                                              " " +
                                              "Insert Into Mas_Employee_Personal ( " +
                                              "  Deactivate " +
                                              " ,CreatedBy " +
                                              " ,CreatedDate " +
                                              " ,MachineName " +
                                              " ,PersonalEmployeeId " +
                                              " ,AadhaarNo " +
                                               " ,NameAsPerAadhaar " +
                                              " ,PrimaryMobile " +
                                              " ,BirthdayDate " +
                                              " ,EmployeeQualificationID " +
                                              " ,Gender " +
                                              " ,MaritalStatus " + ") OUTPUT Inserted.PersonalId INTO @IdentityValue_PersonalEmployeeID  values (" +
                                              "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + " '" + Dns.GetHostName().ToString() + "'," + "(SELECT ID FROM @IdentityValue_EmployeeID)," +
                                              "'" + Data.AadhaarNo + "'," +
                                              "'" + Data.NameAsPerAadhaar + "'," +
                                              "'" + Data.PrimaryMobile + "'," +
                                              "'" + Convert.ToDateTime(Data.BirthdayDate).ToString("yyyy-MM-dd") + "'," +
                                              "'" + List_Qualification_Id + "'," +
                                              "'" + Data.Gender + "'," +
                                              "'" + Data.MaritalStatus + "'" + ")" +
                                              " " +
                                              " " +
                                              " " +
                                              " Update Mas_Employee_Personal Set PersonalId_Encrypted = (master.dbo.fn_varbintohexstr(HashBytes('SHA2_256 ', convert(nvarchar(70), (SELECT ID FROM @IdentityValue_PersonalEmployeeID)))))  where PersonalId=(SELECT ID FROM @IdentityValue_PersonalEmployeeID)" +
                                              " " +
                                              " " +
                                              " " +
                                              "Insert Into Mas_Employee_Attendance ( " +
                                              "  Deactivate " +
                                              " ,CreatedBy " +
                                              " ,CreatedDate " +
                                              " ,MachineName " +
                                              " ,AttendanceEmployeeId " +
                                              " ,EM_Atten_WOFF1 " +
                                              " ,EM_Atten_ShiftGroupId " +
                                              " ,EM_Atten_ShiftRuleId " +
                                              " ,EM_Atten_OT_Applicable " +
                                              " ,EM_Atten_OTMultiplyBy " +
                                              " ,EM_Atten_PerDayShiftHrs " + ") OUTPUT Inserted.AttendanceId INTO @IdentityValue_AttendanceEmployeeID   values (" +
                                              "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," + "(SELECT ID FROM @IdentityValue_EmployeeID)," +
                                              "'" + Data.EM_Atten_WOFF1 + "'," +
                                              "'" + List_ShiftGroup_Id + "'," +
                                              "'" + List_ShiftRule_Id + "'," +
                                              "1," +
                                              "1," +
                                              "8.5" + ")" +
                                               " " +
                                              " " +
                                              " " +
                                              " Update Mas_employee_Attendance Set AttendanceId_Encrypted = (master.dbo.fn_varbintohexstr(HashBytes('SHA2_256 ', convert(nvarchar(70), (SELECT ID FROM @IdentityValue_AttendanceEmployeeID)))))  where AttendanceId=(SELECT ID FROM @IdentityValue_AttendanceEmployeeID)" +
                                               " " +
                                              " " +
                                              " " +
                                              "   Insert Into Mas_Employee_Address ( " +
                                              "   Deactivate " +
                                              " , CreatedBy " +
                                              " , CreatedDate " +
                                              " , MachineName " +
                                              " , AddressEmployeeId " +
                                              " , PresentPin " +
                                              " , PresentState " +
                                              " , PresentDistrict " +
                                              " , PresentTaluka " +
                                              " , PresentPO " +
                                              " , PresentCity " +
                                              " , PresentPostelAddress " + ") OUTPUT Inserted.AddressId INTO @IdentityValue_AddressEmployeeID   values (" +
                                              "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," + "(SELECT ID FROM @IdentityValue_EmployeeID)," +
                                              "'" + Data.PresentPin + "'," +
                                              "'" + Data.PresentState + "'," +
                                              "'" + Data.PresentDistrict + "'," +
                                              "'" + Data.PresentTaluka + "'," +
                                              "'" + Data.PresentPO + "'," +
                                              "'" + Data.PresentCity + "'," +
                                              "'" + Data.PresentPostelAddress + "'" + ")" +
                                               " " +
                                              " " +
                                              " " +
                                              " Update Mas_Employee_Address Set AddressId_Encrypted = (master.dbo.fn_varbintohexstr(HashBytes('SHA2_256 ', convert(nvarchar(70), (SELECT ID FROM @IdentityValue_AddressEmployeeID)))))  where AddressId=(SELECT ID FROM @IdentityValue_AddressEmployeeID)" +
                                             " " +
                                             " " +
                                              "   Insert Into Mas_Employee_Statutory ( " +
                                              "   Deactivate " +
                                              " , CreatedBy " +
                                              " , CreatedDate " +
                                              " , MachineName " +
                                              " , StatutoryEmployeeId " +
                                              " , ESIC_Applicable " +
                                              " , PT_Applicable " +
                                              " , LWF_Applicable " +
                                              " , PF_Applicable " +
                                              " , PF_FSType " +
                                              " , PF_FS_Name " +
                                              " , PF_NO " +
                                              " , PF_UAN " +
                                              " , ESIC_NO " +
                                              " , PF_Reletion1 " +
                                              " , PF_Nominee1 " + ") OUTPUT Inserted.StatutoryId INTO @IdentityValue_StatutoryEmployeeID   values (" +
                                              "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," + "(SELECT ID FROM @IdentityValue_EmployeeID)," +
                                              "'1'," +
                                              "'1'," +
                                              "'1'," +
                                              "'1'," +
                                              "'" + Data.PF_FSType + "'," +
                                              "'" + Data.PF_FS_Name + "'," +
                                              "'" + Data.PF_NO + "'," +
                                              "'" + Data.PF_UAN + "'," +
                                              "'" + Data.ESIC_NO + "'," +
                                              "'" + List_RelationWithNominee_Id + "'," +
                                              "'" + Data.PF_Nominee1 + "'" + ")" +
                                               " " +
                                              " " +
                                              " " +
                                              " Update Mas_Employee_Statutory Set StatutoryId_Encrypted = (master.dbo.fn_varbintohexstr(HashBytes('SHA2_256 ', convert(nvarchar(70), (SELECT ID FROM @IdentityValue_StatutoryEmployeeID)))))  where StatutoryId=(SELECT ID FROM @IdentityValue_StatutoryEmployeeID)" +
                                             " " +
                                             " ";


                    strBuilder.Append(Employee_Admin);
                    strBuilder.Append(" DELETE FROM @IdentityValue_EmployeeID     ;");
                    strBuilder.Append(" DELETE FROM @IdentityValue_PersonalEmployeeID     ;");
                    strBuilder.Append(" DELETE FROM @IdentityValue_AttendanceEmployeeID     ;");
                    strBuilder.Append(" DELETE FROM @IdentityValue_AddressEmployeeID     ;");
                    strBuilder.Append(" DELETE FROM @IdentityValue_StatutoryEmployeeID     ;");

                    i = i + 1;
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
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Icon = "error" }, JsonRequestBehavior.AllowGet);
                //Session["GetErrorMessage"] = ex.Message;
                //return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region Get ID

        void GetCompany_Id_Name(string Name, out int Company_Id)
        {
            try
            {
                var matchingRows = from row in dt_companyName.AsEnumerable()
                                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                                   select row;

                var varCompany_Id = matchingRows.LastOrDefault();

                //var varCompany_Id = (from row in dt_company.AsEnumerable()
                //                     where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                //                     select row).Last();
                if (varCompany_Id == null)
                {
                    Company_Id = 0;
                }
                else
                {
                    Company_Id = Convert.ToInt32(varCompany_Id["Id"]);
                }
            }
            catch
            {


                Company_Id = 0;
            }
        }

        void GetCompany_Id(string Name, out int Company_Id)
        {
            try
            {
                var matchingRows = from row in dt_company.AsEnumerable()
                                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                                   select row;

                var varCompany_Id = matchingRows.LastOrDefault();

                //var varCompany_Id = (from row in dt_company.AsEnumerable()
                //                     where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                //                     select row).Last();
                if (varCompany_Id == null)
                {
                    Company_Id = 0;
                }
                else
                {
                    Company_Id = Convert.ToInt32(varCompany_Id["Id"]);
                }
            }
            catch
            {


                Company_Id = 0;
            }
        }

        void GetBranch_Id_Name(string Name, out int Branch_Id)
        {

            try
            {

                //var matchingRows = from row in dt_Contractor.AsEnumerable()
                //                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                //                   select row;

                var matchingRows = from row in dt_BU.AsEnumerable()
                                   where string.Equals(row.Field<string>("BranchName"), Name, StringComparison.OrdinalIgnoreCase)
                                   select row;
                var varBranch_Id = matchingRows.LastOrDefault();
                if (varBranch_Id == null)
                {
                    Branch_Id = 0;
                }
                else
                {
                    Branch_Id = Convert.ToInt32(varBranch_Id["BranchId"]);
                }
            }
            catch (Exception ex)
            {
                Branch_Id = 0;
            }
        }

        void GetBranch_Id(string Name, int companyId, out int Branch_Id)
        {

            try
            {

                //var matchingRows = from row in dt_Contractor.AsEnumerable()
                //                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                //                   select row;

                var matchingRows = from row in dt_BU.AsEnumerable()
                                   where string.Equals(row.Field<string>("BranchName"), Name, StringComparison.OrdinalIgnoreCase)
                                         && row.Field<int>("CmpId") == Convert.ToInt16(companyId)
                                   select row;
                var varBranch_Id = matchingRows.LastOrDefault();
                if (varBranch_Id == null)
                {
                    Branch_Id = 0;
                }
                else
                {
                    Branch_Id = Convert.ToInt32(varBranch_Id["BranchId"]);
                }
            }
            catch (Exception ex)
            {
                Branch_Id = 0;
            }
        }

        void GetContractor_Id_Name(string Name, out int Contractor_Id)
        {
            try
            {
                var matchingRows = from row in dt_Contractor.AsEnumerable()
                                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                                   select row;

                var varContractor_Id = matchingRows.LastOrDefault();

                //var varContractor_Id = (from row in dt_Contractor.AsEnumerable()
                //                        where (row.Field<string>("Name") == Name)
                //                        select row).Last();
                if (varContractor_Id == null)
                {
                    Contractor_Id = 0;
                }
                else
                {
                    Contractor_Id = Convert.ToInt32(varContractor_Id["Id"]);
                }
            }
            catch
            {
                Contractor_Id = 0;
            }
        }

        void GetContractor_Id(string Name, out int Contractor_Id, int companyId, int List_BUId)
        {

            try
            {
                //var matchingRows = from row in dt_Contractor.AsEnumerable()
                //                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                //                   select row;

                var matchingRows = from row in dt_Contractor.AsEnumerable()
                                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                                         && row.Field<int>("CmpID") == Convert.ToInt16(companyId) && row.Field<int>("BranchID") == Convert.ToInt16(List_BUId)
                                   select row;
                var varContractor_Id = matchingRows.LastOrDefault();
                if (varContractor_Id == null)
                {
                    Contractor_Id = 0;
                }
                else
                {
                    Contractor_Id = Convert.ToInt32(varContractor_Id["Id"]);
                }
            }
            catch (Exception ex)
            {
                Contractor_Id = 0;
            }
        }

        void GetQualification_Id(string Name, out int Qualification_Id)
        {
            try
            {

                var matchingRows = from row in dt_Qualification.AsEnumerable()
                                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                                   select row;

                var varQualification_Id = matchingRows.LastOrDefault();
                //var varQualification_Id = (from row in dt_Qualification.AsEnumerable()
                //                           where (row.Field<string>("Name") == Name)
                //                           select row).Last();
                if (varQualification_Id == null)
                {
                    Qualification_Id = 0;
                }
                else
                {
                    Qualification_Id = Convert.ToInt32(varQualification_Id["Id"]);
                }
            }
            catch
            {
                Qualification_Id = 0;
            }
        }

        void GetDepartment_Id(string Name, out int Department_Id)
        {

            try
            {

                //var varQualification_Id = (from row in dt_Qualification.AsEnumerable()
                //                           where row.Field<string>("Name").Equals(Name, StringComparison.OrdinalIgnoreCase)
                //                           select row).LastOrDefault();
                var matchingRows = from row in dt_Department.AsEnumerable()
                                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                                   select row;

                var varDepartment_Id = matchingRows.LastOrDefault();
                //var varDepartment_Id = (from row in dt_Department.AsEnumerable()
                //                        where string.Equals(row.Field<string>("Name") == Name,StringComparison.OrdinalIgnoreCase)
                //                        select row).LastOrDefault();
                if (varDepartment_Id == null)
                {
                    Department_Id = 0;
                }
                else
                {
                    Department_Id = Convert.ToInt32(varDepartment_Id["Id"]);
                }
            }
            catch
            {
                Department_Id = 0;
            }
        }

        void GetSubDepatment_Id(string Name, int DepartmentId, out int SubDepatment_Id)
        {
            try
            {

                //var matchingRows = from row in dt_SubDepartment.AsEnumerable()
                //                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                //                   select row;

                string nameTrimmed = Name?.Trim();
                int deptId = Convert.ToInt32(DepartmentId);

                var matchingRows = from row in dt_SubDepartment.AsEnumerable()
                                   where string.Equals(row.Field<string>("SubDepartmentName")?.Trim(), nameTrimmed, StringComparison.OrdinalIgnoreCase)
                                      && row.Field<int>("DepartmentId") == deptId
                                   select row;

                var varSubDepatment_Id = matchingRows.LastOrDefault();

                //var matchingRows = from row in dt_SubDepartment.AsEnumerable()
                //                   where string.Equals(row.Field<string>("SubDepartmentName"), Name, StringComparison.OrdinalIgnoreCase) &&
                //                         row.Field<int>("DepartmentId") == Convert.ToInt16(DepartmentId)
                //                   select row;

                //var varSubDepatment_Id = matchingRows.LastOrDefault();
                if (varSubDepatment_Id == null)
                {
                    SubDepatment_Id = 0;
                }
                else
                {
                    SubDepatment_Id = Convert.ToInt32(varSubDepatment_Id["SubDepartmentId"]);
                }
            }
            catch
            {
                SubDepatment_Id = 0;
            }
        }

        void GetDesignation_Id(string Name, out int Designation_Id)
        {
            try
            {
                var matchingRows = from row in dt_Designation.AsEnumerable()
                                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                                   select row;

                var varDesignation_Id = matchingRows.LastOrDefault();

                //var varDesignation_Id = (from row in dt_Designation.AsEnumerable()
                //                         where string.Equals(row.Field<string>("Name") == Name,StringComparison.OrdinalIgnoreCase)
                //                         select row).Last();
                if (varDesignation_Id == null)
                {
                    Designation_Id = 0;
                }
                else
                {
                    Designation_Id = Convert.ToInt32(varDesignation_Id["Id"]);
                }
            }
            catch
            {
                Designation_Id = 0;
            }
        }

        //void GetDesignation_Id(string Name,int LManpowerCategory_Id, out int Designation_Id)
        //{
        //    try
        //    {
        //        var matchingRows = from row in dt_Designation.AsEnumerable()
        //                           where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
        //                                 && row.Field<int>("KPISubCategoryId") == Convert.ToInt16(LManpowerCategory_Id)
        //                           select row;

        //        //var matchingRows = from row in dt_Designation.AsEnumerable()
        //        //                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
        //        //                   select row;

        //        var varDesignation_Id = matchingRows.LastOrDefault();

        //        //var varDesignation_Id = (from row in dt_Designation.AsEnumerable()
        //        //                         where string.Equals(row.Field<string>("Name") == Name,StringComparison.OrdinalIgnoreCase)
        //        //                         select row).Last();
        //        if (varDesignation_Id == null)
        //        {
        //            Designation_Id = 0;
        //        }
        //        else
        //        {
        //            Designation_Id = Convert.ToInt32(varDesignation_Id["Id"]);
        //        }
        //    }
        //    catch
        //    {
        //        Designation_Id = 0;
        //    }
        //}

        void GetGrade_Id(string Name, out int Grade_Id)
        {
            try
            {

                var matchingRows = from row in dt_Grade.AsEnumerable()
                                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                                   select row;

                var varGrade_Id = matchingRows.LastOrDefault();
                //var varGrade_Id = (from row in dt_Grade.AsEnumerable()
                //                   where string.Equals(row.Field<string>("Name") == Name,StringComparison.OrdinalIgnoreCase)
                //                   select row).Last();
                if (varGrade_Id == null)
                {
                    Grade_Id = 0;
                }
                else
                {
                    Grade_Id = Convert.ToInt32(varGrade_Id["Id"]);
                }
            }
            catch
            {
                Grade_Id = 0;
            }
        }

        void GetManpowerCategory_Id(string Name, int companyId, int BUId, out int ManpowerCategory_Id)
        {
            try
            {

                //var matchingRows = from row in dt_SubDepartment.AsEnumerable()
                //                   where string.Equals(row.Field<string>("SubDepartmentName"), Name, StringComparison.OrdinalIgnoreCase) &&
                //                         row.Field<int>("DepartmentId") == Convert.ToInt16(DepartmentId)
                //                   select row;

                //var varSubDepatment_Id = matchingRows.LastOrDefault();

                var matchingRows = from row in dt_ManpowerCategory.AsEnumerable()
                                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase) 
                                   select row;
                var varManpowerCategory_Id = matchingRows.LastOrDefault();
                if (varManpowerCategory_Id == null)
                {
                    ManpowerCategory_Id = 0;
                }
                else
                {
                    ManpowerCategory_Id = Convert.ToInt32(varManpowerCategory_Id["Id"]);
                }
            }
            catch
            {
                ManpowerCategory_Id = 0;
            }
        }

        void GetLine_Id(string Name, int companyId, int BUId, out int Line_Id)
        {
            try
            {

                //var matchingRows = from row in dt_SubDepartment.AsEnumerable()
                //                   where string.Equals(row.Field<string>("SubDepartmentName"), Name, StringComparison.OrdinalIgnoreCase) &&
                //                         row.Field<int>("DepartmentId") == Convert.ToInt16(DepartmentId)
                //                   select row;

                //var varSubDepatment_Id = matchingRows.LastOrDefault();

                var matchingRows = from row in dt_Line.AsEnumerable()
                                   where string.Equals(row.Field<string>("LineName"), Name, StringComparison.OrdinalIgnoreCase) && row.Field<int>("CmpId") == Convert.ToInt16(companyId) && row.Field<int>("BranchId") == Convert.ToInt16(BUId)
                                   select row;
                var varLine_Id = matchingRows.LastOrDefault();
                if (varLine_Id == null)
                {
                    Line_Id = 0;
                }
                else
                {
                    Line_Id = Convert.ToInt32(varLine_Id["LineId"]);
                }
            }
            catch
            {
                Line_Id = 0;
            }
        }

        void GetUnit_Id(string Name, int companyId, int BUId, out int Unit_Id)
        {
            try
            {
                //var matchingRows = from row in dt_SubUnit.AsEnumerable()
                //                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                //                   select row;



                var matchingRows = from row in dt_SubUnit.AsEnumerable()
                                   where string.Equals(row.Field<string>("UnitName"), Name, StringComparison.OrdinalIgnoreCase) && row.Field<int>("CmpID") == Convert.ToInt16(companyId) && row.Field<int>("UnitBranchId") == Convert.ToInt16(BUId)
                                   select row;

                var varUnit_Id = matchingRows.LastOrDefault();
                if (varUnit_Id == null)
                {
                    Unit_Id = 0;
                }
                else
                {
                    Unit_Id = Convert.ToInt32(varUnit_Id["UnitId"]);
                }
            }
            catch
            {
                Unit_Id = 0;
            }
        }

        void GetShiftGroup_Id(string Name, int companyId, int BUId, out int ShiftGroup_Id)
        {
            try
            {


                var matchingRows = from row in dt_ShiftGroup.AsEnumerable()
                                   where string.Equals(row.Field<string>("ShiftGroupFName"), Name, StringComparison.OrdinalIgnoreCase) && row.Field<int>("CmpId") == Convert.ToInt16(companyId) && row.Field<int>("ShiftGroupBranchId") == Convert.ToInt16(BUId)
                                   select row;
                var varShiftGroup_Id = matchingRows.LastOrDefault();
                if (varShiftGroup_Id == null)
                {
                    ShiftGroup_Id = 0;
                }
                else
                {
                    ShiftGroup_Id = Convert.ToInt32(varShiftGroup_Id["ShiftGroupId"]);
                }
            }
            catch
            {
                ShiftGroup_Id = 0;
            }
        }

        void GetShiftRule_Id(string Name, int companyId, int BUId, out int ShiftRule_Id)
        {
            try
            {
                var matchingRows = from row in dt_ShiftRule.AsEnumerable()
                                   where string.Equals(row.Field<string>("ShiftRuleName"), Name, StringComparison.OrdinalIgnoreCase) && row.Field<int>("CmpId") == Convert.ToInt16(companyId) && row.Field<int>("ShiftRuleBranchId") == Convert.ToInt16(BUId)
                                   select row;
                var varShiftRule_Id = matchingRows.LastOrDefault();
                if (varShiftRule_Id == null)
                {
                    ShiftRule_Id = 0;
                }
                else
                {
                    ShiftRule_Id = Convert.ToInt32(varShiftRule_Id["ShiftRuleId"]);
                }
            }
            catch
            {
                ShiftRule_Id = 0;
            }
        }

        void GetAssessmentLevel_Id(string Name, out int AssessmentLevel_Id)
        {

            try
            {

                var matchingRows = from row in dt_AssessmentLevel.AsEnumerable()
                                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                                   select row;
                var varAssessmentLevel_Id = matchingRows.LastOrDefault();
                if (varAssessmentLevel_Id == null)
                {
                    AssessmentLevel_Id = 0;
                }
                else
                {
                    AssessmentLevel_Id = Convert.ToInt32(varAssessmentLevel_Id["Id"]);
                }
            }
            catch
            {
                AssessmentLevel_Id = 0;
            }
        }

        void GetCriticalStage_Id(string Name, out int CriticalStage_Id)
        {

            try
            {
                var matchingRows = from row in dt_CriticalStage.AsEnumerable()
                                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                                   select row;
                var varCriticalStage_Id = matchingRows.LastOrDefault();
                if (varCriticalStage_Id == null)
                {
                    CriticalStage_Id = 0;
                }
                else
                {
                    CriticalStage_Id = Convert.ToInt32(varCriticalStage_Id["Id"]);
                }
            }
            catch
            {
                CriticalStage_Id = 0;
            }
        }

        void GetReationNonimee_Id(string Name, out int RelationWithNominee_Id)
        {

            try
            {
                var matchingRows = from row in dt_RelationWithNominee.AsEnumerable()
                                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                                   select row;
                var varRelationWithNominee_Id = matchingRows.LastOrDefault();
                if (varRelationWithNominee_Id == null)
                {
                    RelationWithNominee_Id = 0;
                }
                else
                {
                    RelationWithNominee_Id = Convert.ToInt32(varRelationWithNominee_Id["Id"]);
                }
            }
            catch
            {
                RelationWithNominee_Id = 0;
            }
        }

        #endregion

        #region MyRegion
        public ActionResult GetEmployeeHistory(string AadharNo)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramHIS = new DynamicParameters();
                paramHIS.Add("@query", "Select CompanyName as Company,BranchName as BU, EmployeeName as Employee,EmployeeNo,EmployeeCardNo,Contractor_Master.ContractorName,DepartmentName,Mas_Designation.DesignationName,Mas_Grade.GradeName,Replace(Convert(nvarchar(12),Mas_Employee.JoiningDate,106),' ','/') as JoiningDate, Mas_Employee_Personal.AadhaarNo as  AadhaarNo ,CASE WHEN dbo.Mas_Employee.EmployeeLeft = 0 THEN 'Active' ELSE 'Left' END AS Status,replace(convert(nvarchar(12),Mas_Employee.LeavingDate,106),' ','/') as LeavingDate,CASE WHEN EmployeeLeft = 1 THEN CONVERT(varchar(3),  DATEDIFF(MONTH, JoiningDate, LeavingDate) / 12) + ' years ' + CONVERT(varchar(2), DATEDIFF(MONTH, JoiningDate, LeavingDate) % 12) + ' months' ELSE '-' END AS Duration from Mas_Employee,Mas_CompanyProfile,Mas_Branch,Mas_Department,Mas_Employee_Personal,Contractor_Master,Mas_Designation,Mas_Grade where Mas_Employee.Employeeleft=0 and   Mas_Employee.Deactivate=0  and Contractor_Master.Deactivate=0  and Mas_Designation.Deactivate=0 and  Mas_Employee_Personal.Deactivate=0  and Mas_CompanyProfile.Deactivate=0  and Mas_Grade.Deactivate=0 and Mas_Branch.Deactivate=0 and  Mas_Department.Deactivate=0 and Mas_Employee.CmpID=Mas_CompanyProfile.CompanyId and Mas_Employee.EmployeeBranchId=BranchId and Mas_Employee.EmployeeDepartmentID=DepartmentId and Mas_Employee.employeeId=Mas_Employee_Personal.PersonalEmployeeId and Contractor_Master.ContractorId=Mas_Employee.ContractorID and Mas_Designation.DesignationId=Mas_Employee.EmployeeDesignationID and Mas_Grade.GradeId=Mas_Employee.EmployeeGradeID and Mas_Employee_Personal.AadhaarNo='" + AadharNo + "'");
                var ListEmployeeAadharNo = DapperORM.ReturnList<DuplicateAddharNoDataTable>("sp_QueryExcution", paramHIS).ToList();
                return Json(ListEmployeeAadharNo, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }


        }
        #endregion

        #region GetAadhaarDuplicate
        [HttpPost]
        public ActionResult GetAadhaarDuplicate(List<_BulkInsertDuplicateAddharNo> tblAadhaardata)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                string str_aadhar = "";
                foreach (var Data in tblAadhaardata)
                {
                    str_aadhar = str_aadhar + "'" + Data.AadhaarNo + "',";
                }

                if (str_aadhar != "")
                {
                    str_aadhar = str_aadhar.Substring(0, str_aadhar.Length - 1);
                }
                DynamicParameters paramBulk = new DynamicParameters();
                paramBulk.Add("@query", "Select CompanyName as Company,BranchName as BU, EmployeeName as Employee,EmployeeNo,EmployeeCardNo,Contractor_Master.ContractorName,DepartmentName,Mas_Designation.DesignationName,Mas_Grade.GradeName,Replace(Convert(nvarchar(12),Mas_Employee.JoiningDate,106),' ','/') as JoiningDate, Mas_Employee_Personal.AadhaarNo as  AadhaarNo ,CASE WHEN dbo.Mas_Employee.EmployeeLeft = 0 THEN 'Active' ELSE 'Left' END AS Status,replace(convert(nvarchar(12),Mas_Employee.LeavingDate,106),' ','/') as LeavingDate,CASE WHEN EmployeeLeft = 1 THEN CONVERT(varchar(3),  DATEDIFF(MONTH, JoiningDate, LeavingDate) / 12) + ' years ' + CONVERT(varchar(2), DATEDIFF(MONTH, JoiningDate, LeavingDate) % 12) + ' months' ELSE '-' END AS Duration from Mas_Employee,Mas_CompanyProfile,Mas_Branch,Mas_Department,Mas_Employee_Personal,Contractor_Master,Mas_Designation,Mas_Grade where Mas_Employee.Employeeleft=0 and   Mas_Employee.Deactivate=0  and Contractor_Master.Deactivate=0  and Mas_Designation.Deactivate=0 and  Mas_Employee_Personal.Deactivate=0  and Mas_CompanyProfile.Deactivate=0  and Mas_Grade.Deactivate=0 and Mas_Branch.Deactivate=0 and  Mas_Department.Deactivate=0 and Mas_Employee.CmpID=Mas_CompanyProfile.CompanyId and Mas_Employee.EmployeeBranchId=BranchId and Mas_Employee.EmployeeDepartmentID=DepartmentId and Mas_Employee.employeeId=Mas_Employee_Personal.PersonalEmployeeId and Contractor_Master.ContractorId=Mas_Employee.ContractorID and Mas_Designation.DesignationId=Mas_Employee.EmployeeDesignationID and Mas_Grade.GradeId=Mas_Employee.EmployeeGradeID and Mas_Employee_Personal.AadhaarNo in  (" + str_aadhar + ")");
                var ListEmployeeAadharNo = DapperORM.ReturnList<DuplicateAddharNoDataTable>("sp_QueryExcution", paramBulk).ToList();

                //var ListEmployeeAadharNo = DapperORM.DynamicQuerySingle("Select CompanyName as Company,BranchName as BU, EmployeeName as Employee,EmployeeNo,EmployeeCardNo,Contractor_Master.ContractorName,DepartmentName,Mas_Designation.DesignationName,Mas_Grade.GradeName,Replace(Convert(nvarchar(12),Mas_Employee.JoiningDate,106),' ','/') as JoiningDate, Mas_Employee_Personal.AadhaarNo as  AadhaarNo from Mas_Employee,Mas_CompanyProfile,Mas_Branch,Mas_Department,Mas_Employee_Personal,Contractor_Master,Mas_Designation,Mas_Grade where Mas_Employee.Employeeleft=0 and   Mas_Employee.Deactivate=0  and Contractor_Master.Deactivate=0  and Mas_Designation.Deactivate=0 and  Mas_Employee_Personal.Deactivate=0  and Mas_CompanyProfile.Deactivate=0  and Mas_Grade.Deactivate=0 and Mas_Branch.Deactivate=0 and  Mas_Department.Deactivate=0 and Mas_Employee.CmpID=Mas_CompanyProfile.CompanyId and Mas_Employee.EmployeeBranchId=BranchId and Mas_Employee.EmployeeDepartmentID=DepartmentId and Mas_Employee.employeeId=Mas_Employee_Personal.PersonalEmployeeId and Contractor_Master.ContractorId=Mas_Employee.ContractorID and Mas_Designation.DesignationId=Mas_Employee.EmployeeDesignationID and Mas_Grade.GradeId=Mas_Employee.EmployeeGradeID and Mas_Employee_Personal.AadhaarNo in  (" + str_aadhar + ")").ToList();
                if (ListEmployeeAadharNo.Count != 0)
                {
                    //ViewBag.GetListEmployeeAadharNo = ListEmployeeAadharNo;
                    return Json(ListEmployeeAadharNo, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var Message = "Record Not Found";
                    var Icon = "error";
                    return Json(new { Message = Message, Icon = Icon }, JsonRequestBehavior.AllowGet);

                }

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetDesignationDropDown
        [HttpGet]
        public ActionResult GetDesignationDropDown(int? AllocationCategoryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramCategory = new DynamicParameters();
                paramCategory.Add("@query", "Select DesignationId as Id,DesignationName as Name from Mas_Designation,KPI_CategoryDepartment,KPI_SubCategory where Mas_Designation.Deactivate=0 and KPI_SubCategory.Deactivate=0 and KPI_SubCategory.KPISubCategoryId=KPI_CategoryDepartment.KPI_Category_CategoryId and Mas_Designation.DesignationId=KPI_CategoryDepartment.KPI_Category_DesignationId and KPI_CategoryDepartment.KPI_Category_CategoryId=" + AllocationCategoryId + "");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramCategory).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
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