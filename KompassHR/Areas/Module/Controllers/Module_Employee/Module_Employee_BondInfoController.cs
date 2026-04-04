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
using System.Web;
using System.Web.Mvc;
using System.Drawing.Imaging;
using System.Drawing;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_BondInfoController : Controller
    {
        // GET: Module/Module_Employee_BondInfo
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        #region BondInfo Main View 
        [HttpGet]
        public ActionResult Module_Employee_BondInfo()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 433;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Mas_Employee_Bond EmployeeBond = new Mas_Employee_Bond();
                param = new DynamicParameters();
                param.Add("@p_BondEmployeeId", Session["OnboardEmployeeId"]);
                TempData["OnboardEmployeeName"] = Session["OnboardEmployeeName"];
                EmployeeBond = DapperORM.ReturnList<Mas_Employee_Bond>("sp_List_Mas_Employee_Bond", param).FirstOrDefault();

                var SecondPath = "";
                var path = DapperORM.DynamicQueryList("Select BondAttachment from Mas_Employee_Bond Where BondEmployeeId= " + Session["OnboardEmployeeId"] + "").FirstOrDefault();

                if (path != null || path == "")
                {
                    ViewBag.AddUpdateTitle = "Update";
                    SecondPath = path.BondAttachment;

                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Onboarding'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    //var FirstPath = GetFirstPath +"Event"+"\\";
                    var FinalPath = GetFirstPath + Session["OnboardEmployeeId"] + "\\" + "Bond" + "\\" + EmployeeBond.BondAttachment;// First path plus concat folder by Id                                                                
                    
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
                                TempData["Message"] = "Error creating directory: " + ex.Message;
                                TempData["Icon"] = "error";
                            }
                        }

                        if (System.IO.File.Exists(fullPath))
                        {
                            string mimeType = "application/octet-stream";
                            string extension = Path.GetExtension(fullPath).ToLower();

                            byte[] fileBytes = null;

                            if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif")
                            {
                                using (Image image = Image.FromFile(fullPath))
                                {
                                    using (MemoryStream m = new MemoryStream())
                                    {
                                        image.Save(m, image.RawFormat);
                                        fileBytes = m.ToArray();

                                        // Detect MIME type
                                        if (ImageFormat.Jpeg.Equals(image.RawFormat))
                                            mimeType = "image/jpeg";
                                        else if (ImageFormat.Png.Equals(image.RawFormat))
                                            mimeType = "image/png";
                                        else if (ImageFormat.Gif.Equals(image.RawFormat))
                                            mimeType = "image/gif";
                                    }
                                }
                            }
                            else if (extension == ".pdf")
                            {
                                fileBytes = System.IO.File.ReadAllBytes(fullPath);
                                mimeType = "application/pdf";
                            }

                            if (fileBytes != null)
                            {
                                string base64String = Convert.ToBase64String(fileBytes);
                                ViewBag.UploadPhoto = $"data:{mimeType};base64,{base64String}";
                            }
                            else
                            {
                                ViewBag.UploadPhoto = "";
                            }
                        }
                        else
                        {
                            ViewBag.UploadPhoto = "";
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
                if (EmployeeBond != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    TempData["StartDate"] = EmployeeBond.StartDate;
                    TempData["EndDate"] = EmployeeBond.EndDate;
                    TempData["FileName"] = EmployeeBond.BondAttachment;
                    Session["SelectedFile"] = EmployeeBond.BondAttachment;
                }
               
                return View(EmployeeBond);
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
        public ActionResult SaveUpdate(Mas_Employee_Bond Bond, HttpPostedFileBase BondFile, string FileName)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(Bond.BondID_Encrypted) ? "Save" : "Update");
                param.Add("@p_BondID", Bond.BondID);
                param.Add("@p_BondID_Encrypted", Bond.BondID_Encrypted);
                param.Add("@p_BondEmployeeId", Session["OnboardEmployeeId"]);
                param.Add("@p_StartDate", Bond.StartDate);
                param.Add("@p_EndDate", Bond.EndDate);
                param.Add("@p_Amount", Bond.Amount);
                param.Add("@p_Description", Bond.Description);
                //param.Add("@p_BondAttachment", Bond.BondAttachment);
                string fileName = FileName;
                if (BondFile != null)
                {
                    param.Add("@p_BondAttachment", BondFile.FileName);
                }
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_Bond", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();

                TempData["P_Id"] = param.Get<string>("@p_Id");
                if (TempData["P_Id"] != null && Bond.BondID_Encrypted != null || BondFile != null)
                {
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Onboarding'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    //var FirstPath = GetFirstPath +"Event"+"\\";
                    var FirstPath = GetFirstPath + Session["OnboardEmployeeId"] + "\\" + "Bond" + "\\";// First path plus concat folder by Id
                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }

                    if (BondFile != null)
                    {
                        string ImgBondFilePath = "";
                        ImgBondFilePath = FirstPath + BondFile.FileName; //Concat Full Path and create New full Path
                        BondFile.SaveAs(ImgBondFilePath); // This is use for Save image in folder full path
                    }
                    
                }
                return RedirectToAction("Module_Employee_BondInfo", "Module_Employee_BondInfo");
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