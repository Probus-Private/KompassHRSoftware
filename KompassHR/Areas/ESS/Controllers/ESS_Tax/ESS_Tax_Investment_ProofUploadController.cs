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
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Tax
{
    public class ESS_Tax_Investment_ProofUploadController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_Tax_Investment_ProofUpload
        #region ESS_Tax_Investment_ProofUpload
        public ActionResult ESS_Tax_Investment_ProofUpload()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 536;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                //var ProofUpload = DapperORM.DynamicQuerySingleOrDefault("select HRA_ProofUpload,ChapterVIA_Mediclaim80D_ProofUpload ,ChapterVIA_PreventiveHealthCheckUp_ProofUpload ,ChapterVIA_HandicappedDependent80DD_ProofUpload ,ChapterVIA_TreatmentOfSpecifiedDisease80DDB_ProofUpload ,ChapterVIA_IntrestOnEducationalLoan80E_ProofUpload ,ChapterVIA_PhysicallyHandicapped80U_ProofUpload ,ChapterVIA_AdditionalContributiontoNPS80CCD1B_ProofUpload ,ChapterVIA_IntrestOnDepositInSavingAccount80TTA_ProofUpload ,ChapterVIA_IntrestOnDepositInSavingAccount80TTBSeniorCitizens_ProofUpload ,ChapterVIA_IntrestOnLoanForPurchaseOfElectricVehicle80EEB_ProofUpload ,ChapterVIA_FundsEligibleFor100PerDeductionWithoutLimit80G_ProofUpload ,ChapterVIA_FundsEligibleFor50PerDeductionWithoutLimit80G_ProofUpload ,ChapterVIA_FundsEligibleFor50PerDeductionSubjectMaximumLimit80G_ProofUpload ,ChapterVIA_DonationsForScientificResearchOrRuralDevelopment80GGA_ProofUpload ,ChapterVIA_DonationsForPoliticalParty80GGC_ProofUpload  from IncomeTax_InvestmentDeclaration where Deactivate=0 and InvestmentDeclarationFyearId='" + Session["TaxFyearId"] + "' and InvestmentDeclarationEmployeeId='" + Session["EmployeeId"] + "'");
                //ViewBag.GetProofUpload = ProofUpload;

                param.Add("@p_InvestmentDeclarationEmployeeId", Session["EmployeeId"]);
                param.Add("@p_InvestmentDeclarationFyearId", Session["TaxFyearId"]);
                param.Add("@p_InvestmentDeclarationEmployeeNo", Session["EmployeeNo"]);
                var ProofUpload = DapperORM.ReturnList<dynamic>("sp_GetIncomeTaxInvestmentDeclaration_ProofUpload", param).FirstOrDefault();
                ViewBag.GetProofUpload = ProofUpload;


                return View();
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
        public ActionResult SaveUpdate(HttpPostedFileBase AttachFile, IncomeTax_Fyear OBJProofUpload)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                if (AttachFile != null)
                {
                    if (OBJProofUpload.Origin == "HRA")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET HRA_ProofUpload = '" + Session["EmployeeNo"] + " - " + "HRA" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - HRA.*"; // Pattern to match (any extension)
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
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
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
                        string newFileName = Session["EmployeeNo"] + " - " + "HRA" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon= TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                    else if (OBJProofUpload.Origin == "Mediclaim80D")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_Mediclaim80D_ProofUpload = '" + Session["EmployeeNo"] + " - " + "Mediclaim80D" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - Mediclaim80D.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "Mediclaim80D" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                    else if (OBJProofUpload.Origin == "PreventiveHealthCheckUp")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_PreventiveHealthCheckUp_ProofUpload = '" + Session["EmployeeNo"] + " - " + "PreventiveHealthCheckUp" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - PreventiveHealthCheckUp.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "PreventiveHealthCheckUp" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }

                    else if (OBJProofUpload.Origin == "HandicappedDependent80DD")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_HandicappedDependent80DD_ProofUpload = '" + Session["EmployeeNo"] + " - " + "HandicappedDependent80DD" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - HandicappedDependent80DD.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "HandicappedDependent80DD" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                    else if (OBJProofUpload.Origin == "TreatmentOfSpecifiedDisease")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_TreatmentOfSpecifiedDisease80DDB_ProofUpload = '" + Session["EmployeeNo"] + " - " + "TreatmentOfSpecifiedDisease80DDB" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - TreatmentOfSpecifiedDisease80DDB.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "TreatmentOfSpecifiedDisease80DDB" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                    else if (OBJProofUpload.Origin == "IntrestOnEducationalLoan80E")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_IntrestOnEducationalLoan80E_ProofUpload = '" + Session["EmployeeNo"] + " - " + "IntrestOnEducationalLoan80E" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - IntrestOnEducationalLoan80E.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "IntrestOnEducationalLoan80E" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                    else if (OBJProofUpload.Origin == "PhysicallyHandicapped80U")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_PhysicallyHandicapped80U_ProofUpload = '" + Session["EmployeeNo"] + " - " + "PhysicallyHandicapped80U" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - PhysicallyHandicapped80U.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "PhysicallyHandicapped80U" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }

                    else if (OBJProofUpload.Origin == "AdditionalContributionNPS80CCD1B")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_AdditionalContributiontoNPS80CCD1B_ProofUpload = '" + Session["EmployeeNo"] + " - " + "AdditionalContributionNPS80CCD1B" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - AdditionalContributionNPS80CCD1B.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "AdditionalContributionNPS80CCD1B" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }

                    else if (OBJProofUpload.Origin == "IntrestOnDepositinsavingAccount80TTA")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_IntrestOnDepositInSavingAccount80TTA_ProofUpload = '" + Session["EmployeeNo"] + " - " + "IntrestOnDepositinsavingAccount80TTA" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - IntrestOnDepositinsavingAccount80TTA.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "IntrestOnDepositinsavingAccount80TTA" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                    else if (OBJProofUpload.Origin == "IntrestOnDepositinsavingAccount80TTBSeniorCitizens")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_IntrestOnDepositInSavingAccount80TTBSeniorCitizens_ProofUpload = '" + Session["EmployeeNo"] + " - " + "IntrestOnDepositinsavingAccount80TTBSeniorCitizens" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - IntrestOnDepositinsavingAccount80TTBSeniorCitizens.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "IntrestOnDepositinsavingAccount80TTBSeniorCitizens" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }

                    else if (OBJProofUpload.Origin == "PurchaseofElectricVehicle80EEB")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_IntrestOnLoanForPurchaseOfElectricVehicle80EEB_ProofUpload = '" + Session["EmployeeNo"] + " - " + "IntrestOnLoanForPurchaseOfElectricVehicle80EEB" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - IntrestOnLoanForPurchaseOfElectricVehicle80EEB.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "IntrestOnLoanForPurchaseOfElectricVehicle80EEB" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                    else if (OBJProofUpload.Origin == "FundsEligibleFor100PerDeductionWithoutLimit80G")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_FundsEligibleFor100PerDeductionWithoutLimit80G_ProofUpload = '" + Session["EmployeeNo"] + " - " + "FundsEligibleFor100PerDeductionWithoutLimit80G" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - FundsEligibleFor100PerDeductionWithoutLimit80G.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "FundsEligibleFor100PerDeductionWithoutLimit80G" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                    else if (OBJProofUpload.Origin == "FundsEligibleFor50PerDeductionWithoutLimit80G")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_FundsEligibleFor50PerDeductionWithoutLimit80G_ProofUpload = '" + Session["EmployeeNo"] + " - " + "FundsEligibleFor50PerDeductionWithoutLimit80G" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - FundsEligibleFor50PerDeductionWithoutLimit80G.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "FundsEligibleFor50PerDeductionWithoutLimit80G" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                    else if (OBJProofUpload.Origin == "FundsEligibleFor50PerDeductionSubjectMaximumLimit80G")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_FundsEligibleFor50PerDeductionSubjectMaximumLimit80G_ProofUpload = '" + Session["EmployeeNo"] + " - " + "FundsEligibleFor50PerDeductionSubjectMaximumLimit80G" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - FundsEligibleFor50PerDeductionSubjectMaximumLimit80G.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "FundsEligibleFor50PerDeductionSubjectMaximumLimit80G" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }

                    else if (OBJProofUpload.Origin == "DonationsForScientificResearchOrRuralDevelopment80GGA")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_DonationsForScientificResearchOrRuralDevelopment80GGA_ProofUpload = '" + Session["EmployeeNo"] + " - " + "DonationsForScientificResearchOrRuralDevelopment80GGA" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - DonationsForScientificResearchOrRuralDevelopment80GGA.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "DonationsForScientificResearchOrRuralDevelopment80GGA" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }

                    else if (OBJProofUpload.Origin == "DonationsForPoliticalParty80GGC")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_DonationsForPoliticalParty80GGC_ProofUpload = '" + Session["EmployeeNo"] + " - " + "DonationsForPoliticalParty80GGC" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - DonationsForPoliticalParty80GGC.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "DonationsForPoliticalParty80GGC" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }

                    /////---------------------------------------------------------------------------------------------------------------------------------------------------------------------

                    else if (OBJProofUpload.Origin == "PensionPlanfromInsuranceCompaniesMutualFunds")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_PensionPlanFromInsuranceCompaniesMutualFunds_ProofUpload = '" + Session["EmployeeNo"] + " - " + "PensionPlanfromInsuranceCompaniesMutualFunds" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - PensionPlanfromInsuranceCompaniesMutualFunds.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "PensionPlanfromInsuranceCompaniesMutualFunds" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }


                    else if (OBJProofUpload.Origin == "HouseLoanPrincipalRepayment")
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
                        string newFileName = Session["EmployeeNo"] + " - " + "HouseLoanPrincipalRepayment" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }



                    else if (OBJProofUpload.Origin == "LifeInsurancePremium")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_LifeInsurancePremium_ProofUpload = '" + Session["EmployeeNo"] + " - " + "LifeInsurancePremium" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - LifeInsurancePremium.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "LifeInsurancePremium" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }



                    else if (OBJProofUpload.Origin == "NationalSavingCertificate")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_NationalSavingCertificate_ProofUpload = '" + Session["EmployeeNo"] + " - " + "NationalSavingCertificate" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - NationalSavingCertificate.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "NationalSavingCertificate" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }


                    else if (OBJProofUpload.Origin == "NationalSavingCertificateAccuredIntrest")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_NationalSavingCertificateAccuredIntrest_ProofUpload = '" + Session["EmployeeNo"] + " - " + "NationalSavingCertificateAccuredIntrest" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - NationalSavingCertificateAccuredIntrest.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "NationalSavingCertificateAccuredIntrest" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }


                    else if (OBJProofUpload.Origin == "UnitLinkedInsurancePlan")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_UnitLinkedInsurancePlan_ProofUpload = '" + Session["EmployeeNo"] + " - " + "UnitLinkedInsurancePlan" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - UnitLinkedInsurancePlan.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "UnitLinkedInsurancePlan" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }


                    else if (OBJProofUpload.Origin == "ELSSTaxSavingMutualFund")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_ELSSTaxSavingMutualFund_ProofUpload = '" + Session["EmployeeNo"] + " - " + "ELSSTaxSavingMutualFund" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - ELSSTaxSavingMutualFund.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "ELSSTaxSavingMutualFund" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }



                    else if (OBJProofUpload.Origin == "TutionfeesFor2Children")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_TutionfeesFor2Children_ProofUpload = '" + Session["EmployeeNo"] + " - " + "TutionfeesFor2Children" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - TutionfeesFor2Children.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "TutionfeesFor2Children" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }



                    else if (OBJProofUpload.Origin == "TaxSavingSharesNABARDAndOtherbonds")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_TaxSavingSharesNABARDAndOtherbonds_ProofUpload = '" + Session["EmployeeNo"] + " - " + "TaxSavingSharesNABARDAndOtherbonds" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - TaxSavingSharesNABARDAndOtherbonds.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "TaxSavingSharesNABARDAndOtherbonds" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }



                    else if (OBJProofUpload.Origin == "TaxSavingFixDeposits")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_TaxSavingFixDeposits_ProofUpload = '" + Session["EmployeeNo"] + " - " + "TaxSavingFixDeposits" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - TaxSavingFixDeposits.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "TaxSavingFixDeposits" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }


                    else if (OBJProofUpload.Origin == "HousingStampDutyAndRegistration")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_HousingStampDutyAndRegistration_ProofUpload = '" + Session["EmployeeNo"] + " - " + "HousingStampDutyAndRegistration" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - HousingStampDutyAndRegistration.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "HousingStampDutyAndRegistration" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }

                    else if (OBJProofUpload.Origin == "PostOfficeTimeDepositScheme")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_PostOfficeTimeDepositScheme_ProofUpload = '" + Session["EmployeeNo"] + " - " + "PostOfficeTimeDepositScheme" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - PostOfficeTimeDepositScheme.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "PostOfficeTimeDepositScheme" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }

                    else if (OBJProofUpload.Origin == "SeniorCitizenSavingScheme")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_SeniorCitizenSavingScheme_ProofUpload = '" + Session["EmployeeNo"] + " - " + "SeniorCitizenSavingScheme" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - SeniorCitizenSavingScheme.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "SeniorCitizenSavingScheme" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }

                    else if (OBJProofUpload.Origin == "SukanyaSamriddhiScheme")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_SukanyaSamriddhiScheme_ProofUpload = '" + Session["EmployeeNo"] + " - " + "SukanyaSamriddhiScheme" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - SukanyaSamriddhiScheme.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "SukanyaSamriddhiScheme" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                    else if (OBJProofUpload.Origin == "OtherInvestment")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_OtherInvestment_ProofUpload = '" + Session["EmployeeNo"] + " - " + "OtherInvestment" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - OtherInvestment.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "OtherInvestment" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }


                    else if (OBJProofUpload.Origin == "IncomeFromLetOutHouseProperty")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET IncomeFromLetOutHouseProperty_ProofUpload = '" + Session["EmployeeNo"] + " - " + "IncomeFromLetOutHouseProperty" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - IncomeFromLetOutHouseProperty.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "IncomeFromLetOutHouseProperty" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                    else if (OBJProofUpload.Origin == "OtherIncome1")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET OtherIncome1_ProofUpload = '" + Session["EmployeeNo"] + " - " + "OtherIncome1" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - OtherIncome1.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "OtherIncome1" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                    else if (OBJProofUpload.Origin == "OtherIncome2")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET OtherIncome2_ProofUpload = '" + Session["EmployeeNo"] + " - " + "OtherIncome2" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - OtherIncome2.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "OtherIncome2" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }

                    else if (OBJProofUpload.Origin == "OtherIncome3")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET OtherIncome3_ProofUpload = '" + Session["EmployeeNo"] + " - " + "OtherIncome3" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - OtherIncome3.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "OtherIncome3" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }

                    else if (OBJProofUpload.Origin == "PublicProvidentFund")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_PublicProvidentFund_ProofUpload = '" + Session["EmployeeNo"] + " - " + "PublicProvidentFund" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - PublicProvidentFund.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "PublicProvidentFund" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }

                    else if (OBJProofUpload.Origin == "SelfOccupiedHouseLoanRepayments")
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
                            // **Delete only the file matching "{EmployeeNo} - HRA.*" pattern**
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
                        string newFileName = Session["EmployeeNo"] + " - " + "SelfOccupiedHouseLoanRepayments" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }

                    else if (OBJProofUpload.Origin == "PreviousEmployerDetails")
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
                            // **Delete only the file matching "{EmployeeNo} - HRA.*" pattern**
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

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }

                    else if (OBJProofUpload.Origin == "SuperannuationFundContribution")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_SuperannuationFundContribution_ProofUpload = '" + Session["EmployeeNo"] + " - " + "SuperannuationFundContribution" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - SuperannuationFundContribution.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "SuperannuationFundContribution" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }

                    else if (OBJProofUpload.Origin == "EmployeesNationalPensionScheme80CCD1")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_EmployeesNationalPensionScheme80CCD1_ProofUpload = '" + Session["EmployeeNo"] + " - " + "EmployeesNationalPensionScheme80CCD1" + Path.GetExtension(AttachFile.FileName) + "' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - EmployeesNationalPensionScheme80CCD1.*"; // Pattern to match (any extension)
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
                        string newFileName = Session["EmployeeNo"] + " - " + "EmployeesNationalPensionScheme80CCD1" + fileExtension; // Renaming file as "HRA" with original extension
                        string AttachFilePath = Path.Combine(FirstPath, newFileName); // Full Path with new name

                        // Save the file with the new name
                        AttachFile.SaveAs(AttachFilePath);

                        TempData["Message"] = "Record Updated Successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }

                }

                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region Delete
        public ActionResult Delete(string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                if (Origin != null)
                {
                    if (Origin == "HRA")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET HRA_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - HRA.*"; // Pattern to match (any extension)
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
                                //return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }

                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }
                    else if (Origin == "Mediclaim80D")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_Mediclaim80D_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - Mediclaim80D.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }

                        // Extract file extension
                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }
                    else if (Origin == "PreventiveHealthCheckUp")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_PreventiveHealthCheckUp_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - PreventiveHealthCheckUp.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }

                    

                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }

                    else if (Origin == "HandicappedDependent80DD")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_HandicappedDependent80DD_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - HandicappedDependent80DD.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }

                      
                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }
                    else if (Origin == "TreatmentOfSpecifiedDisease")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_TreatmentOfSpecifiedDisease80DDB_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - TreatmentOfSpecifiedDisease80DDB.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }

                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }
                    else if (Origin == "IntrestOnEducationalLoan80E")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_IntrestOnEducationalLoan80E_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - IntrestOnEducationalLoan80E.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }

                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }
                    else if (Origin == "PhysicallyHandicapped80U")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_PhysicallyHandicapped80U_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - PhysicallyHandicapped80U.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }

                    else if (Origin == "AdditionalContributionNPS80CCD1B")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_AdditionalContributiontoNPS80CCD1B_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - AdditionalContributionNPS80CCD1B.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }

                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }

                    else if (Origin == "IntrestOnDepositinsavingAccount80TTA")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_IntrestOnDepositInSavingAccount80TTA_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - IntrestOnDepositinsavingAccount80TTA.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }

                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }
                    else if (Origin == "IntrestOnDepositinsavingAccount80TTBSeniorCitizens")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_IntrestOnDepositInSavingAccount80TTBSeniorCitizens_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - IntrestOnDepositinsavingAccount80TTBSeniorCitizens.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }

                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }

                    else if (Origin == "PurchaseofElectricVehicle80EEB")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_IntrestOnLoanForPurchaseOfElectricVehicle80EEB_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - IntrestOnLoanForPurchaseOfElectricVehicle80EEB.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }

                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }
                    else if (Origin == "FundsEligibleFor100PerDeductionWithoutLimit80G")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_FundsEligibleFor100PerDeductionWithoutLimit80G_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - FundsEligibleFor100PerDeductionWithoutLimit80G.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }

                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }
                    else if (Origin == "FundsEligibleFor50PerDeductionWithoutLimit80G")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_FundsEligibleFor50PerDeductionWithoutLimit80G_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - FundsEligibleFor50PerDeductionWithoutLimit80G.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }

                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }
                    else if (Origin == "FundsEligibleFor50PerDeductionSubjectMaximumLimit80G")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_FundsEligibleFor50PerDeductionSubjectMaximumLimit80G_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - FundsEligibleFor50PerDeductionSubjectMaximumLimit80G.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }

                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }

                    else if (Origin == "DonationsForScientificResearchOrRuralDevelopment80GGA")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_DonationsForScientificResearchOrRuralDevelopment80GGA_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - DonationsForScientificResearchOrRuralDevelopment80GGA.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }

                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }

                    else if (Origin == "DonationsForPoliticalParty80GGC")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET ChapterVIA_DonationsForPoliticalParty80GGC_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - DonationsForPoliticalParty80GGC.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }

                      
                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }





                    else if (Origin == "PensionPlanfromInsuranceCompaniesMutualFunds")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_PensionPlanFromInsuranceCompaniesMutualFunds_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - PensionPlanfromInsuranceCompaniesMutualFunds.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }



                    //////--------------------------------------------------------------------------------
                    else if (Origin == "HouseLoanPrincipalRepayment")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_HouseLoanPrincipalRepayment_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }

                    else if (Origin == "LifeInsurancePremium")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_LifeInsurancePremium_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - LifeInsurancePremium.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }


                    else if (Origin == "NationalSavingCertificate")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_NationalSavingCertificate_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - NationalSavingCertificate.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }

                    else if (Origin == "NationalSavingCertificateAccuredIntrest")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_NationalSavingCertificateAccuredIntrest_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - NationalSavingCertificateAccuredIntrest.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }

                    else if (Origin == "UnitLinkedInsurancePlan")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_UnitLinkedInsurancePlan_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - UnitLinkedInsurancePlan.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }

                    else if (Origin == "ELSSTaxSavingMutualFund")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_ELSSTaxSavingMutualFund_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - ELSSTaxSavingMutualFund.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }


                    else if (Origin == "TutionfeesFor2Children")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_TutionfeesFor2Children_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - TutionfeesFor2Children.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }

                    else if (Origin == "TaxSavingSharesNABARDAndOtherbonds")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_TaxSavingSharesNABARDAndOtherbonds_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - TaxSavingSharesNABARDAndOtherbonds.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }

                    else if (Origin == "TaxSavingFixDeposits")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_TaxSavingFixDeposits_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - TaxSavingFixDeposits.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }

                    else if (Origin == "HousingStampDutyAndRegistration")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_HousingStampDutyAndRegistration_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - HousingStampDutyAndRegistration.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }


                    else if (Origin == "PostOfficeTimeDepositScheme")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_PostOfficeTimeDepositScheme_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - PostOfficeTimeDepositScheme.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }

                    else if (Origin == "SeniorCitizenSavingScheme")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_SeniorCitizenSavingScheme_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - SeniorCitizenSavingScheme.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }

                    else if (Origin == "SukanyaSamriddhiScheme")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_SukanyaSamriddhiScheme_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - SukanyaSamriddhiScheme.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }

                    else if (Origin == "OtherInvestment")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_OtherInvestment_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - OtherInvestment.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }

                    else if (Origin == "IncomeFromLetOutHouseProperty")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET IncomeFromLetOutHouseProperty_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - IncomeFromLetOutHouseProperty.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }


                    else if (Origin == "OtherIncome1")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET OtherIncome1_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - OtherIncome1.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }
                    else if (Origin == "OtherIncome2")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET OtherIncome2_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - OtherIncome2.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }

                    else if (Origin == "OtherIncome3")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET OtherIncome3_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - OtherIncome3.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }
                    else if (Origin == "PublicProvidentFund")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_PublicProvidentFund_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - PublicProvidentFund.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }

                    else if (Origin == "PreviousEmployerDetails")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET Previous_Er_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }

                    else if (Origin == "SelfOccupiedHouseLoanRepayments")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET SelfOccupied_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }

                    else if (Origin == "EmployeesNationalPensionScheme80CCD1")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_EmployeesNationalPensionScheme80CCD1_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - EmployeesNationalPensionScheme80CCD1.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }


                    else if (Origin == "SuperannuationFundContribution")
                    {
                        DapperORM.DynamicQuerySingle("UPDATE IncomeTax_InvestmentDeclaration SET US80C_SuperannuationFundContribution_ProofUpload = '' WHERE InvestmentDeclarationEmployeeId = " + Session["EmployeeId"] + " AND InvestmentDeclarationFyearId = " + Session["TaxFyearId"]);

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
                                string searchPattern = Session["EmployeeNo"] + " - SuperannuationFundContribution.*"; // Pattern to match (any extension)
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
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //  return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                            catch (UnauthorizedAccessException unAuthEx)
                            {
                                TempData["Message"] = "Permission error: " + unAuthEx.Message;
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                                //return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                            }
                        }


                        TempData["Message"] = "Record Deleted Successfully";
                        TempData["Icon"] = "success";
                        return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                    }

                }

                return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Download Attachment
        public ActionResult DownloadAttachment(string DownloadAttachment)
        {
            try
            {
                if(DownloadAttachment!= "File Not Found")
                {
                    if (string.IsNullOrEmpty(DownloadAttachment))
                    {
                        return Json(new { Success = false, Message = "Invalid File.", Icon = "error" }, JsonRequestBehavior.AllowGet);
                    }

                    var fullPath = DownloadAttachment;
                    if (!System.IO.File.Exists(fullPath))
                    {
                        return Json(new { Success = false, Message = "File not found on your server.", Icon = "error" }, JsonRequestBehavior.AllowGet);
                    }

                    var driveLetter = Path.GetPathRoot(DownloadAttachment);
                    if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
                    {
                        return Json(new { Success = false, Message = $"Drive {driveLetter} does not exist.", Icon = "error" }, JsonRequestBehavior.AllowGet);
                    }

                    var fileName = Path.GetFileName(fullPath);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
                    var fileBase64 = Convert.ToBase64String(fileBytes);

                    return Json(new
                    {
                        Success = true,
                        FileName = fileName,
                        FileData = fileBase64,
                        ContentType = MediaTypeNames.Application.Octet
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    TempData["Message"] = "Upload a file";
                    TempData["Icon"] = "error";
                    //return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                }
               
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = $"Internal server error: {ex.Message}", Icon = "error" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

    }
}