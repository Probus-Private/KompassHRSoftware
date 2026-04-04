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
    public class Module_Payroll_FineAndPenaltyController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Module/Module_Payroll_FineAndPenalty
        #region FineAndPenalty MAin VIew 
        public ActionResult Module_Payroll_FineAndPenalty(Payroll_PayrollFine OBJPayrollFine)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 164;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetCompanyName = GetComapnyName;

                var Query = "";
                if (OBJPayrollFine.CmpID != 0)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_employeeid", Session["EmployeeId"]);
                    param.Add("@p_CmpId", OBJPayrollFine.CmpID);
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                    ViewBag.GetBranchName = data;


                    if (OBJPayrollFine.PayrollFineBranchID != 0)
                    {
                        Query = "and MAS_EMPLOYEE.EmployeeBranchId=" + OBJPayrollFine.PayrollFineBranchID + "";

                        DynamicParameters paramEMP = new DynamicParameters();
                        paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId='" + OBJPayrollFine.PayrollFineBranchID + "'and Mas_Employee.EmployeeLeft=0 order by Name");
                        var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                        ViewBag.GetEmployeeList = GetEmployeeName;
                    }
                    else
                    {
                        Query = "and MAs_employee.employeeBranchId in (select mas_branch.BranchID as Id from UserBranchMapping,mas_branch where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + OBJPayrollFine.CmpID + " and UserBranchMapping.IsActive = 1)";
                    }

                    //var GetMonthYear = OBJPayrollFine.OtherEarningMonthYear.ToString("yyyy-MM-dd");
                    if (OBJPayrollFine.PayrollFineEmployeeId == 0)
                    {
                        Query = Query + "and MAS_EMPLOYEE.EMPLOYEEID NOT IN (SELECT PayrollFineEmployeeId FROM Payroll_PayrollFine where Payroll_PayrollFine.deactivate=0 AND PayrollFineMonthYear=FORMAT(convert(datetime," + OBJPayrollFine.PayrollFineMonthYear.ToString("yyyy-MM-dd") + "),''))";
                    }
                    else
                    {
                        Query = Query + "and MAS_EMPLOYEE.EMPLOYEEID =" + OBJPayrollFine.PayrollFineEmployeeId + "";
                    }

                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@P_Qry", Query);
                    var OtherEarningEmployeeLoad = DapperORM.ReturnList<dynamic>("sp_GetOtherEarningEmployeeLoad", paramList).ToList();
                    ViewBag.GetPayrollFineLoad = OtherEarningEmployeeLoad;
                }
                else
                {
                    ViewBag.GetPayrollFineLoad = "";
                    ViewBag.GetBranchName = "";
                    ViewBag.GetEmployeeList = "";
                }
                return View(OBJPayrollFine);
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
            DynamicParameters param = new DynamicParameters();
            param.Add("@query", "Select  BranchId, BranchName from Mas_Branch where Deactivate=0 and CmpId= '" + CmpId + "'");
            var data = DapperORM.ReturnList<Mas_Branch>("sp_QueryExcution", param).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region GetEmployeeName
        [HttpGet]
        public ActionResult GetEmployeeName(int BranchId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@query", "Select EmployeeId As Id,EmployeeName As Name from Mas_employee where Deactivate=0 and EmployeeBranchId='" + BranchId + "'");
            var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
            ViewBag.EmployeeList = data;
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region IsValidation
        [HttpPost]
        public JsonResult IsFineAndPenaltyExists(string CmpID, string PayrollFineBranchID, string DocDate, DateTime PayrollFineMonthYear, List<Payroll_Fine> Record)
        {
            try
            {
                var GetPayrollFineMonthYear = PayrollFineMonthYear.ToString("yyyy-MM-dd");
                for (var i = 0; i < Record.Count; i++)
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_PayrollFineID_Encrypted", "");
                    param.Add("@p_PayrollFineEmployeeId", Record[i].PayrollFineEmployeeId);
                    param.Add("@p_PayrollFineBranchID", PayrollFineBranchID);
                    param.Add("@p_PayrollFineMonthYear", GetPayrollFineMonthYear);

                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_PayrollFine", param);

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
            catch (Exception Ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(string CmpID, string PayrollFineBranchID, string DocDate, DateTime PayrollFineMonthYear, List<Payroll_Fine> Record,string PayrollFineID_Encrypted)
        {
            try
            {
                if (Record != null)
                {
                    for (var i = 0; i < Record.Count; i++)
                    {
                        if (PayrollFineID_Encrypted != "")
                        {
                            param.Add("@p_process", "Update");
                        }
                        else
                        {
                            param.Add("@p_process", "Save");
                        }
                        param.Add("@p_PayrollFineID_Encrypted", PayrollFineID_Encrypted);
                        param.Add("@p_CmpID", CmpID);
                        param.Add("@p_PayrollFineBranchID", PayrollFineBranchID);
                        param.Add("@p_DocDate", DocDate);
                        param.Add("@p_PayrollFineMonthYear", PayrollFineMonthYear);
                        param.Add("@p_PayrollFineEmployeeId", Record[i].PayrollFineEmployeeId);
                        param.Add("@p_PayrollFineAmount", Record[i].PayrollFineAmount);
                        param.Add("@p_PayrollFineRemarks", Record[i].PayrollFineRemarks);

                        param.Add("@p_MachineName", Dns.GetHostName().ToString());
                        param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_PayrollFine", param);
                        TempData["Message"] = param.Get<string>("@p_msg");
                        TempData["Icon"] = param.Get<string>("@p_Icon");
                    }
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return RedirectToAction("Module_Payroll_FineAndPenalty", "Module_Payroll_FineAndPenalty");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "Module_Payroll_FineAndPenalty");
            }
        }
        #endregion

        #region GetList

        public ActionResult GetList(Payroll_Fine_List OBJPayrollFine_List)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 164;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                if (OBJPayrollFine_List.PayrollFineMonthYear != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_PayrollFineID_Encrypted", "List");
                    param.Add("@p_PayrollFineMonthYear", OBJPayrollFine_List.PayrollFineMonthYear);
                    var data = DapperORM.DynamicList("sp_List_Payroll_PayrollFine", param);
                    ViewBag.PayrollFine = data;
                }
                else
                {
                    ViewBag.PayrollFine = "";
                }

                return View(OBJPayrollFine_List);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        #endregion

        #region Delete Event
        public ActionResult Delete(string PayrollFineID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_PayrollFineID_Encrypted", PayrollFineID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_PayrollFine", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Module_Payroll_FineAndPenalty", new { Area = "Module" });
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "Module_Payroll_FineAndPenalty");
            }

        }
        #endregion
    }
}