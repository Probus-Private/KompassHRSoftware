using Dapper;
using KompassHR.Areas.ESS.Models.ESS_PMS;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IO;


namespace KompassHR.Areas.ESS.Controllers.ESS_PMS
{
    public class ESS_PMS_PerformanceApprovalController : Controller
    {
        DynamicParameters param = new DynamicParameters();

        // GET: ESS/ESS_PMS_PerformanceApproval
        public ActionResult ESS_PMS_PerformanceApproval(string OwnObjectiveCreationId_Encrypted,int ObjectivePeriodId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 855;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var param = new DynamicParameters();
                param.Add("@p_OwnObjectiveCreationId_Encrypted", OwnObjectiveCreationId_Encrypted);
                param.Add("@p_ObjectivePeriodId", ObjectivePeriodId);
                var PerformanceLog = DapperORM.DynamicList("sp_List_PMS_ESS_PerformanceApproval", param);
                ViewBag.PerformanceLog = PerformanceLog;

                var param1 = new DynamicParameters();
                param1.Add("@p_Origin", "OwnObjective");
                param1.Add("@p_OwnObjectiveCreationId_Encrypted", OwnObjectiveCreationId_Encrypted);
                param1.Add("@p_ObjectivePeriodId", ObjectivePeriodId);
                var EmpObjectiveInfo = DapperORM.DynamicList("sp_List_PMS_ESS_PerformanceApproval", param1);
                ViewBag.EmpObjectiveInfo = EmpObjectiveInfo;

                var param2 = new DynamicParameters();
                param2.Add("@p_Origin", "ApprovalRatings");
                param2.Add("@p_ObjectivePeriodId", ObjectivePeriodId); // ⭐ NEW

                var ApprovalRatings = DapperORM.DynamicList(
                    "sp_List_PMS_ESS_PerformanceApproval",
                    param2
                );

                ViewBag.ApprovalRatings = ApprovalRatings;


                //var paramPeriod = new DynamicParameters();
                //paramPeriod.Add("@p_EmployeeId", Session["EmployeeId"]);
                //paramPeriod.Add("@p_Origin", "Approval");

                //var approvalObj = DapperORM.DynamicList(
                //    "sp_List_PMS_ESS_PerformanceApproval",
                //    paramPeriod
                //);

                //// ✅ cast properly
                //var approvalList = approvalObj as IEnumerable<dynamic>;

                //ViewBag.CurrentApproval = approvalList;

                var paramPeriod = new DynamicParameters();
                paramPeriod.Add("@p_Origin", "ApprovalPeriod");
                paramPeriod.Add("@p_ObjectivePeriodId", ObjectivePeriodId);

                var approvalPeriodObj = DapperORM.DynamicList(
    "sp_List_PMS_ESS_PerformanceApproval",
    paramPeriod
);

                // ✅ cast object → IEnumerable<dynamic>
                var approvalPeriodList = approvalPeriodObj as IEnumerable<dynamic>;

                // ✅ now safe
                ViewBag.ApprovalPeriod = approvalPeriodList?.FirstOrDefault();

                var paramAlign = new DynamicParameters();
                paramAlign.Add("@p_Origin", "Alignment");
                paramAlign.Add("@p_OwnObjectiveCreationId_Encrypted", OwnObjectiveCreationId_Encrypted);

                var Alignment = DapperORM.DynamicList(
                    "sp_List_PMS_ESS_PerformanceApproval",
                    paramAlign
                );
                var AlignmentList = Alignment as IEnumerable<dynamic>;

                // ✅ now safe
                ViewBag.Alignment = AlignmentList?.FirstOrDefault();
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_PMS_GoalCreation");

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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 855;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_Origin", "Approval");
                var PMSApproval = DapperORM.DynamicList("sp_List_PMS_ESS_PerformanceApproval", param);
                ViewBag.PMSApproval = PMSApproval;
                return View();

                //var param = new DynamicParameters();
                //param.Add("@p_OwnObjectiveCreationId_Encrypted", OwnObjectiveCreationId_Encrypted);
                //var OwnObjectiveInfo = DapperORM.DynamicList("sp_List_PMS_ESS_PerformanceApproval", param);
                //ViewBag.OwnObjectiveInfo = OwnObjectiveInfo;
                //return View();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        public ActionResult ESS_PMS_PerformanceApproval(int DocId,decimal ManagerRating,string Remark,string Status)
        {
            DynamicParameters param = new DynamicParameters();

            param.Add("@p_ObjectivePeriodId", DocId);
            param.Add("@p_ManagerId", Convert.ToInt32(Session["EmployeeId"]));
            param.Add("@p_ManagerRemark", Remark);
            param.Add("@p_Status", Status);
            param.Add("@p_ManagerRating", ManagerRating);
            param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
            param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
            var Result = DapperORM.ExecuteReturn("sp_PMS_Period_ApproveReject", param);

            //DapperORM.ExecuteReturn("sp_PMS_Period_ApproveReject", param);

            TempData["Message"] = param.Get<string>("@p_msg");
            TempData["Icon"] = param.Get<string>("@p_Icon");

            return RedirectToAction("GetList", "ESS_PMS_PerformanceApproval", new { Area = "ESS" });
        }

        public ActionResult ViewPerformanceAttachment(long docId, string fileName)
        {
            try
            {
                var GetDocPath = DapperORM.DynamicQuerySingle(
                    "Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin = 'PmsPerformance'"
                );

                string basePath = GetDocPath.DocInitialPath;
                string fullPath = Path.Combine(basePath, docId.ToString(), fileName);

                if (!System.IO.File.Exists(fullPath))
                {
                    return HttpNotFound("File not found");
                }

                string contentType = MimeMapping.GetMimeMapping(fileName);

                return File(fullPath, contentType, fileName); // opens in browser / downloads
            }
            catch
            {
                return HttpNotFound();
            }
        }

    }
}