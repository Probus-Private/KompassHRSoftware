using Dapper;
using KompassHR.Areas.ESS.Models.ESS_ClaimReimbusement;
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

namespace KompassHR.Areas.ESS.Controllers.ESS_ClaimReimbusement
{
    public class ESS_ClaimReimbusement_GeneralClaimController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_ClaimReimbusement_GeneralClaim
        #region GeneralClaim Main View
        [HttpGet]
        public ActionResult ESS_ClaimReimbusement_GeneralClaim(string GeneralClaimId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 174;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Claim_GeneralClaim Claim_GeneralClaim = new Claim_GeneralClaim();
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    DynamicParameters param1 = new DynamicParameters();
                    var GetVoucherNo = "Select Isnull(Max(DocNo),0)+1 As VoucherNo from Claim_GeneralClaim";
                    var VoucherNo = DapperORM.DynamicQuerySingle(GetVoucherNo);
                    ViewBag.VoucherNo = VoucherNo;

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@query", "SELECT ExpenseCategoryID As Id,ExpenseCategoryName As Name FROM Claim_ExpenseCategory where Deactivate = 0 order by  ExpenseCategoryName");
                    var GetExpenceCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();
                    ViewBag.GetExpenceCategory = GetExpenceCategory;

                    //param = new DynamicParameters();
                    //var EmployeeId = Session["EmployeeId"];
                    //param.Add("@p_GeneralClaimId_Encrypted", "List");
                    //param.Add("@P_Qry", "and Status = 'Pending' and GeneralClaimEmployeeId=" + EmployeeId + "");
                    //var data = DapperORM.ExecuteSP<dynamic>("sp_List_Claim_GeneralClaim", param).ToList();
                    //ViewBag.GeneralCliam = data;

                    //DynamicParameters EmployeeList = new DynamicParameters();
                    //EmployeeList.Add("@p_EmployeeID", Session["EmployeeId"]);
                    //EmployeeList.Add("@p_Origin", "ESS");
                    //var listMas_Employee = DapperORM.ReturnList<AllDropDownBind>("sp_DropDown_Employee", EmployeeList);
                    //ViewBag.GetEmployeeName = listMas_Employee;

                    var IsValid = DapperORM.DynamicQuerySingle(@"select * from Claim_GeneralSetting where CmpId= " + @Session["CompanyId"] + " and Deactivate=0 AND ((FromDay <= ToDay AND DAY(GETDATE()) BETWEEN FromDay AND ToDay) OR (FromDay > ToDay AND (DAY(GETDATE()) >= FromDay OR DAY(GETDATE()) <= ToDay))) ");

                    //TempData["IsValid"] = IsValid?.Count() ?? 0; 
                    TempData["IsValid"] = IsValid != null ? 1 : 0;
                }


                if (GeneralClaimId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    var EmployeeId = Session["EmployeeId"];
                    param.Add("@p_GeneralClaimId_Encrypted", GeneralClaimId_Encrypted);
                    param.Add("@P_GeneralClaimEmployeeId", EmployeeId);
                    Claim_GeneralClaim = DapperORM.ReturnList<Claim_GeneralClaim>("sp_List_Claim_GeneralClaim", param).FirstOrDefault();
                    TempData["VoucherDate"] = Claim_GeneralClaim.DocDate;
                    TempData["FilePath"] = Claim_GeneralClaim.AttachmentPath;
                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {
                        param = new DynamicParameters();
                        var GetVoucherNo = "Select VoucherNo from Claim_GeneralClaim where GeneralClaimId_Encrypted='" + GeneralClaimId_Encrypted + "'";
                        var VoucherNo = DapperORM.DynamicQuerySingle(GetVoucherNo);
                        ViewBag.VoucherNo = VoucherNo;

                    }

                }

