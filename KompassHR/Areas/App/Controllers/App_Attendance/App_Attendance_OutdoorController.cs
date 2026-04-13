using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Areas.Setting.Models.Setting_TimeOffice;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Mvc;


namespace KompassHR.Areas.App.Controllers.App_Attendance
{
    public class App_Attendance_OutdoorController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: App/App_Attendance_Outdoor
        public ActionResult App_Attendance_Outdoor()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 152;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { Area = "App" });
                //}
                var GetDocNo = DapperORM.DynamicQuerySingle("Select Isnull(Max(DocNo),0)+1 As DocNo from Atten_OutDoorCompany");
                var DocNo = GetDocNo != null ? GetDocNo.DocNo : null;
                ViewBag.DocNo = DocNo;

                DynamicParameters paramBranch = new DynamicParameters();
                paramBranch.Add("@query", "Select BranchId as Id,Mas_CompanyProfile.ShortName + ' - '+ BranchName as Name from Mas_Branch,Mas_CompanyProfile where Mas_Branch.Deactivate=0 and Mas_Branch.CmpId=Mas_CompanyProfile.CompanyId");
                ViewBag.VisitingBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranch).ToList();

                //int BackDatedDays=0;
                //  var IsContractor = DapperORM.DynamicQuerySingle(@"Select ContractorID from Mas_Employee where EmployeeId = " + Session["EmployeeId"] + " and Deactivate = 0 and ContractorID = 1").FirstOrDefault();
                //  if(IsContractor.ContractorID ==1)
                //  {
                //      var Staff = DapperORM.DynamicQuerySingle(@"Select BackDatedDays,* from Atten_GeneralSetting where Deactivate=0 and CmpId=" + Session["CompanyId"] + " and OutDoorCompanyBranchId=" + Session["BranchId"] + " and OutDoorCompanySettingName='Staff'").FirstOrDefault();
                //      BackDatedDays=Convert.ToInt32(Staff.BackDatedDays);
                //  }
                //  else
                //  {
                //      var Worker = DapperORM.DynamicQuerySingle(@"Select BackDatedDays,* from Atten_GeneralSetting where Deactivate=0 and CmpId=" + Session["CompanyId"] + " and OutDoorCompanyBranchId=" + Session["BranchId"] + " and OutDoorCompanySettingName='Worker'").FirstOrDefault();
                //      BackDatedDays = Convert.ToInt32(Worker.BackDatedDays);
                //  }

                //  var GetBackDatedDays = BackDatedDays;
                //  if(GetBackDatedDays!=null)
                //  {
                //      ViewBag.GetBackDatedDays = GetBackDatedDays;
                //  }
                //  else
                //  {
                //      ViewBag.GetBackDatedDays = "";
                //  }
                ////  ViewBag.GetBackDatedDays = GetBackDatedDays.BackDatedDays;

                DynamicParameters paramShift = new DynamicParameters();
                paramShift.Add("@p_EmployeeID", Session["EmployeeId"]);
                ViewBag.GetShiftList = DapperORM.ReturnList<Atten_Shifts>("sp_GetShift", paramShift);

                param = new DynamicParameters();
                var ShortHours = DapperORM.DynamicQuerySingle("Select OutDoorCompanyShortPeriod,OutDoorCompanyIstHalfDuration,OutDoorCompanyIIndHalfDuration,OutDoorCompanyFullDayDuration,BackDatedDays,FutureDatedDays from Atten_GeneralSetting where Atten_GeneralSetting.Deactivate=0 and AttenGeneralId = (Select EM_Atten_Atten_OutDoorCompanySettingId from Mas_Employee_Attendance where Mas_Employee_Attendance.Deactivate=0 and Attendanceemployeeid='" + Session["EmployeeId"] + "')");
                //var ShortHours = DapperORM.DynamicQuerySingle("Select OutDoorCompanyShortPeriod,OutDoorCompanyIstHalfDuration,OutDoorCompanyIIndHalfDuration,OutDoorCompanyFullDayDuration,BackDatedDays,FutureDatedDays from Atten_GeneralSetting where Atten_GeneralSetting.Deactivate=0 and AttenGeneralId = (Select EM_Atten_Atten_OutDoorCompanySettingId from Mas_Employee_Attendance where Mas_Employee_Attendance.Deactivate=0 and Attendanceemployeeid='" + Session["EmployeeId"] + "')").FirstOrDefault();
                if (ShortHours != null)
                {
                    ViewBag.OutDoorCompanyShortHrs = ShortHours.OutDoorCompanyShortPeriod;
                    ViewBag.OutDoorCompanyIstHalfDuration = ShortHours.OutDoorCompanyIstHalfDuration;
                    ViewBag.OutDoorCompanyIIndHalfDuration = ShortHours.OutDoorCompanyIIndHalfDuration;
                    ViewBag.OutDoorCompanyFullDayDuration = ShortHours.OutDoorCompanyFullDayDuration;
                    ViewBag.BackDatedDays = ShortHours.BackDatedDays;
                    ViewBag.FutureDatedDays = ShortHours.FutureDatedDays;
                }

                //param = new DynamicParameters();
                //var OutDoorCompanyLastDate = "select max(CreatedDate) As CreatedDate from Atten_OutDoorCompany where deactivate=0";
                //var LastRecored = DapperORM.DynamicQuerySingle(OutDoorCompanyLastDate);
                //ViewBag.LastRecored = LastRecored.CreatedDate;

                param = new DynamicParameters();
                param.Add("@p_EmployeeID", Session["EmployeeId"]);
                ViewBag.GetDeviceLogsList = DapperORM.ReturnList<dynamic>("sp_GetDeviceLogs", param).ToList();
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }

        public ActionResult App_GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_Dashboard", "App_Dashboard", new { Area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 152;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { Area = "App" });
                //}
                param.Add("@P_Qry", "and Atten_OutDoorCompany.OutDoorCompanyEmployeeId=" + Session["EmployeeId"] + "");
                var OutDoorCompanyAll = DapperORM.DynamicList("sp_List_Atten_OutDoorCompany", param);
                ViewBag.OutDoorCompanyAll = OutDoorCompanyAll;

                DynamicParameters ParamManager = new DynamicParameters();
                ParamManager.Add("@query", @"Select EmployeeId as Id , EmployeeName as Name from mas_employee_reporting,mas_employee
                                                where reportingmoduleid = 1 and ReportingEmployeeID = " + Session["EmployeeId"] + " and mas_employee_reporting.Deactivate = 0 and mas_employee_reporting.ReportingManager1 = mas_employee.EmployeeId");
                var Getdata = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", ParamManager);
                ViewBag.GetManagerEmployee = Getdata;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult App_OutdoorDetails(string OutDoorCompanyId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 152;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { Area = "App" });
                //}
                param.Add("@P_Qry", "and Atten_OutDoorCompany.OutDoorCompanyId_Encrypted='" + OutDoorCompanyId_Encrypted + "'");
                var OutDoorCompanyAll = DapperORM.DynamicList("sp_List_Atten_OutDoorCompany", param);
                ViewBag.OutDoorCompanyAll = OutDoorCompanyAll;

                DynamicParameters ParamManager = new DynamicParameters();
                ParamManager.Add("@query", @"Select EmployeeId as Id , EmployeeName as Name from mas_employee_reporting,mas_employee
                                                where reportingmoduleid = 1 and ReportingEmployeeID = " + Session["EmployeeId"] + " and mas_employee_reporting.Deactivate = 0 and mas_employee_reporting.ReportingManager1 = mas_employee.EmployeeId");
                var Getdata = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", ParamManager);
                ViewBag.GetManagerEmployee = Getdata;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }

        #region ESS_TimeOffice_GetApproveRejectList
        public ActionResult App_TimeOffice_GetApproveRejectList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 152;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { Area = "App" });
                //}
                param.Add("@P_Qry", "and Atten_OutDoorCompany.OutDoorCompanyEmployeeId=" + Session["EmployeeId"] + "");
                var OutDoorCompanyAll = DapperORM.DynamicList("sp_List_Atten_OutDoorCompany", param);
                ViewBag.OutDoorCompanyAll = OutDoorCompanyAll;

                DynamicParameters ParamManager = new DynamicParameters();
                ParamManager.Add("@query", @"Select EmployeeId as Id , EmployeeName as Name from mas_employee_reporting,mas_employee
                                                where reportingmoduleid = 1 and ReportingEmployeeID = " + Session["EmployeeId"] + " and mas_employee_reporting.Deactivate = 0 and mas_employee_reporting.ReportingManager1 = mas_employee.EmployeeId");
                var Getdata = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", ParamManager);
                ViewBag.GetManagerEmployee = Getdata;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion

        #region IsValidation
        [HttpGet]
        public ActionResult App_IsOutdoorExists(DateTime Fromdate, DateTime Todate, TimeSpan FromTime, TimeSpan ToTime, string OutDoorCompanyType, int ShiftId, bool IsNightShift)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                //var AdditionalNotifyPersons = AdditionalNotifyPerson.ToString().Trim().Replace("[", "");
                //var AdditionalNotify = AdditionalNotifyPersons.ToString().Trim().Replace("]", "");
                //var AdditionalNotifys = AdditionalNotify.ToString().Trim().Replace(@"""", "");
                //Session["AdditionalNotify"] = AdditionalNotifys;
                param.Add("@p_process", "IsValidation");
                param.Add("@p_OutDoorCompanyEmployeeId", Session["EmployeeId"]);
                param.Add("@p_FromDate", Fromdate);
                param.Add("@p_ToDate", Todate);
                param.Add("@p_FromTime", FromTime);
                param.Add("@p_ToTime", ToTime);
                param.Add("@p_IsNightShift", IsNightShift);
                param.Add("@p_OutDoorCompanyType", OutDoorCompanyType);
                param.Add("@p_OutDoorCompanyShiftId", ShiftId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_OutDoorCompany", param);
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
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult App_SaveUpdate(Atten_OutDoorCompany attenOutDoor)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 152;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { Area = "App" });
                //}

                DynamicParameters param = new DynamicParameters();
                if (attenOutDoor.OutDoorCompanyType == "Short Period")
                {
                    if (Convert.ToDateTime(attenOutDoor.FromDate).Day == Convert.ToDateTime(attenOutDoor.ToDate).Day)
                    {
                        param.Add("@p_IsNightShift", 0);
                    }
                    else
                    {
                        param.Add("@p_IsNightShift", 1);
                    }

                }
                else
                {
                    param.Add("@p_IsNightShift", attenOutDoor.IsNightShift);
                }

                param.Add("@p_process", "Save");

                //param.Add("@p_OutDoorCompanyId", attenOutDoor.OutDoorCompanyID);
                param.Add("@p_OutDoorCompanyId_Encrypted", attenOutDoor.OutDoorCompanyID_Encrypted);
                param.Add("@p_OutDoorCompanyEmployeeId", Session["EmployeeId"]);
                //param.Add("@p_OutDoorCompanyEmployeeId", attenOutDoor.OutDoorCompanyEmployeeID);
                param.Add("@p_OutDoorCompanyType", attenOutDoor.OutDoorCompanyType);
                param.Add("@p_OutDoorCompanyShiftId", attenOutDoor.OutDoorCompanyShiftId);
                param.Add("@p_Direction", attenOutDoor.Direction);
                param.Add("@p_FromDate", attenOutDoor.FromDate);
                param.Add("@p_ToDate", attenOutDoor.ToDate);
                param.Add("@p_FromTime", attenOutDoor.FromTime);
                param.Add("@p_ToTime", attenOutDoor.ToTime);
                param.Add("@p_Reason", attenOutDoor.Reason);
                param.Add("@p_VisitingBranchID", attenOutDoor.VisitingBranchID);
                param.Add("@p_RequestFrom", "Web");
                param.Add("@p_AdditionNotify", Session["AdditionalNotify"]);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_totalDays", attenOutDoor.TotalDays);
                param.Add("@p_totalDuration", attenOutDoor.TotalDuration);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());

                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_OutDoorCompany", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("App_Attendance_Outdoor", "App_Attendance_Outdoor");

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }

        #endregion       

        #region OutDoorType
        [HttpGet]
        public ActionResult App_OutDoorType(int ShiftId, string DayType)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
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
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion

        #region GetContactDetails
        [HttpGet]
        public ActionResult App_GetTotalDuration(string FromTime, string ToTime)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
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
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion    

        #region OutDoorComapnyDelete
        public ActionResult App_OutDoorComapnyDelete(string OutDoorCompanyId_Encrypted, int OutDoorCompanyId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_OutDoorCompanyId", OutDoorCompanyId);
                param.Add("@p_OutDoorCompanyId_Encrypted", OutDoorCompanyId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_OutDoorCompany", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("App_GetList", "App_Attendance_Outdoor");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion

        #region  Cancel Request Request function
        [HttpGet]
        public ActionResult App_CancelRequest(int? OutDoorCompanyId, int ddlManagerId, string Remark, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                param.Add("@p_Origin", Origin);
                param.Add("@p_DocId", OutDoorCompanyId);
                param.Add("@p_ManagerID", ddlManagerId);
                param.Add("@p_CancelRemark", Remark);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeId"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Tra_RequestCancel", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion

        #region GetLunchTime
        [HttpGet]
        public ActionResult App_GetLunchTime(int ShiftId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                DynamicParameters ParLunchTime = new DynamicParameters();
                ParLunchTime.Add("@query", @"select LEFT(Atten_Shifts.LunchTime,5) as LunchTime
                from Mas_Employee, Atten_Shifts, Atten_ShiftGroups, Atten_ShiftGroupShifts, Mas_Employee_Attendance
                where Mas_Employee.Deactivate = 0
                and  Atten_Shifts.Deactivate = 0  and  Atten_Shifts.CmpId = " + Session["CompanyId"] + " and  Atten_ShiftGroups.Deactivate = 0 and Atten_ShiftGroups.CmpID = " + Session["CompanyId"] + " and  Mas_Employee_Attendance.AttendanceEmployeeId = Mas_Employee.EmployeeId and  Mas_Employee_Attendance.EM_Atten_ShiftGroupid = Atten_ShiftGroups.ShiftGroupId and Atten_Shifts.ShiftId=" + ShiftId + "  and  Atten_ShiftGroups.ShiftGroupId = Atten_ShiftGroupShifts.Atten_ShiftGroupShifts_ShiftGroupId and  Atten_ShiftGroupShifts.Atten_ShiftGroupShifts_ShiftId = Atten_Shifts.ShiftId and  EmployeeId = " + Session["EmployeeId"] + "");
                var LunchTime = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", ParLunchTime).FirstOrDefault();
                return Json(new { data = LunchTime }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion

        #region RequestStatus
        [HttpGet]
        public ActionResult App_RequestStatus(int DocId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_DocID", DocId);
                param.Add("@p_Origin", "Atten_OutDoorCompany");
                var data = DapperORM.ExecuteSP<dynamic>("sp_RequestTimeLine", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion

        public ActionResult App_OutInvoiceView(int? DocId)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }

                param.Add("@P_Qry", "and Atten_OutDoorCompany.OutDoorCompanyId=" + DocId + "");
                var OutDoorCompanyAll = DapperORM.DynamicList("sp_List_Atten_OutDoorCompany", param);

                return Json(OutDoorCompanyAll, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
    }
}