using Dapper;
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
    public class Module_Employee_GeneralReportingController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Module/Module_Employee_GeneralReporting

        #region GeneralReporting MAin View
            [HttpGet]
        public ActionResult Module_Employee_GeneralReporting()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Mas_Employee MasEmployee = new Mas_Employee();
                //param.Add("@query", "Select EmployeeId As Id,EmployeeName As Name From Mas_Employee where EmployeeName is not null and  Mas_Employee.Deactivate=0 and CmpId=" + Session["OnboardCmpId"] + " and EmployeeBranchId=" + Session["OnboardBranchId"] + " and EmployeeLeft=0 order by EmployeeName");
                param.Add("@query", "Select EmployeeId As Id,EmployeeName As Name From Mas_Employee where EmployeeName is not null and  Mas_Employee.Deactivate=0 and EmployeeLeft=0 order by EmployeeName");
                var ManagerList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.ManagerLists = ManagerList;

                param = new DynamicParameters();
                param.Add("@p_EmployeeId", Session["OnboardEmployeeId"]);
                MasEmployee = DapperORM.ReturnList<Mas_Employee>("sp_List_Mas_Employee_GeneralReoprting", param).FirstOrDefault();
                if (MasEmployee != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                }
                return View(MasEmployee);
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
        public ActionResult SaveUpdate(Mas_Employee MasEmployee)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var OnboardEmployeeId = Session["OnboardEmployeeId"];
                param.Add("@p_process", "Save");
                param.Add("@p_EmployeeId", OnboardEmployeeId);
                param.Add("@p_ReportingManager1", MasEmployee.ReportingManager1);
                param.Add("@p_ReportingManager2", MasEmployee.ReportingManager2);
                param.Add("@p_ReportingHR", MasEmployee.ReportingHR);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_GeneralReporting]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_GeneralReporting", "Module_Employee_GeneralReporting");
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