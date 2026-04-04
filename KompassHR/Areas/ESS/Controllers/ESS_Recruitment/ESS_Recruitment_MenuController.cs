using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Recruitment
{
    public class ESS_Recruitment_MenuController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: ESS/ESS_Recruitment_Menu
        public ActionResult ESS_Recruitment_Menu(int? id, int? ScreenId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 126;
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

                var GetApprovals = DapperORM.DynamicQueryList("Select Count(*) as Approvals from Tra_Approval where Tra_Approval.Deactivate=0 and TraApproval_ApproverEmployeeId=" + Session["EmployeeId"] + "and Status='Pending' and Origin='ResourceRequest'").FirstOrDefault();
                TempData["GetApprovals"] = GetApprovals?.Approvals;

                var GetRequisitions = DapperORM.DynamicQueryList("Select Count(*) as Requisitions from Recruitment_ResourceRequest where Recruitment_ResourceRequest.Deactivate=0 and ResourceEmployeeId=" + Session["EmployeeId"] + "").FirstOrDefault();
                TempData["GetRequisitions"] = GetRequisitions?.Requisitions;

                var GetInterview = DapperORM.DynamicQueryList("Select Count(*) as interview from Recruitment_interview where Recruitment_interview.Deactivate=0 and InterviewResourceEmployeeId=" + Session["EmployeeId"] + "and InterviewAccept=0 and InterviewStatus='Pending'").FirstOrDefault();
                TempData["GetInterview"] = GetInterview?.interview;

                var GetCandidateDatabase = DapperORM.DynamicQueryList("Select Count(*) as CandidateDatabase from Recruitment_Resume  where ResumeStatus<>'Rejected'").FirstOrDefault();
                TempData["GetCandidateDatabase"] = GetCandidateDatabase?.CandidateDatabase;

                var GetOpenPosition = DapperORM.DynamicQueryList("Select Count(*) as OpenPosition from Recruitment_ResourceRequest where Recruitment_ResourceRequest.Deactivate=0 and CmpId="+Session["CompanyId"]+"").FirstOrDefault();
                TempData["GetOpenPosition"] = GetOpenPosition?.OpenPosition;

                var GetPendingCandidate = DapperORM.DynamicQueryList("Select Count(Recruitment_Resume.ResumeId) as PendingCandidate from Recruitment_Resume ,Recruitment_ResourceRequest where Recruitment_ResourceRequest.Deactivate=0 and Recruitment_ResourceRequest.ResourceId=Recruitment_Resume.Resume_ResourceId and Recruitment_Resume.InterviewId=0 and Recruitment_Resume.PreboardingId=0 and Recruitment_Resume.SendForShortListed=1 and Recruitment_Resume.ResumeStatus='Pending' and Recruitment_ResourceRequest.ResourceEmployeeId=" + Session["EmployeeId"] + "").FirstOrDefault();
                TempData["GetPendingCandidate"] = GetPendingCandidate?.PendingCandidate;

                var GetRequestPool = DapperORM.DynamicQueryList("Select Count(*) as RequestPool from Recruitment_ResourceRequest where Recruitment_ResourceRequest.Deactivate=0 and ReuestPoolRecruiterId=" + Session["EmployeeId"] + " and Status in  ('L1- Approved','L2- Approved','L1- Rejected','L2- Rejected') and Recruitment_ResourceRequest.ResourceRequestStatus<>'Closed'").FirstOrDefault();
                TempData["GetRequestPool"] = GetRequestPool?.RequestPool;

                //param.Add("@p_EmployeeId", Session["EmployeeId"]);
                //param.Add("@p_UserRightsModuleId", Session["ModuleId"]);              
                //param.Add("@p_Transactiontype", "Transation");
                //var GetMenuList = DapperORM.ExecuteSP<dynamic>("sp_GetUserRights_MenuList", param).ToList(); // SP_getReportingManager

                //param.Add("@p_AccessPolicyId", Session["UserAccessPolicyId"]);
                //param.Add("@p_ModuleId", Session["ModuleId"]);
                //param.Add("@p_ScreenMenuType", "Transation");
                //param.Add("@p_ScreenType", "Form");
                //param.Add("@p_ScreenSubId", ScreenId);
                //var GetMenuList = DapperORM.ExecuteSP<dynamic>("sp_Access_SubMenuList", param).ToList(); // SP_getReportingManager
                //ViewBag.GetUserMenuList = GetMenuList;              

                //if (ScreenId != null)
                //{
                //    Session["ScreenId"] = ScreenId;
                //    TempData["ScreenId"] = ScreenId;
                //}
                //else
                //{
                //    TempData["ScreenId"] = Session["ScreenId"];
                //}
                //DynamicParameters paramList = new DynamicParameters();
                //paramList.Add("@ScreenID", Session["ScreenId"]);
                //paramList.Add("@EmployeeID", Session["EmployeeId"]);
                //var GetMenu = DapperORM.ExecuteSP<dynamic>("sp_GetList_Menu", paramList).ToList();
                //if(GetMenu.Count!=0)
                //{                   
                //    if (GetMenu[0].ScreenId == "177")
                //    {
                //        ViewBag.GetMenuListResourceRequest = GetMenu;
                //    }
                //    else
                //    {
                //        ViewBag.GetMenuListResourceRequest = "";
                //    }
                //    if (GetMenu[0].ScreenId == "179")
                //    {
                //        ViewBag.GetMenuListApproval = GetMenu;
                //    }
                //    else
                //    {
                //        ViewBag.GetMenuListApproval = "";
                //    }

                //}
                //else
                //{
                //    ViewBag.GetMenuLists = "";
                //    TempData["ScreenMasterId"] = "";
                //}
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }        

        [HttpGet]
        public ActionResult ForApprovalCandidate(int? DocId, string Encrypted,string Status,string Remark)
        {
            try
            {
                DynamicParameters paramApprove = new DynamicParameters();
                paramApprove.Add("@p_Origin", "Recruitment");
                paramApprove.Add("@p_DocId_Encrypted", Encrypted);
                paramApprove.Add("@p_DocId", DocId);
                paramApprove.Add("@p_Managerid", Session["EmployeeId"]);
                paramApprove.Add("@p_Status", Status);
                paramApprove.Add("@p_ApproveRejectRemark", Remark);
                paramApprove.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                paramApprove.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var GetApprovedResult = DapperORM.ExecuteSP<dynamic>("sp_Approved_Rejected", paramApprove);
                var Message = paramApprove.Get<string>("@p_msg");
                var Icon = paramApprove.Get<string>("@p_Icon");
                TempData["Message"] = Message;
                TempData["Icon"] = Icon.ToString();

               

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

    }
}