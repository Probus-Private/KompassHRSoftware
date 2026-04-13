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
    public class ESS_Training_TraningSubCategoryController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region TraningSubCategory Main View 
        [HttpGet]
        public ActionResult ESS_Training_TraningSubCategory(string TrainingSubCategoryId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 577;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";

                DynamicParameters paramCategory = new DynamicParameters();
                var GetCategoryName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCategoryDropdown", paramCategory).ToList();
                var CID = GetCategoryName.FirstOrDefault()?.Id;
                ViewBag.CategoryName = GetCategoryName;

                Training_TraningSubCategory TrainingSubCategory = new Training_TraningSubCategory();
                if (TrainingSubCategoryId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_TrainingSubCategoryId_Encrypted", TrainingSubCategoryId_Encrypted);
                    TrainingSubCategory = DapperORM.ReturnList<Training_TraningSubCategory>("sp_List_TrainingSubCategory", param).FirstOrDefault();
                }
                return View(TrainingSubCategory);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList Main View 
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 577;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_TrainingSubCategoryId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_TrainingSubCategory", param).ToList();
                ViewBag.GetTrainingSubCategoryList = data;


                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsTrainingSubCategoryExists
        public ActionResult IsTrainingSubCategoryExists(double TrainingCategoryId, string TrainingSubCategory, string TrainingSubCategoryId_Encrypted)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_TrainingCategoryId", TrainingCategoryId);
                    param.Add("@p_TrainingSubCategory", TrainingSubCategory);
                    param.Add("@p_TrainingSubCategoryId_Encrypted", TrainingSubCategoryId_Encrypted);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_TrainingSubCategory", param);
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
        public ActionResult SaveUpdate(Training_TraningSubCategory SubCategory)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                param.Add("@p_process", string.IsNullOrEmpty(SubCategory.TrainingSubCategoryId_Encrypted) ? "Save" : "Update");
                param.Add("@p_TrainingSubCategoryId", SubCategory.TrainingSubCategoryId);
                param.Add("@p_TrainingSubCategoryId_Encrypted", SubCategory.TrainingSubCategoryId_Encrypted);
                param.Add("@p_TrainingCategoryId", SubCategory.TrainingCategoryId);
                param.Add("@p_TrainingSubCategory", SubCategory.TrainingSubCategory);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_TrainingSubCategory", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_Training_TraningSubCategory", "ESS_Training_TraningSubCategory");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(int? TrainingSubCategoryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_TrainingSubCategoryId", TrainingSubCategoryId);
                param.Add("@p_TrainingSubCategoryId_Encrypted", string.Empty);
                param.Add("@p_TrainingSubCategory", string.Empty);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_TrainingSubCategory", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_Training_TraningSubCategory");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region File SaveUpdate on Server
        [HttpPost]
        public ActionResult SaveUpdateUploadFile(TrainingSubcategory_Document Library, HttpPostedFileBase DocumentPath)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                string extension = Path.GetExtension(DocumentPath.FileName).ToLower();
                param.Add("@p_process", string.IsNullOrEmpty(Library.TrainingSubCategoryDocId_Encrypted) ? "Save" : "Update");
                param.Add("@p_TrainingSubCategoryDocId_Encrypted", Library.TrainingSubCategoryDocId_Encrypted);
                param.Add("@p_Training_TrainingSubCategory_TrainingSubCategoryId", Session["TrainingSubCategoryId"]);
                param.Add("@p_Title", Library.Title);
                if (extension == ".jpg" || extension == ".jpeg" || extension == ".jpeg" || extension == ".jpeg")
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

                if (Library.TrainingSubCategoryDocId_Encrypted != null && DocumentPath == null)
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
                var Result = DapperORM.ExecuteReturn("sp_SUD_TrainingSubCategory_Document", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                var DocPKId = param.Get<string>("@p_Id");
                if (Session["TrainingSubCategoryId"] != null)
                {
                    if (Library.DocumentPath != null)
                    {
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Training'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        var FirstPath = GetFirstPath + Convert.ToInt32(Session["TrainingSubCategoryId"]) + "\\"; // First path plus concat folder 
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
                                formData.Add(new StringContent(GetFirstPath + "\\TrainingSubCategory"), "FilePath");
                                formData.Add(new StringContent(Session["TrainingSubCategoryId"].ToString()), "LMSLibraryId");

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
                        var FirstPath = GetFirstPath + Library.TrainingSubCategoryId + "\\"; // First path plus concat folder 
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
                return RedirectToAction("TrainingSubCategoryUploadFile", "ESS_Training_TraningSubCategory", new { TrainingSubCategoryId_Encrypted = Session["TrainingSubCategoryId_Encrypted"], TrainingSubCategoryId = Session["TrainingSubCategoryId"] });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
     
        #region DeleteDocument
        public ActionResult DeleteDocument(string TrainingSubCategoryDocId_Encrypted, string FileName, int Training_TrainingCategory_TrainingSubCategoryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                param.Add("@p_process", "Delete");
                param.Add("@p_TrainingSubCategoryDocId_Encrypted", TrainingSubCategoryDocId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_TrainingSubCategory_Document", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                var DocPKId = param.Get<string>("@p_Id");

                var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Training'");
                var GetFirstPath = GetDocPath.DocInitialPath + "\\" + Training_TrainingCategory_TrainingSubCategoryId + "\\";

                var DocRead = DapperORM.DynamicQuerySingle("Select * from Tool_LMS_DownloadUpload_URL where DocOrigin='TrainingSubCategoryDocDelete'");
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
                return RedirectToAction("TrainingSubCategoryUploadFile", "ESS_Training_TraningSubCategory", new { TrainingSubCategoryId_Encrypted = Session["TrainingSubCategoryId_Encrypted"], TrainingSubCategoryId = Session["TrainingSubCategoryId"] });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region TrainingSubCategoryUploadFile
        public ActionResult TrainingSubCategoryUploadFile(string TrainingSubCategoryId_Encrypted, int? TrainingSubCategoryId)
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
                Session["TrainingSubCategoryId"] = TrainingSubCategoryId;
                Session["TrainingSubCategoryId_Encrypted"] = TrainingSubCategoryId_Encrypted;
                param.Add("@p_TrainingSubCategoryDocId_Encrypted", "List");
                param.Add("@p_TrainingSubCategoryId", Session["TrainingSubCategoryId"]);
                var data = DapperORM.ReturnList<dynamic>("sp_List_TrainingSubCategory_Document", param).ToList();
                ViewBag.GetListTrainingSubCategory = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region TrainingSubCategoryDocumentView
        public ActionResult TrainingSubCategoryDocumentView(double? TrainingSubCategoryId)
        {
            try
            {
                if (Session["EmployeeId"] != null)
                {
                    var GetPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Training'");
                    var FirstPath = GetPath.DocInitialPath + TrainingSubCategoryId + "\\";

                    var DocRead = DapperORM.DynamicQuerySingle("Select * from Tool_LMS_DownloadUpload_URL where DocOrigin='TrainingRead'");
                    var GetDocRead = DocRead.DownloadUploadURL;
                    var GetUrl = GetDocRead + "/TrainingSubCategory/" + TrainingSubCategoryId + "/";

                    var employeeId = Convert.ToInt32(Session["EmployeeId"]);
                    string sql = $@"Select * from Training_TrainingSubCategory_Document where Deactivate=0 and Training_TrainingCategory_TrainingSubCategoryId = {TrainingSubCategoryId};";
                    List<dynamic> GetSecondPath;
                    bool GetIsAssessmentActive = false;
                    using (var result = DapperORM.DynamicMultipleResult(sql))
                    {
                        GetSecondPath = result.Read<dynamic>().ToList();
                    }

                    // TempData["IsAssessmentActive"] = GetIsAssessmentActive;

                    var sourcePaths = GetSecondPath
                        .Where(x => x.DocumentPath != null)
                        .Select(x => new
                        {
                            TrainingSubCategoryDocId = x.TrainingSubCategoryDocId,
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
                            TrainingSubCategoryDocId = x.TrainingSubCategoryDocId,
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
    }
}