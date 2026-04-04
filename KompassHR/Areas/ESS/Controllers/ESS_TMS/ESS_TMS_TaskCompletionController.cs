using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_TMS
{
    public class ESS_TMS_TaskCompletionController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region Main View
        // GET: ESS/ESS_TMS_TMSReports
        public ActionResult ESS_TMS_TaskCompletion(DailyAttendanceReportFilter OBJTMSReport)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 598;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                DynamicParameters paramClient = new DynamicParameters();
                paramClient.Add("@query", "SELECT ClientId as Id, ClientName as Name FROM TMS_Client WHERE Deactivate = 0 ORDER BY Name");
                var Client = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramClient).ToList();
                ViewBag.TMSClient = Client;
          
                ViewBag.TMSModule = "";
                ViewBag.TMSProject = "";
               // ViewBag.TaskCategory = "";

                // ViewBag.GetEmployeeName = "";
                //var results = DapperORM.DynamicQueryMultiple(@"select  ProjectID as Id,ProjectName as Name from TMS_Project where Deactivate=0 order by ProjectName;
                //                                  Select TeamEmployeeId as Id,(Mas_Employee.EmployeeName + ' - ' + CAST(Mas_Employee.EmployeeNo AS VARCHAR)) AS Name from TMS_TeamAssign,Mas_Employee where TeamManagerId=" + Session["EmployeeId"] + " and IsActive=1 and Mas_Employee.EmployeeId=TMS_TeamAssign.TeamEmployeeId and Mas_Employee.Deactivate=0 order by Name");
                //ViewBag.TMSProject = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                //ViewBag.GetEmployeeName = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();


                DynamicParameters paramEmployee = new DynamicParameters();
                paramEmployee.Add("@query", "Select Distinct TeamEmployeeId as Id,(Mas_Employee.EmployeeName + ' - ' + CAST(Mas_Employee.EmployeeNo AS VARCHAR)) AS Name from TMS_TeamAssign,Mas_Employee where TeamManagerId=" + Session["EmployeeId"] + " and IsActive=1 and Mas_Employee.EmployeeId=TMS_TeamAssign.TeamEmployeeId and Mas_Employee.Deactivate=0 order by Name");
                var Employee = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmployee).ToList();
                ViewBag.GetEmployeeName = Employee;

                //DynamicParameters paramTaskCategory = new DynamicParameters();
                //paramTaskCategory.Add("@query", " SELECT TaskCategoryId AS Id, TaskCategoryName AS Name FROM TMS_TaskCategory WHERE Deactivate = 0 ORDER BY Name");
                //var TaskCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramTaskCategory).ToList();
                //ViewBag.TaskCategory = TaskCategory;
                

                //if ((OBJTMSReport.FromDate != DateTime.MinValue) && (OBJTMSReport.ToDate != DateTime.MinValue))
                //{
                //    DynamicParameters param = new DynamicParameters();
                //    if (OBJTMSReport.ProjectID != 0)
                //    {
                //        param.Add("@p_ProjectId", OBJTMSReport.ProjectID);
                //        param.Add("@p_EmployeeId", OBJTMSReport.AssignToEmployeeID);
                //        param.Add("@p_FromDate", OBJTMSReport.FromDate);
                //        param.Add("@p_ToDate", OBJTMSReport.ToDate);
                //    }
                //    else
                //    {
                //        param.Add("@p_ProjectId", "0");
                //        param.Add("@p_EmployeeId", OBJTMSReport.AssignToEmployeeID);
                //        param.Add("@p_FromDate", OBJTMSReport.FromDate);
                //        param.Add("@p_ToDate", OBJTMSReport.ToDate);
                //    }
                //    var data = DapperORM.ExecuteSP<dynamic>("sp_GetTaskCompleteReports", param).ToList();
                //    ViewBag.GetTaskCompleteReport = data;
                //}

                    return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region  GetProject
        [HttpGet]
        public ActionResult GetProject(int ClientID)
        {
              try
               {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_ClientID", ClientID);

                var Project = DapperORM.ReturnList<AllDropDownBind>("sp_Get_TMS_ProjectDropdown", param).ToList();
                ViewBag.TMSProject = Project;
                return Json(new { Project = Project }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetModule
        [HttpGet]
        public ActionResult GetModule(int ProjectID, int ClientID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                List<AllDropDownBind> Module = new List<AllDropDownBind>();
                //List<AllDropDownBind> AssignToEmployeeID = new List<AllDropDownBind>();

                if (ProjectID > 0)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_ProjectId", ProjectID);
                    Module = DapperORM.ReturnList<AllDropDownBind>("sp_GetModuleDropdown", param).ToList();
                    ViewBag.TMSModule = Module;
                    
                    //DynamicParameters param1 = new DynamicParameters();
                    //param1.Add("@p_EmployeeId", Session["EmployeeId"]);

                    //string query = "TMS_TeamAssign.ProjectID =" + ProjectID;
                    //if (ClientID > 0)
                    //{
                    //    query += " and TMS_TeamAssign.clientId =" + ClientID;
                    //}

                    //param1.Add("@P_Qry", query);
                    //AssignToEmployeeID = DapperORM.ReturnList<AllDropDownBind>("sp_TMS_GetAssigntoEmployee", param1).ToList();
                    //ViewBag.GetEmployeeName = AssignToEmployeeID;
                }

                return Json(new { Module = Module}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        //#region GetCategory
        //[HttpGet]
        //public ActionResult GetCategory(int ProjectID, int ModuleID)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { area = "" });
        //        }

        //        List<AllDropDownBind> TaskCategory = new List<AllDropDownBind>();
        //        if (ProjectID > 0 && ModuleID >0)
        //        {
        //            DynamicParameters paramTaskCategory = new DynamicParameters();
        //            paramTaskCategory.Add("@query", " select TMS_TaskCategory.TaskCategoryId As Id,TMS_TaskCategory.TaskCategoryName As Name from TMS_TaskCate_ProjModuleMapping Left Join TMS_TaskCategory On TMS_TaskCategory.TaskCategoryId = TMS_TaskCate_ProjModuleMapping.TaskCategoryId Where TMS_TaskCate_ProjModuleMapping.ProjectId = '" + ProjectID + "' AND TMS_TaskCate_ProjModuleMapping.ModuleId = '" + ModuleID + "' AND TMS_TaskCate_ProjModuleMapping.Deactivate = 0 order By Name");
        //            TaskCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramTaskCategory).ToList();

        //            ViewBag.TaskCategory = TaskCategory;
        //       }
        //        return Json(new { TaskCategory = TaskCategory }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
        //}
        //#endregion

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? AssignToEmployeeID, DateTime fromDate, DateTime ToDate,int ClientID,int ProjectID,int ModuleID)
        {
            try
            {
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("YearlyAttendanceDetails");
                worksheet.Range(1, 1, 1, 26).Merge();
                worksheet.SheetView.FreezeRows(2);
                System.Data.DataTable dt = new System.Data.DataTable();
                List<dynamic> data = new List<dynamic>();

                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_AssignToEmployeeID", AssignToEmployeeID);
                paramList.Add("@p_FromDate", fromDate);
                paramList.Add("@p_ToDate", ToDate);
                paramList.Add("@p_ClientID", ClientID);
                paramList.Add("@p_ProjectID", ProjectID);
                paramList.Add("@p_ModuleID", ModuleID);
              //  paramList.Add("@p_TaskCategoryId", TaskCategoryId);

                data = DapperORM.ExecuteSP<dynamic>("sp_Rpt_TMS_GetTaskCompleteReports", paramList).ToList();
                if (data.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }
                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(data);
                worksheet.Cell(2, 1).InsertTable(dt, false);

                int totalRows = worksheet.RowsUsed().Count();

                var usedRange = worksheet.RangeUsed();
                usedRange.Style.Fill.BackgroundColor = XLColor.White;
                usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Font.FontSize = 10;
                usedRange.Style.Font.FontColor = XLColor.Black;

                worksheet.Cell(1, 1).Value = "Task Completed Report";

                //worksheet.Cell(1, 1).Value = "Yearly Wise Loan Details Report - (" + FromDate.ToString("MMM/yyyy") + ") - (" + ToDate.ToString("MMM/yyyy") + ")";
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Columns().AdjustToContents();

                var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                headerRange.Style.Font.FontSize = 10;
                headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
                headerRange.Style.Font.Bold = true;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report.xlsx");
                }
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