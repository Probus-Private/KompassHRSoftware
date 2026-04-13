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
    public class ESS_LoanAndAdvance_StopAdvanceController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_LoanAndAdvance_StopAdvance
        #region ESS_LoanAndAdvance_StopAdvance
        public ActionResult ESS_LoanAndAdvance_StopAdvance(Payroll_StopAdvance OBJStop)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 497;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Payroll_StopAdvance StopAdvanceSetting = new Payroll_StopAdvance();
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                ViewBag.GetCompanyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetBranchName = "";
                ViewBag.GetEmployeeLoad = "";

                if (OBJStop.CmpId != 0)
                {
                    DynamicParameters paramEmpName = new DynamicParameters();
                    paramEmpName.Add("@p_BranchId", OBJStop.BranchId);
                    paramEmpName.Add("@p_MonthYear", OBJStop.MonthYear.ToString("yyyy-MM-dd"));
                    ViewBag.GetEmployeeLoad = DapperORM.ExecuteSP<dynamic>("sp_Get_PayrollAdvanceEmployee", paramEmpName).ToList();

                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_employeeid", Session["EmployeeId"]);
                    param.Add("@p_CmpId", OBJStop.CmpId);
                    ViewBag.GetBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                }
                return View(StopAdvanceSetting);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 497;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_StopAdvanceID_Encrypted", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var data = DapperORM.DynamicList("sp_List_Payroll_StopAdvance", param);
                ViewBag.GetAdvanceList = data;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_LoanAndAdvance_Advance");
            }
        }
        #endregion

        #region IsVerification
        public JsonResult IsStopAdvanceExists(List<Payroll_StopAdvance_Bulk> Obj_Bulk, int CmpId, int BranchID, string MonthYear)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                if (Obj_Bulk != null)
                {
                    foreach (var Data in Obj_Bulk)
                    {
                        //param.Add("@p_process", "Save");
                        param.Add("@p_CmpId", CmpId);
                        param.Add("@p_BranchId", BranchID);
                        param.Add("@p_StopAdvanceEmployeeID", Data.StopAdvanceEmployeeID);
                        param.Add("@p_MonthYear", DateTime.Parse(MonthYear));
                        param.Add("@p_Remark", Data.Remark);
                        param.Add("@p_AdvanceId", Data.AdvanceId);
                        param.Add("@p_MachineName", Dns.GetHostName().ToString());
                        param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_StopAdvance", param);
                        var Message = param.Get<string>("@p_msg");
                        var Icon = param.Get<string>("@p_Icon");
                        if (Icon == "error")
                        {
                            return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                return Json(true, JsonRequestBehavior.AllowGet);
                //var Message = param.Get<string>("@p_msg");
                //var Icon = param.Get<string>("@p_Icon");
                //if (Message != "")
                //{
                //    return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    return Json(true, JsonRequestBehavior.AllowGet);
                //}
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
        public ActionResult SaveUpdate(List<Payroll_StopAdvance_Bulk> Obj_Bulk, int CmpId, int BranchID, string MonthYear)
        {
            try
            {
                if (Obj_Bulk != null)
                {
                    foreach (var Data in Obj_Bulk)
                    {
                        param.Add("@p_process", "Save");
                        param.Add("@p_CmpId", CmpId);
                        param.Add("@p_BranchId", BranchID);
                        param.Add("@p_StopAdvanceEmployeeID", Data.StopAdvanceEmployeeID);
                        param.Add("@p_MonthYear", DateTime.Parse(MonthYear));
                        param.Add("@p_Remark", Data.Remark);
                        param.Add("@p_AdvanceId", Data.AdvanceId);
                        param.Add("@p_MachineName", Dns.GetHostName().ToString());
                        param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_StopAdvance", param);
                        TempData["Message"] = param.Get<string>("@p_msg");
                        TempData["Icon"] = param.Get<string>("@p_Icon");
                    }
                }
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_LoanAndAdvance_StopAdvance");
            }

        }
        #endregion

        #region Delete
        public ActionResult Delete(string StopAdvanceID_Encrypted, int? StopAdvanceEmployeeID)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_StopAdvanceID_Encrypted", StopAdvanceID_Encrypted);
                param.Add("@p_StopAdvanceEmployeeID", StopAdvanceEmployeeID);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_StopAdvance", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_LoanAndAdvance_StopAdvance");
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