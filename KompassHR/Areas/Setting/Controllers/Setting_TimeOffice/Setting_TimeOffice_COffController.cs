using Dapper;
using KompassHR.Areas.Setting.Models.Setting_TimeOffice;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;


namespace KompassHR.Areas.Setting.Controllers.Setting_TimeOffice
{
    public class Setting_TimeOffice_COffController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        #region COff Main View
        [HttpGet]
        public ActionResult Setting_TimeOffice_COff(string CoffID_Encrypted,int? CmpID)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();

                Atten_CoffSetting coff_setting = new Atten_CoffSetting();              
                param.Add("@query", "Select CompanyId As Id, CompanyName As Name from Mas_CompanyProfile where Deactivate=0 order by Name");
                var GetCompanyName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.CompanyName = GetCompanyName;
                ViewBag.AddUpdateTitle = "Add";

                if (CmpID != null)
                {
                    DynamicParameters ParamBranch = new DynamicParameters();
                    ParamBranch.Add("@p_employeeid", Session["EmployeeId"]);
                    ParamBranch.Add("@p_CmpId", CmpID);
                    ViewBag.Location = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", ParamBranch).ToList();
                }
                else
                {
                    ViewBag.Location = "";
                }


                param = new DynamicParameters();
                if (CoffID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_CoffID_Encrypted", CoffID_Encrypted);
                    coff_setting = DapperORM.ReturnList<Atten_CoffSetting>("sp_List_Atten_CoffSetting", param).FirstOrDefault();
                }

                return View(coff_setting);
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
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion


        #region IsValidaiton
        public ActionResult IsAttenCoffExists(string CoffIDEncrypted, string CoffTypeName)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "IsValidation");
                    param.Add("@p_CoffID_Encrypted", CoffIDEncrypted);
                    param.Add("@p_CoffTypeName", CoffTypeName);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_CoffSetting", param);
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
        public ActionResult SaveUpdate(Atten_CoffSetting coff)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(coff.CoffID_Encrypted) ? "Save" : "Update");
                param.Add("@p_CoffID", coff.CoffID);
                param.Add("@p_CoffID_Encrypted", coff.CoffID_Encrypted);
                param.Add("@p_BranchID", coff.BranchID);
                param.Add("@p_CoffTypeName", coff.CoffTypeName);
                param.Add("@p_CoffRegularDay", coff.CoffRegularDay);
                param.Add("@p_CoffRegularCoff0_5Day_Min", coff.CoffRegularCoff0_5Day_Min);
                param.Add("@p_CoffRegularCoff0_5Day_Max", coff.CoffRegularCoff0_5Day_Max);
                param.Add("@p_CoffRegularCoff1Day_Min", coff.CoffRegularCoff1Day_Min);
                param.Add("@p_CoffRegularCoff1Day_Max", coff.CoffRegularCoff1Day_Max);
                param.Add("@p_CoffRegularCoff1_5Day_Min", coff.CoffRegularCoff1_5Day_Min);
                param.Add("@p_CoffRegularCoff1_5Day_Max", coff.CoffRegularCoff1_5Day_Max);
                param.Add("@p_CoffRegularCoff2Day_Min", coff.CoffRegularCoff2Day_Min);
                param.Add("@p_CoffRegularCoff2Day_Max", coff.CoffRegularCoff2Day_Max);
                param.Add("@p_CoffWOPHCoff0_5day_Min", coff.CoffWOPHCoff0_5day_Min);
                param.Add("@p_CoffWOPHCoff0_5day_Max", coff.CoffWOPHCoff0_5day_Max);
                param.Add("@p_CoffWOPHCoff1day_Min", coff.CoffWOPHCoff1day_Min);
                param.Add("@p_CoffWOPHCoff1day_Max", coff.CoffWOPHCoff1day_Max);
                param.Add("@p_CoffWOPHCoff1_5day_Min", coff.CoffWOPHCoff1_5day_Min);
                param.Add("@p_CoffWOPHCoff1_5day_Max", coff.CoffWOPHCoff1_5day_Max);
                param.Add("@p_CoffWOPHCoff2day_Min", coff.CoffWOPHCoff2day_Min);
                param.Add("@p_CoffWOPHCoff2day_Max", coff.CoffWOPHCoff2day_Max);
                param.Add("@p_CoffVaild", coff.CoffVaild);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_CmpID", coff.CmpID);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Atten_CoffSetting", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return RedirectToAction("Setting_TimeOffice_COff", "Setting_TimeOffice_COff");
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
                param.Add("@p_CoffID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Atten_CoffSetting", param);
                ViewBag.GetCOffSettingList = data;
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
        public ActionResult Delete(string CoffID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_CoffID_Encrypted", CoffID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_CoffSetting", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_TimeOffice_COff");
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