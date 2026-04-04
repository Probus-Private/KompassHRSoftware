using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Recruitment;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Recruitment
{
    public class ESS_Recruitment_CandidateDataController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_Recruitment_CandidateData
        #region MyCandidateData MAin View
        [HttpGet]
        public ActionResult ESS_Recruitment_CandidateData(string ResumeId_Encrypted, int? Resume_ResourceId, string ResourceId_Encrypted)
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
                ViewBag.AddUpdateTitle = "Add";
                //   var ResourceDataa = DapperORM.DynamicQuerySingle("SELECT * FROM Recruitment_ResourceRequest WHERE ResourceId ='"+ Resume_ResourceId + "'");
                //int DesgId = Convert.ToInt32(ResourceDataa?.DesignationID ?? 0);

                // int DesgId = Convert.ToInt32(ResourceDataa.DesignationID);


                Session["Resume_ResourceId"] = Resume_ResourceId;
                var results = DapperORM.DynamicQueryMultiple(@"Select ResumeSourceId  as Id,ResumeSource  as Name  from Recruitment_ResumeSource where deactivate=0;
                                                     Select QualificationPFId as Id,QualificationPFName as Name from Mas_Qualification_PF Where Deactivate = 0 order by Name;
                                                     Select CountryId as Id,Nationality as Name from Mas_Country Where Deactivate = 0 order by IsDefault;
                                                     select NoticePeriodDaysId  as Id,NoticePeriodDays  as Name from Recruitment_NoticePeriodDays where deactivate=0;
                                                     select EmployeeId as Id , concat(Mas_Employee.EmployeeName,' - ' ,Mas_Employee.EmployeeNo)   as Name  from Mas_Employee where deactivate=0 and EmployeeLeft=0 and ContractorID=1 and EmployeeId<>1 order by Name;
                                                     select DesignationId as Id, DesignationName as Name FROM Mas_Designation WHERE  Deactivate = 0;
                                                      Select ResumeSubSourceId  as Id,ResumeSubSource  as Name  from Recruitment_ResumeSubSource where deactivate=0 ");

                ViewBag.ResumeSource = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetQualificationName = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetNationalityName = results[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.NoticePeriod = results[3].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.ReferEmployee = results[4].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.Designation = results[5].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.ResumeSubSource = results[6].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                Session["ResourceId"] = Resume_ResourceId;
                //Session["ResourceId_Encrypted"] = ResourceId_Encrypted;
                if (Resume_ResourceId != null)
                {
                    DynamicParameters param1 = new DynamicParameters();

                    var ResourceId_Encrypted1 = DapperORM.DynamicQuerySingle("SELECT * FROM Recruitment_ResourceRequest WHERE ResourceId ='" + Resume_ResourceId + "'");
                    var ResourceEncryptedId = ResourceId_Encrypted1.ResourceId_Encrypted;
                    param1.Add("@p_EncryptedId", ResourceEncryptedId);
                    param1.Add("@p_Origin", "ApprovalRequest");
                    var ResourcePool = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_Profiles", param1).FirstOrDefault();
                    if (ResourcePool != null)
                    {
                        ViewBag.ResourceApprovalList = ResourcePool;
                    }
                    else
                    {
                        ViewBag.ResourceApprovalList = "";
                    }
                }



                var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Recruitment_Resume";
                var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                ViewBag.DocNo = DocNo;
                Recruitment_Resume RecruitmentResume = new Recruitment_Resume();
                if (ResumeId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_ResumeId_Encrypted", ResumeId_Encrypted);
                    RecruitmentResume = DapperORM.ReturnList<Recruitment_Resume>("sp_List_Recruitment_Resume_ReferenceESS", param).FirstOrDefault();
                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {
                        var GetDocNos = "Select DocNo As DocNo from Recruitment_Resume where ResumeId_Encrypted='" + ResumeId_Encrypted + "'";
                        var DocNos = DapperORM.DynamicQuerySingle(GetDocNos);
                        ViewBag.DocNo = DocNos;
                    }
                    TempData["ResumeSource"] = RecruitmentResume.ResumeSource;
                    TempData["DocDate"] = RecruitmentResume.DocDate;
                    TempData["DOB"] = RecruitmentResume.DOB;
                    TempData["EJD"] = RecruitmentResume.ExpectedJoiningDate;
                    TempData["FilePath"] = RecruitmentResume.FilePath;
                    TempData["ServingNoticePeriod"] = RecruitmentResume.ServingNoticePeriod;
                    TempData["FileName"] = RecruitmentResume.ResumePath;
                    TempData["LeaglBoand"] = RecruitmentResume.LeaglBoand;
                    TempData["HoldingAnyOffer"] = RecruitmentResume.HoldingAnyOffer;
                    Session["SelectedFile"] = RecruitmentResume.ResumePath;
                }
                return View(RecruitmentResume);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region IsEmailidExist
        public ActionResult IsEmailidExist(string ResumeId_Encrypted, string EmailId)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_ResumeId_Encrypted", ResumeId_Encrypted);
                    param.Add("@p_EmailId", EmailId);
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeId"]?.ToString());
                    param.Add("@p_MachineName", System.Net.Dns.GetHostName());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    sqlcon.Execute("sp_SUD_Recruitment_PoolCandidatesUpload", param, commandType: CommandType.StoredProcedure);
                    var Message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    if (!string.IsNullOrEmpty(Message))
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
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region IsMobileExist
        public ActionResult IsMobileNoExist(string ResumeId_Encrypted, string MobileNo)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_ResumeId_Encrypted", ResumeId_Encrypted);
                    param.Add("@p_MobileNo", MobileNo);
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeId"]?.ToString());
                    param.Add("@p_MachineName", System.Net.Dns.GetHostName());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    sqlcon.Execute("sp_SUD_Recruitment_PoolCandidatesUpload", param, commandType: CommandType.StoredProcedure);
                    var Message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    if (!string.IsNullOrEmpty(Message))
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
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        public ActionResult GetSubSource(int ResumeSourceId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("Query", "Select ResumeSubSourceId  as Id,ResumeSubSource  as Name  from Recruitment_ResumeSubSource where ResumeSourceId='" + ResumeSourceId + "' and Deactivate=0");
                var GetSubSource = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                var result = GetSubSource;
                var data = new { result };

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Recruitment_Resume RecruitmentResume, HttpPostedFileBase ResumePath)
        {
            try
            {
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(RecruitmentResume.ResumeId_Encrypted) ? "Save" : "Update");
                param.Add("@p_ResumeId_Encrypted", RecruitmentResume.ResumeId_Encrypted);
                param.Add("@p_Salutation", RecruitmentResume.Salutation);
                param.Add("@p_CandidateName", RecruitmentResume.CandidateName);
                param.Add("@p_DocNo", RecruitmentResume.DocNo);
                param.Add("@p_DocDate", RecruitmentResume.DocDate);
                param.Add("@p_ResumeReferEmployeeId", RecruitmentResume.ResumeReferEmployeeId);
                param.Add("@p_Resume_ResourceId", Session["Resume_ResourceId"]);
                param.Add("@p_HighestQualification", RecruitmentResume.HighestQualification);
                param.Add("@p_QualificationRemark", RecruitmentResume.QualificationRemark);
                param.Add("@p_CurrentlyWorking", RecruitmentResume.CurrentlyWorking);
                param.Add("@p_MaritalStatus", RecruitmentResume.MaritalStatus);
                param.Add("@p_EmailId", RecruitmentResume.EmailId);
                param.Add("@p_DOB", RecruitmentResume.DOB);
                param.Add("@p_MobileNo", RecruitmentResume.MobileNo);
                param.Add("@p_AlternateMobileNo", RecruitmentResume.AlternateMobileNo);
                param.Add("@p_Gender", RecruitmentResume.Gender);
                param.Add("@p_RelevantSkill", RecruitmentResume.RelevantSkill);

                param.Add("@p_ResumeUploadEmployeeId", EmployeeId);
                param.Add("@p_TotalExperience", RecruitmentResume.TotalExperience);
                param.Add("@p_CurrentCity", RecruitmentResume.CurrentCity);
                param.Add("@p_RelevantExperience", RecruitmentResume.RelevantExperience);
                param.Add("@p_LastCompanyName", RecruitmentResume.LastCompanyName);
                param.Add("@p_ResumeCategory", RecruitmentResume.ResumeCategory);
                param.Add("@p_CTC", RecruitmentResume.CTC);
                param.Add("@p_ExpectedCTC", RecruitmentResume.ExpectedCTC);
                param.Add("@p_LastDesignation", RecruitmentResume.LastDesignation);
                param.Add("@p_SutableForDesignation", RecruitmentResume.SutableForDesignation);
                param.Add("@p_Nationality", RecruitmentResume.Nationality);
                param.Add("@p_FamilyDetails", RecruitmentResume.FamilyDetails);
                param.Add("@p_Age", RecruitmentResume.Age);
                param.Add("@p_LeaglBoand", RecruitmentResume.LeaglBoand);
                param.Add("@p_BondMonth", RecruitmentResume.BondMonth);
                param.Add("@p_NoticePeriodDays", RecruitmentResume.NoticePeriodDays);
                param.Add("@p_ServingNoticePeriod", RecruitmentResume.ServingNoticePeriod);
                param.Add("@p_LastWorkingDay", RecruitmentResume.LastWorkingDay);
                param.Add("@p_HoldingAnyOffer", RecruitmentResume.HoldingAnyOffer);
                param.Add("@p_OfferRemark", RecruitmentResume.OfferRemark);
                param.Add("@p_TechnicalDetails", RecruitmentResume.TechnicalDetails);
                param.Add("@p_CommunicationSkill", RecruitmentResume.CommunicationSkill);
                param.Add("@p_JobDescription", RecruitmentResume.JobDescription);
                param.Add("@p_ExpatLocal", RecruitmentResume.ExpatLocal);
                param.Add("@p_ReadyToTraval", RecruitmentResume.ReadyToTraval);
                param.Add("@p_ResumeSource", RecruitmentResume.ResumeSource);
                param.Add("@p_HrRanking", RecruitmentResume.HrRanking);
                param.Add("@p_HrSelection", RecruitmentResume.HrSelection);
                param.Add("@p_ResumeStatus", "Pending");
                param.Add("@p_Refreance", RecruitmentResume.Refreance);
                param.Add("@p_HRComment", RecruitmentResume.HRComment);
                param.Add("@p_CandidateComment", RecruitmentResume.CandidateComment);
                param.Add("@p_CandidateType", RecruitmentResume.CandidateType);
                param.Add("@p_IsManualAuto", "Manual");
                param.Add("@p_Origin", "CandidateUpload");
                param.Add("@p_ResumeAgencyId", 1);
                param.Add("@p_ResumeSubSource", RecruitmentResume.ResumeSubSource);
                param.Add("@p_ExpectedJoiningDate", RecruitmentResume.ExpectedJoiningDate);
                if (RecruitmentResume.ResumeId_Encrypted != null && ResumePath == null)
                {
                    param.Add("@p_ResumePath", Session["SelectedFile"]);
                }
                else
                {
                    param.Add("@p_ResumePath", ResumePath == null ? "" : ResumePath.FileName);
                }
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_PoolCandidatesUpload", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                if (TempData["P_Id"] != null)
                {

                    if (ResumePath != null)
                    {
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='RecruitmentCandidateData'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        var FirstPath = GetFirstPath + TempData["P_Id"] + "\\"; // First path plus concat folder 
                        string directoryPath = Path.GetDirectoryName(FirstPath);
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(FirstPath);
                        }
                        string AttachFilePath = "";
                        AttachFilePath = FirstPath + ResumePath.FileName; //Concat Full Path and create New full Path
                        ResumePath.SaveAs(AttachFilePath); // This is use for Save image in folder full path
                    }
                }
                else
                {

                    if (ResumePath != null)
                    {
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='RecruitmentCandidateData'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        var FirstPath = GetFirstPath + RecruitmentResume.ResumeId + "\\"; // First path plus concat folder 
                        string directoryPath = Path.GetDirectoryName(FirstPath);
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(FirstPath);
                        }
                        string AttachFilePath = "";
                        AttachFilePath = FirstPath + ResumePath.FileName; //Concat Full Path and create New full Path
                        ResumePath.SaveAs(AttachFilePath); // This is use for Save image in folder full path
                    }
                }
                return RedirectToAction("ESS_Recruitment_CandidateData", "ESS_Recruitment_CandidateData", new { Resume_ResourceId = Convert.ToInt32(Session["Resume_ResourceId"]) });
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 130;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_ResumeId_Encrypted", "List");
                var CandidateData = DapperORM.DynamicList("sp_List_Recruitment_Resume_ReferenceESS", param);
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

        #region Delete
        public ActionResult Delete(string ResumeId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_ResumeId_Encrypted", ResumeId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_PoolCandidatesUpload", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("AllCandidateData", "ESS_Recruitment_RequestPool");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Download Image 
        //public ActionResult DownloadFile(string FilePath)
        //{
        //    try
        //    {
        //        if (FilePath != "File Not Found")
        //        {
        //            if (string.IsNullOrEmpty(FilePath))
        //            {
        //                return Json(new { Success = false, Message = "Invalid File.", Icon = "error" }, JsonRequestBehavior.AllowGet);
        //            }

        //            var fullPath = FilePath;
        //            if (!System.IO.File.Exists(fullPath))
        //            {
        //                return Json(new { Success = false, Message = "File not found on your server.", Icon = "error" }, JsonRequestBehavior.AllowGet);
        //            }

        //            var driveLetter = Path.GetPathRoot(FilePath);
        //            if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
        //            {
        //                return Json(new { Success = false, Message = $"Drive {driveLetter} does not exist.", Icon = "error" }, JsonRequestBehavior.AllowGet);
        //            }


        //            var fileName = Path.GetFileName(fullPath);
        //            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
        //            var fileBase64 = Convert.ToBase64String(fileBytes);

        //            return Json(new
        //            {
        //                Success = true,
        //                FileName = fileName,
        //                FileData = fileBase64,
        //                ContentType = MediaTypeNames.Application.Octet
        //            }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            TempData["Message"] = "Upload a file";
        //            TempData["Icon"] = "error";
        //            //return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
        //            return RedirectToAction("GetList", "ESS_ClaimReimbusement_TravelClaim");
        //        }
        //        //if (FilePath != "")
        //        //{
        //        //    if (System.IO.File.Exists(FilePath))
        //        //    {
        //        //        System.IO.File.ReadAllBytes(FilePath);
        //        //        return File(FilePath, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(FilePath));
        //        //    }
        //        //    else
        //        //    {
        //        //        return Json(false);
        //        //    }
        //        //}
        //        //else
        //        //{
        //        //    return RedirectToAction("GetList", "ESS_ClaimReimbusement_TravelClaim", new { Area = "ESS" });
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
        //}

        public ActionResult DownloadFile(string FilePath)
        {
            try
            {
                //var GetDrivePath = (from d in dbContext.Onboarding_Mas_DocumentPath select d.DosumentPath).FirstOrDefault();
                //var fullPath = GetDrivePath + filePath;
                //byte[] fileBytes = GetFile(fullPath);
                if (FilePath != null)
                {
                    System.IO.File.ReadAllBytes(FilePath);
                    return File(FilePath, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(FilePath));
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
    }
}