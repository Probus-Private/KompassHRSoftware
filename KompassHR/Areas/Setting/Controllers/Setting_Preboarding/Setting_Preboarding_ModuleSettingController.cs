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
    public class Setting_Preboarding_ModuleSettingController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_Preboarding_ModuleSetting
        #region ModuleSetting Main View
        [HttpGet]
        public ActionResult Setting_Preboarding_ModuleSetting(string ModuleSettingID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 42;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Preboarding_ModuleSetting objPreboarding_ModuleSetting = new Preboarding_ModuleSetting();
                param.Add("@query", "Select CompanyId As Id,CompanyName As Name from Mas_CompanyProfile Where Deactivate = 0 order by Name");
                var listMas_CompanyProfile = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetCompanyname = listMas_CompanyProfile;

                if (ModuleSettingID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_ModuleSettingID_Encrypted", ModuleSettingID_Encrypted);
                    objPreboarding_ModuleSetting = DapperORM.ReturnList<Preboarding_ModuleSetting>("sp_List_Preboarding_ModuleSetting", param).FirstOrDefault();
                }

                return View(objPreboarding_ModuleSetting);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        [HttpGet]
        public ActionResult IsModuleSettingExists(string ModuleSettingID_Encrypted, string CompulsoryModule)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_CompulsoryModule", CompulsoryModule);
                    param.Add("@p_ModuleSettingID_Encrypted", ModuleSettingID_Encrypted);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Preboarding_ModuleSetting", param);
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

        #region SaveUpdate 
        [HttpPost]
        public ActionResult SaveUpdate(Preboarding_ModuleSetting ObjPreboarding_ModuleSetting)
        {
            try
            {

                param.Add("@p_process", string.IsNullOrEmpty(ObjPreboarding_ModuleSetting.ModuleSettingID_Encrypted) ? "Save" : "Update");
                param.Add("@p_ModuleSettingID", ObjPreboarding_ModuleSetting.ModuleSettingID);
                param.Add("@p_ModuleSettingID_Encrypted", ObjPreboarding_ModuleSetting.ModuleSettingID_Encrypted);
                //param.Add("@p_CmpId", ObjPreboarding_ModuleSetting.CmpId);
                param.Add("@p_CompulsoryModule", ObjPreboarding_ModuleSetting.CompulsoryModule);
                param.Add("@p_Personal", ObjPreboarding_ModuleSetting.Personal);
                param.Add("@p_Address", ObjPreboarding_ModuleSetting.Address);
                param.Add("@p_Reference", ObjPreboarding_ModuleSetting.Reference);
                param.Add("@p_Bank", ObjPreboarding_ModuleSetting.Bank);
                param.Add("@p_Statutory", ObjPreboarding_ModuleSetting.Statutory);
                param.Add("@p_Family", ObjPreboarding_ModuleSetting.Family);
                param.Add("@p_Qualification", ObjPreboarding_ModuleSetting.Qualification);
                param.Add("@p_UploadDocument", ObjPreboarding_ModuleSetting.UploadDocument);
                param.Add("@p_Photo", ObjPreboarding_ModuleSetting.Photo);
                param.Add("@p_Signature", ObjPreboarding_ModuleSetting.Signature);
                param.Add("@p_PreEmployer", ObjPreboarding_ModuleSetting.PreEmployer);
                param.Add("@p_Skill", ObjPreboarding_ModuleSetting.Skill);
                param.Add("@p_Language", ObjPreboarding_ModuleSetting.Language);
                param.Add("@p_Other", ObjPreboarding_ModuleSetting.Other);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Preboarding_ModuleSetting", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return RedirectToAction("Setting_Preboarding_ModuleSetting", "Setting_Preboarding_ModuleSetting");

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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 42;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_ModuleSettingID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Preboarding_ModuleSetting", param);
                ViewBag.GetModuleSetting = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion


        public ActionResult Delete(string ModuleSettingID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_ModuleSettingID_Encrypted", ModuleSettingID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Preboarding_ModuleSetting", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Preboarding_ModuleSetting");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
    }
}