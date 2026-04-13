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
    public class Setting_TimeOffice_CoffGenerationDaysController : Controller
    {
        #region Setting_TimeOffice_CoffGenerationDays Main View 
        // GET: Setting/Setting_TimeOffice_CoffGenerationDays
        public ActionResult Setting_TimeOffice_CoffGenerationDays(Atten_CoffGenerationDays  CoffGenerationDays)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 870;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";

                //GET COMPANY NAME
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                var CMPID = GetComapnyName[0].Id;
                
                if (CoffGenerationDays.CmpId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select BranchId As Id,BranchName As Name From Mas_Branch where Deactivate=0 And CmpId='" + CoffGenerationDays.CmpId + "' order by Name");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    ViewBag.BranchName = data;
                }
                else
                {
                    ViewBag.BranchName = "";
                }
                //if (CoffGenerationDays.COFFGenerationDaysId_Encrypted != null)
                //{
                //    ViewBag.AddUpdateTitle = "Update";
                //    DynamicParameters param = new DynamicParameters();
                //    param.Add("@p_COFFGenerationDaysId_Encrypted", COFFGenerationDaysId_Encrypted);
                //    CoffGeneration = DapperORM.ReturnList<Atten_CoffGenerationDays>("sp_List_RegulizationSetting", param).FirstOrDefault();

                //}
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Get Branch Name
        [HttpGet]
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters paramBranchName = new DynamicParameters();
                paramBranchName.Add("@query", "select BranchId as Id, BranchName as Name  from Mas_Branch where Deactivate = 0 and CmpId ='" + CmpId + "'  order by Name");
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranchName).ToList();
                ViewBag.BranchName = Branch;
                
                return Json(new { Branch = Branch}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
        
        #region IsValidation
        public ActionResult IsExists(string COFFGenerationDaysId_Encrypted,int? CmpId,int? COFFGenerationDaysBranchId,List<double> COFFGenerationDay)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                
                    if (COFFGenerationDay == null || COFFGenerationDay.Count == 0)
                    {
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                    foreach (var day in COFFGenerationDay)
                    {
                        DynamicParameters param = new DynamicParameters();
                        param.Add("@p_process", "IsValidation");
                        param.Add("@p_COFFGenerationDaysId_Encrypted", COFFGenerationDaysId_Encrypted);
                        param.Add("@p_CmpId", CmpId);
                        param.Add("@p_COFFGenerationDaysBranchId", COFFGenerationDaysBranchId);
                        param.Add("@p_COFFGenerationDay", day);
                        param.Add("@p_MachineName", Dns.GetHostName().ToString());
                        param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                        // param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                        // param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                        param.Add("@p_msg", null, DbType.String, ParameterDirection.Output, size: 500);
                        param.Add("@p_Icon", null, DbType.String, ParameterDirection.Output, size: 500);

                        DapperORM.ExecuteReturn("sp_SUD_Atten_CoffGenerationDays", param);

                        var Message = param.Get<string>("@p_msg");
                        var Icon = param.Get<string>("@p_Icon");
                        if (!string.IsNullOrEmpty(Message) && Icon == "error")
                        {
                            return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                        }
                    }

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
        public ActionResult SaveUpdate(Atten_CoffGenerationDays obj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var COFFGenerationDays = Request.Form.GetValues("COFFGenerationDay");
                string lastMessage = "";
                string lastIcon = "";

                if (COFFGenerationDays != null)
                {
                    foreach (var day in COFFGenerationDays)
                {
                 DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", string.IsNullOrEmpty(obj.COFFGenerationDaysId_Encrypted) ? "Save" : "Update");
                param.Add("@p_COFFGenerationDaysId_Encrypted", obj.COFFGenerationDaysId_Encrypted);
                param.Add("@p_CmpId", obj.CmpId);
                param.Add("@p_COFFGenerationDaysBranchId", obj.COFFGenerationDaysBranchId);
                param.Add("@p_COFFGenerationDay", day);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Atten_CoffGenerationDays", param);
                        lastMessage = param.Get<string>("@p_msg");
                        lastIcon = param.Get<string>("@p_Icon");
                    }
                }

                //  TempData["Message"] = param.Get<string>("@p_msg");
                //  TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["Message"] = lastMessage;
                TempData["Icon"] = lastIcon;
                return RedirectToAction("Setting_TimeOffice_CoffGenerationDays", "Setting_TimeOffice_CoffGenerationDays");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Coff Days
        public ActionResult GetCoffDays(int CmpId,int BranchId)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_CmpId", CmpId);
                param.Add("@p_BranchId", BranchId);
                var CoffDays = DapperORM.ReturnList<dynamic>("sp_GetCoffDays", param).ToList();
                return Json(CoffDays, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion
    }
}
