
using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Setting.Models.Setting_SaleForce;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_SaleForce
{
    public class Setting_SalesForce_ReportLevelController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        
        #region Main View Setting_SalesForce_ReportLevel
        // GET: Setting/Setting_SalesForce_ReportLevel
        public ActionResult Setting_SalesForce_ReportLevel(int? Fid)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 813;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";
                MAS_REPORLEVEL MAS_REPORLEVEL = new MAS_REPORLEVEL();
                //if (Fid != null)
                //{
                //    ViewBag.AddUpdateTitle = "Update";
                //    param.Add("@p_Fid", Fid);

                //    MAS_REPORLEVEL = DapperORM.ReturnList<MAS_REPORLEVEL>("sp_List_MAS_REPORLEVEL", param).FirstOrDefault();
                //}
                if (Fid != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    var param = new DynamicParameters();
                    param.Add("@query", $"SELECT * FROM MAS_REPORLEVEL WHERE Fid = {Fid} AND Deleted = 0");

                    MAS_REPORLEVEL = DapperORM.ReturnList<MAS_REPORLEVEL>("Sp_QueryExcution", param)
                                              .FirstOrDefault();
                }
                
                return View(MAS_REPORLEVEL);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region IsReportingLevelExists
        public ActionResult IsReportingLevelExists(string ReportingRole, int ReportingLever)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 813;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {

                    param.Add("@p_process", "IsValidation");
                    //param.Add("@p_Fid", Fid);
                    param.Add("@p_ReportingRole", ReportingRole);
                    param.Add("@p_ReportingLever", ReportingLever);
                    param.Add("@p_msg", dbType: System.Data.DbType.String, direction: System.Data.ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: System.Data.DbType.String, direction: System.Data.ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_MAS_REPORLEVEL", param);
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

        #region Saveupdate 

        [HttpPost]
        public ActionResult SaveUpdate(MAS_REPORLEVEL MAS_REPORLEVEL)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 813;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                if (MAS_REPORLEVEL.Fid == 0 || MAS_REPORLEVEL.Fid == null)
                {
                    param.Add("@p_process", "Save");
                    param.Add("@P_Fid", MAS_REPORLEVEL.Fid);
                    // param.Add("@P_Encrypted_Id", MAS_REPORLEVEL.Encrypted_Id);
                    param.Add("@p_ReportingRole", MAS_REPORLEVEL.ReportingRole);
                    param.Add("@p_ReportingLever", MAS_REPORLEVEL.ReportingLever);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_UserId", 0);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_MAS_REPORLEVEL", param);
                    var msg = param.Get<string>("@p_msg");
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    return RedirectToAction("Setting_SalesForce_ReportLevel", "Setting_SalesForce_ReportLevel");
                }
                else
                {
                    param.Add("@p_process", "Update");
                    param.Add("@P_Fid", MAS_REPORLEVEL.Fid);
                    // param.Add("@P_Encrypted_Id", MAS_REPORLEVEL.Encrypted_Id);
                    param.Add("@p_ReportingRole", MAS_REPORLEVEL.ReportingRole);
                    param.Add("@p_ReportingLever", MAS_REPORLEVEL.ReportingLever);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_UserId", 0);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_MAS_REPORLEVEL", param);
                    var msg = param.Get<string>("@p_msg");
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    return RedirectToAction("Setting_SalesForce_ReportLevel", "Setting_SalesForce_ReportLevel");
                }
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 813;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@query", "Select * from MAS_REPORLEVEL Where Deleted = 0");
                var ReportLevelList = DapperORM.ReturnList<MAS_REPORLEVEL>("Sp_QueryExcution", param).ToList();
                ViewBag.ReportLevelList = ReportLevelList;
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
        public ActionResult Delete(string Fid)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 813;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_Fid", Fid);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_MAS_REPORLEVEL", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_SalesForce_ReportLevel");
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
       
          