using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;


namespace KompassHR.Areas.Reports.Controllers.Report_Claim
{
    public class Reports_Claim_TravelReportController : Controller
    {
        // GET: Reports/Reports_Claim_TravelReport
        public ActionResult Reports_Claim_TravelReport()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 392;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.CompanyName = GetComapnyName;

                var CmpId = GetComapnyName[0].Id;
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                ViewBag.BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                //DynamicParameters paramEmpName = new DynamicParameters();
                //paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and Mas_Employee.EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and Atten_InOut.InOutBranchId in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ") and month( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("MM") + "' and year( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("yyyy") + "' and EmployeeId<>1) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ") and( month(mas_employee.JoiningDate)<='" + DateTime.Now.Date.ToString("MM") + "' and year (mas_employee.JoiningDate)<='" + DateTime.Now.Date.ToString("yyyy") + "') and (month(LeavingDate)='" + DateTime.Now.Date.ToString("MM") + "' and year(mas_employee.LeavingDate)='" + DateTime.Now.Date.ToString("yyyy") + "' or mas_employee.LeavingDate is null) order by Name");
                //ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                DynamicParameters paramEmployee = new DynamicParameters();
                paramEmployee.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0  and CmpId=" + CmpId + " order by EmployeeName");
                ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", paramEmployee).ToList();

