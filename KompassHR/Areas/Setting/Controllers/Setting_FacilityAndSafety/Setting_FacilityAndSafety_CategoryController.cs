using Dapper;
using KompassHR.Areas.Setting.Models.Setting_FacilityAndSafety;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_FacilityAndSafety
{
    public class Setting_FacilityAndSafety_CategoryController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: FacilityAndSafety/FacilityAndSafetyCategory

        #region Facility Main View
        [HttpGet]
        public ActionResult Setting_FacilityAndSafety_Category(string FacilitySafetyCategoryID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                FacilitySafety_Category mas_Category = new FacilitySafety_Category();
                if (FacilitySafetyCategoryID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_FacilitySafetyCategoryID_Encrypted", FacilitySafetyCategoryID_Encrypted);
                    mas_Category = DapperORM.ReturnList<FacilitySafety_Category>("sp_List_FacilitySafety_Category", param).FirstOrDefault();
                }
                return View(mas_Category);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsVerification
        [HttpGet]
        public ActionResult IsFacilityAndsafetyCategoryExists(string FacilitySafetyCategoryName, string FacilitySafetyCategoryID_Encrypted)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {

                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_FacilitySafetyCategoryID_Encrypted", FacilitySafetyCategoryID_Encrypted);
                    param.Add("@p_FacilitySafetyCategoryName", FacilitySafetyCategoryName);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("[sp_SUD_FacilitySafety_Category]", param);
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

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion


        #region Facility SaveUpdate

        [HttpPost]
        public ActionResult SaveUpdate(FacilitySafety_Category Category)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(Category.FacilitySafetyCategoryID_Encrypted) ? "Save" : "Update");
                param.Add("@p_FacilitySafetyCategoryID", Category.FacilitySafetyCategoryID);
                param.Add("@p_FacilitySafetyCategoryID_Encrypted", Category.FacilitySafetyCategoryID_Encrypted);
                param.Add("@p_FacilitySafetyCategoryName", Category.FacilitySafetyCategoryName);
                param.Add("@p_IsActive ", Category.IsActive);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_FacilitySafety_Category", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_FacilityAndSafety_Category", "Setting_FacilityAndSafety_Category");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Facility List View

        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_FacilitySafetyCategoryID_Encrypted", "List");
                var data = DapperORM.ReturnList<FacilitySafety_Category>("sp_List_FacilitySafety_Category", param).ToList();
                ViewBag.GetFacilityAndSafetyList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region Facility Delete
        [HttpGet]
        public ActionResult Delete(string FacilitySafetyCategoryID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_FacilitySafetyCategoryID_Encrypted", FacilitySafetyCategoryID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_FacilitySafety_Category", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_FacilityAndSafety_Category");
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