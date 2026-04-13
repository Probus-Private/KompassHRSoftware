using Dapper;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_AccountAndFinance;
using System;
using KompassHR.Models;
using System.Linq;
using System.Net;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace KompassHR.Areas.Setting.Controllers.Setting_AccountAndFinance
{
    public class Setting_AccountAndFinance_IFSCCodeController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: AccountAndFinance/IFSCCode
        #region Setting_AccountAndFinance_IFSCCode
        [HttpGet]
        public ActionResult Setting_AccountAndFinance_IFSCCode(string IFSCID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Mas_IFSCCode mas_BankIFSCcode = new Mas_IFSCCode();
                param.Add("@query", "Select BankId,BankName from Mas_Bank Where Deactivate=0");
                var listMas_Bankmaster = DapperORM.ReturnList<Mas_Bank>("sp_QueryExcution", param).ToList();
                ViewBag.GetBankName = listMas_Bankmaster;

                if (IFSCID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_IFSCID_Encrypted", IFSCID_Encrypted);
                    mas_BankIFSCcode = DapperORM.ReturnList<Mas_IFSCCode>("sp_List_mas_IFSCCode", param).FirstOrDefault();
                }
                return View(mas_BankIFSCcode);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsIFSCodeExists
        public ActionResult IsIFSCodeExists(string IFSCode, double IFSCCodeBankID, string BranchName, string IFSCID_Encrypted, string Address, string City, string District, string StateName)
        {
            try
            {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_IFSCID_Encrypted", IFSCID_Encrypted);
                    param.Add("@p_IFSCode", IFSCode);
                    param.Add("@p_IFSCCodeBankID", IFSCCodeBankID);
                    param.Add("@p_BranchName", BranchName);
                    param.Add("@p_Address", Address);
                    param.Add("@p_City", City);
                    param.Add("@p_District", District);
                    param.Add("@p_StateName", StateName);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_IFSCCode", param);
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

        #region SaveUpdateIFSCCode
        [HttpPost]
        public ActionResult SaveUpdateIFSCCode(Mas_IFSCCode IFSCCode)
        {
            try
            {

                param.Add("@p_process", string.IsNullOrEmpty(IFSCCode.IFSCID_Encrypted) ? "Save" : "Update");
                param.Add("@p_IFSCID", IFSCCode.IFSCID);
                param.Add("@p_IFSCID_Encrypted", IFSCCode.IFSCID_Encrypted);
                param.Add("@p_IFSCCodeBankID", IFSCCode.IFSCCodeBankID);
                param.Add("@p_IFSCode", IFSCCode.IFSCode);
                param.Add("@p_BranchName", IFSCCode.BranchName);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_Address", IFSCCode.Address);
                param.Add("@p_City", IFSCCode.City);
                param.Add("@p_District", IFSCCode.District);
                param.Add("@p_StateName", IFSCCode.StateName);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_IFSCCode]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Setting_AccountAndFinance_IFSCCode", "Setting_AccountAndFinance_IFSCCode");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
       
        public ActionResult GetList(Mas_Bank BankMass)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@query", "Select BankId,BankName from Mas_Bank Where Deactivate=0");
                var listMas_Bankmaster = DapperORM.ReturnList<Mas_Bank>("sp_QueryExcution", param).ToList();
                ViewBag.GetBankName = listMas_Bankmaster;
                if(BankMass.BankId!=0)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_IFSCID_Encrypted", "List");
                    paramList.Add("@p_BankID", BankMass.BankId);
                    var data = DapperORM.ReturnList<Mas_IFSCCode>("sp_List_mas_IFSCCode", paramList).ToList();
                    ViewBag.GetIFSCCodeList = data;
                }
                else
                {
                    ViewBag.GetIFSCCodeList = "";
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

        //public ActionResult GetIFSCCode(int BankID)
        //{
        //    try
        //    {
        //        param.Add("@p_IFSCID_Encrypted", "List");
        //        param.Add("@p_BankID", BankID);
        //        var data = DapperORM.ReturnList<Mas_IFSCCode>("sp_List_mas_IFSCCode", param).ToList();
        //        return Json(data, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
          
        //}

        #region Delete
        [HttpGet]
        public ActionResult Delete(string IFSCID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_IFSCID_Encrypted", IFSCID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_IFSCCode", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("GetList", "Setting_AccountAndFinance_IFSCCode");
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