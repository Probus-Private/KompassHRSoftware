using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Setting.Models.Setting_UserAccessPolicy;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_UserAccessPolicy
{
    public class Setting_UserAccessPolicy_PolicyScreenMappingController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_UserAccessPolicy_PolicyScreenMapping
        public ActionResult Setting_UserAccessPolicy_PolicyScreenMapping(string UserGroupId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 122;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";

                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "Select ModuleID as Id,ModuleName As Name  from Kompass_Modules where Decativate=0 and IsApplicable=1");
                ViewBag.Kompass_Modules = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param).ToList();

                DynamicParameters paramForm = new DynamicParameters();
                paramForm.Add("@query", "Select ScreenMasterId as Id,ScreenDisplayMenuName As Name  from Tool_ScreenMaster where Deactivate=0 and IsActive=1");
                ViewBag.FormName = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", paramForm).ToList();

                DynamicParameters paramType = new DynamicParameters();
                paramType.Add("@query", "Select Distinct(ScreenMenuType) As Name,0 as ID  from Tool_ScreenMaster where Deactivate=0 and IsActive=1  order by ScreenMenuType");
                ViewBag.TypeList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", paramType).ToList();

                ViewBag.AddUpdateTitle = "Add";
                Tool_UserAccessPolicyMaster ToolUserAccessPolicyMaster = new Tool_UserAccessPolicyMaster();
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_AccessPolicyId_Encrypted", "AccessList");
                ViewBag.GetUserAccessPolicyMaster = DapperORM.ExecuteSP<dynamic>("sp_List_Tool_UserAccessPolicyScreenMapping", paramList).ToList();

                if (UserGroupId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_AccessPolicyId_Encrypted", UserGroupId_Encrypted);                 
                    ViewBag.UserAccessPolicyMaster = DapperORM.ExecuteSP<dynamic>("sp_List_Tool_UserAccessPolicyScreenMapping", param).ToList();
                }
                return View(ToolUserAccessPolicyMaster);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region ModuleWiseScreens
        public ActionResult GetModuleWiseScreens(int ModuleId, string Type)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters paramForm = new DynamicParameters();
                paramForm.Add("@query", "Select ScreenMasterId,ScreenDisplayMenuName,ModuleName,ScreenMenuType,DescriptionForUser from Tool_ScreenMaster where Deactivate=0 and IsActive=1 and ScreenModuleId='" + ModuleId + "'and ScreenMenuType ='"+Type+"' ");
                var _list = DapperORM.ReturnList<UserAccess_PolicyScreenMapping>("sp_QueryExcution", paramForm).ToList();
                return Json(_list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsUserAssccesExists
        [HttpGet]
        public ActionResult IsUserAssccesExists(string UserGroupName, string UserGroupIdEncrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "IsValidation");
                param.Add("@P_UserGroupId_Encrypted", UserGroupIdEncrypted);
                param.Add("@P_UserGroupName", UserGroupName);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Tool_UserAccessPolicyMaster", param);
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
        public ActionResult SaveUpdate(List<Tool_UserAccessPolicyDetails> UserAccessRecoardList, string UserGroupIdEncrypted, string UserGroupName)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var message = "";
                var Icon = "";

                for (int i = 0; i < UserAccessRecoardList.Count; i++)
                {
                    var groupId = UserAccessRecoardList[i].UserGroupDetails_UserGroupID;
                    var sql1 = @"SELECT * FROM Tool_UserAccessPolicyMaster WHERE UserGroupId = @UserGroupId";
                    var UserGroupIdEnc = DapperORM.QuerySingle(sql1, new { UserGroupId = groupId });

                    //var UserGroupIdEnc = sqlcon.Query<dynamic>("select * from Tool_UserAccessPolicyMaster where UserGroupId = '" + UserAccessRecoardList[i].UserGroupDetails_UserGroupID + "'",
                    //new { UserAccessRecoardList[i].UserGroupDetails_UserGroupID }).FirstOrDefault();

                    //var UserGroupIdEncrypted = Convert.ToString(UserGroupIdEnc.UserGroupId_Encrypted);

                    param.Add("@p_process", string.IsNullOrEmpty(UserGroupIdEncrypted) ? "Save" : "Update");
                    param.Add("@P_UserGroupName", UserGroupName);
                    param.Add("@P_UserGroupId_Encrypted", UserGroupIdEncrypted);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter   
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter 
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Tool_UserAccessPolicyMaster", param);
                    message = param.Get<string>("@p_msg");
                    Icon = param.Get<string>("@p_Icon");
                    var p_Id = param.Get<string>("@p_Id");

                    var screenId = UserAccessRecoardList[i].UserGroupDetails_ScreenID;
                    if (UserGroupIdEncrypted != null)
                    {
                        var sql = @"DELETE FROM Tool_UserAccessPolicyDetails  WHERE UserGroupDetails_ScreenID = @ScreenID AND UserGroupDetails_UserGroupID = @GroupID";
                        DapperORM.Executes(sql, new { ScreenID = screenId, GroupID = groupId });
                    }
                    else
                    {
                        var sql = @"DELETE FROM Tool_UserAccessPolicyDetails  WHERE UserGroupDetails_UserGroupID = @P_ID";
                        DapperORM.Executes(sql, new { P_ID = p_Id });
                    }
                }

                //Delete with screenId and Usergroup
                
                    using (var connection = new SqlConnection(DapperORM.connectionString))
                    {
                        List<Tool_UserAccessPolicyDetails> ToolUserPolicy = new List<Tool_UserAccessPolicyDetails>();

                        var sql = @"INSERT INTO Tool_UserAccessPolicyDetails (
                                     UserGroupDetails_UserGroupID ,IsMenu , UserGroupDetails_ScreenID ,UserGroupDetails_ModuleID)
                            VALUES (@UserGroupDetails_UserGroupID , @IsMenu , @UserGroupDetails_ScreenID ,@UserGroupDetails_ModuleID)";
                        for (int i = 0; i < UserAccessRecoardList.Count; i++)
                        {
                            ToolUserPolicy.Add(new Tool_UserAccessPolicyDetails()
                            {
                                UserGroupDetails_UserGroupID = Convert.ToInt32(UserAccessRecoardList[i].UserGroupDetails_UserGroupID),
                                IsMenu = UserAccessRecoardList[i].IsMenu,
                                UserGroupDetails_ScreenID = UserAccessRecoardList[i].UserGroupDetails_ScreenID,
                                UserGroupDetails_ModuleID = UserAccessRecoardList[i].UserGroupDetails_ModuleID,
                            });
                        }
                        var rowsAffected = connection.Execute(sql, ToolUserPolicy);
                    }
                
                TempData["Message"] = message;
                TempData["Icon"] = Icon;

                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 122;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_AccessPolicyId_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Tool_UserAccessPolicyScreenMapping", param);
                ViewBag.GetUserAccessList = data;
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
        public ActionResult Delete(string UserGroupId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_UserGroupId_Encrypted", UserGroupId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Tool_UserAccessPolicyMaster", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_UserAccessPolicy_PolicyScreenMapping");
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