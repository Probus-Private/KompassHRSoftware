using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Training;
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

namespace KompassHR.Areas.ESS.Controllers.ESS_Training
{
    public class ESS_Training_TrainingPlanController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        //[HttpGet]
        #region Training Plan Main View 
        public ActionResult ESS_Training_TrainingPlan(int? TrainingCalenderId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 558;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Training_TrainingPlan TrainingPlan = new Training_TrainingPlan
                {
                    StartDate = DateTime.Today 
                };

                ViewBag.AddUpdateTitle = "Add";
                ViewBag.GetCalenderAssignedEmpolyeeList = "";

                ViewBag.GetAgency = "";
                ViewBag.GetTrainer = "";
                ViewBag.GetIntenalTraineeEmployee = "";

                DynamicParameters paramtrainingCalender = new DynamicParameters();
                var GetTrainingCalender = DapperORM.ReturnList<AllDropDownBind>("sp_GetTrainingCalenderDropdown", paramtrainingCalender).ToList();
                var CID = GetTrainingCalender.FirstOrDefault()?.Id;
                ViewBag.TrainingCalender = GetTrainingCalender;


                return View(TrainingPlan);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsBatchAlreadyExists
        [HttpGet]
        public ActionResult IsBatchAlreadyExists(int? TrainingCalenderId, string Batch)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_TrainingCalenderId", TrainingCalenderId);
                    param.Add("@p_Batch", Batch);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_TrainingPlan", param);
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

        #region SaveUpadte
        [HttpPost]
        public ActionResult SaveUpadte(List<Training_TrainingPlan> CalenderEmployeeList, string TrainingCalenderId, string Batch,
            string StartDate, string StartTime, string EndDate, string EndTime,string Location,string TrainingProviderSource,
            double TrainingProviderId, string TrainingProvidedBy)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 558;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                var param = new DynamicParameters();
                param.Add("@p_process", "Save");
                param.Add("@p_TrainingCalenderId", CalenderEmployeeList[0].TrainingCalenderId);
                param.Add("@p_Batch", Batch);
                param.Add("@p_StartDate", StartDate);
                param.Add("@p_StartTime", StartTime);
                param.Add("@p_EndDate", EndDate);
                param.Add("@p_EndTime", EndTime);
                param.Add("@p_MachineName", Dns.GetHostName());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500); // Output will contain new MasterId
                var data = DapperORM.ExecuteReturn("sp_SUD_TrainingPlan", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                string p_Id = param.Get<string>("@p_Id");
                StringBuilder strBuilder = new StringBuilder();

                string qry = "";
                var createdBy = Session["EmployeeName"]?.ToString().Replace("'", "''");
                var machineName = Dns.GetHostName().Replace("'", "''");
                qry = "UPDATE dbo.Training_Calender " +
                    "SET Location = '"+ Location + "', " +
                    "TrainingProviderSource = '" + TrainingProviderSource + "', " +
                    "TrainingProviderId = '" + TrainingProviderId + "', " +
                    "TrainingProvidedBy = '" + TrainingProvidedBy + "', " +
                    "ModifiedBy = '" + createdBy + "', " +
                    "ModifiedDate = GETDATE(), " +
                    "MachineName = '" + machineName + "' " +
                    "WHERE TrainingCalenderId = '" + CalenderEmployeeList[0].TrainingCalenderId + "';";

                strBuilder.Append(qry);

                for (var i = 0; i < CalenderEmployeeList.Count; i++)
                {
                    var employeeId = CalenderEmployeeList[i].EmployeeId;
                    qry = "INSERT INTO dbo.TrainingPlan_Details " +
                                "(TrainingPlan_MasterId, EmployeeId, Deactivate,CreatedBy, CreatedDate, MachineName) " +
                                "VALUES ('" + p_Id + "', '" + employeeId + "',0, '" + createdBy + "', GETDATE(), '" + machineName + "');";

                    strBuilder.Append(qry);
                }
                string abc = "";
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    TempData["Message"] = "Record save successfully";
                    TempData["Icon"] = "success";
                }
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
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
        public ActionResult GetList(string TrainingCalenderId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 558;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
               
                //param.Add("@P_TrainingPlan_MasterId_Encrypted", "List");
                param.Add("@P_TrainingCalenderId", TrainingCalenderId);
                var GetPlanData = DapperORM.ExecuteSP<dynamic>("sp_List_TrainingPlan", param).ToList();
                ViewBag.GetTrainingPlan = GetPlanData;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region Training Calender details
        [HttpGet]
        public ActionResult GetTrainingCalenderDetails(int TrainingCalenderId)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@p_TrainingCalenderId", TrainingCalenderId);
            var trainingCalender = DapperORM.ReturnList<Training_TrainingCalender>("sp_GetTrainingCalenderDetails", parameters).FirstOrDefault();
            TempData["MonthYear"] = trainingCalender.MonthYear.ToString("MMMM yyyy");

            var result = new
            {
                TrainingDetails = trainingCalender,
                FormattedMonthYear = trainingCalender.MonthYear.ToString("MMMM yyyy")
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Employee details
        [HttpGet]
        public ActionResult GetEmployeeDetails(int? TrainingCalenderId, string DepartmentIds)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }

            List<Training_TrainingPlan> data = new List<Training_TrainingPlan>();

            if (TrainingCalenderId != null)
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_TrainingCalenderId", TrainingCalenderId);
                param.Add("@p_DepartmentIds", DepartmentIds);

                data = DapperORM.ReturnList<Training_TrainingPlan>("sp_Training_GetEmployeeList", param).ToList();
            }
            return Json(data, JsonRequestBehavior.AllowGet);
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
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetInternalEmployeeDropdown", paramtraineremp).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region Trainer
        [HttpGet]
        public ActionResult GetTrainer()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters paramtrainer = new DynamicParameters();
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetTrainerDropdown", paramtrainer).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region Agency
        [HttpGet]
        public ActionResult GetAgency()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters paramagency = new DynamicParameters();
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetAgencyDropdown", paramagency).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetTraining Plan List
        [HttpGet]
        public ActionResult GetTrainingPlanList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 559;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                //THIS LIST IS TRAINING PLAN LIST
                DynamicParameters param = new DynamicParameters();
                param.Add("@P_TrainingPlan_MasterId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_TrainingPlan", param).ToList();
                ViewBag.GetTrainingtPlan = data;


