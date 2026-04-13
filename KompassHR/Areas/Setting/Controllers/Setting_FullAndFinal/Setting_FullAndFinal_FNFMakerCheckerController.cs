using Dapper;
using KompassHR.Areas.Setting.Models.Setting_FullAndFinal;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_FullAndFinal
{
    public class Setting_FullAndFinal_FNFMakerCheckerController : Controller
    {
        //DynamicParameters param = new DynamicParameters();
        //SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region Main View
        // GET: Setting/Setting_FullAndFinal_FNFMakerChecker
        public ActionResult Setting_FullAndFinal_FNFMakerChecker(string MakerChecker_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 846;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                FNFMakerChecker tblObj = new FNFMakerChecker();
                //GET COMPANY NAME
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                var CmpId = GetComapnyName[0].Id;

                if (tblObj.MakerCheckerCmpId != 0)
                {
                    DynamicParameters paramBranch = new DynamicParameters();
                    paramBranch.Add("@query", "select Distinct BranchId as Id,BranchName as Name from Mas_Branch where Deactivate=0 AND CmpId='" + tblObj.MakerCheckerCmpId + "';");
                    var branchList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranch).ToList();
                    ViewBag.BranchName = branchList;
                }
                else
                {
                    ViewBag.BranchName = new List<AllDropDownBind>();
                }

                DynamicParameters paramEmp = new DynamicParameters();
                paramEmp.Add("@p_EmployeeId", Session["EmployeeId"]);
                var EmployeeList = DapperORM.ReturnList<AllDropDownBind>("sp_ddl_GetFNFEmployee", paramEmp).ToList();
                ViewBag.EmployeeName = EmployeeList;

                if (MakerChecker_Encrypted != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_MakerChecker_Encrypted", MakerChecker_Encrypted);
                    tblObj = DapperORM.ReturnList<FNFMakerChecker>("sp_List_FNF_MakerChecker", param).FirstOrDefault();
                    if (!string.IsNullOrEmpty(tblObj.FNFMakerEmpId))
                    {
                        ViewBag.SelectedFNFMakerEmpIds = tblObj.FNFMakerEmpId.Split(',').Select(id => id.Trim()).ToList();
                    }
                    else
                    {
                        ViewBag.SelectedFNFMakerEmpIds = new List<string>();
                    }
                    var Branch1 = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(tblObj.MakerCheckerCmpId), Convert.ToInt32(Session["EmployeeId"]));
                    ViewBag.BranchName = Branch1;
                    return View(tblObj);
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
                DynamicParameters param = new DynamicParameters();

                DynamicParameters paramBranchName = new DynamicParameters();
                paramBranchName.Add("@query", "select BranchId as Id, BranchName as Name  from Mas_Branch where Deactivate = 0 and CmpId ='" + CmpId + "'  order by Name");
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranchName).ToList();
                ViewBag.BranchName = Branch;

                //  var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CmpId), Convert.ToInt32(Session["EmployeeId"]));
                return Json(new { Branch = Branch }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Get Employee Name
        [HttpGet]
        public ActionResult GetEmployeeName(int CmpId, int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_ddl_GetFNFEmployee", param).ToList();

                return Json(new { EmployeeName = EmployeeName }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsValidation
        public ActionResult IsExists(string MakerChecker_Encrypted, double CmpId, double? BranchId, double? FNFMakerEmpId, double? FNFCheckerEmpId, double? AccountEmpId, double? OtherEmpId)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_MakerChecker_Encrypted", MakerChecker_Encrypted);
                    param.Add("@p_MakerCheckerCmpId", CmpId);
                    param.Add("@p_MakerCheckerBranchId", BranchId);
                    // param.Add("@p_IsTopManagerApprove", IsTopManagerApprove);
                    //   param.Add("@p_TopManagerId", TopManagerId);
                    param.Add("@p_FNFMakerEmpId", FNFMakerEmpId);
                    param.Add("@p_FNFCheckerEmpId", FNFCheckerEmpId);
                    param.Add("@p_AccountEmpId", AccountEmpId);
                    param.Add("@p_OtherEmpId", OtherEmpId);
                    // param.Add("@p_IncrementMakerEmpId", IncrementMakerEmpId);
                    //  param.Add("@p_IncrementCheckerEmpId", IncrementCheckerEmpId);

                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_FNF_MakerChecker", param);
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
        public ActionResult SaveUpdate(FNFMakerChecker obj)
        {
            try
            {

                if (obj.MakerCheckerBranchId == 0)
                {

                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@Query", "SELECT BranchId FROM Mas_Branch WHERE Deactivate=0 And  CmpId = " + obj.MakerCheckerCmpId);
                   // var branchList = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param1).ToList();

                    var branchList = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param1).Select(x => (int)x.BranchId).ToList();
                    
                    string finalMessage = "";
                    string finalIcon = "";

                    foreach (int BranchId in branchList)

                    {
                        DynamicParameters param = new DynamicParameters();
                        param.Add("@p_process", string.IsNullOrEmpty(obj.MakerChecker_Encrypted) ? "Save" : "Update");
                        param.Add("@p_MakerChecker_Encrypted", obj.MakerChecker_Encrypted);
                        param.Add("@p_MakerCheckerCmpId", obj.MakerCheckerCmpId);
                        param.Add("@p_MakerCheckerBranchId", BranchId);
                        param.Add("@p_FNFMakerEmpId", obj.SelectedFNFMakerEmpIds);
                        param.Add("@p_FNFCheckerEmpId", obj.FNFCheckerEmpId);
                        param.Add("@p_AccountEmpId", obj.AccountEmpId);
                        param.Add("@p_OtherEmpId", obj.OtherEmpId);
                        param.Add("@p_MachineName", Dns.GetHostName().ToString());
                        param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);

                        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                        var data = DapperORM.ExecuteReturn("sp_SUD_FNF_MakerChecker", param);
                   
                        finalMessage = param.Get<string>("@p_msg");
                        finalIcon = param.Get<string>("@p_Icon");
                    }
                    
                    TempData["Message"] = finalMessage;
                    TempData["Icon"] = finalIcon;
                }
                
                else
                {

                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_process", string.IsNullOrEmpty(obj.MakerChecker_Encrypted) ? "Save" : "Update");
                    param.Add("@p_MakerChecker_Encrypted", obj.MakerChecker_Encrypted);
                    param.Add("@p_MakerCheckerCmpId", obj.MakerCheckerCmpId);
                    param.Add("@p_MakerCheckerBranchId", obj.MakerCheckerBranchId);
                    //  param.Add("@p_TopManagerId", 0);
                    // param.Add("@p_IsTopManagerApprove", 0);
                    // param.Add("@p_FNFMakerEmpId", obj.FNFMakerEmpId);
                    param.Add("@p_FNFMakerEmpId", obj.SelectedFNFMakerEmpIds);
                    param.Add("@p_FNFCheckerEmpId", obj.FNFCheckerEmpId);
                    param.Add("@p_AccountEmpId", obj.AccountEmpId);
                    param.Add("@p_OtherEmpId", obj.OtherEmpId);
                    //param.Add("@p_IncrementMakerEmpId", obj.IncrementMakerEmpId);
                    // param.Add("@p_IncrementCheckerEmpId", obj.IncrementCheckerEmpId);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);

                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_SUD_FNF_MakerChecker", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    //return RedirectToAction("Setting_FullAndFinal_FNFMakerChecker", "Setting_FullAndFinal_FNFMakerChecker");
                }
                return RedirectToAction("Setting_FullAndFinal_FNFMakerChecker", "Setting_FullAndFinal_FNFMakerChecker");

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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 500;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_MakerChecker_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_FNF_MakerChecker", param).ToList();
                ViewBag.ListDetails = data;
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
        public ActionResult Delete(string MakerChecker_Encrypted)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Delete");
                param.Add("@p_MakerChecker_Encrypted", MakerChecker_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_FNF_MakerChecker", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_FullAndFinal_FNFMakerChecker");
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