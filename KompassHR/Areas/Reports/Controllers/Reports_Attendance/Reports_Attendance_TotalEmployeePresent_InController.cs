using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_Attendance
{
    public class Reports_Attendance_TotalEmployeePresent_InController : Controller
    {
        // GET: Reports/Reports_Attendance_TotalEmployeePresent_In
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();

        public ActionResult Reports_Attendance_TotalEmployeePresent_In(DailyAttendanceReportFilter MasReport)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 361;
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
                ViewBag.BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpId + " and Mas_Employee.EmployeeLeft=0");
                ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_qry", " and Mas_ContractorMapping.BranchID in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ")");
                var GetContractorDropdown = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param2).ToList();
                ViewBag.GetContractor = GetContractorDropdown;


                var results = DapperORM.DynamicQueryMultiple(@"select DepartmentId as Id, DepartmentName as Name from Mas_Department where Deactivate=0 order by DepartmentName; 
                                                  select DesignationId as Id,DesignationName as Name from Mas_Designation where Deactivate=0 order by DesignationName
                                                  select GradeId as Id,GradeName as Name from Mas_Grade where Deactivate=0 order by Name");

                ViewBag.DepartmentName = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.DesignationName = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GradeName = results[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.SubDepartmentName = "";
                ViewBag.LineName = "";
                ViewBag.SubUnit = "";

                return View(MasReport);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        //#region GetBusinessUnit
        //[HttpGet]
        //public ActionResult GetBusinessUnit(int CmpId)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { area = "" });
        //        }

        //        DynamicParameters paramEmpName = new DynamicParameters();
        //        paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpId + " and Mas_Employee.EmployeeLeft=0");
        //        var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
        //        ViewBag.EmployeeName = EmployeeName;

        //        DynamicParameters paramLine = new DynamicParameters();
        //        paramLine.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + CmpId + " ");
        //        var List_LineMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();
        //        ViewBag.LineName = GetList_LineMaster;

        //        DynamicParameters param = new DynamicParameters();
        //        param.Add("@p_employeeid", Session["EmployeeId"]);
        //        param.Add("@p_CmpId", CmpId);
        //        var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
        //        return Json(new { EmployeeName = EmployeeName, List_LineMaster = List_LineMaster, Branch = Branch }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }

        //}
        //#endregion

        #region GetUnit
        [HttpGet]
        public ActionResult GetUnit(int? CmpId, int? UnitBranchId)
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
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpId + " and EmployeeBranchId= " + UnitBranchId + "and Mas_Employee.EmployeeLeft=0");
                    var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                    DynamicParameters paramLine = new DynamicParameters();
                    paramLine.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + CmpId + " and Mas_LineMaster.BranchId=" + UnitBranchId + " ");
                    var List_LineMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();

                    DynamicParameters paramUnit = new DynamicParameters();
                    paramLine.Add("@query", "select UnitId as Id,UnitName as Name from Mas_Unit where Deactivate=0 and CmpId=" + CmpId + " and UnitBranchId=" + UnitBranchId + " ");
                    var List_SubUnit = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();

                    return Json(new { EmployeeName = EmployeeName, List_LineMaster = List_LineMaster, List_SubUnit = List_SubUnit }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
                //DynamicParameters paramLine = new DynamicParameters();
                //paramLine.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + CmpId + " and Mas_LineMaster.BranchId=" + UnitBranchId + " order by Name");
                //var List_LineMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();
                //return Json(List_LineMaster, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetSubDepartment
        [HttpGet]
        public ActionResult GetSubDepartment(int? DepartmentId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (DepartmentId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "Select SubDepartmentId As Id,SubDepartmentName as Name from Mas_SubDepartment where Mas_SubDepartment.Deactivate=0 and Mas_SubDepartment.DepartmentId=" + DepartmentId + "");
                    var Data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    return Json(Data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
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
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, DateTime FromDate, int? SubUnitId, int? DepartmentId, int? SubDepartmentId, int? DesignationId, int? LineId, int? EmployeeID, int? GradeId,int ? ContractorId)
        {
            try
            {
               
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("TotalEmployeePresentInReport");
                worksheet.Range(1, 1, 1, 13).Merge();
                worksheet.SheetView.FreezeRows(2); // Freeze the row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();
                var GetFromDate = FromDate.ToString("yyyy-MM-dd");
                var Query = "";
                var Query1 = "";
                if (CmpId != null)
                {
                   

                  


                    if (BranchId != null)
                    {
                        Query1 = "and Mas_Employee.employeebranchid=" + BranchId + " ";
                    }
                    else
                    {
                        Query1 = "and Mas_Employee.employeebranchid in (select mas_branch.BranchID as Id from UserBranchMapping,mas_branch where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + CmpId + " and UserBranchMapping.IsActive = 1) ";
                    }

                    if (DepartmentId != null)
                    {
                        Query = Query + " where Mas_Department.DepartmentId=" + DepartmentId + "";
                    }

                    if (SubDepartmentId != null)
                    {
                        Query = Query + " and Mas_SubDepartment.SubDepartmentId=" + SubDepartmentId + "";
                    }

                    if (DesignationId != null)
                    {
                        Query = Query + " and Mas_Designation.DesignationId=" + DesignationId + "";
                    }

                    if (GradeId != null)
                    {
                        Query = Query + " and Mas_Grade.GradeId=" + GradeId + "";
                    }
                    if (SubUnitId != null)
                    {
                        Query = Query + " and Mas_Unit.UnitId=" + SubUnitId + "";
                    }
                    if (SubDepartmentId != null)
                    {
                        Query = Query + " and Mas_SubDepartment.SubDepartmentId=" + SubDepartmentId + "";
                    }

                    if (LineId != null)
                    {
                        Query = Query + " and Mas_LineMaster.LineId=" + LineId + "";
                    }
                    if (EmployeeID != null)
                    {
                        Query = Query + " and Mas_Employee.EmployeeID=" + EmployeeID + "";
                    }
                    if (ContractorId != null)
                    {
                        Query = Query + " and Contractor_Master.ContractorId=" + ContractorId + "";
                    }
                }

                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_Qry1", Query1);
                paramList.Add("@p_Qry2", Query);
                paramList.Add("@p_Date", GetFromDate);
                var GetTotalEmployeePresentInReport = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Atten_TotalEmployeePresent_IN", paramList).ToList();
                if (GetTotalEmployeePresentInReport.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }
                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(GetTotalEmployeePresentInReport);
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
                worksheet.Cell(1, 1).Value = "Total Employee Present In Report - (" + FromDate.ToString("dd/MMM/yyyy") + ")";
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Columns().AdjustToContents(); // This code for all clomns
                                                        // Set the header row background color to grey and font color to black
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

        #region Print
        public ActionResult Print(int? CmpId, int? BranchId, DateTime FromDate, int? SubUnitId, int? DepartmentId, int? SubDepartmentId, int? DesignationId, int? LineId, int? EmployeeID, int? GradeId, int? ContractorId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var GetFromDate = FromDate.ToString("yyyy-MM-dd");
                var Query = "";
                var Query1 = "";
                string companyName = "";

                if (CmpId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "SELECT CompanyName FROM Mas_CompanyProfile WHERE CompanyId = " + CmpId);

                    var result = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).SingleOrDefault();
                    companyName = result.CompanyName.ToString();

                    if (BranchId != null)
                    {
                        Query1 = "and Mas_Employee.employeebranchid=" + BranchId + " ";
                    }
                    else
                    {
                        Query1 = "and Mas_Employee.employeebranchid in (select mas_branch.BranchID as Id from UserBranchMapping,mas_branch where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + CmpId + " and UserBranchMapping.IsActive = 1) ";
                    }

                    if (DepartmentId != null)
                    {
                        Query = Query + " where Mas_Department.DepartmentId=" + DepartmentId + "";
                    }

                    if (SubDepartmentId != null)
                    {
                        Query = Query + " and Mas_SubDepartment.SubDepartmentId=" + SubDepartmentId + "";
                    }

                    if (DesignationId != null)
                    {
                        Query = Query + " and Mas_Designation.DesignationId=" + DesignationId + "";
                    }

                    if (GradeId != null)
                    {
                        Query = Query + " and Mas_Grade.GradeId=" + GradeId + "";
                    }
                    if (SubUnitId != null)
                    {
                        Query = Query + " and Mas_Unit.UnitId=" + SubUnitId + "";
                    }
                    if (SubDepartmentId != null)
                    {
                        Query = Query + " and Mas_SubDepartment.SubDepartmentId=" + SubDepartmentId + "";
                    }

                    if (LineId != null)
                    {
                        Query = Query + " and Mas_LineMaster.LineId=" + LineId + "";
                    }
                    if (EmployeeID != null)
                    {
                        Query = Query + " and Mas_Employee.EmployeeID=" + EmployeeID + "";
                    }
                    if (ContractorId != null)
                    {
                        Query = Query + " and Contractor_Master.ContractorId=" + ContractorId + "";
                    }
                }

                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_Qry1", Query1);
                paramList.Add("@p_Qry2", Query);
                paramList.Add("@p_Date", GetFromDate);
                var GetData = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Atten_TotalEmployeePresent_IN", paramList).ToList();

                if (GetData.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }

                var firstRow = (IDictionary<string, object>)GetData.First();
                var html = new System.Text.StringBuilder();

                // ✅ Define hidden columns BEFORE looping
                var hiddenColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "EmployeeCardNo",
                    "Company",
                    "BusinessUnit",
                    "Gender",
                    "Age",
                    "Service",
                    "SubDepartment",
                    "Grade",
                    "Contractor",
                    "AssessmentLevel",
                    "CriticalStageYesNo",
                    "CriticalStagName",
                    "Line",
                    "SubUnitName",
                    "AttendanceDate",
                    "ShiftName",
                    "PunchDateTime",
                    "CompanyName" // already skipped earlier
                };

                // Add a proper HTML document structure with <title>
                html.Append("<!DOCTYPE html><html><head>");
                html.Append($"<title>Total Employee Present IN Report- "+ FromDate.ToString("dd/MMM/yyyy")+ "</title>");
                html.Append("</head><body>");

                // Company Name (Centered & bold)
                if (!string.IsNullOrWhiteSpace(companyName))
                {
                    html.Append($"<h2 style='text-align:center;margin-bottom:5px;'>{companyName}</h2>");
                }

                // Report heading
                html.Append($"<h3 style='text-align:left;margin-bottom:10px;'>Total Employee Present IN Report- " + FromDate.ToString("dd/MMM/yyyy") + " </h3>");
                html.Append("<table border='1' style='border-collapse:collapse;width:100%;font-size:12px;'>");

                // Table header
                html.Append("<thead><tr style='background-color:#cddcac;font-weight:bold;'>");
                foreach (var col in firstRow.Keys)
                {
                    if (!hiddenColumns.Contains(col))
                    {
                        html.Append($"<th style='padding:5px;'>{col}</th>");
                    }
                }
                html.Append("</tr></thead>");

                // Table rows
                html.Append("<tbody>");
                foreach (var row in GetData)
                {
                    html.Append("<tr>");
                    foreach (var kv in (IDictionary<string, object>)row)
                    {
                        if (!hiddenColumns.Contains(kv.Key))
                        {
                            html.Append($"<td style='padding:5px;'>{kv.Value}</td>");
                        }
                    }
                    html.Append("</tr>");
                }
                html.Append("</tbody></table>");

                html.Append("</body></html>");

                // Return as HTML so it opens in browser for printing
                return Content(html.ToString(), "text/html");
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