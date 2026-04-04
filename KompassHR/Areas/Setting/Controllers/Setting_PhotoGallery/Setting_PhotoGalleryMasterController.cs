using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Contractor;
using KompassHR.Areas.Setting.Models.Setting_PhotoGalleryMaster;
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

namespace KompassHR.Areas.Setting.Controllers.Setting_PhotoGallery
{
    public class Setting_PhotoGalleryMasterController : Controller
    {
        // GET: Setting/Setting_PhotoGalleryMaster
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        public ActionResult Setting_PhotoGalleryMaster()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Setting_PhotoGalleryMaster SettingPhotoGalleryMaster = new Setting_PhotoGalleryMaster();
                ViewBag.GetData = "";
                var DocNo = DapperORM.DynamicQuerySingle("select ISNULL(MAX(DocNo),0)+1 as DocNo from PhotoGallery");
                TempData["DocNo"] = DocNo.DocNo;

                return View(SettingPhotoGalleryMaster);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpPost]
        public ActionResult SaveUpdate(HttpPostedFileBase ImageFile, Setting_PhotoGalleryMaster objPhotoGalleryMaster, int? PhotoGalleryMasterId)
        {
            try
            {

                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                if (objPhotoGalleryMaster.PhotoGalleryAlbumName != null)
                {
                    param.Add("@p_process", "Save");
                    param.Add("@p_DocNo", objPhotoGalleryMaster.DocNo);
                    param.Add("@p_DocDate", objPhotoGalleryMaster.DocDate);
                    param.Add("@p_PhotoGalleryAlbumName", objPhotoGalleryMaster.PhotoGalleryAlbumName);
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_PhotoGallery", param);
                    TempData["P_Id"] = param.Get<string>("@p_Id");
                }
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_process", "Save");
                param1.Add("@p_PhotoGalleryTitle", objPhotoGalleryMaster.PhotoGalleryTitle);
                param1.Add("@p_PhotoGalleryDescription", objPhotoGalleryMaster.PhotoGalleryDescription);
                param1.Add("@p_FileType", objPhotoGalleryMaster.FileType);
                if (objPhotoGalleryMaster.URLFile != null)
                {
                    param1.Add("@p_URLFile", objPhotoGalleryMaster.URLFile);
                }
                else
                {
                    param1.Add("@p_URLFile", ImageFile == null ? "" : ImageFile.FileName);
                }
                param1.Add("@p_PhotoGalleryId", TempData["P_Id"]);
                param1.Add("@p_MachineName", Dns.GetHostName().ToString());
                param1.Add("@p_CreatedUpdateBy", "Admin");
                param1.Add("@p_DocDate", objPhotoGalleryMaster.DocDate);
                param1.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param1.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param1.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result1 = DapperORM.ExecuteReturn("sp_SUD_PhotoGallery_Master", param1);
                var PId1 = param1.Get<string>("@p_Id");
                TempData["Message"] = param1.Get<string>("@p_msg");
                TempData["Icon"] = param1.Get<string>("@p_Icon");
                TempData["P_Id1"] = PId1;
                var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='PhotoGallery'");
                var GetFirstPath = GetDocPath.DocInitialPath;
                var FirstPath = GetFirstPath + TempData["P_Id1"] + "\\"; // First path plus concat folder by Id
                if (!Directory.Exists(FirstPath))
                {
                    Directory.CreateDirectory(FirstPath);
                }

                if (ImageFile != null)
                {
                    string URLFileFullPath = "";
                    URLFileFullPath = FirstPath + ImageFile.FileName; //Concat Full Path and create New full Path
                    ImageFile.SaveAs(URLFileFullPath); // This is use for Save image in folder full path
                }

                var GetList = DapperORM.DynamicQuerySingle("select PhotoGalleryId, FileType,URLFile,PhotoGalleryMasterId,PhotoGalleryTitle,PhotoGalleryDescription from PhotoGallery_Master where PhotoGalleryId=" + TempData["P_Id"] + "").ToList();
                return Json(new { data = GetList, Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@p_PhotoGalleryMasterId_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_PhotoGallery_Master", param);
                ViewBag.GetPhotoGalleryMasterList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpGet]
        public ActionResult Delete(int? PhotoGalleryMasterId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_PhotoGalleryMasterId", PhotoGalleryMasterId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_PhotoGallery_Master", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_PhotoGalleryMaster");

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        #region Download Image 
        public ActionResult DownloadFile(string filePath)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                //var GetDrivePath = (from d in dbContext.Onboarding_Mas_DocumentPath select d.DosumentPath).FirstOrDefault();
                //var fullPath = GetDrivePath + filePath;
                //byte[] fileBytes = GetFile(fullPath);
                if (filePath != "")
                {
                    System.IO.File.ReadAllBytes(filePath);
                    return File(filePath, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(filePath));
                }
                else
                {
                    return RedirectToAction("GetList", "Setting_PhotoGalleryMaster", new { Area = "Setting" });
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
            
        }
        #endregion

        public ActionResult DeleteOne(int? PhotoGalleryMasterId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_PhotoGalleryMasterId", PhotoGalleryMasterId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_PhotoGallery_Master", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                var GetList = DapperORM.DynamicQuerySingle("select PhotoGalleryId, FileType,URLFile,PhotoGalleryMasterId from PhotoGallery_Master").ToList();
                return Json(new { data = GetList, Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
    }
}