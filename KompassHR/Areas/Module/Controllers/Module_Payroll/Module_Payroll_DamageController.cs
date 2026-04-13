using Dapper;
using KompassHR.Areas.Module.Models.Module_Payroll;
using KompassHR.Areas.Setting.Models.Setting_Leave;
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
    public class Module_Payroll_DamageController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Module/Module_Payroll_Damage
        public ActionResult Module_Payroll_Damage(Payroll_PayrollDamage OBJPayrollDamage)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 165;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@query", "Select  CompanyId As Id, CompanyName As Name from Mas_CompanyProfile where Deactivate=0 order by Name");
                var listCompanyName = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", paramCompany).ToList();
                ViewBag.GetCompanyName = listCompanyName;

                var Query = "";
                if (OBJPayrollDamage.CmpID != 0)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_employeeid", Session["EmployeeId"]);
                    param.Add("@p_CmpId", OBJPayrollDamage.CmpID);
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                    ViewBag.GetBranchName = data;


                    if (OBJPayrollDamage.PayrollDamageBranchID != 0)
                    {
                        Query = "and MAS_EMPLOYEE.EmployeeBranchId=" + OBJPayrollDamage.PayrollDamageBranchID + "";

                        DynamicParameters paramEMP = new DynamicParameters();
                        paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId='" + OBJPayrollDamage.PayrollDamageBranchID + "'and Mas_Employee.EmployeeLeft=0 order by Name");
                        var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                        ViewBag.GetEmployeeList = GetEmployeeName;
                    }
                    else
                    {
                        Query = "and MAs_employee.employeeBranchId in (select mas_branch.BranchID as Id from UserBranchMapping,mas_branch where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + OBJPayrollDamage.CmpID + " and UserBranchMapping.IsActive = 1)";
                    }

                    //var GetMonthYear = OBJPayrollDamage.OtherEarningMonthYear.ToString("yyyy-MM-dd");
                    if (OBJPayrollDamage.PayrollDamageEmployeeId == 0)
                    {
                        Query = Query + "and MAS_EMPLOYEE.EMPLOYEEID NOT IN (SELECT PayrollDamageEmployeeId FROM Payroll_PayrollDamage where Payroll_PayrollDamage.deactivate=0 AND PayrollDamageMonthYear=FORMAT(convert(datetime," + OBJPayrollDamage.PayrollDamageMonthYear.ToString("yyyy-MM-dd") + "),''))";
                    }
                    else
                    {
                        Query = Query + "and MAS_EMPLOYEE.EMPLOYEEID =" + OBJPayrollDamage.PayrollDamageEmployeeId + "";
                    }

                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@P_Qry", Query);
                    var PayrollDamageEmployeeLoad = DapperORM.ReturnList<dynamic>("sp_GetOtherEarningEmployeeLoad", paramList).ToList();
                    ViewBag.GetPayrollDamageEmployee = PayrollDamageEmployeeLoad;
                }
                else
                {
                    ViewBag.GetPayrollDamageEmployee = "";
                    ViewBag.GetBranchName = "";
                    ViewBag.GetEmployeeList = "";
                }
                return View(OBJPayrollDamage);
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
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeeLeft=0 and  EmployeeBranchId=" + BranchId + "order by Name");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.EmployeeList = data;
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region IsValidation
        [HttpPost]
        public JsonResult IsDamageExists(string DocDate, DateTime DamageMonthYear, string PayrollDamageID_Encrypted, List<PayrollDamage> Record, string PayrollDamageBranchID)
        {
            try
            {
                for (var i = 0; i < Record.Count; i++)
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_PayrollDamageID_Encrypted", PayrollDamageID_Encrypted);
                    param.Add("@p_PayrollDamageEmployeeId", Record[i].PayrollDamageEmployeeId);
                    param.Add("@p_PayrollDamageBranchID",PayrollDamageBranchID);
                    param.Add("@p_PayrollDamageMonthYear", DamageMonthYear.ToString("yyyy-MM-dd"));              

                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_PayrollDamage", param);
                    
                }
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
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
            catch (Exception)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(string DocDate, DateTime DamageMonthYear, string PayrollDamageID_Encrypted, List<PayrollDamage> Record, string PayrollDamageBranchID,string CmpID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                if (Record != null)
                {
                    for (var i = 0; i < Record.Count; i++)
                    {
                        if (PayrollDamageID_Encrypted != "")
                        {
                            param.Add("@p_process", "Update");
                        }
                        else
                        {
                            param.Add("@p_process", "Save");
                        }
                        param.Add("@p_PayrollDamageID_Encrypted",PayrollDamageID_Encrypted);
                        param.Add("@p_CmpID", CmpID);
                        param.Add("@p_PayrollDamageBranchID", PayrollDamageBranchID);
                        param.Add("@p_DocDate", DocDate);
                        param.Add("@p_PayrollDamageMonthYear", DamageMonthYear);                       
                        param.Add("@p_PayrollDamageEmployeeId", Record[i].PayrollDamageEmployeeId);
                        param.Add("@p_PayrollDamageAmount", Record[i].PayrollDamageAmount);
                        param.Add("@p_PayrollDamageRemarks", Record[i].PayrollDamageRemarks);

                        param.Add("@p_MachineName", Dns.GetHostName().ToString());
                        param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_PayrollDamage", param);
                        TempData["Message"] = param.Get<string>("@p_msg");
                        TempData["Icon"] = param.Get<string>("@p_Icon");
                    }
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return RedirectToAction("Module_Payroll_Damage", "Module_Payroll_Damage");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "Module_Payroll_Damage");
            }
        }
        #endregion

        #region GetList

        public ActionResult GetList(Payroll_Damage_List PayrollDamage_List)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 165;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                if (PayrollDamage_List.PayrollDamageMonthYear != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_PayrollDamageID_Encrypted", "List");
                    param.Add("@p_PayrollDamageMonthYear", PayrollDamage_List.PayrollDamageMonthYear);
                    var data = DapperORM.DynamicList("sp_List_Payroll_PayrollDamage", param);
                    ViewBag.PayrollDamageList = data;
                }
                else
                {
                    ViewBag.PayrollDamageList = "";
                }

                return View(PayrollDamage_List);
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
        public ActionResult GetListUnit(DateTime? MonthYear)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@p_PayrollDamageID_Encrypted", "List");
            param.Add("@p_PayrollDamageMonthYear", MonthYear);
            var data = DapperORM.DynamicList("sp_List_Payroll_PayrollDamage", param);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Delete Event
        public ActionResult Delete(string PayrollDamageID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_PayrollDamageID_Encrypted", PayrollDamageID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_PayrollDamage", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Module_Payroll_Damage", new { Area = "Module" });
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "Module_Payroll_Damage");
            }

        }
        #endregion
    }
}