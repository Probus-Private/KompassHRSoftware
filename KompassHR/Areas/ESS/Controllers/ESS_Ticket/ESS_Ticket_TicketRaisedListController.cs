using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using KompassHR.Areas.ESS.Models.ESS_Ticket;
using System.Net;
using System.Data;

namespace KompassHR.Areas.ESS.Controllers.ESS_Ticket
{
    public class ESS_Ticket_TicketRaisedListController : Controller
    {
        // GET: ESS/ESS_Ticket_TicketRaisedList

        #region ESS_Ticket_TicketRaisedList
        public ActionResult ESS_Ticket_TicketRaisedList(string status)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 201;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                var EmployeeId = Session["EmployeeId"];

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_TicketRaiseID_Encrypted", "List");
                param.Add("@p_EmployeeId", EmployeeId);
                param.Add("@p_Status", status);
                var data = DapperORM.DynamicList("sp_List_TicketRaiseList", param);
                ViewBag.GetTicketRaiseList = data;

                
                DynamicParameters Param1 = new DynamicParameters();
                Param1.Add("@AssignedTo", EmployeeId);
                var result = DapperORM.DynamicMultipleResultList("sp_List_TicketManagerDashbord", Param1);
                
                // Total Tickets
                TempData["GetTotalTickets"] = result.Read<dynamic>().FirstOrDefault()?.TotalTickets;

                // Open Tickets
                TempData["GetOpenTickets"] = result.Read<dynamic>().FirstOrDefault()?.OpenTickets;

                //  In Process
                TempData["GetInProcessTickets"] = result.Read<dynamic>().FirstOrDefault()?.InprocessTickets;

                //  Resolved
                TempData["GetResolvedTickets"] = result.Read<dynamic>().FirstOrDefault()?.ResolvedTickets;

                //  Escalated
                TempData["GetEscalatedTicket"] = result.Read<dynamic>().FirstOrDefault()?.TotalCount;

                // Critical
                TempData["GetCriticalTicket"] = result.Read<dynamic>().FirstOrDefault()?.TotalCount;

                // High
                TempData["GetHighTicket"] = result.Read<dynamic>().FirstOrDefault()?.TotalCount;

                //  Medium
                TempData["GetMediumTicket"] = result.Read<dynamic>().FirstOrDefault()?.TotalCount;

                //  Low
                TempData["GetLowTicket"] = result.Read<dynamic>().FirstOrDefault()?.TotalCount;

                //  Today Created
                TempData["GetTodayCreate"] = result.Read<dynamic>().FirstOrDefault()?.TotalCount;

                //  Today Resolved
                TempData["GetTodayResolved"] = result.Read<dynamic>().FirstOrDefault()?.TotalCount;

                //  Average Resolution Time
                TempData["AvgResolutionTime"] = result.Read<dynamic>().FirstOrDefault()?.AvgResolutionTime;

                //Average Response Time
                TempData["AvgResponseTime"] = result.Read<dynamic>().FirstOrDefault()?.AvgResponseTime;

                //  Due Today Resolve
                ViewBag.DueTodayResolve = result.Read<dynamic>().ToList();

                //Due Today Response
                ViewBag.DueTodayResponse = result.Read<dynamic>().ToList();

                //  Last 8 Day Resolved
                ViewBag.Last8DayResponse = result.Read<dynamic>().ToList();

                //  Last 8 Day Created
                ViewBag.Last8DayCreated = result.Read<dynamic>().ToList();

                //  Category Wise
                ViewBag.CategoryName = result.Read<dynamic>().ToList();

                //  Top 3 Problem Category
                ViewBag.TopProblemCategoryName = result.Read<dynamic>().ToList();

                int totalTickets = Convert.ToInt32(TempData["GetTotalTickets"] ?? 0);

                int openTickets = Convert.ToInt32(TempData["GetOpenTickets"] ?? 0);
                int inProcessTickets = Convert.ToInt32(TempData["GetInProcessTickets"] ?? 0);
                int CloseTicket = Convert.ToInt32(TempData["GetResolvedTickets"] ?? 0);
                int CriticalPriority = Convert.ToInt32(TempData["GetCriticalTicket"] ?? 0);
                int HighPriority = Convert.ToInt32(TempData["GetHighTicket"] ?? 0);

