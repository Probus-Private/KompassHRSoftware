using Dapper;
using KompassHR.Areas.Module.Models.Module_Payroll;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;

namespace KompassHR.Areas.Module.Controllers.Module_Payroll
{
    public class Module_Payroll_LeaveEncashmentController : Controller
    {
        clsCommonFunction objcon = new clsCommonFunction();
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region Main View
        public ActionResult Module_Payroll_LeaveEncashment(string LeaveEncashmentID_Encrypted)
        {
            try
            {
              
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null
                                ? Convert.ToInt32(Request.QueryString["ScreenId"])
                                : 159;

                bool CheckAccess = new BulkAccessClass().CheckAccess(
                                        screenId,
                                        Convert.ToInt32(Session["UserAccessPolicyId"])
                                   );

                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                Payroll_LeaveEncashment model = new Payroll_LeaveEncashment();

                if (!string.IsNullOrEmpty(LeaveEncashmentID_Encrypted))
                {
                    ViewBag.AddUpdateTitle = "Update";

                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_LeaveEncashmentID_Encrypted", LeaveEncashmentID_Encrypted);
                    model = DapperORM.ReturnList<Payroll_LeaveEncashment>("sp_List_Payroll_LeaveEncashment", param).FirstOrDefault();
                    TempData["FinancialYear"] = model.FinancialYear;

                }
                else
                {
                    ViewBag.AddUpdateTitle = "Add";
                }

                var cmp = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = cmp;

                if (model.CmpId > 0)
                {
                    DynamicParameters p1 = new DynamicParameters();
                    p1.Add("@query", $"select BranchId as Id, BranchName as Name from Mas_Branch where Deactivate=0 AND CmpId={model.CmpId}");
                    ViewBag.BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", p1).ToList();
                }
                else
                {
                    ViewBag.BranchName = new List<AllDropDownBind>();
                }

                if (model.CmpId > 0 && model.BranchId > 0)
                {
                    DynamicParameters p2 = new DynamicParameters();
                    p2.Add("@query",
                        $"select EmployeeId as Id, CONCAT(EmployeeName,' - ',EmployeeNo) as Name from Mas_Employee " +
                        $"where Deactivate=0 and EmployeeLeft=0 and CmpID={model.CmpId} and EmployeeBranchId={model.BranchId}");

                    ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", p2).ToList();
                }
                else
                {
                    ViewBag.EmployeeName = new List<AllDropDownBind>();
                }


                DynamicParameters fy = new DynamicParameters();
                fy.Add("@query", "Select TaxFyearId as Id,TaxYear As Name  from IncomeTax_Fyear where Deactivate=0 and [IsActive]=1 Order By Name");
                ViewBag.FinantialYear = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", fy).ToList();

                return View(model);
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
                ViewBag.BranchName = Branch;

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName, ' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate = 0 and EmployeeLeft=0 and Deactivate=0 AND CmpID='" + CmpId + "'  order by Name");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();


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
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select employeeid as Id,CONCAT(EmployeeName, ' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeeBranchId='" + BranchId + "'and CmpID='" + CmpId + "' and Employeeleft=0 and Deactivate=0 order by Name");
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
        public ActionResult IsExists(string LeaveEncashmentID_Encrypted, int? CmpId, int? BranchId, int? EmployeeId,string FinancialYear)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    DynamicParameters param = new DynamicParameters();
                   
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_LeaveEncashmentID_Encrypted", LeaveEncashmentID_Encrypted);

                    param.Add("@p_CmpID", CmpId);
                    param.Add("@p_BranchId", BranchId);
                    param.Add("@p_EmployeeId", EmployeeId);
                    param.Add("@p_FinancialYear", FinancialYear);

                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);

                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                    var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_LeaveEncashment", param);
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
        public ActionResult SaveUpdate(Payroll_LeaveEncashment obj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters param = new DynamicParameters();

                param.Add("@p_process", string.IsNullOrEmpty(obj.LeaveEncashmentID_Encrypted) ? "Save" : "Update");
                param.Add("@p_LeaveEncashmentID_Encrypted", obj.LeaveEncashmentID_Encrypted);

                param.Add("@p_CmpID", obj.CmpId);
                param.Add("@p_BranchId", obj.BranchId);
                param.Add("@p_EmployeeId", obj.EmployeeId);
                param.Add("@p_FinancialYear", obj.FinancialYear);

                param.Add("@p_Amount", obj.Amount);   // FIXED

                param.Add("@p_Remark", obj.Remark);
                param.Add("@p_MachineName", Dns.GetHostName());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);

                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                DapperORM.ExecuteReturn("sp_SUD_Payroll_LeaveEncashment", param);
            
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");


                return RedirectToAction("GetList", "Module_Payroll_LeaveEncashment");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 850;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_LeaveEncashmentID_Encrypted", "List");

                var data = DapperORM.ExecuteSP<dynamic>("[sp_List_Payroll_LeaveEncashment]", param).ToList();

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
        public ActionResult Delete(string LeaveEncashmentID_Encrypted)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Delete");
                param.Add("@p_LeaveEncashmentID_Encrypted", LeaveEncashmentID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_LeaveEncashment", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Module_Payroll_LeaveEncashment");
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