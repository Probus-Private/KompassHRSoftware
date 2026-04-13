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
    public class Module_Employee_AddressInfoController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Employee/AddressInformation
        #region AddressInfo Main View
        [HttpGet]
        public ActionResult Module_Employee_AddressInfo(string Status)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 425;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                var countEMP = DapperORM.DynamicQuerySingle("Select count(AddressEmployeeId)  as AddressEmployeeId  from Mas_Employee_Address where Mas_Employee_Address.AddressEmployeeId=" + Session["OnboardEmployeeId"] + "");
                TempData["CountAddressEmployeeId"] = countEMP.AddressEmployeeId;
                var countPreBoard = DapperORM.DynamicQuerySingle("select PreboardingFid from Mas_Employee wheRE EmployeeId=" + Session["OnboardEmployeeId"] + "");
                TempData["PreBoardFid"] = countPreBoard?.PreboardingFid;
                Mas_Employee_Address EmployeeAddress = new Mas_Employee_Address();
                if (Status == "Pre")
                {
                    var GetPreboardingFid = DapperORM.DynamicQuerySingle("Select PreboardingFid from Mas_Employee where employeeid=" + Session["OnboardEmployeeId"] + "");
                    var PreboardingFid = GetPreboardingFid.PreboardingFid;

                    param = new DynamicParameters();
                    param.Add("@p_FId", PreboardingFid);
                    EmployeeAddress = DapperORM.ReturnList<Mas_Employee_Address>("sp_GetList_Mas_PreboardEmployeeDetails", param).FirstOrDefault();
                    ViewBag.GetEmployeePersonal = EmployeeAddress;

                    return View(EmployeeAddress);
                }

                param = new DynamicParameters();
                param.Add("@p_AddressEmployeeId", Session["OnboardEmployeeId"]);
                TempData["OnboardEmployeeName"] = Session["OnboardEmployeeName"];
                EmployeeAddress = DapperORM.ReturnList<Mas_Employee_Address>("sp_List_Mas_Employee_Address", param).FirstOrDefault();

                if (EmployeeAddress != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                }
                return View(EmployeeAddress);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetPincode
        [HttpGet]
        public ActionResult GetPincode(int PinCode)
        {
            try
            {  //PinCode
                param.Add("@p_PinCode", PinCode);
                var GetPinCodelist = DapperORM.ExecuteSP<dynamic>("[sp_List_Mas_Employee_PinCode]", param).ToList();
                return Json(new { data = GetPinCodelist }, JsonRequestBehavior.AllowGet);
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
        public ActionResult SaveUpdate(Mas_Employee_Address Employeeaddress)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(Employeeaddress.AddressId_Encrypted) ? "Save" : "Update");
                param.Add("@p_AddressId", Employeeaddress.AddressId);
                param.Add("@p_AddressId_Encrypted", Employeeaddress.AddressId_Encrypted);
                param.Add("@p_AddressEmployeeId", Session["OnboardEmployeeId"]);
                param.Add("@p_PresentPin", Employeeaddress.PresentPin);
                param.Add("@p_PresentState", Employeeaddress.PresentState);
                param.Add("@p_PresentDistrict", Employeeaddress.PresentDistrict);
                param.Add("@p_PresentTaluka", Employeeaddress.PresentTaluka);
                param.Add("@p_PresentPO", Employeeaddress.PresentPO);
                param.Add("@p_PresentCity", Employeeaddress.PresentCity);
                param.Add("@p_PresentPostelAddress", Employeeaddress.PresentPostelAddress);
                param.Add("@p_PermanentPin", Employeeaddress.PermanentPin);
                param.Add("@p_PermanentState", Employeeaddress.PermanentState);
                param.Add("@p_PermanentDistrict", Employeeaddress.PermanentDistrict);
                param.Add("@p_PermanentTaluka", Employeeaddress.PermanentTaluka);
                param.Add("@p_PermanentPO", Employeeaddress.PermanentPO);
                param.Add("@p_PermanentCity", Employeeaddress.PermanentCity);
                param.Add("@p_PermanentPostelAddress", Employeeaddress.PermanentPostelAddress);
                param.Add("@p_PermanentAddressSameAsCurrentAddress", Employeeaddress.PermanentAddressSameAsCurrentAddress);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_Address]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_AddressInfo", "Module_Employee_AddressInfo");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetAddressAsPrebording
        [HttpGet]
        public ActionResult GetAddressAsPrebording()
        {
            try
            {
                DynamicParameters paramAdrress = new DynamicParameters();
                paramAdrress.Add("@p_DocOrigin", "Address");
                paramAdrress.Add("@p_EmployeeId", Session["OnboardEmployeeId"]);
                var data = DapperORM.ExecuteSP<dynamic>("sp_GetPreboardingEmployeeInfo", paramAdrress).FirstOrDefault();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Partial View 
        [HttpGet]
        public PartialViewResult Onboarding_SidebarMenu()
        {
            var OnboardEmployeeId = Session["OnboardEmployeeId"];
            DynamicParameters paramList = new DynamicParameters();
            paramList.Add("@p_EmployeeId", OnboardEmployeeId);
            var StatusCheck = DapperORM.DynamicList("sp_List_Mas_Employee_StatusCheck", paramList);
            ViewBag.GetStatusCheckList = StatusCheck;
            return PartialView("_Onboarding_SidebarMenu");
            //return RedirectToAction("Docuinfo",rec);
        }
        #endregion
    }
}