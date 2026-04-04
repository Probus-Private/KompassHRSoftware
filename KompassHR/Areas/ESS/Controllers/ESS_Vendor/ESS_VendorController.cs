using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Vendor;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Vendor
{
    public class ESS_VendorController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();

        // GET: ESS/ESS_Vendor
        public ActionResult ESS_Vendor(string VendorId_Encrypted)
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
                return View();

            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_Vendor");
            }
        }


        public ActionResult IsVendorExist(string CompanyName, string VendorId_Encrypted)
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
                    param.Add("@p_CompanyName", CompanyName);
                    param.Add("@p_VendorId_Encrypted", VendorId_Encrypted);
                    param.Add("@p_MachineName", System.Net.Dns.GetHostName());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    sqlcon.Execute("sp_SUD_Mas_Vendor", param, commandType: CommandType.StoredProcedure);
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

        public ActionResult SaveUpdate(Mas_Vendor Mas_Vendor)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    TempData["Message"] = "Your session has expired. Please log in again.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    string process = string.IsNullOrEmpty(Mas_Vendor.VendorId_Encrypted) ? "Save" : "Update";
                    var param = new DynamicParameters();

                    param.Add("@p_process", process);
                    param.Add("@p_CmpID", Convert.ToInt64(Session["CmpId"]));
                    param.Add("@p_VendorBranchId", Mas_Vendor.VendorBranchId);
                    param.Add("@p_VendorType", Mas_Vendor.VendorType);
                    param.Add("@p_CompanyName", Mas_Vendor.CompanyName);
                    param.Add("@p_TradeName", Mas_Vendor.TradeName);
                    param.Add("@p_RegisteredAddress", Mas_Vendor.RegisteredAddress);
                    param.Add("@p_OperationAddress", Mas_Vendor.OperationAddress);
                    param.Add("@p_DateOfEstablishment", Mas_Vendor.DateOfEstablishment);
                    param.Add("@p_GSTIN", Mas_Vendor.GSTIN);
                    param.Add("@p_PAN", Mas_Vendor.PAN);
                    param.Add("@p_TAN", Mas_Vendor.TAN);
                    param.Add("@p_UDYAMorMSME", Mas_Vendor.UDYAMorMSME);
                    param.Add("@p_EPFONumber", Mas_Vendor.EPFONumber);
                    param.Add("@p_LabourLicenseNumber", Mas_Vendor.LabourLicenseNumber);
                    param.Add("@p_ISOOrQualityCertificate", Mas_Vendor.ISOOrQualityCertificate);
                    param.Add("@p_NatureOfBusiness", Mas_Vendor.NatureOfBusiness);
                    param.Add("@p_Description", Mas_Vendor.Description);
                    param.Add("@p_IsHavingHirelink", Mas_Vendor.IsHavingHirelink);
                    param.Add("@p_CustomerCode", Mas_Vendor.CustomerCode);
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]?.ToString());
                    param.Add("@p_MachineName", System.Net.Dns.GetHostName());

                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);

                    sqlcon.Execute("sp_SUD_Mas_Vendor", param, commandType: CommandType.StoredProcedure);

                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    TempData["P_Id"] = param.Get<string>("@p_Id");
                }

                return RedirectToAction("ESS_Vendor", "ESS_Vendor", new { area = "ESS" });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult GetList(int? id, int? ScreenId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 142;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_VendorId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Mas_Vendor", param).ToList();
                ViewBag.GetEmployeeInfoList = data;
                

                if (ScreenId != null)
                {
                    Session["ModuleId"] = id;
                    Session["ScreenId"] = ScreenId;
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), id, ScreenId, "SubForm", "Transation");
                    ViewBag.GetUserMenuList = GetMenuList;
                }
                else
                {
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), Convert.ToInt32(Session["ModuleId"]), Convert.ToInt32(Session["ScreenId"]), "SubForm", "Transation");
                    ViewBag.GetUserMenuList = GetMenuList;
                }
                return View();
            }
            catch(Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


        public ActionResult VendorCheckForm(int VendorId, string CompanyName, string GSTIN, string PAN)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                Session["VendorId"] = VendorId;
                Session["VendorName"] = CompanyName;
                Session["VendorGSTIN"] = GSTIN;
                Session["VendorPAN"] = PAN;

                string query = "SELECT * FROM mas_vendorsetup WHERE VendorId = '"+ VendorId + "' AND Deactivate = 0";
                var result = DapperORM.DynamicQuerySingle(query);
                var dict = new Dictionary<string, object>();
                if (result != null)
                {
                    foreach (var prop in (IDictionary<string, object>)result)
                    {
                        dict[prop.Key] = prop.Value;
                    }
                }
                return Json(dict, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, message = ex.Message }, JsonRequestBehavior.AllowGet);
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