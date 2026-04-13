using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Recruitment;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Setting.Models.Setting_Recruitment;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Z.Dapper.Plus;

namespace KompassHR.Areas.ESS.Controllers.ESS_Recruitment
{
    public class ESS_Recruitment_InterviewController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: ESS/ESS_Recruitment_Interview
        #region ESS_Recruitment_Interview MAin View
        [HttpGet]
        public ActionResult ESS_Recruitment_Interview()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 133;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_Qry", "and Recruitment_Interview.InterviewStatus ='Pending' and Recruitment_Interview.InterviewResourceEmployeeId =" + Session["EmployeeId"] + " ");
                var InterviewList = DapperORM.DynamicList("sp_List_Recruitment_InterviewCandidate", param);
                ViewBag.GetInterviewList = InterviewList;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ViewForCoductInterview
        [HttpGet]
        public ActionResult ViewForCoductInterview(string InterviewId_Encrypted, string InterviewId, int? InterviewResourceId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramList = new DynamicParameters();
                Session["InterviewId_Encrypted"] = InterviewId_Encrypted;
                Session["InterviewId"] = InterviewId;
                paramList.Add("@p_EncryptedId", InterviewId_Encrypted);
                paramList.Add("@p_Origin", "Interview");
                var CandiadateSchedule = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_Profiles", paramList).FirstOrDefault();
                ViewBag.CandiadateScheduleList = CandiadateSchedule;
                Session["DesignationID"] = CandiadateSchedule.DesignationID;
                Session["DepartmentID"] = CandiadateSchedule.DepartmentID;
                Session["GradeID"] = CandiadateSchedule.GradeID;

                DynamicParameters paramApprovalRemark = new DynamicParameters();
                paramApprovalRemark.Add("@query", "Select ApproveRejectRemark,Mas_Employee.EmployeeName from Tra_Approval ,Mas_Employee where Tra_Approval.Deactivate=0 and Mas_Employee.Deactivate=0 and Tra_Approval.TraApproval_ApproverEmployeeId=Mas_Employee.EmployeeId and TraApproval_DocId=" + InterviewResourceId + " and  Tra_Approval.origin='ResourceRequest'");
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
        #endregion

        #region ConductInterview View 
        [HttpGet]
        public ActionResult ConductInterview(string InterviewId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EncryptedId", InterviewId_Encrypted);
                paramList.Add("@p_Origin", "ConductInterview");
                var ConductInterview = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_Profiles", paramList).FirstOrDefault();
                ViewBag.ConductInterviewList = ConductInterview;


                var results = DapperORM.DynamicQueryList(@"select CheckListID ,Remarks,Rate  from Recruitment_CheckList where Origin ='Interview CheckList' and Recruitment_CheckList.DesignationID='" + Session["DesignationID"] + "' and Recruitment_CheckList.DepartmentID='" + Session["DepartmentID"] + "' and Recruitment_CheckList.GradeID='" + Session["GradeID"] + "' and Recruitment_CheckList.Deactivate =0").ToList();
                ViewBag.ChecklistName = results;

                DynamicParameters paramSkills = new DynamicParameters();
                paramSkills.Add("@query", "Select DISTINCT (SkillName) As Name from Recruitment_Interview_Skill Where Deactivate=0");
                var SkillsList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramSkills).ToList();
                ViewBag.GetSkillsList = SkillsList;
                ViewBag.GetRemarkList = "";
                return View();
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
        public ActionResult SaveUpdate(string Result, string Reason, string InterviewResult, List<Recruitment_Interview_Result_ChkList> TaskChecklist, /*List<Recruitment_Interview_Result_Skill> TaskSkill,*/ int? InterviewID, int? InterviewResumeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var ResumeUpdate = DapperORM.DynamicQuerySingle("update Recruitment_Resume set InterviewId=" + InterviewID + ",Recruitment_Resume.OfferLetterId=0 where Recruitment_Resume.Deactivate=0 and  ResumeId=" + InterviewResumeId + "");
                var Update = DapperORM.DynamicQuerySingle("Update Recruitment_Interview set InterviewStatus='" + Result + "', Reason='" + Reason + "',InterviewResult='" + InterviewResult + "' where  InterviewId_Encrypted='" + Session["InterviewId_Encrypted"] + "'");
                if (TaskChecklist != null)
                {
                    using (var connection = new SqlConnection(DapperORM.connectionString))
                    {

                        List<Recruitment_Interview_Result_ChkList> Checklist = new List<Recruitment_Interview_Result_ChkList>();
                        var sql = "INSERT INTO Recruitment_Interview_Result_ChkList ( InterviewResultCheckListId, InterviewResultCheckListRate, InterviwerRate,InterviewResultInterviewId, CreatedBy,InterviewResultRemark, MachineName) VALUES (@InterviewResultCheckListId, @InterviewResultCheckListRate, @InterviwerRate,@InterviewResultInterviewId, @CreatedBy,@InterviewResultRemark, @MachineName)";
                        for (int i = 0; i < TaskChecklist.Count; i++)
                        {
                            Checklist.Add(new Recruitment_Interview_Result_ChkList()
                            {
                                InterviewResultCheckListId = TaskChecklist[i].InterviewResultCheckListId,
                                InterviewResultCheckListRate = TaskChecklist[i].InterviewResultCheckListRate,
                                InterviwerRate = TaskChecklist[i].InterviwerRate,
                                InterviewResultInterviewId = Convert.ToDouble(Session["InterviewId"]),
                                CreatedBy = Convert.ToString(Session["EmployeeName"]),
                                InterviewResultRemark = TaskChecklist[i].InterviewResultRemark,
                                MachineName = Dns.GetHostName().ToString()

                            });
                        }
                        var rowsAffected1 = connection.Execute(sql, Checklist);
                    }

                }

                //if (TaskSkill != null)
                //{
                //    using (var connection = new SqlConnection(DapperORM.connectionString))
                //    {

                //        List<Recruitment_Interview_Result_Skill> SkillsList = new List<Recruitment_Interview_Result_Skill>();
                //        var sql = "INSERT INTO Recruitment_Interview_Result_Skill ( Interview_Skill_Name, Interview_Skill_Description, InterviewResultSkillInterviewId, CreatedBy, MachineName) VALUES (@Interview_Skill_Name, @Interview_Skill_Description, @InterviewResultSkillInterviewId, @CreatedBy, @MachineName)";
                //        for (int i = 0; i < TaskSkill.Count; i++)
                //        {
                //            SkillsList.Add(new Recruitment_Interview_Result_Skill()
                //            {
                //                Interview_Skill_Name = TaskSkill[i].Interview_Skill_Name,
                //                Interview_Skill_Description = TaskSkill[i].Interview_Skill_Description,
                //                InterviewResultSkillInterviewId = Convert.ToDouble(Session["InterviewId"]),
                //                CreatedBy = Convert.ToString(Session["EmployeeName"]),
                //                MachineName = Dns.GetHostName().ToString()
                //            });
                //        }
                //        var rowsAffected = connection.Execute(sql, SkillsList);
                //    }

                //}
                TempData["Message"] = "Interview done successfully";
                TempData["Icon"] = "success";

                #region Send MAil
                DynamicParameters paramApproverEmployeeId = new DynamicParameters();
                paramApproverEmployeeId.Add("@query", " Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName, EmployeeId,MAs_companyProfile.CompanyName,Recruitment_ResourceRequest.ResourceId from   MAs_employee,Mas_Designation,MAs_companyProfile,Recruitment_ResourceRequest,Recruitment_Resume  where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID    and MAs_companyProfile.CompanyId=MAs_employee.CmpId and   Recruitment_ResourceRequest.ReuestPoolRecruiterId=MAs_employee.EmployeeId  and  Recruitment_ResourceRequest.ResourceId=Recruitment_Resume.Resume_ResourceId   and Recruitment_ResourceRequest.Deactivate=0   and Recruitment_Resume.Deactivate=0   and Recruitment_Resume.ResumeId =" + InterviewResumeId + "");
                var ReuestPoolRecruiterMail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApproverEmployeeId).FirstOrDefault();
                var GetUrl1 = "select Recruitment_URL  from Tool_CommonTable";
                var GetUrl = DapperORM.DynamicQuerySingle(GetUrl1);
                if (ReuestPoolRecruiterMail != null &&
                    !string.IsNullOrWhiteSpace(Convert.ToString(ReuestPoolRecruiterMail.CompanyMailID)))
                {
                    DynamicParameters paramApprover = new DynamicParameters();
                    paramApprover.Add("@query", "Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName, EmployeeId,MAs_companyProfile.CompanyName  from MAs_employee,Mas_Designation,MAs_companyProfile where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID and MAs_companyProfile.CompanyId=MAs_employee.CmpId and  EmployeeId =" + Session["EmployeeId"] + "");
                    var ResourceRequest = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApprover).FirstOrDefault();

                    DynamicParameters paramPosition = new DynamicParameters();
                    paramPosition.Add("@query", "Select Mas_Designation.DesignationName, Mas_Department.DepartmentName  from Mas_Department,Mas_Designation,Recruitment_ResourceRequest,Recruitment_Resume  where Mas_Designation.DesignationId=Recruitment_ResourceRequest.DesignationID  and  Recruitment_ResourceRequest.ResourceId=Recruitment_Resume.Resume_ResourceId   and  Mas_Department.DepartmentId=Recruitment_ResourceRequest.DepartmentID  and Mas_Department.Deactivate=0 and Mas_Designation.Deactivate=0 and Recruitment_ResourceRequest.Deactivate=0  and  Recruitment_Resume.Deactivate=0   and Recruitment_Resume.ResumeId =" + InterviewResumeId + "");
                    var PositionDetail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramPosition).FirstOrDefault();

