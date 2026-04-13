using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Conference;
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

namespace KompassHR.Areas.ESS.Controllers.ESS_Conference
{
    public class ESS_Curve_DocumentSubmissionController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        GetMenuList ClsGetMenuList = new GetMenuList();

        //[HttpPost]
        //[ValidateInput(false)]
        #region ESS_Curve_DocumentSubmission
        public ActionResult ESS_Curve_DocumentSubmission(Curve_DocumentSubmission Curve_DocumentSubmission)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 906;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";

                var GetInstruction = "Select Instruction from DocumentAssignment_Master where deactivate=0 and DocAssignmentMasterId='" + Curve_DocumentSubmission.DocAssignmentMasterId + "'";
                var Instruction = DapperORM.DynamicQuerySingle(GetInstruction);
                Curve_DocumentSubmission.Instruction = Instruction.Instruction;

                return View(Curve_DocumentSubmission);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 906;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                List<dynamic> finalDocumentList = new List<dynamic>();
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Process", "PendingList");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var menu = DapperORM.ExecuteSP<dynamic>("sp_Onboarding_List_DocumentSubmission", param).ToList();
                finalDocumentList.AddRange(menu);
                ViewBag.GetUserDocumentList = finalDocumentList;
                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Curve_DocumentSubmission ");
            }

        }
        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Curve_DocumentSubmission Curve_DocumentSubmission, HttpPostedFileBase DocumentPath)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                StringBuilder strBuilder = new StringBuilder();
                strBuilder.AppendLine("UPDATE dbo.DocumentAssignment_Details " +
                "SET IsSubmit = 1, " +
                "DocumentPath = '" + DocumentPath.FileName + "', " +
                "DocSubmitDate = GETDATE(), " +
                "Remark = '" + Curve_DocumentSubmission.Remark + "', " +
                "SubmissionStatus = 'Complete', " +
                "Status = 'Pending', " +
                "ModifiedBy = '" + Session["EmployeeName"] + "', " +
                "ModifiedDate = GETDATE()" +
                " WHERE DocAssignmentDetailsId = '" + Curve_DocumentSubmission.DocAssignmentDetailsId + "' and EmployeeId = '" + Session["EmployeeId"] + "'");
                string abc = "";
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    if (DocumentPath != null)
                    {
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Curve'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        var FirstPath = GetFirstPath + "\\" + Curve_DocumentSubmission.DocAssignmentDetailsId + "\\";
                        if (!Directory.Exists(FirstPath))
                        {
                            Directory.CreateDirectory(FirstPath);
                        }
                        if (DocumentPath != null)
                        {
                            string fileFullPath = "";
                            fileFullPath = FirstPath + DocumentPath.FileName;
                            DocumentPath.SaveAs(fileFullPath);
                        }
                    }

                    TempData["Message"] = "Record Save successfully.";
                    TempData["Icon"] = "success";
                }
                return RedirectToAction("GetList", "ESS_Curve_DocumentSubmission");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(int? DocAssignmentDetailsId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var GetStatus = "Select Status from DocumentAssignment_Details where deactivate=0 and DocAssignmentDetailsId = '" + DocAssignmentDetailsId + "'";
                var Status = DapperORM.DynamicQuerySingle(GetStatus);
                var FinalStatus = Status.Status;
                if (FinalStatus == "Approved")
                {
                    TempData["Message"] = "Record Can't delete it already approved.";
                    TempData["Icon"] = "error";
                }
                else
                {

                    StringBuilder strBuilder = new StringBuilder();
                    strBuilder.AppendLine("UPDATE dbo.DocumentAssignment_Details " +
                    "SET IsSubmit = 0, " +
                    "DocumentPath = '', " +
                    "DocSubmitDate = '', " +
                    "Remark = '', " +
                    "SubmissionStatus = 'Pending', " +
                    "ModifiedBy = '" + Session["EmployeeName"] + "', " +
                    "ModifiedDate = GETDATE()" +
                    " WHERE DocAssignmentDetailsId = '" + DocAssignmentDetailsId + "' and EmployeeId = '" + Session["EmployeeId"] + "'");
                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {
                        TempData["Message"] = "Record delete successfully.";
                        TempData["Icon"] = "success";
                    }
                }
                return RedirectToAction("GetList", "ESS_Curve_DocumentSubmission");
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
                    return RedirectToAction("ESS_Curve_DocumentSubmission", "ESS_Curve_DocumentSubmission");
                }

                if (DownloadAttachment == null)
                {
                    TempData["Message"] = "File path information not found.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetList", "ESS_Curve_DocumentSubmission");
                }

                var driveLetter = Path.GetPathRoot(DownloadAttachment);
                // Check if the drive exists
                if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
                {
                    TempData["Message"] = $"Drive {driveLetter} does not exist.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetList", "ESS_Curve_DocumentSubmission");
                }
                // Construct full file path
                var fullPath = DownloadAttachment;

                // Check if the file exists
                if (!System.IO.File.Exists(fullPath))
                {
                    TempData["Message"] = "File not found on your server.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetList", "ESS_Curve_DocumentSubmission");
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