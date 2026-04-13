using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.ESS.Models.ESS_TMS;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Net.Mime;

namespace KompassHR.Areas.ESS.Controllers.ESS_TMS
{
    public class ESS_TMS_TaskAssignController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: TMSSetting/TaskAssign
        public ActionResult ESS_TMS_TaskAssign(TMS_TaskAssign TMS_TaskAssign)
        {
            try
            {
               // TMS_TaskAssign TMSTaskAssign = new TMS_TaskAssign();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 195;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var GetDocNo = DapperORM.DynamicQuerySingle("SELECT ISNULL(MAX(CAST(DocNo AS INT)), 0) + 1 AS DocNo FROM TMS_TaskAssign WHERE Deactivate = 0");
                    ViewBag.DocNo = GetDocNo.DocNo;
                }

                DynamicParameters paramClient = new DynamicParameters();
                paramClient.Add("@query", "SELECT ClientId as Id, ClientName as Name FROM TMS_Client WHERE Deactivate = 0 ORDER BY Name");
                var Client = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramClient).ToList();
                ViewBag.TMSClient = Client;

                ViewBag.TaskCategory = "";
                ViewBag.TaskSubCategory = "";
                ViewBag.GetEmployeeName = "";
                ViewBag.TMSModule = "";
                ViewBag.TMSProject = "";
                if (TMS_TaskAssign.TaskAssignID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_TaskAssignID_Encrypted", TMS_TaskAssign.TaskAssignID_Encrypted);
                    TMS_TaskAssign = DapperORM.ReturnList<TMS_TaskAssign>("sp_List_TMS_TaskAssign", param).FirstOrDefault();

                    DynamicParameters paramTaskSubCategory = new DynamicParameters();
                    paramTaskSubCategory.Add("@query", "SELECT TaskSubCategoryId as Id, TaskSubCategoryName as Name FROM TMS_TaskSubCategory WHERE Deactivate = 0 and TaskCategoryId ='" + TMS_TaskAssign.TaskCategoryId + "' ORDER BY Name");
                    ViewBag.TaskSubCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramTaskSubCategory).ToList();

                    if (TMS_TaskAssign.ClientID > 0)
                    {
                        DynamicParameters param1 = new DynamicParameters();
                        param1.Add("@p_EmployeeId", Session["EmployeeId"]);
                        string query = "TMS_TeamAssign.ClientID =" + TMS_TaskAssign.ClientID;

                        if (TMS_TaskAssign.ProjectID != null)
                        {
                            DynamicParameters param = new DynamicParameters();
                            param.Add("@p_ProjectId", TMS_TaskAssign.ProjectID);
                            var data1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetModuleDropdown", param).ToList();
                            ViewBag.TMSModule = data1;

                            query += " and TMS_TeamAssign.ProjectID =" + TMS_TaskAssign.ProjectID;
                            param1.Add("@P_Qry", query);
                            ViewBag.GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_TMS_GetAssigntoEmployee", param1).ToList();

                            DynamicParameters paramTaskCategory = new DynamicParameters();
                            paramTaskCategory.Add("@query", "Select TMS_TaskCategory.TaskCategoryId as Id, TMS_TaskCategory.TaskCategoryName as Name from TMS_TaskCategory Inner Join TMS_TaskCate_ProjModuleMapping on TMS_TaskCate_ProjModuleMapping.TaskCategoryId = TMS_TaskCategory.TaskCategoryId and IsActive = 1 where TMS_TaskCategory.deactivate = 0 and TMS_TaskCate_ProjModuleMapping.Project = '" + TMS_TaskAssign.ProjectID + "' ORDER BY Name");
                            ViewBag.TaskCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramTaskCategory).ToList();
                        }
                        else
                        {
                            ViewBag.TMSModule = "";
                        }

