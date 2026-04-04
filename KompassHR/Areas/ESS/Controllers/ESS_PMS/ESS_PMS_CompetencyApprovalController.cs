using Dapper;
using KompassHR.Areas.ESS.Models.ESS_PMS;
using KompassHR.Areas.Setting.Models.Setting_PMS;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_PMS
{
    public class ESS_PMS_CompetencyApprovalController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: ESS/ESS_PMS_CompetencyApproval
        public ActionResult ESS_PMS_CompetencyApproval()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 830;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_ManagerId", Session["EmployeeId"]);
                var result = DapperORM.ReturnList<dynamic>("sp_List_PMS_EmployeeCompetency_Approval", param).ToList();
                ViewBag.GetApprover = result;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult ESS_PMS_ManagerRating(string EmployeeCompetencyId_Encrypted)
        {
            try
            {
                param.Add("@p_EmployeeCompetencyId_Encrypted", EmployeeCompetencyId_Encrypted);
                var result = DapperORM.ReturnList<dynamic>("sp_List_PMS_CompetencyDetails", param).ToList();
                ViewBag.GetList = result;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult SaveManagerRating(PMS_EmployeeCompetency model)
        {
            try
            {
                if (model.EmployeeCompetencyId == 0)
                {
                    return Json(new { msg = "Document ID not received.", icon = "error" });
                }

                DynamicParameters param5 = new DynamicParameters();

                param5.Add("@p_PmsCompetencyMapId", model.EmployeeCompetencyId);
                param5.Add("@p_ManagerId", Convert.ToInt64(Session["EmployeeId"]));
                param5.Add("@p_ManagerRating", model.ManagerRating);
                param5.Add("@p_ManagerRemark", model.ManagerRemark);
                param5.Add("@p_Status", "Approved");
                     
                param5.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                param5.Add("@p_icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);

                var Result = DapperORM.ExecuteSP<dynamic>("sp_PMS_Competency_ApproveReject", param5);
                TempData["Message"] = param5.Get<string>("@p_msg");
                TempData["Icon"] = param5.Get<string>("@p_icon");
                return Json(new
                {
                    msg = TempData["Message"],
                    icon = TempData["Icon"]
                });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }



    }
}