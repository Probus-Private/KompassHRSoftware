using Dapper;
using KompassHR.Areas.Setting.Models.Setting_UserAccessPolicy;
using KompassHR.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_UserAccessPolicy
{
    public class Setting_UserAccessPolicy_UserPolicyMappingController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_UserAccessPolicy_UserPolicyMapping
        #region Main View
        public ActionResult Setting_UserAccessPolicy_UserPolicyMapping(int? EmployeeID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 447;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and ContractorId=1 and EmployeeLeft=0 order by Name");
                var EmpolyeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetEmpolyeeName = EmpolyeeName;

                if (EmployeeID != null)
                {
                    param.Add("@query", $@"select MapPolicyId,IsActive,UserGroupId,UserGroupName from Tool_UserAccessPolicyMaster
                                        left outer join  Tool_UserPolicyMapping on UserGroupId = MapPolicyId and EmployeeId = {EmployeeID}
                                        where Tool_UserAccessPolicyMaster.Deactivate = 0 ");
                    var data = DapperORM.ExecuteSP<PolicyMapping>("sp_QueryExcution", param).ToList();
                    ViewBag.GetUserPolicyList = data;
                }
                else
                {
                    ViewBag.GetUserPolicyList = null;
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

        #region SaveUpadte
        [HttpPost]
        public ActionResult SaveUpadte(List<PolicyMapping> userpolicymap, string EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 447;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters paramCheck = new DynamicParameters();
                paramCheck.Add("@query", $@"SELECT COUNT(1) FROM Tool_UserPolicyMapping WHERE EmployeeID = {EmployeeId}");
                var GetCheck = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramCheck).FirstOrDefault();
                if (GetCheck!=null)
                {
                    DapperORM.DynamicQuerySingle($@"DELETE FROM Tool_UserPolicyMapping WHERE EmployeeID = {EmployeeId}");
                }

                if (userpolicymap == null)
                {
                    param.Add("@p_process", "Delete");
                    param.Add("@p_EmployeeID", EmployeeId);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_SUD_UserPolicyMapping", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }
                if (userpolicymap.Count > 0)
                {
                    for (var i = 0; i < userpolicymap.Count; i++)
                    {
                        param.Add("@p_process", "Save");
                        param.Add("@p_EmployeeID", userpolicymap[i].EmployeeID);
                        param.Add("@p_IsActive", userpolicymap[i].IsActive);
                        param.Add("@p_MapPolicyId", userpolicymap[i].MapPolicyId);
                        param.Add("@p_MachineName", Dns.GetHostName().ToString());
                        param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        var data = DapperORM.ExecuteReturn("sp_SUD_UserPolicyMapping", param);
                        TempData["Message"] = param.Get<string>("@p_msg");
                        TempData["Icon"] = param.Get<string>("@p_Icon");
                    }
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
    }
}