using Dapper;
using KompassHR.Areas.Setting.Models.Setting_LMS;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_LMS
{
    public class Setting_LMS_LibraryController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: Setting/Setting_LMS_Library
        #region Setting_LMS_Library
        public ActionResult Setting_LMS_Library(string LMSLibraryId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 564;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetCompanyName = GetComapnyName;


                DynamicParameters paramcmp = new DynamicParameters();
                paramcmp.Add("@query", "Select LMSCategoryId as Id,LMSCategoryName as Name  from LMS_Category where Deactivate=0 Order by Name");
                ViewBag.LMSCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramcmp).ToList();
                LMS_Library ObjLibrary = new LMS_Library();
                ViewBag.AddUpdateTitle = "Add";
                if (LMSLibraryId_Encrypted != null)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    paramList.Add("@p_LMSLibraryId_Encrypted", LMSLibraryId_Encrypted);
                    ObjLibrary = DapperORM.ReturnList<LMS_Library>("sp_List_LMS_Library", paramList).FirstOrDefault();
                    Session["SelectedFile"] = ObjLibrary.DocumentPath;
                }
                return View(ObjLibrary);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsLMSCategoryExists
        public ActionResult IsLMSLibraryExists(int CmpID, string LMSLibraryId_Encrypted, int? LMSLibrary_CategoryId, string Title)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "IsValidation");
                param.Add("@p_CmpID", CmpID);
                param.Add("@p_LMSLibrary_CategoryId", LMSLibrary_CategoryId);
                param.Add("@p_LMSLibraryId_Encrypted", LMSLibraryId_Encrypted);
                param.Add("@p_Title", Title);

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_LMS_Library", param);
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
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(LMS_Library Library, HttpPostedFileBase DocumentPath)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                param.Add("@p_process", string.IsNullOrEmpty(Library.LMSLibraryId_Encrypted) ? "Save" : "Update");
                param.Add("@p_CmpID", Library.CmpID);
                param.Add("@p_LMSLibraryId_Encrypted", Library.LMSLibraryId_Encrypted);
                param.Add("@p_LMSLibrary_CategoryId", Library.LMSLibrary_CategoryId);
                param.Add("@p_Title", Library.Title);
                param.Add("@p_Description", Library.Description);
                param.Add("@p_IsAssessmentRequired", Library.IsAssessmentRequired);
                param.Add("@p_IsDigitalSignature", Library.IsDigitalSignature);
                if (Library.LMSLibraryId_Encrypted != null && DocumentPath == null)
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
                var Result = DapperORM.ExecuteReturn("sp_SUD_LMS_Library", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                TempData["P_Id"] = param.Get<string>("@p_Id");
                if (TempData["P_Id"] != null)
                {

                    if (Library.DocumentPath != null)
                    {
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='LMS'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        var FirstPath = GetFirstPath + TempData["P_Id"] + "\\"; // First path plus concat folder 
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
                else
                {
                    if (DocumentPath != null)
                    {
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='LMS'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        var FirstPath = GetFirstPath + Library.LMSLibraryId + "\\"; // First path plus concat folder 
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
                return RedirectToAction("Setting_LMS_Library", "Setting_LMS_Library");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 564;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_LMSLibraryId_Encrypted", "List");
                var data = DapperORM.ReturnList<dynamic>("sp_List_LMS_Library", param).ToList();
                ViewBag.GetListLMSLibrary = data;
                Session["LMSLibraryId"] = null;
                Session["LMSLibraryId_Encrypted"] = null;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region  Delete
        public ActionResult Delete(string LMSLibraryId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                param.Add("@p_process", "Delete");
                param.Add("@p_LMSLibraryId_Encrypted", LMSLibraryId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_LMS_Library", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_LMS_Library");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region LMSUploadFile
        public ActionResult LMSUploadFile(string LMSLibraryId_Encrypted, int? LMSLibraryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 564;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Session["LMSLibraryId"] = LMSLibraryId;
                Session["LMSLibraryId_Encrypted"] = LMSLibraryId_Encrypted;
                param.Add("@p_LMSLibraryDoc_Encrypted", "List");
                param.Add("@p_LMSLibraryId", Session["LMSLibraryId"]);
                var data = DapperORM.ReturnList<dynamic>("sp_List_LMSLibrary_Document", param).ToList();
                ViewBag.GetListLMSLibrary = data;
                return View();
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
        public ActionResult SaveUpdateUploadFile(LMSLibrary_Document Library, HttpPostedFileBase DocumentPath)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                string extension = Path.GetExtension(DocumentPath.FileName).ToLower();
                param.Add("@p_process", string.IsNullOrEmpty(Library.LMSLibraryDoc_Encrypted) ? "Save" : "Update");
                param.Add("@p_LMSLibraryDoc_Encrypted", Library.LMSLibraryDoc_Encrypted);
                param.Add("@p_LMS_Library_LMSLibraryId", Session["LMSLibraryId"]);
                param.Add("@p_Title", Library.Title);
                if (extension == ".jpg" || extension == ".jpeg" || extension == ".JPG" || extension == ".JPEG" || extension == ".png" || extension == ".PNG")
                {
                    param.Add("@p_FilteType", "Image");
                }
                else if (extension == ".pdf")
                {
                    param.Add("@p_FilteType", "PDF");
                }
                else if (extension == ".mp4" || extension == ".avi" || extension == ".mov" ||
                         extension == ".wmv" || extension == ".flv" || extension == ".mkv" ||
                         extension == ".webm" || extension == ".mpeg" || extension == ".mpg" ||
                         extension == ".3gp")
                {
                    param.Add("@p_FilteType", "Vedio");
                }

                if (Library.LMSLibraryDoc_Encrypted != null && DocumentPath == null)
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
                var Result = DapperORM.ExecuteReturn("sp_SUD_LMSLibrary_Document", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                var DocPKId = param.Get<string>("@p_Id");
                if (Session["LMSLibraryId"] != null)
                {

                    if (Library.DocumentPath != null)
                    {
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='LMS'");
                        var GetFirstPath = GetDocPath.DocInitialPath;

                        //var FirstPath = GetFirstPath + "\\" + Convert.ToInt32(Session["LMSLibraryId"]) + "\\"; // First path plus concat folder 
                        //string directoryPath = Path.GetDirectoryName(FirstPath);
                        //if (!Directory.Exists(directoryPath))
                        //{
                        //    Directory.CreateDirectory(FirstPath);
                        //}
                        //string AttachFilePath = "";
                        //AttachFilePath = FirstPath + Convert.ToInt32(Session["LMSLibraryId" + "_"+DocumentPath.FileName; //Concat Full Path and create New full Path
                        //DocumentPath.SaveAs(AttachFilePath); // This is use for Save image in folder full path


                        var DocRead = DapperORM.DynamicQuerySingle("Select * from Tool_LMS_DownloadUpload_URL where DocOrigin='LMSDocUpload'");
                        var GetDocReadURL = DocRead.DownloadUploadURL;
                        string VerificationUrl = GetDocReadURL;
                        //string VerificationUrl = "http://10.48.218.3:8096/api/LMSDocUpload";
                        using (HttpClient client = new HttpClient())
                        {
                            using (var formData = new MultipartFormDataContent())
                            {
                                // Add string parameters
                                formData.Add(new StringContent(GetFirstPath), "FilePath");
                                formData.Add(new StringContent(Session["LMSLibraryId"].ToString()), "LMSLibraryId");
                                
                                if (DocumentPath != null && DocumentPath.ContentLength > 0)
                                {
                                    var streamContent = new StreamContent(DocumentPath.InputStream);
                                    streamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                                    {
                                        Name = "\"File\"",
                                        FileName = $"\"{DocPKId+"_"+DocumentPath.FileName}\""
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
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='LMS'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        var FirstPath = GetFirstPath + Library.LMSLibraryId + "\\"; // First path plus concat folder 
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
                return RedirectToAction("LMSUploadFile", "Setting_LMS_Library", new { LMSLibraryId_Encrypted = Session["LMSLibraryId_Encrypted"], LMSLibraryId = Session["LMSLibraryId"] });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region DeleteDocument
        public ActionResult DeleteDocument(string LMSLibraryDoc_Encrypted, string FileName, int LMS_Library_LMSLibraryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                param.Add("@p_process", "Delete");
                param.Add("@p_LMSLibraryDoc_Encrypted", LMSLibraryDoc_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_LMSLibrary_Document", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                var DocPKId = param.Get<string>("@p_Id");

                var GetDocPath = DapperORM.DynamicQueryList("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='LMS'").FirstOrDefault();
                var GetFirstPath = GetDocPath.DocInitialPath +"\\" + LMS_Library_LMSLibraryId +"\\";

                var DocRead = DapperORM.DynamicQueryList("Select * from Tool_LMS_DownloadUpload_URL where DocOrigin='LMSDocDelete'").FirstOrDefault();
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
                return RedirectToAction("LMSUploadFile", "Setting_LMS_Library", new { LMSLibraryId_Encrypted = Session["LMSLibraryId_Encrypted"], LMSLibraryId = Session["LMSLibraryId"] });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


        #region Download Attachment
        public ActionResult DownloadAttachment(string DownloadAttachment)
        {
            try
            {
                if (DownloadAttachment != "File Not Found")
                {
                    if (string.IsNullOrEmpty(DownloadAttachment))
                    {
                        return Json(new { Success = false, Message = "Invalid File.", Icon = "error" }, JsonRequestBehavior.AllowGet);
                    }

                    var fullPath = DownloadAttachment;
                    if (!System.IO.File.Exists(fullPath))
                    {
                        return Json(new { Success = false, Message = "File not found on your server.", Icon = "error" }, JsonRequestBehavior.AllowGet);
                    }

                    var driveLetter = Path.GetPathRoot(DownloadAttachment);
                    if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
                    {
                        return Json(new { Success = false, Message = $"Drive {driveLetter} does not exist.", Icon = "error" }, JsonRequestBehavior.AllowGet);
                    }


                    var fileName = Path.GetFileName(fullPath);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
                    var fileBase64 = Convert.ToBase64String(fileBytes);

                    return Json(new
                    {
                        Success = true,
                        FileName = fileName,
                        FileData = fileBase64,
                        ContentType = MediaTypeNames.Application.Octet
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    TempData["Message"] = "Upload a file";
                    TempData["Icon"] = "error";
                    //return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    return RedirectToAction("LMSUploadFile", "Setting_LMS_Library", new { LMSLibraryId_Encrypted = Session["LMSLibraryId_Encrypted"], LMSLibraryId = Session["LMSLibraryId"] });
                }

            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = $"Internal server error: {ex.Message}", Icon = "error" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #endregion

        #region LMSAssessment


        public ActionResult LMSAssessment(string LMSLibraryId_Encrypted, int? LMSLibraryId, string LMSFeedbackID_Encrypted, string CategoryName, string SubCategoryName)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (CategoryName!=null && SubCategoryName!=null)
                {
                    Session["AssessmentCategoryName"] = CategoryName;
                    Session["AssessmentSubCategoryName"] = SubCategoryName;
                    Session["AssessmentLMSLibraryId"] = LMSLibraryId;
                }
                
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 564;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_LMSLibraryDoc_Encrypted", "List");
                var data = DapperORM.ReturnList<dynamic>("sp_List_LMSLibrary_Document", param).ToList();
                ViewBag.GetListLMSLibrary = data;
                LMS_LibraryFeedbackMaster ObjQuestion = new LMS_LibraryFeedbackMaster();
                ViewBag.AddUpdateTitle = "Add";
                if (LMSFeedbackID_Encrypted != null)
                {

                    Session["LMSLibraryId"] = Session["LMSLibraryId"];
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_LMSFeedbackID_Encrypted", LMSFeedbackID_Encrypted);
                    ObjQuestion = DapperORM.ReturnList<LMS_LibraryFeedbackMaster>("sp_List_LMS_LibraryFeedbackMaster", param).FirstOrDefault();

                    DynamicParameters paramOption = new DynamicParameters();
                    paramOption.Add("@query", "select ROW_NUMBER() OVER (ORDER BY (SELECT 1)) AS SrNo, Options as Answer, IsAnswer,InputAnswer from LMS_LibraryFeedbackDetails where LMS_FeedbackMaster_LMSFeedbackID=" + ObjQuestion.LMSFeedbackID + " and Deactivate=0");
                    var List_Options = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramOption).ToList();
                    ViewBag.GetOptions = List_Options;
                    if (List_Options.Any())
                    {
                        ObjQuestion.InputAnswer = List_Options[0].InputAnswer;
                    }
                }
                else
                {
                    Session["LMSLibraryId"] = LMSLibraryId;
                    Session["LMSLibraryId_Encrypted"] = LMSLibraryId_Encrypted;
                }
                return View(ObjQuestion);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }


        #region IsValidation
        [HttpGet]
        public ActionResult IsQuestionExists(string Question, string LMSFeedbackID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_LMSFeedbackID_Encrypted", LMSFeedbackID_Encrypted);
                param.Add("@p_Question", Question);
                param.Add("@p_LMSLibraryId", Session["AssessmentLMSLibraryId"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_LMS_LibraryFeedbackMaster", param);

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
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdateAssessment(List<LMS_LibraryFeedbackDetails_option> Task, string Question,string InputAnswer, string LMSFeedbackID_Encrypted, int Type, int LMSLibraryId, bool IsActive)
        {
            try
            {
                StringBuilder strBuilder = new StringBuilder();
                param.Add("@p_process", string.IsNullOrEmpty(LMSFeedbackID_Encrypted) ? "Save" : "Update");
                param.Add("@p_LMSLibraryId", Session["LMSLibraryId"]);
                param.Add("@p_QuestionType", Type);
                param.Add("@p_Question", Question);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_LMSFeedbackID_Encrypted", LMSFeedbackID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_IsActive", IsActive);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_LMS_LibraryFeedbackMaster", param);
                var PID = param.Get<string>("@p_Id");

                if (LMSFeedbackID_Encrypted != "")
                {
                    DapperORM.DynamicQuerySingle("update LMS_LibraryFeedbackDetails set Deactivate=1,ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=GETDATE(),MachineName='" + Dns.GetHostName().ToString() + "' where LMS_FeedbackMaster_LMSFeedbackID=" + PID + "");
                    if (Type != 3)
                    {
                        if (Task != null)
                        {
                            foreach (var Data in Task)
                            {
                                string Answer = "Insert Into LMS_LibraryFeedbackDetails(" +
                                                      "   Deactivate " +
                                                      " , CreatedBy " +
                                                      " , CreatedDate " +
                                                      " , MachineName " +
                                                      " , LMS_FeedbackMaster_LMSFeedbackID " +
                                                      " , Options " +
                                                       " , IsAnswer " +
                                                       ") values (" +
                                                      "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                                      "'" + PID + "'," +
                                                      "'" + Data.Options + "'," +
                                                      "'" + Data.IsAnswer + "'" +
                                                      ")" +
                                                       " " +
                                                      " " +
                                                       " ";
                                strBuilder.Append(Answer);

                            }
                            string abc = "";
                            if (objcon.SaveStringBuilder(strBuilder, out abc))
                            {

                                TempData["Message"] = "Record update successfully";
                                TempData["Icon"] = "success";
                            }

                            if (abc != "")
                            {
                                DapperORM.DynamicQuerySingle("Insert Into Tool_ErrorLog ( " +
                                                                                    "   Error_Desc " +
                                                                                    " , Error_FormName " +
                                                                                    " , Error_MachinceName " +
                                                                                    " , Error_Date " +
                                                                                    " , Error_UserID " +
                                                                                    " , Error_UserName " + ") values (" +
                                                                                    "'" + strBuilder + "'," +
                                                                                    "'BuklInsert'," +
                                                                                    "'" + Dns.GetHostName().ToString() + "'," +
                                                                                    "GetDate()," +
                                                                                    "'" + Session["EmployeeId"] + "'," +
                                                                                    "'" + Session["EmployeeName"] + "'");
                                TempData["Message"] = abc;
                                TempData["Icon"] = "error";
                            }
                        }

                    }
                    else
                    {
                        string Answer = "Insert Into LMS_LibraryFeedbackDetails(" +
                                              "   Deactivate " +
                                              " , CreatedBy " +
                                              " , CreatedDate " +
                                              " , MachineName " +
                                              " , LMS_FeedbackMaster_LMSFeedbackID " +
                                              " , Options " +
                                              " , IsAnswer " +
                                              " , InputAnswer " +
                                               ") values (" +
                                              "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                              "'" + PID + "'," +
                                              "'Input'," +
                                              "''," +
                                              "'" + InputAnswer + "'" +
                                              ")" +
                                               " " +
                                              " " +
                                               " ";
                        strBuilder.Append(Answer);

                        string abc = "";
                        if (objcon.SaveStringBuilder(strBuilder, out abc))
                        {

                            TempData["Message"] = "Record update successfully";
                            TempData["Icon"] = "success";
                        }

                        if (abc != "")
                        {
                            DapperORM.DynamicQuerySingle("Insert Into Tool_ErrorLog ( " +
                                                                                "   Error_Desc " +
                                                                                " , Error_FormName " +
                                                                                " , Error_MachinceName " +
                                                                                " , Error_Date " +
                                                                                " , Error_UserID " +
                                                                                " , Error_UserName " + ") values (" +
                                                                                "'" + strBuilder + "'," +
                                                                                "'BuklInsert'," +
                                                                                "'" + Dns.GetHostName().ToString() + "'," +
                                                                                "GetDate()," +
                                                                                "'" + Session["EmployeeId"] + "'," +
                                                                                "'" + Session["EmployeeName"] + "'");
                            TempData["Message"] = abc;
                            TempData["Icon"] = "error";
                        }
                    }
                }
                else
                {
                    if (Type != 3)
                    {
                        if (Task != null)
                        {
                            foreach (var Data in Task)
                            {
                                string Answer = "Insert Into LMS_LibraryFeedbackDetails(" +
                                                      "   Deactivate " +
                                                      " , CreatedBy " +
                                                      " , CreatedDate " +
                                                      " , MachineName " +
                                                      " , LMS_FeedbackMaster_LMSFeedbackID " +
                                                      " , Options " +
                                                      " , IsAnswer " +
                                                       ") values (" +
                                                      "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                                      "'" + PID + "'," +
                                                      "'" + Data.Options + "'," +
                                                      "'" + Data.IsAnswer + "'" +
                                                      ")" +
                                                       " " +
                                                      " " +
                                                       " ";
                                strBuilder.Append(Answer);

                            }
                            string abc = "";
                            if (objcon.SaveStringBuilder(strBuilder, out abc))
                            {

                                TempData["Message"] = "Record update successfully";
                                TempData["Icon"] = "success";
                            }

                            if (abc != "")
                            {
                                DapperORM.DynamicQuerySingle("Insert Into Tool_ErrorLog ( " +
                                                                                    "   Error_Desc " +
                                                                                    " , Error_FormName " +
                                                                                    " , Error_MachinceName " +
                                                                                    " , Error_Date " +
                                                                                    " , Error_UserID " +
                                                                                    " , Error_UserName " + ") values (" +
                                                                                    "'" + strBuilder + "'," +
                                                                                    "'BuklInsert'," +
                                                                                    "'" + Dns.GetHostName().ToString() + "'," +
                                                                                    "GetDate()," +
                                                                                    "'" + Session["EmployeeId"] + "'," +
                                                                                    "'" + Session["EmployeeName"] + "'");
                                TempData["Message"] = abc;
                                TempData["Icon"] = "error";
                            }
                        }
                        else
                        {
                            string Answer = "Insert Into LMS_LibraryFeedbackDetails(" +
                                                  "   Deactivate " +
                                                  " , CreatedBy " +
                                                  " , CreatedDate " +
                                                  " , MachineName " +
                                                  " , LMS_FeedbackMaster_LMSFeedbackID " +
                                                  " , Options " +
                                                  " , IsAnswer " +
                                                   ") values (" +
                                                  "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                                  "'" + PID + "'," +
                                                  "'Input'," +
                                                  "''" +
                                                  ")" +
                                                   " " +
                                                  " " +
                                                   " ";
                            strBuilder.Append(Answer);

                            string abc = "";
                            if (objcon.SaveStringBuilder(strBuilder, out abc))
                            {

                                TempData["Message"] = "Record update successfully";
                                TempData["Icon"] = "success";
                            }

                            if (abc != "")
                            {
                                DapperORM.DynamicQuerySingle("Insert Into Tool_ErrorLog ( " +
                                                                                    "   Error_Desc " +
                                                                                    " , Error_FormName " +
                                                                                    " , Error_MachinceName " +
                                                                                    " , Error_Date " +
                                                                                    " , Error_UserID " +
                                                                                    " , Error_UserName " + ") values (" +
                                                                                    "'" + strBuilder + "'," +
                                                                                    "'BuklInsert'," +
                                                                                    "'" + Dns.GetHostName().ToString() + "'," +
                                                                                    "GetDate()," +
                                                                                    "'" + Session["EmployeeId"] + "'," +
                                                                                    "'" + Session["EmployeeName"] + "'");
                                TempData["Message"] = abc;
                                TempData["Icon"] = "error";
                            }
                        }
                    }
                    else
                    {
                        string Answer = "Insert Into LMS_LibraryFeedbackDetails(" +
                                              "   Deactivate " +
                                              " , CreatedBy " +
                                              " , CreatedDate " +
                                              " , MachineName " +
                                              " , LMS_FeedbackMaster_LMSFeedbackID " +
                                              " , Options " +
                                              " , IsAnswer " +
                                               " , InputAnswer " +
                                               ") values (" +
                                              "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                              "'" + PID + "'," +
                                              "'Input'," +
                                              "''," +
                                              "'" + InputAnswer + "'" +
                                              ")" +
                                               " " +
                                              " " +
                                               " ";
                        strBuilder.Append(Answer);

                        string abc = "";
                        if (objcon.SaveStringBuilder(strBuilder, out abc))
                        {

                            TempData["Message"] = "Record update successfully";
                            TempData["Icon"] = "success";
                        }

                        if (abc != "")
                        {
                            DapperORM.DynamicQuerySingle("Insert Into Tool_ErrorLog ( " +
                                                                                "   Error_Desc " +
                                                                                " , Error_FormName " +
                                                                                " , Error_MachinceName " +
                                                                                " , Error_Date " +
                                                                                " , Error_UserID " +
                                                                                " , Error_UserName " + ") values (" +
                                                                                "'" + strBuilder + "'," +
                                                                                "'BuklInsert'," +
                                                                                "'" + Dns.GetHostName().ToString() + "'," +
                                                                                "GetDate()," +
                                                                                "'" + Session["EmployeeId"] + "'," +
                                                                                "'" + Session["EmployeeName"] + "'");
                            TempData["Message"] = abc;
                            TempData["Icon"] = "error";
                        }
                    }
                }

                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetListAssessment
        public ActionResult GetListAssessment(int lmsLibraryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                if (lmsLibraryId==0)
                {
                    lmsLibraryId = Convert.ToInt32(Session["LMSLibraryId"]);
                }
                param.Add("@p_LMSFeedbackID_Encrypted", "List");
                param.Add("@p_LMSLibraryId", lmsLibraryId);
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_LMS_LibraryFeedbackMaster", param);
                ViewBag.GetQuestionList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region GetOption
        public ActionResult GetOption(string LMSFeedbackID)
        {
            try
            {
                var que = DapperORM.DynamicQuerySingle("select options from LMS_LibraryFeedbackDetails where LMS_FeedbackMaster_LMSFeedbackID=" + LMSFeedbackID + "and Deactivate=0 ");
                return Json(que, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");

            }
        }
        #endregion

        #region DeleteAssessment
        public ActionResult DeleteAssessment(string LMSFeedbackID_Encrypted)
        {
            try
            {
                int lmsLibraryId = Convert.ToInt32(Session["LMSLibraryId"]);
                param.Add("@p_process", "Delete");
                param.Add("@p_LMSFeedbackID_Encrypted", LMSFeedbackID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_LMS_LibraryFeedbackMaster", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetListAssessment", "Setting_LMS_Library", new { lmsLibraryId = lmsLibraryId });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
        #endregion




        [HttpPost]
        public ActionResult ScanAndUpload()
        {
            var file = Request.Files["file"];
            if (file == null || file.ContentLength == 0)
            {
                return Content("❌ No file uploaded.");
            }

            // Save the file temporarily
            var tempFolder = Server.MapPath("~/TempUploads/");
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }

            var filePath = Path.Combine(tempFolder, Path.GetFileName(file.FileName));
            file.SaveAs(filePath);

            try
            {
                // Scan the file using Windows Defender
                var scanResult = ScanWithDefender(filePath);

                // Check if the scan result contains "no threats detected"
                if (scanResult.Contains("no threats"))
                {
                    // ✅ Safe
                    return Content("✅ File is clean. You can upload it.");
                }
                else
                {
                    // ❌ Threat found
                    System.IO.File.Delete(filePath); // Delete the file if it's dangerous
                    return Content("❌ Virus detected! File upload blocked.");
                }
            }
            catch (Exception ex)
            {
                return Content($"❌ Error during scan: {ex.Message}");
            }
        }

        private string ScanWithDefender(string filePath)
        {
            // Path to Windows Defender's MpCmdRun.exe
            string platformPath = @"C:\ProgramData\Microsoft\Windows Defender\Platform\";
            var latestVersionFolder = Directory.GetDirectories(platformPath).OrderByDescending(d => d).FirstOrDefault();
            string defenderPath = "";
            if (latestVersionFolder != null)
            {
                defenderPath = Path.Combine(latestVersionFolder, "MpCmdRun.exe");
                Console.WriteLine(defenderPath);  // This will output the latest path
            }
            else
            {
                Console.WriteLine("Windows Defender not found.");
            }

            //string defenderPath = @"C:\ProgramData\Microsoft\Windows Defender\Platform\<latest-version>\MpCmdRun.exe";

            if (!System.IO.File.Exists(defenderPath))
                throw new Exception("Windows Defender not found!");

            var process = new Process();
            process.StartInfo.FileName = defenderPath;
            process.StartInfo.Arguments = $"-Scan -ScanType 3 -File \"{filePath}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;

            process.Start();

            // Read the output synchronously
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output;
        }

    }
}