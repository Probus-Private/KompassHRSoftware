using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_TimeOffice
{
    public class ESS_TimeOffice_RotationalWeekOffController : Controller
    {
        // GET: ESS/ESS_TimeOffice_RotationalWeekOff
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        public ActionResult ESS_TimeOffice_RotationalWeekOff()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 292;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param1 = new DynamicParameters();
                ViewBag.AddUpdateTitle = "Add";
                param1.Add("@query", "select CompanyId as Id, CompanyName as Name from Mas_CompanyProfile  where Deactivate = 0 order by CompanyName");
                ViewBag.ComapnyName = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param1).ToList();
                ViewBag.BranchName = "";
                ViewBag.DepartmentName = "";

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpPost]
        public ActionResult ESS_TimeOffice_RotationalWeekOff(Atten_RotationalWeekOff Obj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                param = new DynamicParameters();
                param.Add("@p_Qry", @"and EmployeeId not in (select RotationalWeekOffEmployeeId from Atten_RotationalWeekOff where Deactivate = 0) 
                                   and Mas_Employee.EmployeeBranchId = " + Obj.RotationalWeekOffBranchId + " and Mas_Employee.EmployeeDepartmentID = " + Obj.RotationalWeekOffDepartmentID + " order by EmployeeCardNo "); //" and Mas_Employee.EmployeeGradeID = "+Obj.+"
                var data = DapperORM.ExecuteSP<dynamic>("sp_GetEmployeeList", param).ToList();
                ViewBag.GetRotationalWeekOff = data;

                DynamicParameters param1 = new DynamicParameters();
                ViewBag.AddUpdateTitle = "Add";
                param1.Add("@query", "select CompanyId as Id, CompanyName as Name from Mas_CompanyProfile  where Deactivate = 0 order by CompanyName");
                ViewBag.ComapnyName = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param1).ToList();

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_employeeid", Session["EmployeeId"]);
                param2.Add("@p_CmpId", Obj.CmpID);
                ViewBag.BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param2).ToList();

                DynamicParameters param3 = new DynamicParameters();
                param3.Add("@query", "Select DepartmentId as Id,Mas_Department.DepartmentName as Name from Mas_Department where Deactivate=0 order by DepartmentName");
                ViewBag.DepartmentName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param3).ToList();

               
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
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "Select DepartmentId as Id,Mas_Department.DepartmentName as Name from Mas_Department where Deactivate=0 order by DepartmentName");
                var data1 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();

                return Json(new { data, data1 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region  SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(List<RotationalWeekOff> RotationalWeekOff, int CmpId, int BranchId,int DepartmentId, string WeekOffDate)
        {
            try
            {
                for (var i = 0; i < RotationalWeekOff.Count; i++)
                {
                    param.Add("@p_process", "Save");
                    param.Add("@p_CmpID", CmpId);
                    param.Add("@p_RotationalWeekOffBranchId", BranchId);
                    //param.Add("@p_RotationalWeekOffFinancialYaerId", Atten_RotationalWeekOffList[i].RotationalWeekOffFinancialYaerId);
                    param.Add("@p_RotationalWeekOffDepartmentID", DepartmentId);
                    param.Add("@p_RotationalWeekOffEmployeeId", RotationalWeekOff[i].EmployeeId);
                    //param.Add("@p_RotationalWeekOffMonthYear", Atten_RotationalWeekOffList[i].RotationalWeekOffMonthYear);
                    param.Add("@p_RotationalWeekOffDate", WeekOffDate);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedupdateBy", Session["EmployeeId"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_SUD_Atten_RotationalWeekOff", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                }
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                //return RedirectToAction("Inbox", "Inbox", new { Area = "Inbox" });
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
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 292;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_RotationalWeekOffId_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Atten_RotationalWeekOff", param);
                ViewBag.GetRotationalWeekOffList = data;

                return View();
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
        public ActionResult Delete(string RotationalWeekOffId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_RotationalWeekOffId_Encrypted", RotationalWeekOffId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_RotationalWeekOff", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_TimeOffice_RotationalWeekOff");
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