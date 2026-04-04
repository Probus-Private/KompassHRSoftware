using Dapper;
using KompassHR.Areas.ESS.Models.ESS_PMS;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_PMS
{
    public class ESS_PMS_ApprovalController : Controller
    {
        DynamicParameters param = new DynamicParameters();

        // GET: ESS/ESS_PMS_Approval
        public ActionResult ESS_PMS_Approval(string OwnObjectiveCreationId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 626;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var param = new DynamicParameters();
                //param.Add("@query", "Select * from PMS_OwnObjectiveCreation where Deactivate = 0 and OwnObjectiveCreationId='" + OwnObjectiveCreationId + "'");
                //var OwnObjectiveInfo = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).ToList();
                //ViewBag.OwnObjectiveInfo = OwnObjectiveInfo;

                param.Add("@p_OwnObjectiveCreationId_Encrypted", OwnObjectiveCreationId_Encrypted);
                var OwnObjectiveInfo = DapperORM.DynamicList("sp_List_PMS_ESS_Approval", param);
                ViewBag.OwnObjectiveInfo = OwnObjectiveInfo;

                return View();
            }
            catch(Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_PMS_GoalCreation");
            }

        }

        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 128;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_Origin", "Approval");
                var PMSApproval = DapperORM.DynamicList("sp_List_PMS_ESS_Approval", param);
                ViewBag.PMSApproval = PMSApproval;
                return View();
            }
            catch(Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_PMS_GoalCreation");

            }
        }

        //#region ForApprovalCandidate
        //[HttpGet]
        //public ActionResult ForApprovalCandidate(int? DocId, string Encrypted, string Status, string Remark)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { area = "" });
        //        }
        //        DynamicParameters paramApprove = new DynamicParameters();
        //        paramApprove.Add("@p_Origin", "PMS");
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
        //        if (Message != "")
        //        {
        //            TempData["Message"] = Message;
        //            TempData["Icon"] = Icon.ToString();
        //            return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(true, JsonRequestBehavior.AllowGet);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }

        //}
        //#endregion

        public ActionResult ForApprovalCandidate(int DocId, string Encrypted, string Status, string Remark)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();

                param.Add("@p_OwnObjectiveCreationId", DocId);
                param.Add("@p_ManagerId", Convert.ToInt32(Session["EmployeeId"]));
                param.Add("@p_ManagerRemark", Remark);
                param.Add("@p_Status", Status);

                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                param.Add("@p_icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);

                var result = DapperORM.ExecuteReturn("sp_PMS_Objective_ApproveReject", param);
                var Message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_icon");
                //TempData["Message"] = param.Get<string>("@p_msg");
                //TempData["Icon"] = param.Get<string>("@p_icon");

                return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error: " + ex.Message;
                TempData["Icon"] = "error";
                return RedirectToAction("GetList", "ESS_PMS_Approval");
            }
        }
    }
}