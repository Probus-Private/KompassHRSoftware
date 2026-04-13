using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Prime;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;


namespace KompassHR.Areas.Setting.Controllers.Setting_Prime
{
    public class Setting_Prime_LocationController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        [HttpGet]
        #region Setting_Prime_Location
        public ActionResult Setting_Prime_Location(string BranchId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 5;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Mas_Branch mas_branch = new Mas_Branch();
                mas_branch.IsActive = true;
                param.Add("@query", "Select  CompanyId, CompanyName from Mas_CompanyProfile  where Deactivate=0 Order by CompanyName");
                var data = DapperORM.ReturnList<Mas_CompanyProfile>("sp_QueryExcution", param).ToList();
                ViewBag.MasCompanyProfileList = data;

                param.Add("@query", "select EmployeeId, EmployeeName from mas_employee where Deactivate =0 and employeeleft=0");
                var LocationHead = DapperORM.ReturnList<Mas_Employee>("sp_QueryExcution", param).ToList();
                ViewBag.LocationHeadList = LocationHead;


                param.Add("@query", "select StateId As Id, StateName As Name from Mas_States where Deactivate =0 ");
                var State = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.StateName = State;

                param.Add("@query", "select EmployeeId, EmployeeName from mas_employee where Deactivate =0 and employeeleft=0");
                var HrPerson = DapperORM.ReturnList<Mas_Employee>("sp_QueryExcution", param).ToList();
                ViewBag.HrPersonList = HrPerson;


                if (BranchId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_BranchId_Encrypted", BranchId_Encrypted);
                    mas_branch = DapperORM.ReturnList<Mas_Branch>("sp_List_Mas_Branch", param).FirstOrDefault();
                    mas_branch.IsActive = mas_branch.IsActive;
                }
                return View(mas_branch);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsLocationExists
        [HttpGet]
        public ActionResult IsLocationExists(double CompanyName, string BranchName, string BranchIdEncrypted)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_CmpId", CompanyName);
                    param.Add("@p_BranchName", BranchName);
                    param.Add("@p_BranchId_Encrypted", BranchIdEncrypted);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Branch", param);
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
        public ActionResult SaveUpdate(Mas_Branch masBranch)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(masBranch.BranchId_Encrypted) ? "Save" : "Update");
                param.Add("@p_BranchId_Encrypted", masBranch.BranchId_Encrypted);
                param.Add("@p_BranchId", masBranch.BranchId);
                param.Add("@p_CmpId", masBranch.CmpId);
                param.Add("@p_BranchName", masBranch.BranchName);
                param.Add("@p_BranchAddress", masBranch.BranchAddress);
                param.Add("@p_StateId", masBranch.StateId);
                param.Add("@p_PhoneNo", masBranch.PhoneNo);
                param.Add("@p_PlantHeadID", masBranch.PlantHeadID);
                param.Add("@p_HrPersonID", masBranch.HRPersonID);
                param.Add("@p_IsActive", masBranch.IsActive);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_Email", masBranch.Email);
                param.Add("@p_GMap_Coordinates", masBranch.GMap_Coordinates);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Branch", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Prime_Location", "Setting_Prime_Location");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 5;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_BranchId_Encrypted", "List");
                var data = DapperORM.ReturnList<Mas_Branch>("sp_List_Mas_Branch", param).ToList();
                ViewBag.CompanyLocationList = data;
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
        public ActionResult Delete(string BranchId_Encrypted, int BranchId)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_BranchId_Encrypted", BranchId_Encrypted);
                param.Add("@@p_BranchId", BranchId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Branch", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Prime_Location");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region BusinessUnitList
        public ActionResult BusinessUnitList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var cmpid = Session["CompanyId"];
                param.Add("@query", @"Select count(employeeid) as TotalEmployee,Mas_CompanyProfile.CompanyName, BranchName,	BranchAddress,	mas_branch.PhoneNo,	Email,	PlantHeadID,	HRPersonID,	CompanyId,	BranchID, IsNull(GMap_Coordinates,'' ) as GMap_Coordinates from  Mas_Employee 
                                    inner join mas_companyprofile on mas_companyprofile.companyid = maS_employee.cmpid
                                    inner join maS_branch on mas_branch.branchid = mas_employee.employeebranchid
                                    where Mas_Employee.Deactivate = 0 and Mas_Branch.Deactivate = 0 and EmployeeLeft = 0 and Mas_Employee.ContractorID = 1 group by BranchName,CompanyName, BranchAddress, mas_branch.PhoneNo, Email, PlantHeadID, HRPersonID,GMap_Coordinates, CompanyId, BranchID");
                var listMasCompanyProfile = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).ToList();
                ViewBag.GetBusinessUnitList = listMasCompanyProfile;


                return View();
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