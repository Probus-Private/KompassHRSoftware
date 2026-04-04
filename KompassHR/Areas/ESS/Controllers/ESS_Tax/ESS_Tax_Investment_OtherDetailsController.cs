using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Tax;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Tax
{
    public class ESS_Tax_Investment_OtherDetailsController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_Tax_Investment_OtherDetails
        #region OtherDetails Main View 
        [HttpGet]
        public ActionResult ESS_Tax_Investment_OtherDetails()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 181;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
                var GetEmpoyee = "Select * from View_IncomeTax_Employee Where EmployeeID = " + Session["EmployeeId"] + "";
                var Otherdetail = DapperORM.ExecuteQuery(GetEmpoyee);
                ViewBag.GetOtherDetails = Otherdetail;

                var Date = DapperORM.DynamicQueryList(" Select  OtherIncome_SubmitDate, OtherIncome_SubmitCount   from IncomeTax_InvestmentDeclaration where Deactivate = 0  and [InvestmentDeclarationFyearId] = " + Session["TaxFyearId"] + " and [InvestmentDeclarationEmployeeId] = " + Session["EmployeeId"] + "").FirstOrDefault();
                TempData["OtherIncome_SubmitDate"] = null;
                TempData["OtherIncome_SubmitCount"] = null;
                if (Date != null)
                {
                    TempData["OtherIncome_SubmitDate"] = Date.OtherIncome_SubmitDate;
                    TempData["OtherIncome_SubmitCount"] = Date.OtherIncome_SubmitCount;
                }

                //Check Submit button valid or not based on open close taxDeclaration beetween Date
                var CheckSubmit = DapperORM.ExecuteQuery(@"Select top 1 [Month],[FromDay],[ToDay] from[IncomeTax_DocumentOpenClose] where Deactivate=0 and  [Month]=Month(Getdate()) and OpenCloseTypeID =1 and day(Getdate()) Between [FromDay] and ([ToDay])");
                TempData["SubmitValid"] = CheckSubmit;

                param = new DynamicParameters();
                param.Add("@p_InvestmentDeclarationFyearId", Session["TaxFyearId"]);
                param.Add("@p_InvestmentDeclarationEmployeeId", Session["EmployeeId"]);
                InvestmentDeclaration_OtherDetails Investment_OtherDetails = new InvestmentDeclaration_OtherDetails();
                Investment_OtherDetails = DapperORM.ReturnList<InvestmentDeclaration_OtherDetails>("sp_List_IncomeTax_InvestmentDeclaration_OtherIncome", param).FirstOrDefault();
                if (Investment_OtherDetails != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                }

                return View(Investment_OtherDetails);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region SaveUpdate
        public ActionResult SaveUpdate(InvestmentDeclaration_OtherDetails InvestmentOtherDetail)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(InvestmentOtherDetail.InvestmentDeclaration_Encrypted) ? "Save" : "Update");
                param.Add("@p_InvestmentDeclarationFyearId", Session["TaxFyearId"]);
                param.Add("@P_InvestmentDeclarationEmployeeId", Session["EmployeeId"]);
                param.Add("@P_InvestmentDeclarationEmployeeNo", 1);
                param.Add("@P_InvestmentDeclarationEmployeeName", "Dhirendra");
                param.Add("@p_OtherIncome1", InvestmentOtherDetail.OtherIncome1);
                param.Add("@p_OtherIncome1_Remark", InvestmentOtherDetail.OtherIncome1_Remark);
                param.Add("@p_OtherIncome2", InvestmentOtherDetail.OtherIncome2);
                param.Add("@p_OtherIncome2_Remark", InvestmentOtherDetail.OtherIncome2_Remark);
                param.Add("@p_OtherIncome3", InvestmentOtherDetail.OtherIncome3);
                param.Add("@p_OtherIncome3_Remark", InvestmentOtherDetail.OtherIncome3_Remark);
                param.Add("@p_TotalOtherIncome", InvestmentOtherDetail.TotalOtherIncome);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_InvestmentDeclaration_OtherIncome", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;

                return RedirectToAction("ESS_Tax_Investment_OtherDetails", "ESS_Tax_Investment_OtherDetails");
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