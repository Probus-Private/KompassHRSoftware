using Dapper;
using KompassHR.Areas.ESS.Models.ESS_LoanAndAdvance;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Account
{
    public class ESS_Account_LoanSettlementController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_Account_LoanSettlement
        public ActionResult ESS_Account_LoanSettlement(string LoanID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_LoanID_Encrypted", LoanID_Encrypted);
                var data = DapperORM.DynamicList("sp_List_Payroll_Loan", param);
                ViewBag.GetLoanList = data;

                var GetLoanId = "Select LoanID As LoanID from Payroll_Loan where deactivate=0 and LoanID_Encrypted='"+ LoanID_Encrypted + "'";
                var LoanId = DapperORM.DynamicQuerySingle(GetLoanId);

                var GetEmployeeId = "Select LoanEmployeeID  from Payroll_Loan where deactivate=0 and LoanID_Encrypted='" + LoanID_Encrypted + "'";
                var EmpId = DapperORM.DynamicQuerySingle(GetEmployeeId);

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_LoanId", LoanId?.LoanID);
                var Cal = DapperORM.DynamicList("sp_Payroll_LoanCalculation", param1);
                ViewBag.Calculations = Cal;
                var row = DapperORM.DynamicQuerySingle(@"
    SELECT MAX(MonthYear) AS LastPaidMonth
    FROM PAYROLL_LOANEMI
    WHERE EmployeeId = '" + EmpId?.LoanEmployeeID + @"'
      AND LoanId = '" + LoanId?.LoanID + @"'
      AND Status = 'Paid'
      AND Deactivate = 0
");

                DateTime? lastPaidMonth = null;

                if (row != null && row.LastPaidMonth != null)
                {
                    lastPaidMonth = (DateTime)row.LastPaidMonth;
                }

                ViewBag.LastPaidMonth = lastPaidMonth?.ToString("yyyy-MM");


                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
            
        }

        [HttpPost]
        public ActionResult ESS_Account_LoanSettlement(int LoanId,DateTime SettledDate,string Remarks)
        {
            try
            {
                var Status = "Settled";
                string updateEmiQuery = "UPDATE Payroll_LoanEMI SET Status = '"+ Status + "' WHERE LoanId = '"+ LoanId + "'";
                DapperORM.ExecuteQuery(updateEmiQuery);
                string updateLoanQuery = "UPDATE Payroll_Loan SET Status = '" + Status + "',SettledDate='"+ SettledDate + "',SettledRemark='" + Remarks + "' WHERE  LoanId = '" + LoanId + "'";
                DapperORM.ExecuteQuery(updateLoanQuery);
                return Json(new
                {
                    Message = "Loan Settled Successfully",
                    Icon = "success"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Message = "Error: " + ex.Message,
                    Icon = "error"
                }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}