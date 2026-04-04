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
    public class Module_Employee_SignatureController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Module/Module_Employee_Signature
        #region Signature Main View
        [HttpGet]
        public ActionResult Module_Employee_Signature()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 435;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                var OnboardEmployeeId = Session["OnboardEmployeeId"];
                Mas_Employee_Signature EmployeeSignature = new Mas_Employee_Signature();
                var countEMP = DapperORM.DynamicQuerySingle("Select count(SignatureEmployeeId)  as SignatureEmployeeId  from Mas_Employee_Signature where Mas_Employee_Signature.SignatureEmployeeId=" + Session["OnboardEmployeeId"] + "");
                TempData["CountSignatureEmployeeId"] = countEMP?.SignatureEmployeeId;

                var countPreBoard = DapperORM.DynamicQuerySingle(
   "SELECT ISNULL(CASE WHEN pre.UploadSignature IS NULL THEN 0 ELSE 1 END, 0) AS IsSignatureAvailable " +
   "FROM preboarding_mas_Employee pre " +
   "LEFT JOIN mas_Employee emp ON emp.PreboardingFid = pre.Fid " +
   "WHERE emp.EmployeeId = " + Session["OnboardEmployeeId"]
);

                TempData["PreBoardFid"] = countPreBoard?.IsSignatureAvailable ?? 0;
                var path = DapperORM.DynamicQueryList("Select SignaturePath from Mas_Employee_Signature Where SignatureEmployeeId= " + Session["OnboardEmployeeId"] + "").FirstOrDefault();
                var SecondPath = "";
                if (path != null || path == "")
                {
                    ViewBag.AddUpdateTitle = "Update";
                    SecondPath = path.SignaturePath;
                    Session["SelectedFile"] = SecondPath;
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Onboarding'");
                    var FisrtPath = GetDocPath.DocInitialPath + Session["OnboardEmployeeId"] + "\\" + "Signature" + "\\";

                    string fullPath = "";
                    fullPath = FisrtPath + SecondPath;

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
                                        ViewBag.UploadSignature = $"data:{mimeType};base64,{base64String}";
                                    }
                                }
                            }
                            else
                            {
                                ViewBag.UploadSignature = "";
                            }
                        }
                    }
                    else
                    {
                        ViewBag.UploadPhoto = "";
                    }


                    //if (fullPath != null)
                    //{
                    //    if (Directory.Exists(fullPath))
                    //    {
                    //        using (Image image = Image.FromFile(fullPath))
                    //        {
                    //            using (MemoryStream m = new MemoryStream())
                    //            {
                    //                image.Save(m, image.RawFormat);
                    //                byte[] imageBytes = m.ToArray();

                    //                // Convert byte[] to Base64 String
                    //                string base64String = Convert.ToBase64String(imageBytes);
                    //                ViewBag.UploadSignature = "data:image; base64," + base64String;
                    //            }
                    //        }
                    //    }
                    //}
                }
                else
                {
                    path = "";
                }
                //var SignaturePath = DapperORM.DynamicQuerySingle("Select SignaturePath from Mas_Employee_Signature Where SignatureEmployeeId= " + Session["OnboardEmployeeId"] + "");
                //TempData["SignaturePath"] = SignaturePath.SignaturePath;
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
        public ActionResult SaveUpdate(Mas_Employee_Signature EmpSignature, HttpPostedFileBase EmployeeSignature)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(EmpSignature.SignatureId_Encrypted) ? "Save" : "Update");
                param.Add("@p_SignatureId", EmpSignature.SignatureId);
                param.Add("@p_SignatureId_Encrypted", EmpSignature.SignatureId_Encrypted);
                param.Add("@p_SignatureEmployeeId", Session["OnboardEmployeeId"]);
                //param.Add("@p_PhotoPath", EmpPhoto.PhotoPath);

                if (EmployeeSignature != null)
                {
                    param.Add("@p_SignaturePath", EmployeeSignature.FileName);
                }
                //if (Session["SelectedFile"] != null || Session["SelectedFile"] != "")
                //{
                //    param.Add("@p_SignaturePath", Session["SelectedFile"]);
                //}
                //else
                //{
                //    param.Add("@p_SignaturePath", EmployeeSignature == null ? "" : EmployeeSignature.FileName);
                //}
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                                                                                                             // param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_Signature]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                var OnboardEmployeeId = Session["OnboardEmployeeId"];
                //TempData["P_Id"] = param.Get<string>("@p_Id");
                if (OnboardEmployeeId != null && EmpSignature.SignatureId_Encrypted != null || EmployeeSignature != null)
                {
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Onboarding'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    //var FirstPath = GetFirstPath +"Event"+"\\";
                    var FirstPath = GetFirstPath + OnboardEmployeeId + "\\" + "Signature" + "\\";// First path plus concat folder by Id
                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }

                    if (EmployeeSignature != null)
                    {
                        string ImgUploadSignatureFilePath = "";
                        ImgUploadSignatureFilePath = FirstPath + EmployeeSignature.FileName; //Concat Full Path and create New full Path
                        EmployeeSignature.SaveAs(ImgUploadSignatureFilePath); // This is use for Save image in folder full path
                    }
                }
                return RedirectToAction("Module_Employee_Signature", "Module_Employee_Signature");
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

                // Get OnboardEmployeeId
                var onboardEmployeeId = Convert.ToString(Session["OnboardEmployeeId"]);
                if (string.IsNullOrEmpty(onboardEmployeeId))
                {
                    System.Diagnostics.Debug.WriteLine("OnboardEmployeeId session is empty.");
                    return RedirectToAction("Module_Employee_Signature", "Module_Employee_Signature");
                }

                // Get PreboardingFid
                var getPreboardingFid = DapperORM.DynamicQueryList("SELECT PreboardingFid FROM Mas_Employee WHERE EmployeeId = " + onboardEmployeeId).FirstOrDefault();
                if (getPreboardingFid?.PreboardingFid == null)
                {
                    System.Diagnostics.Debug.WriteLine("PreboardingFid not found.");
                    return RedirectToAction("Module_Employee_Signature", "Module_Employee_Signature");
                }
                var preboardingFid = getPreboardingFid.PreboardingFid;

                // Init PrePhotosPath
                string prePhotosPath = "";

                try
                {
                    // Fetch signature file name
                    var photoQuery = "SELECT UploadSignature FROM Preboarding_Mas_Employee WHERE Fid = " + preboardingFid;
                    var photoResult = DapperORM.DynamicQuerySingle(photoQuery);
                    prePhotosPath = Convert.ToString(photoResult?.UploadSignature);

                    if (string.IsNullOrEmpty(prePhotosPath))
                    {
                        System.Diagnostics.Debug.WriteLine("Preboarding signature path not found.");
                    }
                    else
                    {
                        // Get Preboarding path
                        var prePathResult = DapperORM.DynamicQuerySingle("SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin = 'Preboarding'");
                        var preDocPath = Convert.ToString(prePathResult?.DocInitialPath);

                        // Get Onboarding path
                        var onboardPathResult = DapperORM.DynamicQuerySingle("SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin = 'Onboarding'");
                        var onboardingDocPath = Convert.ToString(onboardPathResult?.DocInitialPath);

                        if (!string.IsNullOrEmpty(preDocPath) && !string.IsNullOrEmpty(onboardingDocPath))
                        {
                            var sourceFilePath = Path.Combine(preDocPath, preboardingFid.ToString(), "Signature", prePhotosPath);
                            var destinationFilePath = Path.Combine(onboardingDocPath, onboardEmployeeId, "Signature", prePhotosPath);

                            if (System.IO.File.Exists(sourceFilePath))
                            {
                                try
                                {
                                    var destinationDir = Path.GetDirectoryName(destinationFilePath);
                                    if (!Directory.Exists(destinationDir))
                                        Directory.CreateDirectory(destinationDir);

                                    System.IO.File.Copy(sourceFilePath, destinationFilePath, true);
                                    System.Diagnostics.Debug.WriteLine("Signature file copied successfully.");
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine("File copy failed: " + ex.Message);
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("Source signature file not found: " + sourceFilePath);
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Preboarding or Onboarding path missing.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Signature processing error: " + ex.Message);
                }

                // ✅ Always continue with DB procedure
                param.Add("@p_process", "Save");
                param.Add("@p_SignatureEmployeeId", onboardEmployeeId);
                param.Add("@p_SignaturePath", prePhotosPath);  // May be empty
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_Signature]", param);
                return RedirectToAction("Module_Employee_Signature", "Module_Employee_Signature");


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
                ////var queryPreboardingFid = "SELECT PreboardingFid FROM Mas_Employee WHERE EmployeeId = "+ onboardEmployeeId + "";
                //var GetPreboardingFid = DapperORM.DynamicQueryList("SELECT PreboardingFid FROM Mas_Employee WHERE EmployeeId = " + onboardEmployeeId + "").FirstOrDefault();

                //if (GetPreboardingFid?.PreboardingFid == null)
                //{
                //    throw new Exception("PreboardingFid not found.");
                //}

                //var PreboardingFid = GetPreboardingFid.PreboardingFid;

                //// Fetch Preboarding Photo Path
                //var queryPrePhotosPath = "SELECT UploadSignature FROM Preboarding_Mas_Employee WHERE Fid = "+ PreboardingFid + "";
                //var PrePhotosPath = DapperORM.ExecuteQuery(queryPrePhotosPath);

                //if (string.IsNullOrEmpty(PrePhotosPath))
                //{
                //    throw new Exception("Preboarding photo path not found.");
                //}

                //// Get source document path
                //var queryDocPathPre = "SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin = 'Preboarding'";
                //var PreGetDocPath = DapperORM.ExecuteQuery(queryDocPathPre);

                //if (string.IsNullOrEmpty(PreGetDocPath))
                //{
                //    throw new Exception("Source document path not found.");
                //}

                //var sourceFilePathPre = Path.Combine(PreGetDocPath, PreboardingFid.ToString(), "Signature", PrePhotosPath);

                //// Get Employee Upload Photo Path
                //var queryDocPathEmployee = "SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin = 'Onboarding'";
                //var GetDocPath = DapperORM.ExecuteQuery(queryDocPathEmployee);

                //if (string.IsNullOrEmpty(GetDocPath))
                //{
                //    throw new Exception("Employee document path not found.");
                //}

                //var employeeId = Convert.ToString(Session["OnboardEmployeeId"]);
                //var destinationFilePath = Path.Combine(GetDocPath, employeeId, "Signature", PrePhotosPath);

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

                //    param.Add("@p_process",  "Save" );
                //    param.Add("@p_SignatureEmployeeId", Session["OnboardEmployeeId"]);
                //    param.Add("@p_SignaturePath", PrePhotosPath == null ? "" : PrePhotosPath);
                //    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                //    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                //    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                //    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                //    var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_Signature]", param);
                //}
                //else
                //{
                //    throw new FileNotFoundException("Source file not found: " + sourceFilePathPre);
                //}

                //return RedirectToAction("Module_Employee_Signature", "Module_Employee_Signature");
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