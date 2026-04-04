using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Tax;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Tax
{
    public class ESS_Tax_TaxComputationController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_Tax_TaxComputation

        #region ESS_Tax_TaxComputation
        public ActionResult ESS_Tax_TaxComputation(/*int? TaxFyearId*/IncomeTax_Fyear ObjIncomeTax_Fyear)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 515;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "Select TaxFyearId as Id,TaxYear As Name  from IncomeTax_Fyear where Deactivate=0 and [IsActive]=1");
                var GetTaxFyear = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param).ToList();
                ViewBag.GetTaxFyear = GetTaxFyear;

                param = new DynamicParameters();
                //  param.Add("@p_PersonalEmployeeId", Session["EmployeeId"]);
                param.Add("@query", "select EmployeeId, EmployeeNo, EmployeeName, DesignationName, DepartmentName, JoiningDate, AadhaarNo, PAN, CASE WHEN PANAadhaarLink = 1 THEN 'Yes' WHEN PANAadhaarLink = 0 THEN 'No' ELSE '' END AS PANAadhaarLink, DATEDIFF(YEAR, BirthdayDate, GETDATE()) -CASE WHEN(MONTH(BirthdayDate) > MONTH(GETDATE())) OR(MONTH(BirthdayDate) = MONTH(GETDATE()) AND DAY(BirthdayDate) > DAY(GETDATE())) THEN 1 ELSE 0 END AS age from Mas_Employee, Mas_Designation, Mas_Department, Mas_Employee_Personal where Mas_Employee.Deactivate = 0 and EmployeeId = '" + Session["EmployeeId"] + "' and Mas_Designation.DesignationId = Mas_Employee.EmployeeDesignationID and Mas_Department.DepartmentId = Mas_Employee.EmployeeDepartmentID and Mas_Employee_Personal.PersonalId = Mas_Employee.EmployeeId");
                var mas_employeePersonal = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).SingleOrDefault();
                ViewBag.GetEmployeePersonal = mas_employeePersonal;

                //if (TaxFyearId == null)
                //{
                //    TaxFyearId = 0;
                //}
                if(ObjIncomeTax_Fyear.TaxFyearId!=0)
                {

                    var GetFyearId = DapperORM.DynamicQueryList("Select count(IncomeTaxRuleId) as IncomeTaxRuleId from Incometax_rule where Deactivate=0 and IncomeTaxRule_FyearId=" + ObjIncomeTax_Fyear.TaxFyearId + "").FirstOrDefault();
                    var a = GetFyearId.IncomeTaxRuleId;
                    if(a==0)
                    {
                        TempData["Message"] = "Income tax rule/setting is not available for this financial year.";
                        TempData["Icon"] = "error";
                        param = new DynamicParameters();
                        param.Add("@P_EmployeeID", Session["EmployeeId"]);
                        param.Add("@P_FYearID", 0);
                        var data1 = DapperORM.ExecuteSP<dynamic>("sp_Tax_Computation", param).SingleOrDefault();
                        ViewBag.TaxComputation = data1;
                        return View();
                    }
                    else
                    {
                        TempData["Message"] = null;
                        TempData["Icon"] = null;
                    }
                    param = new DynamicParameters();
                    param.Add("@P_EmployeeID", Session["EmployeeId"]);
                    param.Add("@P_FYearID", ObjIncomeTax_Fyear.TaxFyearId);
                    var data = DapperORM.ExecuteSP<dynamic>("sp_Tax_Computation", param).SingleOrDefault();
                    ViewBag.TaxComputation = data;
                }
                else
                {
                    param = new DynamicParameters();
                    param.Add("@P_EmployeeID", Session["EmployeeId"]);
                    param.Add("@P_FYearID", 0);
                    var data = DapperORM.ExecuteSP<dynamic>("sp_Tax_Computation", param).SingleOrDefault();
                    ViewBag.TaxComputation = data;
                   // ViewBag.TaxComputation = "";
                }
               

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion ESS_Tax_TaxComputation




        #region GetHelp
        public ActionResult GetHelp(int TypeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_TypeId", TypeId);
                //var SetupFlag = DapperORM.DynamicList("sp_List_IncomeTax_Help", paramList);

                param.Add("@query", "Select Help_Description from IncomeTax_Help where TypeId='" + TypeId + "'");
                var Help = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).SingleOrDefault();

                return Json(Help, JsonRequestBehavior.AllowGet);
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