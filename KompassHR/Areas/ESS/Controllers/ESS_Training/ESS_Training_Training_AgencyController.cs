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
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Training
{
    public class ESS_Training_Training_AgencyController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        [HttpGet]

        #region Trainer Main View 
        public ActionResult ESS_Training_Training_Agency(string TrainingAgencyId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 556;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Training_TrainingAgency TrainingAgency = new Training_TrainingAgency();
                ViewBag.AddUpdateTitle = "Add";
                if (TrainingAgencyId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@P_TrainingAgencyId_Encrypted", TrainingAgencyId_Encrypted);
                    TrainingAgency = DapperORM.ReturnList<Training_TrainingAgency>("sp_List_TrainingAgency", param).FirstOrDefault();
                    TempData["FromMonth"] = TrainingAgency.FromMonth.ToString("yyyy-MM");
                    TempData["ToMonth"] = TrainingAgency.ToMonth.ToString("yyyy-MM");
                    Session["TrainingAgencyId"] = TrainingAgency.TrainingAgencyId;
                    Session["SelectedFile"] = TrainingAgency.PhotoPath;
                    TempData["FileName"] = TrainingAgency.PhotoPath;
                }

                return View(TrainingAgency);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsTrainingAgencyExists
        public ActionResult IsTrainingAgencyExists(string TrainingAgencyId,string TrainingAgencyId_Encrypted, string TrainingAgencyName, string MobileNo,
                        string WhatappsNo, string EmailId, string Specialization, string ContactPersonName)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@P_TrainingAgencyId_Encrypted", TrainingAgencyId_Encrypted);
                    param.Add("@p_TrainingAgencyName", TrainingAgencyName);
                    param.Add("@p_MobileNo", MobileNo);
                    param.Add("@p_WhatappsNo", WhatappsNo);
                    param.Add("@p_EmailId", EmailId);
                    param.Add("@p_Specialization", Specialization);
                    param.Add("@p_ContactPersonName", ContactPersonName);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_TrainingAgency", param);
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
        public ActionResult SaveUpdate(Training_TrainingAgency Trainer, HttpPostedFileBase PhotoPath)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var Logo = Request.Files["Logo"];
                var Stamp = Request.Files["Stamp"];
                param.Add("@p_process", string.IsNullOrEmpty(Trainer.TrainingAgencyId_Encrypted) ? "Save" : "Update");
                param.Add("@P_TrainingAgencyId_Encrypted", Trainer.TrainingAgencyId_Encrypted);
                param.Add("@p_TrainingAgencyId", Trainer.TrainingAgencyId);
                param.Add("@p_TrainingAgencyName", Trainer.TrainingAgencyName);
                param.Add("@p_MobileNo", Trainer.MobileNo);
                param.Add("@p_WhatappsNo", Trainer.WhatappsNo);
                param.Add("@p_EmailId", Trainer.EmailId);
                param.Add("@p_Specialization", Trainer.Specialization);
                param.Add("@p_ContactPersonName", Trainer.ContactPersonName);
                param.Add("@p_LinkedinProfile", Trainer.LinkedinProfile);
                param.Add("@p_FromMonth", Trainer.FromMonth);
                param.Add("@p_ToMonth", Trainer.ToMonth);
                param.Add("@p_NoOfTrainingConducted", Trainer.NoOfTrainingConducted);
                param.Add("@p_ContractDuration", Trainer.ContractDuration);
                if (Trainer.TrainingAgencyId_Encrypted != null && PhotoPath == null)
                {
                    param.Add("@p_PhotoPath", Session["SelectedFile"]);
                }
                else
                {
                    param.Add("@p_PhotoPath", PhotoPath == null ? "" : PhotoPath.FileName);
                };
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var ExistingFileName = DapperORM.DynamicQueryList("Select * from Training_Agency where TrainingAgencyId=" + Trainer.TrainingAgencyId).FirstOrDefault();
                if (ExistingFileName.PhotoPath != null)
                {
                    if (PhotoPath!=null)
                    {
                        if (ExistingFileName.PhotoPath != PhotoPath.FileName)
                        {
                            var GetDocPath1 = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Training'");
                            var GetFirstPath1 = GetDocPath1.DocInitialPath + "\\TrainingAgency" + "\\" + Trainer.TrainingAgencyId + "\\";
                            var DocRead1 = DapperORM.DynamicQuerySingle("Select * from Tool_LMS_DownloadUpload_URL where DocOrigin='TrainingDelete'");

                            var GetDocReadURL1 = DocRead1.DownloadUploadURL;
                            string VerificationUrl1 = GetDocReadURL1;
                            //string VerificationUrl = "http://10.48.218.3:8096/api/LMSDocDelete";
                            using (HttpClient client = new HttpClient())
                            {
                                using (var formData = new MultipartFormDataContent())
                                {
                                    // Add string parameters
                                    var SetFileName = ExistingFileName.PhotoPath;
                                    formData.Add(new StringContent(GetFirstPath1), "FilePath");
                                    formData.Add(new StringContent(SetFileName), "File");
                                    HttpResponseMessage response = client.PostAsync(VerificationUrl1, formData).GetAwaiter().GetResult();
                                    if (response.IsSuccessStatusCode)
                                    {
                                        string jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                        var FinalData = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                                    }
                                }
                            }
                        }
                    }
                   
                }
                var Result = DapperORM.ExecuteReturn("sp_SUD_TrainingAgency", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                string TrainingAgencyId = TempData["P_Id"]?.ToString() ?? string.Empty;
                if (TempData["P_Id"] != null)
                {

                    if (Trainer.PhotoPath != null)
                    {
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Training'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        var DocRead = DapperORM.DynamicQuerySingle("Select * from Tool_LMS_DownloadUpload_URL where DocOrigin='TrainingUpload'");
                        var GetDocReadURL = DocRead.DownloadUploadURL;
                        string VerificationUrl = GetDocReadURL;
                        //string VerificationUrl = "http://10.48.218.3:8096/api/LMSDocUpload";
                        using (HttpClient client = new HttpClient())
                        {
                            using (var formData = new MultipartFormDataContent())
                            {
                                // Add string parameters
                                formData.Add(new StringContent(GetFirstPath + "\\TrainingAgency"), "FilePath");
                                formData.Add(new StringContent(TrainingAgencyId), "LMSLibraryId");

                                if (PhotoPath != null && PhotoPath.ContentLength > 0)
                                {
                                    var streamContent = new StreamContent(PhotoPath.InputStream);
                                    streamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                                    {
                                        Name = "\"File\"",
                                        FileName = $"\"{PhotoPath.FileName}\""
                                    };
                                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(PhotoPath.ContentType); // e.g., application/pdf

                                    formData.Add(streamContent, "File", PhotoPath.FileName);
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
                    if (PhotoPath != null)
                    {
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Training'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        var DocRead = DapperORM.DynamicQuerySingle("Select * from Tool_LMS_DownloadUpload_URL where DocOrigin='TrainingUpload'");
                        var GetDocReadURL = DocRead.DownloadUploadURL;
                        string VerificationUrl = GetDocReadURL;
                        //string VerificationUrl = "http://10.48.218.3:8096/api/LMSDocUpload";
                        using (HttpClient client = new HttpClient())
                        {
                            using (var formData = new MultipartFormDataContent())
                            {
                                // Add string parameters
                                formData.Add(new StringContent(GetFirstPath + "\\TrainingAgency"), "FilePath");
                                formData.Add(new StringContent(Trainer.TrainingAgencyId.ToString()), "LMSLibraryId");

                                if (PhotoPath != null && PhotoPath.ContentLength > 0)
                                {
                                    var streamContent = new StreamContent(PhotoPath.InputStream);
                                    streamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                                    {
                                        Name = "\"File\"",
                                        FileName = $"\"{PhotoPath.FileName}\""
                                    };
                                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(PhotoPath.ContentType); // e.g., application/pdf

                                    formData.Add(streamContent, "File", PhotoPath.FileName);
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
                return RedirectToAction("ESS_Training_Training_Agency", "ESS_Training_Training_Agency");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 556;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_TrainingAgencyId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_TrainingAgency", param).ToList();
                ViewBag.GetTrainingAgencytList = data;
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
        public ActionResult Delete(double? TrainingAgencyId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_TrainingAgencyId", TrainingAgencyId);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_TrainingAgency", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("GetList", "ESS_Training_Training_Agency");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region PreviewPhoto
        public ActionResult PreviewPhoto(double? TrainingAgencyId, string PhotoPath)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                // Fetch server path information
                var GetPath = DapperORM.DynamicQuerySingle("SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin='Training'");
                var FirstPath = GetPath.DocInitialPath + TrainingAgencyId + "\\"; // Not used here but if needed

                var DocRead = DapperORM.DynamicQuerySingle("SELECT * FROM Tool_LMS_DownloadUpload_URL WHERE DocOrigin='TrainingRead'");
                var GetDocRead = DocRead.DownloadUploadURL;
                var GetUrl = GetDocRead + "/TrainingAgency" + "/" + TrainingAgencyId + "/"; // Base URL for the document

                // Construct full photo URL
                if (!string.IsNullOrEmpty(PhotoPath))
                {
                    // Combine the base URL with the photo name to create the full path
                    string path = Path.Combine(GetUrl, PhotoPath).Replace("\\", "/"); // Ensure it's a valid URL
                                                                                      // TempData["PhotoFile"] = path;
                                                                                      //// Only process if it's a valid image file type
                    string ext = Path.GetExtension(PhotoPath)?.TrimStart('.').ToUpper();
                    if (new[] { "JPG", "JPEG", "PNG" }.Contains(ext))
                    {
                        TempData["PhotoFile"] = new
                        {
                            FileSrc = path,  // Full URL to the image
                            Title = Path.GetFileName(PhotoPath)  // Extract just the file name for the title
                        };
                    }
                }

                return RedirectToAction("GetList", "ESS_Training_Training_Agency");
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