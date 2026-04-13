using Dapper;
using KompassHR.Areas.ESS.Models.ESS_KompassHR_HelpDesk;
using KompassHR.Models;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Runtime.InteropServices;
using System.Net.Http;
using Newtonsoft.Json;

namespace KompassHR.Areas.ESS.Controllers.ESS_KompassHR_HelpDesk
{
    public class ESS_KompassHRHelpDesk_HelpDeskController : Controller
    {
        SqlConnection sqlcons = new SqlConnection(DapperORM.connectionStrings);
        DynamicParameters param = new DynamicParameters();

        // GET: ESS/ESS_KompassHRHelpDesk_HelpDesk

        #region MainView
        public ActionResult ESS_KompassHRHelpDesk_HelpDesk(string HelpDeskId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 794;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var GetDocNo = DapperORM.DynamicQuerySingle("Select Isnull(Max(HelpDesk_DocNo),0)+1 As DocNo from Kompass_HelpDesk");
                var DocNo = GetDocNo != null ? GetDocNo.DocNo : null;               
                ViewBag.DocNo = DocNo;

                DynamicParameters param = new DynamicParameters();
                Kompass_HelpDesk HelpDesk = new Kompass_HelpDesk();
                if (HelpDeskId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    var EmployeeId = Session["EmployeeId"];
                    param.Add("@p_HelpDeskId_Encrypted", "List");
                    param.Add("@p_Qry", " and Kompass_HelpDesk.HelpDeskId_Encrypted='" + HelpDeskId_Encrypted + "' and Status='In-Process'");
                    HelpDesk = DapperORM.ReturnList<Kompass_HelpDesk>("sp_List_Kompass_Helpdesk", param).FirstOrDefault();
                    TempData["DocDate"] = HelpDesk.HelpDesk_DocDate;
                    TempData["AttachFile"] = HelpDesk.AttachFile;
                    TempData["FilePath"] = HelpDesk.FilePath;
                    Session["SelectedFile"] = HelpDesk.FilePath;
                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {
                        param = new DynamicParameters();
                        var GetVoucherNo = "Select HelpDesk_DocNo from Kompass_HelpDesk where HelpDeskId_Encrypted='" + HelpDeskId_Encrypted + "'";
                        var VoucherNo = DapperORM.DynamicQuerySingle(GetVoucherNo);
                        ViewBag.DocNo = VoucherNo.HelpDesk_DocNo;
                    }
                }


                param = new DynamicParameters();
                var GetModuleName = "";
                ViewBag.ModuleList = GetModuleName;

                DynamicParameters paramOption = new DynamicParameters();
                paramOption.Add("@query", "Select ModuleID as Id, ModuleName as Name from Kompass_Modules where Decativate=0");
                var List_Options = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", paramOption).ToList();
                ViewBag.GetModules = List_Options;
                return View(HelpDesk);
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
        
        public ActionResult SaveUpdate(Kompass_HelpDesk helpdesk, HttpPostedFileBase FilePath)
        {
            try
            {
                var EmployeeId = Session["EmployeeId"];
                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_Employeeid", EmployeeId);
                var Empdata = DapperORM.ReturnList<EmployeeGlobalSearch>("sp_GetEmployeeDetails", param2).FirstOrDefault();

                var GetDocNo = DapperORM.DynamicQuerySingle("Select Isnull(Max(HelpDesk_DocNo),0)+1 As DocNo from Kompass_HelpDesk");
                var DocNo = GetDocNo != null ? GetDocNo.DocNo : null;

                DynamicParameters param = new DynamicParameters();
               
                param.Add("@p_process", string.IsNullOrEmpty(helpdesk.HelpDeskId_Encrypted) ? "Save" : "Update");

                param.Add("@p_HelpDeskId_Encrypted", helpdesk.HelpDeskId_Encrypted);
                param.Add("@p_HelpDesk_DocNo", DocNo);
                param.Add("@p_HelpDesk_DocDate", helpdesk.HelpDesk_DocDate);
                param.Add("@p_HelpDesk_RequestBy", EmployeeId);
                param.Add("@p_HelpDesk_RequestByName", Session["EmployeeName"]);

                param.Add("@p_HelpDesk_Priority", helpdesk.HelpDesk_Priority);
                param.Add("@p_HelpDesk_RequestType", helpdesk.HelpDesk_RequestType);
                param.Add("@p_RequestType_DescriptionOther", helpdesk.RequestType_DescriptionOther);
                param.Add("@p_HelpDesk_Title", helpdesk.HelpDesk_Title);
                param.Add("@p_HelpDesk_Description", helpdesk.HelpDesk_Description);
                if (helpdesk.HelpDeskId_Encrypted != null && FilePath == null)
                {
                    param.Add("@p_FilePath", Session["SelectedFile"]);
                }
                else
                {
                    param.Add("@p_FilePath", FilePath == null ? "" : FilePath.FileName);
                }
                param.Add("@p_CmpId", Session["CompanyId"]);
                param.Add("@p_BranchID", Session["BranchId"]);                
                param.Add("@p_ExpectedResult", helpdesk.ExpectedResult);
                param.Add("@p_ActualResult", helpdesk.ActualResult);
                param.Add("@p_ModuleName", helpdesk.ModuleName);                               
                param.Add("@p_CustomerCode", Session["ESSCustomerCode"]);

                param.Add("@p_CompanyName", Empdata.CompanyName);
                param.Add("@p_BranchName", Empdata.BranchName);
                param.Add("@p_Department", Empdata.DepartmentName);
                param.Add("@p_Designation", Empdata.DesignationName);
                param.Add("@p_MobileNo", Empdata.PrimaryMobile);
                param.Add("@p_EmailId", Empdata.PersonalEmailId);

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Kompass_HelpDesk", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                var DocPKId = param.Get<string>("@p_Id"); 
                if (FilePath != null)
                {
                    var docPathQuery = "Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='KompassHelpDesk'";
                    var getDocPath = DapperORM.DynamicQuerySingle(docPathQuery);

                    string basePath = getDocPath.DocInitialPath;
                    string folderPath = TempData["P_Id"] != null
                        ? Path.Combine(basePath, TempData["P_Id"].ToString())
                        : Path.Combine(basePath, helpdesk.HelpDeskId.ToString());

                    // Create directory if it doesn't exist
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    // Save the file
                    string fullFilePath = Path.Combine(folderPath, FilePath.FileName);
                    FilePath.SaveAs(fullFilePath);
                }

                //Save to Support server
                var GetConnectionString = sqlcons.Query(@"Select Con_Server,Con_UserId,Con_Password,Con_Database from HelpDesk_Support where Deactivate=0 and IsActive=1").FirstOrDefault();
                if (GetConnectionString == null)
                {
                    var data = "";
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
               else
                {
                    DapperORM.SetConnectionHelpDesk(GetConnectionString.Con_Server, GetConnectionString.Con_UserId, GetConnectionString.Con_Password, GetConnectionString.Con_Database);
                    var GetDocNo1 = DapperORM.DynamicQuerySingleHelpDesk("Select Isnull(Max(HelpDesk_DocNo),0)+1 As DocNo from Kompass_HelpDesk");
                    var DocNo1 = GetDocNo1 != null ? GetDocNo1.DocNo : null;

                    param.Add("@p_HelpDesk_DocNo", DocNo1);
                    DapperORM.ExecuteReturnHelpDesk("sp_SUD_Kompass_HelpDesk", param);
                }

                if (helpdesk.FilePath != null)
                {
                    var GetDocPath = DapperORM.DynamicQuerySingleHelpDesk("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='KompassHelpDesk'");
                    var GetFirstPath = GetDocPath.DocInitialPath;

                    var DocRead = DapperORM.DynamicQuerySingleHelpDesk("Select * from Tool_LMS_DownloadUpload_URL where DocOrigin='KompassHelpDeskDocUpload'");
                    var GetDocReadURL = DocRead.DownloadUploadURL;
                    string VerificationUrl = GetDocReadURL;
                    //string VerificationUrl = "http://10.48.218.3:8096/api/LMSDocUpload";
                    using (HttpClient client = new HttpClient())
                    {
                        using (var formData = new MultipartFormDataContent())
                        {
                            // Add string parameters
                            formData.Add(new StringContent(GetFirstPath), "FilePath");
                            formData.Add(new StringContent(DocPKId.ToString()), "LMSLibraryId");

                            if (FilePath != null && FilePath.ContentLength > 0)
                            {
                                var streamContent = new StreamContent(FilePath.InputStream);
                                streamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                                {
                                    Name = "\"File\"",
                                    FileName = $"\"{DocPKId + "_" + FilePath.FileName}\""
                                };
                                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(FilePath.ContentType); // e.g., application/pdf
                                formData.Add(streamContent, "File", FilePath.FileName);
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
               
                //save to secondary server
                //string secondaryConnString = ConfigurationManager.ConnectionStrings["SecondaryConnection"].ConnectionString;
                //using (var conn2 = new SqlConnection(secondaryConnString))
                //{
                //    conn2.Open();
                //    conn2.Execute("sp_SUD_Kompass_HelpDesk", param, commandType: CommandType.StoredProcedure);
                //}

                //    if (helpdesk.FilePath != null)
                //    {
                //        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='KompassHelpDesk'");
                //        var GetFirstPath = GetDocPath.DocInitialPath;

                //        var DocRead = DapperORM.DynamicQuerySingle("Select * from Tool_LMS_DownloadUpload_URL where DocOrigin='KompassHelpDesk'");
                //        var GetDocReadURL = DocRead.DownloadUploadURL;
                //        string VerificationUrl = GetDocReadURL;
                //        //string VerificationUrl = "http://10.48.218.3:8096/api/LMSDocUpload";
                //        using (HttpClient client = new HttpClient())
                //        {
                //            using (var formData = new MultipartFormDataContent())
                //            {
                //                // Add string parameters
                //                formData.Add(new StringContent(GetFirstPath), "FilePath");
                //                formData.Add(new StringContent(DocPKId.ToString()), "LMSLibraryId");

                //                if (FilePath != null && FilePath.ContentLength > 0)
                //                {
                //                    var streamContent = new StreamContent(FilePath.InputStream);
                //                    streamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                //                    {
                //                        Name = "\"File\"",
                //                        FileName = $"\"{DocPKId + "_" + FilePath.FileName}\""
                //                    };
                //                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(FilePath.ContentType); // e.g., application/pdf
                //                    formData.Add(streamContent, "File", FilePath.FileName);
                //                }

                //                // Send the form-data content
                //                HttpResponseMessage response = client.PostAsync(VerificationUrl, formData).GetAwaiter().GetResult();

                //                if (response.IsSuccessStatusCode)
                //                {
                //                    string jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                //                    var FinalData = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                //                }
                //            }
                //        }
                //    }                
                //else
                //{

                //    if (FilePath != null)
                //    {
                //        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='KompassHelpDesk'");
                //        var GetFirstPath = GetDocPath.DocInitialPath;
                //        var FirstPath = GetFirstPath + helpdesk.HelpDeskId + "\\"; // First path plus concat folder 
                //        string directoryPath = Path.GetDirectoryName(FirstPath);
                //        if (!Directory.Exists(directoryPath))
                //        {
                //            Directory.CreateDirectory(FirstPath);
                //        }
                //        string AttachFilePath = "";
                //        AttachFilePath = FirstPath + FilePath.FileName; //Concat Full Path and create New full Path
                //        FilePath.SaveAs(AttachFilePath); // This is use for Save image in folder full path
                //    }
                //}


                return RedirectToAction("ESS_KompassHRHelpDesk_HelpDesk", "ESS_KompassHRHelpDesk_HelpDesk");
            }
            catch (Exception Ex)
            {
                Session["GetErrorMessage"] = Ex.Message;
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 200;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                //DynamicParameters param = new DynamicParameters();
                //param.Add("@p_HelpDeskId_Encrypted", "List");
                //param.Add("@p_Qry", " and Kompass_HelpDesk.HelpDesk_RequestBy='" + Session["EmployeeId"] + "'");
                //var data = DapperORM.DynamicList("sp_List_Kompass_Helpdesk", param);
                //ViewBag.GetList = data;

                DynamicParameters param = new DynamicParameters();

                // Input parameters
                param.Add("@p_HelpDeskId_Encrypted", "List");
                param.Add("@p_Qry", " AND Kompass_HelpDesk.HelpDesk_RequestBy='" + Session["EmployeeId"] + "'");
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                var data = DapperORM.DynamicList("sp_List_Kompass_Helpdesk", param);
                ViewBag.GetList = data;
                string msg = param.Get<string>("@p_msg");

                return View();
            }
            catch (Exception Ex)
            {
                Session["GetErrorMessage"] = Ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion
    }
}