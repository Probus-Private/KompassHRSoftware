using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Vendor;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Vendor
{
    public class ESS_Vendor_ContactDetailsController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: ESS/ESS_Vendor_ContactDetails
        public ActionResult ESS_Vendor_ContactDetails(string ContactID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 600;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Mas_Vendor_ContactDetails Mas_Vendor_ContactDetails = new Mas_Vendor_ContactDetails();
                var VendorId = Convert.ToInt32(Session["VendorId"]);
                    param.Add("@p_ContactVendorId", VendorId);
                   var ContactDetails = DapperORM.ReturnList<Mas_Vendor_ContactDetails>("sp_List_Mas_Vendor_ContactDetails", param).ToList();
                ViewBag.GetVendorContact = ContactDetails;
                return View();
            }
            catch(Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult IsContactExist(string VendorId, string EmailId, string MobileNo,string ContactID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    int EmpId = Convert.ToInt32(Session["EmployeeId"]);

                    var param = new DynamicParameters();
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_ContactVendorId", VendorId);
                    param.Add("@p_EmailId", EmailId);
                    param.Add("@p_MobileNo", MobileNo);
                    param.Add("@p_ContactID_Encrypted", ContactID_Encrypted);
                    param.Add("@p_MachineName", System.Net.Dns.GetHostName());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    sqlcon.Execute("sp_SUD_Mas_Vendor_ContactDetails", param, commandType: CommandType.StoredProcedure);
                    var Message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    if (!string.IsNullOrEmpty(Message))
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
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SaveUpdate(Mas_Vendor_ContactDetails contact)
        {
            try
            {
                // Check session
                if (Session["EmployeeId"] == null)
                {
                    TempData["Message"] = "Your session has expired. Please log in again.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                var param = new DynamicParameters();
                param.Add("@p_process", string.IsNullOrEmpty(contact.ContactID_Encrypted) ? "Save" : "Update");
                
                param.Add("@p_ContactVendorId", contact.ContactVendorId);
                param.Add("@p_PrimaryContact", contact.PrimaryContact);
                param.Add("@p_Designation", contact.Designation);
                param.Add("@p_EmailID", contact.EmailID);
                param.Add("@p_MobileNo", contact.MobileNo);
                param.Add("@p_AlternateContactNo", contact.AlternateContactNo);
                param.Add("@p_OfficialLandline", contact.OfficialLandline);
                param.Add("@p_Deactivate", contact.Deactivate);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 10);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);

                var result = DapperORM.ExecuteReturn("sp_SUD_Mas_Vendor_ContactDetails", param); // your stored procedure name

                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                return RedirectToAction("ESS_Vendor_ContactDetails", "ESS_Vendor_ContactDetails", new { area = "ESS" });
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                TempData["Icon"] = "error";
                return RedirectToAction("ESS_Vendor_ContactDetails", "ESS_Vendor_ContactDetails", new { area = "ESS" });
            }
        }

        #region Partial View 
        [HttpGet]
        public PartialViewResult Vendor_SidebarMenu()
        {
            var VendorId = Session["VendorId"];
            DynamicParameters paramList = new DynamicParameters();
            string query = "SELECT * FROM mas_vendorsetup WHERE VendorId = '" + VendorId + "' AND Deactivate = 0";
            var result = DapperORM.DynamicQuerySingle(query);
            ViewBag.GetStatusCheckList = result;
            var id = Session["ModuleId"];
            var ScreenId = Session["ScreenId"];
            var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), Convert.ToInt32(id), Convert.ToInt32(ScreenId), "SubForm", "Transation");
            ViewBag.GetUserMenuList = GetMenuList;
            return PartialView("_Vendor_SidebarMenu");
            //return RedirectToAction("Docuinfo",rec);
        }
        #endregion

    }
}