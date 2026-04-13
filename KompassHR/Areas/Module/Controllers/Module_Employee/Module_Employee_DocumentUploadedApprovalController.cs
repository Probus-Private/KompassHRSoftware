using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_DocumentUploadedApprovalController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        [HttpGet]
        #region Document Assignment Main View 
        public ActionResult Module_Employee_DocumentUploadedApproval(Module_Employee_DocumentAssignment Module_Employee_DocumentAssignment)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 909;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";

                //GET COMPANY NAME
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                var CMPID = GetComapnyName[0].Id;

                //GET BRANCH NAME
                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CMPID), Convert.ToInt32(Session["EmployeeId"]));
                ViewBag.BranchName = Branch;

                var Depatment = DapperORM.DynamicQueryMultiple(@"SELECT  DepartmentId as Id,DepartmentName as Name FROM Mas_Department WHERE Deactivate =0;");
                ViewBag.DepatmentList = Depatment[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                var DocAssignmentMaster = DapperORM.DynamicQueryMultiple(@"SELECT DocAssignmentMasterId as Id,AssignmentTitle as Name FROM DocumentAssignment_Master WHERE Deactivate =0;");
                ViewBag.DocAssignmentMasterList = DocAssignmentMaster[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                if (Module_Employee_DocumentAssignment.CmpId > 0)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_CmpId", Module_Employee_DocumentAssignment.CmpId);
                    paramList.Add("@p_BranchId", Module_Employee_DocumentAssignment.BranchId);
                    paramList.Add("@p_DepartmentId", Module_Employee_DocumentAssignment.DepartmentId);
                    paramList.Add("@p_DocAssignmentMasterId", Module_Employee_DocumentAssignment.DocAssignmentMasterId);
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    var data = DapperORM.ExecuteSP<dynamic>("sp_Onbording_GetUploadDocumentEmployeeList_For_Approval", paramList).ToList();
                    ViewBag.GetUploadDocumentList = data;

                    //GET BRANCH NAME
                    var Branch1 = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(Module_Employee_DocumentAssignment.CmpId), Convert.ToInt32(Session["EmployeeId"]));
                    ViewBag.BranchName = Branch1;
                }
                return View(Module_Employee_DocumentAssignment);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
        #region  Multuple Multiple Approve Reject Request function
        [HttpPost]
        public ActionResult MultipleApproveRejectRequest(List<DocumentAssginedEmployee> EmployeeList, string Status, string Remark)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 745;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                StringBuilder strBuilder = new StringBuilder();

                for (var i = 0; i < EmployeeList.Count; i++)
                {
                    var qry = "UPDATE dbo.DocumentAssignment_Details " +
                    "SET Status = '" + Status + "', " +
                    "ApproveRejectBy = '" + Session["EmployeeName"] + "', " +
                    "ApproveRejectDate = GETDATE(), " +
                    "ApproveRejectRemark = '" + Remark + "' " +
                    "WHERE DocAssignmentDetailsId = '" + EmployeeList[i].DocAssignmentDetailsId + "' and EmployeeId= '" + EmployeeList[i].EmployeeId + "';";
                    strBuilder.Append(qry);
                }
                string abc = "";
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    if (Status == "Approved")
                    {
                        TempData["Message"] = "Record Approved successfully";
                        TempData["Icon"] = "success";
                    }
                    else
                    {
                        TempData["Message"] = "Record Rejected successfully";
                        TempData["Icon"] = "success";
                    }
                }
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
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
                    return RedirectToAction("Module_Employee_DocumentUploadedApproval", "Module_Employee_DocumentUploadedApproval");
                }

                if (DownloadAttachment == null)
                {
                    TempData["Message"] = "File path information not found.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("Module_Employee_DocumentUploadedApproval", "Module_Employee_DocumentUploadedApproval");
                }

                var driveLetter = Path.GetPathRoot(DownloadAttachment);
                // Check if the drive exists
                if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
                {
                    TempData["Message"] = $"Drive {driveLetter} does not exist.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("Module_Employee_DocumentUploadedApproval", "Module_Employee_DocumentUploadedApproval");
                }
                // Construct full file path
                var fullPath = DownloadAttachment;

                // Check if the file exists
                if (!System.IO.File.Exists(fullPath))
                {
                    TempData["Message"] = "File not found on your server.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("Module_Employee_DocumentUploadedApproval", "Module_Employee_DocumentUploadedApproval");
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

        #region GetApproveRejectList
        public ActionResult GetApproveRejectList(int? CmpId, string BranchId, string DepartmentId, int? DocAssignmentMasterId)
        {
            try
            {  
                param.Add("@p_Process", "ApproveRejectList");
                param.Add("@p_CmpId", CmpId);
                param.Add("@p_BranchId", BranchId);
                param.Add("@p_DepartmentId", DepartmentId);
                param.Add("@p_DocAssignmentMasterId", DocAssignmentMasterId);
                var data = DapperORM.ReturnList<dynamic>("sp_Onbording_GetUploadDocumentEmployeeList", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
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