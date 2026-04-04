using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_PMS
{
    public class ESS_PMS_KRAEvaluationController : Controller
    {
        DynamicParameters param = new DynamicParameters();

        //GET: ESS/ESS_PMS_KRAEvaluation
        public ActionResult ESS_PMS_KRAEvaluation(int EmployeeId)
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
                    : 862;

                bool CheckAccess = new BulkAccessClass()
                    .CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));

                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                var yearQuery = "SELECT Fid FROM PMS_Year WHERE IsActive = 1";
                var yearObj = DapperORM.DynamicQuerySingle(yearQuery);
                int yearId = Convert.ToInt32(yearObj.Fid);
               
                var param1 = new DynamicParameters();
                param1.Add("@p_EmployeeId", EmployeeId);
                param1.Add("@p_PMSYearId", yearId);

                ViewBag.KraList =DapperORM.DynamicList("sp_PMS_KRA_FinalCalculation", param1);
                return View();
            }
            catch (Exception ex)
            {
                TempData["Message"] = "An error occurred: " + ex.Message;
                TempData["Icon"] = "error";
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        //public ActionResult ESS_PMS_FinalEvaluation(int EmployeeId)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //            return RedirectToAction("Login", "Login");

        //        int reviewerId = Convert.ToInt32(Session["EmployeeId"]);

        //        /* ================= EMPLOYEE INFO ================= */
        //        var empParam = new DynamicParameters();
        //        empParam.Add("@p_ReviewerEmployeeId", reviewerId);
        //        empParam.Add("@p_TargetEmployeeId", EmployeeId);
        //        empParam.Add("@p_Process", "EmpInfo");

        //        ViewBag.EmpInfo = DapperORM.DynamicList(
        //            "sp_List_PMS_CompetencyFinalEvaluation",
        //            empParam
        //        );

        //        /* ================= COMPETENCY ================= */
        //        var compParam = new DynamicParameters();
        //        compParam.Add("@p_ReviewerEmployeeId", reviewerId);
        //        compParam.Add("@p_TargetEmployeeId", EmployeeId);
        //        compParam.Add("@p_Process", "CompetencyInfo");

        //        ViewBag.CompetencyInfo = DapperORM.DynamicList(
        //            "sp_List_PMS_CompetencyFinalEvaluation",
        //            compParam
        //        );

        //        /* ================= KRA ================= */
        //        var yearQuery = "SELECT Fid FROM PMS_Year WHERE IsActive = 1";
        //        var yearObj = DapperORM.DynamicQuerySingle(yearQuery);
        //        int yearId = Convert.ToInt32(yearObj.Fid);

        //        var kraParam = new DynamicParameters();
        //        kraParam.Add("@p_EmployeeId", EmployeeId);
        //        kraParam.Add("@p_PMSYearId", yearId);

        //        ViewBag.KraList =
        //            DapperORM.DynamicList("sp_PMS_KRA_FinalCalculation", kraParam);

        //        return View();
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
        //}


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
                int EmpId = Convert.ToInt32(Session["EmployeeId"]);
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 862;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "List");
                param.Add("@p_EmployeeId", EmpId);
                var data = DapperORM.DynamicList("sp_List_PMS_FinalEvaluation", param);
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
    }
}