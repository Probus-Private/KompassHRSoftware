using Dapper;
using KompassHR.Areas.ESS.Models.ESS_FNF;
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
    public class ESS_FNF_FNFGratuityCalculationController : Controller
    {
        // GET: ESS_FNF_FNFLeaveCalculation
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region Gratuity Calculation main View
        [HttpGet]
        public ActionResult ESS_FNF_FNFGratuityCalculation(FNF_GratuityCalculation FNF_GratuityCalculation)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 617;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Gratuity Calculation main View
        [HttpGet]
        public ActionResult GratuityCalculation()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_EmployeeId", Session["FNFEmployeeId"]);
                var data = DapperORM.ReturnList<FNF_GratuityCalculation>("sp_FNF_GratuityCalculation", param).FirstOrDefault();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsFNF_GratuityCalculationExists
        [HttpGet]
        public ActionResult IsFNF_GratuityCalculationExists( string CurrentBasic_DA, string GratuityYear, string RountOfGratuityYear, string GratuityAmount ,string CauseOfExit)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_GratuityCalculationEmployeeId", Session["FNFEmployeeId"]);
                    param.Add("@p_CurrentBasic_DA", CurrentBasic_DA);
                    param.Add("@p_GratuityYear",GratuityYear);
                    param.Add("@p_RountOf_GratuityYear", RountOfGratuityYear);
                    param.Add("@p_GratuityAmount", GratuityAmount);
                    param.Add("@p_ExitCause", CauseOfExit);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_FNF_SUD_GratuityCalculation", param);
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
        public ActionResult SaveUpdate(FNF_GratuityCalculation FNF_GratuityCalculation)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                //string extension = Path.GetExtension(PhotoPath.FileName).ToLower();

                param.Add("@p_process", string.IsNullOrEmpty(FNF_GratuityCalculation.GratuityCalculationId_Encrypted) ? "Save" : "Update");
                param.Add("@P_GratuityCalculationId_Encrypted", FNF_GratuityCalculation.GratuityCalculationId_Encrypted);
                param.Add("@p_GratuityCalculationId", FNF_GratuityCalculation.GratuityCalculationId);
                param.Add("@p_GratuityCalculationEmployeeId", Session["FNFEmployeeId"]);
                param.Add("@p_CurrentBasic_DA", FNF_GratuityCalculation.CurrentBasic_DA);
                param.Add("@p_GratuityYear", FNF_GratuityCalculation.GratuityYear);
                param.Add("@p_RountOf_GratuityYear", FNF_GratuityCalculation.RountOf_GratuityYear);
                param.Add("@p_GratuityAmount", FNF_GratuityCalculation.GratuityAmount);
                param.Add("@p_ExitCause", FNF_GratuityCalculation.CauseOfExit);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
               
                var Result = DapperORM.ExecuteReturn("sp_FNF_SUD_GratuityCalculation", param);

                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                string TrainingInternalId = TempData["P_Id"]?.ToString() ?? string.Empty;
               
                    
                return RedirectToAction("ESS_FNF_FNFGratuityCalculation", "ESS_FNF_FNFGratuityCalculation");
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