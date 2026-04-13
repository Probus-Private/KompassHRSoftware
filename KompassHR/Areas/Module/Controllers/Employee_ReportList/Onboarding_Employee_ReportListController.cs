using Dapper;
using KompassHR.Areas.Module.Models.Employee_ReportList;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Employee_ReportList
{
    public class Onboarding_Employee_ReportListController : Controller
    {
        SqlConnection sqlcons = new SqlConnection(DapperORM.connectionStrings);
        DynamicParameters param = new DynamicParameters();
        // GET: Module/Onboarding_Employee_ReportList

        #region Onboarding_Employee_ReportList
        public ActionResult Onboarding_Employee_ReportList(Employee_ReportLists MasReport)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                Employee_ReportLists EMPList = new Employee_ReportLists();
                param.Add("@query", "select CompanyId AS Id,CompanyName As Name from Mas_CompanyProfile where Deactivate=0 order by CompanyName");
                var listMasCompanyProfile = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.ComapnyProfile = listMasCompanyProfile;
                if (MasReport.CompanyId != 0)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@query", "Select * from View_Onboarding_EmployeeList where CompanyId=" + MasReport.CompanyId + " and BranchId=" + MasReport.BranchId + " order by EmployeeName");
                    var EmployeeReportList = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramList).ToList();
                    ViewBag.GetEmployeeReportList = EmployeeReportList;

                    DynamicParameters paramBranchList = new DynamicParameters();
                    paramBranchList.Add("@query", "select BranchId AS Id,BranchName As Name from Mas_Branch where Deactivate=0 and CmpId=" + MasReport.CompanyId + " order by Name");
                    var listMasBranch = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranchList).ToList();
                    ViewBag.GetMasBranch = listMasBranch;
                }
                else
                {
                    ViewBag.GetEmployeeReportList = "";
                    ViewBag.GetMasBranch = "";
                }
                return View(EMPList);
            }
            catch (Exception ex)
            {
                throw;
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
            catch (Exception)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }
        #endregion
    }
}