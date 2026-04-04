using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_TimeOffice
{
    public class ESS_TimeOffice_BulkApprovalController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_TimeOffice_BulkApproval
        #region Main View 
        public ActionResult ESS_TimeOffice_BulkApproval(string GetFilter)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 399;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                if (GetFilter == null)
                {
                    param.Add("@p_List", "All");
                }
                else
                {
                    param.Add("@p_List", GetFilter);
                }

                var GetPending_ForApproval = DapperORM.ExecuteSP<dynamic>("sp_List_Pending_ForApproval_Admin", param).ToList();
                Session["LeaveCount"] = (GetFilter == null) ? GetPending_ForApproval.Count() : Session["LeaveCount"];

                ViewBag.TimeOfficeApprovalList = GetPending_ForApproval;

                DynamicParameters paramCount = new DynamicParameters();
                paramCount.Add("@p_Employeeid", Session["EmployeeId"]);
                var GetCount = DapperORM.ExecuteSP<dynamic>("sp_PendingForApprovalCount_Admin", paramCount).ToList(); // SP_getReportingManager
                TempData["TeamLeaveCount"] = GetCount[0].RequestCount;
                TempData["TeamPersonalGatepassCount"] = GetCount[1].RequestCount;
                TempData["TeamCoffCount"] = GetCount[2].RequestCount;
                TempData["TeamShiftChangeCount"] = GetCount[3].RequestCount;
                TempData["TeamOutDoorCompanyCount"] = GetCount[4].RequestCount;
                TempData["TeamWorkFromHomeCount"] = GetCount[5].RequestCount;
                TempData["TeamPunchMissingCount"] = GetCount[6].RequestCount;
                return View();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region  Multuple Multiple Approve Reject Request
        [HttpPost]
        public ActionResult MultipleApproveRejectRequest(List<RecordList> ObjRecordList)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 399;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                for (var i = 0; i < ObjRecordList.Count; i++)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_Origin", ObjRecordList[i].Origin);
                    param.Add("@p_DocId_Encrypted", ObjRecordList[i].DocID_Encrypted);
                    param.Add("@p_DocId", ObjRecordList[i].DocID);
                    param.Add("@p_Status", ObjRecordList[i].Status);
                    param.Add("@p_ApproveRejectRemark", ObjRecordList[i].RejectRemark);
                    param.Add("@p_Managerid", Session["EmployeeId"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_Approved_Rejected_ForAdmin", param);
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
    }
}