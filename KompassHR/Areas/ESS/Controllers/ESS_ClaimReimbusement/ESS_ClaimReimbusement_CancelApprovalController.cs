using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_ClaimReimbusement
{
    public class ESS_ClaimReimbusement_CancelApprovalController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_ClaimReimbusement_CancelApproval
        #region Main View
        public ActionResult ESS_ClaimReimbusement_CancelApproval()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 488;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_ModuleOrigin", "Claim");
                param.Add("@p_ManagerID", Session["EmployeeId"]);
                param.Add("@p_ListOrigin", "All");
                var ClaimCancelApproval = DapperORM.DynamicList("sp_ClaimCancelRequestForApproval", param);
                if (ClaimCancelApproval != null)
                {
                    ViewBag.ClaimCancelApprovalList = ClaimCancelApproval;
                }
                else
                {
                    ViewBag.ClaimCancelApprovalList = "";
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

        #region ESS_Leave_CancelApproveRejectedList
        public ActionResult ESS_Claim_CancelApproveRejectedList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 488;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }


                param.Add("@p_ManagerID", Session["EmployeeId"]);
                param.Add("@p_ModuleOrigin", "ClaimCanceledList");
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

        #region  View For Cancel Approve Tarvel Claim
        public ActionResult ViewForClaimCancelApprover(string DocId_Encrypted, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 488;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_ManagerID", Session["EmployeeId"]);
                param.Add("@p_DocId_Encrypted", DocId_Encrypted);
                param.Add("@p_ModuleOrigin", "Claim");
                param.Add("@p_ListOrigin", Origin);
                var GetCancelApproval = DapperORM.ExecuteSP<dynamic>("sp_ClaimCancelRequestForApproval", param).FirstOrDefault();
                if (GetCancelApproval != null)
                {
                    ViewBag.ClaimCancelApprovalList = GetCancelApproval;
                }
                else
                {
                    ViewBag.ClaimCancelApprovalList = "";
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

        #region  View For Cancel Approve General Claim
        public ActionResult ViewForGeneralClaimCancelApprover(string DocId_Encrypted, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 488;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_ManagerID", Session["EmployeeId"]);
                param.Add("@p_DocId_Encrypted", DocId_Encrypted);
                param.Add("@p_ModuleOrigin", "Claim");
                param.Add("@p_ListOrigin", Origin);
                var GetCancelApproval = DapperORM.ExecuteSP<dynamic>("sp_ClaimCancelRequestForApproval", param).FirstOrDefault();
                if (GetCancelApproval != null)
                {
                    ViewBag.GeneralClaimCancelApprovalList = GetCancelApproval;
                }
                else
                {
                    ViewBag.ClaimCancelApprovalList = "";
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 488;
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
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region When Cancel Approval Download
        public ActionResult DownloadAttachment(string DownloadAttachment)
        {
            try
            {
                if (string.IsNullOrEmpty(DownloadAttachment))
                {
                    TempData["Message"] = "Invalid File.";
                    TempData["Icon"] = "error";
                    return Json(new { success = true, message = TempData["Message"], icon = TempData["Icon"] });
                }

                if (DownloadAttachment == null)
                {
                    TempData["Message"] = "File path information not found.";
                    TempData["Icon"] = "error";
                    return Json(new { success = true, message = TempData["Message"], icon = TempData["Icon"] });
                }

                var driveLetter = Path.GetPathRoot(DownloadAttachment);
                // Check if the drive exists
                if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
                {
                    TempData["Message"] = $"Drive {driveLetter} does not exist.";
                    TempData["Icon"] = "error";
                    return Json(new { success = true, message = TempData["Message"], icon = TempData["Icon"] });
                }
                // Construct full file path
                var fullPath = DownloadAttachment;

                // Check if the file exists
                if (!System.IO.File.Exists(fullPath))
                {
                    TempData["Message"] = "File not found on your server.";
                    TempData["Icon"] = "error";
                    return Json(new { success = true, message = TempData["Message"], icon = TempData["Icon"] });
                }
                // Return the file for download
                var fileName = Path.GetFileName(fullPath);
                return File(fullPath, MediaTypeNames.Application.Octet, fileName);
            }
            catch (Exception ex)
            {
                // Log the error (you can use a logging framework here)
                return new HttpStatusCodeResult(500, $"Internal server error: {ex.Message}");
            }
        }
        #endregion
    }
}