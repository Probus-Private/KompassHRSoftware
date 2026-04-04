using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_EmployeeTransfer
{
    public class ESS_EmployeeTransfer_ForApprovalController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_EmployeeTransfer_ForApproval
        #region ESS_EmployeeTransfer_ForApproval
        public ActionResult ESS_EmployeeTransfer_ForApproval()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 406;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_List", "All");
                var EmployeeTransfer = DapperORM.DynamicList("sp_List_Pending_EmployeeTransfer", param);
                if (EmployeeTransfer != null)
                {
                    ViewBag.EmployeeTransferApprovalList = EmployeeTransfer;
                }
                else
                {
                    ViewBag.EmployeeTransferApprovalList = "";
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

        #region  View Outdoor Print Form
        public ActionResult ViewForEmployeeBusinessTransferApprover(string DocId_Encrypted, string Origin, int ToBranchId, int EmployeeId, int? DocID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 406;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Session["TransferOrigin"] = Origin;
                Session["OnboardBranchId"] = ToBranchId;
                Session["OnboardEmployeeId"] = EmployeeId;
                Session["TransferDocId"] = DocID;
                if (EmployeeId != null)
                {
                    var a = DapperORM.DynamicQueryList("Select EmployeeNo,EmployeeName,Replace(Convert(nvarchar(12),Mas_employee.JoiningDate,106),' ','/') as JoiningDate,Mas_Department.DepartmentName,Mas_Designation.DesignationName  from Mas_employee,Mas_Department,Mas_Designation where employeeid=" + EmployeeId + " and Mas_Department.DepartmentId=Mas_employee.EmployeeDepartmentID and Mas_Designation.DesignationId=Mas_employee.EmployeeDesignationID and Mas_employee.Deactivate=0").ToList();
                    var b = DapperORM.DynamicQuerySingle("Select CmpId from Mas_Branch where BranchId=" + ToBranchId + " and Mas_Branch.Deactivate=0");
                    Session["OnboardCmpId"] = b.CmpId;

                    Session["OnboardEmployeeName"] = a[0].EmployeeName;
                    Session["OnboardEmpDesignation"] = a[0].DesignationName;
                    Session["OnboardEmployeeNo"] = a[0].EmployeeNo;
                    Session["OnboardEmpDepartment"] = a[0].DepartmentName;
                    Session["OnboardEmpDoj"] = a[0].JoiningDate;


                }
                param.Add("@p_DocId_Encrypted", DocId_Encrypted);
                param.Add("@p_Origin", Origin);
                var GetTransferApproval = DapperORM.ExecuteSP<dynamic>("sp_List_Transfer_Profiles", param).FirstOrDefault();
                if (GetTransferApproval != null)
                {
                    ViewBag.GetTransferApprovalList = GetTransferApproval;
                }
                else
                {
                    ViewBag.GetTransferApprovalList = "";
                }


                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@query", "Select * from View_Onboarding_EmployeeList where EmployeeId=" + EmployeeId + " and Deactivate=0");
                var EmployeeReportList = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramList).FirstOrDefault();
                ViewBag.GetEmployeeReportList = EmployeeReportList;
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region EmployeeTransferApprovedRejectedList
        public ActionResult EmployeeTransferApprovedRejectedList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 406;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_List", "ApprovedRequest");
                var TimeOfficeApproved = DapperORM.DynamicList("sp_List_Pending_EmployeeTransfer", param);
                ViewBag.TransferApprovalRejectedList = TimeOfficeApproved;
                return View();

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region ApproveRequest
        public ActionResult ApproveRequest(int? DocId,string Remark)
        {
            try
            {

                DapperORM.DynamicQuerySingle("Update Trans_Businessunit set TransferStatus='Rejected',ApprovedDate=GETDATE(),TransferReason='"+ Remark + "' where  TransferBusinessUnitId="+DocId+"");
                TempData["Message"] = "Record save Successfully";
                TempData["Icon"] = "success";
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