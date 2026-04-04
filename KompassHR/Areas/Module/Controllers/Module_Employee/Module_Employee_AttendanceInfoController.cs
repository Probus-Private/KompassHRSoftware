using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Setting.Models.Setting_Leave;
using KompassHR.Areas.Setting.Models.Setting_TimeOffice;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_AttendanceInfoController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Employee/AttendanceInformation
        #region AttendanceInfo Main View 
        [HttpGet]
        public ActionResult Module_Employee_AttendanceInfo()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 416;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Mas_Employee_Attendance mas_EmployeeAttendance = new Mas_Employee_Attendance();
                var OnboardEmployeeId = Session["OnboardEmployeeId"];
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EmployeeId", OnboardEmployeeId);
                var StatusCheck = DapperORM.DynamicList("sp_List_Mas_Employee_StatusCheck", paramList);
                ViewBag.GetStatusCheckList = StatusCheck;


                string connectionString = Session["MyNewConnectionString"]?.ToString();
                using (SqlConnection connectionNew = new SqlConnection(connectionString))
                {
                    var ResultList = connectionNew.QueryMultiple(@"Select ShiftGroupId As Id,ShiftGroupFName As Name from Atten_ShiftGroups Where Deactivate=0 and CmpID=" + Session["OnboardCmpId"] + " and ShiftGroupBranchId=" + Session["OnboardBranchId"] + ";"
                                                        + "Select ShiftRuleId As Id,ShiftRuleName As Name from Atten_ShiftRule Where Deactivate=0 and CmpId=" + Session["OnboardCmpId"] + " and ShiftRuleBranchId=" + Session["OnboardBranchId"] + "; "
                                                        + "Select PersonalGatepassSettingId As Id,PersonalGatepassSettingName As Name from Atten_PersonalGatepassSetting Where Deactivate=0 and CmpId=" + Session["OnboardCmpId"] + " and PersonalGatepassSettingBranchId=" + Session["OnboardBranchId"] + ";"
                                                        + "Select ShortLeaveSettingId As Id,ShortLeaveSettingName As Name from Leave_ShortLeaveSetting Where Deactivate=0 and CmpID=" + Session["OnboardCmpId"] + " and ShortLeaveSettingBranchId=" + Session["OnboardBranchId"] + ";"
                                                          + "Select PunchMissingSettingId As Id,PunchMissingSettingName As Name from Atten_PunchMissingSetting Where Deactivate=0 and CmpId=" + Session["OnboardCmpId"] + " and PunchMissingSettingBranchId=" + Session["OnboardBranchId"] + ";"
                                                           + "Select AttenGeneralId As Id, OutDoorCompanySettingName as Name from Atten_GeneralSetting where CmpId =" + Session["OnboardCmpId"] + " and OutDoorCompanyBranchId =" + Session["OnboardBranchId"] + "; "
                                                           + "Select LateMarkSettingId As Id, LateMarkSettingName as Name from Atten_LateMarkSettingMaster where Deactivate=0 and CmpId =" + Session["OnboardCmpId"] + " and LateMarkSettingBranchId =" + Session["OnboardBranchId"] + "; "
                                                        + "Select LeaveGroupId As Id,LeaveGroupName As Name from Leave_Group Where Deactivate=0 and CmpId=" + Session["OnboardCmpId"] + ";");
                    // ViewBag.GetCoffSettingName = ResultList.Read<AllDropDownClass>().ToList();
                    ViewBag.GetShiftGroupsName = ResultList.Read<AllDropDownClass>().ToList();
                    ViewBag.GetShiftRuleName = ResultList.Read<AllDropDownClass>().ToList();
                    ViewBag.GetPersonalGatepassName = ResultList.Read<AllDropDownClass>().ToList();
                    ViewBag.GetShortLeaveName = ResultList.Read<AllDropDownClass>().ToList();
                    ViewBag.GetPunchMissingName = ResultList.Read<AllDropDownClass>().ToList();
                    ViewBag.GetOutdoorSettingName = ResultList.Read<AllDropDownClass>().ToList();
                    ViewBag.GetLateMarkSettingName = ResultList.Read<AllDropDownClass>().ToList();
                    ViewBag.GetLeaveGroupName = ResultList.Read<AllDropDownClass>().ToList();
                }
                ViewBag.LocationList = "";
               


                param = new DynamicParameters();
                TempData["OnboardEmployeeName"] = Session["OnboardEmployeeName"];
                param.Add("@p_AttendanceEmployeeId", Session["OnboardEmployeeId"]);
                mas_EmployeeAttendance = DapperORM.ReturnList<Mas_Employee_Attendance>("sp_List_Mas_Employee_Attendance", param).FirstOrDefault();
                if (mas_EmployeeAttendance != null)
                {
                    ViewBag.AddUpdateTitle = "Update";

                    if (!string.IsNullOrEmpty(mas_EmployeeAttendance.EM_Atten_LocationRegistrationId))
                    {
                        var Location = DapperORM.DynamicQueryMultiple(@"Select LR.LocationRegistrationId as Id , LR.LocationName as Name from Mas_LocationRegistrationMapping_Master LRM Inner Join Mas_LocationRegistrationMapping_Detail LRD on LRD.LocationRegistrationMappingIMasterId= LRM.LocationRegistrationMappingIMasterId Inner Join Mas_LocationRegistration LR on LR.LocationRegistrationId=LRD.LocationRegistrationId and LR.Deactivate=0 where LRM.Deactivate=0 and LRM.LocationRegistrationMappingIMasterId='" + mas_EmployeeAttendance.EM_Atten_LocationRegistrationMappingIMasterId + "' Order by LR.LocationName");
                        ViewBag.LocationList = Location[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                        ViewBag.SelectedLocationIds = mas_EmployeeAttendance.EM_Atten_LocationRegistrationId.Split(',').Select(id => id.Trim()).ToList();
                    }
                    else if (mas_EmployeeAttendance.EM_Atten_LocationRegistrationMappingIMasterId > 0)
                    {
                        DynamicParameters param = new DynamicParameters();
                        param.Add("@query", "Select LR.LocationRegistrationId as Id , LR.LocationName as Name from Mas_LocationRegistrationMapping_Master LRM Inner Join Mas_LocationRegistrationMapping_Detail LRD on LRD.LocationRegistrationMappingIMasterId= LRM.LocationRegistrationMappingIMasterId Inner Join Mas_LocationRegistration LR on LR.LocationRegistrationId=LRD.LocationRegistrationId and LR.Deactivate=0 where LRM.Deactivate=0 and LRM.LocationRegistrationMappingIMasterId='" + mas_EmployeeAttendance.EM_Atten_LocationRegistrationMappingIMasterId + "' Order by LR.LocationName");
                        var Location = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param).ToList();
                        ViewBag.LocationList = Location;
                    }
                    else
                    {
                        ViewBag.SelectedLocationIds = new List<string>();
                    }
                }

                ViewBag.GetAttendance = mas_EmployeeAttendance;

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "select LocationRegistrationMappingIMasterId as Id,GroupName as Name from Mas_LocationRegistrationMapping_Master where Deactivate=0");
                var GetLocationGroup = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param1).ToList();
                ViewBag.GetLocationGroup = GetLocationGroup;

                var result = DapperORM.DynamicQuerySingle(@"SELECT IsCanteenApplicable FROM Tool_CommonTable");
                TempData["GetIsCanteenApplicable"] = result.IsCanteenApplicable;

                return View(mas_EmployeeAttendance);
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
        public ActionResult SaveUpdate(Mas_Employee_Attendance EmployeeAttendance)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                string CheckBox = "";

                if (EmployeeAttendance.EM_Atten_WOFF_Check1 == true)
                {
                    CheckBox = CheckBox + "1";
                }

                if (EmployeeAttendance.EM_Atten_WOFF_Check2 == true)
                {
                    if (CheckBox == "")
                    {
                        CheckBox = CheckBox + "2";
                    }
                    else
                    {
                        CheckBox = CheckBox + ",2";
                    }
                }
                if (EmployeeAttendance.EM_Atten_WOFF_Check3 == true)
                {
                    if (CheckBox == "")
                    {
                        CheckBox = CheckBox + "3";
                    }
                    else
                    {
                        CheckBox = CheckBox + ",3";
                    }
                }
                if (EmployeeAttendance.EM_Atten_WOFF_Check4 == true)
                {
                    if (CheckBox == "")
                    {
                        CheckBox = CheckBox + "4";
                    }
                    else
                    {
                        CheckBox = CheckBox + ",4";
                    }
                }
                if (EmployeeAttendance.EM_Atten_WOFF_Check5 == true)
                {
                    if (CheckBox == "")
                    {
                        CheckBox = CheckBox + "5";
                    }
                    else
                    {
                        CheckBox = CheckBox + ",5";
                    }
                }

                param.Add("@p_process", string.IsNullOrEmpty(EmployeeAttendance.AttendanceId_Encrypted) ? "Save" : "Update");
                param.Add("@p_AttendanceId", EmployeeAttendance.AttendanceId);
                param.Add("@p_AttendanceId_Encrypted", EmployeeAttendance.AttendanceId_Encrypted);
                param.Add("@p_AttendanceEmployeeId", Session["OnboardEmployeeId"]);
                param.Add("@p_EmployeeCardNo", EmployeeAttendance.EmployeeCardNo);
                param.Add("@p_EM_Atten_OT_Applicable", EmployeeAttendance.EM_Atten_OT_Applicable);
                param.Add("@p_EM_Atten_OTMultiplyBy", EmployeeAttendance.EM_Atten_OTMultiplyBy);
                param.Add("@p_EM_Atten_PerDayShiftHrs", EmployeeAttendance.EM_Atten_PerDayShiftHrs);

                if (EmployeeAttendance.EM_Atten_CoffApplicable == null)
                {
                    param.Add("@p_EM_Atten_CoffApplicable", "0");

                }
                else
                {
                    param.Add("@p_EM_Atten_CoffApplicable", EmployeeAttendance.EM_Atten_CoffApplicable);
                }

                param.Add("@p_EM_Atten_CoffSettingId", 0);
                param.Add("@p_EM_Atten_ShiftGroupId", EmployeeAttendance.EM_Atten_ShiftGroupId);
                param.Add("@p_EM_Atten_ShiftRuleId", EmployeeAttendance.EM_Atten_ShiftRuleId);
                param.Add("@p_EM_Atten_WOFF1", EmployeeAttendance.EM_Atten_WOFF1);
                param.Add("@p_EM_Atten_WOFF2", EmployeeAttendance.EM_Atten_WOFF2);
                param.Add("@p_EM_Atten_WOFF2_ForCheck", CheckBox);
                param.Add("@p_EM_Atten_WOFF_Check1", EmployeeAttendance.EM_Atten_WOFF_Check1);
                param.Add("@p_EM_Atten_WOFF_Check2", EmployeeAttendance.EM_Atten_WOFF_Check2);
                param.Add("@p_EM_Atten_WOFF_Check3", EmployeeAttendance.EM_Atten_WOFF_Check3);
                param.Add("@p_EM_Atten_WOFF_Check4", EmployeeAttendance.EM_Atten_WOFF_Check4);
                param.Add("@p_EM_Atten_WOFF_Check5", EmployeeAttendance.EM_Atten_WOFF_Check5);
                param.Add("@p_EM_Atten_LateMarkSettingApplicable", EmployeeAttendance.EM_Atten_LateMarkSettingApplicable);
                param.Add("@p_EM_Atten_LateMarkSettingId", EmployeeAttendance.EM_Atten_LateMarkSettingId);

                param.Add("@p_EM_Atten_LeaveGroupId", EmployeeAttendance.EM_Atten_LeaveGroupId);
                if (EmployeeAttendance.EM_Atten_IsSalaryFullPay == null)
                {
                    param.Add("@p_EM_Atten_IsSalaryFullPay", "0");
                }
                else
                {
                    param.Add("@p_EM_Atten_IsSalaryFullPay", EmployeeAttendance.EM_Atten_IsSalaryFullPay);
                }
                if (EmployeeAttendance.EM_Atten_DefaultAttenShow == null)
                {
                    param.Add("@p_EM_Atten_DefaultAttenShow", "1");
                }
                else
                {
                    param.Add("@p_EM_Atten_DefaultAttenShow", EmployeeAttendance.EM_Atten_DefaultAttenShow);
                }

                param.Add("@p_EM_Atten_SinglePunch_Present", EmployeeAttendance.EM_Atten_SinglePunch_Present);
                param.Add("@p_EM_Atten_RotationalWeekOff", EmployeeAttendance.EM_Atten_RotationalWeekOff);
                if (EmployeeAttendance.EM_Atten_Regularization_Required == null)
                {
                    param.Add("@p_EM_Atten_Regularization_Required", "1");
                }
                else
                {
                    param.Add("@p_EM_Atten_Regularization_Required", EmployeeAttendance.EM_Atten_Regularization_Required);
                }

                //param.Add("@p_EM_Atten_ShortLeaveApplicable", EmployeeAttendance.EM_Atten_ShortLeaveApplicable);
                // param.Add("@p_EM_Atten_ShortLeaveSettingId", EmployeeAttendance.EM_Atten_ShortLeaveSettingId);
                param.Add("@p_EM_Atten_PersonalGatepassApplicable", EmployeeAttendance.EM_Atten_PersonalGatepassApplicable);
                param.Add("@p_EM_Atten_Atten_PersonalGatepassSettingId", EmployeeAttendance.EM_Atten_Atten_PersonalGatepassSettingId);
                param.Add("@p_EM_Atten_PunchMissingApplicable", EmployeeAttendance.EM_Atten_PunchMissingApplicable);
                param.Add("@p_EM_Atten_Atten_PunchMissingSettingId", EmployeeAttendance.EM_Atten_Atten_PunchMissingSettingId);
                param.Add("@p_EM_Atten_flexibleShiftApplicable", EmployeeAttendance.EM_Atten_flexibleShiftApplicable);
                param.Add("@p_EM_Atten_WOPH_CoffApplicable", EmployeeAttendance.EM_Atten_WOPH_CoffApplicable);
                param.Add("@p_EM_Atten_OutDoorCompanyApplicable", EmployeeAttendance.EM_Atten_OutDoorCompanyApplicable);
                param.Add("@p_EM_Atten_Atten_OutDoorCompanySettingId", EmployeeAttendance.EM_Atten_Atten_OutDoorCompanySettingId);
                param.Add("@p_EM_Atten_ShortHRS_Applicable", EmployeeAttendance.EM_Atten_ShortHRS_Applicable);
                param.Add("@p_PHApplicable", EmployeeAttendance.PHApplicable);
                // param.Add("@p_EM_Atten_DailyMonthly", EmployeeAttendance.EM_Atten_DailyMonthly);
                param.Add("@p_EM_Atten_LocationRegistrationMappingIMasterId", EmployeeAttendance.EM_Atten_LocationRegistrationMappingIMasterId);
                param.Add("@p_EM_Atten_LocationRegistrationId", EmployeeAttendance.SelectedLocations);
                param.Add("@p_EM_Atten_IsLocationRemarkCompulsory", EmployeeAttendance.EM_Atten_IsLocationRemarkCompulsory);
                param.Add("@p_EM_Atten_MonthlyRosterApplicable", EmployeeAttendance.EM_Atten_MonthlyRosterApplicable);
                param.Add("@p_EM_Atten_CanteenApplicable", EmployeeAttendance.EM_Atten_CanteenApplicable);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_Attendance]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_AttendanceInfo", "Module_Employee_AttendanceInfo");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Get Location Name
        [HttpGet]
        public ActionResult GetLocation(int LocationGroupId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "Select LR.LocationRegistrationId as Id , LR.LocationName as Name from Mas_LocationRegistrationMapping_Master LRM Inner Join Mas_LocationRegistrationMapping_Detail LRD on LRD.LocationRegistrationMappingIMasterId= LRM.LocationRegistrationMappingIMasterId Inner Join Mas_LocationRegistration LR on LR.LocationRegistrationId=LRD.LocationRegistrationId and LR.Deactivate=0 where LRM.Deactivate=0 and LRM.LocationRegistrationMappingIMasterId='"+ LocationGroupId +"' Order by LR.LocationName");
                var Location = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param).ToList();
                return Json(new { Location = Location }, JsonRequestBehavior.AllowGet);
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