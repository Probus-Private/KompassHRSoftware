using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.ESS.Models.ESS_Recruitment;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IO;
using KompassHR.Areas.Setting.Models.Setting_Leave;
using KompassHR.Areas.Setting.Models.Setting_Prime;
using System.Text;
using System.Net.Mail;

namespace KompassHR.Areas.ESS.Controllers.ESS_Recruitment
{
    public class ESS_Recruitment_ResourcesRequisitionController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: ESS/ESS_Resources_Recruitment

        #region ResourcesRequisition Main View
        [ValidateInput(false)]
        public ActionResult ESS_Recruitment_ResourcesRequisition(string ResourceId_Encrypted, int? CmpId, int? DesignationId, int? DepartmentId,string EditOrView)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 127;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Recruitment_ResourceRequest ResourceRequest = new Recruitment_ResourceRequest();
                if(EditOrView!=null)
                {
                    ViewBag.EditOrView = EditOrView;
                }
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.ComapnyName = GetComapnyName;

                var results = DapperORM.DynamicQueryMultiple(@"SELECT  DepartmentId as Id,DepartmentName as Name FROM Mas_Department WHERE Deactivate =0;
                                                     SELECT DesignationId  as Id, DesignationName as Name FROM Mas_Designation WHERE Deactivate =0 and Mas_Designation.RecruitmentApplicable=1
                                                     SELECT GradeId as Id,GradeName as Name FROM Mas_Grade WHERE Deactivate =0
                                                     select WorkTypeID as Id,WorkType as Name from Recruitment_WorkType where Deactivate=0 
                                                     select PositionTypeID as Id,PositionType as Name from Recruitment_PositionType where Deactivate=0
                                                     Select RecruitmentJDTemplateID as Id ,RecruitmentJDTemplate as Name from Recruitment_JobDescriptionTemplate where Deactivate=0");
                ViewBag.DepatmentList = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.DesignationList = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GradeList = results[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.WorkType = results[3].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.PositionType = results[4].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.JobDescriptionTemplate = results[5].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                DynamicParameters paramCurrency = new DynamicParameters();
                paramCurrency.Add("@query", "select CurrencyId as Id,CurrencyName As Name from Mas_Currency where Deactivate =0");
                var CurrencyName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramCurrency).ToList();
                ViewBag.Currency = CurrencyName;

                DynamicParameters paramcmp = new DynamicParameters();
                paramcmp.Add("@query", "select CmpID,EmployeeBranchId,EmployeeDepartmentID  from Mas_Employee where EmployeeId=" + Session["EmployeeId"] + "");
                var EmployeeDetails = DapperORM.ReturnList<EmployeeDetails>("sp_QueryExcution", paramcmp).ToList();
                ViewBag.EmployeeDetails = EmployeeDetails;


                if (CmpId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_employeeid", Session["EmployeeId"]);
                    param.Add("@p_CmpId", CmpId);
                    ViewBag.GetBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                    //DynamicParameters param = new DynamicParameters();
                    //param.Add("@query", "Select  BranchId as Id, BranchName As Name from Mas_Branch where Deactivate=0 and CmpId= '" + CmpId + "'");
                    //var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    //ViewBag.GetBranchName = BranchName;

                    DynamicParameters paramEmployeename = new DynamicParameters();
                    paramEmployeename.Add("@query", "select EmployeeId as Id ,EmployeeName As Name from Mas_Employee where Mas_Employee.Deactivate=0 and  EmployeeDepartmentID=" + DepartmentId + " and EmployeeDesignationID=" + DesignationId + " order by Name");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmployeename).ToList();
                    ViewBag.GetReplacementEmployee = data;

                    //DynamicParameters paramcmp = new DynamicParameters();
                    //paramcmp.Add("@query", "select CmpID,EmployeeBranchId,EmployeeDepartmentID  from Mas_Employee where EmployeeId="+Session["EmployeeId"]+"");
                    //var CmmpId = DapperORM.ReturnList<EmployeeDetails>("sp_QueryExcution", paramcmp).ToList();
                    //ViewBag.GetCmmpId = CmmpId;
                }
                else
                {
                    ViewBag.GetReplacementEmployee = "";
                    ViewBag.GetBranchName = "";
                }

                var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Recruitment_ResourceRequest";
                var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                ViewBag.DocNo = DocNo;

                if (ResourceId_Encrypted != null)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    paramList.Add("@P_Qry", "and Recruitment_ResourceRequest.ResourceEmployeeId='" + Session["EmployeeId"] + "'");
                    paramList.Add("@p_Origin", "Resource");
                    paramList.Add("@p_ResourceId_Encrypted", ResourceId_Encrypted);