                if (totalTickets > 0)
                {
                    ViewBag.OpenPercent = (openTickets * 100) / totalTickets;
                    ViewBag.InProcessPercent = (inProcessTickets * 100) / totalTickets;
                    ViewBag.ClosePercent = (CloseTicket * 100) / totalTickets;
                    ViewBag.CriticalPercent = (CriticalPriority * 100) / totalTickets;
                    ViewBag.HighPercent = (HighPriority * 100) / totalTickets;
                }
                else
                {
                    ViewBag.OpenPercent = 0;
                    ViewBag.InProcessPercent = 0;
                    ViewBag.ClosePercent = 0;
                    ViewBag.CriticalPercent = 0;
                    ViewBag.HighPercent = 0;
                }

                string avgResponse = Convert.ToString(TempData["AvgResponseTime"]);
                string avgResolution = Convert.ToString(TempData["AvgResolutionTime"]);

                int avgResponseMinutes = 0;
                int avgResolutionMinutes = 0;

                //if (!string.IsNullOrEmpty(avgResponse))
                //{
                //    var parts = avgResponse.Split(' ');
                //    int hours = Convert.ToInt32(parts[0]);
                //    int minutes = Convert.ToInt32(parts[2]);
                //    avgResponseMinutes = (hours * 60) + minutes;
                //}

                //if (!string.IsNullOrEmpty(avgResolution))
                //{
                //    var parts = avgResolution.Split(' ');
                //    int hours = Convert.ToInt32(parts[0]);
                //    int minutes = Convert.ToInt32(parts[2]);
                //    avgResolutionMinutes = (hours * 60) + minutes;
                //}


                if (!string.IsNullOrEmpty(avgResponse))
                {
                    var parts = avgResponse.Split(' ');

                    if (parts.Length >= 3)
                    {
                        int hours = 0;
                        int minutes = 0;

                        int.TryParse(parts[0], out hours);
                        int.TryParse(parts[2], out minutes);

                        avgResponseMinutes = (hours * 60) + minutes;
                    }
                }

                if (!string.IsNullOrEmpty(avgResolution))
                {
                    var parts = avgResolution.Split(' ');

                    if (parts.Length >= 3)
                    {
                        int hours = 0;
                        int minutes = 0;

                        int.TryParse(parts[0], out hours);
                        int.TryParse(parts[2], out minutes);

                        avgResolutionMinutes = (hours * 60) + minutes;
                    }
                }

