using Dapper;
using Google.Authenticator;
using KompassHR.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Security;

namespace KompassHR.Controllers
{
    public class LoginController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        SqlConnection sqlcons = new SqlConnection(DapperORM.connectionStrings);
        DynamicParameters param = new DynamicParameters();
        System.Data.DataSet dataset = new System.Data.DataSet();
        DataTable dtEmpinfo = new DataTable();


        // Make sure the method is async
       

        // GET: Login
        #region Login Page
        [HttpGet]
        public ActionResult Login()
        {
            try
            {
                Login objLogin = new Login();
                string savedCustomerCode = string.Empty;
                if (Request.Cookies["CustomerCode"] != null)
                {
                    savedCustomerCode = Request.Cookies["CustomerCode"].Value;
                }
                objLogin.CustomerCode = savedCustomerCode;
                //Login model = new Login();
                //model.CustomerCode = CheckLoginCookie();
                //model.ESSLoginID = CheckESSLoginIDFromCookie();
                //model.RememberMe = !string.IsNullOrEmpty(model.CustomerCode);
                //foreach (string cookie in Request.Cookies.AllKeys)
                //{
                //    Response.Cookies[cookie].Expires = DateTime.Now.AddDays(-1); // if AddDays(+1) then after 1 hour Cookies will clear
                //}

                
                return View(objLogin);
            }
            catch (Exception ex)
            {

                if (ex.InnerException.Message == "The network path was not found" || ex.InnerException.Message == "The semaphore timeout period has expired")
                {
                    TempData["ErrorMessage"] = "Please check your network connection";
                    return RedirectToAction("ErrorPage", "Login", new { area = "" });
                }
                TempData["ErrorMesage"] = "";
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }

        }
        #endregion

        //private string CheckLoginCookie()
        //{
        //    if (Request.Cookies.Get("ESSCustomerCode") != null && Request.Cookies.Get("ESSLoginID") != null)
        //    {
        //        return Request.Cookies["ESSCustomerCode"].Value;
        //    }
        //    else
        //    {
        //        return string.Empty;
        //    }
        //}

        //private string CheckESSLoginIDFromCookie()
        //{
        //    if (Request.Cookies.Get("ESSLoginID") != null)
        //    {
        //        return Request.Cookies["ESSLoginID"].Value;

        //    }
        //    else
        //    {
        //        return string.Empty;
        //    }

        //}

        // GET: Onboarding

        #region Login1 Implementation
        [HttpGet]
        public ActionResult Login1(string ESSLoginID, string ESSPassword, string ESSCustomerCode, string IsMobileOrWeb, string lat, string lon, string address)
        {
            try
            {
                //foreach (string cookie in Request.Cookies.AllKeys)
                //{
                //    Response.Cookies[cookie].Expires = DateTime.Now.AddDays(-1);
                //}
                //BundleTable.EnableOptimizations = true;

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

                //if (lat == null || lat == "" && lon == null || lon == "")
                //{
                //    var data = "Please enable device location to login.";
                //    return Json(data, JsonRequestBehavior.AllowGet);
                //}


                //if (CheckBoxChecked == true)
                //{
                //    HttpCookie OBJCustomerCode = new HttpCookie("ESSCustomerCode");
                //    OBJCustomerCode.Value = ESSCustomerCode;
                //    OBJCustomerCode.Expires = DateTime.Now.AddDays(90);
                //   // OBJCustomerCode.Expires.AddDays(90);
                //    Response.Cookies.Add(OBJCustomerCode);

                //    HttpCookie OBJESSLoginID = new HttpCookie("ESSLoginID");
                //    OBJESSLoginID.Value = ESSLoginID;
                //    OBJESSLoginID.Expires = DateTime.Now.AddDays(90);
                //   // OBJESSLoginID.Expires.AddDays(90);
                //    Response.Cookies.Add(OBJESSLoginID);
                //}

                Session["IsMobileOrWeb"] = IsMobileOrWeb;
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_ESSLoginID", ESSLoginID);
                param.Add("@p_ESSPassword", ESSPassword);
                var result = DapperORM.ExecuteSP<dynamic>("sp_ESSLogin", param).FirstOrDefault();
                if (result != null)
                {
                    var EmpId = result.EmployeeId;
                    Session["IsAdmin"] = result.IsAdmin;
                    Session["EmployeeId"] = EmpId;
                    Session["UserAccessPolicyId"] = result.UserAccessPolicyId;
                    Session["EmpDepartment"] = result.EmployeeDepartmentID;
                    var ESSLock = result.ESSIsLock;
                    var ESSLoginAttemptCount = result.ESSLoginAttemptCount;



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
                            paramLog.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter


                            //   var resultAdreess = GetAddress(lat, lon).GetAwaiter().GetResult() as JsonResult;
                            var Result = DapperORM.ExecuteReturn("sp_SUD_Tool_UserLogin", paramLog);
                            // var Adreess = resultAdreess.Data;
                            var P_Id = paramLog.Get<string>("@p_Id");
                            var latLong = lat + "," + lon;
                            DynamicParameters paramid = new DynamicParameters();
                            paramid.Add("@query", "UPDATE Tool_UserLogin SET LatLong = '" + latLong + "', Address = '" + address + "' WHERE Id = '" + P_Id + "' ");
                            var currentlatlog = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramid).FirstOrDefault();

                            DynamicParameters Dash = new DynamicParameters();
                            Dash.Add("@p_EmployeeId", EmpId);
                            var Alldata = DapperORM.ExecuteSP<dynamic>("sp_ESSDashboard", Dash).ToList();
                            GetDashboardData();//Get Dashboard data

                            //===========  START MOBILE NOTIFICATION CODE BELOW ================================
                            string customerCode = ESSCustomerCode;
                            //string customerCode = "P_K1005";
                            int empId = Convert.ToInt32(Session["EmployeeId"]);
                            string empName = Convert.ToString(Session["EmployeeName"]);
                            // Fetch data
                            var Sendmessage = DapperORM.DynamicQueryList("SELECT * FROM tbl_NotificationMessage WHERE Origin='Login'").FirstOrDefault();
                            var GetBaseURL = DapperORM.DynamicQueryList("Select baseUrl from App_ImageBaseURL where id=2").FirstOrDefault();
                            Session["GetBaseURLForNotification"] = GetBaseURL?.baseUrl;
                            // Build request model
                            if (Sendmessage != null)
                            {
                                var request = new
                                {
                                    Title = $"🔔 {Sendmessage.MessageTitle}",
                                    Body = $"👤 {Sendmessage.MessageBody}",
                                    NotifyEmpId = Convert.ToInt32(empId),
                                    CreatedBy = empName,
                                    RequestType = Sendmessage.Origin
                                };
                                // Send notification
                                string response = new NotificationService().SendNotification(customerCode, request);
                            }
                            //===========  END MOBILE NOTIFICATION CODE BELOW ================================

                            Session["ESSCustomerCode"] = ESSCustomerCode;
                            DynamicParameters paramCount = new DynamicParameters();
                            paramCount.Add("@query", "Select Count(CompanyId) As Counts from  Mas_CompanyProfile where Deactivate=0 and Isactive=1");
                            var count = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramCount).FirstOrDefault();
                            int Getcount = count.Counts;
                            if (Getcount != 0)
                            {
                                sqlcons.Query("Update CustomerRegistration Set NoOfActiveEntities=" + Getcount + " Where Con_CustomerCode='" + ESSCustomerCode + "'").FirstOrDefault();
                            }
                            if (count.Counts != null)
                            {
                                //Authentication Code
                                DynamicParameters paramath = new DynamicParameters();
                                paramath.Add("@query", "select MFAEnabled, IsMFA, MFAToken from Mas_Employee_ESS where ESSEmployeeId = " + EmpId + " and Deactivate=0");
                                var IsAuthenticated = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramath).FirstOrDefault();

                                if (IsAuthenticated != null && IsAuthenticated.MFAEnabled == true)
                                {
                                    return Json(new
                                    {
                                        status = "CheckAuthentication",
                                        mfaData = new
                                        {
                                            IsMFA = true,
                                            MFAEnabled = true,
                                            MFAToken = IsAuthenticated.MFAToken
                                        }
                                    }, JsonRequestBehavior.AllowGet);
                                }
                                //return RedirectToAction("Dashboard", "Dashboard");
                                //return Json(true, JsonRequestBehavior.AllowGet);
                                if (IsMobileOrWeb == "Web")
                                {
                                    DynamicParameters paramess1 = new DynamicParameters();
                                    paramess1.Add("@query", "UPDATE Mas_Employee_ESS SET ESSLoginAttemptCount = 0 WHERE ESSLoginID = '" + ESSLoginID + "' or ESSPassword='" + ESSPassword + "' AND Deactivate = 0");
                                    var currentCountResult1 = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramess1).FirstOrDefault();
                                    return Json(new { type = "Web", customerCode = ESSCustomerCode }, JsonRequestBehavior.AllowGet);
                                }
                                else if (IsMobileOrWeb == "Mobile")
                                {
                                    return Json("Mobile", JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    return RedirectToAction("Dashboard", "Dashboard");
                                }
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
                    DynamicParameters paramater = new DynamicParameters();
                    paramater.Add("@query", "select ESSLoginAttemptCount from Mas_Employee_ESS where ESSLoginID ='" + ESSLoginID + "'or ESSPassword='" + ESSPassword + "'  ");
                    var ESSLoginAttemptCount = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramater).FirstOrDefault();

