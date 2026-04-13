using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_Leave;
using KompassHR.Models;
using Dapper;
using System.Net;
using System.Data;
using System.Data.SqlClient;

namespace KompassHR.Areas.Setting.Controllers.Setting_Leave
{
    public class Setting_Leave_LeaveDeductionController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);


        // GET: LeaveSetting/LeaveYear
        #region LeaveDeduction main View
        [HttpGet]
        // GET: Setting/Setting_Leave_LeaveDeduction
        public ActionResult Setting_Leave_LeaveDeduction(string LeaveDeductionID_Encrypted)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 642;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Leave_Deduction Leave_Deduction = new Leave_Deduction();

                var GetSequenceNo = "SELECT ISNULL(MAX(Sequence), 0) + 1 AS Sequence FROM Leave_Deduction";
                var result = DapperORM.DynamicQuerySingle(GetSequenceNo); // returns DapperRow
                string Sequence = result.Sequence.ToString();   // safely extract the value
                Leave_Deduction.Sequence = Sequence;

                param = new DynamicParameters();
                param.Add("@query", "SELECT LeaveTypeId as LeaveDeduction_LeaveTypeId,LeaveTypeShortName FROM Leave_Type  where Leave_Type.Deactivate = 0 and IsActive=1  order by LeaveTypeShortName");
                var GetLeaveType = DapperORM.ReturnList<Leave_Deduction>("sp_QueryExcution", param).ToList();
                ViewBag.GetLeaveType = GetLeaveType;

                if (LeaveDeductionID_Encrypted != null)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    paramList.Add("@p_LeaveDeductionID_Encrypted", LeaveDeductionID_Encrypted);
                    Leave_Deduction = DapperORM.ReturnList<Leave_Deduction>("sp_List_Leave_Deduction", paramList).FirstOrDefault();
                   // ViewBag.Sequence = Leave_Deduction.Sequence;
                }

                return View(Leave_Deduction);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsValidation
        [HttpGet]
        public ActionResult IsLeaveDeductionExists(string LeaveDeductionID_Encrypted, string LeaveDeduction_LeaveTypeId, string Sequence)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "IsValidation");
                param.Add("@p_LeaveDeductionID_Encrypted", LeaveDeductionID_Encrypted);
                param.Add("@p_LeaveDeduction_LeaveTypeId", LeaveDeduction_LeaveTypeId);
                param.Add("@p_Sequence",Sequence);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Leave_Deduction", param);
                var Message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
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
        #endregion


        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Leave_Deduction LeaveDeduction)
        {
            try
            {

                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(LeaveDeduction.LeaveDeductionID_Encrypted) ? "Save" : "Update");
                param.Add("@p_LeaveDeductionID", LeaveDeduction.LeaveDeductionID);
                param.Add("@p_LeaveDeductionID_Encrypted", LeaveDeduction.LeaveDeductionID_Encrypted);
                param.Add("@P_LeaveDeduction_LeaveTypeId ", LeaveDeduction.LeaveDeduction_LeaveTypeId);
                param.Add("@p_Sequence", LeaveDeduction.Sequence);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Leave_Deduction", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Leave_LeaveDeduction", "Setting_Leave_LeaveDeduction");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetListView
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 642;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_LeaveDeductionID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Leave_Deduction", param);
                ViewBag.GetLeaveDeductionList = data;
                return View();



            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region Delete
        [HttpGet]
        public ActionResult Delete(string LeaveDeductionID_Encrypted, int LeaveDeductionID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_LeaveDeductionID", LeaveDeductionID);
                param.Add("@p_LeaveDeductionID_Encrypted", LeaveDeductionID_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_Leave_Deduction", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Leave_LeaveDeduction");
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





