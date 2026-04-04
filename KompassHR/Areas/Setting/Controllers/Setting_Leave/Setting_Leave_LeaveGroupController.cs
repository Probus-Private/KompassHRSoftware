using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_Leave;
using KompassHR.Models;
using System.Data;
using System.Net;
using System.Data.SqlClient;

namespace KompassHR.Areas.Setting.Controllers.Setting_Leave
{
    public class Setting_Leave_LeaveGroupController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: LeaveSetting/LeaveGroup
        #region LeaveGroup MAin View
        [HttpGet]
        public ActionResult Setting_Leave_LeaveGroup(string LeaveGroupID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 47;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Leave_Group leave_leavegroup = new Leave_Group();

                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@query", "Select CompanyId As Id,CompanyName As Name from Mas_CompanyProfile Where Deactivate=0");
                var listMas_CompanyProfile = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramList).ToList();
                ViewBag.GetCompanyname = listMas_CompanyProfile;
                if (LeaveGroupID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@P_LeaveGroupID_Encrypted", LeaveGroupID_Encrypted);
                    leave_leavegroup = DapperORM.ReturnList<Leave_Group>("sp_List_Leave_Group", param).FirstOrDefault();
                }
                return View(leave_leavegroup);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsValidation
        public ActionResult IsLeaveGroupExists(string LeaveGroupID_Encrypted, string LeaveGroupName,double CmpId)
        {
            try
            {
                    if (Session["EmployeeId"] == null)
                    {
                        return RedirectToAction("Login", "Login", new { Area = "" });
                    }
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_LeaveGroupID_Encrypted", LeaveGroupID_Encrypted);
                    param.Add("@p_CmpId", CmpId);
                    param.Add("@p_LeaveGroupName", LeaveGroupName);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Leave_Group", param);
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
        public ActionResult SaveUpdate(Leave_Group LeaveGroup)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(LeaveGroup.LeaveGroupID_Encrypted) ? "Save" : "Update");
                param.Add("@p_LeaveGroupID", LeaveGroup.LeaveGroupID);
                param.Add("@p_LeaveGroupID_Encrypted", LeaveGroup.LeaveGroupID_Encrypted);
                param.Add("@p_CmpId", LeaveGroup.CmpId);
                param.Add("@p_LeaveGroupName", LeaveGroup.LeaveGroupName);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Leave_Group", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Leave_LeaveGroup", "Setting_Leave_LeaveGroup");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 47;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_LeaveGroupID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Leave_Group", param);
                ViewBag.GetLeaveGroupList = data;
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
        public ActionResult Delete(string LeaveGroupId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_LeaveGroupId_Encrypted", LeaveGroupId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_Leave_Group", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Leave_LeaveGroup");
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