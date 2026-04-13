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
    public class Rpt_Attendance_OddPunchController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        // GET: Reports/Rpt_Attendance_OddPunch

        #region Rpt_Attendance_OddPunch
        public ActionResult Rpt_Attendance_OddPunch(Atten_LogNotFound OBJOddPunch)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if(OBJOddPunch.EmployeeCardNo!=null)
                {
                    param = new DynamicParameters();
                    param.Add("@p_EmployeeCardNo", OBJOddPunch.EmployeeCardNo);                   
                    var data = DapperORM.ExecuteSP<dynamic>("sp_RptOddPunch", param).ToList();
                    ViewBag.OddPunchReport = data;
                }
                else
                {
                    ViewBag.OddPunchReport = "";
                }             

                return View();
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