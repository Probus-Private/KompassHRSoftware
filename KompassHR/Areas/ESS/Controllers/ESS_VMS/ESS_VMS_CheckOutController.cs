using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_VMS
{
    public class ESS_VMS_CheckOutController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_VMS_CheckOut
        #region ESS_VMS_CheckOut
        public ActionResult ESS_VMS_CheckOut()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 187;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_HostEmployeeId", "" + Session["EmployeeId"] + "");
                var VisitorCheckinoutList = DapperORM.DynamicList("sp_List_Visitor_HostOut", param);
                ViewBag.VisitorCheckout = VisitorCheckinoutList;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region UpdateVisitorCheckinout
        public ActionResult UpdateVisitorCheckinout(string VisitorId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param = new DynamicParameters();
                param.Add("@query", "update Visitor_Tra_Master set HostOutTime = getdate()  where VisitorId_Encrypted ='" + VisitorId_Encrypted + "'");
                var Module = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).ToList();
                TempData["Message"] = "Check Out Successfully";
                TempData["Icon"] = "success";
                return RedirectToAction("ESS_VMS_CheckOut", "ESS_VMS_CheckOut");
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