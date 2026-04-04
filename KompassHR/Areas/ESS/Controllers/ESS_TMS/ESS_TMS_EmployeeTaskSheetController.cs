using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Net.Mime;

namespace KompassHR.Areas.ESS.Controllers.ESS_TMS
{
    public class ESS_TMS_EmployeeTaskSheetController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: TMSSetting/EmployeeTaskSheet
        public ActionResult ESS_TMS_EmployeeTaskSheet()
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
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_TaskAssignID_Encrypted", "List");
                param.Add("@P_Qry", " and TMS_TaskAssign.TaskStatus <>'Complete' and AssignToEmployeeID ='" + EmployeeId + "' and TMS_TaskAssign.Deactivate =0 order by TaskAssignID desc");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_TMS_TaskAssign", param).ToList();
                ViewBag.EmployeeTaskSeet = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpGet]
        public ActionResult GetChildTask(string TaskAssignId)
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
                param.Add("@P_WorkOnTaskTaskAssignID", TaskAssignId);
                var GetChildList = DapperORM.ExecuteSP<dynamic>("sp_GetWorkOnTask", param).ToList();
                //ViewBag.DocNo = GetChildList.DocNo[0];

                return Json(GetChildList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        #region Download Attachment
        public ActionResult DownloadAttachment(string DownloadAttachment)
        {
            try
            {
                if (string.IsNullOrEmpty(DownloadAttachment))
                {
                    TempData["Message"] = "Invalid File.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("ESS_TMS_EmployeeTaskSheet");
                }

                if (DownloadAttachment == null)
                {
                    TempData["Message"] = "File path information not found.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("ESS_TMS_EmployeeTaskSheet");
                }

                var driveLetter = Path.GetPathRoot(DownloadAttachment);
                // Check if the drive exists
                if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
                {
                    TempData["Message"] = $"Drive {driveLetter} does not exist.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("ESS_TMS_EmployeeTaskSheet");
                }
                // Construct full file path
                var fullPath = DownloadAttachment;
                // Check if the file exists
                if (!System.IO.File.Exists(fullPath))
                {
                    TempData["Message"] = "File not found on the server.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("ESS_TMS_EmployeeTaskSheet");
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