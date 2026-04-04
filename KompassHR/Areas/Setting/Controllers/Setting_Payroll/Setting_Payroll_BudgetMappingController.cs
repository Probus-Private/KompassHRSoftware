using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Payroll;
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

namespace KompassHR.Areas.Setting.Controllers.Setting_Payroll
{
    public class Setting_Payroll_BudgetMappingController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: Setting/Setting_Payroll_BudgetMapping

        #region Setting_Payroll_BudgetMapping
        public ActionResult Setting_Payroll_BudgetMapping(string BudgetMappingId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 539;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                //GET COMPANY NAME
                Payroll_BudgetMapping OBJPayroll_BudgetMapping = new Payroll_BudgetMapping();
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                ViewBag.BranchName = "";

                param.Add("@query", "Select Budgetid as id , Budgetname as name from Payroll_Budget where Deactivate=0");
                var BudgetName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.BudgetName = BudgetName;

                if (BudgetMappingId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_BudgetMappingId_Encrypted", BudgetMappingId_Encrypted);
                    OBJPayroll_BudgetMapping = DapperORM.ReturnList<Payroll_BudgetMapping>("sp_List_Setting_MappingBudget", param).FirstOrDefault();

                    var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(OBJPayroll_BudgetMapping.BudgetMappingCmpId), Convert.ToInt32(Session["EmployeeId"]));
                    ViewBag.BranchName = Branch;
                }
                return View(OBJPayroll_BudgetMapping);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region GetList Main View
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 539;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_BudgetMappingId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Setting_MappingBudget", param).ToList();
                ViewBag.GetNoDuesCheckListList = data;
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
        public ActionResult Delete(string BudgetMappingId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@P_BudgetMappingId_Encrypted", BudgetMappingId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_BudgetMapping", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Payroll_BudgetMapping");
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

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Payroll_BudgetMapping BudgetMapping)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(BudgetMapping.BudgetMappingId_Encrypted) ? "Save" : "Update");
                param.Add("@P_BudgetMappingId", BudgetMapping.BudgetMappingId);
                param.Add("@P_BudgetMappingId_Encrypted", BudgetMapping.BudgetMappingId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["BudgetName"]);
                param.Add("@P_BudgetMappingCmpId", BudgetMapping.BudgetMappingCmpId);
                param.Add("@P_BudgetMappingBuId", BudgetMapping.BudgetMappingBuId);
                param.Add("@P_BudgetMappingBugetId", BudgetMapping.BudgetMappingBudgetId);
                param.Add("@P_Amount", BudgetMapping.Amount);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_BudgetMapping", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Payroll_BudgetMapping", "Setting_Payroll_BudgetMapping");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region IsValidaton
        [HttpGet]
        public ActionResult IsNoDuesCheckListExists(int? BudgetMappingBuId, int? BudgetMappingBudgetId, string BudgetMappingId_Encrypted )
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "IsValidation");
                param.Add("@P_BudgetMappingId_Encrypted", BudgetMappingId_Encrypted);
                //.Add("@P_BudgetMappingCmpId", BudgetMappingCmpId);
                param.Add("@P_BudgetMappingBuId", BudgetMappingBuId);
                param.Add("@P_BudgetMappingBugetId", BudgetMappingBudgetId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["BudgetName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_BudgetMapping", param);
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

        #region GetBudget
        [HttpGet]
        public ActionResult GetBudget(int BudgetMappingBudgetId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var BudgetName = new BulkAccessClass().GetEmployeeName(Convert.ToInt32(BudgetMappingBudgetId));
                return Json(new { BudgetName = BudgetName }, JsonRequestBehavior.AllowGet);
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