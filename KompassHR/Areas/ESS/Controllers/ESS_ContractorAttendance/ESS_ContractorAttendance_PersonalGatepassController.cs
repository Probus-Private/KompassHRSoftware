using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Areas.Setting.Models.Setting_TimeOffice;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_ContractorAttendance
{
    public class ESS_ContractorAttendance_PersonalGatepassController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_ContractorAttendance_PersonalGatepass

        #region ESS_ContractorAttendance_PersonalGatepass
        public ActionResult ESS_ContractorAttendance_PersonalGatepass()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 335;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                DynamicParameters log = new DynamicParameters();

                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {

                    param = new DynamicParameters();


                    var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Atten_PersonalGatepass";
                    var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    ViewBag.DocNo = DocNo;

                    //var BackDatedDays = "Select BackDateDays from Atten_PersonalGatepassSetting where Deactivate=0 and CmpId=" + Session["CompanyId"] + "";
                    //var GetBackDatedDays = DapperORM.DynamicQuerySingle(BackDatedDays);
                    //if (GetBackDatedDays != null)
                    //{
                    //    ViewBag.GetBackDatedDays = GetBackDatedDays.BackDateDays;
                    //}
                    //else
                    //{
                    //    ViewBag.GetBackDatedDays = 1;
                    //}
                    DynamicParameters paramPG = new DynamicParameters();
                    paramPG.Add("@query", "Select EM_Atten_Atten_PersonalGatepassSettingId from Mas_Employee_Attendance where AttendanceEmployeeId=" + Session["EmployeeId"] + " and Deactivate=0");
                    var GetPMSettingId = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramPG).FirstOrDefault();

                    if (GetPMSettingId != null)
                    {
                        var PGSettingId = GetPMSettingId.EM_Atten_Atten_PersonalGatepassSettingId;
                        DynamicParameters paramBackDay = new DynamicParameters();
                        paramBackDay.Add("@query", "Select BackDateDays from Atten_PersonalGatepassSetting where Deactivate=0 and CmpId=" + Session["CompanyId"] + " and PersonalGatepassSettingBranchId=" + Session["BranchId"] + "  and PersonalGatepassSettingId=" + PGSettingId + " ");
                        var GetBackDatedDay = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramBackDay).FirstOrDefault();
                        if (GetBackDatedDay != null)
                        {
                            ViewBag.GetBackDatedDays = GetBackDatedDay.BackDateDays;
                        }
                        else
                        {
                            ViewBag.GetBackDatedDays = "";
                        }

                        DynamicParameters paramTotalDuration = new DynamicParameters();
                        paramBackDay.Add("@query", "Select  Minute from  Atten_PersonalGatepassSetting where Deactivate=0 and CmpId=" + Session["CompanyId"] + " and PersonalGatepassSettingBranchId=" + Session["BranchId"] + "  and PersonalGatepassSettingId=" + PGSettingId + "");
                        var Duration = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramBackDay).FirstOrDefault();
                        if (Duration != null)
                        {
                            ViewBag.Duration = Duration.Minute;
                        }
                        else
                        {
                            ViewBag.Duration = 0;
                        }
                    }
                    else
                    {
                        ViewBag.Duration = 0;
                        ViewBag.GetBackDatedDays = "";
                    }

                    param = new DynamicParameters();
                    param.Add("@p_Origin", "ContractorPG");
                    param.Add("@p_EmployeeId", Session["EmployeeId"]);
                    var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorEmployeeDropdown", param).ToList();
                    ViewBag.EmployeeName = GetEmployeeName;

                    ViewBag.GetShiftList = "";

                    param = new DynamicParameters();
                    param.Add("@p_EmployeeID", Session["EmployeeId"]);
                    ViewBag.GetDeviceLogsList = DapperORM.ReturnList<dynamic>("sp_GetDeviceLogs", param).ToList();

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

        #region ESS_ContractorAttendance_GetList
        public ActionResult ESS_ContractorAttendance_GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 335;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@P_Qry", "and Mas_Employee.employeebranchid in (select distinct branchid from UserBranchMapping where AccessWorkForce=1 and isactive=1  and employeeid=" + Session["EmployeeId"] + ") order by DocNo desc ");
                var PersoanlGetpass = DapperORM.ExecuteSP<dynamic>("sp_List_Atten_Worker_PersonalGatepass", param);
                ViewBag.PersoanlGetpass = PersoanlGetpass;

                var qyery = "and branchId in (select distinct BranchID from UserBranchMapping where AccessWorkForce=1 and IsActive=1 and EmployeeID='" + Session["EmployeeId"] + "')";

                return View();
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
        public ActionResult SaveUpdate(Atten_PersonalGatepass AttenPersonalGetPass)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 335;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var CmpId = Session["CompanyId"];
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Save");
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_PersonalGatepassId", AttenPersonalGetPass.PersonalGatepassID);
                param.Add("@p_PersonalGatepassId_Encrypted", AttenPersonalGetPass.PersonalGatepassID_Encrypted);
                param.Add("@p_PersonalGatepassEmployeeId", AttenPersonalGetPass.EmployeeID);
                param.Add("@p_PersonalGatepassBranchId", AttenPersonalGetPass.PersonalGatepassBranchId);
                param.Add("@p_PersonalGatepassShiftId", AttenPersonalGetPass.PersonalGatepassShiftId);
                param.Add("@p_PersonalGatepassShiftName", AttenPersonalGetPass.PersonalGatepassShiftName);
                param.Add("@p_Direction", AttenPersonalGetPass.Direction);
                param.Add("@p_ReportingManager1", Session["EmployeeId"]);
                param.Add("@p_PersonalGatepassDate", AttenPersonalGetPass.PersonalGatepassDate);
                param.Add("@p_FromDateTime", AttenPersonalGetPass.FromTime);
                param.Add("@p_ToDateTime", AttenPersonalGetPass.ToTime);
                param.Add("@p_TotalDuration", AttenPersonalGetPass.TotalDurations);
                param.Add("@p_Reason", AttenPersonalGetPass.Reason);
                param.Add("@p_ApprovedAutoManually", AttenPersonalGetPass.ApprovedAutoManually);
                param.Add("@p_AdditionNotify", Session["AdditionalNotify"]);
                param.Add("@p_RequestFrom", "ContractorWeb");
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_Worker_PersonalGatepass", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                return RedirectToAction("ESS_ContractorAttendance_PersonalGatepass", "ESS_ContractorAttendance_PersonalGatepass");

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
        public ActionResult IsPersoanlGetPassExists(int EmployeeID, DateTime PersoanlDate, DateTime FromTime, DateTime ToTime, Double ShiftId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 335;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "IsValidation");
                param.Add("@p_PersonalGatepassEmployeeId", EmployeeID);
                param.Add("@p_PersonalGatepassShiftId", ShiftId);
                param.Add("@p_FromDateTime", FromTime);
                param.Add("@p_ToDateTime", ToTime);
                param.Add("@p_PersonalGatepassDate", PersoanlDate);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_Worker_PersonalGatepass", param);
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

        #region PersonalGetPassDelete
        [HttpGet]
        public ActionResult PersonalGetPassDelete(string PersonalGatepassId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 335;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_PersonalGatepassId_Encrypted", PersonalGatepassId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_Worker_PersonalGatepass", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_ContractorAttendance_GetList", "ESS_ContractorAttendance_PersonalGatepass");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetTotalDuration
        [HttpGet]
        public ActionResult GetTotalDuration(string FromTime, string ToTime)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                TimeSpan t1 = TimeSpan.Parse(FromTime);
                TimeSpan t2 = TimeSpan.Parse(ToTime);
                double _24h = (new TimeSpan(24, 0, 0)).TotalMilliseconds;
                double diff = t2.TotalMilliseconds - t1.TotalMilliseconds;
                if (diff < 0) diff += _24h;
                var TotalDuration = TimeSpan.FromMilliseconds(diff);
                return Json(new { data = TotalDuration }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetShift
        public ActionResult GetShift(int EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", EmployeeId);
                var data = DapperORM.ReturnList<Atten_Shifts>("sp_GetShift", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetShiftFlag
        [HttpGet]
        public ActionResult GetShiftFlag(int ShiftId)
        {
            try
            {
                DynamicParameters paramFlag = new DynamicParameters();
                paramFlag.Add("@query", "Select  ShiftFlag  from Atten_Shifts where Deactivate=0 and ShiftId=" + ShiftId + "");
                var ShidtFlag = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramFlag).FirstOrDefault();
                return Json(ShidtFlag, JsonRequestBehavior.AllowGet);
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