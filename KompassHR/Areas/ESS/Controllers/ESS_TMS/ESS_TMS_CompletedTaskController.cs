using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
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
    public class ESS_TMS_CompletedTaskController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();


        // GET: TMSSetting/ESS_TMS_CompletedTask
        public ActionResult ESS_TMS_CompletedTask()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 198;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_TaskAssignID_Encrypted", "List");
                param.Add("@P_Qry", " and TMS_TaskAssign.TaskStatus ='Complete' and AssignToEmployeeID='" + EmployeeId + "' order by TaskAssignID desc");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_TMS_TaskAssign", param).ToList();
                ViewBag.CompleteTaskSeet = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        [HttpGet]
        public ActionResult GetChildTask(int TaskAssignId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 198;
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


        //#region Download Image 
        //public ActionResult DownloadFile(string FilePath)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { Area = "" });
        //        }
        //        // CHECK IF USER HAS ACCESS OR NOT
        //        int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 198;
        //        bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
        //        if (!CheckAccess)
        //        {
        //            Session["AccessCheck"] = "False";
        //            return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
        //        }
        //        if (FilePath != "")
        //        {
        //            System.IO.File.ReadAllBytes(FilePath);
        //            return File(FilePath, System.Net.Mime.MediaTypeNames.Application.Octet, FilePath);
        //            //return File(FilePath, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(FilePath));
        //        }
        //        else
        //        {
        //            return RedirectToAction("ESS_TMS_CompletedTask", "ESS_TMS_CompletedTask", new { Area = "ESS" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
            
        //}
        //#endregion

        #region Download Attachment
        public ActionResult DownloadAttachment(string DownloadAttachment)
        {
            try
            {
                if (string.IsNullOrEmpty(DownloadAttachment))
                {
                    TempData["Message"] = "Invalid GeneralClaimId_Encrypted.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("ESS_TMS_CompletedTask");
                }

                if (DownloadAttachment == null)
                {
                    TempData["Message"] = "File path information not found.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("ESS_TMS_CompletedTask");
                }

                var driveLetter = Path.GetPathRoot(DownloadAttachment);
                // Check if the drive exists
                if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
                {
                    TempData["Message"] = $"Drive {driveLetter} does not exist.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("ESS_TMS_CompletedTask");
                }
                // Construct full file path
                var fullPath = DownloadAttachment;
                // Check if the file exists
                if (!System.IO.File.Exists(fullPath))
                {
                    TempData["Message"] = "File not found on the server.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("ESS_TMS_CompletedTask");
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

        public ActionResult SendToApproval(double? TaskAssignID, double? AssignToEmployeeID)
        {
            StringBuilder strBuilder = new StringBuilder();

            var createdBy = Session["EmployeeName"]?.ToString().Replace("'", "''");
            var machineName = Dns.GetHostName().Replace("'", "''");

            strBuilder.AppendLine(
                "UPDATE dbo.TMS_TaskAssign SET " +
                "IsSendForApproval = 1, " +
                "ModifiedBy = '" + createdBy + "', " +
                "ModifiedDate = GETDATE(), " +
                "MachineName = '" + machineName + "' " +
                "WHERE TaskAssignID = " + TaskAssignID + " " +
                "AND AssignToEmployeeID = " + AssignToEmployeeID + ";"
            );

            strBuilder.AppendLine(
                "INSERT INTO Tra_Approval " +
                "(Deactivate, UseBy, CreatedBy, CreatedDate, MachineName, Origin, " +
                "TraApproval_DocId, TraApproval_CompId, TraApproval_BranchId, TraApproval_EmployeeId, " +
                "TraApproval_ApproverEmployeeId, ApproverLevel, IsMandatory, Status, IsActive, TraApproval_ModuleId) " +
                "SELECT " +
                "0, 0, '" + createdBy + "', GETDATE(), '" + machineName + "', 'TMS_TaskAssign', " +
                TaskAssignID + ", '" + Session["CompanyId"] + "', '" + Session["BranchId"] + "', " + AssignToEmployeeID + ", " +
                "ReportingManager1, ApproverLevel, IsCompulsory, 'Pending', 1, 18 " +
                "FROM Mas_Employee_Reporting " +
                "WHERE Deactivate = 0 " +
                "AND ReportingEmployeeID = " + AssignToEmployeeID + " " +
                "AND ReportingModuleID = 21;"
            );

            string abc = "";
            if (objcon.SaveStringBuilder(strBuilder, out abc))
            {
                TempData["Message"] = "Record sent for approval successfully.";
                TempData["Icon"] = "success";
            }

            return RedirectToAction("ESS_TMS_CompletedTask", "ESS_TMS_CompletedTask");
        }

    }
}