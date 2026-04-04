using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Tax
{
    public class ESS_Tax_InvestmentMenuController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: ESS/ESS_Tax_InvestmentMenu
        #region ESS_Tax_InvestmentMenu
        public ActionResult ESS_Tax_InvestmentMenu(int? id, int? ScreenId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (ScreenId != null)
                {
                    Session["ModuleId"] = id;
                    Session["ScreenId"] = ScreenId;
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), id, ScreenId, "SubForm", "Transation");
                    ViewBag.GetUserMenuList = GetMenuList;
                }
                else
                {
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), Convert.ToInt32(Session["ModuleId"]), Convert.ToInt32(Session["ScreenId"]), "SubForm", "Transation");
                    ViewBag.GetUserMenuList = GetMenuList;
                }
                var IncomeTaxUS80C = DapperORM.DynamicQuerySingle("Select * from View_IncomeTax_Employee Where EmployeeID = " + Session["EmployeeId"] + "");
                if (IncomeTaxUS80C != null)
                {
                    ViewBag.GetUS80CEmployee = IncomeTaxUS80C;
                }
                var CheckSubmit = DapperORM.DynamicQueryList(@"Select top 1 [Month],[FromDay],[ToDay] from[IncomeTax_DocumentOpenClose] where Deactivate=0 and  [Month]=Month(Getdate()) and OpenCloseTypeID =1 and day(Getdate()) Between [FromDay] and ([ToDay])").FirstOrDefault();
                TempData["SubmitValid"] = CheckSubmit;
                return View();
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