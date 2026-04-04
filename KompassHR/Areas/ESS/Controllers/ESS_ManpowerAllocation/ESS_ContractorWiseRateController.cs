using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.ESS.Models.ESS_ManpowerAllocation;
using KompassHR.Models;
using Newtonsoft.Json;
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


namespace KompassHR.Areas.ESS.Controllers.ESS_ManpowerAllocation
{
    public class ESS_ContractorWiseRateController : Controller
    {
        clsCommonFunction objcon = new clsCommonFunction();

        #region ESS_ContractorWiseRate Main View 
        // GET: ESS/ESS_ContractorWiseRate
        public ActionResult ESS_ContractorWiseRate(Manpower_ContractorWiseRate Manpower_ContractorWiseRate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 782;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }


                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                var CMPID = GetComapnyName[0].Id;

                ViewBag.BUName = new List<AllDropDownBind>();
                ViewBag.ContractorName = new List<AllDropDownBind>();

                ViewBag.AddUpdateTitle = "Add";
                if ((Manpower_ContractorWiseRate.CmpId > 0) && (Manpower_ContractorWiseRate.FromMonth !=DateTime.MinValue) && (Manpower_ContractorWiseRate.ToMonth != DateTime.MinValue))
                {

                    var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CMPID), Convert.ToInt32(Session["EmployeeId"]));
                    ViewBag.BUName = Branch;

