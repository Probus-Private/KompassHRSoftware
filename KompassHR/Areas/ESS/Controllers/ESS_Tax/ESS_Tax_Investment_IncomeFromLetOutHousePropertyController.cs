using Dapper;

using KompassHR.Areas.ESS.Models.ESS_Tax;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Tax
{
    public class ESS_Tax_Investment_IncomeFromLetOutHousePropertyController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_Tax_Investment_IncomeFromLetOutHouseProperty
        #region IncomeFromLetOutHouseProperty Main View 
        public ActionResult ESS_Tax_Investment_IncomeFromLetOutHouseProperty()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 179;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

                var StandardDeductionNetAnnualValue = DapperORM.DynamicQueryList("Select IncomefromLetOutHouseProperty_StandardEducation  From IncomeTax_Rule where  Deactivate=0 and IncomeTaxRule_FyearId= " + Session["TaxFyearId"] + "").FirstOrDefault();
                if (StandardDeductionNetAnnualValue != null)
                {
                    TempData["IncomefromLetOutHouseProperty_StandardEducation"] = StandardDeductionNetAnnualValue.IncomefromLetOutHouseProperty_StandardEducation;
                }
                else
                {
                    TempData["IncomefromLetOutHouseProperty_StandardEducation"] = "";

                }
                // var StandardDeductionNetAnnualValue = DapperORM.DynamicQuerySingle("Select Incomefromletouthouseproperty_Standarddeduction  From IncomeTax_Rule where  Deactivate=0 and IncomeTaxRuleFyearId= " + Session["TaxFyearId"] + " ");


                var GetEmpoyee = "Select * from View_IncomeTax_Employee Where EmployeeID = " + Session["EmployeeId"] + "";
                var IncomeTaxLetoutHouse = DapperORM.ExecuteQuery(GetEmpoyee);
                ViewBag.GetLetoutHouseEmployee = IncomeTaxLetoutHouse;

                var Date = DapperORM.DynamicQueryList(" Select IncomeFromLetOutHouseProperty_SubmitDate, IncomeFromLetOutHouseProperty_SubmitCount from IncomeTax_InvestmentDeclaration where Deactivate = 0  and [InvestmentDeclarationFyearId] = " + Session["TaxFyearId"] + " and [InvestmentDeclarationEmployeeId] = " + Session["EmployeeId"] + "").FirstOrDefault();
                TempData["IncomeFromLetOutHouseProperty_SubmitDate"] = null;
                TempData["IncomeFromLetOutHouseProperty_SubmitCount"] = null;
                if (Date != null)
                {
                    TempData["IncomeFromLetOutHouseProperty_SubmitDate"] = Date.IncomeFromLetOutHouseProperty_SubmitDate;
                    TempData["IncomeFromLetOutHouseProperty_SubmitCount"] = Date.IncomeFromLetOutHouseProperty_SubmitCount;
                }

                var GetTotal = "Select TotalIncomeFromLetOutHouseProperty from IncomeTax_InvestmentDeclaration Where InvestmentDeclarationEmployeeId= " + Session["EmployeeId"] + "";
                var SetTotal = DapperORM.DynamicQueryList(GetTotal).FirstOrDefault();
                if (SetTotal != null)
                {
                    TempData["TotalIncomeFromLetOutHouseProperty"] = SetTotal.TotalIncomeFromLetOutHouseProperty;
                }
                //Check Submit button valid or not based on open close taxDeclaration beetween Date
                var CheckSubmit = DapperORM.ExecuteQuery(@"Select top 1 [Month],[FromDay],[ToDay] from[IncomeTax_DocumentOpenClose] where Deactivate=0 and  [Month]=Month(Getdate()) and OpenCloseTypeID =1 and day(Getdate()) Between [FromDay] and ([ToDay])");
                TempData["SubmitValid"] = CheckSubmit;

                param = new DynamicParameters();
                param.Add("@p_IncomeFromLetOutHousePropertyFyearId", Session["TaxFyearId"]);
                param.Add("@p_IncomeFromLetOutHousePropertyEmployeeId", Session["EmployeeId"]);
                var LetOutHouseProperty = DapperORM.ReturnList<IncomeTax_InvestmentDeclaration_LetOutHouseProperty>("sp_List_IncomeTax_InvestmentDeclaration_LetOutHouseProperty", param).ToList();
                ViewBag.LetOutHousePropertybag = LetOutHouseProperty;

                if (LetOutHouseProperty.Count() >= 0)
                {
                    ViewBag.AddUpdateTitle = "Update";
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
        public ActionResult SaveUpdate(List<IncomeTax_InvestmentDeclaration_LetOutHouseProperty> Record, InvestmentDeclaration_TotalIncomeLetOutHouseProperty LetOutHouseProperty)
        {
            try
            {
                var LetOutHousePropertyEmployeeId = DapperORM.DynamicQuerySingle("select count(EmployeeId) as LockCount from IncomeTax_RegimeDeclaration where Deactivate=0 and   FYearId='" + Session["TaxFyearId"] + "' and EmployeeId= '" + Session["EmployeeId"] + "' and Status='Approved'");
                if (LetOutHousePropertyEmployeeId.LockCount < 0)
                {
                    TempData["Message"] = "Regime status: Approved , Can't Update";
                    TempData["Icon"] = "error";
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }


                double amount = 0;
                DapperORM.ExecuteQuery("Update IncomeTax_InvestmentDeclaration_LetOutHouseProperty set Deactivate=1 where IncomeFromLetOutHousePropertyFyearId='" + Session["TaxFyearId"] + "' and  IncomeFromLetOutHousePropertyEmployeeId='" + Session["EmployeeId"] + "'");
                for (var i = 0; i < Record.Count; i++)
                {
                    param.Add("@p_process", "Save");
                    param.Add("@p_IncomeFromLetOutHousePropertyFyearId", Session["TaxFyearId"]);
                    param.Add("@p_IncomeFromLetOutHousePropertyEmployeeId", Session["EmployeeId"]);
                    param.Add("@p_DescriptionOfProperty", Record[i].DescriptionOfProperty);
                    param.Add("@p_GrossAnnualValue", Record[i].GrossAnnualValue);
                    param.Add("@p_MunicipalTaxes", Record[i].MunicipalTaxes);
                    param.Add("@p_NetAnnualValue", Record[i].NetAnnualValue);
                    param.Add("@p_StandardDeduction", Record[i].StandardDeduction);
                    param.Add("@p_InterestOnBorrowedCapital", Record[i].InterestOnBorrowedCapital);
                    param.Add("@p_IncomeFromLetOutHouseProperty", Record[i].IncomeFromLetOutHouseProperty);

                    amount = amount + Convert.ToDouble(Record[i].IncomeFromLetOutHouseProperty);
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data1 = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_InvestmentDeclaration_LetOutHouseProperty", param);
                    var message1 = param.Get<string>("@p_msg");
                    var Icon1 = param.Get<string>("@p_Icon");
                    TempData["Message"] = message1;
                    TempData["Icon"] = Icon1;


                }
                param = new DynamicParameters();
                param.Add("@p_process", "Save");
                param.Add("@p_InvestmentDeclarationFyearId", Session["TaxFyearId"]);
                param.Add("@P_InvestmentDeclarationEmployeeId", Session["EmployeeId"]);
                //param.Add("@P_InvestmentDeclarationEmployeeNo", 1);
                //param.Add("@P_InvestmentDeclarationEmployeeName", "Dhirendra");
                param.Add("@p_TotalIncomeFromLetOutHouseProperty", amount);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_IncomeTax_InvestmentDeclaration_LetOutHousePropertyMaster]", param);
                //var message = param.Get<string>("@p_msg");
                //var Icon = param.Get<string>("@p_Icon");
                //TempData["Message"] = message;
                //TempData["Icon"] = Icon;


                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                // return RedirectToAction("IncomeFromLetOutHouseProperty", "IncomeFromLetOutHouseProperty",new {areas= "Investment"});
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