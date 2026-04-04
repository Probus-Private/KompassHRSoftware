using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Prime;
using KompassHR.Areas.Module.Models.Module_Preboarding;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Module.Models.Module_Employee;


namespace KompassHR.Areas.Module.Controllers.Module_Preboarding
{
    public class Module_Preboarding_CreateLinkController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Module/Module_Preboarding_CreateLink

        #region CreateLink Main View
        [HttpGet]
        public ActionResult Module_Preboarding_CreateLink(string FID_Encrypted, int? CmpId, int? BranchID, string OfferLetterGenId_Encrypted, int? OfferLetterGenResumeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    TempData["Message"] = "Your session has expired. Please log in again.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                int EmpId = Convert.ToInt32(Session["EmployeeId"]);
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 138;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Preboarding_Mas_Employee Module_Preboarding_CreateLink = new Preboarding_Mas_Employee();
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var GetDocNo = "Select isnull(Max(DocNo),0)+1 As DocNo from Preboarding_Mas_Employee";
                    var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    ViewBag.DocNo = DocNo;
                }

                var ResultList = DapperORM.DynamicQueryMultiple(@"
                                                       Select DepartmentId as Id,DepartmentName as Name from Mas_Department Where Deactivate=0 order by DepartmentName;
                                                       Select DesignationId As Id,DesignationName As Name from Mas_Designation Where Deactivate=0 order by DesignationName;                                                       
                                                       Select GradeId As Id,GradeName As Name from Mas_Grade Where Deactivate=0 order by GradeName
                                                       select ModuleSettingID As Id,CompulsoryModule As Name from Preboarding_ModuleSetting where Deactivate=0");
                ViewBag.GetDepartmentName = ResultList[0].Select(x => new AllDropDownClass { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetDesignationName = ResultList[1].Select(x => new AllDropDownClass { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetGradeName = ResultList[2].Select(x => new AllDropDownClass { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetCompulsoryModule = ResultList[3].Select(x => new AllDropDownClass { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                //ViewBag.GetDepartmentName = ResultList.Read<AllDropDownClass>().ToList();
                //ViewBag.GetDesignationName = ResultList.Read<AllDropDownClass>().ToList();               
                //ViewBag.GetGradeName = ResultList.Read<AllDropDownClass>().ToList();

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.ComapnyProfile = GetComapnyName;

                DynamicParameters paramHR = new DynamicParameters();
                paramHR.Add("@query", "Select EmployeeId As Id,EmployeeName As Name from Mas_Employee Where Deactivate=0 and EmployeeId=" + Session["EmployeeId"] + " order by EmployeeName");
                var ReportingHRList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramHR).ToList();
                ViewBag.ReportingHR = ReportingHRList;

                if (CmpId != null)
                {

                    DynamicParameters paramBR = new DynamicParameters();
                    paramBR.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBR.Add("@p_CmpId", CmpId);
                    ViewBag.Location = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBR).ToList();

                    //param.Add("@query", "Select  BranchId As Id, BranchName As Name from Mas_Branch where Deactivate=0 and CmpId=" + CmpId + "");
                    //var GetLocation = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    //ViewBag.Location = GetLocation;
                }
                else
                {
                    ViewBag.Location = "";
                }

                //if (CmpId != null && BranchID != null)
                //{
                //    DynamicParameters paramHR = new DynamicParameters();
                //    paramHR.Add("@query", "select Mas_Employee_Reporting.ReportingHR As Id,mas_employee.EmployeeName As Name from Mas_Employee_Reporting,mas_employee,Mas_Branch where Mas_Employee_Reporting.ReportingHR=mas_employee.EmployeeId and Mas_Employee_Reporting.Deactivate=0 and mas_employee.Deactivate=0 and EmployeeLeft=0 and  mas_employee.EmployeeBranchId=Mas_Branch.branchid and Mas_Branch.CmpID= " + CmpId + " and Mas_Branch.branchid=" + BranchID + " and ReportingModuleID=6 order by mas_employee.EmployeeName ");
                //    var ReportingHRList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramHR).ToList();
                //    ViewBag.ReportingHR = ReportingHRList;
                //}
                //else
                //{
                //    ViewBag.ReportingHR = "";
                //}

                var ValidDays = DapperORM.DynamicQueryList("Select PreboardingLinkExpiryDays from Preboarding_GeneralSetting where Deactivate = 0").FirstOrDefault();

                if (ValidDays != null)
                {
                    TempData["ValidDays"] = ValidDays.PreboardingLinkExpiryDays;

                }
                else
                {
                    TempData["ValidDays"] = "";
                }

                TempData["LinkExpiryDate"] = "";
                TempData["JoiningDate"] = "";
                TempData["DocDate"] = "";
                TempData["EM_PF_Applicable"] = "";
                TempData["ESIC_Insurance"] = "";
                if (FID_Encrypted != null)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    paramList.Add("@p_FID_Encrypted", FID_Encrypted);
                    Module_Preboarding_CreateLink = DapperORM.ReturnList<Preboarding_Mas_Employee>("sp_List_CreatePreboardingLink", paramList).FirstOrDefault();

                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {
                        var GetDocNo = "Select DocNo As DocNo from Preboarding_Mas_Employee where FID_Encrypted='" + FID_Encrypted + "'";
                        var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                        ViewBag.DocNo = DocNo;
                    }
                    TempData["LinkExpiryDate"] = Module_Preboarding_CreateLink.LinkExpiryDate;
                    TempData["JoiningDate"] = Module_Preboarding_CreateLink.JoiningDate;
                    TempData["DocDate"] = Module_Preboarding_CreateLink.DocDate;
                    TempData["EM_PF_Applicable"] = Module_Preboarding_CreateLink.EM_PF_Applicable;
                    TempData["ESIC_Insurance"] = Module_Preboarding_CreateLink.ESIC_Insurance;
                }


                if (OfferLetterGenId_Encrypted != null)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_OfferLetterGenId_Encrypted", OfferLetterGenId_Encrypted);
                    Module_Preboarding_CreateLink = DapperORM.ReturnList<Preboarding_Mas_Employee>("sp_Get_Recruitment_OfferToLink", paramList).FirstOrDefault();
                    TempData["OfferLetterGenId_Encrypted"] = OfferLetterGenId_Encrypted;
                    Session["OfferLetterGenResumeId"] = OfferLetterGenResumeId;
                }
                else
                {
                    TempData["OfferLetterGenId_Encrypted"] = "";
                }
                return View(Module_Preboarding_CreateLink);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Isvalidation
        [HttpGet]
        public ActionResult IsPreboardingCreateLinkExists(string PersonalEmailID, string FID_Encrypted)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_PersonalEmailID", PersonalEmailID);
                    param.Add("@p_FID_Encrypted", FID_Encrypted);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Preboarding_CreateLink", param);
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

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Preboarding_Mas_Employee ObjPreboarding_Mas_Employee)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(ObjPreboarding_Mas_Employee.FID_Encrypted) ? "Save" : "Update");
                param.Add("@p_FID", ObjPreboarding_Mas_Employee.FID);
                param.Add("@p_FID_Encrypted", ObjPreboarding_Mas_Employee.FID_Encrypted);
                param.Add("@p_DocNo", ObjPreboarding_Mas_Employee.DocNo);
                param.Add("@p_DocDate", ObjPreboarding_Mas_Employee.DocDate);
                param.Add("@p_SalutationName", ObjPreboarding_Mas_Employee.SalutationName);
                param.Add("@p_EmployeeName ", ObjPreboarding_Mas_Employee.EmployeeName);
                param.Add("@p_ReportingHR", ObjPreboarding_Mas_Employee.ReportingHR);
                param.Add("@p_EssloginID", ObjPreboarding_Mas_Employee.PersonalEmailID);
                param.Add("@p_Esspassword", ObjPreboarding_Mas_Employee.Esspassword);
                param.Add("@p_validDays", ObjPreboarding_Mas_Employee.ValidDays);
                param.Add("@p_LinkExpiryDate", ObjPreboarding_Mas_Employee.LinkExpiryDate);
                param.Add("@p_PersonalEmailID ", ObjPreboarding_Mas_Employee.PersonalEmailID);
                param.Add("@p_CmpId", ObjPreboarding_Mas_Employee.CmpId);
                param.Add("@p_BranchID", ObjPreboarding_Mas_Employee.BranchID);
                param.Add("@p_EmployeeDepartmentId", ObjPreboarding_Mas_Employee.EmployeeDepartmentId);
                param.Add("@p_EmployeeDesignationId", ObjPreboarding_Mas_Employee.EmployeeDesignationId);
                param.Add("@p_EmployeeGradeID", ObjPreboarding_Mas_Employee.EmployeeGradeID);
                param.Add("@p_JoiningDate", ObjPreboarding_Mas_Employee.JoiningDate);
                param.Add("@p_ESIC_Insurance", ObjPreboarding_Mas_Employee.ESIC_Insurance);
                param.Add("@p_EM_PF_Applicable  ", ObjPreboarding_Mas_Employee.EM_PF_Applicable);
                param.Add("@p_ReferenceNo", ObjPreboarding_Mas_Employee.DocNo);
                param.Add("@p_CompulsoryModuleId", ObjPreboarding_Mas_Employee.CompulsoryModuleId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_LinkCreatedEmployeeId", Session["EmployeeId"]);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Preboarding_CreateLink", param);
                var pid = param.Get<string>("@p_Id");
                if (Session["OfferLetterGenResumeId"] != null || pid != null)
                {
                    Session["pid"] = pid;
                    if (Session["OfferLetterGenResumeId"] != null)
                    {
                        var Update = DapperORM.DynamicQuerySingle("Update Recruitment_Resume set PreboardingId='" + pid + "' where Recruitment_Resume.Deactivate=0 and  ResumeId='" + Session["OfferLetterGenResumeId"] + "'");

                    }
                }
                else
                {
                    Session["pid"] = ObjPreboarding_Mas_Employee.FID;
                }

                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Module_Preboarding_CreateLink", "Module_Preboarding_CreateLink");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region List View
        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    TempData["Message"] = "Your session has expired. Please log in again.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                int EmpId = Convert.ToInt32(Session["EmployeeId"]);
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 138;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_FId_Encrypted", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_CreatePreboardingLink", param).ToList();
                ViewBag.GetPreboardingCreateLinkList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region MyRegion
        public ActionResult Delete(string FID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_FID_Encrypted", FID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Preboarding_CreateLink", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Module_Preboarding_CreateLink");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

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

        //#region GetReportingHR
        //[HttpGet]
        //public ActionResult GetReportingHR(int CmpId, int EmployeeBranchId)
        //{
        //    try
        //    {
        //        DynamicParameters paramHR = new DynamicParameters();
        //        paramHR.Add("@query", "select Mas_Employee_Reporting.ReportingHR As Id,mas_employee.EmployeeName As Name from Mas_Employee_Reporting,mas_employee,Mas_Branch where Mas_Employee_Reporting.ReportingHR=mas_employee.EmployeeId and Mas_Employee_Reporting.Deactivate=0 and mas_employee.Deactivate=0 and EmployeeLeft=0 and  mas_employee.EmployeeBranchId=Mas_Branch.branchid and Mas_Branch.CmpID= " + CmpId + " and Mas_Branch.branchid=" + EmployeeBranchId + "  and ReportingModuleID=6 order by mas_employee.EmployeeName ");
        //        var ReportingHRList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramHR).ToList();
        //        return Json(ReportingHRList, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }

        //}
        //#endregion

        #region SendMail
        public ActionResult SendMail(int? EmailFID)
        {
            try
            {
                //    1 Preboarding Link
                //    2 Candidate Login Alert
                //    3 Candidate Final Submit
                //    4 Candidate Partial Rejection
                //    5 Candidate Fully Rejected
                //    6 Candidate Approved
                //    7 Candidate Joining Notification
                //    8 Preboarding Link Reopen
                //    9 Preboarding Link Expiry
                //    10 Candidate Cancel

                if (EmailFID != null)
                {
                    Session["PID"] = EmailFID;
                    var GetcmpId = DapperORM.DynamicQuerySingle("Select CmpID From Preboarding_Mas_employee where Deactivate = 0 and Fid='" + Session["PID"] + "'");
                    decimal CmpId = GetcmpId.CmpID;
                    var SMTPGET = DapperORM.DynamicQuerySingle("Select CmpId, Origin, SMTPServerName, PortNo, SSL, FromEmailId, Password from Tool_EmailSetting where Deactivate = 0 and CmpId = '" + CmpId + "'  and  Origin = '" + 1 + "'");
                    if (SMTPGET.Count != 0)
                    {
                        clsEmail emailListProcess = new clsEmail();
                        emailListProcess.SendMail(int.Parse(Session["PID"].ToString()), Convert.ToInt16(CmpId), "1", "1");
                        TempData["Message"] = "Mail has been Send";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        TempData["Message"] = "SMTP Server Name Not Found";
                        TempData["Icon"] = "error";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    var GetcmpId = DapperORM.DynamicQuerySingle("Select CmpID From Preboarding_Mas_employee where Deactivate = 0 and Fid='" + Session["pid"] + "'");
                    decimal CmpId = GetcmpId.CmpID;
                    var SMTPGET = DapperORM.DynamicQueryList("Select CmpId, Origin, SMTPServerName, PortNo, SSL, FromEmailId, Password from Tool_EmailSetting where Deactivate = 0 and CmpId = '" + CmpId + "'  and  Origin =1").ToList();
                    if (SMTPGET.Count != 0)
                    {
                        clsEmail emailsaveProcess = new clsEmail();
                        emailsaveProcess.SendMail(int.Parse(Session["pid"].ToString()), Convert.ToInt16(CmpId), "1", "1");
                        TempData["Message"] = "Mail has been Send";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        TempData["Message"] = "SMTP Server Name Not Found";
                        TempData["Icon"] = "error";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                    //clsEmail email = new clsEmail();
                    //email.SendMail(int.Parse(Session["PID"].ToString()), Convert.ToInt16(CmpId), "1", "1");
                    //TempData["Message"] = "SMTP Server Name Not Found";
                    //TempData["Icon"] = "error";
                    //return Json(new {  Message= TempData["Message"], Icon=TempData["Icon"]  }, JsonRequestBehavior.AllowGet);
                    ////return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Cancel 
        public ActionResult CancelCandidate(string FID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (FID_Encrypted != null)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    var Module_Preboarding_Cancel = DapperORM.DynamicQuerySingle("Select DocNo,Verify_FinalApproval_RejectRemark,FID_Encrypted,DocDate,Mas_CompanyProfile.CompanyName,MAs_Branch.BranchName,(Preboarding_Mas_Employee.SalutationName +Preboarding_Mas_Employee.EmployeeName) as CandidateName,Mas_Department.DepartmentName,Mas_Designation.DesignationName,FID from Preboarding_Mas_Employee join Mas_CompanyProfile on Mas_CompanyProfile.CompanyId=Preboarding_Mas_Employee.CmpId join Mas_Branch on Mas_Branch.BranchId=Preboarding_Mas_Employee.BranchID join Mas_Department on Mas_Department.DepartmentId =Preboarding_Mas_Employee.EmployeeDepartmentId join Mas_Designation on Mas_Designation.DesignationId=Preboarding_Mas_Employee.EmployeeDesignationId where Preboarding_Mas_Employee.Deactivate=0  and Mas_CompanyProfile.Deactivate=0 and Mas_Branch.deactivate=0 and FID_Encrypted='" + FID_Encrypted + "'");
                    ViewBag.ListCancel = Module_Preboarding_Cancel;
                }
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult CancelCandidateSubmit(string FID_Encrypted, string Verify_FinalApproval_RejectRemark, int? EmailFID)
        {
            try
            {
                if (FID_Encrypted != null)
                {
                    if (Session["EmployeeId"] == null)
                    {
                        return RedirectToAction("Login", "Login", new { area = "" });
                    }
                    DynamicParameters paramCancel = new DynamicParameters();
                    paramCancel.Add("@p_process", "Cancel");
                    paramCancel.Add("@p_Preboarding_Mas_Employee_FID_Encrypted", FID_Encrypted);
                    paramCancel.Add("@p_EmployeeID", Session["EmployeeId"]);
                    paramCancel.Add("@p_CancelRemarks", Verify_FinalApproval_RejectRemark);
                    paramCancel.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    paramCancel.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_Preboarding_CancelPreboarding", paramCancel);
                    var Message = paramCancel.Get<string>("@p_msg");
                    var Icon = paramCancel.Get<string>("@p_Icon");

                    if (EmailFID != null)
                    {
                        Session["PID"] = EmailFID;
                        var GetcmpId = DapperORM.DynamicQuerySingle("Select CmpID From Preboarding_Mas_employee where Deactivate = 0 and Fid='" + Session["PID"] + "'");
                        var CmpId = GetcmpId.CmpID;
                        clsEmail email = new clsEmail();
                        email.SendMail(int.Parse(Session["PID"].ToString()), Convert.ToInt16(CmpId), "1", "10");
                    }
                    return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
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

        #region Reopen Link 
        public ActionResult ReopenLinkCandidate(string FID_Encrypted, int? EmailFID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (FID_Encrypted != null)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    var Module_Preboarding_Cancel = DapperORM.DynamicQuerySingle("Select DocNo,FID_Encrypted,PreboardSubmit,PreboardSubmitDate,DocDate,Mas_CompanyProfile.CompanyName,MAs_Branch.BranchName,(Preboarding_Mas_Employee.SalutationName +Preboarding_Mas_Employee.EmployeeName) as CandidateName,Mas_Department.DepartmentName,Mas_Designation.DesignationName,FID from Preboarding_Mas_Employee join Mas_CompanyProfile on Mas_CompanyProfile.CompanyId=Preboarding_Mas_Employee.CmpId join Mas_Branch on Mas_Branch.BranchId=Preboarding_Mas_Employee.BranchID join Mas_Department on Mas_Department.DepartmentId =Preboarding_Mas_Employee.EmployeeDepartmentId join Mas_Designation on Mas_Designation.DesignationId=Preboarding_Mas_Employee.EmployeeDesignationId where Preboarding_Mas_Employee.Deactivate=0  and Mas_CompanyProfile.Deactivate=0 and Mas_Branch.deactivate=0 and FID_Encrypted='" + FID_Encrypted + "'");
                    ViewBag.ListReopen = Module_Preboarding_Cancel;
                }
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult ReopenLinkSubmit(string FID_Encrypted, int? EmailFID)
        {
            try
            {
                //    1 Preboarding Link
                //    2 Candidate Login Alert
                //    3 Candidate Final Submit
                //    4 Candidate Partial Rejection
                //    5 Candidate Fully Rejected
                //    6 Candidate Approved
                //    7 Candidate Joining Notification
                //    8 Preboarding Link Reopen
                //    9 Preboarding Link Expiry
                //    10 Candidate Cancel
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramReopen = new DynamicParameters();
                //paramReopen.Add("@p_process", "Reopen");
                paramReopen.Add("@p_Preboarding_Mas_Employee_FID_Encrypted", FID_Encrypted);
                paramReopen.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                paramReopen.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_Preboarding_ReOpen", paramReopen);
                var Message = paramReopen.Get<string>("@p_msg");
                var Icon = paramReopen.Get<string>("@p_Icon");

                if (EmailFID != null)
                {
                    Session["PID"] = EmailFID;
                    var GetcmpId = DapperORM.DynamicQuerySingle("Select CmpID From Preboarding_Mas_employee where Deactivate = 0 and Fid='" + Session["PID"] + "'");
                    var CmpId = GetcmpId.CmpID;
                    clsEmail email = new clsEmail();
                    email.SendMail(int.Parse(Session["PID"].ToString()), Convert.ToInt16(CmpId), "1", "8");
                }
                return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
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