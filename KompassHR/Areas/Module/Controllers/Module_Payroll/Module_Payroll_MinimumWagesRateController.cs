using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Mime;
using KompassHR.Models;
using KompassHR.Areas.Module.Models.Module_Payroll;
using System.Text;

namespace KompassHR.Areas.Module.Controllers.Module_Payroll
{
    public class Module_Payroll_MinimumWagesRateController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        #region Module_Payroll_MinimumWagesRate Main View 
        [HttpGet]
        public ActionResult Module_Payroll_MinimumWagesRate(string MinimumWageRatetId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 834;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";

                //GET COMPANY NAME
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                var CMPID = GetComapnyName[0].Id;
                //GET BRANCH NAME
                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CMPID), Convert.ToInt32(Session["EmployeeId"]));
                ViewBag.BranchName = Branch;

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "Select WageCategoryId as Id ,WageCategoryName as Name from Mas_WageCategory where Deactivate=0 order by Name");
                ViewBag.WageCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();

                Payroll_MinimumWagesRate Payroll_MinimumWagesRate = new Payroll_MinimumWagesRate();
                if (MinimumWageRatetId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_MinimumWageRatetId_Encrypted", MinimumWageRatetId_Encrypted);
                    Payroll_MinimumWagesRate = DapperORM.ReturnList<Payroll_MinimumWagesRate>("sp_List_MinimumWageRate", param).FirstOrDefault();
                    TempData["FromDate"] = Payroll_MinimumWagesRate.FromDate.ToString("yyyy-MM-dd");
                    TempData["ToDate"] = Payroll_MinimumWagesRate.ToDate.ToString("yyyy-MM-dd");
                }
                return View(Payroll_MinimumWagesRate);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList Main View 
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 834;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_MinimumWageRatetId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_MinimumWageRate", param).ToList();
                ViewBag.GetMinimumWageRateList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region MinimumWageRate
        public ActionResult IsMinimumWageRateExists(double CmpId, double BranchId, double WageCategoryId, string MinimumWageRatetId_Encrypted, float Basic, float DA)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_CmpId", CmpId);
                    param.Add("@p_BranchId", BranchId);
                    param.Add("@p_MinimumWageRatetId_Encrypted", MinimumWageRatetId_Encrypted);
                    param.Add("@p_WageCategoryId", WageCategoryId);
                    param.Add("@p_Basic", Basic);
                    param.Add("@p_DA", DA);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_MinimumWageRate", param);
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
        public ActionResult SaveUpdate(Payroll_MinimumWagesRate MinimumWageRate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                // 1. Get last active entry
                DynamicParameters paramPrev = new DynamicParameters();
                paramPrev.Add("@query", "SELECT TOP 1 * FROM Payroll_MinimumWageRate WHERE CmpID='"+ MinimumWageRate.CmpId + "' AND BranchId='"+ MinimumWageRate.BranchId + "' AND WageCategoryId='"+ MinimumWageRate.WageCategoryId + "' AND Deactivate=0 ORDER BY FromDate DESC");
               
                var lastEntry = DapperORM.ReturnList<dynamic>("sp_QueryExcution", paramPrev).FirstOrDefault();

                StringBuilder sb = new StringBuilder();

                if (lastEntry != null && string.IsNullOrEmpty(MinimumWageRate.MinimumWageRatetId_Encrypted))
                {
                    DateTime from = MinimumWageRate.FromDate;
                    DateTime NewToDate = new DateTime(from.Year, from.Month, 1).AddDays(-1);
                    sb.Append("UPDATE Payroll_MinimumWageRate SET ToDate ='"+ NewToDate + "' where MinimumWageRatetId='"+ lastEntry.MinimumWageRatetId + "'");
                }

                string dummy = "";
                if (sb.Length > 0)
                {
                    objcon.SaveStringBuilder(sb, out dummy);
                }

                param.Add("@p_process", string.IsNullOrEmpty(MinimumWageRate.MinimumWageRatetId_Encrypted) ? "Save" : "Update");
                param.Add("@p_MinimumWageRatetId_Encrypted", MinimumWageRate.MinimumWageRatetId_Encrypted);
                param.Add("@p_CmpId", MinimumWageRate.CmpId);
                param.Add("@p_BranchId", MinimumWageRate.BranchId);
                param.Add("@p_WageCategoryId", MinimumWageRate.WageCategoryId);
                param.Add("@p_Basic", MinimumWageRate.Basic);
                param.Add("@p_DA", MinimumWageRate.DA);
                param.Add("@p_FromDate", MinimumWageRate.FromDate);
                param.Add("@p_ToDate", MinimumWageRate.ToDate);

                param.Add("@p_MachineName", Dns.GetHostName());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);

                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                var result = DapperORM.ExecuteReturn("sp_SUD_MinimumWageRate", param);

                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                return RedirectToAction("Module_Payroll_MinimumWagesRate", "Module_Payroll_MinimumWagesRate");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(int? MinimumWageRatetId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_MinimumWageRatetId", MinimumWageRatetId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_MinimumWageRate", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Module_Payroll_MinimumWagesRate");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Get Branch Name
        [HttpGet]
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CmpId), Convert.ToInt32(Session["EmployeeId"]));
                return Json(new { Branch = Branch }, JsonRequestBehavior.AllowGet);
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