using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_Onboarding
{
    public class Reports_Onboarding_MenuController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: Reports/Reports_Onboarding_Menu
        #region Reports_Onboarding_Menu 
        public ActionResult Reports_Onboarding_Menu(int? id,int? ScreenId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 275;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                if (ScreenId != null)
                {
                    Session["ModuleId"] = id;
                    Session["ScreenId"] = ScreenId;
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), id, ScreenId, "SubForm", "Report");
                    ViewBag.GetUserMenuList = GetMenuList;
                }
                else
                {
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), Convert.ToInt32(Session["ModuleId"]), Convert.ToInt32(Session["ScreenId"]), "SubForm", "Report");
                    ViewBag.GetUserMenuList = GetMenuList;
                }
                //param.Add("@p_EmployeeId", Session["EmployeeId"]);
                //param.Add("@p_UserRightsModuleId", Session["ModuleId"]);
                //param.Add("@p_Transactiontype", "Report");
                //var GetMenuList = DapperORM.ExecuteSP<dynamic>("sp_GetUserRights_SubMenuList", param).ToList(); // SP_getReportingManager
                //param.Add("@p_AccessPolicyId", Session["UserAccessPolicyId"]);
                //param.Add("@p_ModuleId", Session["ModuleId"]);
                //param.Add("@p_ScreenMenuType", "Report");
                //param.Add("@p_ScreenType", "SubForm");
                //var GetMenuList = DapperORM.ExecuteSP<dynamic>("sp_Access_SubMenuList", param).ToList(); // SP_getReportingManager
                //ViewBag.GetUserMenuList = GetMenuList;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Daily Attendance DropDown
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
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeebranchid in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ") and Mas_Employee.EmployeeLeft=0");
                //paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and Atten_InOut.InOutBranchId in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ") and month( Atten_InOut.InOutDate )='" + GetFromDate.ToString("MM") + "' and year( Atten_InOut.InOutDate )='" + GetFromDate.ToString("yyyy") + "' and EmployeeId<>1) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ")  and ( month(mas_employee.JoiningDate)<='" + GetFromDate.ToString("MM") + "' and year (mas_employee.JoiningDate)<='" + GetFromDate.ToString("yyyy") + "') and (month(LeavingDate)='" + GetFromDate.ToString("MM") + "' and year(mas_employee.LeavingDate)='" + GetFromDate.ToString("yyyy") + "' or mas_employee.LeavingDate is null) order by Name");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                              
                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_qry", " and Mas_ContractorMapping.BranchID in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ")");
                var GetContractorDropdown = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param2).ToList();

                return Json(new { Branch = Branch, EmployeeName = EmployeeName, GetContractorDropdown = GetContractorDropdown }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


        [HttpGet]
        public ActionResult GetUnit(int? CmpId, int? UnitBranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (UnitBranchId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeebranchid in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ") and Mas_Employee.EmployeeLeft=0");
                    var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                    DynamicParameters paramLine = new DynamicParameters();
                    paramLine.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + CmpId + " and Mas_LineMaster.BranchId=" + UnitBranchId + " ");
                    var List_LineMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();

                    DynamicParameters paramUnit = new DynamicParameters();
                    paramLine.Add("@query", "select UnitId as Id,UnitName as Name from Mas_Unit where Deactivate=0 and CmpId=" + CmpId + " and UnitBranchId=" + UnitBranchId + " ");
                    var List_SubUnit = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();


                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@p_qry", " and Mas_ContractorMapping.BranchID =" + UnitBranchId + "");
                    var GetContractorDropdown = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param2).ToList();

                    return Json(new { EmployeeName = EmployeeName, List_LineMaster = List_LineMaster, List_SubUnit = List_SubUnit, GetContractorDropdown = GetContractorDropdown }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeebranchid in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ") and Mas_Employee.EmployeeLeft=0");
                    var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                    DynamicParameters paramLine = new DynamicParameters();
                    paramLine.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + CmpId + " and Mas_LineMaster.BranchId in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ")  ");
                    var List_LineMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();

                    DynamicParameters paramUnit = new DynamicParameters();
                    paramLine.Add("@query", "select UnitId as Id,UnitName as Name from Mas_Unit where Deactivate=0 and CmpId=" + CmpId + " and UnitBranchId  in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ")  ");
                    var List_SubUnit = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@p_qry", " and Mas_ContractorMapping.BranchID in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ") ");
                    var GetContractorDropdown = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param2).ToList();

                    return Json(new { EmployeeName = EmployeeName, List_LineMaster = List_LineMaster, List_SubUnit = List_SubUnit, GetContractorDropdown = GetContractorDropdown }, JsonRequestBehavior.AllowGet);
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