using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
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

namespace KompassHR.Areas.ESS.Controllers.ESS_TimeOffice
{
    public class ESS_TimeOffice_AttendanceRosterController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        // GET: TMSSetting/ProjectMaster
        public ActionResult ESS_TimeOffice_AttendanceRoster(AttenRoster AttenRoster)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 662;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.CompanyName = GetComapnyName;

                ViewBag.GetBusinessUnit = "";
                ViewBag.GetManagerName = "";
                ViewBag.Employees = "";
                ViewBag.GetShiftList = "";
                if (AttenRoster.RosterCmpID > 0)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    //   DynamicParameters param1 = new DynamicParameters();
                    //   param1.Add("@p_RosterID_Encrypted", AttenRoster.RosterID_Encrypted);
                    //   AttenRoster = DapperORM.ReturnList<AttenRoster>("sp_List_Atten_Cycle", param1).FirstOrDefault();
                    if (AttenRoster.RosterManagerID > 0)
                    {
                        DynamicParameters param = new DynamicParameters();
                        param.Add("@p_employeeid", Session["EmployeeId"]);
                        param.Add("@p_CmpId", AttenRoster.RosterCmpID);
                        var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                        ViewBag.GetBusinessUnit = data;
                    }
                    else
                    {
                        ViewBag.GetBusinessUnit = "";
                    }
                }
                return View(AttenRoster);
            }

            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetBusinessUnit(int RosterCmpID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", RosterCmpID);

                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                var BranchId = Branch[0].Id;

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@p_CmpId", RosterCmpID);
                paramEmpName.Add("@p_BranchId", "0");
                var ManagerName = DapperORM.ReturnList<AllDropDownBind>("sp_GetManagerDropdown", paramEmpName).ToList();

                //DynamicParameters paramShift = new DynamicParameters();
                //paramShift.Add("@query", "select distinct Atten_Shifts.ShiftId as Id  ,Atten_Shifts.ShiftName + ' ( ' + RIGHT(CONVERT(VARCHAR, Atten_Shifts.BeginTime, 100), 7) + '-' + RIGHT(CONVERT(VARCHAR, Atten_Shifts.EndTime, 100), 7) + ' )' as Name from Atten_Shifts where Deactivate=0 and CmpId='" + RosterCmpID + "' and Atten_Shifts.ShiftName not in ('Flexi Shift')");
                //var Shift = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramShift).ToList();
                return Json(new { Branch = Branch, ManagerName = ManagerName }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        [HttpGet]
        public ActionResult GetEmployee(int? RosterBranchID, int? RosterCmpID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_CmpId", RosterCmpID);
                param.Add("@p_BranchId", RosterBranchID);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetManagerDropdown", param).ToList();

                DynamicParameters paramShift = new DynamicParameters();
                paramShift.Add("@query", "select distinct Atten_Shifts.ShiftId as Id  ,Atten_Shifts.ShiftName + ' ( ' + RIGHT(CONVERT(VARCHAR, Atten_Shifts.BeginTime, 100), 7) + '-' + RIGHT(CONVERT(VARCHAR, Atten_Shifts.EndTime, 100), 7) + ' )' as Name from Atten_Shifts where Deactivate=0 and CmpId='" + RosterCmpID + "' and ShiftBranchId='" + RosterBranchID + "' and Atten_Shifts.ShiftName not in ('Flexi Shift')");
                var Shift = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramShift).ToList();
                return Json(new { data = data, Shift = Shift }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        #region GetEmployeeList
        [HttpGet]
        public ActionResult GetEmployeeList(int RosterCmpID, int? RosterBranchID, int? RosterManagerID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_CmpId", RosterCmpID);
                param2.Add("@p_BranchId", RosterBranchID);
                param2.Add("@p_EmployeeId", RosterManagerID);

                var data = DapperORM.ReturnList<AttenRoster>("sp_GetEmployeeForAttedanceRoster", param2).ToList();
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
        [HttpPost]
        public ActionResult SaveUpdate(string RosterCmpID, string RosterBranchID, DateTime RosterMonth, List<AttenRoster> EmployeeList, List<AttenRoster> AttedanceRosterList)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                // Check user access
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 662;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                string message = null;
                string icon = null;

                foreach (var employee in EmployeeList)
                {
                    foreach (var roster in AttedanceRosterList)
                    {
                        var param = new DynamicParameters();
                        param.Add("@p_process", "Save");
                        param.Add("@p_RosterCmpID", RosterCmpID);
                        param.Add("@p_RosterBranchID", RosterBranchID);
                        param.Add("@p_RosterEmployeeID", employee.EmployeeId);
                        param.Add("@p_RosterMonthYear", RosterMonth);
                        param.Add("@p_FromDate", roster.FromDate);
                        param.Add("@p_ToDate", roster.ToDate);
                        param.Add("@p_ShiftId", roster.ShiftId);
                        param.Add("@p_Is_WO", roster.Is_WO); // Ensure Is_WO is 1 or 0
                        param.Add("@p_MachineName", Dns.GetHostName());
                        param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                        param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                        var data = DapperORM.ExecuteReturn("sp_SUD_AttenRoster", param);
                        message = param.Get<string>("@p_msg");
                        icon = param.Get<string>("@p_Icon");
                        string p_Id = param.Get<string>("@p_Id"); // Optional: use if needed

                    }
                }

                TempData["Message"] = message ?? "Success";
                TempData["Icon"] = icon ?? "success";
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return Json(new { Message = ex.Message, Icon = "error" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region GetList
        [HttpGet]
        public ActionResult GetList(AttenRoster OBJList)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 662;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                if (OBJList.RosterMonthYear != DateTime.MinValue)
                {
                    param.Add("@p_Origin", "List");
                    param.Add("@p_MonthYear", OBJList.RosterMonthYear);
                    var data = DapperORM.ReturnList<AttenRoster>("sp_List_Atten_Roster", param).ToList();
                    ViewBag.GetAttenRosterList = data;
                }
                else
                {
                    ViewBag.GetAttenRosterList = "";
                }
                return View(OBJList);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(DateTime RosterMonthYear)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("AttendanceRoster");
                worksheet.Range(1, 1, 1, 23).Merge();  //Merge First Row with 60 Clomn
                worksheet.SheetView.FreezeRows(2); // Freeze the row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();
                param.Add("@p_Origin", "Export");
                param.Add("@p_MonthYear", RosterMonthYear);
                data = DapperORM.ExecuteSP<dynamic>("sp_List_Atten_Roster", param).ToList();
                if (data.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }
                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(data);
                worksheet.Cell(2, 1).InsertTable(dt, false);

                var usedRange = worksheet.RangeUsed();
                usedRange.Style.Fill.BackgroundColor = XLColor.White;
                usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Font.FontSize = 10;
                usedRange.Style.Font.FontColor = XLColor.Black;

                // Set the header row name
                worksheet.Cell(1, 1).Value = "Attendance Roster "; // Replace "Excel Name" with the actual name
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(1, 1).Style.Font.Bold = true;

                worksheet.Columns().AdjustToContents(); // This code for all clomns
                var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                headerRange.Style.Font.FontSize = 10;
                headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
                headerRange.Style.Font.Bold = true;

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

        public ActionResult Delete(string RosterID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 662;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_WOID", RosterID);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_AttenRoster", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_TimeOffice_AttendanceRoster");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
    }
}