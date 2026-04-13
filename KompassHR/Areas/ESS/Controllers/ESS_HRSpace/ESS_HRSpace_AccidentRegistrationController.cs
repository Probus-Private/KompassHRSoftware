using Dapper;
using KompassHR.Areas.ESS.Models.ESS_HRSpace;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_HRSpace
{
    public class ESS_HRSpace_AccidentRegistrationController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region ESS_Employee_RaiseGrievance Main View 
        
        [HttpGet]
        // GET: ESS/ESS_HRSpace_AccidentRegistration
        public ActionResult ESS_HRSpace_AccidentRegistration(string AccidentRegisterId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 766;
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
                paramBUName.Add("@query", "select Distinct BranchId as Id,BranchName as Name from Mas_Branch;");
                ViewBag.BUName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBUName).ToList();

                DynamicParameters paramSubUnit = new DynamicParameters();
                paramSubUnit.Add("@query", "select Distinct UnitId as Id,UnitName as Name from Mas_Unit;");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramSubUnit).ToList();
                ViewBag.SubUnit = data;

                DynamicParameters paramShift = new DynamicParameters();
                paramShift.Add("@query", " select Distinct ShiftId as Id,ShiftName as Name from Atten_Shifts;");
                ViewBag.Shift = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramShift).ToList();
           

                ViewBag.AddUpdateTitle = "Add";
                Employee_AccidentRegister AccidentReg = new Employee_AccidentRegister();
                DynamicParameters param = new DynamicParameters();

            
                if (string.IsNullOrEmpty(AccidentRegisterId_Encrypted))
                {
                    var GetAccidentNo = "SELECT ISNULL(MAX(AccidentNo), 0) + 1 AS AccidentNo FROM Employee_AccidentRegister";
                    var result = DapperORM.DynamicQuerySingle(GetAccidentNo);
                    var AccidentNo = result.AccidentNo;
                    ViewBag.AccidentNo = AccidentNo;
                    AccidentReg.AccidentNo = AccidentNo;
                }
                else
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_AccidentRegisterId_Encrypted", AccidentRegisterId_Encrypted);
                    AccidentReg = DapperORM.ReturnList<Employee_AccidentRegister>("sp_List_Employee_AccidentRegistration", param).FirstOrDefault();
                    ViewBag.AccidentNo = AccidentReg?.AccidentNo ?? 0;
                    TempData["DateOfEntry"] = AccidentReg.DateOfEntry.ToString("yyyy-MM-dd");

                }



                return View(AccidentReg);
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
        public ActionResult GetBuisnessUnit(int Company)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select Distinct BranchId as Id,BranchName as Name from Mas_Branch where CmpId='" + Company + "';");
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

        #region GetSubBuisnessUnit
        [HttpGet]
        public ActionResult GetSubBuisnessUnit(int BusinessUnit)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
     
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select Distinct UnitId as Id,UnitName as Name from Mas_Unit where UnitBranchId='"+BusinessUnit+"';");
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


        #region GetShift
        [HttpGet]
        public ActionResult GetShift(int BusinessUnit)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select Distinct ShiftId as Id,ShiftName as Name from Atten_Shifts where ShiftBranchId='" + BusinessUnit + "';");
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


        #region IsAccidentRegisterExists
        public ActionResult IsAccidentRegisterExists(int AccidentNo, string AccidentRegisterId_Encrypted,int AccidentRegisterId)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_AccidentNo", AccidentNo);
                    param.Add("@p_AccidentRegisterId", AccidentRegisterId);
                    param.Add("@p_AccidentRegisterId_Encrypted", AccidentRegisterId_Encrypted);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                    var Result = DapperORM.ExecuteReturn("sp_SUD_AccidentRegistration", param);
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
        public ActionResult SaveUpdate(Employee_AccidentRegister AccidentReg)
        {
            try

            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(AccidentReg.AccidentRegisterId_Encrypted) ? "Save" : "Update");
               // param.Add("@p_AccidentNo", AccidentReg.AccidentNo);
                param.Add("@p_AccidentRegisterId_Encrypted", AccidentReg.AccidentRegisterId_Encrypted);
                param.Add("@p_DateOfEntry", AccidentReg.DateOfEntry);
                param.Add("@p_AccidentTime", AccidentReg.AccidentTime);
                param.Add("@p_Company", AccidentReg.Company);
                param.Add("@p_BusinessUnit", AccidentReg.BusinessUnit);
                param.Add("@p_SubUnit", AccidentReg.SubUnit);
                param.Add("@p_Shift", AccidentReg.Shift);
                param.Add("@p_Description", AccidentReg.Description);
                param.Add("@p_Cause", AccidentReg.Cause);
                param.Add("@p_Nature", AccidentReg.Nature);
                param.Add("@p_ImmediateCorrectionAction", AccidentReg.ImmediateCorrectionAction);
                param.Add("@p_HospitalName", AccidentReg.HospitalName);
                param.Add("@p_WasAnybodyInjured", AccidentReg.WasAnybodyInjured);
                param.Add("@p_WasAnybodyInvolved", AccidentReg.WasAnybodyInvolved);
                param.Add("@p_Witness", AccidentReg.Witness);
                param.Add("@p_AccidentBookedBy", AccidentReg.AccidentBookedBy);

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                var Result = DapperORM.ExecuteReturn("sp_SUD_AccidentRegistration", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");

                var P_Id = param.Get<string>("@p_Id");
              
                return RedirectToAction("GetList", "ESS_HRSpace_AccidentRegistration");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region GetList Main View 
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 766;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "ESS" });
                }
                param.Add("@p_AccidentRegisterId_Encrypted", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);

                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Employee_AccidentRegistration", param).ToList();
                ViewBag.GetAccidentList = data;
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
        public ActionResult DeleteAccident(int? AccidentRegisterId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "ESS" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_AccidentRegisterId", AccidentRegisterId);
                param.Add("@p_CreatedupdateBy", Session["EmployeeId"].ToString());
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                var Result = DapperORM.ExecuteReturn("sp_SUD_AccidentRegistration", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
               // TempData["P_Id"] = param.Get<string>("@p_Id");
                return RedirectToAction("GetList", "ESS_HRSpace_AccidentRegistration");
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