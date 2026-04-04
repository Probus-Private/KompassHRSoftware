using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.ESS.Models.ESS_Conference;
using System.IO;
using System.Net.Mime;

namespace KompassHR.Areas.ESS.Controllers.ESS_Conference
{
    public class ESS_Curve_ReminderController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region ESS_Curve_Reminder Main View 
        [HttpGet]
        public ActionResult ESS_Curve_Reminder(string ReminderID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 836;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";

                var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Curve_Reminder";
                var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                ViewBag.DocNo = DocNo.DocNo;

                Curve_Reminder Curve_Reminder = new Curve_Reminder();
                if (ReminderID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_ReminderID_Encrypted", ReminderID_Encrypted);
                    Curve_Reminder = DapperORM.ReturnList<Curve_Reminder>("sp_List_Reminder", param).FirstOrDefault();
                    //   TempData["DueDate"] = Curve_Reminder.DueDate.ToString("yyyy-MM-dd");
                    if (Curve_Reminder.EndDate != null)
                    {
                        TempData["EndDate"] = Curve_Reminder.EndDate.Value.ToString("yyyy-MM-dd");
                    }
                    if (Curve_Reminder.OneTimeDate != null)
                    {
                        TempData["OneTimeDate"] = Curve_Reminder.OneTimeDate.Value.ToString("yyyy-MM-dd");
                    }
                    if (Curve_Reminder.MonthDate != null)
                    {
                        TempData["MonthDate"] = Curve_Reminder.MonthDate.Value.ToString("yyyy-MM-dd");

                    }
                    if (Curve_Reminder.QuarterlyDate != null)
                    {
                        TempData["QuarterlyDate"] = Curve_Reminder.QuarterlyDate.Value.ToString("yyyy-MM-dd");
                    }
                    if (Curve_Reminder.YearlyDate != null)
                    {
                        TempData["YearlyDate"] = Curve_Reminder.YearlyDate.Value.ToString("yyyy-MM-dd");
                    }
                    if (Curve_Reminder.Time != null)
                    {
                        TimeSpan time = Curve_Reminder.Time1;
                        TempData["Time"] = time.ToString(@"hh\:mm");
                    }
                    if (Curve_Reminder.DailyTime != null)
                    {
                        TimeSpan DailyTime = Curve_Reminder.DailyTime;
                        TempData["DailyTime"] = DailyTime.ToString(@"hh\:mm");
                    }

                    if (Curve_Reminder.WeeklyDays1 != null)
                    {
                        Curve_Reminder.WeeklyDays =Curve_Reminder.WeeklyDays1.Split(',').ToList();
                    }

                    ViewBag.DocNo = Curve_Reminder.DocNo;
                }
                return View(Curve_Reminder);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList Main View 

        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 836;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_ReminderID_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Reminder", param).ToList();
                ViewBag.GetReminderList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

       // #region IsReminderExists
       //public ActionResult IsReminderExists(string ReminderID_Encrypted, int DocNo,DateTime DueDate, string ReminderList, double AlertBeforeDays)
       // {
       //     try
       //     {
       //         using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
       //         {
       //             param.Add("@p_process", "IsValidation");
       //             param.Add("@p_ReminderID_Encrypted", ReminderID_Encrypted);
       //             param.Add("@p_DocNo", DocNo);
       //             param.Add("@p_DueDate", DueDate);
       //             param.Add("@p_ReminderList", ReminderList);
       //             param.Add("@p_AlertBeforeDays", AlertBeforeDays);
       //             param.Add("@p_MachineName", Dns.GetHostName().ToString());
       //             param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
       //             param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
       //             param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
       //             var Result = DapperORM.ExecuteReturn("sp_SUD_Reminder", param);
       //             var Message = param.Get<string>("@p_msg");
       //             var Icon = param.Get<string>("@p_Icon");
       //             if (Message != "")
       //             {
       //                 return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
       //             }
       //             else
       //             {
       //                 return Json(true, JsonRequestBehavior.AllowGet);
       //             }
       //         }

       //     }
       //     catch (Exception ex)
       //     {
       //         Session["GetErrorMessage"] = ex.Message;
       //         return RedirectToAction("ErrorPage", "Login");
       //     }
       // }
       // #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Curve_Reminder Curve_Reminder, HttpPostedFileBase Attachment)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
               
                param.Add("@p_process", string.IsNullOrEmpty(Curve_Reminder.ReminderID_Encrypted) ? "Save" : "Update");
                param.Add("@p_ReminderID_Encrypted", Curve_Reminder.ReminderID_Encrypted);
                param.Add("@p_DocNo", Curve_Reminder.DocNo);
                param.Add("@p_DueDate", Curve_Reminder.DueDate);
                param.Add("@p_ReminderList", Curve_Reminder.ReminderList);
                param.Add("@p_AlertBeforeDays", Curve_Reminder.AlertBeforeDays);
                param.Add("@p_Frequency", Curve_Reminder.Frequency);
                param.Add("@P_OneTimeDate", Curve_Reminder.OneTimeDate);
                param.Add("@P_Time", Curve_Reminder.Time);
                param.Add("@P_Hours", Curve_Reminder.Hours);
                param.Add("@P_DailyTime", Curve_Reminder.DailyTime);
                // param.Add("@P_WeeklyDays", Curve_Reminder.WeeklyDays);
                string weeklyDays = null;

                if (Curve_Reminder.WeeklyDays != null && Curve_Reminder.WeeklyDays.Any())
                {
                    weeklyDays = string.Join(",", Curve_Reminder.WeeklyDays);
                }

                param.Add("@P_WeeklyDays", weeklyDays);
              
                param.Add("@P_MonthDate", Curve_Reminder.MonthDate);
                param.Add("@P_QuarterlyDate", Curve_Reminder.QuarterlyDate);
                param.Add("@P_YearlyDate", Curve_Reminder.YearlyDate);
                param.Add("@P_EndDate", Curve_Reminder.EndDate);
                param.Add("@P_Status", Curve_Reminder.Status);
                param.Add("@P_IsActive ", Curve_Reminder.IsActive);
                param.Add("@p_EmployeeId ", Session["EmployeeId"]);
                if (Curve_Reminder.ReminderID_Encrypted != null && Attachment == null)
                {
                    param.Add("@p_Attachment", Session["SelectedFile"]);
                }
                else
                {
                    param.Add("@p_Attachment", Attachment == null ? "" : Attachment.FileName);
                };
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                var Result = DapperORM.ExecuteReturn("sp_SUD_Reminder", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                if (TempData["P_Id"] != null && Curve_Reminder.ReminderID_Encrypted != null || Attachment != null)
                {
                    var GetDocPath = DapperORM.QuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Reminder'");
                    var GetFirstPath = GetDocPath?.DocInitialPath;
                    var FirstPath = GetFirstPath + TempData["P_Id"] + "\\";
                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }
                    if (Attachment != null)
                    {
                        string fileFullPath = "";
                        fileFullPath = FirstPath + Attachment.FileName;
                        Attachment.SaveAs(fileFullPath);
                    }
                }
                return RedirectToAction("ESS_Curve_Reminder", "ESS_Curve_Reminder");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(int? ReminderID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_ReminderID", ReminderID);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Reminder", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_Curve_Reminder");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Download Attachment
        public ActionResult DownloadAttachment(string DownloadAttachment)
        {
            try
            {
                if (string.IsNullOrEmpty(DownloadAttachment))
                {
                    TempData["Message"] = "Invalid File.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetList", "ESS_Curve_Reminder");
                }

                if (DownloadAttachment == null)
                {
                    TempData["Message"] = "File path information not found.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetList", "ESS_Curve_Reminder");
                }

                var driveLetter = Path.GetPathRoot(DownloadAttachment);
                // Check if the drive exists
                if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
                {
                    TempData["Message"] = $"Drive {driveLetter} does not exist.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetList", "ESS_Curve_Reminder");
                }
                // Construct full file path
                var fullPath = DownloadAttachment;

                // Check if the file exists
                if (!System.IO.File.Exists(fullPath))
                {
                    TempData["Message"] = "File not found on your server.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetList", "ESS_Curve_Reminder");
                }
                // Return the file for download
                var fileName = Path.GetFileName(fullPath);
                return File(fullPath, MediaTypeNames.Application.Octet, fileName);
            }
            catch (Exception ex)
            {
                // Log the error (you can use a logging framework here)
                return new HttpStatusCodeResult(500, $"Internal server error: {ex.Message}");
            }
        }
        #endregion
    }
}