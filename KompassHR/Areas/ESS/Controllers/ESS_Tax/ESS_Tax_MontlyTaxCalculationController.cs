using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Module.Models.Module_TaxDeclaration;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Tax
{
    public class ESS_Tax_MontlyTaxCalculationController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: ESS/ESS_Tax_MontlyTaxCalculation
        #region Main View
        public ActionResult ESS_Tax_MontlyTaxCalculation()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 547;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Session["MonthYear"] = null;
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetCompanyName = GetComapnyName;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(int? CmpId, DateTime Month)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                //string Tax_MonthYear = MonthYear.ToString();
                param = new DynamicParameters();
                param.Add("@p_process", "IsValidation");

                param.Add("@p_EmployeeID", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                param.Add("@p_Tax_MonthYear", Month);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
             //   param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Valid", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteSP<dynamic>("sp_Tax_TaxCalculation", param);
                var Message = param.Get<string>("@p_msg");
                var Valid = param.Get<string>("@p_Valid"); 
                
                var Icon = param.Get<string>("@p_Icon");



                if (Icon == "error" || Message != "")
                {
                    return Json(new { Message, Icon, Valid, Result }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_EmployeeID", Session["EmployeeId"]);
                    param.Add("@p_CmpId", CmpId);
                    param.Add("@p_Tax_MonthYear", Month);
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    // param.Add("@p_Valid", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result1 = DapperORM.ExecuteReturn("sp_Tax_TaxCalculation", param);
                    var Message1 = param.Get<string>("@p_msg");
                    var Icon1 = param.Get<string>("@p_Icon");

                    return Json(new { Message1, Icon1 }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
        [HttpGet]
        public ActionResult GetList(DateTime? MonthlyTaxMonthYear)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 547;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters paramBackDay = new DynamicParameters();
                paramBackDay.Add("@query", "Select TaxFyearId as Id,TaxYear As Name  from IncomeTax_Fyear where Deactivate=0");
                var GetBackDatedDay = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", paramBackDay).ToList();
                ViewBag.GetTaxFyear = GetBackDatedDay;

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_MonthYear", MonthlyTaxMonthYear);               
             
                var data = DapperORM.ExecuteSP<dynamic>("sp_Tax_List_TaxCalculation", param).ToList();
                ViewBag.MonthlyTaxCalculation = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetTaxDetails
        public ActionResult GetTaxDetails(int? TaxCal_FyearID, DateTime TaxCal_MonthYear,int TaxCal_CmpID)
        {
            try
            {

                var SetDate = TaxCal_MonthYear.ToString("yyyy-MM-dd");
                DynamicParameters paramTaxCal = new DynamicParameters();
                paramTaxCal.Add("@p_CmpID", TaxCal_CmpID);
                paramTaxCal.Add("@p_FYearID", TaxCal_FyearID);
                paramTaxCal.Add("@p_MonthYear", SetDate);
                var data = DapperORM.ExecuteSP<dynamic>("sp_Tax_List_TaxCalculation_Show", paramTaxCal).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


        #endregion GetTaxDetails

        #region DeleteMontlyAttendance
        [HttpPost]
        public ActionResult DeleteMonthlyTaxDetails(List<DeleteMonthlyTaxCalculation> ObjDelete, DateTime MonthlyTaxMonthYear)
        {
            try
            {
               // Session["Fyear"] = TaxFyearId;
                StringBuilder strBuilder = new StringBuilder();
                if (ObjDelete != null)
                {
                    foreach (var Data in ObjDelete)
                    {
                        string StringMonthlyAttendance = " Update IncomeTax_TaxCalculation set Deactivate=1,ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=GETDATE() where  TaxCal_Id=" + Data.TaxCal_Id + "           ";
                        strBuilder.Append(StringMonthlyAttendance);

                        string StringMonthlyAttendance1 = " Update IncomeTax_MonthlyTax set Deactivate=1,ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=GETDATE() where  MonthlyTaxEmployeeId=" + Data.TaxCal_EmployeeID + " and MonthlyTaxMonthYear='"+ MonthlyTaxMonthYear + "'   AND MonthlyTaxRemark = 'Salary TDS'       ";
                        strBuilder.Append(StringMonthlyAttendance1);
                    }
                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record delete successfully";
                        TempData["Icon"] = "success";                       
                    }
                    if (abc != "")
                    {
                        DapperORM.DynamicQuerySingle("Insert Into Tool_ErrorLog ( " +
                                                                            "   Error_Desc " +
                                                                            " , Error_FormName " +
                                                                            " , Error_MachinceName " +
                                                                            " , Error_Date " +
                                                                            " , Error_UserID " +
                                                                            " , Error_UserName " + ") values (" +
                                                                            "'" + strBuilder + "'," +
                                                                            "'BuklInsert'," +
                                                                            "'" + Dns.GetHostName().ToString() + "'," +
                                                                            "GetDate()," +
                                                                            "'" + Session["EmployeeId"] + "'," +
                                                                            "'" + Session["EmployeeName"] + "'");
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"], Month = TempData["Month"] }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(DateTime MonthYear, int FyearID,int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("MonthlyTaxCalculation");
                worksheet.Range(1, 1, 1, 23).Merge();  //Merge First Row with 60 Clomn
                worksheet.SheetView.FreezeRows(2); // Freeze the row
                DataTable dt = new DataTable();
                List<dynamic> data = new List<dynamic>();

                param.Add("@p_CmpID", CmpId);
                param.Add("@P_FYearID", FyearID);
                param.Add("@P_MonthYear", MonthYear);
                            
                data = DapperORM.ExecuteSP<dynamic>("sp_Tax_List_TaxCalculation_Export", param).ToList();
                if (data.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }

                DapperORM dprObj = new DapperORM();
                dt = dprObj.ConvertToDataTable(data);
                worksheet.Cell(2, 1).InsertTable(dt, false);
                int totalRows = worksheet.RowsUsed().Count();

                var lastRow = worksheet.Row(totalRows + 1);
                lastRow.Style.Font.Bold = true;
                // Additional styling if required
                lastRow.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                lastRow.Style.Font.FontColor = XLColor.Black;
                lastRow.Style.Font.FontSize = 10;

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
               var SetMonth = (MonthYear).ToString("MMM-yyyy");
                worksheet.Cell(1, 1).Value = "Monthly Tax Calculation Report  - (" + SetMonth + ")";
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
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
    }
}