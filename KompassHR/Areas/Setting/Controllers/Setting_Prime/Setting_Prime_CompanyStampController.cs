using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Prime;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Prime
{
    public class Setting_Prime_CompanyStampController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: PrimeSetting/CompanyStamp
        #region CompanyStamp  MAIn View
        public ActionResult Setting_Prime_CompanyStamp()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 4;
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
                var GetCompanyStamp = "Select Stamp from Mas_CompanyProfile Where CompanyId= " + CompanyId + "";
                var path = DapperORM.DynamicQuerySingle(GetCompanyStamp);
                var SecondPath = path.Stamp;

                var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='CompanyStamp'");
                var FisrtPath = GetDocPath.DocInitialPath + CompanyId + "\\";

                string GetBase64 = null;
                string fullPath = "";
                fullPath = FisrtPath + SecondPath;

                if (SecondPath != null)
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

        #region UploadStamp
        [HttpPost]
        public ActionResult UploadStamp(HttpPostedFileBase Image, Mas_CompanyProfile ObjMas_CompanyProfile)
        {
            try
            {

                var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='CompanyStamp'");
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

                var CompanyId = ObjMas_CompanyProfile.CompanyId;
                var data = sqlcon.Execute("update Mas_CompanyProfile set Stamp = '" + Savepath + "' where CompanyId = '" + CompanyId + "'");

                if (data != 0)
                {
                    TempData["Message"] = "Record Updated Successfully";
                    TempData["Icon"] = "success";
                }
                return RedirectToAction("Setting_Prime_CompanyStamp");
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