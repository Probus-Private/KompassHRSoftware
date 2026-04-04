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

namespace KompassHR.Areas.Reports.Controllers.Reports_Attendance
{
    public class Rpt_Department_wise_CountController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Reports/Rpt_Department_wise_Count
        public ActionResult Rpt_Department_wise_Count(EmployeeWiseReportFilter Obj,int? ScreenSubId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param1 = new DynamicParameters();
                ViewBag.AddUpdateTitle = "Add";

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@query", "select CompanyId as Id, CompanyName as Name from Mas_CompanyProfile  where Deactivate = 0  and  CompanyId in ( Select distinct CmpID from UserBranchMapping where IsActive=1 and EmployeeID=" + Session["EmployeeId"] + ") order by CompanyName ;");
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramCompany).ToList();
                ViewBag.ComapnyName = GetComapnyName;


                if (Obj.FromDate != null || Obj.ToDate != null)
                {
                    param.Add("@p_EmployeeBranchId", Obj.BranchId);
                    param.Add("@p_FromDate", Obj.FromDate);
                    //param.Add("@p_ToDate", Obj.ToDate);
                    var data = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Atten_DepartmentWiseDailyHeadCount", param).ToList();
                    ViewBag.DepartmentWiseReport = data;

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@p_employeeid", Session["EmployeeId"]);
                    param2.Add("@p_CmpId", Obj.CmpId);
                    ViewBag.BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param2).ToList();
                }
                else
                {
                    ViewBag.DepartmentWiseReport = "";
                    ViewBag.BranchName = "";
                }
                return View(Obj);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetBusinessUnit(int? CmpId)
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
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetEmployeeName
        [HttpGet]
        public ActionResult GetEmployeeName(int BranchId)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", @"select EmployeeId as Id ,EmployeeName as Name from Mas_Employee  where Deactivate=0 and EmployeeBranchId=" + BranchId + "");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
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