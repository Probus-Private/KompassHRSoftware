using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_UploadDocumentController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: Module/Module_Employee_UploadDocument
        #region UploadDocument Main View
        [HttpGet]
        public ActionResult Module_Employee_UploadDocument()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 436;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Mas_Employee_UploadDocument EmployeeDocument = new Mas_Employee_UploadDocument(); 
                param.Add("@query", "Select DocumentId as Id,DocumentName as Name from Mas_Document Where Deactivate=0");
                var DocumentType = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetDocumentType = DocumentType;

                var countEMP = DapperORM.DynamicQuerySingle("Select count(DocumentEmployeeId)  as DocumentEmployeeId  from Mas_Employee_UploadDocument where Mas_Employee_UploadDocument.DocumentEmployeeId=" + Session["OnboardEmployeeId"] + "");
                TempData["CountDocumentEmployeeId"] = countEMP.DocumentEmployeeId;

                var countPreBoard = DapperORM.DynamicQuerySingle("select  COUNT(UploadDocumentID) as UploadDocumentID from preboarding_mas_employee_uploaddocuments left join mas_employee on preboardingfid=documentpreboardingfid where employeeid=" + Session["OnboardEmployeeId"] + "");
                TempData["PreBoardFid"] = countPreBoard?.UploadDocumentID;

                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_DocumentEmployeeId", Session["OnboardEmployeeId"]);
                var  EmployeeDocumentList = DapperORM.ExecuteSP<dynamic>("sp_List_Mas_Employee_UploadDocument", paramList).ToList();
                ViewBag.EmployeeUploadDocumentList = EmployeeDocumentList;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region SaveUpdate
        public ActionResult SaveUpdate(Mas_Employee_UploadDocument EmpDocument, HttpPostedFileBase EmployeeFrontPage, HttpPostedFileBase EmployeeBackPage)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                //For Front Page Insert
                if (EmployeeFrontPage!=null)
                {
                    param.Add("@p_process", string.IsNullOrEmpty(EmpDocument.EmployeeUploadDocumentID_Encrypted) ? "Save" : "Update");
                    param.Add("@p_EmployeeUploadDocumentID", EmpDocument.EmployeeUploadDocumentID);
                    param.Add("@p_EmployeeUploadDocumentID_Encrypted", EmpDocument.EmployeeUploadDocumentID_Encrypted);
                    param.Add("@p_DocumentEmployeeId", Session["OnboardEmployeeId"]);
                    param.Add("@p_UploadDocumentName","Front Page");
                    param.Add("@p_DocumentID", EmpDocument.DocumentID);
                    param.Add("@p_DocumentPath", EmployeeFrontPage == null ? "" : EmployeeFrontPage.FileName);
                   
                    //if (EmpDocument.EmployeeUploadDocumentID_Encrypted != null && EmployeeFrontPage == null)
                    //{
                    //    param.Add("@p_DocumentPath", Session["SelectedFile"]);
                    //}
                    //else
                    //{
                    //    param.Add("@p_DocumentPath", EmployeeFrontPage == null ? "" : EmployeeFrontPage.FileName);
                    //}
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                                                                                                                 // param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_UploadDocument]", param);
                    var message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    TempData["Message"] = message;
                    TempData["Icon"] = Icon.ToString();
                    var OnboardEmployeeId = Session["OnboardEmployeeId"];
                    if (OnboardEmployeeId != null && EmpDocument.EmployeeUploadDocumentID_Encrypted != null || EmployeeFrontPage != null)
                    {
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Onboarding'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        //var FirstPath = GetFirstPath +"Event"+"\\";
                        var FirstPath = GetFirstPath + OnboardEmployeeId + "\\" + "Document" + "\\";// First path plus concat folder by Id
                        if (!Directory.Exists(FirstPath))
                        {
                            Directory.CreateDirectory(FirstPath);
                        }

                        if (EmployeeFrontPage != null)
                        {
                            string ImgUploadFrontPageFilePath = "";
                            ImgUploadFrontPageFilePath = FirstPath + EmployeeFrontPage.FileName; //Concat Full Path and create New full Path
                            EmployeeFrontPage.SaveAs(ImgUploadFrontPageFilePath); // This is use for Save image in folder full path
                        }
                    }
                }
                //For Back Page Insert
                if (EmployeeBackPage!=null)
                {
                    param.Add("@p_process", string.IsNullOrEmpty(EmpDocument.EmployeeUploadDocumentID_Encrypted) ? "Save" : "Update");
                    param.Add("@p_EmployeeUploadDocumentID", EmpDocument.EmployeeUploadDocumentID);
                    param.Add("@p_EmployeeUploadDocumentID_Encrypted", EmpDocument.EmployeeUploadDocumentID_Encrypted);
                    param.Add("@p_DocumentEmployeeId", Session["OnboardEmployeeId"]);
                    param.Add("@p_UploadDocumentName","Back Page");
                    param.Add("@p_DocumentID", EmpDocument.DocumentID);
                    param.Add("@p_DocumentPath", EmployeeFrontPage == null ? "" : EmployeeFrontPage.FileName);
                    //if (EmpDocument.EmployeeUploadDocumentID_Encrypted != null && EmployeeBackPage == null)
                    //{
                    //    param.Add("@p_DocumentPath", Session["SelectedFile"]);
                    //}
                    //else
                    //{
                    //    param.Add("@p_DocumentPath", EmployeeBackPage == null ? "" : EmployeeBackPage.FileName);
                    //}
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                                                                                                                 // param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_UploadDocument]", param);
                    var message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    TempData["Message"] = message;
                    TempData["Icon"] = Icon.ToString();
                    var OnboardEmployeeId = Session["OnboardEmployeeId"];
                    if (OnboardEmployeeId != null && EmpDocument.EmployeeUploadDocumentID_Encrypted != null || EmployeeBackPage != null)
                    {
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Onboarding'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        //var FirstPath = GetFirstPath +"Event"+"\\";
                        //var FirstPath = GetFirstPath + OnboardEmployeeId + "\\";// First path plus concat folder by Id
                        var FirstPath = GetFirstPath + OnboardEmployeeId + "\\" + "Document" + "\\";// First path plus concat folder by Id
                        if (!Directory.Exists(FirstPath))
                        {
                            Directory.CreateDirectory(FirstPath);
                        }

                        if (EmployeeBackPage != null)
                        {
                            string ImgUploadBackPageFilePath = "";
                            ImgUploadBackPageFilePath = FirstPath + EmployeeBackPage.FileName; //Concat Full Path and create New full Path
                            EmployeeBackPage.SaveAs(ImgUploadBackPageFilePath); // This is use for Save image in folder full path
                        }
                    }
                }
              
                return RedirectToAction("Module_Employee_UploadDocument", "Module_Employee_UploadDocument");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetDocumentBackPage
        [HttpGet]
        public ActionResult GetDocumentBackPage(int? DocumentId)
        {
            try
            {
                var IsBackPage = DapperORM.DynamicQuerySingle("Select IsBackPage from Mas_Document where Deactivate=0 and DocumentId=" + DocumentId + "");
                return Json(IsBackPage, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
           
        }
        #endregion

        #region MyRegion
        public ActionResult Delete(string EmployeeUploadDocumentID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_EmployeeUploadDocumentID_Encrypted", EmployeeUploadDocumentID_Encrypted);                
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_UploadDocument", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();

                return RedirectToAction("Module_Employee_UploadDocument", "Module_Employee_UploadDocument");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Download Image 
        public ActionResult DownloadFile(string UploadDocumentFile)
        {
            try
            {
                if (UploadDocumentFile != null)
                {
                    System.IO.File.ReadAllBytes(UploadDocumentFile);
                    return File(UploadDocumentFile, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(UploadDocumentFile));
                }
                else
                {
                    return RedirectToAction("Reports_GeneralReport_DMS", "Reports_GeneralReport_DMS", new { Area = "Reports" });
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
           
        }

        #endregion

        #region PreboardingGetDetails
        public ActionResult PreboardingGetDetails()
        {
            try
            {
                // ✅ Check login
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var onboardEmployeeId = Convert.ToString(Session["OnboardEmployeeId"]);
                if (string.IsNullOrEmpty(onboardEmployeeId))
                {
                    throw new Exception("OnboardEmployeeId session is empty.");
                }

                // ✅ Get PreboardingFid
                var getPreboardingFid = DapperORM.DynamicQuerySingle(
                    "SELECT PreboardingFid FROM Mas_Employee WHERE EmployeeId = " + onboardEmployeeId
                );

                if (getPreboardingFid?.PreboardingFid == null)
                {
                    throw new Exception("PreboardingFid not found.");
                }

                var preboardingFid = getPreboardingFid.PreboardingFid;

                // ✅ Insert preboarding documents
                StringBuilder strBuilder = new StringBuilder();

                string insertSql = @"
            INSERT INTO Mas_Employee_UploadDocument
            (Deactivate, CreatedBy, CreatedDate, MachineName, DocumentEmployeeId,
             DocumentID, UploadDocumentName, DocumentPath)
            SELECT 
                0,
                '" + Session["EmployeeName"] + @"',
                GETDATE(),
                '" + Dns.GetHostName() + @"',
                " + onboardEmployeeId + @",
                DocumentID,
                DocumentTypeName,
                UploadDocumentType
            FROM Preboarding_Mas_Employee_UploadDocuments
            WHERE DocumentPreboardingFid = " + preboardingFid;

                strBuilder.Append(insertSql);

                string errMsg = "";
                if (!objcon.SaveStringBuilder(strBuilder, out errMsg))
                {
                    TempData["Message"] = errMsg;
                    TempData["Icon"] = "error";
                    return RedirectToAction("Module_Employee_UploadDocument", "Module_Employee_UploadDocument");
                }

                // ✅ Encrypt newly inserted records
                string encryptSql = @"
            UPDATE M
            SET M.EmployeeUploadDocumentID_Encrypted = 
                master.dbo.fn_varbintohexstr(
                    HASHBYTES('SHA2_256', CONVERT(NVARCHAR(70), M.EmployeeUploadDocumentID))
                )
            FROM Mas_Employee_UploadDocument M
            WHERE M.DocumentEmployeeId = @EmpId
              AND (M.EmployeeUploadDocumentID_Encrypted IS NULL OR M.EmployeeUploadDocumentID_Encrypted = '')";

                DapperORM.Execute(encryptSql, new { EmpId = onboardEmployeeId });

                // ✅ Copy files from preboarding folder to onboarding folder
                var docPathPreResult = DapperORM.DynamicQuerySingle(
                    "SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin = 'Preboarding'"
                );
                var preDocPath = Convert.ToString(docPathPreResult?.DocInitialPath);

                var docPathEmpResult = DapperORM.DynamicQuerySingle(
                    "SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin = 'Onboarding'"
                );
                var onboardDocPath = Convert.ToString(docPathEmpResult?.DocInitialPath);

                if (!string.IsNullOrEmpty(preDocPath) && !string.IsNullOrEmpty(onboardDocPath))
                {
                    var sourceDirectory = Path.Combine(preDocPath, preboardingFid.ToString(), "Document");
                    var destinationDirectory = Path.Combine(onboardDocPath, onboardEmployeeId, "Document");

                    if (Directory.Exists(sourceDirectory))
                    {
                        try
                        {
                            if (!Directory.Exists(destinationDirectory))
                            {
                                Directory.CreateDirectory(destinationDirectory);
                            }

                            var files = Directory.GetFiles(sourceDirectory);
                            foreach (var file in files)
                            {
                                var fileName = Path.GetFileName(file);
                                var destFilePath = Path.Combine(destinationDirectory, fileName);
                                System.IO.File.Copy(file, destFilePath, true);
                            }
                        }
                        catch (Exception copyEx)
                        {
                            System.Diagnostics.Debug.WriteLine("File copy error: " + copyEx.Message);
                        }
                    }
                }

                // ✅ Update SetupDocuments flag
                string updateSetupSql = @"
            UPDATE Mas_Employee_Setup
            SET SetupUploadDocument = 1
            WHERE SetupEmployeeId = @EmpId";

                DapperORM.Execute(updateSetupSql, new { EmpId = onboardEmployeeId });

                TempData["Message"] = "Record Saved Successfully";
                TempData["Icon"] = "success";

                return RedirectToAction("Module_Employee_UploadDocument", "Module_Employee_UploadDocument");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

    }
}