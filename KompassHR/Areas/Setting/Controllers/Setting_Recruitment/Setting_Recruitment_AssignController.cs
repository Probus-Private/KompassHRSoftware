using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Leave;
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
    public class Setting_Recruitment_AssignController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_Recruitment_Assign
        #region  Assign Main view
        [HttpGet]
        public ActionResult Setting_Recruitment_Assign(int? BranchId, int? CmpId, string RecruitmentAssignID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 38;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";

                Recruitment_Assign RecruitmentAssign = new Recruitment_Assign();
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetCompanyName = GetComapnyName;

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and ContractorID=1 and Employeeid<>1 and EmployeeLeft=0 and  EmployeeBranchId in (Select BranchID from UserBranchMapping where EmployeeID=" + Session["EmployeeId"] + " and IsActive=1) order by Name");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();
                ViewBag.GetEmployeeList = data;
                ViewBag.GetBranchName = "";

                if (RecruitmentAssignID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_RecruitmentAssignID_Encrypted", RecruitmentAssignID_Encrypted);
                    RecruitmentAssign = DapperORM.ReturnList<Recruitment_Assign>("sp_List_Recruitment_Assign", param).FirstOrDefault();

                    DynamicParameters param4 = new DynamicParameters();
                    param4.Add("@p_employeeid", Session["EmployeeId"]);
                    param4.Add("@p_CmpId", RecruitmentAssign.CmpID);
                    ViewBag.GetBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param4).ToList();
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
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        //#region GetEmployeeName
        //[HttpGet]
        //public ActionResult GetEmployeeName(int BranchId,int CmpID)
        //{
        //    try
        //    {
        //        DynamicParameters param = new DynamicParameters();
        //        param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and ContractorID=1 and Employeeid<>1 and EmployeeLeft=0 and EmployeeBranchId='" + BranchId + "' and CmpID='" + CmpID + "' order by Name");
        //        var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
        //        ViewBag.EmployeeList = data;
        //        return Json(data, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }

        //}
        //#endregion

        #region IsValidation
        [HttpGet]
        public ActionResult IsAssignExists(int CmpID, string RecruitmentAssignID_Encrypted, int BranchId, int RecruitmentAssignEmployeeID)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_CmpID", CmpID);
                param.Add("@p_BranchId", BranchId);
                param.Add("@p_RecruitmentAssignEmployeeID", RecruitmentAssignEmployeeID);
                param.Add("@p_RecruitmentAssignID_Encrypted", RecruitmentAssignID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_Assign", param);
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
        public ActionResult SaveUpdate(Recruitment_Assign Assign)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(Assign.RecruitmentAssignID_Encrypted) ? "Save" : "Update");
                param.Add("@p_RecruitmentAssignID", Assign.RecruitmentAssignID);
                param.Add("@p_RecruitmentAssignID_Encrypted", Assign.RecruitmentAssignID_Encrypted);
                param.Add("@p_CmpID", Assign.CmpID);
                param.Add("@p_BranchId", Assign.BranchId);
                param.Add("@p_RecruitmentAssignEmployeeID", Assign.RecruitmentAssignEmployeeID);
                param.Add("@p_IsActive", Assign.IsActive);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Recruitment_Assign", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Recruitment_Assign", "Setting_Recruitment_Assign");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList Views
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 38;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_RecruitmentAssignID_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_Assign", param).ToList();
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

        #region MyRegion
        public ActionResult Delete(string RecruitmentAssignID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_RecruitmentAssignID_Encrypted", RecruitmentAssignID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_Assign", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Recruitment_Assign");
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