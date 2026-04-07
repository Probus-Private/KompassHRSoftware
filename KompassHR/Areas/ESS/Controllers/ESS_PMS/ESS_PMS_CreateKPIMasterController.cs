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
    public class ESS_PMS_CreateKPIMasterController : Controller
    {
        DynamicParameters param = new DynamicParameters();

        // GET: ESS/ESS_PMS_CreateKPIMaster
        [HttpGet]
        public ActionResult ESS_PMS_CreateKPIMaster(string KPIId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 692;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                PMS_KPIMaster PMS_KPIMaster = new PMS_KPIMaster();
               if (!string.IsNullOrEmpty(KPIId_Encrypted))
                 {
                        ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_KPIId_Encrypted", KPIId_Encrypted);
                    PMS_KPIMaster = DapperORM.ReturnList<PMS_KPIMaster>("sp_List_PMS_KPIMaster", param).FirstOrDefault();
                    TempData["p_Id"] = PMS_KPIMaster.KPIId;
                }
                return View(PMS_KPIMaster);
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_PMS_CreateKPIMaster");
            }
        }

        public ActionResult IsKPIExist(string Category,string KPIName, string KPIId_Encrypted)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_Category", Category);
                    param.Add("@p_KPIName", KPIName);
                    param.Add("@p_KPIId_Encrypted", KPIId_Encrypted);
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeId"]?.ToString());
                    param.Add("@p_MachineName", System.Net.Dns.GetHostName());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    sqlcon.Execute("sp_SUD_PMS_KPIMaster", param, commandType: CommandType.StoredProcedure);
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
        public ActionResult SaveUpdate(PMS_KPIMaster PMS_KPIMaster)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();

                param.Add("@p_process", string.IsNullOrEmpty(PMS_KPIMaster.KPIId_Encrypted) ? "Save" : "Update");
                param.Add("@p_KPIId", PMS_KPIMaster.KPIId);
                param.Add("@p_KPIId_Encrypted", PMS_KPIMaster.KPIId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_UseBy", PMS_KPIMaster.UseBy);
                param.Add("@p_Category", PMS_KPIMaster.Category);
                param.Add("@p_KPIName", PMS_KPIMaster.KPIName);
                param.Add("@p_Description", PMS_KPIMaster.Description);
                param.Add("@p_Status",true);
                param.Add("@p_LastStatusDate", PMS_KPIMaster.LastStatusDate);
                param.Add("@p_LastStatusBy", PMS_KPIMaster.LastStutusBy);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = DapperORM.ExecuteReturn("sp_SUD_PMS_KPIMaster", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_PMS_CreateKPIMaster", "ESS_PMS_CreateKPIMaster"); // Adjust this to your actual View & Controller name
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 692;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_KPIId_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_PMS_KPIMaster", param);
                ViewBag.GetList = data;
                return View();
            }
            catch(Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult Delete(string KPIId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_KPIId_Encrypted", KPIId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_PMS_KPIMaster", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_PMS_CreateKPIMaster");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

    }
}