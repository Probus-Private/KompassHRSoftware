using ClosedXML.Excel;
using Dapper;
using System.Data;
using System.IO;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using KompassHR.Areas.Reports.Models;

namespace KompassHR.Areas.Reports.Controllers.Report_Payroll
{
    public class Rpt_Payroll_PayrollActController : Controller
    {
        #region Main View
        // GET: Reports/Rpt_Payroll_PayrollAct
        public ActionResult Rpt_Payroll_PayrollAct(MonthWiseFilter MonthWiseFilter)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 759;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                //GET COMPANY NAME
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                var CMPID = GetComapnyName[0].Id;

                //GET BRANCH NAME
                //var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CMPID), Convert.ToInt32(Session["EmployeeId"]));
                //ViewBag.BranchName = Branch;
                if (MonthWiseFilter.CmpId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select BranchId As Id,BranchName As Name From Mas_Branch where Deactivate=0 And CmpId='" + MonthWiseFilter.CmpId + "' order by Name");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    ViewBag.BranchName = data;
                }
                else
                {
                    ViewBag.BranchName = "";
                }

                if (MonthWiseFilter.CmpId != null && MonthWiseFilter.BranchId != null)
                {
                    DynamicParameters paramFormName = new DynamicParameters();
                    //paramStateName.Add("@query", "select StateId As Id, StateName As Name from Mas_States where Deactivate =0 ");            
                    paramFormName.Add("@query", "Select Distinct PayrollActId As[Id], Form + ' (' + Register + ')' AS [Name] from Payroll_Act Where IsActive=1;");
                    ViewBag.FormName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramFormName).ToList();
                }
                else
                {
                    ViewBag.FormName = "";
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
        
        #region Get Branch Name
        [HttpGet]
        public ActionResult GetMonthlyBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select BranchId As Id,BranchName As Name From Mas_Branch where Deactivate=0 And CmpId='" + CmpId + "' order by Name");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);

                //var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CmpId), Convert.ToInt32(Session["EmployeeId"]));
                //return Json(new { Branch = Branch }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
        
        #region GetFormName
        [HttpGet]
        public ActionResult GetFormName(int CmpId, int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select distinct PayrollActId As[Id], Form + ' (' + Register + ')' AS [Name]  from Payroll_Act INNER JOIN Mas_Branch on Mas_Branch.StateId = Payroll_Act.PayrollAct_StateId Left JOIN Mas_Employee on Mas_Branch.BranchId = Mas_Employee.EmployeeBranchId Left join Mas_CompanyProfile on Mas_CompanyProfile.CompanyId = Mas_Employee.CmpID WHERE Mas_Employee.Deactivate = 0 AND Payroll_Act.IsActive=1  AND Mas_CompanyProfile.CompanyId = '" + CmpId + "'AND Mas_Employee.EmployeeBranchId = '" + BranchId + "'");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
    
        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, DateTime Month, DateTime ToMonth, int FormId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var FormRegister = @"
                SELECT Form AS Name, Payroll_Act.Register AS FormRegister, Payroll_Act.Act As [Act],
                Mas_Branch.BranchName AS [BranchName],Mas_Branch.BranchAddress as[BranchAddress]
                FROM Payroll_Act 
                INNER JOIN Mas_Branch ON Mas_Branch.StateId = Payroll_Act.PayrollAct_StateId
                LEFT JOIN Mas_Employee ON Mas_Branch.BranchId = Mas_Employee.EmployeeBranchId
                LEFT JOIN Mas_CompanyProfile ON Mas_CompanyProfile.CompanyId = Mas_Employee.CmpID
                WHERE Mas_Employee.CmpID = '" + CmpId + @"'
                AND Mas_Employee.EmployeeBranchId = '" + BranchId + @"'
                AND Payroll_Act.PayrollActId = '" + FormId + @"'
                GROUP BY Payroll_Act.PayrollActId, Payroll_Act.Register, Mas_Branch.BranchName,Mas_Branch.BranchAddress , Payroll_Act.Act, Form";

                    var Form = DapperORM.DynamicQuerySingle(FormRegister);

                    if (Form != null)
                    {
                        Session["FormAct"] = Form.Act;
                        Session["FormRegister"] = Form.FormRegister;
                        Session["FormName"] = Form.Name;
                        Session["BranchName"] = Form.BranchName;
                        Session["BranchAddress"] = Form.BranchAddress;
                    }
                }
                var workbook = new XLWorkbook();

                #region MAHARASHTRA FORMS

                #region Form O

                if (FormId == 5)///// Form O
                {
                    var worksheet = workbook.Worksheets.Add("FormOReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 15).Merge();
                    worksheet.Range(1, 1, 1, 15).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();
                    var TextInfo = worksheet.Cell(2, 1).RichText;
                    TextInfo.AddText("NAME OF ESTABLISHMENT: ")
                               .SetBold()
                               .SetFontSize(11);

                    TextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim() + "                 ")
                    .SetFontSize(11);
                    TextInfo.AddText("NAME OF Employer: ")
                               .SetBold()
                               .SetFontSize(11);
                    TextInfo.AddText((Session["EmployeeName"] ?? "").ToString().Trim() + "                ")
                            .SetFontSize(11);

                    TextInfo.AddText("Name Of theEmployee: ")
                            .SetBold()
                            .SetFontSize(11);
                    TextInfo.AddText((Session["EmployeeName"] ?? "").ToString().Trim())
                         .SetFontSize(11);
                    TextInfo.AddNewLine();
                    TextInfo.AddText("Description Of Department:")
                               .SetBold()
                               .SetFontSize(11);
                    TextInfo.AddText("12345")
                          .SetFontSize(11);
                    TextInfo.AddNewLine();
                    TextInfo.AddText("EmployeeId: ")
                      .SetBold()
                      .SetFontSize(11);

                    TextInfo.AddText((Session["EmployeeId"] ?? "").ToString().Trim() + "                                                        ")
                            .SetFontSize(11);

                    TextInfo.AddText("Date of Entry into Service:                                   ")
                        .SetBold()
                        .SetFontSize(11);
                    TextInfo.AddText("Thumb impression or Signature: ")
                       .SetBold()
                       .SetFontSize(11);
                    TextInfo.AddNewLine();

                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Range(2, 1, 4, 15).Merge();
                    worksheet.Range(2, 1, 4, 15).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Row(2).Height = 30;
                    worksheet.Row(3).Height = 30;

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);

                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_Form_O", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 5;
                    worksheet.Cell(currentRow, 1).Value = "Accumulation of Leave";
                    worksheet.Range(currentRow, 1, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Leave Allowed";
                    worksheet.Range(currentRow, 3, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Payment for Leave made on";
                    worksheet.Range(currentRow, 5, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "REFUSAL OF LEAVE";
                    worksheet.Range(currentRow, 7, currentRow, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "Payment for Leave on discharge of an employee quitting employment if admissible";
                    worksheet.Range(currentRow, 10, currentRow, 15).Merge();

                    // Sub-headers
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = "Leave due on";
                    worksheet.Cell(currentRow, 2).Value = "No. of days";
                    worksheet.Cell(currentRow, 3).Value = "From";
                    worksheet.Cell(currentRow, 4).Value = "To";
                    worksheet.Cell(currentRow, 5).Value = "1st Moiety";
                    worksheet.Cell(currentRow, 6).Value = "2nd Moiety";
                    worksheet.Cell(currentRow, 7).Value = "Date of application";
                    worksheet.Cell(currentRow, 8).Value = "Date of Refusal";
                    worksheet.Cell(currentRow, 9).Value = "Reason for Refusal";
                    worksheet.Cell(currentRow, 10).Value = "Amount of Leave Refused";
                    worksheet.Cell(currentRow, 11).Value = "Date of discharge";
                    worksheet.Cell(currentRow, 12).Value = "Date";
                    worksheet.Cell(currentRow, 13).Value = "Amount paid";
                    worksheet.Cell(currentRow, 14).Value = "Signature of Left hand thumb impression of employee";
                    worksheet.Cell(currentRow, 15).Value = "Remarks";

                    var headerRange = worksheet.Range(5, 1, currentRow, 15);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count;
                    currentRow++;

                    // ---------- SECOND + THIRD TABLE SIDE BY SIDE ----------

                    int tableStartRow = currentRow;

                    // Second table title
                    worksheet.Cell(tableStartRow, 1).Value = "DETAILS OF FESTIVAL LEAVE";
                    worksheet.Range(tableStartRow, 1, tableStartRow, 7).Merge();

                    // Third table title
                    worksheet.Cell(tableStartRow, 9).Value = "DETAILS OF CASUAL LEAVE";
                    worksheet.Range(tableStartRow, 9, tableStartRow, 14).Merge();

                    tableStartRow++;

                    // Second table headers
                    worksheet.Cell(tableStartRow, 1).Value = "PERIOD";
                    worksheet.Range(tableStartRow, 1, tableStartRow, 2).Merge();
                    worksheet.Cell(tableStartRow, 3).Value = "TOTAL LEAVE";
                    worksheet.Range(tableStartRow, 3, tableStartRow + 1, 3).Merge();
                    worksheet.Cell(tableStartRow, 4).Value = "AVAILED LEAVE";
                    worksheet.Range(tableStartRow, 4, tableStartRow + 1, 4).Merge();
                    worksheet.Cell(tableStartRow, 5).Value = "BALANCE LEAVE";
                    worksheet.Range(tableStartRow, 5, tableStartRow + 1, 5).Merge();
                    worksheet.Cell(tableStartRow, 6).Value = "PAYMENT MADE IN LIEU OF FESTIVAL LEAVE WHEN CALLED FOR WORK";
                    worksheet.Range(tableStartRow, 6, tableStartRow + 1, 6).Merge();
                    worksheet.Cell(tableStartRow, 7).Value = "REMARK";
                    worksheet.Range(tableStartRow, 7, tableStartRow + 1, 7).Merge();

                    // Third table headers
                    worksheet.Cell(tableStartRow, 9).Value = "PERIOD";
                    worksheet.Range(tableStartRow, 9, tableStartRow, 10).Merge();
                    worksheet.Cell(tableStartRow, 11).Value = "TOTAL LEAVE";
                    worksheet.Range(tableStartRow, 11, tableStartRow + 1, 11).Merge();
                    worksheet.Cell(tableStartRow, 12).Value = "AVAILED LEAVE";
                    worksheet.Range(tableStartRow, 12, tableStartRow + 1, 12).Merge();
                    worksheet.Cell(tableStartRow, 13).Value = "BALANCE LEAVE";
                    worksheet.Range(tableStartRow, 13, tableStartRow + 1, 13).Merge();
                    worksheet.Cell(tableStartRow, 14).Value = "REMARK";
                    worksheet.Range(tableStartRow, 14, tableStartRow + 1, 14).Merge();

                    // Sub-header row for both
                    tableStartRow++;
                    worksheet.Cell(tableStartRow, 1).Value = "From";
                    worksheet.Cell(tableStartRow, 2).Value = "To";
                    worksheet.Cell(tableStartRow, 9).Value = "From";
                    worksheet.Cell(tableStartRow, 10).Value = "To";

                    // Styling
                    var headerRange1 = worksheet.Range(tableStartRow - 2, 1, tableStartRow, 7);
                    headerRange1.Style.Font.Bold = true;
                    headerRange1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange1.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange1.Style.Alignment.WrapText = true;
                    headerRange1.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange1.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    var headerRange2 = worksheet.Range(tableStartRow - 2, 9, tableStartRow, 14);
                    headerRange2.Style.Font.Bold = true;
                    headerRange2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange2.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange2.Style.Alignment.WrapText = true;
                    headerRange2.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange2.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill both tables data side by side
                    tableStartRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < 7; c++)
                        {
                            worksheet.Cell(tableStartRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                        for (int c = 0; c < 6; c++)
                        {
                            worksheet.Cell(tableStartRow + r, 9 + c).Value = dt.Rows[r][c];
                        }
                    }

                    worksheet.Columns().AdjustToContents();
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.Width > 10) column.Width = 10;
                        if (column.Width < 10) column.Width = 10;
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form 10( Maternity Benefit Register)
                if (FormId == 1)   //// Form 10

                {
                    var worksheet = workbook.Worksheets.Add("Form10Report");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 17).Merge();
                    worksheet.Range(1, 1, 1, 17).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;
                    //worksheet.SheetView.FreezeRows(1);
                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;
                    richTextInfo.AddText("NAME OF ESTABLISHMENT: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("ADDRESS OF ESTABLISHMENT:")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText("12345")
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("MONTH: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText(Month.ToString("MMM-yyyy"))
                            .SetFontSize(11);
                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Range(2, 1, 4, 17).Merge();
                    worksheet.Range(2, 1, 4, 17).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Row(2).Height = 20;
                    worksheet.Row(3).Height = 20;
                    //worksheet.SheetView.FreezeRows(4);
                    // worksheet.SheetView.FreezeRows(5);
                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_FormX", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    //// Insert table starting row 5
                    //worksheet.Cell(5, 1).InsertTable(dt, false);
                    //var headerRange = worksheet.Range(5, 1, 5, dt.Columns.Count);
                    //headerRange.Style.Font.FontSize = 10;
                    //headerRange.Style.Font.Bold = true;
                    //headerRange.Style.Alignment.WrapText = true;
                    //headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    //var usedRange = worksheet.RangeUsed();
                    //usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    //usedRange.Style.Font.FontSize = 10;
                    //worksheet.Columns().AdjustToContents();
                    //foreach (var column in worksheet.ColumnsUsed())
                    //{
                    //    if (column.Width > 26) column.Width = 26;
                    //}
                    //worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    //worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    //worksheet.PageSetup.FitToPages(1, 0);

                    int currentRow = 5;
                    worksheet.Cell(currentRow, 1).Value = "Name of the woman";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Date of appointment";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Department in which employed";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Nature of work";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Dates (with month and year) on which she is laid off and not employed";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Total days employed in the";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Date on which woman gives payment period";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Date of birth of child";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Date of production of proof of pregnancy under section 6 of the Maternity Benefit Act, 1961";
                    worksheet.Range(currentRow, 9, currentRow, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "Date of production of proof of delivery / miscarriage / death";
                    worksheet.Range(currentRow, 10, currentRow, 10).Merge();
                    worksheet.Cell(currentRow, 11).Value = "Where the maternity benefit delivery, the date on which is paid in advance before it is paid and the amount thereof";
                    worksheet.Range(currentRow, 11, currentRow, 11).Merge();
                    worksheet.Cell(currentRow, 12).Value = "Where the medical bonus is paid, the date on which it is paid and the amount thereof";
                    worksheet.Range(currentRow, 12, currentRow, 12).Merge();
                    worksheet.Cell(currentRow, 13).Value = "Date on which wages on account of leave are paid and amount thereof";
                    worksheet.Range(currentRow, 13, currentRow, 13).Merge();
                    worksheet.Cell(currentRow, 14).Value = "Name of the person nominated by the woman";
                    worksheet.Range(currentRow, 14, currentRow, 14).Merge();
                    worksheet.Cell(currentRow, 15).Value = "If the woman dies, the date of her death, the name of the person to whom maternity benefit and/or other amount was paid, the amount thereof, and the date of payment";
                    worksheet.Range(currentRow, 15, currentRow, 15).Merge();
                    worksheet.Cell(currentRow, 16).Value = "If the woman dies, and the child survives, the name of the person to whom the amount of maternity benefit was paid on behalf of the child and the period for which it was paid";
                    worksheet.Range(currentRow, 16, currentRow, 16).Merge();
                    worksheet.Cell(currentRow, 17).Value = "Remarks column for the use of Inspector";
                    worksheet.Range(currentRow, 17, currentRow, 17).Merge();

                    var headerRange = worksheet.Range(5, 1, currentRow, 17);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }

                    worksheet.Columns().AdjustToContents();
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.ColumnNumber() != 1 && column.ColumnNumber() != 3 && column.ColumnNumber() != 4)
                        {
                            if (column.Width > 12) column.Width = 12;
                        }

                    }
                    worksheet.Column(1).Width = 28;
                    worksheet.Column(3).Width = 19;
                    worksheet.Column(4).Width = 18;
                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form IV
                if (FormId == 2)////Form IV

                {
                    var worksheet = workbook.Worksheets.Add("FormIVReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 10).Merge();
                    worksheet.Range(1, 1, 1, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;
                    // worksheet.SheetView.FreezeRows(1);
                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;
                    richTextInfo.AddText("NAME OF ESTABLISHMENT: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("ADDRESS OF ESTABLISHMENT:")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("MONTH: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText(Month.ToString("MMM-yyyy"))
                            .SetFontSize(11);
                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Range(2, 1, 4, 10).Merge();
                    worksheet.Range(2, 1, 4, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).Height = 20;
                    worksheet.Row(3).Height = 20;
                    // worksheet.SheetView.FreezeRows(4);
                    // worksheet.SheetView.FreezeRows(5);
                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_FormIV", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    //// Insert table starting row 5
                    //worksheet.Cell(5, 1).InsertTable(dt, false);
                    //var headerRange = worksheet.Range(5, 1, 5, dt.Columns.Count);
                    //headerRange.Style.Font.FontSize = 10;
                    //headerRange.Style.Font.Bold = true;
                    //headerRange.Style.Alignment.WrapText = true;
                    //headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    //var usedRange = worksheet.RangeUsed();
                    //usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    //usedRange.Style.Font.FontSize = 10;
                    //worksheet.Columns().AdjustToContents();
                    //foreach (var column in worksheet.ColumnsUsed())
                    //{
                    //    if (column.Width > 26) column.Width = 26;
                    //}
                    //worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    //worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    //worksheet.PageSetup.FitToPages(1, 0);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 5;
                    worksheet.Cell(currentRow, 1).Value = "Sr.No";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Name";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Fathers Name or Ticket No.";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Department";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Date & Amount of Advance Made";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Purpose(s) for which Advance made";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "No. of Instalments by which Advance to be repaid";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Postponement Granted";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Date on which Total Amount Repaid";
                    worksheet.Range(currentRow, 9, currentRow, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "Remark";
                    worksheet.Range(currentRow, 10, currentRow, 10).Merge();


                    var headerRange = worksheet.Range(5, 1, currentRow, 10);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(5, 1, currentRow, 10);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    //worksheet.Columns().AdjustToContents();
                    worksheet.Column(2).Width = 27;
                    worksheet.Column(3).Width = 26;
                    worksheet.Columns().AdjustToContents();
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.ColumnNumber() != 2 && column.ColumnNumber() != 3)
                        {
                            if (column.Width > 18) column.Width = 18;
                        }
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form X
                if (FormId == 4)////Form X

                {
                    var worksheet = workbook.Worksheets.Add("FormXReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 8).Merge();
                    worksheet.Range(1, 1, 1, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;
                    worksheet.SheetView.FreezeRows(1);
                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;
                    richTextInfo.AddText("NAME OF ESTABLISHMENT: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("ADDRESS OF ESTABLISHMENT:")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText("12345")
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("MONTH: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText(Month.ToString("MMM-yyyy"))
                            .SetFontSize(11);
                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Range(2, 1, 4, 16).Merge();
                    worksheet.Range(2, 1, 4, 16).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Row(2).Height = 20;
                    worksheet.Row(3).Height = 20;
                    worksheet.SheetView.FreezeRows(4);
                    worksheet.SheetView.FreezeRows(5);
                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_FormX", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // Insert table starting row 5
                    worksheet.Cell(5, 1).InsertTable(dt, false);
                    var headerRange = worksheet.Range(5, 1, 5, dt.Columns.Count);
                    headerRange.Style.Font.FontSize = 10;
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    var usedRange = worksheet.RangeUsed();
                    usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Font.FontSize = 10;
                    worksheet.Columns().AdjustToContents();
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.Width > 26) column.Width = 26;
                    }
                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form Q
                if (FormId == 6)////Form Q
                {
                    var worksheet = workbook.Worksheets.Add("FormQReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 43).Merge();
                    worksheet.Range(1, 1, 1, 43).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;
                    //worksheet.SheetView.FreezeRows(1);
                    var richTextInfo = worksheet.Cell(2, 1).RichText;
                    richTextInfo.AddText("NAME OF ESTABLISHMENT: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())

                              .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("ADDRESS OF ESTABLISHMENT: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())

                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("MONTH: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText(Month.ToString("MMM-yyyy"))

                             .SetFontSize(11);
                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Range(2, 1, 4, 43).Merge();
                    worksheet.Range(2, 1, 4, 43).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Row(2).Height = 20;
                    worksheet.Row(3).Height = 20;
                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_FormQ", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    int currentRow = 5;
                    worksheet.Cell(currentRow, 1).Value = "SR NO";
                    worksheet.Range(currentRow, 1, currentRow + 1, 1).Merge();

                    worksheet.Cell(currentRow, 2).Value = "EMP ID";
                    worksheet.Range(currentRow, 2, currentRow + 1, 2).Merge();

                    worksheet.Cell(currentRow, 3).Value = "FULLNAME OF THE EMPLOYEE";
                    worksheet.Range(currentRow, 3, currentRow + 1, 3).Merge();

                    worksheet.Cell(currentRow, 4).Value = "SEX";
                    worksheet.Range(currentRow, 4, currentRow + 1, 4).Merge();

                    worksheet.Cell(currentRow, 5).Value = "AGE";
                    worksheet.Range(currentRow, 5, currentRow + 1, 5).Merge();

                    worksheet.Cell(currentRow, 6).Value = "DATE OF ENTRY";
                    worksheet.Range(currentRow, 6, currentRow + 1, 6).Merge();

                    worksheet.Cell(currentRow, 7).Value = "WORKING HOURS";
                    worksheet.Range(currentRow, 7, currentRow, 8).Merge();

                    worksheet.Cell(currentRow, 9).Value = "INTERVAL FOR REST";
                    worksheet.Range(currentRow, 9, currentRow, 10).Merge();

                    worksheet.Cell(currentRow, 11).Value = "NATURE OF WORK & DESIGNATION";
                    worksheet.Range(currentRow, 11, currentRow + 1, 11).Merge();

                    worksheet.Cell(currentRow, 12).Value = "DATE OF THE MONTH";
                    worksheet.Range(currentRow, 12, currentRow, 42).Merge();

                    worksheet.Cell(currentRow, 43).Value = "Total Days Worked";
                    worksheet.Range(currentRow, 43, currentRow + 1, 43).Merge();

                    // Sub-headers row
                    currentRow++;
                    worksheet.Cell(currentRow, 7).Value = "From";
                    worksheet.Cell(currentRow, 8).Value = "To";
                    worksheet.Cell(currentRow, 9).Value = "From";
                    worksheet.Cell(currentRow, 10).Value = "To";
                    for (int day = 1; day <= 31; day++)
                    {
                        worksheet.Cell(currentRow, 11 + day).Value = day.ToString();
                    }

                    var headerRange = worksheet.Range(5, 1, currentRow, 43);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    currentRow++;

                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            var cell = worksheet.Cell(currentRow + r, 1 + c);

                            if (c == 6 || c == 7 || c == 8 || c == 9)
                            {
                                cell.SetValue<string>(dt.Rows[r][c]?.ToString());
                                cell.DataType = XLDataType.Text;
                                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            }

                            else
                            {
                                cell.Value = dt.Rows[r][c];

                            }
                        }
                    }

                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(5, 1, currentRow, 43);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Columns().AdjustToContents();
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.ColumnNumber() != 3 && column.ColumnNumber() != 43)
                        {
                            if (column.Width > 22) column.Width = 22;
                        }
                    }
                    worksheet.Column(3).Width = 30;
                    worksheet.Column(43).Width = 15;
                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);


                }
                #endregion


                #region Form C

                if (FormId == 7)///// Form C
                {
                    var worksheet = workbook.Worksheets.Add("FormCReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 6).Merge();
                    worksheet.Range(1, 1, 1, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    //worksheet.Cell(1, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //worksheet.Cell(1, 1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;
                    worksheet.SheetView.FreezeRows(1);
                    var richTextInfo = worksheet.Cell(2, 1).RichText;
                    richTextInfo.AddText("NAME OF ESTABLISHMENT: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())

                              .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("ADDRESS OF ESTABLISHMENT: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())

                               .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("MLWF NO: ")
                               .SetBold()
                               .SetFontSize(11);


                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    //worksheet.Cell(2, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //worksheet.Cell(2, 1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(2, 1, 4, 6).Merge();
                    worksheet.Range(2, 1, 4, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Row(2).Height = 20;
                    worksheet.Row(3).Height = 20;

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_FormC", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    int currentRow = 5;
                    worksheet.Cell(currentRow, 1).Value = "SR NO";
                    worksheet.Cell(currentRow, 2).Value = "Particular";
                    worksheet.Cell(currentRow, 3).Value = "Quarter ending 31st March";
                    worksheet.Cell(currentRow, 4).Value = "Quarter ending 30th June";
                    worksheet.Cell(currentRow, 5).Value = "Quarter ending 30th September";
                    worksheet.Cell(currentRow, 6).Value = "Quarter ending 31st December";

                    // Apply style only to row 5
                    var headerRange = worksheet.Range(5, 1, 5, 6);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Static rows
                    worksheet.Cell(6, 1).Value = "1";
                    worksheet.Cell(6, 2).Value = "Total Realizations under fines";

                    worksheet.Cell(7, 1).Value = "2";
                    worksheet.Cell(7, 2).Value = "Total amount becoming unpaid accumulations of";
                    worksheet.Cell(7, 3).Value = "NIL";
                    worksheet.Cell(7, 4).Value = "NIL";
                    worksheet.Cell(7, 5).Value = "NIL";
                    worksheet.Cell(7, 6).Value = "NIL";
                    worksheet.Cell(8, 2).Value = "(i) Basic Wages";
                    worksheet.Cell(9, 2).Value = "(ii)Overtime";
                    worksheet.Cell(10, 2).Value = "(iii)Dearness allowance and other allowances";
                    worksheet.Cell(11, 2).Value = "Total";

                    worksheet.Cell(11, 2).Style.Font.Bold = true;
                    worksheet.Cell(7, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(7, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(7, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(7, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    int startRow = 6;
                    int[] quarterColumns = new int[] { 3, 4, 5, 6 };
                    for (int i = 0; i < quarterColumns.Length; i++)
                    {
                        int col = quarterColumns[i];

                        if (i < dt.Rows.Count)
                        {
                            var row = dt.Rows[i];
                            worksheet.Cell(startRow, col).Value = row["EmployeeCount"];
                            worksheet.Cell(8, col).Value = row["TotalBasic"];
                            worksheet.Cell(9, col).Value = row["TotalOT"];
                            worksheet.Cell(10, col).Value = row["TotalDA"];
                        }
                        worksheet.Cell(6, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(8, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(9, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(10, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }

                    foreach (var col in quarterColumns)
                    {
                        double sum = 0;
                        for (int r = 8; r <= 10; r++)
                        {

                            var cellValue = worksheet.Cell(r, col).GetValue<double>();
                            sum += cellValue;
                        }

                        worksheet.Cell(11, col).Value = sum;
                        worksheet.Cell(11, col).Style.Font.Bold = true;
                        worksheet.Cell(11, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(5, 1, 11, 6);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Columns().AdjustToContents();
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.ColumnNumber() != 2)
                        {
                            if (column.Width > 22) column.Width = 22;
                        }
                    }
                    worksheet.Column(2).Width = 45;

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);

                }
                #endregion

                #region Form XX

                if (FormId == 8)///// Form XX
                {
                    var worksheet = workbook.Worksheets.Add("FormXXReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 13).Merge();
                    worksheet.Range(1, 1, 1, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;
                    worksheet.SheetView.FreezeRows(1);
                    var richTextInfo = worksheet.Cell(2, 1).RichText;

                    richTextInfo.AddText("NAME OF ESTABLISHMENT: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())

                              .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("ADDRESS OF ESTABLISHMENT: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())

                               .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("MONTH: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText(Month.ToString("MMM-yyyy"))

                             .SetFontSize(11);

                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(2, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(2, 1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(2, 1, 4, 13).Merge();
                    worksheet.Range(2, 1, 4, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).Height = 20;
                    worksheet.Row(3).Height = 20;
                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_FormXX", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 5;
                    worksheet.Cell(currentRow, 1).Value = "Sr.No";
                    worksheet.Range(currentRow, 1, currentRow + 1, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Name of workman ";
                    worksheet.Range(currentRow, 2, currentRow + 1, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Father's/Husband's name ";
                    worksheet.Range(currentRow, 3, currentRow + 1, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Designation/Nature of employment";
                    worksheet.Range(currentRow, 4, currentRow + 1, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Particulars of damage or loss";
                    worksheet.Range(currentRow, 5, currentRow + 1, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Date of Damage or Loss";
                    worksheet.Range(currentRow, 6, currentRow + 1, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Whether work man showed cause against deduction";
                    worksheet.Range(currentRow, 7, currentRow + 1, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Name of person In whose presence employee's  explanation was heard ";
                    worksheet.Range(currentRow, 8, currentRow + 1, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Amount of deduction imposed ";
                    worksheet.Range(currentRow, 9, currentRow + 1, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "No. of installments";
                    worksheet.Range(currentRow, 10, currentRow + 1, 10).Merge();
                    worksheet.Cell(currentRow, 11).Value = "Date of recovery ";
                    worksheet.Range(currentRow, 11, currentRow, 12).Merge();
                    worksheet.Cell(currentRow, 13).Value = "Remark";
                    worksheet.Range(currentRow, 13, currentRow + 1, 13).Merge();

                    //subheader
                    currentRow++;
                    worksheet.Cell(currentRow, 11).Value = "First installment";

                    worksheet.Cell(currentRow, 12).Value = "Last installment";

                    var headerRange = worksheet.Range(5, 1, currentRow, 13);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }

                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(5, 1, currentRow, 13);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Columns().AdjustToContents();
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.ColumnNumber() != 2 && column.ColumnNumber() != 3 && column.ColumnNumber() != 5 && column.ColumnNumber() != 6
                             && column.ColumnNumber() != 7 && column.ColumnNumber() != 8 && column.ColumnNumber() != 9 && column.ColumnNumber() != 10 && column.ColumnNumber() != 13)
                        {
                            if (column.Width > 58) column.Width = 58;
                        }
                    }
                    worksheet.Column(2).Width = 38;
                    worksheet.Column(3).Width = 22;
                    worksheet.Column(5).Width = 18;
                    worksheet.Column(6).Width = 22;
                    worksheet.Column(7).Width = 22;
                    worksheet.Column(8).Width = 22;
                    worksheet.Column(9).Width = 22;
                    worksheet.Column(10).Width = 22;
                    worksheet.Column(13).Width = 15;

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form XXI

                if (FormId == 9)///// Form XXI
                {
                    var worksheet = workbook.Worksheets.Add("FormXXIReport");

                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 12).Merge();
                    worksheet.Range(1, 1, 1, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;
                    richTextInfo.AddText("NAME OF ESTABLISHMENT: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("ADDRESS OF ESTABLISHMENT:")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("MONTH: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText(Month.ToString("MMM-yyyy"))
                            .SetFontSize(11);

                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Range(2, 1, 4, 12).Merge();
                    worksheet.Range(2, 1, 4, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).Height = 20;
                    worksheet.Row(3).Height = 20;

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_FormXXI", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // --- Sort by PeriodName then Sr.No ---
                    DataView dv = dt.DefaultView;
                    dv.Sort = "PeriodName ASC, [Sr.No] ASC";
                    DataTable sortedDt = dv.ToTable();
                    int currentRow = 5;
                    Action<int> WriteHeader = (row) =>
                    {
                        worksheet.Cell(row, 1).Value = "Sr.No";
                        worksheet.Cell(row, 2).Value = "Name of workman";
                        worksheet.Cell(row, 3).Value = "Father's/ Husband's name";
                        worksheet.Cell(row, 4).Value = "Designation/Nature of employement";
                        worksheet.Cell(row, 5).Value = "Act / Omission for which fine";
                        worksheet.Cell(row, 6).Value = "Date of offence";
                        worksheet.Cell(row, 7).Value = "Whether workman showed cause against fine";
                        worksheet.Cell(row, 8).Value = "Name of persons in whose presence employees";
                        worksheet.Cell(row, 9).Value = "Wage Periods and wage Payable";
                        worksheet.Cell(row, 10).Value = "Amount of fine imposed";
                        worksheet.Cell(row, 11).Value = "Date on which fine realised";
                        worksheet.Cell(row, 12).Value = "Remarks";

                        var headerRange = worksheet.Range(row, 1, row, 12); // only header row bold
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        headerRange.Style.Alignment.WrapText = true;
                        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    };

                    // --- Write Month-wise Data ---
                    string currentMonth = "";
                    for (int r = 0; r < sortedDt.Rows.Count; r++)
                    {
                        string monthName = sortedDt.Rows[r]["PeriodName"].ToString();
                        if (monthName != currentMonth)
                        {
                            currentMonth = monthName;
                            worksheet.Cell(currentRow, 1).Value = currentMonth;
                            worksheet.Range(currentRow, 1, currentRow, 12).Merge();
                            worksheet.Row(currentRow).Style.Font.Bold = true;
                            currentRow++;
                            WriteHeader(currentRow);
                            currentRow++;
                        }
                        int excelCol = 1;
                        for (int c = 0; c < sortedDt.Columns.Count; c++)
                        {
                            if (sortedDt.Columns[c].ColumnName == "PeriodName") continue;
                            //  int excelCol = (c == 0) ? 0 : c;
                            //    worksheet.Cell(currentRow, 1 + excelCol).Value = sortedDt.Rows[r][c];
                            worksheet.Cell(currentRow, excelCol).Value = sortedDt.Rows[r][c];
                            excelCol++;
                        }
                        currentRow++;

                    }

                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(5, 1, currentRow, 12);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    // --- Styling ---
                    worksheet.Column(2).Width = 27;
                    worksheet.Column(4).Width = 22;
                    worksheet.Columns().AdjustToContents();
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.ColumnNumber() != 2 && column.ColumnNumber() != 4)
                        {
                            if (column.Width > 18) column.Width = 18;
                        }
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);

                }
                #endregion

                #region Form A

                if (FormId == 10)///// Form A
                {
                    var worksheet = workbook.Worksheets.Add("FormAReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 7).Merge();
                    worksheet.Range(1, 1, 1, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;
                    richTextInfo.AddText("NAME OF ESTABLISHMENT: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("ADDRESS OF ESTABLISHMENT:")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("MONTH: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText(Month.ToString("MMM-yyyy"))
                            .SetFontSize(11);

                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Range(2, 1, 4, 7).Merge();
                    worksheet.Range(2, 1, 4, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).Height = 20;
                    worksheet.Row(3).Height = 20;

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_FormA", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // --- Sort by PeriodName then Sr.No ---
                    DataView dv = dt.DefaultView;
                    dv.Sort = "PeriodName ASC, [Sr.No] ASC";
                    DataTable sortedDt = dv.ToTable();
                    int currentRow = 5;
                    Action<int> WriteHeader = (row) =>
                    {
                        worksheet.Cell(row, 1).Value = "Sr.No";
                        worksheet.Cell(row, 2).Value = "Name of workman";
                        worksheet.Cell(row, 3).Value = "Wages for the month for which house rent allowance is payable";

                        worksheet.Cell(row, 4).Value = "House Rent Allowance paid";
                        worksheet.Cell(row, 5).Value = " Mode of Payment";
                        worksheet.Cell(row, 6).Value = " Signature of Workmen";
                        worksheet.Cell(row, 7).Value = "Remarks";

                        var headerRange = worksheet.Range(row, 1, row, 7); // only header row bold
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        headerRange.Style.Alignment.WrapText = true;
                        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    };

                    // --- Write Month-wise Data ---
                    string currentMonth = "";
                    for (int r = 0; r < sortedDt.Rows.Count; r++)
                    {
                        string monthName = sortedDt.Rows[r]["PeriodName"].ToString();
                        if (monthName != currentMonth)
                        {
                            currentMonth = monthName;
                            worksheet.Cell(currentRow, 1).Value = currentMonth;
                            worksheet.Range(currentRow, 1, currentRow, 12).Merge();
                            worksheet.Row(currentRow).Style.Font.Bold = true;
                            currentRow++;
                            WriteHeader(currentRow);
                            currentRow++;
                        }
                        int excelCol = 1;
                        for (int c = 0; c < sortedDt.Columns.Count; c++)
                        {
                            if (sortedDt.Columns[c].ColumnName == "PeriodName") continue;
                            //  int excelCol = (c == 0) ? 0 : c;
                            //    worksheet.Cell(currentRow, 1 + excelCol).Value = sortedDt.Rows[r][c];
                            worksheet.Cell(currentRow, excelCol).Value = sortedDt.Rows[r][c];
                            excelCol++;
                        }
                        currentRow++;

                    }

                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(5, 1, currentRow, 7);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    // --- Styling ---
                    worksheet.Column(2).Width = 27;
                    worksheet.Column(4).Width = 22;
                    worksheet.Columns().AdjustToContents();
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.ColumnNumber() != 2 && column.ColumnNumber() != 4)
                        {
                            if (column.Width > 18) column.Width = 18;
                        }
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);

                }
                #endregion

                #region Form XXIII
                if (FormId == 11)//Form XXIII
                {
                    var worksheet = workbook.Worksheets.Add("FormXXIIIReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    // richText.AddNewLine();
                    //richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                    //        .SetBold(false)
                    //        .SetFontSize(10);
                    //richText.AddNewLine();
                    //richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                    //        .SetBold()
                    //        .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 12).Merge();
                    worksheet.Range(1, 1, 1, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 50;
                    worksheet.Cell(2, 1).Clear();
                    //var richTextInfo = worksheet.Cell(2, 1).RichText;
                    //richTextInfo.AddText("NAME OF ESTABLISHMENT: ")
                    //           .SetBold()
                    //           .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                    //         .SetFontSize(11);
                    //richTextInfo.AddNewLine();
                    //richTextInfo.AddText("ADDRESS OF ESTABLISHMENT:")
                    //           .SetBold()
                    //           .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                    //         .SetFontSize(11);
                    //richTextInfo.AddNewLine();
                    //richTextInfo.AddText("MONTH: ")
                    //           .SetBold()
                    //           .SetFontSize(11);
                    //richTextInfo.AddText(Month.ToString("MMM-yyyy"))
                    //        .SetFontSize(11);
                    //worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    //worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    //worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    //worksheet.Range(2, 1, 4, 10).Merge();
                    //worksheet.Row(2).Height = 20;
                    // worksheet.Row(3).Height = 20;
                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_FormXXIII", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    // int currentRow = 5;
                    int currentRow = 2;
                    worksheet.Cell(currentRow, 1).Value = "Sr.No";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Emp No.";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Name Of Workman";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Gender";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = " Designation Nature Of Employment";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Date on Which Overtime Worked Month";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Total Overtime Worked Or Production In Case Of Piece Rated OT Hrs";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Normal Rate Of Wages";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Overtime Rate Of Wages";
                    worksheet.Range(currentRow, 9, currentRow, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "Overtime Earning";
                    worksheet.Range(currentRow, 10, currentRow, 10).Merge();
                    worksheet.Cell(currentRow, 11).Value = "Date Overtime Paid";
                    worksheet.Range(currentRow, 11, currentRow, 11).Merge();
                    worksheet.Cell(currentRow, 12).Value = "Remarks";
                    worksheet.Range(currentRow, 12, currentRow, 12).Merge();
                    var headerRange = worksheet.Range(2, 1, currentRow, 12);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(2, 1, currentRow, 12);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Column(2).Width = 27;
                    worksheet.Column(3).Width = 26;
                    worksheet.Column(5).Width = 26;
                    worksheet.Columns().AdjustToContents();
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.ColumnNumber() != 2 && column.ColumnNumber() != 3 && column.ColumnNumber() != 5)
                        {
                            if (column.Width > 18) column.Width = 18;
                        }
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form 11
                if (FormId == 12)//Form 11
                {
                    var worksheet = workbook.Worksheets.Add("AccidentBookReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                                .SetBold()
                                .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 13).Merge();
                    worksheet.Range(1, 1, 1, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;
                    // worksheet.SheetView.FreezeRows(1);
                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;
                    richTextInfo.AddText("NAME OF ESTABLISHMENT: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("ADDRESS OF ESTABLISHMENT:")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("MONTH: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText(Month.ToString("MMM-yyyy"))
                            .SetFontSize(11);
                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Range(2, 1, 4, 13).Merge();
                    worksheet.Range(1, 1, 4, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).Height = 20;
                    worksheet.Row(3).Height = 20;
                    // worksheet.SheetView.FreezeRows(4);
                    // worksheet.SheetView.FreezeRows(5);
                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_Form11", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 5;
                    //worksheet.Cell(currentRow, 1).Value = "Sr.No";
                    //worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 1).Value = "Date Of Notice";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Time Of Notice";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = " Name and Address of Injured Person";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = " Sex";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Age";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = " Insurance No.";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Shift, department and Occupation of the employee";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = " Details of Injury";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "What exactly was the injured person doing at the time of accident";
                    worksheet.Range(currentRow, 9, currentRow, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "Name, occupation, address and signature or the thumb impression of the person(s) giving notice";
                    worksheet.Range(currentRow, 10, currentRow, 10).Merge();
                    worksheet.Cell(currentRow, 11).Value = "Signature and designation of the person who makes the entry in the Accident Book";
                    worksheet.Range(currentRow, 11, currentRow, 11).Merge();
                    worksheet.Cell(currentRow, 12).Value = " Name, address and occupation of two witnesses";
                    worksheet.Range(currentRow, 12, currentRow, 12).Merge();
                    worksheet.Cell(currentRow, 13).Value = " Remarks, if any";
                    worksheet.Range(currentRow, 13, currentRow, 13).Merge();
                    var headerRange = worksheet.Range(5, 1, currentRow, 13);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(5, 1, currentRow, 13);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    //worksheet.Columns().AdjustToContents();
                    worksheet.Column(1).Width = 27;
                    worksheet.Column(2).Width = 26;
                    worksheet.Column(4).Width = 26;
                    worksheet.Columns().AdjustToContents();
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.ColumnNumber() != 2 && column.ColumnNumber() != 3 && column.ColumnNumber() != 5)
                        {
                            if (column.Width > 18) column.Width = 18;
                        }
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                # region FormVIII
                if (FormId == 13)//Form VIII
                {
                    var worksheet = workbook.Worksheets.Add("FormVIIIReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText("Part I")
                         .SetBold()
                         .SetFontSize(12);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 9).Merge();
                    worksheet.Range(1, 1, 1, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();
                    var TextInfo = worksheet.Cell(2, 1).RichText;
                    TextInfo.AddText("Name and address of the Principal Employer : ")
                               .SetBold()
                               .SetFontSize(11);
                    TextInfo.AddNewLine();
                    TextInfo.AddText("Name and Address of the Establishment : ")
                    .SetBold()
                    .SetFontSize(11);
                    string branchName = (Session["BranchName"] ?? "").ToString().Trim();
                    string branchAddress = (Session["BranchAddress"] ?? "").ToString().Trim();
                    TextInfo.AddText(branchName + "(" + branchAddress + ")")
                    .SetFontSize(11);
                    worksheet.Range(2, 1, 2, 9).Merge();
                    worksheet.Range(2, 1, 2, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Row(2).Height = 40;
                    // worksheet.Row(3).Height = 20;

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);

                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_FormVIII", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 4;
                    worksheet.Cell(currentRow, 4).Value = "Period of contract";
                    worksheet.Range(currentRow, 4, currentRow, 5).Merge();

                    // Sub-headers
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = "Sr.No";
                    worksheet.Cell(currentRow, 2).Value = "Name and Address of contractor";
                    worksheet.Cell(currentRow, 3).Value = "Name of work contract";
                    worksheet.Cell(currentRow, 4).Value = "Location of contract work";
                    worksheet.Cell(currentRow, 5).Value = "From";
                    worksheet.Cell(currentRow, 6).Value = "To";
                    worksheet.Cell(currentRow, 7).Value = "Amount/Value of contract work";
                    worksheet.Cell(currentRow, 8).Value = "Maximum NO. of workmen employed by contractor";
                    worksheet.Cell(currentRow, 9).Value = "Security deposits with the Principal Emp-loyer";

                    var headerRange = worksheet.Range(4, 1, currentRow, 9);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // ---------- Fill first table data ----------
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }

                    currentRow += dt.Rows.Count;
                    currentRow++;

                    worksheet.Range(currentRow, 1, currentRow, 4).Merge();
                    var headingCell = worksheet.Cell(currentRow, 1).RichText;
                    headingCell.AddText("Part II ")
                               .SetBold()
                               .SetFontSize(11);
                    headingCell.AddNewLine();
                    headingCell.AddText("Progress of Contract Work")
                               .SetBold()
                               .SetFontSize(11);
                    worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(currentRow, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(currentRow).Height = 50;
                    currentRow++;
                    worksheet.Range(currentRow, 1, currentRow, 4).Merge();
                    var detailsCell = worksheet.Cell(currentRow, 1).RichText;
                    detailsCell.AddText("NAME OF contractor :                               ")
                               .SetBold()
                               .SetFontSize(11);
                    detailsCell.AddText("Nature of work: ")
                               .SetBold()
                               .SetFontSize(11);

                    worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(currentRow, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(currentRow).Height = 20;

                    currentRow += 2;
                    int tableStartRow = currentRow;
                    worksheet.Cell(tableStartRow, 1).Value = "Wage PERIOD";
                    worksheet.Cell(tableStartRow, 2).Value = "Maximum number of workmen employed by the contractor during the wage period";
                    worksheet.Cell(tableStartRow, 3).Value = "Total amount of wages earned by the workmen";
                    worksheet.Cell(tableStartRow, 4).Value = "Amount actually disbursed on pay day";

                    var headerRange1 = worksheet.Range(tableStartRow, 1, tableStartRow, 4);
                    headerRange1.Style.Font.Bold = true;
                    headerRange1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange1.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange1.Style.Alignment.WrapText = true;
                    headerRange1.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange1.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    tableStartRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < 4; c++)
                        {
                            worksheet.Cell(tableStartRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }

                    worksheet.Columns().AdjustToContents();
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.Width > 40) column.Width = 40;
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form20
                if (FormId == 14)//Form 20
                {
                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {
                        var FormRegister1 = @"
                      SELECT Mas_Employee.EmployeeName AS Name,
                      Mas_Employee_Statutory.PF_FS_Name As [FatherName],
                      Mas_Department.DepartmentName as [Department]
                      FROM Mas_Employee 
                      LEFT JOIN Mas_Employee_Statutory  ON Mas_Employee.EmployeeId = Mas_Employee_Statutory.StatutoryEmployeeId
                      LEFT JOIN Mas_Branch  ON Mas_Branch.BranchId = Mas_Employee.EmployeeBranchId
                      LEFT JOIN Mas_CompanyProfile  ON Mas_CompanyProfile.CompanyId =Mas_Employee.CmpID
                      LEFT JOIN Mas_Department  ON Mas_Department.DepartmentId =Mas_Employee.EmployeeDepartmentID
                      WHERE Mas_Employee.CmpID = '" + CmpId + @"'
                      AND Mas_Employee.EmployeeBranchId = '" + BranchId + @"'
                       AND Mas_employee.EmployeeId='" + Session["EmployeeId"] + @"'
                      GROUP BY Mas_Employee.EmployeeName, Mas_Employee_Statutory.PF_FS_Name,  Mas_Department.DepartmentName";
                        var Form1 = DapperORM.DynamicQuerySingle(FormRegister1);

                        if (Form1 != null)
                        {
                            Session["EmployeeName"] = Form1.Name;
                            Session["FatherName"] = Form1.FatherName;
                            Session["Department"] = Form1.Department;

                        }
                    }

                    var worksheet = workbook.Worksheets.Add("Form20Report");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 22).Merge();
                    worksheet.Range(1, 1, 1, 22).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;
                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;
                    var rt1 = worksheet.Range(2, 1, 2, 22).Merge().FirstCell().RichText;
                    worksheet.Range(2, 1, 2, 22).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    rt1.AddText("Factory: ").SetBold().SetFontSize(11);
                    rt1.AddText("".PadRight(70));

                    rt1.AddText("Part I-Adult ").SetBold().SetFontSize(11);
                    rt1.AddText("".PadRight(70));

                    rt1.AddText("Name: ").SetBold().SetFontSize(11);
                    rt1.AddText((Session["EmployeeName"] ?? "").ToString().Trim()).SetFontSize(11);
                    var rt2 = worksheet.Range(3, 1, 3, 22).Merge().FirstCell().RichText;
                    worksheet.Range(3, 1, 3, 22).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    rt2.AddText("Department: ").SetBold().SetFontSize(11);
                    rt2.AddText((Session["Department"] ?? "").ToString().Trim()).SetFontSize(11);
                    rt2.AddText("".PadRight(40));
                    rt2.AddText("Part II-Children ").SetBold().SetFontSize(11);
                    rt2.AddText("".PadRight(60));
                    rt2.AddText("Father Name: ").SetBold().SetFontSize(11);
                    rt2.AddText((Session["FatherName"] ?? "").ToString().Trim()).SetFontSize(11);

                    var rt3 = worksheet.Range(4, 1, 4, 22).Merge().FirstCell().RichText;
                    worksheet.Range(4, 1, 4, 22).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    rt3.AddText("Number of days worked during calendar year ").SetBold().SetFontSize(11);
                    rt3.AddText("".PadRight(45));
                    rt3.AddText("Leave with wages to credit ").SetBold().SetFontSize(11);

                    worksheet.Row(2).Height = 20;
                    worksheet.Row(3).Height = 20;
                    worksheet.Row(4).Height = 30;

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_FormVIII", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 5;
                    worksheet.Cell(currentRow, 1).Value = "Sr.No";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Sr. No. in the Register of adults/child workers";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Date of entry into Service";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Calendar year of service";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Number of days of work performed";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Number of days lay-off";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Number of days of maternity leave with wages";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Number of days leave with wages enjoyed";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Total cols. 5 to 8";
                    worksheet.Range(currentRow, 9, currentRow, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "Balance of leave with wages from preceding year";
                    worksheet.Range(currentRow, 10, currentRow, 10).Merge();
                    worksheet.Cell(currentRow, 11).Value = "Leave with wages earned during the year mentioned in col. 4";
                    worksheet.Range(currentRow, 11, currentRow, 11).Merge();
                    worksheet.Cell(currentRow, 12).Value = "Total of cols. 10 & 11";
                    worksheet.Range(currentRow, 12, currentRow, 12).Merge();
                    worksheet.Cell(currentRow, 13).Value = "Whether leave with wages refused in accordance with scheme under Sec. 79(8)";
                    worksheet.Range(currentRow, 13, currentRow, 13).Merge();
                    worksheet.Cell(currentRow, 14).Value = "Whether leave with wages not desired during the next calendar year";
                    worksheet.Range(currentRow, 14, currentRow, 14).Merge();
                    worksheet.Cell(currentRow, 15).Value = "Leave with wages enjoyed From To";
                    worksheet.Range(currentRow, 15, currentRow, 15).Merge();
                    worksheet.Cell(currentRow, 16).Value = "Balance to credit";
                    worksheet.Range(currentRow, 16, currentRow, 16).Merge();
                    worksheet.Cell(currentRow, 17).Value = "Normal rate of wages";
                    worksheet.Range(currentRow, 17, currentRow, 17).Merge();
                    worksheet.Cell(currentRow, 18).Value = "Cash equivalent or advantage accruing through concessional sale of foodgrains or other articles";
                    worksheet.Range(currentRow, 18, currentRow, 18).Merge();
                    worksheet.Cell(currentRow, 19).Value = "Rate of wages for leave with wages period (total of cols. 17 & 18)";
                    worksheet.Range(currentRow, 19, currentRow, 19).Merge();
                    worksheet.Cell(currentRow, 20).Value = "Date of discharge";
                    worksheet.Range(currentRow, 20, currentRow, 20).Merge();
                    worksheet.Cell(currentRow, 21).Value = "Date of amount of payment made in lieu of leave with wages due";
                    worksheet.Range(currentRow, 21, currentRow, 21).Merge();
                    worksheet.Cell(currentRow, 22).Value = "Remarks";
                    worksheet.Range(currentRow, 22, currentRow, 22).Merge();
                    var headerRange = worksheet.Range(5, 1, currentRow, 22);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(5, 1, currentRow, 10);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    //worksheet.Columns().AdjustToContents();
                    //worksheet.Column(2).Width = 27;
                    //worksheet.Column(3).Width = 26;
                    worksheet.Columns().AdjustToContents();
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        //if (column.ColumnNumber() != 2 && column.ColumnNumber() != 3)
                        //{
                        if (column.Width > 18) column.Width = 18;
                        //  }
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form10( Register of workers attending to machinery)
                if (FormId == 15)//Form  10
                {
                    var worksheet = workbook.Worksheets.Add("Form10Report");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 7).Merge();
                    worksheet.Range(1, 1, 1, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;
                    worksheet.Cell(2, 1).Clear();

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_Form10_RegWorker", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 2;
                    worksheet.Cell(currentRow, 1).Value = "No";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Name of the  worker";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "father’s name of worker";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Designation or nature of work";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Department";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Date when tight clothes provided";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();

                    worksheet.Cell(currentRow, 7).Value = "Signature or thumb impression of worker";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();


                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    var headerRange = worksheet.Range(2, 1, currentRow, 7);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(2, 1, currentRow, 7);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    //worksheet.Columns().AdjustToContents();
                    //worksheet.Column(2).Width = 27;
                    //worksheet.Column(3).Width = 26;
                    worksheet.Columns().AdjustToContents();
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        //if (column.ColumnNumber() != 2 && column.ColumnNumber() != 3)
                        //{
                        if (column.Width > 22) column.Width = 22;
                        //  }
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form17
                if (FormId == 16)//Form  17
                {
                    var worksheet = workbook.Worksheets.Add("Form17Report");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 10).Merge();
                    worksheet.Range(1, 1, 1, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;
                    worksheet.Cell(2, 1).Clear();

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_Form10_RegWorker", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 2;
                    worksheet.Cell(currentRow, 1).Value = "No";
                    worksheet.Range(currentRow, 1, currentRow + 1, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Name";
                    worksheet.Range(currentRow, 2, currentRow + 1, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Residential Address";
                    worksheet.Range(currentRow, 3, currentRow + 1, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Father’s Name";
                    worksheet.Range(currentRow, 4, currentRow + 1, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Nature of Work";
                    worksheet.Range(currentRow, 5, currentRow + 1, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Letter of group as in Form 16";
                    worksheet.Range(currentRow, 6, currentRow + 1, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Number of relay if working in shifts";
                    worksheet.Range(currentRow, 7, currentRow + 1, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Number and date of certificate if an adolescent";
                    worksheet.Range(currentRow, 8, currentRow, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "Remarks";
                    worksheet.Range(currentRow, 10, currentRow + 1, 10).Merge();

                    //subheader
                    currentRow++;
                    worksheet.Cell(currentRow, 8).Value = "Number of certificate and date";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Token number giving Reference to the certificate";
                    worksheet.Range(currentRow, 9, currentRow, 9).Merge();

                    var headerRange = worksheet.Range(2, 1, currentRow, 10);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(2, 1, currentRow, 10);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    //worksheet.Columns().AdjustToContents();
                    //worksheet.Column(2).Width = 27;
                    //worksheet.Column(3).Width = 26;
                    worksheet.Columns().AdjustToContents();
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        //if (column.ColumnNumber() != 2 && column.ColumnNumber() != 3)
                        //{
                        if (column.Width > 30) column.Width = 30;
                        //  }
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form19
                if (FormId == 17)//Form  19
                {
                    var worksheet = workbook.Worksheets.Add("Form19Report");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 10).Merge();
                    worksheet.Range(1, 1, 1, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;
                    worksheet.Cell(2, 1).Clear();

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_Form10_RegWorker", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 2;
                    worksheet.Cell(currentRow, 1).Value = "No";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Name";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Residential Address";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Father’s Name";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Date of First Employment";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Number of certificate and its date";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Token number giving reference to certificate";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Letter of Group as in Form 18";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Number of relay, if working in shifts";
                    worksheet.Range(currentRow, 9, currentRow, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "Remarks";
                    worksheet.Range(currentRow, 10, currentRow, 10).Merge();

                    var headerRange = worksheet.Range(2, 1, currentRow, 10);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(2, 1, currentRow, 10);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    //worksheet.Columns().AdjustToContents();
                    //worksheet.Column(2).Width = 27;
                    //worksheet.Column(3).Width = 26;
                    worksheet.Columns().AdjustToContents();
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        //if (column.ColumnNumber() != 2 && column.ColumnNumber() != 3)
                        //{
                        if (column.Width > 27) column.Width = 27;
                        //  }
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form30
                if (FormId == 18)//Form 30
                {
                    var worksheet = workbook.Worksheets.Add("Form30Report");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 6).Merge();
                    worksheet.Range(1, 1, 1, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;
                    worksheet.Cell(2, 1).Clear();

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_Form10_RegWorker", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 2;
                    worksheet.Cell(currentRow, 1).Value = "Name of injured person(if any) ";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Date of accident or dangerous occurrence ";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Date of Report(or Form 24) to Inspector ";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Nature of accident or dangerous occurrence ";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Date of return of injured person to work";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Number of days injured person was absent from work";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();


                    var headerRange = worksheet.Range(2, 1, currentRow, 6);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(2, 1, currentRow, 6);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    //worksheet.Columns().AdjustToContents();
                    //worksheet.Column(2).Width = 27;
                    //worksheet.Column(3).Width = 26;
                    worksheet.Columns().AdjustToContents();
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        //if (column.ColumnNumber() != 2 && column.ColumnNumber() != 3)
                        //{
                        if (column.Width > 30) column.Width = 30;
                        //  }
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion


                #endregion

                #region KARNATAKA fORMS

                #region Form T(Combined Muster Roll Cum Register)
                if (FormId == 20)//Form  T(Combined Muster Roll Cum Register)
                {

                    var worksheet = workbook.Worksheets.Add("FormTReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 72).Merge();
                    worksheet.Range(1, 1, 1, 72).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;
                    //worksheet.SheetView.FreezeRows(1);
                    var richTextInfo = worksheet.Cell(2, 1).RichText;
                    richTextInfo.AddText("NAME OF ESTABLISHMENT: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())

                              .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("ADDRESS OF ESTABLISHMENT: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())

                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("MONTH: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText(Month.ToString("MMM-yyyy"))

                             .SetFontSize(11);
                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Range(2, 1, 4, 72).Merge();
                    worksheet.Range(2, 1, 4, 72).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Row(2).Height = 20;
                    worksheet.Row(3).Height = 20;
                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_FormCombinedMusterRoll", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    int currentRow = 5;
                    worksheet.Cell(currentRow, 1).Value = "SR NO";
                    worksheet.Range(currentRow, 1, currentRow + 2, 1).Merge();

                    worksheet.Cell(currentRow, 2).Value = "EMP ID";
                    worksheet.Range(currentRow, 2, currentRow + 2, 2).Merge();

                    worksheet.Cell(currentRow, 3).Value = "NAME OF THE EMPLOYEE";
                    worksheet.Range(currentRow, 3, currentRow + 2, 3).Merge();

                    worksheet.Cell(currentRow, 4).Value = "Father/Husband Name";
                    worksheet.Range(currentRow, 4, currentRow + 2, 4).Merge();

                    worksheet.Cell(currentRow, 5).Value = "MALE/FEMALE";
                    worksheet.Range(currentRow, 5, currentRow + 2, 5).Merge();

                    worksheet.Cell(currentRow, 6).Value = "Designation/Department";
                    worksheet.Range(currentRow, 6, currentRow + 2, 6).Merge();

                    worksheet.Cell(currentRow, 7).Value = "Date Of Joining";
                    worksheet.Range(currentRow, 7, currentRow + 2, 7).Merge();

                    worksheet.Cell(currentRow, 8).Value = "ESI No";
                    worksheet.Range(currentRow, 8, currentRow + 2, 8).Merge();

                    worksheet.Cell(currentRow, 9).Value = "PF NO";
                    worksheet.Range(currentRow, 9, currentRow + 2, 9).Merge();

                    worksheet.Cell(currentRow, 10).Value = "Wages fixed including VDA";
                    worksheet.Range(currentRow, 10, currentRow + 2, 10).Merge();

                    worksheet.Cell(currentRow, 11).Value = " ";
                    worksheet.Range(currentRow, 11, currentRow, 41).Merge();


                    worksheet.Cell(currentRow, 42).Value = "Date of suspension if any";
                    worksheet.Range(currentRow, 42, currentRow + 2, 42).Merge();

                    worksheet.Cell(currentRow, 43).Value = "No.of payable Days";
                    worksheet.Range(currentRow, 43, currentRow + 2, 43).Merge();

                    worksheet.Cell(currentRow, 44).Value = "Total OT hours worked";
                    worksheet.Range(currentRow, 44, currentRow + 2, 44).Merge();
                    //worksheet.Cell(currentRow, 45).Value = "Earned wage and other allowances";
                    //worksheet.Range(currentRow, 45, currentRow, 58).Merge();
                    //worksheet.Cell(currentRow, 59).Value = "Deductions";
                    //worksheet.Range(currentRow, 59, currentRow, 69).Merge();

                    worksheet.Cell(currentRow, 45).Value = "MONTH/YEAR";
                    worksheet.Range(currentRow, 45, currentRow, 69).Merge();

                    worksheet.Cell(currentRow, 70).Value = "Net Payable";
                    worksheet.Range(currentRow, 70, currentRow + 2, 70).Merge();
                    worksheet.Cell(currentRow, 71).Value = "Mode of payment Cash / Cheque No.";
                    worksheet.Range(currentRow, 71, currentRow + 2, 71).Merge();
                    worksheet.Cell(currentRow, 72).Value = "Employee Signature/Thumb Impression";
                    worksheet.Range(currentRow, 72, currentRow + 2, 72).Merge();



                    // -------------------- SUB HEADERS --------------------
                    currentRow++;   // now row 6
                    worksheet.Cell(currentRow, 11).Value = "DATE OF THE MONTH";
                    worksheet.Range(currentRow, 11, currentRow, 41).Merge();
                    worksheet.Cell(currentRow, 45).Value = "Earned wage and other allowances";
                    worksheet.Range(currentRow, 45, currentRow, 58).Merge();
                    worksheet.Cell(currentRow, 59).Value = "Deductions";
                    worksheet.Range(currentRow, 59, currentRow, 69).Merge();


                    // Sub-headers row
                    currentRow++;

                    for (int day = 1; day <= 31; day++)
                    {
                        worksheet.Cell(currentRow, 10 + day).Value = day.ToString();
                    }
                    worksheet.Cell(currentRow, 45).Value = "Basic";
                    worksheet.Cell(currentRow, 46).Value = "DA/VDA";
                    worksheet.Cell(currentRow, 47).Value = "HRA";
                    worksheet.Cell(currentRow, 48).Value = "Conveyance";
                    worksheet.Cell(currentRow, 49).Value = "Med. Allowance";
                    worksheet.Cell(currentRow, 50).Value = "bonus";
                    worksheet.Cell(currentRow, 51).Value = "Special Allow.";
                    worksheet.Cell(currentRow, 52).Value = "OT";
                    worksheet.Cell(currentRow, 53).Value = "NFH";
                    worksheet.Cell(currentRow, 54).Value = "Maternity Benefit";
                    worksheet.Cell(currentRow, 55).Value = "Others";
                    worksheet.Cell(currentRow, 56).Value = "Subsistance allowance if any ";
                    worksheet.Cell(currentRow, 57).Value = "Leave Encashment";
                    worksheet.Cell(currentRow, 58).Value = "GROSS TOTAL";
                    worksheet.Cell(currentRow, 59).Value = "ESI";
                    worksheet.Cell(currentRow, 60).Value = "EPF";
                    worksheet.Cell(currentRow, 61).Value = "PT";
                    worksheet.Cell(currentRow, 62).Value = "TDS(Income Tax)";
                    worksheet.Cell(currentRow, 63).Value = "Society";
                    worksheet.Cell(currentRow, 64).Value = "Insurence";
                    worksheet.Cell(currentRow, 65).Value = "Sal.Adv";
                    worksheet.Cell(currentRow, 66).Value = "Fines";
                    worksheet.Cell(currentRow, 67).Value = "Damage/Loss";
                    worksheet.Cell(currentRow, 68).Value = "Other";
                    worksheet.Cell(currentRow, 69).Value = "Total";

                    var headerRange = worksheet.Range(5, 1, currentRow, 72);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    currentRow++;

                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            var cell = worksheet.Cell(currentRow + r, 1 + c);

                            //if (c == 6 || c == 7 || c == 8 || c == 9)
                            //{
                            //    cell.SetValue<string>(dt.Rows[r][c]?.ToString());
                            //    cell.DataType = XLDataType.Text;
                            //    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            //}

                            //else
                            //{
                            //    cell.Value = dt.Rows[r][c];

                            //}
                        }
                    }

                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(5, 1, currentRow, 72);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Columns().AdjustToContents();
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        //if (column.ColumnNumber() != 3 && column.ColumnNumber() != 43)
                        //{
                        if (column.Width > 22) column.Width = 22;
                        //}
                    }
                    //worksheet.Column(3).Width = 30;
                    //worksheet.Column(43).Width = 15;
                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form IV( Register of Overtime)
                if (FormId == 21)////Form IV( Register of Overtime)
                {
                    var worksheet = workbook.Worksheets.Add("FormIVReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 16).Merge();
                    worksheet.Range(1, 1, 1, 16).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;
                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;
                    //richTextInfo.AddText("NAME OF ESTABLISHMENT: ")
                    //           .SetBold()
                    //           .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                    //         .SetFontSize(11);
                    //richTextInfo.AddNewLine();
                    //richTextInfo.AddText("ADDRESS OF ESTABLISHMENT:")
                    //           .SetBold()
                    //           .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                    //         .SetFontSize(11);
                    //richTextInfo.AddNewLine();
                    richTextInfo.AddText("MONTH: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText(Month.ToString("MMM-yyyy"))
                            .SetFontSize(11);
                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Range(2, 1, 2, 16).Merge();
                    worksheet.Range(2, 1, 2, 16).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).Height = 20;
                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 3;
                    worksheet.Cell(currentRow, 1).Value = "Sr.No";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Name";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = " Father's/Husband's Name";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Sex";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Department";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Designation";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Date on which overtime worked";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Extent of overtime on each occasion";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Total overtime worked or production in case of piece workers";
                    worksheet.Range(currentRow, 9, currentRow, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "Normal hours";
                    worksheet.Range(currentRow, 10, currentRow, 10).Merge();
                    worksheet.Cell(currentRow, 11).Value = "Normal rate";
                    worksheet.Range(currentRow, 11, currentRow, 11).Merge();
                    worksheet.Cell(currentRow, 12).Value = "Overtime rate";
                    worksheet.Range(currentRow, 12, currentRow, 12).Merge();
                    worksheet.Cell(currentRow, 13).Value = "Normal earning";
                    worksheet.Range(currentRow, 13, currentRow, 13).Merge();
                    worksheet.Cell(currentRow, 14).Value = "Overtime earning";
                    worksheet.Range(currentRow, 14, currentRow, 14).Merge();
                    worksheet.Cell(currentRow, 15).Value = "Total earning";
                    worksheet.Range(currentRow, 15, currentRow, 15).Merge();
                    worksheet.Cell(currentRow, 16).Value = "Date on which overtime payment made";
                    worksheet.Range(currentRow, 16, currentRow, 16).Merge();

                    var headerRange = worksheet.Range(3, 1, currentRow, 16);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(3, 1, currentRow, 16);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Column(2).Width = 27;
                    worksheet.Column(3).Width = 26;
                    worksheet.Columns().AdjustToContents();
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.ColumnNumber() != 2 && column.ColumnNumber() != 3)
                        {
                            if (column.Width > 18) column.Width = 18;
                        }
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form 13(REGISTER OF CHILD WORKERS)
                if (FormId == 22)////Form 13(REGISTER OF CHILD WORKERS)
                {
                    var worksheet = workbook.Worksheets.Add("Form13Report");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 10).Merge();
                    worksheet.Range(1, 1, 1, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;
                    worksheet.Cell(2, 1).Clear();
                    //var richTextInfo = worksheet.Cell(2, 1).RichText;
                    //richTextInfo.AddText("NAME OF ESTABLISHMENT: ")
                    //           .SetBold()
                    //           .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                    //         .SetFontSize(11);
                    //richTextInfo.AddNewLine();
                    //richTextInfo.AddText("ADDRESS OF ESTABLISHMENT:")
                    //           .SetBold()
                    //           .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                    //         .SetFontSize(11);
                    //richTextInfo.AddNewLine();
                    //richTextInfo.AddText("MONTH: ")
                    //           .SetBold()
                    //           .SetFontSize(11);
                    //richTextInfo.AddText(Month.ToString("MMM-yyyy"))
                    //        .SetFontSize(11);
                    //worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    //worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    //worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    //worksheet.Range(2, 1, 4, 10).Merge();
                    //worksheet.Range(2, 1, 4, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //worksheet.Row(2).Height = 20;
                    //worksheet.Row(3).Height = 20;

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 2;
                    worksheet.Cell(currentRow, 1).Value = "Sr.No";
                    worksheet.Range(currentRow, 1, currentRow + 1, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Name and Residential address of the Child";
                    worksheet.Range(currentRow, 2, currentRow + 1, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Fathers Name";
                    worksheet.Range(currentRow, 3, currentRow + 1, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Date of first Employment ";
                    worksheet.Range(currentRow, 4, currentRow + 1, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "No. of Certificate and  its date";
                    worksheet.Range(currentRow, 5, currentRow + 1, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Token number giving reference to certificate";
                    worksheet.Range(currentRow, 6, currentRow + 1, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Letter of group as in Form 12";
                    worksheet.Range(currentRow, 7, currentRow + 1, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "No. of relay if working in shifts";
                    worksheet.Range(currentRow, 8, currentRow, 9).Merge();

                    worksheet.Cell(currentRow, 10).Value = "Remark";
                    worksheet.Range(currentRow, 10, currentRow + 1, 10).Merge();

                    //subheader
                    currentRow++;
                    worksheet.Cell(currentRow, 8).Value = "No. of relay";
                    worksheet.Cell(currentRow, 9).Value = "Date of Effect";

                    var headerRange = worksheet.Range(2, 1, currentRow, 10);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(2, 1, currentRow, 10);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 10).AdjustToContents();

                    worksheet.Column(2).Width = 30;
                    worksheet.Column(4).Width = 30;
                    worksheet.Column(5).Width = 30;
                    worksheet.Column(6).Width = 30;
                    worksheet.Column(7).Width = 30;
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.ColumnNumber() != 2 && column.ColumnNumber() != 4 && column.ColumnNumber() != 5 && column.ColumnNumber() != 6 && column.ColumnNumber() != 7)
                        {
                            if (column.Width > 28) column.Width = 28;
                        }
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form 11(REGISTER OF ADULT WORKERS)
                if (FormId == 23)////Form 13(REGISTER OF ADULT WORKERS)
                {
                    var worksheet = workbook.Worksheets.Add("Form11Report");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 9).Merge();
                    worksheet.Range(1, 1, 1, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;
                    worksheet.Cell(2, 1).Clear();
                    //var richTextInfo = worksheet.Cell(2, 1).RichText;
                    //richTextInfo.AddText("NAME OF ESTABLISHMENT: ")
                    //           .SetBold()
                    //           .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                    //         .SetFontSize(11);
                    //richTextInfo.AddNewLine();
                    //richTextInfo.AddText("ADDRESS OF ESTABLISHMENT:")
                    //           .SetBold()
                    //           .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                    //         .SetFontSize(11);
                    //richTextInfo.AddNewLine();
                    //richTextInfo.AddText("MONTH: ")
                    //           .SetBold()
                    //           .SetFontSize(11);
                    //richTextInfo.AddText(Month.ToString("MMM-yyyy"))
                    //        .SetFontSize(11);
                    //worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    //worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    //worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    //worksheet.Range(2, 1, 4, 10).Merge();
                    //worksheet.Range(2, 1, 4, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //worksheet.Row(2).Height = 20;
                    //worksheet.Row(3).Height = 20;

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 2;
                    worksheet.Cell(currentRow, 1).Value = "Sr.No";
                    worksheet.Range(currentRow, 1, currentRow + 1, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Name and Residential address of the worker";
                    worksheet.Range(currentRow, 2, currentRow + 1, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Fathers Name";
                    worksheet.Range(currentRow, 3, currentRow + 1, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Nature of work and Department Symbol";
                    worksheet.Range(currentRow, 4, currentRow + 1, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Relay assigned and date of effect";
                    worksheet.Range(currentRow, 5, currentRow + 1, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Certificate of adolescent";
                    worksheet.Range(currentRow, 6, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Date of first Employment in the Factory";
                    worksheet.Range(currentRow, 8, currentRow + 1, 8).Merge();

                    worksheet.Cell(currentRow, 9).Value = "Remark";
                    worksheet.Range(currentRow, 9, currentRow + 1, 9).Merge();

                    //subheader
                    currentRow++;
                    worksheet.Cell(currentRow, 6).Value = "No. of Certificate and date ";
                    worksheet.Cell(currentRow, 7).Value = "Token number giving reference to the certificate";

                    var headerRange = worksheet.Range(2, 1, currentRow, 9);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(2, 1, currentRow, 9);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 9).AdjustToContents();

                    worksheet.Column(2).Width = 30;
                    worksheet.Column(4).Width = 30;
                    worksheet.Column(5).Width = 30;
                    worksheet.Column(8).Width = 30;
                    //worksheet.Column(7).Width = 30;
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.ColumnNumber() != 2 && column.ColumnNumber() != 4 && column.ColumnNumber() != 5 && column.ColumnNumber() != 8)
                        {
                            if (column.Width > 28) column.Width = 28;
                        }
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form 16( HEALTH  REGISTER )
                if (FormId == 24)////Form 16( HEALTH  REGISTER )
                {
                    var worksheet = workbook.Worksheets.Add("Form11Report");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 17).Merge();
                    worksheet.Range(1, 1, 1, 17).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;
                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;
                    richTextInfo.AddText("NAME OF ESTABLISHMENT: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("ADDRESS OF ESTABLISHMENT:")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    //richTextInfo.AddText("MONTH: ")
                    //           .SetBold()
                    //           .SetFontSize(11);
                    //richTextInfo.AddText(Month.ToString("MMM-yyyy"))
                    //        .SetFontSize(11);
                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Range(2, 1, 3, 17).Merge();
                    worksheet.Range(2, 1, 3, 17).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).Height = 40;
                    worksheet.Row(3).Height = 5;

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 4;
                    worksheet.Cell(currentRow, 1).Value = "Sr.No";
                    worksheet.Range(currentRow, 1, currentRow + 1, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Department/Works";
                    worksheet.Range(currentRow, 2, currentRow + 1, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Name of Worker";
                    worksheet.Range(currentRow, 3, currentRow + 1, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Sex";
                    worksheet.Range(currentRow, 4, currentRow + 1, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Age (at last birth day)";
                    worksheet.Range(currentRow, 5, currentRow + 1, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Date of employment on present work";
                    worksheet.Range(currentRow, 6, currentRow + 1, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Date of leaving or transfer to other work-with reasons for discharge or transfer ";
                    worksheet.Range(currentRow, 7, currentRow + 1, 7).Merge();

                    worksheet.Cell(currentRow, 8).Value = "Nature of job or occupation";
                    worksheet.Range(currentRow, 8, currentRow + 1, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Raw materials products or by-products likely to be exposed to";
                    worksheet.Range(currentRow, 9, currentRow + 1, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "Dates of medical examination and the results thereof";
                    worksheet.Range(currentRow, 10, currentRow, 11).Merge();
                    worksheet.Cell(currentRow, 12).Value = "Signs and symptoms observed during examination";
                    worksheet.Range(currentRow, 12, currentRow + 1, 12).Merge();
                    worksheet.Cell(currentRow, 13).Value = "Nature of tests and results thereof";
                    worksheet.Range(currentRow, 13, currentRow + 1, 13).Merge();
                    worksheet.Cell(currentRow, 14).Value = "If declared unfit for work, state period of suspension with reasons in detail";
                    worksheet.Range(currentRow, 14, currentRow + 1, 14).Merge();
                    worksheet.Cell(currentRow, 15).Value = "Whether  certificate of unfitness issued to the worker";
                    worksheet.Range(currentRow, 15, currentRow + 1, 15).Merge();
                    worksheet.Cell(currentRow, 16).Value = "Re-certified fit to resume duty on";
                    worksheet.Range(currentRow, 16, currentRow + 1, 16).Merge();
                    worksheet.Cell(currentRow, 17).Value = "Signature of the Certifying Surgeon with date";
                    worksheet.Range(currentRow, 17, currentRow + 1, 17).Merge();
                    //subheader
                    currentRow++;
                    worksheet.Cell(currentRow, 10).Value = "Dates";
                    worksheet.Cell(currentRow, 11).Value = "Results Fit or Unit";

                    var headerRange = worksheet.Range(4, 1, currentRow, 17);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(4, 1, currentRow, 17);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 17).AdjustToContents();

                    worksheet.Column(6).Width = 30;
                    worksheet.Column(7).Width = 30;
                    worksheet.Column(8).Width = 30;
                    worksheet.Column(9).Width = 30;
                    worksheet.Column(12).Width = 30;
                    worksheet.Column(13).Width = 30;
                    worksheet.Column(14).Width = 30;
                    worksheet.Column(15).Width = 30;
                    worksheet.Column(16).Width = 30;

                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.ColumnNumber() != 6 && column.ColumnNumber() != 7 && column.ColumnNumber() != 8 && column.ColumnNumber() != 9
                        && column.ColumnNumber() != 12 && column.ColumnNumber() != 13 && column.ColumnNumber() != 14 && column.ColumnNumber() != 15
                        && column.ColumnNumber() != 16
                        )
                        {
                            if (column.Width > 28) column.Width = 28;
                        }
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form 11( ACCIDENT BOOK )
                if (FormId == 25)////Form 11( ACCIDENT BOOK )
                {
                    var worksheet = workbook.Worksheets.Add("Form11Report");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 18).Merge();
                    worksheet.Range(1, 1, 1, 18).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;
                    worksheet.Cell(2, 1).Clear();
                    //var richTextInfo = worksheet.Cell(2, 1).RichText;
                    //richTextInfo.AddText("NAME OF ESTABLISHMENT: ")
                    //           .SetBold()
                    //           .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                    //         .SetFontSize(11);
                    //richTextInfo.AddNewLine();
                    //richTextInfo.AddText("ADDRESS OF ESTABLISHMENT:")
                    //           .SetBold()
                    //           .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                    //         .SetFontSize(11);
                    //richTextInfo.AddNewLine();
                    //richTextInfo.AddText("MONTH: ")
                    //           .SetBold()
                    //           .SetFontSize(11);
                    //richTextInfo.AddText(Month.ToString("MMM-yyyy"))
                    //        .SetFontSize(11);
                    //worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    //worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    //worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    //worksheet.Range(2, 1, 3, 18).Merge();
                    //worksheet.Range(2, 1, 3, 18).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //worksheet.Row(2).Height = 40;
                    // worksheet.Row(3).Height = 5;

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 2;
                    worksheet.Cell(currentRow, 1).Value = "Sr.No";
                    worksheet.Range(currentRow, 1, currentRow + 1, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Date of Notice";
                    worksheet.Range(currentRow, 2, currentRow + 1, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Time of Notice";
                    worksheet.Range(currentRow, 3, currentRow + 1, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Name and Address of Injured Person";
                    worksheet.Range(currentRow, 4, currentRow + 1, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Sex";
                    worksheet.Range(currentRow, 5, currentRow + 1, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Age";
                    worksheet.Range(currentRow, 6, currentRow + 1, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = " Insurance No.";
                    worksheet.Range(currentRow, 7, currentRow + 1, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Shift, department and Occupation of the employee";
                    worksheet.Range(currentRow, 8, currentRow + 1, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = " Details of Injury";
                    worksheet.Range(currentRow, 9, currentRow, 13).Merge();
                    worksheet.Cell(currentRow, 14).Value = "What exactly was the injured person doing at the time of accident";
                    worksheet.Range(currentRow, 14, currentRow + 1, 14).Merge();
                    worksheet.Cell(currentRow, 15).Value = "Name, occupation, address and signature or the thumb impression of the person(s) giving notice";
                    worksheet.Range(currentRow, 15, currentRow + 1, 15).Merge();
                    worksheet.Cell(currentRow, 16).Value = "Signature and designation of the person who makes the entry in theAccident Book";
                    worksheet.Range(currentRow, 16, currentRow + 1, 16).Merge();
                    worksheet.Cell(currentRow, 17).Value = " Name Address and occupation of two witnesses";
                    worksheet.Range(currentRow, 17, currentRow + 1, 17).Merge();
                    worksheet.Cell(currentRow, 18).Value = "Remarks, if any Remarks, if any";
                    worksheet.Range(currentRow, 18, currentRow + 1, 18).Merge();

                    //subheader
                    currentRow++;
                    worksheet.Cell(currentRow, 9).Value = "Cause";
                    worksheet.Cell(currentRow, 10).Value = "Nature";
                    worksheet.Cell(currentRow, 11).Value = "Date";
                    worksheet.Cell(currentRow, 12).Value = "Time";
                    worksheet.Cell(currentRow, 13).Value = "Place";


                    var headerRange = worksheet.Range(2, 1, currentRow, 18);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(2, 1, currentRow, 18);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 18).AdjustToContents();

                    worksheet.Column(4).Width = 30;
                    worksheet.Column(7).Width = 20;
                    worksheet.Column(8).Width = 30;
                    worksheet.Column(14).Width = 30;
                    worksheet.Column(15).Width = 30;
                    worksheet.Column(16).Width = 30;
                    worksheet.Column(17).Width = 30;


                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.ColumnNumber() != 4 && column.ColumnNumber() != 7 && column.ColumnNumber() != 8 && column.ColumnNumber() != 14
                        && column.ColumnNumber() != 15 && column.ColumnNumber() != 16 && column.ColumnNumber() != 17
                        )
                        {
                            if (column.Width > 28) column.Width = 28;
                        }
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form VI( WAGE SLIP)
                if (FormId == 26)////Form VI( WAGE SLIP)
                {
                    var worksheet = workbook.Worksheets.Add("FormVIReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 8).Merge();
                    worksheet.Range(1, 1, 1, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;
                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;
                    richTextInfo.AddText("NAME OF ESTABLISHMENT: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("PLACE:")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();

                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Range(2, 1, 3, 8).Merge();
                    worksheet.Range(2, 1, 3, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).Height = 40;
                    // worksheet.Row(3).Height = 5;

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 4;
                    worksheet.Cell(currentRow, 1).Value = "Name of the worker";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Wage period ";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Minimum rates of wages payable ";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Dates on which overtime worked ";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Gross wages payable ";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Deductions, if any";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = " Actual wages paid ";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Signature of the employee ";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();

                    var headerRange = worksheet.Range(4, 1, currentRow, 8);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(4, 1, currentRow, 8);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 8).AdjustToContents();

                    //worksheet.Column(4).Width = 30;
                    //worksheet.Column(7).Width = 30;
                    //worksheet.Column(8).Width = 30;


                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        //if (column.ColumnNumber() != 4 && column.ColumnNumber() != 7 && column.ColumnNumber() != 8 && column.ColumnNumber() != 14
                        //&& column.ColumnNumber() != 15 && column.ColumnNumber() != 16 && column.ColumnNumber() != 17
                        //)
                        //{
                        if (column.Width > 28) column.Width = 28;
                        //}
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form 15( LEAVE BOOK)
                if (FormId == 27)////Form 15( LEAVE BOOK)
                {
                    var worksheet = workbook.Worksheets.Add("Form15Report");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);
                    richText.AddNewLine();
                    richText.AddText(("Register of Leave Book" ?? "").ToString().Trim())
                     .SetBold()
                     .SetFontSize(12);
                    worksheet.Range(1, 1, 1, 16).Merge();
                    worksheet.Range(1, 1, 1, 16).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;
                    var rt1 = worksheet.Range(2, 1, 2, 16).Merge().FirstCell().RichText;
                    // worksheet.Range(2, 1, 2, 16).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    rt1.AddText("Serial No: ").SetBold().SetFontSize(11);
                    rt1.AddText("".PadLeft(130));

                    rt1.AddText("Name of factory: ").SetBold().SetFontSize(11);
                    rt1.AddText("".PadLeft(110));

                    rt1.AddText("Particulars of Workers: ").SetBold().SetFontSize(11);
                    // rt1.AddText((Session["EmployeeName"] ?? "").ToString().Trim()).SetFontSize(11);
                    var rt2 = worksheet.Range(3, 1, 3, 16).Merge().FirstCell().RichText;
                    //worksheet.Range(3, 1, 3, 16).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    rt2.AddText("Department: ").SetBold().SetFontSize(11);
                    rt2.AddText((Session["Department"] ?? "").ToString().Trim()).SetFontSize(11);
                    rt2.AddText("".PadLeft(123));
                    rt2.AddText("Licence No. ").SetBold().SetFontSize(11);
                    rt2.AddText("".PadLeft(120));
                    rt2.AddText("Adult / Child  ").SetBold().SetFontSize(11);
                    //rt2.AddText((Session["FatherName"] ?? "").ToString().Trim()).SetFontSize(11);

                    var rt3 = worksheet.Range(4, 1, 4, 16).Merge().FirstCell().RichText;
                    rt3.AddText("Serial No. in the Register of Adult/Child Workers:").SetBold().SetFontSize(11);
                    rt3.AddText("".PadLeft(200));
                    rt3.AddText("Father’s Name:").SetBold().SetFontSize(11);
                    rt3.AddText("".PadLeft(100));
                    var rt4 = worksheet.Range(5, 1, 5, 16).Merge().FirstCell().RichText;
                    rt4.AddText("Date of entry into service :").SetBold().SetFontSize(11);
                    rt4.AddText("".PadLeft(230));
                    rt4.AddText("Date of Discharge Date:").SetBold().SetFontSize(11);

                    worksheet.Row(2).Height = 20;
                    worksheet.Row(3).Height = 20;
                    worksheet.Row(4).Height = 20;
                    worksheet.Row(5).Height = 20;

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 6;
                    worksheet.Cell(currentRow, 1).Value = "Number of day during the previous year";
                    worksheet.Range(currentRow, 1, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Total of Columns 1 to 4";
                    worksheet.Range(currentRow, 5, currentRow + 1, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Leave to Credit";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Leave earned during the previous year";
                    worksheet.Range(currentRow, 7, currentRow + 1, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Total of Columns 6 & 7 Leave to credit";
                    worksheet.Range(currentRow, 8, currentRow + 1, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Whether leave in accordance with scheme under section 79(8) was refused";
                    worksheet.Range(currentRow, 9, currentRow + 1, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = " Leave enjoyed from....To";
                    worksheet.Range(currentRow, 10, currentRow + 1, 10).Merge();
                    worksheet.Cell(currentRow, 11).Value = "Balance of Leave at credit ";
                    worksheet.Range(currentRow, 11, currentRow + 1, 11).Merge();
                    worksheet.Cell(currentRow, 12).Value = "Normal rate of wages";
                    worksheet.Range(currentRow, 12, currentRow, 12).Merge();
                    worksheet.Cell(currentRow, 13).Value = "Cash equivalent of advantage accruing through concessional sale of Food-grains and other articles";
                    worksheet.Range(currentRow, 13, currentRow + 1, 13).Merge();
                    worksheet.Cell(currentRow, 14).Value = "Rate of wages for the leave period (Total of column 12 and 13)";
                    worksheet.Range(currentRow, 14, currentRow + 1, 14).Merge();
                    worksheet.Cell(currentRow, 15).Value = "Date and reference to payment vide wages Register or voucher";
                    worksheet.Range(currentRow, 15, currentRow + 1, 15).Merge();
                    worksheet.Cell(currentRow, 16).Value = "Remarks and Initials of Manager for each entry";
                    worksheet.Range(currentRow, 16, currentRow + 1, 16).Merge();

                    //subheader
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = "No. of days work per-formed";
                    worksheet.Cell(currentRow, 2).Value = "No. of days of lay-off";
                    worksheet.Cell(currentRow, 3).Value = "No. of days  of maternity leave";
                    worksheet.Cell(currentRow, 4).Value = "No. of  days of leave enjoyed ";
                    worksheet.Cell(currentRow, 6).Value = "Balance of Leave";


                    var headerRange = worksheet.Range(6, 1, currentRow, 16);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(6, 1, currentRow, 16);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 8).AdjustToContents();

                    //worksheet.Column(4).Width = 30;
                    //worksheet.Column(7).Width = 30;
                    //worksheet.Column(8).Width = 30;


                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        //if (column.ColumnNumber() != 4 && column.ColumnNumber() != 7 && column.ColumnNumber() != 8 && column.ColumnNumber() != 14
                        //&& column.ColumnNumber() != 15 && column.ColumnNumber() != 16 && column.ColumnNumber() != 17
                        //)
                        //{
                        if (column.Width > 28) column.Width = 28;
                        //}
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);

                }
                #endregion

                #region Form XIX(Wages Slip )
                if (FormId == 28)////Form XIX( Wages Slip )
                {
                    var worksheet = workbook.Worksheets.Add("Form15Report");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 8).Merge();
                    worksheet.Range(1, 1, 1, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;
                    richTextInfo.AddText("NAME OF ESTABLISHMENT: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("Address of establishment:")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();

                    richTextInfo.AddText("Nature of work and location of work :")
                             .SetBold()
                             .SetFontSize(11);
                    // richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("Sex and identification marks…:")
                           .SetBold()
                           .SetFontSize(11);
                    // richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("Token/Ticket No:")
                     .SetBold()
                     .SetFontSize(11);

                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Range(2, 1, 3, 8).Merge();
                    worksheet.Range(2, 1, 3, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).Height = 80;


                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 4;
                    worksheet.Cell(currentRow, 1).Value = "No. of days worked";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Rate of daily wages / piece rate ";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "No. of units worked in case of piece rate ";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Dates on which overtime worked";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Overtime hours and amount of overtime wages ";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Gross wages payable ";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Deductions, any";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "if Actual wages paid";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();

                    var headerRange = worksheet.Range(4, 1, currentRow, 8);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(4, 1, currentRow, 8);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 8).AdjustToContents();

                    //worksheet.Column(4).Width = 30;
                    //worksheet.Column(7).Width = 30;
                    //worksheet.Column(8).Width = 30;


                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        //if (column.ColumnNumber() != 4 && column.ColumnNumber() != 7 && column.ColumnNumber() != 8 && column.ColumnNumber() != 14
                        //&& column.ColumnNumber() != 15 && column.ColumnNumber() != 16 && column.ColumnNumber() != 17
                        //)
                        //{
                        if (column.Width > 28) column.Width = 28;
                        //}
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form XII( Register of Contractors)
                if (FormId == 29)////Form XII( Register of Contractors)
                {
                    var worksheet = workbook.Worksheets.Add("Form15Report");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 6).Merge();
                    worksheet.Range(1, 1, 1, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;
                    richTextInfo.AddText("NAME OF ESTABLISHMENT: ")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("ADDRESS OF ESTABLISHMENT:")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();

                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Range(2, 1, 3, 6).Merge();
                    worksheet.Range(2, 1, 3, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).Height = 40;


                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 4;
                    worksheet.Cell(currentRow, 1).Value = "Sr.No.";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Name and address of contractor ";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Nature of contract work";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Location of contract work";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Period of contract From To ";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Maximum number workmen employed by contractor ";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();

                    var headerRange = worksheet.Range(4, 1, currentRow, 6);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(4, 1, currentRow, 6);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 8).AdjustToContents();

                    //worksheet.Column(4).Width = 30;
                    //worksheet.Column(7).Width = 30;
                    //worksheet.Column(8).Width = 30;


                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        //if (column.ColumnNumber() != 4 && column.ColumnNumber() != 7 && column.ColumnNumber() != 8 && column.ColumnNumber() != 14
                        //&& column.ColumnNumber() != 15 && column.ColumnNumber() != 16 && column.ColumnNumber() != 17
                        //)
                        //{
                        if (column.Width > 28) column.Width = 28;
                        //}
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form XXI( Register of Fines)
                if (FormId == 30)////Form XII( Register of Fines)
                {
                    var worksheet = workbook.Worksheets.Add("Form15Report");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 12).Merge();
                    worksheet.Range(1, 1, 1, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;
                    richTextInfo.AddText("Name and address of Contractor : ")
                               .SetBold()
                               .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                    //         .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("Name and address of establishment :")
                               .SetBold()
                               .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                    //         .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("Nature of work and location of work:")
                           .SetBold()
                           .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                    //         .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("Name and address of Principal Employer:")
                  .SetBold()
                  .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                    //         .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Range(2, 1, 3, 12).Merge();
                    worksheet.Range(2, 1, 3, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).Height = 60;


                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 4;
                    worksheet.Cell(currentRow, 1).Value = "Sr.No.";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Name of workman";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Father's/Husband's Name";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Designation/nature of employment ";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Act/Omission for which fine is imposed ";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Date of Offences ";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Whether workman showed cause against fine ";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Name of person in whose presence employees explanation was heard";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Age period and wages payable";
                    worksheet.Range(currentRow, 9, currentRow, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "Amount of fine imposed";
                    worksheet.Range(currentRow, 10, currentRow, 10).Merge();
                    worksheet.Cell(currentRow, 11).Value = "Date on which fine realized ";
                    worksheet.Range(currentRow, 11, currentRow, 11).Merge();
                    worksheet.Cell(currentRow, 12).Value = "Remark ";
                    worksheet.Range(currentRow, 12, currentRow, 12).Merge();
                    var headerRange = worksheet.Range(4, 1, currentRow, 12);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(4, 1, currentRow, 12);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 8).AdjustToContents();

                    //worksheet.Column(4).Width = 30;
                    //worksheet.Column(7).Width = 30;
                    //worksheet.Column(8).Width = 30;


                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        //if (column.ColumnNumber() != 4 && column.ColumnNumber() != 7 && column.ColumnNumber() != 8 && column.ColumnNumber() != 14
                        //&& column.ColumnNumber() != 15 && column.ColumnNumber() != 16 && column.ColumnNumber() != 17
                        //)
                        //{
                        if (column.Width > 28) column.Width = 28;
                        //}
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form 23( REGISTER OF ACCIDENTS AND DANGEROUS OCCURRENCES)
                if (FormId == 31)////Form 23( REGISTER OF ACCIDENTS AND DANGEROUS OCCURRENCES)
                {
                    var worksheet = workbook.Worksheets.Add("Form23Report");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 9).Merge();
                    worksheet.Range(1, 1, 1, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 2;
                    worksheet.Cell(currentRow, 1).Value = "Name of injured person(if any)";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Date of accident or dangerous occurrence";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Time and mode of message to the Inspector";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Date of report (in Form No. 17 to Inspector)";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Nature of Accident or dangerous occurrence";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Details of injury ";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Date of return of injured person to work";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Number of days injured person was absent from work";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Signature of Manager";
                    worksheet.Range(currentRow, 9, currentRow, 9).Merge();

                    var headerRange = worksheet.Range(2, 1, currentRow, 9);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(2, 1, currentRow, 9);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 8).AdjustToContents();

                    //worksheet.Column(4).Width = 30;
                    //worksheet.Column(7).Width = 30;
                    //worksheet.Column(8).Width = 30;


                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        //if (column.ColumnNumber() != 4 && column.ColumnNumber() != 7 && column.ColumnNumber() != 8 && column.ColumnNumber() != 14
                        //&& column.ColumnNumber() != 15 && column.ColumnNumber() != 16 && column.ColumnNumber() != 17
                        //)
                        //{
                        if (column.Width > 28) column.Width = 28;
                        //}
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form 33( REGISTER OF TIGHT FITTING CLOTHING)
                if (FormId == 32)////Form 33( REGISTER OF TIGHT FITTING CLOTHING)
                {
                    var worksheet = workbook.Worksheets.Add("Form33Report");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 7).Merge();
                    worksheet.Range(1, 1, 1, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 2;
                    worksheet.Cell(currentRow, 1).Value = "Name of injured person(if any)";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Name of worker";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Name and Father’s ";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Designation or Nature of work";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Department";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Date when tight clothes provided";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Signature or Thumb impression of worker";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();


                    var headerRange = worksheet.Range(2, 1, currentRow, 7);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(2, 1, currentRow, 7);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 8).AdjustToContents();

                    //worksheet.Column(4).Width = 30;
                    //worksheet.Column(7).Width = 30;
                    //worksheet.Column(8).Width = 30;


                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        //if (column.ColumnNumber() != 4 && column.ColumnNumber() != 7 && column.ColumnNumber() != 8 && column.ColumnNumber() != 14
                        //&& column.ColumnNumber() != 15 && column.ColumnNumber() != 16 && column.ColumnNumber() != 17
                        //)
                        //{
                        if (column.Width > 28) column.Width = 28;
                        //}
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form T(Register of Fine , Deduction Damages or Loss)
                if (FormId == 33)////Form T( Register of Fine , Deduction Damages or Loss)
                {
                    var worksheet = workbook.Worksheets.Add("FormTReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 15).Merge();
                    worksheet.Range(1, 1, 1, 15).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 2;
                    worksheet.Cell(currentRow, 1).Value = "Name and address of the Factory / Establishment ";
                    worksheet.Range(currentRow, 1, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = " ";
                    worksheet.Range(currentRow, 4, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Name and address of the Principal Employer";
                    worksheet.Range(currentRow, 6, currentRow, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Name and address of the Contractor(if any place of work) ";
                    worksheet.Range(currentRow, 9, currentRow, 10).Merge();
                    worksheet.Cell(currentRow, 11).Value = "Month / Year";
                    worksheet.Range(currentRow, 11, currentRow, 13).Merge();

                    //subheader
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = "Sl.No";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Name and employees of the Factory /Establishment ";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Sex M / F ";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Designation on / Employment No. ";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Nature & date of offence for which fine important ";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Date and particulars of damages loss caused ";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Date and purpose for which advance was made  ";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Whether worker show cause against fine/Deduction";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Amount of the imposed deduction advance made ";
                    worksheet.Range(currentRow, 9, currentRow, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "No. of installments granted for repayment of fined Deduction or Advances ";
                    worksheet.Range(currentRow, 10, currentRow, 10).Merge();
                    worksheet.Cell(currentRow, 11).Value = "Wages period and rate of wages payable ";
                    worksheet.Range(currentRow, 11, currentRow, 12).Merge();
                    worksheet.Cell(currentRow, 13).Value = "Date of recovery of fine deduction Advance ";
                    worksheet.Range(currentRow, 13, currentRow, 13).Merge();
                    worksheet.Cell(currentRow, 14).Value = "Designation and Signature of the employee";
                    worksheet.Range(currentRow, 14, currentRow, 14).Merge();
                    worksheet.Cell(currentRow, 15).Value = "Remarks";
                    worksheet.Range(currentRow, 15, currentRow, 15).Merge();

                    //subheader
                    currentRow++;
                    worksheet.Cell(currentRow, 11).Value = "First Installment";
                    worksheet.Cell(currentRow, 12).Value = "Last Installment";


                    var headerRange = worksheet.Range(2, 1, currentRow, 15);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(2, 1, currentRow, 15);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 8).AdjustToContents();

                    worksheet.Column(9).Width = 30;
                    worksheet.Column(10).Width = 30;
                    //worksheet.Column(8).Width = 30;


                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.ColumnNumber() != 9 && column.ColumnNumber() != 10)
                        {
                            if (column.Width > 28) column.Width = 28;
                        }
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form XXII(Register Of Advances)
                if (FormId == 34)////Form XXII( Register Of Advances)
                {
                    var worksheet = workbook.Worksheets.Add("FormXXIIReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 11).Merge();
                    worksheet.Range(1, 1, 1, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;
                    richTextInfo.AddText("Name and address of Contractor : ")
                               .SetBold()
                               .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                    //         .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("Name and address of establishment :")
                               .SetBold()
                               .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                    //         .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("Nature of work and location of work : ")
                               .SetBold()
                               .SetFontSize(11);
                    //richTextInfo.AddText(Month.ToString("MMM-yyyy"))
                    //        .SetFontSize(11);
                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Range(2, 1, 3, 11).Merge();
                    worksheet.Range(2, 1, 3, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).Height = 40;
                    worksheet.Row(3).Height = 20;

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 4;
                    worksheet.Cell(currentRow, 1).Value = "Sr.No";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Name";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Father's/Husband's Name";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Designation/nature of employment";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Wage period and wages payable";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Date and amount of advance given";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Purpose for which advance made";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "No. of instalments by which advance to be repaid ";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Date and amount of each instalment repaid ";
                    worksheet.Range(currentRow, 9, currentRow, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "Date on which last instalment was paid ";
                    worksheet.Range(currentRow, 10, currentRow, 10).Merge();
                    worksheet.Cell(currentRow, 11).Value = "Remark";
                    worksheet.Range(currentRow, 11, currentRow, 11).Merge();

                    var headerRange = worksheet.Range(4, 1, currentRow, 11);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(4, 1, currentRow, 11);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 8).AdjustToContents();

                    // worksheet.Column(9).Width = 30;
                    //worksheet.Column(10).Width = 30;
                    //worksheet.Column(8).Width = 30;

                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        //if (column.ColumnNumber() != 9 && column.ColumnNumber() != 10)
                        //{
                        if (column.Width > 28) column.Width = 28;
                        //}
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form XIII( Register Of Workmen Employed)
                if (FormId == 35)////Form XXII(  Register Of Workmen Employed)
                {
                    var worksheet = workbook.Worksheets.Add("FormXXIIReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 12).Merge();
                    worksheet.Range(1, 1, 1, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;
                    richTextInfo.AddText("Name and address of Contractor : ")
                               .SetBold()
                               .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                    //         .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("Name and address of establishment :")
                               .SetBold()
                               .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                    //         .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("Nature of work and location of work : ")
                               .SetBold()
                               .SetFontSize(11);
                    //richTextInfo.AddText(Month.ToString("MMM-yyyy"))
                    //        .SetFontSize(11);
                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Range(2, 1, 3, 12).Merge();
                    worksheet.Range(2, 1, 3, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).Height = 40;
                    worksheet.Row(3).Height = 20;

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 4;
                    worksheet.Cell(currentRow, 1).Value = "Sr.No";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Name and Surname of workmen ";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Age and Sex";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Father's/Husband's Name";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Nature of Employment /Designation ";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Permanent Home address of workmen Village and Tahsil / Taluka and District";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Local Address ";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Date of Commencement of employment";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Signature or thumb impression of workman ";
                    worksheet.Range(currentRow, 9, currentRow, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "Date of termination of employment";
                    worksheet.Range(currentRow, 10, currentRow, 10).Merge();
                    worksheet.Cell(currentRow, 11).Value = "Reasons for termination ";
                    worksheet.Range(currentRow, 11, currentRow, 11).Merge();
                    worksheet.Cell(currentRow, 12).Value = "Remarks";
                    worksheet.Range(currentRow, 12, currentRow, 12).Merge();

                    var headerRange = worksheet.Range(4, 1, currentRow, 12);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(4, 1, currentRow, 12);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 8).AdjustToContents();

                    // worksheet.Column(9).Width = 30;
                    //worksheet.Column(10).Width = 30;
                    //worksheet.Column(8).Width = 30;

                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        //if (column.ColumnNumber() != 9 && column.ColumnNumber() != 10)
                        //{
                        if (column.Width > 28) column.Width = 28;
                        //}
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form H (KA_LEAVE_WAGES)

                if (FormId == 36)///// Form H(KA_LEAVE_WAGES)
                {
                    var worksheet = workbook.Worksheets.Add("FormHReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 12).Merge();
                    worksheet.Range(1, 1, 1, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();

                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;
                    var rt1 = worksheet.Range(2, 1, 2, 12).Merge().FirstCell().RichText;
                    rt1.AddText("Sr.No in the Register of Adult/Young Person: ").SetBold().SetFontSize(11);
                    rt1.AddText("".PadLeft(130));

                    rt1.AddText("Date Of entry into Service: ").SetBold().SetFontSize(11);
                    rt1.AddText("".PadLeft(110));
                    var rt2 = worksheet.Range(3, 1, 3, 12).Merge().FirstCell().RichText;

                    rt2.AddText("Name Of the Person: ").SetBold().SetFontSize(11);
                    // rt2.AddText((Session["Department"] ?? "").ToString().Trim()).SetFontSize(11);
                    rt2.AddText("".PadLeft(175));
                    rt2.AddText("Father Name: ").SetBold().SetFontSize(11);
                    // rt1.AddText("".PadLeft(110));
                    var rt3 = worksheet.Range(4, 1, 4, 12).Merge().FirstCell().RichText;
                    rt3.AddText("Address:").SetBold().SetFontSize(11);
                    rt3.AddText("".PadLeft(200));

                    worksheet.Row(2).Height = 20;
                    worksheet.Row(3).Height = 20;
                    worksheet.Row(4).Height = 20;

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);

                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_KA_LEAVE_WAGES", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 5;
                    worksheet.Cell(currentRow, 1).Value = "PART I- EARNED LEAVE";
                    worksheet.Range(currentRow, 1, currentRow, 12).Merge();
                    //subheader
                    currentRow++;

                    worksheet.Cell(currentRow, 1).Value = "Sr.No";
                    worksheet.Range(currentRow, 1, currentRow + 1, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "No of Days worked";
                    worksheet.Range(currentRow, 2, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Leave earned";
                    worksheet.Range(currentRow, 5, currentRow + 1, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Leave at credit (incl balance if any, on return from leave on last occasion)";
                    worksheet.Range(currentRow, 6, currentRow + 1, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "leave taken";
                    worksheet.Range(currentRow, 7, currentRow, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "Balance on return from leave";
                    worksheet.Range(currentRow, 10, currentRow + 1, 10).Merge();
                    worksheet.Cell(currentRow, 11).Value = "Date on which wages for leave paid and amount paid";
                    worksheet.Range(currentRow, 11, currentRow, 11).Merge();
                    worksheet.Cell(currentRow, 12).Value = "Remarks";
                    worksheet.Range(currentRow, 12, currentRow, 12).Merge();

                    // Sub-headers
                    currentRow++;
                    worksheet.Cell(currentRow, 2).Value = "From";
                    worksheet.Cell(currentRow, 3).Value = "To";
                    worksheet.Cell(currentRow, 4).Value = "Total Days Worked";
                    worksheet.Cell(currentRow, 7).Value = "From";
                    worksheet.Cell(currentRow, 8).Value = "To";
                    worksheet.Cell(currentRow, 9).Value = "No. of days";


                    var headerRange = worksheet.Range(5, 1, currentRow, 12);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count;
                    currentRow++;
                    //var fullTableRange = worksheet.Range(5, 1, currentRow, 12);
                    //fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // ---------- SECOND ----------

                    int tableStartRow = currentRow;

                    // Second table title
                    worksheet.Cell(tableStartRow, 1).Value = "PART II - Sick/Accident Leave (with pay)";
                    worksheet.Range(tableStartRow, 1, tableStartRow, 5).Merge();

                    tableStartRow++;

                    // Second table headers
                    worksheet.Cell(tableStartRow, 1).Value = "Sr.No";
                    worksheet.Range(tableStartRow, 1, tableStartRow + 1, 1).Merge();
                    worksheet.Cell(tableStartRow, 2).Value = "Year";
                    worksheet.Range(tableStartRow, 2, tableStartRow + 1, 2).Merge();
                    worksheet.Cell(tableStartRow, 3).Value = "Sick/Accident leave";
                    worksheet.Range(tableStartRow, 3, tableStartRow, 4).Merge();
                    worksheet.Cell(tableStartRow, 5).Value = "Balance at the end of the year";
                    worksheet.Range(tableStartRow, 5, tableStartRow + 1, 5).Merge();

                    // Sub-header row for both
                    tableStartRow++;
                    worksheet.Cell(tableStartRow, 3).Value = "Of Credit";
                    worksheet.Cell(tableStartRow, 4).Value = "Availed";

                    // Styling
                    var headerRange1 = worksheet.Range(tableStartRow - 2, 1, tableStartRow, 5);
                    headerRange1.Style.Font.Bold = true;
                    headerRange1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange1.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange1.Style.Alignment.WrapText = true;
                    headerRange1.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange1.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill both tables data side by side
                    tableStartRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < 7; c++)
                        {
                            worksheet.Cell(tableStartRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                        for (int c = 0; c < 6; c++)
                        {
                            worksheet.Cell(tableStartRow + r, 9 + c).Value = dt.Rows[r][c];
                        }
                    }

                    worksheet.Columns().AdjustToContents();
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.Width > 10) column.Width = 10;
                        if (column.Width < 10) column.Width = 10;
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);

                }
                #endregion

                #region Form 9( Register Of Overtime And Payment)
                if (FormId == 37)////Form 9( Register Of Overtime And Payment)
                {
                    var worksheet = workbook.Worksheets.Add("Form9Report");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 13).Merge();
                    worksheet.Range(1, 1, 1, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;
                    richTextInfo.AddText("Name and Address of the Factory/Establishment: ")
                               .SetBold()
                               .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                    //         .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("Name and address of Principal Employer:")
                               .SetBold()
                               .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                    //         .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText(" Name and address of the Contractor (if any): ")
                               .SetBold()
                               .SetFontSize(11);
                    //richTextInfo.AddText(Month.ToString("MMM-yyyy"))
                    //        .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("Month/Year: ")
                               .SetBold()
                               .SetFontSize(11);
                    //richTextInfo.AddText(Month.ToString("MMM-yyyy"))
                    //        .SetFontSize(11);
                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Range(2, 1, 3, 13).Merge();
                    worksheet.Range(2, 1, 3, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).Height = 40;
                    worksheet.Row(3).Height = 20;

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 4;
                    worksheet.Cell(currentRow, 1).Value = "Sr.No";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Employee Name Father/Husband Name";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Sex";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Designation/Employment No.";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Particulars of OT Work";
                    worksheet.Range(currentRow, 5, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Normal rate of wages per hour";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Overtime wages per hour";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Normal piece rate of wages";
                    worksheet.Range(currentRow, 9, currentRow, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "OT piece rate of wages";
                    worksheet.Range(currentRow, 10, currentRow, 10).Merge();
                    worksheet.Cell(currentRow, 11).Value = "Total OT earnings";
                    worksheet.Range(currentRow, 11, currentRow, 11).Merge();
                    worksheet.Cell(currentRow, 12).Value = "Date of payment";
                    worksheet.Range(currentRow, 12, currentRow, 12).Merge();
                    worksheet.Cell(currentRow, 13).Value = "Signature/Thumb impression of the Employee";
                    worksheet.Range(currentRow, 13, currentRow, 13).Merge();

                    //subheader
                    currentRow++;
                    worksheet.Cell(currentRow, 5).Value = "Date";
                    worksheet.Cell(currentRow, 6).Value = "Hours";

                    var headerRange = worksheet.Range(4, 1, currentRow, 13);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(4, 1, currentRow, 13);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 8).AdjustToContents();

                    // worksheet.Column(9).Width = 30;
                    //worksheet.Column(10).Width = 30;
                    //worksheet.Column(8).Width = 30;

                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        //if (column.ColumnNumber() != 9 && column.ColumnNumber() != 10)
                        //{
                        if (column.Width > 28) column.Width = 28;
                        //}
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form 1(Payment of Subsistence Allownce Act)
                if (FormId == 38)////Form 1(Payment of Subsistence Allownce Act)
                {
                    var worksheet = workbook.Worksheets.Add("Form1Report");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 12).Merge();
                    worksheet.Range(1, 1, 1, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;
                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;
                    richTextInfo.AddText("NAME OF ESTABLISHMENT: ")
                                .SetBold()
                                .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("ADDRESS OF ESTABLISHMENT:")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();

                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Range(2, 1, 2, 12).Merge();
                    worksheet.Range(2, 1, 2, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).Height = 40;
                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 3;
                    worksheet.Cell(currentRow, 1).Value = "Sr.No";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Name and address of the employee kept under suspension";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Monthly employments (wages) paid to the employee.";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Department in which the employee was working last and his designation.";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Nature of offence committed and date of offence.";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Date of suspension";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Date of revocation of suspension";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Rate at which subsistence allowance calculated and period for which calculation made";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Amount of subsistence allowance paid and the date of payment";
                    worksheet.Range(currentRow, 9, currentRow, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "Whether the employee has been exonerated or awarded any punishment";
                    worksheet.Range(currentRow, 10, currentRow, 10).Merge();
                    worksheet.Cell(currentRow, 11).Value = "Remarks";
                    worksheet.Range(currentRow, 11, currentRow, 11).Merge();
                    worksheet.Cell(currentRow, 12).Value = "Signature of employee with date for receiving money or postal acknowledgement of money order.";
                    worksheet.Range(currentRow, 12, currentRow, 12).Merge();


                    var headerRange = worksheet.Range(3, 1, currentRow, 12);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(3, 1, currentRow, 12);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    // worksheet.Column(2).Width = 27;
                    //worksheet.Column(3).Width = 26;
                    worksheet.Columns().AdjustToContents();
                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        //    if (column.ColumnNumber() != 2 && column.ColumnNumber() != 3)
                        //    {
                        if (column.Width > 18) column.Width = 18;
                        // }
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form 40(Sickness, Absenteeism Register)
                if (FormId == 39)////Form 40(Sickness, Absenteeism Register)
                {
                    var worksheet = workbook.Worksheets.Add("Form40Report");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 8).Merge();
                    worksheet.Range(1, 1, 1, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;
                    var rt1 = worksheet.Range(2, 1, 2, 8).Merge().FirstCell().RichText;
                    // worksheet.Range(2, 1, 2, 16).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    rt1.AddText("Name of factory: ").SetBold().SetFontSize(11);
                    rt1.AddText("".PadLeft(130));

                    rt1.AddText("Address: ").SetBold().SetFontSize(11);
                    rt1.AddText("".PadLeft(110));

                    var rt2 = worksheet.Range(3, 1, 3, 8).Merge().FirstCell().RichText;
                    //worksheet.Range(3, 1, 3, 16).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    rt2.AddText("Licence No: ").SetBold().SetFontSize(11);
                    //  rt2.AddText((Session["Department"] ?? "").ToString().Trim()).SetFontSize(11);


                    var rt3 = worksheet.Range(4, 1, 4, 8).Merge().FirstCell().RichText;
                    rt3.AddText("MonthYear:").SetBold().SetFontSize(11);

                    worksheet.Row(2).Height = 20;
                    worksheet.Row(3).Height = 20;
                    worksheet.Row(4).Height = 20;
                    worksheet.Row(5).Height = 20;

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 6;
                    worksheet.Cell(currentRow, 1).Value = "Sr.No";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Name of the Worker";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Department";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Date of Absence due to illness";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Date of return to work";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "By Whome Treated";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Name of the diesease as per medical Certificate";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Description of illness in brief ";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();


                    var headerRange = worksheet.Range(6, 1, currentRow, 8);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(6, 1, currentRow, 8);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 8).AdjustToContents();

                    //worksheet.Column(4).Width = 30;
                    //worksheet.Column(7).Width = 30;
                    //worksheet.Column(8).Width = 30;


                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        //if (column.ColumnNumber() != 4 && column.ColumnNumber() != 7 && column.ColumnNumber() != 8 && column.ColumnNumber() != 14
                        //&& column.ColumnNumber() != 15 && column.ColumnNumber() != 16 && column.ColumnNumber() != 17
                        //)
                        //{
                        if (column.Width > 28) column.Width = 28;
                        //}
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form A( Maintenance Of Register)
                if (FormId == 40)////Form A( Maintenance Of Register)
                {
                    var worksheet = workbook.Worksheets.Add("FormAReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 10).Merge();
                    worksheet.Range(1, 1, 1, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;

                    richTextInfo.AddText("NAME OF ESTABLISHMENT: ")
                                .SetBold()
                                .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("ADDRESS OF ESTABLISHMENT:")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("Total number of workers employed:")
                              .SetBold()
                              .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("Total number of men workers employed:")
                           .SetBold()
                           .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("Total number of Women workers employed:")
                          .SetBold()
                          .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    worksheet.Range(2, 1, 2, 10).Merge();
                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    //worksheet.Cell(2, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //worksheet.Cell(2, 1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(2, 1, 2, 10).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(2, 1, 2, 10).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).AdjustToContents();


                    worksheet.Row(2).Height = 100;
                    worksheet.Row(3).Height = 20;
                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 4;
                    worksheet.Cell(currentRow, 1).Value = "Category of Workers";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Brief description Of Work";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "No.Of Men Employed";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "No. Of Women Employed";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Rate Of remuneration Paid";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Basic wage Of Salary";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Dearness allowance";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "House Rent allowance ";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Other allowance ";
                    worksheet.Range(currentRow, 9, currentRow, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "Cash value of concessional supply of essential commodities";
                    worksheet.Range(currentRow, 10, currentRow, 10).Merge();

                    var headerRange = worksheet.Range(4, 1, currentRow, 10);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(4, 1, currentRow, 10);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 8).AdjustToContents();
                    worksheet.Column(10).Width = 30;
                    //worksheet.Column(7).Width = 30;
                    //worksheet.Column(8).Width = 30;

                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.ColumnNumber() != 10)
                        {
                            if (column.Width > 28) column.Width = 28;
                        }
                    }
                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form 32( Prescribed For Report Of Examination For Cranes And Other Lifting Machines)
                if (FormId == 41)////Form 32( Prescribed For Report Of Examination For Cranes And Other Lifting Machines)
                {
                    var worksheet = workbook.Worksheets.Add("Form32Report");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 7).Merge();
                    worksheet.Range(1, 1, 1, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;
                    worksheet.Cell(2, 1).Clear();
                    worksheet.Row(2).Height = 20;

                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 3;
                    worksheet.Cell(currentRow, 1).Value = "Name of occupier of factory";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Address of the Factory";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Distinguishing number of mark (if any) and description sufficient to identify the lifting machine, chain, rope or the lifting tackle";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Date when the lifting machine, chain, rope or lifting tackle was first taken into use in the factory";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Date and number of the certificate relating to any test and examination made under Rule 64(5) and 64(6) together with the name and address of the person who issued the certificate";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Date of each periodical thorough examination made under clause (a)(ii) of sub-section (1) of Section 29 of the Act and Rule 64(7) and by whom it was carried out";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Particulars of any defects affecting the safety working load found at such thorough examination or after annealing and of the step taken to remedy such defects";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();

                    var headerRange = worksheet.Range(3, 1, currentRow, 7);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(3, 1, currentRow, 7);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 8).AdjustToContents();
                    worksheet.Column(10).Width = 30;
                    //worksheet.Column(7).Width = 30;
                    //worksheet.Column(8).Width = 30;

                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.ColumnNumber() != 10)
                        {
                            if (column.Width > 28) column.Width = 28;
                        }
                    }
                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #endregion

                #region DELHI FORMS

                #region Form XII(Register Of Contracter)
                if (FormId == 42)////Form XII( Register Of Contracter)
                {
                    var worksheet = workbook.Worksheets.Add("FormXIIReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 7).Merge();
                    worksheet.Range(1, 1, 1, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;

                    richTextInfo.AddText("NAME OF ESTABLISHMENT: ")
                                .SetBold()
                                .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("ADDRESS OF ESTABLISHMENT:")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();

                    worksheet.Range(2, 1, 2, 7).Merge();
                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    //worksheet.Cell(2, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //worksheet.Cell(2, 1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(2, 1, 2, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(2, 1, 2, 7).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).AdjustToContents();


                    worksheet.Row(2).Height = 80;
                    worksheet.Row(3).Height = 20;
                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 4;
                    worksheet.Cell(currentRow, 1).Value = "Sr.No";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Name and address of contractor";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Nature of work on contractor";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Location of contract work";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Period of contractor Form";
                    worksheet.Range(currentRow, 5, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Maximum No. of workmen employed by contractor";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();

                    //subheader
                    currentRow++;
                    worksheet.Cell(currentRow, 5).Value = "From";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "To";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();


                    var headerRange = worksheet.Range(4, 1, currentRow, 7);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(4, 1, currentRow, 7);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 8).AdjustToContents();

                    worksheet.Column(5).Width = 15;
                    worksheet.Column(6).Width = 15;
                    //worksheet.Column(7).Width = 30;
                    //worksheet.Column(8).Width = 30;


                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.ColumnNumber() != 10)
                        {
                            if (column.Width > 28) column.Width = 28;
                        }
                    }
                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region Form XIII(Register Of Workmen Employed)
                if (FormId == 43)////Form XIII( Register Of Workmen Employed)
                {
                    var worksheet = workbook.Worksheets.Add("FormXIIIReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 13).Merge();
                    worksheet.Range(1, 1, 1, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;

                    richTextInfo.AddText("NAME OF ESTABLISHMENT: ")
                                .SetBold()
                                .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("ADDRESS OF ESTABLISHMENT:")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();

                    worksheet.Range(2, 1, 2, 13).Merge();
                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    //worksheet.Cell(2, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //worksheet.Cell(2, 1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(2, 1, 2, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(2, 1, 2, 13).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).AdjustToContents();


                    worksheet.Row(2).Height = 80;
                    worksheet.Row(3).Height = 20;
                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 4;
                    worksheet.Cell(currentRow, 1).Value = "Employee No";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Name and surname of workmen";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Date of Birth";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Sex";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Father's/Husband's Name";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Nature of Employment/Designation";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Permanent Address";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Local Address";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Date of Commencement of Employment";
                    worksheet.Range(currentRow, 9, currentRow, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "Signature of workmen";
                    worksheet.Range(currentRow, 10, currentRow, 10).Merge();
                    worksheet.Cell(currentRow, 11).Value = "Date of termination of employment";
                    worksheet.Range(currentRow, 11, currentRow, 11).Merge();
                    worksheet.Cell(currentRow, 12).Value = "Reasons for termination";
                    worksheet.Range(currentRow, 12, currentRow, 12).Merge();
                    worksheet.Cell(currentRow, 13).Value = "Remarks";
                    worksheet.Range(currentRow, 13, currentRow, 13).Merge();

                    var headerRange = worksheet.Range(4, 1, currentRow, 13);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(4, 1, currentRow, 13);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 8).AdjustToContents();

                    worksheet.Column(5).Width = 15;
                    worksheet.Column(6).Width = 15;
                    //worksheet.Column(7).Width = 30;
                    //worksheet.Column(8).Width = 30;


                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.ColumnNumber() != 10)
                        {
                            if (column.Width > 28) column.Width = 28;
                        }
                    }
                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region FORM XIX (Wage Slip)
                if (FormId == 44)////FORM XIX ( Wage Slip)
                {
                    var worksheet = workbook.Worksheets.Add("FORMXIXReport");
                var richText = worksheet.Cell(1, 1).RichText;
                richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                        .SetBold()
                        .SetFontSize(13);
                richText.AddNewLine();
                richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                        .SetBold(false)
                        .SetFontSize(10);
                richText.AddNewLine();
                richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                        .SetBold()
                        .SetFontSize(12);

                worksheet.Range(1, 1, 1, 8).Merge();
                worksheet.Range(1, 1, 1, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                worksheet.Row(1).Height = 80;

                worksheet.Cell(2, 1).Clear();
                var richTextInfo = worksheet.Cell(2, 1).RichText;

                richTextInfo.AddText("Name and address of contractor : ")
                            .SetBold()
                            .SetFontSize(11);
                richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                         .SetFontSize(11);
                richTextInfo.AddNewLine();
                richTextInfo.AddText("Name and father's/husband's name of the workman :")
                           .SetBold()
                           .SetFontSize(11);
                //richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                // .SetFontSize(11);
                richTextInfo.AddNewLine();
                richTextInfo.AddText("Nature and location of work:")
                         .SetBold()
                         .SetFontSize(11);
                richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                         .SetFontSize(11);
                richTextInfo.AddNewLine();

                worksheet.Range(2, 1, 2, 8).Merge();
                worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                //worksheet.Cell(2, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                //worksheet.Cell(2, 1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range(2, 1, 2, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range(2, 1, 2, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                worksheet.Row(2).AdjustToContents();


                worksheet.Row(2).Height = 80;
                worksheet.Row(3).Height = 20;
                DataTable dt = new DataTable();
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                paramList.Add("@p_FromMonthYear", Month);
                paramList.Add("@p_ToMonthYear", ToMonth);
                paramList.Add("@p_CompanyId", CmpId);
                paramList.Add("@p_BranchId", BranchId);
                paramList.Add("@p_FormId", FormId);
                var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                if (GetData.Count == 0)
                {
                    return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                }
                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(GetData);

                // ---------- FIRST TABLE ----------
                int currentRow = 4;
                worksheet.Cell(currentRow, 1).Value = "Number of days worked ";
                worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                worksheet.Cell(currentRow, 2).Value = "Number of units worked in case of piece rate workers";
                worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                worksheet.Cell(currentRow, 3).Value = "Rate of daily wages/piece rate ";
                worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                worksheet.Cell(currentRow, 4).Value = "Amount of overtime wages ";
                worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                worksheet.Cell(currentRow, 5).Value = "Gross wages payable ";
                worksheet.Range(currentRow, 5, currentRow, 6).Merge();
                worksheet.Cell(currentRow, 7).Value = "Deductions, If any  ";
                worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                worksheet.Cell(currentRow, 8).Value = "Net amount of wages paid";
                worksheet.Range(currentRow, 8, currentRow, 8).Merge();


                var headerRange = worksheet.Range(4, 1, currentRow, 8);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerRange.Style.Alignment.WrapText = true;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Fill first table data
                currentRow++;
                for (int r = 0; r < dt.Rows.Count; r++)
                {
                    for (int c = 0; c < dt.Columns.Count; c++)
                    {
                        worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                    }
                }
                currentRow += dt.Rows.Count - 1;
                var fullTableRange = worksheet.Range(4, 1, currentRow, 8);
                fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                worksheet.Columns(1, 8).AdjustToContents();

                worksheet.Column(5).Width = 15;
                worksheet.Column(6).Width = 15;
                //worksheet.Column(7).Width = 30;
                //worksheet.Column(8).Width = 30;


                foreach (var column in worksheet.ColumnsUsed())
                {
                    if (column.ColumnNumber() != 10)
                    {
                        if (column.Width > 28) column.Width = 28;
                    }
                }

                worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                worksheet.PageSetup.FitToPages(1, 0);
            }
                #endregion

                #endregion

                #region MADHYA PRADESH FORMS

                #region FORM XX (Register of Deductions for Damage )
                if (FormId == 45)////FORM XX ( Register of Deductions for Damage )
                {
                    var worksheet = workbook.Worksheets.Add("FORMXXReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 13).Merge();
                    worksheet.Range(1, 1, 1, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;

                    richTextInfo.AddText("Name and address of contractor : ")
                                .SetBold()
                                .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("Nature and location of work:")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                     .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("Name and address of establishment :")
                             .SetBold()
                             .SetFontSize(11);
                   // richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                           //  .SetFontSize(11);
                    richTextInfo.AddNewLine();

                    worksheet.Range(2, 1, 2, 13).Merge();
                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    //worksheet.Cell(2, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //worksheet.Cell(2, 1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(2, 1, 2, 13).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(2, 1, 2, 13).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).AdjustToContents();


                    worksheet.Row(2).Height = 80;
                    worksheet.Row(3).Height = 20;
                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 4;
                    worksheet.Cell(currentRow, 1).Value = "Sr.No:";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Name of workman:";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Father's/Husband's name";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Designation/Nature of employment";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Particulars of damage or loss";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Date of damage or loss";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Whether work man showed cause against deduction";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Name of person In whose presence employee's  explanation was heard ";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Amount of deduction imposed";
                    worksheet.Range(currentRow, 9, currentRow, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "No. of installments";
                    worksheet.Range(currentRow, 10, currentRow, 11).Merge();
                    worksheet.Cell(currentRow, 12).Value = "Date of recovery";
                    worksheet.Range(currentRow, 12, currentRow, 12).Merge();
                    worksheet.Cell(currentRow, 13).Value = "Remarks";
                    worksheet.Range(currentRow, 13, currentRow, 13).Merge();
                    //Subheader

                    currentRow++;
                    worksheet.Cell(currentRow, 10).Value = "First installment ";
                    worksheet.Cell(currentRow, 11).Value = "Last installment ";


                    var headerRange = worksheet.Range(4, 1, currentRow, 13);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(4, 1, currentRow, 13);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 8).AdjustToContents();

                    worksheet.Column(10).Width = 15;
                    worksheet.Column(11).Width = 15;
                    //worksheet.Column(7).Width = 30;
                    //worksheet.Column(8).Width = 30;


                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        //if (column.ColumnNumber() != 10)
                        //{
                            if (column.Width > 28) column.Width = 28;
                        //}
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region FORM XXI (Register of Fines )
                if (FormId == 46)////FORM XXI ( Register of Fines )
                {
                    var worksheet = workbook.Worksheets.Add("FORMXXIReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 12).Merge();
                    worksheet.Range(1, 1, 1, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;

                    richTextInfo.AddText("Name and address of contractor : ")
                                .SetBold()
                                .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                             .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("Nature and location of work:")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                     .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("Name and address of establishment :")
                             .SetBold()
                             .SetFontSize(11);
                    // richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                    //  .SetFontSize(11);
                    richTextInfo.AddNewLine();

                    worksheet.Range(2, 1, 2, 12).Merge();
                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    //worksheet.Cell(2, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //worksheet.Cell(2, 1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(2, 1, 2, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(2, 1, 2, 12).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).AdjustToContents();
                    
                    worksheet.Row(2).Height = 80;
                    worksheet.Row(3).Height = 20;
                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 4;
                    worksheet.Cell(currentRow, 1).Value = "Sr.No:";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Name of workman:";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Father's/Husband's name";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Designation/Nature of employment";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Act/Omission for which fine imposed";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Date of offence";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Whether workmen showed against fine";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Name of person in whose presence employer’s explanation was heard";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Wage period and wages payable";
                    worksheet.Range(currentRow, 9, currentRow, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "Amount of fine imposed";
                    worksheet.Range(currentRow, 10, currentRow, 10).Merge();
                    worksheet.Cell(currentRow, 11).Value = "Date on which fine realised";
                    worksheet.Range(currentRow, 11, currentRow, 11).Merge();
                    worksheet.Cell(currentRow, 12).Value = "Remarks";
                    worksheet.Range(currentRow, 12, currentRow, 12).Merge();
       
                    var headerRange = worksheet.Range(4, 1, currentRow, 12);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(4, 1, currentRow, 12);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 8).AdjustToContents();

                  //  worksheet.Column(10).Width = 15;
                    //worksheet.Column(11).Width = 15;
                    //worksheet.Column(7).Width = 30;
                    //worksheet.Column(8).Width = 30;


                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        //if (column.ColumnNumber() != 10)
                        //{
                        if (column.Width > 28) column.Width = 28;
                        //}
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region FORM IV (Register of Overtimes )
                if (FormId == 47)////FORM IV ( Register of Overtimes )
                {
                    var worksheet = workbook.Worksheets.Add("FORMIVReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 12).Merge();
                    worksheet.Range(1, 1, 1, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;

                    richTextInfo.AddText("Name and address of establishment : ")
                                .SetBold()
                                .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                            // .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("Nature and location of work:")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                     .SetFontSize(11);
                    richTextInfo.AddNewLine();
                  
                    worksheet.Range(2, 1, 2, 12).Merge();
                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    //worksheet.Cell(2, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //worksheet.Cell(2, 1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(2, 1, 2, 12).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(2, 1, 2, 12).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).AdjustToContents();

                    worksheet.Row(2).Height = 80;
                    worksheet.Row(3).Height = 20;
                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 4;
                    worksheet.Cell(currentRow, 1).Value = "Month/Year";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Sr.No";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Name of Worker";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Father’s / Husband’s Name";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Sex";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Designation / Nature of Employment";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Date of Overtime Worked";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "No. of Overtime Hours";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Overtime Rate of Wages";
                    worksheet.Range(currentRow, 9, currentRow, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "Overtime Earnings";
                    worksheet.Range(currentRow, 10, currentRow, 10).Merge();
                    worksheet.Cell(currentRow, 11).Value = "Date on which overtime wages paid";
                    worksheet.Range(currentRow, 11, currentRow, 11).Merge();
                    worksheet.Cell(currentRow, 12).Value = "Remarks";
                    worksheet.Range(currentRow, 12, currentRow, 12).Merge();

                    var headerRange = worksheet.Range(4, 1, currentRow, 12);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(4, 1, currentRow, 12);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 8).AdjustToContents();

                    //  worksheet.Column(10).Width = 15;
                    //worksheet.Column(11).Width = 15;
                    //worksheet.Column(7).Width = 30;
                    //worksheet.Column(8).Width = 30;


                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        //if (column.ColumnNumber() != 10)
                        //{
                        if (column.Width > 28) column.Width = 28;
                        //}
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region FORM XII (Register of Contractor )
                if (FormId == 48)////FORM XII ( Register of Contractor )
                {
                    var worksheet = workbook.Worksheets.Add("FORMXIIReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 7).Merge();
                    worksheet.Range(1, 1, 1, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;

                    richTextInfo.AddText("Name and address of establishment : ")
                                .SetBold()
                                .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                    // .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("Nature and location of work:")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                     .SetFontSize(11);
                    richTextInfo.AddNewLine();

                    worksheet.Range(2, 1, 2, 7).Merge();
                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    //worksheet.Cell(2, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //worksheet.Cell(2, 1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(2, 1, 2, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(2, 1, 2, 7).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).AdjustToContents();

                    worksheet.Row(2).Height = 60;
                    worksheet.Row(3).Height = 20;
                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 4;
                    worksheet.Cell(currentRow, 1).Value = "Sr.No";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Name and Address of Contractor";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Nature of Work";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Location of Work";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Period of Contract";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Maximum No. of Workers Employed";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Remarks";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                
                    var headerRange = worksheet.Range(4, 1, currentRow, 7);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(4, 1, currentRow, 7);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 8).AdjustToContents();

                    //  worksheet.Column(10).Width = 15;
                    //worksheet.Column(11).Width = 15;
                    //worksheet.Column(7).Width = 30;
                    //worksheet.Column(8).Width = 30;


                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        //if (column.ColumnNumber() != 10)
                        //{
                        if (column.Width > 28) column.Width = 28;
                        //}
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region FORM XIX (Wage Slip)
                if (FormId == 49)////FORM XIX (Wage Slip)
                {
                    var worksheet = workbook.Worksheets.Add("FORMXIXReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 7).Merge();
                    worksheet.Range(1, 1, 1, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;

                    richTextInfo.AddText("Name and address of contractor  : ")
                                .SetBold()
                                .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                    // .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("Nature and location of work:")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                     .SetFontSize(11);
                    richTextInfo.AddNewLine();

                    worksheet.Range(2, 1, 2, 7).Merge();
                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    //worksheet.Cell(2, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //worksheet.Cell(2, 1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(2, 1, 2, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(2, 1, 2, 7).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).AdjustToContents();

                    worksheet.Row(2).Height = 60;
                    worksheet.Row(3).Height = 20;
                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 4;
                    worksheet.Cell(currentRow, 1).Value = "Number of days worked ";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Number of units worked in case of piece rate workers";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Rate of daily wages/piece rate";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Amount of overtime wages";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Gross wages payable";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Deductions, If any ";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Net amount of wages paid";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();

                    var headerRange = worksheet.Range(4, 1, currentRow, 7);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(4, 1, currentRow, 7);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 8).AdjustToContents();

                    //  worksheet.Column(10).Width = 15;
                    //worksheet.Column(11).Width = 15;
                    //worksheet.Column(7).Width = 30;
                    //worksheet.Column(8).Width = 30;


                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        //if (column.ColumnNumber() != 10)
                        //{
                        if (column.Width > 28) column.Width = 28;
                        //}
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region FORM 11 (Accident Book)
                if (FormId == 50)////FORM 11 (Accident Book)
                {
                    var worksheet = workbook.Worksheets.Add("FORM11Report");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 18).Merge();
                    worksheet.Range(1, 1, 1, 18).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;

                    richTextInfo.AddText("Name and address of contractor  : ")
                                .SetBold()
                                .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                    // .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("Nature and location of work:")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                     .SetFontSize(11);
                    richTextInfo.AddNewLine();

                    worksheet.Range(2, 1, 2, 18).Merge();
                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    //worksheet.Cell(2, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //worksheet.Cell(2, 1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(2, 1, 2, 18).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(2, 1, 2, 18).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).AdjustToContents();

                    worksheet.Row(2).Height = 60;
                    worksheet.Row(3).Height = 20;
                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 4;
                    worksheet.Cell(currentRow, 1).Value = "Sr.No";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Date of Notice";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Time of Notice";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Name and Address of injured person";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Sex";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Age";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Insurance No";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Shift, Department and occupation of the employee";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Cause";
                    worksheet.Range(currentRow, 9, currentRow, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "Nature";
                    worksheet.Range(currentRow, 10, currentRow, 10).Merge();
                    worksheet.Cell(currentRow, 11).Value = "Date";
                    worksheet.Range(currentRow, 11, currentRow, 11).Merge();
                    worksheet.Cell(currentRow, 12).Value = "Time";
                    worksheet.Range(currentRow, 12, currentRow, 12).Merge();
                    worksheet.Cell(currentRow, 13).Value = "Place";
                    worksheet.Range(currentRow, 13, currentRow, 13).Merge();
                    worksheet.Cell(currentRow, 14).Value = "What exactly was the injured person doing at the time of accident";
                    worksheet.Range(currentRow, 14, currentRow, 14).Merge();
                    worksheet.Cell(currentRow, 15).Value = "Name, Occupation, address and signature or the thumb impression of the person(s) giving notice";
                    worksheet.Range(currentRow, 15, currentRow, 15).Merge();
                    worksheet.Cell(currentRow, 16).Value = "Signature and designation of the person who makes the entry in the Accident Book";
                    worksheet.Range(currentRow, 16, currentRow, 16).Merge();
                    worksheet.Cell(currentRow, 17).Value = "Name, address and occupation of two witnesses";
                    worksheet.Range(currentRow, 17, currentRow, 17).Merge();
                    worksheet.Cell(currentRow, 18).Value = "Remarks";
                    worksheet.Range(currentRow, 18, currentRow, 18).Merge();

                    var headerRange = worksheet.Range(4, 1, currentRow, 18);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(4, 1, currentRow, 18);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 8).AdjustToContents();

                    worksheet.Column(14).Width = 20;
                    worksheet.Column(15).Width = 20;
                    worksheet.Column(16).Width = 20;
                    worksheet.Column(17).Width = 20;


                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.ColumnNumber() != 14 && column.ColumnNumber() != 15 && column.ColumnNumber() != 16 && column.ColumnNumber() != 17)
                        {
                            if (column.Width > 58) column.Width = 58;
                        }
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region FORM 18 (Register of Leave with Wages)
                if (FormId == 51)////FORM 18 (Register of Leave with Wages)
                {
                    var worksheet = workbook.Worksheets.Add("FORM18Report");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 18).Merge();
                    worksheet.Range(1, 1, 1, 18).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;

                    richTextInfo.AddText("Name and address of contractor  : ")
                                .SetBold()
                                .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                    // .SetFontSize(11);
                    richTextInfo.AddNewLine();
                    richTextInfo.AddText("Nature and location of work:")
                               .SetBold()
                               .SetFontSize(11);
                    richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                     .SetFontSize(11);
                    richTextInfo.AddNewLine();

                    worksheet.Range(2, 1, 2, 18).Merge();
                    worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    //worksheet.Cell(2, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //worksheet.Cell(2, 1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(2, 1, 2, 18).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(2, 1, 2, 18).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(2).AdjustToContents();

                    worksheet.Row(2).Height = 60;
                    worksheet.Row(3).Height = 20;
                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 4;
                    worksheet.Cell(currentRow, 1).Value = "Sr.No";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Date of Notice";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Time of Notice";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Name and Address of injured person";
                    worksheet.Range(currentRow, 4, currentRow, 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Sex";
                    worksheet.Range(currentRow, 5, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 6).Value = "Age";
                    worksheet.Range(currentRow, 6, currentRow, 6).Merge();
                    worksheet.Cell(currentRow, 7).Value = "Insurance No";
                    worksheet.Range(currentRow, 7, currentRow, 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Shift, Department and occupation of the employee";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();
                    worksheet.Cell(currentRow, 9).Value = "Cause";
                    worksheet.Range(currentRow, 9, currentRow, 9).Merge();
                    worksheet.Cell(currentRow, 10).Value = "Nature";
                    worksheet.Range(currentRow, 10, currentRow, 10).Merge();
                    worksheet.Cell(currentRow, 11).Value = "Date";
                    worksheet.Range(currentRow, 11, currentRow, 11).Merge();
                    worksheet.Cell(currentRow, 12).Value = "Time";
                    worksheet.Range(currentRow, 12, currentRow, 12).Merge();
                    worksheet.Cell(currentRow, 13).Value = "Place";
                    worksheet.Range(currentRow, 13, currentRow, 13).Merge();
                    worksheet.Cell(currentRow, 14).Value = "What exactly was the injured person doing at the time of accident";
                    worksheet.Range(currentRow, 14, currentRow, 14).Merge();
                    worksheet.Cell(currentRow, 15).Value = "Name, Occupation, address and signature or the thumb impression of the person(s) giving notice";
                    worksheet.Range(currentRow, 15, currentRow, 15).Merge();
                    worksheet.Cell(currentRow, 16).Value = "Signature and designation of the person who makes the entry in the Accident Book";
                    worksheet.Range(currentRow, 16, currentRow, 16).Merge();
                    worksheet.Cell(currentRow, 17).Value = "Name, address and occupation of two witnesses";
                    worksheet.Range(currentRow, 17, currentRow, 17).Merge();
                    worksheet.Cell(currentRow, 18).Value = "Remarks";
                    worksheet.Range(currentRow, 18, currentRow, 18).Merge();

                    var headerRange = worksheet.Range(4, 1, currentRow, 18);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(4, 1, currentRow, 18);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 8).AdjustToContents();

                    worksheet.Column(14).Width = 20;
                    worksheet.Column(15).Width = 20;
                    worksheet.Column(16).Width = 20;
                    worksheet.Column(17).Width = 20;


                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        if (column.ColumnNumber() != 14 && column.ColumnNumber() != 15 && column.ColumnNumber() != 16 && column.ColumnNumber() != 17)
                        {
                            if (column.Width > 58) column.Width = 58;
                        }
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #region FORM 7 (Record of White Washing)
                if (FormId == 52)////FORM 7 ( Record of White Washing )
                {
                    var worksheet = workbook.Worksheets.Add("FORM7Report");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);

                    worksheet.Range(1, 1, 1, 7).Merge();
                    worksheet.Range(1, 1, 1, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                   // worksheet.Cell(2, 1).Clear();
                  //  var richTextInfo = worksheet.Cell(2, 1).RichText;

                    //richTextInfo.AddText("Name and address of contractor : ")
                    //            .SetBold()
                    //            .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                    //         .SetFontSize(11);
                    //richTextInfo.AddNewLine();
                    //richTextInfo.AddText("Nature and location of work:")
                    //           .SetBold()
                    //           .SetFontSize(11);
                    //richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                    // .SetFontSize(11);
                    //richTextInfo.AddNewLine();
                    //richTextInfo.AddText("Name and address of establishment :")
                    //         .SetBold()
                    //         .SetFontSize(11);
                    //// richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                    ////  .SetFontSize(11);
                    //richTextInfo.AddNewLine();

                    //worksheet.Range(2, 1, 2, 7).Merge();
                    //worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                    //worksheet.Cell(2, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    //worksheet.Cell(2, 1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    //worksheet.Range(2, 1, 2, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                   // worksheet.Range(2, 1, 2,7).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                   // worksheet.Row(2).AdjustToContents();


                 //   worksheet.Row(2).Height = 10;
                  //  worksheet.Row(3).Height = 20;
                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 2;
                    worksheet.Cell(currentRow, 1).Value = "Part of Factory(Name of Room)";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Parts Limewashed ,Painted varnished or oiled ,e.g.walls,ceilings,wood works etc.";
                    worksheet.Range(currentRow, 2, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Treatments whether Limewashed,Painted varnished or oiled";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Date on which lime-Washing,Painted varnishing or oiling was carried out";
                    worksheet.Range(currentRow, 4, currentRow, 7).Merge();
                
                    //Subheader

                    currentRow++;
                    worksheet.Cell(currentRow, 4).Value = "Date";
                    worksheet.Cell(currentRow, 5).Value = "Month";
                    worksheet.Cell(currentRow, 6).Value = "Year";
                    worksheet.Cell(currentRow, 7).Value = "Remark";


                    var headerRange = worksheet.Range(2, 1, currentRow, 7);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Fill first table data
                    currentRow++;
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                        }
                    }
                    currentRow += dt.Rows.Count - 1;
                    var fullTableRange = worksheet.Range(2, 1, currentRow, 7);
                    fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns(1, 8).AdjustToContents();

                    worksheet.Column(10).Width = 15;
                    worksheet.Column(11).Width = 15;
               

                    foreach (var column in worksheet.ColumnsUsed())
                    {
                        //if (column.ColumnNumber() != 10)
                        //{
                        if (column.Width > 28) column.Width = 28;
                        //}
                    }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);
                }
                #endregion

                #endregion

                #region Form IX
                if (FormId == 3)////Form IX
                {
                    return File(new byte[0], "application/octet-stream", "NotAllowed.txt");
                }
                #endregion


                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "Report.xlsx");
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
        public ActionResult DownloadExcelFile1(int? CmpId, int? BranchId, DateTime Month, DateTime ToMonth, int FormId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var FormRegister = @"
            SELECT Form AS Name, Payroll_Act.Register AS FormRegister, Payroll_Act.Act As [Act],
            Mas_Branch.BranchName AS [BranchName],Mas_Branch.BranchAddress As [BranchAddress]
            FROM Payroll_Act 
            INNER JOIN Mas_Branch ON Mas_Branch.StateId = Payroll_Act.PayrollAct_StateId
            LEFT JOIN Mas_Employee ON Mas_Branch.BranchId = Mas_Employee.EmployeeBranchId
            LEFT JOIN Mas_CompanyProfile ON Mas_CompanyProfile.CompanyId = Mas_Employee.CmpID
            WHERE Mas_Employee.CmpID = '" + CmpId + @"'
            AND Mas_Employee.EmployeeBranchId = '" + BranchId + @"'
            AND Payroll_Act.PayrollActId = '" + FormId + @"'
            GROUP BY Payroll_Act.PayrollActId, Payroll_Act.Register, Mas_Branch.BranchName,Mas_Branch.BranchAddress,Payroll_Act.Act, Form";

                    var Form = DapperORM.DynamicQuerySingle(FormRegister);

                    if (Form != null)
                    {
                        Session["FormAct"] = Form.Act;
                        Session["FormRegister"] = Form.FormRegister;
                        Session["FormName"] = Form.Name;
                        Session["BranchName"] = Form.BranchName;
                        Session["BranchAddress"] = Form.BranchAddress;
                    }
                }

                var workbook = new XLWorkbook();
                //#region FORM XIX (Wage Slip)
                //if (FormId == 44)////FORM XIX ( Wage Slip)
                //{
                var worksheet = workbook.Worksheets.Add("FORMXIXReport");
                    var richText = worksheet.Cell(1, 1).RichText;
                    richText.AddText((Session["FormName"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(13);
                    richText.AddNewLine();
                    richText.AddText((Session["FormAct"] ?? "").ToString().Trim())
                            .SetBold(false)
                            .SetFontSize(10);
                    richText.AddNewLine();
                    richText.AddText((Session["FormRegister"] ?? "").ToString().Trim())
                            .SetBold()
                            .SetFontSize(12);
                  
                    worksheet.Range(1, 1, 1, 8).Merge();
                    worksheet.Range(1, 1, 1, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(1, 1).Style.Alignment.WrapText = true;
                    worksheet.Row(1).Height = 80;

                    worksheet.Cell(2, 1).Clear();
                    var richTextInfo = worksheet.Cell(2, 1).RichText;
         
                richTextInfo.AddText("Name and address of contractor : ")
                            .SetBold()
                            .SetFontSize(11);
                richTextInfo.AddText((Session["BranchName"] ?? "").ToString().Trim())
                         .SetFontSize(11);
                richTextInfo.AddNewLine();
                richTextInfo.AddText("Name and father's/husband's name of the workman :")
                           .SetBold()
                           .SetFontSize(11);
                //richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                        // .SetFontSize(11);
                richTextInfo.AddNewLine();
                richTextInfo.AddText("Nature and location of work:")
                         .SetBold()
                         .SetFontSize(11);
                richTextInfo.AddText((Session["BranchAddress"] ?? "").ToString().Trim())
                         .SetFontSize(11);
                richTextInfo.AddNewLine();

                worksheet.Range(2, 1, 2, 8).Merge();
                worksheet.Cell(2, 1).Style.Alignment.WrapText = true;
                //worksheet.Cell(2, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                //worksheet.Cell(2, 1).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range(2, 1, 2, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range(2, 1, 2, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                worksheet.Row(2).AdjustToContents();
            
               
                     worksheet.Row(2).Height = 80;
                    worksheet.Row(3).Height = 20;
                    DataTable dt = new DataTable();
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", Session["EmployeeId"]);
                    paramList.Add("@p_FromMonthYear", Month);
                    paramList.Add("@p_ToMonthYear", ToMonth);
                    paramList.Add("@p_CompanyId", CmpId);
                    paramList.Add("@p_BranchId", BranchId);
                    paramList.Add("@p_FormId", FormId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Payroll_Act_RegisterOfOvertime", paramList).ToList();
                    if (GetData.Count == 0)
                    {
                        return File(new byte[0], "application/octet-stream", "FileNotFound.txt");
                    }
                    DapperORM dprObj = new DapperORM();
                    dt = dprObj.ConvertToDataTable(GetData);

                    // ---------- FIRST TABLE ----------
                    int currentRow = 4;
                    worksheet.Cell(currentRow, 1).Value = "Number of days worked ";
                    worksheet.Range(currentRow, 1, currentRow, 1).Merge();
                    worksheet.Cell(currentRow, 2).Value = "Number of units worked in case of piece rate workers";
                    worksheet.Range(currentRow, 2, currentRow , 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = "Rate of daily wages/piece rate ";
                    worksheet.Range(currentRow, 3, currentRow, 3).Merge();
                    worksheet.Cell(currentRow, 4).Value = "Amount of overtime wages ";
                    worksheet.Range(currentRow, 4, currentRow , 4).Merge();
                    worksheet.Cell(currentRow, 5).Value = "Gross wages payable ";
                    worksheet.Range(currentRow , 5, currentRow , 6).Merge();
                    worksheet.Cell(currentRow , 7).Value = "Deductions, If any  ";
                    worksheet.Range(currentRow, 7, currentRow , 7).Merge();
                    worksheet.Cell(currentRow, 8).Value = "Net amount of wages paid";
                    worksheet.Range(currentRow, 8, currentRow, 8).Merge();
                

                var headerRange = worksheet.Range(4, 1, currentRow, 8);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Fill first table data
                currentRow++;
                for (int r = 0; r < dt.Rows.Count; r++)
                {
                    for (int c = 0; c < dt.Columns.Count; c++)
                    {
                        worksheet.Cell(currentRow + r, 1 + c).Value = dt.Rows[r][c];
                    }
                }
                currentRow += dt.Rows.Count - 1;
                var fullTableRange = worksheet.Range(4, 1, currentRow, 8);
                fullTableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                fullTableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                worksheet.Columns(1, 8).AdjustToContents();

                    worksheet.Column(5).Width = 15;
                worksheet.Column(6).Width = 15;
                //worksheet.Column(7).Width = 30;
                //worksheet.Column(8).Width = 30;


                foreach (var column in worksheet.ColumnsUsed())
                    {
                    if (column.ColumnNumber() != 10)
                    {
                        if (column.Width > 28) column.Width = 28;
                    }
                }

                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.FitToPages(1, 0);

     

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "Report.xlsx");
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


