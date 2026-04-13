using Dapper;
using KompassHR.Areas.ESS.Models.ESS_ManpowerAllocation;
using KompassHR.Areas.Module.Models.Module_Employee;
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
    public class ESS_ManpowerAllocation_ManpowerMainCategoryController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_ManpowerAllocation_ManpowerMainCategory
        #region ManpowerMainCategory
        public ActionResult ESS_ManpowerAllocation_ManpowerMainCategory(string KPICategoryId_Encrypted,int? CmpID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 341;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                KPI_Category KPICategory = new KPI_Category();
                //DynamicParameters paramList = new DynamicParameters();
                //var results = DapperORM.DynamicQuerySingleMultiple(@"SELECT DesignationId  as Id, DesignationName as Name FROM Mas_Designation WHERE Deactivate =0 order by DesignationId;
                //                                     select CompanyId as Id ,CompanyName as Name from Mas_CompanyProfile where Deactivate = 0;");
                //ViewBag.GetDesignationName = results.Read<AllDropDownClass>().ToList();
                //ViewBag.GetCompanyName = results.Read<AllDropDownClass>().ToList();
                //ViewBag.GetBranchName = "";

                if (KPICategoryId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    DynamicParameters ShiftGroup = new DynamicParameters();
                    ShiftGroup.Add("@p_KPICategoryId_Encrypted", KPICategoryId_Encrypted);
                    KPICategory = DapperORM.ReturnList<KPI_Category>("sp_List_KPI_Category", ShiftGroup).FirstOrDefault();

                    //DynamicParameters paramBranch = new DynamicParameters();
                    //paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                    //paramBranch.Add("@p_CmpId", CmpID);
                    //var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                    //ViewBag.GetBranchName = data;

                  
                }
                return View(KPICategory);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsValidation
        [HttpPost]
        public ActionResult IsManpowerMainAllocationExists(string KPICategoryId_Encrypted, string KPICategoryName)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                
                param.Add("@p_process", "IsValidation");
                param.Add("@p_KPICategoryId_Encrypted", KPICategoryId_Encrypted);
               // param.Add("@p_KPISubCategoryBranchId", KPISubCategoryBranchId);
               // param.Add("@p_CmpID", CompanyName);
                param.Add("@p_KPICategoryName", KPICategoryName);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_KPI_Category", param);
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
        public ActionResult SaveUpdate(KPI_Category ObjKPICategory)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(ObjKPICategory.KPICategoryId_Encrypted) ? "Save" : "Update");
                param.Add("@p_KPICategoryId_Encrypted", ObjKPICategory.KPICategoryId_Encrypted);
                //param.Add("@p_CmpID", ObjKPICategory.CmpID);
               // param.Add("@p_KPISubCategoryBranchId", ObjKPICategory.KPISubCategoryBranchId);
                param.Add("@p_KPICategoryName", ObjKPICategory.KPICategoryName);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_KPI_Category", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return RedirectToAction("ESS_ManpowerAllocation_ManpowerMainCategory", "ESS_ManpowerAllocation_ManpowerMainCategory");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 341;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_KPICategoryId_Encrypted", "List");
                param.Add("@P_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.DynamicList("sp_List_KPI_Category", param);
                ViewBag.GetKPICategoryList = data;
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
        public ActionResult Delete(string KPICategoryId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_KPICategoryId_Encrypted", KPICategoryId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_KPI_Category", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_ManpowerAllocation_ManpowerMainCategory");
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