using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_EmployeeTransfer_PremiumController : Controller
    {
        #region Main View
        DynamicParameters param = new DynamicParameters();
        // GET: Module/Module_Employee_EmployeeTransfer_Premium
        public ActionResult Module_Employee_EmployeeTransfer_Premium(string EmployeeTransferId_Encrypted)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 920;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";

                Employee_EmployeeTransfer_Premium EmployeeTransfer = new Employee_EmployeeTransfer_Premium();

                param.Add("@query", "Select CompanyId as Id,CompanyName as Name from Mas_CompanyProfile Where Deactivate=0");
                var CompanyName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetCompanyname = CompanyName;


                //param.Add("@query", "Select BranchId as Id,BranchName as Name from Mas_Branch Where Deactivate=0");
                //var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                //ViewBag.GetBranchName = BranchName;


                //param.Add("@query", "Select BranchId as Id,BranchName as Name from Mas_Branch Where Deactivate=0");
                //var ToBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                //ViewBag.GetToBranchName = ToBranchName;


                //param.Add("@query", "Select EmployeeId as Id,EmployeeName as Name from Mas_Employee Where Deactivate=0 And EmployeeLeft=0 ");
                //var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                //ViewBag.GetEmployeeName = EmployeeName;
                
                if (EmployeeTransferId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@P_EmployeeTransferId_Encrypted", EmployeeTransferId_Encrypted);
                    EmployeeTransfer = DapperORM.ReturnList<Employee_EmployeeTransfer_Premium>("sp_List_EmployeeTransfer_Premium", param).FirstOrDefault();


                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_employeeid", Session["EmployeeId"]);
                    param1.Add("@p_CmpId", EmployeeTransfer.CmpId);
                    var listMas_Branch = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param1).ToList();
                    ViewBag.GetBranchName = listMas_Branch;
                    ViewBag.GetToBranchName= listMas_Branch;
                    
                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@query", @"SELECT EmployeeId AS Id, CONCAT(EmployeeName, ' - ', EmployeeNo) AS Name FROM Mas_Employee WHERE Deactivate = 0 AND EmployeeLeft = 0 AND EmployeeBranchId = '" + EmployeeTransfer.BranchId + @"' AND CmpId = '" + EmployeeTransfer.CmpId + @"'");
                    var ListEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();          
                    ViewBag.GetEmployeeName = ListEmployeeName;


                    DynamicParameters ParamSG = new DynamicParameters();
                    ParamSG.Add("@query", @"select ShiftGroupId as Id,ShiftGroupFName as Name from Atten_ShiftGroups  where Deactivate=0  and ShiftGroupBranchId=  '" + EmployeeTransfer.ToBranchId + @"'");
                    var ListShiftGroup = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", ParamSG).ToList();
                    ViewBag.GetShiftGroupName = ListShiftGroup;


                    DynamicParameters ParamSR = new DynamicParameters();
                    ParamSR.Add("@query", @"select ShiftRuleId as Id,ShiftRuleName as Name from Atten_ShiftRule  where Deactivate=0  and ShiftRuleBranchId=  '" + EmployeeTransfer.ToBranchId + @"'");
                    var ListShiftRule = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", ParamSR).ToList();
                    ViewBag.GetShiftRuleName = ListShiftRule;


                }
                else
                {
                    ViewBag.GetBranchName = new List<AllDropDownBind>();
                    ViewBag.GetToBranchName = new List<AllDropDownBind>();
                    ViewBag.GetEmployeeName = new List<AllDropDownBind>();
                    ViewBag.GetShiftGroupName = new List<AllDropDownBind>();
                    ViewBag.GetShiftRuleName = new List<AllDropDownBind>();
                }
                TempData["TransferDate"] = EmployeeTransfer.TransferDate;

                return View(EmployeeTransfer);
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

                DynamicParameters paramBranchName = new DynamicParameters();
                paramBranchName.Add("@query", "select BranchId as Id, BranchName as Name  from Mas_Branch where Deactivate = 0 and CmpId ='" + CmpId + "'  order by Name");
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranchName).ToList();
                ViewBag.GetBranchName = Branch;


                DynamicParameters paramToBranchName = new DynamicParameters();
                paramToBranchName.Add("@query", "select BranchId as Id, BranchName as Name  from Mas_Branch where Deactivate = 0 and CmpId ='" + CmpId + "'  order by Name");
                var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramToBranchName).ToList();
                ViewBag.GetToBranchName = BranchName;


                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName, ' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate = 0 and EmployeeLeft=0 and Deactivate=0 AND CmpID='" + CmpId + "'  order by Name");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                ViewBag.GetEmployeeName = EmployeeName; 

                return Json(new { Branch = Branch, EmployeeName = EmployeeName }, JsonRequestBehavior.AllowGet);
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
        public ActionResult GetEmployeeName(int CmpId, int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                //DynamicParameters param = new DynamicParameters();
                //param.Add("@query", "select employeeid as Id,CONCAT(EmployeeName, ' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeeBranchId='" + BranchId + "'and CmpID='" + CmpId + "' and Employeeleft=0 and Deactivate=0 order by Name");
                //var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_CmpId", CmpId);
                param.Add("@p_BranchId", BranchId);
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBonusEmployeeName", param).ToList();

                ViewBag.GetEmployeeName = EmployeeName;

                return Json(new { EmployeeName = EmployeeName }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region GetCanteenApplicable

        [HttpGet]
        public ActionResult GetCanteenApplicable(int EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();

                //param.Add("@query", @"SELECT CASE  WHEN EM_Atten_CanteenApplicable IS NULL THEN 0 WHEN EM_Atten_CanteenApplicable = 0 THEN 0 WHEN EM_Atten_CanteenApplicable = 1 THEN 1  END AS IsCanteenApplicable  FROM Mas_Employee_Attendance WHERE Deactivate = 0  AND AttendanceEmployeeId = " + EmployeeId + " ");
                //var result = DapperORM.ReturnList<CanteenApplicableModel>("sp_QueryExcution", param) .FirstOrDefault();

                param.Add("@query", @"SELECT 
                CASE WHEN MEA.EM_Atten_CanteenApplicable = 1 THEN 1 ELSE 0 END AS IsCanteenApplicable,
                CASE WHEN MCTC.RatePartB_AttendanceBonus_IsApplicable = 1 THEN 1 ELSE 0 END AS IsAttenBonusApplicable,
                MCTC.DailyMonthly
                FROM Mas_Employee_Attendance MEA
                LEFT JOIN Mas_Employee_CTC MCTC 
                ON MCTC.CTCEmployeeId = MEA.AttendanceEmployeeId
                 WHERE MEA.Deactivate = 0 
                AND MEA.AttendanceEmployeeId = " + EmployeeId + @"
                AND MCTC.CTCEmployeeId = " + EmployeeId);

                var result = DapperORM.ReturnList<CanteenApplicableModel>("sp_QueryExcution", param).FirstOrDefault();


                return Json(new
                {
                    IsCanteenApplicable = result?.IsCanteenApplicable ?? false,
                    IsAttenBonusApplicable = result?.IsAttenBonusApplicable ??false,
                    DailyMonthly = result?.DailyMonthly 
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetShiftGroupOrRule
        [HttpGet]
        public ActionResult GetShiftGroupOrRule(int ToBranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();

                param.Add("@query", @"select ShiftGroupId as Id,ShiftGroupFName as Name from Atten_ShiftGroups  where Deactivate=0  and ShiftGroupBranchId='"+ ToBranchId + "' Order By Name");
                var ShiftGroup = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetShiftGroupName = ShiftGroup;


                param.Add("@query", @"select ShiftRuleId as Id,ShiftRuleName as Name from Atten_ShiftRule  where Deactivate=0  and ShiftRuleBranchId ='" + ToBranchId + "' Order By Name ");
                var ShiftRule = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetShiftRuleName = ShiftRule;

                return Json(new { ShiftGroup,ShiftRule }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region IsValidation
        public ActionResult IsExists(string EmployeeTransferId_Encrypted, int? CmpId, int? BranchId, int? EmployeeId)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_EmployeeTransferId_Encrypted", EmployeeTransferId_Encrypted);
                    param.Add("@p_CmpId", CmpId);
                    param.Add("@p_BranchId", BranchId);
                    param.Add("@p_EmployeeId", EmployeeId);
               
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_EmployeeTransfer_Premium", param);
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
        public ActionResult SaveUpdate(Employee_EmployeeTransfer_Premium obj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", string.IsNullOrEmpty(obj.EmployeeTransferId_Encrypted) ? "Save" : "Update");
                param.Add("@p_EmployeeTransferId_Encrypted", obj.EmployeeTransferId_Encrypted);
                param.Add("@p_CmpId", obj.CmpId);
                param.Add("@p_BranchId", obj.BranchId);
                param.Add("@p_EmployeeId", obj.EmployeeId);
                param.Add("@p_ToBranchId", obj.ToBranchId);
                param.Add("@p_TransferDate", obj.TransferDate);
                param.Add("@p_AttendanceBonusApplicable", obj.AttendanceBonusApplicable);
                param.Add("@p_CanteenApplicable", obj.CanteenApplicable);
                param.Add("@p_DailyMonthly", obj.DailyMonthly);
                param.Add("@p_ShiftGroup", obj.ShiftGroup);
                param.Add("@p_ShiftRule", obj.ShiftRule);

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);

                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_EmployeeTransfer_Premium", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Module_Employee_EmployeeTransfer_Premium", "Module_Employee_EmployeeTransfer_Premium");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 920;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_EmployeeTransferId_Encrypted", "List");
              
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_EmployeeTransfer_Premium", param).ToList();
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

        #region Delete
        [HttpGet]
        public ActionResult Delete(string EmployeeTransferId_Encrypted)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Delete");
                param.Add("@p_EmployeeTransferId_Encrypted", EmployeeTransferId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_EmployeeTransfer_Premium", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Module_Employee_EmployeeTransfer_Premium");
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