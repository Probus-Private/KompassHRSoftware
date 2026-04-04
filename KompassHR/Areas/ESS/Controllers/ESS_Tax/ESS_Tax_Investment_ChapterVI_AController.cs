using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Tax;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Tax
{
    public class ESS_Tax_Investment_ChapterVI_AController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_Tax_Investment_ChapterVI_A
        #region ChapterVI_A Main View
        public ActionResult ESS_Tax_Investment_ChapterVI_A()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 180;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
                // var GetEmpoyee = "Select * from View_IncomeTax_Employee Where EmployeeID = " + Session["EmployeeId"] + "";

                var IncomeTaxEmployee = DapperORM.DynamicQueryList("Select  FLOOR((CAST (Convert(Datetime, (Select ToDate from IncomeTax_Fyear where TaxFyearId= " + Session["TaxFyearId"] + " and Deactivate=0)) AS INTEGER) - CAST(Convert(Datetime, BirthdayDate) AS INTEGER)) / 365.25) as Age from Mas_Employee_Personal   where PersonalEmployeeId=" + Session["EmployeeId"] + " and Deactivate=0").FirstOrDefault();
                //var IncomeTaxEmployee = DapperORM.DynamicQuerySingle(GetEmpoyee);
                if (IncomeTaxEmployee != null)
                {
                    TempData["Age"] = IncomeTaxEmployee.Age;
                }
                else
                {
                    TempData["Age"] = "";
                }


                var Get80DRules = DapperORM.DynamicQueryList(@"Select [TaxRule80D_ParentsBelow60_Self_Family_Children] As ParentsBelow60_Self_Family_Children
                                            ,[TaxRule80D_ParentsBelow60_Parents] As ParentsBelow60_Parents
                                            ,[TaxRule80D_Below60_ParentsAbove60_Self_Family_Children] As Below60_ParentsAbove60_Self_Family_Children
                                            ,[TaxRule80D_Below60_ParentsAbove60_Parents] As Below60_ParentsAbove60_Parents
                                            ,[TaxRule80D_BothAbove60_Self_Family_Children] As BothAbove60_Self_Family_Children
                                            ,[TaxRule80D_BothAbove60_Parents] As BothAbove60_Parents
                                            ,[TaxRule80D_BothAbove60_DeductionUnder80D_HealthCheckup] As BothAbove60_DeductionUnder80D_HealthCheckup 

                                            ,[TaxRule80DD_Handicap_Severeamount]  As Handicap_SevereAmount
                                            ,[TaxRule80DD_Handicap_Nonsevereamount] As Handicap_NonSevereAmount
                                            ,[TaxRule80DDB_NormalCitizen_Amount] As NormalCitizen_Amount
                                            ,[TaxRule80DDB_SeniorCitizen_Amount] As SeniorCitizen_Amount
                                            ,[TaxRule80U_Physical_Handicap_Severe_Amount] As Physical_Handicap_Severe_Amount
                                            ,[TaxRule80U_Physical_Handicap_NonSevere_Amount] As Physical_Handicap_NonSevere_Amount
                                            ,[TaxRule80CCD_1B_NPS_Amount] As CCD_1B_NPS_Amount
                                            ,[TaxRule80TTA_Amount] As TTA_Amount
                                            ,[TaxRule80TTB_Amount] As TTB_Amount
                                            ,[TaxRule80EEB_Interest_OnElecticVechile] As Interest_OnElecticVechile
                                            from Incometax_rule where  Deactivate=0 and IncomeTaxRule_FyearId= " + Session["TaxFyearId"] + "").FirstOrDefault();
                if (Get80DRules != null)
                {
                    TempData["80D_ParentsBelow60_Self_Family_Children"] = Get80DRules.ParentsBelow60_Self_Family_Children;
                    TempData["80D_ParentsBelow60_Parents"] = Get80DRules.ParentsBelow60_Parents;
                    TempData["80D_Below60_ParentsAbove60_Self_Family_Children"] = Get80DRules.Below60_ParentsAbove60_Self_Family_Children;
                    TempData["80D_Below60_ParentsAbove60_Parents"] = Get80DRules.Below60_ParentsAbove60_Parents;
                    TempData["80D_BothAbove60_Self_Family_Children"] = Get80DRules.BothAbove60_Self_Family_Children;
                    TempData["80D_BothAbove60_Parents"] = Get80DRules.BothAbove60_Parents;
                    TempData["80D_BothAbove60_DeductionUnder80D_HealthCheckup"] = Get80DRules.BothAbove60_DeductionUnder80D_HealthCheckup;
                    TempData["Limit80DD_Handicap_SevereAmount"] = Get80DRules.Handicap_SevereAmount;
                    TempData["Limit80DD_Handicap_NonSevereAmount"] = Get80DRules.Handicap_NonSevereAmount;
                    TempData["Limit80DDB_NormalCitizen_Amount"] = Get80DRules.NormalCitizen_Amount;
                    TempData["Limit80DDB_SeniorCitizen_Amount"] = Get80DRules.SeniorCitizen_Amount;
                    TempData["Limit80U_Physical_Handicap_Severe_Amount"] = Get80DRules.Physical_Handicap_Severe_Amount;
                    TempData["Limit80U_Physical_Handicap_Nonsevere_Amount"] = Get80DRules.Physical_Handicap_NonSevere_Amount;
                    TempData["Limit80CCD_1B_NPS_Amount"] = Get80DRules.CCD_1B_NPS_Amount;
                    TempData["Limit80TTA_Amount"] = Get80DRules.TTA_Amount;
                    TempData["Limit80TTB_Amount"] = Get80DRules.TTB_Amount;
                    TempData["Limit80EEB_Interes_Onelecticvechile"] = Get80DRules.Interest_OnElecticVechile;
                }

                //Check Submit button valid or not based on open close taxDeclaration beetween Date
                var CheckSubmit = DapperORM.ExecuteQuery(@"Select top 1 [Month],[FromDay],[ToDay] from[IncomeTax_DocumentOpenClose] where Deactivate=0 and  [Month]=Month(Getdate()) and OpenCloseTypeID =1 and day(Getdate()) Between [FromDay] and ([ToDay])");
                TempData["SubmitValid"] = CheckSubmit;

                InvestmentChapterVI_A ChapterVI_Aobj = new InvestmentChapterVI_A();
                var FYId = Session["TaxFyearId"];
                if (ChapterVI_Aobj != null)
                {
                    param.Add("@p_InvestmentDeclarationFyearId", FYId);
                    param.Add("@p_InvestmentDeclarationEmployeeId", Session["EmployeeId"]);
                    ChapterVI_Aobj = DapperORM.ReturnList<InvestmentChapterVI_A>("sp_List_IncomeTax_InvestmentDeclaration_ChapterVI_A", param).FirstOrDefault();
                }

                if (ChapterVI_Aobj != null)
                {
                    TempData["ChapterVIA_TotalAmount"] = ChapterVI_Aobj.ChapterVIA_TotalAmount;

                    TempData["ChapterVIA_Medical80D_Self_CurrentAmt"] = ChapterVI_Aobj.ChapterVIA_Medical80D_Self_CurrentAmt;
                    TempData["ChapterVIA_Medical80D_Self_HealthCheckup_CurrentAmt"] = ChapterVI_Aobj.ChapterVIA_Medical80D_Self_HealthCheckup_CurrentAmt;
                    TempData["ChapterVIA_Medical80D_Parents_CurrentAmt"] = ChapterVI_Aobj.ChapterVIA_Medical80D_Parents_CurrentAmt;
                    TempData["ChapterVIA_Medical80D_HealthCheckup_CurrentAmt"] = ChapterVI_Aobj.ChapterVIA_Medical80D_HealthCheckup_CurrentAmt;
                    if (ChapterVI_Aobj.ChapterVIA_VehiclePurchaseDate.HasValue)
                    {
                        TempData["ChapterVIA_VehiclePurchaseDate"] = ChapterVI_Aobj.ChapterVIA_VehiclePurchaseDate.Value.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        TempData["ChapterVIA_VehiclePurchaseDate"] = string.Empty; // Or handle appropriately
                    }
                }
                return View(ChapterVI_Aobj);
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
        public ActionResult SaveUpdate(List<Add80G_A> Add80G_A, List<Add80G_B> Add80G_B, InvestmentChapterVI_A Record)
        {
            try
            {
                var EmpId = Session["EmployeeId"];
                param.Add("@p_process", "Save");
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                //   param.Add("@P_InvestmentDeclarationId", Record.InvestmentDeclarationId);
                // param.Add("@P_InvestmentDeclaration_Encrypted", Record.InvestmentDeclaration_Encrypted);
                param.Add("@P_InvestmentDeclarationFyearId", Session["TaxFyearId"]);
                param.Add("@P_InvestmentDeclarationEmployeeId", Session["EmployeeId"]);
                param.Add("@P_InvestmentDeclarationEmployeeNo", Record.InvestmentDeclarationEmployeeNo);
                param.Add("@P_InvestmentDeclarationEmployeeName", Record.InvestmentDeclarationEmployeeName);

                param.Add("@p_ChapterVIA_Medical80D_IsSelfCoverd", Record.ChapterVIA_Medical80D_IsSelfCoverd);
                param.Add("@p_ChapterVIA_Medical80D_Self_CurrentAmt", Record.ChapterVIA_Medical80D_Self_CurrentAmt);
                param.Add("@p_ChapterVIA_Medical80D_Self_LimitAmt", Record.ChapterVIA_Medical80D_Self_LimitAmt);
                param.Add("@p_ChapterVIA_Medical80D_Self_EligibleDeductionAmt", Record.ChapterVIA_Medical80D_Self_EligibleDeductionAmt);
                param.Add("@p_ChapterVIA_Medical80D_IsAreYourParentCovered", Record.ChapterVIA_Medical80D_IsAreYourParentCovered);
                param.Add("@p_ChapterVIA_Medical80D_IsParentSeniorCitizen", Record.ChapterVIA_Medical80D_IsParentSeniorCitizen);
                param.Add("@p_ChapterVIA_Medical80D_Parents_CurrentAmt", Record.ChapterVIA_Medical80D_Parents_CurrentAmt);
                param.Add("@p_ChapterVIA_Medical80D_Parents_LimitAmt", Record.ChapterVIA_Medical80D_Parents_LimitAmt);
                param.Add("@p_ChapterVIA_Medical80D_Parents_EligibleDeductionAmt", Record.ChapterVIA_Medical80D_Parents_EligibleDeductionAmt);
                param.Add("@p_ChapterVIA_Medical80D_HealthCheckup_CurrentAmt", Record.ChapterVIA_Medical80D_HealthCheckup_CurrentAmt);
                param.Add("@p_ChapterVIA_Medical80D_HealthCheckup_LimitAmt", Record.ChapterVIA_Medical80D_HealthCheckup_LimitAmt);
                param.Add("@p_ChapterVIA_Medical80D_HealthCheckup_EligibleDeductionAmt", Record.ChapterVIA_Medical80D_HealthCheckup_EligibleDeductionAmt);
                param.Add("@p_ChapterVIA_Medical80D_FinalEligibleDeduction", Record.ChapterVIA_Medical80D_FinalEligibleDeduction);
                param.Add("@p_ChapterVIA_HandicappedDependent80DD_IsHandicappedDependent", Record.ChapterVIA_HandicappedDependent80DD_IsHandicappedDependent);
                param.Add("@p_ChapterVIA_HandicappedDependent80DD_IsSevere", Record.ChapterVIA_HandicappedDependent80DD_IsSevere);
                param.Add("@p_ChapterVIA_HandicappedDependent80DD_IsHandicappedDependent_LimitAmt", Record.ChapterVIA_HandicappedDependent80DD_IsHandicappedDependent_LimitAmt);
                param.Add("@p_ChapterVIA_HandicappedDependent80DD_IsHandicappedDependent_EligibleDeductionAmt", Record.ChapterVIA_HandicappedDependent80DD_IsHandicappedDependent_EligibleDeductionAmt);
                param.Add("@p_ChapterVIA_TreatmentofSpecifiedDiseases80DDB_IsSeniorCitizen", Record.ChapterVIA_TreatmentofSpecifiedDiseases80DDB_IsSeniorCitizen);
                param.Add("@p_ChapterVIA_TreatmentofSpecifiedDiseases80DDB_CurrentAmt", Record.ChapterVIA_TreatmentofSpecifiedDiseases80DDB_CurrentAmt);
                param.Add("@p_ChapterVIA_TreatmentofSpecifiedDiseases80DDB_LimitAmt", Record.ChapterVIA_TreatmentofSpecifiedDiseases80DDB_LimitAmt);
                param.Add("@p_ChapterVIA_TreatmentofSpecifiedDiseases80DDB_EligibleDeductionAmt", Record.ChapterVIA_TreatmentofSpecifiedDiseases80DDB_EligibleDeductionAmt);
                param.Add("@p_ChapterVIA_TreatmentofSpecifiedDiseases80DDB_AmountRecoveredFromInsuranceCompany_CurrentAmt", Record.ChapterVIA_TreatmentofSpecifiedDiseases80DDB_AmountRecoveredFromInsuranceCompany_CurrentAmt);
                param.Add("@p_ChapterVIA_InterestOnEducationalLoan80E_IsFullTime", Record.ChapterVIA_InterestOnEducationalLoan80E_IsFullTime);
                param.Add("@p_ChapterVIA_InterestOnEducationalLoan80E_CurrentAmt", Record.ChapterVIA_InterestOnEducationalLoan80E_CurrentAmt);
                param.Add("@p_ChapterVIA_PhysicallyHandicapped80U_IsPhysicallyHandicapped", Record.ChapterVIA_PhysicallyHandicapped80U_IsPhysicallyHandicapped);
                param.Add("@p_ChapterVIA_PhysicallyHandicapped80U_IsSevere", Record.ChapterVIA_PhysicallyHandicapped80U_IsSevere);
                param.Add("@p_ChapterVIA_PhysicallyHandicapped80U_LimitAmt", Record.ChapterVIA_PhysicallyHandicapped80U_LimitAmt);
                param.Add("@p_ChapterVIA_PhysicallyHandicapped80U_EligibleDeductionAmt", Record.ChapterVIA_PhysicallyHandicapped80U_EligibleDeductionAmt);
                param.Add("@p_ChapterVIA_AdditionalcontributionToNPS80CCD1B_CurrentAmt", Record.ChapterVIA_AdditionalcontributionToNPS80CCD1B_CurrentAmt);
                param.Add("@p_ChapterVIA_AdditionalcontributionToNPS80CCD1B_LimitAmt", Record.ChapterVIA_AdditionalcontributionToNPS80CCD1B_LimitAmt);
                param.Add("@p_ChapterVIA_AdditionalcontributionToNPS80CCD1B_EligibleDeductionAmt", Record.ChapterVIA_AdditionalcontributionToNPS80CCD1B_EligibleDeductionAmt);
                param.Add("@p_ChapterVIA_InterestondepositionSavingaccount80TTA_CurrentAmt", Record.ChapterVIA_InterestondepositionSavingaccount80TTA_CurrentAmt);
                param.Add("@p_ChapterVIA_InterestondepositionSavingaccount80TTA_LimitAmt", Record.ChapterVIA_InterestondepositionSavingaccount80TTA_LimitAmt);
                param.Add("@p_ChapterVIA_InterestondepositionSavingaccount80TTA_EligibleDeductionAmt", Record.ChapterVIA_InterestondepositionSavingaccount80TTA_EligibleDeductionAmt);
                param.Add("@p_ChapterVIA_InterestondepositionSavingaccount80TTB_CurrentAmt", Record.ChapterVIA_InterestondepositionSavingaccount80TTB_CurrentAmt);
                param.Add("@p_ChapterVIA_InterestondepositionSavingaccount80TTB_LimitAmt", Record.ChapterVIA_InterestondepositionSavingaccount80TTB_LimitAmt);
                param.Add("@p_ChapterVIA_InterestondepositionSavingaccount80TTB_EligibleDeductionAmt", Record.ChapterVIA_InterestondepositionSavingaccount80TTB_EligibleDeductionAmt);
                param.Add("@p_ChapterVIA_InterestOnLoanForPurchaseOfElectricVehicle80EEB_CurrentAmt", Record.ChapterVIA_InterestOnLoanForPurchaseOfElectricVehicle80EEB_CurrentAmt);
                param.Add("@p_ChapterVIA_InterestOnLoanForPurchaseOfElectricVehicle80EEB_LimitAmt", Record.ChapterVIA_InterestOnLoanForPurchaseOfElectricVehicle80EEB_LimitAmt);
                param.Add("@p_ChapterVIA_InterestOnLoanForPurchaseOfElectricVehicle80EEB_EligibleDeductionAmt", Record.ChapterVIA_InterestOnLoanForPurchaseOfElectricVehicle80EEB_EligibleDeductionAmt);
                param.Add("@p_ChapterVIA_VehiclePurchaseDate", Record.ChapterVIA_VehiclePurchaseDate);
                param.Add("@p_ChapterVIA_Donations80G_100Per_CurrentAmt", Record.ChapterVIA_Donations80G_100Per_CurrentAmt);
                param.Add("@p_ChapterVIA_Donations80G_100Per_Limit", Record.ChapterVIA_Donations80G_100Per_Limit);
                param.Add("@p_ChapterVIA_Donations80G_100Per_EligibleDeductionAmt", Record.ChapterVIA_Donations80G_100Per_EligibleDeductionAmt);
                param.Add("@p_ChapterVIA_Donations80G_100Per_Remark", Record.ChapterVIA_Donations80G_100Per_Remark);
                param.Add("@p_ChapterVIA_Donations80G_50Per_CurrentAmt", Record.ChapterVIA_Donations80G_50Per_CurrentAmt);
                param.Add("@p_ChapterVIA_Donations80G_50Per_Limit", Record.ChapterVIA_Donations80G_50Per_Limit);
                param.Add("@p_ChapterVIA_Donations80G_50Per_EligibleDeductionAmt", Record.ChapterVIA_Donations80G_50Per_EligibleDeductionAmt);
                param.Add("@p_ChapterVIA_Donations80G_50Per_Remark", Record.ChapterVIA_Donations80G_50Per_Remark);
                param.Add("@p_ChapterVIA_Donations80G_TempleMosqueGurudwara50Per_CurrentAmt", Record.ChapterVIA_Donations80G_TempleMosqueGurudwara50Per_CurrentAmt);
                param.Add("@p_ChapterVIA_Donations80G_TempleMosqueGurudwara50Per_Limit", Record.ChapterVIA_Donations80G_TempleMosqueGurudwara50Per_Limit);
                param.Add("@p_ChapterVIA_Donations80G_TempleMosqueGurudwara50Per_EligibleDeductionAmt", Record.ChapterVIA_Donations80G_TempleMosqueGurudwara50Per_EligibleDeductionAmt);
                param.Add("@p_ChapterVIA_Donations80GGA_ScientificResearch_Ruraldevelopment_CurrentAmt", Record.ChapterVIA_Donations80GGA_ScientificResearch_Ruraldevelopment_CurrentAmt);
                param.Add("@p_ChapterVIA_Donations80GGA_ScientificResearch_Ruraldevelopment_Limit", Record.ChapterVIA_Donations80GGA_ScientificResearch_Ruraldevelopment_Limit);
                param.Add("@p_ChapterVIA_Donations80GGA_ScientificResearch_Ruraldevelopment_EligibleDeductionAmt", Record.ChapterVIA_Donations80GGA_ScientificResearch_Ruraldevelopment_EligibleDeductionAmt);
                param.Add("@p_ChapterVIA_Donations80GGC_PoliticalParties_CurrentAmt", Record.ChapterVIA_Donations80GGC_PoliticalParties_CurrentAmt);
                param.Add("@p_ChapterVIA_Donations80GGC_PoliticalParties_Limit", Record.ChapterVIA_Donations80GGC_PoliticalParties_Limit);
                param.Add("@p_ChapterVIA_Donations80GGC_PoliticalParties_EligibleDeductionAmt", Record.ChapterVIA_Donations80GGC_PoliticalParties_EligibleDeductionAmt);
                param.Add("@p_ChapterVIA_TotalAmount", Record.ChapterVIA_TotalAmount);

                param.Add("@p_ChapterVIA_Medical80D_Self_HealthCheckup_CurrentAmt", Record.ChapterVIA_Medical80D_Self_HealthCheckup_CurrentAmt);
                param.Add("@p_ChapterVIA_Medical80D_Self_HealthCheckup_LimitAmt", Record.ChapterVIA_Medical80D_Self_HealthCheckup_LimitAmt);
                param.Add("@p_ChapterVIA_Medical80D_Self_HealthCheckup_EligibleDeductionAmt", Record.ChapterVIA_Medical80D_Self_HealthCheckup_EligibleDeductionAmt);

                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_InvestmentDeclaration_ChapterVIA", param);
                TempData["Message"] = param.Get<string>("@p_msg"); ;
                TempData["Icon"] = param.Get<string>("@p_Icon"); ;
                return RedirectToAction("ESS_Tax_Investment_ChapterVI_A", "ESS_Tax_Investment_ChapterVI_A");
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