using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Recruitment;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Conference
{
    public class ESS_EmployeePostingController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: ESS/ESS_EmployeePosting
        public ActionResult ESS_EmployeePosting()
        {

            SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 638;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_Origin", "PoolRecruiter");
                var RequestPool = DapperORM.DynamicList("sp_List_Recruitment_ESS_Approval_Assign_Pool", param);
                ViewBag.GetJobOpeningList = RequestPool;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpGet]
        public ActionResult ViewForEmployeePosting(string ResourceId_Encrypted, int? ResourceId)
        {
            try
            {
                //{
                //    param.Add("@p_ResourceId", ResourceId);
                //    var data = DapperORM.ExecuteSP<dynamic>("sp_List_JobApplication_MoreDetails", param);
                //    ViewBag.GetJobOpeningMoreDetailsList = data;
                //    TempData["JobOpeningId"] = JobOpeningId;

                TempData["JobOpeningId"] = ResourceId;
                Session["ResourceId_Encrypted"] = ResourceId_Encrypted;
                param.Add("@p_EncryptedId", ResourceId_Encrypted);
                param.Add("@p_Origin", "ApprovalRequest");
                var ResourcePool = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_Profiles", param).FirstOrDefault();
                if (ResourcePool != null)
                {
                    ViewBag.GetJobOpeningMoreDetailsList = ResourcePool;
                }
                else
                {
                    ViewBag.ResourceApprovalList = "";
                }

                DynamicParameters paramApprovalRemark = new DynamicParameters();
                paramApprovalRemark.Add("@query", "Select ApproveRejectRemark,Mas_Employee.EmployeeName from Tra_Approval ,Mas_Employee where Tra_Approval.Deactivate=0 and Mas_Employee.Deactivate=0 and Tra_Approval.TraApproval_ApproverEmployeeId=Mas_Employee.EmployeeId and TraApproval_DocId=" + ResourceId + "and Tra_Approval.origin='ResourceRequest'and Tra_Approval.Status='Approved'");
                var ApprovalRemark = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApprovalRemark).ToList();
                ViewBag.ApprovalRemarkList = ApprovalRemark;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult EmployeePosting_CandidateData(string ResumeId_Encrypted, int? Resume_ResourceId, string ResourceId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                var EmpId = Convert.ToInt32(Session["EmployeeId"]);
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 638;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Session["Resume_ResourceId"] = Resume_ResourceId;
                var results = DapperORM.DynamicQueryMultiple(@"SELECT DesignationId  as Id, DesignationName as Name FROM Mas_Designation WHERE Deactivate =0
                                                     select ResumeSourceId  as Id,ResumeSource  as Name  from Recruitment_ResumeSource where deactivate=0 and ResumeSourceId=6;
                                                     Select QualificationPFId as Id,QualificationPFName as Name from Mas_Qualification_PF Where Deactivate = 0 order by Name;
                                                     Select CountryId as Id,Nationality as Name from Mas_Country Where Deactivate = 0 order by IsDefault;
                                                     select NoticePeriodDaysId  as Id,NoticePeriodDays  as Name from Recruitment_NoticePeriodDays where deactivate=0
                                                     Select EmployeeId as Id , concat(Mas_Employee.EmployeeName,' - ' ,Mas_Employee.EmployeeNo)   as Name  from Mas_Employee where deactivate=0 and EmployeeLeft=0 and ContractorID=1 and EmployeeId='"+ EmpId + "'");
                //ViewBag.Designation = results.Read<AllDropDownClass>().ToList();
                //ViewBag.ResumeSource = results.Read<AllDropDownClass>().ToList();
                //ViewBag.GetQualificationName = results.Read<AllDropDownClass>().ToList();
                //ViewBag.GetNationalityName = results.Read<AllDropDownClass>().ToList();
                //ViewBag.NoticePeriod = results.Read<AllDropDownClass>().ToList();
                //ViewBag.ReferEmployee = results.Read<AllDropDownClass>().ToList();
                ViewBag.Designation = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.ResumeSource = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetQualificationName = results[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetNationalityName = results[3].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.NoticePeriod = results[4].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.ReferEmployee = results[5].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                ViewBag.Resume_ResourceId = Resume_ResourceId;
                var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Recruitment_Resume";
                var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                ViewBag.DocNo = DocNo;
                Recruitment_Resume RecruitmentResume = new Recruitment_Resume();
                if (ResumeId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_ResumeId_Encrypted", ResumeId_Encrypted);
                    RecruitmentResume = DapperORM.ReturnList<Recruitment_Resume>("sp_List_Recruitment_Resume_ReferenceESS", param).FirstOrDefault();
                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {
                        var GetDocNos = "Select DocNo As DocNo from Recruitment_Resume where ResumeId_Encrypted='" + ResumeId_Encrypted + "'";
                        var DocNos = DapperORM.DynamicQuerySingle(GetDocNos);
                        ViewBag.DocNo = DocNos;
                    }
                    TempData["ResumeSource"] = RecruitmentResume.ResumeSource;
                    TempData["DocDate"] = RecruitmentResume.DocDate;
                    TempData["DOB"] = RecruitmentResume.DOB;
                    TempData["FilePath"] = RecruitmentResume.FilePath;
                    TempData["ServingNoticePeriod"] = RecruitmentResume.ServingNoticePeriod;
                    TempData["FileName"] = RecruitmentResume.ResumePath;
                    TempData["LeaglBoand"] = RecruitmentResume.LeaglBoand;
                    TempData["HoldingAnyOffer"] = RecruitmentResume.HoldingAnyOffer;
                    Session["SelectedFile"] = RecruitmentResume.ResumePath;
                }
                return View(RecruitmentResume);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpPost]
        public ActionResult SaveUpdate(Recruitment_Resume RecruitmentResume, HttpPostedFileBase ResumePath)
        {
            try
            {
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(RecruitmentResume.ResumeId_Encrypted) ? "Save" : "Update");
                param.Add("@p_ResumeId_Encrypted", RecruitmentResume.ResumeId_Encrypted);
                param.Add("@p_Salutation", RecruitmentResume.Salutation);
                param.Add("@p_CandidateName", RecruitmentResume.CandidateName);
                param.Add("@p_DocNo", RecruitmentResume.DocNo);
                param.Add("@p_DocDate", RecruitmentResume.DocDate);
                param.Add("@p_ResumeReferEmployeeId", RecruitmentResume.ResumeReferEmployeeId);
                param.Add("@p_Resume_ResourceId", Session["Resume_ResourceId"]);
                param.Add("@p_HighestQualification", RecruitmentResume.HighestQualification);
                param.Add("@p_QualificationRemark", RecruitmentResume.QualificationRemark);
                param.Add("@p_CurrentlyWorking", RecruitmentResume.CurrentlyWorking);
                param.Add("@p_MaritalStatus", RecruitmentResume.MaritalStatus);
                param.Add("@p_EmailId", RecruitmentResume.EmailId);
                param.Add("@p_DOB", RecruitmentResume.DOB);
                param.Add("@p_MobileNo", RecruitmentResume.MobileNo);
                param.Add("@p_AlternateMobileNo", RecruitmentResume.AlternateMobileNo);
                param.Add("@p_Gender", RecruitmentResume.Gender);
                param.Add("@p_RelevantSkill", RecruitmentResume.RelevantSkill);

                param.Add("@p_ResumeUploadEmployeeId", EmployeeId);
                param.Add("@p_TotalExperience", RecruitmentResume.TotalExperience);
                param.Add("@p_CurrentCity", RecruitmentResume.CurrentCity);
                param.Add("@p_RelevantExperience", RecruitmentResume.RelevantExperience);
                param.Add("@p_LastCompanyName", RecruitmentResume.LastCompanyName);
                param.Add("@p_ResumeCategory", RecruitmentResume.ResumeCategory);
                param.Add("@p_CTC", RecruitmentResume.CTC);
                param.Add("@p_ExpectedCTC", RecruitmentResume.ExpectedCTC);
                param.Add("@p_LastDesignation", RecruitmentResume.LastDesignation);
                param.Add("@p_SutableForDesignation", RecruitmentResume.SutableForDesignation);
                param.Add("@p_Nationality", RecruitmentResume.Nationality);
                param.Add("@p_FamilyDetails", RecruitmentResume.FamilyDetails);
                param.Add("@p_Age", RecruitmentResume.Age);
                param.Add("@p_LeaglBoand", RecruitmentResume.LeaglBoand);
                param.Add("@p_BondMonth", RecruitmentResume.BondMonth);
                param.Add("@p_NoticePeriodDays", RecruitmentResume.NoticePeriodDays);
                param.Add("@p_ServingNoticePeriod", RecruitmentResume.ServingNoticePeriod);
                param.Add("@p_LastWorkingDay", RecruitmentResume.LastWorkingDay);
                param.Add("@p_HoldingAnyOffer", RecruitmentResume.HoldingAnyOffer);
                param.Add("@p_OfferRemark", RecruitmentResume.OfferRemark);
                param.Add("@p_TechnicalDetails", RecruitmentResume.TechnicalDetails);
                param.Add("@p_CommunicationSkill", RecruitmentResume.CommunicationSkill);
                param.Add("@p_JobDescription", RecruitmentResume.JobDescription);
                param.Add("@p_ExpatLocal", RecruitmentResume.ExpatLocal);
                param.Add("@p_ReadyToTraval", RecruitmentResume.ReadyToTraval);
                param.Add("@p_ResumeSource", RecruitmentResume.ResumeSource);
                param.Add("@p_HrRanking", RecruitmentResume.HrRanking);
                param.Add("@p_HrSelection", RecruitmentResume.HrSelection);
                param.Add("@p_ResumeStatus", "Pending");
                param.Add("@p_Refreance", RecruitmentResume.Refreance);
                param.Add("@p_HRComment", RecruitmentResume.HRComment);
                param.Add("@p_CandidateComment", RecruitmentResume.CandidateComment);
                param.Add("@p_CandidateType", RecruitmentResume.CandidateType);
                param.Add("@p_IsManualAuto", "Manual");
                param.Add("@p_Origin", "CandidateUpload");
                param.Add("@p_ResumeAgencyId", 1);
                if (RecruitmentResume.ResumeId_Encrypted != null && ResumePath == null)
                {
                    param.Add("@p_ResumePath", Session["SelectedFile"]);
                }
                else
                {
                    param.Add("@p_ResumePath", ResumePath == null ? "" : ResumePath.FileName);
                }
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_PoolCandidatesUpload", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                if (TempData["P_Id"] != null)
                {

                    if (ResumePath != null)
                    {
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='RecruitmentCandidateData'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        var FirstPath = GetFirstPath + TempData["P_Id"] + "\\"; // First path plus concat folder 
                        string directoryPath = Path.GetDirectoryName(FirstPath);
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(FirstPath);
                        }
                        string AttachFilePath = "";
                        AttachFilePath = FirstPath + ResumePath.FileName; //Concat Full Path and create New full Path
                        ResumePath.SaveAs(AttachFilePath); // This is use for Save image in folder full path
                    }
                }
                else
                {

                    if (ResumePath != null)
                    {
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='RecruitmentCandidateData'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        var FirstPath = GetFirstPath + RecruitmentResume.ResumeId + "\\"; // First path plus concat folder 
                        string directoryPath = Path.GetDirectoryName(FirstPath);
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(FirstPath);
                        }
                        string AttachFilePath = "";
                        AttachFilePath = FirstPath + ResumePath.FileName; //Concat Full Path and create New full Path
                        ResumePath.SaveAs(AttachFilePath); // This is use for Save image in folder full path
                    }
                }

                #region Send MAil
                DynamicParameters ReferEmpId = new DynamicParameters();
                ReferEmpId.Add("@query", " select * from Recruitment_Resume where ResumeId=" + TempData["P_Id"] + "");
                var ReferEmpId1 = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", ReferEmpId).FirstOrDefault();

                if (ReferEmpId1 != null)
                {
                    DynamicParameters ReferByEmployeeMailId = new DynamicParameters();
                    ReferByEmployeeMailId.Add("@query", "select CompanyMailID,PersonalEmailId  from Mas_Employee memp join Mas_Employee_Personal pemp on memp.EmployeeId=pemp.PersonalEmployeeId where employeeid = " + Session["EmployeeId"] + "");
                    var ReferByEmployeeMail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", ReferByEmployeeMailId).FirstOrDefault();
                    
                    DynamicParameters paramToolEmail = new DynamicParameters();
                    paramToolEmail.Add("@query", "Select CmpId, Origin, SMTPServerName, PortNo, SSL, FromEmailId, Password from Tool_EmailSetting where  Deactivate=0 and CmpId=" + Session["CompanyId"] + "  and  Origin='" + 4 + "'");

                    var GetToolEmail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramToolEmail).FirstOrDefault();
                    if (GetToolEmail == null)
                    {
                        return Json(new { success = false, error = "SMTP settings not found." });
                    }
                    StringBuilder strBuilder = new StringBuilder();
                    string SMTPServerName = GetToolEmail.SMTPServerName;
                    int PortNo = GetToolEmail.PortNo;
                    string FromEmailId = GetToolEmail.FromEmailId;
                    string SMTPPassword = GetToolEmail.Password;
                    bool SSL = GetToolEmail.SSL;

                    using (SmtpClient smtp = new SmtpClient(SMTPServerName, PortNo))
                    {
                        smtp.EnableSsl = SSL;
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential(FromEmailId, SMTPPassword);
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.Timeout = 100000;
                        MailMessage mail = new MailMessage(FromEmailId, ReferByEmployeeMail.CompanyMailID ?? ReferByEmployeeMail.PersonalEmailId);
                        mail.Subject = "Thank You for Your Referral";

                        //mail.Body = "Dear " + ReferEmpId1.ReferredByEmployeeName + ",<br><br>" +
                        //            "We sincerely thank you for referring <b>" + ReferEmpId1.CandidateName + "</b> for an opportunity at <b>" + ResourceRequest.CompanyName + "</b>.<br><br>" +
                        //            "Your recommendation is highly appreciated and plays a significant role in strengthening our talent pipeline.<br>" +
                        //            "Our team will review the candidate's profile and proceed accordingly.<br><br>" +
                        //            "Thank you once again for your valuable contribution to our recruitment process.<br><br>" +
                        //            "Warm regards,<br>" +
                        //            DesignationName + "<br>" +
                        //            CompanyName;


                        mail.IsBodyHtml = true;
                        smtp.Send(mail);
                    }

                    //string Employee_Admin = $@"INSERT INTO Recruitment_EmailLog (Recruitment_EmailSendTo, Recruitment_EmailSendBy, Recruitment_ResourceId, Recruitment_EmailLogDate, EmailId,Origin) 
                    //                   VALUES ({RequestPoolRecruiterMail.EmployeeId}, {Session["EmployeeId"]}, {ReuestPoolRecruiterMail.ResourceId }, GETDATE(), '{ReuestPoolRecruiterMail.CompanyMailID}','Employee');";

                    //strBuilder.Append(Employee_Admin);
                    //string abc = "";
                    //if (objcon.SaveStringBuilder(strBuilder, out abc))
                    //{
                    //    //TempData["Message"] = "Record Update successfully";
                    //    //TempData["Icon"] = "success";
                    //}
                    //if (abc != "")
                    //{
                    //    TempData["Message"] = abc;
                    //    TempData["Icon"] = "error";
                    //}
                }
                #endregion

                return RedirectToAction("ESS_EmployeePosting", "ESS_EmployeePosting");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

    }
}