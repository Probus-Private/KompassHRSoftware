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
    public class Module_Employee_BulkUpdateController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: Module/Module_Employee_BulkUpdate

        #region BulkUpdate
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
        DataTable dt_ValidationCheck = new DataTable();
        DataTable dt_ManpowerCategory = new DataTable();

        public ActionResult Module_Employee_BulkUpdate()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 325;
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

                DynamicParameters paramType = new DynamicParameters();
                paramType.Add("@query", "Select Mas_BulkUpdatetype.TypeName as Name, Mas_BulkUpdatetype.Description as Id from Mas_BulkUpdatetype inner join Mas_BulkUpdatetypeMapping on Mas_BulkUpdatetypeMapping.TypeId = Mas_BulkUpdatetype.BulkUpdateTypeId where Mas_BulkUpdatetypeMapping.IsActive = 1 and Mas_BulkUpdatetypeMapping.EmployeeId = " + Session["EmployeeId"] + "  and Mas_BulkUpdatetypeMapping.Deactivate = 0 ");
                var TypeName = DapperORM.ReturnList<PayrollDropDownBind>("sp_QueryExcution", paramType).ToList();
                ViewBag.GetTypeName = TypeName;



                ViewBag.GetBranchName = "";
                ViewBag.GetAadhardata = null;
                ViewBag.GetStatutorydata = null;
                ViewBag.GetAddressdata = null;
                ViewBag.GetCriticalStagedata = null;
                ViewBag.GetDepartmentdata = null;
                ViewBag.GetAttendancedata = null;
                ViewBag.GetManpowerCategory = null;
                ViewBag.GetLineMaster = null;
                ViewBag.GetAssessmentLevel = null;
                ViewBag.GradeNameData = null;
                ViewBag.DesignationNameData = null;
                ViewBag.GetGenderData = null;
                ViewBag.DateOfBirthData = null;
                ViewBag.GetContractorData = null;
                ViewBag.GetMaritalStatusData = null;
                ViewBag.GetMobileNoData = null;
                ViewBag.GetSubUnitData = null;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "Module_Employee_BulkInsert");
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

        #region ImportExcelFile
        [HttpPost]
        public ActionResult Module_Employee_BulkUpdate(HttpPostedFileBase AttachFile, Module_Employee_BulkUpdate GetBulkUpdate)
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
                var GetCmpId = GetComapnyName[0].Id;

                DynamicParameters paramBranch = new DynamicParameters();
                paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                paramBranch.Add("@p_CmpId", GetBulkUpdate.CompanyName);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                ViewBag.GetBranchName = data;

                DynamicParameters paramType = new DynamicParameters();
                paramType.Add("@query", "Select Mas_BulkUpdatetype.TypeName as Name, Mas_BulkUpdatetype.Description as Id from Mas_BulkUpdatetype inner join Mas_BulkUpdatetypeMapping on Mas_BulkUpdatetypeMapping.TypeId = Mas_BulkUpdatetype.BulkUpdateTypeId where Mas_BulkUpdatetypeMapping.IsActive = 1 and Mas_BulkUpdatetypeMapping.EmployeeId = " + Session["EmployeeId"] + "  and Mas_BulkUpdatetypeMapping.Deactivate = 0 ");
                var TypeName = DapperORM.ReturnList<PayrollDropDownBind>("sp_QueryExcution", paramType).ToList();
                ViewBag.GetTypeName = TypeName;

                ViewBag.GetAadhardata = null;
                ViewBag.GetStatutorydata = null;
                ViewBag.GetAddressdata = null;
                ViewBag.GetCriticalStagedata = null;
                ViewBag.GetDepartmentdata = null;
                ViewBag.GetAttendancedata = null;
                ViewBag.GetManpowerCategory = null;
                ViewBag.GetLineMaster = null;
                ViewBag.GetAssessmentLevel = null;
                ViewBag.GradeNameData = null;
                ViewBag.DesignationNameData = null;
                ViewBag.GetGenderData = null;
                ViewBag.DateOfBirthData = null;
                ViewBag.GetContractorData = null;
                ViewBag.GetMaritalStatusData = null;
                ViewBag.GetMobileNoData = null;
                ViewBag.GetSubUnitData = null;

                List<Module_Employee_BulkUpdate> excelDataList = new List<Module_Employee_BulkUpdate>();



                if (AttachFile.ContentLength > 0)
                {

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
                            if (xlWorkwook.Worksheets.Worksheet(1).Cell(row1, 1).GetString() == "")
                            {
                                TempData["Message"] = "Fill in missing information in the first column.";
                                TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                TempData["Icon"] = "error";
                                return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                            }

                            // var excelDataList = new List<Module_Employee_BulkUpdate>();
                            var employeeNumbers = new HashSet<string>(); // To track unique Employee numbers
                            var duplicateEmployeeNumbers = new List<string>(); // To store duplicates

                            while (xlWorkwook.Worksheets.Worksheet(1).Cell(row1, 1).GetString() != "")
                            {
                                string employeeNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row1, 2).GetString();

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
                                return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                            }

                            if (GetBulkUpdate.Type == "Update Aadhar Details")
                            {
                                var worksheet = xlWorkwook.Worksheets.Worksheet(1);
                                var Type = worksheet.Cell(1, 1).GetString();

                                if (Type == "Update Aadhar Details")
                                {
                                    int row = 3;
                                    if (worksheet.Cell(row, 1).GetString() == "")
                                    {
                                        TempData["Message"] = "Fill in missing information in the first column.";
                                        TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                        TempData["Icon"] = "error";
                                        return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                    }

                                    //var excelDataList = new List<Module_Employee_BulkUpdate>();
                                    var aadhaarNumbers = new HashSet<string>(); // To track unique Aadhaar numbers
                                    var duplicateAadhaarNumbers = new List<string>(); // To store duplicates

                                    while (!string.IsNullOrWhiteSpace(worksheet.Cell(row, 1).GetString()))
                                    {
                                        var aadhaarNo = worksheet.Cell(row, 4).GetString().Trim();
                                        bool hasError = false;
                                        List<string> missingFields = new List<string>();

                                        // Validate fields and collect missing fields
                                        string srNo = worksheet.Cell(row, 1).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(srNo)) { hasError = true; missingFields.Add("SrNo"); }

                                        string employeeNo = worksheet.Cell(row, 2).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeNo)) { hasError = true; missingFields.Add("EmployeeNo"); }

                                        string employeeName = worksheet.Cell(row, 3).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeName)) { hasError = true; missingFields.Add("EmployeeName"); }

                                        if (string.IsNullOrWhiteSpace(aadhaarNo)) { hasError = true; missingFields.Add("AadhaarNo"); }

                                        string nameAsPerAadhaar = worksheet.Cell(row, 5).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(nameAsPerAadhaar)) { hasError = true; missingFields.Add("NameAsPerAadhaar"); }

                                        if (!hasError)
                                        {
                                            if (!aadhaarNumbers.Add(aadhaarNo)) // Aadhaar number already exists
                                            {
                                                duplicateAadhaarNumbers.Add(aadhaarNo);
                                            }

                                            // Create a bulk update object with trimmed values
                                            Module_Employee_BulkUpdate bulkUpdate = new Module_Employee_BulkUpdate
                                            {
                                                SrNo = srNo,
                                                EmployeeNo = employeeNo,
                                                EmployeeName = employeeName,
                                                AadhaarNo = aadhaarNo,
                                                NameAsPerAadhaar = nameAsPerAadhaar
                                            };

                                            excelDataList.Add(bulkUpdate);
                                        }
                                        else
                                        {
                                            // Show message for missing fields
                                            //Console.WriteLine($"Row {row} has missing values: {string.Join(", ", missingFields)}");
                                            TempData["Message"] = $"Row {row} has missing values: {string.Join(", ", missingFields)}";
                                            TempData["Icon"] = "error";
                                            return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                        }

                                        row++;
                                    }


                                    // Check for duplicates and handle them
                                    if (duplicateAadhaarNumbers.Any())
                                    {
                                        TempData["Message"] = $"Duplicate Aadhaar Numbers found: {string.Join(", ", duplicateAadhaarNumbers.Distinct())}";
                                        TempData["Title"] = "Duplicate Aadhaar Numbers Detected";
                                        TempData["Icon"] = "error";
                                        return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                    }

                                    // No duplicates found, proceed
                                    ViewBag.GetAadhardata = excelDataList;
                                    ViewBag.GetStatutorydata = null;
                                    ViewBag.GetAddressdata = null;
                                    ViewBag.GetCriticalStagedata = null;
                                    ViewBag.GetDepartmentdata = null;
                                    ViewBag.GetAttendancedata = null;
                                    ViewBag.GetManpowerCategory = null;
                                    ViewBag.GetLineMaster = null;
                                    ViewBag.GetAssessmentLevel = null;
                                    ViewBag.GradeNameData = null;
                                    ViewBag.DesignationNameData = null;
                                    ViewBag.GetGenderData = null;
                                    ViewBag.DateOfBirthData = null;
                                    ViewBag.GetContractorData = null;
                                    ViewBag.GetMaritalStatusData = null;
                                    ViewBag.GetMobileNoData = null;
                                    ViewBag.GetSubUnitData = null;
                                    return View();
                                }
                                else
                                {
                                    TempData["Message"] = "Import a file based on the selected type.";
                                    TempData["Icon"] = "error";
                                    return View();
                                }
                            }


                            if (GetBulkUpdate.Type == "Update Statutory Details")
                            {
                                var worksheet = xlWorkwook.Worksheets.Worksheet(1);
                                var Type = worksheet.Cell(1, 1).GetString();

                                if (Type == "Update Statutory Details")
                                {
                                    int row = 3;
                                    if (worksheet.Cell(row, 1).GetString() == "")
                                    {
                                        TempData["Message"] = "Fill in missing information in the first column.";
                                        TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                        TempData["Icon"] = "error";
                                        return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                    }

                                    // var excelDataList = new List<Module_Employee_BulkUpdate>();
                                    var uanNumbers = new HashSet<string>(); // To track unique PF_UAN numbers
                                    var duplicateUanNumbers = new List<string>(); // To store duplicates

                                    while (!string.IsNullOrWhiteSpace(worksheet.Cell(row, 1).GetString()))
                                    {
                                        var uanNo = worksheet.Cell(row, 4).GetString().Trim();
                                        bool hasError = false;
                                        List<string> missingFields = new List<string>();

                                        // Validate fields and collect missing fields
                                        string srNo = worksheet.Cell(row, 1).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(srNo)) { hasError = true; missingFields.Add("SrNo"); }

                                        string employeeNo = worksheet.Cell(row, 2).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeNo)) { hasError = true; missingFields.Add("EmployeeNo"); }

                                        string employeeName = worksheet.Cell(row, 3).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeName)) { hasError = true; missingFields.Add("EmployeeName"); }

                                        if (string.IsNullOrWhiteSpace(uanNo)) { hasError = true; missingFields.Add("PF_UAN"); }

                                        string esicNo = worksheet.Cell(row, 5).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(esicNo)) { hasError = true; missingFields.Add("ESIC_NO"); }

                                        string pfFsType = worksheet.Cell(row, 6).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(pfFsType)) { hasError = true; missingFields.Add("PF_FSType"); }

                                        string pfFsName = worksheet.Cell(row, 7).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(pfFsName)) { hasError = true; missingFields.Add("PF_FS_Name"); }

                                        if (!hasError)
                                        {
                                            if (!uanNumbers.Add(uanNo)) // Check for duplicate UAN
                                            {
                                                duplicateUanNumbers.Add(uanNo);
                                               // Console.WriteLine($"Row {row}: Duplicate PF_UAN detected ({uanNo}).");
                                            }

                                            // Create a bulk update object with trimmed values
                                            Module_Employee_BulkUpdate bulkUpdate = new Module_Employee_BulkUpdate
                                            {
                                                SrNo = srNo,
                                                EmployeeNo = employeeNo,
                                                EmployeeName = employeeName,
                                                PF_UAN = uanNo,
                                                ESIC_NO = esicNo,
                                                PF_FSType = pfFsType,
                                                PF_FS_Name = pfFsName
                                            };

                                            excelDataList.Add(bulkUpdate);
                                        }
                                        else
                                        {
                                            // Show message for missing fields
                                            //Console.WriteLine($"Row {row} has missing values: {string.Join(", ", missingFields)}");
                                            TempData["Message"] = $"Row {row} has missing values: {string.Join(", ", missingFields)}";
                                            TempData["Icon"] = "error";
                                            return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                        }

                                        row++;
                                    }


                                    // Check for duplicates and handle them
                                    if (duplicateUanNumbers.Any())
                                    {
                                        TempData["Message"] = $"Duplicate PF_UAN Numbers found: {string.Join(", ", duplicateUanNumbers.Distinct())}";
                                        TempData["Title"] = "Duplicate PF_UAN Numbers Detected";
                                        TempData["Icon"] = "error";
                                        return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                    }

                                    // No duplicates found, proceed
                                    ViewBag.GetStatutorydata = excelDataList;
                                    ViewBag.GetAadhardata = null;
                                    ViewBag.GetAddressdata = null;
                                    ViewBag.GetCriticalStagedata = null;
                                    ViewBag.GetDepartmentdata = null;
                                    ViewBag.GetAttendancedata = null;
                                    ViewBag.GetManpowerCategory = null;
                                    ViewBag.GetLineMaster = null;
                                    ViewBag.GetAssessmentLevel = null;
                                    ViewBag.GradeNameData = null;
                                    ViewBag.DesignationNameData = null;
                                    ViewBag.GetGenderData = null;
                                    ViewBag.DateOfBirthData = null;
                                    ViewBag.GetContractorData = null;
                                    ViewBag.GetMaritalStatusData = null;
                                    ViewBag.GetMobileNoData = null;
                                    ViewBag.GetSubUnitData = null;
                                    return View();
                                }
                                else
                                {
                                    TempData["Message"] = "Import a file based on the selected type.";
                                    TempData["Icon"] = "error";
                                    return View();
                                }
                            }

                            if (GetBulkUpdate.Type == "Update Address Details")
                            {
                                var worksheet = xlWorkwook.Worksheets.Worksheet(1);
                                string type = worksheet.Cell(1, 1).GetString().Trim();

                                if (type == "Update Address Details")
                                {
                                    int row = 3;
                                    var employeeNumbers1 = new HashSet<string>(); // HashSet to track duplicate EmployeeNos

                                    if (string.IsNullOrWhiteSpace(worksheet.Cell(row, 1).GetString()))
                                    {
                                        TempData["Message"] = "Fill in missing information in the first column.";
                                        TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                        TempData["Icon"] = "error";
                                        return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                    }

                                    while (!string.IsNullOrWhiteSpace(worksheet.Cell(row, 1).GetString()))
                                    {
                                        // Read and trim values
                                        string srNo = worksheet.Cell(row, 1).GetString().Trim();
                                        string employeeNo = worksheet.Cell(row, 2).GetString().Trim();
                                        string employeeName = worksheet.Cell(row, 3).GetString().Trim();
                                        string presentPin = worksheet.Cell(row, 4).GetString().Trim();
                                        string presentState = worksheet.Cell(row, 5).GetString().Trim();
                                        string presentDistrict = worksheet.Cell(row, 6).GetString().Trim();
                                        string presentTaluka = worksheet.Cell(row, 7).GetString().Trim();
                                        string presentPO = worksheet.Cell(row, 8).GetString().Trim();
                                        string presentCity = worksheet.Cell(row, 9).GetString().Trim();
                                        string presentPostelAddress = worksheet.Cell(row, 10).GetString().Trim();

                                        bool hasError = false;
                                        List<string> missingFields = new List<string>();

                                        // Validate required fields
                                        if (string.IsNullOrWhiteSpace(srNo)) { hasError = true; missingFields.Add("SrNo"); }
                                        if (string.IsNullOrWhiteSpace(employeeNo)) { hasError = true; missingFields.Add("EmployeeNo"); }
                                        if (string.IsNullOrWhiteSpace(employeeName)) { hasError = true; missingFields.Add("EmployeeName"); }
                                        if (string.IsNullOrWhiteSpace(presentPin)) { hasError = true; missingFields.Add("PresentPin"); }
                                        if (string.IsNullOrWhiteSpace(presentState)) { hasError = true; missingFields.Add("PresentState"); }
                                        if (string.IsNullOrWhiteSpace(presentDistrict)) { hasError = true; missingFields.Add("PresentDistrict"); }
                                        if (string.IsNullOrWhiteSpace(presentTaluka)) { hasError = true; missingFields.Add("PresentTaluka"); }
                                        if (string.IsNullOrWhiteSpace(presentPO)) { hasError = true; missingFields.Add("PresentPO"); }
                                        if (string.IsNullOrWhiteSpace(presentCity)) { hasError = true; missingFields.Add("PresentCity"); }
                                        if (string.IsNullOrWhiteSpace(presentPostelAddress)) { hasError = true; missingFields.Add("PresentPostelAddress"); }

                                        if (!hasError)
                                        {
                                            // Check for duplicate EmployeeNo
                                            if (!employeeNumbers1.Add(employeeNo))
                                            {
                                                TempData["Message"] = $"Duplicate EmployeeNo detected at row {row} ({employeeNo}).";
                                                TempData["Icon"] = "error";
                                                return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                            }
                                            else
                                            {
                                                // Create a bulk update object
                                                Module_Employee_BulkUpdate bulkUpdate = new Module_Employee_BulkUpdate
                                                {
                                                    SrNo = srNo,
                                                    EmployeeNo = employeeNo,
                                                    EmployeeName = employeeName,
                                                    PresentPin = presentPin,
                                                    PresentState = presentState,
                                                    PresentDistrict = presentDistrict,
                                                    PresentTaluka = presentTaluka,
                                                    PresentPO = presentPO,
                                                    PresentCity = presentCity,
                                                    PresentPostelAddress = presentPostelAddress
                                                };

                                                excelDataList.Add(bulkUpdate);
                                            }
                                        }
                                        else
                                        {
                                            TempData["Message"] = $"Row {row} has missing values: {string.Join(", ", missingFields)}.";
                                            TempData["Icon"] = "error";
                                            return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                        }

                                        row++;
                                    }

                                    // Pass processed data to ViewBag
                                    ViewBag.GetStatutorydata = null;
                                    ViewBag.GetAadhardata = null;
                                    ViewBag.GetAddressdata = excelDataList;
                                    ViewBag.GetCriticalStagedata = null;
                                    ViewBag.GetDepartmentdata = null;
                                    ViewBag.GetAttendancedata = null;
                                    ViewBag.GetManpowerCategory = null;
                                    ViewBag.GetLineMaster = null;
                                    ViewBag.GetAssessmentLevel = null;
                                    ViewBag.GradeNameData = null;
                                    ViewBag.DesignationNameData = null;
                                    ViewBag.GetGenderData = null;
                                    ViewBag.DateOfBirthData = null;
                                    ViewBag.GetContractorData = null;
                                    ViewBag.GetMaritalStatusData = null;
                                    ViewBag.GetMobileNoData = null;
                                    ViewBag.GetSubUnitData = null;

                                    return View();
                                }
                                else
                                {
                                    TempData["Message"] = "Import a file based on the selected type.";
                                    TempData["Icon"] = "error";
                                    return View();
                                }
                            }

                            if (GetBulkUpdate.Type == "Update Critical Stage Category")
                            {
                                var worksheet = xlWorkwook.Worksheets.Worksheet(1);
                                string type = worksheet.Cell(1, 1).GetString().Trim();

                                if (type == "Update Critical Stage Category")
                                {
                                    int row = 3;
                                    var employeeNumbersCritical = new HashSet<string>(); // HashSet to track duplicate EmployeeNos

                                    if (string.IsNullOrWhiteSpace(worksheet.Cell(row, 1).GetString()))
                                    {
                                        TempData["Message"] = "Fill in missing information in the first column.";
                                        TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                        TempData["Icon"] = "error";
                                        return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                    }

                                    while (!string.IsNullOrWhiteSpace(worksheet.Cell(row, 1).GetString()))
                                    {
                                        // Read and trim values
                                        string srNo = worksheet.Cell(row, 1).GetString().Trim();
                                        string employeeNo = worksheet.Cell(row, 2).GetString().Trim();
                                        string employeeName = worksheet.Cell(row, 3).GetString().Trim();
                                        string isCriticalStageApplicable = worksheet.Cell(row, 4).GetString().Trim();
                                        string employeeCriticalStageID = worksheet.Cell(row, 5).GetString().Trim();

                                        bool hasError = false;
                                        List<string> missingFields = new List<string>();

                                        // Validate required fields
                                        if (string.IsNullOrWhiteSpace(srNo)) { hasError = true; missingFields.Add("SrNo"); }
                                        if (string.IsNullOrWhiteSpace(employeeNo)) { hasError = true; missingFields.Add("EmployeeNo"); }
                                        if (string.IsNullOrWhiteSpace(employeeName)) { hasError = true; missingFields.Add("EmployeeName"); }
                                        if (string.IsNullOrWhiteSpace(isCriticalStageApplicable)) { hasError = true; missingFields.Add("IsCriticalStageApplicable"); }
                                        if (string.IsNullOrWhiteSpace(employeeCriticalStageID)) { hasError = true; missingFields.Add("EmployeeCriticalStageID"); }

                                        if (!hasError)
                                        {
                                            // Check for duplicate EmployeeNo
                                            if (!employeeNumbersCritical.Add(employeeNo))
                                            {
                                                TempData["Message"] = $"Duplicate EmployeeNo detected at row {row} ({employeeNo}).";
                                                TempData["Icon"] = "error";
                                                return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                            }
                                            else
                                            {
                                                // Create a bulk update object
                                                Module_Employee_BulkUpdate bulkUpdate = new Module_Employee_BulkUpdate
                                                {
                                                    SrNo = srNo,
                                                    EmployeeNo = employeeNo,
                                                    EmployeeName = employeeName,
                                                    IsCriticalStageApplicable = isCriticalStageApplicable,
                                                    EmployeeCriticalStageID = employeeCriticalStageID
                                                };

                                                excelDataList.Add(bulkUpdate);
                                            }
                                        }
                                        else
                                        {
                                            TempData["Message"] = $"Row {row} has missing values: {string.Join(", ", missingFields)}.";
                                            TempData["Icon"] = "error";
                                            return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                        }

                                        row++;
                                    }

                                    // Pass processed data to ViewBag
                                    ViewBag.GetStatutorydata = null;
                                    ViewBag.GetAadhardata = null;
                                    ViewBag.GetAddressdata = null;
                                    ViewBag.GetCriticalStagedata = excelDataList;
                                    ViewBag.GetDepartmentdata = null;
                                    ViewBag.GetAttendancedata = null;
                                    ViewBag.GetManpowerCategory = null;
                                    ViewBag.GetLineMaster = null;
                                    ViewBag.GetAssessmentLevel = null;
                                    ViewBag.GradeNameData = null;
                                    ViewBag.DesignationNameData = null;
                                    ViewBag.GetGenderData = null;
                                    ViewBag.DateOfBirthData = null;
                                    ViewBag.GetContractorData = null;
                                    ViewBag.GetMaritalStatusData = null;
                                    ViewBag.GetMobileNoData = null;
                                    ViewBag.GetSubUnitData = null;

                                    return View();
                                }
                                else
                                {
                                    TempData["Message"] = "Import a file based on the selected type.";
                                    TempData["Icon"] = "error";
                                    return View();
                                }
                            }


                            if (GetBulkUpdate.Type == "Update Department")
                            {
                                var worksheet = xlWorkwook.Worksheets.Worksheet(1);
                                var Type = worksheet.Cell(1, 1).GetString();

                                if (Type == "Update Department")
                                {
                                    int row = 3;

                                    // Check if the first column is empty
                                    if (worksheet.Cell(row, 1).GetString() == "")
                                    {
                                        TempData["Message"] = "Fill in missing information in the first column.";
                                        TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                        TempData["Icon"] = "error";
                                        return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                    }

                                    var excelDataList1 = new List<Module_Employee_BulkUpdate>();

                                    while (!string.IsNullOrWhiteSpace(worksheet.Cell(row, 1).GetString()))
                                    {
                                        bool hasError = false;
                                        List<string> missingFields = new List<string>();

                                        Module_Employee_BulkUpdate bulkUpdate = new Module_Employee_BulkUpdate();

                                        // Validate fields
                                        string srNo = worksheet.Cell(row, 1).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(srNo)) { hasError = true; missingFields.Add("SrNo"); }

                                        string employeeNo = worksheet.Cell(row, 2).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeNo)) { hasError = true; missingFields.Add("EmployeeNo"); }

                                        string employeeName = worksheet.Cell(row, 3).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeName)) { hasError = true; missingFields.Add("EmployeeName"); }

                                        string employeeDepartmentID = worksheet.Cell(row, 4).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeDepartmentID)) { hasError = true; missingFields.Add("EmployeeDepartmentID"); }

                                        string employeeSubDepartmentName = worksheet.Cell(row, 5).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeSubDepartmentName)) { hasError = true; missingFields.Add("EmployeeSubDepartmentName"); }

                                        if (!hasError)
                                        {
                                            // Populate bulk update object with validated data
                                            bulkUpdate.SrNo = srNo;
                                            bulkUpdate.EmployeeNo = employeeNo;
                                            bulkUpdate.EmployeeName = employeeName;
                                            bulkUpdate.EmployeeDepartmentID = employeeDepartmentID;
                                            bulkUpdate.EmployeeSubDepartmentName = employeeSubDepartmentName;

                                            excelDataList1.Add(bulkUpdate);
                                        }
                                        else
                                        {
                                            // Show message for missing fields
                                            TempData["Message"] = $"Row {row} has missing values: {string.Join(", ", missingFields)}";
                                            TempData["Icon"] = "error";
                                            return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                        }

                                        row++;
                                    }

                                    // Proceed if no errors
                                    ViewBag.GetAadhardata = null;
                                    ViewBag.GetStatutorydata = null;
                                    ViewBag.GetAddressdata = null;
                                    ViewBag.GetCriticalStagedata = null;
                                    ViewBag.GetDepartmentdata = excelDataList1;
                                    ViewBag.GetAttendancedata = null;
                                    ViewBag.GetManpowerCategory = null;
                                    ViewBag.GetLineMaster = null;
                                    ViewBag.GetAssessmentLevel = null;
                                    ViewBag.GradeNameData = null;
                                    ViewBag.DesignationNameData = null;
                                    ViewBag.GetGenderData = null;
                                    ViewBag.DateOfBirthData = null;
                                    ViewBag.GetContractorData = null;
                                    ViewBag.GetMaritalStatusData = null;
                                    ViewBag.GetMobileNoData = null;
                                    ViewBag.GetSubUnitData = null;

                                    return View();
                                }
                                else
                                {
                                    TempData["Message"] = "Import a file based on the selected type.";
                                    TempData["Icon"] = "error";
                                    return View();
                                }
                            }



                            if (GetBulkUpdate.Type == "Update Attendance")
                            {
                                var worksheet = xlWorkwook.Worksheets.Worksheet(1);
                                var Type = worksheet.Cell(1, 1).GetString();

                                if (Type == "Update Attendance")
                                {
                                    int row = 3;

                                    // Check if the first column is empty
                                    if (worksheet.Cell(row, 1).GetString() == "")
                                    {
                                        TempData["Message"] = "Fill in missing information in the first column.";
                                        TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                        TempData["Icon"] = "error";
                                        return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                    }

                                    var excelDataList2 = new List<Module_Employee_BulkUpdate>();

                                    while (!string.IsNullOrWhiteSpace(worksheet.Cell(row, 1).GetString()))
                                    {
                                        bool hasError = false;
                                        List<string> missingFields = new List<string>();

                                        Module_Employee_BulkUpdate bulkUpdate = new Module_Employee_BulkUpdate();

                                        // Validate fields
                                        string srNo = worksheet.Cell(row, 1).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(srNo)) { hasError = true; missingFields.Add("SrNo"); }

                                        string employeeNo = worksheet.Cell(row, 2).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeNo)) { hasError = true; missingFields.Add("EmployeeNo"); }

                                        string employeeName = worksheet.Cell(row, 3).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeName)) { hasError = true; missingFields.Add("EmployeeName"); }

                                        string shiftGroupId = worksheet.Cell(row, 4).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(shiftGroupId)) { hasError = true; missingFields.Add("EM_Atten_ShiftGroupId"); }

                                        string shiftRuleId = worksheet.Cell(row, 5).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(shiftRuleId)) { hasError = true; missingFields.Add("EM_Atten_ShiftRuleId"); }

                                        string woff1 = worksheet.Cell(row, 6).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(woff1)) { hasError = true; missingFields.Add("EM_Atten_WOFF1"); }

                                        string phApplicable = worksheet.Cell(row, 7).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(phApplicable)) { hasError = true; missingFields.Add("PHApplicable"); }

                                        if (!hasError)
                                        {
                                            // Populate bulk update object with validated data
                                            bulkUpdate.SrNo = srNo;
                                            bulkUpdate.EmployeeNo = employeeNo;
                                            bulkUpdate.EmployeeName = employeeName;
                                            bulkUpdate.EM_Atten_ShiftGroupId = shiftGroupId;
                                            bulkUpdate.EM_Atten_ShiftRuleId = shiftRuleId;
                                            bulkUpdate.EM_Atten_WOFF1 = woff1;
                                            bulkUpdate.PHApplicable = phApplicable;

                                            excelDataList2.Add(bulkUpdate);
                                        }
                                        else
                                        {
                                            // Show message for missing fields
                                            TempData["Message"] = $"Row {row} has missing values: {string.Join(", ", missingFields)}";
                                            TempData["Icon"] = "error";
                                            return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                        }

                                        row++;
                                    }

                                    // Proceed if no errors
                                    ViewBag.GetStatutorydata = null;
                                    ViewBag.GetAadhardata = null;
                                    ViewBag.GetAddressdata = null;
                                    ViewBag.GetCriticalStagedata = null;
                                    ViewBag.GetDepartmentdata = null;
                                    ViewBag.GetAttendancedata = excelDataList2;
                                    ViewBag.GetManpowerCategory = null;
                                    ViewBag.GetLineMaster = null;
                                    ViewBag.GetAssessmentLevel = null;
                                    ViewBag.GradeNameData = null;
                                    ViewBag.DesignationNameData = null;
                                    ViewBag.GetGenderData = null;
                                    ViewBag.DateOfBirthData = null;
                                    ViewBag.GetContractorData = null;
                                    ViewBag.GetMaritalStatusData = null;
                                    ViewBag.GetMobileNoData = null;
                                    ViewBag.GetSubUnitData = null;

                                    return View();
                                }
                                else
                                {
                                    TempData["Message"] = "Import a file based on the selected type.";
                                    TempData["Icon"] = "error";
                                    return View();
                                }
                            }



                            if (GetBulkUpdate.Type == "Update Manpower Category")
                            {
                                var worksheet = xlWorkwook.Worksheets.Worksheet(1);
                                var Type = worksheet.Cell(1, 1).GetString();

                                if (Type == "Update Manpower Category")
                                {
                                    int row = 3;

                                    // Check if the first column is empty
                                    if (worksheet.Cell(row, 1).GetString() == "")
                                    {
                                        TempData["Message"] = "Fill in missing information in the first column.";
                                        TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                        TempData["Icon"] = "error";
                                        return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                    }

                                    var excelDataListManpower = new List<Module_Employee_BulkUpdate>();

                                    while (!string.IsNullOrWhiteSpace(worksheet.Cell(row, 1).GetString()))
                                    {
                                        bool hasError = false;
                                        List<string> missingFields = new List<string>();

                                        Module_Employee_BulkUpdate bulkUpdate = new Module_Employee_BulkUpdate();

                                        // Validate fields
                                        string srNo = worksheet.Cell(row, 1).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(srNo)) { hasError = true; missingFields.Add("SrNo"); }

                                        string employeeNo = worksheet.Cell(row, 2).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeNo)) { hasError = true; missingFields.Add("EmployeeNo"); }

                                        string employeeName = worksheet.Cell(row, 3).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeName)) { hasError = true; missingFields.Add("EmployeeName"); }

                                        string allocationCategoryId = worksheet.Cell(row, 4).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(allocationCategoryId)) { hasError = true; missingFields.Add("EmployeeAllocationCategoryId"); }

                                        if (!hasError)
                                        {
                                            // Populate bulk update object with validated data
                                            bulkUpdate.SrNo = srNo;
                                            bulkUpdate.EmployeeNo = employeeNo;
                                            bulkUpdate.EmployeeName = employeeName;
                                            bulkUpdate.EmployeeAllocationCategoryId = allocationCategoryId;

                                            excelDataListManpower.Add(bulkUpdate);
                                        }
                                        else
                                        {
                                            // Show message for missing fields
                                            TempData["Message"] = $"Row {row} has missing values: {string.Join(", ", missingFields)}";
                                            TempData["Icon"] = "error";
                                            return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                        }

                                        row++;
                                    }

                                    // Proceed if no errors
                                    ViewBag.GetStatutorydata = null;
                                    ViewBag.GetAadhardata = null;
                                    ViewBag.GetAddressdata = null;
                                    ViewBag.GetCriticalStagedata = null;
                                    ViewBag.GetDepartmentdata = null;
                                    ViewBag.GetAttendancedata = null;
                                    ViewBag.GetManpowerCategory = excelDataListManpower;
                                    ViewBag.GetLineMaster = null;
                                    ViewBag.GetAssessmentLevel = null;
                                    ViewBag.GradeNameData = null;
                                    ViewBag.DesignationNameData = null;
                                    ViewBag.GetGenderData = null;
                                    ViewBag.DateOfBirthData = null;
                                    ViewBag.GetContractorData = null;
                                    ViewBag.GetMaritalStatusData = null;
                                    ViewBag.GetMobileNoData = null;
                                    ViewBag.GetSubUnitData = null;

                                    return View();
                                }
                                else
                                {
                                    TempData["Message"] = "Import a file based on the selected type.";
                                    TempData["Icon"] = "error";
                                    return View();
                                }
                            }


                            if (GetBulkUpdate.Type == "Update Line Master")
                            {
                                var worksheet = xlWorkwook.Worksheets.Worksheet(1);
                                var Type = worksheet.Cell(1, 1).GetString();

                                if (Type == "Update Line Master")
                                {
                                    int row = 3;

                                    // Check if the first column is empty
                                    if (worksheet.Cell(row, 1).GetString() == "")
                                    {
                                        TempData["Message"] = "Fill in missing information in the first column.";
                                        TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                        TempData["Icon"] = "error";
                                        return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                    }

                                    var excelDataListLine = new List<Module_Employee_BulkUpdate>();

                                    while (!string.IsNullOrWhiteSpace(worksheet.Cell(row, 1).GetString()))
                                    {
                                        bool hasError = false;
                                        List<string> missingFields = new List<string>();

                                        Module_Employee_BulkUpdate bulkUpdate = new Module_Employee_BulkUpdate();

                                        // Validate fields
                                        string srNo = worksheet.Cell(row, 1).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(srNo)) { hasError = true; missingFields.Add("SrNo"); }

                                        string employeeNo = worksheet.Cell(row, 2).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeNo)) { hasError = true; missingFields.Add("EmployeeNo"); }

                                        string employeeName = worksheet.Cell(row, 3).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeName)) { hasError = true; missingFields.Add("EmployeeName"); }

                                        string lineID = worksheet.Cell(row, 4).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(lineID)) { hasError = true; missingFields.Add("EmployeeLineID"); }

                                        if (!hasError)
                                        {
                                            // Populate bulk update object with validated data
                                            bulkUpdate.SrNo = srNo;
                                            bulkUpdate.EmployeeNo = employeeNo;
                                            bulkUpdate.EmployeeName = employeeName;
                                            bulkUpdate.EmployeeLineID = lineID;

                                            excelDataListLine.Add(bulkUpdate);
                                        }
                                        else
                                        {
                                            // Show message for missing fields
                                            TempData["Message"] = $"Row {row} has missing values: {string.Join(", ", missingFields)}";
                                            TempData["Icon"] = "error";
                                            return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                        }

                                        row++;
                                    }

                                    // Proceed if no errors
                                    ViewBag.GetStatutorydata = null;
                                    ViewBag.GetAadhardata = null;
                                    ViewBag.GetAddressdata = null;
                                    ViewBag.GetCriticalStagedata = null;
                                    ViewBag.GetDepartmentdata = null;
                                    ViewBag.GetAttendancedata = null;
                                    ViewBag.GetManpowerCategory = null;
                                    ViewBag.GetLineMaster = excelDataListLine;
                                    ViewBag.GetAssessmentLevel = null;
                                    ViewBag.GradeNameData = null;
                                    ViewBag.DesignationNameData = null;
                                    ViewBag.GetGenderData = null;
                                    ViewBag.DateOfBirthData = null;
                                    ViewBag.GetContractorData = null;
                                    ViewBag.GetMaritalStatusData = null;
                                    ViewBag.GetMobileNoData = null;
                                    ViewBag.GetSubUnitData = null;

                                    return View();
                                }
                                else
                                {
                                    TempData["Message"] = "Import a file based on the selected type.";
                                    TempData["Icon"] = "error";
                                    return View();
                                }
                            }



                            if (GetBulkUpdate.Type == "Update Assessment Level")
                            {
                                var worksheet = xlWorkwook.Worksheets.Worksheet(1);
                                var Type = worksheet.Cell(1, 1).GetString();

                                if (Type == "Update Assessment Level")
                                {
                                    int row = 3;

                                    // Check if the first column is empty
                                    if (worksheet.Cell(row, 1).GetString() == "")
                                    {
                                        TempData["Message"] = "Fill in missing information in the first column.";
                                        TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                        TempData["Icon"] = "error";
                                        return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                    }

                                    var excelDataListLevel = new List<Module_Employee_BulkUpdate>();

                                    while (!string.IsNullOrWhiteSpace(worksheet.Cell(row, 1).GetString()))
                                    {
                                        bool hasError = false;
                                        List<string> missingFields = new List<string>();

                                        Module_Employee_BulkUpdate bulkUpdate = new Module_Employee_BulkUpdate();

                                        // Validate fields
                                        string srNo = worksheet.Cell(row, 1).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(srNo)) { hasError = true; missingFields.Add("SrNo"); }

                                        string employeeNo = worksheet.Cell(row, 2).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeNo)) { hasError = true; missingFields.Add("EmployeeNo"); }

                                        string employeeName = worksheet.Cell(row, 3).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeName)) { hasError = true; missingFields.Add("EmployeeName"); }

                                        string levelID = worksheet.Cell(row, 4).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(levelID)) { hasError = true; missingFields.Add("EmployeeLevelID"); }

                                        if (!hasError)
                                        {
                                            // Populate bulk update object with validated data
                                            bulkUpdate.SrNo = srNo;
                                            bulkUpdate.EmployeeNo = employeeNo;
                                            bulkUpdate.EmployeeName = employeeName;
                                            bulkUpdate.EmployeeLevelID = levelID;

                                            excelDataListLevel.Add(bulkUpdate);
                                        }
                                        else
                                        {
                                            // Show message for missing fields
                                            TempData["Message"] = $"Row {row} has missing values: {string.Join(", ", missingFields)}";
                                            TempData["Icon"] = "error";
                                            return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                        }

                                        row++;
                                    }

                                    // Proceed if no errors
                                    ViewBag.GetStatutorydata = null;
                                    ViewBag.GetAadhardata = null;
                                    ViewBag.GetAddressdata = null;
                                    ViewBag.GetCriticalStagedata = null;
                                    ViewBag.GetDepartmentdata = null;
                                    ViewBag.GetAttendancedata = null;
                                    ViewBag.GetManpowerCategory = null;
                                    ViewBag.GetLineMaster = null;
                                    ViewBag.GetAssessmentLevel = excelDataListLevel;
                                    ViewBag.GradeNameData = null;
                                    ViewBag.DesignationNameData = null;
                                    ViewBag.GetGenderData = null;
                                    ViewBag.DateOfBirthData = null;
                                    ViewBag.GetContractorData = null;
                                    ViewBag.GetMaritalStatusData = null;
                                    ViewBag.GetMobileNoData = null;
                                    ViewBag.GetSubUnitData = null;

                                    return View();
                                }
                                else
                                {
                                    TempData["Message"] = "Import a file based on the selected type.";
                                    TempData["Icon"] = "error";
                                    return View();
                                }
                            }



                            if (GetBulkUpdate.Type == "Update Employee Gender")
                            {
                                var worksheet = xlWorkwook.Worksheets.Worksheet(1);
                                var Type = worksheet.Cell(1, 1).GetString();

                                if (Type == "Update Employee Gender")
                                {
                                    int row = 3;

                                    // Check if the first column is empty
                                    if (worksheet.Cell(row, 1).GetString() == "")
                                    {
                                        TempData["Message"] = "Fill in missing information in the first column.";
                                        TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                        TempData["Icon"] = "error";
                                        return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                    }

                                    var excelDataListGender = new List<Module_Employee_BulkUpdate>();

                                    while (!string.IsNullOrWhiteSpace(worksheet.Cell(row, 1).GetString()))
                                    {
                                        bool hasError = false;
                                        List<string> missingFields = new List<string>();

                                        Module_Employee_BulkUpdate bulkUpdate = new Module_Employee_BulkUpdate();

                                        // Validate fields
                                        string srNo = worksheet.Cell(row, 1).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(srNo)) { hasError = true; missingFields.Add("SrNo"); }

                                        string employeeNo = worksheet.Cell(row, 2).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeNo)) { hasError = true; missingFields.Add("EmployeeNo"); }

                                        string employeeName = worksheet.Cell(row, 3).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeName)) { hasError = true; missingFields.Add("EmployeeName"); }

                                        string gender = worksheet.Cell(row, 4).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(gender)) { hasError = true; missingFields.Add("Gender"); }

                                        if (!hasError)
                                        {
                                            // Populate bulk update object with validated data
                                            bulkUpdate.SrNo = srNo;
                                            bulkUpdate.EmployeeNo = employeeNo;
                                            bulkUpdate.EmployeeName = employeeName;
                                            bulkUpdate.Gender = gender;

                                            excelDataListGender.Add(bulkUpdate);
                                        }
                                        else
                                        {
                                            // Show message for missing fields
                                            TempData["Message"] = $"Row {row} has missing values: {string.Join(", ", missingFields)}";
                                            TempData["Icon"] = "error";
                                            return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                        }

                                        row++;
                                    }

                                    // Proceed if no errors
                                    ViewBag.GetStatutorydata = null;
                                    ViewBag.GetAadhardata = null;
                                    ViewBag.GetAddressdata = null;
                                    ViewBag.GetCriticalStagedata = null;
                                    ViewBag.GetDepartmentdata = null;
                                    ViewBag.GetAttendancedata = null;
                                    ViewBag.GetManpowerCategory = null;
                                    ViewBag.GetLineMaster = null;
                                    ViewBag.GetAssessmentLevel = null;
                                    ViewBag.GradeNameData = null;
                                    ViewBag.DesignationNameData = null;
                                    ViewBag.GetGenderData = excelDataListGender;
                                    ViewBag.DateOfBirthData = null;
                                    ViewBag.GetContractorData = null;
                                    ViewBag.GetMaritalStatusData = null;
                                    ViewBag.GetMobileNoData = null;
                                    ViewBag.GetSubUnitData = null;

                                    return View();
                                }
                                else
                                {
                                    TempData["Message"] = "Import a file based on the selected type.";
                                    TempData["Icon"] = "error";
                                    return View();
                                }
                            }


                            if (GetBulkUpdate.Type == "Update Employee Grade")
                            {
                                var worksheet = xlWorkwook.Worksheets.Worksheet(1);
                                var Type = worksheet.Cell(1, 1).GetString();

                                if (Type == "Update Employee Grade")
                                {
                                    int row = 3;

                                    // Check if the first column is empty
                                    if (worksheet.Cell(row, 1).GetString() == "")
                                    {
                                        TempData["Message"] = "Fill in missing information in the first column.";
                                        TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                        TempData["Icon"] = "error";
                                        return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                    }

                                    var excelDataListGrade = new List<Module_Employee_BulkUpdate>();

                                    while (!string.IsNullOrWhiteSpace(worksheet.Cell(row, 1).GetString()))
                                    {
                                        bool hasError = false;
                                        List<string> missingFields = new List<string>();

                                        Module_Employee_BulkUpdate bulkUpdate = new Module_Employee_BulkUpdate();

                                        // Validate fields
                                        string srNo = worksheet.Cell(row, 1).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(srNo)) { hasError = true; missingFields.Add("SrNo"); }

                                        string employeeNo = worksheet.Cell(row, 2).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeNo)) { hasError = true; missingFields.Add("EmployeeNo"); }

                                        string employeeName = worksheet.Cell(row, 3).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeName)) { hasError = true; missingFields.Add("EmployeeName"); }

                                        string gradeName = worksheet.Cell(row, 4).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(gradeName)) { hasError = true; missingFields.Add("GradeName"); }

                                        if (!hasError)
                                        {
                                            // Populate bulk update object with validated data
                                            bulkUpdate.SrNo = srNo;
                                            bulkUpdate.EmployeeNo = employeeNo;
                                            bulkUpdate.EmployeeName = employeeName;
                                            bulkUpdate.GradeName = gradeName;

                                            excelDataListGrade.Add(bulkUpdate);
                                        }
                                        else
                                        {
                                            // Show message for missing fields
                                            TempData["Message"] = $"Row {row} has missing values: {string.Join(", ", missingFields)}";
                                            TempData["Icon"] = "error";
                                            return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                        }

                                        row++;
                                    }

                                    // Proceed if no errors
                                    ViewBag.GetStatutorydata = null;
                                    ViewBag.GetAadhardata = null;
                                    ViewBag.GetAddressdata = null;
                                    ViewBag.GetCriticalStagedata = null;
                                    ViewBag.GetDepartmentdata = null;
                                    ViewBag.GetAttendancedata = null;
                                    ViewBag.GetManpowerCategory = null;
                                    ViewBag.GetLineMaster = null;
                                    ViewBag.GetAssessmentLevel = null;
                                    ViewBag.GradeNameData = excelDataListGrade;
                                    ViewBag.DesignationNameData = null;
                                    ViewBag.GetGenderData = null;
                                    ViewBag.DateOfBirthData = null;
                                    ViewBag.GetContractorData = null;
                                    ViewBag.GetMaritalStatusData = null;
                                    ViewBag.GetMobileNoData = null;
                                    ViewBag.GetSubUnitData = null;

                                    return View();
                                }
                                else
                                {
                                    TempData["Message"] = "Import a file based on the selected type.";
                                    TempData["Icon"] = "error";
                                    return View();
                                }
                            }
                            if (GetBulkUpdate.Type == "Update Employee Designation")
                            {
                                var worksheet = xlWorkwook.Worksheets.Worksheet(1);
                                var Type = worksheet.Cell(1, 1).GetString();

                                if (Type == "Update Employee Designation")
                                {
                                    int row = 3;

                                    // Check if the first column is empty
                                    if (worksheet.Cell(row, 1).GetString() == "")
                                    {
                                        TempData["Message"] = "Fill in missing information in the first column.";
                                        TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                        TempData["Icon"] = "error";
                                        return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                    }

                                    var excelDataListDesignation = new List<Module_Employee_BulkUpdate>();

                                    while (!string.IsNullOrWhiteSpace(worksheet.Cell(row, 1).GetString()))
                                    {
                                        bool hasError = false;
                                        List<string> missingFields = new List<string>();

                                        Module_Employee_BulkUpdate bulkUpdate = new Module_Employee_BulkUpdate();

                                        // Validate fields
                                        string srNo = worksheet.Cell(row, 1).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(srNo)) { hasError = true; missingFields.Add("SrNo"); }

                                        string employeeNo = worksheet.Cell(row, 2).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeNo)) { hasError = true; missingFields.Add("EmployeeNo"); }

                                        string employeeName = worksheet.Cell(row, 3).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeName)) { hasError = true; missingFields.Add("EmployeeName"); }

                                        string designationName = worksheet.Cell(row, 4).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(designationName)) { hasError = true; missingFields.Add("DesignationName"); }

                                        if (!hasError)
                                        {
                                            // Populate bulk update object with validated data
                                            bulkUpdate.SrNo = srNo;
                                            bulkUpdate.EmployeeNo = employeeNo;
                                            bulkUpdate.EmployeeName = employeeName;
                                            bulkUpdate.DesignationName = designationName;

                                            excelDataListDesignation.Add(bulkUpdate);
                                        }
                                        else
                                        {
                                            // Show message for missing fields
                                            TempData["Message"] = $"Row {row} has missing values: {string.Join(", ", missingFields)}";
                                            TempData["Icon"] = "error";
                                            return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                        }

                                        row++;
                                    }

                                    // Proceed if no errors
                                    ViewBag.GetStatutorydata = null;
                                    ViewBag.GetAadhardata = null;
                                    ViewBag.GetAddressdata = null;
                                    ViewBag.GetCriticalStagedata = null;
                                    ViewBag.GetDepartmentdata = null;
                                    ViewBag.GetAttendancedata = null;
                                    ViewBag.GetManpowerCategory = null;
                                    ViewBag.GetLineMaster = null;
                                    ViewBag.GetAssessmentLevel = null;
                                    ViewBag.GradeNameData = null;
                                    ViewBag.DesignationNameData = excelDataListDesignation;
                                    ViewBag.GetGenderData = null;
                                    ViewBag.DateOfBirthData = null;
                                    ViewBag.GetContractorData = null;
                                    ViewBag.GetMaritalStatusData = null;
                                    ViewBag.GetMobileNoData = null;
                                    ViewBag.GetSubUnitData = null;

                                    return View();
                                }
                                else
                                {
                                    TempData["Message"] = "Import a file based on the selected type.";
                                    TempData["Icon"] = "error";
                                    return View();
                                }
                            }

                            if (GetBulkUpdate.Type == "Update Employee Date Of Birth")
                            {
                                var worksheet = xlWorkwook.Worksheets.Worksheet(1);
                                var Type = worksheet.Cell(1, 1).GetString();

                                if (Type == "Update Employee Date Of Birth")
                                {
                                    int row = 3;

                                    // Check if the first column is empty
                                    if (worksheet.Cell(row, 1).GetString() == "")
                                    {
                                        TempData["Message"] = "Fill in missing information in the first column.";
                                        TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                        TempData["Icon"] = "error";
                                        return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                    }

                                    var excelDataListDate = new List<Module_Employee_BulkUpdate>();

                                    while (!string.IsNullOrWhiteSpace(worksheet.Cell(row, 1).GetString()))
                                    {
                                        bool hasError = false;
                                        List<string> missingFields = new List<string>();

                                        Module_Employee_BulkUpdate bulkUpdate = new Module_Employee_BulkUpdate();

                                        // Validate fields
                                        string srNo = worksheet.Cell(row, 1).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(srNo)) { hasError = true; missingFields.Add("SrNo"); }

                                        string employeeNo = worksheet.Cell(row, 2).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeNo)) { hasError = true; missingFields.Add("EmployeeNo"); }

                                        string employeeName = worksheet.Cell(row, 3).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeName)) { hasError = true; missingFields.Add("EmployeeName"); }

                                        // Date of Birth
                                        var birthdayDateCell = worksheet.Cell(row, 4);
                                        DateTime? birthdayDate = null;
                                        if (birthdayDateCell.IsEmpty() || birthdayDateCell.GetValue<DateTime?>() == null)
                                        {
                                            birthdayDate = DateTime.Now; // Assign current date if empty
                                        }
                                        else
                                        {
                                            birthdayDate = birthdayDateCell.GetDateTime();
                                        }

                                        if (birthdayDate == null)
                                        {
                                            hasError = true;
                                            missingFields.Add("BirthdayDate");
                                        }

                                        if (!hasError)
                                        {
                                            // Populate bulk update object with validated data
                                            bulkUpdate.SrNo = srNo;
                                            bulkUpdate.EmployeeNo = employeeNo;
                                            bulkUpdate.EmployeeName = employeeName;
                                            bulkUpdate.BirthdayDate = birthdayDate.Value.ToString("yyyy-MM-dd");

                                            excelDataListDate.Add(bulkUpdate);
                                        }
                                        else
                                        {
                                            // Show message for missing fields
                                            TempData["Message"] = $"Row {row} has missing values: {string.Join(", ", missingFields)}";
                                            TempData["Icon"] = "error";
                                            return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                        }

                                        row++;
                                    }

                                    // Proceed if no errors
                                    ViewBag.GetStatutorydata = null;
                                    ViewBag.GetAadhardata = null;
                                    ViewBag.GetAddressdata = null;
                                    ViewBag.GetCriticalStagedata = null;
                                    ViewBag.GetDepartmentdata = null;
                                    ViewBag.GetAttendancedata = null;
                                    ViewBag.GetManpowerCategory = null;
                                    ViewBag.GetLineMaster = null;
                                    ViewBag.GetAssessmentLevel = null;
                                    ViewBag.GradeNameData = null;
                                    ViewBag.DesignationNameData = null;
                                    ViewBag.GetGenderData = null;
                                    ViewBag.DateOfBirthData = excelDataListDate;
                                    ViewBag.GetContractorData = null;
                                    ViewBag.GetMaritalStatusData = null;
                                    ViewBag.GetMobileNoData = null;
                                    ViewBag.GetSubUnitData = null;

                                    return View();
                                }
                                else
                                {
                                    TempData["Message"] = "Import a file based on the selected type.";
                                    TempData["Icon"] = "error";
                                    return View();
                                }
                            }


                            if (GetBulkUpdate.Type == "Update Employee Contractor")
                            {
                                var worksheet = xlWorkwook.Worksheets.Worksheet(1);
                                var Type = worksheet.Cell(1, 1).GetString();

                                if (Type == "Update Employee Contractor")
                                {
                                    int row = 3;

                                    // Check if the first column is empty
                                    if (worksheet.Cell(row, 1).GetString() == "")
                                    {
                                        TempData["Message"] = "Fill in missing information in the first column.";
                                        TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                        TempData["Icon"] = "error";
                                        return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                    }

                                    var excelDataListContractor = new List<Module_Employee_BulkUpdate>();

                                    while (!string.IsNullOrWhiteSpace(worksheet.Cell(row, 1).GetString()))
                                    {
                                        bool hasError = false;
                                        List<string> missingFields = new List<string>();

                                        Module_Employee_BulkUpdate bulkUpdate = new Module_Employee_BulkUpdate();

                                        // Validate fields
                                        string srNo = worksheet.Cell(row, 1).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(srNo)) { hasError = true; missingFields.Add("SrNo"); }

                                        string employeeNo = worksheet.Cell(row, 2).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeNo)) { hasError = true; missingFields.Add("EmployeeNo"); }

                                        string employeeName = worksheet.Cell(row, 3).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeName)) { hasError = true; missingFields.Add("EmployeeName"); }

                                        string contractorName = worksheet.Cell(row, 4).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(contractorName)) { hasError = true; missingFields.Add("ContractorName"); }

                                        if (!hasError)
                                        {
                                            // Populate bulk update object with validated data
                                            bulkUpdate.SrNo = srNo;
                                            bulkUpdate.EmployeeNo = employeeNo;
                                            bulkUpdate.EmployeeName = employeeName;
                                            bulkUpdate.ContractorName = contractorName;

                                            excelDataListContractor.Add(bulkUpdate);
                                        }
                                        else
                                        {
                                            // Show message for missing fields
                                            TempData["Message"] = $"Row {row} has missing values: {string.Join(", ", missingFields)}";
                                            TempData["Icon"] = "error";
                                            return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                        }

                                        row++;
                                    }

                                    // Proceed if no errors
                                    ViewBag.GetStatutorydata = null;
                                    ViewBag.GetAadhardata = null;
                                    ViewBag.GetAddressdata = null;
                                    ViewBag.GetCriticalStagedata = null;
                                    ViewBag.GetDepartmentdata = null;
                                    ViewBag.GetAttendancedata = null;
                                    ViewBag.GetManpowerCategory = null;
                                    ViewBag.GetLineMaster = null;
                                    ViewBag.GetAssessmentLevel = null;
                                    ViewBag.GradeNameData = null;
                                    ViewBag.DesignationNameData = null;
                                    ViewBag.GetGenderData = null;
                                    ViewBag.DateOfBirthData = null;
                                    ViewBag.GetContractorData = excelDataListContractor;
                                    ViewBag.GetMaritalStatusData = null;
                                    ViewBag.GetMobileNoData = null;
                                    ViewBag.GetSubUnitData = null;

                                    return View();
                                }
                                else
                                {
                                    TempData["Message"] = "Import a file based on the selected type.";
                                    TempData["Icon"] = "error";
                                    return View();
                                }
                            }


                            if (GetBulkUpdate.Type == "Update Employee Marital Status")
                            {
                                var worksheet = xlWorkwook.Worksheets.Worksheet(1);
                                var Type = worksheet.Cell(1, 1).GetString();

                                if (Type == "Update Employee Marital Status")
                                {
                                    int row = 3;

                                    // Check if the first column is empty
                                    if (worksheet.Cell(row, 1).GetString() == "")
                                    {
                                        TempData["Message"] = "Fill in missing information in the first column.";
                                        TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                        TempData["Icon"] = "error";
                                        return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                    }

                                    var excelDataListStatus = new List<Module_Employee_BulkUpdate>();

                                    while (!string.IsNullOrWhiteSpace(worksheet.Cell(row, 1).GetString()))
                                    {
                                        bool hasError = false;
                                        List<string> missingFields = new List<string>();

                                        Module_Employee_BulkUpdate bulkUpdate = new Module_Employee_BulkUpdate();

                                        // Validate fields
                                        string srNo = worksheet.Cell(row, 1).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(srNo)) { hasError = true; missingFields.Add("SrNo"); }

                                        string employeeNo = worksheet.Cell(row, 2).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeNo)) { hasError = true; missingFields.Add("EmployeeNo"); }

                                        string employeeName = worksheet.Cell(row, 3).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeName)) { hasError = true; missingFields.Add("EmployeeName"); }

                                        string maritalStatus = worksheet.Cell(row, 4).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(maritalStatus)) { hasError = true; missingFields.Add("MaritalStatus"); }

                                        if (!hasError)
                                        {
                                            // Populate bulk update object with validated data
                                            bulkUpdate.SrNo = srNo;
                                            bulkUpdate.EmployeeNo = employeeNo;
                                            bulkUpdate.EmployeeName = employeeName;
                                            bulkUpdate.MaritalStatus = maritalStatus;

                                            excelDataListStatus.Add(bulkUpdate);
                                        }
                                        else
                                        {
                                            // Show message for missing fields
                                            TempData["Message"] = $"Row {row} has missing values: {string.Join(", ", missingFields)}";
                                            TempData["Icon"] = "error";
                                            return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                        }

                                        row++;
                                    }

                                    // Proceed if no errors
                                    ViewBag.GetStatutorydata = null;
                                    ViewBag.GetAadhardata = null;
                                    ViewBag.GetAddressdata = null;
                                    ViewBag.GetCriticalStagedata = null;
                                    ViewBag.GetDepartmentdata = null;
                                    ViewBag.GetAttendancedata = null;
                                    ViewBag.GetManpowerCategory = null;
                                    ViewBag.GetLineMaster = null;
                                    ViewBag.GetAssessmentLevel = null;
                                    ViewBag.GradeNameData = null;
                                    ViewBag.DesignationNameData = null;
                                    ViewBag.GetGenderData = null;
                                    ViewBag.DateOfBirthData = null;
                                    ViewBag.GetContractorData = null;
                                    ViewBag.GetMaritalStatusData = excelDataListStatus;
                                    ViewBag.GetMobileNoData = null;
                                    ViewBag.GetSubUnitData = null;

                                    return View();
                                }
                                else
                                {
                                    TempData["Message"] = "Import a file based on the selected type.";
                                    TempData["Icon"] = "error";
                                    return View();
                                }
                            }


                            if (GetBulkUpdate.Type == "Update Employee Mobile No")
                            {
                                var worksheet = xlWorkwook.Worksheets.Worksheet(1);
                                var Type = worksheet.Cell(1, 1).GetString();

                                if (Type == "Update Employee Mobile No")
                                {
                                    int row = 3;

                                    // Check if the first column is empty
                                    if (worksheet.Cell(row, 1).GetString() == "")
                                    {
                                        TempData["Message"] = "Fill in missing information in the first column.";
                                        TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                        TempData["Icon"] = "error";
                                        return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                    }

                                    var excelDataListMobile = new List<Module_Employee_BulkUpdate>();

                                    while (!string.IsNullOrWhiteSpace(worksheet.Cell(row, 1).GetString()))
                                    {
                                        bool hasError = false;
                                        List<string> missingFields = new List<string>();

                                        Module_Employee_BulkUpdate bulkUpdate = new Module_Employee_BulkUpdate();

                                        // Validate fields
                                        string srNo = worksheet.Cell(row, 1).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(srNo)) { hasError = true; missingFields.Add("SrNo"); }

                                        string employeeNo = worksheet.Cell(row, 2).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeNo)) { hasError = true; missingFields.Add("EmployeeNo"); }

                                        string employeeName = worksheet.Cell(row, 3).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeName)) { hasError = true; missingFields.Add("EmployeeName"); }

                                        string mobileNo = worksheet.Cell(row, 4).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(mobileNo)) { hasError = true; missingFields.Add("MobileNo"); }

                                        if (!hasError)
                                        {
                                            // Populate bulk update object with validated data
                                            bulkUpdate.SrNo = srNo;
                                            bulkUpdate.EmployeeNo = employeeNo;
                                            bulkUpdate.EmployeeName = employeeName;
                                            bulkUpdate.PrimaryMobile = mobileNo;

                                            excelDataListMobile.Add(bulkUpdate);
                                        }
                                        else
                                        {
                                            // Show message for missing fields
                                            TempData["Message"] = $"Row {row} has missing values: {string.Join(", ", missingFields)}";
                                            TempData["Icon"] = "error";
                                            return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                        }

                                        row++;
                                    }

                                    // Proceed if no errors
                                    ViewBag.GetStatutorydata = null;
                                    ViewBag.GetAadhardata = null;
                                    ViewBag.GetAddressdata = null;
                                    ViewBag.GetCriticalStagedata = null;
                                    ViewBag.GetDepartmentdata = null;
                                    ViewBag.GetAttendancedata = null;
                                    ViewBag.GetManpowerCategory = null;
                                    ViewBag.GetLineMaster = null;
                                    ViewBag.GetAssessmentLevel = null;
                                    ViewBag.GradeNameData = null;
                                    ViewBag.DesignationNameData = null;
                                    ViewBag.GetGenderData = null;
                                    ViewBag.DateOfBirthData = null;
                                    ViewBag.GetContractorData = null;
                                    ViewBag.GetMaritalStatusData = null;
                                    ViewBag.GetMobileNoData = excelDataListMobile;
                                    ViewBag.GetSubUnitData = null;

                                    return View();
                                }
                                else
                                {
                                    TempData["Message"] = "Import a file based on the selected type.";
                                    TempData["Icon"] = "error";
                                    return View();
                                }
                            }


                            if (GetBulkUpdate.Type == "Update Sub Unit")
                            {
                                var worksheet = xlWorkwook.Worksheets.Worksheet(1);
                                var Type = worksheet.Cell(1, 1).GetString();

                                if (Type == "Update Sub Unit")
                                {
                                    int row = 3;

                                    // Check if the first column is empty
                                    if (worksheet.Cell(row, 1).GetString() == "")
                                    {
                                        TempData["Message"] = "Fill in missing information in the first column.";
                                        TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                        TempData["Icon"] = "error";
                                        return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                    }

                                    var excelDataListUnit = new List<Module_Employee_BulkUpdate>();

                                    while (!string.IsNullOrWhiteSpace(worksheet.Cell(row, 1).GetString()))
                                    {
                                        bool hasError = false;
                                        List<string> missingFields = new List<string>();

                                        Module_Employee_BulkUpdate bulkUpdate = new Module_Employee_BulkUpdate();

                                        // Validate fields
                                        string srNo = worksheet.Cell(row, 1).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(srNo)) { hasError = true; missingFields.Add("SrNo"); }

                                        string employeeNo = worksheet.Cell(row, 2).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeNo)) { hasError = true; missingFields.Add("EmployeeNo"); }

                                        string employeeName = worksheet.Cell(row, 3).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(employeeName)) { hasError = true; missingFields.Add("EmployeeName"); }

                                        string subUnitName = worksheet.Cell(row, 4).GetString().Trim();
                                        if (string.IsNullOrWhiteSpace(subUnitName)) { hasError = true; missingFields.Add("SubUnitName"); }

                                        if (!hasError)
                                        {
                                            // Populate bulk update object with validated data
                                            bulkUpdate.SrNo = srNo;
                                            bulkUpdate.EmployeeNo = employeeNo;
                                            bulkUpdate.EmployeeName = employeeName;
                                            bulkUpdate.SubUnitName = subUnitName;

                                            excelDataListUnit.Add(bulkUpdate);
                                        }
                                        else
                                        {
                                            // Show message for missing fields
                                            TempData["Message"] = $"Row {row} has missing values: {string.Join(", ", missingFields)}";
                                            TempData["Icon"] = "error";
                                            return RedirectToAction("Module_Employee_BulkUpdate", "Module_Employee_BulkUpdate");
                                        }

                                        row++;
                                    }

                                    // Proceed if no errors
                                    ViewBag.GetStatutorydata = null;
                                    ViewBag.GetAadhardata = null;
                                    ViewBag.GetAddressdata = null;
                                    ViewBag.GetCriticalStagedata = null;
                                    ViewBag.GetDepartmentdata = null;
                                    ViewBag.GetAttendancedata = null;
                                    ViewBag.GetManpowerCategory = null;
                                    ViewBag.GetLineMaster = null;
                                    ViewBag.GetAssessmentLevel = null;
                                    ViewBag.GradeNameData = null;
                                    ViewBag.DesignationNameData = null;
                                    ViewBag.GetGenderData = null;
                                    ViewBag.DateOfBirthData = null;
                                    ViewBag.GetContractorData = null;
                                    ViewBag.GetMaritalStatusData = null;
                                    ViewBag.GetMobileNoData = null;
                                    ViewBag.GetSubUnitData = excelDataListUnit;
                                

                                    return View();
                                }
                                else
                                {
                                    TempData["Message"] = "Import a file based on the selected type.";
                                    TempData["Icon"] = "error";
                                    return View();
                                }
                            }


                        }
                    }

                    var results = DapperORM.DynamicQueryMultiple(@"select DepartmentId as Id, DepartmentName as Name from Mas_Department where Deactivate=0 order by DepartmentName;
                                                         select DesignationId as Id,DesignationName as Name from Mas_Designation where Deactivate = 0 and ApplicableForWorker = 1 order by DesignationName; 
                                                         select GradeId as Id,GradeName as Name from Mas_Grade where Deactivate=0 order by Name;
                                                         Select EmployeeLevelId as Id ,EmployeeLevel As Name from Mas_EmployeeLevel where Deactivate=0 order by Name ;
                                                         Select QualificationPFId as Id ,QualificationPFName As Name from Mas_Qualification_PF where Deactivate=0 order by Name;
                                                         Select CriticalStageId As Id,CriticalStageName as Name from Mas_CriticalStageCategory where Mas_CriticalStageCategory.Deactivate=0  order by CriticalStageId;");
                    ViewBag.DepartmentList = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                    ViewBag.DesignationList = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                    ViewBag.GradeList = results[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                    //ViewBag.ContractorNameList = results[3].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                    ViewBag.EmployeeLevel = results[3].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                    ViewBag.QualificationName = results[4].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                    ViewBag.CriticalStageName = results[5].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                    ViewBag.SubDepartmentList = "";
                    ViewBag.DesignationList = "";
                    ViewBag.ContractorNameList = "";
                    ViewBag.EmployeeAllocationCategory = "";

                }
                return View();
            }
            catch (Exception ex)
            {

                var lineNumber = new System.Diagnostics.StackTrace(ex, true)
                       .GetFrame(0)?
                       .GetFileLineNumber();

                // Store the error message and line number in the session
                Session["GetErrorMessage"] = $"{ex.Message} Line No: {lineNumber}";
                //      Session["GetErrorMessage"] = ex.Message+" Line No".;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region SaveUpdate
        int List_Contractor_Id = 0;
        int Contractor_Id = 0;
        int List_Department_Id = 0;
        int List_SubDepatment_Id = 0;
        int List_Designation_Id = 0;
        int List_Grade_Id = 0;
        int List_Line_Id = 0;
        int List_Unit_Id = 0;
        int Unit_Id = 0;
        int List_ShiftGroup_Id = 0;
        int List_ShiftRule_Id = 0;
        int List_AssessmentLevel_Id = 0;
        int List_CriticalStage_Id = 0;
        int List_ManpowerCategory_Id = 0;
        int List_Employee_Id = 0;
        int Department_Id = 0;
        int SubDepatment_Id = 0;
        int Designation_Id = 0;
        int Grade_Id = 0;
        int Line_Id = 0;
        int ShiftGroup_Id = 0;
        int ShiftRule_Id = 0;
        int AssessmentLevel_Id = 0;
        int CriticalStage_Id = 0;
        int ManpowerCategory_Id = 0;
        int Employee_Id = 0;
        int EmployeeNo_Id = 0;
        bool GetIsCriticalStageApplicable = false;
        bool GetPHApplicable = false;
        string List_EmployeeName = "";
        string EmployeeName = "";
        string EmployeeNo = "";
        string BranchName = "";


        public ActionResult SaveUpdate(List<Module_Employee_BulkUpdate> tbldata, string Type, int CmpId, int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                string AdharNopattern = @"^\d{12}$";
                string MobileNopattern = @"^\d{10}$";
                string Pincodepattern = @"^\d{6}$";
                string ESICPattern = @"^[a-zA-Z0-9]{10,17}$";

                dt_company = objcon.GetDataTable("select CompanyId as Id, CompanyName as Name from Mas_CompanyProfile  where Deactivate = 0  and  CompanyId in ( Select distinct CmpID from UserBranchMapping where IsActive=1 and EmployeeID=" + Session["EmployeeId"] + ") order by CompanyName ;");
                dt_BU = objcon.GetDataTable("Select UserBranchMapping.BranchID,Mas_Branch.BranchName,Convert (int,UserBranchMapping.CmpId) as CmpId from UserBranchMapping,Mas_Branch where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.BranchID=Mas_Branch.BranchId and Mas_Branch.Deactivate=0");
                dt_ManpowerCategory = objcon.GetDataTable("Select KPISubCategoryId as Id,KPISubCategoryFName as Name,Convert (int,CmpId) as CmpId,Convert (int,KPISubCategoryBranchId) as BranchId  from KPI_SubCategory  where KPI_SubCategory.Deactivate=0 ");
                dt_Contractor = objcon.GetDataTable("Select Mas_ContractorMapping.ContractorID as Id,Contractor_Master.ContractorName as Name,Convert (int,Mas_ContractorMapping.BranchID) as BranchID,Convert (int,Mas_ContractorMapping.CmpID) as CmpID  from Mas_ContractorMapping,Contractor_Master where Mas_ContractorMapping.IsActive=1 and Mas_ContractorMapping.ContractorID=Contractor_Master.ContractorId and Contractor_Master.Deactivate=0 order by ContractorName");
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

                dt_ValidationCheck = objcon.GetDataTable("Select Mas_Employee.EmployeeName,Mas_Employee.EmployeeNo,Mas_Employee.EmployeeCardNo,Mas_Employee_Personal.AadhaarNo   ,Convert (int,Mas_Employee.EmployeeId) As EmployeeId,Mas_Branch.BranchName,Convert (int,Mas_Employee.EmployeeBranchId) AS  EmployeeBranchId ,Mas_Employee_Statutory.PF_UAN from Mas_Employee  inner join Mas_Employee_Personal on Mas_Employee.employeeid=Mas_Employee_Personal.personalEmployeeid  inner join Mas_Branch on Mas_Employee.EmployeeBranchId=Mas_Branch.BranchId inner join Mas_Employee_Statutory on Mas_Employee_Statutory.StatutoryEmployeeId=Mas_Employee.EmployeeId where Mas_Employee.Deactivate=0 and Mas_Employee_Personal.Deactivate=0 and Mas_Employee_Statutory.Deactivate=0");


                StringBuilder strBuilder = new StringBuilder();
                int i = 0;

                //string str_aadhar = "";
                //string str_EmployeeNo = "";
                //string str_EmployeeCardNo = "";
                //foreach (var Data in tbldata)
                //{
                //    str_aadhar = str_aadhar + "'" + Data.AadhaarNo + "',";
                //    str_EmployeeNo = str_EmployeeNo + "'" + Data.EmployeeNo + "',";
                //    str_EmployeeCardNo = str_EmployeeCardNo + "'" + Data.EmployeeCardNo + "',";
                //}

                //if (str_aadhar != "")
                //{
                //    str_aadhar = str_aadhar.Substring(0, str_aadhar.Length - 1);
                //}

                //if (str_EmployeeNo != "")
                //{
                //    str_EmployeeNo = str_EmployeeNo.Substring(0, str_EmployeeNo.Length - 1);
                //}

                //if (str_EmployeeCardNo != "")
                //{
                //    str_EmployeeCardNo = str_EmployeeCardNo.Substring(0, str_EmployeeCardNo.Length - 1);
                //}

                //var ListEmployeeNo = DapperORM.DynamicQuerySingle("Select CompanyName as Company,BranchName as BU, EmployeeName as Employee,DepartmentName,EmployeeNo,EmployeeCardNo from Mas_Employee,Mas_CompanyProfile,Mas_Branch,Mas_Department  where  Mas_Employee.Deactivate=0  and Mas_CompanyProfile.Deactivate=0 and Mas_Branch.Deactivate=0 and  Mas_Department.Deactivate=0 and Mas_Employee.CmpID=Mas_CompanyProfile.CompanyId and Mas_Employee.EmployeeBranchId=BranchId and Mas_Employee.EmployeeDepartmentID=DepartmentId and EmployeeNo in (" + str_EmployeeNo + ")").ToList();
                //if (ListEmployeeNo.Count != 0)
                //{
                //    ViewBag.GetListEmployeeNo = ListEmployeeNo;
                //    return Json(new { ViewBag.GetListEmployeeNo }, JsonRequestBehavior.AllowGet);
                //}
                //var ListEmployeeCardNo = DapperORM.DynamicQuerySingle("Select CompanyName as Company,BranchName as BU, EmployeeName as Employee,DepartmentName,EmployeeNo,EmployeeCardNo from Mas_Employee,Mas_CompanyProfile,Mas_Branch,Mas_Department  where  Mas_Employee.Deactivate=0  and Mas_CompanyProfile.Deactivate=0 and Mas_Branch.Deactivate=0 and  Mas_Department.Deactivate=0 and Mas_Employee.CmpID=Mas_CompanyProfile.CompanyId and Mas_Employee.EmployeeBranchId=BranchId and Mas_Employee.EmployeeDepartmentID=DepartmentId and EmployeeCardNo in (" + str_EmployeeCardNo + ")").ToList();
                //if (ListEmployeeCardNo.Count != 0)
                //{
                //    ViewBag.GetListEmployeeCardNo = ListEmployeeCardNo;
                //    return Json(new { ViewBag.GetListEmployeeCardNo }, JsonRequestBehavior.AllowGet);
                //}

                //var ListEmployeeAadharNo = DapperORM.DynamicQuerySingle("Select CompanyName as Company,BranchName as BU, EmployeeName as Employee,DepartmentName,EmployeeNo,EmployeeCardNo, Mas_Employee_Personal.AadhaarNo as  AadhaarNo from Mas_Employee,Mas_CompanyProfile,Mas_Branch,Mas_Department,Mas_Employee_Personal  where Mas_Employee.Employeeleft=0 and   Mas_Employee.Deactivate=0 and  Mas_Employee_Personal.Deactivate=0  and Mas_CompanyProfile.Deactivate=0 and Mas_Branch.Deactivate=0 and  Mas_Department.Deactivate=0 and Mas_Employee.CmpID=Mas_CompanyProfile.CompanyId and Mas_Employee.EmployeeBranchId=BranchId and Mas_Employee.EmployeeDepartmentID=DepartmentId and Mas_Employee.employeeId=Mas_Employee_Personal.PersonalEmployeeId and Mas_Employee_Personal.AadhaarNo in (" + str_aadhar + ")").ToList();
                //if (ListEmployeeAadharNo.Count != 0)
                //{
                //    ViewBag.GetListEmployeeAadharNo = ListEmployeeAadharNo;
                //    return Json(new { ViewBag.GetListEmployeeAadharNo }, JsonRequestBehavior.AllowGet);
                //}
                if (Type == "Aadhar")
                {
                    foreach (var Data in tbldata)
                    {
                        List_Employee_Id = 0;
                        Employee_Id = 0;
                        List_EmployeeName = "";

                        if (Data.EmployeeNo == null)
                        {
                            var Message = "Enter Employee no in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetEmployeeId_Id(Data.EmployeeNo.ToString(), BranchId, out Employee_Id);
                        List_Employee_Id = Employee_Id;
                        if (Employee_Id == 0)
                        {
                            var Message = "The employee does not belong to this business unit (SrNo). " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        ////EmployeeNo   check  end ----------------------------------------------------------------------------------------
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

                        if (Data.AadhaarNo != null)
                        {
                            GetEmployeeAdhaarNo_Id(Data.AadhaarNo.ToString(), List_Employee_Id, out EmployeeName, out EmployeeNo, out BranchName);
                            List_EmployeeName = EmployeeName;
                            if (EmployeeName != "")
                            {
                                var Message = "This Aadhar has already been assigned to (" + EmployeeName + "  -  " + EmployeeNo + ") from the business unit  (" + BranchName + ") in SrNo " + Data.SrNo;
                                var Icon = "error";
                                var rowno = Data.SrNo;
                                return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                            }
                        }

                        ////AadhaarNo   check  end ----------------------------------------------------------------------------------------

                        string Employee_Admin = "if (exists(select PersonalEmployeeId from  Mas_Employee_Personal where PersonalEmployeeId = " + List_Employee_Id + "  and Mas_Employee_Personal.Deactivate = 0 )) " +
                                                  " Begin  " +
                                                  "   Insert into Log_Mas_Employee_Personal(PersonalId, PersonalId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, PersonalEmployeeId, AadhaarNo, NameAsPerAadhaar, AadhaarNoMobileNoLink, AadhaarNoMobileNo, PAN, NameAsPerPan, PANAadhaarLink, PrimaryMobile, SecondaryMobile, WhatsAppNo, PersonalEmailId, BirthdayDate, AgeOfJoining, BirthdayPlace, BirthdayProofOfDocumentID, BirthdayProofOfCertificateNo, IsDOBSpecial, EmployeeQualificationID, QualificationRemark, Gender, BloodGroup, MaritalStatus, AnniversaryDate, Ifyouwantdonotdisclosemygenderthentick, PhysicallyDisabled, PhysicallyDisableType, PhysicallyDisableRemark, IdentificationMark, DrivingLicenceNo, DrivingLicenceExpiryDate, PassportNo, PassportExpiryDate, EmployeeReligionID, Ifyouwantdonotdisclosemyreligioncastthentick, EmployeeCasteID, EmployeeSpecificDegree, EmployeeBirthProofEducation, EmployeeSubCategory)    " +
                                                  "   Select PersonalId, PersonalId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, PersonalEmployeeId, AadhaarNo, NameAsPerAadhaar, AadhaarNoMobileNoLink, AadhaarNoMobileNo, PAN, NameAsPerPan, PANAadhaarLink, PrimaryMobile, SecondaryMobile, WhatsAppNo, PersonalEmailId, BirthdayDate, AgeOfJoining, BirthdayPlace, BirthdayProofOfDocumentID, BirthdayProofOfCertificateNo, IsDOBSpecial, EmployeeQualificationID, QualificationRemark, Gender, BloodGroup, MaritalStatus, AnniversaryDate, Ifyouwantdonotdisclosemygenderthentick, PhysicallyDisabled, PhysicallyDisableType, PhysicallyDisableRemark, IdentificationMark, DrivingLicenceNo, DrivingLicenceExpiryDate, PassportNo, PassportExpiryDate, EmployeeReligionID, Ifyouwantdonotdisclosemyreligioncastthentick, EmployeeCasteID, EmployeeSpecificDegree, EmployeeBirthProofEducation, EmployeeSubCategory from Mas_Employee_Personal where Mas_Employee_Personal.Deactivate = 0 and Mas_Employee_Personal.PersonalEmployeeId = " + List_Employee_Id + "     " +

                                                   //" Update Mas_Employee Set EmployeeLeft=1 ,LeavingDate=GETDATE(),LeftEmployeeApprovedBy='" + Session["EmployeeName"] + "' where EmployeeId= @EmployeeId  " +
                                                   " end  " +
                                                   "   " +
                                                    "Update Mas_Employee_Personal set  MachineName='" + Dns.GetHostName().ToString() + "'," +
                                                                                                    "ModifiedBy='" + Session["EmployeeName"] + "'," +
                                                                                                    "ModifiedDate=GetDate()," +
                                                                                                    "AadhaarNo  ='" + Data.AadhaarNo + "'," +
                                                                                                    "NameAsPerAadhaar='" + Data.NameAsPerAadhaar + "' " +
                                                                                                    "where PersonalEmployeeId=" + List_Employee_Id + "" +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                     " " +
                                                                                                    " ";
                        strBuilder.Append(Employee_Admin);

                        i = i + 1;
                    }

                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record Update successfully";
                        TempData["Icon"] = "success";
                    }
                    if (abc != "")
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                }

                if (Type == "Address")
                {
                    foreach (var Data in tbldata)
                    {
                        List_Employee_Id = 0;
                        EmployeeNo_Id = 0;

                        if (Data.EmployeeNo == null)
                        {
                            var Message = "Enter Employee no in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetEmployeeId_Id(Data.EmployeeNo.ToString(), BranchId, out Employee_Id);
                        List_Employee_Id = Employee_Id;
                        if (Employee_Id == 0)
                        {
                            var Message = "The employee does not belong to this business unit (SrNo). " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }


                        //EmployeeCardNo   check  end ----------------------------------------------------------------------------------------
                        if (Data.PresentPin == null)
                        {
                            var Message = "Enter Pinocode no  in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }
                        if (!Regex.IsMatch(Data.PresentPin, Pincodepattern))
                        {
                            var Message = "Enter Valid Pincode in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        if (i == 0)
                        {

                            strBuilder.Append("DECLARE @IdentityValue_AddressEmployeeID AS TABLE(ID INT); ");

                            strBuilder.Append("DECLARE @IdentityValue_StatutoryEmployeeID AS TABLE(ID INT); ");

                            strBuilder.Append("DECLARE @EmployeeId  int ");
                        }

                        ////AadhaarNo   check  end ----------------------------------------------------------------------------------------

                        string Employee_Admin = "if (exists(select AddressEmployeeId from  Mas_Employee_Address where AddressEmployeeId = " + List_Employee_Id + "  and Mas_Employee_Address.Deactivate = 0 )) " +
                                                  " Begin " +
                                                  "   Insert into Log_Mas_Employee_Personal(PersonalId, PersonalId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, PersonalEmployeeId, AadhaarNo, NameAsPerAadhaar, AadhaarNoMobileNoLink, AadhaarNoMobileNo, PAN, NameAsPerPan, PANAadhaarLink, PrimaryMobile, SecondaryMobile, WhatsAppNo, PersonalEmailId, BirthdayDate, AgeOfJoining, BirthdayPlace, BirthdayProofOfDocumentID, BirthdayProofOfCertificateNo, IsDOBSpecial, EmployeeQualificationID, QualificationRemark, Gender, BloodGroup, MaritalStatus, AnniversaryDate, Ifyouwantdonotdisclosemygenderthentick, PhysicallyDisabled, PhysicallyDisableType, PhysicallyDisableRemark, IdentificationMark, DrivingLicenceNo, DrivingLicenceExpiryDate, PassportNo, PassportExpiryDate, EmployeeReligionID, Ifyouwantdonotdisclosemyreligioncastthentick, EmployeeCasteID, EmployeeSpecificDegree, EmployeeBirthProofEducation, EmployeeSubCategory)    " +
                                                  "   Select PersonalId, PersonalId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, PersonalEmployeeId, AadhaarNo, NameAsPerAadhaar, AadhaarNoMobileNoLink, AadhaarNoMobileNo, PAN, NameAsPerPan, PANAadhaarLink, PrimaryMobile, SecondaryMobile, WhatsAppNo, PersonalEmailId, BirthdayDate, AgeOfJoining, BirthdayPlace, BirthdayProofOfDocumentID, BirthdayProofOfCertificateNo, IsDOBSpecial, EmployeeQualificationID, QualificationRemark, Gender, BloodGroup, MaritalStatus, AnniversaryDate, Ifyouwantdonotdisclosemygenderthentick, PhysicallyDisabled, PhysicallyDisableType, PhysicallyDisableRemark, IdentificationMark, DrivingLicenceNo, DrivingLicenceExpiryDate, PassportNo, PassportExpiryDate, EmployeeReligionID, Ifyouwantdonotdisclosemyreligioncastthentick, EmployeeCasteID, EmployeeSpecificDegree, EmployeeBirthProofEducation, EmployeeSubCategory from Mas_Employee_Personal where Mas_Employee_Personal.Deactivate = 0 and Mas_Employee_Personal.PersonalEmployeeId = " + List_Employee_Id + "     " +

                                                   //" Update Mas_Employee Set EmployeeLeft=1 ,LeavingDate=GETDATE(),LeftEmployeeApprovedBy='" + Session["EmployeeName"] + "' where EmployeeId= @EmployeeId  " +
                                                   " end  " +
                                                   "   " +

                                                  " if (exists(select AddressEmployeeId from Mas_Employee_Address where AddressEmployeeId = " + List_Employee_Id + "  and Mas_Employee_Address.Deactivate = 0 )) " +
                                                  " Begin " +
                                                   "Update Mas_Employee_Address set  MachineName='" + Dns.GetHostName().ToString() + "'," +
                                                                                                    " ModifiedBy='" + Session["EmployeeName"] + "'," +
                                                                                                    "ModifiedDate=GetDate()," +
                                                                                                    " PresentPin = '" + Data.PresentPin + "'," +
                                                                                                    " PresentState = '" + Data.PresentState + "'," +
                                                                                                    " PresentDistrict = '" + Data.PresentDistrict + "'," +
                                                                                                    " PresentTaluka = '" + Data.PresentTaluka + "'," +
                                                                                                    " PresentPO = '" + Data.PresentPO + "'," +
                                                                                                    " PresentCity = '" + Data.PresentCity + "'," +
                                                                                                    " PresentPostelAddress = '" + Data.PresentPostelAddress + "'" +
                                                                                                    " where AddressEmployeeId =" + List_Employee_Id + "" +
                                                                                                    " " +
                                                                                                     " " +
                                                                                                    " " +
                                                   " end  " +
                                                  "   " +
                                                   "   " +
                                                  " ELSE  " +
                                                  " Begin " +
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
                                                    "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                                    "" + List_Employee_Id + "," +
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
                                                   " end  " +
                                                    " " +
                                                    " " +
                                                    " ";



                        strBuilder.Append(Employee_Admin);
                        strBuilder.Append(" DELETE FROM @IdentityValue_AddressEmployeeID     ;");

                        i = i + 1;
                    }

                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record Update successfully";
                        TempData["Icon"] = "success";
                    }
                    if (abc != "")
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                }

                if (Type == "UAN")
                {
                    foreach (var Data in tbldata)
                    {
                        List_Employee_Id = 0;
                        EmployeeNo_Id = 0;

                        if (Data.EmployeeNo == null)
                        {
                            var Message = "Enter Employee no in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetEmployeeId_Id(Data.EmployeeNo.ToString(), BranchId, out Employee_Id);
                        List_Employee_Id = Employee_Id;
                        if (Employee_Id == 0)
                        {
                            var Message = "The employee does not belong to this business unit (SrNo). " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        if (Data.PF_UAN != null)
                        {
                            GetEmployeeUAN_Id(Data.PF_UAN.ToString(), List_Employee_Id, out EmployeeName, out EmployeeNo, out BranchName);
                            List_EmployeeName = EmployeeName;
                            if (EmployeeName != "")
                            {
                                var Message = "This Uan no has already been assigned to (" + EmployeeName + "  -  " + EmployeeNo + ") from the business unit  (" + BranchName + ") in SrNo " + Data.SrNo;
                                var Icon = "error";
                                var rowno = Data.SrNo;
                                return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                            }
                        }

                        ////EmployeeNo   check  end ----------------------------------------------------------------------------------------
                        if (Data.PF_UAN == null)
                        {
                            var Message = "Enter UAN no  in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }
                        if (!Regex.IsMatch(Data.PF_UAN, AdharNopattern))
                        {
                            var Message = "Enter Valid UAN in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        if (Data.ESIC_NO != null)
                        {
                            if (!Regex.IsMatch(Data.ESIC_NO, ESICPattern))
                            {
                                var Message = "Enter Valid ESIC No in SrNo " + Data.SrNo;
                                var Icon = "error";
                                var rowno = Data.SrNo;
                                return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                            }
                        }

                        ////AadhaarNo   check  end ----------------------------------------------------------------------------------------

                        string Employee_Admin = "if (exists(select StatutoryEmployeeId from  Mas_Employee_Statutory where StatutoryEmployeeId = " + List_Employee_Id + "  and Mas_Employee_Statutory.Deactivate = 0 )) " +
                                                  " Begin " +
                                                  "   Insert into Log_Mas_Employee_Personal(PersonalId, PersonalId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, PersonalEmployeeId, AadhaarNo, NameAsPerAadhaar, AadhaarNoMobileNoLink, AadhaarNoMobileNo, PAN, NameAsPerPan, PANAadhaarLink, PrimaryMobile, SecondaryMobile, WhatsAppNo, PersonalEmailId, BirthdayDate, AgeOfJoining, BirthdayPlace, BirthdayProofOfDocumentID, BirthdayProofOfCertificateNo, IsDOBSpecial, EmployeeQualificationID, QualificationRemark, Gender, BloodGroup, MaritalStatus, AnniversaryDate, Ifyouwantdonotdisclosemygenderthentick, PhysicallyDisabled, PhysicallyDisableType, PhysicallyDisableRemark, IdentificationMark, DrivingLicenceNo, DrivingLicenceExpiryDate, PassportNo, PassportExpiryDate, EmployeeReligionID, Ifyouwantdonotdisclosemyreligioncastthentick, EmployeeCasteID, EmployeeSpecificDegree, EmployeeBirthProofEducation, EmployeeSubCategory)    " +
                                                  "   Select PersonalId, PersonalId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, PersonalEmployeeId, AadhaarNo, NameAsPerAadhaar, AadhaarNoMobileNoLink, AadhaarNoMobileNo, PAN, NameAsPerPan, PANAadhaarLink, PrimaryMobile, SecondaryMobile, WhatsAppNo, PersonalEmailId, BirthdayDate, AgeOfJoining, BirthdayPlace, BirthdayProofOfDocumentID, BirthdayProofOfCertificateNo, IsDOBSpecial, EmployeeQualificationID, QualificationRemark, Gender, BloodGroup, MaritalStatus, AnniversaryDate, Ifyouwantdonotdisclosemygenderthentick, PhysicallyDisabled, PhysicallyDisableType, PhysicallyDisableRemark, IdentificationMark, DrivingLicenceNo, DrivingLicenceExpiryDate, PassportNo, PassportExpiryDate, EmployeeReligionID, Ifyouwantdonotdisclosemyreligioncastthentick, EmployeeCasteID, EmployeeSpecificDegree, EmployeeBirthProofEducation, EmployeeSubCategory from Mas_Employee_Personal where Mas_Employee_Personal.Deactivate = 0 and Mas_Employee_Personal.PersonalEmployeeId = " + List_Employee_Id + "     " +

                                                   //" Update Mas_Employee Set EmployeeLeft=1 ,LeavingDate=GETDATE(),LeftEmployeeApprovedBy='" + Session["EmployeeName"] + "' where EmployeeId= @EmployeeId  " +
                                                   " end  " +
                                                   "   " +
                                                   "Update Mas_Employee_Statutory set  MachineName='" + Dns.GetHostName().ToString() + "'," +
                                                                                                    " ModifiedBy='" + Session["EmployeeName"] + "'," +
                                                                                                    "ModifiedDate=GetDate()," +
                                                                                                    " PF_UAN = '" + Data.PF_UAN + "'," +
                                                                                                    " ESIC_NO = '" + Data.ESIC_NO + "'," +
                                                                                                    " PF_FSType = '" + Data.PF_FSType + "'," +
                                                                                                    " PF_FS_Name = '" + Data.PF_FS_Name + "'" +
                                                                                                    " where StatutoryEmployeeId =" + List_Employee_Id + "" +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " ";

                        strBuilder.Append(Employee_Admin);
                        i = i + 1;
                    }

                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record Update successfully";
                        TempData["Icon"] = "success";
                    }
                    if (abc != "")
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                }

                if (Type == "Critical Stage Category")
                {
                    foreach (var Data in tbldata)
                    {

                        List_CriticalStage_Id = 0;
                        List_Employee_Id = 0;
                        CriticalStage_Id = 0;
                        GetIsCriticalStageApplicable = false;
                        EmployeeNo_Id = 0;

                        if (Data.EmployeeNo == null)
                        {
                            var Message = "Enter Employee no in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetEmployeeId_Id(Data.EmployeeNo.ToString(), BranchId, out Employee_Id);
                        List_Employee_Id = Employee_Id;
                        if (Employee_Id == 0)
                        {
                            var Message = "The employee does not belong to this business unit (SrNo). " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        ////EmployeeNo   check  end ----------------------------------------------------------------------------------------

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

                        string Employee_Admin = "if (exists(select EmployeeId from  Mas_Employee where EmployeeId = " + List_Employee_Id + "  and Mas_Employee.Deactivate = 0 and EmployeeLeft=0 )) " +
                                                  " Begin " +
                                                  "   Insert into Log_Mas_Employee  (EmployeeId, EmployeeId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, CmpID, PreboardingFid, EmployeeBranchId, EmployeeOrigin, EmployeeSeries, EmployeeNo, EmployeeCardNo, LocalExpat, IsNRI, Salutation, EmployeeName, EmployeeLevelID, EmployeeWageID, EmployeeDepartmentID, EmployeeSubDepartmentName, EmployeeDesignationID, EmployeeGradeID, EmployeeGroupID, EmployeeTypeID, EmployeeCostCenterID, EmployeeZoneID, EmployeeUnitID, ContractorID, IsCriticalStageApplicable, EmployeeCriticalStageID, EmployeeLineID, JoiningDate, IsJoiningSpecial, JoiningStatus, TraineeDueDate, ProbationDueDate, ConfirmationDate, IsConfirmation, ConfirmationBy, CompanyMobileNo, CompanyMailID, ReportingHR, ReportingManager1, ReportingManager2, ReportingAccount, NoOfBranchTransfer, IsReasigned, ResignationDate, NoticePeriodDays, IsExit, ExitDate, EmployeeLeft, LeavingDate, LeavingReason, LeavingReasonPF, LeavingReasonESI, EM_PayrollLastDate, VMSApplicable, IsVMSMultipleLocation, LeftEmployeeApprovedBy)    " +
                                                  "   Select EmployeeId, EmployeeId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, CmpID, PreboardingFid, EmployeeBranchId, EmployeeOrigin, EmployeeSeries, EmployeeNo, EmployeeCardNo, LocalExpat, IsNRI, Salutation, EmployeeName, EmployeeLevelID, EmployeeWageID, EmployeeDepartmentID, EmployeeSubDepartmentName, EmployeeDesignationID, EmployeeGradeID, EmployeeGroupID, EmployeeTypeID, EmployeeCostCenterID, EmployeeZoneID, EmployeeUnitID, ContractorID, IsCriticalStageApplicable, EmployeeCriticalStageID, EmployeeLineID, JoiningDate, IsJoiningSpecial, JoiningStatus, TraineeDueDate, ProbationDueDate, ConfirmationDate, IsConfirmation, ConfirmationBy, CompanyMobileNo, CompanyMailID, ReportingHR, ReportingManager1, ReportingManager2, ReportingAccount, NoOfBranchTransfer, IsReasigned, ResignationDate, NoticePeriodDays, IsExit, ExitDate, EmployeeLeft, LeavingDate, LeavingReason, LeavingReasonPF, LeavingReasonESI, EM_PayrollLastDate, VMSApplicable, IsVMSMultipleLocation, LeftEmployeeApprovedBy from Mas_Employee where Mas_Employee.Deactivate = 0 and Mas_Employee.EmployeeId = " + List_Employee_Id + "    " +
                                                   " end  " +
                                                   "   " +
                                                   "Update Mas_Employee set  MachineName='" + Dns.GetHostName().ToString() + "'," +
                                                                                                    " ModifiedBy='" + Session["EmployeeName"] + "'," +
                                                                                                    "ModifiedDate=GetDate()," +
                                                                                                    " IsCriticalStageApplicable = '" + GetIsCriticalStageApplicable + "'," +
                                                                                                    " EmployeeCriticalStageID = '" + List_CriticalStage_Id + "'" +
                                                                                                    " where EmployeeId =" + List_Employee_Id + "" +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " ";
                        strBuilder.Append(Employee_Admin);
                        i = i + 1;
                    }

                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record Update successfully";
                        TempData["Icon"] = "success";
                    }
                    if (abc != "")
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                }
                if (Type == "Department")
                {
                    foreach (var Data in tbldata)
                    {
                        List_Department_Id = 0;
                        List_SubDepatment_Id = 0;
                        List_Employee_Id = 0;
                        Department_Id = 0;
                        SubDepatment_Id = 0;
                        EmployeeNo_Id = 0;

                        if (Data.EmployeeNo == null)
                        {
                            var Message = "Enter Employee no in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetEmployeeId_Id(Data.EmployeeNo.ToString(), BranchId, out Employee_Id);
                        List_Employee_Id = Employee_Id;
                        if (Employee_Id == 0)
                        {
                            var Message = "The employee does not belong to this business unit (SrNo). " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        ////EmployeeNo   check  end ----------------------------------------------------------------------------------------

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


                        string Employee_Admin = "if (exists(select EmployeeId from  Mas_Employee where EmployeeId = " + List_Employee_Id + "  and Mas_Employee.Deactivate = 0 and EmployeeLeft=0 )) " +
                                                  " Begin " +
                                                  "   Insert into Log_Mas_Employee  (EmployeeId, EmployeeId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, CmpID, PreboardingFid, EmployeeBranchId, EmployeeOrigin, EmployeeSeries, EmployeeNo, EmployeeCardNo, LocalExpat, IsNRI, Salutation, EmployeeName, EmployeeLevelID, EmployeeWageID, EmployeeDepartmentID, EmployeeSubDepartmentName, EmployeeDesignationID, EmployeeGradeID, EmployeeGroupID, EmployeeTypeID, EmployeeCostCenterID, EmployeeZoneID, EmployeeUnitID, ContractorID, IsCriticalStageApplicable, EmployeeCriticalStageID, EmployeeLineID, JoiningDate, IsJoiningSpecial, JoiningStatus, TraineeDueDate, ProbationDueDate, ConfirmationDate, IsConfirmation, ConfirmationBy, CompanyMobileNo, CompanyMailID, ReportingHR, ReportingManager1, ReportingManager2, ReportingAccount, NoOfBranchTransfer, IsReasigned, ResignationDate, NoticePeriodDays, IsExit, ExitDate, EmployeeLeft, LeavingDate, LeavingReason, LeavingReasonPF, LeavingReasonESI, EM_PayrollLastDate, VMSApplicable, IsVMSMultipleLocation, LeftEmployeeApprovedBy)    " +
                                                  "   Select EmployeeId, EmployeeId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, CmpID, PreboardingFid, EmployeeBranchId, EmployeeOrigin, EmployeeSeries, EmployeeNo, EmployeeCardNo, LocalExpat, IsNRI, Salutation, EmployeeName, EmployeeLevelID, EmployeeWageID, EmployeeDepartmentID, EmployeeSubDepartmentName, EmployeeDesignationID, EmployeeGradeID, EmployeeGroupID, EmployeeTypeID, EmployeeCostCenterID, EmployeeZoneID, EmployeeUnitID, ContractorID, IsCriticalStageApplicable, EmployeeCriticalStageID, EmployeeLineID, JoiningDate, IsJoiningSpecial, JoiningStatus, TraineeDueDate, ProbationDueDate, ConfirmationDate, IsConfirmation, ConfirmationBy, CompanyMobileNo, CompanyMailID, ReportingHR, ReportingManager1, ReportingManager2, ReportingAccount, NoOfBranchTransfer, IsReasigned, ResignationDate, NoticePeriodDays, IsExit, ExitDate, EmployeeLeft, LeavingDate, LeavingReason, LeavingReasonPF, LeavingReasonESI, EM_PayrollLastDate, VMSApplicable, IsVMSMultipleLocation, LeftEmployeeApprovedBy from Mas_Employee where Mas_Employee.Deactivate = 0 and Mas_Employee.EmployeeId = " + List_Employee_Id + "    " +

                                                   //" Update Mas_Employee Set EmployeeLeft=1 ,LeavingDate=GETDATE(),LeftEmployeeApprovedBy='" + Session["EmployeeName"] + "' where EmployeeId= @EmployeeId  " +
                                                   " end  " +
                                                   "   " +
                                                   "Update Mas_Employee set  MachineName='" + Dns.GetHostName().ToString() + "'," +
                                                                                                    " ModifiedBy='" + Session["EmployeeName"] + "'," +
                                                                                                    " ModifiedDate=GetDate()," +
                                                                                                    " EmployeeDepartmentID = '" + List_Department_Id + "'," +
                                                                                                    " EmployeeSubDepartmentName = '" + List_SubDepatment_Id + "'" +
                                                                                                    " where EmployeeId =" + List_Employee_Id + "" +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " ";
                        strBuilder.Append(Employee_Admin);
                        i = i + 1;
                    }

                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record Update successfully";
                        TempData["Icon"] = "success";
                    }
                    if (abc != "")
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                }

                if (Type == "Attendance")
                {
                    foreach (var Data in tbldata)
                    {

                        List_ShiftGroup_Id = 0;
                        List_ShiftRule_Id = 0;
                        List_Employee_Id = 0;
                        GetPHApplicable = false;

                        ShiftGroup_Id = 0;
                        ShiftRule_Id = 0;
                        EmployeeNo_Id = 0;

                        if (Data.EmployeeNo == null)
                        {
                            var Message = "Enter Employee no in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetEmployeeId_Id(Data.EmployeeNo.ToString(), BranchId, out Employee_Id);
                        List_Employee_Id = Employee_Id;
                        if (Employee_Id == 0)
                        {
                            var Message = "The employee does not belong to this business unit (SrNo). " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        ////EmployeeNo   check  end ----------------------------------------------------------------------------------------


                        if (Data.EM_Atten_ShiftGroupId == null)
                        {
                            var Message = "Enter shift group in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetShiftGroup_Id(Data.EM_Atten_ShiftGroupId.ToString(), CmpId, BranchId, out ShiftGroup_Id);
                        List_ShiftGroup_Id = ShiftGroup_Id;
                        if (List_ShiftGroup_Id == 0)
                        {
                            var Message = "Shift group not valid in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }
                        ////Employee Shift Group check  end ----------------------------------------------------------------------------------------

                        if (Data.EM_Atten_ShiftRuleId == null)
                        {
                            var Message = "Enter shift rule in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetShiftRule_Id(Data.EM_Atten_ShiftRuleId.ToString(), CmpId, BranchId, out ShiftRule_Id);
                        List_ShiftRule_Id = ShiftRule_Id;
                        if (List_ShiftRule_Id == 0)
                        {
                            var Message = "Shift rule not valid in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }
                        ////Employee Rule  check  end ----------------------------------------------------------------------------------------
                        if (Data.EM_Atten_WOFF1 == null)
                        {
                            var Message = "Enter weekly off in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }
                        else if (Data.EM_Atten_WOFF1 == "Sunday" || Data.EM_Atten_WOFF1 == "Monday" || Data.EM_Atten_WOFF1 == "Tuesday" || Data.EM_Atten_WOFF1 == "Wednesday" || Data.EM_Atten_WOFF1 == "Thursday" || Data.EM_Atten_WOFF1 == "Friday" || Data.EM_Atten_WOFF1 == "Saturday" || Data.EM_Atten_WOFF1 == "NA")
                        {

                        }
                        else
                        {
                            var Message = "Please enter proper weekly off in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        if (Data.PHApplicable == null)
                        {
                            var Message = "Enter PH Applicable in the Sr. No " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            if (Data.PHApplicable == "Yes")
                            {
                                GetPHApplicable = true;

                            }
                            else
                            {
                                GetPHApplicable = false;
                            }
                        }



                        ////Employee sub  Department  check  end ----------------------------------------------------------------------------------------


                        string Employee_Admin = "if (exists(select AttendanceEmployeeId from  Mas_Employee_Attendance where AttendanceEmployeeId = " + List_Employee_Id + "  and Mas_Employee_Attendance.Deactivate = 0)) " +
                                                  " Begin " +
                                                     "   Insert into Log_Mas_Employee_Attendance(AttendanceId, AttendanceId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, AttendanceEmployeeId, EmployeeCardNo, EM_Atten_OT_Applicable, EM_Atten_OTMultiplyBy, EM_Atten_PerDayShiftHrs, EM_Atten_CoffApplicable, EM_Atten_CoffSettingId, EM_Atten_ShiftGroupId, EM_Atten_ShiftRuleId, EM_Atten_WOFF1, EM_Atten_WOFF2, EM_Atten_WOFF2_ForCheck, EM_Atten_WOFF_Check1, EM_Atten_WOFF_Check2, EM_Atten_WOFF_Check3, EM_Atten_WOFF_Check4, EM_Atten_WOFF_Check5, EM_Atten_LateMarkSettingApplicable, EM_Atten_LateMarkSettingId, EM_Atten_IsSalaryFullPay, EM_Atten_LeaveGroupId, EM_Atten_DefaultAttenShow, EM_Atten_SinglePunch_Present, EM_Atten_RotationalWeekOff, EM_Atten_Regularization_Required, EM_Atten_ShortLeaveApplicable, EM_Atten_ShortLeaveSettingId, EM_Atten_PersonalGatepassApplicable, EM_Atten_Atten_PersonalGatepassSettingId, EM_Atten_AttendanceLastDate, EM_Atten_PunchMissingApplicable, EM_Atten_Atten_PunchMissingSettingId, EM_Atten_flexibleShiftApplicable, EM_Atten_Atten_OutDoorCompanySettingId, EM_Atten_OutDoorCompanyApplicable, EM_Atten_WOPH_CoffApplicable, PHApplicable)    " +
                                                "   Select AttendanceId, AttendanceId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, AttendanceEmployeeId, EmployeeCardNo, EM_Atten_OT_Applicable, EM_Atten_OTMultiplyBy, EM_Atten_PerDayShiftHrs, EM_Atten_CoffApplicable, EM_Atten_CoffSettingId, EM_Atten_ShiftGroupId, EM_Atten_ShiftRuleId, EM_Atten_WOFF1, EM_Atten_WOFF2, EM_Atten_WOFF2_ForCheck, EM_Atten_WOFF_Check1, EM_Atten_WOFF_Check2, EM_Atten_WOFF_Check3, EM_Atten_WOFF_Check4, EM_Atten_WOFF_Check5, EM_Atten_LateMarkSettingApplicable, EM_Atten_LateMarkSettingId, EM_Atten_IsSalaryFullPay, EM_Atten_LeaveGroupId, EM_Atten_DefaultAttenShow, EM_Atten_SinglePunch_Present, EM_Atten_RotationalWeekOff, EM_Atten_Regularization_Required, EM_Atten_ShortLeaveApplicable, EM_Atten_ShortLeaveSettingId, EM_Atten_PersonalGatepassApplicable, EM_Atten_Atten_PersonalGatepassSettingId, EM_Atten_AttendanceLastDate, EM_Atten_PunchMissingApplicable, EM_Atten_Atten_PunchMissingSettingId, EM_Atten_flexibleShiftApplicable, EM_Atten_Atten_OutDoorCompanySettingId, EM_Atten_OutDoorCompanyApplicable, EM_Atten_WOPH_CoffApplicable, PHApplicable from Mas_Employee_Attendance where Mas_Employee_Attendance.Deactivate = 0 and Mas_Employee_Attendance.AttendanceEmployeeId = " + List_Employee_Id + "   " +

                                                   //" Update Mas_Employee Set EmployeeLeft=1 ,LeavingDate=GETDATE(),LeftEmployeeApprovedBy='" + Session["EmployeeName"] + "' where EmployeeId= @EmployeeId  " +
                                                   " end  " +
                                                   "   " +
                                                   "Update Mas_Employee_Attendance set  MachineName='" + Dns.GetHostName().ToString() + "'," +
                                                                                                    " ModifiedBy='" + Session["EmployeeName"] + "'," +
                                                                                                    " ModifiedDate=GetDate()," +
                                                                                                    " EM_Atten_ShiftGroupId = '" + List_ShiftGroup_Id + "'," +
                                                                                                    " EM_Atten_ShiftRuleId = '" + List_ShiftRule_Id + "'," +
                                                                                                    " EM_Atten_WOFF1 = '" + Data.EM_Atten_WOFF1 + "'," +
                                                                                                    " PHApplicable = '" + GetPHApplicable + "'" +
                                                                                                    " where AttendanceEmployeeId =" + List_Employee_Id + "" +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " ";
                        strBuilder.Append(Employee_Admin);
                        i = i + 1;
                    }

                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record Update successfully";
                        TempData["Icon"] = "success";
                    }
                    if (abc != "")
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                }


                if (Type == "Manpower Category")
                {
                    foreach (var Data in tbldata)
                    {


                        List_Employee_Id = 0;
                        List_ManpowerCategory_Id = 0;
                        ManpowerCategory_Id = 0;
                        EmployeeNo_Id = 0;

                        if (Data.EmployeeNo == null)
                        {
                            var Message = "Enter Employee no in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetEmployeeId_Id(Data.EmployeeNo.ToString(), BranchId, out Employee_Id);
                        List_Employee_Id = Employee_Id;
                        if (Employee_Id == 0)
                        {
                            var Message = "The employee does not belong to this business unit (SrNo). " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        ////EmployeeNo   check  end ----------------------------------------------------------------------------------------


                        if (Data.EmployeeAllocationCategoryId == null)
                        {
                            var Message = "Enter manpower category name in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetManpowerCategory_Id(Data.EmployeeAllocationCategoryId.ToString(), CmpId, BranchId, out ManpowerCategory_Id);
                        List_ManpowerCategory_Id = ManpowerCategory_Id;
                        if (List_ManpowerCategory_Id == 0)
                        {
                            var Message = "Manpower category not valid in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }
                        ////Employee Man power  check  end ----------------------------------------------------------------------------------------


                        string Employee_Admin = "if (exists(select EmployeeId from  Mas_Employee where EmployeeId = " + List_Employee_Id + "  and Mas_Employee.Deactivate = 0)) " +
                                                  " Begin " +
                                                "   Insert into Log_Mas_Employee  (EmployeeId, EmployeeId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, CmpID, PreboardingFid, EmployeeBranchId, EmployeeOrigin, EmployeeSeries, EmployeeNo, EmployeeCardNo, LocalExpat, IsNRI, Salutation, EmployeeName, EmployeeLevelID, EmployeeWageID, EmployeeDepartmentID, EmployeeSubDepartmentName, EmployeeDesignationID, EmployeeGradeID, EmployeeGroupID, EmployeeTypeID, EmployeeCostCenterID, EmployeeZoneID, EmployeeUnitID, ContractorID, IsCriticalStageApplicable, EmployeeCriticalStageID, EmployeeLineID, JoiningDate, IsJoiningSpecial, JoiningStatus, TraineeDueDate, ProbationDueDate, ConfirmationDate, IsConfirmation, ConfirmationBy, CompanyMobileNo, CompanyMailID, ReportingHR, ReportingManager1, ReportingManager2, ReportingAccount, NoOfBranchTransfer, IsReasigned, ResignationDate, NoticePeriodDays, IsExit, ExitDate, EmployeeLeft, LeavingDate, LeavingReason, LeavingReasonPF, LeavingReasonESI, EM_PayrollLastDate, VMSApplicable, IsVMSMultipleLocation, LeftEmployeeApprovedBy)    " +
                                                "   Select EmployeeId, EmployeeId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, CmpID, PreboardingFid, EmployeeBranchId, EmployeeOrigin, EmployeeSeries, EmployeeNo, EmployeeCardNo, LocalExpat, IsNRI, Salutation, EmployeeName, EmployeeLevelID, EmployeeWageID, EmployeeDepartmentID, EmployeeSubDepartmentName, EmployeeDesignationID, EmployeeGradeID, EmployeeGroupID, EmployeeTypeID, EmployeeCostCenterID, EmployeeZoneID, EmployeeUnitID, ContractorID, IsCriticalStageApplicable, EmployeeCriticalStageID, EmployeeLineID, JoiningDate, IsJoiningSpecial, JoiningStatus, TraineeDueDate, ProbationDueDate, ConfirmationDate, IsConfirmation, ConfirmationBy, CompanyMobileNo, CompanyMailID, ReportingHR, ReportingManager1, ReportingManager2, ReportingAccount, NoOfBranchTransfer, IsReasigned, ResignationDate, NoticePeriodDays, IsExit, ExitDate, EmployeeLeft, LeavingDate, LeavingReason, LeavingReasonPF, LeavingReasonESI, EM_PayrollLastDate, VMSApplicable, IsVMSMultipleLocation, LeftEmployeeApprovedBy from Mas_Employee where Mas_Employee.Deactivate = 0 and Mas_Employee.EmployeeId = " + List_Employee_Id + "    " +

                                                   //" Update Mas_Employee Set EmployeeLeft=1 ,LeavingDate=GETDATE(),LeftEmployeeApprovedBy='" + Session["EmployeeName"] + "' where EmployeeId= @EmployeeId  " +
                                                   " end  " +
                                                   "   " +
                                                   "Update Mas_Employee set  MachineName='" + Dns.GetHostName().ToString() + "'," +
                                                                                                    " ModifiedBy='" + Session["EmployeeName"] + "'," +
                                                                                                    " ModifiedDate=GetDate()," +
                                                                                                    " EmployeeAllocationCategoryId = '" + List_ManpowerCategory_Id + "'" +
                                                                                                    " where EmployeeId =" + List_Employee_Id + "" +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " ";
                        strBuilder.Append(Employee_Admin);
                        i = i + 1;
                    }

                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record Update successfully";
                        TempData["Icon"] = "success";
                    }
                    if (abc != "")
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                }

                if (Type == "Line Master")
                {
                    foreach (var Data in tbldata)
                    {


                        List_Employee_Id = 0;
                        List_Line_Id = 0;
                        Line_Id = 0;
                        EmployeeNo_Id = 0;

                        if (Data.EmployeeNo == null)
                        {
                            var Message = "Enter Employee no in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetEmployeeId_Id(Data.EmployeeNo.ToString(), BranchId, out Employee_Id);
                        List_Employee_Id = Employee_Id;
                        if (Employee_Id == 0)
                        {
                            var Message = "The employee does not belong to this business unit (SrNo). " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        ////EmployeeNo   check  end ----------------------------------------------------------------------------------------


                        if (Data.EmployeeLineID == null)
                        {
                            var Message = "Enter line name in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetLine_Id(Data.EmployeeLineID.ToString(), CmpId, BranchId, out Line_Id);
                        List_Line_Id = Line_Id;
                        if (List_Line_Id == 0)
                        {
                            var Message = "Line name not valid in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }
                        ////Employee Line  check  end ----------------------------------------------------------------------------------------


                        string Employee_Admin = "if (exists(select EmployeeId from  Mas_Employee where EmployeeId = " + List_Employee_Id + "  and Mas_Employee.Deactivate = 0)) " +
                                                  " Begin " +
                                                "   Insert into Log_Mas_Employee  (EmployeeId, EmployeeId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, CmpID, PreboardingFid, EmployeeBranchId, EmployeeOrigin, EmployeeSeries, EmployeeNo, EmployeeCardNo, LocalExpat, IsNRI, Salutation, EmployeeName, EmployeeLevelID, EmployeeWageID, EmployeeDepartmentID, EmployeeSubDepartmentName, EmployeeDesignationID, EmployeeGradeID, EmployeeGroupID, EmployeeTypeID, EmployeeCostCenterID, EmployeeZoneID, EmployeeUnitID, ContractorID, IsCriticalStageApplicable, EmployeeCriticalStageID, EmployeeLineID, JoiningDate, IsJoiningSpecial, JoiningStatus, TraineeDueDate, ProbationDueDate, ConfirmationDate, IsConfirmation, ConfirmationBy, CompanyMobileNo, CompanyMailID, ReportingHR, ReportingManager1, ReportingManager2, ReportingAccount, NoOfBranchTransfer, IsReasigned, ResignationDate, NoticePeriodDays, IsExit, ExitDate, EmployeeLeft, LeavingDate, LeavingReason, LeavingReasonPF, LeavingReasonESI, EM_PayrollLastDate, VMSApplicable, IsVMSMultipleLocation, LeftEmployeeApprovedBy)    " +
                                                "   Select EmployeeId, EmployeeId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, CmpID, PreboardingFid, EmployeeBranchId, EmployeeOrigin, EmployeeSeries, EmployeeNo, EmployeeCardNo, LocalExpat, IsNRI, Salutation, EmployeeName, EmployeeLevelID, EmployeeWageID, EmployeeDepartmentID, EmployeeSubDepartmentName, EmployeeDesignationID, EmployeeGradeID, EmployeeGroupID, EmployeeTypeID, EmployeeCostCenterID, EmployeeZoneID, EmployeeUnitID, ContractorID, IsCriticalStageApplicable, EmployeeCriticalStageID, EmployeeLineID, JoiningDate, IsJoiningSpecial, JoiningStatus, TraineeDueDate, ProbationDueDate, ConfirmationDate, IsConfirmation, ConfirmationBy, CompanyMobileNo, CompanyMailID, ReportingHR, ReportingManager1, ReportingManager2, ReportingAccount, NoOfBranchTransfer, IsReasigned, ResignationDate, NoticePeriodDays, IsExit, ExitDate, EmployeeLeft, LeavingDate, LeavingReason, LeavingReasonPF, LeavingReasonESI, EM_PayrollLastDate, VMSApplicable, IsVMSMultipleLocation, LeftEmployeeApprovedBy from Mas_Employee where Mas_Employee.Deactivate = 0 and Mas_Employee.EmployeeId = " + List_Employee_Id + "    " +

                                                   //" Update Mas_Employee Set EmployeeLeft=1 ,LeavingDate=GETDATE(),LeftEmployeeApprovedBy='" + Session["EmployeeName"] + "' where EmployeeId= @EmployeeId  " +
                                                   " end  " +
                                                   "   " +
                                                   "Update Mas_Employee set  MachineName='" + Dns.GetHostName().ToString() + "'," +
                                                                                                    " ModifiedBy='" + Session["EmployeeName"] + "'," +
                                                                                                    " ModifiedDate=GetDate()," +
                                                                                                    " EmployeeLineID = '" + List_Line_Id + "'" +
                                                                                                    " where EmployeeId =" + List_Employee_Id + "" +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " ";
                        strBuilder.Append(Employee_Admin);
                        i = i + 1;
                    }

                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record Update successfully";
                        TempData["Icon"] = "success";
                    }
                    if (abc != "")
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                }

                if (Type == "Assessment Level")
                {
                    foreach (var Data in tbldata)
                    {


                        List_Employee_Id = 0;
                        List_AssessmentLevel_Id = 0;
                        AssessmentLevel_Id = 0;
                        EmployeeNo_Id = 0;

                        if (Data.EmployeeNo == null)
                        {
                            var Message = "Enter Employee no in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetEmployeeId_Id(Data.EmployeeNo.ToString(), BranchId, out Employee_Id);
                        List_Employee_Id = Employee_Id;
                        if (Employee_Id == 0)
                        {
                            var Message = "The employee does not belong to this business unit (SrNo). " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        ////EmployeeNo   check  end ----------------------------------------------------------------------------------------


                        if (Data.EmployeeLevelID == null)
                        {
                            var Message = "Enter assessment level in SrNo " + Data.SrNo;
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
                        ////Employee Line  check  end ----------------------------------------------------------------------------------------


                        string Employee_Admin = "if (exists(select EmployeeId from  Mas_Employee where EmployeeId = " + List_Employee_Id + "  and Mas_Employee.Deactivate = 0)) " +
                                                  " Begin " +
                                                "   Insert into Log_Mas_Employee  (EmployeeId, EmployeeId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, CmpID, PreboardingFid, EmployeeBranchId, EmployeeOrigin, EmployeeSeries, EmployeeNo, EmployeeCardNo, LocalExpat, IsNRI, Salutation, EmployeeName, EmployeeLevelID, EmployeeWageID, EmployeeDepartmentID, EmployeeSubDepartmentName, EmployeeDesignationID, EmployeeGradeID, EmployeeGroupID, EmployeeTypeID, EmployeeCostCenterID, EmployeeZoneID, EmployeeUnitID, ContractorID, IsCriticalStageApplicable, EmployeeCriticalStageID, EmployeeLineID, JoiningDate, IsJoiningSpecial, JoiningStatus, TraineeDueDate, ProbationDueDate, ConfirmationDate, IsConfirmation, ConfirmationBy, CompanyMobileNo, CompanyMailID, ReportingHR, ReportingManager1, ReportingManager2, ReportingAccount, NoOfBranchTransfer, IsReasigned, ResignationDate, NoticePeriodDays, IsExit, ExitDate, EmployeeLeft, LeavingDate, LeavingReason, LeavingReasonPF, LeavingReasonESI, EM_PayrollLastDate, VMSApplicable, IsVMSMultipleLocation, LeftEmployeeApprovedBy)    " +
                                                "   Select EmployeeId, EmployeeId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, CmpID, PreboardingFid, EmployeeBranchId, EmployeeOrigin, EmployeeSeries, EmployeeNo, EmployeeCardNo, LocalExpat, IsNRI, Salutation, EmployeeName, EmployeeLevelID, EmployeeWageID, EmployeeDepartmentID, EmployeeSubDepartmentName, EmployeeDesignationID, EmployeeGradeID, EmployeeGroupID, EmployeeTypeID, EmployeeCostCenterID, EmployeeZoneID, EmployeeUnitID, ContractorID, IsCriticalStageApplicable, EmployeeCriticalStageID, EmployeeLineID, JoiningDate, IsJoiningSpecial, JoiningStatus, TraineeDueDate, ProbationDueDate, ConfirmationDate, IsConfirmation, ConfirmationBy, CompanyMobileNo, CompanyMailID, ReportingHR, ReportingManager1, ReportingManager2, ReportingAccount, NoOfBranchTransfer, IsReasigned, ResignationDate, NoticePeriodDays, IsExit, ExitDate, EmployeeLeft, LeavingDate, LeavingReason, LeavingReasonPF, LeavingReasonESI, EM_PayrollLastDate, VMSApplicable, IsVMSMultipleLocation, LeftEmployeeApprovedBy from Mas_Employee where Mas_Employee.Deactivate = 0 and Mas_Employee.EmployeeId = " + List_Employee_Id + "    " +

                                                   //" Update Mas_Employee Set EmployeeLeft=1 ,LeavingDate=GETDATE(),LeftEmployeeApprovedBy='" + Session["EmployeeName"] + "' where EmployeeId= @EmployeeId  " +
                                                   " end  " +
                                                   "   " +
                                                   "Update Mas_Employee set  MachineName='" + Dns.GetHostName().ToString() + "'," +
                                                                                                    " ModifiedBy='" + Session["EmployeeName"] + "'," +
                                                                                                    " ModifiedDate=GetDate()," +
                                                                                                    " EmployeeLevelID = '" + List_AssessmentLevel_Id + "'" +
                                                                                                    " where EmployeeId =" + List_Employee_Id + "" +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " ";
                        strBuilder.Append(Employee_Admin);
                        i = i + 1;
                    }

                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record Update successfully";
                        TempData["Icon"] = "success";
                    }
                    if (abc != "")
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                }


                if (Type == "Grade")
                {
                    foreach (var Data in tbldata)
                    {
                        List_Employee_Id = 0;
                        List_Grade_Id = 0;

                        Grade_Id = 0;
                        EmployeeNo_Id = 0;

                        if (Data.EmployeeNo == null)
                        {
                            var Message = "Enter Employee no in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetEmployeeId_Id(Data.EmployeeNo.ToString(), BranchId, out Employee_Id);
                        List_Employee_Id = Employee_Id;
                        if (Employee_Id == 0)
                        {
                            var Message = "The employee does not belong to this business unit (SrNo). " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        ////EmployeeNo   check  end ----------------------------------------------------------------------------------------


                        if (Data.GradeName == null)
                        {
                            var Message = "Enter grade name in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetGrade_Id(Data.GradeName.ToString(), out Grade_Id);
                        List_Grade_Id = Grade_Id;
                        if (List_Grade_Id == 0)
                        {
                            var Message = "Grade name not valid in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }
                        ////Employee Line  check  end ----------------------------------------------------------------------------------------


                        string Employee_Admin = "if (exists(select EmployeeId from  Mas_Employee where EmployeeId = " + List_Employee_Id + "  and Mas_Employee.Deactivate = 0)) " +
                                                  " Begin " +
                                                "   Insert into Log_Mas_Employee  (EmployeeId, EmployeeId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, CmpID, PreboardingFid, EmployeeBranchId, EmployeeOrigin, EmployeeSeries, EmployeeNo, EmployeeCardNo, LocalExpat, IsNRI, Salutation, EmployeeName, EmployeeLevelID, EmployeeWageID, EmployeeDepartmentID, EmployeeSubDepartmentName, EmployeeDesignationID, EmployeeGradeID, EmployeeGroupID, EmployeeTypeID, EmployeeCostCenterID, EmployeeZoneID, EmployeeUnitID, ContractorID, IsCriticalStageApplicable, EmployeeCriticalStageID, EmployeeLineID, JoiningDate, IsJoiningSpecial, JoiningStatus, TraineeDueDate, ProbationDueDate, ConfirmationDate, IsConfirmation, ConfirmationBy, CompanyMobileNo, CompanyMailID, ReportingHR, ReportingManager1, ReportingManager2, ReportingAccount, NoOfBranchTransfer, IsReasigned, ResignationDate, NoticePeriodDays, IsExit, ExitDate, EmployeeLeft, LeavingDate, LeavingReason, LeavingReasonPF, LeavingReasonESI, EM_PayrollLastDate, VMSApplicable, IsVMSMultipleLocation, LeftEmployeeApprovedBy)    " +
                                                "   Select EmployeeId, EmployeeId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, CmpID, PreboardingFid, EmployeeBranchId, EmployeeOrigin, EmployeeSeries, EmployeeNo, EmployeeCardNo, LocalExpat, IsNRI, Salutation, EmployeeName, EmployeeLevelID, EmployeeWageID, EmployeeDepartmentID, EmployeeSubDepartmentName, EmployeeDesignationID, EmployeeGradeID, EmployeeGroupID, EmployeeTypeID, EmployeeCostCenterID, EmployeeZoneID, EmployeeUnitID, ContractorID, IsCriticalStageApplicable, EmployeeCriticalStageID, EmployeeLineID, JoiningDate, IsJoiningSpecial, JoiningStatus, TraineeDueDate, ProbationDueDate, ConfirmationDate, IsConfirmation, ConfirmationBy, CompanyMobileNo, CompanyMailID, ReportingHR, ReportingManager1, ReportingManager2, ReportingAccount, NoOfBranchTransfer, IsReasigned, ResignationDate, NoticePeriodDays, IsExit, ExitDate, EmployeeLeft, LeavingDate, LeavingReason, LeavingReasonPF, LeavingReasonESI, EM_PayrollLastDate, VMSApplicable, IsVMSMultipleLocation, LeftEmployeeApprovedBy from Mas_Employee where Mas_Employee.Deactivate = 0 and Mas_Employee.EmployeeId = " + List_Employee_Id + "    " +
                                                " end  " +
                                                "   " +
                                                "Update Mas_Employee set  MachineName='" + Dns.GetHostName().ToString() + "'," +
                                                                                                    " ModifiedBy='" + Session["EmployeeName"] + "'," +
                                                                                                    " ModifiedDate=GetDate()," +
                                                                                                    " EmployeeGradeID = '" + List_Grade_Id + "'" +
                                                                                                    " where EmployeeId =" + List_Employee_Id + "" +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " ";
                        strBuilder.Append(Employee_Admin);
                        i = i + 1;
                    }

                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record Update successfully";
                        TempData["Icon"] = "success";
                    }
                    if (abc != "")
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                }

                if (Type == "Designation")
                {
                    foreach (var Data in tbldata)
                    {


                        List_Employee_Id = 0;
                        List_Designation_Id = 0;

                        Designation_Id = 0;

                        EmployeeNo_Id = 0;

                        if (Data.EmployeeNo == null)
                        {
                            var Message = "Enter Employee no in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetEmployeeId_Id(Data.EmployeeNo.ToString(), BranchId, out Employee_Id);
                        List_Employee_Id = Employee_Id;
                        if (Employee_Id == 0)
                        {
                            var Message = "The employee does not belong to this business unit (SrNo). " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        ////EmployeeNo   check  end ----------------------------------------------------------------------------------------


                        if (Data.DesignationName == null)
                        {
                            var Message = "Enter designation name in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetDesignation_Id(Data.DesignationName.ToString(), out Designation_Id);
                        List_Designation_Id = Designation_Id;
                        if (List_Designation_Id == 0)
                        {
                            var Message = "Designation name not valid in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }
                        ////Employee Line  check  end ----------------------------------------------------------------------------------------


                        string Employee_Admin = "if (exists(select EmployeeId from  Mas_Employee where EmployeeId = " + List_Employee_Id + "  and Mas_Employee.Deactivate = 0)) " +
                                                  " Begin " +
                                                "   Insert into Log_Mas_Employee  (EmployeeId, EmployeeId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, CmpID, PreboardingFid, EmployeeBranchId, EmployeeOrigin, EmployeeSeries, EmployeeNo, EmployeeCardNo, LocalExpat, IsNRI, Salutation, EmployeeName, EmployeeLevelID, EmployeeWageID, EmployeeDepartmentID, EmployeeSubDepartmentName, EmployeeDesignationID, EmployeeGradeID, EmployeeGroupID, EmployeeTypeID, EmployeeCostCenterID, EmployeeZoneID, EmployeeUnitID, ContractorID, IsCriticalStageApplicable, EmployeeCriticalStageID, EmployeeLineID, JoiningDate, IsJoiningSpecial, JoiningStatus, TraineeDueDate, ProbationDueDate, ConfirmationDate, IsConfirmation, ConfirmationBy, CompanyMobileNo, CompanyMailID, ReportingHR, ReportingManager1, ReportingManager2, ReportingAccount, NoOfBranchTransfer, IsReasigned, ResignationDate, NoticePeriodDays, IsExit, ExitDate, EmployeeLeft, LeavingDate, LeavingReason, LeavingReasonPF, LeavingReasonESI, EM_PayrollLastDate, VMSApplicable, IsVMSMultipleLocation, LeftEmployeeApprovedBy)    " +
                                                "   Select EmployeeId, EmployeeId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, CmpID, PreboardingFid, EmployeeBranchId, EmployeeOrigin, EmployeeSeries, EmployeeNo, EmployeeCardNo, LocalExpat, IsNRI, Salutation, EmployeeName, EmployeeLevelID, EmployeeWageID, EmployeeDepartmentID, EmployeeSubDepartmentName, EmployeeDesignationID, EmployeeGradeID, EmployeeGroupID, EmployeeTypeID, EmployeeCostCenterID, EmployeeZoneID, EmployeeUnitID, ContractorID, IsCriticalStageApplicable, EmployeeCriticalStageID, EmployeeLineID, JoiningDate, IsJoiningSpecial, JoiningStatus, TraineeDueDate, ProbationDueDate, ConfirmationDate, IsConfirmation, ConfirmationBy, CompanyMobileNo, CompanyMailID, ReportingHR, ReportingManager1, ReportingManager2, ReportingAccount, NoOfBranchTransfer, IsReasigned, ResignationDate, NoticePeriodDays, IsExit, ExitDate, EmployeeLeft, LeavingDate, LeavingReason, LeavingReasonPF, LeavingReasonESI, EM_PayrollLastDate, VMSApplicable, IsVMSMultipleLocation, LeftEmployeeApprovedBy from Mas_Employee where Mas_Employee.Deactivate = 0 and Mas_Employee.EmployeeId = " + List_Employee_Id + "    " +
                                                " end  " +
                                                "   " +
                                                "Update Mas_Employee set  MachineName='" + Dns.GetHostName().ToString() + "'," +
                                                                                                    " ModifiedBy='" + Session["EmployeeName"] + "'," +
                                                                                                    " ModifiedDate=GetDate()," +
                                                                                                    " EmployeeDesignationID = '" + List_Designation_Id + "'" +
                                                                                                    " where EmployeeId =" + List_Employee_Id + "" +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " ";
                        strBuilder.Append(Employee_Admin);
                        i = i + 1;
                    }

                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record Update successfully";
                        TempData["Icon"] = "success";
                    }
                    if (abc != "")
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                }

                if (Type == "Contractor")
                {
                    foreach (var Data in tbldata)
                    {


                        List_Employee_Id = 0;
                        List_Contractor_Id = 0;
                        Contractor_Id = 0;

                        EmployeeNo_Id = 0;

                        if (Data.EmployeeNo == null)
                        {
                            var Message = "Enter Employee no in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetEmployeeId_Id(Data.EmployeeNo.ToString(), BranchId, out Employee_Id);
                        List_Employee_Id = Employee_Id;
                        if (Employee_Id == 0)
                        {
                            var Message = "The employee does not belong to this business unit (SrNo). " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        ////EmployeeNo   check  end ----------------------------------------------------------------------------------------


                        if (Data.ContractorName == null)
                        {
                            var Message = "Enter Contractor name in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetContractor_Id(Data.ContractorName.ToString(), out Contractor_Id, CmpId, BranchId);
                        List_Contractor_Id = Contractor_Id;
                        if (List_Contractor_Id == 0)
                        {
                            var Message = "Contractor name is not mapped to the specified business unit in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }
                        ////Employee Line  check  end ----------------------------------------------------------------------------------------


                        string Employee_Admin = "if (exists(select EmployeeId from  Mas_Employee where EmployeeId = " + List_Employee_Id + "  and Mas_Employee.Deactivate = 0)) " +
                                                  " Begin " +
                                                "   Insert into Log_Mas_Employee  (EmployeeId, EmployeeId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, CmpID, PreboardingFid, EmployeeBranchId, EmployeeOrigin, EmployeeSeries, EmployeeNo, EmployeeCardNo, LocalExpat, IsNRI, Salutation, EmployeeName, EmployeeLevelID, EmployeeWageID, EmployeeDepartmentID, EmployeeSubDepartmentName, EmployeeDesignationID, EmployeeGradeID, EmployeeGroupID, EmployeeTypeID, EmployeeCostCenterID, EmployeeZoneID, EmployeeUnitID, ContractorID, IsCriticalStageApplicable, EmployeeCriticalStageID, EmployeeLineID, JoiningDate, IsJoiningSpecial, JoiningStatus, TraineeDueDate, ProbationDueDate, ConfirmationDate, IsConfirmation, ConfirmationBy, CompanyMobileNo, CompanyMailID, ReportingHR, ReportingManager1, ReportingManager2, ReportingAccount, NoOfBranchTransfer, IsReasigned, ResignationDate, NoticePeriodDays, IsExit, ExitDate, EmployeeLeft, LeavingDate, LeavingReason, LeavingReasonPF, LeavingReasonESI, EM_PayrollLastDate, VMSApplicable, IsVMSMultipleLocation, LeftEmployeeApprovedBy)    " +
                                                "   Select EmployeeId, EmployeeId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, CmpID, PreboardingFid, EmployeeBranchId, EmployeeOrigin, EmployeeSeries, EmployeeNo, EmployeeCardNo, LocalExpat, IsNRI, Salutation, EmployeeName, EmployeeLevelID, EmployeeWageID, EmployeeDepartmentID, EmployeeSubDepartmentName, EmployeeDesignationID, EmployeeGradeID, EmployeeGroupID, EmployeeTypeID, EmployeeCostCenterID, EmployeeZoneID, EmployeeUnitID, ContractorID, IsCriticalStageApplicable, EmployeeCriticalStageID, EmployeeLineID, JoiningDate, IsJoiningSpecial, JoiningStatus, TraineeDueDate, ProbationDueDate, ConfirmationDate, IsConfirmation, ConfirmationBy, CompanyMobileNo, CompanyMailID, ReportingHR, ReportingManager1, ReportingManager2, ReportingAccount, NoOfBranchTransfer, IsReasigned, ResignationDate, NoticePeriodDays, IsExit, ExitDate, EmployeeLeft, LeavingDate, LeavingReason, LeavingReasonPF, LeavingReasonESI, EM_PayrollLastDate, VMSApplicable, IsVMSMultipleLocation, LeftEmployeeApprovedBy from Mas_Employee where Mas_Employee.Deactivate = 0 and Mas_Employee.EmployeeId = " + List_Employee_Id + "    " +
                                                " end  " +
                                                "   " +
                                                "Update Mas_Employee set  MachineName='" + Dns.GetHostName().ToString() + "'," +
                                                                                                    " ModifiedBy='" + Session["EmployeeName"] + "'," +
                                                                                                    " ModifiedDate=GetDate()," +
                                                                                                    " ContractorID = '" + List_Contractor_Id + "'" +
                                                                                                    " where EmployeeId =" + List_Employee_Id + "" +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " ";
                        strBuilder.Append(Employee_Admin);
                        i = i + 1;
                    }

                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record Update successfully";
                        TempData["Icon"] = "success";
                    }
                    if (abc != "")
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                }

                if (Type == "Gender")
                {
                    foreach (var Data in tbldata)
                    {
                        List_Employee_Id = 0;
                        EmployeeNo_Id = 0;

                        if (Data.EmployeeNo == null)
                        {
                            var Message = "Enter Employee no in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetEmployeeId_Id(Data.EmployeeNo.ToString(), BranchId, out Employee_Id);
                        List_Employee_Id = Employee_Id;
                        if (Employee_Id == 0)
                        {
                            var Message = "The employee does not belong to this business unit (SrNo). " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        ////EmployeeNo   check  end ----------------------------------------------------------------------------------------



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
                        ////Employee Line  check  end ----------------------------------------------------------------------------------------


                        string Employee_Admin = "if (exists(select PersonalEmployeeId from  Mas_Employee_Personal where PersonalEmployeeId = " + List_Employee_Id + "  and Mas_Employee_Personal.Deactivate = 0)) " +
                                                  " Begin " +
                                                 "   Insert into Log_Mas_Employee_Personal(PersonalId, PersonalId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, PersonalEmployeeId, AadhaarNo, NameAsPerAadhaar, AadhaarNoMobileNoLink, AadhaarNoMobileNo, PAN, NameAsPerPan, PANAadhaarLink, PrimaryMobile, SecondaryMobile, WhatsAppNo, PersonalEmailId, BirthdayDate, AgeOfJoining, BirthdayPlace, BirthdayProofOfDocumentID, BirthdayProofOfCertificateNo, IsDOBSpecial, EmployeeQualificationID, QualificationRemark, Gender, BloodGroup, MaritalStatus, AnniversaryDate, Ifyouwantdonotdisclosemygenderthentick, PhysicallyDisabled, PhysicallyDisableType, PhysicallyDisableRemark, IdentificationMark, DrivingLicenceNo, DrivingLicenceExpiryDate, PassportNo, PassportExpiryDate, EmployeeReligionID, Ifyouwantdonotdisclosemyreligioncastthentick, EmployeeCasteID, EmployeeSpecificDegree, EmployeeBirthProofEducation, EmployeeSubCategory)    " +
                                                "   Select PersonalId, PersonalId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, PersonalEmployeeId, AadhaarNo, NameAsPerAadhaar, AadhaarNoMobileNoLink, AadhaarNoMobileNo, PAN, NameAsPerPan, PANAadhaarLink, PrimaryMobile, SecondaryMobile, WhatsAppNo, PersonalEmailId, BirthdayDate, AgeOfJoining, BirthdayPlace, BirthdayProofOfDocumentID, BirthdayProofOfCertificateNo, IsDOBSpecial, EmployeeQualificationID, QualificationRemark, Gender, BloodGroup, MaritalStatus, AnniversaryDate, Ifyouwantdonotdisclosemygenderthentick, PhysicallyDisabled, PhysicallyDisableType, PhysicallyDisableRemark, IdentificationMark, DrivingLicenceNo, DrivingLicenceExpiryDate, PassportNo, PassportExpiryDate, EmployeeReligionID, Ifyouwantdonotdisclosemyreligioncastthentick, EmployeeCasteID, EmployeeSpecificDegree, EmployeeBirthProofEducation, EmployeeSubCategory from Mas_Employee_Personal where Mas_Employee_Personal.Deactivate = 0 and Mas_Employee_Personal.PersonalEmployeeId =" + List_Employee_Id + "    " +
                                                " end  " +
                                                "   " +
                                                "Update Mas_Employee_Personal set  MachineName='" + Dns.GetHostName().ToString() + "'," +
                                                                                                    " ModifiedBy='" + Session["EmployeeName"] + "'," +
                                                                                                    " ModifiedDate=GetDate()," +
                                                                                                    " Gender = '" + Data.Gender + "'" +
                                                                                                    " where PersonalEmployeeId =" + List_Employee_Id + "" +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " ";
                        strBuilder.Append(Employee_Admin);
                        i = i + 1;
                    }

                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record Update successfully";
                        TempData["Icon"] = "success";
                    }
                    if (abc != "")
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                }

                if (Type == "Date Of Birth")
                {
                    foreach (var Data in tbldata)
                    {
                        List_Employee_Id = 0;
                        EmployeeNo_Id = 0;

                        if (Data.EmployeeNo == null)
                        {
                            var Message = "Enter Employee no in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetEmployeeId_Id(Data.EmployeeNo.ToString(), BranchId, out Employee_Id);
                        List_Employee_Id = Employee_Id;
                        if (Employee_Id == 0)
                        {
                            var Message = "The employee does not belong to this business unit (SrNo). " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        ////EmployeeNo   check  end ----------------------------------------------------------------------------------------

                        if (Data.BirthdayDate == null)
                        {
                            var Message = "Enter birthday date in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }



                        string Employee_Admin = "if (exists(select PersonalEmployeeId from  Mas_Employee_Personal where PersonalEmployeeId = " + List_Employee_Id + "  and Mas_Employee_Personal.Deactivate = 0)) " +
                                                  " Begin " +
                                                 "   Insert into Log_Mas_Employee_Personal(PersonalId, PersonalId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, PersonalEmployeeId, AadhaarNo, NameAsPerAadhaar, AadhaarNoMobileNoLink, AadhaarNoMobileNo, PAN, NameAsPerPan, PANAadhaarLink, PrimaryMobile, SecondaryMobile, WhatsAppNo, PersonalEmailId, BirthdayDate, AgeOfJoining, BirthdayPlace, BirthdayProofOfDocumentID, BirthdayProofOfCertificateNo, IsDOBSpecial, EmployeeQualificationID, QualificationRemark, Gender, BloodGroup, MaritalStatus, AnniversaryDate, Ifyouwantdonotdisclosemygenderthentick, PhysicallyDisabled, PhysicallyDisableType, PhysicallyDisableRemark, IdentificationMark, DrivingLicenceNo, DrivingLicenceExpiryDate, PassportNo, PassportExpiryDate, EmployeeReligionID, Ifyouwantdonotdisclosemyreligioncastthentick, EmployeeCasteID, EmployeeSpecificDegree, EmployeeBirthProofEducation, EmployeeSubCategory)    " +
                                                "   Select PersonalId, PersonalId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, PersonalEmployeeId, AadhaarNo, NameAsPerAadhaar, AadhaarNoMobileNoLink, AadhaarNoMobileNo, PAN, NameAsPerPan, PANAadhaarLink, PrimaryMobile, SecondaryMobile, WhatsAppNo, PersonalEmailId, BirthdayDate, AgeOfJoining, BirthdayPlace, BirthdayProofOfDocumentID, BirthdayProofOfCertificateNo, IsDOBSpecial, EmployeeQualificationID, QualificationRemark, Gender, BloodGroup, MaritalStatus, AnniversaryDate, Ifyouwantdonotdisclosemygenderthentick, PhysicallyDisabled, PhysicallyDisableType, PhysicallyDisableRemark, IdentificationMark, DrivingLicenceNo, DrivingLicenceExpiryDate, PassportNo, PassportExpiryDate, EmployeeReligionID, Ifyouwantdonotdisclosemyreligioncastthentick, EmployeeCasteID, EmployeeSpecificDegree, EmployeeBirthProofEducation, EmployeeSubCategory from Mas_Employee_Personal where Mas_Employee_Personal.Deactivate = 0 and Mas_Employee_Personal.PersonalEmployeeId =" + List_Employee_Id + "    " +
                                                " end  " +
                                                "   " +
                                                "Update Mas_Employee_Personal set  MachineName='" + Dns.GetHostName().ToString() + "'," +
                                                                                                    " ModifiedBy='" + Session["EmployeeName"] + "'," +
                                                                                                    " ModifiedDate=GetDate()," +
                                                                                                    " BirthdayDate = '" + Data.BirthdayDate + "'" +
                                                                                                    " where PersonalEmployeeId =" + List_Employee_Id + "" +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " ";
                        strBuilder.Append(Employee_Admin);
                        i = i + 1;
                    }

                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record Update successfully";
                        TempData["Icon"] = "success";
                    }
                    if (abc != "")
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                }


                if (Type == "Marital Status")
                {
                    foreach (var Data in tbldata)
                    {
                        List_Employee_Id = 0;
                        EmployeeNo_Id = 0;

                        if (Data.EmployeeNo == null)
                        {
                            var Message = "Enter Employee no in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetEmployeeId_Id(Data.EmployeeNo.ToString(), BranchId, out Employee_Id);
                        List_Employee_Id = Employee_Id;
                        if (Employee_Id == 0)
                        {
                            var Message = "The employee does not belong to this business unit (SrNo). " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        ////EmployeeNo   check  end ----------------------------------------------------------------------------------------
                        if (Data.MaritalStatus == null)
                        {
                            var Message = "Enter marital status in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }
                        else if (Data.MaritalStatus == "Unmarried" || Data.MaritalStatus == "Married" || Data.MaritalStatus == "Other")
                        {

                        }
                        else
                        {
                            var Message = "Please enter proper marital status in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }


                        string Employee_Admin = "if (exists(select PersonalEmployeeId from  Mas_Employee_Personal where PersonalEmployeeId = " + List_Employee_Id + "  and Mas_Employee_Personal.Deactivate = 0)) " +
                                                  " Begin " +
                                                 "   Insert into Log_Mas_Employee_Personal(PersonalId, PersonalId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, PersonalEmployeeId, AadhaarNo, NameAsPerAadhaar, AadhaarNoMobileNoLink, AadhaarNoMobileNo, PAN, NameAsPerPan, PANAadhaarLink, PrimaryMobile, SecondaryMobile, WhatsAppNo, PersonalEmailId, BirthdayDate, AgeOfJoining, BirthdayPlace, BirthdayProofOfDocumentID, BirthdayProofOfCertificateNo, IsDOBSpecial, EmployeeQualificationID, QualificationRemark, Gender, BloodGroup, MaritalStatus, AnniversaryDate, Ifyouwantdonotdisclosemygenderthentick, PhysicallyDisabled, PhysicallyDisableType, PhysicallyDisableRemark, IdentificationMark, DrivingLicenceNo, DrivingLicenceExpiryDate, PassportNo, PassportExpiryDate, EmployeeReligionID, Ifyouwantdonotdisclosemyreligioncastthentick, EmployeeCasteID, EmployeeSpecificDegree, EmployeeBirthProofEducation, EmployeeSubCategory)    " +
                                                "   Select PersonalId, PersonalId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, PersonalEmployeeId, AadhaarNo, NameAsPerAadhaar, AadhaarNoMobileNoLink, AadhaarNoMobileNo, PAN, NameAsPerPan, PANAadhaarLink, PrimaryMobile, SecondaryMobile, WhatsAppNo, PersonalEmailId, BirthdayDate, AgeOfJoining, BirthdayPlace, BirthdayProofOfDocumentID, BirthdayProofOfCertificateNo, IsDOBSpecial, EmployeeQualificationID, QualificationRemark, Gender, BloodGroup, MaritalStatus, AnniversaryDate, Ifyouwantdonotdisclosemygenderthentick, PhysicallyDisabled, PhysicallyDisableType, PhysicallyDisableRemark, IdentificationMark, DrivingLicenceNo, DrivingLicenceExpiryDate, PassportNo, PassportExpiryDate, EmployeeReligionID, Ifyouwantdonotdisclosemyreligioncastthentick, EmployeeCasteID, EmployeeSpecificDegree, EmployeeBirthProofEducation, EmployeeSubCategory from Mas_Employee_Personal where Mas_Employee_Personal.Deactivate = 0 and Mas_Employee_Personal.PersonalEmployeeId =" + List_Employee_Id + "    " +
                                                " end  " +
                                                "   " +
                                                "Update Mas_Employee_Personal set  MachineName='" + Dns.GetHostName().ToString() + "'," +
                                                                                                    " ModifiedBy='" + Session["EmployeeName"] + "'," +
                                                                                                    " ModifiedDate=GetDate()," +
                                                                                                    " MaritalStatus = '" + Data.MaritalStatus + "'" +
                                                                                                    " where PersonalEmployeeId =" + List_Employee_Id + "" +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " ";
                        strBuilder.Append(Employee_Admin);
                        i = i + 1;
                    }

                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record Update successfully";
                        TempData["Icon"] = "success";
                    }
                    if (abc != "")
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                }


                if (Type == "Primary Mobile No")
                {
                    foreach (var Data in tbldata)
                    {
                        List_Employee_Id = 0;
                        EmployeeNo_Id = 0;

                        if (Data.EmployeeNo == null)
                        {
                            var Message = "Enter Employee no in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetEmployeeId_Id(Data.EmployeeNo.ToString(), BranchId, out Employee_Id);
                        List_Employee_Id = Employee_Id;
                        if (Employee_Id == 0)
                        {
                            var Message = "The employee does not belong to this business unit (SrNo). " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }



                        if (Data.PrimaryMobile == null)
                        {
                            var Message = "Enter valid Mobile no in SrNo" + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

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

                        string Employee_Admin = "if (exists(select PersonalEmployeeId from  Mas_Employee_Personal where PersonalEmployeeId = " + List_Employee_Id + "  and Mas_Employee_Personal.Deactivate = 0)) " +
                                                  " Begin " +
                                                 "   Insert into Log_Mas_Employee_Personal(PersonalId, PersonalId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, PersonalEmployeeId, AadhaarNo, NameAsPerAadhaar, AadhaarNoMobileNoLink, AadhaarNoMobileNo, PAN, NameAsPerPan, PANAadhaarLink, PrimaryMobile, SecondaryMobile, WhatsAppNo, PersonalEmailId, BirthdayDate, AgeOfJoining, BirthdayPlace, BirthdayProofOfDocumentID, BirthdayProofOfCertificateNo, IsDOBSpecial, EmployeeQualificationID, QualificationRemark, Gender, BloodGroup, MaritalStatus, AnniversaryDate, Ifyouwantdonotdisclosemygenderthentick, PhysicallyDisabled, PhysicallyDisableType, PhysicallyDisableRemark, IdentificationMark, DrivingLicenceNo, DrivingLicenceExpiryDate, PassportNo, PassportExpiryDate, EmployeeReligionID, Ifyouwantdonotdisclosemyreligioncastthentick, EmployeeCasteID, EmployeeSpecificDegree, EmployeeBirthProofEducation, EmployeeSubCategory)    " +
                                                "   Select PersonalId, PersonalId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, PersonalEmployeeId, AadhaarNo, NameAsPerAadhaar, AadhaarNoMobileNoLink, AadhaarNoMobileNo, PAN, NameAsPerPan, PANAadhaarLink, PrimaryMobile, SecondaryMobile, WhatsAppNo, PersonalEmailId, BirthdayDate, AgeOfJoining, BirthdayPlace, BirthdayProofOfDocumentID, BirthdayProofOfCertificateNo, IsDOBSpecial, EmployeeQualificationID, QualificationRemark, Gender, BloodGroup, MaritalStatus, AnniversaryDate, Ifyouwantdonotdisclosemygenderthentick, PhysicallyDisabled, PhysicallyDisableType, PhysicallyDisableRemark, IdentificationMark, DrivingLicenceNo, DrivingLicenceExpiryDate, PassportNo, PassportExpiryDate, EmployeeReligionID, Ifyouwantdonotdisclosemyreligioncastthentick, EmployeeCasteID, EmployeeSpecificDegree, EmployeeBirthProofEducation, EmployeeSubCategory from Mas_Employee_Personal where Mas_Employee_Personal.Deactivate = 0 and Mas_Employee_Personal.PersonalEmployeeId =" + List_Employee_Id + "    " +
                                                " end  " +
                                                "   " +
                                                "Update Mas_Employee_Personal set  MachineName='" + Dns.GetHostName().ToString() + "'," +
                                                                                                    " ModifiedBy='" + Session["EmployeeName"] + "'," +
                                                                                                    " ModifiedDate=GetDate()," +
                                                                                                    " PrimaryMobile = '" + Data.PrimaryMobile + "'" +
                                                                                                    " where PersonalEmployeeId =" + List_Employee_Id + "" +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " " +
                                                                                                    " ";
                        strBuilder.Append(Employee_Admin);
                        i = i + 1;
                    }

                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record Update successfully";
                        TempData["Icon"] = "success";
                    }
                    if (abc != "")
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                }


                if (Type == "Sub Unit")
                {
                    foreach (var Data in tbldata)
                    {
                        List_Employee_Id = 0;
                        List_Unit_Id = 0;
                        Unit_Id = 0;
                        EmployeeNo_Id = 0;

                        if (Data.EmployeeNo == null)
                        {
                            var Message = "Enter Employee no in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetEmployeeId_Id(Data.EmployeeNo.ToString(), BranchId, out Employee_Id);
                        List_Employee_Id = Employee_Id;
                        if (Employee_Id == 0)
                        {
                            var Message = "The employee does not belong to this business unit (SrNo). " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        ////EmployeeNo   check  end ----------------------------------------------------------------------------------------

                        if (Data.SubUnitName == null)
                        {
                            var Message = "Enter sub unit in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }

                        GetUnit_Id(Data.SubUnitName.ToString(), CmpId, BranchId, out Unit_Id);
                        List_Unit_Id = Unit_Id;
                        if (List_Unit_Id == 0)
                        {
                            var Message = "Sub unit not valid in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }
                        ////Employee Sub Unit check  end ----------------------------------------------------------------------------------------


                        string Employee_Admin = "if (exists(select EmployeeId from  Mas_Employee where EmployeeId = " + List_Employee_Id + "  and Mas_Employee.Deactivate = 0)) " +
                                                                   " Begin " +
                                                                 "   Insert into Log_Mas_Employee  (EmployeeId, EmployeeId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, CmpID, PreboardingFid, EmployeeBranchId, EmployeeOrigin, EmployeeSeries, EmployeeNo, EmployeeCardNo, LocalExpat, IsNRI, Salutation, EmployeeName, EmployeeLevelID, EmployeeWageID, EmployeeDepartmentID, EmployeeSubDepartmentName, EmployeeDesignationID, EmployeeGradeID, EmployeeGroupID, EmployeeTypeID, EmployeeCostCenterID, EmployeeZoneID, EmployeeUnitID, ContractorID, IsCriticalStageApplicable, EmployeeCriticalStageID, EmployeeLineID, JoiningDate, IsJoiningSpecial, JoiningStatus, TraineeDueDate, ProbationDueDate, ConfirmationDate, IsConfirmation, ConfirmationBy, CompanyMobileNo, CompanyMailID, ReportingHR, ReportingManager1, ReportingManager2, ReportingAccount, NoOfBranchTransfer, IsReasigned, ResignationDate, NoticePeriodDays, IsExit, ExitDate, EmployeeLeft, LeavingDate, LeavingReason, LeavingReasonPF, LeavingReasonESI, EM_PayrollLastDate, VMSApplicable, IsVMSMultipleLocation, LeftEmployeeApprovedBy)    " +
                                                                 "   Select EmployeeId, EmployeeId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, CmpID, PreboardingFid, EmployeeBranchId, EmployeeOrigin, EmployeeSeries, EmployeeNo, EmployeeCardNo, LocalExpat, IsNRI, Salutation, EmployeeName, EmployeeLevelID, EmployeeWageID, EmployeeDepartmentID, EmployeeSubDepartmentName, EmployeeDesignationID, EmployeeGradeID, EmployeeGroupID, EmployeeTypeID, EmployeeCostCenterID, EmployeeZoneID, EmployeeUnitID, ContractorID, IsCriticalStageApplicable, EmployeeCriticalStageID, EmployeeLineID, JoiningDate, IsJoiningSpecial, JoiningStatus, TraineeDueDate, ProbationDueDate, ConfirmationDate, IsConfirmation, ConfirmationBy, CompanyMobileNo, CompanyMailID, ReportingHR, ReportingManager1, ReportingManager2, ReportingAccount, NoOfBranchTransfer, IsReasigned, ResignationDate, NoticePeriodDays, IsExit, ExitDate, EmployeeLeft, LeavingDate, LeavingReason, LeavingReasonPF, LeavingReasonESI, EM_PayrollLastDate, VMSApplicable, IsVMSMultipleLocation, LeftEmployeeApprovedBy from Mas_Employee where Mas_Employee.Deactivate = 0 and Mas_Employee.EmployeeId = " + List_Employee_Id + "    " +
                                                                 " end  " +
                                                                 "   " +
                                                                 "Update Mas_Employee set  MachineName='" + Dns.GetHostName().ToString() + "'," +
                                                                                                                     " ModifiedBy='" + Session["EmployeeName"] + "'," +
                                                                                                                     " ModifiedDate=GetDate()," +
                                                                                                                     " EmployeeUnitID = '" + List_Unit_Id + "'" +
                                                                                                                     " where EmployeeId =" + List_Employee_Id + "" +
                                                                                                                     " " +
                                                                                                                     " " +
                                                                                                                     " " +
                                                                                                                     " ";
                        strBuilder.Append(Employee_Admin);
                        i = i + 1;
                    }

                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record Update successfully";
                        TempData["Icon"] = "success";
                    }
                    if (abc != "")
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
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

        #region Get ID
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


                var matchingRows = from row in dt_SubDepartment.AsEnumerable()
                                   where string.Equals(row.Field<string>("SubDepartmentName"), Name, StringComparison.OrdinalIgnoreCase) &&
                                         row.Field<int>("DepartmentId") == Convert.ToInt16(DepartmentId)
                                   select row;

                var varSubDepatment_Id = matchingRows.LastOrDefault();
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

        //void GetEmployeeId_Id(string EmployeeNo, out int Employee_Id)
        //{

        //    try
        //    {

        //        var matchingRows = from row in dt_ValidationCheck.AsEnumerable()
        //                           where string.Equals(row.Field<string>("EmployeeNo"), EmployeeNo, StringComparison.OrdinalIgnoreCase)
        //                           select row;
        //        var varEmployee_Id = matchingRows.LastOrDefault();
        //        if (varEmployee_Id == null)
        //        {
        //            Employee_Id = 0;
        //        }
        //        else
        //        {
        //            Employee_Id = Convert.ToInt32(varEmployee_Id["EmployeeId"]);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Employee_Id = 0;
        //    }
        //}

        void GetEmployeeAdhaarNo_Id(string AadhaarNo, int List_Employee_Id, out string Employee_Name, out string EmployeeNo, out string BranchName)
        {
            try
            {
                // Find if Aadhaar is already assigned to another employee
                var conflictingRow = dt_ValidationCheck.AsEnumerable()
                    .FirstOrDefault(row =>
                        string.Equals(row.Field<string>("AadhaarNo"), AadhaarNo, StringComparison.OrdinalIgnoreCase) &&
                        row.Field<int>("EmployeeId") != List_Employee_Id);

                if (conflictingRow != null)
                {
                    // Aadhaar assigned to another employee
                    Employee_Name = conflictingRow.Field<string>("EmployeeName");
                    EmployeeNo = conflictingRow.Field<string>("EmployeeNo");
                    BranchName = conflictingRow.Field<string>("BranchName");
                }
                else
                {
                    // Aadhaar is either not found or matches the same employee ID
                    Employee_Name = "";
                    EmployeeNo = "";
                    BranchName = "";
                }
            }
            catch
            {
                // Handle exceptions gracefully by setting default values
                Employee_Name = "";
                EmployeeNo = "";
                BranchName = "";
            }
        }

        void GetEmployeeUAN_Id(string UANNo, int List_Employee_Id, out string Employee_Name, out string EmployeeNo, out string BranchName)
        {
            try
            {
                // Find if Aadhaar is already assigned to another employee
                var conflictingRow = dt_ValidationCheck.AsEnumerable()
                    .FirstOrDefault(row =>
                        string.Equals(row.Field<string>("PF_UAN"), UANNo, StringComparison.OrdinalIgnoreCase) &&
                        row.Field<int>("EmployeeId") != List_Employee_Id);

                if (conflictingRow != null)
                {
                    // Aadhaar assigned to another employee
                    Employee_Name = conflictingRow.Field<string>("EmployeeName");
                    EmployeeNo = conflictingRow.Field<string>("EmployeeNo");
                    BranchName = conflictingRow.Field<string>("BranchName");
                }
                else
                {
                    // Aadhaar is either not found or matches the same employee ID
                    Employee_Name = "";
                    EmployeeNo = "";
                    BranchName = "";
                }
            }
            catch
            {
                // Handle exceptions gracefully by setting default values
                Employee_Name = "";
                EmployeeNo = "";
                BranchName = "";
            }
        }

        void GetEmployeeId_Id(string EmployeeNo, int EmployeeBranchId, out int Employee_Id)
        {
            try
            {
                // Find the row where EmployeeNo matches but belongs to a different branch
                var conflictingRow = dt_ValidationCheck.AsEnumerable()
                    .FirstOrDefault(row =>
                        string.Equals(row.Field<string>("EmployeeNo"), EmployeeNo, StringComparison.OrdinalIgnoreCase) &&
                        row.Field<int>("EmployeeBranchId") != EmployeeBranchId);

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
                            row.Field<int>("EmployeeBranchId") == EmployeeBranchId);

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


        //void GetEmployeeId_Id(string EmployeeNo,int EmployeeBranchId, out int Employee_Id)
        //{
        //    try
        //    {
        //        var conflictingRow = dt_ValidationCheck.AsEnumerable()
        //            .FirstOrDefault(row =>
        //                string.Equals(row.Field<string>("EmployeeNo"), EmployeeNo, StringComparison.OrdinalIgnoreCase) &&
        //                row.Field<int>("EmployeeBranchId") != EmployeeBranchId);

        //        if (conflictingRow != null)
        //        {
        //            // Aadhaar assigned to another employee
        //            Employee_Id = Convert.ToInt32(conflictingRow["EmployeeId"]);
        //           // Employee_Id = conflictingRow.Field<string>("EmployeeName");
        //        }
        //        else
        //        {
        //            // Aadhaar is either not found or matches the same employee ID
        //            Employee_Id = 0;
        //        }
        //    }
        //    catch
        //    {
        //        // Handle exceptions gracefully by setting default values
        //        Employee_Id = 0;
        //    }
        //}

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
                                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase) && row.Field<int>("CmpId") == Convert.ToInt16(companyId) && row.Field<int>("BranchId") == Convert.ToInt16(BUId)
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

        #endregion

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(string Type)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }

            if (Type == "Aadhar")
            {
                string filePath = System.Web.HttpContext.Current.Server.MapPath("~/assets/BulkUpdate/UpdateEmployeeAadharDetails.xlsx");

                if (System.IO.File.Exists(filePath))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "application/octet-stream", "UpdateEmployeeAadharDetails.xlsx");
                }

                return HttpNotFound("File not found.");
            }
            else if (Type == "UAN")
            {
                string filePath = System.Web.HttpContext.Current.Server.MapPath("~/assets/BulkUpdate/UpdateEmployeeStatutoryDetails.xlsx");
                if (System.IO.File.Exists(filePath))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "application/octet-stream", "UpdateEmployeeStatutoryDetails.xlsx");
                }

                return HttpNotFound("File not found.");
            }
            else if (Type == "Address")
            {
                //D:\Abhishek-Pc\KAMPASSHR\SmAuto\SM_KOMPASSHR_ABHISHEK\KompassHR\KompassHR\assets\BulkUpdate\UpdateEmployeeAddressDetails.xlsx
                string filePath = System.Web.HttpContext.Current.Server.MapPath("~/assets/BulkUpdate/UpdateEmployeeAddressDetails.xlsx");
                if (System.IO.File.Exists(filePath))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "application/octet-stream", "UpdateEmployeeAddressDetails.xlsx");
                }

                return HttpNotFound("File not found.");
            }

            else if (Type == "Contractor")
            {
                //D:\Abhishek-Pc\KAMPASSHR\SmAuto\SM_KOMPASSHR_ABHISHEK\KompassHR\KompassHR\assets\BulkUpdate\UpdateEmployeeAddressDetails.xlsx
                string filePath = System.Web.HttpContext.Current.Server.MapPath("~/assets/BulkUpdate/UpdateEmployeeContractor.xlsx");
                if (System.IO.File.Exists(filePath))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "application/octet-stream", "UpdateEmployeeContractor.xlsx");
                }

                return HttpNotFound("File not found.");
            }

            else if (Type == "Critical Stage Category")
            {
                //D:\Abhishek-Pc\KAMPASSHR\SmAuto\SM_KOMPASSHR_ABHISHEK\KompassHR\KompassHR\assets\BulkUpdate\UpdateEmployeeAddressDetails.xlsx
                string filePath = System.Web.HttpContext.Current.Server.MapPath("~/assets/BulkUpdate/UpdateEmployeeCriticalStageCategory.xlsx");
                if (System.IO.File.Exists(filePath))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "application/octet-stream", "UpdateEmployeeCriticalStageCategory.xlsx");
                }

                return HttpNotFound("File not found.");
            }

            else if (Type == "Department")
            {
                //D:\Abhishek-Pc\KAMPASSHR\SmAuto\SM_KOMPASSHR_ABHISHEK\KompassHR\KompassHR\assets\BulkUpdate\UpdateEmployeeAddressDetails.xlsx
                string filePath = System.Web.HttpContext.Current.Server.MapPath("~/assets/BulkUpdate/UpdateEmployeeDepartment.xlsx");
                if (System.IO.File.Exists(filePath))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "application/octet-stream", "UpdateEmployeeDepartment.xlsx");
                }

                return HttpNotFound("File not found.");
            }

            else if (Type == "Attendance")
            {
                //D:\Abhishek-Pc\KAMPASSHR\SmAuto\SM_KOMPASSHR_ABHISHEK\KompassHR\KompassHR\assets\BulkUpdate\UpdateEmployeeAddressDetails.xlsx
                string filePath = System.Web.HttpContext.Current.Server.MapPath("~/assets/BulkUpdate/UpdateEmployeeAttendanceDetails.xlsx");
                if (System.IO.File.Exists(filePath))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "application/octet-stream", "UpdateEmployeeAttendanceDetails.xlsx");
                }

                return HttpNotFound("File not found.");
            }

            else if (Type == "Manpower Category")
            {
                //D:\Abhishek-Pc\KAMPASSHR\SmAuto\SM_KOMPASSHR_ABHISHEK\KompassHR\KompassHR\assets\BulkUpdate\UpdateEmployeeAddressDetails.xlsx
                string filePath = System.Web.HttpContext.Current.Server.MapPath("~/assets/BulkUpdate/UpdateEmployeeManpowerCategory.xlsx");
                if (System.IO.File.Exists(filePath))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "application/octet-stream", "UpdateEmployeeManpowerCategory.xlsx");
                }

                return HttpNotFound("File not found.");
            }

            else if (Type == "Line Master")
            {
                //D:\Abhishek-Pc\KAMPASSHR\SmAuto\SM_KOMPASSHR_ABHISHEK\KompassHR\KompassHR\assets\BulkUpdate\UpdateEmployeeAddressDetails.xlsx
                string filePath = System.Web.HttpContext.Current.Server.MapPath("~/assets/BulkUpdate/UpdateEmployeeLineMaster.xlsx");
                if (System.IO.File.Exists(filePath))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "application/octet-stream", "UpdateEmployeeLineMaster.xlsx");
                }

                return HttpNotFound("File not found.");
            }

            else if (Type == "Assessment Level")
            {
                //D:\Abhishek-Pc\KAMPASSHR\SmAuto\SM_KOMPASSHR_ABHISHEK\KompassHR\KompassHR\assets\BulkUpdate\UpdateEmployeeAddressDetails.xlsx
                string filePath = System.Web.HttpContext.Current.Server.MapPath("~/assets/BulkUpdate/UpdateEmployeeAssessmentLevel.xlsx");
                if (System.IO.File.Exists(filePath))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "application/octet-stream", "UpdateEmployeeAssessmentLevel.xlsx");
                }

                return HttpNotFound("File not found.");
            }

            else if (Type == "Grade")
            {
                //D:\Abhishek-Pc\KAMPASSHR\SmAuto\SM_KOMPASSHR_ABHISHEK\KompassHR\KompassHR\assets\BulkUpdate\UpdateEmployeeAddressDetails.xlsx
                string filePath = System.Web.HttpContext.Current.Server.MapPath("~/assets/BulkUpdate/UpdateEmployeeGrade.xlsx");
                if (System.IO.File.Exists(filePath))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "application/octet-stream", "UpdateEmployeeGrade.xlsx");
                }

                return HttpNotFound("File not found.");
            }

            else if (Type == "Designation")
            {
                //D:\Abhishek-Pc\KAMPASSHR\SmAuto\SM_KOMPASSHR_ABHISHEK\KompassHR\KompassHR\assets\BulkUpdate\UpdateEmployeeAddressDetails.xlsx
                string filePath = System.Web.HttpContext.Current.Server.MapPath("~/assets/BulkUpdate/UpdateEmployeeDesignation.xlsx");
                if (System.IO.File.Exists(filePath))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "application/octet-stream", "UpdateEmployeeDesignation.xlsx");
                }

                return HttpNotFound("File not found.");
            }

            else if (Type == "Sub Unit")
            {
                //D:\Abhishek-Pc\KAMPASSHR\SmAuto\SM_KOMPASSHR_ABHISHEK\KompassHR\KompassHR\assets\BulkUpdate\UpdateEmployeeAddressDetails.xlsx
                string filePath = System.Web.HttpContext.Current.Server.MapPath("~/assets/BulkUpdate/UpdateEmployeeSubUnit.xlsx");
                if (System.IO.File.Exists(filePath))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "application/octet-stream", "UpdateEmployeeSubUnit.xlsx");
                }

                return HttpNotFound("File not found.");
            }

            else if (Type == "Date Of Birth")
            {
                //D:\Abhishek-Pc\KAMPASSHR\SmAuto\SM_KOMPASSHR_ABHISHEK\KompassHR\KompassHR\assets\BulkUpdate\UpdateEmployeeAddressDetails.xlsx
                string filePath = System.Web.HttpContext.Current.Server.MapPath("~/assets/BulkUpdate/UpdateEmployeeDateOfBirth.xlsx");
                if (System.IO.File.Exists(filePath))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "application/octet-stream", "UpdateEmployeeDateOfBirth.xlsx");
                }

                return HttpNotFound("File not found.");
            }

            else if (Type == "Gender")
            {
                //D:\Abhishek-Pc\KAMPASSHR\SmAuto\SM_KOMPASSHR_ABHISHEK\KompassHR\KompassHR\assets\BulkUpdate\UpdateEmployeeAddressDetails.xlsx
                string filePath = System.Web.HttpContext.Current.Server.MapPath("~/assets/BulkUpdate/UpdateEmployeeGender.xlsx");
                if (System.IO.File.Exists(filePath))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "application/octet-stream", "UpdateEmployeeGender.xlsx");
                }

                return HttpNotFound("File not found.");
            }

            else if (Type == "Marital Status")
            {
                //D:\Abhishek-Pc\KAMPASSHR\SmAuto\SM_KOMPASSHR_ABHISHEK\KompassHR\KompassHR\assets\BulkUpdate\UpdateEmployeeMaritalStatus.xlsx
                string filePath = System.Web.HttpContext.Current.Server.MapPath("~/assets/BulkUpdate/UpdateEmployeeMaritalStatus.xlsx");
                if (System.IO.File.Exists(filePath))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "application/octet-stream", "UpdateEmployeeMaritalStatus.xlsx");
                }

                return HttpNotFound("File not found.");
            }
            else if (Type == "Primary Mobile No")
            {
                //D:\Abhishek-Pc\KAMPASSHR\SmAuto\SM_KOMPASSHR_ABHISHEK\KompassHR\KompassHR\assets\BulkUpdate\UpdateEmployeeAddressDetails.xlsx
                string filePath = System.Web.HttpContext.Current.Server.MapPath("~/assets/BulkUpdate/UpdateEmployeeMobileNo.xlsx");
                if (System.IO.File.Exists(filePath))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, "application/octet-stream", "UpdateEmployeeMobileNo.xlsx");
                }

                return HttpNotFound("File not found.");
            }
            return HttpNotFound("Invalid request type.");

        }

        #endregion

        #region ConvertToDataTable
        private static DataTable ConvertToDataTable(IEnumerable<dynamic> data)
        {
            var dataTable = new DataTable();
            var firstRecord = data.FirstOrDefault() as IDictionary<string, object>;

            if (firstRecord != null)
            {
                // Create columns
                foreach (var kvp in firstRecord)
                {
                    dataTable.Columns.Add(kvp.Key, kvp.Value == null ? typeof(object) : kvp.Value.GetType());
                }

                // Populate rows
                foreach (var record in data)
                {
                    var dataRow = dataTable.NewRow();
                    foreach (var kvp in record)
                    {
                        dataRow[kvp.Key] = kvp.Value ?? DBNull.Value;
                    }
                    dataTable.Rows.Add(dataRow);
                }
            }

            return dataTable;
        }

        //private static DataTable ConvertToDataTable(IEnumerable<dynamic> data)
        //{
        //    var dataTable = new DataTable();
        //    var firstRecord = data.FirstOrDefault() as IDictionary<string, object>;

        //    if (firstRecord != null)
        //    {
        //        foreach (var kvp in firstRecord)
        //        {
        //            dataTable.Columns.Add(kvp.Key, kvp.Value == null ? typeof(object) : kvp.Value.GetType());
        //        }

        //        foreach (var record in data)
        //        {
        //            var dataRow = dataTable.NewRow();
        //            foreach (var kvp in record)
        //            {
        //                dataRow[kvp.Key] = kvp.Value;
        //            }
        //            dataTable.Rows.Add(dataRow);
        //        }
        //    }

        //    return dataTable;
        //}
        #endregion

    }
}