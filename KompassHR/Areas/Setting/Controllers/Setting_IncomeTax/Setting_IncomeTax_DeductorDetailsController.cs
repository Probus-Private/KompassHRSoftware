using Dapper;
using KompassHR.Areas.Setting.Models.Setting_AccountAndFinance;
using KompassHR.Areas.Setting.Models.Setting_IncomeTax;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_IncomeTax
{
    public class Setting_IncomeTax_DeductorDetailsController : Controller
    {
        DynamicParameters param = new DynamicParameters();

        [HttpGet]
        public ActionResult Setting_IncomeTax_DeductorDetails(string DeductorResponsibleId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 75;
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
                IncomeTax_DeductorResponsible income = new IncomeTax_DeductorResponsible();

                if (DeductorResponsibleId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_DeductorResponsibleId_Encrypted", DeductorResponsibleId_Encrypted);
                    income = DapperORM.ReturnList<IncomeTax_DeductorResponsible>("sp_List_IncomeTax_DeductorResponsible", param).FirstOrDefault();
                }
                return View(income);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpGet]
        public ActionResult IsDeductorDetailsExists(string DeductorTAN, string DeductorPAN, string DeductorResponsibleId_Encrypted, int CmpId)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_DeductorTAN", DeductorTAN);
                    param.Add("@p_DeductorPAN", DeductorPAN);
                    param.Add("@p_CmpId", CmpId);
                    param.Add("@p_DeductorResponsibleId_Encrypted", DeductorResponsibleId_Encrypted);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_DeductorResponsible", param);
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

        [HttpPost]
        public ActionResult SaveUpdate(IncomeTax_DeductorResponsible DeductorDetails)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(DeductorDetails.DeductorResponsibleId_Encrypted) ? "Save" : "Update");
                param.Add("@P_DeductorResponsibleId_Encrypted", DeductorDetails.DeductorResponsibleId_Encrypted);
                param.Add("@P_DeductorResponsibleId", DeductorDetails.DeductorResponsibleId);
                param.Add("@P_CmpId", DeductorDetails.CmpId);
                param.Add("@P_DeductorTAN", DeductorDetails.DeductorTAN);
                param.Add("@P_DeductorPAN", DeductorDetails.DeductorPAN);
                param.Add("@P_DeductorName", DeductorDetails.DeductorName);
                param.Add("@P_DeductorDivision", DeductorDetails.DeductorDivision);
                param.Add("@P_ReturnUtility", DeductorDetails.ReturnUtility);
                param.Add("@P_DeductorFlatDoorBlockNo", DeductorDetails.DeductorFlatDoorBlockNo);
                param.Add("@P_DeductorNameofPremisesBuilding", DeductorDetails.DeductorNameofPremisesBuilding);
                param.Add("@P_DeductorRoadStreetLane", DeductorDetails.DeductorRoadStreetLane);
                param.Add("@P_DeductorAreaLocality", DeductorDetails.DeductorAreaLocality);
                param.Add("@P_DeductorTownCityDistrict", DeductorDetails.DeductorTownCityDistrict);
                param.Add("@P_DeductorState", DeductorDetails.DeductorState);
                param.Add("@P_DeductorStateCode", DeductorDetails.DeductorStateCode);
                param.Add("@P_DeductorPin", DeductorDetails.DeductorPin);
                param.Add("@P_DeductorEmail", DeductorDetails.DeductorEmail);
                param.Add("@P_DeductorSTD", DeductorDetails.DeductorSTD);
                param.Add("@P_DeductorPhone", DeductorDetails.DeductorPhone);
                param.Add("@P_DeductorMobile", DeductorDetails.DeductorMobile);
                param.Add("@P_DeductorType", DeductorDetails.DeductorType);
                param.Add("@P_IsChangeAddressDeductor", DeductorDetails.IsChangeAddressDeductor);
                param.Add("@P_ResponsibleName", DeductorDetails.ResponsibleName);
                param.Add("@P_ResponsibleDesignation", DeductorDetails.ResponsibleDesignation);
                param.Add("@P_ResponsibleFlatDoorBlockNo", DeductorDetails.ResponsibleFlatDoorBlockNo);
                param.Add("@P_ResponsibleNameofPremisesBuilding", DeductorDetails.ResponsibleNameofPremisesBuilding);
                param.Add("@P_ResponsibleRoadStreetLane", DeductorDetails.ResponsibleRoadStreetLane);
                param.Add("@P_ResponsibleAreaLocality", DeductorDetails.ResponsibleAreaLocality);
                param.Add("@P_ResponsibleTownCityDistrict", DeductorDetails.ResponsibleTownCityDistrict);
                param.Add("@P_ResponsibleState", DeductorDetails.ResponsibleState);
                param.Add("@P_ResponsibleStateCode", DeductorDetails.ResponsibleStateCode);
                param.Add("@P_ResponsiblePin", DeductorDetails.ResponsiblePin);
                param.Add("@P_ResponsibleEmail", DeductorDetails.ResponsibleEmail);
                param.Add("@P_ResponsibleSTD", DeductorDetails.ResponsibleSTD);
                param.Add("@P_ResponsiblePhone", DeductorDetails.ResponsiblePhone);
                param.Add("@P_ResponsibleMobile", DeductorDetails.ResponsibleMobile);
                param.Add("@P_ResponsiblePAN", DeductorDetails.ResponsiblePAN);
                param.Add("@P_IsChangeAddressResponsible", DeductorDetails.IsChangeAddressResponsible);
                param.Add("@P_RPUFileName", DeductorDetails.RPUFileName);
                param.Add("@P_Form16FullName", DeductorDetails.Form16FullName);
                param.Add("@P_Form16SO", DeductorDetails.Form16SO);
                param.Add("@P_Form16Designation", DeductorDetails.Form16Designation);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@P_Form16Palace", DeductorDetails.Form16Palace);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var data = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_DeductorResponsible", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_IncomeTax_DeductorDetails", "Setting_IncomeTax_DeductorDetails");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 75;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_DeductorResponsibleId_Encrypted", "List");
                var data = DapperORM.ReturnList<IncomeTax_DeductorResponsible>("sp_List_IncomeTax_DeductorResponsible", param).ToList();
                ViewBag.DeductionList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


        public ActionResult Delete(string DeductorResponsibleId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_DeductorResponsibleId_Encrypted", DeductorResponsibleId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_DeductorResponsible", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("GetList", "Setting_IncomeTax_DeductorDetails");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
    }
}