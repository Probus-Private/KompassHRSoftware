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
    public class ESS_ClaimReimbusement_TravelClaimController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        // GET: ESS/ESS_ClaimReimbusement_TravelClaim

        #region Main View Page of TravelClaim
        [HttpGet]
        public ActionResult ESS_ClaimReimbusement_TravelClaim()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 173;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var TravelClaimId = DapperORM.DynamicQuerySingle("Select Isnull(Max(VoucherNo),0)+1 As TravelClaimId from Claim_Travel where Deactivate=0");
                TempData["TravelClaimId"] = TravelClaimId?.TravelClaimId;

                param = new DynamicParameters();
                param.Add("@query", "Select TravelPurposeID As Id,TravelPurpose As Name from Claim_TravelPurpose where Deactivate=0");
                var ClaimTravelPurposeId = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.ClaimTravelPurpose = ClaimTravelPurposeId;

                param.Add("@query", "Select VehicleTypeId As Id,VehicleType As Name from Travel_VehicleType where Deactivate=0");
                var GetVehicleType = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.VehicleType = GetVehicleType;

                var EmployeeId = Session["EmployeeId"];
                //param = new DynamicParameters();
                //param.Add("@p_TravelClaimId_Encrypted", "List");
                //param.Add("@P_Qry", "and Status = 'Pending' and TravelClaimEmployeeId='" + EmployeeId + "'");
                //var ClaimTravle = DapperORM.ExecuteSP<dynamic>("sp_List_Claim_Travel", param).ToList();
                //ViewBag.ClaimTravle = ClaimTravle;

                var IsValid = DapperORM.DynamicQuerySingle(@"select * from Claim_GeneralSetting where CmpId= " + @Session["CompanyId"] + " and Deactivate=0 AND ((FromDay <= ToDay AND DAY(GETDATE()) BETWEEN FromDay AND ToDay) OR (FromDay > ToDay AND (DAY(GETDATE()) >= FromDay OR DAY(GETDATE()) <= ToDay))) ");
                TempData["IsValid"] = IsValid != null ? 1 : 0;

                //DynamicParameters EmployeeList = new DynamicParameters();
                //EmployeeList.Add("@p_EmployeeID", Session["EmployeeId"]);
                //EmployeeList.Add("@p_Origin", "ESS");
                //var listMas_Employee = DapperORM.ReturnList<AllDropDownBind>("sp_DropDown_Employee", EmployeeList);
                //ViewBag.GetEmployeeName = listMas_Employee;

                param = new DynamicParameters();

                //var IsValid = DapperORM.DynamicQuerySingle(@"select FromDay from Claim_GeneralSetting where Claim_GeneralSetting.Deactivate=0 and  (day(getdate()) between FromDay  and ToDay ) and CmpId = (select CmpID from Mas_Employee where Mas_Employee.Deactivate=0 and   EmployeeId = " + @Session["EmployeeId"] + " )");
                //TempData["IsValid"] = IsValid.Count();

                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion

        #region IsTravelClaimExists
        [HttpGet]
        public JsonResult IsTravelClaimExists(string TravelClaimIdEncrypted)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_TravelClaimEmployeeId", Session["EmployeeId"]);
                    param.Add("@p_TravelClaimId_Encrypted", TravelClaimIdEncrypted);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Claim_Travel", param);
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
                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region SaveUpdate Travel Claim
        [HttpPost]
        public ActionResult SaveUpdate(HttpPostedFileBase ImgTotalKM, HttpPostedFileBase ImgTransport, HttpPostedFileBase ImgFood, HttpPostedFileBase ImgHotel, HttpPostedFileBase ImgConveyance, HttpPostedFileBase ImgOtherA, HttpPostedFileBase ImgOtherB, Claim_Travel ClaimTravel)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 173;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }



                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_process", string.IsNullOrEmpty(ClaimTravel.TravelClaimId_Encrypted) ? "Save" : "Update");
                param1.Add("@p_TravelClaimId_Encrypted", ClaimTravel.TravelClaimId_Encrypted);
                param1.Add("@p_CreatedUpdateBy", "Admin");
                param1.Add("@p_MachineName", Dns.GetHostName().ToString());
                param1.Add("@p_VoucherDate", ClaimTravel.VoucherDate);
                param1.Add("@p_TravelClaimEmployeeId", Session["EmployeeId"]);
                param1.Add("@p_TravelType", ClaimTravel.TravelType);
                param1.Add("@p_ClaimTravelPurposeId", ClaimTravel.ClaimTravelPurposeId);
                param1.Add("@p_PerKMRate", ClaimTravel.PerKMRate);
                param1.Add("@p_FromDate", ClaimTravel.FromDate);
                param1.Add("@p_ToDate", ClaimTravel.ToDate);
                param1.Add("@p_FromLocation", ClaimTravel.FromLocation);
                param1.Add("@p_ToLocation", ClaimTravel.ToLocation);
                param1.Add("@p_Description", ClaimTravel.Description);
                param1.Add("@p_TotalKM", ClaimTravel.TotalKM);
                param1.Add("@p_Transport", ClaimTravel.Transport);
                param1.Add("@p_TotalKMAmount", ClaimTravel.TotalKMAmount);
                param1.Add("@p_TotalKMPath", ImgTotalKM == null ? "" : ImgTotalKM.FileName);
                param1.Add("@p_TransportPath", ImgTransport == null ? "" : ImgTransport.FileName);
                param1.Add("@p_Food", ClaimTravel.Food);
                param1.Add("@p_FoodPath", ImgFood == null ? "" : ImgFood.FileName);
                param1.Add("@p_Hotel", ClaimTravel.Hotel);
                param1.Add("@p_HotelPath", ImgHotel == null ? "" : ImgHotel.FileName);
                param1.Add("@p_Conveyance", ClaimTravel.Conveyance);
                param1.Add("@p_ConveyanceRemark", ClaimTravel.ConveyanceRemark);
                param1.Add("@p_ConveyancePath", ImgConveyance == null ? "" : ImgConveyance.FileName);
                param1.Add("@p_OtherA", ClaimTravel.OtherA);
                param1.Add("@p_OtherARemark", ClaimTravel.OtherARemark);
                param1.Add("@p_OtherAPath", ImgOtherA == null ? "" : ImgOtherA.FileName);
                param1.Add("@p_OtherB", ClaimTravel.OtherB);
                param1.Add("@p_OtherBRemark", ClaimTravel.OtherBRemark);
                param1.Add("@p_OtherBPath", ImgOtherB == null ? "" : ImgOtherB.FileName);
                param1.Add("@p_TotalAmount", ClaimTravel.TotalAmount);
                param1.Add("@p_ApprovedAmount", ClaimTravel.ApprovedAmount);
                param1.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param1.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param1.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Claim_Travel", param1);
                TempData["Message"] = param1.Get<string>("@p_msg");
                TempData["Icon"] = param1.Get<string>("@p_Icon");
                TempData["P_Id"] = param1.Get<string>("@p_Id");
                if (TempData["P_Id"] != null)
                {
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Claim_Travel'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    var FirstPath = GetFirstPath + TempData["P_Id"] + "\\"; // First path plus concat folder by Id
                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }

                    if (ImgTotalKM != null)
                    {
                        string ImgTotalKMFullPath = "";
                        ImgTotalKMFullPath = FirstPath + ImgTotalKM.FileName; //Concat Full Path and create New full Path
                        ImgTotalKM.SaveAs(ImgTotalKMFullPath); // This is use for Save image in folder full path
                    }

                    if (ImgTransport != null)
                    {
                        string ImgTransportFullPath = "";
                        ImgTransportFullPath = FirstPath + ImgTransport.FileName; //Concat Full Path and create New full Path
                        ImgTransport.SaveAs(ImgTransportFullPath); // This is use for Save image in folder full path
                    }

                    if (ImgFood != null)
                    {
                        string ImgFoodFullPath = "";
                        ImgFoodFullPath = FirstPath + ImgFood.FileName; //Concat Full Path and create New full Path
                        ImgFood.SaveAs(ImgFoodFullPath); // This is use for Save image in folder full path
                    }

                    if (ImgHotel != null)
                    {
                        string ImgHotelFullPath = "";
                        ImgHotelFullPath = FirstPath + ImgHotel.FileName; //Concat Full Path and create New full Path
                        ImgHotel.SaveAs(ImgHotelFullPath); // This is use for Save image in folder full path
                    }

                    if (ImgConveyance != null)
                    {
                        string ImgConveyanceFullPath = "";
                        ImgConveyanceFullPath = FirstPath + ImgConveyance.FileName; //Concat Full Path and create New full Path
                        ImgConveyance.SaveAs(ImgConveyanceFullPath); // This is use for Save image in folder full path
                    }

                    if (ImgOtherA != null)
                    {
                        string ImgOtherAFullPath = "";
                        ImgOtherAFullPath = FirstPath + ImgOtherA.FileName; //Concat Full Path and create New full Path
                        ImgOtherA.SaveAs(ImgOtherAFullPath); // This is use for Save image in folder full path
                    }

                    if (ImgOtherB != null)
                    {
                        string ImgOtherBFullPath = "";
                        ImgOtherBFullPath = FirstPath + ImgOtherB.FileName; //Concat Full Path and create New full Path
                        ImgOtherB.SaveAs(ImgOtherBFullPath); // This is use for Save image in folder full path
                    }
                }

                return RedirectToAction("ESS_ClaimReimbusement_TravelClaim", "ESS_ClaimReimbusement_TravelClaim", new { Area = "ESS" });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message.ToString();
            }
            return RedirectToAction("ESS_ClaimReimbusement_TravelClaim", "ESS_ClaimReimbusement_TravelClaim", new { Area = "ESS" });
        }
        #endregion

        #region GetList
        public ActionResult GetList(string TravelClaimId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 173;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters ParamManager = new DynamicParameters();
                ParamManager.Add("@query", @"Select EmployeeId as Id , EmployeeName as Name from mas_employee_reporting,mas_employee
                                                where reportingmoduleid = 2 and ReportingEmployeeID = " + Session["EmployeeId"] + " and mas_employee_reporting.Deactivate = 0 and mas_employee_reporting.ReportingManager1 = mas_employee.EmployeeId");
                var Getdata = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", ParamManager);
                ViewBag.GetManagerEmployee = Getdata;

                DynamicParameters ClaimTravel = new DynamicParameters();
                var EmployeeId = Session["EmployeeId"];
                ClaimTravel.Add("@p_TravelClaimId_Encrypted", "List");
                ClaimTravel.Add("@P_Qry", "and TravelClaimEmployeeId ='" + EmployeeId + "'");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Claim_Travel", ClaimTravel).ToList();
                if (data != null)
                {
                    ViewBag.GetClaim_Travel = data;
                }
                else
                {
                    ViewBag.GetClaim_Travel = "";
                }

                return View();
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        #endregion

        #region EmployeeChange
        public ActionResult EmployeeChange()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 173;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@query", "select TravelRateId,VehicalType from Claim_TravelRate Rate Right join Mas_Branch Branch On Branch.BranchId = Rate.TravelRateBranchId where Rate.CmpId=" + Session["CompanyId"] + " and  Branch.BranchId=" + Session["EmployeeBranchId"] + "");
                var data = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).ToList();
                return Json(new { data = data }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Delete TravelClaim
        public ActionResult TravelClaimDelete(string TravelClaimId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 173;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_TravelClaimId_Encrypted", TravelClaimId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Claim_Travel", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_ClaimReimbusement_TravelClaim", new { Area = "ESS" });
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_ClaimReimbusement_TravelClaim");
            }

        }
        #endregion

        #region Download Image 
       
        #endregion

        #region Get TravelRate
        public ActionResult GetTravelRate(string TravelType)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 173;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var GetRate = DapperORM.DynamicQuerySingle("Select TravelRateAmount from Claim_TravelRate Where  VehicalType='" + TravelType + "' and CmpId= " + Session["CompanyId"] + " and TravelRateBranchId=" + Session["EmployeeBranchId"] + " and Deactivate=0");
                if (GetRate != null)
                {
                    return Json(new { data = GetRate.TravelRateAmount }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        #region GetContactDetails
        [HttpGet]
        public ActionResult GetContactDetails(string TravelClaimId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 173;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_Origin", "TravelClaim");
                param.Add("@p_DocId_Encrypted", TravelClaimId_Encrypted);
                var GetGenralCliamlist = DapperORM.ExecuteSP<dynamic>("Sp_GetManager_Module", param).ToList();
                var GenralCliamlist = GetGenralCliamlist;
                return Json(new { data = GenralCliamlist }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception Ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet); 
            }
        }
        #endregion

        #region TravelClaimApproved List
        public ActionResult TravelClaimList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 173;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters ClaimTravel = new DynamicParameters();
                var EmployeeId = Session["EmployeeId"];
                ClaimTravel.Add("@p_TravelClaimId_Encrypted", "List");
                ClaimTravel.Add("@P_Qry", "and TravelClaimEmployeeId ='" + EmployeeId + "'");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Claim_Travel", ClaimTravel).ToList();
                if (data.Count != 0)
                {
                    ViewBag.GetClaim_TravelList = data;
                }
                else
                {
                    ViewBag.GetClaim_TravelList = "";
                }

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
        public ActionResult CancelRequest(int? TravelClaimId, int ddlManagerId, string Remark, string Origin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@p_Origin", Origin);
                param.Add("@p_DocId", TravelClaimId);
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
                //if (!System.IO.File.Exists(fullPath))
                //{
                //    return Json(new { Success = false, Message = "File not found on your server.", Icon = "error" }, JsonRequestBehavior.AllowGet);
                //}

                var driveLetter = Path.GetPathRoot(DownloadAttachment);
                if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
                {
                    return Json(new { Success = false, Message = $"Drive {driveLetter} does not exist.", Icon = "error" }, JsonRequestBehavior.AllowGet);
                }

                var fileName = Path.GetFileName(fullPath);
                byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
                var fileBase64 = Convert.ToBase64String(fileBytes);

                //return Json(new { Success = true, FileName = fileName, FileData = fileBase64, ContentType = MediaTypeNames.Application.Octet }, JsonRequestBehavior.AllowGet);
                var jsonResult = Json(new
                {
                    Success = true,
                    FileName = fileName,
                    FileData = fileBase64,
                    ContentType = MediaTypeNames.Application.Octet
                });

                jsonResult.MaxJsonLength = int.MaxValue;   // This line fixes large files

                return jsonResult;


            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = $"Internal server error: {ex.Message}", Icon = "error" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region GetHistory
        [HttpGet]
        public ActionResult GetHistory(string TravelClaimId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                    DynamicParameters MulQuery = new DynamicParameters();
                    dynamic ApprovalHistory = "";
                    MulQuery.Add("@p_DocId_Encrypted", TravelClaimId_Encrypted);
                    MulQuery.Add("@p_Origin", "TravelClaim");
                    using (var multi = DapperORM.DynamicMultipleResultList("sp_List_Claim_Profiles", MulQuery))
                    {
                        ViewBag.ClaimDetails = multi.Read<dynamic>().FirstOrDefault();                        
                        ViewBag.PreviousExpenses = multi.Read<dynamic>().ToList();
                        ApprovalHistory = multi.Read<dynamic>().ToList();
                    }
                return Json(new { ApprovalHistory }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
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
                param.Add("@p_Origin", "TravelClaim");
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
