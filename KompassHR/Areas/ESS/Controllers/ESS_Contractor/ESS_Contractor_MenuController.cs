using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace KompassHR.Areas.ESS.Controllers.ESS_Contractor
{
    public class ESS_Contractor_MenuController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: ESS/ESS_Contractor_Menu
        #region ESS_Contractor_Menu
        public ActionResult ESS_Contractor_Menu(int? id, int? ScreenId)
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
                Session["WorkerOnboardEmployeeId"] = null;
                Session["WorkerOnboardCmpId"] = null;
                Session["WorkerOnboardBranchId"] = null;
                Session["WorkerOnboardEmployeeName"] = null;
                Session["WorkerOnboardEmpDesignation"] = null;
                Session["WorkerOnboardEmployeeNo"] = null;
                Session["WorkerOnboardEmpDepartment"] = null;
                Session["WorkerOnboardEmpDoj"] = null;
                Session["WorkerOnboardEmpDepartmentId"] = null;
                Session["WorkerFirstName"] = null;
                Session["WorkerContractorName"] = null;
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                var GetDashbordCount = DapperORM.ExecuteSP<dynamic>("sp_ContractorDashboardCount", paramList).ToList(); // SP_getReportingManager
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