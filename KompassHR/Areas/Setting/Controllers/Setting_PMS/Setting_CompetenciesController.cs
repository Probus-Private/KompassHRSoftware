using Dapper;
using KompassHR.Areas.Setting.Models.Setting_PMS;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_PMS
{
    public class Setting_CompetenciesController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: Setting/Setting_Competencies
        public ActionResult Setting_Competencies(string CompetencyId_Encrypted, int? FrameworkId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 820;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                // FIRST QUERY (bind all frameworks)
                param = new DynamicParameters();
                param.Add("@query", "select CompetencyFrameworkId as Id, CompentencyType as Name  from PMS_CompetencyFramework where deactivate = 0");
                var GetType = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetType = GetType;


                // ONLY RUN SECOND QUERY IF FrameworkId is NOT null AND NOT 0
                if (FrameworkId != null && FrameworkId != 0)
                {
                    param = new DynamicParameters();   // VERY IMPORTANT
                    param.Add("@query", "select CompetencyFrameworkId as Id, CompentencyType as Name  from PMS_CompetencyFramework where deactivate = 0 and CompetencyFrameworkId = '" + FrameworkId + "'");               
                    GetType = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    ViewBag.GetType = GetType;

                    ViewBag.SelectedFrameworkId = FrameworkId;
                }


                ViewBag.AddUpdateTitle = "Add";
                PMS_Competencies Kknag_Competencies = new PMS_Competencies();
                if (CompetencyId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_CompetencyId_Encrypted", CompetencyId_Encrypted);
                    Kknag_Competencies = DapperORM.ReturnList<PMS_Competencies>("sp_List_PMS_Competencies", param).FirstOrDefault();
                }
                return View(Kknag_Competencies);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult IsValidation(int CompetencyFrameworkId, string CompetencyId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "IsValidation");
                param.Add("@p_CompetencyId_Encrypted", CompetencyId_Encrypted);
                param.Add("@p_CompetencyFrameworkId", CompetencyFrameworkId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_PMS_Competencies", param);
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
            catch(Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(PMS_Competencies PMS_Competencies)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                param.Add("@p_process", string.IsNullOrEmpty(PMS_Competencies.CompetencyId_Encrypted) ? "Save" : "Update");
                param.Add("@p_CompetencyFrameworkId", PMS_Competencies.CompetencyFrameworkId);
                param.Add("@p_Competency", PMS_Competencies.CompetencyName);
                param.Add("@p_CompetencyId_Encrypted", PMS_Competencies.CompetencyId_Encrypted);
                param.Add("@p_Description", PMS_Competencies.Description);
                //param.Add("@p_Weightage", PMS_Competencies.Weightage);

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_PMS_Competencies", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Setting_Competencies", "Setting_Competencies");
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
                    if (Session["EmployeeId"] == null)
                    {
                        return RedirectToAction("Login", "Login", new { Area = "" });
                    }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 820;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_CompetencyId_Encrypted", "List");
                var data = DapperORM.ReturnList<dynamic>("sp_List_PMS_Competencies", param).ToList();
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
        public ActionResult Delete(string CompetencyId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                param.Add("@p_process", "Delete");
                param.Add("@p_CompetencyId_Encrypted", CompetencyId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);

                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_PMS_Competencies", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Competencies");
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