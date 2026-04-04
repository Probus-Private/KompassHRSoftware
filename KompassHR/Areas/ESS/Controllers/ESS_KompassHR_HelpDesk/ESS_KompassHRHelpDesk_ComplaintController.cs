using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.ESS.Models.ESS_KompassHR_HelpDesk;

namespace KompassHR.Areas.ESS.Controllers.ESS_KompassHR_HelpDesk
{
    public class ESS_KompassHRHelpDesk_ComplaintController : Controller
    {
        SqlConnection sqlcons = new SqlConnection(DapperORM.connectionStrings);
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_KompassHRHelpDesk_Complaint

        #region MainView
        public ActionResult ESS_KompassHRHelpDesk_Complaint()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 794;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_HelpDeskId_Encrypted", "List");
                param.Add("@p_Qry", " ORDER BY HelpDeskId DESC");
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                var data = DapperORM.DynamicList("sp_List_Kompass_Helpdesk", param);
                ViewBag.GetList = data;
                string msg = param.Get<string>("@p_msg");


                DynamicParameters param1 = new DynamicParameters();

                var DashboardCount = DapperORM.ReturnList<HelpDeskDashboard>("sp_Dashboard_KompassHR_Helpdesk_Complaints", null).FirstOrDefault();
                ViewBag.DashboardCount = DashboardCount;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region OpenAttachment
        public ActionResult OpenAttachment(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return HttpNotFound("File path not provided");

            try
            {
                filePath = System.Net.WebUtility.UrlDecode(filePath);

                if (!System.IO.File.Exists(filePath))
                    return HttpNotFound("File not found");

                string fileName = Path.GetFileName(filePath);  // ✅ FIXED — needs "using System.IO;"
                string contentType = MimeMapping.GetMimeMapping(fileName);

                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

                // "inline" = open in browser (image/pdf)
                Response.AddHeader("Content-Disposition", "inline; filename=" + fileName);
                return File(fileBytes, contentType);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, "Error opening file: " + ex.Message);
            }
        }
        #endregion

        #region UpdateStatus
        public ActionResult UpdateStatus(string HelpDeskId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {                   
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var sql = @"Update Kompass_HelpDesk set Status='In-Progress' WHERE HelpDeskId_Encrypted = @HelpDeskId_Encrypted";
                DapperORM.Executes(sql, new { HelpDeskId_Encrypted = HelpDeskId_Encrypted });

                param = new DynamicParameters();
                var query = "Select CustomerCode from Kompass_HelpDesk where HelpDeskId_Encrypted='" + HelpDeskId_Encrypted + "'";
                var Code = DapperORM.DynamicQuerySingle(query);
                string CustomerCode = Convert.ToString(Code.CustomerCode);

                var GetConnectionString = sqlcons.Query(@"Select Con_Server,Con_UserId,Con_Password,Con_Database,Con_CustomerCode from  CustomerRegistration where Con_CustomerCode='" + CustomerCode + "' and Deactivate=0 and Isactive=1").FirstOrDefault();
                if (GetConnectionString == null)
                {
                    var data = "";
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                if (GetConnectionString.Con_CustomerCode == CustomerCode)
                {
                    DapperORM.SetConnectionHelpDesk(GetConnectionString.Con_Server, GetConnectionString.Con_UserId, GetConnectionString.Con_Password, GetConnectionString.Con_Database);
                    var sql1 = @"Update Kompass_HelpDesk set Status='In-Progress' WHERE HelpDeskId_Encrypted = @HelpDeskId_Encrypted";
                    DapperORM.ExecutesHelpDeskQuery(sql1, new { HelpDeskId_Encrypted = HelpDeskId_Encrypted });
                }
               
                return Json(true,JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Response
        public ActionResult TicketResponse(string HelpDeskId_Encrypted,int complaintId,string newStatus,string notes,string actualResultUpdate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var sql = @"Update Kompass_HelpDesk set Status='"+ newStatus + "' , ResolutionComment='"+ notes + "' , ResolutionActualResult='"+ actualResultUpdate + "',ResolutionDate=GetDate(),ResolutionBy= '"+ Session["EmployeeId"] + "' WHERE HelpDeskId = @HelpDeskId";
                DapperORM.Executes(sql, new { HelpDeskId = complaintId });

                param = new DynamicParameters();
                var query = "Select CustomerCode from Kompass_HelpDesk where HelpDeskId_Encrypted='" + HelpDeskId_Encrypted + "'";
                var Code = DapperORM.DynamicQuerySingle(query);
                string CustomerCode = Convert.ToString(Code.CustomerCode);

                var GetConnectionString = sqlcons.Query(@"Select Con_Server,Con_UserId,Con_Password,Con_Database,Con_CustomerCode from  CustomerRegistration where Con_CustomerCode='" + CustomerCode + "' and Deactivate=0 and Isactive=1").FirstOrDefault();
                if (GetConnectionString == null)
                {
                    var data = "";
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                if (GetConnectionString.Con_CustomerCode == CustomerCode)
                {
                    DapperORM.SetConnectionHelpDesk(GetConnectionString.Con_Server, GetConnectionString.Con_UserId, GetConnectionString.Con_Password, GetConnectionString.Con_Database);
                    var sql1 = @"Update Kompass_HelpDesk set Status='" + newStatus + "' , ResolutionComment='" + notes + "' , ResolutionActualResult='" + actualResultUpdate + "' ,ResolutionDate=GetDate() ,ResolutionBy= '" + Session["EmployeeId"] + "' WHERE HelpDeskId = @HelpDeskId";
                    DapperORM.ExecutesHelpDeskQuery(sql1, new { HelpDeskId = complaintId });
                }
                var Message = "Record saved successfully";
                var Icon = "success";
                return Json(new { Message , Icon }, JsonRequestBehavior.AllowGet);
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