                    if (Manpower_ContractorWiseRate.BranchId > 0)
                    {
                        DynamicParameters paramContractorName = new DynamicParameters();
                        paramContractorName.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + Manpower_ContractorWiseRate.BranchId + "");
                        var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", paramContractorName).ToList();
                        ViewBag.ContractorName = data;
                    }
                    else
                    {
                        ViewBag.ContractorName = "";
                    }
                    DynamicParameters paramHC = new DynamicParameters();
                    paramHC.Add("@p_Process", "DesignationList");
                    paramHC.Add("@p_CmpId", Manpower_ContractorWiseRate.CmpId);
                    paramHC.Add("@p_BranchId", Manpower_ContractorWiseRate.BranchId);
                    paramHC.Add("@p_ContractorId", Manpower_ContractorWiseRate.ContractorId);
                    paramHC.Add("@p_FromMonth", Manpower_ContractorWiseRate.FromMonth);
                    paramHC.Add("@p_ToMonth", Manpower_ContractorWiseRate.ToMonth);
                    var DesignationList = DapperORM.ExecuteSP<dynamic>("sp_Get_ContracterDesignation", paramHC).ToList();

                    ViewBag.GetDesignationList = DesignationList;
                    if (DesignationList == null || !DesignationList.Any())
                    {
                        TempData["Message"] = "No Record Found.";
                        TempData["Icon"] = "error";
                    }

                    if (!string.IsNullOrEmpty(Manpower_ContractorWiseRate.ContractorWiseRateMasterId_Encrypted))
                    {
                        ViewBag.AddUpdateTitle = "Update";

                        DynamicParameters param2 = new DynamicParameters();
                        param2.Add("@p_ContractorWiseRateMasterId_Encrypted", Manpower_ContractorWiseRate.ContractorWiseRateMasterId_Encrypted);
                        Manpower_ContractorWiseRate = DapperORM.ReturnList<Manpower_ContractorWiseRate>("sp_List_Manpower_ContractorWiseRate", param2).FirstOrDefault();
                        if (Manpower_ContractorWiseRate != null)
                        {
                            TempData["FromMonth"] = Manpower_ContractorWiseRate.FromMonth.ToString("yyyy-MM");
                            TempData["ToMonth"] = Manpower_ContractorWiseRate.ToMonth.ToString("yyyy-MM");
                        }
                    }
                }
                return View(Manpower_ContractorWiseRate);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ESS_ContractorWiseRate bulk Main View
        [HttpPost]
        public ActionResult ESS_ContractorWiseRate(HttpPostedFileBase AttachFile, Manpower_ContractorWiseRate obj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                    return RedirectToAction("Login", "Login", new { Area = "" });

                Session["Date"] = null;

                // Check user access
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 780;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;

                if (obj.CmpId > 0)
                {
                    //DynamicParameters paramBUName = new DynamicParameters();
                    //paramBUName.Add("@query", "select BranchId as Id,BranchName as Name from Mas_Branch where CmpId='" + obj.CmpId + "';");
                    //ViewBag.BUName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBUName).ToList();

                    var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(obj.CmpId), Convert.ToInt32(Session["EmployeeId"]));
                    ViewBag.BUName = Branch;

                    if (obj.BranchId > 0)
                    {
                        DynamicParameters paramContractorName = new DynamicParameters();
                        paramContractorName.Add("@query", "select Contractor_Master.ContractorId as Id,Contractor_Master.ContractorName as Name from Contractor_Master " +
                        "Inner Join Mas_ContractorMapping on Mas_ContractorMapping.ContractorID = Contractor_Master.ContractorId and Mas_ContractorMapping.IsActive = 1 " +
                       "where Contractor_Master.Deactivate = 0 and Mas_ContractorMapping.CmpID='" + obj.CmpId + "' AND Mas_ContractorMapping.BranchID='" + obj.BranchId + "' ORDER BY Name;");
                        ViewBag.ContractorName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramContractorName).ToList();
                    }
                    else
                    {
                        ViewBag.ContractorName = "";
                    }
                }

                if (AttachFile != null && AttachFile.ContentLength > 0)
                {
                    // Handle file upload
                    string directoryPath = Server.MapPath("~/assets/MonthlyAttendance");
                    if (!Directory.Exists(directoryPath))
                        Directory.CreateDirectory(directoryPath);

                    string fileName = Path.GetFileName(AttachFile.FileName);
                    string filePath = Path.Combine(directoryPath, fileName);
                    AttachFile.SaveAs(filePath);

                    var excelDataList = new List<IDictionary<string, object>>();

                    using (var xlWorkbook = new XLWorkbook(filePath))
                    {
                        var worksheet = xlWorkbook.Worksheet(1);
                        var columnNames = new List<string>();
                        int col = 1;
                        while (!worksheet.Cell(2, col).IsEmpty())
                        {
                            columnNames.Add(worksheet.Cell(2, col).GetString());
                            col++;
                        }

                        // --- Step 3: Read rows starting from row 3 ---
                        int dataRow = 3;
                        while (!worksheet.Cell(dataRow, 1).IsEmpty())
                        {
                            var rowDict = new Dictionary<string, object>();
                            for (int c = 0; c < columnNames.Count; c++)
                            {
                                rowDict[columnNames[c]] = worksheet.Cell(dataRow, c + 1).Value;
                            }
                            excelDataList.Add(rowDict);
                            dataRow++;
                        }

                        // --- Step 4: Convert to typed list ---
                        var typedList = new List<Manpower_ContractorWiseRate>();
                        foreach (var rowDict in excelDataList)
                        {
                            int hcMappingDetailsId = 0;
                            if (rowDict.ContainsKey("ContractorWiseRateDetailsId") &&
                                rowDict["ContractorWiseRateDetailsId"] != null &&
                                !string.IsNullOrWhiteSpace(rowDict["ContractorWiseRateDetailsId"].ToString()))
                            {
                                int.TryParse(rowDict["ContractorWiseRateDetailsId"].ToString(), out hcMappingDetailsId);
                            }

                            string DesignationName = rowDict.ContainsKey("DesignationName") ? rowDict["DesignationName"].ToString().Trim() : "";

                            // --- Calculate LineId from LineName ---
                            double DesignationId = 0;
                            if (!string.IsNullOrEmpty(DesignationName))
                            {
                                var GetDesignationId = "SELECT DesignationId FROM Mas_Designation WHERE DesignationName = '" + DesignationName + "' AND Deactivate = 0";
                                var DesignationID = DapperORM.DynamicQuerySingle(GetDesignationId);

                                if (DesignationID != null)
                                {
                                    DesignationId = (double)DesignationID.DesignationId;
                                }
                            }

                            typedList.Add(new Manpower_ContractorWiseRate
                            {
                                DesignationName = DesignationName,
                                PerDayRate = rowDict.ContainsKey("PerDayRate") && !string.IsNullOrWhiteSpace(rowDict["PerDayRate"]?.ToString())
                                                ? Convert.ToInt32(rowDict["PerDayRate"]) : 0,
                                CanteenRate = rowDict.ContainsKey("CanteenRate") && !string.IsNullOrWhiteSpace(rowDict["CanteenRate"]?.ToString())
                                                ? Convert.ToSingle(rowDict["CanteenRate"]) : 0,
                                AttendanceBonusRate = rowDict.ContainsKey("AttendanceBonusRate") && !string.IsNullOrWhiteSpace(rowDict["AttendanceBonusRate"]?.ToString())
                                                ? Convert.ToSingle(rowDict["AttendanceBonusRate"]) : 0,
                                TransportAllowanceRate = rowDict.ContainsKey("TransportAllowanceRate") && !string.IsNullOrWhiteSpace(rowDict["TransportAllowanceRate"]?.ToString())
                                                ? Convert.ToSingle(rowDict["TransportAllowanceRate"]) : 0,
                                Other1 = rowDict.ContainsKey("Other1") && !string.IsNullOrWhiteSpace(rowDict["Other1"]?.ToString())
                                                ? Convert.ToSingle(rowDict["Other1"]) : 0,
                                Other2 = rowDict.ContainsKey("Other2") && !string.IsNullOrWhiteSpace(rowDict["Other2"]?.ToString())
                                                ? Convert.ToSingle(rowDict["Other2"]) : 0,
                                DesignationId = DesignationId,
                                ContractorWiseRateDetailsId = hcMappingDetailsId
                            });
                        }

                        // --- Step 5: Assign typed list to ViewBag ---
                        ViewBag.GetDesignationList = typedList;
                        ViewBag.count = typedList.Count;
                    }

                    // Delete uploaded file after processing
                    System.IO.File.Delete(filePath);
                }

                else
                {
                    if (obj.ContractorId > 0)
                    {
                        DynamicParameters paramHC = new DynamicParameters();
                        paramHC.Add("@p_Process", "DesignationList");
                        paramHC.Add("@p_CmpId", (obj.CmpId));
                        paramHC.Add("@p_BranchId", (obj.BranchId));
                        paramHC.Add("@p_ContractorId", (obj.ContractorId));
                        paramHC.Add("@p_FromMonth", obj.FromMonth);
                        paramHC.Add("@p_ToMonth", obj.ToMonth);
                        var DesignationList = DapperORM.ExecuteSP<dynamic>("sp_Get_ContracterDesignation", paramHC).ToList();
                        ViewBag.GetDesignationList = DesignationList;
                    }
                    else
                    {
                        ViewBag.GetDesignationList = new List<Manpower_ContractorWiseRate>(); // Empty list
                    }
                }
                if (obj.FromMonth != DateTime.MinValue)
                {
                    TempData["FromMonth"] = obj.FromMonth.ToString("yyyy-MM");
                    TempData["ToMonth"] = obj.ToMonth.ToString("yyyy-MM");
                }
                else
                {
                    TempData["FromMonth"] = "";
                    TempData["ToMonth"] = "";
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

        #region GetBuisnessUnit
        [HttpGet]
        public ActionResult GetBuisnessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CmpId), Convert.ToInt32(Session["EmployeeId"]));
                return Json(new { Branch = Branch }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetContractor
        [HttpGet]
        public ActionResult GetContractor(int BranchId, int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters paramContractorName = new DynamicParameters();
                paramContractorName.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + BranchId + "");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", paramContractorName).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsContractorWiseRateExists
        [HttpGet]
        public ActionResult IsContractorWiseRateExists(int CmpId, int BranchId, int ContractorId, string FromMonth, string ToMonth, string ContractorWiseRateMasterId_Encrypted)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_CmpId", CmpId);
                    param.Add("@p_BranchId", BranchId);
                    param.Add("@p_ContractorId", ContractorId);
                    param.Add("@p_FromMonth", Convert.ToDateTime(FromMonth));
                    param.Add("@p_ToMonth", Convert.ToDateTime(ToMonth));
                    param.Add("@p_ContractorWiseRateMasterId_Encrypted", ContractorWiseRateMasterId_Encrypted);
                  //  param.Add("@p_MachineName", Dns.GetHostName().ToString());
                   // param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                    var Result = DapperORM.ExecuteReturn("sp_SUD_Manpower_ContractorWiseRate", param);
                    var Message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    if (Message != "")
                    {
                        return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(true, JsonRequestBehavior.AllowGet);
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

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(List<Manpower_ContractorWiseRate> ContractorRateMapping, string ContractorWiseRateMasterId_Encrypted, string CmpId, string BranchId, string ContractorId, string ServiceCharges,
            string SupervisorCharges, double? Rate,double? Percentage, bool IsGross, bool IsOT, DateTime FromMonth, DateTime ToMonth)
        {
            try

            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                StringBuilder strBuilder = new StringBuilder();

                var GetContractorWiseRateMasterId = "SELECT ISNULL(ContractorWiseRateMasterId,0) AS ContractorWiseRateMasterId " +
                                                  "FROM Manpower_ContractorWiseRateMaster WHERE CmpId = '" + CmpId + "' " +
                                                  "AND BranchId = '" + BranchId + "'AND ContractorId = '" + ContractorId + "' AND Deactivate = 0" +
                                                  " AND FromMonth <= '" + ToMonth + " 'And  ToMonth >= '" + FromMonth + " '";

                var ContractorWiseRateMasterID = DapperORM.DynamicQuerySingle(GetContractorWiseRateMasterId);
                double ContractorWiseRateMasterId = 0;

                if (ContractorWiseRateMasterID != null)
                {
                    ContractorWiseRateMasterId = (double)ContractorWiseRateMasterID.ContractorWiseRateMasterId;
                }
                string masterId = "";

                if (ContractorWiseRateMasterId == 0 || ContractorWiseRateMasterId == null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_process", string.IsNullOrEmpty(ContractorWiseRateMasterId_Encrypted) ? "Save" : "Update");
                    param.Add("@p_ContractorWiseRateMasterId_Encrypted", ContractorWiseRateMasterId_Encrypted);
                    param.Add("@p_CmpId", CmpId);
                    param.Add("@p_BranchId", BranchId);
                    param.Add("@p_ContractorId", ContractorId);
                    param.Add("@p_ServiceCharges", ServiceCharges);
                    param.Add("@p_SupervisorCharges", SupervisorCharges);
                    param.Add("@p_Rate", Rate);
                    param.Add("@p_Percentage", Percentage);
                    param.Add("@p_IsGross", IsGross);
                    param.Add("@p_IsOT", IsOT);
                    param.Add("@p_FromMonth", Convert.ToDateTime(FromMonth));
                    param.Add("@p_ToMonth", Convert.ToDateTime(ToMonth));
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                    var Result = DapperORM.ExecuteReturn("sp_SUD_Manpower_ContractorWiseRate", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    TempData["P_Id"] = param.Get<string>("@p_Id");
                    masterId = param.Get<string>("@p_Id");
                }
                else
                {
                    masterId = ContractorWiseRateMasterId.ToString();
                }

                for (var i = 0; i < ContractorRateMapping.Count; i++)
                {
                    if (ContractorRateMapping[i].ContractorWiseRateDetailsId == 0)
                    {
                        string qry = "INSERT INTO dbo.Manpower_ContractorWiseRateDetails " +
                                       "(ContractorWiseRateMasterId, DesignationId, PerDayRate, " +
                                       "CanteenRate, AttendanceBonusRate ,TransportAllowanceRate,Other1,Other2,Deactivate, CreatedBy, CreatedDate, MachineName)" +
                                       "VALUES ('" + masterId + "', " +
                                       "'" + ContractorRateMapping[i].DesignationId + "', " +
                                       "'" + ContractorRateMapping[i].PerDayRate + "', " +
                                       "'" + ContractorRateMapping[i].CanteenRate + "', " +
                                       "'" + ContractorRateMapping[i].AttendanceBonusRate + "', " +
                                       "'" + ContractorRateMapping[i].TransportAllowanceRate + "', " +
                                       "'" + ContractorRateMapping[i].Other1 + "', " +
                                       "'" + ContractorRateMapping[i].Other2 + "', " +
                                       "0, '" + Session["EmployeeName"] + "', GETDATE(), '" + Dns.GetHostName() + "');";

                        strBuilder.Append(qry);
                    }
                    else
                    {
                        string qry = "UPDATE dbo.Manpower_ContractorWiseRateDetails " +
                                     "SET PerDayRate = '" + ContractorRateMapping[i].PerDayRate + "', " +
                                     "CanteenRate = '" + ContractorRateMapping[i].CanteenRate + "', " +
                                     "AttendanceBonusRate = '" + ContractorRateMapping[i].AttendanceBonusRate + "', " +
                                     "TransportAllowanceRate = '" + ContractorRateMapping[i].TransportAllowanceRate + "', " +
                                     "Other1 = '" + ContractorRateMapping[i].Other1 + "', " +
                                     "Other2 = '" + ContractorRateMapping[i].Other2 + "', " +
                                     "ModifiedBy = '" + Session["EmployeeName"] + "', " +
                                     "ModifiedDate = GETDATE(), " +
                                     "MachineName = '" + Dns.GetHostName() + "' " +
                                     "WHERE ContractorWiseRateDetailsId = '" + ContractorRateMapping[i].ContractorWiseRateDetailsId + "' " +
                                     "AND DesignationId='" + ContractorRateMapping[i].DesignationId + "';";

                        strBuilder.Append(qry);
                    }
                }

                string abc = "";
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    TempData["Message"] = "Record Save Successfully";
                    TempData["Icon"] = "success";
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

        #region GetList Main View 
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 782;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "ESS" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ContractorWiseRateMasterId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Manpower_ContractorWiseRate", param).ToList();
                ViewBag.GetRateList = data;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(int? ContractorWiseRateMasterId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "ESS" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Delete");
                param.Add("@p_ContractorWiseRateMasterId", ContractorWiseRateMasterId);
                param.Add("@p_CreatedupdateBy", Session["EmployeeName"].ToString());
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                var Result = DapperORM.ExecuteReturn("sp_SUD_Manpower_ContractorWiseRate", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                var P_Id = param.Get<string>("@p_Id");
                return RedirectToAction("GetList", "ESS_ContractorWiseRate");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ShowDesignationList
        [HttpGet]
        public ActionResult ShowDesignationList(int? CmpId, int? BranchId, int? ContractorId,DateTime FromMonth,DateTime ToMonth)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            DynamicParameters paramHC = new DynamicParameters();
            paramHC.Add("@p_Process", "DesignationList");
            paramHC.Add("@p_CmpId", CmpId);
            paramHC.Add("@p_BranchId", BranchId);
            paramHC.Add("@p_ContractorId", ContractorId);
            paramHC.Add("@p_FromMonth", FromMonth);
            paramHC.Add("@p_ToMonth", ToMonth);
            var data = DapperORM.ExecuteSP<dynamic>("sp_Get_ContracterDesignation", paramHC).ToList();

            string jsonData = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region DeleteContractorWiseRateDetails
        public ActionResult DeleteContractorWiseRateDetails(double? ContractorWiseRateDetailsId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                StringBuilder strBuilder = new StringBuilder();
                var createdBy = Session["EmployeeName"]?.ToString().Replace("'", "''");
                var machineName = Dns.GetHostName().Replace("'", "''");
                strBuilder.AppendLine("UPDATE dbo.Manpower_ContractorWiseRateDetails " +
                    "SET Deactivate = 1, " +
                    "ModifiedBy = '" + createdBy + "', " +
                    "ModifiedDate = GETDATE(), " +
                    "MachineName = '" + machineName + "' " +
                    "WHERE ContractorWiseRateDetailsId = '" + ContractorWiseRateDetailsId + "';");
                string abc = "";
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    TempData["Message"] = "Record Deleted successfully.";
                    TempData["Icon"] = "success";
                }
                return RedirectToAction("GetList", "ESS_ContractorWiseRate");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(double? CmpId, double? BranchId, double? ContractorId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramHC = new DynamicParameters();
                paramHC.Add("@p_Process", "DesignationList");
                paramHC.Add("@p_CmpId", CmpId);
                paramHC.Add("@p_BranchId", BranchId);
                paramHC.Add("@p_ContractorId", ContractorId);
                var data = DapperORM.ExecuteSP<dynamic>("sp_Get_ContracterDesignation", paramHC).ToList();
                if (!data.Any())
                {
                    TempData["Message"] = "No data available to export.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("ESS_ContractorWiseRate");
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("HC_MDMapping");

                    var firstRow = (IDictionary<string, object>)data.First();
                    var columnNames = firstRow.Keys.ToList();
                    int totalColumns = columnNames.Count;

                    worksheet.Range(1, 1, 1, totalColumns).Merge();
                    worksheet.Cell(1, 1).Value = "Category Wise Rate";
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Font.FontSize = 12;

                    worksheet.SheetView.FreezeRows(2);

                    // Write headers
                    for (int col = 0; col < totalColumns; col++)
                    {
                        worksheet.Cell(2, col + 1).Value = columnNames[col];
                    }

                    // Write data rows
                    for (int row = 0; row < data.Count; row++)
                    {
                        var dataRow = (IDictionary<string, object>)data[row];
                        for (int col = 0; col < totalColumns; col++)
                        {
                            var value = dataRow.ContainsKey(columnNames[col]) ? dataRow[columnNames[col]]?.ToString() : "";
                            worksheet.Cell(row + 3, col + 1).Value = value;
                        }
                    }

                    // Style range
                    var usedRange = worksheet.RangeUsed();
                    usedRange.Style.Fill.BackgroundColor = XLColor.White;
                    usedRange.Style.Border.SetTopBorder(XLBorderStyleValues.Thin);
                    usedRange.Style.Border.SetBottomBorder(XLBorderStyleValues.Thin);
                    usedRange.Style.Border.SetLeftBorder(XLBorderStyleValues.Thin);
                    usedRange.Style.Border.SetRightBorder(XLBorderStyleValues.Thin);
                    usedRange.Style.Font.FontSize = 10;
                    usedRange.Style.Font.FontColor = XLColor.Black;

                    // Style header
                    var headerRange = worksheet.Range(2, 1, 2, totalColumns);
                    headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                    headerRange.Style.Font.FontSize = 10;
                    headerRange.Style.Font.FontColor = XLColor.Black;
                    headerRange.Style.Font.Bold = true;

                    // Auto-fit visible columns (from 22 onward)
                    for (int col = 22; col <= totalColumns; col++)
                    {
                        worksheet.Column(col).AdjustToContents();
                    }

                    // === HIDE specific columns ===
                    var hiddenColumns = new List<string>
                    {
                        "DesignationId",
                        "ContractorWiseRateDetailsId"
                    };

                    foreach (var colName in hiddenColumns)
                    {
                        int colIndex = columnNames.IndexOf(colName) + 1; // ClosedXML columns are 1-based
                        if (colIndex > 0)
                        {
                            worksheet.Column(colIndex).Hide();
                        }
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;
                        return File(stream.ToArray(),
                                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                    "ContractorWiseRate.xlsx");
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