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
    public class Module_Employee_BlockEmployeeController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Module/Module_Employee_BlockEmployee
        #region Module_Employee_BlockEmployee
        public ActionResult Module_Employee_BlockEmployee(string BlockEmployeeId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 403;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Mas_BlockEmployee BlockEmployee = new Mas_BlockEmployee();
                if (BlockEmployeeId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_BlockEmployeeId_Encrypted", BlockEmployeeId_Encrypted);
                    BlockEmployee = DapperORM.ReturnList<Mas_BlockEmployee>("sp_List_Mas_BlockEmployee", param).FirstOrDefault();
                }
                return View(BlockEmployee);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region BlockEmployeeList
        public ActionResult BlockEmployeeList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 403;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_BlockEmployeeId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Mas_BlockEmployee", param).ToList();
                ViewBag.BlockEmployeeList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsBlockEmployeeExists
        [HttpGet]
        public ActionResult IsBlockEmployeeExists(string AadharNo, string BlockEmployeeId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_AadharNo", AadharNo);
                param.Add("@p_BlockEmployeeId_Encrypted", BlockEmployeeId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_BlockEmployee", param);
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
        public ActionResult SaveUpdate(Mas_BlockEmployee GetBlockEmployee)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(GetBlockEmployee.BlockEmployeeId_Encrypted) ? "Save" : "Update");
                param.Add("@p_BlockEmployeeId", GetBlockEmployee.BlockEmployeeId);
                param.Add("@p_BlockEmployeeId_Encrypted", GetBlockEmployee.BlockEmployeeId_Encrypted);
                param.Add("@p_BlockEmployeeName", GetBlockEmployee.BlockEmployeeName);
                param.Add("@p_AadharNo", GetBlockEmployee.AadharNo);
                param.Add("@p_PanNo", GetBlockEmployee.PanNo);
                param.Add("@p_Reason", GetBlockEmployee.Reason);
                param.Add("@p_IsBlock", GetBlockEmployee.IsBlock);
                param.Add("@p_BlockedBy", Session["EmployeeId"]);
                //---------------------------------End Address Info--------------------------------
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                DapperORM.ExecuteReturn("sp_SUD_Mas_BlockEmployee", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Module_Employee_BlockEmployee", "Module_Employee_BlockEmployee");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(string BlockEmployeeId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_BlockEmployeeId_Encrypted", BlockEmployeeId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_BlockEmployee", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("BlockEmployeeList", "Module_Employee_BlockEmployee");
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