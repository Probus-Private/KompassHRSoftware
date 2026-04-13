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

namespace KompassHR.Areas.ESS.Controllers.ESS_PMS
{
    public class ESS_PMS_OwnGoalCreationController : Controller
    {
        DynamicParameters param = new DynamicParameters();

        // GET: ESS/ESS_PMS_OwnGoalCreation
        [HttpGet]
        public ActionResult ESS_PMS_OwnGoalCreation(string OwnObjectiveCreationId_Encrypted)
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 603;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                var param2 = new DynamicParameters();
                param2.Add("@query", "Select ObjectiveId,ObjectiveTitle from PMS_Objectives where Deactivate=0 and Origin='Individual' and EmployeeId='" + EmpId + "'");
                var ObjectiveList = DapperORM.ReturnList<PMS_Objectives>("sp_QueryExcution", param2).ToList();
                ViewBag.ObjectiveList = ObjectiveList;
                ViewBag.AddUpdateTitle = "Add";

                var weightageParam = new DynamicParameters();
                var safeEmployeeId = EmpId.ToString().Replace("'", "''");
                var weightageQuery = @"SELECT SUM(CAST(Weightage AS decimal(10,2))) AS TotalWeightage FROM PMS_OwnObjectiveCreation WHERE EmployeeId = '" + safeEmployeeId + "' AND deactivate=0 and Weightage IS NOT NULL";
                weightageParam.Add("@query", weightageQuery);
                var result = DapperORM.ReturnList<decimal?>("sp_QueryExcution", weightageParam).FirstOrDefault();
                decimal totalWeightage = result ?? 0;
                ViewBag.TotalWeightage = totalWeightage;

                DynamicParameters param3 = new DynamicParameters();
                param.Add("@query", "select distinct(Category) as Name from PMS_KPIMaster where Deactivate=0");
                var CategoryList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.CategoryList = CategoryList;

                DynamicParameters param4 = new DynamicParameters();
                param.Add("@query", "SELECT KPIId as Id,KPIName as Name FROM PMS_KPIMaster WHERE Deactivate=0");
                var KPIList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.KPIList = KPIList;

                DynamicParameters param6 = new DynamicParameters();
                param6.Add("@query", "select TargetTypeId AS Id,TargetType AS Name from PMS_TargetType where Deactivate=0");
                var TragetTypeList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param6).ToList();
                ViewBag.TragetTypeList = TragetTypeList;

