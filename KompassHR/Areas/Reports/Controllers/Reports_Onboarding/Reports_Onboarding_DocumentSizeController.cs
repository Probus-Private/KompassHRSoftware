using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_Onboarding
{
    public class Reports_Onboarding_DocumentSizeController : Controller
    {
        #region Main View
        // GET: Reports/Reports_Onboarding_DocumentSize
        public ActionResult Reports_Onboarding_DocumentSize()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) :847;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                var GetData = DapperORM.ExecuteSP<dynamic>("sp_List_GetDocumentSize", param).ToList();
                ViewBag.GetDetailsList = GetData;
                

                decimal totalFolderSize = GetData.Sum(x => (decimal?)(x.FolderSizeMB) ?? 0m);
                ViewBag.totalFolderSize = totalFolderSize;


                decimal totalFolderSizeGB = totalFolderSize / 1024m;
                ViewBag.TotalFolderSizeGB = Math.Round(totalFolderSizeGB, 3); 

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