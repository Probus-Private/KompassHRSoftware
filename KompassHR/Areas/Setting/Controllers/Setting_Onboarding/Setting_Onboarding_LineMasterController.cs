using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Leave;
using KompassHR.Areas.Setting.Models.Setting_Onboarding;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Onboarding
{
    public class Setting_Onboarding_LineMasterController : Controller
    {
        // GET: Setting/Setting_Onboarding_LineMaster
        DynamicParameters param = new DynamicParameters();
        #region LineMaster Main View
        [HttpGet]
        public ActionResult Setting_Onboarding_LineMaster(string LineId_Encrypted,int? CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 12;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Mas_LineMaster mas_linemaster = new Mas_LineMaster();
                ViewBag.AddUpdateTitle = "Add";

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.ComapnyProfile = GetComapnyName;

                //param.Add("@query", "select CompanyId As Id,CompanyName AS Name from Mas_CompanyProfile where Deactivate=0 order by CompanyName");
                //var listMasCompanyProfile = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                //ViewBag.ComapnyProfile = listMasCompanyProfile;
                if (LineId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_LineId_Encrypted", LineId_Encrypted);
                    mas_linemaster = DapperORM.ReturnList<Mas_LineMaster>("sp_List_Mas_LineMaster", param).FirstOrDefault();
                }
                if (CmpId != null)
                {
                    DynamicParameters paramCompany1 = new DynamicParameters();
                    paramCompany1.Add("@query", "select BranchId,BranchName from Mas_Branch where Deactivate=0 and CmpId= '" + CmpId + "' order by BranchName");
                    var listMasBranch = DapperORM.ReturnList<Mas_Branch>("sp_QueryExcution", paramCompany1);
                    ViewBag.MasBranch = listMasBranch;
                }
                else
                {
                    ViewBag.MasBranch = "";
                }
                return View(mas_linemaster);
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
        public ActionResult IsLineMasterExists(string LineName, string LineId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_LineName", LineName);
                    param.Add("@p_LineId_Encrypted", LineId_Encrypted);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_LineMaster", param);
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

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Mas_LineMaster LineMaster)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(LineMaster.LineId_Encrypted) ? "Save" : "Update");
                param.Add("@p_LineId", LineMaster.LineId);
                param.Add("@p_LineId_Encrypted", LineMaster.LineId_Encrypted);
                param.Add("@p_LineName", LineMaster.LineName);
                param.Add("@p_CmpId", LineMaster.CmpId);
                var i = LineMaster.IsDefault;
                param.Add("@p_IsDefault", LineMaster.IsDefault);
                param.Add("@p_BranchId", LineMaster.BranchId);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_LineMaster", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Onboarding_LineMaster", "Setting_Onboarding_LineMaster");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 12;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_LineId_Encrypted", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                ViewBag.LineMaster = DapperORM.DynamicList("sp_List_Mas_LineMaster", param);
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
        public ActionResult Delete(string LineId_Encrypted,int LineId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_LineId_Encrypted", LineId_Encrypted);
                param.Add("@p_LineId", LineId);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_LineMaster", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Onboarding_LineMaster");
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
                    return RedirectToAction("Login", "Login", new { Area = "" });
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

    }
}