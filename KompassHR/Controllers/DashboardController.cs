using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Filter;
using Dapper;
using System.Data.SqlClient;
using KompassHR.Models;
using System.Data;
using System.Collections.Generic;
using System.Net;
using MachineName;
using System.IO;
using System.Dynamic;
using System.Drawing;
using System.Text;

namespace KompassHR.Controllers
{
    public class DashboardController : Controller
    {

        // GET: Dashboard
        // [LoginAuthentication] // Use for Authentication filtter for Login 
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        #region Dashboard
        [HttpGet]
        public ActionResult Dashboard()
        {
            try
            {
                if (Session["EmployeeId"] != null)
                {
                    //This code checks if the user has UserAccess permission and redirects if they don't.
                    if (Session["AccessCheck"] != null && Session["AccessCheck"].ToString() == "False")
                    {
                        TempData["Message"] = "Access restricted: You do not have permission.";
                        TempData["Icon"] = "info";
                        Session["AccessCheck"] = null;
                    }
                    // Save the CustomerCode in a cookie for 30 days
                    string customerCode = string.Empty;
                    if (Request.Cookies["CustomerCode"] != null)
                    {
                        customerCode = Request.Cookies["CustomerCode"].Value;
                    }

                    //string ipAddress = HttpContext.Request.UserHostAddress;
                    //string PCName = Dns.GetHostEntry(Request.ServerVariables["REMOTE_ADDR"]).HostName;
                    //string IP = Request.UserHostName;
                    //string compName = DetermineCompName(IP);
                    //Session["GetMachineName1"] = ipAddress;
                    //Session["GetMachineName2"] = PCName;
                    //Session["GetMachineName3"] = IP;
                    //Session["GetMachineName4"] = compName;

                    var EmpId = Convert.ToInt32(Session["EmployeeId"]);
                    string m = Form1.MachineID.ToString();
                    Session["GetMachineName5"] = m;
                    //string MachineName1 = Environment.MachineName;
                    //string MachineName2 = System.Net.Dns.GetHostName();
                    //string MachineName3 = System.Environment.GetEnvironmentVariable("COMPUTERNAME");
                    //string[] MachineName4 = System.Net.Dns.GetHostEntry(System.Web.HttpContext.Current.Request.ServerVariables["remote_addr"]).HostName.Split(new Char[] { '.' });
                    //TempData["Test1"] = MachineName1;
                    //TempData["Test2"] = MachineName2;
                    //TempData["Test3"] = MachineName3;
                    //TempData["Test4"] = MachineName4[0];

                    //GET DASHBOARD REQUISITION DATA FROM sp_ESSDashboard THIS SP
                    System.Data.DataSet dataset = new System.Data.DataSet();
                    dataset = new System.Data.DataSet();
                    SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
                    if (sqlcon != null && sqlcon.State == ConnectionState.Closed)
                    {
                        sqlcon.Open();
                    }


                    DynamicParameters paramCount = new DynamicParameters();
                    paramCount.Add("@p_Employeeid", Session["EmployeeId"]);
                    var GetCount = DapperORM.ExecuteSP<dynamic>("sp_QuickRquisitionDashboard", paramCount).ToList(); // SP_getReportingManager
                    TempData["SLAccess"] = GetCount[0].SLAccess;
                    TempData["ODAccess"] = GetCount[1].SLAccess;
                    TempData["PGAccess"] = GetCount[2].SLAccess;
                    TempData["WFHAccess"] = GetCount[3].SLAccess;
                    TempData["PMAccess"] = GetCount[4].PMAccess;
                    TempData["CheckINOutAccess"] = GetCount[5].SLAccess;
                    TempData["SCAccess"] = GetCount[5].SLAccess;

                    







                    DynamicParameters NotifyAttenCount = new DynamicParameters();
                    NotifyAttenCount.Add("@p_EmployeeId", Session["EmployeeId"]);
                    NotifyAttenCount.Add("@p_List", "All");
                    var AttennotificationList = DapperORM.ExecuteSP<dynamic>("sp_List_Pending_ForApproval", NotifyAttenCount).ToList();
                    Session["Attendance_NotificationCount"] = AttennotificationList;

                    DynamicParameters NotifyLeaveCount = new DynamicParameters();
                    NotifyLeaveCount.Add("@p_EmployeeId", Session["EmployeeId"]);
                    NotifyLeaveCount.Add("@p_Origin", "Approval");
                    var LeavenotificationList = DapperORM.ExecuteSP<dynamic>("sp_List_ESS_Leave_Approval", NotifyLeaveCount).ToList();
                    Session["Leave_NotificationCount"] = LeavenotificationList;

                    DynamicParameters NotifyTClaimeCount = new DynamicParameters();
                    NotifyTClaimeCount.Add("@p_EmployeeId", Session["EmployeeId"]);
                    NotifyTClaimeCount.Add("@p_Origin", "Approval");
                    NotifyTClaimeCount.Add("@p_ClaimOrigin", "Claim_Travel");
                    var TClaimnotificationList = DapperORM.ExecuteSP<dynamic>("sp_List_ESS_Claim_Approval", NotifyTClaimeCount).ToList();

                    DynamicParameters NotifyGClaimeCount = new DynamicParameters();
                    NotifyGClaimeCount.Add("@p_EmployeeId", Session["EmployeeId"]);
                    NotifyGClaimeCount.Add("@p_Origin", "Approval");
                    NotifyGClaimeCount.Add("@p_ClaimOrigin", "Claim_General");
                    var GClaimnotificationList = DapperORM.ExecuteSP<dynamic>("sp_List_ESS_Claim_Approval", NotifyGClaimeCount).ToList();

                    // Combine both lists into a single session object
                    var combinedNotifications = new Dictionary<string, List<dynamic>>
                    {
                        { "TravelClaim", TClaimnotificationList },
                        { "GeneralClaim",GClaimnotificationList }
                    };

                    // Store in session
                    Session["GTClaim_NotificationCount"] = combinedNotifications;

                    var NotifyBirthdayList = DapperORM.ListOfDynamicQueryListWithParam<dynamic>($@"
                    SELECT me.EmployeeName, FORMAT(mp.BirthdayDate, 'dd-MMM') AS BirthdayDate,  
                    DATEDIFF(DAY, CAST(GETDATE() AS DATE),  DATEADD(YEAR, DATEDIFF(YEAR, mp.BirthdayDate, GETDATE()), mp.BirthdayDate)) AS DaysLeft
                    FROM Mas_employee me
                    INNER JOIN Mas_Employee_Personal mp ON mp.PersonalEmployeeId = me.EmployeeId
                    WHERE me.Deactivate = 0 AND me.EmployeeLeft = 0 
	                AND me.ContractorID = 1 AND DATEADD(YEAR, DATEDIFF(YEAR, mp.BirthdayDate, GETDATE()), mp.BirthdayDate) BETWEEN CAST(GETDATE() AS DATE)
		            AND CAST(DATEADD(DAY, 30, GETDATE()) AS DATE)ORDER BY DaysLeft").ToList();

                    //AND me.EmployeeDepartmentId = (SELECT e.EmployeeDepartmentId FROM mas_employee e WHERE e.EmployeeId = { Session["EmployeeId"]})
                    Session["Birthday_NotificationCount"] = NotifyBirthdayList;


                    DynamicParameters NotifyResignationCount = new DynamicParameters();
                    NotifyResignationCount.Add("@p_EmployeeId", Session["EmployeeId"]);
                    NotifyResignationCount.Add("@p_Origin", "Approval");
                    var ResignationNotificationList = DapperORM.ExecuteSP<dynamic>("sp_List_ESS_resignation_Approval", NotifyResignationCount).ToList();
                    Session["Resignation_NotificationCount"] = ResignationNotificationList;

                    //Reminder 

                    DynamicParameters NotifyReminderCount = new DynamicParameters();
                    NotifyReminderCount.Add("@p_EmployeeId", Session["EmployeeId"]);

                    var ReminderNotificationList = DapperORM.ExecuteSP<dynamic>("sp_List_ESS_ReminderNotify", NotifyReminderCount).ToList();
                    Session["Reminder_NotificationCount"] = ReminderNotificationList;



                    // Employee Confirmation
                  
                    List<dynamic> EmployeeConfirmationList = null;
                    DynamicParameters paramHR = new DynamicParameters();
                    paramHR.Add("@query", "Select  HRId from Tool_HRId where HRId='" + Session["EmployeeId"] + "'");
                    var GetHRId = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramHR).FirstOrDefault();
                    if (GetHRId != null)
                    {
                        Session["HRId"] = GetHRId.HRId;
                        DynamicParameters empConfirmCount = new DynamicParameters();
                        empConfirmCount.Add("@p_EmployeeId", Session["EmployeeId"]);
                        EmployeeConfirmationList = DapperORM.ExecuteSP<dynamic>("sp_List_ESS_EmployeeConfirmationNotify", empConfirmCount).ToList();
                        Session["EmployeeConfirmation_NotificationCount"] = EmployeeConfirmationList;
                        EmployeeConfirmationList.Count();

                    }
                    else
                    {
                        Session["HRId"] = null;
                    }

                    AttennotificationList.Count();
                    LeavenotificationList.Count();
                    TClaimnotificationList.Count();
                    GClaimnotificationList.Count();
                    NotifyBirthdayList.Count();
                    ResignationNotificationList.Count();

                    int totalNotificationsCount =
                        (AttennotificationList?.Count() ?? 0) +
                        (LeavenotificationList?.Count() ?? 0) +
                        (TClaimnotificationList?.Count() ?? 0) +
                        (GClaimnotificationList?.Count() ?? 0) +
                        (NotifyBirthdayList?.Count() ?? 0) +
                        (ResignationNotificationList?.Count() ?? 0) +
                        (EmployeeConfirmationList?.Count() ?? 0);

                    Session["Total_NotificationCount"] = totalNotificationsCount;

                    #region Change Password Notification
                    DynamicParameters paramPass = new DynamicParameters();
                    paramPass.Add("@query", "SELECT T.PasswordChangeDays,T.PasswordChangeExtraDays,ESS.ESSLastPasswordChange,ESS.CreatedDate,ISNULL(ESS.ESSLastPasswordChange, ESS.CreatedDate) AS EffectivePasswordChangeDate,DATEDIFF(DAY, ISNULL(ESS.ESSLastPasswordChange, ESS.CreatedDate), GETDATE()) AS DaysSincePasswordChange,DATEDIFF(DAY, ISNULL(ESS.ESSLastPasswordChange, ESS.CreatedDate), GETDATE()) AS DaysFromEffectivePasswordChangeDate,E.* FROM Tool_CommonTable T CROSS JOIN Mas_Employee E JOIN Mas_Employee_ESS ESS ON E.EmployeeId = ESS.ESSEmployeeId WHERE E.EmployeeId = '" + EmpId + "' ;");
                    var Record1 = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramPass).FirstOrDefault();
                    ViewBag.LastChangePass = Record1.EffectivePasswordChangeDate;
                    ViewBag.MinDateDays = Record1.PasswordChangeDays;
                    ViewBag.MaxDateDays = ((Record1.PasswordChangeDays) + (Record1.PasswordChangeExtraDays));
                    ViewBag.DaysSinceChange = Record1.DaysSincePasswordChange;
                    #endregion

                    #region List Declaration
                    List<DashboardInfo> DashTable = new List<DashboardInfo>();

                    #endregion

                    #region List Class Objects Declaration
                    SqlCommand cmd = new SqlCommand("dbo.[sp_ESSDashboard_My]", sqlcon);
                    cmd.CommandTimeout = 0;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_EmployeeId", Session["EmployeeId"]);
                    SqlDataAdapter dtadp = new SqlDataAdapter(cmd);
                    dtadp.Fill(dataset);
                    DataTable dtDashInfo = dataset.Tables[0];

                  
                    DashboardInfo Dash = new DashboardInfo();
                    if (dtDashInfo.Rows.Count != 0)
                    {
                        Dash.EmployeeId = dtDashInfo.Rows[0]["EmployeeId"].ToString();
                        Dash.LeaveCount = dtDashInfo.Rows[0]["LeaveCount"].ToString();
                        Dash.ShortLeaveCount = dtDashInfo.Rows[0]["ShortLeaveCount"].ToString();
                        Dash.CoffCount = dtDashInfo.Rows[0]["CoffRequisitionCount"].ToString();
                        Dash.CoffGeneratedCount = dtDashInfo.Rows[0]["CoffGeneratedCount"].ToString();
                        Dash.OutDoorCompanyCount = dtDashInfo.Rows[0]["OutDoorCompanyCount"].ToString();
                        Dash.PersonalGatepassCount = dtDashInfo.Rows[0]["PersonalGatepassCount"].ToString();
                        Dash.PunchMissingCount = dtDashInfo.Rows[0]["PunchMissingCount"].ToString();
                        Dash.ShiftChangeCount = dtDashInfo.Rows[0]["ShiftChangeCount"].ToString();
                        Dash.VisitorAppointmentCount = dtDashInfo.Rows[0]["VisitorAppointmentCount"].ToString();
                        Dash.FNF_EmployeeResignationCount = dtDashInfo.Rows[0]["FNF_EmployeeResignationCount"].ToString();
                        Dash.MonthBirthdayTeamCount = dtDashInfo.Rows[0]["MonthBirthdayTeamCount"].ToString();
                        Dash.MonthBirthdayDepartmentCount = dtDashInfo.Rows[0]["MonthBirthdayDepartmentCount"].ToString();
                        Dash.MonthBirthdayFavouriteCount = dtDashInfo.Rows[0]["MonthBirthdayFavouriteCount"].ToString();
                        Dash.TodayBirthdayTeamCount = dtDashInfo.Rows[0]["TodayBirthdayTeamCount"].ToString();
                        Dash.TodayBirthdayDepartmentCount = dtDashInfo.Rows[0]["TodayBirthdayDepartmentCount"].ToString();
                        Dash.TodayBirthdayFavouriteCount = dtDashInfo.Rows[0]["TodayBirthdayFavouriteCount"].ToString();
                        Dash.TodayBirthdayMissedCount = dtDashInfo.Rows[0]["TodayBirthdayMissedCount"].ToString();
                        Dash.NewJoineeTeam = dtDashInfo.Rows[0]["NewJoineeTeam"].ToString();
                        Dash.NewJoineeDepartment = dtDashInfo.Rows[0]["NewJoineeDepartment"].ToString();
                        Dash.TicketTotal = dtDashInfo.Rows[0]["TicketTotal"].ToString();
                        Dash.TicketOpen = dtDashInfo.Rows[0]["TicketOpen"].ToString();
                        Dash.TicketReview = dtDashInfo.Rows[0]["TicketReview"].ToString();
                        Dash.TicketClose = dtDashInfo.Rows[0]["TicketClose"].ToString();
                        Dash.CurrentOpening = dtDashInfo.Rows[0]["CurrentOpening"].ToString();
                        Dash.WarningsTotal = dtDashInfo.Rows[0]["WarningsTotal"].ToString();
                        Dash.WarningsThisMonth = dtDashInfo.Rows[0]["WarningsThisMonth"].ToString();
                        Dash.MyTeamMember = dtDashInfo.Rows[0]["MyTeamMember"].ToString();
                        Dash.MyTeamMemberMale = dtDashInfo.Rows[0]["MyTeamMemberMale"].ToString();
                        Dash.MyTeamMemberFemale = dtDashInfo.Rows[0]["MyTeamMemberFemale"].ToString();
                        Dash.MyDepartmentMember = dtDashInfo.Rows[0]["MyDepartmentMember"].ToString();
                        Dash.MyDepartmentMemberMale = dtDashInfo.Rows[0]["MyDepartmentMemberMale"].ToString();
                        Dash.MyDepartmentMemberFemale = dtDashInfo.Rows[0]["MyDepartmentMemberFemale"].ToString();
                        Dash.TotalBU = dtDashInfo.Rows[0]["TotalBU"].ToString();
                        DashTable.Add(Dash);

                        //TempData["MyTeamMember"] = Dash.MyTeamMember;
                        //TempData["MyTeamMemberMale"] = Dash.MyTeamMemberMale;
                        //TempData["MyTeamMemberFemale"] = Dash.MyTeamMemberFemale;
                        //TempData["MyDepartmentMember"] = Dash.MyDepartmentMember;
                        //TempData["MyDepartmentMemberMale"] = Dash.MyDepartmentMemberMale;
                        //TempData["MyDepartmentMemberFemale"] = Dash.MyDepartmentMemberFemale;

                        ViewBag.MyTeamMember= Dash.MyTeamMember;
                        ViewBag.MyTeamMemberMale= Dash.MyTeamMemberMale;
                        ViewBag.MyTeamMemberFemale = Dash.MyTeamMemberFemale;
                        ViewBag.MyDepartmentMember= Dash.MyDepartmentMember;
                        ViewBag.MyDepartmentMemberMale= Dash.MyDepartmentMemberMale;
                        ViewBag.MyDepartmentMemberFemale = Dash.MyDepartmentMemberFemale;

                        if (Dash != null)
                        {
                            //Quick My Requisition Count Bind
                            ViewBag.GetDashData = Dash;
                            TempData["CurrentOpening"] = Dash.CurrentOpening;
                            TempData["TotalLocation"] = Dash.TotalBU;
                        }
                    }


                    //GET LEAVE BALANCE
                    DynamicParameters paramLeaveBal = new DynamicParameters();
                    paramLeaveBal.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramLeaveBal.Add("@p_CmpId", Session["CompanyId"]);
                    var GetBalance = DapperORM.ExecuteSP<dynamic>("sp_GetLeaveBalance", paramLeaveBal).ToList(); // SP_getReportingManager
                    if (GetBalance.Count != 0)
                    {
                        ViewBag.GetLeaveBalance = GetBalance;
                    }
                    else
                    {
                        ViewBag.GetLeaveBalance = "";
                    }

                    //Quick Team Requisition Count Bind
                    DynamicParameters TeamCount = new DynamicParameters();
                    TeamCount.Add("@p_EmployeeId", Session["EmployeeId"]);
                    var GetTeamCount = DapperORM.ExecuteSP<dynamic>("sp_ESSDashboard_Team", TeamCount).ToList();
                    if (GetTeamCount.Count() != 0)
                    {
                        ViewBag.GetRquisitionTeamCount = GetTeamCount;
                    }
                    else
                    {
                        ViewBag.GetRquisitionTeamCount = null;
                    }

                    //GET NEW JOINING COUNT
                    //var Date = "select convert(nvarchar(20),BirthdayDate,113) As BirthdayDate from mas_employee_Personal  where  PersonalEmployeeId='" + Session["EmployeeId"]+"'";
                    //var Date1 = DapperORM.DynamicQuerySingle(Date);

                    //var Date1 = DapperORM.DynamicQuerySingle(@" select  case when Month(BirthdayDate) > Month( getdate())
                    //                              then convert(nvarchar(11), Convert(datetime, convert(nvarchar(7), BirthdayDate, 106) + convert(nvarchar, year(getdate()))), 106) + ' 00:00:00'
                    //                              else
                    //                              Case when  year(BirthdayDate)%4 =0  
                    //                              then convert(nvarchar(11), Convert(datetime, convert(nvarchar(7), dateadd(day, 0, BirthdayDate), 106) + convert(nvarchar, year(dateadd(year, 4, getdate())))), 106) + ' 00:00:00'
                    //                              Else convert(nvarchar(11), Convert(datetime, convert(nvarchar(7),dateadd(day, 0, BirthdayDate) , 106) + convert(nvarchar, year(dateadd(year, 1, getdate())))), 106) + ' 00:00:00' end
                    //                              end
                    //                              As BirthdayDate from mas_employee_Personal  where PersonalEmployeeId = '" + Session["EmployeeId"] + "'").FirstOrDefault();
                    var Date1 = DapperORM.DynamicQueryList(@"SELECT CASE WHEN MONTH(BirthdayDate) = 2 AND DAY(BirthdayDate) = 29 
                    THEN CASE  WHEN (MONTH(GETDATE()) < 2 OR (MONTH(GETDATE()) = 2 AND DAY(GETDATE()) <= 29))
                    AND (YEAR(GETDATE()) % 4 = 0 AND (YEAR(GETDATE()) % 100 <> 0 OR YEAR(GETDATE()) % 400 = 0))
                    THEN '29 Feb ' + CONVERT(VARCHAR, YEAR(GETDATE())) + ' 00:00:00'
                    ELSE 
                    '29 Feb ' + CONVERT(VARCHAR, 
                    CASE 
                    WHEN ((YEAR(GETDATE()) + 1) % 4 = 0 AND ((YEAR(GETDATE()) + 1) % 100 <> 0 OR (YEAR(GETDATE()) + 1) % 400 = 0)) 
                    THEN YEAR(GETDATE()) + 1 WHEN ((YEAR(GETDATE()) + 2) % 4 = 0 AND ((YEAR(GETDATE()) + 2) % 100 <> 0 OR (YEAR(GETDATE()) + 2) % 400 = 0)) 
                    THEN YEAR(GETDATE()) + 2 WHEN ((YEAR(GETDATE()) + 3) % 4 = 0 AND ((YEAR(GETDATE()) + 3) % 100 <> 0 OR (YEAR(GETDATE()) + 3) % 400 = 0)) 
                    THEN YEAR(GETDATE()) + 3 ELSE YEAR(GETDATE()) + 4 END) + ' 00:00:00'
                    END ELSE
                    CASE 
                    WHEN MONTH(BirthdayDate) > MONTH(GETDATE()) OR (MONTH(BirthdayDate) = MONTH(GETDATE()) AND DAY(BirthdayDate) >= DAY(GETDATE()))
                    THEN CONVERT(VARCHAR(11), CAST(CONVERT(VARCHAR(2), DAY(BirthdayDate)) + ' ' +DATENAME(MONTH, BirthdayDate) + ' ' +
                    CONVERT(VARCHAR(4), YEAR(GETDATE())) AS DATETIME), 106) + ' 00:00:00'
                     ELSE CONVERT(VARCHAR(11), CAST(CONVERT(VARCHAR(2), DAY(BirthdayDate)) + ' ' + DATENAME(MONTH, BirthdayDate) + ' ' +
                    CONVERT(VARCHAR(4), YEAR(GETDATE()) + 1) AS DATETIME), 106) + ' 00:00:00'
                    END
                    END AS BirthdayDate FROM mas_employee_Personal WHERE PersonalEmployeeId = '" + Session["EmployeeId"] + "'").FirstOrDefault();
                    if (Date1 != null)
                    {
                        ViewBag.Dates = Date1.BirthdayDate;
                    }
                    else
                    {
                        ViewBag.Dates = null;
                    }

                    //GET WORK ANNIVERSARY DATE

                    DynamicParameters CalenderParam = new DynamicParameters();
                    CalenderParam.Add("@p_EmployeeId", Session["EmployeeId"]);
                    ViewBag.GetCalenderData = DapperORM.ExecuteSP<dynamic>("sp_GetAttendanceCalender", CalenderParam).ToList();

                    //Quick Requisition Team Count Bind
                    DynamicParameters SideMenuCount = new DynamicParameters();
                    SideMenuCount.Add("@p_EmployeeId", Session["EmployeeId"]);
                    var GetSideMenuCount = DapperORM.ExecuteSP<dynamic>("sp_ESSDashboard_SideMenuCount", SideMenuCount).FirstOrDefault();
                    if (GetTeamCount != null)
                    {
                        // ViewBag.GetGetSideMenuCount = GetSideMenuCount;
                        Session["Inbox_Count"] = GetSideMenuCount.Inbox_Count;
                        Session["OutBox_Count"] = GetSideMenuCount.OutBox_Count;
                        Session["Tax_Count"] = GetSideMenuCount.Tax_Count;
                        Session["PMS_Count"] = GetSideMenuCount.PMS_Count;
                        Session["TMS_Count"] = GetSideMenuCount.TMS_Count;
                        Session["VMS_Count"] = GetSideMenuCount.VMS_Count;
                        Session["FNF_Count"] = GetSideMenuCount.FNF_Count;
                        Session["Conference_Count"] = GetSideMenuCount.Conference_Count;
                        Session["CurrentOpening_Count"] = GetSideMenuCount.CurrentOpening_Count;
                        Session["Warnning_Count"] = GetSideMenuCount.Warnning_Count;
                        Session["EmployeeReview_Count"] = GetSideMenuCount.EmployeeReview_Count;
                        Session["Canteen_Count"] = GetSideMenuCount.Canteen_Count;
                    }
                    else
                    {
                        ViewBag.GetGetSideMenuCount = "";
                    }
                    var DocRead = DapperORM.DynamicQuerySingle("SELECT * FROM Tool_LMS_DownloadUpload_URL WHERE DocOrigin='TrainingRead'");
                    var GetDocRead = DocRead?.DownloadUploadURL ?? string.Empty;
                    var GetUrl = GetDocRead + "/TrainingCalender/";

                    var GetbannerDetails = DapperORM.DynamicQueryList($@"SELECT *  FROM Training_Calender 
                    WHERE ',' + DepartmentId + ',' LIKE '%,{Session["EmployeeDepartmentId"]},%' AND Deactivate = 0").ToList();
                    //var GetbannerDetails = results.Read<dynamic>().ToList();
                    var sourcePaths = new List<dynamic>();

                    foreach (var x in GetbannerDetails)
                    {
                        if (x?.BannerFile == null)
                            continue; // Skip if BannerFile is null

                        dynamic obj = new ExpandoObject();

                        obj.TrainingCalenderId = x.TrainingCalenderId;
                        obj.FullPath = $"{GetUrl.TrimEnd('/')}/{x.TrainingCalenderId}/Banner_{x.BannerFile}".Replace("\\", "/");

                        // Safely get extension only if DocumentPath is not null or empty
                        var docPath = x.DocumentPath as string;
                        obj.Extension = !string.IsNullOrWhiteSpace(docPath)
                            ? Path.GetExtension(docPath)?.TrimStart('.').ToUpper()
                            : string.Empty;

                        obj.TrainingCalenderId_Encrypted = x.TrainingCalenderId_Encrypted as string ?? string.Empty;

                        sourcePaths.Add(obj);
                    }

                    ViewBag.BannerImages = sourcePaths;

                    //var DocRead = DapperORM.DynamicQuerySingle("Select * from Tool_LMS_DownloadUpload_URL where DocOrigin='TrainingRead'");
                    //var GetDocRead = DocRead.DownloadUploadURL;
                    //var GetUrl = GetDocRead + "/TrainingCalender/";


                    //var results = DapperORM.DynamicQuerySingleMultiple($@"SELECT * 
                    //                                FROM Training_Calender 
                    //                                WHERE ',' + DepartmentId + ',' LIKE '%,{Session["EmployeeDepartmentId"]},%' and Deactivate=0");
                    //var GetbannerDetails = results.Read<dynamic>().ToList();
                    //var sourcePaths = new List<dynamic>();

                    //foreach (var x in GetbannerDetails.Where(x => x.BannerFile != null))
                    //{
                    //    dynamic obj = new ExpandoObject();
                    //    obj.TrainingCalenderId = x.TrainingCalenderId;
                    //    obj.FullPath = $"{GetUrl.TrimEnd('/')}/{x.TrainingCalenderId}/Banner_{x.BannerFile}".Replace("\\", "/");
                    //    obj.Extension = Path.GetExtension((string)x.DocumentPath)?.TrimStart('.').ToUpper();
                    //    obj.TrainingCalenderId_Encrypted = (string)x.TrainingCalenderId_Encrypted;

                    //    sourcePaths.Add(obj);
                    //}
                    //ViewBag.BannerImages = sourcePaths;

                    return View();
                    #endregion
                }
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            catch (Exception ex)
            {
                // Redirect to custom error page with error message
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetFile
        public ActionResult GetFile(int DocId,string DocOrigin)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                var GetPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='"+ DocOrigin + "'");
                var FisrtPath = GetPath?.DocInitialPath;
               
                var FullPath = "";
                dynamic SecondPath = "";
                if (DocOrigin== "Announcement")
                {
                     SecondPath = DapperORM.DynamicQuerySingle("Select FilePath from Event_Announcement where AnnouncementID='" + DocId + "'");
                    FullPath = FisrtPath + DocId + "\\" + SecondPath.FilePath;
                }
                else if (DocOrigin == "Event")
                {
                    SecondPath = DapperORM.DynamicQuerySingle("Select FilePath from Event_Event where EventID='" + DocId + "'");
                    FullPath = FisrtPath + DocId + "\\" + SecondPath.FilePath;
                }
                else if (DocOrigin == "News")
                {
                    SecondPath = DapperORM.DynamicQuerySingle("Select FilePath from Event_News where NewsID='" + DocId + "'");
                    FullPath = FisrtPath + DocId + "\\" + SecondPath.FilePath;
                }
                else if (DocOrigin == "RewardAndRecognition")
                {
                    SecondPath = DapperORM.DynamicQuerySingle("Select FilePath from Event_Reward where RewardId='" + DocId + "'");
                    FullPath = FisrtPath + DocId + "\\" + SecondPath.FilePath;
                }                
                var FilePath = "";
                string extension = "";

                    if (System.IO.File.Exists(FullPath))
                    {
                         extension = Path.GetExtension(FullPath).ToLower();

                        if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                        {
                            using (Image image = Image.FromFile(FullPath))
                            {
                                using (MemoryStream m = new MemoryStream())
                                {
                                    image.Save(m, image.RawFormat);
                                    byte[] imageBytes = m.ToArray();
                                    string base64String = Convert.ToBase64String(imageBytes);
                                    FilePath = "data:image;base64," + base64String; // ✅ image base64
                                }
                            }
                        }
                        else if (extension == ".pdf")
                        {
                            byte[] fileBytes = System.IO.File.ReadAllBytes(FullPath);
                            string base64String = Convert.ToBase64String(fileBytes);
                            FilePath = "data:application/pdf;base64," + base64String; // ✅ pdf base64
                        }
                    }                                                 
                return Json(new
                {
                    filePath = FilePath,
                    extension = extension
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion GetFile

        #region ViewForLeave
        public ActionResult ViewForLeave(string Origin, string Type, string TeamType, string Leave_Statistics_Type, string Upcomingbday, string NewJoineeType)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }


                if (Origin != null)
                {
                    DynamicParameters paramLeave = new DynamicParameters();
                    paramLeave.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramLeave.Add("@p_Origin", Origin);
                    paramLeave.Add("@p_Type", Type);
                    var GetLeavePendingList = DapperORM.ExecuteSP<dynamic>("SP_Dashboard_Requisition_Pending_List", paramLeave).ToList();
                    ViewBag.LeavePendingList = GetLeavePendingList;
                }

                //Today Team Status List
                if (TeamType != null)
                {
                    DynamicParameters TeamStatus = new DynamicParameters();
                    TeamStatus.Add("@p_Managerid", Session["EmployeeId"]);
                    TeamStatus.Add("@p_Type", TeamType);
                    var GetTodayTeamStatusList = DapperORM.ExecuteSP<dynamic>("SP_Dashboard_Today_Team_Status_List", TeamStatus).ToList();
                    ViewBag.LeavePendingList = GetTodayTeamStatusList;
                }

                //Leave Statistics List
                if (Leave_Statistics_Type != null)
                {
                    DynamicParameters LeaveStatistics = new DynamicParameters();
                    LeaveStatistics.Add("@p_EmployeeId", Session["EmployeeId"]);
                    LeaveStatistics.Add("@p_Leave_Statistics_Type", TeamType);
                    var GetLeaveStatistics = DapperORM.ExecuteSP<dynamic>("SP_Dashboard_Leave_Statistics_List", LeaveStatistics).ToList();
                    ViewBag.LeavePendingList = GetLeaveStatistics;
                }

                //MonthBirthdayTeamList
                if (Upcomingbday == "MonthBirthdayTeamList")
                {
                    DynamicParameters ParamMonthBirthdayTeamList = new DynamicParameters();
                    ParamMonthBirthdayTeamList.Add("@query", @"select EmployeeName, DepartmentName, DesignationName,'' as Description from mas_employee
                                                                inner join Mas_Employee_Personal on PersonalEmployeeId = mas_employee.EmployeeId
                                                                inner join Mas_Department on DepartmentId = mas_employee.EmployeeDepartmentID
                                                                inner join Mas_Designation on DesignationId =mas_employee.EmployeeDesignationID 
                                                                where Mas_Employee_Personal.PersonalEmployeeId=EmployeeId 
                                                                and mas_employee.deactivate=0 and employeeleft=0 
                                                                and ((ReportingManager1 =" + Session["EmployeeId"] + ") or (ReportingManager2 = " + Session["EmployeeId"] + ")) and month(BirthdayDate) =month(getdate()) and ContractorId=1"); /*and year(BirthdayDate) = year(getdate()*/
                    var MonthBirthdayTeamCount = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", ParamMonthBirthdayTeamList).ToList();
                    ViewBag.LeavePendingList = MonthBirthdayTeamCount;
                }
                //TodayBirthdayTeamList
                if (Upcomingbday == "TodayBirthdayTeamList")
                {
                    DynamicParameters ParamTodayBirthdayTeamList = new DynamicParameters();
                    ParamTodayBirthdayTeamList.Add("@query", @"select EmployeeName, DepartmentName, DesignationName,'' as Description from mas_employee
                                                                inner join Mas_Employee_Personal on PersonalEmployeeId = mas_employee.EmployeeId
                                                                inner join Mas_Department on DepartmentId = mas_employee.EmployeeDepartmentID
                                                                inner join Mas_Designation on DesignationId =mas_employee.EmployeeDesignationID 
                                                               where Mas_Employee_Personal.PersonalEmployeeId=EmployeeId 
                                                               and   mas_employee.deactivate=0 and employeeleft=0 
                                                               and ((ReportingManager1 =" + Session["EmployeeId"] + ") or (ReportingManager2 =" + Session["EmployeeId"] + ")) and day(BirthdayDate) =day(getdate()) and month(BirthdayDate) =month(getdate()) and ContractorId=1");
                    var TodayBirthdayTeamList = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", ParamTodayBirthdayTeamList).ToList();
                    ViewBag.LeavePendingList = TodayBirthdayTeamList;
                }
                //MonthBirthdayDepartmentList
                if (Upcomingbday == "MonthBirthdayDepartmentList")
                {
                    DynamicParameters ParamMonthBirthdayDepartmentList = new DynamicParameters();
                    ParamMonthBirthdayDepartmentList.Add("@query", @"select EmployeeName, DepartmentName, DesignationName,'' as Description from mas_employee
                                                                inner join Mas_Employee_Personal on PersonalEmployeeId = mas_employee.EmployeeId
                                                                inner join Mas_Department on DepartmentId = mas_employee.EmployeeDepartmentID
                                                                inner join Mas_Designation on DesignationId =mas_employee.EmployeeDesignationID 
                                                                     where Mas_Employee_Personal.PersonalEmployeeId=EmployeeId 
                                                                     and mas_employee.deactivate=0 and employeeleft=0 
                                                                     and EmployeeDepartmentid=(select e.Employeedepartmentid from mas_employee e where e.employeeid=" + Session["EmployeeId"] + ") and month(BirthdayDate) =month(getdate()) and ContractorId=1"); /*and year(BirthdayDate) = year(getdate()*/
                    var MonthBirthdayDepartmentList = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", ParamMonthBirthdayDepartmentList).ToList();
                    ViewBag.LeavePendingList = MonthBirthdayDepartmentList;
                }

                //TodayBirthdayDepartmentList
                if (Upcomingbday == "TodayBirthdayDepartmentList")
                {
                    DynamicParameters ParamTodayBirthdayDepartmentList = new DynamicParameters();
                    ParamTodayBirthdayDepartmentList.Add("@query", @"select EmployeeName, DepartmentName, DesignationName,'' as Description from mas_employee
                                                                inner join Mas_Employee_Personal on PersonalEmployeeId = mas_employee.EmployeeId
                                                                inner join Mas_Department on DepartmentId = mas_employee.EmployeeDepartmentID
                                                                inner join Mas_Designation on DesignationId =mas_employee.EmployeeDesignationID 
                                                                     where Mas_Employee_Personal.PersonalEmployeeId=EmployeeId 
                                                                     and   mas_employee.deactivate=0 and employeeleft=0 
                                                                     and EmployeeDepartmentid=(select e.Employeedepartmentid from mas_employee e where e.employeeid=" + Session["EmployeeId"] + ") and day(BirthdayDate) =day(getdate()) and month(BirthdayDate) =month(getdate()) and ContractorId=1");
                    var TodayBirthdayDepartmentList = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", ParamTodayBirthdayDepartmentList).ToList();
                    ViewBag.LeavePendingList = TodayBirthdayDepartmentList;
                }
                ////MonthBirthdayFavouriteCount
                //if (Upcomingbday != null)
                //{
                //    //MonthBirthdayFavouriteCount
                //}
                ////TodayBirthdayFavouriteCount
                //if (Leave_Statistics_Type != null)
                //{
                //    //TodayBirthdayFavouriteCount
                //}

                //TodayBirthdayMissedList
                if (Upcomingbday == "TodayBirthdayMissedList")
                {
                    DynamicParameters ParamTodayBirthdayMissedList = new DynamicParameters();
                    ParamTodayBirthdayMissedList.Add("@query", @"select EmployeeName, DepartmentName, DesignationName,'' as Description from mas_employee
                                                                inner join Mas_Employee_Personal on PersonalEmployeeId = mas_employee.EmployeeId
                                                                inner join Mas_Department on DepartmentId = mas_employee.EmployeeDepartmentID
                                                                inner join Mas_Designation on DesignationId =mas_employee.EmployeeDesignationID 
                                                                     where Mas_Employee_Personal.PersonalEmployeeId=EmployeeId 
                                                                     and   mas_employee.deactivate=0 and employeeleft=0 
                                                                     and ((ReportingManager1 =" + Session["EmployeeId"] + ") or (ReportingManager2 =" + Session["EmployeeId"] + ")) and day(BirthdayDate)<day(getdate()) and month(BirthdayDate) =month(getdate()) and ContractorId=1");
                    var TodayBirthdayMissedList = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", ParamTodayBirthdayMissedList).ToList();
                    ViewBag.LeavePendingList = TodayBirthdayMissedList;
                }
                if (NewJoineeType == "TeamList")
                {
                    DynamicParameters NewJoineeTeam = new DynamicParameters();
                    NewJoineeTeam.Add("@query", @"select EmployeeName, DepartmentName, DesignationName,'' as Description from mas_employee
                                                                 inner join Mas_Department on DepartmentId = mas_employee.EmployeeDepartmentID
                                                                 inner join Mas_Designation on DesignationId =mas_employee.EmployeeDesignationID 
                                                                 where mas_employee.deactivate=0 and employeeleft=0  
                                                                 and JoiningDate>=DATEADD (M, -1, GETDATE()) AND YEAR(JoiningDate) = YEAR(GETDATE()) and mas_employee.contractorid=1  
                                                                 and mas_employee.EmployeeBranchId=" + Session["BranchId"] + "and ((ReportingManager1 =" + Session["EmployeeId"] + ") or (ReportingManager2 =" + Session["EmployeeId"] + ")) ");
                    var NewJoineeTeamList = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", NewJoineeTeam).ToList();
                    ViewBag.LeavePendingList = NewJoineeTeamList;
                }

                if (NewJoineeType == "DepartmentList")
                {
                    DynamicParameters NewJoineeDepartment = new DynamicParameters();
                    NewJoineeDepartment.Add("@query", @"select EmployeeName, DepartmentName, DesignationName,'' as Description from mas_employee
                                                        inner join Mas_Department on DepartmentId = mas_employee.EmployeeDepartmentID
                                                        inner join Mas_Designation on DesignationId =mas_employee.EmployeeDesignationID 
                                                        where mas_employee.deactivate=0 and employeeleft=0 and contractorid=1
                                                        and JoiningDate>=DATEADD (M, -1, GETDATE()) AND YEAR(JoiningDate) = YEAR(GETDATE())
                                                        and EmployeeDepartmentid=(select e.Employeedepartmentid from mas_employee e where e.deactivate=0 and e.contractorid=1  and e.employeeid=" + Session["EmployeeId"] + " and e.EmployeeBranchId=" + Session["BranchId"] + ")");
                    var NewJoineeDepartmentList = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", NewJoineeDepartment).ToList();
                    ViewBag.LeavePendingList = NewJoineeDepartmentList;
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

        #region EmployeeGlobalSearch
        public ActionResult EmployeeGlobalSearch(string SearchText)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                //param.Add("@query", "select top(1) PlantHREmployeeID from Mas_PlantHR where PlantHREmployeeID=" + Session["EmployeeId"] + " and Deactivate=0");
                //var PlantHREmployeeID = DapperORM.ExecuteSP<EmployeeGlobalSearch>("sp_QueryExcution", param).ToList();

                //if (PlantHREmployeeID.Count != 0)
                //{
                //    DynamicParameters paramy = new DynamicParameters();
                //    paramy.Add("@query", "Select top(500) * from View_Global_EmployeeSearch where EmployeeNo like '%" + SearchText + "%' or EmployeeCardNo like '%" + SearchText + "%' or EmployeeName like '%" + SearchText + "%' or PrimaryMobileNo like '%" + SearchText + "%' or AadharNo like '%" + SearchText + "%' or PanNo like '%" + SearchText + "%'");
                //    var GetEmployeeGlobalSearch = DapperORM.ExecuteSP<EmployeeGlobalSearch>("sp_QueryExcution", paramy).ToList();
                //    return Json(GetEmployeeGlobalSearch, JsonRequestBehavior.AllowGet);
                //}

                param.Add("@query", "select top(1) EmployeeId from Search_Employee where EmployeeId=" + Session["EmployeeId"] + " and Deactivate=0");
                var EmployeeID = DapperORM.ExecuteSP<EmployeeGlobalSearch>("sp_QueryExcution", param).ToList();

                ViewBag.GetEmployeeData = null;
                if (EmployeeID.Count != 0)
                {
                    DynamicParameters paramy = new DynamicParameters();
                    paramy.Add("@query", "Select top(500) * from View_Global_EmployeeSearch where EmployeeNo like '%" + SearchText + "%' or EmployeeCardNo like '%" + SearchText + "%' or EmployeeName like '%" + SearchText + "%' or PrimaryMobileNo like '%" + SearchText + "%' or AadharNo like '%" + SearchText + "%' or PanNo like '%" + SearchText + "%'");
                    var GetEmployeeData = DapperORM.ExecuteSP<EmployeeGlobalSearch>("sp_QueryExcution", paramy).ToList();
                    ViewBag.GetEmployeeData = GetEmployeeData;
                    return Json(new { type = "IF", data = GetEmployeeData }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    DynamicParameters paramy = new DynamicParameters();
                    paramy.Add("@query", "Select top(500) * from View_Global_EmployeeSearch where EmployeeNo like '%" + SearchText + "%' or EmployeeCardNo like '%" + SearchText + "%' or EmployeeName like '%" + SearchText + "%' or PrimaryMobileNo like '%" + SearchText + "%' or AadharNo like '%" + SearchText + "%' or PanNo like '%" + SearchText + "%'");
                    var GetEmployeeGlobalSearch = DapperORM.ExecuteSP<EmployeeGlobalSearch>("sp_QueryExcution", paramy).ToList();
                    return Json(new { type = "ELSE", data = GetEmployeeGlobalSearch }, JsonRequestBehavior.AllowGet);
                }
                //return Json(new { data1 = GetEmployeeData, data2 = GetEmployeeGlobalSearch }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GlobalEmployeeDetails
        public ActionResult EmployeeDetails(int EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                //-----------------------------------------------------------------------------------//
                DynamicParameters MulQuery = new DynamicParameters();
                MulQuery.Add("@p_EmployeeId", EmployeeId);
                using (var multi = DapperORM.DynamicMultipleResultList("sp_Global_EmployeeSearch", MulQuery))
                {
                    ViewBag.PersonalDetails = multi.Read<dynamic>().FirstOrDefault();
                    ViewBag.CTCDetails = multi.Read<dynamic>().FirstOrDefault();
                    var IncrementDetailsList = multi.Read<dynamic>().ToList();
                    ViewBag.CurrentIncrement = IncrementDetailsList.FirstOrDefault();
                    ViewBag.IncrementDetailsList = IncrementDetailsList.Skip(1).ToList();
                    ViewBag.AttendanceDetails = multi.Read<dynamic>().FirstOrDefault();
                    ViewBag.AssetsDetails= multi.Read<dynamic>().ToList();
                    ViewBag.ChecklistDetails= multi.Read<dynamic>().ToList();
                }

                //-----------------------------------------------------------------------------------//
                var SecondPath = "";

                var path = DapperORM.DynamicQueryList("Select PhotoPath from Mas_Employee_Photo Where PhotoEmployeeId= " + EmployeeId + "").FirstOrDefault();
                if (path != null || path == "")
                {
                    try
                    {
                        ViewBag.AddUpdateTitle = "Update";
                        SecondPath = path.PhotoPath;
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Onboarding'");
                        var FisrtPath = GetDocPath.DocInitialPath + EmployeeId + "\\" + "Photo" + "\\";

                        string fullPath = "";
                        fullPath = FisrtPath + SecondPath;


                        string directoryPath = Path.GetDirectoryName(fullPath);
                        if (!Directory.Exists(directoryPath))
                        {
                            ViewBag.UploadPhoto = "";
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
                                    ViewBag.UploadPhoto = "data:image; base64," + base64String;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message != null)
                        {
                            ViewBag.UploadPhoto = "";
                        }
                    }
                }
                else
                {
                    ViewBag.UploadPhoto = "";
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

        public static string DetermineCompName(string IP)
        {
            IPAddress myIP = IPAddress.Parse(IP);
            IPHostEntry GetIPHost = Dns.GetHostEntry(myIP);
            List<string> compName = GetIPHost.HostName.ToString().Split('.').ToList();
            return compName.First();
        }

        public JsonResult GetLeaveNotifications()
        {
            DynamicParameters NotifyLeaveCount = new DynamicParameters();
            NotifyLeaveCount.Add("@p_EmployeeId", Session["EmployeeId"]);
            NotifyLeaveCount.Add("@p_Origin", "Approval");

            var LeavenotificationList = DapperORM.ExecuteSP<dynamic>("sp_List_ESS_Leave_Approval", NotifyLeaveCount).ToList();
            Session["Leave_NotificationCount"] = LeavenotificationList;
            return Json(LeavenotificationList, JsonRequestBehavior.AllowGet);
        }

        #region HolidayList
        public ActionResult HolidayList()
        {
            return View();
        }
        #endregion HolidayList
    }
}
