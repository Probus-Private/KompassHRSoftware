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
using KompassHR.Areas.Setting.Models.Setting_SaleForce;
using System.IO;

namespace KompassHR.Areas.Setting.Controllers.Setting_SaleForce
{
    public class Setting_SaleForce_ServiceController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        // GET: TMSSetting/ProjectMaster
        public ActionResult Setting_SaleForce_Service(string Encrypted_Id)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 807;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";

                param.Add("@query", "Select Fid as Id,ServiceCategory as Name from MAS_Sercatg where deleted = 0 order by Name desc");
                var ServiceCatList = DapperORM.ReturnList<AllDropDownBind>("Sp_QueryExcution", param).ToList();
                ViewBag.ServiceCatList = ServiceCatList;

                MAS_SERVICE MAS_SERVICE = new MAS_SERVICE();
                if (Encrypted_Id != null)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    param1.Add("@p_Encrypted_Id", Encrypted_Id);
                    MAS_SERVICE = DapperORM.ReturnList<MAS_SERVICE>("sp_List_MAS_SERVICE", param1).FirstOrDefault();
                    TempData["SelectedFile"] = MAS_SERVICE.Attachment;
                    TempData["FileName"] = MAS_SERVICE.Attachment;
                }
              
                return View(MAS_SERVICE);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        public ActionResult IsServiceExists(string Encrypted_Id, int MAS_SERCATG, string Service)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 807;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {

                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_Encrypted_Id", Encrypted_Id);
                    param.Add("@p_Service", Service);
                    param.Add("@p_MAS_SERCATG", MAS_SERCATG);
                    param.Add("@p_msg", dbType: System.Data.DbType.String, direction: System.Data.ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: System.Data.DbType.String, direction: System.Data.ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("Sp_SUD_MAS_SERVICE", param);
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
        public ActionResult SaveUpdate(MAS_SERVICE MAS_SERVICE, HttpPostedFileBase Attachment)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 808;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(MAS_SERVICE.Encrypted_Id) ? "Save" : "Update");
                param.Add("@P_Fid", MAS_SERVICE.Fid);
                param.Add("@P_Encrypted_Id", MAS_SERVICE.Encrypted_Id);
                param.Add("@p_MAS_SERCATG", MAS_SERVICE.MAS_SERCATG);
                param.Add("@p_Service", MAS_SERVICE.Service);
                param.Add("@p_Attachment", Attachment == null ? "" : Attachment.FileName);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                //param.Add("@p_Mip", Dns.GetHostByName(MachineId).AddressList[0].ToString());
                param.Add("@p_UserId", 0);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var data = DapperORM.ExecuteReturn("Sp_SUD_MAS_SERVICE", param);
                var msg = param.Get<string>("@p_msg");
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                
                if (Attachment != null)
                {
                    var GetDocPath = DapperORM.DynamicQueryList("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='SalesForce'").FirstOrDefault();
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    var FirstPath = GetFirstPath + TempData["P_Id"] + "\\";
                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }
                    if (Attachment != null)
                    {
                        string fileFullPath = "";
                        fileFullPath = FirstPath + Attachment.FileName;
                        Attachment.SaveAs(fileFullPath);
                    }
                }
                return RedirectToAction("Setting_SaleForce_Service", "Setting_SaleForce_Service");

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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 807;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_Encrypted_Id", "List");
                var ServiceList = DapperORM.ReturnList<MAS_SERVICE>("sp_List_MAS_SERVICE", param).ToList();
                ViewBag.ServiceList = ServiceList;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        public ActionResult Delete(string Encrypted_Id)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 808;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_Encrypted_Id", Encrypted_Id);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_MAS_SERVICE", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_SaleForce_Service");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
    }
}