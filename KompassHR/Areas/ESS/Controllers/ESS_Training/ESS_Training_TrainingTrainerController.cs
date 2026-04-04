using System;
using System.Collections.Generic;
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
using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Training;

namespace KompassHR.Areas.ESS.Controllers.ESS_Training
{
    public class ESS_Training_TrainingTrainerController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();

        [HttpGet]

        #region External Trainer Main View 
        public ActionResult ESS_Training_TrainingTrainer(string TrainerId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 553;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Training_TrainingTrainer TrainingTrainer = new Training_TrainingTrainer();
                ViewBag.AddUpdateTitle = "Add";
                TempData["FileName"] = "";
                if (TrainerId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@P_TrainerId_Encrypted", TrainerId_Encrypted);
                    TrainingTrainer = DapperORM.ReturnList<Training_TrainingTrainer>("sp_List_TrainingTrainer", param).FirstOrDefault();
                    Session["TrainerId"] = TrainingTrainer.TrainerId;
                    Session["SelectedFile"] = TrainingTrainer.PhotoPath;
                    TempData["FileName"] = TrainingTrainer.PhotoPath;
                    TempData["FromMonth"] = TrainingTrainer.FromMonth.ToString("yyyy-MM");
                    TempData["ToMonth"] = TrainingTrainer.ToMonth.ToString("yyyy-MM");
                }

                return View(TrainingTrainer);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsTrainerExists
        public ActionResult IsTrainerExists(string TrainerId,string TrainerId_Encrypted,string TrainerName,string MobileNo,
                        string WhatappsNo,string EmailId,string Specialization ,string TotalExperience)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@P_TrainerId_Encrypted", TrainerId_Encrypted);
                   // param.Add("@p_TrainerId",TrainerId);
                    param.Add("@p_TrainerName",TrainerName);
                    param.Add("@p_MobileNo", MobileNo);
                    param.Add("@p_WhatappsNo",WhatappsNo);
                    param.Add("@p_EmailId",EmailId);
                    param.Add("@p_Specialization",Specialization);
                    param.Add("@p_TotalExperience",TotalExperience);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_TrainingTrainer", param);
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
        public ActionResult SaveUpdate(Training_TrainingTrainer Trainer, HttpPostedFileBase PhotoPath)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var Logo = Request.Files["Logo"];
                var Stamp = Request.Files["Stamp"];
                param.Add("@p_process", string.IsNullOrEmpty(Trainer.TrainerId_Encrypted) ? "Save" : "Update");
                param.Add("@P_TrainerId_Encrypted", Trainer.TrainerId_Encrypted);
                param.Add("@p_TrainerId", Trainer.TrainerId);
                param.Add("@p_TrainerName", Trainer.TrainerName);
                param.Add("@p_MobileNo", Trainer.MobileNo);
                param.Add("@p_WhatappsNo", Trainer.WhatappsNo);
                param.Add("@p_EmailId", Trainer.EmailId);
                param.Add("@p_Specialization", Trainer.Specialization);
                param.Add("@p_TotalExperience", Trainer.TotalExperience);
                param.Add("@p_NoOfTrainingConducted", Trainer.NoOfTrainingConducted);
                param.Add("@p_FromMonth", Trainer.FromMonth);
                param.Add("@p_ToMonth", Trainer.ToMonth);
                param.Add("@p_ContractDuration", Trainer.ContractDuration);
                param.Add("@p_LinkedinProfile", Trainer.LinkedinProfile);
                if (Trainer.TrainerId_Encrypted != null && PhotoPath == null)
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

                var ExistingFileName = DapperORM.DynamicQueryList("Select * from Traning_Trainer where TrainerId=" + Trainer.TrainerId).FirstOrDefault();
                if(ExistingFileName.PhotoPath != null)
                {
                    if (ExistingFileName.PhotoPath != PhotoPath.FileName)
                    {
                        var GetDocPath1 = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Training'");
                        var GetFirstPath1 = GetDocPath1.DocInitialPath + "\\ExternalTrainer" + "\\" + Trainer.TrainerId + "\\";
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
                                formData.Add(new StringContent(GetFirstPath1 ), "FilePath");
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
                var Result = DapperORM.ExecuteReturn("sp_SUD_TrainingTrainer", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                string TrainerId = TempData["P_Id"]?.ToString() ?? string.Empty;
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
                                formData.Add(new StringContent(GetFirstPath + "\\ExternalTrainer"), "FilePath");
                                formData.Add(new StringContent(TrainerId), "LMSLibraryId");

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
                                formData.Add(new StringContent(GetFirstPath + "\\ExternalTrainer"), "FilePath");
                                formData.Add(new StringContent(Trainer.TrainerId.ToString()), "LMSLibraryId");

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
                return RedirectToAction("ESS_Training_TrainingTrainer", "ESS_Training_TrainingTrainer");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 553;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_TrainerId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_TrainingTrainer", param).ToList();
                ViewBag.GetTrainertList = data;
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
        public ActionResult Delete(double? TrainerId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_TrainerId", TrainerId);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_TrainingTrainer", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("GetList", "ESS_Training_TrainingTrainer");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region PreviewPhoto
        public ActionResult PreviewPhoto(double? TrainerId, string PhotoPath)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                // Fetch server path information
                var GetPath = DapperORM.DynamicQuerySingle("SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin='Training'");
                var FirstPath = GetPath.DocInitialPath + TrainerId + "\\"; // Not used here but if needed

                var DocRead = DapperORM.DynamicQuerySingle("SELECT * FROM Tool_LMS_DownloadUpload_URL WHERE DocOrigin='TrainingRead'");
                var GetDocRead = DocRead.DownloadUploadURL;
                var GetUrl = GetDocRead + "/ExternalTrainer" + "/" + TrainerId + "/"; // Base URL for the document

                // Construct full photo URL
                if (!string.IsNullOrEmpty(PhotoPath))
                {
                    string path = Path.Combine(GetUrl, PhotoPath).Replace("\\", "/"); 
                    string ext = Path.GetExtension(PhotoPath)?.TrimStart('.').ToUpper();
                    if (new[] { "JPG", "JPEG", "PNG" }.Contains(ext))
                    {
                        TempData["PhotoFile"] = new
                        {
                            FileSrc = path,
                            Title = Path.GetFileName(PhotoPath)
                        };
                    }
                }

                return RedirectToAction("GetList", "ESS_Training_TrainingTrainer");
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