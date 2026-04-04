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
    public class Setting_Prime_CompanyEmergencyContactController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_Prime_CompanyEmergencyContact

        #region CompanyEmergency Contact Main View 
        [HttpGet]
        public ActionResult Setting_Prime_CompanyEmergencyContact(string CompanyEmergencyContactId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Mas_CompanyEmergencyContact Mas_EmergencyContact = new Mas_CompanyEmergencyContact();
                DynamicParameters paramBranch = new DynamicParameters();
                paramBranch.Add("@query", "Select BranchId as Id,BranchName as Name from Mas_Branch Where Deactivate=0");
                var Mas_BranchList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranch).ToList();
                ViewBag.GetBranchName = Mas_BranchList;


                if (CompanyEmergencyContactId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@P_CompanyEmergencyContactId_Encrypted", CompanyEmergencyContactId_Encrypted);
                    Mas_EmergencyContact = DapperORM.ReturnList<Mas_CompanyEmergencyContact>("sp_List_Mas_CompanyEmergencyContact", param).FirstOrDefault();
                }
                return View(Mas_EmergencyContact);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsVAlidation
        public ActionResult IsContactExists(string MobileNo, string CompanyEmergencyContactId_Encrypted)
        {
            try
            {              
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_MobileNo", MobileNo);
                    param.Add("@p_CompanyEmergencyContactId_Encrypted", CompanyEmergencyContactId_Encrypted);                  
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_CompanyEmergencyContact", param);
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
        public ActionResult SaveUpdate(Mas_CompanyEmergencyContact CompanyEmergencyContact)
        {
            try
            {
                var CmpId = Session["CompanyId"];
                param.Add("@p_process", string.IsNullOrEmpty(CompanyEmergencyContact.CompanyEmergencyContactId_Encrypted) ? "Save" : "Update");
                param.Add("@p_CompanyEmergencyContactId", CompanyEmergencyContact.CompanyEmergencyContactId);
                param.Add("@P_CompanyEmergencyContactId_Encrypted", CompanyEmergencyContact.CompanyEmergencyContactId_Encrypted);
                param.Add("@p_CmpID", CmpId);
                param.Add("@p_ContactBranchId", CompanyEmergencyContact.ContactBranchId);
                param.Add("@p_Name", CompanyEmergencyContact.Name);
                param.Add("@p_Type", CompanyEmergencyContact.Type);
                param.Add("@p_Designation", CompanyEmergencyContact.Designation);
                param.Add("@p_MobileNo", CompanyEmergencyContact.MobileNo);
                param.Add("@p_IsGlobal", CompanyEmergencyContact.IsGlobal);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_CompanyEmergencyContact", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Setting_Prime_CompanyEmergencyContact", "Setting_Prime_CompanyEmergencyContact");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList View 
        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_CompanyEmergencyContactId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Mas_CompanyEmergencyContact", param).ToList();
                ViewBag.GetEmergencyContactList = data;
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
        public ActionResult Delete(string CompanyEmergencyContactId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_CompanyEmergencyContactId_Encrypted", CompanyEmergencyContactId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_CompanyEmergencyContact", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();

                return RedirectToAction("GetList", "Setting_Prime_CompanyEmergencyContact");
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
