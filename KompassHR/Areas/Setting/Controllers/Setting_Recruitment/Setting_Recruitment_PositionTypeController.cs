using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_Recruitment;
using KompassHR.Models;
using System.Net;
using System.Data;
using System.Data.SqlClient;

namespace KompassHR.Areas.Setting.Controllers.Setting_Recruitment
{
    public class Setting_Recruitment_PositionTypeController : Controller
    {
        // GET: Setting/Setting_Recruitment_PositionType
        public ActionResult Setting_Recruitment_PositionType(string PositionTypeID_Encrypted)
        {
            try
            {
                if(Session["EmployeeId"]==null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 246;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Recruitment_PositionType RecruitmentPositionType = new Recruitment_PositionType();
                if(PositionTypeID_Encrypted!=null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    DynamicParameters param = new DynamicParameters();

                    param.Add("@p_positionTypeID_Encrypted", PositionTypeID_Encrypted);
                    RecruitmentPositionType = DapperORM.ReturnList<Recruitment_PositionType>("sp_List_Recruitment_PositionType", param).FirstOrDefault();
                    ViewBag.GetPositionTypeList = RecruitmentPositionType;
                }
                return View(RecruitmentPositionType);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        //Position Type is Exists
        public ActionResult IsPositionTypeExists(string PositionType,string PositionTypeID_Encrypted)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                   if(PositionTypeID_Encrypted=="")
                    {
                        DynamicParameters param = new DynamicParameters();
                        param.Add("@p_process", "IsValidation");
                        param.Add("@p_positionType", PositionType);
                        param.Add("@p_positionTypeID_Encrypted", PositionTypeID_Encrypted);
                        param.Add("@p_MachineName", Dns.GetHostName().ToString());
                        param.Add("@p_CreatedupdateBy", "Admin");
                        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                        var result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_PositionType", param);
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
        // Save Update Position type
        public ActionResult SaveUpdatePositionType(Recruitment_PositionType RecruitmentPositionType)
        {
            try
            {
                if(Session["EmployeeId"]==null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", string.IsNullOrEmpty(RecruitmentPositionType.PositionTypeID_Encrypted) ? "Save" : "Update");
                param.Add("@p_positionType", RecruitmentPositionType.PositionType);
                param.Add("@p_positionTypeID_Encrypted", RecruitmentPositionType.PositionTypeID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedupdateBy", "Admin");
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_PositionType", param);
                var msg = param.Get<string>("@p_msg");
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                return RedirectToAction("Setting_Recruitment_PositionType", "Setting_Recruitment_PositionType");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        //Position Type List
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 246;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "List");
                var result = DapperORM.ReturnList<Recruitment_PositionType>("sp_List_Recruitment_PositionType", param).ToList();
                ViewBag.GetPositionTypeList = result;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
            
        }
        //Delete Position Type
        public ActionResult Delete(string PositionTypeID_Encrypted)
        {
            try
            {
                if(Session["EmployeeId"]==null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Delete");
                param.Add("@p_positionTypeID_Encrypted", PositionTypeID_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_PositionType", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                return RedirectToAction("GetList", "Setting_Recruitment_PositionType");

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
    }
}