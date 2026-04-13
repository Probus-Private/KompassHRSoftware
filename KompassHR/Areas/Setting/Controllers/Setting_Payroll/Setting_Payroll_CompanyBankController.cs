using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_Payroll;
using KompassHR.Models;
using System.Net;
using System.Data;
using System.Data.SqlClient;
using System.Text;
namespace KompassHR.Areas.Setting.Controllers.Setting_Payroll
{
    public class Setting_Payroll_CompanyBankController : Controller
    {
        // GET: Setting/Setting_Payroll_CompanyBank
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        StringBuilder strBuilder = new StringBuilder();

        #region Main View
        public ActionResult Setting_Payroll_CompanyBank(string CompanyBankId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 495;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                //GET COMPANY NAME
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                ViewBag.BranchName = "";
                ViewBag.EmployeeName = "";

                Payroll_Company_Bank Payroll_Company_Bank = new Payroll_Company_Bank();

                if (CompanyBankId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";

                    param = new DynamicParameters();
                    param.Add("@p_CompanyBankId_Encrypted", CompanyBankId_Encrypted);
                    Payroll_Company_Bank = DapperORM.ReturnList<Payroll_Company_Bank>("sp_List_Payrol_CompanyBank", param).FirstOrDefault();

                    var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(Payroll_Company_Bank.CompanyBankCmpID), Convert.ToInt32(Session["EmployeeId"]));
                    ViewBag.BranchName = Branch;

                }
                return View(Payroll_Company_Bank);

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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 495;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@P_CompanyBankId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Payrol_CompanyBank", param).ToList();
                ViewBag.CompanyBankList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsValidaton
        [HttpGet]
        public ActionResult IsAccountNoExists(string AccountNo,string CompanyBankId_Encrypted,int CmpID,int BranchID)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_CompanyBankId_Encrypted", CompanyBankId_Encrypted);
                param.Add("@p_AccountNo", AccountNo);
                param.Add("@p_CompanyBankCmpID", CmpID);
                param.Add("@p_CompanyBankBUId", BranchID);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payrol_CompanyBank", param);
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
        public ActionResult SaveUpdate(Payroll_Company_Bank CompanyBank)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(CompanyBank.CompanyBankId_Encrypted) ? "Save" : "Update");
                param.Add("@P_CompanyBankId", CompanyBank.CompanyBankId);
                param.Add("@P_CompanyBankId_Encrypted", CompanyBank.CompanyBankId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);

                param.Add("@P_CompanyBankCmpID", CompanyBank.CompanyBankCmpID);
                param.Add("@P_CompanyBankBUId", CompanyBank.CompanyBankBUId);
                param.Add("@P_BankName", CompanyBank.BankName);
                param.Add("@P_BranchName", CompanyBank.BranchName);
                param.Add("@P_IFSCCode", CompanyBank.IFSCCode);
                param.Add("@P_AccountNo", CompanyBank.AccountNo);
                param.Add("@P_IsDefault", CompanyBank.IsDefault);

                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payrol_CompanyBank", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Payroll_CompanyBank", "Setting_Payroll_CompanyBank");
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
                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CmpId), Convert.ToInt32(Session["EmployeeId"]));
                return Json(new { Branch = Branch }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region Delete
        [HttpGet]
        public ActionResult Delete(string CompanyBankId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@P_CompanyBankId_Encrypted", CompanyBankId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payrol_CompanyBank", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Payroll_CompanyBank");
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