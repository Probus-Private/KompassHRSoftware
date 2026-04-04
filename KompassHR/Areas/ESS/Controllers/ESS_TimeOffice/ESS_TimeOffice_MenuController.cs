using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_TimeOffice
{
   
    public class ESS_TimeOffice_MenuController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: ESS/ESS_TimeOffice_Menu
        #region ESS_TimeOffice_Menu View
        [HttpGet]
        public ActionResult ESS_TimeOffice_Menu(int? id,int? ScreenId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 150;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                if (ScreenId != null)
                {
                    Session["ModuleId"] = id;
                    Session["ScreenId"] = ScreenId;
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), id, ScreenId, "Form", "Transation");
                    ViewBag.GetUserMenuList = GetMenuList;
                }
                else
                {
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), Convert.ToInt32(Session["ModuleId"]), Convert.ToInt32(Session["ScreenId"]), "Form", "Transation");
                     ViewBag.GetUserMenuList = GetMenuList;
                }
                
                DynamicParameters paramCount = new DynamicParameters();
                paramCount.Add("@p_Employeeid", Session["EmployeeId"]);
                var GetCount = DapperORM.ExecuteSP<dynamic>("sp_AttendanceDasboardCount", paramCount).ToList(); // SP_getReportingManager
                TempData["CountWorkFromHome"] = GetCount[1].RequestCount;
                TempData["CountShiftChange"] = GetCount[2].RequestCount;
                TempData["CountPersonalGatepass"] = GetCount[3].RequestCount;
                TempData["CountOutDoorCompany"] = GetCount[4].RequestCount;
                TempData["CountPunchMissing"] = GetCount[5].RequestCount;
                TempData["ApprovelRequestCount"] = GetCount[0].RequestCount;

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