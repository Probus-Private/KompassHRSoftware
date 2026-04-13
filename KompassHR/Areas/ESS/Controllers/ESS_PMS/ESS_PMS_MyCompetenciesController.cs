using Dapper;
using KompassHR.Areas.ESS.Models.ESS_PMS;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_PMS
{
    public class ESS_PMS_MyCompetenciesController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: ESS/ESS_PMS_MyCompetencies
        public ActionResult ESS_PMS_MyCompetencies()
       {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 829;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var EmpId = Convert.ToInt32(Session["EmployeeId"]);
                param.Add("@p_EmployeeId", EmpId);
                param.Add("@p_CompetencyMappingId_Encrypted", "List");
                param.Add("@p_DesignationId", Session["DesignationId"]);
                var data = DapperORM.ReturnList<dynamic>("sp_List_PMS_MyCompetency", param).ToList();
                ViewBag.GetList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult CompetencySelfRating(string EmployeeCompetencyId_Encrypted,string CompetencyMappingId_Encrypted)
        {
            try
            {
                param.Add("@p_EmployeeCompetencyId_Encrypted", null);
                param.Add("@p_CompetencyMappingId_Encrypted", CompetencyMappingId_Encrypted);
                var data = DapperORM.ReturnList<dynamic>("sp_List_PMS_CompetencyDetails", param).ToList();
                ViewBag.GetList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpPost]
        public ActionResult SaveUpdate(PMS_EmployeeCompetency PMS_EmployeeCompetency)
        {
            try
            {
                var EmployeeId = Session["EmployeeId"];
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", string.IsNullOrEmpty(PMS_EmployeeCompetency.EmployeeCompetencyId_Encrypted) ? "Save" : "Update");
                param.Add("@p_EmployeeCompetencyId_Encrypted", PMS_EmployeeCompetency.EmployeeCompetencyId_Encrypted);
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_SelfRating", PMS_EmployeeCompetency.SelfRating);
                param.Add("@p_Remark", PMS_EmployeeCompetency.Remark);
                param.Add("@p_CompetencyMappingId", PMS_EmployeeCompetency.CompetencyMappingId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var data = DapperORM.ExecuteReturn("sp_SUD_PMS_EmployeeCompetency", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_PMS_MyCompetencies", "ESS_PMS_MyCompetencies");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

    }
}