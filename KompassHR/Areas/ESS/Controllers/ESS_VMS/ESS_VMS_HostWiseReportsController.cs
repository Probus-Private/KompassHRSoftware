using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Leave;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_VMS
{
    public class ESS_VMS_HostWiseReportsController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_VMS_HostWiseReports
        #region ESS_VMS_HostWiseReports
        public ActionResult ESS_VMS_HostWiseReports()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 192;
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

                var CmpId = GetComapnyName[0].Id;
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                ViewBag.GetBusinessUnit = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpId + " and Mas_Employee.EmployeeLeft=0");
                ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region GetHostWiseReports
        [HttpPost]
        public ActionResult ESS_VMS_HostWiseReports(string Fromdate, string Todate, string companyName, string businessUnit, int EmployeeName)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@query", "Select  CompanyId, CompanyName from Mas_CompanyProfile where Deactivate=0 order by CompanyName");
                var GetCompanyName = DapperORM.ReturnList<Setting.Models.Setting_PolicyAndLibrary.Mas_CompanyProfile>("sp_QueryExcution", param).ToList();
                ViewBag.CompanyName = GetCompanyName;

                param.Add("@query", "select EmployeeId,EmployeeName from Mas_Employee where Deactivate=0 order by EmployeeName");
                var GetEmployeeId = DapperORM.ReturnList<Mas_Employee>("sp_QueryExcution", param).ToList();
                ViewBag.EmployeeName = GetEmployeeId;

                param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", companyName);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                ViewBag.GetBusinessUnit = data;

                param = new DynamicParameters();
                //param.Add("@P_Qry", "AND Visitor_Appointment.AppointmentDate >= '" + Fromdate + "' AND Visitor_Appointment.AppointmentDate <= '" + Todate + "' AND Mas_CompanyProfile.CompanyId = '" + companyName + "' AND Mas_Branch.BranchId = '" + businessUnit + "' AND Visitor_Appointment.VisitorAppointmentEmployeeID='" + EmployeeName + "'");
                param.Add("@P_Qry", "AND Visitor_Appointment.AppointmentDate >= '" + Fromdate + "' AND Visitor_Appointment.AppointmentDate <= '" + Todate + "' AND Mas_CompanyProfile.CompanyId = '" + companyName + "' AND Mas_Branch.BranchId = '" + businessUnit + "' and Visitor_Appointment.VisitorAppointmentEmployeeID = '" + EmployeeName + "' and Visitor_Appointment.Deactivate = 0");
                //param.Add("@P_Qry", "and convert(date, Visitor_Appointment.AppointmentDate , 106) between '" + Fromdate + "' and '" + Todate + "' and Mas_CompanyProfile.CompanyId = '" + companyName + "' and Mas_Branch.BranchId = '" + businessUnit + "' and Visitor_Appointment.VisitorAppointmentEmployeeID = '" + EmployeeName + "' and Visitor_Appointment.Deactivate = 0");
                var getHostWiseReport = DapperORM.ExecuteSP<dynamic>("sp_GetVisitorListFromDateToDate", param).ToList();
                ViewBag.GetHostWiseReport = getHostWiseReport;


                return View();
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

        #region GetUnit
        [HttpGet]
        public ActionResult GetUnit(int? UnitBranchId, int? CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (UnitBranchId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId= " + UnitBranchId + "and Mas_Employee.EmployeeLeft=0");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and Mas_Employee.EmployeeBranchId in (select BranchID as Id from UserBranchMapping where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and UserBranchMapping.CmpID = " + CmpId + " and UserBranchMapping.IsActive = 1) and Mas_Employee.EmployeeLeft=0");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, DateTime FromDate, DateTime ToDate, int? EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("HostWiseReports");
                worksheet.Range(1, 1, 1, 35).Merge();
                worksheet.SheetView.FreezeRows(2); // Freeze the first row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();
                var GetFromDate = FromDate.ToString("yyyy-MM-dd");
                var GetToDate = ToDate.ToString("yyyy-MM-dd");
                var Query = "";
                if (CmpId != null)
                {
                    Query = " and Mas_Employee.CmpId=" + CmpId + "";
                    if (BranchId != null)
                    {
                        Query = Query + " AND Mas_Employee.EmployeeBranchId = " + BranchId + "";
                    }
                    else
                    {
                        Query = Query + " and Mas_Employee.EmployeeBranchId in (select BranchID as Id from UserBranchMapping where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and UserBranchMapping.CmpID = " + CmpId + " and UserBranchMapping.IsActive = 1)";
                    }
                    if (EmployeeId != null)
                    {
                        Query = Query + "  and Mas_Employee.EmployeeId=" + EmployeeId + "";
                    }

                    Query = Query + " and convert(date, Visitor_Tra_Master.InDateTime, 106) between '" + GetFromDate + "' and '" + GetToDate + "'";

                }

                DynamicParameters paramList = new DynamicParameters();

                paramList.Add("@P_Qry", Query);
                var getDateWiseReports = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Visitor_DetailList", paramList).ToList();
                if (getDateWiseReports.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }
                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(getDateWiseReports);
                worksheet.Cell(2, 1).InsertTable(dt, false);

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
                worksheet.Cell(1, 1).Value = "Host Wise Reports - (" + FromDate.ToString("dd/MMM/yyyy") + " - " + ToDate.ToString("dd/MMM/yyyy") + ")";
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

    }
}