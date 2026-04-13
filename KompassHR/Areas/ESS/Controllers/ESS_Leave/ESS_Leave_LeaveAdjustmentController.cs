using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Leave;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Setting.Models.Setting_Leave;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Leave
{
    public class ESS_Leave_LeaveAdjustmentController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_Leave_LeaveAdjustment
        #region ESS_Leave_LeaveAdjustment Main View 
        public ActionResult ESS_Leave_LeaveAdjustment(string LeaveOpeningBalanceId_Encrypted, int? CmpID, int? EmployeeId, int? BranchID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 149;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Leave_Adjustment LeaveAdjustment = new Leave_Adjustment();
                ViewBag.AddUpdateTitle = "Add";
                param.Add("@query", "select CompanyId as Id, CompanyName as Name from Mas_CompanyProfile  where Deactivate = 0 order by CompanyName");
                ViewBag.ComapnyName = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param).ToList();

                if (LeaveOpeningBalanceId_Encrypted != null)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    param1.Add("@p_LeaveAdjustmentId_Encrypted", LeaveOpeningBalanceId_Encrypted);
                    LeaveAdjustment = DapperORM.ReturnList<Leave_Adjustment>("sp_List_Leave_Adjustment", param1).FirstOrDefault();

                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_employeeid", Session["EmployeeId"]);
                    param.Add("@p_CmpId", CmpID);
                    ViewBag.BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                    //DynamicParameters paramBranch = new DynamicParameters();
                    //paramBranch.Add("@query", "select  BranchId as Id, BranchName as Name  from Mas_Branch where Deactivate =0 and CmpId=" + CmpID + " order by BranchName ");
                    //ViewBag.BranchName = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", paramBranch).ToList();

                    LeaveAdjustment.LeaveAdjustmenBranchId = Convert.ToInt32(BranchID);

                    DynamicParameters paramEmployee = new DynamicParameters();
                    paramEmployee.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0  and EmployeeBranchId=" + BranchID + " order by EmployeeName");
                    ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", paramEmployee).ToList();

                    //DynamicParameters paramType = new DynamicParameters();
                    //paramType.Add("@query", @"Select distinct(LeaveTypeId) As Id, Concat(LeaveTypeName ,'-' ,LeaveTypeShortName) as Name from [dbo].[Leave_OpeningBalance] 
                    //                       inner join Leave_Type on Leave_Type.LeaveTypeId = Leave_OpeningBalance.LeaveOpeningBalanceLeaveTypeId
                    //                       where LeaveOpeningBalanceEmployeeId = " + EmployeeId + "");
                    //ViewBag.LeaveTypeName = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", paramType).ToList();
                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@query", "select Leave_Setting.LeaveSettingId as Id ,concat(Leave_Group.LeaveGroupName, ' - ', Leave_Type.LeaveTypeShortName) as Name from Leave_Setting, Leave_Group, Leave_Type where Leave_Setting.Deactivate = 0 and Leave_Group.Deactivate = 0 and Leave_Type.Deactivate = 0 and Leave_Setting.LeaveSettingLeaveGroupId = Leave_Group.LeaveGroupId and Leave_Type.LeaveTypeId = Leave_Setting.LeaveSettingLeaveTypeId and Leave_Setting.CmpId=" + CmpID + "");
                    var LeaveType = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();
                    ViewBag.LeaveTypeName = LeaveType;

                    DynamicParameters paramyear = new DynamicParameters();
                    paramyear.Add("@query", @"select LeaveYearID as Id, cast(year(FromDate) as nvarchar(4))+'-'+cast(YEAR(ToDate) as nvarchar(4)) as Name from[dbo].[Leave_Year] where Deactivate = 0 and CmpId = " + CmpID + "");
                    ViewBag.GetLeaveYear = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", paramyear).ToList();

                }
                else
                {
                    ViewBag.BranchName = "";
                    ViewBag.LeaveTypeName = "";
                    ViewBag.EmployeeName = "";
                    ViewBag.GetLeaveYear = "";
                }
                return View(LeaveAdjustment);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "select LeaveYearID as Id, cast(year(FromDate) as nvarchar(4))+'-'+cast(YEAR(ToDate) as nvarchar(4)) as Name from[dbo].[Leave_Year] where Deactivate = 0 and CmpId = " + CmpId + "");
                var data1 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();

                return Json(new { data, data1 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetEmployeeName
        [HttpGet]
        public ActionResult GetEmployeeName(int BranchId)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", @"select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0  and EmployeeBranchId=" + BranchId + " order by EmployeeName");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetLeaveType
        [HttpGet]
        public ActionResult GetLeaveType(int CmpID)
        {
            try
            {
                //DynamicParameters param = new DynamicParameters();
                //param.Add("@query", @"Select distinct(LeaveTypeId) As Id, Concat(LeaveTypeName ,'-' ,LeaveTypeShortName) as Name from [dbo].[Leave_OpeningBalance] 
                //                           inner join Leave_Type on Leave_Type.LeaveTypeId = Leave_OpeningBalance.LeaveOpeningBalanceLeaveTypeId
                //                           where LeaveOpeningBalanceEmployeeId = " + EmployeeId + "");
                //var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                DynamicParameters param = new DynamicParameters();
                param.Add("@query", @"select Leave_Setting.LeaveSettingId as Id ,concat(Leave_Group.LeaveGroupName, ' - ', Leave_Type.LeaveTypeShortName) as Name 
                                    from Leave_Setting, Leave_Group, Leave_Type where Leave_Setting.Deactivate = 0 and Leave_Group.Deactivate = 0 
                                    and Leave_Type.Deactivate = 0 and Leave_Setting.LeaveSettingLeaveGroupId = Leave_Group.LeaveGroupId and Leave_Type.LeaveTypeId = Leave_Setting.LeaveSettingLeaveTypeId 
                                    and Leave_Setting.CmpId=" + CmpID + " and LeaveSettingLeaveTypeId<>1");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region SaveUpdate
        public ActionResult SaveUpdate(Leave_Adjustment LeaveAdjustment)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 149;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_process", string.IsNullOrEmpty(LeaveAdjustment.LeaveAdjustmentId_Encrypted) ? "Save" : "Update");
                param.Add("@P_CmpID", LeaveAdjustment.CmpID);
                //param.Add("@p_LeaveOpeningBalanceBranchID", Openingbalance.LeaveOpeningBalanceBranchID);
                param.Add("@P_LeaveAdjustmentSettingId", LeaveAdjustment.LeaveAdjustmentSettingId);
                param.Add("@P_LeaveAdjustmentYearId", LeaveAdjustment.LeaveAdjustmentYearId);
                param.Add("@P_LeaveAdjustmentId_Encrypted", LeaveAdjustment.LeaveAdjustmentId_Encrypted);
                param.Add("@P_NoOfLeave", LeaveAdjustment.NoOfLeave);
                param.Add("@P_LeaveAdjustmentEmployeeId", LeaveAdjustment.LeaveAdjustmentEmployeeId);
                param.Add("@P_AdjustmentRemark", LeaveAdjustment.AdjustmentRemark);
                param.Add("@P_MachineName", Dns.GetHostName().ToString());
                param.Add("@P_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@P_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@P_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Leave_Adjustment", param); 
                 TempData["Message"] = param.Get<string>("@P_msg");
                TempData["Icon"] = param.Get<string>("@P_Icon");

                return RedirectToAction("ESS_Leave_LeaveAdjustment", "ESS_Leave_LeaveAdjustment");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 149;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_LeaveAdjustmentId_Encrypted", "List");
                var OpneingBalancelist = DapperORM.DynamicList("sp_List_Leave_Adjustment", param);
                ViewBag.OpneingBalancelist = OpneingBalancelist;

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
        public ActionResult Delete(string LeaveAdjustmentId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 149;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_LeaveAdjustmentId_Encrypted", LeaveAdjustmentId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Leave_Adjustment", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_Leave_LeaveAdjustment");
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