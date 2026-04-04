using Dapper;
using KompassHR.Areas.ESS.Models.ESS_ManpowerAllocation;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_ManpowerAllocation
{
    public class ESS_ManpowerAllocation_IDELockController : Controller
    {
        // GET: ESS/ESS_ManpowerAllocation_IDELock
        #region MainView
        public ActionResult ESS_ManpowerAllocation_IDELock(string IDE_Encrypted_Id)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                // Check access permissions
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 791;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";

                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;

                DynamicParameters paramBUName = new DynamicParameters();
                paramBUName.Add("@query", "select Distinct BranchId as Id,BranchName as Name from Mas_Branch;");
                ViewBag.BUName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBUName).ToList();

                Manpower_IDELock lok = new Manpower_IDELock();
                if (IDE_Encrypted_Id!=null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_IDE_Encrypted_Id", IDE_Encrypted_Id);
                    lok = DapperORM.ReturnList<Manpower_IDELock>("Sp_List_Manpower_IDELock", param).FirstOrDefault();
                }
               
                return View(lok);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetBuisnessUnit
        [HttpGet]
        public ActionResult GetBuisnessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select Distinct BranchId as Id,BranchName as Name from Mas_Branch where CmpId='" + CmpId + "';");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsLockExists
        public ActionResult IsLockExists(int? CmpId, int? BranchId, string IDE_Encrypted_Id, int? IDEId,DateTime? MonthYear)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_CmpId", CmpId);
                    param.Add("@p_BranchId", BranchId);
                    param.Add("@p_IDEId", IDEId);
                    param.Add("@p_IDE_Encrypted_Id", IDE_Encrypted_Id);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                    var Result = DapperORM.ExecuteReturn("sp_SUD_Manpower_IDELock", param);
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
        public ActionResult SaveUpdate(Manpower_IDELock lok)
        {
            try

            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", string.IsNullOrEmpty(lok.IDE_Encrypted_Id) ? "Save" : "Update");
                param.Add("@p_IDE_Encrypted_Id", lok.IDE_Encrypted_Id);
                param.Add("@p_MonthYear", lok.MonthYear);
                param.Add("@p_CmpId", lok.CmpId);
                param.Add("@p_BranchId", lok.BranchId);

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                var Result = DapperORM.ExecuteReturn("sp_SUD_Manpower_IDELock", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");

                var P_Id = param.Get<string>("@p_Id");

                return RedirectToAction("ESS_ManpowerAllocation_IDELock", "ESS_ManpowerAllocation_IDELock");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 791;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_IDE_Encrypted_Id", "List");
                var data = DapperORM.DynamicList("Sp_List_Manpower_IDELock", param);
                ViewBag.GetIDELock = data;


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
        public ActionResult Delete(string IDE_Encrypted_Id)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 780;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                DynamicParameters param = new DynamicParameters();

                param.Add("@p_process", "Delete");
                param.Add("@p_IDE_Encrypted_Id", IDE_Encrypted_Id);

                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Manpower_IDELock", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_ManpowerAllocation_IDELock");
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