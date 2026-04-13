using Dapper;
using KompassHR.Areas.ESS.Models.ESS_PMS;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_PMS
{
    public class ESS_PMS_FinalEvaluationController : Controller
    {
        DynamicParameters param = new DynamicParameters();

        public ActionResult ESS_PMS_FinalEvaluation(int EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    TempData["Message"] = "Your session has expired. Please log in again.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null? Convert.ToInt32(Request.QueryString["ScreenId"]) : 877;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
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
                ViewBag.KraList = DapperORM.DynamicList("sp_PMS_KRA_FinalCalculation", param1);
                ViewBag.YearId = yearId;
                ViewBag.EmployeeId = EmployeeId;
                int reviewerId = Convert.ToInt32(Session["EmployeeId"]);
                ViewBag.reviewerId = reviewerId;

                var OutOfRatingValue = "select Weightage from PMS_Weightage where Deactivate=0";
                var OutOfRating1 = DapperORM.DynamicQuerySingle(OutOfRatingValue);
                int OutOfRating = Convert.ToInt32(OutOfRating1.Weightage);
                ViewBag.OutOfRating = OutOfRating;
                var empParam = new DynamicParameters();
                empParam.Add("@p_ReviewerEmployeeId", reviewerId);
                empParam.Add("@p_TargetEmployeeId", EmployeeId);
                empParam.Add("@p_Process", "EmpInfo");
                ViewBag.EmpInfo = DapperORM.DynamicList("sp_List_PMS_CompetencyFinalEvaluation", empParam);

                var compParam = new DynamicParameters();
                compParam.Add("@p_ReviewerEmployeeId", reviewerId);
                compParam.Add("@p_TargetEmployeeId", EmployeeId);
                compParam.Add("@p_Process", "CompetencyInfo");
                ViewBag.CompetencyInfo = DapperORM.DynamicList("sp_List_PMS_CompetencyFinalEvaluation", compParam);

                return View();
            }
            catch (Exception ex)
            {
                TempData["Message"] = "An error occurred: " + ex.Message;
                TempData["Icon"] = "error";
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

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

                int screenId = Request.QueryString["ScreenId"] != null
                    ? Convert.ToInt32(Request.QueryString["ScreenId"])
                    : 877;

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
                DynamicParameters paramlist = new DynamicParameters();
                paramlist.Add("@p_ReviewerEmployeeId", Session["EmployeeId"]);
                paramlist.Add("@p_PMSYearId", yearId);
                paramlist.Add("@p_Origin", "List");
                var data = DapperORM.DynamicList("sp_List_PMS_FinalEvaluation", paramlist);
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

        public ActionResult IsValidation(int EmployeeId,int YearId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "IsValidation");
                param.Add("@p_EmployeeId", EmployeeId);
                param.Add("@p_PMS_YearId", YearId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_PMS_FinalEvaluation", param);
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
                TempData["Message"] = "An error occurred: " + ex.Message;
                TempData["Icon"] = "error";
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        [HttpPost]
        public ActionResult FinalSubmit(PMS_FinalEvaluation model)
        {
            try
            {
                if (model.EmployeeId == null)
                {
                    TempData["Message"] = "Session expired.";
                    return RedirectToAction("GetList");
                }

                DynamicParameters param = new DynamicParameters();

                param.Add("@p_process", "SAVE");
                param.Add("@p_CreatedBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName());

                param.Add("@p_PMS_YearId", model.YearId);
                param.Add("@p_EmployeeId", model.EmployeeId);
                param.Add("@p_ReviewerId", model.ReviewerId);
                param.Add("@p_OutOfRating", model.OutOfRating);
                param.Add("@p_KRAFinalRating", model.KRAFinalRating);
                param.Add("@p_CompetencyFinalRating", model.CompetencyFinalRating);
                param.Add("@p_TotalFinalRating", model.TotalFinalRating);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 300);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
                var Result = DapperORM.ExecuteReturn("sp_SUD_PMS_FinalEvaluation", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                // EXEC sp_SUD_PMS_FinalEvaluation (Dapper)

                return RedirectToAction("GetList", "ESS_PMS_FinalEvaluation", new { area = "ESS" });

            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return RedirectToAction("GetList");
            }
        }
    }
}
