using Dapper;
using System.Drawing;
using KompassHR.Areas.ESS.Models.ESS_FNF;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Drawing.Imaging;


namespace KompassHR.Areas.ESS.Controllers.ESS_FNF
{
    public class ESS_FNF_Maxroof_FNFCalculationStatementController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        #region ESS_FNF_Maxroof_FNFCalculationStatement Main View 
        public ActionResult ESS_FNF_Maxroof_FNFCalculationStatement(Maxroof_FNFCalculations Maxroof_FNFCalculations)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 921;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitleTitle = "Add";
                DynamicParameters paramEmployee = new DynamicParameters();
                paramEmployee.Add("@query", "Select EmployeeId as Id,EmployeeName + ' (' + Mas_Employee.EmployeeNo + ')' as Name from Mas_Employee where Deactivate = 0 and Mas_Employee.Deactivate = 0  and EmployeeId='" + Maxroof_FNFCalculations.FNFCal_EmployeeId + "'");
                var GetEmployee = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", paramEmployee).ToList();
                ViewBag.AllEmployeeName = GetEmployee;

                var GetCmpID = "Select CmpID from Mas_Employee where Deactivate=0 and EmployeeId='" + Maxroof_FNFCalculations.FNFCal_EmployeeId + "'";
                var CmpID = DapperORM.DynamicQuerySingle(GetCmpID);

