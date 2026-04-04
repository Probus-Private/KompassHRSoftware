using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_ClaimReimbusement
{
    public class ESS_ClaimReimbusement_ApprovalController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: ESS/ESS_ClaimReimbusement_Approval
        #region  Main View Approval List
        public ActionResult ESS_ClaimReimbusement_Approval(int? id, int? ScreenId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 175;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                if (ScreenId != null)
                {
                    Session["ModuleId"] = id;
                    Session["ScreenId"] = ScreenId;
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), id, ScreenId, "Form", "Transation");
                    ViewBag.GetUserMenuList = GetMenuList;
                }
                else
                {
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), Convert.ToInt32(Session["ModuleId"]), Convert.ToInt32(Session["ScreenId"]), "Form", "Transation");
                    ViewBag.GetUserMenuList = GetMenuList;
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

        

        #region  View TravelClaim Print Form
        public ActionResult ViewForTravelClaimRequestApprover(string DocId_Encrypted, string Type)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 175;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_DocId_Encrypted", DocId_Encrypted);
                param.Add("@p_Origin", Type);
                var GetClaimApproval = DapperORM.ExecuteSP<dynamic>("sp_List_Claim_Profiles", param).FirstOrDefault();
                ViewBag.TravelClaimApprovalList = GetClaimApproval;
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

       
        //#region Approval Request
        //[HttpGet]
        //public ActionResult ForApprovalCandidate(int? DocId, string Encrypted, string Status, string Remark)
        //{
        //    try
        //    {
        //        DynamicParameters paramApprove = new DynamicParameters();
        //        paramApprove.Add("@p_Origin", "Recruitment");
        //        paramApprove.Add("@p_DocId_Encrypted", Encrypted);
        //        paramApprove.Add("@p_DocId", DocId);
        //        paramApprove.Add("@p_Managerid", Session["EmployeeId"]);
        //        paramApprove.Add("@p_Status", Status);
        //        paramApprove.Add("@p_ApproveRejectRemark", Remark);
        //        paramApprove.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
        //        paramApprove.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
        //        var GetApprovedResult = DapperORM.ExecuteSP<dynamic>("sp_Approved_Rejected", paramApprove);
        //        var Message = paramApprove.Get<string>("@p_msg");
        //        var Icon = paramApprove.Get<string>("@p_Icon");
        //        TempData["Message"] = Message;
        //        TempData["Icon"] = Icon.ToString();
        //        if (Message != "")
        //        {
        //            return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(true, JsonRequestBehavior.AllowGet);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        return RedirectToAction(ex.Message.ToString(), "ESS_Recruitment_ResourcesRequisition");
        //    }
        //}
        //#endregion

        #region Download Attachment

        //public ActionResult DownloadAttachment(string GeneralClaimId_Encrypted, int GeneralClaimId)
        //{
        //    if (string.IsNullOrEmpty(GeneralClaimId_Encrypted))
        //    {
        //        TempData["Message"] = "Invalid GeneralClaimId_Encrypted.";
        //        TempData["Icon"] = "error";
        //        return RedirectToAction("ESS_ClaimReimbusement_Approval");
        //        //return new HttpStatusCodeResult(400, "Invalid GeneralClaimId_Encrypted.");
        //    }

        //    try
        //    {
        //        using (var sqlcon = new SqlConnection(DapperORM.connectionString))
        //        {
        //            sqlcon.Open();

        //            // Fetch AttachmentPath
        //            var attachmentPathResult = DapperORM.DynamicQuerySingleOrDefault<dynamic>(
        //                "SELECT AttachmentPath FROM Claim_GeneralClaim WHERE GeneralClaimId_Encrypted = @GeneralClaimId_Encrypted",
        //                new { GeneralClaimId_Encrypted }
        //            );

        //            // Fetch DocInitialPath
        //            var docPathResult = DapperORM.DynamicQuerySingleOrDefault<dynamic>(
        //                "SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin = 'Claim_General'"
        //            );

        //            // Validate database query results
        //            if (attachmentPathResult == null || docPathResult == null)
        //            {
        //                TempData["Message"] = "File path information not found.";
        //                TempData["Icon"] = "error";
        //                return RedirectToAction("ESS_ClaimReimbusement_Approval");
        //                //return HttpNotFound("File path information not found.");
        //            }

        //            var attachmentPath = attachmentPathResult.AttachmentPath;
        //            var docInitialPath = docPathResult.DocInitialPath;

        //            if (string.IsNullOrEmpty(attachmentPath) || string.IsNullOrEmpty(docInitialPath))
        //            {
        //                TempData["Message"] = "Attachment path or initial path is empty.";
        //                TempData["Icon"] = "error";
        //                return RedirectToAction("ESS_ClaimReimbusement_Approval");
        //                //return HttpNotFound("Attachment path or initial path is empty.");
        //            }

        //            // Extract the drive letter from DocInitialPath (e.g., "C:\")
        //            var driveLetter = Path.GetPathRoot(docInitialPath);

        //            // Check if the drive exists
        //            if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
        //            {
        //                TempData["Message"] = $"Drive {driveLetter} does not exist.";
        //                TempData["Icon"] = "error";
        //                return RedirectToAction("ESS_ClaimReimbusement_Approval");
        //                //return HttpNotFound($"Drive {driveLetter} does not exist.");
        //            }

        //            // Construct full file path
        //            var fullPath = Path.Combine(docInitialPath, GeneralClaimId.ToString(), attachmentPath);

        //            // Check if the file exists
        //            if (!System.IO.File.Exists(fullPath))
        //            {
        //                TempData["Message"] = "File not found on the server.";
        //                TempData["Icon"] = "error";
        //                return RedirectToAction("ESS_ClaimReimbusement_Approval");
        //                //return HttpNotFound("File not found on the server.");
        //            }

        //            // Return the file for download
        //            var fileName = Path.GetFileName(fullPath);
        //            return File(fullPath, MediaTypeNames.Application.Octet, fileName);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the error (you can use a logging framework here)
        //        return new HttpStatusCodeResult(500, $"Internal server error: {ex.Message}");
        //    }
        //}

        //public FileResult DownloadAttachment(string GeneralClaimId_Encrypted, int GeneralClaimId)
        //{
        //    try
        //    {
        //        var EmployeeId = Session["EmployeeId"];
        //        if (EmployeeId != null)
        //        {

        //                var AttachmentPath = DapperORM.DynamicQuerySingle("Select AttachmentPath from Claim_GeneralClaim where GeneralClaimId_Encrypted='" + GeneralClaimId_Encrypted + "'");
        //                var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Claim_General'");

        //                var APath = GetDocPath.DocInitialPath + GeneralClaimId + '\\' + AttachmentPath.AttachmentPath;

        //                if (AttachmentPath.AttachmentPath != null)
        //                {
        //                    return File(APath, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(APath));
        //                }
        //                return null;

        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}
        #endregion

        #region ApprovedRejectedList
        public ActionResult ClaimsApprovedRejectedList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 175;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_Origin", "Approved");
                param.Add("@p_ClaimOrigin", "Claim_Travel");
                var ClaimsApprovalRejected = DapperORM.DynamicList("sp_List_ESS_Claim_Approval", param);
                ViewBag.ApprovalRejectedList = ClaimsApprovalRejected;
                return View();

            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_ClaimReimbusement_Approval");
            }
        }

        #endregion
    }
}