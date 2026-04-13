using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;


namespace KompassHR.Areas.Reports.Controllers.Reports_Attendance
{
    public class Report_Contractor_Invoice_RptController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        // GET: Reports/Report_Contractor_Invoice_Rpt
        #region Report_Contractor_Invoice_Rpt
        public ActionResult Report_Contractor_Invoice_Rpt(DailyAttendanceReportFilter MasReport)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 789;
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
                var CMPID = GetComapnyName[0].Id;

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CMPID);
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                var BranchID = Branch[0].Id;
                ViewBag.GetBranchName = Branch;

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + BranchID + "");
                var GetContractorDropdown = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param2).ToList();
                ViewBag.GetContractor = GetContractorDropdown;
                return View(MasReport);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region Get BusinessUnit
        [HttpGet]
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetContractor
        [HttpGet]
        public ActionResult GetContractor(int? BranchId)
        {
            try
            {
                if (BranchId == null)
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + BranchId + "");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", param).ToList();
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

        //#region DownloadExcelFile
        //public ActionResult DownloadExcelFile(int? BranchId, DateTime FromDate, int? CmpId, int? ContractorId)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { Area = "" });
        //        }

        //        DynamicParameters Branch = new DynamicParameters();
        //        Branch.Add("@p_employeeid", Session["EmployeeId"]);
        //        Branch.Add("@p_CmpId", CmpId);
        //        var allBranches = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", Branch).ToList();

        //        var selectedBranches = BranchId != null
        //            ? allBranches.Where(b => b.Id == BranchId.Value).ToList()
        //            : allBranches;

        //        var workbook = new XLWorkbook();

        //        foreach (var branch in selectedBranches)
        //        {
        //            string branchName = Regex.Replace(branch.Name, @"[\\/\?\*\[\]]", "-");
        //            double branchId = branch.Id;

        //            // Fetch data
        //            DynamicParameters param = new DynamicParameters();
        //            param.Add("@p_MonthYear", FromDate);
        //            param.Add("@p_BranchId", branchId);
        //            param.Add("@p_ContractorId", ContractorId?.ToString() ?? "");

        //            var data = DapperORM.ExecuteSP<dynamic>("SP_Rpt_ConctratorPayrollSummery", param).ToList();
        //            var dprObj = new DapperORM();
        //            DataTable dt = dprObj.ConvertToDataTable(data);

        //            // Skip if no data
        //            if (data.Count <= 1 || dt.Rows.Count <= 1)
        //            {
        //                return new HttpStatusCodeResult(204, "No Data");
        //            }

        //            double sumTotal = 0;
        //            double sumPaidDays = 0;

        //            // Get "Total" from last row of table (already calculated by SP)
        //            if (dt.Columns.Contains("Total") && dt.Rows.Count > 0)
        //            {
        //                var lastRow = dt.Rows[dt.Rows.Count - 1];
        //                if (lastRow["Total"] != DBNull.Value)
        //                {
        //                    sumTotal = Convert.ToDouble(lastRow["Total"]);
        //                }
        //            }

        //            // Calculate PaidDays sum
        //            if (dt.Columns.Contains("PaidDays"))
        //            {
        //                foreach (DataRow row in dt.Rows)
        //                {
        //                    if (row["PaidDays"] != DBNull.Value)
        //                    {
        //                        sumPaidDays += Convert.ToDouble(row["PaidDays"]);
        //                    }
        //                }
        //            }

        //            // Get Contractor Rates
        //            DynamicParameters prContractorWiseRate = new DynamicParameters();
        //            prContractorWiseRate.Add("@query", $"Select ServiceCharges, SupervisorCharges, Percentage, Rate from Manpower_ContractorWiseRateMaster where Deactivate=0 and '{FromDate}' between FromMonth and ToMonth and ContractorId='{ContractorId}' and BranchId='{branchId}'");
        //            var ContractorWiseRateMaster = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", prContractorWiseRate).FirstOrDefault();

        //            // Calculate Service Charges
        //            double serviceChargeAmount = 0;
        //            string serviceChargeDesc = "";
        //            if (ContractorWiseRateMaster.ServiceCharges == "Percentage")
        //            {
        //                serviceChargeAmount = (sumTotal * ContractorWiseRateMaster.Percentage) / 100;
        //                serviceChargeDesc = $"Service Charges (Percentage - {ContractorWiseRateMaster.Percentage}%)";
        //            }
        //            else if (ContractorWiseRateMaster.ServiceCharges == "Perday")
        //            {
        //                serviceChargeAmount = ContractorWiseRateMaster.Rate * sumPaidDays;
        //                serviceChargeDesc = $"Service Charges (Perday - {ContractorWiseRateMaster.Rate})";
        //            }
        //            else
        //            {
        //                serviceChargeAmount = ContractorWiseRateMaster.Rate;
        //                serviceChargeDesc = $"Service Charges (Fixed - {ContractorWiseRateMaster.Rate})";
        //            }

        //            // Supervisor Charges
        //            double supervisorCharges = Convert.ToDouble(ContractorWiseRateMaster.SupervisorCharges);

        //            // Final Invoice Amount
        //            double invoicingAmount = sumTotal + serviceChargeAmount + supervisorCharges;

        //            // Create Worksheet
        //            var worksheet = workbook.Worksheets.Add(branchName);
        //            worksheet.Range(1, 1, 1, 10).Merge();
        //            worksheet.Cell(1, 1).Value = $"Contractor Invoice - ({FromDate:MMM/yyyy}) - {branchName}";
        //            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        //            worksheet.Cell(1, 1).Style.Font.Bold = true;

        //            // Insert DataTable
        //            worksheet.Cell(2, 1).InsertTable(dt, false);
        //            worksheet.SheetView.FreezeRows(2);

        //            // Get last used row
        //            int totalRows = worksheet.LastRowUsed().RowNumber();

        //            // Find column index for "Total"
        //            int totalColumnIndex = dt.Columns.Contains("Total") ? dt.Columns["Total"].Ordinal + 1 : dt.Columns.Count;
        //            int mergeUntil = dt.Columns.Count - 1; // Merge from col 1 to last-1 col

        //            // Append Service Charges row
        //            int serviceRow = totalRows + 1;
        //            var serviceLabelRange = worksheet.Range(serviceRow, 1, serviceRow, mergeUntil);
        //            serviceLabelRange.Merge();
        //            serviceLabelRange.Value = serviceChargeDesc;
        //            serviceLabelRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        //            worksheet.Cell(serviceRow, totalColumnIndex).Value = serviceChargeAmount;

        //            // Append Supervisor Charges row
        //            int supervisorRow = serviceRow + 1;
        //            var supervisorLabelRange = worksheet.Range(supervisorRow, 1, supervisorRow, mergeUntil);
        //            supervisorLabelRange.Merge();
        //            supervisorLabelRange.Value = "Supervisor Charges";
        //            supervisorLabelRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        //            worksheet.Cell(supervisorRow, totalColumnIndex).Value = supervisorCharges;

        //            // Append Invoicing Amount row
        //            int invoiceRow = supervisorRow + 1;
        //            var invoiceLabelRange = worksheet.Range(invoiceRow, 1, invoiceRow, mergeUntil);
        //            invoiceLabelRange.Merge();
        //            invoiceLabelRange.Value = "Invoicing Amount";
        //            invoiceLabelRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        //            worksheet.Cell(invoiceRow, totalColumnIndex).Value = invoicingAmount;

        //            // Style appended rows
        //            for (int row = serviceRow; row <= invoiceRow; row++)
        //            {
        //                worksheet.Range(row, 1, row, dt.Columns.Count).Style.Font.Bold = true;
        //                worksheet.Range(row, 1, row, dt.Columns.Count).Style.Fill.BackgroundColor = XLColor.LightYellow;
        //                worksheet.Range(row, 1, row, dt.Columns.Count).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        //            }


        //            // Style Entire Sheet
        //            var usedRange = worksheet.RangeUsed();
        //            usedRange.Style.Fill.BackgroundColor = XLColor.White;
        //            usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        //            usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        //            usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        //            usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
        //            usedRange.Style.Font.FontSize = 10;
        //            usedRange.Style.Font.FontColor = XLColor.Black;

        //            var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
        //            headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
        //            headerRange.Style.Font.FontSize = 10;
        //            headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
        //            headerRange.Style.Font.Bold = true;

        //            worksheet.Columns().AdjustToContents();
        //        }

        //        using (var stream = new MemoryStream())
        //        {
        //            workbook.SaveAs(stream);
        //            byte[] fileBytes = stream.ToArray();

        //            Response.Clear();
        //            Response.Buffer = true;
        //            Response.SuppressContent = false;
        //            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //            Response.AddHeader("Content-Disposition", $"attachment; filename=ContractorInvoice_{FromDate:yyyyMM}.xlsx");
        //            Response.BinaryWrite(fileBytes);
        //            Response.Flush();
        //            Response.End();
        //        }

        //        return new EmptyResult();
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
        //}
        //#endregion



        //#region DownloadExcelFile
        //public ActionResult DownloadExcelFile(int? BranchId, DateTime FromDate, int? CmpId, int? ContractorId)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { Area = "" });
        //        }

        //        DynamicParameters Branch = new DynamicParameters();
        //        Branch.Add("@p_employeeid", Session["EmployeeId"]);
        //        Branch.Add("@p_CmpId", CmpId);
        //        var allBranches = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", Branch).ToList();

        //        // Filter branch list if a specific BranchId is provided
        //        var selectedBranches = BranchId != null
        //            ? allBranches.Where(b => b.Id == BranchId.Value).ToList()
        //            : allBranches;

        //        var workbook = new XLWorkbook();

        //        // Check if any branches have data
        //        bool hasData = false;

        //        foreach (var branch in selectedBranches)
        //        {
        //            string branchName = Regex.Replace(branch.Name, @"[\\/\?\*\[\]]", "-");
        //            double branchId = branch.Id;
        //            DataTable dt = new DataTable();
        //            List<dynamic> data = new List<dynamic>();

        //            DynamicParameters param = new DynamicParameters();
        //            param.Add("@p_MonthYear", FromDate);
        //            param.Add("@p_BranchId", branchId);
        //            param.Add("@p_ContractorId", ContractorId?.ToString() ?? "");
        //            data = DapperORM.ExecuteSP<dynamic>("SP_Rpt_ConctratorPayrollSummery", param).ToList();

        //            DapperORM dprObj = new DapperORM();
        //            dt = dprObj.ConvertToDataTable(data);

        //            if (data.Count == 1 || dt.Rows.Count == 1)
        //            {
        //                byte[] emptyFileContents = new byte[0];
        //                return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
        //            }

        //            double sumTotal = 0;
        //            double sumPaidDays = 0;
        //            double serviceChargeAmount = 0;
        //            double supervisorCharges = 0;
        //            string serviceChargeType = "";
        //            // Calculate sumTotal from the "Total" column
        //            if (dt.Columns.Contains("Total") && dt.Rows.Count > 0)
        //            {
        //                var lastRow = dt.Rows[dt.Rows.Count - 1];
        //                if (lastRow["Total"] != DBNull.Value)
        //                {
        //                    sumTotal = Convert.ToDouble(lastRow["Total"]);
        //                }
        //            }

        //            // Calculate sumPaidDays
        //            if (dt.Columns.Contains("PaidDays"))
        //            {
        //                for (int i = 0; i < dt.Rows.Count; i++)
        //                {
        //                    if (dt.Rows[i]["PaidDays"] != DBNull.Value)
        //                    {
        //                        sumPaidDays += Convert.ToDouble(dt.Rows[i]["PaidDays"]);
        //                    }
        //                }
        //            }

        //            //var GetDetails = "Select ServiceCharges,Percentage,Rate from Manpower_ContractorWiseRateMaster where Deactivate=0 and '"+ FromDate + "' between FromMonth and ToMonth and ContractorId='"+ ContractorId +"' and BranchId='" + branchId + "';";
        //            //var ContractorWiseRateMaster = DapperORM.DynamicQueryMultiple(GetDetails);

        //            DynamicParameters prContractorWiseRate = new DynamicParameters();
        //            prContractorWiseRate.Add("@query", "Select ServiceCharges,SupervisorCharges,Percentage,Rate from Manpower_ContractorWiseRateMaster where Deactivate=0 and '" + FromDate + "' between FromMonth and ToMonth and ContractorId='" + ContractorId + "' and BranchId='" + branchId + "'");
        //            var ContractorWiseRateMaster = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", prContractorWiseRate).FirstOrDefault();




        //            if (ContractorWiseRateMaster.ServiceCharges == "Percentage")
        //            {
        //                serviceChargeAmount = (sumTotal * ContractorWiseRateMaster.Percentage) / 100;
        //            }
        //            else if (ContractorWiseRateMaster.ServiceCharges == "Perday")
        //            {
        //                serviceChargeAmount = ContractorWiseRateMaster.Rate * sumPaidDays;
        //            }
        //            else
        //            {
        //                serviceChargeAmount = ContractorWiseRateMaster.Rate;
        //            }
        //            // Create worksheet
        //            var worksheet = workbook.Worksheets.Add(branchName);

        //            // Set the header row
        //            worksheet.Range(1, 1, 1, 10).Merge();
        //            worksheet.Cell(1, 1).Value = "Contractor Invoice - (" + FromDate.ToString("MMM/yyyy") + ") - " + branchName;
        //            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        //            worksheet.Cell(1, 1).Style.Font.Bold = true;

        //            // Insert the datatable starting at row 2, column 1
        //            worksheet.Cell(2, 1).InsertTable(dt, false);

        //            int totalRows = worksheet.RowsUsed().Count();
        //            worksheet.SheetView.FreezeRows(2); // Freeze the header row






        //            // Add service charge amount row below the last data row
        //            int newRow = totalRows + 1;
        //            var serviceChargeRange = worksheet.Range(newRow, 1, newRow, Math.Max(22, dt.Columns.Count));
        //            serviceChargeRange.Merge();
        //            if (ContractorWiseRateMaster.ServiceCharges == "Percentage")
        //            {
        //                serviceChargeRange.Value = $"Service Charges ({ContractorWiseRateMaster.ServiceCharges} - {ContractorWiseRateMaster.Percentage} %) = {serviceChargeAmount}";

        //            }
        //            else
        //            {
        //                serviceChargeRange.Value = $"Service Charges ({ContractorWiseRateMaster.ServiceCharges} - { ContractorWiseRateMaster.Rate}) = {serviceChargeAmount}";
        //            }
        //            serviceChargeRange.Style.Font.Bold = true;
        //            serviceChargeRange.Style.Fill.BackgroundColor = XLColor.LightYellow;
        //            serviceChargeRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        //            serviceChargeRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

        //            // Add Supervisor Charges row
        //            int supervisorRow = newRow + 1;
        //            var supervisorRange = worksheet.Range(supervisorRow, 1, supervisorRow, Math.Max(22, dt.Columns.Count));
        //            supervisorRange.Merge();
        //            supervisorRange.Value = $"Supervisor Charges = {ContractorWiseRateMaster.SupervisorCharges}";
        //            supervisorRange.Style.Font.Bold = true;
        //            supervisorRange.Style.Fill.BackgroundColor = XLColor.LightYellow;
        //            supervisorRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        //            supervisorRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

        //            // Add Invoicing Amount row
        //            int invoicingRow = newRow + 2;
        //            var invoicingRange = worksheet.Range(invoicingRow, 1, invoicingRow, Math.Max(22, dt.Columns.Count));
        //            invoicingRange.Merge();
        //            supervisorCharges = Convert.ToDouble(ContractorWiseRateMaster.SupervisorCharges);
        //            double invoicingAmount = (sumTotal + serviceChargeAmount + supervisorCharges);
        //            invoicingRange.Value = $"Invoicing Amount = {invoicingAmount}";
        //            invoicingRange.Style.Font.Bold = true;
        //            invoicingRange.Style.Fill.BackgroundColor = XLColor.LightYellow;
        //            invoicingRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        //            invoicingRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

        //            // Style the data table
        //            var usedRange = worksheet.RangeUsed();
        //            usedRange.Style.Fill.BackgroundColor = XLColor.White;
        //            usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        //            usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        //            usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        //            usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
        //            usedRange.Style.Font.FontSize = 10;
        //            usedRange.Style.Font.FontColor = XLColor.Black;

        //            // Style header row
        //            var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
        //            headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
        //            headerRange.Style.Font.FontSize = 10;
        //            headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
        //            headerRange.Style.Font.Bold = true;

        //            worksheet.Columns().AdjustToContents(); // Adjust columns width to fit content
        //        }

        //        using (var stream = new MemoryStream())
        //        {
        //            workbook.SaveAs(stream);
        //            stream.Position = 0;

        //            return File(stream.ToArray(),
        //                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //                $"ContractorInvoice_{FromDate:yyyyMM}.xlsx");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
        //}
        //#endregion


        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? BranchId, DateTime FromDate, int? CmpId, int? ContractorId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters Branch = new DynamicParameters();
                Branch.Add("@p_employeeid", Session["EmployeeId"]);
                Branch.Add("@p_CmpId", CmpId);
                var allBranches = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", Branch).ToList();
                // Filter branch list if a specific BranchId is provided
                var selectedBranches = BranchId != null
                    ? allBranches.Where(b => b.Id == BranchId.Value).ToList()
                    : allBranches;


                var workbook = new XLWorkbook();
                foreach (var branch in selectedBranches)
                {
                    string branchName = Regex.Replace(branch.Name, @"[\\/\?\*\[\]]", "-");
                    double branchId = branch.Id;
                    DataTable dt = new DataTable();
                    List<dynamic> data = new List<dynamic>();

                   

                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_MonthYear", FromDate);
                    param.Add("@p_BranchId", branchId);
                    param.Add("@p_ContractorId", ContractorId?.ToString() ?? "");
                    data = DapperORM.ExecuteSP<dynamic>("SP_Rpt_ConctratorPayrollSummery", param).ToList();



                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(data);

                    if (data.Count == 1 || dt.Rows.Count == 1)
                    {
                        byte[] emptyFileContents = new byte[0];
                        return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                    }

                    if (data.Count == 0)
                    {
                        if (dt.Rows.Count == 0)
                        {
                            continue;
                        }
                        byte[] emptyFileContents = new byte[0];
                        return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                    }

                    var worksheet = workbook.Worksheets.Add(branchName);

                   

                    //double sumTotal = 0;
                    //double sumPaidDays = 0;
                    //double serviceChargeAmount = 0;
                    //double supervisorCharges = 0;
                    //string serviceChargeType = "";
                    ////Calculate sumTotal from the "Total" column
                    //if (dt.Columns.Contains("Total") && dt.Rows.Count > 0)
                    //{
                    //    var lastRow = dt.Rows[dt.Rows.Count - 1];
                    //    if (lastRow["Total"] != DBNull.Value)
                    //    {
                    //        sumTotal = Convert.ToDouble(lastRow["Total"]);
                    //    }
                    //}

                    //// Calculate sumPaidDays
                    //if (dt.Columns.Contains("PaidDays"))
                    //{
                    //    for (int i = 0; i < dt.Rows.Count; i++)
                    //    {
                    //        if (dt.Rows[i]["PaidDays"] != DBNull.Value)
                    //        {
                    //            sumPaidDays += Convert.ToDouble(dt.Rows[i]["PaidDays"]);
                    //        }
                    //    }
                    //}

                    ////var GetDetails = "Select ServiceCharges,Percentage,Rate from Manpower_ContractorWiseRateMaster where Deactivate=0 and '"+ FromDate + "' between FromMonth and ToMonth and ContractorId='"+ ContractorId +"' and BranchId='" + branchId + "';";
                    ////var ContractorWiseRateMaster = DapperORM.DynamicQueryMultiple(GetDetails);

                    //DynamicParameters prContractorWiseRate = new DynamicParameters();
                    //prContractorWiseRate.Add("@query", "Select ServiceCharges,SupervisorCharges,Percentage,Rate from Manpower_ContractorWiseRateMaster where Deactivate=0 and '" + FromDate + "' between FromMonth and ToMonth and ContractorId='" + ContractorId + "' and BranchId='" + branchId + "'");
                    //var ContractorWiseRateMaster = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", prContractorWiseRate).FirstOrDefault();

                    //if (ContractorWiseRateMaster.ServiceCharges == "Percentage")
                    //{
                    //    serviceChargeAmount = (sumTotal * ContractorWiseRateMaster.Percentage) / 100;
                    //}
                    //else if (ContractorWiseRateMaster.ServiceCharges == "Perday")
                    //{
                    //    serviceChargeAmount = ContractorWiseRateMaster.Rate * sumPaidDays;
                    //}
                    //else
                    //{
                    //    serviceChargeAmount = ContractorWiseRateMaster.Rate;
                    //}

                   

                    //// Set the header row
                    //worksheet.Range(1, 1, 1, 10).Merge();
                    //worksheet.SheetView.FreezeRows(2); // Freeze the row

                    //worksheet.Cell(1, 1).Value = "Contractor Invoice - (" + FromDate.ToString("MMM/yyyy") + ") - " + branchName;
                    //worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    //worksheet.Cell(1, 1).Style.Font.Bold = true;
                    //// Insert the datatable starting at row 2, column 1
                    //worksheet.Cell(2, 1).InsertTable(dt, false);

                    //int totalRows = worksheet.RowsUsed().Count();
                    //worksheet.SheetView.FreezeRows(2); // Freeze the header row






                    ////// Add service charge amount row below the last data row
                    ////int newRow = totalRows + 1;
                    ////var serviceChargeRange = worksheet.Range(newRow, 1, newRow, Math.Max(22, dt.Columns.Count));
                    ////serviceChargeRange.Merge();
                    ////if (ContractorWiseRateMaster.ServiceCharges == "Percentage")
                    ////{
                    ////    serviceChargeRange.Value = $"Service Charges ({ContractorWiseRateMaster.ServiceCharges} - {ContractorWiseRateMaster.Percentage} %) = {serviceChargeAmount}";

                    ////}
                    ////else
                    ////{
                    ////    serviceChargeRange.Value = $"Service Charges ({ContractorWiseRateMaster.ServiceCharges} - { ContractorWiseRateMaster.Rate}) = {serviceChargeAmount}";
                    ////}
                    ////serviceChargeRange.Style.Font.Bold = true;
                    ////serviceChargeRange.Style.Fill.BackgroundColor = XLColor.LightYellow;
                    ////serviceChargeRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ////serviceChargeRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);


                    ////// Add Supervisor Charges row
                    ////int supervisorRow = newRow + 1;
                    ////var supervisorRange = worksheet.Range(supervisorRow, 1, supervisorRow, Math.Max(22, dt.Columns.Count));
                    ////supervisorRange.Merge();
                    ////supervisorRange.Value = $"Supervisor Charges = {ContractorWiseRateMaster.SupervisorCharges}";
                    ////supervisorRange.Style.Font.Bold = true;
                    ////supervisorRange.Style.Fill.BackgroundColor = XLColor.LightYellow;
                    ////supervisorRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ////supervisorRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

                    ////// Add Invoicing Amount row
                    ////int invoicingRow = newRow + 2;
                    ////var invoicingRange = worksheet.Range(invoicingRow, 1, invoicingRow, Math.Max(22, dt.Columns.Count));
                    ////invoicingRange.Merge();
                    ////supervisorCharges = Convert.ToDouble(ContractorWiseRateMaster.SupervisorCharges);
                    ////double invoicingAmount = (sumTotal + serviceChargeAmount + supervisorCharges);
                    ////invoicingRange.Value = $"Invoicing Amount = {invoicingAmount}";
                    ////invoicingRange.Style.Font.Bold = true;
                    ////invoicingRange.Style.Fill.BackgroundColor = XLColor.LightYellow;
                    ////invoicingRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ////invoicingRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);



                    //// Get safe column count (DO NOT hardcode 22)
                    //int totalCols = dt != null && dt.Columns.Count > 0 ? dt.Columns.Count : 1;

                    //// Ensure valid row
                    //int newRow = totalRows + 1;
                    //if (newRow <= 0) newRow = 1;

                    //// =====================
                    //// Service Charges Row
                    //// =====================
                    //var serviceChargeRange = worksheet.Range(newRow, 1, newRow, totalCols);

                    //// Merge ONLY if multiple columns
                    //if (totalCols > 1)
                    //{
                    //    serviceChargeRange.Merge();
                    //}

                    //string serviceText = "";

                    //if (ContractorWiseRateMaster.ServiceCharges == "Percentage")
                    //{
                    //    serviceText = $"Service Charges (Percentage - {ContractorWiseRateMaster.Percentage} %) = {serviceChargeAmount}";
                    //}
                    //else
                    //{
                    //    serviceText = $"Service Charges (Rate - {ContractorWiseRateMaster.Rate}) = {serviceChargeAmount}";
                    //}

                    //// Set value safely
                    //serviceChargeRange.Cell(1, 1).Value = serviceText;

                    //// Apply style AFTER value
                    //serviceChargeRange.Style.Font.Bold = true;
                    //serviceChargeRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    //serviceChargeRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

                    //// =====================
                    //// Supervisor Charges Row
                    //// =====================
                    //int supervisorRow = newRow + 1;

                    //var supervisorRange = worksheet.Range(supervisorRow, 1, supervisorRow, totalCols);

                    //if (totalCols > 1)
                    //{
                    //    supervisorRange.Merge();
                    //}

                    
                    //double.TryParse(Convert.ToString(ContractorWiseRateMaster.SupervisorCharges), out supervisorCharges);

                    //supervisorRange.Cell(1, 1).Value = $"Supervisor Charges = {supervisorCharges}";

                    //supervisorRange.Style.Font.Bold = true;
                    //supervisorRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    //supervisorRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

                    //// =====================
                    //// Invoicing Amount Row
                    //// =====================
                    //int invoicingRow = newRow + 2;

                    //var invoicingRange = worksheet.Range(invoicingRow, 1, invoicingRow, totalCols);

                    //if (totalCols > 1)
                    //{
                    //    invoicingRange.Merge();
                    //}

                    //// Safe calculations
                    //double safeSumTotal = 0;
                    //double safeServiceCharge = 0;

                    //double.TryParse(Convert.ToString(sumTotal), out safeSumTotal);
                    //double.TryParse(Convert.ToString(serviceChargeAmount), out safeServiceCharge);

                    //double invoicingAmount = safeSumTotal + safeServiceCharge + supervisorCharges;

                    //invoicingRange.Cell(1, 1).Value = $"Invoicing Amount = {invoicingAmount}";

                    //invoicingRange.Style.Font.Bold = true;
                    //invoicingRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    //invoicingRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);














                    worksheet.Cell(2, 1).InsertTable(dt, false);
                    var usedRange = worksheet.RangeUsed();
                    usedRange.Style.Fill.BackgroundColor = XLColor.White;
                    usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Font.FontSize = 10;
                    usedRange.Style.Font.FontColor = XLColor.Black;


                    worksheet.Columns().AdjustToContents(); // This code for all clomns
                    var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
                    headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                    headerRange.Style.Font.FontSize = 10;
                    headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
                    headerRange.Style.Font.Bold = true;
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0; // Reset the stream position to the beginning
                    // Return the file to the client
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
