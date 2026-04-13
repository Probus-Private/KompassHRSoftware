using Dapper;
using KompassHR.Areas.Setting.Models.Setting_TMS;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_TMS
{
    public class Setting_TMS_TaskCategory_ProjectModuleMappingController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: TMSSetting/Setting_TMS_TaskCategory_ProjectModuleMapping
        public ActionResult Setting_TMS_TaskCategory_ProjectModuleMapping(string TaskCategoryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 880;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "SELECT TaskCategoryId as Id, TaskCategoryName as Name FROM TMS_TaskCategory WHERE Deactivate = 0 ORDER BY Name");
                var TaskCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();
                ViewBag.GetTaskCategory = TaskCategory;

                if (TaskCategoryId != null)
                {
                    param.Add("@p_TaskCategoryId", TaskCategoryId);
                    var GetProjectModule = DapperORM.ReturnList<TaskCategory_ProjectModuleMapping>("sp_List_TMS_ProjectModule", param).ToList();
                    ViewBag.GetProjectModuleList = GetProjectModule;
                }
                else
                {
                    ViewBag.GetProjectModuleList = "";
                }
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region SaveUpadte
        [HttpPost]
        public ActionResult SaveUpadte(List<TaskCategory_ProjectModuleMapping> TaskCat_ProjectModuleMap, string TaskCategoryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 880;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                if (TaskCat_ProjectModuleMap == null)
                {
                    param.Add("@p_process", "Delete");
                    param.Add("@p_TaskCategoryId", TaskCategoryId);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_TMS_SUD_TaskCat_ProjectModuleMapping", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }
                if (TaskCat_ProjectModuleMap.Count > 0)
                {
                    for (var i = 0; i < TaskCat_ProjectModuleMap.Count; i++)
                    {
                        param.Add("@p_process", "Save");
                        param.Add("@p_TaskCategoryId", TaskCategoryId);
                        param.Add("@p_IsActive", TaskCat_ProjectModuleMap[i].IsActive);
                        param.Add("@p_ProjectId", TaskCat_ProjectModuleMap[i].ProjectId);
                        param.Add("@p_ModuleId", TaskCat_ProjectModuleMap[i].ModuleId);
                        param.Add("@p_MachineName", Dns.GetHostName().ToString());
                        param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        var data = DapperORM.ExecuteReturn("sp_TMS_SUD_TaskCat_ProjectModuleMapping", param);
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