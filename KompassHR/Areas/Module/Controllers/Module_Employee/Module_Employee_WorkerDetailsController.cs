using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
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

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_WorkerDetailsController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: Module/Module_Employee_WorkerDetails
        #region Module_Employee_WorkerStatutoryDetails
        public ActionResult Module_Employee_WorkerStatutoryDetails()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                Mas_Employee_Statutory EmployeeStatutory = new Mas_Employee_Statutory();
                ViewBag.AddUpdateTitle = "Add";
                //var ResultList = DapperORM.DynamicQuerySingleMultiple(@" Select PTCodeId as Id,PTCode as Name from Payroll_PTCode Where Deactivate=0 and PTCodeBranchId in (select UserBranchMapping.BranchID as Id from UserBranchMapping where  UserBranchMapping.employeeid = " + Session["WorkerOnboardEmployeeId"] + "  and UserBranchMapping.CmpID = " + Session["WorkerOnboardCmpId"] + " and UserBranchMapping.IsActive = 1 union select EmployeeBranchId as Id  from Mas_Employee where  EmployeeId= " + Session["WorkerOnboardEmployeeId"] + "  and Mas_Employee.CmpID= " +Session["WorkerOnboardCmpId"]  + " );"
                //                                        + "Select LWFCodeId as Id,LWFCode as Name from Payroll_LWFCode Where Deactivate=0 and Payroll_LWFCode.LWFCodeBranchId in (select UserBranchMapping.BranchID as Id from UserBranchMapping where  UserBranchMapping.employeeid = " + Session["WorkerOnboardEmployeeId"] + "  and UserBranchMapping.CmpID = " + Session["WorkerOnboardCmpId"] + " and UserBranchMapping.IsActive = 1 union select EmployeeBranchId as Id  from Mas_Employee where  EmployeeId= " + Session["WorkerOnboardEmployeeId"] + "  and Mas_Employee.CmpID= " + Session["WorkerOnboardCmpId"] + " );"
                //                                        + "Select ESICCodeId as Id,ESICCode as Name from Payroll_ESICCode Where Deactivate=0 and Payroll_ESICCode.ESICCodeBranchId in (select UserBranchMapping.BranchID as Id from UserBranchMapping where  UserBranchMapping.employeeid = " + Session["WorkerOnboardEmployeeId"] + "  and UserBranchMapping.CmpID = " + Session["WorkerOnboardCmpId"] + " and UserBranchMapping.IsActive = 1 union select EmployeeBranchId as Id  from Mas_Employee where  EmployeeId= " + Session["WorkerOnboardEmployeeId"] + "  and Mas_Employee.CmpID= " + Session["WorkerOnboardCmpId"] + " );"
                //                                        + "Select PFCodeId as Id,PFCode as Name from Payroll_PFCode Where Deactivate=0 and Payroll_PFCode.PFCodeBranchId in (select UserBranchMapping.BranchID as Id from UserBranchMapping where  UserBranchMapping.employeeid = " + Session["WorkerOnboardEmployeeId"] + "  and UserBranchMapping.CmpID = " + Session["WorkerOnboardCmpId"] + " and UserBranchMapping.IsActive = 1 union select EmployeeBranchId as Id  from Mas_Employee where  EmployeeId= " + Session["WorkerOnboardEmployeeId"] + "  and Mas_Employee.CmpID= " + Session["WorkerOnboardCmpId"] + " );"
                //                                        + "Select RelationId as Id,RelationName as Name from Mas_Relation Where Deactivate=0;");
                //ViewBag.GetPTStateCode = ResultList.Read<AllDropDownClass>().ToList();
                //ViewBag.GetLWFCode = ResultList.Read<AllDropDownClass>().ToList();
                //ViewBag.GetESICCode = ResultList.Read<AllDropDownClass>().ToList();
                //ViewBag.GetPFCode = ResultList.Read<AllDropDownClass>().ToList();
                //ViewBag.GetRelationName = ResultList.Read<AllDropDownClass>().ToList();

                var ResultList = DapperORM.DynamicQueryMultiple(@"Select PTCodeId as Id,PTCode as Name from Payroll_PTCode Where Deactivate=0 and CmpID=" + Session["WorkerOnboardCmpId"] + ""
                                                       + "Select LWFCodeId as Id,LWFCode as Name from Payroll_LWFCode Where Deactivate=0 and CmpID=" + Session["WorkerOnboardCmpId"] + ""
                                                       + "Select ESICCodeId as Id,ESICCode as Name from Payroll_ESICCode Where Deactivate=0 and  CmpID=" + Session["WorkerOnboardCmpId"] + " and ESICCodeBranchId=" + Session["WorkerOnboardBranchId"] + ""
                                                       + "Select PFCodeId as Id,PFCode as Name from Payroll_PFCode Where Deactivate=0 and  CmpId=" + Session["WorkerOnboardCmpId"] + ""
                                                       + "Select PTSlabMasterId as Id,Remark as Name from Payroll_PTSlab_Master Where Deactivate=0 ;"
                                                       + "Select LWFSlabMasterId as Id,Remark as Name from Payroll_LWFSlab_Master Where Deactivate=0 ;"
                                                       + "Select PFWagesMasterId as Id,PFWagesRemark as Name from Payroll_PFWages_Master Where Deactivate=0 and  CmpId=" + Session["WorkerOnboardCmpId"] + ""
                                                       + "Select RelationId as Id,RelationName as Name from Mas_Relation Where Deactivate=0;");
                ViewBag.GetPTStateCode = ResultList[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetLWFCode = ResultList[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetESICCode = ResultList[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetPFCode = ResultList[3].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetPTSlabMaster = ResultList[4].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetLWFSlabMaster = ResultList[5].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetPFWageMaster = ResultList[6].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetRelationName = ResultList[7].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_StatutoryEmployeeId", Session["WorkerOnboardEmployeeId"]);
                EmployeeStatutory = DapperORM.ReturnList<Mas_Employee_Statutory>("sp_List_Mas_Employee_Statutory", paramList).FirstOrDefault();
                if (EmployeeStatutory != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    TempData["PF_DOB1"] = EmployeeStatutory.PF_DOB1;
                }
                ViewBag.PFApplicable = EmployeeStatutory;
                return View(EmployeeStatutory);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpPost]
        public ActionResult SaveWorkerStatutory(Mas_Employee_Statutory Statutory)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(Statutory.StatutoryId_Encrypted) ? "Save" : "Update");
                param.Add("@p_StatutoryId", Statutory.StatutoryId);
                param.Add("@p_StatutoryId_Encrypted", Statutory.StatutoryId_Encrypted);
                param.Add("@p_StatutoryEmployeeId", Session["WorkerOnboardEmployeeId"]);
                param.Add("@p_ESIC_Applicable", Statutory.ESIC_Applicable);
                param.Add("@p_ESIC_CodeId", Statutory.ESIC_CodeId);
                param.Add("@p_ESIC_NO", Statutory.ESIC_NO);
                param.Add("@p_ESIC_ClosingDate", Statutory.ESIC_ClosingDate);
                param.Add("@p_ESIC_IS_OldESICNo", Statutory.ESIC_IS_OldESICNo);
                param.Add("@p_ESIC_IS_LinkWithESIC", Statutory.ESIC_IS_LinkWithESIC);
                param.Add("@p_ESIC_PreviousESICNo", Statutory.ESIC_PreviousESICNo);
                param.Add("@p_PT_Applicable", Statutory.PT_Applicable);
                param.Add("@p_PT_CodeId", Statutory.PT_CodeId);
                param.Add("@p_LWF_Applicable", Statutory.LWF_Applicable);
                param.Add("@p_LWF_CodeId", Statutory.LWF_CodeId);
                param.Add("@p_PF_Applicable", Statutory.PF_Applicable);
                param.Add("@p_PF_CodeId", Statutory.PF_CodeId);
                param.Add("@p_PF_Limit", Statutory.PF_Limit);
                //param.Add("@p_PF_EPS", Statutory.PF_EPS);
                param.Add("@p_PF_VPF", Statutory.PF_VPF);
                param.Add("@p_PF_FSType", Statutory.PF_FSType);
                param.Add("@p_PF_FS_Name", Statutory.PF_FS_Name);
                param.Add("@p_PF_UAN", Statutory.PF_UAN);
                param.Add("@p_PF_NO", Statutory.PF_NO);
                param.Add("@p_PF_Nominee1", Statutory.PF_Nominee1);
                param.Add("@p_PF_Reletion1", Statutory.PF_Reletion1);
                param.Add("@p_PF_DOB1", Statutory.PF_DOB1);
                param.Add("@p_PF_Share1", Statutory.PF_Share1);
                param.Add("@p_PF_Address1", Statutory.PF_Address1);
                param.Add("@p_PF_GuardianName1", Statutory.PF_GuardianName1);
                param.Add("@p_PF_MobileNo", Statutory.PF_MobileNo);
                param.Add("@p_PF_BankName", Statutory.PF_BankName);
                param.Add("@p_PF_BankIFSC", Statutory.PF_BankIFSC);
                param.Add("@p_PF_Account", Statutory.PF_Account);
                param.Add("@p_PF_1952", Statutory.PF_1952);
                param.Add("@p_PF_1995", Statutory.PF_1995);
                param.Add("@p_PF_PreviousPFNo", Statutory.PF_PreviousPFNo);
                param.Add("@p_PF_ExitDate", Statutory.PF_ExitDate);
                param.Add("@p_PF_CertificateNo", Statutory.PF_CertificateNo);
                param.Add("@p_PF_PPO", Statutory.PF_PPO);
                param.Add("@p_PF_OldUANNo", Statutory.PF_OldUANNo);
                param.Add("@p_PF_LinkWithUAN", Statutory.PF_LinkWithUAN);
                param.Add("@p_PTSlab_MasterId", Statutory.PTSlab_MasterId);
                param.Add("@p_LWFSlab_MasterId", Statutory.LWFSlab_MasterId);
                param.Add("@p_PFWages_MasterId", Statutory.PFWages_MasterId);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_Statutory", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_WorkerStatutoryDetails", "Module_Employee_WorkerDetails");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Module_Employee_WorkerBankDetails
        public ActionResult Module_Employee_WorkerBankDetails()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Mas_Employee_Bank Mas_employeebank = new Mas_Employee_Bank();
                param = new DynamicParameters();
                TempData["WorkerOnboardEmployeeName"] = Session["WorkerOnboardEmployeeName"];
                param.Add("@p_EmployeeBankEmployeeId", Session["WorkerOnboardEmployeeId"]);
                Mas_employeebank = DapperORM.ReturnList<Mas_Employee_Bank>("sp_List_Mas_Employee_Bank", param).FirstOrDefault();
                if (Mas_employeebank != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                }
                return View(Mas_employeebank);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpPost]
        public ActionResult SaveWorkerBank(Mas_Employee_Bank EmployeeBank)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var EmployeeId = Session["WorkerEmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(EmployeeBank.EmployeeBankId_Encrypted) ? "Save" : "Update");
                param.Add("@p_EmployeeBankId", EmployeeBank.EmployeeBankId);
                param.Add("@p_EmployeeBankId_Encrypted", EmployeeBank.EmployeeBankId_Encrypted);
                param.Add("@p_EmployeeBankEmployeeId", Session["WorkerOnboardEmployeeId"]);
                param.Add("@p_SalaryIFSC", EmployeeBank.SalaryIFSC);
                param.Add("@p_SalaryBankName", EmployeeBank.SalaryBankName);
                param.Add("@p_SalaryAccountNo", EmployeeBank.SalaryAccountNo);
                param.Add("@p_SalaryConfirmAccountNo", EmployeeBank.SalaryConfirmAccountNo);
                param.Add("@p_SalaryBankAddress", EmployeeBank.SalaryBankAddress);
                param.Add("@p_SalaryBankBranchName", EmployeeBank.SalaryBankBranchName);
                param.Add("@p_SalaryNameAsPerBank", EmployeeBank.SalaryNameAsPerBank);
                //param.Add("@p_PermanentIFSC", EmployeeBank.PermanentIFSC);
                //param.Add("@p_PermanentBankName", EmployeeBank.PermanentBankName);
                //param.Add("@p_PermanentAccountNo", EmployeeBank.PermanentAccountNo);
                //param.Add("@p_PermanentConfirmAccountNo", EmployeeBank.PermanentConfirmAccountNo);
                //param.Add("@p_PermanentBankAddress", EmployeeBank.PermanentBankAddress);
                //param.Add("@p_PermanentBankBranchName", EmployeeBank.PermanentBankBranchName);
                //param.Add("@p_PermanentNameAsPerBank", EmployeeBank.PermanentNameAsPerBank);
                param.Add("@p_SalaryMode", EmployeeBank.SalaryMode);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_Bank]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_WorkerBankDetails", "Module_Employee_WorkerDetails");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }




        #endregion

        #region Module_Employee_WorkerFamilyDetails
        public ActionResult Module_Employee_WorkerFamilyDetails()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                TempData["WorkerOnboardEmployeeName"] = Session["WorkerOnboardEmployeeName"];
                ViewBag.AddUpdateTitle = "Add";
                Mas_Employee_Family mas_employeeFamily = new Mas_Employee_Family();
                param.Add("@query", "Select RelationId,RelationName from Mas_Relation Where Deactivate=0");
                var List_Relation = DapperORM.ReturnList<Mas_Relation>("sp_QueryExcution", param).ToList();
                ViewBag.GetRelationName = List_Relation;

                param = new DynamicParameters();
                param.Add("@p_FamilyEmployeeId", Session["WorkerOnboardEmployeeId"]);
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Mas_Employee_Family", param).ToList();
                ViewBag.GetEmployeeFamilyList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region IsValidation
        [HttpGet]
        public ActionResult IsFamilyExists(string FamilyId_Encrypted, string AadharNo)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "IsValidation");
                param.Add("@p_FamilyId_Encrypted", FamilyId_Encrypted);
                param.Add("@p_AadharNo", AadharNo);

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_Family", param);
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
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveWorkerFamily(Mas_Employee_Family Employeefamily)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var WorkerEmployeeId = Session["WorkerOnboardEmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(Employeefamily.FamilyId_Encrypted) ? "Save" : "Update");
                param.Add("@p_FamilyId", Employeefamily.FamilyId);
                param.Add("@p_FamilyId_Encrypted", Employeefamily.FamilyId_Encrypted);
                param.Add("@p_FamilyEmployeeId", WorkerEmployeeId);
                param.Add("@p_MemberName", Employeefamily.MemberName);
                param.Add("@p_Relation", Employeefamily.Relation);
                param.Add("@p_DOB", Employeefamily.DOB);
                param.Add("@p_AadharNo", Employeefamily.AadharNo);
                param.Add("@p_ESIC_InsuranceType", Employeefamily.ESIC_InsuranceType);
                param.Add("@p_Member_Residing", Employeefamily.Member_Residing);
                param.Add("@p_TownName", Employeefamily.TownName);
                param.Add("@p_StateName", Employeefamily.StateName);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_Family]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_WorkerFamilyDetails", "Module_Employee_WorkerDetails");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        [HttpGet]
        public ActionResult Delete(string FamilyId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_FamilyId_Encrypted", FamilyId_Encrypted);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parame
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_Family", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_WorkerFamilyDetails", "Module_Employee_WorkerDetails");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
        #endregion

        #region Module_Employee_WorkerDocumentsDetails
        public ActionResult Module_Employee_WorkerDocumentsDetails()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                Mas_Employee_UploadDocument EmployeeDocument = new Mas_Employee_UploadDocument();
                param.Add("@query", "Select DocumentId as Id,DocumentName as Name from Mas_Document Where Deactivate=0");
                var DocumentType = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetDocumentType = DocumentType;

                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_DocumentEmployeeId", Session["WorkerOnboardEmployeeId"]);
                var EmployeeDocumentList = DapperORM.ExecuteSP<dynamic>("sp_List_Mas_Employee_UploadDocument", paramList).ToList();
                ViewBag.EmployeeUploadDocumentList = EmployeeDocumentList;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region SaveUpdate
        public ActionResult SaveWorkerUploadDocument(Mas_Employee_UploadDocument EmpDocument, HttpPostedFileBase EmployeeFrontPage, HttpPostedFileBase EmployeeBackPage)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                //For Front Page Insert
                if (EmployeeFrontPage != null)
                {
                    param.Add("@p_process", string.IsNullOrEmpty(EmpDocument.EmployeeUploadDocumentID_Encrypted) ? "Save" : "Update");
                    param.Add("@p_EmployeeUploadDocumentID", EmpDocument.EmployeeUploadDocumentID);
                    param.Add("@p_EmployeeUploadDocumentID_Encrypted", EmpDocument.EmployeeUploadDocumentID_Encrypted);
                    param.Add("@p_DocumentEmployeeId", Session["WorkerOnboardEmployeeId"]);
                    param.Add("@p_UploadDocumentName", "Front Page");
                    param.Add("@p_DocumentID", EmpDocument.DocumentID);
                    if (EmpDocument.EmployeeUploadDocumentID_Encrypted != null && EmployeeFrontPage == null)
                    {
                        param.Add("@p_DocumentPath", Session["SelectedFile"]);
                    }
                    else
                    {
                        param.Add("@p_DocumentPath", EmployeeFrontPage == null ? "" : EmployeeFrontPage.FileName);
                    }
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                                                                                                                 // param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_UploadDocument]", param);
                    var message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    TempData["Message"] = message;
                    TempData["Icon"] = Icon.ToString();
                    var OnboardEmployeeId = Session["WorkerOnboardEmployeeId"];
                    if (OnboardEmployeeId != null && EmpDocument.EmployeeUploadDocumentID_Encrypted != null || EmployeeFrontPage != null)
                    {
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='EmployeeUploadDocument'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        //var FirstPath = GetFirstPath +"Event"+"\\";
                        var FirstPath = GetFirstPath + OnboardEmployeeId + "\\" + "Documents" + "\\";// First path plus concat folder by Id
                        if (!Directory.Exists(FirstPath))
                        {
                            Directory.CreateDirectory(FirstPath);
                        }

                        if (EmployeeFrontPage != null)
                        {
                            string ImgUploadFrontPageFilePath = "";
                            ImgUploadFrontPageFilePath = FirstPath + EmployeeFrontPage.FileName; //Concat Full Path and create New full Path
                            EmployeeFrontPage.SaveAs(ImgUploadFrontPageFilePath); // This is use for Save image in folder full path
                        }
                    }
                }
                //For Back Page Insert
                if (EmployeeBackPage != null)
                {
                    param.Add("@p_process", string.IsNullOrEmpty(EmpDocument.EmployeeUploadDocumentID_Encrypted) ? "Save" : "Update");
                    param.Add("@p_EmployeeUploadDocumentID", EmpDocument.EmployeeUploadDocumentID);
                    param.Add("@p_EmployeeUploadDocumentID_Encrypted", EmpDocument.EmployeeUploadDocumentID_Encrypted);
                    param.Add("@p_DocumentEmployeeId", Session["WorkerOnboardEmployeeId"]);
                    param.Add("@p_UploadDocumentName", "Back Page");
                    param.Add("@p_DocumentID", EmpDocument.DocumentID);
                    if (EmpDocument.EmployeeUploadDocumentID_Encrypted != null && EmployeeBackPage == null)
                    {
                        param.Add("@p_DocumentPath", Session["SelectedFile"]);
                    }
                    else
                    {
                        param.Add("@p_DocumentPath", EmployeeBackPage == null ? "" : EmployeeBackPage.FileName);
                    }
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                                                                                                                 // param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_UploadDocument]", param);
                    var message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    TempData["Message"] = message;
                    TempData["Icon"] = Icon.ToString();
                    var OnboardEmployeeId = Session["WorkerOnboardEmployeeId"];
                    if (OnboardEmployeeId != null && EmpDocument.EmployeeUploadDocumentID_Encrypted != null || EmployeeBackPage != null)
                    {
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='EmployeeUploadDocument'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        //var FirstPath = GetFirstPath +"Event"+"\\";
                        var FirstPath = GetFirstPath + OnboardEmployeeId + "\\" + "Documents" + "\\";// First path plus concat folder by Id
                        if (!Directory.Exists(FirstPath))
                        {
                            Directory.CreateDirectory(FirstPath);
                        }

                        if (EmployeeBackPage != null)
                        {
                            string ImgUploadBackPageFilePath = "";
                            ImgUploadBackPageFilePath = FirstPath + EmployeeBackPage.FileName; //Concat Full Path and create New full Path
                            EmployeeBackPage.SaveAs(ImgUploadBackPageFilePath); // This is use for Save image in folder full path
                        }
                    }
                }

                return RedirectToAction("Module_Employee_WorkerDocumentsDetails", "Module_Employee_WorkerDetails");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetDocumentBackPage
        [HttpGet]
        public ActionResult GetDocumentBackPage(int? DocumentId)
        {
            try
            {
                var IsBackPage = DapperORM.DynamicQuerySingle("Select IsBackPage from Mas_Document where Deactivate=0 and DocumentId=" + DocumentId + "");
                return Json(IsBackPage, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region MyRegion
        public ActionResult DocumentDelete(string EmployeeUploadDocumentID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_EmployeeUploadDocumentID_Encrypted", EmployeeUploadDocumentID_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_UploadDocument", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();

                return RedirectToAction("Module_Employee_WorkerDocumentsDetails", "Module_Employee_WorkerDetails");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Download Image 
        public ActionResult DownloadFile(string UploadDocumentFile)
        {
            try
            {
                if (UploadDocumentFile != null)
                {
                    System.IO.File.ReadAllBytes(UploadDocumentFile);
                    return File(UploadDocumentFile, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(UploadDocumentFile));
                }
                else
                {
                    return RedirectToAction("Module_Employee_WorkerDocumentsDetails", "Module_Employee_WorkerDetails", new { Area = "Module" });
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        #endregion
        #endregion


        #region Module_Employee_WorkerESSDetails
        public ActionResult Module_Employee_WorkerESSDetails()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                TempData["WorkerOnboardEmployeeName"] = Session["WorkerOnboardEmployeeName"];

                Mas_Employee_ESS Mas_employeeESS = new Mas_Employee_ESS();
                param = new DynamicParameters();
                param.Add("@p_ESSEmployeeId", Session["WorkerOnboardEmployeeId"]);
                Mas_employeeESS = DapperORM.ReturnList<Mas_Employee_ESS>("sp_List_Mas_Employee_ESS", param).FirstOrDefault();
                if (Mas_employeeESS != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                }

                //DynamicParameters param3 = new DynamicParameters();
                //param3.Add("@query", "select UserGroupId as Id , UserGroupName as Name  from Tool_UserAccessPolicyMaster  where Deactivate=0 order by UserGroupName");
                //var UserGroupPolicy = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param3).ToList();
                //ViewBag.GroupName = UserGroupPolicy;

                DynamicParameters param3 = new DynamicParameters();
                param3.Add("@p_EmployeeId", Session["EmployeeId"]);
                param3.Add("@p_OnboardEmployeeid", Session["WorkerOnboardEmployeeId"]);
                var UserGroupPolicy = DapperORM.ReturnList<AllDropDownBind>("sp_GetUserPolicyMappedDropdown", param3).ToList();
                ViewBag.GroupName = UserGroupPolicy;



                var DesinationName = Session["WorkerOnboardEmpDesignation"];
                param = new DynamicParameters();
                param.Add("@query", "select DesignationId as Id from Mas_Designation where Deactivate=0 and DesignationName='" + DesinationName + "'");
                var GetDesignationId = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).FirstOrDefault();

                param = new DynamicParameters();
                param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and ContractorId=1 and employeeid in (select distinct UserRightsEmployeeId from tool_userrights where deactivate = 0) and EmployeeDesignationID = '" + GetDesignationId.Id + "'  and employeeid<> '" + Session["WorkerOnboardEmployeeId"] + "'");
                var GetFromEmployee = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetMasEmployee = GetFromEmployee;
                ViewBag.count = GetFromEmployee.Count();

                param = new DynamicParameters();
                param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and ContractorId=1 and employeeid not in (select distinct UserRightsEmployeeId from tool_userrights where deactivate = 0 and UserRightsEmployeeId ='" + Session["WorkerOnboardEmployeeId"] + "') and EmployeeId ='" + Session["WorkerOnboardEmployeeId"] + "'");
                var GetToEmployee = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).FirstOrDefault();
                if (GetToEmployee != null)
                {
                    var Name = GetToEmployee.Name;
                    ViewBag.GetToEmployee = Name;
                }
                else
                {
                    ViewBag.GetToEmployee = null;
                }
                if (GetToEmployee != null || GetToEmployee != "")
                {
                    ViewBag.countToUser = 1;
                }
                else
                {
                    ViewBag.countToUser = 0;
                }

                return View(Mas_employeeESS);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #region IsVerification
        [HttpGet]
        public ActionResult IsESSLoginExists(string ESSId_Encrypted, string ESSLoginID)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_ESSId_Encrypted", ESSId_Encrypted);
                param.Add("@p_ESSLoginID", ESSLoginID);
                param.Add("@p_ESSEmployeeId", Session["WorkerOnboardEmployeeId"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_ESS", param);
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
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdateESS(Mas_Employee_ESS EmployeeESS)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(EmployeeESS.ESSId_Encrypted) ? "Save" : "Update");
                param.Add("@p_ESSId", EmployeeESS.ESSId);
                param.Add("@p_ESSId_Encrypted", EmployeeESS.ESSId_Encrypted);
                param.Add("@p_ESSEmployeeId", Session["WorkerOnboardEmployeeId"]);
                param.Add("@p_ESSLoginID", EmployeeESS.ESSLoginID);
                param.Add("@p_ESSPassword", EmployeeESS.ESSPassword);
                param.Add("@p_ESSSecurityQuestion", EmployeeESS.ESSSecurityQuestion);
                param.Add("@p_ESSAnswer", EmployeeESS.ESSAnswer);
                param.Add("@p_IsAdmin", EmployeeESS.IsAdmin);
                param.Add("@p_ESSIsActive", EmployeeESS.ESSIsActive);
                param.Add("@p_ESSIsLock", EmployeeESS.ESSIsLock);
                param.Add("@p_UserAccessPolicyId", EmployeeESS.UserAccessPolicyId);
                param.Add("@p_ESSLoginAttemptCount", EmployeeESS.ESSLoginAttemptCount);
                param.Add("@p_IsExit", EmployeeESS.IsExit);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_ESS]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_WorkerESSDetails", "Module_Employee_WorkerDetails");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion



        #endregion

        #region Partial View 
        [HttpGet]
        public PartialViewResult WorkerDetails_SidebarMenu()
        {
            try
            {

                var WorkerEmployeeId = Session["WorkerOnboardEmployeeId"];
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EmployeeId", WorkerEmployeeId);
                var StatusCheck = DapperORM.DynamicList("sp_List_Mas_Employee_StatusCheck", paramList);
                ViewBag.GetStatusCheckList = StatusCheck;

                var id = Session["ModuleId"];
                var ScreenId = Session["ScreenId"];
                var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), Convert.ToInt32(id), Convert.ToInt32(ScreenId), "SubForm", "Transation");
                ViewBag.GetUserMenuList = GetMenuList;

                return PartialView("_WorkerDetails_SidebarMenu");
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}