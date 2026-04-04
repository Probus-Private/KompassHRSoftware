using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Areas.Setting.Models.Setting_TimeOffice;
//using KompassHR.Areas.Requisition.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_TimeOffice
{
    public class ESS_TimeOffice_PersonalGatePassController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_TimeOffice_PersonalGatePass
        #region ESS_TimeOffice_PersonalGatePass MAin View
        [HttpGet]
        public ActionResult ESS_TimeOffice_PersonalGatePass()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 153;
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
                    //var TotalDuration = "Select  Minute from  Atten_PersonalGatepassSetting";
                    //var Duration = DapperORM.DynamicQuerySingle(TotalDuration);
                    //if (Duration != null)
                    //{
                    //    ViewBag.Duration = Duration.Minute;
                    //}
                    //else
                    //{
                    //    ViewBag.Duration = 0;
                    //}


                    var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Atten_PersonalGatepass";
                    var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    ViewBag.DocNo = DocNo;

                    //var BackDatedDays = "Select BackDateDays from Atten_PersonalGatepassSetting where Deactivate=0 and CmpId=" + Session["CompanyId"] + "";
                    //var GetBackDatedDays = DapperORM.DynamicQuerySingle(BackDatedDays);

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
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ESS_TimeOffice_GetApproveRejectPersOnalGatepassList
        public ActionResult ESS_TimeOffice_GetApproveRejectPersOnalGatepassList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 153;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_Qry", "and PersonalGatepassEmployeeId = '" + Session["EmployeeId"] + "'");
                var PersoanlGetpass = DapperORM.DynamicList("sp_List_Atten_PersonalGatepass", param);
                ViewBag.PersoanlGetpass = PersoanlGetpass;

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
        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Atten_PersonalGatepass AttenPersonalGetPass)
        {
            try
            {
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 153;
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

                //=========== START NOTIFICATION SEND CODE ================================
                string customerCode = Convert.ToString(Session["ESSCustomerCode"]);
                //string customerCode = "P_K1005";
                int empId = Convert.ToInt32(Session["EmployeeId"]);
                string empName = Convert.ToString(Session["EmployeeName"]);
                // Fetch data
                var Sendmessage = DapperORM.DynamicQueryList("SELECT * FROM tbl_NotificationMessage WHERE Origin='PG'").FirstOrDefault();
                var approver = DapperORM.DynamicQueryList($@"SELECT ReportingManager1 AS EmpId FROM Mas_Employee_Reporting WHERE ReportingModuleId=1 AND Deactivate=0 AND ApproverLevel=1 AND ReportingEmployeeID={empId}").FirstOrDefault();
                // Build request model
                if (Sendmessage != null && approver != null)
                {
                    var request = new
                    {
                        Title = $"🔔 {Sendmessage.MessageTitle}",
                        Body = $"👤 {Sendmessage.MessageBody} {empName}",
                        NotifyEmpId = Convert.ToInt32(approver.EmpId),
                        CreatedBy = empName,
                        RequestType = Sendmessage.Origin
                    };
                    // Send notification
                    string response = new NotificationService().SendNotification(customerCode, request);
                }
                //=========== END NOTIFICATION SEND CODE ================================

                //return Json(new { success = true, message = responseMessage });
                return RedirectToAction("ESS_TimeOffice_PersonalGatePass", "ESS_TimeOffice_PersonalGatePass");

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
        public ActionResult IsPersoanlGetPassExists(/*double EmployeeID,*/ DateTime PersoanlDate, DateTime FromTime, DateTime ToTime, double ShiftId, string AdditionalNotifyPerson)
        {
            try
            {
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
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region GetContactDetails
        [HttpGet]
        public ActionResult GetContactDetails(int EmployeeId, string PersonalGatepassId_Encrypted)
        {
            try
            {
                param.Add("@p_Origin", "PGRequest");
                param.Add("@p_DocId_Encrypted", PersonalGatepassId_Encrypted);
                var GetPGlist = DapperORM.ExecuteSP<dynamic>("Sp_GetManager_Module", param).ToList(); // SP_getReportingManager
                var GetOutDoor = GetPGlist;
                return Json(new { data = GetOutDoor }, JsonRequestBehavior.AllowGet);
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
        public ActionResult PersonalGetPassDelete(string PersonalGatepassId_Encrypted, int PersonalGatepassId)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_PersonalGatepassId_Encrypted", PersonalGatepassId_Encrypted);
                param.Add("@p_PersonalGatepassId", PersonalGatepassId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_PersonalGatepass", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_TimeOffice_GetApproveRejectPersOnalGatepassList", "ESS_TimeOffice_PersonalGatePass");
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
        public ActionResult GetTotalDuration(string FromTime, string ToTime)
        {
            try
            {
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

        #region  Cancel Request Request function
        [HttpGet]
        public ActionResult CancelRequest(int? PersonalGatepassId, int ddlManagerId, string Remark, string Origin)
        {
            try
            {

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
                return RedirectToAction("ErrorPage", "Login");
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
                    return RedirectToAction("Login", "Login", new { Area = "" });
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