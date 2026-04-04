using Dapper;
using System;
using System.Net;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_NightTracker;
using KompassHR.Models;

namespace KompassHR.Areas.Setting.Controllers.Setting_NightTracker
{
    public class Setting_NightTracker_NightRoundLocationController : Controller
    {
        // GET: Setting/Setting_NightTracker_NightRoundLocation
        public ActionResult Setting_NightTracker_NightRoundLocation(string LocationID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
              
                ViewBag.AddUpdateTitle = "Add";
                NightRound_Location nightroundlocation = new NightRound_Location();
                if (LocationID_Encrypted != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_LocationId_Encrypted", LocationID_Encrypted);
                    nightroundlocation = DapperORM.ReturnList<NightRound_Location>("sp_List_NightRound_Location", param).FirstOrDefault();
                }
                return View(nightroundlocation);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult IsLocatioExists(string LocationName, string LocationId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_LocationName", LocationName);
                    param.Add("@p_LocationID_Encrypted", LocationId_Encrypted);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_NightRound_Location", param);
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

        //Save NightRound_Location
        [HttpPost]
        public ActionResult SaveUpdateLocation(NightRound_Location nightroundlocation)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", string.IsNullOrEmpty(nightroundlocation.LocationId_Encrypted) ? "Save" : "Update");
                param.Add("@p_LocationId", nightroundlocation.LocationID);
                param.Add("@p_LocationID_Encrypted", nightroundlocation.LocationId_Encrypted);
                param.Add("@p_LocationName", nightroundlocation.LocationName);
                param.Add("@p_IsActive", nightroundlocation.IsActive);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_NightRound_Location", param);
                var msg = param.Get<string>("@p_msg");
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_NightTracker_NightRoundLocation", "Setting_NightTracker_NightRoundLocation");

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
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
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Process", "List");
                var data = DapperORM.ReturnList<NightRound_Location>("sp_List_NightRound_Location", param).ToList();
                ViewBag.GetLocationNameList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        //Delete Location
        public ActionResult Delete(string LocationId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Delete");
                param.Add("@p_LocationID_Encrypted", LocationId_Encrypted);             
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_NightRound_Location", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_NightTracker_NightRoundLocation");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
    }
}