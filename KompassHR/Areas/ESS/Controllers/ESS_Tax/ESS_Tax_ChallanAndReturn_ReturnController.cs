using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Tax;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Tax
{
    public class ESS_Tax_ChallanAndReturn_ReturnController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_Tax_ChallanAndReturn_Return
        #region MainView
        public ActionResult ESS_Tax_ChallanAndReturn_Return()
        {
            try
            {

                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 640;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";

                var GetChallanNo = "Select Isnull(Max(ChallanNo),0)+1 As ChallanNo from IncomeTax_Challan";
                var ChallanNo = DapperORM.DynamicQuerySingle(GetChallanNo);
                ViewBag.ChallanNo = ChallanNo;

                Session["MonthYear"] = null;
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "Select TaxFyearId as Id,TaxYear As Name  from IncomeTax_Fyear where Deactivate=0 and [IsActive]=1");
                ViewBag.GetTaxFyear = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param).ToList();

                DynamicParameters paramTAN = new DynamicParameters();
                paramTAN.Add("@query", "Select DeductorResponsibleId as Id,DeductorTAN As Name  from IncomeTax_DeductorResponsible where Deactivate=0 ");
                ViewBag.GetTAN = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", paramTAN).ToList();

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region DownloadExcel
        public ActionResult DownloadExcelFile1(int FYearID, int TAN_ID, string Quarter, DateTime Date)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("ChallanReturnReport");
                worksheet.Range(1, 1, 1, 35).Merge();
                worksheet.SheetView.FreezeRows(2); // Freeze the first row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();


                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_TANID", TAN_ID);
                paramList.Add("@p_FyearId", FYearID);
                paramList.Add("@p_Quarter", Quarter);
                paramList.Add("@p_Date", Date);

                var GetTaxComputationReport = DapperORM.ExecuteSP<dynamic>("sp_Tax_Return_Excel", paramList).ToList();
                if (GetTaxComputationReport.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }
                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(GetTaxComputationReport);
                worksheet.Cell(2, 1).InsertTable(dt, false);
                int totalRows = worksheet.RowsUsed().Count();



                // Set the background color to white and apply borders
                var usedRange = worksheet.RangeUsed();
                usedRange.Style.Fill.BackgroundColor = XLColor.White;
                usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Font.FontSize = 10;
                usedRange.Style.Font.FontColor = XLColor.Black;

                // Set the header row name
                worksheet.Cell(1, 1).Value = "Challan Return Report  - (" + FYearID + ")";
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Columns().AdjustToContents(); // This code for all clomns

                var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                headerRange.Style.Font.FontSize = 10;
                headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
                headerRange.Style.Font.Bold = true;

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
                ex.Message.ToString();
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion DownloadExcel

        #region DownloadRPU
        public ActionResult DownloadRPUFile(int FYearID, int TAN_ID, string Quarter, DateTime Date, string TAN, string Fyear, string QuarterName)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_TANID", TAN_ID);
                paramList.Add("@p_FyearId", FYearID);
                paramList.Add("@p_Quarter", Quarter);
                paramList.Add("@p_Date", Date);

                var rpuData = DapperORM.ExecuteSP<dynamic>("sp_Tax_Return_RPUFile", paramList).ToList();
                if (rpuData.Count == 0)
                {
                    byte[] emptyFileContents = Encoding.UTF8.GetBytes("No data found.");
                    return File(emptyFileContents, "text/plain", "FileNotFound.txt");
                }

                // Convert RPU data to text format
                StringBuilder rpuText = new StringBuilder();
                foreach (var record in rpuData)
                {
                    // Use IDictionary to handle dynamic object properties
                    var dict = (IDictionary<string, object>)record;
                    List<string> values = new List<string>();
                    foreach (var pair in dict)
                    {
                        // Safely handle null values
                        string value = pair.Value != null ? pair.Value.ToString() : "";
                        values.Add(value);
                    }
                    // Join values with pipe delimiter (adjust delimiter as per RPU format)
                    rpuText.AppendLine(string.Join("|", values));
                }

                // Convert the text to bytes
                byte[] fileContents = Encoding.UTF8.GetBytes(rpuText.ToString());

                // Return the file as a plain text file with .txt extension
                return File(fileContents, "application/octet-stream", $"{TAN}-{Fyear}-{QuarterName}");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        public ActionResult DownloadExcelFile(int FYearID, int TAN_ID, string Quarter, DateTime Date, string TAN, string Fyear, string QuarterName)
        {
            try
            {
                // Check if user is logged in
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                // Create a new Excel workbook
                var workbook = new XLWorkbook();

                // Define sheet names for multiple result sets
                string[] sheetNames = { "TAN Detail", "Challan Detail", "Deductee Detail", "Salary Detail" };

                // Set up Dapper parameters
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_TANID", TAN_ID);
                paramList.Add("@p_FyearId", FYearID);
                paramList.Add("@p_Quarter", Quarter);
                paramList.Add("@p_Date", Date);

                // Execute stored procedure with multiple result sets
                string connectionString = Session["MyNewConnectionString"]?.ToString();
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (var multi = connection.QueryMultiple("sp_Tax_Return_Excel", paramList, commandType: CommandType.StoredProcedure))
                    {
                        int sheetIndex = 0;

                        // Process the first result set (TAN Detail)
                        if (!multi.IsConsumed && sheetIndex == 0)
                        {
                            var data = multi.Read<IncomeTax_Challan_Return_TanDetail>().FirstOrDefault();
                            var worksheet = workbook.Worksheets.Add(sheetNames[sheetIndex]);

                            // Add the title (merge cells and style)
                            worksheet.Range(1, 1, 1, 2).Merge();
                            worksheet.Cell(1, 1).Value = "Quarterly statement of deduction of tax under sub-section (3) of section 200 of the Income Tax Act, 1961 in respect of Salary.";
                            worksheet.Cell(1, 1).Style.Font.Bold = true;
                            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                            // Add "For Quarter Ended" with QuarterName
                            worksheet.Range(2, 1, 2, 2).Merge();
                            worksheet.Cell(2, 1).Value = $"FOR QUARTER ENDED - {QuarterName}";
                            worksheet.Cell(2, 1).Style.Font.Bold = true;
                            worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                           
                            if (data != null)
                            {
                                var fields = new List<Tuple<string, string>>
                        {


                            new Tuple<string, string>("1.Particulars of Statement", "HEADING"), // Heading

                            new Tuple<string, string>("Tax Deduction and Collection Account No. (TAN)", data.DeductorTAN ?? ""),
                            new Tuple<string, string>("Permanent Account Number.", data.DeductorPAN ?? ""),
                            new Tuple<string, string>("Financial Year ", ""),
                            new Tuple<string, string>("Assessment Year ", ""),
                            new Tuple<string, string>("Type of Deductor ", data.DeductorType ?? ""),

                            new Tuple<string, string>("2.Particulars of Deductor", "HEADING"), // Heading

                            new Tuple<string, string>("Name ", data.DeductorName ?? ""),
                            new Tuple<string, string>("Branch/Division (If Any) ", data.DeductorDivision ?? ""),
                            new Tuple<string, string>("Name of Premises /Building", data.DeductorNameofPremisesBuilding ?? ""),
                            new Tuple<string, string>("Area/Location", data.DeductorAreaLocality ?? ""),
                            new Tuple<string, string>("PIN Code ", data.DeductorPin ?? ""),
                            new Tuple<string, string>("Flat No.", data.DeductorFlatDoorBlockNo ?? ""),
                            new Tuple<string, string>("Road/Street/Lane", data.DeductorRoadStreetLane ?? ""),
                            new Tuple<string, string>("Town/City/District", data.DeductorTownCityDistrict ?? ""),
                            new Tuple<string, string>("State ", data.DeductorState ?? ""),
                            new Tuple<string, string>("E-mail", data.DeductorEmail ?? ""),
                            new Tuple<string, string>("Has Address Changed Since Last Return ", string.IsNullOrEmpty(data.IsChangeAddressDeductor) ? "No" : data.IsChangeAddressDeductor),

                            new Tuple<string, string>("3.Particulars of the Person Responsible for Deduction of Tax", "HEADING"), // Heading

                            new Tuple<string, string>("Name ", data.ResponsibleName ?? ""),
                            new Tuple<string, string>("Designation ", data.ResponsibleDesignation ?? ""),
                            new Tuple<string, string>("Name of Premises/Building", data.ResponsibleNameofPremisesBuilding ?? ""),
                            new Tuple<string, string>("Area/Location", data.ResponsibleAreaLocality ?? ""),
                            new Tuple<string, string>("PIN Code ", data.ResponsiblePin ?? ""),
                            new Tuple<string, string>("Permanent Account Number ", data.ResponsiblePAN ?? ""),
                            new Tuple<string, string>("Flat No. ", data.ResponsibleFlatDoorBlockNo ?? ""),
                            new Tuple<string, string>("Road/Street/Lane", data.ResponsibleRoadStreetLane ?? ""),
                            new Tuple<string, string>("Town/City/District", data.ResponsibleTownCityDistrict ?? ""),
                            new Tuple<string, string>("State ", data.ResponsibleState ?? ""),
                            new Tuple<string, string>("E-mail", data.ResponsibleEmail ?? ""),
                            new Tuple<string, string>("Mobile No.", data.ResponsibleMobile ?? ""),
                        };

                                // Populate the fields starting from row 4
                                int row = 4;
                                foreach (var field in fields)
                                {
                                    if (field.Item2 == "HEADING")
                                    {
                                        // Render heading: merge cells, apply bold font, and background color
                                        worksheet.Range(row, 1, row, 2).Merge();
                                        worksheet.Cell(row, 1).Value = field.Item1;
                                        worksheet.Cell(row, 1).Style.Font.Bold = true;
                                        worksheet.Cell(row, 1).Style.Font.FontSize = 12;
                                        worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(200, 200, 200); // Light gray background
                                    }
                                    else
                                    {
                                        // Render regular field
                                        worksheet.Cell(row, 1).Value = field.Item1;
                                        worksheet.Cell(row, 2).Value = field.Item2;
                                        worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                        worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    }
                                    row++;
                                }

                                // Apply styling to the table (excluding headings)
                                var tableRange = worksheet.Range(4, 1, row - 1, 2);
                                tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                                tableRange.Style.Font.FontSize = 10;
                                tableRange.Style.Font.FontColor = XLColor.Black;

                                // Adjust column widths
                                worksheet.Column(1).Width = 40; // Labels column
                                worksheet.Column(2).Width = 20; // Values column
                            }
                            else
                            {
                                // Handle case where no data is returned
                                worksheet.Cell(4, 1).Value = "No TAN Detail data available.";
                            }
                            sheetIndex++;
                        }

                        // Process remaining result sets (Challan Detail, Deductee Detail, Salary Detail)
                        while (!multi.IsConsumed && sheetIndex < sheetNames.Length)
                        {
                            var data = multi.Read().ToList();
                            if (data.Any())
                            {
                                var dprObj = new DapperORM();
                                var dt = dprObj.ConvertToDataTable(data);
                                var worksheet = workbook.Worksheets.Add(sheetNames[sheetIndex]);

                                worksheet.Range(1, 1, 1, dt.Columns.Count).Merge();
                                worksheet.Cell(1, 1).Value = $"{sheetNames[sheetIndex]} - ({FYearID})";
                                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                worksheet.Cell(1, 1).Style.Font.Bold = true;

                                worksheet.Cell(2, 1).InsertTable(dt, false);
                                worksheet.SheetView.FreezeRows(2);

                                var usedRange = worksheet.RangeUsed();
                                usedRange.Style.Fill.BackgroundColor = XLColor.White;
                                usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                usedRange.Style.Font.FontSize = 10;
                                usedRange.Style.Font.FontColor = XLColor.Black;

                                var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
                                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                                headerRange.Style.Font.FontSize = 10;
                                headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
                                headerRange.Style.Font.Bold = true;

                                worksheet.Columns().AdjustToContents();
                            }
                            sheetIndex++;
                        }
                    }
                }

                // Check if any data was added
                if (workbook.Worksheets.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }

                // Save the workbook to a stream
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
    }
}
