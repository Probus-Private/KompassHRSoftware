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

namespace KompassHR.Areas.ESS.Controllers.ESS_ContractorAttendance
{
    public class ESS_ContractorAttendance_OutDoorController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_ContractorAttendance_OutDoor
        #region ESS_ContractorAttendance_OutDoor
        public ActionResult ESS_ContractorAttendance_OutDoor()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 336;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters log = new DynamicParameters();

                DynamicParameters ComapnyName = new DynamicParameters();
                ComapnyName.Add("@P_Qry", "and Status = 'Pending' and Atten_OutDoorCompany.OutDoorCompanyEmployeeId=" + Session["EmployeeId"] + "");
                var OutDoorCompany = DapperORM.DynamicList("sp_List_Atten_OutDoorCompany", ComapnyName);
                ViewBag.OutDoorCompany = OutDoorCompany;

                //DynamicParameters paramEMP = new DynamicParameters();
                //paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeeid<>1 and EmployeeLeft=0 and employeebranchid in (select distinct branchid from UserBranchMapping where AccessWorkForce=1 and isactive=1  and employeeid=" + Session["EmployeeId"] + ") order by Name");
                //var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                //ViewBag.EmployeeName = GetEmployeeName;
                param = new DynamicParameters();
                param.Add("@p_Origin", "ContractorPG");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorEmployeeDropdown", param).ToList();
                ViewBag.EmployeeName = GetEmployeeName;

                var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Atten_OutDoorCompany";
                var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                ViewBag.DocNo = DocNo;


                //var BackDatedDays = "Select BackDatedDays from Atten_GeneralSetting where Deactivate=0 and CmpId=" + Session["CompanyId"] + "";
                //var GetBackDatedDays = DapperORM.DynamicQuerySingle(BackDatedDays);
                //if (GetBackDatedDays != null)
                //{
                //    ViewBag.GetBackDatedDays = GetBackDatedDays.BackDatedDays;
                //}
                //else
                //{
                //    ViewBag.GetBackDatedDays = "";
                //}
                //  ViewBag.GetBackDatedDays = GetBackDatedDays.BackDatedDays;


                //DynamicParameters paramShift = new DynamicParameters();
                //paramShift.Add("@p_EmployeeID", Session["EmployeeId"]);
                //ViewBag.GetShiftList = DapperORM.ReturnList<Atten_Shifts>("sp_GetShift", paramShift);

                param = new DynamicParameters();

                var ShortHours = DapperORM.DynamicQuerySingle("Select OutDoorCompanyShortPeriod,OutDoorCompanyIstHalfDuration,OutDoorCompanyIIndHalfDuration,OutDoorCompanyFullDayDuration from Atten_GeneralSetting where CmpID=" + Session["CompanyId"] + " and OutDoorCompanyBranchId="+ Session["BranchId"] + "and OutDoorCompanySettingName='Worker'");
                if (ShortHours != null)
                {
                    ViewBag.OutDoorCompanyShortHrs = ShortHours.OutDoorCompanyShortPeriod;
                    ViewBag.OutDoorCompanyIstHalfDuration = ShortHours.OutDoorCompanyIstHalfDuration;
                    ViewBag.OutDoorCompanyIIndHalfDuration = ShortHours.OutDoorCompanyIIndHalfDuration;
                    ViewBag.OutDoorCompanyFullDayDuration = ShortHours.OutDoorCompanyFullDayDuration;
                }

                //param = new DynamicParameters();
                //param.Add("@p_EmployeeID", Session["EmployeeId"]);
                //ViewBag.GetDeviceLogsList = DapperORM.ReturnList<dynamic>("sp_GetDeviceLogs", param).ToList();
                ViewBag.GetShiftList = "";
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 336;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_Qry", "and Mas_Employee.employeebranchid in (select distinct branchid from UserBranchMapping where AccessWorkForce=1 and isactive=1  and employeeid=" + Session["EmployeeId"] + ") order by DocNo desc");
                var OutDoorCompanyAll = DapperORM.DynamicList("sp_List_Atten_Worker_OutDoorCompany", param);
                ViewBag.OutDoorCompanyAll = OutDoorCompanyAll;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetEmployeeShift
        [HttpGet]
        public ActionResult GetEmployeeShift(int EmployeeID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters paramShift = new DynamicParameters();
                paramShift.Add("@p_EmployeeID", EmployeeID);
                var GetShiftList = DapperORM.ReturnList<Atten_Shifts>("sp_GetShift", paramShift);

                DynamicParameters paramEMP = new DynamicParameters();
                paramEMP.Add("@query", "Select EmployeeBranchId from Mas_Employee where EmployeeId=" + EmployeeID + "");
                var GetEmployeeName = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramEMP).FirstOrDefault();

