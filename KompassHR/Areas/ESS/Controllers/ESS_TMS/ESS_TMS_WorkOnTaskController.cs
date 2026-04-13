using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TMS;
using KompassHR.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;


namespace KompassHR.Areas.ESS.Controllers.ESS_TMS
{
    public class ESS_TMS_WorkOnTaskController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: TMSSetting/WorkOnTask
        public ActionResult ESS_TMS_WorkOnTask(string TaskAssignID, string TaskTittle, string StartDate, string DocNo, string WorkOnTaskID_Encrypted, string AssignerName, string DateOrTime, string AssignTime, string Priority, string ProjectName, string ModuleName, string Clientname, string TaskCategoryName, string TaskSubCategoryName)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 197;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                TMS_WorkOnTask TMSWorkOnTask = new TMS_WorkOnTask();
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    //var GetDocNo = DapperORM.DynamicQuerySingle("Select Isnull(Max(DocNo),0)+1 As DocNo from TMS_WorkOnTask where Deactivate=0");
                    //var SetDocNo = GetDocNo.DocNo;
                    ViewBag.DocNo = DocNo;

                    var DocMinMaxDate = DapperORM.DynamicQuerySingle("Select MinDate,MaxDate from tbl_Set_FutureDate where FormName='WorkOnTask'");
                    TempData["DocMinDate"] = DocMinMaxDate.MinDate;
                    TempData["DocMaxDate"] = DocMinMaxDate.MaxDate;
                    //if (TaskAssignID != null)
                    //{
                    //    var Description = DapperORM.DynamicQuerySingle("Select Description from TMS_TaskAssign where Deactivate=0 and TaskAssignID=" + TaskAssignID + " ");
                    //    TMSWorkOnTask.WorkOnTask = Description.Description;
                    //}
                }
                if (TaskAssignID != null && TaskTittle != null && StartDate != null)
                {
                    TMSWorkOnTask.WorkOnTaskTaskAssignID = Convert.ToDouble(TaskAssignID);
                    TMSWorkOnTask.TaskDate = Convert.ToDateTime(StartDate);
                    TMSWorkOnTask.TaskName = TaskTittle;
                    TMSWorkOnTask.DocNos = DocNo;
                    TempData["CreatedDate"] = TMSWorkOnTask.TaskDate;
                    Session["TaskAssignID"] = TMSWorkOnTask.WorkOnTaskTaskAssignID;
                    TempData["AssignerName"] = AssignerName;
                    TempData["AssignDate"] = DateOrTime;
                    TempData["AssignTime"] = AssignTime;
                    TempData["Priority"] = Priority;
                    TempData["ProjectName"] = ProjectName;
                    TempData["ModuleName"] = ModuleName;
                    TempData["Clientname"] = Clientname;
                    TempData["TaskCategoryName"] = TaskCategoryName;
                    TempData["TaskSubCategoryName"] = TaskSubCategoryName;
                    return View(TMSWorkOnTask);
                }
                if (WorkOnTaskID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_WorkOnTaskID_Encrypted", WorkOnTaskID_Encrypted);
                    TMSWorkOnTask = DapperORM.ReturnList<TMS_WorkOnTask>("sp_List_TMS_WorkOnTask", param).FirstOrDefault();
                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {
                        var GetDocNo = "Select DocNo from TMS_WorkOnTask where Deactivate=0 and WorkOnTaskID_Encrypted='" + WorkOnTaskID_Encrypted + "'";
                        var DocNos = DapperORM.DynamicQuerySingle(GetDocNo);
                        var SetdocNo = DocNos.DocNo;
                        ViewBag.DocNo = SetdocNo;
                    }
                }
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveUpdate(TMS_WorkOnTask TmsWorkOnTask, HttpPostedFileBase FilePath, string WorkOnTask)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 197;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var TaskAssignId = Session["TaskAssignID"];
                DynamicParameters paramWorkOnTask = new DynamicParameters();
                paramWorkOnTask.Add("@p_process", string.IsNullOrEmpty(TmsWorkOnTask.WorkOnTaskID_Encrypted) ? "Save" : "Update");
                paramWorkOnTask.Add("@p_WorkOnTaskID", TmsWorkOnTask.WorkOnTaskID);
                paramWorkOnTask.Add("@p_WorkOnTaskID_Encrypted", TmsWorkOnTask.WorkOnTaskID_Encrypted);
                paramWorkOnTask.Add("@p_DocNo", TmsWorkOnTask.DocNo);
                paramWorkOnTask.Add("@p_DocDate", TmsWorkOnTask.DocDate);
                paramWorkOnTask.Add("@p_WorkOnTaskTaskAssignID", TaskAssignId);
                paramWorkOnTask.Add("@p_TaskDate", TmsWorkOnTask.TaskDate);
                paramWorkOnTask.Add("@p_TaskTime", TmsWorkOnTask.TaskTime);
                paramWorkOnTask.Add("@p_WorkFromTime", TmsWorkOnTask.WorkFromTime);
                paramWorkOnTask.Add("@p_WorkToTime", TmsWorkOnTask.WorkToTime);
                paramWorkOnTask.Add("@p_WorkOnTask", WorkOnTask);
                paramWorkOnTask.Add("@p_TaskStatus", TmsWorkOnTask.TaskStatus);
                paramWorkOnTask.Add("@p_CompletePercentage", TmsWorkOnTask.CompletePercentage);
                paramWorkOnTask.Add("@p_FilePath", FilePath == null ? "" : FilePath.FileName);
                paramWorkOnTask.Add("@p_KRA", TmsWorkOnTask.KRA);
                paramWorkOnTask.Add("@p_MachineName", Dns.GetHostName().ToString());
                paramWorkOnTask.Add("@p_CreatedUpdateBy", "Admin");
                paramWorkOnTask.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                paramWorkOnTask.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                paramWorkOnTask.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_TMS_WorkOnTask", paramWorkOnTask);
                TempData["Message"] = paramWorkOnTask.Get<string>("@p_msg");
                TempData["Icon"] = paramWorkOnTask.Get<string>("@p_Icon");
                TempData["P_Id"] = paramWorkOnTask.Get<string>("@p_Id");
                if (TempData["P_Id"] != null && TmsWorkOnTask.WorkOnTaskID_Encrypted != null || FilePath != null)
                {
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='WorkOnTask'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    var FirstPath = GetFirstPath + TempData["P_Id"] + "\\";
                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }
                    if (FilePath != null)
                    {
                        string ImgTotalKMFullPath = "";
                        ImgTotalKMFullPath = FirstPath + FilePath.FileName;
                        FilePath.SaveAs(ImgTotalKMFullPath);
                    }
                }
                return RedirectToAction("ESS_TMS_EmployeeTaskSheet", "ESS_TMS_EmployeeTaskSheet");
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
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 197;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_Qry", " ");
                var TaksAssign = DapperORM.DynamicList("sp_List_TMS_WorkOnTask", param);
                ViewBag.TaksAssign = TaksAssign;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region Delete
        public ActionResult Delete(string WorkOnTaskID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 197;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_WorkOnTaskID_Encrypted", WorkOnTaskID_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_TMS_WorkOnTask", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_TMS_WorkOnTask");
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
                    return RedirectToAction("GetList");
                }

                if (DownloadAttachment == null)
                {
                    TempData["Message"] = "File path information not found.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetList");
                }

                var driveLetter = Path.GetPathRoot(DownloadAttachment);
                // Check if the drive exists
                if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
                {
                    TempData["Message"] = $"Drive {driveLetter} does not exist.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetList");
                }
                // Construct full file path
                var fullPath = DownloadAttachment;
                // Check if the file exists
                if (!System.IO.File.Exists(fullPath))
                {
                    TempData["Message"] = "File not found on the server.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetList");
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