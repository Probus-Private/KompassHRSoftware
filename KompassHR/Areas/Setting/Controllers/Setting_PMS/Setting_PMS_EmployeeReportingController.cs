using Dapper;
using KompassHR.Areas.ESS.Models.ESS_PMS;
using KompassHR.Areas.Setting.Models.Setting_PMS;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_PMS
{
    public class Setting_PMS_EmployeeReportingController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: Setting/Setting_PMS_EmployeeReporting
        public ActionResult Setting_PMS_EmployeeReporting(string PmsEmployeeReportingId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                    return RedirectToAction("Login", "Login", new { Area = "" });

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 833;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = PmsEmployeeReportingId_Encrypted == null ? "Add" : "Update";

                // 1️⃣ Employee dropdown
                DynamicParameters paramEmp = new DynamicParameters();
                paramEmp.Add("Query", "SELECT EmployeeId AS Id, CONCAT(EmployeeName,' - ',EmployeeNo) AS Name FROM Mas_Employee WHERE Deactivate=0 AND employeeleft=0 AND contractorid=1 AND employeeid<>1");
                ViewBag.getEmployeeID = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmp).ToList();

                // 2️⃣ Logged-in employee info (optional)
                int EmpId = Convert.ToInt32(Session["EmployeeId"]);
                DynamicParameters paramInfo = new DynamicParameters();
                paramInfo.Add("Query", $"SELECT * FROM Mas_Employee WHERE Deactivate=0 AND employeeleft=0 AND contractorid=1 AND employeeid={EmpId}");
                ViewBag.GetEmployeeInfo = DapperORM.ReturnList<dynamic>("sp_QueryExcution", paramInfo).FirstOrDefault();

                // 3️⃣ Edit mode: get approver data
                if (!string.IsNullOrEmpty(PmsEmployeeReportingId_Encrypted))
                {
                    DynamicParameters paramEdit = new DynamicParameters();
                    paramEdit.Add("@P_PmsEmployeeReportingId_Encrypted", PmsEmployeeReportingId_Encrypted);
                    var result = DapperORM.ReturnList<dynamic>("sp_List_PMS_EmployeeReporting", paramEdit).FirstOrDefault();
                    ViewBag.GetApprover = result;
                }

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult IsValidation(int EmployeeId ,string PmsEmployeeReportingId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_PmsEmployeeReportingId_Encrypted",PmsEmployeeReportingId_Encrypted);
                    param.Add("@p_EmployeeId", EmployeeId);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_PMS_EmployeeReporting", param);
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

        public ActionResult SaveUpdate(PMS_EmployeeReporting PMS_EmployeeReporting)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", string.IsNullOrEmpty(PMS_EmployeeReporting.PmsEmployeeReportingId_Encrypted) ? "Save" : "Update");
                param.Add("@p_PmsEmployeeReportingId_Encrypted", PMS_EmployeeReporting.PmsEmployeeReportingId_Encrypted);
                param.Add("@p_CmpId", Session["CompanyId"]);
                param.Add("@p_BranchId", Session["BranchId"]);
                param.Add("@p_EmployeeId", PMS_EmployeeReporting.EmployeeId);
                param.Add("@p_ReportingManagerId1", PMS_EmployeeReporting.ReportingManagerId1);
                param.Add("@p_ReportingManagerId2", PMS_EmployeeReporting.ReportingManagerId2);
                param.Add("@p_ReportingManagerId3", PMS_EmployeeReporting.ReportingManagerId3);
                param.Add("@p_ReportingManagerId4", PMS_EmployeeReporting.ReportingManagerId4);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_PMS_EmployeeReporting", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_PMS_EmployeeReporting", "Setting_PMS_EmployeeReporting");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult GetEmployeeDetails(int EmpId)
        {
            try
            {
                param.Add("Query", "select  EmployeeNo,CompanyName,BranchName,DepartmentName,DesignationName from Mas_Employee emp join Mas_Department dept on emp.EmployeeDepartmentID=dept.DepartmentId join Mas_Designation desg on emp.EmployeeDesignationID=desg.DesignationId join Mas_CompanyProfile cmp on emp.CmpID=cmp.CompanyId join Mas_Branch branch on emp.EmployeeBranchId=branch.BranchId where  emp.Deactivate=0 and employeeleft=0 and contractorid=1 and employeeid<>1 and emp.employeeid='" + EmpId + "'");
                var GetEmployeeInfo = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).FirstOrDefault();
                ViewBag.GetEmployeeInfo = GetEmployeeInfo;
                return Json(GetEmployeeInfo, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 833;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_PmsEmployeeReportingId_Encrypted", "List");
                var result = DapperORM.ReturnList<dynamic>("sp_List_PMS_EmployeeReporting", param).ToList();
                ViewBag.GetApproverList = result;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
    }
}