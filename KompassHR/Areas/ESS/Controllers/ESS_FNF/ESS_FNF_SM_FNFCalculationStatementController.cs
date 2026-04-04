using Dapper;
using KompassHR.Areas.ESS.Models.ESS_FNF;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_FNF
{
    public class ESS_FNF_SM_FNFCalculationStatementController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        #region ESS_FNF_SM_FNFCalculationStatement Main View 
        public ActionResult ESS_FNF_SM_FNFCalculationStatement(SM_FNFCalculations SM_FNFCalculations)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 827;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitleTitle = "Add";
                DynamicParameters paramEmployee = new DynamicParameters();
                paramEmployee.Add("@query", "Select EmployeeId as Id,EmployeeName + ' (' + Mas_Employee.EmployeeNo + ')' as Name from Mas_Employee where Deactivate = 0 and Mas_Employee.Deactivate = 0  and EmployeeId='" + SM_FNFCalculations.FNFCal_EmployeeId + "'");
                var GetEmployee = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", paramEmployee).ToList();
                ViewBag.AllEmployeeName = GetEmployee;

                var GetCmpID = "Select CmpID from Mas_Employee where Deactivate=0 and EmployeeId='" + SM_FNFCalculations.FNFCal_EmployeeId + "'";
                var CmpID = DapperORM.DynamicQuerySingle(GetCmpID);

                DynamicParameters paramLeaveYear = new DynamicParameters();
                paramLeaveYear.Add("@query", "select LeaveYearID as Id, cast(year(FromDate) as nvarchar(4))+'-'+cast(YEAR(ToDate) as nvarchar(4)) as Name from[dbo].[Leave_Year] where Deactivate = 0  and IsActivate=1  and CmpId='" + CmpID.CmpID + "' order by IsDefault desc,FromDate desc");
                var LeaveYearGet = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLeaveYear).ToList();
                ViewBag.GetLeaveYear = LeaveYearGet;

                DynamicParameters paramCY = new DynamicParameters();
                var BonusYear = DapperORM.ReturnList<AllDropDownBind>("sp_FNF_GetBonusYearDropdown", paramCY).ToList();
                ViewBag.GetBonusYear = BonusYear;

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@P_EmployeeId", SM_FNFCalculations.FNFCal_EmployeeId);
                var info = DapperORM.ExecuteSP<dynamic>("sp_FNF_GetDetail_Show", param1).FirstOrDefault();
                ViewBag.EmployeeDetails = info;

                ViewBag.AddUpdateTitle = "Add";
                if (SM_FNFCalculations.FNFCal_EmployeeId != 0)
                {
                    param.Add("@p_EmployeeId", SM_FNFCalculations.FNFCal_EmployeeId);
                    var data = DapperORM.ReturnList<dynamic>("sp_FNF_FNFEmployee", param).FirstOrDefault();
                    TempData["DOJ"] = data.DOJ;
                    TempData["ResignationDate"] = data.ResignationDate;
                    TempData["LastWorkingDate"] = data.LastWorkingDate;
                    ViewBag.GetData = data;
                }
                else
                {
                    ViewBag.GetData = ""; ;
                }
                if (SM_FNFCalculations.FNFCald != 0)
                {
                    ViewBag.AddUpdateTitleTitle = "Update";
                    DynamicParameters paramupdate = new DynamicParameters();
                    paramupdate.Add("@p_Process", "Update");
                    paramupdate.Add("@P_FNFCald", SM_FNFCalculations.FNFCald);
                    SM_FNFCalculations = DapperORM.ReturnList<SM_FNFCalculations>("sp_FNF_List_FNFCalculation", paramupdate).FirstOrDefault();
                }
                return View(SM_FNFCalculations);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsSM_FNFCalculationStatementExits
        public ActionResult IsSM_FNFCalculationStatementExits(string FnfCalId_Encrypted, double FNFCal_EmployeeId)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@P_FnfCalId_Encrypted", FnfCalId_Encrypted);
                    param.Add("@p_FNFCal_EmployeeId", FNFCal_EmployeeId);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_SM_FNFCalculationStatement", param);
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
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(SM_FNFCalculations SM_FNFCalculations)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(SM_FNFCalculations.FNFCal_Encrypted) ? "Save" : "Update");
                param.Add("@p_FNFCal_Encrypted", SM_FNFCalculations.FNFCal_Encrypted);
                param.Add("@p_FNFCald", SM_FNFCalculations.FNFCald);
                param.Add("@p_FNFEmployeeId", SM_FNFCalculations.FNFCal_EmployeeId);
                param.Add("@p_FNFCmpID", SM_FNFCalculations.CmpID);
                param.Add("@p_FNFBranchID", SM_FNFCalculations.BranchID);
                param.Add("@p_FNFDocNo", SM_FNFCalculations.DocNo);
                param.Add("@p_FNFDate", SM_FNFCalculations.FNFDate);   // REQUIRED
                param.Add("@p_EmployeeName", SM_FNFCalculations.EmployeeName);
                param.Add("@p_EmployeeNo", SM_FNFCalculations.EmployeeNo);
                param.Add("@p_Department", SM_FNFCalculations.Department);
                param.Add("@p_Desination", SM_FNFCalculations.Desination);
                param.Add("@p_Grade", SM_FNFCalculations.Grade);
                param.Add("@p_BusinessUnit", SM_FNFCalculations.BusinessUnit);

                param.Add("@p_JoiningDate", SM_FNFCalculations.JoiningDate);
                param.Add("@p_ReginationDate", SM_FNFCalculations.ReginationDate);
                param.Add("@p_LastWorkingDate", SM_FNFCalculations.LastWorkingDate);
                param.Add("@p_RelievingDate", SM_FNFCalculations.RelievingDate);
                param.Add("@p_NoticePeriodEndDate", SM_FNFCalculations.NoticePeriodEndDate);

                param.Add("@p_DailyMonthly", SM_FNFCalculations.DailyMonthly);
                param.Add("@p_Bond_Applicable", SM_FNFCalculations.Bond_Applicable);
                param.Add("@p_BondEndDate", SM_FNFCalculations.BondEndDate);
                param.Add("@p_BondAmt", SM_FNFCalculations.BondAmt);
                param.Add("@p_BondRemark", SM_FNFCalculations.BondRemark);

                param.Add("@P_A_Salary_Applicable", SM_FNFCalculations.A_Salary_Applicable);
                param.Add("@P_A_SalaryDate", SM_FNFCalculations.A_SalaryDate);
                param.Add("@P_B_Salary_Applicable", SM_FNFCalculations.B_Salary_Applicable);
                param.Add("@P_B_SalaryDate", SM_FNFCalculations.B_SalaryDate);
                param.Add("@P_C_Salary_Applicable", SM_FNFCalculations.C_Salary_Applicable);
                param.Add("@P_C_SalaryDate", SM_FNFCalculations.C_SalaryDate);
                param.Add("@P_FY_Preivous_Bonus_Year_Applicable", SM_FNFCalculations.FY_Preivous_Bonus_Year_Applicable);
                param.Add("@P_FY_Preivous_Bonus_Year", SM_FNFCalculations.FY_Preivous_Bonus_Year);
                param.Add("@P_FY_Current_Bonus_Year_Applicable", SM_FNFCalculations.FY_Current_Bonus_Year_Applicable);
                param.Add("@P_FY_Current_Bonus_Year", SM_FNFCalculations.FY_Current_Bonus_Year);
                param.Add("@P_LeaveYear_Applicable", SM_FNFCalculations.LeaveYear_Applicable);
                param.Add("@P_LeaveMasterLeaveYearId", SM_FNFCalculations.LeaveMasterLeaveYearId);

                param.Add("@p_GrossSalary", SM_FNFCalculations.GrossSalary);
                param.Add("@p_BasicSalary", SM_FNFCalculations.BasicSalary);

                param.Add("@p_UnpaidSalaryA_Date", SM_FNFCalculations.UnpaidSalaryA_Date);
                param.Add("@p_UnpaidSalaryA_Amount", SM_FNFCalculations.UnpaidSalaryA_Amount);
                param.Add("@p_UnpaidSalaryB_Date", SM_FNFCalculations.UnpaidSalaryB_Date);
                param.Add("@p_UnpaidSalaryB_Amount", SM_FNFCalculations.UnpaidSalaryB_Amount);
                param.Add("@p_UnpaidSalaryC_Date", SM_FNFCalculations.UnpaidSalaryC_Date);
                param.Add("@p_UnpaidSalaryC_Amount", SM_FNFCalculations.UnpaidSalaryC_Amount);
                param.Add("@p_TotalUnpaidSalaryAmount", SM_FNFCalculations.TotalUnpaidSalaryAmount);

                param.Add("@p_PreivousBonus_Remark", SM_FNFCalculations.PreivousBonus_Remark);
                param.Add("@p_PreivousBonus_Amount", SM_FNFCalculations.BonusPreviousYearAmount);
                param.Add("@p_CurrentBonus_Remark", SM_FNFCalculations.CurrentBonus_Remark);
                param.Add("@p_CurrentBonus_Amount", SM_FNFCalculations.BonusCurrentYearAmount);
                param.Add("@p_TotalBonus_Amount", SM_FNFCalculations.TotalBonus_Amount);

                param.Add("@p_OtherEarningA_Remark", SM_FNFCalculations.OtherEarningA_Remark);
                param.Add("@p_OtherEarningA_Amount", SM_FNFCalculations.OtherEarningA_Amount);

                param.Add("@p_OtherEarningB_Remark", SM_FNFCalculations.OtherEarningB_Remark);
                param.Add("@p_OtherEarningB_Amount", SM_FNFCalculations.OtherEarningB_Amount);
                param.Add("@p_TotalOtherEarning", SM_FNFCalculations.TotalOtherEarning);

                param.Add("@p_PreivousLeave_Remark", SM_FNFCalculations.LeavePreviousYear);
                param.Add("@p_PreivousLeave_Days", SM_FNFCalculations.PreivousLeave_Days);
                param.Add("@p_PreivousLeave_Amount", SM_FNFCalculations.LeavePreviousYearAmount);

                param.Add("@p_CurrentLeave_Remark", SM_FNFCalculations.LeaveCurrentYear);
                param.Add("@p_CurrentLeave_Days", SM_FNFCalculations.CurrentLeave_Days);
                param.Add("@p_Current_Amount", SM_FNFCalculations.LeaveCurrentYearAmount);
                param.Add("@p_TotalLeave_Amount", SM_FNFCalculations.TotalLeaveEncashmentAmount);
                param.Add("@p_TotalEarning", SM_FNFCalculations.TotalAmountPayable);

                param.Add("@p_OtherDeductionA_Remark", SM_FNFCalculations.OtherDeductionA_Remark);
                param.Add("@p_OtherDeductionA_Amount", SM_FNFCalculations.OtherDeductionA_Amount);

                param.Add("@p_OtherDeductionB_Remark", SM_FNFCalculations.OtherDeductionB_Remark);
                param.Add("@p_OtherDeductionB_Amount", SM_FNFCalculations.OtherDeductionB_Amount);

                param.Add("@p_Advance", SM_FNFCalculations.AdvanceRecovered);
                param.Add("@p_TDS", SM_FNFCalculations.SalaryTDS);

                param.Add("@p_ShortFallNoticePay_Days", SM_FNFCalculations.NoticePeriodDays);
                param.Add("@p_ShortFallNoticePay_Amount", SM_FNFCalculations.NoticePeriodDayAmount);

                param.Add("@p_ServicesBond", SM_FNFCalculations.MinServiceBondDeduction);
                param.Add("@p_NoticePeriodRemark", SM_FNFCalculations.NoticePeriodRemark);
                param.Add("@p_OtherDeduction", SM_FNFCalculations.OtherAmount);
                param.Add("@p_TotalDeduction", SM_FNFCalculations.TotalAmountDeduction);
                param.Add("@p_FinalPay", SM_FNFCalculations.NetAmountPayable);

                param.Add("@p_Gratuity_Applicable", SM_FNFCalculations.Gratuity_Applicable);
                param.Add("@p_Gratuity_Service", SM_FNFCalculations.Gratuity_Service);

                param.Add("@p_FNFMakerId", Session["EmployeeId"]);
                param.Add("@p_FNFMakerRemark", SM_FNFCalculations.Remark);
                param.Add("@p_ResignationId", SM_FNFCalculations.ResignationId);
                param.Add("@p_MachineName", Dns.GetHostName());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 10);

                var Result = DapperORM.ExecuteReturn("sp_SUD_FNF_FNFCalculationStatement", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_FNF_SM_FNFCalculationStatement");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 827;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                DynamicParameters paramFNFMakerCheckerAccount = new DynamicParameters();
                paramFNFMakerCheckerAccount.Add("@P_Employeeid", Session["EmployeeId"]);
                var GetFNFMakerCheckerAccount = DapperORM.ExecuteSP<dynamic>("sp_FNF_Get_FNFMakerCheckerAccount", paramFNFMakerCheckerAccount).FirstOrDefault();
                if (GetFNFMakerCheckerAccount != null)
                {
                    ViewBag.FNFMakerId = GetFNFMakerCheckerAccount.FNFMakerId;
                    ViewBag.FNFCheckerId = GetFNFMakerCheckerAccount.FNFCheckerId;
                    ViewBag.AccountId = GetFNFMakerCheckerAccount.AccountId;
                }
                else
                {
                    ViewBag.FNFMakerId = "";
                    ViewBag.FNFCheckerId = "";
                    ViewBag.AccountId = "";
                }

                var GetRejectCount = "Select count(FNFCald) RejectCount from FNF_Calculation where Deactivate=0 and EmployeeCheckerStatus='Rejected'";
                var FNFRejectCount = DapperORM.DynamicQuerySingle(GetRejectCount);
                if (FNFRejectCount != null)
                {
                    TempData["FNFRejectCount"] = FNFRejectCount.RejectCount;
                }
                else
                {
                    TempData["FNFRejectCount"] = "0";
                }
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@P_EmployeeID", Session["EmployeeId"]);
                var FNFPendingList = DapperORM.ReturnList<dynamic>("sp_FNF_FNFCalulation_Pending", param1).ToList();
                ViewBag.GetFNFPendingList = FNFPendingList;

                var GetFNFPendingCount = "  SELECT Count(FNFId)  as TotalCount FROM  dbo.FNF_EmployeeResignation LEFT JOIN dbo.FNF_Calculation ON  dbo.FNF_Calculation.ResignationId = FNF_EmployeeResignation.FNFId  WHERE dbo.FNF_EmployeeResignation.Deactivate = 0 AND dbo.FNF_EmployeeResignation.IsRevoke = '0' AND FNF_EmployeeResignation.IsCalculated = 1 AND isnull(FNF_Calculation.IsFNFCalulation, 0) = 0 AND FNF_EmployeeResignation.EmployeeMakerId = '" + Session["EmployeeId"] + "'";
                var FNFPendingCount = DapperORM.DynamicQuerySingle(GetFNFPendingCount);
                if (FNFPendingCount != null)
                {
                    TempData["FNFPendingCount"] = FNFPendingCount.TotalCount;
                }
                else
                {
                    TempData["FNFPendingCount"] = "0";
                }

                DynamicParameters paramFNFCalculation = new DynamicParameters();
                paramFNFCalculation.Add("@p_Process", "List");
                paramFNFCalculation.Add("@P_EmployeeId", Session["EmployeeId"]);
                var FNFCompleteList = DapperORM.ReturnList<dynamic>("sp_FNF_List_FNFCalculation", paramFNFCalculation).ToList();
                ViewBag.GetFNFCompleteList = FNFCompleteList;

                var GetFNFCompleteCount = FNFCompleteList?.Count() ?? 0;
                TempData["FNFCompleteCount"] = GetFNFCompleteCount;
                //var FNFCompleteCount = DapperORM.DynamicQuerySingle(GetFNFCompleteCount);
                //if (FNFCompleteCount != null)
                //{
                //    TempData["FNFCompleteCount"] = FNFCompleteCount.TotalCount;
                //}
                //else
                //{
                //    TempData["FNFCompleteCount"] = "0";
                //}

                DynamicParameters paramCheckerFNF = new DynamicParameters();
                paramCheckerFNF.Add("@p_Process", "Checker");
                paramCheckerFNF.Add("@P_EmployeeID", Session["EmployeeId"]);
                var CheckerFNFPendingList = DapperORM.ReturnList<dynamic>("sp_FNF_List_FNFForChckerAccountApproval", paramCheckerFNF).ToList();
                ViewBag.GetCheckerFNFPendingList = CheckerFNFPendingList;

                var GetCheckerFNFPendingCount = "Select count(FNFCald) as TotalCount from FNF_Calculation inner join FNF_EmployeeResignation on FNF_EmployeeResignation.FNFId = FNF_Calculation.ResignationId where FNF_Calculation.Deactivate = 0 and IsSendForApproval = 1 and FNF_Calculation.EmployeeCheckerStatus = 'Pending' and FNF_EmployeeResignation.EmployeeCheckerId ='" + Session["EmployeeId"] + "'";
                var CheckerFNFPendingCount = DapperORM.DynamicQuerySingle(GetCheckerFNFPendingCount);
                if (CheckerFNFPendingCount != null)
                {
                    TempData["CheckerFNFPendingCount"] = CheckerFNFPendingCount.TotalCount;
                }
                else
                {
                    TempData["CheckerFNFPendingCount"] = "0";
                }

                DynamicParameters paramAccountFNF = new DynamicParameters();
                paramAccountFNF.Add("@p_Process", "Account");
                paramAccountFNF.Add("@P_EmployeeID", Session["EmployeeId"]);
                var AccountFNFPendingList = DapperORM.ReturnList<dynamic>("sp_FNF_List_FNFForChckerAccountApproval", paramAccountFNF).ToList();
                ViewBag.GetAccountFNFPendingList = AccountFNFPendingList;

                var GetAccountFNFPendingCount = "select Count(FNFCald) totalcount from FNF_Calculation inner join FNF_EmployeeResignation on FNF_EmployeeResignation.FNFId=FNF_Calculation.ResignationId where  FNF_Calculation.Deactivate=0 and  IsSendForApproval=1 and EmployeeCheckerStatus not in ('Pending','Rejected') and EmployeeAccountStatus = 'Pending' and FNF_EmployeeResignation.EmployeeAccountId='" + Session["EmployeeId"] + "'";
                var AccountFNFPendingCount = DapperORM.DynamicQuerySingle(GetAccountFNFPendingCount);
                if (AccountFNFPendingCount != null)
                {
                    TempData["AccountFNFPendingCount"] = AccountFNFPendingCount.totalcount;
                }
                else
                {
                    TempData["AccountFNFPendingCount"] = "0";
                }

                if (GetFNFMakerCheckerAccount != null && Convert.ToInt32(GetFNFMakerCheckerAccount.FNFMakerId) == Convert.ToInt32(Session["EmployeeId"]))
                {
                    DynamicParameters parammaker = new DynamicParameters();
                    parammaker.Add("@p_Process", "ApproveRejectList_ForMaker");
                    parammaker.Add("@P_EmployeeID", Session["EmployeeId"]);
                    var CheckerAccountApproveRejectList = DapperORM.ReturnList<dynamic>("sp_FNF_List_FNFForChckerAccountApproval", parammaker).ToList();
                    ViewBag.GetCheckerAccountApproveRejectList = CheckerAccountApproveRejectList;

                    var GetMaker_checker_Account_ApproveRejectcount = "select count(FNFCald) as totalCount from FNF_Calculation where FNF_Calculation.Deactivate=0 and IsSendForApproval=1 and FNF_Calculation.EmployeeMakerId='" + Session["EmployeeId"] + "'";
                    var Maker_checker_Account_ApproveRejectcount = DapperORM.DynamicQuerySingle(GetMaker_checker_Account_ApproveRejectcount);
                    if (Maker_checker_Account_ApproveRejectcount != null)
                    {
                        TempData["Maker_checker_Account_ApproveRejectcount"] = Maker_checker_Account_ApproveRejectcount.totalCount;
                    }
                    else
                    {
                        TempData["Maker_checker_Account_ApproveRejectcount"] = "0";
                    }
                }
                else
                {
                    DynamicParameters paramAccount = new DynamicParameters();
                    paramAccount.Add("@p_Process", "ApproveRejectList");
                    paramAccount.Add("@P_EmployeeID", Session["EmployeeId"]);
                    var CheckerAccountApproveRejectList = DapperORM.ReturnList<dynamic>("sp_FNF_List_FNFForChckerAccountApproval", paramAccount).ToList();
                    ViewBag.GetCheckerAccountApproveRejectList = CheckerAccountApproveRejectList;

                    if (GetFNFMakerCheckerAccount != null && Convert.ToInt32(GetFNFMakerCheckerAccount.FNFCheckerId) == Convert.ToInt32(Session["EmployeeId"]))
                    {
                        var GetMaker_checker_Account_ApproveRejectcount = "select Count(FNFCald) Totalcount from FNF_Calculation where  Deactivate=0  and EmployeeCheckerStatus != 'Pending' and EmployeeCheckerId='" + Session["EmployeeId"] + "'";
                        var Maker_checker_Account_ApproveRejectcount = DapperORM.DynamicQuerySingle(GetMaker_checker_Account_ApproveRejectcount);
                        if (Maker_checker_Account_ApproveRejectcount != null)
                        {
                            TempData["Maker_checker_Account_ApproveRejectcount"] = Maker_checker_Account_ApproveRejectcount.Totalcount;
                        }
                        else
                        {
                            TempData["Maker_checker_Account_ApproveRejectcount"] = "0";
                        }
                    }
                    if (GetFNFMakerCheckerAccount != null && Convert.ToInt32(GetFNFMakerCheckerAccount.AccountId) == Convert.ToInt32(Session["EmployeeId"]))
                    {
                        var GetMaker_checker_Account_ApproveRejectcount = "select Count(FNFCald) Totalcount from FNF_Calculation where  Deactivate=0  and EmployeeAccountStatus != 'Pending' and EmployeeAccountId='" + Session["EmployeeId"] + "'";
                        var Maker_checker_Account_ApproveRejectcount = DapperORM.DynamicQuerySingle(GetMaker_checker_Account_ApproveRejectcount);
                        if (Maker_checker_Account_ApproveRejectcount != null)
                        {
                            TempData["Maker_checker_Account_ApproveRejectcount"] = Maker_checker_Account_ApproveRejectcount.Totalcount;
                        }
                        else
                        {
                            TempData["Maker_checker_Account_ApproveRejectcount"] = "0";
                        }
                    }

                }
                DynamicParameters NPTdata = new DynamicParameters();
                NPTdata.Add("@query", "Select Id, MonthDays as Name from tbl_NoticePeriodType_MonthDays");
                var GetNPTdata = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", NPTdata);
                ViewBag.NoticePeriodMonthDays = GetNPTdata;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region Delete
        public ActionResult Delete(double? FNFCald)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_FNFCald", FNFCald);
                param.Add("@p_MachineName", Dns.GetHostName());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_FNF_FNFCalculationStatement", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("GetList", "ESS_FNF_SM_FNFCalculationStatement");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region PreViewStatement
        public ActionResult PreViewStatement(double? FNFCal_EmployeeId, int ResignationId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var GetCompanyName = "select Mas_CompanyProfile.CompanyName from Mas_Employee inner join Mas_CompanyProfile on Mas_CompanyProfile.CompanyId = Mas_Employee.CmpID where Mas_CompanyProfile.Deactivate=0 and Mas_Employee.EmployeeId='" + FNFCal_EmployeeId + "'";
                var CompanyName = DapperORM.DynamicQuerySingle(GetCompanyName);
                ViewBag.CompanyName = CompanyName.CompanyName;

                SM_FNFCalculations SM_FNFCalculations = new SM_FNFCalculations();
                DynamicParameters paramFNFCalculation = new DynamicParameters();
                paramFNFCalculation.Add("@p_Process", "Print");
                paramFNFCalculation.Add("@P_EmployeeId", FNFCal_EmployeeId);
                paramFNFCalculation.Add("@P_ResignationId", ResignationId);
                var FNFCalculationsList = DapperORM.ReturnList<dynamic>("sp_FNF_List_FNFCalculation", paramFNFCalculation).FirstOrDefault();
                ViewBag.GetFNFCalculationsdetailsList = FNFCalculationsList;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetBonusPreviousYear
        [HttpGet]
        public ActionResult GetBonusPreviousYear(bool FY_Preivous_Bonus_Year_Applicable)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                List<AllDropDownBind> BonusPreviousYear = new List<AllDropDownBind>();
                if (FY_Preivous_Bonus_Year_Applicable == true)
                {
                    DynamicParameters paramLeaveYear = new DynamicParameters();
                    paramLeaveYear.Add("@query", "select BonusYearId as Id,cast(year(FromYear) as nvarchar(4))+'-'+cast(YEAR(ToYear) as nvarchar(4)) as Name from Payroll_BonusYear where Deactivate = 0 and IsPreviousYear ='" + FY_Preivous_Bonus_Year_Applicable + "'");
                    BonusPreviousYear = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLeaveYear).ToList();
                }
                else
                {
                    DynamicParameters paramCY = new DynamicParameters();
                    BonusPreviousYear = DapperORM.ReturnList<AllDropDownBind>("sp_FNF_GetBonusYearDropdown", paramCY).ToList();

                }
                return Json(new { BonusPreviousYear = BonusPreviousYear }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetBonusCurrentYear
        [HttpGet]
        public ActionResult GetBonusCurrentYear(bool FY_Current_Bonus_Year_Applicable)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                List<AllDropDownBind> BonusCurrentYear = new List<AllDropDownBind>();
                if (FY_Current_Bonus_Year_Applicable == true)
                {
                    DynamicParameters paramLeaveYear = new DynamicParameters();
                    paramLeaveYear.Add("@query", "select BonusYearId as Id,cast(year(FromYear) as nvarchar(4))+'-'+cast(YEAR(ToYear) as nvarchar(4)) as Name from Payroll_BonusYear where Deactivate = 0 and IsCurrentYear ='" + FY_Current_Bonus_Year_Applicable + "'");
                    BonusCurrentYear = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLeaveYear).ToList();

                }
                else
                {
                    DynamicParameters paramCY = new DynamicParameters();
                    BonusCurrentYear = DapperORM.ReturnList<AllDropDownBind>("sp_FNF_GetBonusYearDropdown", paramCY).ToList();

                }

                return Json(new { BonusCurrentYear = BonusCurrentYear }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetLeaveYear
        [HttpGet]
        public ActionResult GetLeaveYear(bool LeaveYear_Applicable, int EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                List<AllDropDownBind> LeaveYear = new List<AllDropDownBind>();
                if (LeaveYear_Applicable == true)
                {
                    var GetCmpID = "Select CmpID from Mas_Employee where Deactivate=0 and EmployeeId='" + EmployeeId + "'";
                    var CmpID = DapperORM.DynamicQuerySingle(GetCmpID);

                    DynamicParameters paramLeaveYear = new DynamicParameters();
                    paramLeaveYear.Add("@query", "select LeaveYearID as Id, cast(year(FromDate) as nvarchar(4))+'-'+cast(YEAR(ToDate) as nvarchar(4)) as Name from[dbo].[Leave_Year] where Deactivate = 0  and IsActivate=1  and CmpId='" + CmpID.CmpID + "' order by IsDefault desc,FromDate desc");
                    LeaveYear = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLeaveYear).ToList();
                }
                return Json(new { LeaveYear = LeaveYear }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetUnpaidSalaryDetails
        [HttpGet]
        public ActionResult GetUnpaidSalaryDetails(int FNFCal_EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramUnpaidSalary = new DynamicParameters();
                paramUnpaidSalary.Add("@P_EmployeeId", FNFCal_EmployeeId);
                var GetUnpaidSalary = DapperORM.ReturnList<dynamic>("sp_FNF_Employee_Cal_Detail", paramUnpaidSalary).FirstOrDefault();
                ViewBag.GetFnfDetails = GetUnpaidSalary;
                return Json(new { GetUnpaidSalary = GetUnpaidSalary }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region Get_SM_FNFDetails
        [HttpGet]
        public ActionResult Get_SM_FNFDetails(int FNFCal_EmployeeId, string A_Salary_Applicable, DateTime? A_SalaryDate, string B_Salary_Applicable, DateTime? B_SalaryDate, string C_Salary_Applicable, DateTime? C_SalaryDate,
            string FY_Preivous_Bonus_Year_Applicable, string FY_Preivous_Bonus_Year, string FY_Current_Bonus_Year_Applicable, string FY_Current_Bonus_Year, int? LeaveMasterLeaveYearId, string LeaveYear_Applicable)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramFNFCalculation = new DynamicParameters();
                paramFNFCalculation.Add("@P_A_Salary_Applicable", A_Salary_Applicable);
                paramFNFCalculation.Add("@P_A_SalaryDate", A_SalaryDate);
                paramFNFCalculation.Add("@P_B_Salary_Applicable", B_Salary_Applicable);
                paramFNFCalculation.Add("@P_B_SalaryDate", B_SalaryDate);
                paramFNFCalculation.Add("@P_C_Salary_Applicable", C_Salary_Applicable);
                paramFNFCalculation.Add("@P_C_SalaryDate", C_SalaryDate);
                paramFNFCalculation.Add("@P_FY_Preivous_Bonus_Year_Applicable", FY_Preivous_Bonus_Year_Applicable);
                paramFNFCalculation.Add("@P_FY_Preivous_Bonus_Year", FY_Preivous_Bonus_Year);
                paramFNFCalculation.Add("@P_FY_Current_Bonus_Year_Applicable", FY_Current_Bonus_Year_Applicable);
                paramFNFCalculation.Add("@P_FY_Current_Bonus_Year", FY_Current_Bonus_Year);
                paramFNFCalculation.Add("@P_LeaveYear_Applicable", LeaveYear_Applicable);
                paramFNFCalculation.Add("@P_LeaveMasterLeaveYearId", LeaveMasterLeaveYearId);
                paramFNFCalculation.Add("@P_EmployeeId", FNFCal_EmployeeId);
                var GetFnfDetails = DapperORM.ReturnList<dynamic>("sp_FNF_GetDetail_Show", paramFNFCalculation).FirstOrDefault();
                ViewBag.GetFnfDetails = GetFnfDetails;
                return Json(new { GetFnfDetails = GetFnfDetails }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        //Checker check & Send to account
        #region SendToApproval
        public ActionResult ViewForSendToApproval(double? FNFCald, double? FNFCal_EmployeeId, int ResignationId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 827;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                SM_FNFCalculations SM_FNFCalculations = new SM_FNFCalculations();
                DynamicParameters paramFNFCalculation = new DynamicParameters();
                paramFNFCalculation.Add("@p_Process", "Approval");
                paramFNFCalculation.Add("@P_FNFCald", FNFCald);
                paramFNFCalculation.Add("@P_EmployeeId", FNFCal_EmployeeId);
                paramFNFCalculation.Add("@P_ResignationId", ResignationId);
                SM_FNFCalculations = DapperORM.ReturnList<SM_FNFCalculations>("sp_FNF_List_FNFCalculation", paramFNFCalculation).FirstOrDefault();
                ViewBag.EmployeeDetails = SM_FNFCalculations;
                return View(SM_FNFCalculations);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region
        public ActionResult SendToApproval(double? FNFCald)
        {
            StringBuilder strBuilder = new StringBuilder();
            var createdBy = Session["EmployeeName"]?.ToString().Replace("'", "''");
            var machineName = Dns.GetHostName().Replace("'", "''");
            strBuilder.AppendLine("UPDATE dbo.FNF_Calculation " +
                "SET IsSendForApproval = 1, " +
                "ModifiedBy = '" + createdBy + "', " +
                "ModifiedDate = GETDATE(), " +
                "MachineName = '" + machineName + "' " +
                "WHERE FNFCald = '" + FNFCald + "';");
            string abc = "";
            if (objcon.SaveStringBuilder(strBuilder, out abc))
            {
                TempData["Message"] = "Record send for approval successfully.";
                TempData["Icon"] = "success";
            }
            return RedirectToAction("GetList", "ESS_FNF_SM_FNFCalculationStatement");
        }
        #endregion

        //Checker check & Send to account
        #region ViewForCheckerAccountApproval
        public ActionResult ViewForCheckerAccountApproval(double? FNFCald, double? FNFCal_EmployeeId, int ResignationId, string Process)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 827;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Approval";

                SM_FNFCalculations SM_FNFCalculations = new SM_FNFCalculations();
                DynamicParameters paramFNFCalculation = new DynamicParameters();
                paramFNFCalculation.Add("@p_Process", "Approval");
                paramFNFCalculation.Add("@P_FNFCald", FNFCald);
                paramFNFCalculation.Add("@P_EmployeeId", FNFCal_EmployeeId);
                paramFNFCalculation.Add("@P_ResignationId", ResignationId);
                SM_FNFCalculations = DapperORM.ReturnList<SM_FNFCalculations>("sp_FNF_List_FNFCalculation", paramFNFCalculation).FirstOrDefault();
                ViewBag.EmployeeDetails = SM_FNFCalculations;
                ViewBag.Process = Process;

                TempData["A_SalaryDate"] = SM_FNFCalculations.A_SalaryDate.HasValue? SM_FNFCalculations.A_SalaryDate.Value.ToString("yyyy-MM"): "";
                TempData["B_SalaryDate"] = SM_FNFCalculations.B_SalaryDate.HasValue ? SM_FNFCalculations.B_SalaryDate.Value.ToString("yyyy-MM"): "";
                TempData["C_SalaryDate"] = SM_FNFCalculations.C_SalaryDate.HasValue ? SM_FNFCalculations.C_SalaryDate.Value.ToString("yyyy-MM"): "";

                var GetCmpID = "Select CmpID from Mas_Employee where Deactivate=0 and EmployeeId='" + FNFCal_EmployeeId + "'";
                var CmpID = DapperORM.DynamicQuerySingle(GetCmpID);

                DynamicParameters paramLeaveYear = new DynamicParameters();
                paramLeaveYear.Add("@query", "select LeaveYearID as Id, cast(year(FromDate) as nvarchar(4))+'-'+cast(YEAR(ToDate) as nvarchar(4)) as Name from[dbo].[Leave_Year] where Deactivate = 0  and IsActivate=1  and CmpId='" + CmpID.CmpID + "' order by IsDefault desc,FromDate desc");
                var LeaveYearGet = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLeaveYear).ToList();
                ViewBag.GetLeaveYear = LeaveYearGet;

                DynamicParameters paramCY = new DynamicParameters();
                var BonusYear = DapperORM.ReturnList<AllDropDownBind>("sp_FNF_GetBonusYearDropdown", paramCY).ToList();
                ViewBag.GetBonusYear = BonusYear;
                return View(SM_FNFCalculations);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region
        public ActionResult ForFNFCheckerApproval(double? FNFCald, string Status, string Remark)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }

            StringBuilder strBuilder = new StringBuilder();
            var createdBy = Session["EmployeeName"]?.ToString().Replace("'", "''");
            var machineName = Dns.GetHostName().Replace("'", "''");
            if (Status == "Approved")
            {
                strBuilder.AppendLine("UPDATE dbo.FNF_Calculation " +
                "SET EmployeeCheckerId = '" + Session["EmployeeId"] + "', " +
                 "EmployeeCheckerDate = GETDATE(), " +
                 "EmployeeCheckerStatus = '" + Status + "', " +
                "EmployeeCheckerRemark = '" + Remark + "' " +
                "WHERE FNFCald = '" + FNFCald + "';");
                string abc = "";
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    TempData["Message"] = "Record approved successfully.";
                    TempData["Icon"] = "success";
                }
            }
            else
            {
                strBuilder.AppendLine("UPDATE dbo.FNF_Calculation " +
                "SET EmployeeCheckerId = '" + Session["EmployeeId"] + "', " +
                 "EmployeeCheckerDate = GETDATE(), " +
                 "EmployeeCheckerStatus = '" + Status + "', " +
                "EmployeeCheckerRemark = '" + Remark + "', " +
                 "IsSendForApproval = 0" +
                "WHERE FNFCald = '" + FNFCald + "';");
                string abc = "";
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    TempData["Message"] = "Record rejected successfully.";
                    TempData["Icon"] = "success";
                }
            }
            return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region
        public ActionResult ForFNFAccountApproval(double? FNFCald, string Status, string Remark)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            StringBuilder strBuilder = new StringBuilder();
            var createdBy = Session["EmployeeName"]?.ToString().Replace("'", "''");
            var machineName = Dns.GetHostName().Replace("'", "''");
            if (Status == "Approved")
            {
                strBuilder.AppendLine("UPDATE dbo.FNF_Calculation " +
                    "SET EmployeeAccountId = '" + Session["EmployeeId"] + "', " +
                     "EmployeeAccountDate = GETDATE(), " +
                     "EmployeeAccountStatus = '" + Status + "', " +
                    "EmployeeAccountRemark = '" + Remark + "' " +
                    "WHERE FNFCald = '" + FNFCald + "';");
                string abc = "";
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {

                    TempData["Message"] = "Record approved successfully.";
                    TempData["Icon"] = "success";
                }
            }
            else
            {
                strBuilder.AppendLine("UPDATE dbo.FNF_Calculation " +
                    "SET EmployeeAccountId = '" + Session["EmployeeId"] + "', " +
                     "EmployeeAccountDate = GETDATE(), " +
                     "EmployeeAccountStatus = '" + Status + "', " +
                     "EmployeeAccountRemark = '" + Remark + "' " +
                     "WHERE FNFCald = '" + FNFCald + "';");
                string abc = "";
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    TempData["Message"] = "Record rejected successfully.";
                    TempData["Icon"] = "success";
                }
            }
            return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region GetSalaryDetails
        [HttpGet]
        public ActionResult GetSalaryDetails(int FNFCal_EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@P_EmployeeId", FNFCal_EmployeeId);
                var salaryList = DapperORM.ReturnList<dynamic>("sp_FNF_GetSalaryDetails", param);
                return Json(new { salaryList = salaryList }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetBonusDetails
        [HttpGet]
        public ActionResult GetBonusDetails(int FNFCal_EmployeeId, int BonusYear)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@P_EmployeeId", FNFCal_EmployeeId);
                param.Add("@P_BonusYear", BonusYear);
                var BonusList = DapperORM.ReturnList<dynamic>("sp_FNF_GetBonusDetails", param);
                return Json(new { BonusList = BonusList }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion


        #region GetLeaveEncashmentDetails
        [HttpGet]
        public ActionResult GetLeaveEncashmentDetails(int FNFCal_EmployeeId, int LeaveMasterLeaveYearId, string yearType)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@P_EmployeeId", FNFCal_EmployeeId);
                param.Add("@P_LeaveMasterLeaveYearId", LeaveMasterLeaveYearId);
                param.Add("@P_YearType", yearType);
                var LeaveEncashmentList = DapperORM.ReturnList<dynamic>("sp_FNF_GetLeaveEncashmentDetails", param);
                return Json(new { LeaveEncashmentList = LeaveEncashmentList }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetNoticePeriodDays
        [HttpGet]
        public ActionResult GetNoticePeriodDays(int EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", $@"Select NoticePeriodDays,NoticePeriodType,Format(NoticePeriodEndDate,'dd/MMM/yyyy') as NoticePeriodEndDate from FNF_EmployeeResignation where FNF_EmployeeResignation.deactivate=0 and FNFEmployeeId='{EmployeeId}'");
                var info = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).FirstOrDefault();
                return Json(new { info = info }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region
        [HttpGet]
        public ActionResult GetNoticePeriodMonthDays(string NoticePeriodType)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                //ViewBag.NoticePeriodMonthDays = DapperORM.DynamicQuerySingle("Select Type Id, MonthDays as Name from tbl_NoticePeriodType_MonthDays where Type={NoticePeriodType}");
                DynamicParameters MonthDays = new DynamicParameters();
                MonthDays.Add("@query", $@"Select Id, MonthDays as Name from tbl_NoticePeriodType_MonthDays where Type='{NoticePeriodType}'");
                var NoticePeriodMonthDays = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", MonthDays).ToList();
                return Json(new { NoticePeriodMonthDays = NoticePeriodMonthDays }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region
        [HttpPost]
        public ActionResult UpdateNoticePeriod(FNF_EmployeeResignation EmployeeResignation)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                StringBuilder strBuilder = new StringBuilder();
                var createdBy = Session["EmployeeName"]?.ToString().Replace("'", "''");
                var machineName = Dns.GetHostName().Replace("'", "''");
                strBuilder.AppendLine("UPDATE dbo.FNF_EmployeeResignation " +
                    "SET ModifiedBy = '" + Session["EmployeeId"] + "', " +
                     "ModifiedDate = GETDATE(), " +
                     "NoticePeriodDays = '" + EmployeeResignation.NoticePeriodMonthDays + "', " +
                     "NoticePeriodType = '" + EmployeeResignation.NoticePeriodType + "', " +
                     "NoticePeriodEndDate = '" + EmployeeResignation.NoticePeriodEndDate + "', " +
                     "RelievingDate = '" + EmployeeResignation.RelievingDate + "', " +
                     "LastWorkingDate = '" + EmployeeResignation.LastWorkingDate + "' " +
                    "WHERE FNFId = '" + EmployeeResignation.FnfId + "' and FNFEmployeeId='" + EmployeeResignation.FnfEmployeeId + "';");
                string abc = "";
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    TempData["Message"] = "Record Save successfully.";
                    TempData["Icon"] = "success";
                }
                return RedirectToAction("GetList", "ESS_FNF_SM_FNFCalculationStatement");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion


        #region GetFNFCalculationRejectList
        [HttpGet]
        public ActionResult GetFNFCalculationRejectList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var data = DapperORM.ExecuteSP<dynamic>("sp_FNF_List_FNFCalculationCheckerRejectList", param).ToList();
                string jsonData = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

    }
}