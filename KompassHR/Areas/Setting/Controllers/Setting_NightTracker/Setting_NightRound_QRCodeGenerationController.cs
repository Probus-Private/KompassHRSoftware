using Dapper;
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_NightTracker;
using KompassHR.Models;

namespace KompassHR.Areas.Setting.Controllers.Setting_NightTracker
{
    public class Setting_NightRound_QRCodeGenerationController : Controller
    {
        // GET: Setting/Setting_NightRound_QRCodeGeneration
        public ActionResult Setting_NightRound_QRCodeGeneration(string QRCodeID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters Param = new DynamicParameters();
                //Param.Add("@p_Process", "List");
                //var data = DapperORM.ReturnList<NightRound_Location>("sp_List_NightRound_Location", Param).ToList();
                //ViewBag.GetLocationName = new SelectList(data, "LocationID", "LocationName");

                Param.Add("@query", "Select  LocationID as Id, LocationName As Name from NightRound_Location where Deactivate=0" );
                var LocationName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", Param).ToList();
                ViewBag.GetLocationName = LocationName;

                ViewBag.AddUpdateTitle = "Add";
                NightRound_QRCodeGeneration QRCodeGeneration = new NightRound_QRCodeGeneration();
                if(QRCodeID_Encrypted!=null)
                {
                    DynamicParameters param = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_QRCodeID_Encrypted", QRCodeID_Encrypted);
                    QRCodeGeneration = DapperORM.ReturnList<NightRound_QRCodeGeneration>("sp_List_NightRound_QRCodeGeneration", param).FirstOrDefault();
                }

                return View(QRCodeGeneration);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        public ActionResult IsQRCodeLocationExist(string QRCodeLocationName,string QRCodeID_Encrypted,int dpdLocationNameID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    DynamicParameters Param = new DynamicParameters();
                    Param.Add("@p_process", "IsValidation");
                    Param.Add("@p_QRCodeLocationName", QRCodeLocationName);
                    Param.Add("@p_QRCodeID_Encrypted", QRCodeID_Encrypted);
                    Param.Add("@p_LocationID", dpdLocationNameID);
                    Param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    Param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    Param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    Param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_NightRound_QRCodeGeneration", Param);
                    var Message = Param.Get<string>("@p_msg");
                    var Icon = Param.Get<string>("@p_Icon");
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

        //Save QRCode Location
        [HttpPost]
        public ActionResult SaveUpdateQRCodeLocation(NightRound_QRCodeGeneration QRCodeGeneration)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters Param = new DynamicParameters();
                Param.Add("@p_process", string.IsNullOrEmpty(QRCodeGeneration.QRCodeID_Encrypted) ? "Save" : "Update");
                Param.Add("@p_QRCodeID", QRCodeGeneration.QRCodeID);
                Param.Add("@p_QRCodeID_Encrypted", QRCodeGeneration.QRCodeID_Encrypted);
                Param.Add("@p_QRCodeLocationName", QRCodeGeneration.QRCodeLocationName);
                Param.Add("@p_LocationID", QRCodeGeneration.LocationID);
                Param.Add("@p_IsActive", QRCodeGeneration.IsActive);
                Param.Add("@p_MachineName", Dns.GetHostName().ToString());
                Param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                Param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                Param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_NightRound_QRCodeGeneration", Param);
                var msg = Param.Get<string>("@p_msg");
                TempData["Message"] = Param.Get<string>("@p_msg");
                TempData["Icon"] = Param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_NightRound_QRCodeGeneration", "Setting_NightRound_QRCodeGeneration");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        //public ActionResult SaveQRCodeLocation()
        //{
        //    return View();
        //}
        //Get List
        [HttpGet]
        public ActionResult GetQRCodeLocationList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters Param = new DynamicParameters();
                Param.Add("@p_process", "List");
                var Data = DapperORM.ReturnList<NightRound_QRCodeGeneration>("sp_List_NightRound_QRCodeGeneration", Param).ToList();
                ViewBag.GetQRCodeLocationList = Data;
                return View();

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        public ActionResult Delete(string QRCodeID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters Param = new DynamicParameters();
                Param.Add("@p_process", "Delete");
                Param.Add("@p_QRCodeID_Encrypted", QRCodeID_Encrypted);
                Param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                Param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                Param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_NightRound_QRCodeGeneration", Param);
                var msg = Param.Get<string>("@p_msg");
                TempData["Message"] = Param.Get<string>("@p_msg");
                TempData["Icon"] = Param.Get<string>("@p_Icon");
                return RedirectToAction("GetQRCodeLocationList", "Setting_NightRound_QRCodeGeneration");

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
    }
}