using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Onboarding;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Onboarding
{
    public class Setting_Onboarding_CriticalStageCategoryController : Controller
    {
        // GET: Setting/Setting_Onboarding_CriticalStageCategory
        DynamicParameters param = new DynamicParameters();
        #region  Main View
        [HttpGet]
        public ActionResult Setting_Onboarding_CriticalStageCategory(string CriticalStageId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 34;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Mas_CriticalStageCategory mas_criticalstagecategory = new Mas_CriticalStageCategory();
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.ComapnyName = GetComapnyName;
                var CMPID = GetComapnyName[0].Id;

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_employeeid", Session["EmployeeId"]);
                param1.Add("@p_CmpId", CMPID);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param1).ToList();
                ViewBag.BranchName = data;
                var BUID = data[0].Id;

                DynamicParameters param4 = new DynamicParameters();
                param4.Add("@query", "Select LineId as Id, LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate = 0 and Mas_LineMaster.CmpId = '" + CMPID + "' and Mas_LineMaster.BranchId = '" + BUID + "'");
                var MasLineMasterit = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param4).ToList();
                ViewBag.GetLineName = MasLineMasterit;

                //param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeeid<>1 and EmployeeLeft=0 and Mas_Employee.ContractorID<>1 and employeebranchid in (select distinct branchid from UserBranchMapping where AccessWorkForce = 1 and isactive = 1 and employeeid = '" + Session["EmployeeId"] + "') order by EmployeeName");
                //var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                //ViewBag.EmployeeName = GetEmployeeName;

                ViewBag.AddUpdateTitle = "Add";
                if (CriticalStageId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_CriticalStageId_Encrypted", CriticalStageId_Encrypted);
                    mas_criticalstagecategory = DapperORM.ReturnList<Mas_CriticalStageCategory>("sp_List_Mas_CriticalStageCategory", param).FirstOrDefault();
                }
                return View(mas_criticalstagecategory);
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
        public ActionResult IsCriticalStageCategoryExists(string CriticalStageName, string CriticalStageId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_CriticalStageName", CriticalStageName);
                    param.Add("@p_CriticalStageId_Encrypted", CriticalStageId_Encrypted);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_CriticalStageCategory", param);
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
        public ActionResult SaveUpdate(Mas_CriticalStageCategory criticalstagecategory)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(criticalstagecategory.CriticalStageId_Encrypted) ? "Save" : "Update");
                param.Add("@p_CriticalStageId_Encrypted", criticalstagecategory.CriticalStageId_Encrypted);
                param.Add("@p_CriticalStageName", criticalstagecategory.CriticalStageName);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_CriticalStageCategory", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Onboarding_CriticalStageCategory", "Setting_Onboarding_CriticalStageCategory");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 34;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_CriticalStageId_Encrypted", "List");
                ViewBag.EmployeeCriticalStage = DapperORM.DynamicList("sp_List_Mas_CriticalStageCategory", param);
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
        public ActionResult Delete(string CriticalStageId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_CriticalStageId_Encrypted", CriticalStageId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_CriticalStageCategory", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Onboarding_CriticalStageCategory");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetLineMaster(int? BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@query", "Select Lineid as Id,LineName As Name from Mas_LineMaster where BranchId="+ BranchId + " order by Name");
                var GetLineMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                return Json(GetLineMaster, JsonRequestBehavior.AllowGet);
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