using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Tax;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Tax
{
    public class ESS_Tax_Investment_UnderSection80CController : Controller
    {
        // GET: ESS/ESS_Tax_Investment_UnderSection80C
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        #region ESS_Tax_Investment_UnderSection80C Main View 
        [HttpGet]
        public ActionResult ESS_Tax_Investment_UnderSection(string InvestmentDeclaration_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 178;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
                var Amount80C = DapperORM.DynamicQueryList("Select [TaxRule80C_Amount] As Limit80C From IncomeTax_Rule where  Deactivate=0 and IncomeTaxRule_FyearId= " + Session["TaxFyearId"] + " ").FirstOrDefault();
                // Amount80C = DapperORM.DynamicQuerySingle("Select [TaxRule80C_Amount] As Limit80C From IncomeTax_Rule where  Deactivate=0 and Year(FromDate)=year(GetDate())");
                if (Amount80C != null)
                {
                    TempData["80C_Amount"] = Amount80C.Limit80C;
                }

                var GetEmpoyee = "Select * from View_IncomeTax_Employee Where EmployeeID = " + Session["EmployeeId"] + "";
                var IncomeTaxUS80C = DapperORM.ExecuteQuery(GetEmpoyee);
                if (IncomeTaxUS80C != null)
                {
                    ViewBag.GetUS80CEmployee = IncomeTaxUS80C;
                }

                var Date = DapperORM.DynamicQueryList(" Select US80C_SubmitDate, US80C_SubmitCount from IncomeTax_InvestmentDeclaration where Deactivate = 0  and [InvestmentDeclarationFyearId] = " + Session["TaxFyearId"] + " and [InvestmentDeclarationEmployeeId] = " + Session["EmployeeId"] + "").FirstOrDefault();
                TempData["US80C_SubmitDate"] = null;
                TempData["US80C_SubmitCount"] = null;
                if (Date != null)
                {
                    TempData["US80C_SubmitDate"] = Date.US80C_SubmitDate;
                    TempData["US80C_SubmitCount"] = Date.US80C_SubmitCount;
                }
                //Check Submit button valid or not based on open close taxDeclaration beetween Date
                var CheckSubmit = DapperORM.ExecuteQuery(@"Select top 1 [Month],[FromDay],[ToDay] from[IncomeTax_DocumentOpenClose] where Deactivate=0 and  [Month]=Month(Getdate()) and OpenCloseTypeID =1 and day(Getdate()) Between [FromDay] and ([ToDay])");
                TempData["SubmitValid"] = CheckSubmit;

                // var GetFinancialYear = 1;
                InvestmentDeclaration_80C InvestmentDeclaration = new InvestmentDeclaration_80C();
                ViewBag.AddUpdateTitle = "Add";

                //if (GetFinancialYear == 1 && Convert.ToInt32(Session["EmployeeId"]) == 20)
                //{

                param.Add("@p_InvestmentDeclarationFyearId", Session["TaxFyearId"]);
                param.Add("@p_InvestmentDeclarationEmployeeId", Session["EmployeeId"]);
                param.Add("@p_EmployeeNo", Session["EmployeeNo"]);
                InvestmentDeclaration = DapperORM.ReturnList<InvestmentDeclaration_80C>("sp_List_IncomeTax_InvestmentDeclaration_80C", param).FirstOrDefault();
                if (InvestmentDeclaration != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                }

                return View(InvestmentDeclaration);
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
        public ActionResult SaveUpdate(InvestmentDeclaration_80C InvestmentDeclaration, HttpPostedFileBase AttachFile)
        {
            try
            {
                //        param.Add("@query", "Select  * from Mas_Employee where EmployeeId='" + Convert.ToInt32(Session["EmployeeId"]) + "'");
                //         var Employee = DapperORM.ReturnList<Mas_Employee>("sp_QueryExcution", param).FirstOrDefault();

                //param.Add("@query", "Select  Fid, TDSYear from Tool_FinancialYear where TDSActive=1");
                //var GetFinancialYear = DapperORM.ReturnList<Tool_FinancialYear>("sp_QueryExcution", param).ToList();
                //ViewBag.FinancialYear = GetFinancialYear;

                param.Add("@p_process", string.IsNullOrEmpty(InvestmentDeclaration.InvestmentDeclaration_Encrypted) ? "Save" : "Update");
                param.Add("@p_InvestmentDeclarationFyearId", Session["TaxFyearId"]);
                param.Add("@p_InvestmentDeclarationEmployeeId", Session["EmployeeId"]);
                //param.Add("@p_InvestmentDeclarationEmployeeNo", 20);
                //param.Add("@p_InvestmentDeclarationEmployeeName", "Rahul");


                param.Add("@p_US80C_EmployeesNationalPensionScheme80CCD1_CurrentAmt", InvestmentDeclaration.US80C_EmployeesNationalPensionScheme80CCD1_CurrentAmt);
                param.Add("@p_US80C_EmployeesNationalPensionScheme80CCD1_LimitAmt", InvestmentDeclaration.US80C_EmployeesNationalPensionScheme80CCD1_LimitAmt);
                param.Add("@p_US80C_EmployeesNationalPensionScheme80CCD1_EligibleDeductionAmt", InvestmentDeclaration.US80C_EmployeesNationalPensionScheme80CCD1_EligibleDeductionAmt);
                param.Add("@p_US80C_PensionPlanfromInsuranceCompaniesMutualFunds_CurrentAmt", InvestmentDeclaration.US80C_PensionPlanfromInsuranceCompaniesMutualFunds_CurrentAmt);
                param.Add("@p_US80C_PensionPlanfromInsuranceCompaniesMutualFunds_LimitAmt", InvestmentDeclaration.US80C_PensionPlanfromInsuranceCompaniesMutualFunds_LimitAmt);
                param.Add("@p_US80C_PensionPlanfromInsuranceCompaniesMutualFunds_EligibleDeductionAmt", InvestmentDeclaration.US80C_PensionPlanfromInsuranceCompaniesMutualFunds_EligibleDeductionAmt);
                param.Add("@p_US80C_HousingLoanPrincipalRepayment_CurrentAmt", InvestmentDeclaration.US80C_HousingLoanPrincipalRepayment_CurrentAmt);
                param.Add("@p_US80C_HousingLoanPrincipalRepayment_LimitAmt", InvestmentDeclaration.US80C_HousingLoanPrincipalRepayment_LimitAmt);
                param.Add("@p_US80C_HousingLoanPrincipalRepayment_EligibleDeductionAmt", InvestmentDeclaration.US80C_HousingLoanPrincipalRepayment_EligibleDeductionAmt);
                param.Add("@p_US80C_LifeInsurancePremium_CurrentAmt", InvestmentDeclaration.US80C_LifeInsurancePremium_CurrentAmt);
                param.Add("@p_US80C_LifeInsurancePremium_LimitAmt", InvestmentDeclaration.US80C_LifeInsurancePremium_LimitAmt);
                param.Add("@p_US80C_LifeInsurancePremium_EligibleDeductionAmt", InvestmentDeclaration.US80C_LifeInsurancePremium_EligibleDeductionAmt);
                param.Add("@p_US80C_NationalSavingCertificate_CurrentAmt", InvestmentDeclaration.US80C_NationalSavingCertificate_CurrentAmt);
                param.Add("@p_US80C_NationalSavingCertificate_LimitAmt", InvestmentDeclaration.US80C_NationalSavingCertificate_LimitAmt);
                param.Add("@p_US80C_NationalSavingCertificate_EligibleDeductionAmt", InvestmentDeclaration.US80C_NationalSavingCertificate_EligibleDeductionAmt);
                param.Add("@p_US80C_NationalSavingCertificateAccruedInterest_CurrentAmt", InvestmentDeclaration.US80C_NationalSavingCertificateAccruedInterest_CurrentAmt);
                param.Add("@p_US80C_NationalSavingCertificateAccruedInterest_LimitAmt", InvestmentDeclaration.US80C_NationalSavingCertificateAccruedInterest_LimitAmt);
                param.Add("@p_US80C_NationalSavingCertificateAccruedInterest_EligibleDeductionAmt", InvestmentDeclaration.US80C_NationalSavingCertificateAccruedInterest_EligibleDeductionAmt);
                param.Add("@p_US80C_PublicProvidentFund_CurrentAmt", InvestmentDeclaration.US80C_PublicProvidentFund_CurrentAmt);
                param.Add("@p_US80C_PublicProvidentFund_LimitAmt", InvestmentDeclaration.US80C_PublicProvidentFund_LimitAmt);
                param.Add("@p_US80C_PublicProvidentFund_EligibleDeductionAmt", InvestmentDeclaration.US80C_PublicProvidentFund_EligibleDeductionAmt);
                param.Add("@p_US80C_UnitLinkedInsurancePlan_CurrentAmt", InvestmentDeclaration.US80C_UnitLinkedInsurancePlan_CurrentAmt);
                param.Add("@p_US80C_UnitLinkedInsurancePlan_LimitAmt", InvestmentDeclaration.US80C_UnitLinkedInsurancePlan_LimitAmt);
                param.Add("@p_US80C_UnitLinkedInsurancePlan_EligibleDeductionAmt", InvestmentDeclaration.US80C_UnitLinkedInsurancePlan_EligibleDeductionAmt);
                param.Add("@p_US80C_ELSSTaxSavingMutualFund_CurrentAmt", InvestmentDeclaration.US80C_ELSSTaxSavingMutualFund_CurrentAmt);
                param.Add("@p_US80C_ELSSTaxSavingMutualFund_LimitAmt", InvestmentDeclaration.US80C_ELSSTaxSavingMutualFund_LimitAmt);
                param.Add("@p_US80C_ELSSTaxSavingMutualFund_EligibleDeductionAmt", InvestmentDeclaration.US80C_ELSSTaxSavingMutualFund_EligibleDeductionAmt);
                param.Add("@p_US80C_Tuitionfeesfor2children_CurrentAmt", InvestmentDeclaration.US80C_Tuitionfeesfor2children_CurrentAmt);
                param.Add("@p_US80C_Tuitionfeesfor2children_LimitAmt", InvestmentDeclaration.US80C_Tuitionfeesfor2children_LimitAmt);
                param.Add("@p_US80C_Tuitionfeesfor2children_EligibleDeductionAmt", InvestmentDeclaration.US80C_Tuitionfeesfor2children_EligibleDeductionAmt);
                param.Add("@p_US80C_TaxSavingSharesNABARDandOtherBonds_CurrentAmt", InvestmentDeclaration.US80C_TaxSavingSharesNABARDandOtherBonds_CurrentAmt);
                param.Add("@p_US80C_TaxSavingSharesNABARDandOtherBonds_LimitAmt", InvestmentDeclaration.US80C_TaxSavingSharesNABARDandOtherBonds_LimitAmt);
                param.Add("@p_US80C_TaxSavingSharesNABARDandOtherBonds_EligibleDeductionAmt", InvestmentDeclaration.US80C_TaxSavingSharesNABARDandOtherBonds_EligibleDeductionAmt);
                param.Add("@p_US80C_TaxsavingFixDepositsTenure5yearsormore_CurrentAmt", InvestmentDeclaration.US80C_TaxsavingFixDepositsTenure5yearsormore_CurrentAmt);
                param.Add("@p_US80C_TaxsavingFixDepositsTenure5yearsormore_LimitAmt", InvestmentDeclaration.US80C_TaxsavingFixDepositsTenure5yearsormore_LimitAmt);
                param.Add("@p_US80C_TaxsavingFixDepositsTenure5yearsormore_EligibleDeductionAmt", InvestmentDeclaration.US80C_TaxsavingFixDepositsTenure5yearsormore_EligibleDeductionAmt);
                param.Add("@p_US80C_HousingStampDutyRegistration_CurrentAmt", InvestmentDeclaration.US80C_HousingStampDutyRegistration_CurrentAmt);
                param.Add("@p_US80C_HousingStampDutyRegistration_LimitAmt", InvestmentDeclaration.US80C_HousingStampDutyRegistration_LimitAmt);
                param.Add("@p_US80C_HousingStampDutyRegistration_EligibleDeductionAmt", InvestmentDeclaration.US80C_HousingStampDutyRegistration_EligibleDeductionAmt);
                param.Add("@p_US80C_PostOfficeTimeDepositScheme_CurrentAmt", InvestmentDeclaration.US80C_PostOfficeTimeDepositScheme_CurrentAmt);
                param.Add("@p_US80C_PostOfficeTimeDepositScheme_LimitAmt", InvestmentDeclaration.US80C_PostOfficeTimeDepositScheme_LimitAmt);
                param.Add("@p_US80C_PostOfficeTimeDepositScheme_EligibleDeductionAmt", InvestmentDeclaration.US80C_PostOfficeTimeDepositScheme_EligibleDeductionAmt);
                param.Add("@p_US80C_SeniorCitizenSavingScheme_CurrentAmt", InvestmentDeclaration.US80C_SeniorCitizenSavingScheme_CurrentAmt);
                param.Add("@p_US80C_SeniorCitizenSavingScheme_LimitAmt", InvestmentDeclaration.US80C_SeniorCitizenSavingScheme_LimitAmt);
                param.Add("@p_US80C_SeniorCitizenSavingScheme_EligibleDeductionAmt", InvestmentDeclaration.US80C_SeniorCitizenSavingScheme_EligibleDeductionAmt);
                param.Add("@p_US80C_SukanyaSamriddhiScheme_CurrentAmt", InvestmentDeclaration.US80C_SukanyaSamriddhiScheme_CurrentAmt);
                param.Add("@p_US80C_SukanyaSamriddhiScheme_LimitAmt", InvestmentDeclaration.US80C_SukanyaSamriddhiScheme_LimitAmt);
                param.Add("@p_US80C_SukanyaSamriddhiScheme_EligibleDeductionAmt", InvestmentDeclaration.US80C_SukanyaSamriddhiScheme_EligibleDeductionAmt);
                param.Add("@p_US80C_80CCC_CurrentAmt", InvestmentDeclaration.US80C_80CCC_CurrentAmt);
                param.Add("@p_US80C_80CCC_LimitAmt", InvestmentDeclaration.US80C_80CCC_LimitAmt);
                param.Add("@p_US80C_80CCC_EligibleDeductionAmt", InvestmentDeclaration.US80C_80CCC_EligibleDeductionAmt);
                param.Add("@p_US80C_SuperannuationFundContribution_CurrentAmt", InvestmentDeclaration.US80C_SuperannuationFundContribution_CurrentAmt);
                param.Add("@p_US80C_SuperannuationFundContribution_LimitAmt", InvestmentDeclaration.US80C_SuperannuationFundContribution_LimitAmt);
                param.Add("@p_US80C_SuperannuationFundContribution_EligibleDeductionAmt", InvestmentDeclaration.US80C_SuperannuationFundContribution_EligibleDeductionAmt);


                param.Add("@p_US80C_Total", InvestmentDeclaration.US80C_Total);
                param.Add("@p_US80C_Total_Limit", InvestmentDeclaration.US80C_Total_Limit);
                param.Add("@p_US80C_Total_EligibleDeduction", InvestmentDeclaration.US80C_Total_EligibleDeduction);
                param.Add("@p_CreatedUpdateBy", Convert.ToInt32(Session["EmployeeId"]));
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_InvestmentDeclaration_80C", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;

                if (Icon == "success")
                {
                    if (AttachFile != null)
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_HouseLoanPrincipalRepayment_ProofUpload = '" + Session["EmployeeNo"] + " - " + "HouseLoanPrincipalRepayment" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

                        var GetDocPath = DapperORM.DynamicQuerySingle("SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin = 'IncomeTax_Investment'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        var FirstPath = Path.Combine(GetFirstPath, Session["EmployeeNo"].ToString()) + "\\";

                        if (!Directory.Exists(FirstPath))
                        {
                            Directory.CreateDirectory(FirstPath);
                        }
                        else
                        {
                            // **Delete only the file matching "{EmployeeNo} - HRA.*" pattern**
                            try
                            {
                                string searchPattern = Session["EmployeeNo"] + " - HouseLoanPrincipalRepayment.*"; // Pattern to match (any extension)
                                string[] files = Directory.GetFiles(FirstPath, searchPattern); // Get matching files

                                foreach (string file in files)
                                {
                                    if (System.IO.File.Exists(file))
                                    {
                                        System.IO.File.SetAttributes(file, FileAttributes.Normal);
                                        System.IO.File.Delete(file);
                                    }
                                }
                            }
                            catch (IOException ioEx)
                            {
                                TempData["Message"] = "File deletion error: " + ioEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_UnderSection", "ESS_Tax_Investment_UnderSection80C");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_UnderSection", "ESS_Tax_Investment_UnderSection80C");
                            }
                        }

                        // Extract file extension
                        string fileExtension = Path.GetExtension(AttachFile.FileName);
                        string newFileName = Session["EmployeeNo"] + " - " + "HouseLoanPrincipalRepayment" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);
                    }
                }

                return RedirectToAction("ESS_Tax_Investment_UnderSection");

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