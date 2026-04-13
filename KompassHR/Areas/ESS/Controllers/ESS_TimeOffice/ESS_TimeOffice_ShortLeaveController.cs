using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
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

namespace KompassHR.Areas.ESS.Controllers.ESS_TimeOffice
{
    public class ESS_TimeOffice_ShortLeaveController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_TimeOffice_ShortLeave

        #region ESS_TimeOffice_ShortLeave Main View
        [HttpGet]
        public ActionResult ESS_TimeOffice_ShortLeave(string ShortLeaveId_Encrypted, string ShortLeaveSettingId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 151;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {                    

                    var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Leave_ShortLeave where Deactivate=0";
                    var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    ViewBag.DocNo = DocNo;

                    DynamicParameters paramDirection = new DynamicParameters();
                    paramDirection.Add("@p_EmployeeID", Session["EmployeeId"]);
                    var GetDirection = DapperORM.ReturnList<AllDropDownBind>("sp_GetShiftDirections", paramDirection);
                    ViewBag.Direction = GetDirection;

                    param = new DynamicParameters();
                    var ShortLeaveLastDate = "select max(CreatedDate) As CreatedDate from Leave_ShortLeave where Deactivate=0";
                    var LastRecored = DapperORM.DynamicQuerySingle(ShortLeaveLastDate);
                    ViewBag.LastRecored = LastRecored.CreatedDate;

                    DynamicParameters paramPG = new DynamicParameters();
                    paramPG.Add("@query", "Select EM_Atten_ShortLeaveSettingId from Mas_Employee_Attendance where AttendanceEmployeeId=" + Session["EmployeeId"] + " and Deactivate=0");
                    var GetSLSettingId = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramPG).FirstOrDefault();

                    if (GetSLSettingId != null)
                    {
                        var SLSettingId = GetSLSettingId.EM_Atten_ShortLeaveSettingId;

                        DynamicParameters paramBackDay = new DynamicParameters();
                        paramBackDay.Add("@query", "Select BackDateDays from Leave_ShortLeaveSetting where Deactivate=0 and CmpId=" + Session["CompanyId"] + " and ShortLeaveSettingBranchId=" + Session["BranchId"] + "  and ShortLeaveSettingId=" + SLSettingId + " ");
                        var GetBackDatedDay = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramBackDay).FirstOrDefault();
                        if (GetBackDatedDay != null)
                        {
                            ViewBag.GetBackDatedDays = GetBackDatedDay.BackDateDays;
                        }
                        else
                        {
                            ViewBag.GetBackDatedDays = "";
                        }

                        //var Duration = DapperORM.DynamicQuerySingle("select  Minute from Leave_ShortLeaveSetting where Deactivate=0 and CmpID= '" + Session["CompanyId"] + "'  and ShortLeaveSettingBranchId='"+ Session["BranchId"] + "' and ShortLeaveSettingId="+ SLSettingId + "")
                        //if(Duration!=null)
                        //{
                        //    ViewBag.Duration = Duration.Minute;
                        //}
                        DynamicParameters paramTotalDuration = new DynamicParameters();
                        paramBackDay.Add("@query", "Select  Minute from  Leave_ShortLeaveSetting where Deactivate=0 and CmpID=" + Session["CompanyId"] + " and ShortLeaveSettingBranchId=" + Session["BranchId"] + "  and ShortLeaveSettingId=" + SLSettingId + "");
                        var Duration = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramBackDay).FirstOrDefault();
                        if (Duration != null)
                        {
                            ViewBag.Duration = Duration.Minute;
                        }
                        else
                        {
                            ViewBag.Duration = 0;
                        }

                        DynamicParameters paramShift = new DynamicParameters();
                    paramShift.Add("@p_EmployeeID", Session["EmployeeId"]);
                    ViewBag.GetShiftList = DapperORM.ReturnList<Atten_Shifts>("sp_GetShift", paramShift);

                    param = new DynamicParameters();
                    param.Add("@p_EmployeeID", Session["EmployeeId"]);
                    ViewBag.GetDeviceLogsList = DapperORM.ReturnList<dynamic>("sp_GetDeviceLogs", param).ToList();

                    //For Total Count
                    DynamicParameters Params = new DynamicParameters();
                    Params.Add("@query", "Select MonthlyLimit from Mas_Employee_Attendance ,Leave_ShortLeaveSetting where EM_Atten_ShortLeaveSettingID =Leave_ShortLeaveSetting.ShortLeaveSettingID  and AttendanceEmployeeId='" + Session["EmployeeID"] + "'"); // Pass EmpolyeeID
                    ViewBag.GetTotalCount = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", Params).ToList();
                  
                    //For Availed
                    DynamicParameters Params1 = new DynamicParameters();
                    Params1.Add("@query", "  select count(*) as Availed from Leave_ShortLeave where shortleaveemployeeid = '" + Session["EmployeeID"] + "'and month(ShortLeaveDate) = month(getdate()) and deactivate = 0 and Status not in ('L1- Rejected', 'Cancel')"); // Pass EmpolyeeID
                    ViewBag.GetAvailedCount = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", Params1).ToList();

