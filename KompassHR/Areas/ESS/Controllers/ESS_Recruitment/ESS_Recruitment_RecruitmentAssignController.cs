using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.ESS.Models.ESS_Recruitment;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Net;
using System.Data;

using System.Text;
using System.Net.Mail;

namespace KompassHR.Areas.ESS.Controllers.ESS_Recruitment
{
    public class ESS_Recruitment_RecruitmentAssignController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();

        // GET: ESS/ESS_Recruitment_RecruitmentAssign
        #region ESS_Recruitment_RecruitmentAssign
        public ActionResult ESS_Recruitment_RecruitmentAssign()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 129;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_Origin", "Assign");
                var AssignRequest = DapperORM.DynamicList("sp_List_Recruitment_ESS_Approval_Assign_Pool", param);
                ViewBag.AssignRequest = AssignRequest;
                return View();

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ViewForRequestAssign
        public ActionResult ViewForRequestAssign(string ResourceId_Encrypted, int? DocNo)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var results = DapperORM.DynamicQueryList(@"select TeamAssignEmployeeId as Id, Mas_Employee.EmployeeName as Name from Recruitment_TeamAssign left join Mas_Employee on Recruitment_TeamAssign.TeamAssignEmployeeId=Mas_Employee.EmployeeId where Recruitment_TeamAssign.RecruitmentHeadEmployeeId=" + Session["EmployeeId"] + " and Mas_Employee.Deactivate=0 and Recruitment_TeamAssign.Deactivate=0");
                ViewBag.EmployeeName = results.Select(x => new AllDropDownClass
                {
                    Id = (double)x.Id,
                    Name = x.Name
                });

                Session["ResourceId_Encrypted"] = ResourceId_Encrypted;
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EncryptedId", ResourceId_Encrypted);
                paramList.Add("@p_Origin", "ApprovalRequest");
                var ResourceApproval = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_Profiles", paramList).FirstOrDefault();
                ViewBag.ResourceApprovalList = ResourceApproval;

