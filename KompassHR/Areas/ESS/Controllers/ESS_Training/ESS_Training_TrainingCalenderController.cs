using System;
using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Training;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Dynamic;

namespace KompassHR.Areas.ESS.Controllers.ESS_Training
{
    public class ESS_Training_TrainingCalenderController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region Training Calendar main View
        [HttpGet]
        public ActionResult ESS_Training_TrainingCalender(string TrainingCalenderId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 557;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";

                Training_TrainingCalender Training_Calender = new Training_TrainingCalender();

                DynamicParameters paramCategory = new DynamicParameters();
                var GetCategoryName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCategoryDropdown", paramCategory).ToList();
                var CID = GetCategoryName.FirstOrDefault()?.Id;
                ViewBag.CategoryName = GetCategoryName;

                DynamicParameters paramtrainingtype = new DynamicParameters();
                var GetTrainingType = DapperORM.ReturnList<AllDropDownBind>("sp_GetTrainingTypeDropdown", paramtrainingtype).ToList();
                var TID = GetTrainingType.FirstOrDefault()?.Id;
                ViewBag.GetTrainingType = GetTrainingType;

                DynamicParameters paramtrainingfrequency = new DynamicParameters();
                var GetTrainingFrequency = DapperORM.ReturnList<AllDropDownBind>("sp_GetTrainingFrequencyDropdown", paramtrainingfrequency).ToList();
                var FID = GetTrainingFrequency.FirstOrDefault()?.Id;
                ViewBag.GetTrainingFrequency = GetTrainingFrequency;

                DynamicParameters paramagency = new DynamicParameters();
                var GetAgency = DapperORM.ReturnList<AllDropDownBind>("sp_GetAgencyDropdown", paramagency).ToList();
                var AID = GetAgency.FirstOrDefault()?.Id;
                ViewBag.GetAgency = GetAgency;

                DynamicParameters paramtrainer = new DynamicParameters();
                var GetTrainer = DapperORM.ReturnList<AllDropDownBind>("sp_GetTrainerDropdown", paramtrainer).ToList();
                var GID = GetTrainer.FirstOrDefault()?.Id;
                ViewBag.GetTrainer = GetTrainer;

                ViewBag.GetIntenalTraineeEmployee = "";
                DynamicParameters paramassesmenttype = new DynamicParameters();
                var GetAssesmentType = DapperORM.ReturnList<AllDropDownBind>("sp_GetAssesmentTypeDropdown", paramassesmenttype).ToList();
                var MID = GetAssesmentType.FirstOrDefault()?.Id;
                ViewBag.GetAssesmentType = GetAssesmentType;
                ViewBag.SubcatergoryList = "";
                var results = DapperORM.DynamicQueryMultiple(@"SELECT  DepartmentId as Id,DepartmentName as Name FROM Mas_Department WHERE Deactivate =0;");
                ViewBag.DepatmentList = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                //ViewBag.DepatmentList = results.Read<AllDropDownBind>().ToList();

                if (TrainingCalenderId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@P_TrainingCalenderId_Encrypted", TrainingCalenderId_Encrypted);
                    Training_Calender = DapperORM.ReturnList<Training_TrainingCalender>("sp_List_TrainingCalender", param).FirstOrDefault();
                    TempData["QRScanImage"] = Training_Calender.EnrollmentAttachment;
                    TempData["MonthYear"] = Training_Calender.MonthYear.ToString("yyyy-MM-ddTHH:mm");
                    TempData["EnrollmentLastDate"] = Training_Calender.EnrollmentLastDate.ToString("yyyy-MM-dd");
                    if (!string.IsNullOrEmpty(Training_Calender.DepartmentId))
                    {
                        ViewBag.SelectedDepartmentIds = Training_Calender.DepartmentId.Split(',').Select(id => id.Trim()).ToList();
                    }
                    else
                    {
                        ViewBag.SelectedDepartmentIds = new List<string>();
                    }
                    if (!string.IsNullOrEmpty(Training_Calender.TraningSubCategoryId))
                    {
                        ViewBag.SelectedSubCategoryIds = Training_Calender.TraningSubCategoryId.Split(',').Select(id => id.Trim()).ToList();
                    }
                    else
                    {
                        ViewBag.SelectedSubCategoryIds = new List<string>();
                    }

                    if (Training_Calender.TrainingProviderSource == "Internal")
                    {
                        DynamicParameters paramInternalTrainer = new DynamicParameters();
                        paramInternalTrainer.Add("@p_CatId", Training_Calender.TraningCategoryId);
                        var internalTrainers = DapperORM.ReturnList<AllDropDownBind>("sp_GetInternalEmployeeDropdown", paramInternalTrainer).ToList();
                        ViewBag.GetIntenalTraineeEmployee = internalTrainers;
                    }
                    else
                    {
                        ViewBag.GetIntenalTraineeEmployee = new List<SelectListItem>();
                    }
                }

                return View(Training_Calender);
            }

            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 557;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_TrainingCalenderId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_TrainingCalender", param).ToList();
                ViewBag.GetTrainingCalender = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region Internal Trainer
        [HttpGet]
        public ActionResult GetInternalTrainer(int? categoryID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters paramtraineremp = new DynamicParameters();
                paramtraineremp.Add("@p_CatId", categoryID);
                var employees = DapperORM.ReturnList<AllDropDownBind>("sp_GetInternalEmployeeDropdown", paramtraineremp).ToList();
                var subcategories = DapperORM.ReturnList<AllDropDownBind>("sp_GetTrainingSubCategoryDropdown", paramtraineremp).ToList();

                var result = new Training_TrainingCalender
                {
                    InternalEmployees = employees,
                    TrainingSubcategories = subcategories
                };

                return Json(result, JsonRequestBehavior.AllowGet);
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
        public ActionResult SaveUpdate(Training_TrainingCalender Training_calender, HttpPostedFileBase EnrollmentAttachment, HttpPostedFileBase BannerFile)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(Training_calender.TrainingCalenderId_Encrypted) ? "Save" : "Update");
                param.Add("@P_TrainingCalenderId_Encrypted", Training_calender.TrainingCalenderId_Encrypted);
                param.Add("@p_TrainingCalenderName", Training_calender.TrainingCalenderName);
                param.Add("@p_TrainingDescription", Training_calender.TrainingDescription);
                param.Add("@p_TraningCategoryId", Training_calender.TraningCategoryId);
                param.Add("@p_TraningSubCategoryId", Training_calender.SelectedTrainingSubcategories);
                param.Add("@p_TrainingTypeId", Training_calender.TrainingTypeId);
                param.Add("@p_AssesmentTypeId", Training_calender.AssesmentTypeId);
                param.Add("@p_MonthYear", Training_calender.MonthYear);
                param.Add("@p_DepartmentId", Training_calender.SelectedDepartments);
                param.Add("@p_ModeOfTraining", Training_calender.ModeOfTraining);
                param.Add("@p_Location", Training_calender.Location);
                param.Add("@p_TrainingfrequencyId", Training_calender.TrainingfrequencyId);
                param.Add("@p_TrainingProviderSource", Training_calender.TrainingProviderSource);

