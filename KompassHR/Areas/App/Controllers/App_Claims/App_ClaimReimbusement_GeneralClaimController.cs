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
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.App.Controllers.App_Claims
{
    public class App_ClaimReimbusement_GeneralClaimController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/App_ClaimReimbusement_GeneralClaim
        #region GeneralClaim Main View
        [HttpGet]
        public ActionResult App_ClaimReimbusement_GeneralClaim(string GeneralClaimId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 174;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { Area = "App" });
                //}
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

                    var IsValid = DapperORM.DynamicQueryList(@"select * from Claim_GeneralSetting where CmpId= " + @Session["CompanyId"] + " and Deactivate=0 and day(getdate()) between  [FromDay] and [ToDay] ").ToList();
                    //TempData["IsValid"] = IsValid.Count();
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
                   
                        param = new DynamicParameters();
                        var GetVoucherNo = "Select VoucherNo from Claim_GeneralClaim where GeneralClaimId_Encrypted='" + GeneralClaimId_Encrypted + "'";
                        var VoucherNo = DapperORM.DynamicQueryList(GetVoucherNo).FirstOrDefault();
                        ViewBag.VoucherNo = VoucherNo;
                }

                return View(Claim_GeneralClaim);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion

        #region GetList
        [HttpGet]
        public ActionResult App_GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 174;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { Area = "App" });
                //}
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
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion

        #region IsValidation
        //, HttpPostedFileBase file
        public JsonResult IsGeneralClaimsExists(DateTime VoucherDate, string GeneralClaimId_Encrypted, double ExpenceCategoryId, double ExpenceAmount, string ExpenceDescription, string AttachmentPath)
        {
            try
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
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 174;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { Area = "App" });
                //}
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
                    if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
                    {
                        TempData["Message"] = $"Drive {driveLetter} does not exist.";
                        TempData["Icon"] = "error";
                        return RedirectToAction("App_ClaimReimbusement_GeneralClaim");
                    }
                    // Check if the file exists
                    //if (!System.IO.File.Exists(FirstPath))
                    //{
                    //    TempData["Message"] = "File not found on the server.";
                    //    TempData["Icon"] = "error";
                    //    return RedirectToAction("App_ClaimReimbusement_GeneralClaim");
                    //}
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
                return RedirectToAction("App_ClaimReimbusement_GeneralClaim", "App_ClaimReimbusement_GeneralClaim");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
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
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
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
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
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
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
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
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
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
                return RedirectToAction("App_GetList", "App_ClaimReimbusement_GeneralClaim");
            }
            catch (Exception Ex)
            {
                Session["GetErrorMessage"] = Ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }

        }

        #endregion

        #region ClaimsDetails
       
        [HttpGet]
        public ActionResult App_GeneralDetails(int? GeneralClaimId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 146;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("App_Dashboard", "App_Dashboard", new { Area = "App" });
                //}

                param.Add("@p_GeneralClaimId_Encrypted", "List");
                param.Add("@P_Qry", "Claim_GeneralClaim.GeneralClaimEmployeeId=" + Session["EmployeeId"] + " And GeneralClaimId=" + GeneralClaimId + " ");
                var GeneralClaim = DapperORM.ReturnList<dynamic>("sp_List_Claim_GeneralClaim", param).ToList();
                ViewBag.GeneralClaim = GeneralClaim;

                DynamicParameters ParamManager = new DynamicParameters();
                ParamManager.Add("@query", @"Select EmployeeId as Id , EmployeeName as Name from mas_employee_reporting,mas_employee
                                                where reportingmoduleid = 1 and ReportingEmployeeID = " + Session["EmployeeId"] + " and mas_employee_reporting.Deactivate = 0 and mas_employee_reporting.ReportingManager1 = mas_employee.EmployeeId");
                var Getdata = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", ParamManager);
                ViewBag.GetManagerEmployee = Getdata;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
       
        #endregion

        #region DownloadFile
        public ActionResult DownloadFile(string fileName)
        {
            try
            {
                if (fileName != null)
                {
                    string filePath = Path.Combine(fileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                        string afileName = Path.GetFileName(fileName);
                        return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, afileName);
                    }
                    return HttpNotFound("File not found.");
                }
                else
                {
                    return HttpNotFound("File not found.");
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion
    }
}