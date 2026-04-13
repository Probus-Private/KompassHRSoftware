using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.App.Controllers.App_Attendance
{
    public class App_Attendance_CalendarController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        // GET: App/App_Attendance_Calendar
        public ActionResult App_Attendance_Calendar(string Date)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // Handle null or empty Date
                DateTime SetDate = DateTime.Today;
                if (!string.IsNullOrEmpty(Date))
                {
                    DateTime.TryParse(Date, out SetDate);
                }
                ViewBag.GetDate = SetDate.ToString("yyyy-MM");
                DynamicParameters param1 = new DynamicParameters();
                if (Date != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_Date", SetDate);
                    param.Add("@p_BranchId", Session["BranchId"]);
                    param.Add("@p_EmployeeId", Session["EmployeeId"]);
                    var EmployeeWiseReport = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Atten_EmployeeWise", param).ToList();
                    ViewBag.EmployeeWiseReport = EmployeeWiseReport;

                    DynamicParameters param4 = new DynamicParameters();
                    param4.Add("@p_Date", SetDate);
                    param4.Add("@p_BranchId", Session["BranchId"]);
                    param4.Add("@p_EmployeeId", Session["EmployeeId"]);
                    var EmployeeWiseSummeryReport = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Atten_EmployeeWiseSummery", param4).ToList();
                    ViewBag.EmployeeWiseSummeryReport = EmployeeWiseSummeryReport;

                    DynamicParameters EmployeeDetails = new DynamicParameters();
                    EmployeeDetails.Add("@query", @"Select Mas_Employee.EmployeeName, DepartmentName, DesignationName, EmployeeNo, Mas_CompanyProfile.CompanyName,Mas_Branch.BranchName  from Mas_Employee
                                                    join Mas_Department on Mas_Department.DepartmentId = Mas_Employee.EmployeeDepartmentID
                                                    join Mas_Designation on Mas_Designation.DesignationId = Mas_Employee.EmployeeDesignationID
                                                    join Mas_CompanyProfile on Mas_CompanyProfile.CompanyId = Mas_Employee.CmpID
                                                    join Mas_Branch on Mas_Branch.BranchId = Mas_Employee.EmployeeBranchId
                                                    Where EmployeeId =" + Session["EmployeeId"] + " and Mas_Employee.Deactivate = 0");
                    var datas = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", EmployeeDetails).ToList();
                    ViewBag.GetEmployeeDetails = datas;
                }
                else
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_Date", DateTime.Now);
                    param.Add("@p_BranchId", Session["BranchId"]);
                    param.Add("@p_EmployeeId", Session["EmployeeId"]);
                    var EmployeeWiseReport = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Atten_EmployeeWise", param).ToList();
                    ViewBag.EmployeeWiseReport = EmployeeWiseReport;

                    DynamicParameters param4 = new DynamicParameters();
                    param4.Add("@p_Date", DateTime.Now);
                    param4.Add("@p_BranchId", Session["BranchId"]);
                    param4.Add("@p_EmployeeId", Session["EmployeeId"]);
                    var EmployeeWiseSummeryReport = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Atten_EmployeeWiseSummery", param4).ToList();
                    ViewBag.EmployeeWiseSummeryReport = EmployeeWiseSummeryReport;

                    DynamicParameters EmployeeDetails = new DynamicParameters();
                    EmployeeDetails.Add("@query", @"Select Mas_Employee.EmployeeName, DepartmentName, DesignationName, EmployeeNo, Mas_CompanyProfile.CompanyName,Mas_Branch.BranchName  from Mas_Employee
                                                    join Mas_Department on Mas_Department.DepartmentId = Mas_Employee.EmployeeDepartmentID
                                                    join Mas_Designation on Mas_Designation.DesignationId = Mas_Employee.EmployeeDesignationID
                                                    join Mas_CompanyProfile on Mas_CompanyProfile.CompanyId = Mas_Employee.CmpID
                                                    join Mas_Branch on Mas_Branch.BranchId = Mas_Employee.EmployeeBranchId
                                                    Where EmployeeId =" + Session["EmployeeId"] + " and Mas_Employee.Deactivate = 0");
                    var datas = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", EmployeeDetails).ToList();
                    ViewBag.GetEmployeeDetails = datas;

                }
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
    }
}