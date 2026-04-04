
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
    public class Setting_SaleForce_ReportAccessController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region Main View Setting_SaleForce_ReportAccess

        // GET: Setting/Setting_SaleForce_ReportAccess
        public ActionResult Setting_SaleForce_ReportAccess( string access)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 822;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";
                //var MainRole = Session["MainRole"].ToString();
                var userId = Convert.ToInt32(Session["EmployeeId"]);
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {

                    DynamicParameters param2 = new DynamicParameters();
                    //param2.Add("@Role", MainRole);
                    param2.Add("@EmployeeId", userId);
                    var ReportAccessList = DapperORM.ReturnList<dynamic>("SP_Report_Mapping_List", param2).ToList();
                   // ViewBag.ReportAccessList = ReportAccessList;
                   // ViewBag.Access = access;



                    var userList = ReportAccessList.Select(x => new
                    {
                        Fid = (int)x.Fid,
                        UserName = (string)x.UserName
                    }).ToList();

                    ViewBag.ReportAccessList = userList;

                    return View();
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
        
        public JsonResult GetRptName(string Module)
        {
            try
            {
               // DapperORM.SetConnection();
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@query", $"SELECT Distinct T2.Deleted,T1.* FROM MAS_REPORT T1 Left JOIN MAS_REPORTMAPPING T2 ON T1.FID = T2.MAS_REPORT_fID AND T2.deleted = 0 left JOIN Mas_employee T3 ON T2.USERS_Fid = T3.EmployeeId WHERE T1.Module = '{Module}' and T1.Deleted = 0");
                    var ReportNameList = DapperORM.ReturnList<MAS_REPORT>("Sp_QueryExcution", param).ToList();
                    //ViewBag.ReportAccessList = ReportNameList;
                    return Json(ReportNameList, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }
        
        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(MAS_REPORTMAPPING MAS_REPORTMAPPING, string access)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                if (MAS_REPORTMAPPING.Encrypted_Id == null)
                {
                    param.Add("@p_process", "Save");
                    param.Add("@p_Fid", MAS_REPORTMAPPING.Fid);
                    param.Add("@p_Encrypted_Id", MAS_REPORTMAPPING.Encrypted_Id);
                    param.Add("@p_MAS_REPORT_Fid", MAS_REPORTMAPPING.MAS_REPORT_Fid);
                    param.Add("@p_USERS_Fid", MAS_REPORTMAPPING.USERS_Fid);
                    param.Add("@p_UserId", Convert.ToInt32(Session["EmployeeId"]));
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                   // param.Add("@p_Mip", Dns.GetHostByName(MachineId).AddressList[0].ToString());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var data = DapperORM.ExecuteReturn("Sp_SUD_MAS_REPORTMAPPING", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    return RedirectToAction("Setting_SaleForce_ReportAccess", "Setting_SaleForce_ReportAccess");
                }
                else
                {
                    return RedirectToAction("Setting_SaleForce_ReportAccess", "Setting_SaleForce_ReportAccess");
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
        public ActionResult GetList(string access)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 814;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var userId = Convert.ToInt32(Session["EmployeeId"]);
                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@EmployeeId", userId);
                var ReportAccessList = DapperORM.ReturnList<dynamic>("SP_Report_Mapping_List", param2).ToList();
                ViewBag.ReportAccessList = ReportAccessList;
                ViewBag.Access = access;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        public ActionResult Delete(int? Fid)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 822;
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
                var Result = DapperORM.ExecuteReturn("Sp_SUD_MAS_REPORTMAPPING", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_SaleForce_ReportAccess");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
    }
}