using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using System.Data;
using System.Net;
using System.Data.SqlClient;
using KompassHR.Areas.ESS.Models.ESS_Tax;
namespace KompassHR.Areas.ESS.Controllers.ESS_Tax
{
    public class Ess_Tax_PanAdharUpdateController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/Ess_Tax_PanAdharUpdate
        #region Main View
        public ActionResult Ess_Tax_PanAdharUpdate()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 525;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
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

        #region SearchEmployee
        public ActionResult SearchEmployee(string EmployeeNo)
        {
            try
            {
                DynamicParameters param1 = new DynamicParameters();

                param1.Add("@p_EmployeeNo", EmployeeNo);
                var EmployeeDetails = DapperORM.ExecuteSP<dynamic>("sp_List_TaxEmployeeDetails", param1).ToList();
                ViewBag.EmployeeDetails = EmployeeDetails;

                return Json(EmployeeDetails, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region IsValidation
        [HttpGet]
        public ActionResult IsPANAaddhaarExists(string PAN, string Aadhaar, double EmployeeId)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_PAN", PAN);
                param.Add("@p_Aadhaar", Aadhaar);
                param.Add("@p_EmployeeId", EmployeeId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_PanAadhaarUpdate", param);
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
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(IncomeTax_InvestmentDeclaration_PanAdharUpdate PanAdharUpdate)
        {
            try
            {
                param.Add("@p_process", "Update");
                param.Add("@p_PAN", PanAdharUpdate.PAN);
                param.Add("@p_Aadhaar", PanAdharUpdate.AadhaarNo);
                param.Add("@p_PANAadhaarLink", PanAdharUpdate.PANAadhaarLink);
                param.Add("@p_EmployeeId", PanAdharUpdate.EmployeeId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_PanAadhaarUpdate", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Ess_Tax_PanAdharUpdate", "Ess_Tax_PanAdharUpdate");

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