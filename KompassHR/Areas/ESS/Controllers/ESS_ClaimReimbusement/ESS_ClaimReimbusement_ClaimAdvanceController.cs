using Dapper;
using KompassHR.Areas.ESS.Models.ESS_ClaimReimbusement;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_ClaimReimbusement
{
    public class ESS_ClaimReimbusement_ClaimAdvanceController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_ClaimReimbusement_ClaimAdvance
        public ActionResult ESS_ClaimReimbusement_ClaimAdvance()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 916;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.GetClaim_Advance = "";
                ViewBag.List = "";
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region GetList
        public ActionResult GetList(string AdvanceClaimId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 916;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters ParamManager = new DynamicParameters();
                ParamManager.Add("@query", @"Select EmployeeId as Id , EmployeeName as Name from mas_employee_reporting,mas_employee
                                                where reportingmoduleid = 2 and ReportingEmployeeID = " + Session["EmployeeId"] + " and mas_employee_reporting.Deactivate = 0 and mas_employee_reporting.ReportingManager1 = mas_employee.EmployeeId");
                var Getdata = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", ParamManager);
                ViewBag.GetManagerEmployee = Getdata;

                DynamicParameters ClaimAdvance = new DynamicParameters();
                var EmployeeId = Session["EmployeeId"];
                ClaimAdvance.Add("@p_AdvanceClaimId_Encrypted", "List");
                ClaimAdvance.Add("@P_Qry", "and AdvanceClaimEmployeeId ='" + EmployeeId + "'");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Claim_Advance", ClaimAdvance).ToList();
                if (data != null)
                {
                    ViewBag.GetClaim_Advance = data;
                }
                else
                {
                    ViewBag.GetClaim_Advance = "";
                }

                return View();
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(List<Claim_Advance> updates)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 916;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters paramAdvanceClaim = new DynamicParameters();
                paramAdvanceClaim.Add("@p_process", "Save");
                paramAdvanceClaim.Add("@p_AdvanceClaimEmployeeId", Session["EmployeeId"]);
                paramAdvanceClaim.Add("@p_CreatedupdateBy", Session["EmployeeName"]);
                paramAdvanceClaim.Add("@p_MachineName", Dns.GetHostName().ToString());
                paramAdvanceClaim.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                paramAdvanceClaim.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                paramAdvanceClaim.Add("@p_Id", dbType: DbType.Int32, direction: ParameterDirection.Output, size: 500);//OutPut Parameter

                var data = DapperORM.ExecuteReturn("sp_SUD_Claim_Advance", paramAdvanceClaim);
                TempData["Message"] = paramAdvanceClaim.Get<string>("@p_msg");
                TempData["Icon"] = paramAdvanceClaim.Get<string>("@p_Icon");
                var AdvanceClaimId = paramAdvanceClaim.Get<int>("@p_Id");

                if (AdvanceClaimId !=null)
                {
                    for (int i = 0; i < updates.Count; i++)
                    {
                        DynamicParameters param = new DynamicParameters();
                        param.Add("@p_process", "Save");
                        param.Add("@p_AdvanceClaimId", AdvanceClaimId);
                        param.Add("@p_FromDate", updates[i].FromDate);
                        param.Add("@p_ToDate", updates[i].ToDate);
                        param.Add("@p_FromLocation", updates[i].FromLocation);
                        param.Add("@p_ToLocation", updates[i].ToLocation);
                        param.Add("@p_AdvanceClaimEmployeeId", Session["EmployeeId"]);
                        param.Add("@p_AdvaceClaimPurposeId", updates[i].AdvancePurposeId);
                        param.Add("@p_CreatedupdateBy", Session["EmployeeName"]);
                        param.Add("@p_MachineName", Dns.GetHostName().ToString());
                        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                        var data1 = DapperORM.ExecuteReturn("sp_SUD_Claim_AdvanceDetails", param);
                        TempData["Message"] = param.Get<string>("@p_msg");
                        TempData["Icon"] = param.Get<string>("@p_Icon");
                    }
                }   
                
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Icon = "error" }, JsonRequestBehavior.AllowGet);

            }
        }
        #endregion

        #region ShowClaimList
        [HttpGet]
        public ActionResult ShowAdvanceClaimDetails(int AdvanceClaimId)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            DynamicParameters param = new DynamicParameters();
           
            param.Add("@p_AdvanceClaimId", AdvanceClaimId);
            var data = DapperORM.ReturnList<dynamic>("sp_List_Claim_AdvanceDetails", param).ToList();
            string jsonData = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}