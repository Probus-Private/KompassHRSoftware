using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_VMS
{
    public class ESS_VMS_VisitorEntryController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_VMS_VisitorEntry

        #region VisitorEntry MAin view 
        public ActionResult ESS_VMS_VisitorEntry()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 188;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters paramAppoint = new DynamicParameters();
                paramAppoint.Add("@P_VisitorEmployeeID", Session["EmployeeId"]);
                var VisitorAppointmentList = DapperORM.DynamicList("sp_List_Visitor_GateAppointment", paramAppoint);
                ViewBag.VisitorAppointment = VisitorAppointmentList;

                DynamicParameters paramVisitor = new DynamicParameters();
                paramVisitor.Add("@P_VisitorEmployeeID", Session["EmployeeId"]);
                paramVisitor.Add("@p_VisitorId_Encrypted", "List");
                var VisitorList = DapperORM.DynamicList("sp_List_Visitor_InVisitor", paramVisitor);
                ViewBag.InVisitorAppointment = VisitorList;

                DynamicParameters paramCount = new DynamicParameters();
                paramCount.Add("@p_Employeeid", Session["EmployeeId"]);
                var GetCount = DapperORM.ExecuteSP<dynamic>("sp_VMSDasboardCount", paramCount).ToList(); // SP_getReportingManager
                TempData["TodayAppointment"] = GetCount[0].TodayAppointment;
                TempData["TodayInVisitor"] = GetCount[1].TodayAppointment;
                TempData["TotalOutVisitors"] = GetCount[2].TodayAppointment;
                TempData["TodayInOutVistor"] = GetCount[3].TodayAppointment;
                TempData["PersonalGatepassCount"] = GetCount[4].TodayAppointment;
                TempData["OutDoorCount"] = GetCount[5].TodayAppointment;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region PersonalGatepassList
        public ActionResult PersonalGatepassList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 188;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters paramTotalDuration = new DynamicParameters();
                paramTotalDuration.Add("@query", "Select  Minute from  Atten_PersonalGatepassSetting where Deactivate=0 and CmpId=" + Session["CompanyId"] + " and PersonalGatepassSettingBranchId=" + Session["BranchId"] + "  and PersonalGatepassSettingId=(Select EM_Atten_Atten_PersonalGatepassSettingId from Mas_Employee_Attendance where AttendanceEmployeeId=" + Session["EmployeeId"] + " and Deactivate=0)");
                var Duration = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramTotalDuration).FirstOrDefault();
                if (Duration != null)
                {
                    ViewBag.Duration = Duration.Minute;
                }
                else
                {
                    ViewBag.Duration = 0;
                }

                //param.Add("@P_Qry", "AND Tra_Approval.Status = 'Approved' AND CONVERT(date, Atten_PersonalGatepass.PersonalGatepassDate) = CONVERT(date, GETDATE())");
                param.Add("@P_Qry", " and Atten_PersonalGatepass.Status like '%Approved%' and CONVERT(date, Atten_PersonalGatepass.PersonalGatepassDate) = CONVERT(date, GETDATE()) and Atten_PersonalGatepass.VisitorGateINTime is null and Atten_PersonalGatepass.PersonalGatepassBranchId in (select userbranchmapping.BranchID from userbranchmapping where userbranchmapping.EmployeeID='" + Session["EmployeeId"] + "' and userbranchmapping.IsActive=1) "); //  and CONVERT(date, Atten_PersonalGatepass.PersonalGatepassDate) = CONVERT(date, GETDATE())//and Atten_PersonalGatepass.Visitor_INOut is null 
                var PersoanlGetpass = DapperORM.DynamicList("sp_List_Atten_PersonalGatepass", param);

                ViewBag.PersoanlGetpass = PersoanlGetpass;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region OfficialOutDoorList
        public ActionResult OfficialOutDoorList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 188;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters OfficialOutDoorList = new DynamicParameters();
                OfficialOutDoorList.Add("@P_Qry", " and Mas_Employee.deactivate=0 and (Atten_OutDoorCompany.VisitingBranchID in (select userbranchmapping.BranchID from userbranchmapping where userbranchmapping.EmployeeID='" + Session["Employeeid"] + "' and userbranchmapping.IsActive=1)  or Atten_OutDoorCompany.OutDoorCompanyBranchId in (select userbranchmapping.BranchID from userbranchmapping where userbranchmapping.EmployeeID='" + Session["Employeeid"] + "'   and userbranchmapping.IsActive=1))");
                ViewBag.OfficialOutDoorList = DapperORM.ExecuteSP<dynamic>("sp_List_OfficialOutDoorList", OfficialOutDoorList).ToList();
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region UpdateCheckOutVisitor
        public ActionResult UpdateCheckOutVisitor(int PersonalGatepassId, string InOut)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_PersonalGatepassId", PersonalGatepassId);
                param.Add("@p_Origin", InOut);
                var VisitorList = DapperORM.ExecuteSP<dynamic>("sp_Update_CheckInOutVisitor", param);
                //DapperORM.DynamicQuerySingle("Update Atten_PersonalGatepass set Visitor_INOut= 'Out' where PersonalGatepassId_Encrypted='" + PersonalGatepassId_Encrypted + "'");
                TempData["Message"] = "Record save successfully";
                TempData["Icon"] = "success";
                return RedirectToAction("PersonalGatepassList", "ESS_VMS_VisitorEntry");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region CheckOutVisitor
        [HttpGet]
        public ActionResult UpdateGardCheckOutVisitor(string VisitorIdEncrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                if (VisitorIdEncrypted != null)
                {
                    param.Add("@p_VisitorId_Encrypted", VisitorIdEncrypted);
                    param.Add("@P_VisitorEmployeeID", Session["EmployeeId"]);
                    var VisitorList = DapperORM.DynamicList("sp_List_Visitor_InVisitor", param);
                    TempData["Message"] = "Record save successfully";
                    TempData["Icon"] = "success";
                    return Json(new { Massage = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    TempData["Message"] = "Something went wrong";
                    TempData["Icon"] = "error";
                    return RedirectToAction("ESS_VMS_VisitorEntry", "ESS_VMS_VisitorEntry");
                }
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