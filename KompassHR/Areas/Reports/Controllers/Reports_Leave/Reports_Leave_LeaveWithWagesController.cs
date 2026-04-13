using Dapper;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_Leave
{
    public class Reports_Leave_LeaveWithWagesController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Reports/Reports_Leave_LeaveWithWages
        #region LeaveWithWages
        public ActionResult Reports_Leave_LeaveWithWages(LeaveWithWage Obj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 408;
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
                DynamicParameters param6 = new DynamicParameters();
                param6.Add("@p_employeeid", Session["EmployeeId"]);
                param6.Add("@p_CmpId", CmpId);
                var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param6).ToList();
                ViewBag.BranchName = BranchName;
                var BUID = BranchName[0].Id;

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and Mas_Employee.EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and Atten_InOut.InOutBranchId =" + BUID + " and year( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("yyyy") + "' and EmployeeId<>1) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid =" + BUID + "  and (year (mas_employee.JoiningDate)<='" + DateTime.Now.Date.ToString("yyyy") + "') and (year(mas_employee.LeavingDate)<='" + DateTime.Now.Date.ToString("yyyy") + "' or mas_employee.LeavingDate is null) order by Name");
                ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();

                param = new DynamicParameters();
                param.Add("@query", "select LeaveYearID as Id, cast(year(FromDate) as nvarchar(4))+'-'+cast(YEAR(ToDate) as nvarchar(4)) as Name from[dbo].[Leave_Year] where Deactivate = 0  and CmpId='" + CmpId + "' order by IsDefault desc,FromDate desc");
                var LeaveYearGet = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetLeaveYear = LeaveYearGet;

                if (Obj.EmployeeId != null)
                {
                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@p_employeeid", Session["EmployeeId"]);
                    param2.Add("@p_CmpId", Obj.CmpId);
                    ViewBag.BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param2).ToList();

                    DynamicParameters param9 = new DynamicParameters();
                    param9.Add("@query", "select LeaveYearID as Id, cast(year(FromDate) as nvarchar(4))+'-'+cast(YEAR(ToDate) as nvarchar(4)) as Name from[dbo].[Leave_Year] where Deactivate = 0   and CmpId='" + Obj.CmpId + "' order by IsDefault desc,FromDate desc");
                    var GetLeaveYearGet = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param9).ToList();
                    ViewBag.GetLeaveYear = GetLeaveYearGet;

                    DynamicParameters param3 = new DynamicParameters();
                    //  paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and Mas_Employee.EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and Atten_InOut.InOutBranchId =" + BUID + " and year( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("yyyy") + "' and EmployeeId<>1) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid =" + BUID + "  and (year (mas_employee.JoiningDate)<='" + DateTime.Now.Date.ToString("yyyy") + "') and (year(mas_employee.LeavingDate)<='" + DateTime.Now.Date.ToString("yyyy") + "' or mas_employee.LeavingDate is null) order by Name");
                    param3.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and Mas_Employee.EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and Atten_InOut.InOutBranchId =" + Obj.BranchId + " and year( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("yyyy") + "' and EmployeeId<>1) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid =" + Obj.BranchId + "  and (year (mas_employee.JoiningDate)<='" + DateTime.Now.Date.ToString("yyyy") + "') and (year(mas_employee.LeavingDate)<='" + DateTime.Now.Date.ToString("yyyy") + "' or mas_employee.LeavingDate is null) order by Name");
                    // param3.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and Mas_Employee.EmployeeBranchId='" + Obj.BranchId + "' And employeeLeft=0 order by Name");
                    ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param3).ToList();

                    //DynamicParameters param3 = new DynamicParameters();
                    //param3.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and month( InOutMonthYear)=" + GetMonth + " and Year(InOutMonthYear)=" + GetMonthYear + " and InOutBranchId=" + Obj.BranchId + " ) order by Name");
                    //var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param3).ToList();
                    //ViewBag.EmployeeName = EmployeeName;

                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_Year", Obj.Year);
                    param.Add("@p_EmployeeId", Obj.EmployeeId);
                    var LeaveWithWagesRegister = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Leave_LeaveWithWagesRegister", param).ToList();
                    ViewBag.GetLeaveWithWagesRegister = LeaveWithWagesRegister;

                    DynamicParameters EmployeeDetails = new DynamicParameters();
                    EmployeeDetails.Add("@query", @"Select Mas_Employee.EmployeeName, DepartmentName, DesignationName, EmployeeNo,Mas_Branch.BranchName,Mas_CompanyProfile.CompanyName,Contractor_Master.ContractorName,REPLACE(convert(nvarchar(12),Mas_Employee.JoiningDate,106),' ','/') as JoiningDate, REPLACE(convert(nvarchar(12),Mas_Employee.LeavingDate,106),' ','/') as LeavingDate  ,Mas_Employee_Address.PermanentPostelAddress as Address from Mas_Employee
                                                    left join Mas_Department on Mas_Department.DepartmentId = Mas_Employee.EmployeeDepartmentID
                                                    left join Mas_Designation on Mas_Designation.DesignationId = Mas_Employee.EmployeeDesignationID
                                                    left join Mas_CompanyProfile on Mas_CompanyProfile.CompanyId = Mas_Employee.CmpID
                                                    left join Mas_Branch on Mas_Branch.BranchId = Mas_Employee.EmployeeBranchId
                                                    left join Contractor_Master on Contractor_Master.ContractorId = Mas_Employee.ContractorID
                                                    left join Mas_Employee_Address on Mas_Employee_Address.AddressEmployeeId = Mas_Employee.EmployeeId
                                                    Where Mas_Employee.EmployeeId =" + Obj.EmployeeId + " and Mas_Employee.Deactivate = 0");
                    var datas = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", EmployeeDetails).ToList();
                    ViewBag.GetEmployeeDetails = datas;



                }
                else
                {
                    ViewBag.GetLeaveWithWagesRegister = "";
                    ViewBag.GetEmployeeDetails = "";
                }
                return View(Obj);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region Monthly Attendance DropDown
        [HttpGet]
        public ActionResult GetMonthlyBusinessUnit(int CmpId)
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
                var GetBranchId = Branch[0].Id;

                DynamicParameters paramEmpName = new DynamicParameters();
                //paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and year( Atten_InOut.InOutDate )='" + GetMonthYear + "' and EmployeeId<>1 and InOutBranchId=" + GetBranchId + " ) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid=" + GetBranchId + " and (year (mas_employee.JoiningDate)<='" + GetMonthYear+ "')  and (year(mas_employee.LeavingDate)='" + GetMonthYear+ "' or mas_employee.LeavingDate is null) order by Name");
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and Mas_Employee.EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and Atten_InOut.InOutBranchId =" + GetBranchId + " and year( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("yyyy") + "' and EmployeeId<>1) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid =" + GetBranchId + "  and (year (mas_employee.JoiningDate)<='" + DateTime.Now.Date.ToString("yyyy") + "') and (year(mas_employee.LeavingDate)<='" + DateTime.Now.Date.ToString("yyyy") + "' or mas_employee.LeavingDate is null) order by Name");
                // paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId=" + GetBranchId + " and EmployeeLeft=0 and EmployeeId<>1 order by Name");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();

                param = new DynamicParameters();
                param.Add("@query", "select LeaveYearID as Id, cast(year(FromDate) as nvarchar(4))+'-'+cast(YEAR(ToDate) as nvarchar(4)) as Name from[dbo].[Leave_Year] where Deactivate = 0  and IsActivate=1  and CmpId='" + CmpId + "' order by IsDefault desc,FromDate desc");
                var LeaveYearGet = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                return Json(new { EmployeeName = EmployeeName, Branch = Branch, LeaveYearGet = LeaveYearGet }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        [HttpGet]
        public ActionResult GetMonthlyEmployeeName(int CmpId, int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and Mas_Employee.EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and Atten_InOut.InOutBranchId =" + BranchId + " and year( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("yyyy") + "' and EmployeeId<>1) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid =" + BranchId + "  and (year (mas_employee.JoiningDate)<='" + DateTime.Now.Date.ToString("yyyy") + "') and (year(mas_employee.LeavingDate)<='" + DateTime.Now.Date.ToString("yyyy") + "' or mas_employee.LeavingDate is null) order by Name");
                //param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId=" + BranchId + " and EmployeeLeft=0 and EmployeeId<>1 order by Name");
                //param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0  and year( Atten_InOut.InOutDate )='" + GetMonth + "' and EmployeeId<>1 and InOutBranchId=" + BranchId + " ) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid=" + BranchId + "  and (year (mas_employee.JoiningDate)<='" + GetMonth + "')  and ( year(mas_employee.LeavingDate)='" + GetMonth + "' or mas_employee.LeavingDate is null) order by Name");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        //[HttpGet]
        //public ActionResult GetMonthlyEmployeeNameDateWise(int BranchId, int GetMonth)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { area = "" });
        //        }
        //        DynamicParameters param = new DynamicParameters();
        //        param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and Mas_Employee.EmployeeBranchId='" + BranchId + "' And employeeLeft=0 and EmployeeId<>1 order by Name");
        //        var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
        //        return Json(data, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
        //}
        #endregion


    }
}