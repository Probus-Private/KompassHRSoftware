using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Conference;
using KompassHR.Areas.Setting.Models.Setting_Conference;
using KompassHR.Areas.Setting.Models.Setting_Leave;
using KompassHR.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Conference
{
    public class ESS_Conference_ConferenceBookingController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_Conference_ConferenceBooking
        #region Conference Main View Page 
        public ActionResult ESS_Conference_ConferenceBooking(string ConferenceBookingID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";
                Conference_Booking ObjConference_Booking = new Conference_Booking();

                var ConferenceBookingID = DapperORM.DynamicQuerySingle("Select Isnull(Max(ConferenceBookingID),0)+1 As ConferenceBookingID from Conference_Booking");
                TempData["ConferenceBookingID"] = ConferenceBookingID.ConferenceBookingID;

                param.Add("@query", "Select ConferenceListID As Id ,ConferenceName As [Name] from Conference_List where deactivate=0");
                var ConferenceName = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.ConferenceNameList = ConferenceName;

                param = new DynamicParameters();
                param.Add("@query", "Select  BranchId, BranchName from Mas_Branch where Deactivate=0 and CmpId=(Select CmpId from Mas_Employee where Deactivate = 0 and employeeId='" + Session["EmployeeId"] + "') order by BranchName");
                var GetLocation = DapperORM.ReturnList<Mas_Branch>("sp_QueryExcution", param).ToList();
                ViewBag.Location = GetLocation;

                if (ConferenceBookingID_Encrypted != null)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    param1.Add("@p_ConferenceBookingID_Encrypted", ConferenceBookingID_Encrypted);
                    ObjConference_Booking = DapperORM.ReturnList<Conference_Booking>("sp_List_Conference_Booking", param1).FirstOrDefault();
                }
                TempData["BookingDate"] = ObjConference_Booking.BookingDate;               
            
                return View(ObjConference_Booking);
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Conference_ConferenceBooking ");
            }
        }
        #endregion

        public ActionResult GetListOfConferenceBooking()
        {
            try
            {
                DynamicParameters ConfBook = new DynamicParameters();
                ConfBook.Add("@p_ConferenceBookingID_Encrypted", "List");
                ConfBook.Add("@P_Qry", "And convert(Date, BookingDate) >=CONVERT(Date, getdate()) And BookingEmployeeID=" + Session["EmployeeId"] + "");           
                ViewBag.ConferenceBooking = DapperORM.ExecuteSP<dynamic>("sp_List_Conference_Booking", ConfBook).ToList();
                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Conference_ConferenceBooking ");
            }
        }



        #region SaveUpdate Conference

        [HttpPost]
        public ActionResult SaveUpdate(Conference_Booking ObjConference)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(ObjConference.ConferenceBookingID_Encrypted) ? "Save" : "Update");
                param.Add("@p_ConferenceBookingID_Encrypted", ObjConference.ConferenceBookingID_Encrypted);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_BookingEmployeeID", Session["EmployeeId"]);
                param.Add("@p_BookingDate", ObjConference.BookingDate);
                param.Add("@p_ConferenceID", ObjConference.ConferenceID);
                param.Add("@p_BranchID", ObjConference.BranchID);
                param.Add("@p_Subject", ObjConference.Subject);
                param.Add("@p_Description", ObjConference.Description);
                param.Add("@p_FromTime", ObjConference.FromTime);
                param.Add("@p_ToTime", ObjConference.ToTime);
                param.Add("@p_TotalDuration", ObjConference.TotalDuration);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Conference_Booking", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                return RedirectToAction("ESS_Conference_ConferenceBooking", "ESS_Conference_ConferenceBooking", new { Area = "ESS" });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message.ToString();
            }
            return RedirectToAction("ESS_Conference_ConferenceBooking", "ESS_Conference_ConferenceBooking", new { Area = "ESS" });
        }
        #endregion


        //#region Main View Page ConferenceBookingList
        //public ActionResult GetList()
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { Area = "" });
        //        }              
        //        param.Add("@p_ConferenceBookingID_Encrypted", "List");
        //        var data = DapperORM.ExecuteSP<dynamic>("sp_List_Get_Conference_Booking", param).ToList();
        //        if (data != null)
        //        {
        //            ViewBag.GetConference_Booking = data;
        //        }
        //        else
        //        {
        //            ViewBag.GetConference_Booking = "";
        //        }
        //        return View();
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }

        //}
        //#endregion

        [HttpGet]
        public ActionResult GetTotalDuration(string FromTime, string ToTime)
        {
            try
            {
                TimeSpan t1 = TimeSpan.Parse(FromTime);
                TimeSpan t2 = TimeSpan.Parse(ToTime);
                double _24h = (new TimeSpan(24, 0, 0)).TotalMilliseconds;
                double diff = t2.TotalMilliseconds - t1.TotalMilliseconds;
                if (diff < 0) diff += _24h;
                var TotalDuration = TimeSpan.FromMilliseconds(diff);
                return Json(new { data = TotalDuration }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception Ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet); ;
            }
        }

        #region GetConferenceDetails
        public ActionResult GetConferenceDetails(string ConferenceId, DateTime BookingDate, int BranchID)
        {
            try
            {
                //var ConferenceDescription = DapperORM.DynamicQuerySingle("Select ConferenceDescription from Conference_List Where ConferenceListID =" + ConferenceId + "");
                //var data = ConferenceDescription.ConferenceDescription;

                param.Add("@p_ConferenceListID", ConferenceId);
                param.Add("@p_BookingDate", BookingDate);
                param.Add("@P_BranchID", BranchID);
                var ConferenceAvilable = DapperORM.ExecuteSP<dynamic>("sp_List_ConferenceAvilable", param).ToList();
                return Json( ConferenceAvilable  , JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Conference_ConferenceBooking ");
            }

        }
        #endregion

        public ActionResult ConferenceName(int SelectBuniessUnit)
        {
            try
            {              
                param.Add("@query", "select ConferenceName,ConferenceListID from Conference_List where BranchId='" + SelectBuniessUnit + "' ");
                var GetConferenceName = DapperORM.ReturnList<Conference_List>("sp_QueryExcution", param).ToList();
                return Json(GetConferenceName, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex )
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_Conference_ConferenceBooking ");
            }
        }

        #region Delete TravelClaim
        public ActionResult ConferenceBookingDelete(string ConferenceBookingID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_ConferenceBookingID_Encrypted", ConferenceBookingID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Conference_Booking", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_Conference_ConferenceBooking", new { Area = "ESS" });
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Conference_ConferenceBooking ");
            }

        }
        #endregion

    }
}