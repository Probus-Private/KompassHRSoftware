using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Leave
{
    public class ESS_Leave_ApprovalController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_Leave_Approval
        #region Approval MAin View
        [HttpGet]
        public ActionResult ESS_Leave_Approval()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 147;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_Origin", "Approval");
                var LeaveApproval = DapperORM.DynamicList("sp_List_ESS_Leave_Approval", param);
                ViewBag.LeaveApprovalList = LeaveApproval;

                DynamicParameters ConflictEmployee = new DynamicParameters();
                ConflictEmployee.Add("@query", @"Select EmployeeName,Replace(convert(nvarchar(12),FromDate,106),' ','/') as FromDate,Replace(convert(nvarchar(12),ToDate,106),' ','/') as  ToDate 
                                               , TotalDays from Mas_Employee Inner join Leave_Master on Leave_Master.LeaveMasterEmployeeId=Mas_Employee.EmployeeId
                                                 where Mas_Employee.Employeeid in (Select ReportingEmployeeID from MAs_employee_Reporting where MAs_employee_Reporting.Deactivate=0 and  ReportingManager1=" + Session["EmployeeId"] + ") and Mas_Employee.Deactivate=0 and Leave_Master.Deactivate=0  and Mas_Employee.EmployeeId<>" + Session["EmployeeId"] + " and year(FromDate) >= year(GETDATE()) and year(ToDate) <=year(GETDATE())");
                ViewBag.GetConflictEmployee = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", ConflictEmployee);

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region View for ViewForLeaveRequestApprover 
        public ActionResult ViewForLeaveRequestApprover(string DocId_Encrypted, string Type, string ApproverMode, int? EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 147;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Session["ApproverMode"] = ApproverMode;
                param.Add("@p_DocId_Encrypted", DocId_Encrypted);
                param.Add("@p_Origin", Type);
                var GetLeaveApproval = DapperORM.ExecuteSP<dynamic>("sp_List_Leave_Profiles", param).FirstOrDefault();
                ViewBag.LeaveApprovalList = GetLeaveApproval;
                TempData["LeaveApprovalStatus"] = GetLeaveApproval.Status;

                param = new DynamicParameters();
                param.Add("@query", "select LeaveYearID as Id, cast(year(FromDate) as nvarchar(4))+'-'+cast(YEAR(ToDate) as nvarchar(4)) as Name from[dbo].[Leave_Year] where Deactivate = 0  and IsActivate=1  and CmpId='" + Session["CompanyId"] + "' order by IsDefault desc,FromDate desc");
                var LeaveYearGet = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                DynamicParameters paramLeaveBal = new DynamicParameters();
                paramLeaveBal.Add("@p_EmployeeId", EmployeeId);
                paramLeaveBal.Add("@p_CmpId", Session["CompanyId"]);
                paramLeaveBal.Add("@p_FyearId", LeaveYearGet[0].Id);
                var GetBalance = DapperORM.ReturnList<dynamic>("sp_GetLeaveBalance", paramLeaveBal).ToList();
                ViewBag.GetLeaveBalance = GetBalance;

                DynamicParameters paramlist = new DynamicParameters();
                paramlist.Add("@p_EmployeeId", EmployeeId);
                var LeaveAll = DapperORM.DynamicList("sp_List_EmployeeLeave", paramlist);
                ViewBag.LeaveAll = LeaveAll;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region View for ViewForCOFFRequestApprover 
        public ActionResult ViewForCOFFRequestApprover(string DocId_Encrypted, string Type, string ApproverMode)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 147;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Session["ApproverMode"] = ApproverMode;
                param.Add("@p_DocId_Encrypted", DocId_Encrypted);
                param.Add("@p_Origin", Type);
                var GetCOFFApproval = DapperORM.ExecuteSP<dynamic>("sp_List_Leave_Profiles", param).FirstOrDefault();
                ViewBag.COFFApprovalList = GetCOFFApproval;
                return View();
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
        public ActionResult ApproveLeaveRequest(int? DocId, string Encrypted, string Status, string Remark, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 147;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var ApproverMode = Convert.ToString(Session["ApproverMode"]);
                if (ApproverMode == "Bulk")
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_Origin", Origin);
                    param.Add("@p_DocId_Encrypted", Encrypted);
                    param.Add("@p_DocId", DocId);
                    param.Add("@p_Status", Status);
                    param.Add("@p_ApproveRejectRemark", Remark);
                    //param.Add("@p_Managerid", Session["EmployeeId"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data1 = DapperORM.ExecuteReturn("sp_Approved_Rejected_ForAdmin", param);
                    var message1 = param.Get<string>("@p_msg");
                    var Icon1 = param.Get<string>("@p_Icon");
                    TempData["Message"] = message1;
                    TempData["Icon"] = Icon1;
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"], ApproverMode = "Bulk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    param.Add("@p_Origin", Origin);
                    param.Add("@p_DocId", DocId);
                    param.Add("@p_DocId_Encrypted", Encrypted);
                    param.Add("@p_ApproveRejectRemark", Remark);
                    param.Add("@p_Status", Status);
                    param.Add("@p_Managerid", Session["EmployeeId"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_Approved_Rejected", param);
                    var message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    TempData["Message"] = message;
                    TempData["Icon"] = Icon;
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"], ApproverMode = "Approval" }, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ApprovedRejectedList
        public ActionResult LeaveApprovedRejectedList(MonthWiseFilter OBJList)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 147;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_Origin", "ApprovedRequest");
                param.Add("@p_MonthYear", OBJList.Month);
                var LeaveApprovalRejected = DapperORM.DynamicList("sp_List_ESS_Leave_Approval", param);

                ViewBag.LeaveApprovalRejectedList = LeaveApprovalRejected;
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
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 147;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
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
                    var data = DapperORM.ExecuteReturn("sp_Approved_Rejected", param);
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