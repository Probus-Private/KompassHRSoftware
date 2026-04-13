using Dapper;
using KompassHR.Areas.Setting.Models.Setting_TimeOffice;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_TimeOffice
{
    public class Setting_TimeOffice_ShiftRuleController : Controller
    {
        // GET: Setting/Setting_TimeOffice_ShiftRule
        DynamicParameters param = new DynamicParameters();
        #region ShiftRule Main View
        [HttpGet]
        public ActionResult Setting_TimeOffice_ShiftRule(string ShiftRuleId_Encrypted, int? CompanyId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 54;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Atten_ShiftRule Atten_ShiftRule = new Atten_ShiftRule();   

                param.Add("@query", "Select  CompanyId As Id, CompanyName As Name from Mas_CompanyProfile where Deactivate = 0 order by Name");
                var GetCompanyName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.CompanyName = GetCompanyName;
                param = new DynamicParameters();

                if (CompanyId != null)
                {
                    DynamicParameters ParamBranch = new DynamicParameters();
                    ParamBranch.Add("@p_employeeid", Session["EmployeeId"]);
                    ParamBranch.Add("@p_CmpId", CompanyId);
                    ViewBag.Location = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", ParamBranch).ToList();
                }
                else
                {
                    ViewBag.Location = "";
                }
                if (ShiftRuleId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_ShiftRuleId_Encrypted", ShiftRuleId_Encrypted);
                    Atten_ShiftRule = DapperORM.ReturnList<Atten_ShiftRule>("sp_List_Atten_ShiftRule", param1).FirstOrDefault();
                }
                return View(Atten_ShiftRule);
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
                    return RedirectToAction("Login", "Login", new { Area = "" });
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

        #region IsValidation
        [HttpGet]
        public ActionResult IsShiftRuleExists(string ShiftRuleName, string ShiftRuleSName, string ShiftRuleId_Encrypted, double ShiftRuleBranchId, double CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "IsValidation");
                param.Add("@p_ShiftRuleId_Encrypted", ShiftRuleId_Encrypted);
                param.Add("@p_CmpId", CmpId);
                param.Add("@p_ShiftRuleBranchId", ShiftRuleBranchId);
                param.Add("@p_ShiftRuleName", ShiftRuleName);
                param.Add("@p_ShiftRuleSName", ShiftRuleSName);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Atten_ShiftRule", param);
                var Message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = Message;
                TempData["Icon"] = Icon;
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
        public ActionResult SaveUpdate(Atten_ShiftRule ShiftRule)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var isa = ShiftRule.IsDefault;

                param.Add("@p_process", string.IsNullOrEmpty(ShiftRule.ShiftRuleId_Encrypted) ? "Save" : "Update");
                param.Add("@p_ShiftRuleId_Encrypted", ShiftRule.ShiftRuleId_Encrypted);
                param.Add("@p_CmpId", ShiftRule.CmpId);
                param.Add("@p_ShiftRuleBranchId", ShiftRule.ShiftRuleBranchId);
                param.Add("@p_ShiftRuleName", ShiftRule.ShiftRuleName);
                param.Add("@p_ShiftRuleSName", ShiftRule.ShiftRuleSName);
                param.Add("@p_OTFormula", "Total Duration-Shift Hours");
                param.Add("@p_MinOT", ShiftRule.MinOT);
                param.Add("@p_IsMaxOT", ShiftRule.IsMaxOT);
                param.Add("@p_MaxOT", ShiftRule.MaxOT);
                param.Add("@p_FullDayDuration", 0);
                param.Add("@p_HalfDayDuration", 0);
                param.Add("@p_IsMarkHalfDayForLate", ShiftRule.IsMarkHalfDayForLate);
                param.Add("@p_HalfDayLateByMins", ShiftRule.HalfDayLateByMins);
                param.Add("@p_IsMarkHalfdayForEarlyGoing", ShiftRule.IsMarkHalfdayForEarlyGoing);
                param.Add("@p_HalfDayEarlyGoingMins", ShiftRule.HalfDayEarlyGoingMins);
             
                param.Add("@p_IsDefault", ShiftRule.IsDefault);
                param.Add("@p_OTCalculation", ShiftRule.OTCalculation);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Atten_ShiftRule", param);
                var Message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = Message;
                TempData["Icon"] = Icon;
                return RedirectToAction("Setting_TimeOffice_ShiftRule", "Setting_TimeOffice_ShiftRule");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 54;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_ShiftRuleId_Encrypted", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.DynamicList("sp_List_Atten_ShiftRule", param);
                ViewBag.GetShiftRuleList = data;
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
        public ActionResult Delete(int ShiftRuleId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_ShiftRuleId", ShiftRuleId);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_ShiftRule", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_TimeOffice_ShiftRule", new { Area = "Setting" });
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