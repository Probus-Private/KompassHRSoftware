using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Leave;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_AccountAndFinance;
using System.Net;
using System.Data;
using System.Data.SqlClient;

namespace KompassHR.Areas.Setting.Controllers.Setting_Leave
{
    public class Setting_Leave_LeaveEncashmentController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: LeaveSetting/Setting_Leave_LeaveEncashment
        #region Setting_Leave_LeaveEncashment
        public ActionResult Setting_Leave_LeaveEncashment(string LeaveEncashSettingId_Encrypted, Leave_EncashmentSetting obj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 49;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Leave_EncashmentSetting Leave_Encashment = new Leave_EncashmentSetting();


                param.Add("@query", "Select CompanyId,CompanyName from Mas_CompanyProfile Where Deactivate=0");
                var listMas_CompanyProfile = DapperORM.ReturnList<Mas_CompanyProfile>("sp_QueryExcution", param).ToList();
                ViewBag.GetCompanyname = listMas_CompanyProfile;

                //param.Add("@query", "Select [LeaveTypeId],LeaveSettingLeaveGroupId,Leave_Group.LeaveGroupName+'-'+Leave_Type.LeaveTypeName  as LeaveTypeName from Leave_Setting,Leave_Type,Leave_Group "
                //   + "where Leave_Setting.Deactivate = 0 and Leave_Type.Deactivate = 0 and Leave_Group.Deactivate = 0 "
                //   + "and Leave_Setting.LeaveSettingLeaveGroupId = Leave_Group.LeaveGroupId "
                //   + "and Leave_Setting.LeaveSettingLeaveTypeId = Leave_Type.LeaveTypeId "
                //   + "and[dbo].[Leave_Setting].[IsEncashment] = 1 "
                //   + "order by Leave_Type.LeaveTypeName");
                //List<DropdownLeaveType> DropdownLeaveType = new List<DropdownLeaveType>();
                //DropdownLeaveType = DapperORM.ReturnList<DropdownLeaveType>("sp_QueryExcution", param).ToList();
                //ViewBag.GetLeaveType = DropdownLeaveType;

                DynamicParameters param2 = new DynamicParameters();
                //param2.Add("@query", "select Leave_Setting.LeaveSettingId as Id ,concat(Leave_Group.LeaveGroupName, ' - ', Leave_Type.LeaveTypeShortName) as Name from Leave_Setting, Leave_Group, Leave_Type where Leave_Setting.Deactivate = 0 and Leave_Group.Deactivate = 0 and Leave_Type.Deactivate = 0 and Leave_Setting.LeaveSettingLeaveGroupId = Leave_Group.LeaveGroupId and Leave_Type.LeaveTypeId = Leave_Setting.LeaveSettingLeaveTypeId");
                param2.Add("@query", $@"SELECT 
                                        MIN(Leave_Setting.LeaveSettingLeaveTypeId) AS Id,
                                        CONCAT(Leave_Group.LeaveGroupName, ' - ', Leave_Type.LeaveTypeShortName) AS Name
                                    FROM 
                                        Leave_Setting
                                    JOIN 
                                        Leave_Group ON Leave_Setting.LeaveSettingLeaveGroupId = Leave_Group.LeaveGroupId
                                    JOIN 
                                        Leave_Type ON Leave_Setting.LeaveSettingLeaveTypeId = Leave_Type.LeaveTypeId
                                    WHERE 
                                        Leave_Setting.Deactivate = 0 AND
                                        Leave_Group.Deactivate = 0 AND
                                        Leave_Type.Deactivate = 0
                                    GROUP BY 
                                        Leave_Group.LeaveGroupName,
                                        Leave_Type.LeaveTypeShortName;");
                var DropdownLeaveType = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();
                ViewBag.GetLeaveType = DropdownLeaveType;


                if (LeaveEncashSettingId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_LeaveEncashSettingId_Encrypted", LeaveEncashSettingId_Encrypted);
                    Leave_Encashment = DapperORM.ReturnList<Leave_EncashmentSetting>("sp_List_Leave_EncashmentSetting", param).FirstOrDefault();
                }
                ViewBag.Leave_Encashment = Leave_Encashment;
                return View(Leave_Encashment);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsLeaveEncashmentExists
        public ActionResult IsLeaveEncashmentExists(string LeaveEncashSettingId_Encrypted, string LeaveEncashSettingEarningId, string LeaveType)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {

                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_LeaveEncashSettingId_Encrypted", LeaveEncashSettingId_Encrypted);
                    param.Add("@p_LeaveEncashSettingLeaveTypeId", LeaveType);
                    //  param.Add("@p_LeaveEncashSettingtLeaveSettingId", LeaveEncashSettingEarningId);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Leave_EncashmentSetting", param);
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
        public ActionResult SaveUpdate(Leave_EncashmentSetting Leave_Encashment)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                string LeaveEncashSettingEarningId = Request["LeaveEncashSettingEarningId"];
                param.Add("@p_process", string.IsNullOrEmpty(Leave_Encashment.LeaveEncashSettingId_Encrypted) ? "Save" : "Update");
                param.Add("@p_LeaveEncashSettingId_Encrypted", Leave_Encashment.LeaveEncashSettingId_Encrypted);
                param.Add("@p_LeaveEncashSettingtLeaveSettingId", Leave_Encashment.LeaveEncashSettingtLeaveSettingId);
                param.Add("@p_CmpId", Leave_Encashment.CmpId);
                param.Add("@p_LeaveEncashSettingEarningId", LeaveEncashSettingEarningId);
                param.Add("@p_LeaveEncashSettingLeaveTypeId", Leave_Encashment.LeaveEncashSettingLeaveTypeId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Leave_EncashmentSetting", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Setting_Leave_LeaveEncashment", "Setting_Leave_LeaveEncashment");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 49;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_LeaveEncashSettingId_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Leave_EncashmentSetting", param);
                ViewBag.GeLeaveEncashmentList = data;
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
        public ActionResult Delete(string LeaveEncashSettingId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_LeaveEncashSettingId_Encrypted", LeaveEncashSettingId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_Leave_EncashmentSetting", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Leave_LeaveEncashment", new { Area = "Setting" });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetLeaveType(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@query", "select Leave_Setting.LeaveSettingId as Id ,concat(Leave_Group.LeaveGroupName, ' - ', Leave_Type.LeaveTypeShortName) as Name from Leave_Setting, Leave_Group, Leave_Type where Leave_Setting.Deactivate = 0 and Leave_Group.Deactivate = 0 and Leave_Type.Deactivate = 0 and Leave_Setting.LeaveSettingLeaveGroupId = Leave_Group.LeaveGroupId and Leave_Type.LeaveTypeId = Leave_Setting.LeaveSettingLeaveTypeId and Leave_Setting.CmpId=" + CmpId + "");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();
                return Json(new { data = data }, JsonRequestBehavior.AllowGet);
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