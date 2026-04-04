using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Tax;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Tax
{
    public class ESS_Tax_Investment_PreviousEmployerDetailsController : Controller
    {
        // GET: ESS/ESS_Tax_Investment_PreviousEmployerDetails
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        #region PreviousEmployerDetails Main View
        [HttpGet]
        public ActionResult ESS_Tax_Investment_PreviousEmployerDetails()
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 182;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

                var GetEmpoyee = "Select * from View_IncomeTax_Employee Where EmployeeID = " + Session["EmployeeId"] + "";
                var Previousemployerdetail = DapperORM.DynamicQueryList(GetEmpoyee).FirstOrDefault();
                var JoiningDate = Previousemployerdetail.JoiningDate;
                var FromDate = DapperORM.DynamicQueryList("Select FromDate from IncomeTax_Fyear where Deactivate=0 and IsActive=1 and TaxFyearId= " + Session["TaxFyearId"] + "").FirstOrDefault();
                ViewBag.GetPreviousemployerdetail = Previousemployerdetail;
                DateTime joiningDate = JoiningDate;
                DateTime fromDate = FromDate.FromDate;

                if (joiningDate < fromDate)
                {
                    TempData["Message"] = "Employee is not allowed to fill Previous Employer Details";
                    TempData["Icon"] = "error";
                    return View();
                }

                var Date = DapperORM.DynamicQueryList(" Select  Previous_Er_SubmitDate, Previous_Er_SubmitCount   from IncomeTax_InvestmentDeclaration where Deactivate = 0  and [InvestmentDeclarationFyearId] = " + Session["TaxFyearId"] + " and [InvestmentDeclarationEmployeeId] = " + Session["EmployeeId"] + "").FirstOrDefault();
                TempData["Previous_Er_SubmitDate"] = null;
                TempData["Previous_Er_SubmitCount"] = null;
                if (Date != null)
                {
                    TempData["Previous_Er_SubmitDate"] = Date.Previous_Er_SubmitDate;
                    TempData["Previous_Er_SubmitCount"] = Date.Previous_Er_SubmitCount;
                }

                //Check Submit button valid or not based on open close taxDeclaration beetween Date
                var CheckSubmit = DapperORM.ExecuteQuery(@"Select top 1 [Month],[FromDay],[ToDay] from[IncomeTax_DocumentOpenClose] where Deactivate=0 and  [Month]=Month(Getdate()) and OpenCloseTypeID =1 and day(Getdate()) Between [FromDay] and ([ToDay])");
                TempData["SubmitValid"] = CheckSubmit;
                IncomeTax_InvestmentDeclaration_PreviousEmployer obj = new IncomeTax_InvestmentDeclaration_PreviousEmployer();

                var Er_UnemployedDescription = DapperORM.DynamicQueryList("Select  Previous_Er_Unemployed ,Previous_Er_ProofUpload ,Previous_Er_UnemployedDescription from IncomeTax_InvestmentDeclaration where InvestmentDeclarationFyearId=" + Session["TaxFyearId"] + "  and InvestmentDeclarationEmployeeId =" + Session["EmployeeId"] + " and Deactivate = 0").FirstOrDefault();
                if (Er_UnemployedDescription !=null)
                {
                    if (Er_UnemployedDescription.Previous_Er_Unemployed != null)
                    {
                        if (Er_UnemployedDescription.Previous_Er_Unemployed != null)
                        {
                            obj.Previous_Er_Unemployed = Er_UnemployedDescription.Previous_Er_Unemployed;
                            obj.Previous_Er_UnemployedDescription = Er_UnemployedDescription.Previous_Er_UnemployedDescription;
                            obj.Previous_Er_ProofUpload = Er_UnemployedDescription.Previous_Er_ProofUpload;
                        }
                        else
                        {
                            obj.Previous_Er_Unemployed = false;
                            obj.Previous_Er_UnemployedDescription = "";
                            obj.Previous_Er_ProofUpload = Er_UnemployedDescription.Previous_Er_ProofUpload;
                        }

                    }
                    else
                    {
                        param = new DynamicParameters();

                        param.Add("@p_PreviousEmployerFyearId", Session["TaxFyearId"]);
                        param.Add("@p_PreviousEmployerEmployeeId", Session["EmployeeId"]);
                        var PreviousEmployerDetailsList = DapperORM.ReturnList<IncomeTax_InvestmentDeclaration_PreviousEmployer>("sp_List_IncomeTax_InvestmentDeclaration_PreviousEmployer", param).ToList();
                        ViewBag.PreviousEmployerDetailsListBag = PreviousEmployerDetailsList;
                    }
                }
               

                //if (PreviousEmployerDetailsList != null)
                //{
                //    ViewBag.AddUpdateTitle = "Update";
                //}
                return View(obj);

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
        public ActionResult SaveUpdate(string Task/*List<IncomeTax_InvestmentDeclaration_PreviousEmployer> Record*/, InvestmentDeclaration_TotalPreviousEmployer TotalPreviousEmployer, HttpPostedFileBase AttachFile, bool UnemployedInPreviousYear, string UnemployedInPreviousYearDescription)
        {
            try
            {
                var LetOutHousePropertyEmployeeId = DapperORM.DynamicQuerySingle("select count(EmployeeId) as LockCount from IncomeTax_RegimeDeclaration where Deactivate=0 and   FYearId='" + Session["TaxFyearId"] + "' and EmployeeId= '" + Session["EmployeeId"] + "' and Status='Approved'");
                if (LetOutHousePropertyEmployeeId.LockCount < 0)
                {
                    TempData["Message"] = "Regime status: Approved , Can't Update";
                    TempData["Icon"] = "error";
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }
                List<IncomeTax_InvestmentDeclaration_PreviousEmployer> Record =
               JsonConvert.DeserializeObject<List<IncomeTax_InvestmentDeclaration_PreviousEmployer>>(Task);

                double AmtPrevious_Er_GrossSalary = 0,
                 AmtPrevious_Er_StandardDeduction = 0,
                 AmtPrevious_Er_ProfessionalTax = 0,
                 AmtPrevious_Er_80C = 0,
                 AmtPrevious_Er_TaxDeductedAtSource = 0,
                 AmtPrevious_Er_SelfOccupiedHousePropertyRepayment = 0,
                 AmtPrevious_Er_Mediclaim80D = 0,
                 AmtPrevious_Er_HandicappedDependent80DD = 0,
                 AmtPrevious_Er_TreatmentOfSpecifiedDiseases80DDB = 0,
                 AmtPrevious_Er_InterestOnEducationalLoan80E = 0,
                 AmtPrevious_Er_Physicallyhandicapped80U = 0,
                 AmtPrevious_Er_NPSEmployeeContribution80CCD1B = 0,
                 AmtPrevious_Er_NPSEmployerContribution80CCD2 = 0;
                StringBuilder strBuilder = new StringBuilder();
                DapperORM.ExecuteQuery("Update IncomeTax_InvestmentDeclaration_PreviousEmployer set Deactivate=1 where PreviousEmployerFyearId='" + Session["TaxFyearId"] + "'    and  PreviousEmployerEmployeeId='" + Session["EmployeeId"] + "'");
                if (UnemployedInPreviousYear == true)
                {


                    string InvestmentDeclaration = @" IF EXISTS (
                                                        SELECT InvestmentDeclarationEmployeeId 
                                                        FROM IncomeTax_InvestmentDeclaration 
                                                        WHERE InvestmentDeclarationFyearId = '" + Session["TaxFyearId"] + @"' 
                                                          AND InvestmentDeclarationEmployeeId = '" + Session["EmployeeId"] + @"' 
                                                          AND Deactivate = 0
                                                    )
                                                    BEGIN
                                                        UPDATE IncomeTax_InvestmentDeclaration 
                                                        SET 
                                                            Previous_Er_Unemployed = " + (UnemployedInPreviousYear ? "1" : "0") + @",
                                                            Previous_Er_UnemployedDescription = '" + UnemployedInPreviousYearDescription + @"' 
                                                        WHERE 
                                                            InvestmentDeclarationFyearId = '" + Session["TaxFyearId"] + @"' 
                                                            AND InvestmentDeclarationEmployeeId = '" + Session["EmployeeId"] + @"'
                                                    END
                                                    ELSE
                                                    BEGIN
                                                        INSERT INTO IncomeTax_InvestmentDeclaration (
                                                            CreatedBy, CreatedDate, Deactivate,
                                                            InvestmentDeclarationFyearId,
                                                            InvestmentDeclarationEmployeeId,
                                                            InvestmentDeclarationEmployeeNo,
                                                            InvestmentDeclarationEmployeeName,
                                                            Previous_Er_Unemployed,
                                                            Previous_Er_UnemployedDescription
                                                        )
                                                        VALUES (
                                                            '" + Session["EmployeeName"] + @"', GETDATE(), 0,
                                                            '" + Session["TaxFyearId"] + @"',
                                                            '" + Session["EmployeeId"] + @"',
                                                            '" + Session["EmployeeNo"] + @"',
                                                            '" + Session["EmployeeName"] + @"',
                                                            " + (UnemployedInPreviousYear ? "1" : "0") + @",
                                                            '" + UnemployedInPreviousYearDescription + @"'
                                                        )
                                                    END";

                    strBuilder.Append(InvestmentDeclaration);

                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record save successfully";
                        TempData["Icon"] = "success";
                    }
                    if (abc != "")
                    {
                        DapperORM.DynamicQuerySingle("Insert Into Tool_ErrorLog ( " +
                                    "   Error_Desc " +
                                    " , Error_FormName " +
                                    " , Error_MachinceName " +
                                    " , Error_Date " +
                                    " , Error_UserID " +
                                    " , Error_UserName " + ") values (" +
                                    "'" + strBuilder + "'," +
                                    "'BuklInsert'," +
                                    "'" + Dns.GetHostName().ToString() + "'," +
                                    "GetDate()," +
                                    "'" + Session["EmployeeId"] + "'," +
                                    "'" + Session["EmployeeName"] + "'");
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                    //DapperORM.DynamicQuerySingle("update IncomeTax_InvestmentDeclaration set Previous_Er_Unemployed="+ UnemployedInPreviousYear + ",Previous_Er_UnemployedDescription='"+ UnemployedInPreviousYearDescription + "' where InvestmentDeclarationFyearId='" + Session["TaxFyearId"] + "' and InvestmentDeclarationEmployeeId='" + Session["EmployeeId"] + "'");
                }
                else
                {
                    for (var i = 0; i < Record.Count; i++)
                    {

                        param.Add("@p_process", "Save");
                        param.Add("@p_PreviousEmployerFyearId", Session["TaxFyearId"]);
                        param.Add("@p_PreviousEmployerEmployeeId", Session["EmployeeId"]);
                        param.Add("@p_PreviousEmployerName", Record[i].PreviousEmployerName);
                        param.Add("@p_PreviousEmployerAddress", Record[i].PreviousEmployerAddress);
                        param.Add("@p_PreviousEmployerGrossSalary", Record[i].PreviousEmployerGrossSalary);
                        param.Add("@p_PreviousEmployerStandardDeduction", Record[i].PreviousEmployerStandardDeduction);
                        param.Add("@p_PreviousEmployerProfessionalTax", Record[i].PreviousEmployerProfessionalTax);
                        param.Add("@p_PreviousEmployer80C", Record[i].PreviousEmployer80C);
                        param.Add("@p_PreviousEmployerTaxDeductedAtSource", Record[i].PreviousEmployerTaxDeductedAtSource);
                        param.Add("@p_PreviousEmployerSelfOccupiedHousePropertyRepayment", Record[i].PreviousEmployerSelfOccupiedHousePropertyRepayment);
                        param.Add("@p_PreviousEmployerMediclaim80D", Record[i].PreviousEmployerMediclaim80D);
                        param.Add("@p_PreviousEmployerHandicappedDependent80DD", Record[i].PreviousEmployerHandicappedDependent80DD);
                        param.Add("@p_PreviousEmployerTreatmentOfSpecifiedDiseases80DDB", Record[i].PreviousEmployerTreatmentOfSpecifiedDiseases80DDB);
                        param.Add("@p_PreviousEmployerInterestOnEducationalLoan80E", Record[i].PreviousEmployerInterestOnEducationalLoan80E);
                        param.Add("@p_PreviousEmployerPhysicallyhandicapped80U", Record[i].PreviousEmployerPhysicallyhandicapped80U);
                        param.Add("@p_PreviousEmployerNPSEmployeeContribution80CCD1B", Record[i].PreviousEmployerNPSEmployeeContribution80CCD1B);
                        param.Add("@p_PreviousEmployerNPSEmployerContribution80CCD2", Record[i].PreviousEmployerNPSEmployerContribution80CCD2);

                        //Total Amount 
                        AmtPrevious_Er_GrossSalary = AmtPrevious_Er_GrossSalary + Convert.ToDouble(Record[i].PreviousEmployerGrossSalary);
                        AmtPrevious_Er_StandardDeduction = AmtPrevious_Er_StandardDeduction + Convert.ToDouble(Record[i].PreviousEmployerStandardDeduction);
                        AmtPrevious_Er_ProfessionalTax = AmtPrevious_Er_ProfessionalTax + Convert.ToDouble(Record[i].PreviousEmployerProfessionalTax);
                        AmtPrevious_Er_80C = AmtPrevious_Er_80C + Convert.ToDouble(Record[i].PreviousEmployer80C);
                        AmtPrevious_Er_TaxDeductedAtSource = AmtPrevious_Er_TaxDeductedAtSource + Convert.ToDouble(Record[i].PreviousEmployerTaxDeductedAtSource);
                        AmtPrevious_Er_SelfOccupiedHousePropertyRepayment = AmtPrevious_Er_SelfOccupiedHousePropertyRepayment + Convert.ToDouble(Record[i].PreviousEmployerSelfOccupiedHousePropertyRepayment);
                        AmtPrevious_Er_Mediclaim80D = AmtPrevious_Er_Mediclaim80D + Convert.ToDouble(Record[i].PreviousEmployerMediclaim80D);
                        AmtPrevious_Er_HandicappedDependent80DD = AmtPrevious_Er_HandicappedDependent80DD + Convert.ToDouble(Record[i].PreviousEmployerHandicappedDependent80DD);
                        AmtPrevious_Er_TreatmentOfSpecifiedDiseases80DDB = AmtPrevious_Er_TreatmentOfSpecifiedDiseases80DDB + Convert.ToDouble(Record[i].PreviousEmployerTreatmentOfSpecifiedDiseases80DDB);
                        AmtPrevious_Er_InterestOnEducationalLoan80E = AmtPrevious_Er_InterestOnEducationalLoan80E + Convert.ToDouble(Record[i].PreviousEmployerInterestOnEducationalLoan80E);
                        AmtPrevious_Er_Physicallyhandicapped80U = AmtPrevious_Er_Physicallyhandicapped80U + Convert.ToDouble(Record[i].PreviousEmployerPhysicallyhandicapped80U);
                        AmtPrevious_Er_NPSEmployeeContribution80CCD1B = AmtPrevious_Er_NPSEmployeeContribution80CCD1B + Convert.ToDouble(Record[i].PreviousEmployerNPSEmployeeContribution80CCD1B);
                        AmtPrevious_Er_NPSEmployerContribution80CCD2 = AmtPrevious_Er_NPSEmployerContribution80CCD2 + Convert.ToDouble(Record[i].PreviousEmployerNPSEmployerContribution80CCD2);

                        param.Add("@p_PreviousEmployerJoiningDate", Record[i].PreviousEmployerJoiningDate);
                        param.Add("@p_PreviousEmployerSalaryMonth", Record[i].PreviousEmployerSalaryMonth);
                        param.Add("@p_PreviousEmployerLeavingDate", Record[i].PreviousEmployerLeavingDate);

                        param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                        param.Add("@p_MachineName", Dns.GetHostName().ToString());
                        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        var data1 = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_InvestmentDeclaration_PreviousEmployer", param);
                        var message1 = param.Get<string>("@p_msg");
                        var Icon1 = param.Get<string>("@p_Icon");
                        TempData["Message"] = message1;
                        TempData["Icon"] = Icon1;


                    }
                    param = new DynamicParameters();
                    param.Add("@p_process", "Save");
                    param.Add("@p_InvestmentDeclarationFyearId", Session["TaxFyearId"]);
                    param.Add("@P_InvestmentDeclarationEmployeeId", Session["EmployeeId"]);
                    param.Add("@P_InvestmentDeclarationEmployeeNo", Session["EmployeeNo"]);
                    param.Add("@P_InvestmentDeclarationEmployeeName", Session["EmployeeName"]);

                    param.Add("@p_Previous_Er_GrossSalary", AmtPrevious_Er_GrossSalary);
                    param.Add("@p_Previous_Er_StandardDeduction", AmtPrevious_Er_StandardDeduction);
                    param.Add("@p_Previous_Er_ProfessionalTax", AmtPrevious_Er_ProfessionalTax);
                    param.Add("@p_Previous_Er_80C", AmtPrevious_Er_80C);
                    param.Add("@p_Previous_Er_TaxDeductedAtSource", AmtPrevious_Er_TaxDeductedAtSource);
                    param.Add("@p_Previous_Er_SelfOccupiedHousePropertyRepayment", AmtPrevious_Er_SelfOccupiedHousePropertyRepayment);
                    param.Add("@p_Previous_Er_Mediclaim80D", AmtPrevious_Er_Mediclaim80D);
                    param.Add("@p_Previous_Er_HandicappedDependent80DD ", AmtPrevious_Er_HandicappedDependent80DD);
                    param.Add("@p_Previous_Er_TreatmentOfSpecifiedDiseases80DDB", AmtPrevious_Er_TreatmentOfSpecifiedDiseases80DDB);
                    param.Add("@p_Previous_Er_InterestOnEducationalLoan80E", AmtPrevious_Er_InterestOnEducationalLoan80E);
                    param.Add("@p_Previous_Er_Physicallyhandicapped80U", AmtPrevious_Er_Physicallyhandicapped80U);
                    param.Add("@p_Previous_Er_NPSEmployeeContribution80CCD1B", AmtPrevious_Er_NPSEmployeeContribution80CCD1B);
                    param.Add("@p_Previous_Er_NPSEmployerContribution80CCD2", AmtPrevious_Er_NPSEmployerContribution80CCD2);


                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("[sp_SUD_IncomeTax_InvestmentDeclaration_PreviousEmployerMaster]", param);
                    var message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    TempData["Message"] = message;
                    TempData["Icon"] = Icon;
                }




                if (AttachFile != null)
                {
                    DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET Previous_Er_ProofUpload = '" + Session["EmployeeNo"] + " - " + "PreviousEmployerDetails" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                            string searchPattern = Session["EmployeeNo"] + " - PreviousEmployerDetails.*"; // Pattern to match (any extension)
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
                            return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                            //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                        }
                        catch (UnauthorizedAccessException unAuthEx)
                        {
                            TempData["Message"] = "Permission error: " + unAuthEx.Message;
                            TempData["Icon"] = "error";
                            return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                            //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                        }
                    }

                    // Extract file extension
                    string fileExtension = Path.GetExtension(AttachFile.FileName);
                    string newFileName = Session["EmployeeNo"] + " - " + "PreviousEmployerDetails" + fileExtension; // Renaming file as "HRA" with original extension
                    string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                    // Save the file with the new name
                    AttachFile.SaveAs(AttachFilePath);

                    //TempData["Message"] = "Record Updated Successfully";
                    //TempData["Icon"] = "success";
                    //return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                //return RedirectToAction("PreviousEmployerDetails", "PreviousEmployerDetails");
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