                    int attempts = ESSLoginAttemptCount.ESSLoginAttemptCount;
                    int attemptCount = ESSLoginAttemptCount.ESSLoginAttemptCount + 1;
                    if (attempts >= 5)
                    {
                        DynamicParameters paramess1 = new DynamicParameters();
                        paramess1.Add("@query", "UPDATE Mas_Employee_ESS SET ESSIsLock = 1 WHERE ESSLoginID = '" + ESSLoginID + "' or ESSPassword='" + ESSPassword + "' AND Deactivate = 0");
                        var currentCountResult1 = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramess1).FirstOrDefault();

                        var data = "Account Locked after 5 failed attempts";
                        return Json(data, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        DynamicParameters paramess1 = new DynamicParameters();
                        paramess1.Add("@query", "UPDATE Mas_Employee_ESS SET ESSLoginAttemptCount = " + attemptCount + " WHERE ESSLoginID = '" + ESSLoginID + "' or ESSPassword='" + ESSPassword + "' AND Deactivate = 0");
                        var currentCountResult1 = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramess1).FirstOrDefault();

                        var data = "Invalid loginid and password";
                        return Json(data, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                var ErrorMsg = "";
                return Json(new { data = ex.Message, ErrorMsg = "DBError" }, JsonRequestBehavior.AllowGet);
                //Session["GetErrorMessage"] = ex.Message;
                //var ErrorMSG = ex.Message;
                //if (ErrorMSG.Contains("A network-related or instance-specific error occurred"))
                //{
                //    return RedirectToAction("ErrorPage", "Login");
                //}
            }
        }
        #endregion

        #region Dashboard Implementation
        public void GetDashboardData()
        {
            try
            {
                //string MachineName = System.Environment.MachineName;
                //string MachineName = User.Identity.Name;
                //string UserName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                //string IPAddress = HttpContext.Current.Request.UserHostAddress;
                //Session["MachineName"] = MachineName;
                //Session["UserName"] = UserName;

                //GET DASHBOARD REQUISITION DATA FROM sp_ESSDashboard THIS SP
                var EmpId = Session["EmployeeId"];
                System.Data.DataSet dataset = new System.Data.DataSet();
                dataset = new System.Data.DataSet();
                SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
                //Write Code For ESS Menu Rights

                DynamicParameters paramMenu = new DynamicParameters();
                paramMenu.Add("@p_AccessPolicyId", Session["UserAccessPolicyId"]);
                paramMenu.Add("@p_ScreenMenuType", "Transation");
                var MenuList = DapperORM.ExecuteSP<dynamic>("sp_Access_GetSideMenuList", paramMenu).ToList();
                Session["GetTransactionMenulist"] = MenuList;
                bool exists = MenuList.Any(m => m.ScreenId == 583); // This is use for on dashbord LMS show based on Access
                Session["IsTrainingAccess"] = exists;

                // SP_getReportingManager
                //var GerTransationRightsMenu = DapperORM.DynamicQuerySingle(@"Select Kompass_Modules.ModuleID,Kompass_Modules.ModuleID_Encrypted,Kompass_Modules.ModuleName,Kompass_Modules.ModuleIcon,tool_screenmaster.ScreenUrl
                //                                    from Tool_UserRights,Kompass_Modules,tool_screenmaster where UserRightsModuleId=Kompass_Modules.ModuleId and tool_screenmaster.ScreenId=Tool_UserRights.UserRightsScreenId and Tool_UserRights.Deactivate=0
                //                                    and Tool_ScreenMaster.IsActive=1 and Kompass_Modules.IsApplicable=1 and Tool_UserRights.UserRightsEmployeeId=" + Session["EmployeeId"] + " and Tool_ScreenMaster.ScreenSequence=0 and Tool_ScreenMaster.ScreenMenuType in ('Transation','Report') and Kompass_Modules.TransationSequenceNo is not null and(Tool_UserRights.IsMenu = 1 or Tool_UserRights.IsSave = 1 or    Tool_UserRights.IsUpdate = 1 or Tool_UserRights.IsDelete = 1 or Tool_UserRights.IsList = 1) order by Kompass_Modules.TransationSequenceNo asc").ToList();
                //Session["GetTransactionMenulist"] = GerTransationRightsMenu;

                //var GerSettingMenu = DapperORM.DynamicQuerySingle(@"Select Kompass_Modules.ModuleID,Kompass_Modules.ModuleID_Encrypted,Kompass_Modules.ModuleName,Kompass_Modules.ModuleIcon,tool_screenmaster.ScreenUrl
                //                                    from Tool_UserRights,Kompass_Modules,tool_screenmaster where UserRightsModuleId=Kompass_Modules.ModuleId and tool_screenmaster.ScreenId=Tool_UserRights.UserRightsScreenId and Tool_UserRights.Deactivate=0
                //                                    and Tool_ScreenMaster.IsActive=1 and Kompass_Modules.IsApplicable=1 and Tool_UserRights.UserRightsEmployeeId=" + Session["EmployeeId"] + " and Tool_ScreenMaster.ScreenSequence=0 and Tool_ScreenMaster.ScreenMenuType='Setting' and Kompass_Modules.SettingSequenceNo is not null and(Tool_UserRights.IsMenu = 1 or Tool_UserRights.IsSave = 1 or    Tool_UserRights.IsUpdate = 1 or Tool_UserRights.IsDelete = 1 or Tool_UserRights.IsList = 1) order by Kompass_Modules.SettingSequenceNo asc").ToList();
                //Session["GetSettingMenulist"] = GerSettingMenu;


                //TODAY TEAM STATUS
                DynamicParameters paramTeamStatus = new DynamicParameters();
                paramTeamStatus.Add("@p_Managerid", Session["EmployeeId"]);
                var GetTeamStatus = DapperORM.ExecuteSP<dynamic>("SP_Dashboard_TodayTeamStatus", paramTeamStatus).ToList(); // SP_getReportingManager
                //Session["GetTodayTeamStatus"] = GetTeamStatus;

                Session["TodayTeamLeaveCount"] = GetTeamStatus[0].countt;
                Session["TodayTeamPresentCount"] = GetTeamStatus[1].countt;
                Session["TodayTeamWeeklyOffCount"] = GetTeamStatus[2].countt;
                Session["TodayTeamAbsentCount"] = GetTeamStatus[3].countt;
                Session["TodayTeamOutDoorCount"] = GetTeamStatus[4].countt;
                Session["TodayTeamWFHCount"] = GetTeamStatus[5].countt;
                Session["TodayTeamShortLeaveCount"] = GetTeamStatus[6].countt;



                DynamicParameters param = new DynamicParameters();
                //param.Add("@query", "Select Item from dbo.SplitString( (select dbo.FunGetDateDifferenceSaperately(joiningdate,getdate())from mas_employee where employeeid=" + EmpId + "),' ')");
                param.Add("@query", @"SELECT  CASE WHEN DATEDIFF(MONTH,A.Period_Start,A.Period_End) < 12
			                            THEN  '0'
			                            WHEN (DATEDIFF(MONTH,A.Period_Start,A.Period_End) % 12 = 0)
			                            THEN  CONVERT(VARCHAR(4),DATEDIFF(MONTH,A.Period_Start,A.Period_End) / 12 ) --+ ' Years '			  
                                        ELSE  CONVERT(VARCHAR(4),DATEDIFF(MONTH,A.Period_Start,A.Period_End) / 12 ) --+ ' Years '
			                            END AS [Years]
	                                    , CASE WHEN DATEDIFF(MONTH,A.Period_Start,A.Period_End) < 1
			                            THEN '0'
			                            WHEN DATEDIFF(MONTH,A.Period_Start,A.Period_End) < 12
			                            THEN CONVERT(VARCHAR(2),DATEDIFF(MONTH,A.Period_Start,A.Period_End)) --+ ' Months '
		                                WHEN DATEDIFF(MONTH,A.Period_Start,A.Period_End) % 12 > 0
			                            THEN CONVERT(VARCHAR(2),DATEDIFF(MONTH,A.Period_Start,A.Period_End) % 12) --+ ' Months '
			                            ELSE '0'
		                                END AS [Months]
	                                    ,CASE WHEN DATEDIFF(DAY,(DATEADD(MONTH,DATEDIFF(MONTH,A.Period_Start,A.Period_End),A.Period_Start)),A.Period_End) > 0
		                                THEN CONVERT(VARCHAR(2),DATEDIFF(DAY,(DATEADD(MONTH,DATEDIFF(MONTH,A.Period_Start,A.Period_End),A.Period_Start)),A.Period_End)) --+ ' Days'
		                                ELSE '0'
		                                END AS [Days]
		                                From
                                        (SELECT	Employeeid ,EmployeeName ,JoiningDate
	                                    ,CONVERT(VARCHAR(4),DATEPART(YY,JoiningDate)) + '-' + RIGHT('00'+CONVERT(VARCHAR(2),DATEPART(M,JoiningDate)),2) +'-'+ '01' Period_Start
	                                    ,DATEADD(D,- DATEDIFF(DAY,DATEPART(d,CONVERT(VARCHAR(4),DATEPART(YY,JoiningDate)) + '-' + RIGHT('00'+CONVERT(VARCHAR(2),DATEPART(M,JoiningDate)),2) +'-'+ '01'),DATEPART(d,JoiningDate)),getdate()) Period_End
                                        FROM mas_employee where EmployeeId=" + Session["EmployeeId"] + ") as A");
                var WorkAnniversaryDate = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).FirstOrDefault();
                //var GetWorkAnniversaryDate = WorkAnniversaryDate.Count();
                if (WorkAnniversaryDate != null)
                {
                    Session["WorkAnniversaryDateYears"] = WorkAnniversaryDate.Years;
                    Session["WorkAnniversaryDateMonths"] = WorkAnniversaryDate.Months;
                    Session["WorkAnniversaryDateDays"] = WorkAnniversaryDate.Days;
                }
                else
                {
                    Session["WorkAnniversaryDateYears"] = null;
                    Session["WorkAnniversaryDateMonths"] = null;
                    Session["WorkAnniversaryDateDays"] = null;
                }

                var CheckIsManager = DapperORM.DynamicQuerySingle(@"select count(EmployeeId) as EmployeeId  from mas_employee where employeeid in (select distinct ReportingManager1 from
                                    Mas_Employee_Reporting) and employeeid = " + Session["EmployeeId"] + "");
                if (CheckIsManager.Count() != 0)
                {
                    Session["CheckIsManager"] = CheckIsManager;
                }
                else
                {
                    Session["CheckIsManager"] = null;
                }



                //var GerTransationRightsMenu = DapperORM.DynamicQuerySingle(@"Select Distinct Tool_UserRights.UserRightsModuleId,Kompass_Modules.ModuleName,Kompass_Modules.ModuleIcon,tool_screenmaster.ScreenUrl
                //                                    from Tool_UserRights,Kompass_Modules,tool_screenmaster where UserRightsModuleId=Kompass_Modules.ModuleId and tool_screenmaster.ScreenId=Tool_UserRights.UserRightsScreenId and Tool_UserRights.Deactivate=0
                //                                    and Tool_ScreenMaster.IsActive=1 and Kompass_Modules.IsApplicable=1 and Tool_UserRights.UserRightsEmployeeId=" + Session["EmployeeId"] + " and Tool_ScreenMaster.ScreenSequence=0 and Tool_ScreenMaster.ScreenMenuType='Transation'").ToList();
                //Session["GetTransactionMenulist"] = GerTransationRightsMenu;

                //var GerSettingMenu = DapperORM.DynamicQuerySingle(@"Select Distinct Tool_UserRights.UserRightsModuleId,Kompass_Modules.ModuleName,Kompass_Modules.ModuleIcon,tool_screenmaster.ScreenUrl
                //                                    from Tool_UserRights,Kompass_Modules,tool_screenmaster where UserRightsModuleId=Kompass_Modules.ModuleId and tool_screenmaster.ScreenId=Tool_UserRights.UserRightsScreenId and Tool_UserRights.Deactivate=0
                //                                    and Tool_ScreenMaster.IsActive=1 and Kompass_Modules.IsApplicable=1 and Tool_UserRights.UserRightsEmployeeId=" + Session["EmployeeId"] + " and Tool_ScreenMaster.ScreenSequence=0 and Tool_ScreenMaster.ScreenMenuType='Setting'").ToList();
                //Session["GetSettingMenulist"] = GerSettingMenu;
                string connectionString = Session["MyNewConnectionString"]?.ToString();
                SqlConnection sqlconSession = new SqlConnection(connectionString);
                if (sqlconSession != null && sqlconSession.State == ConnectionState.Closed)
                {
                    sqlconSession.Open();
                }
                SqlCommand cmd = new SqlCommand("dbo.[sp_ESSDashboard]", sqlconSession);
                //SqlCommand cmd = new SqlCommand("dbo.[sp_ESSDashboard]", sqlcon);
                cmd.CommandTimeout = 0;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@p_EmployeeId", EmpId);
                SqlDataAdapter dtadp = new SqlDataAdapter(cmd);
                dtadp.Fill(dataset);
                DataTable dtEmpinfo = dataset.Tables[0];
                DataTable dtEmgCont = dataset.Tables[1];
                DataTable dtPubHoliday = dataset.Tables[2];
                DataTable dtIsInNat = dataset.Tables[3];

                DataTable dtNews = dataset.Tables[4];
                DataTable dtEvents = dataset.Tables[5];
                DataTable dtAnnouncement = dataset.Tables[6];
                DataTable dtReward = dataset.Tables[7];

                SqlDataReader dr = cmd.ExecuteReader();
                #region List Declaration
                List<DashboardEmpInfo> EmpTable = new List<DashboardEmpInfo>();
                List<IsInternational> InternatTable = new List<IsInternational>();
                List<DashboardEmergencyContact> EmergencyTable = new List<DashboardEmergencyContact>();
                List<DashboardPublicHoliday> PublicHolidayTable = new List<DashboardPublicHoliday>();

                List<News> lstNews = new List<News>();
                List<Events> lstEvents = new List<Events>();
                List<Announcement> lstAnnouncement = new List<Announcement>();
                List<Reward> lstReward = new List<Reward>();
                #endregion

                #region List Class Objects Declaration
                DashboardEmpInfo Emp = new DashboardEmpInfo();
                IsInternational IsInNat = new IsInternational();
                DashboardEmergencyContact EmgCont = new DashboardEmergencyContact();
                DashboardPublicHoliday PublicHoliday = new DashboardPublicHoliday();

                News ObjNews = new News();
                Events ObjEvents = new Events();
                Announcement ObjAnnouncement = new Announcement();
                Reward ObjReward = new Reward();

                #endregion

                if (dtEmpinfo.Rows.Count != 0)
                {
                    Emp.TimeLine_Year = dtEmpinfo.Rows[0]["TimeLine_Year"].ToString();
                    Emp.TimeLine_Month = dtEmpinfo.Rows[0]["TimeLine_Month"].ToString();
                    Emp.TimeLine_Day = dtEmpinfo.Rows[0]["TimeLine_Day"].ToString();
                    Emp.TimeLine_DayOfWeek = dtEmpinfo.Rows[0]["TimeLine_DayOfWeek"].ToString();
                    Emp.TimeLine_WeekOfCurrentYear = dtEmpinfo.Rows[0]["TimeLine_WeekOfCurrentYear"].ToString();
                    Emp.TimeLine_DayOfYear = dtEmpinfo.Rows[0]["TimeLine_DayOfYear"].ToString();
                    Emp.TimeLine_QuarterOfYear = dtEmpinfo.Rows[0]["TimeLine_QuarterOfYear"].ToString();
                    Emp.EmployeeId = dtEmpinfo.Rows[0]["EmployeeId"].ToString();
                    Emp.CompanyName = dtEmpinfo.Rows[0]["CompanyName"].ToString();
                    Emp.CompanyId = Convert.ToInt32(dtEmpinfo.Rows[0]["CompanyId"].ToString());
                    Emp.EmployeeBranchId = Convert.ToInt32(dtEmpinfo.Rows[0]["EmployeeBranchId"].ToString());
                    Emp.BranchName = dtEmpinfo.Rows[0]["BranchName"].ToString();
                    Emp.EmployeeName = dtEmpinfo.Rows[0]["EmployeeName"].ToString();
                    Emp.EmployeeNo = dtEmpinfo.Rows[0]["EmployeeNo"].ToString();
                    Emp.EmployeeCardNo = dtEmpinfo.Rows[0]["EmployeeCardNo"].ToString();
                    Emp.Gender = dtEmpinfo.Rows[0]["Gender"].ToString();
                    //Emp.JoiningDate = Convert.ToDateTime(dtEmpinfo.Rows[0]["JoiningDate"].ToString());
                    Emp.JoiningDate = dtEmpinfo.Rows[0]["JoiningDate"] != DBNull.Value ? Convert.ToDateTime(dtEmpinfo.Rows[0]["JoiningDate"]) : (DateTime?)null;
                    //Emp.ConfirmationDate = Convert.ToDateTime(dtEmpinfo.Rows[0]["ConfirmationDate"].ToString());
                    //Emp.ProbationDueDate = Convert.ToDateTime(dtEmpinfo.Rows[0]["ProbationDueDate"].ToString());
                    //Emp.BirthdayDate = Convert.ToDateTime(dtEmpinfo.Rows[0]["BirthdayDate"].ToString());
                    Emp.EmployeeDepartmentId = Convert.ToInt32(dtEmpinfo.Rows[0]["EmployeeDepartmentId"].ToString());
                    Emp.DepartmentName = dtEmpinfo.Rows[0]["DepartmentName"].ToString();
                    Emp.EmployeeDesignationId = dtEmpinfo.Rows[0]["EmployeeDesignationId"].ToString();
                    Emp.DesignationName = dtEmpinfo.Rows[0]["DesignationName"].ToString();
                    Emp.GradeName = dtEmpinfo.Rows[0]["GradeName"].ToString();
                    Emp.EmployeeGroupName = dtEmpinfo.Rows[0]["EmployeeGroupName"].ToString();
                    Emp.EmployeeTypeName = dtEmpinfo.Rows[0]["EmployeeTypeName"].ToString();
                    //Emp.WeekOff1 = dtEmpinfo.Rows[0]["WeekOff1"].ToString();
                    //Emp.WeekOff2 = dtEmpinfo.Rows[0]["WeekOff2"].ToString();
                    //Emp.WeekOff2Week = dtEmpinfo.Rows[0]["WeekOff2Week"].ToString();
                    //Emp.PrimaryMobile = dtEmpinfo.Rows[0]["PrimaryMobile"].ToString();
                    //Emp.SecondaryMobile = dtEmpinfo.Rows[0]["SecondaryMobile"].ToString();
                    //Emp.WhatsAppNo = dtEmpinfo.Rows[0]["WhatsAppNo"].ToString();
                    //Emp.PersonalEmailId = dtEmpinfo.Rows[0]["PersonalEmailId"].ToString();
                    //Emp.ReportingManager1Id = Convert.ToInt32(dtEmpinfo.Rows[0]["ReportingManager1Id"].ToString());
                    //Emp.ReportingManager2Id = Convert.ToInt32(dtEmpinfo.Rows[0]["ReportingManager2Id"].ToString());
                    //Emp.ReportingHRId = Convert.ToInt32(dtEmpinfo.Rows[0]["ReportingHRId"].ToString());
                    //Emp.ReportingAccountId = Convert.ToInt32(dtEmpinfo.Rows[0]["ReportingAccountId"].ToString());
                    //Emp.ReportingManager1Name = dtEmpinfo.Rows[0]["ReportingManager1Name"].ToString();
                    //Emp.ReportingManager2Name = dtEmpinfo.Rows[0]["ReportingManager2Name"].ToString();
                    //Emp.ReportingHRName = dtEmpinfo.Rows[0]["ReportingHRName"].ToString();
                    //Emp.ReportingAccountName = dtEmpinfo.Rows[0]["ReportingAccountName"].ToString();
                    Emp.TotalAge = dtEmpinfo.Rows[0]["TotalAge"].ToString();
                    Emp.TotalExperience = dtEmpinfo.Rows[0]["TotalExperience"].ToString();
                    Emp.IsManager = dtEmpinfo.Rows[0]["IsManager"].ToString();
                    Emp.LastLoginTime = dtEmpinfo.Rows[0]["LastLoginTime"].ToString();
                    Emp.LastPasswordChangeDayCount = dtEmpinfo.Rows[0]["LastPasswordChangeDayCount"].ToString();
                    Emp.EmployeeFirstName = dtEmpinfo.Rows[0]["EmployeeFirstName"].ToString();
                    Emp.Remark1 = dtEmpinfo.Rows[0]["Remark1"].ToString();
                    Emp.Remark2 = dtEmpinfo.Rows[0]["Remark2"].ToString();
                    Emp.Remark3 = dtEmpinfo.Rows[0]["Remark3"].ToString();
                    Emp.Remark4 = dtEmpinfo.Rows[0]["Remark4"].ToString();
                    Emp.Remark5 = dtEmpinfo.Rows[0]["Remark5"].ToString();
                    Emp.Remark6 = dtEmpinfo.Rows[0]["Remark6"].ToString();
                    Emp.Remark7 = dtEmpinfo.Rows[0]["Remark7"].ToString();
                    Emp.Remark8 = dtEmpinfo.Rows[0]["Remark8"].ToString();
                    Emp.Remark9 = dtEmpinfo.Rows[0]["Remark9"].ToString();
                    Emp.Remark10 = dtEmpinfo.Rows[0]["Remark10"].ToString();
                    Emp.PersonalEmailId = dtEmpinfo.Rows[0]["PersonalEmailId"].ToString();

                    Emp.HeaderColor = dtEmpinfo.Rows[0]["HeaderColor"].ToString();
                    Emp.HeaderTextColor = dtEmpinfo.Rows[0]["HeaderTextColor"].ToString();

                    EmpTable.Add(Emp);


                    string FirstNameOnly = Emp.EmployeeName;
                    Session["EmployeeFirstNameOnly"] = FirstNameOnly.Split(' ')[0];

                    Session["EmployeeNo"] = Emp.EmployeeNo;
                    Session["EmployeeName"] = Emp.EmployeeName;
                    Session["CompanyName"] = Emp.CompanyName;
                    Session["EmployeeFirstName"] = Emp.EmployeeFirstName;
                    Session["LastLoginTime"] = Emp.LastLoginTime;
                    Session["DesignationName"] = Emp.DesignationName;
                    Session["IsManager"] = Emp.IsManager;
                    Session["DesignationId"] = Emp.EmployeeDesignationId;
                    Session["CompanyId"] = Emp.CompanyId;
                    Session["BranchId"] = Emp.EmployeeBranchId;
                    Session["EmployeeBranchId"] = Emp.EmployeeBranchId;
                    Session["TimeLine_Year"] = Emp.TimeLine_Year;
                    Session["TimeLine_Month"] = Emp.TimeLine_Month;
                    Session["TimeLine_Day"] = Emp.TimeLine_Day;
                    Session["TimeLine_DayOfWeek"] = Emp.TimeLine_DayOfWeek;
                    Session["TimeLine_WeekOfCurrentYear"] = Emp.TimeLine_WeekOfCurrentYear;
                    Session["TimeLine_DayOfYear"] = Emp.TimeLine_DayOfYear;
                    Session["TimeLine_QuarterOfYear"] = Emp.TimeLine_QuarterOfYear;
                    Session["DepartmentName"] = Emp.DepartmentName;
                    Session["EmployeeDepartmentId"] = Emp.EmployeeDepartmentId;
                    Session["JoiningDate"] = Emp.JoiningDate;
                    Session["EmployeeCardNo"] = Emp.EmployeeCardNo;
                    Session["ManagerId1"] = Emp.ReportingManager1Id;
                    Session["ManagerId2"] = Emp.ReportingManager2Id;
                    Session["HRId"] = Emp.ReportingHRId;
                    Session["BranchName"] = Emp.BranchName;
                    Session["PersonalEmailId"] = Emp.PersonalEmailId;

                    Session["HeaderColor"] = Emp.HeaderColor;
                    Session["HeaderTextColor"] = Emp.HeaderTextColor;
                }

                //EMERGENCY CONTACT GET LIST
                for (int i = 0; i < dtEmgCont.Rows.Count; i++)
                {
                    EmgCont = EmgCont = new DashboardEmergencyContact();
                    EmgCont.Name = dtEmgCont.Rows[i]["Name"].ToString();
                    EmgCont.Type = dtEmgCont.Rows[i]["Type"].ToString();
                    EmgCont.MobileNo = dtEmgCont.Rows[i]["MobileNo"].ToString();
                    EmergencyTable.Add(EmgCont);
                }
                //Session["EmgContactList"] = EmergencyTable;
                if (EmergencyTable != null)
                {
                    Session["EmgContactList"] = EmergencyTable;
                }
                else
                {
                    Session["EmgContactList"] = "";
                }

                //PUBLIC HOLIDAY GET LIST
                for (int i = 0; i < dtPubHoliday.Rows.Count; i++)
                {
                    PublicHoliday = PublicHoliday = new DashboardPublicHoliday();
                    // PublicHoliday.PublicHolidayDate = DateTime.Parse(dtPubHoliday.Rows[i]["PublicHolidayDate"].ToString());
                    PublicHoliday.PublicHolidayDate = dtPubHoliday.Rows[i]["PublicHolidayDate"].ToString();
                    PublicHoliday.Description = dtPubHoliday.Rows[i]["Description"].ToString();
                    PublicHolidayTable.Add(PublicHoliday);
                }

                if (EmergencyTable.Count != null)
                {
                    Session["PublicHolidayList"] = PublicHolidayTable;
                }
                else
                {
                    Session["PublicHolidayList"] = "";
                }

                //ADVERTISE CODE
                for (int i = 0; i < dtIsInNat.Rows.Count; i++)
                {
                    IsInNat = new IsInternational();
                    IsInNat.Type = dtIsInNat.Rows[i]["Type"].ToString();
                    IsInNat.Description = dtIsInNat.Rows[i]["Description"].ToString();
                    InternatTable.Add(IsInNat);
                }

                if (InternatTable.Count != 0)
                {
                    Session["IsInternationalTypeAndDesc"] = InternatTable;
                }
                else
                {
                    Session["IsInternationalTypeAndDesc"] = null;
                }

                //NEWS
                var NewsGetPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='News'");
                var NewsFisrtPath = NewsGetPath?.DocInitialPath;
                var NewsSecondPath = "";
                var NewsFullPath = "";
                for (int i = 0; i < dtNews.Rows.Count; i++)
                {
                    ObjNews = new News();
                    ObjNews.NewsID = Convert.ToInt32(dtNews.Rows[i]["NewsID"]);
                    ObjNews.NewsTitle = dtNews.Rows[i]["NewsTitle"].ToString();
                    ObjNews.NewsDescripation = dtNews.Rows[i]["NewsDescripation"].ToString();

                    NewsSecondPath = dtNews.Rows[i]["FilePath"].ToString();
                    NewsFullPath = NewsFisrtPath + ObjNews.NewsID + "\\" + NewsSecondPath;
                    if (System.IO.File.Exists(NewsFullPath))
                    {
                        string extension = Path.GetExtension(NewsFullPath).ToLower();

                        if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                        {
                            using (Image image = Image.FromFile(NewsFullPath))
                            {
                                using (MemoryStream m = new MemoryStream())
                                {
                                    image.Save(m, image.RawFormat);
                                    byte[] imageBytes = m.ToArray();
                                    string base64String = Convert.ToBase64String(imageBytes);
                                    ObjNews.FilePath = "data:image;base64," + base64String; // ✅ image base64
                                }
                            }
                        }
                        else if (extension == ".pdf")
                        {
                            byte[] fileBytes = System.IO.File.ReadAllBytes(NewsFullPath);
                            string base64String = Convert.ToBase64String(fileBytes);
                            ObjNews.FilePath = "data:application/pdf;base64," + base64String; // ✅ pdf base64
                        }
                    }
                    lstNews.Add(ObjNews);
                }
                if (lstNews.Count != 0)
                {
                    Session["News"] = lstNews;
                }
                else
                {
                    Session["News"] = null;
                }

                //EVENTS
                var EventGetPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Event'");
                var EventFisrtPath = EventGetPath?.DocInitialPath;
                var EventSecondPath = "";
                var EventFullPath = "";
                for (int i = 0; i < dtEvents.Rows.Count; i++)
                {
                    ObjEvents = new Events();
                    ObjEvents.EventID = Convert.ToInt32(dtEvents.Rows[i]["EventID"]);
                    ObjEvents.EventTitle = dtEvents.Rows[i]["EventTitle"].ToString();
                    ObjEvents.EventDescripation = dtEvents.Rows[i]["EventDescripation"].ToString();
                    EventSecondPath = dtEvents.Rows[i]["FilePath"].ToString();

                    EventFullPath = EventFisrtPath + ObjEvents.EventID + "\\" + EventSecondPath;
                    if (System.IO.File.Exists(EventFullPath))
                    {
                        string extension = Path.GetExtension(EventFullPath).ToLower();
                        byte[] fileBytes = System.IO.File.ReadAllBytes(EventFullPath);
                        string base64String = Convert.ToBase64String(fileBytes);

                        if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                        {
                            ObjEvents.FilePath = "data:image/" + extension.Replace(".", "") + ";base64," + base64String;
                        }
                        if (extension == ".pdf")
                        {
                            ObjEvents.FilePath = "data:application/pdf;base64," + base64String;
                        }
                    }

                    //if (EventSecondPath != "")
                    //{
                    //    try
                    //    {
                    //        EventFullPath = Path.Combine(EventFisrtPath, ObjEvents.EventID.ToString(), EventSecondPath);
                    //        string directoryPath = Path.GetDirectoryName(EventFullPath);
                    //        if (!Directory.Exists(directoryPath))
                    //        {
                    //            ObjEvents.FilePath = "";
                    //        }
                    //        else
                    //        {
                    //            string extension = Path.GetExtension(EventFullPath).ToLower();
                    //            if (System.IO.File.Exists(EventFullPath))
                    //            {
                    //                if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                    //                {
                    //                    using (Image image = Image.FromFile(EventFullPath))
                    //                    {
                    //                        using (MemoryStream m = new MemoryStream())
                    //                        {
                    //                            image.Save(m, image.RawFormat);
                    //                            byte[] imageBytes = m.ToArray();

                    //                            string base64String = Convert.ToBase64String(imageBytes);
                    //                            ObjEvents.FilePath = "data:image/" + extension.Replace(".", "") + ";base64," + base64String;
                    //                        }
                    //                    }
                    //                }
                    //                else if (extension == ".pdf")
                    //                {
                    //                    // Directly read PDF bytes
                    //                    byte[] fileBytes = System.IO.File.ReadAllBytes(EventFullPath);
                    //                    string base64String = Convert.ToBase64String(fileBytes);
                    //                    ObjEvents.FilePath = "data:application/pdf;base64," + base64String;
                    //                }
                    //                else
                    //                {
                    //                    ObjEvents.FilePath = ""; // unsupported file type
                    //                }
                    //            }
                    //            else
                    //            {
                    //                ObjEvents.FilePath = ""; // file not found
                    //            }


                    //            //string extension = Path.GetExtension(EventFullPath).ToLower();
                    //            //byte[] fileBytes = System.IO.File.ReadAllBytes(EventFullPath);
                    //            //string base64String = Convert.ToBase64String(fileBytes);

                    //            //if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                    //            //{
                    //            //    ObjEvents.FilePath = "data:image/" + extension.Replace(".", "") + ";base64," + base64String;
                    //            //}
                    //            //if (extension == ".pdf")
                    //            //{
                    //            //    ObjEvents.FilePath = "data:application/pdf;base64," + base64String;
                    //            //}
                    //        }
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        if (ex.Message != null)
                    //        {
                    //            ObjEvents.FilePath = "";
                    //        }
                    //    }
                    //}




                    //EventFullPath = EventFisrtPath +  ObjEvents.EventID + "\\" + EventSecondPath;
                    //if (System.IO.File.Exists(EventFullPath))
                    //{
                    //    string extension = Path.GetExtension(EventFullPath).ToLower();

                    //    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                    //    {
                    //        using (Image image = Image.FromFile(EventFullPath))
                    //        {
                    //            using (MemoryStream m = new MemoryStream())
                    //            {
                    //                image.Save(m, image.RawFormat);
                    //                byte[] imageBytes = m.ToArray();
                    //                string base64String = Convert.ToBase64String(imageBytes);
                    //                ObjEvents.FilePath = "data:image;base64," + base64String; // ✅ image base64
                    //            }
                    //        }
                    //    }
                    //    else if (extension == ".pdf")
                    //    {
                    //        byte[] fileBytes = System.IO.File.ReadAllBytes(EventFullPath);
                    //        string base64String = Convert.ToBase64String(fileBytes);
                    //        ObjEvents.FilePath = "data:application/pdf;base64," + base64String; // ✅ pdf base64
                    //    }
                    //}

                    lstEvents.Add(ObjEvents);
                }
                if (lstEvents.Count != 0)
                {
                    Session["Events"] = lstEvents;
                }
                else
                {
                    Session["Events"] = null;
                }

                //ANNOUNCEMENT

                var AnnouncementGetPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Announcement'");
                var AnnouncementFisrtPath = AnnouncementGetPath?.DocInitialPath;
                var AnnouncementSecondPath = "";
                var AnnouncementFullPath = "";
                for (int i = 0; i < dtAnnouncement.Rows.Count; i++)
                {
                    ObjAnnouncement = new Announcement();
                    ObjAnnouncement.AnnouncementID = Convert.ToInt32(dtAnnouncement.Rows[i]["AnnouncementID"]);
                    ObjAnnouncement.AnnouncementTitle = dtAnnouncement.Rows[i]["AnnouncementTitle"].ToString();
                    ObjAnnouncement.AnnouncementDescripition = dtAnnouncement.Rows[i]["AnnouncementDescripition"].ToString();

                    AnnouncementSecondPath = dtAnnouncement.Rows[i]["FilePath"].ToString();
                    AnnouncementFullPath = AnnouncementFisrtPath + ObjAnnouncement.AnnouncementID + "\\" + AnnouncementSecondPath;
                    if (System.IO.File.Exists(AnnouncementFullPath))
                    {
                        string extension = Path.GetExtension(AnnouncementFullPath).ToLower();

                        if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                        {
                            using (Image image = Image.FromFile(AnnouncementFullPath))
                            {
                                using (MemoryStream m = new MemoryStream())
                                {
                                    image.Save(m, image.RawFormat);
                                    byte[] imageBytes = m.ToArray();
                                    string base64String = Convert.ToBase64String(imageBytes);
                                    ObjAnnouncement.FilePath = "data:image;base64," + base64String; // ✅ image base64
                                }
                            }
                        }
                        else if (extension == ".pdf")
                        {
                            byte[] fileBytes = System.IO.File.ReadAllBytes(AnnouncementFullPath);
                            string base64String = Convert.ToBase64String(fileBytes);
                            ObjAnnouncement.FilePath = "data:application/pdf;base64," + base64String; // ✅ pdf base64
                        }
                    }




                    lstAnnouncement.Add(ObjAnnouncement);
                }
                if (lstAnnouncement.Count != 0)
                {
                    Session["Announcement"] = lstAnnouncement;
                }
                else
                {
                    Session["Announcement"] = null;
                }

                //REWARD
                var RewardGetPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='RewardAndRecognition'");
                var RewardFisrtPath = RewardGetPath?.DocInitialPath;
                var RewardSecondPath = "";
                var RewardFullPath = "";
                for (int i = 0; i < dtReward.Rows.Count; i++)
                {
                    ObjReward = new Reward();
                    ObjReward.RewardID = Convert.ToInt32(dtReward.Rows[i]["RewardID"]);
                    ObjReward.RewardTitle = dtReward.Rows[i]["RewardTitle"].ToString();
                    ObjReward.RewardDescripition = dtReward.Rows[i]["RewardDescripition"].ToString();
                    ObjReward.FilePath = dtReward.Rows[i]["FilePath"].ToString();

                    //RewardFullPath = RewardFisrtPath + ObjReward.RewardID + "\\" + RewardSecondPath;
                    //if (System.IO.File.Exists(RewardFullPath))
                    //{
                    //    string extension = Path.GetExtension(RewardFullPath).ToLower();

                    //    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                    //    {
                    //        using (Image image = Image.FromFile(RewardFullPath))
                    //        {
                    //            using (MemoryStream m = new MemoryStream())
                    //            {
                    //                image.Save(m, image.RawFormat);
                    //                byte[] imageBytes = m.ToArray();
                    //                string base64String = Convert.ToBase64String(imageBytes);
                    //                ObjReward.FilePath = "data:image;base64," + base64String; // ✅ image base64
                    //            }
                    //        }
                    //    }
                    //    else if (extension == ".pdf")
                    //    {
                    //        byte[] fileBytes = System.IO.File.ReadAllBytes(RewardFullPath);
                    //        string base64String = Convert.ToBase64String(fileBytes);
                    //        ObjReward.FilePath = "data:application/pdf;base64," + base64String; // ✅ pdf base64
                    //    }
                    //}
                    lstReward.Add(ObjReward);
                }
                if (lstReward.Count != 0)
                {
                    Session["Reward"] = lstReward;
                }
                else
                {
                    Session["Reward"] = null;
                }

                DynamicParameters UpcomingDate = new DynamicParameters();
                UpcomingDate.Add("@p_CmpId", Session["CompanyId"]);
                UpcomingDate.Add("@p_BranchId", Session["BranchId"]);
                UpcomingDate.Add("@p_EmployeeId", Session["EmployeeId"]);
                var GetUpcomingDate = DapperORM.ExecuteSP<dynamic>("SP_GetUpcommingDates", UpcomingDate).ToList(); // SP_getReportingManager


                if (GetUpcomingDate != null)
                {
                    Session["UpcomingDate"] = GetUpcomingDate;
                    //Session["Date"] = GetUpcomingDate.Date;
                    //Session["Name"] = GetUpcomingDate.Name;
                    //Session["RemainingDays"] = GetUpcomingDate.RemainingDays;
                }
                else
                {
                    Session["UpcomingDate"] = "";
                }


                //sqlcon.Close();

                //SET COMPANY LOGO COMPANY WISE
                var path = DapperORM.DynamicQuerySingle("Select Logo from Mas_CompanyProfile Where CompanyId = " + Session["CompanyId"] + "");
                var SecondPath = path.Logo;
                var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='CompanyLogo'");
                var FisrtPath = GetDocPath.DocInitialPath + Session["CompanyId"] + "\\";
                string GetBase64 = null;
                string fullPath = "";
                fullPath = FisrtPath + SecondPath;
                string extensionType = Path.GetExtension(fullPath).ToLower();
                string mimeType = "image/jpeg";

                if (extensionType == ".png")
                {
                    mimeType = "image/png";
                }
                else if (extensionType == ".jpg" || extensionType == ".jpeg")
                {
                    mimeType = "image/jpeg";
                }

                //string Extention = System.IO.Path.GetExtension(fullPath);
                if (path.Logo != null)
                {
                    try
                    {
                        string directoryPath = Path.GetDirectoryName(fullPath);
                        if (!Directory.Exists(directoryPath))
                        {
                            Session["CompanyLogo"] = "";
                        }
                        else
                        {
                            using (Image image = Image.FromFile(fullPath))
                            {
                                using (MemoryStream m = new MemoryStream())
                                {
                                    image.Save(m, image.RawFormat);
                                    byte[] imageBytes = m.ToArray();

                                    // Convert byte[] to Base64 String
                                    string base64String = Convert.ToBase64String(imageBytes);
                                    Session.Remove("CompanyLogo");
                                    Session["CompanyLogo"] = "data:" + mimeType + ";base64," + base64String;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message != null)
                        {
                            Session["CompanyLogo"] = "";
                        }
                    }
                }

                //if (path.Logo != null)
                //{
                //    string Extention = System.IO.Path.GetExtension(fullPath);
                //    if (Directory.Exists(fullPath))
                //    {
                //        if (!Directory.Exists(fullPath))
                //        {
                //            using (Image image = Image.FromFile(fullPath))
                //            {
                //                using (MemoryStream m = new MemoryStream())
                //                {
                //                    image.Save(m, image.RawFormat);
                //                    byte[] imageBytes = m.ToArray();
                //                    // Convert byte[] to Base64 String
                //                    string base64String = Convert.ToBase64String(imageBytes);
                //                    GetBase64 = "data:image; base64," + base64String;
                //                    Session["CompanyLogo"] = GetBase64;
                //                }
                //            }
                //        }
                //    }

                //}

                //SET EMPLOYEE PROFILE PHOTO
                DynamicParameters paramProfilePath1 = new DynamicParameters();
                paramProfilePath1.Add("@query", @"Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Onboarding'");
                var path1 = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramProfilePath1).FirstOrDefault();
                var FirstPaths = "";
                if (path1 != null)
                {
                    FirstPaths = path1.DocInitialPath;
                }

                DynamicParameters paramProfilePath2 = new DynamicParameters();
                paramProfilePath2.Add("@query", @"Select PhotoPath from Mas_Employee_Photo where PhotoEmployeeId= " + Session["EmployeeId"] + "");
                var path2 = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramProfilePath2).FirstOrDefault();

                string GetBase64s = null;
                string fullPaths = "";

                if (path2 != null)
                {
                    var SecondPaths = path2.PhotoPath;
                    fullPaths = FirstPaths + Session["EmployeeId"] + '\\' + "Photo" + '\\' + SecondPaths;
                }
                if (true)
                {

                }

                string extensionTypeProfile = Path.GetExtension(fullPath).ToLower();
                string mimeTypeProfile = "image/jpeg";

                if (extensionTypeProfile == ".png")
                {
                    mimeTypeProfile = "image/png";
                }
                else if (extensionTypeProfile == ".jpg" || extensionTypeProfile == ".jpeg")
                {
                    mimeTypeProfile = "image/jpeg";
                }

                if (path1.DocInitialPath != null)
                {
                    try
                    {
                        if (fullPaths != "")
                        {
                            string directoryPath = Path.GetDirectoryName(fullPaths);
                            if (!Directory.Exists(directoryPath))
                            {
                                Session["EmployeeProfile"] = "";
                            }
                            else
                            {
                                using (Image image = Image.FromFile(fullPaths))
                                {
                                    using (MemoryStream m = new MemoryStream())
                                    {
                                        image.Save(m, image.RawFormat);
                                        byte[] imageBytes = m.ToArray();

                                        // Convert byte[] to Base64 String
                                        string base64String = Convert.ToBase64String(imageBytes);
                                        Session["EmployeeProfile"] = "data:" + mimeTypeProfile + ";base64," + base64String;
                                    }
                                }
                            }
                        }
                        else
                        {
                            Session["EmployeeProfile"] = "";
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message != null)
                        {
                            Session["EmployeeProfile"] = "";
                        }
                    }
                }

                //if(dtIsInNat.Rows.Count != 0)
                //{
                //    IsInNat.Type = dtIsInNat.Rows[0]["Type"].ToString();
                //    IsInNat.Description = dtIsInNat.Rows[0]["Description"].ToString();
                //    InternatTable.Add(IsInNat);
                //    Session["IsInternationalType"] = IsInNat.Type;
                //    Session["IsInternationalDesc"] = IsInNat.Description;
                //    sqlcon.Close();
                //}
                //else
                //{
                //    Session["IsInternationalType"] = "";
                //    Session["IsInternationalDesc"] = "";
                //    sqlcon.Close();
                //}

            }//try End
            catch (Exception ex)
            {
                if (ex.Message != null)
                {
                    Session["CompanyLogo"] = "";
                }
                // throw ex;
            }
        }
        #endregion

        #region Logout
        public ActionResult Logout()
        {
            try
            {
                if (Session["EmployeeId"] != null)
                {
                    DynamicParameters paramLog = new DynamicParameters();
                    paramLog.Add("@p_process", "Save");
                    paramLog.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramLog.Add("@p_Status", "Logout");
                    paramLog.Add("@p_MachineName", Dns.GetHostName().ToString());
                    paramLog.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Tool_UserLogin", paramLog);
                }
                Session.Clear();
                Session.Abandon();
                FormsAuthentication.SignOut();
                //foreach (string cookie in Request.Cookies.AllKeys)
                //{
                //    Response.Cookies[cookie].Expires = DateTime.Now.AddDays(-1); // if AddDays(+1) then after 1 hour Cookies will clear
                //}
                return RedirectToAction("Login", "Login");
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        #endregion

        #region ErrorPage
        public ActionResult ErrorPage()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion ErrorPage

        #region ClearErrorSession
        public ActionResult ClearErrorSession()
        {
            try
            {
                Session["GetErrorMessage"] = "";
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion ErrorPage

        #region Login Check ForgotPassword
        [HttpGet]
        public ActionResult GetSecurityQuestion(string CustomerCode, string EmployeeNo)
        {
            try
            {
                var GetConnectionString = sqlcons.Query(@"Select Con_Server, Con_UserId, Con_Password, Con_Database, Con_CustomerCode 
                                                   from CustomerRegistration 
                                                   where Con_CustomerCode = @CustomerCode and Deactivate = 0 and Isactive = 1", new { CustomerCode }).FirstOrDefault();

                if (GetConnectionString.Con_Server == null)
                {
                    return Json(new { Message = "Please enter valid customer code", Icon = "error" }, JsonRequestBehavior.AllowGet);
                }

                if (GetConnectionString.Con_CustomerCode == CustomerCode)
                {
                    DapperORM.SetConnection(GetConnectionString.Con_Server, GetConnectionString.Con_UserId, GetConnectionString.Con_Password, GetConnectionString.Con_Database);
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "Select EmployeeId from Mas_Employee where EmployeeNo='" + EmployeeNo + "' and Deactivate=0 and EmployeeLeft=0");
                    var GetEmployeeId = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).FirstOrDefault();
                    if (GetEmployeeId != null)
                    {
                        DynamicParameters param2 = new DynamicParameters();
                        param2.Add("@query", "Select ESSSecurityQuestion, ESSPassword, ESSAnswer,CompanyMailID,PersonalEmailId,ESSLoginID,ESSLoginAttemptCount from Mas_Employee_ESS,Mas_Employee,MAs_Employee_Personal where ESSEmployeeId = '" + GetEmployeeId.EmployeeId + "' and Mas_Employee_ESS.Deactivate = 0  and MAs_Employee_Personal.Deactivate=0 and Mas_Employee.Deactivate=0 and Mas_Employee_ESS.ESSEmployeeId= Mas_Employee.EmployeeId and MAs_Employee_Personal.PersonalEmployeeId=Mas_Employee.EmployeeId");
                        var GetSequrityQueAns = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param2).FirstOrDefault();

                        DynamicParameters paramCount = new DynamicParameters();
                        paramCount.Add("@query", "SELECT ESSLoginAttemptCount FROM Mas_Employee_ESS WHERE ESSEmployeeId = '" + GetEmployeeId.EmployeeId + "' AND Deactivate = 0");
                        var currentCountResult = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramCount).FirstOrDefault();

                        int currentCount = 0;
                        if (currentCountResult != null && currentCountResult.ESSLoginAttemptCount != null)
                        {
                            currentCount = Convert.ToInt32(currentCountResult.ESSLoginAttemptCount);
                        }

                        int newCount = currentCount + 1;

                        if (newCount > 3)
                        {
                            DynamicParameters paramess1 = new DynamicParameters();
                            paramess1.Add("@query", "UPDATE Mas_Employee_ESS SET ESSIsLock = 1 WHERE ESSEmployeeId = '" + GetEmployeeId.EmployeeId + "' AND Deactivate = 0");
                            var currentCountResult1 = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramess1).FirstOrDefault();
                            var data = new { message = "Your account has been locked due to multiple failed login attempts. Please contact the admin to restore access.", status = "locked" };
                            return Json(data, JsonRequestBehavior.AllowGet);
                        }


                        var RemainingAttempt = (3 - (GetSequrityQueAns.ESSLoginAttemptCount));
                        if (GetSequrityQueAns != null)
                        {
                            var data = new
                            {
                                Question = GetSequrityQueAns.ESSSecurityQuestion,
                                Password = GetSequrityQueAns.ESSPassword,
                                Answer = GetSequrityQueAns.ESSAnswer,
                                CompanyEmail = GetSequrityQueAns.CompanyMailID,
                                PersonalEmail = GetSequrityQueAns.PersonalEmailId,
                                ESSLoginID = GetSequrityQueAns.ESSLoginID,
                                CustomerCode = CustomerCode,
                                RemainingAttempt = RemainingAttempt
                            };

                            return Json(new { data }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { Message = "No security question found", Icon = "error" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { Message = "Please enter a valid employee number!", Icon = "error" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { Message = "Customer code is invalid", Icon = "error" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpGet]
        public ActionResult ForgotPassword(string EmployeeNo, string SendPassword, string EmailId, string SendESSLoginID, string CustomerCode)
        {
            try
            {


                DynamicParameters paramId = new DynamicParameters();
                paramId.Add("@query", "Select EmployeeId from Mas_Employee where EmployeeNo='" + EmployeeNo + "' and Deactivate=0 and EmployeeLeft=0");
                var GetEmployeeId = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramId).FirstOrDefault();
                if (GetEmployeeId != null)
                {

                    DynamicParameters paramCount = new DynamicParameters();
                    paramCount.Add("@query", "SELECT ESSLoginAttemptCount FROM Mas_Employee_ESS WHERE ESSEmployeeId = '" + GetEmployeeId.EmployeeId + "' AND Deactivate = 0");
                    //paramCount.Add("@ESSEmployeeId", GetEmployeeId.EmployeeId);
                    var currentCountResult = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramCount).FirstOrDefault();

                    int currentCount = 0;
                    if (currentCountResult != null && currentCountResult.ESSLoginAttemptCount != null)
                    {
                        currentCount = Convert.ToInt32(currentCountResult.ESSLoginAttemptCount);
                    }

                    int newCount = currentCount + 1;

                    if (newCount > 3)
                    {
                        //DynamicParameters paramess = new DynamicParameters();
                        //paramess.Add("@query", "UPDATE Mas_Employee_ESS SET ESSLoginAttemptCount = @NewCount WHERE ESSEmployeeId = @ESSEmployeeId AND EmployeeLeft = 0 AND Deactivate = 0");
                        //paramess.Add("@NewCount", newCount);
                        //paramess.Add("@ESSEmployeeId", GetEmployeeId.EmployeeId);
                        //DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramess);

                        var data = new { message = "Account has been locked due to multiple failed attempts.", status = "locked" };
                        return Json(data, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        DynamicParameters paramess = new DynamicParameters();
                        paramess.Add("@query", "UPDATE Mas_Employee_ESS SET ESSLoginAttemptCount = '" + newCount + "' WHERE ESSEmployeeId = '" + GetEmployeeId.EmployeeId + "' AND Deactivate = 0");
                        var currentCountResult1 = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramess).FirstOrDefault();

                    }


                    DynamicParameters paramCMPID = new DynamicParameters();
                    paramCMPID.Add("@query", "Select CmpID,EmployeeName from Mas_Employee where Deactivate=0 and  EmployeeId='" + GetEmployeeId.EmployeeId + "' and  EmployeeLeft=0");
                    var GetCMPID = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramCMPID).FirstOrDefault();

                    DynamicParameters paramToolEmail = new DynamicParameters();
                    paramToolEmail.Add("@query", "Select CmpId, Origin, SMTPServerName, PortNo, SSL, FromEmailId, Password from Tool_EmailSetting where  Deactivate=0 and CmpId='" + GetCMPID.CmpID + "'  and  Origin='" + 4 + "'");
                    var GetToolEmail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramToolEmail).FirstOrDefault();

                    // var ServerDetail = DapperORM.DynamicQuerySingle("Select CmpId, Origin, SMTPServerName, PortNo, SSL, FromEmailId, Password from Tool_EmailSetting where  Deactivate=0 and CmpId='" + GetCMPID.CmpID + "'  and  Origin='" + 1 + "' ");
                    if (GetToolEmail != null)
                    {
                        var SMTPServerName = GetToolEmail.SMTPServerName;
                        var PortNo = GetToolEmail.PortNo;
                        var FromEmailId = GetToolEmail.FromEmailId;
                        var SMTPPassword = GetToolEmail.Password;
                        var SSL = GetToolEmail.SSL;

                        SmtpClient smtp = new SmtpClient(SMTPServerName, PortNo);
                        smtp.EnableSsl = true;
                        smtp.UseDefaultCredentials = true;
                        smtp.Timeout = 100000;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.Credentials = new System.Net.NetworkCredential(FromEmailId, SMTPPassword);
                        MailMessage mail = new MailMessage(FromEmailId, EmailId);
                        mail.Subject = "Account Recovery";
                        var GetBody = "Dear <b>" + GetCMPID.EmployeeName + "</b>,<br><br>" +
                            "As per your request, here are your login credentials.</br></br><br><br>" +
                            "Customer Code: <b>" + CustomerCode + "</b><br>" +
                            "Login Id: <b>" + SendESSLoginID + "</b> </br><br>" +
                            "Password: <b>" + SendPassword + "</b></br></br><br><br>" +
                            "Please ensure to securely store this information. If you did not request this or have any concerns about the security of your account, please contact to admin immediately.</br></br><br><br>" +
                            "Thank you for using KompassHR.</br></br><br>" +
                            "Best regards.</br><br>" +
                            "KompassHR</br>";
                        mail.Body = GetBody;
                        //  mail.Attachments.Add(new Attachment(Fbank.ExportToStream(ExportFormatType.PortableDocFormat), "PaymentSlip.pdf"));
                        mail.IsBodyHtml = true;
                        smtp.Send(mail);
                        var data = "Password send successfully ! to your register emailid";
                        return Json(data, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var data = "SMTP Server Not Found";
                        return Json(data, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    var data = "Employee not found";
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ValidateMFA
        //validate Authentication
        [HttpPost]
        public ActionResult ValidateMFA(string code)
        {
            try
            {
                string empId = Session["EmployeeNo"]?.ToString();

                if (string.IsNullOrEmpty(empId) || string.IsNullOrEmpty(code))
                {
                    return Json(new { status = "error", message = "Invalid input." });
                }

                var paramvd = new DynamicParameters();
                paramvd.Add("@query", $"SELECT IsMFA, MFAToken, MFAEnabled FROM mas_employee_ess ess join Mas_Employee emp on ess.ESSEmployeeId = emp.EmployeeId WHERE emp.EmployeeNo = '" + empId + "' AND  ess.Deactivate = 0 and emp.Deactivate=0 and ess.IsExit=0 and ESSIsActive=1");
                var user = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramvd).FirstOrDefault();
                if (user == null || user.MFAToken == null)
                {
                    return Json(new { status = "error", message = "Token not found." });
                }

                // Check if MFA is actually enabled
                if (user.IsMFA == null || !Convert.ToBoolean(user.IsMFA) || !Convert.ToBoolean(user.MFAEnabled))
                {
                    return Json(new { status = "error", message = "MFA is not enabled for this user." });
                }

                string lastUsedCode = Session["LastUsedCode"]?.ToString();

                if (code == lastUsedCode)
                {
                    return Json(new { status = "failed", message = "Code already used. Please wait for a new code." });
                }

                var tfa = new TwoFactorAuthenticator();

                // Clean the code (remove spaces, trim)
                string cleanCode = code?.Trim().Replace(" ", "");

                // Get ALL possible valid codes for debugging - SAME AS YOUR SETUP METHOD
                string currentCode = tfa.GetCurrentPIN(user.MFAToken);
                string[] allValidCodes = tfa.GetCurrentPINs(user.MFAToken, TimeSpan.FromSeconds(90));

                // Try multiple validation methods - SAME AS YOUR SETUP METHOD
                bool isValidDirect = tfa.ValidateTwoFactorPIN(user.MFAToken, cleanCode);
                bool isValidWithTolerance = tfa.ValidateTwoFactorPIN(user.MFAToken, cleanCode, TimeSpan.FromSeconds(30));
                bool isValidExtended = tfa.ValidateTwoFactorPIN(user.MFAToken, cleanCode, TimeSpan.FromSeconds(60));

                bool isValid = isValidDirect || isValidWithTolerance || isValidExtended;

                Session["MFAAuthenticated"] = false;

                if (isValid)
                {
                    Session["LastUsedCode"] = cleanCode;
                    Session["MFAAuthenticated"] = true;

                    return Json(new { status = "success" });
                }
                else
                {
                    return Json(new
                    {
                        status = "failed",
                        message = "Invalid authentication code.",
                        debugInfo = new
                        {
                            storedToken = user.MFAToken,
                            enteredCode = cleanCode,
                            currentExpectedCode = currentCode,
                            allValidCodes = allValidCodes,
                            validationResults = new
                            {
                                direct = isValidDirect,
                                with30sTolerance = isValidWithTolerance,
                                with60sTolerance = isValidExtended
                            },
                            serverTime = DateTime.Now.ToString("HH:mm:ss"),
                            timeTicks = DateTime.Now.Ticks,
                            tokenLength = user.MFAToken?.Length
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "Error: " + ex.Message });
            }
        }
        #endregion

        #region saveAuditEntry
        public static void saveAuditEntry(DateTime fDate, int loginId, string origin, string formName, string actionMethod, string action, string result, string errorMessage, string _clientIp, string _clientMachineName)
        {
            //DapperORM.SetConnection();
            var parameters = new DynamicParameters();

            parameters.Add("@p_LoginId", loginId);
            parameters.Add("@p_EMPMAIN_Fid", loginId);
            parameters.Add("@p_Origin", origin);
            parameters.Add("@p_Result", result);
            parameters.Add("@p_FormName", formName);
            parameters.Add("@p_ActionMethod", actionMethod);
            parameters.Add("@p_Action", action);
            parameters.Add("@p_ErrorMessage", errorMessage);
            parameters.Add("@p_BUCode", null);  // optional, if needed
            parameters.Add("@p_Fid", null);     // optional
            parameters.Add("@p_FDate", fDate);  // Not used in the proc logic, but you can include it
            //parameters.Add("@p_Mip", Environment.ma);
            //parameters.Add("@p_Mid", Environment.MachineName);

            parameters.Add("@p_Mip", Dns.GetHostEntry(Dns.GetHostName())
                            .AddressList
                            .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?
                            .ToString());

            parameters.Add("@p_Mid", Environment.MachineName);

            var saleByserCatg = DapperORM.ReturnList<dynamic>("SP_LOG_AUDIT", parameters);


            //connection.Execute("SP_LOG_AUDIT", parameters, commandType: CommandType.StoredProcedure);

        }
        #endregion

        //[HttpPost]
        //public ActionResult SendNotification()
        //{
        //    try
        //    {
        //        using (var connection = new SqlConnection(DapperORM.connectionString))
        //        {
        //            string fcmToken = "f34nBtaCT1yYZhVM6K-s9A:APA91bGdTS-cTPfL5cPbnaGG7xm1n4z_HRFNF3m6PoxB6xrlmpv5lPTQmuwGnHmc8pzrQG8oTWerTJqcl0uf7eO51JFMZIvUW6IxFRqP2VqeKiGa_SoJ3LA";

        //            // var fcmTokenRaw = connection.QueryFirstOrDefault<string>(@"select Device_Tokens from Mas_Employee_ESS where ESSEmployeeId = @EmployeeId", new
        //            //{
        //            //    EmployeeId = req.NotifyEmpId
        //            //});

        //            //string fcmToken = fcmTokenRaw.ToString();

        //            var Data = SendNotification(
        //                        "fcmToken123456789",      // FCM Token (device token)
        //                        "Leave Request Approved", // Notification title
        //                        "Your leave request has been approved.", // Notification body
        //                        1001,                     // NotifyEmpId (employee ID)
        //                        "Leave"                   // RequestType
        //                    );

        //            //var Data = await _notificationService.SendNotificationAsync(
        //            //fcmToken,
        //            //req.Title,
        //            //req.Body,
        //            //req.NotifyEmpId,
        //            //req.RequestType
        //            //);

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //public ActionResult SendNotification(string FcmToken, string Title, string Body, int EmployeeId, string RequestType)
        //{
        //    var message = new Message()
        //    {
        //        Token = FcmToken,
        //        Notification = new Notification
        //        {
        //            Title = Title,
        //            Body = Body
        //        },

        //        Data = new Dictionary<string, string>
        //                {
        //                    { "type", RequestType },
        //                    { "employeeId", EmployeeId.ToString() },

        //                },
        //    };

        //    string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        //    //return response;

        //    return new NotificationResult
        //    {
        //        Message = message,
        //        Response = response
        //    };
        //}


    }
}