                    ResourceRequest = DapperORM.ReturnList<Recruitment_ResourceRequest>("sp_List_Recruitment_ESS_ResourceRequest", paramList).FirstOrDefault();
                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {
                        var GetDocNos = "Select DocNo As DocNo from Recruitment_ResourceRequest where ResourceId_Encrypted='" + ResourceId_Encrypted + "'";
                        var DocNos = DapperORM.DynamicQuerySingle(GetDocNos);
                        ViewBag.DocNo = DocNos;
                    }
                    TempData["PositionTypeID"] = ResourceRequest.PositionTypeID;
                    TempData["Currency"] = ResourceRequest.Currency;
                    TempData["EmployeeifReplacement"] = ResourceRequest.EmployeeifReplacement;
                    TempData["ClosingDate"] = ResourceRequest.ClosingDate;
                    TempData["DocDate"] = ResourceRequest.DocDate;
                    TempData["AttachFile"] = ResourceRequest.AttachFile;
                    TempData["FilePath"] = ResourceRequest.FilePath;
                    Session["SelectedFile"] = ResourceRequest.AttachFile;
                    TempData["PositionBudgeted"] = ResourceRequest.PositionBudgeted;
                    TempData["LegalBond"] = ResourceRequest.LegalBond;
                    TempData["JDTemplate"] = ResourceRequest.JDTemplateId;
                    TempData["Remark"] = ResourceRequest.Remark;
                }
                TempData["Action"] = !string.IsNullOrEmpty(ResourceId_Encrypted) ? "Update" : "Save";
                return View(ResourceRequest);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsValidation
        public ActionResult IsRecruitmentExists(double Designation, string ClosingDate, string ResourceEncrypted, double BranchId, double CmpId, double? Department, double? Grade)
        {
            try
            {

                var Message = "";
                var Icon = "";
                var AttenLockCount = DapperORM.DynamicQuerySingle("Select count(*) as CheckListCount  from Recruitment_CheckList where  CmpId=" + CmpId + " and CheckListBranchId=" + BranchId + " and DepartmentID=" + Department + " and DesignationID=" + Designation + " and GradeID=" + Grade + " and Deactivate=0");
                //if (AttenLockCount.CheckListCount == 0)
                //{
                //    Message = "Check list not created";
                //    Icon = "error";
                //    return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                //}

                param.Add("@p_process", "IsValidation");
                param.Add("@p_DesignationID", Designation);
                param.Add("@p_ClosingDate", ClosingDate);
                param.Add("@p_ResourceBranchId", BranchId);
                param.Add("@p_CmpId", CmpId);
                param.Add("@p_ResourceId_Encrypted", ResourceEncrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_ResourceRequest", param);
                Message = param.Get<string>("@p_msg");
                Icon = param.Get<string>("@p_Icon");
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

        #region SaveUpdate
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveUpdate(Recruitment_ResourceRequest RecruitmentRequest, HttpPostedFileBase AttachFile, string Remark)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                string hiddenValue = HttpUtility.HtmlDecode(Request.Form["HiddenField"]);
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(RecruitmentRequest.ResourceId_Encrypted) ? "Save" : "Update");
                param.Add("@p_ResourceId_Encrypted", RecruitmentRequest.ResourceId_Encrypted);
                param.Add("@p_CmpId", RecruitmentRequest.CmpId);
                param.Add("@p_ResourceBranchId", RecruitmentRequest.ResourceBranchId);
                param.Add("@p_DocDate", RecruitmentRequest.DocDate);
                param.Add("@p_DesignationID", RecruitmentRequest.DesignationID);
                param.Add("@p_DepartmentID", RecruitmentRequest.DepartmentID);
                param.Add("@p_GradeID", RecruitmentRequest.GradeID);
                param.Add("@p_ClosingDate", RecruitmentRequest.ClosingDate);
                param.Add("@p_TotalPositions", RecruitmentRequest.TotalPositions);
                param.Add("@p_PositionTypeID", RecruitmentRequest.PositionTypeID);
                param.Add("@p_LegalBondDuration", RecruitmentRequest.LegalBondDuration);
                param.Add("@p_Confidential", RecruitmentRequest.Confidential);
                param.Add("@p_MinExperience", RecruitmentRequest.MinExperience);
                param.Add("@p_MaxExperience", RecruitmentRequest.MaxExperience);
                param.Add("@p_Qualification", RecruitmentRequest.Qualification);
                param.Add("@p_PreferredLanguage", RecruitmentRequest.PreferredLanguage);
                param.Add("@p_PreferredTechnicalSkills", RecruitmentRequest.PreferredTechnicalSkills);
                param.Add("@p_MusthaveTechnicalSkills", RecruitmentRequest.MusthaveTechnicalSkills);
                param.Add("@p_Competencies", RecruitmentRequest.Competencies);
                param.Add("@p_PositionBudgeted", RecruitmentRequest.PositionBudgeted);
                param.Add("@p_MinBudget", RecruitmentRequest.MinBudget);
                param.Add("@p_MaxBudget", RecruitmentRequest.MaxBudget);
                param.Add("@p_MinAge", RecruitmentRequest.MinAge);
                param.Add("@p_MaxAge", RecruitmentRequest.MaxAge);
                param.Add("@p_LegalBond", RecruitmentRequest.LegalBond);
                param.Add("@p_PreferredMaritalStatus", RecruitmentRequest.PreferredMaritalStatus);
                param.Add("@p_PreferredGender", RecruitmentRequest.PreferredGender);
                param.Add("@p_Residence", RecruitmentRequest.Residence);
                param.Add("@p_Remark", Remark);
                param.Add("@p_WorkTypeID", RecruitmentRequest.WorkTypeID);
                param.Add("@p_PreferredShiftTiming", RecruitmentRequest.PreferredShiftTiming);
                param.Add("@p_Priority", RecruitmentRequest.Priority);
                param.Add("@p_JDTemplateId", RecruitmentRequest.JDTemplateId);
                param.Add("@p_CandidateType", RecruitmentRequest.CandidateType);
                param.Add("@p_EmployeeifReplacement", RecruitmentRequest.EmployeeifReplacement);
                param.Add("@p_Currency", RecruitmentRequest.Currency);
                //if (RecruitmentRequest.ResourceId_Encrypted != null && AttachFile == null)
                //{
                //    param.Add("@p_AttachFile", Session["SelectedFile"]);
                //}
                //else
                //{
                //    param.Add("@p_AttachFile", AttachFile == null ? "" : AttachFile.FileName);
                //}

                param.Add("@p_AttachFile", AttachFile == null ? "" : AttachFile.FileName);
                param.Add("@p_ResourceEmployeeId", EmployeeId);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_ResourceRequest", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                if (TempData["P_Id"] != null)
                {

                    if (AttachFile != null)
                    {
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Recruitment'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        var FirstPath = GetFirstPath + TempData["P_Id"] + "\\"; // First path plus concat folder 
                        string directoryPath = Path.GetDirectoryName(FirstPath);
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(FirstPath);
                        }
                        string AttachFilePath = "";
                        AttachFilePath = FirstPath + AttachFile.FileName; //Concat Full Path and create New full Path
                        AttachFile.SaveAs(AttachFilePath); // This is use for Save image in folder full path
                    }

                    #region Send Mail

                    DynamicParameters paramApproverEmployeeId = new DynamicParameters();
                    paramApproverEmployeeId.Add("@query", "Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName, EmployeeId,MAs_companyProfile.CompanyName from MAs_employee,Mas_Designation,MAs_companyProfile,Tra_Approval,Recruitment_ResourceRequest where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID and MAs_companyProfile.CompanyId=MAs_employee.CmpId and Tra_Approval.TraApproval_DocId=Recruitment_ResourceRequest.ResourceId and MAs_employee.EmployeeId=Tra_Approval.TraApproval_ApproverEmployeeId and Tra_Approval.Origin='ResourceRequest' and ApproverLevel=1 and Recruitment_ResourceRequest.Deactivate=0 and Tra_Approval.Deactivate=0 and Recruitment_ResourceRequest.ResourceId =" + TempData["P_Id"]);
                    var GetUrl1 = "select Recruitment_URL  from Tool_CommonTable";
                    var GetUrl = DapperORM.DynamicQuerySingle(GetUrl1);
                    var ApproverEmployeeIdEmail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApproverEmployeeId).FirstOrDefault();

                    if (ApproverEmployeeIdEmail != null && !string.IsNullOrEmpty(Convert.ToString(ApproverEmployeeIdEmail.CompanyMailID)))
                    {
                        DynamicParameters paramApprover = new DynamicParameters();
                        paramApprover.Add("@query", "Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName, EmployeeId,MAs_companyProfile.CompanyName from MAs_employee,Mas_Designation,MAs_companyProfile where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID and MAs_companyProfile.CompanyId=MAs_employee.CmpId and EmployeeId =" + Session["EmployeeId"]);

                        var ResourceRequest = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApprover).FirstOrDefault();

                        DynamicParameters paramPosition = new DynamicParameters();
                        paramPosition.Add("@query", "Select Mas_Designation.DesignationName, Mas_Department.DepartmentName from Mas_Department,Mas_Designation,Recruitment_ResourceRequest where Mas_Designation.DesignationId=Recruitment_ResourceRequest.DesignationID and Mas_Department.DepartmentId=Recruitment_ResourceRequest.DepartmentID and Mas_Department.Deactivate=0 and Mas_Designation.Deactivate=0 and Recruitment_ResourceRequest.Deactivate=0 and Recruitment_ResourceRequest.ResourceId =" + TempData["P_Id"]);

                        var PositionDetail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramPosition).FirstOrDefault();

                        DynamicParameters paramToolEmail = new DynamicParameters();
                        paramToolEmail.Add("@query", "Select CmpId, Origin, SMTPServerName, PortNo, SSL, FromEmailId, Password from Tool_EmailSetting where Deactivate=0 and CmpId=" + Session["CompanyId"] + " and Origin='4'");

                        var GetToolEmail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramToolEmail).FirstOrDefault();

                        if (GetToolEmail == null || string.IsNullOrEmpty(Convert.ToString(GetToolEmail.FromEmailId)))
                        {
                            return Json(new { success = false, error = "SMTP settings not found." });
                        }

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
                            mail.To.Add(ApproverEmployeeIdEmail.CompanyMailID);

                            mail.Subject = "Pending Recruitment Resource Request – Approval Require";

                            mail.Body = "Dear " + (Convert.ToString(ApproverEmployeeIdEmail?.EmployeeName) ?? "") + ",<br><br>" +
                                        "I would like to bring to your attention the recruitment resource request submitted on " + DateTime.Now.ToString("dd/MMM/yyyy") +
                                        " for the " + (PositionDetail?.DesignationName ?? "") +
                                        " within the " + (PositionDetail?.DepartmentName ?? "") + ".<br><br>" +
                                        "Kindly log in to " + GetUrl.Recruitment_URL + " and approve the request.<br><br>" +
                                        "Best regards,<br>" +
                                        (ResourceRequest?.EmployeeName ?? "") + "<br>" +
                                        (ResourceRequest?.DesignationName ?? "") + "<br>" +
                                        (ResourceRequest?.CompanyName ?? "");

                            mail.IsBodyHtml = true;

                            smtp.Send(mail);
                        }
                    }

                    #endregion
                }
                else
                {

                    if (AttachFile != null)
                    {
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Recruitment'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        var FirstPath = GetFirstPath + RecruitmentRequest.ResourceId + "\\"; // First path plus concat folder 
                        string directoryPath = Path.GetDirectoryName(FirstPath);
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(FirstPath);
                        }
                        string AttachFilePath = "";
                        AttachFilePath = FirstPath + AttachFile?.FileName; //Concat Full Path and create New full Path
                        AttachFile.SaveAs(AttachFilePath); // This is use for Save image in folder full path
                    }
                }

                return RedirectToAction("GetList", "ESS_Recruitment_ResourcesRequisition");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region GetList
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 127;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_Qry", " and Recruitment_ResourceRequest.ResourceEmployeeId='" + Session["EmployeeId"] + "'");
                //param.Add("@P_Qry", " and Recruitment_ResourceRequest.ResourceEmployeeId='" + Session["EmployeeId"] + "'and Recruitment_ResourceRequest.ResourceRequestStatus<>'Closed'");
                param.Add("@p_ResourceId_Encrypted", "List");
                param.Add("@p_Origin", "Resource");
                var ResourceRecruitment = DapperORM.DynamicList("sp_List_Recruitment_ESS_ResourceRequest", param);
                ViewBag.ResourceRecruitment = ResourceRecruitment;
                return View();

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

        //#region Bu
        //[HttpGet]
        //public ActionResult GetBU(int EmpID)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { area = "" });
        //        }
        //        DynamicParameters param = new DynamicParameters();
        //        param.Add("@query", "SELECT mas_branch.BranchId AS Id, mas_branch.BranchName AS Name FROM Mas_Employee LEFT JOIN mas_branch ON mas_employee.EmployeeBranchId = mas_branch.BranchId WHERE mas_employee.Deactivate = 0 AND mas_employee.EmployeeId=" + EmpID + "");
        //        var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
        //        return Json(data, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }

        //}
        //#endregion

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region Delete
        public ActionResult Delete(string ResourceId_Encrypted, string Remark)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@p_process", "Closed");
                param.Add("@p_ResourceRequestStatus", "Closed");
                param.Add("@p_ResourceRequestRemark", Remark);
                param.Add("@p_ResourceId_Encrypted", ResourceId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_ResourceRequest", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_Recruitment_ResourcesRequisition");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region DeleteRecruitment
        public ActionResult DeleteRecruitment(string ResourceId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_ResourceId_Encrypted", ResourceId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_ResourceRequest", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_Recruitment_ResourcesRequisition");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region PositionType
        public ActionResult PositionType(string Replecment)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var EmployeeName = "select EmployeeId,EmployeeName from Mas_Employee where Deactivate=0";
                return Json(EmployeeName, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Approval List
        public ActionResult ApprovalList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 128;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_Origin", "Approval");
                var ResourceApproval = DapperORM.DynamicList("sp_List_Recruitment_ESS_Approval_Assign_Pool", param);
                ViewBag.ResourceApprovalList = ResourceApproval;
                return View();

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Approval ViewForRequestApprover
        public ActionResult ViewForRequestApprover(string ResourceId_Encrypted, int? DocNo)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@p_EncryptedId", ResourceId_Encrypted);
                param.Add("@p_Origin", "ApprovalRequest");
                var ResourceApproval = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_Profiles", param).FirstOrDefault();
                if (ResourceApproval != null)
                {
                    ViewBag.ResourceApprovalList = ResourceApproval;
                }
                else
                {
                    ViewBag.ResourceApprovalList = "";
                }

                DynamicParameters paramApprovalRemark = new DynamicParameters();
                paramApprovalRemark.Add("@query", "Select ApproveRejectRemark,Mas_Employee.EmployeeName from Tra_Approval ,Mas_Employee where Tra_Approval.Deactivate=0 and Mas_Employee.Deactivate=0 and Tra_Approval.TraApproval_ApproverEmployeeId=Mas_Employee.EmployeeId and TraApproval_DocId=" + DocNo + " and  Tra_Approval.origin='ResourceRequest' and Tra_Approval.Status='Approved'");
                var ApprovalRemark = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApprovalRemark).ToList();
                ViewBag.ApprovalRemarkList = ApprovalRemark;

                string updateEmiQuery = "update Recruitment_ResourceRequest set ManagerStatus='Seen' ,ManagerStatusDate=GETDATE() where ResourceId_Encrypted='" + ResourceId_Encrypted + "'";
                DapperORM.ExecuteQuery(updateEmiQuery);

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
        #region ForApprovalCandidate
        [HttpGet]
        public ActionResult ForApprovalCandidate(int? DocId, string Encrypted, string Status, string Remark)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramApprove = new DynamicParameters();
                paramApprove.Add("@p_Origin", "Recruitment");
                paramApprove.Add("@p_DocId_Encrypted", Encrypted);
                paramApprove.Add("@p_DocId", DocId);
                paramApprove.Add("@p_Managerid", Session["EmployeeId"]);
                paramApprove.Add("@p_Status", Status);
                paramApprove.Add("@p_ApproveRejectRemark", Remark);
                paramApprove.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                paramApprove.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var GetApprovedResult = DapperORM.ExecuteSP<dynamic>("sp_Approved_Rejected", paramApprove);
                var Message = paramApprove.Get<string>("@p_msg");
                var Icon = paramApprove.Get<string>("@p_Icon");
                if (Message != "")
                {
                    TempData["Message"] = Message;
                    TempData["Icon"] = Icon.ToString();

                    #region Send Mail
                    DynamicParameters paramApproverEmployeeId = new DynamicParameters();
                    paramApproverEmployeeId.Add("@query", "Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName, EmployeeId,MAs_companyProfile.CompanyName from MAs_employee,Mas_Designation,MAs_companyProfile,Tra_Approval,Recruitment_ResourceRequest where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID   and MAs_companyProfile.CompanyId=MAs_employee.CmpId and   Tra_Approval.TraApproval_DocId=Recruitment_ResourceRequest.ResourceId and MAs_employee.EmployeeId=Tra_Approval.TraApproval_ApproverEmployeeId and Tra_Approval.Origin='ResourceRequest' and ApproverLevel=2 and Recruitment_ResourceRequest.Deactivate=0 and  Tra_Approval.Deactivate=0 and Recruitment_ResourceRequest.ResourceId =" + DocId + "");
                    var ApproverEmployeeIdEmail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApproverEmployeeId).FirstOrDefault();
                    var GetUrl1 = "select Recruitment_URL  from Tool_CommonTable";
                    var GetUrl = DapperORM.DynamicQuerySingle(GetUrl1);
                    if (ApproverEmployeeIdEmail != null &&
                        !string.IsNullOrWhiteSpace(Convert.ToString(ApproverEmployeeIdEmail.CompanyMailID)))
                    {
                        DynamicParameters paramApprover = new DynamicParameters();
                        paramApprover.Add("@query", "Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName, EmployeeId,MAs_companyProfile.CompanyName  from MAs_employee,Mas_Designation,MAs_companyProfile where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID and MAs_companyProfile.CompanyId=MAs_employee.CmpId and  EmployeeId =" + Session["EmployeeId"] + "");
                        var ResourceRequestApproval1 = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApprover).FirstOrDefault();

                        DynamicParameters paramPosition = new DynamicParameters();
                        paramPosition.Add("@query", "Select Mas_Designation.DesignationName, Mas_Department.DepartmentName,replace(convert(nvarchar(12),Recruitment_ResourceRequest.DocDate,106),' ','/') as  DocDate  from Mas_Department,Mas_Designation,Recruitment_ResourceRequest  where Mas_Designation.DesignationId=Recruitment_ResourceRequest.DesignationID and  Mas_Department.DepartmentId=Recruitment_ResourceRequest.DepartmentID  and Mas_Department.Deactivate=0 and Mas_Designation.Deactivate=0 and Recruitment_ResourceRequest.Deactivate=0  and Recruitment_ResourceRequest.ResourceId =" + DocId + "");
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
                            mail.To.Add(Convert.ToString(ApproverEmployeeIdEmail.CompanyMailID));
                            mail.Subject = "Pending Recruitment Resource Request – Approval Require";
                            mail.Body = "Dear " + ApproverEmployeeIdEmail.EmployeeName + ",<br><br>I would like to bring to your attention the recruitment resource request submitted on <b>" + PositionDetail.DocDate + "</b> for the <b>" + PositionDetail.DesignationName + "</b> within the <b>" + PositionDetail.DepartmentName + "</b> . As of today, the request remains pending approval.<br><br>Timely approval is essential to initiate the recruitment process and avoid potential delays in achieving our departmental objectives.<br><br>" +
                                        "Kindly log in to "+ GetUrl.Recruitment_URL + " and approved recruitment resource request from the pending requests section.<br><br>" +
                                        "Your prompt attention to this matter would be greatly appreciated. Please let me know if any additional information or documentation is required to facilitate the approval process.<br><br>Thank you for your consideration.<br><br>" +
                                        "Best regards,<br>" + ResourceRequestApproval1.EmployeeName + "<br>" + ResourceRequestApproval1.DesignationName + "<br>" + ResourceRequestApproval1.CompanyName + "<br>" + "";


                            mail.IsBodyHtml = true;
                            smtp.Send(mail);
                        }

                        string Employee_Admin = $@"INSERT INTO Recruitment_EmailLog (Recruitment_EmailSendTo, Recruitment_EmailSendBy, Recruitment_ResourceId, Recruitment_EmailLogDate, EmailId,Origin) 
                                       VALUES ({ApproverEmployeeIdEmail.EmployeeId}, {Session["EmployeeId"]}, {DocId }, GETDATE(), '{ApproverEmployeeIdEmail.CompanyMailID}','Employee');";

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
                    else
                    {
                        #region Send MAil
                        DynamicParameters paramAssign= new DynamicParameters();
                        paramAssign.Add("@query", "Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName,  EmployeeId,MAs_companyProfile.CompanyName from MAs_employee,Mas_Designation, MAs_companyProfile,Recruitment_ResourceRequest where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID and MAs_companyProfile.CompanyId=MAs_employee.CmpId and   MAs_employee.EmployeeId=Recruitment_ResourceRequest.RequestAssignId and   Recruitment_ResourceRequest.Deactivate=0 and Recruitment_ResourceRequest.ResourceId =" + DocId + "");
                        var paramAssignEmail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramAssign).FirstOrDefault();

                        if (paramAssignEmail != null &&
                            !string.IsNullOrWhiteSpace(Convert.ToString(paramAssignEmail.CompanyMailID)))
                        {
                            DynamicParameters paramApprover = new DynamicParameters();
                            paramApprover.Add("@query", "Select CompanyMailID,CompanyMobileNo,Mas_Designation.DesignationName,EmployeeName, EmployeeId,MAs_companyProfile.CompanyName  from MAs_employee,Mas_Designation,MAs_companyProfile where Mas_Designation.DesignationId=MAs_employee.EmployeeDesignationID and MAs_companyProfile.CompanyId=MAs_employee.CmpId and  EmployeeId =" + Session["EmployeeId"] + "");
                            var ResourceRequest = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramApprover).FirstOrDefault();

                            DynamicParameters paramPosition = new DynamicParameters();
                            paramPosition.Add("@query", "Select Mas_Designation.DesignationName, Mas_Department.DepartmentName,replace(convert(nvarchar(12),Recruitment_ResourceRequest.DocDate,106),' ','/') as  DocDate  from Mas_Department,Mas_Designation,Recruitment_ResourceRequest  where Mas_Designation.DesignationId=Recruitment_ResourceRequest.DesignationID and  Mas_Department.DepartmentId=Recruitment_ResourceRequest.DepartmentID  and Mas_Department.Deactivate=0 and Mas_Designation.Deactivate=0 and Recruitment_ResourceRequest.Deactivate=0  and Recruitment_ResourceRequest.ResourceId =" + DocId + "");
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
                                mail.To.Add(Convert.ToString(paramAssignEmail.CompanyMailID));
                                mail.Subject = "Pending Request for Recruitment Resource Allocation";
                                mail.Body = "Dear " + paramAssignEmail.EmployeeName + ",<br><br>I would like to bring to your attention the recruitment resource request submitted on <b>" + PositionDetail.DocDate + "</b> for the <b>" + PositionDetail.DesignationName + "</b> within the <b>" + PositionDetail.DepartmentName + "</b> .As of today, it is essential to assign an employee with hiring privileges to initiate the recruitment process promptly and prevent potential delays in meeting our departmental objectives.<br><br>" +
                                            "Kindly log in to " + GetUrl.Recruitment_URL + " andallocate an employee to handle a recruitment resource request from the pending requests section.<br><br>" +
                                            "Your prompt attention to this matter would be greatly appreciated. Please let me know if any additional information or documentation is required to facilitate the approval process.<br><br>Thank you for your consideration.<br><br>" +
                                            "Best regards,<br>" + ResourceRequest.EmployeeName + "<br>" + ResourceRequest.DesignationName + "<br>" + ResourceRequest.CompanyName + "<br>" + "";


                                mail.IsBodyHtml = true;
                                smtp.Send(mail);
                            }

                            string Employee_Admin = $@"INSERT INTO Recruitment_EmailLog (Recruitment_EmailSendTo, Recruitment_EmailSendBy, Recruitment_ResourceId, Recruitment_EmailLogDate, EmailId,Origin) 
                                       VALUES ({paramAssignEmail.EmployeeId}, {Session["EmployeeId"]}, {DocId }, GETDATE(), '{paramAssignEmail.CompanyMailID}','Employee');";

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
                    }
                    #endregion


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

        #region ForApprovalCandidate
        [HttpGet]
        public ActionResult ClosingResourceRequest(string ResourceId_Encrypted, string Remark)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramApprove = new DynamicParameters();
                paramApprove.Add("@p_process", "Closed");
                paramApprove.Add("@p_ResourceId_Encrypted", ResourceId_Encrypted);
                paramApprove.Add("@p_ResourceRequestStatus", "Closed");
                paramApprove.Add("@p_ResourceRequestRemark", Remark);
                paramApprove.Add("@p_MachineName", Dns.GetHostName().ToString());
                paramApprove.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                paramApprove.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var GetApprovedResult = DapperORM.ExecuteSP<dynamic>("sp_SUD_Recruitment_ResourceRequest", paramApprove);
                var Message = paramApprove.Get<string>("@p_msg");
                var Icon = paramApprove.Get<string>("@p_Icon");
                if (Message != "")
                {
                    TempData["Message"] = Message;
                    TempData["Icon"] = Icon.ToString();
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

        #region ApprovedRejectedList
        public ActionResult ApprovedRejectedList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 128;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_Origin", "ApprovedRequest");
                var ResourceApprovalRejected = DapperORM.DynamicList("sp_List_Recruitment_ESS_Approval_Assign_Pool", param);
                ViewBag.ApprovalRejectedList = ResourceApprovalRejected;
                return View();

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region ResourceRequisitionList
        public ActionResult ResourceRequisitionList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 242;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_Qry", " and Recruitment_ResourceRequest.ResourceRequestStatus<>'Closed'");
                param.Add("@p_ResourceId_Encrypted", "List");
                param.Add("@p_Origin", "ResourceRequisitionList");
                var ResourceRecruitment = DapperORM.DynamicList("sp_List_Recruitment_ESS_ResourceRequest", param);
                ViewBag.ResourceRecruitment = ResourceRecruitment;
                return View();

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region GetRepalcementEmployee
        [HttpGet]
        public ActionResult GetRepalcementEmployee(int? DepartmentIds, int? DesignationIds)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "SELECT  e.EmployeeId AS Id,e.EmployeeName + '| CTC: ' + CAST(ISNULL(c.RateTotalPartAB,0) * 12 AS VARCHAR(20))  AS Name FROM Mas_Employee e INNER JOIN Mas_Department d ON d.DepartmentId = e.EmployeeDepartmentID  LEFT JOIN Mas_Employee_CTC c  ON c.CTCEmployeeId = e.EmployeeId where E.Deactivate=0  AND YEAR(C.TODATE)='2999' and  EmployeeDepartmentID=" + DepartmentIds + " and EmployeeDesignationID=" + DesignationIds + " order by Name");
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

        #region GetDesignationWiseDetails
        [HttpGet]
        public ActionResult GetDesignationWiseDetails(int? DesignationID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramDesig = new DynamicParameters();
                paramDesig.Add("@query", "Select MinimumAge,MaximumAge,MinimumExperience,MaximumExperience,MinimumBudget,MaximumBudget from Mas_Designation where Mas_Designation.RecruitmentApplicable=1  and Mas_Designation.Deactivate=0 and Mas_Designation.DesignationId=" + DesignationID + "");
                var DesignationList = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramDesig).FirstOrDefault();
                return Json(DesignationList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetRequestRevertView
        [HttpGet]
        public ActionResult GetRequestRevertView(string ResourceId_Encrypted, int? DocNo)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@p_EncryptedId", ResourceId_Encrypted);
                param.Add("@p_Origin", "ApprovalRequest");
                var ResourceApproval = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_Profiles", param).FirstOrDefault();
                if (ResourceApproval != null)
                {
                    ViewBag.ResourceApprovalList = ResourceApproval;
                }
                else
                {
                    ViewBag.ResourceApprovalList = "";
                }

                DynamicParameters paramApprovalRemark = new DynamicParameters();
                paramApprovalRemark.Add("@query", "Select ApproveRejectRemark,Mas_Employee.EmployeeName from Tra_Approval ,Mas_Employee where Tra_Approval.Deactivate=0 and Mas_Employee.Deactivate=0 and Tra_Approval.TraApproval_ApproverEmployeeId=Mas_Employee.EmployeeId and TraApproval_DocId=" + DocNo + " and  Tra_Approval.origin='ResourceRequest' and Tra_Approval.Status='Approved'");
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

        #region SaveRequestRevert
        [HttpGet]
        public ActionResult SaveRequestRevert(string RequestRevertRemark, int? DocId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramRevert = new DynamicParameters();
                paramRevert.Add("@p_RequestEmployeeID", Session["EmployeeId"]);
                paramRevert.Add("@p_Origin", "ResourceRequest");
                paramRevert.Add("@p_ModuleID", 3);
                paramRevert.Add("@p_DocId", DocId);
                paramRevert.Add("@p_RequestRevertRemark", RequestRevertRemark);
                paramRevert.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                paramRevert.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_Request_Revert", paramRevert);
                var Message = paramRevert.Get<string>("@p_msg");
                var Icon = paramRevert.Get<string>("@p_Icon");
                if (Message != "")
                {
                    //TempData["Message"] = Message;
                    //TempData["Icon"] = Icon.ToString();
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

        #region JDDescription
        [HttpGet]
        public ActionResult JDDescription(int? JDTemplate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "Select Description from Recruitment_JobDescriptionTemplate where Deactivate=0 and RecruitmentJDTemplateID=" + JDTemplate + "");
                var data = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region Get JdDropdown
        [HttpGet]
        public ActionResult JdDropdown(int DeptId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramcmp = new DynamicParameters();
                paramcmp.Add("@query", "select RecruitmentJDTemplateID as Id, RecruitmentJDTemplate as Name from Recruitment_JobDescriptionTemplate where JDTemplateDepartmentId=" + DeptId + "");
                var JobTemplate = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramcmp).ToList();
                ViewBag.JD = JobTemplate;

                return Json(JobTemplate, JsonRequestBehavior.AllowGet);
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