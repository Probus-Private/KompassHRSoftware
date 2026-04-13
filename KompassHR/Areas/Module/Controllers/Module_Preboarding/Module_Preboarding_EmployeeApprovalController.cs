using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Module.Models.Module_Preboarding;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Preboarding
{
    public class Module_Preboarding_EmployeeApprovalController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Module/Module_Preboarding_EmployeeApproval
        #region EmployeeApproval Main View
            [HttpGet]
        public ActionResult Module_Preboarding_EmployeeApproval(int? Fid,int? CmpId, string FID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }


                Session["FID_Encrypted"] = FID_Encrypted;
                Session["Fid"] = Fid;
                Session["CmpId"] = CmpId;
                Preboarding_Mas_Employee ForApprovalList = new Preboarding_Mas_Employee();
                var ResultList = DapperORM.DynamicQueryMultiple(@"
                    SELECT ReligionId AS Id, ReligionName AS Name FROM Mas_Religion WHERE Deactivate = 0 ORDER BY ReligionName;
                    SELECT CasteID AS Id, CasteName AS Name FROM Mas_Caste WHERE Deactivate = 0 ORDER BY CasteName;
                    SELECT QualificationPFId AS Id, QualificationPFName AS Name FROM Mas_Qualification_PF WHERE Deactivate = 0 ORDER BY QualificationPFName;
                    SELECT VerificationId AS Id, VerificationName AS Name FROM Mas_Verification WHERE Deactivate = 0 ORDER BY VerificationName;
                    SELECT DocumentId AS Id, DocumentName AS Name FROM Mas_Document WHERE Deactivate = 0 ORDER BY DocumentName;
                    SELECT RelationId AS Id, RelationName AS Name FROM Mas_Relation WHERE Deactivate = 0 ORDER BY RelationName;
                ");

                ViewBag.GetReligionName = ResultList[0]
                    .Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                ViewBag.GetCastName = ResultList[1]
                    .Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                ViewBag.GetQualificationName = ResultList[2]
                    .Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                ViewBag.GetVerificationName = ResultList[3]
                    .Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                ViewBag.GetDocumentName = ResultList[4]
                    .Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                ViewBag.GetRelativeName = ResultList[5]
                    .Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                //var ResultList = DapperORM.DynamicQueryMultiple(@"Select ReligionId as Id,ReligionName as Name from Mas_Religion Where Deactivate=0 order by ReligionName;
                //                                       Select CasteID As Id,CasteName AS Name from Mas_Caste Where Deactivate=0 order by CasteName;
                //                                       Select QualificationPFId as Id,QualificationPFName as Name from Mas_Qualification_PF Where Deactivate=0 order by QualificationPFName;
                //                                       Select VerificationId As Id,VerificationName As Name from Mas_Verification Where Deactivate=0 order by VerificationName;
                //                                       Select DocumentId As Id,DocumentName As Name from Mas_Document Where Deactivate=0 order by DocumentName;
                //                                       Select RelationId As Id,RelationName As Name from Mas_Relation Where Deactivate=0 order by RelationName");

                //ViewBag.GetReligionName = ResultList.Read<AllDropDownClass>().ToList();
                //ViewBag.GetCastName = ResultList.Read<AllDropDownClass>().ToList();
                //ViewBag.GetQualificationName = ResultList.Read<AllDropDownClass>().ToList();
                //ViewBag.GetVerificationName = ResultList.Read<AllDropDownClass>().ToList();
                //ViewBag.GetDocumentName = ResultList.Read<AllDropDownClass>().ToList();
                //ViewBag.GetRelativeName = ResultList.Read<AllDropDownClass>().ToList();              

                param = new DynamicParameters();
                param.Add("@p_Fid", Fid);
                ForApprovalList = DapperORM.ReturnList<Preboarding_Mas_Employee>("sp_List_Preboarding_CandidateList_ForApproval", param).FirstOrDefault();

                param = new DynamicParameters();
                param.Add("@p_PreboardingQualificationFid", Fid);
                var ForApprovalQualificationList = DapperORM.ExecuteSP<dynamic>("sp_List_Preboarding_Mas_EmployeeQualification", param).ToList();
                ViewBag.QualificationList = ForApprovalQualificationList;

                param = new DynamicParameters();
                param.Add("@p_FamilyPreboardingFid", Fid);
                var ForApprovalFamilyList = DapperORM.ExecuteSP<dynamic>("sp_List_Preboarding_Mas_EmployeeFamily", param).ToList();
                ViewBag.FamilyList = ForApprovalFamilyList;

                param = new DynamicParameters();
                param.Add("@p_ExperiencePreboardingFid", Fid);
                var ForApprovalExperienceList = DapperORM.ExecuteSP<dynamic>("sp_List_Preboarding_Mas_EmployeeExperience", param).ToList();
                ViewBag.ExperienceList = ForApprovalExperienceList;

                param = new DynamicParameters();
                param.Add("@p_SkillPreboardingFid", Fid);
                var ForApprovalSkillList = DapperORM.ExecuteSP<dynamic>("sp_List_Preboarding_Mas_EmployeeSkill", param).ToList();
                ViewBag.SkillList = ForApprovalSkillList;

                param = new DynamicParameters();
                param.Add("@p_LanguagePreboardingFid", Fid);
                var ForApprovalLanguageList = DapperORM.ExecuteSP<dynamic>("sp_List_Preboarding_Mas_EmployeeLanguage", param).ToList();
                ViewBag.LanguageList = ForApprovalLanguageList;

                param = new DynamicParameters();
                param.Add("@p_DocumentPreboardingFid", Fid);
                var ForApprovalUploadDocumentList = DapperORM.ExecuteSP<dynamic>("sp_List_Preboarding_Mas_Employee_UploadDocuments", param).ToList();
                ViewBag.UploadDocumentList = ForApprovalUploadDocumentList; 

                 TempData["BirthdayDate"] = ForApprovalList.BirthdayDate;
                TempData["AnniversryDate"] = ForApprovalList.AnniversryDate;
                TempData["PassportExpiryDate"] = ForApprovalList.PassportExpiryDate;
                TempData["EM_PF_ExitDate"] = ForApprovalList.EM_PF_ExitDate;
                TempData["EM_Nom_DOB"] = ForApprovalList.EM_Nom_DOB;
                TempData["DrivingExpiryDate"] = ForApprovalList.DrivingExpiryDate;

                var GetRejectedRemark = DapperORM.DynamicQuerySingle("Select Verify_Personal_Status,Verify_Address_Status,Verify_References_Status,Verify_Bank_Status,Verify_Family_Status,Verify_Qualification_Status,Verify_Document_Status,Verify_Photo_Status,Verify_Signature_Status,Verify_PreEmployer_Status,Verify_Skill_Status,Verify_Language_Status,Verify_Other_Status,Verify_Personal_RejectRemark,Verify_Address_RejectRemark,Verify_References_RejectRemark,Verify_Bank_RejectRemark,Verify_Family_RejectRemark,Verify_Qualification_RejectRemark,Verify_Document_RejectRemark,Verify_Photo_RejectRemark,Verify_Signature_RejectRemark,Verify_PreEmployer_RejectRemark,Verify_Skill_RejectRemark,Verify_Language_RejectRemark,verify_Other_RejectRemark,Verify_Statutory_Status,Verify_Statutory_RejectRemark from Preboarding_Mas_Employee where FID= " + Fid + "");

                ViewBag.RejectedRemark = GetRejectedRemark;



                //var AddharCard=DapperORM.DynamicQuerySingle("")
                //Upload Photo ====================================================================================================================================>>
                var GetUploadPhoto = "Select Photo from Preboarding_Mas_Employee Where Fid= " + Fid + "";
                var SecondPathPhoto = "";
                var pathPhoto = DapperORM.DynamicQuerySingle(GetUploadPhoto);

                if (pathPhoto.Photo != null)
                {
                    SecondPathPhoto = pathPhoto.Photo;
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Preboarding'");
                    var FisrtPathPhoto = GetDocPath.DocInitialPath + Fid + "\\" + "Photo" + "\\";

                    string fullPath = "";
                    fullPath = FisrtPathPhoto + SecondPathPhoto;
                    if (SecondPathPhoto != null)
                    {
                        TempData["Photo"] = fullPath;
                    }
                    else
                    {
                        TempData["Photo"] = "";
                    }

                    if (fullPath != null)
                    {
                        if (!Directory.Exists(fullPath))
                        {
                            try
                            {
                                using (Image image = Image.FromFile(fullPath))
                                {
                                    using (MemoryStream m = new MemoryStream())
                                    {
                                        image.Save(m, image.RawFormat);
                                        byte[] imageBytes = m.ToArray();

                                        // Convert byte[] to Base64 String
                                        string base64String = Convert.ToBase64String(imageBytes);
                                        ViewBag.UploadPhoto = "data:image; base64," + base64String;
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                ViewBag.UploadPhoto = "";
                            }
                          
                        }
                    }
                }
                else
                {
                    ViewBag.UploadPhoto = "";
                }
                //Upload Signature=====================================================================================================================================================>>
                var GetUploadSignature = "Select UploadSignature from Preboarding_Mas_Employee Where Fid= " + Fid + "";
                var SecondPathSignature = "";
                var pathSignature = DapperORM.DynamicQuerySingle(GetUploadSignature);

                if (pathSignature.UploadSignature != null)
                {
                    SecondPathSignature = pathSignature.UploadSignature;
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Preboarding'");
                    var FisrtPathSignature = GetDocPath.DocInitialPath + Fid + "\\" + "Signature" + "\\";

                    string fullPathSignature = "";
                    fullPathSignature = FisrtPathSignature + SecondPathSignature;
                    if (SecondPathSignature != null)
                    {
                        TempData["Signature"] = fullPathSignature;
                    }
                    else
                    {
                        TempData["Signature"] = "";
                    }


                    if (fullPathSignature != null)
                    {
                        if (!Directory.Exists(fullPathSignature))
                        {
                            try
                            {
                                using (Image image = Image.FromFile(fullPathSignature))
                                {
                                    using (MemoryStream m = new MemoryStream())
                                    {
                                        image.Save(m, image.RawFormat);
                                        byte[] imageBytes = m.ToArray();

                                        // Convert byte[] to Base64 String
                                        string base64String = Convert.ToBase64String(imageBytes);
                                        ViewBag.UploadSignature = "data:image; base64," + base64String;
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                ViewBag.UploadSignature = "";
                            }
                        }
                    }
                }
                else
                {
                    ViewBag.UploadSignature = "";
                }

                if (ForApprovalList!=null)
                {
                    TempData["EmployeeName"] = ForApprovalList.EmployeeName;
                    TempData["DesignationName"] = ForApprovalList.DesignationName;
                    TempData["DepartmentName"] = ForApprovalList.DepartmentName;
                    TempData["JoiningDate"] = ForApprovalList.JoiningDate;
                }
                else
                {
                    TempData["EmployeeName"] = "";
                    TempData["DesignationName"] = "";
                    TempData["DepartmentName"] = "";
                    TempData["JoiningDate"] = "";
                }
                ViewBag.ApprovedRejected = ForApprovalList;
                return View(ForApprovalList);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
     

        #region IsValidation And IsApprovedOrRejectedCheck
        [HttpGet]
        public ActionResult IsApprovedOrRejectedCheck(string FID_Encrypted, string Status, string Remark,string Origin,int? EmailFID)
        {
            try
            {                
                param.Add("@p_Origin", Origin);
                param.Add("@p_Preboarding_Mas_Employee_FID_Encrypted", FID_Encrypted);
                param.Add("@p_Status", Status);
                param.Add("@p_RejectRemarks", Remark);
                param.Add("@p_EmployeeID", Session["EmployeeId"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_Preboarding_Validation_Approval", param);
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

        //    1 Preboarding Link
        //    2 Candidate Login Alert
        //    3 Candidate Final Submit
        //    4 Candidate Partial Rejection
        //    5 Candidate Fully Rejected
        //    6 Candidate Approved
        //    7 Candidate Joining Notification
        //    8 Preboarding Link Reopen
        //    9 Preboarding Link Expiry
        //    10 Candidate Cancel


        #region IsValidation And FinalApproval 
        [HttpGet]
        public ActionResult IsFinalApprovalCheck(string FID_Encrypted, string Status,string CmpId, string Remark,int? EmailFID)
        {
            try
            {    
                   
                param.Add("@p_Preboarding_Mas_Employee_FID_Encrypted", FID_Encrypted);
                param.Add("@p_Status", Status);
                param.Add("@p_Preboarding_CmpId", CmpId);
                param.Add("@p_EmployeeID", Session["EmployeeId"]);
                param.Add("@p_RejectRemarks", Remark);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_Preboarding_Final_Approval", param);
                var Message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                if(Icon== "success")
                {
                    if (Status == "Rejected")
                    {
                        if (EmailFID != null)
                        {
                            Session["PID"] = EmailFID;
                            var GetcmpId = DapperORM.DynamicQuerySingle("Select CmpID From Preboarding_Mas_employee where Deactivate = 0 and Fid='" + Session["PID"] + "'");
                            var CmpIdS = GetcmpId.CmpID;
                            clsEmail email = new clsEmail();
                            email.SendMail(int.Parse(Session["PID"].ToString()), Convert.ToInt16(CmpIdS), "1", "5");
                        }
                    }
                    else
                    {
                        if (EmailFID != null)
                        {
                            Session["PID"] = EmailFID;
                            var GetcmpId = DapperORM.DynamicQuerySingle("Select CmpID From Preboarding_Mas_employee where Deactivate = 0 and Fid='" + Session["PID"] + "'");
                            var CmpIdS = GetcmpId.CmpID;
                            clsEmail email = new clsEmail();
                            email.SendMail(int.Parse(Session["PID"].ToString()), Convert.ToInt16(CmpIdS), "1", "6");
                        }
                    }
                }
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

        #region Download Image 
        public ActionResult UploadDocumentDownloadFile(string UploadDocument)
        {
            try
            {
                //var GetDrivePath = (from d in dbContext.Onboarding_Mas_DocumentPath select d.DosumentPath).FirstOrDefault();
                //var fullPath = GetDrivePath + filePath;
                //byte[] fileBytes = GetFile(fullPath);
                if (UploadDocument != null)
                {
                    System.IO.File.ReadAllBytes(UploadDocument);
                    return File(UploadDocument, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(UploadDocument));
                }
                else
                {
                    return RedirectToAction("Module_Preboarding_EmployeeApproval", "Module_Preboarding_EmployeeApproval", new { Area = "Module n" });
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion


        #region SendMail
        public ActionResult SendMail(int? EmailFID)
        {
            try
            {
                //    1 Preboarding Link
                //    2 Candidate Login Alert
                //    3 Candidate Final Submit
                //    4 Candidate Partial Rejection
                //    5 Candidate Fully Rejected
                //    6 Candidate Approved
                //    7 Candidate Joining Notification
                //    8 Preboarding Link Reopen
                //    9 Preboarding Link Expiry
                //    10 Candidate Cancels
                if (EmailFID != null)
                {
                    Session["PID"] = EmailFID;
                    var GetcmpId = DapperORM.DynamicQuerySingle("Select CmpID From Preboarding_Mas_employee where Deactivate = 0 and Fid='" + Session["PID"] + "'");
                    var CmpId = GetcmpId.CmpID;
                    clsEmail email = new clsEmail();
                    email.SendMail(int.Parse(Session["PID"].ToString()), Convert.ToInt16(CmpId), "1", "4");
                }

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region SaveUpdateAddressInfo
        [HttpPost]
        public ActionResult SaveUpdateAddressInfo(Preboarding_Mas_Employee Employeeaddress)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DapperORM.DynamicQuerySingle("Update Preboarding_Mas_Employee set  PresentPin ='" + Employeeaddress.PresentPin + "' ,PresentState ='" + Employeeaddress.PresentState + "' ,PresentDistrict ='" + Employeeaddress.PresentDistrict + "',PresentTaluk ='" + Employeeaddress.PresentTaluk + "' ,PresentPO ='" + Employeeaddress.PresentPO + "',PresentCity ='" + Employeeaddress.PresentCity + "' ,PresentOther ='" + Employeeaddress.PresentOther + "' ,PermanentPin='" + Employeeaddress.PermanentPin + "',PermanentState='" + Employeeaddress.PermanentState + "' ,PermanentDistrict ='" + Employeeaddress.PermanentDistrict + "' ,PermanentTaluk ='" + Employeeaddress.PermanentTaluk + "',PermanentPO='" + Employeeaddress.PermanentPO + "' ,PermanentCity='" + Employeeaddress.PermanentCity + "' ,PermanentOther ='" + Employeeaddress.PermanentOther + "' where Fid =" + Employeeaddress.FID + "");
                TempData["Message"] = "Record save successfully";
                TempData["Icon"] = "success";
                return RedirectToAction("Module_Preboarding_EmployeeApproval", "Module_Preboarding_EmployeeApproval",new { FID_Encrypted = Session["FID_Encrypted"], Fid=Session["Fid"], CmpId=Session["CmpId"] });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region SaveUpdateReferencesInfo
        [HttpPost]
        public ActionResult SaveUpdateReferencesInfo(Preboarding_Mas_Employee Employeeaddress)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DapperORM.DynamicQuerySingle("update Preboarding_Mas_Employee set  NeighboursName1 ='"+ Employeeaddress.NeighboursName1 + "',NeighboursMobile1 = ='" + Employeeaddress.NeighboursMobile1 + "',NeighboursName2  ='" + Employeeaddress.NeighboursName2 + "',NeighboursMobile2  ='" + Employeeaddress.NeighboursMobile2 + "',RelativeName  ='" + Employeeaddress.RelativeName + "',RelativeRelation  ='" + Employeeaddress.RelativeRelation + "',RelativeMobile ='" + Employeeaddress.RelativeMobile + "',EmergencyName1 ='" + Employeeaddress.EmergencyName2 + "',EmergencyRelation1  ='" + Employeeaddress.EmergencyRelation1 + "',EmergencyMobile1  ='" + Employeeaddress.EmergencyMobile1 + "',EmergencyName2 ='" + Employeeaddress.EmergencyName2 + "',EmergencyRelation2  ='" + Employeeaddress.EmergencyRelation2 + "',EmergencyMobile2  ='" + Employeeaddress.EmergencyMobile2 + "',CompanyRefName1  ='" + Employeeaddress.CompanyRefName1 + "',CompanyRefMobile1  ='" + Employeeaddress.CompanyRefMobile1 + "',CompanyRefName2  ='" + Employeeaddress.CompanyRefName2 + "',CompanyRefMobile2  ='" + Employeeaddress.CompanyRefMobile2 + "' where Fid =" + Employeeaddress.FID + "");
                TempData["Message"] = "Record save successfully";
                TempData["Icon"] = "success";
                return RedirectToAction("Module_Preboarding_EmployeeApproval", "Module_Preboarding_EmployeeApproval", new { FID_Encrypted = Session["FID_Encrypted"], Fid = Session["Fid"], CmpId = Session["CmpId"] });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region SaveUpdateBankInfo
        [HttpPost]
        public ActionResult SaveUpdateBankInfo(Preboarding_Mas_Employee Employeeaddress)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DapperORM.DynamicQuerySingle("update Preboarding_Mas_Employee set IFSCCode ='"+ Employeeaddress.IFSCCode + "',BankName ='" + Employeeaddress.IFSCCode + "',AccountNo ='" + Employeeaddress.AccountNo + "',BankDetail ='" + Employeeaddress.BankDetail + "',BankBranchName ='" + Employeeaddress.BankBranchName + "',NameAsPerBank ='" + Employeeaddress.NameAsPerBank + "' where  Fid =" + Employeeaddress.FID + "");
                TempData["Message"] = "Record save successfully";
                TempData["Icon"] = "success";
                return RedirectToAction("Module_Preboarding_EmployeeApproval", "Module_Preboarding_EmployeeApproval", new { FID_Encrypted = Session["FID_Encrypted"], Fid = Session["Fid"], CmpId = Session["CmpId"] });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region SaveStatutoryInfo
        [HttpPost]
        public ActionResult SaveStatutoryInfo(Preboarding_Mas_Employee Employeeaddress)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DapperORM.DynamicQuerySingle("update Preboarding_Mas_Employee set ESIC_Insurance ='" + Employeeaddress.ESIC_Insurance + "',EM_ESI_Applicable ='" + Employeeaddress.EM_ESI_Applicable + "',EM_ESINO ='" + Employeeaddress.EM_ESINO + "',EM_PF_Applicable ='" + Employeeaddress.EM_PF_Applicable + "',EM_PF_Limit ='" + Employeeaddress.EM_PF_Limit + "',EM_PF_EPS ='" + Employeeaddress.EM_PF_EPS + "',EM_PF_MobileNo ='" + Employeeaddress.EM_PF_MobileNo + "',EM_PF_BankName ='" + Employeeaddress.EM_PF_BankName + "',EM_PF_BankIFSC ='" + Employeeaddress.EM_PF_BankIFSC + "',EM_PF_Account ='" + Employeeaddress.EM_PF_Account + "',EM_PF_ExitDate ='" + Employeeaddress.EM_PF_ExitDate + "',EM_PF_CertificateNo ='" + Employeeaddress.EM_PF_CertificateNo + "',EM_PF_PPO ='" + Employeeaddress.EM_PF_PPO + "',EM_PF_Opting ='" + Employeeaddress.EM_PF_Opting + "',EM_PF_VPF ='" + Employeeaddress.EM_PF_VPF + "',EM_FSType ='" + Employeeaddress.EM_FSType + "',EM_FS_Name ='" + Employeeaddress.EM_FS_Name + "',EM_PF_UAN ='" + Employeeaddress.EM_PF_UAN + "',EM_PF_NO ='" + Employeeaddress.EM_PF_NO + "',EM_PF_RELATION ='" + Employeeaddress.EM_PF_RELATION + "',EM_FNAME ='" + Employeeaddress.EM_FNAME + "',EM_Nom_DOB ='" + Employeeaddress.EM_Nom_DOB + "',EM_PF_Share ='" + Employeeaddress.EM_PF_Share + "',EM_PF_Address ='" + Employeeaddress.EM_PF_Address + "',EM_OldUANNo ='" + Employeeaddress.EM_OldUANNo + "',EM_LinkWithUAN ='" + Employeeaddress.EM_LinkWithUAN + "',EM_OldESICNo ='" + Employeeaddress.EM_OldESICNo + "',EM_LinkWithESIC ='" + Employeeaddress.EM_LinkWithESIC + "',EM_PF_RELETION2 ='" + Employeeaddress.EM_PF_RELETION2 + "',EM_RelationNAME2 ='" + Employeeaddress.EM_RelationNAME2 + "',EM_Nom_DOB2 ='" + Employeeaddress.EM_Nom_DOB2 + "',EM_PF_Share2 ='" + Employeeaddress.EM_PF_Share2 + "',EM_PF_Address2 ='" + Employeeaddress.EM_PF_Address2 + "',EM_GuardianName2 ='" + Employeeaddress.EM_GuardianName2 + "',EM_PF_RELETION3 ='" + Employeeaddress.EM_PF_RELETION3 + "',EM_RelationNAME3 ='" + Employeeaddress.EM_RelationNAME3 + "',EM_Nom_DOB3 ='" + Employeeaddress.EM_Nom_DOB3 + "',EM_PF_Share3 ='" + Employeeaddress.EM_PF_Share3 + "',EM_PF_Address3 ='" + Employeeaddress.EM_PF_Address3 + "',EM_GuardianName3 ='" + Employeeaddress.EM_GuardianName3 + "' where   Fid =" + Employeeaddress.FID + "");
                TempData["Message"] = "Record save successfully";
                TempData["Icon"] = "success";
                return RedirectToAction("Module_Preboarding_EmployeeApproval", "Module_Preboarding_EmployeeApproval", new { FID_Encrypted = Session["FID_Encrypted"], Fid = Session["Fid"], CmpId = Session["CmpId"] });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region SaveUpdatePersonalInfo
        [HttpPost]
        public ActionResult SaveUpdatePersonalInfo(Preboarding_Mas_Employee Employeeaddress)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DapperORM.DynamicQuerySingle("update Preboarding_Mas_Employee set BirthdayDate ='"+ Employeeaddress.BirthdayDate + "',BirthdayPlace ='"+ Employeeaddress.BirthdayPlace + "',BirthdayProofOfDocumentID ='"+ Employeeaddress.BirthdayProofOfDocumentID + "',BirthdayProofOfCertificateNo ='"+ Employeeaddress.BirthdayProofOfCertificateNo + "',Gender ='"+ Employeeaddress.Gender + "',GenderNotDisclose ='"+ Employeeaddress.GenderNotDisclose + "',BloodGroup ='"+ Employeeaddress.BloodGroup + "',MaritalStatus ='"+ Employeeaddress.MaritalStatus + "',AnniversryDate ='"+ Employeeaddress.AnniversryDate + "',EmployeeReligionID ='"+ Employeeaddress.EmployeeReligionID + "',EmployeeCasteID ='"+ Employeeaddress.EmployeeCasteID + "',CastNotDisclose ='"+ Employeeaddress.CastNotDisclose + "',EmployeeSubCategory ='"+ Employeeaddress.EmployeeSubCategory + "',PhysicalDisable ='"+ Employeeaddress.PhysicalDisable + "',PhysicalDisableType ='"+ Employeeaddress.PhysicalDisableType + "',PhysicalDisableRemark ='"+ Employeeaddress.PhysicalDisableRemark + "',IdentificationMark ='"+ Employeeaddress.IdentificationMark + "',EmployeeQualificationID ='"+ Employeeaddress.EmployeeQualificationID + "',QualificationRemark ='"+ Employeeaddress.QualificationRemark + "',Mobile1 ='"+ Employeeaddress.Mobile1 + "',Mobile2 ='"+ Employeeaddress.Mobile2 + "',WhatsAppNo ='"+ Employeeaddress.WhatsAppNo + "',PersonalEmailID ='"+ Employeeaddress.PersonalEmailID + "',EmpNRI ='"+ Employeeaddress.EmpNRI + "',DrivingLicenceNo ='"+ Employeeaddress.DrivingLicenceNo + "',DrivingExpiryDate ='"+ Employeeaddress.DrivingExpiryDate + "'  ,PanNo ='"+ Employeeaddress.PanNo + "',NameAsPerPan ='"+ Employeeaddress.NameAsPerPan + "',AadhaarNo ='"+ Employeeaddress.AadhaarNo + "',NameAsPerAadhaar ='"+ Employeeaddress.NameAsPerAadhaar + "',PANAadhaarLink ='"+ Employeeaddress.PANAadhaarLink + "',AadhaarNoMobileNoLink ='"+ Employeeaddress.AadhaarNoMobileNoLink + "',AadhaarNoMobileNo ='"+ Employeeaddress.AadhaarNoMobileNo + "'where  Fid =" + Employeeaddress.FID + "");
                TempData["Message"] = "Record save successfully";
                TempData["Icon"] = "success";
                return RedirectToAction("Module_Preboarding_EmployeeApproval", "Module_Preboarding_EmployeeApproval", new { FID_Encrypted = Session["FID_Encrypted"], Fid = Session["Fid"], CmpId = Session["CmpId"] });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region SaveUpdateOtherInfo
        [HttpPost]
        public ActionResult SaveUpdateOtherInfo(Preboarding_Mas_Employee Employeeaddress)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DapperORM.DynamicQuerySingle("update Preboarding_Mas_Employee set  Hobbies ='"+ Employeeaddress.Hobbies + "' ,Height ='"+ Employeeaddress.Height + "' ,Weight ='"+ Employeeaddress.Weight + "' ,MedDescription ='"+ Employeeaddress.MedDescription + "' ,HospitalAddress ='"+ Employeeaddress.HospitalAddress + "' ,PhysicianName ='"+ Employeeaddress.PhysicianName + "' ,HospitalContactNo ='"+ Employeeaddress.HospitalContactNo + "' ,NickName ='"+ Employeeaddress.NickName + "' ,FavouriteColor ='"+ Employeeaddress.FavouriteColor + "' ,FavouriteMovie ='"+ Employeeaddress.FavouriteMovie + "' ,FavouriteActor ='"+ Employeeaddress.FavouriteActor + "' ,FavouriteGame ='"+ Employeeaddress.FavouriteGame + "' ,FavouriteSong ='"+ Employeeaddress.FavouriteSong + "' ,FavouriteFood ='"+ Employeeaddress.FavouriteFood + "' ,FavouritePlace ='"+ Employeeaddress.FavouritePlace + "' ,FavouriteDreamCar ='"+ Employeeaddress.FavouriteDreamCar + "' ,AboutYourself ='"+ Employeeaddress.AboutYourself + "' ,Extracurricular ='"+ Employeeaddress.Extracurricular + "' ,EyeNo ='"+ Employeeaddress.EyeNo + "' ,LuckNo ='"+ Employeeaddress.LuckNo + "' where  Fid =" + Employeeaddress.FID + "");
                TempData["Message"] = "Record save successfully";
                TempData["Icon"] = "success";
                return RedirectToAction("Module_Preboarding_EmployeeApproval", "Module_Preboarding_EmployeeApproval", new { FID_Encrypted = Session["FID_Encrypted"], Fid = Session["Fid"], CmpId = Session["CmpId"] });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region SaveUpdateUploadPhotoInfo
        public ActionResult SaveUpdateUploadPhotoInfo(Preboarding_Mas_Employee Employeeaddress, HttpPostedFileBase EmployeePhoto)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DapperORM.DynamicQuerySingle("update Preboarding_Mas_Employee set  Photo='"+ EmployeePhoto.FileName+ "' where  Fid =" + Employeeaddress.FID + "");
                TempData["Message"] = "Record update successfully";
                TempData["Icon"] = "success";
                var OnboardEmployeeId = Session["OnboardEmployeeId"];
                //TempData["P_Id"] = param.Get<string>("@p_Id");
                if (EmployeePhoto != null)
                {
                    try
                    {
                        // Fetch the document path from the database
                        var GetPath = DapperORM.DynamicQuerySingle("SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin = 'Preboarding'");
                        var GetFirstPath = GetPath != null ? GetPath.DocInitialPath : null;
                        // Construct the full path
                        var FirstPath = Path.Combine(GetFirstPath, Employeeaddress.FID.ToString(), "Photo");

                        // Ensure the directory exists
                        if (!Directory.Exists(FirstPath))
                        {
                            Directory.CreateDirectory(FirstPath);
                        }

                        // Save the file if it exists
                        if (EmployeePhoto != null)
                        {
                            string ImgUploadPhotoFilePath = Path.Combine(FirstPath, EmployeePhoto.FileName);
                            EmployeePhoto.SaveAs(ImgUploadPhotoFilePath); // Save image to folder
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["Message"] = "Error: " + ex.Message;
                        TempData["Icon"] = "error";
                    }
                }

                return RedirectToAction("Module_Preboarding_EmployeeApproval", "Module_Preboarding_EmployeeApproval", new { FID_Encrypted = Session["FID_Encrypted"], Fid = Session["Fid"], CmpId = Session["CmpId"] });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region SaveUpdateUploadPhotoInfo
        public ActionResult SaveUpdateUploadSignatureInfo(Preboarding_Mas_Employee Employeeaddress, HttpPostedFileBase EmployeeSignature)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DapperORM.DynamicQuerySingle("update Preboarding_Mas_Employee set  UploadSignature='" + EmployeeSignature.FileName + "' where  Fid =" + Employeeaddress.FID + "");
                TempData["Message"] = "Record update successfully";
                TempData["Icon"] = "success";
                var OnboardEmployeeId = Session["OnboardEmployeeId"];
                //TempData["P_Id"] = param.Get<string>("@p_Id");
                if (EmployeeSignature != null)
                {
                    try
                    {
                        // Fetch the document path from the database
                        var GetPath = DapperORM.DynamicQuerySingle("SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin = 'Preboarding'");
                        var GetFirstPath = GetPath != null ? GetPath.DocInitialPath : null;
                        // Construct the full path
                        var FirstPath = Path.Combine(GetFirstPath, Employeeaddress.FID.ToString(), "Signature");

                        // Ensure the directory exists
                        if (!Directory.Exists(FirstPath))
                        {
                            Directory.CreateDirectory(FirstPath);
                        }

                        // Save the file if it exists
                        if (EmployeeSignature != null)
                        {
                            string ImgUploadPhotoFilePath = Path.Combine(FirstPath, EmployeeSignature.FileName);
                            EmployeeSignature.SaveAs(ImgUploadPhotoFilePath); // Save image to folder
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["Message"] = "Error: " + ex.Message;
                        TempData["Icon"] = "error";
                    }
                }

                return RedirectToAction("Module_Preboarding_EmployeeApproval", "Module_Preboarding_EmployeeApproval", new { FID_Encrypted = Session["FID_Encrypted"], Fid = Session["Fid"], CmpId = Session["CmpId"] });
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