using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_IncomeTax;
using System.Data.SqlClient;

namespace KompassHR.Areas.Setting.Controllers.Setting_IncomeTax
{
    public class Setting_IncomeTax_TaxRuleController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: IncomeTaxSetting/IncomeTax_TaxRuleSetting
        #region Setting_IncomeTax_TaxRule
        [HttpGet]
        public ActionResult Setting_IncomeTax_TaxRule(string IncomeTaxRule_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 77;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                IncomeTax_Rule Incometax_rule = new IncomeTax_Rule();

                param.Add("@query", "Select TaxFyearId,TaxYear from IncomeTax_Fyear Where Deactivate=0");
                var listMas_FinancialYear = DapperORM.ReturnList<IncomeTax_Fyear>("sp_QueryExcution", param).ToList();
                ViewBag.GetFinancialYear = listMas_FinancialYear;

                if (IncomeTaxRule_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_IncomeTaxRule_Encrypted", IncomeTaxRule_Encrypted);
                    Incometax_rule = DapperORM.ReturnList<IncomeTax_Rule>("sp_List_IncomeTax_Rule", param).FirstOrDefault();
                    TempData["HRA_Calculation_Monthly_Yearly"] = Incometax_rule.HRA_Calculation_Monthly_Yearly;
                    TempData["ToDate"] = Incometax_rule.ToDate.ToString("yyyy-MM-dd");
                    TempData["FromDate"] = Incometax_rule.FromDate.ToString("yyyy-MM-dd");
                }
                else
                {
                    TempData["ToDate"] = null;
                    TempData["FromDate"] = null;
                }
                return View(Incometax_rule);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region IsTaxRuleExists
        [HttpGet]
        public ActionResult IsTaxRuleExists(string IncomeTaxRuleFyearId, string IncomeTaxRule_Encrypted)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {

                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_IncomeTaxRule_FyearId", IncomeTaxRuleFyearId);
                    param.Add("@p_IncomeTaxRule_Encrypted", IncomeTaxRule_Encrypted);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_Rule", param);
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
        public ActionResult SaveUpdate(IncomeTax_Rule IncomeTaxRule)
        {

            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(IncomeTaxRule.IncomeTaxRule_Encrypted) ? "Save" : "Update");
                param.Add("@p_IncomeTaxRuleId", IncomeTaxRule.IncomeTaxRuleId);
                param.Add("@p_IncomeTaxRule_Encrypted", IncomeTaxRule.IncomeTaxRule_Encrypted);
                param.Add("@p_IncomeTaxRule_FyearId", IncomeTaxRule.IncomeTaxRule_FyearId);
                // param.Add("@p_FromDate", IncomeTaxRule.FromDate);
                // param.Add("@p_ToDate", IncomeTaxRule.ToDate);
                //param.Add("@p_ToDate", IncomeTaxRule.ToDate);
                param.Add("@p_HRA_Metro_Percentage", IncomeTaxRule.HRA_Metro_Percentage);
                param.Add("@p_HRA_Metro_Minus", IncomeTaxRule.HRA_Metro_Minus);
                param.Add("@p_HRA_NoNMetro_Percentage", IncomeTaxRule.HRA_NoNMetro_Percentage);
                param.Add("@p_HRA_NoNMetro_Minus", IncomeTaxRule.HRA_NoNMetro_Minus);
                param.Add("@p_HRA_Calculation_Monthly_Yearly", IncomeTaxRule.HRA_Calculation_Monthly_Yearly);
                param.Add("@p_HRA_PanLimit", IncomeTaxRule.HRA_PanLimit);
                param.Add("@p_ChildEducation_Allowance", IncomeTaxRule.ChildEducation_Allowance);
                param.Add("@p_ChildEducation_NoOfChildren", IncomeTaxRule.ChildEducation_NoOfChildren);
                param.Add("@p_ChildHostel_Allowance", IncomeTaxRule.ChildHostel_Allowance);
                param.Add("@p_ChildHostel_NoOfChildren", IncomeTaxRule.ChildHostel_NoOfChildren);
                param.Add("@p_Conveyance_Handicap_Amount", IncomeTaxRule.Conveyance_Handicap_Amount);
                param.Add("@p_LeaveSalary_Amount", IncomeTaxRule.LeaveSalary_Amount);
                param.Add("@p_Gratuity_Coverd_Amount", IncomeTaxRule.Gratuity_Coverd_Amount);
                param.Add("@p_Gratuity_NotCoverd_Amount", IncomeTaxRule.Gratuity_NotCoverd_Amount);
                param.Add("@p_StandardEducation_OldRegime_Amount", IncomeTaxRule.StandardEducation_OldRegime_Amount);
                param.Add("@p_StandardEducation_NewRegime_Amount", IncomeTaxRule.StandardEducation_NewRegime_Amount);
                param.Add("@p_ProfessionalTax_Amount", IncomeTaxRule.ProfessionalTax_Amount);
                param.Add("@p_TaxRule80C_Amount", IncomeTaxRule.TaxRule80C_Amount);
                param.Add("@p_TaxRule80TTA_Amount", IncomeTaxRule.TaxRule80TTA_Amount);
                param.Add("@p_TaxRule80TTB_Amount", IncomeTaxRule.TaxRule80TTB_Amount);
                param.Add("@p_TaxRule80D_ParentsBelow60_Self_Family_Children", IncomeTaxRule.TaxRule80D_ParentsBelow60_Self_Family_Children);
                param.Add("@p_TaxRule80D_ParentsBelow60_Parents", IncomeTaxRule.TaxRule80D_ParentsBelow60_Parents);
                param.Add("@p_TaxRule80D_ParentsBelow60_Deduction80D", IncomeTaxRule.TaxRule80D_ParentsBelow60_Deduction80D);
                param.Add("@p_TaxRule80D_Below60_ParentsAbove60_Self_Family_Children", IncomeTaxRule.TaxRule80D_Below60_ParentsAbove60_Self_Family_Children);
                param.Add("@p_TaxRule80D_Below60_ParentsAbove60_Parents", IncomeTaxRule.TaxRule80D_Below60_ParentsAbove60_Parents);
                param.Add("@p_TaxRule80D_Below60_ParentsAbove60_Deduction80D", IncomeTaxRule.TaxRule80D_Below60_ParentsAbove60_Deduction80D);
                param.Add("@p_TaxRule80D_BothAbove60_Self_Family_Children", IncomeTaxRule.TaxRule80D_BothAbove60_Self_Family_Children);
                param.Add("@p_TaxRule80D_BothAbove60_Parents", IncomeTaxRule.TaxRule80D_BothAbove60_Parents);
                param.Add("@p_TaxRule80D_BothAbove60_DeductionUnder80D", IncomeTaxRule.TaxRule80D_BothAbove60_DeductionUnder80D);
                param.Add("@p_TaxRule80D_BothAbove60_DeductionUnder80D_HealthCheckup", IncomeTaxRule.TaxRule80D_BothAbove60_DeductionUnder80D_HealthCheckup);
                param.Add("@p_Housingloan_After_1999", IncomeTaxRule.Housingloan_After_1999);
                param.Add("@p_Housingloan_Before_1999", IncomeTaxRule.Housingloan_Before_1999);
                param.Add("@p_Housingloan_RepairOrRenewal", IncomeTaxRule.Housingloan_RepairOrRenewal);
                param.Add("@p_TaxRule80CCD_1B_NPS_Amount", IncomeTaxRule.TaxRule80CCD_1B_NPS_Amount);
                param.Add("@p_TaxRule80DD_Handicap_Severeamount", IncomeTaxRule.TaxRule80DD_Handicap_SevereAmount);
                param.Add("@p_TaxRule80DD_Handicap_Nonsevereamount", IncomeTaxRule.TaxRule80DD_Handicap_NonSevereAmount);
                param.Add("@p_TaxRule80DDB_NormalCitizen_Amount", IncomeTaxRule.TaxRule80DDB_NormalCitizen_Amount);
                param.Add("@p_TaxRule80DDB_SeniorCitizen_Amount", IncomeTaxRule.TaxRule80DDB_SeniorCitizen_Amount);
                param.Add("@p_TaxRule80U_Physical_Handicap_Severe_Amount", IncomeTaxRule.TaxRule80U_Physical_Handicap_Severe_Amount);
                param.Add("@p_TaxRule80U_Physical_Handicap_Nonsevere_Amount", IncomeTaxRule.TaxRule80U_Physical_Handicap_Nonsevere_Amount);
                param.Add("@p_TaxRule80EEB_Interest_OnElecticVechile", IncomeTaxRule.TaxRule80EEB_Interest_OnElecticVechile);
                param.Add("@p_IncomefromLetOutHouseProperty_StandardEducation", IncomeTaxRule.IncomefromLetOutHouseProperty_StandardEducation);
                param.Add("@p_TaxRule80EE_Limit", IncomeTaxRule.TaxRule80EE_Limit);
                param.Add("@p_TaxRule80EE_Loan_Maxamount", IncomeTaxRule.TaxRule80EE_Loan_Maxamount);
                param.Add("@p_TaxRule80EE_PropertyValues", IncomeTaxRule.TaxRule80EE_PropertyValues);
                param.Add("@p_TaxRule80EEA_Limit", IncomeTaxRule.TaxRule80EEA_Limit);
                param.Add("@p_TaxRule80EEA_PropertyValues", IncomeTaxRule.TaxRule80EEA_PropertyValues);
                param.Add("@p_HPLoss_Maxamount", IncomeTaxRule.HPLoss_Maxamount);
                param.Add("@p_TaxRule_Rabate_OldRegime", IncomeTaxRule.TaxRule_Rabate_OldRegime);
                param.Add("@p_TaxRule_Rabate_NewRegime", IncomeTaxRule.TaxRule_Rabate_NewRegime);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_Rule", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;

                return RedirectToAction("Setting_IncomeTax_TaxRule", "Setting_IncomeTax_TaxRule");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 77;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_IncomeTaxRule_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_IncomeTax_Rule", param);
                ViewBag.GetTaxRuleList = data;
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