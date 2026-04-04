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
    public class Setting_Preboarding_InstructionController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: Setting/Setting_Preboarding_Instruction
        public ActionResult Setting_Preboarding_Instruction(string PreInsructionId_Encrypted)
        {
         
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 40;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("query", "select CompanyName as Name,CompanyId as Id from Mas_CompanyProfile where Deactivate=0");
                var CompanyName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param);
                ViewBag.GetCompanyName = CompanyName;
                ViewBag.AddUpdateTitle = "Add";
                Onboarding_Instruction Setting_Preboarding_Instruction = new Onboarding_Instruction();
                if (PreInsructionId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_PreInsructionId_Encrypted", PreInsructionId_Encrypted);
                    Setting_Preboarding_Instruction = DapperORM.ReturnList<Onboarding_Instruction>("sp_List_Onboarding_Instruction", param1).FirstOrDefault();
                }

                return View(Setting_Preboarding_Instruction);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult IsPreboardingInstructionExists(string Remark, string PreInsructionId_Encrypted,int ddlCompanyId)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_Remark", Remark);
                    param.Add("@p_PreInsructionId_Encrypted", PreInsructionId_Encrypted);
                    param.Add("@p_CmpId", ddlCompanyId);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", "Admin");
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Onboarding_Instruction", param);
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

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpPost]
        public ActionResult SaveUpdate(Onboarding_Instruction ObjOnboarding_Instruction)
        {

            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(ObjOnboarding_Instruction.PreInsructionId_Encrypted) ? "Save" : "Update");
                param.Add("@p_PreInsructionId", ObjOnboarding_Instruction.PreInsructionId);
                param.Add("@p_PreInsructionId_Encrypted", ObjOnboarding_Instruction.PreInsructionId_Encrypted);
                param.Add("@p_Remark", ObjOnboarding_Instruction.Remark);
                param.Add("@p_CmpId", ObjOnboarding_Instruction.CmpID);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", "Admin");
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Onboarding_Instruction", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Preboarding_Instruction", "Setting_Preboarding_Instruction");
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
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 40;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_PreInsructionId_Encrypted", "List");
                var data = DapperORM.ReturnList<Onboarding_Instruction>("sp_List_Onboarding_Instruction", param).ToList();
                ViewBag.GetPreboardingInstructionList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult Delete(string PreInsructionId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_PreInsructionId_Encrypted", PreInsructionId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Onboarding_Instruction", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Preboarding_Instruction");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
    }
}