using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Recruitment;
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

namespace KompassHR.Areas.ESS.Controllers.ESS_Recruitment
{
    public class ESS_Recruitment_OfferLetterGenerationController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: ESS/ESS_Recruitment_OfferLetterGeneration
        #region OfferLetterGeneration MAin View
        [HttpGet]
        public ActionResult ESS_Recruitment_OfferLetterGeneration()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 134;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_InterviewPoolEmployeeId", Session["EmployeeId"]);
                var OfferLetterData = DapperORM.DynamicList("sp_List_Recruitment_OfferLetterGeneration", param);
                ViewBag.OfferLetterlist = OfferLetterData;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ViewForCandidateStatus MAin View 
        [HttpGet]
        public ActionResult ViewForCandidateStatus(string InterviewId_Encrypted, int? InterviewId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                //Session["InterviewId"] = InterviewId;
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EncryptedId", InterviewId_Encrypted);
                paramList.Add("@p_Origin", "OfferLetter");
                var CandidateStatus = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_Profiles", paramList).FirstOrDefault();
                ViewBag.CandidateStatusProfile = CandidateStatus;

                DynamicParameters paramchkList = new DynamicParameters();
                paramchkList.Add("@p_EncryptedId", InterviewId);
                paramchkList.Add("@p_Origin", "Checklist");
                var CandidateChecklist = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_Profiles", paramchkList).ToList();
                ViewBag.CandidateChecklistProfile = CandidateChecklist;


                //DynamicParameters paramskillList = new DynamicParameters();
                //paramskillList.Add("@p_EncryptedId", InterviewId);
                //paramskillList.Add("@p_Origin", "SkillChecklist");
                //var Candidateskilllist = DapperORM.ReturnList<dynamic>("sp_List_Recruitment_Profiles", paramskillList).ToList();
                //ViewBag.CandidateSkillProfile = Candidateskilllist;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ViewForCandidateCTC MAin View 
        [HttpGet]
        public ActionResult ViewForCandidateCTC(string InterviewId_Encrypted, int? InterviewId, string OfferLetterGenId_Encrypted, int? ResumeId, int? Resume_ResourceId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                Session["InterviewId_Encrypted"] = InterviewId_Encrypted;
                Session["InterviewId"] = InterviewId;

                var approverList = DapperORM.ReturnList<AllDropDownBind>("sp_RecruitmentApproverEmployees", null).ToList();

                ViewBag.ApproverName = approverList;

                //if (OfferLetterGenId_Encrypted!=null)
                //{
                //    DynamicParameters paramApp = new DynamicParameters();
                //    paramApp.Add("@query", "Select EmployeeId as Id, concat(EmployeeName,' - ',EmployeeNo) as Name from Mas_Employee where EmployeeDesignationID=49 or EmployeeId in (Select RequestAssignId from Recruitment_ResourceRequest where ResourceId=" + Resume_ResourceId + " and Deactivate=0) or EmployeeId in (Select ResourceEmployeeId from Recruitment_ResourceRequest where ResourceId=" + Resume_ResourceId + " and Deactivate=0)  and EmployeeLeft=0 and Deactivate=0 and ContractorID=1");
                //    ViewBag.ApproverName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramApp).ToList();
                //}
                //else
                //{
                //    DynamicParameters paramApp = new DynamicParameters();
                //    paramApp.Add("@query", "Select EmployeeId as Id, concat(EmployeeName,' - ',EmployeeNo) as Name from Mas_Employee where EmployeeDesignationID=49 or EmployeeId in (Select RequestAssignId from Recruitment_ResourceRequest where ResourceId=" + Resume_ResourceId + " and Deactivate=0) or EmployeeId in (Select ResourceEmployeeId from Recruitment_ResourceRequest where ResourceId=" + Resume_ResourceId + " and Deactivate=0)  and EmployeeLeft=0 and Deactivate=0 and ContractorID=1");
                //    ViewBag.ApproverName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramApp).ToList();
                //}



                Recruitment_OfferLetterGeneration OfferLetterGenerationData = new Recruitment_OfferLetterGeneration();
                if (OfferLetterGenId_Encrypted != null)
                {
                    param.Add("@p_OfferLetterGenId_Encrypted", OfferLetterGenId_Encrypted);
                    OfferLetterGenerationData = DapperORM.ReturnList<Recruitment_OfferLetterGeneration>("sp_List_Recruitment_OfferLetter_Generated", param).FirstOrDefault();
                    TempData["JoiningDate"] = OfferLetterGenerationData.JoiningDate;

                    var GetInterviewId_Encrypted = DapperORM.DynamicQuerySingle("Select InterviewId_Encrypted from Recruitment_Interview where  InterviewId=" + InterviewId + "").FirstOrDefault();
                    var EncryptedId = GetInterviewId_Encrypted.InterviewId_Encrypted;
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EncryptedId", EncryptedId);
                    paramList.Add("@p_Origin", "OfferLetter");
                    var CandidateStatus = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_Profiles", paramList).FirstOrDefault();
                    ViewBag.CandidateStatusProfile = CandidateStatus;

                    //DynamicParameters paramp = new DynamicParameters();
                    //paramp.Add("@p_DocId", OfferLetterGenerationData.OfferLetterGenResumeId);
                    //var GetOfferLetterTemplate = DapperORM.ExecuteSP<dynamic>("sp_GetOfferLetterTemplate", paramp).FirstOrDefault();
                    ViewBag.CandidateStatusProfileLetter = OfferLetterGenerationData.OfferLetterTemplate;
                }
                else
                {
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EncryptedId", InterviewId_Encrypted);
                    paramList.Add("@p_Origin", "OfferLetter");
                    var CandidateStatus = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_Profiles", paramList).FirstOrDefault();
                    ViewBag.CandidateStatusProfile = CandidateStatus;

                    DynamicParameters paramp = new DynamicParameters();
                    paramp.Add("@p_DocId", ResumeId);
                    var GetOfferLetterTemplate = DapperORM.ExecuteSP<dynamic>("sp_GetOfferLetterTemplate", paramp).FirstOrDefault();
                    ViewBag.CandidateStatusProfileLetter = GetOfferLetterTemplate.Body;


                }
                return View(OfferLetterGenerationData);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsOfferLetterValidatioin
        [HttpGet]
        public ActionResult IsOfferLetterValidation(string OfferLetterGenId_Encrypted)
        {
            try
            {

                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_OfferLetterGenId_Encrypted", OfferLetterGenId_Encrypted);
                    param.Add("@p_OfferLetterGenInterviewId", Session["InterviewId"]);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", "Admin");
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_OfferLetterGeneration", param);
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

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region SaveUpdate Interview Detail 
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveUpdate(Recruitment_OfferLetterGeneration OfferLetterGeneration, string OfferLetterTemplate)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(OfferLetterGeneration.OfferLetterGenId_Encrypted) ? "Save" : "Update");
                param.Add("@p_OfferLetterGenId", OfferLetterGeneration.OfferLetterGenId);
                param.Add("@p_OfferLetterGenId_Encrypted", OfferLetterGeneration.OfferLetterGenId_Encrypted);
                param.Add("@p_OfferLetterGenPoolEmployeeId", Session["EmployeeId"]);
                param.Add("@p_OfferLetterGenResourceEmployeeId", OfferLetterGeneration.OfferLetterApproverId);
                param.Add("@p_OfferLetterGenInterviewId", Session["InterviewId"]);
                param.Add("@p_CTC", OfferLetterGeneration.CTC);
                param.Add("@p_JoiningDate", OfferLetterGeneration.JoiningDate);
                param.Add("@p_OfferLetterTemplate", OfferLetterGeneration.OfferLetterTemplate);
                param.Add("@p_GrossAmount", OfferLetterGeneration.GrossAmount);
                param.Add("@p_InHandAmount", OfferLetterGeneration.InHandAmount);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Recruitment_OfferLetterGeneration", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                if (OfferLetterGeneration.OfferLetterGenId != 0)
                {
                    var Update = DapperORM.DynamicQuerySingle("Update Recruitment_Resume set OfferLetterId=" + OfferLetterGeneration.OfferLetterGenId + " where Recruitment_Resume.Deactivate=0 and  Recruitment_Resume.ResumeId=" + OfferLetterGeneration.OfferLetterGenResumeId + "");
                }
                else
                {
                    var Update = DapperORM.DynamicQuerySingle("Update Recruitment_Resume set OfferLetterId=" + TempData["P_Id"] + " where Recruitment_Resume.Deactivate=0 and  Recruitment_Resume.ResumeId=" + OfferLetterGeneration.OfferLetterGenResumeId + "");
                }
                #region send mIAl
                DynamicParameters paramApproverEmployeeId = new DynamicParameters();
                paramApproverEmployeeId.Add("@query", "Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName, EmployeeId  from MAs_employee,Mas_Designation where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID and  EmployeeId =" + OfferLetterGeneration.OfferLetterApproverId + "");
                var ApproverEmployeeIdEmail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApproverEmployeeId).FirstOrDefault();
                var GetUrl1 = "select Recruitment_URL  from Tool_CommonTable";
                var GetUrl = DapperORM.DynamicQuerySingle(GetUrl1);
                if (ApproverEmployeeIdEmail != null &&
    !string.IsNullOrWhiteSpace(Convert.ToString(ApproverEmployeeIdEmail.CompanyMailID)))
                {
                        DynamicParameters paramApprover = new DynamicParameters();
                        paramApprover.Add("@query", "Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName, EmployeeId,MAs_companyProfile.CompanyName  from MAs_employee,Mas_Designation,MAs_companyProfile where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID and MAs_companyProfile.CompanyId=MAs_employee.CmpId and  EmployeeId =" + Session["EmployeeId"] + "");
                        var HRMail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApprover).FirstOrDefault();



                        DynamicParameters paramToolEmail = new DynamicParameters();
                        paramToolEmail.Add("@query", "Select CmpId, Origin, SMTPServerName, PortNo, SSL, FromEmailId, Password from Tool_EmailSetting where  Deactivate=0 and CmpId=" + Session["CompanyId"] + "  and  Origin='" + 4 + "'");
                        var GetToolEmail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramToolEmail).FirstOrDefault();

                       if (GetToolEmail == null || 
    string.IsNullOrWhiteSpace(Convert.ToString(GetToolEmail.FromEmailId)))
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
                        MailMessage mail = new MailMessage();
                        mail.From = new MailAddress(FromEmailId);
                        mail.To.Add(Convert.ToString(ApproverEmployeeIdEmail.CompanyMailID));
                        mail.Subject = "Offer Letter Approval Request";
                            mail.Body = "Dear " + ApproverEmployeeIdEmail.EmployeeName + ",<br><br>This is a kind reminder that an offer letter approval request is pending in the ESS portal.<br><br>" +
                                        "Kindly log in to " + GetUrl.Recruitment_URL + " and navigate to the Approvals section to review and approve the request at your earliest convenience.<br><br>" +
                                        " Your prompt action will help us proceed with the next steps in the recruitment process.<br><br>Thank you for your attention.<br><br>" +
                                        " Best regards,<br>" + HRMail.EmployeeName + "<br>" + HRMail.DesignationName + "<br>" + HRMail.CompanyName + "<br>" + "";


                            mail.IsBodyHtml = true;
                            smtp.Send(mail);
                        }
                        string Employee_Admin = $@"INSERT INTO Recruitment_EmailLog (Recruitment_EmailSendTo, Recruitment_EmailSendBy, Recruitment_ResourceId, Recruitment_EmailLogDate, EmailId,Origin) 
                                       VALUES ({ApproverEmployeeIdEmail.EmployeeId}, {Session["EmployeeId"]}, {OfferLetterGeneration.OfferLetterGenResumeId }, GETDATE(), '{ApproverEmployeeIdEmail.CompanyMailID}','Employee');";

