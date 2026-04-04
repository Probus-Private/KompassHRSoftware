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
    public class Setting_EventAndNews_NewsController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: EventAndNewsSetting/NewsSetting
        public ActionResult Setting_EventAndNews_News(string NewsID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Event_News EventNews = new Event_News();
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Event_News";
                    var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    ViewBag.DocNo = DocNo;
                }
                TempData["DocDate"] = DateTime.Now;
                if (NewsID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_NewsID_Encrypted", NewsID_Encrypted);
                    EventNews = DapperORM.ReturnList<Event_News>("sp_List_Event_News", param).FirstOrDefault();
                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {
                        var GetDocNo = "Select DocNo from Event_News where NewsID_Encrypted='" + NewsID_Encrypted + "'";
                        var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                        ViewBag.DocNo = DocNo;
                    }
                    TempData["DocDate"] = EventNews.DocDate;
                }
                TempData["FromDate"] = EventNews.NewsFromDate;
                TempData["ToDate"] = EventNews.NewsToDate;
                TempData["FileName"] = EventNews.FilePath;
                Session["SelectedFile"] = EventNews.FilePath;
                return View(EventNews);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


        public ActionResult IsNewsExists(string NewsTitle, string NewsIDEncrypted)
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
                    param.Add("@p_NewsTitle", NewsTitle);
                    param.Add("@p_NewsID_Encrypted", NewsIDEncrypted);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Event_News", param);
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

        public ActionResult SaveUpdate(Event_News EventNews, HttpPostedFileBase FilePath)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(EventNews.NewsID_Encrypted) ? "Save" : "Update");
                param.Add("@p_NewsID", EventNews.NewsID);
                param.Add("@p_NewsID_Encrypted", EventNews.NewsID_Encrypted);
                param.Add("@p_Docdate", EventNews.DocDate);
                param.Add("@p_DocNo", EventNews.DocNo);
                param.Add("@p_NewsTitle", EventNews.NewsTitle);
                param.Add("@p_NewsDescripation", EventNews.NewsDescripation);
                param.Add("@p_NewsFromDate", EventNews.NewsFromDate);
                param.Add("@p_NewsToDate", EventNews.NewsToDate);
                param.Add("@p_IsActive", EventNews.IsActive);
                if (EventNews.NewsID_Encrypted != null && FilePath == null)
                {
                    param.Add("@p_FilePath", Session["SelectedFile"]);
                }
                else
                {
                    param.Add("@p_FilePath", FilePath == null ? "" : FilePath.FileName);
                }
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Event_News", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                TempData["P_Id"] = param.Get<string>("@p_Id");
                if (TempData["P_Id"] != null && EventNews.NewsID_Encrypted != null || FilePath != null)
                {
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='News'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    var FirstPath = GetFirstPath + TempData["P_Id"] + "\\"; // First path plus concat folder by Id
                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }
                    if (FilePath != null)
                    {
                        string ImgTotalKMFullPath = "";
                        ImgTotalKMFullPath = FirstPath + FilePath.FileName; //Concat Full Path and create New full Path
                        if (Directory.Exists(FirstPath))
                        {
                            string[] files = Directory.GetFiles(FirstPath);
                            foreach (string file in files)
                            {
                                System.IO.File.Delete(file);
                            }
                        }
                        FilePath.SaveAs(ImgTotalKMFullPath); // This is use for Save image in folder full path
                    }
                }
                return RedirectToAction("GetList", "Setting_EventAndNews_News");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_NewsID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Event_News", param);
                ViewBag.NewsList = data;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }


        public ActionResult DownloadFile(string filePath)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (filePath != null)
                {
                    System.IO.File.ReadAllBytes(filePath);
                    return File(filePath, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(filePath));


                }
                else
                {
                    return RedirectToAction("GetList", "Setting_EventAndNews_News", new { Area = "Setting" });
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
           
        }

        public ActionResult Delete(string NewsID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_NewsID_Encrypted", NewsID_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Event_News", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_EventAndNews_News");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
    }
}