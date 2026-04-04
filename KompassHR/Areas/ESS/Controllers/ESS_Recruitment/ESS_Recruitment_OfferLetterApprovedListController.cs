using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using SelectPdf;

namespace KompassHR.Areas.ESS.Controllers.ESS_Recruitment
{
    public class ESS_Recruitment_OfferLetterApprovedListController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_Recruitment_OfferLetterApprovedList

        #region OfferLetterApprovedList Main View
        [HttpGet]
        public ActionResult ESS_Recruitment_OfferLetterApprovedList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 136;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var OfferLetterApproved = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_OfferLetterApproved").ToList();
                ViewBag.OfferLetterApprovedList = OfferLetterApproved;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetOfferTemplate
        public ActionResult GetOfferTemplate(string OfferLetterGenResumeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 136;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var a = DapperORM.DynamicQuerySingle("Select OfferLetterTemplate,OfferLetterGenResumeId from Recruitment_OfferLetterGeneration where OfferLetterGenId_Encrypted='" + OfferLetterGenResumeId + "'");
                var OfferLetterTemplate = a.OfferLetterTemplate;
                var OfferLetterResumeId = a.OfferLetterGenResumeId;
                return Json(new { OfferLetterResumeId = OfferLetterResumeId, OfferLetterTemplate = OfferLetterTemplate }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Code For Send Mail
        public class PDFRequest
        {
            public string HtmlContent { get; set; }
        }
        [HttpPost]
        public JsonResult GenerateAndSendEmail(PDFRequest request, int? OfferLetterResumeId)
        {
            try
            {
                if (string.IsNullOrEmpty(request.HtmlContent))
                {
                    return Json(new { success = false, error = "HTML content is empty." });
                }

                var GetEmpEmailAndName = DapperORM.DynamicQuerySingle("Select EmailId from Recruitment_Resume where ResumeId=" + OfferLetterResumeId + "");


                // Generate PDF from HTML
                var converter = new HtmlToPdf();
                var pdfDocument = converter.ConvertHtmlString(request.HtmlContent);
                string pdfPath = Server.MapPath("~/Temp/OfferLetter.pdf");
                pdfDocument.Save(pdfPath);
                pdfDocument.Close();

                DynamicParameters paramToolEmail = new DynamicParameters();
                paramToolEmail.Add("@query", "Select CmpId, Origin, SMTPServerName, PortNo, SSL, FromEmailId, Password from Tool_EmailSetting where  Deactivate=0 and CmpId=" + Session["CompanyId"] + "  and  Origin='" + 4 + "'");
                var GetToolEmail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramToolEmail).FirstOrDefault();


                DynamicParameters paramApprover = new DynamicParameters();
                paramApprover.Add("@query", "Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName, EmployeeId,MAs_companyProfile.CompanyName  from MAs_employee,Mas_Designation,MAs_companyProfile where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID and MAs_companyProfile.CompanyId=MAs_employee.CmpId and  EmployeeId =" + Session["EmployeeId"] + "");
                var HRMail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApprover).FirstOrDefault();


                DynamicParameters paramCan = new DynamicParameters();
                paramCan.Add("@query", "Select Recruitment_Resume.CandidateName,Mas_Designation.DesignationName,Mas_CompanyProfile.CompanyName from Recruitment_Resume,Recruitment_ResourceRequest,Mas_Designation,Mas_CompanyProfile where  Recruitment_ResourceRequest.ResourceId=Recruitment_Resume.Resume_ResourceId and Recruitment_Resume.Deactivate=0 and Recruitment_ResourceRequest.Deactivate=0 and  Mas_Designation.DesignationId=Recruitment_ResourceRequest.DesignationID and Mas_CompanyProfile.CompanyId=Recruitment_ResourceRequest.CmpId and Recruitment_Resume.ResumeId=" + OfferLetterResumeId + " ");
                var paramCanMail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramCan).FirstOrDefault();

                // var ServerDetail = DapperORM.DynamicQuerySingle("Select CmpId, Origin, SMTPServerName, PortNo, SSL, FromEmailId, Password from Tool_EmailSetting where  Deactivate=0 and CmpId='" + GetCMPID.CmpID + "'  and  Origin='" + 1 + "' ");
                if (GetToolEmail != null)
                {
                    var SMTPServerName = GetToolEmail.SMTPServerName;
                    var PortNo = GetToolEmail.PortNo;
                    var FromEmailId = GetToolEmail.FromEmailId;
                    var SMTPPassword = GetToolEmail.Password;
                    var SSL = GetToolEmail.SSL;

                    SmtpClient smtp = new SmtpClient(SMTPServerName, PortNo);
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = true;
                    smtp.Timeout = 100000;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                    smtp.Credentials = new System.Net.NetworkCredential(FromEmailId, SMTPPassword);

                    MailMessage mail = new MailMessage(FromEmailId, GetEmpEmailAndName.EmailId);
                    mail.Subject = "Congratulations! Your Offer Letter from " + paramCanMail.CompanyName + "";
                    mail.Body = "Dear " + paramCanMail.CandidateName + ",<br><br>We are pleased to inform you that after careful evaluation, we would like to extend to you an offer for the position of " + paramCanMail.DesignationName + " at " + paramCanMail.CompanyName + ".<br>" +
                                    "<br>Attached to this email, you will find your formal offer letter detailing the terms and conditions of your employment, including your compensation, benefits, and start date.<br> <br>Please review the offer letter carefully. If everything is in order, kindly sign and return a scanned copy by 15 days. Should you have any questions or need clarification on any part of the offer, feel free to reach out to us at " + HRMail.CompanyMobileNo + ".<br> <br>" +
                                    "We are excited about the possibility of you joining our team and contributing to our mutual success.<br><br> Looking forward to hearing from you soon!<br><br>" +
                                    " Warm regards,<br>" + HRMail.EmployeeName + "<br>" + HRMail.DesignationName + "<br>" + HRMail.CompanyName + "<br>" + HRMail.CompanyMailID + "<br>" + HRMail.CompanyMobileNo + "" + "";



                    mail.IsBodyHtml = true;

                    // Attach the PDF
                    mail.Attachments.Add(new Attachment(pdfPath));

                    smtp.Send(mail);
                    // Dispose of the attachment to release the file lock
                    mail.Attachments.Dispose();
                    // Clean up the PDF file
                    if (System.IO.File.Exists(pdfPath))
                    {
                        System.IO.File.Delete(pdfPath);
                    }
                }
                //return Json(data, JsonRequestBehavior.AllowGet);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        #endregion

        #region GetAppointmentLetter
        public ActionResult GetAppointmentLetter()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 136;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
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
    }
}