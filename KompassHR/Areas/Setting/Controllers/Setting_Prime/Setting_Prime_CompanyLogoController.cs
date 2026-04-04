using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Prime;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Prime
{
    public class Setting_Prime_CompanyLogoController : Controller
    {
        // GET: Setting/Setting_Prime_CompanyLogo
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region Setting_Prime_CompanyLogo Main View
        public ActionResult Setting_Prime_CompanyLogo()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 3;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var CmpId = Session["CompanyId"];
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "Select  CompanyId, CompanyName from Mas_CompanyProfile where Deactivate =0 order by CompanyName");
                var data = DapperORM.ReturnList<Mas_CompanyProfile>("sp_QueryExcution", param).ToList();
                ViewBag.GetCompanyNameList = data;
                return View();

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region Images
        public ActionResult Images(int CompanyId)
        {
            try
            {
                var path = DapperORM.DynamicQuerySingle("Select Logo from Mas_CompanyProfile Where CompanyId= " + CompanyId + "");
                var SecondPath = path != null ? path.Logo : null;

                var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='CompanyLogo'");
                var FisrtPath = GetDocPath.DocInitialPath + CompanyId + "\\";

                string GetBase64 = null;
                string fullPath = "";
                fullPath = FisrtPath + SecondPath;

                if (path.Logo != null)
                {
                    if (!Directory.Exists(fullPath))
                    {
                        using (Image image = Image.FromFile(fullPath))
                        {
                            using (MemoryStream m = new MemoryStream())
                            {
                                image.Save(m, image.RawFormat);
                                byte[] imageBytes = m.ToArray();
                                // Convert byte[] to Base64 String
                                string base64String = Convert.ToBase64String(imageBytes);
                                GetBase64 = "data:image; base64," + base64String;
                            }
                        }
                    }
                }
                return Json(new { data = GetBase64 }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region UploadLogo
        [HttpPost]
        public ActionResult UploadLogo(HttpPostedFileBase Image, Mas_CompanyProfile ObjMas_CompanyProfile)
        {
            try
            {
                var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='CompanyLogo'");
                var MoveLocation = GetDocPath.DocInitialPath + ObjMas_CompanyProfile.CompanyId + "\\";
                string driveRoot = Path.GetPathRoot(MoveLocation); // e.g., "E:\\"
                if (!Directory.Exists(driveRoot))
                {
                    TempData["Message"] = $"Drive '{driveRoot}' does not exist. Please check the path.";
                    TempData["Icon"] = "info";
                    return RedirectToAction("Setting_Prime_CompanyStamp");
                }
                if (!Directory.Exists(MoveLocation))
                {
                    Directory.CreateDirectory(MoveLocation);
                }
                string path = "";
                path = MoveLocation + Image.FileName;  //Save Image in this path
                string Savepath = "";
                Savepath = Image.FileName;
                Image.SaveAs(path);

                var CmpIds = ObjMas_CompanyProfile.CompanyId;
                var data = DapperORM.Execute("update Mas_CompanyProfile set Logo = @Savepath where CompanyId = @CmpIds", new { Savepath, CmpIds });

                if (data != 0)
                {
                    TempData["Message"] = "Record Updated Successfully";
                    TempData["Icon"] = "success";
                }
                return RedirectToAction("Setting_Prime_CompanyLogo");
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