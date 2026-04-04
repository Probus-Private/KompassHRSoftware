using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_BulkUpdateReportingController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: Module/Module_Employee_BulkUpdateReporting
        #region Main View
        public ActionResult Module_Employee_BulkUpdateReporting(BulkUpdateReporting OBJBulk)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 402;
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

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", @"Select ApprovalLevelId as Id , ApprovalLevelName as Name from Tool_ApprovalLevel where Deactivate=0");
                ViewBag.Levels = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@query", @"Select ModuleID as Id , ModuleName as Name from Tool_Module where Deactivate=0");
                ViewBag.ModuleName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();
                ViewBag.GetEmployees = "";
                if (OBJBulk.CmpId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", @"Select Distinct(Mas_Employee_Reporting.ReportingEmployeeID) as Id,  Concat(Mas_Employee.EmployeeName ,' - ', Mas_Employee.EmployeeNo) as Name  from Mas_Employee_Reporting 
                                      Inner Join Mas_Employee on Mas_Employee.EmployeeId = Mas_Employee_Reporting.ReportingEmployeeID
                                      where Mas_Employee_Reporting.ReportingManager1 = " + OBJBulk.ManagerId + " and ReportingModuleID=" + OBJBulk.ModelId + " and ApproverLevel=" + OBJBulk.LevelId + " and Mas_Employee_Reporting.Deactivate = 0 and Mas_Employee.EmployeeLeft=0");
                    var GetEmployees = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).ToList();
                    ViewBag.GetEmployees = GetEmployees;

                    DynamicParameters param7 = new DynamicParameters();
                    param7.Add("@p_employeeid", Session["EmployeeId"]);
                    param7.Add("@p_CmpId", OBJBulk.CmpId);
                    ViewBag.BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param7).ToList();

                    DynamicParameters param3 = new DynamicParameters();
                    param3.Add("@query", @"SELECT DISTINCT(R.ReportingManager1) as Id,Concat(M.EmployeeName ,' - ', M.EmployeeNo) as Name  FROM Mas_Employee_Reporting R JOIN Mas_Employee M ON R.ReportingManager1 = M.EmployeeId WHERE R.deactivate = 0 and m.EmployeeBranchId=" + OBJBulk.BranchId + " order by Name ");
                    ViewBag.ManagerName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param3).ToList();

                    DynamicParameters param4 = new DynamicParameters();
                    param4.Add("@query", @"select employeeid as Id,CONCAT(EmployeeName, ' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeeBranchId=" + OBJBulk.BranchId + " and EmployeeLeft=0 and ContractorID=1 and EmployeeId<>1 order by Name ");
                    ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param4).ToList();

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
        public ActionResult SaveUpdate(List<BulkUpdateReporting> BulkUpdateReporting, int? ManagerId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                //if(ManagerId!=null)
                //{
                //    var pending = DapperORM.DynamicQuerySingle("Select COUNT(*) as PendingCount from Tra_Approval where Status='Pending' and TraApproval_ApproverEmployeeId="+ ManagerId + "").FirstOrDefault();
                //    var a = pending.PendingCount;
                //    if (a>0)
                //    {
                //        TempData["Message"] = "Manager with pending requests awaiting approval.";
                //        TempData["Icon"] = "error";
                //        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                //    }
                //}

                StringBuilder strBuilder = new StringBuilder();
                string abc = "";
                for (int i = 0; i < BulkUpdateReporting.Count; i++)
                {
                    var pending = DapperORM.DynamicQuerySingle("Select COUNT(*) as PendingCount from Tra_Approval where Status='Pending' and TraApproval_ApproverEmployeeId=" + ManagerId + " and TraApproval_EmployeeId=" + BulkUpdateReporting[i].EmployeeId + " AND TraApproval_ModuleId=" + BulkUpdateReporting[i].ModelId + " and Deactivate=0");
                    var a = pending?.PendingCount;
                    if (a > 0)
                    {
                        var PendingemployeeName = DapperORM.DynamicQueryList("Select Concat(Mas_Employee.EmployeeName ,' - ', Mas_Employee.EmployeeNo ,' - ' ,Mas_Branch.BranchName) as Name from Mas_Employee,Mas_Branch  where Mas_Branch.BranchId=Mas_Employee.EmployeeBranchId and  employeeid=" + BulkUpdateReporting[i].EmployeeId + "").FirstOrDefault();
                        var EMPName = PendingemployeeName?.Name;
                        TempData["Message"] = "Manager with pending requests awaiting approval of employee " + EMPName + "";
                        TempData["Icon"] = "error";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                    string SaveReporting = $@"UPDATE Mas_Employee_Reporting 
                                                    SET ReportingManager1 = {BulkUpdateReporting[i].NewManagerId}
                                                    WHERE ReportingModuleID = {BulkUpdateReporting[i].ModelId} 
                                                    AND ReportingEmployeeID = {BulkUpdateReporting[i].EmployeeId} 
                                                    AND ReportingManager1 = {BulkUpdateReporting[i].ManagerId} 
                                                    AND ApproverLevel = {BulkUpdateReporting[i].LevelId};";
                    strBuilder.Append(SaveReporting);
                }

                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    TempData["Message"] = "Record Save successfully";
                    TempData["Icon"] = "success";
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }
                if (abc != "")
                {
                    TempData["Message"] = abc;
                    TempData["Icon"] = "error";
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }
                return RedirectToAction("Module_Employee_BulkUpdateReporting", "Module_Employee_BulkUpdateReporting");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetEmployees
        [HttpGet]
        public ActionResult GetEmployees(int ManagerId, int ModelId, int LeaveId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@query", @"Select Distinct(Mas_Employee_Reporting.ReportingEmployeeID) as Id, Mas_Employee.EmployeeName from Mas_Employee_Reporting 
                                      Inner Join Mas_Employee on Mas_Employee.EmployeeId = Mas_Employee_Reporting.ReportingEmployeeID
                                      where Mas_Employee_Reporting.ReportingManager1 = " + ManagerId + " and ReportingModuleID=" + ModelId + " and ApproverLevel=" + LeaveId + " and Mas_Employee_Reporting.Deactivate = 0");
                var GetEmployees = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).ToList();
                var jsonResult = JsonConvert.SerializeObject(GetEmployees);
                return Json(jsonResult, JsonRequestBehavior.AllowGet);
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

        #region GetManagerName
        [HttpGet]
        public ActionResult GetManagerName(int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters param3 = new DynamicParameters();
                param3.Add("@query", @"SELECT DISTINCT(R.ReportingManager1) as Id,Concat(M.EmployeeName ,' - ', M.EmployeeNo) as Name  FROM Mas_Employee_Reporting R JOIN Mas_Employee M ON R.ReportingManager1 = M.EmployeeId WHERE R.deactivate = 0 and m.EmployeeBranchId=" + BranchId + " order by Name ");
                var ManagerName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param3).ToList();

                DynamicParameters param4 = new DynamicParameters();
                param4.Add("@query", @"select employeeid as Id,CONCAT(EmployeeName, ' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeeBranchId=" + BranchId + " and EmployeeLeft=0 and ContractorID=1 and EmployeeId<>1 order by Name ");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param4).ToList();

                return Json(new { ManagerName = ManagerName, EmployeeName = EmployeeName }, JsonRequestBehavior.AllowGet);
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