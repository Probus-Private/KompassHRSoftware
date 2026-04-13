using ClosedXML.Excel;
using Dapper;
using DocumentFormat.OpenXml.Drawing.Charts;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_BulkInsert_PremiumController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        //clsCommonFunction objcon = new clsCommonFunction();
        // GET: Module/Module_Employee_BulkInsert_Premium

        #region Main View
        public ActionResult Module_Employee_BulkInsert_Premium(Module_Employee_BulkInsert_Premium obj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 779;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.ComapnyName = GetComapnyName;
                var GetCmpId = GetComapnyName[0]?.Id;

                DynamicParameters paramBranch = new DynamicParameters();
                paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                paramBranch.Add("@p_CmpId", GetCmpId);
                var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                ViewBag.BranchName = BranchName;
                return View(obj);
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
        public ActionResult Module_Employee_BulkInsert_Premium(HttpPostedFileBase AttachFile, Module_Employee_BulkInsert_Premium obj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.ComapnyName = GetComapnyName;

                DynamicParameters paramBranch = new DynamicParameters();
                paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                paramBranch.Add("@p_CmpId", obj.CompanyName);
                var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                ViewBag.BranchName = BranchName;

                if (AttachFile == null || AttachFile.ContentLength == 0)
                {
                    TempData["Message"] = "Please upload a valid Excel file.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("Module_Employee_BulkInsert", "Module_Employee_BulkInsert");
                }

                var excelDataList = new List<Module_Employee_BulkInsert_Premium>();

                using (var workbook = new XLWorkbook(AttachFile.InputStream))
                {
                    var ws = workbook.Worksheet(1);
                    var rows = ws.RangeUsed().RowsUsed().Skip(1);

                    foreach (var row in rows)
                    {
                        if (row.Cell(1).IsEmpty()) break;

                        var BulkInsert = new Module_Employee_BulkInsert_Premium
                        {
                            SrNo = row.Cell(1).GetValue<int>(),
                            CompanyName = row.Cell(2).GetString(),
                            BranchName = row.Cell(3).GetString(),
                            ContractorName = row.Cell(4).GetString(),
                            Salutation = row.Cell(5).GetString(),
                            EmployeeName = row.Cell(6).GetString(),
                            EmployeeNo = row.Cell(7).GetString(),
                            EmployeeCardNo = row.Cell(8).GetString(),
                            Designation = row.Cell(9).GetString(),

                            AadhaarNo = row.Cell(10).GetString(),
                            NameAsPerAadhaar = row.Cell(11).GetString(),
                            PrimaryMobile = row.Cell(12).GetString(),
                            Gender = row.Cell(13).GetString(),
                            BirthDate = row.Cell(14).IsEmpty() ? "" : row.Cell(14).GetDateTime().ToString("yyyy-MM-dd"),
                            MaritalStatus = row.Cell(15).GetString(),
                            HighestQualification = row.Cell(16).GetString(),
                            JoiningDate = row.Cell(17).IsEmpty() ? "" : row.Cell(17).GetDateTime().ToString("yyyy-MM-dd"),
                            ShiftGroup = row.Cell(18).GetString(),
                            ShiftRule = row.Cell(19).GetString(),
                            CanteenApplicable = row.Cell(20).GetString() == "Yes" ? 1 : 0,
                            Pincode = row.Cell(21).GetString(),
                            StateName = row.Cell(22).GetString(),
                            DistrictName = row.Cell(23).GetString(),
                            TalukaName = row.Cell(24).GetString(),
                            POName = row.Cell(25).GetString(),
                            CityName = row.Cell(26).GetString(),
                            PostalAddress = row.Cell(27).GetString(),
                            PayType = row.Cell(28).GetString(),
                            Rate = row.Cell(29).GetString(),
                            AttendanceBonus = row.Cell(30).GetString(),
                            AccountNo = row.Cell(31).GetString(),
                            BankName = row.Cell(32).GetString(),
                            IFSCCode = row.Cell(33).GetString()


                            //Department = row.Cell(9).GetString(),
                            //SubDepartment = row.Cell(10).GetString(),
                            //ManpowerCategory = row.Cell(11).GetString(),
                            //Grade = row.Cell(13).GetString(),
                            //CriticalStageApplicable = row.Cell(14).GetString(),
                            //CriticalStageCategory = row.Cell(15).GetString(),
                            //LineMaster = row.Cell(16).GetString(),
                            //AssessmentLevel = row.Cell(17).GetString(),
                            //WeeklyOff = row.Cell(27).GetString(),
                            //PFNo = row.Cell(31).GetString(),
                            //RelationType = row.Cell(32).GetString(),
                            //RelationName = row.Cell(33).GetString(),
                            //UANNo = row.Cell(34).GetString(),
                            //ESICNo = row.Cell(35).GetString(),
                            //NomineeRelation = row.Cell(36).GetString(),
                            //NomineeName = row.Cell(37).GetString(),
                            //SubUnit = row.Cell(10).GetString(),
                            
                            

                        };

                        excelDataList.Add(BulkInsert);
                    }
                }

                ViewBag.count = excelDataList.Count;
                ViewBag.GetExceldata = excelDataList;

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
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                return Json(new { data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Bulk Insert
        [HttpPost]
        public ActionResult SaveUpdate(SaveBulkInsertRequest Gettbldata, string CmpId, string BranchId)
        {
            try
            {
                // Aadhaar validation regex: only 12 digits
                var aadhaarRegex = new System.Text.RegularExpressions.Regex(@"^\d{12}$");

                // Validation before creating DataTable
                foreach (var item in Gettbldata.tbldata)
                {
                    // Aadhaar required check
                    if (string.IsNullOrWhiteSpace(item.AadhaarNo))
                    {
                        return Json(new { Icon = "error", Message = $"Aadhaar number is required for sr.no. {item.SrNo}" });
                    }
                    // Aadhaar format check (12 digit numeric)
                    if (!aadhaarRegex.IsMatch(item.AadhaarNo))
                    {
                        return Json(new { Icon = "error", Message = $"Invalid Aadhaar number for sr.no. {item.SrNo}. Aadhaar must be 12 digits." });
                    }
                }

                // --- Aadhaar Duplicate Validation ---
                var aadhaarList = Gettbldata.tbldata.Select(x => x.AadhaarNo).Where(x => !string.IsNullOrEmpty(x)).ToList();
                if (aadhaarList.Count > 0)
                {
                    //var existingAadhaars = DapperORM.DynamicQueryListWithParam("SELECT AadhaarNo FROM Mas_Employee_Personal  WHERE Deactivate=0 And AadhaarNo IN @Values",
                    //    new { Values = aadhaarList }
                    //);
                    var existingAadhaars = DapperORM.DynamicQueryListWithParam($@"SELECT P.AadhaarNo FROM Mas_Employee_Personal P
                                            INNER JOIN Mas_Employee E ON P.PersonalEmployeeId = E.EmployeeId WHERE P.Deactivate = 0 
                                            AND E.EmployeeLeft != 1 AND P.AadhaarNo IN @Values",
                                            new { Values = aadhaarList }
                                            );
                    if (existingAadhaars != null && existingAadhaars.Any())
                    {
                        var duplicateNos = existingAadhaars.Select(a => (string)a.AadhaarNo).ToList();
                        // Find SrNo of duplicates in uploaded data
                        var duplicateSrNos = Gettbldata.tbldata
                            .Where(x => duplicateNos.Contains(x.AadhaarNo)).Select(x => x.SrNo).ToList();
                        return Json(new { Icon = "error", Message = $"Aadhaar already exists for Sr.No. {string.Join(", ", duplicateSrNos)}" });
                    }
                }

                // --- EmployeeNo Duplicate Validation ---
                var employeenoList = Gettbldata.tbldata.Select(x => x.EmployeeNo).Where(x => !string.IsNullOrEmpty(x)).ToList();
                if (employeenoList.Count > 0)
                {
                    var existingEmployeeno = DapperORM.DynamicQueryListWithParam("SELECT EmployeeNo FROM Mas_Employee WHERE Deactivate=0 And EmployeeNo IN @Values",
                        new { Values = employeenoList }
                    );
                    if (existingEmployeeno != null && existingEmployeeno.Any())
                    {
                        var duplicateNos = existingEmployeeno.Select(e => (string)e.EmployeeNo).ToList();
                        var duplicateSrNos = Gettbldata.tbldata
                            .Where(x => duplicateNos.Contains(x.EmployeeNo)).Select(x => x.SrNo).ToList();
                        return Json(new { Icon = "error", Message = $"Employee no. already exists for Sr.No(s): {string.Join(", ", duplicateSrNos)}" });
                    }
                }

                // --- EmployeeCardNo Duplicate Validation ---
                var employeecardnoList = Gettbldata.tbldata.Select(x => x.EmployeeCardNo).Where(x => !string.IsNullOrEmpty(x)).ToList();
                if (employeecardnoList.Count > 0)
                {
                    var existingEmployeecardno = DapperORM.DynamicQueryListWithParam("SELECT EmployeeCardNo FROM Mas_Employee WHERE Deactivate=0 And EmployeeCardNo IN @Values",
                        new { Values = employeecardnoList }
                    );
                    if (existingEmployeecardno != null && existingEmployeecardno.Any())
                    {
                        var duplicateNos = existingEmployeecardno.Select(e => (string)e.EmployeeCardNo).ToList();
                        var duplicateSrNos = Gettbldata.tbldata
                            .Where(x => duplicateNos.Contains(x.EmployeeCardNo)).Select(x => x.SrNo).ToList();
                        return Json(new { Icon = "error", Message = $"Employee card no. already exists for Sr.No(s): {string.Join(", ", duplicateSrNos)}" });
                    }
                }


                // Create DataTable
                System.Data.DataTable dt = new System.Data.DataTable();
                dt.Columns.Add("SrNo", typeof(int));
                dt.Columns.Add("CompanyName", typeof(string));
                dt.Columns.Add("BranchName", typeof(string));
                dt.Columns.Add("ContractorName", typeof(string));
                dt.Columns.Add("Salutation", typeof(string));
                dt.Columns.Add("EmployeeName", typeof(string));
                dt.Columns.Add("EmployeeNo", typeof(string));
                dt.Columns.Add("EmployeeCardNo", typeof(string));
                dt.Columns.Add("Designation", typeof(string));

                dt.Columns.Add("AadhaarNo", typeof(string));
                dt.Columns.Add("NameAsPerAadhaar", typeof(string));
                dt.Columns.Add("PrimaryMobile", typeof(string));
                dt.Columns.Add("Gender", typeof(string));
                dt.Columns.Add("BirthDate", typeof(string));
                dt.Columns.Add("MaritalStatus", typeof(string));
                dt.Columns.Add("HighestQualification", typeof(string));
                dt.Columns.Add("JoiningDate", typeof(string));
                dt.Columns.Add("ShiftGroup", typeof(string));
                dt.Columns.Add("ShiftRule", typeof(string));
                dt.Columns.Add("CanteenApplicable", typeof(int));
                dt.Columns.Add("Pincode", typeof(string));
                dt.Columns.Add("StateName", typeof(string));
                dt.Columns.Add("DistrictName", typeof(string));
                dt.Columns.Add("TalukaName", typeof(string));
                dt.Columns.Add("POName", typeof(string));
                dt.Columns.Add("CityName", typeof(string));
                dt.Columns.Add("PostalAddress", typeof(string));
                dt.Columns.Add("PayType", typeof(string));
                dt.Columns.Add("Rate", typeof(string));
                dt.Columns.Add("AttendanceBonus", typeof(string));
                dt.Columns.Add("AccountNo", typeof(string));
                dt.Columns.Add("BankName", typeof(string));
                dt.Columns.Add("IFSCCode", typeof(string));


                //dt.Columns.Add("SubUnit", typeof(string));
                //dt.Columns.Add("Department", typeof(string));
                //dt.Columns.Add("SubDepartment", typeof(string));
                //dt.Columns.Add("ManpowerCategory", typeof(string));
                //dt.Columns.Add("Grade", typeof(string));
                //dt.Columns.Add("CriticalStageApplicable", typeof(string));
                //dt.Columns.Add("CriticalStageCategory", typeof(string));
                //dt.Columns.Add("LineMaster", typeof(string));
                //dt.Columns.Add("AssessmentLevel", typeof(string));
                //dt.Columns.Add("WeeklyOff", typeof(string));
                //dt.Columns.Add("PFNo", typeof(string));
                //dt.Columns.Add("RelationType", typeof(string));
                //dt.Columns.Add("RelationName", typeof(string));
                //dt.Columns.Add("UANNo", typeof(string));
                //dt.Columns.Add("ESICNo", typeof(string));
                //dt.Columns.Add("NomineeRelation", typeof(string));
                //dt.Columns.Add("NomineeName", typeof(string));
                


                // Fill rows
                foreach (var item in Gettbldata.tbldata)
                {
                    dt.Rows.Add(
                        item.SrNo,
                        item.CompanyName,
                        item.BranchName,
                        item.ContractorName,
                        item.Salutation,
                        item.EmployeeName,
                        item.EmployeeNo,
                        item.EmployeeCardNo,
                        item.Designation,

                        item.AadhaarNo,
                        item.NameAsPerAadhaar,
                        item.PrimaryMobile,
                        item.Gender,
                        item.BirthDate,
                        item.MaritalStatus,
                        item.HighestQualification,
                        item.JoiningDate,
                        item.ShiftGroup,
                        item.ShiftRule,
                        item.CanteenApplicable,
                        item.Pincode,
                        item.StateName,
                        item.DistrictName,
                        item.TalukaName,
                        item.POName,
                        item.CityName,
                        item.PostalAddress,
                        item.PayType,
                        item.Rate,
                        item.AttendanceBonus,
                        item.AccountNo,
                        item.BankName,
                        item.IFSCCode


                        // item.SubUnit,
                        //item.Department,
                        //item.SubDepartment,
                        //item.ManpowerCategory,
                        //item.Grade,
                        //item.CriticalStageApplicable,
                        //item.CriticalStageCategory,
                        //item.LineMaster,
                        //item.AssessmentLevel,
                        //item.WeeklyOff,
                        //item.PFNo,
                        //item.RelationType,
                        //item.RelationName,
                        //item.UANNo,
                        //item.ESICNo,
                        //item.NomineeRelation,
                        //item.NomineeName,

                    );
                }

                // Dapper parameters
                var param = new DynamicParameters();
                param.Add("@tbl_dt_allData", dt.AsTableValuedParameter("tbl_TypeBulkinsertPremium"));
                param.Add("@p_CmpId", Gettbldata.CmpId);
                param.Add("@p_BranchId", Gettbldata.BranchId);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_ModifiedBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName());
                param.Add("@p_Msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var data = DapperORM.ExecuteReturn("sp_SUD_Employee_BulkInsert_Premium", param);
                return Json(new
                {
                    Icon = param.Get<string>("@p_Icon"),
                    Message = param.Get<string>("@p_Msg")
                });
            }
            catch (Exception ex)
            {
                return Json(new { Icon = "error", Message = ex.Message });
            }
        }

        #endregion

        #region Notificaton Class
        public class NotificationModel
        {
            public string Title { get; set; }
            public string Body { get; set; }
            public int NotifyEmpId { get; set; }
            public string CreatedBy { get; set; }
            public string RequestType { get; set; }
        }
        #endregion

        #region This Below Code Testing Purpose
        [HttpPost]
        public JsonResult SendNotification(NotificationModel model)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string VerificationUrl = "http://115.124.123.180:8094/api/SendNotification";
                   
                    // ✅ Add header
                    string GetCustomerCode = Convert.ToString(Session["ESSCustomerCode"]);
                    GetCustomerCode = "P_K1006";
                    client.DefaultRequestHeaders.Add("CustomerCode", GetCustomerCode);

                    // ✅ Prepare request body
                    var requestData = new
                    {
                        Title = model.Title,
                        Body = model.Body,
                        NotifyEmpId = Session["EmployeeId"],
                        CreatedBy = Session["EmployeeName"],
                        RequestType = model.RequestType
                    };

                    // ✅ Serialize to JSON
                    string json = JsonConvert.SerializeObject(requestData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // ✅ Send POST request
                    HttpResponseMessage response = client.PostAsync(VerificationUrl, content).GetAwaiter().GetResult();

                    // ✅ Handle response
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        Console.WriteLine("✅ API Response: " + jsonResponse);
                        return Json(new { success = true, message = "Notification saved successfully!" });
                    }
                    else
                    {
                        //Console.WriteLine("❌ API Error: " + (int)response.StatusCode + " - " + response.ReasonPhrase);
                        string errorResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        Console.WriteLine("🔍 Error Details: " + errorResponse);
                        return Json(new { success = "error", message = errorResponse });
                    }
                }

               
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion
    }
}