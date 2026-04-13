using Dapper;
using KompassHR.Areas.ESS.Models.ESS_ClaimReimbusement;
using KompassHR.Areas.Reports.Models;
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
    public class ESS_ClaimReimbusement_ConveyanceApprovalController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_ClaimReimbusement_ConveyanceApproval

        #region MainView
        public ActionResult ESS_ClaimReimbusement_ConveyanceApproval(MonthWiseFilter Obj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 704;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_EmployeeId", Session["EmployeeId"]);
                param1.Add("@p_BranchId", Session["BranchID"]);
                param1.Add("@p_Origin", "Approval");
                param1.Add("@p_ClaimOrigin", "Claim_Travel");
                param1.Add("@p_Date", Obj.Month);
                var ResourceApproval = DapperORM.DynamicList("sp_List_ESS_Claim_Approval", param1);
                ViewBag.ClaimApprovalList = ResourceApproval;
                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion 

        #region  View GeneralClaim 
        public ActionResult ViewForConveyanceClaimRequestApprover(string DocId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 704;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
         
                DynamicParameters MulQuery = new DynamicParameters();
                MulQuery.Add("@p_DocId_Encrypted", DocId_Encrypted);
                MulQuery.Add("@p_Origin", "TravelClaim");
                using (var multi = DapperORM.DynamicMultipleResultList("sp_List_Claim_Profiles", MulQuery))
                {
                    ViewBag.ClaimDetails = multi.Read<dynamic>().FirstOrDefault();
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

        #region Download
        public ActionResult DownloadAttachment(string DownloadAttachment)
        {
            try
            {
                if (string.IsNullOrEmpty(DownloadAttachment))
                {
                    return Json(new { Success = false, Message = "Invalid File.", Icon = "error" }, JsonRequestBehavior.AllowGet);
                }

                var fullPath = DownloadAttachment;
                //if (!System.IO.File.Exists(fullPath))
                //{
                //    return Json(new { Success = false, Message = "File not found on your server.", Icon = "error" }, JsonRequestBehavior.AllowGet);
                //}

                var driveLetter = Path.GetPathRoot(DownloadAttachment);
                if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
                {
                    return Json(new { Success = false, Message = $"Drive {driveLetter} does not exist.", Icon = "error" }, JsonRequestBehavior.AllowGet);
                }

                var fileName = Path.GetFileName(fullPath);
                byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
                var fileBase64 = Convert.ToBase64String(fileBytes);

                //return Json(new { Success = true, FileName = fileName, FileData = fileBase64, ContentType = MediaTypeNames.Application.Octet }, JsonRequestBehavior.AllowGet);
                var jsonResult = Json(new
                {
                    Success = true,
                    FileName = fileName,
                    FileData = fileBase64,
                    ContentType = MediaTypeNames.Application.Octet
                });

                jsonResult.MaxJsonLength = int.MaxValue;   // This line fixes large files

                return jsonResult;


            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = $"Internal server error: {ex.Message}", Icon = "error" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region  Approve 
        [HttpGet]
        public ActionResult ApproveClaimRequest(int? DocId, string Encrypted, string Status, string Remark, string Origin, int? ApproveAmount)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 704;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                //return RedirectToAction("Inbox", "Inbox", new { Area = "Inbox" });
                param.Add("@p_Origin", Origin);
                param.Add("@p_DocId_Encrypted", Encrypted);
                param.Add("@p_ApproveRejectRemark", Remark);
                param.Add("@p_DocId", DocId);
                param.Add("@p_Status", Status);
                param.Add("@p_ApproveAmount", ApproveAmount);
                param.Add("@p_Managerid", Session["EmployeeId"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_Approved_Rejected", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                //return RedirectToAction("Inbox", "Inbox", new { Area = "Inbox" });
            }
            catch (Exception ex)
            {
                return null;
            }
        }
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 704;
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