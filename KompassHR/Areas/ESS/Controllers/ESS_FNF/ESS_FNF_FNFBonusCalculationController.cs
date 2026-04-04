using Dapper;
using KompassHR.Areas.ESS.Models.ESS_FNF;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_FNF
{
    public class ESS_FNF_FNFBonusCalculationController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        // GET: ESS/ESS_TMS_TMSReports
        public ActionResult ESS_FNF_FNFBonusCalculation(FNF_Calculation FNF_Calculation)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 597;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                //var results = DapperORM.DynamicQuerySingleMultiple(@"select  ProjectID as Id,ProjectName as Name from TMS_Project where Deactivate=0 order by ProjectName;
                //                                  Select TeamEmployeeId as Id,(Mas_Employee.EmployeeName + ' - ' + CAST(Mas_Employee.EmployeeNo AS VARCHAR)) AS Name from TMS_TeamAssign,Mas_Employee where TeamManagerId=" + Session["EmployeeId"] + " and IsActive=1 and Mas_Employee.EmployeeId=TMS_TeamAssign.TeamEmployeeId and Mas_Employee.Deactivate=0 order by Name");

                //ViewBag.TMSProject = results.Read<AllDropDownClass>().ToList();
                //ViewBag.GetEmployeeName = results.Read<AllDropDownClass>().ToList();
                //if ((OBJTMSReport.ProjectID > 0))
                //{
                //    DynamicParameters param = new DynamicParameters();
                //    param.Add("@p_ProjectId", OBJTMSReport.ProjectID);
                //    param.Add("@p_EmployeeId", OBJTMSReport.AssignToEmployeeID);
                //    param.Add("@p_FromDate", OBJTMSReport.FromDate);
                //    param.Add("@p_ToDate", OBJTMSReport.ToDate);
                //    var data = DapperORM.ExecuteSP<dynamic>("sp_GetTaskStatusReports", param).ToList();
                //    ViewBag.GetTaskStatusReport = data;
                //}
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
    }
}