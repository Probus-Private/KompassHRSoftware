using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Preboarding;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Preboarding
{
    public class Setting_Preboarding_AttachmentsController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: Setting/Setting_Preboarding_Attachments
        public ActionResult Setting_Preboarding_Attachments()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 799;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdateMultiple(Preboarding_Document Doc, List<HttpPostedFileBase> AttachmentFiles, List<string> FileNames)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                if (AttachmentFiles == null || AttachmentFiles.Count == 0)
                {
                    TempData["Message"] = "Please upload at least one file.";
                    TempData["Icon"] = "warning";
                    return RedirectToAction("Setting_Preboarding_Attachments", "Setting_Preboarding_Attachments");
                }

                // 🔁 Loop through each file
                for (int i = 0; i < AttachmentFiles.Count; i++)
                {
                    var file = AttachmentFiles[i];
                    var fileName = FileNames[i];

                    if (file != null)
                    {
                        DynamicParameters param = new DynamicParameters();
                        param.Add("@p_process", string.IsNullOrEmpty(Doc.PreboardingDocumentId_Encrypted) ? "Save" : "Update");
                        param.Add("@p_PreboardingDocumentId_Encrypted", Doc.PreboardingDocumentId_Encrypted);
                        param.Add("@p_DocumentName", fileName);
                        param.Add("@p_DocumentPath", file.FileName);
                        param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                        param.Add("@p_MachineName", Dns.GetHostName().ToString());
                        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                        param.Add("@p_CmpId", Session["CompanyId"]);
                        // 💾 Call stored procedure
                        var data = DapperORM.ExecuteReturn("sp_SUD_Preboarding_Document", param);

                        var message = param.Get<string>("@p_msg");
                        var icon = param.Get<string>("@p_Icon");
                        TempData["Message"] = message;
                        TempData["Icon"] = icon;

                        // 💾 Save file physically
                        var GetDocPath = DapperORM.DynamicQuerySingle("SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin='PreboardingAttachment'");
                        var BasePath = GetDocPath.DocInitialPath;
                        var FolderPath = Path.Combine(BasePath, "PreboardingDocuments");

                        if (!Directory.Exists(FolderPath))
                            Directory.CreateDirectory(FolderPath);

                        string fileSavePath = Path.Combine(FolderPath, file.FileName);
                        file.SaveAs(fileSavePath);

                    }

                }
                return Json(new { icon = "success", message = "Record saved successfully." });

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 799;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var result = DapperORM.ReturnList<dynamic>("sp_List_Preboarding_Attachment").ToList();
                ViewBag.GetDocumentsList = result;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

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
                    return RedirectToAction("DownloadDoc", "Onboarding");
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


        public ActionResult Delete(string PreboardingDocumentId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Delete");
                param.Add("@p_PreboardingDocumentId_Encrypted", PreboardingDocumentId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = DapperORM.ExecuteReturn("sp_SUD_Preboarding_Document", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Preboarding_Attachments");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

    }
}