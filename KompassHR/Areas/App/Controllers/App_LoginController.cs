using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Drawing;
using System.IO;
using System.Net.Mime;

namespace KompassHR.Areas.App.Controllers
{
    public class App_LoginController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        SqlConnection sqlcons = new SqlConnection(DapperORM.connectionStrings);
        DynamicParameters param = new DynamicParameters();
        // GET: App/App_Login
        #region App Login
        public ActionResult App_Login()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion

        #region Mobile Login All data Maintain In Session Implementation
        [HttpGet]
        public ActionResult App_MobileLogin(string ESSLoginID, string ESSPassword, string ESSCustomerCode)
        {
            try
            {
                var GetConnectionString = sqlcons.Query(@"SELECT Con_Server, Con_UserId, Con_Password, Con_Database, Con_CustomerCode 
                                                   FROM CustomerRegistration 
                                                   WHERE Con_CustomerCode = @Code AND Deactivate = 0 AND Isactive = 1",
                                                         new { Code = ESSCustomerCode }).FirstOrDefault();

                if (GetConnectionString == null || GetConnectionString.Con_CustomerCode != ESSCustomerCode)
                {
                    return Content("Invalid customer code."); // stop processing silently
                }
                DapperORM.SetConnection(GetConnectionString.Con_Server, GetConnectionString.Con_UserId, GetConnectionString.Con_Password, GetConnectionString.Con_Database);
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ESSLoginID", ESSLoginID);
                param.Add("@p_ESSPassword", ESSPassword);
                var result = DapperORM.ExecuteSP<dynamic>("sp_ESSLogin", param).FirstOrDefault();
                if (result == null || result.EmployeeId == null || result.ESSLock == true)
                {
                    return Content("Invalid login credentials or account locked."); // stop processing silently
                }

                var EmpId = result.EmployeeId;
                Session["IsAdmin"] = result.IsAdmin;
                Session["EmployeeId"] = EmpId;
                Session["UserAccessPolicyId"] = result.UserAccessPolicyId;
                Session["CompanyId"] = result.CmpID;
                Session["BranchId"] = result.EmployeeBranchId;
                Session["ESSCustomerCode"] = ESSCustomerCode;
                try
                {
                    // Get logo file name and path
                    var logoData = DapperORM.DynamicQuerySingle("SELECT Logo FROM Mas_CompanyProfile WHERE CompanyId = " + Session["CompanyId"] + "");
                    var pathData = DapperORM.DynamicQuerySingle("SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin = 'CompanyLogo'");
                    //var logoFile = DapperORM.DynamicQuerySingle<string>("SELECT Logo FROM Mas_CompanyProfile WHERE CompanyId = @CompanyId", new { CompanyId = Session["CompanyId"] });
                    //var basePath = DapperORM.DynamicQuerySingle<string>("SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin = 'CompanyLogo'");
                    string logoFile = logoData?.Logo;
                    string basePath = pathData?.DocInitialPath;
                    string fullPath = Path.Combine(basePath, Session["CompanyId"].ToString(), logoFile);
                    Session["CompanyLogo"] = ""; // default

                    if (System.IO.File.Exists(fullPath))
                    {
                        byte[] imageBytes = System.IO.File.ReadAllBytes(fullPath);
                        string mimeType = GetMimeType(fullPath); // get mime type
                        string base64 = Convert.ToBase64String(imageBytes);
                        Session["CompanyLogo"] = $"data:{mimeType};base64,{base64}";
                    }
                }
                catch
                {
                    Session["CompanyLogo"] = "";
                }

                // Log user login
                DynamicParameters paramLog = new DynamicParameters();
                paramLog.Add("@p_process", "Save");
                paramLog.Add("@p_EmployeeId", EmpId);
                paramLog.Add("@p_Status", "Login");
                paramLog.Add("@p_MachineName", Dns.GetHostName().ToString());
                paramLog.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                DapperORM.ExecuteReturn("sp_SUD_Tool_UserLogin", paramLog);

                // Dashboard data
                DynamicParameters Dash = new DynamicParameters();
                Dash.Add("@p_EmployeeId", EmpId);
                var Alldata = DapperORM.ExecuteSP<dynamic>("sp_ESSDashboard", Dash).ToList();

                Session["ESSCustomerCode"] = ESSCustomerCode;

                DynamicParameters paramCount = new DynamicParameters();
                paramCount.Add("@query", "SELECT COUNT(CompanyId) AS Counts FROM Mas_CompanyProfile WHERE Deactivate = 0 AND Isactive = 1");
                var count = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramCount).FirstOrDefault();
                // You can perform conditional logic, logging, etc. here
                if (count?.Counts == null)
                {
                    return Content("Company profile not found.");
                }
                else
                {
                    return new EmptyResult();
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        private string GetMimeType(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();

            switch (ext)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".bmp":
                    return "image/bmp";
                case ".svg":
                    return "image/svg+xml";
                default:
                    return "image"; // fallback
            }
        }



        [HttpGet]
        public ActionResult App_MobileLoginOld(string ESSLoginID, string ESSPassword, string ESSCustomerCode)
        {
            try
            {
                var GetConnectionString = sqlcons.Query(@"Select Con_Server,Con_UserId,Con_Password,Con_Database,Con_CustomerCode from  CustomerRegistration where Con_CustomerCode='" + ESSCustomerCode + "' and Deactivate=0 and Isactive=1").FirstOrDefault();
                if (GetConnectionString == null)
                {
                    var data = "Please enter valid customer code";
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                if (GetConnectionString.Con_CustomerCode == ESSCustomerCode)
                {
                    DapperORM.SetConnection(GetConnectionString.Con_Server, GetConnectionString.Con_UserId, GetConnectionString.Con_Password, GetConnectionString.Con_Database);
                }
                else
                {
                    var data = "Customer code is invalid";
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ESSLoginID", ESSLoginID);
                param.Add("@p_ESSPassword", ESSPassword);
                var result = DapperORM.ExecuteSP<dynamic>("sp_ESSLogin", param).FirstOrDefault();
                if (result != null)
                {
                    var EmpId = result.EmployeeId;
                    Session["IsAdmin"] = result.IsAdmin;
                    Session["EmployeeId"] = EmpId;
                    Session["BranchId"] = result.EmployeeBranchId;
                    Session["UserAccessPolicyId"] = result.UserAccessPolicyId;
                    var ESSLock = result.ESSLock;
                    if (EmpId != null)
                    {
                        if (ESSLock == true)
                        {
                            var data = "Your account has been locked";
                            return Json(data, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            DynamicParameters paramLog = new DynamicParameters();
                            paramLog.Add("@p_process", "Save");
                            paramLog.Add("@p_EmployeeId", EmpId);
                            paramLog.Add("@p_Status", "Login");
                            paramLog.Add("@p_MachineName", Dns.GetHostName().ToString());
                            paramLog.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                            var Result = DapperORM.ExecuteReturn("sp_SUD_Tool_UserLogin", paramLog);
                            DynamicParameters Dash = new DynamicParameters();
                            Dash.Add("@p_EmployeeId", EmpId);
                            var Alldata = DapperORM.ExecuteSP<dynamic>("sp_ESSDashboard", Dash).ToList();

                            Session["ESSCustomerCode"] = ESSCustomerCode;
                            DynamicParameters paramCount = new DynamicParameters();
                            paramCount.Add("@query", "Select Count(CompanyId) As Counts from  Mas_CompanyProfile where Deactivate=0 and Isactive=1");
                            var count = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramCount).FirstOrDefault();
                            if (count.Counts != null)
                            {
                                return RedirectToAction("App_Dashboard", "App_Dashboard", new { area = "App" });
                            }
                            else
                            {
                                var data = "Invalid credentials";
                                return Json(data, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    else
                    {
                        var data = "Invalid loginid and password";
                        return Json(data, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    var data = "Invalid loginid and password";
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var ErrorMsg = "";
                return Json(new { data = ex.Message, ErrorMsg = "DBError" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Profile View
        public ActionResult App_Profile()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_Employeeid", Session["EmployeeId"]);
                var Profile = DapperORM.ReturnList<dynamic>("sp_App_Profile", param).FirstOrDefault();
                ViewBag.Profile = Profile;

                string base64Image = "";
                if (Profile != null && !string.IsNullOrEmpty(Profile.PhotoPath))
                {
                    string filePath = Profile.PhotoPath;

                    if (System.IO.File.Exists(filePath))
                    {
                        byte[] imageBytes = System.IO.File.ReadAllBytes(filePath);
                        base64Image = Convert.ToBase64String(imageBytes);
                    }
                }
                ViewBag.ProfilePhotoBase64 = base64Image;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion

        #region Approve Mobile View
        public ActionResult App_ApproveRejectList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                //DynamicParameters AppRejList = new DynamicParameters();
                //AppRejList.Add("@p_EmployeeId", Session["EmployeeId"]);
                //AppRejList.Add("@p_List", "All");
                //param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                //ViewBag.GetPendingRequestList = DapperORM.ExecuteSP<dynamic>("sp_App_PendingRequest", AppRejList);


                // --- 1. Cancel Leave Request ---
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ManagerID", Session["EmployeeId"]);
                param.Add("@p_ModuleOrigin", "Leave");
                param.Add("@p_ListOrigin", "App_All");
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var cancelLeaveRequestList = (IEnumerable<dynamic>)DapperORM.DynamicList("sp_CancelRequestForApproval", param);

                // --- 2. Cancel Attendance Request ---
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_ManagerID", Session["EmployeeId"]);
                param1.Add("@p_ModuleOrigin", "Attendance");
                param1.Add("@p_ListOrigin", "App_All");
                param1.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var cancelAttendanceRequestList = (IEnumerable<dynamic>)DapperORM.DynamicList("sp_CancelRequestForApproval", param1);

                // --- 3. Pending Request ---
                DynamicParameters appRejList = new DynamicParameters();
                appRejList.Add("@p_EmployeeId", Session["EmployeeId"]);
                appRejList.Add("@p_List", "All");
                appRejList.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var pendingRequestList = (IEnumerable<dynamic>)DapperORM.DynamicList("sp_App_PendingRequest", appRejList);

                // --- 4. Combine all 3 lists ---
                var combinedList = cancelLeaveRequestList
                                    .Concat(cancelAttendanceRequestList)
                                    .Concat(pendingRequestList)
                                    .ToList();

                // --- 5. Assign to ViewBag ---
                ViewBag.GetPendingRequestList = combinedList;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        
        public ActionResult App_ApproveRejectDetails(int? DocId, string EmployeeId, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_DocId", DocId);
                param.Add("@p_Origin", Origin);
                param.Add("@p_EmployeeId", EmployeeId);
                // Assuming DapperORM.connection is your valid SqlConnection
                using (var multi = DapperORM.DynamicMultipleResultList("sp_App_PendingRequest_Detail", param))
                {
                    var GetData = multi.Read<dynamic>().ToList();       
                    var leaveHistory = multi.Read<dynamic>().ToList();
                    ViewBag.GetLeaveHistory = leaveHistory;

                    switch (Origin)
                    {
                        case "Leave":
                            ViewBag.GetLeaveDetails = GetData;
                            break;

                        case "Outdoor":
                            ViewBag.GetOutDoorDetails = GetData;
                            break;

                        case "PunchMissing":
                            ViewBag.GetPunchMissingDetails = GetData;
                            break;

                        case "ShiftChange":
                            ViewBag.GetShiftChangeDetails = GetData;
                            break;

                        case "WorkFromHome":
                            ViewBag.GetWorkFromHomeDetails = GetData;
                            break;

                        case "Coff":
                            ViewBag.GetCoffDetails = GetData;
                            break;

                        case "CO":
                            ViewBag.GetCoffDetails = GetData;
                            break;

                        case "PersonalGatepass":
                            ViewBag.GetPersonalGatepassDetails = GetData;
                            break;

                        case "TravelClaim":
                            ViewBag.GetTravelClaimDetails = GetData;
                            break;

                        case "GeneralClaim":
                            ViewBag.GetGeneralClaimDetails = GetData;
                            break;
                    }
                }
                TempData["Origin"] = Origin;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }

        public ActionResult App_CancelApproveRejectDetails(string Origin, string DocId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                param.Add("@p_ManagerID", Session["EmployeeId"]);
                param.Add("@p_DocId_Encrypted", DocId_Encrypted);
                if (Origin=="Leave" || Origin=="CO")
                {
                    param.Add("@p_ModuleOrigin", "Leave");
                }
                else
                {
                    param.Add("@p_ModuleOrigin", "Attendance");
                }
                param.Add("@p_ListOrigin", Origin);
                //var GetCancelApproval = DapperORM.ExecuteSP<dynamic>("sp_CancelRequestForApproval", param).FirstOrDefault();
                using (var GetCancelApproval = DapperORM.DynamicMultipleResultList("sp_CancelRequestForApproval", param))
                {
                    var GetData = GetCancelApproval.Read<dynamic>().ToList();
                    //var leaveHistory = GetCancelApproval.Read<dynamic>().ToList();
                    //ViewBag.GetLeaveHistory = leaveHistory;

                    switch (Origin)
                    {
                        case "Leave":
                            ViewBag.GetLeaveDetails = GetData;
                            break;

                        case "OD":
                            ViewBag.GetOutDoorDetails = GetData;
                            break;

                        case "PM":
                            ViewBag.GetPunchMissingDetails = GetData;
                            break;

                        case "SC":
                            ViewBag.GetShiftChangeDetails = GetData;
                            break;

                        case "WFH":
                            ViewBag.GetWorkFromHomeDetails = GetData;
                            break;


                        case "CO":
                            ViewBag.GetCoffDetails = GetData;
                            break;

                        case "PG":
                            ViewBag.GetPersonalGatepassDetails = GetData;
                            break;

                        case "TravelClaim":
                            ViewBag.GetTravelClaimDetails = GetData;
                            break;

                        case "GeneralClaim":
                            ViewBag.GetGeneralClaimDetails = GetData;
                            break;
                    }
                }
                TempData["Origin"] = Origin;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }

        #region  Approve Leave Request function
        [HttpGet]
        public ActionResult ApproveRequest(int? DocId, string Encrypted, string Status, string Remark, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 157;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { area = "App" });
                //}
                var ApproverMode = Convert.ToString(Session["ApproverMode"]);
                if (ApproverMode == "Bulk")
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_Origin", Origin);
                    param.Add("@p_DocId_Encrypted", Encrypted);
                    param.Add("@p_DocId", DocId);
                    param.Add("@p_Status", Status);
                    param.Add("@p_ApproveRejectRemark", Remark);
                    param.Add("@p_Managerid", Session["EmployeeId"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data1 = DapperORM.ExecuteReturn("sp_Approved_Rejected_ForAdmin", param);
                    var message1 = param.Get<string>("@p_msg");
                    var Icon1 = param.Get<string>("@p_Icon");
                    TempData["Message"] = message1;
                    TempData["Icon"] = Icon1;
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"], ApproverMode = "Bulk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    param.Add("@p_Origin", Origin);
                    param.Add("@p_DocId_Encrypted", Encrypted);
                    param.Add("@p_DocId", DocId);
                    param.Add("@p_ApproveRejectRemark", Remark);
                    param.Add("@p_Status", Status);
                    param.Add("@p_Managerid", Session["EmployeeId"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_Approved_Rejected", param);
                    var message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    TempData["Message"] = message;
                    TempData["Icon"] = Icon;
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"], ApproverMode = "Approval" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion
        #endregion

        #region  Manager Approved Or Reject Emoployee Received Cancel Request
        [HttpGet]
        public ActionResult RequestForCancelApprove(int? DocId, string Encrypted, string Status, string Remark, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : (262 == 262 || 255 == 255 ? 262 : 255);
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_Origin", Origin);
                param.Add("@p_DocId_Encrypted", Encrypted);
                param.Add("@p_DocId", DocId);
                param.Add("@p_ApproveRejectRemark", Remark);
                param.Add("@p_Status", Status);
                param.Add("@p_Managerid", Session["EmployeeId"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_Cancel_Approved_Rejected", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                //return RedirectToAction("Inbox", "Inbox", new { Area = "Inbox" });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region  Approve Claim
        [HttpGet]
        public ActionResult ApproveClaimRequest(int? DocId, string Encrypted, string Status, string Remark, string Origin, int? ApproveAmount)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 705;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                //return RedirectToAction("Inbox", "Inbox", new { Area = "Inbox" });
                param.Add("@p_Origin", Origin);
                param.Add("@p_DocId_Encrypted", Encrypted);
                param.Add("@p_ApproveRejectRemark", Remark);
                param.Add("@p_DocId", DocId);
                param.Add("@p_Status", Status);
                param.Add("@p_ApproveAmount", ApproveAmount);
                param.Add("@p_Managerid", Session["EmployeeId"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_Approved_Rejected", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                //return RedirectToAction("Inbox", "Inbox", new { Area = "Inbox" });
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region Notification 
        public ActionResult App_Notification()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion

        #region ErrorPage
        public ActionResult App_ErrorPage()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion ErrorPage

        #region SessionExpire
        public ActionResult App_SessionExpire()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion ErrorPage

        public ActionResult DownloadAttachment(string DownloadAttachment)
        {
            try
            {
                if (string.IsNullOrEmpty(DownloadAttachment))
                    return Content("Invalid file path");

                var fullPath = DownloadAttachment;
                if (!System.IO.File.Exists(fullPath))
                    return Content("File not found on server");

                var fileName = Path.GetFileName(fullPath);
                var contentType = MimeMapping.GetMimeMapping(fileName); // auto-detect type
                var fileBytes = System.IO.File.ReadAllBytes(fullPath);

                return File(fileBytes, contentType, fileName); // this triggers browser download
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }

        public ActionResult DownloadAttachment1(string DownloadAttachment)
        {
            try
            {
                if (string.IsNullOrEmpty(DownloadAttachment))
                {
                    return Json(new { Success = false, Message = "Invalid File.", Icon = "error" }, JsonRequestBehavior.AllowGet);
                }

                var fullPath = DownloadAttachment;
                if (!System.IO.File.Exists(fullPath))
                {
                    return Json(new { Success = false, Message = "File not found on your server.", Icon = "error" }, JsonRequestBehavior.AllowGet);
                }

                var driveLetter = Path.GetPathRoot(DownloadAttachment);
                if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
                {
                    return Json(new { Success = false, Message = $"Drive {driveLetter} does not exist.", Icon = "error" }, JsonRequestBehavior.AllowGet);
                }


                var fileName = Path.GetFileName(fullPath);
                byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
                var fileBase64 = Convert.ToBase64String(fileBytes);

                return Json(new
                {
                    Success = true,
                    FileName = fileName,
                    FileData = fileBase64,
                    ContentType = MediaTypeNames.Application.Octet
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = $"Internal server error: {ex.Message}", Icon = "error" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}