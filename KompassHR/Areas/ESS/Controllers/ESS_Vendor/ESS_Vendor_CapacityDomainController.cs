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
    public class ESS_Vendor_CapacityDomainController : Controller
    {
        DynamicParameters param = new DynamicParameters();

        // GET: ESS/ESS_Vendor_CapacityDomain
        public ActionResult ESS_Vendor_CapacityDomain()
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

                param.Add("@p_VendorId", Session["VendorId"]);
                var data = DapperORM.DynamicList("sp_List_Mas_Vendor_CapacityDomain", param);
                ViewBag.GetCapacityDetails = data;

                return View();

            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_Vendor");
            }
        }

        public ActionResult IsVendorCapacityExist(string VendorId, string VendorCapacityId_Encrypted)
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
                    param.Add("@p_VendorId", VendorId);
                    param.Add("@p_VendorCapacityId_Encrypted", VendorCapacityId_Encrypted);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                    sqlcon.Execute("sp_SUD_Mas_Vendor_CapacityDomain", param, commandType: CommandType.StoredProcedure);
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

        public ActionResult SaveUpdate(Mas_Vendor_CapacityDomain capacity)
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
                    var param = new DynamicParameters();

                    param.Add("@p_process",
                        string.IsNullOrEmpty(capacity.VendorCapacityId_Encrypted) ? "Save" : "Update");

                    param.Add("@p_VendorCapacityId_Encrypted", capacity.VendorCapacityId_Encrypted); // ✅ FIX
                    param.Add("@p_VendorId", capacity.VendorId);
                    param.Add("@p_IndustriesServed", capacity.IndustriesServed);
                    param.Add("@p_SkillSetAvailable", capacity.SkillSetAvailable);
                    param.Add("@p_NoOfEmployeeSupplied", capacity.NoOfEmployeeSupplied);
                    param.Add("@p_LocationCovered", capacity.LocationCovered);
                    param.Add("@p_BackgroundVerification", capacity.BackgroundVerification);
                    param.Add("@p_PoliceVerification", capacity.PoliceVerification);
                    param.Add("@p_OnBoardingSupport", capacity.OnBoardingSupport);
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]?.ToString());
                    param.Add("@p_MachineName", System.Net.Dns.GetHostName());

                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);

                    sqlcon.Execute(
                        "sp_SUD_Mas_Vendor_CapacityDomain",
                        param,
                        commandType: CommandType.StoredProcedure
                    );

                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    TempData["P_Id"] = param.Get<string>("@p_Id");
                }

                return RedirectToAction("ESS_Vendor_CapacityDomain", "ESS_Vendor_CapacityDomain", new { area = "ESS" });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpGet]
        public JsonResult GetCapacityById(string id)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }

                string query = @"
            SELECT VendorCapacityId_Encrypted,
                   IndustriesServed,
                   SkillSetAvailable,
                   NoOfEmployeeSupplied,
                   LocationCovered,
                   BackgroundVerification,
                   PoliceVerification,
                   OnBoardingSupport
            FROM Mas_Vendor_CapacityDomain
            WHERE Deactivate = 0
              AND VendorCapacityId_Encrypted = '"+id+"'";
                

                var result = DapperORM.DynamicQuerySingle(query);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }


    }
}