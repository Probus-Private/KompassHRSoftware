using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_GeneralReport
{
    public class Reports_GeneralReport_RecordCountController : Controller
    {
        // GET: Reports/Reports_GeneralReport_RecordCount
       
        public ActionResult Reports_GeneralReport_RecordCount()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                var data = DapperORM.ExecuteSP<dynamic>("SP_RecordCount").ToList();
                ViewBag.RecordList = data;
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