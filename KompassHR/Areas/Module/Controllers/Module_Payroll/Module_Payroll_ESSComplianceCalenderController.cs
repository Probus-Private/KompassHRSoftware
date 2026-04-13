using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using KompassHR.Areas.Module.Models.Module_Payroll;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Data;
using System.IO;
using System.Net.Mime;
using System.Data.SqlClient;

namespace KompassHR.Areas.Module.Controllers.Module_Payroll
{
    public class Module_Payroll_ESSComplianceCalenderController : Controller
    {
        #region Main View
        // GET: Module/Module_Payroll_ESSComplianceCalender
        public ActionResult Module_Payroll_ESSComplianceCalender(string ComplianceCalenderId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 878;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";

                Payroll_ESSComplianceCalender Payroll_ESSComplianceCalender = new Payroll_ESSComplianceCalender();

                DynamicParameters p1 = new DynamicParameters();
                p1.Add("@query", $"select  ComplianceCategoryId As Id ,ComplianceCategory As Name from Payroll_ComplianceCategory Where Deactivate=0 Order By Name");
                ViewBag.ComplianceCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", p1).ToList();

                //if(Payroll_ESSComplianceCalender.ComplianceCategoryId != null || Payroll_ESSComplianceCalender.ComplianceCategoryId > 0)
                //{
                //    DynamicParameters paramComplianceName = new DynamicParameters();
                //    paramComplianceName.Add("@query", "select ComplianceCalenderId As Id,ComplianceName As Name From Payroll_ComplianceCalenderSetting Where Deactivate=0  and ComplianceCategoryId ='" + Payroll_ESSComplianceCalender.ComplianceCategoryId + "'  order by Name");
                //    var ComplianceName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramComplianceName).ToList();
                //    ViewBag.ComplianceName = ComplianceName;

                //}
                //else
                //{
                    ViewBag.ComplianceName = new List<AllDropDownBind>();
              //  }


                if (ComplianceCalenderId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_ComplianceCalenderId_Encrypted", ComplianceCalenderId_Encrypted);
                    Payroll_ESSComplianceCalender = DapperORM.ReturnList<Payroll_ESSComplianceCalender>("sp_List_Payroll_RaiseComplianceCalender", param).FirstOrDefault();

                    if (Payroll_ESSComplianceCalender.ComplianceCategoryId > 0)
                    {
                        DynamicParameters p2 = new DynamicParameters();
                        p2.Add("@query", $"select ComplianceCalenderId As Id, ComplianceName As Name From Payroll_ComplianceCalenderSetting Where Deactivate = 0  and ComplianceCategoryId = '" + Payroll_ESSComplianceCalender.ComplianceCategoryId + "'  order by Name");
                        ViewBag.ComplianceName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", p2).ToList();
                    }

                    if (Payroll_ESSComplianceCalender.EndDate != null)
                    {
                        TempData["EndDate"] = Payroll_ESSComplianceCalender.EndDate.Value.ToString("yyyy-MM-dd");
                    }

                }
                return View(Payroll_ESSComplianceCalender);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Get Compliance Name
        [HttpGet]
        public ActionResult GetComplianceName(int ComplianceCategoryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramComplianceName = new DynamicParameters();
                paramComplianceName.Add("@query", "select ComplianceCalenderId As Id,ComplianceName As Name From Payroll_ComplianceCalenderSetting Where Deactivate=0  and ComplianceCategoryId ='" + ComplianceCategoryId + "'  order by Name");
                var ComplianceName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramComplianceName).ToList();
                ViewBag.ComplianceName = ComplianceName;

                return Json(new { ComplianceName = ComplianceName }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
        
        #region  GetComplianceType
        [HttpGet]
        public ActionResult GetComplianceType(int ComplianceId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // var GetComplianceType = "select Frequency from Payroll_ComplianceCalenderSetting where Deactivate=0 AND ComplianceCalenderId='" + ComplianceId + "';";
                //  var GetComplianceType = "SELECT CASE WHEN Frequency = 'Monthly' THEN  CONCAT('Monthly (', ComplianceName, ' Monthly Date is ', DAY(MonthDate), ')') WHEN Frequency = 'Quarterly' THEN CONCAT('Quarterly (', ComplianceName, ' Quarterly Date is ', CONVERT(VARCHAR(5), QuarterlyDate, 105), ')') WHEN Frequency = 'Yearly' THEN CONCAT('Yearly (', ComplianceName, ' Yearly Date is ', FORMAT(YearlyDate, 'dd-MMM-yyyy'), ')') ELSE 'Invalid Frequency' END AS Frequency FROM Payroll_ComplianceCalenderSetting WHERE Deactivate = 0  AND ComplianceCalenderId='" + ComplianceId + "';";
              //  var GetComplianceType = "SELECT CASE WHEN Frequency = 'Monthly' THEN CONCAT('Monthly (', ComplianceName, ' Monthly Date is ', CAST(MonthDate AS INT), ')') WHEN Frequency = 'Quarterly'THEN CONCAT('Quarterly (', ComplianceName, ' Quarterly Date is ', QuarterlyDate, ')')  WHEN Frequency = 'Yearly' THEN CONCAT('Yearly (', ComplianceName, ' Yearly Date is ', FORMAT(YearlyDate, 'dd-MMM-yyyy'), ')') ELSE 'Invalid Frequency' END AS Frequency FROM Payroll_ComplianceCalenderSetting WHERE Deactivate = 0 AND ComplianceCalenderId='" + ComplianceId + "';";
              //  var Type = DapperORM.DynamicQuerySingle(GetComplianceType);
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ComplianceId", ComplianceId);
                var Type = DapperORM.ExecuteSP<dynamic>("sp_Payroll_ComplianceCalenderType", param).FirstOrDefault();
                var data = Type.Frequency;
                return Json(data, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsValidation
        public ActionResult IsExists(string ComplianceCalenderId_Encrypted, int? ComplianceId, int? ComplianceCategoryId,  DateTime EndDate,string Type)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_ComplianceCalenderId_Encrypted", ComplianceCalenderId_Encrypted);
                    param.Add("@p_ComplianceId", ComplianceId);
                    param.Add("@p_ComplianceCategoryId", ComplianceCategoryId);
                    param.Add("@p_EndDate", EndDate);
                    param.Add("@p_Type", Type);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_RaiseComplianceCalender", param);
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
        public ActionResult SaveUpdate(Payroll_ESSComplianceCalender obj, HttpPostedFileBase Attachment)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", string.IsNullOrEmpty(obj.ComplianceCalenderId_Encrypted) ? "Save" : "Update");
                param.Add("@p_ComplianceCalenderId_Encrypted", obj.ComplianceCalenderId_Encrypted);
                param.Add("@p_ComplianceId", obj.ComplianceId);
                param.Add("@p_Type", obj.Type);
                param.Add("@p_ComplianceCategoryId", obj.ComplianceCategoryId);
                param.Add("@p_Remark", obj.Remark);
                param.Add("@P_EndDate", obj.EndDate);

                if (obj.ComplianceCalenderId_Encrypted != null && Attachment == null)
                {
                    param.Add("@p_Attachment", Session["SelectedFile"]);
                }
                else
                {
                    param.Add("@p_Attachment", Attachment == null ? "" : Attachment.FileName);
                };
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_RaiseComplianceCalender", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                if (TempData["P_Id"] != null && obj.ComplianceCalenderId_Encrypted != null || Attachment != null)
                {
                    var GetDocPath = DapperORM.QuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='ComplianceCalender'");
                    var GetFirstPath = GetDocPath?.DocInitialPath;
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
                return RedirectToAction("Module_Payroll_ESSComplianceCalender", "Module_Payroll_ESSComplianceCalender");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList 
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 878;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ComplianceCalenderId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Payroll_RaiseComplianceCalender", param).ToList();
                ViewBag.ListDetails = data;
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
        public ActionResult Delete(string ComplianceCalenderId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Delete");
                param.Add("@p_ComplianceCalenderId_Encrypted", ComplianceCalenderId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_RaiseComplianceCalender", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Module_Payroll_ESSComplianceCalender");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Download Attachment
        public ActionResult DownloadAttachment(string DownloadAttachment)
        {
            try
            {
                if (string.IsNullOrEmpty(DownloadAttachment))
                {
                    TempData["Message"] = "Invalid File.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetList", "Module_Payroll_ESSComplianceCalender");
                }

                if (DownloadAttachment == null)
                {
                    TempData["Message"] = "File path information not found.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetList", "Module_Payroll_ESSComplianceCalender");
                }

                var driveLetter = Path.GetPathRoot(DownloadAttachment);
                // Check if the drive exists
                if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
                {
                    TempData["Message"] = $"Drive {driveLetter} does not exist.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetList", "Module_Payroll_ESSComplianceCalender");
                }
                // Construct full file path
                var fullPath = DownloadAttachment;

                // Check if the file exists
                if (!System.IO.File.Exists(fullPath))
                {
                    TempData["Message"] = "File not found on your server.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetList", "Module_Payroll_ESSComplianceCalender");
                }
                // Return the file for download
                var fileName = Path.GetFileName(fullPath);
                return File(fullPath, MediaTypeNames.Application.Octet, fileName);
            }
            catch (Exception ex)
            {
                // Log the error (you can use a logging framework here)
                return new HttpStatusCodeResult(500, $"Internal server error: {ex.Message}");
            }
        }
        #endregion

    }
}