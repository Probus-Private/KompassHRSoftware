using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClosedXML.Excel;
using Newtonsoft.Json;
using System.Data;
using System.IO;
using System.ComponentModel;
using OfficeOpenXml;
using System.Reflection;
using KompassHR.Areas.Module.Models.Module_Employee;
using System.Configuration;
using System.Text;
using System.Net;

namespace KompassHR.Areas.Setting.Controllers.Setting_UserAccessPolicy
{
    public class Setting_MasEmployee_ExportController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();

        // GET: Setting/Setting_MasEmployee_Export
        #region Main View
        public ActionResult Setting_MasEmployee_Export()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 694;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var GetEmployeeParams = new DynamicParameters();
                var employeeColumns = DapperORM.ExecuteSP<dynamic>("sp_List_GetMasEmployeeColumns").ToList();
                ViewBag.GetEmployeeList = employeeColumns;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region DownloadExcelFile
        [HttpPost]
        public ActionResult DownloadExcelFile(string selectedFieldsJson)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                List<string> columnNames = TryParseColumnNames(selectedFieldsJson);
                DataTable dtFiltered = ConvertListToPivotedDataTable(columnNames);
                using (var wb = new XLWorkbook())
                using (var ms = new MemoryStream())
                {
                    wb.Worksheets.Add(dtFiltered, "SelectedFields");
                    wb.SaveAs(ms);
                    ms.Position = 0;
                    return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SelectedFields.xlsx");
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        private List<string> TryParseColumnNames(string input)
        {
            var jsonParsed = JsonConvert.DeserializeObject<List<string>>(input);
            if (jsonParsed != null && jsonParsed.Any())
            {
                return jsonParsed;
            }
            input = input.Trim();

            if (input.StartsWith("[\"") && input.EndsWith("\"]"))
            {
                input = input.Substring(2, input.Length - 4);
            }

            return input.Split('\t').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        }
        private DataTable ConvertListToPivotedDataTable(List<string> columnNames)
        {
            DataTable dt = new DataTable();

            foreach (var colName in columnNames)
            {
                if (!dt.Columns.Contains(colName))
                {
                    dt.Columns.Add(colName, typeof(string));
                }
            }
            dt.Rows.Add(dt.NewRow());
            return dt;
        }

        #region UploadExcel
        [HttpPost]
        public ActionResult UploadExcel_WithSP(HttpPostedFileBase AttachFile)
        {
            var importList = new List<Mas_Employee_Import>();
            try
            {
                using (var package = new ExcelPackage(AttachFile.InputStream))
                {
                    if (package.Workbook == null || package.Workbook.Worksheets.Count == 0)
                        return Json(new { success = false, message = "Excel file has no worksheets." }, JsonRequestBehavior.AllowGet);

                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                    if (worksheet == null || worksheet.Dimension == null)
                        return Json(new { success = false, message = "Worksheet is empty or invalid." }, JsonRequestBehavior.AllowGet);

                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;

                    Dictionary<int, PropertyInfo> columnMappings = new Dictionary<int, PropertyInfo>();
                    var props = typeof(Mas_Employee_Import).GetProperties();

                    for (int col = 1; col <= colCount; col++)
                    {
                        string header = worksheet.Cells[1, col].Text.Trim();
                        var property = props.FirstOrDefault(p => p.Name.Equals(header, StringComparison.OrdinalIgnoreCase));
                        if (property != null)
                        {
                            columnMappings[col] = property;
                        }
                    }

                    if (columnMappings.Count == 0)
                    {
                        return Json(new { success = false, message = "Excel headers do not match model properties." }, JsonRequestBehavior.AllowGet);
                    }

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var emp = new Mas_Employee_Import();

                        foreach (var kv in columnMappings)
                        {
                            int col = kv.Key;
                            var prop = kv.Value;
                            var cellValue = worksheet.Cells[row, col].Text.Trim();

                            if (!string.IsNullOrEmpty(cellValue))
                            {
                                try
                                {
                                    object convertedValue = Convert.ChangeType(cellValue, prop.PropertyType);
                                    prop.SetValue(emp, convertedValue);
                                }
                                catch
                                {
                                    prop.SetValue(emp, null);
                                }
                            }
                        }
                        importList.Add(emp);
                    }
                }


                foreach (var emp in importList)
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@p_process", "Save");
                    parameters.Add("@p_ME_EmployeeId_Encrypted", emp.EmployeeId_Encrypted);
                    parameters.Add("@p_CreatedUpdateBy", Session["EmployeeName"]?.ToString() ?? "System");
                    parameters.Add("@p_MachineName", Environment.MachineName);
                    parameters.Add("@p_ME_CmpID", emp.CmpID);
                    parameters.Add("@p_ME_PreboardingFid", emp.PreboardingFid);
                    parameters.Add("@p_ME_EmployeeBranchId", emp.EmployeeBranchId);
                    parameters.Add("@p_ME_EmployeeOrigin", emp.EmployeeOrigin);
                    parameters.Add("@p_ME_EmployeeSeries", emp.EmployeeSeries);
                    parameters.Add("@p_ME_EmployeeNo", emp.EmployeeNo);
                    parameters.Add("@p_ME_EmployeeCardNo", emp.EmployeeCardNo);
                    parameters.Add("@p_ME_LocalExpat", emp.LocalExpat);
                    parameters.Add("@p_ME_IsNRI", emp.IsNRI);
                    parameters.Add("@p_ME_Salutation", emp.Salutation);
                    parameters.Add("@p_ME_EmployeeName", emp.EmployeeName);
                    parameters.Add("@p_ME_EmployeeLevelID", emp.EmployeeLevelID);
                    parameters.Add("@p_ME_EmployeeWageID", emp.EmployeeWageID);
                    parameters.Add("@p_ME_EmployeeDepartmentID", emp.EmployeeDepartmentID);
                    parameters.Add("@p_ME_EmployeeSubDepartmentName", emp.EmployeeSubDepartmentName);
                    parameters.Add("@p_ME_EmployeeDesignationID", emp.EmployeeDesignationID);
                    parameters.Add("@p_ME_EmployeeGradeID", emp.EmployeeGradeID);
                    parameters.Add("@p_ME_EmployeeGroupID", emp.EmployeeGroupID);
                    parameters.Add("@p_ME_EmployeeTypeID", emp.EmployeeTypeID);
                    parameters.Add("@p_ME_EmployeeCostCenterID", emp.EmployeeCostCenterID);
                    parameters.Add("@p_ME_EmployeeZoneID", emp.EmployeeZoneID);
                    parameters.Add("@p_ME_EmployeeUnitID", emp.EmployeeUnitID);
                    parameters.Add("@p_ME_ContractorID", emp.ContractorID);
                    parameters.Add("@p_ME_IsCriticalStageApplicable", emp.IsCriticalStageApplicable);
                    parameters.Add("@p_ME_EmployeeCriticalStageID", emp.EmployeeCriticalStageID);
                    parameters.Add("@p_ME_JoiningDate", emp.JoiningDate);
                    parameters.Add("@p_ME_IsJoiningSpecial", emp.IsJoiningSpecial);
                    parameters.Add("@p_ME_JoiningStatus", emp.JoiningStatus);
                    parameters.Add("@p_ME_TraineeDueDate", emp.TraineeDueDate);
                    parameters.Add("@p_ME_ProbationDueDate", emp.ProbationDueDate);
                    parameters.Add("@p_ME_ConfirmationDate", emp.ConfirmationDate);
                    parameters.Add("@p_ME_IsConfirmation", emp.IsConfirmation);
                    parameters.Add("@p_ME_ConfirmationBy", emp.ConfirmationBy);
                    parameters.Add("@p_ME_CompanyMobileNo", emp.CompanyMobileNo);
                    parameters.Add("@p_ME_CompanyMailID", emp.CompanyMailID);
                    parameters.Add("@p_ME_ReportingHR", emp.ReportingHR);
                    parameters.Add("@p_ME_ReportingManager1", emp.ReportingManager1);
                    parameters.Add("@p_ME_ReportingManager2", emp.ReportingManager2);
                    parameters.Add("@p_ME_ReportingAccount", emp.ReportingAccount);
                    parameters.Add("@p_ME_NoOfBranchTransfer", emp.NoOfBranchTransfer);
                    parameters.Add("@p_ME_IsReasigned", emp.IsReasigned);
                    parameters.Add("@p_ME_ResignationDate", emp.ResignationDate);
                    parameters.Add("@p_ME_NoticePeriodDays", emp.NoticePeriodDays);
                    parameters.Add("@p_ME_IsExit", emp.IsExit);
                    parameters.Add("@p_ME_ExitDate", emp.ExitDate);
                    parameters.Add("@p_ME_EmployeeLeft", emp.EmployeeLeft);
                    parameters.Add("@p_ME_LeavingDate", emp.LeavingDate);
                    parameters.Add("@p_ME_LeavingReason", emp.LeavingReason);
                    parameters.Add("@p_ME_LeavingReasonPF", emp.LeavingReasonPF);
                    parameters.Add("@p_ME_LeavingReasonESI", emp.LeavingReasonESI);
                    parameters.Add("@p_ME_EM_PayrollLastDate", emp.EM_PayrollLastDate);
                    parameters.Add("@p_ME_EM_Atten_DailyMonthly", emp.EM_Atten_DailyMonthly);

                    parameters.Add("@p_MasESS_ESSId_Encrypted", emp.ESSId_Encrypted);
                    //parameters.Add("@p_MasESS_ESSEmployeeId    
                    parameters.Add("@p_MasESS_ESSLoginID", emp.ESSLoginID);
                    parameters.Add("@p_MasESS_ESSPassword", emp.ESSPassword);
                    parameters.Add("@p_MasESS_ESSSecurityQuestion", emp.ESSSecurityQuestion);
                    parameters.Add("@p_MasESS_ESSAnswer", emp.ESSAnswer);
                    parameters.Add("@p_MasESS_ESSIsActive", emp.ESSIsActive);
                    parameters.Add("@p_MasESS_ESSIsLock", emp.ESSIsLock);
                    parameters.Add("@p_MasESS_ESSLoginAttemptCount", emp.ESSLoginAttemptCount);
                    //parameters.Add("@p_MasESS_ESSLastLoginTime", emp.ESSLastLoginTime);
                    //parameters.Add("@p_MasESS_ESSLastPasswordChange", emp.ESSLastPasswordChange);
                    parameters.Add("@p_MasESS_IsExit", emp.IsExit);
                    parameters.Add("@p_MasESS_IsApp", emp.IsApp);
                    parameters.Add("@p_MasESS_IsAdmin", emp.IsAdmin);
                    parameters.Add("@p_MasESS_UserAccessPolicyId", emp.UserAccessPolicyId);
                    parameters.Add("@p_MasESS_MFAEnabled", emp.MFAEnabled);
                    parameters.Add("@p_MasESS_IsMFA", emp.IsMFA);
                    parameters.Add("@p_MasESS_MFAToken", emp.MFAToken);
                    // parameters.Add("@p_MasESS_Device_Tokens", emp.Device_Tokens);

                    parameters.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                    parameters.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                    var result = DapperORM.ExecuteReturn("sp_SUD_EmployeeImportExport", parameters);
                    TempData["Message"] = parameters.Get<string>("@p_msg");
                    TempData["Icon"] = parameters.Get<string>("@p_Icon");
                }
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        #region UploadExcel
        [HttpPost]
        public ActionResult UploadExcel(HttpPostedFileBase AttachFile)
        {
            var importList = new List<Mas_Employee_Import>();
            try
            {
                using (var package = new ExcelPackage(AttachFile.InputStream))
                {
                    if (package.Workbook == null || package.Workbook.Worksheets.Count == 0)
                        return Json(new { success = false, message = "Excel file has no worksheets." }, JsonRequestBehavior.AllowGet);

                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                    if (worksheet == null || worksheet.Dimension == null)
                        return Json(new { success = false, message = "Worksheet is empty or invalid." }, JsonRequestBehavior.AllowGet);

                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;

                    Dictionary<int, PropertyInfo> columnMappings = new Dictionary<int, PropertyInfo>();
                    var props = typeof(Mas_Employee_Import).GetProperties();

                    for (int col = 1; col <= colCount; col++)
                    {
                        string header = worksheet.Cells[1, col].Text.Trim();
                        var property = props.FirstOrDefault(p => p.Name.Equals(header, StringComparison.OrdinalIgnoreCase));
                        if (property != null)
                        {
                            columnMappings[col] = property;
                        }
                    }

                    if (columnMappings.Count == 0)
                    {
                        return Json(new { success = false, message = "Excel headers do not match model properties." }, JsonRequestBehavior.AllowGet);
                    }

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var emp = new Mas_Employee_Import();

                        foreach (var kv in columnMappings)
                        {
                            int col = kv.Key;
                            var prop = kv.Value;
                            var cellValue = worksheet.Cells[row, col].Text.Trim();

                            if (!string.IsNullOrEmpty(cellValue))
                            {
                                try
                                {
                                    object convertedValue = Convert.ChangeType(cellValue, prop.PropertyType);
                                    prop.SetValue(emp, convertedValue);
                                }
                                catch
                                {
                                    prop.SetValue(emp, null);
                                }
                            }
                        }
                        importList.Add(emp);
                    }
                }

                foreach (var emp in importList)
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@p_process", "Save");
                    parameters.Add("@p_ME_EmployeeId_Encrypted", emp.EmployeeId_Encrypted);
                    parameters.Add("@p_CreatedUpdateBy", Session["EmployeeName"]?.ToString() ?? "System");
                    parameters.Add("@p_MachineName", Environment.MachineName);
                    parameters.Add("@p_ME_CmpID", emp.CmpID);
                    parameters.Add("@p_ME_PreboardingFid", emp.PreboardingFid);
                    parameters.Add("@p_ME_EmployeeBranchId", emp.EmployeeBranchId);
                    parameters.Add("@p_ME_EmployeeOrigin", emp.EmployeeOrigin);
                    parameters.Add("@p_ME_EmployeeSeries", emp.EmployeeSeries);
                    parameters.Add("@p_ME_EmployeeNo", emp.EmployeeNo);
                    parameters.Add("@p_ME_EmployeeCardNo", emp.EmployeeCardNo);
                    parameters.Add("@p_ME_LocalExpat", emp.LocalExpat);
                    parameters.Add("@p_ME_IsNRI", emp.IsNRI);
                    parameters.Add("@p_ME_Salutation", emp.Salutation);
                    parameters.Add("@p_ME_EmployeeName", emp.EmployeeName);
                    parameters.Add("@p_ME_EmployeeLevelID", emp.EmployeeLevelID);
                    parameters.Add("@p_ME_EmployeeWageID", emp.EmployeeWageID);
                    parameters.Add("@p_ME_EmployeeDepartmentID", emp.EmployeeDepartmentID);
                    parameters.Add("@p_ME_EmployeeSubDepartmentName", emp.EmployeeSubDepartmentName);
                    parameters.Add("@p_ME_EmployeeDesignationID", emp.EmployeeDesignationID);
                    parameters.Add("@p_ME_EmployeeGradeID", emp.EmployeeGradeID);
                    parameters.Add("@p_ME_EmployeeGroupID", emp.EmployeeGroupID);
                    parameters.Add("@p_ME_EmployeeTypeID", emp.EmployeeTypeID);
                    parameters.Add("@p_ME_EmployeeCostCenterID", emp.EmployeeCostCenterID);
                    parameters.Add("@p_ME_EmployeeZoneID", emp.EmployeeZoneID);
                    parameters.Add("@p_ME_EmployeeUnitID", emp.EmployeeUnitID);
                    parameters.Add("@p_ME_ContractorID", emp.ContractorID);
                    parameters.Add("@p_ME_IsCriticalStageApplicable", emp.IsCriticalStageApplicable);
                    parameters.Add("@p_ME_EmployeeCriticalStageID", emp.EmployeeCriticalStageID);
                    parameters.Add("@p_ME_JoiningDate", emp.JoiningDate);
                    parameters.Add("@p_ME_IsJoiningSpecial", emp.IsJoiningSpecial);
                    parameters.Add("@p_ME_JoiningStatus", emp.JoiningStatus);
                    parameters.Add("@p_ME_TraineeDueDate", emp.TraineeDueDate);
                    parameters.Add("@p_ME_ProbationDueDate", emp.ProbationDueDate);
                    parameters.Add("@p_ME_ConfirmationDate", emp.ConfirmationDate);
                    parameters.Add("@p_ME_IsConfirmation", emp.IsConfirmation);
                    parameters.Add("@p_ME_ConfirmationBy", emp.ConfirmationBy);
                    parameters.Add("@p_ME_CompanyMobileNo", emp.CompanyMobileNo);
                    parameters.Add("@p_ME_CompanyMailID", emp.CompanyMailID);
                    parameters.Add("@p_ME_ReportingHR", emp.ReportingHR);
                    parameters.Add("@p_ME_ReportingManager1", emp.ReportingManager1);
                    parameters.Add("@p_ME_ReportingManager2", emp.ReportingManager2);
                    parameters.Add("@p_ME_ReportingAccount", emp.ReportingAccount);
                    parameters.Add("@p_ME_NoOfBranchTransfer", emp.NoOfBranchTransfer);
                    parameters.Add("@p_ME_IsReasigned", emp.IsReasigned);
                    parameters.Add("@p_ME_ResignationDate", emp.ResignationDate);
                    parameters.Add("@p_ME_NoticePeriodDays", emp.NoticePeriodDays);
                    parameters.Add("@p_ME_IsExit", emp.IsExit);
                    parameters.Add("@p_ME_ExitDate", emp.ExitDate);
                    parameters.Add("@p_ME_EmployeeLeft", emp.EmployeeLeft);
                    parameters.Add("@p_ME_LeavingDate", emp.LeavingDate);
                    parameters.Add("@p_ME_LeavingReason", emp.LeavingReason);
                    parameters.Add("@p_ME_LeavingReasonPF", emp.LeavingReasonPF);
                    parameters.Add("@p_ME_LeavingReasonESI", emp.LeavingReasonESI);
                    parameters.Add("@p_ME_EM_PayrollLastDate", emp.EM_PayrollLastDate);
                    parameters.Add("@p_ME_EM_Atten_DailyMonthly", emp.EM_Atten_DailyMonthly);
                    parameters.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                    parameters.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                    parameters.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                    var result = DapperORM.ExecuteReturn("sp_SUD_EmployeeImportExport", parameters);
                    TempData["Message"] = parameters.Get<string>("@p_msg");
                    TempData["Icon"] = parameters.Get<string>("@p_Icon");
                    string p_Id = parameters.Get<string>("@p_Id");

                    StringBuilder strBuilder = new StringBuilder();
                    string qry = "";
                    var machineName = Dns.GetHostName().Replace("'", "''");
                    strBuilder.Append(qry);
                    //============================================ Start Insert For Mas_Employee_ESS_Import================================================

                    qry = "INSERT INTO dbo.Mas_Employee_ESS_Import " +
                   "(Deactivate, UseBy, CreatedBy, CreatedDate, MachineName, ESSEmployeeId, ESSLoginID, ESSPassword, " +
                   "ESSSecurityQuestion, ESSAnswer, ESSIsActive, ESSIsLock, ESSLoginAttemptCount, IsExit, UserAccessPolicyId, " +
                   "IsApp, MFAEnabled, IsMFA, MFAToken) " +
                   "VALUES (" +
                   "0, " +
                   "0, " +
                   "'" + Session["EmployeeName"] + "', " +
                   "GETDATE(), " +
                   "'" + machineName + "', " +
                     p_Id + ", " +
                     emp.ESSLoginID + "," +
                    "'" + emp.ESSPassword + "', " +
                   "'" + emp.ESSSecurityQuestion + "', " +
                   "'" + emp.ESSAnswer + "', " +
                   Convert.ToInt32(emp.ESSIsActive) + ", " +
                   Convert.ToInt32(emp.ESSIsLock) + ", " +
                   emp.ESSLoginAttemptCount + ", " +
                   Convert.ToInt32(emp.IsExit) + ", " +
                   emp.UserAccessPolicyId + ", " +
                   Convert.ToInt32(emp.IsApp) + ", " +
                   Convert.ToInt32(emp.MFAEnabled) + ", " +
                   Convert.ToInt32(emp.IsMFA) + ", " +
                   "'" + emp.MFAToken + "'" +
                   ");";
                    strBuilder.AppendLine(qry);

                    qry = "UPDATE Mas_Employee_ESS_Import " +
                          "SET ESSId_Encrypted = master.dbo.fn_varbintohexstr(HashBytes('SHA2_256', CONVERT(NVARCHAR(70), ESSEmployeeId))) " +
                          "WHERE ESSEmployeeId = " + p_Id + ";";

                    strBuilder.AppendLine(qry);
                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {
                        TempData["Message"] = "Excel data uploaded and saved successfully!";
                        TempData["Icon"] = "success";
                    }
                    else
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                    //============================================ END Insert For Mas_Employee_ESS_Import================================================
                    //============================================ Start Insert For Mas_Employee_Personl================================================
                    StringBuilder strPer = new StringBuilder();
                    strBuilder.Append(qry);
                    qry = "INSERT INTO dbo.Mas_Employee_Personal_Import " +
                   "(Deactivate, UseBy, CreatedBy, CreatedDate, MachineName, PersonalEmployeeId, AadhaarNo, NameAsPerAadhaar, " +
                   "AadhaarNoMobileNoLink, AadhaarNoMobileNo, PAN, NameAsPerPan, PANAadhaarLink, PrimaryMobile, SecondaryMobile, " +
                   "WhatsAppNo, PersonalEmailId, BirthdayDate, AgeOfJoining, BirthdayPlace, BirthdayProofOfDocumentID, " +
                   "BirthdayProofOfCertificateNo, IsDOBSpecial, EmployeeQualificationID, QualificationRemark, Gender, BloodGroup, " +
                   "MaritalStatus, AnniversaryDate, Ifyouwantdonotdisclosemygenderthentick, PhysicallyDisabled, PhysicallyDisableType, " +
                   "PhysicallyDisableRemark, IdentificationMark, DrivingLicenceNo, DrivingLicenceExpiryDate, PassportNo, " +
                   "PassportExpiryDate, EmployeeReligionID, Ifyouwantdonotdisclosemyreligioncastthentick, EmployeeCasteID, " +
                   "EmployeeSpecificDegree, EmployeeBirthProofEducation, EmployeeSubCategory) " +
                   "VALUES (" +
                   "0, " +
                   "0, " +
                   "'" + Session["EmployeeName"] + "', " +
                   "GETDATE(), " +
                   "'" + machineName + "', " +
                   p_Id + ", " +
                   "'" + emp.AadhaarNo + "', " +
                   "'" + emp.NameAsPerAadhaar + "', " +
                   Convert.ToInt32(emp.AadhaarNoMobileNoLink) + ", " +
                   "'" + emp.AadhaarNoMobileNo + "', " +
                   "'" + emp.PAN + "', " +
                   "'" + emp.NameAsPerPan + "', " +
                   Convert.ToInt32(emp.PANAadhaarLink) + ", " +
                   "'" + emp.PrimaryMobile + "', " +
                   "'" + emp.SecondaryMobile + "', " +
                   "'" + emp.WhatsAppNo + "', " +
                   "'" + emp.PersonalEmailId + "', " +
                   (emp.BirthdayDate.HasValue
                       ? "'" + emp.BirthdayDate.Value.ToString("yyyy-MM-dd") + "'"
                       : "NULL") + ", " +
                   "'" + emp.AgeOfJoining + "', " +
                   "'" + emp.BirthdayPlace + "', " +
                   emp.BirthdayProofOfDocumentID + ", " +
                   "'" + emp.BirthdayProofOfCertificateNo + "', " +
                   Convert.ToInt32(emp.IsDOBSpecial) + ", " +
                   emp.EmployeeQualificationID + ", " +
                   "'" + emp.QualificationRemark + "', " +
                   "'" + emp.Gender + "', " +
                   "'" + emp.BloodGroup + "', " +
                   "'" + emp.MaritalStatus + "', " +
                   "'" + emp.AnniversaryDate + "', " +
                   Convert.ToInt32(emp.Ifyouwantdonotdisclosemygenderthentick) + ", " +
                   "'" + emp.PhysicallyDisabled + "', " +
                   "'" + emp.PhysicallyDisableType + "', " +
                   "'" + emp.PhysicallyDisableRemark + "', " +
                   "'" + emp.IdentificationMark + "', " +
                   "'" + emp.DrivingLicenceNo + "', " +
                   "'" + emp.DrivingLicenceExpiryDate + "', " +
                   "'" + emp.PassportNo + "', " +
                   "'" + emp.PassportExpiryDate + "', " +
                   emp.EmployeeReligionID + ", " +
                   Convert.ToInt32(emp.Ifyouwantdonotdisclosemyreligioncastthentick) + ", " +
                   emp.EmployeeCasteID + ", " +
                   "'" + emp.EmployeeSpecificDegree + "', " +
                   "'" + emp.EmployeeBirthProofEducation + "', " +
                   "'" + emp.EmployeeSubCategory + "'" +
                   ");";

                    strPer.AppendLine(qry);
                    qry = "UPDATE Mas_Employee_Personal_Import " + "SET PersonalId_Encrypted = master.dbo.fn_varbintohexstr(HashBytes('SHA2_256', CONVERT(NVARCHAR(70), PersonalEmployeeId))) " + "WHERE PersonalEmployeeId = " + p_Id + ";";

                    strPer.AppendLine(qry);
                    //string abc = "";
                    if (objcon.SaveStringBuilder(strPer, out abc))
                    {
                        TempData["Message"] = "Excel data uploaded and saved successfully!";
                        TempData["Icon"] = "success";
                    }
                    else
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }

                    //============================================ END Insert For Mas_Employee_Personl================================================

                    //============================================ Start Insert For Mas_Employee_Address_Import ================================================
                    StringBuilder strAdd = new StringBuilder();
                    strBuilder.Append(qry);

                    qry = "INSERT INTO dbo.Mas_Employee_Address_Import " +
                   "(Deactivate,UseBy,CreatedBy,CreatedDate,MachineName,AddressEmployeeId,PresentPin,PresentState, " +
                   "PresentDistrict,PresentTaluka,PresentPO,PresentCity,PresentPostelAddress,PermanentPin,PermanentState,PermanentDistrict,PermanentTaluka, " +
                   "PermanentPO,PermanentCity,PermanentAddressSameAsCurrentAddress,PermanentPostelAddress)" +
                   "VALUES (" +
                   "0, " +
                   "0, " +
                   "'" + Session["EmployeeName"] + "', " +
                   "GETDATE(), " +
                   "'" + machineName + "', " +
                   p_Id + ", " +
                   "'" + emp.PresentPin + "', " +
                   "'" + emp.PresentState + "', " +
                   "'" + emp.PresentDistrict + "', " +
                   "'" + emp.PresentTaluka + "', " +
                   "'" + emp.PresentPO + "', " +
                   "'" + emp.PresentCity + "', " +
                   "'" + emp.PresentPostelAddress + "', " +
                   "'" + emp.PermanentPin + "', " +
                   "'" + emp.PermanentState + "', " +
                   "'" + emp.PermanentDistrict + "', " +
                   "'" + emp.PermanentTaluka + "', " +
                   "'" + emp.PermanentPO + "', " +
                "'" + emp.PermanentCity + "', " +
                   Convert.ToInt32(emp.PermanentAddressSameAsCurrentAddress) + ", " +
                "'" + emp.PermanentPostelAddress + "'" +
                 ");";

                    strAdd.AppendLine(qry);
                    qry = "UPDATE Mas_Employee_Address_Import " + "SET AddressId_Encrypted = master.dbo.fn_varbintohexstr(HashBytes('SHA2_256', CONVERT(NVARCHAR(70), AddressEmployeeId))) " + "WHERE AddressEmployeeId = " + p_Id + ";";

                    strAdd.AppendLine(qry);
                    if (objcon.SaveStringBuilder(strAdd, out abc))
                    {
                        TempData["Message"] = "Excel data uploaded and saved successfully!";
                        TempData["Icon"] = "success";
                    }
                    else
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                    //============================================ END Insert For Mas_Employee_Address_Import ================================================
                    //============================================ Start Insert For Mas_Employee_Statutory_Import ================================================

                    StringBuilder strSta = new StringBuilder();
                    strBuilder.Append(qry);

                    qry = "INSERT INTO dbo.Mas_Employee_Statutory_Import " +
                   "(Deactivate,UseBy,CreatedBy,CreatedDate,MachineName,StatutoryEmployeeId,ESIC_Applicable," +
                   "ESIC_SettingID,ESIC_NO,ESIC_ClosingDate,ESIC_PreviousESICNo,ESIC_IS_LinkWithESIC,PT_Applicable,PT_SettingID,LWF_Applicable,LWF_SettingID,LWF_LIN, " +
                   "Gratuity_Applicable,Gratuity_No,PF_Applicable,PF_SettingID,PF_Limit,PF_EPS,PF_VPF,PF_FSType,PF_FS_Name,PF_UAN,PF_NO,PF_Nominee1, " +
                   "PF_Reletion1,PF_DOB1,PF_Share1,PF_Address1,PF_GuardianName1,PF_Nominee2,PF_Reletion2,PF_DOB2,PF_Share2,PF_Address2,PF_GuardianName2,PF_Nominee3,PF_Reletion3," +
                   "PF_DOB3,PF_Share3,PF_Address3,PF_GuardianName3,PF_MobileNo,PF_BankName,PF_BankIFSC,PF_Account,PF_1952,PF_1995,PF_PreviousPFNo,PF_ExitDate," +
                  "PF_CertificateNo,PF_PPO,PF_OldUANNo,PF_LinkWithUAN,ESIC_CodeId,PT_CodeId,PTSlab_MasterId,LWF_CodeId,LWFSlab_MasterId,PF_CodeId,PFWages_MasterId)" +
                   "VALUES (" +
                   "0, " +
                   "0, " +
                   "'" + Session["EmployeeName"] + "', " +
                   "GETDATE(), " +
                   "'" + machineName + "', " +
                   p_Id + ", " +
                    Convert.ToInt32(emp.ESIC_Applicable) + ", " +
                   emp.ESIC_SettingID + ", " +
                   "'" + emp.ESIC_NO + "', " +
                   "'" + emp.ESIC_ClosingDate + "', " +
                   "'" + emp.ESIC_PreviousESICNo + "', " +
                  Convert.ToInt32(emp.ESIC_IS_LinkWithESIC) + ", " +
                    Convert.ToInt32(emp.PT_Applicable) + ", " +
                 emp.PT_SettingID + ", " +
                   Convert.ToInt32(emp.LWF_Applicable) + ", " +
                   emp.LWF_SettingID + ", " +
                     "'" + emp.LWF_LIN + "', " +
                      Convert.ToInt32(emp.Gratuity_Applicable) + ", " +
                       "'" + emp.Gratuity_No + "', " +
                       Convert.ToInt32(emp.PF_Applicable) + ", " +
                       emp.PF_SettingID + ", " +
                       Convert.ToInt32(emp.PF_Limit) + ", " +
                        Convert.ToInt32(emp.PF_EPS) + ", " +
                        "'" + emp.PF_VPF + "', " +
                         "'" + emp.PF_FSType + "', " +
                          "'" + emp.PF_FS_Name + "', " +
                           "'" + emp.PF_UAN + "', " +
                            "'" + emp.PF_NO + "', " +
                            "'" + emp.PF_Nominee1 + "', " +
                            "'" + emp.PF_Reletion1 + "', " +
                            "'" + emp.PF_DOB1 + "', " +
                            "'" + emp.PF_Share1 + "', " +
                            "'" + emp.PF_Address1 + "', " +
                            "'" + emp.PF_GuardianName1 + "', " +
                            "'" + emp.PF_Nominee2 + "', " +
                            "'" + emp.PF_Reletion2 + "', " +
                            "'" + emp.PF_DOB2 + "', " +
                            "'" + emp.PF_Share2 + "', " +
                            "'" + emp.PF_Address2 + "', " +
                            "'" + emp.PF_GuardianName2 + "', " +
                            "'" + emp.PF_Nominee3 + "', " +
                            "'" + emp.PF_Reletion3 + "', " +
                            "'" + emp.PF_DOB3 + "', " +
                            "'" + emp.PF_Share3 + "', " +
                            "'" + emp.PF_Address3 + "', " +
                            "'" + emp.PF_GuardianName3 + "', " +
                            "'" + emp.PF_MobileNo + "', " +
                            "'" + emp.PF_BankName + "', " +
                            "'" + emp.PF_BankIFSC + "', " +
                            "'" + emp.PF_Account + "', " +
                            Convert.ToInt32(emp.PF_1952) + ", " +
                            Convert.ToInt32(emp.PF_1995) + ", " +
                             "'" + emp.PF_PreviousPFNo + "', " +
                             "'" + emp.PF_ExitDate + "', " +
                              "'" + emp.PF_CertificateNo + "', " +
                               "'" + emp.PF_PPO + "', " +
                               Convert.ToInt32(emp.PF_OldUANNo) + ", " +
                               Convert.ToInt32(emp.PF_LinkWithUAN) + ", " +
                                emp.ESIC_CodeId + ", " +
                                emp.PT_CodeId + ", " +
                                emp.PTSlab_MasterId + ", " +
                                emp.LWF_CodeId + ", " +
                                emp.LWFSlab_MasterId + ", " +
                                emp.PF_CodeId + ", " +
                                emp.PFWages_MasterId + ");";

                    strSta.AppendLine(qry);
                    qry = "UPDATE Mas_Employee_Statutory_Import " + "SET StatutoryId_Encrypted = master.dbo.fn_varbintohexstr(HashBytes('SHA2_256', CONVERT(NVARCHAR(70), StatutoryEmployeeId))) " + "WHERE StatutoryEmployeeId = " + p_Id + ";";

                    strSta.AppendLine(qry);
                    if (objcon.SaveStringBuilder(strSta, out abc))
                    {
                        TempData["Message"] = "Excel data uploaded and saved successfully!";
                        TempData["Icon"] = "success";
                    }
                    else
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                    //============================================ END Insert For Mas_Employee_Statutory_Import ================================================
                    //============================================ Start Insert For Mas_Employee_Attendance_Import ================================================

                    StringBuilder strAtt = new StringBuilder();
                    strBuilder.Append(qry);
                   
                    qry = "INSERT INTO dbo.Mas_Employee_Attendance_Import (" +
                   "Deactivate, UseBy, CreatedBy, CreatedDate, MachineName, AttendanceEmployeeId, EmployeeCardNo, " +
                   "EM_Atten_OT_Applicable, EM_Atten_OTMultiplyBy, EM_Atten_PerDayShiftHrs, EM_Atten_CoffApplicable, EM_Atten_CoffSettingId, " +
                   "EM_Atten_ShiftGroupId, EM_Atten_ShiftRuleId, EM_Atten_WOFF1, EM_Atten_WOFF2, EM_Atten_WOFF2_ForCheck, " +
                   "EM_Atten_WOFF_Check1, EM_Atten_WOFF_Check2, EM_Atten_WOFF_Check3, EM_Atten_WOFF_Check4, EM_Atten_WOFF_Check5, " +
                   "EM_Atten_LateMarkSettingApplicable, EM_Atten_LateMarkSettingId, EM_Atten_IsSalaryFullPay, EM_Atten_LeaveGroupId, " +
                   "EM_Atten_DefaultAttenShow, EM_Atten_SinglePunch_Present, EM_Atten_RotationalWeekOff, EM_Atten_Regularization_Required, " +
                   "EM_Atten_ShortLeaveApplicable, EM_Atten_ShortLeaveSettingId, EM_Atten_PersonalGatepassApplicable, EM_Atten_Atten_PersonalGatepassSettingId, " +
                   " EM_Atten_PunchMissingApplicable, EM_Atten_Atten_PunchMissingSettingId, EM_Atten_flexibleShiftApplicable, " +
                   "EM_Atten_Atten_OutDoorCompanySettingId, EM_Atten_OutDoorCompanyApplicable, EM_Atten_WOPH_CoffApplicable, PHApplicable, " +
                   "EM_Atten_ShortHRS_Applicable, EM_Atten_LocationRegistrationMappingIMasterId, EM_Atten_MonthlyRosterApplicable" +
                    ") VALUES (" +
                   "0, " +                               
                   "0, " +                                
                   "'" + Session["EmployeeName"] + "', " +
                   "GETDATE(), " +                      
                   "'" + machineName + "', " +           
                   p_Id + ", " +                           
                   (string.IsNullOrEmpty(emp.EmployeeCardNo) ? "NULL" : "'" + emp.EmployeeCardNo + "'") + ", " +
                   Convert.ToInt32(emp.EM_Atten_OT_Applicable) + ", " +
                   emp.EM_Atten_OTMultiplyBy + ", " +
                   Convert.ToInt32(emp.EM_Atten_PerDayShiftHrs) + ", " +
                   Convert.ToInt32(emp.EM_Atten_CoffApplicable) + ", " +
                   Convert.ToInt32(emp.EM_Atten_CoffSettingId) + ", " +
                   emp.EM_Atten_ShiftGroupId + ", " +
                   emp.EM_Atten_ShiftRuleId + ", " +
                   "'" + (emp.EM_Atten_WOFF1 ?? "") + "', " +
                   "'" + (emp.EM_Atten_WOFF2 ?? "") + "', " +
                   "'" + (emp.EM_Atten_WOFF2_ForCheck ?? "") + "', " +
                   Convert.ToInt32(emp.EM_Atten_WOFF_Check1) + ", " +
                   Convert.ToInt32(emp.EM_Atten_WOFF_Check2) + ", " +
                   Convert.ToInt32(emp.EM_Atten_WOFF_Check3) + ", " +
                   Convert.ToInt32(emp.EM_Atten_WOFF_Check4) + ", " +
                   Convert.ToInt32(emp.EM_Atten_WOFF_Check5) + ", " +
                   Convert.ToInt32(emp.EM_Atten_LateMarkSettingApplicable) + ", " +
                     Convert.ToInt32(emp.EM_Atten_LateMarkSettingId) + ", " +
                   (emp.EM_Atten_IsSalaryFullPay ? "1" : "0") + ", " +
                   emp.EM_Atten_LeaveGroupId + ", " +
                   Convert.ToInt32(emp.EM_Atten_DefaultAttenShow) + ", " +
                   Convert.ToInt32(emp.EM_Atten_SinglePunch_Present) + ", " +
                   Convert.ToInt32(emp.EM_Atten_RotationalWeekOff) + ", " +
                   Convert.ToInt32(emp.EM_Atten_Regularization_Required) + ", " +
                   Convert.ToInt32(emp.EM_Atten_ShortLeaveApplicable) + ", " +
                   emp.EM_Atten_ShortLeaveSettingId + ", " +
                   Convert.ToInt32(emp.EM_Atten_PersonalGatepassApplicable) + ", " +
                   emp.EM_Atten_Atten_PersonalGatepassSettingId + ", " +
                   Convert.ToInt32(emp.EM_Atten_PunchMissingApplicable) + ", " +
                   emp.EM_Atten_Atten_PunchMissingSettingId + ", " +
                   Convert.ToInt32(emp.EM_Atten_flexibleShiftApplicable) + ", " +
                   emp.EM_Atten_Atten_OutDoorCompanySettingId + ", " +
                   Convert.ToInt32(emp.EM_Atten_OutDoorCompanyApplicable) + ", " +
                   Convert.ToInt32(emp.EM_Atten_WOPH_CoffApplicable) + ", " +
                   Convert.ToInt32(emp.PHApplicable) + ", " +
                   Convert.ToInt32(emp.EM_Atten_ShortHRS_Applicable) + ", " +
                   emp.EM_Atten_LocationRegistrationMappingIMasterId + ", " +
                   Convert.ToInt32(emp.EM_Atten_MonthlyRosterApplicable) +
               ");";

                    strAtt.AppendLine(qry);
                    qry = "UPDATE Mas_Employee_Attendance_Import " + "SET AttendanceId_Encrypted = master.dbo.fn_varbintohexstr(HashBytes('SHA2_256', CONVERT(NVARCHAR(70), AttendanceId))) " + "WHERE AttendanceId = " + p_Id + ";";

                    strAtt.AppendLine(qry);
                    if (objcon.SaveStringBuilder(strAtt, out abc))
                    {
                        TempData["Message"] = "Excel data uploaded and saved successfully!";
                        TempData["Icon"] = "success";
                    }
                    else
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }

