using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_TimeOffice
{
    public class ESS_TimeOffice_LocationMappingController : Controller
    {
        // GET: ESS/ESS_TimeOffice_LocationMapping
        DynamicParameters param = new DynamicParameters();
        #region MainView
        public ActionResult ESS_TimeOffice_LocationMapping(string LocationRegistrationMappingIMasterId_Encrypted,int? LocationRegistrationMappingIMasterId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 635;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";              

                DynamicParameters param1 = new DynamicParameters();
                string sqlQuery;
                sqlQuery = @"SELECT 
                    LocationRegistrationId,
                    'unchecked' AS checkbox,
                    LocationName
                 FROM Mas_LocationRegistration
                 WHERE Deactivate = 0";

                Mas_LocationRegistrationMapping_Master LocationGroup = new Mas_LocationRegistrationMapping_Master();
                if (LocationRegistrationMappingIMasterId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    DynamicParameters Group = new DynamicParameters();
                    Group.Add("@p_LocationRegistrationMappingIMasterId_Encrypted", LocationRegistrationMappingIMasterId_Encrypted);
                    LocationGroup = DapperORM.ReturnList<Mas_LocationRegistrationMapping_Master>("sp_List_Mas_LocationRegistrationMapping_Master", Group).FirstOrDefault();
    
                        sqlQuery = @"SELECT 
                    Mas_LocationRegistration.LocationRegistrationId,
                    CASE 
                        WHEN Mas_LocationRegistrationMapping_Detail.LocationRegistrationMappingIMasterId IS NULL 
                        THEN 'unchecked' 
                        ELSE 'checked' 
                    END AS checkbox,
                    Mas_LocationRegistration.LocationName
                 FROM Mas_LocationRegistration
                 LEFT JOIN Mas_LocationRegistrationMapping_Detail 
                    ON Mas_LocationRegistration.LocationRegistrationId = Mas_LocationRegistrationMapping_Detail.LocationRegistrationId 
                    AND Mas_LocationRegistrationMapping_Detail.LocationRegistrationMappingIMasterId = " + LocationRegistrationMappingIMasterId + @" 
                 WHERE Mas_LocationRegistration.Deactivate = 0";
                   
                }

                param1.Add("@query", sqlQuery);
                ViewBag.SelectedLocationList = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param1).ToList();
                return View(LocationGroup);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsValidation
        [HttpPost]
        public ActionResult IsLocationGroupExists(string LocationRegistrationMappingIMasterId_Encrypted,string GroupName,  List<Mas_LocationRegistrationMapping_Master> lstLocationGroup)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                List<int> Arraylist = new List<int>() { };
                string result = "";
                
               
                for (var i = 0; i < lstLocationGroup.Count; i++)
                {
                    result = result + lstLocationGroup[i].LocationRegistrationId + ",";                    
                }

                result = result.TrimEnd(',');
                Session["lstLocationGroup"] = result;
                param.Add("@p_process", "IsValidation");
                param.Add("@P_LocationRegistrationMappingIMasterId_Encrypted", LocationRegistrationMappingIMasterId_Encrypted);              
                param.Add("@P_GroupName", GroupName);
                param.Add("@P_LocationgroupLocationIDs", result);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_LocationRegistrationMapping_Master", param);
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
        public ActionResult SaveUpdate(Mas_LocationRegistrationMapping_Master group)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var lstLocationGroup = Session["lstLocationGroup"];

                param.Add("@p_process", string.IsNullOrEmpty(group.LocationRegistrationMappingIMasterId_Encrypted) ? "Save" : "Update");
                param.Add("@P_LocationRegistrationMappingIMasterId_Encrypted", group.LocationRegistrationMappingIMasterId_Encrypted);
               
                param.Add("@P_GroupName", group.GroupName);                
                param.Add("@P_LocationgroupLocationIDs", lstLocationGroup);                            
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
              
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Mas_LocationRegistrationMapping_Master", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return RedirectToAction("ESS_TimeOffice_LocationMapping", "ESS_TimeOffice_LocationMapping");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
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
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 635;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_LocationRegistrationMappingIMasterId_Encrypted", "List");
             //   param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@P_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.DynamicList("sp_List_Mas_LocationRegistrationMapping_Master", param);
                ViewBag.GetTimeAndAttendanceGroupList = data;
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
        public ActionResult Delete(double? LocationRegistrationMappingIMasterId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_LocationRegistrationMappingIMasterId", LocationRegistrationMappingIMasterId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_LocationRegistrationMapping_Master", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_TimeOffice_LocationMapping");
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