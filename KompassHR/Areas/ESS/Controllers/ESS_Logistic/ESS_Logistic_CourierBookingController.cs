using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Logistic;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Logistic
{
    public class ESS_Logistic_CourierBookingController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region CourierServiceProvider Main View 
        [HttpGet]
        public ActionResult ESS_Logistic_CourierBooking(Logistics_Booking Logistics_Booking)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 770;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";

                var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Logistics_CourierBooking";
                var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                ViewBag.DocNo = DocNo;

                DynamicParameters paramMNG = new DynamicParameters();
                paramMNG.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeeLeft=0 and  EmployeeBranchId in (Select BranchID from UserBranchMapping WHERE EmployeeID=" + Session["EmployeeId"] + " and IsActive=1)    and ContractorId=1 and EmployeeId<>1  order by Name");
                var data1 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramMNG).ToList();
                ViewBag.GetEmployee = data1;
                ViewBag.GetCourierServiceProvider = "";

                // Logistics_Booking Logistics_Booking = new Logistics_Booking();
                if (Logistics_Booking.CourierBookingId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_CourierBookingId_Encrypted", Logistics_Booking.CourierBookingId_Encrypted);
                    Logistics_Booking = DapperORM.ReturnList<Logistics_Booking>("sp_LogiTrack_List_CourierBooking", param).FirstOrDefault();

                    DynamicParameters paramcs = new DynamicParameters();
                    paramcs.Add("@query", "select CourierServiceProviderID as Id, CourierServiceProviderName as Name  from Logistics_CourierServiceProvider where Deactivate=0 and CourierType='" + Logistics_Booking.ParcelType + "';");
                    ViewBag.GetCourierServiceProvider = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramcs).ToList();
                    TempData["Attachment"] = Logistics_Booking.Attachment;
                }
                return View(Logistics_Booking);
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 770;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_CourierBookingId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_LogiTrack_List_CourierBooking", param).ToList();
                ViewBag.GetCourierBookingList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetCourierServiceProvider
        [HttpGet]
        public ActionResult GetCourierServiceProvider(string ParcelType)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "select CourierServiceProviderID as Id, CourierServiceProviderName as Name  from Logistics_CourierServiceProvider where Deactivate=0 and CourierType='" + ParcelType + "';");
                var CourierServiceProvider = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();
                return Json(new { CourierServiceProvider = CourierServiceProvider }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsCourierBookingExists
        public ActionResult IsCourierBookingExists( string CourierBookingId_Encrypted,string BookingEmployeeId, string ParcelType)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", string.IsNullOrEmpty(CourierBookingId_Encrypted) ? "Save" : "Update");
                    param.Add("@p_CourierBookingId_Encrypted",CourierBookingId_Encrypted);
                    param.Add("@p_BookingEmployeeId",BookingEmployeeId);
                    param.Add("@p_ParcelType", ParcelType);
                    //param.Add("@p_Description", Logistics_Booking.Description);
                    //param.Add("@p_CourierServiceProviderId", Logistics_Booking.CourierServiceProviderId);
                    //param.Add("@p_SourceAddress", Logistics_Booking.SourceAddress);
                    //param.Add("@p_DestinationAddress", Logistics_Booking.DestinationAddress);
                    //param.Add("@p_TrackingNumber", Logistics_Booking.TrackingNumber);
                    //param.Add("@p_CourierCharges", Logistics_Booking.CourierCharges);
                    //param.Add("@p_HandOverCourier", Logistics_Booking.HandOverCourier);
                    //param.Add("@p_Attachment", Logistics_Booking.Attachment);
                    //param.Add("@p_IsConfidential", Logistics_Booking.IsConfidential);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_LogiTrack_SUD_CourierBooking", param);
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
        public ActionResult SaveUpdate(Logistics_Booking Logistics_Booking, HttpPostedFileBase Attachment)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(Logistics_Booking.CourierBookingId_Encrypted) ? "Save" : "Update");
                param.Add("@p_CourierBookingId", Logistics_Booking.CourierBookingId);
                param.Add("@p_CourierBookingId_Encrypted", Logistics_Booking.CourierBookingId_Encrypted);
                param.Add("@p_DocNo", Logistics_Booking.DocNo);
                param.Add("@p_DocDate", Logistics_Booking.DocDate);
                param.Add("@p_BookingEmployeeId", Logistics_Booking.BookingEmployeeId);
                param.Add("@p_ParcelType", Logistics_Booking.ParcelType);
                param.Add("@p_Description", Logistics_Booking.Description);
                param.Add("@p_CourierServiceProviderId", Logistics_Booking.CourierServiceProviderId);
                param.Add("@p_SourceAddress", Logistics_Booking.SourceAddress);
                param.Add("@p_DestinationAddress", Logistics_Booking.DestinationAddress);
                param.Add("@p_TrackingNumber", Logistics_Booking.TrackingNumber);
                param.Add("@p_CourierCharges", Logistics_Booking.CourierCharges);
                param.Add("@p_HandOverCourier", Logistics_Booking.HandOverCourier);
                param.Add("@p_Attachment", Attachment == null ? "" : Attachment.FileName);
                param.Add("@p_IsConfidential", Logistics_Booking.IsConfidential);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_LogiTrack_SUD_CourierBooking", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                if (TempData["P_Id"] != null && Logistics_Booking.CourierBookingId_Encrypted != null || Attachment != null)
                {
                    var GetDocPath = DapperORM.QuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='CourierBooking'");
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
                return RedirectToAction("ESS_Logistic_CourierBooking", "ESS_Logistic_CourierBooking");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(int? CourierBookingId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_CourierBookingId", CourierBookingId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_LogiTrack_SUD_CourierBooking", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_Logistic_CourierBooking");
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
                    return RedirectToAction("GetList", "ESS_Logistic_CourierBooking");
                }

                if (DownloadAttachment == null)
                {
                    TempData["Message"] = "File path information not found.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetList", "ESS_Logistic_CourierBooking");
                }

                var driveLetter = Path.GetPathRoot(DownloadAttachment);
                // Check if the drive exists
                if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
                {
                    TempData["Message"] = $"Drive {driveLetter} does not exist.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetList", "ESS_Logistic_CourierBooking");
                }
                // Construct full file path
                var fullPath = DownloadAttachment;

                // Check if the file exists
                if (!System.IO.File.Exists(fullPath))
                {
                    TempData["Message"] = "File not found on your server.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetList", "ESS_Logistic_CourierBooking");
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