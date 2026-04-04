using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_MenuController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: Module/Module_Employee_Menu
        #region Menu View
        public ActionResult Module_Employee_Menu(int? id,int? ScreenId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (ScreenId != null)
                {
                    Session["ModuleId"] = id;
                    Session["ScreenId"] = ScreenId;
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), id, ScreenId, "Form", "Transation");
                    ViewBag.GetUserMenuList = GetMenuList;
                }
                else
                {
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), Convert.ToInt32(Session["ModuleId"]), Convert.ToInt32(Session["ScreenId"]), "Form", "Transation");
                    ViewBag.GetUserMenuList = GetMenuList;
                }

                Session["OnboardEmployeeId"] = null;
                Session["OnboardCmpId"] = null;
                Session["OnboardBranchId"] = null;
                Session["OnboardEmployeeName"] = null;
                Session["OnboardEmpDesignation"] = null;
                Session["OnboardEmployeeNo"] = null;
                Session["OnboardEmpDepartment"] = null;
                Session["OnboardEmpDoj"] = null;
                Session["OnboardEmpDepartmentId"] = null;
                Session["TransferOrigin"] = null;
                Session["OnboardingContractorDropdownAccess"] = null;

                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                var GetDashbordCount = DapperORM.ExecuteSP<dynamic>("sp_OnboardingDashboardCount", paramList).ToList(); // SP_getReportingManager
                TempData["ActiveEmployeeCount"] = GetDashbordCount[0].EmpCount;
                TempData["LeftEmployeeCount"] = GetDashbordCount[1].EmpCount;
                TempData["NewJoiningEmployeeCount"] = GetDashbordCount[2].EmpCount;
                TempData["MaleEmployeeCount"] = GetDashbordCount[3].EmpCount;
                TempData["FemaleEmployeeCount"] = GetDashbordCount[4].EmpCount;
                TempData["TenuredEmployeeCount"] = GetDashbordCount[5].EmpCount;
                return View();
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