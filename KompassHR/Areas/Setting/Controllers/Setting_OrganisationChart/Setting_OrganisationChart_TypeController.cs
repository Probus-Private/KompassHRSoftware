using Dapper;
using KompassHR.Areas.Setting.Models.Setting_OrganisationChart;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_OrganisationChart
{
    public class Setting_OrganisationChart_TypeController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        public object BudgetName { get; private set; }
        #region OrganisationType Main View
        // GET: Setting/Setting_OrganisationChart_Type
        public ActionResult Setting_OrganisationChart_Type(string OrganisationTypeId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 542;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Organisation_Type organisation_Type = new Organisation_Type();

                if (OrganisationTypeId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_OrganisationTypeId_Encrypted", OrganisationTypeId_Encrypted);
                    organisation_Type = DapperORM.ReturnList<Organisation_Type>("sp_List_Organisation_Type", param).FirstOrDefault();
                }
                return View(organisation_Type);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
       
        #region IsValidation
        [HttpGet]
        public ActionResult IsOrganisationTypeExists(string OrganisationTypeName, string OrganisationTypeRemark, string OrganisationTypeId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_OrganisationTypeName", OrganisationTypeName);
                param.Add("@p_OrganisationTypeRemark", OrganisationTypeRemark);
                param.Add("@p_OrganisationTypeId_Encrypted", OrganisationTypeId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Organisation_Type", param);
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
        public ActionResult SaveUpdate(Organisation_Type OrganisationType)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(OrganisationType.OrganisationTypeId_Encrypted) ? "Save" : "Update");
                param.Add("@p_OrganisationTypeId_Encrypted", OrganisationType.OrganisationTypeId_Encrypted);
                param.Add("@p_OrganisationTypeId", OrganisationType.OrganisationTypeId);
                param.Add("@p_ShowForEmployee", OrganisationType.ShowForEmployee);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_OrganisationTypeName", OrganisationType.OrganisationTypeName);
                param.Add("@p_OrganisationTypeRemark", OrganisationType.OrganisationTypeRemark);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Organisation_Type", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_OrganisationChart_Type", "Setting_OrganisationChart_Type");

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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 542;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_OrganisationTypeId_Encrypted", "List");
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ReturnList<Organisation_Type>("sp_List_Organisation_Type", param).ToList();
                ViewBag.GetOrganisationTypeList = data;
                return View();
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
        public ActionResult Delete(string OrganisationTypeId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_OrganisationTypeId_Encrypted", OrganisationTypeId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString()); 
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Organisation_Type", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_OrganisationChart_Type", new { Area = "Setting" });
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