                DynamicParameters paramLeaveYear = new DynamicParameters();
                paramLeaveYear.Add("@query", "select LeaveYearID as Id, cast(year(FromDate) as nvarchar(4))+'-'+cast(YEAR(ToDate) as nvarchar(4)) as Name from[dbo].[Leave_Year] where Deactivate = 0  and IsActivate=1  and CmpId='" + CmpID.CmpID + "' order by IsDefault desc,FromDate desc");
                var LeaveYearGet = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLeaveYear).ToList();
                ViewBag.GetLeaveYear = LeaveYearGet;

                DynamicParameters paramCY = new DynamicParameters();
                var BonusYear = DapperORM.ReturnList<AllDropDownBind>("sp_FNF_GetBonusYearDropdown", paramCY).ToList();
                ViewBag.GetBonusYear = BonusYear;

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@P_EmployeeId", Maxroof_FNFCalculations.FNFCal_EmployeeId);
                var info = DapperORM.ExecuteSP<dynamic>("sp_MaxRoof_FNF_GetDetail_Show", param1).FirstOrDefault();
                ViewBag.EmployeeDetails = info;

                ViewBag.AddUpdateTitle = "Add";
                if (Maxroof_FNFCalculations.FNFCal_EmployeeId != 0)
                {
                    param.Add("@p_EmployeeId", Maxroof_FNFCalculations.FNFCal_EmployeeId);
                    var data = DapperORM.ReturnList<dynamic>("sp_MaxRoof_FNF_FNFEmployee", param).FirstOrDefault();
                    TempData["DOJ"] = data.DOJ;
                    TempData["ResignationDate"] = data.ResignationDate;
                    TempData["LastWorkingDate"] = data.LastWorkingDate;
                    ViewBag.GetData = data;
                }
                else
                {
                    ViewBag.GetData = ""; ;
                }
                if (Maxroof_FNFCalculations.FNFCald != 0)
                {
                    ViewBag.AddUpdateTitleTitle = "Update";
                    DynamicParameters paramupdate = new DynamicParameters();
                    paramupdate.Add("@p_Process", "Update");
                    paramupdate.Add("@P_FNFCald", Maxroof_FNFCalculations.FNFCald);
                    Maxroof_FNFCalculations = DapperORM.ReturnList<Maxroof_FNFCalculations>("sp_MaxRoof_FNF_List_FNFCalculation", paramupdate).FirstOrDefault();
                }
                return View(Maxroof_FNFCalculations);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsMaxroof_FNFCalculationstatementExits
        public ActionResult IsMaxroof_FNFCalculationstatementExits(string FnfCalId_Encrypted, double FNFCal_EmployeeId)
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
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Maxroof_FNFCalculationstatement", param);
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
        public ActionResult SaveUpdate(Maxroof_FNFCalculations Maxroof_FNFCalculations)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(Maxroof_FNFCalculations.FNFCal_Encrypted) ? "Save" : "Update");
                param.Add("@p_FNFCal_Encrypted", Maxroof_FNFCalculations.FNFCal_Encrypted);
                param.Add("@p_FNFCald", Maxroof_FNFCalculations.FNFCald);
                param.Add("@p_FNFEmployeeId", Maxroof_FNFCalculations.FNFCal_EmployeeId);
                param.Add("@p_FNFCmpID", Maxroof_FNFCalculations.CmpID);
                param.Add("@p_FNFBranchID", Maxroof_FNFCalculations.BranchID);
                param.Add("@p_FNFDocNo", Maxroof_FNFCalculations.DocNo);
                param.Add("@p_FNFDate", Maxroof_FNFCalculations.FNFDate);   // REQUIRED
                param.Add("@p_EmployeeName", Maxroof_FNFCalculations.EmployeeName);
                param.Add("@p_EmployeeNo", Maxroof_FNFCalculations.EmployeeNo);
                param.Add("@p_Department", Maxroof_FNFCalculations.Department);
                param.Add("@p_Designation", Maxroof_FNFCalculations.Designation);
                param.Add("@p_Grade", Maxroof_FNFCalculations.Grade);
                param.Add("@p_BusinessUnit", Maxroof_FNFCalculations.BusinessUnit);
                param.Add("@p_JoiningDate", Maxroof_FNFCalculations.JoiningDate);
                param.Add("@p_ReginationDate", Maxroof_FNFCalculations.ResignationDate);
                param.Add("@p_LastWorkingDate", Maxroof_FNFCalculations.LastWorkingDate);
                param.Add("@p_RelievingDate", Maxroof_FNFCalculations.RelievingDate);
                param.Add("@p_NoticePeriodEndDate", Maxroof_FNFCalculations.NoticePeriodEndDate);
                param.Add("@p_DailyMonthly", Maxroof_FNFCalculations.DailyMonthly);
                param.Add("@p_Bond_Applicable", Maxroof_FNFCalculations.Bond_Applicable);
                param.Add("@p_BondEndDate", Maxroof_FNFCalculations.BondEndDate);
                param.Add("@p_BondAmt", Maxroof_FNFCalculations.BondAmt);
                param.Add("@P_A_Salary_Applicable", Maxroof_FNFCalculations.A_Salary_Applicable);
                param.Add("@P_A_SalaryDate", Maxroof_FNFCalculations.A_SalaryDate);
                param.Add("@P_FY_Preivous_Bonus_Year_Applicable", Maxroof_FNFCalculations.FY_Preivous_Bonus_Year_Applicable);
                param.Add("@P_FY_Preivous_Bonus_Year", Maxroof_FNFCalculations.FY_Preivous_Bonus_Year);
                param.Add("@P_FY_Current_Bonus_Year_Applicable", Maxroof_FNFCalculations.FY_Current_Bonus_Year_Applicable);
                param.Add("@P_FY_Current_Bonus_Year", Maxroof_FNFCalculations.FY_Current_Bonus_Year);
                param.Add("@P_LeaveYear_Applicable", Maxroof_FNFCalculations.LeaveYear_Applicable);
                param.Add("@P_LeaveMasterLeaveYearId", Maxroof_FNFCalculations.LeaveMasterLeaveYearId);
                param.Add("@p_UnpaidSalaryA_Date", Maxroof_FNFCalculations.UnpaidSalaryA_Date);
                param.Add("@p_UnpaidSalaryA_Amount", Maxroof_FNFCalculations.UnpaidSalaryA_Amount);
                param.Add("@p_TotalUnpaidSalaryAmount", Maxroof_FNFCalculations.TotalUnpaidSalaryAmount);
                param.Add("@p_LastSalaryPaidMonth", Maxroof_FNFCalculations.LastSalaryPaidMonth);
                param.Add("@p_PLDays", Maxroof_FNFCalculations.PLDays);
                param.Add("@p_PayableDays", Maxroof_FNFCalculations.PayableDays);
                param.Add("@p_MonthDays", Maxroof_FNFCalculations.MonthDays);
                param.Add("@p_LOPDays", Maxroof_FNFCalculations.LOPDays);
                param.Add("@p_EffectiveWorkDays", Maxroof_FNFCalculations.EffectiveWorkDays);
                param.Add("@p_PreivousBonus_Remark", Maxroof_FNFCalculations.PreivousBonus_Remark);
                param.Add("@p_PreivousBonus_Amount", Maxroof_FNFCalculations.BonusPreviousYearAmount);
                param.Add("@p_CurrentBonus_Remark", Maxroof_FNFCalculations.CurrentBonus_Remark);
                param.Add("@p_CurrentBonus_Amount", Maxroof_FNFCalculations.BonusCurrentYearAmount);
                param.Add("@p_TotalBonus_Amount", Maxroof_FNFCalculations.TotalBonus_Amount);
                param.Add("@p_PreivousLeave_Remark", Maxroof_FNFCalculations.LeavePreviousYear);
                param.Add("@p_PreivousLeave_Days", Maxroof_FNFCalculations.PreivousLeave_Days);
                param.Add("@p_PreivousLeave_Amount", Maxroof_FNFCalculations.LeavePreviousYearAmount);
                param.Add("@p_CurrentLeave_Remark", Maxroof_FNFCalculations.LeaveCurrentYear);
                param.Add("@p_CurrentLeave_Days", Maxroof_FNFCalculations.CurrentLeave_Days);
                param.Add("@p_Current_Amount", Maxroof_FNFCalculations.LeaveCurrentYearAmount);
                param.Add("@p_TotalLeave_Amount", Maxroof_FNFCalculations.TotalLeave_Amount);
                param.Add("@p_TotalIncome", Maxroof_FNFCalculations.TotalIncome);
                param.Add("@p_Advance", Maxroof_FNFCalculations.AdvanceRecovered);
                param.Add("@p_TDS", Maxroof_FNFCalculations.SalaryTDS);
                param.Add("@p_TotalDeduction", Maxroof_FNFCalculations.TotalDeduction);
                param.Add("@p_NetPay", Maxroof_FNFCalculations.NetPay);
                param.Add("@p_NetPayInWord", Maxroof_FNFCalculations.NetPayInWord);
                param.Add("@p_Gratuity_Applicable", Maxroof_FNFCalculations.Gratuity_Applicable);
                param.Add("@p_Gratuity_Service", Maxroof_FNFCalculations.Gratuity_Service);
                param.Add("@p_NetPayInWord", Maxroof_FNFCalculations.NetPayInWord);
                param.Add("@p_Gratuity_Applicable", Maxroof_FNFCalculations.Gratuity_Applicable);
                param.Add("@p_Gratuity_Service", Maxroof_FNFCalculations.Gratuity_Service);
                param.Add("@p_GratuityAmount", Maxroof_FNFCalculations.GratuityAmount);
                param.Add("@p_GratuityAmountInWord", Maxroof_FNFCalculations.GratuityAmountInWord);
                param.Add("@p_FinalNetPayable", Maxroof_FNFCalculations.FinalNetPayable);
                param.Add("@p_FinalNetPayableInWord", Maxroof_FNFCalculations.FinalNetPayableInWord);
                param.Add("@p_FNFMakerId", Session["EmployeeId"]);
                param.Add("@p_FNFMakerRemark", Maxroof_FNFCalculations.Remark);
                param.Add("@p_ResignationId", Maxroof_FNFCalculations.ResignationId);
                param.Add("@p_MachineName", Dns.GetHostName());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 10);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_MaxRoof_SUD_FNF_FNFCalculationStatement", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                string p_Id = param.Get<string>("@p_Id");
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 921;
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

                var GetRejectCount = "Select count(FNFCald) RejectCount from FNF_Calculation_MaxRoof where Deactivate=0 and EmployeeCheckerStatus='Rejected'";
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
                var FNFPendingList = DapperORM.ReturnList<dynamic>("sp_MaxRoof_FNF_FNFCalulation_Pending", param1).ToList();
                ViewBag.GetFNFPendingList = FNFPendingList;

                TempData["FNFPendingCount"] = FNFPendingList?.Count() ?? 0;

                DynamicParameters paramFNFCalculation = new DynamicParameters();
                paramFNFCalculation.Add("@p_Process", "List");
                paramFNFCalculation.Add("@P_EmployeeId", Session["EmployeeId"]);
                var FNFCompleteList = DapperORM.ReturnList<dynamic>("sp_MaxRoof_FNF_List_FNFCalculation", paramFNFCalculation).ToList();
                ViewBag.GetFNFCompleteList = FNFCompleteList;
                TempData["FNFCompleteCount"] = FNFCompleteList?.Count() ?? 0;

                DynamicParameters paramCheckerFNF = new DynamicParameters();
                paramCheckerFNF.Add("@p_Process", "Checker");
                paramCheckerFNF.Add("@P_EmployeeID", Session["EmployeeId"]);
                var CheckerFNFPendingList = DapperORM.ReturnList<dynamic>("sp_MaxRoof_FNF_List_FNFForChckerAccountApproval", paramCheckerFNF).ToList();
                ViewBag.GetCheckerFNFPendingList = CheckerFNFPendingList;

                TempData["CheckerFNFPendingCount"] = CheckerFNFPendingList?.Count() ?? 0;
                DynamicParameters paramAccountFNF = new DynamicParameters();
                paramAccountFNF.Add("@p_Process", "Account");
                paramAccountFNF.Add("@P_EmployeeID", Session["EmployeeId"]);
                var AccountFNFPendingList = DapperORM.ReturnList<dynamic>("sp_MaxRoof_FNF_List_FNFForChckerAccountApproval", paramAccountFNF).ToList();
                ViewBag.GetAccountFNFPendingList = AccountFNFPendingList;

                TempData["AccountFNFPendingCount"] = AccountFNFPendingList?.Count() ?? 0;
                if ((Convert.ToInt32(GetFNFMakerCheckerAccount.FNFMakerId) == Convert.ToInt32(Session["EmployeeId"])) && (Convert.ToInt32(GetFNFMakerCheckerAccount.FNFCheckerId) == Convert.ToInt32(Session["EmployeeId"])))
                {
                    DynamicParameters paramMakerChecker = new DynamicParameters();
                    paramMakerChecker.Add("@p_Process", "ApproveRejectList");
                    paramMakerChecker.Add("@P_EmployeeID", Session["EmployeeId"]);
                    var CheckerAccountApproveRejectList = DapperORM.ReturnList<dynamic>("sp_MaxRoof_FNF_List_FNFForChckerAccountApproval", paramMakerChecker).ToList();
                    ViewBag.GetCheckerAccountApproveRejectList = CheckerAccountApproveRejectList;

                    if (GetFNFMakerCheckerAccount != null && Convert.ToInt32(GetFNFMakerCheckerAccount.FNFCheckerId) == Convert.ToInt32(Session["EmployeeId"]))
                    {
                        var GetMaker_checker_Account_ApproveRejectcount = "select Count(FNFCald) Totalcount from FNF_Calculation_MaxRoof where  Deactivate=0 and  EmployeeCheckerStatus != 'Pending' and EmployeeCheckerId='" + Session["EmployeeId"] + "'";
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
                else if (GetFNFMakerCheckerAccount != null && Convert.ToInt32(GetFNFMakerCheckerAccount.FNFMakerId) == Convert.ToInt32(Session["EmployeeId"]))
                {
                    DynamicParameters parammaker = new DynamicParameters();
                    parammaker.Add("@p_Process", "ApproveRejectList_ForMaker");
                    parammaker.Add("@P_EmployeeID", Session["EmployeeId"]);
                    var CheckerAccountApproveRejectList = DapperORM.ReturnList<dynamic>("sp_MaxRoof_FNF_List_FNFForChckerAccountApproval", parammaker).ToList();
                    ViewBag.GetCheckerAccountApproveRejectList = CheckerAccountApproveRejectList;

                    var GetMaker_checker_Account_ApproveRejectcount = "select count(FNFCald) as totalCount from FNF_Calculation_MaxRoof where FNF_Calculation_MaxRoof.Deactivate=0 and IsSendForApproval=1 and FNF_Calculation_MaxRoof.EmployeeMakerId='" + Session["EmployeeId"] + "'";
                    var Maker_checker_Account_ApproveRejectcount = DapperORM.DynamicQuerySingle(GetMaker_checker_Account_ApproveRejectcount);
                    if (Maker_checker_Account_ApproveRejectcount != null)
                    {
                        TempData["Maker_checker_Account_ApproveRejectcount"] = CheckerAccountApproveRejectList?.Count() ?? 0;
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
                    var CheckerAccountApproveRejectList = DapperORM.ReturnList<dynamic>("sp_MaxRoof_FNF_List_FNFForChckerAccountApproval", paramAccount).ToList();
                    ViewBag.GetCheckerAccountApproveRejectList = CheckerAccountApproveRejectList;

                    if (GetFNFMakerCheckerAccount != null && Convert.ToInt32(GetFNFMakerCheckerAccount.FNFCheckerId) == Convert.ToInt32(Session["EmployeeId"]))
                    {
                        var GetMaker_checker_Account_ApproveRejectcount = "select Count(FNFCald) Totalcount from FNF_Calculation_MaxRoof where  Deactivate=0  and EmployeeCheckerStatus != 'Pending' and EmployeeCheckerId='" + Session["EmployeeId"] + "'";
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
                        var GetMaker_checker_Account_ApproveRejectcount = "select Count(FNFCald) Totalcount from FNF_Calculation_MaxRoof where  Deactivate=0  and EmployeeAccountStatus != 'Pending' and EmployeeAccountId='" + Session["EmployeeId"] + "'";
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
                var Result = DapperORM.ExecuteReturn("sp_MaxRoof_SUD_FNF_FNFCalculationStatement", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("GetList", "ESS_FNF_Maxroof_FNFCalculationStatement");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region PreViewStatement
        public ActionResult PreViewStatement(int? FNFCald ,double? FNFCal_EmployeeId, int ResignationId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var GetCompanyName = "select Mas_CompanyProfile.CompanyName,Mas_Employee.CmpID from Mas_Employee inner join Mas_CompanyProfile on Mas_CompanyProfile.CompanyId = Mas_Employee.CmpID where Mas_CompanyProfile.Deactivate=0 and Mas_Employee.EmployeeId='" + FNFCal_EmployeeId + "'";
                var CompanyName = DapperORM.DynamicQuerySingle(GetCompanyName);
                ViewBag.CompanyName = CompanyName.CompanyName;
                var CmpId = CompanyName.CmpID;

                Maxroof_FNFCalculations Maxroof_FNFCalculations = new Maxroof_FNFCalculations();
                DynamicParameters paramFNFCalculation = new DynamicParameters();
                paramFNFCalculation.Add("@p_Process", "Print");
                paramFNFCalculation.Add("@P_EmployeeId", FNFCal_EmployeeId);
                paramFNFCalculation.Add("@P_ResignationId", ResignationId);
                var FNFCalculationsList = DapperORM.ReturnList<dynamic>("sp_MaxRoof_FNF_List_FNFCalculation", paramFNFCalculation).FirstOrDefault();
                ViewBag.GetFNFCalculationsdetailsList = FNFCalculationsList;

                DynamicParameters EarningInfo = new DynamicParameters();
                EarningInfo.Add("@P_FNFCald", FNFCald);
                ViewBag.earnings = DapperORM.ReturnList<dynamic>("sp_MaxRoof_FNF_Earnings", EarningInfo).ToList();

                DynamicParameters DeductionInfo = new DynamicParameters();
                DeductionInfo.Add("@P_FNFCald", FNFCald);
                ViewBag.deductions = DapperORM.ReturnList<dynamic>("sp_MaxRoof_FNF_Deduction", DeductionInfo).ToList();

                if (FNFCalculationsList.EmployeeCheckerStatus != "Pending")
                {
                    //SET COMPANY LOGO COMPANY WISE
                    var path = DapperORM.DynamicQuerySingle("Select SignaturePath from Mas_Employee_Signature where Deactivate=0 and SignatureEmployeeId=" + FNFCalculationsList.EmployeeCheckerId + "");
                    var SecondPath = path.SignaturePath;
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Onboarding'");
                    var FirstPath = GetDocPath.DocInitialPath + FNFCalculationsList.EmployeeCheckerId + "\\" + "Signature" + "\\";
                    // string GetBase64 = null;
                    string fullPath = "";
                    fullPath = FirstPath + SecondPath;
                    string extensionType1 = Path.GetExtension(fullPath).ToLower();
                    string mimeType1 = "image/jpeg";

                    if (extensionType1 == ".png")
                    {
                        mimeType1 = "image/png";
                    }
                    else if (extensionType1 == ".jpg" || extensionType1 == ".jpeg")
                    {
                        mimeType1 = "image/jpeg";
                    }

                    //string Extention = System.IO.Path.GetExtension(fullPath);
                    if (path.SignaturePath != null)
                    {
                        try
                        {
                            string directoryPath = Path.GetDirectoryName(fullPath);
                            if (!Directory.Exists(directoryPath))
                            {
                                Session["SignaturePath"] = "";
                            }
                            else
                            {
                                using (Image image = Image.FromFile(fullPath))
                                {
                                    using (MemoryStream m = new MemoryStream())
                                    {
                                        image.Save(m, image.RawFormat);
                                        byte[] imageBytes = m.ToArray();

                                        // Convert byte[] to Base64 String
                                        string base64String = Convert.ToBase64String(imageBytes);
                                        Session.Remove("SignaturePath");
                                        Session["SignaturePath"] = "data:" + mimeType1 + ";base64," + base64String;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message != null)
                            {
                                Session["SignaturePath"] = "";
                            }
                        }
                    }
                }


                var GetCompanyStamp = "Select Stamp from Mas_CompanyProfile Where CompanyId= " + CmpId + "";
                var path1 = DapperORM.DynamicQuerySingle(GetCompanyStamp);
                var SecondPath1 = path1.Stamp;

                var GetDocPath1 = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='CompanyStamp'");
                var FisrtPath1 = GetDocPath1.DocInitialPath + CmpId + "\\";

                string GetBase64 = null;
                string fullPath1 = "";
                fullPath1 = FisrtPath1 + SecondPath1;
                string extensionType = Path.GetExtension(fullPath1).ToLower();
                string mimeType = "image/jpeg";

                if (extensionType == ".png")
                {
                    mimeType = "image/png";
                }
                else if (extensionType == ".jpg" || extensionType == ".jpeg")
                {
                    mimeType = "image/jpeg";
                }
                if (SecondPath1 != null)
                {
                    if (!Directory.Exists(fullPath1))
                    {
                        using (Image image = Image.FromFile(fullPath1))
                        {
                            using (MemoryStream m = new MemoryStream())
                            {
                                image.Save(m, image.RawFormat);
                                byte[] imageBytes = m.ToArray();
                                // Convert byte[] to Base64 String
                                string base64String = Convert.ToBase64String(imageBytes);
                                Session.Remove("Stamp");
                                Session["Stamp"] = "data:" + mimeType + ";base64," + base64String;
                            }
                        }
                    }
                }
                else
                {
                    Session["Stamp"] = "";
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
                var GetUnpaidSalary = DapperORM.ReturnList<dynamic>("sp_MaxRoof_FNF_Employee_Cal_Detail", paramUnpaidSalary).FirstOrDefault();
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

        #region Get_MaxRoof_FNFDetails
        [HttpGet]
        public ActionResult Get_MaxRoof_FNFDetails(int FNFCal_EmployeeId, string A_Salary_Applicable, DateTime? A_SalaryDate,
            string FY_Preivous_Bonus_Year_Applicable, string FY_Preivous_Bonus_Year, string FY_Current_Bonus_Year_Applicable, string FY_Current_Bonus_Year, int? LeaveMasterLeaveYearId, string LeaveYear_Applicable)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                dynamic GetFnfDetails = null;
                List<FNF_Earning_Info> GetEarning = new List<FNF_Earning_Info>();
                List<FNF_Deduction_Info> GetDeduction = new List<FNF_Deduction_Info>();
                DynamicParameters MulQuery = new DynamicParameters();
                MulQuery.Add("@P_A_Salary_Applicable", A_Salary_Applicable);
                MulQuery.Add("@P_A_SalaryDate", A_SalaryDate);
                MulQuery.Add("@P_FY_Preivous_Bonus_Year_Applicable", FY_Preivous_Bonus_Year_Applicable);
                MulQuery.Add("@P_FY_Preivous_Bonus_Year", FY_Preivous_Bonus_Year);
                MulQuery.Add("@P_FY_Current_Bonus_Year_Applicable", FY_Current_Bonus_Year_Applicable);
                MulQuery.Add("@P_FY_Current_Bonus_Year", FY_Current_Bonus_Year);
                MulQuery.Add("@P_LeaveYear_Applicable", LeaveYear_Applicable);
                MulQuery.Add("@P_LeaveMasterLeaveYearId", LeaveMasterLeaveYearId);
                MulQuery.Add("@P_EmployeeId", FNFCal_EmployeeId);
                using (var multi = DapperORM.DynamicMultipleResultList("sp_MaxRoof_FNF_GetDetail_Show", MulQuery))
                {
                    GetFnfDetails = multi.Read<dynamic>().FirstOrDefault();
                    GetEarning = multi.Read<FNF_Earning_Info>().ToList();
                    GetDeduction = multi.Read<FNF_Deduction_Info>().ToList();
                }

                return Json(new { GetFnfDetails = GetFnfDetails, GetEarning = GetEarning, GetDeduction = GetDeduction }, JsonRequestBehavior.AllowGet);

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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 921;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                Maxroof_FNFCalculations Maxroof_FNFCalculations = new Maxroof_FNFCalculations();
                DynamicParameters paramFNFCalculation = new DynamicParameters();
                paramFNFCalculation.Add("@p_Process", "Approval");
                paramFNFCalculation.Add("@P_FNFCald", FNFCald);
                paramFNFCalculation.Add("@P_EmployeeId", FNFCal_EmployeeId);
                paramFNFCalculation.Add("@P_ResignationId", ResignationId);
                Maxroof_FNFCalculations = DapperORM.ReturnList<Maxroof_FNFCalculations>("sp_MaxRoof_FNF_List_FNFCalculation", paramFNFCalculation).FirstOrDefault();
                ViewBag.EmployeeDetails = Maxroof_FNFCalculations;
                return View(Maxroof_FNFCalculations);
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
            strBuilder.AppendLine("UPDATE dbo.FNF_Calculation_MaxRoof " +
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
            return RedirectToAction("GetList", "ESS_FNF_Maxroof_FNFCalculationStatement");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 921;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Approval";

                Maxroof_FNFCalculations Maxroof_FNFCalculations = new Maxroof_FNFCalculations();
                DynamicParameters paramFNFCalculation = new DynamicParameters();
                paramFNFCalculation.Add("@p_Process", "Approval");
                paramFNFCalculation.Add("@P_FNFCald", FNFCald);
                paramFNFCalculation.Add("@P_EmployeeId", FNFCal_EmployeeId);
                paramFNFCalculation.Add("@P_ResignationId", ResignationId);
                Maxroof_FNFCalculations = DapperORM.ReturnList<Maxroof_FNFCalculations>("sp_MaxRoof_FNF_List_FNFCalculation", paramFNFCalculation).FirstOrDefault();
                ViewBag.EmployeeDetails = Maxroof_FNFCalculations;

                DynamicParameters EarningInfo = new DynamicParameters();
                EarningInfo.Add("@P_FNFCald", FNFCald);
                Maxroof_FNFCalculations.earnings = DapperORM.ReturnList<FNF_Earning_Info>("sp_MaxRoof_FNF_Earnings", EarningInfo).ToList();

                DynamicParameters DeductionInfo = new DynamicParameters();
                DeductionInfo.Add("@P_FNFCald", FNFCald);
                Maxroof_FNFCalculations.deductions = DapperORM.ReturnList<FNF_Deduction_Info>("sp_MaxRoof_FNF_Deduction", DeductionInfo).ToList();

                ViewBag.Process = Process;

                TempData["A_SalaryDate"] = Maxroof_FNFCalculations.A_SalaryDate.HasValue ? Maxroof_FNFCalculations.A_SalaryDate.Value.ToString("yyyy-MM") : "";
             
                var GetCmpID = "Select CmpID from Mas_Employee where Deactivate=0 and EmployeeId='" + FNFCal_EmployeeId + "'";
                var CmpID = DapperORM.DynamicQuerySingle(GetCmpID);

                DynamicParameters paramLeaveYear = new DynamicParameters();
                paramLeaveYear.Add("@query", "select LeaveYearID as Id, cast(year(FromDate) as nvarchar(4))+'-'+cast(YEAR(ToDate) as nvarchar(4)) as Name from[dbo].[Leave_Year] where Deactivate = 0  and IsActivate=1  and CmpId='" + CmpID.CmpID + "' order by IsDefault desc,FromDate desc");
                var LeaveYearGet = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLeaveYear).ToList();
                ViewBag.GetLeaveYear = LeaveYearGet;

                DynamicParameters paramCY = new DynamicParameters();
                var BonusYear = DapperORM.ReturnList<AllDropDownBind>("sp_FNF_GetBonusYearDropdown", paramCY).ToList();
                ViewBag.GetBonusYear = BonusYear;
                return View(Maxroof_FNFCalculations);
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
                strBuilder.AppendLine("UPDATE dbo.FNF_Calculation_MaxRoof " +
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
                strBuilder.AppendLine("UPDATE dbo.FNF_Calculation_MaxRoof " +
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
                strBuilder.AppendLine("UPDATE dbo.FNF_Calculation_MaxRoof " +
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
                strBuilder.AppendLine("UPDATE dbo.FNF_Calculation_MaxRoof " +
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
                return RedirectToAction("GetList", "ESS_FNF_Maxroof_FNFCalculationStatement");
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
                var data = DapperORM.ExecuteSP<dynamic>("sp_MaxRoof_FNF_List_FNFCalculationCheckerRejectList", param).ToList();
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

        #region  Multuple checker Approve Reject Request function
        [HttpPost]
        public ActionResult MultipleApproveRejectRequest(List<RecordList> ObjRecordList)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 921;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                StringBuilder strBuilder = new StringBuilder();
                var createdBy = Session["EmployeeName"]?.ToString().Replace("'", "''");
                var machineName = Dns.GetHostName().Replace("'", "''");

                if (ObjRecordList.Count > 0)
                {
                    for (var i = 0; i < ObjRecordList.Count; i++)
                    {
                        strBuilder.AppendLine("UPDATE dbo.FNF_Calculation_MaxRoof " +
                        "SET EmployeeCheckerId = '" + Session["EmployeeId"] + "', " +
                         "EmployeeCheckerDate = GETDATE(), " +
                         "EmployeeCheckerStatus = '" + ObjRecordList[i].Status + "' " +
                         "WHERE FNFCald = '" + ObjRecordList[i].FNFCald + "';");
                        string abc = "";
                        if (objcon.SaveStringBuilder(strBuilder, out abc))
                        {
                            TempData["Message"] = "Record approved successfully.";
                            TempData["Icon"] = "success";
                        }
                    }
                }
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region CancelFNF
        public ActionResult CancelFNF(double? FNFCald,string remark)
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

                strBuilder.AppendLine("UPDATE dbo.FNF_Calculation_MaxRoof SET CancelBy = '" + Session["EmployeeName"] + "' ,CancelDate = GETDATE(),IsCancel = 1,Deactivate=1,IsSendForApproval = 0 ,IsFNFCalulation=0, CancelRemark='" + remark+"' , EmployeeMakerId=NULL,EmployeeCheckerId=NULL WHERE FNFCald = '" + FNFCald + "';");
                string abc = "";
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    TempData["Message"] = "Record Cancel successfully.";
                    TempData["Icon"] = "success";
                }

                return RedirectToAction("GetList", "ESS_FNF_Maxroof_FNFCalculationStatement");
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