using Dapper;
using KompassHR.Areas.ESS.Models.ESS_ManpowerAllocation;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_ManpowerAllocation
{
    public class ESS_ManpowerAllocation_R1R2R3MappingController : Controller
    {
        #region ESS_ManpowerAllocation_R1R2R3Mapping Main View 
        // GET: ESS/ESS_ManpowerAllocation_R1R2R3Mapping
        [HttpGet]
        public ActionResult ESS_ManpowerAllocation_R1R2R3Mapping(string R1R2R3MappingId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 772;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                var CMPID = GetComapnyName[0].Id;

                DynamicParameters paramBUName = new DynamicParameters();
                paramBUName.Add("@query", "select Distinct BranchId as Id,BranchName as Name from Mas_Branch Where Deactivate=0;");
                ViewBag.BUName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBUName).ToList();

                ViewBag.AddUpdateTitle = "Add";
                Mapping_R1R2R3 Mapping  = new Mapping_R1R2R3();
                DynamicParameters param = new DynamicParameters();

               // if (R1R2R3MappingId_Encrypted != null|| R1R2R3MappingId_Encrypted ==" ")
                if (!string.IsNullOrEmpty(R1R2R3MappingId_Encrypted))
                    {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_R1R2R3MappingId_Encrypted", R1R2R3MappingId_Encrypted);
                    Mapping = DapperORM.ReturnList<Mapping_R1R2R3>("sp_List_Manpower_Mapping_R1R2R3", param).FirstOrDefault();
                    if (Mapping != null)
                    {

                        TempData["MonthYear"] = Mapping.MonthYear.HasValue
                        ? Mapping.MonthYear.Value.ToString("yyyy-MM")
                      : string.Empty;

                        TempData["R1FromDate"] = Mapping.R1FromDate.HasValue
                         ? Mapping.R1FromDate.Value.ToString("yyyy-MM-dd")
                         : string.Empty;
                        TempData["R1ToDate"] = Mapping.R1ToDate.HasValue
                        ? Mapping.R1ToDate.Value.ToString("yyyy-MM-dd")
                        : string.Empty;
                        TempData["R2FromDate"] = Mapping.R2FromDate.HasValue
                        ? Mapping.R2FromDate.Value.ToString("yyyy-MM-dd")
                        : string.Empty;
                        TempData["R2ToDate"] = Mapping.R2ToDate.HasValue
                        ? Mapping.R2ToDate.Value.ToString("yyyy-MM-dd")
                        : string.Empty;
                        TempData["R3FromDate"] = Mapping.R3FromDate.HasValue
                        ? Mapping.R3FromDate.Value.ToString("yyyy-MM-dd")
                        : string.Empty;
                        TempData["R3ToDate"] = Mapping.R3ToDate.HasValue
                      ? Mapping.R3ToDate.Value.ToString("yyyy-MM-dd")
                      : string.Empty;
                    }

                }
                return View(Mapping);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetBuisnessUnit
        [HttpGet]
        public ActionResult GetBuisnessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select Distinct BranchId as Id,BranchName as Name from Mas_Branch where Deactivate=0 AND CmpId='" + CmpId + "';");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsR1R2R3MappingExists
        public ActionResult IsR1R2R3MappingExists(int CmpId,int BranchId, string R1R2R3MappingId_Encrypted, int R1R2R3MappingId)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_CmpId", CmpId);
                    param.Add("@p_BranchId", BranchId);
                    param.Add("@p_R1R2R3MappingId", R1R2R3MappingId);
                    param.Add("@p_R1R2R3MappingId_Encrypted", R1R2R3MappingId_Encrypted);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                    var Result = DapperORM.ExecuteReturn("sp_SUD_Manpower_Mapping_R1R2R3", param);
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
        public ActionResult SaveUpdate(Mapping_R1R2R3  Mapping)
        {
            try

            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", string.IsNullOrEmpty(Mapping.R1R2R3MappingId_Encrypted) ? "Save" : "Update");
                param.Add("@p_R1R2R3MappingId_Encrypted", Mapping.R1R2R3MappingId_Encrypted);
                param.Add("@p_MonthYear", Mapping.MonthYear);
                param.Add("@p_CmpId", Mapping.CmpId);
                param.Add("@p_BranchId", Mapping.BranchId);
                param.Add("@p_R1FromDate", Mapping.R1FromDate);
                param.Add("@p_R1ToDate", Mapping.R1ToDate);
                param.Add("@p_R2FromDate", Mapping.R2FromDate);
                param.Add("@p_R2ToDate", Mapping.R2ToDate);
                param.Add("@p_R3FromDate", Mapping.R3FromDate);
                param.Add("@p_R3ToDate", Mapping.R3ToDate);
            
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                var Result = DapperORM.ExecuteReturn("sp_SUD_Manpower_Mapping_R1R2R3", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");

                var P_Id = param.Get<string>("@p_Id");

                return RedirectToAction("ESS_ManpowerAllocation_R1R2R3Mapping", "ESS_ManpowerAllocation_R1R2R3Mapping");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList Main View 
        public ActionResult GetList(string MonthYear)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 772;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "ESS" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_R1R2R3MappingId_Encrypted", "List");
                param.Add("@p_MonthYear", null);
                param.Add("@p_EmployeeId", Session["EmployeeId"]);

                //var data = DapperORM.ExecuteSP<dynamic>("sp_List_Mapping_R1R2R3", param).ToList();
                // ViewBag.GetMappingList = data;
                ViewBag.GetMappingList = new List<dynamic>();

                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;

                DynamicParameters paramBUName = new DynamicParameters();
                paramBUName.Add("@query", "select Distinct BranchId as Id, BranchName as Name from Mas_Branch;");
                ViewBag.BUName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBUName).ToList();
                
                if(!string.IsNullOrEmpty(MonthYear))
                {
                    DateTime month = DateTime.ParseExact(MonthYear, "yyyy-MM", null);

                    var data = DapperORM.ExecuteSP<dynamic>("sp_List_Manpower_Mapping_R1R2R3", param).ToList();
                    // var list = DapperORM.ExecuteSP<dynamic>("sp_List_Mapping_R1R2R3", param).ToList();
                    var list = data.Where(x =>
                       DateTime.Parse(x.MonthYear.ToString()).Year == month.Year &&
                       DateTime.Parse(x.MonthYear.ToString()).Month == month.Month).ToList();
                    ViewBag.GetMappingList = list;
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

        #region Delete
        public ActionResult DeleteMapping(int? R1R2R3MappingId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "ESS" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Delete");
                param.Add("@p_R1R2R3MappingId", R1R2R3MappingId);
                param.Add("@p_CreatedupdateBy", Session["EmployeeName"].ToString());
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                var Result = DapperORM.ExecuteReturn("sp_SUD_Manpower_Mapping_R1R2R3", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                // TempData["P_Id"] = param.Get<string>("@p_Id");
                return RedirectToAction("GetList", "ESS_ManpowerAllocation_R1R2R3Mapping");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Copy 
        [HttpPost]
        public ActionResult CopyMapping(int R1R2R3MappingId, int CmpId, int BranchId,DateTime MonthYear)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Copy");
                param.Add("@p_R1R2R3MappingId", R1R2R3MappingId);
                param.Add("@p_CmpId", CmpId);
                param.Add("@p_BranchId", BranchId);
                param.Add("@p_MonthYear", MonthYear);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                var result = DapperORM.ExecuteReturn("sp_SUD_Manpower_Mapping_R1R2R3", param);
                //TempData["Message"] = param.Get<string>("@p_msg");
                //TempData["Icon"] = param.Get<string>("@p_Icon");
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
                return RedirectToAction("GetList", "ESS_ManpowerAllocation_R1R2R3Mapping");
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

