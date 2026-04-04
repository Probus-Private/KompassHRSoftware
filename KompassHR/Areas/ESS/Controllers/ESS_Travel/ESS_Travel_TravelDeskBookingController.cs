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
    public class ESS_Travel_TravelDeskBookingController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param1 = new DynamicParameters();

        // GET: ESS/ESS_Travel_TravelDeskBooking
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
                param1.Add("@p_TravelCalenderID", 0);
                param1.Add("@P_Qry", "and BookingStatus not in('Pending')and BookingType='Travel Desk' order by TravelPlanID desc");
                ViewBag.TravelPlanDetails = DapperORM.ReturnList<Travel_Plan_Travel>("sp_List_Travel_Plan_Travel", param1).ToList();

                DynamicParameters param3 = new DynamicParameters();
                param3.Add("@p_AccomodationPlanEmployeeId", Session["EmployeeId"]);
                param3.Add("@p_AccomodationPlanID_Encrypted", "List");
                param3.Add("@p_TravelCalenderID", 0);
                param3.Add("@P_Qry", "and BookingStatus not in('Pending')and BookingType='Travel Desk' order by AccomodationPlanID desc");
                ViewBag.AccomodationPlanDetails = DapperORM.ReturnList<Travel_Plan_Accomodation>("sp_List_Travel_Plan_Accomodation", param3).ToList();

                DynamicParameters param4 = new DynamicParameters();
                param4.Add("@p_LTPlanEmployeeId", Session["EmployeeId"]);
                param4.Add("@p_LocalTranPlanID_Encrypted", "List");
                param4.Add("@p_TravelCalenderID", 0);
                param4.Add("@P_Qry", "and BookingStatus not in('Pending') and BookingType='Travel Desk' order by LocalTranPlanID desc");

                ViewBag.LocalPlanDetails = DapperORM.ReturnList<Travel_Plan_LocalTransport>("sp_List_Travel_Plan_LocalTransport", param4).ToList();

                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Travel_Plans");
            }

        }
        
        public ActionResult TravelBknConfirm(string TravelPlanID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                var TravelCalenderID1 = DapperORM.DynamicQuerySingle("SELECT TravelCalenderID FROM Travel_Plan_Travel Where Deactivate=0  and TravelPlanID_Encrypted='" + TravelPlanID_Encrypted + "'");
                var TravelCalenderID = TravelCalenderID1.TravelCalenderID;
                
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_TravelPlanEmployeeId", Session["EmployeeId"]);
                param1.Add("@p_TravelPlanID_Encrypted", "List");
                param1.Add("@p_TravelCalenderID", TravelCalenderID);
                param1.Add("@P_Qry", " ");

                ViewBag.TravelPlanDetails = DapperORM.ReturnList<Travel_Plan_Travel>("sp_List_Travel_Plan_Travel", param1).FirstOrDefault();
                var encyCalId = ViewBag.TravelPlanDetails.TravelCalenderID_Encrypted;
                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_Employeeid", Session["EmployeeId"]);
                param2.Add("@p_TravelCalenderID_Encrypted", encyCalId);
                ViewBag.Calender = DapperORM.ReturnList<Travel_Calender>("sp_List_Travel_Calender", param2).FirstOrDefault();

                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Travel_Plans");
            }

        }

        public ActionResult AccomodationBknConfirm(string AccomodationPlanID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                var TravelCalenderID1 = DapperORM.DynamicQuerySingle("SELECT TravelCalenderID FROM Travel_Plan_Accomodation Where Deactivate=0  and AccomodationPlanID_Encrypted='" + AccomodationPlanID_Encrypted + "'");
                var TravelCalenderID = TravelCalenderID1.TravelCalenderID;


                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_AccomodationPlanEmployeeId", Session["EmployeeId"]);
                param1.Add("@p_AccomodationPlanID_Encrypted", "List");
                param1.Add("@p_TravelCalenderID", TravelCalenderID);
                param1.Add("@P_Qry", " ");
                ViewBag.TravelPlanDetails = DapperORM.ReturnList<Travel_Plan_Accomodation>("sp_List_Travel_Plan_Accomodation", param1).FirstOrDefault();
                var encyCalId = ViewBag.TravelPlanDetails.TravelCalenderID_Encrypted;
                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_Employeeid", Session["EmployeeId"]);
                param2.Add("@p_TravelCalenderID_Encrypted", encyCalId);
                ViewBag.Calender = DapperORM.ReturnList<Travel_Calender>("sp_List_Travel_Calender", param2).FirstOrDefault();

                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Travel_Plans");
            }

        }

        public ActionResult LocTransportBknConfirm(string LocalTranPlanID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                var TravelCalenderID1 = DapperORM.DynamicQuerySingle("SELECT TravelCalenderID FROM Travel_Plan_LocalTransport Where Deactivate=0  and LocalTranPlanID_Encrypted='" + LocalTranPlanID_Encrypted + "'");
                var TravelCalenderID = TravelCalenderID1.TravelCalenderID;


                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_LTPlanEmployeeId", Session["EmployeeId"]);
                param1.Add("@p_LocalTranPlanID_Encrypted", "List");
                param1.Add("@p_TravelCalenderID", TravelCalenderID);
                param1.Add("@P_Qry", " ");
                ViewBag.TravelPlanDetails = DapperORM.ReturnList<Travel_Plan_LocalTransport>("sp_List_Travel_Plan_LocalTransport", param1).FirstOrDefault();
                var encyCalId = ViewBag.TravelPlanDetails.TravelCalenderID_Encrypted;
                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_Employeeid", Session["EmployeeId"]);
                param2.Add("@p_TravelCalenderID_Encrypted", encyCalId);
                ViewBag.Calender = DapperORM.ReturnList<Travel_Calender>("sp_List_Travel_Calender", param2).FirstOrDefault();

                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Travel_Plans");
            }

        }

        [HttpPost]
        public ActionResult Save(Travel_Plan_Travel Travel, HttpPostedFileBase[] UploadDocs)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            try
            {
                DynamicParameters param = new DynamicParameters();

                if (Travel.TravelPlanID_Encrypted == " " || Travel.TravelPlanID_Encrypted == null)
                {
                    param.Add("@p_process", "Save");
                    param.Add("@P_DocDate", DateTime.Now);
                }
                else
                {
                    param.Add("@p_process", "Update");
                    param.Add("@P_CmpId", Travel.CmpId);

                }
                param.Add("@p_DocNo", Travel.DocNo);
                param.Add("@p_TravelPlanID_Encrypted", Travel.TravelPlanID_Encrypted);
                param.Add("@p_TravelPlanEmployeeId", Session["EmployeeId"]);
                param.Add("@p_TravelCalenderID", Travel.TravelCalenderID);
                param.Add("@p_PlanType", Travel.PlanType);
                param.Add("@p_Mode", Travel.Mode);
                param.Add("@p_BookingStatus", "Booked");
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

                if (UploadDocs != null)
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
                            DynamicParameters param1 = new DynamicParameters();

                            string URLFileFullPath = "";
                            URLFileFullPath = FirstPath + UploadDoc.FileName; //Concat Full Path and create New full Path
                            UploadDoc.SaveAs(URLFileFullPath); // This is use for Save image in folder full path

                            param1.Add("@p_process", "Save");
                            param1.Add("@p_TravelDocsID", 0);
                            param1.Add("@p_TravelDocsID_Encrypted", "");
                            param1.Add("@p_TravelPlanEmployeeId", Session["EmployeeId"]);
                            param1.Add("@p_TravelCalenderID", Travel.TravelCalenderID);
                            param1.Add("@p_TravelPlansID", GetTravelPlanID);
                            param1.Add("@p_PlanType", Travel.PlanType);
                            param1.Add("@p_Mode", Travel.Mode);
                            param1.Add("@p_DocumentName", UploadDoc.FileName);
                            param1.Add("@p_FileLocation", "/Travel/" + UploadDoc.FileName);
                            param1.Add("@p_Note", Travel.Note);
                            param1.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                            param1.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                            param1.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                            var data1 = DapperORM.ExecuteReturn("sp_SUD_Travel_Documents", param);
                            //TempData["Message"] = param.Get<string>("@p_msg");
                            //TempData["Icon"] = param.Get<string>("@p_Icon");

                        }
                    }
                }
                return RedirectToAction("GetList", "ESS_Travel_TravelDeskBooking", new { TravelPlanID_Encrypted = Travel.TravelPlanID_Encrypted });
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "TravelApproval");
            }
        }

        [HttpPost]
        public ActionResult SaveAccomodation(Travel_Plan_Accomodation Travel , HttpPostedFileBase[] UploadDocs)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            try
            {
                DynamicParameters param = new DynamicParameters();

                if (Travel.AccomodationPlanID_Encrypted == " " || Travel.AccomodationPlanID_Encrypted == null)
                {
                    param.Add("@p_process", "Save");
                }
                else
                {
                    param.Add("@p_process", "Update");
                }
                param.Add("@p_AccomodationPlanID_Encrypted", Travel.AccomodationPlanID_Encrypted);
                param.Add("@p_AccomodationPlanEmployeeId", Session["EmployeeId"]);
                param.Add("@p_TravelCalenderID", Travel.TravelCalenderID);
                param.Add("@p_BookingStatus", "Booked");
                param.Add("@p_BKHotelName", Travel.BKHotelName);
                param.Add("@p_BKHotelAddress", Travel.BKHotelAddress);
                param.Add("@p_BKHotelMapLocation", Travel.BKHotelMapLocation);
                param.Add("@p_BKOccupancyType", Travel.BKOccupancyType);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_AccomodationPlanID1", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter

                var data = DapperORM.ExecuteReturn("sp_SUD_Travel_Plan_Accomodation", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                var GetTravelPlanID = param.Get<string>("@p_AccomodationPlanID1");

                if (UploadDocs != null)
                {
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='TravelDesk'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    var FirstPath = GetFirstPath + "Accomodation" + "\\"; // First path plus concat with new folder
                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }
                    foreach (HttpPostedFileBase UploadDoc in UploadDocs)
                    {


                        if (UploadDoc.ContentLength > 0)
                        {
                            DynamicParameters param1 = new DynamicParameters();

                            string URLFileFullPath = "";
                            URLFileFullPath = FirstPath + UploadDoc.FileName; //Concat Full Path and create New full Path
                            UploadDoc.SaveAs(URLFileFullPath); // This is use for Save image in folder full path

                            param1.Add("@p_process", "Save");
                            param1.Add("@p_TravelDocsID", 0);
                            param1.Add("@p_TravelDocsID_Encrypted", "");
                            param1.Add("@p_TravelPlanEmployeeId", Session["EmployeeId"]);
                            param1.Add("@p_TravelCalenderID", Travel.TravelCalenderID);
                            param1.Add("@p_TravelPlansID", GetTravelPlanID);
                            param1.Add("@p_PlanType", Travel.PlanType);
                            param1.Add("@p_Mode", Travel.Mode);
                            param1.Add("@p_DocumentName", UploadDoc.FileName);
                            param1.Add("@p_FileLocation", "/Accomodation/" + UploadDoc.FileName);
                            param1.Add("@p_Note", Travel.Note);
                            param1.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                            param1.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                            param1.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                            var data1 = DapperORM.ExecuteReturn("sp_SUD_Travel_Documents", param);
                            //TempData["Message"] = param.Get<string>("@p_msg");
                            //TempData["Icon"] = param.Get<string>("@p_Icon");

                        }
                    }
                }

                return RedirectToAction("GetList", "ESS_Travel_TravelDeskBooking", new { });
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "AccomodationBknConfirm");
            }
        }

        [HttpPost]
        public ActionResult SaveLocalTransport(Travel_Plan_LocalTransport Travel, HttpPostedFileBase[] UploadDocs)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            try
            {
                DynamicParameters param = new DynamicParameters();

                if (Travel.LocalTranPlanID_Encrypted == " " || Travel.LocalTranPlanID_Encrypted == null)
                {
                    param.Add("@p_process", "Save");
                }
                else
                {
                    param.Add("@p_process", "Update");
                }
                param.Add("@p_LocalTranPlanID_Encrypted", Travel.LocalTranPlanID_Encrypted);
                param.Add("@p_LTPlanEmployeeId", Session["EmployeeId"]);
                param.Add("@p_TravelCalenderID", Travel.TravelCalenderID);
                param.Add("@p_BookingStatus", "Booked");
                param.Add("@p_BKCabType", Travel.BKCabType);
                param.Add("@p_BKCabContactPerson", Travel.BKCabContactPerson);
                param.Add("@p_BKCabBookingInstruction", Travel.BKCabBookingInstruction);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_LocalTranPlanID1", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter

                var data = DapperORM.ExecuteReturn("sp_SUD_Travel_Plan_LocalTransport", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                var GetTravelPlanID = param.Get<string>("@p_LocalTranPlanID1");


                if (UploadDocs != null)
                {
                    //D:\Documents\VendorConsultant\Accomodation\
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='TravelDesk'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    var FirstPath = GetFirstPath + "LocalTransport" + "\\"; // First path plus concat with new folder
                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }
                    foreach (HttpPostedFileBase UploadDoc in UploadDocs)
                    {
                        if (UploadDoc.ContentLength > 0)
                        {
                            DynamicParameters param11 = new DynamicParameters();

                            string URLFileFullPath = "";
                            URLFileFullPath = FirstPath + UploadDoc.FileName; //Concat Full Path and create New full Path
                            UploadDoc.SaveAs(URLFileFullPath); // This is use for Save image in folder full path

                            param11.Add("@p_process", "Save");
                            param11.Add("@p_TravelDocsID", 0);
                            param11.Add("@p_TravelDocsID_Encrypted", "");
                            param11.Add("@p_TravelPlanEmployeeId", Session["EmployeeId"]);
                            param11.Add("@p_TravelCalenderID", Travel.TravelCalenderID);
                            param11.Add("@p_TravelPlansID", GetTravelPlanID);
                            param11.Add("@p_PlanType", Travel.PlanType);
                            param11.Add("@p_Mode", Travel.Mode);
                            param11.Add("@p_DocumentName", UploadDoc.FileName);
                            param11.Add("@p_FileLocation", "/LocalTransport/" + UploadDoc.FileName);
                            param11.Add("@p_Note", Travel.Note);
                            param11.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                            param11.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                            param11.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                            var data1 = DapperORM.ExecuteReturn("sp_SUD_Travel_Documents", param);
                            //TempData["Message"] = param.Get<string>("@p_msg");
                            //TempData["Icon"] = param.Get<string>("@p_Icon");

                        }
                    }
                }

                return RedirectToAction("GetList", "ESS_Travel_TravelDeskBooking", new { });
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "LocTransportBknConfirm");
            }
        }

    }
}