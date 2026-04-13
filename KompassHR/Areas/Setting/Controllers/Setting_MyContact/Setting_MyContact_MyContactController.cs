using Dapper;
using KompassHR.Areas.Setting.Models.MyContact;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_MyContact
{
    public class Setting_MyContact_MyContactController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: MyContactSetting/MyContact

        #region MyContact Main View
        [HttpGet]
        public ActionResult Setting_MyContact_MyContact(string MyContactId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Mas_MyContact mas_MyContact = new Mas_MyContact();
                DynamicParameters paramDropDown = new DynamicParameters();
                paramDropDown.Add("@query", "Select ContactCategoryId AS Id,ContactCategoryName As [Name] from Mas_ContactCategory Where Deactivate=0 order by Name");
                var listMas_ContactCategory = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", paramDropDown).ToList();
                ViewBag.GetContactCategoryName = listMas_ContactCategory;

                if (MyContactId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_MyContactId_Encrypted", MyContactId_Encrypted);
                    mas_MyContact = DapperORM.ReturnList<Mas_MyContact>("sp_List_Mas_MyContact", param).FirstOrDefault();
                }
                return View(mas_MyContact);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsVerification
        [HttpGet]
        public ActionResult IsContactExists(string ContactNo, string MyContactId_Encrypted, string MyContactEmployeeID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var EmployeeID = Session["EmployeeID"];
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_ContactNo", ContactNo);
                    param.Add("@p_MyContactEmployeeID", EmployeeID);
                    param.Add("@p_MyContactId_Encrypted", MyContactId_Encrypted);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("[sp_SUD_Mas_MyContact]", param);
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

        #region MyContact SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Mas_MyContact Contact)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(Contact.MyContactId_Encrypted) ? "Save" : "Update");
                param.Add("@p_MyContactId", Contact.MyContactId);
                param.Add("@p_MyContactId_Encrypted", Contact.MyContactId_Encrypted);
                param.Add("@p_ContactCategoryID", Contact.ContactCategoryID);
                param.Add("@p_MyContactEmployeeID", EmployeeId);
                param.Add("@p_PersonName", Contact.PersonName);
                param.Add("@p_CompanyName", Contact.CompanyName);
                param.Add("@p_Designation", Contact.Designation);
                param.Add("@p_EmailID", Contact.EmailID);
                param.Add("@p_ContactNo", Contact.ContactNo);
                param.Add("@p_WhatsAppNo", Contact.WhatsAppNo);
                param.Add("@p_Address", Contact.Address);
                param.Add("@p_OpenToAll", Contact.OpenToAll);

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Mas_MyContact", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_MyContact_MyContact", "Setting_MyContact_MyContact");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region MyContact List View
        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_MyContactId_Encrypted", "List");
                var data = DapperORM.ReturnList<Mas_MyContact>("sp_List_Mas_MyContact", param).ToList();
                ViewBag.GetContactList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region MyContact Delete
        [HttpGet]
        public ActionResult Delete(string MyContactId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_MyContactId_Encrypted", MyContactId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_MyContact", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_MyContact_MyContact");
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