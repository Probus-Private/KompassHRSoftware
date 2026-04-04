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

namespace KompassHR.Areas.Reports.Controllers.Reports_Leave
{
    public class Reports_Leave_PostLeaveByAdminController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Reports/Reports_Leave_PostLeaveByAdmin
        #region Reports_Leave_PostLeaveByAdmin
        public ActionResult Reports_Leave_PostLeaveByAdmin(DailyAttendanceReportFilter MasReport)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 320;
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

                var results = DapperORM.DynamicQueryMultiple(@"select DepartmentId as Id, DepartmentName as Name from Mas_Department where Deactivate=0 order by DepartmentName; 
                                                  select DesignationId as Id,DesignationName as Name from Mas_Designation where Deactivate=0 order by DesignationName
                                                  select GradeId as Id,GradeName as Name from Mas_Grade where Deactivate=0 order by Name");
                ViewBag.DepartmentName = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.DesignationName = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GradeName = results[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                //ViewBag.DepartmentName = results.Read<AllDropDownClass>().ToList();
                //ViewBag.DesignationName = results.Read<AllDropDownClass>().ToList();
                //ViewBag.GradeName = results.Read<AllDropDownClass>().ToList();
                ViewBag.DailyAttendanceReport = "";
                ViewBag.SubDepartmentName = "";
                ViewBag.LineName = "";
                ViewBag.SubUnit = "";


                //var Query = "";
                //var Query1 = "";
                //if (MasReport.CmpId != null)
                //{
                //    if (MasReport.BranchId != null)
                //    {
                //        Query = "and Mas_Branch.BranchId=" + MasReport.BranchId + "";

                //        DynamicParameters paramLine = new DynamicParameters();
                //        paramLine.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + MasReport.CmpId + " and Mas_LineMaster.BranchId=" + MasReport.BranchId + " ");
                //        var List_LineMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();
                //        ViewBag.LineName = List_LineMaster;

                //        DynamicParameters paramEMP = new DynamicParameters();
                //        paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + MasReport.CmpId + " and EmployeeBranchId= " + MasReport.BranchId + "and Mas_Employee.EmployeeLeft=0");
                //        var SGetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                //        ViewBag.EmployeeName = SGetEmployeeName;

                //    }
                //    else
                //    {
                //        Query = " where BranchId in (select mas_branch.BranchID as Id from UserBranchMapping,mas_branch where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + MasReport.CmpId + " and UserBranchMapping.IsActive = 1)";
                //        ViewBag.LineName = "";
                //        ViewBag.EmployeeName = "";
                //    }

                //    if (MasReport.DepartmentId != null)
                //    {
                //        Query = Query + " and  mas_department.DepartmentId=" + MasReport.DepartmentId + "";

                //        DynamicParameters paramSub = new DynamicParameters();
                //        paramSub.Add("@query", "Select SubDepartmentId As Id,SubDepartmentName as Name from Mas_SubDepartment where Mas_SubDepartment.Deactivate=0 and Mas_SubDepartment.DepartmentId=" + MasReport.DepartmentId + "");
                //        var DataDept = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramSub).ToList();
                //        ViewBag.SubDepartmentName = DataDept;

                //    }
                //    else
                //    {
                //        ViewBag.SubDepartmentName = "";
                //    }


                //    if (MasReport.SubDepartmentId != null)
                //    {
                //        Query = Query + " and Mas_SubDepartment.SubDepartmentId=" + MasReport.SubDepartmentId + "";
                //    }


                //    if (MasReport.DesignationId != null)
                //    {
                //        Query = Query + " and Mas_designation.DesignationId=" + MasReport.DesignationId + "";
                //    }


                //    if (MasReport.GradeId != null)
                //    {
                //        Query = Query + " and Mas_Grade.GradeId=" + MasReport.GradeId + "";
                //    }


                //    if (MasReport.LineId != null)
                //    {
                //        Query = Query + " and Mas_LineMaster.LineId=" + MasReport.LineId + "";
                //    }
                //    if (MasReport.EmployeeID != null)
                //    {
                //        Query = Query + " and EmployeeID=" + MasReport.EmployeeID + "";
                //        Query1 = Query1 + " and EmployeeID=" + MasReport.EmployeeID + "";
                //    }
                //    if (MasReport.FromDate != null && MasReport.ToDate != null)
                //    {
                //        Query = Query + "and Leave_Master.FromDate >='" + MasReport.FromDate + "' and Leave_Master.FromDate <='" + MasReport.ToDate + "'";

                //    }

                //    DynamicParameters paramList = new DynamicParameters();
                //    paramList.Add("@P_Qry", Query);
                //    var GetPostLeaveByAdmin = DapperORM.ReturnList<dynamic>("sp_List_Leave_PostByAdmin", paramList).ToList();
                //    ViewBag.GetReportGetPostLeaveByAdmin = GetPostLeaveByAdmin;

                //    DynamicParameters paramBranchList = new DynamicParameters();
                //    paramBranchList.Add("@p_employeeid", Session["EmployeeId"]);
                //    paramBranchList.Add("@p_CmpId", MasReport.CmpId);
                //    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranchList).ToList();
                //    ViewBag.BranchName = data;

                //    TempData["ShowData"] = true;
                //}
                //else
                //{
                //    ViewBag.LateComingReport = "";
                //    ViewBag.BranchName = "";
                //    ViewBag.SubDepartmentName = "";
                //    ViewBag.LineName = "";
                //    ViewBag.GetReportGetPostLeaveByAdmin = "";
                //    //ViewBag.EmployeeName = "";
                //}
                return View(MasReport);
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

                    return Json(new { EmployeeName = EmployeeName, List_LineMaster = List_LineMaster }, JsonRequestBehavior.AllowGet);
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
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, DateTime FromDate, int? SubUnitId, int? DepartmentId, int? SubDepartmentId, int? DesignationId, int? LineId, int? EmployeeID, int? GradeId, DateTime ToDate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("PostLeaveByAdminReport");
                worksheet.Range(1, 1, 1, 35).Merge();
                worksheet.SheetView.FreezeRows(2); // Freeze the first row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();

                var Query = "";
                var Query1 = "";
                if (CmpId != null)
                {
                    if (BranchId != null)
                    {
                        Query = "and Mas_Branch.BranchId=" + BranchId + "";
                    }
                    else
                    {
                        Query = " where BranchId in (select UserBranchMapping.BranchID as Id from UserBranchMapping where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + "  and UserBranchMapping.CmpID = " + CmpId + " and UserBranchMapping.IsActive = 1 union select EmployeeBranchId as Id  from Mas_Employee where  EmployeeId= " + Session["EmployeeId"] + "  and Mas_Employee.CmpID= " + CmpId + " )";
                    }

                    if (DepartmentId != null)
                    {
                        Query = Query + " and  mas_department.DepartmentId=" + DepartmentId + "";
                    }

                    if (SubDepartmentId != null)
                    {
                        Query = Query + " and Mas_SubDepartment.SubDepartmentId=" + SubDepartmentId + "";
                    }

                    if (DesignationId != null)
                    {
                        Query = Query + " and Mas_designation.DesignationId=" + DesignationId + "";
                    }

                    if (GradeId != null)
                    {
                        Query = Query + " and Mas_Grade.GradeId=" + GradeId + "";
                    }

                    if (LineId != null)
                    {
                        Query = Query + " and Mas_LineMaster.LineId=" + LineId + "";
                    }
                    if (EmployeeID != null)
                    {
                        Query = Query + " and EmployeeID=" + EmployeeID + "";
                        Query1 = Query1 + " and EmployeeID=" + EmployeeID + "";
                    }
                    if (FromDate != null && ToDate != null)
                    {
                        Query = Query + "and Leave_Master.FromDate >='" + FromDate + "' and Leave_Master.FromDate <='" + ToDate + "'";

                    }
                }
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@P_Qry", Query);
                var GetPostLeaveByAdmin = DapperORM.ReturnList<dynamic>("sp_List_Leave_PostByAdmin", paramList).ToList();
                if (GetPostLeaveByAdmin.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }
                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(GetPostLeaveByAdmin);
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
                worksheet.Cell(1, 1).Value = "Post Leave By Admin Report- (" + FromDate.ToString("dd/MMM/yyyy") + " - " + ToDate.ToString("dd/MMM/yyyy") + ")";
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