using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Other;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Other
{
    public class ESS_Other_AdditionalNotifyController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_Other_AdditionalNotify
        #region ESS_Other_AdditionalNotify Main View 
        [HttpGet]
        public ActionResult ESS_Other_AdditionalNotify(string AdditionNotifyId_Encrypted)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 225;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";
                Session["EmployeeId"] = 1;
                Mas_AdditionNotify mas_FavouriteBirthday = new Mas_AdditionNotify();
                if (AdditionNotifyId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_AdditionNotifyId_Encrypted", AdditionNotifyId_Encrypted);
                    mas_FavouriteBirthday = DapperORM.ReturnList<Mas_AdditionNotify>("sp_List_Mas_AdditionNotify", param).FirstOrDefault();
                }


                var AdditionalNotification = "select max(CreatedDate) As CreatedDate from Mas_AdditionNotify";
                var LastRecored = DapperORM.DynamicQuerySingle(AdditionalNotification);
                ViewBag.LastRecored = LastRecored.CreatedDate;

                return View(mas_FavouriteBirthday);
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_Other_AdditionalNotify");
            }

        }
        #endregion

        #region IsValidation
        [HttpGet]
        public JsonResult IsAdditionalNotifyExists(double AdditionNotifyEmployeeID, double AdditionNotifyIdTeamEmployeeID, string AdditionNotifyId_Encrypted)
        {
            try
            {


                param.Add("@p_process", "IsValidation");
                param.Add("@p_AdditionNotifyEmployeeID", AdditionNotifyEmployeeID);
                param.Add("@p_AdditionNotifyIdTeamEmployeeID", AdditionNotifyIdTeamEmployeeID);
                param.Add("@p_AdditionNotifyId_Encrypted", AdditionNotifyId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_AdditionNotify", param);
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
            catch (Exception Ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Mas_AdditionNotify AdditionalNotify)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(AdditionalNotify.AdditionNotifyId_Encrypted) ? "Save" : "Update");
                param.Add("@p_AdditionNotifyId", AdditionalNotify.AdditionNotifyId);
                param.Add("@p_AdditionNotifyId_Encrypted", AdditionalNotify.AdditionNotifyId_Encrypted);
                param.Add("@p_AdditionNotifyEmployeeID", Session["EmployeeId"]);
                param.Add("@p_AdditionNotifyIdTeamEmployeeID", 1);
                param.Add("@p_AdditionNotifyEmailID", AdditionalNotify.AdditionNotifyEmailID);

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Mas_AdditionNotify", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("ESS_Other_AdditionalNotify", "ESS_Other_AdditionalNotify");
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_Other_AdditionalNotify");
            }

        }
        #endregion

        #region GetList
        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 225;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";
                param.Add("@p_AdditionNotifyId_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Mas_AdditionNotify", param);
                ViewBag.GetAdditionNotifyList = data;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_Other_AdditionalNotify");
            }

        }

        #endregion
        #region Delete
        [HttpGet]
        public ActionResult Delete(string AdditionNotifyId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_AdditionNotifyId_Encrypted", AdditionNotifyId_Encrypted);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_AdditionNotify", param);

                return RedirectToAction("GetList", "ESS_Other_AdditionalNotify");
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_Other_AdditionalNotify");
            }
        }

        #endregion
    }
}