                int maxMinutes = 1440; 

           
                ViewBag.AvgResponsePercent = (avgResponseMinutes * 100) / maxMinutes;
                ViewBag.AvgResolutionPercent = (avgResolutionMinutes * 100) / maxMinutes;

                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Ticket_TicketRaise");
            }
        }
        #endregion
        
        #region Download File/Image
        public ActionResult DownloadFile(string FilePath)
        {
            if (FilePath != null)
            {
                System.IO.File.ReadAllBytes(FilePath);
                return File(FilePath, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(FilePath));
            }
            else
            {
                return RedirectToAction("ESS_Ticket_TicketRaisedList", "ESS_Ticket_TicketRaisedList", new { Area = "ESS" });
            }
        }
        #endregion
        
        #region ESS_Ticket_TicketSolution view
        public ActionResult ViewTicketSolution(string TicketRaiseID_Encrypted,int TicketRaiseID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    DynamicParameters param1 = new DynamicParameters();
                    var DocNo = DapperORM.DynamicQuerySingle("Select Isnull(Max(DocNo),0)+1 As DocNo from Ticket_Raise_SLA_Action");
                    ViewBag.DocNo = DocNo;
                }
                TempData["DocDate"] = DateTime.Now;
                ViewBag.TicketRaiseIDFromSP = TicketRaiseID;

                ViewBag.AddUpdateTitle = "Add";

               DynamicParameters param = new DynamicParameters();
                //param.Add("@p_TicketRaiseID_Encrypted", "List");
                param.Add("@p_TicketRaiseID_Encrypted", TicketRaiseID_Encrypted);
                param.Add("@p_TicketRaiseID", TicketRaiseID);
                var result = DapperORM.ReturnList<dynamic>("sp_List_ViewTicketSolution", param).ToList();
                ViewBag.GetTicketList = result;

                if (result != null && result.Count > 0)
                {
                    ViewBag.TicketRaiseEmployeeIDFromSP = result[0].TicketRaiseEmployeeID;
                }
                else
                {
                    ViewBag.TicketRaiseEmployeeIDFromSP = null;
                }

              
                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Ticket_TicketRaise");
            }

        }
        #endregion

        //#region SaveUpdate
        //[HttpPost]
        //public ActionResult SaveUpdate(Ticket_Raise_SLA_Action Ticket_Raise_SLA_Action, HttpPostedFileBase FilePath)
        //{
        //    try
        //    {
        //        DynamicParameters param = new DynamicParameters();
        //        var EmployeeId = Session["EmployeeId"];
        //       param.Add("@p_process", string.IsNullOrEmpty(Ticket_Raise_SLA_Action.TicketRaiseSLAActionId_Encrypted) ? "Save" : "Update");
        //      // param.Add("@p_TicketRaiseSLAActionId", Ticket_Raise_SLA_Action.TicketRaiseSLAActionId);
        //        param.Add("@p_TicketRaiseSLAActionId_Encrypted", Ticket_Raise_SLA_Action.TicketRaiseSLAActionId_Encrypted);
        //        param.Add("@p_SLAAction_TicketRaiseID", Ticket_Raise_SLA_Action.SLAAction_TicketRaiseID);
        //        param.Add("@p_TicketRaiseEmployeeID", Ticket_Raise_SLA_Action.TicketRaiseEmployeeID);
        //        param.Add("@p_AssignedTo", Session["EmployeeId"]);
        //        param.Add("@p_DocNo", Ticket_Raise_SLA_Action.DocNo);
        //        param.Add("@p_DocDate", Ticket_Raise_SLA_Action.DocDate);
        //        param.Add("@p_Solution", Ticket_Raise_SLA_Action.Solution);
        //        param.Add("@p_Status", Ticket_Raise_SLA_Action.Status);

        //        if (Ticket_Raise_SLA_Action.TicketRaiseSLAActionId_Encrypted != null && FilePath == null)
        //        {
        //            param.Add("@p_FilePath", Session["SelectedFile"]);
        //        }
        //        else
        //        {
        //            param.Add("@p_FilePath", FilePath == null ? "" : FilePath.FileName);
        //        }
        //        param.Add("@p_MachineName", Dns.GetHostName().ToString());
        //        param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
        //        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //        param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //        //var Result = DapperORM.ExecuteReturn("sp_SUD_Ticket_Solution", param);
        //        var Result = DapperORM.ExecuteReturn("sp_SUD_Ticket_Raise_SLA_Action", param);
        //        //TempData["Message"] = param.Get<string>("@p_msg");
        //        //TempData["Icon"] = param.Get<string>("@p_Icon");
        //        //TempData["P_Id"] = param.Get<string>("@p_Id");


        //        var msg = param.Get<string>("@p_msg");
        //        var icon = param.Get<string>("@p_Icon");
        //        var newId = param.Get<string>("@p_Id");

        //        if (TempData["P_Id"] != null)
        //        {
        //            var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Ticket_Solution'");
        //            var GetFirstPath = GetDocPath.DocInitialPath;
        //            var FirstPath = GetFirstPath + TempData["P_Id"] + "\\";
        //            if (!Directory.Exists(FirstPath))
        //            {
        //                Directory.CreateDirectory(FirstPath);
        //            }
        //            if (FilePath != null)
        //            {
        //                string URLFileFullPath = "";
        //                URLFileFullPath = FirstPath + FilePath.FileName; 
        //                FilePath.SaveAs(URLFileFullPath); 
        //            }
        //        }
        //        else
        //        {
        //            var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Ticket_Solution'");
        //            var GetFirstPath = GetDocPath.DocInitialPath;
        //            var FirstPath = GetFirstPath + Ticket_Raise_SLA_Action.SLAAction_TicketRaiseID + "\\";
        //            if (!Directory.Exists(FirstPath))
        //            {
        //                Directory.CreateDirectory(FirstPath);
        //            }
        //            if (FilePath != null)
        //            {
        //                string URLFileFullPath = "";
        //                URLFileFullPath = FirstPath + FilePath.FileName; 
        //                FilePath.SaveAs(URLFileFullPath); 
        //            }
        //        }
        //        // return RedirectToAction("ESS_Ticket_TicketRaisedList", "ESS_Ticket_TicketRaisedList");

        //        return Json(new
        //        { success = true,message = msg, icon = icon, id = newId, }, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception Ex)
        //    {

        //        return RedirectToAction(Ex.Message.ToString(), "ESS_Ticket_TicketSolution");
        //    }
        //}

        //#endregion
        
        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Ticket_Raise_SLA_Action Ticket_Raise_SLA_Action, HttpPostedFileBase FilePath)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                var EmployeeId = Session["EmployeeId"];

                param.Add("@p_process", string.IsNullOrEmpty(Ticket_Raise_SLA_Action.TicketRaiseSLAActionId_Encrypted) ? "Save" : "Update");
                param.Add("@p_TicketRaiseSLAActionId_Encrypted", Ticket_Raise_SLA_Action.TicketRaiseSLAActionId_Encrypted);
                param.Add("@p_SLAAction_TicketRaiseID", Ticket_Raise_SLA_Action.SLAAction_TicketRaiseID);
                param.Add("@p_TicketRaiseEmployeeID", Ticket_Raise_SLA_Action.TicketRaiseEmployeeID);
                param.Add("@p_AssignedTo", Session["EmployeeId"]);
                param.Add("@p_DocNo", Ticket_Raise_SLA_Action.DocNo);
                param.Add("@p_DocDate", DateTime.Now);
                param.Add("@p_Solution", Ticket_Raise_SLA_Action.Solution);
                param.Add("@p_Status", Ticket_Raise_SLA_Action.Status);

                // FilePath Logic
                if (Ticket_Raise_SLA_Action.TicketRaiseSLAActionId_Encrypted != null && FilePath == null)
                {
                    param.Add("@p_FilePath", Session["SelectedFile"]);
                }
                else
                {
                    param.Add("@p_FilePath", FilePath == null ? "" : FilePath.FileName);
                }

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                var Result = DapperORM.ExecuteReturn("sp_SUD_Ticket_Raise_SLA_Action", param);

                var msg = param.Get<string>("@p_msg");
                var icon = param.Get<string>("@p_Icon");
                var newId = param.Get<string>("@p_Id"); 
                var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Ticket_Solution'");
                var BasePath = GetDocPath.DocInitialPath;

                string FolderPath = "";
                
                if (!string.IsNullOrEmpty(newId))
                {
                    FolderPath = BasePath + newId + "\\";
                }
                else 
                {
                    FolderPath = BasePath + Ticket_Raise_SLA_Action.SLAAction_TicketRaiseID + "\\";
                }
                if (!Directory.Exists(FolderPath))
                {
                    Directory.CreateDirectory(FolderPath);
                }

                if (FilePath != null)
                {
                    string FullPath = FolderPath + FilePath.FileName;
                    FilePath.SaveAs(FullPath);
                }

                return Json(new
                { success = true,message = msg, icon = icon,
                    id = newId,
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Ticket_TicketSolution");
            }
        }
        #endregion

        #region ViewTicketSolutionHistoryList
        public ActionResult ViewTicketSolutionHistoryList(int? TicketRaiseID)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_EmployeeId", EmployeeId);
                param.Add("@p_TicketRaiseID", TicketRaiseID);
                var data = DapperORM.DynamicList("sp_List_Ticket_Solution", param);
                ViewBag.GetTicketSolutionHistory = data;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion
        
        #region DownloadAttachment

        public FileResult DownloadSolutionHistory(string TicketRaiseSLAActionId_Encrypted)
        {
            try
            {
                var EmployeeId = Session["EmployeeId"];
                if (EmployeeId != null)
                {

                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {

                        var SolutionId = "select TicketRaiseSLAActionId from Ticket_Raise_SLA_Action where TicketRaiseSLAActionId_Encrypted='" + TicketRaiseSLAActionId_Encrypted + "'";
                        var id = DapperORM.DynamicQuerySingle(SolutionId);
                        var GetAttachmentPath = "Select FilePath from Ticket_Raise_SLA_Action where TicketRaiseSLAActionId_Encrypted='" + TicketRaiseSLAActionId_Encrypted + "'";
                        var AttachmentPath = DapperORM.DynamicQuerySingle(GetAttachmentPath);

                        var GetPath = "Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Ticket_Solution'";
                        var GetDocPath = DapperORM.DynamicQuerySingle(GetPath);

                        var APath = GetDocPath.DocInitialPath + id.TicketRaiseSLAActionId + "\\" + AttachmentPath.FilePath;

                        if (AttachmentPath.FilePath != null)
                        {
                            return File(APath, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(APath));

                        }
                        return null;
                    }

                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion
    }
}