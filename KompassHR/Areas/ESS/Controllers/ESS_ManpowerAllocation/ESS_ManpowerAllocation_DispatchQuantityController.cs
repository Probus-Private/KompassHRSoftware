using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.ESS.Models.ESS_ManpowerAllocation;
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

namespace KompassHR.Areas.ESS.Controllers.ESS_ManpowerAllocation
{
    public class ESS_ManpowerAllocation_DispatchQuantityController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        StringBuilder strBuilder = new StringBuilder();
        // GET: ESS/ESS_ManpowerAllocation_DispatchQuantity
        #region Dispatch Quantity Main View
        public ActionResult ESS_ManpowerAllocation_DispatchQuantity(Manpower_HC_MD_DispatchQuantityMaster dispatchQty)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 907;
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

                ViewBag.GetBranchName = "";
                if (dispatchQty.CmpId > 0)
                {
                    TempData["Date"] = dispatchQty.Date.ToString("yyyy-MM-dd");

                    DynamicParameters paramBU = new DynamicParameters();
                    paramBU.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBU.Add("@p_CmpId", dispatchQty.CmpId);
                    ViewBag.GetBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBU).ToList();

                    DynamicParameters paramHC = new DynamicParameters();
                    paramHC.Add("@p_Process", "DispatchQuantity_List");
                    paramHC.Add("@p_CmpId", dispatchQty.CmpId);
                    paramHC.Add("@p_BranchId", dispatchQty.BranchId);
                    paramHC.Add("@p_Date", dispatchQty.Date);
                    var DispatchQuantityList = DapperORM.ExecuteSP<dynamic>("sp_Get_Manpower_DispatchQuantity", paramHC).ToList();
                    ViewBag.DispatchQuantityList = DispatchQuantityList;

