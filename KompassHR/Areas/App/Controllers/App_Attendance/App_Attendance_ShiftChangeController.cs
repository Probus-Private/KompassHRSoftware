using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.App.Controllers.App_Attendance
{
    public class App_Attendance_ShiftChangeController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: App/App_Attendance_ShiftChange


        #region App_Attendance_ShiftChange
        public ActionResult App_Attendance_ShiftChange(string ShiftChangeID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 310;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { Area = "App" });
                //}
                ViewBag.AddUpdateTitle = "Add";
                var GetDocNo = DapperORM.DynamicQuerySingle("Select Isnull(Max(DocNo),0)+1 As DocNo from Atten_ShiftChange");
                var DocNo = GetDocNo != null ? GetDocNo.DocNo : null;

                //var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Atten_ShiftChange";
                //var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                //var DocNo = DapperORM.DynamicQuerySingle("Select Isnull(Max(DocNo),0)+1 As DocNo from Ticket_Raise");
                ViewBag.DocNo = DocNo;

                var Id = Session["EmployeeId"];
                param = new DynamicParameters();
                param.Add("@query", "select EmployeeBranchId as BranchId from Mas_Employee where Deactivate=0 and EmployeeId='" + Session["EmployeeId"] + "'");
                var GetBranchId = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).FirstOrDefault();

                param = new DynamicParameters();
                param.Add("@query", "select Atten_Shifts.ShiftId as Id  ,Atten_Shifts.ShiftName + ' ( ' + RIGHT(CONVERT(VARCHAR, Atten_Shifts.BeginTime, 100), 7) + '-' + RIGHT(CONVERT(VARCHAR, Atten_Shifts.EndTime, 100), 7) + ' )' as Name from Atten_Shifts where Deactivate=0 and ShiftBranchId='" + GetBranchId.BranchId + "' and Atten_Shifts.ShiftName not in ('Flexi Shift')");
                var GetShift = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetShiftList = GetShift;

                //var ShiftChangeLastDay = "select max(CreatedDate) As CreatedDate from Atten_ShiftChange where Deactivate=0";
                //var LastRecored = DapperORM.DynamicQuerySingle(ShiftChangeLastDay);
                //ViewBag.LastRecored = LastRecored.CreatedDate;


                ShiftChange shiftchange = new ShiftChange();
                if (ShiftChangeID_Encrypted != null)
                {
                    param = new DynamicParameters();
                    param.Add("@query", "select Status from Atten_ShiftChange where Deactivate=0 and Status='Pending' and ShiftChangeId_Encrypted='" + ShiftChangeID_Encrypted + "' ");
                    var GetStatus = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).FirstOrDefault();
                    if (GetStatus == null || GetStatus.Status != "Pending")
                    {
                        TempData["Message"] = "The shift change request is not pending, therefore it cannot be updated.";
                        TempData["Icon"] = "warning";
                        return RedirectToAction("GetList", "ESS_TimeOffice_ShiftChange");
                    }

                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_Qry", "and Atten_ShiftChange.ShiftChangeId_Encrypted='" + ShiftChangeID_Encrypted + "'");
                    shiftchange = DapperORM.ReturnList<ShiftChange>("sp_List_Atten_ShiftChange", param).FirstOrDefault();
                    TempData["FromDate"] = shiftchange.FromDate;
                    TempData["ToDate"] = shiftchange.ToDate;

                    //var GetVoucherNo = "Select DocNo from Atten_ShiftChange where ShiftChangeId_Encrypted='" + ShiftChangeID_Encrypted + "'";
                    //var VoucherNo = DapperORM.DynamicQuerySingle(GetVoucherNo);
                    var GetVoucherNo = DapperORM.DynamicQuerySingle("Select DocNo from Atten_ShiftChange where ShiftChangeId_Encrypted='" + ShiftChangeID_Encrypted + "'");
                    var VoucherNo = GetVoucherNo != null ? GetVoucherNo.DocNo : null;
                    ViewBag.DocNo = VoucherNo;
                }
                TempData["GetMinMaxDate"] = DapperORM.DynamicQuerySingle("Select BackDateDays as MinDate, FutureDateDays as MaxDate from Atten_ShiftChangeSetting where CmpId='" + Session["CompanyId"] + "' and ShiftChangeSettingBranchId='" + Session["BranchId"] + "'");
                //TempData["GetMinMaxDate"] = DapperORM.DynamicQuerySingle(@"Select BackDateDays as MinDate, FutureDateDays as MaxDate from Atten_ShiftChangeSetting where CmpId='" + Session["CompanyId"] + "' and ShiftChangeSettingBranchId='" + Session["BranchId"] + "'").FirstOrDefault();
                DynamicParameters ConflictEmployee = new DynamicParameters();
                ConflictEmployee.Add("@query", @"Select EmployeeName,Replace(convert(nvarchar(12),FromDate,106),' ','/') as FromDate,Replace(convert(nvarchar(12),ToDate,106),' ','/') as  ToDate 
                                               from Mas_Employee Inner join Atten_ShiftChange on Atten_ShiftChange.ShiftChangeEmployeeId=Mas_Employee.EmployeeId
                                               where Mas_Employee.Employeeid in (Select ReportingEmployeeID from MAs_employee_Reporting where MAs_employee_Reporting.Deactivate=0 
                                               and  ReportingManager1=" + Session["EmployeeId"] + ") and Mas_Employee.Deactivate=0 and Atten_ShiftChange.Deactivate=0  and Mas_Employee.EmployeeId<>" + Session["EmployeeId"] + "");
                //ConflictEmployee.Add("@query", @"Select EmployeeName,replace(convert(nvarchar(11),FromDate,106), ' ','/') as FromDate , replace(convert(nvarchar(11),ToDate,106), ' ','/') as ToDate , TotalDays from Mas_Employee 
                //                                    Inner join Leave_Master on Leave_Master.LeaveMasterEmployeeId=Mas_Employee.EmployeeId
                //                                    where Mas_Employee.EmployeeDepartmentID=(select e.Employeedepartmentid from mas_employee e where e.employeeid =" + Session["EmployeeId"] + ") and Mas_Employee.EmployeeBranchId="+Session["BranchId"] +" and Mas_Employee.Deactivate = 0 and Leave_Master.Deactivate = 0  and Mas_Employee.EmployeeId != " + Session["EmployeeId"] + "");
                ViewBag.GetConflictEmployee = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", ConflictEmployee);

                return View(shiftchange);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion

        #region GetList
        public ActionResult App_GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 310;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { Area = "App" });
                //}
                param.Add("@p_Qry", "and Atten_ShiftChange.ShiftChangeEmployeeId='" + Session["EmployeeId"] + "'");
                var getShiftChangeList = DapperORM.ReturnList<dynamic>("sp_List_Atten_ShiftChange", param).ToList();
                ViewBag.GetShiftChangeList = getShiftChangeList;

                DynamicParameters ParamManager = new DynamicParameters();
                ParamManager.Add("@query", @"Select EmployeeId as Id , EmployeeName as Name from mas_employee_reporting,mas_employee
                                                where reportingmoduleid = 1 and ReportingEmployeeID = " + Session["EmployeeId"] + " and mas_employee_reporting.Deactivate = 0 and mas_employee_reporting.ReportingManager1 = mas_employee.EmployeeId");
                var Getdata = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", ParamManager);
                ViewBag.GetManagerEmployee = Getdata;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion

        #region ShiftChange Cancel Request Request function
        [HttpGet]
        public ActionResult App_CancelRequest(int? GetShiftChangeId, int ddlManagerId, string Remark, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                param.Add("@p_Origin", Origin);
                param.Add("@p_DocId", GetShiftChangeId);
                param.Add("@p_ManagerID", ddlManagerId);
                param.Add("@p_CancelRemark", Remark);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeId"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Tra_RequestCancel", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion

        #region IsValidation
        [HttpGet]
        public ActionResult App_IsShiftChangeExists(DateTime ToDate, int? ShiftNameId, DateTime FromDate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                param.Add("@p_process", "IsValidation");
                param.Add("@p_ShiftChangeEmployeeId", Session["EmployeeId"]);
                param.Add("@p_FromDate", Convert.ToDateTime(FromDate).ToString("yyyy/MM/dd"));
                param.Add("@p_ToDate", Convert.ToDateTime(ToDate).ToString("yyyy/MM/dd"));
                param.Add("@p_ShiftChangeShiftId", ShiftNameId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_ShiftChange", param);
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
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion

        #region SaveUpdateShiftChange
        public ActionResult App_SaveUpdate(ShiftChange shiftchange)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 310;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { Area = "App" });
                //}
                param.Add("@p_process", string.IsNullOrEmpty(shiftchange.ShiftChangeID_Encrypted) ? "Save" : "Update");
                param.Add("@p_ShiftChangeId", shiftchange.ShiftChangeID);
                param.Add("@p_ShiftChangeId_Encrypted", shiftchange.ShiftChangeID_Encrypted);
                param.Add("@p_DocNo", shiftchange.DocNo);
                param.Add("@p_FromDate", shiftchange.FromDate);
                param.Add("@p_ToDate", shiftchange.ToDate);
                param.Add("@p_ShiftChangeShiftId", shiftchange.ShiftChangeShiftId);
                param.Add("@p_ShiftChangeReason", shiftchange.ShiftChangeReason);
                param.Add("@p_ShiftChangeEmployeeId", Session["EmployeeId"]);
                param.Add("@p_RequestFrom", "Web");
                param.Add("@p_CmpId", Session["CompanyId"]);
                param.Add("@p_ShiftChangeBranchId", Session["BranchId"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_ShiftChange", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("App_Attendance_ShiftChange", "App_Attendance_ShiftChange");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion

        #region RequestStatus
        [HttpGet]
        public ActionResult App_RequestStatus(int DocId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_DocID", DocId);
                param.Add("@p_Origin", "Atten_ShiftChange");
                var data = DapperORM.ExecuteSP<dynamic>("sp_RequestTimeLine", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }

        }
        #endregion

        #region Delete
        public ActionResult App_Delete(string ShiftChangeID_Encrypted, int? ShiftChangeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                param = new DynamicParameters();
                param.Add("@p_process", "Delete");
                param.Add("@p_ShiftChangeId_Encrypted", ShiftChangeID_Encrypted);
                param.Add("@p_ShiftChangeId", ShiftChangeId);
                param.Add("@p_CreatedupdateBy", "Admin");
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_ShiftChange", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("App_GetList", "App_Attendance_ShiftChange");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion

        #region
        public ActionResult App_ShiftChangeDetails(string ShiftChangeID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 310;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { Area = "App" });
                //}
                param.Add("@p_Qry", "and Atten_ShiftChange.ShiftChangeId_Encrypted='" + ShiftChangeID_Encrypted + "'");
                var getShiftChangeList = DapperORM.ReturnList<dynamic>("sp_List_Atten_ShiftChange", param).ToList();
                ViewBag.GetShiftChangeList = getShiftChangeList;

                DynamicParameters ParamManager = new DynamicParameters();
                ParamManager.Add("@query", @"Select EmployeeId as Id , EmployeeName as Name from mas_employee_reporting,mas_employee
                                                where reportingmoduleid = 1 and ReportingEmployeeID = " + Session["EmployeeId"] + " and mas_employee_reporting.Deactivate = 0 and mas_employee_reporting.ReportingManager1 = mas_employee.EmployeeId");
                var Getdata = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", ParamManager);
                ViewBag.GetManagerEmployee = Getdata;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion

    }
}