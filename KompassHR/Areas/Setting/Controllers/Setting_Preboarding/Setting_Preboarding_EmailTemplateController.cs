using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Preboarding;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Preboarding
{
    public class Setting_Preboarding_EmailTemplateController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_Preboarding_EmailTemplate
        #region Setting_Preboarding_EmailTemplate MAin View
        [HttpGet]
        public ActionResult Setting_Preboarding_EmailTemplate(string EmailTemplateID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 43;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@query", "Select DocOriginID as Id,DocOriginName As Name from Preboarding_EmailDocOrigin Where Deactivate=0");
                var EmailDocOrigin = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetEmailDocOrigin = EmailDocOrigin;

                ViewBag.AddUpdateTitle = "Add";
                Preboarding_EmailTemplate PreboardingEmailTemplate = new Preboarding_EmailTemplate();
                if (EmailTemplateID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_EmailTemplateID_Encrypted", EmailTemplateID_Encrypted);
                    PreboardingEmailTemplate = DapperORM.ReturnList<Preboarding_EmailTemplate>("sp_List_Preboarding_EmailTemplate", param).FirstOrDefault();
                }
                return View(PreboardingEmailTemplate);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion


        #region  IsVAlidation
        [HttpGet]
        public ActionResult IsEmailValidationExists(string EmailTemplateID_Encrypted,double DocOrigin)
        {
            try
            {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_DocOrigin", DocOrigin);
                    param.Add("@p_EmailTemplateID_Encrypted", EmailTemplateID_Encrypted);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Preboarding_EmailTemplate", param);
                    var Message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    if (Message != "")
                    {
                        return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }               
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region SaveUpdate 
        [HttpPost, ValidateInput(false)]
        public ActionResult SaveUpdate(Preboarding_EmailTemplate ObjPreboarding_EmailTemplate)
        {
            try
            {

                param.Add("@p_process", string.IsNullOrEmpty(ObjPreboarding_EmailTemplate.EmailTemplateID_Encrypted) ? "Save" : "Update");
                param.Add("@p_EmailTemplateID", ObjPreboarding_EmailTemplate.EmailTemplateID);
                param.Add("@p_EmailTemplateID_Encrypted", ObjPreboarding_EmailTemplate.EmailTemplateID_Encrypted);
                param.Add("@p_DocOrigin", ObjPreboarding_EmailTemplate.DocOrigin);
                param.Add("@p_SendEmail", ObjPreboarding_EmailTemplate.SendEmail);
                param.Add("@p_BCCIsRequired", ObjPreboarding_EmailTemplate.BCCIsRequired);
                param.Add("@p_EmailSubject", ObjPreboarding_EmailTemplate.EmailSubject);
                param.Add("@p_EmailBody", ObjPreboarding_EmailTemplate.EmailBody);
                param.Add("@p_SendSMS", ObjPreboarding_EmailTemplate.SendSMS);
                param.Add("@p_SMSBody", ObjPreboarding_EmailTemplate.SMSBody);
                param.Add("@p_SendWhatsapp", ObjPreboarding_EmailTemplate.SendWhatsapp);
                param.Add("@p_WhatsappBody", ObjPreboarding_EmailTemplate.WhatsappBody);           
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Preboarding_EmailTemplate", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return RedirectToAction("Setting_Preboarding_EmailTemplate", "Setting_Preboarding_EmailTemplate");

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region getList
        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 43;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_EmailTemplateID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Preboarding_EmailTemplate", param);
                ViewBag.GetEmailTemplate = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region
        [HttpGet]
        public ActionResult Delete(string EmailTemplateID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_EmailTemplateID_Encrypted", EmailTemplateID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_Preboarding_EmailTemplate", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();

                return RedirectToAction("GetList", "Setting_Preboarding_EmailTemplate");
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