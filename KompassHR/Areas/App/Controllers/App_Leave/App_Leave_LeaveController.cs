using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Leave;
using KompassHR.Areas.Setting.Models.Setting_TimeOffice;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.App.Controllers.App_Leave
{
    public class App_Leave_LeaveController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: App/App_Leave_Leave
        #region Main View
        public ActionResult App_Leave_Leave()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 634;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";

                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { area = "App" });
                //}
                var GetDocNo = DapperORM.DynamicQuerySingle("Select Isnull(Max(DocNo),0)+1 As DocNo from Leave_Master");
                var DocNo = GetDocNo != null ? GetDocNo.DocNo : null;
                //var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Leave_Master";
                //var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                ViewBag.DocNo = DocNo;
                
                param = new DynamicParameters();
                param.Add("@query", "select LeaveYearID as Id, cast(year(FromDate) as nvarchar(4))+'-'+cast(YEAR(ToDate) as nvarchar(4)) as Name from[dbo].[Leave_Year] where Deactivate = 0  and IsActivate=1  and CmpId='" + Session["CompanyId"] + "' order by IsDefault desc,FromDate desc");
                var LeaveYearGet = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetLeaveYear = LeaveYearGet;

                param = new DynamicParameters();
                param.Add("@query", "SELECT LeaveTypeId,LeaveTypeShortName FROM Leave_Type,Leave_Setting where Leave_Type.Deactivate = 0 and Leave_Setting.Deactivate=0 and Leave_Type.LeaveTypeId=Leave_Setting.LeaveSettingLeaveTypeId and Leave_Setting.LeaveSettingLeaveGroupId=(Select  EM_Atten_LeaveGroupID from Mas_Employee_Attendance where Deactivate=0 and AttendanceEmployeeId=" + Session["EmployeeId"] + " )");
                var GetLeaveType = DapperORM.ReturnList<leave_type>("sp_QueryExcution", param).ToList();
                ViewBag.GetLeaveType = GetLeaveType;

                param = new DynamicParameters();
                var ShortLeaveLastDate = "select max(CreatedDate) As CreatedDate from Leave_Master";
                var LastRecored = DapperORM.DynamicQuerySingle(ShortLeaveLastDate);
                ViewBag.LastRecored = LastRecored.CreatedDate;

                param = new DynamicParameters();
                param.Add("@P_Qry", "and Leave_Master.Status = 'Pending' and LeaveMasterEmployeeId=" + Session["EmployeeId"] + "");
                var LeaveList = DapperORM.DynamicList("sp_List_Leave_Master", param);
                ViewBag.LeaveList = LeaveList;

                DynamicParameters paramShift = new DynamicParameters();
                paramShift.Add("@p_EmployeeID", Session["EmployeeId"]);
                ViewBag.GetShiftList = DapperORM.ReturnList<Atten_Shifts>("sp_GetShift", paramShift);


                if (LeaveYearGet.Count != 0)
                {
                    DynamicParameters paramLeaveBal = new DynamicParameters();
                    paramLeaveBal.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramLeaveBal.Add("@p_CmpId", Session["CompanyId"]);
                    paramLeaveBal.Add("@p_FyearId", LeaveYearGet[0].Id);
                    var GetBalance = DapperORM.ExecuteSP<dynamic>("sp_GetLeaveBalance", paramLeaveBal).ToList(); // SP_getReportingManager
                    ViewBag.GetLeaveBalance = GetBalance;
                }
                else
                {
                    ViewBag.GetLeaveBalance = "";

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

        #region IsValidation

        [HttpGet]
        public ActionResult IsLeaveExists(string letLeaveMasterLeaveYearId, string LeaveMasterShiftId, string TotalDays, DateTime FormDate, DateTime Todate, string LeaveMasterDayType, int LeaveTypeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                //var a = DapperORM.DynamicQuerySingle("Select COUNT(inoutemployeeid) as inoutemployeeid from Atten_InOut where inoutemployeeid=" + Session["EmployeeId"]+" and Deactivate=0 and  (convert (date,Atten_InOut.InOutDate)  between '" + FormDate.ToString("yyyy-MM-dd") + "'  and '"+ Todate.ToString("yyyy-MM-dd") + "')").FirstOrDefault();
                //if (a.inoutemployeeid>0)
                //{
                //    var Message = "Employee Punches Found";
                //    var Icon = "error";
                //    var Punches = "PunchesFound";
                //    DynamicParameters paramLogTime = new DynamicParameters();
                //    paramLogTime.Add("@query", @"Select InOutIntime as Intime,InOutOutTime as OutTime,     convert(char(5), DATEADD(s, InOutDuration * 60, 0), 108)  as Duration from Atten_InOut where inoutemployeeid=" + Session["EmployeeId"] + " and Deactivate=0 and  (convert (date,Atten_InOut.InOutDate) between '" + FormDate.ToString("yyyy-MM-dd") + "' and '" + Todate.ToString("yyyy-MM-dd") + "')  and InOutIntime IS NOT NULL and InOutOutTime IS NOT NULL ");
                //    var Logdata = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramLogTime).ToList();
                //    return Json(new { Message, Icon, Logdata, Punches }, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                param.Add("@p_process", "IsValidation");
                param.Add("@p_LeaveMasterEmployeeId", Session["EmployeeId"]);
                param.Add("@p_FromDate", FormDate.ToString("yyyy-MM-dd"));
                param.Add("@p_ToDate", Todate.ToString("yyyy-MM-dd"));
                param.Add("@p_TotalDays", TotalDays);
                param.Add("@p_LeaveMasterLeaveTypeId", LeaveTypeId);
                param.Add("@p_LeaveMasterDayType", LeaveMasterDayType);
                param.Add("@p_LeaveMasterLeaveYearId", letLeaveMasterLeaveYearId);
                param.Add("@p_LeaveMasterShiftId", LeaveMasterShiftId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Leave_Master", param);
                var Message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                var Punches = "";
                if (Message != "")
                {
                    return Json(new { Message, Icon, Punches }, JsonRequestBehavior.AllowGet);

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
        public ActionResult SaveUpdate(Leave_Master leaveMaster, HttpPostedFileBase LeaveFilePath)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 634;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";

                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { area = "App" });
                //}
                var CmpId = Session["CompanyId"];
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", "Save");
                param.Add("@p_LeaveMasterId_Encrypted", leaveMaster.LeaveMasterId_Encrypted);
                param.Add("@p_LeaveMasterEmployeeId", EmployeeId);
                param.Add("@p_LeaveMasterShiftId", leaveMaster.LeaveMasterShiftId);
                param.Add("@p_LeaveMasterLeaveTypeId", leaveMaster.LeaveMasterLeaveTypeId);
                param.Add("@p_LeaveMasterDayType", leaveMaster.LeaveMasterDayType);
                param.Add("@p_CmpId", CmpId);
                param.Add("@p_FromDate", leaveMaster.FromDate);
                param.Add("@p_ToDate", leaveMaster.ToDate);
                param.Add("@p_LeaveMasterLeaveYearId", leaveMaster.LeaveMasterLeaveYearId);
                param.Add("@p_TotalDays", leaveMaster.TotalDays);
                param.Add("@p_Reason", leaveMaster.Reason);
                param.Add("@p_RequestFrom", "Web");
                if (leaveMaster.LeaveMasterId_Encrypted != null && LeaveFilePath == null)
                {
                    param.Add("@p_DocumentPath", Session["SelectedFile"]);
                }
                else
                {
                    param.Add("@p_DocumentPath", LeaveFilePath == null ? "" : LeaveFilePath.FileName);
                }
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                DapperORM.ExecuteReturn("sp_SUD_Leave_Master", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                if (TempData["P_Id"] != null && leaveMaster.LeaveMasterId_Encrypted != null || LeaveFilePath != null)
                {
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='LeaveMaster'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    var FirstPath = GetFirstPath + TempData["P_Id"] + "\\";// First path plus concat folder by Id
                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }

                    if (LeaveFilePath != null)
                    {
                        string ImgLeaveFilePath = "";
                        ImgLeaveFilePath = FirstPath + LeaveFilePath.FileName; //Concat Full Path and create New full Path
                        LeaveFilePath.SaveAs(ImgLeaveFilePath); // This is use for Save image in folder full path
                    }
                }
                return RedirectToAction("App_Leave_Leave", "App_Leave_Leave");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }

        #endregion

        #region GetLeaveBalance
        [HttpGet]
        public ActionResult GetLeaveBalance(int FYearId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_CmpId", Session["CompanyId"]);
                param.Add("@p_FyearId", FYearId);
                //param.Add("@p_LeaveTypeId", LeaveTypeId);
                var GetLeaveBalance = DapperORM.ExecuteSP<dynamic>("sp_GetLeaveBalance", param).ToList(); // SP_getReportingManager
                return Json(new { data = GetLeaveBalance }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion

        #region LeaveDelete
        [HttpGet]
        public ActionResult LeaveDelete(string LeaveMasterId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 634;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";

                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { area = "App" });
                //}
                param.Add("@p_process", "Delete");
                param.Add("@p_LeaveMasterId_Encrypted", LeaveMasterId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Leave_Master", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("App_GetList", "App_Leave_Leave");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }

        }
        #endregion

        #region App_GetList
        public ActionResult App_GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 634;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { area = "App" });
                //}
                param.Add("@P_Qry", "and Leave_Master.LeaveMasterEmployeeId=" + Session["EmployeeId"] + "");
                var LeaveAll = DapperORM.ExecuteSP<dynamic>("sp_List_Leave_Master", param);
                ViewBag.LeaveAll = LeaveAll;

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

        #region  Approve Leave Request function
        [HttpGet]
        public ActionResult ApproveLeaveRequest(int? DocId, string Encrypted, string Status, string Remark, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                param.Add("@p_Origin", Origin);
                param.Add("@p_DocId", DocId);
                param.Add("@p_DocId_Encrypted", Encrypted);
                param.Add("@p_ApproveRejectRemark", Remark);
                param.Add("@p_Status", Status);
                param.Add("@p_Managerid", Session["EmployeeId"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_Approved_Rejected", param);
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

        #region Leave Cancel Request Request function
        [HttpGet]
        public ActionResult CancelRequest(int? LeaveMasterId, int ddlManagerId, string Remark, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                param.Add("@p_Origin", Origin);
                param.Add("@p_DocId", LeaveMasterId);
                param.Add("@p_ManagerID", ddlManagerId);
                param.Add("@p_CancelRemark", Remark);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
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

        #region GetFutureAndBackDays
        public ActionResult GetFutureAndBackDays(int GetLeaveTypeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                param = new DynamicParameters();
                param.Add("@query", "SELECT BackDateDays ,FutureDateDays,IsDocRequired,IsAfterDays FROM Leave_Type,Leave_Setting where Leave_Type.Deactivate = 0 and Leave_Setting.Deactivate=0 and Leave_Type.LeaveTypeId=Leave_Setting.LeaveSettingLeaveTypeId and Leave_Setting.LeaveSettingLeaveGroupId=(Select  EM_Atten_LeaveGroupID from Mas_Employee_Attendance where AttendanceEmployeeId=" + Session["EmployeeId"] + ") and Leave_Setting.LeaveSettingLeaveTypeId=" + GetLeaveTypeId + "");
                var data = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion

        #region App_LeaveDetails
        [HttpGet]
        public ActionResult App_LeaveDetails(int? LeaveMasterId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 634;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { area = "App" });
                //}
                param.Add("@P_Qry", "and Leave_Master.LeaveMasterEmployeeId=" + Session["EmployeeId"] + " And LeaveMasterId=" + LeaveMasterId + " ");
                var LeaveAll = DapperORM.DynamicList("sp_List_Leave_Master", param);
                ViewBag.LeaveAll = LeaveAll;

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
                param.Add("@p_Origin", "Leave_Master");
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


        #region DownloadFile
        public ActionResult DownloadFile(string fileName)
        {
            try
            {
                if (fileName != null)
                {
                    string filePath = Path.Combine(fileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                        string afileName = Path.GetFileName(fileName);
                        return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, afileName);
                    }
                    return HttpNotFound("File not found.");
                }
                else
                {
                    return HttpNotFound("File not found.");
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion
    }
}