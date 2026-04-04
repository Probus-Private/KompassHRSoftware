using Dapper;
using KompassHR.Areas.ESS.Models.ESS_LoanAndAdvance;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_LoanAndAdvance
{
    public class ESS_LoanAndAdvance_AdvanceController : Controller
    {
        clsCommonFunction objcon = new clsCommonFunction();
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        // GET: ESS/ESS_LoanAndAdvance_Advance

        #region Advance Main View 
        public ActionResult ESS_LoanAndAdvance_Advance(string AdvanceID_Encrypted, string mode = "")
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 163;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Payroll_Advance AdvanceSetting = new Payroll_Advance();
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                ViewBag.GetCompanyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetBranchName = "";
                ViewBag.GetEmployeeName = "";
                //   ViewBag.BalanceAmount = 0;
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var GetDocNo = "Select isnull(Max(DocNo),0)+1 As DocNo from Payroll_Advance";
                    var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    ViewBag.DocNo = DocNo;
                }

                ViewBag.IsSettlement = false;
                if (AdvanceID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_AdvanceID_Encrypted", AdvanceID_Encrypted);
                    AdvanceSetting = DapperORM.ReturnList<Payroll_Advance>("sp_List_Payroll_Advance", param).FirstOrDefault();

                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {
                        var GetDocNo = "Select DocNo As DocNo from Payroll_Advance where AdvanceID_Encrypted='" + AdvanceID_Encrypted + "'";
                        var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                        ViewBag.DocNo = DocNo;
                    }

                    DynamicParameters paramBU = new DynamicParameters();
                    paramBU.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBU.Add("@p_CmpId", AdvanceSetting.CmpId);
                    ViewBag.GetBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBU).ToList();

                    DynamicParameters paramEmpName = new DynamicParameters();
                    paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Mas_Employee.Deactivate=0 and Mas_Employee.EmployeeBranchId= " + AdvanceSetting.BranchId + " and ContractorID=1 ORDER BY Name");
                    ViewBag.GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();

                    //var GetBalanceAmount = "SELECT ISNULL(MAX(Payroll_Advance.AdvanceAmount),0) - ISNULL(SUM(Payroll_Advance_Paid.PaidAmount),0) AS BalanceAmount FROM Payroll_Advance LEFT JOIN Payroll_Advance_Paid ON Payroll_Advance.AdvanceID = Payroll_Advance_Paid.Payroll_Advance_Id AND Payroll_Advance_Paid.Deactivate = 0 WHERE Payroll_Advance.Deactivate = 0 AND Payroll_Advance.AdvanceID_Encrypted = '" + AdvanceID_Encrypted + "'";
                    //var BalanceAmount = DapperORM.DynamicQuerySingle(GetBalanceAmount);
                    //AdvanceSetting.BalanceAmount = BalanceAmount.BalanceAmount;
                    //AdvanceSetting.SettlementAmount = BalanceAmount.BalanceAmount;

                    TempData["DocDate"] = AdvanceSetting.DocDate.ToString("yyyy-MM-dd");
                    TempData["FromMonthYear"] = AdvanceSetting.FromMonthYear.ToString("yyyy-MM");
                    TempData["ToMonthYear"] = AdvanceSetting.ToMonthYear.ToString("yyyy-MM");
                }

                if (AdvanceID_Encrypted != null && (mode == "settlement"))
                {
                    ViewBag.IsSettlement = true;

                    if (AdvanceID_Encrypted != null)
                    {
                        ViewBag.AddUpdateTitle = "AdvanceSettlment";
                        param = new DynamicParameters();
                        param.Add("@p_AdvanceID_Encrypted", AdvanceID_Encrypted);
                        AdvanceSetting = DapperORM.ReturnList<Payroll_Advance>("sp_List_Payroll_Advance", param).FirstOrDefault();

                        using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                        {
                            var GetDocNo = "Select DocNo As DocNo from Payroll_Advance where AdvanceID_Encrypted='" + AdvanceID_Encrypted + "'";
                            var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                            ViewBag.DocNo = DocNo;
                        }

                        DynamicParameters paramBU = new DynamicParameters();
                        paramBU.Add("@p_employeeid", Session["EmployeeId"]);
                        paramBU.Add("@p_CmpId", AdvanceSetting.CmpId);
                        ViewBag.GetBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBU).ToList();

                        DynamicParameters paramEmpName = new DynamicParameters();
                        paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Mas_Employee.Deactivate=0 and Mas_Employee.EmployeeBranchId= " + AdvanceSetting.BranchId + " and ContractorID=1 ORDER BY Name");
                        ViewBag.GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();

                        var GetBalanceAmount = "SELECT ISNULL(MAX(Payroll_Advance.AdvanceAmount),0) - ISNULL(SUM(Payroll_Advance_Paid.PaidAmount),0) AS BalanceAmount FROM Payroll_Advance LEFT JOIN Payroll_Advance_Paid ON Payroll_Advance.AdvanceID = Payroll_Advance_Paid.Payroll_Advance_Id AND Payroll_Advance_Paid.Deactivate = 0 WHERE Payroll_Advance.Deactivate = 0 AND Payroll_Advance.AdvanceID_Encrypted = '" + AdvanceID_Encrypted + "'";
                        var BalanceAmount = DapperORM.DynamicQuerySingle(GetBalanceAmount);
                        AdvanceSetting.BalanceAmount = BalanceAmount.BalanceAmount;
                        AdvanceSetting.SettlementAmount = BalanceAmount.BalanceAmount;

                        TempData["DocDate"] = AdvanceSetting.DocDate.ToString("yyyy-MM-dd");
                        TempData["FromMonthYear"] = AdvanceSetting.FromMonthYear.ToString("yyyy-MM");
                        TempData["ToMonthYear"] = AdvanceSetting.ToMonthYear.ToString("yyyy-MM");
                    }

                }

                return View(AdvanceSetting);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region IsVerification
        public JsonResult IsAdvanceExists(double AdvanceEmployeeID, DateTime DeducationMonth, string AdvanceID_Encrypted, int BranchId)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {

                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_AdvanceEmployeeID", AdvanceEmployeeID);
                    param.Add("@p_AdvanceID_Encrypted", AdvanceID_Encrypted);
                    param.Add("@p_BranchId", BranchId);
                    param.Add("@p_FromMonthYear", DeducationMonth.ToString("yyyy-MM-dd"));
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("[sp_SUD_Payroll_Advance]", param);
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

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Payroll_Advance Advance, string Mode)
        {
            try
            {

                param.Add("@p_process", string.IsNullOrEmpty(Advance.AdvanceID_Encrypted) ? "Save" : "Update");
                param.Add("@p_AdvanceID", Advance.AdvanceID);
                param.Add("@p_AdvanceID_Encrypted", Advance.AdvanceID_Encrypted);
                param.Add("@p_CmpID", Advance.CmpId);
                param.Add("@p_BranchId", Advance.BranchId);
                param.Add("@p_DocNo", Advance.DocNo);
                param.Add("@p_DocDate", Advance.DocDate);
                param.Add("@p_AdvanceEmployeeID", Advance.AdvanceEmployeeID);
                param.Add("@p_AdvanceAmount", Advance.AdvanceAmount);
                param.Add("@p_FromMonthYear", Advance.FromMonthYear);
                param.Add("@p_ToMonthYear", Advance.ToMonthYear);
                param.Add("@p_InstallmentAmount", Advance.InstallmentAmount);
                param.Add("@p_NoOfInstallment", Advance.NoOfInstallment);
                param.Add("@p_Remark", Advance.Remark);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                if (Mode == "AdvanceSettlment")
                {
                    param.Add("@p_BalanceAmount", Advance.BalanceAmount);
                    param.Add("@p_SettlementAmount", Advance.SettlementAmount);
                    param.Add("@p_AdvanceSettlementRemark", Advance.AdvanceSettlementRemark);
                    param.Add("@p_IsAdvanceSettlement", Advance.IsAdvanceSettlement);
                    param.Add("@p_SettlementDate", DateTime.Now);
                }
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_Advance", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                if (Mode == "AdvanceSettlment")
                {
                    StringBuilder strBuilder = new StringBuilder();

                    string qry = "";
                    var createdBy = Session["EmployeeName"]?.ToString().Replace("'", "''");
                    var machineName = Dns.GetHostName().Replace("'", "''");
                    qry = "INSERT INTO dbo.Payroll_Advance_Paid (" +
                        "Deactivate, CreatedBy, CreatedDate, MachineName, " +
                        "PaidEmployeeID, PaidAmount, MonthYear, Payroll_Advance_Id,IsAdvanceSettlement) " +
                        "VALUES (" +
                        "'0', " +
                        "'" + Session["EmployeeName"] + "', " +
                        "GETDATE(), " +
                        "'" + Dns.GetHostName().ToString() + "', " +
                        "'" + Advance.AdvanceEmployeeID + "', " +
                        "'" + Advance.SettlementAmount + "', " +
                        "CONVERT(date, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)), " +
                        "'" + Advance.AdvanceID + "', " +
                        "'" + Advance.IsAdvanceSettlement + "');";

                    strBuilder.Append(qry);

                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {
                        TempData["Message"] = "Record update successfully";
                        TempData["Icon"] = "success";
                    }
                }
                return RedirectToAction("ESS_LoanAndAdvance_Advance", "ESS_LoanAndAdvance_Advance");
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_LoanAndAdvance_Advance");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 163;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_AdvanceID_Encrypted", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var data = DapperORM.DynamicList("sp_List_Payroll_Advance", param);
                ViewBag.GetAdvanceList = data;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_LoanAndAdvance_Advance");
            }
        }
        #endregion

        #region Delete Loan
        public ActionResult Delete(string AdvanceID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_AdvanceID_Encrypted", AdvanceID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_Advance", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_LoanAndAdvance_Advance");
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_LoanAndAdvance_Advance");
            }

        }
        #endregion

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetEmployeeName
        [HttpGet]
        public ActionResult GetEmployeeName(int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Mas_Employee.Deactivate=0 and Mas_Employee.EmployeeBranchId= " + BranchId + " ORDER BY Name");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region   PayrollAdvance_Sidebar
        [HttpGet]
        public ActionResult PayrollAdvance_Sidebar(int? AdvanceID)
        {
            try
            {

                var WorkerEmployeeId = Session["WorkerOnboardEmployeeId"];
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@P_Qry", " and Payroll_Advance.AdvanceID=" + AdvanceID + "");
                var AdvanceEmployeeWise = DapperORM.DynamicList("sp_GetAdvanceEmployeeWise", paramList);
                //ViewBag.GetAdvanceEmployeeWise = AdvanceEmployeeWise;
                return Json(AdvanceEmployeeWise, JsonRequestBehavior.AllowGet);

                //return PartialView("_PayrollAdvancePartial");
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}