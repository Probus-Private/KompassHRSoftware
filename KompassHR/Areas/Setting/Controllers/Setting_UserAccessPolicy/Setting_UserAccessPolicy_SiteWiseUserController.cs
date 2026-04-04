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
    public class Setting_UserAccessPolicy_SiteWiseUserController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_UserAccessPolicy_SiteWiseUser
        #region Setting_UserAccessPolicy_SiteWiseUser
        public ActionResult Setting_UserAccessPolicy_SiteWiseUser(int? EmployeeID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 124;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and ContractorId=1 order by Name");
                var EmpolyeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetEmpolyeeName = EmpolyeeName;


                if (EmployeeID != null)
                {
                    param = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_EmployeeID", EmployeeID);
                    ViewBag.UserSiteMapping = DapperORM.DynamicList("sp_List_UserBranchMapping", param);
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

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetBusinessUnit(int? EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select isnull(UserBranchMapping.IsActive,0) as IsActive,Mas_CompanyProfile.CompanyId,Mas_CompanyProfile.CompanyName,Mas_Branch.BranchId,Mas_Branch.BranchName from Mas_Branch INNER JOIN  Mas_CompanyProfile on Mas_Branch.CmpId = Mas_CompanyProfile.CompanyId left join UserBranchMapping on UserBranchMapping.BranchID=Mas_Branch.BranchId and EmployeeID='" + EmployeeId + "' where Mas_Branch.Deactivate = 0 and Mas_CompanyProfile.Deactivate = 0 order by CompanyName , BranchName");
                var CompanyandbranchName = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).ToList();
                //DynamicParameters param = new DynamicParameters();
                //var CompanyName = "select isnull(UserBranchMapping.IsActive,0) as IsActive,Mas_CompanyProfile.CompanyId,Mas_CompanyProfile.CompanyName,Mas_Branch.BranchId,Mas_Branch.BranchName from Mas_Branch INNER JOIN  Mas_CompanyProfile on Mas_Branch.CmpId = Mas_CompanyProfile.CompanyId left join UserBranchMapping on UserBranchMapping.BranchID=Mas_Branch.BranchId and EmployeeID='" + EmployeeId + "' where Mas_Branch.Deactivate = 0 and Mas_CompanyProfile.Deactivate = 0 order by CompanyName , BranchName";
                //var CompanyandbranchName = DapperORM.DynamicQuerySingle(CompanyName);
                return Json(CompanyandbranchName, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetEmployee
        public ActionResult GetEmployee(int? EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select count(EmployeeID) as EmployeeID from UserBranchMapping where IsActive=1 and EmployeeID = " + EmployeeId + "");
                var query = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).ToList();

                //var query = DapperORM.DynamicQuerySingle("select count(EmployeeID) as EmployeeID from UserBranchMapping where IsActive=1 and EmployeeID = " + EmployeeId + "");
                //var employeeCount = DapperORM.DynamicQuerySingle<int>(query);               
                return Json(query, JsonRequestBehavior.AllowGet);
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
        public ActionResult SaveUpadte(List<UserBranchMapping> usersitemap)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (usersitemap.Count > 0)
                {

                    for (var i = 0; i < usersitemap.Count; i++)
                    {

                        param.Add("@p_process", "Save");
                        param.Add("@p_BranchID", usersitemap[i].BranchID);
                        param.Add("@p_CmpID", usersitemap[i].CmpID);
                        param.Add("@p_EmployeeID", usersitemap[i].EmployeeID);
                        param.Add("@p_IsActive", usersitemap[i].IsActive);
                        param.Add("@p_MachineName", Dns.GetHostName().ToString());
                        param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        var data = DapperORM.ExecuteReturn("sp_SUD_UserBranchMapping", param);
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 124;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_MapId_Encrypted", "List");
                var data = DapperORM.ReturnList<dynamic>("sp_List_UserBranchMapping", param).ToList();
                ViewBag.getEmployeeName = data;
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
        public ActionResult Delete(int? EmployeeID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_EmployeeID", EmployeeID);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_UserBranchMapping", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_UserAccessPolicy_SiteWiseUser");
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