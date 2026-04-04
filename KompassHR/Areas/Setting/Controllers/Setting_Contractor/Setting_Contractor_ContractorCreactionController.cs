using Dapper;
//using KompassHR.Areas.Setting.Models.Setting_ClaimReimbusement;
using KompassHR.Areas.Setting.Models.Setting_Contractor;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;


namespace KompassHR.Areas.Setting.Controllers.Setting_Contractor
{
    public class Setting_Contractor_ContractorCreactionController : Controller
    {
        // GET: ContractorSetting/ContractorCreaction
        DynamicParameters param = new DynamicParameters();
        #region Setting_Contractor_ContractorCreaction


        public ActionResult Setting_Contractor_ContractorCreaction(string ContractorId_Encrypted)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 111;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Contractor_Master ContractorCreaction = new Contractor_Master();


                if (ContractorId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_ContractorId_Encrypted", ContractorId_Encrypted);
                    ContractorCreaction = DapperORM.ReturnList<Contractor_Master>("sp_List_Contractor_Master", param).FirstOrDefault();
                    TempData["FromDate"] = ContractorCreaction.FromDate;
                    TempData["ToDate"] = ContractorCreaction.ToDate;
                }

                return View(ContractorCreaction);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region IsContractorCreactionExists



        public ActionResult IsContractorCreactionExists(string ContractorName, string ContractorId_Encrypted, string ContractorBranchID, string CmpId)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_ContractorName", ContractorName);
                    param.Add("@p_ContractorId_Encrypted", ContractorId_Encrypted);
                    //param.Add("@p_CmpId", CmpId);
                    //param.Add("@p_ContractorBranchID", ContractorBranchID);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Contractor_Master", param);
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
        public ActionResult SaveUpdate(Contractor_Master ContractorCreaction)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 111;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(ContractorCreaction.ContractorId_Encrypted) ? "Save" : "Update");
                param.Add("@p_ContractorId", ContractorCreaction.ContractorId);
                param.Add("@p_ContractorId_Encrypted", ContractorCreaction.ContractorId_Encrypted);
                param.Add("@p_ContractorName", ContractorCreaction.ContractorName);
                //param.Add("@p_CmpId", ContractorCreaction.CmpId);
                //param.Add("@p_ContractorBranchID", ContractorCreaction.ContractorBranchID);
                param.Add("@p_ContractorAddress", ContractorCreaction.ContractorAddress);
                param.Add("@p_FromDate", ContractorCreaction.FromDate);
                param.Add("@p_ToDate", ContractorCreaction.ToDate);
                param.Add("@p_ContactNo", ContractorCreaction.ContactNo);
                param.Add("@p_WhatsAppNo", ContractorCreaction.WhatsAppNo);
                param.Add("@p_ContactorEmailID", ContractorCreaction.ContactorEmailID);
                param.Add("@p_FaxNo", ContractorCreaction.FaxNo);
                param.Add("@p_ContactPersonName", ContractorCreaction.ContactPersonName);
                param.Add("@p_ContactPersonEmailID", ContractorCreaction.ContactPersonEmailID);
                param.Add("@p_ContactPersonContactNo", ContractorCreaction.ContactPersonContactNo);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Contractor_Master", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Contractor_ContractorCreaction", "Setting_Contractor_ContractorCreaction");
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
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 111;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_ContractorId_Encrypted", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var data = DapperORM.DynamicList("sp_List_Contractor_Master", param);
                ViewBag.GetContractorCreactionList = data;
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
        public ActionResult Delete(String ContractorId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 111;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_ContractorId_Encrypted", ContractorId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Contractor_Master", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Contractor_ContractorCreaction");
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