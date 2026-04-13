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
    public class ESS_Other_FavouriteBirthdayController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_Other_FavouriteBirthday
        #region ESS_Other_FavouriteBirthday Main View 
        [HttpGet]
        public ActionResult ESS_Other_FavouriteBirthday(string FavouriteBirthdayID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 224;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                Session["EmployeeId"] = 1;
                ViewBag.AddUpdateTitle = "Add";
                Mas_FavouriteBirthday mas_FavouriteBirthday = new Mas_FavouriteBirthday();
                if (FavouriteBirthdayID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_FavouriteBirthdayID_Encrypted", FavouriteBirthdayID_Encrypted);
                    mas_FavouriteBirthday = DapperORM.ReturnList<Mas_FavouriteBirthday>("sp_List_Mas_FavouriteBirthday", param).FirstOrDefault();
                }

                var FavouriteBirthday = "select max(CreatedDate) As CreatedDate from Mas_AdditionNotify";
                var LastRecored = DapperORM.DynamicQuerySingle(FavouriteBirthday);
                ViewBag.LastRecored = LastRecored.CreatedDate;
                TempData["FavouriteBirthdayDate"] = mas_FavouriteBirthday.FavouriteBirthdayDate;


                return View(mas_FavouriteBirthday);
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_Other_FavouriteBirthday");
            }
        }
        #endregion

        #region IsValidation
        [HttpGet]
        public JsonResult IsFavouriteBirthdayExists(string FavouriteBirthdayName, string FavouriteBirthdayID_Encrypted)
        {
            try
            {


                param.Add("@p_process", "IsValidation");
                param.Add("@p_FavouriteBirthdayName", FavouriteBirthdayName);
                param.Add("@p_FavouriteBirthdayID_Encrypted", FavouriteBirthdayID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_FavouriteBirthday", param);
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
                //return RedirectToAction(Ex.Message.ToString(), "Wage");
            }
        }
        #endregion

        #region SaveUpdate

        [HttpPost]
        public ActionResult SaveUpdate(Mas_FavouriteBirthday FavouriteBirthday)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(FavouriteBirthday.FavouriteBirthdayID_Encrypted) ? "Save" : "Update");
                param.Add("@p_FavouriteBirthdayID", FavouriteBirthday.FavouriteBirthdayID);
                param.Add("@p_FavouriteBirthdayID_Encrypted", FavouriteBirthday.FavouriteBirthdayID_Encrypted);
                param.Add("@p_FavouriteBirthdayEmployeeID", Session["EmployeeId"]);
                param.Add("@p_FavouriteBirthdayName", FavouriteBirthday.FavouriteBirthdayName);
                param.Add("@p_FavouriteBirthdayDate", FavouriteBirthday.FavouriteBirthdayDate);

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Mas_FavouriteBirthday", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_Other_FavouriteBirthday", "ESS_Other_FavouriteBirthday");
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_Other_FavouriteBirthday");
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

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 224;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_FavouriteBirthdayID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Mas_FavouriteBirthday", param);
                ViewBag.GetFavouriteBirthdayList = data;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_Other_FavouriteBirthday");
            }

        }
        #endregion

        #region Delete
        [HttpGet]
        public ActionResult Delete(string FavouriteBirthdayID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_FavouriteBirthdayID_Encrypted", FavouriteBirthdayID_Encrypted);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_FavouriteBirthday", param);

                return RedirectToAction("GetList", "ESS_Other_FavouriteBirthday");
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_Other_FavouriteBirthday");
            }
        }


        #endregion

    }
}