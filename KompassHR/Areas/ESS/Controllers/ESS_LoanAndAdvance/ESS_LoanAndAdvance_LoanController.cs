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

namespace KompassHR.Areas.ESS.Controllers.ESS_LoanAndAdvance
{
    public class ESS_LoanAndAdvance_LoanController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_LoanAndAdvance_Loan

        #region Loan Main View
        public ActionResult ESS_LoanAndAdvance_Loan(string LoanID_Encrypted)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 162;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Payroll_Loan PayrollLoan = new Payroll_Loan();
                var Manager1 = Session["ManagerId1"];
                var Manger2 = Session["ManagerId2"];
                var HRId = Session["HRId"];
                var EmployeeId = Session["EmployeeId"];
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var GetDocNo = "Select isnull(Max(DocNo),0)+1 As DocNo from Payroll_Loan";
                    var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    ViewBag.DocNo = DocNo;
                }            

                param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0  and EmployeeLeft=0 and  EmployeeBranchId=(select EmployeeBranchId from Mas_Employee where EmployeeId=" + EmployeeId + ")");
                var GuarantorName = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetGuarantorName = GuarantorName;

                DynamicParameters EmployeeList = new DynamicParameters();
                EmployeeList.Add("@p_EmployeeID", Session["EmployeeId"]);
                EmployeeList.Add("@p_Barnchid", "");
                EmployeeList.Add("@p_Origin", "ESS");
                var listMas_Employee = DapperORM.ReturnList<AllDropDownBind>("sp_DropDown_Employee", EmployeeList);
                ViewBag.GetEmployeeName = listMas_Employee;

                param = new DynamicParameters();
                param.Add("@p_LoanID_Encrypted", "List");
                param.Add("@P_Qry", "and Status = 'Pending' and LoanEmployeeID='" + EmployeeId + "'");
                var Loan = DapperORM.ExecuteSP<dynamic>("sp_List_Payroll_Loan", param).ToList();
                ViewBag.Loan = Loan;

                DynamicParameters param1 = new DynamicParameters();
                var EmployeerReviweLastRecord = "select max(CreatedDate) As CreatedDate from Payroll_Loan where LoanEmployeeID="+ EmployeeId + " and Deactivate = 0";
                var LastRecored = DapperORM.DynamicQuerySingle(EmployeerReviweLastRecord);
                ViewBag.LastRecored = LastRecored.CreatedDate;

                if (LoanID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_LoanID_Encrypted", LoanID_Encrypted);
                    PayrollLoan = DapperORM.ReturnList<Payroll_Loan>("sp_List_Payroll_Loan", param).FirstOrDefault();

                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {
                        var GetDocNo = "Select DocNo As DocNo from Payroll_Loan where LoanID_Encrypted='" + LoanID_Encrypted + "'";
                        var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                        ViewBag.DocNo = DocNo;
                    }
                }
                TempData["DocDate"] = PayrollLoan.DocDate;
                //TempData["RequiredDate"] = PayrollLoan.RequiredDate;
                //TempData["DeducationMonth"] = PayrollLoan.DeducationMonth;
                return View(PayrollLoan);
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_LoanAndAdvance_Loan");
            }

        }
        #endregion

        #region IsVerification

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
                //return RedirectToAction(Ex.Message.ToString(), "Wage");
            }
        }

        #endregion

        #region Loan SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Payroll_Loan Loan)
        {
            try
            {
                var CompanyId = Session["CompanyId"];
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(Loan.LoanID_Encrypted) ? "Save" : "Update");
                param.Add("@p_LoanID", Loan.LoanID);
                param.Add("@p_LoanID_Encrypted", Loan.LoanID_Encrypted);
                param.Add("@p_CmpID", CompanyId);
                param.Add("@p_BranchID", Loan.BranchID);
                param.Add("@p_DocNo", Loan.DocNo);
                param.Add("@p_DocDate", Loan.DocDate);
                param.Add("@p_LoanEmployeeID", Loan.LoanEmployeeID);
                param.Add("@p_LoanAmount", Loan.LoanAmount);
                param.Add("@p_NoOfInstallment", Loan.NoOfInstallment);
                //param.Add("@p_RequiredDate", Loan.RequiredDate);
                param.Add("@p_GuarantorName1", Loan.GuarantorName1);
                param.Add("@p_GuarantorName2", Loan.GuarantorName2);
                //param.Add("@p_DeducationMonth", Loan.DeducationMonth);
                param.Add("@p_Reason", Loan.Reason);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_Loan", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_LoanAndAdvance_Loan", "ESS_LoanAndAdvance_Loan");
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_LoanAndAdvance_Loan");
            }

        }

        #endregion

        #region GetContactDetails
        [HttpGet]
        public ActionResult GetContactDetails(int EmployeeId, string LoanID_Encrypted)
        {
            try
            {
                param.Add("@p_Origin", "ESSLoan");
                param.Add("@p_DocId_Encrypted", LoanID_Encrypted);
                var GetLoanList = DapperORM.ExecuteSP<dynamic>("Sp_GetManager_Module", param).ToList(); // SP_getReportingManager
                var GetLoanLists = GetLoanList;
                return Json(new { data = GetLoanLists }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet); ;
            }
        }
        #endregion

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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 162;
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
                return RedirectToAction("ESS_LoanAndAdvance_Loan", "ESS_LoanAndAdvance_Loan");
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_LoanAndAdvance_Loan");
            }

        }
        #endregion
    }
}