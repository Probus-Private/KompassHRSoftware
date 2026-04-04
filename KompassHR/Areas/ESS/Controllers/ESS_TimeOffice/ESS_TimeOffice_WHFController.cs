using Dapper;
using KompassHR.Areas.Setting.Models.Setting_TimeOffice;
using KompassHR.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;


namespace KompassHR.Areas.ESS.Controllers.ESS_TimeOffice
{
    public class ESS_TimeOffice_WHFController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_TimeOffice_WHF
        #region ESS Work Form Home Main View
        [HttpGet]
        public ActionResult ESS_TimeOffice_WHF()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 154;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Atten_WorkFromHome";
                    var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    ViewBag.DocNo = DocNo;

                    //param = new DynamicParameters();
                    //var WFHLastDate = "select max(CreatedDate) as CreatedDate  from Atten_WorkFromHome";
                    //var LastRecored = DapperORM.DynamicQuerySingle(WFHLastDate);
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
                }
                TempData["GetMinMaxDate"] = DapperORM.DynamicQueryList(@"Select BackDateDays as MinDate,FutureDateDays as MaxDate from  Atten_WorkfromHomeSetting where Atten_WorkfromHomeSetting.Deactivate=0 and  WFHSettingBranchId=" + Session["BranchId"]+"").FirstOrDefault();
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ESS_TimeOffice_WHF_ApprovalandrejectList
        public ActionResult ESS_TimeOffice_WHF_ApprovalandrejectList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 154;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_Qry", "and WFHEmployeeId='" + Session["EmployeeId"] + "'");
                var WFHList = DapperORM.DynamicList("sp_List_Atten_WorkFromHome", param);
                ViewBag.WFHList = WFHList;

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
        public ActionResult IsWHFExists(string WHFType, DateTime FromDate, DateTime Todate, string AdditionalNotifyPerson, string ToTime, string FromTime)
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
                param.Add("@p_WFHEmployeeId", Session["EmployeeId"]);
                param.Add("@p_WFHType", WHFType);
                param.Add("@p_FromDate", FromDate);
                param.Add("@p_ToDate", Todate);
                param.Add("@p_FromTime", FromTime);
                param.Add("@p_ToTime", ToTime);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_WorkFromHome", param);
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
        public ActionResult SaveUpdate(Models.ESS_TimeOffice.Atten_WorkFromHome WorkFromHome , HttpPostedFileBase WFHFilePath)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 154;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                var CmpId = Session["CompanyId"];
                param.Add("@p_process", "Save");
                param.Add("@p_WFHId", WorkFromHome.WFHId);
                param.Add("@p_WFHId_Encrypted", WorkFromHome.WFHId_Encrypted);
                param.Add("@p_WFHEmployeeId", Session["EmployeeId"]);
                param.Add("@p_WFHBranchId", WorkFromHome.WFHIdBranchId);
                param.Add("@p_WFHType", WorkFromHome.WFHType);
                param.Add("@p_WFHShiftId", WorkFromHome.WFHShiftId);
                param.Add("@p_WFHShiftName", WorkFromHome.WFHShiftName);
                param.Add("@p_FromDate", WorkFromHome.FromDate);
                param.Add("@p_ToDate", WorkFromHome.ToDate);
                param.Add("@p_FromTime", WorkFromHome.FromTime);
                param.Add("@p_ToTime", WorkFromHome.ToTime);
                param.Add("@p_Reason", WorkFromHome.Reason);
                param.Add("@p_TotalDuration", WorkFromHome.TotalDuration);
                if (WFHFilePath!=null)
                {
                    param.Add("@p_DocumentPath", WFHFilePath == null ? "" : WFHFilePath.FileName);
                }
                param.Add("@p_AdditionNotify", Session["AdditionalNotify"]);
                param.Add("@p_RequestFrom", "Web");
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_WorkFromHome", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                if (TempData["P_Id"] != null && WorkFromHome.WFHId_Encrypted != null || WFHFilePath != null)
                {
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='WFH'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    var FirstPath = GetFirstPath + TempData["P_Id"] + "\\";// First path plus concat folder by Id
                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }
                    if (WFHFilePath != null)
                    {
                        string ImgLeaveFilePath = "";
                        ImgLeaveFilePath = FirstPath + WFHFilePath.FileName; 
                        WFHFilePath.SaveAs(ImgLeaveFilePath); 
                    }
                }
                //=========== START NOTIFICATION SEND CODE ================================
                string customerCode = Convert.ToString(Session["ESSCustomerCode"]);
                //string customerCode = "P_K1005";
                int empId = Convert.ToInt32(Session["EmployeeId"]);
                string empName = Convert.ToString(Session["EmployeeName"]);

                // Fetch data
                var Sendmessage = DapperORM.DynamicQueryList("SELECT * FROM tbl_NotificationMessage WHERE Origin='WFH'").FirstOrDefault();
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
                return RedirectToAction("ESS_TimeOffice_WHF", "ESS_TimeOffice_WHF");

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
        public ActionResult GetContactDetails(int EmployeeId, string WFHId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_Origin", "WFHRequest");
                param.Add("@p_DocId_Encrypted", WFHId_Encrypted);
                var GetWFHlist = DapperORM.ExecuteSP<dynamic>("Sp_GetManager_Module", param).ToList(); // SP_getReportingManager
                var GetWorkFormHome = GetWFHlist;
                return Json(new { data = GetWorkFormHome }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion   

        #region WorkFromHomeType
        [HttpGet]
        public ActionResult WorkFromHomeType(int ShiftId, string DayType)
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

        #region Delete
        public ActionResult Delete(string WFHId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_WFHId_Encrypted", WFHId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_WorkFromHome", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_TimeOffice_WHF_ApprovalandrejectList", "ESS_TimeOffice_WHF");
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
        public ActionResult CancelRequest(int? WFHId, int ddlManagerId, string Remark, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_Origin", Origin);
                param.Add("@p_DocId", WFHId);
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
                param.Add("@p_Origin", "Atten_WorkFromHome");
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

        #region Download Image 
        public ActionResult DownloadFile(string FilePath)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (FilePath != null)
                {
                    System.IO.File.ReadAllBytes(FilePath);
                    return File(FilePath, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(FilePath));
                }
                else
                {
                    return RedirectToAction("ESS_TimeOffice_WHF_ApprovalandrejectList", "ESS_TimeOffice_WHF", new { Area = "ESS" });
                }
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