using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Tax;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using Microsoft.ReportingServices.DataProcessing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_Attendance
{
    public class Reports_TaxSheetController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        //GET: Reports/Reports_TaxSheet
        public ActionResult Reports_TaxSheet(IncomeTax_Fyear ObjIncomeTax_Fyear)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            // CHECK IF USER HAS ACCESS OR NOT
            int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 576;
            bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
            if (!CheckAccess)
            {
                Session["AccessCheck"] = "False";
                return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
            }
            DynamicParameters param = new DynamicParameters();
            param.Add("@query", "Select TaxFyearId as Id,TaxYear As Name  from IncomeTax_Fyear where Deactivate=0 and [IsActive]=1");
            var GetTaxFyear = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param).ToList();
            ViewBag.GetTaxFyear = GetTaxFyear;

            DynamicParameters MulQuery = new DynamicParameters();
            MulQuery.Add("@p_EmployeeID", Convert.ToInt64(Session["EmployeeId"]));
            MulQuery.Add("@p_FYearID", ObjIncomeTax_Fyear.TaxFyearId);
            using (var multi = DapperORM.DynamicMultipleResultList("sp_Tax_Sheet", MulQuery))
            {
                ViewBag.AttendanceDetails = multi.Read<dynamic>().ToList();
                //ViewBag.AttendanceDetails = multi.Read<dynamic>().ToList().OrderByDescending(x => x.AcHeadName).ToList();
                ViewBag.SalaryDetails = multi.Read<dynamic>().ToList();
                ViewBag.OtherDetails = multi.Read<dynamic>().ToList();
                ViewBag.PersonalDetails = multi.Read<dynamic>().FirstOrDefault();
                ViewBag.TaxDetails = multi.Read<dynamic>().FirstOrDefault();
            }
            return View();
        }
    }
}