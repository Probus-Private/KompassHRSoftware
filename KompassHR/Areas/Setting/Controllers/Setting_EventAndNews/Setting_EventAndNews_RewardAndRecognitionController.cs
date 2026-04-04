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
    public class Setting_EventAndNews_RewardAndRecognitionController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: EventAndNewsSetting/RewardAndRecognitionSetting
        #region Reward Main View
        [HttpGet]
        public ActionResult Setting_EventAndNews_RewardAndRecognition(string RewardID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Event_Reward RewardSetting = new Event_Reward();
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var GetDocNo = "Select isnull(Max(DocNo),0)+1 As DocNo from Event_Reward";
                    var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    ViewBag.DocNo = DocNo;
                }

                param.Add("@query", "Select BranchId AS Id,BranchName As [Name] from Mas_Branch Where Deactivate=0 order by Name");
                var listMas_Branch = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetBranchname = listMas_Branch;

                param.Add("@query", "Select EmployeeId AS Id,EmployeeName As [Name] from Mas_Employee Where Deactivate=0 order by Name");
                var listMas_Employee = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetEmployeeName = listMas_Employee;

                if (RewardID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_RewardID_Encrypted", RewardID_Encrypted);
                    RewardSetting = DapperORM.ReturnList<Event_Reward>("sp_List_Event_Reward", param).FirstOrDefault();

                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {
                        var GetDocNo = "Select DocNo As DocNo from Event_Reward where RewardID_Encrypted='" + RewardID_Encrypted + "'";
                        var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                        ViewBag.DocNo = DocNo;
                    }
                }
                TempData["DocDate"] = RewardSetting.DocDate;
                TempData["RewardFromDate"] = RewardSetting.RewardFromDate;
                TempData["RewardToDate"] = RewardSetting.RewardToDate;
                TempData["FileName"] = RewardSetting.FilePath;
                Session["SelectedFile"] = RewardSetting.FilePath;
                return View(RewardSetting);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Reward SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Event_Reward ObjReward, HttpPostedFileBase RewardFile)
        {
            try
            {
                var CompanyId = Session["CompanyId"];
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(ObjReward.RewardID_Encrypted) ? "Save" : "Update");
                param.Add("@p_RewardID", ObjReward.RewardID);
                param.Add("@p_RewardID_Encrypted", ObjReward.RewardID_Encrypted);
                param.Add("@p_CmpID", CompanyId);
                param.Add("@p_BranchID", ObjReward.BranchID);
                param.Add("@p_DocNo", ObjReward.DocNo);
                param.Add("@p_RewardTitle", ObjReward.RewardTitle);
                param.Add("@p_RewardDescripition", ObjReward.RewardDescripition);
                param.Add("@p_DocDate", ObjReward.DocDate);
                param.Add("@p_RewardFromDate", ObjReward.RewardFromDate);
                param.Add("@p_RewardToDate", ObjReward.RewardToDate);
                param.Add("@p_RewardEmployeeID", ObjReward.RewardEmployeeID);
                //param.Add("@p_FilePath", RewardFile == null ? "" : RewardFile.FileName);
                param.Add("@p_IsActive", ObjReward.IsActive);
                if (ObjReward.RewardID_Encrypted != null && RewardFile == null)
                {
                    param.Add("@p_FilePath", Session["SelectedFile"]);
                }
                else
                {
                    param.Add("@p_FilePath", RewardFile == null ? "" : RewardFile.FileName);
                }

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Event_Reward", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                TempData["P_Id"] = param.Get<string>("@p_Id");
                if (TempData["P_Id"] != null && ObjReward.RewardID_Encrypted != null || RewardFile != null)
                {
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='RewardAndRecognition'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    //var FirstPath = GetFirstPath +"Event"+"\\";
                    var FirstPath = GetFirstPath + TempData["P_Id"] + "\\";// First path plus concat folder by Id
                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }

                    if (RewardFile != null)
                    {
                        string ImgEventFilePath = "";
                        ImgEventFilePath = FirstPath + RewardFile.FileName; //Concat Full Path and create New full Path
                        if (Directory.Exists(FirstPath))
                        {
                            string[] files = Directory.GetFiles(FirstPath);
                            foreach (string file in files)
                            {
                                System.IO.File.Delete(file);
                            }
                        }
                        RewardFile.SaveAs(ImgEventFilePath); // This is use for Save image in folder full path
                    }
                }
                return RedirectToAction("GetList", "Setting_EventAndNews_RewardAndRecognition");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsVerification
        public ActionResult IsRewardExists(string RewardTitle, double BranchID, double RewardEmployeeID, string RewardID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var EmployeeId = Session["EmployeeId"];
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_RewardTitle", RewardTitle);
                    param.Add("@p_RewardID_Encrypted", RewardID_Encrypted);
                    param.Add("@p_BranchID", BranchID);
                    param.Add("@p_RewardEmployeeID", EmployeeId);

                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("[sp_SUD_Event_Reward]", param);
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

        #region Reward List View
        [HttpGet]
        public ActionResult GetList()
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_RewardID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Event_Reward", param);
                ViewBag.GetRewardList = data;
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
        public ActionResult DownloadFile(string RewardFile)
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
                if (RewardFile != null)
                {
                    System.IO.File.ReadAllBytes(RewardFile);
                    return File(RewardFile, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(RewardFile));
                }
                else
                {
                    return RedirectToAction("GetList", "Setting_EventAndNews_RewardAndRecognition", new { Area = "Setting" });
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
        public ActionResult Delete(string RewardID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_RewardID_Encrypted", RewardID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Event_Reward", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_EventAndNews_RewardAndRecognition", new { Area = "Setting" });
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