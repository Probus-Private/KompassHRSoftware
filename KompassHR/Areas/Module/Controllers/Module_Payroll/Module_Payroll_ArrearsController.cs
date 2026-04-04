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
    public class Module_Payroll_ArrearsController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: Module/Module_Payroll_Arrears
        #region Module_Payroll_Arrears
        public ActionResult Module_Payroll_Arrears(string ArrearsId_Encrypted)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            // CHECK IF USER HAS ACCESS OR NOT
            int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 476;
            bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
            if (!CheckAccess)
            {
                Session["AccessCheck"] = "False";
                return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
            }

            Payroll_Arrears ObjArrears = new Payroll_Arrears();

            //GET COMPANY NAME
            var GetComapnyName = new BulkAccessClass().GetCompanyName();
            ViewBag.CompanyName = GetComapnyName;
            var CMPID = GetComapnyName[0].Id;
            //GET BRANCH NAME
            var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CMPID), Convert.ToInt32(Session["EmployeeId"]));
            ViewBag.BranchName = Branch;


            if (ArrearsId_Encrypted != null)
            {
                DynamicParameters param = new DynamicParameters();
                ViewBag.AddUpdateTitle = "Update";
                param = new DynamicParameters();
                param.Add("p_ArrearsId_Encrypted", ArrearsId_Encrypted);
                ObjArrears = DapperORM.ReturnList<Payroll_Arrears>("sp_List_Payroll_Arrears", param).FirstOrDefault();

                var Branch1 = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(ObjArrears.ArrearsCmpId), Convert.ToInt32(Session["EmployeeId"]));
                ViewBag.BranchName = Branch1;

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "select employeeid as Id,CONCAT(EmployeeName, ' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeeBranchId='" + ObjArrears.ArrearsBranchId + "' and Employeeleft=0 order by Name");
                ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();
            }

            return View(ObjArrears);
        }
        #endregion

        #region GetList
        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 476;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_ArrearsId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Payroll_Arrears", param).ToList();
                ViewBag.GetPayrollArrears = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Get Branch Name
        [HttpGet]
        public ActionResult GetMonthlyBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CmpId), Convert.ToInt32(Session["EmployeeId"]));
                return Json(new { Branch = Branch }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Get Employee Name
        [HttpGet]
        public ActionResult GetEmployeeName(int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select employeeid as Id,CONCAT(EmployeeName, ' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeeBranchId='" + BranchId + "' and Employeeleft=0 order by Name");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                return Json(new { EmployeeName = EmployeeName }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsValidation
        public JsonResult IsExists(string ArrearsId_Encrypted, double ArrearsCmpId, double? ArrearsBranchId, double? ArrearsEmployeeId, string Type, DateTime? ArrearsCalculateMonthYear, DateTime? ArrearsPaidMonthYear, float PayableDays, float PayableHrs, float ArrearsPayableDays, float ArrearsPaidHrs,float ArrearsAmount)
        {
            try
            {
                int month = ArrearsCalculateMonthYear.Value.Month;
                int year = ArrearsCalculateMonthYear.Value.Year;

                var PayrollCount = DapperORM.DynamicQuerySingle("Select Count(PayrollLockId) as LockCount from Payroll_LOck where Deactivate=0 and PayrollLockBranchID=" + ArrearsBranchId + " and Month(PayrollLockMonthYear)='" + month + "'  and Year(PayrollLockMonthYear) ='" + year + "' and Status=1");
                if (PayrollCount.LockCount != 0)
                {
                    TempData["Message"] = "The record can't be saved because the payroll for this month ('" + Convert.ToDateTime(ArrearsCalculateMonthYear).ToString("MMM-yyyy") + "') is locked.";
                    TempData["Icon"] = "error";
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }

                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_ArrearsId_Encrypted", ArrearsId_Encrypted);
                    param.Add("@p_ArrearsCmpId", ArrearsCmpId);
                    param.Add("@p_ArrearsBranchId", ArrearsBranchId);
                    param.Add("@p_ArrearsEmployeeId", ArrearsEmployeeId);
                    param.Add("@p_ArrearsType", Type);
                    param.Add("@p_ArrearsMonthYear", ArrearsCalculateMonthYear);
                    param.Add("@p_PaidMonthYear", ArrearsPaidMonthYear);

                    param.Add("@p_PayableDays", PayableDays);
                    param.Add("@p_PayableOTHrs", PayableHrs);
                    param.Add("@p_ArrearsPayableDays", ArrearsPayableDays);
                    param.Add("@p_ArrearsPayableOTHrs", ArrearsPaidHrs);
                    param.Add("@p_ArrearsAmount", ArrearsAmount);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_Arrears", param);
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

            }
            catch (Exception Ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
                //return RedirectToAction(Ex.Message.ToString(), "Wage");
            }
        }

        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Payroll_Arrears Arrears)
        {
            try
            {
                var CompanyId = Session["CompanyId"];
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(Arrears.ArrearsId_Encrypted) ? "Save" : "Update");
                param.Add("@p_ArrearsId", Arrears.ArrearsId);
                param.Add("@p_ArrearsId_Encrypted", Arrears.ArrearsId_Encrypted);
                param.Add("@p_ArrearsCmpId", Arrears.ArrearsCmpId);
                param.Add("@p_ArrearsBranchId", Arrears.ArrearsBranchId);
                param.Add("@p_ArrearsEmployeeId", Arrears.ArrearsEmployeeId);
                param.Add("@p_ArrearsType", Arrears.ArrearsType);
                param.Add("@p_ArrearsMonthYear", Arrears.ArrearsCalculateMonthYear);
                param.Add("@p_PaidMonthYear", Arrears.ArrearsPaidMonthYear);
                param.Add("@p_PayableDays", Arrears.PayableDays);
                param.Add("@p_PayableOTHrs", Arrears.PayableOTHrs);
                param.Add("@p_ArrearsPayableDays", Arrears.ArrearsPayableDays);
                param.Add("@p_ArrearsPayableOTHrs", Arrears.ArrearsPayableOTHrs);
                param.Add("@p_ArrearsAmount", Arrears.ArrearsAmount);
                param.Add("@p_ArrearsRemark", Arrears.Remark);
                param.Add("@p_ArrearsPF", Arrears.ArrearsPF);
                param.Add("@p_ArrearsPT", Arrears.ArrearsPT);
                param.Add("@p_ArrearsESI", Arrears.ArrearsESI);
                param.Add("@p_IsMergeWithSalary", Arrears.IsMergeWithSalary);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_Arrears", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Module_Payroll_Arrears", "Module_Payroll_Arrears");
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "Module_Payroll_Arrears");
            }
        }
        #endregion

        #region GetArrearsDetails
        [HttpGet]
        public ActionResult GetArrearsDetails(string CmpId, string BranchId, int? EmployeeId, string Type, DateTime CalMonthYear, DateTime PaidMonthYear)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_BranchId", BranchId);
                param.Add("@p_PaidMonth", PaidMonthYear);
                param.Add("@p_ArrearsType", Type);
                param.Add("@p_EmployeeId", EmployeeId);
                param.Add("@p_ArrearsCalculateMonth", CalMonthYear);
                var data = DapperORM.ExecuteSP<dynamic>("sp_Get_Payroll_ArrearsDetail", param).ToList(); // SP_getReportingManager

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(string ArrearsId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_ArrearsId_Encrypted", ArrearsId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_Arrears", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Module_Payroll_Arrears");
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