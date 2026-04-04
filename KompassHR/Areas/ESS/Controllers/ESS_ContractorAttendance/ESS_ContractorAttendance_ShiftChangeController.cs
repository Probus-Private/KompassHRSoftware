using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Areas.Setting.Models.Setting_TimeOffice;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_ContractorAttendance
{
    public class ESS_ContractorAttendance_ShiftChangeController : Controller
    {

        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_ContractorAttendance_ShiftChange
        #region ContractorAttendance_ShiftChange
        public ActionResult ESS_ContractorAttendance_ShiftChange(string ShiftChangeID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 337;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {

                    var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from atten_shiftchange";
                    var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    ViewBag.DocNo = DocNo;
                    //Shift 
                    //DynamicParameters paramShift = new DynamicParameters();
                    //paramShift.Add("@p_EmployeeID", Session["EmployeeId"]);
                    //ViewBag.GetShiftList = DapperORM.ReturnList<Atten_Shifts>("sp_GetShift", paramShift);
                    ViewBag.GetShiftList = "";

                    //DynamicParameters paramEMP = new DynamicParameters();
                    //paramEMP.Add("@query", @"select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeebranchid in (select distinct branchid from UserBranchMapping where AccessWorkForce = 1 and isactive = 1 and EmployeeId<>1 and EmployeeId = " + Session["EmployeeId"] + ") Order by EmployeeName");
                    //var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                    //ViewBag.EmployeeName = GetEmployeeName;
                    param = new DynamicParameters();
                    param.Add("@p_Origin", "ContractorPG");
                    param.Add("@p_EmployeeId", Session["EmployeeId"]);
                    var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorEmployeeDropdown", param).ToList();
                    ViewBag.EmployeeName = GetEmployeeName;

                    if (ShiftChangeID_Encrypted != null)
                    {
                        param.Add("@query", "Select * from Atten_ShiftChange where Deactivate=0  and ShiftChangeID_Encrypted='" + ShiftChangeID_Encrypted + "'");
                        var add = DapperORM.ReturnList<Models.ESS_TimeOffice.ShiftChange>("sp_QueryExcution", param).FirstOrDefault();
                        //ViewBag.AtdDateLog = Convert.ToDateTime(add.ShiftChangeDate).ToString("dd/MMM/yyy");
                        return View(add);
                    }
                    else
                    {

                    }
                }

                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion

        #region IsValidation
        [HttpGet]
        public ActionResult IsWorkerShiftChangeExists(DateTime FromDate, int? ShiftChangeEmployeeId,DateTime ToDate,string ShiftChangeId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_ShiftChangeEmployeeId", ShiftChangeEmployeeId);
                param.Add("@p_FromDate", Convert.ToDateTime(FromDate).ToString("yyyy/MM/dd"));
                param.Add("@p_ToDate", Convert.ToDateTime(ToDate).ToString("yyyy/MM/dd"));
                // param.Add("@p_ShiftChangeID_Encrypted", ShiftChangeID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_Worker_ShiftChange", param);
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

        #region SaveUpdateShiftChange
        public ActionResult SaveUpdateShiftChange(ShiftChange shiftchange)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 337;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(shiftchange.ShiftChangeID_Encrypted) ? "Save" : "Update");
                param.Add("@_ShiftChangeId", shiftchange.ShiftChangeID);
                param.Add("@p_ShiftChangeId_Encrypted", shiftchange.ShiftChangeID_Encrypted);
                param.Add("@p_DocNo", shiftchange.DocNo);
                param.Add("@p_FromDate", shiftchange.FromDate);
                param.Add("@p_ToDate", shiftchange.ToDate);
                param.Add("@p_ShiftChangeShiftId", shiftchange.ShiftChangeShiftId);
                param.Add("@p_ShiftChangeReason", shiftchange.ShiftChangeReason);
                param.Add("@p_ShiftChangeEmployeeId", shiftchange.ShiftChangeEmployeeid);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_ReportingManager1", Session["Employeeid"]);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_RequestFrom", "ContractorWeb");
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_Worker_ShiftChange", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_ContractorAttendance_ShiftChange", "ESS_ContractorAttendance_ShiftChange");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetList
        public ActionResult ESS_ContractorAttendance_GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 337;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param = new DynamicParameters();
                param.Add("@p_ShiftChangeId_Encrypted", "List");
                param.Add("@p_Qry", " and ShiftChangeBranchId in (select distinct BranchID from UserBranchMapping where IsActive=1 and AccessWorkForce=1 and EmployeeID='" + Session["EmployeeId"] + "')");
                var getShiftChangeList = DapperORM.ReturnList<dynamic>("sp_List_Atten_Worker_ShiftChange", param).ToList();
                ViewBag.GetShiftChangeList = getShiftChangeList;

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
        public ActionResult Delete(string ShiftChangeID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 337;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                } 
                param = new DynamicParameters();
                param.Add("@p_process", "Delete");
                param.Add("@p_ShiftChangeId_Encrypted", ShiftChangeID_Encrypted);
                param.Add("@p_CreatedupdateBy", "Admin");
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_Worker_ShiftChange", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_ContractorAttendance_GetList", "ESS_ContractorAttendance_ShiftChange");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetEmployeeShift
        [HttpGet]
        public ActionResult GetEmployeeShift(int EmployeeID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters paramShift = new DynamicParameters();
                paramShift.Add("@p_EmployeeID", EmployeeID);
                var GetShiftList = DapperORM.ReturnList<Atten_Shifts>("sp_GetShifts", paramShift);
                return Json(GetShiftList, JsonRequestBehavior.AllowGet);
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