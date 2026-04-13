
using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Setting.Models.Setting_SaleForce;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_SaleForce
{
    public class Setting_SalesForce_IndustryMappingController : Controller
    {

        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region Main View Setting_SalesForce_IndustryMapping
        // GET: Setting/Setting_SalesForce_IndustryMapping
        public ActionResult Setting_SalesForce_IndustryMapping(int?Fid)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 814;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";

                param.Add("@query", $"select Fid, IndustryType from MAS_INDUSTRYTYPE where Deleted=0");
                var IndustryTypes = DapperORM.ReturnList<MAS_INDUSTRYTYPE>("Sp_QueryExcution", param).ToList();
                ViewBag.IndustryTypes = IndustryTypes;

                // param.Add("@query", $"select EmployeeId AS Fid  , EmployeeName from Mas_Employee where Deactivate=0 AND EmployeeLeft=1 order by EmployeeName");
                //// var EmployeeName = DapperORM.ReturnList<Mas_Employee>("Sp_QueryExcution", param).ToList();
                // var EmployeeName = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                // ViewBag.EmployeeName = EmployeeName;

                param.Add("@query", $"select EmployeeId, EmployeeName from Mas_Employee where Deactivate = 0 AND EmployeeLeft = 0  order by EmployeeName");
                var EmployeeName = DapperORM.ReturnList<Mas_Employee>("Sp_QueryExcution", param).ToList();
                ViewBag.EmployeeName = EmployeeName;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        //public ActionResult GetEMPBUCODE(int Fid)
        //{
        //    try
        //    {
        //        // DapperORM.SetConnection();
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { area = "" });
        //        }

        //        param.Add("@query", $"select EmployeeId As Fid , EmployeeBranchId As BuCode from Mas_Employee where EmployeeId = '{Fid}'");
        //        var EmployeeName = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                
        //        return Json(EmployeeName, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return RedirectToAction("ErrorPage", "Login", new { area = "" });

        //    }
        //}



        #region IsIndustryMappingExists
        public ActionResult IsIndustryMappingExists(int MAS_INDUSTRYTYPE, int MAS_Employee_Fid)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 813;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {

                    param.Add("@p_process", "IsValidation");
                    //param.Add("@p_Fid", Fid);
                    param.Add("@p_MAS_INDUSTRYTYPE", MAS_INDUSTRYTYPE);
                    param.Add("@p_MAS_Employee_Fid", MAS_Employee_Fid);
                    param.Add("@p_msg", dbType: System.Data.DbType.String, direction: System.Data.ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: System.Data.DbType.String, direction: System.Data.ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_MAS_EMPINDUSTYPE", param);
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
        public ActionResult SaveUpdate(MAS_EMPINDUSTYPE MAS_EMPINDUSTYPE)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@p_process", "IsValidation");
                param.Add("@p_MAS_Employee_Fid", MAS_EMPINDUSTYPE.MAS_Employee_Fid);
                param.Add("@p_MAS_INDUSTRYTYPE", MAS_EMPINDUSTYPE.MAS_INDUSTRYTYPE);
                param.Add("@p_BUCode", MAS_EMPINDUSTYPE.BUCode);
                param.Add("@p_msg", dbType: System.Data.DbType.String, direction: System.Data.ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: System.Data.DbType.String, direction: System.Data.ParameterDirection.Output, size: 500);
                var data = DapperORM.ExecuteReturn("sp_SUD_MAS_EMPINDUSTYPE", param);
                var Message = param.Get<string>("@p_msg");
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                if (Message == "")
                {
                    param.Add("@p_process", "Save");
                    param.Add("@P_Fid", MAS_EMPINDUSTYPE.Fid);
                    param.Add("@p_MAS_Employee_Fid", MAS_EMPINDUSTYPE.MAS_Employee_Fid);
                    param.Add("@p_MAS_INDUSTRYTYPE", MAS_EMPINDUSTYPE.MAS_INDUSTRYTYPE);
                    param.Add("@p_BUCode", MAS_EMPINDUSTYPE.BUCode);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    //param.Add("@p_Mip", Dns.GetHostByName(MachineId).AddressList[0].ToString());
                    // param.Add("@p_UserId", Session["EmployeeId"]);
                    param.Add("@p_UserId", 0);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    data = DapperORM.ExecuteReturn("sp_SUD_MAS_EMPINDUSTYPE", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    return RedirectToAction("Setting_SalesForce_IndustryMapping", "Setting_SalesForce_IndustryMapping");
                }
                return RedirectToAction("Setting_SalesForce_IndustryMapping", "Setting_SalesForce_IndustryMapping");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 814;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@query", "select Mas_Employee.EmployeeName, MAS_INDUSTRYTYPE.IndustryType, MAS_EMPINDUSTYPE.* from MAS_EMPINDUSTYPE join Mas_Employee on MAS_EMPINDUSTYPE.MAS_Employee_Fid = Mas_Employee.EmployeeId join MAS_INDUSTRYTYPE  on MAS_EMPINDUSTYPE.MAS_INDUSTRYTYPE = MAS_INDUSTRYTYPE.Fid where MAS_EMPINDUSTYPE.Deleted = 0");
                var IndustryMappingList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                ViewBag.IndustryMappingList = IndustryMappingList;
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
        public ActionResult Delete(string Fid)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 814;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_Id", Fid);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_MAS_EMPINDUSTYPE", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_SalesForce_IndustryMapping");
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