using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_GeneralReport
{
    public class Reports_GeneralReport_DMSController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        // GET: Reports/Reports_GeneralReport_DMS
        #region Reports_GeneralReport_DMS Main view
        public ActionResult Reports_GeneralReport_DMS()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var EmployeeDocument = DapperORM.DynamicQuerySingle("Select DocOriginName as DocOrigin,replace(round(SUM(FileSize)/1024,2,0),0,'') as FileSize from DMS_EmployeeDocument where Deactivate=0 Group By DocOriginName").ToList();
                ViewBag.DMSListName = EmployeeDocument;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region DMSRecordPreview
        [HttpGet]
        public ActionResult DMSRecordPreview(string DocOrigin)
        {
            try
            {
                param.Add("@p_DocOrigin", DocOrigin);
                var data = DapperORM.ExecuteSP<dynamic>("sp_DMSRecord", param).ToList();
                ViewBag.DMSRecordPreviewList = data;
                //var EmployeeDocument = DapperORM.DynamicQuerySingle("Select SUM(FileSize)/1024 as FileSize from DMS_EmployeeDocument where Deactivate=0 Group By DocOriginName").ToList();
                //ViewBag.DMSListName = EmployeeDocument;
                return Json(data = ViewBag.DMSRecordPreviewList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
            
        }
        #endregion

        #region DMSEmployeeWise
        [HttpGet]
        public ActionResult DMSEmployeeWises(int Id, string Origin)
        {
            try
            {
                param.Add("@p_DocOrigin", Origin);
                param.Add("@p_EmployeeId", Id);
                var data = DapperORM.ExecuteSP<dynamic>("sp_DMSEmployeeRecord", param).ToList();
                ViewBag.DMSEmployeeWiseList = data;
                return Json(data = ViewBag.DMSEmployeeWiseList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
           
        }
        #endregion

        #region Download Image 
        public ActionResult DownloadFile(string AnnouncementFile)
        {
            try
            {
                if (AnnouncementFile != null)
                {
                    System.IO.File.ReadAllBytes(AnnouncementFile);
                    return File(AnnouncementFile, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(AnnouncementFile));
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
    }
}