                    if (dispatchQty.DispatchQuantityMasterID_Encrypted != null)
                    {
                        ViewBag.AddUpdateTitle = "Update";

                        DynamicParameters param2 = new DynamicParameters();
                        param2.Add("@p_DispatchQuantityMasterID_Encrypted", dispatchQty.DispatchQuantityMasterID_Encrypted);
                        param2.Add("@p_EmployeeId", Session["EmployeeId"]);
                        dispatchQty = DapperORM.ReturnList<Manpower_HC_MD_DispatchQuantityMaster>("sp_List_Manpower_DispatchQuantity", param2).FirstOrDefault();
                        if (dispatchQty != null)
                        {
                            TempData["Date"] = dispatchQty.Date.ToString("yyyy-MM-dd");
                        }
                    }
                }
                    return View(dispatchQty);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Bulk
        [HttpPost]
        public ActionResult ESS_ManpowerAllocation_DispatchQuantity(HttpPostedFileBase AttachFile, Manpower_HC_MD_DispatchQuantityMaster obj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                    return RedirectToAction("Login", "Login", new { Area = "" });

                Session["Date"] = null;

                // Check user access
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 907;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
               
                // Populate company dropdown
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetCompanyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetCompanyName = GetCompanyName;

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", obj.CmpId);
                ViewBag.GetBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                TempData["Date"] = obj.Date.ToString("yyyy-MM-dd");

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

                        // --- Step 1: Validate HeadCount & MonthDays ---
                        int validateRow = 2;
                        while (!worksheet.Cell(validateRow, 2).IsEmpty())
                        {
                            string headCount = worksheet.Cell(validateRow, 3).GetString().Trim();
                            if (string.IsNullOrEmpty(headCount))
                            {
                                TempData["Message"] = $"Fill in missing information in the HeadCount column at row {validateRow}.";
                                TempData["Title"] = "Empty HeadCount Column Detected in Excel Sheet";
                                TempData["Icon"] = "error";
                                System.IO.File.Delete(filePath);
                                return RedirectToAction("ESS_ManpowerAllocation_DispatchQuantity");
                            }

                            string ManDays = worksheet.Cell(validateRow, 4).GetString().Trim();
                            if (string.IsNullOrEmpty(ManDays))
                            {
                                TempData["Message"] = $"Fill in missing information in the ManDays column at row {validateRow}.";
                                TempData["Title"] = "Empty ManDays Column Detected in Excel Sheet";
                                TempData["Icon"] = "error";
                                System.IO.File.Delete(filePath);
                                return RedirectToAction("ESS_ManpowerAllocation_DispatchQuantity");
                            }

                            validateRow++;
                        }

                        // --- Step 2: Read headers from row 2 ---
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
                        var typedList = new List<Manpower_HC_MD_DispatchQuantityMaster>();
                        foreach (var rowDict in excelDataList)
                        {
                            int DispatchQuantityDetailsID = 0;
                            if (rowDict.ContainsKey("DispatchQuantityDetailsID") &&
                                rowDict["DispatchQuantityDetailsID"] != null &&
                                !string.IsNullOrWhiteSpace(rowDict["DispatchQuantityDetailsID"].ToString()))
                            {
                                int.TryParse(rowDict["DispatchQuantityDetailsID"].ToString(), out DispatchQuantityDetailsID);
                            }

                            string lineName = rowDict.ContainsKey("LineName") ? rowDict["LineName"].ToString().Trim() : "";
                            string categoryName = rowDict.ContainsKey("KPISubCategoryFName") ? rowDict["KPISubCategoryFName"].ToString().Trim() : "";

                            // --- Calculate LineId from LineName ---
                            double lineId = 0;
                            if (!string.IsNullOrEmpty(lineName))
                            {
                                var GetlineId = "SELECT LineId FROM Mas_LineMaster WHERE LineName = '" + lineName + "' AND Deactivate = 0";
                                var LineId = DapperORM.DynamicQuerySingle(GetlineId);

                                if (LineId != null)
                                {
                                    lineId = (double)LineId.LineId;
                                }
                            }

                            // --- Calculate KPI SubCategoryId from CategoryName ---
                            double kpiSubCategoryId = 0;
                            if (!string.IsNullOrEmpty(categoryName))
                            {
                                var GetKPISubCategoryId = "SELECT KPISubCategoryId FROM KPI_SubCategory WHERE KPISubCategoryFName = '" + categoryName + "' AND Deactivate = 0";
                                var KPISubCategoryId = DapperORM.DynamicQuerySingle(GetKPISubCategoryId);

                                if (KPISubCategoryId != null)
                                {
                                    kpiSubCategoryId = (double)KPISubCategoryId.KPISubCategoryId;
                                }
                            }

                            typedList.Add(new Manpower_HC_MD_DispatchQuantityMaster
                            {
                                LineName = lineName,
                                KPISubCategoryFName = categoryName,
                                HeadCount = rowDict.ContainsKey("HeadCount") && !string.IsNullOrWhiteSpace(rowDict["HeadCount"]?.ToString())
                                                ? Convert.ToInt32(rowDict["HeadCount"]) : 0,
                                ManDays = rowDict.ContainsKey("ManDays") && !string.IsNullOrWhiteSpace(rowDict["ManDays"]?.ToString())
                                                ? Convert.ToSingle(rowDict["ManDays"]) : 0,
                                LineId = lineId,
                                KPISubCategoryId = kpiSubCategoryId,
                                DispatchQuantityDetailsID = DispatchQuantityDetailsID
                            });
                        }

                        // --- Step 5: Assign typed list to ViewBag ---
                        ViewBag.DispatchQuantityList = typedList;
                        ViewBag.count = typedList.Count;
                    }

                    // Delete uploaded file after processing
                    System.IO.File.Delete(filePath);
                }

                else
                {
                    if (obj.CmpId != 0 && obj.Date != DateTime.MinValue)
                    {


                        DynamicParameters paramList = new DynamicParameters();
                        paramList.Add("@p_Process", "DispatchQuantity_List");
                        paramList.Add("@p_CmpId", obj.CmpId);
                        paramList.Add("@p_BranchId", obj.BranchId);
                        paramList.Add("@p_Date", obj.Date);
                        var DispatchQuantityList = DapperORM.ExecuteSP<dynamic>("sp_Get_Manpower_DispatchQuantity", paramList).ToList();

                        ViewBag.DispatchQuantityList = DispatchQuantityList;

                        if (DispatchQuantityList == null || !DispatchQuantityList.Any())
                        {
                            var GetCompanyName1 = "select CompanyName from Mas_CompanyProfile where CompanyId = '" + obj.CmpId + "' AND Deactivate = 0";
                            var CompanyName = DapperORM.DynamicQuerySingle(GetCompanyName1);

                            var GetBranchName = "select BranchName from Mas_Branch where  BranchId = '" + obj.BranchId + "' AND Deactivate = 0";
                            var BranchName = DapperORM.DynamicQuerySingle(GetBranchName);

                            TempData["Message"] = "Line and Manpower Category Mapping not found for Company - '" + CompanyName.CompanyName + "' and Business Unit -'" + BranchName.BranchName + "'.";
                            TempData["Icon"] = "error";
                        }
                    }
                    else
                    {
                        ViewBag.DispatchQuantityList = new List<Manpower_HC_MD_DispatchQuantityMaster>(); // Empty list
                    }
                }

                return View(obj);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion Bulk

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                DynamicParameters paramBU = new DynamicParameters();
                paramBU.Add("@p_employeeid", Session["EmployeeId"]);
                paramBU.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBU).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsHC_MDMappingExists
        public ActionResult IsDispatchQuantityExists(string CmpId, string BranchId, DateTime Date, string DispatchQuantityMasterID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 907;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {

                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_DispatchQuantityMasterID_Encrypted", DispatchQuantityMasterID_Encrypted);
                    param.Add("@p_CmpId", CmpId);
                    param.Add("@p_BranchId", BranchId);
                    param.Add("@p_Date", Date);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Manpower_HC_MDDispatchQuantity", param);
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 780;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_DispatchQuantityMasterID_Encrypted", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var data = DapperORM.DynamicList("sp_List_Manpower_DispatchQuantity", param);
                ViewBag.GetHC_MDDispatchQuantityData = data;


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
        [HttpPost]
        public ActionResult SaveUpadte(List<Manpower_HC_MD_DispatchQuantityMaster> HC_MDDisapcthQuantity, string CmpId, string BranchId, DateTime Date, string DispatchQuantityMasterID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 907;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                StringBuilder strBuilder = new StringBuilder();

                var GetDispatchQuantityMasterID = "SELECT ISNULL(DispatchQuantityMasterID,0) AS DispatchQuantityMasterID " +
                                                  "FROM Manpower_HC_MD_DispatchQuantityMaster WHERE CmpId = '" + CmpId + "' " +
                                                  "AND BranchId = '" + BranchId + "'AND Date = '" + Date + "' AND Deactivate = 0";

                var DispatchQuantityMasterID = DapperORM.DynamicQuerySingle(GetDispatchQuantityMasterID);
                double DispatchQuantityMasterId = 0;

                if (DispatchQuantityMasterID != null)
                {
                    DispatchQuantityMasterId = (double)DispatchQuantityMasterID.DispatchQuantityMasterID;
                }
                string masterId = "";

                if (DispatchQuantityMasterId == 0 || DispatchQuantityMasterId == null)
                {
                    var param = new DynamicParameters();
                    param.Add("@p_process", string.IsNullOrEmpty(DispatchQuantityMasterID_Encrypted) ? "Save" : "Update");
                    param.Add("@p_DispatchQuantityMasterID_Encrypted", DispatchQuantityMasterID_Encrypted);
                    param.Add("@p_CmpId", CmpId);
                    param.Add("@p_BranchId", BranchId);
                    param.Add("@p_Date", Date);
                    param.Add("@p_MachineName", Dns.GetHostName());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                    var data = DapperORM.ExecuteReturn("sp_SUD_Manpower_HC_MDDispatchQuantity", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    masterId = param.Get<string>("@p_Id"); // New MasterId
                }
                else
                {
                    masterId = DispatchQuantityMasterId.ToString();
                }

                for (var i = 0; i < HC_MDDisapcthQuantity.Count; i++)
                {
                    if (HC_MDDisapcthQuantity[i].DispatchQuantityDetailsID == 0)
                    {
                        string qry = "INSERT INTO dbo.Manpower_HC_MD_DispatchQuantityDetails " +
                                       "(DispatchQuantityMaster_Id, LineId, ManpowerCategoryId, " +
                                       "HeadCount, ManDays ,Deactivate, CreatedBy, CreatedDate, MachineName)" +
                                       "VALUES ('" + masterId + "', " +
                                       "'" + HC_MDDisapcthQuantity[i].LineId + "', " +
                                       "'" + HC_MDDisapcthQuantity[i].KPISubCategoryId + "', " +
                                       "'" + HC_MDDisapcthQuantity[i].HeadCount + "', " +
                                       "'" + HC_MDDisapcthQuantity[i].ManDays + "', " +
                                       "0, '" + Session["EmployeeName"] + "', GETDATE(), '" + Dns.GetHostName() + "');";

                        strBuilder.Append(qry);
                    }
                    else
                    {
                        string qry = "UPDATE dbo.Manpower_HC_MD_DispatchQuantityDetails " +
                                     "SET HeadCount = '" + HC_MDDisapcthQuantity[i].HeadCount + "', " +
                                     "ManDays = '" + HC_MDDisapcthQuantity[i].ManDays + "', " +
                                     "ModifiedBy = '" + Session["EmployeeName"] + "', " +
                                     "ModifiedDate = GETDATE(), " +
                                     "MachineName = '" + Dns.GetHostName() + "' " +
                                     "WHERE DispatchQuantityDetailsID = '" + HC_MDDisapcthQuantity[i].DispatchQuantityDetailsID + "' " +
                                     "AND LineId='" + HC_MDDisapcthQuantity[i].LineId + "' " +
                                     "AND ManpowerCategoryId = '" + HC_MDDisapcthQuantity[i].KPISubCategoryId + "';";

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

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(double? CmpId, double? BranchId, DateTime Date)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                // Initialize Dapper parameters
                DynamicParameters paramHC = new DynamicParameters();
                paramHC.Add("@p_Process", "DispatchQuantity_List");
                paramHC.Add("@p_CmpId", CmpId);
                paramHC.Add("@p_BranchId", BranchId);
                paramHC.Add("@p_Date", Date);

                var data = DapperORM.ExecuteSP<dynamic>("sp_Get_Manpower_DispatchQuantity", paramHC).ToList();

                if (!data.Any())
                {
                    TempData["Message"] = "No data available to export.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("ESS_ManpowerAllocation_DispatchQuantity");
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("HC_MDDispatchQuantity");

                    var firstRow = (IDictionary<string, object>)data.First();
                    var columnNames = firstRow.Keys.ToList();
                    int totalColumns = columnNames.Count;

                    worksheet.Range(1, 1, 1, totalColumns).Merge();
                    worksheet.Cell(1, 1).Value = "Manpower Dispatch Quantity";
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
                "LineId",
                "KPISubCategoryId",
                "DispatchQuantityMasterID",
                "DispatchQuantityDetailsID"
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
                                    "DispatchQuantity_Mapping.xlsx");
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

        #region Delete
        public ActionResult Delete(int DispatchQuantityMasterID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 780;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_process", "Delete");
                param.Add("@p_DispatchQuantityMasterID", DispatchQuantityMasterID);

                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Manpower_HC_MDDispatchQuantity", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_ManpowerAllocation_DispatchQuantity");
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