                    //For Balance
                    DynamicParameters Balance = new DynamicParameters();
                    Balance.Add("@query", "Select MonthlyLimit as Balance from Mas_Employee_Attendance ,Leave_ShortLeaveSetting where EM_Atten_ShortLeaveSettingID =Leave_ShortLeaveSetting.ShortLeaveSettingID  and AttendanceEmployeeId='" + Session["EmployeeID"] + "'"); // Pass EmpolyeeID
                    ViewBag.GetBalanceCount = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", Balance).ToList();

                    DynamicParameters AdditionalList = new DynamicParameters();
                    AdditionalList.Add("@query", "Select AdditionNotifyId As Id,AdditionNotifyEmailID As Name from Mas_AdditionNotify Where Deactivate=0 and AdditionNotifyEmployeeID=" + Session["EmployeeId"] + "");
                    var listMas_AdditionalList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", AdditionalList);
                    ViewBag.GetAdditionalList = listMas_AdditionalList;

                }
                return View();
            }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        
        public ActionResult ESS_Timeoffice_GetAllRejectApprovalList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 151;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@P_Qry", "and ShortLeaveEmployeeId=" + Session["EmployeeId"] + "");
                var ShortLeaves = DapperORM.DynamicList("sp_List_Leave_ShortLeave", param);
                ViewBag.ShortLeaves = ShortLeaves;

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

        #region ISValidation
        [HttpGet]
        public ActionResult IsShortLeaveExists( DateTime ShortLeaveDate, TimeSpan FromTime, TimeSpan ToTime, int shortLeaveShiftId, string AdditionalNotifyPerson)
        {
            try
            {
                //var AdditionalNotifyPersons = AdditionalNotifyPerson.ToString().Trim().Replace("[", "");
                //var AdditionalNotify = AdditionalNotifyPersons.ToString().Trim().Replace("]", "");
                //var AdditionalNotifys = AdditionalNotify.ToString().Trim().Replace(@"""", "");
                //Session["AdditionalNotify"] = AdditionalNotifys;
                param.Add("@p_process", "IsValidation");
                param.Add("@p_ShortLeaveEmployeeId", Session["EmployeeId"]);
                param.Add("@p_ShortLeaveDate",Convert.ToDateTime(ShortLeaveDate).ToString("MM/dd/yyyy"));
                param.Add("@p_StartTime", FromTime);
                param.Add("@p_EndTime", ToTime);
                param.Add("@p_ShortLeaveShIftId", shortLeaveShiftId);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Leave_ShortLeave", param);
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
        public ActionResult SaveUpdate(Leave_ShortLeave ShortLeave)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", string.IsNullOrEmpty(ShortLeave.ShortLeaveId_Encrypted) ? "Save" : "Update");
                param.Add("@p_ShortLeaveId", ShortLeave.ShortLeaveId);
                param.Add("@p_ShortLeaveId_Encrypted", ShortLeave.ShortLeaveId_Encrypted);
                //param.Add("@p_CmpId", Session["CompanyId"]);
                //param.Add("@p_ShortLeaveEmployeeId", ShortLeave.ShortLeaveEmployeeId);
                param.Add("@p_ShortLeaveEmployeeId", Session["EmployeeId"]);
                param.Add("@p_DocNo", ShortLeave.DocNo);
                param.Add("@p_ShortLeaveDate", ShortLeave.ShortLeaveDate);
                param.Add("@p_ShortLeaveShiftId", ShortLeave.ShortLeaveShiftId);
                param.Add("@p_Direction", ShortLeave.Direction);
                param.Add("@p_StartTime", ShortLeave.StartTime);
                param.Add("@p_EndTime", ShortLeave.EndTime);
                param.Add("@p_Reason", ShortLeave.Reason);
                param.Add("@p_RequestFrom", "Web");
                param.Add("@p_AdditionNotify", Session["AdditionalNotify"]);
                param.Add("@p_Status", "Pending");
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Leave_ShortLeave", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_TimeOffice_ShortLeave", "ESS_TimeOffice_ShortLeave");
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
        public ActionResult GetContactDetails(int EmployeeId, double ShortLeaveId)
        {
            try
            {
                param.Add("@p_Origin", "SLRequest");
                param.Add("@p_DocId", ShortLeaveId);
                var GetSLlist = DapperORM.ExecuteSP<dynamic>("Sp_GetManager_Module", param).ToList(); // SP_getReportingManager
                var GetShortLeave = GetSLlist;
                return Json(new { data = GetShortLeave }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region DeleteShortLeave
        public ActionResult DeleteShortLeave(string ShortLeaveID_Encrypted)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Delete");
                param.Add("@p_ShortLeaveID_Encrypted", ShortLeaveID_Encrypted);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Leave_ShortLeave", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_Timeoffice_GetAllRejectApprovalList", "ESS_TimeOffice_ShortLeave");
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
        public ActionResult CancelRequest(int? ShortLeaveId, int ddlManagerId, string Remark, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }


                param.Add("@p_Origin", Origin);
                param.Add("@p_DocId", ShortLeaveId);
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

        #region Send Notification Popup Modal Partial View 
        [HttpGet]
        public PartialViewResult SendNotificationPopupModal()
        {
            try
            {
                var OnboardEmployeeId = Session["OnboardEmployeeId"];
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EmployeeId", OnboardEmployeeId);
                var StatusCheck = DapperORM.DynamicList("sp_List_Mas_Employee_StatusCheck", paramList);
                ViewBag.GetStatusCheckList = StatusCheck;

                return PartialView("_Onboarding_SidebarMenu");
                //return RedirectToAction("Docuinfo",rec);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}