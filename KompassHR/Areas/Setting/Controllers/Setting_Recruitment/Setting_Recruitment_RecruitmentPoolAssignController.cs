using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Recruitment;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Recruitment
{
    public class Setting_Recruitment_RecruitmentPoolAssignController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_Recruitment_RecruitmentPoolAssign

        #region  Setting_Recruitment_RecruitmentPoolAssign Main view
        [HttpGet]
        public ActionResult Setting_Recruitment_RecruitmentPoolAssign(string RecruitmentPoolAssignId_Encrypted, RecruitmentPoolAssign RecruitmentAssign)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) :861;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";

              //   RecruitmentPoolAssign RecruitmentAssign = new RecruitmentPoolAssign();
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_Employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetCompanyName = GetComapnyName;

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@query", "select EmployeeId As Id,Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee  where Deactivate=0 AND EmployeeLeft=0 Order By Name");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();
                ViewBag.GetEmployeeList = data;
                ViewBag.GetBranchName = "";

                if (RecruitmentPoolAssignId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_RecruitmentPoolAssignId_Encrypted", RecruitmentPoolAssignId_Encrypted);
                    RecruitmentAssign = DapperORM.ReturnList<RecruitmentPoolAssign>("sp_List_Recruitment_RecruitmentPoolAssign", param).FirstOrDefault();

                    DynamicParameters paramBranchName = new DynamicParameters();
                    paramBranchName.Add("@query", "select BranchId as Id, BranchName as Name  from Mas_Branch where Deactivate = 0 and CmpId ='" + RecruitmentAssign.CmpId + "'  order by Name");
                    var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranchName).ToList();
                    ViewBag.GetBranchName = Branch;

                    //DynamicParameters param4 = new DynamicParameters();
                    //param4.Add("@p_employeeid", Session["EmployeeId"]);
                    //param4.Add("@p_CmpId", RecruitmentAssign.CmpId);
                    //ViewBag.GetBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param4).ToList();
                }
                return View(RecruitmentAssign);
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
             //   DynamicParameters param = new DynamicParameters();
               // param.Add("@query", "select BranchId As Id,BranchName As Name from Mas_Branch  where CmpId='" + CmpId + "'");
              //  var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                DynamicParameters paramBranchName = new DynamicParameters();
                paramBranchName.Add("@query", "select BranchId as Id, BranchName as Name  from Mas_Branch where Deactivate = 0 and CmpId ='" + CmpId + "'  order by Name");
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranchName).ToList();
                ViewBag.GetBranchName = Branch;
                return Json(new { Branch = Branch}, JsonRequestBehavior.AllowGet);

                // return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region IsValidation
        [HttpGet]
        public ActionResult IsAssignExists(int CmpId, string RecruitmentPoolAssignId_Encrypted, int BranchId, int RecruitmentAssignEmployeeId)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_CmpId", CmpId);
                param.Add("@p_BranchId", BranchId);
                param.Add("@p_RecruitmentAssignEmployeeId", RecruitmentAssignEmployeeId);
                param.Add("@p_RecruitmentPoolAssignId_Encrypted", RecruitmentPoolAssignId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_RecruitmentPoolAssign", param);
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
        public ActionResult SaveUpdate(RecruitmentPoolAssign Assign)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(Assign.RecruitmentPoolAssignId_Encrypted) ? "Save" : "Update");
                param.Add("@p_RecruitmentPoolAssignId", Assign.RecruitmentPoolAssignId);
                param.Add("@p_RecruitmentPoolAssignId_Encrypted", Assign.RecruitmentPoolAssignId_Encrypted);
                param.Add("@p_CmpId", Assign.CmpId);
                param.Add("@p_BranchId", Assign.BranchId);
                param.Add("@p_RecruitmentAssignEmployeeId", Assign.RecruitmentAssignEmployeeId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var data = DapperORM.ExecuteReturn("sp_SUD_Recruitment_RecruitmentPoolAssign", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Recruitment_RecruitmentPoolAssign", "Setting_Recruitment_RecruitmentPoolAssign");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) :861;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_RecruitmentPoolAssignId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_RecruitmentPoolAssign", param).ToList();
                ViewBag.GetAssignList = data;
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
        public ActionResult Delete(string RecruitmentPoolAssignId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_RecruitmentPoolAssignId_Encrypted", RecruitmentPoolAssignId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_RecruitmentPoolAssign", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Recruitment_RecruitmentPoolAssign");
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