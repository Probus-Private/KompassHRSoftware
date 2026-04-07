using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Recruitment
{
    public class ESS_Recruitment_OfferLetterApprovalController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: ESS/ESS_Recruitment_OfferLetterApproval
        #region OfferLetterApproval MAin View 
        [HttpGet]
        public ActionResult ESS_Recruitment_OfferLetterApproval()
        {
            try
            {

                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 135;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_OfferLetterGenResourceEmployeeId", Session["EmployeeId"]);
                param.Add("@p_Origin", "OfferLetter");
                var OfferLetterApproval = DapperORM.DynamicList("sp_List_Recruitment_OfferLetterApproval", param);
                ViewBag.OfferLetterApprovalList = OfferLetterApproval;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ForOfferLetterApproval View 
        [HttpGet]
        public ActionResult ForOfferLetterApproval(string OfferLetterGenId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EncryptedId", OfferLetterGenId_Encrypted);
                paramList.Add("@p_Origin", "OfferLetterApproved");
                var OfferLetterApproval = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_Profiles", paramList).FirstOrDefault();
                ViewBag.OfferLetterApprovalList = OfferLetterApproval;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region ForOfferLetterApproval View 
        [HttpGet]
        public ActionResult SaveUpdate(string OfferLetterGenId_Encrypted, string Status, string Remark)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var GetUrl1 = "select Recruitment_URL  from Tool_CommonTable";
                var GetUrl = DapperORM.DynamicQuerySingle(GetUrl1);
                if (Status == "Approved")
                {
                    var Update = DapperORM.DynamicQuerySingle("Update Recruitment_OfferLetterGeneration set OffterLetterStatus='" + Status + "' where OfferLetterGenId_Encrypted='" + OfferLetterGenId_Encrypted + "'");

                    var OfferLetterGenResourceId = DapperORM.DynamicQuerySingle("Select ReuestPoolRecruiterId,ResourceId from Recruitment_ResourceRequest where ResourceId= (Select OfferLetterGenResourceId from Recruitment_OfferLetterGeneration where OfferLetterGenId_Encrypted='" + OfferLetterGenId_Encrypted + "')");
                   
                    DynamicParameters paramApproverEmployeeId = new DynamicParameters();
                    paramApproverEmployeeId.Add("@query", "Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName, EmployeeId,MAs_companyProfile.CompanyName  from MAs_employee,Mas_Designation,MAs_companyProfile where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID  and MAs_companyProfile.CompanyId=MAs_employee.CmpId and  EmployeeId =" + OfferLetterGenResourceId.ReuestPoolRecruiterId + "");
                    var ApproverEmployeeIdEmail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApproverEmployeeId).FirstOrDefault();
                   
                    if (ApproverEmployeeIdEmail != null &&
    !string.IsNullOrWhiteSpace(Convert.ToString(ApproverEmployeeIdEmail.CompanyMailID)))
                    {
                        
                        DynamicParameters paramApprover = new DynamicParameters();
                            paramApprover.Add("@query", "Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName, EmployeeId,MAs_companyProfile.CompanyName  from MAs_employee,Mas_Designation,MAs_companyProfile where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID and MAs_companyProfile.CompanyId=MAs_employee.CmpId and  EmployeeId =" + Session["EmployeeId"] + "");
                            var HRMail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApprover).FirstOrDefault();

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
                                MailMessage mail = new MailMessage(FromEmailId, ApproverEmployeeIdEmail.CompanyMailID);

                                mail.Subject = "Approved Offer Letter – Action Required";
                                mail.Body = "Dear " + ApproverEmployeeIdEmail.EmployeeName + ",<br><br>The offer letter request has been approved in the ESS system.<br><br>" +
                                            "Kindly log in to " + GetUrl.Recruitment_URL + " and download the approved offer letter from the pending requests section.<br><br>" +
                                            "Please ensure the document is reviewed and then forwarded to the respective candidate at the earliest.<br><br>Let us know once the offer has been sent or if any support is needed.<br><br>" +
                                            " Best regards,<br>" + HRMail.EmployeeName + "<br>" + HRMail.DesignationName + "<br>" + HRMail.CompanyName + "<br>" + "";


                                mail.IsBodyHtml = true;
                                smtp.Send(mail);
                            }

                            string Employee_Admin = $@"INSERT INTO Recruitment_EmailLog (Recruitment_EmailSendTo, Recruitment_EmailSendBy, Recruitment_ResourceId, Recruitment_EmailLogDate, EmailId,Origin) 
                                       VALUES ({ApproverEmployeeIdEmail.EmployeeId}, {Session["EmployeeId"]}, {OfferLetterGenResourceId.ResourceId }, GETDATE(), '{ApproverEmployeeIdEmail.CompanyMailID}','Employee');";

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
                      
                    
                    


                    TempData["Message"] = "Offer Letter approved succesfully";
                    TempData["Icon"] = "success";
                }
                else
                {
                    var Update = DapperORM.DynamicQuerySingle("Update Recruitment_OfferLetterGeneration set OffterLetterStatus='" + Status + "',OffterLetterRejectRemark='" + Remark + "' where OfferLetterGenId_Encrypted='" + OfferLetterGenId_Encrypted + "'");
                    TempData["Message"] = "Offer Letter rejected succesfully";
                    TempData["Icon"] = "success";
                    var OfferLetterGenResourceId = DapperORM.DynamicQuerySingle("Select ReuestPoolRecruiterId,ResourceId from Recruitment_ResourceRequest where ResourceId= (Select OfferLetterGenResourceId from Recruitment_OfferLetterGeneration where OfferLetterGenId_Encrypted='" + OfferLetterGenId_Encrypted + "')");

                    DynamicParameters paramApproverEmployeeId = new DynamicParameters();
                    paramApproverEmployeeId.Add("@query", "Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName, EmployeeId,MAs_companyProfile.CompanyName  from MAs_employee,Mas_Designation,MAs_companyProfile where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID  and MAs_companyProfile.CompanyId=MAs_employee.CmpId and  EmployeeId =" + OfferLetterGenResourceId.ReuestPoolRecruiterId + "");
                    var ApproverEmployeeIdEmail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApproverEmployeeId).FirstOrDefault();

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
                                MailMessage mail = new MailMessage(FromEmailId, ApproverEmployeeIdEmail.CompanyMailID);

                                mail.Subject = "Offer Letter – Action Required";
                                mail.Body = "Dear " + ApproverEmployeeIdEmail.EmployeeName + ",<br><br>The offer letter request has been approved in the ESS system.<br><br>" +
                                            "Kindly log in to " + GetUrl.Recruitment_URL + " and download the approved offer letter from the pending requests section.<br><br>" +
                                            "Please ensure the document is reviewed and then forwarded to the respective candidate at the earliest.<br><br>Let us know once the offer has been sent or if any support is needed.<br><br>" +
                                            " Best regards,<br>" + HRMail.EmployeeName + "<br>" + HRMail.DesignationName + "<br>" + HRMail.CompanyName + "<br>" + "";


                                mail.IsBodyHtml = true;
                                smtp.Send(mail);
                            }

                            string Employee_Admin = $@"INSERT INTO Recruitment_EmailLog (Recruitment_EmailSendTo, Recruitment_EmailSendBy, Recruitment_ResourceId, Recruitment_EmailLogDate, EmailId,Origin) 
                                       VALUES ({ApproverEmployeeIdEmail.EmployeeId}, {Session["EmployeeId"]}, {OfferLetterGenResourceId.ResourceId }, GETDATE(), '{ApproverEmployeeIdEmail.CompanyMailID}','Employee');";

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

                    
                }
                return RedirectToAction("ESS_Recruitment_OfferLetterApproval", "ESS_Recruitment_OfferLetterApproval");

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region OfferLetterApproval MAin View 
        [HttpGet]
        public ActionResult OfferLetterApprovedList()
        {
            try
            {

                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 135;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_OfferLetterGenResourceEmployeeId", Session["EmployeeId"]);
                param.Add("@p_Origin", "OfferLetterApproved");
                var OfferLetterApproval = DapperORM.DynamicList("sp_List_Recruitment_OfferLetterApproval", param);
                ViewBag.OfferLetterApprovalList = OfferLetterApproval;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ForOfferLetterApproval View 
        [HttpGet]
        public ActionResult ForOfferLetterRevertProcess(string OfferLetterGenId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EncryptedId", OfferLetterGenId_Encrypted);
                paramList.Add("@p_Origin", "OfferLetterRevertProcess");
                var OfferLetterApproval = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_Profiles", paramList).FirstOrDefault();
                ViewBag.OfferLetterApprovalRevert = OfferLetterApproval;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Revised offer letter

        public ActionResult RevisedOfferLetter(string OfferLetterGenId_Encrypted,int? OfferLetterGenResumeId,string Remark)
        {
            try
            {
                var UpdateOffer = DapperORM.DynamicQuerySingle("Update Recruitment_OfferLetterGeneration set Deactivate=1,OffterLetterStatus='Revised',OffterLetterRejectRemark='" + Remark + "' where OfferLetterGenId_Encrypted='" + OfferLetterGenId_Encrypted + "'");               
                var UpdateResume = DapperORM.DynamicQuerySingle("Update Recruitment_Resume set OfferLetterId = 0 where ResumeId = '"+ OfferLetterGenResumeId + "'");
                TempData["Message"] = "Offer Letter revised succesfully";
                TempData["Icon"] = "success";
                return RedirectToAction("ESS_Recruitment_OfferLetterApproval", "ESS_Recruitment_OfferLetterApproval");
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