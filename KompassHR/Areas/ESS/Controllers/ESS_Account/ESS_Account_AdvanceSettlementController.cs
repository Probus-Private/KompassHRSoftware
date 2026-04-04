using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Account
{
    public class ESS_Account_AdvanceSettlementController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_Account_AdvanceSettlement
        public ActionResult ESS_Account_AdvanceSettlement()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 656;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_AdvanceID_Encrypted", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var data = DapperORM.DynamicList("sp_List_Payroll_Advance", param);
                ViewBag.GetAdvanceList = data;
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