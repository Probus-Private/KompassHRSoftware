using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_TimeOffice
{
    public class ESS_TimeOffice_AttendanceRegularizationController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_TimeOffice_AttendanceRegularization
        #region Main Page ttendanceRegularization
        public ActionResult ESS_TimeOffice_AttendanceRegularization()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 156;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                //param.Add("@p_EmployeeId", Session["EmployeeId"]);
                //param.Add("@p_FromDate", Session["FromDate"]);
                //param.Add("@p_ToDate", Session["ToDate"]);
                //var data = DapperORM.ExecuteSP<dynamic>("sp_ESS_Rpt_AttenRegularization_Summary", param).ToList();
                //ViewBag.AttendanceRegularization = data;

                DynamicParameters CalenderEmp = new DynamicParameters();
                CalenderEmp.Add("@p_EmployeeId", Convert.ToInt32(Session["EmployeeId"]));
                ViewBag.GetEmployeeList = DapperORM.ReturnList<AllDropDownBind>("sp_GetAttendanceCalender_Employee", CalenderEmp).ToList();

                //DynamicParameters EmployeeName = new DynamicParameters();
                //EmployeeName.Add("@query", $@"SELECT DISTINCT Mas_Employee.EmployeeId as Id, Mas_Employee.EmployeeName as Name  
                //                            FROM dbo.Mas_Employee LEFT JOIN Mas_Employee_Reporting
                //                                ON Mas_Employee_Reporting.ReportingEmployeeID = Mas_Employee.EmployeeId
                //                                WHERE
                //                                Mas_Employee.Deactivate = 0
                //                                AND Mas_Employee.ContractorID = 1
                //                                AND Mas_Employee.EmployeeLeft = 0
                //                                AND Mas_Employee_Reporting.Deactivate = 0
                //                                AND(Mas_Employee.EmployeeId = {Session["EmployeeId"]} OR Mas_Employee_Reporting.ReportingManager1 = {Session["EmployeeId"]})");
                //var GetEmployee = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", EmployeeName).ToList();
                //ViewBag.GetEmployeeList = GetEmployee;

                var ManagerId1 = Session["ManagerId1"];
                var ManagerId2 = Session["ManagerId2"];
                var HRId = Session["HRId"];

                var data1 = DapperORM.DynamicQueryList("Select Name,MobileNo,Type, Designation from Mas_CompanyEmergencyContact where Deactivate=0").FirstOrDefault();
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Get PhotoBase64Convert
        public ActionResult FilterAttendanceRegularization(DateTime FromDate, DateTime ToDate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                Session["FromDate"] = FromDate;
                Session["ToDate"] = ToDate;
                return RedirectToAction("ESS_TimeOffice_AttendanceRegularization", "ESS_TimeOffice_AttendanceRegularization", new { area = "ESS" });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Calender
        [HttpPost]
        public ActionResult GetCalendarData(string Date, int? EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DateTime date = DateTime.Parse(Date);
                int Month = date.Month;
                var Year = date.Year;
                DynamicParameters Calender = new DynamicParameters();
                Calender.Add("@p_EmployeeId", EmployeeId ?? Convert.ToInt32(Session["EmployeeId"]));
                Calender.Add("@p_Month", Month);
                Calender.Add("@p_Year", Year);
                var GetCalendarData = DapperORM.ExecuteSP<AttendanceRegularization>("sp_GetAttendanceCalender_Month", Calender).ToList();
                return Json(GetCalendarData, JsonRequestBehavior.AllowGet);

                //var DefaultAttenShow = DapperORM.DynamicQuerySingle("Select EM_Atten_DefaultAttenShow from Mas_Employee_Attendance where AttendanceEmployeeId=" + Session["EmployeeId"] + " and Deactivate=0");
                //var Check = DefaultAttenShow?.EM_Atten_DefaultAttenShow;
                //if (Check == true)
                //{
                //    param.Add("@p_EmployeeId", Session["EmployeeId"]);
                //    param.Add("@p_Month", Month);
                //    param.Add("@p_Year", Year);
                //    var GetCalendarData = DapperORM.ExecuteSP<AttendanceRegularization>("sp_GetAttendanceCalender_Month", param).ToList();
                //    return Json(GetCalendarData, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    return Json(false, JsonRequestBehavior.AllowGet);
                //}


                //if (Check == true)
                //{
                //    param.Add("@EmployeeCode", Session["EmployeeCardNo"]);
                //    param.Add("@Date", Date);
                //    var GetCalendarData = DapperORM.ExecuteSP<AttendanceRegularization>("SP_ESS_CalenderDashboard_ShowAllData", param).ToList();
                //    return Json(GetCalendarData, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    param.Add("@EmployeeCode", Session["EmployeeCardNo"]);
                //    param.Add("@Date", Date);
                //    var GetCalendarData = DapperORM.ExecuteSP<AttendanceRegularization>("SP_ESS_CalenderDashboard_ShowOnlyLogVerify", param).ToList();
                //    return Json(GetCalendarData, JsonRequestBehavior.AllowGet);
                //}
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