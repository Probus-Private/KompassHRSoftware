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
    public class Module_Employee_EmployeeListController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: Module/Module_Employee_EmployeeList
        #region EmployeeList Main View
        public ActionResult Module_Employee_EmployeeList(int? id, int? ScreenId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 142;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                //}
                TempData["CheckEmpInStatutory"] = null;
                param.Add("@p_EmployeeID", Session["EmployeeId"]);
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Mas_Employee", param).ToList();
                ViewBag.GetEmployeeInfoList = data;


                if (ScreenId != null)
                {
                    Session["ModuleId"] = id;
                    Session["ScreenId"] = ScreenId;
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), id, ScreenId, "SubForm", "Transation");
                    ViewBag.GetUserMenuList = GetMenuList;
                }
                else
                {
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), Convert.ToInt32(Session["ModuleId"]), Convert.ToInt32(Session["ScreenId"]), "SubForm", "Transation");
                    ViewBag.GetUserMenuList = GetMenuList;
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
            return View();
        }
        #endregion

        #region OnboardingForm
        public ActionResult OnboardingForm(int? EmpId, string EmpName, string EmpDesignation, string EmployeeNo, string EmpDepartment, string EmpDoj, string CmpId, string BranchId, string EmpDepartmentId, string EmpDesignationId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                TempData["OnboardEmployeeId"] = EmpId;
                Session["OnboardEmployeeId"] = EmpId;
                Session["OnboardCmpId"] = CmpId;
                Session["OnboardBranchId"] = BranchId;
                Session["OnboardEmployeeName"] = EmpName;
                Session["OnboardEmpDesignation"] = EmpDesignation;
                Session["OnboardEmployeeNo"] = EmployeeNo;
                Session["OnboardEmpDepartment"] = EmpDepartment;
                Session["OnboardEmpDoj"] = EmpDoj;
                Session["OnboardEmpDepartmentId"] = EmpDepartmentId;
                Session["OnboardEmpDesignationId"] = EmpDesignationId;

                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EmployeeId", EmpId);
                var SetupFlag = DapperORM.ExecuteSP<dynamic>("sp_List_Mas_Employee_StatusCheck", paramList);
                //return Json(SetupFlag, JsonRequestBehavior.AllowGet);
                return Json(new { SetupFlag, EmpId = EmpId }, JsonRequestBehavior.AllowGet);
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