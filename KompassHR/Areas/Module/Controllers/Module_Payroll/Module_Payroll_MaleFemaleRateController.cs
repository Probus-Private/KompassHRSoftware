using Dapper;
using KompassHR.Areas.Module.Models.Module_Payroll;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Payroll
{
    public class Module_Payroll_MaleFemaleRateController : Controller
    {
        clsCommonFunction objcon = new clsCommonFunction();
        #region Main View
        // GET: Module/Module_Payroll_MaleFemaleRate
        public ActionResult Module_Payroll_MaleFemaleRate(string MaleFemaleRateId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 845;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";

                Payroll_MaleFemaleRate tblObj = new Payroll_MaleFemaleRate();
                //GET COMPANY NAME
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                var CmpId = GetComapnyName[0].Id;
           
                if (tblObj.MaleFemaleRateCmpid != 0)
                {
                    DynamicParameters paramBranch = new DynamicParameters();
                    paramBranch.Add("@query", "select Distinct BranchId as Id,BranchName as Name from Mas_Branch where Deactivate=0 AND CmpId='" + tblObj.MaleFemaleRateCmpid + "';");
                    var branchList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranch).ToList();
                    ViewBag.BranchName = branchList;

                }
                else
                {
                    ViewBag.BranchName = new List<AllDropDownBind>();
                }
                
                if (MaleFemaleRateId_Encrypted != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_MaleFemaleRateId_Encrypted", MaleFemaleRateId_Encrypted);
                    tblObj = DapperORM.ReturnList<Payroll_MaleFemaleRate>("sp_List_Payroll_MaleFemaleRate", param).FirstOrDefault();

                    //var Branch1 = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(tblObj.MaleFemaleRateCmpid), Convert.ToInt32(Session["EmployeeId"]));
                    //ViewBag.BranchName = Branch1;
                    DynamicParameters paramBranchName = new DynamicParameters();
                    paramBranchName.Add("@query", "select BranchId as Id, BranchName as Name  from Mas_Branch where Deactivate = 0 and CmpId ='" + tblObj.MaleFemaleRateCmpid + "'  order by Name");
                    var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranchName).ToList();
                    ViewBag.BranchName = Branch;

                    TempData["RateFromDate"] = tblObj.RateFromDate.ToString("yyyy-MM-dd");
                    TempData["RateTodate"] = tblObj.RateTodate.ToString("yyyy-MM-dd");


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
        public ActionResult GetBusinessUnit(int MaleFemaleRateCmpid)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
              //  DynamicParameters param = new DynamicParameters();

                DynamicParameters paramBranchName = new DynamicParameters();
                paramBranchName.Add("@query", "select BranchId as Id, BranchName as Name  from Mas_Branch where Deactivate = 0 and CmpId ='" + MaleFemaleRateCmpid + "'  order by Name");
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

        #region IsValidation
        public ActionResult IsExists(string MaleFemaleRateId_Encrypted, double MaleFemaleRateCmpid, double? MaleFemaleRateBranchid, string Gender,double? Rate, double? InsumentRate)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_MaleFemaleRateId_Encrypted", MaleFemaleRateId_Encrypted);
                    param.Add("@p_MaleFemaleRateCmpid", MaleFemaleRateCmpid);
                    param.Add("@p_MaleFemaleRateBranchid", MaleFemaleRateBranchid);
                    param.Add("@p_Gender", Gender);
                    param.Add("@p_Rate", Rate);
                    param.Add("@p_InsumentRate", InsumentRate);
                   // param.Add("@p_RateFromDate", RateFromDate);
                   // param.Add("@p_RateTodate", RateTodate);
              
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_MaleFemaleRate", param);
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
        public ActionResult SaveUpdate(Payroll_MaleFemaleRate obj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramPrev = new DynamicParameters();
                paramPrev.Add("@query", "SELECT TOP 1 * FROM Payroll_MaleFemaleRate WHERE MaleFemaleRateCmpid='" + obj.MaleFemaleRateCmpid + "' AND MaleFemaleRateBranchid='" + obj.MaleFemaleRateBranchid + "' AND Gender='" + obj.Gender + "' AND Deactivate=0 ORDER BY RateFromDate DESC");

                var lastEntry = DapperORM.ReturnList<dynamic>("sp_QueryExcution", paramPrev).FirstOrDefault();
                StringBuilder sb = new StringBuilder();

                if (lastEntry != null && string.IsNullOrEmpty(obj.MaleFemaleRateId_Encrypted))
                {
                    DateTime lastFromDate = Convert.ToDateTime(lastEntry.RateFromDate);
                    DateTime newFromDate = obj.RateFromDate;
                    if (newFromDate <= lastFromDate)
                    {
                        TempData["Message"] = "New entry From Date must be greater than the last entry.";
                        TempData["Icon"] = "error";
                        return RedirectToAction("Module_Payroll_MaleFemaleRate", "Module_Payroll_MaleFemaleRate");
                    }
                    using (SqlConnection con = new SqlConnection(DapperORM.connectionString))
                    {
                        con.Open();
                        string updateQuery = "UPDATE Payroll_MaleFemaleRate SET RateTodate = @NewToDate WHERE MaleFemaleRateId = @LastId";
                        DynamicParameters paramUpdate = new DynamicParameters();
                        paramUpdate.Add("@NewToDate", newFromDate.AddDays(-1));
                        paramUpdate.Add("@LastId", lastEntry.MaleFemaleRateId);
                        con.Execute(updateQuery, paramUpdate);
                    }
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", string.IsNullOrEmpty(obj.MaleFemaleRateId_Encrypted) ? "Save" : "Update");
                param.Add("@p_MaleFemaleRateId_Encrypted", obj.MaleFemaleRateId_Encrypted);
                param.Add("@p_MaleFemaleRateCmpid", obj.MaleFemaleRateCmpid);
                param.Add("@p_MaleFemaleRateBranchid", obj.MaleFemaleRateBranchid);
                param.Add("@p_Gender", obj.Gender);
                param.Add("@p_Rate", obj.Rate);
                param.Add("@p_InsumentRate", obj.InsumentRate);
                param.Add("@p_RateFromDate", obj.RateFromDate);
                param.Add("@p_RateTodate", obj.RateTodate);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);

                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_MaleFemaleRate", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Module_Payroll_MaleFemaleRate", "Module_Payroll_MaleFemaleRate");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 845;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_MaleFemaleRateId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Payroll_MaleFemaleRate", param).ToList();
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
        public ActionResult Delete(string MaleFemaleRateId_Encrypted)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Delete");
                param.Add("@p_MaleFemaleRateId_Encrypted", MaleFemaleRateId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_MaleFemaleRate", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Module_Payroll_MaleFemaleRate");
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
