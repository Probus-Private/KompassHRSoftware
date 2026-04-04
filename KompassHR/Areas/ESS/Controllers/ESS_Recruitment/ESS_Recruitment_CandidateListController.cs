using Dapper;
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

namespace KompassHR.Areas.ESS.Controllers.ESS_Recruitment
{
    public class ESS_Recruitment_CandidateListController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: ESS/ESS_Recruitment_CandidateList
        #region CandidateList MAin VIew
        [HttpGet]
        public ActionResult ESS_Recruitment_CandidateList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 131;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_Resume_ResourceEmployeeId", Session["EmployeeId"]);
                param.Add("@p_Origin", "CandidateList");
                var CandidateData = DapperORM.DynamicList("sp_List_Recruitment_SendCandidate", param);
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

        #region CandidateForShortListing View
        [HttpGet]
        public ActionResult CandidateForShortListing(string ResumeId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EncryptedId", ResumeId_Encrypted);
                paramList.Add("@p_Origin", "CandidateList");
                var CandidateForShortList = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_Profiles", paramList).FirstOrDefault();
                if (CandidateForShortList != null)
                {
                    ViewBag.GetCandidateForShortList = CandidateForShortList;
                }
                else
                {
                    ViewBag.GetCandidateForShortList = "";
                }
                var GetDoc = DapperORM.DynamicQueryList("Select DocNo from Recruitment_ResourceRequest where ResourceId=(Select Resume_ResourceId from Recruitment_Resume where ResumeId_Encrypted='" + ResumeId_Encrypted + "')").FirstOrDefault();
                var DocNo = GetDoc?.DocNo;
                DynamicParameters paramApprovalRemark = new DynamicParameters();
                paramApprovalRemark.Add("@query", "Select ApproveRejectRemark,Mas_Employee.EmployeeName from Tra_Approval ,Mas_Employee where Tra_Approval.Deactivate=0 and Mas_Employee.Deactivate=0 and Tra_Approval.TraApproval_ApproverEmployeeId=Mas_Employee.EmployeeId and TraApproval_DocId=" + DocNo + " and  Tra_Approval.origin='ResourceRequest'");
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

        #region CandidateShortListed 
        [HttpGet]
        public ActionResult CandidateShortListed(string ResumeId_Encrypted, string Status, string Remark)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                if (ResumeId_Encrypted != null)
                {
                    var Update = DapperORM.DynamicQuerySingle("Update Recruitment_Resume set ResumeStatus='" + Status + "',LastCallByRecruiterRemark='" + Remark + "' where ResumeId_Encrypted='" + ResumeId_Encrypted + "'");
                    //var Update = DapperORM.DynamicQuerySingle("Update Recruitment_Resume set ResumeStatus='ShortListed' where ResumeId_Encrypted='"+ ResumeId_Encrypted + "'");
                    TempData["Message"] = "Candidate " + Status + "";
                    TempData["Icon"] = "success";

                    #region Send MAil
                    DynamicParameters paramApproverEmployeeId = new DynamicParameters();
                    paramApproverEmployeeId.Add("@query", "Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName,  EmployeeId,MAs_companyProfile.CompanyName, Recruitment_ResourceRequest.ResourceId from MAs_employee,Mas_Designation,MAs_companyProfile,Recruitment_ResourceRequest ,Recruitment_Resume where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID   and MAs_companyProfile.CompanyId=MAs_employee.CmpId and Recruitment_ResourceRequest.ReuestPoolRecruiterId=Mas_Employee.EmployeeId and  Recruitment_Resume.Resume_ResourceId=Recruitment_ResourceRequest.ResourceId and Recruitment_ResourceRequest.Deactivate=0 and Mas_Employee.Deactivate=0 and Recruitment_Resume.Deactivate=0 and Recruitment_Resume.ResumeId_Encrypted='" + ResumeId_Encrypted + "'");
                    var ReuestPoolRecruiterMail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApproverEmployeeId).FirstOrDefault();
                    var GetUrl1 = "select Recruitment_URL  from Tool_CommonTable";
                    var GetUrl = DapperORM.DynamicQuerySingle(GetUrl1);
                    if (ReuestPoolRecruiterMail != null && !string.IsNullOrWhiteSpace(Convert.ToString(ReuestPoolRecruiterMail.CompanyMailID)))
                    {
                        DynamicParameters paramApprover = new DynamicParameters();
                        paramApprover.Add("@query", "Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName, EmployeeId,MAs_companyProfile.CompanyName  from MAs_employee,Mas_Designation,MAs_companyProfile where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID and MAs_companyProfile.CompanyId=MAs_employee.CmpId and  EmployeeId =" + Session["EmployeeId"] + "");
                        var ResourceRequest = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApprover).FirstOrDefault();

                        DynamicParameters paramPosition = new DynamicParameters();
                        paramPosition.Add("@query", "Select Mas_Designation.DesignationName, Mas_Department.DepartmentName  ,Recruitment_ResourceRequest.ResourceId   from Mas_Department,Mas_Designation,Recruitment_ResourceRequest ,Recruitment_Resume where Mas_Designation.DesignationId=Recruitment_ResourceRequest.DesignationID and Mas_Department.DepartmentId=Recruitment_ResourceRequest.DepartmentID and  Recruitment_Resume.Resume_ResourceId=Recruitment_ResourceRequest.ResourceId and Mas_Department.Deactivate=0 and Mas_Designation.Deactivate=0 and Recruitment_ResourceRequest.Deactivate=0  and Recruitment_Resume.Deactivate=0 and Recruitment_Resume.ResumeId_Encrypted='" + ResumeId_Encrypted + "'");
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
                            mail.To.Add(Convert.ToString(ReuestPoolRecruiterMail.CompanyMailID));
                            mail.Subject = " Action Required: Candidate Shortlisted";
                            mail.Body = "Dear " + ReuestPoolRecruiterMail.EmployeeName + ",<br><br>I would like to bring to your attention the recruitment resource request submitted on " + DateTime.Now.ToString("dd/MMM/yyyy") + " for the " + PositionDetail.DesignationName + " within the " + PositionDetail.DepartmentName + " . A candidate has been shortlisted for a Recruitment Resource Request and is awaiting your action to proceed with the next steps.<br><br>" +
                                        "Kindly log in to " + GetUrl.Recruitment_URL + " and take the required action on the candidate shortlisted under the Resource Request to move the recruitment process ahead.<br><br>" +
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

                }
                else
                {
                    TempData["Message"] = "Candidate not shortlisted";
                    TempData["Icon"] = "error";
                    return RedirectToAction("ESS_Recruitment_CandidateList", "ESS_Recruitment_CandidateList");
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
                    return RedirectToAction("ESS_Recruitment_CandidateList", "ESS_Recruitment_CandidateList", new { Area = "ESS" });
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region CandidateShortListed 
        [HttpGet]
        public ActionResult CandidateShortlistedViewForRequester()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 131;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_Resume_ResourceEmployeeId", Session["EmployeeId"]);
                param.Add("@p_Origin", "ShortlistedList");
                var CandidateDatas = DapperORM.DynamicList("sp_List_Recruitment_SendCandidate", param);
                ViewBag.CandidateDataListView = CandidateDatas;
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