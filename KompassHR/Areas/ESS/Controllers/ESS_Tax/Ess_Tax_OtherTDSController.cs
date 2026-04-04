using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Tax;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Tax
{
    public class Ess_Tax_OtherTDSController : Controller
    {
        // GET: ESS/Ess_Tax_OtherTDS

        DynamicParameters param = new DynamicParameters();

        #region Ess_Tax_OtherTDS  
        public ActionResult Ess_Tax_OtherTDS(string OtherTDSId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 532;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "Select TaxFyearId as Id,TaxYear As Name  from IncomeTax_Fyear where Deactivate=0 and [IsActive]=1");
                var GetTaxFyear = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param).ToList();
                ViewBag.GetTaxFyear = GetTaxFyear;

                param = new DynamicParameters();
                param.Add("@query", "Select EmployeeId as Id, Concat (EmployeeName,'-',EmployeeNo) As Name  from Mas_Employee where Deactivate=0  and Employeeleft=0 and ContractorID=1 Order By EmployeeName");
                var GetEmployeeList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param).ToList();
                ViewBag.GetEmployeeList = GetEmployeeList;

                param = new DynamicParameters();
                param.Add("@query", "select OtherTypeId as Id, OtherTypeName as Name  from IncomeTax_OtherType where Deactivate=0");
                var GetOtherTypeList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param).ToList();
                ViewBag.GetOtherTypeList = GetOtherTypeList;

                IncomeTax_OtherTDS Incometax_OtherTDS = new IncomeTax_OtherTDS();
                if (OtherTDSId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_OtherTDSId_Encrypted", OtherTDSId_Encrypted);
                    Incometax_OtherTDS = DapperORM.ReturnList<IncomeTax_OtherTDS>("sp_List_IncomeTax_OtherTDS", param).FirstOrDefault();
                    TempData["MonthYear"] = Incometax_OtherTDS.MonthYear.ToString("yyyy-MM"); ;
                }
                return View(Incometax_OtherTDS);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion Ess_Tax_OtherTDS
       
        #region SaveUpdate      
        public ActionResult SaveUpdate(IncomeTax_OtherTDS OtherTDS)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(OtherTDS.OtherTDSId_Encrypted) ? "Save" : "Update");
                param.Add("@p_OtherTDSId_Encrypted", OtherTDS.OtherTDSId_Encrypted);
                param.Add("@p_OtherTDSFyearId", OtherTDS.OtherTDSFyearId);
                param.Add("@p_OtherTDSEmployeeId", OtherTDS.OtherTDSEmployeeId);
                param.Add("@p_OtherTypeId", OtherTDS.OtherTypeId);
                param.Add("@p_TotalAmount", OtherTDS.TotalAmount);
                param.Add("@p_TDSAmount", OtherTDS.TDSAmount);
                param.Add("@p_MonthYear", OtherTDS.MonthYear);
                param.Add("@p_Remark", OtherTDS.Remark);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_OtherTDS", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                return RedirectToAction("Ess_Tax_OtherTDS", "Ess_Tax_OtherTDS");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        
        #endregion SaveUpdate

        #region GetList
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 532;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_OtherTDSId_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_IncomeTax_OtherTDS", param);
                ViewBag.GetOtherTDSList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion GetList

        #region Delete
        public ActionResult Delete(string OtherTDSId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_OtherTDSId_Encrypted", OtherTDSId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_OtherTDS", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Ess_Tax_OtherTDS");
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