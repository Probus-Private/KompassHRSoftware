using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_Leave
{
    public class Reports_Leave_LeaveBalanceController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        // GET: Reports/Reports_Leave_LeaveBalance
        public ActionResult Reports_Leave_LeaveBalance(DailyAttendanceReportFilter MasReport)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var results = DapperORM.DynamicQueryMultiple(@"select CompanyId as Id, CompanyName as Name from Mas_CompanyProfile  where Deactivate = 0 order by CompanyName;                                                  
                                                  select DepartmentId as Id, DepartmentName as Name from Mas_Department where Deactivate=0 order by DepartmentName; 
                                                  select DesignationId as Id,DesignationName as Name from Mas_Designation where Deactivate=0 order by DesignationName
                                                  select GradeId as Id,GradeName as Name from Mas_Grade where Deactivate=0 order by Name");
                ViewBag.CompanyName = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.DepartmentName = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.DesignationName = results[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GradeName = results[3].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                //ViewBag.CompanyName = results.Read<AllDropDownClass>().ToList();
                //ViewBag.DepartmentName = results.Read<AllDropDownClass>().ToList();
                //ViewBag.DesignationName = results.Read<AllDropDownClass>().ToList();
                //ViewBag.GradeName = results.Read<AllDropDownClass>().ToList();

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
                        paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + MasReport.CmpId + " and EmployeeBranchId= " + MasReport.BranchId + "");
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
                    var GetFromDate = MasReport.FromDate.ToString("yyyy-MM-dd");
                    var GetToDate = MasReport.ToDate.ToString("yyyy-MM-dd");
                    if (MasReport.FromDate != null)
                    {
                        Query = Query + " and LeavingDate between '" + GetFromDate + "' and '" + GetToDate + "'";
                    }
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@P_Qry", Query);
                    var GetDepartmentWiseReport = DapperORM.ReturnList<dynamic>("sp_Rpt_Atten_DepartmentWiseDailyHeadCount", paramList).ToList();
                    ViewBag.DepartmentWiseReport = GetDepartmentWiseReport;

                    DynamicParameters paramBranchList = new DynamicParameters();
                    paramBranchList.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBranchList.Add("@p_CmpId", MasReport.CmpId);
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranchList).ToList();
                    ViewBag.BranchName = data;
                }
                else
                {
                    ViewBag.DepartmentWiseReport = "";
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
                return Json(new { data }, JsonRequestBehavior.AllowGet);
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
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpId + " and EmployeeBranchId= " + UnitBranchId + "");
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