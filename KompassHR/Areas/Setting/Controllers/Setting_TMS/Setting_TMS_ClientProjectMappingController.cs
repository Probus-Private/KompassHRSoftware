using Dapper;
using KompassHR.Areas.Setting.Models.Setting_TMS;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_TMS
{
    public class Setting_TMS_ClientProjectMappingController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        // GET: Setting/ Setting_TMS_ClientProjectMapping
        #region Main View
        public ActionResult Setting_TMS_ClientProjectMapping(int? ClientId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                // Check access permissions
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 670;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";

                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "SELECT ClientId as Id, ClientName as Name FROM TMS_Client WHERE Deactivate = 0 ORDER BY Name");
                var Client = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetClient = Client;


                if (ClientId != null)
                {
                    param.Add("@query", $@"Select ClientProjectMappingId,TMS_Project.ProjectID ,IsNull(TMS_ClientProjectMapping.IsActive,0)IsActive ,TMS_Project.ProjectName 
                          from TMS_Project Left outer Join TMS_ClientProjectMapping on TMS_ClientProjectMapping.ProjectId=TMS_Project.ProjectID and TMS_ClientProjectMapping.ClientId={ClientId}
                          where TMS_Project.Deactivate=0 order by ProjectName ");
                    var data = DapperORM.ReturnList<ProjectMapping>("sp_QueryExcution", param).ToList();
                    ViewBag.GetProjectList = data;
                }
                else
                {
                    ViewBag.GetProjectList = "";
                }
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion



        #region SaveUpadte
        [HttpPost]
        public ActionResult SaveUpadte(List<ProjectMapping> clientprojectmap, string ClientId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 670;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                if (clientprojectmap == null)
                {
                    param.Add("@p_process", "Delete");
                    param.Add("@p_ClientId", ClientId);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_SUD_ClientProjectMapping", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }
                if (clientprojectmap.Count > 0)
                {
                    for (var i = 0; i < clientprojectmap.Count; i++)
                    {
                        param.Add("@p_process", "Save");
                        param.Add("@p_ClientId", clientprojectmap[i].ClientId);
                        param.Add("@p_IsActive", clientprojectmap[i].IsActive);
                        param.Add("@p_ProjectId", clientprojectmap[i].ProjectID);
                        param.Add("@p_MachineName", Dns.GetHostName().ToString());
                        param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        var data = DapperORM.ExecuteReturn("sp_SUD_ClientProjectMapping", param);
                        TempData["Message"] = param.Get<string>("@p_msg");
                        TempData["Icon"] = param.Get<string>("@p_Icon");

                    }
                }
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
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