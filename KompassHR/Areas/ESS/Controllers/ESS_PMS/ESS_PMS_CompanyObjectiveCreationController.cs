using Dapper;
using KompassHR.Areas.ESS.Models.ESS_PMS;
using KompassHR.Areas.Setting.Models.Setting_Prime;
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
    public class ESS_PMS_CompanyObjectiveCreationController : Controller
    {
        DynamicParameters param = new DynamicParameters();

        // GET: ESS/ESS_PMS_CompanyObjectiveCreation
        public ActionResult ESS_PMS_CompanyObjectiveCreation(string CompanyObjectiveCreationId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 600;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "Select DepartmentId,DepartmentName from Mas_Department Where Deactivate=0");
                var DepartmentList = DapperORM.ReturnList<Mas_Department>("sp_QueryExcution", param1).ToList();
                ViewBag.DepartmentList = DepartmentList;

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@query", "select Fid as Id,PMS_Year  as Name from PMS_Year where IsActive=1 ");
                var YearList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();
                ViewBag.YearList = YearList;

                DynamicParameters param3 = new DynamicParameters();
                param3.Add("@query", "select EmployeeId,EmployeeName from Mas_Employee where ContractorID=1 and Deactivate=0 and EmployeeLeft=0");
                var EmployeeList = DapperORM.ReturnList<Mas_Employee>("sp_QueryExcution", param3).ToList();
                ViewBag.EmployeeList = EmployeeList;

                DynamicParameters param6 = new DynamicParameters();
                param6.Add("@query", "select TargetTypeId AS Id,TargetType AS Name from PMS_TargetType where Deactivate=0");
                var TragetTypeList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param6).ToList();
                ViewBag.TragetTypeList = TragetTypeList;
                DynamicParameters param7 = new DynamicParameters();
                param7.Add("@query", "select UOM as Name from PMS_UOM where Deactivate=0");
                var UOMList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param7).ToList();
                ViewBag.UOMList = UOMList;
                DynamicParameters param4 = new DynamicParameters();
                param4.Add("@query", @"SELECT 
    y.PMS_Year,
    COUNT(c.CompanyObjectiveCreationId) AS ObjectiveCount
FROM PMS_Year y
LEFT JOIN PMS_CompanyObjectiveCreation c
    ON c.YearId = y.Fid
WHERE y.IsActive = 1
GROUP BY y.PMS_Year
ORDER BY y.PMS_Year;");

                var YearObjectiveList = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param4).ToList();
                ViewBag.YearObjectiveList = YearObjectiveList;


                ViewBag.AddUpdateTitle = "Add";
                PMS_CompanyObjectiveCreation PMS_CompanyObjectiveCreation = new PMS_CompanyObjectiveCreation();
                if (CompanyObjectiveCreationId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_CompanyObjectiveCreationId_Encrypted", CompanyObjectiveCreationId_Encrypted);
                    PMS_CompanyObjectiveCreation = DapperORM.ReturnList<PMS_CompanyObjectiveCreation>("sp_List_PMS_CompanyObjectiveCreation", param).FirstOrDefault();
                    TempData["p_Id"] = PMS_CompanyObjectiveCreation.CompanyObjectiveCreationId;
                }
                return View(PMS_CompanyObjectiveCreation);
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_PMS_GoalCreation");
            }
        }

        [HttpGet]
        public JsonResult IsObjectiveTitleExists(string ObjectiveTitle, string GoalCreationIDEncrypted)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var param = new DynamicParameters();
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_ObjectiveTitle", ObjectiveTitle);
                    param.Add("@p_CompanyObjectiveCreationId_Encrypted", GoalCreationIDEncrypted);
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeId"]?.ToString());
                    param.Add("@p_MachineName", System.Net.Dns.GetHostName());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    sqlcon.Execute("sp_SUD_PMS_CompanyObjectiveCreation", param, commandType: CommandType.StoredProcedure);
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
            }
            catch (Exception ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveUpdate(PMS_CompanyObjectiveCreation PMS_CompanyObjectiveCreation, string SmartGoal, string GoalOwnerId)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }

            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    string process = string.IsNullOrEmpty(PMS_CompanyObjectiveCreation.CompanyObjectiveCreationId_Encrypted) ? "Save" : "Update";

                    var param = new DynamicParameters();
                    param.Add("@p_process", string.IsNullOrEmpty(PMS_CompanyObjectiveCreation.CompanyObjectiveCreationId_Encrypted) ? "Save" : "Update");
                    param.Add("@p_CompanyObjectiveCreationId_Encrypted", PMS_CompanyObjectiveCreation.CompanyObjectiveCreationId_Encrypted);
                    param.Add("@p_YearId", PMS_CompanyObjectiveCreation.YearId);
                    param.Add("@p_ObjectiveTitle", PMS_CompanyObjectiveCreation.ObjectiveTitle);
                    param.Add("@p_GoalTitle", PMS_CompanyObjectiveCreation.GoalTitle);
                    param.Add("@p_SmartGoal", SmartGoal);
                    param.Add("@p_DepartmentId", PMS_CompanyObjectiveCreation.DepartmentId);
                    param.Add("@p_GoalType", PMS_CompanyObjectiveCreation.GoalType);
                    param.Add("@p_GoalCategory", PMS_CompanyObjectiveCreation.GoalCategory);
                    param.Add("@p_TargetType", PMS_CompanyObjectiveCreation.TargetType);
                    param.Add("@p_Target", PMS_CompanyObjectiveCreation.Target);
                    param.Add("@p_UOM", PMS_CompanyObjectiveCreation.UOM);
                    param.Add("@p_GoalOwner", PMS_CompanyObjectiveCreation.GoalOwner);
                    param.Add("@p_StartDate", PMS_CompanyObjectiveCreation.StartDate);
                    param.Add("@p_EndDate", PMS_CompanyObjectiveCreation.EndDate);
                    param.Add("@p_CmpId", Session["CompanyId"]); 
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]?.ToString());
                    param.Add("@p_MachineName", System.Net.Dns.GetHostName());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_PMS_CompanyObjectiveCreation", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    TempData["P_Id"] = param.Get<string>("@p_Id");

                    return RedirectToAction("GetList", "ESS_PMS_CompanyObjectiveCreation");
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                TempData["Icon"] = "error";
                return RedirectToAction("ESS_PMS_CompanyObjectiveCreation", new { area = "ESS" });
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 600;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_CompanyObjectiveCreationId_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_PMS_CompanyObjectiveCreation", param);
                ViewBag.GetList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }


        public ActionResult Delete(string CompanyObjectiveCreationId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_CompanyObjectiveCreationId_Encrypted", CompanyObjectiveCreationId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_PMS_CompanyObjectiveCreation", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_PMS_CompanyObjectiveCreation");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
    }
}