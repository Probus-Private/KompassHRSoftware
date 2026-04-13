using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_AppointmentLetterController : Controller
    {
        DynamicParameters paramList = new DynamicParameters();

        // GET: Module/Module_Employee_AppointmentLetter
        public ActionResult Module_Employee_AppointmentLetter()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 513;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var GetEmployee = new BulkAccessClass().GetAllEmployeeName();
                ViewBag.AllEmployeeName = GetEmployee;

                return View();

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult GetEmployeeDetails(int EmployeeID)
        {
            try
            {
                paramList.Add("@p_EmployeeId", EmployeeID);
                var GetEmployeeDetails = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Employee_Appointment", paramList).ToList();
                ViewBag.GetEmployeeDetails = GetEmployeeDetails;

                return Json(GetEmployeeDetails, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
    }
}