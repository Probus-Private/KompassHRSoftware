using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Tax
{
    public class Ess_Tax_DeclarationApprovalController : Controller
    {
        // GET: ESS/Ess_Tax_DeclarationApproval

        DynamicParameters param = new DynamicParameters();

        public ActionResult Ess_Tax_DeclarationApproval(int? TaxFyearId,int? Type)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 524;
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
                param.Add("@P_FYearID", TaxFyearId);
                param.Add("@P_Type", Type);
                var data = DapperORM.ExecuteSP<dynamic>("sp_Tax_Declaration_Approval_Reject", param).ToList();
                ViewBag.DeclarationApproval = data;


                var dynamicList = ViewBag.DeclarationApproval as List<dynamic>;
                foreach (var Site in dynamicList)
                {
                    foreach (var property in (IDictionary<string, object>)Site)
                    {
                        var a = property.Key;
                    }
                }
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