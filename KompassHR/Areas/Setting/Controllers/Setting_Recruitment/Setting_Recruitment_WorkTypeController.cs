using Dapper;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_Recruitment;
using KompassHR.Models;
using System.Net;

namespace KompassHR.Areas.Setting.Controllers.Setting_Recruitment
{
    public class Setting_Recruitment_WorkTypeController : Controller
    {
        // GET: Setting/Setting_Recruitment_WorkType
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region Setting_Recruitment_WorkType
        public ActionResult Setting_Recruitment_WorkType(string WorkTypeID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 247;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Recruitment_WorkType RecruitmentWorkType = new Recruitment_WorkType();
                if (WorkTypeID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_WorkTypeID_Encrypted", WorkTypeID_Encrypted);
                    RecruitmentWorkType = DapperORM.ReturnList<Recruitment_WorkType>("sp_List_Recruitment_WorkType", param).FirstOrDefault();
                }
                return View(RecruitmentWorkType);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsWorkTypeExists
        public ActionResult IsWorkTypeExists(string WorkType, string WorkTypeID_Encrypted)
        {
            try
            {
                if (WorkTypeID_Encrypted == "")
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_WorkType", WorkType);
                    param.Add("@p_WorkTypeID_Encrypted", WorkTypeID_Encrypted);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedupdateBy", "Admin");
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_WorkType", param);
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
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region SaveUpdateWorkType
        public ActionResult SaveUpdateWorkType(Recruitment_WorkType RecruitmentWorkType)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                param.Add("@p_process", string.IsNullOrEmpty(RecruitmentWorkType.WorkTypeID_Encrypted) ? "Save" : "Update");
                param.Add("@p_WorkTypeID", RecruitmentWorkType.WorkTypeID);
                param.Add("@p_WorkTypeID_Encrypted", RecruitmentWorkType.WorkTypeID_Encrypted);
                param.Add("@p_WorkType", RecruitmentWorkType.WorkType);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", "Admin");
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_WorkType", param);
                var msg = param.Get<string>("@p_msg");
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Recruitment_WorkType", "Setting_Recruitment_WorkType");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 247;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "List");
                var result = DapperORM.ReturnList<Recruitment_WorkType>("sp_List_Recruitment_WorkType", param).ToList();
                ViewBag.GetWorkTypeList = result;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(string WorkTypeID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Delete");
                param.Add("@p_WorkTypeID_Encrypted", WorkTypeID_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_WorkType", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Recruitment_WorkType");
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