                return View(Claim_GeneralClaim);
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_ClaimReimbusement_GeneralClaim");
            }
        }
        #endregion

        #region IsValidation
        //, HttpPostedFileBase file
        public JsonResult IsGeneralClaimsExists(DateTime VoucherDate, string GeneralClaimId_Encrypted, double ExpenceCategoryId, double ExpenceAmount, string ExpenceDescription, string AttachmentPath)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var EmployeeId = Session["EmployeeId"];

                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_GeneralClaimId_Encrypted", GeneralClaimId_Encrypted);
                    param.Add("@p_GeneralClaimEmployeeId", EmployeeId);
                    param.Add("@p_DocDate", VoucherDate);
                    param.Add("@p_GeneralClaimExpenceCategoryID", ExpenceCategoryId);
                    param.Add("@p_GeneralClaimAmount", ExpenceAmount);
                    param.Add("@p_GeneralClaimDescription", ExpenceDescription);
                    param.Add("@p_AttachmentPath", AttachmentPath);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Claim_GeneralClaim", param);
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
            catch (Exception Ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet); ;
            }
        }

        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(HttpPostedFileBase AttachmentPath, Claim_GeneralClaim Claim_GeneralClaim)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 174;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(Claim_GeneralClaim.GeneralClaimId_Encrypted) ? "Save" : "Update");
                param.Add("@p_GeneralClaimId", Claim_GeneralClaim.GeneralClaimId);
                param.Add("@p_GeneralClaimId_Encrypted", Claim_GeneralClaim.GeneralClaimId_Encrypted);
                param.Add("@p_CmpId", Session["CompanyId"]);
                param.Add("@p_GeneralClaimEmployeeId", EmployeeId);
                param.Add("@p_DocNo", Claim_GeneralClaim.DocNo);
                param.Add("@p_DocDate", Claim_GeneralClaim.DocDate);
                param.Add("@p_FromDate", Claim_GeneralClaim.FromDate);
                param.Add("@p_ToDate", Claim_GeneralClaim.ToDate);
                param.Add("@p_GeneralClaimExpenceCategoryID", Claim_GeneralClaim.GeneralClaimExpenseCategoryID);
                param.Add("@p_GeneralClaimAmount", Claim_GeneralClaim.GeneralClaimAmount);
                param.Add("@p_Status", "Pending");
                param.Add("@p_GeneralClaimDescription", Claim_GeneralClaim.GeneralClaimDescription);
                if (AttachmentPath != null)
                    param.Add("@p_AttachmentPath", AttachmentPath.FileName);// Claim_GeneralClaim.AttachmentPath);
                else
                    param.Add("@p_AttachmentPath", "");
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Claim_GeneralClaim", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                if (TempData["P_Id"] != null)
                {
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Claim_General'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    var FirstPath = GetFirstPath + TempData["P_Id"] + "\\";// First path plus concat folder by Id

                    var driveLetter = Path.GetPathRoot(FirstPath);
                    // Check if the drive exists
                    //if (!System.IO.File.Exists(FirstPath))
                    //{
                    //    TempData["Message"] = "File not found on the server.";
                    //    TempData["Icon"] = "error";
                    //    return RedirectToAction("ESS_ClaimReimbusement_GeneralClaim");
                    //}             
                    if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
                    {
                        TempData["Message"] = $"Drive {driveLetter} does not exist.";
                        TempData["Icon"] = "error";
                        return RedirectToAction("ESS_ClaimReimbusement_GeneralClaim");
                    }
                    // Check if the file exists

                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }

                    if (AttachmentPath != null)
                    {
                        string ImgGeneralClaimFilePath = "";
                        ImgGeneralClaimFilePath = FirstPath + AttachmentPath.FileName; //Concat Full Path and create New full Path
                        AttachmentPath.SaveAs(ImgGeneralClaimFilePath); // This is use for Save image in folder full path
                    }
                }
                return RedirectToAction("ESS_ClaimReimbusement_GeneralClaim", "ESS_ClaimReimbusement_GeneralClaim");
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_ClaimReimbusement_GeneralClaim");
            }
        }

        #endregion

        #region GetContactDetails
        [HttpGet]
        public ActionResult GetContactDetails(string GeneralClaimId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 174;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_Origin", "GeneralClaim");
                param.Add("@p_DocId_Encrypted", GeneralClaimId_Encrypted);
                var GetGenralCliamlist = DapperORM.ExecuteSP<dynamic>("Sp_GetManager_Module", param).ToList();
                var GenralCliamlist = GetGenralCliamlist;
                return Json(new { data = GenralCliamlist }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception Ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet); ;
            }
        }
        #endregion

        #region GetList
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 174;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters ParamManager = new DynamicParameters();
                ParamManager.Add("@query", @"Select EmployeeId as Id , EmployeeName as Name from mas_employee_reporting,mas_employee
                                                where reportingmoduleid = 2 and ReportingEmployeeID = " + Session["EmployeeId"] + " and mas_employee_reporting.Deactivate = 0 and mas_employee_reporting.ReportingManager1 = mas_employee.EmployeeId");
                var Getdata = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", ParamManager);
                ViewBag.GetManagerEmployee = Getdata;

                DynamicParameters ClaimTravel = new DynamicParameters();
                var EmployeeId = Session["EmployeeId"];
                ClaimTravel.Add("@p_GeneralClaimId_Encrypted", "List");
                ClaimTravel.Add("@P_Qry", "GeneralClaimEmployeeId ='" + EmployeeId + "'");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Claim_GeneralClaim", ClaimTravel).ToList();
                ViewBag.GetGeneralClaimList = data;
                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_ClaimReimbusement_GeneralClaim");
            }
        }
        #endregion

        #region ViewForGeneralClaim
        public ActionResult ViewForGeneralClaim(string GeneralClaimId_Encrypted,string Type)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 174;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_DocId_Encrypted", GeneralClaimId_Encrypted);
                param.Add("@p_Origin", Type);
                var GetClaimApproval = DapperORM.ExecuteSP<dynamic>("sp_List_Claim_Profiles", param).FirstOrDefault();
                ViewBag.GeneralClaimApprovalList = GetClaimApproval;
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Delete
        [HttpGet]
        public ActionResult Delete(string GeneralClaimId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_GeneralClaimId_Encrypted", GeneralClaimId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Claim_GeneralClaim", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_ClaimReimbusement_GeneralClaim");
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_ClaimReimbusement_GeneralClaim");
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

        #endregion

        #region DownloadAttachment
        //public ActionResult DownloadAttachment1(string GeneralClaimId_Encrypted, int GeneralClaimId)
        //{
        //    try
        //    {
        //        var EmployeeId = Session["EmployeeId"];
        //        if (EmployeeId != null)
        //        {
        //            using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
        //            {
        //                var AttachmentPath = DapperORM.DynamicQuerySingle("Select AttachmentPath from Claim_GeneralClaim where GeneralClaimId_Encrypted='" + GeneralClaimId_Encrypted + "'");
        //                var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Claim_General'");

        //                var APath = GetDocPath.DocInitialPath + GeneralClaimId + '\\' + AttachmentPath.AttachmentPath;

        //                var driveLetter = Path.GetPathRoot(APath);
        //                // Check if the drive exists
        //                if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
        //                {
        //                    TempData["Message"] = $"Drive {driveLetter} does not exist.";
        //                    TempData["Icon"] = "error";
        //                    return RedirectToAction("GeneralClaimList", "ESS_ClaimReimbusement_GeneralClaim", new { area = "ESS" });
        //                }
        //                // Construct full file path
        //                var fullPath = APath;
        //                // Check if the file exists
        //                if (!System.IO.File.Exists(fullPath))
        //                {
        //                    TempData["Message"] = "File not found on the server.";
        //                    TempData["Icon"] = "error";
        //                    return RedirectToAction("GeneralClaimList", "ESS_ClaimReimbusement_GeneralClaim", new { area = "ESS" });

        //                }
        //                // Return the file for download
        //                var fileName = Path.GetFileName(fullPath);
        //                return File(fullPath, MediaTypeNames.Application.Octet, fileName);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the error (you can use a logging framework here)
        //        return new HttpStatusCodeResult(500, $"Internal server error: {ex.Message}");
        //    }
        //}
        #endregion

        #region GeneralClaimList List
        public ActionResult GeneralClaimList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 174;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters ClaimTravel = new DynamicParameters();
                var EmployeeId = Session["EmployeeId"];
                ClaimTravel.Add("@p_GeneralClaimId_Encrypted", "List");
                ClaimTravel.Add("@P_Qry", " and GeneralClaimEmployeeId ='" + EmployeeId + "'");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Claim_GeneralClaim", ClaimTravel).ToList();
                ViewBag.GeneralClaimList = data;
                return View();
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        #endregion

        #region Claim Cancel Request Request function
        [HttpGet]
        public ActionResult CancelRequest(int? GeneralClaimId, int ddlManagerId, string Remark, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@p_Origin", Origin);
                param.Add("@p_DocId", GeneralClaimId);
                param.Add("@p_ManagerID", ddlManagerId);
                param.Add("@p_CancelRemark", Remark);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Tra_RequestCancel", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region RequestStatus
        [HttpGet]
        public ActionResult RequestStatus(int DocId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_DocID", DocId);
                param.Add("@p_Origin", "GeneralClaim");
                var data = DapperORM.ExecuteSP<dynamic>("sp_RequestTimeLine", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);

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