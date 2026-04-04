using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Module.Models.Module_Payroll;
using KompassHR.Models;
using System.Net;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.IO;
using System.Net.Mime;

namespace KompassHR.Areas.Module.Controllers.Module_Payroll
{
    public class Module_Payroll_ComplienceDocumentUploadController : Controller
    {
        // GET: Module/Module_Payroll_ComplienceDocumentUpload
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        StringBuilder strBuilder = new StringBuilder();

        public ActionResult Module_Payroll_ComplienceDocumentUpload(string ComplienceDocUploadId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 499;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                //GET COMPANY NAME
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                ViewBag.BranchName = "";
                ViewBag.EmployeeName = "";
                ViewBag.GetSubTypeName = "";

                Payroll_ComplienceDocUpload DocUpload = new Payroll_ComplienceDocUpload();

                param.Add("@query", "select ComplienceDocTypeId as Id , ComplienceDocTypeName as Name from Payroll_ComplienceDocType  Where Deactivate = 0");
                var GetDoc = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetDocTypeName = GetDoc;
                
                if (ComplienceDocUploadId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";

                    param = new DynamicParameters();
                    param.Add("@p_ComplienceDocUploadId_Encrypted", ComplienceDocUploadId_Encrypted);
                    DocUpload = DapperORM.ReturnList<Payroll_ComplienceDocUpload>("sp_List_Payrol_ComplienceDocUpload", param).FirstOrDefault();
                    
                    var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(DocUpload.CmpId), Convert.ToInt32(Session["EmployeeId"]));
                    ViewBag.BranchName = Branch;

                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@query", "select Payroll_ComplienceSubType.ComplienceSubTypeId as Id, Payroll_ComplienceSubType.SubTypeName as Name from Payroll_ComplienceDocumentUpload INNER JOIN Payroll_ComplienceSubType ON Payroll_ComplienceDocumentUpload.ComplienceSubTypeId = Payroll_ComplienceSubType.ComplienceSubTypeId where Payroll_ComplienceDocumentUpload.ComplienceDocUploadId_Encrypted='"+ComplienceDocUploadId_Encrypted+ "' and Payroll_ComplienceDocumentUpload.Deactivate=0");
                    var SubType = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();
                    ViewBag.GetSubTypeName = SubType;


                }
                return View(DocUpload);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #region GetList Main View
        [HttpGet]
        public ActionResult GetList(Payroll_ComplienceDocUpload ObJDoc)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 499;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.ComplienceDocUploadList = "";

                if (ObJDoc.MonthYear!=null)
                {
                    param.Add("@P_ComplienceDocUploadId_Encrypted", "List");
                    param.Add("@p_MonthYear", ObJDoc.MonthYear);
                    var data = DapperORM.ExecuteSP<dynamic>("sp_List_Payrol_ComplienceDocUpload", param).ToList();
                    ViewBag.ComplienceDocUploadList = data;
                }
                else
                {
                    //var month = DateTime.Now;
                    // DateTime month = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    param.Add("@P_ComplienceDocUploadId_Encrypted", "List");
                    param.Add("@p_MonthYear", DateTime.Now);
                    var data = DapperORM.ExecuteSP<dynamic>("sp_List_Payrol_ComplienceDocUpload", param).ToList();
                    ViewBag.ComplienceDocUploadList = data;
                }

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CmpId), Convert.ToInt32(Session["EmployeeId"]));
                return Json(new { Branch = Branch }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetSubDoc
        [HttpGet]
        public ActionResult GetSubDoc(int DocType)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
               
                param.Add("@query", "select ComplienceSubTypeId as Id, SubTypeName as Name from Payroll_ComplienceSubType where ComplienceDocTypeId="+DocType+"  and Deactivate = 0");
                var SubType = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetSubTypeName = SubType;
                return Json(new { SubType = SubType }, JsonRequestBehavior.AllowGet);
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
        [ValidateInput(false)]
        public ActionResult SaveUpdate(Payroll_ComplienceDocUpload DocUpload, HttpPostedFileBase AssignFilePath)
        {
            try
            {

                param.Add("@p_process", string.IsNullOrEmpty(DocUpload.ComplienceDocUploadId_Encrypted) ? "Save" : "Update");
                param.Add("@p_ComplienceDocUploadId", DocUpload.ComplienceDocUploadId);
                param.Add("@p_ComplienceDocUploadId_Encrypted", DocUpload.ComplienceDocUploadId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);

                param.Add("@p_CmpId", DocUpload.CmpId);
                param.Add("@p_BranchID", DocUpload.BranchID);
                param.Add("@p_MonthYear", DocUpload.MonthYear);
                param.Add("@p_remark", DocUpload.Remark);
                param.Add("@p_ComplienceDocTypeId", DocUpload.ComplienceDocTypeId);
                param.Add("@p_ComplienceSubTypeId", DocUpload.ComplienceSubTypeId);
                param.Add("@p_FilePath", AssignFilePath == null ? "" : AssignFilePath.FileName);

                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payrol_ComplienceDocUpload", param);
                var PID = param.Get<string>("@p_Id");
                if (PID != null && DocUpload.ComplienceDocUploadId_Encrypted != null || AssignFilePath != null)
                {
                    var GetDocPath = DapperORM.DynamicQueryList("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='ComplienceDocumentUpload'").FirstOrDefault();

                    if (GetDocPath!=null)
                    {
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        var FirstPath = GetFirstPath + PID + "\\";
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
                    
                }
           
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return RedirectToAction("Module_Payroll_ComplienceDocumentUpload", "Module_Payroll_ComplienceDocumentUpload");                
                //return RedirectToAction("Setting_FullAndFinal_NoDuesChecklist", "Setting_FullAndFinal_NoDuesChecklist");
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
                if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(c => c.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
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


        #region Delete
        [HttpGet]
        public ActionResult Delete(string ComplienceDocUploadId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_ComplienceDocUploadId_Encrypted", ComplienceDocUploadId_Encrypted);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parame
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payrol_ComplienceDocUpload", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("GetList", "Module_Payroll_ComplienceDocumentUpload");
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