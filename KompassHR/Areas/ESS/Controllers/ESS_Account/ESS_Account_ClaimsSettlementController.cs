using Dapper;
using KompassHR.Areas.ESS.Models.ESS_ClaimReimbusement;
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

namespace KompassHR.Areas.ESS.Controllers.ESS_Account
{
    public class ESS_Account_ClaimsSettlementController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_Account_ClaimsSettlement
        public ActionResult ESS_Account_ClaimsSettlement()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 652;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_Origin", "Approval");
                var ResourceApproval = DapperORM.DynamicList("sp_List_ESS_Claim_Account", param);
                ViewBag.ClaimApprovalList = ResourceApproval;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ESS_ClaimReimbusement_TravelClaim", "ESS_ClaimReimbusement_TravelClaim", new { Area = "ESS" });
            }
        }

        #region  View GeneralClaim 
        public ActionResult ViewForGeneralClaimRequestApprover(string DocId_Encrypted, string Type)
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
                //param.Add("@p_DocId_Encrypted", DocId_Encrypted);
                //param.Add("@p_Origin", Type);
                //var GetClaimApproval = DapperORM.ExecuteSP<dynamic>("sp_List_Claim_Profiles", param).FirstOrDefault();
                //ViewBag.GeneralClaimApprovalList = GetClaimApproval;

                DynamicParameters MulQuery = new DynamicParameters();
                MulQuery.Add("@p_DocId_Encrypted", DocId_Encrypted);
                MulQuery.Add("@p_Origin", Type);
                using (var multi = DapperORM.DynamicMultipleResultList("sp_List_Claim_Profiles", MulQuery))
                {
                    ViewBag.GeneralClaimApprovalList = multi.Read<dynamic>().FirstOrDefault();
                    ViewBag.PreviousExpenses = multi.Read<dynamic>().ToList();
                    ViewBag.ApprovalHistory = multi.Read<dynamic>().ToList();
                }
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 652;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                //param.Add("@p_DocId_Encrypted", DocId_Encrypted);
                //param.Add("@p_Origin", Type);
                //var GetClaimApproval = DapperORM.ExecuteSP<dynamic>("sp_List_Claim_Profiles", param).FirstOrDefault();
                //ViewBag.TravelClaimApprovalList = GetClaimApproval;

                DynamicParameters MulQuery = new DynamicParameters();
                MulQuery.Add("@p_DocId_Encrypted", DocId_Encrypted);
                MulQuery.Add("@p_Origin", Type);
                using (var multi = DapperORM.DynamicMultipleResultList("sp_List_Claim_Profiles", MulQuery))
                {
                    ViewBag.TravelClaimApprovalList = multi.Read<dynamic>().FirstOrDefault();
                    ViewBag.PreviousExpenses = multi.Read<dynamic>().ToList();
                    ViewBag.ApprovalHistory = multi.Read<dynamic>().ToList();
                }
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Download Attachment

        public ActionResult DownloadAttachment(string DownloadAttachment)
        {
            try
            {
                if (string.IsNullOrEmpty(DownloadAttachment))
                {
                    return Json(new { Success = false, Message = "Invalid File.", Icon = "error" }, JsonRequestBehavior.AllowGet);
                }

                var fullPath = DownloadAttachment;
                if (!System.IO.File.Exists(fullPath))
                {
                    return Json(new { Success = false, Message = "File not found on your server.", Icon = "error" }, JsonRequestBehavior.AllowGet);
                }

                var driveLetter = Path.GetPathRoot(DownloadAttachment);
                if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
                {
                    return Json(new { Success = false, Message = $"Drive {driveLetter} does not exist.", Icon = "error" }, JsonRequestBehavior.AllowGet);
                }


                var fileName = Path.GetFileName(fullPath);
                byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
                var fileBase64 = Convert.ToBase64String(fileBytes);

                return Json(new
                {
                    Success = true,
                    FileName = fileName,
                    FileData = fileBase64,
                    ContentType = MediaTypeNames.Application.Octet
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = $"Internal server error: {ex.Message}", Icon = "error" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Disbursement
        public ActionResult Disbursement(int? DocId, string Encrypted, string Status, string Remark, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 652;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }


                if (Origin == "TravelClaim")
                {
                    DapperORM.DynamicQuerySingle("update Claim_Travel set  ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=GETDATE() ,TravelClaimAccountBy='" + Session["EmployeeId"] + "',TravelClaimAccountDate= GETDATE(),IsClaimDisbursment=1   where TravelClaimId_Encrypted='" + Encrypted + "'");
                }

                else if (Origin == "GeneralClaim")
                {
                    DapperORM.DynamicQuerySingle("update Claim_GeneralClaim set  ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=GETDATE() ,GeneralClaimAccountBy='" + Session["EmployeeId"] + "',GeneralClaimAccountDate= GETDATE(),,IsClaimDisbursment=1   where GeneralClaimId_Encrypted='" + Encrypted + "'");
                }

                TempData["Message"] = "Record updated successfully";
                TempData["Icon"] = "success";
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return null;
            }
        }
        #endregion

        #region Disbursmentlist

        public ActionResult AccountDisbursmentList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 652;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_Origin", "Approved");
                var DisbursmentList = DapperORM.DynamicList("sp_List_ESS_Claim_Account", param);
                ViewBag.DisbursmentList = DisbursmentList;
                return View();

            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_Account_ClaimsSettlement");
            }
        }
        #endregion


        #region OpenAttachment
        public ActionResult OpenAttachment(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return HttpNotFound("File path not provided");

            try
            {
                filePath = System.Net.WebUtility.UrlDecode(filePath);

                if (!System.IO.File.Exists(filePath))
                    return HttpNotFound("File not found");

                string fileName = Path.GetFileName(filePath);
                string contentType = MimeMapping.GetMimeMapping(fileName);

                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);


                Response.AddHeader("Content-Disposition", "inline; filename=" + fileName);
                return File(fileBytes, contentType);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, "Error opening file: " + ex.Message);
            }
        }
        #endregion
    }
}