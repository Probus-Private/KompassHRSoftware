using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Travel;
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

namespace KompassHR.Areas.ESS.Controllers.ESS_Travel
{
    public class ESS_Travel_PlansController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_Travel_Plans

        public ActionResult ESS_Travel_Plans(string TravelPlanID_Encrypted ,string TravelCalenderID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                Travel_Plans travel = new Travel_Plans();
                Travel_Calender travel2 = new Travel_Calender();
                Session["TravelCalenderID_Encrypted"] = TravelCalenderID_Encrypted;
                ViewBag.AddUpdateTitle = "Add";
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@p_Employeeid", Session["EmployeeId"]);
                    param2.Add("@p_TravelCalenderID_Encrypted", TravelCalenderID_Encrypted);
                   var dt = DapperORM.ReturnList<Travel_Calender>("sp_List_Travel_Calender", param2).FirstOrDefault();
                    ViewBag.TravelPurpose = dt.TravelPurpose;
                    ViewBag.PurposeDescription = dt.PurposeDescription;
                    ViewBag.TravelType = dt.TravelType;
                    ViewBag.TourStart = dt.TourStart;
                    ViewBag.TourEnd = dt.TourEnd;
                    ViewBag.TravelNote = dt.TravelNote;
                    ViewBag.TravelCalenderID = dt.TravelCalenderID;
                    ViewBag.TravelCalenderEmployeeId = dt.TravelCalenderEmployeeId;

                    DynamicParameters param1 = new DynamicParameters();
                    

                    param1.Add("@p_TravelPlanEmployeeId", Session["EmployeeId"]);
                    param1.Add("@p_TravelPlanID_Encrypted", "List");
                    param1.Add("@p_TravelCalenderID", dt.TravelCalenderID);
                    param1.Add("@p_Qry", "and Travel_Plan_Travel.TravelPlanEmployeeId = " + Session["EmployeeId"] + " and Travel_Plan_Travel.TravelCalenderID ="+ dt.TravelCalenderID + " order by TravelPlanID desc");
                    ViewBag.TravelPlanDetails = DapperORM.ReturnList<Travel_Plan_Travel>("sp_List_Travel_Plan_Travel", param1).ToList();

                    DynamicParameters param3 = new DynamicParameters();
                    param3.Add("@p_AccomodationPlanEmployeeId", Session["EmployeeId"]);
                    param3.Add("@p_AccomodationPlanID_Encrypted", "List");
                    param3.Add("@p_TravelCalenderID", dt.TravelCalenderID);
                    param3.Add("@p_Qry", "and Travel_Plan_Accomodation.AccomodationPlanEmployeeId = " + Session["EmployeeId"] + "and  Travel_Plan_Accomodation.TravelCalenderID = " + dt.TravelCalenderID + "order by AccomodationPlanID desc");               
                    ViewBag.AccomodationPlanDetails = DapperORM.ReturnList<Travel_Plan_Accomodation>("sp_List_Travel_Plan_Accomodation", param3).ToList();


                    DynamicParameters param4 = new DynamicParameters();
                    param4.Add("@p_LTPlanEmployeeId", Session["EmployeeId"]);
                    param4.Add("@p_LocalTranPlanID_Encrypted", "List");
                    param4.Add("@p_TravelCalenderID", dt.TravelCalenderID);
                    param4.Add("@p_Qry", "and Travel_Plan_LocalTransport.LTPlanEmployeeId = " + Session["EmployeeId"] + " and Travel_Plan_LocalTransport.TravelCalenderID = " + dt.TravelCalenderID + "order by LocalTranPlanID desc");
                    ViewBag.LocalPlanDetails = DapperORM.ReturnList<Travel_Plan_LocalTransport>("sp_List_Travel_Plan_LocalTransport", param4).ToList();
                    
                    //if (TravelPlanID_Encrypted != null)
                    //{
                    //    var GetDocNoT = "SELECT CASE  WHEN MAX(DocNoT) IS NULL THEN 1 ELSE MAX(DocNoT) + 1  END AS DocNoT FROM Travel_Plan_Travel Where Deactivate=0 and TravelPlanID_Encrypted=" + TravelPlanID_Encrypted;
                    //    var DocNoT = DapperORM.DynamicQuerySingle(GetDocNoT);
                    //    ViewBag.DocNoT = DocNoT;
                    //}
                    //else
                    //{
                    //    var GetDocNoT = "SELECT CASE  WHEN MAX(DocNoT) IS NULL THEN 1 ELSE MAX(DocNoT) + 1  END AS DocNoT FROM Travel_Plan_Travel Where Deactivate=0";
                    //    var DocNoT = DapperORM.DynamicQuerySingle(GetDocNoT);
                    //    ViewBag.DocNoT = DocNoT;
                    //}
                    var GetDocNoT = "SELECT CASE  WHEN MAX(DocNo) IS NULL THEN 1 ELSE MAX(DocNo) + 1  END AS DocNo FROM Travel_Plan_Travel Where Deactivate=0";
                      var DocNoT = DapperORM.DynamicQuerySingle(GetDocNoT);
                      ViewBag.DocNoT = DocNoT;

                    var GetDocNoA = "SELECT CASE  WHEN MAX(DocNo) IS NULL THEN 1 ELSE MAX(DocNo) + 1  END AS DocNo FROM Travel_Plan_Accomodation Where Deactivate=0";
                    var DocNoA = DapperORM.DynamicQuerySingle(GetDocNoA);
                    ViewBag.DocNoA = DocNoA;

                    var GetDocNoLT = "SELECT CASE  WHEN MAX(DocNo) IS NULL THEN 1 ELSE MAX(DocNo) + 1  END AS DocNo FROM Travel_Plan_LocalTransport Where Deactivate=0";
                    var DocNoLT = DapperORM.DynamicQuerySingle(GetDocNoLT);
                    ViewBag.DocNoLT = DocNoLT;

                }
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_Travel_Calender");
            }
        }

        [HttpGet]
        public JsonResult IsTravelPlansExists(string TravelPlanID_Encrypted, DateTime StartDate, DateTime EndDate,int ? TravelCalenderID ,string Mode)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_TravelPlanEmployeeId", Session["EmployeeId"]);
                    param.Add("@p_TravelPlanID_Encrypted", TravelPlanID_Encrypted);
                    param.Add("@p_StartDate", StartDate);
                    param.Add("@p_EndDate", EndDate);
                    param.Add("@p_TravelCalenderID", TravelCalenderID);
                    param.Add("@p_Mode", Mode);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Travel_Plan_Travel", param);
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

        [HttpGet]
        public JsonResult IsAccomodationPlansExists(string AccomodationPlanID_Encrypted, DateTime StartDate, DateTime EndDate, int? TravelCalenderID, string Mode)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_AccomodationPlanEmployeeId", Session["EmployeeId"]);
                    param.Add("@p_AccomodationPlanID_Encrypted", AccomodationPlanID_Encrypted);
                    param.Add("@p_StartDate", StartDate);
                    param.Add("@p_EndDate", EndDate);
                    param.Add("@p_TravelCalenderID", TravelCalenderID);
                    param.Add("@p_Mode", Mode);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Travel_Plan_Accomodation", param);
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
        
        [HttpGet]
        public JsonResult IsLTPlansExists(string LocalTranPlanID_Encrypted, DateTime StartDate, DateTime EndDate, int? TravelCalenderID, string Mode)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_LTPlanEmployeeId", Session["EmployeeId"]);
                    param.Add("@p_LocalTranPlanID_Encrypted", LocalTranPlanID_Encrypted);
                    param.Add("@p_StartDate", StartDate);
                    param.Add("@p_EndDate", EndDate);
                    param.Add("@p_TravelCalenderID", TravelCalenderID);
                    param.Add("@p_Mode", Mode);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Travel_Plan_LocalTransport", param);
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
        
        [HttpPost]
        public ActionResult Save(Travel_Plan_Travel Travel ,HttpPostedFileBase[] UploadDocs)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            try
            {
                if (Travel.TravelPlanID_Encrypted == " " || Travel.TravelPlanID_Encrypted == null)
                {
                    param.Add("@p_process", "Save");
                    param.Add("@P_DocDate", DateTime.Now);
                }
                else
                {
                    param.Add("@p_process", "Update");
                    param.Add("@P_CmpId", Travel.CmpId);
                    param.Add("@P_DocDate", Travel.DocDate);
                }
                param.Add("@p_DocNo", Travel.DocNo);
                param.Add("@p_TravelPlanID_Encrypted", Travel.TravelPlanID_Encrypted);
                param.Add("@p_TravelPlanEmployeeId", Session["EmployeeId"]);
                param.Add("@p_TravelCalenderID", Travel.TravelCalenderID);
                param.Add("@p_PlanType", Travel.PlanType);
                param.Add("@p_Mode", Travel.Mode);
                param.Add("@p_StartDate", Travel.StartDate);
                param.Add("@p_EndDate", Travel.EndDate);
                param.Add("@p_StartTime", Travel.StartTime);
                param.Add("@p_EndTime", Travel.EndTime);
                param.Add("@p_StartLocation", Travel.StartLocation);
                param.Add("@p_EndLocation", Travel.EndLocation);
                param.Add("@p_TravelMealPreference", Travel.TravelMealPreference);
                param.Add("@p_Note", Travel.Note);
                param.Add("@p_BookingType", Travel.BookingType);
                param.Add("@p_BookingStatus", "Pending");
                param.Add("@p_TravelBranchID", Travel.TravelBranchID);
                param.Add("@p_BKAirlineName", Travel.BKAirlineName);
                param.Add("@p_BKAircraftNo", Travel.BKAircraftNo);
                param.Add("@p_BKSeatNo", Travel.BKSeatNo);
                param.Add("@p_BKCheckIn", Travel.BKCheckIn);
                param.Add("@p_BKCheckTime", Travel.BKCheckTime);
                param.Add("@p_BKTakeOffTime", Travel.BKTakeOffTime);
                param.Add("@p_RejectionRemark", Travel.RejectionRemark);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_TravelPlanID1", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Travel_Plan_Travel", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                var GetTravelPlanID = param.Get<string>("@p_TravelPlanID1");


                if (UploadDocs[0] != null)
                {

                    //D:\Documents\VendorConsultant\Accomodation\
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='TravelDesk'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    var FirstPath = GetFirstPath + "Travel" + "\\"; // First path plus concat with new folder
                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }
                    foreach (HttpPostedFileBase UploadDoc in UploadDocs)
                    {
                        if (UploadDoc.ContentLength > 0)
                        {
                            DynamicParameters param = new DynamicParameters();

                            string URLFileFullPath = "";
                            URLFileFullPath = FirstPath + UploadDoc.FileName; //Concat Full Path and create New full Path
                            UploadDoc.SaveAs(URLFileFullPath); // This is use for Save image in folder full path

                            param.Add("@p_process", "Save");
                            param.Add("@p_TravelDocsID", 0);
                            param.Add("@p_TravelDocsID_Encrypted", "");
                            param.Add("@p_TravelPlanEmployeeId", Session["EmployeeId"]);
                            param.Add("@p_TravelCalenderID", Travel.TravelCalenderID);
                            param.Add("@p_TravelPlansID", GetTravelPlanID);
                            param.Add("@p_PlanType", Travel.PlanType);
                            param.Add("@p_Mode", Travel.Mode);
                            param.Add("@p_DocumentName", UploadDoc.FileName);
                            param.Add("@p_FileLocation", "/Travel/" + UploadDoc.FileName);
                            param.Add("@p_Note", Travel.Note);
                            param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                            param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                            param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                            var data1 = DapperORM.ExecuteReturn("sp_SUD_Travel_Documents", param);
                            //TempData["Message"] = param.Get<string>("@p_msg");
                            //TempData["Icon"] = param.Get<string>("@p_Icon");

                        }
                    }
                }
                return RedirectToAction("ESS_Travel_Plans", "ESS_Travel_Plans",new { TravelCalenderID_Encrypted=Convert.ToString(Session["TravelCalenderID_Encrypted"]) } );
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_Travel_Plans");
            }
        }
        
        [HttpPost]
        public ActionResult SaveA(Travel_Plan_Travel Travel, HttpPostedFileBase[] UploadDocs2)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            try
            {
                if (Travel.TravelPlanID_Encrypted == " " || Travel.TravelPlanID_Encrypted == null)
                {
                    param.Add("@p_process", "Save");
                    param.Add("@P_DocDate", DateTime.Now);
                }
                else
                {
                    param.Add("@p_process", "Update");
                    param.Add("@P_CmpId", Travel.CmpId);
                    param.Add("@P_DocDate", Travel.DocDate);
                }
                param.Add("@p_DocNo", Travel.DocNo);
                param.Add("@p_AccomodationPlanID_Encrypted", Travel.TravelPlanID_Encrypted);
                param.Add("@p_AccomodationPlanEmployeeId", Session["EmployeeId"]);
                param.Add("@p_TravelCalenderID", Travel.TravelCalenderID);
                param.Add("@p_PlanType", Travel.PlanType);
                param.Add("@p_Mode", Travel.Mode);
                param.Add("@p_StartDate", Travel.StartDate);
                param.Add("@p_EndDate", Travel.EndDate);
                param.Add("@p_Note", Travel.Note);
                param.Add("@p_BookingType", Travel.BookingType);
                param.Add("@p_BookingStatus", "Pending");
                param.Add("@p_TravelBranchID", Travel.TravelBranchID);
                param.Add("@p_Breakfast", Travel.BKAirlineName);
                param.Add("@p_Lunch", Travel.BKAircraftNo);
                param.Add("@p_Dinner", Travel.BKSeatNo);
                param.Add("@p_BKHotelName", Travel.BKHotelName);
                param.Add("@p_BKHotelAddress", "");
                param.Add("@p_BKHotelMapLocation", "");
                param.Add("@p_BKOccupancyType", "");

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_AccomodationPlanID1", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Travel_Plan_Accomodation", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                var GetTravelPlanID = param.Get<string>("@p_AccomodationPlanID1");

                //D:\Documents\VendorConsultant\Accomodation\

                if(UploadDocs2[0] != null)
                {
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='TravelDesk'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    var FirstPath = GetFirstPath + "Accomodation" + "\\"; // First path plus concat with new folder
                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }
                    foreach (HttpPostedFileBase UploadDoc in UploadDocs2)
                    {


                        if (UploadDoc.ContentLength > 0)
                        {
                            DynamicParameters param = new DynamicParameters();

                            string URLFileFullPath = "";
                            URLFileFullPath = FirstPath + UploadDoc.FileName; //Concat Full Path and create New full Path
                            UploadDoc.SaveAs(URLFileFullPath); // This is use for Save image in folder full path

                            param.Add("@p_process", "Save");
                            param.Add("@p_TravelDocsID", 0);
                            param.Add("@p_TravelDocsID_Encrypted", "");
                            param.Add("@p_TravelPlanEmployeeId", Session["EmployeeId"]);
                            param.Add("@p_TravelCalenderID", Travel.TravelCalenderID);
                            param.Add("@p_TravelPlansID", GetTravelPlanID);
                            param.Add("@p_PlanType", Travel.PlanType);
                            param.Add("@p_Mode", Travel.Mode);
                            param.Add("@p_DocumentName", UploadDoc.FileName);
                            param.Add("@p_FileLocation", "/Accomodation/" + UploadDoc.FileName);
                            param.Add("@p_Note", Travel.Note);
                            param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                            param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                            param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                            var data1 = DapperORM.ExecuteReturn("sp_SUD_Travel_Documents", param);
                            //TempData["Message"] = param.Get<string>("@p_msg");
                            //TempData["Icon"] = param.Get<string>("@p_Icon");

                        }
                    }
                }
              
                
                return RedirectToAction("ESS_Travel_Plans", "ESS_Travel_Plans", new { TravelCalenderID_Encrypted = Convert.ToString(Session["TravelCalenderID_Encrypted"]) });
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_Travel_Plans");
            }
        }

        [HttpPost]
        public ActionResult SaveLT(Travel_Plan_Travel Travel, HttpPostedFileBase[] UploadDocs3)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            try
            {
                if (Travel.TravelPlanID_Encrypted == " " || Travel.TravelPlanID_Encrypted == null)
                {
                    param.Add("@p_process", "Save");
                    param.Add("@P_DocDate", DateTime.Now);
                }
                else
                {
                    param.Add("@p_process", "Update");
                    param.Add("@P_CmpId", Travel.CmpId);
                    param.Add("@P_DocDate", Travel.DocDate);
                }
                param.Add("@p_DocNo", Travel.DocNo);
                param.Add("@p_LocalTranPlanID_Encrypted", Travel.TravelPlanID_Encrypted);
                param.Add("@p_LTPlanEmployeeId", Session["EmployeeId"]);
                param.Add("@p_TravelCalenderID", Travel.TravelCalenderID);
                param.Add("@p_PlanType", Travel.PlanType);
                param.Add("@p_Mode", Travel.Mode);
                param.Add("@p_StartDate", Travel.StartDate);
                param.Add("@p_EndDate", Travel.EndDate);
                param.Add("@p_StartTime", Travel.StartTime);
                param.Add("@p_EndTime", Travel.EndTime);
                param.Add("@p_StartLocation", Travel.StartLocation);
                param.Add("@p_EndLocation", Travel.EndLocation);
                param.Add("@p_TravelMealPreference", Travel.TravelMealPreference);
                param.Add("@p_Note", Travel.Note);
                param.Add("@p_BookingType", Travel.BookingType);
                param.Add("@p_BookingStatus", "Pending");
                param.Add("@p_TravelBranchID", Travel.TravelBranchID);
                param.Add("@p_BKCabType", "");
                param.Add("@p_BKCabContactPerson","");
                param.Add("@p_BKCabBookingInstruction","");
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_LocalTranPlanID1", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter

                var data = DapperORM.ExecuteReturn("sp_SUD_Travel_Plan_LocalTransport", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                var GetTravelPlanID = param.Get<string>("@p_LocalTranPlanID1");

                if ( UploadDocs3 != null)
                {
                    //D:\Documents\VendorConsultant\Accomodation\
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='TravelDesk'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    var FirstPath = GetFirstPath + "LocalTransport" + "\\"; // First path plus concat with new folder
                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }
                    foreach (HttpPostedFileBase UploadDoc in UploadDocs3)
                    {
                        if (UploadDoc.ContentLength > 0)
                        {
                            DynamicParameters param = new DynamicParameters();

                            string URLFileFullPath = "";
                            URLFileFullPath = FirstPath + UploadDoc.FileName; //Concat Full Path and create New full Path
                            UploadDoc.SaveAs(URLFileFullPath); // This is use for Save image in folder full path

                            param.Add("@p_process", "Save");
                            param.Add("@p_TravelDocsID", 0);
                            param.Add("@p_TravelDocsID_Encrypted", "");
                            param.Add("@p_TravelPlanEmployeeId", Session["EmployeeId"]);
                            param.Add("@p_TravelCalenderID", Travel.TravelCalenderID);
                            param.Add("@p_TravelPlansID", GetTravelPlanID);
                            param.Add("@p_PlanType", Travel.PlanType);
                            param.Add("@p_Mode", Travel.Mode);
                            param.Add("@p_DocumentName", UploadDoc.FileName);
                            param.Add("@p_FileLocation", "/LocalTransport/" + UploadDoc.FileName);
                            param.Add("@p_Note", Travel.Note);
                            param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                            param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                            param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                            var data1 = DapperORM.ExecuteReturn("sp_SUD_Travel_Documents", param);
                            //TempData["Message"] = param.Get<string>("@p_msg");
                            //TempData["Icon"] = param.Get<string>("@p_Icon");

                        }
                    }
                }
                
                return RedirectToAction("ESS_Travel_Plans", "ESS_Travel_Plans", new { TravelCalenderID_Encrypted = Convert.ToString(Session["TravelCalenderID_Encrypted"]) });
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_Travel_Plans");
            }
        }
        
        #region Delete
        [HttpGet]
        public ActionResult DeleteTravel(string TravelPlanID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_TravelPlanID_Encrypted", TravelPlanID_Encrypted);
                param.Add("@p_TravelPlanEmployeeId", Session["EmployeeId"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Travel_Plan_Travel", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_Travel_Plans", "ESS_Travel_Plans",new { TravelCalenderID_Encrypted = Convert.ToString(Session["TravelCalenderID_Encrypted"]) });
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Travel_Plans");
            }
        }

        [HttpGet]
        public ActionResult DeleteAccomodation(string AccomodationPlanID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_AccomodationPlanEmployeeId", Session["EmployeeId"]);
                param.Add("@p_AccomodationPlanID_Encrypted", AccomodationPlanID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Travel_Plan_Accomodation", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_Travel_Plans", "ESS_Travel_Plans" , new { TravelCalenderID_Encrypted = Convert.ToString(Session["TravelCalenderID_Encrypted"]) });
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Travel_Plans" );
            }
        }

        [HttpGet]
        public ActionResult DeleteLT(string LocalTranPlanID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_LTPlanEmployeeId", Session["EmployeeId"]);
                param.Add("@p_LocalTranPlanID_Encrypted", LocalTranPlanID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Travel_Plan_LocalTransport", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_Travel_Plans", "ESS_Travel_Plans", new { TravelCalenderID_Encrypted = Convert.ToString(Session["TravelCalenderID_Encrypted"]) });
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Travel_Plans");
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
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_TravelPlanEmployeeId", Session["EmployeeId"]);
                param1.Add("@p_TravelPlanID_Encrypted", "List");
                ViewBag.CalenderDetails = DapperORM.ReturnList<Travel_Plans>("sp_List_Travel_Plans", param1).ToList();
                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Travel_Plans");
            }

        }
        #endregion
    }
}