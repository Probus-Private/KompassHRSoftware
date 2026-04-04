using Dapper;
using KompassHR.Areas.ESS.Models.ESS_FNF;
using KompassHR.Areas.Setting.Models.Setting_FullAndFinal;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_FNF
{
    public class ESS_FNF_EmployeeResignationController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        [HttpGet]

        #region ESS_FNF_EmployeeResignation Main View 
        public ActionResult ESS_FNF_EmployeeResignation(string FnfId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 624;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                FNF_EmployeeResignation EmployeeResignation = new FNF_EmployeeResignation();
                ViewBag.AddUpdateTitle = "Add";

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetCompanyName = GetComapnyName;
                var CmpId = GetComapnyName[0].Id;

                //var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CmpId), Convert.ToInt32(Session["EmployeeId"]));
                //ViewBag.GetBranchName = Branch;
                //var BranchId = Branch[0].Id;
                ViewBag.GetBranchName = "";

                //DynamicParameters param7 = new DynamicParameters();
                //param7.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeeLeft=0 and  CmpID=" + CmpId + "  and ContractorId=1 order by Name");
                //var data7 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param7).ToList();
                //ViewBag.AllEmployeeName = data7;
                ViewBag.AllEmployeeName = "";

                DynamicParameters param6 = new DynamicParameters();
                param6.Add("@query", "Select ReasonId,ReasonName from FNF_Reason Where Deactivate=0");
                var list_FNF_Reason = DapperORM.ReturnList<FNF_Reason>("sp_QueryExcution", param6).ToList();
                ViewBag.GetReasonName = list_FNF_Reason;


                DynamicParameters paramResType = new DynamicParameters();
                paramResType.Add("@query", "Select ResignationTypeId as Id,ResignationType As Name from FNF_ResignationType Where Deactivate=0 Order By Name");
                var ResignationType = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramResType).ToList();
                ViewBag.GetResignationType = ResignationType;


                ViewBag.NoticePeriodMonthDays = "";
               DynamicParameters NPTdata = new DynamicParameters();
                NPTdata.Add("@query", "Select Id, MonthDays as Name from tbl_NoticePeriodType_MonthDays");
                var GetNPTdata = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", NPTdata);
                ViewBag.NoticePeriodMonthDays = GetNPTdata;
                TempData["NoticePeriodMonthDays"] = EmployeeResignation?.NoticePeriodMonthDays;

                if (FnfId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_FnfId_Encrypted", FnfId_Encrypted);
                    EmployeeResignation = DapperORM.ExecuteSP<FNF_EmployeeResignation>("sp_List_FNF_EmployeeResignationByHRlist", param).FirstOrDefault();
                    TempData["ResignationDate"] = EmployeeResignation.ResignationDate.ToString("yyyy-MM-dd");
                    TempData["RelievingDate"] = EmployeeResignation.RelievingDate.ToString("yyyy-MM-dd"); 
                    TempData["LastWorkingDate"] = EmployeeResignation.LastWorkingDate.ToString("yyyy-MM-dd"); 
                    TempData["NoticePeriodEndDate"] = EmployeeResignation.NoticePeriodEndDate.ToString("yyyy-MM-dd");
                    TempData["DOJ"] = EmployeeResignation.DOJ.ToString("yyyy-MM-dd");

                    DynamicParameters NPTdata1 = new DynamicParameters();
                    NPTdata1.Add("@query", "Select Id, MonthDays as Name from tbl_NoticePeriodType_MonthDays where Type='"+ EmployeeResignation.NoticePeriodType+"'");
                    var GetNPTdata1 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", NPTdata1);
                    ViewBag.NoticePeriodMonthDays = GetNPTdata1;
                    TempData["NoticePeriodMonthDays"] = EmployeeResignation?.NoticePeriodMonthDays;
                    if (EmployeeResignation.CmpId > 0)
                    {
                        DynamicParameters paramBranch = new DynamicParameters();
                        paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                        paramBranch.Add("@p_CmpId", EmployeeResignation.CmpId);
                        var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                        ViewBag.GetBranchName = BranchName;

                        if (EmployeeResignation.FnfBranchID > 0)
                        {
                            DynamicParameters param = new DynamicParameters();
                            param.Add("@p_employeeid", Session["EmployeeId"]);
                            param.Add("@p_BranchId", EmployeeResignation.FnfBranchID);
                            var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetEmployeeDropdownForResignation", param).ToList();
                            ViewBag.AllEmployeeName = data;

                        }
                    }
                }

                return View(EmployeeResignation);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsFnfResignationExists
        [HttpGet]
        public JsonResult IsFnfResignationExists(string FnfId_Encrypted,int FnfEmployeeId)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_FnfEmployeeId", FnfEmployeeId);
                    param.Add("@p_FnfId_Encrypted", FnfId_Encrypted);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_FNF_EmployeeResignationByHR", param);
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
        #endregion

        #region SaveUpdate
        [HttpPost]
       // [ValidateInput(false)]
        public ActionResult SaveUpdate(FNF_EmployeeResignation EmployeeResignation)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(EmployeeResignation.FnfId_Encrypted) ? "Save" : "Update");
                param.Add("@P_FnfID", EmployeeResignation.FnfId);
                param.Add("@P_FnfID_Encrypted", EmployeeResignation.FnfId_Encrypted);
                param.Add("@P_CmpId", EmployeeResignation.CmpId);
                param.Add("@P_BranchId", EmployeeResignation.FnfBranchID);
                param.Add("@P_FnfEmployeeId", EmployeeResignation.FnfEmployeeId);
                param.Add("@P_FnfReasonId", EmployeeResignation.FnfReasonId);
                param.Add("@p_ResignationTypeId", EmployeeResignation.ResignationTypeId);
                param.Add("@P_Remark", EmployeeResignation.Remark);
                param.Add("@P_ResignationDate", Convert.ToDateTime(EmployeeResignation.ResignationDate));
                param.Add("@P_RelievingDate", EmployeeResignation.RelievingDate);
                param.Add("@p_LastWorkingDate", Convert.ToDateTime(EmployeeResignation.LastWorkingDate));
                param.Add("@p_NoticePeriodEndDate", Convert.ToDateTime(EmployeeResignation.NoticePeriodEndDate));
                param.Add("@P_NoticePeriodDays", EmployeeResignation.NoticePeriodMonthDays);
                param.Add("@p_NoticePeriodType", EmployeeResignation.NoticePeriodType);
                param.Add("@p_IsCalculated", 1);
                param.Add("@p_IsManually", EmployeeResignation.IsManually);
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_FNF_EmployeeResignationByHR", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                
                return RedirectToAction("ESS_FNF_EmployeeResignation", "ESS_FNF_EmployeeResignation");
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_FNF_EmployeeResignation");
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
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 624;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_FnfId_Encrypted", "List");
                var FNFEmployeRegignationList = DapperORM.ExecuteSP<dynamic>("sp_List_FNF_EmployeeResignationByHRlist", param).ToList();
                ViewBag.ResignationList = FNFEmployeRegignationList;
                return View();
                
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_FNF_EmployeeResignation ");
            }

        }
        #endregion

        #region Delete
        [HttpGet]
        public ActionResult Delete(string FnfId)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_FnfId", FnfId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_FNF_EmployeeResignationByHR", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_FNF_EmployeeResignation");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
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
                var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                return Json(new { BranchName }, JsonRequestBehavior.AllowGet);
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


                param.Add("@p_Employeeid", EmployeeId);
                var info = DapperORM.ExecuteSP<dynamic>("sp_FNF_GetEmployeeDetails", param).FirstOrDefault();
                return Json(new { info = info }, JsonRequestBehavior.AllowGet);
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
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_BranchId", BranchId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetEmployeeDropdownForResignation", param).ToList();

                //DynamicParameters param = new DynamicParameters();
                //param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and  EmployeeBranchId=" + BranchId + "  and ContractorId=1 order by Name");
                //var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
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
                MonthDays.Add("@query", $@"Select Id, MonthDays as Name from tbl_NoticePeriodType_MonthDays where Type='{NoticePeriodType}' ORDER BY MonthDays");
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
    }
}