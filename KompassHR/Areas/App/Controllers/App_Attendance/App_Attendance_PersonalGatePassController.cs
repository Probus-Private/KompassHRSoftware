using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Areas.Setting.Models.Setting_TimeOffice;
using KompassHR.Models;
using System.Data;
using System.Data.SqlClient;
using System.Net;

namespace KompassHR.Areas.App.Controllers.App_Attendance
{
    public class App_Attendance_PersonalGatePassController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/App_Attendance_PersonalGatePass
        #region App_Attendance_PersonalGatePass MAin View
        [HttpGet]
        public ActionResult App_Attendance_PersonalGatePass()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 153;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { Area = "App" });
                //}
                DynamicParameters log = new DynamicParameters();
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {

                    param = new DynamicParameters();

                    var GetDocNo = DapperORM.DynamicQuerySingle("Select Isnull(Max(DocNo),0)+1 As DocNo from Atten_PersonalGatepass");
                    var DocNo = GetDocNo != null ? GetDocNo.DocNo : null;
                    ViewBag.DocNo = DocNo;

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
                        paramTotalDuration.Add("@query", "Select  Minute from  Atten_PersonalGatepassSetting where Deactivate=0 and CmpId=" + Session["CompanyId"] + " and PersonalGatepassSettingBranchId=" + Session["BranchId"] + "  and PersonalGatepassSettingId=" + PGSettingId + "");
                        var Duration = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramTotalDuration).FirstOrDefault();
                        if (Duration != null)
                        {
                            ViewBag.Duration = Duration.Minute;
                        }
                        else
                        {
                            ViewBag.Duration = 0;
                        }
                    }

                    //param = new DynamicParameters();
                    //var OutDoorCompanyLastDate = "select max(CreatedDate) As CreatedDate from Atten_PersonalGatepass where deactivate=0";
                    //var LastRecored = DapperORM.DynamicQuerySingle(OutDoorCompanyLastDate);
                    //ViewBag.LastRecored = LastRecored.CreatedDate;


                    param = new DynamicParameters();
                    param.Add("@p_EmployeeID", Session["EmployeeId"]);
                    ViewBag.GetDeviceLogsList = DapperORM.ReturnList<dynamic>("sp_GetDeviceLogs", param).ToList();

                    DynamicParameters paramShift = new DynamicParameters();
                    paramShift.Add("@p_EmployeeID", Session["EmployeeId"]);
                    ViewBag.GetShiftList = DapperORM.ReturnList<Atten_Shifts>("sp_GetShift", paramShift);

                    //DynamicParameters AdditionalList = new DynamicParameters();
                    //AdditionalList.Add("@query", "Select AdditionNotifyId As Id,AdditionNotifyEmailID As Name from Mas_AdditionNotify Where Deactivate=0 and AdditionNotifyEmployeeID=" + Session["EmployeeId"] + "");
                    //var listMas_AdditionalList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", AdditionalList);
                    //ViewBag.GetAdditionalList = listMas_AdditionalList;

                }

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion

        #region App_Attendance_GetApproveRejectPersonalGatepassList
        public ActionResult App_Attendance_GetApproveRejectPersonalGatepassList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 153;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { Area = "App" });
                //}
                param.Add("@P_Qry", "and PersonalGatepassEmployeeId = '" + Session["EmployeeId"] + "'");
                var PersonalGatepass = DapperORM.ReturnList<dynamic>("sp_List_Atten_PersonalGatepass", param).ToList();
                ViewBag.PersonalGatepass = PersonalGatepass;

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

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Atten_PersonalGatepass AttenPersonalGetPass)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 153;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { Area = "App" });
                //}
                var CmpId = Session["CompanyId"];
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Save");
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_PersonalGatepassId", AttenPersonalGetPass.PersonalGatepassID);
                param.Add("@p_PersonalGatepassId_Encrypted", AttenPersonalGetPass.PersonalGatepassID_Encrypted);
                param.Add("@p_PersonalGatepassEmployeeId", Session["EmployeeId"]);
                param.Add("@p_PersonalGatepassBranchId", AttenPersonalGetPass.PersonalGatepassBranchId);
                param.Add("@p_PersonalGatepassShiftId", AttenPersonalGetPass.PersonalGatepassShiftId);
                param.Add("@p_PersonalGatepassShiftName", AttenPersonalGetPass.PersonalGatepassShiftName);
                //param.Add("@p_Direction", AttenPersonalGetPass.Direction);
                param.Add("@p_PersonalGatepassDate", AttenPersonalGetPass.PersonalGatepassDate);
                param.Add("@p_FromDateTime", AttenPersonalGetPass.FromTime);
                param.Add("@p_ToDateTime", AttenPersonalGetPass.ToTime);
                param.Add("@p_TotalDuration", AttenPersonalGetPass.TotalDurations);
                param.Add("@p_Reason", AttenPersonalGetPass.Reason);
                param.Add("@p_Status", "Pending");
                param.Add("@p_ApprovedAutoManually", AttenPersonalGetPass.ApprovedAutoManually);
                // param.Add("@p_AdditionNotify", Session["AdditionalNotify"]);
                param.Add("@p_RequestFrom", "Web");
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_PersonalGatepass", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                return RedirectToAction("App_Attendance_PersonalGatePass", "App_Attendance_PersonalGatePass");

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
        public ActionResult IsPersoanlGetPassExists(/*double EmployeeID,*/ DateTime PersoanlDate, DateTime FromTime, DateTime ToTime, double ShiftId, string AdditionalNotifyPerson)
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
                param.Add("@p_PersonalGatepassEmployeeId", Session["EmployeeID"]);
                param.Add("@p_PersonalGatepassShiftId", ShiftId);
                param.Add("@p_FromDateTime", FromTime);
                param.Add("@p_ToDateTime", ToTime);
                param.Add("@p_PersonalGatepassDate", PersoanlDate);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_PersonalGatepass", param);
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

        #region GetContactDetails
        [HttpGet]
        public ActionResult GetContactDetails(int EmployeeId, string PersonalGatepassId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                param.Add("@p_Origin", "PGRequest");
                param.Add("@p_DocId_Encrypted", PersonalGatepassId_Encrypted);
                var GetPGlist = DapperORM.ExecuteSP<dynamic>("Sp_GetManager_Module", param).ToList(); // SP_getReportingManager
                var GetOutDoor = GetPGlist;
                return Json(new { data = GetOutDoor }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }

        #endregion    

        #region PersonalGetPassDelete
        [HttpGet]
        public ActionResult Delete(string PersonalGatepassId_Encrypted,string PersonalGatepassId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_PersonalGatepassId_Encrypted", PersonalGatepassId_Encrypted);
               param.Add("@p_PersonalGatepassId", PersonalGatepassId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_PersonalGatepass", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("App_Attendance_GetApproveRejectPersonalGatepassList", "App_Attendance_PersonalGatePass");
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
        public ActionResult GetTotalDuration(string FromTime, string ToTime)
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

        #region  Cancel Request Request function
        [HttpGet]
        public ActionResult CancelRequest(int? PersonalGatepassId, int ddlManagerId, string Remark, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                param.Add("@p_Origin", Origin);
                param.Add("@p_DocId", PersonalGatepassId);
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

        #region RequestStatus
        [HttpGet]
        public ActionResult RequestStatus(int DocId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_DocID", DocId);
                param.Add("@p_Origin", "Atten_PersonalGatepass");
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


        #region App_PersonalGatePassDetails
        [HttpGet]
        public ActionResult App_PersonalGatePassDetails(int? PersonalGatepassId)
        {
            try
            {

                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 153;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { Area = "App" });
                //}
                param.Add("@P_Qry", "AND PersonalGatepassEmployeeId = '" + Session["EmployeeId"] + "' AND PersonalGatepassId = '" + PersonalGatepassId + "'");
                var PersonalGatepass = DapperORM.ReturnList<dynamic>("sp_List_Atten_PersonalGatepass", param).ToList();
                ViewBag.PersonalGatepass = PersonalGatepass;

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