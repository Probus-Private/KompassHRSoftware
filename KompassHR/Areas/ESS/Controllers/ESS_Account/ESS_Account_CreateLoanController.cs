using Dapper;
using KompassHR.Areas.ESS.Models.ESS_LoanAndAdvance;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Account
{
    public class ESS_Account_CreateLoanController : Controller
    {
        DynamicParameters param = new DynamicParameters();

        // GET: ESS/ESS_Account_CreateLoan
        public ActionResult ESS_Account_CreateLoan(string LoanID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 761;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var GetDocNo = "Select isnull(Max(DocNo),0)+1 As DocNo from Payroll_Loan";
                    var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    ViewBag.DocNo = DocNo.DocNo;
                }
                var EmployeeId = Session["EmployeeId"];
                param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0  and EmployeeLeft=0 and  EmployeeBranchId=(select EmployeeBranchId from Mas_Employee where EmployeeId=" + EmployeeId + ")");
                var GuarantorName = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetEmployees = GuarantorName;

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0  and EmployeeLeft=0 and  EmployeeBranchId=(select EmployeeBranchId from Mas_Employee where EmployeeId=" + EmployeeId + ")");
                var EmployeeList = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", param1).ToList();
                ViewBag.EmployeeList = EmployeeList;

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@query", "select LoanTypeId as Id,LoanType as Name from Mas_Payroll_LoanType where deactivate=0");
                var GetLoanTypes = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", param2).ToList();
                ViewBag.GetLoanTypes = GetLoanTypes;
                Payroll_Loan Payroll_Loan = new Payroll_Loan();
                if(LoanID_Encrypted!=null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_LoanID_Encrypted", LoanID_Encrypted);
                    Payroll_Loan = DapperORM.ReturnList<Payroll_Loan>("sp_List_Payroll_Loan", param).FirstOrDefault();
                    ViewBag.DecuctionstartDate = Payroll_Loan.DeducationStartDate;
                    ViewBag.FormattedDocDate = Payroll_Loan.DocDate;
                    ViewBag.DocNo = Payroll_Loan.DocNo;
                }
                return View(Payroll_Loan);
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_LoanAndAdvance_Loan");

            }
        }
        
        public JsonResult IsLoanExists(int LoanEmployeeID, string LoanIDEncrypted)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {

                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_LoanEmployeeID", LoanEmployeeID);
                    param.Add("@p_LoanID_Encrypted", LoanIDEncrypted);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_Loan", param);
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
            catch (Exception Ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpPost]
        public ActionResult SaveUpdate(Payroll_Loan Loan)
        {
            try
            {
                var CompanyId = Session["CompanyId"];
                var EmployeeId = Session["EmployeeId"];

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", string.IsNullOrEmpty(Loan.LoanID_Encrypted) ? "Save" : "Update");
                param.Add("@p_LoanID", Loan.LoanID);
                param.Add("@p_LoanID_Encrypted", Loan.LoanID_Encrypted);
                param.Add("@p_CmpID", CompanyId);
                param.Add("@p_BranchID", Loan.BranchID);
                param.Add("@p_DocNo", Loan.DocNo);
                param.Add("@p_DocDate", Loan.DocDate);
                param.Add("@p_LoanTypeId", Loan.LoanTypeId);
                param.Add("@p_LoanEmployeeID", Loan.LoanEmployeeID);
                param.Add("@p_LoanAmount", Loan.LoanAmount);
                param.Add("@p_LoanInterest", Loan.LoanInterest);               
                param.Add("@p_InterestAmount", Loan.InterestAmount);         
                param.Add("@p_TotalLoanAmount", Loan.TotalLoanAmount);       
                param.Add("@p_NoOfInstallment", Loan.NoOfInstallment);
                param.Add("@p_MonthlyDeductionAmount", Loan.MonthlyDeductionAmount);   
                param.Add("@p_GuarantorName1", Loan.GuarantorName1);
                param.Add("@p_GuarantorName2", Loan.GuarantorName2);
                param.Add("@p_DeducationStartDate", Loan.DeducationStartDate);
                param.Add("@p_Reason", Loan.Reason);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                
                var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_Loan", param);

                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                return RedirectToAction("ESS_Account_CreateLoan", "ESS_Account_CreateLoan");
            }
            catch (Exception ex)
            {
                // Handle gracefully
                TempData["Message"] = "Error: " + ex.Message;
                TempData["Icon"] = "error";
                return RedirectToAction("ESS_Account_CreateLoan", "ESS_Account_CreateLoan");
            }
        }

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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 761;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_LoanID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Payroll_Loan", param);
                ViewBag.GetLoanList = data;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_LoanAndAdvance_Loan");
            }
        }
        #endregion

        #region Delete Loan
        public ActionResult Delete(string LoanID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_LoanID_Encrypted", LoanID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_Loan", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_Account_CreateLoan");
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Account_CreateLoan");
            }

        }
        #endregion

        public ActionResult GetLoanDetails(int LoanId)
        {
            try
            {
                param.Add("@p_LoanId", LoanId);
                var LoanDetails = DapperORM.ExecuteSP<dynamic>("sp_List_Payroll_Payroll_LoanEMI", param).ToList();
                return Json(new { success = true, data = LoanDetails }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception Ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

    }
}