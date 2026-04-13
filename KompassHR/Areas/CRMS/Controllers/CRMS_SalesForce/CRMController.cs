
using Dapper;
using KompassHR.Models;
//using KompassHR.Models.helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
//using Rotativa;
//using Rotativa.MVC;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
//using KompassHR.Areas.Customer.Models;
using System.Net.Mail;
using OfficeOpenXml;
using System.Drawing;
using OfficeOpenXml.Style;
using KompassHR.Areas.CRMS.Models;
using System.Globalization;
using KompassHR.Controllers;
using System.Net.Sockets;
using KompassHR.Models;

//namespace OceanFM.Areas.CRM.Controllers
namespace KompassHR.Areas.CRMS.Controllers.CRMS_SalesForce
{
    public class CRMController : Controller
    {
        string MachineId = Dns.GetHostName();
        DynamicParameters param = new DynamicParameters();
        DynamicParameters param1 = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);


        private readonly string _clientIp;
        private readonly string _clientMachineName;

        public object HomeController { get; private set; }

        public CRMController()
        {
            _clientIp = System.Web.HttpContext.Current?.Request?.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(_clientIp))
            {
                _clientIp = System.Web.HttpContext.Current?.Request?.ServerVariables["REMOTE_ADDR"];
            }

            if (_clientIp == "::1") _clientIp = "G.R.P";

            _clientMachineName = System.Web.HttpContext.Current?.Request?.UserHostName;
        }

      

        public ActionResult CRM_Dashboard1(ACCMAIN mn)

        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // DapperORM.SetConnection();

                DateTime fdate = mn.Fdate;
              //  var mainRole = Session["MainRole"].ToString();
                var userId = Session["EmployeeId"].ToString();
                if (fdate == DateTime.MinValue)
                {
                    DateTime d = DateTime.Now;
                    mn.Fdate = d;
                    int monthNumber = d.Month;
                    int yearNumber = d.Year;
                    string formattedMonth = monthNumber.ToString("D2");
                    string mm = $"{formattedMonth}-{yearNumber}";
                    ViewBag.mm = mm;
                    TempData["m"] = mm;

                    param.Add("@query", $"Select sit.SiteName,sit.SiteCloseDate,acc.Encrypted_Id from Accsites sit join Accmain acc on sit.ACCMAIN_Fid = acc.Fid  where sit.Deleted = 0");
                    var Accsites = DapperORM.ExecuteSP<dynamic>("Sp_QueryExcution", param).ToList();

                    param.Add("@query", $"SELECT distinct acc.Encrypted_Id, acc.Fid,acc.SiteName,acc.SiteCloseDate FROM MAS_OPSSITES ops JOIN ACCSITES acc ON ops.ACCSITE_Fid = acc.Fid WHERE acc.SiteCloseDate >= GETDATE() AND acc.SiteCloseDate <= DATEADD(DAY, 30, GETDATE()) AND ops.Active = 1 AND ( ops.MAS_EMployee_Fid = '{userId}' )");
                    var sitesclosing30days = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.sitesclosing30days = sitesclosing30days;

                    param.Add("@query", $"SELECT distinct acc.Encrypted_Id, acc.Fid,acc.SiteName,acc.SiteCloseDate FROM MAS_OPSSITES ops JOIN ACCSITES acc ON ops.ACCSITE_Fid = acc.Fid  WHERE SiteCloseDate >= DATEADD(DAY, 30, GETDATE()) AND SiteCloseDate <= DATEADD(DAY, 60, GETDATE()) AND ops.Active = 1 AND ( ops.MAS_EMployee_Fid = '{userId}' )");
                    var sitesclosing60days = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.sitesclosing60days = sitesclosing60days;

                    param.Add("@query", $"SELECT distinct acc.Encrypted_Id, acc.Fid,acc.SiteName,acc.SiteCloseDate FROM MAS_OPSSITES ops JOIN ACCSITES acc ON ops.ACCSITE_Fid = acc.Fid  WHERE SiteCloseDate >= DATEADD(DAY, 60, GETDATE()) AND SiteCloseDate <= DATEADD(DAY, 90, GETDATE()) AND ops.Active = 1 AND ( ops.MAS_EMployee_Fid = '{userId}' );");
                    var sitesclosing90days = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.sitesclosing90days = sitesclosing90days;

                    //param.Add("@query", $"select coalesce(SUM(TotalContractValue),0) as TotalContractValue from ACCCONTRACT where Active = 1 and Deleted = 0 and ACCMAIN_Fid >0 and ContractEndDate >= Cast(GetDate() as date) and (UserId = {userId})");
                    param.Add("@query", $"SELECT COALESCE(SUM(TotalContractValue), 0) AS TotalContractValue FROM ACCCONTRACT WHERE Active = 1 AND Deleted = 0 AND ACCMAIN_Fid > 0 AND ContractEndDate < CAST(GETDATE() AS date) and (UserId = {userId})");
                    var TotalContractValue = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.TotalContractValue = TotalContractValue.TotalContractValue;

                    param.Add("@query", $"select coalesce(COUNT(Fid),0) as Fid from ACCCONTRACT where Active = 1 and Deleted = 0 and ACCMAIN_Fid >0 AND ContractEndDate < CAST(GETDATE() AS date) and (UserId = {userId}) ");
                    var TotalAccountCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.TotalAccountCount = TotalAccountCount.Fid;
                    if (TotalContractValue.TotalContractValue != null && TotalAccountCount.Fid != null && TotalAccountCount.Fid != 0)
                    {
                        ViewBag.AvgContractValue = (TotalContractValue.TotalContractValue / TotalAccountCount.Fid).ToString("#.##");
                    }
                    else
                    {
                        ViewBag.AvgContractValue = "0.00";
                    }
                    DynamicParameters param3 = new DynamicParameters();
                    param3.Add("@Month", monthNumber);
                    param3.Add("@Year", yearNumber);
                   // param3.Add("@mainRole", mainRole);
                    param3.Add("@UserId", userId);
                    var monthlyContractValue = DapperORM.ReturnList<dynamic>("SP_MonthWise_TCV", param3).FirstOrDefault();
                    ViewBag.monthlyContractValue = monthlyContractValue;
                    param.Add("@query", $"Select MAx(src.Fid) as srcFid,Count(CASE WHEN mas.Origin = 'Lead' and acc.Stage != 'Lost'  AND ( acc.UserId = '{userId}') THEN 1 END) as Count,src.LeadSource From Accmain acc Join MAS_LEADSOURCE src on src.Fid = acc.MAS_LEADSOURCE_Fid join MAs_stages mas on mas.fid = acc.MAS_STAGES_FID where acc.Deleted = 0 and Format(acc.Fdate,'MM-yyyy')='{mm}'   group by LeadSource");
                    var LeadSourcesCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.LeadSourcesCount = LeadSourcesCount;

                    param.Add("@query", $"Select MAx(src.Fid) as srcFid,Count(CASE WHEN mas.Origin = 'Prospect' and acc.Stage != 'Lost'  AND ( acc.UserId = {userId})THEN 1 END) as Count,src.LeadSource From Accmain acc Join MAS_LEADSOURCE src on src.Fid = acc.MAS_LEADSOURCE_Fid join MAs_stages mas on mas.fid = acc.MAS_STAGES_FID where acc.Deleted = 0 and Format(acc.Fdate,'MM-yyyy')='{mm}' group by LeadSource ");
                    var OpportunitySourcesCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.OpportunitySourcesCount = OpportunitySourcesCount;

                    param.Add("@query", $"Select MAx(src.Fid) as srcFid,Count(CASE WHEN mas.Origin = 'Account' and acc.Stage != 'Lost'  AND ( acc.UserId = {userId}) THEN 1 END) as Count,src.LeadSource From Accmain acc  Join MAS_LEADSOURCE src on src.Fid = acc.MAS_LEADSOURCE_Fid join MAs_stages mas on mas.fid = acc.MAS_STAGES_FID where acc.Deleted = 0  and Format(acc.Fdate,'MM-yyyy')='{mm}' group by  LeadSource ");
                    var AccountSourcesCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.AccountSourcesCount = AccountSourcesCount;

                    //param.Add("@query", $"Select Max(ct.Flag) as country,MAX(st.Fid) as State_Fid ,st.StateName,Count(*) as Count from ACCLocation loc join accMAin acc on acc.Fid = loc.Accmain_Fid and acc.Stage != 'Lost' join MAS_STATE st on st.Fid = loc.MAS_STATE_Fid join MAS_COUNTRY ct on ct.Fid = st.MAS_COUNTRY_Fid where loc.Deleted = 0 and (acc.UserId = {userId}) group by st.StateName");
                    param.Add("@query", $"Select MAX(st.StateId) as State_Fid ,st.StateName, Count(*) as Count from ACCLocation loc join accMAin acc on acc.Fid = loc.Accmain_Fid and acc.Stage != 'Lost' join Mas_States st on st.StateId = loc.MAS_STATE_Fid join Mas_Country ct on ct.CountryId = st.CountryId where loc.Deleted = 0 and acc.UserId = { userId} group by st.StateName");
                    var StateList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.StateList = StateList;

                    param.Add("@query", $"Select top 3 Sum(ctr.TotalContractValue) as TCV,ctr.ACCMAIN_Fid,Max(acc.CompanyName) as CompanyName from ACCCONTRACT ctr join ACCMAIN acc on acc.Fid = ctr.ACCMAIN_Fid and acc.Deleted = 0 where ctr.Deleted = 0 and ( acc.UserId = {userId}) and Month(ctr.fdate) = {monthNumber} and year(ctr.Fdate) = {yearNumber}  group by ctr.ACCMAIN_Fid order by TCV desc");
                    var topContracts = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.topContracts = topContracts;

                    //param.Add("@query", $"select Distinct mgmt.MAS_Employee_Fid  from ATD_DAYMGMT as mgmt join MAS_EMPLOYEES as emp on mgmt.MAS_Employee_Fid=emp.Fid where CONVERT(Date,mgmt.InDate)= CONVERT(Date,GETDATE()) and mgmt.Deleted=0 and emp.EmployeeCategory='Sales'");
                    param.Add("@query", $"select Distinct mgmt.CheckInOutEmployeeId  from Atten_CheckInOut as mgmt join Mas_Employee as emp on mgmt.CheckInOutEmployeeId = emp.EmployeeId where CONVERT(Date, mgmt.CheckInOutDateTime) = CONVERT(Date, GETDATE()) and mgmt.Deactivate = 0 ");
                    var presentPeopleList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    int presentPeoples = presentPeopleList.Count;
                    ViewBag.PresentPeoples = presentPeoples;
                    param.Add("@query", $"select * from Mas_Employee where Deactivate=0 AND EmployeeLeft=0 order By EmployeeName");
                    var AbsentCountList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    int AbsentCount = AbsentCountList.Count;
                    ViewBag.AbsentCount = (AbsentCount - presentPeoples);
                  //  param.Add("@query", $"select Distinct atd.MAS_Employee_Fid, atd.ACCSITES_Fid ,atd.InDate from ATD_OPSATD as atd join MAS_EMPLOYEES as  emp on emp.Fid=atd.MAS_Employee_Fid where atd.ACCSITES_Fid IS NOT NULL and atd.Deleted=0 and emp.EmployeeCategory='Sales' and TRY_CONVERT(Date,atd.InDate)= CONVERT(Date,GETDATE())");
                  //  var totalSitevisitList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                  //  int totalSitevisit = totalSitevisitList.Count;
                  //  ViewBag.totalSitevisit = totalSitevisit;

                    DynamicParameters param4 = new DynamicParameters();
              //    param4.Add("@mainRole", mainRole);
                    param4.Add("@UserId", userId);
                    var countData = DapperORM.ReturnList<dynamic>("Sp_last30days_count", param4).FirstOrDefault();
                    ViewBag.LeadCount = countData.LeadCount;
                    ViewBag.opportunityCount = countData.ProspectCount;
                    ViewBag.accountCount = countData.AccountCount;

                    param1.Add("@p_month", mm);
                  //  param1.Add("@mainRole", mainRole);
                    param1.Add("@UserId", userId);
                    var data = DapperORM.ReturnList<dynamic>("Sp_LeadToProp_ConvRate", param1).FirstOrDefault();
                    ViewBag.ConvRate = data;

                    DynamicParameters param2 = new DynamicParameters();
                    string date = $"01-{mm}";
                    var convDate = Convert.ToDateTime(date);

                    param2.Add("@p_StartDate", new DateTime(DateTime.Now.AddMonths(-5).Year, DateTime.Now.AddMonths(-5).Month, 1));
                    param2.Add("@p_EndDate", DateTime.Today);
                  //  param2.Add("@p_mainRole", mainRole);
                    param2.Add("@p_UserId", userId);
                    var graphData = DapperORM.ReturnList<dynamic>("SP_OPP_Acc_Count", param2).ToList();
                    ViewBag.Data = graphData;


                    DynamicParameters param5 = new DynamicParameters();
                 //   param5.Add("@mainRole", mainRole);
                    param5.Add("@UserId", userId);
                    var saleByserCatg = DapperORM.ReturnList<dynamic>("Sp_SaleBySerCatg", param5).ToList();
                    ViewBag.saleByserCatg = saleByserCatg;

                    param.Add("@query", $"select Count(Fid) AS InvoiceCount from ACCINVOICE where MONTH(BillMonth) = MONTH(GETDATE()) and YEAR(BillMonth) = YEAR(GETDATE())AND( UserId = {userId})AND Deleted = 0");
                    var InvoiceCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.InvoiceCount = InvoiceCount;

                    param.Add("@query", $"select Count(*) AS TotalCount from ACCINVOICE where Deleted = 0;");
                    var InvoiceCountInvoice = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.InvoiceCountInvoice = InvoiceCountInvoice.TotalCount;
                }
                else
                {

                    param.Add("@query", $"select Count(Fid) AS InvoiceCount from ACCINVOICE where MONTH(BillMonth) = MONTH(GETDATE()) and YEAR(BillMonth) = YEAR(GETDATE())AND( UserId = {userId})AND Deleted = 0");
                    var InvoiceCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.InvoiceCount = InvoiceCount;

                    param.Add("@query", $"select Count(*) AS TotalCount from ACCINVOICE where Deleted = 0;");
                    var InvoiceCountInvoice = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.InvoiceCountInvoice = InvoiceCountInvoice.TotalCount;

                    param.Add("@query", $"Select sit.SiteName,sit.SiteCloseDate,acc.Encrypted_Id from Accsites sit join Accmain acc on sit.ACCMAIN_Fid = acc.Fid  where sit.Deleted = 0");
                    var Accsites = DapperORM.ReturnList<ACCSITES>("Sp_QueryExcution", param).ToList();
                    ViewBag.sitesclosing30days = Accsites.Where(x => x.SiteCloseDate >= DateTime.Now && x.SiteCloseDate <= DateTime.Now.AddDays(30)).ToList();
                    ViewBag.sitesclosing60days = Accsites.Where(x => x.SiteCloseDate >= DateTime.Now.AddDays(30) && x.SiteCloseDate <= DateTime.Now.AddDays(60)).ToList();
                    ViewBag.sitesclosing90days = Accsites.Where(x => x.SiteCloseDate >= DateTime.Now.AddDays(60) && x.SiteCloseDate <= DateTime.Now.AddDays(90)).ToList();
                    DateTime m = Convert.ToDateTime(mn.Fdate);

                    int monthNumber = m.Month;
                    int yearNumber = m.Year;
                    string formattedMonth = monthNumber.ToString("D2");
                    string mm = $"{formattedMonth}-{yearNumber}";

                    ViewBag.mm = mm;
                    TempData["m"] = mm;

                    param.Add("@query", $"select coalesce(SUM(TotalContractValue),0) as TotalContractValue from ACCCONTRACT where Active = 1 and Deleted = 0 and ACCMAIN_Fid >0 and ContractEndDate >= Cast(GetDate() as date) and ( UserId = {userId})");
                    var TotalContractValue = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.TotalContractValue = TotalContractValue.TotalContractValue;

                    DynamicParameters param3 = new DynamicParameters();
                    param3.Add("@Month", monthNumber);
                    param3.Add("@Year", yearNumber);
                  // param3.Add("@mainRole", mainRole);
                    param3.Add("@UserId", userId);
                    var monthlyContractValue = DapperORM.ReturnList<dynamic>("SP_MonthWise_TCV", param3).FirstOrDefault();
                    ViewBag.monthlyContractValue = monthlyContractValue;
                    param.Add("@query", $"select coalesce(COUNT(Fid),0) as Fid from ACCCONTRACT where Active = 1 and Deleted = 0 and ACCMAIN_Fid >0 and ContractEndDate >= Cast(GetDate() as date) and (UserId = {userId}) ");
                    var TotalAccountCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.TotalAccountCount = TotalAccountCount.Fid;
                    if (TotalContractValue.TotalContractValue != null && TotalAccountCount.Fid != null && TotalAccountCount.Fid != 0)
                    {
                        ViewBag.AvgContractValue = (TotalContractValue.TotalContractValue / TotalAccountCount.Fid).ToString("#.##");
                    }
                    else
                    {
                        ViewBag.AvgContractValue = "0.00";
                    }

                    param.Add("@query", $"Select MAx(src.Fid) as srcFid,Count(*) as Count,src.LeadSource From Accmain acc Join MAS_LEADSOURCE src on src.Fid = acc.MAS_LEADSOURCE_Fid where MAS_STAGES_FID Between 1 and 4 and (acc.UserId = {userId}) group by LeadSource");
                    var LeadSourcesCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.LeadSourcesCount = LeadSourcesCount;

                    param.Add("@query", $"Select MAx(src.Fid) as srcFid,Count(*) as Count,src.LeadSource From Accmain acc Join MAS_LEADSOURCE src on src.Fid = acc.MAS_LEADSOURCE_Fid where MAS_STAGES_FID Between 5 and 8 and ( acc.UserId = {userId}) group by LeadSource ");
                    var OpportunitySourcesCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.OpportunitySourcesCount = OpportunitySourcesCount;

                    param.Add("@query", $"Select MAx(src.Fid) as srcFid,Count(*) as Count,src.LeadSource From Accmain acc Join MAS_LEADSOURCE src on src.Fid = acc.MAS_LEADSOURCE_Fid where MAS_STAGES_FID = 11 and ( acc.UserId = {userId}) group by   LeadSource");
                    var AccountSourcesCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.AccountSourcesCount = AccountSourcesCount;

                   // param.Add("@query", $"select Distinct mgmt.MAS_Employee_Fid  from ATD_DAYMGMT as mgmt join MAS_EMPLOYEES as emp on mgmt.MAS_Employee_Fid=emp.Fid where CONVERT(Date,mgmt.InDate)= CONVERT(Date,GETDATE()) and mgmt.Deleted=0 and emp.EmployeeCategory='Sales'");
                    param.Add("@query", $"select Distinct mgmt.CheckInOutEmployeeId  from Atten_CheckInOut as mgmt join Mas_Employee as emp on mgmt.CheckInOutEmployeeId = emp.EmployeeId where CONVERT(Date, mgmt.CheckInOutDateTime) = CONVERT(Date, GETDATE()) and mgmt.Deactivate = 0 ");
                    var presentPeopleList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    int presentPeoples = presentPeopleList.Count;
                    ViewBag.PresentPeoples = presentPeoples;

                    // param.Add("@query", $"select * from MAS_EMPLOYEES where EmployeeCategory='Sales'");
                    param.Add("@query", $"select * from Mas_Employee where Deactivate=0 AND EmployeeLeft=0 order By EmployeeName");
                    var AbsentCountList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    int AbsentCount = AbsentCountList.Count;
                    ViewBag.AbsentCount = (AbsentCount - presentPeoples);

                    //param.Add("@query", $"select Distinct atd.MAS_Employee_Fid, atd.ACCSITES_Fid ,atd.InDate from ATD_OPSATD as atd join MAS_EMPLOYEES as  emp on emp.Fid=atd.MAS_Employee_Fid where atd.ACCSITES_Fid IS NOT NULL and atd.Deleted=0 and emp.EmployeeCategory='Sales' and TRY_CONVERT(Date,atd.InDate)= CONVERT(Date,GETDATE())");
                    //var totalSitevisitList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    //int totalSitevisit = totalSitevisitList.Count;
                    //ViewBag.totalSitevisit = totalSitevisit;

                    param1.Add("@p_month", mm);
                    // param1.Add("@mainRole", mainRole);
                    param1.Add("@UserId", userId);
                    var data = DapperORM.ReturnList<dynamic>("Sp_LeadToProp_ConvRate", param1).FirstOrDefault();
                    ViewBag.ConvRate = data;

                    param.Add("@query", $"Select top 3 Sum(ctr.TotalContractValue) as TCV,ctr.ACCMAIN_Fid,Max(acc.CompanyName) as CompanyName from ACCCONTRACT ctr join ACCMAIN acc on acc.Fid = ctr.ACCMAIN_Fid where ctr.Deleted = 0 and (acc.UserId = {userId}) and Month(ctr.fdate) = {monthNumber} and year(ctr.Fdate) = {yearNumber}  group by ctr.ACCMAIN_Fid order by TCV desc");
                    var topContracts = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.topContracts = topContracts;

                    DynamicParameters param4 = new DynamicParameters();
                   // param4.Add("@mainRole", mainRole);
                    param4.Add("@UserId", userId);
                    var countData = DapperORM.ReturnList<dynamic>("Sp_last30days_count", param4).FirstOrDefault();
                    ViewBag.LeadCount = countData.LeadCount;
                    ViewBag.opportunityCount = countData.ProspectCount;
                    ViewBag.accountCount = countData.AccountCount;

                    DynamicParameters param2 = new DynamicParameters();
                    string date = $"01-{mm}";
                    var convDate = Convert.ToDateTime(date);
                    param2.Add("@p_StartDate", new DateTime(DateTime.Now.AddMonths(-5).Year, DateTime.Now.AddMonths(-5).Month, 1));
                    param2.Add("@p_EndDate", DateTime.Today);
                   // param2.Add("@p_mainRole", mainRole);
                    param2.Add("@p_UserId", userId);
                    var graphData = DapperORM.ReturnList<dynamic>("SP_OPP_Acc_Count", param2).ToList();
                    ViewBag.Data = graphData;

                    // param.Add("@query", $"Select Max(ct.Flag) as country,MAX(st.Fid) as State_Fid ,st.StateName,Count(*) as Count from ACCLocation loc join accMAin acc on acc.Fid = loc.Accmain_Fid and acc.Stage != 'Lost' join MAS_STATE st on st.Fid = loc.MAS_STATE_Fid join MAS_COUNTRY ct on ct.Fid = st.MAS_COUNTRY_Fid where loc.Deleted = 0 and ( acc.UserId = {userId}) group by st.StateName");
                    param.Add("@query", $"Select MAX(st.StateId) as State_Fid ,st.StateName, Count(*) as Count from ACCLocation loc join accMAin acc on acc.Fid = loc.Accmain_Fid and acc.Stage != 'Lost' join Mas_States st on st.StateId = loc.MAS_STATE_Fid join Mas_Country ct on ct.CountryId = st.CountryId where loc.Deleted = 0 and acc.UserId = { userId} group by st.StateName");
                    var StateList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.StateList = StateList;

                    DynamicParameters param5 = new DynamicParameters();
                   // param5.Add("@mainRole", mainRole);
                    param5.Add("@UserId", userId);
                    var saleByserCatg = DapperORM.ReturnList<dynamic>("Sp_SaleBySerCatg", param5).ToList();
                    ViewBag.saleByserCatg = saleByserCatg;
                }

                 LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "Dashboard", "CRM_Dashboard1", "CRM_Dashboard1", "View", "Passed", "", _clientIp, _clientMachineName);

                return View(mn);
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "Dashboard", "CRM_Dashboard1", "CRM_Dashboard1", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }

        }


        public ActionResult LocationViseInfo(MAS_STATE state)
        {
            try
            {
                var mainRole = Session["MainRole"].ToString();
                var userId = Session["EmployeeId"].ToString();

                param.Add("@query", "select Fid, StateName from MAS_STATE where Deleted = 0");
                var StateList = DapperORM.ReturnList<MAS_STATE>("Sp_QueryExcution", param).ToList();
                ViewBag.StateList = StateList;

                param.Add("@query", "select Distinct Origin from MAS_STAGES where Deleted = 0");
                var OriginList = DapperORM.ReturnList<MAS_STAGES>("Sp_QueryExcution", param).ToList();
                ViewBag.OriginList = OriginList;

                if (state.Origin == null)
                {
                    param.Add("@query", $"select t3.EmployeeName, t4.Origin, t1.* from ACCMAIN t1 left join USERS t2 on t1.UserId=t2.Fid and t2.Deleted = 0 left join MAS_EMPLOYEES t3 on t2.EmployeeId = t3.Fid and t3.Deleted = 0 join MAS_STAGES t4 on t1.MAS_STAGES_FID = t4.Fid and t4.Deleted = 0 and t4.Stage !='Lost' join Acclocation t5 on t1.fid = t5.Accmain_Fid and t5.deleted=0 join MAS_STATE t6 on t5.Mas_State_Fid = t6.Fid and t6.Deleted=0 and t6.Fid = '{state.Fid}' where t1.Deleted = 0 and ('admin' = '{mainRole}' or 'salesadmin' = '{mainRole}'or t1.UserId = {userId})");
                    var InfoList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.InfoList = InfoList;
                }
                else if (state.Origin == "Lead")
                {
                    param.Add("@query", $"select t3.EmployeeName, t4.Origin, t1.* from ACCMAIN t1 left join USERS t2 on t1.UserId=t2.Fid and t2.Deleted = 0 left join MAS_EMPLOYEES t3 on t2.EmployeeId = t3.Fid and t3.Deleted = 0 join MAS_STAGES t4 on t1.MAS_STAGES_FID = t4.Fid and t4.Deleted = 0 and t4.Origin = 'Lead' and t4.Stage !='Lost' join Acclocation t5 on t1.fid = t5.Accmain_Fid and t5.deleted=0 join MAS_STATE t6 on t5.Mas_State_Fid = t6.Fid and t6.Deleted=0 and t6.Fid = '{state.Fid}' where t1.Deleted = 0 and ('admin' = '{mainRole}' or 'salesadmin' = '{mainRole}'or t1.UserId = {userId})");
                    var InfoList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.InfoList = InfoList;
                }
                else if (state.Origin == "Prospect")
                {
                    param.Add("@query", $"select t3.EmployeeName, t4.Origin, t1.* from ACCMAIN t1 left join USERS t2 on t1.UserId=t2.Fid and t2.Deleted = 0 left join MAS_EMPLOYEES t3 on t2.EmployeeId = t3.Fid and t3.Deleted = 0 join MAS_STAGES t4 on t1.MAS_STAGES_FID = t4.Fid and t4.Deleted = 0 and t4.Origin = 'Prospect' and t4.Stage !='Lost' join Acclocation t5 on t1.fid = t5.Accmain_Fid and t5.deleted=0 join MAS_STATE t6 on t5.Mas_State_Fid = t6.Fid and t6.Deleted=0 and t6.Fid = '{state.Fid}' where t1.Deleted = 0 and ('admin' = '{mainRole}' or 'salesadmin' = '{mainRole}'or t1.UserId = {userId})");
                    var InfoList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.InfoList = InfoList;
                }
                else
                {
                    param.Add("@query", $"select t3.EmployeeName, t4.Origin, t1.* from ACCMAIN t1 left join USERS t2 on t1.UserId=t2.Fid and t2.Deleted = 0 left join MAS_EMPLOYEES t3 on t2.EmployeeId = t3.Fid and t3.Deleted = 0 join MAS_STAGES t4 on t1.MAS_STAGES_FID = t4.Fid and t4.Deleted = 0 and t4.Origin = 'Account' and t4.Stage not in ('Lost','Closed') join Acclocation t5 on t1.fid = t5.Accmain_Fid and t5.deleted=0 join MAS_STATE t6 on t5.Mas_State_Fid = t6.Fid and t6.Deleted=0 and t6.Fid = '{state.Fid}' where t1.Deleted = 0 and ('admin' = '{mainRole}' or 'salesadmin' = '{mainRole}'or t1.UserId = {userId})");
                    var InfoList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.InfoList = InfoList;
                }
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "LocationViseInfo", "LocationViseInfo", "View", "Passed", "", _clientIp, _clientMachineName);

                return View(state);
            }
            catch (Exception ex)
            {
               LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "LocationViseInfo", "LocationViseInfo", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }


        public ActionResult LeadDashboard(ACCMAIN mn)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                //DapperORM.SetConnection();

                DateTime fdate = mn.Fdate; // Assuming mn.Fdate is of type DateTime
              //  var mainRole = Session["MainRole"].ToString();
                var EmpID = Session["EmployeeId"].ToString();
                if (fdate == DateTime.MinValue)
                {
                    DateTime d = DateTime.Now;
                    mn.Fdate = d;// Gets the current date and time
                    int monthNumber = d.Month;
                    int yearNumber = d.Year;
                    string formattedMonth = monthNumber.ToString("D2");
                    string mm = $"{formattedMonth}-{yearNumber}";
                    ViewBag.mm = mm;
                    TempData["m"] = mm;


                    param.Add("@query", $"SELECT TOP 10 a.*,(SELECT COUNT(*)FROM ACCMAIN b WHERE b.fdate = a.fdate AND(b.UserId = { Convert.ToInt32(Session["EmployeeId"])})AND b.MAS_Stages_Fid BETWEEN 1 AND 4) AS fdateCount FROM ACCMAIN a WHERE ( a.UserId = { Convert.ToInt32(Session["EmployeeId"])}) AND a.MAS_Stages_Fid BETWEEN 1 AND 4 ORDER BY a.fid DESC;");
                    var LeadList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.LeadList = LeadList;

                    param.Add("@query", $"select top 10 ac.CompanyName,ac.ExpectedContractValue from ACCMAIN as ac join MAS_STAGES as stg on ac.MAS_STAGES_FID=stg.Fid where Format(ac.Fdate,'MM-yyyy')='{mm}' and ac.ExpectedContractValue Is not null  and stg.Stage Not in('Lost','Won') and stg.Deleted=0 and ac.Deleted=0 order by ac.ExpectedContractValue desc");
                    var top10ECV = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.top10ECV = top10ECV;

                    param.Add("@query", $"select ac.LeadPriority,count(ac.Fid) as Lead from ACCMAIN as ac join  MAS_STAGES as st on ac.MAS_STAGES_FID=st.Fid where  ac.LeadPriority Is Not Null and ac.LeadPriority !='' and  st.Stage not in('Lost') and st.Origin='Lead' and ac.Deleted=0 and (ac.UserId = {Convert.ToInt32(Session["EmployeeId"])})and FORMAT(ac.Fdate,'MM-yyyy')='{mm}' group by ac.LeadPriority");
                    var LeadPriorityData = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.LeadPriorityData = LeadPriorityData;

                    param.Add("@query", $"select ac.CompanyCategory ,COUNT(ac.Fid) as LeadCount from ACCMAIN  as ac join MAS_STAGES as st on ac.MAS_STAGES_FID=st.Fid where ac.CompanyCategory Is Not Null and st.Stage not in('Lost') and st.Origin='Lead' and ac.Deleted=0 and (ac.UserId = {Convert.ToInt32(Session["EmployeeId"])})and FORMAT(ac.Fdate,'MM-yyyy')='{mm}' group by ac.CompanyCategory");
                    var CategoryWise = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.CategoryWise = CategoryWise;


                    //if (mainRole == "admin")
                    if (Session["EmployeeId"] != null)
                    {
                        param.Add("@query", $"SELECT FORMAT(ac.Fdate, 'yyyy-MM') AS Period,ISNULL(SUM(CASE WHEN ms.Origin = 'Lead' THEN ac.ExpectedContractValue ELSE 0 END),0) AS Lead,ISNULL(SUM(CASE WHEN ms.Origin = 'Prospect' THEN ac.ExpectedContractValue ELSE 0 END),0) AS Opportunity,ISNULL(SUM(CASE WHEN ms.Origin = 'Account' THEN ac.ExpectedContractValue ELSE 0 END),0) AS Account FROM ACCMAIN ac JOIN MAS_STAGES ms ON ac.MAS_STAGES_FID = ms.Fid WHERE Format(ac.Fdate,'MM-yyyy')='{mm}' and ac.Fid not in (select ACCMAIN_Fid from ACCCONTRACT where Active=1 and Deleted=0) AND ms.Origin IN ('Lead', 'Prospect','Account') AND ac.Fdate IS NOT NULL GROUP BY FORMAT(ac.Fdate, 'yyyy-MM')ORDER BY FORMAT(ac.Fdate, 'yyyy-MM')");
                        var HotPriorityCount1 = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                        if (HotPriorityCount1 != null)
                        {
                            var leadValue = HotPriorityCount1.Lead;
                            var opportunityValue = HotPriorityCount1.Opportunity;
                            var AccountValue = HotPriorityCount1.Account;
                            var periodValue = HotPriorityCount1.Period;

                            ViewBag.LeadValue = leadValue;
                            ViewBag.OpportunityValue = opportunityValue;
                            ViewBag.AccountValue = HotPriorityCount1.Account;
                            ViewBag.PeriodValue = periodValue;
                            ViewBag.NewOrder = Convert.ToInt32(leadValue + opportunityValue + AccountValue);
                        }
                        else
                        {
                            ViewBag.LeadValue = 0;
                            ViewBag.OpportunityValue = 0;
                            ViewBag.AccountValue = 0;
                            ViewBag.PeriodValue = "";
                            ViewBag.NewOrder = 0;
                        }
                    }
                    else
                    {
                        param.Add("@query", $"SELECT FORMAT(ac.Fdate, 'yyyy-MM') AS Period,ISNULL(SUM(CASE WHEN ms.Origin = 'Lead' THEN ac.ExpectedContractValue ELSE 0 END),0) AS Lead,ISNULL(SUM(CASE WHEN ms.Origin = 'Prospect' THEN ac.ExpectedContractValue ELSE 0 END),0) AS Opportunity,ISNULL(SUM(CASE WHEN ms.Origin = 'Account' THEN ac.ExpectedContractValue ELSE 0 END),0) AS Account FROM ACCMAIN ac JOIN MAS_STAGES ms ON ac.MAS_STAGES_FID = ms.Fid WHERE Format(ac.Fdate,'MM-yyyy')='{mm}' and ac.UserId='" + Convert.ToInt32(Session["EmployeeId"]) + "' and ac.Fid not in (select ACCMAIN_Fid from ACCCONTRACT where Active=1 and Deleted=0) AND ms.Origin IN ('Lead', 'Prospect','Account') AND ac.Fdate IS NOT NULL GROUP BY FORMAT(ac.Fdate, 'yyyy-MM')ORDER BY FORMAT(ac.Fdate, 'yyyy-MM')");
                        var HotPriorityCount1 = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                        if (HotPriorityCount1 != null)
                        {
                            var leadValue = HotPriorityCount1.Lead;
                            var opportunityValue = HotPriorityCount1.Opportunity;
                            var AccountValue = HotPriorityCount1.Account;
                            var periodValue = HotPriorityCount1.Period;

                            ViewBag.LeadValue = leadValue;
                            ViewBag.OpportunityValue = opportunityValue;
                            ViewBag.AccountValue = HotPriorityCount1.Account;
                            ViewBag.PeriodValue = periodValue;
                            ViewBag.NewOrder = Convert.ToInt32(leadValue + opportunityValue + AccountValue);
                        }
                        else
                        {
                            ViewBag.LeadValue = 0;
                            ViewBag.OpportunityValue = 0;
                            ViewBag.AccountValue = 0;
                            ViewBag.PeriodValue = "";
                            ViewBag.NewOrder = 0;
                        }
                    }

                    param.Add("@query", $"SELECT COUNT(CASE WHEN acc.LeadPriority = 'Hot' THEN 1 END) AS HotPriorityCount, COUNT(CASE WHEN acc.LeadPriority = 'Warm' THEN 1 END) AS WarmPriorityCount, COUNT(CASE WHEN acc.LeadPriority = 'Cold' THEN 1 END) AS ColdPriorityCount FROM AccMain acc JOIN MAS_STAGES stages ON stages.Fid = acc.MAS_STAGES_FID AND stages.Deleted = 0 WHERE stages.Origin = 'Lead' AND acc.Stage != 'Lost' AND acc.LeadPriority IN('Hot', 'Cold', 'Warm') AND acc.Deleted = 0 AND acc.UserId IN(SELECT EmployeeId FROM Tool_UserLogin WHERE EmployeeId IN(SELECT EmployeeId FROM Mas_Employee WHERE EmployeeId = { EmpID})) and Format(acc.Fdate,'MM-yyyy') = '{mm}'");
                    var HotPriorityCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();

                    ViewBag.HotPriorityCount = HotPriorityCount ?? new
                    {
                        HotPriorityCount = 0,
                        ColdPriorityCount = 0,
                        WarmPriorityCount = 0
                    };

                    param.Add("@query", $"Select Count(acc.Fid) as TotalHotPriorityCount from AccMain acc join MAS_STAGES stages on stages.Fid = acc.MAS_STAGES_FID and stages.Deleted = 0 where stages.Origin = 'Lead' and acc.Stage != 'Lost' and acc.LeadPriority = 'Hot' and acc.Deleted = 0 and (acc.UserId = {Convert.ToInt32(Session["EmployeeId"])})");
                    var TotalHotPriorityCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.TotalHotPriorityCount = TotalHotPriorityCount.TotalHotPriorityCount;

                    param.Add("@query", $"select count(t1.Fid) as count from ACCMAIN t1 join MAS_STAGES t2 on t2.Fid = t1.MAS_STAGES_FID and t2.Origin ='Lead' and t2.Stage != 'Lost'  where t1.deleted = 0 and ( t1.UserId = {Convert.ToInt32(Session["EmployeeId"])}) and Format(t1.Fdate,'MM-yyyy') = '{mm}'");
                    var crntmontLead = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.crntmontLead = crntmontLead.count;

                    param.Add("@query", $"select count(t1.Fid) as count from ACCMAIN t1 join MAS_STAGES t2 on t2.Fid = t1.MAS_STAGES_FID and t2.Origin ='Lead' and t2.Stage != 'Lost' where t1.deleted = 0 and ( t1.UserId = {Convert.ToInt32(Session["EmployeeId"])}) ");
                    var ActiveLeads = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.ActiveLeads = ActiveLeads.count;


                    param.Add("@query", $@"SELECT  COUNT(*) Count FROM ACCOWNERS WHERE Deleted = 0  AND TranferBy_Fid IS NOT NULL and  (LeadOwner_Fid = {Convert.ToInt32(Session["EmployeeId"])})and Format(Fdate,'MM-yyyy') = '{mm}'");
                    var CurrentMonthTransferLeads = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.CurrentMonthTransferLeads = CurrentMonthTransferLeads.Count;


                    param.Add("@query", $@"SELECT  COUNT(*) Count FROM ACCOWNERS WHERE Deleted = 0  AND TranferBy_Fid IS NOT NULL and  ( LeadOwner_Fid = {Convert.ToInt32(Session["EmployeeId"])})");
                    var TotalTransferLeads = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.TotalTransferLeads = TotalTransferLeads.Count;


                    //param.Add("@query", $@"select ISNULL(sum(ISNULL(ExpectedContractValue,0)),0) AS ValueCount  from ACCMAIN left join Tool_UserLogin on users.EmployeeId = ACCMAIN.UserId  WHERE ACCMAIN.Deleted = 0  AND  ( users.EmployeeId = {Convert.ToInt32(Session["EmployeeId"])})and Format(ACCMAIN.Fdate,'MM-yyyy') = '{mm}'");
                    param.Add("@query", $@"SELECT ISNULL(SUM(ISNULL(ACCMAIN.ExpectedContractValue,0)),0) AS ValueCount FROM ACCMAIN LEFT JOIN Tool_UserLogin AS usr ON usr.EmployeeId = ACCMAIN.UserId WHERE ACCMAIN.Deleted = 0 AND usr.EmployeeId = {Convert.ToInt32(Session["EmployeeId"])}AND FORMAT(ACCMAIN.Fdate,'MM-yyyy') = '{mm}'");

                    var CurrentMonthContractValue = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.CurrentMonthContractValue = CurrentMonthContractValue.ValueCount;


                    //param.Add("@query", $@"select  ISNULL(sum(ISNULL(ExpectedContractValue,0)),0) AS ValueCount from ACCMAIN left join USERS on users.fid = ACCMAIN.UserId  WHERE ACCMAIN.Deleted = 0  AND  (USERS.EmployeeId = {Convert.ToInt32(Session["EmployeeId"])})");
                    param.Add("@query", $@"select  ISNULL(sum(ISNULL(ExpectedContractValue,0)),0) AS ValueCount from ACCMAIN left join Tool_UserLogin As users on users.EmployeeId = ACCMAIN.UserId  WHERE ACCMAIN.Deleted = 0 AND (users.EmployeeId = {Convert.ToInt32(Session["EmployeeId"])})");
                    var TotalCurrentMonthContractValue = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.TotalCurrentMonthContractValue = TotalCurrentMonthContractValue.ValueCount;

                    DynamicParameters param2 = new DynamicParameters();
                    DynamicParameters param3 = new DynamicParameters();
                   // param3.Add("@mainrole", mainRole);
                    param3.Add("@userid", Convert.ToInt32(Session["EmployeeId"]));
                    param3.Add("@Stage", "");
                    var data = DapperORM.ReturnList<dynamic>("Sp_LeadWiseInfo", param3).ToList();
                    ViewBag.List = data;

                   // param2.Add("@mainrole", mainRole);
                    param2.Add("@userid", Convert.ToInt32(Session["EmployeeId"]));
                    param2.Add("@p_Identyty", "IndustryTypeWiseLead");
                    var data1 = DapperORM.ReturnList<dynamic>("Sp_IndustryTypeWiseLead", param2).ToList();
                    ViewBag.List1 = data1;

                    param2.Add("@p_Identyty", "CompanyCategory");
                    var data2 = DapperORM.ReturnList<dynamic>("Sp_IndustryTypeWiseLead", param2).ToList();
                    ViewBag.List2 = data2;

                    param2.Add("@p_Identyty", "LeadSource");
                    var data3 = DapperORM.ReturnList<dynamic>("Sp_IndustryTypeWiseLead", param2).ToList();
                    ViewBag.List3 = data3;

                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_month", null);
                    param1.Add("@p_UserId", Convert.ToInt32(Session["EmployeeId"]));
                    //param1.Add("@p_mainRole", Session["MainRole"].ToString());
                    var Communication = DapperORM.ReturnList<dynamic>("SP_ACCCOMM_Dashboard", param1).ToList();
                    ViewBag.Communication = Communication;

                    param.Add("@query", $"select count(t1.Fid) as count from ACCMAIN t1 join MAS_STAGES t2 on t2.Fid = t1.MAS_STAGES_FID and t2.Origin ='Lead' and t2.Stage = 'Lost'  where t1.deleted = 0 and (t1.UserId = {Convert.ToInt32(Session["EmployeeId"])}) and Format(t1.Fdate,'MM-yyyy') = '{mm}'");
                    var lostlead = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.lostlead = Convert.ToInt32(lostlead.count);

                    if (lostlead.count == 0)
                    {
                        ViewBag.lostlead = 0;
                    }

                    //param.Add("@query", $"Select Max(ct.Flag) as country,MAX(st.Fid) as State_Fid ,st.StateName,Count(*) as Count from ACCLocation loc join accMAin acc on acc.Fid = loc.Accmain_Fid and acc.Stage != 'Lost' join MAS_STATE st on st.Fid = loc.MAS_STATE_Fid join MAS_COUNTRY ct on ct.Fid = st.MAS_COUNTRY_Fid join MAS_STAGES stage on stage.Fid = acc.MAs_stages_Fid where loc.Deleted = 0 and stage.Origin = 'Lead' and (acc.UserId = {Convert.ToInt32(Session["EmployeeId"])}) group by st.StateName");
                    param.Add("@query", $"Select MAX(st.StateId) as State_Fid ,st.StateName,Count(*) as Count from ACCLocation loc join accMAin acc on acc.Fid = loc.Accmain_Fid and acc.Stage != 'Lost' join Mas_States st on st.StateId = loc.MAS_STATE_Fid join Mas_Country ct on ct.CountryId = st.CountryId join MAS_STAGES stage on stage.Fid = acc.MAs_stages_Fid where loc.Deleted = 0 and stage.Origin = 'Lead' and (acc.UserId = {Convert.ToInt32(Session["EmployeeId"])}) group by st.StateName");
                    var StateList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.StateList = StateList;

                    string selectedDate = Request.QueryString["selectedDate"] ?? DateTime.Now.ToString("yyyy-MM-dd");

                    param.Add("@query", $@"SELECT CAST(ActivityDate AS DATE) AS ActivityDate,COUNT(*) AS ActivityCount FROM ACCACTIVITY WHERE Deleted = 0 AND CAST(ActivityDate AS DATE) = CAST(GETDATE() AS DATE)GROUP BY CAST(ActivityDate AS DATE)ORDER BY ActivityDate;");
                    var calendarData = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.CalendarData = calendarData;

                    param.Add("@query", $@"SELECT CommunicationType,CAST(CommunicationDate AS DATE) AS CommunicationDate,COUNT(*) AS CommunicationCount FROM ACCCOMM WHERE Deleted = 0 AND CAST(CommunicationDate AS DATE) =  CAST(GETDATE() AS DATE)GROUP BY CommunicationType, CAST(CommunicationDate AS DATE) ORDER BY CommunicationDate;");
                    var CommunicationData = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.CommunicationData = CommunicationData;

                    //param.Add("@query", $@"SELECT CAST(AttendanceDate AS DATE) AS AttendanceDay,COUNT(ACCMAIN_Fid) AS VisitCount FROM ATD_OPSATD WHERE CAST(AttendanceDate AS DATE) = CAST(GETDATE() AS DATE)GROUP BY CAST(AttendanceDate AS DATE);");
                    //var VisitCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    //ViewBag.VisitCount = VisitCount;

                }
                else
                {
                    DateTime m = Convert.ToDateTime(mn.Fdate);

                    int monthNumber = m.Month;
                    int yearNumber = m.Year;
                    string formattedMonth = monthNumber.ToString("D2");
                    string mm = $"{formattedMonth}-{yearNumber}";
                    ViewBag.mm = mm;
                    TempData["m"] = mm;

                    param.Add("@query", $"SELECT TOP 10 a.*,(SELECT COUNT(*)FROM ACCMAIN b WHERE b.fdate = a.fdate AND( b.UserId = { Convert.ToInt32(Session["EmployeeId"])})AND b.MAS_Stages_Fid BETWEEN 1 AND 4) AS fdateCount FROM ACCMAIN a WHERE ( a.UserId = { Convert.ToInt32(Session["EmployeeId"])}) AND a.MAS_Stages_Fid BETWEEN 1 AND 4 ORDER BY a.fid DESC;");
                    var LeadList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.LeadList = LeadList;

                    param.Add("@query", $"Select Count(acc.LeadPriority) as TotalPriorityCount from AccMain acc join MAS_STAGES stages on stages.Fid = acc.MAS_STAGES_FID and stages.Deleted = 0 where stages.Origin = 'Lead'and acc.Deleted = 0 and ( acc.UserId = {Convert.ToInt32(Session["EmployeeId"])})");
                    var TotalPriorityCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.TotalPriorityCount = TotalPriorityCount.TotalPriorityCount;

                    param.Add("@query", $"Select Count(acc.Fid) as TotalHotPriorityCount from AccMain acc join MAS_STAGES stages on stages.Fid = acc.MAS_STAGES_FID and stages.Deleted = 0 where stages.Origin = 'Lead' and acc.Stage != 'Lost' and acc.LeadPriority = 'Hot' and acc.Deleted = 0 and (acc.UserId = {Convert.ToInt32(Session["EmployeeId"])})");
                    var TotalHotPriorityCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.TotalHotPriorityCount = TotalHotPriorityCount.TotalHotPriorityCount;


                    param.Add("@query", $"Select Count(acc.Fid) as HotPriorityCount from AccMain acc join MAS_STAGES stages on stages.Fid = acc.MAS_STAGES_FID and stages.Deleted = 0 where stages.Origin = 'Lead' and acc.Stage != 'Lost' and acc.LeadPriority = 'Hot' and acc.Deleted = 0 and ( acc.UserId = {Convert.ToInt32(Session["EmployeeId"])}) and Format(acc.Fdate,'MM-yyyy') = '{mm}'");
                    var HotPriorityCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.HotPriorityCount = HotPriorityCount.HotPriorityCount;


                    param.Add("@query", $"Select Count(acc.Fid) as TotalColdPriorityCount from AccMain acc join MAS_STAGES stages on stages.Fid = acc.MAS_STAGES_FID and stages.Deleted = 0 where stages.Origin = 'Lead'and acc.LeadPriority = 'cold'and acc.Deleted = 0 and ( acc.UserId = {Convert.ToInt32(Session["EmployeeId"])})and Format(acc.Fdate,'MM-yyyy') = '{mm}'");
                    var ColdPriorityCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.ColdPriorityCount = ColdPriorityCount.TotalColdPriorityCount;

                    param.Add("@query", $"Select Count(acc.Fid) as TotalwarmPriorityCount from AccMain acc join MAS_STAGES stages on stages.Fid = acc.MAS_STAGES_FID and stages.Deleted = 0 where stages.Origin = 'Lead'and acc.LeadPriority = 'warm'and acc.Deleted = 0 and ( acc.UserId = {Convert.ToInt32(Session["EmployeeId"])})and Format(acc.Fdate,'MM-yyyy') = '{mm}'");
                    var WarmPriorityCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.WarmPriorityCount = WarmPriorityCount.TotalColdPriorityCount;


                    param.Add("@query", $"select top 10 ac.CompanyName,ac.ExpectedContractValue from ACCMAIN as ac join MAS_STAGES as stg on ac.MAS_STAGES_FID=stg.Fid where Format(ac.Fdate,'MM-yyyy')='{mm}' and ac.ExpectedContractValue Is not null  and stg.Stage Not in('Lost','Won') and stg.Deleted=0 and ac.Deleted=0 order by ac.ExpectedContractValue desc");
                    var top10ECV = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.top10ECV = top10ECV;

                    param.Add("@query", $"select ac.LeadPriority,count(ac.Fid) as Lead from ACCMAIN as ac join  MAS_STAGES as st on ac.MAS_STAGES_FID=st.Fid where  ac.LeadPriority Is Not Null and ac.LeadPriority !='' and  st.Stage not in('Lost') and st.Origin='Lead' and ac.Deleted=0 and (ac.UserId = {Convert.ToInt32(Session["EmployeeId"])})and FORMAT(ac.Fdate,'MM-yyyy')='{mm}' group by ac.LeadPriority");
                    var LeadPriorityData = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.LeadPriorityData = LeadPriorityData;

                    param.Add("@query", $"select ac.CompanyCategory ,COUNT(ac.Fid) as LeadCount from ACCMAIN  as ac join MAS_STAGES as st on ac.MAS_STAGES_FID=st.Fid where ac.CompanyCategory Is Not Null and st.Stage not in('Lost') and st.Origin='Lead' and ac.Deleted=0 and (ac.UserId = {Convert.ToInt32(Session["EmployeeId"])})and FORMAT(ac.Fdate,'MM-yyyy')='{mm}' group by ac.CompanyCategory");
                    var CategoryWise = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.CategoryWise = CategoryWise;
                    param.Add("@query", $"SELECT FORMAT(ac.Fdate, 'yyyy-MM') AS Period, ISNULL(SUM(CASE WHEN ms.Origin = 'Lead' THEN ac.ExpectedContractValue ELSE 0 END),0) AS Lead,ISNULL(SUM(CASE WHEN ms.Origin = 'Prospect' THEN ac.ExpectedContractValue ELSE 0 END),0) AS Opportunity FROM ACCMAIN ac JOIN MAS_STAGES ms ON ac.MAS_STAGES_FID = ms.Fid WHERE Format(ac.Fdate,'MM-yyyy')='{mm}' AND ms.Origin IN ('Lead', 'Prospect') AND ac.Fdate IS NOT NULL GROUP BY FORMAT(ac.Fdate, 'yyyy-MM') ORDER BY FORMAT(ac.Fdate, 'yyyy-MM')");
                    var HotPriorityCount1 = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();

                    if (HotPriorityCount1 != null)
                    {
                        var leadValue = HotPriorityCount1.Lead;
                        var opportunityValue = HotPriorityCount1.Opportunity;
                        var periodValue = HotPriorityCount1.Period;

                        ViewBag.LeadValue = leadValue;
                        ViewBag.OpportunityValue = opportunityValue;
                        ViewBag.PeriodValue = periodValue;
                        ViewBag.NewOrder = Convert.ToInt32(leadValue + opportunityValue);
                    }
                    else
                    {
                        ViewBag.LeadValue = 0;
                        ViewBag.OpportunityValue = 0;
                        ViewBag.PeriodValue = "";
                        ViewBag.NewOrder = 0;
                    }

                    DynamicParameters param2 = new DynamicParameters();
                    DynamicParameters param3 = new DynamicParameters();
                    //var Stage = "";
                    //param3.Add("@mainrole", mainRole);
                    param3.Add("@userid", Convert.ToInt32(Session["EmployeeId"]));
                    param3.Add("@Stage", "");
                    var data = DapperORM.ReturnList<dynamic>("Sp_LeadWiseInfo", param3).ToList();
                    ViewBag.List = data;

                   // param2.Add("@mainrole", mainRole);
                    param2.Add("@userid", Convert.ToInt32(Session["EmployeeId"]));
                    param2.Add("@p_Identyty", "IndustryTypeWiseLead");
                    var data1 = DapperORM.ReturnList<dynamic>("Sp_IndustryTypeWiseLead", param2).ToList();
                    ViewBag.List1 = data1;

                    param2.Add("@p_Identyty", "CompanyCategory");
                    var data2 = DapperORM.ReturnList<dynamic>("Sp_IndustryTypeWiseLead", param2).ToList();
                    ViewBag.List2 = data2;

                    param2.Add("@p_Identyty", "LeadSource");
                    var data3 = DapperORM.ReturnList<dynamic>("Sp_IndustryTypeWiseLead", param2).ToList();
                    ViewBag.List3 = data3;

                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_month", null);
                    param1.Add("@p_UserId", Convert.ToInt32(Session["EmployeeId"]));
                   // param1.Add("@p_mainRole", Session["MainRole"].ToString());
                    var Communication = DapperORM.ReturnList<dynamic>("SP_ACCCOMM_Dashboard", param1).ToList();
                    ViewBag.Communication = Communication;

                    param.Add("@query", $"select count(t1.Fid) as count from ACCMAIN t1 join MAS_STAGES t2 on t2.Fid = t1.MAS_STAGES_FID and t2.Origin ='Lead' and t2.Stage != 'Lost'  where t1.deleted = 0 and ( t1.UserId = {Convert.ToInt32(Session["EmployeeId"])}) and Format(t1.Fdate,'MM-yyyy') = '{mm}'");
                    var crntmontLead = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.crntmontLead = crntmontLead.count;


                    param.Add("@query", $"select count(t1.Fid) as count from ACCMAIN t1 join MAS_STAGES t2 on t2.Fid = t1.MAS_STAGES_FID and t2.Origin ='Lead' and t2.Stage != 'Lost' where t1.deleted = 0 and (t1.UserId = {Convert.ToInt32(Session["EmployeeId"])})");
                    var ActiveLeads = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.ActiveLeads = ActiveLeads.count;

                    param.Add("@query", $@"SELECT  COUNT(*) Count FROM ACCOWNERS WHERE Deleted = 0  AND TranferBy_Fid IS NOT NULL and  (LeadOwner_Fid = {Convert.ToInt32(Session["EmployeeId"])})and Format(Fdate,'MM-yyyy') = '{mm}'");
                    var CurrentMonthTransferLeads = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.CurrentMonthTransferLeads = CurrentMonthTransferLeads.Count;


                    param.Add("@query", $@"SELECT  COUNT(*) Count FROM ACCOWNERS WHERE Deleted = 0  AND TranferBy_Fid IS NOT NULL and  (LeadOwner_Fid = {Convert.ToInt32(Session["EmployeeId"])})");
                    var TotalTransferLeads = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.TotalTransferLeads = TotalTransferLeads.Count;

                    param.Add("@query", $@"select ISNULL(sum(ISNULL(ExpectedContractValue,0)),0) AS ValueCount  from ACCMAIN left join USERS on users.fid = ACCMAIN.UserId  WHERE ACCMAIN.Deleted = 0  AND  (USERS.EmployeeId = {Convert.ToInt32(Session["EmployeeId"])})and Format(ACCMAIN.Fdate,'MM-yyyy') = '{mm}'");
                    var CurrentMonthContractValue = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.CurrentMonthContractValue = CurrentMonthContractValue.ValueCount;


                    param.Add("@query", $@"select  ISNULL(sum(ISNULL(ExpectedContractValue,0)),0) AS ValueCount  from ACCMAIN left join Tool_UserLogin As users on users.EmployeeId = ACCMAIN.UserId  WHERE ACCMAIN.Deleted = 0 AND  users.EmployeeId = {Convert.ToInt32(Session["EmployeeId"])}");
                    var TotalCurrentMonthContractValue = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.TotalCurrentMonthContractValue = TotalCurrentMonthContractValue.ValueCount;
                    //

                    //param.Add("@query", $"Select Max(ct.Flag) as country,MAX(st.Fid) as State_Fid ,st.StateName,Count(*) as Count from ACCLocation loc join accMAin acc on acc.Fid = loc.Accmain_Fid and acc.Stage != 'Lost' join MAS_STATE st on st.Fid = loc.MAS_STATE_Fid join MAS_COUNTRY ct on ct.Fid = st.MAS_COUNTRY_Fid join MAS_STAGES stage on stage.Fid = acc.MAs_stages_Fid where loc.Deleted = 0 and stage.Origin = 'Lead' and (acc.UserId = {Convert.ToInt32(Session["EmployeeId"])}) group by st.StateName");
                    param.Add("@query", $"  Select MAX(st.StateId) as State_Fid ,st.StateName,Count(*) as Count from ACCLocation loc join accMAin acc on acc.Fid = loc.Accmain_Fid and acc.Stage != 'Lost' join Mas_States st on st.StateId = loc.MAS_STATE_Fid join Mas_Country ct on ct.CountryId = st.CountryId join MAS_STAGES stage on stage.Fid = acc.MAs_stages_Fid where loc.Deleted = 0 and stage.Origin = 'Lead' and (acc.UserId = {Convert.ToInt32(Session["EmployeeId"])}) group by st.StateName");
                    var StateList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.StateList = StateList;

                    string selectedDate = Request.QueryString["selectedDate"] ?? DateTime.Now.ToString("yyyy-MM-dd");

                    param.Add("@query", $@"SELECT CAST(ActivityDate AS DATE) AS ActivityDate,COUNT(*) AS ActivityCount FROM ACCACTIVITY WHERE Deleted = 0 AND CAST(ActivityDate AS DATE) = CAST(GETDATE() AS DATE)GROUP BY CAST(ActivityDate AS DATE)ORDER BY ActivityDate;");
                    var calendarData = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.CalendarData = calendarData;

                    param.Add("@query", $@"SELECT CommunicationType,CAST(CommunicationDate AS DATE) AS CommunicationDate,COUNT(*) AS CommunicationCount FROM ACCCOMM WHERE Deleted = 0 AND CAST(CommunicationDate AS DATE) =  CAST(GETDATE() AS DATE)GROUP BY CommunicationType, CAST(CommunicationDate AS DATE) ORDER BY CommunicationDate;");
                    var CommunicationData = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.CommunicationData = CommunicationData;

                    //param.Add("@query", $@"SELECT CAST(AttendanceDate AS DATE) AS AttendanceDay,COUNT(ACCMAIN_Fid) AS VisitCount FROM ATD_OPSATD WHERE CAST(AttendanceDate AS DATE) = CAST(GETDATE() AS DATE)GROUP BY CAST(AttendanceDate AS DATE);");
                    //var VisitCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    //ViewBag.VisitCount = VisitCount;
                }

                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "Dashboard", "LeadDashboard", "LeadDashboard", "View", "Passed", "", _clientIp, _clientMachineName);

                return View(mn);
            }
            catch (Exception ex)
            {
               LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "Dashboard", "LeadDashboard", "LeadDashboard", "View", "Failed", ex.Message, _clientIp, _clientMachineName);
               return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }

        }


        public ActionResult OpportunityDashboard(ACCMAIN mn)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DateTime fdate = mn.Fdate; // Assuming mn.Fdate is of type DateTime
             //   var mainRole = Session["MainRole"].ToString();
                // DapperORM.SetConnection();
                if (fdate == DateTime.MinValue)
                {
                    DateTime d = DateTime.Now;
                    mn.Fdate = d;
                    
                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@p_StartDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));
                    //param2.Add("@p_EndDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1));
                    param2.Add("@p_EndDate", DateTime.Today);
                   // param2.Add("@p_mainRole", mainRole);
                    param2.Add("@p_UserId", Convert.ToInt32(Session["EmployeeId"]));
                    var graphData = DapperORM.ReturnList<dynamic>("SP_OPP_Acc_Count", param2).ToList();
                    ViewBag.Data = graphData;
                    
                    param.Add("@query", $"Select isnull(Sum(prop.TotalContractValue),0) as 'SumTCV' from ACCPROPOSAL prop join ACCMAIN acc on acc.Fid = prop.ACCMAIN_Fid and acc.Deleted = 0 and acc.Stage != 'Lost' where prop.Deleted = 0 and (acc.UserId = {Convert.ToInt32(Session["EmployeeId"])})");
                    var SumTCV = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.SumTCV = SumTCV;

                    param.Add("@query", $"Select COUNT(prop.Fid) ProposalCount from ACCPROPOSAL prop join ACCMAIN acc on acc.Fid = prop.ACCMAIN_Fid and acc.Deleted = 0 and acc.Stage != 'Lost' where prop.Deleted = 0 and Month(prop.ExpectedClosingDate) = Month(GetDate()) and Year(prop.ExpectedClosingDate) = Year(GetDate()) and (acc.UserId = {Convert.ToInt32(Session["EmployeeId"])})");
                    var CountTP = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.CountTP = CountTP;

                    param.Add("@query", $"Select Coalesce(avg(prop.TotalContractValue),0) ProposalAvg from ACCPROPOSAL prop join ACCMAIN acc on acc.Fid = prop.ACCMAIN_Fid and acc.Deleted = 0 and acc.Stage != 'Lost' where prop.Deleted = 0 and Month(prop.ExpectedClosingDate) = Month(GetDate()) and Year(prop.ExpectedClosingDate) = Year(GetDate()) and (acc.UserId = {Convert.ToInt32(Session["EmployeeId"])})");
                    var AvgTP = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    var atp = AvgTP.ProposalAvg as double?;
                    ViewBag.AvgTP = atp.Value.ToString("N2");

                    param.Add("@query", $"select count(com.Fid) as CommunicationCount from ACCCOMM com join ACCMAIN acc on com.ACCMAIN_Fid=acc.Fid join MAS_STAGES stg on acc.MAS_STAGES_FID=stg.Fid where stg.Origin='Prospect' and stg.stage != 'Lost' and com.Deleted=0 and stg.Deleted=0 and Month(com.Fdate) = Month(GetDate()) and Year(com.Fdate) = Year(GetDate()) and (acc.UserId = {Convert.ToInt32(Session["EmployeeId"])})");
                    var CommunicationCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.CommunicationCount = CommunicationCount;

                    param.Add("@query", $"SELECT FORMAT(ISNULL(SUM(prop.ManagementFeeValue), 0), 'C', 'hi-IN') AS ManagementCount FROM ACCPROPOSAL prop JOIN ACCMAIN acc ON acc.Fid = prop.ACCMAIN_Fid AND acc.Deleted = 0 AND acc.Stage != 'Lost' WHERE prop.Deleted = 0 AND (acc.UserId = {Convert.ToInt32(Session["EmployeeId"])})");
                    var ManagementCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.ManagementCount = ManagementCount.ManagementCount;

                    param.Add("@query", $"select count(prop.Fid) AS ActivityCount from ACCACTIVITY prop join ACCMAIN acc on acc.Fid = prop.ACCMAIN_Fid and acc.Deleted = 0 and acc.Stage != 'Lost' where prop.Origin='Opportunity' and prop.Deleted = 0 and Month(prop.Fdate) = Month(GetDate()) and Year(prop.Fdate) = Year(GetDate()) and (acc.UserId = {Convert.ToInt32(Session["EmployeeId"])})");
                    var ActivityCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.ActivityCount = ActivityCount.ActivityCount;

                    param.Add("@query", $"Select Count(acc.Fid) as OpportunityCount from AccMain acc join MAS_STAGES stages on stages.Fid = acc.MAS_STAGES_FID and stages.Deleted = 0 where stages.Origin = 'Prospect' and acc.Stage != 'Lost'  and acc.Deleted = 0 and (acc.UserId = {Convert.ToInt32(Session["EmployeeId"])})");
                    var OpportunityCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.OpportunityCount = OpportunityCount;

                    param.Add("@query", $"Select ISNULL(SUM(prop.TotalContractValue), 0) AS TotalPraposalvalue from ACCPROPOSAL prop join ACCMAIN acc on acc.Fid = prop.ACCMAIN_Fid and acc.Deleted = 0 and acc.Stage != 'Lost' where prop.Deleted = 0 and Month(prop.ExpectedClosingDate) = Month(GetDate()) and Year(prop.ExpectedClosingDate) = Year(GetDate())  and (acc.UserId = {Convert.ToInt32(Session["EmployeeId"])})");
                    var ExpectedClosingCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.ExpectedClosingCount = ExpectedClosingCount.TotalPraposalvalue;

                    param2.Add("@p_StartDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));
                    //param2.Add("@p_EndDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1));
                    param2.Add("@p_EndDate", DateTime.Today);
                   // param2.Add("@p_mainRole", mainRole);
                    param2.Add("@p_UserId", Convert.ToInt32(Session["EmployeeId"]));
                    var OpportunityCountAll = DapperORM.ReturnList<dynamic>("SP_Count_Allprospects", param2).ToList();
                    ViewBag.OpportunityCountAll = OpportunityCountAll;

                    param.Add("@query", $"Select isnull(Count(acc.Fid),0) as 'TenderApplCount' from accmain acc join MAS_STAGES stages on stages.Fid = acc.MAS_STAGES_FID and stages.Deleted = 0 where stages.Origin = 'Prospect' and acc.Stage != 'Lost'  and acc.Deleted = 0 and acc.TenderApplicable = 1 and (acc.UserId = {Convert.ToInt32(Session["EmployeeId"])})");
                    var TenderApplCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.TenderApplCount = TenderApplCount;

                    param.Add("@query", $"select Top(5) CompanyName,LeadName,Stage from ACCMAIN where Stage = 'Won' and (UserId = {Convert.ToInt32(Session["EmployeeId"])})");
                    var Opportunity = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.Opportunity = Opportunity;
                    param.Add("@query", $"SELECT COUNT(*) AS Count FROM ACCMAIN acc JOIN MAS_STAGES stg ON acc.MAS_STAGES_FID = stg.Fid JOIN ACCSERVICES ser ON ser.ACCMAIN_Fid = acc.Fid WHERE stg.Origin = 'Prospect' AND stg.Stage NOT IN ('Lost') AND (acc.UserId = {Convert.ToInt32(Session["EmployeeId"])});");
                    var Count = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.Count = Count.Count;
                }
                else
                {
                    param.Add("@query", $"SELECT COUNT(*) AS Count FROM ACCMAIN acc JOIN MAS_STAGES stg ON acc.MAS_STAGES_FID = stg.Fid JOIN ACCSERVICES ser ON ser.ACCMAIN_Fid = ser.MAS_SERVICE WHERE stg.Origin = 'Prospect' AND stg.Stage NOT IN ('Lost') AND (acc.UserId = {Convert.ToInt32(Session["EmployeeId"])});");
                    var Count = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.Count = Count.Count;

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@p_StartDate", new DateTime(fdate.Year, fdate.Month, 1));
                    //param2.Add("@p_EndDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1));
                    param2.Add("@p_EndDate", fdate.AddMonths(1).AddDays(-1));
                   // param2.Add("@p_mainRole", mainRole);
                    param2.Add("@p_UserId", Convert.ToInt32(Session["EmployeeId"]));
                    var graphData = DapperORM.ReturnList<dynamic>("SP_OPP_Acc_Count", param2).ToList();
                    ViewBag.Data = graphData;

                    param.Add("@query", $"Select isnull(Sum(prop.TotalContractValue),0) as 'SumTCV' from ACCPROPOSAL prop join ACCMAIN acc on acc.Fid = prop.ACCMAIN_Fid and acc.Deleted = 0 and acc.Stage != 'Lost' where prop.Deleted = 0 and Month(prop.ExpectedClosingDate) = Month('{mn.Fdate.ToString("yyyy-MM-dd")}') and Year(prop.ExpectedClosingDate) = Year('{mn.Fdate.ToString("yyyy-MM-dd")}') and (acc.UserId = {Convert.ToInt32(Session["EmployeeId"])})");
                    var SumTCV = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.SumTCV = SumTCV;

                    param.Add("@query", $"Select COUNT(prop.Fid) ProposalCount from ACCPROPOSAL prop join ACCMAIN acc on acc.Fid = prop.ACCMAIN_Fid and acc.Deleted = 0 and acc.Stage != 'Lost' where prop.Deleted = 0 and Month(prop.ExpectedClosingDate) = Month('{mn.Fdate.ToString("yyyy-MM-dd")}') and Year(prop.ExpectedClosingDate) = Year('{mn.Fdate.ToString("yyyy-MM-dd")}') and (acc.UserId = {Convert.ToInt32(Session["EmployeeId"])})");
                    var CountTP = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.CountTP = CountTP;

                    param.Add("@query", $"Select avg(prop.TotalContractValue) ProposalAvg from ACCPROPOSAL prop join ACCMAIN acc on acc.Fid = prop.ACCMAIN_Fid and acc.Deleted = 0 and Month(prop.ExpectedClosingDate) = Month('{mn.Fdate.ToString("yyyy-MM-dd")}') and Year(prop.ExpectedClosingDate) = Year('{mn.Fdate.ToString("yyyy-MM-dd")}') and acc.Stage != 'Lost' where prop.Deleted = 0  and ( acc.UserId = {Convert.ToInt32(Session["EmployeeId"])})");
                    var AvgTP = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.AvgTP = AvgTP.ProposalAvg as double?;

                    param.Add("@query", $"select count(com.Fid) as CommunicationCount from ACCCOMM com join ACCMAIN acc on com.ACCMAIN_Fid=acc.Fid join MAS_STAGES stg on acc.MAS_STAGES_FID=stg.Fid where stg.Origin='Prospect' and stg.stage != 'Lost' and com.Deleted=0 and stg.Deleted=0 and Month(com.Fdate) = Month('{mn.Fdate.ToString("yyyy-MM-dd")}') and Year(com.Fdate) = Year('{mn.Fdate.ToString("yyyy-MM-dd")}') and (acc.UserId = {Convert.ToInt32(Session["EmployeeId"])})");
                    var CommunicationCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.CommunicationCount = CommunicationCount;

                    param.Add("@query", $"select count(prop.Fid) AS ActivityCount from ACCACTIVITY prop join ACCMAIN acc on acc.Fid = prop.ACCMAIN_Fid and acc.Deleted = 0 and acc.Stage != 'Lost' where prop.Origin='Opportunity' and prop.Deleted = 0  and (acc.UserId = {Convert.ToInt32(Session["EmployeeId"])}) and Month(prop.Fdate) = Month('{mn.Fdate.ToString("yyyy-MM-dd")}') and Year(prop.Fdate) = Year('{mn.Fdate.ToString("yyyy-MM-dd")}')");
                    var ActivityCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.ActivityCount = ActivityCount.ActivityCount;

                    param.Add("@query", $"Select Count(acc.Fid) as OpportunityCount from AccMain acc join MAS_STAGES stages on stages.Fid = acc.MAS_STAGES_FID and stages.Deleted = 0 where stages.Origin = 'Prospect' and acc.Stage != 'Lost'  and acc.Deleted = 0 and (acc.UserId = {Convert.ToInt32(Session["EmployeeId"])})");
                    var OpportunityCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.OpportunityCount = OpportunityCount;

                    param.Add("@query", $"Select Count(prop.Fid) as ExpectedClosingCount from ACCPROPOSAL prop join ACCMAIN acc on acc.Fid = prop.ACCMAIN_Fid and acc.Deleted = 0 and acc.Stage != 'Lost' where prop.Deleted = 0 and Month(prop.ExpectedClosingDate) = Month('{mn.Fdate.ToString("yyyy-MM-dd")}') and Year(prop.ExpectedClosingDate) = Year('{mn.Fdate.ToString("yyyy-MM-dd")}') and (acc.UserId = {Convert.ToInt32(Session["EmployeeId"])})");
                    var ExpectedClosingCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.ExpectedClosingCount = ExpectedClosingCount;

                    param2.Add("@p_StartDate", new DateTime(fdate.Year, fdate.Month, 1));
                    //param2.Add("@p_EndDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1));
                    param2.Add("@p_EndDate", fdate.AddMonths(1).AddDays(-1));
                   // param2.Add("@p_mainRole", mainRole);
                    param2.Add("@p_UserId", Convert.ToInt32(Session["EmployeeId"]));
                    var OpportunityCountAll = DapperORM.ReturnList<dynamic>("SP_Count_Allprospects", param2).ToList();
                    ViewBag.OpportunityCountAll = OpportunityCountAll;

                    param.Add("@query", $"Select isnull(Count(acc.Fid),0) as 'TenderApplCount' from accmain acc join MAS_STAGES stages on stages.Fid = acc.MAS_STAGES_FID and stages.Deleted = 0 where stages.Origin = 'Prospect' and acc.Stage != 'Lost'  and acc.Deleted = 0 and acc.TenderApplicable = 1 and (acc.UserId = {Convert.ToInt32(Session["EmployeeId"])})");
                    var TenderApplCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.TenderApplCount = TenderApplCount;
                }
               LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "Dashboard", "OpportunityDashboard", "OpportunityDashboard", "View", "Passed", "", _clientIp, _clientMachineName);

                return View(mn);
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "Dashboard", "OpportunityDashboard", "OpportunityDashboard", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult AccountDashboard(ACCMAIN mn)
        {
            try
            {

                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                //  DapperORM.SetConnection();

                DateTime fdate = mn.Fdate; // Assuming mn.Fdate is of type DateTime
             //   var mainRole = Session["MainRole"].ToString();

                if (fdate == DateTime.MinValue)
                {
                    DateTime d = DateTime.Now;
                    mn.Fdate = d;

                    param.Add("@query", $"select coalesce(SUM(TotalContractValue),0) as TotalContractValue from ACCCONTRACT where Active = 1 and Deleted = 0 and ACCMAIN_Fid >0 AND ((IsContractExtended = 1 AND ContractExtendedDate >= CAST(GETDATE() AS DATE)) OR (IsContractExtended = 0 AND ContractEndDate >= CAST(GETDATE() AS DATE))) and(UserId = {Convert.ToInt32(Session["EmployeeId"])})");
                    var SumTCV = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.SumTCV = SumTCV;

                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_Encrypted_Id", "AccountList");
                    //param1.Add("@p_UserRole", Session["MainRole"]);
                    param1.Add("@p_Userid", Session["EmployeeId"]);
                    var data = DapperORM.ReturnList<ACCMAIN>("sp_List_ACCMAIN", param1).ToList();

                    var AccountCount = data.Count();
                    ViewBag.AccountCount = AccountCount;

                    param.Add("@query", $"select coalesce(COUNT(Fid),0) as Fid from ACCCONTRACT where Active = 1 and Deleted = 0 and ACCMAIN_Fid >0 and ContractEndDate >= Cast(GetDate() as date) and (UserId = {Convert.ToInt32(Session["EmployeeId"])}) ");
                    var TotalContractCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.TotalContractCount = TotalContractCount.Fid;

                    var InvoiceList = DapperORM.ReturnList<dynamic>("AccDashboard_pendingInvoice").FirstOrDefault();
                    ViewBag.InvoiceList = InvoiceList;

                }
                else
                {
                    param.Add("@query", $"select coalesce(SUM(TotalContractValue),0) as TotalContractValue from ACCCONTRACT where Active = 1 and Deleted = 0 and ACCMAIN_Fid >0 AND ((IsContractExtended = 1 AND ContractExtendedDate >= CAST(GETDATE() AS DATE)) OR (IsContractExtended = 0 AND ContractEndDate >= CAST(GETDATE() AS DATE))) and(acc.UserId = {Convert.ToInt32(Session["EmployeeId"])})");
                    var SumTCV = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.SumTCV = SumTCV;

                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_Encrypted_Id", "AccountList");
                  //  param1.Add("@p_UserRole", Session["MainRole"]);
                    param1.Add("@p_Userid", Session["EmployeeId"]);
                    var data = DapperORM.ReturnList<ACCMAIN>("sp_List_ACCMAIN", param1).ToList();

                    var AccountCount = data.Count();
                    ViewBag.AccountCount = AccountCount;

                    param.Add("@query", $"select coalesce(COUNT(Fid),0) as Fid from ACCCONTRACT where Active = 1 and Deleted = 0 and ACCMAIN_Fid >0 and ContractEndDate >= Cast(GetDate() as date) and (UserId = {Convert.ToInt32(Session["EmployeeId"])}) ");
                    var TotalContractCount = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.TotalContractCount = TotalContractCount.Fid;

                    var InvoiceList = DapperORM.ReturnList<dynamic>("AccDashboard_pendingInvoice").FirstOrDefault();
                    ViewBag.InvoiceList = InvoiceList;
                }
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "Dashboard", "AccountDashboard", "AccountDashboard", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "Dashboard", "AccountDashboard", "AccountDashboard", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        [HttpGet]
        public ActionResult CRM_LeadForm(string Encrypted_Id)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var Id = Session["EmployeeId"];
                if (Encrypted_Id != null)
                {
                    // DapperORM.SetConnection();
                    {
                        param.Add("@query", "Select CompanyId As Fid, CompanyName from Mas_CompanyProfile where Deactivate=0");
                       // param.Add("@query", "Select CompanyId As Fid, CompanyName from Mas_CompanyProfile where Deactivate=0");
                        var CompanyNameList = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).ToList();
                         ViewBag.CmpName = CompanyNameList;

                        param.Add("@query", "Select Fid as Fid,LeadSource from MAS_LEADSOURCE where deleted=0");
                        var LeadSourceList = DapperORM.ReturnList<MAS_LEADSOURCE>("Sp_QueryExcution", param).ToList();
                        ViewBag.LeadSourceList = LeadSourceList;

                        param.Add("@query", $"If Exists(select * from MAS_EMPINDUSTYPE where MAS_Employee_Fid='{Id}') begin (Select t1.Fid,t1.IndustryType from MAS_INDUSTRYTYPE t1 join MAS_EMPINDUSTYPE t2 on t1.Fid=t2.MAS_INDUSTRYTYPE where t1.Deleted=0 and t2.MAS_Employee_Fid='{Id}' and t2.Deleted = 0)end else begin SELECT Fid, IndustryType FROM MAS_INDUSTRYTYPE  WHERE Deleted = 0; end");
                        var IndustryTypeList = DapperORM.ReturnList<MAS_INDUSTRYTYPE>("Sp_QueryExcution", param).ToList();
                        ViewBag.IndustryTypeList = IndustryTypeList;
                        
                        param.Add("@query", "Select BranchId as Fid,BranchName as CompanyName from Mas_Branch where Deactivate=0");
                        var BUCodeList = DapperORM.ReturnList<Ocean>("Sp_QueryExcution", param).ToList();
                        ViewBag.BUCodeList = BUCodeList;

                        param.Add("@query", "Select CurrencyCountryId As Fid,ShortName As Currency from Mas_Currency where Deactivate=0");
                        var CountryCodeList = DapperORM.ReturnList<MAS_COUNTRY>("Sp_QueryExcution", param).ToList();
                        ViewBag.CountryCodeList = CountryCodeList;

                        DynamicParameters param1 = new DynamicParameters();
                        param1.Add("@p_Encrypted_Id", Encrypted_Id);
                        var data = DapperORM.ReturnList<ACCMAIN>("sp_List_ACCMAIN", param1).FirstOrDefault();
                        ViewBag.List = data;
                      //  ViewBag.CmpName = new SelectList(CompanyNameList, "Fid", "CompanyName", data.CompanyName1);
                        ViewBag.EncryptedId = Encrypted_Id;
                        
                        return View(data);
                    }
                }
                ViewBag.AddUpdateTitle = "Add";
                ViewBag.EncryptedId = Encrypted_Id;
                var EmployeeName = Session["EmployeeName"];
                var EmployeeId = Session["EmployeeId"];
                // DapperORM.SetConnection();
                {
       
                    param.Add("@query", "Select CompanyId As Fid, CompanyName from Mas_CompanyProfile where Deactivate=0");
                    var CmpName = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).ToList();
                    ViewBag.CmpName = CmpName;

                    param.Add("@query", "Select Fid, CompanyName from ACCMAIN where deleted=0");
                    var CompanyNameList = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).ToList();
                    ViewBag.CompanyNameList = CompanyNameList;


                    //param.Add("@query", "Select BranchId as Fid,BranchName as CompanyName from Mas_Branch where Deactivate=0 Order By BranchName");
                    //var BUCodeList = DapperORM.ReturnList<Ocean>("Sp_QueryExcution", param).ToList();
                    //ViewBag.BUCodeList = BUCodeList;
                    ViewBag.BUCodeList = new List<Ocean>();

                    param.Add("@query", "Select Fid as Fid,LeadSource from MAS_LEADSOURCE where deleted=0");
                    var LeadSourceList = DapperORM.ReturnList<MAS_LEADSOURCE>("Sp_QueryExcution", param).ToList();
                    ViewBag.LeadSourceList = LeadSourceList;

                    param.Add("@query", "Select CurrencyCountryId As Fid,ShortName As Currency from Mas_Currency where Deactivate=0");
                    var CountryCodeList = DapperORM.ReturnList<MAS_COUNTRY>("Sp_QueryExcution", param).ToList();
                    ViewBag.CountryCodeList = CountryCodeList;

                    param.Add("@query", $"If Exists(select * from MAS_EMPINDUSTYPE where MAS_Employee_Fid='{Id}') begin (Select t1.Fid,t1.IndustryType from MAS_INDUSTRYTYPE t1 join MAS_EMPINDUSTYPE t2 on t1.Fid=t2.MAS_INDUSTRYTYPE where t1.Deleted=0 and t2.MAS_Employee_Fid='{Id}' and t2.Deleted = 0)end else begin SELECT Fid, IndustryType FROM MAS_INDUSTRYTYPE  WHERE Deleted = 0; end");
                    var IndustryTypeList = DapperORM.ReturnList<MAS_INDUSTRYTYPE>("Sp_QueryExcution", param).ToList();
                    ViewBag.IndustryTypeList = IndustryTypeList;
                }
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "Dashboard", "CRM_LeadForm", "CRM_LeadForm", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "Dashboard", "CRM_LeadForm", "CRM_LeadForm", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }


        #region Get Branch Name
        [HttpGet]
        public ActionResult GetBusinessUnit(int ? CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select BranchId As Id,BranchName As Name From Mas_Branch where Deactivate=0 And CmpId='" + CmpId + "' order by Name");
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                // return Json(data, JsonRequestBehavior.AllowGet);
                return Json(new { Branch = Branch }, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        [HttpGet]
        public ActionResult GetCompanyList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // DapperORM.SetConnection();

                param.Add("@query", "Select Fid, CompanyName from ACCMAIN where deleted=0");
                var CompanyNameList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();

                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetCompanyList", "GetCompanyList", "View", "Passed", "", _clientIp, _clientMachineName);

                return Json(new { CompanyNameList = CompanyNameList }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetCompanyList", "GetCompanyList", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public JsonResult IsLeadExist(string Encrypted_Id, string CompanyContactNumber, string CompanyEmail, string CompanyName, string LeadName)
        {
            try
            {
                // DapperORM.SetConnection();
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_Encrypted_Id", Encrypted_Id);
                    param.Add("@p_CompanyContactNumber", CompanyContactNumber);
                    param.Add("@p_CompanyEmail", CompanyEmail);
                    param.Add("@p_CompanyName", CompanyName);
                    param.Add("@p_LeadName", LeadName);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("Sp_SUD_ACCMAIN", param);
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

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult CRM_LeadForm(ACCMAIN ACCMAIN)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
                //  DapperORM.SetConnection();
                {
                    string process = string.IsNullOrEmpty(ACCMAIN.Encrypted_Id) ? "Save" : "Update";

                    var param = new DynamicParameters();
                    param.Add("@p_process", process);
                    param.Add("@P_Fid", ACCMAIN.Fid);
                    param.Add("@p_Encrypted_Id", ACCMAIN.Encrypted_Id, dbType: DbType.String, direction: ParameterDirection.InputOutput, size: 70);
                    param.Add("@p_LeadPriority", ACCMAIN.LeadPriority);
                    param.Add("@p_LeadName", ACCMAIN.LeadName);
                    param.Add("@p_BUCode", ACCMAIN.BUCode);
                    param.Add("@p_CompanyName", ACCMAIN.CompanyId);
                    param.Add("@p_CompanyCategory", ACCMAIN.CompanyCategory);
                    param.Add("@p_OrganisationType", ACCMAIN.OrganisationType);
                    param.Add("@p_MAS_LEADSOURCE_Fid", ACCMAIN.MAS_LEADSOURCE_Fid);
                    param.Add("@p_LeadType", ACCMAIN.LeadType);
                    param.Add("@p_CINNo", ACCMAIN.CINNo);
                    param.Add("@p_MAS_INDUSTRYTYPE_Fid", ACCMAIN.MAS_INDUSTRYTYPE_Fid);
                    param.Add("@p_CompanyEmail", ACCMAIN.CompanyEmail);
                    param.Add("@p_CompanyContactNumber", ACCMAIN.CompanyContactNumber);
                    param.Add("@p_CountryCode", ACCMAIN.CountryCode);
                    param.Add("@p_Summary", ACCMAIN.Summary);
                    param.Add("@p_IsDirectAccount", ACCMAIN.IsDirectAccount);
                    param.Add("@P_MAS_STAGES_FID", ACCMAIN.MAS_STAGES_FID);
                    param.Add("@p_Stage", ACCMAIN.Stage);
                    param.Add("@p_ECVCurrency", ACCMAIN.ECVCurrency);
                    param.Add("@p_CompanyRating", ACCMAIN.CompanyRating);
                    param.Add("@p_ReferenceName", ACCMAIN.ReferenceName);
                    param.Add("@p_TenderApplicable", ACCMAIN.TenderApplicable);
                    param.Add("@p_Tendersource", ACCMAIN.Tendersource);
                    param.Add("@p_TenderNumber", ACCMAIN.TenderNumber);
                    param.Add("@p_ExpectedContractValue", ACCMAIN.ExpectedContractValue);
                    param.Add("@p_Website", ACCMAIN.Website);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_Mip", Dns.GetHostByName(MachineId).AddressList[0].ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeId"]);
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 70);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 10);

                    var data = DapperORM.ExecuteReturn("Sp_SUD_ACCMAIN", param);

                    var FId = param.Get<string>("@p_Id");



                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    var message = param.Get<string>("@p_msg");

                    string Id = string.IsNullOrEmpty(ACCMAIN.Encrypted_Id)
                                ? param.Get<string>("@p_Encrypted_Id")
                                : ACCMAIN.Encrypted_Id;

                    ViewBag.EncryptedId = Id;

                    //if (ACCMAIN.CompanyLogoPic != null)
                    //{
                    //    var GetDocPath = sqlcon.QuerySingle("Select DocumentURL from SET_DOCURLDRIVE");
                    //    var GetFirstPath = GetDocPath.DocumentURL;
                    //    var FirstPath = Path.Combine(GetFirstPath, "CRM", "CompanyLogo", Convert.ToString(ACCMAIN.CompanyName));

                    //    if (!Directory.Exists(FirstPath))
                    //    {
                    //        Directory.CreateDirectory(FirstPath);
                    //    }
                    //    string ImgUploadFrontPageFilePath = Path.Combine(FirstPath, ACCMAIN.CompanyLogoPic.FileName);
                    //    ACCMAIN.CompanyLogoPic.SaveAs(ImgUploadFrontPageFilePath);
                    //    sqlcon.Query("update AccMain set CompanyLogo='" + ImgUploadFrontPageFilePath + "' where Encrypted_Id='" + Id + "'");

                    //}

                    if (ACCMAIN.CompanyLogoPic != null)
                    {
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='SalesForce'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                        
                    var FirstPath = Path.Combine(GetFirstPath, "CRM", "CompanyLogo", "Add_Documents", Convert.ToString(ACCMAIN.CompanyName));

                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }

                    if (ACCMAIN.CompanyLogoPic != null)
                    {
                        string ImgUploadFrontPageFilePath = Path.Combine(FirstPath, ACCMAIN.CompanyLogoPic.FileName);
                            ACCMAIN.CompanyLogoPic.SaveAs(ImgUploadFrontPageFilePath);
                        sqlcon.Query("update AccMain set CompanyLogo='" + ImgUploadFrontPageFilePath + "' where Encrypted_Id='" + Id + "'");
                        //sqlcon.Query("INSERT INTO ACCDMS (AttachmentURL, Origin, ACCMAIN_Fid, Encrypted_Id,Deleted) VALUES ('" + ImgUploadFrontPageFilePath + "', 'Activity', '" + Id + "', '" + ENCRYPTED + "',0)");
                    }

                }

                if (message == "Record saved successfully" && ACCMAIN.Encrypted_Id == null)
                    {
                        var param3 = new DynamicParameters();
                        param3.Add("@p_process", "Save");
                        param3.Add("@P_ACCMAIN_Fid", FId);
                        param3.Add("@p_BUCode", ACCMAIN.BUCode);
                        param3.Add("@p_CreatedUpdateBy", Session["EmployeeId"]);
                        param3.Add("@p_MAS_STAGES_FID", ACCMAIN.MAS_STAGES_FID);
                        param3.Add("@p_MachineName", Dns.GetHostName().ToString());
                        param3.Add("@p_Mip", Dns.GetHostByName(MachineId).AddressList[0].ToString());
                        var data5 = DapperORM.ExecuteReturn("Sp_SUD_ACCSTAGES", param3);

                        LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "CRM_LeadForm", "CRM_LeadForm", "Created", "Passed", "", _clientIp, _clientMachineName);

                    }

                    if (ACCMAIN.Stage != "Won")
                    {
                        return RedirectToAction("Lead_Details", "CRM", new { EncryptedId = Id });
                    }
                    else
                    {
                        return RedirectToAction("AccountDetails", "CRM", new { EncryptedId = Id });
                    }
                }
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "CRM_LeadForm", "CRM_LeadForm", "Created", "Deleted", ex.Message, _clientIp, _clientMachineName);

                // Log the error here if needed
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }


        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                //DapperORM.SetConnection();
                {
                    param.Add("@p_Encrypted_Id", "List");
                    var data = DapperORM.DynamicList("sp_List_ACCMAIN", param);
                    ViewBag.List = data;
                    return View();
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        [HttpGet]
        public ActionResult AddService(string Acc_Encrypted_Id)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
                //  DapperORM.SetConnection();
                {
                    param.Add("@query", "Select Fid as Fid,ServiceCategory from MAS_SERCATG where deleted=0");
                    var ServiceCatList = DapperORM.ReturnList<MAS_SERCATG>("Sp_QueryExcution", param).ToList();
                    ViewBag.ServiceCatList = ServiceCatList;

                    param.Add("@query", "Select Fid,Service from MAS_SERVICE where deleted=0");
                    var ServiceList = DapperORM.ReturnList<MAS_SERVICE>("Sp_QueryExcution", param).ToList();
                    ViewBag.ServiceList = ServiceList;

                    param.Add("@query", "Select Fid from ACCMAIN where Encrypted_Id=Encrypted_Id and deleted=0");
                    var AccmainFid = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.AccmainFid = AccmainFid.Fid;
                    ViewBag.AccEncryptedId = Acc_Encrypted_Id;

                    param.Add("@query", "select ACCSERVICES.fid as AccFid,ACCSERVICES.Encrypted_Id as Sevice_EncryptedId,accmain.LeadName,MAS_SERCATG.ServiceCategory,MAS_SERVICE.service from ACCSERVICES ACCSERVICES join ACCMAIN accmain on ACCSERVICES.accmain_fid=ACCMAIN.fid join MAS_SERVICE MAS_SERVICE on MAS_SERVICE.fid = ACCSERVICES.mas_service join MAS_SERCATG MAS_SERCATG on MAS_SERVICE.mas_sercatg = MAS_SERCATG.Fid where ACCSERVICES.deleted = 0 and ACCMAIN.Fid='" + AccmainFid.Fid + "'");
                    var ACCServiceList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.ACCServiceList = ACCServiceList;


                }
               LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AddService", "AddService", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
               LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AddService", "AddService", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        [HttpGet]
        public ActionResult ServiceCategoryWiseService(int ServiceCategory)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
                // DapperORM.SetConnection();
                {

                    param.Add("@query", "Select Fid as Fid,Service from MAS_SERVICE where MAS_SERCATG='" + ServiceCategory + "' AND Deleted=0 ");
                    var ServiceList = DapperORM.ReturnList<MAS_SERVICE>("Sp_QueryExcution", param).ToList();
                    ViewBag.LeadSourceList = ServiceList;
                    return Json(ServiceList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, "An internal server error occurred: " + ex.Message);

            }
        }

        public JsonResult IsServiceExist(string Encrypted_Id, int? ServiceId, int? ACCMAIN_Fid)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_Encrypted_Id", Encrypted_Id);
                    param.Add("@p_MAS_SERVICE", ServiceId);
                    param.Add("@p_ACCMAIN_Fid", ACCMAIN_Fid);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("Sp_SUD_ACCSERVICES", param);
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
            catch (Exception ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddService(ACCSERVICES ACCSERVICES, string Acc_EncryptedId)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
                // DapperORM.SetConnection();
                {
                    param.Add("@p_process", "Save");
                    param.Add("@P_Fid", ACCSERVICES.Fid);
                    param.Add("@P_Encrypted_Id", ACCSERVICES.Encrypted_Id);
                    param.Add("@p_ACCMAIN_Fid", ACCSERVICES.ACCMAIN_Fid);
                    param.Add("@p_MAS_SERVICE", ACCSERVICES.MAS_SERVICE);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_Mip", Dns.GetHostByName(MachineId).AddressList[0].ToString());
                    param.Add("@p_UserId", Session["EmployeeId"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var data = DapperORM.ExecuteReturn("Sp_SUD_ACCSERVICES", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AddService", "AddService", "Created", "Passed", "", _clientIp, _clientMachineName);

                    return RedirectToAction("CRM_LeadForm", "CRM", new { Encrypted_Id = Acc_EncryptedId });
                }
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AddService", "AddService", "Created", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        //[HttpGet]
        //public ActionResult DeleteService(string EncryptedId)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { Area = "" });
        //        }
        //        param.Add("@p_process", "Delete");
        //        param.Add("@p_Encrypted_Id", EncryptedId);
        //        param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
        //        param.Add("@p_MachineName", Dns.GetHostName().ToString());
        //        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
        //        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parame
        //        var Result = DapperORM.ExecuteReturn("Sp_SUD_ACCSERVICES", param);
        //        var message = param.Get<string>("@p_msg");
        //        var Icon = param.Get<string>("@p_Icon");
        //        TempData["Message"] = message;
        //        TempData["Icon"] = Icon.ToString();
        //        return Json(true, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //         return RedirectToAction("ErrorPage", "Login", new { area = "" });
        //    }
        //}

        public ActionResult CRMDetails(string EncryptedId)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters param1 = new DynamicParameters();

                param1.Add("@p_Encrypted_Id", EncryptedId);
                var List = DapperORM.ReturnList<dynamic>("sp_List_ACCMAIN", param1).FirstOrDefault();
                ViewBag.List = List;
                // DapperORM.SetConnection();
                {

                    param.Add("@query", "Select Fid as Fid,ServiceCategory from MAS_SERCATG where deleted=0");
                    var ServiceCatList = DapperORM.ReturnList<MAS_SERCATG>("Sp_QueryExcution", param).ToList();
                    ViewBag.ServiceCatList = ServiceCatList;

                    param.Add("@query", "Select Fid,Service from MAS_SERVICE where deleted=0");
                    var ServiceList = DapperORM.ReturnList<MAS_SERVICE>("Sp_QueryExcution", param).ToList();
                    ViewBag.ServiceList = ServiceList;

                    param.Add("@query", "Select Fid from ACCMAIN where Encrypted_Id=Encrypted_Id and deleted=0");
                    var AccmainFid = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.AccmainFid = AccmainFid.Fid;
                    ViewBag.AccEncryptedId = EncryptedId;
                    ViewBag.BUCode = AccmainFid.BUCode;

                    param.Add("@query", "select ACCSERVICES.fid as AccFid,ACCSERVICES.Encrypted_Id,accmain.LeadName,MAS_SERCATG.ServiceCategory,MAS_SERVICE.service from ACCSERVICES ACCSERVICES join ACCMAIN accmain on ACCSERVICES.accmain_fid=ACCMAIN.fid join MAS_SERVICE MAS_SERVICE on MAS_SERVICE.fid = ACCSERVICES.mas_service join MAS_SERCATG MAS_SERCATG on MAS_SERVICE.mas_sercatg = MAS_SERCATG.Fid where ACCSERVICES.deleted = 0 and ACCMAIN.Fid='" + AccmainFid.Fid + "'");
                    var ACCServiceList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.ACCServiceList = ACCServiceList;


                    param.Add("@query", "Select DepartmentId As Fid,DepartmentName As Department from Mas_Department where Deactivate=0");
                    var DepartmentList = DapperORM.ReturnList<MAS_DEPT>("Sp_QueryExcution", param).ToList();
                    ViewBag.DepartmentList = DepartmentList;

                    param.Add("@query", "Select DesignationId As Fid,DesignationName As Designation from Mas_Designation where Deactivate=0");
                    var DesignationList = DapperORM.ReturnList<MAS_DESG>("Sp_QueryExcution", param).ToList();
                    ViewBag.DesignationList = DesignationList;

                }
                 LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "CRMDetails", "CRMDetails", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();

            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }
        
        public ActionResult Lead_Dashboard()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                //DapperORM.SetConnection();

                var EMP = Session["EmployeeId"];
                param.Add("@p_Encrypted_Id", "LeadList");
               // param.Add("@p_UserRole", Session["MainRole"]);
                param.Add("@p_Userid", Session["EmployeeId"]);

                var data = DapperORM.ReturnList<ACCMAIN>("sp_List_ACCMAIN", param).ToList();
                var processedData = data.Select(x => new
                {

                    x.LeadName,
                    x.CompanyName,
                    x.Stage,
                    x.CompanyRating,
                    Encrypted_Id = x.Encrypted_Id
                }).ToList();

                ViewBag.List = processedData;
                //var data = DapperORM.ReturnList<ACCMAIN>("sp_List_ACCMAIN", param).ToList();
                //ViewBag.List = data;

                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "OpportunityList", "OpportunityList", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Lead_Dashboard", "Lead_Dashboard", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

     
        public ActionResult Lead_Details(string EncryptedId)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
                // DapperORM.SetConnection();
                {
                    param.Add("@query", "Select Fid,Service,MAS_SERCATG from MAS_SERVICE where DELETED=0");
                    var ServiceList = DapperORM.ReturnList<MAS_SERVICE>("Sp_QueryExcution", param).ToList();
                    ViewBag.ServiceList = ServiceList;

                    param.Add("@query", "Select Encrypted_Id from ACCMAIN where Stage in ('Identification','Qualification') and DELETED=0");
                    var AllEncryptedIds = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.AllEncryptedIds = AllEncryptedIds;


                    param.Add("@query", "Select Fid as Fid,ServiceCategory from MAS_SERCATG where DELETED=0");
                    var ServiceCatList = DapperORM.ReturnList<MAS_SERCATG>("Sp_QueryExcution", param).ToList();
                    ViewBag.ServiceCatList = ServiceCatList;

                    param.Add("@query", "Select DepartmentId As Fid,DepartmentName As Department from Mas_Department where Deactivate=0");
                    var DepartmentList = DapperORM.ReturnList<MAS_DEPT>("Sp_QueryExcution", param).ToList();
                    ViewBag.DepartmentList = DepartmentList;

                    param.Add("@query", "Select DesignationId As Fid,DesignationName As Designation from Mas_Designation where Deactivate=0");
                    var DesignationList = DapperORM.ReturnList<MAS_DESG>("Sp_QueryExcution", param).ToList();
                    ViewBag.DesignationList = DesignationList;

                    //  param.Add("@query", "Select * from ACCMAIN where Encrypted_Id='" + EncryptedId + "' and DELETED=0");
                    param.Add("@query", " SELECT acc.*, comp.CompanyName FROM ACCMAIN acc LEFT JOIN Mas_CompanyProfile comp ON acc.CompanyName = comp.CompanyId WHERE acc.Encrypted_Id = '" + EncryptedId + "' AND acc.DELETED = 0");
                    var AccMainDetails = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.AccMainDetails = AccMainDetails;
                    // ViewBag.CompanyLogo = AccMainDetails.CompanyLogo;
                    // ViewBag.BUCode = AccMainDetails.BUCode;

                    param.Add("@query", "Select CountryId As Fid,CountryName from Mas_Country where Deactivate=0");
                    var CountryList = DapperORM.ReturnList<MAS_COUNTRY>("Sp_QueryExcution", param).ToList();
                    ViewBag.CountryList = CountryList;

                    param.Add("@query", "Select StateId As Fid,StateName from Mas_States where Deactivate=0");
                    var StateList = DapperORM.ReturnList<MAS_STATE>("Sp_QueryExcution", param).ToList();
                    ViewBag.StateList = StateList;

                    ViewBag.EncryptedId = EncryptedId;

                    param.Add("@query", "select ACCSERVICES.fid as AccFid,ACCSERVICES.Encrypted_Id as Sevice_EncryptedId,accmain.LeadName,MAS_SERCATG.ServiceCategory,MAS_SERCATG.Fid as CategoryFid,MAS_SERVICE.Fid as ServiceFid,MAS_SERVICE.service from ACCSERVICES ACCSERVICES join ACCMAIN accmain on ACCSERVICES.accmain_fid=ACCMAIN.fid join MAS_SERVICE MAS_SERVICE on MAS_SERVICE.fid = ACCSERVICES.mas_service join MAS_SERCATG MAS_SERCATG on MAS_SERVICE.mas_sercatg = MAS_SERCATG.Fid where ACCSERVICES.deleted = 0 and ACCMAIN.Fid='" + AccMainDetails.Fid + "'");
                    //param.Add("@query", "select ACCSERVICES.fid as AccFid,ACCSERVICES.Encrypted_Id as Sevice_EncryptedId,accmain.LeadName,MAS_SERCATG.ServiceCategory,MAS_SERCATG.Fid as CategoryFid,MAS_SERVICE.Fid as ServiceFid,MAS_SERVICE.service from ACCSERVICES ACCSERVICES join ACCMAIN accmain on ACCSERVICES.accmain_fid=ACCMAIN.fid join MAS_SERVICE MAS_SERVICE on MAS_SERVICE.fid = ACCSERVICES.mas_service join MAS_SERCATG MAS_SERCATG on MAS_SERVICE.mas_sercatg = MAS_SERCATG.Fid where ACCSERVICES.deleted = 0 ");
                    var ACCServiceList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.ACCServiceList = ACCServiceList;

                    param.Add("@query", " select contact.IsPrimaryContact, contact.LinkedInProfile,contact.Encrypted_Id as Contact_EncryptedId, ACCMAIN_Fid, contact.ContactPerson, dept.DepartmentName, dept.DepartmentId as DepartmentId, desg.DesignationId as DesignationId, desg.DesignationName, contact.MobileNo, contact.WhatsappNo, contact.EmailId, contact.CountryCode from ACCCONTACT contact join Mas_Department dept on contact.Department = dept.DepartmentId join Mas_Designation desg on contact.Designation = desg.DesignationId where contact.Deleted = 0 and contact.ACCMAIN_Fid='" + AccMainDetails.Fid + "'");
                    //param.Add("@query", " select contact.IsPrimaryContact, contact.LinkedInProfile,contact.Encrypted_Id as Contact_EncryptedId, ACCMAIN_Fid, contact.ContactPerson, dept.DepartmentName, dept.DepartmentId as DepartmentId, desg.DesignationId as DesignationId, desg.DesignationName, contact.MobileNo, contact.WhatsappNo, contact.EmailId, contact.CountryCode from ACCCONTACT contact join Mas_Department dept on contact.Department = dept.DepartmentId join Mas_Designation desg on contact.Designation = desg.DesignationId where contact.Deleted = 0 ");
                    var ACCContactList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.ACCContactList = ACCContactList;

                    param.Add("@query", "select acc.LocationName,acc.Encrypted_Id as Location_EncryptedId,con.CountryName, con.CountryId as CountryId,state.StateName,state.stateId as StateId, acc.Address,acc.Area,acc.IsHeadOffice,acc.City,acc.Pincode from ACCLOCATION acc join Mas_country con on acc.MAS_COUNTRY_Fid = con.CountryId join Mas_States state on acc.MAS_STATE_Fid = state.StateId where acc.ACCMAIN_Fid='" + AccMainDetails.Fid + "' and acc.Deleted=0");
                    //param.Add("@query", "select acc.LocationName,acc.Encrypted_Id as Location_EncryptedId,con.CountryName, con.CountryId as CountryId,state.StateName,state.stateId as StateId, acc.Address,acc.Area,acc.IsHeadOffice,acc.City,acc.Pincode from ACCLOCATION acc join Mas_country con on acc.MAS_COUNTRY_Fid = con.CountryId join Mas_States state on acc.MAS_STATE_Fid = state.StateId where acc.Deleted=0");
                    var ACCLocationList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                    ViewBag.ACCLocationList = ACCLocationList;

                    param.Add("@query", "select Fid as Fid,ContactPerson from ACCCONTACT where ACCMAIN_Fid='" + AccMainDetails.Fid + "' and DELETED=0");
                    //param.Add("@query", "select Fid as Fid,ContactPerson from ACCCONTACT where  DELETED=0");
                    var ContactPersonList = DapperORM.ReturnList<ACCCONTACT>("Sp_QueryExcution", param).ToList();
                    ViewBag.ContactPersonList = ContactPersonList;

                    param.Add("@query", "select Fid as Fid,Stage from MAS_STAGES where origin='Lead' and deleted=0");
                    var StageList = DapperORM.ReturnList<MAS_STAGES>("Sp_QueryExcution", param).ToList();
                    ViewBag.StageList = StageList;

                    param1.Add("@query", " SELECT acc.*,con.Fid contactFid,con.ContactPerson as ContactPersonName FROM ACCACTIVITY ACC JOIN ACCCONTACT CON ON ACC.ACCCONTACT_Fid=con.Fid where acc.Deleted = 0 and con.Deleted = 0 and acc.ACCMAIN_Fid ='" + AccMainDetails.Fid + "'");
                    var ActivitityList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();
                    ViewBag.ActivitityList = ActivitityList;

                    //param.Add("@query", "  Select * from MAs_Employees where EmployeeCategory = 'Operations' and Active =1");
                    param.Add("@query", "  Select * from Mas_Employee where  Deactivate =0");
                    var OpsList = DapperORM.ReturnList<MAS_EMPLOYEES>("Sp_QueryExcution", param).ToList();
                    ViewBag.OpsList = OpsList;

                    param1.Add("@query", "select acccomm.*,con.fid as ConFid,con.ContactPerson as ContactPerson  from ACCCOMM acccomm join ACCCONTACT con on acccomm.ACCCONTACT_Fid = con.Fid where acccomm.Deleted = 0 and acccomm.ACCMAIN_Fid='" + AccMainDetails.Fid + "'");
                    var ACCCommList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();
                    ViewBag.ACCCommList = ACCCommList;

                    param.Add("@query", "select * from ACCDMS where ACCMAIN_Fid='" + AccMainDetails.Fid + "' and DELETED=0");
                    var AddDocuments = DapperORM.ReturnList<ACCDMS>("Sp_QueryExcution", param).ToList();
                    ViewBag.AddDocuments = AddDocuments;

                    param.Add("@query", "Select CountryId As Fid,DialCode As CountryCode from Mas_country where Deactivate=0");
                    var CountryCodeList = DapperORM.ReturnList<MAS_COUNTRY>("Sp_QueryExcution", param).ToList();
                    ViewBag.CountryCodeList = CountryCodeList;

                    var today = DateTime.Today.ToString("yyyy-MM-dd");
                    ViewBag.TodayDate = today;

                    //param.Add("@query", "WITH RankedStages AS (SELECT t2.Stage, t1.*, ROW_NUMBER() OVER (PARTITION BY t1.MAS_STAGES_Fid ORDER BY t1.TimeStamp DESC) AS rn FROM  ACCSTAGES t1 JOIN MAS_STAGES t2 ON t1.MAS_STAGES_Fid = t2.Fid WHERE t1.ACCMAIN_Fid = '" + AccMainDetails.Fid + "' and t1.Deleted=0)SELECT * FROM RankedStages WHERE rn = 1;");
                    //var StagesRecords = DapperORM.ReturnList<ACCSTAGES>("Sp_QueryExcution", param).ToList();
                    //ViewBag.StagesRecords = StagesRecords;

                }
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Lead_Details", "Lead_Details", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Lead_Details", "Lead_Details", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        //public JsonResult IsServiceExist(string Encrypted_Id, int? ServiceId, int? ACCMAIN_Fid)
        //{
        //    try
        //    {
        //        //  DapperORM.SetConnection();
        //        {

        //            param.Add("@p_process", "IsValidation");
        //            //param.Add("@p_Encrypted_Id", Encrypted_Id);
        //            param.Add("@p_MAS_SERVICE", ServiceId);
        //            param.Add("@p_ACCMAIN_Fid", ACCMAIN_Fid);
        //            param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //            param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //            var Result = DapperORM.ExecuteReturn("Sp_SUD_ACCSERVICES", param);
        //            var Message = param.Get<string>("@p_msg");
        //            var Icon = param.Get<string>("@p_Icon");

        //            if (Message != "")
        //            {
        //                return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
        //            }
        //            else
        //            {
        //                return Json(true, JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(false, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public ActionResult SaveLeadService(ACCSERVICES ACCSERVICES, string Acc_EncryptedId)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
                // DapperORM.SetConnection();
                {

                    string process = string.IsNullOrEmpty(ACCSERVICES.Encrypted_Id) ? "Save" : "Update";

                    param.Add("@p_process", process);
                    param.Add("@P_Fid", ACCSERVICES.Fid);
                    param.Add("@P_Encrypted_Id", ACCSERVICES.Encrypted_Id);
                    param.Add("@p_ACCMAIN_Fid", ACCSERVICES.ACCMAIN_Fid);
                    param.Add("@p_BUCode", ACCSERVICES.BUCode);
                    param.Add("@p_MAS_SERVICE", ACCSERVICES.MAS_SERVICE);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_Mip", Dns.GetHostByName(MachineId).AddressList[0].ToString());
                    param.Add("@p_UserId", Session["EmployeeId"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var data = DapperORM.ExecuteReturn("Sp_SUD_ACCSERVICES", param);
                    var Message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@query", "select ACCSERVICES.fid as AccFid,ACCSERVICES.Encrypted_Id as Sevice_EncryptedId,accmain.LeadName,MAS_SERCATG.ServiceCategory,MAS_SERCATG.Fid as CategoryFid,MAS_SERVICE.Fid as ServiceFid,MAS_SERVICE.service from ACCSERVICES ACCSERVICES join ACCMAIN accmain on ACCSERVICES.accmain_fid=ACCMAIN.fid join MAS_SERVICE MAS_SERVICE on MAS_SERVICE.fid = ACCSERVICES.mas_service join MAS_SERCATG MAS_SERCATG on MAS_SERVICE.mas_sercatg = MAS_SERCATG.Fid where ACCSERVICES.deleted = 0 and ACCMAIN.Fid='" + ACCSERVICES.ACCMAIN_Fid + "'");
                    var ACCServiceList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();

                    LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "SaveLeadService", "SaveLeadService", "Created", "Passed", "", _clientIp, _clientMachineName);

                    return Json(new { ACCServiceList, Message, Icon }, JsonRequestBehavior.AllowGet);

                    //return RedirectToAction("Lead_Details", "CRM", new { EncryptedId = Acc_EncryptedId });
                }
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "SaveLeadService", "SaveLeadService", "Created", "Failed", ex.Message, _clientIp, _clientMachineName);

                return new HttpStatusCodeResult(500, "An internal server error occurred: " + ex.Message);

            }
        }

        [HttpGet]
        public ActionResult DeleteService(string EncryptedId, string Acc_EncryptedId, string Identity)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                //  DapperORM.SetConnection();
                {
                    param.Add("@p_process", "Delete");
                    param.Add("@p_Encrypted_Id", EncryptedId);
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parame
                    var Result = DapperORM.ExecuteReturn("Sp_SUD_ACCSERVICES", param);
                    var message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    TempData["Message"] = message;
                    TempData["Icon"] = Icon.ToString();

                    LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "DeleteService", "DeleteService", "Deleted", "Passed", "", _clientIp, _clientMachineName);

                    return RedirectToAction(Identity, "CRM", new { EncryptedId = Acc_EncryptedId });
                }
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "DeleteService", "DeleteService", "Deleted", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        [HttpPost]
        public ActionResult SaveLeadContact(ACCCONTACT ACCCONTACT, string Acc_EncryptedId, string IsRedirect)
        {
     
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
                // DapperORM.SetConnection();
                {

                    DynamicParameters param2 = new DynamicParameters();

                    param2.Add("@p_process", "IsValidation");
                    param2.Add("@P_Encrypted_Id", ACCCONTACT.Encrypted_Id);
                    param2.Add("@p_MobileNo", ACCCONTACT.MobileNo);
                    param2.Add("@p_EmailId", ACCCONTACT.EmailId);
                    param2.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param2.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var data1 = DapperORM.ExecuteReturn("Sp_SUD_ACCCONTACT", param2);
                    var Message = param2.Get<string>("@p_msg");
                    var Icon = param2.Get<string>("@p_Icon");

                    if (Message == "")
                    {
                        string process = string.IsNullOrEmpty(ACCCONTACT.Encrypted_Id) ? "Save" : "Update";
                        param.Add("@p_process", process);
                        param.Add("@P_Fid", ACCCONTACT.Fid);
                        param.Add("@P_Encrypted_Id", ACCCONTACT.Encrypted_Id);
                        param.Add("@p_ACCMAIN_Fid", ACCCONTACT.ACCMAIN_Fid);
                        param.Add("@p_BUCode", ACCCONTACT.BUCode);
                        param.Add("@p_ContactPerson", ACCCONTACT.ContactPerson);
                        param.Add("@p_MobileNo", ACCCONTACT.MobileNo);
                        param.Add("@p_WhatsappNo", ACCCONTACT.WhatsappNo);
                        param.Add("@p_EmailId", ACCCONTACT.EmailId);
                        param.Add("@p_Designation", ACCCONTACT.Designation);
                        param.Add("@p_Department", ACCCONTACT.Department);
                        param.Add("@p_LinkedInProfile", ACCCONTACT.LinkedInProfile);
                        param.Add("@p_CountryCode", ACCCONTACT.CountryCode);
                        param.Add("@p_IsPrimaryContact", ACCCONTACT.IsPrimaryContact);
                        param.Add("@p_MachineName", Dns.GetHostName().ToString());
                        param.Add("@p_UserId", Session["EmployeeId"]);
                        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                        param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                        var data = DapperORM.ExecuteReturn("Sp_SUD_ACCCONTACT", param);
                        Message = param.Get<string>("@p_msg");
                        Icon = param.Get<string>("@p_Icon");

                        var ContactFid = param.Get<string>("@p_Id");
                        DynamicParameters param3 = new DynamicParameters();

                        //  sqlcon.Query("Update ACCSITES set ACCCONTACT_Fid= " + ContactFid + " where ACCSITES.Fid=" + ACCCONTACT.Sitelist + "");


                    }
                    //if (IsRedirect == "True")
                    //{
                    //    return RedirectToAction("Lead_Dashboard", "CRM", new { area = "CRMS" });
                    //}
                    //if (IsRedirect == "Opportunity")
                    //{
                    //    return RedirectToAction("OpportunityList", "CRM", new { area = "CRMS" });
                    //}
                    param1.Add("@query", "select contact.IsPrimaryContact, contact.LinkedInProfile,contact.Encrypted_Id as Contact_EncryptedId,ACCMAIN_Fid,contact.ContactPerson,dept.DepartmentName,dept.DepartmentId as DepartmentId, desg.DesignationId as DesignationId, desg.DesignationName,contact.MobileNo, contact.WhatsappNo, contact.EmailId, contact.CountryCode from ACCCONTACT contact join Mas_Department dept on contact.Department = dept.DepartmentId  join Mas_Designation desg on contact.Designation = desg.DesignationId where contact.Deleted = 0 and contact.ACCMAIN_Fid='" + ACCCONTACT.ACCMAIN_Fid + "'");
                    var ACCContactList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();

                    LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "SaveLeadContact", "SaveLeadContact", "View", "Passed", "", _clientIp, _clientMachineName);

                    return Json(new { ACCContactList, Message, Icon }, JsonRequestBehavior.AllowGet);
                    //return RedirectToAction("Lead_Details", "CRM", new { EncryptedId = Acc_EncryptedId });
                }
            }
            catch (Exception ex)
            {
              LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "SaveLeadContact", "SaveLeadContact", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return new HttpStatusCodeResult(500, "An internal server error occurred: " + ex.Message);
            }
        }

        public ActionResult DeleteContact(string EncryptedId, string Acc_EncryptedId, string Identity)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
                //  DapperORM.SetConnection();
                {
                    param.Add("@p_process", "Delete");
                    param.Add("@p_Encrypted_Id", EncryptedId);
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parame
                    var Result = DapperORM.ExecuteReturn("Sp_SUD_ACCCONTACT", param);
                    var message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    TempData["Message"] = message;
                    TempData["Icon"] = Icon;

                    if (Acc_EncryptedId == null)
                    {
                        return RedirectToAction(Identity, "CRM", new { area = "CRMS" });
                    }
                  LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "DeleteContact", "DeleteContact", "Deleted", "Passed", "", _clientIp, _clientMachineName);

                    return RedirectToAction(Identity, "CRM", new { EncryptedId = Acc_EncryptedId });
                }
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "DeleteContact", "DeleteContact", "Deleted", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        [ValidateInput(false)]
        [HttpPost]
        //public ActionResult SaveLeadCommunication(ACCCOMM ACCCOMM, string Acc_EncryptedId)
        public ActionResult SaveLeadCommunication(ACCCOMM ACCCOMM, string Acc_EncryptedId)
        {

            //string IsRedirect
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
                //  DapperORM.SetConnection();
                {
                    string process = string.IsNullOrEmpty(ACCCOMM.Encrypted_Id) ? "Save" : "Update";

                    param.Add("@p_process", process);
                    param.Add("@P_Fid", ACCCOMM.Fid);
                    param.Add("@P_Encrypted_Id", ACCCOMM.Encrypted_Id);
                    param.Add("@P_ACCMAIN_Fid", ACCCOMM.ACCMAIN_Fid);
                    param.Add("@p_BUCode", ACCCOMM.BUCode);
                    param.Add("@p_ACCCONTACT_Fid", ACCCOMM.ACCCONTACT_Fid);
                    param.Add("@p_CommunicationDate", ACCCOMM.CommunicationDate);
                    param.Add("@p_CommunicationType", ACCCOMM.CommunicationType);
                    param.Add("@p_Description", ACCCOMM.Description);
                    param.Add("@p_NextFollowUpDate", ACCCOMM.NextFollowUpDate);
                    param.Add("@p_FollowUpRemark", ACCCOMM.FollowUpRemark);
                    param.Add("@p_FollowUpStatus", ACCCOMM.FollowUpStatus);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_Mip", Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString());
                    param.Add("@p_UserId", Session["EmployeeId"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var data = DapperORM.ExecuteReturn("Sp_SUD_ACCCOMM", param);
                    var Message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");

                    //if (IsRedirect == "True")
                    //{
                    //    return RedirectToAction("Lead_Dashboard", "CRM", new { area = "CRMS" });
                    //}
                    //if (IsRedirect == "Opportunity")
                    //{
                    //    return RedirectToAction("OpportunityList", "CRM", new { area = "CRMS" });
                    //}
                    param1.Add("@query", "select acccomm.*,con.fid as ConFid,con.ContactPerson as ContactPerson  from ACCCOMM acccomm join ACCCONTACT con on acccomm.ACCCONTACT_Fid = con.Fid where acccomm.Deleted = 0 and acccomm.ACCMAIN_Fid='" + ACCCOMM.ACCMAIN_Fid + "'");
                    var ACCCommList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();

                 LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "SaveLeadCommunication", "SaveLeadCommunication", "Created", "Passed", "", _clientIp, _clientMachineName);

                    return Json(new { ACCCommList, Icon, Message }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "SaveLeadCommunication", "SaveLeadCommunication", "Created", "Failed", ex.Message, _clientIp, _clientMachineName);

                return new HttpStatusCodeResult(500, "An internal server error occurred: " + ex.Message);
            }
        }

        public ActionResult SaveLeadLocation(ACCLOCATION ACCLOCATION, string Acc_EncryptedId, string IsRedirect)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
                if (ACCLOCATION.Encrypted_Id == null)
                {
                    ACCLOCATION.Encrypted_Id = "";
                }
                // DapperORM.SetConnection();
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_Encrypted_Id", ACCLOCATION.Encrypted_Id);
                    param.Add("@p_LocationName", ACCLOCATION.LocationName);
                    param.Add("@p_ACCMAIN_Fid", ACCLOCATION.ACCMAIN_Fid);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("Sp_SUD_ACCLOCATION", param);
                    var Message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    if (Message != "")
                    {
                        param1.Add("@query", "select acc.LocationName,acc.Encrypted_Id as Location_EncryptedId,con.CountryName,con.CountryId as CountryId, state.StateName, state.StateId as StateId, acc.Address, acc.IsHeadOffice, acc.City, acc.Area, acc.Pincode from ACCLOCATION acc join Mas_Country con on acc.MAS_COUNTRY_Fid = con.CountryId join Mas_States state on acc.MAS_STATE_Fid = state.StateId where acc.ACCMAIN_Fid='" + ACCLOCATION.ACCMAIN_Fid + "' and acc.Deleted=0");
                        var ACCLocationList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();
                        return Json(new { ACCLocationList, Icon, Message }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        param1.Add("@query", $"select CountryId AS Fid from Mas_Country where CountryName='{ACCLOCATION.MAS_COUNTRY_Fid1}' and Deactivate=0");
                        var CountryId = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).FirstOrDefault();

                        param1.Add("@query", $"select StateId As Fid from Mas_States where StateName='{ACCLOCATION.MAS_STATE_Fid1}' and Deactivate=0");
                        var StateId = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).FirstOrDefault();

                        //if (CountryId == null)
                        //{
                        //    Message = "Country Not Matching";
                        //    Icon = "error";
                        //}
                        if (StateId == null)
                        {
                            Message = "State Not Matching";
                            Icon = "error";
                        }

                        string process = string.IsNullOrEmpty(ACCLOCATION.Encrypted_Id) ? "Save" : "Update";
                        param.Add("@p_process", process);
                        param.Add("@P_Fid", ACCLOCATION.Fid);
                        param.Add("@P_Encrypted_Id", ACCLOCATION.Encrypted_Id);
                        param.Add("@P_ACCMAIN_Fid", ACCLOCATION.ACCMAIN_Fid);
                        param.Add("@p_BUCode", ACCLOCATION.BUCode);
                        param.Add("@p_Pincode", ACCLOCATION.Pincode);
                        param.Add("@P_LocationName", ACCLOCATION.LocationName);
                        param.Add("@P_Address", ACCLOCATION.Address);
                        param.Add("@P_Area", ACCLOCATION.Area);
                         param.Add("@P_MAS_COUNTRY_Fid", CountryId.Fid);
                        param.Add("@P_MAS_STATE_Fid", StateId.Fid);
                        param.Add("@P_IsHeadOffice", ACCLOCATION.IsHeadOffice);
                        param.Add("@P_City", ACCLOCATION.City);
                        param.Add("@p_MachineName", Dns.GetHostName().ToString());
                        param.Add("@p_Mip", Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString());
                        param.Add("@p_UserId", Session["EmployeeId"]);
                        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                        var data = DapperORM.ExecuteReturn("Sp_SUD_ACCLOCATION", param);
                        Message = param.Get<string>("@p_msg");
                        Icon = param.Get<string>("@p_Icon");

                        //if (IsRedirect == "True")
                        //{
                        //    return RedirectToAction("Lead_Dashboard", "CRM", new { area = "CRMS" });
                        //}
                        //if (IsRedirect == "Opportunity")
                        //{
                        //    return RedirectToAction("OpportunityList", "CRM", new { area = "CRMS" });
                        //}
                        param1.Add("@query", "select acc.LocationName,acc.Encrypted_Id as Location_EncryptedId,con.CountryName, con.CountryId as CountryId,state.StateName, state.StateId as StateId, acc.Address, acc.IsHeadOffice, acc.City,acc.Area, acc.Pincode from ACCLOCATION acc join Mas_Country con on acc.MAS_COUNTRY_Fid = con.CountryId join Mas_States state on acc.MAS_STATE_Fid = state.StateId  where acc.ACCMAIN_Fid='" + ACCLOCATION.ACCMAIN_Fid + "' and acc.Deleted=0");
                        var ACCLocationList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();
                         LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "SaveLeadLocation", "SaveLeadLocation", "Created", "Passed", "", _clientIp, _clientMachineName);

                        return Json(new { ACCLocationList, Icon, Message }, JsonRequestBehavior.AllowGet);
                    }
                }

            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "SaveLeadLocation", "SaveLeadLocation", "Created", "Failed", ex.Message, _clientIp, _clientMachineName);

                return new HttpStatusCodeResult(500, "An internal server error occurred: " + ex.Message);
            }
        }

        public ActionResult GetCountAll(int Fid)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
                // DapperORM.SetConnection();
                {
                    param.Add("@query", $"select COUNT(ACCSERVICES.Fid) AS ServiceCount from ACCSERVICES  join ACCMAIN accmain on ACCSERVICES.accmain_fid=ACCMAIN.fid join MAS_SERVICE MAS_SERVICE on MAS_SERVICE.fid = ACCSERVICES.mas_service join MAS_SERCATG MAS_SERCATG on MAS_SERVICE.mas_sercatg = MAS_SERCATG.Fid where ACCSERVICES.deleted = 0 and ACCMAIN.Fid='{Fid}'");
                    var ServiceCount = DapperORM.ReturnList<ACCSERVICES>("Sp_QueryExcution", param).FirstOrDefault();
                    param.Add("@query", $"select COUNT(contact.Fid) AS ContactCount from ACCCONTACT contact join Mas_Department dept on contact.Department=dept.DepartmentId join Mas_Designation desg on contact.Designation=desg.DesignationId where contact.Deleted=0 and contact.ACCMAIN_Fid='{Fid}'");
                    var ContactCount = DapperORM.ReturnList<ACCCONTACT>("Sp_QueryExcution", param).FirstOrDefault();
                    param.Add("@query", $"select COUNT(acc.Fid) AS LocationCount from ACCLOCATION acc join Mas_country con on acc.MAS_COUNTRY_Fid=con.CountryId join Mas_states state on acc.MAS_STATE_Fid = state.StateId where acc.ACCMAIN_Fid='" + Fid + "' and acc.Deleted=0");
                    var LocationCount = DapperORM.ReturnList<ACCLOCATION>("Sp_QueryExcution", param).FirstOrDefault();
                    param.Add("@query", $"SELECT COUNT(ACC.Fid) AS ActivityCount FROM ACCACTIVITY ACC JOIN ACCCONTACT CON ON ACC.ACCCONTACT_Fid = con.Fid WHERE acc.Deleted = 0 AND con.Deleted = 0 AND acc.ACCMAIN_Fid = '{Fid}'");
                    var ActivityCount = DapperORM.ReturnList<ACCACTIVITY>("Sp_QueryExcution", param).FirstOrDefault();
                    param.Add("@query", $"select COUNT(acccomm.Fid) AS CommunicationCount from ACCCOMM acccomm join ACCCONTACT con on acccomm.ACCCONTACT_Fid = con.Fid where acccomm.Deleted = 0 and acccomm.ACCMAIN_Fid='" + Fid + "'");
                    var CommunicationCount = DapperORM.ReturnList<ACCCOMM>("Sp_QueryExcution", param).FirstOrDefault();
                    var obj = new
                    {
                        ServiceCount = ServiceCount,
                        ContactCount = ContactCount,
                        LocationCount = LocationCount,
                        ActivityCount = ActivityCount,
                        CommunicationCount = CommunicationCount,
                    };

                  LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetCountAll", "GetCountAll", "View", "Passed", "", _clientIp, _clientMachineName);

                    return Json(obj, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetCountAll", "GetCountAll", "View", "Passed", "", _clientIp, _clientMachineName);

                return new HttpStatusCodeResult(500, "An internal server error occurred: " + ex.Message);
            }
        }

        public ActionResult GetStates(int txtPincode)
        {
            try
            {
                //DapperORM.SetConnection();
                {
                    if (Session["EmployeeId"] == null)
                    {
                        return RedirectToAction("Login", "Login", new { area = "" });
                    }
                    param.Add("@query", $"Select * from Mas_PinCode where Deactivate = 0 and PinCode = {txtPincode}");
                  //  param.Add("@query", $"Select * from PINCODES where Deleted = 0 and PIN = {txtPincode}");

                    var AreaList = DapperORM.ReturnList<PINCODES>("Sp_QueryExcution", param).ToList();
                    var obj = new
                    {
                        AreaList = AreaList,
                    };
                    return Json(obj, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, "An internal server error occurred: " + ex.Message);
            }
        }


        public ActionResult GetContractProp(int Fid)
        {
            try
            {
                // DapperORM.SetConnection();
                {
                    if (Session["EmployeeId"] == null)
                    {
                        return RedirectToAction("Login", "Login", new { area = "" });
                    }
                    param.Add("@query", $"Select * from ACCPROPOSAL where ACCMAIN_Fid='{Fid}' and deleted=0 ORDER BY [Fid] DESC");
                    var Proposal = DapperORM.ReturnList<ACCPROPOSAL>("Sp_QueryExcution", param).FirstOrDefault();

                    param.Add("@query", $"select acc.*  from ACCLOCATION acc join Mas_country con on acc.MAS_COUNTRY_Fid = con.CountryId join Mas_States state on acc.MAS_STATE_Fid = state.StateId where acc.ACCMAIN_Fid ={ Fid} and acc.Deleted = 0");
                    var Location = DapperORM.ReturnList<ACCLOCATION>("Sp_QueryExcution", param).ToList();
                    var obj = new
                    {
                        Proposal = Proposal,
                        Location = Location
                    };
                   LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetContractProp", "GetContractProp", "View", "Passed", "", _clientIp, _clientMachineName);

                    return Json(obj, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetContractProp", "GetContractProp", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return new HttpStatusCodeResult(500, "An internal server error occurred: " + ex.Message);
            }
        }

        public ActionResult GetConPersonList(string EncryptedId)
        {
            try
            {
                //DapperORM.SetConnection();
                {
                    if (Session["EmployeeId"] == null)
                    {
                        return RedirectToAction("Login", "Login", new { area = "" });
                    }
                    param.Add("@query", "Select * from ACCMAIN where Encrypted_Id='" + EncryptedId + "' and DELETED=0");
                    var AccMainDetails = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).FirstOrDefault();

                    param.Add("@query", "select Fid as Fid,ContactPerson from ACCCONTACT where ACCMAIN_Fid='" + AccMainDetails.Fid + "' and DELETED=0");
                    var ContactPersonList = DapperORM.ReturnList<ACCCONTACT>("Sp_QueryExcution", param).ToList();

                    var obj = new
                    {
                        ContactPersonList = ContactPersonList,
                    };
                    LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetConPersonList", "GetConPersonList", "View", "Passed", "", _clientIp, _clientMachineName);

                    return Json(obj, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetConPersonList", "GetConPersonList", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return new HttpStatusCodeResult(500, "An internal server error occurred: " + ex.Message);
            }
        }


        public ActionResult GetCSC2(string txtAddress, int pincode)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@query", $"Select Mas_Country.CountryName as Country ,StateName,OfficeName from Mas_PinCode inner join Mas_Country on Mas_Country.CountryId=Mas_PinCode.CountryId where Mas_PinCode.Deactivate = 0 and OfficeName = '{txtAddress}' and PinCode = '{pincode}'");
                var CSCList = DapperORM.ReturnList<PINCODES>("Sp_QueryExcution", param).ToList();

                var obj = new
                {
                    CSCList = CSCList,
                };
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetCSC2", "GetCSC2", "View", "Passed", "", _clientIp, _clientMachineName);

                return Json(obj, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetCSC2", "GetCSC2", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return new HttpStatusCodeResult(500, "An internal server error occurred: " + ex.Message);
            }
        }

        public ActionResult DeleteLocation(string EncryptedId, string Acc_EncryptedId, string Identity)
        {
            try
            {
                // DapperORM.SetConnection();
                {
                    if (Session["EmployeeId"] == null)
                    {
                        return RedirectToAction("Login", "Login", new { area = "" });
                    }
                    param.Add("@p_process", "Delete");
                    param.Add("@p_Encrypted_Id", EncryptedId);
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parame
                   var Result = DapperORM.ExecuteReturn("Sp_SUD_ACCLOCATION", param);
                    var message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    TempData["Message"] = message;
                    TempData["Icon"] = Icon;

                    if (Acc_EncryptedId == null)
                    {
                        return RedirectToAction(Identity, "CRM", new { area = "CRMS" });
                    }
                    LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "DeleteLocation", "DeleteLocation", "Deleted", "Passed", "", _clientIp, _clientMachineName);

                    return RedirectToAction(Identity, "CRM", new { EncryptedId = Acc_EncryptedId });
                }
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "DeleteLocation", "DeleteLocation", "Deleted", "Failed", ex.Message, _clientIp, _clientMachineName);
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult SaveLeadActivity(ACCACTIVITY ACCACTIVITY, string Acc_EncryptedId, HttpPostedFileBase Attachment, string IsRedirect)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                string process = string.IsNullOrEmpty(ACCACTIVITY.Encrypted_Id) ? "Save" : "Update";
                var param = new DynamicParameters();
                param.Add("@p_process", process);
                param.Add("@p_Fid", ACCACTIVITY.Fid);
                //param.Add("@p_Encrypted_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 70);
                param.Add("@p_Encrypted_Id", ACCACTIVITY.Encrypted_Id);
                param.Add("@P_ACCMAIN_Fid", ACCACTIVITY.ACCMAIN_Fid);
                param.Add("@p_BUCode", ACCACTIVITY.BUCode);
                param.Add("@P_ACCCONTACT_Fid", ACCACTIVITY.ACCCONTACT_Fid);
                //var origin = "Lead";
                param.Add("@P_Origin", ACCACTIVITY.Origin);
                param.Add("@P_ActivityType", ACCACTIVITY.ActivityType);
                param.Add("@P_Description", ACCACTIVITY.Description);
                param.Add("@P_ActivityDate", ACCACTIVITY.ActivityDate);
                param.Add("@P_Status", ACCACTIVITY.Status);
                param.Add("@P_ShareAcctEmployee", ACCACTIVITY.ShareAcctEmployee);
                param.Add("@P_ShareAcctivityToOperation", ACCACTIVITY.ShareAcctivityToOperation);
                param.Add("@P_TodoCheck", ACCACTIVITY.TodoCheck);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_Mip", Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString());
                param.Add("@p_UserId", Session["EmployeeId"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 10);
                param.Add("@p_EncryptedId", dbType: DbType.String, direction: ParameterDirection.Output, size: 70);
                var data = DapperORM.ExecuteReturn("Sp_SUD_ACCACTIVITY", param);
                var Message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                if (Attachment != null)
                {
                    string ENCRYPTED = null;
                    int Id = 0;
                    if (ACCACTIVITY.Encrypted_Id != null)
                    {
                        ENCRYPTED = ACCACTIVITY.Encrypted_Id;
                        DynamicParameters param3 = new DynamicParameters();

                        param3.Add("@query", "select Fid from ACCACTIVITY where Encrypted_Id='" + ACCACTIVITY.Encrypted_Id + "' and deleted=0");
                        var CheckId = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param3).FirstOrDefault();

                        Id = Convert.ToInt32(CheckId.Fid);
                    }
                    else
                    {
                        ENCRYPTED = param.Get<string>("@p_EncryptedId");

                        DynamicParameters param3 = new DynamicParameters();

                        param3.Add("@query", "select Fid from ACCACTIVITY where Encrypted_Id='" + ENCRYPTED + "' and deleted=0");
                        var CheckId = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param3).FirstOrDefault();

                        Id = Convert.ToInt32(CheckId.Fid);
                    }


                    //   var GetDocPath = sqlcon.QuerySingle("Select DocumentURL from SET_DOCURLDRIVE");
                    //  var GetFirstPath = GetDocPath.DocumentURL;

                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='SalesForce'");
                    var GetFirstPath = GetDocPath.DocInitialPath;

                    var FirstPath = Path.Combine(GetFirstPath, "CRM", "Lead", "Activity", Convert.ToString(Id));

                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }

                    if (Attachment != null)
                    {
                        string ImgUploadFrontPageFilePath = Path.Combine(FirstPath, Attachment.FileName);
                        Attachment.SaveAs(ImgUploadFrontPageFilePath);
                        sqlcon.Query("update ACCACTIVITY set Attachment='" + ImgUploadFrontPageFilePath + "' where Encrypted_Id='" + ENCRYPTED + "'");
                        sqlcon.Query($"INSERT INTO ACCDMS (AttachmentURL, Origin, ACCMAIN_Fid, Stage,Deleted) VALUES ('" + ImgUploadFrontPageFilePath + "', 'Activity', '" + Id + "','" + ACCACTIVITY.Mid + "',0)");

                    }

                }

                var param1 = new DynamicParameters();
                param1.Add("@query", $"SELECT acc.*,con.Fid contactFid, con.ContactPerson as ContactPersonName FROM ACCACTIVITY ACC JOIN ACCCONTACT CON ON ACC.ACCCONTACT_Fid = con.Fid WHERE acc.Deleted = 0 AND con.Deleted = 0 AND acc.ACCMAIN_Fid = '{ACCACTIVITY.ACCMAIN_Fid}'");
                var ACCActivityList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();
                if (IsRedirect == "True")
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                if (IsRedirect == "Opportunity")
                {
                    return RedirectToAction("OpportunityList", "CRM", new { area = "CRMS" });
                }
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "SaveLeadActivity", "SaveLeadActivity", "View", "Passed", "", _clientIp, _clientMachineName);

                return Json(new { ACCActivityList, Message, Icon }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "SaveLeadActivity", "SaveLeadActivity", "View", "Failed", ex.Message, _clientIp, _clientMachineName);
                return new HttpStatusCodeResult(500, "An internal server error occurred: " + ex.Message);
            }
        }

        public ActionResult DeleteActivity(string EncryptedId, string Acc_EncryptedId, string Identity)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                //  DapperORM.SetConnection();
                {
                    param.Add("@p_process", "Delete");
                    param.Add("@p_Encrypted_Id", EncryptedId);
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parame
                    var Result = DapperORM.ExecuteReturn("Sp_SUD_ACCACTIVITY", param);
                    var message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    TempData["Message"] = message;
                    TempData["Icon"] = Icon;

                    if (Acc_EncryptedId == null)
                    {
                        return RedirectToAction(Identity, "CRM", new { area = "CRMS" });
                    }
                    LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "DeleteActivity", "DeleteActivity", "Deleted", "Passed", "", _clientIp, _clientMachineName);

                    return RedirectToAction(Identity, "CRM", new { EncryptedId = Acc_EncryptedId });
                }
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "DeleteActivity", "DeleteActivity", "Deleted", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult GetViewFile(string FilePath)
        {
            //  DapperORM.SetConnection();
            string fileName = Path.GetFileName(FilePath);
            string mimeType = MimeMapping.GetMimeMapping(fileName);

            // Return the file with its original name
            return File(FilePath, mimeType, fileName);
        }

        public ActionResult AddLeadContact()
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Logout", "Home", new { area = "" });
                }
                // DapperORM.SetConnection();
                {
                    var UserId = Session["EmployeeId"];
                    //   var mainRole = Session["MainRole"].ToString();
                    //param.Add("@query", $"Select Fid,CompanyName from ACCMAIN where ('admin' = '{mainRole}' or 'salesadmin' = '{mainRole}' or UserId = '{UserId}') and Deleted=0 AND (Stage='Identification' or Stage='Qualification')");
                    param.Add("@query", $"Select ACCMAIN.Fid,Mas_CompanyProfile.CompanyName from ACCMAIN Left JOIN Mas_CompanyProfile ON ACCMAIN.CompanyName=Mas_CompanyProfile.CompanyId  where ( UserId = '{UserId}') and Deleted=0 AND (Stage='Identification' or Stage='Qualification')");

                    var CompanyList = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).ToList();
                    ViewBag.CompanyList = CompanyList;

                    param.Add("@query", "Select DepartmentId As Fid,DepartmentName As Department from Mas_Department where Deactivate=0");
                    var DepartmentList = DapperORM.ReturnList<MAS_DEPT>("Sp_QueryExcution", param).ToList();
                    ViewBag.DepartmentList = DepartmentList;

                    param.Add("@query", "Select DesignationId As Fid ,DesignationName As Designation from Mas_Designation where Deactivate=0");
                    var DesignationList = DapperORM.ReturnList<MAS_DESG>("Sp_QueryExcution", param).ToList();
                    ViewBag.DesignationList = DesignationList;

                    param.Add("@query", "Select CountryId As Fid,DialCode As CountryCode from Mas_Country where Deactivate=0");
                    var CountryCodeList = DapperORM.ReturnList<MAS_COUNTRY>("Sp_QueryExcution", param).ToList();
                    ViewBag.CountryCodeList = CountryCodeList;



                    LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AddLeadContact", "AddLeadContact", "View", "Passed", "", _clientIp, _clientMachineName);

                    return View();
                }
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AddLeadContact", "AddLeadContact", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult AddLeadLocation()
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
                // DapperORM.SetConnection();
                {
                    var UserId = Session["EmployeeId"];
                    //var mainRole = Session["MainRole"].ToString();
                    //param.Add("@query", $"Select Fid,CompanyName from ACCMAIN where Deleted=0 AND (Stage='Identification' or Stage='Qualification')");
                    param.Add("@query", $"Select ACCMAIN.Fid,Mas_CompanyProfile.CompanyName from ACCMAIN Left JOIN Mas_CompanyProfile ON ACCMAIN.CompanyName=Mas_CompanyProfile.CompanyId where Deleted=0 AND (Stage='Identification' or Stage='Qualification')");
                    var CompanyList = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).ToList();
                    ViewBag.CompanyList = CompanyList;

                    param.Add("@query", "Select Fid,Area,City from PINCODES where Deleted=0 ");
                    //param.Add("@query", "Select PinId As Fid,OfficeName AS Area from Mas_PinCode where Deactivate=0 ");
                    var AreaList = DapperORM.ReturnList<PINCODES>("Sp_QueryExcution", param).ToList();
                    ViewBag.AreaList = AreaList;

                    param.Add("@query", "Select CountryId As Fid,CountryName from Mas_Country where Deactivate=0");
                    var CountryList = DapperORM.ReturnList<MAS_COUNTRY>("Sp_QueryExcution", param).ToList();
                    ViewBag.CountryList = CountryList;

                    param.Add("@query", "Select StateId As Fid,StateName from Mas_States where Deactivate=0");
                    var StateList = DapperORM.ReturnList<MAS_STATE>("Sp_QueryExcution", param).ToList();
                    ViewBag.StateList = StateList;
                    LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AddLeadLocation", "AddLeadLocation", "View", "Passed", "", _clientIp, _clientMachineName);

                    return View();
                }
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AddLeadLocation", "AddLeadLocation", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult AddLeadCommunication()
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                //  DapperORM.SetConnection();
                {
                    var UserId = Session["EmployeeId"];
                    //var mainRole = Session["MainRole"].ToString();
                    //param.Add("@query", $"Select Fid,CompanyName from ACCMAIN where ('admin' = '{mainRole}' or 'salesadmin' = '{mainRole}' or UserId = '{UserId}') and Deleted=0 AND (Stage='Identification' or Stage='Qualification')");
                    param.Add("@query", $"Select ACCMAIN.Fid,Mas_CompanyProfile.CompanyName from ACCMAIN Left JOIN Mas_CompanyProfile ON ACCMAIN.CompanyName=Mas_CompanyProfile.CompanyId where UserId = '{UserId}' and Deleted=0 AND (Stage='Identification' or Stage='Qualification')");
                    var CompanyList = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).ToList();
                    ViewBag.CompanyList = CompanyList;

                    param.Add("@query", "select Fid as Fid,ContactPerson from ACCCONTACT where Deleted=0");
                    var ContactPersonList = DapperORM.ReturnList<ACCCONTACT>("Sp_QueryExcution", param).ToList();
                    ViewBag.ContactPersonList = ContactPersonList;

                    LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AddLeadCommunication", "AddLeadCommunication", "View", "Passed", "", _clientIp, _clientMachineName);

                    return View();
                }
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AddLeadCommunication", "AddLeadCommunication", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        [HttpGet]
        public ActionResult GetConPerson(int txtACCMAIN_Fid)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var param = new DynamicParameters();
                var param1 = new DynamicParameters();

                param.Add("@query", $"Select * from ACCCONTACT where deleted = 0 and ACCMAIN_Fid = '{txtACCMAIN_Fid}'");
                var EmployeeList = DapperORM.ReturnList<ACCCONTACT>("Sp_QueryExcution", param).ToList();

                param.Add("@query", $"Select BUCode from ACCMAIN where Fid = '{txtACCMAIN_Fid}'");
                var BuCode = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).FirstOrDefault();

                param.Add("@query", "select contact.LinkedInProfile,contact.Encrypted_Id as Contact_EncryptedId,ACCMAIN_Fid,contact.ContactPerson,dept.DepartmentName As Department,dept.DepartmentId as DepartmentId,desg.DesignationId as DesignationId,desg.DesignationName As Designation,contact.MobileNo,contact.WhatsappNo,contact.EmailId,contact.CountryCode from ACCCONTACT contact join Mas_Department dept on contact.Department=dept.DepartmentId join Mas_Designation desg on contact.Designation=desg.DesignationId where contact.Deleted=0 and contact.ACCMAIN_Fid='" + txtACCMAIN_Fid + "'");
                var ACCContactList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();

                param.Add("@query", "select acc.LocationName,acc.Encrypted_Id as Location_EncryptedId,con.CountryName, con.CountryId as CountryId,state.StateName,state.StateId as StateId, acc.Address,acc.IsHeadOffice,acc.City,acc.Area,acc.Pincode from ACCLOCATION acc join Mas_Country con on acc.MAS_COUNTRY_Fid=con.CountryId join Mas_States state on acc.MAS_STATE_Fid = state.StateId where acc.ACCMAIN_Fid='" + txtACCMAIN_Fid + "' and acc.Deleted=0");
                var ACCLocationList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();

                param1.Add("@query", "select acccomm.*,con.fid as ConFid,con.ContactPerson as ContactPerson  from ACCCOMM acccomm join ACCCONTACT con on acccomm.ACCCONTACT_Fid = con.Fid where acccomm.Deleted = 0 and acccomm.ACCMAIN_Fid='" + txtACCMAIN_Fid + "'");
                var ACCCommList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();

                param1.Add("@query", $"SELECT acc.*,con.Fid contactFid, con.ContactPerson as ContactPersonName FROM ACCACTIVITY ACC JOIN ACCCONTACT CON ON ACC.ACCCONTACT_Fid = con.Fid WHERE acc.Deleted = 0 AND con.Deleted = 0 AND acc.ACCMAIN_Fid = '{txtACCMAIN_Fid}'");
                var ACCActivityList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();

                var obj = new
                {
                    EmployeeList = EmployeeList,
                    BuCode = BuCode,
                    ACCContactList = ACCContactList,
                    ACCLocationList = ACCLocationList,
                    ACCCommList = ACCCommList,
                    ACCActivityList = ACCActivityList
                };
                 LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetConPerson", "GetConPerson", "View", "Passed", "", _clientIp, _clientMachineName);

                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                  LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetConPerson", "GetConPerson", "View", "Failed", Ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult AddLeadActivity()
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var UserId = Session["EmployeeId"];
                    // var mainRole = Session["MainRole"].ToString();
                    //param.Add("@query", $"Select Fid,CompanyName from ACCMAIN where ('admin' = '{mainRole}' or 'salesadmin' = '{mainRole}' or UserId = '{UserId}') and Deleted=0 AND (Stage='Identification' or Stage='Qualification')");
                    param.Add("@query", $"Select ACCMAIN.Fid,Mas_CompanyProfile.CompanyName from ACCMAIN Left JOIN Mas_CompanyProfile ON ACCMAIN.CompanyName=Mas_CompanyProfile.CompanyId where (UserId = '{UserId}') and Deleted=0 AND (Stage='Identification' or Stage='Qualification')");
                    var CompanyList = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).ToList();
                    ViewBag.CompanyList = CompanyList;

                    param.Add("@query", "select Fid as Fid,ContactPerson from ACCCONTACT where Deleted=0");
                    var ContactPersonList = DapperORM.ReturnList<ACCCONTACT>("Sp_QueryExcution", param).ToList();
                    ViewBag.ContactPersonList = ContactPersonList;

                    param.Add("@query", "Select * from Mas_Employee  Where  Deactivate =0");
                    var OpsList = DapperORM.ReturnList<MAS_EMPLOYEES>("Sp_QueryExcution", param).ToList();
                    ViewBag.OpsList = OpsList;

                   LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AddLeadActivity", "AddLeadActivity", "View", "Passed", "", _clientIp, _clientMachineName);

                    return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AddLeadActivity", "AddLeadActivity", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public JsonResult UpdateLeadStage(string Encrypted_Id, string Stage)
        {
            if (Session["EmployeeId"] == null)
            {
                throw new Exception();
            }
            try
            {

                //    DapperORM.SetConnection();
                //{
                    var GetId = sqlcon.QuerySingle("Select Fid from ACCMAIN WHERE Encrypted_Id='" + Encrypted_Id + "'");
                   // int Accmain_Fid = GetId.Fid;
                    int Accmain_Fid = (int)GetId.Fid;

                    param.Add("@query", "Select * from ACCSERVICES WHERE deleted=0 and ACCMAIN_Fid='" + Accmain_Fid + "'");
                    var CheckService = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();

                    if (CheckService.Count != 0)
                    {
                        if (Stage == "Won")
                        {
                            sqlcon.Query("update ACCMAIN set MAS_STAGES_FID=5 where Encrypted_Id='" + Encrypted_Id + "'");
                            sqlcon.Query("update ACCMAIN set Stage='Proposal' where Encrypted_Id='" + Encrypted_Id + "'");
                            param.Add("@query", "Select * from ACCMAIN WHERE Encrypted_Id='" + Encrypted_Id + "'");
                            var GetAccInfo = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).FirstOrDefault();

                            var param3 = new DynamicParameters();
                            param3.Add("@p_process", "Save");
                            param3.Add("@p_Encrypted_Id", GetAccInfo.Encrypted_Id);
                            param3.Add("@P_ACCMAIN_Fid", GetAccInfo.Fid);
                            param3.Add("@p_BUCode", GetAccInfo.BUCode);
                            param3.Add("@p_CreatedUpdateBy", Session["EmployeeId"]);
                            param3.Add("@p_MAS_STAGES_FID", GetAccInfo.MAS_STAGES_FID);
                            param3.Add("@p_MachineName", Dns.GetHostName().ToString());
                            param3.Add("@p_Mip", Dns.GetHostByName(MachineId).AddressList[0].ToString());
                            var data = DapperORM.ExecuteReturn("Sp_SUD_ACCSTAGES", param3);

                            return Json(new { success = true, redirectUrl = Url.Action("Lead_Dashboard", "CRM") });
                        }
                        else
                        {

                            sqlcon.Query("update ACCMAIN set Stage='" + Stage + "' where Encrypted_Id='" + Encrypted_Id + "'");
                            param.Add("@query", "select Fid from MAS_STAGES where Stage='" + Stage + "' and origin='Lead'");
                            var Fid = DapperORM.ReturnList<MAS_STAGES>("Sp_QueryExcution", param).FirstOrDefault();

                            sqlcon.Query("update ACCMAIN set MAS_STAGES_FID='" + Fid.Fid + "' where Encrypted_Id='" + Encrypted_Id + "'");

                            DynamicParameters param1 = new DynamicParameters();
                            param1.Add("@query", "Select * from ACCMAIN WHERE Encrypted_Id='" + Encrypted_Id + "'");
                            var GetAccInfo = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param1).FirstOrDefault();

                            var param3 = new DynamicParameters();
                            param3.Add("@p_process", "Save");
                            param3.Add("@p_Encrypted_Id", GetAccInfo.Encrypted_Id);
                            param3.Add("p_ACCMAIN_Fid", GetAccInfo.Fid);
                            param3.Add("@p_BUCode", GetAccInfo.BUCode);
                            param3.Add("@p_CreatedUpdateBy", Session["EmployeeId"]);
                            param3.Add("@p_MAS_STAGES_FID", GetAccInfo.MAS_STAGES_FID);
                            param3.Add("@p_MachineName", Dns.GetHostName().ToString());
                            param3.Add("@p_Mip", Dns.GetHostByName(MachineId).AddressList[0].ToString());
                            var data = DapperORM.ExecuteReturn("Sp_SUD_ACCSTAGES", param3);

                            return Json(new { success = true, redirectUrl = Url.Action("Lead_Details", "CRM", new { EncryptedId = Encrypted_Id }) });
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "Stage cannot be changed because there are no associated services." });
                    }
                //}
            }
            catch (Exception ex)
            {
             LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "UpdateLeadStage", "UpdateLeadStage", "Created", "Failed", ex.Message, _clientIp, _clientMachineName);

                return Json(new { success = false, message = "An error occurred while processing your request." });
            }
        }


        public ActionResult SaveDocuments(ACCDMS ACCDMS, string Acc_EncryptedId, HttpPostedFileBase AttachmentURL, string IsRedirect)
        {
            if (Session["EmployeeId"] == null)
            {
                return new HttpStatusCodeResult(400, "Session Expired");
            }
            try
            {
                // DapperORM.SetConnection();
                {
                    string process = string.IsNullOrEmpty(ACCDMS.Encrypted_Id) ? "Save" : "Update";
                    var param = new DynamicParameters();
                    param.Add("@p_process", process);
                    param.Add("@p_Fid", ACCDMS.Fid);
                    param.Add("@p_Encrypted_Id", ACCDMS.Encrypted_Id);
                    param.Add("@P_ACCMAIN_Fid", ACCDMS.ACCMAIN_Fid);
                    param.Add("@p_BUCode", ACCDMS.BUCode);
                    param.Add("@P_DocumentName", ACCDMS.DocumentName);
                    param.Add("@P_Remark", ACCDMS.Remark);
                    param.Add("@P_Stage", ACCDMS.Stage);
                    param.Add("@p_Mid", Dns.GetHostName().ToString());
                    param.Add("@p_Mip", Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString());
                    param.Add("@p_UserId", Session["EmployeeId"]);
                    param.Add("@p_EncryptedId", dbType: DbType.String, direction: ParameterDirection.Output, size: 70);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 10);

                    var data = DapperORM.ExecuteReturn("Sp_SUD_ACCDMS", param);
                    var Message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    if (AttachmentURL != null)
                    {
                        string ENCRYPTED = null;
                        int Id = 0;
                        if (ACCDMS.Encrypted_Id != null)
                        {
                            ENCRYPTED = ACCDMS.Encrypted_Id;
                            DynamicParameters param3 = new DynamicParameters();

                            param3.Add("@query", "select Fid from ACCDMS where Encrypted_Id='" + ACCDMS.Encrypted_Id + "'");
                            var CheckId = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param3).FirstOrDefault();

                            Id = Convert.ToInt32(CheckId.Fid);
                        }
                        else
                        {
                            ENCRYPTED = param.Get<string>("@p_EncryptedId");

                            DynamicParameters param3 = new DynamicParameters();

                            param3.Add("@query", "select Fid from ACCDMS where Encrypted_Id='" + ENCRYPTED + "'");
                            var CheckId = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param3).FirstOrDefault();

                            Id = Convert.ToInt32(CheckId.Fid);
                        }


                        //var GetDocPath = sqlcon.QuerySingle("Select DocumentURL from SET_DOCURLDRIVE");
                        //var GetFirstPath = GetDocPath.DocumentURL;

                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='SalesForce'");
                        var GetFirstPath = GetDocPath.DocInitialPath;


                        var FirstPath = Path.Combine(GetFirstPath, "CRM", "Lead", "Add_Documents", Convert.ToString(Id));

                        if (!Directory.Exists(FirstPath))
                        {
                            Directory.CreateDirectory(FirstPath);
                        }

                        if (AttachmentURL != null)
                        {
                            string ImgUploadFrontPageFilePath = Path.Combine(FirstPath, AttachmentURL.FileName);
                            AttachmentURL.SaveAs(ImgUploadFrontPageFilePath);
                            sqlcon.Query("update ACCDMS set AttachmentURL='" + ImgUploadFrontPageFilePath + "' where Encrypted_Id='" + ENCRYPTED + "'");
                            //sqlcon.Query("INSERT INTO ACCDMS (AttachmentURL, Origin, ACCMAIN_Fid, Encrypted_Id,Deleted) VALUES ('" + ImgUploadFrontPageFilePath + "', 'Activity', '" + Id + "', '" + ENCRYPTED + "',0)");
                        }

                    }

                    var param1 = new DynamicParameters();
                    param1.Add("@query", $"Select * from ACCDMS where deleted=0 and ACCMAIN_Fid = '{ACCDMS.ACCMAIN_Fid}'");
                    var ACCActivityList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();
                    //if (IsRedirect == "True")
                    //{
                    //    return Json(true, JsonRequestBehavior.AllowGet);
                    //}
                    //if (IsRedirect == "Opportunity")
                    //{
                    //    return RedirectToAction("OpportunityList", "CRM", new { area = "CRMS" });
                    //}
                    LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "SaveDocuments", "SaveDocuments", "Created", "Passed", "", _clientIp, _clientMachineName);

                    return Json(new { ACCActivityList, Message, Icon }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "SaveDocuments", "SaveDocuments", "Created", "Failed", ex.Message, _clientIp, _clientMachineName);

                return new HttpStatusCodeResult(500, "An internal server error occurred: " + ex.Message);
            }
        }

        public ActionResult DeleteDocument(string EncryptedId, string Acc_EncryptedId, string Identity)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                //  DapperORM.SetConnection();
                {
                    param.Add("@p_process", "Delete");
                    param.Add("@p_Encrypted_Id", EncryptedId);
                    param.Add("@p_UserId", Session["EmployeeId"]);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parame
                    var Result = DapperORM.ExecuteReturn("Sp_SUD_ACCDMS", param);
                    var message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    TempData["Message"] = message;
                    TempData["Icon"] = Icon;

                   LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "DeleteDocument", "DeleteDocument", "Deleted", "Passed", "", _clientIp, _clientMachineName);

                    return RedirectToAction(Identity, "CRM", new { EncryptedId = Acc_EncryptedId });
                }
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "DeleteDocument", "DeleteDocument", "Deleted", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult GetSerT(string EncryptedId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return new HttpStatusCodeResult(400, "Session Expired");
                }
                //  DapperORM.SetConnection();
                {

                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@query", "select ACCSERVICES.fid as AccFid,ACCSERVICES.Encrypted_Id as Sevice_EncryptedId,accmain.LeadName,MAS_SERCATG.ServiceCategory,MAS_SERCATG.Fid as CategoryFid,MAS_SERVICE.Fid as ServiceFid,MAS_SERVICE.service from ACCSERVICES ACCSERVICES join ACCMAIN accmain on ACCSERVICES.accmain_fid=ACCMAIN.fid join MAS_SERVICE MAS_SERVICE on MAS_SERVICE.fid = ACCSERVICES.mas_service join MAS_SERCATG MAS_SERCATG on MAS_SERVICE.mas_sercatg = MAS_SERCATG.Fid where ACCSERVICES.deleted = 0 and ACCMAIN.Encrypted_Id='" + EncryptedId + "'");
                    var ACCServiceList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();
                    var obj = new
                    {
                        ACCServiceList = ACCServiceList,
                    };
                     LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetSerT", "GetSerT", "View", "Passed", "", _clientIp, _clientMachineName);

                    return Json(obj, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetSerT", "GetSerT", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return new HttpStatusCodeResult(500, "An internal server error occurred: " + ex.Message);
            }
        }

        public ActionResult GetConT(string EncryptedId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@query", "Select * from ACCMAIN where Encrypted_Id='" + EncryptedId + "' and DELETED=0");
                var AccMainDetails = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).FirstOrDefault();

                param.Add("@query", "select contact.LinkedInProfile,contact.Encrypted_Id as Contact_EncryptedId,ACCMAIN_Fid,contact.ContactPerson,dept.DepartmentName As Department,dept.DepartmentId as DepartmentId,desg.DesignationId as DesignationId,desg.DesignationName As Designation,contact.MobileNo,contact.WhatsappNo,contact.EmailId,contact.CountryCode from ACCCONTACT contact join Mas_Department dept on contact.Department=dept.DepartmentId join Mas_Designation desg on contact.Designation=desg.DesignationId where contact.Deleted=0 and contact.ACCMAIN_Fid='" + AccMainDetails.Fid + "'");
                var ACCContactList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                ViewBag.ACCContactList = ACCContactList;
                var obj = new
                {
                    ACCContactList = ACCContactList,
                };
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetConT", "GetConT", "View", "Passed", "", _clientIp, _clientMachineName);

                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetConT", "GetConT", "View", "Passed", "", _clientIp, _clientMachineName);

                return new HttpStatusCodeResult(500, "An internal server error occurred: " + ex.Message);
            }
        }

        public ActionResult GetLocT(string EncryptedId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@query", "Select * from ACCMAIN where Encrypted_Id='" + EncryptedId + "' and DELETED=0");
                var AccMainDetails = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).FirstOrDefault();

                DynamicParameters param1 = new DynamicParameters();
                //param1.Add("@query", "select acc.LocationName,acc.Encrypted_Id as Location_EncryptedId,con.CountryName, con.Fid as CountryId,state.StateName,state.Fid as StateId, acc.Address,acc.Area,acc.IsHeadOffice,acc.City,acc.Pincode from ACCLOCATION acc join MAS_COUNTRY con on acc.MAS_COUNTRY_Fid=con.Fid join MAS_STATE state on acc.MAS_STATE_Fid = state.Fid where acc.ACCMAIN_Fid='" + AccMainDetails.Fid + "' and acc.Deleted=0");
                param1.Add("@query", "select acc.LocationName,acc.Encrypted_Id as Location_EncryptedId,con.CountryName, con.CountryId, state.StateName, state.StateId, acc.Address, acc.Area, acc.IsHeadOffice,acc.City, acc.Pincode from ACCLOCATION acc join Mas_Country con on acc.MAS_COUNTRY_Fid = con.CountryId join Mas_States state on acc.MAS_STATE_Fid = state.StateId where acc.ACCMAIN_Fid='" + AccMainDetails.Fid + "' and acc.Deleted=0");
                var ACCLocationList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();
                var obj = new
                {
                    ACCLocationList = ACCLocationList,
                };
               LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetLocT", "GetLocT", "View", "Passed", "", _clientIp, _clientMachineName);

                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetLocT", "GetLocT", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return new HttpStatusCodeResult(500, "An internal server error occurred: " + ex.Message);
            }
        }

        public ActionResult GetActT(string EncryptedId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@query", "Select * from ACCMAIN where Encrypted_Id='" + EncryptedId + "' and DELETED=0");
                var AccMainDetails = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).FirstOrDefault();

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", " SELECT acc.*,con.Fid contactFid,con.ContactPerson as ContactPersonName FROM ACCACTIVITY ACC JOIN ACCCONTACT CON ON ACC.ACCCONTACT_Fid=con.Fid where acc.Deleted = 0 and con.Deleted = 0 and acc.ACCMAIN_Fid ='" + AccMainDetails.Fid + "'");
                var ActivitityList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();
                var obj = new
                {
                    ActivitityList = ActivitityList,
                };
               LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetActT", "GetActT", "View", "Passed", "", _clientIp, _clientMachineName);

                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
              LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetActT", "GetActT", "View", "Passed", "", _clientIp, _clientMachineName);

                return new HttpStatusCodeResult(500, "An internal server error occurred: " + ex.Message);
            }
        }

        public ActionResult GetCommT(string EncryptedId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                //  DapperORM.SetConnection();
                {
                    param.Add("@query", "Select * from ACCMAIN where Encrypted_Id='" + EncryptedId + "' and DELETED=0");
                    var AccMainDetails = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).FirstOrDefault();

                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@query", "select acccomm.*,con.fid as ConFid,con.ContactPerson as ContactPerson  from ACCCOMM acccomm join ACCCONTACT con on acccomm.ACCCONTACT_Fid = con.Fid where acccomm.Deleted = 0 and acccomm.ACCMAIN_Fid='" + AccMainDetails.Fid + "'");
                    var ACCCommList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();
                    var obj = new
                    {
                        ACCCommList = ACCCommList,
                    };
                   LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetCommT", "GetCommT", "View", "Passed", "", _clientIp, _clientMachineName);

                    return Json(obj, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
               LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetCommT", "GetCommT", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return new HttpStatusCodeResult(500, "An internal server error occurred: " + ex.Message);
            }
        }

     

        //        #region Opportunity (Developed by Kiran)

        public ActionResult CRM_OpportunityForm(string Encrypted_Id)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_Encrypted_Id", Encrypted_Id);
                var data = DapperORM.ReturnList<ACCMAIN>("sp_List_ACCMAIN", param1).FirstOrDefault();
                ViewBag.List = data;
                ViewBag.EncryptedId = Encrypted_Id;
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "CRM_OpportunityForm", "CRM_OpportunityForm", "View", "Passed", "", _clientIp, _clientMachineName);

                return View(data);
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "CRM_OpportunityForm", "CRM_OpportunityForm", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }
        #region Mayuri
        public ActionResult OpportunityList()
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
                param.Add("@p_Encrypted_Id", "OpportunityList");
               // param.Add("@p_UserRole", Session["MainRole"]);
                param.Add("@p_Userid", Session["EmployeeId"]);
                var data = DapperORM.ReturnList<ACCMAIN>("sp_List_ACCMAIN", param).ToList();
                ViewBag.List = data;

                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "OpportunityList", "OpportunityList", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "OpportunityList", "OpportunityList", "View", "Failed", ex.Message, _clientIp, _clientMachineName);
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }
        #endregion

        public ActionResult OpportunityDetails(string EncryptedId)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
                param.Add("@query", "Select Fid,Service,MAS_SERCATG from MAS_SERVICE where DELETED=0");
                var ServiceList = DapperORM.ReturnList<MAS_SERVICE>("Sp_QueryExcution", param).ToList();
                ViewBag.ServiceList = ServiceList;

                //param.Add("@query", "Select CountryId as Fid,CountryName as Currency from Mas_Country where Deactivate=0");
                //var ServiceList1 = DapperORM.ReturnList<MAS_COUNTRY>("Sp_QueryExcution", param).ToList();
                //ViewBag.CurrencyList = ServiceList1;
                
                param.Add("@query", "Select Encrypted_Id from ACCMAIN where Stage in ('Proposal','Under Review') and deleted=0");
                var AllEncryptedIds = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                ViewBag.AllEncryptedIds = AllEncryptedIds;

                param.Add("@query", "Select Fid as Fid,ServiceCategory from MAS_SERCATG where DELETED=0");
                var ServiceCatList = DapperORM.ReturnList<MAS_SERCATG>("Sp_QueryExcution", param).ToList();
                ViewBag.ServiceCatList = ServiceCatList;

                param.Add("@query", "select DepartmentId as Fid,DepartmentName as Department from Mas_Department where Deactivate=0");
                var DepartmentList = DapperORM.ReturnList<MAS_DEPT>("Sp_QueryExcution", param).ToList();
                ViewBag.DepartmentList = DepartmentList;

                param.Add("@query", "select DesignationId as Fid,DesignationName as Designation from Mas_Designation where Deactivate=0");
                var DesignationList = DapperORM.ReturnList<MAS_DESG>("Sp_QueryExcution", param).ToList();
                ViewBag.DesignationList = DesignationList;

                //param.Add("@query", "Select * from ACCMAIN where Encrypted_Id='" + EncryptedId + "' and deleted=0");
                param.Add("@query", " SELECT acc.*, comp.CompanyName FROM ACCMAIN acc LEFT JOIN Mas_CompanyProfile comp ON acc.CompanyName = comp.CompanyId WHERE acc.Encrypted_Id = '" + EncryptedId + "' AND acc.DELETED = 0");
                var AccMainDetails = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).FirstOrDefault();
                ViewBag.AccMainDetails = AccMainDetails;
                ViewBag.CompanyLogo = AccMainDetails.CompanyLogo;
                ViewBag.BUCode = AccMainDetails.BUCode;

                param.Add("@query", "Select CountryId as Fid,CountryName from Mas_Country where Deactivate=0");
                var CountryList = DapperORM.ReturnList<MAS_COUNTRY>("Sp_QueryExcution", param).ToList();
                ViewBag.CountryList = CountryList;

                param.Add("@query", "Select StateId as Fid,StateName from Mas_States where Deactivate=0");
                var StateList = DapperORM.ReturnList<MAS_STATE>("Sp_QueryExcution", param).ToList();
                ViewBag.StateList = StateList;

                ViewBag.EncryptedId = EncryptedId;

                param.Add("@query", "select ACCSERVICES.fid as AccFid,ACCSERVICES.Encrypted_Id as Sevice_EncryptedId,accmain.LeadName,MAS_SERCATG.ServiceCategory,MAS_SERCATG.Fid as CategoryFid,MAS_SERVICE.Fid as ServiceFid,MAS_SERVICE.service from ACCSERVICES ACCSERVICES join ACCMAIN accmain on ACCSERVICES.accmain_fid=ACCMAIN.fid join MAS_SERVICE MAS_SERVICE on MAS_SERVICE.fid = ACCSERVICES.mas_service join MAS_SERCATG MAS_SERCATG on MAS_SERVICE.mas_sercatg = MAS_SERCATG.Fid where ACCSERVICES.deleted = 0 and ACCMAIN.Fid='" + AccMainDetails.Fid + "'");
                var ACCServiceList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                ViewBag.ACCServiceList = ACCServiceList;

                param.Add("@query", "select contact.LinkedInProfile,contact.Encrypted_Id as Contact_EncryptedId,ACCMAIN_Fid,contact.ContactPerson,dept.DepartmentName As Department,dept.DepartmentId as DepartmentId,desg.DesignationId as DesignationId,desg.DesignationName As Designation,contact.MobileNo,contact.WhatsappNo,contact.EmailId,contact.CountryCode from ACCCONTACT contact join Mas_Department dept on contact.Department=dept.DepartmentId join Mas_Designation desg on contact.Designation=desg.DesignationId where contact.Deleted=0 and contact.ACCMAIN_Fid='" + AccMainDetails.Fid + "'");
                var ACCContactList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                ViewBag.ACCContactList = ACCContactList;

                param.Add("@query", "select acc.LocationName,acc.Encrypted_Id as Location_EncryptedId,con.CountryName,con.CountryId as CountryId,state.StateName,state.StateId as StateId, acc.Address,acc.Area,acc.IsHeadOffice,acc.City,acc.Pincode from ACCLOCATION acc join Mas_Country con on acc.MAS_COUNTRY_Fid=con.CountryId join Mas_States state on acc.MAS_STATE_Fid = state.StateId where acc.ACCMAIN_Fid='" + AccMainDetails.Fid + "' and acc.Deleted=0");
                var ACCLocationList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                ViewBag.ACCLocationList = ACCLocationList;

                param.Add("@query", "select Fid as Fid,ContactPerson from ACCCONTACT where ACCMAIN_Fid='" + AccMainDetails.Fid + "' and deleted=0");
                var ContactPersonList = DapperORM.ReturnList<ACCCONTACT>("Sp_QueryExcution", param).ToList();
                ViewBag.ContactPersonList = ContactPersonList;

                param.Add("@query", "select Fid as Fid,Stage from MAS_STAGES where origin='Prospect' and deleted=0");
                var StageList = DapperORM.ReturnList<MAS_STAGES>("Sp_QueryExcution", param).ToList();
                ViewBag.StageList = StageList;

                param1.Add("@query", " SELECT acc.*,con.Fid contactFid,con.ContactPerson as ContactPersonName FROM ACCACTIVITY ACC JOIN ACCCONTACT CON ON ACC.ACCCONTACT_Fid=con.Fid where acc.Deleted = 0 and con.Deleted = 0 and acc.ACCMAIN_Fid ='" + AccMainDetails.Fid + "'");
                var ActivitityList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();
                ViewBag.ActivitityList = ActivitityList;

                param.Add("@query", "Select * from Mas_Employee where Deactivate=0 and EmployeeLeft=0 and ContractorID=1");
                var OpsList = DapperORM.ReturnList<MAS_EMPLOYEES>("Sp_QueryExcution", param).ToList();
                ViewBag.OpsList = OpsList;

                param1.Add("@query", "select acccomm.*,con.fid as ConFid,con.ContactPerson as ContactPerson  from ACCCOMM acccomm join ACCCONTACT con on acccomm.ACCCONTACT_Fid = con.Fid where acccomm.Deleted = 0 and acccomm.ACCMAIN_Fid='" + AccMainDetails.Fid + "'");
                var ACCCommList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();
                ViewBag.ACCCommList = ACCCommList;

                param1.Add("@query", "select * from accproposal where Deleted = 0 and ACCMAIN_Fid='" + AccMainDetails.Fid + "'");
                var ACCProposalList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();
                ViewBag.ACCProposalList = ACCProposalList;

                param.Add("@query", "select * from ACCDMS where ACCMAIN_Fid='" + AccMainDetails.Fid + "' and DELETED=0");
                var AddDocuments = DapperORM.ReturnList<ACCDMS>("Sp_QueryExcution", param).ToList();
                ViewBag.AddDocuments = AddDocuments;

                param.Add("@query", "Select CountryId as Fid,DialCode as CountryCode from Mas_Country where Deactivate=0");
                var CountryCodeList = DapperORM.ReturnList<MAS_COUNTRY>("Sp_QueryExcution", param).ToList();
                ViewBag.CountryCodeList = CountryCodeList;

                var today = DateTime.Today.ToString("yyyy-MM-dd");
                ViewBag.TodayDate = today;

                param.Add("@query", "WITH RankedStages AS (SELECT t2.Stage, t1.*, ROW_NUMBER() OVER (PARTITION BY t1.MAS_STAGES_Fid ORDER BY t1.TimeStamp DESC) AS rn FROM  ACCSTAGES t1 JOIN MAS_STAGES t2 ON t1.MAS_STAGES_Fid = t2.Fid WHERE t1.ACCMAIN_Fid = '" + AccMainDetails.Fid + "' and t1.Deleted=0)SELECT * FROM RankedStages WHERE rn = 1;");
                var StagesRecords = DapperORM.ReturnList<ACCSTAGES>("Sp_QueryExcution", param).ToList();
                ViewBag.StagesRecords = StagesRecords;

                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "OpportunityDetails", "OpportunityDetails", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "OpportunityDetails", "OpportunityDetails", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult UpdateOpportunityStage(string Encrypted_Id, string Stage)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
                var GetId = sqlcon.QuerySingle("Select Fid from ACCMAIN WHERE Encrypted_Id='" + Encrypted_Id + "'");
                int Accmain_Fid = (int)GetId.Fid;

                param.Add("@query", "Select * from ACCPROPOSAL WHERE deleted=0 and ACCMAIN_Fid='" + Accmain_Fid + "'");
                var CheckService = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();

                if (CheckService.Count != 0)
                {
                    if (Stage == "Won")
                    {

                        sqlcon.Query("update ACCMAIN set MAS_STAGES_FID=11 where Encrypted_Id='" + Encrypted_Id + "'");
                        sqlcon.Query("update ACCMAIN set Stage='Won' where Encrypted_Id='" + Encrypted_Id + "'");
                        param.Add("@query", "Select * from ACCMAIN WHERE Encrypted_Id='" + Encrypted_Id + "'");
                        var GetAccInfo = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).FirstOrDefault();

                        var param3 = new DynamicParameters();
                        param3.Add("@p_process", "Save");
                        param3.Add("@p_Encrypted_Id", GetAccInfo.Encrypted_Id);
                        param3.Add("@P_ACCMAIN_Fid", GetAccInfo.Fid);
                        param3.Add("@p_BUCode", GetAccInfo.BUCode);
                        param3.Add("@p_MAS_STAGES_FID", GetAccInfo.MAS_STAGES_FID);
                        param3.Add("@p_CreatedUpdateBy", Session["EmployeeId"]);
                        param3.Add("@p_MachineName", Dns.GetHostName().ToString());
                        param3.Add("@p_Mip", Dns.GetHostByName(MachineId).AddressList[0].ToString());
                        var data = DapperORM.ExecuteReturn("Sp_SUD_ACCSTAGES", param3);
                        return Json(new { success = true, redirectUrl = Url.Action("OpportunityList", "CRM") });
                    }
                    else
                    {
                        sqlcon.Query("update ACCMAIN set Stage='" + Stage + "' where Encrypted_Id='" + Encrypted_Id + "'");
                        param.Add("@query", "select Fid from MAS_STAGES where Stage='" + Stage + "' and origin='Prospect'");
                        var Fid = DapperORM.ReturnList<MAS_STAGES>("Sp_QueryExcution", param).FirstOrDefault();
                        sqlcon.Query("update ACCMAIN set MAS_STAGES_FID='" + Fid.Fid + "' where Encrypted_Id='" + Encrypted_Id + "'");
                        param.Add("@query", "Select * from ACCMAIN WHERE Encrypted_Id='" + Encrypted_Id + "'");
                        var GetAccInfo = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).FirstOrDefault();

                        var param3 = new DynamicParameters();
                        param3.Add("@p_process", "Save");
                        param3.Add("@p_Encrypted_Id", GetAccInfo.Encrypted_Id);
                        param3.Add("@P_ACCMAIN_Fid", GetAccInfo.Fid);
                        param3.Add("@p_BUCode", GetAccInfo.BUCode);
                        param3.Add("@p_MAS_STAGES_FID", GetAccInfo.MAS_STAGES_FID);
                        param3.Add("@p_CreatedUpdateBy", Session["EmployeeId"]);
                        param3.Add("@p_MachineName", Dns.GetHostName().ToString());
                        param3.Add("@p_Mip", Dns.GetHostByName(MachineId).AddressList[0].ToString());
                        var data = DapperORM.ExecuteReturn("Sp_SUD_ACCSTAGES", param3);

                        LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "UpdateOpportunityStage", "UpdateOpportunityStage", "Created", "Passed", "", _clientIp, _clientMachineName);
                        return Json(new { success = true, redirectUrl = Url.Action("OpportunityDetails", "CRM", new { EncryptedId = Encrypted_Id }) });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Stage cannot be changed because there are no Proposals" });
                }
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "UpdateOpportunityStage", "UpdateOpportunityStage", "Created", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult SaveProposal(ACCPROPOSAL ACCPROPOSAL, string Acc_EncryptedId, HttpPostedFileBase ProposalFileAttachement)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {

                DynamicParameters param5 = new DynamicParameters();

                param5.Add("@p_process", "IsValidation");
                param5.Add("@p_Encrypted_Id", ACCPROPOSAL.Encrypted_Id);
                param5.Add("@p_ProposalNo", ACCPROPOSAL.ProposalNo);
                param5.Add("@p_ACCMAIN_Fid", ACCPROPOSAL.ACCMAIN_Fid);
                param5.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param5.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var data5 = DapperORM.ExecuteReturn("Sp_SUD_ACCPROPOSAL", param5);
                var Message = param5.Get<string>("@p_msg");
                var Icon = param5.Get<string>("@p_Icon");


                if (Message == "")
                {
                    string process = string.IsNullOrEmpty(ACCPROPOSAL.Encrypted_Id) ? "Save" : "Update";

                    param.Add("@p_process", process);
                    param.Add("@P_Fid", ACCPROPOSAL.Fid);
                    param.Add("@p_Encrypted_Id", ACCPROPOSAL.Encrypted_Id);
                    param.Add("@p_ACCMAIN_Fid", ACCPROPOSAL.ACCMAIN_Fid);
                    param.Add("@p_BUCode", ACCPROPOSAL.BUCode);
                    param.Add("@p_Mid", Dns.GetHostName().ToString());
                    param.Add("@p_Mip", Dns.GetHostByName(MachineId).AddressList[0].ToString());
                    param.Add("@p_UserId", Session["EmployeeId"]);
                    param.Add("@p_Currency", ACCPROPOSAL.Currency);
                    param.Add("@p_ProposalNo", ACCPROPOSAL.ProposalNo);
                    param.Add("@p_RevisionNo", ACCPROPOSAL.RevisionNo);
                    param.Add("@p_ProposalSubmissionDate", ACCPROPOSAL.ProposalSubmissionDate);
                    param.Add("@p_ExpectedClosingDate", ACCPROPOSAL.ExpectedClosingDate);
                    param.Add("@p_TypeOfContract", ACCPROPOSAL.TypeOfContract);
                    param.Add("@p_ManpowerCount", ACCPROPOSAL.ManpowerCount);
                    param.Add("@p_TaxPercentage", ACCPROPOSAL.TaxPercentage);
                    param.Add("@p_ContractTenure", ACCPROPOSAL.ContractTenure);
                    param.Add("@p_TotalContractValue", ACCPROPOSAL.TotalContractValue);
                    param.Add("@p_MonthlyContractValue", ACCPROPOSAL.MonthlyContractValue);
                    param.Add("@p_AnnualContractValue", ACCPROPOSAL.AnnualContractValue);
                    param.Add("@p_TaxApplicable", ACCPROPOSAL.TaxApplicable);
                    param.Add("@p_ACVInclusiveOfTax", ACCPROPOSAL.ACVInclusiveOfTax);
                    param.Add("@p_ManagementFeePer", ACCPROPOSAL.ManagementFeePer);
                    param.Add("@p_ManagementFeeValue", ACCPROPOSAL.ManagementFeeValue);
                    param.Add("@p_RevisedProposalDate", ACCPROPOSAL.RevisedProposalDate);
                    param.Add("@p_Deleted", ACCPROPOSAL.ACCMAIN_Fid);
                    param.Add("@p_EncryptedId", dbType: DbType.String, direction: ParameterDirection.Output, size: 70);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var data = DapperORM.ExecuteReturn("Sp_SUD_ACCPROPOSAL", param);
                    Message = param.Get<string>("@p_msg");
                    Icon = param.Get<string>("@p_Icon");
                    string ENCRYPTED = null;
                    int Id = 0;
                    if (ACCPROPOSAL.Encrypted_Id != null)
                    {
                        ENCRYPTED = ACCPROPOSAL.Encrypted_Id;
                        DynamicParameters param3 = new DynamicParameters();

                        param3.Add("@query", "select Fid from ACCPROPOSAL where Encrypted_Id='" + ACCPROPOSAL.Encrypted_Id + "'");
                        var CheckId = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param3).FirstOrDefault();

                        Id = Convert.ToInt32(CheckId.Fid);
                    }
                    else
                    {
                        ENCRYPTED = param.Get<string>("@p_EncryptedId");

                        DynamicParameters param3 = new DynamicParameters();

                        param3.Add("@query", "select Fid from ACCPROPOSAL where Encrypted_Id='" + ENCRYPTED + "'");
                        var CheckId = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param3).FirstOrDefault();

                        Id = Convert.ToInt32(CheckId.Fid);
                    }

                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='SalesForce'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    var FirstPath = Path.Combine(GetFirstPath, "CRM", "Opportunity", "Proposal", Convert.ToString(Id));

                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }

                    if (ProposalFileAttachement != null)
                    {
                        string ImgUploadFrontPageFilePath = Path.Combine(FirstPath, ProposalFileAttachement.FileName);
                        ProposalFileAttachement.SaveAs(ImgUploadFrontPageFilePath);

                        sqlcon.Query("update ACCPROPOSAL set ProposalFileAttachement='" + ImgUploadFrontPageFilePath + "' where Encrypted_Id='" + ENCRYPTED + "'");

                    }

                }

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "select * from ACCPROPOSAL where Deleted=0 and ACCMAIN_Fid='" + ACCPROPOSAL.ACCMAIN_Fid + "'");
                var ACCProposalList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();
                //ViewBag.ACCServiceList = ACCServiceList;

                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "SaveProposal", "SaveProposal", "View", "Passed", "", _clientIp, _clientMachineName);

                return Json(new { ACCProposalList, Message, Icon }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "SaveProposal", "SaveProposal", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        #region Mayuri
        public ActionResult AddOpportunityContact()
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                var UserId = Session["EmployeeId"];
                param.Add("@query", $"Select ACCMAIN.Fid,Mas_CompanyProfile.CompanyName from ACCMAIN Left JOIN Mas_CompanyProfile ON ACCMAIN.CompanyName=Mas_CompanyProfile.CompanyId where Deleted=0 ");
                var CompanyList = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).ToList();
                ViewBag.CompanyList = CompanyList;

                param.Add("@query", "select DepartmentId as Fid,DepartmentName as Department from Mas_Department where Deactivate=0");
                var DepartmentList = DapperORM.ReturnList<MAS_DEPT>("Sp_QueryExcution", param).ToList();
                ViewBag.DepartmentList = DepartmentList;

                param.Add("@query", "select DesignationId as Fid,DesignationName as Designation from Mas_Designation where Deactivate=0");
                var DesignationList = DapperORM.ReturnList<MAS_DESG>("Sp_QueryExcution", param).ToList();
                ViewBag.DesignationList = DesignationList;

                param.Add("@query", "select CountryId as Fid,DialCode as CountryCode  from Mas_Country where Deactivate=0");
                var CountryCodeList = DapperORM.ReturnList<MAS_COUNTRY>("Sp_QueryExcution", param).ToList();
                ViewBag.CountryCodeList = CountryCodeList;

                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AddOpportunityContact", "AddOpportunityContact", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AddOpportunityContact", "AddOpportunityContact", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }
        #endregion

        public ActionResult AddOpportunityLocation()
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                var UserId = Session["EmployeeId"];
                //  var mainRole = Session["MainRole"].ToString();
                param.Add("@query", $"Select ACCMAIN.Fid,Mas_CompanyProfile.CompanyName from ACCMAIN Left JOIN Mas_CompanyProfile ON ACCMAIN.CompanyName=Mas_CompanyProfile.CompanyId where Deleted=0 ");
                var CompanyList = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).ToList();
                ViewBag.CompanyList = CompanyList;

                param.Add("@query", "select CountryId as Fid,CountryName from Mas_Country where Deactivate=0");
                var CountryList = DapperORM.ReturnList<MAS_COUNTRY>("Sp_QueryExcution", param).ToList();
                ViewBag.CountryList = CountryList;

                param.Add("@query", "select StateId as Fid,StateName from Mas_States where Deactivate=0");
                var StateList = DapperORM.ReturnList<MAS_STATE>("Sp_QueryExcution", param).ToList();
                ViewBag.StateList = StateList;

                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AddOpportunityLocation", "AddOpportunityLocation", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AddOpportunityLocation", "AddOpportunityLocation", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult AddOpportunityCommunication()
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
                //  var UserId = Session["Employee_Fid"].ToString();

                param.Add("@query", $"Select ACCMAIN.Fid,Mas_CompanyProfile.CompanyName from ACCMAIN Left JOIN Mas_CompanyProfile ON ACCMAIN.CompanyName=Mas_CompanyProfile.CompanyId where Deleted=0 "); var CompanyList = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).ToList();
                ViewBag.CompanyList = CompanyList;

                param.Add("@query", "select Fid as Fid,ContactPerson from ACCCONTACT where Deleted=0");
                var ContactPersonList = DapperORM.ReturnList<ACCCONTACT>("Sp_QueryExcution", param).ToList();
                ViewBag.ContactPersonList = ContactPersonList;

                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "View", "AddOpportunityCommunication", "AddOpportunityCommunication", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "View", "AddOpportunityCommunication", "AddOpportunityCommunication", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult AddOpportunityActivity()
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
                // DapperORM.SetConnection();
                var UserId = Session["EmployeeId"];
                //var mainRole = Session["MainRole"].ToString();
                param.Add("@query", $"Select ACCMAIN.Fid,Mas_CompanyProfile.CompanyName from ACCMAIN Left JOIN Mas_CompanyProfile ON ACCMAIN.CompanyName=Mas_CompanyProfile.CompanyId where Deleted=0 "); var CompanyList = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).ToList();
                ViewBag.CompanyList = CompanyList;

                param.Add("@query", "select Fid as Fid,ContactPerson from ACCCONTACT where Deleted=0");
                var ContactPersonList = DapperORM.ReturnList<ACCCONTACT>("Sp_QueryExcution", param).ToList();
                ViewBag.ContactPersonList = ContactPersonList;

                param.Add("@query", "Select * from Mas_Employee where Deactivate=0 and ContractorID <> 1 and EmployeeLeft=0");
                var OpsList = DapperORM.ReturnList<MAS_EMPLOYEES>("Sp_QueryExcution", param).ToList();
                ViewBag.OpsList = OpsList;

                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "View", "AddOpportunityActivity", "AddOpportunityActivity", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "View", "AddOpportunityActivity", "AddOpportunityActivity", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult CompanyDetails(string DetailsId)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
                //DapperORM.SetConnection();
                //param.Add("@query", $"select acc.*,(select t2.EmployeeName from USERS t1 join MAS_EMPLOYEES t2 on t1.EmployeeId=t2.Fid where t1.Fid=acc.Fid)as EmployeeName from  ACCMAIN acc join USERS emp on acc.UserId=emp.Fid where acc.CompanyName='{ DetailsId}'");
                param.Add("@query", $"SELECT ACCMAIN.*,(SELECT TOP 1 Mas_Employee.EmployeeName FROM Tool_UserLogin JOIN Mas_Employee ON Tool_UserLogin.EmployeeId = Mas_Employee.EmployeeId WHERE Tool_UserLogin.EmployeeId = ACCMAIN.UserId) AS EmployeeName FROM ACCMAIN where ACCMAIN.CompanyName='{ DetailsId}'");
                var ACCMainDetails = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                return Json(ACCMainDetails, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
               LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "View", "CompanyDetails", "CompanyDetails", "View", "Failed", ex.Message, _clientIp, _clientMachineName);
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        //#endregion

        #region Account

        public ActionResult CRM_AccountForm(string Encrypted_Id)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                if (Encrypted_Id != null)
                {
                    //DapperORM.SetConnection();
                    {
                        // param.Add("@query", "Select Fid, CompanyName from ACCMAIN where deleted=0");
                        param.Add("@query", "Select CompanyId As Fid, CompanyName from Mas_CompanyProfile where Deactivate=0");
                        var CompanyNameList = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).ToList();
                        ViewBag.CmpName = CompanyNameList;


                        param.Add("@query", "Select Fid as Fid,LeadSource from MAS_LEADSOURCE where deleted=0");
                        var LeadSourceList = DapperORM.ReturnList<MAS_LEADSOURCE>("Sp_QueryExcution", param).ToList();
                        ViewBag.LeadSourceList = LeadSourceList;

                        param.Add("@query", "Select Fid,IndustryType from MAS_INDUSTRYTYPE where deleted=0");
                        var IndustryTypeList = DapperORM.ReturnList<MAS_INDUSTRYTYPE>("Sp_QueryExcution", param).ToList();
                        ViewBag.IndustryTypeList = IndustryTypeList;

                        param.Add("@query", "Select BranchId As Fid,BranchName As CompanyName from Mas_Branch where Deactivate=0");
                        var BUCodeList = DapperORM.ReturnList<Ocean>("Sp_QueryExcution", param).ToList();
                        ViewBag.BUCodeList = BUCodeList;

                        //param.Add("@query", "Select CountryId As Fid,CountryName As CountryCode from Mas_Country where Deactivate=0");
                        param.Add("@query", " Select CurrencyCountryId As Fid,ShortName As Currency from Mas_Currency where Deactivate=0");
                        var CountryCodeList = DapperORM.ReturnList<MAS_COUNTRY>("Sp_QueryExcution", param).ToList();
                        ViewBag.CountryCodeList = CountryCodeList;
                        
                        DynamicParameters param1 = new DynamicParameters();
                        param1.Add("@p_Encrypted_Id", Encrypted_Id);
                        var data = DapperORM.ReturnList<ACCMAIN>("sp_List_ACCMAIN", param1).FirstOrDefault();
                        ViewBag.List = data;
                        ViewBag.IsDirectAccount = data.IsDirectAccount;
                        ViewBag.EncryptedId = Encrypted_Id;
                        LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "Form", "CRM_AccountForm", "CRM_AccountForm", "Create", "Passed", "", _clientIp, _clientMachineName);

                        return View(data);
                    }
                }
                ViewBag.AddUpdateTitle = "Add";
                ViewBag.EncryptedId = Encrypted_Id;
                var EmployeeName = Session["EmployeeName"];
                var EmployeeId = Session["EmployeeId"];
              //  DapperORM.SetConnection();
                {
                    //  param.Add("@query", "Select Fid, CompanyName from ACCMAIN where deleted=0");
                    param.Add("@query", "Select CompanyId As Fid, CompanyName from Mas_CompanyProfile where Deactivate=0");
                    var CompanyNameList = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).ToList();
                    // ViewBag.CompanyNameList = CompanyNameList;
                    ViewBag.CmpName = CompanyNameList;


                    //param.Add("@query", "Select BranchId As Fid,BranchName As CompanyName from Mas_Branch where Deactivate=0");
                    //var BUCodeList = DapperORM.ReturnList<Ocean>("Sp_QueryExcution", param).ToList();
                    //ViewBag.BUCodeList = BUCodeList;
                    ViewBag.BUCodeList = new List<Ocean>();

                    param.Add("@query", "Select Fid as Fid,LeadSource from MAS_LEADSOURCE where deleted=0");
                    var LeadSourceList = DapperORM.ReturnList<MAS_LEADSOURCE>("Sp_QueryExcution", param).ToList();
                    ViewBag.LeadSourceList = LeadSourceList;

                    param.Add("@query", "Select Fid,IndustryType from MAS_INDUSTRYTYPE where deleted=0");
                    var IndustryTypeList = DapperORM.ReturnList<MAS_INDUSTRYTYPE>("Sp_QueryExcution", param).ToList();
                    ViewBag.IndustryTypeList = IndustryTypeList;

                    //param.Add("@query", "Select CountryId As Fid,CountryName As CountryCode from Mas_Country where Deactivate=0");
                    param.Add("@query", " Select CurrencyCountryId As Fid,ShortName As Currency from Mas_Currency where Deactivate=0");
                    var CountryCodeList = DapperORM.ReturnList<MAS_COUNTRY>("Sp_QueryExcution", param).ToList();
                    ViewBag.CountryCodeList = CountryCodeList;
                }
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "Form", "CRM_AccountForm", "CRM_AccountForm", "Create", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "Form", "CRM_AccountForm", "CRM_AccountForm", "Create", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }


        public ActionResult AccountList()
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

               // param.Add("@p_Encrypted_Id", "AccountList");
                  //  param.Add("@p_UserRole", Session["MainRole"]);
                   // param.Add("@p_Userid", Session["EmployeeId"]);

                var EMP = Session["EmployeeId"];
                param.Add("@p_Encrypted_Id", "AccountList");
               // param.Add("@p_UserRole", Session["MainRole"]);
                param.Add("@p_Userid", Session["EmployeeId"]);

                var data = DapperORM.ReturnList<ACCMAIN>("sp_List_ACCMAIN", param).ToList();
                var processedData = data.Select(x => new
                {
                    x.LeadName,
                    x.CompanyName,
                    x.Stage,
                    x.CompanyRating,
                    Encrypted_Id = x.Encrypted_Id
                }).ToList();

                ViewBag.List = processedData;
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AccountList", "AccountList", "List", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AccountList", "AccountList", "List", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult AccountDetails(string EncryptedId)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
                //DapperORM.SetConnection();
                param.Add("@query", "Select Fid,Service,MAS_SERCATG from MAS_SERVICE");
                var ServiceList = DapperORM.ReturnList<MAS_SERVICE>("Sp_QueryExcution", param).ToList();
                ViewBag.ServiceList = ServiceList;

                param.Add("@query", "Select CountryId As Fid,CountryName As Currency from Mas_Country where Deactivate=0");
                var ServiceList1 = DapperORM.ReturnList<MAS_COUNTRY>("Sp_QueryExcution", param).ToList();
                ViewBag.CurrencyList = ServiceList1;
                
                param.Add("@query", "Select Encrypted_Id from ACCMAIN where Stage in ('Won')");
                var AllEncryptedIds = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                ViewBag.AllEncryptedIds = AllEncryptedIds;

                param.Add("@query", "Select Fid as Fid,ServiceCategory from MAS_SERCATG where Deleted = 0");
                var ServiceCatList = DapperORM.ReturnList<MAS_SERCATG>("Sp_QueryExcution", param).ToList();
                ViewBag.ServiceCatList = ServiceCatList;

                param.Add("@query", "select DepartmentId as Fid,DepartmentName as Department from Mas_Department where Deactivate=0 Order By DepartmentName");
                var DepartmentList = DapperORM.ReturnList<MAS_DEPT>("Sp_QueryExcution", param).ToList();
                ViewBag.DepartmentList = DepartmentList;

                param.Add("@query", "select DesignationId as Fid,DesignationName as Designation from Mas_Designation where Deactivate=0 Order By DesignationName");
                var DesignationList = DapperORM.ReturnList<MAS_DESG>("Sp_QueryExcution", param).ToList();
                ViewBag.DesignationList = DesignationList;

                param.Add("@query", "SELECT acc.*, comp.CompanyName FROM ACCMAIN acc LEFT JOIN Mas_CompanyProfile comp ON acc.CompanyName = comp.CompanyId WHERE acc.Encrypted_Id = '" + EncryptedId + "' AND acc.DELETED = 0");
                //param.Add("@query", " SELECT acc.*, comp.CompanyName FROM ACCMAIN acc LEFT JOIN Mas_CompanyProfile comp ON acc.CompanyName = comp.CompanyId WHERE acc.Encrypted_Id = '" + EncryptedId + "' AND acc.DELETED = 0");
                var AccMainDetails = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).FirstOrDefault();
                ViewBag.AccMainDetails = AccMainDetails;
                ViewBag.BUCode = AccMainDetails.BUCode;
                ViewBag.CompanyLogo = AccMainDetails.CompanyLogo;
                ViewBag.Fid = AccMainDetails.Fid;
                ViewBag.IsDirectAccount = AccMainDetails.IsDirectAccount;

                // param.Add("@query", " SELECT acc.*, comp.CompanyName FROM ACCMAIN acc LEFT JOIN Mas_CompanyProfile comp ON acc.CompanyName = comp.CompanyId where Encrypted_Id='" + EncryptedId + "'");
                 param.Add("@query", "select acc.Fid,acc.Encrypted_Id,acc.Fdate,acc.Mid,acc.Mip,acc.BUCode,acc.UserId,acc.LeadPriority,acc.LeadName,acc.CompanyName as [CompanyName1],comp.CompanyName,acc.CompanyCategory,acc.OrganisationType,acc.LeadType,acc.MAS_LEADSOURCE_Fid,acc.MAS_INDUSTRYTYPE_Fid,acc.CompanyEmail,acc.CountryCode,acc.CompanyContactNumber,acc.CINNo,acc.Summary,acc.CompanyRating,acc.MAS_STAGES_FID,acc.Stage,acc.AreaInSqureFt,acc.Discription,acc.LegalEntityName,acc.ContractStartDate,acc.ContractEndDate,acc.ECVCurrency,acc.ExpectedContractValue,acc.RateRevision,acc.DurationInMonths,acc.PaymentTermsInDays,acc.PenaltyApplicable,acc.NeedsPatrolling,acc.PatrollingNotes,acc.TranferBy_Fid,acc.TransferDate,acc.IsDirectAccount,acc.IsCoOwner,acc.Deleted,acc.Latitude,acc.Longitude,acc.ReferenceName,acc.Website,acc.CompanyLogo,acc.TenderApplicable,acc.TenderNumber,acc.Tendersource,acc.Deleted_By From ACCMAIN acc LEFT JOIN Mas_CompanyProfile comp ON acc.CompanyName = comp.CompanyId  where Encrypted_Id ='" + EncryptedId + "'");
                var AccmainList = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).ToList();
                ViewBag.AccmainList = AccmainList;

                var AccmainList1 = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).FirstOrDefault();
                ViewBag.AccmainList1 = AccmainList1;

                param.Add("@query", "Select CountryId As Fid,CountryName from Mas_Country where Deactivate=0");
                var CountryList = DapperORM.ReturnList<MAS_COUNTRY>("Sp_QueryExcution", param).ToList();
                ViewBag.CountryList = CountryList;

                param.Add("@query", "Select StateId As Fid,StateName from Mas_States where Deactivate=0");
                var StateList = DapperORM.ReturnList<MAS_STATE>("Sp_QueryExcution", param).ToList();
                ViewBag.StateList = StateList;

                ViewBag.EncryptedId = EncryptedId;

                param.Add("@query", "select ACCSERVICES.fid as AccFid,ACCSERVICES.Encrypted_Id as Sevice_EncryptedId,accmain.LeadName,MAS_SERCATG.ServiceCategory,MAS_SERCATG.Fid as CategoryFid,MAS_SERVICE.Fid as ServiceFid,MAS_SERVICE.service from ACCSERVICES ACCSERVICES join ACCMAIN accmain on ACCSERVICES.accmain_fid=ACCMAIN.fid join MAS_SERVICE MAS_SERVICE on MAS_SERVICE.fid = ACCSERVICES.mas_service join MAS_SERCATG MAS_SERCATG on MAS_SERVICE.mas_sercatg = MAS_SERCATG.Fid where ACCSERVICES.deleted = 0 and ACCMAIN.Fid='" + AccMainDetails.Fid + "'");
                var ACCServiceList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                ViewBag.ACCServiceList = ACCServiceList;

                //param.Add("@query", "select contact.LinkedInProfile,contact.Encrypted_Id as Contact_EncryptedId,ACCMAIN_Fid,contact.ContactPerson,dept.Department,dept.Fid as DepartmentId,desg.Fid as DesignationId,desg.Designation,contact.MobileNo,contact.WhatsappNo,contact.EmailId,contact.CountryCode,IsPrimaryContact from ACCCONTACT contact join MAS_DEPT dept on contact.Department=dept.Fid join MAS_DESG desg on contact.Designation=desg.Fid where contact.Deleted=0 and contact.ACCMAIN_Fid='" + AccMainDetails.Fid + "'");
                param.Add("@query", "select contact.LinkedInProfile,contact.Encrypted_Id as Contact_EncryptedId,contact.ACCMAIN_Fid,SiteName,contact.ContactPerson, dept.DepartmentName As Department, dept.DepartmentId , desg.DesignationId, desg.DesignationName As Designation, contact.MobileNo,contact.WhatsappNo, contact.EmailId, contact.CountryCode, IsPrimaryContact, accsites.Deleted from ACCCONTACT contact join Mas_Department dept on contact.Department = dept.DepartmentId join MAs_Designation desg on contact.Designation = desg.DesignationId left join accsites on accsites.acccontact_fid = contact.fid where contact.Deleted = 0  and(accsites.Deleted = 0 or accsites.Deleted is null) and contact.ACCMAIN_Fid = '" + AccMainDetails.Fid + "'");

                var ACCContactList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                ViewBag.ACCContactList = ACCContactList;

                param.Add("@query", "select acc.LocationName,acc.Encrypted_Id as Location_EncryptedId,con.CountryName, con.CountryId ,state.StateName,state.StateId, acc.Address,acc.Area,acc.IsHeadOffice,acc.City, acc.Pincode from ACCLOCATION acc join Mas_Country con on acc.MAS_COUNTRY_Fid=con.CountryId join Mas_States state on acc.MAS_STATE_Fid = state.StateId where acc.ACCMAIN_Fid='" + AccMainDetails.Fid + "' and acc.Deleted=0");
                var ACCLocationList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                ViewBag.ACCLocationList = ACCLocationList;

                param.Add("@query", "select Fid as Fid,ContactPerson from ACCCONTACT where ACCMAIN_Fid='" + AccMainDetails.Fid + "'and Deleted =0");
                var ContactPersonList = DapperORM.ReturnList<ACCCONTACT>("Sp_QueryExcution", param).ToList();
                ViewBag.ContactPersonList = ContactPersonList;

                param.Add("@query", "select Fid as Fid,Stage from MAS_STAGES where origin='Prospect'");
                var StageList = DapperORM.ReturnList<MAS_STAGES>("Sp_QueryExcution", param).ToList();
                ViewBag.StageList = StageList;

                param1.Add("@query", " SELECT acc.*,con.Fid contactFid,con.ContactPerson as ContactPersonName FROM ACCACTIVITY ACC JOIN ACCCONTACT CON ON ACC.ACCCONTACT_Fid=con.Fid where acc.Deleted = 0 and con.Deleted = 0 and acc.ACCMAIN_Fid ='" + AccMainDetails.Fid + "'");
                var ActivitityList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();
                ViewBag.ActivitityList = ActivitityList;

                param.Add("@query", "  Select * from Mas_Employee where EmployeeLeft=0 and Deactivate =0");
                var OpsList = DapperORM.ReturnList<MAS_EMPLOYEES>("Sp_QueryExcution", param).ToList();
                ViewBag.OpsList = OpsList;

                param1.Add("@query", "select acccomm.*,con.fid as ConFid,con.ContactPerson as ContactPerson  from ACCCOMM acccomm join ACCCONTACT con on acccomm.ACCCONTACT_Fid = con.Fid where acccomm.Deleted = 0 and acccomm.ACCMAIN_Fid='" + AccMainDetails.Fid + "'");
                var ACCCommList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();
                ViewBag.ACCCommList = ACCCommList;

                param.Add("@query", "select t2.Encrypted_Id AS Site_Encrypted_Id, t1.*, t2.* from ACCCONTRACT t1 join ACCSITES t2 on t1.Fid = t2.ACCCONTRACT_Fid where t1.Deleted = 0 and t1.ACCMAIN_Fid ='" + AccMainDetails.Fid + "' and t2.deleted = 0");
                var accContractList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                ViewBag.AccContractList = accContractList;

                param.Add("@query", "select * from ACCPROPOSAL where Deleted = 0 and ACCMAIN_Fid ='" + AccMainDetails.Fid + "'");
                var AccProposalList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();
                ViewBag.AccProposalList = AccProposalList;

                param1.Add("@query", "select mp.*, con.LegalEntityName, desg.DesignationId,desg.DesignationName from accmanpower mp join Mas_Designation desg on mp.MAS_DESG_Fid = desg.DesignationId left join ACCCONTRACT con on con.Fid = mp.ACCCONTRACT_Fid  where mp.deleted = 0 and con.Deleted = 0 and mp.ACCMAIN_Fid ='" + AccMainDetails.Fid + "'");
                var ACCManpowerList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();
                ViewBag.ACCManpowerList = ACCManpowerList;

                param1.Add("@query", "select MAS_SERVICE.Fid,MAS_SERVICE.Service from ACCSERVICES ACCSERVICES join MAS_SERVICE MAS_SERVICE on ACCSERVICES.MAS_SERVICE = MAS_SERVICE.Fid where ACCSERVICES.Deleted = 0 and ACCSERVICES.ACCMAIN_Fid = '" + AccMainDetails.Fid + "'");
                var ContractServiceList = DapperORM.ReturnList<MAS_SERVICE>("Sp_QueryExcution", param1).ToList();
                ViewBag.ContractServiceList = ContractServiceList;

                param1.Add("@query", "select Fid,LocationName from acclocation where deleted=0 and ACCMAIN_Fid = '" + AccMainDetails.Fid + "'");
                var ContractLocationList = DapperORM.ReturnList<ACCLOCATION>("Sp_QueryExcution", param1).ToList();
                ViewBag.ContractLocationList = ContractLocationList;

                param.Add("@query", "select * from ACCDMS where ACCMAIN_Fid='" + AccMainDetails.Fid + "' and DELETED=0");
                var AddDocuments = DapperORM.ReturnList<ACCDMS>("Sp_QueryExcution", param).ToList();
                ViewBag.AddDocuments = AddDocuments;

                param.Add("@query", "Select CountryId As Fid,DialCode As CountryCode from Mas_country where Deactivate=0");
                var CountryCodeList = DapperORM.ReturnList<MAS_COUNTRY>("Sp_QueryExcution", param).ToList();
                ViewBag.CountryCodeList = CountryCodeList;

                param.Add("@query", "select * from ACCSITES where ACCMAIN_Fid='" + AccMainDetails.Fid + "' and Deleted=0");
                //param.Add("@query", "select * from ACCSITES where Deleted=0");
                var siteloadlist = DapperORM.ReturnList<ACCSITES>("Sp_QueryExcution", param).ToList();
                ViewBag.siteloadlist = siteloadlist;

                var today = DateTime.Today.ToString("yyyy-MM-dd");
                ViewBag.TodayDate = today;
                
                string result = Convert.ToString(Session["Con_Server"]) + Convert.ToString(Session["Con_Database"]);

                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AccountDetails", "AccountDetails", "View", "Passed", result, _clientIp, _clientMachineName);

                return View();

            }
            catch (Exception ex)
            {
                string result = Convert.ToString(Session["Con_Server"]) + Convert.ToString(Session["Con_Database"]);
                string RE = result + ex.Message;
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AccountDetails", "AccountDetails", "View", "Failed", RE, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }


        //        public ActionResult Update_LeadInfo(ACCMAIN ACCMAIN)
        //        {
        //            if (Session["EmployeeId"] == null)
        //            {
        //                return RedirectToAction("Logout", "Home", new { area = "" });
        //            }
        //            try
        //            {
        //                DapperORM.SetConnection();
        //                param.Add("@p_process", "Update");
        //                param.Add("@P_Fid", ACCMAIN.Fid);
        //                param.Add("@P_Encrypted_Id", ACCMAIN.Encrypted_Id);
        //                param.Add("@p_LegalEntityName", ACCMAIN.LegalEntityName);
        //                param.Add("@p_ContractStartDate", ACCMAIN.ContractStartDate);
        //                param.Add("@p_ContractEndDate", ACCMAIN.ContractEndDate);
        //                param.Add("@p_DurationInMonths", ACCMAIN.DurationInMonths);
        //                param.Add("@p_PaymentTermsInDays", ACCMAIN.PaymentTermsInDays);
        //                param.Add("@p_NeedsPatrolling", ACCMAIN.NeedsPatrolling);
        //                param.Add("@p_PenaltyApplicable", ACCMAIN.PenaltyApplicable);
        //                param.Add("@p_RateRevision", ACCMAIN.RateRevision);
        //                param.Add("@p_PatrollingNotes", ACCMAIN.PatrollingNotes);
        //                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //                var data = DapperORM.ExecuteReturn("Sp_UPDATE_ACCOUNT", param);
        //                TempData["Message"] = param.Get<string>("@p_msg");
        //                TempData["Icon"] = param.Get<string>("@p_Icon");
        //                DynamicParameters param1 = new DynamicParameters();
        //                param1.Add("@query", "select* from ACCMAIN where Fid='" + ACCMAIN.Fid + "'");
        //                var ACCLIST = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();
        //                HomeController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Update_LeadInfo", "Update_LeadInfo", "View", "Passed", "", _clientIp, _clientMachineName);

        //                return Json(ACCLIST, JsonRequestBehavior.AllowGet);
        //            }
        //            catch (Exception ex)
        //            {
        //                HomeController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Update_LeadInfo", "Update_LeadInfo", "View", "Passed", "", _clientIp, _clientMachineName);

        //                return RedirectToAction("ErrorPage", "Login", new { area = "" });
        //            }
        //        }

        public ActionResult AddAccountContact()
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
               // DapperORM.SetConnection();
                var UserId = Session["EmployeeId"];
                // var mainRole = Session["MainRole"].ToString();
                //param.Add("@query", $"Select Fid,CompanyName from ACCMAIN where ('admin' = '{mainRole}' or 'salesadmin' = '{mainRole}' or UserId = '{UserId}') and Deleted=0 AND Stage='Won'");
                param.Add("@query", $" Select Distinct ACCMAIN.Fid,Mas_CompanyProfile.CompanyName from ACCMAIN Left JOIN Mas_CompanyProfile ON ACCMAIN.CompanyName=Mas_CompanyProfile.CompanyId where Deleted=0 ");
                var CompanyList = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).ToList();
                ViewBag.CompanyList = CompanyList;

                param.Add("@query", "Select DepartmentId As Fid,DepartmentName As Department from Mas_Department where Deactivate=0 Order By Department");
                var DepartmentList = DapperORM.ReturnList<MAS_DEPT>("Sp_QueryExcution", param).ToList();
                ViewBag.DepartmentList = DepartmentList;

                param.Add("@query", "Select DesignationId As Fid,DesignationName As Designation from Mas_Designation where Deactivate=0 Order By Designation");
                var DesignationList = DapperORM.ReturnList<MAS_DESG>("Sp_QueryExcution", param).ToList();
                ViewBag.DesignationList = DesignationList;

                param.Add("@query", "Select CountryId As Fid,DialCode As CountryCode from Mas_Country where Deactivate=0 ");
                var CountryCodeList = DapperORM.ReturnList<MAS_COUNTRY>("Sp_QueryExcution", param).ToList();
                ViewBag.CountryCodeList = CountryCodeList;
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AddAccountContact", "AddAccountContact", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AddAccountContact", "AddAccountContact", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult AddAccountLocation()
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
                //DapperORM.SetConnection();
                var UserId = Session["EmployeeId"];
                //  var mainRole = Session["MainRole"].ToString();

                //param.Add("@query", $"Select Fid,CompanyName from ACCMAIN where ('admin' = '{mainRole}' or 'salesadmin' = '{mainRole}' or UserId = '{UserId}') and Deleted=0 AND Stage='Won'");
                param.Add("@query", $" Select Distinct ACCMAIN.Fid,Mas_CompanyProfile.CompanyName from ACCMAIN Left JOIN Mas_CompanyProfile ON ACCMAIN.CompanyName=Mas_CompanyProfile.CompanyId where Deleted=0");
                var CompanyList = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).ToList();
                ViewBag.CompanyList = CompanyList;

                param.Add("@query", "Select CountryId As Fid,CountryName from Mas_Country where Deactivate=0");
                var CountryList = DapperORM.ReturnList<MAS_COUNTRY>("Sp_QueryExcution", param).ToList();
                ViewBag.CountryList = CountryList;

                param.Add("@query", "Select StateId As Fid,StateName from Mas_States where Deactivate=0");
                var StateList = DapperORM.ReturnList<MAS_STATE>("Sp_QueryExcution", param).ToList();
                ViewBag.StateList = StateList;

                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AddAccountContact", "AddAccountContact", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AddAccountContact", "AddAccountContact", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult AddAccountCommunication()
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
               // DapperORM.SetConnection();
                var UserId = Session["EmployeeId"];
                //var mainRole = Session["MainRole"].ToString();
                param.Add("@query", $" Select Distinct ACCMAIN.Fid,Mas_CompanyProfile.CompanyName from ACCMAIN Left JOIN Mas_CompanyProfile ON ACCMAIN.CompanyName=Mas_CompanyProfile.CompanyId where Deleted=0 ");
                var CompanyList = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).ToList();
                ViewBag.CompanyList = CompanyList;

                param.Add("@query", "select Fid as Fid,ContactPerson from ACCCONTACT where Deleted=0");
                var ContactPersonList = DapperORM.ReturnList<ACCCONTACT>("Sp_QueryExcution", param).ToList();
                ViewBag.ContactPersonList = ContactPersonList;

                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AddAccountCommunication", "AddAccountCommunication", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AddAccountCommunication", "AddAccountCommunication", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult AddAccountActivity()
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
               // DapperORM.SetConnection();
                var UserId = Session["EmployeeId"];
                //var mainRole = Session["MainRole"].ToString();
                param.Add("@query", $" Select Distinct ACCMAIN.Fid,Mas_CompanyProfile.CompanyName from ACCMAIN Left JOIN Mas_CompanyProfile ON ACCMAIN.CompanyName=Mas_CompanyProfile.CompanyId where Deleted=0");
                var CompanyList = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).ToList();
                ViewBag.CompanyList = CompanyList;

                param.Add("@query", "select Fid as Fid,ContactPerson from ACCCONTACT where Deleted=0");
                var ContactPersonList = DapperORM.ReturnList<ACCCONTACT>("Sp_QueryExcution", param).ToList();
                ViewBag.ContactPersonList = ContactPersonList;


                param.Add("@query", "  Select * from Mas_employee where Deactivate=0 And EmployeeLeft=0");
                var OpsList = DapperORM.ReturnList<MAS_EMPLOYEES>("Sp_QueryExcution", param).ToList();
                ViewBag.OpsList = OpsList;
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AddAccountActivity", "AddAccountActivity", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "AddAccountActivity", "AddAccountActivity", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }
        
        public ActionResult SaveContract1(ACCCONTRACT accContract, string accEncryptedId)
        {
            // Session and user check
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }

            int EmployeeId = Convert.ToInt32(Session["EmployeeId"]);
            var getcon = DapperORM.connectionString;
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(getcon))
                {
                    try
                    {
                        sqlcon.Open();
                    }
                    catch (SqlException sqlx)
                    {
                        string userMsg = " 1 Could not connect to SQL Server. Please check your database connection. Details: " + sqlx.Message.ToString() + "-" + getcon;
                        LoginController.saveAuditEntry(DateTime.Now, EmployeeId, "List", "SaveContract", "SaveContract", "Create",
                            "Failed", userMsg, _clientIp, _clientMachineName);
                        return new HttpStatusCodeResult(500, userMsg);
                    }

                    // Get client/server IP (optional)
                    var ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList
                        .FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString();

                    string Message = "";
                    string Icon = "";

                    try
                    {
                        // Save logic for insert only (Fid==0). You may want to add handling for update if needed.
                        if (accContract.Fid == 0)
                        {
                            // Build parameters for insert
                            DynamicParameters param5 = new DynamicParameters();
                            string process = string.IsNullOrEmpty(accContract.Encrypted_Id) ? "Save" : "Update";
                            param5.Add("@p_process", process);
                            param5.Add("@P_Fid", accContract.Fid);
                            param5.Add("@P_Encrypted_Id", accContract.Encrypted_Id);
                            param5.Add("@p_ACCMAIN_Fid", accContract.ACCMAIN_Fid);
                            param5.Add("@p_BUCode", accContract.BUCode);
                            param5.Add("@p_LegalEntityName", accContract.LegalEntityName);
                            param5.Add("@p_ContractStartDate", accContract.ContractStartDate);
                            param5.Add("@p_ContractEndDate", accContract.ContractEndDate);
                            param5.Add("@p_ContractTenure", accContract.ContractTenure);
                            param5.Add("@p_TotalContractValue", accContract.TotalContractValue);
                            param5.Add("@p_MonthlyContractValue", accContract.MonthlyContractValue);
                            param5.Add("@p_AnnualContractValue", accContract.AnnualContractValue);
                            param5.Add("@p_Currency", accContract.Currency);
                            param5.Add("@p_ACVAmountIncludingTax", accContract.ACVAmountIncludingTax);
                           // param5.Add("@p_ManagementFeeInPercent", accContract.ManagementFeeInPercent);
                          //  param5.Add("@p_ManagementFee", accContract.ManagementFee);
                            param5.Add("@p_InvoiceGenDate", accContract.InvoiceGenDate);
                         //   param5.Add("@p_InvoiceDispatchDate", accContract.InvoiceDispatchDate);
                           // param5.Add("@p_CustomerAcknowledge", accContract.CustomerAcknowledge);
                          //  param5.Add("@p_OJTDate", accContract.OJTDate);
                         //   param5.Add("@p_SiteMobilizationDate", accContract.SiteMobilizationDate);
                          //  param5.Add("@p_RateRevision", accContract.RateRevision);
                            param5.Add("@p_DurationInMonths", accContract.DurationInMonths);
                            param5.Add("@p_PaymentTermsInDays", accContract.PaymentTermsInDays);
                          //  param5.Add("@p_PenaltyApplicable", accContract.PenaltyApplicable);
                          //  param5.Add("@p_IsContractExtended", accContract.IsContractExtended);
                          //  param5.Add("@p_ContractExtendedDate", accContract.ContractExtendedDate);
                         //   param5.Add("@p_ACCSERVICES_Fid", accContract.ACCSERVICES_Fid);
                            //param5.Add("@p_ACCLOCATION_Fid", accContract.ACCLOCATION_Fid);
                            param5.Add("@p_Active", accContract.Active);
                            param5.Add("@p_Deleted", accContract.Deleted);
                            param5.Add("@p_TaxPercentage", accContract.TaxPercentage);
                           // param5.Add("@p_AssociateTraining", accContract.AssociateTraining);
                           // param5.Add("@p_TrainingFrequency", accContract.TrainingFrequency);
                           // param5.Add("@p_TrainingPenalty", accContract.TrainingPenalty);
                            param5.Add("@p_MachineName", Dns.GetHostName().ToString());
                            param5.Add("@p_Mip", ipAddress);
                            param5.Add("@p_UserId", EmployeeId);
                            param5.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 70);
                            param5.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                            param5.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                            var data = DapperORM.ExecuteReturn("Sp_SUD_ACCCONTRACT", param5);

                            //var Connectionstring = connectionString();
                            //var data = DapperORM.ExecuteReturn11("Sp_SUD_ACCCONTRACT", param5, Connectionstring);
                            Message = param5.Get<string>("@p_msg") ?? "";
                            Icon = param5.Get<string>("@p_Icon") ?? "";
                            var ACCCONTRACT_Fid = param5.Get<string>("@p_Id") ?? "0";

                            // === File upload handling ===
                            try
                            {
                             
                                if (accContract.AttachmentFile != null)
                                {
                                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='SalesForce'");
                                    if (GetDocPath == null || !((IDictionary<string, object>)GetDocPath).ContainsKey("DocInitialPath"))
                                        throw new Exception("DocInitialPath not found in Tool_Documnet_DirectoryPath table.");
                                    var GetFirstPath = GetDocPath.DocInitialPath;
                                    var FirstPath = Path.Combine(GetFirstPath, "CRM", "Contract", "Add_Documents", ACCCONTRACT_Fid);
                                    if (!Directory.Exists(FirstPath))
                                    {
                                        Directory.CreateDirectory(FirstPath);
                                    }
                                    if (accContract.AttachmentFile != null)
                                    {
                                        string ImgFilePath = Path.Combine(FirstPath, accContract.AttachmentFile.FileName);
                                        accContract.AttachmentFile.SaveAs(ImgFilePath);
                                        sqlcon.Query("update ACCCONTRACT set Attachment=@FilePath where Fid=@Fid",
                                                     new { FilePath = ImgFilePath, Fid = ACCCONTRACT_Fid });
                                    }
                                }

                                if (accContract.ContractAttachmentFile != null)
                                {
                                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='SalesForce'");
                                    if (GetDocPath == null || !((IDictionary<string, object>)GetDocPath).ContainsKey("DocInitialPath"))
                                        throw new Exception("DocInitialPath not found in Tool_Documnet_DirectoryPath table.");
                                    var GetFirstPath = GetDocPath.DocInitialPath;
                                    var FirstPath = Path.Combine(GetFirstPath, "CRM", "Contract", "Add_Documents", ACCCONTRACT_Fid);
                                    if (!Directory.Exists(FirstPath))
                                    {
                                        Directory.CreateDirectory(FirstPath);
                                    }
                                    if (accContract.ContractAttachmentFile != null)
                                    {
                                        string ImgFilePath = Path.Combine(FirstPath, accContract.ContractAttachmentFile.FileName);
                                        accContract.ContractAttachmentFile.SaveAs(ImgFilePath);
                                        sqlcon.Query("update ACCCONTRACT set ContractAttachment=@FilePath where Fid=@Fid",
                                                     new { FilePath = ImgFilePath, Fid = ACCCONTRACT_Fid });
                                    }
                                }

                            }
                            catch (IOException ioex)
                            {
                                LoginController.saveAuditEntry(DateTime.Now, EmployeeId, "FileIO", "SaveContract", "SaveContract", "Create",
                                    "Failed", ioex.Message.ToString() + "2 -" + getcon, _clientIp, _clientMachineName);
                                return new HttpStatusCodeResult(500, "File operation error: " + ioex.Message);
                            }
                            // ==== End file upload handling ===

                            if (Message != "")
                            {
                                // Save Site details if required
                                DynamicParameters param3 = new DynamicParameters();
                                string process1 = string.IsNullOrEmpty(accContract.Site_Encrypted_Id) ? "Save" : "Update";
                                param3.Add("@p_process", process1);
                                param3.Add("@P_Encrypted_Id", accContract.Site_Encrypted_Id);
                                param3.Add("@p_ACCMAIN_Fid", accContract.ACCMAIN_Fid);
                                param3.Add("@p_SiteCode", accContract.SiteCode);
                                param3.Add("@p_SiteName", accContract.LegalEntityName);
                                param3.Add("@p_SiteType", accContract.SiteType);
                                param3.Add("@p_BUCode", accContract.BUCode);
                                param3.Add("@p_ACCCONTRACT_Fid", ACCCONTRACT_Fid);
                                param3.Add("@p_SiteGrading", accContract.SiteGrading);
                                param3.Add("@p_IsWeekOff", accContract.IsWeekOff);
                               // param3.Add("@p_SiteOJTDate", accContract.OJTDate);
                                param3.Add("@p_SiteOpenDate", accContract.ContractStartDate);
                                param3.Add("@p_InterviewRequired", accContract.InterviewRequired);
                                param3.Add("@p_IsInventory", accContract.IsInventory);
                                param3.Add("@p_InventoryBy", accContract.InventoryBy);
                                param3.Add("@p_UserId", EmployeeId);
                                param3.Add("@p_MachineName", Dns.GetHostName().ToString());
                                param3.Add("@p_Mip", ipAddress);
                                param3.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                                param3.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                               // DapperORM.SetConnection();
                               var data1 = DapperORM.ExecuteReturn1("Sp_SUD_ACCSITES", param3, commandTimeout: 120);
                                Message = param3.Get<string>("@p_msg") ?? Message;
                                Icon = param3.Get<string>("@p_Icon") ?? Icon;
                            }
                        }

                        // Final list fetch (always run)
                        DynamicParameters param1 = new DynamicParameters();
                        param1.Add("@query", $"select t2.Encrypted_Id AS Site_Encrypted_Id, t1.Encrypted_Id AS Contract_Encrypted_Id, t1.*, t2.* from ACCCONTRACT t1 join ACCSITES t2 on t1.Fid = t2.ACCCONTRACT_Fid where t1.Deleted = 0 and t1.ACCMAIN_Fid = '{accContract.ACCMAIN_Fid}' and t2.deleted = 0");
                        var accContractList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();

                        LoginController.saveAuditEntry(DateTime.Now, EmployeeId, "List", "SaveContract", "SaveContract", "Create",
                            "Passed", "", _clientIp, _clientMachineName);

                        // Return result
                        return Json(new { accContractList, Message, Icon }, JsonRequestBehavior.AllowGet);
                    }
                    catch (SqlException sqlx)
                    {
                        LoginController.saveAuditEntry(DateTime.Now, EmployeeId, "SQL", "SaveContract", "SaveContract", "Create",
                            "Failed", sqlx.Message.ToString() + "3 -" + getcon, _clientIp, _clientMachineName);
                        return new HttpStatusCodeResult(500, "SQL Error: " + sqlx.Message);
                    }
                    catch (Exception ex)
                    {
                        LoginController.saveAuditEntry(DateTime.Now, EmployeeId, "General", "SaveContract", "SaveContract", "Create",
                            "Failed", ex.Message.ToString() + "4 -" + getcon, _clientIp, _clientMachineName);
                        return new HttpStatusCodeResult(500, "An error occurred: " + ex.Message);
                    }
                } // using SqlConnection
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, EmployeeId, "TopLevel", "SaveContract", "SaveContract", "Create",
                    "Failed", ex.Message.ToString() + "5 -" + getcon, _clientIp, _clientMachineName);
                return new HttpStatusCodeResult(500, "Unexpected error: " + ex.Message);
            }
        }
        
        public ActionResult SaveLeadManpower(ACCMANPOWER ACCMANPOWER)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
               // DapperORM.SetConnection();
                string process = string.IsNullOrEmpty(ACCMANPOWER.Encrypted_Id) ? "Save" : "Update";

                param.Add("@p_process", process);
                param.Add("@P_Fid", ACCMANPOWER.Fid);
                param.Add("@P_Encrypted_Id", ACCMANPOWER.Encrypted_Id);
                param.Add("@p_ACCMAIN_Fid", ACCMANPOWER.ACCMAIN_Fid);
                param.Add("@p_BUCode", ACCMANPOWER.BUCode);
                param.Add("@p_ACCCONTRACT_Fid", ACCMANPOWER.ACCCONTRACT_Fid);
                param.Add("@p_MAS_DESG_Fid", ACCMANPOWER.MAS_DESG_Fid);
                param.Add("@p_MaleManpowerCount", ACCMANPOWER.MaleManpowerCount);
                param.Add("@p_FemaleManpowerCount", ACCMANPOWER.FemaleManpowerCount);
                param.Add("@p_MaleEarning", ACCMANPOWER.MaleEarning);
                param.Add("@p_FemaleDeduction", ACCMANPOWER.FemaleDeduction);
                param.Add("@p_FemaleEarning", ACCMANPOWER.FemaleEarning);
                param.Add("@p_MaleDeduction", ACCMANPOWER.MaleDeduction);
                param.Add("@p_NoOfDaysInMonth", ACCMANPOWER.NoOfDaysInMonth);
                param.Add("@p_MaxAllowOT", ACCMANPOWER.MaxAllowOT);
                param.Add("@p_Mid", Dns.GetHostByName(MachineId).AddressList[0].ToString());
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_UserId", Session["EmployeeId"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var data = DapperORM.ExecuteReturn("Sp_SUD_ACCMANPOWER", param);
                var Message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "select mp.*,con.LegalEntityName, desg.DesignationId, desg.DesignationName as DesignationName from accmanpower mp join Mas_Designation desg on mp.MAS_DESG_Fid = desg.DesignationId left join ACCCONTRACT con on con.Fid = mp.ACCCONTRACT_Fid where mp.deleted = 0 and con.Deleted = 0  and mp.ACCMAIN_Fid ='" + ACCMANPOWER.ACCMAIN_Fid + "'");
                var ACCManpowerList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();
                //ViewBag.ACCServiceList = ACCServiceList;
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "SaveLeadManpower", "SaveLeadManpower", "Create", "Passed", "", _clientIp, _clientMachineName);

                return Json(new { ACCManpowerList, Message, Icon }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "SaveLeadManpower", "SaveLeadManpower", "Create", "Failed", ex.Message, _clientIp, _clientMachineName);

                return new HttpStatusCodeResult(500, "An internal server error occurred: " + ex.Message);

            }
        }

        #endregion

        #region Lead Transfer

        public ActionResult LeadTransfer()
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
                //  DapperORM.SetConnection();

                var EmpId = Convert.ToInt32(Session["EmployeeId"]);
                param.Add("@query", $"select acc.CompanyName, acc.Deleted AS accsel, acc.Fid as AccFid, ao.* from ACCOWNERS ao  join ACCMAIN acc on ao.ACCMAIN_Fid=acc.Fid and ao.Deleted=0  and acc.Deleted = 0  join MAS_STAGES stage on acc.MAS_STAGES_FID=stage.Fid and stage.Origin='Lead' and acc.Stage!='Lost' and acc.Stage!='Won'");
                var CompanyList = DapperORM.ReturnList<ACCOWNERS>("Sp_QueryExcution", param).ToList();
                ViewBag.CompanyList = CompanyList;

                //param.Add("@query", $"select Distinct acc.CompanyName from ACCOWNERS ao join ACCMAIN acc on ao.ACCMAIN_Fid=acc.Fid and ao.Deleted=0  and acc.Deleted = 0  join MAS_STAGES stage on acc.MAS_STAGES_FID=stage.Fid and stage.Origin='Lead' and acc.Stage!='Lost' and acc.Stage!='Won'");
                param.Add("@query", $" SELECT DISTINCT acc.CompanyName As Fid, mcp.CompanyName FROM ACCOWNERS ao JOIN ACCMAIN acc ON ao.ACCMAIN_Fid = acc.Fid LEFT JOIN Mas_CompanyProfile mcp ON acc.CompanyName = mcp.CompanyId join MAS_STAGES stage on acc.MAS_STAGES_FID=stage.Fid  WHERE ao.Deleted = 0 AND acc.Deleted = 0 and stage.Origin='Lead' and acc.Stage!='Lost' and acc.Stage!='Won'");
                var CompanyList1 = DapperORM.ReturnList<ACCOWNERS>("Sp_QueryExcution", param).ToList();
                ViewBag.CompanyList1 = CompanyList1;

                DynamicParameters param1 = new DynamicParameters();

                //param1.Add("@query", $"SELECT u1.FID, e1.EmployeeName FROM MAS_EMPLOYEES e1 left join USERS u1 on e1.Fid=u1.EmployeeId where e1.Active=1 and e1.EmployeeCategory In ('Operations','Sales') AND u1.FID <> '{EmpId}'");
                param1.Add("@query", $"Select EmployeeId As Fid, EmployeeName From Mas_employee Where Deactivate = 0 AND EmployeeLeft = 0 ORDER BY EmployeeName");
                var EmployeeList = DapperORM.ReturnList<MAS_EMPLOYEES>("Sp_QueryExcution", param1).ToList();
                ViewBag.EmployeeList = EmployeeList;

                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "LeadTransfer", "LeadTransfer", "Create", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "LeadTransfer", "LeadTransfer", "Create", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }
        #endregion

        //        public ActionResult LeadWiseTrnEmployee(int AccmainFid)
        //        {
        //            try
        //            {
        //                DapperORM.SetConnection();
        //                param.Add("@query", "SELECT FID, EMPLOYEENAME FROM MAS_EMPLOYEES WHERE FID NOT IN (SELECT CASE WHEN TranferBy_Fid IS NULL THEN UserId ELSE TranferBy_Fid END FROM ACCMAIN WHERE Deleted = 0 AND Fid = '" + AccmainFid + "');");
        //                var EmployeeList = DapperORM.ReturnList<MAS_EMPLOYEES>("Sp_QueryExcution", param).ToList();
        //                ViewBag.EmployeeList = EmployeeList;
        //                return Json(EmployeeList, JsonRequestBehavior.AllowGet);
        //            }
        //            catch (Exception ex)
        //            {
        //                HomeController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "LeadWiseTrnEmployee", "LeadWiseTrnEmployee", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

        //                return RedirectToAction("ErrorPage", "Login", new { area = "" });
        //            }
        //        }

        //        [HttpGet]
        //        public ActionResult GetbucodeLT(int ACCMAIN_Fid)
        //        {
        //            try
        //            {
        //                DapperORM.SetConnection();
        //                if (Session["MainRole"] == null)
        //                {
        //                    return RedirectToAction("Logout", "Home", new { area = "" });
        //                }

        //                //param.Add("@query", $"SELECT FID, EMPLOYEENAME FROM MAS_EMPLOYEES where deleted=0 and EmployeeCategory In ('Operations','Sales') and Active = 1");
        //                //var EmpList = DapperORM.ReturnList<MAS_EMPLOYEES>("Sp_QueryExcution", param).FirstOrDefault();

        //                param.Add("@query", $"Select BUCode, Fid from ACCMAIN where Fid = '{ACCMAIN_Fid}'");
        //                var BuCode = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).FirstOrDefault();
        //                //var obj = new
        //                //{
        //                //    //EmployeeList = EmpList,
        //                //    BuCode = BuCode
        //                //};
        //                HomeController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetbucodeLT", "GetbucodeLT", "View", "Passed", "", _clientIp, _clientMachineName);

        //                return Json(BuCode, JsonRequestBehavior.AllowGet);
        //            }
        //            catch (Exception Ex)
        //            {
        //                HomeController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetbucodeLT", "GetbucodeLT", "View", "Failed", Ex.Message, _clientIp, _clientMachineName);

        //                return RedirectToAction("ErrorPage", "Login", new { area = "" });
        //            }
        //        }

        public ActionResult IsTransferExist(int ACCMAIN_Fid, int TranferBy_Fid)
        {
            try
            {
                // DapperORM.SetConnection();

                param.Add("@p_process", "IsValidation");

                //param.Add("@p_Encrypted_Id", Encrypted_Id);
                param.Add("@p_Fid", ACCMAIN_Fid);
                param.Add("@p_TranferBy_Fid", TranferBy_Fid);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("Sp_SUD_ACCMAIN", param);
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
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }


        public ActionResult SaveLeadTransfer(ACCOWNERS ACCOWNERS)
        {
            try
            {
                var UserId = Session["EmployeeId"];
                if (UserId == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                
                //param.Add("@p_process", "Update");
                //param.Add("@P_Encrypted_Id", ACCOWNERS.Encrypted_Id);
                //param.Add("@P_Fid", ACCOWNERS.Fid);
                //param.Add("@p_ACCMAIN_Fid", ACCOWNERS.ACCMAIN_Fid);
                //param.Add("@p_BUCode", ACCOWNERS.BUCode);
                //param.Add("@p_LeadOwner_Fid", ACCOWNERS.LeadOwner_Fid);
                //param.Add("@p_OwnershipDate", ACCOWNERS.OwnershipDate);
                //param.Add("@p_TranferBy_Fid", ACCOWNERS.TranferBy_Fid);
                //param.Add("@p_TransferRemark", ACCOWNERS.TransferRemark);
                //param.Add("@p_TransferDate", ACCOWNERS.TransferDate);
                //param.Add("@p_MachineName", Dns.GetHostName().ToString());
                //param.Add("@p_Mip", Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString());
                //param.Add("@p_UserId", Convert.ToInt32(UserId));
                //param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                //param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                //var data = DapperORM.ExecuteReturn("Sp_SUD_ACCOWNERS", param);
                //TempData["Message"] = param.Get<string>("@p_msg");
                //TempData["Icon"] = param.Get<string>("@p_Icon");
                
                string process = string.IsNullOrEmpty(ACCOWNERS.Encrypted_Id) ? "Save" : "Update";
                param.Add("@p_process", "Update");
                param.Add("@P_Fid", ACCOWNERS.Fid);
                param.Add("@P_Encrypted_Id", ACCOWNERS.Encrypted_Id);
                param.Add("@p_ACCMAIN_Fid", ACCOWNERS.ACCMAIN_Fid);
                param.Add("@p_BUCode", ACCOWNERS.BUCode);
                param.Add("@p_LeadOwner_Fid", ACCOWNERS.TranferBy_Fid);
                param.Add("@p_TransferRemark", ACCOWNERS.TransferRemark);
                param.Add("@p_OwnershipDate", ACCOWNERS.OwnershipDate);
                param.Add("@p_TranferBy_Fid", Session["EmployeeId"]);
                //param.Add("@p_TransferDate", ACCOWNERS.TransferDate);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_Mip", Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString());
                param.Add("@p_UserId", Session["EmployeeId"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var data = DapperORM.ExecuteReturn("Sp_SUD_ACCOWNERS", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");


                //    var Message = param.Get<string>("@p_msg");

                //            if (Message != "")
                //            {

                //                DynamicParameters param3 = new DynamicParameters();
                //                //param3.Add("@query", "select MAS_Employees.Fid,EmployeeName,EmailId from  MAS_Employees left join USERS on USERS.EmployeeId = MAS_Employees.Fid Where USERS.Fid = '" + ACCOWNERS.TranferBy_Fid + "' and  USERS.Active = 1 and USERS.Deleted = 0 and MAS_Employees.Deleted = 0 ");
                //                param3.Add("@query", "select Mas_Employee.EmployeeId,Mas_Employee.EmployeeName from  Mas_Employee Where EmployeeId = '" + ACCOWNERS.TranferBy_Fid + "' AND Mas_Employee.Deactivate = 0");
                //                var Com = DapperORM.ReturnList<MAS_EMPLOYEES>("Sp_QueryExcution", param3).FirstOrDefault();
                //                var param4 = new DynamicParameters();
                //                param4.Add("@query", $"select * from ACCMAIN  where Deleted=0 and Fid='" + ACCOWNERS.ACCMAIN_Fid + "'");
                //                var SiteLoc = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param4).FirstOrDefault();
                //                var param5 = new DynamicParameters();
                //                param5.Add("@query", $"select * from ACCCONTACT  where Deleted=0 and ACCMAIN_Fid='" + ACCOWNERS.ACCMAIN_Fid + "' and IsPrimaryContact=1");
                //                var Con = DapperORM.ReturnList<ACCCONTACT>("Sp_QueryExcution", param5).FirstOrDefault();
                //                if (Con == null)
                //                {
                //                    var param9 = new DynamicParameters();
                //                    param9.Add("@query", $"select 'Not Assigned' as ContactName,CompanyEmail as email,CompanyContactNumber as conno from ACCMAIN  where Deleted=0 and Fid='" + ACCOWNERS.ACCMAIN_Fid + "'");
                //                    var Con1 = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param9).FirstOrDefault();
                //                    var c1 = Con1.ContactName;
                //                    var e1 = Con1.email;
                //                    var n1 = Con1.conno;
                //                    TempData["c1"] = c1;
                //                    TempData["e1"] = e1;
                //                    TempData["n1"] = n1;
                //                }
                //                else
                //                {
                //                    var param10 = new DynamicParameters();
                //                    param10.Add("@query", $"select * from ACCCONTACT  where Deleted=0 and ACCMAIN_Fid='" + ACCOWNERS.ACCMAIN_Fid + "' and IsPrimaryContact=1");
                //                    var Con2 = DapperORM.ReturnList<ACCCONTACT>("Sp_QueryExcution", param10).FirstOrDefault();
                //                    var c1 = Con2.ContactPerson;
                //                    var e1 = Con2.EmailId;
                //                    var n1 = Con2.MobileNo;
                //                    TempData["c1"] = c1;
                //                    TempData["e1"] = e1;
                //                    TempData["n1"] = n1;
                //                }
                //                var param6 = new DynamicParameters();
                //                param6.Add("@query", $"select * from ACCLOCATION  where Deleted=0 and ACCMAIN_Fid='" + ACCOWNERS.ACCMAIN_Fid + "'");
                //                var add = DapperORM.ReturnList<ACCLOCATION>("Sp_QueryExcution", param6).FirstOrDefault();
                //                var param7 = new DynamicParameters();
                //                param7.Add("@query", $"select (select LeadSource from MAS_LEADSOURCE where Fid=a.MAS_LEADSOURCE_Fid and Deleted=0) as Leadsource  from ACCMAIN as a where Fid='" + ACCOWNERS.ACCMAIN_Fid + "'");
                //                var lead = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param7).FirstOrDefault();
                //                // var Remark = ;

                //                string c2 = Convert.ToString(TempData["c1"]);
                //                string e2 = Convert.ToString(TempData["e1"]);
                //                string n2 = Convert.ToString(TempData["n1"]);
                //                string EmployeeName = Com?.EmployeeName ?? "N/A";
                //                string Email1 = Com?.EmailId ?? "N/A";
                //                string CompanyName = SiteLoc?.CompanyName ?? "N/A";
                //                string ContactPerson = c2 ?? "N/A";
                //                string Contact = n2 ?? "N/A";
                //                string Email = e2 ?? "N/A";
                //                string address = add?.Address ?? "N/A";
                //                string Leadsource = lead?.Leadsource ?? "N/A";
                //                string Remark = ACCOWNERS.TransferRemark ?? "N/A";
                //                // string Link = "115.124.123.77";

                //                string body = "Hi <b>" + EmployeeName + "</b>,<br><br>" +

                //      "A new lead has been transferred to you.<br><br>" +

                //      "Lead Details:<br><br>" +
                //      "-Name: <b>" + CompanyName + "</b><br>" +
                //      "- Contact person: <b>" + ContactPerson + "</b><br>" +
                //      "- Contact: <b>" + Contact + "</b> /<b>" + Email + " </b><br>" +
                //      "- Address:<b> " + address + "</b><br>" +
                //       "- Source: <b>" + Leadsource + "</b><br>" +
                //               "<br>" +
                //         "- Transfer reason: <b>" + Remark + "</b><br>" +
                //"<br>" +
                //      "Please follow up at your earliest convenience..<br><br>" +

                //      "Thanks,<br>" +
                //      "<b>Sales/OceanFMS</b>";




                //                string Subject = "A new lead has been transferred to you ";

                //                var managerMailid = Email1;

                //                string message = body;
                //                string type = "Service Incentive";

                //                SendEmail(managerMailid, Subject, body, type);

                //                int toid = Convert.ToInt32(Com.Fid);
                //                int fromid = Convert.ToInt32(Session["EmployeeId"]);

                //                string CustomerCode = Convert.ToString(Session["CustomerCode"]);
                //                string LeadType = "Lead";

                //  string apiUrl = $"http://115.124.123.77:8090/api/sendnotification?employeeId={toid}&fromId={fromid}&customerCode={CustomerCode}&LeadType={LeadType}";

                //string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjIwOTAyMjMxOTksImlzcyI6Imh0dHA6Ly9PY2Vhbi5jb20iLCJhdWQiOiJodHRwOi8vT2NlYW4uY29tIn0.nW7tQ7A-s3W43aasUJMsv8BO8RZcNUvNsO5b5R2vn9Q";

                //using (var httpClient = new HttpClient())
                //{
                //    try
                //    {

                //        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                //        var response = httpClient.GetAsync(apiUrl).Result;

                //        if (response.IsSuccessStatusCode)
                //        {
                //            string result = response.Content.ReadAsStringAsync().Result;

                //        }
                //        else
                //        {
                //            Console.WriteLine($"API call failed. Status code: {response.StatusCode}");
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "SaveLeadTransfer", "SaveLeadTransfer", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                //        Console.WriteLine($"Error calling API: {ex.Message}");
                //    }
                //}

                //  }

                return RedirectToAction("LeadTransfer", "CRM", new { area = "CRMS" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public JsonResult GetLeadsByCompany(string companyName)
        {
            // DapperORM.SetConnection();

            var EmpId = Convert.ToInt32(Session["EmployeeId"]);

            DynamicParameters param = new DynamicParameters();
            param.Add("@query", $"select Distinct Fid, LeadName from ACCMAIN where CompanyName ='" + companyName + "' and Deleted=0");
            var CompanyList = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).ToList();
            var result = CompanyList.Select(x => new
            {
                x.Fid,
                x.LeadName
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetOpportunityByCompany(string companyName)
        {
          //  DapperORM.SetConnection();

            var EmpId = Convert.ToInt32(Session["EmployeeId"]);

            DynamicParameters param = new DynamicParameters();

            param.Add("@query", $"select Distinct ACCMAIN.Fid, LeadName from ACCMAIN left join  MAS_STAGES on MAS_STAGES.fid = ACCMAIN.MAS_STAGES_FID where CompanyName ='" + companyName + "' and ACCMAIN.Deleted=0 ");

            var CompanyList = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).ToList();
            var result = CompanyList.Select(x => new
            {
                x.Fid,
                x.LeadName
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllLeadDetails(int ACCMAIN_Fid)
        {
            // DapperORM.SetConnection();

            var result = new LeadInvoiceViewModel();

            // Lead basic details
            var param = new DynamicParameters();
            param.Add("@query", $@"
                SELECT 
                    FORMAT(acc.Fdate,'dd-MM-yyyy') AS LeadCreatedDate,
                    (SELECT EmployeeName FROM Mas_Employee WHERE EmployeeId= acc.UserId) AS CreatedBy,
                    (SELECT LeadSource FROM MAS_LEADSOURCE WHERE Fid= acc.MAS_LEADSOURCE_Fid AND Deleted=0) AS LeadSource,
                    acc.Summary, CONCAT(
                CASE 
                    WHEN (SELECT Origin FROM MAS_STAGES WHERE acc.MAS_STAGES_FID = MAS_STAGES.Fid) ='Prospect' 
                        THEN 'Opportunity' 
                    ELSE (SELECT Origin FROM MAS_STAGES WHERE acc.MAS_STAGES_FID = MAS_STAGES.Fid) 
                END, 
                ' - ', acc.Stage) as Stage
                FROM ACCMAIN AS acc 
                WHERE acc.Fid='{ACCMAIN_Fid}'");

            var leadInfo = DapperORM.ReturnList<LeadInvoiceViewModel>("Sp_QueryExcution", param).FirstOrDefault();
            if (leadInfo != null)
            {
                result.LeadCreatedDate = leadInfo.LeadCreatedDate;
                result.CreatedBy = leadInfo.CreatedBy;
                result.LeadSource = leadInfo.LeadSource;
                result.Summary = leadInfo.Summary;
                result.Stage = leadInfo.Stage;
            }

            // Contact Info
            var param1 = new DynamicParameters();
            param1.Add("@query", $"SELECT ContactPerson, MobileNo, EmailId FROM ACCCONTACT WHERE Deleted=0 AND ACCMAIN_Fid='{ACCMAIN_Fid}'");
            var contact = DapperORM.ReturnList<ACCCONTACT>("Sp_QueryExcution", param1).FirstOrDefault();

            if (contact != null)
            {
                result.ContactPerson = contact.ContactPerson;
                result.MobileNo = contact.MobileNo;
                result.EmailId = contact.EmailId;
            }
            else
            {
                var paramFallback = new DynamicParameters();
                paramFallback.Add("@query", $"SELECT 'Not Assigned' AS ContactPerson, CompanyContactNumber AS MobileNo, CompanyEmail AS EmailId FROM ACCMAIN WHERE Deleted=0 AND Fid='{ACCMAIN_Fid}'");
                var fallback = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", paramFallback).FirstOrDefault();
                if (fallback != null)
                {
                    result.ContactPerson = fallback.ContactPerson;
                    result.MobileNo = fallback.MobileNo;
                    result.EmailId = fallback.EmailId;
                }
            }

            LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetAllLeadDetails", "GetAllLeadDetails", "View", "Passed", "", _clientIp, _clientMachineName);

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        //        public JsonResult GetAllCounts(int TranferBy_Fid)
        //        {
        //            DapperORM.SetConnection();

        //            var result = new LeadInvoiceViewModel();

        //            var param2 = new DynamicParameters();
        //            param2.Add("@query", $"SELECT ISNULL(COUNT(co.LeadOwner_Fid),0) AS LeadOwner_Fid FROM ACCOWNERS AS co JOIN ACCMAIN AS ac ON ac.Fid=co.ACCMAIN_Fid JOIN MAS_STAGES AS st ON st.Fid=ac.MAS_STAGES_FID WHERE st.Origin='Lead' AND st.Stage NOT IN ('Lost','Won') AND co.TranferBy_Fid is null AND co.LeadOwner_Fid='{TranferBy_Fid}' AND co.Deleted=0 AND ac.Deleted=0");
        //            result.LeadOwnerCount = DapperORM.ReturnList<int>("Sp_QueryExcution", param2).FirstOrDefault();

        //            // TransferBy Count
        //            var param3 = new DynamicParameters();
        //            param3.Add("@query", $"SELECT ISNULL(COUNT(co.TranferBy_Fid),0) AS TranferBy_Fid FROM ACCOWNERS AS co JOIN ACCMAIN AS ac ON ac.Fid=co.ACCMAIN_Fid JOIN MAS_STAGES AS st ON st.Fid=ac.MAS_STAGES_FID WHERE st.Origin='Lead' AND st.Stage NOT IN ('Lost', 'Won') AND co.TranferBy_Fid='{TranferBy_Fid}' AND co.Deleted=0 AND ac.Deleted=0");
        //            result.TransferByCount = DapperORM.ReturnList<int>("Sp_QueryExcution", param3).FirstOrDefault();

        //            var param4 = new DynamicParameters();
        //            param2.Add("@query", $"SELECT ISNULL(COUNT(co.LeadOwner_Fid),0) AS LeadOwner_Fid FROM ACCOWNERS AS co JOIN ACCMAIN AS ac ON ac.Fid=co.ACCMAIN_Fid JOIN MAS_STAGES AS st ON st.Fid=ac.MAS_STAGES_FID WHERE st.Origin='Prospect' AND st.Stage NOT IN ('Lost','Won') AND co.TranferBy_Fid is null AND co.LeadOwner_Fid='{TranferBy_Fid}' AND co.Deleted=0 AND ac.Deleted=0");
        //            result.opportunityCount = DapperORM.ReturnList<int>("Sp_QueryExcution", param4).FirstOrDefault();

        //            // TransferBy Count
        //            var param5 = new DynamicParameters();
        //            param3.Add("@query", $"SELECT ISNULL(COUNT(co.TranferBy_Fid),0) AS TranferBy_Fid FROM ACCOWNERS AS co JOIN ACCMAIN AS ac ON ac.Fid=co.ACCMAIN_Fid JOIN MAS_STAGES AS st ON st.Fid=ac.MAS_STAGES_FID WHERE st.Origin='Prospect' AND st.Stage NOT IN ('Lost', 'Won') AND co.TranferBy_Fid='{TranferBy_Fid}' AND co.Deleted=0 AND ac.Deleted=0");
        //            result.opportunityTransferByCount = DapperORM.ReturnList<int>("Sp_QueryExcution", param5).FirstOrDefault();


        //            HomeController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "GetAllCounts", "GetAllCounts", "View", "Passed", "", _clientIp, _clientMachineName);

        //            return Json(result, JsonRequestBehavior.AllowGet);
        //        }

        //        #endregion

        #region Oppportunity Transfer

        public ActionResult OppportunityTransfer()
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
                var EmpId = Convert.ToInt32(Session["EmployeeId"]);
                param.Add("@query", $"select acc.CompanyName, acc.Deleted AS accsel, acc.Fid as AccFid, ao.* from ACCOWNERS ao  join ACCMAIN acc on ao.ACCMAIN_Fid=acc.Fid and ao.Deleted=0  and acc.Deleted = 0  join MAS_STAGES stage on acc.MAS_STAGES_FID=stage.Fid and stage.Origin='Lead' and acc.Stage!='Lost' and acc.Stage!='Won'");
                var CompanyList = DapperORM.ReturnList<ACCOWNERS>("Sp_QueryExcution", param).ToList();
                ViewBag.CompanyList = CompanyList;

                param.Add("@query", $" SELECT DISTINCT acc.CompanyName As Fid, mcp.CompanyName FROM ACCOWNERS ao JOIN ACCMAIN acc ON ao.ACCMAIN_Fid = acc.Fid LEFT JOIN Mas_CompanyProfile mcp ON acc.CompanyName = mcp.CompanyId join MAS_STAGES stage on acc.MAS_STAGES_FID=stage.Fid  WHERE ao.Deleted = 0 AND acc.Deleted = 0 and stage.Origin='Lead' and acc.Stage!='Lost' and acc.Stage!='Won'");
                var CompanyList1 = DapperORM.ReturnList<ACCOWNERS>("Sp_QueryExcution", param).ToList();
                ViewBag.CompanyList1 = CompanyList1;

                DynamicParameters param1 = new DynamicParameters();

                param1.Add("@query", $"Select EmployeeId As Fid, EmployeeName From Mas_employee Where Deactivate = 0 AND EmployeeLeft = 0 ORDER BY EmployeeName");
                var EmployeeList = DapperORM.ReturnList<MAS_EMPLOYEES>("Sp_QueryExcution", param1).ToList();
                ViewBag.EmployeeList = EmployeeList;

                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "OppportunityTransfer", "OppportunityTransfer", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "OppportunityTransfer", "OppportunityTransfer", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }
        #endregion


        public ActionResult SaveOppportunityTransfer(ACCOWNERS ACCOWNERS)
        {
            try
            {
              //  DapperORM.SetConnection();
                string process = string.IsNullOrEmpty(ACCOWNERS.Encrypted_Id) ? "Save" : "Update";
                param.Add("@p_process", "Update");
                param.Add("@P_Fid", ACCOWNERS.Fid);
                param.Add("@P_Encrypted_Id", ACCOWNERS.Encrypted_Id);
                param.Add("@p_ACCMAIN_Fid", ACCOWNERS.ACCMAIN_Fid);
                param.Add("@p_BUCode", ACCOWNERS.BUCode);
                param.Add("@p_LeadOwner_Fid", ACCOWNERS.TranferBy_Fid);
                param.Add("@p_TransferRemark", ACCOWNERS.TransferRemark);
                param.Add("@p_OwnershipDate", ACCOWNERS.OwnershipDate);
                param.Add("@p_TranferBy_Fid", Session["EmployeeId"]);
                //param.Add("@p_TransferDate", ACCOWNERS.TransferDate);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_Mip", Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString());
                param.Add("@p_UserId", Session["EmployeeId"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var data = DapperORM.ExecuteReturn("Sp_SUD_ACCOWNERS", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");

    //            var Message = param.Get<string>("@p_msg");

    //            if (Message != "")
    //            {

    //                DynamicParameters param3 = new DynamicParameters();
    //                param3.Add("@query", "select Mas_Employee.EmployeeId,Mas_Employee.EmployeeName from  Mas_Employee Where EmployeeId = '" + ACCOWNERS.TranferBy_Fid + "' AND Mas_Employee.Deactivate = 0");
    //                var Com = DapperORM.ReturnList<MAS_EMPLOYEES>("Sp_QueryExcution", param3).FirstOrDefault();
    //                var param4 = new DynamicParameters();
    //                param4.Add("@query", $"select * from ACCMAIN  where Deleted=0 and Fid='" + ACCOWNERS.ACCMAIN_Fid + "'");
    //                var SiteLoc = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param4).FirstOrDefault();
    //                var param5 = new DynamicParameters();
    //                param5.Add("@query", $"select * from ACCCONTACT  where Deleted=0 and ACCMAIN_Fid='" + ACCOWNERS.ACCMAIN_Fid + "' and IsPrimaryContact=1");
    //                var Con = DapperORM.ReturnList<ACCCONTACT>("Sp_QueryExcution", param5).FirstOrDefault();
    //                if (Con == null)
    //                {
    //                    var param9 = new DynamicParameters();
    //                    param9.Add("@query", $"select 'Not Assigned' as ContactName,CompanyEmail as email,CompanyContactNumber as conno from ACCMAIN  where Deleted=0 and Fid='" + ACCOWNERS.ACCMAIN_Fid + "'");
    //                    var Con1 = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param9).FirstOrDefault();
    //                    var c1 = Con1.ContactName;
    //                    var e1 = Con1.email;
    //                    var n1 = Con1.conno;
    //                    TempData["c1"] = c1;
    //                    TempData["e1"] = e1;
    //                    TempData["n1"] = n1;
    //                }
    //                else
    //                {
    //                    var param10 = new DynamicParameters();
    //                    param10.Add("@query", $"select * from ACCCONTACT  where Deleted=0 and ACCMAIN_Fid='" + ACCOWNERS.ACCMAIN_Fid + "' and IsPrimaryContact=1");
    //                    var Con2 = DapperORM.ReturnList<ACCCONTACT>("Sp_QueryExcution", param10).FirstOrDefault();
    //                    var c1 = Con2.ContactPerson;
    //                    var e1 = Con2.EmailId;
    //                    var n1 = Con2.MobileNo;
    //                    TempData["c1"] = c1;
    //                    TempData["e1"] = e1;
    //                    TempData["n1"] = n1;
    //                }
    //                var param6 = new DynamicParameters();
    //                param6.Add("@query", $"select * from ACCLOCATION  where Deleted=0 and ACCMAIN_Fid='" + ACCOWNERS.ACCMAIN_Fid + "'");
    //                var add = DapperORM.ReturnList<ACCLOCATION>("Sp_QueryExcution", param6).FirstOrDefault();
    //                var param7 = new DynamicParameters();
    //                param7.Add("@query", $"select (select LeadSource from MAS_LEADSOURCE where Fid=a.MAS_LEADSOURCE_Fid and Deleted=0) as Leadsource  from ACCMAIN as a where Fid='" + ACCOWNERS.ACCMAIN_Fid + "'");
    //                var lead = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param7).FirstOrDefault();
    //                // var Remark = ;

    //                string c2 = Convert.ToString(TempData["c1"]);
    //                string e2 = Convert.ToString(TempData["e1"]);
    //                string n2 = Convert.ToString(TempData["n1"]);
    //                string EmployeeName = Com?.EmployeeName ?? "N/A";
    //                string Email1 = Com?.EmailId ?? "N/A";
    //                string CompanyName = SiteLoc?.CompanyName ?? "N/A";
    //                string ContactPerson = c2 ?? "N/A";
    //                string Contact = n2 ?? "N/A";
    //                string Email = e2 ?? "N/A";
    //                string address = add?.Address ?? "N/A";
    //                string Leadsource = lead?.Leadsource ?? "N/A";
    //                string Remark = ACCOWNERS.TransferRemark ?? "N/A";
    //                // string Link = "115.124.123.77";

    //                string body = "Hi <b>" + EmployeeName + "</b>,<br><br>" +

    //      "A new Oppportunity has been transferred to you.<br><br>" +

    //      "Lead Details:<br><br>" +
    //      "-Name: <b>" + CompanyName + "</b><br>" +
    //      "- Contact person: <b>" + ContactPerson + "</b><br>" +
    //      "- Contact: <b>" + Contact + "</b> /<b>" + Email + " </b><br>" +
    //       "- Address:<b> " + address + "</b><br>" +
    //        "- Source: <b>" + Leadsource + "</b><br>" +
    //               "<br>" +
    //         "- Transfer reason: <b>" + Remark + "</b><br>" +
    //// "- Notes: " +  + "<br><br>" +
    //"<br>" +
    //      "Please follow up at your earliest convenience..<br><br>" +

    //      "Thanks,<br>" +
    //      "<b>Sales/OceanFMS</b>";




    //                string Subject = "A new Oppportunity has been transferred to you. ";

    //                var managerMailid = Email1;
    //                // var managerMailid = "rahul@probus.co.in";
    //                string message = body;
    //                string type = "Service Incentive";
    //                //TwilioClient.Init(accountSid, authToken);

    //                SendEmail(managerMailid, Subject, body, type);

    //                int toid = Convert.ToInt32(Com.Fid);
    //                int fromid = Convert.ToInt32(Session["EmployeeId"]);

    //                string CustomerCode = Convert.ToString(Session["CustomerCode"]);
    //                string LeadType = "Opportunity";

    //                string apiUrl = $"http://115.124.123.77:8090/api/sendnotification?employeeId={toid}&fromId={fromid}&customerCode={CustomerCode}&LeadType={LeadType}";
    //                // test  string apiUrl = $"http://115.124.123.77:8092/api/sendnotification?employeeId={toid}&fromId={fromid}";

    //                // Your token
    //                string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjIwOTAyMjMxOTksImlzcyI6Imh0dHA6Ly9PY2Vhbi5jb20iLCJhdWQiOiJodHRwOi8vT2NlYW4uY29tIn0.nW7tQ7A-s3W43aasUJMsv8BO8RZcNUvNsO5b5R2vn9Q";

    //                using (var httpClient = new HttpClient())
    //                {
    //                    try
    //                    {
    //                        // Set Authorization header
    //                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    //                        var response = httpClient.GetAsync(apiUrl).Result;  // Synchronous

    //                        if (response.IsSuccessStatusCode)
    //                        {
    //                            string result = response.Content.ReadAsStringAsync().Result;
    //                            // Optional: process 'result' or log into DB
    //                        }
    //                        else
    //                        {
    //                            Console.WriteLine($"API call failed. Status code: {response.StatusCode}");
    //                        }
    //                    }
    //                    catch (Exception ex)
    //                    {
    //                        LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "SaveOppportunityTransfer", "SaveOppportunityTransfer", "Create", "Failed", ex.Message, _clientIp, _clientMachineName);

    //                        Console.WriteLine($"Error calling API: {ex.Message}");
    //                    }
                   // }
             //   }
                return RedirectToAction("OppportunityTransfer", "CRM", new { area = "CRMS" });

            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }
        
        //        #region Lead Reports

        public ActionResult MAS_ReportList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                // DapperORM.SetConnection();
                {
                    param.Add("@query", "SELECT DISTINCT Module FROM MAS_REPORT where Deleted = 0");
                    var ReportList = DapperORM.ReturnList<MAS_REPORT>("Sp_QueryExcution", param).ToList();
                    ViewBag.ReportList = ReportList;
                }
               LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "MAS_ReportList", "MAS_ReportList", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "MAS_ReportList", "MAS_ReportList", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult MAS_ReportNameList(string modules)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                int Id;
                Id = Convert.ToInt32(Session["EmployeeId"]);

                string query = $"select Distinct SubModule from MAS_REPORT where Module = '{modules}'";

                var parameters = new DynamicParameters();
                //parameters.Add("Module", modules);

                //if (Session["MainRole"].ToString() != "Admin")
                //{
                //    query += " and t2.USERS_Fid = @UserId";
                //    parameters.Add("UserId", Id);
                //}
               // DapperORM.SetConnection();
                {
                    var ReportNameList = sqlcon.Query<MAS_REPORT>(query, parameters).ToList();
                    ViewBag.ReportNameList = ReportNameList;
                }

                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "MAS_ReportNameList", "MAS_ReportNameList", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "MAS_ReportNameList", "MAS_ReportNameList", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult ViewListList(string modules)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                int Id = Convert.ToInt32(Session["EmployeeId"]);
               // var mainRole = Session["MainRole"].ToString();

                //string query = $"  select Distinct  t1.* from MAS_REPORT t1 join MAS_REPORTMAPPING t2 on t1.Fid = t2.MAS_REPORT_Fid where t1.SubModule = '{modules}'  and t2.Deleted = 0 and  ('admin' = '{mainRole}' or 'SalesAdmin' = '{mainRole}' or t2.USERS_Fid = '{Id}')";

                string query = $"select Distinct  t1.* from MAS_REPORT t1 join MAS_REPORTMAPPING t2 on t1.Fid = t2.MAS_REPORT_Fid where t2.Deleted=0 AND t1.SubModule = '{modules}' AND t2.USERS_Fid = '{Id}'";
                var parameters = new DynamicParameters();
                ViewBag.module = modules;

                //  DapperORM.SetConnection();
                {
                    var ReportNameList = sqlcon.Query<MAS_REPORT>(query, parameters).ToList();
                    ViewBag.ReportNameList = ReportNameList;
                }
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "ViewListList", "ViewListList", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "ViewListList", "ViewListList", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult Lead_by_source(DateTime? FromDate, DateTime? ToDate, int? LeadSource)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                int leadOwnerCount = 0;
                int isCoOwnerCount = 0;
                int id = Convert.ToInt32(Session["EmployeeId"]);
                List<int> transferByFids = new List<int>();
                List<int> accmainFids = new List<int>();
                string EmployeeName = Convert.ToString(Session["EMPName"]);
                // string baseQuery = $"SELECT t3.LeadSource,(select E1.EmployeeName from USERS users join MAS_EMPLOYEES E1 on E1.Fid = users.EmployeeId where users.Fid=t1.UserId)as EmployeeName , t1.* FROM ACCMAIN t1 JOIN USERS t2 ON t1.UserId = t2.Fid JOIN MAS_LEADSOURCE t3 ON t1.MAS_LEADSOURCE_Fid = t3.Fid left join MAS_STAGES stage on t1.MAS_STAGES_FID=stage.Fid WHERE t1.Deleted = 0 and t1.Stage in ('Identification', 'Qualification')";

                // string baseQuery = $"SELECT t3.LeadSource,t1.LeadName,t1.CompanyName,t1.Fdate, E1.EmployeeName FROM ACCMAIN t1 JOIN Tool_UserLogin t2 ON t1.UserId = t2.EmployeeId JOIN MAS_EMPLOYEE E1 ON t2.EmployeeId = E1.EmployeeId JOIN MAS_LEADSOURCE t3 ON t1.MAS_LEADSOURCE_Fid = t3.Fid LEFT JOIN MAS_STAGES stage ON t1.MAS_STAGES_FID = stage.Fid WHERE t1.Deleted = 0 AND t1.Stage IN ('Identification', 'Qualification')GROUP BY t3.LeadSource,t1.LeadName,t1.CompanyName,t1.Fdate, E1.EmployeeName";

                //string baseQuery = @"SELECT  t3.LeadSource,t1.LeadName,t1.CompanyName, t1.Fdate,E1.EmployeeName FROM ACCMAIN t1 JOIN Tool_UserLogin t2 ON t1.UserId = t2.EmployeeId JOIN MAS_EMPLOYEE E1 ON t2.EmployeeId = E1.EmployeeId JOIN MAS_LEADSOURCE t3 ON t1.MAS_LEADSOURCE_Fid = t3.Fid LEFT JOIN MAS_STAGES stage ON t1.MAS_STAGES_FID = stage.Fid WHERE t1.Deleted = 0 AND t1.Stage IN ('Identification', 'Qualification')";
                string baseQuery = @"SELECT  t3.LeadSource,t1.LeadName,t1.CompanyName As CompanyId, t1.Fdate,E1.EmployeeName,t4.CompanyName FROM ACCMAIN t1 
                  JOIN Tool_UserLogin t2 ON t1.UserId = t2.EmployeeId JOIN MAS_EMPLOYEE E1 ON t2.EmployeeId = E1.EmployeeId JOIN MAS_LEADSOURCE t3 ON t1.MAS_LEADSOURCE_Fid = t3.Fid LEFT JOIN MAS_STAGES stage ON 
                   t1.MAS_STAGES_FID = stage.Fid  Left Join Mas_CompanyProfile t4 ON t4.CompanyId=t1.CompanyName WHERE t1.Deleted = 0 AND t1.Stage IN ('Identification', 'Qualification')";
                var parameters = new DynamicParameters();

                //if (Session["MainRole"].ToString() != "Admin" && Session["MainRole"].ToString() != "SalesAdmin")
                //{
                //    baseQuery += " AND t1.UserId = @UserId";
                //    parameters.Add("UserId", id);
                //}
                if (LeadSource != null)
                {
                    baseQuery += " AND t1.MAS_LEADSOURCE_Fid = @LeadSource";
                    parameters.Add("LeadSource", LeadSource);
                }

                if (FromDate != null && ToDate != null)
                {
                    baseQuery += " AND t1.Fdate BETWEEN @FromDate AND @ToDate";
                    parameters.Add("FromDate", FromDate);
                    parameters.Add("ToDate", ToDate.Value.AddDays(1));
                }
                else if (FromDate != null)
                {
                    ToDate = DateTime.Now.Date;
                    baseQuery += " AND t1.Fdate >= @FromDate";
                    parameters.Add("FromDate", FromDate);
                }

                //DapperORM.SetConnection();
                {

                    baseQuery += " GROUP BY t3.LeadSource, t1.LeadName,t1.CompanyName, t4.CompanyName, t1.Fdate, E1.EmployeeName";

                    var leadBySourceList = sqlcon.Query<ACCMAIN>(baseQuery, parameters).ToList();

                  //  var leadBySourceList = sqlcon.Query<ACCMAIN>(baseQuery, parameters).ToList();
                    ViewBag.Lead_by_sourceList = leadBySourceList;

                    var leadSourceQuery = "SELECT * FROM MAS_LEADSOURCE WHERE Deleted = 0";
                    var Leadsource1 = sqlcon.Query<MAS_LEADSOURCE>(leadSourceQuery).ToList();
                    ViewBag.Leadsource1 = Leadsource1;
                }

                ViewBag.Module = Convert.ToString(Session["Module"]);
                ViewBag.EmployeeName = EmployeeName;
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Lead_by_source", "Lead_by_source", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Lead_by_source", "Lead_by_source", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                // Log the exception if needed
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult Lead_by_Stage(int? LeadStages, DateTime? FromDate, DateTime? ToDate)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                int leadOwnerCount = 0;
                int isCoOwnerCount = 0;
                List<int> transferByFids = new List<int>();
                List<int> accmainFids = new List<int>();
                var parameters = new DynamicParameters();
                string EmployeeName = Convert.ToString(Session["EMPName"]);
                int id = Convert.ToInt32(Session["EmployeeId"]);
                //  string query = @"SELECT  t3.LeadSource,stage.Stage,t1.LeadName,t1.CompanyName, t1.Fdate,E1.EmployeeName FROM ACCMAIN t1 JOIN Tool_UserLogin t2 ON t1.UserId = t2.EmployeeId JOIN MAS_EMPLOYEE E1 ON t2.EmployeeId = E1.EmployeeId JOIN MAS_LEADSOURCE t3 ON t1.MAS_LEADSOURCE_Fid = t3.Fid LEFT JOIN MAS_STAGES stage ON t1.MAS_STAGES_FID = stage.Fid WHERE t1.Deleted = 0 AND t1.Stage IN ('Identification', 'Qualification')";
                 string query = @"SELECT  t3.LeadSource,stage.Stage,t1.LeadName,t1.CompanyName as CompanyId,t4.CompanyName, t1.Fdate,E1.EmployeeName FROM ACCMAIN t1 JOIN Tool_UserLogin t2 ON t1.UserId = t2.EmployeeId JOIN MAS_EMPLOYEE E1 ON t2.EmployeeId = E1.EmployeeId JOIN MAS_LEADSOURCE t3 ON t1.MAS_LEADSOURCE_Fid = t3.Fid LEFT JOIN MAS_STAGES stage ON t1.MAS_STAGES_FID = stage.Fid Left Join Mas_CompanyProfile t4 ON t4.CompanyId=t1.CompanyName WHERE t1.Deleted = 0 AND t1.Stage IN ('Identification', 'Qualification')";

                parameters.Add("@query", $"SELECT * FROM ACCOWNERS WHERE Deleted = 0 AND (LeadOwner_Fid = '{id}' OR UserId = '{id}')");
                var ACCOWNERS = DapperORM.ReturnList<ACCOWNERS>("Sp_QueryExcution", parameters).ToList();

                if (LeadStages != null)
                {
                    query += " and t1.MAS_STAGES_FID = @LeadStages";
                    parameters.Add("LeadStages", LeadStages);
                }


                if (FromDate != null && ToDate != null)
                {
                    query += " and t1.Fdate between @FromDate and @ToDate";
                    parameters.Add("FromDate", FromDate);
                    parameters.Add("ToDate", ToDate.Value.AddDays(1));
                }
                else if (FromDate != null)
                {
                    ToDate = DateTime.Now.Date;
                    query += " and t1.Fdate >= @FromDate";
                    parameters.Add("FromDate", FromDate);
                }
                
                {

                    query += " GROUP BY t3.LeadSource,stage.Stage, t1.LeadName, t1.CompanyName,t4.CompanyName, t1.Fdate, E1.EmployeeName";

                    var Lead_by_StagesList = sqlcon.Query<ACCMAIN>(query, parameters).ToList();
                    ViewBag.Lead_by_StagesList = Lead_by_StagesList;

                    param.Add("@query", "SELECT * FROM MAS_STAGES where Deleted = 0 and origin = 'Lead' AND stage <> 'Won'");
                    var Leadstages = DapperORM.ReturnList<MAS_STAGES>("Sp_QueryExcution", param).ToList();

                    ViewBag.Leadstages = Leadstages;
                }


                ViewBag.Module = Session["Module"];
                ViewBag.EmployeeName = EmployeeName;
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Lead_by_Stage", "Lead_by_Stage", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Lead_by_Stage", "Lead_by_Stage", "View", "Passed", "", _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult Lead_by_Industry(DateTime? FromDate, DateTime? ToDate, int? IndustryType)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                int leadOwnerCount = 0;
                int isCoOwnerCount = 0;
                List<int> transferByFids = new List<int>();
                List<int> accmainFids = new List<int>();
                var parameters = new DynamicParameters();
                string EmployeeName = Convert.ToString(Session["EMPName"]);

                int id = Convert.ToInt32(Session["EmployeeId"]);
                //string query = @"SELECT Distinct t3.IndustryType,stage.Stage,t1.LeadName,t1.CompanyName, t1.Fdate,E1.EmployeeName FROM ACCMAIN t1 JOIN Tool_UserLogin t2 ON t1.UserId = t2.EmployeeId JOIN MAS_EMPLOYEE E1 ON t2.EmployeeId = E1.EmployeeId JOIN MAS_INDUSTRYTYPE t3 ON t1.MAS_INDUSTRYTYPE_Fid = t3.Fid LEFT JOIN MAS_STAGES stage ON t1.MAS_STAGES_FID = stage.Fid WHERE t1.Deleted = 0 AND t1.Stage IN ('Identification', 'Qualification')";
                string query = @"SELECT Distinct t3.IndustryType,stage.Stage,t1.LeadName,t1.CompanyName As CompanyId,t4.CompanyName, t1.Fdate,E1.EmployeeName FROM ACCMAIN t1 JOIN Tool_UserLogin t2 ON t1.UserId = t2.EmployeeId JOIN MAS_EMPLOYEE E1 ON t2.EmployeeId = E1.EmployeeId JOIN MAS_INDUSTRYTYPE t3 ON t1.MAS_INDUSTRYTYPE_Fid = t3.Fid LEFT JOIN MAS_STAGES stage ON t1.MAS_STAGES_FID = stage.Fid Left Join Mas_CompanyProfile t4 ON t4.CompanyId=t1.CompanyName WHERE t1.Deleted = 0 AND t1.Stage IN ('Identification', 'Qualification')";

                parameters.Add("@query", $"SELECT * FROM ACCOWNERS WHERE Deleted = 0 AND (LeadOwner_Fid = '{id}' OR UserId = '{id}')");
                var ACCOWNERS = DapperORM.ReturnList<ACCOWNERS>("Sp_QueryExcution", parameters).ToList();

                if (IndustryType != null)
                {
                    query += " and t1.MAS_INDUSTRYTYPE_Fid = @IndustryType";
                    parameters.Add("IndustryType", IndustryType);
                }


                if (FromDate != null && ToDate != null)
                {
                    query += " and t1.Fdate between @FromDate and @ToDate";
                    parameters.Add("FromDate", FromDate);
                    parameters.Add("ToDate", ToDate.Value.AddDays(1));
                }
                else if (FromDate != null)
                {
                    ToDate = DateTime.Now.Date;
                    query += " and t1.Fdate >= @FromDate";
                    parameters.Add("FromDate", FromDate);
                }



                //if (Session["MainRole"].ToString() != "Admin" && Session["MainRole"].ToString() != "SalesAdmin")
                //{
                //    query += " and t1.UserId = @UserId";
                //    parameters.Add("UserId", id);
                //}

                //DapperORM.SetConnection();
                //{
                //    var Lead_by_IndustryTypeList = sqlcon.Query<ACCMAIN>(query, parameters).ToList();
                //    ViewBag.Lead_by_IndustryTypeList = Lead_by_IndustryTypeList;
                //    param.Add("@query", "SELECT * FROM MAS_INDUSTRYTYPE where Deleted = 0");
                //    var LeadIndusrty = DapperORM.ReturnList<MAS_INDUSTRYTYPE>("Sp_QueryExcution", param).ToList();
                //    ViewBag.LeadIndusrty = LeadIndusrty;
                //}
                {

                    query += " GROUP BY t3.IndustryType,stage.Stage, t1.LeadName, t1.CompanyName,t4.CompanyName, t1.Fdate, E1.EmployeeName";

                    var Lead_by_IndustryTypeList = sqlcon.Query<ACCMAIN>(query, parameters).ToList();
                    ViewBag.Lead_by_IndustryTypeList = Lead_by_IndustryTypeList;

                    param.Add("@query", "SELECT * FROM MAS_INDUSTRYTYPE where Deleted = 0");
                    var LeadIndustry = DapperORM.ReturnList<MAS_INDUSTRYTYPE>("Sp_QueryExcution", param).ToList();

                    ViewBag.LeadIndustry = LeadIndustry;
                }



                ViewBag.Module = Session["Module"];
                ViewBag.EmployeeName = EmployeeName;
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Lead_by_Industry", "Lead_by_Industry", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Lead_by_Industry", "Lead_by_Industry", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult Lost_Lead_Report(DateTime? FromDate, DateTime? ToDate)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                int leadOwnerCount = 0;
                int isCoOwnerCount = 0;
                List<int> transferByFids = new List<int>();
                List<int> accmainFids = new List<int>();
                var parameters = new DynamicParameters();
                string EmployeeName = Convert.ToString(Session["EmployeeName"]);
                int id = Convert.ToInt32(Session["EmployeeId"]);
              //  string query = $"SELECT Distinct t3.LeadSource,stage.Stage,t1.LeadName,t1.CompanyName, t1.Fdate,E1.EmployeeName FROM ACCMAIN t1 JOIN Tool_UserLogin t2 ON t1.UserId = t2.EmployeeId JOIN MAS_EMPLOYEE E1 ON t2.EmployeeId = E1.EmployeeId JOIN MAS_LEADSOURCE t3 ON t1.MAS_INDUSTRYTYPE_Fid = t3.Fid LEFT JOIN MAS_STAGES stage ON t1.MAS_STAGES_FID = stage.Fid WHERE t1.Deleted = 0 AND stage.Origin = 'Lead' and stage.Stage = 'Lost'";
                string query = $"SELECT Distinct t3.LeadSource,stage.Stage,t1.LeadName,t1.CompanyName as CompanyId,t4.CompanyName, t1.Fdate,E1.EmployeeName FROM ACCMAIN t1 JOIN Tool_UserLogin t2 ON t1.UserId = t2.EmployeeId JOIN MAS_EMPLOYEE E1 ON t2.EmployeeId = E1.EmployeeId JOIN MAS_LEADSOURCE t3 ON t1.MAS_INDUSTRYTYPE_Fid = t3.Fid LEFT JOIN MAS_STAGES stage ON t1.MAS_STAGES_FID = stage.Fid Left Join Mas_CompanyProfile t4 ON t4.CompanyId=t1.CompanyName WHERE t1.Deleted = 0 AND stage.Origin = 'Lead' and stage.Stage = 'Lost'";

                parameters.Add("@query", $"SELECT * FROM ACCOWNERS WHERE Deleted = 0 AND (LeadOwner_Fid = '{id}' OR UserId = '{id}')");
                var ACCOWNERS = DapperORM.ReturnList<ACCOWNERS>("Sp_QueryExcution", parameters).ToList();



                //if (LeadLost != null)
                //{
                //    query += " and t1.MAS_STAGES_FID = @LeadLost";
                //    parameters.Add("LeadLost", LeadLost);
                //}


                if (FromDate != null && ToDate != null)
                {
                    query += " and t1.Fdate between @FromDate and @ToDate";
                    parameters.Add("FromDate", FromDate);
                    parameters.Add("ToDate", ToDate.Value.AddDays(1));
                }
                else if (FromDate != null)
                {
                    ToDate = DateTime.Now.Date;
                    query += " and t1.Fdate >= @FromDate";
                    parameters.Add("FromDate", FromDate);
                }
                
                //DapperORM.SetConnection();
                {
                    var Lead_by_StagesList = sqlcon.Query<ACCMAIN>(query, parameters).ToList();
                    ViewBag.Lead_by_StagesList = Lead_by_StagesList;
                }


                {

                    query += " GROUP BY t3.LeadSource,stage.Stage, t1.LeadName, t1.CompanyName, t4.CompanyName,t1.Fdate, E1.EmployeeName";

                    var Lead_by_StagesList = sqlcon.Query<ACCMAIN>(query, parameters).ToList();
                    ViewBag.Lead_by_StagesList = Lead_by_StagesList;

                    //param.Add("@query", "SELECT * FROM MAS_INDUSTRYTYPE where Deleted = 0");
                    //var LeadIndustry = DapperORM.ReturnList<MAS_INDUSTRYTYPE>("Sp_QueryExcution", param).ToList();

                    //ViewBag.LeadIndustry = LeadIndustry;
                }
                ViewBag.Module = Session["Module"];
                ViewBag.EmployeeName = EmployeeName;
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Lost_Lead_Report", "Lost_Lead_Report", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();

            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Lost_Lead_Report", "Lost_Lead_Report", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult Lead_Contact_List(string Contact)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                int leadOwnerCount = 0;
                int isCoOwnerCount = 0;
                List<int> transferByFids = new List<int>();
                List<int> accmainFids = new List<int>();
                var parameters = new DynamicParameters();
                string EmployeeName = Convert.ToString(Session["EmployeeName"]);
                int id = Convert.ToInt32(Session["EmployeeId"]);
                //string query = "SELECT T2.LeadName,T1.* FROM ACCCONTACT T1 JOIN ACCMAIN T2 ON T1.ACCMAIN_Fid = T2.Fid WHERE T1.Deleted = 0 and t2.Stage in ('Identification', 'Qualification')";
                string query = "SELECT T2.LeadName,t4.CompanyName,T1.* FROM ACCCONTACT T1 JOIN ACCMAIN T2 ON T1.ACCMAIN_Fid = T2.Fid Left Join Mas_CompanyProfile t4 ON t4.CompanyId=t2.CompanyName WHERE T1.Deleted = 0 and t2.Stage in ('Identification', 'Qualification')";

                parameters.Add("@query", $"SELECT * FROM ACCOWNERS WHERE Deleted = 0 AND (LeadOwner_Fid = '{id}' OR UserId = '{id}')");
                var ACCOWNERS = DapperORM.ReturnList<ACCOWNERS>("Sp_QueryExcution", parameters).ToList();


                if (Contact != null)
                {
                    query += " and T2.LeadName = @Contact";
                    parameters.Add("Contact", Contact);
                }


                //if (Session["MainRole"].ToString() != "Admin" && Session["MainRole"].ToString() != "SalesAdmin")
                //{
                //    query += " and t1.UserId = @UserId";
                //    parameters.Add("UserId", id);
                //}

               // DapperORM.SetConnection();
                {
                    var Lead_Contact_ListList = sqlcon.Query<ACCCONTACT>(query, parameters).ToList();
                    ViewBag.Lead_Contact_ListList = Lead_Contact_ListList;
                    param.Add("@query", "SELECT * FROM ACCMAIN where Deleted = 0 ");
                    var LeadContactList = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).ToList();
                    ViewBag.LeadContactList = LeadContactList;
                }
                ViewBag.Module = Session["Module"];
                ViewBag.EmployeeName = EmployeeName;
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Lead_Contact_List", "Lead_Contact_List", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Lead_Contact_List", "Lead_Contact_List", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }
        
        public ActionResult Lead_Transfer_Report(DateTime? FromDate, DateTime? ToDate)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                List<int> transferByFids = new List<int>();
                List<int> accmainFids = new List<int>();
                var parameters = new DynamicParameters();
                var EmployeeId = Convert.ToInt32(Session["EmployeeId"]);
                param.Add("@EmpID", EmployeeId);
              //  param.Add("@p_MainRole", Session["MainRole"].ToString());
                param.Add("@FromDate", FromDate);
                param.Add("@ToDate", ToDate);
                var data = DapperORM.ReturnList<dynamic>("Sp_LeadTransferReport", param).ToList();
                ViewBag.data = data;
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Lead_Transfer_Report", "Lead_Transfer_Report", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Lead_Transfer_Report", "Lead_Transfer_Report", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }
        
        public ActionResult Lead_Master_Report(DateTime? FromDate, DateTime? ToDate)
        {
            try
            {
                //DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                List<int> transferByFids = new List<int>();
                List<int> accmainFids = new List<int>();
                var parameters = new DynamicParameters();
                var EmployeeId = Convert.ToInt32(Session["EmployeeId"]);
                param.Add("@EmpID", EmployeeId);
               // param.Add("@p_MainRole", Session["MainRole"].ToString());
                param.Add("@FromDate", FromDate);
                param.Add("@ToDate", ToDate);
                var data = DapperORM.ReturnList<dynamic>("Sp_LeadMasterREPT", param).ToList();
                ViewBag.data = data;
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Lead_Master_Report", "Lead_Master_Report", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Lead_Master_Report", "Lead_Master_Report", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

      
        
        #region Opportunity Reports

        public ActionResult Opportunity_by_source(DateTime? FromDate, DateTime? ToDate, int? LeadSource)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                int leadOwnerCount = 0;
                int isCoOwnerCount = 0;
                int id = Convert.ToInt32(Session["EmployeeId"]);
                List<int> transferByFids = new List<int>();
                List<int> accmainFids = new List<int>();
                string EmployeeName = Convert.ToString(Session["EmployeeName"]);
                //string baseQuery = $"SELECT Distinct t3.LeadSource,E1.EmployeeName, t1.*FROM ACCMAIN t1 JOIN Tool_UserLogin t2 ON t1.UserId = t2.EmployeeId JOIN Mas_employee E1 ON E1.EmployeeId = t2.EmployeeId JOIN MAS_LEADSOURCE t3 ON t1.MAS_LEADSOURCE_Fid = t3.Fid LEFT JOIN MAS_STAGES stage ON t1.MAS_STAGES_FID = stage.Fid WHERE t1.Deleted = 0 AND t1.Stage IN ('Proposal', 'Under Review')";
                string baseQuery = $"SELECT DISTINCT t3.LeadSource,E1.EmployeeName,t4.CompanyName AS CompanyName, t1.Fid,t1.Encrypted_Id,t1.Fdate,t1.Mid, t1.Mip,t1.BUCode,t1.UserId,t1.LeadPriority,t1.LeadName,t1.CompanyName AS CompanyId,t1.CompanyCategory, t1.OrganisationType,t1.LeadType,t1.MAS_LEADSOURCE_Fid,t1.MAS_INDUSTRYTYPE_Fid,t1.CompanyEmail,t1.CountryCode,t1.CompanyContactNumber,t1.CINNo,t1.Summary,t1.CompanyRating,t1.MAS_STAGES_FID,t1.Stage,t1.AreaInSqureFt, t1.Discription,t1.LegalEntityName,t1.ContractStartDate,t1.ContractEndDate,t1.ECVCurrency,t1.ExpectedContractValue, t1.RateRevision, t1.DurationInMonths, t1.PaymentTermsInDays, t1.PenaltyApplicable, t1.NeedsPatrolling,t1.PatrollingNotes,t1.TranferBy_Fid, t1.TransferDate, t1.IsDirectAccount,t1.IsCoOwner, t1.Deleted,t1.Latitude,t1.Longitude,t1.ReferenceName,t1.Website,t1.CompanyLogo,t1.TenderApplicable, t1.TenderNumber,t1.Tendersource FROM ACCMAIN t1 JOIN Tool_UserLogin t2 ON t1.UserId = t2.EmployeeId JOIN Mas_employee E1 ON E1.EmployeeId = t2.EmployeeId JOIN MAS_LEADSOURCE t3 ON t1.MAS_LEADSOURCE_Fid = t3.Fid LEFT JOIN MAS_STAGES stage ON t1.MAS_STAGES_FID = stage.Fid LEFT JOIN Mas_CompanyProfile t4 ON t1.CompanyName = t4.CompanyId WHERE t1.Deleted = 0 AND t1.Stage IN ('Proposal', 'Under Review')";

                var parameters = new DynamicParameters();

                parameters.Add("@query", $"SELECT * FROM ACCOWNERS WHERE Deleted = 0 AND (LeadOwner_Fid = '{id}' OR UserId = '{id}')");
                var ACCOWNERS = DapperORM.ReturnList<ACCOWNERS>("Sp_QueryExcution", parameters).ToList();


                //if (Session["MainRole"].ToString() != "Admin" && Session["MainRole"].ToString() != "SalesAdmin")
                //{
                //    baseQuery += " AND t1.UserId = @UserId";
                //    parameters.Add("UserId", id);
                //}
                if (LeadSource != null)
                {
                    baseQuery += " AND t1.MAS_LEADSOURCE_Fid = @LeadSource";
                    parameters.Add("LeadSource", LeadSource);
                }

                if (FromDate != null && ToDate != null)
                {
                    baseQuery += " AND t1.Fdate BETWEEN @FromDate AND @ToDate";
                    parameters.Add("FromDate", FromDate);
                    parameters.Add("ToDate", ToDate.Value.AddDays(1));
                }
                else if (FromDate != null)
                {
                    ToDate = DateTime.Now.Date;
                    baseQuery += " AND t1.Fdate >= @FromDate";
                    parameters.Add("FromDate", FromDate);
                }

                parameters.Add("FromDate", FromDate);

                //DapperORM.SetConnection();
                {
                    var leadBySourceList = sqlcon.Query<ACCMAIN>(baseQuery, parameters).ToList();
                    ViewBag.Lead_by_sourceList = leadBySourceList;

                    var leadSourceQuery = "SELECT * FROM MAS_LEADSOURCE WHERE Deleted = 0";
                    var Leadsource1 = sqlcon.Query<MAS_LEADSOURCE>(leadSourceQuery).ToList();
                    ViewBag.Leadsource1 = Leadsource1;
                }

                ViewBag.Module = Convert.ToString(Session["Module"]);
                ViewBag.EmployeeName = EmployeeName;
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Opportunity_by_source", "Opportunity_by_source", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Opportunity_by_source", "Opportunity_by_source", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                // Log the exception if needed
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult Opportunity_by_Stage(int? LeadStages, DateTime? FromDate, DateTime? ToDate)
        {
            try
            {
                //  DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                int leadOwnerCount = 0;
                int isCoOwnerCount = 0;
                List<int> transferByFids = new List<int>();
                List<int> accmainFids = new List<int>();
                var parameters = new DynamicParameters();
                string EmployeeName = Convert.ToString(Session["EmployeeName"]);
                int id = Convert.ToInt32(Session["EmployeeId"]);
                // string query = @"SELECT t3.LeadSource,stage.Stage,t1.LeadName,t1.CompanyName, t1.Fdate,E1.EmployeeName FROM ACCMAIN t1 JOIN Tool_UserLogin t2 ON t1.UserId = t2.EmployeeId JOIN MAS_EMPLOYEE E1 ON t2.EmployeeId = E1.EmployeeId JOIN MAS_LEADSOURCE t3 ON t1.MAS_LEADSOURCE_Fid = t3.Fid LEFT JOIN MAS_STAGES stage ON t1.MAS_STAGES_FID = stage.Fid WHERE t1.Deleted = 0 AND t1.Stage IN ('Proposal', 'Under Review','Lost','Negotiations')";
                string query = @"SELECT t3.LeadSource,stage.Stage,t1.LeadName,t1.CompanyName As CompanyId,t4.CompanyName, t1.Fdate,E1.EmployeeName FROM ACCMAIN t1 JOIN Tool_UserLogin t2 ON t1.UserId = t2.EmployeeId JOIN MAS_EMPLOYEE E1 ON t2.EmployeeId = E1.EmployeeId JOIN MAS_LEADSOURCE t3 ON t1.MAS_LEADSOURCE_Fid = t3.Fid LEFT JOIN MAS_STAGES stage ON t1.MAS_STAGES_FID = stage.Fid Left Join Mas_CompanyProfile t4 ON t4.CompanyId=t1.CompanyName WHERE t1.Deleted = 0 AND t1.Stage IN ('Proposal', 'Under Review','Lost','Negotiations') ";

                parameters.Add("@query", $"SELECT * FROM ACCOWNERS WHERE Deleted = 0 AND (LeadOwner_Fid = '{id}' OR UserId = '{id}')");
                var ACCOWNERS = DapperORM.ReturnList<ACCOWNERS>("Sp_QueryExcution", parameters).ToList();

                if (LeadStages != null)
                {
                    query += " and t1.MAS_STAGES_FID = @LeadStages";
                    parameters.Add("LeadStages", LeadStages);
                }


                if (FromDate != null && ToDate != null)
                {
                    query += " and t1.Fdate between @FromDate and @ToDate";
                    parameters.Add("FromDate", FromDate);
                    parameters.Add("ToDate", ToDate.Value.AddDays(1));
                }
                else if (FromDate != null)
                {
                    ToDate = DateTime.Now.Date;
                    query += " and t1.Fdate >= @FromDate";
                    parameters.Add("FromDate", FromDate);
                }



                //if (Session["MainRole"].ToString() != "Admin" && Session["MainRole"].ToString() != "SalesAdmin")
                //{
                //    query += " and t1.UserId = @UserId";
                //    parameters.Add("UserId", id);
                //}

             //   DapperORM.SetConnection();
                {
                    var Lead_by_StagesList = sqlcon.Query<ACCMAIN>(query, parameters).ToList();
                    ViewBag.Lead_by_StagesList = Lead_by_StagesList;
                    param.Add("@query", "SELECT * FROM MAS_STAGES where Deleted = 0 and origin = 'Prospect' AND stage <> 'Won'");
                    var Leadstages = DapperORM.ReturnList<MAS_STAGES>("Sp_QueryExcution", param).ToList();
                    ViewBag.Leadstages = Leadstages;
                }
                {

                    query += " GROUP BY t3.LeadSource,stage.Stage, t1.LeadName, t1.CompanyName,t4.CompanyName, t1.Fdate, E1.EmployeeName";

                    var Lead_by_StagesList = sqlcon.Query<ACCMAIN>(query, parameters).ToList();
                    ViewBag.Lead_by_StagesList = Lead_by_StagesList;

                    param.Add("@query", "SELECT * FROM MAS_STAGES where Deleted = 0 and origin = 'Prospect' AND stage <> 'Won'");
                    var Leadstages = DapperORM.ReturnList<MAS_STAGES>("Sp_QueryExcution", param).ToList();

                    ViewBag.Leadstages = Leadstages;
                }
                ViewBag.Module = Session["Module"];
                ViewBag.EmployeeName = EmployeeName;
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Opportunity_by_Stage", "Opportunity_by_Stage", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Opportunity_by_Stage", "Opportunity_by_Stage", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult Opportunity_by_Industry(DateTime? FromDate, DateTime? ToDate, int? IndustryType)
        {
            try
            {
                //  DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                int leadOwnerCount = 0;
                int isCoOwnerCount = 0;
                List<int> transferByFids = new List<int>();
                List<int> accmainFids = new List<int>();
                var parameters = new DynamicParameters();
                string EmployeeName = Convert.ToString(Session["EmployeeName"]);

                int id = Convert.ToInt32(Session["EmployeeId"]);
                //string query = @"SELECT Distinct t3.IndustryType,stage.Stage,t1.LeadName,t1.CompanyName, t1.Fdate,E1.EmployeeName FROM ACCMAIN t1 JOIN Tool_UserLogin t2 ON t1.UserId = t2.EmployeeId JOIN MAS_EMPLOYEE E1 ON t2.EmployeeId = E1.EmployeeId JOIN MAS_INDUSTRYTYPE t3 ON t1.MAS_INDUSTRYTYPE_Fid = t3.Fid LEFT JOIN MAS_STAGES stage ON t1.MAS_STAGES_FID = stage.Fid WHERE t1.Deleted = 0 AND t1.Stage IN ('Proposal', 'Under Review')";
                string query = @"SELECT Distinct t3.IndustryType,stage.Stage,t1.LeadName,t1.CompanyName As CompanyId,t4.CompanyName, t1.Fdate,E1.EmployeeName FROM ACCMAIN t1 JOIN Tool_UserLogin t2 ON t1.UserId = t2.EmployeeId JOIN MAS_EMPLOYEE E1 ON t2.EmployeeId = E1.EmployeeId JOIN MAS_INDUSTRYTYPE t3 ON t1.MAS_INDUSTRYTYPE_Fid = t3.Fid LEFT JOIN MAS_STAGES stage ON t1.MAS_STAGES_FID = stage.Fid Left Join Mas_CompanyProfile t4 ON t4.CompanyId=t1.CompanyName  WHERE t1.Deleted = 0 AND t1.Stage IN ('Proposal', 'Under Review')";
            
                parameters.Add("@query", $"SELECT * FROM ACCOWNERS WHERE Deleted = 0 AND (LeadOwner_Fid = '{id}' OR UserId = '{id}')");
                var ACCOWNERS = DapperORM.ReturnList<ACCOWNERS>("Sp_QueryExcution", parameters).ToList();

                if (IndustryType != null)
                {
                    query += " and t1.MAS_INDUSTRYTYPE_Fid = @IndustryType";
                    parameters.Add("IndustryType", IndustryType);
                }


                if (FromDate != null && ToDate != null)
                {
                    query += " and t1.Fdate between @FromDate and @ToDate";
                    parameters.Add("FromDate", FromDate);
                    parameters.Add("ToDate", ToDate.Value.AddDays(1));
                }
                else if (FromDate != null)
                {
                    ToDate = DateTime.Now.Date;
                    query += " and t1.Fdate >= @FromDate";
                    parameters.Add("FromDate", FromDate);
                }



                //if (Session["MainRole"].ToString() != "Admin" && Session["MainRole"].ToString() != "SalesAdmin")
                //{
                //    query += " and t1.UserId = @UserId";
                //    parameters.Add("UserId", id);
                //}

                //DapperORM.SetConnection();
                //{
                //    var Lead_by_IndustryTypeList = sqlcon.Query<ACCMAIN>(query, parameters).ToList();
                //    ViewBag.Lead_by_IndustryTypeList = Lead_by_IndustryTypeList;
                //    param.Add("@query", "SELECT * FROM MAS_INDUSTRYTYPE where Deleted = 0");
                //    var LeadIndusrty = DapperORM.ReturnList<MAS_INDUSTRYTYPE>("Sp_QueryExcution", param).ToList();
                //    ViewBag.LeadIndusrty = LeadIndusrty;
                //}

                {

                    query += " GROUP BY t3.IndustryType,stage.Stage, t1.LeadName, t1.CompanyName,t4.CompanyName, t1.Fdate, E1.EmployeeName";

                    var Lead_by_IndustryTypeList = sqlcon.Query<ACCMAIN>(query, parameters).ToList();
                    ViewBag.Lead_by_IndustryTypeList = Lead_by_IndustryTypeList;

                    param.Add("@query", "SELECT * FROM MAS_INDUSTRYTYPE where Deleted = 0");
                    var LeadIndustry = DapperORM.ReturnList<MAS_INDUSTRYTYPE>("Sp_QueryExcution", param).ToList();

                    ViewBag.LeadIndustry = LeadIndustry;
                }
                ViewBag.Module = Session["Module"];
                ViewBag.EmployeeName = EmployeeName;
               LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Opportunity_by_Industry", "Opportunity_by_Industry", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Opportunity_by_Industry", "Opportunity_by_Industry", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult Lost_Opportunity(DateTime? FromDate, DateTime? ToDate)
        {
            try
            {
                //DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                
                int leadOwnerCount = 0;
                int isCoOwnerCount = 0;
                List<int> transferByFids = new List<int>();
                List<int> accmainFids = new List<int>();
                var parameters = new DynamicParameters();
                string EmployeeName = Convert.ToString(Session["EmployeeName"]);
                int id = Convert.ToInt32(Session["EmployeeId"]);
                //string query = $"SELECT Distinct t3.LeadSource,stage.Stage,t1.LeadName,t1.CompanyName, t1.Fdate,E1.EmployeeName FROM ACCMAIN t1 JOIN Tool_UserLogin t2 ON t1.UserId = t2.EmployeeId JOIN MAS_EMPLOYEE E1 ON t2.EmployeeId = E1.EmployeeId JOIN MAS_LEADSOURCE t3 ON t1.MAS_INDUSTRYTYPE_Fid = t3.Fid LEFT JOIN MAS_STAGES stage ON t1.MAS_STAGES_FID = stage.Fid WHERE t1.Deleted = 0 AND stage.Origin = 'Prospect' and stage.Stage = 'Lost'";
                string query = $"SELECT Distinct t3.LeadSource,stage.Stage,t1.LeadName,t1.CompanyName As companyId,t4.CompanyName, t1.Fdate,E1.EmployeeName FROM ACCMAIN t1 JOIN Tool_UserLogin t2 ON t1.UserId = t2.EmployeeId JOIN MAS_EMPLOYEE E1 ON t2.EmployeeId = E1.EmployeeId JOIN MAS_LEADSOURCE t3 ON t1.MAS_INDUSTRYTYPE_Fid = t3.Fid LEFT JOIN MAS_STAGES stage ON t1.MAS_STAGES_FID = stage.Fid Left Join Mas_CompanyProfile t4 ON t4.CompanyId=t1.CompanyName WHERE t1.Deleted = 0 AND stage.Origin = 'Prospect' and stage.Stage = 'Lost'";

                parameters.Add("@query", $"SELECT * FROM ACCOWNERS WHERE Deleted = 0 AND (LeadOwner_Fid = '{id}' OR UserId = '{id}')");
                var ACCOWNERS = DapperORM.ReturnList<ACCOWNERS>("Sp_QueryExcution", parameters).ToList();

                if (FromDate != null && ToDate != null)
                {
                    query += " and t1.Fdate between @FromDate and @ToDate";
                    parameters.Add("FromDate", FromDate);
                    parameters.Add("ToDate", ToDate.Value.AddDays(1));
                }
                else if (FromDate != null)
                {
                    ToDate = DateTime.Now.Date;
                    query += " and t1.Fdate >= @FromDate";
                    parameters.Add("FromDate", FromDate);
                }



                //if (Session["MainRole"].ToString() != "Admin" && Session["MainRole"].ToString() != "SalesAdmin")
                //{
                //    query += " and t1.UserId = @UserId";
                //    parameters.Add("UserId", id);
                //}


               // DapperORM.SetConnection();
                //{
                //    var Lead_by_StagesList = sqlcon.Query<ACCMAIN>(query, parameters).ToList();
                //    ViewBag.Lead_by_StagesList = Lead_by_StagesList;

                //}
                {

                    query += " GROUP BY t3.LeadSource,stage.Stage, t1.LeadName, t1.CompanyName, t4.CompanyName,t1.Fdate, E1.EmployeeName";

                    var Lead_by_StagesList = sqlcon.Query<ACCMAIN>(query, parameters).ToList();
                    ViewBag.Lead_by_StagesList = Lead_by_StagesList;

                
                }
                ViewBag.Module = Session["Module"];
                ViewBag.EmployeeName = EmployeeName;
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Lost_Opportunity", "Lost_Opportunity", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();

            }
            catch (Exception ex)
            {
               LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Lost_Opportunity", "Lost_Opportunity", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult Opportunity_Contact_List(string Contact)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                int leadOwnerCount = 0;
                int isCoOwnerCount = 0;
                List<int> transferByFids = new List<int>();
                List<int> accmainFids = new List<int>();
                var parameters = new DynamicParameters();
                string EmployeeName = Convert.ToString(Session["EmployeeName"]);
                int id = Convert.ToInt32(Session["EmployeeId"]);
                //string query = "SELECT T2.LeadName,T1.* FROM ACCCONTACT T1 JOIN ACCMAIN T2 ON T1.ACCMAIN_Fid = T2.Fid WHERE T1.Deleted = 0 and t2.Stage in ('Proposal', 'Under Review','Lost','Negotiations')";
                string query = "SELECT T2.LeadName,T1.* FROM ACCCONTACT T1 JOIN ACCMAIN T2 ON T1.ACCMAIN_Fid = T2.Fid WHERE T1.Deleted = 0 and t2.Stage in ('Proposal', 'Under Review','Lost','Negotiations')";

                parameters.Add("@query", $"SELECT * FROM ACCOWNERS WHERE Deleted = 0 AND (LeadOwner_Fid = '{id}' OR UserId = '{id}')");
                var ACCOWNERS = DapperORM.ReturnList<ACCOWNERS>("Sp_QueryExcution", parameters).ToList();


                if (Contact != null)
                {
                    query += " and T2.LeadName = @Contact";
                    parameters.Add("Contact", Contact);
                }

                //if (Session["MainRole"].ToString() != "Admin" && Session["MainRole"].ToString() != "SalesAdmin")
                //{
                //    query += " and t1.UserId = @UserId";
                //    parameters.Add("UserId", id);
                //}


               // DapperORM.SetConnection();
                {
                    var Lead_Contact_ListList = sqlcon.Query<ACCCONTACT>(query, parameters).ToList();
                    ViewBag.Lead_Contact_ListList = Lead_Contact_ListList;
                    param.Add("@query", "SELECT * FROM ACCMAIN where Deleted = 0 ");
                    var LeadContactList = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).ToList();
                    ViewBag.LeadContactList = LeadContactList;
                }
                ViewBag.Module = Session["Module"];
                ViewBag.EmployeeName = EmployeeName;
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Opportunity_Contact_List", "Opportunity_Contact_List", "View", "Passed", "", _clientIp, _clientMachineName);

                return View();
            }
            catch (Exception ex)
            {
                LoginController.saveAuditEntry(DateTime.Now, Convert.ToInt32(Session["EmployeeId"]), "List", "Opportunity_Contact_List", "Opportunity_Contact_List", "View", "Failed", ex.Message, _clientIp, _clientMachineName);

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }


          #endregion

      
      #region Location

        public ActionResult SiteLocList()
        {
            try
            {
               // DapperORM.SetConnection();
                param.Add("@query", "select t2.SiteName, t1.* from ACCSITELOCT t1 join ACCSITES t2 on t1.ACCSITES_Fid=t2.Fid where t1.Active=1");
                var SiteLocList = DapperORM.ReturnList<ACCSITELOCT>("Sp_QueryExcution", param).ToList();
                ViewBag.SiteLocList = SiteLocList;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });

            }
        }

        public ActionResult SiteLocListCreationForm(string Encrypted_Id, int Id)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                //DapperORM.SetConnection();
                {
                    //param.Add("@query", $"Select * from ACCSITELOCT where Active=1 and Encrypted_Id = '{Encrypted_Id}'");
                    //var MAS_EMPRSGNList = DapperORM.ReturnList<ACCSITELOCT>("Sp_QueryExcution", param).FirstOrDefault();
                    param.Add("@query", $"Select * from ACCSITES where Deleted = 0 and ACCMAIN_Fid = '{Id}'");
                    var SiteList = DapperORM.ReturnList<ACCSITES>("Sp_QueryExcution", param).ToList();
                    ViewBag.SiteList = SiteList;

                    param.Add("@query", $"select loc.Encrypted_Id,loc.ACCSITES_Fid, acc.SiteName,loc.Address,loc.LocationName,loc.GPSLattitude,loc.GPSLongitude from ACCSITES acc join ACCSITELOCT loc  on   acc.fid =loc.ACCSITES_Fid where acc.ACCMAIN_Fid='{Id}'and loc.Active=1and acc.SiteStatus='Active'");
                    var SitedetailsList = DapperORM.ReturnList<ACCSITELOCT>("Sp_QueryExcution", param).ToList();
                    ViewBag.SitedetailsList = SitedetailsList;

                    Session["EnIdSL"] = Encrypted_Id;
                    //ViewBag.EncryptedId = Session["EnIdSL"];
                    return View();
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }

        }

        [HttpPost]
        public ActionResult SiteLocListCreationForm(ACCSITELOCT ACCSITELOCT)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param1 = new DynamicParameters();

                param1.Add("@query", $"SELECT BUCode FROM ACCSITES WHERE Deleted = 0 and Fid = '{ACCSITELOCT.ACCSITES_Fid}'");
                var BUCode = DapperORM.ReturnList<ACCSITES>("Sp_QueryExcution", param1).FirstOrDefault();


                string process = string.IsNullOrEmpty(ACCSITELOCT.Encrypted_Id) ? "Save" : "Update";
                param.Add("@p_process", process);
                param.Add("@P_Encrypted_Id", ACCSITELOCT.Encrypted_Id);
                param.Add("@p_ACCSITES_Fid", ACCSITELOCT.ACCSITES_Fid);
                param.Add("@p_LocationName", ACCSITELOCT.LocationName);
                param.Add("@p_Address", ACCSITELOCT.Address);
                param.Add("@p_GPSLattitude", ACCSITELOCT.GPSLattitude);
                param.Add("@p_BUCode", BUCode.BUCode);
                param.Add("@p_GPSLongitude", ACCSITELOCT.GPSLongitude);
                param.Add("@p_IsAttendanceLocation", ACCSITELOCT.IsAttendanceLocation);
                //param.Add("@p_UserId", Session["EmployeeId"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_Mip", Dns.GetHostByName(MachineId).AddressList[0].ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var data = DapperORM.ExecuteReturn("Sp_SUD_ACCSITELOCT", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                var EncryptedId = Session["EnIdSL"];
                return RedirectToAction("AccountDetails", "CRM", new { EncryptedId = EncryptedId, area = "CRMS" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        [HttpGet]
        public JsonResult IsSiteLocExist(string Encrypted_Id, string LocationName, int ACCSITES_Fid)
        {
            try
            {
              //  DapperORM.SetConnection();
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_Encrypted_Id", Encrypted_Id);
                    param.Add("@p_LocationName", LocationName);
                    param.Add("@p_ACCSITES_Fid", ACCSITES_Fid);
                    param.Add("@p_msg", dbType: System.Data.DbType.String, direction: System.Data.ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: System.Data.DbType.String, direction: System.Data.ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("Sp_SUD_ACCSITELOCT", param);
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

      #region Site Close

        public ActionResult SiteClose()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // DapperORM.SetConnection();
                {
                    param.Add("@query", $"Select * from ACCSITES Where SiteStatus = 'Active' AND Deleted=0 ");
                    var Sites = DapperORM.ReturnList<ACCSITES>("Sp_QueryExcution", param).ToList();
                    ViewBag.Sites = Sites;
                    return View();
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }

        }

        public ActionResult SiteCloseCreation(string Encrypted_Id, int Id)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                if (Encrypted_Id == null)
                {
                    Encrypted_Id = "";
                }
                var UserId = Session["EmployeeId"];
              //  DapperORM.SetConnection();
                {

                    param.Add("@query", $"Select * from ACCSITES Where ACCMAIN_Fid = '{Id}' AND Deleted=0 and SiteStatus = 'Active'");
                    var Sites = DapperORM.ReturnList<ACCSITES>("Sp_QueryExcution", param).ToList();
                    ViewBag.Sites = Sites;
                    Session["EnIdSC"] = Encrypted_Id;
                    ViewBag.EncryptedId = Session["EnIdSC"];
                    return View();
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }

        }

        [HttpPost]
        public ActionResult SiteCloseCreation(ACCSITES ACCSITES)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                param.Add("@p_process", "LocationClose");
                param.Add("@p_Id", ACCSITES.Fid);
                param.Add("@p_LocationCloseDate", ACCSITES.SiteCloseDate);
                param.Add("@p_LocationCloseReason", ACCSITES.SiteCloseReason);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_Mip", Dns.GetHostAddresses(Dns.GetHostName())[0].ToString());
                param.Add("@p_UserId", Session["EmployeeId"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var data = DapperORM.ExecuteReturn("sp_SUD_ACCSITELOCT", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                var EncryptedId = Session["EnIdSC"];
                return RedirectToAction("AccountDetails", "CRM", new { EncryptedId = EncryptedId, area = "CRMS" });

            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }


        public ActionResult GetLocation(int Fid)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                param.Add("@query", $"Select * from ACCSITELOCT where ACCSITES_Fid = {Fid} and Active = 1");
                var Location = DapperORM.ReturnList<ACCSITELOCT>("Sp_QueryExcution", param).ToList();
                var obj = new
                {
                    Location = Location
                };
                return Json(obj, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }


       #endregion

     #region Invoice

        public ActionResult InvoiceList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

               // var MainRole = Session["MainRole"].ToString();
                var userId = Convert.ToInt32(Session["EmployeeId"]);
                
                //DapperORM.SetConnection();
                {
                    param.Add("@query", $"select acc.SiteName,voc.* from ACCINVOICE voc join ACCSITES acc on voc.ACCSITES_Fid=acc.Fid where voc.Deleted = 0 and voc.ACCSITES_Fid in(select distinct ACCSITE_Fid from MAS_OPSSITES where (MAS_EMployee_Fid = '{userId}'))");
                    var InvoiceList = DapperORM.ReturnList<ACCINVOICE>("Sp_QueryExcution", param).ToList();
                    ViewBag.InvoiceList = InvoiceList;
                }
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult AddNewInvoice(string id, DateTime? date)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

               // var MainRole = Session["MainRole"].ToString();
                var userId = Convert.ToInt32(Session["EmployeeId"]);


                param.Add("@query", $"Select * from ACCINVOICE where Encrypted_Id = '{id}'");
                var Compl = DapperORM.ReturnList<ACCINVOICE>("Sp_QueryExcution", param).FirstOrDefault();

                //param.Add("@query", $"select * from ACCSITES where Deleted = 0 and SiteStatus ='Active'");
                param.Add("@query", $"select distinct acc.Fid,acc.SiteName from MAS_OPSSITES ops join ACCSITES acc on ops.ACCSITE_Fid=acc.fid where  ops.Active=1  and (ops.MAS_EMployee_Fid = '{userId}')");
                var SiteList = DapperORM.ReturnList<ACCSITES>("Sp_QueryExcution", param).ToList();
                ViewBag.SiteList = SiteList;

                if (Compl == null)
                {
                    var newCompl = new ACCINVOICE();
                    newCompl.BillMonth = date;
                    newCompl.InvoiceDate = date;
                    newCompl.DueDate = date;
                    return View(newCompl);
                }
                else
                {
                    if (Compl.InvoiceCopy != null)
                    {
                        if (System.IO.File.Exists(Compl.InvoiceCopy))
                        {
                            ViewBag.Invoice = Compl.InvoiceCopy;
                        }
                    }

                    if (Compl.SupportingDocument != null)
                    {
                        if (System.IO.File.Exists(Compl.SupportingDocument))
                        {
                            ViewBag.SupDoc = Compl.SupportingDocument;
                        }
                    }
                    Compl.InvoiceBalanceAmountstring = Compl.InvoiceBalanceAmount.ToString();
                    Compl.InvoiceBasicAmountstring = Compl.InvoiceBasicAmount.ToString();
                    Compl.InvoicePaidAmountstring = Compl.InvoicePaidAmount.ToString();
                    Compl.InvoiceTaxAmountstring = Compl.InvoiceTaxAmount.ToString();
                    Compl.Mid = Compl.ACCSITES_Fid.ToString();
                }
                return View(Compl);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }


        [HttpGet]
        public bool IsInvoiceExist(string Encrypted_Id, DateTime? BillMonth, int ACCSITES_Fid)
        {
            try
            {
              //  DapperORM.SetConnection();
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_BillMonth", BillMonth);
                    param.Add("@p_ACCSITES_Fid", ACCSITES_Fid);
                    param.Add("@p_Encrypted_Id", Encrypted_Id);
                    param.Add("@p_msg", dbType: System.Data.DbType.String, direction: System.Data.ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: System.Data.DbType.String, direction: System.Data.ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("Sp_SUD_ACCINVOICE", param);
                    var Message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    if (Message != "")
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (Exception Ex)
            {
                return false;
            }
        }

        [HttpPost]
        public ActionResult AddNewInvoice(ACCINVOICE ACCINVOICE)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                var obj = IsInvoiceExist(ACCINVOICE.Encrypted_Id, ACCINVOICE.BillMonth, ACCINVOICE.ACCSITES_Fid);
                if (!obj)
                {
                    TempData["Message"] = "Invoice Already Exist";
                    TempData["Icon"] = "error";
                    return RedirectToAction("InvoiceList", "CRM", new { area = "CRMS" });
                }

                DynamicParameters param1 = new DynamicParameters();
                DynamicParameters param2 = new DynamicParameters();

                ACCINVOICE.ACCSITES_Fid = ACCINVOICE.ACCSITES_Fid == 0 ? Convert.ToInt32(ACCINVOICE.Mid) : ACCINVOICE.ACCSITES_Fid;
                param2.Add("@query", $"select * from ACCSITES where Deleted = 0 and SiteStatus = 'Active' and Fid='{ACCINVOICE.ACCSITES_Fid}'");
                var BUCode1 = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param2).FirstOrDefault();

                var BUCode = BUCode1.BUCode;
                
                var GetDocPath = DapperORM.DynamicQuerySingle("SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin='SalesForce'");
                var basePath = GetDocPath.DocInitialPath;
                string invoiceFolder = Path.Combine(basePath, "CRM", "Invoice", Session["EmployeeId"].ToString());

                if (!Directory.Exists(invoiceFolder))
                {
                    Directory.CreateDirectory(invoiceFolder);
                }

                // ======================= SAVE Invoice Copy ===========================
                if (ACCINVOICE.InvoiceCopyFile != null)
                {
                    string fileName = $"{ACCINVOICE.InvoiceNumber}{Path.GetExtension(ACCINVOICE.InvoiceCopyFile.FileName)}";
                    string savePath = Path.Combine(invoiceFolder, fileName);

                    ACCINVOICE.InvoiceCopyFile.SaveAs(savePath);
                    ACCINVOICE.InvoiceCopy = savePath.Replace("\\\\", "\\");
                }

                // ======================= SAVE Supporting Document ====================
                if (ACCINVOICE.SupportingDocumentFile != null)
                {
                    string fileName2 = $"SupportingDoc_{ACCINVOICE.InvoiceNumber}{Path.GetExtension(ACCINVOICE.SupportingDocumentFile.FileName)}";
                    string savePath2 = Path.Combine(invoiceFolder, fileName2);

                    ACCINVOICE.SupportingDocumentFile.SaveAs(savePath2);
                    ACCINVOICE.SupportingDocument = savePath2.Replace("\\\\", "\\");
                }

                string process = string.IsNullOrEmpty(ACCINVOICE.Encrypted_Id) ? "Save" : "Update";

                param.Add("@p_process", process);
                param.Add("@P_Fid", ACCINVOICE.Fid);
                param.Add("@P_Encrypted_Id", ACCINVOICE.Encrypted_Id);
                param.Add("@p_ACCSITES_Fid", ACCINVOICE.ACCSITES_Fid);
                param.Add("@p_BUCode", BUCode);
                param.Add("@p_BillMonth", ACCINVOICE.BillMonth);
                param.Add("@p_InvoiceDate", ACCINVOICE.InvoiceDate);
                param.Add("@p_DueDate", ACCINVOICE.DueDate);
                param.Add("@p_InvoiceNumber", ACCINVOICE.InvoiceNumber);
                param.Add("@p_InvoiceBasicAmount", Convert.ToDouble(ACCINVOICE.InvoiceBasicAmountstring));
                param.Add("@p_InvoiceTaxAmount", Convert.ToDouble(ACCINVOICE.InvoiceTaxAmountstring));
                param.Add("@p_InvoicePaidAmount", Convert.ToDouble(ACCINVOICE.InvoicePaidAmountstring));
                param.Add("@p_InvoiceBalanceAmount", Convert.ToDouble(ACCINVOICE.InvoiceBalanceAmountstring));
                param.Add("@p_InvoiceCopy", ACCINVOICE.InvoiceCopy);
                param.Add("@p_SupportingDocument", ACCINVOICE.SupportingDocument);
                param.Add("@p_UserId", Convert.ToInt32(Session["EmployeeId"]));
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
               // param.Add("@p_UserId", Session["Employee_Fid"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var data = DapperORM.ExecuteReturn("Sp_SUD_ACCINVOICE", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("InvoiceList", "CRM", new { area = "CRMS" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        [HttpGet]
        public JsonResult GetInvoiceDetails(int siteId, string month)
        {
            try
            {
              //  DapperORM.SetConnection();
                DynamicParameters param = new DynamicParameters();

                param.Add("@query", $@"
                    SELECT 
                        CAST(InvoiceBasicAmount AS VARCHAR) AS InvoiceBasicAmountstring, 
                        CAST(InvoiceTaxPercentage AS VARCHAR) AS InvoiceTaxPercentagestring
                    FROM ACCINVOICE 
                    WHERE ACCSITES_Fid = {siteId} 
                        AND FORMAT(BillMonth, 'yyyy-MM') = '{month}' 
                        AND Deleted = 0");

                var result = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).FirstOrDefault();

                if (result != null)
                {
                    return Json(new
                    {
                        success = true,
                        basicAmount = result.InvoiceBasicAmountstring,
                        taxPercentage = result.InvoiceTaxPercentagestring
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred" }, JsonRequestBehavior.AllowGet);
            }
        }

    #endregion
     

    #region Site Mapping
        
        public ActionResult SiteMapping(string Encrypted_Id, int Id)
        {
            try
            {
                //DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param1.Add("@query", "SELECT * FROM MAS_REPORLEVEL WHERE Deleted = 0");
                var ReportingLevel = DapperORM.ReturnList<MAS_REPORLEVEL>("Sp_QueryExcution", param1).ToList();
                ViewBag.ReportingLevel = ReportingLevel;

                param.Add("@query", $"Select * from ACCSITES where Deleted = 0 and ACCMAIN_Fid = '{Id}'");
                var SiteName = DapperORM.ReturnList<ACCSITES>("Sp_QueryExcution", param).ToList();
                ViewBag.SiteName = SiteName;

                param.Add("@query", $" SELECT OP.Fid,OP.ACCSITE_Fid,AC.SiteName,OP.MAS_EMployee_Fid,ME.EmployeeName, op.Level,MR.ReportingRole, op.ActiveFrom, op.NoOfSiteVisit, op.UnitOfSiteVisit, op.NoOfSiteVisit,op.UnitOfSiteVisit FROM MAS_OPSSITES OP JOIN ACCSITES AC ON OP.ACCSITE_Fid = AC.Fid JOIN Mas_Employee ME ON OP.MAS_EMployee_Fid = ME.EmployeeId JOIN MAS_REPORLEVEL MR ON OP.Level = MR.ReportingLever where ME.EmployeeLeft = 0 and ME.Deactivate = 0 and Op.Deleted = 0 and op.Active = 1  and MR.Deleted = 0 and AC.ACCMAIN_Fid = '{Id}'");
                var sitelist = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                ViewBag.sitelist = sitelist;

                param.Add("@query", $" Select EmployeeId As Fid ,EmployeeName From Mas_employee Where Deactivate=0 and EmployeeLeft=0 Order By EmployeeName");
        
                var EmployeeList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param).ToList();
                ViewBag.EmployeeList = EmployeeList;

                Session["EnIdSL"] = Encrypted_Id;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        [HttpPost]
        public ActionResult MANG_AscSiteCreation(MAS_OPSSITES MAS_OPSSITES)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }


                if (MAS_OPSSITES.Encrypted_Id == null)
                {
                    param.Add("@p_process", "Save");
                    param.Add("@P_Fid", MAS_OPSSITES.Fid);
                    param.Add("@P_Encrypted_Id", MAS_OPSSITES.Encrypted_Id);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_BUCode", MAS_OPSSITES.BUCode);
                    param.Add("@p_ACCSITE_Fid", MAS_OPSSITES.ACCSITE_Fid);
                    param.Add("@p_MAS_EMployee_Fid", MAS_OPSSITES.MAS_EMployee_Fid);
                    param.Add("@p_Level", MAS_OPSSITES.Level);
                    param.Add("@p_UnitOfSiteVisit", MAS_OPSSITES.UnitOfSiteVisit);
                    param.Add("@p_NoOfSiteVisit", MAS_OPSSITES.NoOfSiteVisit);
                    param.Add("@p_ActiveFrom", MAS_OPSSITES.ActiveFrom);
                    param.Add("@p_Active", MAS_OPSSITES.Active);
                    param.Add("@p_Mip", Dns.GetHostByName(MachineId).AddressList[0].ToString());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var data = DapperORM.ExecuteReturn("Sp_SUD_MAS_OPSSITES", param);
                    var Message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    var Fid = param.Get<string>("@p_Id");
                    return Json(new { Message = Message, Icon = Icon, Fid = Fid }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    param.Add("@p_process", "Update");
                    param.Add("@P_Fid", MAS_OPSSITES.Fid);
                    param.Add("@P_Encrypted_Id", MAS_OPSSITES.Encrypted_Id);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_BUCode", MAS_OPSSITES.BUCode);
                    param.Add("@p_ACCSITE_Fid", MAS_OPSSITES.ACCSITE_Fid);
                    param.Add("@p_MAS_EMployee_Fid", MAS_OPSSITES.MAS_EMployee_Fid);
                    param.Add("@p_Level", MAS_OPSSITES.Level);
                    param.Add("@p_UnitOfSiteVisit", MAS_OPSSITES.UnitOfSiteVisit);
                    param.Add("@p_NoOfSiteVisit", MAS_OPSSITES.NoOfSiteVisit);
                    param.Add("@p_ActiveFrom", MAS_OPSSITES.ActiveFrom);
                    param.Add("@p_Active", MAS_OPSSITES.Active);
                    param.Add("@p_Mip", Dns.GetHostByName(MachineId).AddressList[0].ToString());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var data = DapperORM.ExecuteReturn("Sp_SUD_MAS_OPSSITES", param);
                    var Message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    var Fid = MAS_OPSSITES.Fid;
                    return Json(new { Message = Message, Icon = Icon, Fid = Fid }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        [HttpPost]
        public ActionResult DeleteSiteMapping(int Fid)
        {
            try
            {
                //DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                if (Fid > 0)
                {
                    var param1 = new DynamicParameters();
                    param1.Add("@query", $"UPDATE MAS_OPSSITES SET Active = 0  WHERE Fid = {Fid}");

                    if (Fid > 0)
                    {
                        var rowsAffected = DapperORM.ExecuteReturn("Sp_QueryExcution", param1);

                        return Json(new { Message = "Record deleted successfully.", Icon = "success" });
                    }
                    else
                    {
                        return Json(new { Message = "Record not found or already deleted.", Icon = "error" });
                    }
                }
                else
                {
                    return Json(new { Message = "Invalid Site Id.", Icon = "error" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { Message = "An error occurred while deleting the record.", Icon = "error" });
            }
        }

        public ActionResult GetEmployeeTypeWise(int ACCSITES_Fid)
        {
            try
            {
                //DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param1.Add("@query", $"select EmployeeId As Fid ,EmployeeName from Mas_Employee where Deactivate =0 and EmployeeLeft =0 Order By EmployeeName ");
                //  var EmployeeList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();

                //    var obj = new
                //    {
                //        EmployeeList = EmployeeList
                //    };
                //    return Json(obj, JsonRequestBehavior.AllowGet);
                //}

                var EmployeeList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1)
              .Select(x => new
              {
                  Fid = (int)x.Fid,
                  EmployeeName = (string)x.EmployeeName
              })
              .ToList();

                return Json(new { EmployeeList = EmployeeList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

           #endregion


     
   #region Site Checklist Master

        public ActionResult CheckMaster(int? Id)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@query", $"SELECT M.*,S.SiteName,A.ActivityName,R.ReportingRole FROM MAS_CHKMEASUR M JOIN ACCSITES S ON M.ACCSITES_Fid = S.Fid and S.ACCMAIN_Fid = {Id} JOIN MAS_CHKMOPS A ON M.MAS_CHKMOPS_Fid = A.Fid  JOIN MAS_REPORLEVEL R ON M.MAS_REPORLEVEL_Fid = R.Fid WHERE M.Deleted = 0; ");
                    var Checkmaster = DapperORM.ReturnList<MAS_CHKMEASUR>("Sp_QueryExcution", param).ToList();
                    ViewBag.Checkmaster = Checkmaster;

                    ViewBag.Accmain = Id;

                    return View();
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        [HttpGet]
        public ActionResult MAS_CHKMAS(int? ids, int? Accmain)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                if (ids == null)
                {
                    ids = 0;
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@query", $"Select * from ACCSITES where Deleted = 0 and ACCMAIN_Fid = {Accmain}");
                    var SiteList = DapperORM.ReturnList<ACCSITES>("Sp_QueryExcution", param).ToList();
                    ViewBag.SiteList = SiteList;

                    param.Add("@query", "Select * from MAS_CHKMOPS where Deleted = 0");
                    var Activity = DapperORM.ReturnList<MAS_CHKMOPS>("Sp_QueryExcution", param).ToList();
                    ViewBag.Activity = Activity;

                    param.Add("@query", "SELECT * FROM MAS_REPORLEVEL WHERE Deleted = 0 AND ReportingRole NOT IN ('Associate', 'Supervisor')");
                    var Report = DapperORM.ReturnList<MAS_REPORLEVEL>("Sp_QueryExcution", param).ToList();
                    ViewBag.Report = Report;
                    ViewBag.Accmain = Accmain;
                    MAS_CHKMEASUR model = new MAS_CHKMEASUR();
                    if (ids > 0)
                    {
                        param.Add("@query", $@"
                        SELECT M.Fid, 
                               M.ACCSITES_Fid, S.SiteName, 
                               M.MAS_CHKMOPS_Fid, A.ActivityName, 
                               M.MAS_REPORLEVEL_Fid, R.ReportingRole, 
                               M.ActivityCount, M.ActivityUnit 
                        FROM MAS_CHKMEASUR M
                        LEFT JOIN ACCSITES S ON M.ACCSITES_Fid = S.Fid
                        LEFT JOIN MAS_CHKMOPS A ON M.MAS_CHKMOPS_Fid = A.Fid
                        LEFT JOIN MAS_REPORLEVEL R ON M.MAS_REPORLEVEL_Fid = R.Fid
                        WHERE M.Fid = {ids}");
                        model = DapperORM.ReturnList<MAS_CHKMEASUR>("Sp_QueryExcution", param).FirstOrDefault();

                        ViewBag.SiteName = model?.SiteName;
                        ViewBag.ActivityName = model?.ActivityName;
                        ViewBag.ReportingRole = model?.ReportingRole;
                    }

                    return View(model);

                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }
        
        [HttpPost]
        public ActionResult MAS_CHKMAS(MAS_CHKMEASUR MASCHK)
        {
            try
            {
                //  DapperORM.SetConnection();

                // Check if the session is valid
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                var param = new DynamicParameters();
                string processType = MASCHK.Fid == 0 ? "Save" : "Update"; // Determine process type
                param.Add("@p_process", processType);
                param.Add("@p_Fid", MASCHK.Fid);
                param.Add("@p_Mip", Dns.GetHostName().ToString());
                param.Add("@p_Mip", Dns.GetHostByName(MachineId).AddressList[0].ToString());
                param.Add("@p_ACCSITES_Fid", MASCHK.ACCSITES_Fid);
                param.Add("@p_MAS_CHKMOPS_Fid", MASCHK.MAS_CHKMOPS_Fid);
                param.Add("@p_MAS_REPORLEVEL_Fid", MASCHK.MAS_REPORLEVEL_Fid);
                param.Add("@p_ActivityCount", MASCHK.ActivityCount);
                param.Add("@p_ActivityUnit", MASCHK.ActivityUnit);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                var data = DapperORM.ExecuteReturn("Sp_SUD_MAS_CHKMEASUR", param);

                // Retrieve output parameters
                string message = param.Get<string>("@p_msg");
                string icon = param.Get<string>("@p_Icon");

                // Return JSON response instead of redirecting
                return Json(new { status = icon, message = message });
            }
            catch (SqlException sqlEx)
            {
                return Json(new { status = "error", message = "Database error: " + sqlEx.Message });
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "An unexpected error occurred: " + ex.Message });
            }
        }


        [HttpGet]
        public JsonResult DeleteCHKMAS(string Fid)
        {
            try
            {
                if (Fid == null)
                {
                    return Json(new { Message = "Invalid record ID!", Icon = "error" }, JsonRequestBehavior.AllowGet);
                }

              //  DapperORM.SetConnection();
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_process", "Delete");
                    param.Add("@p_Fid", Fid);
                    param.Add("@p_msg", dbType: System.Data.DbType.String, direction: System.Data.ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: System.Data.DbType.String, direction: System.Data.ParameterDirection.Output, size: 500);

                    var result = DapperORM.ExecuteReturn("Sp_SUD_MAS_CHKMEASUR", param);

                    var message = param.Get<string>("@p_msg");
                    var icon = param.Get<string>("@p_Icon");

                    return Json(new { Message = message, Icon = icon }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Message = "An error occurred while deleting the record.", Icon = "error" }, JsonRequestBehavior.AllowGet);
            }
        }

      

    #endregion

        

        public bool SendEmail(string toEmail, string subject, string emailBody, string type)
        {
            try
            {
                //DapperORM.SetConnection();
                DynamicParameters param3 = new DynamicParameters();
                param3.Add("@query", $"select * from SET_EMAILSETTINGS where Origin='email1'");
                var Emailsetting = DapperORM.ReturnList<SET_EMAILSETTINGS>("Sp_QueryExcution", param3).FirstOrDefault();
                //string senderEmail ="priyanka@probus.co.in";
                //string senderPassword ="Priyanka@123.";

                string senderEmail = Emailsetting.EmailID;
                string senderPassword = Emailsetting.Password;

                var SmtpServerName = Emailsetting.SmtpServerName;
                var portNo = Emailsetting.PortNo;
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(SmtpServerName, Convert.ToInt32(portNo));

                client.EnableSsl = true;
                client.Timeout = 100000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(senderEmail, senderPassword);
                using (var message = new MailMessage(senderEmail, toEmail))
                {
                    message.Subject = subject;
                    message.Body = emailBody;
                    message.IsBodyHtml = true;
                    client.Send(message);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

     
        public ActionResult AccountDetailsEdit(string EncryptedId, int? Fid)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }
            try
            {
              //  DapperORM.SetConnection();
                param.Add("@query", "Select * from ACCMAIN where Encrypted_Id='" + EncryptedId + "'");
                var AccMainDetails = DapperORM.ReturnList<ACCMAIN>("Sp_QueryExcution", param).FirstOrDefault();
                ViewBag.EncryptedId = EncryptedId;

                param1.Add("@query", $@"select AC.SiteName,AC.SiteType,AC.IsInventory,AC.InventoryBy,AC.Encrypted_Id, AC.SiteCode,AM.Fid,AC.Fid,AC.SiteOpenDate,AC.SiteCloseDate,AC.SiteStatus from ACCSITES AC join ACCMAIN AM on AC.ACCMAIN_Fid = AM.Fid where Ac.Deleted = 0 AND AC.ACCMAIN_Fid = '{Fid}' ");
                var ACCSites = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param1).ToList();
                ViewBag.ACCSites = ACCSites;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }
        
        public ActionResult CurrentMonthActiveLeads(string date)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
               // var mainRole = Session["MainRole"].ToString();
                var EmpID = Convert.ToInt32(Session["EmployeeId"]);

                // param.Add("@query", $"select t1.*,t2.Origin,t2.Stage from ACCMAIN t1 join MAS_STAGES t2 on t2.Fid = t1.MAS_STAGES_FID and t2.Origin ='Lead' and t2.Stage != 'Lost' where t1.deleted = 0 and (t1.UserId in (select Fid from USERS where EmployeeId  in (select Fid from fn_GetEmployeeHierarchy ({EmpID}))))and Format(t1.Fdate,'yyyy-MM') = '" + date + "'");
                param.Add("@query", $"SELECT  t1.*, t2.Origin, t2.Stage FROM ACCMAIN t1 JOIN MAS_STAGES t2 ON t2.Fid = t1.MAS_STAGES_FID AND t2.Origin = 'Lead'AND t2.Stage != 'Lost' WHERE t1.deleted = 0 AND t1.UserId IN (SELECT EmployeeId FROM Tool_UserLogin WHERE EmployeeId IN(SELECT EmployeeId FROM Mas_Employee WHERE EmployeeId = 5068)) and Format(t1.Fdate,'yyyy-MM') = '" + date + "'");
                var LeadList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param);
                ViewBag.LeadList = LeadList;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult TotalActiveLeads(string date)
        {
            try
            {
                //DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
               // var mainRole = Session["MainRole"].ToString();
                var EmpID = Convert.ToInt32(Session["EmployeeId"]);

                // param.Add("@query", $"select * from ACCMAIN t1 join MAS_STAGES t2 on t2.Fid = t1.MAS_STAGES_FID and t2.Origin ='Lead' and t2.Stage != 'Lost' where t1.deleted = 0 and('admin' = '{mainRole}' or 'SalesAdmin' = '{mainRole}' or t1.UserID in (select Fid from USERS where EmployeeId  in (select Fid from fn_GetEmployeeHierarchy ({EmpID}))))");
                param.Add("@query", $"SELECT * FROM ACCMAIN t1 JOIN MAS_STAGES t2 ON t2.Fid = t1.MAS_STAGES_FID AND t2.Origin = 'Lead'AND t2.Stage != 'Lost' WHERE t1.deleted = 0 AND t1.UserID IN(SELECT EmployeeId FROM Tool_UserLogin WHERE EmployeeId IN(SELECT EmployeeId FROM Mas_Employee WHERE EmployeeId = { EmpID} ) )");

                var LeadList1 = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param);
                ViewBag.LeadList1 = LeadList1;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult TotalTransferLeads(string date)
        {
            try
            {
                //DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
               //var mainRole = Session["MainRole"].ToString();
                var EmpID = Convert.ToInt32(Session["EmployeeId"]);

                //param.Add("@query", $"SELECT acc.*,* FROM ACCOWNERS join ACCMAIN acc on ACCOWNERS.ACCMAIN_Fid = acc.Fid WHERE ACCOWNERS.Deleted = 0 AND ACCOWNERS.TranferBy_Fid IS NOT NULL and('admin' = '{mainRole}' or 'SalesAdmin' = '{mainRole}' or ACCOWNERS.LeadOwner_Fid in (select Fid from USERS where EmployeeId  in (select Fid from fn_GetEmployeeHierarchy ({EmpID}))))");
             param.Add("@query", $"SELECT acc.* FROM ACCOWNERS JOIN ACCMAIN acc ON ACCOWNERS.ACCMAIN_Fid = acc.Fid WHERE ACCOWNERS.Deleted = 0 AND ACCOWNERS.TranferBy_Fid IS NOT NULL AND ACCOWNERS.LeadOwner_Fid IN(SELECT EmployeeId FROM Tool_UserLogin WHERE EmployeeId IN(SELECT EmployeeId FROM Mas_employee WHERE EmployeeId = { EmpID}))");
                var TransferLead = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param);
                ViewBag.TransferLead = TransferLead;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult TotalLeadContractValue(string date)
        {
            try
            {
                //DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
              //  var mainRole = Session["MainRole"].ToString();
                var EmpID = Convert.ToInt32(Session["EmployeeId"]);

                //param.Add("@query", $"select * from ACCMAIN left join USERS on users.fid = ACCMAIN.UserId  WHERE ACCMAIN.Deleted = 0 AND('admin' = '{mainRole}' or 'SalesAdmin' = '{mainRole}' or USERS.EmployeeId in (select Fid from USERS where EmployeeId  in (select Fid from fn_GetEmployeeHierarchy ({EmpID}))))");
                param.Add("@query", $"SELECT ACCMAIN.* FROM ACCMAIN INNER JOIN Mas_Employee AS emp ON ACCMAIN.UserId = emp.EmployeeId WHERE ACCMAIN.Deleted = 0 AND emp.EmployeeId IN(SELECT EmployeeId FROM Mas_employee Where EmployeeId ={ EmpID})AND emp.Deactivate = 0 AND emp.EmployeeLeft = 0 ");

                var LeadContractValue = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param);
                ViewBag.LeadContractValue = LeadContractValue;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult TotalLeadPriority(string date)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
              //  var mainRole = Session["MainRole"].ToString();
                var EmpID = Convert.ToInt32(Session["EmployeeId"]);

                //param.Add("@query", $"Select * from AccMain acc join MAS_STAGES stages on stages.Fid = acc.MAS_STAGES_FID and stages.Deleted = 0 where stages.Origin = 'Lead' and acc.Deleted = 0  and acc.Stage != 'Lost' and('{mainRole}' = 'admin' or '{mainRole}' = 'SalesAdmin' or acc.UserId in (select Fid from USERS where EmployeeId  in (select Fid from fn_GetEmployeeHierarchy ({EmpID}))))");
                param.Add("@query", $"SELECT * FROM AccMain acc JOIN MAS_STAGES stages ON stages.Fid = acc.MAS_STAGES_FID AND stages.Deleted = 0 WHERE stages.Origin = 'Lead' AND acc.Deleted = 0 AND acc.Stage != 'Lost' AND acc.UserId IN(SELECT EmployeeId FROM Tool_UserLogin WHERE EmployeeId IN(SELECT EmployeeID FROM Mas_Employee WHERE EmployeeID = { EmpID}) )");
                var TotalLeadPriority = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param);
                ViewBag.TotalLeadPriority = TotalLeadPriority;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult CurrentmonthTransferedLeads(string date)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                //var mainRole = Session["MainRole"].ToString();
                var EmpID = Convert.ToInt32(Session["EmployeeId"]);

                //  param.Add("@query", $"SELECT * ,(select CompanyName from ACCMAIN where Fid =ACCOWNERS.ACCMAIN_Fid) as CompanyName,(select EmployeeName from MAS_EMPLOYEES where Fid =ACCOWNERS.LeadOwner_Fid) as LeadOwner,(select EmployeeName from MAS_EMPLOYEES where Fid =ACCOWNERS.TranferBy_Fid) as TransferredName FROM ACCOWNERS WHERE Deleted = 0  AND TranferBy_Fid IS NOT NULL and  ('admin'= '{mainRole}' or 'SalesAdmin' = '{mainRole}' or LeadOwner_Fid in (select Fid from USERS where EmployeeId  in (select Fid from fn_GetEmployeeHierarchy ({EmpID})))) and Format(Fdate,'yyyy-MM')= '{date}'");
                param.Add("@query", $@" SELECT a.*,
    (SELECT CompanyName FROM ACCMAIN WHERE Fid = a.ACCMAIN_Fid) AS CompanyName,
    (SELECT EmployeeName FROM Mas_Employee WHERE EmployeeId = a.LeadOwner_Fid) AS LeadOwner,
    (SELECT EmployeeName FROM Mas_Employee WHERE EmployeeId = a.TranferBy_Fid) AS TransferredName
     FROM ACCOWNERS a WHERE a.Deleted = 0 AND a.TranferBy_Fid IS NOT NULL
    AND a.LeadOwner_Fid IN (SELECT EmployeeId FROM Tool_UserLogin WHERE EmployeeId IN (
     SELECT EmployeeId FROM Mas_Employee WHERE EmployeeId = 5068))AND FORMAT(a.Fdate, 'yyyy-MM') = '{date}';");
                
                var TransferredLeads = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param);
                ViewBag.TransferredLeads = TransferredLeads;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult HotPriorityLeadsLinkLeads(string date)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                //   var mainRole = Session["MainRole"].ToString();
                var EmpID = Convert.ToInt32(Session["EmployeeId"]);

                //param.Add("@query", $"Select acc.*,stages.Origin,stages.Stage from AccMain acc join MAS_STAGES stages on stages.Fid = acc.MAS_STAGES_FID and stages.Deleted = 0 where stages.Origin = 'Lead' and acc.Stage != 'Lost' and acc.LeadPriority IN ('Hot', 'Cold', 'Warm') and acc.Deleted = 0 and ('{mainRole}' = 'admin' or '{mainRole}' = 'SalesAdmin' or acc.UserId in (select Fid from USERS where EmployeeId  in (select Fid from fn_GetEmployeeHierarchy ({EmpID})))) and Format(acc.Fdate,'yyyy-MM') ='" + date + "'");
                param.Add("@query", $@"SELECT DISTINCT acc.*, stages.Origin, stages.Stage FROM AccMain AS acc JOIN MAS_STAGES AS stages 
                 ON stages.Fid = acc.MAS_STAGES_FID AND stages.Deleted = 0  WHERE stages.Origin = 'Lead'
                 AND acc.Stage != 'Lost' AND acc.LeadPriority IN ('Hot', 'Cold', 'Warm')AND acc.Deleted = 0 
                AND acc.UserId IN (SELECT EmployeeId FROM Tool_UserLogin WHERE EmployeeId IN (SELECT EmployeeId FROM Mas_Employee WHERE EmployeeId = {EmpID}))AND FORMAT(acc.Fdate, 'yyyy-MM') = '{date}'");

                var HotPriority = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param);
                ViewBag.HotPriority = HotPriority;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult ExpectedContractValueDetails(string date)
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
               // var mainRole = Session["MainRole"].ToString();
                var empId = Convert.ToInt32(Session["EmployeeId"]);

                //param.Add("@query", $"select ac.*,(select Stage from MAS_STAGES where Fid=ac.MAS_STAGES_FID) as Stage,(select Origin from MAS_STAGES where Fid=ac.MAS_STAGES_FID) as Origin from ACCMAIN as ac left join USERS as us on us.fid = ac.UserId WHERE ac.Deleted = 0 AND ('admin'= '{mainRole}' or 'SalesAdmin' = '{mainRole}' or us.EmployeeId IN (select Fid from fn_GetEmployeeHierarchy ({empId}))) and Format(ac.Fdate,'yyyy-MM') = '" + date + "'");
                param.Add("@query", $@"SELECT DISTINCT ac.*,(SELECT Stage FROM MAS_STAGES WHERE Fid = ac.MAS_STAGES_FID) AS Stage,(SELECT Origin FROM MAS_STAGES WHERE Fid = ac.MAS_STAGES_FID) AS Origin
              FROM ACCMAIN AS ac LEFT JOIN Tool_UserLogin AS us ON us.EmployeeId = ac.UserId
              WHERE ac.Deleted = 0 AND us.EmployeeId IN (SELECT EmployeeId FROM Mas_Employee WHERE EmployeeId = {empId}) AND FORMAT(ac.Fdate, 'yyyy-MM') = '{date}'");
                
                var ContractList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param);
                ViewBag.ContractList = ContractList;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }


        public JsonResult GetAnalyticsData(string filterType, string d1)
        {
            try
            {
                //  DapperORM.SetConnection();
                string a = d1 + "-" + "01";
                DateTime now = Convert.ToDateTime(a);

                DateTime startDate = new DateTime(now.Year, now.Month, 1);
                DateTime endDate = startDate.AddMonths(1).AddDays(-1);
                // edited
                // var mainRole = Session["MainRole"]?.ToString();
                int? employeeId = null;

                if (employeeId != null)
                {
                    employeeId = Convert.ToInt32(Session["EmployeeId"]);
                }
                // edited
                string selectField = filterType == "Weekly"
                    ? "DAY(ac.Fdate)"
                    : $"'{now.ToString("yyyy-MM")}'";

                string groupBy = filterType == "Weekly"
                    ? "DAY(ac.Fdate)"
                    : null;
                
                var data = DapperORM.ExecuteSP<dynamic>("Sp_GetAnalyticsData", new{FilterType = filterType, StartDate = startDate,EndDate = endDate,YearMonth = now.ToString("yyyy-MM")}).ToList();
                
                var result = new
                {
                    success = true,
                    labels = data.Select(d => d.Period.ToString()),
                    leadCounts = data.Select(d => Convert.ToInt32(d.LeadCount)),
                    opportunityCounts = data.Select(d => Convert.ToInt32(d.OpportunityCount)),
                    accountCounts = data.Select(d => Convert.ToInt32(d.AccountCount)) // ✅ new column
                };
                
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while retrieving analytics data.",
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
        
        public ActionResult ProposalReport()
        {
            try
            {
                // DapperORM.SetConnection();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                //   var MainRole = Session["MainRole"].ToString();
                var UserId = Session["EmployeeId"].ToString();

               // param.Add("@MainRole", MainRole);
                param.Add("@UserId", UserId);

                var ProposalDetails = DapperORM.ReturnList<dynamic>("Sp_GetProposalDetails", param).ToList();
                ViewBag.ProposalDetails = ProposalDetails;

                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { area = "" });
            }
        }


     #region(Daily EMP/Site By GRP)

        public ActionResult DailyPresentEmployeeList()
        {

            try
            {
                if (Session["EmployeeName"] != null)
                {
                    //param.Add("@query", $"select Distinct mgmt.MAS_Employee_Fid,emp.EmployeeName,mgmt.InLat,mgmt.InLong,mgmt.OutLat,mgmt.OutLong,(select LocationName from ACCSITELOCT where Fid=emp.ACCSITES_Fid) as LocationName ,mgmt.InDate from ATD_DAYMGMT as mgmt join MAS_EMPLOYEES as emp on mgmt.MAS_Employee_Fid=emp.Fid where CONVERT(Date,mgmt.InDate)= CONVERT(Date,GETDATE()) and mgmt.Deleted=0 and emp.EmployeeCategory='Sales'");
                    param.Add("@query", $"select Distinct mgmt.CheckInOutEmployeeId,emp.EmployeeName, mgmt.CheckInOutLatitude, mgmt.CheckInOutLongitude,mgmt.CheckInOutDateTime from Atten_CheckInOut as mgmt join Mas_Employee as emp on mgmt.CheckInOutEmployeeId = emp.EmployeeId where emp.Deactivate = 0 AND emp.EmployeeLeft = 0 AND mgmt.Deactivate = 0 AND CONVERT(Date, mgmt.CheckInOutDateTime) = CONVERT(Date, GETDATE())");
                    var PresentEmployeeList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param);
                    ViewBag.PresentEmployeeList = PresentEmployeeList;
                    return View();
                }
                else
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                };
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult DailyAbsentEmployeeList()
        {

            try
            {
                if (Session["EmployeeName"] != null)
                {
                    //param.Add("@query", $"select Fid as MAS_Employee_Fid ,EmployeeName,(select LocationName from ACCSITELOCT where Fid=MAS_EMPLOYEES.ACCSITES_Fid) as LocationName ,CONVERT(Date,GETDATE())  as InDate from MAS_EMPLOYEES where EmployeeCategory='Sales' and Fid not in(select MAS_Employee_Fid from ATD_DAYMGMT where  CONVERT(Date,InDate)= CONVERT(Date,GETDATE()))");
                    param.Add("@query", $"SELECT ME.EmployeeId, ME.EmployeeName, CONVERT(date, GETDATE()) AS CheckInOutDateTime  FROM Mas_Employee ME WHERE ME.EmployeeLeft = 0 AND ME.Deactivate = 0 AND ME.EmployeeId NOT IN( SELECT ACO.CheckInOutEmployeeId FROM Atten_CheckInOut ACO WHERE ACO.Deactivate = 0 AND CONVERT(date, ACO.CheckInOutDateTime) = CONVERT(date, GETDATE()))ORDER BY ME.EmployeeName");
                    var AbsentEmployeeList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param);
                    ViewBag.AbsentEmployeeList = AbsentEmployeeList;
                    return View();
                }
                else
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                };
            }
            catch (Exception ex)
            {

                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult DailySiteWiseEmployeeList()
        {

            try
            {
                if (Session["EmployeeName"] != null)
                {
                    param.Add("@query", $"select Distinct atd.MAS_Employee_Fid,emp.EmployeeName, atd.ACCSITES_Fid ,(select LocationName from ACCSITELOCT where Fid= atd.ACCSITES_Fid) as LocationName,atd.InDate from ATD_OPSATD as atd  join MAS_EMPLOYEES as emp on emp.Fid = atd.MAS_Employee_Fid where atd.ACCSITES_Fid IS NOT NULL and atd.Deleted = 0 and emp.EmployeeCategory = 'Sales' and CONVERT(Date, atd.InDate) = CONVERT(Date, GETDATE())");
                    var SiteWiseEmployeeList = DapperORM.ReturnList<dynamic>("Sp_QueryExcution", param);
                    ViewBag.SiteWiseEmployeeList = SiteWiseEmployeeList;
                    return View();
                }
                else
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                };
            }
            catch (Exception ex)
            {

                return RedirectToAction("Login", "Login", new { area = "" });
            }
        }
        #endregion

    }
}

