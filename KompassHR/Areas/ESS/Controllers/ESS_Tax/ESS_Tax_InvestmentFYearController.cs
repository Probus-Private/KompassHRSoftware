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
    public class ESS_Tax_InvestmentFYearController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_Tax_InvestmentFYear
        public ActionResult ESS_Tax_InvestmentFYear(int? id, int? ScreenId)
        {
            try
            {
                if (id != null)
                {
                    Session["ModuleId"] = id;
                    Session["ScreenId"] = ScreenId;
                }
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                //  var data = DapperORM.DynamicQuerySingle("");
                DynamicParameters paramBackDay = new DynamicParameters();
                paramBackDay.Add("@query", "Select TaxFyearId as Id,TaxYear As Name  from IncomeTax_Fyear where Deactivate=0 and [IsActive]=1");
                var GetBackDatedDay = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", paramBackDay).ToList();
                //   var data = DapperORM.ReturnList<IncomeTax_Fyear>("sp_GetTax_Fyear").ToList();
                ViewBag.GetTaxFyear = GetBackDatedDay;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        [HttpGet]
        public ActionResult SendTaxFyearIdAndYear(int TaxFyearId, string TaxYear)
        {
            try
            {
                Session["TaxFyearId"] = TaxFyearId;
                Session["TaxYear"] = TaxYear;
                SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
                var GetFyearId = DapperORM.DynamicQuerySingle("Select IncomeTaxRule_FyearId from Incometax_rule where Deactivate=0 and IncomeTaxRule_FyearId=" + TaxFyearId + "");
                //return Json( GetFyearId , JsonRequestBehavior.AllowGet);
                var ModuleId = Session["ModuleId"];
                var ScreenId = Session["ScreenId"];
                return Json(new { GetFyearId = GetFyearId, ModuleId = ModuleId, }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
    }
}
