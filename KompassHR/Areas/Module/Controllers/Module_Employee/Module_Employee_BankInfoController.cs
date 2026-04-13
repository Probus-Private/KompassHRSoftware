using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_BankInfoController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Module/Module_Employee_BankInfo
        #region BankInfo Main View
        [HttpGet]
        public ActionResult Module_Employee_BankInfo(string Status)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 420;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Mas_Employee_Bank Mas_employeebank = new Mas_Employee_Bank();
                var countEMP = DapperORM.DynamicQuerySingle("Select count(EmployeeBankEmployeeId)  as EmployeeBankEmployeeId  from Mas_Employee_Bank where Mas_Employee_Bank.EmployeeBankEmployeeId=" + Session["OnboardEmployeeId"] + "");
                TempData["CountEmployeeBankEmployeeId"] = countEMP.EmployeeBankEmployeeId;
                var countPreBoard = DapperORM.DynamicQuerySingle("select PreboardingFid from Mas_Employee wheRE EmployeeId=" + Session["OnboardEmployeeId"] + "");
                TempData["PreBoardFid"] = countPreBoard?.PreboardingFid;
                if (Status == "Pre")
                {
                    var GetPreboardingFid = DapperORM.DynamicQuerySingle("Select PreboardingFid from Mas_Employee where employeeid=" + Session["OnboardEmployeeId"] + "");
                    var PreboardingFid = GetPreboardingFid.PreboardingFid;

                    param = new DynamicParameters();
                    param.Add("@p_FId", PreboardingFid);
                    Mas_employeebank = DapperORM.ReturnList<Mas_Employee_Bank>("sp_GetList_Mas_PreboardEmployeeDetails", param).FirstOrDefault();
                    ViewBag.GetEmployeePersonal = Mas_employeebank;
                   
                    return View(Mas_employeebank);
                }

                param = new DynamicParameters();
                TempData["OnboardEmployeeName"] = Session["OnboardEmployeeName"];
                param.Add("@p_EmployeeBankEmployeeId", Session["OnboardEmployeeId"]);
                Mas_employeebank = DapperORM.ReturnList<Mas_Employee_Bank>("sp_List_Mas_Employee_Bank", param).FirstOrDefault();
                if (Mas_employeebank != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                }
                return View(Mas_employeebank);
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
        public ActionResult SaveUpdate(Mas_Employee_Bank EmployeeBank)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(EmployeeBank.EmployeeBankId_Encrypted) ? "Save" : "Update");
                param.Add("@p_EmployeeBankId", EmployeeBank.EmployeeBankId);
                param.Add("@p_EmployeeBankId_Encrypted", EmployeeBank.EmployeeBankId_Encrypted);
                param.Add("@p_EmployeeBankEmployeeId", Session["OnboardEmployeeId"]);
                param.Add("@p_SalaryIFSC", EmployeeBank.SalaryIFSC);
                param.Add("@p_SalaryBankName", EmployeeBank.SalaryBankName);
                param.Add("@p_SalaryAccountNo", EmployeeBank.SalaryAccountNo);
                param.Add("@p_SalaryConfirmAccountNo", EmployeeBank.SalaryConfirmAccountNo);
                param.Add("@p_SalaryBankAddress", EmployeeBank.SalaryBankAddress);
                param.Add("@p_SalaryBankBranchName", EmployeeBank.SalaryBankBranchName);
                param.Add("@p_SalaryNameAsPerBank", EmployeeBank.SalaryNameAsPerBank);
                param.Add("@p_PermanentIFSC", EmployeeBank.PermanentIFSC);
                param.Add("@p_PermanentBankName", EmployeeBank.PermanentBankName);
                param.Add("@p_PermanentAccountNo", EmployeeBank.PermanentAccountNo);
                param.Add("@p_PermanentConfirmAccountNo", EmployeeBank.PermanentConfirmAccountNo);
                param.Add("@p_PermanentBankAddress", EmployeeBank.PermanentBankAddress);
                param.Add("@p_PermanentBankBranchName", EmployeeBank.PermanentBankBranchName);
                param.Add("@p_PermanentNameAsPerBank", EmployeeBank.PermanentNameAsPerBank);
                param.Add("@p_SalaryMode", EmployeeBank.SalaryMode);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_Bank]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_BankInfo", "Module_Employee_BankInfo");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetIFSCCode
        [HttpGet]
        public ActionResult GetIFSCCode(string IFSCode)
        {
            try
            {
                param.Add("@p_IFSCode", IFSCode);
                var GetIFSCCodelist = DapperORM.ExecuteSP<dynamic>("[sp_List_Mas_Employee_IFSCCode]", param).ToList();
                return Json(new { data = GetIFSCCodelist }, JsonRequestBehavior.AllowGet);
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