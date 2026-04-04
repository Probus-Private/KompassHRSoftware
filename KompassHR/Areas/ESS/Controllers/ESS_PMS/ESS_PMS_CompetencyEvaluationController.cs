using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_PMS
{
    public class ESS_PMS_CompetencyEvaluationController : Controller
    {
        // GET: ESS/ESS_PMS_CompetencyEvaluation
        public ActionResult ESS_PMS_CompetencyEvaluation(int EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                    return RedirectToAction("Login", "Login");

                int reviewerId = Convert.ToInt32(Session["EmployeeId"]);
                var empParam = new DynamicParameters();
                empParam.Add("@p_ReviewerEmployeeId", reviewerId);
                empParam.Add("@p_TargetEmployeeId", EmployeeId);
                empParam.Add("@p_Process", "EmpInfo");
                ViewBag.EmpInfo = DapperORM.DynamicList("sp_List_PMS_CompetencyFinalEvaluation",empParam);
                var compParam = new DynamicParameters();
                compParam.Add("@p_ReviewerEmployeeId", reviewerId);
                compParam.Add("@p_TargetEmployeeId", EmployeeId);
                compParam.Add("@p_Process", "CompetencyInfo");
                ViewBag.CompetencyInfo = DapperORM.DynamicList("sp_List_PMS_CompetencyFinalEvaluation",compParam);
                return View();
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
                DynamicParameters paramlist = new DynamicParameters();
                paramlist.Add("@p_ReviewerEmployeeId", Session["EmployeeId"]);
                paramlist.Add("@p_Process", "List");
                var data = DapperORM.DynamicList("sp_List_PMS_CompetencyFinalEvaluation", paramlist);
                ViewBag.GetList = data;
                return View();
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                TempData["Icon"] = "error";
                return RedirectToAction("GetList", "ESS_PMS_CompetencyEvaluation", new { area = "ESS" });
            }         
        }
    }
}