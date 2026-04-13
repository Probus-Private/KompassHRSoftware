using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Prime;
using KompassHR.Areas.Setting.Models.Setting_Statutory;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Statutory
{
    public class Setting_Statutory_ESICContributionController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        // GET: Setting/Setting_Statutory_ESICContribution
        #region Setting_Statutory_ESICContribution
        public ActionResult Setting_Statutory_ESICContribution(string ESICContribution_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 449;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Payroll_ESICContribution ESICContri = new Payroll_ESICContribution();
                if (ESICContribution_Encrypted != null)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    paramList.Add("@p_ESICContribution_Encrypted", ESICContribution_Encrypted);
                    ESICContri = DapperORM.ReturnList<Payroll_ESICContribution>("sp_List_Payroll_ESICContribution", paramList).FirstOrDefault();
                    TempData["FromDate"] = ESICContri.FromDate.ToString("yyyy-MM-dd");
                    TempData["ToDate"] = ESICContri.ToDate.ToString("yyyy-MM-dd");
                }
                else
                {
                    TempData["FromDate"] = null;
                    TempData["ToDate"] = null;
                }
                return View(ESICContri);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsValidation
        [HttpGet]
        public ActionResult IsESICContributionExists(DateTime FromDate, string ESICContribution_Encrypted)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_ESICContribution_Encrypted", ESICContribution_Encrypted);
                param.Add("@p_FromDate", FromDate);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_ESICContribution", param);
                var Message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                if (Message != "")
                {
                    return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Payroll_ESICContribution payrollESIC)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(payrollESIC.ESICContribution_Encrypted) ? "Save" : "Update");
                param.Add("@p_ESICContributionId", payrollESIC.ESICContributionId);
                param.Add("@p_ESICContribution_Encrypted", payrollESIC.ESICContribution_Encrypted);
                param.Add("@p_FromDate", payrollESIC.FromDate);
                param.Add("@p_ToDate", payrollESIC.ToDate);
                param.Add("@p_ESICEmployee", payrollESIC.ESICEmployee);
                param.Add("@p_ESICEmployer", payrollESIC.ESICEmployer);
                param.Add("@p_ESICLimit", payrollESIC.ESICLimit);
                param.Add("@p_ESICLimitForPhysicalHandicapped", payrollESIC.ESICLimitForPhysicalHandicapped);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_ESICContribution", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Statutory_ESICContribution", "Setting_Statutory_ESICContribution");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 95;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_ESICContribution_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Payroll_ESICContribution", param);
                ViewBag.GetPayrollESICList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region MyRegion
        [HttpGet]
        public ActionResult Delete(string ESICContribution_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_ESICContribution_Encrypted", ESICContribution_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_ESICContribution", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Statutory_ESICContribution");

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