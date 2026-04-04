using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
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
using static KompassHR.Areas.ESS.Models.ESS_TimeOffice.Atten_WeekOffAdjust;

namespace KompassHR.Areas.ESS.Controllers.ESS_TimeOffice
{
    public class ESS_TimeOffice_PublicHolidayAdjustController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_TimeOffice_PublicHolidayAdjust
        #region Public Holiday Main View
        public ActionResult ESS_TimeOffice_PublicHolidayAdjust(Atten_WeekOffAdjust OBJAtten_WeekOffAdjust)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 484;
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
                var GetDefaultCmpId = GetComapnyName[0].Id;

                DynamicParameters paramDeptName = new DynamicParameters();
                paramDeptName.Add("@query", "Select DepartmentId As Id,DepartmentName As Name from Mas_Department where Deactivate=0 order by Name");
                var GetDepartment = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramDeptName).ToList();
                ViewBag.GetEmployeeDepartment = GetDepartment;

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", GetDefaultCmpId);
                var listBranchName1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                ViewBag.GetBranchName = listBranchName1;

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + listBranchName1[0].Id + "");
                var GetContractorDropdown = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param2).ToList();
                ViewBag.ContractorName = GetContractorDropdown;

                ViewBag.GetUnitList = "";

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "Select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and Mas_Employee.EmployeeLeft=0 and EmployeeBranchId in (select mas_branch.BranchID as Id from UserBranchMapping,mas_branch where  UserBranchMapping.employeeid =5068 and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + GetDefaultCmpId + " and UserBranchMapping.IsActive = 1 ) order by Name");
                var GetEmployeeName1 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();
                ViewBag.GetEmployeeList = GetEmployeeName1;

                var Query = "";
                if (OBJAtten_WeekOffAdjust.CmpId != null)
                {
                    DynamicParameters param0 = new DynamicParameters();
                    param0.Add("@p_employeeid", Session["EmployeeId"]);
                    param0.Add("@p_CmpId", OBJAtten_WeekOffAdjust.CmpId);
                    var listBranchName0 = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param0).ToList();
                    ViewBag.GetBranchName = listBranchName0;

                    if (OBJAtten_WeekOffAdjust.WOBranchId != null)
                    {
                        
                        DynamicParameters paramEMP = new DynamicParameters();
                        paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId='" + OBJAtten_WeekOffAdjust.WOBranchId + "'and Mas_Employee.EmployeeLeft=0 order by Name");
                        var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                        ViewBag.GetEmployeeList = GetEmployeeName;

                        DynamicParameters paramUnit1 = new DynamicParameters();
                        paramUnit1.Add("@query", "select UnitId as Id,UnitName as Name from Mas_Unit where Deactivate=0 and UnitBranchId=" + OBJAtten_WeekOffAdjust.WOBranchId + "");
                        var List_SubUnit1 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramUnit1).ToList();
                        ViewBag.GetUnitList = List_SubUnit1;

                        DynamicParameters param11 = new DynamicParameters();
                        param11.Add("@p_qry", " and  Mas_ContractorMapping.BranchID=" + OBJAtten_WeekOffAdjust.WOBranchId + "");
                        var GetContractorDropdown11 = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param11).ToList();
                        ViewBag.ContractorName = GetContractorDropdown11;
                    }
                   
                    if (OBJAtten_WeekOffAdjust.WOBranchId != null && OBJAtten_WeekOffAdjust.ContractorID != null)
                    {
                        DynamicParameters paramEMP = new DynamicParameters();
                        paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId='" + OBJAtten_WeekOffAdjust.WOBranchId + "' and ContractorID='" + OBJAtten_WeekOffAdjust.ContractorID + "'and Mas_Employee.EmployeeLeft=0 order by Name");
                        var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                        ViewBag.GetEmployeeList = GetEmployeeName;
                    }

                    if (OBJAtten_WeekOffAdjust.WODepartmentID != null)
                    {
                        Query = Query + " and MAS_EMPLOYEE.EMPLOYEEDepartmentId=" + OBJAtten_WeekOffAdjust.WODepartmentID + "";

                    }
                    var GetFromDate = OBJAtten_WeekOffAdjust.WOFromDate.ToString("yyyy-MM-dd");
                    var GetAdjustDate = OBJAtten_WeekOffAdjust.WOAdjustDate.ToString("yyyy-MM-dd");
                    var OnlyFromDate = OBJAtten_WeekOffAdjust.WOAdjustDate.ToString("dd");
                    if (OBJAtten_WeekOffAdjust.WOAdjustEmployeeId == null)
                    {
                        Query = Query + " and MAS_EMPLOYEE.EmployeeBranchId=" + OBJAtten_WeekOffAdjust.WOBranchId + " and MAS_EMPLOYEE.EMPLOYEEID NOT IN (SELECT WOAdjustEmployeeId FROM Atten_WeekOffAdjust where Atten_WeekOffAdjust.WOBranchId = " + OBJAtten_WeekOffAdjust.WOBranchId + " and  Atten_WeekOffAdjust.deactivate = 0 and WOFromDate = '" + GetFromDate + "' Union SELECT WOAdjustEmployeeId FROM Atten_WeekOffAdjust where Atten_WeekOffAdjust.WOBranchId = " + OBJAtten_WeekOffAdjust.WOBranchId + " and Atten_WeekOffAdjust.deactivate = 0 and WOAdjustDate = '" + GetAdjustDate + "' ) and Mas_Employee_Attendance.EM_Atten_WOFF1 = FORMAT(convert(datetime, '" + GetFromDate + "'), 'dddd')";
                    }
                    else
                    {
                        Query = Query + " and MAS_EMPLOYEE.EMPLOYEEID NOT IN (SELECT WOAdjustEmployeeId FROM Atten_WeekOffAdjust where Atten_WeekOffAdjust.WOBranchId = " + OBJAtten_WeekOffAdjust.WOBranchId + " and  Atten_WeekOffAdjust.deactivate = 0 and WOFromDate = '" + GetFromDate + "' Union SELECT WOAdjustEmployeeId FROM Atten_WeekOffAdjust where Atten_WeekOffAdjust.WOBranchId = " + OBJAtten_WeekOffAdjust.WOBranchId + " and Atten_WeekOffAdjust.deactivate = 0 and WOAdjustDate = '" + GetAdjustDate + "' ) and Mas_Employee_Attendance.EM_Atten_WOFF1 = FORMAT(convert(datetime, '" + GetFromDate + "'), 'dddd') and MAS_EMPLOYEE.EMPLOYEEID=" + OBJAtten_WeekOffAdjust.WOAdjustEmployeeId + "";
                    }

                    if (OBJAtten_WeekOffAdjust.SubUnitId != null)
                    {
                        Query = Query + " and MAS_EMPLOYEE.EmployeeUnitID=" + OBJAtten_WeekOffAdjust.SubUnitId + "";
                    }
                    if (OBJAtten_WeekOffAdjust.ContractorID != null)
                    {
                        Query = Query + " and MAS_EMPLOYEE.ContractorID=" + OBJAtten_WeekOffAdjust.ContractorID + "";
                    }

                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@P_Qry", Query);
                    var GetWOEmployeeLoad = DapperORM.ReturnList<dynamic>("sp_GetWOEmployeeLoad", paramList).ToList();
                    ViewBag.WOEmployeeLoad = GetWOEmployeeLoad;
                    if (GetWOEmployeeLoad.Count == 0)
                    {
                        TempData["Message"] = "Record Not Found !";
                        TempData["Icon"] = "error";
                    }
                }
                else
                {
                    ViewBag.WOEmployeeLoad = "";
                }


                return View(OBJAtten_WeekOffAdjust);
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
                var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                DynamicParameters param6 = new DynamicParameters();
                param6.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId  in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ") and Mas_Employee.EmployeeLeft=0");
                var EmployeeList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param6).ToList();

                //DynamicParameters param2 = new DynamicParameters();
                //param2.Add("@p_qry", " and  Mas_ContractorMapping.CmpID=" + CmpId + "");
                //var GetContractorDropdown = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param2).ToList();
                //ViewBag.ContractorName = GetContractorDropdown;

                return Json(new { BranchName = BranchName, EmployeeList = EmployeeList }, JsonRequestBehavior.AllowGet);
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
        public ActionResult GetEmployeeName(int? WOBranchId, int? ContractorID, int? CmpId)
        {
            try
            {
                if (ContractorID == null && WOBranchId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId='" + WOBranchId + "'and Mas_Employee.EmployeeLeft=0  order by Name");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else if (ContractorID != null && WOBranchId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId='" + WOBranchId + "' and ContractorID='" + ContractorID + "'and Mas_Employee.EmployeeLeft=0  order by Name");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID='" + CmpId + "' and Mas_Employee.EmployeeLeft=0 order by Name");
                    var data2 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();
                    return Json(data2, JsonRequestBehavior.AllowGet);
                }

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
        public ActionResult IsPublicHolidayExists(DateTime WOAdjustDate, List<WeekOffAdjust> Record, DateTime WOFromDate, int? WOBranchId)//IsWeekOfAdjustmentExists
        {
            try
            {
                var AttenLockCount = DapperORM.DynamicQuerySingle("Select Count(AtdLockIDBranchId) as LockCount from Atten_Lock where Deactivate=0  and Month(AtdLockIDMonth)='" + WOFromDate.ToString("MM") + "'  and Year(AtdLockIDMonth) ='" + WOFromDate.ToString("yyyy") + "'  and AtdLockIDBranchId=" + WOBranchId + " and AtdLock=1");
                if (AttenLockCount.LockCount != 0)
                {
                    var message1 = "Record can''t be saved because the month/year is already locked.";
                    var Icon2 = "error";
                    return Json(new { Message = message1, Icon = Icon2 }, JsonRequestBehavior.AllowGet);
                }
                //var OtherDeductionDocDate = DateTime.Parse(DocDate);
                var OtherDeductionMonthYear = WOAdjustDate.ToString("yyyy-MM-dd");
                for (var i = 0; i < Record.Count; i++)
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_PHAdjustEmployeeId", Record[i].WOAdjustEmployeeId);
                    param.Add("@p_PHAdjustDate", WOAdjustDate.ToString("yyyy-MM-dd"));
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_PublicHolidayAdjust", param);
                    var Message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    if (Icon == "error")
                    {
                        return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                    }

                }
                return Json(true, JsonRequestBehavior.AllowGet);
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
        public ActionResult SaveUpdate(List<WeekOffAdjust> Record, int? CmpID, int? WODepartmentID, DateTime WOFromDate, DateTime WOAdjustDate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 484;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                if (Record != null)
                {
                    for (var i = 0; i < Record.Count; i++)
                    {
                        param.Add("@p_process", "Save");
                        param.Add("@p_CmpID", CmpID);
                        param.Add("@p_PHDepartmentID", WODepartmentID);
                        param.Add("@p_PHAdjustEmployeeId", Record[i].WOAdjustEmployeeId);
                        param.Add("@p_PHFromDate", WOFromDate);
                        param.Add("@p_PHAdjustDate", WOAdjustDate);
                        param.Add("@p_MachineName", Dns.GetHostName().ToString());
                        param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        var data = DapperORM.ExecuteReturn("sp_SUD_Atten_PublicHolidayAdjust", param); 
                         TempData["Message"] = param.Get<string>("@p_msg");
                        TempData["Icon"] = param.Get<string>("@p_Icon");
                    }
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return RedirectToAction("ESS_TimeOffice_PublicHolidayAdjust", "ESS_TimeOffice_PublicHolidayAdjust");
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
        public ActionResult GetList(WeekOffAdjust_List WeekOffAdjustList)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 484;
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
                var GetDefaultCmpId = GetComapnyName[0].Id;

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", GetDefaultCmpId);
                var listBranchName1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                ViewBag.GetBranchName = listBranchName1;

                if (WeekOffAdjustList.WOAdjustDate != null)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_employeeid", Session["EmployeeId"]);
                    param1.Add("@p_CmpId", WeekOffAdjustList.CmpId);
                    var listBranchName4 = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param1).ToList();
                    ViewBag.GetBranchName = listBranchName4;


                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_PublicHolidayAdjustId_Encrypted", "List");
                    paramList.Add("@p_PHFromDate", WeekOffAdjustList.WOFromDate);
                    paramList.Add("@p_BranchId", WeekOffAdjustList.WOBranchId);
                    paramList.Add("@p_PHAdjustDate", WeekOffAdjustList.WOAdjustDate);
                    var data = DapperORM.DynamicList("sp_List_Atten_PublicHoliayAdjust", paramList);
                    ViewBag.GetWeekOffAdjustList = data;
                    return View(WeekOffAdjustList);
                }
                else
                {
                    ViewBag.GetWeekOffAdjustList = "";
                    return View();
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region Delete
        public ActionResult Delete(List<WeekOffAdjust_List> ObjRecordList)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                for (var i = 0; i < ObjRecordList.Count; i++)
                {
                    param.Add("@p_process", "Delete");
                    param.Add("@p_PHAdjustEmployeeId", ObjRecordList[i].EmployeeID);
                    param.Add("@p_PHBranchId", ObjRecordList[i].WOBranchId);
                    param.Add("@p_PublicHolidayAdjustId_Encrypted", ObjRecordList[i].WeekOffAdjustId_Encrypted);
                    param.Add("@p_PHAdjustDate", ObjRecordList[i].WOAdjustDate);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_PublicHolidayAdjust", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    var Error = param.Get<string>("@p_Icon");
                    if (Error == "error")
                    {
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }

                }
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
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
        public ActionResult GetEmployeeList(int? WOBranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                if (WOBranchId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId='" + WOBranchId + "'and Mas_Employee.EmployeeLeft= 0 order By Name");
                    var EmployeeList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                    DynamicParameters paramUnit = new DynamicParameters();
                    paramUnit.Add("@query", "select UnitId as Id,UnitName as Name from Mas_Unit where Deactivate=0 and UnitBranchId=" + WOBranchId + "");
                    var List_SubUnit = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramUnit).ToList();

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + WOBranchId + "");
                    var GetContractorDropdown = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param2).ToList();

                    return Json(new { EmployeeList = EmployeeList, List_SubUnit = List_SubUnit, GetContractorDropdown = GetContractorDropdown }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }

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
