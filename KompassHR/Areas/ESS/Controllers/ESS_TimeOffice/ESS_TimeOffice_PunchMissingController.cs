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

namespace KompassHR.Areas.ESS.Controllers.ESS_TimeOffice
{
    public class ESS_TimeOffice_PunchMissingController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_TimeOffice_PunchMissing
        #region ESS_TimeOffice_PunchMissing Main View
        [HttpGet]
        public ActionResult ESS_TimeOffice_PunchMissing(string PunchMissingId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 155;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    //sneha code
                    
                    var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Atten_PunchMissing";
                    var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    ViewBag.DocNo = DocNo;

                    DynamicParameters paramPM = new DynamicParameters();
                    paramPM.Add("@query", "Select EM_Atten_Atten_PunchMissingSettingId from Mas_Employee_Attendance where AttendanceEmployeeId=" + Session["EmployeeId"] + " and Deactivate=0");
                    var GetPMSettingId = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramPM).FirstOrDefault();
                    

                    //var GetBackDatedDays = DapperORM.DynamicQuerySingle("Select BackDateDays from Atten_PunchMissingSetting where Deactivate=0  and PunchMissingSettingId=" + PMSettingId + " and CmpId=" + Session["CompanyId"] + " and PunchMissingSettingBranchId=" + Session["BranchId"] + ""); //
                    if (GetPMSettingId != null)
                    {
                        var PMSettingId = GetPMSettingId.EM_Atten_Atten_PunchMissingSettingId;

                        DynamicParameters paramBackDay = new DynamicParameters();
                        paramBackDay.Add("@query", "Select BackDateDays from Atten_PunchMissingSetting where Deactivate=0 and CmpId=" + Session["CompanyId"] + " and PunchMissingSettingBranchId=" + Session["BranchId"] + "  and PunchMissingSettingId=" + PMSettingId + " ");
                        var GetBackDatedDay = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramBackDay).FirstOrDefault();
                        if (GetBackDatedDay != null)
                        {
                            ViewBag.GetBackDatedDays = GetBackDatedDay.BackDateDays;
                        }
                        else
                        {
                            ViewBag.GetBackDatedDays = "";
                        }
                    }
                    else
                    {
                        ViewBag.GetBackDatedDays = "";
                    }
                  

                    //Ganesh
                    //var GetDate = "Select TOP 1 * from Leave_ShortLeave Where ShortLeaveEmployeeID = 20 order by 1 desc";
                    //var Date = DapperORM.DynamicQuerySingle(GetDate);
                    //ViewBag.GetLastDate = Date;
                    //ganesh code
                    //var GetDocNo = "select Max(DocNo)+1 As DocNo from Atten_PunchMissing";
                    //var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    //ViewBag.DocNo = DocNo;

                    //param = new DynamicParameters();
                    //param.Add("@p_EmployeeID", "20");
                    //ViewBag.GetShift = DapperORM.ReturnList<Atten_Shifts>("sp_GetShift", param).ToList();

                    //Sneha Code
                    //var GetCMPID = DapperORM.DynamicQuerySingle("Select CmpID from Mas_Employee where EmployeeId=20 and Deactivate=0").FirstOrDefault();
                    //ViewBag.CMPID = GetCMPID.CmpID;

                    //param.Add("@query", "Select * from Atten_PunchMissingSetting where CmpId=1 and PunchMissingSettingBranchId=10 and Deactivate=0");
                    //var WHFList = DapperORM.DynamicList("sp_List_Atten_PunchMissing", param);
                    //ViewBag.WHFList = WHFList;
                    //param = new DynamicParameters();
                    //var PunchMissingLastDay = "select max(CreatedDate) As CreatedDate from Atten_PunchMissing where Deactivate=0";
                    //var LastRecored = DapperORM.DynamicQuerySingle(PunchMissingLastDay);
                    //ViewBag.LastRecored = LastRecored.CreatedDate;

                    DynamicParameters paramShift = new DynamicParameters();
                    paramShift.Add("@p_EmployeeID", Session["EmployeeId"]);
                    ViewBag.GetShiftList = DapperORM.ReturnList<Atten_Shifts>("sp_GetShift", paramShift);

                    DynamicParameters AdditionalList = new DynamicParameters();
                    AdditionalList.Add("@query", "Select AdditionNotifyId As Id,AdditionNotifyEmailID As Name from Mas_AdditionNotify Where Deactivate=0 and AdditionNotifyEmployeeID=" + Session["EmployeeId"] + "");
                    var listMas_AdditionalList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", AdditionalList);
                    ViewBag.GetAdditionalList = listMas_AdditionalList;

                    param = new DynamicParameters();
                    param.Add("@p_EmployeeID", Session["EmployeeId"]);
                    ViewBag.GetDeviceLogsList = DapperORM.ReturnList<dynamic>("sp_GetDeviceLogs", param).ToList();

                    if (PunchMissingId_Encrypted != null)
                    {
                        param.Add("@query", "Select * from Atten_PunchMissing where Deactivate=0 and PunchMissingEmployeeId="+ Session["EmployeeId"] + " and PunchMissingId_Encrypted='" + PunchMissingId_Encrypted + "'");
                        var add = DapperORM.ReturnList<Models.ESS_TimeOffice.Atten_PunchMissing>("sp_QueryExcution", param).FirstOrDefault();

                        ViewBag.AtdDate = Convert.ToDateTime(add.PunchMissingAttendanceDate).ToString("dd/MMM/yyy");
                        ViewBag.AtdDateLog = Convert.ToDateTime(add.PunchMissingLogDate).ToString("dd/MMM/yyy");
                        return View(add);
                    }
                    else
                    {
                        return View();
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

        #region ESS_TimeOffice_GetPunchMissingApprovalpendingList
        public ActionResult ESS_TimeOffice_GetPunchMissingApprovalpendingList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 155;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@P_Qry", "and PunchMissingEmployeeId = '" + Session["EmployeeId"] + "'");
                var PendingList = DapperORM.DynamicList("sp_List_Atten_PunchMissing", param);
                ViewBag.PendingList = PendingList;

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

        #region IsValidation
        [HttpGet]
        //public ActionResult IsPunchMissingExists(DateTime PunchmissingDate, string AdditionalNotifyPerson ,string PunchMissingID_Encrypted, string PunchMissingInOut)
        public ActionResult IsPunchMissingExists(int ddlShift, string PunchMissingInOut, DateTime PunchMissingAttendanceDate, DateTime PunchMissingLogDate, TimeSpan PunchMissingLogTime)
        {
            try
            {
                //var AdditionalNotifyPersons = AdditionalNotifyPerson.ToString().Trim().Replace("[", "");
                //var AdditionalNotify = AdditionalNotifyPersons.ToString().Trim().Replace("]", "");
                //var AdditionalNotifys = AdditionalNotify.ToString().Trim().Replace(@"""", "");
                //Session["AdditionalNotify"] = AdditionalNotifys;
                //param.Add("@p_PunchMissingId_Encrypted", PunchMissingID_Encrypted);
                param.Add("@p_process", "IsValidation");
                param.Add("@p_PunchMissingShiftId", ddlShift);
                param.Add("@p_PunchMissingInOut", PunchMissingInOut);
                param.Add("@p_PunchMissingAttendanceDate", Convert.ToDateTime(PunchMissingAttendanceDate).ToString("yyyy/MM/dd"));
                param.Add("@p_PunchMissingLogDate", Convert.ToDateTime(PunchMissingLogDate).ToString("yyyy/MM/dd"));
                param.Add("@p_PunchMissingLogTime", PunchMissingLogTime);
                param.Add("@p_PunchMissingEmployeeId", Session["EmployeeId"]);

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_PunchMissing", param);
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
        public ActionResult SaveUpdate(Models.ESS_TimeOffice.Atten_PunchMissing InOut)
        {
            try
            {
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 155;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "Save");
                param.Add("@P_PunchMissingEmployeeId", Session["EmployeeId"]);
                param.Add("@P_DocNo", InOut.DocNo);
                param.Add("@P_PunchMissingAttendanceDate", InOut.PunchMissingAttendanceDate);
                param.Add("@P_PunchMissingLogDate", InOut.PunchMissingLogDate);
                param.Add("@P_PunchMissingLogTime", InOut.PunchMissingLogTime);
                param.Add("@P_PunchMissingInOut", InOut.PunchMissingInOut);
                param.Add("@P_PunchMissingShiftId", InOut.PunchMissingShiftId);
                param.Add("@P_Reason", InOut.Reason);
                param.Add("@P_RequestFrom", "Web");
                param.Add("@P_AdditionNotify", Session["AdditionalNotify"]);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Atten_PunchMissing", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                //=========== START NOTIFICATION SEND CODE ================================
                string customerCode = Convert.ToString(Session["ESSCustomerCode"]);
                //string customerCode = "P_K1005";
                int empId = Convert.ToInt32(Session["EmployeeId"]);
                string empName = Convert.ToString(Session["EmployeeName"]);

                // Fetch data
                var Sendmessage = DapperORM.DynamicQueryList("SELECT * FROM tbl_NotificationMessage WHERE Origin='PM'").FirstOrDefault();
                var approver = DapperORM.DynamicQueryList($@"SELECT ReportingManager1 AS EmpId FROM Mas_Employee_Reporting WHERE ReportingModuleId=1 AND Deactivate=0 AND ApproverLevel=1 AND ReportingEmployeeID={empId}").FirstOrDefault();
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
                    string response = new NotificationService().SendNotification(customerCode, request);
                }
               
                //=========== END NOTIFICATION SEND CODE ================================
                return RedirectToAction("ESS_TimeOffice_PunchMissing", "ESS_TimeOffice_PunchMissing");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region GetContactDetails
        public ActionResult GetContactDetails(int EmployeeId, string PunchMissingId_Encrypted)
        {
            try
            {
                param.Add("@p_Origin", "PMRequest");
                param.Add("@p_DocId_Encrypted", PunchMissingId_Encrypted);
                var GetPMList = DapperORM.ExecuteSP<dynamic>("Sp_GetManager_Module", param).ToList(); // SP_getReportingManager
                var GetPunchMissing = GetPMList;
                return Json(new { data = GetPunchMissing }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetShiftFlag
        public ActionResult GetShiftFlag(int ShiftId)
        {
            try
            {
                var GetData = DapperORM.DynamicQuerySingle("select ShiftFlag from atten_shifts where deactivate=0 and ShiftId="+ ShiftId + "");
                var GetShiftFlag = GetData.ShiftFlag;
                return Json(new { data = GetShiftFlag }, JsonRequestBehavior.AllowGet);
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
        public ActionResult Delete(string PunchMissingId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@P_PunchMissingId_Encrypted", PunchMissingId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_PunchMissing", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_TimeOffice_PunchMissing", "ESS_TimeOffice_PunchMissing");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete1
        [HttpGet]
        public ActionResult DeleteList(string PunchMissingId_Encrypted , int PunchMissingId)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@P_PunchMissingId_Encrypted", PunchMissingId_Encrypted);
                param.Add("@p_PunchMissingId", PunchMissingId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_PunchMissing", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_TimeOffice_GetPunchMissingApprovalpendingList", "ESS_TimeOffice_PunchMissing");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GET_ShiftTiming
        [HttpGet]
        public ActionResult GET_ShiftTiming(int? PunchMissingShiftId, string PunchMissingLogTime)
        {
            try
            {
                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_ShifId", PunchMissingShiftId);
                param2.Add("@p_DayType", "Shift");
                var GetTime = DapperORM.ReturnList<dynamic>("sp_GetShiftTime", param2).ToList();
                return Json(GetTime, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
           
        }
        #endregion

        #region GET_ShiftTimingIn
        [HttpGet]
        public ActionResult GET_ShiftTimingIn(string PunchMissingLogTime, string PunchMissingInOut, bool AllowBeforeStartShift, bool AllowAfterEndShift, string Start, string End)
        {
            try
            {
                var Shiftstart = Start;
                var ShiftEnd = End;
                DateTime selectedintime = Convert.ToDateTime(PunchMissingLogTime);
                DateTime ShiftStartTime = Convert.ToDateTime(Shiftstart);
                DateTime to = Convert.ToDateTime(ShiftEnd);
                var Allowistruebefore = AllowBeforeStartShift;
                var AllowistrueAfter = AllowAfterEndShift;
                var type = PunchMissingInOut;
                if (type == "In")
                {
                    if (Allowistruebefore == true)
                    {
                        if (selectedintime < ShiftStartTime)
                        {
                            var Allowin = "BY";
                            return Json(Allowin, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var Allowin = "BN";
                            return Json(Allowin, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        if (selectedintime < ShiftStartTime)
                        {
                            var Allowin = "BDN";
                            return Json(Allowin, JsonRequestBehavior.AllowGet);
                        }
                        if (selectedintime > to)
                        {
                            var Allowin = "BDNTO";
                            return Json(Allowin, JsonRequestBehavior.AllowGet);
                        }

                        else
                        {
                            var Allowin = "";
                            return Json(Allowin, JsonRequestBehavior.AllowGet);
                        }
                    }
                }


                if (type == "Out")
                {
                    if (AllowistrueAfter == true)
                    {
                        if (selectedintime <= to)
                        {
                            var Allowin = "AY";
                            return Json(Allowin, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var Allowin = "AN";
                            return Json(Allowin, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        if (selectedintime > to)
                        {
                            var Allowin = "ADN";
                            return Json(Allowin, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var Allowin = "";
                            return Json(Allowin, JsonRequestBehavior.AllowGet);

                        }
                    }
                }
                else
                {
                    var Allowin = "Select";
                    return Json(Allowin, JsonRequestBehavior.AllowGet);
                }
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
        public ActionResult CancelRequest(int? PunchMissingId, int ddlManagerId, string Remark, string Origin)
        {
            try
            {

                param.Add("@p_Origin", Origin);
                param.Add("@p_DocId", PunchMissingId);
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
                param.Add("@p_Origin", "Atten_PunchMissing");
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
    }
}