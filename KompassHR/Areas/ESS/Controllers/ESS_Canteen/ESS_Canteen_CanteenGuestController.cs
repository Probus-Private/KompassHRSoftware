using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Canteen;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Canteen
{
    public class ESS_Canteen_CanteenGuestController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/ESS_Canteen_CanteenGuest
        #region CanteenGuest Main View
        public ActionResult ESS_Canteen_CanteenGuest(string CanteenGuestID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 209;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";
                Canteen_GuestMaster CanteenMaster = new Canteen_GuestMaster();
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var GetDocNo = "Select isnull(Max(DocNo),0)+1 As DocNo from Canteen_GuestMaster";
                    var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    ViewBag.DocNo = DocNo;
                }
                var EmployeeId = Session["EmployeeId"];
                param = new DynamicParameters();
                var CanteenLastRecord = "select max(CreatedDate) As CreatedDate from Canteen_GuestMaster where CanteenGuestHostEmployeeID ="+ EmployeeId + " and Deactivate = 0";
                var LastRecored = DapperORM.DynamicQuerySingle(CanteenLastRecord);
                ViewBag.LastRecored = LastRecored.CreatedDate;

                if (CanteenGuestID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_CanteenGuestID_Encrypted", CanteenGuestID_Encrypted);
                    CanteenMaster = DapperORM.ReturnList<Canteen_GuestMaster>("sp_List_Canteen_GuestMaster", param).FirstOrDefault();

                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {
                        var GetDocNo = "Select DocNo As DocNo from Canteen_GuestMaster where CanteenGuestID_Encrypted='" + CanteenGuestID_Encrypted + "'";
                        var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                        ViewBag.DocNo = DocNo;
                    }
                }
                TempData["DocDate"] = CanteenMaster.DocDate;
                TempData["CanteenGuestFormDate"] = CanteenMaster.CanteenGuestFormDate;
                TempData["CanteenGuestToDate"] = CanteenMaster.CanteenGuestToDate;
                return View(CanteenMaster);
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_Canteen_CanteenGuest");
            }

        }
        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Canteen_GuestMaster CanteenGuest)
        {
            try
            {
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(CanteenGuest.CanteenGuestID_Encrypted) ? "Save" : "Update");
                param.Add("@p_CanteenGuestID", CanteenGuest.CanteenGuestID);
                param.Add("@p_CanteenGuestID_Encrypted", CanteenGuest.CanteenGuestID_Encrypted);
                param.Add("@p_DocNo", CanteenGuest.DocNo);
                param.Add("@p_DocDate", CanteenGuest.DocDate);
                param.Add("@p_CanteenGuestHostEmployeeID", EmployeeId);
                param.Add("@p_NoOfGuest", CanteenGuest.NoOfGuest);
                param.Add("@p_FoodPreference", CanteenGuest.FoodPreference);
                param.Add("@p_MorningBreakfast", CanteenGuest.MorningBreakfast);
                param.Add("@p_EveningBreakfast", CanteenGuest.EveningBreakfast);
                param.Add("@p_Lunch", CanteenGuest.Lunch);
                param.Add("@p_Dinner", CanteenGuest.Dinner);
                param.Add("@p_Brunch", CanteenGuest.Brunch);
                param.Add("@p_CanteenGuestFormDate", CanteenGuest.CanteenGuestFormDate);
                param.Add("@p_CanteenGuestToDate", CanteenGuest.CanteenGuestToDate);
                param.Add("@p_CanteenGuestRemark", CanteenGuest.CanteenGuestRemark);

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Canteen_GuestMaster", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_Canteen_CanteenGuest", "ESS_Canteen_CanteenGuest");
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_Canteen_CanteenGuest");
            }
        }
        #endregion

        #region GetList View
        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 209;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_CanteenGuestID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Canteen_GuestMaster", param);
                ViewBag.GetCanteenGuestList = data;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_Canteen_CanteenGuest");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(string CanteenGuestID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_CanteenGuestID_Encrypted", CanteenGuestID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Canteen_GuestMaster", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_Canteen_CanteenGuest");
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Canteen_CanteenGuest");
            }
        }
        #endregion
    }
}