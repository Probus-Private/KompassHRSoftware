using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_FullAndFinal;
using KompassHR.Models;
using System.Net;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace KompassHR.Areas.Setting.Controllers
{
    public class Setting_FullAndFinal_DuesMappingController : Controller
    {
        // GET: Setting/Setting_FullAndFinal_DuesMapping

        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        StringBuilder strBuilder = new StringBuilder();


        public ActionResult Setting_FullAndFinal_DuesMapping(string FNFNoDuesMappingID_Encrypted)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 478;
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


                param.Add("@query", "select FNFNoDuesId as Id, NoDuesAndClearenceTitle as [Name] from FNF_DuesAndClearence_Master where Deactivate=0");
                var list_NoDuesName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.NoDuesName = list_NoDuesName;

                FNF_DuesMapping FNF_DuesMapping = new FNF_DuesMapping();

                if (FNFNoDuesMappingID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";

                    param = new DynamicParameters();
                    param.Add("@p_FNFNoDuesMappingID_Encrypted", FNFNoDuesMappingID_Encrypted);
                    FNF_DuesMapping = DapperORM.ReturnList<FNF_DuesMapping>("sp_List_FNF_NoDuesMapping", param).FirstOrDefault();

                    var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(FNF_DuesMapping.CompanyID), Convert.ToInt32(Session["EmployeeId"]));
                    ViewBag.BranchName = Branch;

                    var EmployeeName = new BulkAccessClass().GetEmployeeName(Convert.ToInt32(FNF_DuesMapping.BusinessUnitID));
                    ViewBag.EmployeeName = EmployeeName;
                }
                return View(FNF_DuesMapping);
                
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

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

                param.Add("@P_FNFNoDuesMappingID_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_FNF_NoDuesMapping", param).ToList();
                ViewBag.GetNoDuesCheckListList = data;
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
        public ActionResult IsNoDuesCheckListExists(int FNFNoDuesId,int CompanyID, int BusinessUnitID, string FNFNoDuesMappingID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_FNFNoDuesMappingID_Encrypted", FNFNoDuesMappingID_Encrypted);
                param.Add("@P_FNFNoDuesId", FNFNoDuesId);
                param.Add("@P_CompanyID", CompanyID);
                param.Add("@P_BusinessUnitID", BusinessUnitID);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_FNF_NoDuesMapping", param);
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
        public ActionResult SaveUpdate(FNF_DuesMapping DuesMapping)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(DuesMapping.FNFNoDuesMappingID_Encrypted) ? "Save" : "Update");
                param.Add("@P_FNFNoDuesMappingID", DuesMapping.FNFNoDuesMappingID);
                param.Add("@P_FNFNoDuesMappingID_Encrypted", DuesMapping.FNFNoDuesMappingID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@P_CompanyID", DuesMapping.CompanyID);
                param.Add("@P_BusinessUnitID", DuesMapping.BusinessUnitID);
                param.Add("@P_EmployeeID", DuesMapping.EmployeeID);
                param.Add("@P_FNFNoDuesId", DuesMapping.FNFNoDuesId);

                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_FNF_NoDuesMapping]", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_FullAndFinal_DuesMapping", "Setting_FullAndFinal_DuesMapping");
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

        #region Delete
        [HttpGet]
        public ActionResult Delete(string FNFNoDuesMappingID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@P_FNFNoDuesMappingID_Encrypted", FNFNoDuesMappingID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_FNF_NoDuesMapping", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_FullAndFinal_DuesMapping");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region view
        public ActionResult Viewa(string FNFNoDuesMappingID)
        {
            try
            {
                var que = DapperORM.DynamicQuerySingle("select RequiredClearanceName from FNF_DuesAndClearence_Details where NoDuesAndClearenceTitle_MasterID=" + FNFNoDuesMappingID + " and Deactivate=0");
                return Json(que, JsonRequestBehavior.AllowGet);
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
