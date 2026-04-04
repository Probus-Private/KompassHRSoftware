using Dapper;
using KompassHR.Areas.ESS.Models.ESS_FNF;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_FNF
{
    public class ESS_FNF_FNFLeaveCalculationController : Controller
    {
        // GET: ESS_FNF_FNFLeaveCalculation
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        GetMenuList ClsGetMenuList = new GetMenuList();

        #region LeaveCalculation main View
        [HttpGet]
        public ActionResult ESS_FNF_FNFLeaveCalculation(FNF_Calculation FNF_Calculation)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 608;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";

                param = new DynamicParameters();
                param.Add("@query", "select LeaveYearID as Id, cast(year(FromDate) as nvarchar(4))+'-'+cast(YEAR(ToDate) as nvarchar(4)) as Name from[dbo].[Leave_Year] where Deactivate = 0  and IsActivate=1  and CmpId='" + Session["CompanyId"] + "' order by IsDefault desc,FromDate desc");
                var LeaveYearGet = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetLeaveYear = LeaveYearGet;

                ViewBag.GetProjectProgressReport = "";
                return View(FNF_Calculation);
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