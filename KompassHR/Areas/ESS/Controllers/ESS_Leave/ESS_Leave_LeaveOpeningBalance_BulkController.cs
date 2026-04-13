using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Leave;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Leave
{
    public class ESS_Leave_LeaveOpeningBalance_BulkController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();

        DataTable dt_Employee = new DataTable();
        DataTable dt_LeaveSetting = new DataTable();
        // GET: ESS/ESS_Leave_LeaveOpeningBalance_Bulk
        #region LeaveOpneingBalance_Bulk Main View
        public ActionResult ESS_Leave_LeaveOpeningBalance_Bulk(Leave_OpeningBalance LeaveOpeningBalance)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 391;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Leave_OpeningBalance LeaveOpeningBalances = new Leave_OpeningBalance();
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.ComapnyName = GetComapnyName;

                if (LeaveOpeningBalance.CmpID != 0)
                {
                    param = new DynamicParameters();
                    param.Add("@p_Process", "Export");
                    param.Add("@p_CmpID", LeaveOpeningBalance.CmpID);
                    param.Add("@p_EmployeeBranchId", LeaveOpeningBalance.LeaveOpeningBalanceBranchID);
                    param.Add("@p_LeaveSettingId", LeaveOpeningBalance.LeaveOpeningBalanceLeaveTypeId);
                    var GetLeaveOpneingBalanceEmployee = DapperORM.ExecuteSP<dynamic>("sp_List_GetLeaveOpneingBalance", param).ToList();
                    ViewBag.LeaveOpneingBalanceEmployee = GetLeaveOpneingBalanceEmployee;

                    DynamicParameters paramBranch = new DynamicParameters();
                    paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBranch.Add("@p_CmpId", LeaveOpeningBalance.CmpID);
                    var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                    ViewBag.BranchName = BranchName;

                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@query", "select LeaveYearID as Id, cast(year(FromDate) as nvarchar(4))+'-'+cast(YEAR(ToDate) as nvarchar(4)) as Name from[dbo].[Leave_Year] where Deactivate = 0 and IsActivate=1  and CmpId=" + LeaveOpeningBalance.CmpID + " order by IsDefault desc,FromDate desc");
                    var LeaveYear = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();
                    ViewBag.GetLeaveYear = LeaveYear;

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@query", "select Leave_Setting.LeaveSettingId as Id ,,Leave_Type.LeaveTypeId concat(Leave_Group.LeaveGroupName, ' - ', Leave_Type.LeaveTypeShortName) as Name from Leave_Setting, Leave_Group, Leave_Type where Leave_Setting.Deactivate = 0 and Leave_Group.Deactivate = 0 and Leave_Type.Deactivate = 0 and Leave_Setting.LeaveSettingLeaveGroupId = Leave_Group.LeaveGroupId and Leave_Type.LeaveTypeId = Leave_Setting.LeaveSettingLeaveTypeId and Leave_Setting.CmpId=" + LeaveOpeningBalance.CmpID + "");
                    var LeaveType = DapperORM.ReturnList<LeaveTypeIdName>("sp_QueryExcution", param2).ToList();
                    ViewBag.LeaveTypeName = LeaveType;

                }
                else
                {
                    ViewBag.LeaveOpneingBalanceEmployee = "";
                    ViewBag.BranchName = "";
                    ViewBag.LeaveTypeName = "";
                    ViewBag.GetLeaveYear = "";
                    ViewBag.GetExceldata = "";
                }
                return View(LeaveOpeningBalances);
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

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "select LeaveYearID as Id, cast(year(FromDate) as nvarchar(4))+'-'+cast(YEAR(ToDate) as nvarchar(4)) as Name from[dbo].[Leave_Year] where Deactivate = 0  and IsActivate=1  and CmpId=" + CmpId + " order by IsDefault desc,FromDate desc");
                var LeaveYear = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@query", "select Leave_Setting.LeaveSettingId as Id ,Leave_Type.LeaveTypeId,concat(Leave_Group.LeaveGroupName, ' - ', Leave_Type.LeaveTypeShortName) as Name from Leave_Setting, Leave_Group, Leave_Type where Leave_Setting.Deactivate = 0 and Leave_Group.Deactivate = 0 and Leave_Type.Deactivate = 0 and Leave_Setting.LeaveSettingLeaveGroupId = Leave_Group.LeaveGroupId and Leave_Type.LeaveTypeId = Leave_Setting.LeaveSettingLeaveTypeId and Leave_Setting.CmpId=" + CmpId + "");
                var LeaveType = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param2).ToList();
                return Json(new { data = data, LeaveYear = LeaveYear, LeaveType = LeaveType }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, int? LeaveYearId, int? LeaveTypeId/*, DateTime FromDate, int? SubUnitId, int? DepartmentId, int? SubDepartmentId, int? DesignationId, int? LineId, int? GradeId, DateTime ToDate*/)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 391;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("LeaveOpeningBalance");
                worksheet.Range(1, 1, 1, 9).Merge();
                worksheet.SheetView.FreezeRows(2); // Freeze the first row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();

                if (CmpId != 0)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    //if (EmployeeId == null)
                    //{
                    //    paramList.Add("@P_EmployeeIds", "All");
                    //}
                    //else
                    //{
                    //    paramList.Add("@P_EmployeeIds", EmployeeId);
                    //}
                    paramList.Add("@p_Process", "Export");
                    paramList.Add("@p_CmpID", CmpId);
                    paramList.Add("@p_EmployeeBranchId", BranchId);
                    paramList.Add("@p_LeaveSettingId", LeaveTypeId);
                    paramList.Add("@p_LeaveYearId", LeaveYearId);
                    //DynamicParameters paramList = new DynamicParameters();
                    //paramList.Add("@P_Qry", Query);
                    var GetLeaveOpneingBalance = DapperORM.ReturnList<dynamic>("sp_List_GetLeaveOpneingBalance", paramList).ToList();

                    if (GetLeaveOpneingBalance.Count == 0)
                    {
                        byte[] emptyFileContents = new byte[0];
                        return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetLeaveOpneingBalance);
                    worksheet.Cell(2, 1).InsertTable(dt, false);
                }
                int totalRows = worksheet.RowsUsed().Count();

                // Set the background color to white and apply borders
                var usedRange = worksheet.RangeUsed();
                usedRange.Style.Fill.BackgroundColor = XLColor.White;
                usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Font.FontSize = 10;
                usedRange.Style.Font.FontColor = XLColor.Black;

                // Set the header row name
                var FromDate = DateTime.Now.Date;
                worksheet.Cell(1, 1).Value = "Leave Opening Balance";
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Columns().AdjustToContents(); // This code for all clomns

                var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                headerRange.Style.Font.FontSize = 10;
                headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
                headerRange.Style.Font.Bold = true;

                // Save the workbook
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0; // Reset the stream position to the beginning

                    // Return the file to the client
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report.xlsx");
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region LeaveOpneingBalance_Bulk Main View
        [HttpPost]
        public ActionResult ESS_Leave_LeaveOpeningBalance_Bulk(HttpPostedFileBase AttachFile, Leave_OpeningBalance LeaveOpeningBalance)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 391;
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

                //ViewBag.LeaveOpneingBalanceEmployee = "";
                DynamicParameters paramBranch = new DynamicParameters();
                paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                paramBranch.Add("@p_CmpId", LeaveOpeningBalance.CmpID);
                var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                ViewBag.BranchName = BranchName;

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "select LeaveYearID as Id, cast(year(FromDate) as nvarchar(4))+'-'+cast(YEAR(ToDate) as nvarchar(4)) as Name from[dbo].[Leave_Year] where Deactivate = 0 and IsActivate=1  and CmpId=" + LeaveOpeningBalance.CmpID + " order by IsDefault desc,FromDate desc");
                var LeaveYear = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();
                ViewBag.GetLeaveYear = LeaveYear;

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@query", "select Leave_Setting.LeaveSettingId as Id,Leave_Type.LeaveTypeId ,concat(Leave_Group.LeaveGroupName, ' - ', Leave_Type.LeaveTypeShortName) as Name from Leave_Setting, Leave_Group, Leave_Type where Leave_Setting.Deactivate = 0 and Leave_Group.Deactivate = 0 and Leave_Type.Deactivate = 0 and Leave_Setting.LeaveSettingLeaveGroupId = Leave_Group.LeaveGroupId and Leave_Type.LeaveTypeId = Leave_Setting.LeaveSettingLeaveTypeId and Leave_Setting.CmpId=" + LeaveOpeningBalance.CmpID + "");
                var LeaveType = DapperORM.ReturnList<LeaveTypeIdName>("sp_QueryExcution", param2).ToList();
                ViewBag.LeaveTypeName = LeaveType;



                List<LeaveOpeningBalance> excelDataList = new List<LeaveOpeningBalance>();

                if (AttachFile.ContentLength > 0)
                {

                    string directoryPath = Server.MapPath("~/assets/LeaveOpeningBalance");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    // Get the file name
                    string fileName = Path.GetFileName(AttachFile.FileName);

                    // Combine directory path with file name to get full file path
                    string filePath = Path.Combine(directoryPath, fileName);
                    // Save the file
                    AttachFile.SaveAs(filePath);

                    XLWorkbook xlWorkwook = new XLWorkbook(filePath);
                    int row = 3;
                    if (xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString() == "")
                    {

                        TempData["Message"] = "Fill in missing information in the first column.";
                        TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                        TempData["Icon"] = "error";
                        return RedirectToAction("ESS_TimeOffice_ManualAttendanceUpload", "ESS_TimeOffice_ManualAttendanceUpload");
                    }
                    while (xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString() != "")
                    {
                        LeaveOpeningBalance ManualInsert = new LeaveOpeningBalance();
                        ManualInsert.EmployeeNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString();
                        ManualInsert.EmployeeName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 2).GetString();
                        ManualInsert.BusinessUnit = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 3).GetString();
                        ManualInsert.DesignationName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 4).GetString();
                        ManualInsert.DepartmentName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 5).GetString();
                        ManualInsert.JoiningDate = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 6).GetString();
                        ManualInsert.LeaveYear = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 7).GetString();
                        ManualInsert.LeaveType = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 8).GetString();
                        //ManualInsert.YearlyLeave = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 9).GetString();
                        var yearlyLeaveCell = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 9);
                        ManualInsert.YearlyLeave = yearlyLeaveCell.CachedValue == null ? "" : yearlyLeaveCell.CachedValue.ToString();
                        excelDataList.Add(ManualInsert);
                        row++;
                    }
                    System.IO.File.Delete(filePath);
                    ViewBag.count = 1;
                    ViewBag.GetExceldata = excelDataList;
                }
                return View(LeaveOpeningBalance);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Save Update

        int List_CmpID_Id = 0;
        int List_Branch_Id = 0;
        int List_EmployeeId_Id = 0;
        int List_LeaveSettingLeaveGroupId_Id = 0;
        int List_LeaveSettingLeave_Id = 0;
        int List_LeaveSettingLeaveType_Id = 0;

        int CompanyID_Id = 0;
        int Branch_Id = 0;
        int EmployeeId_Id = 0;
        int LeaveSettingLeaveGroupId_Id = 0;
        int LeaveSettingLeave_Id = 0;
        int LeaveSettingLeaveType_Id = 0;
        [HttpPost]
        public ActionResult SaveUpadte(List<LeaveOpeningBalance> LeaveOpeningBalance, int? LeaveTypeId,int? SettingId, int? LeaveYearId, int? BranchId, int? CmpID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 391;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                dt_Employee = objcon.GetDataTable("Select EmployeeNo,Convert (int,Mas_Employee.EmployeeBranchId) as  EmployeeBranchId,Convert (int,Mas_Employee.CmpID) as CmpID, Convert (int,Mas_Employee.EmployeeId) as EmployeeId from Mas_Employee where Deactivate=0 and EmployeeLeft=0 and ContractorID=1");
                dt_LeaveSetting = objcon.GetDataTable("select   Convert (int,Leave_Setting.LeaveSettingId) as Id ,concat(Leave_Group.LeaveGroupName, ' - ', Leave_Type.LeaveTypeShortName) as Name,  Convert (int,Leave_Setting.CmpId) AS CmpId , Convert (int,Leave_Setting.LeaveSettingLeaveGroupId) AS LeaveSettingLeaveGroupId  , Convert (int,Leave_Setting.LeaveSettingLeaveTypeId) AS LeaveSettingLeaveTypeId  from Leave_Setting, Leave_Group, Leave_Type where Leave_Setting.Deactivate = 0 and Leave_Group.Deactivate = 0 and Leave_Type.Deactivate = 0 and Leave_Setting.LeaveSettingLeaveGroupId = Leave_Group.LeaveGroupId and Leave_Type.LeaveTypeId = Leave_Setting.LeaveSettingLeaveTypeId");

                //List_CmpID_Id = 0;
                //List_Branch_Id = 0;
                //List_EmployeeId_Id = 0;
                //List_LeaveSettingLeaveGroupId_Id = 0;
                //List_LeaveSettingLeave_Id = 0;
                //List_LeaveSettingLeaveType_Id = 0;

                //CompanyID_Id = 0;
                //Branch_Id = 0;
                //EmployeeId_Id = 0;
                //LeaveSettingLeaveGroupId_Id = 0;
                //LeaveSettingLeave_Id = 0;
                //LeaveSettingLeaveType_Id = 0;

                foreach (var Data in LeaveOpeningBalance)
                {

                    GetCompany_Id(Data.EmployeeNo.ToString(), out CompanyID_Id);
                    List_CmpID_Id = CompanyID_Id;

                    GetBranch_Id(Data.EmployeeNo.ToString(), out Branch_Id);
                    List_Branch_Id = Branch_Id;

                    GetEmployeeId_Id(Data.EmployeeNo.ToString(), List_Branch_Id, out EmployeeId_Id);
                    List_EmployeeId_Id = EmployeeId_Id;
                    if (EmployeeId_Id == 0)
                    {
                        var Message = "The employee does not belong to this business unit (EmployeeNo). " + Data.EmployeeNo;
                        var Icon = "error";
                        var rowno = Data.EmployeeNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }

                    GetLeaveSettingLeave_Id(Data.LeaveType.ToString(), List_CmpID_Id, out LeaveSettingLeave_Id);
                    List_LeaveSettingLeave_Id = LeaveSettingLeave_Id;
                    if (LeaveSettingLeave_Id == 0)
                    {
                        var Message = "Leave Type is invalid of (EmployeeNo). " + Data.EmployeeNo;
                        var Icon = "error";
                        var rowno = Data.EmployeeNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }


                    GetLeaveSettingLeaveGroupId(List_LeaveSettingLeave_Id, out LeaveSettingLeaveGroupId_Id);
                    List_LeaveSettingLeaveGroupId_Id = LeaveSettingLeaveGroupId_Id;

                    GetLeaveSettingLeaveTypeId(List_LeaveSettingLeave_Id, out LeaveSettingLeaveType_Id);
                    List_LeaveSettingLeaveType_Id = LeaveSettingLeaveType_Id;


                    var ALeaveYear_Id = DapperORM.DynamicQueryList("Select LeaveYearId from Leave_Year where IsActivate=1 and IsDefault=1 and CmpId=" + List_CmpID_Id + "").FirstOrDefault();
                    if (ALeaveYear_Id==null)
                    {
                        return Json(new { Message = "Leave Year Not Define", Icon = "error" }, JsonRequestBehavior.AllowGet);
                    }
                     
                    var YearId = ALeaveYear_Id.LeaveYearId;
                   
                    //var CmpID = DapperORM.DynamicQuerySingle("Select CmpID from Mas_Employee  where EmployeeId=" + a + "").FirstOrDefault();

                    param.Add("@p_process", "Save");
                    param.Add("@p_CmpID", List_CmpID_Id);
                    param.Add("@p_LeaveOpeningBalanceBranchID", List_Branch_Id);
                    //param.Add("@p_LeaveOpeningBalanceSettingId", LeaveTypeId);

                    param.Add("@p_LeaveOpeningBalanceSettingId", SettingId);
                    param.Add("@p_LeaveOpeningBalanceLeaveTypeId", LeaveTypeId);

                    param.Add("@p_LeaveOpeningBalanceLeaveYearId", LeaveYearId);
                    //param.Add("@p_LeaveOpeningBalanceId_Encrypted", LeaveOpeningBalance[i].LeaveOpeningBalanceId_Encrypted);
                    param.Add("@p_NoOfLeave", Data.YearlyLeave);
                    param.Add("@p_LeaveOpeningBalanceEmployeeId", List_EmployeeId_Id);
                    param.Add("@p_OpeningAdjCarryCredit_Remark", "Opening Balance");
                    param.Add("@p_Remark", "Bulk");
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_SUD_LeaveOpneingBalance", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
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

        #region GetC Id
        void GetCompany_Id(string Name, out int Company_Id)
        {
            try
            {
                var matchingRows = from row in dt_Employee.AsEnumerable()
                                   where string.Equals(row.Field<string>("EmployeeNo"), Name, StringComparison.OrdinalIgnoreCase)
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
                    Company_Id = Convert.ToInt32(varCompany_Id["CmpID"]);
                }
            }
            catch
            {
                Company_Id = 0;
            }
        }

        void GetBranch_Id(string Name, out int Branch_Id)
        {

            try
            {

                //var matchingRows = from row in dt_Contractor.AsEnumerable()
                //                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                //                   select row;

                var matchingRows = from row in dt_Employee.AsEnumerable()
                                   where string.Equals(row.Field<string>("EmployeeNo"), Name, StringComparison.OrdinalIgnoreCase)
                                   select row;
                var varBranch_Id = matchingRows.LastOrDefault();
                if (varBranch_Id == null)
                {
                    Branch_Id = 0;
                }
                else
                {
                    Branch_Id = Convert.ToInt32(varBranch_Id["EmployeeBranchId"]);
                }
            }
            catch (Exception ex)
            {
                Branch_Id = 0;
            }
        }

        void GetEmployeeId_Id(string Name, int List_Branch_Id, out int EmployeeId_Id)
        {
            try
            {
                // Find the row where EmployeeNo matches but belongs to a different branch
                var conflictingRow = dt_Employee.AsEnumerable()
                    .FirstOrDefault(row =>
                        string.Equals(row.Field<string>("EmployeeNo"), Name, StringComparison.OrdinalIgnoreCase) &&
                        row.Field<int>("EmployeeBranchId") != List_Branch_Id);

                if (conflictingRow != null)
                {
                    // EmployeeNo exists but belongs to a different branch
                    EmployeeId_Id = 0;
                }
                else
                {
                    // Check if EmployeeNo exists in the specified branch
                    var matchingRow = dt_Employee.AsEnumerable()
                        .FirstOrDefault(row =>
                            string.Equals(row.Field<string>("EmployeeNo"), Name, StringComparison.OrdinalIgnoreCase) &&
                            row.Field<int>("EmployeeBranchId") == List_Branch_Id);

                    if (matchingRow != null)
                    {
                        // EmployeeNo belongs to the specified branch, return EmployeeId
                        EmployeeId_Id = matchingRow.Field<int>("EmployeeId");
                    }
                    else
                    {
                        // EmployeeNo does not exist at all
                        EmployeeId_Id = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception (if necessary) and set default value
                Console.WriteLine($"Error: {ex.Message}");
                EmployeeId_Id = 0;
            }




            //try
            //{

            //    var matchingRows = from row in dt_Employee.AsEnumerable()
            //                       where string.Equals(row.Field<string>("EmployeeNo"), Name, StringComparison.OrdinalIgnoreCase)
            //                       select row;
            //    var varEmployee_Id = matchingRows.LastOrDefault();
            //    if (varEmployee_Id == null)
            //    {
            //        EmployeeId_Id = 0;
            //    }
            //    else
            //    {
            //        EmployeeId_Id = Convert.ToInt32(varEmployee_Id["EmployeeId"]);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    EmployeeId_Id = 0;
            //}
        }

        void GetLeaveSettingLeave_Id(string Name, int companyId, out int LeaveSettingLeave_Id)
        {

            try
            {

                //var matchingRows = from row in dt_Contractor.AsEnumerable()
                //                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                //                   select row;

                var matchingRows = from row in dt_LeaveSetting.AsEnumerable()
                                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                                         && row.Field<int>("CmpId") == Convert.ToInt16(companyId)
                                   select row;
                var varLeaveTypeId_Id = matchingRows.LastOrDefault();
                if (varLeaveTypeId_Id == null)
                {
                    LeaveSettingLeave_Id = 0;
                }
                else
                {
                    LeaveSettingLeave_Id = Convert.ToInt32(varLeaveTypeId_Id["Id"]);
                }
            }
            catch (Exception ex)
            {
                LeaveSettingLeave_Id = 0;
            }
        }

        void GetLeaveSettingLeaveGroupId(int List_LeaveSettingLeave_Id, out int LeaveSettingLeaveGroupId_Id)
        {
            try
            {
                // Filter rows where "Id" matches leaveSettingLeaveId
                var matchingRow = dt_LeaveSetting.AsEnumerable()
                                  .LastOrDefault(row => row.Field<int>("Id") == List_LeaveSettingLeave_Id);

                if (matchingRow != null)
                {
                    // Retrieve "LeaveSettingLeaveGroupId" if matching row is found
                    LeaveSettingLeaveGroupId_Id = matchingRow.Field<int>("LeaveSettingLeaveGroupId");
                }
                else
                {
                    LeaveSettingLeaveGroupId_Id = 0; // Default value if no match is found
                }
            }
            catch (Exception ex)
            {
                // Log exception if needed
                //  Console.WriteLine($"Error: {ex.Message}");
                LeaveSettingLeaveGroupId_Id = 0; // Default value in case of an error
            }
        }

        void GetLeaveSettingLeaveTypeId(int List_LeaveSettingLeave_Id, out int LeaveSettingLeaveType_Id)
        {
            try
            {
                // Filter rows where "Id" matches leaveSettingLeaveId
                var matchingRow = dt_LeaveSetting.AsEnumerable()
                                  .LastOrDefault(row => row.Field<int>("Id") == List_LeaveSettingLeave_Id);

                if (matchingRow != null)
                {
                    // Retrieve "LeaveSettingLeaveGroupId" if matching row is found
                    LeaveSettingLeaveType_Id = matchingRow.Field<int>("LeaveSettingLeaveTypeId");
                }
                else
                {
                    LeaveSettingLeaveType_Id = 0; // Default value if no match is found
                }
            }
            catch (Exception ex)
            {
                // Log exception if needed
                //  Console.WriteLine($"Error: {ex.Message}");
                LeaveSettingLeaveType_Id = 0; // Default value in case of an error
            }
        }
        #endregion

    }
}