                param.Add("@p_IsEnrollmentRequired", Training_calender.IsEnrollmentRequired);
                //param.Add("@p_EnrollmentLastDate", Training_calender.EnrollmentLastDate);
                if (Training_calender.EnrollmentLastDate != DateTime.MinValue)
                {
                    param.Add("@p_EnrollmentLastDate", Training_calender.EnrollmentLastDate);
                }

                param.Add("@p_TrainingFeesType", Training_calender.TrainingFeesType);
                param.Add("@p_TrainingFeesPaidType", Training_calender.TrainingFeesPaidType);
                param.Add("@p_TrainingFeesAmount", Training_calender.TrainingFeesAmount);
                //Banner File QR Code File
                if (EnrollmentAttachment != null)
                {
                    string base64String = "";
                    using (var binaryReader = new BinaryReader(EnrollmentAttachment.InputStream))
                    {
                        byte[] imageBytes = binaryReader.ReadBytes(EnrollmentAttachment.ContentLength);
                        base64String = Convert.ToBase64String(imageBytes);
                    }
                    param.Add("@p_EnrollmentAttachment", base64String);
                }


                if (BannerFile != null)
                {
                    param.Add("@p_BannerFile", BannerFile.FileName);
                }

                //if (Training_calender.TrainingCalenderId_Encrypted != null)
                //{

