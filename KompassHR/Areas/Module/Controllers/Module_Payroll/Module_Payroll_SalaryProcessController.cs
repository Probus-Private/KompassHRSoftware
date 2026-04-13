using Dapper;
using KompassHR.Areas.Module.Models.Module_Payroll;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace KompassHR.Areas.Module.Controllers.Module_Payroll
{
    public class Module_Payroll_SalaryProcessController : Controller
    {
        // GET: Module/Module_Payroll_SalaryProcess
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        public ActionResult Module_Payroll_SalaryProcess()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                //DailyAttendanceReportFilter ObjDailyAttendance = new DailyAttendanceReportFilter();

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@query", "Select  CompanyId As Id, CompanyName As Name from Mas_CompanyProfile where Deactivate = 0 order by Name");
                ViewBag.GetCompanyName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramCompany).ToList();
                ViewBag.GetBranchName = "";

                //if (obj.CmpId != 0)
                //{
                //    DynamicParameters paramList = new DynamicParameters();
                //    paramList.Add("@p_Cmpid", obj.CmpId);
                //    paramList.Add("@p_Branchid", obj.BranchId);
                //    paramList.Add("@p_AttenMonthYear", obj.Month);
                //    paramList.Add("@p_LoadWithLogData", obj.LoadWithLog);
                //    var data = DapperORM.ExecuteSP<dynamic>("SP_MonthlyAttendance_SWATI", paramList).ToList();
                //    ViewBag.SalaryProcess = data;

                //    DynamicParameters param = new DynamicParameters();
                //    param.Add("@p_employeeid", Session["EmployeeId"]);
                //    param.Add("@p_CmpId", obj.CmpId);
                //    ViewBag.GetBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                //}
                //else
                //{
                //    ViewBag.SalaryProcess = "";
                //}
                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }

        }
        #endregion

        #region Save
        [HttpPost]
        public ActionResult SaveUpdate(List<Payroll_Salary> ObjSalaryProcess, int CmpId, int BranchId, DateTime Month)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                //for (var i = 0; i < DesignationRecord.Count; i++)
                //{
                //    param.Add("@p_process", "Save");
                //    param.Add("@p_AssignDesignationEmployeeID", DesignationRecord[i].AssignDesignationEmployeeID);
                //    param.Add("@p_RecruitmentDesignationID", DesignationRecord[i].RecruitmentDesignationID);
                //    param.Add("@p_IsActive", DesignationRecord[i].IsActive);
                //    param.Add("@p_MachineName", Session["MachineName"]);
                //    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                //    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                //    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                //    var Result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_AssignDesignation", param);
                //    var msg = param.Get<string>("@p_msg");
                //    TempData["Message"] = param.Get<string>("@p_msg");
                //    TempData["Icon"] = param.Get<string>("@p_Icon");
                //}
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}