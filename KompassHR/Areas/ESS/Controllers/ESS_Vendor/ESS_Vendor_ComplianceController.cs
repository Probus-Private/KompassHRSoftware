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
    public class ESS_Vendor_ComplianceController : Controller
    {
        DynamicParameters param = new DynamicParameters();

        // GET: ESS/ESS_Vendor_Compliance
        public ActionResult ESS_Vendor_Compliance()
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
                var VendorId = Convert.ToInt32(Session["VendorId"]);
                param.Add("@p_VendorId", VendorId);
                var ComplianceDetails = DapperORM.ReturnList<dynamic>("sp_List_Mas_Vendor_Compliance", param).ToList();
                ViewBag.ComplianceDetails = ComplianceDetails;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult IsComplianceExist(string VendorComplianceId_Encrypted,int VendorId)
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
                    param.Add("@p_VendorComplianceId_Encrypted", VendorComplianceId_Encrypted);
                    param.Add("@p_VendorId", VendorId);
                    param.Add("@p_MachineName", System.Net.Dns.GetHostName());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    sqlcon.Execute("sp_SUD_Mas_Vendor_Compliance", param, commandType: CommandType.StoredProcedure);
                    var Message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    if (!string.IsNullOrEmpty(Message))
                    {
                        return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { Icon = "success" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult SaveUpdate(Mas_Vendor_Compliance Compliance)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    TempData["Message"] = "Your session has expired. Please log in again.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                var param = new DynamicParameters();
                param.Add("@p_process", string.IsNullOrEmpty(Compliance.VendorComplianceId_Encrypted) ? "Save" : "Update");

                param.Add("@p_VendorId", Compliance.VendorId);
                param.Add("@p_VendorComplianceId_Encrypted", Compliance.VendorComplianceId_Encrypted);
                param.Add("@p_AgreementStartDate", Compliance.AgreementStartDate);
                param.Add("@p_AgreementEndDate", Compliance.AgreementEndDate);
                param.Add("@p_ScopeOfServices", Compliance.ScopeOfServices);
                param.Add("@p_SLADetails", Compliance.SLADetails);
                param.Add("@p_ServiceCharges", Compliance.ServiceCharges);
                param.Add("@p_PaymentTerms", Compliance.PaymentTerms);
                param.Add("@p_BankName", Compliance.BankName);
                param.Add("@p_IFSCCode", Compliance.IFSCCode);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 10);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);

                var result = DapperORM.ExecuteReturn("sp_SUD_Mas_Vendor_Compliance", param); // your stored procedure name

                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                return RedirectToAction("ESS_Vendor_Compliance", "ESS_Vendor_Compliance", new { area = "ESS" });
            }
            catch(Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpGet]
        public JsonResult GetComplianceById(string id)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }

                string query = "SELECT VendorComplianceId_Encrypted,ScopeOfServices,AgreementStartDate,AgreementEndDate,SLADetails,ServiceCharges,PaymentTerms,BankName,IFSCCode FROM Mas_Vendor_Compliance WHERE Deactivate = 0 AND VendorComplianceId_Encrypted = '" + id+"'"; ;
                var result = DapperORM.DynamicQuerySingle(query);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }


        }

    }
