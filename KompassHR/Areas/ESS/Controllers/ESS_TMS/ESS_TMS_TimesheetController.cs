using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TMS;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_TMS
{
    public class ESS_TMS_TimesheetController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();

        #region TimeSheet Main View 
        [HttpGet]
        public ActionResult ESS_TMS_Timesheet(TMS_Timesheet TMSTimesheet)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 196;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var GetDocNo = DapperORM.QuerySingle("SELECT ISNULL(MAX(CAST(DocNo AS INT)), 0) + 1 AS DocNo FROM TMS_Timesheet WHERE Deactivate = 0");
                    ViewBag.DocNo = GetDocNo?.DocNo;

                    var DocMinMaxDate = DapperORM.QuerySingle("Select MinDate,MaxDate from tbl_Set_FutureDate where FormName='WorkOnTask'");
                    TempData["DocMinDate"] = DocMinMaxDate?.MinDate ?? 31;
                    TempData["DocMaxDate"] = DocMinMaxDate?.MaxDate ?? 0;
                }
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "Select ClientID AS Id,ClientName As [Name] from TMS_Client Where Deactivate=0  And IsActive=1 Order by ClientName");
                var listMas_Client = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", param1).ToList();
                ViewBag.GetClientName = listMas_Client;
                ViewBag.GetProjectName = "";
                ViewBag.GetModuleName = "";
                ViewBag.GetResponsiblePersonName = "";
                //DynamicParameters paramTaskCategory = new DynamicParameters();
                //paramTaskCategory.Add("@query", " SELECT TaskCategoryId AS Id, TaskCategoryName AS Name FROM TMS_TaskCategory WHERE Deactivate = 0 ORDER BY Name");
                //var TaskCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramTaskCategory).ToList();
                //ViewBag.TaskCategory = TaskCategory;
                ViewBag.TaskCategory = "";
                ViewBag.TaskSubCategory = "";
                if (TMSTimesheet.TimeSheetID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    DynamicParameters TimeSheetTMS = new DynamicParameters();
                    TimeSheetTMS.Add("@P_Qry", "and TimesheetEmployeeID='" + Session["EmployeeId"] + "' And TimeSheetID_Encrypted='" + TMSTimesheet.TimeSheetID_Encrypted + "'");
                    TMSTimesheet = DapperORM.ReturnList<TMS_Timesheet>("sp_List_TMS_Timesheet", TimeSheetTMS).FirstOrDefault();

                    DynamicParameters paramTaskSubCategory = new DynamicParameters();
                    paramTaskSubCategory.Add("@query", "SELECT TaskSubCategoryId as Id, TaskSubCategoryName as Name FROM TMS_TaskSubCategory WHERE Deactivate = 0 and TaskCategoryId ='" + TMSTimesheet.TaskCategoryId + "' ORDER BY Name");
                    ViewBag.TaskSubCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramTaskSubCategory).ToList();
                    if (TMSTimesheet.ClientID > 0)
                    {
                        if (TMSTimesheet.ProjectID != null)
                        {
                            DynamicParameters param = new DynamicParameters();
                            param.Add("@p_ProjectId", TMSTimesheet.ProjectID);
                            var data1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetModuleDropdown", param).ToList();
                            ViewBag.GetModuleName = data1;
                        }
                        else
                        {
                            ViewBag.GetModuleName = "";
                        }
                        ViewBag.AddUpdateTitle = "Update";
                        DynamicParameters paramProject = new DynamicParameters();
                        paramProject.Add("@p_employeeid", Session["EmployeeId"]);
                        paramProject.Add("@p_ClientId", TMSTimesheet.ClientID);
                        var data = DapperORM.ReturnList<AllDropDownBind>("sp_Get_TMS_ProjectDropdown", paramProject).ToList();
                        ViewBag.GetProjectName = data;
                    }
                    else
                    {
                        ViewBag.GetProjectName = "";
                    }
                    TempData["DocDate"] = TMSTimesheet.DocDate;
                    var SetdocNo = TMSTimesheet.DocNo;
                    ViewBag.DocNo = SetdocNo;
                }
                return View(TMSTimesheet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region
        public ActionResult GetListTimeSheet()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 196;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters TimeSheetTMS = new DynamicParameters();
                TimeSheetTMS.Add("@P_Qry", "and TimesheetEmployeeID='" + Session["EmployeeId"] + "' order by TimeSheetID desc");
                var TimesheetPending = DapperORM.DynamicList("sp_List_TMS_Timesheet", TimeSheetTMS);
                ViewBag.Timesheetlist = TimesheetPending;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Timesheet SaveUpdate
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveUpdate(TMS_Timesheet Timesheet, HttpPostedFileBase AssignFilePath, string TaskDescription)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 196;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var CompanyId = Session["CompanyId"];
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(Timesheet.TimeSheetID_Encrypted) ? "Save" : "Update");
                param.Add("@p_TimeSheetID", Timesheet.TimeSheetID);
                param.Add("@p_TimeSheetID_Encrypted", Timesheet.TimeSheetID_Encrypted);
                param.Add("@p_CmpID", CompanyId);
                param.Add("@p_TimesheetEmployeeID", EmployeeId);
                param.Add("@p_DocNo", Timesheet.DocNo);
                param.Add("@p_DocDate", Timesheet.DocDate);
                param.Add("@p_ProjectID", Timesheet.ProjectID);
                param.Add("@p_ModuleId", Timesheet.ModuleId);
                param.Add("@p_ClientID", Timesheet.ClientID);
                param.Add("@p_TaskCategoryId", Timesheet.TaskCategoryId);
                param.Add("@p_TaskSubCategoryId", Timesheet.TaskSubCategoryId);
                param.Add("@p_TaskTitle", Timesheet.TaskTitle);
                param.Add("@p_TaskDescription", TaskDescription);
                param.Add("@p_Time", Timesheet.Time);
                param.Add("@p_ResponsiblePerson", Timesheet.ResponsiblePerson);
                // param.Add("@p_ToTime", Timesheet.ToTime);
                param.Add("@p_TaskStatus", Timesheet.TaskStatus);
                param.Add("@p_CompletePercentage", Timesheet.CompletePercentage);
                param.Add("@p_AssignFilePath", AssignFilePath == null ? "" : AssignFilePath.FileName);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", "Admin");
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_TMS_Timesheet", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                if (TempData["P_Id"] != null && Timesheet.TimeSheetID_Encrypted != null || AssignFilePath != null)
                {
                    var GetDocPath = DapperORM.QuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='WorkOnTask'");
                    var GetFirstPath = GetDocPath?.DocInitialPath;
                    var FirstPath = GetFirstPath + TempData["P_Id"] + "\\";
                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }
                    if (AssignFilePath != null)
                    {
                        string fileFullPath = "";
                        fileFullPath = FirstPath + AssignFilePath.FileName;
                        AssignFilePath.SaveAs(fileFullPath);
                    }
                }
                return RedirectToAction("ESS_TMS_Timesheet", "ESS_TMS_Timesheet");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region Delete Timesheet
        public ActionResult Delete(string TimeSheetID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 196;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_TimeSheetID_Encrypted", TimeSheetID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", "Admin");
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_TMS_Timesheet", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetListTimeSheet", "ESS_TMS_Timesheet");
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
                    return RedirectToAction("GetListTimeSheet", "ESS_TMS_Timesheet");
                }

                if (DownloadAttachment == null)
                {
                    TempData["Message"] = "File path information not found.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetListTimeSheet", "ESS_TMS_Timesheet");
                }

                var driveLetter = Path.GetPathRoot(DownloadAttachment);
                // Check if the drive exists
                if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
                {
                    TempData["Message"] = $"Drive {driveLetter} does not exist.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetListTimeSheet", "ESS_TMS_Timesheet");
                }
                // Construct full file path
                var fullPath = DownloadAttachment;

                // Check if the file exists
                if (!System.IO.File.Exists(fullPath))
                {
                    TempData["Message"] = "File not found on your server.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetListTimeSheet", "ESS_TMS_Timesheet");
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

        #region GetModule
        [HttpGet]
        public ActionResult GetModule(int ProjectId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ProjectId", ProjectId);
                var module = DapperORM.ReturnList<AllDropDownBind>("sp_GetModuleDropdown", param).ToList();

                DynamicParameters paramTaskCategory = new DynamicParameters();
                paramTaskCategory.Add("@query", "Select TMS_TaskCategory.TaskCategoryId as Id, TMS_TaskCategory.TaskCategoryName as Name from TMS_TaskCategory Inner Join TMS_TaskCate_ProjModuleMapping on TMS_TaskCate_ProjModuleMapping.TaskCategoryId = TMS_TaskCategory.TaskCategoryId and IsActive = 1 where TMS_TaskCategory.deactivate = 0 and TMS_TaskCate_ProjModuleMapping.ProjectId = '" + ProjectId + "' ORDER BY Name");
                var taskCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramTaskCategory).ToList();

                return Json(new { module = module, taskCategory= taskCategory}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetProject
        [HttpGet]
        public ActionResult GetProject(int Clientd)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_ClientId", Clientd);

                var Project = DapperORM.ReturnList<AllDropDownBind>("sp_Get_TMS_ProjectDropdown", param).ToList();

               //  var ResponsiblePerson = DapperORM.DynamicQuerySingle(@"select ClientDetailsId Id ,ResponsiblePerson as Name from TMS_ClientDetails where ClientId='" + Clientd + "'");

                DynamicParameters paramResponsiblePerson = new DynamicParameters();
                paramResponsiblePerson.Add("@query", "select ClientDetailsId Id ,ResponsiblePerson as Name from TMS_ClientDetails where ClientId='" + Clientd + "' ORDER BY Name");
                var ResponsiblePerson = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramResponsiblePerson).ToList();


                return Json(new { Project = Project, ResponsiblePerson= ResponsiblePerson }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetTaskCategory
        [HttpGet]
        public ActionResult GetTaskCategory(int? ProjectId, int ModuleId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramTaskCategory = new DynamicParameters();
                paramTaskCategory.Add("@query", "Select TMS_TaskCategory.TaskCategoryId as Id, TMS_TaskCategory.TaskCategoryName as Name from TMS_TaskCategory Inner Join TMS_TaskCate_ProjModuleMapping on TMS_TaskCate_ProjModuleMapping.TaskCategoryId = TMS_TaskCategory.TaskCategoryId and IsActive = 1 where TMS_TaskCategory.deactivate = 0 and TMS_TaskCate_ProjModuleMapping.ProjectId='" + ProjectId + "' and TMS_TaskCate_ProjModuleMapping.ModuleId='" + ModuleId + "' order by Name");
                var TaskCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramTaskCategory).ToList();
                return Json(new { TaskCategory = TaskCategory }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetTaskSubCategory
        [HttpGet]
        public ActionResult GetTaskSubCategory(int TaskCategoryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramTaskSubCategory = new DynamicParameters();
                paramTaskSubCategory.Add("@query", "SELECT TaskSubCategoryId as Id, TaskSubCategoryName as Name FROM TMS_TaskSubCategory WHERE Deactivate = 0 and TaskCategoryId ='" + TaskCategoryId + "' ORDER BY Name");
                var TaskSubCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramTaskSubCategory).ToList();
                return Json(new { TaskSubCategory = TaskSubCategory }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region
        public ActionResult SendToApproval(double? TimeSheetID, double? TimesheetEmployeeID)
        {
            StringBuilder strBuilder = new StringBuilder();
            var createdBy = Session["EmployeeName"]?.ToString().Replace("'", "''");
            var machineName = Dns.GetHostName().Replace("'", "''");

            var GetId = "select Id from Tool_Module where ModuleName='TMS' and Deactivate=0";
            var Id = DapperORM.DynamicQuerySingle(GetId);

            strBuilder.AppendLine(
                "UPDATE dbo.TMS_Timesheet SET " +
                "IsSendForApproval = 1, " +
                "ModifiedBy = '" + createdBy + "', " +
                "ModifiedDate = GETDATE(), " +
                "MachineName = '" + machineName + "' " +
                "WHERE TimeSheetID = " + TimeSheetID + " " +
                "AND TimesheetEmployeeID = " + TimesheetEmployeeID + ";"
            );

            strBuilder.AppendLine(
                "INSERT INTO Tra_Approval " +
                "(Deactivate, UseBy, CreatedBy, CreatedDate, MachineName, Origin, " +
                "TraApproval_DocId, TraApproval_CompId, TraApproval_BranchId, TraApproval_EmployeeId, " +
                "TraApproval_ApproverEmployeeId, ApproverLevel, IsMandatory, Status, IsActive, TraApproval_ModuleId) " +
                "SELECT " +
                "0, 0, '" + createdBy + "', GETDATE(), '" + machineName + "', 'TMS_Timesheet', " +
                TimeSheetID + ", '" + Session["CompanyId"] + "', '" + Session["BranchId"] + "', " + TimesheetEmployeeID + ", " +
                "ReportingManager1, ApproverLevel, IsCompulsory, 'Pending', 1, 18 " +
                "FROM Mas_Employee_Reporting " +
                "WHERE Deactivate = 0 " +
                "AND ReportingEmployeeID = " + TimesheetEmployeeID + " " +
                "AND ReportingModuleID = "+ Id.Id + ";"
            );

            string abc = "";
            if (objcon.SaveStringBuilder(strBuilder, out abc))
            {
                TempData["Message"] = "Record sent for approval successfully.";
                TempData["Icon"] = "success";
            }
            return RedirectToAction("GetListTimeSheet", "ESS_TMS_Timesheet");
        }
        #endregion
    }
}