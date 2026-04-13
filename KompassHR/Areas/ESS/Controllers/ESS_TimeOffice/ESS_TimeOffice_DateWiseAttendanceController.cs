using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_TimeOffice
{
    public class ESS_TimeOffice_DateWiseAttendanceController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_TimeOffice_DateWiseAttendance
        #region Main View
        public ActionResult ESS_TimeOffice_DateWiseAttendance()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 364;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var GetMaxDays = DapperORM.DynamicQueryList(@"Select Atten_Regu_MaxDay from Tool_CommonTable").FirstOrDefault();
                TempData["Atten_Regu_MaxDay"] = GetMaxDays?.Atten_Regu_MaxDay;
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.CompanyName = GetComapnyName;
                var CmpId = GetComapnyName[0].Id;

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                ViewBag.BranchNameList = BranchName;
                var BranchId = BranchName[0].Id;

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpId + " and EmployeeBranchId= " + BranchId + "and Mas_Employee.EmployeeLeft=0");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();
                ViewBag.EmployeeName = EmployeeName;
                
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region SaveUpdate
        public ActionResult SaveUpdate(DateWiseAttendance OBJ)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 364;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                //var AttenLockCount = DapperORM.DynamicQuerySingle("Select Count(AtdLockIDBranchId) as LockCount from Atten_Lock where Deactivate=0  and Month(AtdLockIDMonth)='" + OBJ.FromDate.ToString("MM") + "'  and Year(AtdLockIDMonth) ='" + OBJ.FromDate.ToString("yyyy") + "'  and AtdLockIDBranchId=" + OBJ.BranchId + " and AtdLock=1");
                //if (AttenLockCount.LockCount != 0)
                //{
                //    TempData["Message"] = "Record can''t be saved because the month/year is already locked.";
                //    TempData["Icon"] = "error";
                //Session["ReprocessFromDate"] = OBJ.FromDate.ToString("yyyy-MM-dd");
                //Session["ReprocessToDate"] = OBJ.ToDate.ToString("yyyy-MM-dd");
                //    return RedirectToAction("ESS_TimeOffice_DateWiseAttendance", "ESS_TimeOffice_DateWiseAttendance");
                //}
                param.Add("@P_FromDate", OBJ.FromDate.ToString("yyyy-MM-dd"));
                param.Add("@P_ToDate", OBJ.ToDate.ToString("yyyy-MM-dd"));
                param.Add("@P_BranchID", OBJ.BranchId);
                param.Add("@P_Flag", OBJ.Process);
                param.Add("@P_Origin", "DateWise");
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("Sp_Attendance_Regularization_Date", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                //TempData["Message"] = "Record Save Sucessfully";
                //TempData["Icon"] = "success";
                Session["ReprocessFromDate"] = OBJ.FromDate.ToString("yyyy-MM-dd");
                Session["ReprocessToDate"] = OBJ.ToDate.ToString("yyyy-MM-dd");
                return RedirectToAction("ESS_TimeOffice_DateWiseAttendance", "ESS_TimeOffice_DateWiseAttendance");
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

        #region GetEmployeeName
        [HttpGet]
        public ActionResult GetEmployeeName(int CmpId, int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpId + " and EmployeeBranchId= " + BranchId + "and Mas_Employee.EmployeeLeft=0");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();

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