                    //============================================ END Insert For Mas_Employee_Attendance_Import ================================================
                    //============================================ Start Insert For Mas_Employee_Bank_Import ================================================

                    StringBuilder strBank = new StringBuilder();
                    strBuilder.Append(qry);

                    qry = "INSERT INTO dbo.Mas_Employee_Bank_Import " +
                   "(Deactivate,UseBy,CreatedBy,CreatedDate,MachineName,EmployeeBankEmployeeId,SalaryIFSC," +
                   "SalaryBankName,SalaryAccountNo,SalaryConfirmAccountNo,SalaryBankAddress,SalaryBankBranchName,SalaryNameAsPerBank,PermanentIFSC," +
                    "PermanentBankName,PermanentAccountNo,PermanentConfirmAccountNo,PermanentBankAddress,PermanentBankBranchName,PermanentNameAsPerBank,SalaryMode)" +
                   "VALUES (" +
                   "0, " +
                   "0, " +
                   "'" + Session["EmployeeName"] + "', " +
                   "GETDATE(), " +
                   "'" + machineName + "', " +
                   p_Id + ", " +
                   "'" + emp.SalaryIFSC + "', " +
                   "'" + emp.SalaryBankName + "', " +
                   "'" + emp.SalaryAccountNo + "', " +
                    "'" + emp.SalaryConfirmAccountNo + "', " +
                     "'" + emp.SalaryBankAddress + "', " +
                      "'" + emp.SalaryBankBranchName + "', " +
                       "'" + emp.SalaryNameAsPerBank + "', " +
                        "'" + emp.PermanentIFSC + "', " +
                         "'" + emp.PermanentBankName + "', " +
                          "'" + emp.PermanentAccountNo + "', " +
                         "'" + emp.PermanentConfirmAccountNo + "', " +
                         "'" + emp.PermanentBankAddress + "', " +
                         "'" + emp.PermanentBankBranchName + "', " +
                         "'" + emp.PermanentNameAsPerBank + "', " +
                     "'" + emp.SalaryMode + "' " +
                      ");";

                    strBank.AppendLine(qry);
                    qry = "UPDATE Mas_Employee_Bank_Import " +
                      "SET EmployeeBankId_Encrypted = master.dbo.fn_varbintohexstr(" + "HashBytes('SHA2_256', CONVERT(NVARCHAR(70), EmployeeBankEmployeeId))) " + "WHERE EmployeeBankEmployeeId = " + p_Id + ";";

                    strBank.AppendLine(qry);
                    if (objcon.SaveStringBuilder(strBank, out abc))
                    {
                        TempData["Message"] = "Excel data uploaded and saved successfully!";
                        TempData["Icon"] = "success";
                    }
                    else
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }

                }
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


    }
}