using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.ESS.Models.ESS_PMS;
using System.Net;
using System.Data;

namespace KompassHR.Areas.ESS.Controllers.ESS_PMS
{
    public class ESS_PMS_CompetencyMappingController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: ESS/ESS_PMS_CompentencyMapping
        [HttpGet]
        public ActionResult ESS_PMS_CompetencyMapping(string CompetencyMappingId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 828;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                PMS_CompentencyMapping PMS_CompentencyMapping = new PMS_CompentencyMapping();

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "select  DesignationId as Id,DesignationName AS Name from Mas_Designation where Deactivate=0");
                var DesignationList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();
                ViewBag.DesignationList = DesignationList;

                var query = @"SELECT Fid FROM PMS_Year WHERE IsActive=1";
                var YearId = DapperORM.DynamicQuerySingle(query);
                ViewBag.Year = YearId.Fid;

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@query", "select PMS_Competencies.CompetencyId as Id,PMS_Competencies.Competency as Name from PMS_Competencies PMS_Competencies left join PMS_CompetencyFramework  PMS_CompetencyFramework on  PMS_Competencies.CompetencyFrameworkId = PMS_CompetencyFramework.CompetencyFrameworkId where PMS_CompetencyFramework.Category != 'Common To All'");
                var CompetencyList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();
                ViewBag.CompetencyList = CompetencyList;


                if (CompetencyMappingId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_CompetencyMappingId_Encrypted", CompetencyMappingId_Encrypted);
                    PMS_CompentencyMapping = DapperORM.ReturnList<PMS_CompentencyMapping>("sp_List_PMS_Competency_Mapping", param).FirstOrDefault();
               
                }
                return View(PMS_CompentencyMapping);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult IsValidation(int DesignationId, int CompetencyId,string CompetencyMappingId_Encrypted,int Pms_YearId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "IsValidation");
                param.Add("@p_CompetencyMappingId_Encrypted", CompetencyMappingId_Encrypted);
                param.Add("@p_DesignationID", DesignationId);
                param.Add("@p_CompetencyID ", CompetencyId);
                param.Add("@p_PMS_YearId", Pms_YearId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Competency_Mapping", param);
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

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(PMS_CompentencyMapping PMS_CompentencyMapping)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(PMS_CompentencyMapping.CompetencyMappingId_Encrypted) ? "Save" : "Update");
                param.Add("@p_DesignationID", PMS_CompentencyMapping.DesignationID);
                param.Add("@p_CompetencyID", PMS_CompentencyMapping.CompetencyID);
                param.Add("@p_PMS_YearId", PMS_CompentencyMapping.PMS_YearId);
                //param.Add("@p_Weightage", PMS_CompentencyMapping.Weightage);
                param.Add("@p_Description", PMS_CompentencyMapping.Description);
                param.Add("@p_CompetencyMappingId_Encrypted", PMS_CompentencyMapping.CompetencyMappingId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Competency_Mapping", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("ESS_PMS_CompetencyMapping", "ESS_PMS_CompetencyMapping");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetList
        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 819;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_CompetencyMappingId_Encrypted", "List");
                var data = DapperORM.ReturnList<dynamic>("sp_List_PMS_Competency_Mapping", param).ToList();
                ViewBag.GetList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region  Delete
        public ActionResult Delete(string CompetencyMappingId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                param.Add("@p_process", "Delete");
                param.Add("@p_CompetencyMappingId_Encrypted", CompetencyMappingId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Competency_Mapping", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_PMS_CompetencyMapping");
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