using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Leave;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Leave
{
    public class ESS_Leave_PlanningController : Controller
    {
        #region ESS_Leave_Planning Main View 
        // GET: ESS/ESS_Leave_Planning
        [HttpGet]
        public ActionResult ESS_Leave_Planning(string LeavePlanningId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 776;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
           
                ViewBag.AddUpdateTitle = "Add";
                Leave_Planning Leave = new Leave_Planning();
                DynamicParameters param = new DynamicParameters();

                if (!string.IsNullOrEmpty(LeavePlanningId_Encrypted))
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_LeavePlanningId_Encrypted", LeavePlanningId_Encrypted);
                    Leave = DapperORM.ReturnList<Leave_Planning>("sp_List_Leave_Planning", param).FirstOrDefault();
                    TempData["FromDate"] = Leave.FromDate.HasValue
                       ? Leave.FromDate.Value.ToString("yyyy-MM-dd")
                     : string.Empty;

                    TempData["ToDate"] = Leave.ToDate.HasValue
                     ? Leave.ToDate.Value.ToString("yyyy-MM-dd")
                   : string.Empty;
                }
                return View(Leave);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsLeavePlanningExists
        public ActionResult IsLeavePlanningExists(string LeavePlanningId_Encrypted, int LeavePlanningId,DateTime FromDate,DateTime ToDate)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_FromDate", FromDate);
                    param.Add("@p_ToDate", ToDate);
                    param.Add("@p_LeavePlanningId", LeavePlanningId);
                    param.Add("@p_LeavePlanningId_Encrypted", LeavePlanningId_Encrypted);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_EmployeeId", Session["EmployeeId"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                    var Result = DapperORM.ExecuteReturn("sp_SUD_Leave_Planning", param);
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
        public ActionResult SaveUpdate(Leave_Planning LeavePlanning)
        {
            try

            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", string.IsNullOrEmpty(LeavePlanning.LeavePlanningId_Encrypted) ? "Save" : "Update");
                param.Add("@p_LeavePlanningId_Encrypted", LeavePlanning.LeavePlanningId_Encrypted);
                param.Add("@p_FromDate", LeavePlanning.FromDate);
                param.Add("@p_ToDate", LeavePlanning.ToDate);
                param.Add("@p_Reason", LeavePlanning.Reason);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                var Result = DapperORM.ExecuteReturn("sp_SUD_Leave_Planning", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");

                var P_Id = param.Get<string>("@p_Id");

                return RedirectToAction("GetList", "ESS_Leave_Planning");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList Main View 
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 776;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "ESS" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_LeavePlanningId_Encrypted", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Leave_Planning", param).ToList();
                ViewBag.GetPlanningList = data;
            
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
        public ActionResult DeletePlanning(int? LeavePlanningId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "ESS" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Delete");
                param.Add("@p_LeavePlanningId", LeavePlanningId);
                param.Add("@p_CreatedupdateBy", Session["EmployeeName"].ToString());
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                var Result = DapperORM.ExecuteReturn("sp_SUD_Leave_Planning", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                // TempData["P_Id"] = param.Get<string>("@p_Id");
                return RedirectToAction("GetList", "ESS_Leave_Planning");
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