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

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Module_Employee_VMSInfoController : Controller
    {
        // GET: Module/Module_Employee_VMSInfo
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        #region VMSInfo MAin View
        [HttpGet]
        public ActionResult Module_Employee_VMSInfo()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 422;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "  Add";
                TempData["OnboardEmployeeName"] = Session["OnboardEmployeeName"];
                Mas_Employee_VMS Mas_EmployeeVMS = new Mas_Employee_VMS();
                param = new DynamicParameters();
                param.Add("@p_EmployeeVMSEmployeeId", Session["OnboardEmployeeId"]);
                Mas_EmployeeVMS = DapperORM.ReturnList<Mas_Employee_VMS>("sp_List_Mas_Employee_VMS", param).FirstOrDefault();
               
                if (Mas_EmployeeVMS != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                }
                return View(Mas_EmployeeVMS);
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
        public ActionResult SaveUpdate(Mas_Employee_VMS EmployeeVMS)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(EmployeeVMS.EmployeeVMSId_Encrypted) ? "Save" : "Update");
                param.Add("@p_EmployeeVMSId", EmployeeVMS.EmployeeVMSId);
                param.Add("@p_EmployeeVMSId_Encrypted", EmployeeVMS.EmployeeVMSId_Encrypted);
                param.Add("@p_EmployeeVMSEmployeeId", Session["OnboardEmployeeId"]);
                param.Add("@p_ExtensionNo", EmployeeVMS.ExtensionNo);
                param.Add("@p_CabinNo", EmployeeVMS.CabinNo);
                param.Add("@p_FloorNo", EmployeeVMS.FloorNo);
                param.Add("@p_IsVMSMultiLocationApplicable", EmployeeVMS.IsVMSMultiLocationApplicable);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_VMS]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_VMSInfo", "Module_Employee_VMSInfo");
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