                DynamicParameters paramDate = new DynamicParameters();
                paramDate.Add("@query", "select replace(convert(nvarchar(12),Recruitment_ResourceRequest.ClosingDate,106),' ','/') as ClosingDate   from Recruitment_ResourceRequest where ResourceId_Encrypted='" + ResourceId_Encrypted + "' and Deactivate=0");
                var ClosingDate = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramDate).ToList();
                ViewBag.ClosingDate = ClosingDate;

                DynamicParameters paramApprovalRemark = new DynamicParameters();
                paramApprovalRemark.Add("@query", "Select ApproveRejectRemark,Mas_Employee.EmployeeName from Tra_Approval ,Mas_Employee where Tra_Approval.Deactivate=0 and Mas_Employee.Deactivate=0 and Tra_Approval.TraApproval_ApproverEmployeeId=Mas_Employee.EmployeeId and TraApproval_DocId=" + DocNo + " and  Tra_Approval.origin='ResourceRequest'and Tra_Approval.Status='Approved'");
                var ApprovalRemark = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApprovalRemark).ToList();
                ViewBag.ApprovalRemarkList = ApprovalRemark;

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "select case when MaxBudget>0 then 'true' else 'false' end as BudgetApproved,MaxBudget,case when LegalBond=1 then'true' else 'false' end as  LegalBondApplicable ,LegalBondDuration from Recruitment_ResourceRequest where ResourceId_Encrypted='" + ResourceId_Encrypted + "' and Deactivate=0");
                var list = DapperORM.ExecuteSP<Recruitment_ResourceRequest_Update>("sp_QueryExcution", param1).ToList();
                ViewBag.ApproverList = list;

                DynamicParameters paramemp = new DynamicParameters();
                paramemp.Add("@query", "select TeamAssignEmployeeId as Id,concat(Mas_Employee.EmployeeName,' - ',Mas_Employee.EmployeeNo) as Name from Recruitment_TeamAssign left join Mas_Employee on Recruitment_TeamAssign.TeamAssignEmployeeId=Mas_Employee.EmployeeId where RecruitmentHeadEmployeeId=" + Session["EmployeeId"] + "  and Recruitment_TeamAssign.Deactivate=0");
                var ResponsibleRecruitment = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", paramemp).ToList();
                ViewBag.ResponsibleRecruitment = ResponsibleRecruitment;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region UpdateEployee
        public ActionResult UpdateEployee(int EmpolyeeId, DateTime date, bool BudgetApproved, int? MaxBudget, bool LegalBondApplicable, int? LegalBondDuration)
        {
            try
            {
                var ResourceId_Encrypted = Session["ResourceId_Encrypted"];
               // param.Add("@query", "update Recruitment_ResourceRequest set ReuestPoolRecruiterId = " + EmpolyeeId + " , ReuestPoolRecruiterDate = '" + Convert.ToDateTime(date).ToString("dd/MMM/yyyy") + "'  where ResourceId_Encrypted ='" + ResourceId_Encrypted + "'");
                // var Module = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).ToList();
                DapperORM.DynamicQuerySingle("update Recruitment_ResourceRequest set ReuestPoolRecruiterId = " + EmpolyeeId + " , ReuestPoolRecruiterDate = '" + Convert.ToDateTime(date).ToString("dd/MMM/yyyy") + "'  where ResourceId_Encrypted ='" + ResourceId_Encrypted + "'");
                //DynamicParameters param1 = new DynamicParameters();
                //param1.Add("@query", "update Recruitment_ResourceRequest set Assigned_ClosingDate='"+ Convert.ToDateTime(date).ToString("dd/MMM/yyyy") + "' , Assigned_EmployeeId="+EmpolyeeId+" , Assigned_LegalBond='"+LegalBondApplicable+"'  , Assigned_LegalBondMonth="+LegalBondDuration+" , Assigned_PositionBudget='"+BudgetApproved+"'  , Assigned_PositionMaxBudget="+MaxBudget+" where ResourceId_Encrypted='"+ResourceId_Encrypted+"'");
                //var Update = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param1).ToList();
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_process", "Update");
                param1.Add("@P_ResourceId_Encrypted", ResourceId_Encrypted);
                param1.Add("@P_Assigned_ClosingDate", Convert.ToDateTime(date).ToString("dd/MMM/yyyy"));
                param1.Add("@P_Assigned_BudgetApproved", BudgetApproved);
                param1.Add("@P_Assigned_MaxBudget", MaxBudget);
                param1.Add("@P_Assigned_LegalBondApplicable", LegalBondApplicable);
                param1.Add("@P_Assigned_LegalBondDuration", LegalBondDuration);
                param1.Add("@P_ResponsibleRecruitmentEmployeeId", EmpolyeeId);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Update_Recruitment_ResourceRequest", param1);


                #region Send MAil
                DynamicParameters paramApproverEmployeeId = new DynamicParameters();
                paramApproverEmployeeId.Add("@query", "Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName, EmployeeId,MAs_companyProfile.CompanyName,Recruitment_ResourceRequest.ResourceId from   MAs_employee,Mas_Designation,MAs_companyProfile,Recruitment_ResourceRequest  where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID    and MAs_companyProfile.CompanyId=MAs_employee.CmpId and   Recruitment_ResourceRequest.ReuestPoolRecruiterId=MAs_employee.EmployeeId  and Recruitment_ResourceRequest.Deactivate=0   and Recruitment_ResourceRequest.ResourceId_Encrypted ='" + ResourceId_Encrypted + "'");
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
                        paramPosition.Add("@query", "Select Mas_Designation.DesignationName, Mas_Department.DepartmentName  from Mas_Department,Mas_Designation,Recruitment_ResourceRequest  where Mas_Designation.DesignationId=Recruitment_ResourceRequest.DesignationID and  Mas_Department.DepartmentId=Recruitment_ResourceRequest.DepartmentID  and Mas_Department.Deactivate=0 and Mas_Designation.Deactivate=0 and Recruitment_ResourceRequest.Deactivate=0  and Recruitment_ResourceRequest.ResourceId_Encrypted ='" + ResourceId_Encrypted + "'");
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
                            MailMessage mail = new MailMessage(FromEmailId, ReuestPoolRecruiterMail.CompanyMailID);

                            mail.Subject = "Action Required: Recruitment Resource Request Assigned";
                            mail.Body = "Dear " + ReuestPoolRecruiterMail.EmployeeName + ",<br><br>I would like to bring to your attention the recruitment resource request submitted on <b>" + DateTime.Now.ToString("dd/MMM/yyyy") + "</b> for the <b>" + PositionDetail.DesignationName + "</b> within the <b>" + PositionDetail.DepartmentName + "</b> . As of today, A Recruitment Resource Request has been assigned to you.<br><br>" +
                                        "Kindly log in to " + GetUrl.Recruitment_URL + " and take the necessary action on the assigned resource request to move it forward.<br><br>" +
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
                return Json(true, JsonRequestBehavior.AllowGet);

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
                    return RedirectToAction("ESS_Recruitment_RecruitmentAssign", "ESS_Recruitment_RecruitmentAssign", new { Area = "ESS" });
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region RecruitmentAssignedList
        public ActionResult RecruitmentAssignedList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 129;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_Origin", "AssignList");
                var AssignRequest = DapperORM.DynamicList("sp_List_Recruitment_ESS_Approval_Assign_Pool", param);
                ViewBag.AssignRequestList = AssignRequest;
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