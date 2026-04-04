using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_LMS
{
    public class ESS_LMS_Training_TrainingController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: ESS/ESS_LMS_Training_Training
        public ActionResult ESS_LMS_Training_Training(int? id, int? ScreenId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 551;
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
                var GetCount = DapperORM.ExecuteSP<dynamic>("sp_TrainingDasboardCount", paramCount).ToList(); // SP_getReportingManager
                TempData["Training_Category"] = GetCount[0].RequestCount;
                TempData["Training_SubCategory"] = GetCount[1].RequestCount;
                TempData["Training_Internal"] = GetCount[2].RequestCount;
                TempData["External_Trainer"] = GetCount[3].RequestCount;
                TempData["Training_Agency"] = GetCount[4].RequestCount;
                TempData["Training_Calender"] = GetCount[5].RequestCount;
                TempData["Training_Plan"] = GetCount[6].RequestCount;
                TempData["Training_Conduct"] = GetCount[7].RequestCount;
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