using Dapper;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Mvc;

using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Drawing;

namespace KompassHR.Areas.ESS.Controllers.ESS_MyPolicyAndLibrary
{
    public class ESS_MyPolicyAndLibrary_MyPolicyController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        HttpClient client = new HttpClient();

        // GET: MyPolicy/MyPolicys
        // Sneha

        #region MyPolicys Main View
        public class PolicyLibraryModel
        {
            public int PolicyLibraryId { get; set; }
            public string GroupName { get; set; }
            public string PolicyName { get; set; }
            public string Remark { get; set; }
            public string FileType { get; set; }

            public string DocumentPath { get; set; }      // Actual DB/File path
            public string DocumentPreview { get; set; }   // Base64 string for UI
        }

        [HttpGet]
        public async Task<ActionResult> ESS_MyPolicyAndLibrary_MyPolicy()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                param = new DynamicParameters();
                param.Add("@EmployeeId", Session["EmployeeId"]);
                var masPolicyLibrary = DapperORM.ExecuteSP<PolicyLibraryModel>("sp_GetMapedPolicyGroup", param);

                var getDocPath = DapperORM.DynamicQuerySingle("SELECT DocInitialPath FROM [dbo].[Tool_Documnet_DirectoryPath] WHERE DocOrigin = 'PolicyLibrary'");
                var basePath = getDocPath.DocInitialPath;

                foreach (var item in masPolicyLibrary)
                {
                    // full physical path
                    string fullPath = Path.Combine(basePath, item.PolicyLibraryId.ToString(), item.DocumentPath ?? "");

                    if (System.IO.File.Exists(fullPath))
                    {
                        string extension = Path.GetExtension(fullPath).ToLower();

                        if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                        {
                            using (Image image = Image.FromFile(fullPath))
                            using (MemoryStream m = new MemoryStream())
                            {
                                image.Save(m, image.RawFormat);
                                byte[] imageBytes = m.ToArray();
                                string base64String = Convert.ToBase64String(imageBytes);

                                item.DocumentPreview = "data:image;base64," + base64String;
                            }
                        }
                        else if (extension == ".pdf")
                        {
                            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
                            string base64String = Convert.ToBase64String(fileBytes);

                            item.DocumentPreview = "data:application/pdf;base64," + base64String;
                        }
                        else if (extension == ".doc")
                        {
                            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
                            string base64String = Convert.ToBase64String(fileBytes);

                            item.DocumentPreview = "data:application/msword;base64," + base64String;
                        }
                        else if (extension == ".docx")
                        {
                            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
                            string base64String = Convert.ToBase64String(fileBytes);

                            item.DocumentPreview = "data:application/vnd.openxmlformats-officedocument.wordprocessingml.document;base64," + base64String;
                        }
                        else if (extension == ".xls" || extension == ".xlsx")
                        {
                            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
                            string base64String = Convert.ToBase64String(fileBytes);

                            item.DocumentPreview = "data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64," + base64String;
                        }
                    }
                }

                ViewBag.GetEmployeeNamePolicy = masPolicyLibrary;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion




