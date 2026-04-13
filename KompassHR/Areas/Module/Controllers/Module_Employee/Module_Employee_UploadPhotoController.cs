using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Drawing.Imaging;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_UploadPhotoController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Module/Module_Employee_UploadPhoto
        #region UploadPhoto Main View 
        [HttpGet]
        public ActionResult Module_Employee_UploadPhoto()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 434;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                var OnboardEmployeeId = Session["OnboardEmployeeId"];
                Mas_Employee_Photo EmployeePhoto = new Mas_Employee_Photo();

                var countEMP = DapperORM.DynamicQuerySingle("Select count(PhotoEmployeeId)  as PhotoEmployeeId  from Mas_Employee_Photo where Mas_Employee_Photo.PhotoEmployeeId=" + Session["OnboardEmployeeId"] + "");
                TempData["CountPhotoEmployeeId"] = countEMP?.PhotoEmployeeId;

                var countPreBoard = DapperORM.DynamicQuerySingle(
         "SELECT ISNULL(CASE WHEN pre.UploadSignature IS NULL THEN 0 ELSE 1 END, 0) AS IsSignatureAvailable " +
         "FROM preboarding_mas_Employee pre " +
         "LEFT JOIN mas_Employee emp ON emp.PreboardingFid = pre.Fid " +
         "WHERE emp.EmployeeId = " + Session["OnboardEmployeeId"]
     );

                TempData["PreBoardFid"] = countPreBoard?.IsSignatureAvailable ?? 0;

                var SecondPath = "";
                var path = DapperORM.DynamicQueryList("Select PhotoPath from Mas_Employee_Photo Where PhotoEmployeeId= " + Session["OnboardEmployeeId"] + "").FirstOrDefault();

                if (path != null || path == "")
                {
                    ViewBag.AddUpdateTitle = "Update";
                    SecondPath = path.PhotoPath;

                    var GetDocPath1 = DapperORM.DynamicQuerySingle("SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin = 'Onboarding'");
                    var GetFirstPath = GetDocPath1.DocInitialPath;
                    var FinalPath = GetFirstPath + Session["OnboardEmployeeId"] + "\\" + "Photo" + "\\" + SecondPath;// First path plus concat folder by Id

                    //Session["SelectedFile"] = SecondPath;
                    //var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Onboarding'");
                    //var FisrtPath = GetDocPath.DocInitialPath + Session["OnboardEmployeeId"] + "\\" + "Photo" + "\\";

                    string fullPath = "";
                    //fullPath = FisrtPath + SecondPath;
                    fullPath = FinalPath;

                    if (fullPath != null)
                    {
                        string directoryPath = Path.GetDirectoryName(fullPath);
                        if (!Directory.Exists(directoryPath))
                        {
                            try
                            {
                                var destinationDirectory = Path.GetDirectoryName(directoryPath);
                                if (!Directory.Exists(destinationDirectory))
                                {
                                    Directory.CreateDirectory(destinationDirectory);
                                }
                            }
                            catch (Exception ex)
                            {
                                TempData["Message"] = "Error creating directory:" + ex.Message;
                                TempData["Icon"] = "error";
                            }
                        }
                        else
                        {
                            string mimeType = "image/png"; // or "image/jpeg", depending on your file

                            if (Directory.Exists(directoryPath))
                            {
                                using (Image image = Image.FromFile(fullPath))
                                {
                                    using (MemoryStream m = new MemoryStream())
                                    {
                                        image.Save(m, image.RawFormat);
                                        byte[] imageBytes = m.ToArray();

                                        // Detect MIME type based on RawFormat
                                        if (ImageFormat.Jpeg.Equals(image.RawFormat))
                                            mimeType = "image/jpeg";
                                        else if (ImageFormat.Png.Equals(image.RawFormat))
                                            mimeType = "image/png";
                                        else if (ImageFormat.Gif.Equals(image.RawFormat))
                                            mimeType = "image/gif";

                                        string base64String = Convert.ToBase64String(imageBytes);
                                        ViewBag.UploadPhoto = $"data:{mimeType};base64,{base64String}";
                                    }
                                }
                            }
                            else
                            {
                                ViewBag.UploadPhoto = "";
                            }
                        }
                    }
                    else
                    {
                        ViewBag.UploadPhoto = "";
                    }
                }
                else
                {
                    path = "";
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

        #region SaveUpdate
        public ActionResult SaveUpdate(Mas_Employee_Photo EmpPhoto, HttpPostedFileBase EmployeePhoto)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(EmpPhoto.PhotoId_Encrypted) ? "Save" : "Update");
                param.Add("@p_PhotoId", EmpPhoto.PhotoId);
                param.Add("@p_PhotoId_Encrypted", EmpPhoto.PhotoId_Encrypted);
                param.Add("@p_PhotoEmployeeId", Session["OnboardEmployeeId"]);
                //param.Add("@p_PhotoPath", EmpPhoto.PhotoPath);

                if (EmployeePhoto != null)
                {
                    param.Add("@p_PhotoPath", EmployeePhoto.FileName);
                }
                //else
                //{
                //    param.Add("@p_PhotoPath", EmployeePhoto == null ? "" : EmployeePhoto.FileName);
                //}


                //if (Session["SelectedFile"] != null || Session["SelectedFile"] != "")
                //{
                //    param.Add("@p_PhotoPath", Session["SelectedFile"]);
                //}
                //else
                //{
                //    param.Add("@p_PhotoPath", EmployeePhoto == null ? "" : EmployeePhoto.FileName);
                //}
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                                                                                                             // param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_Photo]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                var OnboardEmployeeId = Session["OnboardEmployeeId"];
                //TempData["P_Id"] = param.Get<string>("@p_Id");
                if (OnboardEmployeeId != null && (EmpPhoto?.PhotoId_Encrypted != null || EmployeePhoto != null))
                {
                    var GetDocPath = DapperORM.DynamicQuerySingle("SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin = 'Onboarding'");
                    var GetFirstPath = GetDocPath.DocInitialPath;

                    var FirstPath = GetFirstPath + OnboardEmployeeId + "\\" + "Photo" + "\\";// First path plus concat folder by Id
                    // Check if directory exists
                    if (!Directory.Exists(FirstPath))
                    {
                        try
                        {
                            var destinationDirectory = Path.GetDirectoryName(FirstPath);
                            if (!Directory.Exists(destinationDirectory))
                            {
                                Directory.CreateDirectory(destinationDirectory);
                            }
                        }
                        catch (Exception ex)
                        {
                            TempData["Message"] = "Error creating directory:" + ex.Message;
                            TempData["Icon"] = "error";
                        }
                    }
                    if (EmployeePhoto != null)
                    {
                        string ImgUploadPhotoFilePath = Path.Combine(FirstPath, EmployeePhoto.FileName);
                        try
                        {
                            EmployeePhoto.SaveAs(ImgUploadPhotoFilePath); // Save image in folder full path
                        }
                        catch (Exception ex)
                        {
                            TempData["Message"] = "Error saving photo: " + ex.Message;
                            TempData["Icon"] = "error";
                        }
                    }
                }


                //if (OnboardEmployeeId != null && EmpPhoto.PhotoId_Encrypted != null || EmployeePhoto != null)
                //{
                //    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Onboarding'");
                //    var GetFirstPath = GetDocPath.DocInitialPath;
                //    //var FirstPath = GetFirstPath +"Event"+"\\";
                //    var FirstPath = GetFirstPath + OnboardEmployeeId + "\\" + "Photo" + "\\";// First path plus concat folder by Id
                //    if (!Directory.Exists(FirstPath))
                //    {
                //        Directory.CreateDirectory(FirstPath);
                //    }

                //    if (EmployeePhoto != null)
                //    {
                //        string ImgUploadPhotoFilePath = "";
                //        ImgUploadPhotoFilePath = FirstPath + EmployeePhoto.FileName; //Concat Full Path and create New full Path
                //        EmployeePhoto.SaveAs(ImgUploadPhotoFilePath); // This is use for Save image in folder full path
                //    }
                //}
                return RedirectToAction("Module_Employee_UploadPhoto", "Module_Employee_UploadPhoto");
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
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var onboardEmployeeId = Convert.ToString(Session["OnboardEmployeeId"]);
                if (string.IsNullOrEmpty(onboardEmployeeId))
                {
                    System.Diagnostics.Debug.WriteLine("OnboardEmployeeId session is empty.");
                    return RedirectToAction("Module_Employee_UploadPhoto", "Module_Employee_UploadPhoto");
                }

                // Get PreboardingFid
                var preboardingResult = DapperORM.DynamicQuerySingle("SELECT PreboardingFid FROM Mas_Employee WHERE EmployeeId = " + onboardEmployeeId);
                var preboardingFid = preboardingResult?.PreboardingFid;
                if (preboardingFid == null)
                {
                    System.Diagnostics.Debug.WriteLine("PreboardingFid not found.");
                    return RedirectToAction("Module_Employee_UploadPhoto", "Module_Employee_UploadPhoto");
                }

                // Initialize prePhotosPath
                string prePhotosPath = "";

                try
                {
                    var photoResult = DapperORM.DynamicQuerySingle("SELECT Photo FROM Preboarding_Mas_Employee WHERE Fid = " + preboardingFid);
                    prePhotosPath = Convert.ToString(photoResult?.Photo);

                    if (string.IsNullOrEmpty(prePhotosPath))
                    {
                        System.Diagnostics.Debug.WriteLine("Preboarding photo path not found.");
                    }
                    else
                    {
                        // Try file copy
                        var docPathPreResult = DapperORM.DynamicQuerySingle("SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin = 'Preboarding'");
                        var preDocPath = Convert.ToString(docPathPreResult?.DocInitialPath);

                        var docPathOnboardingResult = DapperORM.DynamicQuerySingle("SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin = 'Onboarding'");
                        var onboardingDocPath = Convert.ToString(docPathOnboardingResult?.DocInitialPath);

                        if (!string.IsNullOrEmpty(preDocPath) && !string.IsNullOrEmpty(onboardingDocPath))
                        {
                            var sourceFilePath = Path.Combine(preDocPath, preboardingFid.ToString(), "Photo", prePhotosPath);
                            var destinationFilePath = Path.Combine(onboardingDocPath, onboardEmployeeId, "Photo", prePhotosPath);

                            if (System.IO.File.Exists(sourceFilePath))
                            {
                                try
                                {
                                    var destinationDir = Path.GetDirectoryName(destinationFilePath);
                                    if (!Directory.Exists(destinationDir))
                                    {
                                        Directory.CreateDirectory(destinationDir);
                                    }

                                    System.IO.File.Copy(sourceFilePath, destinationFilePath, true);
                                    System.Diagnostics.Debug.WriteLine("File copied successfully.");
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine("File copy failed: " + ex.Message);
                                    // Continue execution
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("Source file not found: " + sourceFilePath);
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Document path(s) missing.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Exception while handling photo path or file copy: " + ex.Message);
                    // Do not stop code
                }

                // 🔄 Continue with DB logic (always run, even if photo logic failed)
                var param = new DynamicParameters();
                param.Add("@p_process", "Save");
                param.Add("@p_PhotoEmployeeId", onboardEmployeeId);
                param.Add("@p_PhotoPath", prePhotosPath); // This may be empty if error happened
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                var result = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_Photo]", param);

                return RedirectToAction("Module_Employee_UploadPhoto", "Module_Employee_UploadPhoto");


                //if (Session["EmployeeId"] == null)
                //{
                //    return RedirectToAction("Login", "Login", new { Area = "" });
                //}

                //// Ensure OnboardEmployeeId is not null
                //var onboardEmployeeId = Convert.ToString(Session["OnboardEmployeeId"]);
                //if (string.IsNullOrEmpty(onboardEmployeeId))
                //{
                //    throw new Exception("OnboardEmployeeId session is empty.");
                //}

                //// Get PreboardingFid using parameterized query
                //var queryPreboardingFid = DapperORM.DynamicQueryList("SELECT PreboardingFid FROM Mas_Employee WHERE EmployeeId = '" + onboardEmployeeId + "'").FirstOrDefault();
                //var GetPreboardingFid = queryPreboardingFid != null ? queryPreboardingFid.PreboardingFid : null;
                ////var queryPreboardingFid = "SELECT PreboardingFid FROM Mas_Employee WHERE EmployeeId = @EmployeeId";
                ////var GetPreboardingFid = DapperORM.DynamicQuerySingle(queryPreboardingFid, new { EmployeeId = onboardEmployeeId });

                //if (GetPreboardingFid == null)
                //{
                //    throw new Exception("PreboardingFid not found.");
                //}

                //var PreboardingFid = GetPreboardingFid;

                //// Fetch Preboarding Photo Path
                //var queryPrePhotosPath = DapperORM.DynamicQuerySingle("SELECT Photo FROM Preboarding_Mas_Employee WHERE Fid = " + PreboardingFid + "");
                ////var PrePhotosPath = queryPreboardingFid != null ? queryPreboardingFid.PreboardingFid : null;

                ////var queryPrePhotosPath = "SELECT Photo FROM Preboarding_Mas_Employee WHERE Fid = @PreboardingFid";
                ////var PrePhotosPath = DapperORM.DynamicQueryList(queryPrePhotosPath, new { PreboardingFid });

                //var PrePhotosPath = queryPreboardingFid?.PreboardingFid;

                //if (PrePhotosPath == null || PrePhotosPath == 0)
                //{

                //}
                //// Get source document path
                //var queryDocPathPre = "SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin = 'Preboarding'";
                //var PreGetDocPaths = DapperORM.DynamicQuerySingle(queryDocPathPre);
                //var PreGetDocPath = PreGetDocPaths.DocInitialPath;
                //if (string.IsNullOrEmpty(PreGetDocPath))
                //{

                //}

                //var sourceFilePathPre = Path.Combine(PreGetDocPath, PreboardingFid.ToString(), "Photo", PrePhotosPath);

                //// Get Employee Upload Photo Path
                //var queryDocPathEmployee = "SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin = 'Onboarding'";
                //var GetDocPath = DapperORM.ExecuteQuery(queryDocPathEmployee);

                //if (string.IsNullOrEmpty(GetDocPath))
                //{
                //    throw new Exception("Employee document path not found.");
                //}

                //var employeeId = Convert.ToString(Session["OnboardEmployeeId"]);
                //var destinationFilePath = Path.Combine(GetDocPath, employeeId, "Photo", PrePhotosPath);

                //if (System.IO.File.Exists(sourceFilePathPre))
                //{
                //    try
                //    {
                //        // Ensure the directory exists before copying the file
                //        var destinationDirectory = Path.GetDirectoryName(destinationFilePath);
                //        if (!Directory.Exists(destinationDirectory))
                //        {
                //            Directory.CreateDirectory(destinationDirectory);
                //        }

                //        // Copy the file
                //        System.IO.File.Copy(sourceFilePathPre, destinationFilePath, true);

                //        // Log success message
                //        System.Diagnostics.Debug.WriteLine("File copied successfully to: " + destinationFilePath);
                //    }
                //    catch (Exception fileEx)
                //    {
                //        System.Diagnostics.Debug.WriteLine("File copy failed: " + fileEx.Message);
                //        throw new Exception("Error copying file: " + fileEx.Message);
                //    }

                //    param.Add("@p_process", "Save");
                //    param.Add("@p_PhotoEmployeeId", Session["OnboardEmployeeId"]);
                //    //param.Add("@p_PhotoPath", EmpPhoto.PhotoPath);
                //    param.Add("@p_PhotoPath", PrePhotosPath == null ? "" : PrePhotosPath);
                //    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                //    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                //    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                //    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                //    var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_Photo]", param);
                //}
                //else
                //{
                //    throw new FileNotFoundException("Source file not found: " + sourceFilePathPre);
                //}

                //return RedirectToAction("Module_Employee_UploadPhoto", "Module_Employee_UploadPhoto");
            }
            catch (Exception ex)
            {
                // Log the error properly
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
    }
}