using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_FNF
{
    public class ESS_FNF_FNFCalculationController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: Module/Module_Employee_EmployeeList
        #region EmployeeList Main View
        public ActionResult ESS_FNF_FNFCalculation(int? id, int? ScreenId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 607;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                TempData["CheckEmpInStatutory"] = null;
                param.Add("@p_employeeid", "0");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_FNF_EmployeeResignationlist", param).ToList();
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

        #region FNFCalculationForm
        public ActionResult FNFCalculationForm(int EmpId, string EmpName, string EmpDesignation, string EmployeeNo, string EmpDepartment, string EmpDoj, string CmpId, string BranchId, string EmpDepartmentId, string EmpDesignationId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                Session["FNFEmployeeId"] = EmpId;
                Session["FNFCmpId"] = CmpId;
                Session["FNFCalculationId"] = BranchId;
                Session["FNFEmployeeName"] = EmpName;
                Session["FNFEmpDesignation"] = EmpDesignation;
                Session["FNFEmployeeNo"] = EmployeeNo;
                Session["FNFEmpDepartment"] = EmpDepartment;
                Session["FNFEmpDoj"] = EmpDoj;
                Session["FNFEmpDepartmentId"] = EmpDepartmentId;
                Session["FNFEmpDesignationId"] = EmpDesignationId;

                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EmployeeId", EmpId);
                var SetupFlag = DapperORM.DynamicList("sp_List_Mas_Employee_StatusCheck", paramList);
                return Json(SetupFlag, JsonRequestBehavior.AllowGet);
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