                        DynamicParameters paramProject = new DynamicParameters();
                        paramProject.Add("@p_employeeid", Session["EmployeeId"]);
                        paramProject.Add("@p_ClientId", TMS_TaskAssign.ClientID);
                        var data = DapperORM.ReturnList<AllDropDownBind>("sp_Get_TMS_ProjectDropdown", paramProject).ToList();
                        ViewBag.TMSProject = data;
                    }
                    else
                    {
                        ViewBag.TMSProject = "";
                    }
                  
                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {
                        var GetDocNo = "Select DocNo from TMS_TaskAssign where Deactivate=0 and TaskAssignID_Encrypted='" + TMS_TaskAssign.TaskAssignID_Encrypted + "'";
                        var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                        ViewBag.DocNo = DocNo;
                    }
                    TempData["DocDate"] = TMS_TaskAssign.DocDate;
                    TempData["StartDate"] = TMS_TaskAssign.StartDate;
                    TempData["DueDate"] = TMS_TaskAssign.DueDate;
                    var SetdocNo = TMS_TaskAssign.DocNo;
                    ViewBag.DocNo = SetdocNo;
                }
                return View(TMS_TaskAssign);
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_TMS_TaskAssign");
            }
        }

        public ActionResult GetListTaskAssign()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 195;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_TaskAssignID_Encrypted", "List");
                param.Add("@P_Qry", "and AssignerEmployeeID =" + Session["EmployeeId"] + " order by TaskAssignID desc");
                var TaskAssign = DapperORM.DynamicList("sp_List_TMS_TaskAssign", param);
                ViewBag.TaksAssignList = TaskAssign;
                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_TMS_TaskAssign");
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveUpdate(TMS_TaskAssign TMSTaskAssign, HttpPostedFileBase AssignFilePath, string Description)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 195;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var EmployeeID = Session["EmployeeId"];
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_process", string.IsNullOrEmpty(TMSTaskAssign.TaskAssignID_Encrypted) ? "Save" : "Update");
                param1.Add("@p_TaskAssignID", TMSTaskAssign.TaskAssignID);
                param1.Add("@p_TaskAssignID_Encrypted", TMSTaskAssign.TaskAssignID_Encrypted);
                param1.Add("@p_DocNo", TMSTaskAssign.DocNo);
                param1.Add("@p_DocDate", TMSTaskAssign.DocDate);
                param1.Add("@p_ProjectID", TMSTaskAssign.ProjectID);
                param1.Add("@p_ModuleId", TMSTaskAssign.ModuleId);
                param1.Add("@p_ClientID", TMSTaskAssign.ClientID);
                param1.Add("@p_Description", TMSTaskAssign.Description);
                param1.Add("@p_StartDate", TMSTaskAssign.StartDate);
                param1.Add("@p_Description", Description);
                param1.Add("@p_AssignToEmployeeID", TMSTaskAssign.AssignToEmployeeID);
                param1.Add("@p_Priority", TMSTaskAssign.Priority);
                param1.Add("@p_TaskCategoryID", TMSTaskAssign.TaskCategoryId);
                param1.Add("@p_TaskSubCategoryId", TMSTaskAssign.TaskSubCategoryId);
                param1.Add("@p_AssignErEmployeeID", EmployeeID);
                //param1.Add("@p_FromTime", TMSTaskAssign.FromTime);
                //param1.Add("@p_ToTime", TMSTaskAssign.ToTime);
                param1.Add("@p_Time", TMSTaskAssign.Time);
                param1.Add("@p_TotalDuration", TMSTaskAssign.TotalDuration);
                param1.Add("@p_TaskType", TMSTaskAssign.TaskType);
                param1.Add("@p_TaskTittle", TMSTaskAssign.TaskTittle);
                param1.Add("@p_TaskStatus", "Pending");
                param1.Add("@p_AssignFilePath", AssignFilePath == null ? "" : AssignFilePath.FileName);
                param1.Add("@p_DueDate", TMSTaskAssign.DueDate);
                param1.Add("@p_MachineName", Dns.GetHostName().ToString());
                param1.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param1.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param1.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param1.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_TMS_TaskAssign", param1);

                TempData["Message"] = param1.Get<string>("@p_msg");
                TempData["Icon"] = param1.Get<string>("@p_Icon");
                TempData["P_Id"] = param1.Get<string>("@p_Id");
                if (TempData["P_Id"] != null && TMSTaskAssign.TaskAssignID_Encrypted != null || AssignFilePath != null)
                {
                    var GetDocPath = DapperORM.DynamicQueryList("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='TaskAssign'").FirstOrDefault();
                    var GetFirstPath = GetDocPath.DocInitialPath;
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
                return RedirectToAction("ESS_TMS_TaskAssign", "ESS_TMS_TaskAssign");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        //[HttpGet]
        //public ActionResult GetList()
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { area = "" });
        //        }
        //        // CHECK IF USER HAS ACCESS OR NOT
        //        int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 195;
        //        bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
        //        if (!CheckAccess)
        //        {
        //            Session["AccessCheck"] = "False";
        //            return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
        //        }
        //        param.Add("@p_TaskAssignID_Encrypted", "List");
        //        param.Add("@P_Qry", " and TMS_WorkOnTask.WorkOnTaskTaskAssignID = TMS_TaskAssign.TaskAssignID and TMS_TaskAssign.Status not in ('In Process','Pending') and  TMS_TaskAssign.Deactivate=0 and AssignerEmployeeID =" + Session["EmployeeId"] + "");
        //        var TaksAssign = DapperORM.DynamicList("sp_List_TMS_TaskAssign", param);
        //        ViewBag.TaksAssignList = TaksAssign;
        //        return View();
        //    }
        //    catch (Exception Ex)
        //    {
        //        return RedirectToAction(Ex.Message.ToString(), "ESS_TMS_TaskAssign ");
        //    }
        //}

        [HttpGet]
        public ActionResult GetChildTask(int TaskAssignId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 195;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_WorkOnTaskTaskAssignID", TaskAssignId);
                var GetChildList = DapperORM.ExecuteSP<dynamic>("sp_GetWorkOnTask", param).ToList();
                ViewBag.DocNo = GetChildList;

                return Json(GetChildList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }


        public ActionResult Delete(string TaskAssignID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 195;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_TaskAssignID_Encrypted", TaskAssignID_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_TMS_TaskAssign", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetListTaskAssign", "ESS_TMS_TaskAssign");
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_TMS_TaskAssign");
            }

        }

        public ActionResult GetCountTaskAssign(int EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 195;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param = new DynamicParameters();
                param.Add("@p_AssignToEmployeeID", EmployeeId);
                param.Add("@p_Sql", "Count");
                var data = DapperORM.ExecuteSP<dynamic>("sp_count_TMS_TaskAssign", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_TMS_TaskAssign");
            }

        }

        public ActionResult GetPreviewTaskAssign(int EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 195;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param = new DynamicParameters();
                param.Add("@p_AssignToEmployeeID", EmployeeId);
                param.Add("@p_Sql", "Preview");
                var data = DapperORM.ExecuteSP<dynamic>("sp_count_TMS_TaskAssign", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_TMS_TaskAssign");
            }

        }


        #region GetTotalDuration
        [HttpGet]
        public ActionResult GetTotalDuration(string FromTime, string ToTime)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 195;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                TimeSpan t1 = TimeSpan.Parse(FromTime);
                TimeSpan t2 = TimeSpan.Parse(ToTime);
                double _24h = (new TimeSpan(24, 0, 0)).TotalMilliseconds;
                double diff = t2.TotalMilliseconds - t1.TotalMilliseconds;
                if (diff < 0) diff += _24h;
                var TotalDuration = TimeSpan.FromMilliseconds(diff);
                return Json(new { data = TotalDuration }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception Ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet); ;
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
                    return RedirectToAction("GetListTaskAssign");
                }

                if (DownloadAttachment == null)
                {
                    TempData["Message"] = "File path information not found.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetListTaskAssign");
                }

                var driveLetter = Path.GetPathRoot(DownloadAttachment);
                // Check if the drive exists
                if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
                {
                    TempData["Message"] = $"Drive {driveLetter} does not exist.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetListTaskAssign");
                }
                // Construct full file path
                var fullPath = DownloadAttachment;
                // Check if the file exists
                if (!System.IO.File.Exists(fullPath))
                {
                    TempData["Message"] = "File not found on the server.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetListTaskAssign");
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
                return Json(new { Project = Project }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
        #region GetModule
        [HttpGet]
        public ActionResult GetModule(int ProjectId, int clientId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                List<AllDropDownBind> data = new List<AllDropDownBind>();
                List<AllDropDownBind> assigntoemployee = new List<AllDropDownBind>();
                List<AllDropDownBind> taskCategory = new List<AllDropDownBind>();

                if (ProjectId > 0)
                {
                    //Module
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_ProjectId", ProjectId);
                    data = DapperORM.ReturnList<AllDropDownBind>("sp_GetModuleDropdown", param).ToList();

                    //Employee
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_EmployeeId", Session["EmployeeId"]);
                    string query = "TMS_TeamAssign.ProjectID =" + ProjectId;
                    if (clientId > 0)
                    {
                        query += " and TMS_TeamAssign.clientId =" + clientId;
                    }
                    param1.Add("@P_Qry", query);
                    assigntoemployee = DapperORM.ReturnList<AllDropDownBind>("sp_TMS_GetAssigntoEmployee", param1).ToList();

                    //Task Category
                    DynamicParameters paramTaskCategory = new DynamicParameters();
                    paramTaskCategory.Add("@query", "Select TMS_TaskCategory.TaskCategoryId as Id, TMS_TaskCategory.TaskCategoryName as Name from TMS_TaskCategory Inner Join TMS_TaskCate_ProjModuleMapping on TMS_TaskCate_ProjModuleMapping.TaskCategoryId = TMS_TaskCategory.TaskCategoryId and IsActive = 1 where TMS_TaskCategory.deactivate = 0 and TMS_TaskCate_ProjModuleMapping.ProjectId = '" + ProjectId + "' ORDER BY Name");
                    taskCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramTaskCategory).ToList();
                }
                return Json(new { data = data, assigntoemployee = assigntoemployee , taskCategory = taskCategory }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetAssigntoEmployee
        [HttpGet]
        public ActionResult GetAssigntoEmployee(int? ProjectId,int ClientID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                List<AllDropDownBind> assigntoemployee = new List<AllDropDownBind>();
                if (ClientID > 0)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_EmployeeId", Session["EmployeeId"]);
                    string query = "TMS_TeamAssign.ClientID =" + ClientID;

                    if (ProjectId > 0)
                    {
                        query += " and TMS_TeamAssign.ProjectID =" + ProjectId;
                    }

                    param1.Add("@P_Qry", query);
                    assigntoemployee = DapperORM.ReturnList<AllDropDownBind>("sp_TMS_GetAssigntoEmployee", param1).ToList();
                }


                return Json(new { assigntoemployee = assigntoemployee }, JsonRequestBehavior.AllowGet);
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
                paramTaskCategory.Add("@query", "Select TMS_TaskCategory.TaskCategoryId as Id, TMS_TaskCategory.TaskCategoryName as Name from TMS_TaskCategory Inner Join TMS_TaskCate_ProjModuleMapping on TMS_TaskCate_ProjModuleMapping.TaskCategoryId = TMS_TaskCategory.TaskCategoryId and IsActive = 1 where TMS_TaskCategory.deactivate = 0 and TMS_TaskCate_ProjModuleMapping.ProjectId='" + ProjectId + "' and TMS_TaskCate_ProjModuleMapping.ModuleId='"+ ModuleId +"' order by Name");
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
                paramTaskSubCategory.Add("@query", "SELECT TaskSubCategoryId as Id, TaskSubCategoryName as Name FROM TMS_TaskSubCategory WHERE Deactivate = 0 and TaskCategoryId ='"+ TaskCategoryId +"' ORDER BY Name");
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
    }
}