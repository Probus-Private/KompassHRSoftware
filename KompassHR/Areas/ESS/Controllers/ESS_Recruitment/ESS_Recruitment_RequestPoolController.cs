using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Recruitment;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using SelectPdf;
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

namespace KompassHR.Areas.ESS.Controllers.ESS_Recruitment
{
    public class ESS_Recruitment_RequestPoolController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: ESS/ESS_Recruitment_RequestPool
        #region RequestPool Main VIew
        [HttpGet]
        public ActionResult ESS_Recruitment_RequestPool()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 130;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_Origin", "PoolRecruiter");
                var RequestPool = DapperORM.DynamicList("sp_List_Recruitment_ESS_Approval_Assign_Pool", param);
                ViewBag.RequestPool = RequestPool;
                Session["AssignForShortlistResourceId"] = null;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ViewForRequestPool View
        [HttpGet]
        public ActionResult ViewForRequestPool(string ResourceId_Encrypted, int? ResourceId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                Session["ResourceId"] = ResourceId;
                Session["ResourceId_Encrypted"] = ResourceId_Encrypted;
                param.Add("@p_EncryptedId", ResourceId_Encrypted);
                param.Add("@p_Origin", "ApprovalRequest");
                var ResourcePool = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_Profiles", param).FirstOrDefault();
                if (ResourcePool != null)
                {
                    ViewBag.ResourceApprovalList = ResourcePool;
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
        #endregion

        #region GetInvoice
        [HttpGet]
        public ActionResult GetInvoice(string ResourceId_Encrypted, int? ResourceId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 130;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Session["ResourceId"] = ResourceId;
                Session["ResourceId_Encrypted"] = ResourceId_Encrypted;
                param.Add("@p_EncryptedId", ResourceId_Encrypted);
                param.Add("@p_Origin", "ApprovalRequest");
                var ResourcePool = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_Profiles", param).FirstOrDefault();
                if (ResourcePool != null)
                {
                    ViewBag.ResourceApprovalList = ResourcePool;
                }
                else
                {
                    ViewBag.ResourceApprovalList = "";
                }
                DynamicParameters paramApprovalRemark = new DynamicParameters();
                paramApprovalRemark.Add("@query", "Select ApproveRejectRemark,Mas_Employee.EmployeeName from Tra_Approval ,Mas_Employee where Tra_Approval.Deactivate=0 and Mas_Employee.Deactivate=0 and Tra_Approval.TraApproval_ApproverEmployeeId=Mas_Employee.EmployeeId and TraApproval_DocId=" + ResourceId + "and Tra_Approval.origin='ResourceRequest'and Tra_Approval.Status='Approved'");
                var ApprovalRemark = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApprovalRemark).ToList();
                ViewBag.ApprovalRemarkList = ApprovalRemark;

                DynamicParameters paramAgency = new DynamicParameters();
                paramAgency.Add("@query", "select AgencyId as Id,AgencyName as Name from Recruitment_Agency where Deactivate=0");
                var AgencyAssign = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", paramAgency).ToList();
                ViewBag.AgencyAssign = AgencyAssign;
                return View();

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetAgencyEmail
        public ActionResult GetAgencyEmail(int AgencyAssignId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@query", "select EmailAddress from Recruitment_Agency where AgencyId=" + AgencyAssignId + " and Deactivate=0");
                var mail = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).FirstOrDefault();

                return Json(new { success = true, mail = mail?.EmailAddress ?? "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion


        #region CandidateListForAssign Shortlist View
        public ActionResult CandidateAssignForShortlist(string ResourceId_Encrypted, int? ResourceId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@p_Resume_ResourceId", ResourceId);
                Session["AssignForShortlistResourceId"] = ResourceId;
                var CandidateData = DapperORM.DynamicList("sp_List_Recruitment_Resume_PoolAssignCandidate", param);
                ViewBag.CandidateDataList = CandidateData;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region CandidateListForAssign Shortlist 
        [HttpPost]
        public ActionResult CandidateSendForShotList(List<ShortList> Task)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                if (Task != null)
                {
                    for (var i = 0; i < Task.Count; i++)
                    {
                        var Update = DapperORM.DynamicQuerySingle("Update Recruitment_Resume set  SendForShortListed=1 where ResumeId=" + Task[i].ResumeId + "");
                    }
                    var Message = "Selected candidate has been sent to the requester for shortlisting.";
                    var Icon = "success";


                    #region Send MAil
                    DynamicParameters paramApproverEmployeeId = new DynamicParameters();
                    paramApproverEmployeeId.Add("@query", "Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName, EmployeeId,MAs_companyProfile.CompanyName  from MAs_employee,Mas_Designation,MAs_companyProfile,Recruitment_ResourceRequest where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID   and MAs_companyProfile.CompanyId=MAs_employee.CmpId and   Mas_Employee.EmployeeId=Recruitment_ResourceRequest.ResourceEmployeeId and Recruitment_ResourceRequest.Deactivate=0 and Mas_Employee.Deactivate=0   and Recruitment_ResourceRequest.ResourceId =" + Session["AssignForShortlistResourceId"] + "");
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
                        paramPosition.Add("@query", "Select Mas_Designation.DesignationName, Mas_Department.DepartmentName  from Mas_Department,Mas_Designation,Recruitment_ResourceRequest  where Mas_Designation.DesignationId=Recruitment_ResourceRequest.DesignationID and  Mas_Department.DepartmentId=Recruitment_ResourceRequest.DepartmentID  and Mas_Department.Deactivate=0 and Mas_Designation.Deactivate=0 and Recruitment_ResourceRequest.Deactivate=0  and Recruitment_ResourceRequest.ResourceId=" + Session["AssignForShortlistResourceId"] + "");
                        var PositionDetail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramPosition).FirstOrDefault();

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
                            MailMessage mail = new MailMessage();
                            mail.From = new MailAddress(FromEmailId);
                            mail.To.Add(Convert.ToString(ReuestPoolRecruiterMail.CompanyMailID)); mail.Subject = " Action Required: Candidate Shortlisting Pending";
                            mail.Body = "Dear " + ReuestPoolRecruiterMail.EmployeeName + ",<br><br>I would like to bring to your attention the recruitment resource request submitted on <b>" + DateTime.Now.ToString("dd/MMM/yyyy") + "</b> for the <b>" + PositionDetail.DesignationName + "</b> within the <b>" + PositionDetail.DepartmentName + "</b> . A candidate has been uploaded against a Recruitment Resource Request and is pending your action for shortlisting.<br><br>" +
                                        "Kindly log in to " + GetUrl.Recruitment_URL + " and take the necessary action on the candidate uploaded against the Resource Request to move the process forward.<br><br>" +
                                        "Your prompt attention to this matter would be greatly appreciated. Please let me know if any additional information or documentation is required to facilitate process.<br><br>Thank you for your consideration.<br><br>" +
                                        "Best regards,<br>" + ResourceRequest.EmployeeName + "<br>" + ResourceRequest.DesignationName + "<br>" + ResourceRequest.CompanyName + "<br>" + "";


                            mail.IsBodyHtml = true;
                            smtp.Send(mail);
                        }

                        string Employee_Admin = $@"INSERT INTO Recruitment_EmailLog (Recruitment_EmailSendTo, Recruitment_EmailSendBy, Recruitment_ResourceId, Recruitment_EmailLogDate, EmailId,Origin) 
                                       VALUES ({ReuestPoolRecruiterMail.EmployeeId}, {Session["EmployeeId"]}, {Session["AssignForShortlistResourceId"] }, GETDATE(), '{ReuestPoolRecruiterMail.CompanyMailID}','Employee');";

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


                    return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    var Message = "Please select candidate";
                    var Icon = "error";
                    return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
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
        public ActionResult DownloadFile(string ResourceRequest)
        {
            try
            {
                //var GetDrivePath = (from d in dbContext.Onboarding_Mas_DocumentPath select d.DosumentPath).FirstOrDefault();
                //var fullPath = GetDrivePath + filePath;
                //byte[] fileBytes = GetFile(fullPath);
                if (ResourceRequest != null)
                {
                    System.IO.File.ReadAllBytes(ResourceRequest);
                    return File(ResourceRequest, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(ResourceRequest));
                }
                else
                {
                    return RedirectToAction("ESS_Recruitment_RequestPool", "ESS_Recruitment_RequestPool", new { Area = "ESS" });
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region CandidateList data Resource wise 
        public ActionResult CandidateDataGetByResourceId()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 130;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_EncryptedId", Session["ResourceId_Encrypted"]);
                param.Add("@p_Origin", "CandidateDataResourceId");
                var ResourceIdWise = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_Profiles", param).ToList();
                if (ResourceIdWise != null)
                {
                    ViewBag.ResourcIdWise = ResourceIdWise;
                }
                else
                {
                    ViewBag.ResourceApprovalList = "";
                }
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region All Candidate Data 
        public ActionResult AllCandidateData()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 130;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_ResumeId_Encrypted", "List");
                var CandidateData = DapperORM.DynamicList("sp_List_Recruitment_Resume_AllCandidate", param);
                ViewBag.CandidateData = CandidateData;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region AssignResourceToAllCandidate
        public ActionResult AssignResourceToAllCandidate(List<AssignResourceIdToAllCandidate> Record)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                if (Record != null)
                {
                    for (var i = 0; i < Record.Count; i++)
                    {
                        DapperORM.DynamicQuerySingle("Update Recruitment_Resume set  Resume_ResourceId=" + Session["ResourceId"] + " where ResumeId_Encrypted='" + Record[i].ResumeId_Encrypted + "'");
                    }
                    var Message = "Candidate Uploaded";
                    var Icon = "success";
                    return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    var Message = "Please select candidate";
                    var Icon = "error";
                    return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region BulkUploadCandidateList
        public ActionResult BulkUploadCandidateList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region SendMail
        public class PDFRequest
        {
            public string HtmlContent { get; set; }
            public string AgencyEmail { get; set; }
            public string textAgencyEmail { get; set; }
            public string AgencyId { get; set; }

        }

        [HttpPost]
        public ActionResult SendMail(PDFRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.AgencyEmail))
                {
                    return Json(new { success = false, error = "Recipient email is required." });
                }
                var converter = new HtmlToPdf();
                var pdfDocument = converter.ConvertHtmlString(request.HtmlContent);
                string pdfPath = Server.MapPath("~/Temp/JobDiscription.pdf");
                pdfDocument.Save(pdfPath);
                pdfDocument.Close();


                param.Add("@p_EncryptedId", Session["ResourceId_Encrypted"]);
                param.Add("@p_Origin", "ApprovalRequest");
                var ResourcePool = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_Profiles", param).FirstOrDefault();

                // Excel File Path
                string excelPath = Path.Combine(Server.MapPath("~/assets/BulkUpload"), "BulkCandidate.xlsx");

                // Fetch SMTP details securely using a parameterized query
                DynamicParameters paramToolEmail = new DynamicParameters();
                paramToolEmail.Add("@query", "Select CmpId, Origin, SMTPServerName, PortNo, SSL, FromEmailId, Password from Tool_EmailSetting where  Deactivate=0 and CmpId=" + Session["CompanyId"] + "  and  Origin='" + 4 + "'");
                var GetToolEmail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramToolEmail).FirstOrDefault();

                if (GetToolEmail == null)
                {
                    return Json(new { success = false, error = "SMTP settings not found." });
                }

                string SMTPServerName = GetToolEmail.SMTPServerName;
                int PortNo = GetToolEmail.PortNo;
                string FromEmailId = GetToolEmail.FromEmailId;
                string SMTPPassword = GetToolEmail.Password;
                bool SSL = GetToolEmail.SSL;
                StringBuilder strBuilder = new StringBuilder();
                using (SmtpClient smtp = new SmtpClient(SMTPServerName, PortNo))
                {
                    smtp.EnableSsl = SSL;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(FromEmailId, SMTPPassword);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Timeout = 100000;

                    using (MailMessage mail = new MailMessage(FromEmailId, request.AgencyEmail))
                    {
                        mail.Subject = "Request for Candidate List – " + ResourcePool.DesignationName + " Position at " + ResourcePool.CompanyName + "";
                        mail.Body = "Dear " + request.textAgencyEmail + ",<br><br>I hope you're doing well. We are currently hiring for the position of " + ResourcePool.DesignationName + " at " + ResourcePool.CompanyName + ", and we would like your assistance in sourcing suitable candidates for this role.<br>" +
                                    "<br><b>Job Details:</b><br><b>Position:</b> " + ResourcePool.DesignationName + "<br><b> Location: </b> " + ResourcePool.BranchAddress + " <br><b>Employment Type:</b> " + ResourcePool.WorkType + " <br>" +
                                    " <br> For your reference, we have attached a detailed job profile PDF file outlining all role requirements and expectations.Additionally, please find attached an Excel file where you can list potential candidates along with their details." +
                                    " We would appreciate it if you could review the job profile and share a list of suitable candidates at your earliest convenience.Feel free to reach out if you have any questions or require further information." +
                                    "Looking forward to your prompt response.<br><br> Best regards,<br> " + Session["EmployeeName"] + "<br>" + Session["DepartmentName"] + "<br>" + ResourcePool.CompanyName + "";

                        mail.IsBodyHtml = true;

                        string Employee_Admin = $@"INSERT INTO Recruitment_EmailLog (Recruitment_EmailSendTo, Recruitment_EmailSendBy, Recruitment_ResourceId, Recruitment_EmailLogDate, EmailId,Origin) 
                                       VALUES ({request.AgencyId}, {Session["EmployeeId"]}, {ResourcePool.ResourceId}, GETDATE(), '{request.AgencyEmail}','Agency');";

                        strBuilder.Append(Employee_Admin);


                        string abc = "";
                        if (objcon.SaveStringBuilder(strBuilder, out abc))
                        {

                            // TempData["Message"] = "Record Update successfully";
                            // TempData["Icon"] = "success";
                        }
                        if (abc != "")
                        {
                            TempData["Message"] = abc;
                            TempData["Icon"] = "error";
                        }

                        // Attach PDF
                        if (System.IO.File.Exists(pdfPath))
                        {
                            mail.Attachments.Add(new Attachment(pdfPath));
                        }
                        else
                        {
                            return Json(new { success = false, error = "PDF file not found." });
                        }

                        // Attach Excel File
                        if (System.IO.File.Exists(excelPath))
                        {
                            mail.Attachments.Add(new Attachment(excelPath));
                        }
                        else
                        {
                            return Json(new { success = false, error = "Excel attachment file not found." });
                        }

                        try
                        {
                            smtp.Send(mail);
                        }
                        catch (SmtpException smtpEx)
                        {
                            return Json(new { success = false, error = "SMTP Error: " + smtpEx.InnerException });
                        }
                    }
                }

                // Delete PDF after sending email
                if (System.IO.File.Exists(pdfPath))
                {
                    System.IO.File.Delete(pdfPath);
                }

                return Json(new { success = true, message = "Email sent successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Error: " + ex.Message });
            }
        }


        #endregion

    }
}