using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using KompassHR.Models;
using System.Web.Mvc;
using System.Net;
using System.Data;
using KompassHR.Areas.Setting.Models.Setting_TimeOffice;
using System.Data.SqlClient;

namespace KompassHR.Areas.Setting.Controllers.Setting_TimeOffice
{
    public class Setting_TimeOffice_ShiftController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        #region Shift Main View
        [HttpGet]
        public ActionResult Setting_TimeOffice_Shift(string ShiftId_Encrypted , int? CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 52;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                ViewBag.AddUpdateTitle = "Add";               
                param.Add("@query", "Select  CompanyId As Id, CompanyName As Name from Mas_CompanyProfile where Deactivate= 0 order by Name");
                var GetCompanyName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.CompanyName = GetCompanyName;

                if (CmpId != null)
                {
                    DynamicParameters ParamBranch = new DynamicParameters();
                    ParamBranch.Add("@p_employeeid", Session["EmployeeId"]);
                    ParamBranch.Add("@p_CmpId", CmpId);
                    ViewBag.Location = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", ParamBranch).ToList();
                }
                else
                {
                    ViewBag.Location = "";
                }

                Atten_Shifts Atten_Shifts = new Atten_Shifts();
                if (ShiftId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_ShiftId_Encrypted", ShiftId_Encrypted);
                    Atten_Shifts = DapperORM.ReturnList<Atten_Shifts>("sp_List_Atten_Shifts", param).FirstOrDefault();
                }
                return View(Atten_Shifts);
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
        public ActionResult IsShiftExists(string SName, string ShiftName, double ShiftBranchId, double CmpID, string ShiftId_Encrypted)
        {
            try
            {              

                param.Add("@p_process", "IsValidation");
                param.Add("@P_ShiftId_Encrypted", ShiftId_Encrypted);
                param.Add("@P_ShiftBranchId", ShiftBranchId);
                param.Add("@P_CmpID", CmpID);
                param.Add("@P_SName", SName);
                param.Add("@P_ShiftName", ShiftName);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_Shifts", param);
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

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
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
        [HttpPost]
        public ActionResult SaveUpdate(Atten_Shifts shift)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(shift.ShiftId_Encrypted) ? "Save" : "Update");
                param.Add("@p_ShiftId_Encrypted", shift.ShiftId_Encrypted);
                param.Add("@p_ShiftId", shift.ShiftId);
                param.Add("@p_ShiftName", shift.ShiftName);
                param.Add("@p_ShiftBranchId", shift.ShiftBranchId);
                param.Add("@p_SName", shift.SName);
                param.Add("@p_BeginTime", shift.BeginTime);
                param.Add("@p_EndTime", shift.EndTime);
                param.Add("@p_GraceTimeForLateComming", shift.GraceTimeForLateComming);
                param.Add("@p_GraceTimeForEarlyGoing", shift.GraceTimeForEarlyGoing);
                param.Add("@p_PunchBeginDuration", 0);
                param.Add("@p_LunchTime", shift.LunchTime);
                param.Add("@p_LunchTimeMin", shift.LunchTimeMin);
                param.Add("@p_LunchTimePunchBegin", shift.LunchTimePunchBegin);
                param.Add("@p_ShiftFlag", shift.ShiftFlag);
                param.Add("@p_Duration", shift.Duration); 
                param.Add("@p_PunchEndDuration", 0);
                param.Add("@p_LunchFlag", 0);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_CmpID", shift.CmpId);
                param.Add("@p_ShiftDurationForOTCalculation", shift.ShiftDurationForOTCalculation);
                param.Add("@p_FullDayDuration", shift.ShiftDurationForPP);
                param.Add("@p_HalfDayDuration", shift.ShiftDurationForHD);

                param.Add("@p_Coff_Half_Regular_Applicable", shift.Coff_Half_Regular_Applicable);
                param.Add("@p_Coff_Half_Regular_From", shift.Coff_Half_Regular_From);
                param.Add("@p_Coff_Half_Regular_To", shift.Coff_Half_Regular_To);
                param.Add("@p_Coff_Full_Regular_Applicable", shift.Coff_Full_Regular_Applicable);
                param.Add("@p_Coff_Full_Regular_From", shift.Coff_Full_Regular_From);
                param.Add("@p_Coff_Full_Regular_To", shift.Coff_Full_Regular_To);
                param.Add("@p_Coff_Full_Half_Regular_Applicable", shift.Coff_Full_Half_Regular_Applicable);
                param.Add("@p_Coff_Full_Half_Regular_From", shift.Coff_Full_Half_Regular_From);
                param.Add("@p_Coff_Full_Half_Regular_To", shift.Coff_Full_Half_Regular_To);
                param.Add("@p_Coff_Two_Regular_Applicable", shift.Coff_Two_Regular_Applicable);
                param.Add("@p_Coff_Two_Regular_From", shift.Coff_Two_Regular_From);
                param.Add("@p_Coff_Two_Regular_To", shift.Coff_Two_Regular_To);

                param.Add("@p_Coff_Half_PH_Applicable", shift.Coff_Half_PH_Applicable);
                param.Add("@p_Coff_Half_PH_From", shift.Coff_Half_PH_From);
                param.Add("@p_Coff_Half_PH_To", shift.Coff_Half_PH_To);
                param.Add("@p_Coff_Full_PH_Applicable", shift.Coff_Full_PH_Applicable);
                param.Add("@p_Coff_Full_PH_From", shift.Coff_Full_PH_From);
                param.Add("@p_Coff_Full_PH_To", shift.Coff_Full_PH_To);
                param.Add("@p_Coff_Full_Half_PH_Applicable", shift.Coff_Full_Half_PH_Applicable);
                param.Add("@p_Coff_Full_Half_PH_From", shift.Coff_Full_Half_PH_From);
                param.Add("@p_Coff_Full_Half_PH_To", shift.Coff_Full_Half_PH_To);
                param.Add("@p_Coff_Two_PH_Applicable", shift.Coff_Two_PH_Applicable);
                param.Add("@p_Coff_Two_PH_From", shift.Coff_Two_PH_From);
                param.Add("@p_Coff_Two_PH_To", shift.Coff_Two_PH_To);
                param.Add("@p_Coff_Two_Half_PH_Applicable", shift.Coff_Two_Half_PH_Applicable);
                param.Add("@p_Coff_Two_Half_PH_From", shift.Coff_Two_Half_PH_From);
                param.Add("@p_Coff_Two_Half_PH_To", shift.Coff_Two_Half_PH_To);
                param.Add("@p_Coff_Three_PH_Applicable", shift.Coff_Three_PH_Applicable);
                param.Add("@p_Coff_Three_PH_From", shift.Coff_Three_PH_From);
                param.Add("@p_Coff_Three_PH_To", shift.Coff_Three_PH_To);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Atten_Shifts", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return RedirectToAction("Setting_TimeOffice_Shift", "Setting_TimeOffice_Shift");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region MyRegion
        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 52;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_ShiftId_Encrypted", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var data = DapperORM.DynamicList("sp_List_Atten_Shifts", param);
                ViewBag.GetTimeAndAttendanceList = data;
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
        public ActionResult Delete(string ShiftId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                param.Add("@p_process", "Delete");
                param.Add("@p_ShiftId_Encrypted", ShiftId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_Shifts", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_TimeOffice_Shift");
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