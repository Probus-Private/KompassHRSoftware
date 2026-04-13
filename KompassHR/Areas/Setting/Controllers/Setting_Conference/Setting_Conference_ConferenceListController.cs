using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Conference;
using KompassHR.Areas.Setting.Models.Setting_TimeOffice;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Conference
{
    public class Setting_Conference_ConferenceListController : Controller
    {
        // GET: ConferenceSetting/ConferenceSettingList
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region ConferenceSettingList Main View Page
        public ActionResult Setting_Conference_ConferenceList(string ConferenceListID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Conference_List ObjConference_List = new Conference_List();

                param.Add("@query", "select BranchId,BranchName from Mas_Branch where Deactivate=0 ");
                var listMasBranch = DapperORM.ExecuteSP<Mas_Branch>("sp_QueryExcution", param).ToList();
                ViewBag.MasBranch = listMasBranch;

                if (ConferenceListID_Encrypted != null)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    param1.Add("@p_ConferenceListID_Encrypted", ConferenceListID_Encrypted);
                    ObjConference_List = DapperORM.ReturnList<Conference_List>("sp_List_Conference_List", param1).FirstOrDefault();
                    //return View(ObjConference_List);
                }
                return View(ObjConference_List);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region IsVAlidation
        [HttpGet]
        public ActionResult IsConferenceExists(string ConferenceName, string BranchID,string ConferenceListID_Encrypted)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_ConferenceName", ConferenceName);
                    param.Add("@p_BranchID", BranchID);
                    param.Add("@p_ConferenceListID_Encrypted", ConferenceListID_Encrypted);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Bank", param);
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

        #region SaveUpdate ConferenceSetting

        [HttpPost]
        public ActionResult SaveUpdate(Conference_List ObjConferenceSetting)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(ObjConferenceSetting.ConferenceListID_Encrypted) ? "Save" : "Update");
                param.Add("@p_ConferenceListID_Encrypted", ObjConferenceSetting.ConferenceListID_Encrypted);               
                param.Add("@p_BranchID", ObjConferenceSetting.BranchID);
                param.Add("@p_CmpID", Session["CompanyId"]);
                param.Add("@p_ConferenceName", ObjConferenceSetting.ConferenceName);
                param.Add("@p_ConferenceDescription", ObjConferenceSetting.ConferenceDescription);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Conference_List", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Conference_ConferenceList", "Setting_Conference_ConferenceList");
            }

            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ConferenceSettingGetList Main View Page
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // DynamicParameters ClaimTravel = new DynamicParameters();
                param.Add("@p_ConferenceListID_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Conference_List", param).ToList();
                if (data != null)
                {
                    ViewBag.GetConference_List = data;
                }
                else
                {
                    ViewBag.GetConference_List = "";
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

        #region Delete TravelClaim
        public ActionResult Delete(string ConferenceListID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_ConferenceListID_Encrypted", ConferenceListID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Conference_List", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Conference_ConferenceList", new { Area = "Setting" });
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