                //THIS LIST IS TRAINING CALENDAR LIST
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@P_TrainingCalenderId_Encrypted", "List");
                var GetCalenderData = DapperORM.ExecuteSP<dynamic>("sp_List_TrainingCalender", param1).ToList();
                ViewBag.GetTrainingCalender = GetCalenderData;

                //THIS LIST IS TRAINING COMPLETED LIST
                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@P_TrainingCalenderId", "List");
                var GetCompleteTrainingData = DapperORM.ExecuteSP<dynamic>("sp_List_TrainingConduct", param2).ToList();
                ViewBag.GetCompleteTraining = GetCompleteTrainingData;

                //THIS LIST IS TRAINING PENDING EMPLOYEE LIST
                DynamicParameters param3 = new DynamicParameters();
                param3.Add("@P_TrainingCalenderId", "List");
                var GetPindingTrainingData = DapperORM.ExecuteSP<dynamic>("sp_List_TrainingPending", param3).ToList();
                ViewBag.GetPendingTraining = GetPindingTrainingData;

                //THIS LIST IS TRAINING BATCH PENDING LIST
                DynamicParameters param4 = new DynamicParameters();
                var GetPindingTrainingBatchData = DapperORM.ExecuteSP<dynamic>("sp_List_Pending_Training_Batch", param4).ToList();
                ViewBag.GetPendingTrainingBatch = GetPindingTrainingBatchData;

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
        public ActionResult Delete(double? TrainingPlan_MasterId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
               
                StringBuilder strBuilder = new StringBuilder();
                var createdBy = Session["EmployeeName"]?.ToString().Replace("'", "''");
                    var machineName = Dns.GetHostName().Replace("'", "''");
                strBuilder.AppendLine("UPDATE dbo.TrainingPlan_Master " +
                    "SET Deactivate = 1, " +
                    "ModifiedBy = '" + createdBy + "', " +
                    "ModifiedDate = GETDATE(), " +
                    "MachineName = '" + machineName + "' " +
                    "WHERE TrainingPlan_MasterId = '" + TrainingPlan_MasterId + "';");

                strBuilder.AppendLine("UPDATE dbo.TrainingPlan_Details " +
                    "SET Deactivate = 1 " +
                    "WHERE TrainingPlan_MasterId = '" + TrainingPlan_MasterId + "';");

                string abc = "";
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    TempData["Message"] = "Record Deleted successfully.";
                    TempData["Icon"] = "success";
                }
                return RedirectToAction("GetTrainingPlanList", "ESS_Training_TrainingPlan");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Employee details
        [HttpGet]
        public ActionResult ShowEmployeeList(int? TrainingCalenderId, string Batch)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            DynamicParameters param = new DynamicParameters();
            param.Add("@p_TrainingCalenderId", TrainingCalenderId);
            param.Add("@p_Batch", Batch);
            var data = DapperORM.ReturnList<dynamic>("sp_ShowEmployeeList", param).ToList();
            string jsonData = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region TrainingPlanUploadFile
        public ActionResult TrainingPlanUploadFile(string TrainingPlan_MasterId_Encrypted, int? TrainingPlan_MasterId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 558;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Session["TrainingPlan_MasterId"] = TrainingPlan_MasterId;
                Session["TrainingPlan_MasterId_Encrypted"] = TrainingPlan_MasterId_Encrypted;

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_TrainingPlanDoc_Encrypted", "List");
                param.Add("@p_TrainingPlan_MasterId", Session["TrainingPlan_MasterId"]);
                var data = DapperORM.ReturnList<dynamic>("sp_List_TrainingPlan_Document", param).ToList();
                ViewBag.GetListTrainingPlan = data;
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
        public ActionResult SaveUpdateUploadFile(TrainingPlan_Document PlanDoc, HttpPostedFileBase DocumentPath)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                string extension = Path.GetExtension(DocumentPath.FileName).ToLower();
                param.Add("@p_process", string.IsNullOrEmpty(PlanDoc.TrainingPlanDoc_Encrypted) ? "Save" : "Update");
                param.Add("@p_TrainingPlanDoc_Encrypted", PlanDoc.TrainingPlanDoc_Encrypted);
                param.Add("@p_TrainingPlan_PlanId", Session["TrainingPlan_MasterId"]);
                param.Add("@p_Title", PlanDoc.Title);
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

                if (PlanDoc.TrainingPlanDoc_Encrypted != null && DocumentPath == null)
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
                var Result = DapperORM.ExecuteReturn("sp_SUD_TrainingPlan_Document", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                var DocPKId = param.Get<string>("@p_Id");
                if (Session["TrainingPlan_MasterId"] != null)
                {

                    if (PlanDoc.DocumentPath != null)
                    {
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='TrainingPlan'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        
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
                                formData.Add(new StringContent(Session["TrainingPlan_MasterId"].ToString()), "LMSLibraryId");

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
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='TrainingPlan'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        var FirstPath = GetFirstPath + PlanDoc.TrainingPlanDocId + "\\"; // First path plus concat folder 
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
                return RedirectToAction("TrainingPlanUploadFile", "ESS_Training_TrainingPlan", new { TrainingPlan_MasterId_Encrypted = Session["TrainingPlan_MasterId_Encrypted"], TrainingPlan_MasterId = Session["TrainingPlan_MasterId"] });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region DeleteDocument
        public ActionResult DeleteDocument(string TrainingPlanDoc_Encrypted, string FileName, int TrainingPlan_PlanId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                param.Add("@p_process", "Delete");
                param.Add("@p_TrainingPlanDoc_Encrypted", TrainingPlanDoc_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_TrainingPlan_Document", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                var DocPKId = param.Get<string>("@p_Id");

                var GetDocPath = DapperORM.DynamicQueryList("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='TrainingPlan'").FirstOrDefault();
                var GetFirstPath = GetDocPath.DocInitialPath + "\\" + TrainingPlan_PlanId + "\\";

                var DocRead = DapperORM.DynamicQueryList("Select * from Tool_LMS_DownloadUpload_URL where DocOrigin='LMSDocDelete'").FirstOrDefault();
                var GetDocReadURL = DocRead.DownloadUploadURL;
                string VerificationUrl = GetDocReadURL;
                //string VerificationUrl = "http://10.48.218.3:8096/api/LMSDocDelete";
                using (HttpClient client = new HttpClient())
                {
                    using (var formData = new MultipartFormDataContent())
                    {
                        // Add string parameters
                        var SetFileName =  FileName;
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
                return RedirectToAction("TrainingPlanUploadFile", "ESS_Training_TrainingPlan", new { TrainingPlan_MasterId_Encrypted = Session["TrainingPlan_MasterId_Encrypted"], TrainingPlan_MasterId = Session["TrainingPlan_MasterId"] });
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
    }
}