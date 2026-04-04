using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Setting.Models.Setting_IncomeTax;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_IncomeTax
{
    public class Setting_IncomeTax_TANBuMappingController : Controller
    {
        // GET: Setting/Setting_IncomeTax_TANBuMapping
        DynamicParameters param = new DynamicParameters();
        #region MainView
        public ActionResult Setting_IncomeTax_TANBuMapping(string TANBuMappingID_Encrypted,int? TANBuMappingCmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 596;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.CompanyName = GetComapnyName;
                IncomeTax_TANBuMapping income = new IncomeTax_TANBuMapping();

                if (TANBuMappingID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_TANBuMappingID_Encrypted", TANBuMappingID_Encrypted);
                    income = DapperORM.ReturnList<IncomeTax_TANBuMapping>("sp_List_IncomeTax_TANBuMapping", param).FirstOrDefault();

                    DynamicParameters paramBranch = new DynamicParameters();
                    paramBranch.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBranch.Add("@p_CmpId", TANBuMappingCmpId);
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranch).ToList();
                    ViewBag.BranchName = data;

                    DynamicParameters paramTAN = new DynamicParameters();
                    paramTAN.Add("@query", "Select DeductorResponsibleId as Id,DeductorTAN As Name  from IncomeTax_DeductorResponsible where Deactivate=0 and CmpId= " + TANBuMappingCmpId + " ");
                    ViewBag.GetTAN = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", paramTAN).ToList();
                }
                else
                {
                    ViewBag.BranchName = "";
                    ViewBag.GetTAN = "";
                }

                
               
                return View(income);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion MainView

        #region Isvalidation
        [HttpGet]
        public ActionResult IsTANBUExists(int TANBuMappingBranchId, int TANId, string TANBuMappingID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "IsValidation");
                param.Add("@p_TANBuMappingBranchId", TANBuMappingBranchId);
                param.Add("@p_TANId", TANId);
                param.Add("@p_TANBuMappingID_Encrypted", TANBuMappingID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_TANBuMapping", param);
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
        public ActionResult SaveUpdate(IncomeTax_TANBuMapping TANBuMapping)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(TANBuMapping.TANBuMappingID_Encrypted) ? "Save" : "Update");
                param.Add("@P_TANBuMappingID_Encrypted", TANBuMapping.TANBuMappingID_Encrypted);
                param.Add("@p_TANBuMappingCmpId", TANBuMapping.TANBuMappingCmpId);
                param.Add("@p_TANBuMappingBranchId", TANBuMapping.TANBuMappingBranchId);
                param.Add("@p_TANId", TANBuMapping.TANId);
               
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_TANBuMapping", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_IncomeTax_TANBuMapping", "Setting_IncomeTax_TANBuMapping");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 596;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_TANBuMappingID_Encrypted", "List");
                var data = DapperORM.ReturnList<dynamic>("sp_List_IncomeTax_TANBuMapping", param).ToList();
                ViewBag.TANBuMappingList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion GetList

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

                DynamicParameters paramTAN = new DynamicParameters();
                paramTAN.Add("@query", "Select DeductorResponsibleId as Id,DeductorTAN As Name  from IncomeTax_DeductorResponsible where Deactivate=0 and CmpId= "+ CmpId +" ");
                var TAN = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", paramTAN).ToList();

                return Json(new { data, TAN } , JsonRequestBehavior.AllowGet);
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
        public ActionResult Delete(string TANBuMappingID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_TANBuMappingID_Encrypted", TANBuMappingID_Encrypted);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_TANBuMapping", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_IncomeTax_TANBuMapping", new { Area = "Setting" });
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