using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Dashboards.Controllers
{
    public class ESS_RecruitmentDashboardController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: Dashboards/ESS_RecruitmentDashboard


        public ActionResult ESS_RecruitmentDashboard()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null
                  ? Convert.ToInt32(Request.QueryString["ScreenId"])
                  : 645;

                bool CheckAccess = new BulkAccessClass()
                    .CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));

                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                var paramcards = new DynamicParameters();
                paramcards.Add("@Type", "Cards");
                var CardsData = DapperORM.ReturnList<dynamic>(
                                "sp_Recruitment_Dashboard",
                                paramcards
                             ).FirstOrDefault();
                if (CardsData != null)
                {
                    ViewBag.Applied = CardsData.Applied;
                    ViewBag.Shortlisted = CardsData.Shortlisted;
                    ViewBag.Interviewed = CardsData.Interviewed;
                    ViewBag.Offered = CardsData.Offered;
                    ViewBag.ShortlistedPercent = CardsData.ShortlistedPercent;
                    ViewBag.InterviewedPercent = CardsData.InterviewedPercent;
                    ViewBag.OfferedPercent = CardsData.OfferedPercent;
                }

                var paramTable = new DynamicParameters();
                paramTable.Add("@Type", "HiringAnalysis");

                var HiringAnalysis = DapperORM.ReturnList<dynamic>(
                                        "sp_Recruitment_Dashboard",
                                        paramTable
                                     );

                ViewBag.HiringAnalysis = HiringAnalysis;

                var paramOverview = new DynamicParameters();
                paramOverview.Add("@Type", "RecruitmentOverview");

                var overview = DapperORM.ReturnList<dynamic>(
                                "sp_Recruitment_Dashboard",
                                paramOverview
                              ).FirstOrDefault();

                if (overview != null)
                {
                    ViewBag.OfferAcceptance = overview.OfferAcceptance;
                    ViewBag.OverallHireRate = overview.OverallHireRate;
                }

                var paramChart = new DynamicParameters();
                paramChart.Add("@Type", "RecruitmentOverviewChart");

                var chartData = DapperORM.ReturnList<dynamic>(
                                    "sp_Recruitment_Dashboard",
                                    paramChart
                                ).FirstOrDefault();

                if (chartData != null)
                {
                    ViewBag.TotalApplications = chartData.TotalApplications;
                    ViewBag.FilledPositions = chartData.FilledPositions;
                }




                var paramRecruitmentCards = new DynamicParameters();
                paramRecruitmentCards.Add("@Type", "RECRUITMENTCARDS");

                var CardsData1 = DapperORM.ReturnList<dynamic>("sp_Recruitment_Dashboard", paramRecruitmentCards).FirstOrDefault();
                if (CardsData1 != null)
                {
                    ViewBag.OpenPositions = CardsData1.OpenPositions;
                    ViewBag.TotalCandidates = CardsData1.TotalCandidates;
                    ViewBag.InterviewsToday = CardsData1.InterviewsToday;
                    ViewBag.OffersReleased = CardsData1.OffersReleased;
                }
                var paramAvg = new DynamicParameters();
                paramAvg.Add("@Type", "AVERAGETIMEHIRE");

                var avgHire = DapperORM.ReturnList<dynamic>(
                                "sp_Recruitment_Dashboard",
                                paramAvg
                              ).FirstOrDefault();

                if (avgHire != null)
                {
                    ViewBag.AppliedToShortlisted = avgHire.AppliedToShortlisted;
                    ViewBag.ShortlistedToInterview = avgHire.ShortlistedToInterview;
                    ViewBag.InterviewToOffer = avgHire.InterviewToOffer;
                    ViewBag.OfferToAcceptance = avgHire.OfferToAcceptance;
                    ViewBag.FasterThanIndustry = avgHire.FasterThanIndustry;
                }

                var paramSchInterview = new DynamicParameters();
                paramSchInterview.Add("@Type", "ScheduledInterview");
                var ScheduleInterview = DapperORM.ReturnList<dynamic>(
                                        "sp_Recruitment_Dashboard",
                                        paramSchInterview
                                     );

                ViewBag.ScheduleInterview = ScheduleInterview;

                 
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
         

        //    public ActionResult ESS_RecruitmentDashboard()
        //    {
        //        try
        //        {
        //            if (Session["EmployeeId"] == null)
        //            {
        //                return RedirectToAction("Login", "Login", new { area = "" });
        //            }

            //            DynamicParameters param = new DynamicParameters();

            //            // GenderWise
            //            param.Add("@p_ReportType", "GenderWise");
            //            var genderWise = DapperORM.DynamicList("sp_RecruitmentDashboard", param);
            //            ViewBag.GenderWise = genderWise;

            //            // OpenVsWIP
            //            DynamicParameters param1 = new DynamicParameters();
            //            param1.Add("@p_ReportType", "OpenVsWIP");
            //            var openVsWip = DapperORM.DynamicList("sp_RecruitmentDashboard", param1);
            //            ViewBag.OpenVsWIP = openVsWip;

            //            // DeptWise
            //            DynamicParameters param2 = new DynamicParameters();
            //            param2.Add("@p_ReportType", "DeptWise");
            //            var deptWise = DapperORM.DynamicList("sp_RecruitmentDashboard", param2);
            //            ViewBag.DeptWise = deptWise;

            //            // PositionType
            //            DynamicParameters param3 = new DynamicParameters();
            //            param3.Add("@p_ReportType", "PositionType");
            //            var positionType = DapperORM.DynamicList("sp_RecruitmentDashboard", param3);
            //            ViewBag.PositionType = positionType;

            //            // CompanySummary
            //            DynamicParameters param4 = new DynamicParameters();
            //            param4.Add("@p_ReportType", "CompanySummary");
            //            var companySummary = DapperORM.DynamicList("sp_RecruitmentDashboard", param4);
            //            ViewBag.CompanySummary = companySummary;

            //            // 🆕 MONTHLY DEPT-WISE REQUEST SUMMARY (Current Year, Status = 'open')
            //            var currentYear = DateTime.Now.Year;
            //            var param5 = new DynamicParameters();
            //            param5.Add("@p_Year", currentYear);
            //            param5.Add("@p_Status", "open");
            //            ViewBag.DeptMonthlyData = DapperORM.DynamicList("sp_MonthlyDeptWiseRequests", param5);

            //            ViewBag.SelectedYear = currentYear;
            //            ViewBag.SelectedStatus = "open";

            //            var result = DapperORM.DynamicQuerySingle(@"
            //    SELECT 
            //        COUNT(*) AS Applied,
            //        SUM(CASE WHEN SendForShortListed = 1 THEN 1 ELSE 0 END) AS Shortlisted,
            //        SUM(CASE WHEN InterviewId IS NOT NULL AND InterviewId > 0 THEN 1 ELSE 0 END) AS Interviewed,
            //        SUM(CASE WHEN OfferLetterId IS NOT NULL AND OfferLetterId > 0 THEN 1 ELSE 0 END) AS Offered
            //    FROM Recruitment_Resume
            //    WHERE Deactivate = 0
            //");

            //            ViewBag.Applied = result.Applied;
            //            ViewBag.Shortlisted = result.Shortlisted;
            //            ViewBag.Interviewed = result.Interviewed;
            //            ViewBag.Offered = result.Offered;

            //            return View();
            //        }
            //        catch (Exception ex)
            //        {
            //            Session["GetErrorMessage"] = ex.Message;
            //            return RedirectToAction("ErrorPage", "Login");
            //        }
            //    }

        [HttpPost]
        public JsonResult GetMonthlyDeptWiseRequests(int year, string status)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Year", year);
                param.Add("@p_Status", status);

                var data = DapperORM.DynamicList("sp_MonthlyDeptWiseRequests", param);

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, message = ex.Message });
            }
        }

    }
}