                DynamicParameters paramEMPSetting = new DynamicParameters();
                paramEMPSetting.Add("@query", "select EM_Atten_Atten_OutDoorCompanySettingId from Mas_Employee_Attendance where AttendanceEmployeeId=" + EmployeeID + " and Deactivate=0");
                var GetEmployeeOutDoorCompanySetting = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramEMPSetting).FirstOrDefault();
               // var GetEmployeeNameShortperiod = "";
                if (GetEmployeeOutDoorCompanySetting.EM_Atten_Atten_OutDoorCompanySettingId!=null)
                {
                    DynamicParameters paramShort = new DynamicParameters();
                    paramShort.Add("@query", "Select OutDoorCompanyShortPeriod,OutDoorCompanyIstHalfDuration,OutDoorCompanyIIndHalfDuration,OutDoorCompanyFullDayDuration from Atten_GeneralSetting where  AttenGeneralId=" + GetEmployeeOutDoorCompanySetting.EM_Atten_Atten_OutDoorCompanySettingId + "and Deactivate=0");
                    var GetEmployeeNameShort = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramShort).FirstOrDefault();
                    //GetEmployeeNameShortperiod = GetEmployeeNameShort;
                    return Json(new { GetShiftList = GetShiftList, GetEmployeeNameShort = GetEmployeeNameShort }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    DynamicParameters paramShort = new DynamicParameters();
                    paramShort.Add("@query", "Select OutDoorCompanyShortPeriod,OutDoorCompanyIstHalfDuration,OutDoorCompanyIIndHalfDuration,OutDoorCompanyFullDayDuration from Atten_GeneralSetting where  OutDoorCompanyBranchId=" + GetEmployeeName.EmployeeBranchId + "and Deactivate=0 and IsDefault=1");
                    var GetEmployeeNameShort = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramShort).FirstOrDefault();
                    //GetEmployeeNameShortperiod = GetEmployeeNameShort;
                    return Json(new { GetShiftList = GetShiftList, GetEmployeeNameShort = GetEmployeeNameShort }, JsonRequestBehavior.AllowGet);
                }
                
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region OutDoorType
        [HttpGet]
        public ActionResult OutDoorType(int ShiftId, string DayType)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramShift = new DynamicParameters();
                paramShift.Add("@p_DayType", DayType);
                paramShift.Add("@p_ShifId", ShiftId);
                var data = DapperORM.DynamicList("sp_GetShiftTime", paramShift);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetContactDetails
        [HttpGet]
        public ActionResult GetContactDetails(int EmployeeId, string OutDoorCompanyId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_Origin", "ODRequest");
                param.Add("@p_DocId_Encrypted", OutDoorCompanyId_Encrypted);
                var GetODlist = DapperORM.ExecuteSP<dynamic>("Sp_GetManager_Module", param).ToList(); // SP_getReportingManager
                var GetOutDoor = GetODlist;
                return Json(new { data = GetOutDoor }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetContactDetails
        [HttpGet]
        public ActionResult GetTotalDuration(string FromTime, string ToTime, int? EmployeeID)
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

                DynamicParameters paramEMP = new DynamicParameters();
                paramEMP.Add("@query", "Select EmployeeBranchId from Mas_Employee where EmployeeId=" + EmployeeID + "");
                var GetEmployeeName = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramEMP).FirstOrDefault();

                DynamicParameters paramEMPSetting = new DynamicParameters();
                paramEMPSetting.Add("@query", "select EM_Atten_Atten_OutDoorCompanySettingId from Mas_Employee_Attendance where AttendanceEmployeeId=" + EmployeeID + " and Deactivate=0");
                var GetEmployeeOutDoorCompanySetting = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramEMPSetting).FirstOrDefault();
                var GetEmployeeNameShortperiod = "";
                if (GetEmployeeOutDoorCompanySetting.EM_Atten_Atten_OutDoorCompanySettingId != null)
                {
                    DynamicParameters paramShort = new DynamicParameters();
                    paramShort.Add("@query", "Select OutDoorCompanyShortPeriod,OutDoorCompanyIstHalfDuration,OutDoorCompanyIIndHalfDuration,OutDoorCompanyFullDayDuration from Atten_GeneralSetting where  AttenGeneralId=" + GetEmployeeOutDoorCompanySetting.EM_Atten_Atten_OutDoorCompanySettingId + "and Deactivate=0");
                    var GetEmployeeNameShort = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramShort).FirstOrDefault();
                    GetEmployeeNameShortperiod = GetEmployeeNameShort;
                }
                else
                {
                    DynamicParameters paramShort = new DynamicParameters();
                    paramShort.Add("@query", "Select OutDoorCompanyShortPeriod,OutDoorCompanyIstHalfDuration,OutDoorCompanyIIndHalfDuration,OutDoorCompanyFullDayDuration from Atten_GeneralSetting where  OutDoorCompanyBranchId=" + GetEmployeeName.EmployeeBranchId + "and Deactivate=0 and IsDefault=1");
                    var GetEmployeeNameShort = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramShort).FirstOrDefault();
                    GetEmployeeNameShortperiod = GetEmployeeNameShort;
                }
                return Json(new { data = TotalDuration, GetEmployeeNameShortperiod = GetEmployeeNameShortperiod }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion    

        #region GetLunchTime
        [HttpGet]
        public ActionResult GetLunchTime(int ShiftId,int EmployeeID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var GetCmpID = DapperORM.DynamicQuerySingle("Select CmpID from Mas_Employee where EmployeeId="+ EmployeeID + "").FirstOrDefault();
                var CmpID = GetCmpID.CmpID;
                DynamicParameters ParLunchTime = new DynamicParameters();
                ParLunchTime.Add("@query", @"select LEFT(Atten_Shifts.LunchTime,5) as LunchTime
                from Mas_Employee, Atten_Shifts, Atten_ShiftGroups, Atten_ShiftGroupShifts, Mas_Employee_Attendance
                where Mas_Employee.Deactivate = 0
                and  Atten_Shifts.Deactivate = 0  and  Atten_Shifts.CmpId = " + CmpID + " and  Atten_ShiftGroups.Deactivate = 0 and Atten_ShiftGroups.CmpID = " + CmpID + " and  Mas_Employee_Attendance.AttendanceEmployeeId = Mas_Employee.EmployeeId and  Mas_Employee_Attendance.EM_Atten_ShiftGroupid = Atten_ShiftGroups.ShiftGroupId and Atten_Shifts.ShiftId=" + ShiftId + "  and  Atten_ShiftGroups.ShiftGroupId = Atten_ShiftGroupShifts.Atten_ShiftGroupShifts_ShiftGroupId and  Atten_ShiftGroupShifts.Atten_ShiftGroupShifts_ShiftId = Atten_Shifts.ShiftId and  EmployeeId = " + EmployeeID + "");
                var LunchTime = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", ParLunchTime).FirstOrDefault();
                return Json(new { data = LunchTime }, JsonRequestBehavior.AllowGet);
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
        public ActionResult IsOutdoorExists(/*int EmployeeId,*/ DateTime Fromdate, DateTime Todate, string OutDoorCompanyShift, string AdditionalNotifyPerson,int EmployeeID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                //var AdditionalNotifyPersons = AdditionalNotifyPerson.ToString().Trim().Replace("[", "");
                //var AdditionalNotify = AdditionalNotifyPersons.ToString().Trim().Replace("]", "");
                //var AdditionalNotifys = AdditionalNotify.ToString().Trim().Replace(@"""", "");
                //Session["AdditionalNotify"] = AdditionalNotifys;
                param.Add("@p_process", "IsValidation");
                param.Add("@p_OutDoorCompanyEmployeeId", EmployeeID);
                param.Add("@p_FromDate", Fromdate);
                param.Add("@p_ToDate", Todate);
                param.Add("@p_OutDoorCompanyType", OutDoorCompanyShift);
                //param.Add("@p_Direction", Diraction);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_Worker_OutDoorCompany", param);
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
        public ActionResult SaveUpdate(Models.ESS_TimeOffice.Atten_OutDoorCompany attenOutDoor)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 336;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Save");
                //param.Add("@p_OutDoorCompanyId", attenOutDoor.OutDoorCompanyID);
                param.Add("@p_OutDoorCompanyId_Encrypted", attenOutDoor.OutDoorCompanyID_Encrypted);
                param.Add("@p_OutDoorCompanyEmployeeId", attenOutDoor.OutDoorCompanyEmployeeID);
                //param.Add("@p_OutDoorCompanyEmployeeId", attenOutDoor.OutDoorCompanyEmployeeID);
                param.Add("@p_OutDoorCompanyType", attenOutDoor.OutDoorCompanyType);
                param.Add("@p_OutDoorCompanyShiftId", attenOutDoor.OutDoorCompanyShiftId);
                //param.Add("@p_Direction", attenOutDoor.Direction);
                param.Add("@p_FromDate", attenOutDoor.FromDate);
                param.Add("@p_ToDate", attenOutDoor.ToDate);
                param.Add("@p_FromTime", attenOutDoor.FromTime);
                param.Add("@p_ToTime", attenOutDoor.ToTime);
                param.Add("@p_Reason", attenOutDoor.Reason);
                param.Add("@p_RequestFrom", "ContractorWeb");
                param.Add("@p_ReportingManager1", Session["EmployeeId"]);
             
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_totalDuration", attenOutDoor.TotalDuration);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_Worker_OutDoorCompany", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_ContractorAttendance_OutDoor", "ESS_ContractorAttendance_OutDoor");

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
        public ActionResult Delete(string OutDoorCompanyId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 336;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_OutDoorCompanyId_Encrypted", OutDoorCompanyId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_Worker_OutDoorCompany", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_ContractorAttendance_GetList", "ESS_ContractorAttendance_OutDoor");
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