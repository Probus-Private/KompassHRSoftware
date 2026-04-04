using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_ManpowerAllocation
{
    public class ESS_UpdateLineManpowerDesignationMappingController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_UpdateLineManpowerDesignationMapping

        #region MainView
        public ActionResult ESS_UpdateLineManpowerDesignationMapping()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 773;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                ViewBag.GetBranchName = "";
                ViewBag.GetEmployeeList = "";
                ViewBag.List="";
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
                var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
             
                return Json(new { BranchName }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetEmployeeList
        public ActionResult GetEmployees(int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "Select EmployeeId as Id, Concat (EmployeeName,'-',EmployeeNo) As Name  from Mas_Employee where Deactivate=0  and Employeeleft=0 and ContractorID=1 and EmployeeBranchId='" + BranchId + "' Order By EmployeeName");
                var GetEmployeeList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param1).ToList();

                return Json(new { GetEmployeeList }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetList
        public ActionResult GetList(int? CmpId,int? InOutBranchId, string InOutEmployeeId, DateTime? InOutDate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 773;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                ViewBag.GetBranchName = "";
                ViewBag.GetEmployeeList = "";
                ViewBag.List = "";
                if (CmpId!=null)
                {
                    param.Add("@p_CmpId", CmpId);
                    param.Add("@p_BranchId", InOutBranchId);
                    param.Add("@p_EmployeerID", string.IsNullOrEmpty(InOutEmployeeId) ? null : InOutEmployeeId);
                    param.Add("@p_Date", InOutDate);
                    var List = DapperORM.DynamicList("sp_List_UpdateLineManpowerDesignation", param);
                    ViewBag.List = List;

                   
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_employeeid", Session["EmployeeId"]);
                    param1.Add("@p_CmpId", CmpId);
                    var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param1).ToList();
                    ViewBag.GetBranchName = BranchName;

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@query", "Select EmployeeId as Id, Concat (EmployeeName,'-',EmployeeNo) As Name  from Mas_Employee where Deactivate=0  and Employeeleft=0 and ContractorID=1 and EmployeeBranchId='" + InOutBranchId + "' Order By EmployeeName");
                    var GetEmployeeList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param2).ToList();
                    ViewBag.GetEmployeeList = GetEmployeeList;

                    DynamicParameters param3 = new DynamicParameters();
                    param3.Add("@query", "Select DesignationId as Id, DesignationName as Name   from Mas_Designation where Deactivate=0");
                    ViewBag.GetDesignationList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param3).ToList();


                    DynamicParameters param4 = new DynamicParameters();
                    param4.Add("@query", "Select LineId as Id, LineName as Name from Mas_LineMaster where Deactivate=0");
                    ViewBag.GetLineList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param4).ToList();

                    DynamicParameters param5 = new DynamicParameters();
                    param5.Add("@query", "Select KPISubCategoryId as Id, KPISubCategoryFName as Name from KPI_SubCategory where Deactivate=0");
                    ViewBag.GetCategoryList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param5).ToList();

                }


                return View("ESS_UpdateLineManpowerDesignationMapping");
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
        public ActionResult SaveUpdate(List<AttendanceRegu_Atten_InOut> updates)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 773;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                for (int i = 0; i < updates.Count; i++)
                {
                    param.Add("@p_process", "Update");
                    param.Add("@p_InOutId", updates[i].InOutID);
                    param.Add("@p_InOut_EmpLineId", updates[i].InOut_EmpLineId);
                    param.Add("@p_InOut_EmpDesignationId", updates[i].InOut_EmpDesignationId);
                    param.Add("@p_InOut_EmpCategoryId", updates[i].InOut_EmpCategoryId);                  
                    param.Add("@p_CreatedupdateBy", Session["EmployeeName"]);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_SUD_UpdateLineDesignationCategory", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                }
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Icon = "error" }, JsonRequestBehavior.AllowGet);

            }
        }
        #endregion

        #region GetInOutData
        public ActionResult GetInOutData(int? CmpId, int? InOutBranchId, string InOutEmployeeId, DateTime? InOutFromDate, DateTime? InOutToDate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                ViewBag.GetBranchName = "";
                ViewBag.GetEmployeeList = "";
                ViewBag.List = "";
                if (CmpId != null)
                {
                    param.Add("@p_CmpId", CmpId);
                    param.Add("@p_BranchId", InOutBranchId);
                    param.Add("@p_EmployeerID", string.IsNullOrEmpty(InOutEmployeeId) ? null : InOutEmployeeId);
                    param.Add("@p_FromDate", InOutFromDate);
                    param.Add("@p_ToDate", InOutToDate);
                    var List = DapperORM.DynamicList("sp_UpdateLineManpowerDesignation", param);
                    ViewBag.List = List;


                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_employeeid", Session["EmployeeId"]);
                    param1.Add("@p_CmpId", CmpId);
                    var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param1).ToList();
                    ViewBag.GetBranchName = BranchName;

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@query", "Select EmployeeId as Id, Concat (EmployeeName,'-',EmployeeNo) As Name  from Mas_Employee where Deactivate=0  and Employeeleft=0 and ContractorID=1 and EmployeeBranchId='" + InOutBranchId + "' Order By EmployeeName");
                    var GetEmployeeList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param2).ToList();
                    ViewBag.GetEmployeeList = GetEmployeeList;

                    DynamicParameters param3 = new DynamicParameters();
                    param3.Add("@query", "Select DesignationId as Id, DesignationName as Name   from Mas_Designation where Deactivate=0");
                    ViewBag.GetDesignationList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param3).ToList();


                    DynamicParameters param4 = new DynamicParameters();
                    param4.Add("@query", "Select LineId as Id, LineName as Name from Mas_LineMaster where Deactivate=0");
                    ViewBag.GetLineList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param4).ToList();

                    DynamicParameters param5 = new DynamicParameters();
                    param5.Add("@query", "Select KPISubCategoryId as Id, KPISubCategoryFName as Name from KPI_SubCategory where Deactivate=0");
                    ViewBag.GetCategoryList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param5).ToList();

                }


                return View("ESS_UpdateLineManpowerDesignationMapping");
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