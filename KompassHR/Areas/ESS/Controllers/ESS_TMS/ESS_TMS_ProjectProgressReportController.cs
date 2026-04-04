using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_TMS
{
    public class ESS_TMS_ProjectProgressReportController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        // GET: ESS/ESS_TMS_TMSReports
        public ActionResult ESS_TMS_ProjectProgressReport(DailyAttendanceReportFilter OBJTMSReport)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 606;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                var results = DapperORM.DynamicQueryMultiple(@"select  ProjectID as Id,ProjectName as Name from TMS_Project where Deactivate=0 order by ProjectName");
                ViewBag.TMSProject = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                if ((OBJTMSReport.FromDate != DateTime.MinValue) && (OBJTMSReport.ToDate != DateTime.MinValue))
                {
                    DynamicParameters param = new DynamicParameters();
                    if (OBJTMSReport.ProjectID != 0)
                    {
                        param.Add("@p_ProjectId", OBJTMSReport.ProjectID);
                        param.Add("@p_EmployeeId", OBJTMSReport.AssignToEmployeeID);
                        param.Add("@p_FromDate", OBJTMSReport.FromDate);
                        param.Add("@p_ToDate", OBJTMSReport.ToDate);
                    }
                    else
                    {
                        param.Add("@p_ProjectId", "0");
                        param.Add("@p_EmployeeId", OBJTMSReport.AssignToEmployeeID);
                        param.Add("@p_FromDate", OBJTMSReport.FromDate);
                        param.Add("@p_ToDate", OBJTMSReport.ToDate);
                    }
                    var data = DapperORM.ExecuteSP<dynamic>("sp_GetProjectProgressReports", param).ToList();
                    ViewBag.GetProjectProgressReport = data;
                }
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