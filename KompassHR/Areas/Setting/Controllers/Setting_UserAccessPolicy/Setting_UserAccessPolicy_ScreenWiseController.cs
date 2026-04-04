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
    public class Setting_UserAccessPolicy_ScreenWiseController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_UserAccessPolicy_ScreenWise

        #region MainView
        public ActionResult Setting_UserAccessPolicy_ScreenWise()
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
                ViewBag.FormName=DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", paramForm).ToList();

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_AccessPolicyId_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Tool_UserAccessPolicyMaster", param1);
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

        #region GetFormName
        [HttpGet]
        public ActionResult GetFormName(int UserGroupDetails_ModuleID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters paramForm = new DynamicParameters();
                paramForm.Add("@query", "Select ScreenMasterId as Id,ScreenDisplayMenuName As Name  from Tool_ScreenMaster where Deactivate=0 and IsActive=1 and ScreenModuleId='"+ UserGroupDetails_ModuleID + "'");
                var data = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", paramForm).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);                
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region CheckAccess
        public ActionResult checkAccess(int UserGroupDetails_ScreenID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters paramForm = new DynamicParameters();
                paramForm.Add("@query", "Select *  from Tool_UserAccessPolicyDetails where  UserGroupDetails_ScreenID='" + UserGroupDetails_ScreenID + "'");
                var data = DapperORM.ReturnList<dynamic>("sp_QueryExcution", paramForm).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
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
        public ActionResult IsUserAssccesExists(string UserGroupDetails_ScreenID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                //param.Add("@p_process", "IsValidation");
                //param.Add("@P_UserGroupId_Encrypted", UserGroupIdEncrypted);
                //param.Add("@P_UserGroupName", UserGroupName);
                //param.Add("@p_MachineName", Dns.GetHostName().ToString());
                //param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                //param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                //param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                //var Result = DapperORM.ExecuteReturn("sp_SUD_Tool_UserAccessPolicyMaster", param);
                //var Message = param.Get<string>("@p_msg");
                //var Icon = param.Get<string>("@p_Icon");
                //if (Message != "")
                //{
                //    return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    return Json(true, JsonRequestBehavior.AllowGet);
                //}

                return Json("", JsonRequestBehavior.AllowGet);
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
        public ActionResult SaveUpdate( List<Tool_UserAccessPolicyDetails> UserAccessRecoardList,int UserGroupDetails_ScreenID,string AddOrRemove,int UserGroupDetails_ModuleID)
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
                    //          new { UserAccessRecoardList[i].UserGroupDetails_UserGroupID }).FirstOrDefault();
                        var UserGroupIdEncrypted = Convert.ToString(UserGroupIdEnc.UserGroupId_Encrypted);

                        param.Add("@p_process", string.IsNullOrEmpty(UserGroupIdEncrypted) ? "Save" : "Update");
                        param.Add("@P_UserGroupName", UserGroupIdEnc.UserGroupName);
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
                    if (UserGroupDetails_ScreenID != null)
                    {
                        var sql = @"DELETE FROM Tool_UserAccessPolicyDetails  WHERE UserGroupDetails_ScreenID = @ScreenID AND UserGroupDetails_UserGroupID = @GroupID";
                        DapperORM.Executes(sql, new { ScreenID = screenId, GroupID = groupId });
                        //sqlcon.Query("delete from Tool_UserAccessPolicyDetails where UserGroupDetails_ScreenID = '" + UserGroupDetails_ScreenID + "' and UserGroupDetails_UserGroupID= '"+ UserAccessRecoardList[i].UserGroupDetails_UserGroupID + "'",
                        //            new { UserGroupDetails_ScreenID });
                    }
                    else
                    {
                        var sql = @"DELETE FROM Tool_UserAccessPolicyDetails  WHERE UserGroupDetails_ScreenID = @ScreenID";
                        DapperORM.Executes(sql, new { ScreenID = screenId });
                        //sqlcon.Query("delete from Tool_UserAccessPolicyDetails  where UserGroupDetails_ScreenID ='" + UserGroupDetails_ScreenID + "'");
                    }                    
                }

                //Delete with screenId and Usergroup
                if (AddOrRemove == "Add")
                {
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
                                UserGroupDetails_ModuleID= UserGroupDetails_ModuleID,
                            });
                        }
                        var rowsAffected = connection.Execute(sql, ToolUserPolicy);
                    }
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
        
    }
}