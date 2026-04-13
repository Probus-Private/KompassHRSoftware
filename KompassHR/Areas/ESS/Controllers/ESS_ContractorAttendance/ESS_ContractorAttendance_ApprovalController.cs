using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_ContractorAttendance
{
    public class ESS_ContractorAttendance_ApprovalController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_ContractorAttendance_Approval
        #region ESS_ContractorAttendance_Approval
        public ActionResult ESS_ContractorAttendance_Approval()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 356;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_List", "All");
                var WorkerForApproval = DapperORM.DynamicList("sp_List_AttenPending_WorkerForApproval", param);
                ViewBag.WorkerForApprovalList = WorkerForApproval;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ESS_ContractorAttendance_Approval
        public ActionResult ESS_ContractorAttendance_ApprovedRejectedList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 356;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_List", "ApprovedRequest");
                var WorkerForApprovalRejected = DapperORM.DynamicList("sp_List_AttenPending_WorkerForApproval", param);
                ViewBag.WorkerForApprovalRejectedList = WorkerForApprovalRejected;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ESS_ContractorAtten_ViewForOutdoorRequestApprover Print Form
        public ActionResult ESS_ContractorAtten_ViewForOutdoorRequestApprover(string DocId_Encrypted, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
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
        public ActionResult ESS_ContractorAtten_ViewForPunchMissingRequestApprover(string DocId_Encrypted, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
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
        public ActionResult ESS_ContractorAtten_ViewForPersonalGetpassRequestApprover(string DocId_Encrypted, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
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

        #region  View Short Leave Print Form
        public ActionResult ESS_ContractorAtten_ViewForShortLeaveRequestApprover(string DocId_Encrypted, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
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
        public ActionResult ESS_ContractorAtten_ViewForShiftChangeRequestApprover(string DocId_Encrypted, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
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

        #region  Multuple Multiple Approve Reject Request function
        [HttpPost]
        public ActionResult MultipleApproveRejectRequest(List<RecordList> ObjRecordList)
        {
            try
            {
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
                    var data = DapperORM.ExecuteReturn("sp_Approved_Rejected_Worker", param);
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

        #region  Approve Leave Request function
        [HttpGet]
        public ActionResult ApproveRequest(int? DocId, string Encrypted, string Status, string Remark, string Origin)
        {
            try
            {
                param.Add("@p_Origin", Origin);
                param.Add("@p_DocId_Encrypted", Encrypted);
                param.Add("@p_DocId", DocId);
                param.Add("@p_ApproveRejectRemark", Remark);
                param.Add("@p_Status", Status);
                param.Add("@p_Managerid", Session["EmployeeId"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_Approved_Rejected_Worker", param);
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

    }
}