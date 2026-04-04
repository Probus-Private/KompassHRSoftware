using Dapper;
using KompassHR.Areas.Setting.Models.Setting_EventAndNews;
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

namespace KompassHR.Areas.Setting.Controllers.Setting_EventAndNews
{
    public class Setting_EventAndNews_AnnouncementController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: EventAndNewsSetting/AnnouncementSetting

        #region Announcement Main View
        [HttpGet]
        public ActionResult Setting_EventAndNews_Announcement(string AnnouncementID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Event_Announcement AnnouncementSetting = new Event_Announcement();
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var GetDocNo = "Select isnull(Max(DocNo),0)+1 As DocNo from Event_Announcement";
                    var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    ViewBag.DocNo = DocNo;
                }

                param.Add("@query", "Select BranchId AS Id,BranchName As [Name] from Mas_Branch Where Deactivate=0 order by Name");
                var listMas_Branch = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetBranchname = listMas_Branch;
                TempData["DocDate"] = DateTime.Now;
                if (AnnouncementID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_AnnouncementID_Encrypted", AnnouncementID_Encrypted);
                    AnnouncementSetting = DapperORM.ReturnList<Event_Announcement>("sp_List_Event_Announcement", param).FirstOrDefault();

                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {
                        var GetDocNo = "Select DocNo As DocNo from Event_Announcement where AnnouncementID_Encrypted='" + AnnouncementID_Encrypted + "'";
                        var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                        ViewBag.DocNo = DocNo;
                    }
                    TempData["DocDate"] = AnnouncementSetting.DocDate;
                }
                
                TempData["FromDate"] = AnnouncementSetting.FromDate;
                TempData["ToDate"] = AnnouncementSetting.ToDate;
                TempData["FileName"] = AnnouncementSetting.FilePath;
                Session["SelectedFile"] = AnnouncementSetting.FilePath;
                return View(AnnouncementSetting);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region IsValidation
        [HttpGet]
        public ActionResult IsAnnouncementExists(string AnnouncementID_Encrypted, string AnnouncementTitle, double BranchID)
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
                    param.Add("@p_AnnouncementID_Encrypted", AnnouncementID_Encrypted);
                    param.Add("@p_AnnouncementTitle", AnnouncementTitle);
                    param.Add("@p_BranchID", BranchID);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Event_Announcement", param);
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

        #region Announcement SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Event_Announcement ObjAnnouncement, HttpPostedFileBase AnnouncementFile)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var CompanyId = Session["CompanyId"];
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(ObjAnnouncement.AnnouncementID_Encrypted) ? "Save" : "Update");
                param.Add("@p_AnnouncementID", ObjAnnouncement.AnnouncementID);
                param.Add("@p_AnnouncementID_Encrypted", ObjAnnouncement.AnnouncementID_Encrypted);
                param.Add("@p_CmpID", CompanyId);
                param.Add("@p_BranchID", ObjAnnouncement.BranchID);
                param.Add("@p_DocNo", ObjAnnouncement.DocNo);
                param.Add("@p_AnnouncementTitle", ObjAnnouncement.AnnouncementTitle);
                param.Add("@p_AnnouncementDescripition", ObjAnnouncement.AnnouncementDescripition);
                param.Add("@p_DocDate", ObjAnnouncement.DocDate);
                param.Add("@p_AnnounceEmployeeID", EmployeeId);
                param.Add("@p_FromDate", ObjAnnouncement.FromDate);
                param.Add("@p_ToDate", ObjAnnouncement.ToDate);
                //param.Add("@p_FilePath", AnnouncementFile == null ? "" : AnnouncementFile.FileName);
                param.Add("@p_IsActive", ObjAnnouncement.IsActive);
                if (ObjAnnouncement.AnnouncementID_Encrypted != null && AnnouncementFile == null)
                {
                    param.Add("@p_FilePath", Session["SelectedFile"]);
                }
                else
                {
                    param.Add("@p_FilePath", AnnouncementFile == null ? "" : AnnouncementFile.FileName);
                }

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Event_Announcement", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                if (TempData["P_Id"] != null && ObjAnnouncement.AnnouncementID_Encrypted != null || AnnouncementFile != null)
                {
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Announcement'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    //var FirstPath = GetFirstPath +"Event"+"\\";
                    var FirstPath = GetFirstPath + TempData["P_Id"] + "\\";// First path plus concat folder by Id
                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }

                    if (AnnouncementFile != null)
                    {
                        string ImgEventFilePath = "";
                        ImgEventFilePath = FirstPath + AnnouncementFile.FileName; //Concat Full Path and create New full Path
                        if (Directory.Exists(FirstPath))
                        {
                            string[] files = Directory.GetFiles(FirstPath);
                            foreach (string file in files)
                            {
                                System.IO.File.Delete(file);
                            }
                        }
                        AnnouncementFile.SaveAs(ImgEventFilePath); // This is use for Save image in folder full path
                    }
                }
                return RedirectToAction("GetList", "Setting_EventAndNews_Announcement");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Announcement List View 
        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_AnnouncementID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Event_Announcement", param);
                ViewBag.GetAnnouncementList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region Download Image 
        public ActionResult DownloadFile(string FilePath)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
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
                    return RedirectToAction("GetList", "Setting_EventAndNews_Announcement", new { Area = "Setting" });
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
            
        }

        #endregion

        #region Delete Event
        public ActionResult Delete(string AnnouncementID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_AnnouncementID_Encrypted", AnnouncementID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Event_Announcement", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_EventAndNews_Announcement", new { Area = "Setting" });
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