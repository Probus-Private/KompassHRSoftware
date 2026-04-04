using Dapper;
using iTextSharp.text;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Drawing;



namespace KompassHR.Areas.Setting.Controllers.Setting_Payroll
{
    public class Setting_Payroll_PayslipTemplateController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: Setting/Setting_Payroll_PayslipTemplate
        public ActionResult Setting_Payroll_PayslipTemplate()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 392;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@P_TemplateId", "List");
                var PayslipTemplate = DapperORM.ReturnList<dynamic>("sp_List_PayslipTemplate",param).ToList();
                ViewBag.Payslip = PayslipTemplate;


                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult PreviewTemplate(int templateId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@P_TemplateId", templateId);
          
                var template = DapperORM.ReturnList<dynamic>("sp_List_PayslipTemplate",param).FirstOrDefault();

                if (template == null)
                    return HttpNotFound();

                string html = template.Payslip_Template;

                //// Sample preview data
                //var model = new PayslipViewModel
                //{
                //    EmployeeName = "John Doe",
                //    EmployeeCode = "EMP001",
                //    BasicSalary = 25000,
                //    HRA = 8000,
                //    Allowances = 5000,
                //    Deductions = 3000
                //};

                //model.NetSalary = model.BasicSalary + model.HRA + model.Allowances - model.Deductions;

                //// Replace placeholders
                //html = html.Replace("{{EmployeeName}}", model.EmployeeName);
                //html = html.Replace("{{EmployeeCode}}", model.EmployeeCode);
                //html = html.Replace("{{BasicSalary}}", model.BasicSalary.ToString("N2"));
                //html = html.Replace("{{NetSalary}}", model.NetSalary.ToString("N2"));

                


                ViewBag.TemplateHtml = html;

                DynamicParameters PersonalInfo = new DynamicParameters();
                PersonalInfo.Add("@p_SalaryID", 0);
                PersonalInfo.Add("@p_Template_Id", templateId);
                var GetPersonal = DapperORM.ExecuteSP<dynamic>("sp_GeneratePayslipHTML", PersonalInfo).FirstOrDefault();
                var TemplateHtml = GetPersonal.PayslipHTML;

                // your logo path
                string logoPath = Convert.ToString(Session["CompanyLogo"]);

                // replace placeholder
                TemplateHtml = TemplateHtml.Replace("{{CompanyLogo}}", logoPath);

                ViewBag.TemplateHtml = TemplateHtml;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region TemplateSelection
        public ActionResult TemplateSelection(int templateId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Save" );                
                param.Add("@p_Payslip_TemplateId", templateId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_PayslipSetting", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                return RedirectToAction("Setting_Payroll_PayslipTemplate");
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        #endregion TemplateSelection
    }
}