                    DynamicParameters paramToolEmail = new DynamicParameters();
                    paramToolEmail.Add("@query", "Select CmpId, Origin, SMTPServerName, PortNo, SSL, FromEmailId, Password from Tool_EmailSetting where  Deactivate=0 and CmpId=" + Session["CompanyId"] + "  and  Origin='" + 4 + "'");

                    var GetToolEmail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramToolEmail).FirstOrDefault();
                    //if (GetToolEmail == null)
                    //{
                    //    return Json(new { success = false, error = "SMTP settings not found." });
                    //}
                    if (GetToolEmail == null ||
    string.IsNullOrWhiteSpace(Convert.ToString(GetToolEmail.FromEmailId)))
                    {
                        TempData["Message"] = "SMTP settings not found.";
                        TempData["Icon"] = "error";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
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
                        mail.To.Add(Convert.ToString(ReuestPoolRecruiterMail.CompanyMailID));
                        mail.Subject = "Interview Completed – Further Action Required";
                        mail.Body = "Dear " + ReuestPoolRecruiterMail.EmployeeName + ",<br><br>I would like to bring to your attention the recruitment resource request submitted on <b>" + DateTime.Now.ToString("dd/MMM/yyyy") + "</b> for the <b>" + PositionDetail.DesignationName + "</b> within the <b>" + PositionDetail.DepartmentName + "</b> . The candidate has successfully completed the interview and has been " + Result + ".<br><br>" +
                                    "Kindly log in to " + GetUrl.Recruitment_URL + " and take the necessary actions on the candidate to proceed with the next steps<br><br>" +
                                    "Your prompt attention to this matter would be greatly appreciated. Please let me know if any additional information or documentation is required to facilitate process.<br><br>Thank you for your consideration.<br><br>" +
                                    "Best regards,<br>" + ResourceRequest.EmployeeName + "<br>" + ResourceRequest.DesignationName + "<br>" + ResourceRequest.CompanyName + "<br>" + "";


                        mail.IsBodyHtml = true;
                        smtp.Send(mail);
                    }

                    string Employee_Admin = $@"INSERT INTO Recruitment_EmailLog (Recruitment_EmailSendTo, Recruitment_EmailSendBy, Recruitment_ResourceId, Recruitment_EmailLogDate, EmailId,Origin) 
                                       VALUES ({ReuestPoolRecruiterMail.EmployeeId}, {Session["EmployeeId"]}, {ReuestPoolRecruiterMail.ResourceId }, GETDATE(), '{ReuestPoolRecruiterMail.CompanyMailID}','Employee');";

                    strBuilder.Append(Employee_Admin);
                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {
                        //TempData["Message"] = "Record Update successfully";
                        //TempData["Icon"] = "success";
                    }
                    if (abc != "")
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                }

                #endregion

                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                //return RedirectToAction("ESS_Recruitment_Interview", "ESS_Recruitment_Interview");

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region SaveUpdate Interview Detail 
        [HttpGet]
        public ActionResult InterviewAcceptReject(string Status, string Remark)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                if (Status == "Rejected")
                {
                    var Update = DapperORM.DynamicQuerySingle("Update Recruitment_Interview set InterviewStatus='" + Status + "', Reason='" + Remark + "' where  InterviewId_Encrypted='" + Session["InterviewId_Encrypted"] + "'");
                    TempData["Message"] = "Interview rejected succesfully";
                    TempData["Icon"] = "success";

                    #region Send maail
                    // Fetch SMTP details
                    DynamicParameters paramToolEmail = new DynamicParameters();
                    paramToolEmail.Add("@query", "SELECT CmpId, Origin, SMTPServerName, PortNo, SSL, FromEmailId, Password FROM Tool_EmailSetting WHERE Deactivate = 0 AND CmpId = '" + Session["CompanyId"] + "' AND Origin = '4'");
                    var GetToolEmail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramToolEmail).FirstOrDefault();

                    DynamicParameters paramApproverEmployeeId = new DynamicParameters();
                    paramApproverEmployeeId.Add("@query", "select TraApproval_ApproverEmployeeId as ApproverEmployeeId  from Tra_Approval where Deactivate=0 and TraApproval_DocId=(Select InterviewResourceId from Recruitment_Interview where Recruitment_Interview.InterviewId_Encrypted='" + Session["InterviewId_Encrypted"] + "') and TraApproval_ModuleId=3");
                    var ApproverEmployeeId = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApproverEmployeeId).ToList();

                    string approverIds = string.Join(",", ApproverEmployeeId.Select(a => Convert.ToInt32(a.ApproverEmployeeId)));

                    paramApproverEmployeeId.Add("@query", "Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName, EmployeeId  from MAs_employee,Mas_Designation where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID and  EmployeeId in (" + approverIds + ")");
                    var ApproverEmployeeIdEmail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApproverEmployeeId).ToList();


                    var approversList = ApproverEmployeeIdEmail.ToList(); // Get the list of approvers

                    // Declare arrays to hold the dynamic approval information
                    List<string> mailIds = new List<string>();
                    List<string> mobiles = new List<string>();
                    List<string> names = new List<string>();
                    List<string> designations = new List<string>();
                    List<string> empIds = new List<string>();

                    // Loop through the list of approvers and assign values dynamically
                    for (int i = 0; i < approversList.Count; i++)
                    {
                        var approver = approversList[i];

                        // Dynamically assign values to corresponding lists
                        mailIds.Add(approver.CompanyMailID);
                        mobiles.Add(approver.CompanyMobileNo);
                        names.Add(approver.EmployeeName);
                        designations.Add(approver.DesignationName);
                        empIds.Add(approver.EmployeeId.ToString());
                    }

                    // If you need to access specific approvers, you can now refer to them like this:
                    string mailid1 = null;
                    string mailid2 = null;
                    string mailid3 = null;

                    if (mailIds.Count > 0)
                    {
                        mailid1 = mailIds[0];  // 1st approver's mail
                    }

                    if (mailIds.Count > 1)
                    {
                        mailid2 = mailIds[1];  // 2nd approver's mail
                    }

                    if (mailIds.Count > 2)
                    {
                        mailid3 = mailIds[2];  // 3rd approver's mail
                    }
                    // 3rd approver's mail
                    // Continue as needed for other properties

                    // Similarly for the rest:
                    string mobile1 = mobiles.Count > 0 ? mobiles[0] : "";
                    string namesCommaSeparated = string.Join(",", names);
                    string designation1 = designations.Count > 0 ? designations[0] : "";
                    string empId1 = empIds.Count > 0 ? empIds[0] : "";

                    DynamicParameters paramResourceEmployeeId = new DynamicParameters();
                    paramResourceEmployeeId.Add("@query", "select ResourceEmployeeId from Recruitment_ResourceRequest where ResourceId=(Select InterviewResourceId from Recruitment_Interview where Recruitment_Interview.InterviewId_Encrypted='" + Session["InterviewId_Encrypted"] + "')  and  Deactivate=0 ");
                    var ResourceEmployeeId = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramResourceEmployeeId).FirstOrDefault();
                    paramResourceEmployeeId.Add("@query", "Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName,EmployeeId from MAs_employee,Mas_Designation where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID and  EmployeeId=" + ResourceEmployeeId.ResourceEmployeeId + "");
                    var ResourceEmployeeIdEmail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramResourceEmployeeId).FirstOrDefault();
                    DynamicParameters paramInterviewer = new DynamicParameters();
                    paramInterviewer.Add("@query", "Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName,EmployeeId from MAs_employee,Mas_Designation where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID and  EmployeeId=(Select InterviewerEmployeeId from Recruitment_Interview where InterviewId_Encrypted='" + Session["InterviewId_Encrypted"] + "')");
                    var InterviewerEmployeeIdEmail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramInterviewer).FirstOrDefault();


                    DynamicParameters paramId = new DynamicParameters();
                    paramId.Add("@query", "Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName,EmployeeId from MAs_employee,Mas_Designation where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID and  EmployeeId=(Select InterviewPoolEmployeeId from Recruitment_Interview where InterviewId_Encrypted='" + Session["InterviewId_Encrypted"] + "')");
                    var PoolRecruiterEmail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramId).FirstOrDefault();

                    DynamicParameters paramRecruitmentcandidate = new DynamicParameters();
                    paramRecruitmentcandidate.Add("@query", "Select Recruitment_Resume.EmailId as CandidateMail,Recruitment_Resume.Resume_ResourceId as ResourceId,Recruitment_Resume.CandidateName,Recruitment_ResourceRequest.ReuestPoolRecruiterId as ReuestPoolRecruiterId , Mas_CompanyProfile.CompanyName,Mas_Designation.DesignationName from Recruitment_Resume ,Mas_CompanyProfile,Mas_Designation, Recruitment_ResourceRequest where  Recruitment_ResourceRequest.ResourceId=Recruitment_Resume.Resume_ResourceId and Recruitment_ResourceRequest.CmpId=Mas_CompanyProfile.CompanyId and Mas_Designation.DesignationId=Recruitment_ResourceRequest.DesignationID and   Recruitment_ResourceRequest.Deactivate=0 and Recruitment_Resume.Deactivate=0 and  ResumeId=(Select InterviewResumeId  from Recruitment_Interview  where InterviewId_Encrypted='" + Session["InterviewId_Encrypted"] + "')");
                    var RecruitmentInterview = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramRecruitmentcandidate).FirstOrDefault();


                    if (PoolRecruiterEmail == null ||
    string.IsNullOrWhiteSpace(Convert.ToString(PoolRecruiterEmail.CompanyMailID)))
                    {
                        TempData["Message"] = "Enter a Recruiter Email Id";
                        TempData["Icon"] = "error";
                        return RedirectToAction("ESS_Recruitment_ShortListed", "ESS_Recruitment_ShortListed");
                    }
                    StringBuilder strBuilder = new StringBuilder();


                    var CandidateMail = RecruitmentInterview.CandidateMail;
                    if (GetToolEmail != null &&
    !string.IsNullOrWhiteSpace(Convert.ToString(GetToolEmail.FromEmailId)))
                    {
                        var SMTPServerName = GetToolEmail.SMTPServerName;
                        var PortNo = GetToolEmail.PortNo;
                        var FromEmailId = GetToolEmail.FromEmailId;
                        var SMTPPassword = GetToolEmail.Password;
                        var SSL = GetToolEmail.SSL;

                        using (SmtpClient smtp = new SmtpClient(SMTPServerName, PortNo))
                        {
                            smtp.EnableSsl = SSL;
                            smtp.UseDefaultCredentials = false;
                            smtp.Credentials = new System.Net.NetworkCredential(FromEmailId, SMTPPassword);
                            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                            smtp.Timeout = 100000;

                            try
                            {
                                using (MailMessage mail = new MailMessage())
                                {
                                    mail.From = new MailAddress(FromEmailId);

                                    // 📩 Ensure CandidateMail is provided
                                    if (string.IsNullOrEmpty(RecruitmentInterview.CandidateMail))
                                    {
                                        TempData["Message"] = "Enter a Recruiter Email Id";
                                        TempData["Icon"] = "error";
                                        return RedirectToAction("ESS_Recruitment_ShortListed", "ESS_Recruitment_ShortListed");
                                    }

                                    mail.To.Add(new MailAddress(RecruitmentInterview.CandidateMail));

                                    List<string> recipients = new List<string>();

                                    if (mailid1 != null && !string.IsNullOrEmpty(mailIds[0]))
                                        recipients.Add(mailIds[0]);

                                    if (mailid2 != null && !string.IsNullOrEmpty(mailIds[1]))
                                        recipients.Add(mailIds[1]);

                                    if (mailid3 != null && !string.IsNullOrEmpty(mailIds[2]))
                                        recipients.Add(mailIds[2]);


                                    if (ResourceEmployeeIdEmail != null && !string.IsNullOrEmpty(ResourceEmployeeIdEmail.CompanyMailID))
                                        recipients.Add(ResourceEmployeeIdEmail.CompanyMailID);

                                    if (!string.IsNullOrEmpty(RecruitmentInterview.CandidateMail))
                                        recipients.Add(RecruitmentInterview.CandidateMail);

                                    if (InterviewerEmployeeIdEmail != null && !string.IsNullOrEmpty(InterviewerEmployeeIdEmail.CompanyMailID))
                                        recipients.Add(InterviewerEmployeeIdEmail.CompanyMailID);

                                    foreach (var recipient in recipients)
                                    {
                                        int ResourceId = (int)RecruitmentInterview.ResourceId;
                                        int Recruitment_EmailSendBy = (int)RecruitmentInterview.ReuestPoolRecruiterId;
                                        int Recruitment_EmailSendTo = 0; // Default value

                                        if (recipient == RecruitmentInterview.CandidateMail)
                                        {
                                            // Set to Candidate Employee ID if available
                                            Recruitment_EmailSendTo = 0;
                                        }
                                        else if (ResourceEmployeeIdEmail != null && recipient == ResourceEmployeeIdEmail.CompanyMailID)
                                        {
                                            Recruitment_EmailSendTo = (int)ResourceEmployeeIdEmail.EmployeeId;
                                        }
                                        else if (ApproverEmployeeIdEmail != null && recipient == mailid1)
                                        {
                                            Recruitment_EmailSendTo = (int)empId1[0];
                                        }
                                        else if (ApproverEmployeeIdEmail != null && recipient == mailid2)
                                        {
                                            Recruitment_EmailSendTo = (int)empId1[1];
                                        }
                                        else if (ApproverEmployeeIdEmail != null && recipient == mailid3)
                                        {
                                            Recruitment_EmailSendTo = (int)empId1[2];
                                        }
                                        else if (InterviewerEmployeeIdEmail != null && recipient == InterviewerEmployeeIdEmail.CompanyMailID)
                                        {
                                            Recruitment_EmailSendTo = (int)InterviewerEmployeeIdEmail.EmployeeId;
                                        }

                                        string Employee_Admin = $@"INSERT INTO Recruitment_EmailLog (Recruitment_EmailSendTo, Recruitment_EmailSendBy, Recruitment_ResourceId, Recruitment_EmailLogDate, EmailId,Origin) 
                                       VALUES ({Recruitment_EmailSendTo}, {Recruitment_EmailSendBy}, {ResourceId}, GETDATE(), '{recipient}','Employee');";

                                        strBuilder.Append(Employee_Admin);
                                    }

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
                                    // 📩 Add Other Emails in "CC"
                                    if (!string.IsNullOrEmpty(mailid1))
                                        mail.CC.Add(new MailAddress(mailIds[0]));

                                    if (!string.IsNullOrEmpty(mailid2))
                                        mail.CC.Add(new MailAddress(mailIds[1]));

                                    if (!string.IsNullOrEmpty(mailid3))
                                        mail.CC.Add(new MailAddress(mailIds[2]));

                                    if (!string.IsNullOrEmpty(ResourceEmployeeIdEmail?.CompanyMailID))
                                        mail.CC.Add(new MailAddress(ResourceEmployeeIdEmail.CompanyMailID));

                                    if (!string.IsNullOrEmpty(InterviewerEmployeeIdEmail?.CompanyMailID))
                                        mail.CC.Add(new MailAddress(InterviewerEmployeeIdEmail.CompanyMailID));

                                    // 📩 Email Content
                                    mail.Subject = $"Interview update for the {RecruitmentInterview.designationname} Position at {RecruitmentInterview.CompanyName}";

                                    mail.Body = $@"
                                            Dear {RecruitmentInterview.CandidateName}, <br><br>
                                            I would like to inform you that, due to unforeseen circumstances, we were unable to proceed with your scheduled interview for the {RecruitmentInterview.designationname} position at {RecruitmentInterview.CompanyName}.</b>.
                                            We sincerely apologize for any inconvenience this may have caused. We highly value your interest in joining SMAuto and will coordinate with you shortly to arrange a new date and time for the interview as per mutual convenience.<br><br>
                                            For any further queries, feel free to reach out to our HR, {PoolRecruiterEmail.EmployeeName}.<br><br>
                                            Thank you for your understanding and patience.<br><br>
                                            Please feel free to reach out if you have any questions.<br><br>
                                            Best regards,<br>
                                            {PoolRecruiterEmail?.EmployeeName ?? "HR"}<br>
                                            {GetToolEmail?.FromEmailId ?? ""}<br>
                                            {RecruitmentInterview.CompanyName}<br>
                                            {PoolRecruiterEmail?.CompanyMobileNo ?? ""}
                                            ";

                                    mail.IsBodyHtml = true;
                                    smtp.Send(mail);

                                    // Console.WriteLine("Email sent successfully.");
                                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            catch (Exception ex)
                            {
                                TempData["Message"] = "Error: " + ex.Message;
                                TempData["Icon"] = "error";
                            }
                        }
                    }
                    #endregion



                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var Update = DapperORM.DynamicQuerySingle("Update Recruitment_Interview set InterviewAccept=1, InterviewAcceptRemark='" + Remark + "' where  InterviewId_Encrypted='" + Session["InterviewId_Encrypted"] + "'");
                    TempData["Message"] = "Interview accepted succesfully";
                    TempData["Icon"] = "success";
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Get skills Remark
        [HttpGet]
        public ActionResult GetSkillsRemark(string Skill)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "Select InterviewSkillId As Id,Description As  Name from Recruitment_Interview_Skill Where Deactivate=0  and SkillName= '" + Skill + "'");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Download Image 
        public ActionResult DownloadFile(string FilePath)
        {
            try
            {
                if (FilePath != "")
                {
                    if (System.IO.File.Exists(FilePath))
                    {
                        System.IO.File.ReadAllBytes(FilePath);
                        return File(FilePath, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(FilePath));
                    }
                    else
                    {
                        return Json(false);
                    }
                }
                else
                {
                    return RedirectToAction("ESS_Recruitment_Interview", "ESS_Recruitment_Interview", new { Area = "ESS" });
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region Get skills Remark
        [HttpGet]
        public ActionResult GetChecklist()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var DeptId = Convert.ToInt32(Session["DepartmentID"]);
                var DesgId = Convert.ToInt32(Session["DesignationID"]);
                var GradeId = Convert.ToInt32(Session["GradeID"]);
                var results = DapperORM.DynamicQueryList(@"select CheckListID ,Remarks,Rate  from Recruitment_CheckList where Origin ='Interview CheckList' and Recruitment_CheckList.DesignationID='" + Session["DesignationID"] + "' and Recruitment_CheckList.DepartmentID='" + Session["DepartmentID"] + "' and Recruitment_CheckList.GradeID='" + Session["GradeID"] + "' and Recruitment_CheckList.Deactivate =0").ToList();
                if (results.Count != 0)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ESS_Recruitment_Interview MAin View
        [HttpGet]
        public ActionResult InterviewScheduledCandidatelist()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 133;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_Qry", "and Recruitment_Interview.InterviewResourceEmployeeId =" + Session["EmployeeId"] + " and Recruitment_Interview.InterviewStatus in ('Selected','Rejected')");
                var InterviewList = DapperORM.DynamicList("sp_List_Recruitment_InterviewCandidate", param);
                ViewBag.GetInterviewList = InterviewList;
                return View();
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