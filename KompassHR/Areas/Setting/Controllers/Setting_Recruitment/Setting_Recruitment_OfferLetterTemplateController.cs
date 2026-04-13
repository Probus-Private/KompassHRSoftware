using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Recruitment;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Recruitment
{
    public class Setting_Recruitment_OfferLetterTemplateController : Controller
    {
        // GET: Setting/Setting_Recruitment_OfferLetterTemplate
        public ActionResult Setting_Recruitment_OfferLetterTemplate()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    TempData["Message"] = "Your session has expired. Please log in again.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null
                    ? Convert.ToInt32(Request.QueryString["ScreenId"])
                    : 879;

                bool CheckAccess = new BulkAccessClass()
                    .CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));

                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                var CMPID = GetComapnyName[0].Id;
                return View();
            }
            catch (Exception ex)
            {
                TempData["Message"] = "An error occurred: " + ex.Message;
                TempData["Icon"] = "error";
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    TempData["Message"] = "Your session has expired. Please log in again.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null
                    ? Convert.ToInt32(Request.QueryString["ScreenId"])
                    : 879;

                bool CheckAccess = new BulkAccessClass()
                    .CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));

                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                DynamicParameters paramlist = new DynamicParameters();
                paramlist.Add("@p_LetterTemplateId_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Recruitment_OfferLetterTemplate", paramlist);
                ViewBag.GetList = data;
                return View();
            }
            catch (Exception ex)
            {
                TempData["Message"] = "An error occurred: " + ex.Message;
                TempData["Icon"] = "error";
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveUpdate(Recruitment_LetterTemplates letter)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();

                // 🔥 IMPORTANT: Decide Save or Update
                string process = string.IsNullOrEmpty(letter.LetterTemplateId_Encrypted)
                                    ? "Save"
                                    : "Update";

                param.Add("@p_process", process);

                param.Add("@p_LetterTemplateId_Encrypted", letter.LetterTemplateId_Encrypted);
                param.Add("@p_CmpId", letter.CmpId);
                param.Add("@p_DocOrigin", 1);

                // ✅ REQUIRED FIELDS
                param.Add("@p_TemplateSubject", letter.TemplateSubject);
                param.Add("@p_TemplateBody", letter.TemplateBody);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName());

                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 10);

                DapperORM.ExecuteReturn("sp_SUD_Recruitment_LetterTemplates", param);

                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                return RedirectToAction("GetList", "Setting_Recruitment_OfferLetterTemplate", new { area = "" });
            }
            catch (Exception ex)
            {
                TempData["Message"] = "An error occurred: " + ex.Message;
                TempData["Icon"] = "error";
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

    }
}