using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_FullAndFinal;
using KompassHR.Models;
using Dapper;
using System.Net;
using System.Data;
using System.Data.SqlClient;

namespace KompassHR.Areas.Setting.Controllers.Setting_FullAndFinal
{
    public class Setting_FullAndFinal_TermsAndConditionController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_FullAndFinal_TermsAndCondition
        public ActionResult Setting_FullAndFinal_TermsAndCondition(string FNFTermsAndConditionID_Encrypted) //485
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 485;
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

                FNF_TermsAndCondition FNF_TermsAndCondition = new FNF_TermsAndCondition();

                if (FNFTermsAndConditionID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";

                    param = new DynamicParameters();
                    param.Add("@P_FNFTermsAndConditionID_Encrypted", FNFTermsAndConditionID_Encrypted);
                    FNF_TermsAndCondition = DapperORM.ReturnList<FNF_TermsAndCondition>("sp_List_FNF_TermsAndCondition", param).FirstOrDefault();

                    var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(FNF_TermsAndCondition.CompanyID), Convert.ToInt32(Session["EmployeeId"]));
                    ViewBag.BranchName = Branch;

                    var EmployeeName = new BulkAccessClass().GetEmployeeName(Convert.ToInt32(FNF_TermsAndCondition.BusinessUnitID));
                    ViewBag.EmployeeName = EmployeeName;
                    return View(FNF_TermsAndCondition);
                }
                return View();

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #region IsValidaton
        [HttpGet]
        public ActionResult IsNoDuesCheckListExists(int CompanyID,int BusinessUnitID, string FNFTermsAndConditionID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@P_FNFTermsAndConditionID_Encrypted", FNFTermsAndConditionID_Encrypted);
                param.Add("@P_CompanyID", CompanyID);
                param.Add("@P_BusinessUnitID", BusinessUnitID);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_FNF_TermsAndCondition", param);
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
        [ValidateInput(false)]
        public ActionResult SaveUpdate(FNF_TermsAndCondition OBJTermsAndCondition, string TermsAndConditionName)
        {
            try
           {

                param.Add("@p_process", string.IsNullOrEmpty(OBJTermsAndCondition.FNFTermsAndConditionID_Encrypted) ? "Save" : "Update");
                param.Add("@P_FNFTermsAndConditionID", OBJTermsAndCondition.FNFTermsAndConditionID);
                param.Add("@P_FNFTermsAndConditionID_Encrypted", OBJTermsAndCondition.FNFTermsAndConditionID_Encrypted);
                param.Add("@P_TermsAndConditionName", TermsAndConditionName);
                param.Add("@P_CompanyID", OBJTermsAndCondition.CompanyID);
                param.Add("@P_BusinessUnitID", OBJTermsAndCondition. BusinessUnitID);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_FNF_TermsAndCondition", param);
                var PID = param.Get<string>("@p_Id");
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                //return Json(new { message, Icon }, JsonRequestBehavior.AllowGet);
                ////return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                return RedirectToAction("Setting_FullAndFinal_TermsAndCondition", "Setting_FullAndFinal_TermsAndCondition");

            }




            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }


        }
        #endregion

        #region GetList Main View
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 71;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@P_FNFTermsAndConditionID_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_FNF_TermsAndCondition", param).ToList();
                ViewBag.GetTermsAndConditionList = data;
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
        public ActionResult Delete(string FNFTermsAndConditionID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_FNFTermsAndConditionID_Encrypted", FNFTermsAndConditionID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_FNF_TermsAndCondition", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_FullAndFinal_TermsAndCondition");
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

        #region GetEmployeeName
        [HttpGet]
        public ActionResult GetEmployeeName(int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var EmployeeName = new BulkAccessClass().GetEmployeeName(Convert.ToInt32(BranchId));
                return Json(new { EmployeeName = EmployeeName }, JsonRequestBehavior.AllowGet);
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