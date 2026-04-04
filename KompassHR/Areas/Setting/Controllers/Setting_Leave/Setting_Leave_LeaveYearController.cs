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
    public class Setting_Leave_LeaveYearController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: LeaveSetting/LeaveYear
        #region LeaveYear main View
        [HttpGet]
        public ActionResult Setting_Leave_LeaveYear(string LeaveYearID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 46;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Leave_Year Leave_leaveyear = new Leave_Year();

                param.Add("@query", "Select CompanyId As Id,CompanyName As Name from Mas_CompanyProfile Where Deactivate=0");
                var listMas_CompanyProfile = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetCompanyname = listMas_CompanyProfile;
                if (LeaveYearID_Encrypted != null)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    paramList.Add("@p_LeaveYearID_Encrypted", LeaveYearID_Encrypted);
                    Leave_leaveyear = DapperORM.ReturnList<Leave_Year>("sp_List_Leave_Year", paramList).FirstOrDefault();
                    TempData["FromDate"] = Leave_leaveyear.FromDate;
                    TempData["ToDate"] = Leave_leaveyear.ToDate;
                }
                
                return View(Leave_leaveyear);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Isvalidation
        [HttpGet]
        public ActionResult IsLeaveYearExists(DateTime FromDate, DateTime ToDate,string LeaveYearID_Encrypted,double CmpID)
        {
            try
            {
                    if (Session["EmployeeId"] == null)
                    {
                        return RedirectToAction("Login", "Login", new { Area = "" });
                    }
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_LeaveYearID_Encrypted", LeaveYearID_Encrypted);
                    param.Add("@p_FromDate", FromDate);
                    param.Add("@p_ToDate", ToDate);
                    param.Add("@p_CmpID", CmpID);                 
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Leave_Year", param);
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
        public ActionResult SaveUpdate(Leave_Year LeaveYear)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(LeaveYear.LeaveYearID_Encrypted) ? "Save" : "Update");
                param.Add("@p_LeaveYearID", LeaveYear.LeaveYearID);
                param.Add("@p_LeaveYearID_Encrypted", LeaveYear.LeaveYearID_Encrypted);
                param.Add("@p_FromDate", LeaveYear.FromDate);
                param.Add("@p_ToDate", LeaveYear.ToDate);
                param.Add("@p_CmpID", LeaveYear.CmpID);
                param.Add("@p_IsActivate", LeaveYear.IsActivate);
                param.Add("@p_IsDefault", LeaveYear.IsDefault);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Leave_Year", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Leave_LeaveYear", "Setting_Leave_LeaveYear");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetLisView
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 46;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_LeaveYearID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Leave_Year", param);
                ViewBag.GetLeaveYearList = data;
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
        public ActionResult Delete(string LeaveYearID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_LeaveYearID_Encrypted", LeaveYearID_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_Leave_Year", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Leave_LeaveYear");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion        

        public ActionResult GetDate(DateTime start)
        {
            try
            {
                // Add 1 year, then subtract 1 month
                var targetDate = start.AddYears(1).AddMonths(-1);
                // Get last day of that month
                var monthEnd = new DateTime(targetDate.Year, targetDate.Month, DateTime.DaysInMonth(targetDate.Year, targetDate.Month));
                return Json(new { SetDate = monthEnd.ToString("yyyy-MM-dd") }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }



        //public ActionResult GetDate(DateTime start)
        //{
        //    try
        //    {
        //        var MonthYear = start.ToString("yyyy-MM-dd");
        //        var Month = start.ToString("MM");
        //        if(Month=="04")
        //        {
        //            DynamicParameters paramDate = new DynamicParameters();
        //            var GetMonthYear = DapperORM.DynamicQuerySingle(" SELECT DATEFROMPARTS(YEAR(DATEADD(yy,DATEDIFF(yy, 0,'" + MonthYear + "')+1,+1)),3,31) as SetDate;");
        //            return Json(GetMonthYear, JsonRequestBehavior.AllowGet);
        //        }
        //        else if(Month == "01")
        //        {
        //            DynamicParameters paramDate = new DynamicParameters();
        //            var GetMonthYear = DapperORM.DynamicQuerySingle("SELECT DATEFROMPARTS(YEAR('" + MonthYear + "'),12,31);");
        //            return Json(GetMonthYear, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(false, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
        //}
    }
}