using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_TimeOffice
{
    public class ESS_TimeOffice_AttendanceLockController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
         // GET: ESS/ESS_TimeOffice_AttendanceLock
        #region Main View 
        public ActionResult ESS_TimeOffice_AttendanceLock(AttendanceLock AttendanceLock)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 389;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.Employees = "";
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.CompanyName = GetComapnyName;
                if (AttendanceLock.MonthYear != null)
                {
                    var GetMonth = AttendanceLock.MonthYear.ToString("MM");
                    var GetYear = AttendanceLock.MonthYear.ToString("yyyy");
                    DynamicParameters paramBranch = new DynamicParameters();
                    param.Add("@query", @"SELECT Mas_Branch.BranchId AS Id, Mas_Branch.BranchName AS Name,Mas_CompanyProfile.CompanyName as CompanyName,Mas_Branch.CmpId  ,case when  Atten_Lock.AtdLock=1 then 'checked' else '' end as  CheckBox FROM  Mas_Branch LEFT JOIN Mas_CompanyProfile    ON Mas_Branch.CmpId = Mas_CompanyProfile.CompanyId LEFT JOIN Atten_Lock    ON Mas_Branch.BranchId = Atten_Lock.AtdLockIDBranchId  AND MONTH(Atten_Lock.AtdLockIDMonth) = '" + GetMonth + "' and year (Atten_Lock.AtdLockIDMonth)  = '" + GetYear + "' WHERE  Mas_Branch.Deactivate = 0 order by Mas_Branch.BranchId"); /**/
                    ViewBag.Employees = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).ToList();
       
                }
              
                return View(AttendanceLock);
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
                var BranchId = Branch[0].Id;

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpId + " and EmployeeBranchId= " + BranchId + " and Mas_Employee.EmployeeLeft=0");
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

        #region GetUnit
        [HttpGet]
        public ActionResult GetUnit(int? UnitBranchId, int? CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (UnitBranchId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId= " + UnitBranchId + "and Mas_Employee.EmployeeLeft=0 Order By EmployeeName");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and Mas_Employee.EmployeeBranchId in (select BranchID as Id from UserBranchMapping where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and UserBranchMapping.CmpID = " + CmpId + " and UserBranchMapping.IsActive = 1) and Mas_Employee.EmployeeLeft=0 Order By EmployeeName");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
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
        public ActionResult GetEmployees(AttendanceLock AttendanceLock)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 389;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "Select EmployeeId,EmployeeName from Mas_Employee where Deactivate=0 and EmployeeLeft=0 and CmpID=" + AttendanceLock.CmpId + " and EmployeeBranchId=" + AttendanceLock.BranchId + "");
                ViewBag.Employees = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).ToList();

                return RedirectToAction("ESS_TimeOffice_AttendanceLock", "ESS_TimeOffice_AttendanceLock");
                //return Json(new { data = GetEmployees }, JsonRequestBehavior.AllowGet);
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
        public ActionResult SaveUpdate(List<AttendanceLock> AttendanceLock, DateTime MonthYear)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 389;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                StringBuilder strBuilder = new StringBuilder();
                string abc = "";
                var GetMonthYear = MonthYear.ToString("MM");
                var GetYear = MonthYear.ToString("yyyy");
                if (AttendanceLock == null)
                {
                    string DeleteAttenLock1 = "Update Atten_Lock set AtdLock=0,ModifiedDate=GETDATE(),ModifiedBy='" + Session["EmployeeName"] + "',MachineName='" + Dns.GetHostName().ToString() + "'  Where" +
                                         " month(AtdLockIDMonth) = '" + GetMonthYear + "'" +
                                         " and year(AtdLockIDMonth) = '" + GetYear + "'";
                    strBuilder.Append(DeleteAttenLock1);

                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {
                        TempData["Message"] = "Record Save successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                    if (abc != "")
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                    return RedirectToAction("ESS_TimeOffice_AttendanceLock", "ESS_TimeOffice_AttendanceLock");
                }
                else
                {
                    string DeleteAttenLock = " Delete from Atten_Lock  Where" +
                                         " month(AtdLockIDMonth) = '" + GetMonthYear + "'" +
                                         " and year(AtdLockIDMonth) = '" + GetYear + "' and Deactivate=0";
                    strBuilder.Append(DeleteAttenLock);

                    for (int i = 0; i < AttendanceLock.Count; i++)
                    {
                        string SaveAttendanceLock = "";
                        var getCycleSql = DapperORM.DynamicQueryList("SELECT AtdType, FromDay, ToDay FROM Atten_Cycle WHERE CmpId = "+ AttendanceLock[i].CmpId + " AND AtdLockIDBranchId = "+ AttendanceLock[i].BranchId + "").FirstOrDefault();

                        if (getCycleSql != null && getCycleSql.AtdType == "Monthly")
                        {
                            // Get 1st and last date of the month
                            DateTime fromDate = new DateTime(MonthYear.Year, MonthYear.Month, 1);
                            DateTime toDate = fromDate.AddMonths(1).AddDays(-1);

                            SaveAttendanceLock = $@"
                            INSERT INTO Atten_Lock 
                            (Deactivate, CreatedBy, CreatedDate, CmpId, AtdLockIDBranchId, AtdLockIDMonth, AtdLock, FromDate, ToDate)
                            VALUES 
                            ('0', '{Session["EmployeeName"]}', GETDATE(), '{AttendanceLock[i].CmpId}', '{AttendanceLock[i].BranchId}', 
                            '{MonthYear:yyyy-MM-dd}', '1', '{fromDate:yyyy-MM-dd}', '{toDate:yyyy-MM-dd}')";

                        }

                        else if (getCycleSql != null && getCycleSql.AtdType == "Interval")
                        {
                            // Parse FromDay and ToDay safely
                            int fDay = 1;
                            int.TryParse(getCycleSql.FromDay, out fDay);

                            int tDay = 1;
                            int.TryParse(getCycleSql.ToDay, out tDay);

                            // Previous month
                            DateTime previousMonth = MonthYear.AddMonths(-1);
                            int prevMonthDays = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
                            int safeFromDay = Math.Min(fDay, prevMonthDays);
                            DateTime fromDate = new DateTime(previousMonth.Year, previousMonth.Month, safeFromDay);

                            // Current month
                            int currentMonthDays = DateTime.DaysInMonth(MonthYear.Year, MonthYear.Month);
                            int safeToDay = Math.Min(tDay, currentMonthDays);
                            DateTime toDate = new DateTime(MonthYear.Year, MonthYear.Month, safeToDay);

                            // SQL (same as before)
                            SaveAttendanceLock = $@"
                            INSERT INTO Atten_Lock 
                            (Deactivate, CreatedBy, CreatedDate, CmpId, AtdLockIDBranchId, AtdLockIDMonth, AtdLock, FromDate, ToDate)
                            VALUES 
                            ('0', '{Session["EmployeeName"]}', GETDATE(), '{AttendanceLock[i].CmpId}', '{AttendanceLock[i].BranchId}', 
                            '{MonthYear:yyyy-MM-dd}', '1', '{fromDate:yyyy-MM-dd}', '{toDate:yyyy-MM-dd}')";
                        }
                        strBuilder.Append(SaveAttendanceLock);
                    }

                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {
                        TempData["Message"] = "Record Save successfully";
                        TempData["Icon"] = "success";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                    if (abc != "")
                    {
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                    return RedirectToAction("ESS_TimeOffice_AttendanceLock", "ESS_TimeOffice_AttendanceLock");
                }

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