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
    public class ESS_HeadCountMappingController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        StringBuilder strBuilder = new StringBuilder();

        // GET: Setting/ ESS_HeadCountMapping
        #region Main View
        public ActionResult ESS_HeadCountMapping(Manpower_HeadCountMapping Manpower_HeadCountMapping)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                // Check access permissions
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 774;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetCompanyName = GetComapnyName;
                DynamicParameters paramLine = new DynamicParameters();
                paramLine.Add("@query", "SELECT LineId as Id, LineName as Name FROM Mas_LineMaster WHERE Deactivate = 0 ORDER BY Name");
                var Line = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();
                ViewBag.GetLineName = Line;

                ViewBag.GetBranchName = "";
                ViewBag.GetHeadCountMappingData = "";

                if (Manpower_HeadCountMapping.CmpId > 0)
                {


                    var branch = DapperORM.DynamicQueryMultiple("SELECT DISTINCT Mas_Branch.BranchId AS Id, BranchName AS Name FROM Mas_Branch " +
                     "INNER JOIN Manpower_Mapping_R1R2R3 ON Manpower_Mapping_R1R2R3.BranchId = Mas_Branch.BranchId " +
                     "WHERE Manpower_Mapping_R1R2R3.Deactivate = 0 " +
                     "AND Mas_Branch.Deactivate = 0 " +
                     "AND Manpower_Mapping_R1R2R3.CmpId = '" + Manpower_HeadCountMapping.CmpId + "'");
                    ViewBag.GetBranchName = branch[0].Select(x => new AllDropDownBind { Id = Convert.ToDouble(x.Id), Name = (string)x.Name }).ToList();

                    DynamicParameters paramHC = new DynamicParameters();
                    paramHC.Add("@p_Process", "List");
                    paramHC.Add("@p_CmpId", Manpower_HeadCountMapping.CmpId);
                    paramHC.Add("@p_BranchId", Manpower_HeadCountMapping.BranchId);
                    paramHC.Add("@p_MonthYear", Manpower_HeadCountMapping.MonthYear);
                    ViewBag.GetLineandManpowerCategoryList = DapperORM.ExecuteSP<dynamic>("sp_Get_LineManPowerCatgeory", paramHC).ToList();

                    TempData["MonthYear"] = Manpower_HeadCountMapping.MonthYear.ToString("yyyy-MM");

                    if (Manpower_HeadCountMapping.HeadCountMappingMasterID_Encrypted != null)
                    {
                        ViewBag.AddUpdateTitle = "Update";
                        DynamicParameters param2 = new DynamicParameters();
                        param2.Add("@p_HeadCountMappingMasterID_Encrypted", Manpower_HeadCountMapping.HeadCountMappingMasterID_Encrypted);
                        param2.Add("@p_EmployeeId", Session["EmployeeId"]);
                        Manpower_HeadCountMapping = DapperORM.ReturnList<Manpower_HeadCountMapping>("sp_List_Manpower_HeadCountMapping", param2).FirstOrDefault();
                        if (Manpower_HeadCountMapping != null)
                        {
                            TempData["MonthYear"] = Manpower_HeadCountMapping.MonthYear.ToString("yyyy-MM");
                        }
                    }
                }

                return View(Manpower_HeadCountMapping);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ESS_HeadCountMapping MAin View
        [HttpPost]
        public ActionResult ESS_HeadCountMapping(Manpower_HeadCountMapping Manpower_HeadCountMapping, HttpPostedFileBase AttachFile)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 774;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetCompanyName = GetComapnyName;

                DynamicParameters paramLine = new DynamicParameters();
                paramLine.Add("@query", "SELECT LineId as Id, LineName as Name FROM Mas_LineMaster WHERE Deactivate = 0 ORDER BY Name");
                var Line = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();
                ViewBag.GetLineName = Line;

                ViewBag.GetBranchName = "";

                if (Manpower_HeadCountMapping.CmpId > 0)
                {
                    var branch = DapperORM.DynamicQueryMultiple("SELECT DISTINCT Mas_Branch.BranchId AS Id, BranchName AS Name FROM Mas_Branch " +
                   "INNER JOIN Manpower_Mapping_R1R2R3 ON Manpower_Mapping_R1R2R3.BranchId = Mas_Branch.BranchId " +
                   "WHERE Manpower_Mapping_R1R2R3.Deactivate = 0 " +
                   "AND Mas_Branch.Deactivate = 0 " +
                   "AND Manpower_Mapping_R1R2R3.CmpId = '" + Manpower_HeadCountMapping.CmpId + "'");
                    ViewBag.GetBranchName = branch[0].Select(x => new AllDropDownBind { Id = Convert.ToDouble(x.Id), Name = (string)x.Name }).ToList();
                }

                if (Manpower_HeadCountMapping.MonthYear != DateTime.MinValue)
                {
                    string formattedDate = Manpower_HeadCountMapping.MonthYear.ToString("yyyy-MM-dd");

                    var GetR1R2R3Dates = "SELECT * FROM Manpower_Mapping_R1R2R3"
                        + " WHERE CmpId = " + Manpower_HeadCountMapping.CmpId
                        + " AND Deactivate = 0"
                        + " AND BranchId = " + Manpower_HeadCountMapping.BranchId
                        + " AND MONTH(MonthYear) = MONTH(CONVERT(DATE, '" + formattedDate + "'))"
                        + " AND YEAR(MonthYear) = YEAR(CONVERT(DATE, '" + formattedDate + "'))";

                    var data = DapperORM.DynamicQuerySingle(GetR1R2R3Dates);

                    if (data == null)
                    {
                        TempData["Message"] = "R1/R2/R3 Mapping not found for this month ('" + Manpower_HeadCountMapping.MonthYear.ToString("MMM-yyyy") + "').";
                        TempData["Icon"] = "error";
                    }
                    else
                    {
                        if (AttachFile != null && AttachFile.ContentLength > 0)
                        {
                            List<Manpower_HeadCountMapping_Bulk_excel> excelDataList = new List<Manpower_HeadCountMapping_Bulk_excel>();

                            using (var stream = new MemoryStream())
                            {
                                AttachFile.InputStream.Position = 0;
                                XLWorkbook xlWorkbook = new XLWorkbook(AttachFile.InputStream);
                                var worksheet = xlWorkbook.Worksheet(1);

                                int row = 2; // Row 1 is header

                                while (!worksheet.Cell(row, 2).IsEmpty())
                                {
                                    string lineName = worksheet.Cell(row, 1).GetString().Trim();
                                    string categoryName = worksheet.Cell(row, 2).GetString().Trim();
                                    string r1headCountStr = worksheet.Cell(row, 3).GetString().Trim();
                                    string r2headCountStr = worksheet.Cell(row, 4).GetString().Trim();
                                    string r3headCountStr = worksheet.Cell(row, 5).GetString().Trim();
                                    double r1headCount = 0, r2headCount = 0, r3headCount = 0;
                                    double.TryParse(r1headCountStr, out r1headCount);
                                    double.TryParse(r2headCountStr, out r2headCount);
                                    double.TryParse(r3headCountStr, out r3headCount);

                                    if (!string.IsNullOrEmpty(lineName) && !string.IsNullOrEmpty(categoryName) && r1headCount > 0)
                                    {
                                        var GetlineId = "SELECT LineId FROM Mas_LineMaster WHERE LineName = '" + lineName + "' AND Deactivate = 0";
                                        var lineId = DapperORM.DynamicQuerySingle(GetlineId);
                                        double LineId = (double)lineId.LineId;

                                        var GetKPISubCategoryId = "SELECT KPISubCategoryId FROM KPI_SubCategory WHERE KPISubCategoryFName = '" + categoryName + "' AND Deactivate = 0";
                                        var subCategoryId = DapperORM.DynamicQuerySingle(GetKPISubCategoryId);
                                        double KPISubCategoryId = (double)subCategoryId.KPISubCategoryId;

                                        excelDataList.Add(new Manpower_HeadCountMapping_Bulk_excel
                                        {
                                            LineName = lineName,
                                            KPISubCategoryFName = categoryName,
                                            R1HeadCount = r1headCount,
                                            R2HeadCount = r2headCount,
                                            R3HeadCount = r3headCount,
                                            LineId = LineId,
                                            KPISubCategoryId = KPISubCategoryId
                                        });
                                    }

                                    row++;
                                }

                                ViewBag.GetLineandManpowerCategoryList = excelDataList;
                            }
                        }
                        else if ((Manpower_HeadCountMapping.CmpId != 0) && (Manpower_HeadCountMapping.MonthYear != DateTime.MinValue))
                        {
                            DynamicParameters paramHC = new DynamicParameters();
                            paramHC.Add("@p_Process", "List");
                            paramHC.Add("@p_CmpId", Manpower_HeadCountMapping.CmpId);
                            paramHC.Add("@p_BranchId", Manpower_HeadCountMapping.BranchId);
                            paramHC.Add("@p_LineId", Manpower_HeadCountMapping.LineId);
                            paramHC.Add("@p_MonthYear", Manpower_HeadCountMapping.MonthYear);
                            var lineManpowerList = DapperORM.ExecuteSP<dynamic>("sp_Get_LineManPowerCatgeory", paramHC).ToList();
                            ViewBag.GetLineandManpowerCategoryList = lineManpowerList;
                            if (lineManpowerList == null || !lineManpowerList.Any())
                            {
                                var GetCompanyName = "select CompanyName from Mas_CompanyProfile where CompanyId = '" + Manpower_HeadCountMapping.CmpId + "' AND Deactivate = 0";
                                var CompanyName = DapperORM.DynamicQuerySingle(GetCompanyName);

                                var GetBranchName = "select BranchName from Mas_Branch where  BranchId = '" + Manpower_HeadCountMapping.BranchId + "' AND Deactivate = 0";
                                var BranchName = DapperORM.DynamicQuerySingle(GetBranchName);

                                TempData["Message"] = "Line and Manpower Category Mapping not found for Company - '" + CompanyName.CompanyName + "' and Business Unit -'" + BranchName.BranchName + "'.";
                                TempData["Icon"] = "error";
                            }
                        }
                    }
                }
                return View(Manpower_HeadCountMapping);
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
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "SELECT DISTINCT Mas_Branch.BranchId AS Id, BranchName AS Name FROM Mas_Branch " +
                 "INNER JOIN Manpower_Mapping_R1R2R3 ON Manpower_Mapping_R1R2R3.BranchId = Mas_Branch.BranchId " +
                 "WHERE Manpower_Mapping_R1R2R3.Deactivate = 0 " +
                 "AND Mas_Branch.Deactivate = 0 " +
                 "AND Manpower_Mapping_R1R2R3.CmpId = '" + CmpId + "'");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetR1R2R3Dates
        [HttpGet]
        public ActionResult GetR1R2R3Dates(int CmpId, int BranchId, DateTime MonthYear, string R1R2R3)
        {
            try
            {
                string formattedDate = MonthYear.ToString("yyyy-MM-dd");

                var GetR1R2R3Dates = "SELECT " + R1R2R3 + "FromDate AS FromDate," + R1R2R3 + "ToDate AS ToDate FROM Manpower_Mapping_R1R2R3"
                    + " WHERE CmpId = " + CmpId
                    + " AND BranchId = " + BranchId
                    + " AND MONTH(MonthYear) = MONTH(CONVERT(DATE, '" + formattedDate + "'))"
                    + " AND YEAR(MonthYear) = YEAR(CONVERT(DATE, '" + formattedDate + "'))";

                var data = DapperORM.DynamicQuerySingle(GetR1R2R3Dates);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsHeadCountMappingExists
        public ActionResult IsHeadCountMappingExists(string CmpId, string BranchId, DateTime MonthYear, string HeadCountMappingMasterID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 774;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {

                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_HeadCountMappingMasterID_Encrypted", HeadCountMappingMasterID_Encrypted);
                    param.Add("@p_CmpId", CmpId);
                    param.Add("@p_BranchId", BranchId);
                    param.Add("@p_MonthYear", MonthYear);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Manpower_HeadCountMapping", param);
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
        public ActionResult SaveUpadte(List<Manpower_HeadCountMapping> HeadCountMapping, string CmpId, string BranchId, DateTime MonthYear, string HeadCountMappingMasterID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 774;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                StringBuilder strBuilder = new StringBuilder();

                var GetHeadCountMappingMasterID = "SELECT ISNULL(HeadCountMappingMasterID,0) AS HeadCountMappingMasterID " +
                                                  "FROM Manpower_HeadCountMappingMaster WHERE CmpId = '" + CmpId + "' " +
                                                  "AND BranchId = '" + BranchId + "'AND MonthYear = '" + MonthYear + "' AND Deactivate = 0";

                var HeadCountMappingMasterID = DapperORM.DynamicQuerySingle(GetHeadCountMappingMasterID);
                double HeadCountMappingMasterId = 0;

                if (HeadCountMappingMasterID != null)
                {
                    HeadCountMappingMasterId = (double)HeadCountMappingMasterID.HeadCountMappingMasterID;
                }
                string masterId = "";

                if (HeadCountMappingMasterId == 0 || HeadCountMappingMasterId == null)
                {
                    var param = new DynamicParameters();
                    param.Add("@p_process", string.IsNullOrEmpty(HeadCountMappingMasterID_Encrypted) ? "Save" : "Update");
                    param.Add("@p_HeadCountMappingMasterID_Encrypted", HeadCountMappingMasterID_Encrypted);
                    param.Add("@p_CmpId", CmpId);
                    param.Add("@p_BranchId", BranchId);
                    param.Add("@p_MonthYear", MonthYear);
                    param.Add("@p_MachineName", Dns.GetHostName());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                    var data = DapperORM.ExecuteReturn("sp_SUD_Manpower_HeadCountMapping", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    masterId = param.Get<string>("@p_Id"); // New MasterId
                }
                else
                {
                    masterId = HeadCountMappingMasterId.ToString();
                }

                for (var i = 0; i < HeadCountMapping.Count; i++)
                {
                    if (HeadCountMapping[i].HeadCountMappingDetailsID == 0)
                    {
                        string qry = "INSERT INTO dbo.Manpower_HeadCountMappingDetails " +
                                       "(HeadCountMappingMasterID, LineId, ManpowerCategoryId, " +
                                       "LineR1SCDay, LineR2SCDay, LineR3SCDay, " +
                                       "R1HeadCount, R2HeadCount, R3HeadCount, " +
                                       "R1MonthDays, R2MonthDays, R3MonthDays, " +
                                       "Deactivate, CreatedBy, CreatedDate, MachineName) " +
                                       "VALUES ('" + masterId + "', " +
                                       "'" + HeadCountMapping[i].LineId + "', " +
                                       "'" + HeadCountMapping[i].KPISubCategoryId + "', " +
                                       "'" + HeadCountMapping[i].LineR1SCDay + "', " +
                                       "'" + HeadCountMapping[i].LineR2SCDay + "', " +
                                       "'" + HeadCountMapping[i].LineR3SCDay + "', " +
                                       "'" + HeadCountMapping[i].R1HeadCount + "', " +
                                       "'" + HeadCountMapping[i].R2HeadCount + "', " +
                                       "'" + HeadCountMapping[i].R3HeadCount + "', " +
                                       "'" + HeadCountMapping[i].R1MonthDays + "', " +
                                       "'" + HeadCountMapping[i].R2MonthDays + "', " +
                                       "'" + HeadCountMapping[i].R3MonthDays + "', " +
                                       "0, '" + Session["EmployeeName"] + "', GETDATE(), '" + Dns.GetHostName() + "');";

                        strBuilder.Append(qry);


                    }
                    else
                    {
                        string qry = "UPDATE dbo.Manpower_HeadCountMappingDetails " +
                                     "SET R1HeadCount = '" + HeadCountMapping[i].R1HeadCount + "', " +
                                     "R2HeadCount = '" + HeadCountMapping[i].R2HeadCount + "', " +
                                     "R3HeadCount = '" + HeadCountMapping[i].R3HeadCount + "', " +
                                     "LineR1SCDay = '" + HeadCountMapping[i].LineR1SCDay + "', " +
                                     "LineR2SCDay = '" + HeadCountMapping[i].LineR2SCDay + "', " +
                                     "LineR3SCDay = '" + HeadCountMapping[i].LineR3SCDay + "', " +
                                     "R1MonthDays = '" + HeadCountMapping[i].R1MonthDays + "', " +
                                     "R2MonthDays = '" + HeadCountMapping[i].R2MonthDays + "', " +
                                     "R3MonthDays = '" + HeadCountMapping[i].R3MonthDays + "', " +
                                     "ModifiedBy = '" + Session["EmployeeName"] + "', " +
                                     "ModifiedDate = GETDATE(), " +
                                     "MachineName = '" + Dns.GetHostName() + "' " +
                                     "WHERE HeadCountMappingDetailsID = '" + HeadCountMapping[i].HeadCountMappingDetailsID + "' " +
                                     "AND LineId='" + HeadCountMapping[i].LineId + "' " +
                                     "AND ManpowerCategoryId = '" + HeadCountMapping[i].KPISubCategoryId + "';";

                        strBuilder.Append(qry);
                    }
                }

                string abc = "";
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    TempData["Message"] = "Record save successfully";
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

        #region GetList
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 774;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_HeadCountMappingMasterID_Encrypted", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var data = DapperORM.DynamicList("sp_List_Manpower_HeadCountMapping", param);
                ViewBag.GetHeadCountMappingData = data;


                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ShowLine_ManpowerCategory_HeadCountList
        [HttpGet]
        public ActionResult ShowLine_ManpowerCategory_HeadCountList(DateTime MonthYear, int? CmpId, int? BranchId)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }

            DynamicParameters paramH = new DynamicParameters();
            paramH.Add("@p_Process", "LineManpowerHeadCountList");
            paramH.Add("@p_CmpId", CmpId);
            paramH.Add("@p_BranchId", BranchId);
            paramH.Add("@p_MonthYear", MonthYear);
            var data = DapperORM.ExecuteSP<dynamic>("sp_Get_LineManPowerCatgeory", paramH).ToList();
            string jsonData = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Delete
        public ActionResult Delete(double HeadCountMappingMasterID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 774;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_process", "Delete");
                param.Add("@p_HeadCountMappingMasterID", HeadCountMappingMasterID);

                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Manpower_HeadCountMapping", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_HeadCountMapping");
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