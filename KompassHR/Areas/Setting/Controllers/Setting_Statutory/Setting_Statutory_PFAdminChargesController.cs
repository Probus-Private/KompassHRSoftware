using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Statutory;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Statutory
{
    public class Setting_Statutory_PFAdminChargesController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        // GET: Setting/Setting_Statutory_PFAdminCharges
        #region Setting_Statutory_PFAdminCharges
        public ActionResult Setting_Statutory_PFAdminCharges(string PFAdminChargesId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 450;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Payroll_PFAdminCharges PFAdminCharge = new Payroll_PFAdminCharges();
                if (PFAdminChargesId_Encrypted != null)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    paramList.Add("@p_PFAdminChargesId_Encrypted", PFAdminChargesId_Encrypted);
                    PFAdminCharge = DapperORM.ReturnList<Payroll_PFAdminCharges>("sp_List_Payroll_PFAdminCharges", paramList).FirstOrDefault();
                    TempData["FromDate"] = PFAdminCharge.FromDate.ToString("yyyy-MM-dd");
                    TempData["ToDate"] = PFAdminCharge.ToDate.ToString("yyyy-MM-dd");
                }
                else
                {
                    TempData["FromDate"] = null;
                    TempData["ToDate"] = null;
                }
                return View(PFAdminCharge);
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
        public ActionResult IsPFAdminExists(DateTime FromDate, string PFAdminChargesId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "IsValidation");
                param.Add("@p_PFAdminChargesId_Encrypted", PFAdminChargesId_Encrypted);
                param.Add("@p_FromDate", FromDate);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_Payroll_PFAdminCharges", param);
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
        public ActionResult SaveUpdate(Payroll_PFAdminCharges ObjPFCode)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(ObjPFCode.PFAdminChargesId_Encrypted) ? "Save" : "Update");
                param.Add("@p_PFAdminChargesId_Encrypted", ObjPFCode.PFAdminChargesId_Encrypted);
                param.Add("@p_ToDate", ObjPFCode.ToDate);
                param.Add("@p_FromDate", ObjPFCode.FromDate);
                param.Add("@p_PFEmployee", ObjPFCode.PFEmployee);
                param.Add("@p_PFEmployeer", ObjPFCode.PFEmployeer);
                param.Add("@p_PFAC1", ObjPFCode.PFAC1);
                param.Add("@p_PFAC10", ObjPFCode.PFAC10);
                param.Add("@p_PFAC2", ObjPFCode.PFAC2);
                param.Add("@p_PFAC21", ObjPFCode.PFAC21);
                param.Add("@p_PFAC22", ObjPFCode.PFAC22);
                param.Add("@p_PFAgeLimit", ObjPFCode.PFAgeLimit);
                param.Add("@p_PFCeilingLimit", ObjPFCode.PFCeilingLimit);
                param.Add("@p_EPSCeilingLimit", ObjPFCode.EPSCeilingLimit);
                param.Add("@p_EDLICeilingLimit", ObjPFCode.EDLICeilingLimit);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_Payroll_PFAdminCharges", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Statutory_PFAdminCharges", "Setting_Statutory_PFAdminCharges");
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
        public ActionResult Delete(string PFAdminChargesId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_PFAdminChargesId_Encrypted", PFAdminChargesId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_Payroll_PFAdminCharges", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Statutory_PFAdminCharges");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 94;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_PFAdminChargesId_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Payroll_PFAdminCharges", param);
                ViewBag.GetPayrollPFList = data;
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