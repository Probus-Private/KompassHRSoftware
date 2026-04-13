using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_Attendance
{
    public class Reports_Attendance_MyAttendanceController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Reports/Reports_Attendance_MyAttendance
        public ActionResult Reports_Attendance_MyAttendance(MonthWiseFilter Obj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 377;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param1 = new DynamicParameters();
                ViewBag.AddUpdateTitle = "Add";
                if (Obj.Month != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_Date", Obj.Month);
                    param.Add("@p_BranchId", Session["BranchID"]);
                    param.Add("@p_EmployeeId", Session["EmployeeId"]);
                    var EmployeeWiseReport = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Atten_EmployeeWise", param).ToList();
                    ViewBag.EmployeeWiseReport = EmployeeWiseReport;

                    DynamicParameters param4 = new DynamicParameters();
                    param4.Add("@p_Date", Obj.Month);
                    param4.Add("@p_BranchId", Session["BranchID"]);
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
                    param.Add("@p_BranchId", Session["BranchID"]);
                    param.Add("@p_EmployeeId", Session["EmployeeId"]);
                    var EmployeeWiseReport = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Atten_EmployeeWise", param).ToList();
                    ViewBag.EmployeeWiseReport = EmployeeWiseReport;

                    DynamicParameters param4 = new DynamicParameters();
                    param4.Add("@p_Date", DateTime.Now);
                    param4.Add("@p_BranchId", Session["BranchID"]);
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
                return View(Obj);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
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

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpId + " and Mas_Employee.EmployeeLeft=0");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                //ViewBag.EmployeeName = EmployeeName;

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                return Json(new { EmployeeName = EmployeeName, Branch = Branch }, JsonRequestBehavior.AllowGet);
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
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpId + " and EmployeeBranchId= " + BranchId + "and Mas_Employee.EmployeeLeft=0");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                //DynamicParameters param = new DynamicParameters();
                //param.Add("@query", @"select EmployeeId as Id ,EmployeeName as Name from Mas_Employee  where Deactivate=0 and EmployeeBranchId=" + BranchId + "");
                //var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
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