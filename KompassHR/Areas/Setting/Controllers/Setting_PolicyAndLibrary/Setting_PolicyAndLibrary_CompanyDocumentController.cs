using Dapper;
using KompassHR.Models;
//using KompassHR.Areas.Setting.Models.Setting_Prime;
using KompassHR.Areas.Setting.Models.Setting_PolicyAndLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IO;
//using KompassHR.Areas.Setting.Models.CompanyPolicySetting;

namespace KompassHR.Areas.Setting.Controllers.Setting_PolicyAndLibrary
{
    public class Setting_PolicyAndLibrary_CompanyDocumentController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region Setting_PolicyAndLibrary_CompanyDocument
        public ActionResult Setting_PolicyAndLibrary_CompanyDocument(string CompanyDocumentLibraryId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Mas_CompanyDocumentLibrary mas_CompanyDocumentLiabrary = new Mas_CompanyDocumentLibrary();
                param.Add("@query", "Select  CompanyId, CompanyName from Mas_CompanyProfile where Deactivate=0 order by CompanyName");
                var listMas_CompanyProfile = DapperORM.ReturnList<Mas_CompanyProfile>("sp_QueryExcution", param).ToList();
                ViewBag.Mas_CompanyDocumentList = listMas_CompanyProfile;

                param.Add("@query", "Select CompanyDocumentCategoryId,DocumentCategoryName From Mas_CompanyDocumentCategory where Deactivate=0 order by DocumentCategoryName");
                var mas_CompanyDocumentCategory = DapperORM.ReturnList<Mas_CompanyDocumentCategory>("sp_QueryExcution", param).ToList();
                ViewBag.masCompanyDocumentCategoryList = mas_CompanyDocumentCategory;

                if (CompanyDocumentLibraryId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_CompanyDocumentLibraryId_Encrypted", CompanyDocumentLibraryId_Encrypted);
                    mas_CompanyDocumentLiabrary = DapperORM.ReturnList<Mas_CompanyDocumentLibrary>("sp_List_Mas_CompanyDocumentLibrary", param).FirstOrDefault();
                    Session["SelectedFile"] = mas_CompanyDocumentLiabrary.DocumentPath;
                    TempData["FileName"] = mas_CompanyDocumentLiabrary.DocumentPath;
                    TempData["FileType"] = mas_CompanyDocumentLiabrary.FileType;
                }
                return View(mas_CompanyDocumentLiabrary);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region IsCompanyDocumentExists
        public ActionResult IsCompanyDocumentExists(double CompanyName, double DocumentLiabraryCategory, string CompanyDocumentLiabraryId_Encrypted, string Description, string URL)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_CompanyDocumentLibraryId_Encrypted", CompanyDocumentLiabraryId_Encrypted);
                    param.Add("@p_CmpId", CompanyName);
                    param.Add("@p_CompanyDocumentLibraryCategoryId", DocumentLiabraryCategory);
                    param.Add("@p_Description", Description);
                    param.Add("@p_URL", URL);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_CompanyDocumentLibrary", param);
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
        public ActionResult SaveUpdate(Mas_CompanyDocumentLibrary masCompanyDocumentLiabrary, HttpPostedFileBase FilePath)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(masCompanyDocumentLiabrary.CompanyDocumentLibraryId_Encrypted) ? "Save" : "Update");
                param.Add("@p_CompanyDocumentLibraryId_Encrypted", masCompanyDocumentLiabrary.CompanyDocumentLibraryId_Encrypted);
                param.Add("@p_CompanyDocumentLibraryId", masCompanyDocumentLiabrary.CompanyDocumentLibraryId);
                param.Add("@p_CmpId", masCompanyDocumentLiabrary.CmpID);
                param.Add("@p_CompanyDocumentLibraryCategoryId", masCompanyDocumentLiabrary.CompanyDocumentLibraryCategoryId);
                param.Add("@p_Description", masCompanyDocumentLiabrary.Description);
                param.Add("@p_FileType", masCompanyDocumentLiabrary.FileType);
                if (masCompanyDocumentLiabrary.FileType == "URL")
                {
                    param.Add("@p_DocumentPath", masCompanyDocumentLiabrary.DocumentPath);
                }
                else
                {
                    if (masCompanyDocumentLiabrary.DocumentPath != null && FilePath == null)
                    {
                        param.Add("@p_DocumentPath", Session["SelectedFile"]);
                    }
                    else
                    {
                        param.Add("@p_DocumentPath", FilePath == null ? "" : FilePath.FileName);
                    }
                }

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_CompanyDocumentLibrary", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["p_Id"] = param.Get<string>("@p_Id");

                if (TempData["p_Id"] != null || FilePath != null)
                {
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='CompanyLibrary'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    var FirstPath = GetFirstPath + TempData["p_Id"] + "\\"; // First path plus concat folder by Id
                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }
                    if (FilePath != null)
                    {
                        string ImgTotalKMFullPath = "";
                        foreach (var file in Directory.GetFiles(FirstPath))
                            System.IO.File.Delete(file);
                        ImgTotalKMFullPath = FirstPath + FilePath.FileName; //Concat Full Path and create New full Path
                        FilePath.SaveAs(ImgTotalKMFullPath); // This is use for Save image in folder full path
                    }
                }
                return RedirectToAction("Setting_PolicyAndLibrary_CompanyDocument", "Setting_PolicyAndLibrary_CompanyDocument");
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
                param.Add("@p_CompanyDocumentLibraryId_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Mas_CompanyDocumentLibrary", param);
                ViewBag.CompanyDocumentList = data;
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
        public ActionResult Delete(string CompanyDocumentLibraryId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_CompanyDocumentLibraryId_Encrypted", CompanyDocumentLibraryId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_CompanyDocumentLibrary", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_PolicyAndLibrary_CompanyDocument");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Download Image 
        public ActionResult DownloadFile(string DocumentPath)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (!System.IO.File.Exists(DocumentPath))
                {
                    TempData["Message"] = "File Not Found";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetList", "Setting_PolicyAndLibrary_CompanyDocument", new { Area = "Setting" });
                }
                else
                {
                    var fileBytes = System.IO.File.ReadAllBytes(DocumentPath);
                    var fileName = Path.GetFileName(DocumentPath);

                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }

                //if (DocumentPath != null)
                //{
                //    System.IO.File.ReadAllBytes(DocumentPath);
                //    return File(DocumentPath, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(DocumentPath));
                //}
                //else
                //{
                //    return RedirectToAction("GetList", "Setting_PolicyAndLibrary_CompanyDocument", new { Area = "Setting" });
                //}
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