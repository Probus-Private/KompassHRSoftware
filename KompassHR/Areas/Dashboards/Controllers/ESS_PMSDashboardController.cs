using Dapper;
using KompassHR.Areas.Dashboards.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Dashboards.Controllers
{
    public class ESS_PMSDashboardController : Controller
    {
        // GET: Dashboards/ESS_PMSDashboard
        public ActionResult ESS_PMSDashboard()
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
                    : 901;

                bool CheckAccess = new BulkAccessClass()
                    .CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));

                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var query = @"SELECT Fid FROM PMS_Year WHERE IsActive=1";
                var YearId = DapperORM.DynamicQuerySingle(query);
                ViewBag.Year = YearId.Fid;

                var param = new DynamicParameters();
                param.Add("@Type", "COMPANY");
                param.Add("@YearId", YearId.Fid);

                var data = DapperORM.ReturnList<dynamic>(
                    "sp_PMSDashboard",
                    param
                ).ToList();

                ViewBag.CompanyObjectives = data;

                var paramSummary = new DynamicParameters();
                paramSummary.Add("@Type", "SUMMARY");
                paramSummary.Add("@YearId", YearId.Fid);

                var summary = DapperORM.ReturnList<dynamic>(
                                "sp_PMSDashboard",
                                paramSummary
                             ).FirstOrDefault();

                ViewBag.Completed = summary.CompletedEmployeeCount;
                ViewBag.NotCompleted = summary.NotCompletedEmployeeCount;
                ViewBag.AvgFinalRating = summary.AvgFinalRating;
                ViewBag.AvgKraRating = summary.AvgKRARating;
                ViewBag.AvgCompetency = summary.AvgCompetencyRating;

                var goalParam = new DynamicParameters();
                goalParam.Add("@Type", "GOALSTATUS");
                goalParam.Add("@YearId", YearId.Fid);

                var goalStatus = DapperORM.ReturnList<dynamic>(
                                    "sp_PMSDashboard",
                                    goalParam
                                 ).ToList();

                ViewBag.GoalStatus = goalStatus;

                var heatParam = new DynamicParameters();
                heatParam.Add("@Type", "DEPTHEATMAP");
                heatParam.Add("@YearId", YearId.Fid);

                var heatmap = DapperORM.ReturnList<dynamic>(
                                    "sp_PMSDashboard",
                                    heatParam
                              ).ToList();

                ViewBag.DeptHeatmap = heatmap;
                var paramDept = new DynamicParameters();
                paramDept.Add("@Type", "DEPTGOALSTATUS");
                paramDept.Add("@YearId", YearId.Fid);

                var deptStatus = DapperORM.ReturnList<dynamic>(
                                    "sp_PMSDashboard",
                                    paramDept
                                 ).ToList();
                ViewBag.DeptGoalStatus = deptStatus;

                var performerparam = new DynamicParameters();
                performerparam.Add("@p_YearId", YearId.Fid);
                performerparam.Add("@p_TargetType", "Quarterly");

                var performers = DapperORM.ReturnList<dynamic>(
                    "sp_PMS_HighLowPerformers",
                    performerparam
                ).ToList();

                ViewBag.HighPerformers = performers
                    .Where(x => x.PerformanceType == "Top Performer")
                    .ToList();

                ViewBag.LowPerformers = performers
                    .Where(x => x.PerformanceType == "Low Performer")
                    .ToList();
                return View();
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                TempData["Icon"] = "error";
                return RedirectToAction("ESS_PMSDashboard", new { area = "ESS" });
            }
        }

        public JsonResult GetObjectiveChildren(string origin, long parentId)
        {
            var query = @"SELECT Fid FROM PMS_Year WHERE IsActive=1";
            var YearId = DapperORM.DynamicQuerySingle(query);

            var param = new DynamicParameters();
            param.Add("@Type", origin);   // TEAM or OWN
            param.Add("@ParentId", parentId);
            param.Add("@YearId", YearId.Fid);

            var data = DapperORM.ReturnList<dynamic>(
                            "sp_PMSDashboard",
                            param
                        ).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}