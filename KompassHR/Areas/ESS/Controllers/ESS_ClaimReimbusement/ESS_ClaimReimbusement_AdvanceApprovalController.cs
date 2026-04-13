using Dapper;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_ClaimReimbusement
{
    public class ESS_ClaimReimbusement_AdvanceApprovalController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        #region Main View 
        // GET: ESS/ESS_ClaimReimbusement_AdvanceApproval
        public ActionResult ESS_ClaimReimbusement_AdvanceApproval(MonthWiseFilter Obj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 705;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();

                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_BranchId", Session["BranchID"]);
                param.Add("@p_CompanyId", Session["CompanyId"]);
                param.Add("@p_Origin", "Approval");
                param.Add("@p_Date", Obj.Month);
                var ResourceApproval = DapperORM.DynamicList("sp_List_ESS_AdvanceClaim_Approval", param);
                ViewBag.ClaimApprovalList = ResourceApproval;
                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        #endregion


        #region  View AdvanceClaim 
        public ActionResult ViewForAdvanceClaimRequestApprover(string DocId_Encrypted)
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
                MulQuery.Add("@p_Origin", "AdvanceClaim");
                //MulQuery.Add("@p_DocId", DocId);
                using (var multi = DapperORM.DynamicMultipleResultList("sp_List_AdvanceClaim_Profiles", MulQuery))
                {
                    ViewBag.ClaimDetails = multi.Read<dynamic>().FirstOrDefault();
                    ViewBag.AdvanceDetails = multi.Read<dynamic>().ToList();

                    //ViewBag.PreviousExpenses = multi.Read<dynamic>().ToList();
                    //ViewBag.ApprovalHistory = multi.Read<dynamic>().ToList();
                }
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
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
                param.Add("@p_BranchId", Session["BranchID"]);
                param.Add("@p_CompanyId", Session["CompanyId"]);
                param.Add("@p_Origin", "Approved");
              
                var ClaimsApprovalRejected = DapperORM.DynamicList("sp_List_ESS_AdvanceClaim_Approval", param);
                ViewBag.ApprovalRejectedList = ClaimsApprovalRejected;
                return View();

            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_ClaimReimbusement_AdvanceApproval");
            }
        }

        #endregion
    }
}