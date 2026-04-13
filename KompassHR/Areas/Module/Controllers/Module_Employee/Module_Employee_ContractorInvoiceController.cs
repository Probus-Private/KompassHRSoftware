using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_ContractorInvoiceController : Controller
    {
        
        #region Main View 
        public ActionResult Module_Employee_ContractorInvoice(int? CmpId, int? BranchId, int? ContractorId, DateTime? InvoiceMonth)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 926;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                // COMPANY
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.CompanyName = GetComapnyName;


                if (CmpId != null)
                {
                    DynamicParameters paramB = new DynamicParameters();
                    paramB.Add("@p_employeeid", Session["EmployeeId"]);
                    paramB.Add("@p_CmpId", CmpId);
                    ViewBag.BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramB).ToList();
                }
                else
                {
                    ViewBag.BranchName = "";
                }

  
                if (CmpId != null && BranchId != null)
                {
                    DynamicParameters Contractparam = new DynamicParameters();
                    Contractparam.Add("@Query",
                        "select DISTINCT Contractor_Master.ContractorId as Id, Contractor_Master.ContractorName As Name " +
                        "from Contractor_Master " +
                        "inner join Mas_ContractorMapping on Mas_ContractorMapping.ContractorID = Contractor_Master.ContractorId " +
                        "where Contractor_Master.Deactivate = 0 and Mas_ContractorMapping.CmpID = '" + CmpId +
                        "' and Mas_ContractorMapping.BranchId = '" + BranchId + "'");
                    ViewBag.ContractorName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", Contractparam).ToList();
                }
                else
                {
                    ViewBag.ContractorName = "";
                }


                if (InvoiceMonth != null && CmpId != null && BranchId != null)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_MonthYear", InvoiceMonth);
                    param1.Add("@p_CmpId", CmpId);
                    param1.Add("@p_BranchId", BranchId);
                    param1.Add("@p_ContractorId", ContractorId);

                    var data = DapperORM.ExecuteSP<dynamic>("SP_Invoice_Conctrator_Invoice_Show", param1).ToList();

                    if (data.Count > 0)
                    {
                        int hideCount = 0;
                        var hideValue = ((IDictionary<string, object>)data[0])["Hide"];
                        int.TryParse(hideValue?.ToString(), out hideCount);

                        ViewBag.HideCount = hideCount;
                        ViewBag.InvoiceData = data;
                    }
                    else
                    {
                        ViewBag.InvoiceData = new List<dynamic>();
                        TempData["Message"] = "Record Not Found";
                        TempData["Icon"] = "error";
                        return View();
                    }
                }
                else
                {
                    ViewBag.InvoiceData = new List<dynamic>();
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
        
        #region GetContractorName
        [HttpGet]
        public ActionResult GetContractorName(int CmpId,int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                //DynamicParameters param = new DynamicParameters();
                //param.Add("@Query", "select  DISTINCT Contractor_Master.ContractorId as Id,Contractor_Master.ContractorName As Name from Contractor_Master Inner Join Mas_ContractorMapping on  Mas_ContractorMapping.ContractorID = Contractor_Master.ContractorId where Contractor_Master.Deactivate = 0 and Mas_ContractorMapping.CmpID = '" + CmpId + "' and Mas_ContractorMapping.BranchId = '" + BranchId + "'");
                //var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                //return Json(data, JsonRequestBehavior.AllowGet);

                DynamicParameters paramContractorName = new DynamicParameters();
                paramContractorName.Add("@p_qry", " and Mas_ContractorMapping.BranchID=" + BranchId + "");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetContractorDropdown", paramContractorName).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);

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
        public ActionResult SaveUpdate(Contractor_Invoice model)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                string Message = "";
                string Icon = "";

                foreach (var employee in model.ContractorInvoice)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_process", string.IsNullOrEmpty(employee.ContractorInvoiceId_Encrypted) ? "Save" : "Update");
                    param.Add("@p_ContractorInvoiceId_Encrypted", employee.ContractorInvoiceId_Encrypted);
                    //param.Add("@p_CmpId", CmpId);
                    //param.Add("@p_BranchId", BranchId);
                    //param.Add("@p_ContractorId", ContractorId);

                    param.Add("@p_CmpId", model.CmpId);
                    param.Add("@p_BranchId", model.BranchId);
                 //   param.Add("@p_ContractorId", model.ContractorId);

                    param.Add("@p_ContractorMonthlyContractorId", employee.ContractorMonthlyContractorId);
                    param.Add("@p_ContractorMonthlyContractorName", employee.Contractor);
                    param.Add("@p_ContractorMonthlyMonthYear", employee.ContractorMonthlyMonthYear);
                    param.Add("@p_ContractorNoOfEmployee", employee.NoOfEmployee);
                    param.Add("@p_NoOfDays", employee.NoOfDays);
                    param.Add("@p_Total_PerDayAmount", employee.Total_PerDayAmount);
                    param.Add("@p_Total_CanteenAmount", employee.Total_CanteenAmount);
                    param.Add("@p_Total_AttendanceBonusAmount", employee.Total_AttendanceBonusAmount);
                    param.Add("@p_Total_TransportAllowAmount", employee.Total_TransportAllowAmount);
                    param.Add("@p_Total_OtherRate1Amount", employee.Total_OtherRate1Amount);
                    param.Add("@p_Total_OtherRate2Amount", employee.Total_OtherRate2Amount);
                    param.Add("@p_TotalAmount", employee.TotalAmount);
                    param.Add("@p_ServiceCharges", employee.ServiceCharges);
                    param.Add("@p_Rate", employee.Rate);
                    param.Add("@p_Percentage", employee.Percentage);
                    param.Add("@p_IsGross", employee.IsGross);
                    param.Add("@p_IsOT", employee.IsOT);
                    param.Add("@p_SupervisorCharges", employee.SupervisorCharges);
                    param.Add("@p_Total_ServiceCharges", employee.Total_ServiceCharges);
                    param.Add("@p_Total_SupervisorCharges", employee.Total_SupervisorCharges);
                    param.Add("@p_Total_InvoiceAmout", employee.Total_InvoiceAmout);
                    
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var data = DapperORM.ExecuteReturn("sp_SUD_Contractor_Invoice1", param);
                    Message = param.Get<string>("@p_msg");
                    Icon = param.Get<string>("@p_Icon");

                }
                return Json(new { success = true, message = Message, icon = Icon });
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
        public ActionResult GetList(int? CmpId, int? BranchId, DateTime? InvoiceMonth)
        {
            try 
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 926;
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

                var CmpID = GetComapnyName[0].Id;
                ViewBag.BranchName = "";
                ViewBag.ContractorName = "";


                if (InvoiceMonth != null && CmpId != null && BranchId != null)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_MonthYear", InvoiceMonth);
                    param1.Add("@p_CmpId", CmpId);
                    param1.Add("@p_BranchId", BranchId);
                   // param1.Add("@p_ContractorId", ContractorId);
                    var data = DapperORM.ExecuteSP<dynamic>("sp_List_Contractor_InvoiceList", param1).ToList();

                    //int hideCount = 0;
                    //if (data.Count > 0)
                    //{
                    //    var hideValue = ((IDictionary<string, object>)data[0])["Hide"];

                    //    if (hideValue != null && hideValue is int)
                    //    {
                    //        hideCount = (int)hideValue;
                    //    }
                    //    else if (hideValue != null)
                    //    {
                    //        int.TryParse(hideValue.ToString(), out hideCount);
                    //    }
                    //}
                    //ViewBag.HideCount = hideCount;

                    if (data == null || data.Count == 0)
                    {
                        TempData["Message"] = "Record Not Found";
                        TempData["Icon"] = "error";
                        //  return View();
                    }

                    ViewBag.InvoiceListData = data;
                    
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_employeeid", Session["EmployeeId"]);
                    param.Add("@p_CmpId", CmpId);
                    ViewBag.BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                    DynamicParameters Contractparam = new DynamicParameters();
                    Contractparam.Add("@Query", "select  DISTINCT Contractor_Master.ContractorId as Id,Contractor_Master.ContractorName As Name from Contractor_Master Inner Join Mas_ContractorMapping on  Mas_ContractorMapping.ContractorID = Contractor_Master.ContractorId where Contractor_Master.Deactivate = 0 and Mas_ContractorMapping.CmpID = '" + CmpId + "' and Mas_ContractorMapping.BranchId = '" + BranchId + "'");
                    ViewBag.ContractorName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", Contractparam).ToList();

                }
                else
                {
                    ViewBag.InvoiceListData = new List<dynamic>();
                 
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
        
        #region Delete
        [HttpGet]
        public ActionResult Delete(int? ContractorInvoiceId)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Delete");
                param.Add("@p_ContractorInvoiceId", ContractorInvoiceId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_Delete_Contractor_Invoice", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Module_Employee_ContractorInvoice");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region DownloadExcelFile
        public ActionResult DownloadExcelFile(int? CmpId, int? BranchId, int? ContractorId, DateTime InvoiceMonth)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("ContractorInvoice");

                // -------------------------
                // ✅ FETCH COMPANY NAME
                // -------------------------
                DynamicParameters paramComp = new DynamicParameters();
                paramComp.Add("@Query", "SELECT CompanyName FROM Mas_CompanyProfile WHERE CompanyId = '" + CmpId + "'");
                var compResult = DapperORM.ExecuteSP<dynamic>("Sp_QueryExcution", paramComp).FirstOrDefault();
                string companyName = compResult?.CompanyName ?? "";

                // -------------------------
                // ✅ FETCH BRANCH NAME
                // -------------------------
                DynamicParameters paramBranch = new DynamicParameters();
                paramBranch.Add("@Query", "SELECT BranchName FROM Mas_Branch WHERE BranchId = '" + BranchId + "'");
                var branchResult = DapperORM.ExecuteSP<dynamic>("Sp_QueryExcution", paramBranch).FirstOrDefault();
                string branchName = branchResult?.BranchName ?? "";

                // -------------------------
                // ✅ GET DATA
                // -------------------------

                //worksheet.Range(1, 1, 1, 10).Merge();
                //worksheet.SheetView.FreezeRows(2);
                //DataTable dt = new DataTable();
                //List<dynamic> data = new List<dynamic>();

                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_ContractorId", ContractorId);
                paramList.Add("@p_MonthYear", InvoiceMonth);
                paramList.Add("@p_CmpId", CmpId);
                paramList.Add("@p_BranchId", BranchId);
                var GetData = DapperORM.ExecuteSP<dynamic>("SP_Invoice_Conctrator_Invoice_Show", paramList).ToList();
                if (GetData.Count == 0)
                {
                    byte[] emptyFileContents = new byte[0];
                    return File(emptyFileContents, "application/octet-stream", "FileNotFound.txt");
                }


                DapperORM dprObj = new DapperORM();
                DataTable dt = dprObj.ConvertToDataTable(GetData);

                int totalColumns = dt.Columns.Count;

                // -------------------------
                // ✅ HEADER ROW 1
                // -------------------------

                worksheet.Cell(1, 1).Value = companyName + " - " + branchName;
                worksheet.Range(1, 1, 1, totalColumns).Merge();
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                // -------------------------
                // ✅ HEADER ROW 2
                // -------------------------
                worksheet.Cell(2, 1).Value = "Contractor Invoice - (" + InvoiceMonth.ToString("MMM/yyyy") + ")";
                worksheet.Range(2, 1, 2, totalColumns).Merge();
                worksheet.Cell(2, 1).Style.Font.Bold = true;
                worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                // -------------------------
                // ❄ FREEZE FIRST 2 ROWS
                // -------------------------
                worksheet.SheetView.FreezeRows(2);

                // -------------------------
                // ✅ INSERT TABLE FROM ROW 3
                // -------------------------
                worksheet.Cell(3, 1).InsertTable(dt, false);
               
                worksheet.Column(1).Hide();
                worksheet.Column(2).Hide();


                // HEADER (ROW 3)
                // -------------------------
                var headerRange = worksheet.Range(3, 1, 3, totalColumns);
                headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontColor = XLColor.Black;
                headerRange.Style.Font.FontSize = 10;

                // -------------------------
                // Apply standard formatting
                // -------------------------
                var usedRange = worksheet.RangeUsed();
                usedRange.Style.Font.FontSize = 10;
                usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                worksheet.Columns().AdjustToContents();

                //int totalRows = worksheet.RowsUsed().Count();

                //var lastRow = worksheet.Row(totalRows + 1);
                //lastRow.Style.Font.Bold = false;

                //lastRow.Style.Font.FontColor = XLColor.Black;
                //lastRow.Style.Font.FontSize = 10;
                //var usedRange = worksheet.RangeUsed();
                //usedRange.Style.Fill.BackgroundColor = XLColor.White;
                //usedRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                //usedRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                //usedRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                //usedRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                //usedRange.Style.Font.FontSize = 10;
                //usedRange.Style.Font.FontColor = XLColor.Black;
                //worksheet.Cell(1, 1).Value = "Contractor Invoice";

                //worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                //worksheet.Cell(1, 1).Style.Font.Bold = true;
                //worksheet.Columns().AdjustToContents();
                //var headerRange = worksheet.Range(2, 1, 2, dt.Columns.Count);
                //headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(205, 222, 172);
                //headerRange.Style.Font.FontSize = 10;
                //headerRange.Style.Font.FontColor = XLColor.FromArgb(1, 0, 0);
                //headerRange.Style.Font.Bold = true;

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