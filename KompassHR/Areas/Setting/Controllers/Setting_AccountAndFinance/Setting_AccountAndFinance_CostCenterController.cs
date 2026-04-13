using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_AccountAndFinance;
using System.Net;

namespace KompassHR.Areas.Setting.Controllers.Setting_AccountAndFinance
{
    public class Setting_AccountAndFinance_CostCenterController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: AccountAndFinance/CostCenter
        #region Setting_AccountAndFinance_CostCenter
        [HttpGet]
        public ActionResult Setting_AccountAndFinance_CostCenter(string CostCenterId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Mas_CostCenter mas_CostCenter = new Mas_CostCenter();
                param.Add("@query", "Select CompanyId,CompanyName from Mas_CompanyProfile Where Deactivate=0");
                var listMas_CompanyProfile = DapperORM.ReturnList<Mas_CompanyProfile>("sp_QueryExcution", param).ToList();
                ViewBag.GetCompanyname = listMas_CompanyProfile;

                if (CostCenterId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_CostCenterId_Encrypted", CostCenterId_Encrypted);
                    mas_CostCenter = DapperORM.ReturnList<Mas_CostCenter>("sp_List_Mas_CostCenter", param).FirstOrDefault();
                }
                return View(mas_CostCenter);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsCostCenterExists
        [HttpGet]
        public ActionResult IsCostCenterExists(int CmpID, string CostCenterName, string CostCenterId_Encrypted)
        {
            try
            {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_CostCenterName", CostCenterName);
                    param.Add("@p_CmpId", CmpID);
                    param.Add("@p_CostCenterId_Encrypted", CostCenterId_Encrypted);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_CostCenter", param);
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
        public ActionResult SaveUpdate(Mas_CostCenter CostCenter)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(CostCenter.CostCenterId_Encrypted) ? "Save" : "Update");
                param.Add("@p_CostCenterId", CostCenter.CostCenterId);
                param.Add("@p_CostCenterId_Encrypted", CostCenter.CostCenterId_Encrypted);
                param.Add("@p_CostCenterName", CostCenter.CostCenterName);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CmpId", CostCenter.CmpID);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_IsDefault", CostCenter.IsDefault);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_CostCenter]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Setting_AccountAndFinance_CostCenter", "Setting_AccountAndFinance_CostCenter");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region MyRegion
        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_CostCenterId_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Mas_CostCenter", param);
                ViewBag.GetCostCenterList = data;
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
        [HttpGet]
        public ActionResult Delete(string CostCenterId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_CostCenterId_Encrypted", CostCenterId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_CostCenter", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_AccountAndFinance_CostCenter");
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