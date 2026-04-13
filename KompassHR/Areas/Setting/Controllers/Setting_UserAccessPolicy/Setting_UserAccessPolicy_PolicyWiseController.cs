using Dapper;
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
    public class Setting_UserAccessPolicy_PolicyWiseController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_UserAccessPolicy_PolicyWise
        public ActionResult Setting_UserAccessPolicy_PolicyWise(string UserGroupId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                //CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 122;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Tool_UserAccessPolicyMaster ToolUserAccessPolicyMaster = new Tool_UserAccessPolicyMaster();
                ViewBag.GetUserAccessPolicyMaster = DapperORM.ExecuteSP<dynamic>("sp_Access_GetScreenList").ToList();

                if (UserGroupId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_AccessPolicyId_Encrypted", UserGroupId_Encrypted);
                    // param.Add("@p_UserGroupId", UserGroupId);
                    ViewBag.UserAccessPolicyMaster = DapperORM.ExecuteSP<dynamic>("sp_List_Tool_UserAccessPolicyMaster", param).ToList();
                }
                return View(ToolUserAccessPolicyMaster);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


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
        public ActionResult SaveUpdate(string UserGroupName, string UserGroupIdEncrypted, List<Tool_UserAccessPolicyDetails> UserAccessRecoardList)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // param.Add("@p_process", "Save");
                param.Add("@p_process", string.IsNullOrEmpty(UserGroupIdEncrypted) ? "Save" : "Update");
                param.Add("@P_UserGroupName", UserGroupName);
                param.Add("@P_UserGroupId_Encrypted", UserGroupIdEncrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter   
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter 
                var Result = DapperORM.ExecuteReturn("sp_SUD_Tool_UserAccessPolicyMaster", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                var p_Id = param.Get<string>("@p_Id");

                //var Id = DapperORM.DynamicQueryList($@"select UserGroupId from Tool_UserAccessPolicyMaster where UserGroupId_Encrypted = { @UserGroupIdEncrypted}").FirstOrDefault();
                //DapperORM.DynamicQueryList($@"delete from Tool_UserAccessPolicyDetails where UserGroupDetails_UserGroupID = {@Id}").FirstOrDefault();

                string connectionString = Session["MyNewConnectionString"]?.ToString();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    List<Tool_UserAccessPolicyDetails> ToolUserPolicy = new List<Tool_UserAccessPolicyDetails>();
                    if (UserGroupIdEncrypted != null)
                    {
                        var Id = connection.Query<int>("select UserGroupId from Tool_UserAccessPolicyMaster where UserGroupId_Encrypted = @UserGroupIdEncrypted",
                           new { UserGroupIdEncrypted }).FirstOrDefault();
                        connection.Query("delete from Tool_UserAccessPolicyDetails where UserGroupDetails_UserGroupID = @Id",
                                    new { Id });
                        //var Id= sqlcon.Query("select UserGroupId from Tool_UserAccessPolicyMaster  where UserGroupId_Encrypted ='" + UserGroupIdEncrypted + "'").FirstOrDefault();
                        // sqlcon.Query("delete from Tool_UserAccessPolicyDetails  where UserGroupDetails_UserGroupID ='" + Id + "'");
                    }
                    else
                    {
                        connection.Query("delete from Tool_UserAccessPolicyDetails  where UserGroupDetails_UserGroupID ='" + p_Id + "'");
                    }
                    var sql = @"INSERT INTO Tool_UserAccessPolicyDetails (
                                     UserGroupDetails_UserGroupID ,IsMenu , UserGroupDetails_ScreenID )
                            VALUES (@UserGroupDetails_UserGroupID , @IsMenu , @UserGroupDetails_ScreenID )";
                    for (int i = 0; i < UserAccessRecoardList.Count; i++)
                    {
                        ToolUserPolicy.Add(new Tool_UserAccessPolicyDetails()
                        {
                            UserGroupDetails_UserGroupID = Convert.ToInt32(p_Id),
                            IsMenu = UserAccessRecoardList[i].IsMenu,
                            UserGroupDetails_ScreenID = UserAccessRecoardList[i].UserGroupDetails_ScreenID,
                        });
                    }
                    var rowsAffected = connection.Execute(sql, ToolUserPolicy);
                    TempData["Message"] = message;
                    TempData["Icon"] = Icon;
                }
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
                var data = DapperORM.DynamicList("sp_List_Tool_UserAccessPolicyMaster", param);
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
                return RedirectToAction("GetList", "Setting_UserAccessPolicy_PolicyWise");
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