        #region PolicyList View
        [HttpGet]
        public ActionResult GetList(int? PolicyLibraryPolicyId,string FileType)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from[dbo].[Tool_Documnet_DirectoryPath] where DocOrigin = 'PolicyLibrary'");
                var GetFirstPath = GetDocPath.DocInitialPath;
                if (FileType == "UploadFile")
                {
                    param.Add("@query", "SELECT [PolicyLibraryId],PolicyName, [Remark], DocumentPath,FileType, CASE WHEN DocumentPath LIKE 'http%' THEN DocumentPath ELSE '" + GetFirstPath + "' + CONVERT(varchar(50), [PolicyLibraryId]) + '\\' + DocumentPath END AS DocumentPath FROM[dbo].[Mas_PolicyLibrary] INNER JOIN dbo.Mas_Policy ON dbo.Mas_PolicyLibrary.PolicyLibraryPolicyId = dbo.Mas_Policy.PolicyId WHERE[Mas_PolicyLibrary].[Deactivate] = 0 AND PolicyLibraryPolicyId = " + PolicyLibraryPolicyId);
                    var ListOfPolicy = DapperORM.DynamicList("sp_QueryExcution", param);
                    ViewBag.ListOfpolicys = ListOfPolicy;

                }
                else
                {
                    
                        param.Add("@query", "SELECT [PolicyLibraryId], [Remark], DocumentPath,FileType FROM[dbo].[Mas_PolicyLibrary] WHERE[Deactivate] = 0 AND PolicyLibraryPolicyId =  " + PolicyLibraryPolicyId + "");
                        var ListOfPolicy = DapperORM.DynamicList("sp_QueryExcution", param);
                        ViewBag.ListOfpolicys = ListOfPolicy;
                    
                }
                //param.Add("@query", "Select  [PolicyLibraryId],[Remark],'" + GetFirstPath + "'+CONVERT(varchar(50), [PolicyLibraryId])+'\\'+DocumentPath as DocumentPath from [dbo].[Mas_PolicyLibrary] where [Deactivate]=0 and  PolicyLibraryPolicyId=" + PolicyLibraryPolicyId);
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Download File
        public ActionResult Download(string DocumentPath, int? PolicyLibraryPolicyId)
        {
            try
            {
                if (string.IsNullOrEmpty(DocumentPath))
                {
                    return Json(new { Success = false, Message = "Invalid File.", Icon = "error" }, JsonRequestBehavior.AllowGet);
                }

                var fullPath = DocumentPath;
                if (!System.IO.File.Exists(fullPath))
                {
                    return Json(new { Success = false, Message = "File not found on your server.", Icon = "error" }, JsonRequestBehavior.AllowGet);
                }

                var driveLetter = Path.GetPathRoot(DocumentPath);
                if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
                {
                    return Json(new { Success = false, Message = $"Drive {driveLetter} does not exist.", Icon = "error" }, JsonRequestBehavior.AllowGet);
                }

                var fileName = Path.GetFileName(fullPath);
                byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
                var fileBase64 = Convert.ToBase64String(fileBytes);

                return Json(new
                {
                    Success = true,
                    FileName = fileName,
                    FileData = fileBase64,
                    ContentType = MediaTypeNames.Application.Octet
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = $"Internal server error: {ex.Message}", Icon = "error" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        [HttpPost]
        public JsonResult PreviewExcel(string base64File)
        {
            try
            {
                var bytes = Convert.FromBase64String(base64File.Split(',')[1]);
                var fileName = Guid.NewGuid() + ".xlsx";
                var path = Server.MapPath("~/other/" + fileName);
                System.IO.File.WriteAllBytes(path, bytes);

                var url = Request.Url.GetLeftPart(UriPartial.Authority) + Url.Content("~/other/" + fileName);
                return Json(new { success = true, url });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        //#region PDF View
        //public ActionResult PDFPreview(string filename, int? policyLibraryId)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] != null)
        //        {
        //            //var path = Path.Combine(Server.MapPath("~/Images"), filename);
        //            //ViewBag.FileSrc = MimeMapping.GetMimeMapping(Path.GetFileName(filename));
        //            //return File(filename, mimeType);

        //            //var getFileDirectoryPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath Where DocOrigin='PolicyLibrary'").FirstOrDefault();
        //            //var fullPath = getFileDirectoryPath.DocInitialPath  + policyLibraryId + "\\" + filename;

        //            //var GetFinalPath = @"C:\ProbusSoftwarePvtLtd\Documents\PolicyLibrary\16\"+ filename + "";
        //            //TempData["GetFileName"] = fullPath;

        //            /*string filePath = fullPath;*/
        //            string filePath = "C:\\ProbusSoftwarePvtLtd\\Documents\\PolicyLibrary\\16\\7.Attendance Policy.pdf";
        //            string contentType = "application/pdf";

        //            if (System.IO.File.Exists(filePath))
        //            {
        //                return File(filePath, contentType);
        //            }
        //            else
        //            {
        //                return HttpNotFound("File not found.");
        //            }
        //        }
        //        else
        //        {
        //            return RedirectToAction("ErrorPage", "Home");
        //        }
        //        return View();
        //    }
        //    catch (Exception e)
        //    {
        //        return RedirectToAction("ErrorPage", "Home");
        //    }
        //}
        //#endregion

    }

}