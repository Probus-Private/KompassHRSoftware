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
    public class Setting_EventAndNews_EventController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: EventAndNewsSetting/EventSetting

        #region Event Main View
        [HttpGet]
        public ActionResult Setting_EventAndNews_Event(string EventID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Event_Event EventSetting = new Event_Event();
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var GetDocNo = "Select isnull(Max(DocNo),0)+1 As DocNo from Event_Event";
                    var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    ViewBag.DocNo = DocNo;
                }
                param.Add("@query", "Select BranchId AS Id,BranchName As [Name] from Mas_Branch Where Deactivate=0 order by Name");
                var listMas_Branch = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetBranchname = listMas_Branch;
                TempData["DocDate"] = DateTime.Now;
                if (EventID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_EventID_Encrypted", EventID_Encrypted);
                    EventSetting = DapperORM.ReturnList<Event_Event>("sp_List_Event_Event", param).FirstOrDefault();

                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {
                        var GetDocNo = "Select DocNo As DocNo from Event_Event where EventID_Encrypted='" + EventID_Encrypted + "'";
                        var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                        ViewBag.DocNo = DocNo;
                    }
                    TempData["DocDate"] = EventSetting.DocDate;
                }
                
                TempData["EventDate"] = EventSetting.EventDate;
                TempData["FileName"] = EventSetting.FilePath;
                Session["SelectedFile"] = EventSetting.FilePath;
                if (EventSetting != null)
                {
                    var ToTime = EventSetting.ToTime.ToLongTimeString();
                    var FromTime = EventSetting.FromTime.ToLongTimeString();
                    TempData["ToTime"] = ToTime;
                    TempData["FromTime"] = FromTime;
                }
                return View(EventSetting);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region Event SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Event_Event ObjEvent, HttpPostedFileBase EventFile)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var CompanyId = Session["CompanyId"];
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(ObjEvent.EventID_Encrypted) ? "Save" : "Update");
                param.Add("@p_EventID", ObjEvent.EventID);
                param.Add("@p_EventID_Encrypted", ObjEvent.EventID_Encrypted);
                param.Add("@p_CmpID", CompanyId);
                param.Add("@p_BranchID", ObjEvent.BranchID);
                param.Add("@p_DocNo", ObjEvent.DocNo);
                param.Add("@p_EventTitle", ObjEvent.EventTitle);
                param.Add("@p_EventDescripation", ObjEvent.EventDescripation);
                param.Add("@p_DocDate", ObjEvent.DocDate);
                param.Add("@p_EventDate", ObjEvent.EventDate);
                param.Add("@p_FromTime", ObjEvent.FromTime);
                param.Add("@p_ToTime", ObjEvent.ToTime);
                param.Add("@p_FilePath", EventFile == null ? "" : EventFile.FileName);
                param.Add("@p_IsActive", ObjEvent.IsActive);
                //if (ObjEvent.EventID_Encrypted != null && EventFile == null)
                //{
                //    param.Add("@p_FilePath", Session["SelectedFile"]);
                //}
                //else
                //{
                //    param.Add("@p_FilePath", EventFile == null ? "" : EventFile.FileName);
                //}

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Event_Event", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                TempData["P_Id"] = param.Get<string>("@p_Id");
                if (TempData["P_Id"] != null && ObjEvent.EventID_Encrypted != null || EventFile != null)
                {
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Event'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    //var FirstPath = GetFirstPath +"Event"+"\\";
                    var FirstPath = GetFirstPath + TempData["P_Id"] + "\\";// First path plus concat folder by Id
                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }

                    if (EventFile != null)
                    {
                        string ImgEventFilePath = "";
                        ImgEventFilePath = FirstPath + EventFile.FileName; //Concat Full Path and create New full Path
                        if (Directory.Exists(FirstPath))
                        {
                            string[] files = Directory.GetFiles(FirstPath);
                            foreach (string file in files)
                            {
                                System.IO.File.Delete(file);
                            }
                        }
                        EventFile.SaveAs(ImgEventFilePath); // This is use for Save image in folder full path
                    }
                }
                return RedirectToAction("GetList", "Setting_EventAndNews_Event");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region IfValidation
        [HttpGet]
        public ActionResult IsEventExists(string EventID_Encrypted, string EventTitle, double BranchID)
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
                    param.Add("@p_EventID_Encrypted", EventID_Encrypted);
                    param.Add("@p_EventTitle", EventTitle);
                    param.Add("@p_BranchID", BranchID);

                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Event_Event", param);
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

        #region Event List View
        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_EventID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Event_Event", param);
                ViewBag.GetEeventList = data;
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
        public ActionResult DownloadFile(string EventFile)
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
                if (EventFile != null)
                {
                    System.IO.File.ReadAllBytes(EventFile);
                    return File(EventFile, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(EventFile));
                }
                else
                {
                    return RedirectToAction("GetList", "Setting_EventAndNews_Event", new { Area = "Setting" });
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
        public ActionResult Delete(string EventID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_EventID_Encrypted", EventID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Event_Event", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_EventAndNews_Event", new { area = "Setting" });
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