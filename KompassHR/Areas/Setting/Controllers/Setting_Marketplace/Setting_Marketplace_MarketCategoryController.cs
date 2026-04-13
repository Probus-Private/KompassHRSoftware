using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Marketplace;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Marketplace
{
    public class Setting_Marketplace_MarketCategoryController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: MarketPlaceSetting/MarketCategory
        public ActionResult Setting_Marketplace_MarketCategory(string MarketPlaceCategoryID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                ViewBag.AddUpdateTitle = "Add";
                MarketPlace_Category MarketPlaceCategory = new MarketPlace_Category();
                if (MarketPlaceCategoryID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@P_MarketPlaceCategoryID_Encrypted", MarketPlaceCategoryID_Encrypted);
                    MarketPlaceCategory = DapperORM.ReturnList<MarketPlace_Category>("sp_List_MarketPlace_Category", param).FirstOrDefault();
                }
                return View(MarketPlaceCategory);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        public ActionResult SaveUpdate(MarketPlace_Category marketPlaceCatagory)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(marketPlaceCatagory.MarketPlaceCategoryID_Encrypted) ? "Save" : "Update");
                param.Add("@p_MarketPlaceCategoryID", marketPlaceCatagory.MarketPlaceCategoryID);
                param.Add("@p_MarketPlaceCategoryID_Encrypted", marketPlaceCatagory.MarketPlaceCategoryID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_MarketPlaceCategory", marketPlaceCatagory.MarketPlaceCategory);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_MarketPlace_Category", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Marketplace_MarketCategory", "Setting_Marketplace_MarketCategory");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult IsMarketCategoryExists(string PlaceCategory, string MarketPlaceCategoryIDEncrypted)
        {
            try
            {

                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {

                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_MarketPlaceCategory", PlaceCategory);
                    param.Add("@p_MarketPlaceCategoryID_Encrypted", MarketPlaceCategoryIDEncrypted);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_MarketPlace_Category", param);
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


        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_MarketPlaceCategoryID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_MarketPlace_Category", param);
                ViewBag.GetMarketCategory = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        public ActionResult Delete(string MarketPlaceCategoryID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_MarketPlaceCategoryID_Encrypted", MarketPlaceCategoryID_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_MarketPlace_Category", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Marketplace_MarketCategory");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
    }
}