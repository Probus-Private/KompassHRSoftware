using Dapper;
using KompassHR.Areas.Module.Models.Module_Payroll;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Payroll
{
    public class Module_Payroll_PayrollHoldController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: Module/Module_Payroll_PayrollHold
        #region Module_Payroll_PayrollHold
        public ActionResult Module_Payroll_PayrollHold(string PayrollHoldId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 492;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Payroll_SalaryHold _SalaryHold = new Payroll_SalaryHold();
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                ViewBag.GetCompanyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetBranchName = "";
                ViewBag.GetEmployeeName = "";

                if (PayrollHoldId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_PayrollHoldId_Encrypted", PayrollHoldId_Encrypted);
                    _SalaryHold = DapperORM.ReturnList<Payroll_SalaryHold>("sp_List_Payroll_SalaryHold", param).FirstOrDefault();

                    DynamicParameters paramBu = new DynamicParameters();
                    paramBu.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBu.Add("@p_CmpId", _SalaryHold.CmpId);
                    ViewBag.GetBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBu).ToList();

                    DynamicParameters paramEmpName = new DynamicParameters();
                    paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Mas_Employee.Deactivate=0 and Mas_Employee.EmployeeBranchId= " + _SalaryHold.BranchId + " and Mas_Employee.EmployeeLeft=0 and ContractorID=1 ORDER BY Name");
                    ViewBag.GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                }
                return View(_SalaryHold);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetList
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 492;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_PayrollHoldId_Encrypted", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Payroll_SalaryHold", param).ToList();
                ViewBag._SalaryHold = data;

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
        public ActionResult GetEmployeeName(int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Mas_Employee.Deactivate=0 and Mas_Employee.EmployeeBranchId= " + BranchId + " and Mas_Employee.EmployeeLeft=0 and ContractorID=1 ORDER BY Name");
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

        #region GetEmployeeSalaryAmount
        [HttpGet]
        public ActionResult GetEmployeeSalaryAmount(int EmployeeId, string SalaryMonth)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                var parsedSalaryMonth = DateTime.Parse(SalaryMonth);
                var data = DapperORM.DynamicQuerySingle("Select SalaryNetPay from Payroll_Salary where SalaryEmployeeId=" + EmployeeId + " and Deactivate=0 and SalaryIsDisburse=0 and  (month(SalaryMonthYear)='" + parsedSalaryMonth.ToString("MM") + "' and year(SalaryMonthYear)='" + parsedSalaryMonth.ToString("yyyy") + "')").FirstOrDefault();
                if(data!=null)
                {
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    TempData["Message"] = "The payroll process has not been completed.";
                    TempData["Icon"] = "error";
                    return Json(new { Message= TempData["Message"] ,Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }
               
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsPayrollHoldExists
        [HttpGet]
        public ActionResult IsPayrollHoldExists(int? Payroll_SalaryEmployeeId, string Payroll_SalaryMonth, string PayrollHoldId_Encrypted,int? BranchId,int? CmpId)
        {
            try
            {
                var parsedSalaryMonth = DateTime.Parse(Payroll_SalaryMonth);
                param.Add("@p_process", "IsValidation");
                param.Add("@p_Payroll_SalaryMonth", parsedSalaryMonth.ToString("yyyy-MM-dd"));
                param.Add("@p_Payroll_SalaryEmployeeId", Payroll_SalaryEmployeeId);
                param.Add("@p_PayrollHoldId_Encrypted", PayrollHoldId_Encrypted);
                param.Add("@p_BranchId", BranchId);
                param.Add("@p_CmpId", CmpId);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_SalaryHold", param);
                var Message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                if (Message != "")
                {
                    return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
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
        public ActionResult SaveUpdate(Payroll_SalaryHold OBJHold)
        {
            try
            {
                var PayrollCount = DapperORM.DynamicQuerySingle("Select Count(PayrollLockId) as LockCount from Payroll_LOck where Deactivate=0 and PayrollLockBranchID=" + OBJHold.BranchId + " and Month(PayrollLockMonthYear)='" + OBJHold.Payroll_SalaryMonth.ToString("MM") + "'  and Year(PayrollLockMonthYear) ='" + OBJHold.Payroll_SalaryMonth.ToString("yyyy") + "' and Status=1");
                if (PayrollCount.LockCount != 0)
                {
                    TempData["Message"] = "The record can't be saved because the payroll for this month ('" + OBJHold.Payroll_SalaryMonth.ToString("MMM-yyyy") + "') is locked.";
                    TempData["Icon"] = "error";
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }

                param.Add("@p_process", string.IsNullOrEmpty(OBJHold.PayrollHoldId_Encrypted) ? "Save" : "Update");
                param.Add("@p_PayrollHoldId_Encrypted", OBJHold.PayrollHoldId_Encrypted);
                param.Add("@p_BranchId", OBJHold.BranchId);
                param.Add("@p_CmpId", OBJHold.CmpId);
                param.Add("@p_Payroll_SalaryEmployeeId", OBJHold.Payroll_SalaryEmployeeId);
                param.Add("@p_Payroll_SalaryMonth", OBJHold.Payroll_SalaryMonth.ToString("yyyy-MM-dd"));
                param.Add("@p_Payroll_SalaryAmount", OBJHold.Payroll_SalaryAmount);
                param.Add("@p_HoldRemark", OBJHold.HoldRemark);
                param.Add("@p_HoldEmployeeId", Session["EmployeeId"]);
                param.Add("@p_ReleaseRemark", OBJHold.ReleaseRemark);
                param.Add("@p_ReleaseEmployeeId", Session["EmployeeId"]);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_SalaryHold", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Module_Payroll_PayrollHold", "Module_Payroll_PayrollHold");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(string PayrollHoldId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_PayrollHoldId_Encrypted", PayrollHoldId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_SalaryHold", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Module_Payroll_PayrollHold");
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