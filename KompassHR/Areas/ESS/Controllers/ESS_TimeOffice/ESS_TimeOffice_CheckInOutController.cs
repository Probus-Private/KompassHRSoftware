using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace KompassHR.Areas.ESS.Controllers.ESS_TimeOffice
{
    public class ESS_TimeOffice_CheckInOutController : Controller
    {
        // GET: ESS/ESS_TimeOffice_CheckInOut
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        private string apiKey = "AIzaSyAYsghP-xQqNChpzr-EkxjWHOZXnX-prmI"; 


        [HttpGet]
        [Route("api/location/getaddress")]
        public async Task<ActionResult> GetAddress(string lat, string lng)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={lat},{lng}&key={apiKey}";
                    var response = await client.GetAsync(url).ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                        return Json(new { error = "Error calling Google Maps API" }, JsonRequestBehavior.AllowGet);

                    string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    JObject json = JObject.Parse(content);

                    if (json["status"].ToString() != "OK")
                        return Json(new { error = "Address not found" }, JsonRequestBehavior.AllowGet);

                    string address = json["results"][0]["formatted_address"].ToString();
                    return Json(address, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMesage"] = "";
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }
        #region ESS_TimeOffice_CheckInOut
        public ActionResult ESS_TimeOffice_CheckInOut(CheckInOut OBJCheckInOut)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 267;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters CalenderEmp = new DynamicParameters();
                CalenderEmp.Add("@p_EmployeeId", Convert.ToInt32(Session["EmployeeId"]));
                ViewBag.GetEmployeeList = DapperORM.ReturnList<AllDropDownBind>("sp_GetAttendanceCalender_Employee", CalenderEmp).ToList();


                var EmpId = Convert.ToInt32(Session["EmployeeId"]);
                param.Add("@query", "Select Mas_LocationRegistration.LocationRegistrationId As Id, Mas_LocationRegistration.LocationName as Name from Mas_LocationRegistration , Mas_LocationRegistrationMapping_Master, Mas_LocationRegistrationMapping_Detail,Mas_Employee_Attendance,Mas_Employee where Mas_LocationRegistration.Deactivate = 0 and Mas_LocationRegistrationMapping_Master.LocationRegistrationMappingIMasterId = Mas_LocationRegistrationMapping_Detail.LocationRegistrationMappingIMasterId and Mas_LocationRegistrationMapping_Detail.LocationRegistrationId = Mas_LocationRegistration.LocationRegistrationId and Mas_Employee_Attendance.EM_Atten_LocationRegistrationMappingIMasterId = Mas_LocationRegistrationMapping_Master.LocationRegistrationMappingIMasterId and Mas_Employee.EmployeeId = Mas_Employee_Attendance.AttendanceEmployeeId and Mas_LocationRegistrationMapping_Master.Deactivate = 0 and Mas_Employee_Attendance.Deactivate = 0 and Mas_Employee.Deactivate = 0 and Mas_Employee.EmployeeId = '" + EmpId + "'");
                var GetLocationName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.LocationName = GetLocationName;
                if (OBJCheckInOut.Date != null)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    //paramList.Add("@p_Employeeid", Session["EmployeeId"]);
                    paramList.Add("@p_Employeeid", OBJCheckInOut.EmployeeId);
                    paramList.Add("@p_Month", OBJCheckInOut.Date);
                    var data = DapperORM.ExecuteSP<dynamic>("sp_List_AttenCheckInOut", paramList).ToList();
                    ViewBag.CheckInOutList = data;
                }
                else
                {
                    ViewBag.CheckInOutList = "";
                }
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region SUD CheckIn CheckOut

        public ActionResult SaveCheckInOut(string Latitude, string Longitude, string Direction, string CheckInOutLocationId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 267;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var paramIsAddress = new DynamicParameters();
                paramIsAddress.Add("@query", @"SELECT TOP 1 LocationApiAddressApplicable FROM Tool_CommonTable");
                var Result = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramIsAddress).FirstOrDefault();
                var LocationApiAddressApplicable = Result?.LocationApiAddressApplicable ?? 0;
                string CheckInOutAddress = "";
                if (LocationApiAddressApplicable==true)
                {
                    var resultAdreess = GetAddress(Latitude, Longitude).GetAwaiter().GetResult() as JsonResult;
                    CheckInOutAddress = resultAdreess.Data?.ToString();
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Save");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_CmpId", Session["CompanyId"]);
                param.Add("@p_BranchId", Session["BranchId"]);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CheckInOutLatitude", Latitude);
                param.Add("@p_CheckInOutLongitude", Longitude);
                param.Add("@p_CheckInOutDirection", Direction);
                param.Add("@p_CheckInOutLocationId", CheckInOutLocationId);
                param.Add("@p_CheckInOutAddress", CheckInOutAddress);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteSP<dynamic>("sp_SUD_Atten_CheckInOut", param).ToList();
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_TimeOffice_CheckInOut", "ESS_TimeOffice_CheckInOut", new { area = "ESS" });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetClientLatitudeAndLangitude
        public ActionResult GetClientLatitudeAndLangitude(int GetLocationName)
        {
            try
            {
                var GetLocationCredentials = DapperORM.DynamicQuerySingle("Select  LocationLatitude,LocationLongitude,LocationAreaRange from Mas_LocationRegistration where LocationRegistrationId=" + GetLocationName + "");
                return Json(GetLocationCredentials, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        public ActionResult ViewSelfie(string filePath, string Id)
        {
            if (string.IsNullOrWhiteSpace(filePath) || filePath.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
            {
                return HttpNotFound("Invalid file name.");
            }
            //var EmpId = Convert.ToInt32(Session["EmployeeId"]);
            //var GetEmpNo = DapperORM.DynamicQuerySingle("select Employeeno from mas_employee where employeeid='"+ EmpId + "'");
            //var GetNo = GetEmpNo.Employeeno;
            string directoryPath = "";
            var GetPath = DapperORM.DynamicQuerySingle("select * from Tool_Documnet_DirectoryPath where DocOrigin='Checkinout'");
            directoryPath = GetPath.DocInitialPath;
            string fullPath = Path.Combine(directoryPath, Id, filePath);

            if (!System.IO.File.Exists(fullPath))
            {
                return HttpNotFound("File not found.");
            }

            string contentType = MimeMapping.GetMimeMapping(fullPath);
            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(fileBytes, contentType);
        }

    }
}

