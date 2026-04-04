
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
    public class Setting_SaleForce_SiteTypeController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);


        #region Setting_SaleForce_SiteType  Main view
        // GET: Setting/Setting_SaleForce_SiteType
        public ActionResult Setting_SaleForce_SiteType(string Encrypted_Id)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 811;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                MAS_SITETYPE MAS_SITETYPE = new MAS_SITETYPE();
                //if (Encrypted_Id != null)
                //{
                //    ViewBag.AddUpdateTitle = "Update";
                //    param.Add("@p_Encrypted_Id", Encrypted_Id);
                //    MAS_SITETYPE = DapperORM.ReturnList<MAS_SERCATG>("sp_List_MAS_SERCATG", param).FirstOrDefault();
                //}

                if (Encrypted_Id != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    var param = new DynamicParameters();
                    param.Add("@query", $"SELECT * FROM MAS_SITETYPE WHERE Encrypted_Id = '{Encrypted_Id}' AND Deleted = 0");

                    MAS_SITETYPE = DapperORM.ReturnList<MAS_SITETYPE>("Sp_QueryExcution", param)
                                              .FirstOrDefault();
                }
                return View(MAS_SITETYPE);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
      #endregion

        #region IsServiceCategoryExists
        public ActionResult IsSiteTypeExists(string SiteType, string Encrypted_Id)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 811;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {

                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_Encrypted_Id", Encrypted_Id);
                    param.Add("@p_SiteType", SiteType);
                    param.Add("@p_msg", dbType: System.Data.DbType.String, direction: System.Data.ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: System.Data.DbType.String, direction: System.Data.ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_MAS_SITETYPE", param);
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
        public ActionResult SaveUpdate(MAS_SITETYPE MAS_SITETYPE)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 811;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                if ( MAS_SITETYPE.Encrypted_Id == null)
                {
                    param.Add("@p_process", "Save");
                    param.Add("@P_Fid", MAS_SITETYPE.Fid);
                    param.Add("@P_Encrypted_Id", MAS_SITETYPE.Encrypted_Id);
                    param.Add("@p_SiteType", MAS_SITETYPE.SiteType);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_UserId", 0);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_MAS_SITETYPE", param);
                    var msg = param.Get<string>("@p_msg");
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    return RedirectToAction("Setting_SaleForce_SiteType", "Setting_SaleForce_SiteType");
                }
                else
                {
                    param.Add("@p_process", "Update");
                    param.Add("@P_Fid", MAS_SITETYPE.Fid);
                    param.Add("@P_Encrypted_Id", MAS_SITETYPE.Encrypted_Id);
                    param.Add("@p_SiteType", MAS_SITETYPE.SiteType);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_UserId", 0);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_MAS_SITETYPE", param);
                    var msg = param.Get<string>("@p_msg");
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    return RedirectToAction("Setting_SaleForce_SiteType", "Setting_SaleForce_SiteType");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 810;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@query", "Select * from MAS_SITETYPE Where Deleted = 0");
                var SiteTypeList = DapperORM.ReturnList<MAS_SITETYPE>("Sp_QueryExcution", param).ToList();
                ViewBag.SiteTypeList = SiteTypeList;
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
        public ActionResult Delete(string Encrypted_Id)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 811;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_Encrypted_Id", Encrypted_Id);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_MAS_SITETYPE", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_SaleForce_SiteType");
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
