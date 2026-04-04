using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_MyPolicyAndLibrary
{
    public class ESS_MyPolicyAndLibrary_MyLibraryController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: MyPolicy/Library

        #region Library Main View
        [HttpGet]
        public ActionResult ESS_MyPolicyAndLibrary_MyLibrary()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 207;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param = new DynamicParameters();
                param.Add("@query", "Select Distinct DocumentCategoryName,CompanyDocumentLibraryCategoryId from Mas_CompanyDocumentLibrary,Mas_CompanyDocumentCategory where Mas_CompanyDocumentLibrary.Deactivate=0 and Mas_CompanyDocumentLibrary.CompanyDocumentLibraryCategoryId=Mas_CompanyDocumentCategory.CompanyDocumentCategoryId");
                var MasDocumentLibrary = DapperORM.DynamicList("sp_QueryExcution", param);
                ViewBag.GetEmployeeName = MasDocumentLibrary;
                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "Library");
            }
        }
        #endregion

        #region LibraryList View
        [HttpGet]
        public ActionResult GetList(int? CompanyDocumentLibraryCategoryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 207;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from[dbo].[Tool_Documnet_DirectoryPath] where DocOrigin = 'CompanyLibrary'");
                var GetFirstPath = GetDocPath.DocInitialPath;

                param.Add("@query", "Select  [CompanyDocumentLibraryId],[Description],'" + GetFirstPath + "'+CONVERT(varchar(50), CompanyDocumentLibraryId)+'\\'+DocumentPath as DocumentPath from [dbo].[Mas_CompanyDocumentLibrary] where [Deactivate]=0 and  [CompanyDocumentLibraryCategoryId]=" + CompanyDocumentLibraryCategoryId);
                var ListOfLibrary = DapperORM.DynamicList("sp_QueryExcution", param);
                ViewBag.ListOfLibrary = ListOfLibrary;
                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "Library");
            }
        }
        #endregion

        #region Download File
        public ActionResult Download(string DocumentPath)
        {
            try
            {
                if (DocumentPath != null)
                {
                    System.IO.File.ReadAllBytes(DocumentPath);
                    return File(DocumentPath, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(DocumentPath));
                }
                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "Library");
            }
        }
        #endregion
    }
}