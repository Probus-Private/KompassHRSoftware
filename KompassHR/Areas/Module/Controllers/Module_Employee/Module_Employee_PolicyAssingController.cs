using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Module_Employee_PolicyAssingController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Module/Module_Employee_PolicyAssing
        public ActionResult Module_Employee_PolicyAssing()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 512;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@query", "select PolicyGroupMasterId as Id , GroupName as Name from Mas_PolicyGroup_Master where Deactivate=0");
                var GroupName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetGroupName = GroupName;

                param = new DynamicParameters();
                param.Add("@p_EmployeePolicyEmployeeID", Session["OnboardEmployeeId"]);
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Mas_Employee_PolicyAssign", param).ToList();
                ViewBag.GetPolicyAssignList = data;
                
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region IsValidation
        [HttpGet]
        public ActionResult IsPolicyExists(string PolicyId_Encrypted)
        {
            try
            {

                param.Add("@p_process", "IsValidation");
                param.Add("@p_AssetId_Encrypted", PolicyId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_Asset", param);
                var Message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                if (Message != "")
                {
                    return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }

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
        public ActionResult SaveUpdate(Mas_Employee_PolicyAsign PolicyAssign)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var OnboardEmployeeId = Session["OnboardEmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(PolicyAssign.EmployeePolicyId_Encrypted) ? "Save" : "Update");
                param.Add("@p_EmployeePolicyId_Encrypted", PolicyAssign.EmployeePolicyId_Encrypted);
                param.Add("@p_EmployeePolicyEmployeeID", OnboardEmployeeId);
                param.Add("@p_PolicyId", PolicyAssign.PolicyId);
               

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_PolictAssign]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_PolicyAssing", "Module_Employee_PolicyAssing");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        [HttpGet]
        public ActionResult Delete(string EmployeePolicyId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_EmployeePolicyId_Encrypted", EmployeePolicyId_Encrypted);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parame
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_PolictAssign", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_PolicyAssing", "Module_Employee_PolicyAssing");
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