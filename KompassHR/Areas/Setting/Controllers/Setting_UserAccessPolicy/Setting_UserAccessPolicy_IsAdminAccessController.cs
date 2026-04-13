using Dapper;
using KompassHR.Areas.Setting.Models.Setting_UserAccessPolicy;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_UserAccessPolicy
{
    public class Setting_UserAccessPolicy_IsAdminAccessController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        // GET: Setting/Setting_UserAccessPolicy_IsAdminAccess
        #region Main View
        public ActionResult Setting_UserAccessPolicy_IsAdminAccess(string ESSId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 448;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                IsAdminAccess IsAdminAccess = new IsAdminAccess();
                if (ESSId_Encrypted != null)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@query", $@"select IsAdmin,EmployeeId,ESSId_Encrypted,EmployeeName from Mas_Employee_ESS
                                inner Join Mas_Employee on Mas_Employee.EmployeeId = Mas_Employee_ESS.ESSEmployeeId 
                                where Mas_Employee.Deactivate=0 And IsAdmin=1 And ESSId_Encrypted='{ESSId_Encrypted}'");
                    IsAdminAccess = DapperORM.ExecuteSP<IsAdminAccess>("sp_QueryExcution", param1).FirstOrDefault();
                }
                param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and ContractorId=1 and EmployeeLeft=0 order by Name");
                var EmpolyeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetEmpolyeeName = EmpolyeeName;
                return View(IsAdminAccess);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(IsAdminAccess Obj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int isAdminValue = Obj.IsAdmin ? 1 : 0;
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", $@"UPDATE Mas_Employee_ESS SET IsAdmin = {isAdminValue} WHERE ESSEmployeeId = {Obj.EmployeeId}");
                DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).FirstOrDefault();
                TempData["Message"] = "Record save successfully";
                TempData["Icon"] = "success";
                return RedirectToAction("Setting_UserAccessPolicy_IsAdminAccess", "Setting_UserAccessPolicy_IsAdminAccess");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 448;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", $@"  Select ESSId_Encrypted,EmployeeId,Concat(EmployeeName,' - ',EmployeeNo) as EmployeeName,Mas_designation.DesignationName,Mas_Department.DepartmentName,Mas_Branch.BranchName ,case when IsAdmin=1 then 'Yes' else 'No' end as IsAdmin
                                         from Mas_Employee_ess
                                         left join Mas_Employee on Mas_Employee.EmployeeId=Mas_Employee_ess.ESSEmployeeId
                                         left join  Mas_designation on Mas_designation.DesignationId=Mas_Employee.EmployeeDesignationID
                                         left join  Mas_Department on Mas_Department.DepartmentId=Mas_Employee.EmployeeDepartmentID
                                         left join  Mas_Branch on Mas_Branch.BranchId=Mas_Employee.EmployeeBranchId
                                         where   IsAdmin=1 and Mas_Employee_ess.Deactivate=0");
                var data = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).ToList();
                ViewBag.GetIsAdminAccess = data;
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