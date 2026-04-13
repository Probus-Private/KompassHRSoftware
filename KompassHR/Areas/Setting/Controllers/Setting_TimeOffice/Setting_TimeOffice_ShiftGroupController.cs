using Dapper;
using KompassHR.Areas.Setting.Models.Setting_TimeOffice;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_TimeOffice
{
    public class Setting_TimeOffice_ShiftGroupController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        #region ShiftGroup Main View
        [HttpGet]
        public ActionResult Setting_TimeOffice_ShiftGroup(string ShiftGroupId_Encrypted, int? ShiftGroupId, int? CompanyId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 53;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                Atten_ShiftGroups Atten_ShiftGroups = new Atten_ShiftGroups();
                param.Add("@query", "Select  CompanyId As Id, CompanyName As Name from Mas_CompanyProfile where Deactivate = 0 order by Name");
                var GetCompanyName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.CompanyName = GetCompanyName;

                ViewBag.AddUpdateTitle = "Add";
                param = new DynamicParameters();
                if (CompanyId != null)
                {
                    DynamicParameters ParamBranch = new DynamicParameters();
                    ParamBranch.Add("@p_employeeid", Session["EmployeeId"]);
                    ParamBranch.Add("@p_CmpId", CompanyId);
                    ViewBag.Location = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", ParamBranch).ToList();
                }
                else
                {
                    ViewBag.Location = "";
                }
                if (ShiftGroupId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    DynamicParameters ShiftGroup = new DynamicParameters();
                    ShiftGroup.Add("@p_ShiftGroupId_Encrypted", ShiftGroupId_Encrypted);
                    Atten_ShiftGroups = DapperORM.ReturnList<Atten_ShiftGroups>("sp_List_Atten_ShiftGroups", ShiftGroup).FirstOrDefault();
                    var ShiftGroupBranchId = Atten_ShiftGroups.ShiftGroupBranchId;

                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@query", @" select ShiftId,
                                             case when Atten_ShiftGroupShifts_ShiftId is null then 'unchecked' else 'checked' end as checkbox,
                                             Atten_Shifts.ShiftName+' ( '+SName+' ) '+' ( '+format(CONVERT(datetime, Atten_Shifts.BeginTime),'HH:mm')+
                                             +'-'+  format(CONVERT(datetime, Atten_Shifts.EndTime), 'HH:mm') +' )'	as ShiftName,case when ShiftFlag=1 then 'Day & Night' else 'Day' end as ShiftFlag,ShiftFlag as DayNight from Atten_Shifts 
                                            left join Atten_ShiftGroupShifts on Atten_Shifts.ShiftId=Atten_ShiftGroupShifts.Atten_ShiftGroupShifts_ShiftId 
                                            and Atten_ShiftGroupShifts.Atten_ShiftGroupShifts_ShiftGroupId=" + ShiftGroupId + " where Atten_Shifts.Deactivate=0 and Atten_Shifts.ShiftBranchId=" + ShiftGroupBranchId + " order by Atten_Shifts.BeginTime ");
                    ViewBag.SelectedShiftList = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param1).ToList();
                }
                return View(Atten_ShiftGroups);
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
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetShiftList
        [HttpGet]
        public ActionResult GetShiftList(int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "Select ShiftId,Atten_Shifts.ShiftName+' ( '+SName+' ) '+' ( '+format(CONVERT(datetime, Atten_Shifts.BeginTime),'HH:mm')+ +'-'+  format(CONVERT(datetime, Atten_Shifts.EndTime), 'HH:mm') +' )'	as ShiftName , case when ShiftFlag=1 then 'Day & Night' else 'Day' end as ShiftName,ShiftFlag from Atten_Shifts where Deactivate=0 and ShiftBranchId = '" + BranchId + "' order by Atten_Shifts.BeginTime ");
                var GetLocation = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).ToList();
                var data = GetLocation;
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region IsValidation
        [HttpPost]
        public ActionResult IsShiftGroupExists(string ShiftGroupIdEncrypted, string CompanyName, string BusinessUnit, string ShiftGroupFName, string ShiftGroupSName, List<ShiftGroup> lstShiftGroup)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                List<int> Arraylist = new List<int>() { };
                string result = "";
                bool SecondDayShiftApplicable;
                Session["SecondDayShiftApplicable"] = false;
                for (var i = 0; i < lstShiftGroup.Count; i++)
                {
                    result = result + lstShiftGroup[i].ShiftId + ",";
                    if (lstShiftGroup[i].DayNight == true)
                    {
                        SecondDayShiftApplicable = lstShiftGroup[i].DayNight;
                        Session["SecondDayShiftApplicable"] = SecondDayShiftApplicable;
                    }

                }

                result = result.TrimEnd(',');
                Session["lstShiftGroup"] = result;
                param.Add("@p_process", "IsValidation");
                param.Add("@P_ShiftGroupId_Encrypted", ShiftGroupIdEncrypted);
                param.Add("@P_ShiftGroupBranchId", BusinessUnit);
                param.Add("@P_CmpID", CompanyName);
                param.Add("@P_ShiftGroupFName", ShiftGroupFName);
                param.Add("@P_ShiftGroupSName", ShiftGroupSName);
                param.Add("@p_shiftgroupShiftIDs", result);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_ShiftGroups", param);
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
        public ActionResult SaveUpdate(Atten_ShiftGroups group)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var lstShiftGroup = Session["lstShiftGroup"];

                param.Add("@p_process", string.IsNullOrEmpty(group.ShiftGroupId_Encrypted) ? "Save" : "Update");
                param.Add("@P_ShiftGroupId_Encrypted", group.ShiftGroupId_Encrypted);
                param.Add("@P_ShiftGroupBranchId", group.ShiftGroupBranchId);
                param.Add("@P_ShiftGroupFName", group.ShiftGroupFName);
                param.Add("@P_ShiftGroupSName", group.ShiftGroupSName);
                param.Add("@p_shiftgroupShiftIDs", lstShiftGroup);
                param.Add("@P_SecondDayShiftApplicable", Session["SecondDayShiftApplicable"]);
                param.Add("@p_IsDefault", group.IsDefault);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_CmpID", group.CmpID);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Atten_ShiftGroups", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return RedirectToAction("Setting_TimeOffice_ShiftGroup", "Setting_TimeOffice_ShiftGroup");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 53;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_ShiftGroupId_Encrypted", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@P_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.DynamicList("sp_List_Atten_ShiftGroups", param);
                ViewBag.GetTimeAndAttendanceGroupList = data;
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
        public ActionResult Delete(double? ShiftGroupId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_ShiftGroupId", ShiftGroupId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_ShiftGroups", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_TimeOffice_ShiftGroup");
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