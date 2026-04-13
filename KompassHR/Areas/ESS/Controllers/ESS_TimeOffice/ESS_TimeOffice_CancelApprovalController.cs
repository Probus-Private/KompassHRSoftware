using Dapper;
using KompassHR.Models;
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
    public class ESS_TimeOffice_CancelApprovalController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_TimeOffice_CancelApproval
        #region Main VIEW
        public ActionResult ESS_TimeOffice_CancelApproval()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : (262 == 262 || 255 == 255 ? 262 : 255);
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var ModuleId = Session["ModuleId"];

                if (Convert.ToInt32(ModuleId) == 4)
                {
                    param.Add("@p_ModuleOrigin", "Leave");
                }
                if (Convert.ToInt32(ModuleId) == 5)
                {
                    param.Add("@p_ModuleOrigin", "Attendance");
                }


                param.Add("@p_ManagerID", Session["EmployeeId"]);
                param.Add("@p_ListOrigin", "All");
                var TimeOfficeCancelApproval = DapperORM.DynamicList("sp_CancelRequestForApproval", param);
                if (TimeOfficeCancelApproval != null)
                {
                    ViewBag.TimeOfficeCancelApprovalList = TimeOfficeCancelApproval;
                }
                else
                {
                    ViewBag.TimeOfficeCancelApprovalList = "";
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

        #region  View For Cancel Approve LEAVE
        public ActionResult ViewForLeaveCancelApprover(string DocId_Encrypted, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 262;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_ManagerID", Session["EmployeeId"]);
                param.Add("@p_DocId_Encrypted", DocId_Encrypted);
                param.Add("@p_ModuleOrigin", "Leave");
                param.Add("@p_ListOrigin", Origin);
                var GetCancelApproval = DapperORM.ExecuteSP<dynamic>("sp_CancelRequestForApproval", param).FirstOrDefault();
                if (GetCancelApproval != null)
                {
                    ViewBag.CancelApprovalList = GetCancelApproval;
                }
                else
                {
                    ViewBag.CancelApprovalList = "";
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

        #region  View For Cancel Approve PUNCH MISSING
        public ActionResult ViewForCancelApprover(string DocId_Encrypted, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : (262 == 262 || 255 == 255 ? 262 : 255);
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_ManagerID", Session["EmployeeId"]);
                param.Add("@p_DocId_Encrypted", DocId_Encrypted);
                param.Add("@p_ModuleOrigin", "Attendance");
                param.Add("@p_ListOrigin", Origin);
                var GetCancelApproval = DapperORM.ExecuteSP<dynamic>("sp_CancelRequestForApproval", param).FirstOrDefault();
                if (GetCancelApproval != null)
                {
                    ViewBag.CancelApprovalList = GetCancelApproval;
                }
                else
                {
                    ViewBag.CancelApprovalList = "";
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

        #region  View For Cancel Approve PERSONAL GATEPASS
        public ActionResult ViewForPGCancelApprover(string DocId_Encrypted, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : (262 == 262 || 255 == 255 ? 262 : 255);
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_ManagerID", Session["EmployeeId"]);
                param.Add("@p_DocId_Encrypted", DocId_Encrypted);
                param.Add("@p_ModuleOrigin", "Attendance");
                param.Add("@p_ListOrigin", Origin);
                var GetCancelApproval = DapperORM.ExecuteSP<dynamic>("sp_CancelRequestForApproval", param).FirstOrDefault();
                if (GetCancelApproval != null)
                {
                    ViewBag.CancelApprovalList = GetCancelApproval;
                }
                else
                {
                    ViewBag.CancelApprovalList = "";
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

        #region  View For Cancel Approve OUTDOOR
        public ActionResult ViewForODCancelApprover(string DocId_Encrypted, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : (262 == 262 || 255 == 255 ? 262 : 255);
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_ManagerID", Session["EmployeeId"]);
                param.Add("@p_DocId_Encrypted", DocId_Encrypted);
                param.Add("@p_ModuleOrigin", "Attendance");
                param.Add("@p_ListOrigin", Origin);
                var GetCancelApproval = DapperORM.ExecuteSP<dynamic>("sp_CancelRequestForApproval", param).FirstOrDefault();
                if (GetCancelApproval != null)
                {
                    ViewBag.CancelApprovalList = GetCancelApproval;
                }
                else
                {
                    ViewBag.CancelApprovalList = "";
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

        #region  View For Cancel Approve SHORT LEAVE
        public ActionResult ViewForSLCancelApprover(string DocId_Encrypted, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : (262 == 262 || 255 == 255 ? 262 : 255);
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_ManagerID", Session["EmployeeId"]);
                param.Add("@p_DocId_Encrypted", DocId_Encrypted);
                param.Add("@p_ModuleOrigin", "Attendance");
                param.Add("@p_ListOrigin", Origin);
                var GetCancelApproval = DapperORM.ExecuteSP<dynamic>("sp_CancelRequestForApproval", param).FirstOrDefault();
                if (GetCancelApproval != null)
                {
                    ViewBag.CancelApprovalList = GetCancelApproval;
                }
                else
                {
                    ViewBag.CancelApprovalList = "";
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

        #region  View For Cancel Approve WORK FROM HOME
        public ActionResult ViewForWFHCancelApprover(string DocId_Encrypted, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : (262 == 262 || 255 == 255 ? 262 : 255);
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_ManagerID", Session["EmployeeId"]);
                param.Add("@p_DocId_Encrypted", DocId_Encrypted);
                param.Add("@p_ModuleOrigin", "Attendance");
                param.Add("@p_ListOrigin", Origin);
                var GetCancelApproval = DapperORM.ExecuteSP<dynamic>("sp_CancelRequestForApproval", param).FirstOrDefault();
                if (GetCancelApproval != null)
                {
                    ViewBag.CancelApprovalList = GetCancelApproval;
                }
                else
                {
                    ViewBag.CancelApprovalList = "";
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

        #region  View For Cancel Approve CoffRequest
        public ActionResult ViewForCoffRequestCancelApprover(string DocId_Encrypted, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : (262 == 262 || 255 == 255 ? 262 : 255);
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_ManagerID", Session["EmployeeId"]);
                param.Add("@p_DocId_Encrypted", DocId_Encrypted);
                param.Add("@p_ModuleOrigin", "Leave");
                param.Add("@p_ListOrigin", Origin);
                var GetCancelApproval = DapperORM.ExecuteSP<dynamic>("sp_CancelRequestForApproval", param).FirstOrDefault();
                if (GetCancelApproval != null)
                {
                    ViewBag.CancelApprovalList = GetCancelApproval;
                }
                else
                {
                    ViewBag.CancelApprovalList = "";
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
        public ActionResult ViewForShiftChangeRequestCancelApprover(string DocId_Encrypted, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : (262 == 262 || 255 == 255 ? 262 : 255);
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_ManagerID", Session["EmployeeId"]);
                param.Add("@p_DocId_Encrypted", DocId_Encrypted);
                param.Add("@p_ModuleOrigin", "Attendance");
                param.Add("@p_ListOrigin", Origin);
                var GetShiftChangeApproval = DapperORM.ExecuteSP<dynamic>("sp_CancelRequestForApproval", param).FirstOrDefault();
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

        #region  Manager Approved Or Reject Emoployee Received Cancel Request
        [HttpGet]
        public ActionResult RequestForCancelApprove(int? DocId, string Encrypted, string Status, string Remark, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : (262 == 262 || 255 == 255 ? 262 : 255);
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_Origin", Origin);
                param.Add("@p_DocId_Encrypted", Encrypted);
                param.Add("@p_DocId", DocId);
                param.Add("@p_ApproveRejectRemark", Remark);
                param.Add("@p_Status", Status);
                param.Add("@p_Managerid", Session["EmployeeId"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_Cancel_Approved_Rejected", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                //return RedirectToAction("Inbox", "Inbox", new { Area = "Inbox" });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ESS_Leave_CancelApproveRejectedList
        public ActionResult ESS_Leave_CancelApproveRejectedList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : (262 == 262 || 255 == 255 ? 262 : 255);
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_ManagerID", Session["EmployeeId"]);
                param.Add("@p_ModuleOrigin", "CanceledList");
                param.Add("@p_ListOrigin", "All");
                var LeaveCanceledList = DapperORM.DynamicList("sp_CancelRequestForApproval", param);
                ViewBag.LeaveCanceledList = LeaveCanceledList;
                return View();

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