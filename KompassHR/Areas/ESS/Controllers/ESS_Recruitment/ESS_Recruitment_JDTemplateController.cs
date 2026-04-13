using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Recruitment;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
namespace KompassHR.Areas.ESS.Controllers.ESS_Recruitment
{
    public class ESS_Recruitment_JDTemplateController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: ESS/ESS_Recruitment_JDTemplate
        #region ESS_Recruitment_JDTemplate
        public ActionResult ESS_Recruitment_JDTemplate(string RecruitmentJDTemplateID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 473;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "select Departmentid as Id, DepartmentName Name from mas_department where Deactivate=0");
                var Department = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", param1).ToList();
                ViewBag.DepartmenthName = Department;

                if (RecruitmentJDTemplateID_Encrypted != null)
                {
                    Recruitment_JobDescriptionTemplate Template = new Recruitment_JobDescriptionTemplate();
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_RecruitmentJDTemplateID_Encrypted", RecruitmentJDTemplateID_Encrypted);
                    Template = DapperORM.ReturnList<Recruitment_JobDescriptionTemplate>("sp_List_Recruitment_JobDescriptionTemplate", param).FirstOrDefault();
                    return View(Template);
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
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveUpdate(Recruitment_JobDescriptionTemplate OBJTemplate, string Description)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                param.Add("@p_process", string.IsNullOrEmpty(OBJTemplate.RecruitmentJDTemplateID_Encrypted) ? "Save" : "Update");
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_RecruitmentJDTemplate", OBJTemplate.RecruitmentJDTemplate);
                param.Add("@p_JDTemplateDepartmentId", OBJTemplate.JDTemplateDepartmentId);
                param.Add("@p_Description", Description);
                param.Add("@p_RecruitmentJDTemplateID_Encrypted", OBJTemplate.RecruitmentJDTemplateID_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_JobDescriptionTemplate", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_Recruitment_JDTemplate", "ESS_Recruitment_JDTemplate");
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        [HttpGet]
        public ActionResult IsTemplateExists(string RecruitmentJDTemplate, string EmployeeTypeID_Encrypted, int Dept)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_RecruitmentJDTemplate", RecruitmentJDTemplate);
                param.Add("@p_JDTemplateDepartmentId", Dept);
                param.Add("@p_RecruitmentJDTemplateID_Encrypted", EmployeeTypeID_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_JobDescriptionTemplate", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                //var Message = param.Get<string>("@p_msg");
                //var Icon = param.Get<string>("@p_Icon");
                //if (Message != "")
                //{
                //    return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    return Json(true, JsonRequestBehavior.AllowGet);
                //}
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }



        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 473;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_RecruitmentJDTemplateID_Encrypted", "List");
                var data = DapperORM.ReturnList<Recruitment_JobDescriptionTemplate>("sp_List_Recruitment_JobDescriptionTemplate", param).ToList();
                ViewBag.JDTemplateList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        public ActionResult Delete(string RecruitmentJDTemplateID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_RecruitmentJDTemplateID_Encrypted", RecruitmentJDTemplateID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmploeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_JobDescriptionTemplate", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_Recruitment_JDTemplate", "ESS_Recruitment_JDTemplate");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
    }
}