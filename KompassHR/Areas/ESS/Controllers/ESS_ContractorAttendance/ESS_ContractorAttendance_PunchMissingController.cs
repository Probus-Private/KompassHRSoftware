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
    public class ESS_ContractorAttendance_PunchMissingController : Controller
    {
        // GET: ESS/ESS_ContractorAttendance_PunchMissing
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        #region Main View 
        public ActionResult ESS_ContractorAttendance_PunchMissing(string PunchMissingId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 334;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    //sneha code
                    var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Atten_PunchMissing";
                    var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    ViewBag.DocNo = DocNo;

                    //var GetBackDatedDays = DapperORM.DynamicQuerySingle("Select BackDateDays from Atten_PunchMissingSetting where Deactivate=0 and CmpId=" + Session["CompanyId"] + "");
                    //if (GetBackDatedDays != null)
                    //{
                    //    ViewBag.GetBackDatedDays = GetBackDatedDays.BackDateDays;
                    //}
                    //else
                    //{
                    //    ViewBag.GetBackDatedDays = "";
                    //}

                    param = new DynamicParameters();
                    var PunchMissingLastDay = "select max(CreatedDate) As CreatedDate from Atten_PunchMissing";
                    var LastRecored = DapperORM.DynamicQuerySingle(PunchMissingLastDay);
                    ViewBag.LastRecored = LastRecored.CreatedDate;

                    //DynamicParameters paramShift = new DynamicParameters();
                    //paramShift.Add("@p_EmployeeID", Session["EmployeeId"]);
                    ViewBag.GetShiftList = "";

                    //DynamicParameters AdditionalList = new DynamicParameters();
                    //AdditionalList.Add("@query", "Select AdditionNotifyId As Id,AdditionNotifyEmailID As Name from Mas_AdditionNotify Where Deactivate=0 and AdditionNotifyEmployeeID=" + Session["EmployeeId"] + "");
                    //var listMas_AdditionalList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", AdditionalList);
                    //ViewBag.GetAdditionalList = listMas_AdditionalList;

                    //DynamicParameters paramEMP = new DynamicParameters();
                    //paramEMP.Add("@query", @"select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from  mas_employee where deactivate=0 and employeeleft=0 and employeebranchid in (select  distinct BranchID from UserBranchMapping where accessworkforce = 1 and isactive = 1 and employeeid = '"+Session["EmployeeID"]+"' ) Order by EmployeeName");
                    //var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                    //ViewBag.EmployeeName = GetEmployeeName;
                    param = new DynamicParameters();
                    param.Add("@p_Origin", "ContractorPM");
                    param.Add("@p_EmployeeId", Session["EmployeeId"]);
                    var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorEmployeeDropdown", param).ToList();
                    ViewBag.EmployeeName = GetEmployeeName;

                    //param = new DynamicParameters();
                    //param.Add("@p_EmployeeID", Session["EmployeeId"]);
                    //ViewBag.GetDeviceLogsList = DapperORM.ReturnList<dynamic>("sp_GetDeviceLogs", param).ToList();

                    if (PunchMissingId_Encrypted != null)
                    {
                        param.Add("@query", "Select * from Atten_PunchMissing where Deactivate=0 and PunchMissingEmployeeId="+ Session["EmployeeId"] + " and PunchMissingId_Encrypted='" + PunchMissingId_Encrypted + "'");
                        var add = DapperORM.ReturnList<Models.ESS_TimeOffice.Atten_PunchMissing>("sp_QueryExcution", param).FirstOrDefault();

                        ViewBag.AtdDate = Convert.ToDateTime(add.PunchMissingAttendanceDate).ToString("dd/MMM/yyy");
                        ViewBag.AtdDateLog = Convert.ToDateTime(add.PunchMissingLogDate).ToString("dd/MMM/yyy");
                        return View(add);
                    }
                    else
                    {
                        return View();
                    }
                }
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
        public ActionResult IsPunchMissingExists(DateTime PunchmissingDate, string PunchMissingID_Encrypted, int EmployeeId, string PunchMissingInOut,DateTime PunchMissingLogDate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 334;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "IsValidation");
                param.Add("@p_PunchMissingEmployeeId", EmployeeId);
                param.Add("@p_PunchMissingAttendanceDate", Convert.ToDateTime(PunchmissingDate).ToString("yyyy/MM/dd"));
                param.Add("@p_PunchMissingId_Encrypted", PunchMissingID_Encrypted);
                param.Add("@p_PunchMissingLogDate", Convert.ToDateTime(PunchMissingLogDate).ToString("yyyy/MM/dd"));
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_PunchMissingInOut", PunchMissingInOut);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_Worker_PunchMissing", param);
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
        public ActionResult SaveUpdate(Models.ESS_TimeOffice.Atten_PunchMissing InOut)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 334;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "Save");
                param.Add("@P_PunchMissingEmployeeId", InOut.PunchMissingEmployeeID);
                param.Add("@P_DocNo", InOut.DocNo);
                param.Add("@P_PunchMissingAttendanceDate", InOut.PunchMissingAttendanceDate);
                param.Add("@P_PunchMissingLogDate", InOut.PunchMissingLogDate);
                param.Add("@P_PunchMissingLogTime", InOut.PunchMissingLogTime);
                param.Add("@P_PunchMissingInOut", InOut.PunchMissingInOut);
                param.Add("@P_PunchMissingShiftId", InOut.PunchMissingShiftId);
                param.Add("@P_Reason", InOut.Reason);
                param.Add("@P_RequestFrom", "ContractorWeb");
               // param.Add("@P_AdditionNotify", Session["AdditionalNotify"]);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@@p_ReportingManager1", Session["EmployeeId"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Atten_Worker_PunchMissing", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_ContractorAttendance_PunchMissing", "ESS_ContractorAttendance_PunchMissing");
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
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 334;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@P_Qry", "and PunchMissingBranchId in (select  distinct BranchID from UserBranchMapping where accessworkforce = 1 and isactive = 1 and employeeid = " + Session["EmployeeId"] + ") order by DocNo Desc");
                var PendingList = DapperORM.DynamicList("sp_List_Atten_Worker_PunchMissing", param);
                ViewBag.PendingList = PendingList;

                DynamicParameters ParamManager = new DynamicParameters();
                ParamManager.Add("@query", @"select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from  mas_employee where deactivate=0 and employeeleft=0 and employeebranchid in (select  distinct BranchID from UserBranchMapping where accessworkforce = 1 and isactive = 1 and employeeid = '"+Session["EmployeeId"]+"' )");
                var Getdata = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", ParamManager);
                ViewBag.GetManagerEmployee = Getdata;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetShift
        [HttpGet]
        public ActionResult GetShift(int EmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", EmpId);
                var Shift = DapperORM.ReturnList<Atten_Shifts>("sp_GetShift", param).ToList();
                return Json(new { ShiftList = Shift }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region Delete
        public ActionResult Delete(string PunchMissingId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 334;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_PunchMissingId_Encrypted", PunchMissingId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_Worker_PunchMissing", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_ContractorAttendance_GetList", "ESS_ContractorAttendance_PunchMissing");
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