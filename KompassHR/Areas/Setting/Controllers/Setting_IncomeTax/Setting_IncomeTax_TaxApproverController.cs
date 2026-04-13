using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using KompassHR.Areas.Setting.Models.Setting_IncomeTax;
using System.Data;
using System.Net;
using System.Data.SqlClient;
namespace KompassHR.Areas.Setting.Controllers.Setting_IncomeTax
{
    public class Setting_IncomeTax_TaxApproverController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_IncomeTax_TaxApprover
        public ActionResult Setting_IncomeTax_TaxApprover(string TaxApproverId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 522;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var GetEmployee = new BulkAccessClass().AllEmployeeName();
                ViewBag.AllEmployeeName = GetEmployee;

                param.Add("@query", "select  BranchId as Id ,BranchName as Name from Mas_Branch where Deactivate=0");
                var Location = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.AllBranchName = Location;

                ViewBag.AddUpdateTitle = "Add";

                TaxApprover TaxApprover = new TaxApprover();

                if (TaxApproverId_Encrypted != null)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    param1.Add("@p_TaxApproverId_Encrypted", TaxApproverId_Encrypted);
                    TaxApprover = DapperORM.ReturnList<TaxApprover>("sp_List_TaxApprover", param1).FirstOrDefault();
                }
                return View(TaxApprover);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region IsValidation
        [HttpGet]
        public ActionResult IsApproverExists(int EmployeeID, string TaxApproverId_Encrypted, string BranchName)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_TaxApproverEmployeeID", EmployeeID);
                param.Add("@p_TaxApproverId_Encrypted", TaxApproverId_Encrypted);
                param.Add("@p_Location", BranchName);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_TaxApprover", param);
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
        public ActionResult SaveUpdate(TaxApprover TaxApprover)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(TaxApprover.TaxApproverId_Encrypted) ? "Save" : "Update");
                param.Add("@P_TaxApproverId_Encrypted", TaxApprover.TaxApproverId_Encrypted);
                param.Add("@p_TaxApproverEmployeeID", TaxApprover.TaxApproverEmployeeID);
                param.Add("@p_CompanyMobileNo", TaxApprover.CompanyMobileNo);
                param.Add("@p_CompanyEmailID", TaxApprover.CompanyEmailID);
                param.Add("@p_Location", TaxApprover.BranchName);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_TaxApprover", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_IncomeTax_TaxApprover", "Setting_IncomeTax_TaxApprover");

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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 522;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_TaxApproverId_Encrypted", "List");
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ReturnList<TaxApprover>("sp_List_TaxApprover", param).ToList();
                ViewBag.GetTaxApproverList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        [HttpGet]
        public ActionResult Delete(string TaxApproverId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_TaxApproverId_Encrypted", TaxApproverId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_TaxApprover", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_IncomeTax_TaxApprover", new { Area = "Setting" });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Company
        [HttpGet]
        public ActionResult Company(double EmployeeID)
        {
            try
            {

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "select CompanyMobileNo from Mas_Employee where EmployeeId= " + EmployeeID + " and Deactivate=0");
                var CompanyMobileNo = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param1).FirstOrDefault();

                param1.Add("@query", "select CompanyMailID from Mas_Employee where EmployeeId= " + EmployeeID + " and Deactivate=0");
                var CompanyMailID = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param1).FirstOrDefault();



                return Json(new { success = true, CompanyMobileNo = CompanyMobileNo?.CompanyMobileNo ?? "", CompanyMailID = CompanyMailID?.CompanyMailID ?? "" }, JsonRequestBehavior.AllowGet);


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