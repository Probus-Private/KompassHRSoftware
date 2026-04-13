using Dapper;
using KompassHR.Areas.ESS.Models.ESS_PMS;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;


namespace KompassHR.Areas.ESS.Controllers.ESS_PMS
{
    public class ESS_PMS_PerformanceController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        // GET: ESS/ESS_PMS_Performance
        public ActionResult ESS_PMS_Performance()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 691;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var param = new DynamicParameters();
                DynamicParameters param4 = new DynamicParameters();
                param4.Add("@query", "SELECT KPIId as Id,KPIName as Name FROM PMS_KPIMaster WHERE Deactivate=0");
                var KPIList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param4).ToList();
                ViewBag.KPIList = KPIList;

                param.Add("@p_EmployeeId", Convert.ToInt32(Session["EmployeeId"]));

                var rows = DapperORM.DynamicList("sp_List_PMS_Objectives", param);
                var list = new List<dynamic>();

                foreach (IDictionary<string, object> row in rows)
                {
                    dynamic exp = new ExpandoObject();
                    var dict = (IDictionary<string, object>)exp;

                    foreach (var kv in row)
                        dict[kv.Key] = kv.Value;

                    list.Add(exp);
                }

                ViewBag.OwnObjectiveInfo = list;


                var param2 = new DynamicParameters();
                param2.Add("@query", "Select ObjectiveId,ObjectiveTitle from PMS_Objectives where Deactivate=0 and Origin='Individual'");
                var ObjectiveList = DapperORM.ReturnList<PMS_Objectives>("sp_QueryExcution", param2).ToList();
                ViewBag.ObjectiveList = ObjectiveList;

                //DynamicParameters param3 = new DynamicParameters();
                //param3.Add("@query", "select distinct(Category) as Name from PMS_KPIMaster where Deactivate=0");
                //var CategoryList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param3).ToList();
                //ViewBag.CategoryList = CategoryList;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_PMS_GoalCreation");
            }
        }

        public ActionResult ObjectiveWise(int ObjectiveId)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@p_ObjectiveId", ObjectiveId);
                param.Add("@p_EmployeeId", Convert.ToInt32(Session["EmployeeId"]));

                var result = DapperORM.DynamicList("sp_List_PMS_ObjectivesWiseKRAs", param);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult KRAUpdateForm(string origin, int FormId)
        {
            try
            {
                string sql;

                if (origin == "Own")
                {
                    sql = "SELECT obj.ObjectiveTitle, own.KRA, own.ObjectiveId, own.OwnObjectiveCreationId " +
                          "FROM PMS_OwnObjectiveCreation own " +
                          "LEFT JOIN PMS_Objectives obj ON own.ObjectiveId = obj.ObjectiveId " +
                          "WHERE own.Deactivate = 0 AND own.OwnObjectiveCreationId = @Id";
                }
                else
                {
                    sql = "SELECT obj.ObjectiveTitle, own.KRA, own.ObjectiveId, own.TeamObjectiveCreationId as OwnObjectiveCreationId " +
                          "FROM PMS_TeamObjectiveCreation own " +
                          "LEFT JOIN PMS_Objectives obj ON own.ObjectiveId = obj.ObjectiveId " +
                          "WHERE own.Deactivate = 0 AND own.TeamObjectiveCreationId = @Id";
                }

                var param = new DynamicParameters();
                param.Add("@Id", FormId);

                var data = DapperORM.Execute(sql, param);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SaveUpdate(PMS_Performance model, string ProcessType, HttpPostedFileBase file)
        {
            try
            {
                var param = new DynamicParameters();

                if (ProcessType == "FinalSubmit")
                    param.Add("@p_process", "FinalSubmit");
                else
                    param.Add("@p_process", "Save");   // ✅ FIX

                param.Add("@p_OwnObjectiveCreationId", model.OwnObjectiveCreationId);
                param.Add("@p_KraUpdatedDate", model.KraUpdatedDate);
                param.Add("@p_ActualValue", model.ActualValue);
                param.Add("@p_PercentageValue", Math.Round(model.PercentageValue, 2));
                param.Add("@p_Status", model.Status);
                param.Add("@p_Status", model.Status);
                param.Add("@p_Comment", model.Comment);
                param.Add("@p_ObjectivePeriodId", model.ObjectivePeriodId);
                param.Add("@p_CmpId", Session["CompanyId"]);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName());
                if (file != null)
                {
                    param.Add("@p_file", file.FileName);
                }
                else
                {
                    param.Add("@p_file", "");
                }
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 10);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                
                    var Result = DapperORM.ExecuteReturn("sp_SUD_PMS_Performance", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    var P_Id = param.Get<string>("@p_Id");

                if (P_Id != null)
                {
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin = 'PmsPerformance'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    var FirstPath = Path.Combine(GetFirstPath, P_Id.ToString());

                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }

                    if (file != null)
                    {
                        string Image1 = Path.Combine(FirstPath, file.FileName);
                        file.SaveAs(Image1);
                    }
                    

                }
                //DapperORM.ExecuteReturn("sp_SUD_PMS_Performance", param);

                return Json(new
                {
                    success = param.Get<string>("@p_Icon") == "success",
                    message = param.Get<string>("@p_msg"),
                    icon = param.Get<string>("@p_Icon")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public JsonResult GetObjectiveProgress(int ObjectiveId)
        {
            try
            {
                // STEP 1: Get all KRA IDs that belong to this single objective
                var kraList = DapperORM.DynamicQuerySingle(
                    @"SELECT ObjectiveId 
              FROM PMS_OwnObjectiveCreation 
              WHERE OwnObjectiveCreationId ='" + ObjectiveId + "' AND deactivate = 0");

                var Id = kraList.ObjectiveId;
                var avgProgress = DapperORM.DynamicQuerySingle(
                    $@"SELECT ISNULL(AVG(ProgressPercentage),0)
               FROM PMS_OwnObjectiveCreation
               WHERE deactivate = 0
               AND objectiveId='" + Id + "' ");

                return Json(avgProgress, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetKRAHistory(int OwnObjectiveId)
        {
            try
            {
                var kraList = DapperORM.DynamicQueryList(@"SELECT KraUpdatedDate,PercentageValue,ActualValue,Status,Comment,PerformanceId,Attachment AS AttachmentPath FROM PMS_Performance WHERE OwnObjectiveCreationId = '"+ OwnObjectiveId + "' AND Deactivate = 0");
                return Json(kraList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        public JsonResult GetObjectivePeriods(long ownObjectiveCreationId)
        {
            try
            {
                var param = new DynamicParameters();

                var list = DapperORM.DynamicQueryMultiple(@"SELECT 
                ObjectivePeriodId,
                PeriodLabel,
                PeriodFromDate,
                PeriodToDate,
                IsFinalSubmit,
                ApprovalStatus
            FROM PMS_ObjectivePeriod
            WHERE OwnObjectiveCreationId = '"+ ownObjectiveCreationId + "' AND Deactivate = 0 ORDER BY PeriodFromDate");

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ViewAttachment(string docId, string fileName)
        {
            try
            {
                // 🔹 Get base directory from DB
                var pathInfo = DapperORM.DynamicQuerySingle(@"
            SELECT DocInitialPath 
            FROM Tool_Documnet_DirectoryPath 
            WHERE DocOrigin = 'PmsPerformance'
        ");

                string basePath = pathInfo.DocInitialPath;   // physical root
                string fullPath = Path.Combine(basePath, docId, fileName);

                if (!System.IO.File.Exists(fullPath))
                    return HttpNotFound("File not found");

                string contentType = MimeMapping.GetMimeMapping(fullPath);

                return File(fullPath, contentType);
            }
            catch (Exception ex)
            {
                return Content("Unable to load attachment");
            }
        }


    }
}