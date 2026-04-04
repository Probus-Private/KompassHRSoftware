using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_AllEmployeeController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        // GET: Module/Module_Employee_AllEmployee
        #region AllEmployee
        public ActionResult Module_Employee_AllEmployee()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_EmployeeID", Session["EmployeeId"]);
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Mas_Employee", param).ToList();
                ViewBag.GetEmployeeInfoList = data;
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 263;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
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

        #region MyRegion
        public ActionResult EmployeeProfileView(string EmployeeId_Encrypted, int? EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (EmployeeId_Encrypted != null && EmployeeId != null)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@query", "Select * from View_Onboarding_EmployeeList where (EmployeeId_Encrypted='" + EmployeeId_Encrypted + "' or EmployeeId=" + EmployeeId + ") and Deactivate=0");
                    var EmployeeReportList = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramList).FirstOrDefault();
                    ViewBag.GetEmployeeReportList = EmployeeReportList;


                    var SecondPath = "";
                    var path = DapperORM.DynamicQueryList("Select PhotoPath from Mas_Employee_Photo Where PhotoEmployeeId= " + EmployeeId + "").FirstOrDefault();

                    //if (path != null || path == "")
                    //{
                    //    ViewBag.AddUpdateTitle = "Update";
                    //    SecondPath = path.PhotoPath;
                    //    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Onboarding'");
                    //    var FisrtPath = GetDocPath.DocInitialPath + EmployeeId + "\\" + "Photo" + "\\";


                    //    string fullPath = "";
                    //    fullPath = FisrtPath + SecondPath;

                    //    if (fullPath != null)
                    //    {
                    //        if (!Directory.Exists(fullPath))
                    //        {

                    //            using (Image image = Image.FromFile(fullPath))
                    //            {
                    //                using (MemoryStream m = new MemoryStream())
                    //                {
                    //                    image.Save(m, image.RawFormat);
                    //                    byte[] imageBytes = m.ToArray();

                    //                    // Convert byte[] to Base64 String
                    //                    string base64String = Convert.ToBase64String(imageBytes);
                    //                    ViewBag.UploadPhoto = "data:image; base64," + base64String;
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    path = "";
                    //}

                    if (path != null || path == "")
                    {
                        try
                        {
                            ViewBag.AddUpdateTitle = "Update";
                            SecondPath = path.PhotoPath;
                            var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Onboarding'");
                            var FisrtPath = GetDocPath.DocInitialPath + EmployeeId + "\\" + "Photo" + "\\";

                            string fullPath = "";
                            fullPath = FisrtPath + SecondPath;


                            string directoryPath = Path.GetDirectoryName(fullPath);
                            if (!Directory.Exists(directoryPath))
                            {
                                ViewBag.UploadPhoto = "";
                            }
                            else
                            {
                                using (Image image = Image.FromFile(fullPath))
                                {
                                    using (MemoryStream m = new MemoryStream())
                                    {
                                        image.Save(m, image.RawFormat);
                                        byte[] imageBytes = m.ToArray();

                                        // Convert byte[] to Base64 String
                                        string base64String = Convert.ToBase64String(imageBytes);
                                        ViewBag.UploadPhoto = "data:image; base64," + base64String;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message != null)
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
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@query", "Select * from View_Onboarding_EmployeeList where EmployeeId=" + Session["EmployeeId"] + " and Deactivate=0");
                    var EmployeeReportList = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramList).FirstOrDefault();
                    ViewBag.GetEmployeeReportList = EmployeeReportList;

                    var GetCompanyStamp = "Select PhotoPath from Mas_Employee_Photo Where PhotoEmployeeId= " + Session["EmployeeId"] + "";
                    var SecondPath = "";
                    var path = DapperORM.DynamicQuerySingle(GetCompanyStamp);


                    if (path != null || path == "")
                    {
                        try
                        {

                            ViewBag.AddUpdateTitle = "Update";
                            SecondPath = path.PhotoPath;
                            var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Onboarding'");
                            var FisrtPath = GetDocPath.DocInitialPath + Session["EmployeeId"] + "\\" + "Photo" + "\\";

                            string fullPath = "";
                            fullPath = FisrtPath + SecondPath;

                            string directoryPath = Path.GetDirectoryName(fullPath);
                            if (!Directory.Exists(directoryPath))
                            {
                                ViewBag.UploadPhoto = "";
                            }
                            else
                            {
                                using (Image image = Image.FromFile(fullPath))
                                {
                                    using (MemoryStream m = new MemoryStream())
                                    {
                                        image.Save(m, image.RawFormat);
                                        byte[] imageBytes = m.ToArray();

                                        // Convert byte[] to Base64 String
                                        string base64String = Convert.ToBase64String(imageBytes);
                                        ViewBag.UploadPhoto = "data:image; base64," + base64String;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message != null)
                            {
                                ViewBag.UploadPhoto = "";
                            }
                        }
                    }
                    else
                    {
                        ViewBag.UploadPhoto = "";
                    }

                    //if (path != null || path == "")
                    //{
                    //    ViewBag.AddUpdateTitle = "Update";
                    //    SecondPath = path.PhotoPath;
                    //    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Onboarding'");
                    //    var FisrtPath = GetDocPath.DocInitialPath + Session["EmployeeId"] + "\\" + "Photo" + "\\";


                    //    string fullPath = "";
                    //    fullPath = FisrtPath + SecondPath;

                    //    if (fullPath != null)
                    //    {
                    //        if (!Directory.Exists(fullPath))
                    //        {

                    //            using (Image image = Image.FromFile(fullPath))
                    //            {
                    //                using (MemoryStream m = new MemoryStream())
                    //                {
                    //                    image.Save(m, image.RawFormat);
                    //                    byte[] imageBytes = m.ToArray();

                    //                    // Convert byte[] to Base64 String
                    //                    string base64String = Convert.ToBase64String(imageBytes);
                    //                    ViewBag.UploadPhoto = "data:image; base64," + base64String;
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    path = "";
                    //}
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

    }
}