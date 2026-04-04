using Dapper;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_EmployeeConfirmationController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: Module/Module_Employee_EmployeeConfirmation
        #region Module_Employee_EmployeeConfirmation
        public ActionResult Module_Employee_EmployeeConfirmation(EmployeeConfirmation OBJPayrollApproval)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 487;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.ComapnyName = GetComapnyName;
                ViewBag.BranchName = "";

                ViewBag.GetEmployeeList = "";
                if (OBJPayrollApproval.Month != null)
                {
                    DynamicParameters param7 = new DynamicParameters();
                    param7.Add("@p_employeeid", Session["EmployeeId"]);
                    param7.Add("@p_CmpId", OBJPayrollApproval.CmpId);
                    ViewBag.BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param7).ToList();

                    DateTime payrollMonth;
                    if (DateTime.TryParse(OBJPayrollApproval.Month.ToString(), out payrollMonth))
                    {
                        DynamicParameters paramMNG = new DynamicParameters();
                        paramMNG.Add("@query", @"SELECT Mas_Employee.EmployeeId,
                        Mas_Employee.EmployeeNo, Mas_Employee.EmployeeName, Mas_Branch.BranchName, Mas_Department.DepartmentName,
                        Mas_Designation.DesignationName, REPLACE(CONVERT(nvarchar(12), Mas_Employee.JoiningDate, 106), ' ', '/') AS JoiningDate,
                        REPLACE(CONVERT(nvarchar(12), Mas_Employee.ConfirmationDate, 106), ' ', '/') AS ConfirmationDate
                        FROM Mas_Employee
                        INNER JOIN Mas_Branch ON Mas_Employee.EmployeeBranchId = Mas_Branch.BranchId
                        INNER JOIN Mas_Department ON Mas_Employee.EmployeeDepartmentID = Mas_Department.DepartmentId
                        INNER JOIN Mas_Designation ON Mas_Employee.EmployeeDesignationID = Mas_Designation.DesignationId
                        WHERE IsConfirmation = 0
                        AND Mas_Employee.Deactivate = 0
                        AND EmployeeLeft = 0
                        AND EmployeeId <> 1
                        AND ContractorID = 1
                        AND EmployeeBranchId  = " + OBJPayrollApproval.BranchId + @" 
                        AND MONTH(ConfirmationDate) = " + payrollMonth.Month + @" 
                        AND YEAR(ConfirmationDate) = " + payrollMonth.Year + @" 
                    ORDER BY EmployeeNo");
                        var data1 = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramMNG).ToList();
                        ViewBag.GetEmployeeList = data1;
                    }
                    else
                    {
                        // Handle parsing error for OBJPayrollApproval.Month
                        Session["GetErrorMessage"] = "Invalid Month format provided.";
                        return RedirectToAction("ErrorPage", "Login");
                    }
                }

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
        public ActionResult SaveUpdate(List<EmployeeConfirmation> BulkConfirmation, DateTime Month)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }


                for (int i = 0; i < BulkConfirmation.Count; i++)
                {
                    param.Add("@p_process", "Save");
                    param.Add("@p_EmployeeId", BulkConfirmation[i].EmployeeId);
                    param.Add("@p_Date", Month);

                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_SUD_Mas_EmployeeConfirmation", param);
                    var message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    TempData["Message"] = message;
                    TempData["Icon"] = Icon;
                    if (Icon == "error")
                    {
                        //TempData["Message"] = message;
                        // TempData["Icon"] = Icon;
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return Json(new { Message = Session["GetErrorMessage"], Icon ="error" }, JsonRequestBehavior.AllowGet);
               // return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion


    }
}