                DynamicParameters param7 = new DynamicParameters();
                param7.Add("@query", "select UOM as Name from PMS_UOM where Deactivate=0");
                var UOMList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param7).ToList();
                ViewBag.UOMList = UOMList;

                //DynamicParameters param8 = new DynamicParameters();
                //param8.Add("@query", "SELECT Fid FROM PMS_Year WHERE IsActive=1");
                //var Year =
                //ViewBag.UOMList = UOMList;

                var query = @"SELECT Fid FROM PMS_Year WHERE IsActive=1";
                //var Fid = query.FirstOrDefault();
                var YearId = DapperORM.DynamicQuerySingle(query);
                ViewBag.Year = YearId.Fid;

                PMS_OwnObjectiveCreation PMS_OwnObjectiveCreation = new PMS_OwnObjectiveCreation();
                if (OwnObjectiveCreationId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    var param = new DynamicParameters();
                    param.Add("@p_OwnObjectiveCreationId_Encrypted", OwnObjectiveCreationId_Encrypted);
                    PMS_OwnObjectiveCreation = DapperORM.ReturnList<PMS_OwnObjectiveCreation>("sp_List_PMS_OwnObjectiveCreation", param).FirstOrDefault();
                    TempData["p_Id"] = PMS_OwnObjectiveCreation.OwnObjectiveCreationId;
                    ViewBag.IsAlign = PMS_OwnObjectiveCreation.IsAlign;
                    ViewBag.AlignWith = PMS_OwnObjectiveCreation.AlignWith;
                    ViewBag.ObjectiveId = PMS_OwnObjectiveCreation.ObjectiveId;
                    ViewBag.AlignWithType = PMS_OwnObjectiveCreation.AlignWithType;
                }

                return View(PMS_OwnObjectiveCreation);
            }
            catch (Exception ex)
            {
                TempData["Message"] = "An error occurred: " + ex.Message;
                TempData["Icon"] = "error";
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult OriginWiseObjective(string Origin)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }

            int EmployeeId = Convert.ToInt32(Session["EmployeeId"]);

            DynamicParameters param = new DynamicParameters();
            string query = "";

            if (Origin == "Team")
            {
                query = @"
         SELECT 
    team.OwnObjectiveCreationId as Id,
    (obj.ObjectiveTitle + ' - ' + team.KRA) AS Name
FROM PMS_OwnObjectiveCreation team
INNER JOIN PMS_Objectives obj 
    ON team.ObjectiveId = obj.ObjectiveId
WHERE team.Deactivate = 0
  AND team.Origin = 'Team'
  AND (',' + team.EmployeeId + ',' LIKE '%," + EmployeeId + ",%')";
            }
            else
            {
                query = @"
            SELECT ObjectiveId as Id, ObjectiveTitle as Name
            FROM PMS_Objectives
            WHERE Deactivate = 0
              AND Origin = 'Individual' and EmployeeId='"+ EmployeeId + "'";
            }

            param.Add("@query", query);

            var list = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public ActionResult CategoryWiseKPI(string Category)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            DynamicParameters param = new DynamicParameters();
            param.Add("@query", "Select KPIId as Id,KPIName as Name FROM PMS_KPIMaster WHERE Deactivate=0 and Category='" + Category + "'");
            var KPINameList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
            return Json(KPINameList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult KPIWiseDescription(int KPIID)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            DynamicParameters param = new DynamicParameters();
            param.Add("@query", "Select Description FROM PMS_KPIMaster WHERE Deactivate=0 and KPIId='" + KPIID + "'");
            var KPINameList = DapperORM.ReturnList<PMS_KPIMaster>("sp_QueryExcution", param).FirstOrDefault();
            return Json(KPINameList.Description, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveUpdate(PMS_OwnObjectiveCreation own)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@p_process", string.IsNullOrEmpty(own.OwnObjectiveCreationId_Encrypted) ? "Save" : "Update");

                param.Add("@p_OwnObjectiveCreationId", own.OwnObjectiveCreationId);
                param.Add("@p_OwnObjectiveCreationId_Encrypted", own.OwnObjectiveCreationId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_DepartmentId", Session["EmpDepartment"]);
                param.Add("@p_EmployeeId", HttpContext.Session["EmployeeId"]);

                long? alignWithId = null;
                string alignWithType = null;

                if (own.IsAlign == true && own.AlignWith == "Team")
                {
                    // own.ObjectiveId NOW contains Team OwnObjectiveCreationId
                    long teamOwnObjectiveId = own.ObjectiveId;

                    // 🔥 Fetch correct ObjectiveId from that row
                    var paramFetch = new DynamicParameters();
                    paramFetch.Add("@query", @"
        SELECT ObjectiveId 
        FROM PMS_OwnObjectiveCreation
        WHERE OwnObjectiveCreationId = " + teamOwnObjectiveId);

                    var objectiveId = DapperORM
                        .ReturnList<long>("sp_QueryExcution", paramFetch)
                        .FirstOrDefault();

                    if (objectiveId == 0)
                    {
                        TempData["Message"] = "Invalid team KRA selected.";
                        TempData["Icon"] = "error";
                        return RedirectToAction("ESS_PMS_OwnGoalCreation", new { area = "ESS" });
                    }

                    alignWithType = "Team";
                    alignWithId = teamOwnObjectiveId;

                    // 🔥 overwrite ObjectiveId safely
                    own.ObjectiveId = Convert.ToInt32(objectiveId);
                }

                else if (own.IsAlign == true && own.AlignWith == "Company")
                {
                    alignWithType = "Company";
                    alignWithId = own.ObjectiveId; // if needed
                }

                param.Add("@p_IsAlign", own.IsAlign);
                param.Add("@p_AlignWith", alignWithId);
                param.Add("@p_ObjectiveId", own.ObjectiveId);
                param.Add("@p_KRA", own.KRA);
                param.Add("@p_SmartPrinciple", own.SmartPrinciple);
                param.Add("@p_UOM", own.UOM);
                param.Add("@p_Target", own.Target);
                param.Add("@p_Origin", "Own");
                param.Add("@p_StartDate", own.StartDate);
                param.Add("@p_EndDate", own.EndDate);
                param.Add("@p_KPICategory", own.KPICategory);
                param.Add("@p_KPINameId", own.KPINameId);
                param.Add("@p_TargetType", own.TargetType);
                param.Add("@p_Weightage", own.Weightage);
                param.Add("@p_BranchID", Session["BranchId"]);
                param.Add("@p_CmpId", Session["CompanyId"]);
                param.Add("@p_Visibility", own.Visibility);
                param.Add("@p_Description", own.Description);
                param.Add("@p_KPICategory", own.KPICategory);
                param.Add("@p_KPINameId", own.KPINameId);
                param.Add("@p_AlignWithType", alignWithType);
                param.Add("@p_Attachment", own.Attachment);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 10);
                var Result = DapperORM.ExecuteReturn("sp_SUD_PMS_OwnObjectiveCreation", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                return RedirectToAction("ESS_PMS_OwnGoalCreation", "ESS_PMS_OwnGoalCreation", new { area = "ESS" });
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                TempData["Icon"] = "error";
                return RedirectToAction("ESS_PMS_OwnGoalCreation", new { area = "ESS" });
            }
        }

        public ActionResult IsOwnGoalExist(int? ObjectiveId, int? EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int EmpId = Convert.ToInt32(Session["EmployeeId"]);

                    var param = new DynamicParameters();
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_ObjectiveId", ObjectiveId);
                    param.Add("@p_EmployeeId", EmpId);
                    param.Add("@p_MachineName", System.Net.Dns.GetHostName());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_PMS_OwnObjectiveCreation", param);
                    var Message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    if (!string.IsNullOrEmpty(Message))
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
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult IsObjectiveExist(string ObjectiveTitle, string Origin)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var param = new DynamicParameters();
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_ObjectiveTitle", ObjectiveTitle);
                    param.Add("@p_Origin", Origin);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_PMS_Objectives", param);
                    var Message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Message = "An error occurred: " + ex.Message, Icon = "error" }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult SaveObjective(PMS_Objectives PMS_Objectives)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    TempData["Message"] = "Your session has expired. Please log in again.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    string process = string.IsNullOrEmpty(PMS_Objectives.ObjectiveId_Encrypted) ? "Save" : "Update";
                    var param = new DynamicParameters();
                    param.Add("@p_process", process);
                    param.Add("@p_ObjectiveId_Encrypted", PMS_Objectives.ObjectiveId_Encrypted);
                    param.Add("@p_ObjectiveTitle", PMS_Objectives.ObjectiveTitle);
                    param.Add("@p_Origin", PMS_Objectives.Origin);
                    param.Add("@p_EmployeeId", Session["EmployeeId"]);
                    param.Add("@p_CmpId", Session["CmpId"]?.ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]?.ToString());
                    param.Add("@p_MachineName", System.Net.Dns.GetHostName());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                    sqlcon.Execute("sp_SUD_PMS_Objectives", param, commandType: CommandType.StoredProcedure);

                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    TempData["P_Id"] = param.Get<string>("@p_Id");

                    return RedirectToAction("ESS_PMS_OwnGoalCreation", new { area = "ESS" });
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "An error occurred while saving the objective: " + ex.Message;
                TempData["Icon"] = "error";
                return RedirectToAction("ESS_PMS_OwnGoalCreation", new { area = "ESS" });
            }
        }

        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_OwnObjectiveCreationId_Encrypted", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var data = DapperORM.DynamicList("sp_List_PMS_OwnObjectiveCreation", param);
                ViewBag.GetList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        public ActionResult Delete(string OwnObjectiveCreationId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_OwnObjectiveCreationId_Encrypted", OwnObjectiveCreationId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_PMS_OwnObjectiveCreation", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_PMS_OwnGoalCreation");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult ObjectiveWiseDetails(string AlignWith, int Objective)
        {
            try
            {
                var CmpId = Session["CompanyId"];
                var BranchId = Session["BranchId"];

                string query;

                if (AlignWith == "Team")
                {
                    // 🔥 UNIQUE → OwnObjectiveCreationId
                    query = @"
                SELECT *
                FROM PMS_OwnObjectiveCreation
                WHERE Deactivate = 0
                  AND Origin = 'Team'
                  AND OwnObjectiveCreationId = " + Objective + @"
                  AND CmpId = " + CmpId + @"
                  AND BranchId = " + BranchId;
                }
                else
                {
                    // 🔥 Company objectives → ObjectiveId
                //    query = @"
                //SELECT TOP 1 *
                //FROM PMS_OwnObjectiveCreation
                //WHERE Deactivate = 0
                //  AND Origin = 'Company'
                //  AND ObjectiveId = " + Objective + @"
                //  AND CmpId = " + CmpId + @"
                //  AND BranchId = " + BranchId;

                    query = @"Select * from  PMS_CompanyObjectiveCreation where CompanyObjectiveCreationId = '"+ Objective + "'and CmpId= '"+ CmpId + "'";
                }

                var result = DapperORM.DynamicQuerySingle(query);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }




    }
}