                //    if (EnrollmentAttachment != null)
                //    {
                //        param.Add("@p_EnrollmentAttachment", EnrollmentAttachment.FileName);
                //    }
                //}
                //else
                //{
                //    if (EnrollmentAttachment != null)
                //    {
                //        param.Add("@p_EnrollmentAttachment", EnrollmentAttachment.FileName);
                //    }
                //}
                //Banner File
                //if (Training_calender.BannerFile != null)
                //{
                //    if (BannerFile != null)
                //    {
                //        param.Add("@p_BannerFile", BannerFile.FileName);
                //    }
                //}
                //else
                //{
                //    if (BannerFile != null)
                //    {
                //        param.Add("@p_BannerFile", BannerFile.FileName);
                //    }
                //}


                if (Training_calender.TrainingProviderSource == "Internal")
                {
                    param.Add("@p_TrainingProviderId", Training_calender.TrainingProviderId);
                }
                else if (Training_calender.TrainingProviderSource == "External")
                {
                    param.Add("@p_TrainingProviderId", Training_calender.TrainerId);
                }
                else
                {
                    param.Add("@p_TrainingProviderId", Training_calender.TrainingAgencyId);
                }
                param.Add("@p_TrainingProvidedBy", Training_calender.TrainingProvidedBy);
                param.Add("@p_MaxParticipants", Training_calender.MaxParticipants);
                param.Add("@p_MinParticipants", Training_calender.MinParticipants);
                param.Add("@p_EnrolledCount", Training_calender.EnrolledCount);
                // param.Add("@p_WaitList", Training_calender.WaitList);
                param.Add("@p_AssesmentRequired", Training_calender.AssesmentRequired);

                param.Add("@p_CourseMaterial", Training_calender.CourseMaterial);
                param.Add("@p_FoodIncluded", Training_calender.FoodIncluded);
                param.Add("@p_Accommodation", Training_calender.Accommodation);
                param.Add("@p_Travel", Training_calender.Travel);
                param.Add("@p_Certificate", Training_calender.Certificate);
                param.Add("@p_GPSLattitude", Training_calender.GPSLattitude);
                param.Add("@p_GPSLongitude", Training_calender.GPSLongitude);

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_TrainingCalender", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                var GetPid = param.Get<string>("@p_Id");

                if (GetPid != null)
                {
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Training'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    var FirstPath = GetFirstPath + "\\" + "TrainingCalender" + "\\" + GetPid + "\\";// First path plus concat folder by Id

                    var driveLetter = Path.GetPathRoot(FirstPath);
                    // Check if the drive exists
                    if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
                    {
                        TempData["Message"] = $"Drive {driveLetter} does not exist.";
                        //if (!System.IO.File.Exists(FirstPath))
                        //{
                        //    TempData["Message"] = "File not found on the server.";
                        //    TempData["Icon"] = "error";
                        //    return RedirectToAction("ESS_Training_TrainingCalender");
                        //}                        TempData["Icon"] = "error";
                        return RedirectToAction("ESS_Training_TrainingCalender");
                    }
                    // Check if the file exists

                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }

