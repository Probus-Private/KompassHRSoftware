using Dapper;
using KompassHR.Areas.Setting.Models.Setting_IncomeTax;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_IncomeTax
{
    public class Setting_IncomeTax_TaxHelpController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: Setting/Setting_IncomeTax_TaxHelp

        #region Setting_IncomeTax_TaxHelp
        [ValidateInput(false)]
        public ActionResult Setting_IncomeTax_TaxHelp(string HelpId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 518;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                IncomeTax_Help Incometax_help = new IncomeTax_Help();

                param.Add("@query", "Select TaxFyearId,TaxYear from IncomeTax_Fyear Where Deactivate=0");
                var listMas_FinancialYear = DapperORM.ReturnList<IncomeTax_Fyear>("sp_QueryExcution", param).ToList();
                ViewBag.GetFinancialYear = listMas_FinancialYear;

                param.Add("@query", "Select TypeId,Type from IncomeTax_HelpType Where Deactivate=0");
                var TypeList = DapperORM.ReturnList<IncomeTax_HelpType>("sp_QueryExcution", param).ToList();
                ViewBag.TypeList = TypeList;

                if (HelpId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_HelpId_Encrypted", HelpId_Encrypted);
                    Incometax_help = DapperORM.ReturnList<IncomeTax_Help>("sp_List_IncomeTax_Help", param).FirstOrDefault();                   
                }
                else
                {
                    TempData["ToDate"] = null;
                    TempData["FromDate"] = null;
                }
                return View(Incometax_help);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion Setting_IncomeTax_TaxHelp


        #region IsTaxRuleExists
        [HttpGet]
        public ActionResult IsTaxHelpExists(string ddlIncomeTaxHelpFyearId, string HelpId_Encrypted)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_IncomeTaxHelp_FyearId", ddlIncomeTaxHelpFyearId);
                    param.Add("@p_HelpId_Encrypted", HelpId_Encrypted);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_Help", param);
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
        [ValidateInput(false)]
        public ActionResult SaveUpdate(IncomeTax_Help Help )
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });                    
                }
                param.Add("@p_process", string.IsNullOrEmpty(Help.HelpId_Encrypted) ? "Save" : "Update");
                param.Add("@p_HelpId_Encrypted", Help.HelpId_Encrypted);
                param.Add("@p_IncomeTaxHelp_FyearId", Help.IncomeTaxHelp_FyearId);
                param.Add("@p_TypeId", Help.TypeId);
                param.Add("@p_Help_Description", Help.Help_Description);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_Help", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                return RedirectToAction("Setting_IncomeTax_TaxHelp", "Setting_IncomeTax_TaxHelp");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion SaveUpdate

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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 518;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_HelpId_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_IncomeTax_Help", param);
                ViewBag.GetTaxHelpList = data;
                return View();
               
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion GetList

        #region Delete
        public ActionResult Delete(string HelpId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_HelpId_Encrypted", HelpId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_Help", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_IncomeTax_TaxHelp");
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