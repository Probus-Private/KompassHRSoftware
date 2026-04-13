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
    public class ESS_Tax_SelfOccupiedHouseLoanController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: ESS/ESS_Tax_SelfOccupiedHouseLoan
        #region ESS_Tax_SelfOccupiedHouseLoan
        public ActionResult ESS_Tax_SelfOccupiedHouseLoan()
        {
            try
            {

                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 528;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var CheckSubmit = DapperORM.ExecuteQuery(@"Select top 1 [Month],[FromDay],[ToDay] from[IncomeTax_DocumentOpenClose] where Deactivate=0 and  [Month]=Month(Getdate()) and OpenCloseTypeID =1 and day(Getdate()) Between [FromDay] and ([ToDay])");
                TempData["SubmitValid"] = CheckSubmit;

                var GetTaxRuleLimit = DapperORM.ExecuteQuery("Select TaxRule80EE_Limit,TaxRule80EEA_Limit,Housingloan_After_1999,Housingloan_Before_1999,Housingloan_RepairOrRenewal from IncomeTax_Rule where deactivate=0 and  IncomeTaxRule_FyearId= " + Session["TaxFyearId"] + " ");
                if (GetTaxRuleLimit != null)
                {
                    ViewBag.GetTaxRuleLimit = GetTaxRuleLimit;
                }
                InvestmentDeclaration_SelfOccupiedHouseLoan OBJSelfOccupiedHouseLoan = new InvestmentDeclaration_SelfOccupiedHouseLoan();
                var FYId = Session["TaxFyearId"];
                if (OBJSelfOccupiedHouseLoan != null)
                {
                    param.Add("@p_InvestmentDeclarationFyearId", FYId);
                    param.Add("@p_InvestmentDeclarationEmployeeId", Session["EmployeeId"]);
                    OBJSelfOccupiedHouseLoan = DapperORM.ReturnList<InvestmentDeclaration_SelfOccupiedHouseLoan>("sp_List_IncomeTax_InvestmentDeclaration_SelfOccupiedHouseLoan", param).FirstOrDefault();
                    ViewBag.SelfOccupied_IsCoOwner = OBJSelfOccupiedHouseLoan?.SelfOccupied_IsCoOwner;
                }
                return View(OBJSelfOccupiedHouseLoan);
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
        public ActionResult SaveUpdate(InvestmentDeclaration_SelfOccupiedHouseLoan InvestmentDeclaration, HttpPostedFileBase AttachFile)
        {
            try
            {
                // param.Add("@p_process", string.IsNullOrEmpty(InvestmentDeclaration.InvestmentDeclaration_Encrypted) ? "Save" : "Update");
                param.Add("@p_InvestmentDeclarationFyearId", Session["TaxFyearId"]);
                param.Add("@p_InvestmentDeclarationEmployeeId", Session["EmployeeId"]);



                param.Add("@p_SelfOccupied_TypeID", InvestmentDeclaration.SelfOccupied_TypeID);
                param.Add("@p_SelfOccupied_CurrentAmt", InvestmentDeclaration.SelfOccupied_CurrentAmt);
                param.Add("@p_SelfOccupied_Limit", InvestmentDeclaration.SelfOccupied_Limit);
                param.Add("@p_SelfOccupied_EligibleDeductionAmt", InvestmentDeclaration.SelfOccupied_EligibleDeductionAmt);
                if (InvestmentDeclaration.SelfOccupied_TypeID == 1)
                {
                    param.Add("@p_SelfOccupied_Type", "Loan On or After 01/04/1999");
                }
                else if (InvestmentDeclaration.SelfOccupied_TypeID == 2)
                {
                    param.Add("@p_SelfOccupied_Type", "Loan Before 01/04/1999");
                }
                else if (InvestmentDeclaration.SelfOccupied_TypeID == 3)
                {
                    param.Add("@p_SelfOccupied_Type", "Loan On or After 01/04/1999 (First Home (Section 80EEA) (01/Apr/2019 - 31/Mar/2022))");
                }
                else if (InvestmentDeclaration.SelfOccupied_TypeID == 4)
                {
                    param.Add("@p_SelfOccupied_Type", "Loan Before 01/04/1999 (First Home (Section 80EEA) (01/Apr/2019 - 31/Mar/2022))");
                }
                else if (InvestmentDeclaration.SelfOccupied_TypeID == 5)
                {
                    param.Add("@p_SelfOccupied_Type", "Loan On or After 01/04/1999 (First Home (Section 80EE) (01/Apr/2016 - 31/Mar/2017))");
                }
                else if (InvestmentDeclaration.SelfOccupied_TypeID == 6)
                {
                    param.Add("@p_SelfOccupied_Type", "Loan Before 01/04/1999 (First Home (Section 80EE) (01/Apr/2016 - 31/Mar/2017))");
                }
                else if (InvestmentDeclaration.SelfOccupied_TypeID == 7)
                {
                    param.Add("@p_SelfOccupied_Type", "Repair or renewal or reconstruction of the house");
                }
                param.Add("@p_SelfOccupied_IsCoOwner", InvestmentDeclaration.SelfOccupied_IsCoOwner);
                param.Add("@p_SelfOccupied_Co_Applicant_Percentage", InvestmentDeclaration.SelfOccupied_Co_Applicant_Percentage);
                param.Add("@p_CreatedUpdateBy", Convert.ToInt32(Session["EmployeeId"]));
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_InvestmentDeclaration_SelfOccupiedHouseLoan", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;

                if (Icon == "success")
                {
                    if (AttachFile != null)
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET SelfOccupied_ProofUpload = '" + Session["EmployeeNo"] + " - " + "SelfOccupiedHouseLoanRepayments" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

                        var GetDocPath = DapperORM.DynamicQuerySingle("SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin = 'IncomeTax_Investment'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        var FirstPath = Path.Combine(GetFirstPath, Session["EmployeeNo"].ToString()) + "\\";

                        if (!Directory.Exists(FirstPath))
                        {
                            Directory.CreateDirectory(FirstPath);
                        }
                        else
                        {
                            // **Delete only the file matching "{EmployeeId} - HRA.*" pattern**
                            try
                            {
                                string searchPattern = Session["EmployeeNo"] + " - SelfOccupiedHouseLoanRepayments.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_SelfOccupiedHouseLoan", "ESS_Tax_SelfOccupiedHouseLoan");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_SelfOccupiedHouseLoan", "ESS_Tax_SelfOccupiedHouseLoan");
                            }
                        }

                        // Extract file extension
                        string fileExtension = Path.GetExtension(AttachFile.FileName);
                        string newFileName = Session["EmployeeNo"] + " - " + "SelfOccupiedHouseLoanRepayments" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);
                    }
                }
                return RedirectToAction("ESS_Tax_SelfOccupiedHouseLoan", "ESS_Tax_SelfOccupiedHouseLoan");

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