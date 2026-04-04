using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Module.Models.Module_TaxDeclaration;
using KompassHR.Areas.ESS.Models.ESS_Tax;
using System.Net;
using System.Data;

using System.IO;
using System.Net.Mime;
using Newtonsoft.Json;
using System.Text;

namespace KompassHR.Areas.ESS.Controllers.ESS_Tax
{
    public class Ess_Tax_DeclarationApproval_OldRegimeController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: ESS/Ess_Tax_DeclarationApproval_OldRegime
        #region Ess_Tax_DeclarationApproval_OldRegime
        public ActionResult Ess_Tax_DeclarationApproval_OldRegime(string TaxFyearId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 531;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "Select TaxFyearId as Id,TaxYear As Name  from IncomeTax_Fyear where Deactivate=0 and [IsActive]=1");
                var GetTaxFyear = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param).ToList();
                ViewBag.GetTaxFyear = GetTaxFyear;


                param = new DynamicParameters();
                param.Add("@P_FYearID", TaxFyearId);
                param.Add("@P_EmployeeID", Session["EmployeeId"]);
                var data = DapperORM.ExecuteSP<IncomeTax_InvestmentDeclaration>("sp_Tax_Declaration_List_AR_OldRegime", param).ToList();
                ViewBag.OldRegimeList = data;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion Ess_Tax_DeclarationApproval_OldRegime

        #region OldRegimeDetails

        public ActionResult OldRegimeDetails(int? InvestmentDeclarationId, int? EmployeeId, int? InvestmentDeclarationFyearId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                Session["InvestmentDeclarationId"] = InvestmentDeclarationId;

                #region InvestmentData

                param = new DynamicParameters();
                param.Add("@query", "Select * from IncomeTax_InvestmentDeclaration where Deactivate=0 and InvestmentDeclarationId= " + InvestmentDeclarationId + " ");
                var InvestmentDeclaration = DapperORM.ReturnList<InvestmentDeclaration_Approval>("sp_QueryExcution", param).FirstOrDefault();

                //Session["EmployeeId"] = InvestmentDeclaration.InvestmentDeclarationEmployeeId;
                //Session["TaxFyearId"] = InvestmentDeclaration.InvestmentDeclarationFyearId;
                #endregion InvestmentData

                #region GetEmployee
                var GetEmployee = "Select * from View_IncomeTax_Employee Where EmployeeID = " + InvestmentDeclaration.InvestmentDeclarationEmployeeId + "";
                var Employee = DapperORM.DynamicQuerySingle(GetEmployee);
                if (Employee != null)
                {
                    ViewBag.GetEmployee = Employee;
                }
                #endregion

                #region HRA
                var Date = DapperORM.DynamicQuerySingle($@"Select  HRA_SubmitDate, HRA_SubmitCount from IncomeTax_InvestmentDeclaration where Deactivate = 0  and InvestmentDeclarationFyearId = {InvestmentDeclaration.InvestmentDeclarationFyearId} and InvestmentDeclarationEmployeeId = {InvestmentDeclaration.InvestmentDeclarationEmployeeId}");
                TempData["HRA_SubmitDate"] = null;
                TempData["HRA_SubmitCount"] = null;
                if (Date != null)
                {
                    TempData["HRA_SubmitDate"] = Date.HRA_SubmitDate;
                    TempData["HRA_SubmitCount"] = Date.HRA_SubmitCount;
                }

                var PanLimit = DapperORM.DynamicQuerySingle("Select HRA_PanLimit From IncomeTax_Rule where  Deactivate=0 and IncomeTaxRule_FyearId= " + InvestmentDeclaration.InvestmentDeclarationFyearId + "");
                if (PanLimit != null)
                {
                    TempData["HRA_PanLimit"] = PanLimit.HRA_PanLimit;
                }

                #endregion HRA

                #region 80C


                //  SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
                var Amount80C = DapperORM.DynamicQuerySingle("Select [TaxRule80C_Amount] As Limit80C From IncomeTax_Rule where  Deactivate=0 and IncomeTaxRule_FyearId= " + InvestmentDeclaration.InvestmentDeclarationFyearId + " ");
                // var Amount80C = DapperORM.DynamicQuerySingle("Select [TaxRule80C_Amount] As Limit80C From IncomeTax_Rule where  Deactivate=0 and Year(FromDate)=year(GetDate())");
                if (Amount80C != null)
                {
                    TempData["80C_Amount"] = Amount80C.Limit80C;
                }
                TempData["US80C_SubmitDate"] = null;
                TempData["US80C_SubmitCount"] = null;
                if (InvestmentDeclaration != null)
                {
                    TempData["US80C_SubmitDate"] = InvestmentDeclaration.US80C_SubmitDate;
                    TempData["US80C_SubmitCount"] = InvestmentDeclaration.US80C_SubmitCount;
                }
                #endregion 80C

                #region SelfOccupiedHouseLoan
                var GetTaxRuleLimit = DapperORM.DynamicQuerySingle("Select TaxRule80EE_Limit,TaxRule80EEA_Limit,Housingloan_After_1999,Housingloan_Before_1999,Housingloan_RepairOrRenewal from IncomeTax_Rule where deactivate=0 and  IncomeTaxRule_FyearId= " + InvestmentDeclaration.InvestmentDeclarationFyearId + " ");
                if (GetTaxRuleLimit != null)
                {
                    ViewBag.GetTaxRuleLimit = GetTaxRuleLimit;
                }

                #endregion SelfOccupiedHouseLoan

                #region  LetOutHouseProperty
                var StandardDeductionNetAnnualValue = DapperORM.DynamicQuerySingle("Select IncomefromLetOutHouseProperty_StandardEducation  From IncomeTax_Rule where  Deactivate=0 and year (FromDate)=year(GETDATE())");
                if (StandardDeductionNetAnnualValue != null)
                {
                    TempData["IncomefromLetOutHouseProperty_StandardEducation"] = StandardDeductionNetAnnualValue.IncomefromLetOutHouseProperty_StandardEducation;
                }
                else
                {
                    TempData["IncomefromLetOutHouseProperty_StandardEducation"] = "";
                }

                var Date2 = DapperORM.DynamicQuerySingle(" Select IncomeFromLetOutHouseProperty_SubmitDate, IncomeFromLetOutHouseProperty_SubmitCount from IncomeTax_InvestmentDeclaration where Deactivate = 0  and [InvestmentDeclarationId] = " + InvestmentDeclarationId + " ");
                TempData["IncomeFromLetOutHouseProperty_SubmitDate"] = null;
                TempData["IncomeFromLetOutHouseProperty_SubmitCount"] = null;
                if (Date != null)
                {
                    TempData["IncomeFromLetOutHouseProperty_SubmitDate"] = Date2.IncomeFromLetOutHouseProperty_SubmitDate;
                    TempData["IncomeFromLetOutHouseProperty_SubmitCount"] = Date2.IncomeFromLetOutHouseProperty_SubmitCount;
                }

                var GetTotal = "Select TotalIncomeFromLetOutHouseProperty from IncomeTax_InvestmentDeclaration Where InvestmentDeclarationId= " + InvestmentDeclarationId + "";
                var SetTotal = DapperORM.DynamicQuerySingle(GetTotal);
                if (SetTotal != null)
                {
                    TempData["TotalIncomeFromLetOutHouseProperty"] = SetTotal.TotalIncomeFromLetOutHouseProperty;
                }
                param = new DynamicParameters();
                param.Add("@p_IncomeFromLetOutHousePropertyFyearId", InvestmentDeclaration.InvestmentDeclarationFyearId);
                param.Add("@p_IncomeFromLetOutHousePropertyEmployeeId", InvestmentDeclaration.InvestmentDeclarationEmployeeId);
                var LetOutHouseProperty = DapperORM.ReturnList<IncomeTax_InvestmentDeclaration_LetOutHouseProperty>("sp_List_IncomeTax_InvestmentDeclaration_LetOutHouseProperty", param).ToList();
                ViewBag.LetOutHousePropertybag = LetOutHouseProperty;

                #endregion LetOutHouseProperty

                #region Chapter VI-A

                // var GetEmpoyee = "Select * from View_IncomeTax_Employee Where EmployeeID = " + Session["EmployeeId"] + "";

                var IncomeTaxEmployee = DapperORM.DynamicQuerySingle("Select  FLOOR((CAST (Convert(Datetime, (Select ToDate from IncomeTax_Fyear where TaxFyearId= " + InvestmentDeclaration.InvestmentDeclarationFyearId + " and Deactivate=0)) AS INTEGER) - CAST(Convert(Datetime, BirthdayDate) AS INTEGER)) / 365.25) as Age from Mas_Employee_Personal   where PersonalEmployeeId=" + InvestmentDeclaration.InvestmentDeclarationEmployeeId + " and Deactivate=0").FirstOrDefault();
                //var IncomeTaxEmployee = DapperORM.DynamicQuerySingle(GetEmpoyee);
                if (IncomeTaxEmployee != null)
                {
                    TempData["Age"] = IncomeTaxEmployee.Age;
                }
                else
                {
                    TempData["Age"] = "";
                }


                var Get80DRules = DapperORM.DynamicQuerySingle(@"Select [TaxRule80D_ParentsBelow60_Self_Family_Children] As ParentsBelow60_Self_Family_Children
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
                                            from Incometax_rule where  Deactivate=0 and IncomeTaxRule_FyearId= " + InvestmentDeclaration.InvestmentDeclarationFyearId + "").FirstOrDefault();
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
                var CheckSubmit = DapperORM.DynamicQuerySingle(@"Select top 1 [Month],[FromDay],[ToDay] from[IncomeTax_DocumentOpenClose] where Deactivate=0 and  [Month]=Month(Getdate()) and OpenCloseTypeID =1 and day(Getdate()) Between [FromDay] and ([ToDay])");
                TempData["SubmitValid"] = CheckSubmit;

                InvestmentChapterVI_A ChapterVI_Aobj = new InvestmentChapterVI_A();
                // var FYId = Session["TaxFyearId"];
                if (ChapterVI_Aobj != null)
                {
                    param = new DynamicParameters();
                    param.Add("@p_InvestmentDeclarationFyearId", InvestmentDeclaration.InvestmentDeclarationFyearId);
                    param.Add("@p_InvestmentDeclarationEmployeeId", InvestmentDeclaration.InvestmentDeclarationEmployeeId);
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

                #endregion Chapter VI-A

                #region Other Income
                var Date3 = DapperORM.DynamicQuerySingle(" Select  OtherIncome_SubmitDate, OtherIncome_SubmitCount   from IncomeTax_InvestmentDeclaration where Deactivate = 0  and [InvestmentDeclarationFyearId] = " + InvestmentDeclaration.InvestmentDeclarationFyearId + " and [InvestmentDeclarationEmployeeId] = " + InvestmentDeclaration.InvestmentDeclarationEmployeeId + "");
                TempData["OtherIncome_SubmitDate"] = null;
                TempData["OtherIncome_SubmitCount"] = null;
                if (Date != null)
                {
                    TempData["OtherIncome_SubmitDate"] = Date3.OtherIncome_SubmitDate;
                    TempData["OtherIncome_SubmitCount"] = Date3.OtherIncome_SubmitCount;
                }

                #endregion Other Income

                #region PreviousEmployer
                var Date1 = DapperORM.DynamicQuerySingle("Select Previous_Er_SubmitDate, Previous_Er_SubmitCount  from IncomeTax_InvestmentDeclaration where Deactivate = 0  and [InvestmentDeclarationId] = " + InvestmentDeclarationId + " ");
                TempData["Previous_Er_SubmitDate"] = null;
                TempData["Previous_Er_SubmitCount"] = null;
                if (Date != null)
                {
                    TempData["Previous_Er_SubmitDate"] = Date1.Previous_Er_SubmitDate;
                    TempData["Previous_Er_SubmitCount"] = Date1.Previous_Er_SubmitCount;
                }


                param = new DynamicParameters();
                param.Add("@p_PreviousEmployerFyearId", InvestmentDeclaration.InvestmentDeclarationFyearId);
                param.Add("@p_PreviousEmployerEmployeeId", InvestmentDeclaration.InvestmentDeclarationEmployeeId);
                var PreviousEmployerDetailsList = DapperORM.ReturnList<IncomeTax_InvestmentDeclaration_PreviousEmployer>("sp_List_IncomeTax_InvestmentDeclaration_PreviousEmployer", param).ToList();
                ViewBag.PreviousEmployerDetailsListBag = PreviousEmployerDetailsList;

                #endregion PreviousEmployer

                #region ProofDownload
                param = new DynamicParameters();
                param.Add("@p_InvestmentDeclarationEmployeeId", InvestmentDeclaration.InvestmentDeclarationEmployeeId);
                param.Add("@p_InvestmentDeclarationFyearId", InvestmentDeclaration.InvestmentDeclarationFyearId);
                param.Add("@p_InvestmentDeclarationEmployeeNo", Employee.EmployeeNo);
                var ProofUpload = DapperORM.ReturnList<dynamic>("sp_GetIncomeTaxInvestmentDeclaration_ProofUpload", param).FirstOrDefault();
                ViewBag.GetProofUpload = ProofUpload;
                // InvestmentDeclaration.OtherIncome2_Remark = ProofUpload.HRAProof_Upload;

                #endregion ProofDownload

                return View(InvestmentDeclaration);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion OldRegimeDetails

        #region ApproveReject

        public ActionResult ApproveReject(List<InvestmentDeclaration_ApproveReject> ObjRecordList)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                for (int i = 0; i < ObjRecordList.Count; i++)
                {
                    DynamicParameters param = new DynamicParameters();

                    param.Add("@query", "UPDATE IncomeTax_InvestmentDeclaration " +
                    "SET OfficerEmployeeId = " + Session["EmployeeId"] +
                    ", OfficerStatus = '" + ObjRecordList[i].Status + "'" +
                    ", OfficerApproveRejectRemark = '" + ObjRecordList[i].RejectRemark + "'" +
                    ", OfficerApproveRejectDate = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                    " WHERE Deactivate = 0 " +
                    "AND InvestmentDeclarationId = " + ObjRecordList[i].InvestmentDeclarationId +
                    " AND InvestmentDeclarationFyearId = " + ObjRecordList[i].InvestmentDeclarationFyearId +
                    " AND InvestmentDeclarationEmployeeId = " + ObjRecordList[i].EmployeeId);
                    var GetData = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).FirstOrDefault();

                    if (ObjRecordList[i].Status == "Approved")
                    {
                        param.Add("@query", "UPDATE IncomeTax_RegimeDeclaration " +
                    "SET ApprovedByEmployeeId = " + Session["EmployeeId"] +
                    ", Status = '" + ObjRecordList[i].Status + "'" +
                    " , ActualStatus = " + "'OldRegime '" +
                    ", ApprovedDate = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                    " WHERE Deactivate = 0 " +
                    " AND FYearId = " + ObjRecordList[i].InvestmentDeclarationFyearId +
                    " AND EmployeeId = " + ObjRecordList[i].EmployeeId
                    );
                        var GetData1 = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).FirstOrDefault();
                    }
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion ApproveReject

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
                    return RedirectToAction("ESS_Tax_Investment_ProofUpload", "ESS_Tax_Investment_ProofUpload");
                }

            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = $"Internal server error: {ex.Message}", Icon = "error" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region MetroNonMetroCity
        [HttpPost]
        public ActionResult CheckMetroNonMetro(string CityName)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_CityName", CityName);
                param.Add("@query", "Select MetroCity from IncomeTax_MetroCity where Deactivate=0 and MetroCity='" + CityName + "'");
                var City = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).SingleOrDefault();

                return Json(City, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion MetroNonMetroCity

        #region HRA

        [HttpPost]
        public ActionResult UpdateHRA(InvestmentDeclaration_HRA Approval)
        {
            try
            {

                param.Add("@p_process", string.IsNullOrEmpty(Approval.InvestmentDeclaration_Encrypted) ? "Save" : "Update");
                param.Add("@p_InvestmentDeclarationFyearId", Approval.InvestmentDeclarationFyearId);
                param.Add("@P_InvestmentDeclarationEmployeeId", Approval.InvestmentDeclarationEmployeeId);

                //--------------------Metro-------------------------------------------
                param.Add("@P_HRA_Apr_Metro", Approval.HRA_Apr_Metro);
                param.Add("@P_HRA_May_Metro", Approval.HRA_May_Metro);
                param.Add("@P_HRA_Jun_Metro", Approval.HRA_Jun_Metro);
                param.Add("@P_HRA_Jul_Metro", Approval.HRA_Jul_Metro);
                param.Add("@P_HRA_Aug_Metro", Approval.HRA_Aug_Metro);
                param.Add("@P_HRA_Sep_Metro", Approval.HRA_Sep_Metro);
                param.Add("@P_HRA_Oct_Metro", Approval.HRA_Oct_Metro);
                param.Add("@P_HRA_Nov_Metro", Approval.HRA_Nov_Metro);
                param.Add("@P_HRA_Dec_Metro", Approval.HRA_Dec_Metro);
                param.Add("@P_HRA_Jan_Metro", Approval.HRA_Jan_Metro);
                param.Add("@P_HRA_Feb_Metro", Approval.HRA_Feb_Metro);
                param.Add("@P_HRA_Mar_Metro", Approval.HRA_Mar_Metro);
                param.Add("@P_HRA_Jan_Metro", Approval.HRA_Jan_Metro);
                //--------------------City Name-------------------------------------------

                param.Add("@P_HRA_Apr_CityName", Approval.HRA_Apr_CityName);
                param.Add("@P_HRA_May_CityName", Approval.HRA_May_CityName);
                param.Add("@P_HRA_Jun_CityName", Approval.HRA_Jun_CityName);
                param.Add("@P_HRA_Jul_CityName", Approval.HRA_Jul_CityName);
                param.Add("@P_HRA_Aug_CityName", Approval.HRA_Aug_CityName);
                param.Add("@P_HRA_Sep_CityName", Approval.HRA_Sep_CityName);
                param.Add("@P_HRA_Oct_CityName", Approval.HRA_Oct_CityName);
                param.Add("@P_HRA_Nov_CityName", Approval.HRA_Nov_CityName);
                param.Add("@P_HRA_Dec_CityName", Approval.HRA_Dec_CityName);
                param.Add("@P_HRA_Jan_CityName", Approval.HRA_Jan_CityName);
                param.Add("@P_HRA_Feb_CityName", Approval.HRA_Feb_CityName);
                param.Add("@P_HRA_Mar_CityName", Approval.HRA_Mar_CityName);

                //--------------------Rent Amount-------------------------------------------
                param.Add("@P_HRA_Apr_CurrentAmt", Approval.HRA_Apr_CurrentAmt);
                param.Add("@P_HRA_May_CurrentAmt", Approval.HRA_May_CurrentAmt);
                param.Add("@P_HRA_Jun_CurrentAmt", Approval.HRA_Jun_CurrentAmt);
                param.Add("@P_HRA_Jul_CurrentAmt", Approval.HRA_Jul_CurrentAmt);
                param.Add("@P_HRA_Aug_CurrentAmt", Approval.HRA_Aug_CurrentAmt);
                param.Add("@P_HRA_Sep_CurrentAmt", Approval.HRA_Sep_CurrentAmt);
                param.Add("@P_HRA_Oct_CurrentAmt", Approval.HRA_Oct_CurrentAmt);
                param.Add("@P_HRA_Nov_CurrentAmt", Approval.HRA_Nov_CurrentAmt);
                param.Add("@P_HRA_Dec_CurrentAmt", Approval.HRA_Dec_CurrentAmt);
                param.Add("@P_HRA_Jan_CurrentAmt", Approval.HRA_Jan_CurrentAmt);
                param.Add("@P_HRA_Feb_CurrentAmt", Approval.HRA_Feb_CurrentAmt);
                param.Add("@P_HRA_Mar_CurrentAmt", Approval.HRA_Mar_CurrentAmt);

                //--------------------LandLord Name-------------------------------------------
                param.Add("@P_HRA_Apr_LandlordName", Approval.HRA_Apr_LandlordName);
                param.Add("@P_HRA_May_LandlordName", Approval.HRA_May_LandlordName);
                param.Add("@P_HRA_Jun_LandlordName", Approval.HRA_Jun_LandlordName);
                param.Add("@P_HRA_Jul_LandlordName", Approval.HRA_Jul_LandlordName);
                param.Add("@P_HRA_Aug_LandlordName", Approval.HRA_Aug_LandlordName);
                param.Add("@P_HRA_Sep_LandlordName", Approval.HRA_Sep_LandlordName);
                param.Add("@P_HRA_Oct_LandlordName", Approval.HRA_Oct_LandlordName);
                param.Add("@P_HRA_Nov_LandlordName", Approval.HRA_Nov_LandlordName);
                param.Add("@P_HRA_Dec_LandlordName", Approval.HRA_Dec_LandlordName);
                param.Add("@P_HRA_Jan_LandlordName", Approval.HRA_Jan_LandlordName);
                param.Add("@P_HRA_Feb_LandlordName", Approval.HRA_Feb_LandlordName);
                param.Add("@P_HRA_Mar_LandlordName", Approval.HRA_Mar_LandlordName);

                //--------------------LandLord PAN-------------------------------------
                param.Add("@P_HRA_Apr_PanNo", Approval.HRA_Apr_PanNo);
                param.Add("@P_HRA_May_PanNo", Approval.HRA_May_PanNo);
                param.Add("@P_HRA_Jun_PanNo", Approval.HRA_Jun_PanNo);
                param.Add("@P_HRA_Jul_PanNo", Approval.HRA_Jul_PanNo);
                param.Add("@P_HRA_Aug_PanNo", Approval.HRA_Aug_PanNo);
                param.Add("@P_HRA_Sep_PanNo", Approval.HRA_Sep_PanNo);
                param.Add("@P_HRA_Oct_PanNo", Approval.HRA_Oct_PanNo);
                param.Add("@P_HRA_Nov_PanNo", Approval.HRA_Nov_PanNo);
                param.Add("@P_HRA_Dec_PanNo", Approval.HRA_Dec_PanNo);
                param.Add("@P_HRA_Jan_PanNo", Approval.HRA_Jan_PanNo);
                param.Add("@P_HRA_Feb_PanNo", Approval.HRA_Feb_PanNo);
                param.Add("@P_HRA_Mar_PanNo", Approval.HRA_Mar_PanNo);

                //--------------------Address-------------------------------------------
                param.Add("@P_HRA_Apr_Address", Approval.HRA_Apr_Address);
                param.Add("@P_HRA_May_Address", Approval.HRA_May_Address);
                param.Add("@P_HRA_Jun_Address", Approval.HRA_Jun_Address);
                param.Add("@P_HRA_Jul_Address", Approval.HRA_Jul_Address);
                param.Add("@p_HRA_Aug_Address", Approval.HRA_Aug_Address);
                param.Add("@P_HRA_Sep_Address", Approval.HRA_Sep_Address);
                param.Add("@P_HRA_Oct_Address", Approval.HRA_Oct_Address);
                param.Add("@P_HRA_Nov_Address", Approval.HRA_Nov_Address);
                param.Add("@P_HRA_Dec_Address", Approval.HRA_Dec_Address);
                param.Add("@P_HRA_Jan_Address", Approval.HRA_Jan_Address);
                param.Add("@P_HRA_Feb_Address", Approval.HRA_Feb_Address);
                param.Add("@P_HRA_Mar_Address", Approval.HRA_Mar_Address);

                param.Add("@p_HRA_Total", Approval.HRA_Total);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_InvestmentDeclaration_HRA", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;

                return RedirectToAction("OldRegimeDetails", "Ess_Tax_DeclarationApproval_OldRegime", new { InvestmentDeclarationId = Approval.InvestmentDeclarationId });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion HRA

        #region UpdateUnderSection80C
        [HttpPost]
        public ActionResult UpdateUnderSection80C(InvestmentDeclaration_80C Approval)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(Approval.InvestmentDeclaration_Encrypted) ? "Save" : "Update");
                param.Add("@p_InvestmentDeclarationFyearId", Approval.InvestmentDeclarationFyearId);
                param.Add("@p_InvestmentDeclarationEmployeeId", Approval.InvestmentDeclarationEmployeeId);

                param.Add("@p_US80C_EmployeesNationalPensionScheme80CCD1_CurrentAmt", Approval.US80C_EmployeesNationalPensionScheme80CCD1_CurrentAmt);
                param.Add("@p_US80C_EmployeesNationalPensionScheme80CCD1_LimitAmt", Approval.US80C_EmployeesNationalPensionScheme80CCD1_LimitAmt);
                param.Add("@p_US80C_EmployeesNationalPensionScheme80CCD1_EligibleDeductionAmt", Approval.US80C_EmployeesNationalPensionScheme80CCD1_EligibleDeductionAmt);
                param.Add("@p_US80C_PensionPlanfromInsuranceCompaniesMutualFunds_CurrentAmt", Approval.US80C_PensionPlanfromInsuranceCompaniesMutualFunds_CurrentAmt);
                param.Add("@p_US80C_PensionPlanfromInsuranceCompaniesMutualFunds_LimitAmt", Approval.US80C_PensionPlanfromInsuranceCompaniesMutualFunds_LimitAmt);
                param.Add("@p_US80C_PensionPlanfromInsuranceCompaniesMutualFunds_EligibleDeductionAmt", Approval.US80C_PensionPlanfromInsuranceCompaniesMutualFunds_EligibleDeductionAmt);
                param.Add("@p_US80C_HousingLoanPrincipalRepayment_CurrentAmt", Approval.US80C_HousingLoanPrincipalRepayment_CurrentAmt);
                param.Add("@p_US80C_HousingLoanPrincipalRepayment_LimitAmt", Approval.US80C_HousingLoanPrincipalRepayment_LimitAmt);
                param.Add("@p_US80C_HousingLoanPrincipalRepayment_EligibleDeductionAmt", Approval.US80C_HousingLoanPrincipalRepayment_EligibleDeductionAmt);
                param.Add("@p_US80C_LifeInsurancePremium_CurrentAmt", Approval.US80C_LifeInsurancePremium_CurrentAmt);
                param.Add("@p_US80C_LifeInsurancePremium_LimitAmt", Approval.US80C_LifeInsurancePremium_LimitAmt);
                param.Add("@p_US80C_LifeInsurancePremium_EligibleDeductionAmt", Approval.US80C_LifeInsurancePremium_EligibleDeductionAmt);
                param.Add("@p_US80C_NationalSavingCertificate_CurrentAmt", Approval.US80C_NationalSavingCertificate_CurrentAmt);
                param.Add("@p_US80C_NationalSavingCertificate_LimitAmt", Approval.US80C_NationalSavingCertificate_LimitAmt);
                param.Add("@p_US80C_NationalSavingCertificate_EligibleDeductionAmt", Approval.US80C_NationalSavingCertificate_EligibleDeductionAmt);
                param.Add("@p_US80C_NationalSavingCertificateAccruedInterest_CurrentAmt", Approval.US80C_NationalSavingCertificateAccruedInterest_CurrentAmt);
                param.Add("@p_US80C_NationalSavingCertificateAccruedInterest_LimitAmt", Approval.US80C_NationalSavingCertificateAccruedInterest_LimitAmt);
                param.Add("@p_US80C_NationalSavingCertificateAccruedInterest_EligibleDeductionAmt", Approval.US80C_NationalSavingCertificateAccruedInterest_EligibleDeductionAmt);
                param.Add("@p_US80C_PublicProvidentFund_CurrentAmt", Approval.US80C_PublicProvidentFund_CurrentAmt);
                param.Add("@p_US80C_PublicProvidentFund_LimitAmt", Approval.US80C_PublicProvidentFund_LimitAmt);
                param.Add("@p_US80C_PublicProvidentFund_EligibleDeductionAmt", Approval.US80C_PublicProvidentFund_EligibleDeductionAmt);
                param.Add("@p_US80C_UnitLinkedInsurancePlan_CurrentAmt", Approval.US80C_UnitLinkedInsurancePlan_CurrentAmt);
                param.Add("@p_US80C_UnitLinkedInsurancePlan_LimitAmt", Approval.US80C_UnitLinkedInsurancePlan_LimitAmt);
                param.Add("@p_US80C_UnitLinkedInsurancePlan_EligibleDeductionAmt", Approval.US80C_UnitLinkedInsurancePlan_EligibleDeductionAmt);
                param.Add("@p_US80C_ELSSTaxSavingMutualFund_CurrentAmt", Approval.US80C_ELSSTaxSavingMutualFund_CurrentAmt);
                param.Add("@p_US80C_ELSSTaxSavingMutualFund_LimitAmt", Approval.US80C_ELSSTaxSavingMutualFund_LimitAmt);
                param.Add("@p_US80C_ELSSTaxSavingMutualFund_EligibleDeductionAmt", Approval.US80C_ELSSTaxSavingMutualFund_EligibleDeductionAmt);
                param.Add("@p_US80C_Tuitionfeesfor2children_CurrentAmt", Approval.US80C_Tuitionfeesfor2children_CurrentAmt);
                param.Add("@p_US80C_Tuitionfeesfor2children_LimitAmt", Approval.US80C_Tuitionfeesfor2children_LimitAmt);
                param.Add("@p_US80C_Tuitionfeesfor2children_EligibleDeductionAmt", Approval.US80C_Tuitionfeesfor2children_EligibleDeductionAmt);
                param.Add("@p_US80C_TaxSavingSharesNABARDandOtherBonds_CurrentAmt", Approval.US80C_TaxSavingSharesNABARDandOtherBonds_CurrentAmt);
                param.Add("@p_US80C_TaxSavingSharesNABARDandOtherBonds_LimitAmt", Approval.US80C_TaxSavingSharesNABARDandOtherBonds_LimitAmt);
                param.Add("@p_US80C_TaxSavingSharesNABARDandOtherBonds_EligibleDeductionAmt", Approval.US80C_TaxSavingSharesNABARDandOtherBonds_EligibleDeductionAmt);
                param.Add("@p_US80C_TaxsavingFixDepositsTenure5yearsormore_CurrentAmt", Approval.US80C_TaxsavingFixDepositsTenure5yearsormore_CurrentAmt);
                param.Add("@p_US80C_TaxsavingFixDepositsTenure5yearsormore_LimitAmt", Approval.US80C_TaxsavingFixDepositsTenure5yearsormore_LimitAmt);
                param.Add("@p_US80C_TaxsavingFixDepositsTenure5yearsormore_EligibleDeductionAmt", Approval.US80C_TaxsavingFixDepositsTenure5yearsormore_EligibleDeductionAmt);
                param.Add("@p_US80C_HousingStampDutyRegistration_CurrentAmt", Approval.US80C_HousingStampDutyRegistration_CurrentAmt);
                param.Add("@p_US80C_HousingStampDutyRegistration_LimitAmt", Approval.US80C_HousingStampDutyRegistration_LimitAmt);
                param.Add("@p_US80C_HousingStampDutyRegistration_EligibleDeductionAmt", Approval.US80C_HousingStampDutyRegistration_EligibleDeductionAmt);
                param.Add("@p_US80C_PostOfficeTimeDepositScheme_CurrentAmt", Approval.US80C_PostOfficeTimeDepositScheme_CurrentAmt);
                param.Add("@p_US80C_PostOfficeTimeDepositScheme_LimitAmt", Approval.US80C_PostOfficeTimeDepositScheme_LimitAmt);
                param.Add("@p_US80C_PostOfficeTimeDepositScheme_EligibleDeductionAmt", Approval.US80C_PostOfficeTimeDepositScheme_EligibleDeductionAmt);
                param.Add("@p_US80C_SeniorCitizenSavingScheme_CurrentAmt", Approval.US80C_SeniorCitizenSavingScheme_CurrentAmt);
                param.Add("@p_US80C_SeniorCitizenSavingScheme_LimitAmt", Approval.US80C_SeniorCitizenSavingScheme_LimitAmt);
                param.Add("@p_US80C_SeniorCitizenSavingScheme_EligibleDeductionAmt", Approval.US80C_SeniorCitizenSavingScheme_EligibleDeductionAmt);
                param.Add("@p_US80C_SukanyaSamriddhiScheme_CurrentAmt", Approval.US80C_SukanyaSamriddhiScheme_CurrentAmt);
                param.Add("@p_US80C_SukanyaSamriddhiScheme_LimitAmt", Approval.US80C_SukanyaSamriddhiScheme_LimitAmt);
                param.Add("@p_US80C_SukanyaSamriddhiScheme_EligibleDeductionAmt", Approval.US80C_SukanyaSamriddhiScheme_EligibleDeductionAmt);
                param.Add("@p_US80C_80CCC_CurrentAmt", Approval.US80C_80CCC_CurrentAmt);
                param.Add("@p_US80C_80CCC_LimitAmt", Approval.US80C_80CCC_LimitAmt);
                param.Add("@p_US80C_80CCC_EligibleDeductionAmt", Approval.US80C_80CCC_EligibleDeductionAmt);
                param.Add("@p_US80C_SuperannuationFundContribution_CurrentAmt", Approval.US80C_SuperannuationFundContribution_CurrentAmt);
                param.Add("@p_US80C_SuperannuationFundContribution_LimitAmt", Approval.US80C_SuperannuationFundContribution_LimitAmt);
                param.Add("@p_US80C_SuperannuationFundContribution_EligibleDeductionAmt", Approval.US80C_SuperannuationFundContribution_EligibleDeductionAmt);

                param.Add("@p_US80C_Total", Approval.US80C_Total);
                param.Add("@p_US80C_Total_Limit", Approval.US80C_Total_Limit);
                param.Add("@p_US80C_Total_EligibleDeduction", Approval.US80C_Total_EligibleDeduction);


                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_InvestmentDeclaration_80C", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;

                return RedirectToAction("OldRegimeDetails", "Ess_Tax_DeclarationApproval_OldRegime", new { InvestmentDeclarationId = Approval.InvestmentDeclarationId });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion UpdateUnderSection80C

        #region SelfOccupiedHouseLoan

        [HttpPost]
        public ActionResult UpdateSelfOccupiedHouseLoan(InvestmentDeclaration_SelfOccupiedHouseLoan Approval)
        {
            try
            {
                // param.Add("@p_process", string.IsNullOrEmpty(InvestmentDeclaration.InvestmentDeclaration_Encrypted) ? "Save" : "Update");
                param.Add("@p_InvestmentDeclarationFyearId", Approval.InvestmentDeclarationFyearId);
                param.Add("@p_InvestmentDeclarationEmployeeId", Approval.InvestmentDeclarationEmployeeId);

                param.Add("@p_SelfOccupied_TypeID", Approval.SelfOccupied_TypeID);
                param.Add("@p_SelfOccupied_CurrentAmt", Approval.SelfOccupied_CurrentAmt);
                param.Add("@p_SelfOccupied_Limit", Approval.SelfOccupied_Limit);
                param.Add("@p_SelfOccupied_EligibleDeductionAmt", Approval.SelfOccupied_EligibleDeductionAmt);
                if (Approval.SelfOccupied_TypeID == 1)
                {
                    param.Add("@p_SelfOccupied_Type", "Loan On or After 01/04/1999");
                }
                else if (Approval.SelfOccupied_TypeID == 2)
                {
                    param.Add("@p_SelfOccupied_Type", "Loan Before 01/04/1999");
                }
                else if (Approval.SelfOccupied_TypeID == 3)
                {
                    param.Add("@p_SelfOccupied_Type", "Loan On or After 01/04/1999 (First Home (Section 80EEA) (01/Apr/2019 - 31/Mar/2022))");
                }
                else if (Approval.SelfOccupied_TypeID == 4)
                {
                    param.Add("@p_SelfOccupied_Type", "Loan Before 01/04/1999 (First Home (Section 80EEA) (01/Apr/2019 - 31/Mar/2022))");
                }
                else if (Approval.SelfOccupied_TypeID == 5)
                {
                    param.Add("@p_SelfOccupied_Type", "Loan On or After 01/04/1999 (First Home (Section 80EE) (01/Apr/2016 - 31/Mar/2017))");
                }
                else if (Approval.SelfOccupied_TypeID == 6)
                {
                    param.Add("@p_SelfOccupied_Type", "Loan Before 01/04/1999 (First Home (Section 80EE) (01/Apr/2016 - 31/Mar/2017))");
                }
                else if (Approval.SelfOccupied_TypeID == 7)
                {
                    param.Add("@p_SelfOccupied_Type", "Repair or renewal or reconstruction of the house");
                }
                param.Add("@p_SelfOccupied_IsCoOwner", Approval.SelfOccupied_IsCoOwner);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_InvestmentDeclaration_SelfOccupiedHouseLoan", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return RedirectToAction("OldRegimeDetails", "Ess_Tax_DeclarationApproval_OldRegime", new { InvestmentDeclarationId = Approval.InvestmentDeclarationId });

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion SelfOccupiedHouseLoan

        #region LetOutHouseProperty
        public ActionResult UpdateLetOutHouseProperty(List<IncomeTax_InvestmentDeclaration_LetOutHouseProperty> Task, InvestmentDeclaration_TotalIncomeLetOutHouseProperty LetOutHouseProperty)
        {
            try
            {
                double amount = 0;
                DapperORM.ExecuteQuery("Update IncomeTax_InvestmentDeclaration_LetOutHouseProperty set Deactivate=1 where IncomeFromLetOutHousePropertyFyearId='" + Task[1].IncomeFromLetOutHousePropertyFyearId + "' and  IncomeFromLetOutHousePropertyEmployeeId='" + Task[1].IncomeFromLetOutHousePropertyEmployeeId + "'");
                double InvestmentDeclarationFyearId = Task[0].IncomeFromLetOutHousePropertyFyearId;
                double IncomeFromLetOutHousePropertyEmployeeId = Task[0].IncomeFromLetOutHousePropertyEmployeeId;
                for (var i = 0; i < Task.Count; i++)
                {
                    param.Add("@p_process", "Save");
                    param.Add("@p_IncomeFromLetOutHousePropertyFyearId", Task[i].IncomeFromLetOutHousePropertyFyearId);
                    param.Add("@p_IncomeFromLetOutHousePropertyEmployeeId", Task[i].IncomeFromLetOutHousePropertyEmployeeId);
                    param.Add("@p_DescriptionOfProperty", Task[i].DescriptionOfProperty);
                    param.Add("@p_GrossAnnualValue", Task[i].GrossAnnualValue);
                    param.Add("@p_MunicipalTaxes", Task[i].MunicipalTaxes);
                    param.Add("@p_NetAnnualValue", Task[i].NetAnnualValue);
                    param.Add("@p_StandardDeduction", Task[i].StandardDeduction);
                    param.Add("@p_InterestOnBorrowedCapital", Task[i].InterestOnBorrowedCapital);
                    param.Add("@p_IncomeFromLetOutHouseProperty", Task[i].IncomeFromLetOutHouseProperty);

                    amount = amount + Convert.ToDouble(Task[i].IncomeFromLetOutHouseProperty);

                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data1 = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_InvestmentDeclaration_LetOutHouseProperty", param);
                    var message1 = param.Get<string>("@p_msg");
                    var Icon1 = param.Get<string>("@p_Icon");
                    TempData["Message"] = message1;
                    TempData["Icon"] = Icon1;


                }
                param = new DynamicParameters();
                param.Add("@p_process", "Save");
                param.Add("@p_InvestmentDeclarationFyearId", InvestmentDeclarationFyearId);
                param.Add("@P_InvestmentDeclarationEmployeeId", IncomeFromLetOutHousePropertyEmployeeId);

                param.Add("@p_TotalIncomeFromLetOutHouseProperty", amount);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_IncomeTax_InvestmentDeclaration_LetOutHousePropertyMaster]", param);
                //var message = param.Get<string>("@p_msg");
                //var Icon = param.Get<string>("@p_Icon");
                //TempData["Message"] = message;
                //TempData["Icon"] = Icon;

                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion LetOutHouseProperty

        #region ChapterVI-A        
        [HttpPost]
        public ActionResult UpdateChapterVIA(List<Add80G_A> Add80G_A, List<Add80G_B> Add80G_B, InvestmentChapterVI_A Record)
        {
            try
            {
                var EmpId = Session["EmployeeId"];
                param.Add("@p_process", "Save");

                //param.Add("@p_Id", Record.PreviousEmployerName);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@P_InvestmentDeclarationId", Record.InvestmentDeclarationId);
                //param.Add("@P_InvestmentDeclaration_Encrypted", Record.InvestmentDeclaration_Encrypted);
                param.Add("@P_InvestmentDeclarationFyearId", Record.InvestmentDeclarationFyearId);
                param.Add("@P_InvestmentDeclarationEmployeeId", Record.InvestmentDeclarationEmployeeId);
                //param.Add("@P_InvestmentDeclarationEmployeeNo", Record.InvestmentDeclarationEmployeeNo);
                //param.Add("@P_InvestmentDeclarationEmployeeName", Record.InvestmentDeclarationEmployeeName);

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
                //  param.Add("@p_ChapterVIA_VehiclePurchaseDate",Record.ChapterVIA_VehiclePurchaseDate == DateTime.MinValue ? (object)DBNull.Value : Record.ChapterVIA_VehiclePurchaseDate);
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

                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_InvestmentDeclaration_ChapterVIA", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;


                return RedirectToAction("OldRegimeDetails", "Ess_Tax_DeclarationApproval_OldRegime", new { InvestmentDeclarationId = Record.InvestmentDeclarationId });

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion ChapterVI-A

        #region OtherDetails
        [HttpPost]
        public ActionResult UpdateOtherDetails(InvestmentDeclaration_OtherDetails Approval)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(Approval.InvestmentDeclaration_Encrypted) ? "Save" : "Update");
                param.Add("@p_InvestmentDeclarationFyearId", Approval.InvestmentDeclarationFyearId);
                param.Add("@P_InvestmentDeclarationEmployeeId", Approval.InvestmentDeclarationEmployeeId);

                param.Add("@p_OtherIncome1", Approval.OtherIncome1);
                param.Add("@p_OtherIncome1_Remark", Approval.OtherIncome1_Remark);
                param.Add("@p_OtherIncome2", Approval.OtherIncome2);
                param.Add("@p_OtherIncome2_Remark", Approval.OtherIncome2_Remark);
                param.Add("@p_OtherIncome3", Approval.OtherIncome3);
                param.Add("@p_OtherIncome3_Remark", Approval.OtherIncome3_Remark);
                param.Add("@p_TotalOtherIncome", Approval.TotalOtherIncome);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_InvestmentDeclaration_OtherIncome", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;

                return RedirectToAction("OldRegimeDetails", "Ess_Tax_DeclarationApproval_OldRegime", new { InvestmentDeclarationId = Approval.InvestmentDeclarationId });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion OtherDetails

        #region PreviousEmployer       
        [HttpPost]
        public ActionResult UpdatePreviousEmployer(string Task/*List<IncomeTax_InvestmentDeclaration_PreviousEmployer> Record*/, InvestmentDeclaration_TotalPreviousEmployer TotalPreviousEmployer, bool UnemployedInPreviousYear, string UnemployedInPreviousYearDescription, int PreviousEmployerFyearId, int PreviousEmployerEmployeeId, string InvestmentDeclarationEmployeeName, string EmployeeNo)
        // public ActionResult UpdatePreviousEmployer(List<IncomeTax_InvestmentDeclaration_PreviousEmployer> Record, InvestmentDeclaration_TotalPreviousEmployer TotalPreviousEmployer)
        {
            try
            {
                StringBuilder strBuilder = new StringBuilder();
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
                double fy = 0, emp = 0;
                if (Record != null)
                {
                    //if (Record.Count > 0)
                    //{
                    //    fy = Record[0].PreviousEmployerFyearId;
                    //    emp = Record[0].PreviousEmployerEmployeeId;
                    //}
                    DapperORM.ExecuteQuery("Update IncomeTax_InvestmentDeclaration_PreviousEmployer set Deactivate=1 where PreviousEmployerFyearId='" + PreviousEmployerFyearId + "'  and  PreviousEmployerEmployeeId='" + PreviousEmployerEmployeeId + "'");
                }


                if (UnemployedInPreviousYear == true)
                {


                    string InvestmentDeclaration = @" IF EXISTS (
                                                        SELECT InvestmentDeclarationEmployeeId 
                                                        FROM IncomeTax_InvestmentDeclaration 
                                                        WHERE InvestmentDeclarationFyearId = '" + PreviousEmployerFyearId + @"' 
                                                          AND InvestmentDeclarationEmployeeId = '" + PreviousEmployerEmployeeId + @"' 
                                                          AND Deactivate = 0
                                                    )
                                                    BEGIN
                                                        UPDATE IncomeTax_InvestmentDeclaration 
                                                        SET 
                                                            Previous_Er_Unemployed = " + (UnemployedInPreviousYear ? "1" : "0") + @",
                                                            Previous_Er_UnemployedDescription = '" + UnemployedInPreviousYearDescription + @"' 
                                                        WHERE 
                                                            InvestmentDeclarationFyearId = '" + PreviousEmployerFyearId + @"' 
                                                            AND InvestmentDeclarationEmployeeId = '" + PreviousEmployerEmployeeId + @"'
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
                                                            '" + InvestmentDeclarationEmployeeName + @"', GETDATE(), 0,
                                                            '" + PreviousEmployerFyearId + @"',
                                                            '" + PreviousEmployerEmployeeId + @"',
                                                            '" + EmployeeNo + @"',
                                                            '" + InvestmentDeclarationEmployeeName + @"',
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
                        param.Add("@p_PreviousEmployerFyearId", Record[i].PreviousEmployerFyearId);
                        param.Add("@p_PreviousEmployerEmployeeId", Record[i].PreviousEmployerEmployeeId);
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
                    param.Add("@p_InvestmentDeclarationFyearId", Record[0].PreviousEmployerFyearId);
                    param.Add("@P_InvestmentDeclarationEmployeeId", Record[0].PreviousEmployerEmployeeId);

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

                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    //return RedirectToAction("PreviousEmployerDetails", "PreviousEmployerDetails");
                }
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion PreviousEmployer

        #region GetList
        public ActionResult GetList(int? TaxFyearId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 531;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "Select TaxFyearId as Id,TaxYear As Name  from IncomeTax_Fyear where Deactivate=0 and [IsActive]=1");
                var GetTaxFyear = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param).ToList();
                ViewBag.GetTaxFyear = GetTaxFyear;


                param = new DynamicParameters();
                param.Add("@p_FYearID", TaxFyearId);
                param.Add("@p_RegimeTypeTD", 1);
                var data = DapperORM.ExecuteSP<dynamic>("SP_List_IncomeTax_ApprovedRegimedeclartion", param).ToList();
                ViewBag.ApprovalList = data;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion GetList

    }
}