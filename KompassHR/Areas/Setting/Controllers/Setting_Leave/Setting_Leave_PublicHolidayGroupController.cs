using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_Leave;
using Dapper;
using System.Net;
using System.Data;

namespace KompassHR.Areas.Setting.Controllers.Setting_Leave
{
    public class Setting_Leave_PublicHolidayGroupController : Controller
    {
        #region Main View
        // GET: Setting/Setting_Leave_PublicHolidayGroup
        public ActionResult Setting_Leave_PublicHolidayGroup(string PublicHolidayGroupID_Encrypted)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 891;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                PublicHolidayGroup PublicHolidayGroup1 = new PublicHolidayGroup();

                DynamicParameters param = new DynamicParameters();

                //param = new DynamicParameters();
                //param.Add("@query", "SELECT LeaveTypeId as LeaveDeduction_LeaveTypeId,LeaveTypeShortName FROM Leave_Type  where Leave_Type.Deactivate = 0 and IsActive=1  order by LeaveTypeShortName");
                //var GetLeaveType = DapperORM.ReturnList<Leave_Deduction>("sp_QueryExcution", param).ToList();
                //ViewBag.GetLeaveType = GetLeaveType;

                if (PublicHolidayGroupID_Encrypted != null)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    paramList.Add("@p_PublicHolidayGroupID_Encrypted", PublicHolidayGroupID_Encrypted);
                    PublicHolidayGroup1 = DapperORM.ReturnList<PublicHolidayGroup>("sp_List_Leave_PublicHolidayGroup", paramList).FirstOrDefault();
                }

                return View(PublicHolidayGroup1);
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
        public ActionResult IsPublicHolidayGroupExists(string PublicHolidayGroupID_Encrypted, string HolidayGroup)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "IsValidation");
                param.Add("@p_PublicHolidayGroupID_Encrypted", PublicHolidayGroupID_Encrypted);
               // param.Add("@p_PublicHolidayGroupID", PublicHolidayGroupID);
                param.Add("@p_HolidayGroup", HolidayGroup);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Leave_PublicHolidayGroup", param);
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
        public ActionResult SaveUpdate(PublicHolidayGroup PublicHolidayGroup)
        {
            try
            {

                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", string.IsNullOrEmpty(PublicHolidayGroup.PublicHolidayGroupID_Encrypted) ? "Save" : "Update");
                param.Add("@p_PublicHolidayGroupID", PublicHolidayGroup.PublicHolidayGroupID);
                param.Add("@p_PublicHolidayGroupID_Encrypted", PublicHolidayGroup.PublicHolidayGroupID_Encrypted);
                param.Add("@P_HolidayGroup", PublicHolidayGroup.HolidayGroup);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Leave_PublicHolidayGroup", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Leave_PublicHolidayGroup", "Setting_Leave_PublicHolidayGroup");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region GetListView
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 891;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_PublicHolidayGroupID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Leave_PublicHolidayGroup", param);
                ViewBag.GetHolidaygroupList = data;
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
        public ActionResult Delete(string PublicHolidayGroupID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Delete");
                //param.Add("@p_LeaveDeductionID", LeaveDeductionID);
                param.Add("@p_PublicHolidayGroupID_Encrypted", PublicHolidayGroupID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_Leave_PublicHolidayGroup", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Leave_PublicHolidayGroup");
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