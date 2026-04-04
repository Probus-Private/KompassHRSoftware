using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_FullAndFinal;
using KompassHR.Models;
using Dapper;
using System.Net;
using System.Data;
using System.Data.SqlClient;

namespace KompassHR.Areas.Setting.Controllers.Setting_FullAndFinal
{
    public class Setting_FullAndFinal_NoticePeriodController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: FNF/NoticePeriod
        #region NoticePeriod MAin View
        [HttpGet]
        public ActionResult Setting_FullAndFinal_NoticePeriod(string NoticePeriodId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 70;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                FNF_NoticePeriod FNF_noticeperiod = new FNF_NoticePeriod();
                param.Add("@query", "Select GradeId,GradeName from Mas_Grade Where Deactivate=0");
                var listMas_Grade = DapperORM.ReturnList<Mas_Grade>("sp_QueryExcution", param).ToList();
                ViewBag.GetGradeName = listMas_Grade;

                param.Add("@query", "Select DesignationId as Id,DesignationName as [Name] from Mas_Designation Where Deactivate=0");
                var List_Designation = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetDesignationName = List_Designation;

                param.Add("@query", "Select DepartmentId as Id,DepartmentName as [Name] from Mas_Department Where Deactivate=0");
                var List_Department = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetDepartmentName = List_Department;

                if (NoticePeriodId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_NoticePeriodId_Encrypted", NoticePeriodId_Encrypted);
                    FNF_noticeperiod = DapperORM.ReturnList<FNF_NoticePeriod>("sp_List_FNF_NoticePeriod", param).FirstOrDefault();
                }
                return View(FNF_noticeperiod);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsValidation
        [HttpGet]
        public ActionResult IsNoticePeriodExists(double NoticePeriodGradeId, string NoticePeriodId_Encrypted, double DeptID, double DesgID)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_NoticePeriodId_Encrypted", NoticePeriodId_Encrypted);
                param.Add("@P_NoticePeriodGradeId", NoticePeriodGradeId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@P_DepartmentID", DeptID);
                param.Add("@P_DesignationID", DesgID);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_FNF_NoticePeriod", param);
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
        [HttpPost]
        public ActionResult SaveUpdate(FNF_NoticePeriod NoticePeriod)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(NoticePeriod.NoticePeriodId_Encrypted) ? "Save" : "Update");
                param.Add("@P_NoticePeriodId", NoticePeriod.NoticePeriodId);
                param.Add("@p_NoticePeriodId_Encrypted", NoticePeriod.NoticePeriodId_Encrypted);
                param.Add("@P_NoticePeriodGradeId", NoticePeriod.NoticePeriodGradeId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@P_NoticePeriodDays", NoticePeriod.NoticePeriodDays);
                param.Add("@P_DepartmentID", NoticePeriod.DepartmentID);
                param.Add("@P_DesignationID", NoticePeriod.DesignationID);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_FNF_NoticePeriod]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Setting_FullAndFinal_NoticePeriod", "Setting_FullAndFinal_NoticePeriod");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetList
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 70;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_NoticePeriodId_Encrypted", "List");
                var data = DapperORM.ReturnList<FNF_NoticePeriod>("sp_List_FNF_NoticePeriod", param).ToList();
                ViewBag.GetNoticePeriodList = data;
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
        [HttpGet]
        public ActionResult Delete(string NoticePeriodId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@P_NoticePeriodId_Encrypted", NoticePeriodId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_FNF_NoticePeriod", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_FullAndFinal_NoticePeriod");
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