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
    public class EmergencyContactController : Controller
    {
        // GET: Dashboards/EmergencyContact
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        public ActionResult EmergencyContact()
        {
            try
            {
                param.Add("@query", "Select Name,MobileNo,Type, Designation from Mas_CompanyEmergencyContact where Deactivate=0");
                var data = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).ToList();
                ViewBag.GetCoffSettingName = data;

                //var data = DapperORM.DynamicQuerySingle("Select Name,MobileNo,Type, Designation from Mas_CompanyEmergencyContact where Deactivate=0").FirstOrDefault();
                //var data = DapperORM.DynamicList("sp_List_EmergencyContact_ESS");
                ViewBag.EmergencyContactList = data;
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