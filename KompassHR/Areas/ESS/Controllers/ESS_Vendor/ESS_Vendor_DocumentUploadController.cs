using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Vendor;
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

namespace KompassHR.Areas.ESS.Controllers.ESS_Vendor
{
    public class ESS_Vendor_DocumentUploadController : Controller
    {
        DynamicParameters param = new DynamicParameters();

        // GET: ESS/ESS_Vendor_DocumentUpload
        public ActionResult ESS_Vendor_DocumentUpload()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 600;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_VendorId", Session["VendorId"]);
                var data = DapperORM.DynamicList("sp_List_Mas_Vendor_CapacityDomain", param);
                ViewBag.GetCapacityDetails = data;

                var VendorId = Convert.ToInt32(Session["VendorId"]);
                param.Add("@p_VendorId", VendorId);
                var DocumentDetails = DapperORM.ReturnList<dynamic>("sp_List_Mas_Vendor_Documents", param).ToList();
                ViewBag.DocumentDetails = DocumentDetails;
                return View();
            }
            catch (Exception ex)
            {
                TempData["Message"] = "An error occurred while loading the page.";
                TempData["Icon"] = "error";
                return RedirectToAction("ESS_Vendor_DocumentUpload");
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveUpdate(Mas_Vendor_Documents document,
            HttpPostedFileBase GSTCertificate, HttpPostedFileBase PANCard, HttpPostedFileBase EPFOCerificate,
            HttpPostedFileBase LabourLicense, HttpPostedFileBase CancelledCheque,
            HttpPostedFileBase CompanyProfile, HttpPostedFileBase SignedAgreement)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    TempData["Message"] = "Your session has expired. Please log in again.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                // Validate at least one file is uploaded for new records
                if (string.IsNullOrEmpty(document.VendorDocumentId_Encrypted) &&
                    GSTCertificate == null && PANCard == null && EPFOCerificate == null &&
                    LabourLicense == null && CancelledCheque == null &&
                    CompanyProfile == null && SignedAgreement == null)
                {
                    TempData["Message"] = "Please upload at least one document.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("ESS_Vendor_DocumentUpload");
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", string.IsNullOrEmpty(document.VendorDocumentId_Encrypted) ? "Save" : "Update");
                param.Add("@p_VendorId", document.VendorId);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());

                // Only add file parameters if files are provided
                if (GSTCertificate != null)
                    param.Add("@p_GSTCertificate", (document.VendorId + "-" + "GSTCertificate" + Path.GetExtension(GSTCertificate.FileName)));
                if (PANCard != null)
                    param.Add("@p_PANCard", (document.VendorId + "-" + "PANCard" + Path.GetExtension(PANCard.FileName)));
                if (EPFOCerificate != null)
                    param.Add("@p_EPFOCerificate", (document.VendorId + "-" + "EPFOCerificate" + Path.GetExtension(EPFOCerificate.FileName)));
                if (LabourLicense != null)
                    param.Add("@p_LabourLicense", (document.VendorId + "-" + "LabourLicense" + Path.GetExtension(LabourLicense.FileName)));
                if (CancelledCheque != null)
                    param.Add("@p_CancelledCheque", (document.VendorId + "-" + "CancelledCheque" + Path.GetExtension(CancelledCheque.FileName)));
                if (CompanyProfile != null)
                    param.Add("@p_CompanyProfile", (document.VendorId + "-" + "CompanyProfile" + Path.GetExtension(CompanyProfile.FileName)));
                if (SignedAgreement != null)
                    param.Add("@p_SignedAgreement", (document.VendorId + "-" + "SignedAgreement" + Path.GetExtension(SignedAgreement.FileName)));

                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                var data = DapperORM.ExecuteReturn("sp_SUD_Mas_Vendor_Documents", param);
                var message = param.Get<string>("@p_msg");
                var icon = param.Get<string>("@p_Icon");
                var P_Id = param.Get<string>("@p_Id");

                TempData["Message"] = message;
                TempData["Icon"] = icon;

                if (!string.IsNullOrEmpty(P_Id))
                {
                    var GetDocPath = DapperORM.DynamicQuerySingle("SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin = 'Vendor'");
                    string basePath = Path.Combine(GetDocPath.DocInitialPath, P_Id);

                    if (!Directory.Exists(basePath))
                        Directory.CreateDirectory(basePath);

                    // Only save files that were provided
                    if (GSTCertificate != null)
                        SaveFile(GSTCertificate, "GSTCertificate", document.VendorId.ToString(), basePath);
                    if (PANCard != null)
                        SaveFile(PANCard, "PANCard", document.VendorId.ToString(), basePath);
                    if (EPFOCerificate != null)
                        SaveFile(EPFOCerificate, "EPFOCerificate", document.VendorId.ToString(), basePath);
                    if (LabourLicense != null)
                        SaveFile(LabourLicense, "LabourLicense", document.VendorId.ToString(), basePath);
                    if (CancelledCheque != null)
                        SaveFile(CancelledCheque, "CancelledCheque", document.VendorId.ToString(), basePath);
                    if (CompanyProfile != null)
                        SaveFile(CompanyProfile, "CompanyProfile", document.VendorId.ToString(), basePath);
                    if (SignedAgreement != null)
                        SaveFile(SignedAgreement, "SignedAgreement", document.VendorId.ToString(), basePath);
                }

                return RedirectToAction("ESS_Vendor_DocumentUpload");
            }
            catch (Exception ex)
            {
                TempData["Message"] = "An error occurred while saving the documents.";
                TempData["Icon"] = "error";
                return RedirectToAction("ESS_Vendor_DocumentUpload");
            }
        }

        private string SaveFile(HttpPostedFileBase file, string type, string id, string folderPath)
        {
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    // Validate file size (5MB limit)
                    if (file.ContentLength > 5 * 1024 * 1024)
                    {
                        throw new Exception($"File {type} exceeds maximum size limit of 5MB");
                    }

                    // Validate file extensions
                    var allowedExtensions = new[] { ".pdf", ".jpg", ".png", ".doc", ".docx" };
                    string extension = Path.GetExtension(file.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        throw new Exception($"Invalid file type for {type}. Allowed types: PDF, JPG, PNG, DOC, DOCX");
                    }

                    string fileName = $"{id}-{type}{extension}";
                    string fullPath = Path.Combine(folderPath, fileName);

                    // Ensure unique filename
                    if (System.IO.File.Exists(fullPath))
                    {
                        fileName = $"{id}-{type}-{DateTime.Now:yyyyMMddHHmmss}{extension}";
                        fullPath = Path.Combine(folderPath, fileName);
                    }

                    file.SaveAs(fullPath);
                    return fileName;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving {type} file: {ex.Message}");
            }
        }

        [HttpGet]
        public ActionResult IsVendorDocumentExist(string VendorId, string VendorDocumentId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return Json(new { Message = "Session expired. Please login again.", Icon = "error" }, JsonRequestBehavior.AllowGet);
                }

                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var param = new DynamicParameters();
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_VendorId", VendorId);
                    param.Add("@p_VendorDocumentId_Encrypted", VendorDocumentId_Encrypted);
                    param.Add("@p_MachineName", Dns.GetHostName());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                    sqlcon.Execute("sp_SUD_Mas_Vendor_Documents", param, commandType: CommandType.StoredProcedure);

                    var Message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");

                    if (!string.IsNullOrEmpty(Message))
                    {
                        return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Message = "An error occurred during validation.", Icon = "error" }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult ViewVendorDocument(string encId, string fileName)
        {
            if (Session["EmployeeId"] == null)
                return new HttpStatusCodeResult(401);

            // 🔥 Physical path ONLY from DB
            var docPath = DapperORM.DynamicQuerySingle(
                "SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin = 'Vendor'"
            );

            if (docPath == null)
                return HttpNotFound("Document path not configured");

            string fullPath = Path.Combine(
                docPath.DocInitialPath,
                encId,
                fileName
            );

            if (!System.IO.File.Exists(fullPath))
                return HttpNotFound("File not found");

            return File(fullPath, MimeMapping.GetMimeMapping(fullPath));
        }

    }
}