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
    public class ESS_PMS_GoalCreationController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_PMS_GoalCreation
        public ActionResult ESS_PMS_GoalCreation()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 595;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@query", "Select GoalTypeID,GoalType from PMS_GoalType Where Deactivate=0");
                var GoalTypeList = DapperORM.ReturnList<PMS_GoalType>("sp_QueryExcution", param).ToList();
                ViewBag.GoalTypeList = GoalTypeList;

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "Select DepartmentId,DepartmentName from Mas_Department Where Deactivate=0");
                var DepartmentList = DapperORM.ReturnList<Mas_Department>("sp_QueryExcution", param1).ToList();
                ViewBag.DepartmentList = DepartmentList;

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@query", "Select GoalCreationID,GoalTitle from PMS_GoalCreation Where Deactivate=0");
                var AlignmentList = DapperORM.ReturnList<PMS_GoalCreation>("sp_QueryExcution", param2).ToList();
                ViewBag.AlignmentList = AlignmentList;

                return View();
            }
            catch(Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_PMS_GoalCreation");
            }
        }

        public ActionResult IsGoalTitleExists(string GoalTitle, string GoalCreationIDEncrypted)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_GoalTitle", GoalTitle);
                    param.Add("@p_GoalCreationIDEncrypted", GoalCreationIDEncrypted);
                    param.Add("@p_MachineName", Dns.GetHostName());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                    var result = DapperORM.ExecuteReturn("sp_SUD_PMS_GoalCreation", param);
                    var message = param.Get<string>("@p_msg");
                    var icon = param.Get<string>("@p_Icon");

                    if (!string.IsNullOrEmpty(message))
                    {
                        return Json(new { Message = message, Icon = icon }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


        [HttpPost]
        public ActionResult SaveUpdate(PMS_GoalCreation model)
        {
            try
            {
                var EmployeeId = Convert.ToInt64(Session["EmployeeId"]);
                var CreatedBy = Session["EmployeeName"].ToString();
                var MachineName = Dns.GetHostName();

                DynamicParameters param = new DynamicParameters();

                param.Add("@p_process", string.IsNullOrEmpty(model.GoalCreationIDEncrypted) ? "Save" : "Update");
                param.Add("@p_GoalCreationID", model.GoalCreationID);
                param.Add("@p_GoalCreationIDEncrypted", model.GoalCreationIDEncrypted);
                param.Add("@p_CmpID", model.CmpID); // Optional, will be overwritten in SP from EmployeeId
                param.Add("@p_EmployeeId", EmployeeId);

                param.Add("@p_GoalTitle", model.GoalTitle);
                param.Add("@p_Description", model.Description);
                param.Add("@p_GoalTypeId", model.GoalTypeId);
                param.Add("@p_DepartmentId", model.DepartmentId);
                param.Add("@p_Alignment", model.Alignment);
                param.Add("@p_SuccessMatrix", model.SuccessMatrix);
                param.Add("@p_Weightage", model.Weightage);
                param.Add("@p_Visibility", model.Visibility);
                param.Add("@p_StartDate", model.StartDate);
                param.Add("@p_EndDate", model.EndDate);

                param.Add("@p_CreatedUpdateBy", CreatedBy);
                param.Add("@p_MachineName", MachineName);

                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 10);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);

                DapperORM.ExecuteReturn("sp_SUD_PMS_GoalCreation", param);

                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["p_Id"] = param.Get<string>("@p_Id");

                return RedirectToAction("ESS_PMS_GoalCreation", "ESS_PMS_GoalCreation", new { Area = "ESS" });
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error: " + ex.Message;
                TempData["Icon"] = "error";
                return RedirectToAction("ESS_PMS_GoalCreation", "ESS_PMS_GoalCreation", new { Area = "ESS" });
            }
        }

        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                var EmployeeId = Session["EmployeeId"];
                //param.Add("@p_MarketPlaceEmployeeID", EmployeeId);
                param.Add("@p_GoalCreationID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_PMS_GoalCreation", param);
                ViewBag.GetGoalList = data;
                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_PMS_GoalCreation");
            }

        }


    }
}