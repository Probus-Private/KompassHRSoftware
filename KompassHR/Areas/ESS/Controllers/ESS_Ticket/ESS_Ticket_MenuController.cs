using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Ticket
{
    public class ESS_Ticket_MenuController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: ESS/ESS_Ticket_Menu
        #region ESS_Ticket_Menu Main View 
        [HttpGet]
        public ActionResult ESS_Ticket_Menu(int? id, int? ScreenId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 199;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                if (ScreenId != null)
                {
                    Session["ModuleId"] = id;
                    Session["ScreenId"] = ScreenId;
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), id, ScreenId, "Form", "Transation");
                    ViewBag.GetUserMenuList = GetMenuList;
                }
                else
                {
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), Convert.ToInt32(Session["ModuleId"]), Convert.ToInt32(Session["ScreenId"]), "Form", "Transation");
                    ViewBag.GetUserMenuList = GetMenuList;
                }
                var EmployeeId = Session["EmployeeId"];

                param.Add("@p_TicketRaiseID_Encrypted", "List");
                param.Add("@p_EmployeeId", EmployeeId);
                var data = DapperORM.DynamicList("sp_List_Ticket_Raise", param);
                
              //  var data = DapperORM.DynamicList("sp_List_OpenTicket_Raise", param);
                ViewBag.GetTicketRaiseList = data;

                DynamicParameters Param1 = new DynamicParameters();

                Param1.Add("@EmployeeId", EmployeeId);
                //  var result = DapperORM.DynamicMultipleResult("sp_List_TicketMenuDetails", Param1);
                var result = DapperORM.DynamicMultipleResultList("sp_List_TicketMenuDetails", Param1);


                var ticketCounts = result.Read<dynamic>().FirstOrDefault();

                TempData["GetTotalTickets"] = ticketCounts?.TotalTickets;
                TempData["GetOpenTickets"] = ticketCounts?.OpenTickets;
                TempData["GetInProcessTickets"] = ticketCounts?.InprocessTickets;
                TempData["GetResolvedTickets"] = ticketCounts?.ResolvedTickets;
                TempData["GetOverdueTickets"] = ticketCounts?.OverdueTickets;

                ViewBag.TicketCategoryCount = result.Read<dynamic>().ToList();


                var priority = result.Read<dynamic>().ToList();
                ViewBag.PriorityCounts = priority;

              ViewBag.Critical = priority.First(x => x.Priority == "Critical").Total;
                ViewBag.High = priority.First(x => x.Priority == "High").Total;
                ViewBag.Medium = priority.First(x => x.Priority == "Medium").Total;
                ViewBag.Low = priority.First(x => x.Priority == "Low").Total;

                //var GetTotalTickets = DapperORM.DynamicQuerySingle(@"select count(*) as TotalTickets from Ticket_Raise where Deactivate=0 and TicketRaiseEmployeeID='" + Session["EmployeeId"] + "'");
                //TempData["GetTotalTickets"] = GetTotalTickets?.TotalTickets;

                //var GetOpenTickets = DapperORM.DynamicQuerySingle(@"select count(*) as OpenTickets from Ticket_Raise where Deactivate=0 and Status='Open' and TicketRaiseEmployeeID='" + Session["EmployeeId"] + "'");
                //TempData["GetOpenTickets"] = GetOpenTickets?.OpenTickets;

                //var GetInProcessTickets = DapperORM.DynamicQuerySingle(@"select count(*) as InprocessTickets from Ticket_Raise where Deactivate=0 and Status='In process' and TicketRaiseEmployeeID='" + Session["EmployeeId"] + "'");
                //TempData["GetInProcessTickets"] = GetInProcessTickets?.InprocessTickets;

                //var GetResolvedTickets = DapperORM.DynamicQuerySingle(@"select count(*) as ResolvedTickets from Ticket_Raise where Deactivate=0 and Status='Close' and TicketRaiseEmployeeID='" + Session["EmployeeId"] + "'");
                //TempData["GetResolvedTickets"] = GetResolvedTickets?.ResolvedTickets;

                //var GetOverdueTickets = DapperORM.DynamicQuerySingle(@"select count(*) as OverdueTickets from Ticket_Raise where Deactivate=0 and Status='Escalated' and TicketRaiseEmployeeID='" + Session["EmployeeId"] + "'");
                //TempData["GetOverdueTickets"] = GetOverdueTickets?.OverdueTickets;

                //var ticketCategoryCount = DapperORM.DynamicQueryListWithParam(@"SELECT  TC.TicketCategoryName,COUNT(TR.TicketCategoryID) AS Total FROM Ticket_Raise TR INNER JOIN Ticket_Category TC  ON TR.TicketCategoryID = TC.TicketCategoryID WHERE TR.Deactivate = 0  AND TC.Deactivate = 0  AND TR.TicketRaiseEmployeeID = @EmployeeId GROUP BY  TC.TicketCategoryName", new { EmployeeId = Session["EmployeeId"] });
                //ViewBag.TicketCategoryCount = ticketCategoryCount;

                //var ticketPriorityCounts = DapperORM.DynamicQueryListWithParam(@"SELECT P.Priority, COUNT(TR.Priority) AS Total, CASE WHEN COUNT(TR.Priority) > 0 THEN CAST((COUNT(TR.Priority) * 100.0) /(SELECT COUNT(*) FROM Ticket_Raise WHERE Deactivate = 0  AND TicketRaiseEmployeeID = '"+ Session["EmployeeId"] + "') AS INT) ELSE 0 END AS Percentage FROM ( SELECT 'Critical' AS Priority UNION ALL SELECT 'High' UNION ALL SELECT 'Medium' UNION ALL SELECT 'Low') P LEFT JOIN Ticket_Raise TR  ON TR.Priority = P.Priority AND TR.Deactivate = 0 AND TR.TicketRaiseEmployeeID = '"+ Session["EmployeeId"] + "' GROUP BY P.Priority");
                //ViewBag.PriorityCounts = ticketPriorityCounts;

                //ViewBag.Critical = ticketPriorityCounts.First(x => x.Priority == "Critical").Total;
                //ViewBag.High = ticketPriorityCounts.First(x => x.Priority == "High").Total;
                //ViewBag.Medium = ticketPriorityCounts.First(x => x.Priority == "Medium").Total;
                //ViewBag.Low = ticketPriorityCounts.First(x => x.Priority == "Low").Total;


                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Ticket_Menu ");
            }
        }
        #endregion

    }
}