using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_Leave;
using System.Net;
using Dapper;
using KompassHR.Models;
using System.Data;
using System.Data.SqlClient;

namespace KompassHR.Areas.Setting.Controllers.Setting_Leave
{
    public class Setting_Leave_LeaveTypeController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: LeaveSetting/LeaveType
        #region LeaveType Main View
        [HttpGet]
        public ActionResult Setting_Leave_LeaveType(string LeaveTypeID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 45;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                leave_type leave_leavetype = new leave_type();
                ViewBag.AddUpdateTitle = "Add";
                if (LeaveTypeID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_LeaveTypeID_Encrypted", LeaveTypeID_Encrypted);
                    leave_leavetype = DapperORM.ReturnList<leave_type>("sp_List_Leave_Type", param).FirstOrDefault();
                }
                return View(leave_leavetype);
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
        public ActionResult IsLeaveTypeExists(string LeaveTypeName, string LeaveTypeId_Encrypted, int LeaveTypeId, string LeaveTypeShortName)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_LeaveTypeID_Encrypted", LeaveTypeId_Encrypted);
                    param.Add("@p_LeaveTypeID", LeaveTypeId);
                    param.Add("@P_LeaveTypeName ", LeaveTypeName);
                    param.Add("@P_LeaveTypeShortName", LeaveTypeShortName);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Leave_Type", param);
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
        public ActionResult SaveUpdate(leave_type LeaveType)
        {
            try
            {

                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(LeaveType.LeaveTypeId_Encrypted) ? "Save" : "Update");
                param.Add("@p_LeaveTypeID", LeaveType.LeaveTypeId);
                param.Add("@p_LeaveTypeID_Encrypted", LeaveType.LeaveTypeId_Encrypted);
                param.Add("@P_LeaveTypeName ", LeaveType.LeaveTypeName);
                param.Add("@p_LeaveTypeShortName", LeaveType.LeaveTypeShortName);
                param.Add("@p_IsActive ", LeaveType.IsActive);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Leave_Type", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Leave_LeaveType", "Setting_Leave_LeaveType");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 45;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_LeaveTypeID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Leave_Type", param);
                ViewBag.GetLeaveTypeList = data;
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
        public ActionResult Delete(string LeaveTypeId_Encrypted, int LeaveTypeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_LeaveTypeID", LeaveTypeId);
                param.Add("@p_LeaveTypeId_Encrypted", LeaveTypeId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_Leave_Type", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Leave_LeaveType");
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