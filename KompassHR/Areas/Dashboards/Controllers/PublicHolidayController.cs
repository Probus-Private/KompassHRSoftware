using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Dashboards.Controllers
{
    public class PublicHolidayController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Dashboards/PublicHoliday
        public ActionResult PublicHoliday()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters PublicHoliday = new DynamicParameters();
                PublicHoliday.Add("@p_BranchId", Session["BranchId"]);
                var HolidayList =  DapperORM.ExecuteSP<dynamic>("sp_List_Atten_PublicHoliday_ESS", PublicHoliday).ToList();
                ViewBag.GetPublicHolidaysList = HolidayList;

                DynamicParameters PublicHolidayCount = new DynamicParameters();
                PublicHolidayCount.Add("@p_BranchId", Session["BranchId"]);
                var HolidayCount = DapperORM.ExecuteSP<dynamic>("sp_List_Atten_PublicHoliday_ESS_Count", PublicHolidayCount).ToList();
                ViewBag.GetPublicHolidayscount = HolidayCount;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
    }
}