                    if (EnrollmentAttachment != null)
                    {
                        // Clear existing files in the folder (if any)
                        var directory = new DirectoryInfo(FirstPath);
                        foreach (var file in directory.GetFiles())
                        {
                            file.Delete();
                        }
                        string ImgGeneralClaimFilePath = "";
                        ImgGeneralClaimFilePath = FirstPath + EnrollmentAttachment.FileName; //Concat Full Path and create New full Path
                        EnrollmentAttachment.SaveAs(ImgGeneralClaimFilePath); // This is use for Save image in folder full path
                    }
                }


                //THIS CODE FOR BANNER FILE SAVE ON SERVER
                if (BannerFile != null)
                {
                    var GetDocPath1 = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Training'");
                    var GetFirstPath1 = GetDocPath1.DocInitialPath;

                    var FirstPath1 = GetFirstPath1 + "\\" + "TrainingCalender" + "\\";
                    //string directoryPath = Path.GetDirectoryName(FirstPath1);
                    //if (!Directory.Exists(directoryPath))
                    //{
                    //    Directory.CreateDirectory(FirstPath1);
                    //}
                    var DocRead = DapperORM.DynamicQuerySingle("Select * from Tool_LMS_DownloadUpload_URL where DocOrigin='TrainingUpload'");
                    var GetDocReadURL = DocRead.DownloadUploadURL;
                    string VerificationUrl = GetDocReadURL;

                    using (HttpClient client = new HttpClient())
                    {
                        using (var formData = new MultipartFormDataContent())
                        {
                            // Add string parameters
                            formData.Add(new StringContent(FirstPath1), "FilePath");
                            formData.Add(new StringContent(GetPid), "LMSLibraryId");
                            if (BannerFile != null && BannerFile.ContentLength > 0)
                            {
                                var streamContent = new StreamContent(BannerFile.InputStream);
                                streamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                                {
                                    Name = "\"File\"",
                                    FileName = $"\"{"Banner" + "_" + BannerFile.FileName}\""
                                };
                                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(BannerFile.ContentType); // e.g., application/pdf

                                formData.Add(streamContent, "File", BannerFile.FileName);
                            }

                            // Send the form-data content
                            HttpResponseMessage response = client.PostAsync(VerificationUrl, formData).GetAwaiter().GetResult();
                            if (response.IsSuccessStatusCode)
                            {
                                string jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                var FinalData = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                            }
                        }
                    }
                }
                return RedirectToAction("ESS_Training_TrainingCalender", "ESS_Training_TrainingCalender");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(double? TrainingCalenderId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_TrainingCalenderId", TrainingCalenderId);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_TrainingCalender", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("GetList", "ESS_Training_TrainingCalender");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region TrainingUploadFile
        public ActionResult TrainingUploadFile(string TrainingCalenderId_Encrypted, int? TrainingCalenderId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 557;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Session["TrainingCalenderId"] = TrainingCalenderId;
                Session["TrainingCalenderId_Encrypted"] = TrainingCalenderId_Encrypted;
                param.Add("@p_TrainingCalenderDocId_Encrypted", "List");
                param.Add("@p_TrainingCalenderId", Session["TrainingCalenderId"]);
                var data = DapperORM.ReturnList<dynamic>("sp_List_TrainingCalender_Document", param).ToList();
                ViewBag.GetListTrainingCalender = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region File SaveUpdate on Server
        [HttpPost]
        public ActionResult SaveUpdateUploadFile(TrainingCalender_Document Library, HttpPostedFileBase DocumentPath)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                string extension = Path.GetExtension(DocumentPath.FileName).ToLower();
                param.Add("@p_process", string.IsNullOrEmpty(Library.TrainingCalenderDocId_Encrypted) ? "Save" : "Update");
                param.Add("@p_TrainingCalenderDocId_Encrypted", Library.TrainingCalenderDocId_Encrypted);
                param.Add("@p_Training_TrainingCalender_TrainingCalenderId", Session["TrainingCalenderId"]);
                param.Add("@p_Title", Library.Title);
                if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                {
                    param.Add("@p_FilteType", "Image");
                }
                else if (extension == ".pdf")
                {
                    param.Add("@p_FilteType", "PDF");
                }
                else if (extension == ".mp4")
                {
                    param.Add("@p_FilteType", "Vedio");
                }

                if (Library.TrainingCalenderDocId_Encrypted != null && DocumentPath == null)
                {
                    param.Add("@p_DocumentPath", Session["SelectedFile"]);
                }
                else
                {
                    param.Add("@p_DocumentPath", DocumentPath == null ? "" : DocumentPath.FileName);
                }
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_TrainingCalender_Document", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                var DocPKId = param.Get<string>("@p_Id");
                if (Session["TrainingCalenderId"] != null)
                {
                    if (Library.DocumentPath != null)
                    {
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Training'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        var FirstPath = GetFirstPath + Convert.ToInt32(Session["TrainingCalenderId"]) + "\\"; // First path plus concat folder 
                        string directoryPath = Path.GetDirectoryName(FirstPath);
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(FirstPath);
                        }
                        string AttachFilePath = "";
                        AttachFilePath = FirstPath + DocPKId + "_" + DocumentPath.FileName; //Concat Full Path and create New full Path
                        DocumentPath.SaveAs(AttachFilePath); // This is use for Save image in folder full path

                        var DocRead = DapperORM.DynamicQuerySingle("Select * from Tool_LMS_DownloadUpload_URL where DocOrigin='TrainingUpload'");
                        var GetDocReadURL = DocRead.DownloadUploadURL;
                        string VerificationUrl = GetDocReadURL;
                        //string VerificationUrl = "http://10.48.218.3:8096/api/LMSDocUpload";
                        using (HttpClient client = new HttpClient())
                        {
                            using (var formData = new MultipartFormDataContent())
                            {
                                // Add string parameters
                                formData.Add(new StringContent(GetFirstPath + "\\TrainingCalender"), "FilePath");
                                formData.Add(new StringContent(Session["TrainingCalenderId"].ToString()), "LMSLibraryId");

                                if (DocumentPath != null && DocumentPath.ContentLength > 0)
                                {
                                    var streamContent = new StreamContent(DocumentPath.InputStream);
                                    streamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                                    {
                                        Name = "\"File\"",
                                        FileName = $"\"{DocPKId + "_" + DocumentPath.FileName}\""
                                    };
                                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(DocumentPath.ContentType); // e.g., application/pdf

                                    formData.Add(streamContent, "File", DocumentPath.FileName);
                                }

                                // Send the form-data content
                                HttpResponseMessage response = client.PostAsync(VerificationUrl, formData).GetAwaiter().GetResult();

                                if (response.IsSuccessStatusCode)
                                {
                                    string jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                    var FinalData = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                                }
                            }
                        }
                    }
                }
                else
                {

                    if (DocumentPath != null)
                    {
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Training'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        var FirstPath = GetFirstPath + Library.TrainingCalenderId + "\\"; // First path plus concat folder 
                        string directoryPath = Path.GetDirectoryName(FirstPath);
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(FirstPath);
                        }
                        string AttachFilePath = "";
                        AttachFilePath = FirstPath + DocumentPath.FileName; //Concat Full Path and create New full Path
                        DocumentPath.SaveAs(AttachFilePath); // This is use for Save image in folder full path
                    }
                }
                return RedirectToAction("TrainingUploadFile", "ESS_Training_TrainingCalender", new { TrainingCalenderId_Encrypted = Session["TrainingCalenderId_Encrypted"], TrainingCalenderId = Session["TrainingCalenderId"] });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        //#region DeleteDocument
        public ActionResult DeleteDocument(string TrainingCalenderDocId_Encrypted, string FileName, int Training_TrainingCalender_TrainingCalenderId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                param.Add("@p_process", "Delete");
                param.Add("@p_TrainingCalenderDocId_Encrypted", TrainingCalenderDocId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_TrainingCalender_Document", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                var DocPKId = param.Get<string>("@p_Id");

                var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='TrainingCalender'");
                var GetFirstPath = GetDocPath.DocInitialPath + "\\" + Training_TrainingCalender_TrainingCalenderId + "\\";

                var DocRead = DapperORM.DynamicQuerySingle("Select * from Tool_LMS_DownloadUpload_URL where DocOrigin='TrainingCalenderDocDelete'");
                var GetDocReadURL = DocRead.DownloadUploadURL;
                string VerificationUrl = GetDocReadURL;
                //string VerificationUrl = "http://10.48.218.3:8096/api/LMSDocDelete";
                using (HttpClient client = new HttpClient())
                {
                    using (var formData = new MultipartFormDataContent())
                    {
                        // Add string parameters
                        var SetFileName = FileName;
                        formData.Add(new StringContent(GetFirstPath), "FilePath");
                        formData.Add(new StringContent(SetFileName), "File");
                        HttpResponseMessage response = client.PostAsync(VerificationUrl, formData).GetAwaiter().GetResult();
                        if (response.IsSuccessStatusCode)
                        {
                            string jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            var FinalData = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                        }
                    }
                }
                return RedirectToAction("TrainingUploadFile", "ESS_Training_TrainingCalender", new { TrainingCalenderId_Encrypted = Session["TrainingCalenderId_Encrypted"], TrainingCalenderId = Session["TrainingCalenderId"] });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


        //#region Download Attachment
        public ActionResult DownloadAttachment(double TrainingCalenderId, string DownloadAttachment)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                // Fetch server path information
                var GetPath = DapperORM.DynamicQuerySingle("SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin='Training'");
                var FirstPath = GetPath.DocInitialPath + DownloadAttachment + "\\"; // Not used here but if needed

                var DocRead = DapperORM.DynamicQuerySingle("SELECT * FROM Tool_LMS_DownloadUpload_URL WHERE DocOrigin='TrainingRead'");
                var GetDocRead = DocRead.DownloadUploadURL;
                var GetUrl = GetDocRead + "/TrainingCalender" + "/" + TrainingCalenderId + "/"; // Base URL for the document

                // Construct full photo URL
                if (!string.IsNullOrEmpty(DownloadAttachment))
                {
                    // Combine the base URL with the photo name to create the full path
                    string path = Path.Combine(GetUrl, DownloadAttachment).Replace("\\", "/"); // Ensure it's a valid URL
                                                                                               // TempData["PhotoFile"] = path;
                                                                                               //// Only process if it's a valid image file type
                    string ext = Path.GetExtension(DownloadAttachment)?.TrimStart('.').ToUpper();
                    if (new[] { "PDF", "JPG", "JPEG", "PNG" }.Contains(ext))
                    {
                        TempData["PhotoFile"] = new
                        {
                            FileSrc = path,  // Full URL to the image
                            Title = Path.GetFileName(DownloadAttachment)  // Extract just the file name for the title
                        };
                    }
                }

                return RedirectToAction("GetList", "ESS_Training_TrainingInternal");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        //#endregion
        #endregion

        #endregion

        #region TrainingDocumentView
        public ActionResult TrainingDocumentView(int? TrainingCalenderId)
        {
            try
            {
                if (Session["EmployeeId"] != null)
                {
                    var GetPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Training'");
                    var FirstPath = GetPath?.DocInitialPath + "\\" + TrainingCalenderId + "\\";

                    var DocRead = DapperORM.DynamicQuerySingle("Select * from Tool_LMS_DownloadUpload_URL where DocOrigin='TrainingRead'");
                    var GetDocRead = DocRead?.DownloadUploadURL;
                    var GetUrl = GetDocRead + "/TrainingCalender/" + TrainingCalenderId + "/";

                    var employeeId = Convert.ToInt32(Session["EmployeeId"]);
                    string sql = $@"Select * from TrainingCalender_Document where Deactivate=0 and Training_TrainingCalender_TrainingCalenderId = {TrainingCalenderId};";
                    List<dynamic> GetSecondPath;
                    GetSecondPath = DapperORM.DynamicQueryList($@"Select * from TrainingCalender_Document where Deactivate=0 and Training_TrainingCalender_TrainingCalenderId = {TrainingCalenderId}").ToList();
                    //GetSecondPath = result.Read<dynamic>();
                    //bool GetIsAssessmentActive = false;
                    //using (var result = DapperORM.ExecuteQuery(sql))
                    //{
                    //    GetSecondPath = result.Read<dynamic>().ToList();
                    //}
                    // TempData["IsAssessmentActive"] = GetIsAssessmentActive;

                    var sourcePaths = GetSecondPath
                        .Where(x => x.DocumentPath != null)
                        .Select(x => new
                        {
                            TrainingCalenderDocId = x.TrainingCalenderDocId,
                            FullPath = Path.Combine(GetUrl, (string)x.DocumentPath),
                            DocumentPath = x.DocumentPath,
                            Title = (string)x.Title,
                            Extension = Path.GetExtension((string)x.DocumentPath)?.TrimStart('.').ToUpper(),
                        })
                        .ToList();
                    // Step 2: Filter by valid extensions and build final fileList
                    var fileList = sourcePaths
                        .Where(x => new[] { "PDF", "JPG", "JPEG", "PNG", "MP4", "AVI" }.Contains(x.Extension))
                        .Select(x => new
                        {
                            FileSrc = x.FullPath.Replace("\\", "/"),
                            FileType = x.Extension,
                            DocumentPath = x.DocumentPath,
                            Title = x.Title,
                            TrainingCalenderDocId = x.TrainingCalenderDocId,
                        })
                        .ToList();
                    TempData["FileList"] = fileList;

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

        #region Employee Training Event Calender 
        [HttpGet]
        public ActionResult CalenderEventView(string TrainingCalenderId_Encrypted, string BannerImage)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@P_TrainingCalenderId_Encrypted", TrainingCalenderId_Encrypted);
                var GetData = DapperORM.ReturnList<Training_TrainingCalender>("sp_List_TrainingCalender", param).FirstOrDefault();
                ViewBag.GetCalenderEventDetails = GetData;
                if(BannerImage!=null)
                {
                    TempData["BannerImage"] = BannerImage;
                }
                else
                {
                    var DocRead = DapperORM.DynamicQuerySingle("SELECT * FROM Tool_LMS_DownloadUpload_URL WHERE DocOrigin = 'TrainingRead'");
                    var GetDocRead = DocRead.DownloadUploadURL;
                    var GetUrl = GetDocRead + "/TrainingCalender/";

                    if (GetData != null && !string.IsNullOrEmpty(GetData.BannerFile))
                    {
                        var fullPath = $"{GetUrl.TrimEnd('/')}/{GetData.TrainingCalenderId}/Banner_{GetData.BannerFile}".Replace("\\", "/");
                        TempData["BannerImage"] = fullPath;
                    }

                }

                TempData["QRScanImage"] = GetData.EnrollmentAttachment;

                //string query = @"SELECT COUNT(*) AS TotalInterested FROM Training_Calender_IntrestEnroll_Status
                //WHERE IsIntrested = 1 AND EmployeeId = @EmployeeId AND TrainingCalenderId = (SELECT TrainingCalenderId 
                //FROM Training_Calender WHERE TrainingCalenderId_Encrypted = @EncryptedId)";
                //TempData["TotalIntrestCount"] = DapperORM.DynamicQuerySingle<int>(query, new
                //{
                //    EmployeeId = Session["EmployeeId"],
                //    EncryptedId = TrainingCalenderId_Encrypted
                //});



                var GetCount = DapperORM.DynamicQuerySingle($@"SELECT COUNT(*) AS TotalInterested FROM Training_Calender_IntrestEnroll_Status
                WHERE IsIntrested = 1 AND EmployeeId = " + Session["EmployeeId"] + " AND TrainingCalenderId = (SELECT TrainingCalenderId FROM Training_Calender WHERE TrainingCalenderId_Encrypted='" + TrainingCalenderId_Encrypted + "')");
                TempData["TotalIntrestCount"] = GetCount != null ? GetCount.TotalInterested : null;

                ViewBag.UpcomingEvents = DapperORM.DynamicQueryList($@"SELECT Top(3) tc.TrainingCalenderName
                ,tc.TrainingCalenderId_Encrypted
	            ,tc.TrainingDescription
	            ,subcat.TrainingSubcategory
	            ,c.TrainingCategory
	            ,t.TrainingTypeName
	            ,tc.MonthYear
	            ,dept.DepartmentNames
	            ,tc.ModeOfTraining
	            ,tc.Location
	            ,tc.TrainingProvidedBy
	            ,tc.EnrollmentLastDate
	            ,tc.TrainingFeesType
	            ,tc.TrainingFeesPaidType
	            ,tc.TrainingFeesAmount
            FROM dbo.Training_Calender tc
            INNER JOIN dbo.Training_Category c ON c.TrainingCategoryId = tc.TraningCategoryId
            INNER JOIN dbo.Training_Type t ON t.TrainingTypeId = tc.TrainingTypeId
            LEFT JOIN dbo.Assesment_Type a ON a.AssesmentTypeId = tc.AssesmentTypeId
            -- Subcategory names
            CROSS APPLY (
	            SELECT STRING_AGG(s.TrainingSubCategory, ', ') AS TrainingSubcategory
	            FROM STRING_SPLIT(REPLACE(CAST(tc.TraningSubCategoryId AS NVARCHAR(MAX)), '.', ','), ',') AS ids
	            INNER JOIN dbo.Training_SubCategory s ON TRY_CAST(ids.value AS INT) = s.TrainingSubCategoryId
	            ) AS subcat
            -- Department names
            CROSS APPLY (
	            SELECT STRING_AGG(d.DepartmentName, ', ') AS DepartmentNames
	            FROM STRING_SPLIT(REPLACE(CAST(tc.DepartmentId AS NVARCHAR(MAX)), '.', ','), ',') AS ids
	            INNER JOIN dbo.Mas_Department d ON TRY_CAST(ids.value AS INT) = d.DepartmentId
	            ) AS dept
            WHERE 
            ',' + DepartmentId + ',' LIKE '%,{Session["EmployeeDepartmentId"]},%' And
            tc.Deactivate = '0'
	            AND EOMONTH(MonthYear) >= EOMONTH(GETDATE())  Order by MonthYear");
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Save Intrested Employee Entry
        [HttpPost]
        public ActionResult SaveEmployeeIntrestStatus(int? TrainingCalenderId)
        {
            try
            {
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_process", "Save");
                param1.Add("@p_EmployeeId", Session["EmployeeId"]);
                param1.Add("@p_MachineName", Dns.GetHostName().ToString());
                param1.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param1.Add("@p_TrainingCalenderId", TrainingCalenderId);
                param1.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);  // Output Parameter
                param1.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);  // Output Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Training_Calender_IntrestEnroll_Status", param1);

                TempData["Message"] = param1.Get<string>("@p_msg");
                TempData["Icon"] = param1.Get<string>("@p_Icon");

                //var totalInterested = DapperORM.DynamicQuerySingle<int>($@"SELECT COUNT(*) AS TotalInterested FROM Training_Calender_IntrestEnroll_Status
                //                WHERE IsIntrested = 1  and EmployeeId={Session["EmployeeId"]} and TrainingCalenderId={TrainingCalenderId}; ");


                var GetData = DapperORM.DynamicQuerySingle($@"SELECT COUNT(*) AS TotalInterested FROM Training_Calender_IntrestEnroll_Status
                                WHERE IsIntrested = 1  and EmployeeId={Session["EmployeeId"]} and TrainingCalenderId={TrainingCalenderId}; ");
                var totalInterested = GetData != null ? GetData.TotalInterested : null;

                return Json(new
                {
                    Message = TempData["Message"],
                    Icon = TempData["Icon"],
                    TotalCount = totalInterested
                }, JsonRequestBehavior.AllowGet);
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