using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using System.Data;
using System.Net;
using System.Data.SqlClient;
using KompassHR.Areas.Setting.Models.Setting_OrganisationChart;


namespace KompassHR.Areas.Setting.Controllers.Setting_OrganisationChart
{
    public class Setting_OrganisationChart_LevelController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_OrganisationChart_Level
        #region main view
        public ActionResult Setting_OrganisationChart_Level(string OrganisationChartId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 530;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";

                param.Add("@query", "select OrganisationTypeId as Id, OrganisationTypeName as Name from Organisation_Type where Deactivate=0");
                var TypeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.TypeName = TypeName;

                ViewBag.level = "";

                ViewBag.SubLevel = "";

                OrganisationLevel OrganisationLevel = new OrganisationLevel();

                if (OrganisationChartId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";

                    param = new DynamicParameters();
                    param.Add("@p_OrganisationChartId_Encrypted", OrganisationChartId_Encrypted);
                    OrganisationLevel = DapperORM.ReturnList<OrganisationLevel>("sp_List_Organisation_Chart", param).FirstOrDefault();

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@query", "select OrganisationLevelId Id, OrganisationLevel as Name from Organisation_Level where OrganisationLevelId= " + OrganisationLevel.OrganisationLevelId + " and Deactivate=0 ");
                    var level = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();
                    ViewBag.level = level;

                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@query", " select OrganisationChartId as id , OrganisationRemark as Name from Organisation_Chart where OrganisationTypeId=" + OrganisationLevel.OrganisationTypeId + " and Deactivate=0 ");
                    var SubLevel = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();
                    ViewBag.SubLevel = SubLevel;

                    return View(OrganisationLevel);
                }

                return View();

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");

            }
        }
        #endregion

        #region GetLevel
        public ActionResult GetLevel(int OrganisationTypeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                //param.Add("@query", "select max(Organisation_Chart.OrganisationLevelId) as Id,Organisation_Level.OrganisationLevel as Name from Organisation_Chart left join Organisation_Level on Organisation_Level.OrganisationLevelId=Organisation_Chart.OrganisationLevelId where OrganisationTypeId="+OrganisationTypeId+" and Organisation_Chart.Deactivate=0  group by Organisation_Level.OrganisationLevel ");
                //var level = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                //ViewBag.level = level;
                //select MAX(OrganisationLevelId)from Organisation_Chart where OrganisationTypeId = 2

                //param.Add("@query", "select MAX(OrganisationLevelId) from Organisation_Chart where OrganisationTypeId = "+ OrganisationTypeId + " and  Deactivate=0 ");
                //var maxcount = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).FirstOrDefault();
                //ViewBag.maxcount = maxcount;

                var id = sqlcon.ExecuteScalar("select MAX(OrganisationLevelId) from Organisation_Chart where OrganisationTypeId = " + OrganisationTypeId + " and  Deactivate=0");

                var count = id;
                param.Add("@query", "select top (" + count + " +1) (Organisation_Level.OrganisationLevelId) Id, Organisation_Level.OrganisationLevel as Name from Organisation_Level where  Deactivate=0 ");
                var level = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.level = level;

                param.Add("@query", " select OrganisationChartId as id , OrganisationRemark as Name from Organisation_Chart where OrganisationTypeId=" + OrganisationTypeId + " and Deactivate=0 ");
                var SubLevel = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.SubLevel = SubLevel;

                if (level.Count == 0)
                {
                    param.Add("@query", "select top (1) OrganisationLevelId as Id,OrganisationLevel as Name from Organisation_Level where Deactivate=0");
                    var level1 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    return Json(new { success = true, level = level1 }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = true, level = level, SubLevel = SubLevel }, JsonRequestBehavior.AllowGet);

                }

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetSubLevel
        public ActionResult GetSubLevel(int OrganisationLevelId, int OrganisationTypeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                param.Add("@query", "select OrganisationChartId as Id,OrganisationRemark as Name from Organisation_Chart where OrganisationLevelId=" + OrganisationLevelId + "-1 and OrganisationTypeId=" + OrganisationTypeId + " and Deactivate=0 ");
                var Sublevel = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.Sublevel = Sublevel;

                return Json(new { success = true, Sublevel = Sublevel }, JsonRequestBehavior.AllowGet);

                //if (level.Count == 0)
                //{
                //    param.Add("@query", "select top (1) OrganisationLevelId as Id,OrganisationLevel as Name from Organisation_Level where Deactivate=0");
                //    var level1 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                //    return Json(new { success = true, level = level1 }, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    return Json(new { success = true, level = level, SubLevel = SubLevel }, JsonRequestBehavior.AllowGet);

                //}

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region IsValidation
        [HttpGet]
        public ActionResult IsExists(int OrganisationTypeId, int OrganisationLevelId, string OrganisationRemark)
        {
            try
            {

                param.Add("@p_process", "IsValidation");
                param.Add("@p_OrganisationTypeId", OrganisationTypeId);
                param.Add("@p_OrganisationLevelId", OrganisationLevelId);
                param.Add("@p_OrganisationRemark", OrganisationRemark);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_OrganisationChart", param);
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
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(OrganisationLevel OrganisationLevel)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                param.Add("@p_process", string.IsNullOrEmpty(OrganisationLevel.OrganisationChartId_Encrypted) ? "Save" : "Update");
                param.Add("@p_OrganisationChartId", OrganisationLevel.OrganisationChartId);
                param.Add("@p_OrganisationChartId_Encrypted", OrganisationLevel.OrganisationChartId_Encrypted);
                param.Add("@p_OrganisationTypeId", OrganisationLevel.OrganisationTypeId);
                param.Add("@p_OrganisationLevelId", OrganisationLevel.OrganisationLevelId);
                param.Add("@p_OrganisationRemark", OrganisationLevel.OrganisationRemark);
                param.Add("@p_OrganisationSubLevelId", OrganisationLevel.OrganisationSubLevelId);
                param.Add("@p_OrganisationTitle", OrganisationLevel.OrganisationTitle);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_OrganisationChart]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Setting_OrganisationChart_Level", "Setting_OrganisationChart_Level");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList Main View
        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 530;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_OrganisationChartId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Organisation_Chart", param).ToList();
                ViewBag.GetOrganisationList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        [HttpGet]
        public ActionResult Delete(string OrganisationChartId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_OrganisationChartId_Encrypted", OrganisationChartId_Encrypted);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parame
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_OrganisationChart", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                return RedirectToAction("GetList", "Setting_OrganisationChart_Level");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


    }
}

