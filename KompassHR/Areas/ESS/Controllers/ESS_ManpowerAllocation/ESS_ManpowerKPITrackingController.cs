using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_ManpowerAllocation
{
    public class ESS_ManpowerKPITrackingController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_ManpowerKPITracking
        #region MainView
        public ActionResult ESS_ManpowerKPITracking()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 781;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                ViewBag.GetBranchName = "";
                ViewBag.List =null;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

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

                param.Add("@query", "Select BranchID as Id, BranchName As Name  from Mas_Branch where Deactivate=0 and CmpId='" + CmpId + "' Order By BranchName");
                var BranchName = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param).ToList();

                return Json(new { BranchName }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
        public ActionResult GetList(int? CmpId, int? InOutBranchId, DateTime? InOutDate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 781;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                ViewBag.GetBranchName = "";
                ViewBag.GetEmployeeList = "";
                ViewBag.List = "";

                if (CmpId != null)
                {
                    ViewBag.InOutDate = InOutDate;

                    var Month = InOutDate.Value.Month;
                    var Year = InOutDate.Value.Year;

                    param.Add("@p_CmpId", CmpId);
                    param.Add("@p_BranchId", InOutBranchId);
                    param.Add("@p_Month", Month);
                    param.Add("@p_Year", Year);
                    var List = DapperORM.DynamicList("sp_Manpower_KPI", param);
                    ViewBag.List = List;
                
                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@p_CmpId", CmpId);
                    param2.Add("@p_BranchId", InOutBranchId);
                    param2.Add("@p_Month", Month);
                    param2.Add("@p_Year", Year);
                    var KPIPlant = DapperORM.DynamicList("sp_Manpower_KPIPlant", param2);
                    ViewBag.KPIPlantList = KPIPlant;

                   

                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@query", "Select BranchID as Id, BranchName As Name  from Mas_Branch where Deactivate=0 and CmpId='" + CmpId + "' Order By BranchName");
                    var BranchName = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param1).ToList();
                    ViewBag.GetBranchName = BranchName;
                }
                return View("ESS_ManpowerKPITracking");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region DownloadExcel

        public class GetBranch
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private List<GetBranch> GetBranchIds(int companyId)
        {
            var param = new DynamicParameters();
            param.Add("@query", "Select BranchID as Id,BranchName as Name from Mas_Branch where Deactivate=0 and CmpId='" + companyId + "'");
            var branchIds = DapperORM.DynamicList("sp_QueryExcution", param);
            var result = new List<GetBranch>();

            if (branchIds == null)
            {
                Console.WriteLine($"No branch IDs found for companyId: {companyId}");
                return result;
            }

            foreach (var b in branchIds)
            {
                if (b.Id != null)
                {
                    try
                    {
                        Console.WriteLine($"Processing branch data: Id={b.Id} (Type: {b.Id?.GetType()?.Name ?? "null"}), Name={b.Name}");
                        result.Add(new GetBranch
                        {
                            Id = (int)b.Id, // Explicit cast to int
                            Name = b.Name?.ToString() ?? "Unknown"
                        });
                    }
                    catch (InvalidCastException ex)
                    {
                        Console.WriteLine($"Invalid BranchID: {b.Id} (Type: {b.Id?.GetType()?.Name ?? "null"}, Error: {ex.Message})");
                    }
                }
                else
                {
                    Console.WriteLine("BranchID is null");
                }
            }
            return result;
        }

        public ActionResult DownloadExcel(int companyId, string branchId, DateTime month)
        {
            var debugInfo = new List<string>();

            var workbook = new XLWorkbook();
            List<dynamic> allPlantData = new List<dynamic>();
            List<GetBranch> branches;

            bool isAllBranches = string.IsNullOrEmpty(branchId);

            if (isAllBranches)
            {
                branches = GetBranchIds(companyId);
            }
            else
            {
                try
                {
                    var param = new DynamicParameters();
                    param.Add("@query", "SELECT CAST(BranchID AS INT) AS Id, BranchName AS Name FROM Mas_Branch WHERE Deactivate = 0 AND CmpId = '" + companyId + "' AND BranchID = '" + branchId + "'");

                    var branchData = DapperORM.DynamicList("sp_QueryExcution", param);

                    branches = new List<GetBranch>();
                    if (branchData != null)
                    {
                        try
                        {
                            foreach (var item in branchData)
                            {
                                branches.Add(new GetBranch
                                {
                                    Id = (int)item.Id,
                                    Name = item.Name?.ToString() ?? "Unknown"
                                });
                            }

                        }
                        catch (InvalidCastException ex)
                        {
                            // Log the exception if needed
                            // e.g., Console.WriteLine($"Error casting branch data: {ex.Message}");
                        }
                    }
                    if (!branches.Any())
                    {
                        branches = new List<GetBranch> { new GetBranch { Id = int.Parse(branchId), Name = "Unknown" } };
                    }
                }
                catch (Exception ex)
                {
                    branches = new List<GetBranch> { new GetBranch { Id = int.Parse(branchId), Name = "Unknown" } };
                }
            }

            var kpiSheetDataPerBranch = new Dictionary<int, List<dynamic>>();
            foreach (var branch in branches)
            {

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_CmpId", companyId);
                param.Add("@p_BranchId", branch.Id);
                param.Add("@p_Month", month.Month);
                param.Add("@p_Year", month.Year);

                try
                {
                    var kpiPlant = DapperORM.DynamicList("sp_Manpower_KPIPlant", param);
                    allPlantData.AddRange(kpiPlant as IEnumerable<dynamic> ?? new List<dynamic>());
                }
                catch (Exception ex)
                {
                    allPlantData.AddRange(new List<dynamic>());
                }

                param = new DynamicParameters();
                param.Add("@p_CmpId", companyId);
                param.Add("@p_BranchId", branch.Id);
                param.Add("@p_Month", month.Month);
                param.Add("@p_Year", month.Year);

                try
                {
                    var sheetDataObj = DapperORM.DynamicList("sp_Manpower_KPI", param);
                    var sheetData = sheetDataObj as IEnumerable<dynamic>;
                    if (sheetData != null && sheetData.Any())
                    {
                        var firstRow = sheetData.FirstOrDefault();
                        if (firstRow != null)
                        {
                            var properties = ((IDictionary<string, object>)firstRow)
                                .Select(kvp => $"{kvp.Key}: {kvp.Value ?? "null"}")
                                .ToList();
                        }
                    }
                    else
                    {
                    }
                    kpiSheetDataPerBranch[branch.Id] = sheetData?.ToList() ?? new List<dynamic>();
                }
                catch (Exception ex)
                {
                    kpiSheetDataPerBranch[branch.Id] = new List<dynamic>();
                }
            }

            CreateAllPlantsSheet(workbook, allPlantData, month);
            foreach (var branch in branches)
            {
                var data = kpiSheetDataPerBranch[branch.Id];
                var firstItem = data.FirstOrDefault();
                var sheetName = SanitizeSheetName(branch.Name);
                CreateKPISheet(workbook, data, month, sheetName, firstItem);
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Manpower_KPI_Report.xlsx");
            }
        }

        private void CreateAllPlantsSheet(XLWorkbook workbook, List<dynamic> data, DateTime month)
        {
            if (!data.Any()) return;
            string monthStr = month.ToString("MMM''yy").ToUpper();

            var ws = workbook.Worksheets.Add($"MANPOWER KPI SUMMARY -{monthStr}");

            int row = 1;

            ws.Cell(row, 1).Value = $"MANPOWER KPI SUMMARY - {monthStr}";
            ws.Range(row, 1, row, 6).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center).Font.SetBold();

            row++;
            ws.Cell(row, 1).Value = data.First().CompanyName;
            ws.Range(row, 1, row, 6).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center).Font.SetBold();

            row++;
            var groups = data.GroupBy(x => new { x.BranchName, x.UnitName });

            foreach (var unitGroup in groups)
            {
                ws.Cell(row, 1).Value = $"{unitGroup.Key.BranchName} - {unitGroup.Key.UnitName}";
                ws.Range(row, 1, row, 6).Merge().Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.FromHtml("#e9ecef"));
                row++;

                ws.Cell(row, 1).Value = "";
                ws.Cell(row, 2).Value = "Std. Mandays V/s. Actual";
                ws.Cell(row, 3).Value = $"Target {monthStr}";
                ws.Cell(row, 4).Value = $"Actual {monthStr}";
                ws.Cell(row, 5).Value = "Variance/Day";
                ws.Cell(row, 6).Value = "Variance %";
                ws.Range(row, 1, row, 6).Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.White);
                row++;

                foreach (var item in unitGroup)
                {
                    ws.Cell(row, 1).Value = item.Sr;
                    ws.Cell(row, 2).Value = item.Remark;
                    ws.Cell(row, 3).Value = item.TargetValue;
                    ws.Cell(row, 4).Value = item.ActualValue;
                    ws.Cell(row, 5).Value = item.VarianceDay;
                    ws.Cell(row, 6).Value = item.Variance;
                    row++;
                }

                ws.Cell(row, 1).Value = "Note :- Above manpower includes both Production, Supporting & Operating cell manpower.";
                ws.Range(row, 1, row, 6).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left).Font.SetFontSize(10);
                row++;
            }

            ws.Column(1).Width = 10;
            ws.Column(2).Width = 40;
            ws.Columns(3, 6).Width = 20;
            ws.Range(1, 1, row - 1, 6).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border.SetInsideBorder(XLBorderStyleValues.Thin);
        }

        private void CreateKPISheet(XLWorkbook workbook, List<dynamic> data, DateTime month, string sheetName, dynamic firstRow)
        {
            if (!data.Any() || firstRow == null) return;

            var ws = workbook.Worksheets.Add(sheetName);
            int row = 1, col = 1;

            // Header Row 1
            ws.Cell(row, col).Value = "MP CATEGORY\n(PLEASE REFER MP CATEGORY SHEET FOR MP SUB CATEGORY)";
            ws.Range(row, col, row + 3, col).Merge().Style
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                .Font.SetFontSize(8);
            col++;

            ws.Cell(row, col).Value = "R1";
            ws.Range(row, col, row, col + 1).Merge();
            col += 2;

            ws.Cell(row, col).Value = "R2";
            ws.Range(row, col, row, col + 1).Merge();
            col += 2;

            ws.Cell(row, col).Value = "R3";
            ws.Range(row, col, row, col + 1).Merge();
            col += 2;

            ws.Cell(row, col).Value = "AVERAGE OF R1, R2 & R3";
            ws.Range(row, col, row, col + 1).Merge();
            col += 2;

            ws.Cell(row, col).Value = "AVERAGE MANPOWER USED PER DAY";
            ws.Range(row, col, row + 2, col + 1).Merge();
            col += 2;

            ws.Cell(row, col).Value = "GAP MANPOWER ACTUAL VS AVERAGE OF R1,R2,R3";
            ws.Range(row, col, row + 2, col + 2).Merge();
            col += 3;

            ws.Cell(row, col).Value = "MANPOWER AGAINST DISPATCH QTY.";
            ws.Range(row, col, row + 2, col + 1).Merge();
            col += 2;

            ws.Cell(row, col).Value = "GAP MANPOWER ACTUAL VS AGAINST DISPATCH QTY.";
            ws.Range(row, col, row + 2, col + 2).Merge();
            col += 3;

            ws.Cell(row, col).Value = "DATEWISE MANDAYS";
            ws.Range(row, col, row + 2, col + 61).Merge();
            col += 62;

            ws.Cell(row, col).Value = "TOTAL";
            ws.Range(row, col, row + 3, col + 1).Merge();
            col += 2;

            ws.Cell(row, col).Value = "AVG";
            ws.Range(row, col, row + 3, col + 1).Merge();

            // Row 2
            row++;
            col = 2;
            ws.Cell(row, col).Value = "FROM"; col++;
            ws.Cell(row, col).Value = "TO"; col++;
            ws.Cell(row, col).Value = "FROM"; col++;
            ws.Cell(row, col).Value = "TO"; col++;
            ws.Cell(row, col).Value = "FROM"; col++;
            ws.Cell(row, col).Value = "TO"; col++;
            ws.Cell(row, col).Value = "FROM"; col++;
            ws.Cell(row, col).Value = "TO"; col += 2;
            col += 3;
            col += 2;
            col += 3;
            col += 62;

            // Row 3
            row++;
            col = 2;
            ws.Cell(row, col).Value = firstRow.R1_FromDate?.ToString("dd/MM/yy") ?? "-"; col++;
            ws.Cell(row, col).Value = firstRow.R1_ToDate?.ToString("dd/MM/yy") ?? "-"; col++;
            ws.Cell(row, col).Value = firstRow.R2_FromDate?.ToString("dd/MM/yy") ?? "-"; col++;
            ws.Cell(row, col).Value = firstRow.R2_ToDate?.ToString("dd/MM/yy") ?? "-"; col++;
            ws.Cell(row, col).Value = firstRow.R3_FromDate?.ToString("dd/MM/yy") ?? "-"; col++;
            ws.Cell(row, col).Value = firstRow.R3_ToDate?.ToString("dd/MM/yy") ?? "-"; col++;
            ws.Cell(row, col).Value = firstRow.AVERAGE_OF_R_FromDate?.ToString("dd/MM/yy") ?? "-"; col++;
            ws.Cell(row, col).Value = firstRow.AVERAGE_OF_R_ToDate?.ToString("dd/MM/yy") ?? "-"; col += 2;
            col += 3;
            col += 2;
            col += 3;
            col += 62;

            // Row 4
            row++;
            col = 1;
            ws.Cell(row, col).Value = "";
            col++;
            ws.Cell(row, col).Value = "IED-HC (12 HRS.)"; col++;
            ws.Cell(row, col).Value = "IED-MD (8 HRS.)"; col++;
            ws.Cell(row, col).Value = "IED-HC (12 HRS.)"; col++;
            ws.Cell(row, col).Value = "IED-MD (8 HRS.)"; col++;
            ws.Cell(row, col).Value = "IED-HC (12 HRS.)"; col++;
            ws.Cell(row, col).Value = "IED-MD (8 HRS.)"; col++;
            ws.Cell(row, col).Value = "IED-HC (12 HRS.)"; col++;
            ws.Cell(row, col).Value = "IED-MD (8 HRS.)"; col++;
            ws.Cell(row, col).Value = "PLANT HC (12 HRS.)"; col++;
            ws.Cell(row, col).Value = "PLANT MD (8 HRS.)"; col++;
            ws.Cell(row, col).Value = "GAP HC (12 HRS.)"; col++;
            ws.Cell(row, col).Value = "GAP MD (8 HRS.)"; col++;
            ws.Cell(row, col).Value = "GAP % (EXCESS MD)"; col++;
            ws.Cell(row, col).Value = "IED-HC (12 HRS.)"; col++;
            ws.Cell(row, col).Value = "IED-MD (8 HRS.)"; col++;
            ws.Cell(row, col).Value = "GAP HC (12 HRS.)"; col++;
            ws.Cell(row, col).Value = "GAP MD (8 HRS.)"; col++;
            ws.Cell(row, col).Value = "GAP % (EXCESS MD)"; col++;
            for (int day = 1; day <= 31; day++)
            {
                ws.Cell(row, col).Value = day.ToString();
                ws.Range(row, col, row, col + 1).Merge();
                col += 2;
            }
            ws.Cell(row, col).Value = "HC (12 HRS.)"; ws.Cell(row, col).Style.Font.SetBold(); col++;
            ws.Cell(row, col).Value = "MD (8 HRS.)"; ws.Cell(row, col).Style.Font.SetBold(); col++;
            ws.Cell(row, col).Value = "HC (12 HRS.)"; ws.Cell(row, col).Style.Font.SetBold(); col++;
            ws.Cell(row, col).Value = "MD (8 HRS.)"; ws.Cell(row, col).Style.Font.SetBold();

            // Row 5
            row++;
            col = 1;
            ws.Cell(row, col).Value = "HC= HEADCOUNT, MD= MANDAYS"; ws.Cell(row, col).Style.Font.SetBold(); col++;
            ws.Range(row, col, row, col + 1).Merge().Value = "A"; ws.Cell(row, col).Style.Font.SetBold(); col += 2;
            ws.Range(row, col, row, col + 1).Merge().Value = "B"; ws.Cell(row, col).Style.Font.SetBold(); col += 2;
            ws.Range(row, col, row, col + 1).Merge().Value = "C"; ws.Cell(row, col).Style.Font.SetBold(); col += 2;
            ws.Range(row, col, row, col + 1).Merge().Value = "D= AVERAGE(A,B,C)"; ws.Cell(row, col).Style.Font.SetBold(); col += 2;
            ws.Range(row, col, row, col + 1).Merge().Value = "E"; ws.Cell(row, col).Style.Font.SetBold(); col += 2;
            ws.Range(row, col, row, col + 1).Merge().Value = "F=E-D"; ws.Cell(row, col).Style.Font.SetBold(); col += 2;
            ws.Cell(row, col).Value = "(F/D)*100"; ws.Cell(row, col).Style.Font.SetBold(); col++;
            ws.Range(row, col, row, col + 1).Merge().Value = "G"; ws.Cell(row, col).Style.Font.SetBold(); col += 2;
            ws.Range(row, col, row, col + 1).Merge().Value = "H=E-G"; ws.Cell(row, col).Style.Font.SetBold(); col += 2;
            ws.Cell(row, col).Value = "(H/G)*100"; ws.Cell(row, col).Style.Font.SetBold(); col++;
            for (int day = 1; day <= 31; day++)
            {
                ws.Cell(row, col).Value = "HC (12 HRS.)"; ws.Cell(row, col).Style.Font.SetBold(); col++;
                ws.Cell(row, col).Value = "MD (8 HRS.)"; ws.Cell(row, col).Style.Font.SetBold(); col++;
            }
            ws.Cell(row, col).Value = "HC (12 HRS.)"; ws.Cell(row, col).Style.Font.SetBold(); col++;
            ws.Cell(row, col).Value = "MD (8 HRS.)"; ws.Cell(row, col).Style.Font.SetBold(); col++;
            ws.Cell(row, col).Value = "HC (12 HRS.)"; ws.Cell(row, col).Style.Font.SetBold(); col++;
            ws.Cell(row, col).Value = "MD (8 HRS.)"; ws.Cell(row, col).Style.Font.SetBold();

            // Apply header styles
            ws.Range(1, 1, row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                .Font.SetBold()
                .Alignment.SetWrapText();

            // Body
            var groups = data.GroupBy(x => x.LineName ?? "Unknown");
            foreach (var group in groups)
            {
                row++;
                ws.Cell(row, 1).Value = group.Key;
                ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left).Font.SetBold();
                ws.Range(row, 1, row, col).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#cfe2ff"));
                int bodyCol = 2;
                ws.Cell(row, bodyCol++).Value = "Schedule/Day";
                ws.Cell(row, bodyCol++).Value = "322";
                ws.Cell(row, bodyCol++).Value = "Schedule/Day";
                ws.Cell(row, bodyCol++).Value = "322";
                ws.Cell(row, bodyCol++).Value = "Schedule/Day";
                ws.Cell(row, bodyCol++).Value = "322";
                ws.Cell(row, bodyCol++).Value = "Schedule/Day";
                ws.Cell(row, bodyCol++).Value = "322";
                for (int i = bodyCol; i <= col; i++)
                {
                    ws.Cell(row, i).Value = "";
                }
                row++;

                foreach (var item in group)
                {
                    int itemCol = 1;
                    ws.Cell(row, itemCol++).Value = item.ManpowerName ?? "-";
                    ws.Cell(row, itemCol - 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                    ws.Cell(row, itemCol++).Value = item.R1_IED_HC ?? 0;
                    ws.Cell(row, itemCol++).Value = item.R1_IED_MD ?? 0;
                    ws.Cell(row, itemCol++).Value = item.R2_IED_HC ?? 0;
                    ws.Cell(row, itemCol++).Value = item.R2_IED_MD ?? 0;
                    ws.Cell(row, itemCol++).Value = item.R3_IED_HC ?? 0;
                    ws.Cell(row, itemCol++).Value = item.R3_IED_MD ?? 0;
                    ws.Cell(row, itemCol++).Value = item.AVERAGE_OF_R1R2R3_HC ?? 0;
                    ws.Cell(row, itemCol++).Value = item.AVERAGE_OF_R1R2R3_MD ?? 0;
                    ws.Cell(row, itemCol++).Value = item.AVERAGE_MANPOWER_USED_PER_DAY_HC ?? 0;
                    ws.Cell(row, itemCol++).Value = item.AVERAGE_MANPOWER_USED_PER_DAY_MD ?? 0;
                    ws.Cell(row, itemCol++).Value = item.GAP_PLANT_HC ?? 0;
                    ws.Cell(row, itemCol++).Value = item.GAP_PLANT_MD ?? 0;
                    ws.Cell(row, itemCol++).Value = item.GAP_EXCESS_MD ?? 0;
                    ws.Cell(row, itemCol++).Value = item.MANPOWER_AGAINST_DISPATCH_QTY_HC ?? 0;
                    ws.Cell(row, itemCol++).Value = item.MANPOWER_AGAINST_DISPATCH_QTY_MD ?? 0;
                    ws.Cell(row, itemCol++).Value = item.GAP_DISPATCH_QTY_HC ?? 0;
                    ws.Cell(row, itemCol++).Value = item.GAP_DISPATCH_QTY_MD ?? 0;
                    ws.Cell(row, itemCol++).Value = item.GAP_DISPATCH_QTY_EXCESS_MD ?? 0;
                    var itemDict = item as IDictionary<string, object>;
                    for (int day = 1; day <= 31; day++)
                    {
                        string hcKey = $"A{day}HC";
                        string mdKey = $"A{day}MD";
                        object hcValue = itemDict.ContainsKey(hcKey) ? itemDict[hcKey] ?? 0 : 0;
                        object mdValue = itemDict.ContainsKey(mdKey) ? itemDict[mdKey] ?? 0 : 0;
                        ws.Cell(row, itemCol++).Value = hcValue;
                        ws.Cell(row, itemCol++).Value = mdValue;
                    }
                    ws.Cell(row, itemCol++).Value = item.TotalHC ?? 0;
                    ws.Cell(row, itemCol++).Value = item.TotalMD ?? 0;
                    ws.Cell(row, itemCol++).Value = item.AVGHC ?? 0;
                    ws.Cell(row, itemCol++).Value = item.AVGMD ?? 0;
                    row++;
                }

                // Total row
                ws.Cell(row, 1).Value = "TOTAL";
                ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left).Font.SetBold();
                int totalCol = 2;
                ws.Cell(row, totalCol++).Value = group.Sum(x => (double?)x.R1_IED_HC ?? 0);
                ws.Cell(row, totalCol++).Value = group.Sum(x => (double?)x.R1_IED_MD ?? 0);
                ws.Cell(row, totalCol++).Value = group.Sum(x => (double?)x.R2_IED_HC ?? 0);
                ws.Cell(row, totalCol++).Value = group.Sum(x => (double?)x.R2_IED_MD ?? 0);
                ws.Cell(row, totalCol++).Value = group.Sum(x => (double?)x.R3_IED_HC ?? 0);
                ws.Cell(row, totalCol++).Value = group.Sum(x => (double?)x.R3_IED_MD ?? 0);
                ws.Cell(row, totalCol++).Value = group.Sum(x => (double?)x.AVERAGE_OF_R1R2R3_HC ?? 0);
                ws.Cell(row, totalCol++).Value = group.Sum(x => (double?)x.AVERAGE_OF_R1R2R3_MD ?? 0);
                ws.Cell(row, totalCol++).Value = group.Sum(x => (double?)x.AVERAGE_MANPOWER_USED_PER_DAY_HC ?? 0);
                ws.Cell(row, totalCol++).Value = group.Sum(x => (double?)x.AVERAGE_MANPOWER_USED_PER_DAY_MD ?? 0);
                ws.Cell(row, totalCol++).Value = group.Sum(x => (double?)x.GAP_PLANT_HC ?? 0);
                ws.Cell(row, totalCol++).Value = group.Sum(x => (double?)x.GAP_PLANT_MD ?? 0);
                ws.Cell(row, totalCol++).Value = group.Sum(x => (double?)x.GAP_EXCESS_MD ?? 0);
                ws.Cell(row, totalCol++).Value = 0;
                ws.Cell(row, totalCol++).Value = 0;
                ws.Cell(row, totalCol++).Value = 0;
                ws.Cell(row, totalCol++).Value = 0;
                ws.Cell(row, totalCol++).Value = 0;
                for (int day = 1; day <= 31; day++)
                {
                    string hcKey = $"A{day}HC";
                    string mdKey = $"A{day}MD";
                    ws.Cell(row, totalCol++).Value = group.Sum(x => ((IDictionary<string, object>)x).ContainsKey(hcKey) ? (double?)((IDictionary<string, object>)x)[hcKey] ?? 0 : 0);
                    ws.Cell(row, totalCol++).Value = group.Sum(x => ((IDictionary<string, object>)x).ContainsKey(mdKey) ? (double?)((IDictionary<string, object>)x)[mdKey] ?? 0 : 0);
                }
                ws.Cell(row, totalCol++).Value = group.Sum(x => (double?)x.TotalHC ?? 0);
                ws.Cell(row, totalCol++).Value = group.Sum(x => (double?)x.TotalMD ?? 0);
                ws.Cell(row, totalCol++).Value = group.Sum(x => (double?)x.AVGHC ?? 0);
                ws.Cell(row, totalCol++).Value = group.Sum(x => (double?)x.AVGMD ?? 0);
                row++;
            }

            ws.Column(1).Width = 20;
            for (int c = 2; c <= col; c++)
            {
                ws.Column(c).Width = 10;
            }
            ws.Range(1, 1, row - 1, col).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border.SetInsideBorder(XLBorderStyleValues.Thin);
        }

        private string SanitizeSheetName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "Sheet";

            var invalidChars = new[] { '*', '?', ':', '/', '\\', '[', ']' };
            foreach (var c in invalidChars)
            {
                name = name.Replace(c.ToString(), "");
            }

            if (name.Length > 31)
                name = name.Substring(0, 31);

            return string.IsNullOrEmpty(name) ? "Sheet" : name;
        }
        #endregion
    }
}