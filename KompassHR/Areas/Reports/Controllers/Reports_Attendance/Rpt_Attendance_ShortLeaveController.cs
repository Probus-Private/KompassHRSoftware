using Dapper;
using KompassHR.Areas.Reports.Models;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_Attendance
{
    public class Rpt_Attendance_ShortLeaveController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Reports/Rpt_Attendance_ShortLeave
        #region Rpt_Attendance_ShortLeave
        public ActionResult Rpt_Attendance_ShortLeave(DailyAttendanceReportFilter MasReport) //DailyAttendanceReportFilter Daily_AttendanceReportFilter
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
                ViewBag.CompanyName = GetComapnyName;


                var results = DapperORM.DynamicQueryMultiple(@"select DepartmentId as Id, DepartmentName as Name from Mas_Department where Deactivate=0 order by DepartmentName; 
                                                  select DesignationId as Id,DesignationName as Name from Mas_Designation where Deactivate=0 order by DesignationName
                                                  select GradeId as Id,GradeName as Name from Mas_Grade where Deactivate=0 order by Name");

                //ViewBag.CompanyName = results.Read<AllDropDownClass>().ToList();
                ViewBag.DepartmentName = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.DesignationName = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GradeName = results[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                var Query = "";
                if (MasReport.CmpId != null)
                {
                    if (MasReport.BranchId != null)
                    {
                        Query = "and Mas_Branch.BranchId=" + MasReport.BranchId + "";
                        DynamicParameters paramLine = new DynamicParameters();
                        paramLine.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + MasReport.CmpId + " and Mas_LineMaster.BranchId=" + MasReport.BranchId + " ");
                        var List_LineMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();
                        ViewBag.LineName = List_LineMaster;

                        DynamicParameters paramEMP = new DynamicParameters();
                        paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + MasReport.CmpId + " and EmployeeBranchId= " + MasReport.BranchId + "and Mas_Employee.EmployeeLeft=0");
                        var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                        ViewBag.EmployeeName = GetEmployeeName;
                    }
                    else
                    {
                        Query = "and mas_branch.BranchId in (select mas_branch.BranchID as Id from UserBranchMapping,mas_branch where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + MasReport.CmpId + " and UserBranchMapping.IsActive = 1)";
                        ViewBag.LineName = "";
                        ViewBag.EmployeeName = "";
                    }

                    if (MasReport.DepartmentId != null)
                    {
                        Query = Query + " and Mas_Department.DepartmentId=" + MasReport.DepartmentId + "";

                        DynamicParameters paramSub = new DynamicParameters();
                        paramSub.Add("@query", "Select SubDepartmentId As Id,SubDepartmentName as Name from Mas_SubDepartment where Mas_SubDepartment.Deactivate=0 and Mas_SubDepartment.DepartmentId=" + MasReport.DepartmentId + "");
                        var DataDept = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramSub).ToList();
                        ViewBag.SubDepartmentName = DataDept;
                    }
                    else
                    {
                        ViewBag.SubDepartmentName = "";
                    }
                    if (MasReport.SubDepartmentId != null)
                    {
                        Query = Query + " and Mas_SubDepartment.SubDepartmentId=" + MasReport.SubDepartmentId + "";
                    }
                    if (MasReport.DesignationId != null)
                    {
                        Query = Query + " and Mas_Designation.DesignationId=" + MasReport.DesignationId + "";
                    }
                    if (MasReport.GradeId != null)
                    {
                        Query = Query + " and Mas_Grade.GradeId=" + MasReport.GradeId + "";
                    }
                    if (MasReport.LineId != null)
                    {
                        Query = Query + " and Mas_LineMaster.LineId=" + MasReport.LineId + "";
                    }
                    if (MasReport.EmployeeID != null)
                    {
                        Query = Query + " and Mas_Employee.EmployeeID=" + MasReport.EmployeeID + "";
                    }
                    var GetFromDate = MasReport.FromDate.ToString("yyyy-MM-dd");
                    var GetToDate = MasReport.ToDate.ToString("yyyy-MM-dd");
                    if (MasReport.FromDate != null)
                    {
                        Query = Query + " and convert(date,Leave_ShortLeave.ShortLeaveDate) between '" + GetFromDate + "' and '" + GetToDate + "'";
                    }
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@P_Qry", Query);
                    var ShortLeaveReport = DapperORM.ReturnList<dynamic>("sp_List_Leave_ShortLeave", paramList).ToList();
                    ViewBag.ShortLeaveReport = ShortLeaveReport;

                    DynamicParameters paramBranchList = new DynamicParameters();
                    paramBranchList.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBranchList.Add("@p_CmpId", MasReport.CmpId);
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranchList).ToList();
                    ViewBag.BranchName = data;
                }
                else
                {
                    ViewBag.ShortLeaveReport = "";
                    ViewBag.BranchName = "";
                    ViewBag.SubDepartmentName = "";
                    ViewBag.LineName = "";
                    ViewBag.EmployeeName = "";
                }
                return View(MasReport);
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

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpId + " and Mas_Employee.EmployeeLeft=0");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                //ViewBag.EmployeeName = EmployeeName;

                DynamicParameters paramLine = new DynamicParameters();
                paramLine.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + CmpId + " ");
                var List_LineMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();
                //ViewBag.LineName = GetList_LineMaster;

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                return Json(new { EmployeeName = EmployeeName, List_LineMaster = List_LineMaster, Branch = Branch }, JsonRequestBehavior.AllowGet);
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

    }
}