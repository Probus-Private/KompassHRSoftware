using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using KompassHR.Areas.Setting.Models.Setting_UserAccessPolicy;
using System.Data;
using System.Net;
using System.Data.SqlClient;
namespace KompassHR.Areas.Setting.Controllers.Setting_UserAccessPolicy
{
    public class Setting_UserAccessPolicy_EmployeeVerifyAPIController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_UserAccessPolicy_EmployeeVerifyAPI
        public ActionResult Setting_UserAccessPolicy_EmployeeVerifyAPI(string VerifyId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 523;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var GetEmployee = new BulkAccessClass().AllEmployeeName();
                ViewBag.AllEmployeeName = GetEmployee;

                ViewBag.AddUpdateTitle = "Add";

                EmployeeVerifyAPI EmployeeVerifyAPI = new EmployeeVerifyAPI();

                if (VerifyId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_VerifyId_Encrypted", VerifyId_Encrypted);
                    EmployeeVerifyAPI = DapperORM.ReturnList<EmployeeVerifyAPI>("sp_List_EmployeeVerifyAPISetting", param).FirstOrDefault();
                }
                return View(EmployeeVerifyAPI);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");

            }
        }

        #region IsValidation
        [HttpGet]
        public ActionResult IsDocumentExists(int EmployeeID, string VerifyId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_EmployeeId", EmployeeID);
                param.Add("@p_VerifyId_Encrypted", VerifyId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_EmployeeVerifyAPISetting", param);
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
        public ActionResult SaveUpdate(EmployeeVerifyAPI EmployeeVerifyAPI)
        {
            try
            {
                    param.Add("@p_process", string.IsNullOrEmpty(EmployeeVerifyAPI.VerifyId_Encrypted) ? "Save" : "Update");
                    param.Add("@P_VerifyId_Encrypted", EmployeeVerifyAPI.VerifyId_Encrypted);
                    param.Add("@p_EmployeeId", EmployeeVerifyAPI.EmployeeId);
                    param.Add("@p_IsAadhar", EmployeeVerifyAPI.IsAadhar);
                    param.Add("@p_IsPAN", EmployeeVerifyAPI.IsPAN);
                    param.Add("@p_IsPassport", EmployeeVerifyAPI.IsPassport);
                    param.Add("@p_IsVoter", EmployeeVerifyAPI.IsVoter);
                    param.Add("@p_IsVehicleRC", EmployeeVerifyAPI.IsVehicleRC);
                    param.Add("@p_IsDrivingLicence", EmployeeVerifyAPI.IsDrivingLicence);
                    param.Add("@p_IsEmployeement", EmployeeVerifyAPI.IsEmployeement);
                    param.Add("@p_IsGeoLocation", EmployeeVerifyAPI.IsGeoLocation);
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_SUD_EmployeeVerifyAPISetting", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    return RedirectToAction("Setting_UserAccessPolicy_EmployeeVerifyAPI", "Setting_UserAccessPolicy_EmployeeVerifyAPI");

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        //#region GetList
        //[HttpGet]
        //public ActionResult GetList(double EmployeeID)
        //{
        //    try
        //    {

        //        DynamicParameters param1 = new DynamicParameters();
        //        param1.Add("@query", "select * from Tool_EmployeeVerifyAPISetting where EmployeeId= " + EmployeeID + " and Deactivate=0");
        //        var List = DapperORM.ReturnList<EmployeeVerifyAPI>("sp_QueryExcution", param1).ToList();

        //        //return Json(new { success = true, List = List?.List ?? "" }, JsonRequestBehavior.AllowGet);
        //        return Json(new { success = true, List = List }, JsonRequestBehavior.AllowGet);


        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
        //}
        //#endregion

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
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 523;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_VerifyId_Encrypted", "List");
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ReturnList<dynamic>("sp_List_EmployeeVerifyAPISetting", param).ToList();
                ViewBag.GetDocumentList = data;
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
        public ActionResult Delete(string VerifyId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_VerifyId_Encrypted", VerifyId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_EmployeeVerifyAPISetting", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_UserAccessPolicy_EmployeeVerifyAPI", new { Area = "Setting" });
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