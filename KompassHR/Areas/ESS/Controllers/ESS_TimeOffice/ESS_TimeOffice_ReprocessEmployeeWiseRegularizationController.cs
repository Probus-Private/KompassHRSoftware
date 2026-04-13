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
    public class ESS_TimeOffice_ReprocessEmployeeWiseRegularizationController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_TimeOffice_ReprocessEmployeeWiseRegularization
        #region ESS_TimeOffice_ReprocessEmployeeWiseRegularization
        public ActionResult ESS_TimeOffice_ReprocessEmployeeWiseRegularization()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 401;
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
                //var GetCurrentMonth = DateTime.Now.Date.ToString("MM");
                //var  GetCurrentYear  = DateTime.Now.Date.ToString("yyyy");
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and Mas_Employee.EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and Atten_InOut.InOutBranchId =" + BranchId + " and month( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("MM") + "' and year( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("yyyy") + "' and EmployeeId<>1) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid=" + BranchId + " and( month(mas_employee.JoiningDate)<='" + DateTime.Now.Date.ToString("MM") + "' and year (mas_employee.JoiningDate)<='" + DateTime.Now.Date.ToString("yyyy") + "') and (month(LeavingDate)='" + DateTime.Now.Date.ToString("MM") + "' and year(mas_employee.LeavingDate)='" + DateTime.Now.Date.ToString("yyyy") + "' or mas_employee.LeavingDate is null) order by Name");
                ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                //DynamicParameters paramEmpName = new DynamicParameters();
                //paramEmpName.Add("@query", "select  EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpId + " and EmployeeBranchId=" + BranchId + " and Mas_Employee.EmployeeLeft=0 Order By EmployeeName");
                //ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();


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
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select  EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId= " + Branch[0].Id + " and Mas_Employee.EmployeeLeft=0");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();

                return Json(new { Branch = Branch, EmployeeName = EmployeeName }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetEmployee
        [HttpGet]
        public ActionResult GetEmployee(int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select  EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId= " + BranchId + " and Mas_Employee.EmployeeLeft=0");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
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
        public ActionResult SaveUpdate(/*int GetBranchId, int GetEmployeeId,DateTime GetFromDate,DateTime GetToDate*/EmployeeWiseAttendance ReprocessEmployeeAttendance)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 401;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                // var AttenLockCount = DapperORM.DynamicQuerySingle("Select Count(AtdLockIDBranchId) as LockCount from Atten_Lock where Deactivate=0  and Month(AtdLockIDMonth)='" + ReprocessEmployeeAttendance.FromDate.ToString("MM") + "'  and Year(AtdLockIDMonth) ='" + ReprocessEmployeeAttendance.FromDate.ToString("yyyy") + "'  and AtdLockIDBranchId=" + ReprocessEmployeeAttendance.BranchId + " and AtdLock=1");

                DynamicParameters paramLock = new DynamicParameters();
                paramLock.Add("@p_FromDate", ReprocessEmployeeAttendance.FromDate.ToString("yyyy-MM-dd"));
                paramLock.Add("@p_ToDate", ReprocessEmployeeAttendance.ToDate.ToString("yyyy-MM-dd"));
                paramLock.Add("@p_AtdLockIDBranchId", ReprocessEmployeeAttendance.BranchId);
                var AttenLockCount = DapperORM.ReturnList<dynamic>("sp_Check_AttenLock", paramLock).FirstOrDefault();
                if (AttenLockCount.LockCount != 0)
                {
                    TempData["Message"] = "Record can''t be saved because the month/year is already locked.";
                    TempData["Icon"] = "error";
                    Session["ReprocessFromDate"] = ReprocessEmployeeAttendance.FromDate.ToString("yyyy-MM-dd");
                    Session["ReprocessToDate"] = ReprocessEmployeeAttendance.ToDate.ToString("yyyy-MM-dd");
                    return RedirectToAction("ESS_TimeOffice_ReprocessEmployeeWiseRegularization", "ESS_TimeOffice_ReprocessEmployeeWiseRegularization");
                }
                param.Add("@p_Origin", "ReprocessEmployeeWiseAttendance");
                param.Add("@p_BranchId", ReprocessEmployeeAttendance.BranchId);
                param.Add("@p_EmployeeId", ReprocessEmployeeAttendance.EmployeeID);
                param.Add("@p_FromDate", ReprocessEmployeeAttendance.FromDate.ToString("yyyy-MM-dd"));
                param.Add("@p_ToDate", ReprocessEmployeeAttendance.ToDate.ToString("yyyy-MM-dd"));
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("Sp_Attendance_Regularization_Date_Reprocess", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                // return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                Session["ReprocessFromDate"] = ReprocessEmployeeAttendance.FromDate.ToString("yyyy-MM-dd");
                Session["ReprocessToDate"] = ReprocessEmployeeAttendance.ToDate.ToString("yyyy-MM-dd");
                return RedirectToAction("ESS_TimeOffice_ReprocessEmployeeWiseRegularization", "ESS_TimeOffice_ReprocessEmployeeWiseRegularization");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                var Message = ex;
                var Icon = "error";
                return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


    }
}