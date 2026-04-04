using Dapper;
using KompassHR.Areas.ESS.Models.ESS_ContractorAttendance;
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

namespace KompassHR.Areas.ESS.Controllers.ESS_ContractorAttendance
{
    public class ESS_ContractorAttendance_ContractorMappingController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_ContractorAttendance_ContractorMapping
        #region ESS_ContractorAttendance_ContractorMapping
        public ActionResult ESS_ContractorAttendance_ContractorMapping(Mas_ContractorMapping ContractorMapping)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 359;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.CompanyName = GetComapnyName;
                var BranchIDContractor = GetComapnyName[0].Id;

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", BranchIDContractor);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                ViewBag.BranchName = data;

                DynamicParameters paramEMP = new DynamicParameters();
                paramEMP.Add("@query", "Select ContractorId as Id,ContractorName as Name from Contractor_Master where Deactivate=0 order by Id");
                var GetContractorName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                ViewBag.ContractorName = GetContractorName;

                if (ContractorMapping.CmpID != 0 && ContractorMapping.BranchID != 0)
                {
                    DynamicParameters paramMapping = new DynamicParameters();
                    paramMapping.Add("@query", "Select '1' as CheckBox, ContractorID  from  Mas_ContractorMapping where BranchID =" + ContractorMapping.BranchID + " and CmpID=" + ContractorMapping.CmpID + " and  IsActive=1  union  Select '0' as CheckBox,Contractor_Master.ContractorId  from  Contractor_Master where  Deactivate=0 and ContractorId not in (select  ContractorID from Mas_ContractorMapping where BranchID =" + ContractorMapping.BranchID + " and CmpID=" + ContractorMapping.CmpID + " and  IsActive=1)order by Mas_ContractorMapping.ContractorID");
                    var GetContractorMapping = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramMapping).ToList();
                    ViewBag.GetContractorMappingList = GetContractorMapping;

                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_employeeid", Session["EmployeeId"]);
                    param1.Add("@p_CmpId", ContractorMapping.CmpID);
                    var data1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param1).ToList();
                    ViewBag.BranchName = data1;
                }
                return View(ContractorMapping);
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

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(List<ListContractorMapping> Record, string CmpId, string BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (Record.Count > 0)
                {
                    for (var i = 0; i < Record.Count; i++)
                    {

                        param.Add("@p_process", "Save");
                        param.Add("@p_BranchID", BranchId);
                        param.Add("@p_CmpID", CmpId);
                        param.Add("@p_ContractorID", Record[i].ContractorID);
                        param.Add("@p_IsActive", true);
                        param.Add("@p_MachineName", Dns.GetHostName().ToString());
                        param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        var data = DapperORM.ExecuteReturn("sp_SUD_Mas_ContractorMapping", param);
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
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 359;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_ContractorMapId_Encrypted", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var MasContractorMapping = DapperORM.DynamicList("sp_List_Mas_ContractorMapping", param);
                ViewBag.MasContractorMappingList = MasContractorMapping;
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
        public ActionResult Delete(int? BranchID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_BranchID", BranchID);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_ContractorMapping", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_ContractorAttendance_ContractorMapping");
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