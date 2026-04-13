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
    public class ESS_Recruitment_ShortListedController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: ESS/ESS_Recruitment_ShortListed
        #region ShortListed List VIew 
        [HttpGet]
        public ActionResult ESS_Recruitment_ShortListed()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 132;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_Qry", "and Recruitment_Resume.ResumeStatus='ShortListed' and Recruitment_Resume.ResumeUploadEmployeeId =" + Session["EmployeeId"] + " ");
                var ShortListedData = DapperORM.DynamicList("sp_List_Recruitment_ShortListedCandidate", param);
                ViewBag.ShortListedDataList = ShortListedData;
                return View();
            }

            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ViewForCreatingInterviewLink Main View 
        [HttpGet]
        public ActionResult ViewForCreatingInterviewLink(string ResumeId_Encrypted, int? ResumeId, int? Resume_ResourceId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                Session["ResumeId"] = ResumeId;
                param.Add("@p_EncryptedId", ResumeId_Encrypted);
                param.Add("@p_Origin", "ShortListed");
                var GetShortListedData = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_Profiles", param).FirstOrDefault();
                ViewBag.ShortListedProfile = GetShortListedData;

                string Encrypted = ResumeId_Encrypted;

                //DynamicParameters paramApprovalRemark = new DynamicParameters();
                //paramApprovalRemark.Add("@query", "Select LastCallByRecruiterRemark,Mas_Employee.EmployeeName as ShortEmployeeName from Recruitment_Resume,Mas_Employee,Recruitment_ResourceRequest where Recruitment_Resume.Deactivate=0 and Mas_Employee.Deactivate=0 and Recruitment_ResourceRequest.Deactivate=0 and Recruitment_ResourceRequest.ResourceId=Recruitment_Resume.Resume_ResourceId and Mas_Employee.EmployeeId=Recruitment_ResourceRequest.ResourceEmployeeId and ResumeId=" + ResumeId + "");
                //var ApprovalRemark = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApprovalRemark).ToList();
                //ViewBag.ApprovalRemarkList = ApprovalRemark;

                DynamicParameters paramPosition = new DynamicParameters();
                paramPosition.Add("@query", "Select Mas_Designation.DesignationName, Mas_Department.DepartmentName ,Recruitment_ResourceRequest.DepartmentID  from Mas_Department,Mas_Designation,Recruitment_ResourceRequest  where Mas_Designation.DesignationId=Recruitment_ResourceRequest.DesignationID and  Mas_Department.DepartmentId=Recruitment_ResourceRequest.DepartmentID  and Mas_Department.Deactivate=0 and Mas_Designation.Deactivate=0 and Recruitment_ResourceRequest.Deactivate=0  and Recruitment_ResourceRequest.ResourceId =" + Resume_ResourceId + "");
                var PositionDetail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramPosition).FirstOrDefault();

                DynamicParameters paramApprovalRemark = new DynamicParameters();
                paramApprovalRemark.Add("@query", "Select ApproveRejectRemark,Mas_Employee.EmployeeName from Tra_Approval ,Mas_Employee where Tra_Approval.Deactivate=0 and Mas_Employee.Deactivate=0 and Tra_Approval.TraApproval_ApproverEmployeeId=Mas_Employee.EmployeeId  and Tra_Approval.origin='ResourceRequest' and TraApproval_DocId=" + Resume_ResourceId + " ");
                var ApprovalRemark = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApprovalRemark).ToList();
                ViewBag.ApprovalRemarkList = ApprovalRemark;

                DynamicParameters paramBranch = new DynamicParameters();
                paramBranch.Add("@query", "SELECT BRANCHID as Id,BRANCHNAME as Name FROM MAS_BRANCH WHERE Deactivate=0");
                var paramBranch1 = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", paramBranch).ToList();
                ViewBag.BranchList = paramBranch1;

                //DynamicParameters paramEmp = new DynamicParameters();
                //paramEmp.Add("@query", "select EmployeeId as Id,concat(Mas_Employee.EmployeeName,' - ',Mas_Employee.EmployeeNo) as Name from Mas_Employee where EmployeeDepartmentID=" + PositionDetail.DepartmentID + " and ContractorID=1 and Deactivate=0 and Employeeleft=0 order by EmployeeName asc");
                //var Emp = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", paramEmp).ToList();
                //ViewBag.InterviewerEmployee = Emp;

                DynamicParameters param1 = new DynamicParameters();

                param1.Add("@p_DepartmentId", PositionDetail.DepartmentID);
                var GetInterviewerList = DapperORM.ExecuteSP<AllDropDownBind>("sp_RecruitmentInterviewerList", param1).ToList();
                ViewBag.InterviewerEmployee = GetInterviewerList;

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
        public ActionResult SaveUpdate(Recruitment_Interview RecruitmentInterview)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(RecruitmentInterview.InterviewId_Encrypted) ? "Save" : "Update");
                param.Add("@p_InterviewId", RecruitmentInterview.InterviewId);
                param.Add("@p_InterviewId_Encrypted", RecruitmentInterview.InterviewId_Encrypted);
                param.Add("@p_InterviewPoolEmployeeId", Session["EmployeeId"]);
                param.Add("@p_InterviewResumeId", Session["ResumeId"]);
                param.Add("@p_InterviewType", RecruitmentInterview.InterviewType);
                param.Add("@p_InterviewDate", RecruitmentInterview.InterviewDate);
                param.Add("@p_InterviewTime", RecruitmentInterview.InterviewTime);
                param.Add("@p_MeetingLink", RecruitmentInterview.MeetingLink);
                param.Add("@p_InterviewerEmployeeId", RecruitmentInterview.InterviewerEmployeeId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", "Admin");
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Recruitment_Interview", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_EncryptedId", RecruitmentInterview.ResumeId_Encrypted);
                param1.Add("@p_Origin", "ShortListed");
                var Data = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_Profiles", param1).FirstOrDefault();


                #region Send maail
                // Fetch SMTP details
                DynamicParameters paramToolEmail = new DynamicParameters();
                paramToolEmail.Add("@query", "SELECT CmpId, Origin, SMTPServerName, PortNo, SSL, FromEmailId, Password FROM Tool_EmailSetting WHERE Deactivate = 0 AND CmpId = '" + Session["CompanyId"] + "' AND Origin = '4'");
                var GetToolEmail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramToolEmail).FirstOrDefault();

                DynamicParameters paramApproverEmployeeId = new DynamicParameters();
                paramApproverEmployeeId.Add("@query", "select TraApproval_ApproverEmployeeId as ApproverEmployeeId  from Tra_Approval where Deactivate=0 and TraApproval_DocId=" + Data.ResourceId + " and TraApproval_ModuleId=3");
                var ApproverEmployeeId = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApproverEmployeeId).ToList();

                string approverIds = "";

                if (ApproverEmployeeId != null && ApproverEmployeeId.Count > 0)
                {
                    approverIds = string.Join(",", ApproverEmployeeId.Select(a => Convert.ToInt32(a.ApproverEmployeeId)));
                }
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
                paramResourceEmployeeId.Add("@query", "select ResourceEmployeeId from Recruitment_ResourceRequest where ResourceId=" + Data.ResourceId + " and  Deactivate=0 ");
                var ResourceEmployeeId = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramResourceEmployeeId).FirstOrDefault();
                paramResourceEmployeeId.Add("@query", "Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName,EmployeeId from MAs_employee,Mas_Designation where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID and  EmployeeId=" + ResourceEmployeeId.ResourceEmployeeId + "");
                var ResourceEmployeeIdEmail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramResourceEmployeeId).FirstOrDefault();
                DynamicParameters paramInterviewer = new DynamicParameters();
                paramInterviewer.Add("@query", "Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName,EmployeeId from MAs_employee,Mas_Designation where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID and  EmployeeId=" + RecruitmentInterview.InterviewerEmployeeId + "");
                var InterviewerEmployeeIdEmail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramInterviewer).FirstOrDefault();


                DynamicParameters paramId = new DynamicParameters();
                paramId.Add("@query", "Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName,EmployeeId from MAs_employee,Mas_Designation where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID and  EmployeeId=" + RecruitmentInterview.ReuestPoolRecruiterId + "");
                var PoolRecruiterEmail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramId).FirstOrDefault();

                if (PoolRecruiterEmail == null || string.IsNullOrEmpty(PoolRecruiterEmail.CompanyMailID))
                {
                  
                    return RedirectToAction("ESS_Recruitment_ShortListed", "ESS_Recruitment_ShortListed");
                }
                StringBuilder strBuilder = new StringBuilder();


                var CandidateMail = RecruitmentInterview.CandidateMail;
                if (GetToolEmail == null || string.IsNullOrEmpty(Convert.ToString(GetToolEmail.FromEmailId)))
                {
                  
                    return RedirectToAction("ESS_Recruitment_ShortListed", "ESS_Recruitment_ShortListed");
                }
                else
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
                                    else if (recipient == mailid1 && empIds.Count > 0)
                                    {
                                        Recruitment_EmailSendTo = Convert.ToInt32(empIds[0]);
                                    }
                                    else if (recipient == mailid2 && empIds.Count > 1)
                                    {
                                        Recruitment_EmailSendTo = Convert.ToInt32(empIds[1]);
                                    }
                                    else if (recipient == mailid3 && empIds.Count > 2)
                                    {
                                        Recruitment_EmailSendTo = Convert.ToInt32(empIds[2]);
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
                                mail.Subject = $"Interview Invitation for {RecruitmentInterview.designationname} Position at {RecruitmentInterview.CompanyName}";

                                mail.Body = $@"
                                            Dear {RecruitmentInterview.CandidateName}, <br><br>
                                            We are pleased to inform you that your application for the <b>{RecruitmentInterview.designationname}</b> position at <b>{RecruitmentInterview.CompanyName}</b> has been shortlisted.
                                            We would like to schedule an interview with you.<br><br>
                                           
                                            <b>Interview Details</b><br>
                                            <b>Interview Date:</b> {RecruitmentInterview.InterviewDate:dd/MMM/yyyy} <br>
                                            <b>Interview Time:</b>  {RecruitmentInterview.InterviewTime:hh:mm tt} <br>
                                            <b>Address/Link:</b> {RecruitmentInterview.InterviewType}/{RecruitmentInterview.MeetingLink} <br><br>
                                           
                                            <b>Interviewer(s):</b><br>
                                            1) {ResourceEmployeeIdEmail?.EmployeeName ?? "N/A"} <br>
                                            2) {namesCommaSeparated}  <br>
                                            3) {InterviewerEmployeeIdEmail?.EmployeeName ?? "N/A"} <br><br>
                                           
                                            Please confirm your availability. Looking forward to your confirmation.<br><br>
                                           
                                            Best regards,<br>
                                            {PoolRecruiterEmail?.EmployeeName ?? "HR"}<br>
                                            {GetToolEmail?.FromEmailId ?? ""}<br>
                                            {RecruitmentInterview.CompanyName}<br>
                                            {PoolRecruiterEmail?.CompanyMobileNo ?? ""}
                                            ";

                                mail.IsBodyHtml = true;
                                smtp.Send(mail);

                                // Console.WriteLine("Email sent successfully.");
                                return RedirectToAction("ESS_Recruitment_ShortListed", "ESS_Recruitment_ShortListed");
                            }
                        }
                        catch (Exception ex)
                        {
                            TempData["Message"] = "Error: " + ex.Message;
                            TempData["Icon"] = "error";
                        }
                    }
                    #endregion

                }
                return RedirectToAction("ESS_Recruitment_ShortListed", "ESS_Recruitment_ShortListed");
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
                    return RedirectToAction("ESS_Recruitment_ShortListed", "ESS_Recruitment_ShortListed", new { Area = "ESS" });
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ShortListed List VIew 
        [HttpGet]
        public ActionResult CandidateInterviewList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 132;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_Qry", "and Recruitment_Resume.ResumeStatus ='InterviewSchedule' and Recruitment_Interview.InterviewPoolEmployeeId=" + Session["EmployeeId"] + " ");
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


        public ActionResult CreateLinkView(string ResumeId_Encrypted, int? ResumeId, int? Resume_ResourceId,string UpdateOrView)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                Session["ResumeId"] = ResumeId;
                param.Add("@p_EncryptedId", ResumeId_Encrypted);
                param.Add("@p_Origin", "ShortListed");

                if(UpdateOrView != "Edit")
                {
                    ViewBag.UpdateOrView = "View";
                }
                else
                {
                    ViewBag.UpdateOrView = "Edit";
                }
                var GetShortListedData = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_Profiles", param).FirstOrDefault();
                ViewBag.ShortListedProfile = GetShortListedData;

                string Encrypted = ResumeId_Encrypted;

                DynamicParameters paramPosition = new DynamicParameters();
                paramPosition.Add("@query", "Select Mas_Designation.DesignationName, Mas_Department.DepartmentName ,Recruitment_ResourceRequest.DepartmentID  from Mas_Department,Mas_Designation,Recruitment_ResourceRequest  where Mas_Designation.DesignationId=Recruitment_ResourceRequest.DesignationID and  Mas_Department.DepartmentId=Recruitment_ResourceRequest.DepartmentID  and Mas_Department.Deactivate=0 and Mas_Designation.Deactivate=0 and Recruitment_ResourceRequest.Deactivate=0  and Recruitment_ResourceRequest.ResourceId =" + Resume_ResourceId + "");
                var PositionDetail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramPosition).FirstOrDefault();

                DynamicParameters paramApprovalRemark = new DynamicParameters();
                paramApprovalRemark.Add("@query", "Select ApproveRejectRemark,Mas_Employee.EmployeeName from Tra_Approval ,Mas_Employee where Tra_Approval.Deactivate=0 and Mas_Employee.Deactivate=0 and Tra_Approval.TraApproval_ApproverEmployeeId=Mas_Employee.EmployeeId  and Tra_Approval.origin='ResourceRequest' and TraApproval_DocId=" + Resume_ResourceId + " ");
                var ApprovalRemark = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApprovalRemark).ToList();
                ViewBag.ApprovalRemarkList = ApprovalRemark;

                DynamicParameters param1 = new DynamicParameters();

                param1.Add("@p_DepartmentId", PositionDetail.DepartmentID);
                var GetInterviewerList = DapperORM.ExecuteSP<AllDropDownBind>("sp_RecruitmentInterviewerList", param1).ToList();
                ViewBag.InterviewerEmployee = GetInterviewerList;

                DynamicParameters param2 = new DynamicParameters();
                Recruitment_Interview Interview = new Recruitment_Interview();
                param2.Add("@p_Qry", " and ResumeId_Encrypted='"+ ResumeId_Encrypted + "' and ResumeId ='"+ ResumeId + "' and Resume_ResourceId='"+ Resume_ResourceId + "'");
                Interview = DapperORM.ExecuteSP<Recruitment_Interview>(
                                "sp_List_Recruitment_InterviewCandidate",
                                param2
                            ).FirstOrDefault();

                DynamicParameters paramBranch = new DynamicParameters();
                paramBranch.Add("@query", "SELECT BRANCHID as Id,BRANCHNAME as Name FROM MAS_BRANCH WHERE Deactivate=0");
                var paramBranch1 = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", paramBranch).ToList();
                ViewBag.BranchList = paramBranch1;

                //Interview = DapperORM.ExecuteSP("sp_List_Recruitment_InterviewCandidate", param2);
                ViewBag.GetInterviewList = Interview;

                return View(Interview);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public JsonResult GetBranchAddress(int branchId)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
               
                var AddressByBranch1 = "SELECT BranchAddress FROM MAS_BRANCH WHERE Deactivate=0 and BranchId='"+ branchId + "'";
                var AddressByBranch = DapperORM.DynamicQuerySingle(AddressByBranch1);
                var BranchAddress = AddressByBranch.BranchAddress;

                return Json(new
                {
                    Address = BranchAddress
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
    }
}