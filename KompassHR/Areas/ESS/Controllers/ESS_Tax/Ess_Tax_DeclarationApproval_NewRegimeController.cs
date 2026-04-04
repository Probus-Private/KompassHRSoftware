using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;

using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.ESS.Models.ESS_Tax;
using KompassHR.Areas.Module.Models.Module_TaxDeclaration;
using System.Net;
using System.Data;

namespace KompassHR.Areas.ESS.Controllers.ESS_Tax
{
    public class Ess_Tax_DeclarationApproval_NewRegimeController : Controller
    {
        // GET: ESS/Ess_Tax_DeclarationApproval_NewRegime

        #region Ess_Tax_DeclarationApproval_NewRegime
        public ActionResult Ess_Tax_DeclarationApproval_NewRegime(int? TaxFyearId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 533;
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
                var Employeeid = Session["EmployeeId"];
                if (TaxFyearId == null)
                {
                    TaxFyearId = 0;
                    Employeeid = 0;
                }

                param = new DynamicParameters();
                param.Add("@P_FYearID", TaxFyearId);
                param.Add("@P_EmployeeID", Employeeid);
                var data = DapperORM.ExecuteSP<dynamic>("sp_Tax_Declaration_List_AR_NewRegime", param).ToList();
                ViewBag.DeclarationApproval = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion Ess_Tax_DeclarationApproval_NewRegime

        #region ApproveReject

        public ActionResult ApproveReject(List<InvestmentDeclaration_ApproveReject> ObjRecordList)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                for (int i = 0; i < ObjRecordList.Count; i++)
                {
                    DynamicParameters param = new DynamicParameters();

                    param.Add("@p_FYearId", ObjRecordList[i].InvestmentDeclarationFyearId);
                    param.Add("@p_InvestmentDeclarationId", ObjRecordList[i].InvestmentDeclarationId);
                    param.Add("@p_FYearId", ObjRecordList[i].InvestmentDeclarationFyearId);
                    param.Add("@p_EmployeeId", ObjRecordList[i].EmployeeId);
                    param.Add("@p_ActualStatus", ObjRecordList[i].ActualStatus);
                    param.Add("@p_Status", ObjRecordList[i].Status);
                    param.Add("@p_RejectRemark", ObjRecordList[i].RejectRemark);
                    param.Add("@p_ApprovedByEmployeeId", Session["EmployeeId"]);
                    param.Add("@p_ApprovedByEmployeeName", Session["EmployeeName"]);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());

                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_IncomeTax_NewRegimeApproval", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");


                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion ApproveReject


        #region GetList
        public ActionResult GetList(int? TaxFyearId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 533;
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
                param.Add("@p_FYearID", TaxFyearId);
                param.Add("@p_RegimeTypeTD", 2);
                var data = DapperORM.ExecuteSP<dynamic>("SP_List_IncomeTax_ApprovedRegimedeclartion", param).ToList();
                ViewBag.ApprovalList = data;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion GetList
    }
}