                        strBuilder.Append(Employee_Admin);
                        string abc = "";
                        if (objcon.SaveStringBuilder(strBuilder, out abc))
                        {

                            TempData["Message"] = "Record Update successfully";
                            TempData["Icon"] = "success";
                        }
                        if (abc != "")
                        {
                            TempData["Message"] = abc;
                            TempData["Icon"] = "error";
                        }
                    
                }
                #endregion


                return RedirectToAction("ESS_Recruitment_OfferLetterGeneration", "ESS_Recruitment_OfferLetterGeneration");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Download Image 
        public ActionResult DownloadFile(string ResourceRequest)
        {
            try
            {
                if (ResourceRequest != null)
                {
                    System.IO.File.ReadAllBytes(ResourceRequest);
                    return File(ResourceRequest, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(ResourceRequest));
                }
                else
                {
                    return RedirectToAction("ApprovalList", "ESS_Resources_Recruitment", new { Area = "ESS" });
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region OfferLetterGeneratedList
        [HttpGet]
        public ActionResult OfferLetterGeneratedList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 134;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_OfferLetterGenId_Encrypted", "List");
                param.Add("@p_OfferLetterGenId_EmployeeId", Session["EmployeeId"]);
                var OfferLetterData = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_OfferLetter_Generated", param).ToList();
                ViewBag.OfferLetterGeneratedlist = OfferLetterData;
                return View();

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete Offer Letter
        public ActionResult Delete(string OfferLetterGenId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_OfferLetterGenId_Encrypted", OfferLetterGenId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_OfferLetterGeneration", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_Recruitment_OfferLetterGeneration", "ESS_Recruitment_OfferLetterGeneration");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        public ActionResult RevisedOfferLetterList(string Id)
        {
            try
            {
                var param2 = new DynamicParameters();
                param2.Add("@query", "select res.CandidateName,DepartmentName,DesignationName,res.EmailId,res.MobileNo,let.CTC,GrossAmount,InHandAmount from Recruitment_OfferLetterGeneration let left join Recruitment_Resume res on let.OfferLetterGenResumeId=res.ResumeId left join Mas_Department dept on let.OfferLetterGenDepartmentId =dept.DepartmentId left join Mas_Designation desg on let.OfferLetterGenDesignationId=desg.DesignationId WHERE res.ResumeId_Encrypted='" + Id + "'");
                var List = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param2).ToList();
                ViewBag.List = List;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

    }
}