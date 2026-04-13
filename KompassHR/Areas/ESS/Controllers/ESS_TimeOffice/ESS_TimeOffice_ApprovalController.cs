using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_TimeOffice
{
    public class ESS_TimeOffice_ApprovalController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_TimeOffice_Approval
        #region  Main View TimeOffice List
        public ActionResult ESS_TimeOffice_Approval()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 157;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_List", "All");
                var TimeOfficeApproval = DapperORM.DynamicList("sp_List_Pending_ForApproval", param);
                //int GetCheck = TimeOfficeApproval;
                if (TimeOfficeApproval != null)
                {
                    ViewBag.TimeOfficeApprovalList = TimeOfficeApproval;
                }
                else
                {
                    ViewBag.TimeOfficeApprovalList = "";
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

        #region  View Outdoor Print Form
        public ActionResult ViewForOutdoorRequestApprover(string DocId_Encrypted, string Origin, string ApproverMode)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 157;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Session["ApproverMode"] = ApproverMode;
                param.Add("@p_DocId_Encrypted", DocId_Encrypted);
                param.Add("@p_Origin", Origin);
                var GetOutdoorApproval = DapperORM.ExecuteSP<dynamic>("sp_List_TimeOffice_Profiles", param).FirstOrDefault();
                if (GetOutdoorApproval != null)
                {
                    ViewBag.OutdoorApprovalList = GetOutdoorApproval;
                }
                else
                {
                    ViewBag.OutdoorApprovalList = "";
                }
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region  View Punch Missing Print Form
        public ActionResult ViewForPunchMissingRequestApprover(string DocId_Encrypted, string Origin, string ApproverMode)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 157;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Session["ApproverMode"] = ApproverMode;
                param.Add("@p_DocId_Encrypted", DocId_Encrypted);
                param.Add("@p_Origin", Origin);
                var GetPunchMissingApproval = DapperORM.ExecuteSP<dynamic>("sp_List_TimeOffice_Profiles", param).FirstOrDefault();
                if (GetPunchMissingApproval != null)
                {
                    ViewBag.PunchMissingApprovalList = GetPunchMissingApproval;
                }
                else
                {
                    ViewBag.PunchMissingApprovalList = null;
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

        #region  View Personal Getpass Print Form
        public ActionResult ViewForPersonalGetpassRequestApprover(string DocId_Encrypted, string Origin, string ApproverMode)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 157;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Session["ApproverMode"] = ApproverMode;
                param.Add("@p_DocId_Encrypted", DocId_Encrypted);
                param.Add("@p_Origin", Origin);
                var GetPersonalGetpassApproval = DapperORM.ExecuteSP<dynamic>("sp_List_TimeOffice_Profiles", param).FirstOrDefault();
                if (GetPersonalGetpassApproval != null)
                {
                    ViewBag.PersonalGetpassApprovalList = GetPersonalGetpassApproval;
                }
                else
                {
                    ViewBag.PersonalGetpassApprovalList = "";
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

        #region  View Work From Home Print Form
        public ActionResult ViewForWorkFromHomeRequestApprover(string DocId_Encrypted, string Origin, string ApproverMode)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 157;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Session["ApproverMode"] = ApproverMode;
                param.Add("@p_DocId_Encrypted", DocId_Encrypted);
                param.Add("@p_Origin", Origin);
                var GetWorkFromHomeApproval = DapperORM.ExecuteSP<dynamic>("sp_List_TimeOffice_Profiles", param).FirstOrDefault();
                if (GetWorkFromHomeApproval != null)
                {
                    ViewBag.WorkFromHomeApprovalList = GetWorkFromHomeApproval;
                }
                else
                {
                    ViewBag.WorkFromHomeApprovalList = "";
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

        #region  View Short Leave Print Form
        public ActionResult ViewForShortLeaveRequestApprover(string DocId_Encrypted, string Origin, string ApproverMode)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 157;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Session["ApproverMode"] = ApproverMode;
                param.Add("@p_DocId_Encrypted", DocId_Encrypted);
                param.Add("@p_Origin", Origin);
                var GetShortLeaveApproval = DapperORM.ExecuteSP<dynamic>("sp_List_TimeOffice_Profiles", param).FirstOrDefault();
                if (GetShortLeaveApproval != null)
                {
                    ViewBag.ShortLeaveApprovalList = GetShortLeaveApproval;
                }
                else
                {
                    ViewBag.ShortLeaveApprovalList = "";
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

        #region  View Shift Change Print Form
        public ActionResult ViewForShiftChangeRequestApprover(string DocId_Encrypted, string Origin, string ApproverMode)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 157;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Session["ApproverMode"] = ApproverMode;
                param.Add("@p_DocId_Encrypted", DocId_Encrypted);
                param.Add("@p_Origin", Origin);
                var GetShiftChangeApproval = DapperORM.ExecuteSP<dynamic>("sp_List_TimeOffice_Profiles", param).FirstOrDefault();
                if (GetShiftChangeApproval != null)
                {
                    ViewBag.ShiftChangeApprovalList = GetShiftChangeApproval;
                }
                else
                {
                    ViewBag.ShiftChangeApprovalList = "";
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

        #region  Approve Leave Request function
        [HttpGet]
        public ActionResult ApproveRequest(int? DocId, string Encrypted, string Status, string Remark, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 157;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var ApproverMode = Convert.ToString(Session["ApproverMode"]);
                if (ApproverMode == "Bulk")
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_Origin", Origin);
                    param.Add("@p_DocId_Encrypted", Encrypted);
                    param.Add("@p_DocId", DocId);
                    param.Add("@p_Status", Status);
                    param.Add("@p_ApproveRejectRemark", Remark);
                    param.Add("@p_Managerid", Session["EmployeeId"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data1 = DapperORM.ExecuteReturn("sp_Approved_Rejected_ForAdmin", param);
                    var message1 = param.Get<string>("@p_msg");
                    var Icon1 = param.Get<string>("@p_Icon");
                    TempData["Message"] = message1;
                    TempData["Icon"] = Icon1;
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"], ApproverMode = "Bulk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    param.Add("@p_Origin", Origin);
                    param.Add("@p_DocId_Encrypted", Encrypted);
                    param.Add("@p_DocId", DocId);
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

                    //=========== NOTIFICATION SEND CODE BELOW ================================
                    string customerCode = Convert.ToString(Session["ESSCustomerCode"]);
                    //string customerCode = "P_K1005";
                    int empId = Convert.ToInt32(Session["EmployeeId"]);
                    string empName = Convert.ToString(Session["EmployeeName"]);
                    var Sendmessage = DapperORM.DynamicQueryList($@"SELECT * FROM tbl_NotificationMessage WHERE Origin='{Origin}'").FirstOrDefault() ?? new { MessageTitle = "", MessageBody = "", Origin = "" };
                    // Get approver safely
                    var approver = DapperORM.DynamicQueryList($@"SELECT TraApproval_EmployeeId AS EmpId, TraApproval_ApproverEmployeeId AS ApproverId
                                            FROM Tra_Approval WHERE Origin='Atten_ShiftChange' AND Status='Pending' AND Deactivate=0 AND TraApproval_DocId={DocId}"
                    ).FirstOrDefault() ?? new { EmpId = 0, ApproverId = 0 };

                    // 🟢 Validate before sending
                    if (Sendmessage != null && approver.EmpId != 0)
                    {
                        // SEND FOR EMPLOYEE
                        var request = new
                        {
                            Title = $"🔔 {Sendmessage.MessageTitle}",
                            Body = $"👤 {Sendmessage.MessageBody} {empName}",
                            NotifyEmpId = Convert.ToInt32(approver.EmpId),
                            CreatedBy = empName,
                            RequestType = Sendmessage.Origin
                        };

                        string response = new NotificationService().SendNotification(customerCode, request);

                        // SEND FOR 2nd APPROVER (only if exists)
                        var getEmpName = DapperORM.DynamicQueryList($@"SELECT EmployeeName FROM Mas_Employee WHERE EmployeeId={approver.EmpId}").FirstOrDefault() ?? new { EmployeeName = "" };

                        if (!string.IsNullOrEmpty(getEmpName.EmployeeName))
                        {
                            var request2 = new
                            {
                                Title = Sendmessage.MessageTitle,
                                Body = $"{Sendmessage.MessageBody} {getEmpName.EmployeeName}",
                                NotifyEmpId = Convert.ToInt32(approver.ApproverId),
                                CreatedBy = empName,
                                RequestType = Sendmessage.Origin
                            };
                            string response2 = new NotificationService().SendNotification(customerCode, request2);
                        }
                    }

                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"], ApproverMode = "Approval" }, JsonRequestBehavior.AllowGet);

                    //string customerCode = "P_K1005";
                    //int notifyEmpId = Convert.ToInt32(Session["EmployeeId"]);
                    //var GetData = DapperORM.QuerySingle($@"Select * from tbl_NotificationMessage where Origin='SC'");
                    //var NotifySendEmpId = DapperORM.QuerySingle($@"Select TraApproval_EmployeeId EmpId,TraApproval_ApproverEmployeeId ApproverId from Tra_Approval where Origin='Atten_ShiftChange'
                    //                    and Status='Pending' and Deactivate =0 and TraApproval_DocId={DocId}");

                    //var GetEmployeeName = Convert.ToString(Session["EmployeeName"]);
                    //NotificationModel NodiModel = new NotificationModel();
                    ////SEND FOR EMPLOYEE
                    //var requestData = new
                    //{
                    //    Title = GetData.MessageTitle,
                    //    Body = "Request Approved by" + " " + GetEmployeeName,
                    //    NotifyEmpId = Convert.ToInt32(NotifySendEmpId.EmpId),
                    //    CreatedBy = GetEmployeeName,
                    //    RequestType = GetData.RequestType
                    //};
                    //// ✅ Call service
                    //NotificationService service = new NotificationService();
                    //string responseMessage = service.SendNotification(customerCode, requestData);


                    //SEND FOR 2nd Approver
                    //var GetEmpName = DapperORM.QuerySingle($@"Select EmployeeName from Mas_Employee where EmployeeId={NotifySendEmpId.EmpId}");
                    //var requestData1 = new
                    //{
                    //    Title = GetData.MessageTitle,
                    //    Body = GetData.MessageBody + " " + GetEmpName.EmployeeName,
                    //    NotifyEmpId = Convert.ToInt32(NotifySendEmpId.ApproverId),
                    //    CreatedBy = GetEmployeeName,
                    //    RequestType = GetData.RequestType
                    //};
                    //// ✅ Call service

                    //string responseMessage1 = service.SendNotification(customerCode, requestData1);

                }
                //return RedirectToAction("Inbox", "Inbox", new { Area = "Inbox" });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region  Multuple Multiple Approve Reject Request
        [HttpPost]
        public ActionResult MultipleApproveRejectRequest(List<RecordList> ObjRecordList)
        {
            try
            {
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 157;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                for (var i = 0; i < ObjRecordList.Count; i++)
                {
                    param.Add("@p_Origin", ObjRecordList[i].Origin);
                    param.Add("@p_DocId_Encrypted", ObjRecordList[i].DocID_Encrypted);
                    param.Add("@p_DocId", ObjRecordList[i].DocID);
                    param.Add("@p_Status", ObjRecordList[i].Status);
                    param.Add("@p_ApproveRejectRemark", ObjRecordList[i].RejectRemark);
                    param.Add("@p_Managerid", Session["EmployeeId"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_Approved_Rejected", param);
                    var message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    TempData["Message"] = message;
                    TempData["Icon"] = Icon;
                }
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region AttendanceApprovedRejectedList
        public ActionResult AttendanceApprovedRejectedList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 157;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_List", "ApprovedRequest");
                var TimeOfficeApproved = DapperORM.DynamicList("sp_List_Pending_ForApproval", param);
                ViewBag.AttendanceApprovalRejectedList = TimeOfficeApproved;
                return View();

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        public ActionResult GetEmployeeLog(int DocId, string Origin)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@p_DocNo", DocId);
            param.Add("@p_DocOrigin", Origin);
            var data = DapperORM.ReturnList<dynamic>("sp_GetDeviceLogs_OnApproval", param).ToList();
            string jsonData = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

    }
}