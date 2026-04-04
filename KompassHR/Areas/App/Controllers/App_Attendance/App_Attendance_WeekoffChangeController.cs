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
    public class App_Attendance_WeekoffChangeController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: App/App_Attendance_WeekoffChange
        public ActionResult App_Attendance_WeekoffChange(string WOID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 146;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { Area = "App" });
                //}

                ViewBag.AddUpdateTitle = "Add";
                Atten_WO TMS_Project = new Atten_WO();
                if (WOID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_WOID_Encrypted", WOID_Encrypted);
                    TMS_Project = DapperORM.ReturnList<Atten_WO>("sp_List_Atten_WO", param).FirstOrDefault();
                    TempData["WO_Date"] = TMS_Project.WO_Date.ToString("yyyy-MM-dd");
                }
                return View(TMS_Project);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }

        public ActionResult App_GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 146;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { Area = "App" });
                //}
                param.Add("@P_WOID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Atten_WO", param);
                ViewBag.GetAttenWOList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }

        }

        public ActionResult App_IsAttenWOExists(string WODate, string Remark, string WOID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 146;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { Area = "App" });
                //}
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {

                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_WO_EmployeeId", Session["EmployeeId"]);
                    param.Add("@p_WO_Date", WODate);
                    param.Add("@p_Remark", Remark);
                    param.Add("@p_WOID_Encrypted", WOID_Encrypted);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_WO", param);
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
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        [HttpPost]
        public ActionResult App_SaveUpdate(Atten_WO wo)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 146;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { Area = "App" });
                //}
                param.Add("@p_process", string.IsNullOrEmpty(wo.WOID_Encrypted) ? "Save" : "Update");
                param.Add("@p_WOID", wo.WOID);
                param.Add("@p_WOID_Encrypted", wo.WOID_Encrypted);
                param.Add("@p_WO_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_WO_Date", wo.WO_Date);
                param.Add("@p_Remark", wo.Remark);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_WO", param);
                var msg = param.Get<string>("@p_msg");
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("App_Attendance_WeekoffChange", "App_Attendance_WeekoffChange");

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }

        public ActionResult App_Delete(string WOID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 146;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { Area = "App" });
                //}
                param.Add("@p_process", "Delete");
                param.Add("@p_WOID", WOID);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_WO", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("App_GetList", "App_Attendance_WeekoffChange");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }

        public ActionResult App_AttendanceWODetails(string WOID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 146;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { Area = "App" });
                //}
                param.Add("@P_WOID_Encrypted", WOID_Encrypted);
                var data = DapperORM.DynamicList("sp_List_Atten_WO", param);
                ViewBag.GetAttenWOList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }

    }
}