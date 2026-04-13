using Dapper;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Models;
using KompassHR.Areas.Setting.Models.Setting_FacilityAndSafety;
using KompassHR.Areas.Setting.Models.Setting_Prime;

namespace KompassHR.Areas.Setting.Controllers.Setting_AccountAndFinance
{
    public class Setting_AccountAndFinance_AccountsTeamController : Controller
    {


        // GET: Setting/Setting_AccountAndFinance_AccountsTeam
        public ActionResult Setting_AccountAndFinance_AccountsTeam()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {

                    var AccountsTeamData = DapperORM.DynamicQuerySingle(@"select concat(employeeseries,employeeno) employeeno,Salutation,EmployeeName,DepartmentName,CompanyMailID,CmpID,Mas_Designation.DesignationName  from mas_department,Mas_Designation,Mas_Employee
                                                    where mas_department.departmentid=Mas_Employee.EmployeeDepartmentID and Mas_Designation.DesignationId=mas_employee.EmployeeDesignationID 
                                                    and mas_department.Deactivate=0 and  maS_employee.Deactivate=0 and DepartmentName like '%Account%'
                                                    order by mas_employee.employeename");

                    ViewBag.GetAccountList = AccountsTeamData;
                }
                //SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

                //var AccountsTeamData = DapperORM.DynamicQuerySingle(@"select concat(employeeseries,employeeno) employeeno,Salutation,EmployeeName,DepartmentName,Mas_Designation.DesignationName from mas_department,Mas_Designation,Mas_Employee
                //                                    where mas_department.departmentid=Mas_Employee.EmployeeDepartmentID and Mas_Designation.DesignationId=mas_employee.EmployeeDesignationID 
                //                                    and mas_department.Deactivate=0 and  maS_employee.Deactivate=0 and DepartmentName like '%Account%'
                //                                    order by mas_employee.employeename");

              
                //ViewBag.GetAccountList = AccountsTeamData;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
    }
}