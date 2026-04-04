using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Leave;
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
    public class Setting_UserCopyController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_UserCopy
        #region Setting_UserCopy
        public ActionResult Setting_UserCopy(ReplicationUserRight model)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 125;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
               
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetCompanyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.CompanyName = GetCompanyName;
                var BUDI = GetCompanyName[0].Id;
                DynamicParameters paramBranch1 = new DynamicParameters();
                paramBranch1.Add("@p_employeeid", Session["EmployeeId"]);
                paramBranch1.Add("@p_CmpId", BUDI);
                var BranchNameList = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch1).ToList();
                ViewBag.BranchName = BranchNameList;

                DynamicParameters paramAccessPolicy = new DynamicParameters();
                paramAccessPolicy.Add("@query", "Select UserGroupId as Id,UserGroupName as Name from Tool_UserAccessPolicyMaster where Deactivate=0");
                var UserAccessPolicyMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramAccessPolicy).ToList();
                ViewBag.UserAccessPolicyMaster = UserAccessPolicyMaster;
                var branchId = Request.Form["BranchId"];
                if (model.CmpId != null)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_EmployeeBranchId", model.BranchId);
                    var GetEmployeePolicy = DapperORM.ExecuteSP<dynamic>("sp_Access_GetEmployeePolicy", param1).ToList();
                    ViewBag.GetEmployeePolicy = GetEmployeePolicy;

                    param = new DynamicParameters();
                    param.Add("@p_CmpId", model.CmpId);
                    param.Add("@p_employeeid", Session["EmployeeId"]);
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                    DynamicParameters paramBranch2 = new DynamicParameters();
                    paramBranch2.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBranch2.Add("@p_CmpId", model.CmpId);
                    var BranchNameList1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch2).ToList();
                    ViewBag.BranchName = BranchNameList1;
                }
                else
                {
                    ViewBag.GetEmployeePolicy = "";
                }

                return View(model);
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
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param = new DynamicParameters();
                param.Add("@p_CmpId", CmpId);
                param.Add("@p_employeeid", Session["EmployeeId"]);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }

        }
        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(int PolicyId, List<ReplicationUserRight> RecordList)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                if (RecordList != null)
                {
                    //for (var i = 0; i < RecordList.Count; i++)
                    //{
                    //    var GetLeavingDate = RecordList[i].EmployeeId.ToString("yyyy-MM-dd");
                    //    DapperORM.DynamicQuerySingle("Update Mas_Employee_ESS set UserAccessPolicyId=" + PolicyId + " where ESSEmployeeId=" + RecordList[i].EmployeeId + " and Mas_Employee_ESS.Deactivate=0");
                    //}
                    using (var connection = new SqlConnection(DapperORM.connectionString))
                    {

                        List<ReplicationUserRight> ToolUserPolicy = new List<ReplicationUserRight>();
                        var sql1 = "Update Mas_Employee_ESS Set UserAccessPolicyId=@PolicyId Where ESSEmployeeId=@EmployeeId and Mas_Employee_ESS.Deactivate=0";
                        for (int i = 0; i < RecordList.Count; i++)
                        {
                            ToolUserPolicy.Add(new ReplicationUserRight()
                            {
                                PolicyId = Convert.ToInt32(PolicyId),
                                EmployeeId = RecordList[i].EmployeeId,
                            });
                        }
                        var rowsAffected1 = connection.Execute(sql1, ToolUserPolicy);


                        TempData["Message"] = "Record updated successfully";
                        TempData["Icon"] = "success";

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