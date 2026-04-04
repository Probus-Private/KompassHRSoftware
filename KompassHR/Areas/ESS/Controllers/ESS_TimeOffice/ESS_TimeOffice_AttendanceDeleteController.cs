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
    public class ESS_TimeOffice_AttendanceDeleteController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_TimeOffice_AttendanceDelete
        #region ESS_TimeOffice_AttendanceDelete
        public ActionResult ESS_TimeOffice_AttendanceDelete()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 411;
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

                var CmpId = GetComapnyName[0].Id;
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var GetBranchId = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                ViewBag.GetBusinessUnit = GetBranchId;
                var BranchId = GetBranchId[0].Id;

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and Mas_Employee.EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and Atten_InOut.InOutBranchId =" + BranchId + " and month( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("MM") + "' and year( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("yyyy") + "' and EmployeeId<>1) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid=" + BranchId + " and( month(mas_employee.JoiningDate)<='" + DateTime.Now.Date.ToString("MM") + "' and year (mas_employee.JoiningDate)<='" + DateTime.Now.Date.ToString("yyyy") + "') and (month(LeavingDate)='" + DateTime.Now.Date.ToString("MM") + "' and year(mas_employee.LeavingDate)='" + DateTime.Now.Date.ToString("yyyy") + "' or mas_employee.LeavingDate is null) order by Name");
                ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                ViewBag.EmployeeLog = "";
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
        [HttpPost]
        public ActionResult SaveUpdate(EmployeeWiseAttendance EmployeeWiseAttendance)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 411;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                //var AttenLockCount = DapperORM.DynamicQuerySingle("Select Count(AtdLockIDBranchId) as LockCount from Atten_Lock where Deactivate=0  and Month(AtdLockIDMonth)='" + EmployeeWiseAttendance.FromDate.ToString("MM") + "'  and Year(AtdLockIDMonth) ='" + EmployeeWiseAttendance.FromDate.ToString("yyyy") + "'  and AtdLockIDBranchId=" + EmployeeWiseAttendance.BranchId + " and AtdLock=1");
                
                DynamicParameters paramLock = new DynamicParameters();
                paramLock.Add("@p_FromDate", EmployeeWiseAttendance.FromDate.ToString("yyyy-MM-dd"));
                paramLock.Add("@p_ToDate", EmployeeWiseAttendance.ToDate.ToString("yyyy-MM-dd"));
                paramLock.Add("@p_AtdLockIDBranchId", EmployeeWiseAttendance.BranchId);
                var AttenLockCount = DapperORM.ReturnList<dynamic>("sp_Check_AttenLock", paramLock).FirstOrDefault();
                if (AttenLockCount.LockCount != 0)
                {
                    TempData["Message"] = "Record can''t be saved because the month/year is already locked.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("ESS_TimeOffice_AttendanceDelete", "ESS_TimeOffice_AttendanceDelete");
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_EmployeeId", EmployeeWiseAttendance.EmployeeID);
                param.Add("@p_BranchId", EmployeeWiseAttendance.BranchId);
                param.Add("@p_FromDate", EmployeeWiseAttendance.FromDate.ToString("yyyy-MM-dd"));
                param.Add("@p_ToDate", EmployeeWiseAttendance.ToDate.ToString("yyyy-MM-dd"));
                param.Add("@p_OTApproved", EmployeeWiseAttendance.OTApproved);
                param.Add("@p_PunchAdjustment", EmployeeWiseAttendance.PunchAdjustment);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_AttendanceDelete", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_TimeOffice_AttendanceDelete", "ESS_TimeOffice_AttendanceDelete");

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