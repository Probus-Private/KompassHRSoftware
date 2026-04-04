using Dapper;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_Attendance
{
    public class Rpt_MonthWise_AttendanceController : Controller
    {
        // GET: Reports/Rpt_MonthWise_Attendance
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        public ActionResult Rpt_MonthWise_Attendance(MonthWiseReportFilter ObjMonth)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var Getmonth = ObjMonth.Month.ToString("MM");
                var Getyear = ObjMonth.Month.ToString("yyyy");
                int month = Convert.ToInt32(Getmonth);
                int year = Convert.ToInt32(Getyear);
                ViewBag.GetDays = DateTime.DaysInMonth(year, month);

                //if (FromDate != null || ToDate != null)
                //{
                //    param.Add("@p_EmployeeBranchId", Session["EmployeeBranchId"]);
                //    param.Add("@p_FromDate", FromDate);
                //    param.Add("@p_ToDate", ToDate);
                //    var data = DapperORM.ExecuteSP<dynamic>("sp_RptAttendanceDepartmentWiseDailyHeadCount", param).ToList();
                //    ViewBag.DepartmentWiseReport = data;
                //    TempData["FromDate"] = string.Format("{0:yyyy-MM-dd}", FromDate);
                //    TempData["ToDate"] = string.Format("{0:yyyy-MM-dd}", ToDate);
                //}
                //else
                //{
                //    ViewBag.DepartmentWiseReport = "";
                //}
                return View(ObjMonth);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
    }
}