using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Module.Models.Module_Payroll;
using System.Net;
using System.Data;
using System.IO;
using System.Net.Mime;

namespace KompassHR.Areas.Module.Controllers.Module_Payroll
{
    public class Module_Payroll_ComplianceCalenderController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        
        #region Main View
        // GET: Module/Module_Payroll_ComplianceCalender
        public ActionResult Module_Payroll_ComplianceCalender(string ComplianceCalenderId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 874;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";

                //var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Payroll_ComplianceCalender";
                //var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                //ViewBag.DocNo = DocNo.DocNo;

                DynamicParameters p1 = new DynamicParameters();
                p1.Add("@query", $"select  ComplianceCategoryId As Id ,ComplianceCategory As Name from Payroll_ComplianceCategory Where Deactivate=0 Order By Name");
                ViewBag.ComplianceCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", p1).ToList();


                DynamicParameters p2 = new DynamicParameters();
                p2.Add("@query", $"select  EmployeeId As Id , EmployeeName As Name from Mas_Employee Where Deactivate=0 And EmployeeLeft=0  Order By Name");
                ViewBag.EmployeeList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", p2).ToList();
                
                Payroll_ComplianceCalender Payroll_ComplianceCalender = new Payroll_ComplianceCalender();

                if (ComplianceCalenderId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_ComplianceCalenderId_Encrypted", ComplianceCalenderId_Encrypted);
                    Payroll_ComplianceCalender = DapperORM.ReturnList<Payroll_ComplianceCalender>("sp_List_Payroll_ComplianceCalender", param).FirstOrDefault();
                    //   TempData["DueDate"] = Curve_Reminder.DueDate.ToString("yyyy-MM-dd");

                    TempData["Employeeid"] = Payroll_ComplianceCalender.EmployeeId;

                    //if (Payroll_ComplianceCalender.EndDate != null)
                    //{
                    //    TempData["EndDate"] = Payroll_ComplianceCalender.EndDate.Value.ToString("yyyy-MM-dd");
                    //}
                    //if (Payroll_ComplianceCalender.OneTimeDate != null)
                    //{
                    //    TempData["OneTimeDate"] = Payroll_ComplianceCalender.OneTimeDate.Value.ToString("yyyy-MM-dd");
                    //}
                    //if (Payroll_ComplianceCalender.MonthDate != null)
                    //{
                    //    TempData["MonthDate"] = Payroll_ComplianceCalender.MonthDate.Value.ToString("yyyy-MM-dd");
                    //}
                    //if (Payroll_ComplianceCalender.QuarterlyDate != null)
                    //{
                    //    TempData["QuarterlyDate"] = Payroll_ComplianceCalender.QuarterlyDate.Value.ToString("yyyy-MM-dd");
                    //}
                    if (!string.IsNullOrEmpty(Payroll_ComplianceCalender.QuarterlyDate))
                    {
                        var arr = Payroll_ComplianceCalender.QuarterlyDate.Split(',');
                        Payroll_ComplianceCalender.QuarterlyDateList = new Dictionary<string, string>
                        {
                          { "Q1", arr.Length > 0 ? arr[0] : "" },
                          { "Q2", arr.Length > 1 ? arr[1] : "" },
                          { "Q3", arr.Length > 2 ? arr[2] : "" },
                          { "Q4", arr.Length > 3 ? arr[3] : "" }
                      };
                    }

                    if (Payroll_ComplianceCalender.YearlyDate != null)
                    {
                        TempData["YearlyDate"] = Payroll_ComplianceCalender.YearlyDate.Value.ToString("yyyy-MM-dd");
                    }
                    //if (Payroll_ComplianceCalender.Time != null)
                    //{
                    //    TimeSpan time = Payroll_ComplianceCalender.Time1;
                    //    TempData["Time"] = time.ToString(@"hh\:mm");
                    //}
                    //if (Payroll_ComplianceCalender.DailyTime != null)
                    //{
                    //    TimeSpan DailyTime = Payroll_ComplianceCalender.DailyTime;
                    //    TempData["DailyTime"] = DailyTime.ToString(@"hh\:mm");
                    //}

                    //if (Payroll_ComplianceCalender.WeeklyDays1 != null)
                    //{
                    //    Payroll_ComplianceCalender.WeeklyDays = Payroll_ComplianceCalender.WeeklyDays1.Split(',').ToList();
                    //}

                   // ViewBag.DocNo = Payroll_ComplianceCalender.DocNo;
                }
                return View(Payroll_ComplianceCalender);
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
        public ActionResult SaveUpdate(Payroll_ComplianceCalender obj, HttpPostedFileBase Attachment)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                string employeeCsv = string.Join(",", obj.EmployeeIds);

                param.Add("@p_process", string.IsNullOrEmpty(obj.ComplianceCalenderId_Encrypted) ? "Save" : "Update");
                param.Add("@p_ComplianceCalenderId_Encrypted", obj.ComplianceCalenderId_Encrypted);
               // param.Add("@p_DocNo", obj.DocNo);
              //  param.Add("@p_DueDate", obj.DueDate);
                param.Add("@p_ComplianceName", obj.ComplianceName);
                param.Add("@p_ComplianceCategoryId", obj.ComplianceCategoryId);
                param.Add("@p_Remark", obj.Remark);
                param.Add("@p_Authority", employeeCsv);
                param.Add("@p_Reminder", obj.Reminder);
                param.Add("@p_Frequency", obj.Frequency);
                //   param.Add("@P_OneTimeDate", obj.OneTimeDate);
                //  param.Add("@P_Time", obj.Time);
                // param.Add("@P_Hours", obj.Hours);
                // param.Add("@P_DailyTime", obj.DailyTime);

                //string weeklyDays = null;

                //if (obj.WeeklyDays != null && obj.WeeklyDays.Any())
                //{
                //    weeklyDays = string.Join(",", obj.WeeklyDays);
                //}

                //param.Add("@P_WeeklyDays", weeklyDays);


                string quarterlyCsv = null;
                if (obj.QuarterlyDateList != null && obj.QuarterlyDateList.Any())
                {
                    var orderedQuarters = new[] { "Q1", "Q2", "Q3", "Q4" };
                    // quarterlyCsv = string.Join(",",orderedQuarters.Select(q =>obj.QuarterlyDateList.ContainsKey(q) ? obj.QuarterlyDateList[q]  : "" ) );}
                    var dates = orderedQuarters.Select(q => obj.QuarterlyDateList.ContainsKey(q) ? obj.QuarterlyDateList[q] : null).ToList();
                    quarterlyCsv = dates.All(string.IsNullOrEmpty) ? null : string.Join(",", dates);
                }
                    param.Add("@P_QuarterlyDate", quarterlyCsv);
                param.Add("@P_MonthDate", obj.MonthDate);
                param.Add("@P_YearlyDate", obj.YearlyDate);
               // param.Add("@P_EndDate", obj.EndDate);
               // param.Add("@P_Status", obj.Status);
            
                //if (obj.ComplianceCalenderId_Encrypted != null && Attachment == null)
                //{
                //    param.Add("@p_Attachment", Session["SelectedFile"]);
                //}
                //else
                //{
                //    param.Add("@p_Attachment", Attachment == null ? "" : Attachment.FileName);
                //};
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_ComplianceCalender", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                //if (TempData["P_Id"] != null && obj.ComplianceCalenderId_Encrypted != null || Attachment != null)
                //{
                //    var GetDocPath = DapperORM.QuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='ComplianceCalender'");
                //    var GetFirstPath = GetDocPath?.DocInitialPath;
                //    var FirstPath = GetFirstPath + TempData["P_Id"] + "\\";
                //    if (!Directory.Exists(FirstPath))
                //    {
                //        Directory.CreateDirectory(FirstPath);
                //    }
                //    if (Attachment != null)
                //    {
                //        string fileFullPath = "";
                //        fileFullPath = FirstPath + Attachment.FileName;
                //        Attachment.SaveAs(fileFullPath);
                //    }
                //}
                return RedirectToAction("Module_Payroll_ComplianceCalender", "Module_Payroll_ComplianceCalender");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 874;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_ComplianceCalenderId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Payroll_ComplianceCalender", param).ToList();
                ViewBag.ListDetails = data;
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
        public ActionResult Delete(string ComplianceCalenderId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_ComplianceCalenderId_Encrypted", ComplianceCalenderId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_ComplianceCalender", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Module_Payroll_ComplianceCalender");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        //#region Download Attachment
        //public ActionResult DownloadAttachment(string DownloadAttachment)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(DownloadAttachment))
        //        {
        //            TempData["Message"] = "Invalid File.";
        //            TempData["Icon"] = "error";
        //            return RedirectToAction("GetList", "Module_Payroll_ComplianceCalender");
        //        }

        //        if (DownloadAttachment == null)
        //        {
        //            TempData["Message"] = "File path information not found.";
        //            TempData["Icon"] = "error";
        //            return RedirectToAction("GetList", "Module_Payroll_ComplianceCalender");
        //        }

        //        var driveLetter = Path.GetPathRoot(DownloadAttachment);
        //        // Check if the drive exists
        //        if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
        //        {
        //            TempData["Message"] = $"Drive {driveLetter} does not exist.";
        //            TempData["Icon"] = "error";
        //            return RedirectToAction("GetList", "Module_Payroll_ComplianceCalender");
        //        }
        //        // Construct full file path
        //        var fullPath = DownloadAttachment;

        //        // Check if the file exists
        //        if (!System.IO.File.Exists(fullPath))
        //        {
        //            TempData["Message"] = "File not found on your server.";
        //            TempData["Icon"] = "error";
        //            return RedirectToAction("GetList", "Module_Payroll_ComplianceCalender");
        //        }
        //        // Return the file for download
        //        var fileName = Path.GetFileName(fullPath);
        //        return File(fullPath, MediaTypeNames.Application.Octet, fileName);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the error (you can use a logging framework here)
        //        return new HttpStatusCodeResult(500, $"Internal server error: {ex.Message}");
        //    }
        //}
        //#endregion

    }
}