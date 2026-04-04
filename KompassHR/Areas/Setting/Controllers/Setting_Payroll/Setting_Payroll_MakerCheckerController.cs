using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Payroll;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Payroll
{
    public class Setting_Payroll_MakerCheckerController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_Payroll_MakerChecker
        #region Main View
        public ActionResult Setting_Payroll_MakerChecker(string MakerChecker_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 500;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Payroll_MakerChecker tblObj = new Payroll_MakerChecker();
                //GET COMPANY NAME
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                var CmpId = GetComapnyName[0].Id;
                //GET BRANCH NAME
                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CmpId), Convert.ToInt32(Session["EmployeeId"]));
                ViewBag.BranchName = Branch;

                DynamicParameters param3 = new DynamicParameters();
                param3.Add("@query", "select employeeid as Id,CONCAT(EmployeeName, ' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and ContractorId=1 and EmployeeId<>1 and Employeeleft=0 order by Name");
                ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param3).ToList();

                //DynamicParameters param3 = new DynamicParameters();
                //ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_MakerChecker_EmployeeName", param3).ToList();

                if (MakerChecker_Encrypted != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_MakerChecker_Encrypted", MakerChecker_Encrypted);
                    tblObj = DapperORM.ReturnList<Payroll_MakerChecker>("sp_List_Payroll_MakerChecker", param).FirstOrDefault();

                    var Branch1 = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(tblObj.MakerCheckerCmpId), Convert.ToInt32(Session["EmployeeId"]));
                    ViewBag.BranchName = Branch1;

                    //DynamicParameters param1 = new DynamicParameters();
                    //param1.Add("@query", "select employeeid as Id,CONCAT(EmployeeName, ' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeeBranchId='" + tblObj.MakerCheckerBranchId + "' and Employeeleft=0 order by Name");
                    //ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();
                    return View(tblObj);
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 500;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_MakerChecker_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Payroll_MakerChecker", param).ToList();
                ViewBag.ListDetails = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsValidation
        public ActionResult IsExists(string MakerChecker_Encrypted, double CmpId, double? BranchId, bool? IsTopManagerApprove, double? TopManagerId, double? PayrollMakerEmpId, double? PayrollCheckerEmpId, double? AccountMakerEmpId, double? AccountCheckerEmpId,double? IncrementMakerEmpId ,double? IncrementCheckerEmpId)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_MakerChecker_Encrypted", MakerChecker_Encrypted);
                    param.Add("@p_MakerCheckerCmpId", CmpId);
                    param.Add("@p_MakerCheckerBranchId", BranchId);
                    param.Add("@p_IsTopManagerApprove", IsTopManagerApprove);
                    param.Add("@p_TopManagerId", TopManagerId);
                    param.Add("@p_PayrollMakerEmpId", PayrollMakerEmpId);
                    param.Add("@p_PayrollCheckerEmpId", PayrollCheckerEmpId);
                    param.Add("@p_AccountMakerEmpId", AccountMakerEmpId);
                    param.Add("@p_AccountCheckerEmpId", AccountCheckerEmpId);
                    param.Add("@p_IncrementMakerEmpId", IncrementMakerEmpId);
                    param.Add("@p_IncrementCheckerEmpId", IncrementCheckerEmpId);

                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_MakerChecker", param);
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
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Payroll_MakerChecker obj)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(obj.MakerChecker_Encrypted) ? "Save" : "Update");
                param.Add("@p_MakerChecker_Encrypted", obj.MakerChecker_Encrypted);
                param.Add("@p_MakerCheckerCmpId", obj.MakerCheckerCmpId);
                param.Add("@p_MakerCheckerBranchId", obj.MakerCheckerBranchId);
                //param.Add("@p_IsTopManagerApprove", obj.IsTopManagerApprove);
                param.Add("@p_TopManagerId", 0);
                param.Add("@p_IsTopManagerApprove", 0);
                //param.Add("@p_TopManagerId", obj.TopManagerId);
                param.Add("@p_PayrollMakerEmpId", obj.PayrollMakerEmpId);
                param.Add("@p_PayrollCheckerEmpId", obj.PayrollCheckerEmpId);
               // param.Add("@p_PayrollApprovalEmpId", obj.PayrollApprovalEmpId);
                param.Add("@p_AccountMakerEmpId", obj.AccountMakerEmpId);
                param.Add("@p_AccountCheckerEmpId", obj.AccountCheckerEmpId);
                param.Add("@p_IncrementMakerEmpId", obj.IncrementMakerEmpId);
                param.Add("@p_IncrementCheckerEmpId", obj.IncrementCheckerEmpId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);

                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_MakerChecker", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Payroll_MakerChecker", "Setting_Payroll_MakerChecker");
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
        public ActionResult GetBusinessUnit(int CmpId)
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

        #region Delete
        [HttpGet]
        public ActionResult Delete(string MakerChecker_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_MakerChecker_Encrypted", MakerChecker_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_MakerChecker", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Payroll_MakerChecker");
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