using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_JobInfoController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Employee/Module_Employee_JobInfo
        #region JobInfo MAin View
        [HttpGet]
        public ActionResult Module_Employee_JobInfo()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 421;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                TempData["OnboardEmployeeName"] = Session["OnboardEmployeeName"];
                ViewBag.AddUpdateTitle = "Add";
                Mas_Employee_JobDescription mas_Jobdescription = new Mas_Employee_JobDescription();
                param = new DynamicParameters();
                param.Add("@p_JobDescriptionEmployeeId", Session["OnboardEmployeeId"]);
                mas_Jobdescription = DapperORM.ReturnList<Mas_Employee_JobDescription>("sp_List_Mas_Employee_JobDescription", param).FirstOrDefault();
                if (mas_Jobdescription != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                }
                return View(mas_Jobdescription);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region MyRegion
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveUpdate(Mas_Employee_JobDescription Module_Employee_JobInfo, string JobDescriptionRemark)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(Module_Employee_JobInfo.JobDescriptionId_Encrypted) ? "Save" : "Update");
                param.Add("@p_JobDescriptionId", Module_Employee_JobInfo.JobDescriptionId);
                param.Add("@p_JobDescriptionId_Encrypted", Module_Employee_JobInfo.JobDescriptionId_Encrypted);
                param.Add("@p_JobDescriptionEmployeeId", Session["OnboardEmployeeId"]);
                param.Add("@p_JobDescriptionRemark", JobDescriptionRemark);

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_JobDescription", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Module_Employee_JobInfo", "Module_Employee_JobInfo");
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