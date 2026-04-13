using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Email;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Email
{
    public class Setting_Email_SMTPDetailsController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        // GET: Setting/Setting_Email_SMTPDetails
        public ActionResult Setting_Email_SMTPDetails(string EmailID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Tool_EmailSetting ToolEmailSetting = new Tool_EmailSetting();
                param.Add("@query", "select CompanyName as Name,CompanyId as Id from Mas_CompanyProfile where Deactivate = 0");
                var list_ComapnyProfile = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetCompanyProfile = list_ComapnyProfile;

                param.Add("@query", "select EmailOrigin as Id,OriginName as Name from Tool_EmailOrigin where Deactivate=0");
                var EmailOrigin = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetEmailOrigin = EmailOrigin;

                if(EmailID_Encrypted != null)
                {
                    param = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_EmailID_Encrypted", EmailID_Encrypted);
                    ToolEmailSetting = DapperORM.ReturnList<Tool_EmailSetting>("sp_List_Tool_EmailSetting", param).FirstOrDefault();
                }
                return View(ToolEmailSetting);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }


        }


        public ActionResult IsAalradySmptDetails(int CompanyName ,int Origin ,string EmailIDEncrypted)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_EmailID_Encrypted", EmailIDEncrypted);
                param.Add("@p_Origin ", Origin);
                param.Add("@p_CmpId", CompanyName);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Tool_EmailSetting", param);
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


        public ActionResult SaveUpdate(Tool_EmailSetting EmailSetting)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(EmailSetting.EmailID_Encrypted) ? "Save" : "Update");               
                param.Add("@p_EmailID_Encrypted", EmailSetting.EmailID_Encrypted);
                param.Add("@p_SMTPServerName", EmailSetting.SMTPServerName);
                param.Add("@p_CmpId", EmailSetting.CmpId);
                param.Add("@p_PortNo", EmailSetting.PortNo);
                param.Add("@p_SSL", EmailSetting.SSL);
                param.Add("@p_FromEmailId", EmailSetting.FromEmailId);
                param.Add("@p_Origin", EmailSetting.Origin);
                param.Add("@p_Password", EmailSetting.Password);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Tool_EmailSetting", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Setting_Email_SMTPDetails", "Setting_Email_SMTPDetails");               
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                param = new DynamicParameters();
                param.Add("@p_EmailID_Encrypted", "List");             
                var data = DapperORM.DynamicList("sp_List_Tool_EmailSetting", param);
                ViewBag.SmtpDetails = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult Delete(String EmailID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_EmailID_Encrypted", EmailID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Tool_EmailSetting", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Email_SMTPDetails");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

    }
}