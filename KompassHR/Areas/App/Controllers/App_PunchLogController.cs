using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.App.Controllers
{
    public class App_PunchLogController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        // GET: App/App_PunchLog
        public ActionResult App_PunchLog(string fromDate, string toDate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                DynamicParameters param = new DynamicParameters();

                // Get employee ID from session
                param.Add("@p_EmployeeID", Session["EmployeeId"]);

                // Parse or default to today's date
                DateTime from = DateTime.Today;
                DateTime to = DateTime.Today;

                if (!string.IsNullOrEmpty(fromDate))
                {
                    DateTime.TryParse(fromDate, out from);
                }

                if (!string.IsNullOrEmpty(toDate))
                {
                    DateTime.TryParse(toDate, out to);
                }

                param.Add("@p_FromDate", from);
                param.Add("@p_Todate", to);

                // Call stored procedure
                var GetLog = DapperORM.ExecuteSP<dynamic>("Sp_App_Get_AttendanceLog", param).ToList();
                if (GetLog.Count > 0)
                {
                    ViewBag.GetPunchLog = GetLog;
                    // Optional: pass default dates to the view
                    ViewBag.FromDate = from.ToString("yyyy-MM-dd");
                    ViewBag.ToDate = to.ToString("yyyy-MM-dd");
                }
                else
                {
                    if (fromDate != null)
                    {
                        ViewBag.FromDate = from.ToString("yyyy-MM-dd");
                        ViewBag.ToDate = to.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        DateTime from1 = DateTime.Today;
                        DateTime to1 = DateTime.Today;
                        ViewBag.FromDate = from1.ToString("yyyy-MM-dd");
                        ViewBag.ToDate = to1.ToString("yyyy-MM-dd");
                    }
                    ViewBag.GetPunchLog = null;
                }
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }

        //[HttpPost]
        //public ActionResult App_PunchLog(string fromDate, string toDate)
        //{
        //    try
        //    {
        //        DynamicParameters param = new DynamicParameters();

        //        // Get employee ID from session
        //        param.Add("@p_EmployeeID", Session["EmployeeId"]);

        //        // Parse or default to today's date
        //        DateTime from = DateTime.Today;
        //        DateTime to = DateTime.Today;

        //        if (!string.IsNullOrEmpty(fromDate))
        //        {
        //            DateTime.TryParse(fromDate, out from);
        //        }

        //        if (!string.IsNullOrEmpty(toDate))
        //        {
        //            DateTime.TryParse(toDate, out to);
        //        }

        //        param.Add("@p_FromDate", from);
        //        param.Add("@p_Todate", to);

        //        // Call stored procedure
        //        var GetLog = DapperORM.ExecuteSP<dynamic>("Sp_App_Get_AttendanceLog", param).ToList();
        //        if (GetLog.Count > 0)
        //        {
        //            ViewBag.GetPunchLog = GetLog;
        //            // Optional: pass default dates to the view
        //            ViewBag.FromDate = from.ToString("yyyy-MM-dd");
        //            ViewBag.ToDate = to.ToString("yyyy-MM-dd");
        //        }
        //        else
        //        {
        //            if (fromDate != null)
        //            {
        //                ViewBag.FromDate = from.ToString("yyyy-MM-dd");
        //                ViewBag.ToDate = to.ToString("yyyy-MM-dd");
        //            }
        //            else
        //            {
        //                DateTime from1 = DateTime.Today;
        //                DateTime to1 = DateTime.Today;
        //                ViewBag.FromDate = from1.ToString("yyyy-MM-dd");
        //                ViewBag.ToDate = to1.ToString("yyyy-MM-dd");
        //            }
        //            ViewBag.GetPunchLog = null;
        //        }
        //        return View();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        [HttpGet]
        public ActionResult App_ShowMap()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }


        [HttpPost]
        public ActionResult App_ShowMap(string Lat, string Long, string LogDate, string LogTime, string Direction, string LocationName, string Remarks, string Origin, string SelfiePath)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                TempData["Latitude"] = Lat;
                TempData["Longitude"] = Long;
                TempData["LogDate"] = LogDate;
                TempData["LogTime"] = LogTime;
                TempData["Direction"] = Direction;
                TempData["Remarks"] = Remarks;
                TempData["Origin"] = Origin;
                TempData["SelfiePath"] = SelfiePath;
                TempData["LocationName"] = LocationName;
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