                var results = DapperORM.DynamicQueryMultiple(@"select DepartmentId as Id, DepartmentName as Name from Mas_Department where Deactivate=0 order by DepartmentName; 
                                                  select DesignationId as Id,DesignationName as Name from Mas_Designation where Deactivate=0 order by DesignationName
                                                  select GradeId as Id,GradeName as Name from Mas_Grade where Deactivate=0 order by Name");
                ViewBag.GradeName = results[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                DynamicParameters paramEmployee = new DynamicParameters();
                paramEmployee.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0  and CmpId=" + CmpId + " order by EmployeeName");
                var EmployeeName = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", paramEmployee).ToList();

                //DynamicParameters paramEmpName = new DynamicParameters();
                //paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and Mas_Employee.EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and Atten_InOut.InOutBranchId in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ") and month( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("MM") + "' and year( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("yyyy") + "' and EmployeeId<>1) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ") and( month(mas_employee.JoiningDate)<='" + DateTime.Now.Date.ToString("MM") + "' and year (mas_employee.JoiningDate)<='" + DateTime.Now.Date.ToString("yyyy") + "') and (month(LeavingDate)='" + DateTime.Now.Date.ToString("MM") + "' and year(mas_employee.LeavingDate)='" + DateTime.Now.Date.ToString("yyyy") + "' or mas_employee.LeavingDate is null) order by Name");
                //var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                return Json(new { EmployeeName = EmployeeName, BranchName= BranchName }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetEmployee
        [HttpGet]
        public ActionResult GetEmployee(int? CmpId, int? UnitBranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (UnitBranchId != null)
                {
                    //DynamicParameters param = new DynamicParameters();
                    //param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Mas_Employee.Deactivate=0 and Mas_Employee.EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where Atten_InOut.deactivate=0 and Atten_InOut.InOutBranchId=" + UnitBranchId + " and month( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("MM") + "' and year( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("yyyy") + "' and EmployeeId<>1) order by Name");
                    //var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                    DynamicParameters paramEmployee = new DynamicParameters();
                    paramEmployee.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0  and EmployeeBranchId=" + UnitBranchId + " order by EmployeeName");
                    var EmployeeName = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", paramEmployee).ToList();
                    return Json(new { EmployeeName = EmployeeName}, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, int? AllEmployeeId, int? DepartmentId, int? DesignationId, DateTime Month)
        {
            try
            {
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("TravelClaim Report");
                worksheet.Range(1, 1, 1, 21).Merge();
                worksheet.SheetView.FreezeRows(2); // Freeze the second row

                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();
                var Query = "";

                DynamicParameters paramList = new DynamicParameters();
                // paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                paramList.Add("@p_CompanyId", CmpId);
                paramList.Add("@p_BranchId", BranchId);
                paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                paramList.Add("@p_AllEmployeeId", AllEmployeeId);
                paramList.Add("@p_Month", Month);
                data = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Claim_TravelClaim", paramList).ToList();

                if (data.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }

                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(data);

                // Insert data into worksheet
                worksheet.Cell(2, 1).InsertTable(dt, false);

                // Handle all *_Link columns dynamically
                foreach (DataColumn col in dt.Columns)
                {
                    if (!col.ColumnName.EndsWith("_Link"))
                        continue;

                    int colIndex = col.Ordinal + 1;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var cell = worksheet.Cell(i + 3, colIndex);
                        string link = cell.GetString();

                        if (string.IsNullOrWhiteSpace(link))
                        {
                            cell.Value = "";
                            continue;
                        }
                        if (link.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                          link.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                        {
                            cell.Hyperlink = new XLHyperlink(link);
                            cell.Value = "Open";
                            cell.Style.Font.FontColor = XLColor.Blue;
                            cell.Style.Font.Underline = XLFontUnderlineValues.Single;
                            continue;
                        }
                        string fullPath = HostingEnvironment.MapPath(link);
                        if (System.IO.File.Exists(fullPath))
                        {
                            cell.Hyperlink = new XLHyperlink(link);
                            cell.Value = "Open";
                            cell.Style.Font.FontColor = XLColor.Blue;
                            cell.Style.Font.Underline = XLFontUnderlineValues.Single;
                        }
                        else
                        {
                            cell.Value = "";
                        }
                    }
                }

                //        string fullPath = link;
                //        if (!System.IO.Path.IsPathRooted(fullPath))
                //        {
                //            fullPath = System.Web.Hosting.HostingEnvironment.MapPath(link);
                //        }
                //        if (System.IO.File.Exists(fullPath))
                //        {
                //            cell.Hyperlink = new XLHyperlink(link);
                //            cell.Value = "Open";
                //            cell.Style.Font.FontColor = XLColor.Blue;
                //            cell.Style.Font.Underline = XLFontUnderlineValues.Single;
                //        }
                //        else
                //        {
                //            cell.Value = ""; 
                //        }
                //    }
                //}

                // Apply styling
                var totalRows = worksheet.RowsUsed().Count();
                var usedRange = worksheet.RangeUsed();
                usedRange.Style.Fill.BackgroundColor = XLColor.White;
                usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Font.FontSize = 10;
                usedRange.Style.Font.FontColor = XLColor.Black;

                // Header row
                worksheet.Cell(1, 1).Value = $"Travel Claim Report - ({Month:MMM/yyyy})";
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(1, 1).Style.Font.Bold = true;

                // Column header styling
                var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                headerRange.Style.Font.FontSize = 10;
                headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
                headerRange.Style.Font.Bold = true;

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TravelClaimReport.xlsx");
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        //#region DownloadExcelFile
        //public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, int? EmployeeId, int? DepartmentId, int? DesignationId, DateTime FromDate, DateTime ToDate)
        //{
        //    try
        //    {
        //        var workbook = new XLWorkbook();
        //        var worksheet = workbook.Worksheets.Add("TravelClaim Report");
        //        worksheet.Range(1, 1, 1, 21).Merge();
        //        worksheet.SheetView.FreezeRows(2); // Freeze the row
        //        DataTable dt = new DataTable();
        //        List<dynamic> data = new List<dynamic>();
        //        var Query = "";
        //        if (CmpId != null)
        //        {
        //            // Filter by CmpId from Claim_Travel table
        //            Query = " AND Claim_Travel.CmpID = " + CmpId;

        //            if (BranchId != null)
        //            {
        //                Query += " AND Mas_Branch.BranchId = " + BranchId;
        //            }
        //            else
        //            {
        //                Query += " AND Mas_Branch.BranchId IN ( " +
        //                         " SELECT UserBranchMapping.BranchID " +
        //                         " FROM UserBranchMapping " +
        //                         " WHERE UserBranchMapping.EmployeeId = " + Session["EmployeeId"] +
        //                         " AND UserBranchMapping.CmpID = " + CmpId +
        //                         " AND UserBranchMapping.IsActive = 1 " +
        //                         " UNION " +
        //                         " SELECT EmployeeBranchId " +
        //                         " FROM Mas_Employee " +
        //                         " WHERE EmployeeId = " + Session["EmployeeId"] +
        //                         " AND Mas_Employee.CmpID = " + CmpId +
        //                         " )";
        //            }
        //            if (EmployeeId != null)
        //            {
        //                Query += " AND Claim_Travel.TravelClaimEmployeeId = " + EmployeeId;
        //            }

        //            // Add date range condition
        //            Query += " AND Claim_Travel.FromDate BETWEEN '" + FromDate.ToString("yyyy-MM-dd") +
        //                     "' AND '" + ToDate.ToString("yyyy-MM-dd") + "'";
        //        }

        //        DynamicParameters paramList = new DynamicParameters();
        //        paramList.Add("@P_Qry", Query);
        //        data = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Claim_TravelClaim", paramList).ToList();
        //        if (data.Count == 0)
        //        {
        //            byte[] emptyFileContents = new byte[0];
        //            return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
        //        }

        //        DapperORM dprObj = new DapperORM();
        //        dt = dprObj.ConvertToDataTable(data);
        //        //dt.Rows.RemoveAt(0);
        //        worksheet.Cell(2, 1).InsertTable(dt, false);

        //        //  ADD THIS BLOCK — MAKES TotalKMPath_Link CLICKABLE

        //        //if (dt.Columns.Contains("TotalKMPath_Link"))
        //        //{
        //        //    int linkColumnIndex = dt.Columns["TotalKMPath_Link"].Ordinal + 1;

        //        //    for (int i = 0; i < dt.Rows.Count; i++)
        //        //    {
        //        //        var cell = worksheet.Cell(i + 3, linkColumnIndex); 
        //        //        string link = cell.GetString();

        //        //        if (string.IsNullOrWhiteSpace(link))
        //        //        {
        //        //            cell.Value = "";
        //        //            continue;
        //        //        }

        //        //        string fullPath = link;
        //        //        if (!fullPath.Contains(":"))
        //        //        {
        //        //            fullPath = System.Web.Hosting.HostingEnvironment.MapPath(link);
        //        //        }

        //        //        if (!System.IO.File.Exists(fullPath))
        //        //        {
        //        //            cell.Value = "";
        //        //            continue;
        //        //        }

        //        //        cell.Hyperlink = new XLHyperlink(link);
        //        //        cell.Value = "Open";
        //        //        cell.Style.Font.FontColor = XLColor.Blue;
        //        //        cell.Style.Font.Underline = XLFontUnderlineValues.Single;
        //        //    }
        //        //}

        //        List<string> filePathColumns = new List<string>
        //          {
        //                "TotalKMPath_Link",
        //                "TransportPath_Link",
        //                "FoodPath_Link",
        //                 "HotelPath_Link",
        //                 "ConveyancePath_Link",
        //                 "OtherAPath_Link",
        //                 "OtherBPath_Link"
        //           };
        //        foreach (var colName in filePathColumns)
        //        {
        //            if (!dt.Columns.Contains(colName))
        //                continue;

        //            int colIndex = dt.Columns[colName].Ordinal + 1; // Excel index

        //            for (int i = 0; i < dt.Rows.Count; i++)
        //            {
        //                var cell = worksheet.Cell(i + 3, colIndex); 
        //                string link = cell.GetString();
        //                if (string.IsNullOrWhiteSpace(link))
        //                {
        //                    cell.Value = "";
        //                    continue;
        //                }
        //                string fullPath = link;
        //                if (!fullPath.Contains(":")) 
        //                    fullPath = System.Web.Hosting.HostingEnvironment.MapPath(link);
        //                if (!System.IO.File.Exists(fullPath))
        //                {
        //                    cell.Value = "";
        //                    continue;
        //                }
        //                cell.Hyperlink = new XLHyperlink(link);
        //                cell.Value = "Open";
        //                cell.Style.Font.FontColor = XLColor.Blue;
        //                cell.Style.Font.Underline = XLFontUnderlineValues.Single;
        //            }
        //        }

        //        // END FIX 


        //        int totalRows = worksheet.RowsUsed().Count();

        //        // Set the background color to white and apply borders
        //        var usedRange = worksheet.RangeUsed();
        //        usedRange.Style.Fill.BackgroundColor = XLColor.White;
        //        usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        //        usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        //        usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        //        usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
        //        usedRange.Style.Font.FontSize = 10;
        //        usedRange.Style.Font.FontColor = XLColor.Black;

        //        // Set the header row name
        //        worksheet.Cell(1, 1).Value = "Travel Claim Report - (" + FromDate.ToString("dd/MMM/yyyy") + " - " + ToDate.ToString("dd/MMM/yyyy") + ")";
        //        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        //        worksheet.Cell(1, 1).Style.Font.Bold = true;

        //        // Set the header row background color to grey and font color to black
        //        var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
        //        headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
        //        headerRange.Style.Font.FontSize = 10;
        //        headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
        //        headerRange.Style.Font.Bold = true;

        //        worksheet.Columns().AdjustToContents();

        //        using (var stream = new MemoryStream())
        //        {
        //            workbook.SaveAs(stream);
        //            stream.Position = 0; // Reset the stream position to the beginning
        //            // Return the file to the client
        //            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TravelClaimReport.xlsx");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
        //}
        //#endregion
    }
}