using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Tax;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
namespace KompassHR.Areas.ESS.Controllers.ESS_Tax
{
    public class ESS_Tax_ChallanAndReturn_MonthlyChallanController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region MainView
        public ActionResult ESS_Tax_ChallanAndReturn_MonthlyChallan()
        {
            try
            {

                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 639;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";

                var GetChallanNo = "Select Isnull(Max(ChallanNo),0)+1 As ChallanNo from IncomeTax_Challan";
                var ChallanNo = DapperORM.DynamicQuerySingle(GetChallanNo);
                ViewBag.ChallanNo = ChallanNo;

                Session["MonthYear"] = null;
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "Select TaxFyearId as Id,TaxYear As Name  from IncomeTax_Fyear where Deactivate=0 and [IsActive]=1");
                ViewBag.GetTaxFyear = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param).ToList();

                DynamicParameters paramTAN = new DynamicParameters();
                paramTAN.Add("@query", "Select DeductorResponsibleId as Id,DeductorTAN As Name  from IncomeTax_DeductorResponsible where Deactivate=0 ");
                ViewBag.GetTAN = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", paramTAN).ToList();


                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion MainView

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(IncomeTax_Challan Challan)
        {
            try
            {
                var GetTAN = "Select DeductorTAN from IncomeTax_DeductorResponsible where DeductorResponsibleId=" + Challan.ChallanTANId + "";
                var TAN = DapperORM.DynamicQuerySingle(GetTAN);

                param.Add("@p_process", string.IsNullOrEmpty(Challan.ChallanId_Encrypted) ? "Save" : "Update");
                param.Add("@P_ChallanId_Encrypted", Challan.ChallanId_Encrypted);
                param.Add("@p_CmpId", Challan.CmpId);

                param.Add("@p_DocDate", Challan.DocDate);
                param.Add("@p_ChallanNo", Challan.ChallanNo);
                param.Add("@p_TAN", TAN.DeductorTAN);

                param.Add("@p_ChallanTANId", Challan.ChallanTANId);
                param.Add("@p_ChallanFyearId", Challan.ChallanFyearId);
                param.Add("@p_Quarter", Challan.Quarter);

                param.Add("@p_QuarterMonth", Challan.QuarterMonth);
                param.Add("@p_ChallanBranchId", Challan.ChallanBranchId);
                param.Add("@p_ChallanEmployeeId", Challan.ChallanEmployeeId);

                param.Add("@p_TDSAmount", Challan.TDSAmount);
                param.Add("@p_SurchargeAmount", Challan.SurchargeAmount);
                param.Add("@p_EducationAmount", Challan.EducationAmount);

                param.Add("@p_InterstAmount", Challan.InterstAmount);
                param.Add("@p_FeeAmount", Challan.FeeAmount);
                param.Add("@p_PenaltyOtherAmount", Challan.PenaltyOtherAmount);

                param.Add("@p_TotalTaxDepositedAmount", Challan.TotalTaxDepositedAmount);
                param.Add("@p_BSR", Challan.BSR);
                param.Add("@p_ChallanSerialNo", Challan.ChallanSerialNo);

                param.Add("@p_TaxDepositeDate", Challan.TaxDepositeDate);
                param.Add("@p_MinorHeadOfChallan", Challan.MinorHeadOfChallan);
                param.Add("@p_InterestToBeAllocatedApportioned", Challan.InterestToBeAllocatedApportioned);

                param.Add("@p_OtherAmount", Challan.OtherAmount);
                param.Add("@p_NILLChallanIndicator", Challan.NILLChallanIndicator);
                param.Add("@p_Remark", Challan.Remark);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());

                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_Challan", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_Tax_ChallanAndReturn_MonthlyChallan", "ESS_Tax_ChallanAndReturn_MonthlyChallan");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsChallanExists
        public ActionResult IsChallanExists(string ChallanId_Encrypted, int ChallanFyearId, string Quarter, DateTime QuarterMonth)
        {
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        #endregion IsChallanExists

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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 235;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_ChallanId_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_IncomeTax_Challan", param);
                ViewBag.GetReplacementInputList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion GetList

        #region AddDeductee
        public ActionResult AddDeductee(int challanId, int tanId, DateTime QuarterMonth)
        {
            try
            {
                var GetChallanDetails = "Select * from IncomeTax_Challan where ChallanId=" + challanId + "  ";
                var ChallanDetails = DapperORM.DynamicQuerySingle(GetChallanDetails);
                ViewBag.ChallanDetails = ChallanDetails;

                //var branchIds = DapperORM.DynamicQueryList("SELECT TANBuMappingBranchId FROM IncomeTax_TANBuMapping WHERE TANId = "+ tanId + " and Deactivate=0").ToList();
                //var branchId = string.Join(",", branchIds);
                Session["challanId"] = challanId;
                //param.Add("@p_BranchId", branchId);
                //param.Add("@p_MonthYear", QuarterMonth);
                param.Add("@p_ChallanId", challanId);
                var data = DapperORM.DynamicList("sp_List_IncomeTax_Challan_Deductee", param);
                ViewBag.EmployeeDetails = data;

                IncomeTax_Challan_Deductee deductee = new IncomeTax_Challan_Deductee();
                deductee.IncomeTax_Challan_ChallanId = challanId;
                deductee.SrNoInChallanAsPerRunningSerialNo = Convert.ToInt32(ChallanDetails.ChallanNo);

                return View(deductee);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region SaveChallanDeductee

        [HttpPost]
        //public ActionResult SaveChallanDeductee(List<IncomeTax_Challan_Deductee> Deductee)
        //{
        //    try
        //    {
        //        for (int i = 0; i < Deductee.Count; i++)
        //        {
        //            DynamicParameters param = new DynamicParameters();
        //            var GetEmployeeSerialNo = "Select Isnull(Max(EmployeeSerialNo),0)+1 As EmployeeSerialNo from IncomeTax_Challan_Deductee";
        //            var EmployeeSerialNo = DapperORM.DynamicQuerySingle(GetEmployeeSerialNo);

        //            var GetChallanDetails = "Select * from IncomeTax_Challan where ChallanId=" + Deductee[i].IncomeTax_Challan_ChallanId + "";
        //            var ChallanDetails = DapperORM.DynamicQuerySingle(GetChallanDetails);

        //            DateTime quarterMonth = ChallanDetails.QuarterMonth;
        //            DateTime lastDateOfMonth = new DateTime(quarterMonth.Year, quarterMonth.Month, DateTime.DaysInMonth(quarterMonth.Year, quarterMonth.Month));

        //            var GetEmployeeDetails = "Select * from Mas_Employee where EmployeeId=" + Deductee[i].EmployeeID + "";
        //            var EmployeeDetails = DapperORM.DynamicQuerySingle(GetEmployeeDetails);




        //            param.Add("@p_process", Convert.ToString(Deductee[i].ChallanDeducteeID_Encrypted) == null ? "Save" : "Update");
        //         //   param.Add("@p_process",  "Save" );
        //            param.Add("@P_IncomeTax_Challan_ChallanId", Deductee[i].IncomeTax_Challan_ChallanId);
        //            param.Add("@P_ChallanDeducteeBranchId", Deductee[i].ChallanDeducteeBranchId);

        //            param.Add("@P_EmployeeID", Deductee[i].EmployeeID);
        //            param.Add("@P_EmployeeSerialNo", EmployeeSerialNo.EmployeeSerialNo);
        //            param.Add("@P_SrNoInChallanAsPerRunningSerialNo", Deductee[i].SrNoInChallanAsPerRunningSerialNo);

        //            param.Add("@P_EmployeeReferenceNumberProvidedByEmployer", Deductee[i].EmployeeReferenceNumberProvidedByEmployer);
        //            param.Add("@P_Mode", Deductee[i].Mode);
        //            param.Add("@P_PANOfTheEmployee", Deductee[i].PANOfTheEmployee);

        //            param.Add("@P_NameOfTheEmployee", EmployeeDetails.EmployeeName);
        //            param.Add("@P_SectionCodeAnnexure2", "92B");
        //            param.Add("@P_DateOfPaymentCredit", lastDateOfMonth);

        //            param.Add("@P_DateOfDeduction", lastDateOfMonth);
        //            param.Add("@P_AmountPaidCredited", Deductee[i].AmountPaidCredited);
        //            param.Add("@P_TDSAmount", Deductee[i].TDSAmount);

        //            param.Add("@P_SurchargeAmount", Deductee[i].SurchargeAmount);
        //            param.Add("@P_EducationCessAmount", Deductee[i].EducationCessAmount);
        //            param.Add("@P_TotalTDSAmount", Deductee[i].TotalTDSAmount);

        //            param.Add("@P_TotalTaxDepositedAmount", Deductee[i].TotalTDSAmount);
        //            param.Add("@P_DateOfDeposite", ChallanDetails.TaxDepositeDate);
        //            param.Add("@P_ReasonForNonDeductionLowerDeductionIfAny", Deductee[i].ReasonForNonDeductionLowerDeductionIfAny);

        //            param.Add("@P_NumberOfCertificate", Deductee[i].NumberOfCertificate);
        //            param.Add("@P_ErrorDescription", Deductee[i].ErrorDescription);


        //            param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
        //            param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
        //            var data = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_Challan_Deductee", param);
        //            TempData["Message"] = param.Get<string>("@p_msg");
        //            TempData["Icon"] = param.Get<string>("@p_Icon");
        //        }



        //        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"], Month = TempData["Month"] }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
        //}



        public ActionResult SaveChallanDeductee(List<IncomeTax_Challan_Deductee> Deductee)
        {
            try
            {
                using (var connection = sqlcon)
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var challanId = Deductee?.FirstOrDefault()?.IncomeTax_Challan_ChallanId ?? 0;
                            if (challanId == 0)
                                throw new Exception("Challan ID is missing.");

                            // Get next EmployeeSerialNo once
                            string getSerialSql = "SELECT ISNULL(MAX(EmployeeSerialNo), 0) FROM IncomeTax_Challan_Deductee";
                            int nextSerialNo = connection.QuerySingle<int>(getSerialSql, transaction: transaction) + 1;

                            // Get challan details once
                            string getChallanSql = "SELECT * FROM IncomeTax_Challan WHERE ChallanId = @ChallanId";
                            var challanDetails = connection.QuerySingle(getChallanSql, new { ChallanId = challanId }, transaction: transaction);

                            DateTime quarterMonth = challanDetails.QuarterMonth;
                            DateTime lastDateOfMonth = new DateTime(quarterMonth.Year, quarterMonth.Month, DateTime.DaysInMonth(quarterMonth.Year, quarterMonth.Month));

                            // Cache employee details
                            Dictionary<int, dynamic> employeeCache = new Dictionary<int, dynamic>();

                            foreach (var deductee in Deductee)
                            {
                                if (deductee.IsActive)
                                {
                                    // Get employee details from cache or DB
                                    if (!employeeCache.ContainsKey(deductee.EmployeeID))
                                    {
                                        string getEmpSql = "SELECT * FROM Mas_Employee WHERE EmployeeId = @EmployeeId";
                                        var emp = connection.QuerySingle(getEmpSql, new { EmployeeId = deductee.EmployeeID }, transaction: transaction);
                                        employeeCache[deductee.EmployeeID] = emp;
                                    }
                                    var employeeDetails = employeeCache[deductee.EmployeeID];

                                    var param = new DynamicParameters();

                                    string processType = string.IsNullOrEmpty(Convert.ToString(deductee.ChallanDeducteeID_Encrypted)) ? "Save" : "Update";
                                    param.Add("@p_process", processType);
                                    param.Add("@P_IncomeTax_Challan_ChallanId", deductee.IncomeTax_Challan_ChallanId);
                                    param.Add("@P_ChallanDeducteeBranchId", deductee.ChallanDeducteeBranchId);
                                    param.Add("@P_EmployeeID", deductee.EmployeeID);
                                    param.Add("@P_EmployeeSerialNo", nextSerialNo++);
                                    param.Add("@P_SrNoInChallanAsPerRunningSerialNo", deductee.SrNoInChallanAsPerRunningSerialNo);
                                    param.Add("@P_EmployeeReferenceNumberProvidedByEmployer", deductee.EmployeeReferenceNumberProvidedByEmployer);
                                    param.Add("@P_Mode", deductee.Mode);
                                    param.Add("@P_PANOfTheEmployee", deductee.PANOfTheEmployee);
                                    param.Add("@P_NameOfTheEmployee", employeeDetails.EmployeeName);
                                    param.Add("@P_SectionCodeAnnexure2", "92B");
                                    param.Add("@P_DateOfPaymentCredit", lastDateOfMonth);
                                    param.Add("@P_DateOfDeduction", lastDateOfMonth);
                                    param.Add("@P_AmountPaidCredited", deductee.AmountPaidCredited);
                                    param.Add("@P_TDSAmount", deductee.TDSAmount);
                                    param.Add("@P_SurchargeAmount", deductee.SurchargeAmount);
                                    param.Add("@P_EducationCessAmount", deductee.EducationCessAmount);
                                    param.Add("@P_TotalTDSAmount", deductee.TotalTDSAmount);
                                    param.Add("@P_TotalTaxDepositedAmount", deductee.TotalTDSAmount);
                                    param.Add("@P_DateOfDeposite", challanDetails.TaxDepositeDate);
                                    param.Add("@P_ReasonForNonDeductionLowerDeductionIfAny", deductee.ReasonForNonDeductionLowerDeductionIfAny);
                                    param.Add("@P_NumberOfCertificate", deductee.NumberOfCertificate);
                                    param.Add("@P_ErrorDescription", deductee.ErrorDescription);

                                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                                    // Use transaction-aware execution
                                    connection.Execute("sp_SUD_IncomeTax_Challan_Deductee", param, transaction, commandType: CommandType.StoredProcedure);

                                    TempData["Message"] = param.Get<string>("@p_msg");
                                    TempData["Icon"] = param.Get<string>("@p_Icon");
                                }
                                else if (!string.IsNullOrEmpty(deductee.ChallanDeducteeID_Encrypted))
                                {
                                    string deactivateSql = @"
                                UPDATE IncomeTax_Challan_Deductee 
                                SET Deactivate = 1 
                                WHERE ChallanDeducteeID_Encrypted = @ChallanDeducteeID_Encrypted 
                                  AND IncomeTax_Challan_ChallanId = @ChallanId";
                                    connection.Execute(deactivateSql, new
                                    {
                                        deductee.ChallanDeducteeID_Encrypted,
                                        ChallanId = challanId
                                    }, transaction);
                                }
                            }

                            transaction.Commit();
                            return Json(new
                            {
                                Message = TempData["Message"],
                                Icon = TempData["Icon"],
                                Month = TempData["Month"]
                            }, JsonRequestBehavior.AllowGet);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            Session["GetErrorMessage"] = ex.Message;
                            return RedirectToAction("ErrorPage", "Login");
                        }
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

        #region Delete
        public ActionResult Delete(string ChallanDeducteeID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "ESS" });
                }
                param.Add("@p_process", "Deactivate");
                param.Add("@p_ChallanDeducteeID_Encrypted", ChallanDeducteeID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                var Result = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_Challan_Deductee", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("AddDeductee", "ESS_Tax_ChallanAndReturn_MonthlyChallan");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ChallanDocument
        public ActionResult ChallanFileUpload(int? challanId, string ChallanId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 235;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Session["challanId"] = challanId;
                Session["ChallanId_Encrypted"] = ChallanId_Encrypted;
                param.Add("@p_ChallanDocument_EncryptedId", "List");
                param.Add("@p_ChallanId", Session["challanId"]);
                var data = DapperORM.ReturnList<dynamic>("sp_List_IncomeTax_Challan_Document", param).ToList();
                ViewBag.GetListChallanDocs = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


        public ActionResult SaveUpdateUploadFile(IncomeTax_Challan_Document Document, HttpPostedFileBase DocumentPath)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(Document.ChallanDocument_EncryptedId) ? "Save" : "Update");
                param.Add("@p_ChallanDocumentId", Document.ChallanDocumentId);
                param.Add("@p_ChallanDocument_EncryptedId", Document.ChallanDocument_EncryptedId);
                param.Add("@p_IncomeTax_Challan_ChallanId", Session["challanId"]);
                param.Add("@p_Title", Document.Title);
                param.Add("@p_FileType", Document.FileType);

                if (DocumentPath != null)
                    param.Add("@p_DocumentPath", DocumentPath.FileName);// Claim_GeneralClaim.AttachmentPath);
                else
                    param.Add("@p_DocumentPath", "");
                param.Add("@p_CreatedUpdateBy", Convert.ToInt32(Session["EmployeeId"]));
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var data = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_Challan_Document", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                var P_Id = param.Get<string>("@p_Id");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                TempData["P_Id"] = param.Get<string>("@p_Id");

                if (TempData["P_Id"] != null)
                {
                    //var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='IncomeTax_Challan'");
                    //   var GetFirstPath = GetDocPath.DocInitialPath;
                    //   var FirstPath = GetFirstPath + TempData["P_Id"] + "\\";// First path plus concat folder by Id

                    //var driveLetter = Path.GetPathRoot(FirstPath);
                    //// Check if the drive exists
                    //if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
                    //{
                    //    TempData["Message"] = $"Drive {driveLetter} does not exist.";
                    //    TempData["Icon"] = "error";
                    //    return RedirectToAction("ChallanFileUpload");
                    //}
                    // Check if the file exists
                    //if (!System.IO.File.Exists(FirstPath))
                    //{
                    //    TempData["Message"] = "File not found on the server.";
                    //    TempData["Icon"] = "error";
                    //    return RedirectToAction("ChallanFileUpload");
                    //}
                    //if (!Directory.Exists(FirstPath))
                    //{
                    //    Directory.CreateDirectory(FirstPath);
                    //}

                    //if (DocumentPath != null)
                    //{
                    //    string FilePath = "";
                    //    FilePath = FirstPath + DocumentPath.FileName; //Concat Full Path and create New full Path
                    //    DocumentPath.SaveAs(FilePath); // This is use for Save image in folder full path
                    //}


                    if (DocumentPath != null)
                    {
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='IncomeTax_Challan'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        var FirstPath = GetFirstPath + Session["challanId"] + "\\"; // First path plus concat folder 
                        string directoryPath = Path.GetDirectoryName(FirstPath);
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(FirstPath);
                        }
                        string AttachFilePath = "";
                        //AttachFilePath = FirstPath + DocumentPath.FileName; //Concat Full Path and create New full Path
                        AttachFilePath = FirstPath + (Convert.ToInt32(TempData["P_Id"]) + "_" + DocumentPath.FileName);
                        DocumentPath.SaveAs(AttachFilePath); // This is use for Save image in folder full path
                    }
                }
                return RedirectToAction("ChallanFileUpload", "ESS_Tax_ChallanAndReturn_MonthlyChallan", new { challanId = Session["challanId"], ChallanId_Encrypted = Session["ChallanId_Encrypted"] });

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region DeleteDocument
        public ActionResult DeleteDocument(string ChallanDocument_EncryptedId, string FileName, int IncomeTax_Challan_ChallanId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                param.Add("@p_process", "Delete");
                param.Add("@p_ChallanDocument_EncryptedId", ChallanDocument_EncryptedId);
                param.Add("@p_CreatedUpdateBy", Convert.ToInt32(Session["EmployeeId"]));
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_Challan_Document", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                var DocPKId = param.Get<string>("@p_Id");

                var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='IncomeTax_Challan'");
                var GetFirstPath = GetDocPath.DocInitialPath + "\\" + IncomeTax_Challan_ChallanId + "\\";


                return RedirectToAction("ChallanFileUpload", "ESS_Tax_ChallanAndReturn_MonthlyChallan", new { ChallanId_Encrypted = Session["ChallanId_Encrypted"], challanId = Session["challanId"] });
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
                if (DownloadAttachment != "File Not Found")
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
                    return RedirectToAction("ChallanFileUpload", "ESS_Tax_ChallanAndReturn_MonthlyChallan", new { ChallanId_Encrypted = Session["ChallanId_Encrypted"], challanId = Session["challanId"] });
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